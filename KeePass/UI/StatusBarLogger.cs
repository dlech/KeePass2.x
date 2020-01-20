/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2020 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Windows.Forms;

using KeePassLib.Interfaces;

namespace KeePass.UI
{
	public sealed class StatusBarLogger : IStatusLogger
	{
		private ToolStripStatusLabel m_sbText = null;
		private ToolStripProgressBar m_pbProgress = null;

		private bool m_bActive = false;
		private uint m_uLastPercent = 0;

#if DEBUG
		~StatusBarLogger()
		{
			Debug.Assert(!m_bActive);
		}
#endif

		public void SetControls(ToolStripStatusLabel sbText, ToolStripProgressBar pbProgress)
		{
			Debug.Assert(!m_bActive);

			m_sbText = sbText;

			m_pbProgress = pbProgress;
			if(pbProgress != null)
			{
				if(pbProgress.Minimum != 0) pbProgress.Minimum = 0;
				if(pbProgress.Maximum != 100) pbProgress.Maximum = 100;
			}
		}

		private void SetStyle(ProgressBarStyle s)
		{
			try { m_pbProgress.Style = s; }
			catch(Exception) { Debug.Assert(false); }
		}

		public void StartLogging(string strOperation, bool bWriteOperationToLog)
		{
			Debug.Assert(!m_bActive);

			m_bActive = true;
			m_uLastPercent = 0;

			if(m_pbProgress != null)
			{
				m_pbProgress.Value = 0;
				SetStyle(ProgressBarStyle.Marquee);
				m_pbProgress.Visible = true;
			}

			SetText(strOperation, LogStatusType.Info);
		}

		public void EndLogging()
		{
			Debug.Assert(m_bActive);

			if(m_pbProgress != null)
			{
				m_pbProgress.Visible = false;
				SetStyle(ProgressBarStyle.Continuous);
				m_pbProgress.Value = 100;
			}

			m_uLastPercent = 100;
			m_bActive = false;
		}

		public bool SetProgress(uint uPercent)
		{
			Debug.Assert(m_bActive);

			if(m_pbProgress != null)
			{
				if(uPercent != m_uLastPercent)
				{
					m_pbProgress.Value = (int)uPercent;
					m_uLastPercent = uPercent;

					SetStyle((uPercent == 0) ? ProgressBarStyle.Marquee :
						ProgressBarStyle.Continuous);
					UIUtil.DoEventsByTime(true);
				}
				else UIUtil.DoEventsByTime(false);
			}

			return true;
		}

		public bool SetText(string strNewText, LogStatusType lsType)
		{
			Debug.Assert(m_bActive);
			
			if((m_sbText != null) && (lsType == LogStatusType.Info))
			{
				m_sbText.Text = (strNewText ?? string.Empty);
				UIUtil.DoEventsByTime(true);
			}

			return true;
		}

		public bool ContinueWork()
		{
			Debug.Assert(m_bActive);

			UIUtil.DoEventsByTime(false);
			return true;
		}
	}
}
