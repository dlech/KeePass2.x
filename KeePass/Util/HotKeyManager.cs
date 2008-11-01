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
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;

using KeePass.Native;

namespace KeePass.Util
{
	public static class HotKeyManager
	{
		private static IntPtr m_hRecvWnd = IntPtr.Zero;
		private static List<int> m_vRegisteredIDs = new List<int>();

		private const uint MOD_ALT = 1;
		private const uint MOD_CONTROL = 2;
		private const uint MOD_SHIFT = 4;
		private const uint MOD_WIN = 8;

		public static IntPtr ReceiverWindow
		{
			get { return m_hRecvWnd; }
			set { m_hRecvWnd = value; }
		}

		public static bool RegisterHotKey(int nId, Keys kKey)
		{
			if(kKey == Keys.None) return false;

			uint uMod = 0;
			if((kKey & Keys.Shift) != Keys.None) uMod |= MOD_SHIFT;
			if((kKey & Keys.Alt) != Keys.None) uMod |= MOD_ALT;
			if((kKey & Keys.Control) != Keys.None) uMod |= MOD_CONTROL;

			uint vkCode = (uint)(kKey & Keys.KeyCode);

			UnregisterHotKey(nId);

			try
			{
				if(NativeMethods.RegisterHotKey(m_hRecvWnd, nId, uMod, vkCode))
				{
					m_vRegisteredIDs.Add(nId);
					return true;
				}
			}
			catch(Exception) { Debug.Assert(false); }

			return false;
		}

		public static bool UnregisterHotKey(int nId)
		{
			if(m_vRegisteredIDs.IndexOf(nId) >= 0)
			{
				m_vRegisteredIDs.Remove(nId);

				try
				{
					bool bResult = NativeMethods.UnregisterHotKey(m_hRecvWnd, nId);
					Debug.Assert(bResult);
					return bResult;
				}
				catch(Exception) { Debug.Assert(false); }
			}

			return false;
		}

		public static void UnregisterAll()
		{
			try
			{
				foreach(int nID in m_vRegisteredIDs)
				{
#if DEBUG
					bool bResult = NativeMethods.UnregisterHotKey(m_hRecvWnd, nID);
					Debug.Assert(bResult);
#else
					NativeMethods.UnregisterHotKey(m_hRecvWnd, nID);
#endif
				}
			}
			catch(Exception) { Debug.Assert(false); }

			m_vRegisteredIDs.Clear();
		}
	}
}
