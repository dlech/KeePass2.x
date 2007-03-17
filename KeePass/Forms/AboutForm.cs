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
using System.IO;

using KeePass.App;
using KeePass.UI;
using KeePass.Resources;
using KeePass.Util;
using KeePass.DataExchange;

using KeePassLib;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class AboutForm : Form
	{
		public AboutForm()
		{
			InitializeComponent();
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			string strTitle = PwDefs.ProductName;
			string strDesc = KPRes.Version + " " + PwDefs.VersionString;

			m_bannerImage.Image = BannerFactory.CreateBanner(m_bannerImage.Width,
				m_bannerImage.Height, BannerFactory.BannerStyle.Default,
				Properties.Resources.B48x48_MessageBox_Info, strTitle, strDesc);
			this.Icon = Properties.Resources.KeePass;

			m_lvComponents.Columns.Add(KPRes.Components, 100, HorizontalAlignment.Left);
			m_lvComponents.Columns.Add(KPRes.Version, 100, HorizontalAlignment.Left);

			GetAppComponents();

			int nWidth = m_lvComponents.ClientRectangle.Width / 2;
			int nMod = m_lvComponents.ClientRectangle.Width % 2;
			m_lvComponents.Columns[0].Width = nWidth;
			m_lvComponents.Columns[1].Width = nWidth + nMod;
		}

		private void GetAppComponents()
		{
			ListViewItem lvi = new ListViewItem(KPRes.KeePassLibCLong);
			if(!Kdb3File.IsLibraryInstalled())
				lvi.SubItems.Add(KPRes.NotInstalled);
			else lvi.SubItems.Add(Kdb3Manager.KeePassVersionString + " (0x" +
				Kdb3Manager.LibraryBuild.ToString("X4") + ")");
			m_lvComponents.Items.Add(lvi);

			lvi = new ListViewItem(KPRes.XSLStylesheets);
			string strPath = WinUtil.GetExecutable();
			strPath = UrlUtil.GetFileDirectory(strPath, true);
			bool bInstalled = File.Exists(strPath + AppDefs.XslFileHtmlLite);
			bInstalled &= File.Exists(strPath + AppDefs.XslFileHtmlFull);
			bInstalled &= File.Exists(strPath + AppDefs.XslFileHtmlTabular);

			if(!bInstalled) lvi.SubItems.Add(KPRes.NotInstalled);
			else lvi.SubItems.Add(PwDefs.VersionString);
			m_lvComponents.Items.Add(lvi);
		}

		private void OnLinkHomepage(object sender, LinkLabelLinkClickedEventArgs e)
		{
			WinUtil.OpenUrlInNewBrowser(PwDefs.HomepageUrl, null);
		}

		private void OnLinkHelpFile(object sender, LinkLabelLinkClickedEventArgs e)
		{
			AppHelp.ShowHelp(null, null);
		}

		private void OnLinkLicenseFile(object sender, LinkLabelLinkClickedEventArgs e)
		{
			AppHelp.ShowHelp(AppDefs.HelpTopics.License, null, true);
		}

		private void OnLinkAcknowledgements(object sender, LinkLabelLinkClickedEventArgs e)
		{
			AppHelp.ShowHelp(AppDefs.HelpTopics.Acknowledgements, null, true);
		}

		private void OnLinkDonate(object sender, LinkLabelLinkClickedEventArgs e)
		{
			WinUtil.OpenUrlInNewBrowser(PwDefs.DonationsUrl, null);
		}
	}
}
