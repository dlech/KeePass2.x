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

namespace KeePassLib.Native
{
	internal static class NativeMethods
	{
		[DllImport("KeePassNtv32.dll", EntryPoint = "TransformKey")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool TransformKey32(IntPtr pBuf256,
			IntPtr pKey256, UInt64 uRounds);

		[DllImport("KeePassNtv64.dll", EntryPoint = "TransformKey")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool TransformKey64(IntPtr pBuf256,
			IntPtr pKey256, UInt64 uRounds);

		internal static bool TransformKey(IntPtr pBuf256, IntPtr pKey256,
			UInt64 uRounds)
		{
			if(Marshal.SizeOf(typeof(IntPtr)) == 8)
				return TransformKey64(pBuf256, pKey256, uRounds);
			else
				return TransformKey32(pBuf256, pKey256, uRounds);
		}

		[DllImport("KeePassNtv32.dll", EntryPoint = "TransformKeyTimed")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool TransformKeyTimed32(IntPtr pBuf256,
			IntPtr pKey256, ref UInt64 puRounds, UInt32 uSeconds);

		[DllImport("KeePassNtv64.dll", EntryPoint = "TransformKeyTimed")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool TransformKeyTimed64(IntPtr pBuf256,
			IntPtr pKey256, ref UInt64 puRounds, UInt32 uSeconds);

		internal static bool TransformKeyTimed(IntPtr pBuf256, IntPtr pKey256,
			ref UInt64 puRounds, UInt32 uSeconds)
		{
			if(Marshal.SizeOf(typeof(IntPtr)) == 8)
				return TransformKeyTimed64(pBuf256, pKey256, ref puRounds, uSeconds);
			else
				return TransformKeyTimed32(pBuf256, pKey256, ref puRounds, uSeconds);
		}
	}
}
