/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2021 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.IO;
using System.Text;
using System.Windows.Forms;

using KeePass.App;
using KeePass.Plugins;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class PluginsForm : Form, IGwmWindow
	{
		private PluginManager m_mgr = null;
		private bool m_bBlockListUpdate = false;
		private ImageList m_ilIcons = new ImageList();

		public bool CanCloseWithoutDataLoss { get { return true; } }

		internal void InitEx(PluginManager mgr)
		{
			Debug.Assert(mgr != null);
			m_mgr = mgr;
		}

		public PluginsForm()
		{
			InitializeComponent();
			Program.Translation.ApplyTo(this);
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			if(m_mgr == null) { Debug.Assert(false); throw new InvalidOperationException(); }

			GlobalWindowManager.AddWindow(this, this);

			BannerFactory.CreateBannerEx(this, m_bannerImage,
				Properties.Resources.B48x48_BlockDevice, KPRes.Plugins,
				KPRes.PluginsDesc);
			this.Icon = AppIcons.Default;

			Debug.Assert(!m_lblCacheSize.AutoSize); // For RTL support
			m_lblCacheSize.Text += " " + StrUtil.FormatDataSize(
				PlgxCache.GetUsedCacheSize()) + ".";

			m_cbCacheDeleteOld.Checked = Program.Config.Application.Start.PluginCacheDeleteOld;

			if(string.IsNullOrEmpty(PluginManager.UserDirectory))
			{
				Debug.Assert(false);
				m_btnOpenFolder.Enabled = false;
			}

			m_lvPlugins.Columns.Add(KPRes.Plugin);
			m_lvPlugins.Columns.Add(KPRes.Version);
			m_lvPlugins.Columns.Add(KPRes.Author);
			m_lvPlugins.Columns.Add(KPRes.Description);
			m_lvPlugins.Columns.Add(KPRes.File);
			UIUtil.ResizeColumns(m_lvPlugins, new int[] {
				4, 2, 3, 0, 2 }, true);

			m_ilIcons.ImageSize = new Size(DpiUtil.ScaleIntX(16),
				DpiUtil.ScaleIntY(16));
			m_ilIcons.ColorDepth = ColorDepth.Depth32Bit;
			m_lvPlugins.SmallImageList = m_ilIcons;

			UpdatePluginsList();
			if(m_lvPlugins.Items.Count > 0)
			{
				m_lvPlugins.Items[0].Selected = true;
				UIUtil.SetFocus(m_lvPlugins, this);
			}

			UpdatePluginDescription();
		}

		private void CleanUpEx()
		{
			if(m_ilIcons != null)
			{
				m_lvPlugins.SmallImageList = null; // Detach event handlers
				m_ilIcons.Dispose();
				m_ilIcons = null;
			}
		}

		private void UpdatePluginsList()
		{
			if(m_bBlockListUpdate) return;
			m_bBlockListUpdate = true;

			m_lvPlugins.Items.Clear();
			m_ilIcons.Images.Clear();

			m_ilIcons.Images.Add(Properties.Resources.B16x16_BlockDevice);

			List<PluginInfo> lInfos = new List<PluginInfo>(m_mgr);
			lInfos.Sort(PluginsForm.ComparePluginInfos);

			foreach(PluginInfo pi in lInfos)
			{
				ListViewItem lvi = m_lvPlugins.Items.Add(pi.Name);

				lvi.SubItems.Add(pi.FileVersion);
				lvi.SubItems.Add(pi.Author);
				lvi.SubItems.Add(pi.Description);
				lvi.SubItems.Add(pi.DisplayFilePath);

				Plugin p = pi.Interface;
				Debug.Assert(p != null);

				int nImageIndex = 0;
				Image img = ((p != null) ? p.SmallIcon : null);
				if(img != null)
				{
					nImageIndex = m_ilIcons.Images.Count;
					m_ilIcons.Images.Add(img);
				}
				lvi.ImageIndex = nImageIndex;
			}

			m_bBlockListUpdate = false;
			UpdatePluginDescription();
		}

		private static int ComparePluginInfos(PluginInfo x, PluginInfo y)
		{
			return string.Compare(x.Name, y.Name, StrUtil.CaseIgnoreCmp);
		}

		private void UpdatePluginDescription()
		{
			Debug.Assert(!m_lblSelectedPluginDesc.AutoSize); // For RTL support

			ListView.SelectedListViewItemCollection lvsic = m_lvPlugins.SelectedItems;
			if(lvsic.Count == 0)
			{
				m_grpPluginDesc.Text = string.Empty;
				m_lblSelectedPluginDesc.Text = string.Empty;
				return;
			}

			ListViewItem lvi = lvsic[0];
			m_grpPluginDesc.Text = lvi.SubItems[0].Text;
			m_lblSelectedPluginDesc.Text = lvi.SubItems[3].Text;
		}

		private void OnBtnClose(object sender, EventArgs e)
		{
		}

		private void OnPluginListSelectedIndexChanged(object sender, EventArgs e)
		{
			UpdatePluginDescription();
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			CleanUpEx();
			GlobalWindowManager.RemoveWindow(this);
		}

		private void OnBtnClearCache(object sender, EventArgs e)
		{
			Program.Config.Application.Start.PluginCacheClearOnce = true;

			MessageService.ShowInfo(KPRes.PluginCacheClearInfo);
		}

		private void OnFormClosing(object sender, FormClosingEventArgs e)
		{
			Program.Config.Application.Start.PluginCacheDeleteOld =
				m_cbCacheDeleteOld.Checked;
		}

		private void OnBtnGetMore(object sender, EventArgs e)
		{
			WinUtil.OpenUrl(PwDefs.PluginsUrl, null);
		}

		private void OnBtnOpenFolder(object sender, EventArgs e)
		{
			try
			{
				string str = PluginManager.UserDirectory;
				if(string.IsNullOrEmpty(str)) { Debug.Assert(false); return; }

				if(!Directory.Exists(str)) Directory.CreateDirectory(str);

				WinUtil.OpenUrlDirectly(str);
			}
			catch(Exception ex) { MessageService.ShowWarning(ex); }
		}
	}
}
