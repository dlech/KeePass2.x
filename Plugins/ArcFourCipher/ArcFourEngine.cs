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
using System.Diagnostics;

using KeePassLib;
using KeePassLib.Cryptography.Cipher;

namespace ArcFourCipher
{
	public sealed class ArcFourEngine : ICipherEngine
	{
		private PwUuid m_uuidCipher;

		private static readonly byte[] ArcFourUuidBytes = new byte[]{
			0xA6, 0x47, 0x30, 0x81, 0xE9, 0x6E, 0x4F, 0xBA,
			0xAE, 0xDC, 0x98, 0xB3, 0xEA, 0x55, 0xA1, 0xC7
		};

		public ArcFourEngine()
		{
			m_uuidCipher = new PwUuid(ArcFourUuidBytes);
		}

		public PwUuid CipherUuid
		{
			get
			{
				Debug.Assert(m_uuidCipher != null);
				return m_uuidCipher;
			}
		}

		public string DisplayName
		{
			get { return "ArcFour Cipher (RC4 compatible)"; }
		}

		public Stream EncryptStream(Stream sPlainText, byte[] pbKey, byte[] pbIV)
		{
			return new ArcFourStream(sPlainText, true, pbKey, pbIV);
		}

		public Stream DecryptStream(Stream sEncrypted, byte[] pbKey, byte[] pbIV)
		{
			return new ArcFourStream(sEncrypted, false, pbKey, pbIV);
		}
	}
}
