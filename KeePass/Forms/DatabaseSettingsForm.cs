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
using KeePassLib.Cryptography.Cipher;
using KeePassLib.Keys;
using KeePassLib.Security;

namespace KeePass.Forms
{
	public partial class DatabaseSettingsForm : Form
	{
		private bool m_bCreatingNew = false;
		private PwDatabase m_pwDatabase = null;

		public DatabaseSettingsForm()
		{
			InitializeComponent();
		}

		public void InitEx(bool bCreatingNew, PwDatabase pwDatabase)
		{
			m_bCreatingNew = bCreatingNew;

			Debug.Assert(pwDatabase != null); if(pwDatabase == null) throw new ArgumentNullException();
			m_pwDatabase = pwDatabase;
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			Debug.Assert(m_pwDatabase != null); if(m_pwDatabase == null) throw new ArgumentNullException();

			m_bannerImage.Image = BannerFactory.CreateBanner(m_bannerImage.Width,
				m_bannerImage.Height, BannerFactory.BannerStyle.Default,
				Properties.Resources.B48x48_Ark, KPRes.DatabaseSettings,
				KPRes.DatabaseSettingsDesc);
			this.Icon = Properties.Resources.KeePass;

			if(m_bCreatingNew) this.Text = KPRes.ConfigureOnNewDatabase;
			else this.Text = KPRes.DatabaseSettings;

			m_tbDbName.Text = m_pwDatabase.Name;
			m_tbDbDesc.Text = m_pwDatabase.Description;
			m_tbDefaultUser.Text = m_pwDatabase.DefaultUserName;

			for(int inx = 0; inx < CipherPool.GlobalPool.EngineCount; ++inx)
				m_cmbEncAlgo.Items.Add(CipherPool.GlobalPool[inx].DisplayName);

			if(m_cmbEncAlgo.Items.Count > 0)
			{
				int nIndex = CipherPool.GlobalPool.GetCipherIndex(m_pwDatabase.DataCipherUuid);
				m_cmbEncAlgo.SelectedIndex = ((nIndex >= 0) ? nIndex : 0);
			}

			m_numEncRounds.Minimum = ulong.MinValue;
			m_numEncRounds.Maximum = ulong.MaxValue;
			m_numEncRounds.Value = m_pwDatabase.KeyEncryptionRounds;

			m_lbMemProt.Items.Add(KPRes.Title, m_pwDatabase.MemoryProtection.ProtectTitle);
			m_lbMemProt.Items.Add(KPRes.UserName, m_pwDatabase.MemoryProtection.ProtectUserName);
			m_lbMemProt.Items.Add(KPRes.Password, m_pwDatabase.MemoryProtection.ProtectPassword);
			m_lbMemProt.Items.Add(KPRes.URL, m_pwDatabase.MemoryProtection.ProtectUrl);
			m_lbMemProt.Items.Add(KPRes.Notes, m_pwDatabase.MemoryProtection.ProtectNotes);

			m_cbAutoEnableHiding.Checked = m_pwDatabase.MemoryProtection.AutoEnableVisualHiding;

			if(m_pwDatabase.Compression == PwCompressionAlgorithm.None)
				m_rbNone.Checked = true;
			else if(m_pwDatabase.Compression == PwCompressionAlgorithm.GZip)
				m_rbGZip.Checked = true;
			else { Debug.Assert(false); }
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			m_pwDatabase.Name = m_tbDbName.Text;
			m_pwDatabase.Description = m_tbDbDesc.Text;
			m_pwDatabase.DefaultUserName = m_tbDefaultUser.Text;

			int nCipher = CipherPool.GlobalPool.GetCipherIndex(m_cmbEncAlgo.Text);
			Debug.Assert(nCipher >= 0);
			if(nCipher >= 0)
				m_pwDatabase.DataCipherUuid = CipherPool.GlobalPool[nCipher].CipherUuid;
			else
				m_pwDatabase.DataCipherUuid = new PwUuid(StandardAesEngine.AesUuidBytes);

			m_pwDatabase.KeyEncryptionRounds = (ulong)m_numEncRounds.Value;

			if(m_rbNone.Checked) m_pwDatabase.Compression = PwCompressionAlgorithm.None;
			else if(m_rbGZip.Checked) m_pwDatabase.Compression = PwCompressionAlgorithm.GZip;
			else { Debug.Assert(false); }

			m_pwDatabase.MemoryProtection.ProtectTitle = UpdateMemoryProtection(0,
				m_pwDatabase.MemoryProtection.ProtectTitle, PwDefs.TitleField);
			m_pwDatabase.MemoryProtection.ProtectUserName = UpdateMemoryProtection(1,
				m_pwDatabase.MemoryProtection.ProtectUserName, PwDefs.UserNameField);
			m_pwDatabase.MemoryProtection.ProtectPassword = UpdateMemoryProtection(2,
				m_pwDatabase.MemoryProtection.ProtectPassword, PwDefs.PasswordField);
			m_pwDatabase.MemoryProtection.ProtectUrl = UpdateMemoryProtection(3,
				m_pwDatabase.MemoryProtection.ProtectUrl, PwDefs.UrlField);
			m_pwDatabase.MemoryProtection.ProtectNotes = UpdateMemoryProtection(4,
				m_pwDatabase.MemoryProtection.ProtectNotes, PwDefs.NotesField);

			m_pwDatabase.MemoryProtection.AutoEnableVisualHiding = m_cbAutoEnableHiding.Checked;
		}

		private bool UpdateMemoryProtection(int nIndex, bool bOldSetting, string strFieldID)
		{
			bool bNewProt = m_lbMemProt.GetItemChecked(nIndex);

			if(bNewProt != bOldSetting)
			{
				m_pwDatabase.RootGroup.EnableStringFieldProtection(strFieldID, bNewProt);
			}

#if DEBUG
			KeePassLib.Delegates.GroupHandler gh = delegate(PwGroup pg)
			{
				return true;
			};
			KeePassLib.Delegates.EntryHandler eh = delegate(PwEntry pe)
			{
				Debug.Assert(pe.Strings.Get(strFieldID).IsProtected == bNewProt);
				return true;
			};
			Debug.Assert(m_pwDatabase.RootGroup.TraverseTree(TraversalMethod.PreOrder, gh, eh));
#endif

			return bNewProt;
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
		}

		private void OnBtnHelp(object sender, EventArgs e)
		{
			string strSubTopic = null;
			if(m_tabMain.SelectedTab == m_tabGeneral)
				strSubTopic = AppDefs.HelpTopics.DbSettingsGeneral;
			else if(m_tabMain.SelectedTab == m_tabSecurity)
				strSubTopic = AppDefs.HelpTopics.DbSettingsSecurity;
			else if(m_tabMain.SelectedTab == m_tabProtection)
				strSubTopic = AppDefs.HelpTopics.DbSettingsProtection;
			else if(m_tabMain.SelectedTab == m_tabCompression)
				strSubTopic = AppDefs.HelpTopics.DbSettingsCompression;

			AppHelp.ShowHelp(AppDefs.HelpTopics.DatabaseSettings, strSubTopic);
		}

		private void OnLinkClicked1SecondDelayRounds(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if(e.Button == MouseButtons.Left)
				m_numEncRounds.Value = CompositeKey.TransformKeyBenchmark(1000, 3001);
		}
	}
}