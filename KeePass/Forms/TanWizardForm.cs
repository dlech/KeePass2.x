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

using KeePass.UI;
using KeePass.Resources;

using KeePassLib;
using KeePassLib.Security;

namespace KeePass.Forms
{
	public partial class TanWizardForm : Form
	{
		private PwDatabase m_pwDatabase = null;
		private PwGroup m_pgStorage = null;

		public void InitEx(PwDatabase pwParent, PwGroup pgStorage)
		{
			m_pwDatabase = pwParent;
			m_pgStorage = pgStorage;
		}

		public TanWizardForm()
		{
			InitializeComponent();
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			Debug.Assert(m_pwDatabase != null); if(m_pwDatabase == null) throw new ArgumentNullException();
			Debug.Assert(m_pgStorage != null); if(m_pgStorage == null) throw new ArgumentNullException();

			GlobalWindowManager.AddWindow(this);

			m_bannerImage.Image = BannerFactory.CreateBanner(m_bannerImage.Width,
				m_bannerImage.Height, BannerFactory.BannerStyle.Default,
				KeePass.Properties.Resources.B48x48_Wizard, KPRes.TANWizard,
				KPRes.TANWizardDesc);
			
			this.Icon = Properties.Resources.KeePass;
			this.Text = KPRes.TANWizard;

			EnableControlsEx();
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			ParseTANs();
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
		}

		private void EnableControlsEx()
		{
			m_numTANsIndex.Enabled = m_cbNumberTANs.Checked;
		}

		private void ParseTANs()
		{
			StringBuilder sb = new StringBuilder();
			string strText = m_tbTANs.Text;
			int nTANIndex = (int)m_numTANsIndex.Value;
			bool bSetIndex = m_cbNumberTANs.Checked;

			for(int i = 0; i < strText.Length; ++i)
			{
				char ch = strText[i];

				if((ch >= '0') && (ch <= '9'))
					sb.Append(ch);
				else if((ch >= 'A') && (ch <= 'Z'))
					sb.Append(ch);
				else if((ch >= 'a') && (ch <= 'z'))
					sb.Append(ch);
				else
				{
					AddTAN(sb.ToString(), bSetIndex, nTANIndex);
					++nTANIndex;

					sb = new StringBuilder();
				}
			}
		}

		private void AddTAN(string strTAN, bool bSetIndex, int nTANIndex)
		{
			if(strTAN.Length == 0) return;

			PwEntry pe = new PwEntry(m_pgStorage, true, true);
			pe.Strings.Set(PwDefs.TitleField, new ProtectedString(
				m_pwDatabase.MemoryProtection.ProtectTitle, PwDefs.TanTitle));

			pe.Strings.Set(PwDefs.PasswordField, new ProtectedString(
				m_pwDatabase.MemoryProtection.ProtectPassword, strTAN));

			if(bSetIndex && (nTANIndex >= 0))
			{
				Debug.Assert(PwDefs.TanIndexField == PwDefs.UserNameField);

				pe.Strings.Set(PwDefs.TanIndexField, new ProtectedString(
					m_pwDatabase.MemoryProtection.ProtectUserName, nTANIndex.ToString()));
			}

			m_pgStorage.Entries.Add(pe);
		}

		private void OnNumberTANsCheckedChanged(object sender, EventArgs e)
		{
			EnableControlsEx();
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			GlobalWindowManager.RemoveWindow(this);
		}
	}
}
