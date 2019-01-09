/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2019 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using KeePassLib.Translation;

namespace KeePass.UI
{
	public sealed class RtlAwareResizeScope : IDisposable
	{
		private List<Control> m_lControls = new List<Control>();
		private List<int> m_lOrgX = new List<int>();
		private List<int> m_lOrgW = new List<int>();

		public RtlAwareResizeScope(params Control[] v)
		{
			if(v != null)
			{
				foreach(Control c in v)
				{
					if(c == null) { Debug.Assert(false); continue; }

					try
					{
						if((c.RightToLeft == RightToLeft.Yes) &&
							KPTranslation.IsRtlMoveChildsRequired(c.Parent))
						{
							int x = c.Location.X;
							int w = c.Width;

							m_lControls.Add(c);
							m_lOrgX.Add(x);
							m_lOrgW.Add(w);
						}
					}
					catch(Exception) { Debug.Assert(false); }
				}
			}
			else { Debug.Assert(false); }
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool bDisposing)
		{
			if(!bDisposing) { Debug.Assert(false); return; }

			for(int i = 0; i < m_lControls.Count; ++i)
			{
				try
				{
					Control c = m_lControls[i];
					int xOrg = m_lOrgX[i];
					int wOrg = m_lOrgW[i];
					Point ptNew = c.Location;
					int wNew = c.Width;

					Debug.Assert(c.RightToLeft == RightToLeft.Yes);
					if((wNew != wOrg) && (ptNew.X == xOrg))
						c.Location = new Point(xOrg + wOrg - wNew, ptNew.Y);
				}
				catch(Exception) { Debug.Assert(false); }
			}

			m_lControls.Clear();
			m_lOrgX.Clear();
			m_lOrgW.Clear();
		}
	}
}
