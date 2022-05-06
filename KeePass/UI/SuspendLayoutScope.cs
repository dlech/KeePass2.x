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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace KeePass.UI
{
	internal sealed class SuspendLayoutScope : IDisposable
	{
		private readonly List<Control> m_l = new List<Control>();
		private readonly bool m_bPerformLayoutOnDispose;

		public SuspendLayoutScope(bool bPerformLayoutOnDispose, params Control[] v)
		{
			if(v != null)
			{
				for(int i = 0; i < v.Length; ++i)
				{
					Control c = v[i];
					if(c == null) { Debug.Assert(false); continue; }

#if DEBUG
					// Nested controls should be passed from outer to inner
					string strName = c.Name;
					if(!string.IsNullOrEmpty(strName))
					{
						for(int j = i + 1; j < v.Length; ++j)
						{
							Debug.Assert((v[j] == null) || (v[j].Controls.Find(
								strName, true).Length == 0));
						}
					}
					else { Debug.Assert(c is SplitterPanel); }
#endif

					c.SuspendLayout();
					m_l.Add(c);
				}

				Debug.Assert(m_l.Count != 0);
			}
			else { Debug.Assert(false); }

			m_bPerformLayoutOnDispose = bPerformLayoutOnDispose;
		}

		public void Dispose()
		{
			Debug.Assert(m_l.Count != 0);

			foreach(Control c in m_l) c.ResumeLayout(m_bPerformLayoutOnDispose);
			m_l.Clear();
		}
	}
}
