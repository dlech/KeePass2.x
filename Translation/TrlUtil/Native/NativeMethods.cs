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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace TrlUtil.Native
{
	internal static class NativeMethods
	{
		internal const int WM_UPDATEUISTATE = 0x0128;

		private const int TCM_FIRST = 0x1300;
		internal const int TCM_SETCURSEL = TCM_FIRST + 12;

		internal const int UIS_CLEAR = 2;

		internal const int UISF_HIDEACCEL = 0x2;

		internal enum ProcessDpiAwareness : uint
		{
			Unaware = 0,
			SystemAware = 1,
			PerMonitorAware = 2
		}

		[DllImport("User32.dll")]
		internal static extern IntPtr SendMessage(IntPtr hWnd, int nMsg,
			IntPtr wParam, IntPtr lParam);

		[DllImport("ShCore.dll")]
		internal static extern int SetProcessDpiAwareness(
			[MarshalAs(UnmanagedType.U4)] ProcessDpiAwareness a);

		internal static int MakeLong(int wLow, int wHigh)
		{
			return (((wHigh & 0xFFFF) << 16) | (wLow & 0xFFFF));
		}
	}
}
