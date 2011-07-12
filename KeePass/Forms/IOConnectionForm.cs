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
using System.IO;
using System.Diagnostics;

using KeePass.App;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;

using KeePassLib.Serialization;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class IOConnectionForm : Form
	{
		private bool m_bSave = false;
		private IOConnectionInfo m_ioc = new IOConnectionInfo();
		private bool m_bCanRememberCred = true;
		private bool m_bTestConnection = false;

		public IOConnectionInfo IOConnectionInfo
		{
			get { return m_ioc; }
		}

		public void InitEx(bool bSave, IOConnectionInfo ioc, bool bCanRememberCred,
			bool bTestConnection)
		{
			m_bSave = bSave;
			if(ioc != null) m_ioc = ioc;
			m_bCanRememberCred = bCanRememberCred;
			m_bTestConnection = bTestConnection;
		}

		public IOConnectionForm()
		{
			InitializeComponent();
			Program.Translation.ApplyTo(this);
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			// Must work without a parent window
			Debug.Assert(this.StartPosition == FormStartPosition.CenterScreen);

			GlobalWindowManager.AddWindow(this);

			string strTitle = (m_bSave ? KPRes.UrlSaveTitle : KPRes.UrlOpenTitle);
			string strDesc = (m_bSave ? KPRes.UrlSaveDesc : KPRes.UrlOpenDesc);

			BannerFactory.CreateBannerEx(this, m_bannerImage,
				KeePass.Properties.Resources.B48x48_WWW, strTitle, strDesc);
			this.Icon = Properties.Resources.KeePass;
			this.Text = strTitle;

			FontUtil.AssignDefaultBold(m_lblUrl);
			FontUtil.AssignDefaultBold(m_lblUserName);
			FontUtil.AssignDefaultBold(m_lblPassword);
			FontUtil.AssignDefaultBold(m_lblRemember);

			m_tbUrl.Text = (m_ioc.IsLocalFile() ? string.Empty : m_ioc.Path);
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

			if(m_bCanRememberCred == false)
			{
				m_cmbCredSaveMode.SelectedIndex = 0;
				m_cmbCredSaveMode.Enabled = false;
			}

			UIUtil.SetFocus(m_tbUrl, this);

			if((m_tbUrl.TextLength > 0) && (m_tbUserName.TextLength > 0))
				UIUtil.SetFocus(m_tbPassword, this);
			else if(m_tbUrl.TextLength > 0)
				UIUtil.SetFocus(m_tbUserName, this);
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			string strUrl = m_tbUrl.Text;

			if(strUrl.IndexOf(@"://") < 0)
			{
				m_ttInvalidUrl.Show(KPRes.InvalidUrl, m_tbUrl);
				return;
			}

			m_ioc.Path = strUrl;
			m_ioc.UserName = m_tbUserName.Text;
			m_ioc.Password = m_tbPassword.Text;

			if(m_cmbCredSaveMode.SelectedIndex == 1)
				m_ioc.CredSaveMode = IOCredSaveMode.UserNameOnly;
			else if(m_cmbCredSaveMode.SelectedIndex == 2)
				m_ioc.CredSaveMode = IOCredSaveMode.SaveCred;
			else
				m_ioc.CredSaveMode = IOCredSaveMode.NoSave;

			if(m_bTestConnection && !m_bSave)
			{
				if(this.TestConnectionEx() == false)
					this.DialogResult = DialogResult.None;
			}
		}

		private bool TestConnectionEx()
		{
			bool bResult = true;
			bool bOK = m_btnOK.Enabled, bCancel = m_btnCancel.Enabled;
			bool bCombo = m_cmbCredSaveMode.Enabled;

			m_btnOK.Enabled = m_btnCancel.Enabled = m_tbUrl.Enabled =
				m_tbUserName.Enabled = m_tbPassword.Enabled =
				m_btnHelp.Enabled = m_cmbCredSaveMode.Enabled = false;

			Application.DoEvents();

			try
			{
				if(!IOConnection.FileExists(m_ioc, true))
					throw new FileNotFoundException();
			}
			catch(Exception exTest)
			{
				string strError = exTest.Message;
				if((exTest.InnerException != null) &&
					!string.IsNullOrEmpty(exTest.InnerException.Message))
					strError += MessageService.NewParagraph +
						exTest.InnerException.Message;

				MessageService.ShowWarning(m_ioc.GetDisplayName(), strError);
				bResult = false;
			}

			m_btnOK.Enabled = bOK;
			m_btnCancel.Enabled = bCancel;
			m_cmbCredSaveMode.Enabled = bCombo;
			m_btnHelp.Enabled = m_tbUserName.Enabled = m_tbUrl.Enabled =
				m_tbPassword.Enabled = true;
			return bResult;
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
		}

		private void OnBtnHelp(object sender, EventArgs e)
		{
			AppHelp.ShowHelp(AppDefs.HelpTopics.IOConnections, null);
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			GlobalWindowManager.RemoveWindow(this);
		}
	}
}