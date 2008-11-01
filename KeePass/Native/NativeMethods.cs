/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2008 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePass.UI;

namespace KeePass.Native
{
	internal static partial class NativeMethods
	{
		[DllImport("User32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool IsWindow(IntPtr hWnd);

		[DllImport("User32.dll")]
		internal static extern IntPtr SendMessage(IntPtr hWnd, int nMsg,
			IntPtr wParam, IntPtr lParam);

		[DllImport("User32.dll", SetLastError = true)]
		internal static extern int RegisterWindowMessage(string lpString);

		[DllImport("User32.dll")]
		internal static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

		[DllImport("User32.dll")]
		internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);

		[DllImport("User32.dll", SetLastError = true)]
		internal static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		[DllImport("User32.dll", SetLastError = true)]
		internal static extern int GetWindowTextLength(IntPtr hWnd);

		[DllImport("User32.dll", SetLastError = true)]
		private static extern int GetWindowText(IntPtr hWnd,
			[Out] StringBuilder lpString, int nMaxCount);

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

		[DllImport("User32.dll")]
		internal static extern IntPtr GetForegroundWindow();

		[DllImport("User32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("User32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool EnumWindows(EnumWindowsProc lpEnumFunc,
			IntPtr lParam);

		[DllImport("User32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers,
			uint vk);

		[DllImport("User32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);

		[DllImport("User32.dll", EntryPoint = "SendInput", SetLastError = true)]
		internal static extern uint SendInput32(uint nInputs, INPUT32[] pInputs, int cbSize);

		[DllImport("User32.dll", EntryPoint = "SendInput", SetLastError = true)]
		internal static extern uint SendInput64Special(uint nInputs,
			SpecializedKeyboardINPUT64[] pInputs, int cbSize);

		[DllImport("User32.dll")]
		internal static extern IntPtr GetMessageExtraInfo();

		[DllImport("User32.dll")]
		internal static extern uint MapVirtualKey(uint uCode, uint uMapType);

		[DllImport("User32.dll")]
		internal static extern ushort GetKeyState(int vKey);

		[DllImport("User32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool BlockInput([MarshalAs(UnmanagedType.Bool)]
			bool fBlockIt);

		[DllImport("User32.dll")]
		internal static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

		[DllImport("User32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

		[DllImport("ShlWApi.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool PathCompactPathEx(StringBuilder pszOut,
			string szPath, uint cchMax, uint dwFlags);

		[DllImport("User32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool DrawAnimatedRects(IntPtr hWnd,
			int idAni, [In] ref RECT lprcFrom, [In] ref RECT lprcTo);

		[DllImport("User32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GetComboBoxInfo(IntPtr hWnd,
			ref COMBOBOXINFO pcbi);

		[DllImport("User32.dll", SetLastError = true)]
		internal static extern IntPtr CreateDesktop(string lpszDesktop,
			string lpszDevice, IntPtr pDevMode, UInt32 dwFlags,
			[MarshalAs(UnmanagedType.U4)] DesktopFlags dwDesiredAccess,
			IntPtr lpSecurityAttributes);

		[DllImport("User32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool CloseDesktop(IntPtr hDesktop);

		// [DllImport("User32.dll", SetLastError = true)]
		// internal static extern IntPtr OpenDesktop(string lpszDesktop,
		//	UInt32 dwFlags, [MarshalAs(UnmanagedType.Bool)] bool fInherit,
		//	[MarshalAs(UnmanagedType.U4)] DesktopFlags dwDesiredAccess);

		[DllImport("User32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SwitchDesktop(IntPtr hDesktop);

		// [DllImport("User32.dll", SetLastError = true)]
		// internal static extern IntPtr OpenInputDesktop(uint dwFlags,
		//	[MarshalAs(UnmanagedType.Bool)] bool fInherit,
		//	[MarshalAs(UnmanagedType.U4)] DesktopFlags dwDesiredAccess);

		[DllImport("User32.dll", SetLastError = true)]
		internal static extern IntPtr GetThreadDesktop(uint dwThreadId);

		[DllImport("User32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SetThreadDesktop(IntPtr hDesktop);

		[DllImport("Kernel32.dll")]
		internal static extern uint GetCurrentThreadId();

		[DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		internal static extern IntPtr CreateFile(string lpFileName,
			[MarshalAs(UnmanagedType.U4)] EFileAccess dwDesiredAccess,
			[MarshalAs(UnmanagedType.U4)] EFileShare dwShareMode,
			IntPtr lpSecurityAttributes,
			[MarshalAs(UnmanagedType.U4)] ECreationDisposition dwCreationDisposition,
			[MarshalAs(UnmanagedType.U4)] uint dwFlagsAndAttributes,
			IntPtr hTemplateFile);

		[DllImport("Kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool CloseHandle(IntPtr hObject);

		[DllImport("Kernel32.dll", ExactSpelling = true, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode,
			IntPtr lpInBuffer, uint nInBufferSize, IntPtr lpOutBuffer, uint nOutBufferSize,
			out uint lpBytesReturned, IntPtr lpOverlapped);

		// [DllImport("DwmApi.dll")]
		// internal static extern Int32 DwmExtendFrameIntoClientArea(IntPtr hWnd,
		//	ref MARGINS pMarInset);

		// [DllImport("DwmApi.dll")]
		// internal static extern Int32 DwmIsCompositionEnabled(ref Int32 pfEnabled);

		[DllImport("ComCtl32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
		internal static extern Int32 TaskDialogIndirect([In] ref VtdConfig pTaskConfig,
			[Out] out int pnButton, [Out] out int pnRadioButton,
			[Out] out bool pfVerificationFlagChecked);
	}
}
