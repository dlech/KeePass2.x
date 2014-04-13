/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2014 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Diagnostics;

using Microsoft.Win32;

using KeePass.Util.Spr;

using KeePassLib.Native;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.Util
{
	public static class AppLocator
	{
		private const int BrwIE = 0;
		private const int BrwFirefox = 1;
		private const int BrwOpera = 2;
		private const int BrwChrome = 3;
		private const int BrwSafari = 4;

		private static Dictionary<int, string> m_dictPaths =
			new Dictionary<int, string>();

		public static string InternetExplorerPath
		{
			get { return GetPath(BrwIE, FindInternetExplorer); }
		}

		public static string FirefoxPath
		{
			get { return GetPath(BrwFirefox, FindFirefox); }
		}

		public static string OperaPath
		{
			get { return GetPath(BrwOpera, FindOpera); }
		}

		public static string ChromePath
		{
			get { return GetPath(BrwChrome, FindChrome); }
		}

		public static string SafariPath
		{
			get { return GetPath(BrwSafari, FindSafari); }
		}

		private delegate string FindAppDelegate();

		private static string GetPath(int iBrwID, FindAppDelegate f)
		{
			string strPath;
			if(m_dictPaths.TryGetValue(iBrwID, out strPath)) return strPath;

			try
			{
				strPath = f();
				if((strPath != null) && (strPath.Length == 0)) strPath = null;
			}
			catch(Exception) { strPath = null; }

			m_dictPaths[iBrwID] = strPath;
			return strPath;
		}

		public static string FillPlaceholders(string strText, SprContext ctx)
		{
			string str = strText;

			str = AppLocator.ReplacePath(str, @"{INTERNETEXPLORER}",
				AppLocator.InternetExplorerPath, ctx);
			str = AppLocator.ReplacePath(str, @"{FIREFOX}",
				AppLocator.FirefoxPath, ctx);
			str = AppLocator.ReplacePath(str, @"{OPERA}",
				AppLocator.OperaPath, ctx);
			str = AppLocator.ReplacePath(str, @"{GOOGLECHROME}",
				AppLocator.ChromePath, ctx);
			str = AppLocator.ReplacePath(str, @"{SAFARI}",
				AppLocator.SafariPath, ctx);

			return str;
		}

		private static string ReplacePath(string str, string strPlaceholder,
			string strFill, SprContext ctx)
		{
			if(str == null) { Debug.Assert(false); return string.Empty; }
			if(strPlaceholder == null) { Debug.Assert(false); return str; }
			if(strPlaceholder.Length == 0) { Debug.Assert(false); return str; }
			if(strFill == null) return str; // No assert

			string strRep;
			if((ctx != null) && ctx.EncodeQuotesForCommandLine)
				strRep = "\"" + SprEngine.TransformContent(strFill, ctx) + "\"";
			else
				strRep = SprEngine.TransformContent("\"" + strFill + "\"", ctx);

			return StrUtil.ReplaceCaseInsensitive(str, strPlaceholder, strRep);
		}

		private static string FindInternetExplorer()
		{
			for(int i = 0; i < 4; ++i)
			{
				RegistryKey k = null;
				if(i == 0)
					k = Registry.LocalMachine.OpenSubKey(
						"SOFTWARE\\Clients\\StartMenuInternet\\IEXPLORE.EXE\\shell\\open\\command", false);
				else if(i == 1)
					k = Registry.LocalMachine.OpenSubKey(
						"SOFTWARE\\Wow6432Node\\Clients\\StartMenuInternet\\IEXPLORE.EXE\\shell\\open\\command", false);
				else if(i == 2)
					k = Registry.ClassesRoot.OpenSubKey(
						"IE.AssocFile.HTM\\shell\\open\\command", false);
				else if(i == 3)
					k = Registry.ClassesRoot.OpenSubKey(
						"Applications\\iexplore.exe\\shell\\open\\command", false);

				if(k == null) continue;

				string str = (k.GetValue(string.Empty) as string);
				k.Close();

				if(str == null) continue;

				str = UrlUtil.GetQuotedAppPath(str).Trim();
				if(str.Length == 0) continue;
				// https://sourceforge.net/p/keepass/discussion/329221/thread/6b292ede/
				if(str.StartsWith("iexplore.exe", StrUtil.CaseIgnoreCmp)) continue;

				return str;
			}

			return null;
		}

		private static string FindFirefox()
		{
			if(NativeLib.IsUnix()) return FindAppUnix("firefox");

			try
			{
				string strPath = FindFirefoxPr(false);
				if(!string.IsNullOrEmpty(strPath)) return strPath;
			}
			catch(Exception) { }

			return FindFirefoxPr(true);
		}

		private static string FindFirefoxPr(bool bWowNode)
		{
			RegistryKey kFirefox = Registry.LocalMachine.OpenSubKey(bWowNode ?
				"SOFTWARE\\Wow6432Node\\Mozilla\\Mozilla Firefox" :
				"SOFTWARE\\Mozilla\\Mozilla Firefox", false);
			if(kFirefox == null) return null;

			string strCurVer = (kFirefox.GetValue("CurrentVersion") as string);
			if(string.IsNullOrEmpty(strCurVer))
			{
				kFirefox.Close();
				return null;
			}

			RegistryKey kMain = kFirefox.OpenSubKey(strCurVer + "\\Main", false);
			if(kMain == null)
			{
				Debug.Assert(false);
				kFirefox.Close();
				return null;
			}

			string strPath = (kMain.GetValue("PathToExe") as string);
			if(!string.IsNullOrEmpty(strPath))
			{
				strPath = strPath.Trim();
				strPath = UrlUtil.GetQuotedAppPath(strPath).Trim();
			}
			else { Debug.Assert(false); }

			kMain.Close();
			kFirefox.Close();
			return strPath;
		}

		private static string FindOpera()
		{
			if(NativeLib.IsUnix()) return FindAppUnix("opera");

			for(int i = 0; i < 2; ++i)
			{
				RegistryKey k = null;
				if(i == 0) // Opera 20.0.1387.77
					k = Registry.LocalMachine.OpenSubKey(
						"SOFTWARE\\Clients\\StartMenuInternet\\OperaStable\\shell\\open\\command", false);
				// else if(i == 1) // Old
				//	k = Registry.LocalMachine.OpenSubKey(
				//		"SOFTWARE\\Clients\\StartMenuInternet\\Opera\\shell\\open\\command", false);
				else if(i == 1) // Old
					k = Registry.ClassesRoot.OpenSubKey(
						"Opera.HTML\\shell\\open\\command", false);

				if(k == null) continue;

				string strPath = (k.GetValue(string.Empty) as string);
				if(!string.IsNullOrEmpty(strPath))
					strPath = UrlUtil.GetQuotedAppPath(strPath).Trim();
				else { Debug.Assert(false); }

				k.Close();
				if(!string.IsNullOrEmpty(strPath)) return strPath;
			}

			return null;
		}

		private static string FindChrome()
		{
			if(NativeLib.IsUnix())
			{
				string str = FindAppUnix("google-chrome");
				if(!string.IsNullOrEmpty(str)) return str;
				return FindAppUnix("chromium-browser");
			}

			string strPath = FindChromeNew();
			if(string.IsNullOrEmpty(strPath))
				strPath = FindChromeOld();
			return strPath;
		}

		// HKEY_CLASSES_ROOT\\ChromeHTML[.ID]\\shell\\open\\command
		private static string FindChromeNew()
		{
			RegistryKey kHtml = Registry.ClassesRoot.OpenSubKey("ChromeHTML", false);
			if(kHtml == null) // New versions append an ID
			{
				string[] vKeys = Registry.ClassesRoot.GetSubKeyNames();
				foreach(string strEnum in vKeys)
				{
					if(strEnum.StartsWith("ChromeHTML.", StrUtil.CaseIgnoreCmp))
					{
						kHtml = Registry.ClassesRoot.OpenSubKey(strEnum, false);
						break;
					}
				}

				if(kHtml == null) return null;
			}

			RegistryKey kCommand = kHtml.OpenSubKey("shell\\open\\command", false);
			if(kCommand == null)
			{
				Debug.Assert(false);
				kHtml.Close();
				return null;
			}

			string strPath = (kCommand.GetValue(string.Empty) as string);
			if(!string.IsNullOrEmpty(strPath))
			{
				strPath = strPath.Trim();
				strPath = UrlUtil.GetQuotedAppPath(strPath).Trim();
			}
			else { Debug.Assert(false); }

			kCommand.Close();
			kHtml.Close();
			return strPath;
		}

		// HKEY_CLASSES_ROOT\\Applications\\chrome.exe\\shell\\open\\command
		private static string FindChromeOld()
		{
			RegistryKey kCommand = Registry.ClassesRoot.OpenSubKey(
				"Applications\\chrome.exe\\shell\\open\\command", false);
			if(kCommand == null) return null;

			string strPath = (kCommand.GetValue(string.Empty) as string);
			if(!string.IsNullOrEmpty(strPath))
			{
				strPath = strPath.Trim();
				strPath = UrlUtil.GetQuotedAppPath(strPath).Trim();
			}
			else { Debug.Assert(false); }

			kCommand.Close();
			return strPath;
		}

		// HKEY_LOCAL_MACHINE\\SOFTWARE\\Apple Computer, Inc.\\Safari\\BrowserExe
		private static string FindSafari()
		{
			RegistryKey kSafari = Registry.LocalMachine.OpenSubKey(
				"SOFTWARE\\Apple Computer, Inc.\\Safari", false);
			if(kSafari == null) return null;

			string strPath = (kSafari.GetValue("BrowserExe") as string);
			if(!string.IsNullOrEmpty(strPath))
			{
				strPath = strPath.Trim();
				strPath = UrlUtil.GetQuotedAppPath(strPath).Trim();
			}
			else { Debug.Assert(false); }

			kSafari.Close();
			return strPath;
		}

		public static string FindAppUnix(string strApp)
		{
			string strArgPrefix = "-b ";
			if(NativeLib.GetPlatformID() == PlatformID.MacOSX)
				strArgPrefix = string.Empty; // FR 3535696

			string str = NativeLib.RunConsoleApp("whereis", strArgPrefix + strApp);
			if(str == null) return null;

			str = str.Trim();

			int iPrefix = str.IndexOf(':');
			if(iPrefix >= 0) str = str.Substring(iPrefix + 1).Trim();

			int iSep = str.IndexOfAny(new char[]{ ' ', '\t', '\r', '\n' });
			if(iSep >= 0) str = str.Substring(0, iSep);

			return ((str.Length > 0) ? str : null);
		}
	}
}
