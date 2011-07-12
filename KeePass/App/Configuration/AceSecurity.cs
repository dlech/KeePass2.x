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
using System.Text;

namespace KeePass.App.Configuration
{
	public sealed class AceSecurity
	{
		public AceSecurity()
		{
		}

		private AceWorkspaceLocking m_wsl = new AceWorkspaceLocking();
		public AceWorkspaceLocking WorkspaceLocking
		{
			get { return m_wsl; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_wsl = value;
			}
		}

		private AppPolicyFlags m_appPolicy = new AppPolicyFlags();
		public AppPolicyFlags Policy
		{
			get { return m_appPolicy; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_appPolicy = value;
			}
		}

		private AceMasterPassword m_mp = new AceMasterPassword();
		public AceMasterPassword MasterPassword
		{
			get { return m_mp; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_mp = value;
			}
		}

		private bool m_bSecureDesktop = false;
		public bool MasterKeyOnSecureDesktop
		{
			get { return m_bSecureDesktop; }
			set { m_bSecureDesktop = value; }
		}

		private bool m_bClipClearOnExit = true;
		public bool ClipboardClearOnExit
		{
			get { return m_bClipClearOnExit; }
			set { m_bClipClearOnExit = value; }
		}

		private int m_nClipClearSeconds = 12;
		public int ClipboardClearAfterSeconds
		{
			get { return m_nClipClearSeconds; }
			set { m_nClipClearSeconds = value; }
		}

		// Disabled by default, because Office's clipboard tools
		// crash with the Clipboard Viewer Ignore format
		// (when it is set using OleSetClipboard)
		private bool m_bUseClipboardViewerIgnoreFmt = false;
		public bool UseClipboardViewerIgnoreFormat
		{
			get { return m_bUseClipboardViewerIgnoreFmt; }
			set { m_bUseClipboardViewerIgnoreFmt = value; }
		}

		private bool m_bClearKeyCmdLineOpt = true;
		public bool ClearKeyCommandLineParams
		{
			get { return m_bClearKeyCmdLineOpt; }
			set { m_bClearKeyCmdLineOpt = value; }
		}
	}

	public sealed class AceWorkspaceLocking
	{
		public AceWorkspaceLocking()
		{
		}

		private bool m_bOnMinimize = false;
		public bool LockOnWindowMinimize
		{
			get { return m_bOnMinimize; }
			set { m_bOnMinimize = value; }
		}

		private bool m_bOnSessionSwitch = false;
		public bool LockOnSessionSwitch
		{
			get { return m_bOnSessionSwitch; }
			set { m_bOnSessionSwitch = value; }
		}

		private bool m_bOnSuspend = false;
		public bool LockOnSuspend
		{
			get { return m_bOnSuspend; }
			set { m_bOnSuspend = value; }
		}

		private bool m_bOnRemoteControlChange = false;
		public bool LockOnRemoteControlChange
		{
			get { return m_bOnRemoteControlChange; }
			set { m_bOnRemoteControlChange = value; }
		}

		private uint m_uLockAfterTime = 0;
		public uint LockAfterTime
		{
			get { return m_uLockAfterTime; }
			set { m_uLockAfterTime = value; }
		}

		private uint m_uLockAfterGlobalTime = 0;
		public uint LockAfterGlobalTime
		{
			get { return m_uLockAfterGlobalTime; }
			set { m_uLockAfterGlobalTime = value; }
		}

		private bool m_bExitInsteadOfLockingAfterTime = false;
		public bool ExitInsteadOfLockingAfterTime
		{
			get { return m_bExitInsteadOfLockingAfterTime; }
			set { m_bExitInsteadOfLockingAfterTime = value; }
		}

		private bool m_bAlwaysExitInsteadOfLocking = false;
		public bool AlwaysExitInsteadOfLocking
		{
			get { return m_bAlwaysExitInsteadOfLocking; }
			set { m_bAlwaysExitInsteadOfLocking = value; }
		}
	}

	public sealed class AceMasterPassword
	{
		public AceMasterPassword()
		{
		}

		private uint m_uMinLength = 0;
		public uint MinimumLength
		{
			get { return m_uMinLength; }
			set { m_uMinLength = value; }
		}

		private uint m_uMinQuality = 0;
		public uint MinimumQuality
		{
			get { return m_uMinQuality; }
			set { m_uMinQuality = value; }
		}
	}
}
