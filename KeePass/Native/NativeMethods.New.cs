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
using System.Text;
using System.Security;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;

using KeePass.UI;
using KeePass.Util;

using KeePassLib.Utility;

namespace KeePass.Native
{
	internal static partial class NativeMethods
	{
		internal static string GetWindowText(IntPtr hWnd, bool bTrim)
		{
			int nLength = GetWindowTextLength(hWnd);
			if(nLength <= 0) return string.Empty;

			StringBuilder sb = new StringBuilder(nLength + 1);
			GetWindowText(hWnd, sb, sb.Capacity);
			string strWindow = sb.ToString();

			return (bTrim ? strWindow.Trim() : strWindow);
		}

		internal static IntPtr GetForegroundWindowHandle()
		{
			if(!KeePassLib.Native.NativeLib.IsUnix())
				return GetForegroundWindow(); // Windows API

			try
			{
				return new IntPtr(int.Parse(RunXDoTool("getactivewindow")));
			}
			catch(Exception) { Debug.Assert(false); }
			return IntPtr.Zero;
		}

		private static readonly char[] m_vWindowTrim = { '\r', '\n' };
		internal static void GetForegroundWindowInfo(out IntPtr hWnd,
			out string strWindowText, bool bTrimWindow)
		{
			hWnd = GetForegroundWindowHandle();

			if(!KeePassLib.Native.NativeLib.IsUnix()) // Windows
				strWindowText = GetWindowText(hWnd, bTrimWindow);
			else // Unix
			{
				strWindowText = RunXDoTool("getactivewindow getwindowname");
				if(!string.IsNullOrEmpty(strWindowText))
				{
					if(bTrimWindow) strWindowText = strWindowText.Trim();
					else strWindowText = strWindowText.Trim(m_vWindowTrim);
				}
			}
		}

		internal static bool IsWindowEx(IntPtr hWnd)
		{
			if(!KeePassLib.Native.NativeLib.IsUnix()) // Windows
				return IsWindow(hWnd);

			return true;
		}

		internal static int GetWindowStyle(IntPtr hWnd)
		{
			return GetWindowLong(hWnd, GWL_STYLE);
		}

		internal static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex)
		{
			if(IntPtr.Size > 4) return GetClassLongPtr64(hWnd, nIndex);
			return GetClassLongPtr32(hWnd, nIndex);
		}

		internal static bool SetForegroundWindowEx(IntPtr hWnd)
		{
			if(!KeePassLib.Native.NativeLib.IsUnix())
				return SetForegroundWindow(hWnd);

			return (RunXDoTool("windowactivate " +
				hWnd.ToInt64().ToString()).Trim().Length == 0);
		}

		internal static bool EnsureForegroundWindow(IntPtr hWnd)
		{
			if(IsWindowEx(hWnd) == false) return false;

			if(SetForegroundWindowEx(hWnd) == false)
			{
				Debug.Assert(false);
				return false;
			}

			int nStartMS = Environment.TickCount;
			while((Environment.TickCount - nStartMS) < 1000)
			{
				IntPtr h = GetForegroundWindowHandle();
				if(h == hWnd) return true;

				Application.DoEvents();
			}

			return false;
		}

		internal static bool LoseFocus(Form fCurrent)
		{
			if(KeePassLib.Native.NativeLib.IsUnix())
				return LoseFocusUnix(fCurrent);

			try
			{
				IntPtr hCurrentWnd = ((fCurrent != null) ? fCurrent.Handle : IntPtr.Zero);
				IntPtr hWnd = GetWindow(hCurrentWnd, GW_HWNDNEXT);

				while(true)
				{
					if(hWnd != hCurrentWnd)
					{
						int nStyle = GetWindowStyle(hWnd);
						if(((nStyle & WS_VISIBLE) != 0) &&
							(GetWindowTextLength(hWnd) > 0))
						{
							// Skip the taskbar window (required for Windows 7,
							// when the target window is the only other window
							// in the taskbar)
							if(!IsTaskBar(hWnd)) break;
						}
					}

					hWnd = GetWindow(hWnd, GW_HWNDNEXT);
					if(hWnd == IntPtr.Zero) break;
				}

				if(hWnd == IntPtr.Zero) return false;

				Debug.Assert(GetWindowText(hWnd, true) != "Start");
				return EnsureForegroundWindow(hWnd);
			}
			catch(Exception) { Debug.Assert(false); }

			return false;
		}

		internal static bool IsTaskBar(IntPtr hWnd)
		{
			try
			{
				string strText = GetWindowText(hWnd, true);
				if(strText == null) return false;
				if(!strText.Equals("Start", StrUtil.CaseIgnoreCmp)) return false;

				uint uProcessId;
				NativeMethods.GetWindowThreadProcessId(hWnd, out uProcessId);

				Process p = Process.GetProcessById((int)uProcessId);
				string strExe = UrlUtil.GetFileName(p.MainModule.FileName).Trim();

				return strExe.Equals("Explorer.exe", StrUtil.CaseIgnoreCmp);
			}
			catch(Exception) { Debug.Assert(false); }

			return false;
		}

		public static bool IsInvalidHandleValue(IntPtr p)
		{
			long h = p.ToInt64();
			if(h == -1) return true;
			if(h == 0xFFFFFFFF) return true;

			return false;
		}

		// Workaround for only partially visible list view items
		/* public static void EnsureVisible(ListView lv, int nIndex, bool bPartialOK)
		{
			Debug.Assert(lv != null); if(lv == null) return;
			Debug.Assert(nIndex >= 0); if(nIndex < 0) return;
			Debug.Assert(nIndex < lv.Items.Count); if(nIndex >= lv.Items.Count) return;

			int nPartialOK = (bPartialOK ? 1 : 0);
			try
			{
				NativeMethods.SendMessage(lv.Handle, LVM_ENSUREVISIBLE,
					new IntPtr(nIndex), new IntPtr(nPartialOK));
			}
			catch(Exception) { Debug.Assert(false); }
		} */

		public static int GetScrollPosY(IntPtr hWnd)
		{
			try
			{
				SCROLLINFO si = new SCROLLINFO();
				si.cbSize = (uint)Marshal.SizeOf(si);
				si.fMask = (uint)ScrollInfoMask.SIF_POS;

				if(GetScrollInfo(hWnd, (int)ScrollBarDirection.SB_VERT, ref si))
					return si.nPos;

				Debug.Assert(false);
			}
			catch(Exception) { Debug.Assert(false); }

			return 0;
		}

		public static void Scroll(ListView lv, int dx, int dy)
		{
			if(lv == null) throw new ArgumentNullException("lv");

			try { SendMessage(lv.Handle, LVM_SCROLL, (IntPtr)dx, (IntPtr)dy); }
			catch(Exception) { Debug.Assert(false); }
		}

		/* public static void ScrollAbsY(IntPtr hWnd, int y)
		{
			try
			{
				SCROLLINFO si = new SCROLLINFO();
				si.cbSize = (uint)Marshal.SizeOf(si);
				si.fMask = (uint)ScrollInfoMask.SIF_POS;
				si.nPos = y;

				SetScrollInfo(hWnd, (int)ScrollBarDirection.SB_VERT, ref si, true);
			}
			catch(Exception) { Debug.Assert(false); }
		} */

		/* public static void Scroll(IntPtr h, int dx, int dy)
		{
			if(h == IntPtr.Zero) { Debug.Assert(false); return; } // No throw

			try
			{
				ScrollWindowEx(h, dx, dy, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero,
					IntPtr.Zero, SW_INVALIDATE);
			}
			catch(Exception) { Debug.Assert(false); }
		} */

		/* internal static void ClearIconicBitmaps(IntPtr hWnd)
		{
			// TaskbarList.SetThumbnailClip(hWnd, new Rectangle(0, 0, 1, 1));
			// TaskbarList.SetThumbnailClip(hWnd, null);

			try { DwmInvalidateIconicBitmaps(hWnd); }
			catch(Exception) { Debug.Assert(!WinUtil.IsAtLeastWindows7); }
		} */

		internal static uint? GetLastInputTime()
		{
			try
			{
				LASTINPUTINFO lii = new LASTINPUTINFO();
				lii.cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO));

				if(!GetLastInputInfo(ref lii)) { Debug.Assert(false); return null; }

				return lii.dwTime;
			}
			catch(Exception)
			{
				Debug.Assert(KeePassLib.Native.NativeLib.IsUnix() ||
					WinUtil.IsWindows9x);
			}

			return null;
		}

		internal static bool SHGetFileInfo(string strPath, int nPrefImgDim,
			out Image img, out string strDisplayName)
		{
			img = null;
			strDisplayName = null;

			try
			{
				SHFILEINFO fi = new SHFILEINFO();

				IntPtr p = SHGetFileInfo(strPath, 0, ref fi, (uint)Marshal.SizeOf(typeof(
					SHFILEINFO)), SHGFI_ICON | SHGFI_SMALLICON | SHGFI_DISPLAYNAME);
				if(p == IntPtr.Zero) return false;

				if(fi.hIcon != IntPtr.Zero)
				{
					using(Icon ico = Icon.FromHandle(fi.hIcon)) // Doesn't take ownership
					{
						img = new Bitmap(nPrefImgDim, nPrefImgDim);
						using(Graphics g = Graphics.FromImage(img))
						{
							g.Clear(Color.Transparent);
							g.InterpolationMode = InterpolationMode.HighQualityBicubic;
							g.SmoothingMode = SmoothingMode.HighQuality;
							g.DrawIcon(ico, new Rectangle(0, 0, nPrefImgDim, nPrefImgDim));
						}
					}

					if(!DestroyIcon(fi.hIcon)) { Debug.Assert(false); }
				}

				strDisplayName = fi.szDisplayName;
				return true;
			}
			catch(Exception) { Debug.Assert(false); }

			return false;
		}

		/// <summary>
		/// Method for testing whether a file exists or not. Also
		/// supports NTFS alternate data streams.
		/// </summary>
		/// <param name="strFilePath">Path of the file or stream.</param>
		/// <returns><c>true</c> if the file exists.</returns>
		public static bool FileExists(string strFilePath)
		{
			if(strFilePath == null) throw new ArgumentNullException("strFilePath");

			try
			{
				return (GetFileAttributes(strFilePath) != INVALID_FILE_ATTRIBUTES);
			}
			catch(Exception) { Debug.Assert(KeePassLib.Native.NativeLib.IsUnix()); }

			// Fallback to .NET method for Unix-like systems
			try { return File.Exists(strFilePath); }
			catch(Exception) { Debug.Assert(false); } // Invalid path

			return false;
		}

		/* internal static LVGROUP GetGroupInfoByIndex(ListView lv, uint uIndex)
		{
			if(lv == null) throw new ArgumentNullException("lv");
			if(uIndex >= (uint)lv.Groups.Count)
				throw new ArgumentOutOfRangeException("uIndex");

			const int nStrLen = 1024;

			LVGROUP g = new LVGROUP();
			g.cbSize = (uint)Marshal.SizeOf(typeof(LVGROUP));

			g.mask = ...;

			g.pszHeader = new StringBuilder(nStrLen);
			g.cchHeader = nStrLen - 1;
			g.pszFooter = new StringBuilder(nStrLen);
			g.cchFooter = nStrLen - 1;
			g.pszSubtitle = new StringBuilder(nStrLen);
			g.cchSubtitle = (uint)(nStrLen - 1);
			g.pszTask = new StringBuilder(nStrLen);
			g.cchTask = (uint)(nStrLen - 1);
			g.pszDescriptionTop = new StringBuilder(nStrLen);
			g.cchDescriptionTop = (uint)(nStrLen - 1);
			g.pszDescriptionBottom = new StringBuilder(nStrLen);
			g.cchDescriptionBottom = (uint)(nStrLen - 1);
			g.pszSubsetTitle = new StringBuilder(nStrLen);
			g.cchSubsetTitle = (uint)(nStrLen - 1);

			SendMessageLVGroup(lv.Handle, LVM_GETGROUPINFOBYINDEX,
				new IntPtr((int)uIndex), ref g);
			return g;
		} */

		/* internal static uint GetGroupStateByIndex(ListView lv, uint uIndex,
			uint uStateMask, out int iGroupID)
		{
			if(lv == null) throw new ArgumentNullException("lv");
			if(uIndex >= (uint)lv.Groups.Count)
				throw new ArgumentOutOfRangeException("uIndex");

			LVGROUP g = new LVGROUP();
			g.cbSize = (uint)Marshal.SizeOf(g);

			g.mask = (LVGF_STATE | LVGF_GROUPID);
			g.stateMask = uStateMask;

			SendMessageLVGroup(lv.Handle, LVM_GETGROUPINFOBYINDEX,
				new IntPtr((int)uIndex), ref g);

			iGroupID = g.iGroupId;
			return g.state;
		}

		internal static void SetGroupState(ListView lv, int iGroupID,
			uint uStateMask, uint uState)
		{
			if(lv == null) throw new ArgumentNullException("lv");

			LVGROUP g = new LVGROUP();
			g.cbSize = (uint)Marshal.SizeOf(g);

			g.mask = LVGF_STATE;
			g.stateMask = uStateMask;
			g.state = uState;

			SendMessageLVGroup(lv.Handle, LVM_SETGROUPINFO,
				new IntPtr(iGroupID), ref g);
		} */

		/* internal static int GetGroupIDByIndex(ListView lv, uint uIndex)
		{
			if(lv == null) { Debug.Assert(false); return 0; }

			LVGROUP g = new LVGROUP();
			g.cbSize = (uint)Marshal.SizeOf(g);
			g.AssertSize();

			g.mask = NativeMethods.LVGF_GROUPID;

			if(SendMessageLVGroup(lv.Handle, LVM_GETGROUPINFOBYINDEX,
				new IntPtr((int)uIndex), ref g) == IntPtr.Zero)
			{
				Debug.Assert(false);
			}

			return g.iGroupId;
		}

		internal static void SetGroupTask(ListView lv, int iGroupID,
			string strTask)
		{
			if(lv == null) { Debug.Assert(false); return; }

			LVGROUP g = new LVGROUP();
			g.cbSize = (uint)Marshal.SizeOf(g);
			g.AssertSize();

			g.mask = LVGF_TASK;

			g.pszTask = strTask;
			g.cchTask = (uint)((strTask != null) ? strTask.Length : 0);

			if(SendMessageLVGroup(lv.Handle, LVM_SETGROUPINFO,
				new IntPtr(iGroupID), ref g) == (new IntPtr(-1)))
			{
				Debug.Assert(false);
			}
		} */
	}
}
