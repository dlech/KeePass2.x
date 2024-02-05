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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using KeePassLib.Native;
using KeePassLib.Utility;

namespace KeePassLib.Cryptography.KeyDerivation
{
	public sealed partial class AesKdf : KdfEngine
	{
		private const int GCryptBufBlocks = 4096;

		private static bool GCryptInit()
		{
			if(!NativeLib.IsUnix()) return false; // Independent of workaround state
			if(!MonoWorkarounds.IsRequired(1468)) return false; // Can be turned off

			// gcry_check_version initializes the library;
			// throws when LibGCrypt is not available
			NativeMethods.gcry_check_version(IntPtr.Zero);
			return true;
		}

		private static bool GCryptEncrypt(IntPtr h, IntPtr pbZero, IntPtr pbBuf,
			ulong uRounds, byte[] pbOut16)
		{
			ulong r = 0;

			while(uRounds != 0)
			{
				r = Math.Min(uRounds, (ulong)GCryptBufBlocks);
				IntPtr cb = new IntPtr((int)r << 4);

				if(NativeMethods.gcry_cipher_encrypt(h, pbBuf, cb, pbZero, cb) != 0)
				{
					Debug.Assert(false);
					return false;
				}

				uRounds -= r;
			}

			if((pbOut16 != null) && (r != 0))
				Marshal.Copy(MemUtil.AddPtr(pbBuf, ((long)r - 1) << 4), pbOut16, 0, 16);
			return true;
		}

		private static bool GCryptRun16(byte[] pbData16, byte[] pbKey32,
			ref ulong uRounds, uint uTimeMs)
		{
			IntPtr h = IntPtr.Zero;
			try
			{
				if(NativeMethods.gcry_cipher_open(ref h, NativeMethods.GCRY_CIPHER_AES256,
					NativeMethods.GCRY_CIPHER_MODE_CBC, 0) != 0)
				{
					Debug.Assert(false);
					return false;
				}

				using(NativeBufferEx nbKey = new NativeBufferEx(pbKey32, true, true, 16))
				{
					if(NativeMethods.gcry_cipher_setkey(h, nbKey.Data, new IntPtr(32)) != 0)
					{
						Debug.Assert(false);
						return false;
					}
				}

				using(NativeBufferEx nbData = new NativeBufferEx(pbData16, true, true, 16))
				{
					if(NativeMethods.gcry_cipher_setiv(h, nbData.Data, new IntPtr(16)) != 0)
					{
						Debug.Assert(false);
						return false;
					}
				}

				using(NativeBufferEx nbZero = new NativeBufferEx(GCryptBufBlocks << 4,
					true, false, true, 16))
				{
					using(NativeBufferEx nbBuf = new NativeBufferEx(GCryptBufBlocks << 4,
						false, true, true, 16))
					{
						if(uTimeMs == 0)
						{
							if(!GCryptEncrypt(h, nbZero.Data, nbBuf.Data, uRounds,
								pbData16)) return false;
						}
						else
						{
							const ulong uStep = 4096; // Cf. GCryptBufBlocks

							int tStart = Environment.TickCount;
							while((uint)(Environment.TickCount - tStart) < uTimeMs)
							{
								if(!GCryptEncrypt(h, nbZero.Data, nbBuf.Data, uStep,
									null)) return false;

								uRounds += uStep;
								if(uRounds < uStep) // Overflow
								{
									uRounds = unchecked(ulong.MaxValue - 8UL);
									break;
								}
							}
						}
					}
				}

				return true;
			}
			catch(Exception) { Debug.Assert(false); }
			finally
			{
				if(h != IntPtr.Zero)
				{
					try { NativeMethods.gcry_cipher_close(h); }
					catch(Exception) { Debug.Assert(false); }
				}
			}

			return false;
		}

		private static bool GCryptRun32(byte[] pbData32, byte[] pbKey32,
			ref ulong uRounds, uint uTimeMs)
		{
			if(pbData32 == null) { Debug.Assert(false); return false; }
			if(pbData32.Length != 32) { Debug.Assert(false); return false; }
			if(pbKey32 == null) { Debug.Assert(false); return false; }
			if(pbKey32.Length != 32) { Debug.Assert(false); return false; }

			byte[] pbData16L = new byte[16];
			byte[] pbData16R = new byte[16];

			try
			{
				if(!GCryptInit()) return false;

				Array.Copy(pbData32, 0, pbData16L, 0, 16);
				Array.Copy(pbData32, 16, pbData16R, 0, 16);

				ulong uRoundsL = uRounds, uRoundsR = uRounds;
				bool bR = false;

				Thread thR = new Thread(new ThreadStart(delegate()
				{
					bR = GCryptRun16(pbData16R, pbKey32, ref uRoundsR, uTimeMs);
				}));
				thR.Start();

				bool bL = GCryptRun16(pbData16L, pbKey32, ref uRoundsL, uTimeMs);

				thR.Join();

				if(bL && bR)
				{
					if(uTimeMs == 0)
					{
						Array.Copy(pbData16L, 0, pbData32, 0, 16);
						Array.Copy(pbData16R, 0, pbData32, 16, 16);
					}
					else
						uRounds = (Math.Min(uRoundsL, ulong.MaxValue >> 1) +
							Math.Min(uRoundsR, ulong.MaxValue >> 1)) >> 1;

					return true;
				}
			}
			catch(Exception) { Debug.Assert(false); }
			finally
			{
				MemUtil.ZeroByteArray(pbData16L);
				MemUtil.ZeroByteArray(pbData16R);
			}

			return false;
		}

		private static bool TransformKeyGCrypt(byte[] pbData32, byte[] pbKey32,
			ulong uRounds)
		{
			return GCryptRun32(pbData32, pbKey32, ref uRounds, 0);
		}

		private static bool TransformKeyBenchmarkGCrypt(uint uTimeMs, out ulong uRounds)
		{
			uRounds = 0;
			if(uTimeMs == 0) { Debug.Assert(false); return true; }

			byte[] pbData32 = new byte[32];
			pbData32[0] = 0x7E;
			byte[] pbKey32 = new byte[32];
			pbKey32[0] = 0x4B;

			return GCryptRun32(pbData32, pbKey32, ref uRounds, uTimeMs);
		}
	}
}
