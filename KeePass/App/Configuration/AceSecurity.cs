/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2025 Dominik Reichl <dominik.reichl@t-online.de>

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

		private int m_nMasterKeyTries = 3;
		[DefaultValue(3)]
		public int MasterKeyTries
		{
			get { return m_nMasterKeyTries; }
			set { m_nMasterKeyTries = value; }
		}

		[DefaultValue(false)]
		public bool MasterKeyOnSecureDesktop { get; set; }

		private string m_strMasterKeyExpiryRec = string.Empty;
		[DefaultValue("")]
		public string MasterKeyExpiryRec
		{
			get { return m_strMasterKeyExpiryRec; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strMasterKeyExpiryRec = value;
			}
		}

		private string m_strMasterKeyExpiryForce = string.Empty;
		[DefaultValue("")]
		public string MasterKeyExpiryForce
		{
			get { return m_strMasterKeyExpiryForce; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strMasterKeyExpiryForce = value;
			}
		}

		private bool m_bKeyTrfWeakWarning = true;
		[DefaultValue(true)]
		public bool KeyTransformWeakWarning
		{
			get { return m_bKeyTrfWeakWarning; }
			set { m_bKeyTrfWeakWarning = value; }
		}

		private bool m_bClipClearOnExit = true;
		[DefaultValue(true)]
		public bool ClipboardClearOnExit
		{
			get { return m_bClipClearOnExit; }
			set { m_bClipClearOnExit = value; }
		}

		private int m_nClipClearSeconds = 12;
		[DefaultValue(12)]
		public int ClipboardClearAfterSeconds
		{
			get { return m_nClipClearSeconds; }
			set { m_nClipClearSeconds = value; }
		}

		private bool m_bClipNoPersist = true;
		[DefaultValue(true)]
		public bool ClipboardNoPersist
		{
			get { return m_bClipNoPersist; }
			set { m_bClipNoPersist = value; }
		}

		// The clipboard tools of old Office versions crash when
		// storing the 'Clipboard Viewer Ignore' format using the
		// OleSetClipboard function.
		// Therefore, the default value of the option to use this
		// format should be true if and only if KeePass uses the
		// SetClipboardData function only (i.e. no OLE).
		// Note that the .NET Framework and the UWP seem to use
		// OLE internally.
		private bool m_bUseClipboardViewerIgnoreFmt = true;
		[DefaultValue(true)]
		public bool UseClipboardViewerIgnoreFormat
		{
			get { return m_bUseClipboardViewerIgnoreFmt; }
			set { m_bUseClipboardViewerIgnoreFmt = value; }
		}

		private bool m_bClearKeyCmdLineOpt = true;
		[DefaultValue(true)]
		public bool ClearKeyCommandLineParams
		{
			get { return m_bClearKeyCmdLineOpt; }
			set { m_bClearKeyCmdLineOpt = value; }
		}

		[DefaultValue(false)]
		public bool SslCertsAcceptInvalid { get; set; }

		[DefaultValue(false)]
		public bool PreventScreenCapture { get; set; }

		// https://keepass.info/help/v2_dev/customize.html#opt
		[DefaultValue(false)]
		public bool ProtectProcessWithDacl { get; set; }
	}

	public sealed class AceWorkspaceLocking
	{
		public AceWorkspaceLocking()
		{
		}

		[DefaultValue(false)]
		public bool LockOnWindowMinimize { get; set; }

		[DefaultValue(false)]
		public bool LockOnWindowMinimizeToTray { get; set; }

		[DefaultValue(false)]
		public bool LockOnSessionSwitch { get; set; }

		[DefaultValue(false)]
		public bool LockOnSuspend { get; set; }

		[DefaultValue(false)]
		public bool LockOnRemoteControlChange { get; set; }

		public uint LockAfterTime { get; set; }
		public uint LockAfterGlobalTime { get; set; }

		[DefaultValue(false)]
		public bool ExitInsteadOfLockingAfterTime { get; set; }

		[DefaultValue(false)]
		public bool AlwaysExitInsteadOfLocking { get; set; }
	}

	public sealed class AceMasterPassword
	{
		public AceMasterPassword()
		{
		}

		public uint MinimumLength { get; set; }
		public uint MinimumQuality { get; set; }

		private bool m_bRememberWhileOpen = true;
		[DefaultValue(true)]
		public bool RememberWhileOpen
		{
			get { return m_bRememberWhileOpen; }
			set { m_bRememberWhileOpen = value; }
		}
	}
}
