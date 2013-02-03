/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2013 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePass.Resources;
using KeePass.UI;

using KeePassLib;
using KeePassLib.Interfaces;

namespace KeePass.Forms
{
	public partial class StatusLoggerForm : Form, IStatusLogger
	{
		private bool m_bIsModal = false;
		private bool m_bCancelled = false;
		private bool m_bCloseMode = false;
		private uint m_uLastPercent = 0;

		private uint uWarnings = 0;
		private uint uErrors = 0;

		public void InitEx(bool bIsModal)
		{
			m_bIsModal = bIsModal;
		}

		public void StartLogging(string strOperation, bool bWriteOperationToLog)
		{
			if(strOperation != null)
			{
				this.Text = PwDefs.ShortProductName + " - " + strOperation;
				
				if(bWriteOperationToLog)
					this.SetText(strOperation, LogStatusType.Info);
			}

			m_pbProgress.Value = 0;
			m_uLastPercent = 0;
		}

		public void EndLogging()
		{
			m_btnCancel.Text = KPRes.CloseButton;
			m_bCloseMode = true;

			this.SetText(string.Empty, LogStatusType.AdditionalInfo);

			string strFinish = KPRes.Ready + " " + uErrors.ToString() + " " + KPRes.Errors +
				", " + uWarnings.ToString() + " " + KPRes.Warnings + ".";
			this.SetText(strFinish, LogStatusType.Info);

			m_pbProgress.Value = 100;
			m_uLastPercent = 100;

			Application.DoEvents();
		}

		public bool SetProgress(uint uPercent)
		{
			if(uPercent != m_uLastPercent)
			{
				m_pbProgress.Value = (int)uPercent;
				m_uLastPercent = uPercent;

				Application.DoEvents();
			}

			return !m_bCancelled;
		}

		public bool SetText(string strNewText, LogStatusType lsType)
		{
			if(strNewText != null)
			{
				m_lvMessages.Items.Add(strNewText, (int)lsType);
				m_lvMessages.EnsureVisible(m_lvMessages.Items.Count - 1);
			}

			if(lsType == LogStatusType.Warning) ++uWarnings;
			else if(lsType == LogStatusType.Error) ++uErrors;

			ProcessResize();
			Application.DoEvents();
			return !m_bCancelled;
		}

		public bool ContinueWork()
		{
			Application.DoEvents();

			return !m_bCancelled;
		}

		public StatusLoggerForm()
		{
			InitializeComponent();
			Program.Translation.ApplyTo(this);
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			GlobalWindowManager.AddWindow(this);

			this.Icon = Properties.Resources.KeePass;
			this.Text = PwDefs.ShortProductName;
			
			m_pbProgress.Minimum = 0;
			m_pbProgress.Maximum = 100;

			int nWidth = m_lvMessages.ClientRectangle.Width - 1;
			m_lvMessages.Columns.Add("<>", nWidth);

			ProcessResize();
		}

		private void ProcessResize()
		{
			if(m_lvMessages.Columns.Count > 0)
			{
				int nColumnWidth = m_lvMessages.ClientRectangle.Width - 2;
				m_lvMessages.Columns[0].Width = nColumnWidth;
			}
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
			if(m_bCloseMode)
			{
				if(m_bIsModal) this.DialogResult = DialogResult.Cancel;
				else Close();
			}
			else
			{
				m_bCancelled = true;
				this.DialogResult = DialogResult.None;
			}
		}

		private void OnMessagesSelectedIndexChanged(object sender, EventArgs e)
		{
			ListView.SelectedListViewItemCollection slvic = m_lvMessages.SelectedItems;
			if(slvic.Count == 0)
			{
				m_tbDetails.Text = string.Empty;
				return;
			}

			UIUtil.SetMultilineText(m_tbDetails, slvic[0].Text);
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			GlobalWindowManager.RemoveWindow(this);
		}
	}
}
