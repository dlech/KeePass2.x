/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2018 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using KeePassLib.Utility;

namespace KeePassLib.Native
{
	internal static partial class NativeMethods
	{
		internal const int MAX_PATH = 260;

		internal const long INVALID_HANDLE_VALUE = -1;

		internal const uint MOVEFILE_REPLACE_EXISTING = 0x00000001;
		internal const uint MOVEFILE_COPY_ALLOWED = 0x00000002;

		internal const uint FILE_SUPPORTS_TRANSACTIONS = 0x00200000;
		internal const int MAX_TRANSACTION_DESCRIPTION_LENGTH = 64;

		// internal const uint TF_SFT_SHOWNORMAL = 0x00000001;
		// internal const uint TF_SFT_HIDDEN = 0x00000008;

		/* [DllImport("KeePassNtv32.dll", EntryPoint = "TransformKey")]
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
			if(IntPtr.Size == 4)
				return TransformKey32(pBuf256, pKey256, uRounds);
			return TransformKey64(pBuf256, pKey256, uRounds);
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
			if(IntPtr.Size == 4)
				return TransformKeyTimed32(pBuf256, pKey256, ref puRounds, uSeconds);
			return TransformKeyTimed64(pBuf256, pKey256, ref puRounds, uSeconds);
		} */

#if !KeePassUAP
		[DllImport("KeePassLibC32.dll", EntryPoint = "TransformKey256")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool TransformKey32(IntPtr pBuf256,
			IntPtr pKey256, UInt64 uRounds);

		[DllImport("KeePassLibC64.dll", EntryPoint = "TransformKey256")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool TransformKey64(IntPtr pBuf256,
			IntPtr pKey256, UInt64 uRounds);

		internal static bool TransformKey(IntPtr pBuf256, IntPtr pKey256,
			UInt64 uRounds)
		{
			if(IntPtr.Size == 4)
				return TransformKey32(pBuf256, pKey256, uRounds);
			return TransformKey64(pBuf256, pKey256, uRounds);
		}

		[DllImport("KeePassLibC32.dll", EntryPoint = "TransformKeyBenchmark256")]
		private static extern UInt64 TransformKeyBenchmark32(UInt32 uTimeMs);

		[DllImport("KeePassLibC64.dll", EntryPoint = "TransformKeyBenchmark256")]
		private static extern UInt64 TransformKeyBenchmark64(UInt32 uTimeMs);

		internal static UInt64 TransformKeyBenchmark(UInt32 uTimeMs)
		{
			if(IntPtr.Size == 4)
				return TransformKeyBenchmark32(uTimeMs);
			return TransformKeyBenchmark64(uTimeMs);
		}
#endif

		/* [DllImport("KeePassLibC32.dll", EntryPoint = "TF_ShowLangBar")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool TF_ShowLangBar32(UInt32 dwFlags);

		[DllImport("KeePassLibC64.dll", EntryPoint = "TF_ShowLangBar")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool TF_ShowLangBar64(UInt32 dwFlags);

		internal static bool TfShowLangBar(uint dwFlags)
		{
			if(IntPtr.Size == 4) return TF_ShowLangBar32(dwFlags);
			return TF_ShowLangBar64(dwFlags);
		} */

		[DllImport("KeePassLibC32.dll", EntryPoint = "ProtectProcessWithDacl")]
		private static extern void ProtectProcessWithDacl32();

		[DllImport("KeePassLibC64.dll", EntryPoint = "ProtectProcessWithDacl")]
		private static extern void ProtectProcessWithDacl64();

		internal static void ProtectProcessWithDacl()
		{
			try
			{
				if(NativeLib.IsUnix()) return;

				if(IntPtr.Size == 4) ProtectProcessWithDacl32();
				else ProtectProcessWithDacl64();
			}
			catch(Exception) { Debug.Assert(false); }
		}

		[DllImport("Kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool CloseHandle(IntPtr hObject);

		[DllImport("Kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = false,
			SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GetVolumeInformation(string lpRootPathName,
			StringBuilder lpVolumeNameBuffer, UInt32 nVolumeNameSize,
			ref UInt32 lpVolumeSerialNumber, ref UInt32 lpMaximumComponentLength,
			ref UInt32 lpFileSystemFlags, StringBuilder lpFileSystemNameBuffer,
			UInt32 nFileSystemNameSize);

		[DllImport("Kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = false,
			SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool MoveFileEx(string lpExistingFileName,
			string lpNewFileName, UInt32 dwFlags);

		[DllImport("KtmW32.dll", CharSet = CharSet.Unicode, ExactSpelling = true,
			SetLastError = true)]
		internal static extern IntPtr CreateTransaction(IntPtr lpTransactionAttributes,
			IntPtr lpUOW, UInt32 dwCreateOptions, UInt32 dwIsolationLevel,
			UInt32 dwIsolationFlags, UInt32 dwTimeout, string lpDescription);

		[DllImport("KtmW32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool CommitTransaction(IntPtr hTransaction);

		[DllImport("Kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = false,
			SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool MoveFileTransacted(string lpExistingFileName,
			string lpNewFileName, IntPtr lpProgressRoutine, IntPtr lpData,
			UInt32 dwFlags, IntPtr hTransaction);

#if (!KeePassLibSD && !KeePassUAP)
		[DllImport("ShlWApi.dll", CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool PathRelativePathTo([Out] StringBuilder pszPath,
			[In] string pszFrom, uint dwAttrFrom, [In] string pszTo, uint dwAttrTo);

		[DllImport("ShlWApi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		private static extern int StrCmpLogicalW(string x, string y);

		private static bool? m_obSupportsLogicalCmp = null;

		private static void TestNaturalComparisonsSupport()
		{
			try
			{
				StrCmpLogicalW("0", "0"); // Throws exception if unsupported
				m_obSupportsLogicalCmp = true;
			}
			catch(Exception) { m_obSupportsLogicalCmp = false; }
		}
#endif

		internal static bool SupportsStrCmpNaturally
		{
			get
			{
#if (!KeePassLibSD && !KeePassUAP)
				if(!m_obSupportsLogicalCmp.HasValue)
					TestNaturalComparisonsSupport();

				return m_obSupportsLogicalCmp.Value;
#else
				return false;
#endif
			}
		}

		internal static int StrCmpNaturally(string x, string y)
		{
#if (!KeePassLibSD && !KeePassUAP)
			if(!NativeMethods.SupportsStrCmpNaturally)
			{
				Debug.Assert(false);
				return string.Compare(x, y, true);
			}

			return StrCmpLogicalW(x, y);
#else
			Debug.Assert(false);
			return string.Compare(x, y, true);
#endif
		}

		internal static string GetUserRuntimeDir()
		{
#if KeePassLibSD
			return Path.GetTempPath();
#else
#if KeePassUAP
			string strRtDir = EnvironmentExt.AppDataLocalFolderPath;
#else
			string strRtDir = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR");
			if(string.IsNullOrEmpty(strRtDir))
				strRtDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			if(string.IsNullOrEmpty(strRtDir))
			{
				Debug.Assert(false);
				return Path.GetTempPath(); // Not UrlUtil (otherwise cyclic)
			}
#endif

			strRtDir = UrlUtil.EnsureTerminatingSeparator(strRtDir, false);
			strRtDir += PwDefs.ShortProductName;

			return strRtDir;
#endif
		}
	}
}
