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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using KeePassLib.Interfaces;

namespace KeePassLib.Collections
{
	/// <summary>
	/// List of objects that implement <c>IDeepCloneable</c>
	/// and cannot be <c>null</c>.
	/// </summary>
	/// <typeparam name="T">Object type.</typeparam>
	public sealed class PwObjectList<T> : IEnumerable<T>
		where T : class, IDeepCloneable<T>
	{
		private List<T> m_l = new List<T>();

		public uint UCount
		{
			get { return (uint)m_l.Count; }
		}

		public PwObjectList()
		{
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return m_l.GetEnumerator();
		}

		public IEnumerator<T> GetEnumerator()
		{
			return m_l.GetEnumerator();
		}

		public void Clear()
		{
			m_l.Clear();
		}

		public PwObjectList<T> CloneDeep()
		{
			PwObjectList<T> l = new PwObjectList<T>();
			foreach(T o in m_l) l.Add(o.CloneDeep());
			return l;
		}

		public PwObjectList<T> CloneShallow()
		{
			PwObjectList<T> l = new PwObjectList<T>();
			l.Add(this);
			return l;
		}

		public List<T> CloneShallowToList()
		{
			return new List<T>(m_l);
		}

		public void Add(T o)
		{
			if(o == null) { Debug.Assert(false); throw new ArgumentNullException("o"); }

			m_l.Add(o);
		}

		public void Add(PwObjectList<T> l)
		{
			if(l == null) { Debug.Assert(false); throw new ArgumentNullException("l"); }

			m_l.AddRange(l.m_l);
		}

		public void Add(List<T> l)
		{
			if(l == null) { Debug.Assert(false); throw new ArgumentNullException("l"); }

			foreach(T o in l)
			{
				if(o == null) { Debug.Assert(false); throw new ArgumentOutOfRangeException("l"); }
			}

			m_l.AddRange(l);
		}

		public void Insert(uint uIndex, T o)
		{
			if(o == null) { Debug.Assert(false); throw new ArgumentNullException("o"); }

			m_l.Insert((int)uIndex, o);
		}

		public T GetAt(uint uIndex)
		{
			if(uIndex >= m_l.Count) { Debug.Assert(false); throw new ArgumentOutOfRangeException("uIndex"); }

			return m_l[(int)uIndex];
		}

		public void SetAt(uint uIndex, T o)
		{
			if(uIndex >= m_l.Count) { Debug.Assert(false); throw new ArgumentOutOfRangeException("uIndex"); }
			if(o == null) { Debug.Assert(false); throw new ArgumentNullException("o"); }

			m_l[(int)uIndex] = o;
		}

		public List<T> GetRange(uint uStartIndexIncl, uint uEndIndexIncl)
		{
			if(uStartIndexIncl >= (uint)m_l.Count)
				throw new ArgumentOutOfRangeException("uStartIndexIncl");
			if(uEndIndexIncl >= (uint)m_l.Count)
				throw new ArgumentOutOfRangeException("uEndIndexIncl");
			if(uStartIndexIncl > uEndIndexIncl)
				throw new ArgumentException();

			List<T> l = new List<T>((int)(uEndIndexIncl - uStartIndexIncl) + 1);
			for(uint u = uStartIndexIncl; u <= uEndIndexIncl; ++u)
				l.Add(m_l[(int)u]);

			return l;
		}

		public int IndexOf(T o)
		{
			if(o == null) { Debug.Assert(false); throw new ArgumentNullException("o"); }

			return m_l.IndexOf(o);
		}

		public bool Remove(T o)
		{
			if(o == null) { Debug.Assert(false); throw new ArgumentNullException("o"); }

			return m_l.Remove(o);
		}

		public void RemoveAt(uint uIndex)
		{
			m_l.RemoveAt((int)uIndex);
		}

		public void MoveOne(T o, bool bUp)
		{
			if(o == null) { Debug.Assert(false); throw new ArgumentNullException("o"); }

			int c = m_l.Count;
			if(c <= 1) return;

			int i = m_l.IndexOf(o);
			if(i < 0) { Debug.Assert(false); return; }

			if(bUp && (i != 0)) // No assert for top item
			{
				T oTemp = m_l[i - 1];
				m_l[i - 1] = m_l[i];
				m_l[i] = oTemp;
			}
			else if(!bUp && (i != (c - 1))) // No assert for bottom item
			{
				T oTemp = m_l[i + 1];
				m_l[i + 1] = m_l[i];
				m_l[i] = oTemp;
			}
		}

		public void MoveOne(T[] v, bool bUp)
		{
			if(v == null) { Debug.Assert(false); throw new ArgumentNullException("v"); }

			List<int> lIndices = new List<int>();
			foreach(T o in v)
			{
				if(o == null) { Debug.Assert(false); continue; }

				int p = m_l.IndexOf(o);
				if(p >= 0) lIndices.Add(p);
				else { Debug.Assert(false); }
			}

			MoveOne(lIndices.ToArray(), bUp);
		}

		public void MoveOne(int[] vIndices, bool bUp)
		{
			if(vIndices == null) { Debug.Assert(false); throw new ArgumentNullException("vIndices"); }

			int n = m_l.Count;
			if(n <= 1) return; // No moving possible

			int m = vIndices.Length;
			if(m == 0) return; // Nothing to move

			int[] v = new int[m];
			Array.Copy(vIndices, v, m);
			Array.Sort<int>(v);

			if((v[0] < 0) || (v[m - 1] >= n)) { Debug.Assert(false); return; }
			if((bUp && (v[0] == 0)) || (!bUp && (v[m - 1] == (n - 1))))
				return; // Moving as a block is not possible

			int iStart = (bUp ? 0 : (m - 1));
			int iExcl = (bUp ? m : -1);
			int iStep = (bUp ? 1 : -1);

			for(int i = iStart; i != iExcl; i += iStep)
			{
				int p = v[i];
				T o = m_l[p];

				if(bUp)
				{
					m_l[p] = m_l[p - 1];
					m_l[p - 1] = o;
				}
				else
				{
					m_l[p] = m_l[p + 1];
					m_l[p + 1] = o;
				}
			}
		}

		public void MoveTopBottom(T[] v, bool bTop)
		{
			if(v == null) { Debug.Assert(false); throw new ArgumentNullException("v"); }
			if(v.Length == 0) return;
			if(v.Length > m_l.Count) { Debug.Assert(false); return; }

			List<T> lMoved = new List<T>(v.Length);
			List<T> lOthers = new List<T>(m_l.Count - v.Length);
			foreach(T o in m_l)
			{
				if(Array.IndexOf(v, o) >= 0) lMoved.Add(o);
				else lOthers.Add(o);
			}
			if(lMoved.Count != v.Length) { Debug.Assert(false); return; }

			m_l = new List<T>(m_l.Count);
			m_l.AddRange(bTop ? lMoved : lOthers);
			m_l.AddRange(bTop ? lOthers : lMoved);
		}

		public void Sort(IComparer<T> tComparer)
		{
			if(tComparer == null) throw new ArgumentNullException("tComparer");

			m_l.Sort(tComparer);
		}

		public void Sort(Comparison<T> tComparison)
		{
			if(tComparison == null) throw new ArgumentNullException("tComparison");

			m_l.Sort(tComparison);
		}

		public static PwObjectList<T> FromArray(T[] v)
		{
			if(v == null) throw new ArgumentNullException("v");

			PwObjectList<T> l = new PwObjectList<T>();
			foreach(T o in v) l.Add(o);
			return l;
		}

		public static PwObjectList<T> FromList(List<T> l)
		{
			if(l == null) throw new ArgumentNullException("l");

			PwObjectList<T> lNew = new PwObjectList<T>();
			lNew.Add(l);
			return lNew;
		}
	}
}
