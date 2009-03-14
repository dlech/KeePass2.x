/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2009 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Drawing;
using System.Diagnostics;

namespace KeePass.UI
{
	public sealed class DynamicMenuEventArgs : EventArgs
	{
		private string m_strItemName = string.Empty;

		public string ItemName
		{
			get { return m_strItemName; }
		}

		public DynamicMenuEventArgs(string strItemName)
		{
			Debug.Assert(strItemName != null);
			if(strItemName == null) throw new ArgumentNullException("strItemName");

			m_strItemName = strItemName;
		}
	}

	public sealed class DynamicMenu
	{
		private ToolStripMenuItem m_tsmiHost;
		private List<ToolStripItem> m_vMenuItems = new List<ToolStripItem>();

		public event EventHandler<DynamicMenuEventArgs> MenuClick;

		public DynamicMenu(ToolStripMenuItem tsmiHost)
		{
			Debug.Assert(tsmiHost != null);
			if(tsmiHost == null) throw new ArgumentNullException("tsmiHost");

			m_tsmiHost = tsmiHost;
		}

		~DynamicMenu()
		{
			this.Clear();
		}

		public void Clear()
		{
			foreach(ToolStripItem tsi in m_vMenuItems)
			{
				if(tsi is ToolStripMenuItem)
					tsi.Click -= this.OnMenuClick;

				m_tsmiHost.DropDownItems.Remove(tsi);
			}
			m_vMenuItems.Clear();
		}

		public void AddItem(string strItemText, Image imgSmallIcon)
		{
			Debug.Assert(strItemText != null);
			if(strItemText == null) throw new ArgumentNullException("strItemText");

			ToolStripMenuItem tsmi = new ToolStripMenuItem(strItemText);
			tsmi.Click += this.OnMenuClick;

			if(imgSmallIcon != null) tsmi.Image = imgSmallIcon;

			m_tsmiHost.DropDownItems.Add(tsmi);
			m_vMenuItems.Add(tsmi);
		}

		public void AddSeparator()
		{
			ToolStripSeparator sep = new ToolStripSeparator();

			m_tsmiHost.DropDownItems.Add(sep);
			m_vMenuItems.Add(sep);
		}

		private void OnMenuClick(object sender, EventArgs e)
		{
			ToolStripItem tsi = sender as ToolStripItem;
			Debug.Assert(tsi != null); if(tsi == null) return;

			string strText = tsi.Text;
			Debug.Assert(strText != null); if(strText == null) return;

			DynamicMenuEventArgs args = new DynamicMenuEventArgs(strText);
			if(this.MenuClick != null) this.MenuClick(sender, args);
		}
	}
}
