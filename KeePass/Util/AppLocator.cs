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
using System.Diagnostics;
using System.IO;
using System.Text;

using Microsoft.Win32;

using KeePass.Util.Spr;

using KeePassLib.Native;
using KeePassLib.Utility;

namespace KeePass.Util
{
	public static class AppLocator
	{
		private delegate string FindAppDelegate();

		private static string g_strIEPath = null;
		public static string InternetExplorerPath
		{
			get { return GetPath(ref g_strIEPath, FindInternetExplorer); }
		}

		private static string g_strFirefoxPath = null;
		public static string FirefoxPath
		{
			get { return GetPath(ref g_strFirefoxPath, FindFirefox); }
		}

		private static string g_strOperaPath = null;
		public static string OperaPath
		{
			get { return GetPath(ref g_strOperaPath, FindOpera); }
		}

		private static string g_strChromePath = null;
		public static string ChromePath
		{
			get { return GetPath(ref g_strChromePath, FindChrome); }
		}

		private static string g_strSafariPath = null;
		public static string SafariPath
		{
			get { return GetPath(ref g_strSafariPath, FindSafari); }
		}

		private static string g_strEdgePath = null;
		public static string EdgePath
		{
			get { return GetPath(ref g_strEdgePath, FindEdge); }
		}

		private static bool? g_obEdgeProtocol = null;
		public static bool EdgeProtocolSupported
		{
			get
			{
				if(g_obEdgeProtocol.HasValue) return g_obEdgeProtocol.Value;

				bool b = (RegUtil.GetValue<object>("HKEY_CLASSES_ROOT\\microsoft-edge",
					"URL Protocol") != null);
				g_obEdgeProtocol = b;
				return b;
			}
		}

		private static string GetPath(ref string strPath, FindAppDelegate f)
		{
			if(strPath != null) return strPath;

			string str;
			try { str = f(); }
			catch(Exception) { Debug.Assert(false); str = null; }

			if(str == null) str = string.Empty;
			else str = UrlUtil.GetQuotedAppPath(str).Trim();

			strPath = str;
			return str;
		}

		public static string FillPlaceholders(string strText, SprContext ctx)
		{
			string str = strText;

			str = ReplacePath(str, "{INTERNETEXPLORER}", AppLocator.InternetExplorerPath, ctx);
			str = ReplacePath(str, "{FIREFOX}", AppLocator.FirefoxPath, ctx);
			str = ReplacePath(str, "{OPERA}", AppLocator.OperaPath, ctx);
			str = ReplacePath(str, "{GOOGLECHROME}", AppLocator.ChromePath, ctx);
			str = ReplacePath(str, "{SAFARI}", AppLocator.SafariPath, ctx);
			str = ReplacePath(str, "{EDGE}", AppLocator.EdgePath, ctx);

			return str;
		}

		private static string ReplacePath(string str, string strPlaceholder,
			string strFill, SprContext ctx)
		{
			if(str == null) { Debug.Assert(false); return string.Empty; }
			if(string.IsNullOrEmpty(strPlaceholder)) { Debug.Assert(false); return str; }
			if(string.IsNullOrEmpty(strFill)) return str; // No assert

			string strRep;
			if((ctx != null) && ctx.EncodeForCommandLine)
				strRep = "\"" + SprEngine.TransformContent(strFill, ctx) + "\"";
			else
				strRep = SprEngine.TransformContent("\"" + strFill + "\"", ctx);

			return StrUtil.ReplaceCaseInsensitive(str, strPlaceholder, strRep);
		}

		private static string FindInternetExplorer()
		{
			string[] vKeys = new string[] {
				// https://msdn.microsoft.com/en-us/library/windows/desktop/dd203067.aspx
				"HKEY_CURRENT_USER\\SOFTWARE\\Clients\\StartMenuInternet\\IEXPLORE.EXE\\shell\\open\\command",
				"HKEY_CURRENT_USER\\SOFTWARE\\Wow6432Node\\Clients\\StartMenuInternet\\IEXPLORE.EXE\\shell\\open\\command",
				"HKEY_LOCAL_MACHINE\\SOFTWARE\\Clients\\StartMenuInternet\\IEXPLORE.EXE\\shell\\open\\command",
				"HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Clients\\StartMenuInternet\\IEXPLORE.EXE\\shell\\open\\command",
				"HKEY_CLASSES_ROOT\\IE.AssocFile.HTM\\shell\\open\\command",
				"HKEY_CLASSES_ROOT\\Applications\\iexplore.exe\\shell\\open\\command"
			};

			foreach(string strKey in vKeys)
			{
				string str = RegUtil.GetValue<string>(strKey, string.Empty);
				if(string.IsNullOrEmpty(str)) continue;

				// https://sourceforge.net/p/keepass/discussion/329221/thread/6b292ede/
				if(str.StartsWith("iexplore.exe", StrUtil.CaseIgnoreCmp)) continue;

				return str;
			}

			return null;
		}

		private static string FindFirefox()
		{
			if(NativeLib.IsUnix()) return FindAppUnix("firefox");

			string str = FindFirefoxWin(false);
			if(!string.IsNullOrEmpty(str)) return str;

			str = FindFirefoxWin(true);
			if(!string.IsNullOrEmpty(str)) return str;

			return FindAppByClass(".html", "firefox.exe");
		}

		private static string FindFirefoxWin(bool bWowNode)
		{
			using(RegistryKey rkFirefox = RegUtil.OpenSubKey(Registry.LocalMachine,
				(bWowNode ? "SOFTWARE\\Wow6432Node\\Mozilla\\Mozilla Firefox" :
				"SOFTWARE\\Mozilla\\Mozilla Firefox")))
			{
				if(rkFirefox == null) return null;

				string strCurVer = RegUtil.GetValue<string>(rkFirefox, "CurrentVersion");
				if(string.IsNullOrEmpty(strCurVer))
				{
					// The ESR version stores the 'CurrentVersion' value under
					// 'Mozilla Firefox ESR', but the version-specific info
					// under 'Mozilla Firefox\\<Version>' (without 'ESR')
					strCurVer = RegUtil.GetValue<string>((bWowNode ?
						"HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Mozilla\\Mozilla Firefox ESR" :
						"HKEY_LOCAL_MACHINE\\SOFTWARE\\Mozilla\\Mozilla Firefox ESR"),
						"CurrentVersion");
					if(string.IsNullOrEmpty(strCurVer)) return null;
				}

				using(RegistryKey rkMain = RegUtil.OpenSubKey(rkFirefox,
					strCurVer + "\\Main"))
				{
					if(rkMain == null) { Debug.Assert(false); return null; }

					return RegUtil.GetValue<string>(rkMain, "PathToExe");
				}
			}
		}

		private static string FindOpera()
		{
			if(NativeLib.IsUnix()) return FindAppUnix("opera");

			string[] vKeys = new string[] {
				// https://msdn.microsoft.com/en-us/library/windows/desktop/dd203067.aspx

				// Opera >= 20.0.1387.77
				"HKEY_CURRENT_USER\\SOFTWARE\\Clients\\StartMenuInternet\\OperaStable\\shell\\open\\command",
				"HKEY_LOCAL_MACHINE\\SOFTWARE\\Clients\\StartMenuInternet\\OperaStable\\shell\\open\\command",

				// Old Opera versions
				"HKEY_CURRENT_USER\\SOFTWARE\\Clients\\StartMenuInternet\\Opera\\shell\\open\\command",
				"HKEY_LOCAL_MACHINE\\SOFTWARE\\Clients\\StartMenuInternet\\Opera\\shell\\open\\command",
				"HKEY_CLASSES_ROOT\\Opera.HTML\\shell\\open\\command"
			};

			foreach(string strKey in vKeys)
			{
				string str = RegUtil.GetValue<string>(strKey, string.Empty);
				if(!string.IsNullOrEmpty(str)) return str;
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
			if(!string.IsNullOrEmpty(strPath)) return strPath;

			return RegUtil.GetValue<string>(
				"HKEY_CLASSES_ROOT\\Applications\\chrome.exe\\shell\\open\\command",
				string.Empty);
		}

		// HKEY_CLASSES_ROOT\\ChromeHTML[.ID]\\shell\\open\\command
		private static string FindChromeNew()
		{
			RegistryKey rkHtml = null;
			try
			{
				rkHtml = RegUtil.OpenSubKey(Registry.ClassesRoot, "ChromeHTML");
				if(rkHtml == null) // New versions append an ID
				{
					string[] vNames = Registry.ClassesRoot.GetSubKeyNames();
					foreach(string strName in vNames)
					{
						if(strName.StartsWith("ChromeHTML.", StrUtil.CaseIgnoreCmp))
						{
							rkHtml = RegUtil.OpenSubKey(Registry.ClassesRoot, strName);
							break;
						}
					}

					if(rkHtml == null) return null;
				}

				using(RegistryKey rkCommand = RegUtil.OpenSubKey(rkHtml,
					"shell\\open\\command"))
				{
					if(rkCommand != null)
					{
						string str = RegUtil.GetValue<string>(rkCommand, string.Empty);
						if(!string.IsNullOrEmpty(str)) return str;
					}
					else { Debug.Assert(false); }
				}
			}
			catch(Exception) { Debug.Assert(false); }
			finally { if(rkHtml != null) rkHtml.Close(); }

			return null;
		}

		private static string FindSafari()
		{
			return RegUtil.GetValue<string>(
				"HKEY_LOCAL_MACHINE\\SOFTWARE\\Apple Computer, Inc.\\Safari",
				"BrowserExe");
		}

		// Legacy Edge (EdgeHTML)
		/* private static string FindEdge()
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
		} */

		private static string FindEdge()
		{
			return RegUtil.GetValue<string>(
				"HKEY_LOCAL_MACHINE\\SOFTWARE\\Clients\\StartMenuInternet\\Microsoft Edge\\shell\\open\\command",
				string.Empty);
		}

		private static string FindAppByClass(string strClass, string strExeName)
		{
			if(string.IsNullOrEmpty(strClass)) { Debug.Assert(false); return null; }
			if(string.IsNullOrEmpty(strExeName)) { Debug.Assert(false); return null; }

			Debug.Assert(strClass.StartsWith(".")); // File extension class
			Debug.Assert(strExeName.EndsWith(".exe", StrUtil.CaseIgnoreCmp));

			try
			{
				using(RegistryKey rkOpenWith = RegUtil.OpenSubKey(Registry.ClassesRoot,
					strClass + "\\OpenWithProgids"))
				{
					if(rkOpenWith == null) { Debug.Assert(false); return null; }

					foreach(string strOpenWithClass in rkOpenWith.GetValueNames())
					{
						if(string.IsNullOrEmpty(strOpenWithClass)) { Debug.Assert(false); continue; }

						using(RegistryKey rkCommand = RegUtil.OpenSubKey(Registry.ClassesRoot,
							strOpenWithClass + "\\Shell\\open\\command"))
						{
							if(rkCommand == null) { Debug.Assert(false); continue; }

							string str = RegUtil.GetValue<string>(rkCommand, string.Empty);
							if(string.IsNullOrEmpty(str)) { Debug.Assert(false); continue; }

							str = UrlUtil.GetQuotedAppPath(str).Trim();

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
