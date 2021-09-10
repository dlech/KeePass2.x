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
using KeePass.Resources;
using KeePass.UI;

using KeePassLib.Keys;
using KeePassLib.Serialization;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class KeyFileCreationForm : Form
	{
		private sealed class KfcfInfo
		{
			public readonly ulong Version;
			public readonly string Name;

			public KfcfInfo(ulong uVersion, string strName)
			{
				this.Version = uVersion;
				this.Name = strName;
			}
		}

		private IOConnectionInfo m_ioInfo = new IOConnectionInfo();

		private int m_cBlockUIUpdate = 0;

		private readonly KfcfInfo[] m_vNewFormat = new KfcfInfo[] {
			new KfcfInfo(0x0002000000000000, "2.0 (" + KPRes.Recommended + ")"),
			new KfcfInfo(0x0001000000000000, "1.0 (" + KPRes.CompatWithOldVer + ")")
		};
		private readonly KfcfInfo[] m_vRecFormat = new KfcfInfo[] {
			new KfcfInfo(0x0002000000000000, "2.0"),
			new KfcfInfo(0x0001000000000000, "1.0")
		};

		private string m_strResultFile = null;
		public string ResultFile
		{
			get { return m_strResultFile; }
		}

		public KeyFileCreationForm()
		{
			InitializeComponent();
			GlobalWindowManager.InitializeForm(this);
		}

		public void InitEx(IOConnectionInfo ioInfo)
		{
			if(ioInfo != null) m_ioInfo = ioInfo;
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			++m_cBlockUIUpdate;

			GlobalWindowManager.AddWindow(this);

			BannerFactory.CreateBannerEx(this, m_bannerImage,
				KeePass.Properties.Resources.B48x48_KGPG_Gen,
				KPRes.KeyFileCreateTitle, KPRes.KeyFileCreate + ".");
			this.Icon = AppIcons.Default;
			this.Text = KPRes.KeyFileCreateTitle;

			FontUtil.AssignDefaultBold(m_rbCreate);
			FontUtil.AssignDefaultBold(m_rbRecreate);
			FontUtil.AssignDefaultMono(m_tbRecKeyHash, false);
			FontUtil.AssignDefaultMono(m_tbRecKey, false);

			m_rbCreate.Checked = true;
			m_cbNewEntropy.Checked = true;

			Debug.Assert(!m_cmbNewFormat.Sorted);
			foreach(KfcfInfo kfi in m_vNewFormat)
				m_cmbNewFormat.Items.Add(kfi.Name);
			m_cmbNewFormat.SelectedIndex = 0;

			Debug.Assert(!m_cmbRecFormat.Sorted);
			foreach(KfcfInfo kfi in m_vRecFormat)
				m_cmbRecFormat.Items.Add(kfi.Name);
			m_cmbRecFormat.SelectedIndex = 0;

			m_rbCreate.CheckedChanged += this.OnShouldUpdateUIState;
			m_rbRecreate.CheckedChanged += this.OnShouldUpdateUIState;
			m_cmbRecFormat.SelectedIndexChanged += this.OnShouldUpdateUIState;
			m_tbRecKey.TextChanged += this.OnShouldUpdateUIState;

			--m_cBlockUIUpdate;
			UpdateUIState();
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			GlobalWindowManager.RemoveWindow(this);
		}

		private void UpdateUIState()
		{
			if(m_cBlockUIUpdate > 0) return;

			bool bCreate = m_rbCreate.Checked;
			bool bRecreate = m_rbRecreate.Checked;
			KfcfInfo kfiRec = m_vRecFormat[m_cmbRecFormat.SelectedIndex];

			UIUtil.SetEnabledFast(bCreate, m_lblNewFormat, m_cmbNewFormat,
				m_cbNewEntropy);

			UIUtil.SetEnabledFast(bRecreate, m_lblRecFormat, m_cmbRecFormat,
				m_lblRecKey, m_tbRecKey);
			UIUtil.SetEnabledFast(bRecreate && (kfiRec.Version == 0x0002000000000000),
				m_lblRecKeyHash, m_tbRecKeyHash);

			UIUtil.SetEnabledFast(bCreate || (bRecreate &&
				(m_tbRecKey.Text.Trim().Length != 0)), m_btnOK);
		}

		private void OnShouldUpdateUIState(object sender, EventArgs e)
		{
			UpdateUIState();
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			string strResultFile = null;

			try
			{
				if(m_rbCreate.Checked)
					strResultFile = CreateKeyFile();
				else if(m_rbRecreate.Checked)
					strResultFile = RecreateKeyFile();
				else { Debug.Assert(false); throw new NotSupportedException(); }
			}
			catch(Exception ex) { MessageService.ShowWarning(ex); }

			if(string.IsNullOrEmpty(strResultFile))
				this.DialogResult = DialogResult.None;
			else m_strResultFile = strResultFile;
		}

		private string GetKeyFilePath()
		{
			string strExt = AppDefs.FileExtension.KeyFile;
			string strFilter = AppDefs.GetKeyFileFilter();

			string strName = UrlUtil.StripExtension(UrlUtil.GetFileName(m_ioInfo.Path));
			if(string.IsNullOrEmpty(strName)) strName = KPRes.KeyFileSafe;

			SaveFileDialogEx sfd = UIUtil.CreateSaveFileDialog(KPRes.KeyFileCreateTitle,
				strName + "." + strExt, strFilter, 1, strExt, AppDefs.FileDialogContext.KeyFile);

			if(sfd.ShowDialog() == DialogResult.OK) return sfd.FileName;
			return null;
		}

		private string CreateKeyFile()
		{
			byte[] pbEntropy = null;
			if(m_cbNewEntropy.Checked)
			{
				EntropyForm dlg = new EntropyForm();
				if(dlg.ShowDialog() == DialogResult.OK)
					pbEntropy = dlg.GeneratedEntropy;
				UIUtil.DestroyForm(dlg);

				if(pbEntropy == null) return null;
			}

			string strFilePath = GetKeyFilePath();
			if(string.IsNullOrEmpty(strFilePath)) return null;

			KcpKeyFile.Create(strFilePath, pbEntropy, m_vNewFormat[
				m_cmbNewFormat.SelectedIndex].Version);
			return strFilePath;
		}

		private string RecreateKeyFile()
		{
			ulong uVersion = m_vRecFormat[m_cmbRecFormat.SelectedIndex].Version;

			string strHash = StrUtil.RemoveWhiteSpace(m_tbRecKeyHash.Text);
			// If the hash is empty, set it to null in order to generate one
			if(strHash.Length == 0) strHash = null;

			KfxFile kf = KfxFile.Create(uVersion, m_tbRecKey.Text, strHash);

			// Ask for the file path after verifying the key hash
			string strFilePath = GetKeyFilePath();
			if(string.IsNullOrEmpty(strFilePath)) return null;

			IOConnectionInfo ioc = IOConnectionInfo.FromPath(strFilePath);
			using(Stream s = IOConnection.OpenWrite(ioc))
			{
				kf.Save(s);
			}

			return strFilePath;
		}
	}
}
