/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2011 Dominik Reichl <dominik.reichl@t-online.de>

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

namespace KeePassLib.Security
{
	/// <summary>
	/// Represents an object that is encrypted using a XOR pad until
	/// it is read. The key XOR pad can be changed without revealing the
	/// protected data in process memory.
	/// </summary>
	public sealed class XorredBuffer
	{
		private byte[] m_pbData = new byte[0]; // Never null
		private byte[] m_pbXorPad = new byte[0]; // Never null

		/// <summary>
		/// Length of the protected data in bytes.
		/// </summary>
		public uint Length
		{
			get { return (uint)m_pbData.Length; }
		}

		/// <summary>
		/// Construct a new XOR-protected object using a protected byte array
		/// and a XOR pad that decrypts the protected data. The
		/// <paramref name="pbProtectedData" /> byte array must have the same size
		/// as the <paramref name="pbXorPad" /> byte array.
		/// </summary>
		/// <param name="pbProtectedData">Protected data (XOR pad applied).</param>
		/// <param name="pbXorPad">XOR pad that is used to decrypt the
		/// <paramref name="pbData" /> parameter.</param>
		/// <exception cref="System.ArgumentNullException">Thrown if one of the input
		/// parameters is <c>null</c>.</exception>
		/// <exception cref="System.ArgumentException">Thrown if the byte arrays are
		/// of different size.</exception>
		public XorredBuffer(byte[] pbProtectedData, byte[] pbXorPad)
		{
			Debug.Assert(pbProtectedData != null); if(pbProtectedData == null) throw new ArgumentNullException("pbProtectedData");
			Debug.Assert(pbXorPad != null); if(pbXorPad == null) throw new ArgumentNullException("pbXorPad");

			Debug.Assert(pbProtectedData.Length == pbXorPad.Length);
			if(pbProtectedData.Length != pbXorPad.Length) throw new ArgumentException();

			m_pbData = pbProtectedData;
			m_pbXorPad = pbXorPad;
		}

		private void Decrypt()
		{
			Debug.Assert((m_pbData.Length == m_pbXorPad.Length) || (m_pbXorPad.Length == 0));

			if(m_pbData.Length == m_pbXorPad.Length)
			{
				for(int i = 0; i < m_pbData.Length; ++i)
					m_pbData[i] ^= m_pbXorPad[i];

				m_pbXorPad = new byte[0];
			}
		}

		/// <summary>
		/// Decrypt the buffer. The <c>XorredBuffer</c> protection is useless
		/// after you used this method. The object cannot be re-encrypted.
		/// </summary>
		/// <returns>Unprotected plain-text byte array.</returns>
		public byte[] ReadPlainText()
		{
			Decrypt();
			return m_pbData;
		}

		/// <summary>
		/// Change the protection key for this <c>XorredBuffer</c> object.
		/// The data will first be decrypted using the old key and then
		/// re-encrypted using the new key. This operation doesn't reveal
		/// the plain-text in the process memory.
		/// </summary>
		/// <param name="pbNewXorPad">New protection pad. Must contain exactly
		/// the same number of bytes as the length of the currently protected data.
		/// Use the <c>Length</c> property of the <c>XorredBuffer</c> to query
		/// the data length and pass a correct number of bytes to <c>ChangeKey</c>.</param>
		/// <returns>New protected data (encrypted using the new XOR pad).</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if the input
		/// parameter is <c>null</c>.</exception>
		/// <exception cref="System.ArgumentException">Thrown if the input
		/// byte array doesn't have the correct size.</exception>
		public byte[] ChangeKey(byte[] pbNewXorPad)
		{
			Debug.Assert(pbNewXorPad != null); if(pbNewXorPad == null) throw new ArgumentNullException("pbNewXorPad");

			Debug.Assert(pbNewXorPad.Length == m_pbData.Length);
			if(pbNewXorPad.Length != m_pbData.Length) throw new ArgumentException();

			if(m_pbXorPad.Length == m_pbData.Length) // Data is protected
			{
				for(int i = 0; i < m_pbData.Length; ++i)
					m_pbData[i] ^= (byte)(m_pbXorPad[i] ^ pbNewXorPad[i]);
			}
			else // Data is unprotected
			{
				for(int i = 0; i < m_pbData.Length; ++i)
					m_pbData[i] ^= pbNewXorPad[i];
			}

			m_pbXorPad = pbNewXorPad;
			return m_pbData;
		}

		public bool EqualsValue(XorredBuffer xb)
		{
			if(xb == null) { Debug.Assert(false); throw new ArgumentNullException("xb"); }

			if(xb.m_pbData.Length != m_pbData.Length) return false;

			bool bDecThis = (m_pbData.Length == m_pbXorPad.Length);
			bool bDecOther = (xb.m_pbData.Length == xb.m_pbXorPad.Length);
			for(int i = 0; i < m_pbData.Length; ++i)
			{
				byte bt1 = m_pbData[i];
				if(bDecThis) bt1 ^= m_pbXorPad[i];

				byte bt2 = xb.m_pbData[i];
				if(bDecOther) bt2 ^= xb.m_pbXorPad[i];

				if(bt1 != bt2) return false;
			}

			return true;
		}

		public bool EqualsValue(byte[] pb)
		{
			if(pb == null) { Debug.Assert(false); throw new ArgumentNullException("pb"); }

			if(pb.Length != m_pbData.Length) return false;

			if(m_pbData.Length == m_pbXorPad.Length)
			{
				for(int i = 0; i < m_pbData.Length; ++i)
				{
					if((byte)(m_pbData[i] ^ m_pbXorPad[i]) != pb[i]) return false;
				}
				return true;
			}

			for(int i = 0; i < m_pbData.Length; ++i)
			{
				if(m_pbData[i] != pb[i]) return false;
			}
			return true;
		}

		/// <summary>
		/// XOR all bytes in a data buffer with a pad. Both byte arrays must
		/// be of the same size.
		/// </summary>
		/// <param name="pbData">Data to be protected.</param>
		/// <param name="pbPad">XOR pad.</param>
		/// <exception cref="System.ArgumentNullException">Thrown if one of the
		/// parameters is <c>null</c>.</exception>
		/// <exception cref="System.ArgumentException">Thrown if the length of
		/// the data array and the pad aren't equal.</exception>
		[Obsolete("Use MemUtil.XorArray instead.")]
		public static void XorArrays(byte[] pbData, byte[] pbPad)
		{
			Debug.Assert(pbData != null); if(pbData == null) throw new ArgumentNullException("pbData");
			Debug.Assert(pbPad != null); if(pbPad == null) throw new ArgumentNullException("pbPad");
			
			Debug.Assert(pbData.Length == pbPad.Length);
			if(pbData.Length != pbPad.Length) throw new ArgumentException();

			for(int i = 0; i < pbData.Length; ++i)
				pbData[i] ^= pbPad[i];
		}
	}
}
