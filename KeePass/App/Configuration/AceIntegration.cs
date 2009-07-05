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
using System.Xml.Serialization;

namespace KeePass.App.Configuration
{
	public sealed class AceIntegration
	{
		public AceIntegration()
		{
		}

		private ulong m_hkAutoType = (ulong)(Keys.Control | Keys.Alt | Keys.A);
		public ulong HotKeyGlobalAutoType
		{
			get { return m_hkAutoType; }
			set { m_hkAutoType = value; }
		}

		private ulong m_hkShowWindow = (ulong)(Keys.Control | Keys.Alt | Keys.K);
		public ulong HotKeyShowWindow
		{
			get { return m_hkShowWindow; }
			set { m_hkShowWindow = value; }
		}

		private ulong m_hkEntryMenu = (ulong)(Keys.None);
		public ulong HotKeyEntryMenu
		{
			get { return m_hkEntryMenu; }
			set { m_hkEntryMenu = value; }
		}

		private string m_strUrlOverride = string.Empty;
		public string UrlOverride
		{
			get { return m_strUrlOverride; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strUrlOverride = value;
			}
		}

		private bool m_bSearchKeyFilesOnRemovable = false;
		public bool SearchKeyFilesOnRemovableMedia
		{
			get { return m_bSearchKeyFilesOnRemovable; }
			set { m_bSearchKeyFilesOnRemovable = value; }
		}

		private bool m_bSingleInstance = true;
		public bool LimitToSingleInstance
		{
			get { return m_bSingleInstance; }
			set { m_bSingleInstance = value; }
		}

		private bool m_bPrependInitSeqIE = true;
		public bool AutoTypePrependInitSequenceForIE
		{
			get { return m_bPrependInitSeqIE; }
			set { m_bPrependInitSeqIE = value; }
		}

		private bool m_bSpecialReleaseAlt = true;
		public bool AutoTypeReleaseAltWithKeyPress
		{
			get { return m_bSpecialReleaseAlt; }
			set { m_bSpecialReleaseAlt = value; }
		}
	}
}
