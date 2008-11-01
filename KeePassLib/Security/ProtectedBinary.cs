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
using System.Security.Cryptography;
using System.Diagnostics;

using KeePassLib.Cryptography;
using KeePassLib.Utility;

#if KeePassLibSD
using KeePassLibSD;
#endif

namespace KeePassLib.Security
{
	/// <summary>
	/// Represents a protected binary, i.e. a byte array that is encrypted
	/// in-memory.
	/// </summary>
	public sealed class ProtectedBinary
	{
		// In-memory protection is supported only on Windows 2000 SP3 and
		// higher.
		private static bool m_bProtectionSupported = true;

		private byte[] m_pbData = new byte[0];

		// The real length of the data. This value can be different than
		// m_pbData.Length, as the length of m_pbData always is a multiple
		// of 16 (required for fast in-memory protection).
		private uint m_uDataLen = 0;

		private bool m_bDoProtect = false; // See default constructor.

		private XorredBuffer m_xbEncrypted = null;
		
		/// <summary>
		/// A flag specifying whether the <c>ProtectedBinary</c> object has turned on
		/// in-memory protection or not.
		/// </summary>
		public bool IsProtected
		{
			get { return m_bDoProtect; }
		}

		/// <summary>
		/// A value specifying whether the <c>ProtectedString</c> object is currently
		/// in-memory protected or not. This flag can be different than
		/// <c>IsProtected</c>: if a <c>XorredBuffer</c> is used, the <c>IsProtected</c>
		/// flag represents the memory protection flag, but not the actual protection.
		/// In this case use <c>IsViewable</c>, which returns <c>true</c> if a
		/// <c>XorredBuffer</c> is currently in use.
		/// </summary>
		public bool IsViewable
		{
			get { return (!m_bDoProtect && (m_xbEncrypted == null)); }
		}

		/// <summary>
		/// Length of the stored data.
		/// </summary>
		public uint Length
		{
			get { return m_uDataLen; }
		}

		static ProtectedBinary()
		{
			try // Test if ProtectedMemory is supported
			{
				byte[] pbDummy = new byte[128];
				ProtectedMemory.Protect(pbDummy, MemoryProtectionScope.SameProcess);
			}
			catch(Exception) // Windows 98 / ME
			{
				m_bProtectionSupported = false;
			}
		}

		/// <summary>
		/// Construct a new, empty protected binary data object. Protection
		/// is disabled by default! You need to call the
		/// <c>EnableProtection</c> member function to enable the protection
		/// manually, if you wish the data to be protected.
		/// </summary>
		public ProtectedBinary()
		{
		}

		/// <summary>
		/// Construct a new, empty protected binary data object.
		/// </summary>
		/// <param name="bEnableProtection">If this parameter is <c>true</c>,
		/// the data will be encrypted in-memory. If it is <c>false</c>, the
		/// data is stored in plain-text in the process memory.</param>
		public ProtectedBinary(bool bEnableProtection)
		{
			m_bDoProtect = bEnableProtection;
		}

		/// <summary>
		/// Construct a new protected binary data object.
		/// </summary>
		/// <param name="bEnableProtection">If this paremeter is <c>true</c>,
		/// the data will be encrypted in-memory. If it is <c>false</c>, the
		/// data is stored in plain-text in the process memory.</param>
		/// <param name="pbInitialValue">Initial value of the protected
		/// object. The input parameter is not modified.</param>
		public ProtectedBinary(bool bEnableProtection, byte[] pbInitialValue)
		{
			m_bDoProtect = bEnableProtection;
			SetData(pbInitialValue);
		}

		/// <summary>
		/// Construct a new protected binary data object. Copy the data from
		/// an existing object.
		/// </summary>
		/// <param name="pbTemplate">Existing <c>ProtectedBinary</c> object,
		/// which is used to initialize the new object. This parameter must
		/// not be <c>null</c>.</param>
		/// <exception cref="System.ArgumentNullException">Thrown if the input
		/// parameter is <c>null</c>.</exception>
		public ProtectedBinary(ProtectedBinary pbTemplate)
		{
			Debug.Assert(pbTemplate != null); if(pbTemplate == null) throw new ArgumentNullException("pbTemplate");

			m_bDoProtect = pbTemplate.m_bDoProtect;

			byte[] pbBuf = pbTemplate.ReadData();
			SetData(pbBuf);
			MemUtil.ZeroByteArray(pbBuf);
		}

		/// <summary>
		/// Construct a new protected binary data object. Copy the data from
		/// a <c>XorredBuffer</c> object.
		/// </summary>
		/// <param name="bEnableProtection">Enable protection or not.</param>
		/// <param name="xbProtected"><c>XorredBuffer</c> object used to
		/// initialize the <c>ProtectedBinary</c> object.</param>
		/// <exception cref="System.ArgumentNullException">Thrown if the input
		/// parameter is <c>null</c>.</exception>
		public ProtectedBinary(bool bEnableProtection, XorredBuffer xbProtected)
		{
			Debug.Assert(xbProtected != null); if(xbProtected == null) throw new ArgumentNullException("xbProtected");

			m_bDoProtect = bEnableProtection;
			m_xbEncrypted = xbProtected;
		}

		/// <summary>
		/// Clear the protected data object. Doesn't change the protection level.
		/// </summary>
		public void Clear()
		{
			m_pbData = new byte[0];
			m_uDataLen = 0;

			m_xbEncrypted = null;
		}

		/// <summary>
		/// Change the protection level (protect or don't protect). Note: you
		/// only need to call this function if you really want to change the
		/// protection. If you specified the protection flag in the constructor,
		/// and don't want to change it, you don't need to call this function.
		/// </summary>
		/// <param name="bEnableProtection">If <c>true</c>, the data will be protected
		/// (encrypted in-memory). Otherwise the data will be stored in
		/// plain-text in the process memory.</param>
		public void EnableProtection(bool bEnableProtection)
		{
			if(m_xbEncrypted != null)
			{
				m_bDoProtect = bEnableProtection;
				return;
			}

			if(m_bDoProtect && !bEnableProtection) // Unprotect
			{
				byte[] pb = ReadData();
				Debug.Assert(pb.Length == m_uDataLen);

				Clear();

				m_pbData = pb;
				m_bDoProtect = false;
			}
			else if(!m_bDoProtect && bEnableProtection) // Protect
			{
				m_bDoProtect = true;

				SetData(m_pbData);
			}
		}

		/// <summary>
		/// Set protected data. This function also clears the internal
		/// <c>XorredBuffer</c> object.
		/// </summary>
		/// <param name="pbNew">Data to store in the protected object. The input
		/// byte array will not be modified, the data is copied to an internal
		/// buffer of the protected object. This parameter must not be <c>null</c>;
		/// if you want to clear the object, call the <c>Clear</c> member
		/// function.</param>
		/// <exception cref="System.ArgumentNullException">Thrown if the input
		/// parameter is <c>null</c>.</exception>
		public void SetData(byte[] pbNew)
		{
			this.Clear();

			Debug.Assert(pbNew != null);
			if(pbNew == null) throw new ArgumentNullException("pbNew");

			m_uDataLen = (uint)pbNew.Length;

			if(m_bDoProtect && m_bProtectionSupported)
			{
				int nAllocatedMem = (((int)m_uDataLen / 16) + 1) * 16;
				m_pbData = new byte[nAllocatedMem];
				Array.Clear(m_pbData, nAllocatedMem - 16, 16);
				Array.Copy(pbNew, m_pbData, (int)m_uDataLen);

				ProtectedMemory.Protect(m_pbData, MemoryProtectionScope.SameProcess);
			}
			else // !m_bDoProtect
			{
				m_pbData = new byte[m_uDataLen];
				pbNew.CopyTo(m_pbData, 0);
			}
		}

		/// <summary>
		/// Get the protected data as a byte array. Please note that the returned
		/// byte array is not protected and can therefore been read by any other
		/// applications. Make sure that your clear it properly after usage.
		/// </summary>
		/// <returns>Unprotected byte array. This is always a copy of the internal
		/// protected data and can therefore be cleared safely.</returns>
		public byte[] ReadData()
		{
			if(m_xbEncrypted != null)
			{
				byte[] pb = m_xbEncrypted.ReadPlainText();
				SetData(pb); // Clear the XorredBuffer object

				return (pb ?? new byte[0]);
			}

			if(m_pbData.Length == 0) return new byte[0];

			if(m_bDoProtect && m_bProtectionSupported)
			{
				Debug.Assert((m_pbData.Length % 16) == 0);
				ProtectedMemory.Unprotect(m_pbData, MemoryProtectionScope.SameProcess);
			}

			byte[] pbReturn = new byte[m_uDataLen];
			if(m_uDataLen > 0) Array.Copy(m_pbData, pbReturn, (int)m_uDataLen);

			if(m_bDoProtect && m_bProtectionSupported)
				ProtectedMemory.Protect(m_pbData, MemoryProtectionScope.SameProcess);

			return pbReturn;
		}

		/// <summary>
		/// Read the protected data and return it protected with a sequence
		/// of bytes generated by a random stream. The object's data will be
		/// invisible in process memory only if the object has been initialized
		/// using a <c>XorredBuffer</c>. If no <c>XorredBuffer</c> has been used
		/// or the binary has been read once already (in plain-text), the
		/// operation won't be secure and the protected string will be visible
		/// in process memory.
		/// </summary>
		/// <param name="crsRandomSource">Random number source.</param>
		/// <returns>Protected data.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if the input
		/// parameter is <c>null</c>.</exception>
		public byte[] ReadXorredData(CryptoRandomStream crsRandomSource)
		{
			Debug.Assert(crsRandomSource != null);
			if(crsRandomSource == null) throw new ArgumentNullException("crsRandomSource");

			if(m_xbEncrypted != null)
			{
				uint uLen = m_xbEncrypted.Length;
				byte[] randomPad = crsRandomSource.GetRandomBytes(uLen);
				return m_xbEncrypted.ChangeKey(randomPad);
			}
			else
			{
				byte[] pbData = ReadData();
				uint uLen = (uint)pbData.Length;

				byte[] randomPad = crsRandomSource.GetRandomBytes(uLen);
				Debug.Assert(randomPad.Length == uLen);

				for(uint i = 0; i < uLen; i++)
					pbData[i] ^= randomPad[i];

				return pbData;
			}
		}
	}
}
