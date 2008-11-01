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
using System.Drawing;

namespace KeePass.Native
{
	internal static partial class NativeMethods
	{
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		internal struct MOUSEINPUT32_WithSkip
		{
			public uint __Unused0; // See INPUT32 structure

			public int X;
			public int Y;
			public uint MouseData;
			public uint Flags;
			public uint Time;
			public IntPtr ExtraInfo;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		internal struct KEYBDINPUT32_WithSkip
		{
			public uint __Unused0; // See INPUT32 structure

			public ushort VirtualKeyCode;
			public ushort ScanCode;
			public uint Flags;
			public uint Time;
			public IntPtr ExtraInfo;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		internal struct HARDWAREINPUT32_WithSkip
		{
			public uint __Unused0; // See INPUT32 structure

			public uint Message;
			public ushort ParamL;
			public ushort ParamH;
		}

		[StructLayout(LayoutKind.Explicit)]
		internal struct INPUT32
		{
			[FieldOffset(0)]
			public uint Type;
			[FieldOffset(0)]
			public MOUSEINPUT32_WithSkip MouseInput;
			[FieldOffset(0)]
			public KEYBDINPUT32_WithSkip KeyboardInput;
			[FieldOffset(0)]
			public HARDWAREINPUT32_WithSkip HardwareInput;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		internal struct SpecializedKeyboardINPUT64
		{
			public IntPtr Type;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct CHARFORMAT2
		{
			public UInt32 cbSize;
			public UInt32 dwMask;
			public UInt32 dwEffects;
			public Int32 yHeight;
			public Int32 yOffset;
			public Int32 crTextColor;
			public Byte bCharSet;
			public Byte bPitchAndFamily;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
			public char[] szFaceName;

			public UInt16 wWeight;
			public UInt16 sSpacing;
			public Int32 crBackColor;
			public Int32 lcid;
			public UInt32 dwReserved;
			public Int16 sStyle;
			public Int16 wKerning;
			public Byte bUnderlineType;
			public Byte bAnimation;
			public Byte bRevAuthor;
			public Byte bReserved1;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct RECT
		{
			public Int32 Left;
			public Int32 Top;
			public Int32 Right;
			public Int32 Bottom;

			public RECT(Rectangle rect)
			{
				this.Left = rect.Left;
				this.Top = rect.Top;
				this.Right = rect.Right;
				this.Bottom = rect.Bottom;
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct COMBOBOXINFO
		{
			public Int32 cbSize;
			public RECT rcItem;
			public RECT rcButton;
			public ComboBoxButtonState buttonState;
			public IntPtr hwndCombo;
			public IntPtr hwndEdit;
			public IntPtr hwndList;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct MARGINS
		{
			public Int32 Left;
			public Int32 Right;
			public Int32 Top;
			public Int32 Bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct COPYDATASTRUCT
		{
			public IntPtr dwData;
			public Int32 cbData;
			public IntPtr lpData;
		}
	}
}
