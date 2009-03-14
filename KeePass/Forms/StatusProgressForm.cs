/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2009 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePassLib;
using KeePassLib.Interfaces;

namespace KeePass.Forms
{
	public partial class StatusProgressForm : Form, IStatusLogger
	{
		private volatile bool m_bCancelled = false;

		public bool UserCancelled
		{
			get { return m_bCancelled; }
		}

		public StatusProgressForm()
		{
			InitializeComponent();
			Program.Translation.ApplyTo(this);
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			m_pbTotal.Minimum = 0;
			m_pbTotal.Maximum = 100;
			m_pbTotal.Value = 0;

			this.Text = PwDefs.ShortProductName;
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
			if(m_bCancelled) { Debug.Assert(false); return; }

			m_bCancelled = true;
			m_btnCancel.Enabled = false;
		}

		public delegate void Priv_SetProgressInternal(string strText, int nPercent);

		private void SetProgressInternal(string strText, int nPercent)
		{
			Debug.Assert(!m_lblTotal.InvokeRequired);
			Debug.Assert(!m_pbTotal.InvokeRequired);

			if(strText != null) m_lblTotal.Text = strText;

			if((nPercent >= 0) && (nPercent <= 100)) m_pbTotal.Value = nPercent;
		}

		private bool SetProgressGlobal(string strText, int nPercent)
		{
			if(this.InvokeRequired)
				this.Invoke(new Priv_SetProgressInternal(this.SetProgressInternal),
					strText, nPercent);
			else SetProgressInternal(strText, nPercent);

			Application.DoEvents();
			return !m_bCancelled;
		}

		public void StartLogging(string strOperation, bool bWriteOperationToLog)
		{
			SetProgressGlobal(strOperation, -1);
		}

		public void EndLogging()
		{
		}

		public bool SetProgress(uint uPercent)
		{
			return SetProgressGlobal(null, (int)uPercent);
		}

		public bool SetText(string strNewText, LogStatusType lsType)
		{
			return SetProgressGlobal(strNewText, -1);
		}

		public bool ContinueWork()
		{
			Application.DoEvents();
			return !m_bCancelled;
		}
	}
}
