/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2025 Dominik Reichl <dominik.reichl@t-online.de>

  This program is free software; you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation; either version 2 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, write to the Free Software
  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;

using KeePass.Resources;
using KeePass.UI;

using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Cryptography;
using KeePassLib.Cryptography.Cipher;
using KeePassLib.Cryptography.KeyDerivation;
using KeePassLib.Interfaces;
using KeePassLib.Resources;
using KeePassLib.Security;
using KeePassLib.Serialization;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	internal sealed partial class KeePassKdb1 : FileFormatProvider
	{
		public override bool Export(PwExportInfo pwExportInfo, Stream sOutput,
			IStatusLogger slLogger)
		{
			PwDatabase pd = (pwExportInfo.ContextDatabase ?? new PwDatabase());
			PwGroup pg = pwExportInfo.DataGroup;

			if(slLogger != null)
			{
				if(!string.IsNullOrEmpty(pd.Name))
					slLogger.SetText(KdbStatusPrefix + KPRes.FormatNoDatabaseName,
						LogStatusType.Warning);
				if(!string.IsNullOrEmpty(pd.Description))
					slLogger.SetText(KdbStatusPrefix + KPRes.FormatNoDatabaseDesc,
						LogStatusType.Warning);
			}

			byte[] pbMasterSalt = CryptoRandom.Instance.GetRandomBytes(16);
			byte[] pbIV = CryptoRandom.Instance.GetRandomBytes(16);
			byte[] pbKdfSeed = CryptoRandom.Instance.GetRandomBytes(32);

			ulong cKdfRounds64 = PwDefs.DefaultKeyEncryptionRounds;
			AesKdf kdf = new AesKdf();
			if(pd.KdfParameters.KdfUuid.Equals(kdf.Uuid))
				cKdfRounds64 = pd.KdfParameters.GetUInt64(AesKdf.ParamRounds,
					cKdfRounds64);
			uint cKdfRounds = (uint)Math.Min(cKdfRounds64, 0xFFFFFFFEUL);

			KdfParameters p = kdf.GetDefaultParameters();
			p.SetByteArray(AesKdf.ParamSeed, pbKdfSeed);
			p.SetUInt64(AesKdf.ParamRounds, cKdfRounds);

			Dictionary<string, byte[]> dMetaStreams = new Dictionary<string, byte[]>();
			if(!string.IsNullOrEmpty(pd.DefaultUserName))
				dMetaStreams[KdbMetaDefaultUserName] = StrUtil.GetBytesSZ(
					pd.DefaultUserName, StrUtil.Utf8);
			if(!UIUtil.ColorsEqual(pd.Color, Color.Empty))
				dMetaStreams[KdbMetaDatabaseColor] = new byte[4] {
					pd.Color.R, pd.Color.G, pd.Color.B, 0 };

			uint cGroups, cEntries;
			pg.GetCounts(true, out cGroups, out cEntries);
			++cGroups; // Root group
			cEntries += (uint)dMetaStreams.Count;

			KdbHeaderFlags hf = KdbHeaderFlags.Sha256;

			bool bTwofish = pd.DataCipherUuid.Equals(m_puTwofish);
			PwUuid puCipher = (bTwofish ? m_puTwofish : StandardAesEngine.AesUuid);
			hf |= (bTwofish ? KdbHeaderFlags.Twofish : KdbHeaderFlags.Aes);

			byte[] pbHeader;
			using(MemoryStream ms = new MemoryStream())
			{
				using(BinaryWriter bw = new BinaryWriter(ms))
				{
					bw.Write(KdbxFile.FileSignatureOld1);
					bw.Write(KdbxFile.FileSignatureOld2);
					bw.Write((uint)hf);
					bw.Write(KdbVersion);
					bw.Write(pbMasterSalt);
					bw.Write(pbIV);
					bw.Write(cGroups);
					bw.Write(cEntries);
					bw.Write(new byte[32]); // Content hash placeholder
					bw.Write(pbKdfSeed);
					bw.Write(cKdfRounds);
				}

				pbHeader = ms.ToArray();
				Debug.Assert(pbHeader.Length == KdbHeaderSize);
			}

			byte[] pbHeaderHash = HashHeaderWithoutContentHash(pbHeader);

			byte[] pbContent;
			Dictionary<PwGroup, uint> dGroups = new Dictionary<PwGroup, uint>();
			using(MemoryStream ms = new MemoryStream())
			{
				using(BinaryWriter bw = new BinaryWriter(ms))
				{
					WriteGroups(bw, pg, 0, dGroups, pbHeaderHash);
					Debug.Assert((uint)dGroups.Count == cGroups);

					WriteEntries(bw, pg, dGroups, slLogger);

					uint uRootGroup = dGroups[pg];
					foreach(KeyValuePair<string, byte[]> kvp in dMetaStreams)
						WriteMetaStream(bw, kvp.Key, kvp.Value, uRootGroup, slLogger);
				}

				pbContent = ms.ToArray();
			}

			byte[] pbKey = null;
			try
			{
				Array.Copy(CryptoUtil.HashSha256(pbContent), 0, pbHeader,
					KdbHeaderContentHashPosition, 32);
				sOutput.Write(pbHeader, 0, pbHeader.Length);

				ICipherEngine ce = CipherPool.GlobalPool.GetCipher(puCipher);
				if(ce == null) throw new Exception(KLRes.AlgorithmUnknown);

				pbKey = ComputeCipherKey(pd.MasterKey, p, pbMasterSalt);

				using(Stream s = ce.EncryptStream(sOutput, pbKey, pbIV))
				{
					s.Write(pbContent, 0, pbContent.Length);
				}
			}
			finally
			{
				MemUtil.ZeroByteArray(pbContent);
				MemUtil.ZeroByteArray(pbKey);
			}

			return true;
		}

		private void WriteGroups(BinaryWriter bw, PwGroup pg, ushort usLevel,
			Dictionary<PwGroup, uint> dGroups, byte[] pbHeaderHash)
		{
			uint uIndex = (uint)dGroups.Count + 1;
			dGroups[pg] = uIndex;

			uint uFlags = (pg.IsExpanded ? KdbGroupExpanded : 0);

			if(uIndex == 1) WriteField(bw, 0x0000, GetExtData(pbHeaderHash));
			WriteField(bw, 0x0001, MemUtil.UInt32ToBytes(uIndex));
			WriteField(bw, 0x0002, pg.Name);
			WriteField(bw, 0x0003, PackTime(pg.CreationTime));
			WriteField(bw, 0x0004, PackTime(pg.LastModificationTime));
			WriteField(bw, 0x0005, PackTime(pg.LastAccessTime));
			WriteField(bw, 0x0006, PackTime(pg.Expires ? pg.ExpiryTime : m_dtNeverExpires));
			WriteField(bw, 0x0007, MemUtil.UInt32ToBytes((uint)pg.IconId));
			WriteField(bw, 0x0008, MemUtil.UInt16ToBytes(usLevel));
			WriteField(bw, 0x0009, MemUtil.UInt32ToBytes(uFlags));
			WriteField(bw, 0xFFFF, MemUtil.EmptyByteArray);

			++usLevel;
			foreach(PwGroup pgSub in pg.Groups)
				WriteGroups(bw, pgSub, usLevel, dGroups, pbHeaderHash);
		}

		private void WriteEntries(BinaryWriter bw, PwGroup pg,
			Dictionary<PwGroup, uint> dGroups, IStatusLogger sl)
		{
			uint uGroup = dGroups[pg];

			foreach(PwEntry pe in pg.Entries) WriteEntry(bw, pe, uGroup, sl);

			foreach(PwGroup pgSub in pg.Groups) WriteEntries(bw, pgSub, dGroups, sl);
		}

		private void WriteEntry(BinaryWriter bw, PwEntry pe, uint uParentGroup,
			IStatusLogger sl)
		{
			string strNotes = pe.Strings.ReadSafe(PwDefs.NotesField);
			WriteCustomStrings(ref strNotes, pe);
			WriteUrlOverride(ref strNotes, pe);
			WriteAutoType(ref strNotes, pe);

			WriteField(bw, 0x0001, pe.Uuid.UuidBytes);
			WriteField(bw, 0x0002, MemUtil.UInt32ToBytes(uParentGroup));
			WriteField(bw, 0x0003, MemUtil.UInt32ToBytes((uint)pe.IconId));
			WriteField(bw, 0x0004, pe.Strings.ReadSafe(PwDefs.TitleField));
			WriteField(bw, 0x0005, pe.Strings.ReadSafe(PwDefs.UrlField));
			WriteField(bw, 0x0006, pe.Strings.ReadSafe(PwDefs.UserNameField));
			WriteField(bw, 0x0007, pe.Strings.ReadSafe(PwDefs.PasswordField));
			WriteField(bw, 0x0008, strNotes);
			WriteField(bw, 0x0009, PackTime(pe.CreationTime));
			WriteField(bw, 0x000A, PackTime(pe.LastModificationTime));
			WriteField(bw, 0x000B, PackTime(pe.LastAccessTime));
			WriteField(bw, 0x000C, PackTime(pe.Expires ? pe.ExpiryTime : m_dtNeverExpires));

			foreach(KeyValuePair<string, ProtectedBinary> kvp in pe.Binaries)
			{
				WriteField(bw, 0x000D, kvp.Key);

				byte[] pb = kvp.Value.ReadData();
				WriteField(bw, 0x000E, pb);
				MemUtil.ZeroByteArray(pb);

				break; // In KDB, an entry may have at most one attachment
			}
			if((pe.Binaries.UCount >= 2) && (sl != null))
			{
				string strNL = MessageService.NewLine;
				sl.SetText(KdbStatusPrefix + KPRes.FormatOnlyOneAttachment + strNL + strNL +
					KPRes.Entry + ":" + strNL +
					KPRes.Title + ": " + pe.Strings.ReadSafe(PwDefs.TitleField) + "." + strNL +
					KPRes.UserNameStc + ": " + pe.Strings.ReadSafe(PwDefs.UserNameField) + ".",
					LogStatusType.Warning);
			}

			WriteField(bw, 0xFFFF, MemUtil.EmptyByteArray);
		}

		private static void WriteField(BinaryWriter bw, ushort usType, byte[] pb)
		{
			if(pb == null) { Debug.Assert(false); pb = MemUtil.EmptyByteArray; }

			bw.Write(usType);
			bw.Write(pb.Length);
			bw.Write(pb);
		}

		private static void WriteField(BinaryWriter bw, ushort usType, string str)
		{
			if(str == null) { Debug.Assert(false); str = string.Empty; }

			byte[] pb = StrUtil.Utf8.GetBytes(str);

			bw.Write(usType);
			bw.Write(pb.Length + 1);
			bw.Write(pb);
			bw.Write((byte)0);
		}

		private static byte[] GetExtData(byte[] pbHeaderHash)
		{
			using(MemoryStream ms = new MemoryStream())
			{
				using(BinaryWriter bw = new BinaryWriter(ms))
				{
					WriteField(bw, 0x0001, pbHeaderHash);
					WriteField(bw, 0x0002, CryptoRandom.Instance.GetRandomBytes(32));
					WriteField(bw, 0xFFFF, MemUtil.EmptyByteArray);
				}

				return ms.ToArray();
			}
		}

		private static void WriteCustomStrings(ref string strNotes, PwEntry pe)
		{
			bool bFirst = true;
			foreach(KeyValuePair<string, ProtectedString> kvp in pe.Strings)
			{
				if(PwDefs.IsStandardField(kvp.Key)) continue;

				if(bFirst)
				{
					strNotes = strNotes.TrimEnd();
					if(strNotes.Length != 0) strNotes += MessageService.NewParagraph;

					bFirst = false;
				}

				strNotes += kvp.Key + ": " + kvp.Value.ReadString() +
					MessageService.NewLine;
			}
		}

		private static void WriteUrlOverride(ref string strNotes, PwEntry pe)
		{
			if(pe.OverrideUrl.Length == 0) return;

			strNotes = strNotes.TrimEnd();
			if(strNotes.Length != 0) strNotes += MessageService.NewParagraph;

			strNotes += UrlOverridePrefix + " " + pe.OverrideUrl +
				MessageService.NewLine;
		}

		private static void WriteAutoType(ref string strNotes, PwEntry pe)
		{
			string strNotesT = strNotes.TrimEnd();
			StringBuilder sb = new StringBuilder();
			uint uIndex = 0;
			bool bTan = PwDefs.IsTanEntry(pe);

			Func<string, string> fConvert = (strSeq => ConvertAutoTypeSequence(
				(string.IsNullOrEmpty(strSeq) ? pe.GetAutoTypeSequence() :
				strSeq), false));

			if((pe.AutoType.DefaultSequence.Length != 0) &&
				(pe.AutoType.AssociationsCount == 0)) // Avoid broken indices
			{
				if(strNotesT.Length != 0)
					sb.Append(MessageService.NewParagraph);

				sb.Append(AutoTypePrefix);
				sb.Append(": ");
				sb.AppendLine(fConvert(pe.AutoType.DefaultSequence));

				++uIndex;
			}

			foreach(AutoTypeAssociation a in pe.AutoType.Associations)
			{
				if((uIndex == 0) && (strNotesT.Length != 0))
					sb.Append(MessageService.NewParagraph);

				string strSuffix = ((uIndex == 0) ? string.Empty :
					("-" + uIndex.ToString(NumberFormatInfo.InvariantInfo)));

				string strSeq = fConvert(a.Sequence);
				if((!bTan && (strSeq != PwDefs.DefaultAutoTypeSequence)) ||
					(bTan && (strSeq != PwDefs.DefaultAutoTypeSequenceTan)))
				{
					sb.Append(AutoTypePrefix + strSuffix + ": ");
					sb.AppendLine(strSeq);
				}

				sb.Append(AutoTypeWindowPrefix + strSuffix + ": ");
				sb.AppendLine(a.WindowName);

				++uIndex;
			}

			if(sb.Length != 0) strNotes = strNotesT + sb.ToString();
		}

		private void WriteMetaStream(BinaryWriter bw, string strName,
			byte[] pbData, uint uParentGroup, IStatusLogger sl)
		{
			if(string.IsNullOrEmpty(strName)) { Debug.Assert(false); return; }
			if(pbData == null) { Debug.Assert(false); return; }

			PwEntry pe = new PwEntry(true, true);
			pe.IconId = KdbMetaIcon;
			pe.Strings.Set(PwDefs.TitleField, new ProtectedString(false, KdbMetaTitle));
			pe.Strings.Set(PwDefs.UserNameField, new ProtectedString(false, KdbMetaUserName));
			pe.Strings.Set(PwDefs.UrlField, new ProtectedString(false, KdbMetaUrl));
			pe.Strings.Set(PwDefs.NotesField, new ProtectedString(false, strName));
			pe.Binaries.Set(KdbMetaBinary, new ProtectedBinary(false, pbData));

			WriteEntry(bw, pe, uParentGroup, sl);
		}
	}
}
