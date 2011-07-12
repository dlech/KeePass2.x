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
using System.Windows.Forms;
using System.Xml.Serialization;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Utility;

namespace KeePass.App.Configuration
{
	public sealed class AceIntegration
	{
		private ulong m_hkAutoType = (ulong)(Keys.Control | Keys.Alt | Keys.A);
		public ulong HotKeyGlobalAutoType
		{
			get { return m_hkAutoType; }
			set { m_hkAutoType = value; }
		}

		private ulong m_hkAutoTypeSel = (ulong)Keys.None;
		public ulong HotKeySelectedAutoType
		{
			get { return m_hkAutoTypeSel; }
			set { m_hkAutoTypeSel = value; }
		}

		private ulong m_hkShowWindow = (ulong)(Keys.Control | Keys.Alt | Keys.K);
		public ulong HotKeyShowWindow
		{
			get { return m_hkShowWindow; }
			set { m_hkShowWindow = value; }
		}

		private ulong m_hkEntryMenu = (ulong)Keys.None;
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

		private AceUrlSchemeOverrides m_vSchemeOverrides = new AceUrlSchemeOverrides();
		public AceUrlSchemeOverrides UrlSchemeOverrides
		{
			get { return m_vSchemeOverrides; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_vSchemeOverrides = value;
			}
		}

		private bool m_bSearchKeyFiles = true;
		public bool SearchKeyFiles
		{
			get { return m_bSearchKeyFiles; }
			set { m_bSearchKeyFiles = value; }
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

		private bool m_bMatchByTitle = true;
		public bool AutoTypeMatchByTitle
		{
			get { return m_bMatchByTitle; }
			set { m_bMatchByTitle = value; }
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

		private bool m_bCancelOnWindowChange = false;
		public bool AutoTypeCancelOnWindowChange
		{
			get { return m_bCancelOnWindowChange; }
			set { m_bCancelOnWindowChange = value; }
		}

		private ProxyServerType m_pstProxyType = ProxyServerType.System;
		public ProxyServerType ProxyType
		{
			get { return m_pstProxyType; }
			set { m_pstProxyType = value; }
		}

		private string m_strProxyAddr = string.Empty;
		public string ProxyAddress
		{
			get { return m_strProxyAddr; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strProxyAddr = value;
			}
		}

		private string m_strProxyPort = string.Empty;
		public string ProxyPort
		{
			get { return m_strProxyPort; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strProxyPort = value;
			}
		}

		private string m_strProxyUser = string.Empty;
		public string ProxyUserName
		{
			get { return m_strProxyUser; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strProxyUser = value;
			}
		}

		private string m_strProxyPassword = string.Empty;
		public string ProxyPassword
		{
			get { return m_strProxyPassword; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strProxyPassword = value;
			}
		}

		public AceIntegration()
		{
		}
	}

	public sealed class AceUrlSchemeOverrides : IDeepCloneable<AceUrlSchemeOverrides>
	{
		private bool m_bSetToDefaults = true;
		public bool SetToDefaults
		{
			get { return m_bSetToDefaults; }
			set { m_bSetToDefaults = value; }
		}

		private List<AceUrlSchemeOverride> m_vOverrides =
			new List<AceUrlSchemeOverride>();
		[XmlArrayItem("Override")]
		public List<AceUrlSchemeOverride> Overrides
		{
			get { return m_vOverrides; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_vOverrides = value;
			}
		}

		public AceUrlSchemeOverrides()
		{
		}

		public void SetDefaultsIfEmpty()
		{
			if(m_bSetToDefaults == false) return;

			m_bSetToDefaults = false; // Set only once
			m_vOverrides.Clear(); // Avoid duplication of defaults

			m_vOverrides.Add(new AceUrlSchemeOverride(true, "ssh",
				@"cmd://PuTTY.exe -ssh {USERNAME}@{URL:RMVSCM}"));
			m_vOverrides.Add(new AceUrlSchemeOverride(false, "http",
				"cmd://{INTERNETEXPLORER} \"{URL}\""));
			m_vOverrides.Add(new AceUrlSchemeOverride(false, "https",
				"cmd://{INTERNETEXPLORER} \"{URL}\""));
			m_vOverrides.Add(new AceUrlSchemeOverride(false, "http",
				"cmd://{FIREFOX} \"{URL}\""));
			m_vOverrides.Add(new AceUrlSchemeOverride(false, "https",
				"cmd://{FIREFOX} \"{URL}\""));
			m_vOverrides.Add(new AceUrlSchemeOverride(false, "chrome",
				"cmd://{FIREFOX} -chrome \"{URL}\""));
			m_vOverrides.Add(new AceUrlSchemeOverride(false, "http",
				"cmd://{OPERA} \"{URL}\""));
			m_vOverrides.Add(new AceUrlSchemeOverride(false, "https",
				"cmd://{OPERA} \"{URL}\""));
			m_vOverrides.Add(new AceUrlSchemeOverride(false, "http",
				"cmd://{GOOGLECHROME} \"{URL}\""));
			m_vOverrides.Add(new AceUrlSchemeOverride(false, "https",
				"cmd://{GOOGLECHROME} \"{URL}\""));
			m_vOverrides.Add(new AceUrlSchemeOverride(false, "kdbx",
				"cmd://\"{APPDIR}\\KeePass.exe\" \"{URL:RMVSCM}\" -pw-enc:\"{PASSWORD_ENC}\""));
			m_vOverrides.Add(new AceUrlSchemeOverride(false, "kdbx",
				"cmd://mono \"{APPDIR}/KeePass.exe\" \"{URL:RMVSCM}\" -pw-enc:\"{PASSWORD_ENC}\""));
		}

		public string GetOverrideForUrl(string strUrl)
		{
			if(string.IsNullOrEmpty(strUrl)) return null;

			foreach(AceUrlSchemeOverride ovr in m_vOverrides)
			{
				if(!ovr.Enabled) continue;

				if(strUrl.StartsWith(ovr.Scheme + ":", StrUtil.CaseIgnoreCmp))
					return ovr.UrlOverride;
			}

			return null;
		}

		public AceUrlSchemeOverrides CloneDeep()
		{
			AceUrlSchemeOverrides ovr = new AceUrlSchemeOverrides();

			ovr.m_bSetToDefaults = m_bSetToDefaults;
			foreach(AceUrlSchemeOverride sh in m_vOverrides)
			{
				ovr.m_vOverrides.Add(sh.CloneDeep());
			}

			return ovr;
		}
	}

	public sealed class AceUrlSchemeOverride : IDeepCloneable<AceUrlSchemeOverride>
	{
		private bool m_bEnabled = true;
		public bool Enabled
		{
			get { return m_bEnabled; }
			set { m_bEnabled = value; }
		}

		private string m_strScheme = string.Empty;
		public string Scheme
		{
			get { return m_strScheme; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strScheme = value;
			}
		}

		private string m_strOvr = string.Empty;
		public string UrlOverride
		{
			get { return m_strOvr; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strOvr = value;
			}
		}

		public AceUrlSchemeOverride()
		{
		}

		public AceUrlSchemeOverride(bool bEnable, string strScheme,
			string strUrlOverride)
		{
			if(strScheme == null) throw new ArgumentNullException("strScheme");
			if(strUrlOverride == null) throw new ArgumentNullException("strUrlOverride");

			m_bEnabled = bEnable;
			m_strScheme = strScheme;
			m_strOvr = strUrlOverride;
		}

		public AceUrlSchemeOverride CloneDeep()
		{
			return new AceUrlSchemeOverride(m_bEnabled, m_strScheme, m_strOvr);
		}
	}
}
