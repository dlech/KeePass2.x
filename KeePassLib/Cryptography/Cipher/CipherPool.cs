/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2021 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Text;

namespace KeePassLib.Cryptography.Cipher
{
	/// <summary>
	/// Pool of encryption/decryption algorithms (ciphers).
	/// </summary>
	public sealed class CipherPool
	{
		private List<ICipherEngine> m_lCiphers = new List<ICipherEngine>();

		private static CipherPool m_poolGlobal = null;
		public static CipherPool GlobalPool
		{
			get
			{
				CipherPool cp = m_poolGlobal;
				if(cp == null)
				{
					cp = new CipherPool();
					cp.AddCipher(new StandardAesEngine());
					cp.AddCipher(new ChaCha20Engine());

					m_poolGlobal = cp;
				}

				return cp;
			}
		}

		/// <summary>
		/// Remove all cipher engines from the current pool.
		/// </summary>
		public void Clear()
		{
			m_lCiphers.Clear();
		}

		/// <summary>
		/// Add a cipher engine to the pool.
		/// </summary>
		/// <param name="c">Cipher engine to add. Must not be <c>null</c>.</param>
		public void AddCipher(ICipherEngine c)
		{
			if(c == null) { Debug.Assert(false); throw new ArgumentNullException("c"); }

			// Return if a cipher with that ID is registered already
			foreach(ICipherEngine cEx in m_lCiphers)
			{
				if(cEx.CipherUuid.Equals(c.CipherUuid))
					return;
			}

			m_lCiphers.Add(c);
		}

		/// <summary>
		/// Get a cipher identified by its UUID.
		/// </summary>
		/// <param name="uuidCipher">UUID of the cipher to return.</param>
		/// <returns>Reference to the requested cipher. If the cipher is
		/// not found, <c>null</c> is returned.</returns>
		public ICipherEngine GetCipher(PwUuid uuidCipher)
		{
			foreach(ICipherEngine c in m_lCiphers)
			{
				if(c.CipherUuid.Equals(uuidCipher))
					return c;
			}

			return null;
		}

		/// <summary>
		/// Get the index of a cipher. This index is temporary and should
		/// not be stored or used to identify a cipher.
		/// </summary>
		/// <param name="uuidCipher">UUID of the cipher.</param>
		/// <returns>Index of the requested cipher. Returns <c>-1</c> if
		/// the specified cipher is not found.</returns>
		public int GetCipherIndex(PwUuid uuidCipher)
		{
			for(int i = 0; i < m_lCiphers.Count; ++i)
			{
				if(m_lCiphers[i].CipherUuid.Equals(uuidCipher))
					return i;
			}

			Debug.Assert(false);
			return -1;
		}

		/// <summary>
		/// Get the index of a cipher. This index is temporary and should
		/// not be stored or used to identify a cipher.
		/// </summary>
		/// <param name="strDisplayName">Name of the cipher. Note that
		/// multiple ciphers can have the same name. In this case, the
		/// first matching cipher is returned.</param>
		/// <returns>Cipher with the specified name or <c>-1</c> if
		/// no cipher with that name is found.</returns>
		public int GetCipherIndex(string strDisplayName)
		{
			for(int i = 0; i < m_lCiphers.Count; ++i)
			{
				if(m_lCiphers[i].DisplayName == strDisplayName)
					return i;
			}

			Debug.Assert(false);
			return -1;
		}

		/// <summary>
		/// Get the number of cipher engines in this pool.
		/// </summary>
		public int EngineCount
		{
			get { return m_lCiphers.Count; }
		}

		/// <summary>
		/// Get the cipher engine at the specified position. Throws
		/// an exception if the index is invalid. You can use this
		/// to iterate over all ciphers, but do not use it to
		/// identify ciphers.
		/// </summary>
		/// <param name="nIndex">Index of the requested cipher engine.</param>
		/// <returns>Reference to the cipher engine at the specified
		/// position.</returns>
		public ICipherEngine this[int nIndex]
		{
			get
			{
				if((nIndex < 0) || (nIndex >= m_lCiphers.Count))
					throw new ArgumentOutOfRangeException("nIndex");

				return m_lCiphers[nIndex];
			}
		}
	}
}
