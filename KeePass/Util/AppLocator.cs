/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2012 Dominik Reichl <dominik.reichl@t-online.de>

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

			try { strPath = f(); }
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
			RegistryKey kApps = Registry.ClassesRoot.OpenSubKey("Applications", false);
			RegistryKey kIE = kApps.OpenSubKey("iexplore.exe", false);
			RegistryKey kShell = kIE.OpenSubKey("shell", false);
			RegistryKey kOpen = kShell.OpenSubKey("open", false);
			RegistryKey kCommand = kOpen.OpenSubKey("command", false);
			string strPath = (kCommand.GetValue(string.Empty) as string);

			if(strPath != null)
			{
				strPath = strPath.Trim();
				strPath = UrlUtil.GetQuotedAppPath(strPath).Trim();
			}
			else { Debug.Assert(false); }

			kCommand.Close();
			kOpen.Close();
			kShell.Close();
			kIE.Close();
			kApps.Close();
			return strPath;
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
			RegistryKey kSoftware = Registry.LocalMachine.OpenSubKey("SOFTWARE", false);

			RegistryKey kWow = (bWowNode ? kSoftware.OpenSubKey("Wow6432Node", false) : null);

			RegistryKey kMozilla = (kWow ?? kSoftware).OpenSubKey("Mozilla", false);
			RegistryKey kFirefox = kMozilla.OpenSubKey("Mozilla Firefox", false);

			string strCurVer = (kFirefox.GetValue("CurrentVersion") as string);
			if((strCurVer == null) || (strCurVer.Length == 0))
			{
				kFirefox.Close();
				kMozilla.Close();
				if(kWow != null) kWow.Close();
				kSoftware.Close();
				return null;
			}

			RegistryKey kCurVer = kFirefox.OpenSubKey(strCurVer);
			RegistryKey kMain = kCurVer.OpenSubKey("Main");

			string strPath = (kMain.GetValue("PathToExe") as string);
			if(strPath != null)
			{
				strPath = strPath.Trim();
				strPath = UrlUtil.GetQuotedAppPath(strPath).Trim();
			}
			else { Debug.Assert(false); }

			kMain.Close();
			kCurVer.Close();
			kFirefox.Close();
			kMozilla.Close();
			if(kWow != null) kWow.Close();
			kSoftware.Close();
			return strPath;
		}

		private static string FindOpera()
		{
			if(NativeLib.IsUnix()) return FindAppUnix("opera");

			RegistryKey kHtml = Registry.ClassesRoot.OpenSubKey("Opera.HTML", false);
			RegistryKey kShell = kHtml.OpenSubKey("shell", false);
			RegistryKey kOpen = kShell.OpenSubKey("open", false);
			RegistryKey kCommand = kOpen.OpenSubKey("command", false);
			string strPath = (kCommand.GetValue(string.Empty) as string);

			if((strPath != null) && (strPath.Length > 0))
			{
				strPath = strPath.Trim();
				strPath = UrlUtil.GetQuotedAppPath(strPath).Trim();
			}
			else strPath = null;

			kCommand.Close();
			kOpen.Close();
			kShell.Close();
			kHtml.Close();
			return strPath;
		}

		// HKEY_CLASSES_ROOT\\ChromeHTML\\shell\\open\\command
		private static string FindChrome()
		{
			if(NativeLib.IsUnix()) return FindAppUnix("chromium-browser");

			RegistryKey kHtml = Registry.ClassesRoot.OpenSubKey("ChromeHTML", false);
			RegistryKey kShell = kHtml.OpenSubKey("shell", false);
			RegistryKey kOpen = kShell.OpenSubKey("open", false);
			RegistryKey kCommand = kOpen.OpenSubKey("command", false);
			string strPath = (kCommand.GetValue(string.Empty) as string);

			if(!string.IsNullOrEmpty(strPath))
			{
				strPath = strPath.Trim();
				strPath = UrlUtil.GetQuotedAppPath(strPath).Trim();
			}
			else strPath = null;

			kCommand.Close();
			kOpen.Close();
			kShell.Close();
			kHtml.Close();
			return (strPath ?? FindChromeOld());
		}

		// HKEY_CLASSES_ROOT\\Applications\\chrome.exe\\shell\\open\\command
		private static string FindChromeOld()
		{
			RegistryKey kApps = Registry.ClassesRoot.OpenSubKey("Applications", false);
			RegistryKey kExe = kApps.OpenSubKey("chrome.exe", false);
			RegistryKey kShell = kExe.OpenSubKey("shell", false);
			RegistryKey kOpen = kShell.OpenSubKey("open", false);
			RegistryKey kCommand = kOpen.OpenSubKey("command", false);
			string strPath = (kCommand.GetValue(string.Empty) as string);

			if(!string.IsNullOrEmpty(strPath))
			{
				strPath = strPath.Trim();
				strPath = UrlUtil.GetQuotedAppPath(strPath).Trim();
			}
			else strPath = null;

			kCommand.Close();
			kOpen.Close();
			kShell.Close();
			kExe.Close();
			kApps.Close();
			return strPath;
		}

		// HKEY_LOCAL_MACHINE\\SOFTWARE\\Apple Computer, Inc.\\Safari\\BrowserExe
		private static string FindSafari()
		{
			RegistryKey kSoftware = Registry.LocalMachine.OpenSubKey("SOFTWARE", false);
			RegistryKey kApple = kSoftware.OpenSubKey("Apple Computer, Inc.", false);
			RegistryKey kSafari = kApple.OpenSubKey("Safari", false);
			string strPath = (kSafari.GetValue("BrowserExe") as string);

			if(strPath != null)
			{
				strPath = strPath.Trim();
				strPath = UrlUtil.GetQuotedAppPath(strPath).Trim();
			}
			else { Debug.Assert(false); }

			kSafari.Close();
			kApple.Close();
			kSoftware.Close();
			return strPath;
		}

		public static string FindAppUnix(string strApp)
		{
			string str = WinUtil.RunConsoleApp("whereis", "-b " + strApp);
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
