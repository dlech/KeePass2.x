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
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

using KeePass.App;
using KeePass.Native;

using KeePassLib;
using KeePassLib.Utility;

namespace KeePass.Util
{
	public static class WinUtil
	{
		public static void OpenEntryUrl(PwEntry pe)
		{
			Debug.Assert(pe != null);
			if(pe == null) throw new ArgumentNullException("pe");

			if(pe.OverrideUrl.Length > 0)
				WinUtil.OpenUrlInNewBrowser(pe.OverrideUrl, pe);
			else
			{
				string strOverride = AppConfigEx.GetValue(AppDefs.ConfigKeys.UrlOverride);
				if(strOverride.Length > 0)
					WinUtil.OpenUrlInNewBrowser(strOverride, pe);
				else
					WinUtil.OpenUrlInNewBrowser(pe.Strings.ReadSafe(PwDefs.UrlField), pe);
			}
		}

		public static void OpenUrlInNewBrowser(string strUrlToOpen, PwEntry peDataSource)
		{
			// If URL is null, return false, do not throw exception.
			Debug.Assert(strUrlToOpen != null); if(strUrlToOpen == null) return;

			string strPrevWorkDir = Directory.GetCurrentDirectory();
			string strThisExe = WinUtil.GetExecutable();
			
			string strExeDir = UrlUtil.GetFileDirectory(strThisExe, false);
			try { Directory.SetCurrentDirectory(strExeDir); }
			catch(Exception) { Debug.Assert(false); }

			string strUrl = strUrlToOpen;
			bool bCmdQuotes = strUrl.StartsWith("cmd://");

			PwDatabase pwDatabase = null;
			try { pwDatabase = Program.MainForm.PluginHost.Database; }
			catch(Exception) { Debug.Assert(false); pwDatabase = null; }

			strUrl = StrUtil.FillPlaceholders(strUrl, peDataSource, strThisExe,
				pwDatabase, bCmdQuotes, false);
			strUrl = AppLocator.FillPlaceholders(strUrl, false);

			if(strUrl.StartsWith("cmd://"))
			{
				string strApp, strArgs;
				StrUtil.SplitCommandLine(strUrl.Remove(0, 6), out strApp, out strArgs);

				try
				{
					if((strArgs != null) && (strArgs.Length > 0))
						Process.Start(strApp, strArgs);
					else
						Process.Start(strApp);
				}
				catch(Exception exCmd)
				{
					string strInf = strApp;
					if((strArgs != null) && (strArgs.Length > 0))
						strInf += MessageService.NewLine + strArgs;
					
					MessageService.ShowWarning(strInf, exCmd);
				}
			}
			else
			{
				try { Process.Start(strUrl); }
				catch(Exception exUrl)
				{
					MessageService.ShowWarning(strUrl, exUrl);
				}
			}

			// Restore previous working directory
			try { Directory.SetCurrentDirectory(strPrevWorkDir); }
			catch(Exception) { Debug.Assert(false); }
		}

		public static void Restart()
		{
			try { Process.Start(WinUtil.GetExecutable()); }
			catch(Exception exRestart)
			{
				MessageService.ShowWarning(exRestart);
			}
		}

		public static string GetExecutable()
		{
			string strExePath = null;

			try { strExePath = Assembly.GetExecutingAssembly().Location; }
			catch(Exception) { }

			if((strExePath == null) || (strExePath.Length == 0))
			{
				strExePath = Assembly.GetExecutingAssembly().GetName().CodeBase;
				strExePath = UrlUtil.FileUrlToPath(strExePath);
			}

			return strExePath;
		}

		private const string FontPartsSeparator = @"/:/";

		public static Font FontIDToFont(string strFontID)
		{
			Debug.Assert(strFontID != null); if(strFontID == null) return null;

			string[] vParts = strFontID.Split(new string[] { FontPartsSeparator },
				StringSplitOptions.None);
			if((vParts == null) || (vParts.Length != 6)) return null;

			float fSize;
			if(!float.TryParse(vParts[1], out fSize)) { Debug.Assert(false); return null; }

			FontStyle fs = FontStyle.Regular;
			if(vParts[2] == "1") fs |= FontStyle.Bold;
			if(vParts[3] == "1") fs |= FontStyle.Italic;
			if(vParts[4] == "1") fs |= FontStyle.Underline;
			if(vParts[5] == "1") fs |= FontStyle.Strikeout;

			return new Font(vParts[0], fSize, fs, GraphicsUnit.Point);
		}

		public static string FontToFontID(Font f)
		{
			Debug.Assert(f != null); if(f == null) return string.Empty;

			StringBuilder sb = new StringBuilder();

			sb.Append(f.Name);
			sb.Append(FontPartsSeparator);
			sb.Append(f.SizeInPoints.ToString());
			sb.Append(FontPartsSeparator);
			sb.Append(f.Bold ? "1" : "0");
			sb.Append(FontPartsSeparator);
			sb.Append(f.Italic ? "1" : "0");
			sb.Append(FontPartsSeparator);
			sb.Append(f.Underline ? "1" : "0");
			sb.Append(FontPartsSeparator);
			sb.Append(f.Strikeout ? "1" : "0");

			return sb.ToString();
		}

		/// <summary>
		/// Shorten a path.
		/// </summary>
		/// <param name="strPath">Path to make shorter.</param>
		/// <param name="nMaxChars">Maximum number of characters in the returned string.</param>
		/// <returns>Shortened path.</returns>
		public static string CompactPath(string strPath, int nMaxChars)
		{
			Debug.Assert(strPath != null);
			if(strPath == null) throw new ArgumentNullException("strPath");
			Debug.Assert(nMaxChars >= 0);
			if(nMaxChars < 0) throw new ArgumentOutOfRangeException("nMaxChars");

			if(nMaxChars == 0) return string.Empty;
			if(strPath.Length <= nMaxChars) return strPath;

			try
			{
				StringBuilder sb = new StringBuilder(strPath.Length * 2 + 4);

				if(NativeMethods.PathCompactPathEx(sb, strPath, (uint)nMaxChars, 0) == false)
					return StrUtil.CompactString3Dots(strPath, nMaxChars);

				Debug.Assert(sb.Length <= nMaxChars);
				if((sb.Length <= nMaxChars) && (sb.Length != 0))
					return sb.ToString();
				else
					return StrUtil.CompactString3Dots(strPath, nMaxChars);
			}
			catch(Exception) { Debug.Assert(false); }

			return StrUtil.CompactString3Dots(strPath, nMaxChars);
		}
	}
}
