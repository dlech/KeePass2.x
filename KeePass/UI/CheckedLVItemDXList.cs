/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2007 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePass.App;

using KeePassLib.Utility;

namespace KeePass.UI
{
	public sealed class CheckedLVItemDXList
	{
		private Dictionary<KeyValuePair<string, string>, ListViewItem> m_vItems =
			new Dictionary<KeyValuePair<string, string>, ListViewItem>();

		public CheckedLVItemDXList()
		{
		}

		public void UpdateData(bool bGuiToInternals)
		{
			if(bGuiToInternals)
			{
				foreach(KeyValuePair<KeyValuePair<string, string>, ListViewItem>
					kvp in m_vItems)
				{
					AppConfigEx.SetValue(kvp.Key.Key, kvp.Value.Checked);
				}
			}
			else
			{
				foreach(KeyValuePair<KeyValuePair<string, string>, ListViewItem>
					kvp in m_vItems)
				{
					kvp.Value.Checked = AppConfigEx.GetBool(kvp.Key);
				}
			}
		}

		public void AddItem(KeyValuePair<string, string> kvpItem, ListViewItem lvi)
		{
			Debug.Assert(lvi != null); if(lvi == null) return;

			m_vItems.Add(kvpItem, lvi);
		}

		public void CreateItem(KeyValuePair<string, string> kvpItem, ListView lvList,
			ListViewGroup lvgContainer, string strDisplayString)
		{
			ListViewItem lvi = new ListViewItem(strDisplayString);
			lvi.Group = lvgContainer;
			Debug.Assert(lvgContainer.Items.IndexOf(lvi) >= 0);

			Debug.Assert(lvList != null);
			if(lvList != null) lvList.Items.Add(lvi);

			AddItem(kvpItem, lvi);
		}
	}
}
