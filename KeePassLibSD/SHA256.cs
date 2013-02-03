/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2013 Dominik Reichl <dominik.reichl@t-online.de>

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

using Org.BouncyCastle.Crypto.Digests;

namespace System.Security.Cryptography
{
	// Implement a simplified version of the SHA256Managed
	// class that ships with non-CF .NET 2.0.
	public sealed class SHA256Managed
	{
		public byte[] ComputeHash(byte[] pbData)
		{
			Sha256Digest sha256 = new Sha256Digest();
			sha256.BlockUpdate(pbData, 0, pbData.Length);

			byte[] pbHash = new byte[32];
			sha256.DoFinal(pbHash, 0);

			return pbHash;
		}
	}
}
