/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2007 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Text;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Diagnostics;

namespace KeePassLib.Cryptography.Cipher
{
	/// <summary>
	/// Standard AES cipher implementation.
	/// </summary>
	public sealed class StandardAesEngine : ICipherEngine
	{
		private PwUuid m_uuidCipher;

		/// <summary>
		/// Get the UUID of this cipher engine as <c>PwUuid</c> object.
		/// </summary>
		public PwUuid CipherUuid
		{
			get
			{
				Debug.Assert(m_uuidCipher != null);
				return m_uuidCipher;
			}
		}

		/// <summary>
		/// Get a displayable name describing this cipher engine.
		/// </summary>
		public string DisplayName { get { return @"AES/Rijndael (256-Bit Key)"; } }

		/// <summary>
		/// UUID of the cipher engine. This ID uniquely identifies the
		/// AES engine. Must not be used by other ciphers.
		/// </summary>
		public static readonly byte[] AesUuidBytes = new byte[]{
			0x31, 0xC1, 0xF2, 0xE6, 0xBF, 0x71, 0x43, 0x50,
			0xBE, 0x58, 0x05, 0x21, 0x6A, 0xFC, 0x5A, 0xFF };

		public StandardAesEngine()
		{
			m_uuidCipher = new PwUuid(AesUuidBytes);
		}

		private static void AssertArguments(Stream stream, bool bEncrypt, byte[] pbKey, byte[] pbIV)
		{
			Debug.Assert(stream != null); if(stream == null) throw new ArgumentNullException("sPlainText");

			Debug.Assert(pbKey != null); if(pbKey == null) throw new ArgumentNullException("pbKey");
			Debug.Assert(pbKey.Length == 32);
			if(pbKey.Length != 32) throw new ArgumentException("Key must be 256 bits wide!");

			Debug.Assert(pbIV != null); if(pbIV == null) throw new ArgumentNullException("pbIV");
			Debug.Assert(pbIV.Length == 16);
			if(pbIV.Length != 16) throw new ArgumentException("Initialization vector must be 128 bits wide!");

			if(bEncrypt)
			{
				Debug.Assert(stream.CanWrite);
				if(stream.CanWrite == false) throw new ArgumentException("Stream must be writable!");
			}
			else // Decrypt
			{
				Debug.Assert(stream.CanRead);
				if(stream.CanRead == false) throw new ArgumentException("Encrypted stream must be readable!");
			}
		}

		public Stream EncryptStream(Stream sPlainText, byte[] pbKey, byte[] pbIV)
		{
			StandardAesEngine.AssertArguments(sPlainText, true, pbKey, pbIV);

			RijndaelManaged r = new RijndaelManaged();

			byte[] pbLocalIV = new byte[16];
			Array.Copy(pbIV, pbLocalIV, 16);
			r.IV = pbLocalIV;

			r.KeySize = 256;
			byte[] pbLocalKey = new byte[32];
			Array.Copy(pbKey, pbLocalKey, 32);
			r.Key = pbLocalKey;

			r.Mode = CipherMode.CBC;

			ICryptoTransform iTransform = r.CreateEncryptor();
			Debug.Assert(iTransform != null);
			if(iTransform == null) throw new SecurityException("Unable to create Rijndael transform!");

			return new CryptoStream(sPlainText, iTransform, CryptoStreamMode.Write);
		}

		public Stream DecryptStream(Stream sEncrypted, byte[] pbKey, byte[] pbIV)
		{
			StandardAesEngine.AssertArguments(sEncrypted, false, pbKey, pbIV);

			RijndaelManaged aes = new RijndaelManaged();

			Debug.Assert(pbIV.Length == 16);
			if(pbIV.Length != 16) throw new SecurityException();
			byte[] pbLocalIV = new byte[16];
			Array.Copy(pbIV, pbLocalIV, 16);
			aes.IV = pbLocalIV;

			Debug.Assert(pbKey.Length == 32);
			if(pbKey.Length != 32) throw new SecurityException();
			byte[] pbLocalKey = new byte[32];
			Array.Copy(pbKey, pbLocalKey, 32);
			aes.KeySize = 256;
			aes.Key = pbLocalKey;

			aes.Mode = CipherMode.CBC;
			ICryptoTransform iTransform = aes.CreateDecryptor();

			return new CryptoStream(sEncrypted, iTransform, CryptoStreamMode.Read);
		}
	}
}
