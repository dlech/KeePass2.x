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

using KeePass.App;

using KeePassLib;
using KeePassLib.Utility;

namespace KeePass.Util
{
	public static class WinUtil
	{
		public static void OpenUrlInNewBrowser(string strUrlToOpen, PwEntry peDataSource)
		{
			// If URL is null, return false, do not throw exception.
			Debug.Assert(strUrlToOpen != null); if(strUrlToOpen == null) return;

			string strUrl = strUrlToOpen;
			bool bCmdQuotes = strUrl.StartsWith("cmd://");

			PwDatabase pwDatabase = null;
			try { pwDatabase = Program.MainForm.PluginHost.Database; }
			catch(Exception) { Debug.Assert(false); pwDatabase = null; }

			strUrl = StrUtil.FillPlaceholders(strUrl, peDataSource, WinUtil.GetExecutable(),
				pwDatabase, bCmdQuotes);

			strUrl = AppLocator.FillPlaceholders(strUrl);

			Process p = null;
			if(strUrl.StartsWith("cmd://"))
			{
				string strApp, strArgs;
				StrUtil.SplitCommandLine(strUrl.Remove(0, 6), out strApp, out strArgs);

				try { p = Process.Start(strApp, strArgs); }
				catch(Exception) { p = null; }
			}
			else
			{
				try { p = Process.Start(strUrl); }
				catch(Exception) { p = null; }
			}
		}

		public static void Restart()
		{
			try { Process.Start(WinUtil.GetExecutable()); }
			catch(Exception) { Debug.Assert(false); }
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

		public static string PathSubstPlaceholders(string strPath)
		{
			Debug.Assert(strPath != null); if(strPath == null) return string.Empty;

			string str = strPath;

			string strAppBase = UrlUtil.GetFileDirectory(WinUtil.GetExecutable(), false);
			str = str.Replace("{AppDir}", strAppBase);

			return str;
		}

		[DllImport("User32.dll", EntryPoint = "GetForegroundWindow")]
		private static extern IntPtr Win32GetForegroundWindow();

		public static IntPtr GetForegroundWindow()
		{
			IntPtr hWnd = IntPtr.Zero;

			try { hWnd = WinUtil.Win32GetForegroundWindow(); }
			catch(Exception) { Debug.Assert(false); hWnd = IntPtr.Zero; }

			return hWnd;
		}

		[DllImport("User32.dll", EntryPoint = "IsWindow")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool Win32IsWindow(IntPtr hWnd);

		public static bool IsWindow(IntPtr hWnd)
		{
			bool bResult = false;

			try { bResult = WinUtil.Win32IsWindow(hWnd); }
			catch(Exception) { Debug.Assert(false); bResult = false; }

			return bResult;
		}

		[DllImport("User32.dll", EntryPoint = "SetForegroundWindow")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool Win32SetForegroundWindow(IntPtr hWnd);

		public static bool SetForegroundWindow(IntPtr hWnd)
		{
			bool bResult = false;

			try { bResult = WinUtil.Win32SetForegroundWindow(hWnd); }
			catch(Exception) { Debug.Assert(false); bResult = false; }

			return bResult;
		}

		public static bool EnsureForegroundWindow(IntPtr hWnd)
		{
			if(IsWindow(hWnd) == false) return false;

			if(SetForegroundWindow(hWnd) == false)
			{
				Debug.Assert(false);
				return false;
			}

			int nStartMS = Environment.TickCount;
			while((Environment.TickCount - nStartMS) < 1000)
			{
				IntPtr h = GetForegroundWindow();
				if(h == hWnd) return true;

				Application.DoEvents();
			}

			return false;
		}

		[DllImport("User32.dll", EntryPoint = "GetWindowTextLength", SetLastError = true)]
		private static extern int Win32GetWindowTextLength(IntPtr hWnd);

		public static int GetWindowTextLength(IntPtr hWnd)
		{
			int nLength = 0;

			try { nLength = WinUtil.Win32GetWindowTextLength(hWnd); }
			catch(Exception) { Debug.Assert(false); nLength = 0; }

			return nLength;
		}

		[DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		private static extern int GetWindowText(IntPtr hWnd, [Out] StringBuilder lpString, int nMaxCount);

		public static string GetWindowText(IntPtr hWnd)
		{
			int nLength = WinUtil.GetWindowTextLength(hWnd);
			if(nLength <= 0) return string.Empty;

			try
			{
				StringBuilder sb = new StringBuilder(nLength + 1);
				WinUtil.GetWindowText(hWnd, sb, sb.Capacity);
				return sb.ToString();
			}
			catch(Exception) { Debug.Assert(false); return string.Empty; }
		}

		[DllImport("User32.dll")]
		private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

		public const uint GW_HWNDNEXT = 2;

		[DllImport("User32.dll")]
		private static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

		public static uint GetWindowStyle(IntPtr hWnd)
		{
			return WinUtil.GetWindowLong(hWnd, GWL_STYLE);
		}

		public const int GWL_STYLE = -16;

		public const uint WS_VISIBLE = 0x10000000;

		public static bool LoseFocus(IntPtr hCurrentWnd)
		{
			IntPtr hWnd = WinUtil.GetWindow(hCurrentWnd, GW_HWNDNEXT);

			while(true)
			{
				if(hWnd != hCurrentWnd)
				{
					uint uStyle = WinUtil.GetWindowLong(hWnd, GWL_STYLE);

					if(((uStyle & WS_VISIBLE) != 0) && (GetWindowTextLength(hWnd) > 0))
						break;
				}

				hWnd = WinUtil.GetWindow(hWnd, GW_HWNDNEXT);
				if(hWnd == IntPtr.Zero) break;
			}

			if(hWnd == IntPtr.Zero) return false;

			return WinUtil.EnsureForegroundWindow(hWnd);
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

		public const uint HWND_BROADCAST = 0xFFFF;

		[DllImport("User32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
		private static extern IntPtr SendMessageAPI(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

		public static void SendMessage(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam)
		{
			SendMessageAPI(hWnd, uMsg, wParam, lParam);
		}

		[DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		private static extern uint RegisterWindowMessage(string lpString);

		public static uint RegisterMessage(string strMessage)
		{
			return RegisterWindowMessage(strMessage);
		}

		[return: MarshalAs(UnmanagedType.Bool)]
		public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

		[DllImport("User32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

		public static bool EnumerateWindows(EnumWindowsProc proc, IntPtr lParam)
		{
			return WinUtil.EnumWindows(proc, lParam);
		}
	}
}
