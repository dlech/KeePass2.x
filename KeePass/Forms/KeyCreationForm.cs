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

using KeePass.App;
using KeePass.UI;
using KeePass.Resources;

using KeePassLib;
using KeePassLib.Cryptography;
using KeePassLib.Keys;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class KeyCreationForm : Form
	{
		private CompositeKey m_pKey = null;
		private bool m_bCreatingNew = false;
		private string m_strDisplayName = string.Empty;

		private SecureEdit m_secPassword = new SecureEdit();
		private SecureEdit m_secRepeat = new SecureEdit();

		public CompositeKey CompositeKey
		{
			get
			{
				Debug.Assert(m_pKey != null);
				return m_pKey;
			}
		}

		public KeyCreationForm()
		{
			InitializeComponent();
		}

		public void InitEx(bool bCreatingNew, string strFilePath)
		{
			m_bCreatingNew = bCreatingNew;

			if(strFilePath != null) m_strDisplayName = strFilePath;
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			m_bannerImage.Image = BannerFactory.CreateBanner(m_bannerImage.Width,
				m_bannerImage.Height, BannerFactory.BannerStyle.Default,
				Properties.Resources.B48x48_KGPG_Sign, KPRes.CreateMasterKey,
				m_strDisplayName);
			this.Icon = Properties.Resources.KeePass;

			if(!m_bCreatingNew)
				m_lblIntro.Text = KPRes.ChangeMasterKeyIntro;

			m_secPassword.Attach(m_tbPassword, ProcessTextChangedPassword, true);
			m_secRepeat.Attach(m_tbRepeatPassword, null, true);
			m_cbHidePassword.Checked = true;

			m_cbPassword.Checked = true;
			ProcessTextChangedPassword(sender, e); // Update quality estimation

			m_tbKeyFile.Text = KPRes.NoKeyFileSpecifiedMeta;

			EnableUserControls();
		}

		private void CleanUpEx()
		{
			m_secPassword.Detach();
			m_secRepeat.Detach();
		}

		private bool CreateCompositeKey()
		{
			m_pKey = new CompositeKey();

			if(m_cbPassword.Checked) // Use a password
			{
				if(m_cbHidePassword.Checked)
				{
					if(m_secPassword.ContentsEqualTo(m_secRepeat) == false)
					{
						MessageBox.Show(KPRes.RepeatIsntSame, PwDefs.ShortProductName,
							MessageBoxButtons.OK, MessageBoxIcon.Warning);
						m_pKey = null;
						return false;
					}
				}

				byte[] pb = m_secPassword.ToUTF8();
				m_pKey.AddUserKey(new KcpPassword(pb));
				Array.Clear(pb, 0, pb.Length);
			}

			if(m_cbKeyFile.Checked && (!m_tbKeyFile.Text.Equals(KPRes.NoKeyFileSpecifiedMeta)))
			{
				try { m_pKey.AddUserKey(new KcpKeyFile(m_tbKeyFile.Text)); }
				catch(Exception)
				{
					MessageBox.Show(m_tbKeyFile.Text + "\r\n\r\n" + KPRes.KeyFileError,
						PwDefs.ShortProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return false;
				}
			}
			if(m_cbUserAccount.Checked)
				m_pKey.AddUserKey(new KcpUserAccount());

			return true;
		}

		private void EnableUserControls()
		{
			m_tbPassword.Enabled = m_tbRepeatPassword.Enabled = m_cbHidePassword.Enabled =
				m_lblRepeatPassword.Enabled = m_lblQualityBits.Enabled =
				m_lblEstimatedQuality.Enabled = m_cbPassword.Checked;

			m_btnOpenKeyFile.Enabled = m_btnSaveKeyFile.Enabled =
				m_tbKeyFile.Enabled = m_cbKeyFile.Checked;

			if((!m_cbPassword.Checked) && (!m_cbKeyFile.Checked) && (!m_cbUserAccount.Checked))
				m_btnCreate.Enabled = false;
			else if((m_cbKeyFile.Checked) && (m_tbKeyFile.Text.Equals(KPRes.NoKeyFileSpecifiedMeta)))
				m_btnCreate.Enabled = false;
			else m_btnCreate.Enabled = true;

			SetHidePassword(m_cbHidePassword.Checked, false);

			m_ttRect.SetToolTip(m_tbKeyFile, m_tbKeyFile.Text);
		}

		private void SetHidePassword(bool bHide, bool bUpdateCheckBox)
		{
			if(bUpdateCheckBox) m_cbHidePassword.Checked = bHide;

			m_secPassword.EnableProtection(bHide);
			m_secRepeat.EnableProtection(bHide);
		}

		private void OnCheckedPassword(object sender, EventArgs e)
		{
			EnableUserControls();

			if(m_cbPassword.Checked) m_tbPassword.Focus();
		}

		private void OnCheckedKeyFile(object sender, EventArgs e)
		{
			EnableUserControls();
		}

		private void OnCheckedHidePassword(object sender, EventArgs e)
		{
			SetHidePassword(m_cbHidePassword.Checked, false);
			m_tbPassword.Focus();
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

		private void ProcessTextChangedPassword(object sender, EventArgs e)
		{
			byte[] pbUTF8 = m_secPassword.ToUTF8();
			uint uBits = QualityEstimation.EstimatePasswordBits(pbUTF8);
			MemUtil.ZeroByteArray(pbUTF8);

			m_lblQualityBits.Text = uBits.ToString() + " " + KPRes.Bits;
			int iPos = (int)((100 * uBits) / (256 / 2));
			if(iPos < 0) iPos = 0; else if(iPos > 100) iPos = 100;
			m_pbPasswordQuality.Value = iPos;
		}

		private void OnClickKeyFileCreate(object sender, EventArgs e)
		{
			m_saveKeyFileDialog.FileName = UrlUtil.StripExtension(UrlUtil.GetFileName(m_strDisplayName)) +
				"." + AppDefs.FileExtension.KeyFile;
			m_saveKeyFileDialog.InitialDirectory = UrlUtil.GetFileDirectory(m_strDisplayName, false);

			if(m_saveKeyFileDialog.ShowDialog() == DialogResult.OK)
			{
				EntropyForm dlg = new EntropyForm();

				if(dlg.ShowDialog() == DialogResult.OK)
				{
					byte[] pbAdditionalEntropy = dlg.GeneratedEntropy;
					FileSaveResult fsr = KcpKeyFile.Create(m_saveKeyFileDialog.FileName, pbAdditionalEntropy);

					if(fsr.Code != FileSaveResultCode.Success)
						MessageBox.Show(KPRes.FileCreationError, PwDefs.ShortProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					else
						m_tbKeyFile.Text = m_saveKeyFileDialog.FileName;
				}
			}

			EnableUserControls();
		}

		private void OnClickKeyFileBrowse(object sender, EventArgs e)
		{
			m_openKeyFileDialog.FileName = string.Empty;
			m_openKeyFileDialog.InitialDirectory = UrlUtil.GetFileDirectory(m_strDisplayName, false);

			if(m_openKeyFileDialog.ShowDialog() == DialogResult.OK)
				m_tbKeyFile.Text = m_openKeyFileDialog.FileName;

			EnableUserControls();
		}

		private void OnWinUserCheckedChanged(object sender, EventArgs e)
		{
			EnableUserControls();
		}
	}
}
