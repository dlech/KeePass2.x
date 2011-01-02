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
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace KeePassLibSD
{
	public sealed class SortedDictionary<TKey, TValue> :
		IEnumerable<KeyValuePair<TKey, TValue>>
	{
		private Dictionary<TKey, TValue> m_dict = new Dictionary<TKey, TValue>();

		public int Count
		{
			get { return m_dict.Count; }
		}

		public TValue this[TKey pKey]
		{
			get
			{
				return m_dict[pKey];
			}

			set
			{
				m_dict[pKey] = value;
			}
		}

		public ICollection<TKey> Keys
		{
			get { return m_dict.Keys; }
		}

		public ICollection<TValue> Values
		{
			get { return m_dict.Values; }
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return m_dict.GetEnumerator();
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return m_dict.GetEnumerator();
		}

		public bool TryGetValue(TKey pKey, out TValue pValue)
		{
			return m_dict.TryGetValue(pKey, out pValue);
		}

		public void Clear()
		{
			m_dict.Clear();
		}

		public void Add(TKey pKey, TValue pValue)
		{
			m_dict.Add(pKey, pValue);
		}

		public bool Remove(TKey pKey)
		{
			return m_dict.Remove(pKey);
		}

		public bool ContainsKey(TKey pKey)
		{
			return m_dict.ContainsKey(pKey);
		}
	}
}
