/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2019 Dominik Reichl <dominik.reichl@t-online.de>

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
using KeePass.Native;
using KeePass.Resources;
using KeePass.UI;

using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	/* public sealed class LvfCommand
	{
		private readonly string m_strText;
		public string Text
		{
			get { return m_strText; }
		}

		private readonly Action<ListView> m_fAction;
		public Action<ListView> Action
		{
			get { return m_fAction; }
		}

		public LvfCommand(string strText, Action<ListView> fAction)
		{
			if(strText == null) throw new ArgumentNullException("strText");
			if(fAction == null) throw new ArgumentNullException("fAction");

			m_strText = strText;
			m_fAction = fAction;
		}
	} */

	public partial class ListViewForm : Form
	{
		private string m_strTitle = string.Empty;
		private string m_strSubTitle = string.Empty;
		private string m_strInfo = string.Empty;
		private Image m_imgIcon = null;
		private List<object> m_lData = new List<object>();
		private ImageList m_ilIcons = null;
		private Action<ListView> m_fInit = null;

		private object m_resItem = null;
		public object ResultItem
		{
			get { return m_resItem; }
		}

		private object m_resGroup = null;
		public object ResultGroup
		{
			get { return m_resGroup; }
		}

		private bool m_bEnsureForeground = false;
		public bool EnsureForeground
		{
			get { return m_bEnsureForeground; }
			set { m_bEnsureForeground = value; }
		}

		public void InitEx(string strTitle, string strSubTitle,
			string strInfo, Image imgIcon, List<object> lData,
			ImageList ilIcons, Action<ListView> fInit)
		{
			m_strTitle = (strTitle ?? string.Empty);
			m_strSubTitle = (strSubTitle ?? string.Empty);
			m_strInfo = (strInfo ?? string.Empty);
			m_imgIcon = imgIcon;
			if(lData != null) m_lData = lData;

			if(ilIcons != null)
				m_ilIcons = UIUtil.CloneImageList(ilIcons, true);

			m_fInit = fInit;
		}

		public ListViewForm()
		{
			InitializeComponent();
			Program.Translation.ApplyTo(this);
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			GlobalWindowManager.AddWindow(this);

			this.Text = m_strTitle;
			this.Icon = AppIcons.Default;

			BannerFactory.CreateBannerEx(this, m_bannerImage,
				m_imgIcon, m_strTitle, m_strSubTitle);

			Debug.Assert(!m_lblInfo.AutoSize); // For RTL support
			m_lblInfo.Text = m_strInfo;
			if(m_strInfo.Length == 0)
			{
				int yLabel = m_lblInfo.Location.Y;
				Point ptList = m_lvMain.Location;
				Size szList = m_lvMain.Size;

				m_lblInfo.Visible = false;
				m_lvMain.Location = new Point(ptList.X, yLabel);
				m_lvMain.Size = new Size(szList.Width, szList.Height +
					ptList.Y - yLabel);
			}

			UIUtil.SetExplorerTheme(m_lvMain, true);

			if(m_ilIcons != null) m_lvMain.SmallImageList = m_ilIcons;

			if(m_fInit != null)
			{
				try { m_fInit(m_lvMain); }
				catch(Exception) { Debug.Assert(false); }
			}

			m_lvMain.BeginUpdate();

			ListViewGroup lvgCur = null;
			foreach(object o in m_lData)
			{
				if(o == null) { Debug.Assert(false); continue; }

				ListViewItem lvi = (o as ListViewItem);
				if(lvi != null)
				{
					// Caller should not care about associations
					Debug.Assert(lvi.ListView == null);
					Debug.Assert(lvi.Group == null);

					m_lvMain.Items.Add(lvi);
					if(lvgCur != null) lvgCur.Items.Add(lvi);

					Debug.Assert(lvi.ListView == m_lvMain);
					Debug.Assert(lvi.Group == lvgCur);
					continue;
				}

				ListViewGroup lvg = (o as ListViewGroup);
				if(lvg != null)
				{
					// Caller should not care about associations
					Debug.Assert(lvg.ListView == null);

					m_lvMain.Groups.Add(lvg);
					lvgCur = lvg;

					Debug.Assert(lvg.ListView == m_lvMain);
					continue;
				}

				Debug.Assert(false); // Unknown data type
			}

			m_lvMain.EndUpdate();

			if(m_bEnsureForeground)
			{
				this.BringToFront();
				this.Activate();
			}
		}

		private void CleanUpEx()
		{
			if(m_ilIcons != null)
			{
				m_lvMain.SmallImageList = null; // Detach event handlers
				m_ilIcons.Dispose();
				m_ilIcons = null;
			}
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			CleanUpEx();
			GlobalWindowManager.RemoveWindow(this);
		}

		private bool CreateResult()
		{
			ListView.SelectedListViewItemCollection lvic = m_lvMain.SelectedItems;
			if(lvic.Count != 1) { Debug.Assert(false); return false; }

			ListViewItem lvi = lvic[0];
			if(lvi == null) { Debug.Assert(false); return false; }

			m_resItem = lvi.Tag;
			m_resGroup = ((lvi.Group != null) ? lvi.Group.Tag : null);
			return true;
		}

		private void ProcessItemSelection()
		{
			if(this.DialogResult == DialogResult.OK) return; // Already closing

			if(CreateResult()) this.DialogResult = DialogResult.OK;
		}

		private void OnListItemActivate(object sender, EventArgs e)
		{
			ProcessItemSelection();
		}

		// The item activation handler has a slight delay when clicking an
		// item, thus as a performance optimization we additionally handle
		// item clicks
		private void OnListClick(object sender, EventArgs e)
		{
			ProcessItemSelection();
		}
	}
}
