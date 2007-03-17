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
