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

using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;

using KeePassLib;

namespace KeePass.Forms
{
	public partial class AutoTypeCtxForm : Form
	{
		private List<AutoTypeCtx> m_lCtxs = null;
		private ImageList m_ilIcons = null;

		private string m_strInitialFormRect = string.Empty;
		private string m_strInitialColWidths = string.Empty;

		private int m_nBannerWidth = -1;

		private AutoTypeCtx m_atcSel = null;
		public AutoTypeCtx SelectedCtx
		{
			get { return m_atcSel; }
		}

		public void InitEx(List<AutoTypeCtx> lCtxs, ImageList ilIcons)
		{
			m_lCtxs = lCtxs;
			m_ilIcons = UIUtil.CloneImageList(ilIcons, true);
		}

		public AutoTypeCtxForm()
		{
			InitializeComponent();
			Program.Translation.ApplyTo(this);
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			GlobalWindowManager.AddWindow(this);

			m_lblText.Text = KPRes.AutoTypeEntrySelectionDescLong;
			this.Text = KPRes.AutoTypeEntrySelection;
			this.Icon = Properties.Resources.KeePass;

			string strRect = Program.Config.UI.AutoTypeCtxRect;
			if(strRect.Length > 0) UIUtil.SetWindowScreenRect(this, strRect);
			m_strInitialFormRect = UIUtil.GetWindowScreenRect(this);

			this.MinimumSize = new Size(550, 300);

			UIUtil.SetExplorerTheme(m_lvItems.Handle);

			if(UISystemFonts.ListFont != null)
				m_lvItems.Font = UISystemFonts.ListFont;

			if(m_ilIcons != null) m_lvItems.SmallImageList = m_ilIcons;
			else { Debug.Assert(false); m_ilIcons = new ImageList(); }

			UIUtil.CreateEntryList(m_lvItems, m_lCtxs, m_ilIcons);

			int nWidth = m_lvItems.ClientRectangle.Width / m_lvItems.Columns.Count;
			for(int i = 0; i < m_lvItems.Columns.Count; ++i)
				m_lvItems.Columns[i].Width = nWidth;

			string strColWidths = Program.Config.UI.AutoTypeCtxColumnWidths;
			if(strColWidths.Length > 0) UIUtil.SetColumnWidths(m_lvItems, strColWidths);
			m_strInitialColWidths = UIUtil.GetColumnWidths(m_lvItems);

			ProcessResize();
			this.BringToFront();
			this.Activate();
			UIUtil.SetFocus(m_lvItems, this);
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			CleanUpEx();
			GlobalWindowManager.RemoveWindow(this);
		}

		private void CleanUpEx()
		{
			string strColWidths = UIUtil.GetColumnWidths(m_lvItems);
			if(strColWidths != m_strInitialColWidths)
				Program.Config.UI.AutoTypeCtxColumnWidths = strColWidths;

			string strRect = UIUtil.GetWindowScreenRect(this);
			if(strRect != m_strInitialFormRect)
				Program.Config.UI.AutoTypeCtxRect = strRect;

			if(m_ilIcons != null)
			{
				m_ilIcons.Dispose();
				m_ilIcons = null;
			}
		}

		private void ProcessResize()
		{
			BannerFactory.UpdateBanner(this, m_bannerImage,
				Properties.Resources.B48x48_KGPG_Key2, KPRes.AutoTypeEntrySelection,
				KPRes.AutoTypeEntrySelectionDescShort, ref m_nBannerWidth);
		}

		private bool GetSelectedEntry()
		{
			ListView.SelectedListViewItemCollection slvic = m_lvItems.SelectedItems;
			if(slvic.Count == 1)
			{
				m_atcSel = (slvic[0].Tag as AutoTypeCtx);
				return (m_atcSel != null);
			}

			return false;
		}

		private void ProcessItemSelection()
		{
			if(this.DialogResult == DialogResult.OK) return; // Already closing

			if(GetSelectedEntry()) this.DialogResult = DialogResult.OK;
		}

		private void OnListItemActivate(object sender, EventArgs e)
		{
			ProcessItemSelection();
		}

		private void OnListClick(object sender, EventArgs e)
		{
			ProcessItemSelection();
		}

		private void OnFormResize(object sender, EventArgs e)
		{
			ProcessResize();
		}
	}
}
