/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2012 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Reflection;
using System.Diagnostics;

using KeePass.UI;
using KeePass.Util;

using KeePassLib.Utility;

using KeeNativeLib = KeePassLib.Native;

namespace KeePass.Native
{
	internal static partial class NativeMethods
	{
		[StructLayout(LayoutKind.Sequential)]
		private struct XClassHint
		{
			public IntPtr res_name;
			public IntPtr res_class;
		}

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

		[DllImport("libX11")]
		private static extern int XSetClassHint(IntPtr display, IntPtr window, IntPtr class_hints);

		private static bool LoseFocusUnix(Form fCurrent)
		{
			if(fCurrent == null) { Debug.Assert(false); return true; }

			try
			{
				string strCurrent = RunXDoTool("getwindowfocus -f");
				long lCurrent;
				long.TryParse(strCurrent.Trim(), out lCurrent);

				UIUtil.SetWindowState(fCurrent, FormWindowState.Minimized);

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

			return !(str.Trim().Equals("usage: getactivewindow", StrUtil.CaseIgnoreCmp));
		}

		internal static string RunXDoTool(string strParams)
		{
			try
			{
				Application.DoEvents(); // E.g. for clipboard updates
				string strOutput = KeeNativeLib.NativeLib.RunConsoleApp(
					"xdotool", strParams);
				Application.DoEvents(); // E.g. for clipboard updates
				return (strOutput ?? string.Empty);
			}
			catch(Exception) { Debug.Assert(false); }

			return string.Empty;
		}

		private static Type m_tXplatUIX11 = null;
		private static Type GetXplatUIX11Type(bool bThrowOnError)
		{
			if(m_tXplatUIX11 == null)
			{
				CheckState cs = CheckState.Indeterminate; // In System.Windows.Forms
				string strTypeCS = cs.GetType().AssemblyQualifiedName;
				string strTypeX11 = strTypeCS.Replace("CheckState", "XplatUIX11");
				m_tXplatUIX11 = Type.GetType(strTypeX11, bThrowOnError, true);
			}

			return m_tXplatUIX11;
		}

		private static Type m_tHwnd = null;
		private static Type GetHwndType(bool bThrowOnError)
		{
			if(m_tHwnd == null)
			{
				CheckState cs = CheckState.Indeterminate; // In System.Windows.Forms
				string strTypeCS = cs.GetType().AssemblyQualifiedName;
				string strTypeHwnd = strTypeCS.Replace("CheckState", "Hwnd");
				m_tHwnd = Type.GetType(strTypeHwnd, bThrowOnError, true);
			}

			return m_tHwnd;
		}

		internal static void SetWmClass(Form f, string strName, string strClass)
		{
			if(f == null) { Debug.Assert(false); return; }

			try
			{
				Type tXplatUIX11 = GetXplatUIX11Type(true);
				FieldInfo fiDisplayHandle = tXplatUIX11.GetField("DisplayHandle",
					BindingFlags.NonPublic | BindingFlags.Static);
				IntPtr hDisplay = (IntPtr)fiDisplayHandle.GetValue(null);

				Type tHwnd = GetHwndType(true);
				MethodInfo miObjectFromHandle = tHwnd.GetMethod("ObjectFromHandle",
					BindingFlags.Public | BindingFlags.Static);
				object oHwnd = miObjectFromHandle.Invoke(null, new object[] { f.Handle });

				FieldInfo fiWholeWindow = tHwnd.GetField("whole_window",
					BindingFlags.NonPublic | BindingFlags.Instance);
				IntPtr hWindow = (IntPtr)fiWholeWindow.GetValue(oHwnd);

				XClassHint xch = new XClassHint();
				xch.res_name = Marshal.StringToCoTaskMemAnsi(strName ?? string.Empty);
				xch.res_class = Marshal.StringToCoTaskMemAnsi(strClass ?? string.Empty);
				IntPtr pXch = Marshal.AllocCoTaskMem(Marshal.SizeOf(xch));
				Marshal.StructureToPtr(xch, pXch, false);

				XSetClassHint(hDisplay, hWindow, pXch);

				Marshal.FreeCoTaskMem(pXch);
				Marshal.FreeCoTaskMem(xch.res_name);
				Marshal.FreeCoTaskMem(xch.res_class);
			}
			catch(Exception) { Debug.Assert(false); }
		}
	}
}
