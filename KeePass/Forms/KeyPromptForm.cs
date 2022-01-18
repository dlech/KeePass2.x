/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2022 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Threading;
using System.Windows.Forms;

using KeePass.App;
using KeePass.App.Configuration;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Delegates;
using KeePassLib.Keys;
using KeePassLib.Native;
using KeePassLib.Security;
using KeePassLib.Serialization;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class KeyPromptForm : Form
	{
		private IOConnectionInfo m_ioInfo = new IOConnectionInfo();
		private bool m_bCanExit = false;
		// private bool m_bRedirectActivation = false;
		private string m_strCustomTitle = null;

		private uint m_uUIAutoBlocked = 0;
		private bool m_bDisposed = false;

		private List<string> m_lKeyFileNames = new List<string>();
		// private List<Image> m_lKeyFileImages = new List<Image>();

		private bool m_bSecureDesktop = false;
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DefaultValue(false)]
		public bool SecureDesktopMode
		{
			get { return m_bSecureDesktop; }
			set { m_bSecureDesktop = value; }
		}

		private CompositeKey m_pKey = null;
		public CompositeKey CompositeKey
		{
			get { return m_pKey; }
		}

		private bool m_bHasExited = false;
		public bool HasClosedWithExit
		{
			get { return m_bHasExited; }
		}

		private GAction m_fInvokeAfterClose = null;
		public GAction InvokeAfterClose
		{
			get { return m_fInvokeAfterClose; }
		}

		public KeyPromptForm()
		{
			InitializeComponent();

			SecureTextBoxEx.InitEx(ref m_tbPassword);
			GlobalWindowManager.InitializeForm(this);
		}

		public void InitEx(IOConnectionInfo ioInfo, bool bCanExit,
			bool bRedirectActivation)
		{
			InitEx(ioInfo, bCanExit, bRedirectActivation, null);
		}

		public void InitEx(IOConnectionInfo ioInfo, bool bCanExit,
			bool bRedirectActivation, string strCustomTitle)
		{
			m_ioInfo = (ioInfo ?? new IOConnectionInfo());
			m_bCanExit = bCanExit;
			// m_bRedirectActivation = bRedirectActivation;
			m_strCustomTitle = strCustomTitle;
		}

		internal static DialogResult ShowDialog(IOConnectionInfo ioInfo,
			bool bCanExit, string strCustomTitle, out KeyPromptFormResult r)
		{
			bool bSecDesk = (Program.Config.Security.MasterKeyOnSecureDesktop &&
				ProtectedDialog.IsSupported);

			GFunc<KeyPromptForm> fConstruct = delegate()
			{
				KeyPromptForm f = new KeyPromptForm();
				f.InitEx(ioInfo, bCanExit, true, strCustomTitle);
				f.SecureDesktopMode = bSecDesk;
				return f;
			};

			GFunc<KeyPromptForm, KeyPromptFormResult> fResultBuilder = delegate(
				KeyPromptForm f)
			{
				KeyPromptFormResult rEx = new KeyPromptFormResult();
				rEx.CompositeKey = f.CompositeKey;
				rEx.HasClosedWithExit = f.HasClosedWithExit;
				rEx.InvokeAfterClose = f.InvokeAfterClose;
				return rEx;
			};

			DialogResult dr = ProtectedDialog.ShowDialog<KeyPromptForm,
				KeyPromptFormResult>(bSecDesk, fConstruct, fResultBuilder, out r);

			GAction fIac = ((r != null) ? r.InvokeAfterClose : null);
			if(fIac != null) fIac();

			return dr;
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			++m_uUIAutoBlocked;

			// The password text box should not be focused by default
			// in order to avoid a Caps Lock warning tooltip bug;
			// https://sourceforge.net/p/keepass/bugs/1807/
			Debug.Assert((m_tbPassword.TabIndex >= 2) && !m_tbPassword.Focused);

			GlobalWindowManager.AddWindow(this);
			// if(m_bRedirectActivation) Program.MainForm.RedirectActivationPush(this);

			string strBannerTitle = (!string.IsNullOrEmpty(m_strCustomTitle) ?
				m_strCustomTitle : KPRes.EnterCompositeKey);
			string strBannerDesc = m_ioInfo.GetDisplayName(); // Compacted by banner
			BannerFactory.CreateBannerEx(this, m_bannerImage,
				Properties.Resources.B48x48_KGPG_Key2, strBannerTitle, strBannerDesc);
			this.Icon = AppIcons.Default;

			string strStart = (!string.IsNullOrEmpty(m_strCustomTitle) ?
				m_strCustomTitle : KPRes.OpenDatabase);
			string strName = UrlUtil.GetFileName(m_ioInfo.Path);
			if(!string.IsNullOrEmpty(strName))
				this.Text = strStart + " - " + strName;
			else this.Text = strStart;

			FontUtil.SetDefaultFont(m_cbPassword);
			// FontUtil.AssignDefaultBold(m_cbPassword);
			// FontUtil.AssignDefaultBold(m_cbKeyFile);
			// FontUtil.AssignDefaultBold(m_cbUserAccount);

			UIUtil.ConfigureToolTip(m_ttRect);
			UIUtil.SetToolTip(m_ttRect, m_btnOpenKeyFile, KPRes.KeyFileSelect, true);

			UIUtil.AccSetName(m_tbPassword, m_cbPassword);
			UIUtil.AccSetName(m_cmbKeyFile, m_cbKeyFile);

			PwInputControlGroup.ConfigureHideButton(m_cbHidePassword, m_ttRect);

			// Enable protection before possibly setting a text
			m_cbHidePassword.Checked = true;
			OnHidePasswordCheckedChanged(null, EventArgs.Empty);

			// Must be set manually due to possible object override
			m_tbPassword.TextChanged += this.OnPasswordTextChanged;

			// m_cmbKeyFile.OrderedImageList = m_lKeyFileImages;
			AddKeyFileItem(KPRes.NoKeyFileSpecifiedMeta, true);

			Debug.Assert(!AnyComponentOn());

			// Do not directly compare with Program.CommandLineArgs.FileName,
			// because this may be a relative path instead of an absolute one
			string strCmdLineFile = Program.CommandLineArgs.FileName;
			if(!string.IsNullOrEmpty(strCmdLineFile) && (Program.MainForm != null))
				strCmdLineFile = Program.MainForm.IocFromCommandLine().Path;
			if(!string.IsNullOrEmpty(strCmdLineFile) && strCmdLineFile.Equals(
				m_ioInfo.Path, StrUtil.CaseIgnoreCmp))
			{
				string str;

				str = Program.CommandLineArgs[AppDefs.CommandLineOptions.Password];
				if(str != null)
				{
					m_cbPassword.Checked = true;
					m_tbPassword.Text = str;
				}

				str = Program.CommandLineArgs[AppDefs.CommandLineOptions.PasswordEncrypted];
				if(str != null)
				{
					m_cbPassword.Checked = true;
					m_tbPassword.Text = StrUtil.DecryptString(str);
				}

				str = Program.CommandLineArgs[AppDefs.CommandLineOptions.PasswordStdIn];
				if(str != null)
				{
					ProtectedString ps = KeyUtil.ReadPasswordStdIn(true);
					if(ps != null)
					{
						m_cbPassword.Checked = true;
						m_tbPassword.TextEx = ps;
					}
				}

				str = Program.CommandLineArgs[AppDefs.CommandLineOptions.KeyFile];
				if(!string.IsNullOrEmpty(str)) AddKeyFileItem(str, true);

				str = Program.CommandLineArgs[AppDefs.CommandLineOptions.PreSelect];
				if(!string.IsNullOrEmpty(str)) AddKeyFileItem(str, true);

				str = Program.CommandLineArgs[AppDefs.CommandLineOptions.UserAccount];
				if(str != null) m_cbUserAccount.Checked = true;
			}

			AceKeyAssoc a = Program.Config.Defaults.GetKeySources(m_ioInfo);
			if((a != null) && !AnyComponentOn())
			{
				if(a.Password) m_cbPassword.Checked = true;

				if(!string.IsNullOrEmpty(a.KeyFilePath))
					AddKeyFileItem(a.KeyFilePath, true);
				if(!string.IsNullOrEmpty(a.KeyProvider))
					AddKeyFileItem(a.KeyProvider, true);

				if(a.UserAccount) m_cbUserAccount.Checked = true;
			}

			foreach(KeyProvider kp in Program.KeyProviderPool)
				AddKeyFileItem(kp.Name, false);

			UIUtil.ApplyKeyUIFlags(Program.Config.UI.KeyPromptFlags,
				m_cbPassword, m_cbKeyFile, m_cbUserAccount, m_cbHidePassword);

			if(!m_cbPassword.Enabled && !m_cbPassword.Checked)
			{
				m_tbPassword.Text = string.Empty;
				UIUtil.SetEnabledFast(false, m_tbPassword, m_cbHidePassword);
			}

			if(!m_cbKeyFile.Enabled && !m_cbKeyFile.Checked)
				UIUtil.SetEnabledFast(false, m_cmbKeyFile, m_btnOpenKeyFile);

			if(WinUtil.IsWindows9x || NativeLib.IsUnix())
			{
				UIUtil.SetChecked(m_cbUserAccount, false);
				UIUtil.SetEnabled(m_cbUserAccount, false);
			}

			m_btnExit.Enabled = m_bCanExit;
			m_btnExit.Visible = m_bCanExit;

			--m_uUIAutoBlocked;
			UpdateUIState();

			// Local, but thread will continue to run anyway
			Thread th = new Thread(new ThreadStart(this.OnFormLoadAsync));
			th.Start();
			// ThreadPool.QueueUserWorkItem(new WaitCallback(this.OnFormLoadAsync));

			this.BringToFront();
			this.Activate();
			// UIUtil.SetFocus(m_tbPassword, this); // See OnFormShown
		}

		private void OnFormShown(object sender, EventArgs e)
		{
			// Focusing doesn't always work in OnFormLoad;
			// https://sourceforge.net/p/keepass/feature-requests/1735/
			if(m_tbPassword.CanFocus) UIUtil.ResetFocus(m_tbPassword, this, true);
			else if(m_cmbKeyFile.CanFocus) UIUtil.SetFocus(m_cmbKeyFile, this, true);
			else if(m_btnOK.CanFocus) UIUtil.SetFocus(m_btnOK, this, true);
			else { Debug.Assert(false); }
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			Debug.Assert(m_uUIAutoBlocked == 0);
			Debug.Assert(m_cmbKeyFile.Items.Count == m_lKeyFileNames.Count);

			if(m_bDisposed) { Debug.Assert(false); return; }
			m_bDisposed = true;

			// m_cmbKeyFile.OrderedImageList = null;
			// m_lKeyFileImages.Clear();

			m_tbPassword.TextChanged -= this.OnPasswordTextChanged;

			// if(m_bRedirectActivation) Program.MainForm.RedirectActivationPop();
			GlobalWindowManager.RemoveWindow(this);
		}

		private bool AnyComponentOn()
		{
			string strKeyFile = m_cmbKeyFile.Text;
			bool bKeyFile = (m_cbKeyFile.Checked && !string.IsNullOrEmpty(strKeyFile) &&
				(strKeyFile != KPRes.NoKeyFileSpecifiedMeta));

			return (m_cbPassword.Checked || bKeyFile || m_cbUserAccount.Checked);
		}

		private void UpdateUIState()
		{
			if(m_uUIAutoBlocked != 0) return;
			++m_uUIAutoBlocked;

			UIUtil.SetToolTipByText(m_ttRect, m_cmbKeyFile);

			UIUtil.SetEnabled(m_btnOK, AnyComponentOn());

			--m_uUIAutoBlocked;
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			m_pKey = KeyUtil.KeyFromUI(m_cbPassword, null, m_tbPassword,
				m_cbKeyFile, m_cmbKeyFile, m_cbUserAccount, m_ioInfo, m_bSecureDesktop);
			if(m_pKey == null) this.DialogResult = DialogResult.None;
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
			m_pKey = null;
		}

		private void OnBtnExit(object sender, EventArgs e)
		{
			if(m_bCanExit) m_bHasExited = true;
			else { Debug.Assert(false); this.DialogResult = DialogResult.None; }
		}

		private void OnBtnHelp(object sender, EventArgs e)
		{
			GAction f = delegate() { AppHelp.ShowHelp(AppDefs.HelpTopics.KeySources, null); };

			ProtectedDialog.ContinueOnNormalDesktop(f, this, ref m_fInvokeAfterClose,
				m_bSecureDesktop);
		}

		private void OnPasswordCheckedChanged(object sender, EventArgs e)
		{
			if(m_uUIAutoBlocked != 0) return;

			UpdateUIState();
			if(m_cbPassword.Checked) UIUtil.SetFocus(m_tbPassword, this);
		}

		private void OnPasswordTextChanged(object sender, EventArgs e)
		{
			if(m_uUIAutoBlocked != 0) return;
			++m_uUIAutoBlocked;

			if(m_cbPassword.Enabled)
				UIUtil.SetChecked(m_cbPassword, (m_tbPassword.TextLength != 0));

			--m_uUIAutoBlocked;
			UpdateUIState();
		}

		private void OnHidePasswordCheckedChanged(object sender, EventArgs e)
		{
			bool bAuto = (m_uUIAutoBlocked == 0);
			bool bHide = m_cbHidePassword.Checked;

			if(!bHide && bAuto && !AppPolicy.Try(AppPolicyId.UnhidePasswords))
			{
				UIUtil.SetChecked(m_cbHidePassword, true);
				return;
			}

			m_tbPassword.EnableProtection(bHide);

			if(bAuto) UIUtil.SetFocus(m_tbPassword, this);
		}

		private void OnKeyFileCheckedChanged(object sender, EventArgs e)
		{
			if(m_uUIAutoBlocked != 0) return;
			++m_uUIAutoBlocked;

			if(!m_cbKeyFile.Checked) m_cmbKeyFile.SelectedIndex = 0;

			--m_uUIAutoBlocked;
			UpdateUIState();
		}

		private void OnKeyFileSelectedIndexChanged(object sender, EventArgs e)
		{
			if(m_uUIAutoBlocked != 0) return;
			++m_uUIAutoBlocked;

			if(m_cbKeyFile.Enabled)
			{
				string strKeyFile = m_cmbKeyFile.Text;
				UIUtil.SetChecked(m_cbKeyFile, (!string.IsNullOrEmpty(strKeyFile) &&
					(strKeyFile != KPRes.NoKeyFileSpecifiedMeta)));
			}

			--m_uUIAutoBlocked;
			UpdateUIState();
		}

		private void OnUserAccountCheckedChanged(object sender, EventArgs e)
		{
			UpdateUIState();
		}

		private void OnClickKeyFileBrowse(object sender, EventArgs e)
		{
			string strFile = FileDialogsEx.ShowKeyFileDialog(false,
				KPRes.KeyFileSelect, null, true, m_bSecureDesktop);
			if(string.IsNullOrEmpty(strFile)) return;

			AddKeyFileItem(strFile, true);
		}

		private void OnFormLoadAsync()
		{
			try { PopulateKeyFileSuggestions(); }
			catch(Exception) { Debug.Assert(false); }
		}

		private void PopulateKeyFileSuggestions()
		{
			if(!Program.Config.Integration.SearchKeyFiles) return;

			bool bSearchOnRemovable = Program.Config.Integration.SearchKeyFilesOnRemovableMedia;

			DriveInfo[] vDrives = DriveInfo.GetDrives();
			foreach(DriveInfo di in vDrives)
			{
				DriveType t = di.DriveType;
				if((t == DriveType.NoRootDirectory) || (t == DriveType.CDRom))
					continue;
				if((t == DriveType.Removable) && !bSearchOnRemovable)
					continue;

				ThreadPool.QueueUserWorkItem(new WaitCallback(
					this.AddKeyDriveItemAsync), di);
			}
		}

		private void AddKeyDriveItemAsync(object oDriveInfo)
		{
			try
			{
				DriveInfo di = (oDriveInfo as DriveInfo);
				if(di == null) { Debug.Assert(false); return; }
				if(!di.IsReady) return;

				string[] vExts = new string[] {
					AppDefs.FileExtension.KeyFile, AppDefs.FileExtension.KeyFileAlt
				};

				foreach(string strExt in vExts)
				{
					List<FileInfo> lFiles = UrlUtil.GetFileInfos(di.RootDirectory,
						"*." + strExt, SearchOption.TopDirectoryOnly);

					foreach(FileInfo fi in lFiles)
						AddKeyFileItemAsync(fi.FullName);
				}
			}
			catch(Exception) { Debug.Assert(false); }
		}

		private void AddKeyFileItemAsync(string str)
		{
			if(m_cmbKeyFile.InvokeRequired)
				m_cmbKeyFile.Invoke(new AkfiDelegate(this.AddKeyFileItem), str, false);
			else AddKeyFileItem(str, false);
		}

		private delegate void AkfiDelegate(string str, bool bSelect);
		private void AddKeyFileItem(string str, bool bSelect)
		{
			try
			{
				if(string.IsNullOrEmpty(str)) { Debug.Assert(false); return; }
				if(m_bDisposed) return; // Slow drive

				Debug.Assert(!m_cmbKeyFile.Sorted);

				int iIndex = m_lKeyFileNames.IndexOf(str);
				if(iIndex < 0)
				{
					iIndex = m_lKeyFileNames.Count;
					m_lKeyFileNames.Add(str);
					// m_lKeyFileImages.Add(img);
					m_cmbKeyFile.Items.Add(str);
				}

				if(bSelect)
				{
					++m_uUIAutoBlocked;

					if(m_cbKeyFile.Enabled)
						UIUtil.SetChecked(m_cbKeyFile, (str != KPRes.NoKeyFileSpecifiedMeta));
					m_cmbKeyFile.SelectedIndex = iIndex;

					--m_uUIAutoBlocked;
					UpdateUIState();
				}
			}
			catch(Exception) { Debug.Assert(false); }
		}
	}
}
