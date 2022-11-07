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
using System.Windows.Forms;

using KeePass.App;
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
	public partial class KeyCreationForm : Form
	{
		private IOConnectionInfo m_ioInfo = new IOConnectionInfo();
		private bool m_bCreatingNew = true;

		private uint m_uUIAutoBlocked = 0;

		private PwInputControlGroup m_icgPassword = new PwInputControlGroup();
		private Image m_imgKeyFileWarning = null;
		private Image m_imgAccWarning = null;

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

		private GAction m_fInvokeAfterClose = null;
		public GAction InvokeAfterClose
		{
			get { return m_fInvokeAfterClose; }
		}

		public KeyCreationForm()
		{
			InitializeComponent();

			SecureTextBoxEx.InitEx(ref m_tbPassword);
			SecureTextBoxEx.InitEx(ref m_tbRepeatPassword);

			GlobalWindowManager.InitializeForm(this);
		}

		public void InitEx(IOConnectionInfo ioInfo, bool bCreatingNew)
		{
			m_ioInfo = (ioInfo ?? new IOConnectionInfo());
			m_bCreatingNew = bCreatingNew;
		}

		internal static DialogResult ShowDialog(IOConnectionInfo ioInfo,
			bool bCreatingNew, out KeyCreationFormResult r)
		{
			bool bSecDesk = (Program.Config.Security.MasterKeyOnSecureDesktop &&
				ProtectedDialog.IsSupported);

			GFunc<KeyCreationForm> fConstruct = delegate()
			{
				KeyCreationForm f = new KeyCreationForm();
				f.InitEx(ioInfo, bCreatingNew);
				f.SecureDesktopMode = bSecDesk;
				return f;
			};

			GFunc<KeyCreationForm, KeyCreationFormResult> fResultBuilder = delegate(
				KeyCreationForm f)
			{
				KeyCreationFormResult rEx = new KeyCreationFormResult();
				rEx.CompositeKey = f.CompositeKey;
				rEx.InvokeAfterClose = f.InvokeAfterClose;
				return rEx;
			};

			DialogResult dr = ProtectedDialog.ShowDialog<KeyCreationForm,
				KeyCreationFormResult>(bSecDesk, fConstruct, fResultBuilder, out r);

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

			string strTitle = (m_bCreatingNew ? KPRes.CreateMasterKey :
				KPRes.ChangeMasterKey);

			BannerFactory.CreateBannerEx(this, m_bannerImage,
				Properties.Resources.B48x48_KGPG_Sign, strTitle,
				m_ioInfo.GetDisplayName());
			this.Icon = AppIcons.Default;
			this.Text = strTitle;

			FontUtil.SetDefaultFont(m_cbPassword);
			FontUtil.AssignDefaultBold(m_cbPassword);
			FontUtil.AssignDefaultBold(m_cbKeyFile);
			FontUtil.AssignDefaultBold(m_cbUserAccount);

			UIUtil.ConfigureToolTip(m_ttRect);
			UIUtil.SetToolTip(m_ttRect, m_btnSaveKeyFile, KPRes.KeyFileCreate, false);
			UIUtil.SetToolTip(m_ttRect, m_btnOpenKeyFile, KPRes.KeyFileUseExisting, false);

			AccessibilityEx.SetContext(m_tbPassword, m_cbPassword);
			AccessibilityEx.SetContext(m_cmbKeyFile, m_cbKeyFile);
			AccessibilityEx.SetName(m_picKeyFileWarning, KPRes.Warning);
			AccessibilityEx.SetName(m_picAccWarning, KPRes.Warning);

			// Debug.Assert(!m_lblIntro.AutoSize); // For RTL support
			// if(!m_bCreatingNew)
			//	m_lblIntro.Text = KPRes.ChangeMasterKeyIntroShort;

			m_cbPassword.Checked = true;
			m_icgPassword.Attach(m_tbPassword, m_cbHidePassword, m_lblRepeatPassword,
				m_tbRepeatPassword, m_lblEstimatedQuality, m_pbPasswordQuality,
				m_lblQualityInfo, m_ttRect, this, true, m_bSecureDesktop);

			Debug.Assert(!m_cmbKeyFile.Sorted);
			m_cmbKeyFile.Items.Add(KPRes.NoKeyFileSpecifiedMeta);
			m_cmbKeyFile.SelectedIndex = 0;

			foreach(KeyProvider kp in Program.KeyProviderPool)
				m_cmbKeyFile.Items.Add(kp.Name);

			m_imgKeyFileWarning = UIUtil.IconToBitmap(SystemIcons.Warning,
				DpiUtil.ScaleIntX(16), DpiUtil.ScaleIntY(16));
			m_imgAccWarning = (Image)m_imgKeyFileWarning.Clone();
			m_picKeyFileWarning.Image = m_imgKeyFileWarning;
			m_picAccWarning.Image = m_imgAccWarning;

			UIUtil.ApplyKeyUIFlags(Program.Config.UI.KeyCreationFlags,
				m_cbPassword, m_cbKeyFile, m_cbUserAccount, m_cbHidePassword);

			if(!m_cbKeyFile.Enabled && !m_cbKeyFile.Checked)
				UIUtil.SetEnabledFast(false, m_lblKeyFileInfo, m_lblKeyFileWarning);

			if(WinUtil.IsWindows9x || NativeLib.IsUnix() ||
				(!m_cbUserAccount.Enabled && !m_cbUserAccount.Checked))
			{
				UIUtil.SetChecked(m_cbUserAccount, false);
				UIUtil.SetEnabledFast(false, m_cbUserAccount, m_lblWindowsAccDesc,
					m_lblWindowsAccDesc2);
			}

			m_cbExpert.Checked = (m_cbKeyFile.Checked || m_cbUserAccount.Checked);

			--m_uUIAutoBlocked;
			UpdateUIState();
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

			if(m_imgKeyFileWarning != null)
			{
				m_picKeyFileWarning.Image = null;
				m_imgKeyFileWarning.Dispose();
				m_imgKeyFileWarning = null;
			}

			if(m_imgAccWarning != null)
			{
				m_picAccWarning.Image = null;
				m_imgAccWarning.Dispose();
				m_imgAccWarning = null;
			}

			m_icgPassword.Release();

			GlobalWindowManager.RemoveWindow(this);
		}

		private void UpdateUIState()
		{
			if(m_uUIAutoBlocked != 0) return;
			++m_uUIAutoBlocked;

			bool bPassword = m_cbPassword.Checked;
			bool bExpert = m_cbExpert.Checked;
			bool bKeyFile = m_cbKeyFile.Checked;
			bool bUserAccount = m_cbUserAccount.Checked;

			string strKeyFile = m_cmbKeyFile.Text;
			bool bKeyFileOK = (bKeyFile && !string.IsNullOrEmpty(strKeyFile) &&
				(strKeyFile != KPRes.NoKeyFileSpecifiedMeta));
			bool bKeyProv = (bKeyFileOK && Program.KeyProviderPool.IsKeyProvider(strKeyFile));

			m_icgPassword.Enabled = bPassword;

			m_cbExpert.Enabled = (!bKeyFile && !bUserAccount);

			Control[] vExpert = new Control[] {
				m_cbKeyFile, m_cmbKeyFile, m_btnOpenKeyFile, m_btnSaveKeyFile,
				m_lblKeyFileInfo, m_picKeyFileWarning, m_lblKeyFileWarning,
				m_lnkKeyFile,
				m_cbUserAccount, m_lblWindowsAccDesc, m_picAccWarning,
				m_lblWindowsAccDesc2, m_lnkUserAccount
			};
			foreach(Control c in vExpert) c.Visible = bExpert;

			m_cmbKeyFile.Enabled = bKeyFile;
			UIUtil.SetToolTipByText(m_ttRect, m_cmbKeyFile);
			UIUtil.SetEnabledFast(bKeyFile && !bKeyProv, m_btnOpenKeyFile, m_btnSaveKeyFile);

			bool bOK = (bPassword || bKeyFileOK || bUserAccount); // At least one
			bOK &= (!bKeyFile || bKeyFileOK); // Disallow checked without selection
			UIUtil.SetEnabled(m_btnOK, bOK);

			--m_uUIAutoBlocked;
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			m_pKey = KeyUtil.KeyFromUI(m_cbPassword, m_icgPassword, m_tbPassword,
				m_cbKeyFile, m_cmbKeyFile, m_cbUserAccount, m_ioInfo, m_bSecureDesktop);
			if(m_pKey == null) this.DialogResult = DialogResult.None;
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
			m_pKey = null;
		}

		private void OnPasswordCheckedChanged(object sender, EventArgs e)
		{
			if(m_uUIAutoBlocked != 0) return;

			UpdateUIState(); // Enables the text box (req. for focus), if checked
			if(m_cbPassword.Checked) UIUtil.SetFocus(m_tbPassword, this);
		}

		private void OnExpertCheckedChanged(object sender, EventArgs e)
		{
			UpdateUIState();
		}

		private void OnKeyFileCheckedChanged(object sender, EventArgs e)
		{
			UpdateUIState();
		}

		private void OnKeyFileSelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateUIState();
		}

		private void OnUserAccountCheckedChanged(object sender, EventArgs e)
		{
			UpdateUIState();
		}

		private void OnClickKeyFileCreate(object sender, EventArgs e)
		{
			IOConnectionInfo ioc = m_ioInfo;
			bool bSecDesk = m_bSecureDesktop;

			GAction f = delegate()
			{
				KeyFileCreationForm dlg = new KeyFileCreationForm();
				dlg.InitEx(ioc);

				DialogResult dr = dlg.ShowDialog();
				if((dr == DialogResult.OK) && !bSecDesk)
				{
					string strFile = dlg.ResultFile;
					if(!string.IsNullOrEmpty(strFile))
					{
						m_cmbKeyFile.Items.Add(strFile);
						m_cmbKeyFile.SelectedIndex = m_cmbKeyFile.Items.Count - 1;

						UpdateUIState();
					}
					else { Debug.Assert(false); }
				}

				UIUtil.DestroyForm(dlg);
			};

			ProtectedDialog.ContinueOnNormalDesktop(f, this, ref m_fInvokeAfterClose,
				bSecDesk);
		}

		private void OnClickKeyFileBrowse(object sender, EventArgs e)
		{
			string strFile = FileDialogsEx.ShowKeyFileDialog(false,
				KPRes.KeyFileUseExisting, null, false, m_bSecureDesktop);
			if(string.IsNullOrEmpty(strFile)) return;

			try
			{
				IOConnectionInfo ioc = IOConnectionInfo.FromPath(strFile);
				if(!IOConnection.FileExists(ioc, true))
					throw new FileNotFoundException();

				// Check the file size?
			}
			catch(Exception ex) { MessageService.ShowWarning(strFile, ex); return; }

			if(!KfxFile.CanLoad(strFile))
			{
				if(!MessageService.AskYesNo(strFile + MessageService.NewParagraph +
					KPRes.KeyFileNoXml + MessageService.NewParagraph +
					KPRes.KeyFileUseAnywayQ, null, false))
					return;
			}

			m_cmbKeyFile.Items.Add(strFile);
			m_cmbKeyFile.SelectedIndex = m_cmbKeyFile.Items.Count - 1;

			UpdateUIState();
		}

		private void ShowHelpEx(string strTopic, string strSection)
		{
			GAction f = delegate() { AppHelp.ShowHelp(strTopic, strSection); };

			ProtectedDialog.ContinueOnNormalDesktop(f, this, ref m_fInvokeAfterClose,
				m_bSecureDesktop);
		}

		private void OnBtnHelp(object sender, EventArgs e)
		{
			ShowHelpEx(AppDefs.HelpTopics.KeySources, null);
		}

		private void OnKeyFileLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ShowHelpEx(AppDefs.HelpTopics.KeySources, AppDefs.HelpTopics.KeySourcesKeyFile);
		}

		private void OnUserAccountLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ShowHelpEx(AppDefs.HelpTopics.KeySources, AppDefs.HelpTopics.KeySourcesUserAccount);
		}
	}
}
