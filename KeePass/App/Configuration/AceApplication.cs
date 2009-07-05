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
using System.Xml.Serialization;

using KeePass.Ecas;

using KeePassLib.Serialization;

namespace KeePass.App.Configuration
{
	public sealed class AceApplication
	{
		public AceApplication()
		{
		}

		private string m_strLanguageFile = string.Empty; // = English
		public string LanguageFile
		{
			get { return m_strLanguageFile; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strLanguageFile = value;
			}
		}

		private bool m_bHelpUseLocal = false;
		public bool HelpUseLocal
		{
			get { return m_bHelpUseLocal; }
			set { m_bHelpUseLocal = value; }
		}

		private IOConnectionInfo m_ioLastDb = new IOConnectionInfo();
		public IOConnectionInfo LastUsedFile
		{
			get { return m_ioLastDb; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_ioLastDb = value;
			}
		}

		private AceMru m_mru = new AceMru();
		public AceMru MostRecentlyUsed
		{
			get { return m_mru; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_mru = value;
			}
		}

		private AceStartUp m_su = new AceStartUp();
		public AceStartUp Start
		{
			get { return m_su; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_su = value;
			}
		}

		private AceOpenDb m_fo = new AceOpenDb();
		public AceOpenDb FileOpening
		{
			get { return m_fo; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_fo = value;
			}
		}

		private AceCloseDb m_fc = new AceCloseDb();
		public AceCloseDb FileClosing
		{
			get { return m_fc; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_fc = value;
			}
		}

		private EcasTriggerSystem m_triggers = new EcasTriggerSystem();
		public EcasTriggerSystem TriggerSystem
		{
			get { return m_triggers; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_triggers = value;
			}
		}
	}

	public sealed class AceStartUp
	{
		public AceStartUp()
		{
		}

		private bool m_bOpenLastDb = true;
		public bool OpenLastFile
		{
			get { return m_bOpenLastDb; }
			set { m_bOpenLastDb = value; }
		}

		private bool m_bCheckForUpdate = false;
		public bool CheckForUpdate
		{
			get { return m_bCheckForUpdate; }
			set { m_bCheckForUpdate = value; }
		}

		private bool m_bMinimizedAndLocked = false;
		public bool MinimizedAndLocked
		{
			get { return m_bMinimizedAndLocked; }
			set { m_bMinimizedAndLocked = value; }
		}
	}

	public sealed class AceOpenDb
	{
		public AceOpenDb()
		{
		}

		private bool m_bShowExpiredEntries = false;
		public bool ShowExpiredEntries
		{
			get { return m_bShowExpiredEntries; }
			set { m_bShowExpiredEntries = value; }
		}

		private bool m_bShowSoonToExpireEntries = false;
		public bool ShowSoonToExpireEntries
		{
			get { return m_bShowSoonToExpireEntries; }
			set { m_bShowSoonToExpireEntries = value; }
		}
	}

	public sealed class AceCloseDb
	{
		public AceCloseDb()
		{
		}

		private bool m_bAutoSave = false;
		public bool AutoSave
		{
			get { return m_bAutoSave; }
			set { m_bAutoSave = value; }
		}
	}

	public sealed class AceMru
	{
		public const uint DefaultMaxItemCount = 12;

		public AceMru()
		{
		}

		private uint m_uMaxItems = DefaultMaxItemCount;
		public uint MaxItemCount
		{
			get { return m_uMaxItems; }
			set { m_uMaxItems = value; }
		}

		private List<IOConnectionInfo> m_vItems = new List<IOConnectionInfo>();
		[XmlArrayItem("ConnectionInfo")]
		public List<IOConnectionInfo> Items
		{
			get { return m_vItems; }
			set { m_vItems = value; }
		}
	}
}
