/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2026 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace KeePass.UI
{
	public sealed class CustomTabControlEx : TabControl
	{
		private ToolTip m_tt = null;
		private string m_strLastToolTip = null;
		private int m_iLastToolTipTabIndex = 0;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DefaultValue(false)]
		internal bool ShowToolTipsEx { get; set; }

		public CustomTabControlEx()
		{
			if(Program.DesignMode) return;

			m_tt = new ToolTip();
			UIUtil.ConfigureToolTip(m_tt);
		}

		protected override void Dispose(bool disposing)
		{
			if(disposing && (m_tt != null))
			{
				m_tt.Dispose();
				m_tt = null;
			}

			base.Dispose(disposing);
		}

		private void UpdateToolTipEx(Point? opt)
		{
			if(m_tt == null) return;

			try
			{
				Point pt = (opt ?? PointToClient(Cursor.Position));
				string str = null;
				int i = 0;

				if(this.ShowToolTipsEx && !this.ShowToolTips && this.Visible)
				{
					for(int j = 0; j < this.TabCount; ++j)
					{
						if(GetTabRect(j).Contains(pt))
						{
							str = this.TabPages[j].ToolTipText;
							i = j;
							break;
						}
					}
				}

				if((str != m_strLastToolTip) || (i != m_iLastToolTipTabIndex))
				{
					m_strLastToolTip = str;
					m_iLastToolTipTabIndex = i;

					if(string.IsNullOrEmpty(str)) m_tt.RemoveAll();
					else m_tt.SetToolTip(this, str);
				}
			}
			catch(Exception) { Debug.Assert(false); }
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			UpdateToolTipEx(null);

			base.OnMouseEnter(e);
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			UpdateToolTipEx(null);

			base.OnMouseLeave(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			UpdateToolTipEx(e.Location);

			base.OnMouseMove(e);
		}
	}
}
