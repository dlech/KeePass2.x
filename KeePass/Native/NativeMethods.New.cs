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
		internal static bool EnsureForegroundWindow(IntPtr hWnd)
		{
			if(IsWindow(hWnd) == false)
				return false;

			if(SetForegroundWindow(hWnd) == false)
			{
				Debug.Assert(false);
				return false;
			}

			int nStartMS = Environment.TickCount;
			while((Environment.TickCount - nStartMS) < 1000)
			{
				IntPtr h = GetForegroundWindow();
				if(h == hWnd) return true;

				Application.DoEvents();
			}

			return false;
		}

		internal static bool LoseFocus(IntPtr hCurrentWnd)
		{
			IntPtr hWnd = GetWindow(hCurrentWnd, GW_HWNDNEXT);

			while(true)
			{
				if(hWnd != hCurrentWnd)
				{
					uint uStyle = GetWindowLong(hWnd, GWL_STYLE);

					if(((uStyle & WS_VISIBLE) != 0) &&
						(GetWindowTextLength(hWnd) > 0))
					{
						break;
					}
				}

				hWnd = GetWindow(hWnd, GW_HWNDNEXT);
				if(hWnd == IntPtr.Zero) break;
			}

			if(hWnd == IntPtr.Zero) return false;

			return EnsureForegroundWindow(hWnd);
		}
	}
}
