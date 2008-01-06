/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2008 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Reflection;

using KeePass.App;

using KeePassLib.Utility;

namespace KeePass.UI
{
	public sealed class CheckedLVItemDXList
	{
		private List<object> m_vObjects = new List<object>();
		private List<PropertyInfo> m_vProperties = new List<PropertyInfo>();
		private List<ListViewItem> m_vListViewItems = new List<ListViewItem>();

		public CheckedLVItemDXList()
		{
		}

		public void UpdateData(bool bGuiToInternals)
		{
			for(int i = 0; i < m_vObjects.Count; ++i)
			{
				object o = m_vObjects[i];

				if(bGuiToInternals)
				{
					bool bChecked = m_vListViewItems[i].Checked;
					m_vProperties[i].SetValue(o, bChecked, null);
				}
				else // Internals to GUI
				{
					bool bValue = (bool)m_vProperties[i].GetValue(o, null);
					m_vListViewItems[i].Checked = bValue;
				}
			}
		}

		public ListViewItem CreateItem(object pContainer, string strPropertyName,
			ListView lvList, ListViewGroup lvgContainer, string strDisplayString)
		{
			if(pContainer == null) throw new ArgumentNullException("pContainer");
			if(strPropertyName == null) throw new ArgumentNullException("strPropertyName");
			if(strPropertyName.Length == 0) throw new ArgumentException();
			if(lvList == null) throw new ArgumentNullException("lvList");
			if(strDisplayString == null) throw new ArgumentNullException("strDisplayString");

			Type t = pContainer.GetType();
			PropertyInfo pi = t.GetProperty(strPropertyName);
			if((pi == null) || (pi.PropertyType != typeof(bool)) ||
				(pi.CanRead == false) || (pi.CanWrite == false))
				throw new ArgumentException();

			ListViewItem lvi = new ListViewItem(strDisplayString);
			if(lvgContainer != null)
			{
				lvi.Group = lvgContainer;
				Debug.Assert(lvgContainer.Items.IndexOf(lvi) >= 0);
			}

			lvList.Items.Add(lvi);

			m_vObjects.Add(pContainer);
			m_vProperties.Add(pi);
			m_vListViewItems.Add(lvi);

			return lvi;
		}
	}
}
