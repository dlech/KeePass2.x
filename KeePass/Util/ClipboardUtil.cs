/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2019 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.IO;
using System.Text;
using System.Windows.Forms;

using KeePass.App;
using KeePass.Ecas;
using KeePass.Forms;
using KeePass.Native;
using KeePass.UI;
using KeePass.Util;
using KeePass.Util.Spr;

using KeePassLib;
using KeePassLib.Cryptography;
using KeePassLib.Security;
using KeePassLib.Utility;

using NativeLib = KeePassLib.Native.NativeLib;

namespace KeePass.Util
{
	public static partial class ClipboardUtil
	{
		private static byte[] g_pbDataHash = null;
		private static readonly CriticalSectionEx g_csClearing = new CriticalSectionEx();

		private const string ClipboardIgnoreFormatName = "Clipboard Viewer Ignore";

		[Obsolete]
		public static bool Copy(string strToCopy, bool bIsEntryInfo,
			PwEntry peEntryInfo, PwDatabase pwReferenceSource)
		{
			return Copy(strToCopy, true, bIsEntryInfo, peEntryInfo,
				pwReferenceSource, IntPtr.Zero);
		}

		[Obsolete]
		public static bool Copy(ProtectedString psToCopy, bool bIsEntryInfo,
			PwEntry peEntryInfo, PwDatabase pwReferenceSource)
		{
			if(psToCopy == null) throw new ArgumentNullException("psToCopy");
			return Copy(psToCopy.ReadString(), true, bIsEntryInfo, peEntryInfo,
				pwReferenceSource, IntPtr.Zero);
		}

		public static bool Copy(string strToCopy, bool bSprCompile, bool bIsEntryInfo,
			PwEntry peEntryInfo, PwDatabase pwReferenceSource, IntPtr hOwner)
		{
			if(strToCopy == null) throw new ArgumentNullException("strToCopy");
			if(strToCopy.Length == 0) { Clear(); return true; }

			if(bIsEntryInfo && !AppPolicy.Try(AppPolicyId.CopyToClipboard))
				return false;

			string strData = strToCopy;
			if(bSprCompile)
				strData = SprEngine.Compile(strData, new SprContext(
					peEntryInfo, pwReferenceSource, SprCompileFlags.All));

			try
			{
				if(SetStringUwp(strData)) { }
				else if(!NativeLib.IsUnix()) // Windows
				{
					if(!OpenW(hOwner, true))
						throw new InvalidOperationException();

					bool bFailed = false;
					if(!AttachIgnoreFormatW()) bFailed = true;
					if(!SetDataW(null, strData, null)) bFailed = true;
					CloseW();

					if(bFailed) return false;
				}
				else if(NativeLib.GetPlatformID() == PlatformID.MacOSX)
					SetStringM(strData);
				else if(NativeLib.IsUnix())
					SetStringU(strData);
				else
				{
					Debug.Assert(false);
					Clipboard.SetText(strData);
				}
			}
			catch(Exception) { Debug.Assert(false); return false; }

			g_pbDataHash = HashString(strData);

			if(peEntryInfo != null) peEntryInfo.Touch(false);

			if(bIsEntryInfo)
				Program.TriggerSystem.RaiseEvent(EcasEventIDs.CopiedEntryInfo,
					EcasProperty.Text, strData);

			// SprEngine.Compile might have modified the database
			MainForm mf = Program.MainForm;
			if((mf != null) && bSprCompile)
			{
				mf.RefreshEntriesList();
				mf.UpdateUI(false, null, false, null, false, null, false);
			}

			return true;
		}

		[Obsolete]
		public static bool Copy(byte[] pbToCopy, string strFormat, bool bIsEntryInfo)
		{
			return Copy(pbToCopy, strFormat, bIsEntryInfo, IntPtr.Zero);
		}

		[Obsolete]
		public static bool Copy(byte[] pbToCopy, string strFormat, bool bEncode,
			bool bIsEntryInfo, IntPtr hOwner)
		{
			return Copy(pbToCopy, strFormat, bIsEntryInfo, hOwner);
		}

		public static bool Copy(byte[] pbToCopy, string strFormat, bool bIsEntryInfo,
			IntPtr hOwner)
		{
			if(pbToCopy == null) throw new ArgumentNullException("pbToCopy");
			if(pbToCopy.Length == 0) { Clear(); return true; }

			string strMedia = StrUtil.GetCustomMediaType(strFormat);
			string strData = StrUtil.DataToDataUri(pbToCopy, strMedia);

			return Copy(strData, false, bIsEntryInfo, null, null, hOwner);
		}

		[Obsolete]
		public static byte[] GetEncodedData(string strFormat, IntPtr hOwner)
		{
			return GetData(strFormat);
		}

		public static bool CopyAndMinimize(string strToCopy, bool bIsEntryInfo,
			Form formContext, PwEntry peEntryInfo, PwDatabase pwReferenceSource)
		{
			return CopyAndMinimize(new ProtectedString(false, strToCopy),
				bIsEntryInfo, formContext, peEntryInfo, pwReferenceSource);
		}

		public static bool CopyAndMinimize(ProtectedString psToCopy, bool bIsEntryInfo,
			Form formContext, PwEntry peEntryInfo, PwDatabase pwReferenceSource)
		{
			if(psToCopy == null) throw new ArgumentNullException("psToCopy");

			IntPtr hOwner = ((formContext != null) ? formContext.Handle : IntPtr.Zero);

			if(Copy(psToCopy.ReadString(), true, bIsEntryInfo, peEntryInfo,
				pwReferenceSource, hOwner))
			{
				if(formContext != null)
				{
					if(Program.Config.MainWindow.DropToBackAfterClipboardCopy)
						NativeMethods.LoseFocus(formContext);

					if(Program.Config.MainWindow.MinimizeAfterClipboardCopy)
						UIUtil.SetWindowState(formContext, FormWindowState.Minimized);
				}

				return true;
			}

			return false;
		}

		/// <summary>
		/// Safely clear the clipboard. The clipboard clearing method
		/// of the .NET Framework stores an empty <c>DataObject</c>
		/// in the clipboard; this can cause incompatibilities with
		/// other applications. Therefore, the <c>Clear</c> method of
		/// <c>ClipboardUtil</c> first tries to clear the clipboard using
		/// native Windows functions (which *really* clear the clipboard).
		/// </summary>
		public static void Clear()
		{
			// Ensure that there's no infinite recursion
			if(!g_csClearing.TryEnter()) { Debug.Assert(false); return; }

			// In some situations (e.g. when running in a VM, when using
			// a clipboard extension utility, ...) the clipboard cannot
			// be cleared; for this case we first overwrite the clipboard
			// with a non-sensitive text
			try { Copy("--", false, false, null, null, IntPtr.Zero); }
			catch(Exception) { Debug.Assert(false); }

			bool bNativeSuccess = false;
			try
			{
				if(!NativeLib.IsUnix()) // Windows
				{
					if(OpenW(IntPtr.Zero, true)) // Clears the clipboard
					{
						CloseW();
						bNativeSuccess = true;
					}
				}
				else if(NativeLib.GetPlatformID() == PlatformID.MacOSX)
				{
					SetStringM(string.Empty);
					bNativeSuccess = true;
				}
				else if(NativeLib.IsUnix())
				{
					SetStringU(string.Empty);
					bNativeSuccess = true;
				}
			}
			catch(Exception) { Debug.Assert(false); }

			g_pbDataHash = null;
			g_csClearing.Exit();

			if(bNativeSuccess) return;

			Debug.Assert(false);
			try { Clipboard.Clear(); } // Fallback; empty data object
			catch(Exception) { Debug.Assert(false); }
		}

		public static void ClearIfOwner()
		{
			// Handle-based detection doesn't work well, because a control
			// or dialog that stored the data may not exist anymore and
			// thus GetClipboardOwner returns null
			/* bool bOwnHandle = false;
			try
			{
				if(!NativeLib.IsUnix())
				{
					IntPtr h = NativeMethods.GetClipboardOwner();
					if(h != IntPtr.Zero)
					{
						MainForm mf = Program.MainForm;
						if(((mf != null) && (h == mf.Handle)) ||
							GlobalWindowManager.HasWindow(h))
							bOwnHandle = true;
					}
				}
			}
			catch(Exception) { Debug.Assert(false); } */

			if(g_pbDataHash == null) return;

			byte[] pbCur = ComputeHash();
			if((pbCur == null) || !MemUtil.ArraysEqual(pbCur, g_pbDataHash))
				return;

			Clear();
		}

		private static byte[] HashString(string str)
		{
			try
			{
				if(string.IsNullOrEmpty(str)) return null;

				byte[] pb = StrUtil.Utf8.GetBytes(str);
				return CryptoUtil.HashSha256(pb);
			}
			catch(Exception) { Debug.Assert(false); }

			return null;
		}

		public static byte[] ComputeHash()
		{
			try { return HashString(GetText()); }
			catch(Exception) { Debug.Assert(false); }

			return null;
		}

		public static bool ContainsText()
		{
			if(NativeLib.IsUnix()) return true;
			return Clipboard.ContainsText();
		}

		public static bool ContainsData(string strFormat)
		{
			if(string.IsNullOrEmpty(strFormat)) { Debug.Assert(false); return false; }
			if(strFormat.Equals(DataFormats.UnicodeText, StrUtil.CaseIgnoreCmp) ||
				strFormat.Equals(DataFormats.Text, StrUtil.CaseIgnoreCmp) ||
				strFormat.Equals(DataFormats.OemText, StrUtil.CaseIgnoreCmp))
				return ContainsText();

			string strData = GetText();
			if(string.IsNullOrEmpty(strData)) return false;

			return StrUtil.IsDataUri(strData, StrUtil.GetCustomMediaType(strFormat));
		}

		public static string GetText()
		{
			if(!NativeLib.IsUnix()) // Windows
				return Clipboard.GetText();
			if(NativeLib.GetPlatformID() == PlatformID.MacOSX)
				return GetStringM();
			if(NativeLib.IsUnix())
				return GetStringU();

			Debug.Assert(false);
			return Clipboard.GetText();
		}

		public static byte[] GetData(string strFormat)
		{
			try
			{
				string str = GetText();
				if(string.IsNullOrEmpty(str)) return null;

				string strMedia = StrUtil.GetCustomMediaType(strFormat);
				if(!StrUtil.IsDataUri(str, strMedia)) return null;

				return StrUtil.DataUriToData(str);
			}
			catch(Exception) { Debug.Assert(false); }

			return null;
		}
	}
}
