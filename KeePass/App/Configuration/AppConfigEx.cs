/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2008 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Xml.Serialization;

using KeePassLib.Serialization;

namespace KeePass.App.Configuration
{
	[XmlType(TypeName = "Configuration")]
	public sealed class AppConfigEx
	{
		public AppConfigEx()
		{
		}

		private AceMeta m_meta = new AceMeta();
		public AceMeta Meta
		{
			get { return m_meta; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_meta = value;
			}
		}

		private AceApplication m_aceApp = new AceApplication();
		public AceApplication Application
		{
			get { return m_aceApp; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_aceApp = value;
			}
		}

		private AceLogging m_aceLogging = new AceLogging();
		public AceLogging Logging
		{
			get { return m_aceLogging; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_aceLogging = value;
			}
		}

		private AceMainWindow m_uiMainWindow = new AceMainWindow();
		public AceMainWindow MainWindow
		{
			get { return m_uiMainWindow; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_uiMainWindow = value;
			}
		}

		private AceUI m_aceUI = new AceUI();
		public AceUI UI
		{
			get { return m_aceUI; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_aceUI = value;
			}
		}

		private AceSecurity m_sec = new AceSecurity();
		public AceSecurity Security
		{
			get { return m_sec; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_sec = value;
			}
		}

		private AceNative m_native =  new AceNative();
		public AceNative Native
		{
			get { return m_native; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_native = value;
			}
		}

		private AcePasswordGenerator m_pwGen = new AcePasswordGenerator();
		public AcePasswordGenerator PasswordGenerator
		{
			get { return m_pwGen; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_pwGen = value;
			}
		}

		private AceDefaults m_def = new AceDefaults();
		public AceDefaults Defaults
		{
			get { return m_def; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_def = value;
			}
		}

		private AceIntegration m_int = new AceIntegration();
		public AceIntegration Integration
		{
			get { return m_int; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_int = value;
			}
		}

		public void PrepareSave()
		{
			m_aceApp.LastUsedFile.ClearCredentials(true);

			foreach(IOConnectionInfo iocMru in m_aceApp.MostRecentlyUsed.Items)
				iocMru.ClearCredentials(true);
		}
	}

	public sealed class AceMeta
	{
		public AceMeta()
		{
		}

		private bool m_bPrefLocalCfg = false;
		public bool PreferUserConfiguration
		{
			get { return m_bPrefLocalCfg; }
			set { m_bPrefLocalCfg = value; }
		}

		private bool m_bIsEnforced = false;
		[XmlIgnore]
		public bool IsEnforcedConfiguration
		{
			get { return m_bIsEnforced; }
			set { m_bIsEnforced = value; }
		}
	}
}
