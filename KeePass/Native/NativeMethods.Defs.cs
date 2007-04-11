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
using System.Text;
using System.Security;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;

namespace KeePass.Native
{
	internal static partial class NativeMethods
	{
		internal const int WM_DRAWCLIPBOARD = 0x0308;
		internal const int WM_CHANGECBCHAIN = 0x030D;
		internal const int WM_HOTKEY = 0x0312;
		internal const int WM_USER = 0x0400;

		internal const uint HWND_BROADCAST = 0xFFFF;

		internal const uint INPUT_MOUSE = 0;
		internal const uint INPUT_KEYBOARD = 1;
		internal const uint INPUT_HARDWARE = 2;

		internal const int VK_SHIFT = 0x10;
		internal const int VK_CONTROL = 0x11;
		internal const int VK_MENU = 0x12;
		internal const int VK_CAPITAL = 0x14;
		internal const int VK_LSHIFT = 0xA0;
		internal const int VK_RSHIFT = 0xA1;
		internal const int VK_LCONTROL = 0xA2;
		internal const int VK_RCONTROL = 0xA3;
		internal const int VK_LMENU = 0xA4;
		internal const int VK_RMENU = 0xA5;
		internal const int VK_LWIN = 0x5B;
		internal const int VK_RWIN = 0x5C;

		internal const uint KEYEVENTF_EXTENDEDKEY = 1;
		internal const uint KEYEVENTF_KEYUP = 2;

		internal const uint GW_HWNDNEXT = 2;

		internal const int GWL_STYLE = -16;

		internal const uint WS_VISIBLE = 0x10000000;

		internal const int EM_SETCHARFORMAT = WM_USER + 68;

		internal const int SCF_SELECTION = 0x0001;

		internal const uint CFM_LINK = 0x00000020;
		internal const uint CFE_LINK = 0x00000020;

		[return: MarshalAs(UnmanagedType.Bool)]
		internal delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
	}
}
