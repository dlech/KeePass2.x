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
using System.Diagnostics;
using System.IO;
using System.Text;

using KeePass.Util;
using KeePass.Util.Spr;

using KeePassLib;
using KeePassLib.Native;
using KeePassLib.Utility;

namespace KeePass.App
{
	public enum AppHelpSource
	{
		Local,
		Online
	}

	/// <summary>
	/// Application help provider. Starts an external application that
	/// shows help on a specified topic.
	/// </summary>
	public static class AppHelp
	{
		private static string g_strLocalHelpFile = null;

		/// <summary>
		/// Get the path of the local help file.
		/// </summary>
		public static string LocalHelpFile
		{
			get
			{
				if(g_strLocalHelpFile == null)
					g_strLocalHelpFile = UrlUtil.StripExtension(
						WinUtil.GetExecutable()) + ".chm";

				return g_strLocalHelpFile;
			}
		}

		public static bool LocalHelpAvailable
		{
			get
			{
				try
				{
					string strFile = AppHelp.LocalHelpFile;
					if(!string.IsNullOrEmpty(strFile))
						return File.Exists(strFile);
				}
				catch(Exception) { Debug.Assert(false); }

				return false;
			}
		}

		public static AppHelpSource PreferredHelpSource
		{
			get
			{
				return (Program.Config.Application.HelpUseLocal ?
					AppHelpSource.Local : AppHelpSource.Online);
			}

			set
			{
				Program.Config.Application.HelpUseLocal =
					(value == AppHelpSource.Local);
			}
		}

		/// <summary>
		/// Show a help page.
		/// </summary>
		/// <param name="strTopic">Topic name. May be <c>null</c>.</param>
		/// <param name="strSection">Section name. May be <c>null</c>. Must not start
		/// with the '#' character.</param>
		public static void ShowHelp(string strTopic, string strSection)
		{
			AppHelp.ShowHelp(strTopic, strSection, false);
		}

		/// <summary>
		/// Show a help page.
		/// </summary>
		/// <param name="strTopic">Topic name. May be <c>null</c>.</param>
		/// <param name="strSection">Section name. May be <c>null</c>.
		/// Must not start with the '#' character.</param>
		/// <param name="bPreferLocal">Specify if the local help file should be
		/// preferred. If no local help file is available, the online help
		/// system will be used, independent of the <c>bPreferLocal</c> flag.</param>
		public static void ShowHelp(string strTopic, string strSection, bool bPreferLocal)
		{
			if(ShowHelpOverride(strTopic, strSection)) return;

			if(AppHelp.LocalHelpAvailable)
			{
				if(bPreferLocal || (AppHelp.PreferredHelpSource == AppHelpSource.Local))
					ShowHelpLocal(strTopic, strSection);
				else
					ShowHelpOnline(strTopic, strSection);
			}
			else ShowHelpOnline(strTopic, strSection);
		}

		private static void ShowHelpLocal(string strTopic, string strSection)
		{
			string strFile = AppHelp.LocalHelpFile;
			if(string.IsNullOrEmpty(strFile)) { Debug.Assert(false); return; }

			// Unblock CHM file for proper display of help contents
			WinUtil.RemoveZoneIdentifier(strFile);

			string strCmd = "\"ms-its:" + strFile;
			if(!string.IsNullOrEmpty(strTopic))
			{
				strCmd += "::/help/" + strTopic + ".html";

				if(!string.IsNullOrEmpty(strSection))
					strCmd += "#" + strSection;
			}
			strCmd += "\"";

			if(ShowHelpLocalKcv(strCmd)) return;

			string strDisp = strCmd;
			try
			{
				if(NativeLib.IsUnix())
					NativeLib.StartProcess(strCmd.Trim('\"'));
				else // Windows
				{
					strDisp = "HH.exe " + strDisp;
					NativeLib.StartProcess(WinUtil.LocateSystemApp(
						"hh.exe"), strCmd);
				}
			}
			catch(Exception ex)
			{
				MessageService.ShowWarning(strDisp, ex);
			}
		}

		private static bool ShowHelpLocalKcv(string strQuotedMsIts)
		{
			try
			{
				if(!NativeLib.IsUnix()) return false;

				string strApp = AppLocator.FindAppUnix("kchmviewer");
				if(string.IsNullOrEmpty(strApp)) return false;

				string strFile = StrUtil.GetStringBetween(strQuotedMsIts, 0, ":", "::");
				if(string.IsNullOrEmpty(strFile))
					strFile = StrUtil.GetStringBetween(strQuotedMsIts, 0, ":", "\"");
				if(string.IsNullOrEmpty(strFile))
				{
					Debug.Assert(false);
					return false;
				}

				string strUrl = StrUtil.GetStringBetween(strQuotedMsIts, 0, "::", "\"");

				// https://www.ulduzsoft.com/linux/kchmviewer/kchmviewer-integration-reference/
				string strArgs = "\"" + SprEncoding.EncodeForCommandLine(strFile) + "\"";
				if(!string.IsNullOrEmpty(strUrl))
					strArgs = "-showPage \"" + SprEncoding.EncodeForCommandLine(
						strUrl) + "\" " + strArgs;

				NativeLib.StartProcess(strApp, strArgs);
				return true;
			}
			catch(Exception) { Debug.Assert(false); }

			return false;
		}

		private static void ShowHelpOnline(string strTopic, string strSection)
		{
			string strUrl = GetOnlineUrl(strTopic, strSection);
			WinUtil.OpenUrl(strUrl, null);
		}

		private static bool ShowHelpOverride(string strTopic, string strSection)
		{
			string strUrl = Program.Config.Application.HelpUrl;
			if(string.IsNullOrEmpty(strUrl)) return false;

			string strRel = GetRelativeUrl(strTopic, strSection);

			WinUtil.OpenUrl(strUrl, null, true, strRel);
			return true;
		}

		private static string GetRelativeUrl(string strTopic, string strSection)
		{
			StringBuilder sb = new StringBuilder();

			const string strDefault = AppDefs.HelpTopics.Default;
			sb.Append(string.IsNullOrEmpty(strTopic) ? strDefault : strTopic);
			sb.Append(".html");

			if(!string.IsNullOrEmpty(strSection))
			{
				sb.Append('#');
				sb.Append(strSection);
			}

			return sb.ToString();
		}

		internal static string GetOnlineUrl(string strTopic, string strSection)
		{
			return (PwDefs.HelpUrl + GetRelativeUrl(strTopic, strSection));
		}
	}
}
