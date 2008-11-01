/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2008 Dominik Reichl <dominik.reichl@t-online.de>

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

namespace KeePassLib.Cryptography
{
	/// <summary>
	/// Algorithms supported by <c>CryptoRandomStream</c>.
	/// </summary>
	public enum CrsAlgorithm
	{
		/// <summary>
		/// Not supported.
		/// </summary>
		Null = 0,

		/// <summary>
		/// The ARCFour algorithm (RC4 compatible).
		/// </summary>
		ArcFour = 1
	}

	/// <summary>
	/// A random stream class. The class is initialized using random
	/// bytes provided by the caller. The produced stream has random
	/// properties, but for the same seed always the same stream
	/// is produced, i.e. this class can be used as stream cipher.
	/// </summary>
	public sealed class CryptoRandomStream
	{
		private CrsAlgorithm m_crsAlgorithm = CrsAlgorithm.ArcFour;
		private byte[] m_pbState = new byte[256];
		private byte m_i = 0;
		private byte m_j = 0;

		/// <summary>
		/// Construct a new cryptographically secure random stream object.
		/// </summary>
		/// <param name="genAlgorithm">Algorithm to use.</param>
		/// <param name="pbKey">Initialization key. Must not be <c>null</c> and
		/// must contain at least 1 byte.</param>
		/// <exception cref="System.ArgumentNullException">Thrown if the
		/// <paramref name="pbKey" /> parameter is <c>null</c>.</exception>
		/// <exception cref="System.ArgumentException">Thrown if the
		/// <paramref name="pbKey" /> parameter contains no bytes or the
		/// algorithm is unknown.</exception>
		public CryptoRandomStream(CrsAlgorithm genAlgorithm, byte[] pbKey)
		{
			m_crsAlgorithm = genAlgorithm;

			Debug.Assert(pbKey != null); if(pbKey == null) throw new ArgumentNullException("pbKey");

			uint uKeyLen = (uint)pbKey.Length;
			Debug.Assert(uKeyLen != 0); if(uKeyLen == 0) throw new ArgumentException();

			if(genAlgorithm == CrsAlgorithm.ArcFour)
			{
				uint w, inxKey = 0;

				// Fill the state linearly
				for(w = 0; w < 256; ++w) m_pbState[w] = (byte)w;

				byte i = 0, j = 0, t;
				for(w = 0; w < 256; ++w) // Key setup
				{
					j += (byte)(m_pbState[w] + pbKey[inxKey]);

					t = m_pbState[i]; // Swap entries
					m_pbState[i] = m_pbState[j];
					m_pbState[j] = t;

					++inxKey;
					if(inxKey >= uKeyLen) inxKey = 0;
				}

				this.GetRandomBytes(512); // Throw away the first bytes
			}
			else // Unknown algorithm
			{
				Debug.Assert(false);
				throw new ArgumentException();
			}
		}

		/// <summary>
		/// Get <paramref name="uRequestedCount" /> random bytes.
		/// </summary>
		/// <param name="uRequestedCount">Number of random bytes to retrieve.</param>
		/// <returns>Returns <paramref name="uRequestedCount" /> random bytes.</returns>
		public byte[] GetRandomBytes(uint uRequestedCount)
		{
			if(uRequestedCount == 0) return new byte[0];

			byte[] pbRet = new byte[uRequestedCount];

			if(m_crsAlgorithm == CrsAlgorithm.ArcFour)
			{
				byte t;

				for(uint w = 0; w < uRequestedCount; ++w)
				{
					++m_i;
					m_j += m_pbState[m_i];

					t = m_pbState[m_i]; // Swap entries
					m_pbState[m_i] = m_pbState[m_j];
					m_pbState[m_j] = t;

					t = (byte)(m_pbState[m_i] + m_pbState[m_j]);
					pbRet[w] = m_pbState[t];
				}
			}
			else { Debug.Assert(false); }

			return pbRet;
		}

		public ulong GetRandomUInt64()
		{
			byte[] pb = this.GetRandomBytes(8);

			return ((ulong)pb[0]) | ((ulong)pb[1] << 8) |
				((ulong)pb[2] << 16) | ((ulong)pb[3] << 24) |
				((ulong)pb[4] << 32) | ((ulong)pb[5] << 40) |
				((ulong)pb[6] << 48) | ((ulong)pb[7] << 56);
		}
	}
}
