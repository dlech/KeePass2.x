/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2009 Dominik Reichl <dominik.reichl@t-online.de>

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

namespace KeePassLib
{
	public enum DataProtectionScope
	{
		CurrentUser
	}

	/// <summary>
	/// This ProtectedData class does NOT work like the original implementation
	/// in the non-CF .NET! The DataProtectionScope is useless and only
	/// declared to make it compatible with the original. Also, the optional
	/// entropy is ignored.
	/// </summary>
	public static class ProtectedData
	{
		public static byte[] Protect(byte[] userData, byte[] optionalEntropy,
			DataProtectionScope scope)
		{
			byte[] pb = new byte[userData.Length];
			Array.Copy(userData, pb, userData.Length);
			ProtectedMemory.Protect(pb, MemoryProtectionScope.SameProcess);
			return pb;
		}

		public static byte[] Unprotect(byte[] encryptedData, byte[] optionalEntropy,
			DataProtectionScope scope)
		{
			return Protect(encryptedData, optionalEntropy, scope);
		}
	}
}
