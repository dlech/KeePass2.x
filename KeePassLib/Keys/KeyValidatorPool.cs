/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2022 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePassLib.Utility;

namespace KeePassLib.Keys
{
	public sealed class KeyValidatorPool : IEnumerable<KeyValidator>
	{
		private readonly List<KeyValidator> m_l = new List<KeyValidator>();

		public int Count
		{
			get { return m_l.Count; }
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return m_l.GetEnumerator();
		}

		public IEnumerator<KeyValidator> GetEnumerator()
		{
			return m_l.GetEnumerator();
		}

		public void Add(KeyValidator kv)
		{
			if(kv == null) { Debug.Assert(false); throw new ArgumentNullException("kv"); }

			m_l.Add(kv);
		}

		public bool Remove(KeyValidator kv)
		{
			if(kv == null) { Debug.Assert(false); throw new ArgumentNullException("kv"); }

			return m_l.Remove(kv);
		}

		public string Validate(string strKey, KeyValidationType t)
		{
			if(strKey == null) { Debug.Assert(false); throw new ArgumentNullException("strKey"); }

			foreach(KeyValidator kv in m_l)
			{
				string strError = kv.Validate(strKey, t);
				if(strError != null) return strError;
			}

			return null;
		}

		public string Validate(byte[] pbKeyUtf8, KeyValidationType t)
		{
			if(pbKeyUtf8 == null) { Debug.Assert(false); throw new ArgumentNullException("pbKeyUtf8"); }

			if(m_l.Count == 0) return null;

			string strKey = StrUtil.Utf8.GetString(pbKeyUtf8);
			return Validate(strKey, t);
		}
	}
}
