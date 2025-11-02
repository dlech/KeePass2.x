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
using System.IO;

#if !KeePassUAP
using System.Security.Cryptography;
#endif

using KeePassLib.Cryptography;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePassLib.Keys
{
	/// <summary>
	/// A user key depending on the currently logged on user account.
	/// </summary>
	public sealed class KcpUserAccount : IUserKey
	{
		private const string UserKeyFileName = "ProtectedUserKey.bin";

		// Unique domain separation tag for this class
		private static readonly byte[] g_pbDomainSepTag = new byte[] {
			0xDE, 0x13, 0x5B, 0x5F, 0x18, 0xA3, 0x46, 0x70,
			0xB2, 0x57, 0x24, 0x29, 0x69, 0x88, 0x98, 0xE6
		};

		private readonly ProtectedBinary m_pbKeyData;

		public ProtectedBinary KeyData
		{
			get { return m_pbKeyData; }
		}

		public KcpUserAccount()
		{
			if(!CryptoUtil.IsProtectedDataSupported)
				throw new PlatformNotSupportedException(); // Windows 98/ME

			byte[] pbKey = (LoadUserKey() ?? CreateUserKey());

			try { m_pbKeyData = new ProtectedBinary(true, pbKey); }
			finally { MemUtil.ZeroByteArray(pbKey); }
		}

		// public void Clear()
		// {
		//	m_pbKeyData = null;
		// }

		private static string GetUserKeyFilePath(bool bCreate)
		{
#if KeePassUAP
			string strUserDir = EnvironmentExt.AppDataRoamingFolderPath;
#else
			string strUserDir = Environment.GetFolderPath(
				Environment.SpecialFolder.ApplicationData);
#endif

			strUserDir = UrlUtil.EnsureTerminatingSeparator(strUserDir, false) +
				PwDefs.ShortProductName;

			if(bCreate && !Directory.Exists(strUserDir))
				Directory.CreateDirectory(strUserDir);

			return (UrlUtil.EnsureTerminatingSeparator(strUserDir, false) +
				UserKeyFileName);
		}

		private static byte[] LoadUserKey()
		{
			string strFilePath = GetUserKeyFilePath(false);

			try
			{
				if(!File.Exists(strFilePath)) return null;

				byte[] pbProtectedKey = File.ReadAllBytes(strFilePath);
				if((pbProtectedKey == null) || (pbProtectedKey.Length == 0))
					return null;

				return CryptoUtil.UnprotectData(pbProtectedKey,
					g_pbDomainSepTag, DataProtectionScope.CurrentUser);
			}
			catch(Exception ex)
			{
				throw new ExtendedException(strFilePath, ex);
			}
		}

		private static byte[] CreateUserKey()
		{
			string strFilePath = GetUserKeyFilePath(true);

			byte[] pbRandomKey = CryptoRandom.Instance.GetRandomBytes(64);
			byte[] pbProtectedKey = CryptoUtil.ProtectData(pbRandomKey,
				g_pbDomainSepTag, DataProtectionScope.CurrentUser);

			try
			{
				File.WriteAllBytes(strFilePath, pbProtectedKey);

				byte[] pbLoadedKey = LoadUserKey();
				if(!MemUtil.ArraysEqual(pbLoadedKey, pbRandomKey))
					throw new InvalidDataException();
				return pbLoadedKey;
			}
			catch(Exception ex)
			{
				throw new ExtendedException(strFilePath, ex);
			}
			finally { MemUtil.ZeroByteArray(pbRandomKey); }
		}
	}
}
