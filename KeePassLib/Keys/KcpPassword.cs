/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2018 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Text;

using KeePassLib.Cryptography;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePassLib.Keys
{
	/// <summary>
	/// Master password/passphrase as provided by the user.
	/// </summary>
	public sealed class KcpPassword : IUserKey
	{
		private ProtectedString m_psPassword = null; // Optional
		private ProtectedBinary m_pbKeyData;

		/// <summary>
		/// Get the password as protected string. This is <c>null</c>
		/// unless remembering the password has been turned on.
		/// </summary>
		public ProtectedString Password
		{
			get { return m_psPassword; }
		}

		/// <summary>
		/// Get key data. Querying this property is fast (it returns a
		/// reference to a cached <c>ProtectedBinary</c> object).
		/// If no key data is available, <c>null</c> is returned.
		/// </summary>
		public ProtectedBinary KeyData
		{
			get { return m_pbKeyData; }
		}

		public KcpPassword(byte[] pbPasswordUtf8)
		{
			SetKey(pbPasswordUtf8, true);
		}

		public KcpPassword(byte[] pbPasswordUtf8, bool bRememberPassword)
		{
			SetKey(pbPasswordUtf8, bRememberPassword);
		}

		public KcpPassword(string strPassword)
		{
			byte[] pb = StrUtil.Utf8.GetBytes(strPassword);
			try { SetKey(pb, true); }
			finally { MemUtil.ZeroByteArray(pb); }
		}

		private void SetKey(byte[] pbPasswordUtf8, bool bRememberPassword)
		{
			Debug.Assert(pbPasswordUtf8 != null);
			if(pbPasswordUtf8 == null) throw new ArgumentNullException("pbPasswordUtf8");

#if (DEBUG && !KeePassLibSD)
			Debug.Assert(ValidatePassword(pbPasswordUtf8));
#endif

			byte[] pbRaw = CryptoUtil.HashSha256(pbPasswordUtf8);
			try { m_pbKeyData = new ProtectedBinary(true, pbRaw); }
			finally { MemUtil.ZeroByteArray(pbRaw); }

			if(bRememberPassword)
				m_psPassword = new ProtectedString(true, pbPasswordUtf8);
		}

		// public void Clear()
		// {
		//	m_psPassword = null;
		//	m_pbKeyData = null;
		// }

#if (DEBUG && !KeePassLibSD)
		private static bool ValidatePassword(byte[] pb)
		{
			try
			{
				string str = StrUtil.Utf8.GetString(pb);
				return str.IsNormalized(NormalizationForm.FormC);
			}
			catch(Exception) { Debug.Assert(false); }

			return false;
		}
#endif
	}
}
