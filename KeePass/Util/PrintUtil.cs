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
using System.Windows.Forms;

using Microsoft.Win32;

using KeePass.UI;
using KeePass.Util.Spr;

using KeePassLib.Native;
using KeePassLib.Utility;

namespace KeePass.Util
{
	public static class PrintUtil
	{
		public static void PrintHtml(string strHtml)
		{
			if(string.IsNullOrEmpty(strHtml)) { Debug.Assert(false); return; }

			try
			{
				if(!PrintHtmlShell(strHtml))
				{
					if(!PrintHtmlWB(strHtml))
						PrintHtmlExec(strHtml);
				}
			}
			catch(Exception ex) { MessageService.ShowWarning(ex); }
		}

		private static bool PrintHtmlShell(string strHtml)
		{
			if(NativeLib.IsUnix()) return false;

			try
			{
				string str = RegUtil.GetValue<string>(
					"HKEY_CLASSES_ROOT\\htmlfile\\shell\\print\\command", string.Empty);
				if(string.IsNullOrEmpty(str)) { Debug.Assert(false); return false; }
				str = FixPrintCommandLine(str);

				string strPath = Program.TempFilesPool.GetTempFileName("html");

				string strOrg = str;
				str = UrlUtil.ExpandShellVariables(str, new string[] { strPath }, true);
				if(str == strOrg) { Debug.Assert(false); return false; }

				File.WriteAllText(strPath, strHtml, StrUtil.Utf8);
				WinUtil.OpenUrl("cmd://" + str, null, false, null, false);
				return true;
			}
			catch(Exception) { Debug.Assert(false); }

			return false;
		}

		private static string FixPrintCommandLine(string strCmd)
		{
			string str = strCmd;

			// Workaround for Microsoft Office breaking the 'Print' shell verb;
			// https://sourceforge.net/p/keepass/bugs/1675/
			// https://support.microsoft.com/en-us/help/274527/cannot-print-file-with--htm-extension-from-windows-explorer-by-right-c
			if(str.IndexOf("\\msohtmed.exe", StrUtil.CaseIgnoreCmp) >= 0)
			{
				string strSys = UrlUtil.EnsureTerminatingSeparator(
					Environment.SystemDirectory, false);
				str = "\"" + SprEncoding.EncodeForCommandLine(strSys +
					"rundll32.exe") + "\" \"" + SprEncoding.EncodeForCommandLine(
					strSys + "mshtml.dll") + "\",PrintHTML \"%1\"";
			}

			return str;
		}

		private static bool PrintHtmlWB(string strHtml)
		{
			// Mono's WebBrowser implementation doesn't support printing
			if(NativeLib.IsUnix()) return false;

			try
			{
				// Printing and disposing immediately seems to be supported;
				// https://docs.microsoft.com/en-us/dotnet/framework/winforms/controls/how-to-print-with-a-webbrowser-control
				using(WebBrowser wb = new WebBrowser())
				{
					wb.AllowWebBrowserDrop = false;
					wb.IsWebBrowserContextMenuEnabled = false;
					wb.ScriptErrorsSuppressed = true;
					wb.WebBrowserShortcutsEnabled = false;

					UIUtil.SetWebBrowserDocument(wb, strHtml);

					wb.ShowPrintDialog();
				}

				Program.TempFilesPool.AddWebBrowserPrintContent();
				return true;
			}
			catch(Exception) { Debug.Assert(false); }

			return false;
		}

		private static void PrintHtmlExec(string strHtml)
		{
			string strPath = Program.TempFilesPool.GetTempFileName("html");
			File.WriteAllText(strPath, strHtml, StrUtil.Utf8);

			ProcessStartInfo psi = new ProcessStartInfo();
			psi.FileName = strPath;
			psi.UseShellExecute = true;

			// Try to use the 'Print' verb; if it's not available, the
			// default verb is used (to just display the file)
			string[] v = (psi.Verbs ?? MemUtil.EmptyArray<string>());
			foreach(string strVerb in v)
			{
				if(strVerb == null) { Debug.Assert(false); continue; }

				if(strVerb.Equals("Print", StrUtil.CaseIgnoreCmp))
				{
					psi.Verb = strVerb;
					break;
				}
			}

			NativeLib.StartProcess(psi);
		}
	}
}
