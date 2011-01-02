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
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using KeePass.Native;

namespace KeePass.UI
{
	public sealed class CustomListViewEx : ListView
	{
		public CustomListViewEx() : base()
		{
			try { this.DoubleBuffered = true; }
			catch(Exception) { Debug.Assert(false); }
		}

		/* private Color m_clrPrev = Color.Black;
		private Point m_ptLast = new Point(-1, -1);
		protected override void OnMouseHover(EventArgs e)
		{
			if((m_ptLast.X >= 0) && (m_ptLast.X < this.Columns.Count) &&
				(m_ptLast.Y >= 0) && (m_ptLast.Y < this.Items.Count))
			{
				this.Items[m_ptLast.Y].SubItems[m_ptLast.X].ForeColor = m_clrPrev;
			}

			ListViewHitTestInfo lh = this.HitTest(this.PointToClient(Cursor.Position));
			if((lh.Item != null) && (lh.SubItem != null))
			{
				m_ptLast = new Point(lh.Item.SubItems.IndexOf(lh.SubItem),
					lh.Item.Index);
				m_clrPrev = lh.SubItem.ForeColor;

				lh.SubItem.ForeColor = Color.LightBlue;
			}

			base.OnMouseHover(e);
		} */
	}
}
