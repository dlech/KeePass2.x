/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2024 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Runtime.InteropServices;
using System.Text;

#if !KeePassUAP
using System.Windows.Forms;
#endif

namespace KeePassLib.Native
{
	internal static partial class NativeMethods
	{
#if (!KeePassLibSD && !KeePassUAP)
		[StructLayout(LayoutKind.Sequential)]
		private struct XClassHint
		{
			public IntPtr res_name;
			public IntPtr res_class;
		}

		[DllImport("libX11")]
		private static extern int XSetClassHint(IntPtr display, IntPtr window, IntPtr class_hints);

		private static Type m_tXplatUIX11 = null;
		private static Type GetXplatUIX11Type(bool bThrowOnError)
		{
			if(m_tXplatUIX11 == null)
			{
				// CheckState is in System.Windows.Forms
				string strTypeCS = typeof(CheckState).AssemblyQualifiedName;
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
				// CheckState is in System.Windows.Forms
				string strTypeCS = typeof(CheckState).AssemblyQualifiedName;
				string strTypeHwnd = strTypeCS.Replace("CheckState", "Hwnd");
				m_tHwnd = Type.GetType(strTypeHwnd, bThrowOnError, true);
			}

			return m_tHwnd;
		}

		internal static void SetWmClass(Form f, string strName, string strClass)
		{
			if(f == null) { Debug.Assert(false); return; }

			// The following crashes under MacOS (SIGSEGV in native code,
			// not just an exception), thus skip it when we are on MacOS;
			// https://sourceforge.net/projects/keepass/forums/forum/329221/topic/5860588
			if(NativeLib.GetPlatformID() == PlatformID.MacOSX) return;

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
#endif

		// =============================================================
		// LibGCrypt 1.8.1-1.9.4+

		private const string LibGCrypt = "libgcrypt.so.20";

		internal const int GCRY_CIPHER_AES256 = 9;
		internal const int GCRY_CIPHER_MODE_CBC = 3;

		[DllImport(LibGCrypt)]
		internal static extern IntPtr gcry_check_version(IntPtr lpReqVersion);

		[DllImport(LibGCrypt)]
		internal static extern uint gcry_cipher_open(ref IntPtr ph, int nAlgo,
			int nMode, uint uFlags);

		[DllImport(LibGCrypt)]
		internal static extern void gcry_cipher_close(IntPtr h);

		[DllImport(LibGCrypt)]
		internal static extern uint gcry_cipher_setkey(IntPtr h, IntPtr pbKey,
			IntPtr cbKey); // cbKey is size_t

		[DllImport(LibGCrypt)]
		internal static extern uint gcry_cipher_setiv(IntPtr h, IntPtr pbIV,
			IntPtr cbIV); // cbIV is size_t

		[DllImport(LibGCrypt)]
		internal static extern uint gcry_cipher_encrypt(IntPtr h, IntPtr pbOut,
			IntPtr cbOut, IntPtr pbIn, IntPtr cbIn); // cb* are size_t

		// =============================================================
		// LibArgon2 20190702+

		// Debian 11
		[DllImport("libargon2.so.0", EntryPoint = "argon2_hash")]
		internal static extern int argon2_hash_u0(uint t_cost, uint m_cost,
			uint parallelism, IntPtr pwd, IntPtr pwdlen, IntPtr salt,
			IntPtr saltlen, IntPtr hash, IntPtr hashlen, IntPtr encoded,
			IntPtr encodedlen, int type, uint version);
		// Fedora 34
		[DllImport("libargon2.so.1", EntryPoint = "argon2_hash")]
		internal static extern int argon2_hash_u1(uint t_cost, uint m_cost,
			uint parallelism, IntPtr pwd, IntPtr pwdlen, IntPtr salt,
			IntPtr saltlen, IntPtr hash, IntPtr hashlen, IntPtr encoded,
			IntPtr encodedlen, int type, uint version);
		// Cf. argon2_hash_w32 and argon2_hash_w64

		/* internal static IntPtr Utf8ZFromString(string str)
		{
			byte[] pb = StrUtil.Utf8.GetBytes(str ?? string.Empty);

			IntPtr p = Marshal.AllocCoTaskMem(pb.Length + 1);
			if(p != IntPtr.Zero)
			{
				Marshal.Copy(pb, 0, p, pb.Length);
				Marshal.WriteByte(p, pb.Length, 0);
			}
			else { Debug.Assert(false); }

			return p;
		}

		internal static string Utf8ZToString(IntPtr p)
		{
			if(p == IntPtr.Zero) { Debug.Assert(false); return null; }

			List<byte> l = new List<byte>();
			for(int i = 0; i < int.MaxValue; ++i)
			{
				byte bt = Marshal.ReadByte(p, i);
				if(bt == 0) break;

				l.Add(bt);
			}

			return StrUtil.Utf8.GetString(l.ToArray());
		}

		internal static void Utf8ZFree(IntPtr p)
		{
			if(p != IntPtr.Zero) Marshal.FreeCoTaskMem(p);
		} */

		/* // =============================================================
		// LibGLib 2

		private const string LibGLib = "libglib-2.0.so.0";

		internal const int G_FALSE = 0;

		// https://developer.gnome.org/glib/stable/glib-Memory-Allocation.html
		[DllImport(LibGLib)]
		internal static extern void g_free(IntPtr pMem); // pMem may be null

		// =============================================================
		// LibGTK 3 (3.22.11 / 3.22.24)

		private const string LibGtk = "libgtk-3.so.0";

		internal static readonly IntPtr GDK_SELECTION_PRIMARY = new IntPtr(1);
		internal static readonly IntPtr GDK_SELECTION_CLIPBOARD = new IntPtr(69);

		[DllImport(LibGtk)]
		internal static extern int gtk_init_check(IntPtr pArgc, IntPtr pArgv);

		[DllImport(LibGtk)]
		// The returned handle is owned by GTK and must not be freed
		internal static extern IntPtr gtk_clipboard_get(IntPtr pSelection);

		[DllImport(LibGtk)]
		internal static extern void gtk_clipboard_clear(IntPtr hClipboard);

		[DllImport(LibGtk)]
		internal static extern IntPtr gtk_clipboard_wait_for_text(IntPtr hClipboard);

		[DllImport(LibGtk)]
		internal static extern void gtk_clipboard_set_text(IntPtr hClipboard,
			IntPtr lpText, int cbLen);

		[DllImport(LibGtk)]
		internal static extern void gtk_clipboard_store(IntPtr hClipboard); */
	}
}
