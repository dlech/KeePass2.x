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

using KeePassLib.Native;
using KeePassLib.Utility;

namespace KeePassLib.Cryptography.KeyDerivation
{
	public sealed partial class AesKdf : KdfEngine
	{
		private static bool TransformKeyGCrypt(byte[] pbData32, byte[] pbSeed32,
			ulong uRounds)
		{
			byte[] pbNewData32 = null;
			try
			{
				if(GCryptInitLib())
				{
					pbNewData32 = new byte[32];
					Array.Copy(pbData32, pbNewData32, 32);

					if(TransformKeyGCryptPriv(pbNewData32, pbSeed32, uRounds))
					{
						Array.Copy(pbNewData32, pbData32, 32);
						return true;
					}
				}
			}
			catch(Exception) { }
			finally { if(pbNewData32 != null) MemUtil.ZeroByteArray(pbNewData32); }

			return false;
		}

		private static bool TransformKeyBenchmarkGCrypt(uint uTimeMs, out ulong uRounds)
		{
			uRounds = 0;

			try
			{
				if(GCryptInitLib())
					return TransformKeyBenchmarkGCryptPriv(uTimeMs, ref uRounds);
			}
			catch(Exception) { }

			return false;
		}

		private static bool GCryptInitLib()
		{
			if(!NativeLib.IsUnix()) return false; // Independent of workaround state
			if(!MonoWorkarounds.IsRequired(1468)) return false; // Can be turned off

			// gcry_check_version initializes the library;
			// throws when LibGCrypt is not available
			NativeMethods.gcry_check_version(IntPtr.Zero);
			return true;
		}

		// =============================================================
		// Multi-threaded implementation

		// For some reason, the following multi-threaded implementation
		// is slower than the single-threaded implementation below
		// (threading overhead by Mono? LibGCrypt threading issues?)
		/* private sealed class GCryptTransformInfo : IDisposable
		{
			public IntPtr Data16;
			public IntPtr Seed32;
			public ulong Rounds;
			public uint TimeMs;

			public bool Success = false;

			public GCryptTransformInfo(byte[] pbData32, int iDataOffset,
				byte[] pbSeed32, ulong uRounds, uint uTimeMs)
			{
				this.Data16 = Marshal.AllocCoTaskMem(16);
				Marshal.Copy(pbData32, iDataOffset, this.Data16, 16);

				this.Seed32 = Marshal.AllocCoTaskMem(32);
				Marshal.Copy(pbSeed32, 0, this.Seed32, 32);

				this.Rounds = uRounds;
				this.TimeMs = uTimeMs;
			}

			public void Dispose()
			{
				if(this.Data16 != IntPtr.Zero)
				{
					Marshal.WriteInt64(this.Data16, 0);
					Marshal.WriteInt64(this.Data16, 8, 0);
					Marshal.FreeCoTaskMem(this.Data16);
					this.Data16 = IntPtr.Zero;
				}

				if(this.Seed32 != IntPtr.Zero)
				{
					Marshal.FreeCoTaskMem(this.Seed32);
					this.Seed32 = IntPtr.Zero;
				}
			}
		}

		private static GCryptTransformInfo[] GCryptRun(byte[] pbData32,
			byte[] pbSeed32, ulong uRounds, uint uTimeMs, ParameterizedThreadStart fL,
			ParameterizedThreadStart fR)
		{
			GCryptTransformInfo tiL = new GCryptTransformInfo(pbData32, 0,
				pbSeed32, uRounds, uTimeMs);
			GCryptTransformInfo tiR = new GCryptTransformInfo(pbData32, 16,
				pbSeed32, uRounds, uTimeMs);

			Thread th = new Thread(fL);
			th.Start(tiL);

			fR(tiR);

			th.Join();

			Marshal.Copy(tiL.Data16, pbData32, 0, 16);
			Marshal.Copy(tiR.Data16, pbData32, 16, 16);

			tiL.Dispose();
			tiR.Dispose();

			if(tiL.Success && tiR.Success)
				return new GCryptTransformInfo[2] { tiL, tiR };
			return null;
		}

		private static bool TransformKeyGCryptPriv(byte[] pbData32, byte[] pbSeed32,
			ulong uRounds)
		{
			return (GCryptRun(pbData32, pbSeed32, uRounds, 0,
				new ParameterizedThreadStart(AesKdf.GCryptTransformTh),
				new ParameterizedThreadStart(AesKdf.GCryptTransformTh)) != null);
		}

		private static bool GCryptInitCipher(ref IntPtr h, GCryptTransformInfo ti)
		{
			NativeMethods.gcry_cipher_open(ref h, NativeMethods.GCRY_CIPHER_AES256,
				NativeMethods.GCRY_CIPHER_MODE_ECB, 0);
			if(h == IntPtr.Zero) { Debug.Assert(false); return false; }

			IntPtr n32 = new IntPtr(32);
			if(NativeMethods.gcry_cipher_setkey(h, ti.Seed32, n32) != 0)
			{
				Debug.Assert(false);
				return false;
			}

			return true;
		}

		private static void GCryptTransformTh(object o)
		{
			IntPtr h = IntPtr.Zero;
			try
			{
				GCryptTransformInfo ti = (o as GCryptTransformInfo);
				if(ti == null) { Debug.Assert(false); return; }

				if(!GCryptInitCipher(ref h, ti)) return;

				IntPtr n16 = new IntPtr(16);
				for(ulong u = 0; u < ti.Rounds; ++u)
				{
					if(NativeMethods.gcry_cipher_encrypt(h, ti.Data16, n16,
						IntPtr.Zero, IntPtr.Zero) != 0)
					{
						Debug.Assert(false);
						return;
					}
				}

				ti.Success = true;
			}
			catch(Exception) { Debug.Assert(false); }
			finally
			{
				try { if(h != IntPtr.Zero) NativeMethods.gcry_cipher_close(h); }
				catch(Exception) { Debug.Assert(false); }
			}
		}

		private static bool TransformKeyBenchmarkGCryptPriv(uint uTimeMs, ref ulong uRounds)
		{
			GCryptTransformInfo[] v = GCryptRun(new byte[32], new byte[32],
				0, uTimeMs,
				new ParameterizedThreadStart(AesKdf.GCryptBenchmarkTh),
				new ParameterizedThreadStart(AesKdf.GCryptBenchmarkTh));

			if(v != null)
			{
				ulong uL = Math.Min(v[0].Rounds, ulong.MaxValue >> 1);
				ulong uR = Math.Min(v[1].Rounds, ulong.MaxValue >> 1);
				uRounds = (uL + uR) / 2;

				return true;
			}
			return false;
		}

		private static void GCryptBenchmarkTh(object o)
		{
			IntPtr h = IntPtr.Zero;
			try
			{
				GCryptTransformInfo ti = (o as GCryptTransformInfo);
				if(ti == null) { Debug.Assert(false); return; }

				if(!GCryptInitCipher(ref h, ti)) return;

				ulong r = 0;
				IntPtr n16 = new IntPtr(16);
				int tStart = Environment.TickCount;
				while(true)
				{
					for(ulong j = 0; j < BenchStep; ++j)
					{
						if(NativeMethods.gcry_cipher_encrypt(h, ti.Data16, n16,
							IntPtr.Zero, IntPtr.Zero) != 0)
						{
							Debug.Assert(false);
							return;
						}
					}

					r += BenchStep;
					if(r < BenchStep) // Overflow check
					{
						r = ulong.MaxValue;
						break;
					}

					uint tElapsed = (uint)(Environment.TickCount - tStart);
					if(tElapsed > ti.TimeMs) break;
				}

				ti.Rounds = r;
				ti.Success = true;
			}
			catch(Exception) { Debug.Assert(false); }
			finally
			{
				try { if(h != IntPtr.Zero) NativeMethods.gcry_cipher_close(h); }
				catch(Exception) { Debug.Assert(false); }
			}
		} */

		// =============================================================
		// Single-threaded implementation

		private static bool GCryptInitCipher(ref IntPtr h, IntPtr pSeed32)
		{
			NativeMethods.gcry_cipher_open(ref h, NativeMethods.GCRY_CIPHER_AES256,
				NativeMethods.GCRY_CIPHER_MODE_ECB, 0);
			if(h == IntPtr.Zero) { Debug.Assert(false); return false; }

			IntPtr n32 = new IntPtr(32);
			if(NativeMethods.gcry_cipher_setkey(h, pSeed32, n32) != 0)
			{
				Debug.Assert(false);
				return false;
			}

			return true;
		}

		private static bool GCryptBegin(byte[] pbData32, byte[] pbSeed32,
			ref IntPtr h, ref IntPtr pData32, ref IntPtr pSeed32)
		{
			pData32 = Marshal.AllocCoTaskMem(32);
			pSeed32 = Marshal.AllocCoTaskMem(32);

			Marshal.Copy(pbData32, 0, pData32, 32);
			Marshal.Copy(pbSeed32, 0, pSeed32, 32);

			return GCryptInitCipher(ref h, pSeed32);
		}

		private static void GCryptEnd(IntPtr h, IntPtr pData32, IntPtr pSeed32)
		{
			NativeMethods.gcry_cipher_close(h);

			Marshal.WriteInt64(pData32, 0);
			Marshal.WriteInt64(pData32, 8, 0);
			Marshal.WriteInt64(pData32, 16, 0);
			Marshal.WriteInt64(pData32, 24, 0);

			Marshal.FreeCoTaskMem(pData32);
			Marshal.FreeCoTaskMem(pSeed32);
		}

		private static bool TransformKeyGCryptPriv(byte[] pbData32, byte[] pbSeed32,
			ulong uRounds)
		{
			IntPtr h = IntPtr.Zero, pData32 = IntPtr.Zero, pSeed32 = IntPtr.Zero;
			if(!GCryptBegin(pbData32, pbSeed32, ref h, ref pData32, ref pSeed32))
				return false;

			try
			{
				IntPtr n32 = new IntPtr(32);
				for(ulong i = 0; i < uRounds; ++i)
				{
					if(NativeMethods.gcry_cipher_encrypt(h, pData32, n32,
						IntPtr.Zero, IntPtr.Zero) != 0)
					{
						Debug.Assert(false);
						return false;
					}
				}

				Marshal.Copy(pData32, pbData32, 0, 32);
				return true;
			}
			catch(Exception) { Debug.Assert(false); }
			finally { GCryptEnd(h, pData32, pSeed32); }

			return false;
		}

		private static bool TransformKeyBenchmarkGCryptPriv(uint uTimeMs, ref ulong uRounds)
		{
			byte[] pbData32 = new byte[32];
			byte[] pbSeed32 = new byte[32];

			IntPtr h = IntPtr.Zero, pData32 = IntPtr.Zero, pSeed32 = IntPtr.Zero;
			if(!GCryptBegin(pbData32, pbSeed32, ref h, ref pData32, ref pSeed32))
				return false;

			uint uMaxMs = uTimeMs;
			ulong uDiv = 1;
			if(uMaxMs <= (uint.MaxValue >> 1)) { uMaxMs *= 2U; uDiv = 2; }

			try
			{
				ulong r = 0;
				IntPtr n32 = new IntPtr(32);
				int tStart = Environment.TickCount;
				while(true)
				{
					for(ulong j = 0; j < BenchStep; ++j)
					{
						if(NativeMethods.gcry_cipher_encrypt(h, pData32, n32,
							IntPtr.Zero, IntPtr.Zero) != 0)
						{
							Debug.Assert(false);
							return false;
						}
					}

					r += BenchStep;
					if(r < BenchStep) // Overflow check
					{
						r = ulong.MaxValue;
						break;
					}

					uint tElapsed = (uint)(Environment.TickCount - tStart);
					if(tElapsed > uMaxMs) break;
				}

				uRounds = r / uDiv;
				return true;
			}
			catch(Exception) { Debug.Assert(false); }
			finally { GCryptEnd(h, pData32, pSeed32); }

			return false;
		}
	}
}
