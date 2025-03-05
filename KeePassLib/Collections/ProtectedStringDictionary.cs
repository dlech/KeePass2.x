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
	/// A dictionary of <c>ProtectedString</c> objects.
	/// </summary>
	public sealed class ProtectedStringDictionary :
		IDeepCloneable<ProtectedStringDictionary>,
		IEnumerable<KeyValuePair<string, ProtectedString>>
	{
		private readonly SortedDictionary<string, ProtectedString> m_d =
			new SortedDictionary<string, ProtectedString>();

		/// <summary>
		/// Get the number of items.
		/// </summary>
		public uint UCount
		{
			get { return (uint)m_d.Count; }
		}

		/// <summary>
		/// Construct a new dictionary of protected strings.
		/// </summary>
		public ProtectedStringDictionary()
		{
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return m_d.GetEnumerator();
		}

		public IEnumerator<KeyValuePair<string, ProtectedString>> GetEnumerator()
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
		public ProtectedStringDictionary CloneDeep()
		{
			ProtectedStringDictionary d = new ProtectedStringDictionary();
			CopyTo(d);
			return d;
		}

		internal void CopyTo(ProtectedStringDictionary d)
		{
			if(d == null) { Debug.Assert(false); return; }

			foreach(KeyValuePair<string, ProtectedString> kvp in m_d)
			{
				// ProtectedString objects are immutable
				d.Set(kvp.Key, kvp.Value);
			}
		}

		[Obsolete]
		public bool EqualsDictionary(ProtectedStringDictionary d)
		{
			return EqualsDictionary(d, PwCompareOptions.None, MemProtCmpMode.None);
		}

		[Obsolete]
		public bool EqualsDictionary(ProtectedStringDictionary d,
			MemProtCmpMode mpCompare)
		{
			return EqualsDictionary(d, PwCompareOptions.None, mpCompare);
		}

		public bool EqualsDictionary(ProtectedStringDictionary d,
			PwCompareOptions pwOpt, MemProtCmpMode mpCompare)
		{
			if(d == null) { Debug.Assert(false); return false; }

			bool bNeEqStd = ((pwOpt & PwCompareOptions.NullEmptyEquivStd) !=
				PwCompareOptions.None);
			if(!bNeEqStd)
			{
				if(m_d.Count != d.m_d.Count) return false;
			}

			foreach(KeyValuePair<string, ProtectedString> kvp in m_d)
			{
				bool bStdField = PwDefs.IsStandardField(kvp.Key);
				ProtectedString ps = d.Get(kvp.Key);

				if(bNeEqStd && (ps == null) && bStdField)
					ps = ProtectedString.Empty;

				if(ps == null) return false;

				if(mpCompare == MemProtCmpMode.Full)
				{
					if(ps.IsProtected != kvp.Value.IsProtected) return false;
				}
				else if(mpCompare == MemProtCmpMode.CustomOnly)
				{
					if(!bStdField && (ps.IsProtected != kvp.Value.IsProtected))
						return false;
				}

				if(!ps.Equals(kvp.Value, false)) return false;
			}

			if(bNeEqStd)
			{
				foreach(KeyValuePair<string, ProtectedString> kvp in d.m_d)
				{
					ProtectedString ps = Get(kvp.Key);

					if(ps != null) continue; // Compared previously
					if(!PwDefs.IsStandardField(kvp.Key)) return false;
					if(!kvp.Value.IsEmpty) return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Get one of the protected strings.
		/// </summary>
		/// <param name="strName">String identifier.</param>
		/// <returns>Protected string. If the string identified by
		/// <paramref name="strName" /> cannot be found, the function
		/// returns <c>null</c>.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if the input
		/// parameter is <c>null</c>.</exception>
		public ProtectedString Get(string strName)
		{
			if(strName == null) { Debug.Assert(false); throw new ArgumentNullException("strName"); }

			ProtectedString ps;
			m_d.TryGetValue(strName, out ps);
			return ps;
		}

		/// <summary>
		/// Get one of the protected strings. The return value is never <c>null</c>.
		/// If the requested string cannot be found, an empty protected string
		/// object is returned.
		/// </summary>
		/// <param name="strName">String identifier.</param>
		/// <returns>Returns a protected string object. If the standard string
		/// has not been set yet, the return value is an empty string (<c>""</c>).</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if the input
		/// parameter is <c>null</c>.</exception>
		public ProtectedString GetSafe(string strName)
		{
			if(strName == null) { Debug.Assert(false); throw new ArgumentNullException("strName"); }

			ProtectedString ps;
			m_d.TryGetValue(strName, out ps);
			return (ps ?? ProtectedString.Empty);
		}

		/// <summary>
		/// Test if a named string exists.
		/// </summary>
		/// <param name="strName">Name of the string to try.</param>
		/// <returns>Returns <c>true</c> if the string exists, otherwise <c>false</c>.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if
		/// <paramref name="strName" /> is <c>null</c>.</exception>
		public bool Exists(string strName)
		{
			if(strName == null) { Debug.Assert(false); throw new ArgumentNullException("strName"); }

			return m_d.ContainsKey(strName);
		}

		internal bool Exists(string strName, bool? obEndsWith)
		{
			if(strName == null) { Debug.Assert(false); throw new ArgumentNullException("strName"); }

			const StringComparison sc = StringComparison.Ordinal;
			foreach(string str in m_d.Keys)
			{
				if(!obEndsWith.HasValue)
				{
					if(str.IndexOf(strName, sc) >= 0) return true;
				}
				else if(obEndsWith.Value)
				{
					if(str.EndsWith(strName, sc)) return true;
				}
				else
				{
					if(str.StartsWith(strName, sc)) return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Get one of the protected strings. If the string doesn't exist, the
		/// return value is an empty string (<c>""</c>).
		/// </summary>
		/// <param name="strName">Name of the requested string.</param>
		/// <returns>Requested string value or an empty string, if the named
		/// string doesn't exist.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if the input
		/// parameter is <c>null</c>.</exception>
		public string ReadSafe(string strName)
		{
			if(strName == null) { Debug.Assert(false); throw new ArgumentNullException("strName"); }

			ProtectedString ps;
			if(m_d.TryGetValue(strName, out ps))
				return ps.ReadString();

			return string.Empty;
		}

		/// <summary>
		/// Get one of the entry strings. If the string doesn't exist, the
		/// return value is an empty string (<c>""</c>). If the string is
		/// in-memory protected, the return value is <c>PwDefs.HiddenPassword</c>.
		/// </summary>
		/// <param name="strName">Name of the requested string.</param>
		/// <returns>Returns the requested string in plain-text or
		/// <c>PwDefs.HiddenPassword</c> if the string cannot be found.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if the input
		/// parameter is <c>null</c>.</exception>
		public string ReadSafeEx(string strName)
		{
			if(strName == null) { Debug.Assert(false); throw new ArgumentNullException("strName"); }

			ProtectedString ps;
			if(m_d.TryGetValue(strName, out ps))
			{
				if(ps.IsProtected) return PwDefs.HiddenPassword;
				return ps.ReadString();
			}

			return string.Empty;
		}

		/// <summary>
		/// Set a protected string.
		/// </summary>
		/// <param name="strName">Identifier of the string to set.</param>
		/// <param name="psNewValue">New value. This parameter must not be <c>null</c>.</param>
		/// <exception cref="System.ArgumentNullException">Thrown if any of the input
		/// parameters is <c>null</c>.</exception>
		public void Set(string strName, ProtectedString psNewValue)
		{
			if(strName == null) { Debug.Assert(false); throw new ArgumentNullException("strName"); }
			if(psNewValue == null) { Debug.Assert(false); throw new ArgumentNullException("psNewValue"); }

			m_d[strName] = psNewValue;
		}

		/// <summary>
		/// Remove a protected string.
		/// </summary>
		/// <param name="strName">Identifier of the string to remove.</param>
		/// <returns>Returns <c>true</c> if the object has been successfully
		/// removed, otherwise <c>false</c>.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if the input
		/// parameter is <c>null</c>.</exception>
		public bool Remove(string strName)
		{
			if(strName == null) { Debug.Assert(false); throw new ArgumentNullException("strName"); }

			return m_d.Remove(strName);
		}

		public List<string> GetKeys()
		{
			return new List<string>(m_d.Keys);
		}

		public void EnableProtection(string strName, bool bProtect)
		{
			ProtectedString ps = Get(strName);
			if(ps == null) return; // Nothing to do, no assert

			if(ps.IsProtected != bProtect)
				Set(strName, ps.WithProtection(bProtect));
		}
	}
}
