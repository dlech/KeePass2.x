/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2010 Dominik Reichl <dominik.reichl@t-online.de>

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

namespace KeePass.Native
{
	internal static partial class NativeMethods
	{
		/* private const string PathLibDo = "/usr/lib/gnome-do/libdo";
		private const UnmanagedType NtvStringType = UnmanagedType.LPStr;

		[DllImport(PathLibDo)]
		internal static extern void gnomedo_keybinder_init();

		[DllImport(PathLibDo)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool gnomedo_keybinder_bind(
			[MarshalAs(NtvStringType)] string strKey,
			BindKeyHandler lpfnHandler);

		[DllImport(PathLibDo)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool gnomedo_keybinder_unbind(
			[MarshalAs(NtvStringType)] string strKey,
			BindKeyHandler lpfnHandler);

		internal delegate void BindKeyHandler([MarshalAs(NtvStringType)]
			string strKey, IntPtr lpUserData); */

		/* private const string PathLibTomBoy = "/usr/lib/tomboy/libtomboy.so";

		[DllImport(PathLibTomBoy)]
		internal static extern void tomboy_keybinder_init();

		[DllImport(PathLibTomBoy)]
		internal static extern void tomboy_keybinder_bind(string strKey,
			BindKeyHandler lpHandler);

		[DllImport(PathLibTomBoy)]
		internal static extern void tomboy_keybinder_unbind(string strKey,
			BindKeyHandler lpHandler);

		internal delegate void BindKeyHandler(string strKey, IntPtr lpUserData); */

		private static bool LoseFocusUnix(Form fCurrent)
		{
			if(fCurrent == null) { Debug.Assert(false); return true; }

			try
			{
				string strCurrent = RunXDoTool("getwindowfocus -f");
				long lCurrent;
				long.TryParse(strCurrent.Trim(), out lCurrent);

				fCurrent.WindowState = FormWindowState.Minimized;

				int nStart = Environment.TickCount;
				while((Environment.TickCount - nStart) < 1000)
				{
					Application.DoEvents();

					string strActive = RunXDoTool("getwindowfocus -f");
					long lActive;
					long.TryParse(strActive.Trim(), out lActive);

					if(lActive != lCurrent) break;
				}

				return true;
			}
			catch(Exception) { Debug.Assert(false); }

			return false;
		}

		internal static bool TryXDoTool()
		{
			return !string.IsNullOrEmpty(RunXDoTool("help"));
		}

		internal static bool TryXDoTool(bool bRequireWindowNameSupport)
		{
			if(!bRequireWindowNameSupport) return TryXDoTool();

			string str = RunXDoTool("getactivewindow getwindowname");
			if(string.IsNullOrEmpty(str)) return false;

			return (str.Trim() != "usage: getactivewindow");
		}

		internal static string RunXDoTool(string strParams)
		{
			try
			{
				ProcessStartInfo psi = new ProcessStartInfo();

				psi.CreateNoWindow = true;
				psi.FileName = "xdotool";
				psi.WindowStyle = ProcessWindowStyle.Hidden;
				psi.UseShellExecute = false;
				psi.RedirectStandardOutput = true;

				if(!string.IsNullOrEmpty(strParams)) psi.Arguments = strParams;

				Process p = Process.Start(psi);

				string strOutput = p.StandardOutput.ReadToEnd();
				p.WaitForExit();

				return strOutput;
			}
			catch(Exception) { Debug.Assert(false); }

			return string.Empty;
		}
	}
}
