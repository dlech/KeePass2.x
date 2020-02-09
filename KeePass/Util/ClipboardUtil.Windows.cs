/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2020 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using KeePass.Native;
using KeePass.UI;

using KeePassLib;
using KeePassLib.Delegates;
using KeePassLib.Utility;

namespace KeePass.Util
{
	public static partial class ClipboardUtil
	{
		// https://referencesource.microsoft.com/#system.windows.forms/winforms/managed/system/winforms/Clipboard.cs
		private const int CntUnmanagedRetries = 15; // Default in .NET is 10
		private const int CntUnmanagedDelay = 100;

		private const string CfnViewerIgnore = "Clipboard Viewer Ignore";

		// https://docs.microsoft.com/en-us/windows/win32/dataxchg/clipboard-formats
		private const string CfnNoMonitorProc = "ExcludeClipboardContentFromMonitorProcessing";
		private const string CfnHistory = "CanIncludeInClipboardHistory";
		private const string CfnCloud = "CanUploadToCloudClipboard";

		private static bool OpenW(IntPtr hOwner, bool bEmpty)
		{
			IntPtr h = hOwner;
			if(h == IntPtr.Zero)
			{
				try
				{
					Form f = GlobalWindowManager.TopWindow;
					if(f != null) h = f.Handle;

					if(h == IntPtr.Zero) h = Program.GetSafeMainWindowHandle();
				}
				catch(Exception) { Debug.Assert(false); }
			}

			return InvokeAndRetry(delegate()
			{
				if(NativeMethods.OpenClipboard(h))
				{
					if(bEmpty)
					{
						if(!NativeMethods.EmptyClipboard()) { Debug.Assert(false); }
					}

					return true;
				}

				return false;
			});
		}

		/* private static byte[] GetDataW(uint uFormat)
		{
			IntPtr h = NativeMethods.GetClipboardData(uFormat);
			if(h == IntPtr.Zero) return null;

			UIntPtr pSize = NativeMethods.GlobalSize(h);
			if(pSize == UIntPtr.Zero) return MemUtil.EmptyByteArray;

			IntPtr pMem = NativeMethods.GlobalLock(h);
			if(pMem == IntPtr.Zero) { Debug.Assert(false); return null; }

			byte[] pbMem = new byte[pSize.ToUInt64()];
			Marshal.Copy(pMem, pbMem, 0, pbMem.Length);

			NativeMethods.GlobalUnlock(h); // May return false on success

			return pbMem;
		}

		private static string GetStringW(string strFormat, bool? bForceUni)
		{
			bool bUni = (bForceUni.HasValue ? bForceUni.Value :
				(Marshal.SystemDefaultCharSize >= 2));

			uint uFormat = (bUni ? NativeMethods.CF_UNICODETEXT : NativeMethods.CF_TEXT);
			if(!string.IsNullOrEmpty(strFormat))
				uFormat = NativeMethods.RegisterClipboardFormat(strFormat);

			byte[] pb = GetDataW(uFormat);
			if(pb == null) { Debug.Assert(false); return null; }

			int nBytes = 0;
			for(int i = 0; i < pb.Length; i += (bUni ? 2 : 1))
			{
				if(bUni && (i == (pb.Length - 1))) { Debug.Assert(false); return null; }

				ushort uValue = (bUni ? (ushort)(((ushort)pb[i] << 8) |
					(ushort)pb[i + 1]) : (ushort)pb[i]);
				if(uValue == 0) break;

				nBytes += (bUni ? 2 : 1);
			}

			byte[] pbCharsOnly = new byte[nBytes];
			Array.Copy(pb, pbCharsOnly, nBytes);

			Encoding enc = (bUni ? new UnicodeEncoding(false, false) : Encoding.Default);
			return enc.GetString(pbCharsOnly);
		} */

		private static bool SetDataW(uint uFormat, byte[] pbData)
		{
			UIntPtr pSize = new UIntPtr((uint)pbData.Length);
			IntPtr h = NativeMethods.GlobalAlloc(NativeMethods.GHND, pSize);
			if(h == IntPtr.Zero) { Debug.Assert(false); return false; }

			Debug.Assert(NativeMethods.GlobalSize(h).ToUInt64() >=
				(ulong)pbData.Length); // Might be larger

			IntPtr pMem = NativeMethods.GlobalLock(h);
			if(pMem == IntPtr.Zero)
			{
				Debug.Assert(false);
				NativeMethods.GlobalFree(h);
				return false;
			}

			Marshal.Copy(pbData, 0, pMem, pbData.Length);
			NativeMethods.GlobalUnlock(h); // May return false on success

			if(NativeMethods.SetClipboardData(uFormat, h) == IntPtr.Zero)
			{
				Debug.Assert(false);
				NativeMethods.GlobalFree(h);
				return false;
			}

			return true;
		}

		private static bool SetDataW(uint? ouFormat, string strData, bool? obForceUni)
		{
			if(strData == null) { Debug.Assert(false); return false; }

			bool bUni = (obForceUni.HasValue ? obForceUni.Value :
				(Marshal.SystemDefaultCharSize >= 2));

			uint uFmt = (ouFormat.HasValue ? ouFormat.Value : (bUni ?
				NativeMethods.CF_UNICODETEXT : NativeMethods.CF_TEXT));
			Encoding enc = (bUni ? (new UnicodeEncoding(false, false)) : Encoding.Default);

			byte[] pb = enc.GetBytes(strData);
			byte[] pbWithZero = new byte[pb.Length + (bUni ? 2 : 1)];
			Array.Copy(pb, pbWithZero, pb.Length);
			Debug.Assert(pbWithZero[pb.Length] == 0);

			return SetDataW(uFmt, pbWithZero);
		}

		private static void CloseW()
		{
			if(!NativeMethods.CloseClipboard()) { Debug.Assert(false); }
		}

		private static bool AttachIgnoreFormatsW()
		{
			bool r = true;

			if(Program.Config.Security.UseClipboardViewerIgnoreFormat)
			{
				uint cf = NativeMethods.RegisterClipboardFormat(CfnViewerIgnore);
				if(cf == 0) { Debug.Assert(false); r = false; }
				else if(!SetDataW(cf, PwDefs.ShortProductName, null)) r = false;
			}

			if(Program.Config.Security.ClipboardNoPersist)
			{
				byte[] pbFalse = new byte[4];
				byte[] pbTrue = new byte[] { 1, 0, 0, 0 };

				uint cf = NativeMethods.RegisterClipboardFormat(CfnNoMonitorProc);
				if(cf == 0) { Debug.Assert(false); r = false; }
				// The value type is not defined/documented; we store a BOOL/DWORD
				else if(!SetDataW(cf, pbTrue)) r = false;

				cf = NativeMethods.RegisterClipboardFormat(CfnCloud);
				if(cf == 0) { Debug.Assert(false); r = false; }
				else if(!SetDataW(cf, pbFalse)) r = false;

				cf = NativeMethods.RegisterClipboardFormat(CfnHistory);
				if(cf == 0) { Debug.Assert(false); r = false; }
				else if(!SetDataW(cf, pbFalse)) r = false;
			}

			return r;
		}

		private static bool InvokeAndRetry(GFunc<bool> f)
		{
			if(f == null) { Debug.Assert(false); return false; }

			for(int i = 0; i < CntUnmanagedRetries; ++i)
			{
				try { if(f()) return true; }
				catch(Exception) { }

				Thread.Sleep(CntUnmanagedDelay);
			}

			Debug.Assert(false);
			return false;
		}
	}
}
