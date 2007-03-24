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
using System.Globalization;

using KeePass.UI;
using KeePass.Resources;

using KeePassLib;
using KeePassLib.Collections;

namespace KeePass.Forms
{
	public partial class GroupForm : Form
	{
		private PwGroup m_pwGroup = null;
		private ImageList m_ilClientIcons = null;

		private PwIcon m_pwIconIndex = 0;

		public void InitEx(PwGroup pg, ImageList ilClientIcons)
		{
			m_pwGroup = pg;
			m_ilClientIcons = ilClientIcons;
		}

		public GroupForm()
		{
			InitializeComponent();
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			Debug.Assert(m_pwGroup != null); if(m_pwGroup == null) throw new ArgumentNullException();

			GlobalWindowManager.AddWindow(this);

			m_bannerImage.Image = BannerFactory.CreateBanner(m_bannerImage.Width,
				m_bannerImage.Height, BannerFactory.BannerStyle.Default,
				Properties.Resources.B48x48_Folder_Txt, KPRes.EditGroup,
				KPRes.EditGroupDesc);
			this.Icon = Properties.Resources.KeePass;

			m_dtExpires.CustomFormat = DateTimeFormatInfo.CurrentInfo.ShortDatePattern +
				" " + DateTimeFormatInfo.CurrentInfo.LongTimePattern;

			m_pwIconIndex = m_pwGroup.Icon;
			
			m_tbName.Text = m_pwGroup.Name;
			m_btnIcon.Image = m_ilClientIcons.Images[(int)m_pwIconIndex];

			if(m_pwGroup.Expires)
			{
				m_dtExpires.Value = m_pwGroup.ExpiryTime;
				m_cbExpires.Checked = true;
			}
			else // Does not expire
			{
				m_dtExpires.Value = DateTime.Now;
				m_cbExpires.Checked = false;
			}

			m_tbDefaultAutoTypeSeq.Text = m_pwGroup.DefaultAutoTypeSequence;

			if(m_pwGroup.DefaultAutoTypeSequence.Length == 0)
				m_rbAutoTypeInherit.Checked = true;
			else m_rbAutoTypeOverride.Checked = true;

			EnableControlsEx();
		}

		private void EnableControlsEx()
		{
			m_tbDefaultAutoTypeSeq.Enabled = m_btnAutoTypeEdit.Enabled =
				!m_rbAutoTypeInherit.Checked;
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			m_pwGroup.Name = m_tbName.Text;
			m_pwGroup.Icon = m_pwIconIndex;

			m_pwGroup.Expires = m_cbExpires.Checked;
			m_pwGroup.ExpiryTime = m_dtExpires.Value;

			if(m_rbAutoTypeInherit.Checked)
				m_pwGroup.DefaultAutoTypeSequence = string.Empty;
			else m_pwGroup.DefaultAutoTypeSequence = m_tbDefaultAutoTypeSeq.Text;
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
		}

		private void OnBtnIcon(object sender, EventArgs e)
		{
			IconPickerForm ipf = new IconPickerForm();
			ipf.InitEx(m_ilClientIcons, (uint)m_pwIconIndex);

			if(ipf.ShowDialog() == DialogResult.OK)
			{
				m_pwIconIndex = (PwIcon)ipf.ChosenImageIndex;
				m_btnIcon.Image = m_ilClientIcons.Images[(int)m_pwIconIndex];
			}
		}

		private void OnExpiresValueChanged(object sender, EventArgs e)
		{
			m_cbExpires.Checked = true;
		}

		private void OnAutoTypeInheritCheckedChanged(object sender, EventArgs e)
		{
			EnableControlsEx();
		}

		private void OnBtnAutoTypeEdit(object sender, EventArgs e)
		{
			EditAutoTypeItemForm dlg = new EditAutoTypeItemForm();

			string strName = "(" + KPRes.AutoType + ")";

			AutoTypeConfig atConfig = new AutoTypeConfig();
			atConfig.DefaultSequence = m_tbDefaultAutoTypeSeq.Text;

			dlg.InitEx(atConfig, new ProtectedStringDictionary(),
				strName, true);

			if(dlg.ShowDialog() == DialogResult.OK)
				m_tbDefaultAutoTypeSeq.Text = atConfig.DefaultSequence;

			EnableControlsEx();
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			GlobalWindowManager.RemoveWindow(this);
		}
	}
}