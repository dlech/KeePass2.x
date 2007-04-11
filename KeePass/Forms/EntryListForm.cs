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
using KeePass.Resources;

using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class EntryListForm : Form
	{
		private string m_strTitle = string.Empty;
		private string m_strDescShort = string.Empty;
		private string m_strDescLong = string.Empty;
		private Image m_imgIcon = null;
		private ImageList m_ilIcons = null;
		private PwObjectList<PwEntry> m_vEntries = null;
		private PwEntry m_peSelected = null;
		private bool m_bEnsureForeground = false;

		public PwEntry SelectedEntry
		{
			get { return m_peSelected; }
		}

		public bool EnsureForeground
		{
			get { return m_bEnsureForeground; }
			set { m_bEnsureForeground = value; }
		}

		public void InitEx(string strTitle, string strDescShort, string strDescLong, Image imgIcon, ImageList ilIcons, PwObjectList<PwEntry> vEntries)
		{
			m_strTitle = strTitle;
			m_strDescShort = strDescShort;
			m_strDescLong = strDescLong;
			m_imgIcon = imgIcon;
			m_ilIcons = ilIcons;
			m_vEntries = vEntries;
		}

		public EntryListForm()
		{
			InitializeComponent();
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			Debug.Assert(m_strTitle.Length > 0);
			Debug.Assert(m_imgIcon != null);

			GlobalWindowManager.AddWindow(this);

			m_bannerImage.Image = BannerFactory.CreateBanner(m_bannerImage.Width,
				m_bannerImage.Height, BannerFactory.BannerStyle.Default,
				m_imgIcon, m_strTitle, m_strDescShort);
			m_lblText.Text = m_strDescLong;
			this.Text = m_strTitle;
			this.Icon = Properties.Resources.KeePass;

			if(m_ilIcons != null) m_lvEntries.SmallImageList = m_ilIcons;

			m_lvEntries.Columns.Add(KPRes.Title);
			m_lvEntries.Columns.Add(KPRes.UserName);
			m_lvEntries.Columns.Add(KPRes.URL);

			FillEntriesList();
			ProcessResize();

			if(m_bEnsureForeground)
			{
				this.BringToFront();
				this.Activate();
			}

			EnableControlsEx();
		}

		private void EnableControlsEx()
		{
			m_btnOK.Enabled = (m_lvEntries.SelectedIndices.Count > 0);
		}

		private void ProcessResize()
		{
			int nWidth = m_lvEntries.ClientRectangle.Width / m_lvEntries.Columns.Count;
			for(int i = 0; i < m_lvEntries.Columns.Count; ++i)
				m_lvEntries.Columns[i].Width = nWidth;
		}

		private void FillEntriesList()
		{
			if(m_vEntries == null) { Debug.Assert(false); return; }

			ListViewGroup lvg = new ListViewGroup(Guid.NewGuid().ToString());
			DateTime dtNow = DateTime.Now;
			bool bFirstEntry = true;

			foreach(PwEntry pe in m_vEntries)
			{
				if(pe.ParentGroup != null)
				{
					string strGroup = pe.ParentGroup.GetFullPath();

					if(strGroup != lvg.Header)
					{
						lvg = new ListViewGroup(strGroup, HorizontalAlignment.Left);
						m_lvEntries.Groups.Add(lvg);
					}
				}

				ListViewItem lvi = new ListViewItem(pe.Strings.ReadSafe(PwDefs.TitleField));

				if(pe.Expires && (pe.ExpiryTime <= dtNow))
					lvi.ImageIndex = (int)PwIcon.Expired;
				else lvi.ImageIndex = (int)pe.IconID;

				lvi.SubItems.Add(pe.Strings.ReadSafe(PwDefs.UserNameField));
				lvi.SubItems.Add(pe.Strings.ReadSafe(PwDefs.UrlField));

				lvi.Tag = pe;

				m_lvEntries.Items.Add(lvi);
				lvg.Items.Add(lvi);

				if(bFirstEntry) lvi.Selected = true;
				bFirstEntry = false;
			}
		}

		private bool GetSelectedEntry(bool bSetDialogResult)
		{
			ListView.SelectedListViewItemCollection slvic = m_lvEntries.SelectedItems;
			if(slvic.Count == 1)
			{
				m_peSelected = slvic[0].Tag as PwEntry;

				if(bSetDialogResult) this.DialogResult = DialogResult.OK;
				return true;
			}

			return false;
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			if(GetSelectedEntry(false) == false)
				this.DialogResult = DialogResult.None;
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
		}

		private void OnEntriesItemActivate(object sender, EventArgs e)
		{
			if(GetSelectedEntry(true))
				m_lvEntries.Enabled = false;
		}

		private void OnEntriesSelectedIndexChanged(object sender, EventArgs e)
		{
			EnableControlsEx();
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			GlobalWindowManager.RemoveWindow(this);
		}
	}
}
