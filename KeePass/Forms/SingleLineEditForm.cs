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

using KeePass.UI;

namespace KeePass.Forms
{
	public partial class SingleLineEditForm : Form
	{
		private string m_strTitle = string.Empty;
		private string m_strDesc = string.Empty;
		private string m_strLongDesc = string.Empty;
		private Image m_imgIcon = null;
		private string m_strResultString = string.Empty;

		public string ResultString
		{
			get { return m_strResultString; }
		}

		public SingleLineEditForm()
		{
			InitializeComponent();
		}

		public void InitEx(string strTitle, string strDesc, string strLongDesc, Image imgIcon)
		{
			m_strTitle = strTitle;
			m_strDesc = strDesc;
			m_strLongDesc = strLongDesc;
			m_imgIcon = imgIcon;
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			m_bannerImage.Image = BannerFactory.CreateBanner(m_bannerImage.Width,
				m_bannerImage.Height, BannerFactory.BannerStyle.Default,
				m_imgIcon, m_strTitle, m_strDesc);
			this.Icon = Properties.Resources.KeePass;

			this.Text = m_strTitle;
			m_lblLongDesc.Text = m_strLongDesc;

			m_tbEdit.Focus();
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			m_strResultString = m_tbEdit.Text;
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
		}
	}
}