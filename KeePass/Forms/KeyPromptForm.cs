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
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

using KeePass.App;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Keys;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class KeyPromptForm : Form
	{
		private CompositeKey m_pKey = null;
		private string m_strDisplayName = string.Empty;
		
		private bool m_bCanExit = false;
		private bool m_bHasExited = false;

		private SecureEdit m_secPassword = new SecureEdit();

		private bool m_bInitializing = false;

		private List<string> m_vSuggestions = new List<string>();
		private bool m_bSuggestionsReady = false;

		public CompositeKey CompositeKey
		{
			get
			{
				Debug.Assert(m_pKey != null);
				return m_pKey;
			}
		}

		public bool HasClosedWithExit
		{
			get { return m_bHasExited; }
		}

		public KeyPromptForm()
		{
			InitializeComponent();
		}

		public void InitEx(string strDisplayName, bool bCanExit)
		{
			if(strDisplayName != null) m_strDisplayName = strDisplayName;

			m_bCanExit = bCanExit;
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			Debug.Assert(m_strDisplayName != null);
			if(m_strDisplayName == null) throw new ArgumentNullException();

			GlobalWindowManager.AddWindow(this);

			m_bInitializing = true;

			string strBannerDesc = WinUtil.CompactPath(m_strDisplayName, 45);
			m_bannerImage.Image = BannerFactory.CreateBanner(m_bannerImage.Width,
				m_bannerImage.Height, BannerFactory.BannerStyle.Default,
				Properties.Resources.B48x48_KGPG_Key2, KPRes.EnterCompositeKey,
				strBannerDesc);
			this.Icon = Properties.Resources.KeePass;

			m_tbPassword.Text = string.Empty;
			m_secPassword.Attach(m_tbPassword, ProcessTextChangedPassword, true);

			m_cmbKeyFile.Items.Add(KPRes.NoKeyFileSpecifiedMeta);
			m_cmbKeyFile.SelectedIndex = 0;

			if((Program.CommandLineArgs.FileName != null) &&
				(m_strDisplayName == Program.CommandLineArgs.FileName))
			{
				string str;

				str = Program.CommandLineArgs[AppDefs.CommandLineOptions.Password];
				if(str != null)
				{
					m_cbPassword.Checked = true;
					m_tbPassword.Text = str;
				}

				str = Program.CommandLineArgs[AppDefs.CommandLineOptions.KeyFile];
				if(str != null)
				{
					m_cbKeyFile.Checked = true;

					m_cmbKeyFile.Items.Add(str);
					m_cmbKeyFile.SelectedIndex = m_cmbKeyFile.Items.Count - 1;
				}

				str = Program.CommandLineArgs[AppDefs.CommandLineOptions.PreSelect];
				if(str != null)
				{
					m_cbKeyFile.Checked = true;

					m_cmbKeyFile.Items.Add(str);
					m_cmbKeyFile.SelectedIndex = m_cmbKeyFile.Items.Count - 1;
				}
			}

			m_cbHidePassword.Checked = true;
			OnCheckedHidePassword(sender, e);

			Debug.Assert(m_cmbKeyFile.Text.Length != 0);

			m_btnExit.Enabled = m_bCanExit;
			m_btnExit.Visible = m_bCanExit;

			EnableUserControls();

			m_bInitializing = false;

			Thread th = new Thread(new ThreadStart(PopulateKeyFileSuggestions));
			th.Start();

			this.BringToFront();
			this.Activate();
			m_tbPassword.Focus();
		}

		private void CleanUpEx()
		{
			m_secPassword.Detach();
		}

		private bool CreateCompositeKey()
		{
			m_pKey = new CompositeKey();

			if(m_cbPassword.Checked) // Use a password
			{
				byte[] pb = m_secPassword.ToUTF8();
				m_pKey.AddUserKey(new KcpPassword(pb));
				Array.Clear(pb, 0, pb.Length);
			}

			string strKeyFile = m_cmbKeyFile.Text;
			Debug.Assert(strKeyFile != null); if(strKeyFile == null) strKeyFile = string.Empty;
			if(m_cbKeyFile.Checked && !strKeyFile.Equals(KPRes.NoKeyFileSpecifiedMeta))
			{
				if(ValidateKeyFileLocation() == false)
				{
					m_pKey = null;
					return false;
				}

				try
				{
					KcpKeyFile kf = new KcpKeyFile(strKeyFile);
					m_pKey.AddUserKey(kf);
				}
				catch(Exception)
				{
					MessageService.ShowWarning(strKeyFile, KPRes.KeyFileError);
					m_pKey = null;
					return false;
				}
			}
			if(m_cbUserAccount.Checked)
				m_pKey.AddUserKey(new KcpUserAccount());

			return true;
		}

		private bool ValidateKeyFileLocation()
		{
			string strKeyFile = m_cmbKeyFile.Text;
			Debug.Assert(strKeyFile != null); if(strKeyFile == null) strKeyFile = string.Empty;
			if(strKeyFile.Equals(KPRes.NoKeyFileSpecifiedMeta)) return true;

			bool bSuccess = true;

			if(File.Exists(strKeyFile) == false)
			{
				MessageService.ShowWarning(strKeyFile, KPRes.FileNotFoundError);
				bSuccess = false;
			}

			if(bSuccess == false)
			{
				int nPos = m_cmbKeyFile.Items.IndexOf(strKeyFile);
				if(nPos >= 0) m_cmbKeyFile.Items.RemoveAt(nPos);

				m_cmbKeyFile.SelectedIndex = 0;
			}

			return bSuccess;
		}

		private void EnableUserControls()
		{
			string strKeyFile = m_cmbKeyFile.Text;
			Debug.Assert(strKeyFile != null); if(strKeyFile == null) strKeyFile = string.Empty;
			if(m_cbKeyFile.Checked && strKeyFile.Equals(KPRes.NoKeyFileSpecifiedMeta))
				m_btnOK.Enabled = false;
			else m_btnOK.Enabled = true;
		}

		private void OnCheckedPassword(object sender, EventArgs e)
		{
			if(m_cbPassword.Checked) m_tbPassword.Focus();
		}

		private void OnCheckedKeyFile(object sender, EventArgs e)
		{
			if(m_bInitializing) return;

			if(!m_cbKeyFile.Checked)
				m_cmbKeyFile.SelectedIndex = 0;

			EnableUserControls();
		}

		private void ProcessTextChangedPassword(object sender, EventArgs e)
		{
			m_cbPassword.Checked = (m_tbPassword.Text.Length != 0);
		}

		private void OnCheckedHidePassword(object sender, EventArgs e)
		{
			m_secPassword.EnableProtection(m_cbHidePassword.Checked);
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			if(!CreateCompositeKey()) this.DialogResult = DialogResult.None;
			else CleanUpEx();
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
			CleanUpEx();
		}

		private void OnBtnHelp(object sender, EventArgs e)
		{
			AppHelp.ShowHelp(AppDefs.HelpTopics.KeySources, null);
		}

		private void OnClickKeyFileBrowse(object sender, EventArgs e)
		{
			m_openKeyFileDialog.InitialDirectory = UrlUtil.GetFileDirectory(m_strDisplayName, false);

			if(m_openKeyFileDialog.ShowDialog() == DialogResult.OK)
			{
				m_cbKeyFile.Checked = true;

				m_cmbKeyFile.Items.Add(m_openKeyFileDialog.FileName);
				m_cmbKeyFile.SelectedIndex = m_cmbKeyFile.Items.Count - 1;
			}

			EnableUserControls();
		}

		private void OnKeyFileSelectedIndexChanged(object sender, EventArgs e)
		{
			if(m_bInitializing) return;

			string strKeyFile = m_cmbKeyFile.Text;
			Debug.Assert(strKeyFile != null); if(strKeyFile == null) strKeyFile = string.Empty;
			if(strKeyFile.Equals(KPRes.NoKeyFileSpecifiedMeta) == false)
			{
				if(ValidateKeyFileLocation())
					m_cbKeyFile.Checked = true;
			}
			else m_cbKeyFile.Checked = false;
		}

		private void PopulateKeyFileSuggestions()
		{
			bool bSearchOnRemovable = AppConfigEx.GetBool(
				AppDefs.ConfigKeys.SearchKeyFilesOnRemovable);

			foreach(DriveInfo di in DriveInfo.GetDrives())
			{
				if(di.DriveType == DriveType.NoRootDirectory)
					continue;
				else if((di.DriveType == DriveType.Removable) && !bSearchOnRemovable)
					continue;
				else if(di.DriveType == DriveType.CDRom)
					continue;

				if(di.IsReady == false) continue;

				try
				{
					FileInfo[] vFiles = di.RootDirectory.GetFiles(@"*." +
						AppDefs.FileExtension.KeyFile, SearchOption.TopDirectoryOnly);
					if(vFiles == null) continue;

					foreach(FileInfo fi in vFiles)
						m_vSuggestions.Add(fi.FullName);
				}
				catch(Exception) { Debug.Assert(false); }
			}

			m_bSuggestionsReady = true;
		}

		private void OnKeyFileFillerTimerTick(object sender, EventArgs e)
		{
			if(m_bSuggestionsReady)
			{
				m_bSuggestionsReady = false;
				m_timerKeyFileFiller.Enabled = false;

				foreach(string str in m_vSuggestions)
					m_cmbKeyFile.Items.Add(str);

				m_vSuggestions.Clear();
			}
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			GlobalWindowManager.RemoveWindow(this);
		}

		private void OnBtnExit(object sender, EventArgs e)
		{
			if(m_bCanExit == false)
			{
				Debug.Assert(false);
				this.DialogResult = DialogResult.None;
				return;
			}

			m_bHasExited = true;
		}
	}
}
