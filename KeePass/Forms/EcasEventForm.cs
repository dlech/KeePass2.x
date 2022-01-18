/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2022 Dominik Reichl <dominik.reichl@t-online.de>

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
using KeePass.Ecas;
using KeePass.Resources;
using KeePass.UI;

namespace KeePass.Forms
{
	public partial class EcasEventForm : Form
	{
		private EcasEvent m_eventInOut = null;
		private EcasEvent m_event = null; // Working copy

		private bool m_bBlockTypeSelectionHandler = false;

		public void InitEx(EcasEvent e)
		{
			m_eventInOut = e;
			m_event = e.CloneDeep();
		}

		public EcasEventForm()
		{
			InitializeComponent();
			GlobalWindowManager.InitializeForm(this);
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			GlobalWindowManager.AddWindow(this);

			this.Text = KPRes.Event;
			this.Icon = AppIcons.Default;

			Debug.Assert(!m_lblParamHint.AutoSize); // For RTL support
			m_lblParamHint.Text = KPRes.ParamDescHelp;

			foreach(EcasEventProvider ep in Program.EcasPool.EventProviders)
			{
				foreach(EcasEventType t in ep.Events)
					m_cmbEvents.Items.Add(t.Name);
			}

			UpdateDataEx(m_event, false, EcasTypeDxMode.Selection);
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			GlobalWindowManager.RemoveWindow(this);
		}

		private bool UpdateDataEx(EcasEvent e, bool bGuiToInternal,
			EcasTypeDxMode dxType)
		{
			m_bBlockTypeSelectionHandler = true;
			bool bResult = EcasUtil.UpdateDialog(EcasObjectType.Event, m_cmbEvents,
				m_dgvParams, e, bGuiToInternal, dxType);
			m_bBlockTypeSelectionHandler = false;
			return bResult;
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			if(!UpdateDataEx(m_eventInOut, true, EcasTypeDxMode.Selection))
				this.DialogResult = DialogResult.None;
			else m_eventInOut.RunAtTicks = -1;
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
		}

		private void OnEventsSelectedIndexChanged(object sender, EventArgs e)
		{
			if(m_bBlockTypeSelectionHandler) return;

			UpdateDataEx(m_event, true, EcasTypeDxMode.ParamsTag);
			UpdateDataEx(m_event, false, EcasTypeDxMode.None);
		}

		private void OnBtnHelp(object sender, EventArgs e)
		{
			AppHelp.ShowHelp(AppDefs.HelpTopics.Triggers, AppDefs.HelpTopics.TriggersEvents);
		}
	}
}
