/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2009 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;

namespace KeePass.Native
{
	internal static partial class NativeMethods
	{
		internal static string GetWindowText(IntPtr hWnd)
		{
			int nLength = GetWindowTextLength(hWnd);
			if(nLength <= 0) return string.Empty;

			StringBuilder sb = new StringBuilder(nLength + 1);
			GetWindowText(hWnd, sb, sb.Capacity);
			return sb.ToString();
		}

		internal static int GetWindowStyle(IntPtr hWnd)
		{
			return GetWindowLong(hWnd, GWL_STYLE);
		}

		internal static bool EnsureForegroundWindow(IntPtr hWnd)
		{
			if(IsWindow(hWnd) == false)
				return false;

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

		internal static bool LoseFocus(IntPtr hCurrentWnd)
		{
			IntPtr hWnd = GetWindow(hCurrentWnd, GW_HWNDNEXT);

			while(true)
			{
				if(hWnd != hCurrentWnd)
				{
					int nStyle = GetWindowStyle(hWnd);
					if(((nStyle & WS_VISIBLE) != 0) &&
						(GetWindowTextLength(hWnd) > 0))
					{
						break;
					}
				}

				hWnd = GetWindow(hWnd, GW_HWNDNEXT);
				if(hWnd == IntPtr.Zero) break;
			}

			if(hWnd == IntPtr.Zero) return false;

			return EnsureForegroundWindow(hWnd);
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
	}
}
