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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using KeePass.UI;

namespace KeePass.Forms
{
	public partial class IconPickerForm : Form
	{
		private ImageList m_ilIcons = null;
		private uint m_uDefaultIcon = 0;
		private uint m_uChosenImage = 0;

		public uint ChosenImageIndex
		{
			get { return m_uChosenImage; }
		}

		public IconPickerForm()
		{
			InitializeComponent();
		}

		public void InitEx(ImageList ilIcons, uint uDefaultIcon)
		{
			Debug.Assert(ilIcons != null); if(ilIcons == null) throw new ArgumentNullException();

			m_ilIcons = ilIcons;
			m_uDefaultIcon = uDefaultIcon;
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			Debug.Assert(m_ilIcons != null); if(m_ilIcons == null) throw new ArgumentNullException();

			GlobalWindowManager.AddWindow(this);

			this.Icon = Properties.Resources.KeePass;

			m_lvIcons.SmallImageList = m_ilIcons;

			for(int i = 0; i < m_ilIcons.Images.Count; i++)
				m_lvIcons.Items.Add(i.ToString(), i);

			if(m_lvIcons.Items.Count > m_uDefaultIcon)
				m_lvIcons.Items[(int)m_uDefaultIcon].Selected = true;

			EnableControlsEx();
		}

		private void EnableControlsEx()
		{
			m_btnOK.Enabled = (m_lvIcons.SelectedItems.Count == 1);
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			ListView.SelectedIndexCollection lvsi = m_lvIcons.SelectedIndices;

			Debug.Assert(lvsi.Count == 1);
			if(lvsi.Count != 1) return;

			m_uChosenImage = (uint)lvsi[0];
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
		}

		private void OnIconsItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			EnableControlsEx();
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			GlobalWindowManager.RemoveWindow(this);
		}
	}
}