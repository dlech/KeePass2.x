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
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace KeePass.Util
{
	public static class SendInputEx
	{
		[StructLayout(LayoutKind.Sequential)]
		private struct MOUSEINPUT
		{
			public int X;
			public int Y;
			public uint MouseData;
			public uint Flags;
			public uint Time;
			public IntPtr ExtraInfo;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct KEYBDINPUT
		{
			public ushort VirtualKeyCode;
			public ushort ScanCode;
			public uint Flags;
			public uint Time;
			public IntPtr ExtraInfo;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct HARDWAREINPUT
		{
			public uint Message;
			public ushort ParamL;
			public ushort ParamH;
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct INPUT
		{
			[FieldOffset(0)]
			public uint Type;
			[FieldOffset(4)]
			MOUSEINPUT MouseInput;
			[FieldOffset(4)]
			public KEYBDINPUT KeyboardInput;
			[FieldOffset(4)]
			HARDWAREINPUT HardwareInput;
		}

		[DllImport("User32.dll", SetLastError = true)]
		private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

		[DllImport("User32.dll")]
		private static extern IntPtr GetMessageExtraInfo();

		[DllImport("User32.dll")]
		private static extern uint MapVirtualKey(uint uCode, uint uMapType);

		[DllImport("User32.dll")]
		private static extern ushort GetKeyState(int vKey);

		[DllImport("User32.dll")]
		private static extern bool BlockInput(bool fBlockIt);

		private const uint INPUT_MOUSE = 0;
		private const uint INPUT_KEYBOARD = 1;
		private const uint INPUT_HARDWARE = 2;

		private const int VK_SHIFT = 0x10;
		private const int VK_CONTROL = 0x11;
		private const int VK_MENU = 0x12;
		private const int VK_CAPITAL = 0x14;
		private const int VK_LSHIFT = 0xA0;
		private const int VK_RSHIFT = 0xA1;
		private const int VK_LCONTROL = 0xA2;
		private const int VK_RCONTROL = 0xA3;
		private const int VK_LMENU = 0xA4;
		private const int VK_RMENU = 0xA5;
		private const int VK_LWIN = 0x5B;
		private const int VK_RWIN = 0x5C;

		private const uint KEYEVENTF_EXTENDEDKEY = 1;
		private const uint KEYEVENTF_KEYUP = 2;

		public static void SendKeysWait(string strKeys)
		{
			List<int> lRestore = InitSendKeys();
			SendKeys.SendWait(strKeys);
			FinishSendKeys(lRestore);
		}

		private static List<int> InitSendKeys()
		{
			try
			{
				BlockInput(true);

				SendKeys.Flush();
				Application.DoEvents();

				List<int> lMod = GetActiveKeyModifiers();
				ActivateKeyModifiers(lMod, false);

				return lMod;
			}
			catch(Exception) { Debug.Assert(false); }

			return new List<int>();
		}

		private static void FinishSendKeys(List<int> lRestore)
		{
			try
			{
				ActivateKeyModifiers(lRestore, true);

				BlockInput(false);

				SendKeys.Flush();
				Application.DoEvents();
			}
			catch(Exception) { Debug.Assert(false); }
		}

		private static bool SendModifierVKey(int vKey, bool bDown)
		{
			Debug.Assert((Marshal.SizeOf(typeof(INPUT)) == 28) ||
				(Marshal.SizeOf(typeof(INPUT)) == 32));

			if(bDown || IsKeyModifierActive(vKey))
			{
				INPUT[] pInput = new INPUT[1];

				pInput[0].Type = INPUT_KEYBOARD;
				pInput[0].KeyboardInput.VirtualKeyCode = (ushort)vKey;
				pInput[0].KeyboardInput.ScanCode =
					(ushort)(MapVirtualKey((uint)vKey, 0) & 0xFF);
				pInput[0].KeyboardInput.Flags = (bDown ? 0 : KEYEVENTF_KEYUP) |
					((((vKey >= 0x21) && (vKey <= 0x2E)) ||
					((vKey >= 0x6A) && (vKey <= 0x6F))) ? KEYEVENTF_EXTENDEDKEY : 0);
				pInput[0].KeyboardInput.Time = 0;
				pInput[0].KeyboardInput.ExtraInfo = GetMessageExtraInfo();

				if(SendInput(1, pInput, Marshal.SizeOf(typeof(INPUT))) != 1)
				{
					Debug.Assert(false);
					return false;
				}

				return true;
			}

			return false;
		}

		public static List<int> GetActiveKeyModifiers()
		{
			List<int> lSet = new List<int>();

			AddKeyModifierIfSet(lSet, VK_LSHIFT);
			AddKeyModifierIfSet(lSet, VK_RSHIFT);
			AddKeyModifierIfSet(lSet, VK_SHIFT);

			AddKeyModifierIfSet(lSet, VK_LCONTROL);
			AddKeyModifierIfSet(lSet, VK_RCONTROL);
			AddKeyModifierIfSet(lSet, VK_CONTROL);

			AddKeyModifierIfSet(lSet, VK_LMENU);
			AddKeyModifierIfSet(lSet, VK_RMENU);
			AddKeyModifierIfSet(lSet, VK_MENU);

			AddKeyModifierIfSet(lSet, VK_LWIN);
			AddKeyModifierIfSet(lSet, VK_RWIN);

			AddKeyModifierIfSet(lSet, VK_CAPITAL);

			return lSet;
		}

		private static void AddKeyModifierIfSet(List<int> lList, int vKey)
		{
			if(IsKeyModifierActive(vKey))
				lList.Add(vKey);
		}

		private static bool IsKeyModifierActive(int vKey)
		{
			ushort usState = GetKeyState(vKey);

			if(vKey == VK_CAPITAL)
				return ((usState & 1) != 0);
			else
				return ((usState & 0x8000) != 0);
		}

		public static void ActivateKeyModifiers(List<int> vKeys, bool bDown)
		{
			Debug.Assert(vKeys != null);
			if(vKeys == null) throw new ArgumentNullException("vKeys");

			foreach(int vKey in vKeys)
			{
				SendModifierVKey(vKey, bDown);
			}
		}
	}
}
