/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2025 Dominik Reichl <dominik.reichl@t-online.de>

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

		internal static readonly Guid FOLDERID_SkyDrive = new Guid(
			"A52BBA46-E9E1-435F-B3D9-28DAA648C0F6");

		// internal const uint TF_SFT_SHOWNORMAL = 0x00000001;
		// internal const uint TF_SFT_HIDDEN = 0x00000008;

#if !KeePassUAP
		[DllImport(NativeLib.DllFileX32, EntryPoint = "AesKdfTransformHalf",
			ExactSpelling = true, CallingConvention = NativeLib.DllCallingConvention)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool AesKdfTransformHalfX32(IntPtr pbData16,
			IntPtr pbSeed32, ulong uRounds);

		[DllImport(NativeLib.DllFileX64, EntryPoint = "AesKdfTransformHalf",
			ExactSpelling = true, CallingConvention = NativeLib.DllCallingConvention)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool AesKdfTransformHalfX64(IntPtr pbData16,
			IntPtr pbSeed32, ulong uRounds);

		[DllImport(NativeLib.DllFileA64, EntryPoint = "AesKdfTransformHalf",
			ExactSpelling = true, CallingConvention = NativeLib.DllCallingConvention)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool AesKdfTransformHalfA64(IntPtr pbData16,
			IntPtr pbSeed32, ulong uRounds);

		internal static bool AesKdfTransformHalf(IntPtr pbData16, IntPtr pbSeed32,
			ulong uRounds)
		{
			ArchitectureEx a = NativeLib.ProcessArchitecture;

			if(a == ArchitectureEx.X86)
				return AesKdfTransformHalfX32(pbData16, pbSeed32, uRounds);
			if(a == ArchitectureEx.X64)
				return AesKdfTransformHalfX64(pbData16, pbSeed32, uRounds);
			if(a == ArchitectureEx.Arm64)
				return AesKdfTransformHalfA64(pbData16, pbSeed32, uRounds);

			Debug.Assert(false);
			return false;
		}

		[DllImport(NativeLib.DllFileX32, EntryPoint = "AesKdfTransformBenchmarkHalf",
			ExactSpelling = true, CallingConvention = NativeLib.DllCallingConvention)]
		private static extern ulong AesKdfTransformBenchmarkHalfX32(uint uMilliseconds);

		[DllImport(NativeLib.DllFileX64, EntryPoint = "AesKdfTransformBenchmarkHalf",
			ExactSpelling = true, CallingConvention = NativeLib.DllCallingConvention)]
		private static extern ulong AesKdfTransformBenchmarkHalfX64(uint uMilliseconds);

		[DllImport(NativeLib.DllFileA64, EntryPoint = "AesKdfTransformBenchmarkHalf",
			ExactSpelling = true, CallingConvention = NativeLib.DllCallingConvention)]
		private static extern ulong AesKdfTransformBenchmarkHalfA64(uint uMilliseconds);

		internal static ulong AesKdfTransformBenchmarkHalf(uint uMilliseconds)
		{
			ArchitectureEx a = NativeLib.ProcessArchitecture;

			if(a == ArchitectureEx.X86)
				return AesKdfTransformBenchmarkHalfX32(uMilliseconds);
			if(a == ArchitectureEx.X64)
				return AesKdfTransformBenchmarkHalfX64(uMilliseconds);
			if(a == ArchitectureEx.Arm64)
				return AesKdfTransformBenchmarkHalfA64(uMilliseconds);

			Debug.Assert(false);
			return 0;
		}
#endif

		// =============================================================
		// LibArgon2 20190702/20210625+
		// Cf. methods in 'NativeMethods.Unix.cs'.

		[DllImport(NativeLib.DllFileX32, EntryPoint = "argon2_hash",
			ExactSpelling = true, CallingConvention = NativeLib.DllCallingConvention)]
		private static extern int argon2_hash_x32(uint t_cost, uint m_cost,
			uint parallelism, IntPtr pwd, IntPtr pwdlen, IntPtr salt,
			IntPtr saltlen, IntPtr hash, IntPtr hashlen, IntPtr encoded,
			IntPtr encodedlen, int type, uint version);

		[DllImport(NativeLib.DllFileX64, EntryPoint = "argon2_hash",
			ExactSpelling = true, CallingConvention = NativeLib.DllCallingConvention)]
		private static extern int argon2_hash_x64(uint t_cost, uint m_cost,
			uint parallelism, IntPtr pwd, IntPtr pwdlen, IntPtr salt,
			IntPtr saltlen, IntPtr hash, IntPtr hashlen, IntPtr encoded,
			IntPtr encodedlen, int type, uint version);

		[DllImport(NativeLib.DllFileA64, EntryPoint = "argon2_hash",
			ExactSpelling = true, CallingConvention = NativeLib.DllCallingConvention)]
		private static extern int argon2_hash_a64(uint t_cost, uint m_cost,
			uint parallelism, IntPtr pwd, IntPtr pwdlen, IntPtr salt,
			IntPtr saltlen, IntPtr hash, IntPtr hashlen, IntPtr encoded,
			IntPtr encodedlen, int type, uint version);

		internal static int argon2_hash(uint t_cost, uint m_cost,
			uint parallelism, IntPtr pwd, IntPtr pwdlen, IntPtr salt,
			IntPtr saltlen, IntPtr hash, IntPtr hashlen, IntPtr encoded,
			IntPtr encodedlen, int type, uint version)
		{
			ArchitectureEx a = NativeLib.ProcessArchitecture;

			if(a == ArchitectureEx.X86)
				return argon2_hash_x32(t_cost, m_cost, parallelism, pwd, pwdlen, salt,
					saltlen, hash, hashlen, encoded, encodedlen, type, version);
			if(a == ArchitectureEx.X64)
				return argon2_hash_x64(t_cost, m_cost, parallelism, pwd, pwdlen, salt,
					saltlen, hash, hashlen, encoded, encodedlen, type, version);
			if(a == ArchitectureEx.Arm64)
				return argon2_hash_a64(t_cost, m_cost, parallelism, pwd, pwdlen, salt,
					saltlen, hash, hashlen, encoded, encodedlen, type, version);

			Debug.Assert(false);
			return int.MinValue;
		}

		[DllImport(NativeLib.DllFileX32, EntryPoint = "AuxProtectProcessWithDacl",
			ExactSpelling = true, CallingConvention = NativeLib.DllCallingConvention)]
		private static extern void AuxProtectProcessWithDaclX32();

		[DllImport(NativeLib.DllFileX64, EntryPoint = "AuxProtectProcessWithDacl",
			ExactSpelling = true, CallingConvention = NativeLib.DllCallingConvention)]
		private static extern void AuxProtectProcessWithDaclX64();

		[DllImport(NativeLib.DllFileA64, EntryPoint = "AuxProtectProcessWithDacl",
			ExactSpelling = true, CallingConvention = NativeLib.DllCallingConvention)]
		private static extern void AuxProtectProcessWithDaclA64();

		internal static void AuxProtectProcessWithDacl()
		{
			try
			{
				if(NativeLib.IsUnix()) return;

				ArchitectureEx a = NativeLib.ProcessArchitecture;

				if(a == ArchitectureEx.X86)
					AuxProtectProcessWithDaclX32();
				else if(a == ArchitectureEx.X64)
					AuxProtectProcessWithDaclX64();
				else if(a == ArchitectureEx.Arm64)
					AuxProtectProcessWithDaclA64();
				else { Debug.Assert(false); }
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

		[DllImport("Shell32.dll")]
		private static extern int SHGetKnownFolderPath(ref Guid rfid, uint dwFlags,
			IntPtr hToken, out IntPtr ppszPath);

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

		internal static string GetKnownFolderPath(Guid g)
		{
			if(Marshal.SystemDefaultCharSize != 2) { Debug.Assert(false); return string.Empty; }

			IntPtr pszPath = IntPtr.Zero;
			try
			{
				if(SHGetKnownFolderPath(ref g, 0, IntPtr.Zero, out pszPath) == 0)
				{
					if(pszPath != IntPtr.Zero)
						return Marshal.PtrToStringUni(pszPath);
					Debug.Assert(false);
				}
			}
			catch(Exception) { Debug.Assert(false); }
			finally
			{
				try { if(pszPath != IntPtr.Zero) Marshal.FreeCoTaskMem(pszPath); }
				catch(Exception) { Debug.Assert(false); }
			}

			return string.Empty;
		}
	}
}
