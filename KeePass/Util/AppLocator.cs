/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2022 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Diagnostics;
using System.IO;
using System.Text;

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
		private const int BrwEdge = 5;

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

		/// <summary>
		/// Path to the executable of the legacy Microsoft Edge (EdgeHTML).
		/// The executable cannot be run normally.
		/// </summary>
		public static string EdgePath
		{
			get { return GetPath(BrwEdge, FindEdge); }
		}

		private static bool? m_obEdgeProtocol = null;
		public static bool EdgeProtocolSupported
		{
			get
			{
				if(m_obEdgeProtocol.HasValue)
					return m_obEdgeProtocol.Value;

				bool b = false;
				try
				{
					using(RegistryKey rk = Registry.ClassesRoot.OpenSubKey(
						"microsoft-edge", false))
					{
						if(rk != null)
							b = (rk.GetValue("URL Protocol") != null);
					}
				}
				catch(Exception) { Debug.Assert(NativeLib.IsUnix()); }

				m_obEdgeProtocol = b;
				return b;
			}
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
			// Edge executable cannot be run normally

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
			if((ctx != null) && ctx.EncodeForCommandLine)
				strRep = "\"" + SprEngine.TransformContent(strFill, ctx) + "\"";
			else
				strRep = SprEngine.TransformContent("\"" + strFill + "\"", ctx);

			return StrUtil.ReplaceCaseInsensitive(str, strPlaceholder, strRep);
		}

		private static string FindInternetExplorer()
		{
			const string strIEDef = "SOFTWARE\\Clients\\StartMenuInternet\\IEXPLORE.EXE\\shell\\open\\command";
			const string strIEWow = "SOFTWARE\\Wow6432Node\\Clients\\StartMenuInternet\\IEXPLORE.EXE\\shell\\open\\command";

			for(int i = 0; i < 6; ++i)
			{
				RegistryKey k = null;

				// https://msdn.microsoft.com/en-us/library/windows/desktop/dd203067.aspx
				if(i == 0)
					k = Registry.CurrentUser.OpenSubKey(strIEDef, false);
				else if(i == 1)
					k = Registry.CurrentUser.OpenSubKey(strIEWow, false);
				else if(i == 2)
					k = Registry.LocalMachine.OpenSubKey(strIEDef, false);
				else if(i == 3)
					k = Registry.LocalMachine.OpenSubKey(strIEWow, false);
				else if(i == 4)
					k = Registry.ClassesRoot.OpenSubKey(
						"IE.AssocFile.HTM\\shell\\open\\command", false);
				else
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

			for(int i = 0; i < 2; ++i)
			{
				try
				{
					string str = FindFirefoxWin(i != 0);
					if(!string.IsNullOrEmpty(str)) return str;
				}
				catch(Exception) { Debug.Assert(false); }
			}

			return FindAppByClass(".html", "firefox.exe");
		}

		private static string FindFirefoxWin(bool bWowNode)
		{
			string strPath;

			using(RegistryKey kFirefox = Registry.LocalMachine.OpenSubKey((bWowNode ?
				"SOFTWARE\\Wow6432Node\\Mozilla\\Mozilla Firefox" :
				"SOFTWARE\\Mozilla\\Mozilla Firefox"), false))
			{
				if(kFirefox == null) return null;

				string strCurVer = (kFirefox.GetValue("CurrentVersion") as string);
				if(string.IsNullOrEmpty(strCurVer))
				{
					// The ESR version stores the 'CurrentVersion' value under
					// 'Mozilla Firefox ESR', but the version-specific info
					// under 'Mozilla Firefox\\<Version>' (without 'ESR')
					using(RegistryKey kEsr = Registry.LocalMachine.OpenSubKey((bWowNode ?
						"SOFTWARE\\Wow6432Node\\Mozilla\\Mozilla Firefox ESR" :
						"SOFTWARE\\Mozilla\\Mozilla Firefox ESR"), false))
					{
						if(kEsr != null)
							strCurVer = (kEsr.GetValue("CurrentVersion") as string);
					}

					if(string.IsNullOrEmpty(strCurVer)) return null;
				}

				using(RegistryKey kMain = kFirefox.OpenSubKey(strCurVer + "\\Main", false))
				{
					if(kMain == null) { Debug.Assert(false); return null; }

					strPath = (kMain.GetValue("PathToExe") as string);
					if(!string.IsNullOrEmpty(strPath))
						strPath = UrlUtil.GetQuotedAppPath(strPath).Trim();
					else { Debug.Assert(false); }
				}
			}

			return strPath;
		}

		private static string FindOpera()
		{
			if(NativeLib.IsUnix()) return FindAppUnix("opera");

			// Old Opera versions
			const string strOp12 = "SOFTWARE\\Clients\\StartMenuInternet\\Opera\\shell\\open\\command";
			// Opera >= 20.0.1387.77
			const string strOp20 = "SOFTWARE\\Clients\\StartMenuInternet\\OperaStable\\shell\\open\\command";

			for(int i = 0; i < 5; ++i)
			{
				RegistryKey k = null;

				// https://msdn.microsoft.com/en-us/library/windows/desktop/dd203067.aspx
				if(i == 0)
					k = Registry.CurrentUser.OpenSubKey(strOp20, false);
				else if(i == 1)
					k = Registry.CurrentUser.OpenSubKey(strOp12, false);
				else if(i == 2)
					k = Registry.LocalMachine.OpenSubKey(strOp20, false);
				else if(i == 3)
					k = Registry.LocalMachine.OpenSubKey(strOp12, false);
				else // Old Opera versions
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
				str = FindAppUnix("chromium");
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

		private static string FindEdge()
		{
			string strSys = Environment.SystemDirectory.TrimEnd(
				UrlUtil.LocalDirSepChar);
			if(strSys.EndsWith("32"))
				strSys = strSys.Substring(0, strSys.Length - 2);
			strSys += "Apps";

			if(!Directory.Exists(strSys)) return null;

			string[] vEdgeDirs = Directory.GetDirectories(strSys,
				"Microsoft.MicrosoftEdge*", SearchOption.TopDirectoryOnly);
			if(vEdgeDirs == null) { Debug.Assert(false); return null; }

			foreach(string strEdgeDir in vEdgeDirs)
			{
				string strExe = UrlUtil.EnsureTerminatingSeparator(
					strEdgeDir, false) + "MicrosoftEdge.exe";
				if(File.Exists(strExe)) return strExe;
			}

			return null;
		}

		private static string FindAppByClass(string strClass, string strExeName)
		{
			if(string.IsNullOrEmpty(strClass)) { Debug.Assert(false); return null; }
			if(string.IsNullOrEmpty(strExeName)) { Debug.Assert(false); return null; }

			Debug.Assert(strClass.StartsWith(".")); // File extension class
			Debug.Assert(strExeName.EndsWith(".exe", StrUtil.CaseIgnoreCmp));

			try
			{
				using(RegistryKey kOpenWith = Registry.ClassesRoot.OpenSubKey(
					strClass + "\\OpenWithProgids", false))
				{
					if(kOpenWith == null) { Debug.Assert(false); return null; }

					foreach(string strOpenWithClass in kOpenWith.GetValueNames())
					{
						if(string.IsNullOrEmpty(strOpenWithClass)) { Debug.Assert(false); continue; }

						using(RegistryKey kCommand = Registry.ClassesRoot.OpenSubKey(
							strOpenWithClass + "\\Shell\\open\\command", false))
						{
							if(kCommand == null) { Debug.Assert(false); continue; }

							string str = (kCommand.GetValue(string.Empty) as string);
							if(string.IsNullOrEmpty(str)) { Debug.Assert(false); continue; }

							str = UrlUtil.GetQuotedAppPath(str);

							if(string.Equals(UrlUtil.GetFileName(str), strExeName,
								StrUtil.CaseIgnoreCmp))
								return str;
						}
					}
				}
			}
			catch(Exception) { Debug.Assert(false); }

			return null;
		}

		public static string FindAppUnix(string strApp)
		{
			if(string.IsNullOrEmpty(strApp)) { Debug.Assert(false); return null; }

			string strOpt = "-b ";
			if(NativeLib.GetPlatformID() == PlatformID.MacOSX)
				strOpt = string.Empty; // FR 3535696

			string str = NativeLib.RunConsoleApp("whereis", strOpt + strApp);
			if(string.IsNullOrEmpty(str)) return null;

			int iSep = str.IndexOf(':');
			if(iSep >= 0) str = str.Substring(iSep + 1);

			string[] v = str.Split(new char[] { ' ', '\t', '\r', '\n' });

			foreach(string strPath in v)
			{
				if(string.IsNullOrEmpty(strPath)) continue;

				// Sometimes the first item is a directory
				// (e.g. Chromium Snap package on Kubuntu 21.10)
				try { if(File.Exists(strPath)) return strPath; }
				catch(Exception) { Debug.Assert(false); }
			}

			return null;
		}
	}
}
