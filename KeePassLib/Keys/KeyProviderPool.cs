/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2023 Dominik Reichl <dominik.reichl@t-online.de>

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

namespace KeePassLib.Keys
{
	public sealed class KeyProviderPool : IEnumerable<KeyProvider>
	{
		private readonly List<KeyProvider> m_l = new List<KeyProvider>();

		public int Count
		{
			get { return m_l.Count; }
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return m_l.GetEnumerator();
		}

		public IEnumerator<KeyProvider> GetEnumerator()
		{
			return m_l.GetEnumerator();
		}

		public void Add(KeyProvider kp)
		{
			if(kp == null) { Debug.Assert(false); throw new ArgumentNullException("kp"); }

			m_l.Add(kp);
		}

		public bool Remove(KeyProvider kp)
		{
			if(kp == null) { Debug.Assert(false); throw new ArgumentNullException("kp"); }

			return m_l.Remove(kp);
		}

		public KeyProvider Get(string strName)
		{
			if(strName == null) { Debug.Assert(false); throw new ArgumentNullException("strName"); }

			foreach(KeyProvider kp in m_l)
			{
				if(kp.Name == strName) return kp;
			}

			return null; // No assert
		}

		public bool IsKeyProvider(string strName)
		{
			return (Get(strName) != null);
		}
	}
}
