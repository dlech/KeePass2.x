/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2018 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Threading;

using KeePass.Util;

using KeePassLib;
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
		private static string m_strLocalHelpFile = null;

		/// <summary>
		/// Get/set the path of the local help file.
		/// </summary>
		public static string LocalHelpFile
		{
			get { return m_strLocalHelpFile; }
			set { m_strLocalHelpFile = value; }
		}

		public static bool LocalHelpAvailable
		{
			get
			{
				if(m_strLocalHelpFile == null) return false;

				try { return File.Exists(m_strLocalHelpFile); }
				catch(Exception) { }
				return false;
			}
		}

		public static AppHelpSource PreferredHelpSource
		{
			get
			{
				return ((Program.Config.Application.HelpUseLocal) ?
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
		/// <param name="strSection">Section name. May be <c>null</c>. Must not start
		/// with the '#' character.</param>
		/// <param name="bPreferLocal">Specify if the local help file should be
		/// preferred. If no local help file is available, the online help
		/// system will be used, independent of the <c>bPreferLocal</c> flag.</param>
		public static void ShowHelp(string strTopic, string strSection, bool bPreferLocal)
		{
			if(AppHelp.LocalHelpAvailable)
			{
				if(bPreferLocal || (AppHelp.PreferredHelpSource == AppHelpSource.Local))
					AppHelp.ShowHelpLocal(strTopic, strSection);
				else
					AppHelp.ShowHelpOnline(strTopic, strSection);
			}
			else AppHelp.ShowHelpOnline(strTopic, strSection);
		}

		private static void ShowHelpLocal(string strTopic, string strSection)
		{
			Debug.Assert(m_strLocalHelpFile != null);

			// Unblock CHM file for proper display of help contents
			WinUtil.RemoveZoneIdentifier(m_strLocalHelpFile);

			string strCmd = "\"ms-its:" + m_strLocalHelpFile;

			if(strTopic != null)
				strCmd += "::/help/" + strTopic + ".html";

			if(strSection != null)
			{
				Debug.Assert(strTopic != null); // Topic must be present for section
				strCmd += "#" + strSection;
			}

			strCmd += "\"";

			try
			{
				Process p = Process.Start(WinUtil.LocateSystemApp("hh.exe"), strCmd);
				if(p != null) p.Dispose();
			}
			catch(Exception ex)
			{
				MessageService.ShowWarning("HH.exe " + strCmd, ex);
			}
		}

		private static void ShowHelpOnline(string strTopic, string strSection)
		{
			string strUrl = GetOnlineUrl(strTopic, strSection);
			WinUtil.OpenUrl(strUrl, null);
		}

		internal static string GetOnlineUrl(string strTopic, string strSection)
		{
			string str = PwDefs.HelpUrl;

			if(strTopic != null) str += strTopic + ".html";
			if(strSection != null)
			{
				Debug.Assert(strTopic != null); // Topic must be present for section
				str += "#" + strSection;
			}

			return str;
		}
	}
}
