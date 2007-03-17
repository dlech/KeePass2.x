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

using KeePass.App;
using KeePass.Resources;
using KeePass.UI;

using KeePassLib.Serialization;

namespace KeePass.Forms
{
	public partial class IOConnectionForm : Form
	{
		private bool m_bSave = false;
		private IOConnectionInfo m_ioc = new IOConnectionInfo();

		public IOConnectionInfo IOConnectionInfo
		{
			get { return m_ioc; }
		}

		public void InitEx(bool bSave, IOConnectionInfo ioc)
		{
			m_bSave = bSave;
			if(ioc != null) m_ioc = ioc;
		}

		public IOConnectionForm()
		{
			InitializeComponent();
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			string strTitle = (m_bSave ? KPRes.UrlSaveTitle : KPRes.UrlOpenTitle);
			string strDesc = (m_bSave ? KPRes.UrlSaveDesc : KPRes.UrlOpenDesc);

			m_bannerImage.Image = BannerFactory.CreateBanner(m_bannerImage.Width,
				m_bannerImage.Height, BannerFactory.BannerStyle.Default,
				KeePass.Properties.Resources.B48x48_WWW, strTitle,
				strDesc);
			this.Icon = Properties.Resources.KeePass;
			this.Text = strTitle;

			m_tbUrl.Text = m_ioc.Url;
			m_tbUserName.Text = m_ioc.UserName;
			m_tbPassword.Text = m_ioc.Password;

			m_cmbCredSaveMode.Items.Add(KPRes.CredSaveNone);
			m_cmbCredSaveMode.Items.Add(KPRes.CredSaveUserOnly);
			m_cmbCredSaveMode.Items.Add(KPRes.CredSaveAll);

			if(m_ioc.CredSaveMode == IOCredSaveMode.UserNameOnly)
				m_cmbCredSaveMode.SelectedIndex = 1;
			else if(m_ioc.CredSaveMode == IOCredSaveMode.SaveCred)
				m_cmbCredSaveMode.SelectedIndex = 2;
			else
				m_cmbCredSaveMode.SelectedIndex = 0;

			m_tbUrl.Focus();

			// Give the user name field the focus, if URL is specified.
			// Anyone knows a better solution than SendKeys?
			if(m_ioc.Url.Length > 0) SendKeys.Send(@"{TAB}");
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			string strUrl = m_tbUrl.Text;

			if(strUrl.IndexOf(@"://") < 0)
			{
				m_ttInvalidUrl.Show(KPRes.InvalidUrl, m_tbUrl);
				return;
			}

			m_ioc.Url = strUrl;
			m_ioc.UserName = m_tbUserName.Text;
			m_ioc.Password = m_tbPassword.Text;

			if(m_cmbCredSaveMode.SelectedIndex == 1)
				m_ioc.CredSaveMode = IOCredSaveMode.UserNameOnly;
			else if(m_cmbCredSaveMode.SelectedIndex == 2)
				m_ioc.CredSaveMode = IOCredSaveMode.SaveCred;
			else
				m_ioc.CredSaveMode = IOCredSaveMode.NoSave;
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
		}

		private void OnBtnHelp(object sender, EventArgs e)
		{
			AppHelp.ShowHelp(AppDefs.HelpTopics.IOConnections, null);
		}
	}
}