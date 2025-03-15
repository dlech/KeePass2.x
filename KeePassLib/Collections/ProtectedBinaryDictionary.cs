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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using KeePassLib.Interfaces;
using KeePassLib.Security;

#if KeePassLibSD
using KeePassLibSD;
#endif

namespace KeePassLib.Collections
{
	/// <summary>
	/// A dictionary of <c>ProtectedBinary</c> objects.
	/// </summary>
	public sealed class ProtectedBinaryDictionary :
		IDeepCloneable<ProtectedBinaryDictionary>,
		IEnumerable<KeyValuePair<string, ProtectedBinary>>
	{
		private readonly SortedDictionary<string, ProtectedBinary> m_d =
			new SortedDictionary<string, ProtectedBinary>();

		/// <summary>
		/// Get the number of items.
		/// </summary>
		public uint UCount
		{
			get { return (uint)m_d.Count; }
		}

		/// <summary>
		/// Construct a new dictionary of protected binaries.
		/// </summary>
		public ProtectedBinaryDictionary()
		{
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return m_d.GetEnumerator();
		}

		public IEnumerator<KeyValuePair<string, ProtectedBinary>> GetEnumerator()
		{
			return m_d.GetEnumerator();
		}

		public void Clear()
		{
			m_d.Clear();
		}

		/// <summary>
		/// Deeply clone the object.
		/// </summary>
		/// <returns>Cloned object.</returns>
		public ProtectedBinaryDictionary CloneDeep()
		{
			ProtectedBinaryDictionary d = new ProtectedBinaryDictionary();

			foreach(KeyValuePair<string, ProtectedBinary> kvp in m_d)
			{
				// ProtectedBinary objects are immutable
				d.Set(kvp.Key, kvp.Value);
			}

			return d;
		}

		public bool EqualsDictionary(ProtectedBinaryDictionary d)
		{
			if(d == null) { Debug.Assert(false); return false; }

			if(m_d.Count != d.m_d.Count) return false;

			foreach(KeyValuePair<string, ProtectedBinary> kvp in m_d)
			{
				ProtectedBinary pb = d.Get(kvp.Key);
				if(pb == null) return false;
				if(!pb.Equals(kvp.Value)) return false;
			}

			return true;
		}

		/// <summary>
		/// Get one of the protected binaries.
		/// </summary>
		/// <param name="strName">Binary identifier.</param>
		/// <returns>Protected binary. If the binary identified by
		/// <paramref name="strName" /> cannot be found, the function
		/// returns <c>null</c>.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if the input
		/// parameter is <c>null</c>.</exception>
		public ProtectedBinary Get(string strName)
		{
			if(strName == null) { Debug.Assert(false); throw new ArgumentNullException("strName"); }

			ProtectedBinary pb;
			m_d.TryGetValue(strName, out pb);
			return pb;
		}

		/// <summary>
		/// Set a protected binary.
		/// </summary>
		/// <param name="strName">Identifier of the binary to set.</param>
		/// <param name="pbNewValue">New value. This parameter must not be <c>null</c>.</param>
		/// <exception cref="System.ArgumentNullException">Thrown if any of the input
		/// parameters is <c>null</c>.</exception>
		public void Set(string strName, ProtectedBinary pbNewValue)
		{
			if(strName == null) { Debug.Assert(false); throw new ArgumentNullException("strName"); }
			if(pbNewValue == null) { Debug.Assert(false); throw new ArgumentNullException("pbNewValue"); }

			m_d[strName] = pbNewValue;
		}

		/// <summary>
		/// Remove a protected binary.
		/// </summary>
		/// <param name="strName">Identifier of the binary to remove.</param>
		/// <returns>Returns <c>true</c> if the object has been successfully
		/// removed, otherwise <c>false</c>.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if the input
		/// parameter is <c>null</c>.</exception>
		public bool Remove(string strName)
		{
			if(strName == null) { Debug.Assert(false); throw new ArgumentNullException("strName"); }

			return m_d.Remove(strName);
		}

		public string KeysToString()
		{
			if(m_d.Count == 0) return string.Empty;

			StringBuilder sb = new StringBuilder();
			foreach(KeyValuePair<string, ProtectedBinary> kvp in m_d)
			{
				if(sb.Length > 0) sb.Append(", ");
				sb.Append(kvp.Key);
			}

			return sb.ToString();
		}
	}
}
