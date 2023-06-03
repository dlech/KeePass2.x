/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2023 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace KeePassLib.Native
{
	internal static class ClipboardU
	{
		internal const string XSel = "xsel";
		private const string XSelV = "--version";
		private const string XSelR = "--output --clipboard";
		private const string XSelC = "--clear --clipboard";
		private const string XSelW = "--input --clipboard";
		private const string XSelND = " --nodetach";
		private const AppRunFlags XSelWF = AppRunFlags.WaitForExit;

		private static bool? g_obXSel = null;

		public static string GetText()
		{
			// System.Windows.Forms.Clipboard doesn't work properly,
			// see Mono workarounds 1530/1613

			// string str = GtkGetText();
			// if(str != null) return str;

			return XSelGetText();
		}

		public static bool SetText(string strText, bool bMayBlock)
		{
			string str = (strText ?? string.Empty);

			// System.Windows.Forms.Clipboard doesn't work properly,
			// see Mono workarounds 1530/1613

			// if(GtkSetText(str)) return true;

			return XSelSetText(str, bMayBlock);
		}

		// =============================================================
		// LibGTK

		// Even though GTK+ 3 appears to be loaded already, performing a
		// P/Invoke of LibGTK's gtk_init_check function terminates the
		// process (!) with the following error message:
		// "Gtk-ERROR **: GTK+ 2.x symbols detected. Using GTK+ 2.x and
		// GTK+ 3 in the same process is not supported".

		/* private static bool GtkInit()
		{
			try
			{
				// GTK requires GLib;
				// the following throws if and only if GLib is unavailable
				NativeMethods.g_free(IntPtr.Zero);

				if(NativeMethods.gtk_init_check(IntPtr.Zero, IntPtr.Zero) !=
					NativeMethods.G_FALSE)
					return true;

				Debug.Assert(false);
			}
			catch(Exception) { Debug.Assert(false); }

			return false;
		}

		private static string GtkGetText()
		{
			IntPtr lpText = IntPtr.Zero;
			try
			{
				if(GtkInit())
				{
					IntPtr h = NativeMethods.gtk_clipboard_get(
						NativeMethods.GDK_SELECTION_CLIPBOARD);
					if(h != IntPtr.Zero)
					{
						lpText = NativeMethods.gtk_clipboard_wait_for_text(h);
						if(lpText != IntPtr.Zero)
							return NativeMethods.Utf8ZToString(lpText);
					}
				}
			}
			catch(Exception) { Debug.Assert(false); }
			finally
			{
				try { NativeMethods.g_free(lpText); }
				catch(Exception) { Debug.Assert(false); }
			}

			return null;
		}

		private static bool GtkSetText(string str)
		{
			IntPtr lpText = IntPtr.Zero;
			try
			{
				if(GtkInit())
				{
					lpText = NativeMethods.Utf8ZFromString(str ?? string.Empty);
					if(lpText == IntPtr.Zero) { Debug.Assert(false); return false; }

					bool b = false;
					for(int i = 0; i < 2; ++i)
					{
						IntPtr h = NativeMethods.gtk_clipboard_get((i == 0) ?
							NativeMethods.GDK_SELECTION_PRIMARY :
							NativeMethods.GDK_SELECTION_CLIPBOARD);
						if(h != IntPtr.Zero)
						{
							NativeMethods.gtk_clipboard_clear(h);
							NativeMethods.gtk_clipboard_set_text(h, lpText, -1);
							NativeMethods.gtk_clipboard_store(h);
							b = true;
						}
					}

					return b;
				}
			}
			catch(Exception) { Debug.Assert(false); }
			finally { NativeMethods.Utf8ZFree(lpText); }

			return false;
		} */

		// =============================================================
		// XSel

		private static bool XSelInit()
		{
			if(g_obXSel.HasValue) return g_obXSel.Value;

			string strTest = NativeLib.RunConsoleApp(XSel, XSelV);

			bool b = (strTest != null);
			g_obXSel = b;
			return b;
		}

		private static string XSelGetText()
		{
			if(!XSelInit()) return null;

			return NativeLib.RunConsoleApp(XSel, XSelR);
		}

		private static bool XSelSetText(string str, bool bMayBlock)
		{
			if(!XSelInit()) return false;

			string strOpt = (bMayBlock ? XSelND : string.Empty);

			// xsel with an empty input can hang, thus use --clear
			if(str.Length == 0)
				return (NativeLib.RunConsoleApp(XSel, XSelC + strOpt,
					null, XSelWF) != null);

			// Use --nodetach to prevent clipboard corruption;
			// https://sourceforge.net/p/keepass/bugs/1603/
			return (NativeLib.RunConsoleApp(XSel, XSelW + strOpt,
				str, XSelWF) != null);
		}
	}
}
