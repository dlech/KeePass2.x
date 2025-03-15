/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2025 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Text;
using System.Windows.Forms;

using KeePass.App;
using KeePass.UI;

namespace KeePass.Forms
{
	public partial class SingleLineEditForm : Form
	{
		private string m_strTitle = string.Empty;
		private string m_strDesc = string.Empty;
		private string m_strLongDesc = string.Empty;
		private Image m_imgIcon = null;
		private string m_strDefaultText = string.Empty;
		private string[] m_vSelectable = null;

		private Control m_cEdit = null;

		private SlfFlags m_fl = SlfFlags.None;
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DefaultValue(SlfFlags.None)]
		public SlfFlags FlagsEx
		{
			get { return m_fl; }
			set { m_fl = value; }
		}

		private string m_strResult = string.Empty;
		public string ResultString
		{
			get { return m_strResult; }
		}

		public SingleLineEditForm()
		{
			InitializeComponent();
			GlobalWindowManager.InitializeForm(this);
		}

		public void InitEx(string strTitle, string strDesc, string strLongDesc,
			Image imgIcon, string strDefaultText, string[] vSelectable)
		{
			m_strTitle = strTitle;
			m_strDesc = strDesc;
			m_strLongDesc = strLongDesc;
			m_imgIcon = imgIcon;
			m_strDefaultText = strDefaultText;
			m_vSelectable = vSelectable;
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			if(m_strTitle == null) throw new InvalidOperationException();
			if(m_strDesc == null) throw new InvalidOperationException();
			if(m_strLongDesc == null) throw new InvalidOperationException();
			if(m_imgIcon == null) throw new InvalidOperationException();
			if(m_strDefaultText == null) throw new InvalidOperationException();

			GlobalWindowManager.AddWindow(this);

			BannerFactory.CreateBannerEx(this, m_bannerImage, m_imgIcon,
				m_strTitle, m_strDesc);
			this.Icon = AppIcons.Default;

			this.Text = m_strTitle;

			Debug.Assert(!m_lblLongDesc.AutoSize); // For RTL support
			m_lblLongDesc.Text = m_strLongDesc;

			bool bSensitive = ((m_fl & SlfFlags.Sensitive) != SlfFlags.None);

			if((m_vSelectable == null) || (m_vSelectable.Length == 0))
			{
				m_cmbEdit.Enabled = false;
				m_cmbEdit.Visible = false;

				m_cEdit = m_tbEdit;

				if(bSensitive) m_tbEdit.UseSystemPasswordChar = true;
			}
			else // With selectable values
			{
				m_tbEdit.Enabled = false;
				m_tbEdit.Visible = false;

				m_cEdit = m_cmbEdit;

				Debug.Assert(!bSensitive);

				m_cmbEdit.BeginUpdate();
				foreach(string strItem in m_vSelectable)
					m_cmbEdit.Items.Add(strItem);
				m_cmbEdit.EndUpdate();

				UIUtil.EnableAutoCompletion(m_cmbEdit, false);
			}

			m_cEdit.Text = m_strDefaultText;

			if((m_fl & SlfFlags.ElevationRequired) != SlfFlags.None)
			{
				m_btnOK.FlatStyle = FlatStyle.System;
				m_btnCancel.FlatStyle = FlatStyle.System;
				UIUtil.SetShield(m_btnOK, true);
			}
		}

		private void OnFormShown(object sender, EventArgs e)
		{
			if(m_cEdit != null) UIUtil.SetFocus(m_cEdit, this, true);
			else { Debug.Assert(false); }
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			GlobalWindowManager.RemoveWindow(this);
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			if(m_cEdit != null) m_strResult = (m_cEdit.Text ?? string.Empty);
			else { Debug.Assert(false); }
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
		}
	}
}
