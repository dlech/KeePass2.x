/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2021 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Windows.Forms;

using KeePass.UI;
using KeePass.Util;

using KeePassLib.Utility;

using NativeLib = KeePassLib.Native.NativeLib;

namespace KeePass.Native
{
	internal static partial class NativeMethods
	{
		internal static string GetWindowText(IntPtr hWnd, bool bTrim)
		{
			// cc may be greater than the actual length;
			// https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-getwindowtextlengthw
			int cc = GetWindowTextLength(hWnd);
			if(cc <= 0) return string.Empty;

			// StringBuilder sb = new StringBuilder(cc + 2);
			// int ccReal = GetWindowText(hWnd, sb, cc + 1);
			// if(ccReal <= 0) { Debug.Assert(false); return string.Empty; }
			// // The text isn't always NULL-terminated; trim garbage
			// if(ccReal < sb.Length)
			//	sb.Remove(ccReal, sb.Length - ccReal);
			// string strWindow = sb.ToString();

			string strWindow;
			IntPtr p = IntPtr.Zero;
			try
			{
				int cbChar = Marshal.SystemDefaultCharSize;
				int cb = (cc + 2) * cbChar;
				p = Marshal.AllocCoTaskMem(cb);
				if(p == IntPtr.Zero) { Debug.Assert(false); return string.Empty; }

				byte[] pbZero = new byte[cb];
				Marshal.Copy(pbZero, 0, p, cb);

				int ccReal = GetWindowText(hWnd, p, cc + 1);
				if(ccReal <= 0) { Debug.Assert(false); return string.Empty; }

				if(ccReal <= cc)
				{
					// Ensure correct termination (in case GetWindowText
					// copied too much)
					int ibZero = ccReal * cbChar;
					for(int i = 0; i < cbChar; ++i)
						Marshal.WriteByte(p, ibZero + i, 0);
				}
				else { Debug.Assert(false); return string.Empty; }

				strWindow = (Marshal.PtrToStringAuto(p) ?? string.Empty);
			}
			finally { if(p != IntPtr.Zero) Marshal.FreeCoTaskMem(p); }

			return (bTrim ? strWindow.Trim() : strWindow);
		}

		/* internal static string GetWindowClassName(IntPtr hWnd)
		{
			try
			{
				StringBuilder sb = new StringBuilder(260);

				if(GetClassName(hWnd, sb, 258) > 0)
					return sb.ToString();
				else { Debug.Assert(false); }

				return string.Empty;
			}
			catch(Exception) { Debug.Assert(false); }

			return null;
		} */

		internal static IntPtr GetForegroundWindowHandle()
		{
			if(!NativeLib.IsUnix())
				return GetForegroundWindow(); // Windows API

			try
			{
				return new IntPtr(long.Parse(RunXDoTool(
					"getactivewindow").Trim()));
			}
			catch(Exception) { Debug.Assert(false); }
			return IntPtr.Zero;
		}

		private static readonly char[] g_vWindowNL = new char[] { '\r', '\n' };
		internal static void GetForegroundWindowInfo(out IntPtr hWnd,
			out string strWindowText, bool bTrimWindow)
		{
			hWnd = GetForegroundWindowHandle();

			if(!NativeLib.IsUnix()) // Windows
				strWindowText = GetWindowText(hWnd, bTrimWindow);
			else // Unix
			{
				strWindowText = RunXDoTool("getactivewindow getwindowname");
				if(!string.IsNullOrEmpty(strWindowText))
				{
					if(bTrimWindow) strWindowText = strWindowText.Trim();
					else strWindowText = strWindowText.Trim(g_vWindowNL);
				}
			}
		}

		internal static bool IsWindowEx(IntPtr hWnd)
		{
			if(!NativeLib.IsUnix()) // Windows
				return IsWindow(hWnd);

			return true;
		}

		internal static int GetWindowStyle(IntPtr hWnd)
		{
			return GetWindowLong(hWnd, GWL_STYLE);
		}

		internal static IntPtr GetClassLongPtrEx(IntPtr hWnd, int nIndex)
		{
			if(IntPtr.Size == 4) return GetClassLong(hWnd, nIndex);
			return GetClassLongPtr(hWnd, nIndex);
		}

		internal static bool SetForegroundWindowEx(IntPtr hWnd)
		{
			if(!NativeLib.IsUnix())
				return SetForegroundWindow(hWnd);

			return (RunXDoTool("windowactivate " +
				hWnd.ToInt64().ToString()).Trim().Length == 0);
		}

		internal static bool EnsureForegroundWindow(IntPtr hWnd)
		{
			if(!IsWindowEx(hWnd)) return false;

			IntPtr hWndInit = GetForegroundWindowHandle();

			if(!SetForegroundWindowEx(hWnd))
			{
				Debug.Assert(false);
				return false;
			}

			int nStartMS = Environment.TickCount;
			while((Environment.TickCount - nStartMS) < 1000)
			{
				IntPtr h = GetForegroundWindowHandle();
				if(h == hWnd) return true;

				// Some applications (like Microsoft Edge) have multiple
				// windows and automatically redirect the focus to other
				// windows, thus also break when a different window gets
				// focused (except when h is zero, which can occur while
				// the focus transfer occurs)
				if((h != IntPtr.Zero) && (h != hWndInit)) return true;

				Application.DoEvents();
			}

			return false;
		}

		// Workaround for .NET/Windows TopMost/WS_EX_TOPMOST desynchronization bug;
		// https://sourceforge.net/p/keepass/discussion/329220/thread/d45a3b38e8/
		internal static void SyncTopMost(Form f)
		{
			if(f == null) { Debug.Assert(false); return; }
			if(NativeLib.IsUnix()) return;

			try
			{
				if(!f.TopMost) return; // Managed state

				IntPtr h = f.Handle;
				if(h == IntPtr.Zero) return;

				int s = GetWindowLong(h, GWL_EXSTYLE); // Unmanaged state
				if((s & WS_EX_TOPMOST) == 0)
				{
					f.TopMost = true; // Calls SetWindowPos (if TopLevel)
#if DEBUG
					Trace.WriteLine("Synchronized TopMost/WS_EX_TOPMOST.");
#endif
				}
			}
			catch(Exception) { Debug.Assert(false); }
		}

		internal static IntPtr FindWindow(string strTitle)
		{
			if(strTitle == null) { Debug.Assert(false); return IntPtr.Zero; }

			if(!NativeLib.IsUnix())
				return FindWindowEx(IntPtr.Zero, IntPtr.Zero, null, strTitle);

			// Not --onlyvisible (due to not finding minimized windows)
			string str = RunXDoTool("search --name \"" + strTitle + "\"").Trim();
			if(str.Length == 0) return IntPtr.Zero;

			long l;
			if(long.TryParse(str, out l)) return new IntPtr(l);
			return IntPtr.Zero;
		}

		internal static bool LoseFocus(Form fCurrent, bool bSkipOwnWindows)
		{
			if(NativeLib.IsUnix()) return LoseFocusUnix(fCurrent);

			try
			{
				IntPtr hWnd = ((fCurrent != null) ? fCurrent.Handle : IntPtr.Zero);

				while(true)
				{
					IntPtr hWndPrev = hWnd;
					hWnd = GetWindow(hWnd, GW_HWNDNEXT);

					if(hWnd == IntPtr.Zero) return false;
					if(hWnd == hWndPrev) { Debug.Assert(false); return false; }

					int nStyle = GetWindowStyle(hWnd);
					if((nStyle & WS_VISIBLE) == 0) continue;

					if(GetWindowTextLength(hWnd) == 0) continue;

					if(bSkipOwnWindows && GlobalWindowManager.HasWindowMW(hWnd))
						continue;

					// Skip the taskbar window (required for Windows 7,
					// when the target window is the only other window
					// in the taskbar)
					if(IsTaskBar(hWnd)) continue;

					break;
				}

				Debug.Assert(GetWindowText(hWnd, true) != "Start");
				return EnsureForegroundWindow(hWnd);
			}
			catch(Exception) { Debug.Assert(false); }

			return false;
		}

		internal static bool IsTaskBar(IntPtr hWnd)
		{
			Process p = null;
			try
			{
				string strText = GetWindowText(hWnd, true);
				if(strText == null) return false;
				if(!strText.Equals("Start", StrUtil.CaseIgnoreCmp)) return false;

				uint uProcessId;
				NativeMethods.GetWindowThreadProcessId(hWnd, out uProcessId);

				p = Process.GetProcessById((int)uProcessId);
				string strExe = UrlUtil.GetFileName(p.MainModule.FileName).Trim();

				return strExe.Equals("Explorer.exe", StrUtil.CaseIgnoreCmp);
			}
			catch(Exception) { Debug.Assert(false); }
			finally
			{
				try { if(p != null) p.Dispose(); }
				catch(Exception) { Debug.Assert(false); }
			}

			return false;
		}

		/* internal static bool IsMetroWindow(IntPtr hWnd)
		{
			if(hWnd == IntPtr.Zero) { Debug.Assert(false); return false; }
			if(NativeLib.IsUnix() || !WinUtil.IsAtLeastWindows8)
				return false;

			try
			{
				uint uProcessId;
				NativeMethods.GetWindowThreadProcessId(hWnd, out uProcessId);
				if(uProcessId == 0) { Debug.Assert(false); return false; }

				IntPtr h = NativeMethods.OpenProcess(NativeMethods.PROCESS_QUERY_INFORMATION,
					false, uProcessId);
				if(h == IntPtr.Zero) return false; // No assert

				bool bRet = NativeMethods.IsImmersiveProcess(h);

				NativeMethods.CloseHandle(h);
				return bRet;
			}
			catch(Exception) { Debug.Assert(false); }

			return false;
		} */

		public static bool IsInvalidHandleValue(IntPtr p)
		{
			long h = p.ToInt64();
			if(h == -1) return true;
			if(h == 0xFFFFFFFF) return true;

			return false;
		}

		public static int GetHeaderHeight(ListView lv)
		{
			if(lv == null) { Debug.Assert(false); return 0; }

			try
			{
				if((lv.View == View.Details) && (lv.HeaderStyle !=
					ColumnHeaderStyle.None) && (lv.Columns.Count > 0) &&
					!NativeLib.IsUnix())
				{
					IntPtr hHeader = NativeMethods.SendMessage(lv.Handle,
						NativeMethods.LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
					if(hHeader != IntPtr.Zero)
					{
						NativeMethods.RECT rect = new NativeMethods.RECT();
						if(NativeMethods.GetWindowRect(hHeader, ref rect))
							return (rect.Bottom - rect.Top);
						else { Debug.Assert(false); }
					}
					else { Debug.Assert(false); }
				}
			}
			catch(Exception) { Debug.Assert(false); }

			return 0;
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
			if(NativeLib.IsUnix()) return 0;

			try
			{
				SCROLLINFO si = new SCROLLINFO();
				si.cbSize = (uint)Marshal.SizeOf(typeof(SCROLLINFO));
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
			if(NativeLib.IsUnix()) return;

			try { SendMessage(lv.Handle, LVM_SCROLL, (IntPtr)dx, (IntPtr)dy); }
			catch(Exception) { Debug.Assert(false); }
		}

		/* public static void ScrollAbsY(IntPtr hWnd, int y)
		{
			try
			{
				SCROLLINFO si = new SCROLLINFO();
				si.cbSize = (uint)Marshal.SizeOf(typeof(SCROLLINFO));
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
				Debug.Assert(NativeLib.IsUnix() || WinUtil.IsWindows9x);
			}

			return null;
		}

		internal static bool SHGetFileInfo(string strPath, int dxImg, int dyImg,
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
						img = UIUtil.IconToBitmap(ico, dxImg, dyImg);
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
				// https://sourceforge.net/p/keepass/discussion/329221/thread/65244cc9/
				if(!NativeLib.IsUnix())
					return (GetFileAttributes(strFilePath) != INVALID_FILE_ATTRIBUTES);
			}
			catch(Exception) { Debug.Assert(false); }

			// Fallback to .NET method (for Unix-like systems)
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
			g.cbSize = (uint)Marshal.SizeOf(typeof(LVGROUP));

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
			g.cbSize = (uint)Marshal.SizeOf(typeof(LVGROUP));

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
			g.cbSize = (uint)Marshal.SizeOf(typeof(LVGROUP));
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
			g.cbSize = (uint)Marshal.SizeOf(typeof(LVGROUP));
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

		/* internal static void SetListViewGroupInfo(ListView lv, int iGroupID,
			string strTask, bool? obCollapsible)
		{
			if(lv == null) { Debug.Assert(false); return; }
			if(!WinUtil.IsAtLeastWindowsVista) return;

			LVGROUP g = new LVGROUP();
			g.cbSize = (uint)Marshal.SizeOf(typeof(LVGROUP));
			g.AssertSize();

			if(strTask != null)
			{
				g.mask |= LVGF_TASK;

				g.pszTask = strTask;
				g.cchTask = (uint)strTask.Length;
			}

			if(obCollapsible.HasValue)
			{
				g.mask |= LVGF_STATE;

				g.stateMask = LVGS_COLLAPSIBLE;
				g.state = (obCollapsible.Value ? LVGS_COLLAPSIBLE : 0);
			}

			if(g.mask == 0) return;
			if(SendMessageLVGroup(lv.Handle, LVM_SETGROUPINFO,
				new IntPtr(iGroupID), ref g) == (new IntPtr(-1)))
			{
				Debug.Assert(false);
			}
		}

		internal static int GetListViewGroupID(ListViewGroup lvg)
		{
			if(lvg == null) { Debug.Assert(false); return -1; }

			Type t = typeof(ListViewGroup);
			PropertyInfo pi = t.GetProperty("ID", (BindingFlags.Instance |
				BindingFlags.NonPublic));
			if(pi == null) { Debug.Assert(false); return -1; }
			if(pi.PropertyType != typeof(int)) { Debug.Assert(false); return -1; }

			return (int)pi.GetValue(lvg, null);
		} */

		private static bool GetDesktopName(IntPtr hDesk, out string strAnsi,
			out string strUni)
		{
			strAnsi = null;
			strUni = null;

			const uint cbZ = 12; // Minimal number of terminating zeros
			const uint uBufSize = 64 + cbZ;
			IntPtr pBuf = Marshal.AllocCoTaskMem((int)uBufSize);
			byte[] pbZero = new byte[uBufSize];
			Marshal.Copy(pbZero, 0, pBuf, pbZero.Length);

			try
			{
				uint uReqSize = uBufSize - cbZ;
				bool bSuccess = GetUserObjectInformation(hDesk, 2, pBuf,
					uBufSize - cbZ, ref uReqSize);
				if(uReqSize > (uBufSize - cbZ))
				{
					Marshal.FreeCoTaskMem(pBuf);
					pBuf = Marshal.AllocCoTaskMem((int)(uReqSize + cbZ));
					pbZero = new byte[uReqSize + cbZ];
					Marshal.Copy(pbZero, 0, pBuf, pbZero.Length);

					bSuccess = GetUserObjectInformation(hDesk, 2, pBuf,
						uReqSize, ref uReqSize);
					Debug.Assert((uReqSize + cbZ) == (uint)pbZero.Length);
				}

				if(bSuccess)
				{
					try { strAnsi = Marshal.PtrToStringAnsi(pBuf).Trim(); }
					catch(Exception) { }

					try { strUni = Marshal.PtrToStringUni(pBuf).Trim(); }
					catch(Exception) { }

					return true;
				}
			}
			finally { Marshal.FreeCoTaskMem(pBuf); }

			Debug.Assert(false);
			return false;
		}

		// The GetUserObjectInformation function apparently returns the
		// desktop name using ANSI encoding even on Windows 7 systems.
		// As the encoding is not documented, we test both ANSI and
		// Unicode versions of the name.
		internal static bool? DesktopNameContains(IntPtr hDesk, string strName)
		{
			if(string.IsNullOrEmpty(strName)) { Debug.Assert(false); return false; }

			string strAnsi, strUni;
			if(!GetDesktopName(hDesk, out strAnsi, out strUni)) return null;
			if((strAnsi == null) && (strUni == null)) return null;

			try
			{
				if((strAnsi != null) && (strAnsi.IndexOf(strName,
					StringComparison.OrdinalIgnoreCase) >= 0))
					return true;
			}
			catch(Exception) { Debug.Assert(false); }

			try
			{
				if((strUni != null) && (strUni.IndexOf(strName,
					StringComparison.OrdinalIgnoreCase) >= 0))
					return true;
			}
			catch(Exception) { Debug.Assert(false); }

			return false;
		}

		private static bool? IsKeyDownMessage(ref Message m)
		{
			if(m.Msg == NativeMethods.WM_KEYDOWN) return true;
			if(m.Msg == NativeMethods.WM_KEYUP) return false;
			if(m.Msg == NativeMethods.WM_SYSKEYDOWN) return true;
			if(m.Msg == NativeMethods.WM_SYSKEYUP) return false;
			return null;
		}

		internal static bool GetKeyMessageState(ref Message m, out bool bDown)
		{
			bool? obKeyDown = IsKeyDownMessage(ref m);
			if(!obKeyDown.HasValue)
			{
				Debug.Assert(false);
				bDown = false;
				return false;
			}

			bDown = obKeyDown.Value;
			return true;
		}

		/* internal static string GetKeyboardLayoutNameEx()
		{
			StringBuilder sb = new StringBuilder(KL_NAMELENGTH + 1);
			if(GetKeyboardLayoutName(sb))
			{
				Debug.Assert(sb.Length == (KL_NAMELENGTH - 1));
				return sb.ToString();
			}
			else { Debug.Assert(false); }

			return null;
		} */

		/// <summary>
		/// PRIMARYLANGID macro.
		/// </summary>
		internal static ushort GetPrimaryLangID(ushort uLangID)
		{
			return (ushort)(uLangID & 0x3FFU);
		}

		internal static uint MapVirtualKey3(uint uCode, uint uMapType, IntPtr hKL)
		{
			if(hKL == IntPtr.Zero) return MapVirtualKey(uCode, uMapType);
			return MapVirtualKeyEx(uCode, uMapType, hKL);
		}

		internal static ushort VkKeyScan3(char ch, IntPtr hKL)
		{
			if(hKL == IntPtr.Zero) return VkKeyScan(ch);
			return VkKeyScanEx(ch, hKL);
		}

		/// <returns>
		/// Null, if there exists no translation or an error occured.
		/// An empty string, if the key is a dead key.
		/// Otherwise, the generated Unicode string (typically 1 character,
		/// but can be more when a dead key is stored in the keyboard layout).
		/// </returns>
		internal static string ToUnicode3(int vKey, byte[] pbKeyState, IntPtr hKL)
		{
			const int cbState = 256;
			IntPtr pState = IntPtr.Zero;
			try
			{
				uint uScanCode = MapVirtualKey3((uint)vKey, MAPVK_VK_TO_VSC, hKL);

				pState = Marshal.AllocHGlobal(cbState);
				if(pState == IntPtr.Zero) { Debug.Assert(false); return null; }

				if(pbKeyState != null)
				{
					if(pbKeyState.Length == cbState)
						Marshal.Copy(pbKeyState, 0, pState, cbState);
					else { Debug.Assert(false); return null; }
				}
				else
				{
					// Windows' GetKeyboardState function does not return
					// the current virtual key array; as a workaround,
					// calling GetKeyState is mentioned sometimes, but
					// this doesn't work reliably either;
					// http://pinvoke.net/default.aspx/user32/GetKeyboardState.html

					// GetKeyState(VK_SHIFT);
					// if(!GetKeyboardState(pState)) { Debug.Assert(false); return null; }

					Debug.Assert(false);
					return null;
				}

				const int cchUni = 30;
				StringBuilder sbUni = new StringBuilder(cchUni + 2);

				int r;
				if(hKL == IntPtr.Zero)
					r = ToUnicode((uint)vKey, uScanCode, pState, sbUni,
						cchUni, 0);
				else
					r = ToUnicodeEx((uint)vKey, uScanCode, pState, sbUni,
						cchUni, 0, hKL);

				if(r < 0) return string.Empty; // Dead key
				if(r == 0) return null; // No translation

				string str = sbUni.ToString();
				if(string.IsNullOrEmpty(str)) { Debug.Assert(false); return null; }

				// Extra characters may be returned, but are invalid
				// and should be ignored;
				// https://msdn.microsoft.com/en-us/library/windows/desktop/ms646320.aspx
				if(r < str.Length) str = str.Substring(0, r);

				return str;
			}
			catch(Exception) { Debug.Assert(false); }
			finally { if(pState != IntPtr.Zero) Marshal.FreeHGlobal(pState); }

			return null;
		}
	}
}
