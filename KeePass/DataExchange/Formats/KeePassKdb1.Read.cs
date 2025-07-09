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
using System.Security.Cryptography;
using System.Text;

using KeePass.Resources;

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
		public override void Import(PwDatabase pdStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			byte[] pb = MemUtil.Read(sInput);
			if((pb == null) || (pb.Length < KdbHeaderSize))
				throw new Exception(KLRes.FileIncompleteExpc);

			uint uFlags, cGroups, cEntries;
			byte[] pbMasterSalt, pbIV, pbContentHash;
			using(MemoryStream ms = new MemoryStream(pb, false))
			{
				using(BinaryReader br = new BinaryReader(ms))
				{
					if((br.ReadUInt32() != KdbxFile.FileSignatureOld1) ||
						(br.ReadUInt32() != KdbxFile.FileSignatureOld2))
						throw new Exception(KLRes.FileSigInvalid);

					uFlags = br.ReadUInt32();

					const uint mV = 0xFFFF0000;
					if((br.ReadUInt32() & mV) != (KdbVersion & mV))
						throw new Exception(KLRes.FileVersionUnsupported);

					pbMasterSalt = br.ReadBytes(16);
					pbIV = br.ReadBytes(16);
					cGroups = br.ReadUInt32();
					cEntries = br.ReadUInt32();
					pbContentHash = br.ReadBytes(32);

					AesKdf kdf = new AesKdf();
					KdfParameters p = kdf.GetDefaultParameters();
					p.SetByteArray(AesKdf.ParamSeed, br.ReadBytes(32));
					p.SetUInt64(AesKdf.ParamRounds, br.ReadUInt32());
					pdStorage.KdfParameters = p;

					Debug.Assert(ms.Position == KdbHeaderSize);
				}
			}

			if((uFlags & (uint)KdbHeaderFlags.Sha256) == 0)
				throw new Exception(KLRes.AlgorithmUnknown);

			ICipherEngine ce = null;
			if((uFlags & (uint)KdbHeaderFlags.Aes) != 0)
				ce = CipherPool.GlobalPool.GetCipher(StandardAesEngine.AesUuid);
			else if((uFlags & (uint)KdbHeaderFlags.Twofish) != 0)
				ce = CipherPool.GlobalPool.GetCipher(m_puTwofish);
			if(ce == null) throw new Exception(KLRes.FileUnknownCipher);

			byte[] pbHeaderHash = HashHeaderWithoutContentHash(pb);

			byte[] pbKey = null, pbData = null;
			try
			{
				pbKey = ComputeCipherKey(pdStorage.MasterKey, pdStorage.KdfParameters,
					pbMasterSalt);

				try
				{
					using(MemoryStream ms = new MemoryStream(pb, KdbHeaderSize,
						pb.Length - KdbHeaderSize, false))
					{
						using(Stream s = ce.DecryptStream(ms, pbKey, pbIV))
						{
							pbData = MemUtil.Read(s);
						}
					}
				}
				catch(CryptographicException)
				{
					throw new Exception(KLRes.InvalidCompositeKeyOrCorrupted);
				}

				if(!MemUtil.ArraysEqual(CryptoUtil.HashSha256(pbData), pbContentHash))
					throw new Exception(KLRes.FileCorrupted);

				using(MemoryStream ms = new MemoryStream(pbData, false))
				{
					using(BinaryReader br = new BinaryReader(ms))
					{
						var dGroups = new Dictionary<uint, PwGroup>();

						var sGroups = new Stack<PwGroup>();
						sGroups.Push(pdStorage.RootGroup);

						for(uint u = 0; u < cGroups; ++u)
							ReadGroup(br, dGroups, sGroups, pbHeaderHash);

						for(uint u = 0; u < cEntries; ++u)
						{
							if((slLogger != null) && !slLogger.SetProgress(
								(uint)(((ulong)u * 100) / cEntries)))
								throw new OperationCanceledException();

							ReadEntry(br, dGroups, pdStorage, pbHeaderHash);
						}
					}
				}
			}
			finally
			{
				MemUtil.ZeroByteArray(pbKey);
				MemUtil.ZeroByteArray(pbData);
			}
		}

		private static byte[] ReadField(BinaryReader br, out ushort usType)
		{
			usType = br.ReadUInt16();

			int cb = br.ReadInt32();
			if(cb < 0) throw new OutOfMemoryException();

			byte[] pb = br.ReadBytes(cb);
			if((pb == null) || (pb.Length != cb))
				throw new Exception(KLRes.FileIncompleteExpc);
			return pb;
		}

		private static string ReadString(byte[] pb)
		{
			if(pb == null) throw new ArgumentNullException("pb");

			int cb = pb.Length;
			if((cb <= 0) || (pb[cb - 1] != 0)) throw new FormatException();

			return StrUtil.Utf8.GetString(pb, 0, cb - 1);
		}

		private void ReadGroup(BinaryReader br, Dictionary<uint, PwGroup> dGroups,
			Stack<PwGroup> sGroups, byte[] pbHeaderHash)
		{
			PwGroup pg = new PwGroup(true, true), pgParent = null;

			bool bEnd = false;
			while(!bEnd)
			{
				ushort usType;
				byte[] pb = ReadField(br, out usType);

				switch(usType)
				{
					case 0x0000:
						ReadExtData(pb, pbHeaderHash);
						break;

					case 0x0001:
						dGroups[MemUtil.BytesToUInt32(pb)] = pg;
						break;

					case 0x0002:
						pg.Name = ReadString(pb);
						break;

					case 0x0003:
						pg.CreationTime = UnpackTime(pb);
						break;

					case 0x0004:
						pg.LastModificationTime = UnpackTime(pb);
						break;

					case 0x0005:
						pg.LastAccessTime = UnpackTime(pb);
						break;

					case 0x0006:
						pg.ExpiryTime = UnpackTime(pb);
						pg.Expires = (pg.ExpiryTime != m_dtNeverExpires);
						break;

					case 0x0007:
						uint uIcon = MemUtil.BytesToUInt32(pb);
						if(uIcon < (uint)PwIcon.Count) pg.IconId = (PwIcon)uIcon;
						else { Debug.Assert(false); }
						break;

					case 0x0008:
						Debug.Assert(pgParent == null);
						ushort us = MemUtil.BytesToUInt16(pb);
						if(us >= sGroups.Count) throw new FormatException();
						while(us < (sGroups.Count - 1)) sGroups.Pop();
						pgParent = sGroups.Peek();
						sGroups.Push(pg);
						break;

					case 0x0009:
						uint uFlags = MemUtil.BytesToUInt32(pb);
						pg.IsExpanded = ((uFlags & KdbGroupExpanded) != 0);
						break;

					case 0xFFFF:
						bEnd = true;
						break;

					default: Debug.Assert(false); break;
				}
			}

			if(pgParent == null) throw new FormatException();
			pgParent.AddGroup(pg, true);
		}

		private void ReadEntry(BinaryReader br, Dictionary<uint, PwGroup> dGroups,
			PwDatabase pd, byte[] pbHeaderHash)
		{
			PwEntry pe = new PwEntry(true, true);
			PwGroup pgParent = null;
			string strBinary = null;
			byte[] pbBinary = null;

			bool bEnd = false;
			while(!bEnd)
			{
				ushort usType;
				byte[] pb = ReadField(br, out usType);

				switch(usType)
				{
					case 0x0000:
						ReadExtData(pb, pbHeaderHash);
						break;

					case 0x0001:
						pe.Uuid = new PwUuid(pb);
						break;

					case 0x0002:
						Debug.Assert(pgParent == null);
						dGroups.TryGetValue(MemUtil.BytesToUInt32(pb), out pgParent);
						break;

					case 0x0003:
						uint uIcon = MemUtil.BytesToUInt32(pb);
						if(uIcon < (uint)PwIcon.Count) pe.IconId = (PwIcon)uIcon;
						else { Debug.Assert(false); }
						break;

					case 0x0004:
						ImportUtil.Add(pe, PwDefs.TitleField, ReadString(pb), pd);
						break;

					case 0x0005:
						ImportUtil.Add(pe, PwDefs.UrlField, ReadString(pb), pd);
						break;

					case 0x0006:
						ImportUtil.Add(pe, PwDefs.UserNameField, ReadString(pb), pd);
						break;

					case 0x0007:
						ImportUtil.Add(pe, PwDefs.PasswordField, ReadString(pb), pd);
						break;

					case 0x0008:
						string strNotes = ReadString(pb);
						ReadUrlOverride(ref strNotes, pe);
						ReadAutoType(ref strNotes, pe);
						ImportUtil.Add(pe, PwDefs.NotesField, strNotes, pd);
						break;

					case 0x0009:
						pe.CreationTime = UnpackTime(pb);
						break;

					case 0x000A:
						pe.LastModificationTime = UnpackTime(pb);
						break;

					case 0x000B:
						pe.LastAccessTime = UnpackTime(pb);
						break;

					case 0x000C:
						pe.ExpiryTime = UnpackTime(pb);
						pe.Expires = (pe.ExpiryTime != m_dtNeverExpires);
						break;

					case 0x000D:
						strBinary = ReadString(pb);
						break;

					case 0x000E:
						pbBinary = pb;
						break;

					case 0xFFFF:
						bEnd = true;
						break;

					default: Debug.Assert(false); break;
				}
			}

			if((pbBinary != null) && (pbBinary.Length != 0))
			{
				if(string.IsNullOrEmpty(strBinary))
				{
					Debug.Assert(false);
					strBinary = KPRes.File;
				}

				pe.Binaries.Set(strBinary, new ProtectedBinary(false, pbBinary));
			}

			if(pgParent == null) throw new FormatException();
			if(!ReadMetaStream(pe, pd)) pgParent.AddEntry(pe, true);
		}

		private static void ReadExtData(byte[] pbData, byte[] pbHeaderHash)
		{
			if(pbData.Length == 0) return;

			using(MemoryStream ms = new MemoryStream(pbData, false))
			{
				using(BinaryReader br = new BinaryReader(ms))
				{
					bool bEnd = false;
					while(!bEnd)
					{
						ushort usType;
						byte[] pb = ReadField(br, out usType);

						switch(usType)
						{
							case 0x0000: break; // Ignore

							case 0x0001:
								if((pb.Length == pbHeaderHash.Length) &&
									!MemUtil.ArraysEqual(pb, pbHeaderHash))
									throw new Exception(KLRes.FileCorrupted);
								break;

							case 0x0002: break; // Ignore random data

							case 0xFFFF:
								bEnd = true;
								break;

							default: Debug.Assert(false); break;
						}
					}
				}
			}
		}

		private static void ReadUrlOverride(ref string strNotes, PwEntry pe)
		{
			int iS = strNotes.IndexOf(UrlOverridePrefix, StrUtil.CaseIgnoreCmp);
			if(iS < 0) return;

			int iE = strNotes.IndexOf('\n', iS);
			if(iE < 0) iE = strNotes.Length - 1;

			pe.OverrideUrl = strNotes.Substring(iS + UrlOverridePrefix.Length,
				iE - iS - UrlOverridePrefix.Length + 1).Trim();

			strNotes = strNotes.Remove(iS, iE - iS + 1);
		}

		private static string FindPrefixedLine(string[] vLines, string strPrefix)
		{
			foreach(string str in vLines)
			{
				if(str.StartsWith(strPrefix, StrUtil.CaseIgnoreCmp))
					return str;
			}

			return null;
		}

		private static void ReadAutoType(ref string strNotes, PwEntry pe)
		{
			if(string.IsNullOrEmpty(strNotes)) return;

			strNotes = StrUtil.NormalizeNewLines(strNotes, false);
			string[] vLines = strNotes.Split('\n');

			string strOvr = FindPrefixedLine(vLines, AutoTypePrefix + ":");
			if((strOvr != null) && (strOvr.Length > (AutoTypePrefix.Length + 1)))
			{
				strOvr = strOvr.Substring(AutoTypePrefix.Length + 1).Trim();
				pe.AutoType.DefaultSequence = ConvertAutoTypeSequence(
					strOvr, true);
			}

			StringBuilder sb = new StringBuilder();
			for(int iLine = 0; iLine < vLines.Length; ++iLine)
			{
				string strLine = vLines[iLine];
				bool bProcessed = false;

				for(int iIdx = 0; iIdx < 32; ++iIdx)
				{
					string s = ((iIdx == 0) ? string.Empty : ("-" +
						iIdx.ToString(NumberFormatInfo.InvariantInfo)));
					string strWndPrefix = AutoTypeWindowPrefix + s + ":";
					string strSeqPrefix = AutoTypePrefix + s + ":";

					if(strLine.StartsWith(strWndPrefix, StrUtil.CaseIgnoreCmp) &&
						(strLine.Length > strWndPrefix.Length))
					{
						string strWindow = strLine.Substring(strWndPrefix.Length).Trim();
						string strSeq = FindPrefixedLine(vLines, strSeqPrefix);
						if((strSeq != null) && (strSeq.Length > strSeqPrefix.Length))
							pe.AutoType.Add(new AutoTypeAssociation(strWindow,
								ConvertAutoTypeSequence(strSeq.Substring(
								strSeqPrefix.Length), true)));
						else // Window, but no sequence
							pe.AutoType.Add(new AutoTypeAssociation(strWindow,
								string.Empty));

						bProcessed = true;
						break;
					}
					else if(strLine.StartsWith(strSeqPrefix, StrUtil.CaseIgnoreCmp))
					{
						bProcessed = true;
						break;
					}
				}

				if(!bProcessed)
				{
					if(iLine == (vLines.Length - 1)) sb.Append(strLine);
					else sb.AppendLine(strLine);
				}
			}

			strNotes = sb.ToString();
			// pe.AutoType.Sort();
		}

		private static bool ReadMetaStream(PwEntry pe, PwDatabase pd)
		{
			if(pe == null) { Debug.Assert(false); return false; }

			byte[] pb = null;
			if(pe.Binaries.UCount != 1) return false;
			foreach(KeyValuePair<string, ProtectedBinary> kvp in pe.Binaries)
			{
				if(kvp.Key != KdbMetaBinary) return false;
				pb = kvp.Value.ReadData();
			}

			if((pe.IconId != KdbMetaIcon) ||
				(pe.Strings.ReadSafe(PwDefs.TitleField) != KdbMetaTitle) ||
				(pe.Strings.ReadSafe(PwDefs.UserNameField) != KdbMetaUserName) ||
				(pe.Strings.ReadSafe(PwDefs.UrlField) != KdbMetaUrl))
				return false;

			switch(pe.Strings.ReadSafe(PwDefs.NotesField))
			{
				case KdbMetaDefaultUserName:
					if(pb.Length >= 2)
					{
						pd.DefaultUserName = ReadString(pb);
						pd.DefaultUserNameChanged = DateTime.UtcNow;
					}
					else { Debug.Assert((pb.Length == 1) && (pb[0] == 0)); }
					break;

				case KdbMetaDatabaseColor:
					if(pb.Length >= 4) // >= sizeof(COLORREF), extensible
					{
						pd.Color = Color.FromArgb(pb[0], pb[1], pb[2]);
						pd.SettingsChanged = DateTime.UtcNow;
					}
					else { Debug.Assert(false); }
					break;

				default: break;
			}

			return true;
		}
	}
}
