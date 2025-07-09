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
using System.Security.Cryptography;
using System.Text;

using KeePass.Resources;

using KeePassLib;
using KeePassLib.Cryptography;
using KeePassLib.Cryptography.KeyDerivation;
using KeePassLib.Keys;
using KeePassLib.Resources;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	internal sealed partial class KeePassKdb1 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return true; } }

		public override string FormatName { get { return "KeePass KDB (1.x)"; } }
		public override string DefaultExtension { get { return KeePassKdb1.FileExt1; } }
		public override string ApplicationGroup { get { return PwDefs.ShortProductName; } }

		public override bool RequiresKey { get { return true; } }
		public override bool SupportsUuids { get { return true; } }

		public override Image SmallIcon
		{
			get { return KeePass.Properties.Resources.B16x16_KeePass; }
		}

		internal const string FileExt1 = "kdb";
		internal const string FileExt2 = "pwd";

		private const uint KdbVersion = 0x00030004;
		private const int KdbHeaderSize = 124;
		private const int KdbHeaderContentHashPosition = KdbHeaderSize - 32 - 32 - 4;

		private const uint KdbGroupExpanded = 1;

		private const PwIcon KdbMetaIcon = PwIcon.Key;
		private const string KdbMetaTitle = "Meta-Info";
		private const string KdbMetaUserName = "SYSTEM";
		private const string KdbMetaUrl = "$";
		private const string KdbMetaBinary = "bin-stream";

		private const string KdbMetaDefaultUserName = "Default User Name";
		private const string KdbMetaDatabaseColor = "Database Color";

		private const string KdbStatusPrefix = "KDB: ";

		private const string UrlOverridePrefix = "Url-Override:";
		private const string AutoTypePrefix = "Auto-Type";
		private const string AutoTypeWindowPrefix = "Auto-Type-Window";

		[Flags]
		private enum KdbHeaderFlags
		{
			None = 0,
			Sha256 = 1,
			Aes = 2,
			// ArcFour = 4,
			Twofish = 8
		}

		private readonly PwUuid m_puTwofish = new PwUuid(new byte[] {
			0xAD, 0x68, 0xF2, 0x9F, 0x57, 0x6F, 0x4B, 0xB9,
			0xA3, 0x6A, 0xD4, 0x7A, 0xF9, 0x65, 0x34, 0x6C });

		private readonly DateTime m_dtNeverExpires = (new DateTime(
			2999, 12, 28, 23, 59, 59, DateTimeKind.Local)).ToUniversalTime();
		internal DateTime NeverExpiresTime { get { return m_dtNeverExpires; } }

		private static byte[] HashHeaderWithoutContentHash(byte[] pb)
		{
			using(SHA256Managed h = new SHA256Managed())
			{
				const int i = KdbHeaderContentHashPosition;
				h.TransformBlock(pb, 0, i, pb, 0);
				h.TransformFinalBlock(pb, i + 32, KdbHeaderSize - (i + 32));
				return h.Hash;
			}
		}

		private static byte[] ComputeCipherKey(CompositeKey ck, KdfParameters p,
			byte[] pbMasterSalt)
		{
			if(ck == null) throw new ArgumentNullException("ck");
			if(p == null) throw new ArgumentNullException("p");
			if(pbMasterSalt == null) throw new ArgumentNullException("pbMasterSalt");

			KcpPassword kp = null;
			KcpKeyFile kf = null;
			foreach(IUserKey k in ck.UserKeys)
			{
				KcpPassword kpC = (k as KcpPassword);
				if(kpC != null)
				{
					if(kp == null) { kp = kpC; continue; }
					throw new Exception(KLRes.InvalidCompositeKey);
				}

				KcpKeyFile kfC = (k as KcpKeyFile);
				if(kfC != null)
				{
					if(!string.IsNullOrEmpty(kfC.Path) && KfxFile.CanLoad(kfC.Path))
						throw new Exception(kfC.Path + MessageService.NewParagraph +
							KLRes.FileVersionUnsupported);

					if(kf == null) { kf = kfC; continue; }
					throw new Exception(KLRes.InvalidCompositeKey);
				}

				throw new Exception(KPRes.KdbMasterKeyCmp);
			}

			if((kp == null) && (kf == null))
				throw new Exception(KLRes.InvalidCompositeKey);

			byte[] pbRaw = null, pbT = null, pbST = null;
			try
			{
				Func<byte[]> fPassword = (() =>
				{
					ProtectedString ps = kp.Password;
					if(ps == null)
						throw new Exception(KPRes.OptionReqOn + " '" +
							KPRes.MasterPasswordRmbWhileOpen + "'.");

					char[] v = ps.ReadChars();
					byte[] pb = null;
					try
					{
						pb = Encoding.Default.GetBytes(v);
						return CryptoUtil.HashSha256(pb);
					}
					finally
					{
						MemUtil.ZeroArray<char>(v);
						MemUtil.ZeroByteArray(pb);
					}
				});

				if(kf == null) pbRaw = fPassword();
				else
				{
					if(kp == null) pbRaw = kf.KeyData.ReadData();
					else
					{
						byte[] pbP = null, pbF = null, pbPF = null;
						try
						{
							pbP = fPassword();
							pbF = kf.KeyData.ReadData();
							pbPF = MemUtil.Concat(pbP, pbF);

							pbRaw = CryptoUtil.HashSha256(pbPF);
						}
						finally
						{
							MemUtil.ZeroByteArray(pbP);
							MemUtil.ZeroByteArray(pbF);
							MemUtil.ZeroByteArray(pbPF);
						}
					}
				}

				KdfEngine kdf = KdfPool.Get(p.KdfUuid);
				if(kdf == null) throw new Exception(KLRes.UnknownKdf);
				pbT = kdf.Transform(pbRaw, p);

				pbST = MemUtil.Concat(pbMasterSalt, pbT);
				return CryptoUtil.HashSha256(pbST);
			}
			finally
			{
				MemUtil.ZeroByteArray(pbRaw);
				MemUtil.ZeroByteArray(pbT);
				MemUtil.ZeroByteArray(pbST);
			}
		}

		internal static byte[] PackTime(DateTime dt)
		{
			dt = TimeUtil.ToLocal(dt, true);

			return new byte[5] {
				// 00YYYYYY YYYYYYMM MMDDDDDH HHHHMMMM MMSSSSSS
				(byte)((dt.Year >> 6) & 0x3F),
				(byte)(((dt.Year & 0x3F) << 2) | ((dt.Month >> 2) & 0x03)),
				(byte)(((dt.Month & 0x03) << 6) | ((dt.Day & 0x1F) << 1) |
					((dt.Hour >> 4) & 0x01)),
				(byte)(((dt.Hour & 0x0F) << 4) | ((dt.Minute >> 2) & 0x0F)),
				(byte)(((dt.Minute & 0x03) << 6) | (dt.Second & 0x3F)) };
		}

		internal static DateTime UnpackTime(byte[] pb)
		{
			if(pb == null) { Debug.Assert(false); throw new ArgumentNullException("pb"); }
			if(pb.Length != 5) { Debug.Assert(false); throw new ArgumentException(); }

			int n1 = pb[0], n2 = pb[1], n3 = pb[2], n4 = pb[3], n5 = pb[4];

			int nYear = (n1 << 6) | (n2 >> 2);
			int nMonth = ((n2 & 0x03) << 2) | (n3 >> 6);
			int nDay = (n3 >> 1) & 0x1F;
			int nHour = ((n3 & 0x01) << 4) | (n4 >> 4);
			int nMinute = ((n4 & 0x0F) << 2) | (n5 >> 6);
			int nSecond = n5 & 0x3F;

			// https://sourceforge.net/p/keepass/discussion/329221/thread/07599afd/
			if(nYear > 2999) { Debug.Assert(false); nYear = 2999; }
			if(nMonth > 12) { Debug.Assert(false); nMonth = 12; }
			if(nDay > 31) { Debug.Assert(false); nDay = 28; }
			if(nHour > 23) { Debug.Assert(false); nHour = 23; }
			if(nMinute > 59) { Debug.Assert(false); nMinute = 59; }
			if(nSecond > 59) { Debug.Assert(false); nSecond = 59; }

			try
			{
				return (new DateTime(nYear, nMonth, nDay, nHour, nMinute, nSecond,
					DateTimeKind.Local)).ToUniversalTime();
			}
			catch(Exception) { Debug.Assert(false); }

			return DateTime.UtcNow;
		}

		private static Dictionary<string, string> g_dSeq1xTo2x = null;
		private static string ConvertAutoTypeSequence(string str, bool b1xTo2x)
		{
			if(string.IsNullOrEmpty(str)) return string.Empty;

			if(g_dSeq1xTo2x == null)
				g_dSeq1xTo2x = new Dictionary<string, string>
				{
					{ "{AT}", "@" },
					{ "{PLUS}", "{+}" },
					{ "{PERCENT}", "{%}" },
					{ "{CARET}", "{^}" },
					{ "{TILDE}", "{~}" },
					{ "{LEFTBRACE}", "{{}" },
					{ "{RIGHTBRACE}", "{}}" },
					{ "{LEFTPAREN}", "{(}" },
					{ "{RIGHTPAREN}", "{)}" },
					{ "(+{END})", "+({END})" }
				};

			str = str.Trim();

			foreach(KeyValuePair<string, string> kvp in g_dSeq1xTo2x)
			{
				if(b1xTo2x)
					str = StrUtil.ReplaceCaseInsensitive(str, kvp.Key, kvp.Value);
				else
					str = StrUtil.ReplaceCaseInsensitive(str, kvp.Value, kvp.Key);
			}

			if(!b1xTo2x) str = CapitalizePlaceholders(str);

			return str;
		}

		private static string CapitalizePlaceholders(string str)
		{
			int iOffset = 0;
			while(true)
			{
				int iStart = str.IndexOf('{', iOffset);
				if(iStart < 0) break;

				int iEnd = str.IndexOf('}', iStart);
				if(iEnd < 0) break; // No assert (user data)

				string strPlh = str.Substring(iStart, iEnd - iStart + 1);

				if(!strPlh.StartsWith("{S:", StrUtil.CaseIgnoreCmp))
					str = str.Replace(strPlh, strPlh.ToUpperInvariant());

				iOffset = iStart + 1;
			}

			return str;
		}
	}
}
