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
			Debug.Assert(m_mgr != null); if(m_mgr == null) throw new ArgumentException();

			GlobalWindowManager.AddWindow(this, this);

			BannerFactory.CreateBannerEx(this, m_bannerImage,
				Properties.Resources.B48x48_BlockDevice, KPRes.Plugins,
				KPRes.PluginsDesc);
			this.Icon = Properties.Resources.KeePass;

			m_cbCacheDeleteOld.Checked = Program.Config.Application.Start.PluginCacheDeleteOld;

			m_lvPlugins.Columns.Add(KPRes.Plugin, 197);
			m_lvPlugins.Columns.Add(KPRes.Version, 106);
			m_lvPlugins.Columns.Add(KPRes.Author, 136);
			m_lvPlugins.Columns.Add(KPRes.Description, 0);
			m_lvPlugins.Columns.Add(KPRes.File, 119);

			m_ilIcons.ImageSize = new Size(16, 16);
			m_ilIcons.ColorDepth = ColorDepth.Depth32Bit;

			m_lblCacheSize.Text += " " + StrUtil.FormatDataSize(
				PlgxCache.GetUsedCacheSize()) + ".";

			m_lvPlugins.SmallImageList = m_ilIcons;
			UpdatePluginsList();

			if(m_lvPlugins.Items.Count > 0)
			{
				m_lvPlugins.Items[0].Selected = true;
				UIUtil.SetFocus(m_lvPlugins, this);
			}

			UpdatePluginDescription();
		}

		private void UpdatePluginsList()
		{
			if(m_bBlockListUpdate) return;
			m_bBlockListUpdate = true;

			m_lvPlugins.Items.Clear();
			m_ilIcons.Images.Clear();

			m_ilIcons.Images.Add(Properties.Resources.B16x16_BlockDevice);

			foreach(PluginInfo plugin in m_mgr)
			{
				ListViewItem lvi = new ListViewItem(plugin.Name);
				ListViewItem lviNew = m_lvPlugins.Items.Add(lvi);

				lviNew.SubItems.Add(plugin.FileVersion);
				lviNew.SubItems.Add(plugin.Author);
				lviNew.SubItems.Add(plugin.Description);
				lviNew.SubItems.Add(plugin.DisplayFilePath);

				int nImageIndex = 0;
				Debug.Assert(plugin.Interface != null);
				if((plugin.Interface != null) && (plugin.Interface.SmallIcon != null))
				{
					nImageIndex = m_ilIcons.Images.Count;
					m_ilIcons.Images.Add(plugin.Interface.SmallIcon);
				}

				lviNew.ImageIndex = nImageIndex;
			}

			m_bBlockListUpdate = false;
			UpdatePluginDescription();
		}

		private void UpdatePluginDescription()
		{
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

		private void OnPluginsLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			WinUtil.OpenUrl(PwDefs.PluginsUrl, null);
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			GlobalWindowManager.RemoveWindow(this);
		}

		private void OnBtnClearCache(object sender, EventArgs e)
		{
			Program.Config.Application.Start.PluginCacheClearOnce = true;

			MessageService.ShowInfo(KPRes.PluginCacheClearInfo);
		}

		private void OnFormClosing(object sender, FormClosingEventArgs e)
		{
			Program.Config.Application.Start.PluginCacheDeleteOld = m_cbCacheDeleteOld.Checked;
		}
	}
}
