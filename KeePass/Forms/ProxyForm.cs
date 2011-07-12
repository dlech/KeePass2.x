/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2011 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using KeePass.App.Configuration;
using KeePass.UI;

using KeePassLib;
using KeePassLib.Serialization;

namespace KeePass.Forms
{
	public partial class ProxyForm : Form
	{
		public ProxyForm()
		{
			InitializeComponent();
			Program.Translation.ApplyTo(this);
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			GlobalWindowManager.AddWindow(this);

			this.Icon = Properties.Resources.KeePass;

			ProxyServerType pst = Program.Config.Integration.ProxyType;
			if(pst == ProxyServerType.None) m_rbNoProxy.Checked = true;
			else if(pst == ProxyServerType.Manual) m_rbManualProxy.Checked = true;
			else m_rbSystemProxy.Checked = true;

			m_tbAddress.Text = Program.Config.Integration.ProxyAddress;
			m_tbPort.Text = Program.Config.Integration.ProxyPort;

			m_tbUser.Text = Program.Config.Integration.ProxyUserName;
			m_tbPassword.Text = Program.Config.Integration.ProxyPassword;
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			GlobalWindowManager.RemoveWindow(this);
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			ProxyServerType pst = ProxyServerType.System;
			if(m_rbNoProxy.Checked) pst = ProxyServerType.None;
			else if(m_rbManualProxy.Checked) pst = ProxyServerType.Manual;

			AceIntegration ace = Program.Config.Integration;
			ace.ProxyType = pst;
			ace.ProxyAddress = m_tbAddress.Text;
			ace.ProxyPort = m_tbPort.Text;
			ace.ProxyUserName = m_tbUser.Text;
			ace.ProxyPassword = m_tbPassword.Text;

			IOConnection.SetProxy(pst, ace.ProxyAddress, ace.ProxyPort,
				ace.ProxyUserName, ace.ProxyPassword);
		}

		private void EnableControlsEx()
		{
			if(m_rbNoProxy.Checked)
			{
				m_tbUser.Enabled = m_tbPassword.Enabled =
					m_lblUser.Enabled = m_lblPassword.Enabled = false;
				m_grpAuth.Enabled = false;
			}
			else
			{
				m_grpAuth.Enabled = true;
				m_tbUser.Enabled = m_tbPassword.Enabled =
					m_lblUser.Enabled = m_lblPassword.Enabled = true;
			}

			m_lblAddress.Enabled = m_tbAddress.Enabled = m_lblPort.Enabled =
				m_tbPort.Enabled = m_rbManualProxy.Checked;

			if(!m_rbManualProxy.Checked) m_btnOK.Enabled = true;
			else m_btnOK.Enabled = (m_tbAddress.Text.Length > 0);
		}

		private void OnNoProxyCheckedChanged(object sender, EventArgs e)
		{
			EnableControlsEx();
		}

		private void OnSystemProxyCheckedChanged(object sender, EventArgs e)
		{
			EnableControlsEx();
		}

		private void OnManualProxyCheckedChanged(object sender, EventArgs e)
		{
			EnableControlsEx();
		}

		private void OnAddressTextChanged(object sender, EventArgs e)
		{
			EnableControlsEx();
		}
	}
}
