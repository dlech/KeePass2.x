/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2012 Dominik Reichl <dominik.reichl@t-online.de>

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
	public enum CheckItemLinkType
	{
		None = 0,
		CheckedChecked,
		UncheckedUnchecked,
		CheckedUnchecked,
		UncheckedChecked
	}

	public sealed class CheckedLVItemDXList
	{
		private ListView m_lv;

		private List<object> m_vObjects = new List<object>();
		private List<PropertyInfo> m_vProperties = new List<PropertyInfo>();
		private List<ListViewItem> m_vListViewItems = new List<ListViewItem>();

		private List<CheckItemLink> m_vLinks = new List<CheckItemLink>();

		private sealed class CheckItemLink
		{
			private ListViewItem m_lvSource;
			public ListViewItem Source { get { return m_lvSource; } }

			private ListViewItem m_lvTarget;
			public ListViewItem Target { get { return m_lvTarget; } }

			private CheckItemLinkType m_t;
			public CheckItemLinkType Type { get { return m_t; } }

			public CheckItemLink(ListViewItem lviSource, ListViewItem lviTarget,
				CheckItemLinkType cilType)
			{
				m_lvSource = lviSource;
				m_lvTarget = lviTarget;
				m_t = cilType;
			}
		}

		public CheckedLVItemDXList(ListView lv)
		{
			if(lv == null) throw new ArgumentNullException("lv");

			m_lv = lv;
			m_lv.ItemChecked += this.OnItemCheckedChanged;
		}

#if DEBUG
		~CheckedLVItemDXList()
		{
			Debug.Assert(m_lv == null); // Release should have been called
		}
#endif

		public void Release()
		{
			if(m_lv == null) { Debug.Assert(false); return; }

			m_vObjects.Clear();
			m_vProperties.Clear();
			m_vListViewItems.Clear();
			m_vLinks.Clear();

			m_lv.ItemChecked -= this.OnItemCheckedChanged;
			m_lv = null;
		}

		public void UpdateData(bool bGuiToInternals)
		{
			if(m_lv == null) { Debug.Assert(false); return; }

			for(int i = 0; i < m_vObjects.Count; ++i)
			{
				object o = m_vObjects[i];

				Debug.Assert(m_vListViewItems[i].Index >= 0);
				Debug.Assert(m_lv.Items.IndexOf(m_vListViewItems[i]) >= 0);
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
			ListViewGroup lvgContainer, string strDisplayString)
		{
			if(pContainer == null) throw new ArgumentNullException("pContainer");
			if(strPropertyName == null) throw new ArgumentNullException("strPropertyName");
			if(strPropertyName.Length == 0) throw new ArgumentException();
			if(strDisplayString == null) throw new ArgumentNullException("strDisplayString");

			if(m_lv == null) { Debug.Assert(false); return null; }

			Type t = pContainer.GetType();
			PropertyInfo pi = t.GetProperty(strPropertyName);
			if((pi == null) || (pi.PropertyType != typeof(bool)) ||
				!pi.CanRead || !pi.CanWrite)
				throw new ArgumentException();

			ListViewItem lvi = new ListViewItem(strDisplayString);
			if(lvgContainer != null)
			{
				lvi.Group = lvgContainer;
				Debug.Assert(lvgContainer.Items.IndexOf(lvi) >= 0);
				Debug.Assert(m_lv.Groups.IndexOf(lvgContainer) >= 0);
			}

			m_lv.Items.Add(lvi);

			m_vObjects.Add(pContainer);
			m_vProperties.Add(pi);
			m_vListViewItems.Add(lvi);

			return lvi;
		}

		public void AddLink(ListViewItem lviSource, ListViewItem lviTarget,
			CheckItemLinkType t)
		{
			if(lviSource == null) { Debug.Assert(false); return; }
			if(lviTarget == null) { Debug.Assert(false); return; }

			if(m_lv == null) { Debug.Assert(false); return; }

			Debug.Assert(m_vListViewItems.IndexOf(lviSource) >= 0);
			Debug.Assert(m_vListViewItems.IndexOf(lviTarget) >= 0);

			m_vLinks.Add(new CheckItemLink(lviSource, lviTarget, t));
		}

		private void OnItemCheckedChanged(object sender, ItemCheckedEventArgs e)
		{
			ListViewItem lvi = e.Item;
			if(lvi == null) { Debug.Assert(false); return; }

			bool bChecked = lvi.Checked;

			// Debug.Assert(m_vListViewItems.IndexOf(lvi) >= 0);
			foreach(CheckItemLink cl in m_vLinks)
			{
				if(cl.Source == lvi)
				{
					if(cl.Target.Index < 0) continue;

					if((cl.Type == CheckItemLinkType.CheckedChecked) &&
						bChecked && !cl.Target.Checked)
						cl.Target.Checked = true;
					else if((cl.Type == CheckItemLinkType.UncheckedUnchecked) &&
						!bChecked && cl.Target.Checked)
						cl.Target.Checked = false;
					else if((cl.Type == CheckItemLinkType.CheckedUnchecked) &&
						bChecked && cl.Target.Checked)
						cl.Target.Checked = false;
					else if((cl.Type == CheckItemLinkType.UncheckedChecked) &&
						!bChecked && !cl.Target.Checked)
						cl.Target.Checked = true;
				}
			}
		}
	}
}
