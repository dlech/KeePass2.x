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

namespace KeePass.Native
{
	internal static partial class NativeMethods
	{
		internal const int WM_KEYDOWN = 0x0100;
		internal const int WM_KEYUP = 0x0101;
		internal const int WM_DRAWCLIPBOARD = 0x0308;
		internal const int WM_CHANGECBCHAIN = 0x030D;
		internal const int WM_HOTKEY = 0x0312;
		internal const int WM_USER = 0x0400;
		internal const int WM_SYSCOMMAND = 0x0112;
		internal const int WM_POWERBROADCAST = 0x0218;
		internal const int WM_COPYDATA = 0x004A;

		internal const int HWND_BROADCAST = 0xFFFF;

		internal const uint INPUT_MOUSE = 0;
		internal const uint INPUT_KEYBOARD = 1;
		internal const uint INPUT_HARDWARE = 2;

		internal const int VK_RETURN = 0x0D;
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

		internal const int VK_F5 = 0x74;
		internal const int VK_F6 = 0x75;
		internal const int VK_F7 = 0x76;
		internal const int VK_F8 = 0x77;

		internal const uint KEYEVENTF_EXTENDEDKEY = 1;
		internal const uint KEYEVENTF_KEYUP = 2;

		internal const uint GW_HWNDNEXT = 2;

		internal const int GWL_STYLE = -16;

		internal const int WS_VISIBLE = 0x10000000;

		internal const int EM_SETCHARFORMAT = WM_USER + 68;

		internal const int ES_WANTRETURN = 0x1000;

		internal const int SCF_SELECTION = 0x0001;

		internal const uint CFM_LINK = 0x00000020;
		internal const uint CFE_LINK = 0x00000020;

		internal const int SC_MINIMIZE = 0xF020;
		internal const int SC_MAXIMIZE = 0xF030;

		internal const int IDANI_CAPTION = 3;

		internal const int PBT_APMQUERYSUSPEND = 0x0000;
		internal const int PBT_APMSUSPEND = 0x0004;

		internal const int ECM_FIRST = 0x1500;
		internal const int EM_SETCUEBANNER = ECM_FIRST + 1;

		internal const uint FSCTL_LOCK_VOLUME = 589848;
		internal const uint FSCTL_UNLOCK_VOLUME = 589852;

		internal const int LVM_FIRST = 0x1000;
		internal const int LVM_ENSUREVISIBLE = LVM_FIRST + 19;

		internal const int WM_MOUSEACTIVATE = 0x21;
		internal const int MA_ACTIVATE = 1;
		internal const int MA_ACTIVATEANDEAT = 2;
		internal const int MA_NOACTIVATE = 3;
		internal const int MA_NOACTIVATEANDEAT = 4;

		internal const int BCM_SETSHIELD = 0x160C;

		internal const int SHCNE_ASSOCCHANGED = 0x08000000;
		internal const uint SHCNF_IDLIST = 0x0000;

		[return: MarshalAs(UnmanagedType.Bool)]
		internal delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

		internal enum ComboBoxButtonState : uint
		{
			STATE_SYSTEM_NONE = 0,
			STATE_SYSTEM_INVISIBLE = 0x00008000,
			STATE_SYSTEM_PRESSED = 0x00000008
		}

		[Flags]
		internal enum DesktopFlags : uint
		{
			ReadObjects = 0x0001,
			CreateWindow = 0x0002,
			CreateMenu = 0x0004,
			HookControl = 0x0008,
			JournalRecord = 0x0010,
			JournalPlayback = 0x0020,
			Enumerate = 0x0040,
			WriteObjects = 0x0080,
			SwitchDesktop = 0x0100,
		}

		[Flags]
		internal enum EFileAccess : uint
		{
			GenericRead = 0x80000000,
			GenericWrite = 0x40000000,
			GenericExecute = 0x20000000,
			GenericAll = 0x10000000
		}

		[Flags]
		internal enum EFileShare : uint
		{
			None = 0x00000000,
			Read = 0x00000001,
			Write = 0x00000002,
			Delete = 0x00000004
		}

		internal enum ECreationDisposition : uint
		{
			CreateNew = 1,
			CreateAlways = 2,
			OpenExisting = 3,
			OpenAlways = 4,
			TruncateExisting = 5
		}
	}
}
