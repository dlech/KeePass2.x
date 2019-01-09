/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2019 Dominik Reichl <dominik.reichl@t-online.de>

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
using KeePass.Resources;
using KeePass.UI;

using KeePassLib;

namespace KeePass.Forms
{
	public partial class WebDocForm : Form
	{
		private string m_strTitle = PwDefs.ShortProductName;
		private string m_strDocHtml = null;

		private uint m_uBlockEvents = 1; // For initial navigation

		private string m_strResultUri = null;
		public string ResultUri
		{
			get { return m_strResultUri; }
		}

		public WebDocForm()
		{
			InitializeComponent();
			Program.Translation.ApplyTo(this);
		}

		public void InitEx(string strTitle, string strDocHtml)
		{
			if(!string.IsNullOrEmpty(strTitle)) m_strTitle = strTitle;

			m_strDocHtml = strDocHtml;
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			GlobalWindowManager.AddWindow(this);

			this.Icon = AppIcons.Default;
			this.Text = m_strTitle;

			UIUtil.SetWebBrowserDocument(m_wbMain, m_strDocHtml);

			m_uBlockEvents = 0;
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			GlobalWindowManager.RemoveWindow(this);
		}

		private void OnWebNavigating(object sender, WebBrowserNavigatingEventArgs e)
		{
			if(e == null) { Debug.Assert(false); return; }
			if(m_uBlockEvents != 0) return;

			e.Cancel = true;

			Uri uri = e.Url;
			if(uri != null)
			{
				m_strResultUri = uri.ToString();
				this.DialogResult = DialogResult.OK;
			}
			else { Debug.Assert(false); }
		}
	}
}
