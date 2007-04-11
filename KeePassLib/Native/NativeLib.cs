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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace KeePassLib.Native
{
	/// <summary>
	/// Interface to native library (library containing fast versions of
	/// several cryptographic functions).
	/// </summary>
	public static class NativeLib
	{
		private static bool m_bAllowNative = true;

		/// <summary>
		/// If this property is set to <c>true</c>, the native library is used.
		/// If it is <c>false</c>, all calls to functions in this class will fail.
		/// </summary>
		public static bool AllowNative
		{
			get { return m_bAllowNative; }
			set { m_bAllowNative = value; }
		}

		/// <summary>
		/// Determine if the native library is installed.
		/// </summary>
		/// <returns>Returns <c>true</c>, if the native library is installed.</returns>
		public static bool IsLibraryInstalled()
		{
			byte[] pDummy0 = new byte[32];
			byte[] pDummy1 = new byte[32];

			// Save the native state
			bool bCachedNativeState = m_bAllowNative;

			// Temporarily allow native functions and try to load the library
			m_bAllowNative = true;
			bool bResult = TransformKey256(pDummy0, pDummy1, 16);

			// Pop native state and return result
			m_bAllowNative = bCachedNativeState;
			return bResult;
		}

		/// <summary>
		/// Transform a key.
		/// </summary>
		/// <param name="pBuf256">Source and destination buffer.</param>
		/// <param name="pKey256">Key to use in the transformation.</param>
		/// <param name="uRounds">Number of transformation rounds.</param>
		/// <returns>Returns <c>true</c>, if the key was transformed successfully.</returns>
		public static bool TransformKey256(byte[] pBuf256, byte[] pKey256,
			ulong uRounds)
		{
			if(m_bAllowNative == false) return false;

			KeyValuePair<IntPtr, IntPtr> kvp = PrepareArrays256(pBuf256, pKey256);
			bool bResult = false;

			try
			{
				bResult = NativeMethods.TransformKey(kvp.Key, kvp.Value, uRounds);
			}
			catch(Exception) { bResult = false; }

			if(bResult) GetBuffers256(kvp, pBuf256, pKey256);

			NativeLib.FreeArrays(kvp);
			return bResult;
		}

		/// <summary>
		/// Benchmark key transformation.
		/// </summary>
		/// <param name="pBuf256">Source and destination buffer.</param>
		/// <param name="pKey256">Key to use in the transformation.</param>
		/// <param name="puRounds">Number of transformations done.</param>
		/// <param name="uSeconds">Number of seconds to perform the benchmark.</param>
		/// <returns>Returns <c>true</c>, if the benchmark was successful.</returns>
		public static bool TransformKey256Timed(byte[] pBuf256, byte[] pKey256,
			ref ulong puRounds, uint uSeconds)
		{
			if(m_bAllowNative == false) return false;

			KeyValuePair<IntPtr, IntPtr> kvp = PrepareArrays256(pBuf256, pKey256);
			bool bResult = false;

			try
			{
				bResult = NativeMethods.TransformKeyTimed(kvp.Key, kvp.Value, ref puRounds, uSeconds);
			}
			catch(Exception) { Debug.Assert(false); bResult = false; }

			Debug.Assert(bResult);
			if(bResult) GetBuffers256(kvp, pBuf256, pKey256);

			NativeLib.FreeArrays(kvp);
			return bResult;
		}

		private static KeyValuePair<IntPtr, IntPtr> PrepareArrays256(byte[] pBuf256,
			byte[] pKey256)
		{
			Debug.Assert((pBuf256 != null) && (pBuf256.Length == 32));
			if((pBuf256 == null) || (pBuf256.Length != 32)) throw new ArgumentNullException();

			Debug.Assert((pKey256 != null) && (pKey256.Length == 32));
			if((pKey256 == null) || (pKey256.Length != 32)) throw new ArgumentNullException();

			IntPtr hBuf = Marshal.AllocHGlobal(pBuf256.Length);
			Marshal.Copy(pBuf256, 0, hBuf, pBuf256.Length);

			IntPtr hKey = Marshal.AllocHGlobal(pKey256.Length);
			Marshal.Copy(pKey256, 0, hKey, pKey256.Length);

			return new KeyValuePair<IntPtr, IntPtr>(hBuf, hKey);
		}

		private static void GetBuffers256(KeyValuePair<IntPtr, IntPtr> kvpSource,
			byte[] pbDestBuf, byte[] pbDestKey)
		{
			if(kvpSource.Key != IntPtr.Zero)
				Marshal.Copy(kvpSource.Key, pbDestBuf, 0, pbDestBuf.Length);

			if(kvpSource.Value != IntPtr.Zero)
				Marshal.Copy(kvpSource.Value, pbDestKey, 0, pbDestKey.Length);
		}

		private static void FreeArrays(KeyValuePair<IntPtr, IntPtr> kvpPointers)
		{
			if(kvpPointers.Key != IntPtr.Zero)
				Marshal.FreeHGlobal(kvpPointers.Key);

			if(kvpPointers.Value != IntPtr.Zero)
				Marshal.FreeHGlobal(kvpPointers.Value);
		}
	}
}
