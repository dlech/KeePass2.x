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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

#if !KeePassUAP
using System.Drawing;
using System.Security.Cryptography;
using System.Windows.Forms;
#endif

using KeePassLib.Delegates;
using KeePassLib.Native;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePassLib.Cryptography
{
	/// <summary>
	/// Cryptographically secure pseudo-random number generator.
	/// The returned values are unpredictable and cannot be reproduced.
	/// <c>CryptoRandom</c> is a singleton class.
	/// </summary>
	public sealed class CryptoRandom
	{
		private ProtectedBinary m_pbEntropyPool = new ProtectedBinary(
			true, new byte[64]);
		private RNGCryptoServiceProvider m_rng = new RNGCryptoServiceProvider();
		private ulong m_uCounter;
		private ulong m_uGeneratedBytesCount = 0;

		private static readonly object g_oSyncRoot = new object();
		private readonly object m_oSyncRoot = new object();

		private static CryptoRandom g_pInstance = null;
		public static CryptoRandom Instance
		{
			get
			{
				CryptoRandom cr;
				lock(g_oSyncRoot)
				{
					cr = g_pInstance;
					if(cr == null)
					{
						cr = new CryptoRandom();
						g_pInstance = cr;
					}
				}

				return cr;
			}
		}

		/// <summary>
		/// Get the number of random bytes that this instance generated so far.
		/// Note that this number can be higher than the number of random bytes
		/// actually requested using the <c>GetRandomBytes</c> method.
		/// </summary>
		public ulong GeneratedBytesCount
		{
			get
			{
				ulong u;
				lock(m_oSyncRoot) { u = m_uGeneratedBytesCount; }
				return u;
			}
		}

		/// <summary>
		/// Event that is triggered whenever the internal <c>GenerateRandom256</c>
		/// method is called to generate random bytes.
		/// </summary>
		public event EventHandler GenerateRandom256Pre;

		private CryptoRandom()
		{
			m_uCounter = (ulong)DateTime.UtcNow.ToBinary();

			byte[] pb = GetSystemEntropy();
			AddEntropy(pb);
			MemUtil.ZeroByteArray(pb);
		}

		/// <summary>
		/// Update the internal seed of the random number generator based
		/// on entropy data.
		/// This method is thread-safe.
		/// </summary>
		/// <param name="pbEntropy">Entropy bytes.</param>
		public void AddEntropy(byte[] pbEntropy)
		{
			if(pbEntropy == null) { Debug.Assert(false); return; }
			if(pbEntropy.Length == 0) { Debug.Assert(false); return; }

			byte[] pbNewData = pbEntropy;
			if(pbEntropy.Length > 64)
			{
#if KeePassLibSD
				using(SHA256Managed shaNew = new SHA256Managed())
#else
				using(SHA512Managed shaNew = new SHA512Managed())
#endif
				{
					pbNewData = shaNew.ComputeHash(pbEntropy);
				}
			}

			lock(m_oSyncRoot)
			{
				byte[] pbPool = m_pbEntropyPool.ReadData();
				int cbPool = pbPool.Length;
				int cbNew = pbNewData.Length;

				byte[] pbCmp = new byte[cbPool + cbNew];
				Array.Copy(pbPool, pbCmp, cbPool);
				Array.Copy(pbNewData, 0, pbCmp, cbPool, cbNew);

#if KeePassLibSD
				using(SHA256Managed shaPool = new SHA256Managed())
#else
				using(SHA512Managed shaPool = new SHA512Managed())
#endif
				{
					byte[] pbNewPool = shaPool.ComputeHash(pbCmp);
					m_pbEntropyPool = new ProtectedBinary(true, pbNewPool);
					MemUtil.ZeroByteArray(pbNewPool);
				}

				MemUtil.ZeroByteArray(pbCmp);
				MemUtil.ZeroByteArray(pbPool);
			}

			if(pbNewData != pbEntropy) MemUtil.ZeroByteArray(pbNewData);
		}

		private byte[] GetSystemEntropy()
		{
			SHA512Managed h = new SHA512Managed();
			byte[] pb4 = new byte[4];
			byte[] pb8 = new byte[8];

			GAction<byte[], bool> f = delegate(byte[] pbValue, bool bClearValue)
			{
				if(pbValue == null) { Debug.Assert(false); return; }
				if(pbValue.Length == 0) return;
				h.TransformBlock(pbValue, 0, pbValue.Length, pbValue, 0);
				if(bClearValue) MemUtil.ZeroByteArray(pbValue);
			};
			Action<int> fI32 = delegate(int iValue)
			{
				MemUtil.Int32ToBytesEx(iValue, pb4, 0);
				f(pb4, false);
			};
			Action<long> fI64 = delegate(long lValue)
			{
				MemUtil.Int64ToBytesEx(lValue, pb8, 0);
				f(pb8, false);
			};
			Action<string> fStr = delegate(string strValue)
			{
				if(strValue == null) { Debug.Assert(false); return; }
				if(strValue.Length == 0) return;
				f(StrUtil.Utf8.GetBytes(strValue), false);
			};

			fI32(Environment.TickCount);
			fI64(DateTime.UtcNow.ToBinary());

#if !KeePassLibSD
			// In try-catch for systems without GUI;
			// https://sourceforge.net/p/keepass/discussion/329221/thread/20335b73/
			try
			{
				Point pt = Cursor.Position;
				fI32(pt.X);
				fI32(pt.Y);
			}
			catch(Exception) { Debug.Assert(NativeLib.IsUnix()); }
#endif

			try
			{
				fI32((int)NativeLib.GetPlatformID());
#if KeePassUAP
				fStr(EnvironmentExt.OSVersion.VersionString);
#else
				fStr(Environment.OSVersion.VersionString);
#endif

				fI32(Environment.ProcessorCount);

#if !KeePassUAP
				fStr(Environment.CommandLine);
				fI64(Environment.WorkingSet);
#endif
			}
			catch(Exception) { Debug.Assert(false); }

			try
			{
				foreach(DictionaryEntry de in Environment.GetEnvironmentVariables())
				{
					fStr(de.Key as string);
					fStr(de.Value as string);
				}
			}
			catch(Exception) { Debug.Assert(false); }

			try
			{
#if KeePassUAP
				f(DiagnosticsExt.GetProcessEntropy(), true);
#elif !KeePassLibSD
				using(Process p = Process.GetCurrentProcess())
				{
					fI64(p.Handle.ToInt64());
					fI32(p.HandleCount);
					fI32(p.Id);
					fI64(p.NonpagedSystemMemorySize64);
					fI64(p.PagedMemorySize64);
					fI64(p.PagedSystemMemorySize64);
					fI64(p.PeakPagedMemorySize64);
					fI64(p.PeakVirtualMemorySize64);
					fI64(p.PeakWorkingSet64);
					fI64(p.PrivateMemorySize64);
					fI64(p.StartTime.ToBinary());
					fI64(p.VirtualMemorySize64);
					fI64(p.WorkingSet64);

					// Not supported in Mono 1.2.6:
					// fI32(p.SessionId);
				}
#endif
			}
			catch(Exception) { Debug.Assert(NativeLib.IsUnix()); }

			try
			{
				CultureInfo ci = CultureInfo.CurrentCulture;
				if(ci != null) fI32(ci.GetHashCode());
				else { Debug.Assert(false); }
			}
			catch(Exception) { Debug.Assert(false); }

			f(Guid.NewGuid().ToByteArray(), false);
			f(GetCspRandom(), true);

			h.TransformFinalBlock(MemUtil.EmptyByteArray, 0, 0);
			byte[] pbHash = h.Hash;
			h.Clear();
			MemUtil.ZeroByteArray(pb4);
			MemUtil.ZeroByteArray(pb8);
			return pbHash;
		}

		private byte[] GetCspRandom()
		{
			byte[] pb = new byte[32];

			try { m_rng.GetBytes(pb); }
			catch(Exception)
			{
				Debug.Assert(false);
				MemUtil.Int64ToBytesEx(DateTime.UtcNow.ToBinary(), pb, 0);
			}

			return pb;
		}

		private byte[] GenerateRandom256()
		{
			if(this.GenerateRandom256Pre != null)
				this.GenerateRandom256Pre(this, EventArgs.Empty);

			byte[] pbCmp;
			lock(m_oSyncRoot)
			{
				m_uCounter += 0x74D8B29E4D38E161UL; // Prime number
				byte[] pbCounter = MemUtil.UInt64ToBytes(m_uCounter);

				byte[] pbCsp = GetCspRandom();

				byte[] pbPool = m_pbEntropyPool.ReadData();
				int cbPool = pbPool.Length;
				int cbCtr = pbCounter.Length;
				int cbCsp = pbCsp.Length;

				pbCmp = new byte[cbPool + cbCtr + cbCsp];
				Array.Copy(pbPool, pbCmp, cbPool);
				Array.Copy(pbCounter, 0, pbCmp, cbPool, cbCtr);
				Array.Copy(pbCsp, 0, pbCmp, cbPool + cbCtr, cbCsp);

				MemUtil.ZeroByteArray(pbCsp);
				MemUtil.ZeroByteArray(pbPool);

				m_uGeneratedBytesCount += 32;
			}

			byte[] pbRet = CryptoUtil.HashSha256(pbCmp);
			MemUtil.ZeroByteArray(pbCmp);
			return pbRet;
		}

		/// <summary>
		/// Get a number of cryptographically strong random bytes.
		/// This method is thread-safe.
		/// </summary>
		/// <param name="uRequestedBytes">Number of requested random bytes.</param>
		/// <returns>A byte array consisting of <paramref name="uRequestedBytes" />
		/// random bytes.</returns>
		public byte[] GetRandomBytes(uint uRequestedBytes)
		{
			if(uRequestedBytes == 0) return MemUtil.EmptyByteArray;
			if(uRequestedBytes > (uint)int.MaxValue)
			{
				Debug.Assert(false);
				throw new ArgumentOutOfRangeException("uRequestedBytes");
			}

			int cbRem = (int)uRequestedBytes;
			byte[] pbRes = new byte[cbRem];
			int iPos = 0;

			while(cbRem != 0)
			{
				byte[] pbRandom256 = GenerateRandom256();
				Debug.Assert(pbRandom256.Length == 32);

				int cbCopy = Math.Min(cbRem, pbRandom256.Length);
				Array.Copy(pbRandom256, 0, pbRes, iPos, cbCopy);

				MemUtil.ZeroByteArray(pbRandom256);

				iPos += cbCopy;
				cbRem -= cbCopy;
			}

			Debug.Assert(iPos == pbRes.Length);
			return pbRes;
		}

		private static int g_iWeakSeed = 0;
		public static Random NewWeakRandom()
		{
			long s64 = DateTime.UtcNow.ToBinary();
			int s32 = (int)((s64 >> 32) ^ s64);

			lock(g_oSyncRoot)
			{
				unchecked
				{
					g_iWeakSeed += 0x78A8C4B7; // Prime number
					s32 ^= g_iWeakSeed;
				}
			}

			// Prevent overflow in the Random constructor of .NET 2.0
			if(s32 == int.MinValue) s32 = int.MaxValue;

			return new Random(s32);
		}
	}
}
