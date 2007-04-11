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
using System.Xml;
using System.Diagnostics;

using KeePassLib.Utility;

namespace KeePassLib
{
	/// <summary>
	/// Represents an UUID of a password entry or group. Once created, <c>PwUUID</c>
	/// objects aren't modifyable any more (immutable).
	/// </summary>
	public sealed class PwUuid
	{
		/// <summary>
		/// Standard size in bytes of a UUID.
		/// </summary>
		public const uint UuidSize = 16;

		/// <summary>
		/// Zero UUID (all bytes are zero).
		/// </summary>
		public static readonly PwUuid Zero = new PwUuid();

		private byte[] m_pbUuid = new byte[UuidSize];

		/// <summary>
		/// Get the 16 UUID bytes.
		/// </summary>
		public byte[] UuidBytes
		{
			get { return m_pbUuid; }
		}

		/// <summary>
		/// Construct a new UUID object. Its value is initialized to zero.
		/// </summary>
		private PwUuid()
		{
			SetZero();
		}

		/// <summary>
		/// Construct a new UUID object.
		/// </summary>
		/// <param name="bCreateNew">If this parameter is <c>true</c>, a new
		/// UUID is generated. If it is <c>false</c>, the UUID is initialized
		/// to zero.</param>
		public PwUuid(bool bCreateNew)
		{
			if(bCreateNew) CreateNew();
			else SetZero();
		}

		/// <summary>
		/// Construct a new UUID object.
		/// </summary>
		/// <param name="uuidBytes">Initial value of the <c>PwUUID</c> object.</param>
		public PwUuid(byte[] uuidBytes)
		{
			Set(uuidBytes);
		}

		/// <summary>
		/// Create a new, random UUID.
		/// </summary>
		/// <returns>Returns <c>true</c> if a random UUID has been generated,
		/// otherwise it returns <c>false</c>.</returns>
		private void CreateNew()
		{
			while(true)
			{
				m_pbUuid = Guid.NewGuid().ToByteArray();

				if((m_pbUuid == null) || (m_pbUuid.Length != UuidSize))
					throw new InvalidOperationException();

				if(this.EqualsValue(PwUuid.Zero) == false)
					break;
			}
		}

		/// <summary>
		/// Compare this UUID with another.
		/// </summary>
		/// <param name="uuid">Second UUID object.</param>
		/// <returns>Returns <c>true</c> if both PwUuid object contain the same
		/// value, otherwise <c>false</c> is returned.</returns>
		public bool EqualsValue(PwUuid uuid)
		{
			Debug.Assert(uuid != null);
			if(uuid == null) throw new ArgumentNullException("uuid");

			for(int i = 0; i < UuidSize; i++)
				if(m_pbUuid[i] != uuid.m_pbUuid[i]) return false;

			return true;
		}

		/// <summary>
		/// Convert the UUID to its string representation.
		/// </summary>
		/// <returns>String containing the UUID value.</returns>
		public string ToHexString()
		{
			return MemUtil.ByteArrayToHexString(m_pbUuid);
		}

		/// <summary>
		/// Set the UUID value. The input parameter will not be modified.
		/// </summary>
		/// <param name="uuidBytes">UUID bytes. The byte array must contain
		/// exactly <c>UUIDSize</c> bytes, otherwise the function will fail.</param>
		private void Set(byte[] uuidBytes)
		{
			Debug.Assert((uuidBytes != null) && (uuidBytes.Length == UuidSize));
			if(uuidBytes == null) throw new ArgumentNullException();
			if(uuidBytes.Length != UuidSize) throw new ArgumentException();

			Array.Copy(uuidBytes, m_pbUuid, (int)UuidSize);
		}

		/// <summary>
		/// Set the UUID value to zero.
		/// </summary>
		private void SetZero()
		{
			Array.Clear(m_pbUuid, 0, (int)UuidSize);
		}
	}
}
