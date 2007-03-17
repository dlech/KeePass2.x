/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2007 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Microsoft.Win32;

using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.Util
{
	public static class AppLocator
	{
		private static string m_strIE = null;
		private static string m_strFirefox = null;
		private static string m_strOpera = null;

		public static string InternetExplorerPath
		{
			get
			{
				if(m_strIE != null) return m_strIE;
				else
				{
					try { m_strIE = FindInternetExplorer(); }
					catch(Exception) { m_strIE = null; }

					return m_strIE;
				}
			}
		}

		public static string FirefoxPath
		{
			get
			{
				if(m_strFirefox != null) return m_strFirefox;
				else
				{
					try { m_strFirefox = FindFirefox(); }
					catch(Exception) { m_strFirefox = null; }

					return m_strFirefox;
				}
			}
		}

		public static string OperaPath
		{
			get
			{
				if(m_strOpera != null) return m_strOpera;
				else
				{
					try { m_strOpera = FindOpera(); }
					catch(Exception) { m_strOpera = null; }

					return m_strOpera;
				}
			}
		}

		public static string FillPlaceholders(string strText)
		{
			string str = strText;

			if(AppLocator.InternetExplorerPath != null)
				str = StrUtil.ReplaceCaseInsensitive(str, @"{INTERNETEXPLORER}",
					new ProtectedString(false, "\"" + m_strIE + "\""), false);
			if(AppLocator.FirefoxPath != null)
				str = StrUtil.ReplaceCaseInsensitive(str, @"{FIREFOX}",
					new ProtectedString(false, "\"" + m_strFirefox + "\""), false);
			if(AppLocator.OperaPath != null)
				str = StrUtil.ReplaceCaseInsensitive(str, @"{OPERA}",
					new ProtectedString(false, "\"" + m_strOpera + "\""), false);

			return str;
		}

		private static string FindInternetExplorer()
		{
			RegistryKey kApps = Registry.ClassesRoot.OpenSubKey("Applications", false);
			RegistryKey kIE = kApps.OpenSubKey("iexplore.exe", false);
			RegistryKey kShell = kIE.OpenSubKey("shell", false);
			RegistryKey kOpen = kShell.OpenSubKey("open", false);
			RegistryKey kCommand = kOpen.OpenSubKey("command", false);
			string strPath = (string)kCommand.GetValue(string.Empty);

			strPath = strPath.Trim();
			strPath = UrlUtil.GetQuotedAppPath(strPath).Trim();

			kCommand.Close();
			kOpen.Close();
			kShell.Close();
			kIE.Close();
			kApps.Close();

			return strPath;
		}

		private static string FindFirefox()
		{
			RegistryKey kSoftware = Registry.LocalMachine.OpenSubKey("SOFTWARE", false);
			RegistryKey kMozilla = kSoftware.OpenSubKey("Mozilla", false);
			RegistryKey kFirefox = kMozilla.OpenSubKey("Mozilla Firefox", false);

			string strCurVer = (string)kFirefox.GetValue("CurrentVersion");
			if((strCurVer == null) || (strCurVer.Length == 0)) return null;

			RegistryKey kCurVer = kFirefox.OpenSubKey(strCurVer);
			RegistryKey kMain = kCurVer.OpenSubKey("Main");

			string strPath = (string)kMain.GetValue("PathToExe");
			if((strPath == null) || (strPath.Length == 0)) return null;

			strPath = strPath.Trim();
			strPath = UrlUtil.GetQuotedAppPath(strPath).Trim();

			kMain.Close();
			kCurVer.Close();
			kFirefox.Close();
			kMozilla.Close();
			kSoftware.Close();

			return strPath;
		}

		private static string FindOpera()
		{
			RegistryKey kHtml = Registry.ClassesRoot.OpenSubKey("Opera.HTTP", false);
			RegistryKey kShell = kHtml.OpenSubKey("shell", false);
			RegistryKey kOpen = kShell.OpenSubKey("open", false);
			RegistryKey kCommand = kOpen.OpenSubKey("command", false);
			string strPath = (string)kCommand.GetValue(string.Empty);

			if((strPath != null) && (strPath.Length > 0))
			{
				strPath = strPath.Trim();
				strPath = UrlUtil.GetQuotedAppPath(strPath).Trim();
			}
			else { strPath = null; }

			kCommand.Close();
			kOpen.Close();
			kShell.Close();
			kHtml.Close();

			return strPath;
		}
	}
}
