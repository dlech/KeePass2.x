/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2022 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.IO;
using System.Security;
using System.Text;

#if !KeePassUAP
using System.Security.Cryptography;
#endif

using KeePassLib.Resources;

namespace KeePassLib.Cryptography.Cipher
{
	public sealed class StandardAesEngine : ICipherEngine
	{
#if !KeePassUAP
		private const CipherMode SaeCipherMode = CipherMode.CBC;
		private const PaddingMode SaePaddingMode = PaddingMode.PKCS7;
#endif

		private static PwUuid g_uuidAes = null;
		public static PwUuid AesUuid
		{
			get
			{
				PwUuid pu = g_uuidAes;
				if(pu == null)
				{
					pu = new PwUuid(new byte[] {
						0x31, 0xC1, 0xF2, 0xE6, 0xBF, 0x71, 0x43, 0x50,
						0xBE, 0x58, 0x05, 0x21, 0x6A, 0xFC, 0x5A, 0xFF });
					g_uuidAes = pu;
				}

				return pu;
			}
		}

		public PwUuid CipherUuid
		{
			get { return StandardAesEngine.AesUuid; }
		}

		public string DisplayName
		{
			get
			{
				return ("AES/Rijndael (" + KLRes.KeyBits.Replace(@"{PARAM}",
					"256") + ", FIPS 197)");
			}
		}

		private static void ValidateArguments(Stream s, bool bEncrypt, byte[] pbKey, byte[] pbIV)
		{
			if(s == null) { Debug.Assert(false); throw new ArgumentNullException("s"); }

			if(pbKey == null) { Debug.Assert(false); throw new ArgumentNullException("pbKey"); }
			if(pbKey.Length != 32) { Debug.Assert(false); throw new ArgumentOutOfRangeException("pbKey"); }

			if(pbIV == null) { Debug.Assert(false); throw new ArgumentNullException("pbIV"); }
			if(pbIV.Length != 16) { Debug.Assert(false); throw new ArgumentOutOfRangeException("pbIV"); }

			if(bEncrypt)
			{
				Debug.Assert(s.CanWrite);
				if(!s.CanWrite) throw new ArgumentException("Stream must be writable!");
			}
			else // Decrypt
			{
				Debug.Assert(s.CanRead);
				if(!s.CanRead) throw new ArgumentException("Stream must be readable!");
			}
		}

		private static Stream CreateStream(Stream s, bool bEncrypt, byte[] pbKey, byte[] pbIV)
		{
			StandardAesEngine.ValidateArguments(s, bEncrypt, pbKey, pbIV);

#if KeePassUAP
			return StandardAesEngineExt.CreateStream(s, bEncrypt, pbKey, pbIV);
#else
			SymmetricAlgorithm a = CryptoUtil.CreateAes();
			if(a.BlockSize != 128) // AES block size
			{
				Debug.Assert(false);
				a.BlockSize = 128;
			}
			a.KeySize = 256;
			a.Mode = SaeCipherMode;
			a.Padding = SaePaddingMode;

			ICryptoTransform t;
			if(bEncrypt) t = a.CreateEncryptor(pbKey, pbIV);
			else t = a.CreateDecryptor(pbKey, pbIV);
			if(t == null) { Debug.Assert(false); throw new SecurityException("Unable to create AES transform!"); }

			return new CryptoStreamEx(s, t, bEncrypt ? CryptoStreamMode.Write :
				CryptoStreamMode.Read, a);
#endif
		}

		public Stream EncryptStream(Stream s, byte[] pbKey, byte[] pbIV)
		{
			return StandardAesEngine.CreateStream(s, true, pbKey, pbIV);
		}

		public Stream DecryptStream(Stream s, byte[] pbKey, byte[] pbIV)
		{
			return StandardAesEngine.CreateStream(s, false, pbKey, pbIV);
		}
	}
}
