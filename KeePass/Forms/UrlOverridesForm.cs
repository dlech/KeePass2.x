/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2025 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePass.App;
using KeePass.App.Configuration;
using KeePass.Resources;
using KeePass.UI;

namespace KeePass.Forms
{
	public partial class UrlOverridesForm : Form
	{
		private AceUrlSchemeOverrides m_uso = null;

		[Obsolete]
		public string UrlOverrideAll
		{
			get { return Program.Config.Integration.UrlOverride; }
		}

		[Obsolete]
		public void InitEx(AceUrlSchemeOverrides aceOvr, string strOverrideAll)
		{
		}

		public UrlOverridesForm()
		{
			InitializeComponent();
			GlobalWindowManager.InitializeForm(this);
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			AceIntegration aceInt = Program.Config.Integration;
			m_uso = aceInt.UrlSchemeOverrides.CloneDeep();

			GlobalWindowManager.AddWindow(this);

			this.Icon = AppIcons.Default;
			this.Text = KPRes.UrlOverrides;

			UIUtil.SetExplorerTheme(m_lvOverrides, false);

			int nWidth = m_lvOverrides.ClientSize.Width - UIUtil.GetVScrollBarWidth();
			m_lvOverrides.Columns.Add(KPRes.Scheme, nWidth / 4);
			m_lvOverrides.Columns.Add(KPRes.UrlOverride, (nWidth * 3) / 4);

			UpdateOverridesList(false, false);

			string str = aceInt.UrlOverride;
			m_cbOverrideAll.Checked = (aceInt.UrlOverrideEnabled &&
				!string.IsNullOrEmpty(str));
			m_tbOverrideAll.Text = (str ?? string.Empty);

			AccessibilityEx.SetContext(m_tbOverrideAll, m_cbOverrideAll);

			Debug.Assert(m_btnOK.FlatStyle == m_btnCancel.FlatStyle);
			UIUtil.SetShield(m_btnOK, true);

			EnableControlsEx();
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			GlobalWindowManager.RemoveWindow(this);
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			AppConfigEx cfg = new AppConfigEx();
			AceIntegration aceInt = cfg.Integration;

			aceInt.UrlOverride = m_tbOverrideAll.Text;
			aceInt.UrlOverrideEnabled = (m_cbOverrideAll.Checked &&
				!string.IsNullOrEmpty(aceInt.UrlOverride));
			aceInt.UrlSchemeOverrides = m_uso;

			if(!AppConfigEx.EnforceSections(AceSections.UrlOverride |
				AceSections.UrlSchemeOverrides, cfg, true, true, this, sender))
				this.DialogResult = DialogResult.None;
		}

		private void UpdateOverridesList(bool bRestoreView, bool bUpdateState)
		{
			UIScrollInfo si = (bRestoreView ? UIUtil.GetScrollInfo(
				m_lvOverrides, true) : null);

			m_lvOverrides.BeginUpdate();
			m_lvOverrides.Items.Clear();
			m_lvOverrides.Groups.Clear();

			for(int i = 0; i < 2; ++i)
			{
				List<AceUrlSchemeOverride> l = ((i == 0) ?
					m_uso.BuiltInOverrides : m_uso.CustomOverrides);

				ListViewGroup lvg = new ListViewGroup((i == 0) ?
					KPRes.OverridesBuiltIn : KPRes.OverridesCustom);
				m_lvOverrides.Groups.Add(lvg);

				foreach(AceUrlSchemeOverride o in l)
				{
					ListViewItem lvi = new ListViewItem(o.Scheme);
					lvi.SubItems.Add(o.UrlOverride);
					lvi.Tag = o;
					lvi.Checked = o.Enabled;

					m_lvOverrides.Items.Add(lvi);
					lvg.Items.Add(lvi);
				}
			}

			if(bRestoreView) UIUtil.Scroll(m_lvOverrides, si, false);

			m_lvOverrides.EndUpdate();

			if(bUpdateState) EnableControlsEx();
		}

		private void OnOverridesItemChecked(object sender, ItemCheckedEventArgs e)
		{
			AceUrlSchemeOverride o = (e.Item.Tag as AceUrlSchemeOverride);
			if(o == null) { Debug.Assert(false); return; }

			o.Enabled = e.Item.Checked;
		}

		private void EnableControlsEx()
		{
			ListView.SelectedListViewItemCollection lvsc = m_lvOverrides.SelectedItems;
			int cSel = lvsc.Count;

			bool bBuiltIn = false;
			foreach(ListViewItem lvi in lvsc)
			{
				AceUrlSchemeOverride o = (lvi.Tag as AceUrlSchemeOverride);
				if(o == null) { Debug.Assert(false); continue; }
				if(o.IsBuiltIn) { bBuiltIn = true; break; }
			}

			m_btnEdit.Enabled = ((cSel == 1) && !bBuiltIn);
			m_btnDelete.Enabled = ((cSel >= 1) && !bBuiltIn);

			m_tbOverrideAll.Enabled = m_cbOverrideAll.Checked;
		}

		private void OnOverridesSelectedIndexChanged(object sender, EventArgs e)
		{
			EnableControlsEx();
		}

		private void OnBtnAdd(object sender, EventArgs e)
		{
			AceUrlSchemeOverride o = new AceUrlSchemeOverride(true, string.Empty,
				string.Empty);

			UrlOverrideForm dlg = new UrlOverrideForm();
			dlg.InitEx(o);
			if(UIUtil.ShowDialogAndDestroy(dlg) == DialogResult.OK)
			{
				m_uso.CustomOverrides.Add(o);
				UpdateOverridesList(true, true);
				// m_lvOverrides.EnsureVisible(m_lvOverrides.Items.Count - 1);
			}
		}

		private void OnBtnEdit(object sender, EventArgs e)
		{
			ListView.SelectedListViewItemCollection lvsc = m_lvOverrides.SelectedItems;
			if((lvsc == null) || (lvsc.Count != 1)) { Debug.Assert(false); return; }

			AceUrlSchemeOverride o = (lvsc[0].Tag as AceUrlSchemeOverride);
			if((o == null) || o.IsBuiltIn) { Debug.Assert(false); return; }

			UrlOverrideForm dlg = new UrlOverrideForm();
			dlg.InitEx(o);
			if(UIUtil.ShowDialogAndDestroy(dlg) == DialogResult.OK)
				UpdateOverridesList(true, true);
		}

		private void OnBtnDelete(object sender, EventArgs e)
		{
			ListView.SelectedListViewItemCollection lvsc = m_lvOverrides.SelectedItems;
			if((lvsc == null) || (lvsc.Count == 0)) { Debug.Assert(false); return; }

			foreach(ListViewItem lvi in lvsc)
			{
				AceUrlSchemeOverride o = (lvi.Tag as AceUrlSchemeOverride);
				if((o == null) || o.IsBuiltIn) { Debug.Assert(false); continue; }

				m_uso.CustomOverrides.Remove(o);
			}

			UpdateOverridesList(true, true);
		}

		private void OnOverrideAllCheckedChanged(object sender, EventArgs e)
		{
			EnableControlsEx();
		}
	}
}
