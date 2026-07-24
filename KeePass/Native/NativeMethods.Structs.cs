/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2026 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

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

		[StructLayout(LayoutKind.Explicit)]
		internal struct INPUT32
		{
			[FieldOffset(0)]
			public uint Type;
			[FieldOffset(0)]
			public MOUSEINPUT32_WithSkip MouseInput;
			[FieldOffset(0)]
			public KEYBDINPUT32_WithSkip KeyboardInput;
		}

		// INPUT.KI (40). vk: 8, sc: 10, fl: 12, t: 16, ex: 24
		[StructLayout(LayoutKind.Explicit, Size = 40)]
		internal struct SpecializedKeyboardINPUT64
		{
			[FieldOffset(0)]
			public uint Type;
			[FieldOffset(8)]
			public ushort VirtualKeyCode;
			[FieldOffset(10)]
			public ushort ScanCode;
			[FieldOffset(12)]
			public uint Flags;
			[FieldOffset(16)]
			public uint Time;
			[FieldOffset(24)]
			public IntPtr ExtraInfo;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		internal struct CHARFORMAT2
		{
			public uint cbSize;
			public uint dwMask;
			public uint dwEffects;
			public int yHeight;
			public int yOffset;
			public uint crTextColor;
			public byte bCharSet;
			public byte bPitchAndFamily;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string szFaceName;

			public ushort wWeight;
			public ushort sSpacing;
			public int crBackColor;
			public int lcid;
			public uint dwReserved;
			public short sStyle;
			public short wKerning;
			public byte bUnderlineType;
			public byte bAnimation;
			public byte bRevAuthor;
			public byte bReserved1;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct WINDOWPOS
		{
			public IntPtr hwnd;
			public IntPtr hwndInsertAfter;
			public int x;
			public int y;
			public int cx;
			public int cy;
			public uint flags;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct POINT
		{
			public int x;
			public int y;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;

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
			public int cbSize;
			public RECT rcItem;
			public RECT rcButton;

			[MarshalAs(UnmanagedType.U4)]
			public ComboBoxButtonState buttonState;

			public IntPtr hwndCombo;
			public IntPtr hwndEdit;
			public IntPtr hwndList;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct MARGINS
		{
			public int Left;
			public int Right;
			public int Top;
			public int Bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct COPYDATASTRUCT
		{
			public IntPtr dwData;
			public int cbData;
			public IntPtr lpData;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct SCROLLINFO
		{
			public uint cbSize;
			public uint fMask;
			public int nMin;
			public int nMax;
			public uint nPage;
			public int nPos;
			public int nTrackPos;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct HDITEM
		{
			public uint mask;
			public int cxy;

			[MarshalAs(UnmanagedType.LPTStr)]
			public string pszText;

			public IntPtr hbm;
			public int cchTextMax;
			public int fmt;
			public IntPtr lParam;
			public int iImage;
			public int iOrder;
			public uint type;
			public IntPtr pvFilter;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct NMHDR
		{
			public IntPtr hwndFrom;
			public IntPtr idFrom;
			public uint code;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct NMLVEMPTYMARKUP
		{
			public NMHDR hdr;
			public uint dwFlags;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = L_MAX_URL_LENGTH)]
			public string szMarkup;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct LASTINPUTINFO
		{
			public uint cbSize;
			public uint dwTime;
		}

		/* [StructLayout(LayoutKind.Sequential)]
		internal struct SHCHANGENOTIFYENTRY
		{
			public IntPtr pidl;
			[MarshalAs(UnmanagedType.Bool)]
			public bool fRecursive;
		} */

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		private struct SHFILEINFO
		{
			public IntPtr hIcon;
			public int iIcon;
			public uint dwAttributes;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst =
				KeePassLib.Native.NativeMethods.MAX_PATH)]
			public string szDisplayName;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
			public string szTypeName;
		}

		/* // LVGROUP for Windows Vista and higher
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private struct LVGROUP
		{
			public uint cbSize;
			public uint mask;

			// [MarshalAs(UnmanagedType.LPWStr)]
			// public StringBuilder pszHeader;
			public IntPtr pszHeader;
			public int cchHeader;

			// [MarshalAs(UnmanagedType.LPWStr)]
			// public StringBuilder pszFooter;
			public IntPtr pszFooter;
			public int cchFooter;

			public int iGroupId;
			public uint stateMask;
			public uint state;
			public uint uAlign;

			// [MarshalAs(UnmanagedType.LPWStr)]
			// public StringBuilder pszSubtitle;
			public IntPtr pszSubtitle;
			public uint cchSubtitle;

			[MarshalAs(UnmanagedType.LPWStr)]
			public string pszTask;
			// public StringBuilder pszTask;
			// public IntPtr pszTask;
			public uint cchTask;

			// [MarshalAs(UnmanagedType.LPWStr)]
			// public StringBuilder pszDescriptionTop;
			public IntPtr pszDescriptionTop;
			public uint cchDescriptionTop;

			// [MarshalAs(UnmanagedType.LPWStr)]
			// public StringBuilder pszDescriptionBottom;
			public IntPtr pszDescriptionBottom;
			public uint cchDescriptionBottom;

			public int iTitleImage;
			public int iExtendedImage;
			public int iFirstItem;
			public uint cItems;

			// [MarshalAs(UnmanagedType.LPWStr)]
			// public StringBuilder pszSubsetTitle;
			public IntPtr pszSubsetTitle;
			public uint cchSubsetTitle;
		} */

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		internal struct PROCESSENTRY32
		{
			public uint dwSize;
			public uint cntUsage;
			public uint th32ProcessID;
			public UIntPtr th32DefaultHeapID;
			public uint th32ModuleID;
			public uint cntThreads;
			public uint th32ParentProcessID;
			public int pcPriClassBase;
			public uint dwFlags;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst =
				KeePassLib.Native.NativeMethods.MAX_PATH)]
			public string szExeFile;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		internal struct ACTCTX
		{
			public uint cbSize;
			public uint dwFlags;
			[MarshalAs(UnmanagedType.LPTStr)] // Not LPWStr, see header file
			public string lpSource;
			public ushort wProcessorArchitecture;
			public ushort wLangId;
			[MarshalAs(UnmanagedType.LPTStr)]
			public string lpAssemblyDirectory;
			[MarshalAs(UnmanagedType.LPTStr)]
			public string lpResourceName;
			[MarshalAs(UnmanagedType.LPTStr)]
			public string lpApplicationName;
			public IntPtr hModule;
		}

		// https://msdn.microsoft.com/en-us/library/ms997538.aspx
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		internal struct ICONDIR
		{
			public ushort idReserved;
			public ushort idType;
			public ushort idCount;
		}

		// https://msdn.microsoft.com/en-us/library/ms997538.aspx
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		internal struct ICONDIRENTRY
		{
			public byte bWidth;
			public byte bHeight;
			public byte bColorCount;
			public byte bReserved;
			public ushort wPlanes;
			public ushort wBitCount;
			public uint dwBytesInRes;
			public uint dwImageOffset;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		internal struct BITMAPINFOHEADER
		{
			public uint biSize;
			public int biWidth;
			public int biHeight;
			public ushort biPlanes;
			public ushort biBitCount;
			public uint biCompression;
			public uint biSizeImage;
			public int biXPelsPerMeter;
			public int biYPelsPerMeter;
			public uint biClrUsed;
			public uint biClrImportant;
		}
	}
}
