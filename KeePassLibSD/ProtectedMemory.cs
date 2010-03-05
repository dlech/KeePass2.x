/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2010 Dominik Reichl <dominik.reichl@t-online.de>

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
	public enum MemoryProtectionScope
	{
		SameProcess
	}

	/// <summary>
	/// This ProtectedMemory class does NOT work like the original implementation
	/// in the non-CF .NET! The MemoryProtectionScope is useless and only
	/// declared to make it compatible with the original.
	/// </summary>
	public static class ProtectedMemory
	{
		public static void Protect(byte[] userData, MemoryProtectionScope scope)
		{
			byte bt = 5;

			unchecked
			{
				for(int i = 0; i < userData.Length; ++i)
				{
					userData[i] ^= bt;
					bt += 13;
				}
			}
		}

		public static void Unprotect(byte[] encryptedData, MemoryProtectionScope scope)
		{
			Protect(encryptedData, scope);
		}
	}
}
