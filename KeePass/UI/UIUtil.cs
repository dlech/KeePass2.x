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

using KeePass.Util;

namespace KeePass.UI
{
	public static class UIUtil
	{
		[StructLayout(LayoutKind.Sequential)]
		private struct CHARFORMAT2
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

		private const uint WM_USER = 0x0400;
		private const uint EM_SETCHARFORMAT = WM_USER + 68;

		private const Int32 SCF_SELECTION = 0x0001;

		private const UInt32 CFM_LINK = 0x00000020;
		private const UInt32 CFE_LINK = 0x00000020;

		public static void RtfSetSelectionLink(RichTextBox rtb)
		{
			try
			{
				CHARFORMAT2 cf = new CHARFORMAT2();
				cf.cbSize = (UInt32)Marshal.SizeOf(cf);

				cf.dwMask = CFM_LINK;
				cf.dwEffects = CFE_LINK;

				IntPtr wParam = (IntPtr)SCF_SELECTION;
				IntPtr lParam = Marshal.AllocCoTaskMem(Marshal.SizeOf(cf));
				Marshal.StructureToPtr(cf, lParam, false);

				WinUtil.SendMessage(rtb.Handle, EM_SETCHARFORMAT, wParam, lParam);

				Marshal.FreeCoTaskMem(lParam);
			}
			catch(Exception) { Debug.Assert(false); }
		}
	}
}
