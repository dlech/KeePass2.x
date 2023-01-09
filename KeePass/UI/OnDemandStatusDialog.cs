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
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using KeePass.Forms;

using KeePassLib.Interfaces;
using KeePassLib.Utility;

namespace KeePass.UI
{
	public sealed class OnDemandStatusDialog : IStatusLogger
	{
		private readonly bool m_bUseThread;
		private readonly Form m_fOwner;

		private Thread m_th = null;
		private StatusProgressForm m_dlgModal = null;
		private readonly object m_objSync = new object();

		private const uint InitialProgress = 0;
		private const string InitialStatus = null;

		private volatile string m_strTitle = null;
		private volatile bool m_bTerminate = false;
		private volatile uint m_uProgress = InitialProgress;
		private volatile string m_strProgress = InitialStatus;

		public OnDemandStatusDialog(bool bUseThread, Form fOwner)
		{
			m_bUseThread = bUseThread;
			m_fOwner = fOwner;
		}

		public void StartLogging(string strOperation, bool bWriteOperationToLog)
		{
			m_strTitle = strOperation;
		}

		public void EndLogging()
		{
			lock(m_objSync) { m_bTerminate = true; }
			m_th = null;

			if(m_dlgModal != null)
			{
				DestroyStatusDialog(m_dlgModal);
				m_dlgModal = null;
			}
		}

		public bool SetProgress(uint uPercent)
		{
			lock(m_objSync) { m_uProgress = uPercent; }

			return ((m_dlgModal != null) ? m_dlgModal.SetProgress(uPercent) : true);
		}

		public bool SetText(string strNewText, LogStatusType lsType)
		{
			if(strNewText == null) return true;
			if(lsType != LogStatusType.Info) return true;

			if(m_bUseThread && (m_th == null))
			{
				ThreadStart ts = new ThreadStart(this.GuiThread);
				m_th = new Thread(ts);
				m_th.Start();
			}
			if(!m_bUseThread && (m_dlgModal == null))
				m_dlgModal = ConstructStatusDialog();

			lock(m_objSync) { m_strProgress = strNewText; }
			return ((m_dlgModal != null) ? m_dlgModal.SetText(strNewText, lsType) : true);
		}

		public bool ContinueWork()
		{
			return ((m_dlgModal != null) ? m_dlgModal.ContinueWork() : true);
		}

		private void GuiThread()
		{
			uint uProgress = InitialProgress;
			string strProgress = InitialStatus;

			StatusProgressForm dlg = null;
			while(true)
			{
				lock(m_objSync)
				{
					if(m_bTerminate) break;

					if(m_uProgress != uProgress)
					{
						uProgress = m_uProgress;
						if(dlg != null) dlg.SetProgress(uProgress);
					}

					if(m_strProgress != strProgress)
					{
						strProgress = m_strProgress;

						if(dlg == null) dlg = ConstructStatusDialog();

						dlg.SetText(strProgress, LogStatusType.Info);
					}
				}

				Application.DoEvents();
			}

			DestroyStatusDialog(dlg);
		}

		private StatusProgressForm ConstructStatusDialog()
		{
			StatusProgressForm dlg = StatusProgressForm.ConstructEx(
				m_strTitle, false, true, (m_bUseThread ? null : m_fOwner), null);
			dlg.SetProgress(m_uProgress);

			MainForm mfOwner = ((m_fOwner != null) ? (m_fOwner as MainForm) : null);
			if(!m_bUseThread && (mfOwner != null))
			{
				// mfOwner.RedirectActivationPush(dlg);
				mfOwner.UIBlockInteraction(true);
			}

			return dlg;
		}

		private void DestroyStatusDialog(StatusProgressForm dlg)
		{
			if(dlg != null)
			{
				MainForm mfOwner = ((m_fOwner != null) ? (m_fOwner as MainForm) : null);
				if(!m_bUseThread && (mfOwner != null))
				{
					// mfOwner.RedirectActivationPop();
					mfOwner.UIBlockInteraction(false);
				}

				StatusProgressForm.DestroyEx(dlg);

				// Conflict with 3116455
				// if(mfOwner != null) mfOwner.Activate(); // Prevent disappearing
			}
		}
	}

	public sealed class UIBlockerStatusLogger : IStatusLogger
	{
		private MainForm m_mf;

		private string m_strText = string.Empty;
		private int m_tLastAnim = Environment.TickCount;
		private int m_cDots = 1;

		public UIBlockerStatusLogger(Form fParent)
		{
			m_mf = (fParent as MainForm);
		}

		public void StartLogging(string strOperation, bool bWriteOperationToLog)
		{
			if(m_mf != null)
			{
				TaskbarList.SetProgressState(m_mf, TbpFlag.Indeterminate);
				m_mf.UIBlockInteraction(true);
			}
		}

		public void EndLogging()
		{
			if(m_mf != null)
			{
				m_mf.UIBlockInteraction(false);
				TaskbarList.SetProgressState(m_mf, TbpFlag.NoProgress);
			}
		}

		public bool SetProgress(uint uPercent)
		{
			Animate();
			return true;
		}

		public bool SetText(string strNewText, LogStatusType lsType)
		{
			if((m_mf != null) && !string.IsNullOrEmpty(strNewText))
			{
				m_strText = strNewText;
				m_mf.SetStatusEx(strNewText);

				UIUtil.DoEventsByTime(true);
			}
			else UIUtil.DoEventsByTime(false);

			return true;
		}

		public bool ContinueWork()
		{
			Animate();
			return true;
		}

		private void Animate()
		{
			int t = Environment.TickCount, tLast = m_tLastAnim;
			int d = t - tLast;

			if(d >= 1000)
			{
				m_tLastAnim = t;

				if(m_mf != null)
				{
					string strDots = new string('.', m_cDots);
					m_mf.SetStatusEx(StrUtil.TrimDots(m_strText, false) + strDots);
					m_cDots = (m_cDots % 5) + 1; // At least one dot

					UIUtil.DoEventsByTime(true);
					return;
				}
			}

			UIUtil.DoEventsByTime(false);
		}
	}
}
