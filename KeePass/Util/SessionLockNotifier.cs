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
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using Microsoft.Win32;

namespace KeePass.Util
{
	public sealed class SessionLockNotifier
	{
		private bool m_bEventsRegistered = false;
		private EventHandler m_evHandler = null;

		private bool m_bNotifyOnSuspend;

		public SessionLockNotifier(bool bNotifyOnSuspend)
		{
			m_bNotifyOnSuspend = bNotifyOnSuspend;
		}

#if DEBUG
		~SessionLockNotifier()
		{
			Debug.Assert(m_bEventsRegistered == false);
		}
#endif

		public void Install(EventHandler ev)
		{
			this.Uninstall();

			try
			{
				SystemEvents.SessionEnding += this.OnSessionEnding;
				SystemEvents.SessionSwitch += this.OnSessionSwitch;

				if(m_bNotifyOnSuspend)
					SystemEvents.PowerModeChanged += this.OnPowerModeChanged;
			}
			catch(Exception) { Debug.Assert(WinUtil.IsWindows2000); } // 2000 always fails

			m_bEventsRegistered = true;

			m_evHandler = ev;
		}

		public void Uninstall()
		{
			if(m_bEventsRegistered)
			{
				try
				{
					SystemEvents.SessionEnding -= this.OnSessionEnding;
					SystemEvents.SessionSwitch -= this.OnSessionSwitch;

					if(m_bNotifyOnSuspend)
						SystemEvents.PowerModeChanged -= this.OnPowerModeChanged;
				}
				catch(Exception) { Debug.Assert(WinUtil.IsWindows2000); } // 2000 always fails

				m_bEventsRegistered = false;
			}
		}

		private void OnSessionEnding(object sender, SessionEndingEventArgs e)
		{
			if(m_evHandler != null) m_evHandler(sender, e);
		}

		private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
		{
			if(m_evHandler != null) m_evHandler(sender, e);
		}

		private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
		{
			if((m_evHandler != null) && (e.Mode == PowerModes.Suspend))
				m_evHandler(sender, e);
		}
	}
}
