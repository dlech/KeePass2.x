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
using System.Diagnostics;

namespace KeePassLib.Cryptography
{
	/// <summary>
	/// Cryptographically strong random number generator. The returned values
	/// are unpredictable and cannot be reproduced.
	/// <c>CryptoRandom</c> is a singleton class.
	/// </summary>
	public static class CryptoRandom
	{
		private static RNGCryptoServiceProvider m_rng = null;
		private static bool m_bNotInitializedYet = true;

		/// <summary>
		/// Initialize the random number generator. This function needs to be
		/// called before any of the <c>GetRandom*</c> member functions
		/// can be used. It must be called only once.
		/// </summary>
		/// <returns>Returns <c>true</c> if the generator was initialized
		/// successfully, <c>false</c> otherwise.</returns>
		public static bool Initialize()
		{
			Debug.Assert(m_bNotInitializedYet);
			if(!m_bNotInitializedYet) return true;

			Debug.Assert(m_rng == null);
			m_rng = new RNGCryptoServiceProvider();

			m_bNotInitializedYet = (m_rng == null);
			return (!m_bNotInitializedYet);
		}

		/// <summary>
		/// Get a number of cryptographically strong random bytes.
		/// </summary>
		/// <param name="uRequestedBytes">Number of requested random bytes.</param>
		/// <returns>A byte array consisting of <paramref name="nRequestedBytes" />
		/// random bytes.</returns>
		/// <exception cref="System.Security.SecurityException">Thrown if the
		/// random number generator hasn't been initialized.</exception>
		public static byte[] GetRandomBytes(uint uRequestedBytes)
		{
			Debug.Assert((!m_bNotInitializedYet) && (m_rng != null));
			if(m_rng == null) throw new SecurityException();

			if(uRequestedBytes == 0) return new byte[0]; // Allow zero-length array

			byte[] pb = new byte[uRequestedBytes];
			m_rng.GetBytes(pb);
			return pb;
		}
	}
}
