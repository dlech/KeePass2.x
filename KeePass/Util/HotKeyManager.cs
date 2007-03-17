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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;

namespace KeePass.Util
{
	public static class HotKeyManager
	{
		private static IntPtr m_hRecvWnd = IntPtr.Zero;
		private static List<int> m_vRegisteredIDs = new List<int>();

		public const int WM_HOTKEY = 0x0312;

		private const uint MOD_ALT = 1;
		private const uint MOD_CONTROL = 2;
		private const uint MOD_SHIFT = 4;
		private const uint MOD_WIN = 8;

		public static IntPtr ReceiverWindow
		{
			get { return m_hRecvWnd; }
			set { m_hRecvWnd = value; }
		}

		[DllImport("User32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers,
			uint vk);

		public static bool RegisterHotKey(int nID, Keys kKey, Keys kModifiers)
		{
			if(kKey == Keys.None) return false;

			uint uMod = 0;
			if((kModifiers & Keys.Shift) != Keys.None) uMod |= MOD_SHIFT;
			if((kModifiers & Keys.Menu) != Keys.None) uMod |= MOD_ALT;
			if((kModifiers & Keys.Alt) != Keys.None) uMod |= MOD_ALT;
			if((kModifiers & Keys.Control) != Keys.None) uMod |= MOD_CONTROL;

			HotKeyManager.UnregisterHotKey(nID);
			if(HotKeyManager.RegisterHotKey(m_hRecvWnd, nID, uMod, (uint)kKey))
			{
				m_vRegisteredIDs.Add(nID);
				return true;
			}

			return false;
		}

		[DllImport("User32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

		public static bool UnregisterHotKey(int nID)
		{
			if(m_vRegisteredIDs.IndexOf(nID) >= 0)
			{
				m_vRegisteredIDs.Remove(nID);

				bool bResult = HotKeyManager.UnregisterHotKey(m_hRecvWnd, nID);
				Debug.Assert(bResult);
				return bResult;
			}

			return false;
		}

		public static void UnregisterAll()
		{
			foreach(int nID in m_vRegisteredIDs)
			{
				bool bResult = HotKeyManager.UnregisterHotKey(m_hRecvWnd, nID);
				Debug.Assert(bResult);
			}

			m_vRegisteredIDs.Clear();
		}
	}
}
