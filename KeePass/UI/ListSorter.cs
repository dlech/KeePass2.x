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
using System.Collections;
using System.Windows.Forms;
using System.Diagnostics;

namespace KeePass.UI
{
	public sealed class ListSorter : IComparer
	{
		private int m_nColumn = -1;
		private SortOrder m_oSort = SortOrder.Ascending;

		public int Column
		{
			get { return m_nColumn; }
		}

		public SortOrder Order
		{
			get { return m_oSort; }
		}

		public ListSorter(int nColumn, SortOrder sortOrder)
		{
			m_nColumn = nColumn;

			Debug.Assert(sortOrder != SortOrder.None);
			if(sortOrder != SortOrder.None) m_oSort = sortOrder;
		}

		public int Compare(object x, object y)
		{
			ListViewItem lviX = (ListViewItem)x;
			ListViewItem lviY = (ListViewItem)y;

			if(lviY.SubItems.Count <= m_nColumn)
			{
				if(m_oSort == SortOrder.Ascending)
					return lviX.Text.CompareTo(lviY.Text);
				else
					return lviY.Text.CompareTo(lviX.Text);
			}

			if(m_oSort == SortOrder.Ascending)
			{
				if(m_nColumn <= 0) return lviX.Text.CompareTo(lviY.Text);
				return lviX.SubItems[m_nColumn].Text.CompareTo(lviY.SubItems[m_nColumn].Text);
			}

			if(m_nColumn <= 0) return lviY.Text.CompareTo(lviX.Text);
			return lviY.SubItems[m_nColumn].Text.CompareTo(lviX.SubItems[m_nColumn].Text);
		}
	}
}
