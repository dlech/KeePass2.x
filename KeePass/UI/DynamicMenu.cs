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
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace KeePass.UI
{
	public sealed class DynamicMenuEventArgs : EventArgs
	{
		private readonly string m_strItemName;
		private readonly object m_objTag;

		public string ItemName
		{
			get { return m_strItemName; }
		}

		public object Tag
		{
			get { return m_objTag; }
		}

		public DynamicMenuEventArgs(string strItemName, object objTag)
		{
			if(strItemName == null) { Debug.Assert(false); throw new ArgumentNullException("strItemName"); }

			m_strItemName = strItemName;
			m_objTag = objTag;
		}
	}

	public sealed class DynamicMenu
	{
		private readonly ToolStripItemCollection m_tsicHost;
		private readonly List<ToolStripItem> m_lItems = new List<ToolStripItem>();

		public event EventHandler<DynamicMenuEventArgs> MenuClick;

		// Constructor required by plugins
		public DynamicMenu(ToolStripDropDownItem tsmiHost)
		{
			if(tsmiHost == null) { Debug.Assert(false); throw new ArgumentNullException("tsmiHost"); }

			m_tsicHost = tsmiHost.DropDownItems;
		}

		public DynamicMenu(ToolStripItemCollection tsicHost)
		{
			if(tsicHost == null) { Debug.Assert(false); throw new ArgumentNullException("tsicHost"); }

			m_tsicHost = tsicHost;
		}

		public void Clear()
		{
			Debug.Assert(m_lItems.TrueForAll(tsi => m_tsicHost.Contains(tsi)));

			if(m_tsicHost.Count == m_lItems.Count)
				m_tsicHost.Clear();
			else
			{
				for(int i = m_lItems.Count - 1; i >= 0; --i)
				{
					ToolStripItem tsi = m_lItems[i];

					for(int j = m_tsicHost.Count - 1; j >= 0; --j)
					{
						if(m_tsicHost[j] == tsi)
						{
							m_tsicHost.RemoveAt(j);
							break;
						}
					}
				}
			}

			Debug.Assert(m_lItems.TrueForAll(tsi => !m_tsicHost.Contains(tsi)));

			m_lItems.Clear();
		}

		public ToolStripMenuItem AddItem(string strItemText, Image imgSmallIcon)
		{
			return AddItem(strItemText, imgSmallIcon, null);
		}

		public ToolStripMenuItem AddItem(string strItemText, Image imgSmallIcon,
			object objTag)
		{
			if(strItemText == null) { Debug.Assert(false); throw new ArgumentNullException("strItemText"); }

			ToolStripMenuItem tsmi = new ToolStripMenuItem(strItemText);
			tsmi.Click += this.OnMenuClick;
			tsmi.Tag = objTag;

			if(imgSmallIcon != null) tsmi.Image = imgSmallIcon;

			m_tsicHost.Add(tsmi);
			m_lItems.Add(tsmi);
			return tsmi;
		}

		public void AddSeparator()
		{
			ToolStripSeparator s = new ToolStripSeparator();
			m_tsicHost.Add(s);
			m_lItems.Add(s);
		}

		private void OnMenuClick(object sender, EventArgs e)
		{
			ToolStripItem tsi = (sender as ToolStripItem);
			if(tsi == null) { Debug.Assert(false); return; }

			string strText = tsi.Text;
			if(strText == null) { Debug.Assert(false); return; }

			if(this.MenuClick != null)
				this.MenuClick(sender, new DynamicMenuEventArgs(strText, tsi.Tag));
		}
	}
}
