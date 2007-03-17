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
using System.Security;
using System.Security.Cryptography;

using Microsoft.Win32;

using KeePassLib.Cryptography;
using KeePassLib.Security;

namespace KeePassLib.Keys
{
	/// <summary>
	/// A user key depending on the currently logged on Windows user account.
	/// </summary>
	public sealed class KcpUserAccount : IUserKey
	{
		private ProtectedBinary m_pbKeyData = null;

		private static readonly byte[] m_pbEntropy = new byte[]{
			0xDE, 0x13, 0x5B, 0x5F, 0x18, 0xA3, 0x46, 0x70,
			0xB2, 0x57, 0x24, 0x29, 0x69, 0x88, 0x98, 0xE6
		};

		/// <summary>
		/// Get key data. Querying this property is fast (it returns a
		/// reference to a cached <c>ProtectedBinary</c> object).
		/// If no key data is available, <c>null</c> is returned.
		/// </summary>
		public ProtectedBinary KeyData
		{
			get { return m_pbKeyData; }
		}

		/// <summary>
		/// Construct a machine user key.
		/// </summary>
		public KcpUserAccount()
		{
			byte[] pbKey = LoadUserKey();
			if(pbKey == null) pbKey = CreateUserKey();
			if(pbKey == null) throw new SecurityException();

			m_pbKeyData = new ProtectedBinary(true, pbKey);
			Array.Clear(pbKey, 0, pbKey.Length);
		}

		/// <summary>
		/// Clear the key and securely erase all security-critical information.
		/// </summary>
		public void Clear()
		{
			if(m_pbKeyData != null)
			{
				m_pbKeyData.Clear();
				m_pbKeyData = null;
			}
		}

		private static byte[] LoadUserKey()
		{
			try
			{
				RegistryKey kSoftware = Registry.CurrentUser.OpenSubKey("Software");
				
				RegistryKey kApp = kSoftware.OpenSubKey(PwDefs.ShortProductName);
				if(kApp == null)
				{
					kSoftware.Close();
					return null;
				}

				byte[] pbProtectedKey = kApp.GetValue(PwDefs.ProtectedUserRegKey, null)
					as byte[];

				kApp.Close();
				kSoftware.Close();

				if((pbProtectedKey == null) || (pbProtectedKey.Length == 0))
					return null;

				byte[] pbKey = ProtectedData.Unprotect(pbProtectedKey, m_pbEntropy,
					DataProtectionScope.CurrentUser);

				Array.Clear(pbProtectedKey, 0, pbProtectedKey.Length);
				return pbKey;
			}
			catch(Exception) { }

			return null;
		}

		private static byte[] CreateUserKey()
		{
			try
			{
				RegistryKey kSoftware = Registry.CurrentUser.OpenSubKey("Software");

				RegistryKey kApp = kSoftware.OpenSubKey(PwDefs.ShortProductName, true);
				if(kApp == null)
					kApp = kSoftware.CreateSubKey(PwDefs.ShortProductName);

				byte[] pbKey = CryptoRandom.GetRandomBytes(64);
				byte[] pbProtectedKey = ProtectedData.Protect(pbKey, m_pbEntropy,
					DataProtectionScope.CurrentUser);

				kApp.SetValue(PwDefs.ProtectedUserRegKey, pbProtectedKey,
					RegistryValueKind.Binary);

				kApp.Close();
				kSoftware.Close();

				Array.Clear(pbProtectedKey, 0, pbProtectedKey.Length);
				Array.Clear(pbKey, 0, pbKey.Length);

				return LoadUserKey(); // (Re-)load the key that we just stored
			}
			catch(Exception) { }

			return null;
		}
	}
}
