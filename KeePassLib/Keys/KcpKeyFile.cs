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
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Text;

using KeePassLib.Cryptography;
using KeePassLib.Resources;
using KeePassLib.Security;
using KeePassLib.Serialization;
using KeePassLib.Utility;

namespace KeePassLib.Keys
{
	public sealed class KcpKeyFile : IUserKey
	{
		private readonly string m_strPath;
		private readonly ProtectedBinary m_pbKeyData;

		public string Path
		{
			get { return m_strPath; }
		}

		public ProtectedBinary KeyData
		{
			get { return m_pbKeyData; }
		}

		public KcpKeyFile(string strKeyFile) :
			this(IOConnectionInfo.FromPath(strKeyFile), false)
		{
		}

		public KcpKeyFile(string strKeyFile, bool bThrowIfDbFile) :
			this(IOConnectionInfo.FromPath(strKeyFile), bThrowIfDbFile)
		{
		}

		public KcpKeyFile(IOConnectionInfo iocKeyFile) :
			this(iocKeyFile, false)
		{
		}

		public KcpKeyFile(IOConnectionInfo iocKeyFile, bool bThrowIfDbFile)
		{
			if(iocKeyFile == null) throw new ArgumentNullException("iocKeyFile");

			byte[] pbFileData;
			using(Stream s = IOConnection.OpenRead(iocKeyFile))
			{
				pbFileData = MemUtil.Read(s);
			}
			if(pbFileData == null) throw new IOException();

			if(bThrowIfDbFile && (pbFileData.Length >= 8))
			{
				uint uSig1 = MemUtil.BytesToUInt32(MemUtil.Mid(pbFileData, 0, 4));
				uint uSig2 = MemUtil.BytesToUInt32(MemUtil.Mid(pbFileData, 4, 4));

				if(((uSig1 == KdbxFile.FileSignature1) &&
					(uSig2 == KdbxFile.FileSignature2)) ||
					((uSig1 == KdbxFile.FileSignaturePreRelease1) &&
					(uSig2 == KdbxFile.FileSignaturePreRelease2)) ||
					((uSig1 == KdbxFile.FileSignatureOld1) &&
					(uSig2 == KdbxFile.FileSignatureOld2)))
#if KeePassLibSD
					throw new Exception(KLRes.KeyFileDbSel);
#else
					throw new InvalidDataException(KLRes.KeyFileDbSel);
#endif
			}

			byte[] pbKey = LoadKeyFile(pbFileData);

			m_strPath = iocKeyFile.Path;
			m_pbKeyData = new ProtectedBinary(true, pbKey);

			MemUtil.ZeroByteArray(pbKey);
			MemUtil.ZeroByteArray(pbFileData);
		}

		private static byte[] LoadKeyFile(byte[] pbFileData)
		{
			if(pbFileData == null) throw new ArgumentNullException("pbFileData");

			byte[] pbKey = LoadKeyFileXml(pbFileData);
			if(pbKey != null) return pbKey;

			int cb = pbFileData.Length;
			if(cb == 32) return pbFileData;

			if(cb == 64)
			{
				pbKey = LoadKeyFileHex(pbFileData);
				if(pbKey != null) return pbKey;
			}

			return CryptoUtil.HashSha256(pbFileData);
		}

		private static byte[] LoadKeyFileXml(byte[] pbFileData)
		{
			KfxFile kf;
			try
			{
				using(MemoryStream ms = new MemoryStream(pbFileData, false))
				{
					kf = KfxFile.Load(ms);
				}
			}
			catch(Exception) { return null; }

			// We have a syntactically valid XML key file;
			// failing to verify the key should throw an exception
			return ((kf != null) ? kf.GetKey() : null);
		}

		private static byte[] LoadKeyFileHex(byte[] pbFileData)
		{
			if(pbFileData == null) { Debug.Assert(false); return null; }

			try
			{
				int cc = pbFileData.Length;
				if((cc & 1) != 0) { Debug.Assert(false); return null; }

				if(!StrUtil.IsHexString(pbFileData, true)) return null;

				string strHex = StrUtil.Utf8.GetString(pbFileData);
				return MemUtil.HexStringToByteArray(strHex);
			}
			catch(Exception) { Debug.Assert(false); }

			return null;
		}

		public static void Create(string strFilePath, byte[] pbAdditionalEntropy)
		{
			Create(strFilePath, pbAdditionalEntropy, 0);
		}

		internal static void Create(string strFilePath, byte[] pbAdditionalEntropy,
			ulong uVersion)
		{
			byte[] pbRandom = CryptoRandom.Instance.GetRandomBytes(32);
			if((pbRandom == null) || (pbRandom.Length != 32))
				throw new SecurityException();

			byte[] pbKey;
			if((pbAdditionalEntropy == null) || (pbAdditionalEntropy.Length == 0))
				pbKey = pbRandom;
			else
			{
				int cbAdd = pbAdditionalEntropy.Length;
				int cbRnd = pbRandom.Length;

				byte[] pbCmp = new byte[cbAdd + cbRnd];
				Array.Copy(pbAdditionalEntropy, 0, pbCmp, 0, cbAdd);
				Array.Copy(pbRandom, 0, pbCmp, cbAdd, cbRnd);

				pbKey = CryptoUtil.HashSha256(pbCmp);

				MemUtil.ZeroByteArray(pbCmp);
			}

			KfxFile kf = KfxFile.Create(uVersion, pbKey, null);

			IOConnectionInfo ioc = IOConnectionInfo.FromPath(strFilePath);
			using(Stream s = IOConnection.OpenWrite(ioc))
			{
				kf.Save(s);
			}

			MemUtil.ZeroByteArray(pbKey);
			MemUtil.ZeroByteArray(pbRandom);
		}
	}
}
