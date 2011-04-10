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
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using KeePass.App.Configuration;
using KeePass.Resources;
using KeePass.UI;

namespace KeePass.Forms
{
	public partial class UrlSchemesForm : Form
	{
		private AceUrlSchemeOverrides m_aceOvr = null;
		private AceUrlSchemeOverrides m_aceTmp = null;

		public void InitEx(AceUrlSchemeOverrides aceOvr)
		{
			m_aceOvr = aceOvr;
		}

		public UrlSchemesForm()
		{
			InitializeComponent();
			Program.Translation.ApplyTo(this);
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			if(m_aceOvr == null) throw new InvalidOperationException();
			m_aceTmp = m_aceOvr.CloneDeep();

			GlobalWindowManager.AddWindow(this);

			this.Icon = Properties.Resources.KeePass;
			this.Text = KPRes.UrlSchemeOverrides;

			int nWidth = (m_lvOverrides.ClientSize.Width - UIUtil.GetVScrollBarWidth()) / 4;
			m_lvOverrides.Columns.Add(KPRes.Scheme, nWidth);
			m_lvOverrides.Columns.Add(KPRes.UrlOverride, nWidth * 3 - 1);

			UpdateOverridesList();
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			GlobalWindowManager.RemoveWindow(this);
		}

		private void UpdateOverridesList()
		{
			m_lvOverrides.BeginUpdate();
			m_lvOverrides.Items.Clear();

			foreach(AceUrlSchemeOverride ovr in m_aceTmp.Overrides)
			{
				ListViewItem lvi = new ListViewItem(ovr.Scheme);
				lvi.SubItems.Add(ovr.UrlOverride);
				lvi.Tag = ovr; // Set before setting the Checked property

				lvi.Checked = ovr.Enabled;

				m_lvOverrides.Items.Add(lvi);
			}

			m_lvOverrides.EndUpdate();
			EnableControlsEx();
		}

		private void OnOverridesItemChecked(object sender, ItemCheckedEventArgs e)
		{
			AceUrlSchemeOverride ovr = (e.Item.Tag as AceUrlSchemeOverride);
			if(ovr == null) { Debug.Assert(false); return; }

			ovr.Enabled = e.Item.Checked;
		}

		private void EnableControlsEx()
		{
			m_btnEdit.Enabled = (m_lvOverrides.SelectedIndices.Count == 1);
			m_btnDelete.Enabled = (m_lvOverrides.SelectedIndices.Count >= 1);
		}

		private void OnOverridesSelectedIndexChanged(object sender, EventArgs e)
		{
			EnableControlsEx();
		}

		private void OnBtnAdd(object sender, EventArgs e)
		{
			AceUrlSchemeOverride ovr = new AceUrlSchemeOverride(true, string.Empty,
				string.Empty);

			UrlSchemeForm dlg = new UrlSchemeForm();
			dlg.InitEx(ovr);
			if(UIUtil.ShowDialogAndDestroy(dlg) == DialogResult.OK)
			{
				m_aceTmp.Overrides.Add(ovr);
				UpdateOverridesList();
				m_lvOverrides.EnsureVisible(m_lvOverrides.Items.Count - 1);
			}
		}

		private void OnBtnEdit(object sender, EventArgs e)
		{
			ListView.SelectedListViewItemCollection lvsic = m_lvOverrides.SelectedItems;
			if((lvsic == null) || (lvsic.Count != 1)) return;

			AceUrlSchemeOverride ovr = (lvsic[0].Tag as AceUrlSchemeOverride);
			if(ovr == null) { Debug.Assert(false); return; }

			UrlSchemeForm dlg = new UrlSchemeForm();
			dlg.InitEx(ovr);
			if(UIUtil.ShowDialogAndDestroy(dlg) == DialogResult.OK) UpdateOverridesList();
		}

		private void OnBtnDelete(object sender, EventArgs e)
		{
			ListView.SelectedListViewItemCollection lvsic = m_lvOverrides.SelectedItems;
			if((lvsic == null) || (lvsic.Count == 0)) return;

			foreach(ListViewItem lvi in lvsic)
			{
				AceUrlSchemeOverride ovr = (lvi.Tag as AceUrlSchemeOverride);
				if(ovr == null) { Debug.Assert(false); continue; }

				m_aceTmp.Overrides.Remove(ovr);
			}

			UpdateOverridesList();
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			m_aceOvr.Overrides = m_aceTmp.Overrides;
		}
	}
}
