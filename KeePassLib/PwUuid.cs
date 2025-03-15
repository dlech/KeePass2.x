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
using System.Collections.Generic;
using System.Diagnostics;

using KeePassLib.Utility;

namespace KeePassLib
{
	// [ImmutableObject(true)]
	/// <summary>
	/// Represents a UUID of an object. Once created, a <c>PwUuid</c> object
	/// cannot be modified anymore (immutable).
	/// </summary>
	public sealed class PwUuid : IComparable<PwUuid>, IEquatable<PwUuid>
	{
		/// <summary>
		/// Size of a UUID in bytes.
		/// </summary>
		public const uint UuidSize = 16;

		/// <summary>
		/// Zero UUID (all bytes are zero).
		/// </summary>
		public static readonly PwUuid Zero = new PwUuid(false);

		private readonly byte[] m_pbUuid; // Never null

		/// <summary>
		/// Get the UUID bytes. Must not be modified.
		/// </summary>
		public byte[] UuidBytes
		{
			get { return m_pbUuid; }
		}

		public bool IsZero
		{
			get
			{
				if(object.ReferenceEquals(this, PwUuid.Zero)) return true;

				byte[] pb = m_pbUuid;
				for(int i = 0; i < (int)UuidSize; ++i)
				{
					if(pb[i] != 0) return false;
				}

				return true;
			}
		}

		/// <summary>
		/// Construct a UUID object.
		/// </summary>
		/// <param name="bCreateNew">If this parameter is <c>true</c>, a new
		/// UUID is generated. If it is <c>false</c>, the UUID is zero.</param>
		public PwUuid(bool bCreateNew)
		{
			if(bCreateNew)
			{
				while(true)
				{
					byte[] pb = Guid.NewGuid().ToByteArray();
					if((pb == null) || (pb.Length != (int)UuidSize))
						throw new InvalidOperationException();

					// Zero is reserved; do not generate Zero
					if(!MemUtil.ArraysEqual(pb, PwUuid.Zero.m_pbUuid))
					{
						m_pbUuid = pb;
						break;
					}
					Debug.Assert(false);
				}
			}
			else m_pbUuid = new byte[UuidSize];
		}

		/// <summary>
		/// Construct a UUID object.
		/// </summary>
		/// <param name="uuidBytes">Initial value of the <c>PwUuid</c> object.</param>
		public PwUuid(byte[] uuidBytes)
		{
			if(uuidBytes == null) { Debug.Assert(false); throw new ArgumentNullException("uuidBytes"); }
			if(uuidBytes.Length != (int)UuidSize) { Debug.Assert(false); throw new ArgumentOutOfRangeException("uuidBytes"); }

			m_pbUuid = new byte[UuidSize];
			Array.Copy(uuidBytes, m_pbUuid, (int)UuidSize);
		}

		[Obsolete]
		public bool EqualsValue(PwUuid uuid)
		{
			return Equals(uuid);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as PwUuid);
		}

		public bool Equals(PwUuid other)
		{
			if(object.ReferenceEquals(other, this)) return true;
			if(object.ReferenceEquals(other, null)) { Debug.Assert(false); return false; }

			byte[] pbX = m_pbUuid, pbY = other.m_pbUuid;
			for(int i = 0; i < (int)UuidSize; ++i)
			{
				if(pbX[i] != pbY[i]) return false;
			}

			return true;
		}

		private int m_iHash = 0x693C4762;
		public override int GetHashCode()
		{
			int h = m_iHash;
			if(h != 0x693C4762) return h;

			h = (int)MemUtil.Hash32(m_pbUuid, 0, m_pbUuid.Length);

			m_iHash = h;
			return h;
		}

		public int CompareTo(PwUuid other)
		{
			if(object.ReferenceEquals(other, this)) return 0;
			if(object.ReferenceEquals(other, null)) { Debug.Assert(false); throw new ArgumentNullException("other"); }

			byte[] pbX = m_pbUuid, pbY = other.m_pbUuid;
			for(int i = 0; i < (int)UuidSize; ++i)
			{
				byte x = pbX[i], y = pbY[i];
				if(x != y) return ((x < y) ? -1 : 1);
			}

			return 0;
		}

		/// <summary>
		/// Convert the UUID to its string representation.
		/// </summary>
		/// <returns>String containing the UUID value.</returns>
		public string ToHexString()
		{
			return MemUtil.ByteArrayToHexString(m_pbUuid);
		}

#if DEBUG
		public override string ToString()
		{
			return ToHexString();
		}
#endif
	}

	[Obsolete]
	public sealed class PwUuidComparable : IComparable<PwUuidComparable>
	{
		private readonly byte[] m_pbUuid = new byte[PwUuid.UuidSize];

		public PwUuidComparable(PwUuid pwUuid)
		{
			if(pwUuid == null) throw new ArgumentNullException("pwUuid");

			Array.Copy(pwUuid.UuidBytes, m_pbUuid, (int)PwUuid.UuidSize);
		}

		public int CompareTo(PwUuidComparable other)
		{
			if(object.ReferenceEquals(other, this)) return 0;
			if(object.ReferenceEquals(other, null)) { Debug.Assert(false); throw new ArgumentNullException("other"); }

			byte[] pbX = m_pbUuid, pbY = other.m_pbUuid;
			for(int i = 0; i < (int)PwUuid.UuidSize; ++i)
			{
				byte x = pbX[i], y = pbY[i];
				if(x != y) return ((x < y) ? -1 : 1);
			}

			return 0;
		}
	}
}
