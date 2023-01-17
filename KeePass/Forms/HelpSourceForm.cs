/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2023 Dominik Reichl <dominik.reichl@t-online.de>

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
using KeePass.App.Configuration;
using KeePass.Resources;
using KeePass.UI;

namespace KeePass.Forms
{
	public partial class HelpSourceForm : Form, IGwmWindow
	{
		public bool CanCloseWithoutDataLoss { get { return true; } }

		public HelpSourceForm()
		{
			InitializeComponent();
			GlobalWindowManager.InitializeForm(this);
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			GlobalWindowManager.AddWindow(this, this);

			BannerFactory.CreateBannerEx(this, m_bannerImage,
				Properties.Resources.B48x48_Folder_Download, KPRes.HelpSourceSelection,
				KPRes.HelpSourceSelectionDesc);
			this.Icon = AppIcons.Default;
			this.Text = KPRes.HelpSourceSelection;

			FontUtil.AssignDefaultBold(m_radioLocal);
			FontUtil.AssignDefaultBold(m_radioOnline);

			Debug.Assert(!m_lblLocal.AutoSize); // For RTL support
			if(!AppHelp.LocalHelpAvailable)
			{
				UIUtil.SetEnabledFast(false, m_radioLocal, m_lblLocal);
				m_lblLocal.Text = KPRes.HelpSourceNoLocalOption;

				AppHelp.PreferredHelpSource = AppHelpSource.Online;
			}

			bool bOverride = !string.IsNullOrEmpty(Program.Config.Application.HelpUrl);
			bool bEnforced = AppConfigEx.IsOptionEnforced(Program.Config.Application,
				"HelpUseLocal");

			if(!bOverride)
			{
				if(AppHelp.PreferredHelpSource == AppHelpSource.Local)
					m_radioLocal.Checked = true;
				else
					m_radioOnline.Checked = true;
			}

			if(bOverride || bEnforced)
			{
				UIUtil.SetEnabledFast(false, m_radioLocal, m_lblLocal,
					m_radioOnline, m_lblOnline, m_btnOK);
				UIUtil.SetFocus(m_btnCancel, this);
			}
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			if(m_radioLocal.Checked)
				AppHelp.PreferredHelpSource = AppHelpSource.Local;
			else
				AppHelp.PreferredHelpSource = AppHelpSource.Online;
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			GlobalWindowManager.RemoveWindow(this);
		}
	}
}
