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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

#if KeePassUAP
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
#else
using System.Security.Cryptography;
#endif

using KeePassLib.Native;
using KeePassLib.Utility;

namespace KeePassLib.Cryptography.KeyDerivation
{
	public sealed partial class AesKdf : KdfEngine
	{
		private static readonly PwUuid g_uuid = new PwUuid(new byte[] {
			0xC9, 0xD9, 0xF3, 0x9A, 0x62, 0x8A, 0x44, 0x60,
			0xBF, 0x74, 0x0D, 0x08, 0xC1, 0x8A, 0x4F, 0xEA });

		public static readonly string ParamSeed = "S"; // Byte[32]
		public static readonly string ParamRounds = "R"; // UInt64

		private const int AkcBufferBlocks = 8192;

		public override PwUuid Uuid
		{
			get { return g_uuid; }
		}

		public override string Name
		{
			get { return "AES-KDF"; }
		}

		public AesKdf()
		{
		}

		public override KdfParameters GetDefaultParameters()
		{
			KdfParameters p = base.GetDefaultParameters();
			p.SetUInt64(ParamRounds, PwDefs.DefaultKeyEncryptionRounds);
			return p;
		}

		public override bool AreParametersWeak(KdfParameters p)
		{
			if(p == null) { Debug.Assert(false); return false; }

			ulong uRounds = p.GetUInt64(ParamRounds, ulong.MaxValue);
			return (uRounds < PwDefs.DefaultKeyEncryptionRounds);
		}

		public override void Randomize(KdfParameters p)
		{
			if(p == null) { Debug.Assert(false); return; }
			Debug.Assert(g_uuid.Equals(p.KdfUuid));

			byte[] pbSeed = CryptoRandom.Instance.GetRandomBytes(32);
			p.SetByteArray(ParamSeed, pbSeed);
		}

		public override byte[] Transform(byte[] pbMsg, KdfParameters p)
		{
			if(pbMsg == null) throw new ArgumentNullException("pbMsg");
			if(p == null) throw new ArgumentNullException("p");

			Type tRounds = p.GetTypeOf(ParamRounds);
			if(tRounds == null) throw new ArgumentNullException("p.Rounds");
			if(tRounds != typeof(ulong)) throw new ArgumentOutOfRangeException("p.Rounds");
			ulong uRounds = p.GetUInt64(ParamRounds, 0);

			byte[] pbSeed = p.GetByteArray(ParamSeed);
			if(pbSeed == null) throw new ArgumentNullException("p.Seed");

			if(pbMsg.Length != 32)
			{
				Debug.Assert(false);
				pbMsg = CryptoUtil.HashSha256(pbMsg);
			}

			if(pbSeed.Length != 32)
			{
				Debug.Assert(false);
				pbSeed = CryptoUtil.HashSha256(pbSeed);
			}

			return TransformKey(pbMsg, pbSeed, uRounds);
		}

		private static byte[] TransformKey(byte[] pbData32, byte[] pbSeed32,
			ulong uRounds)
		{
			if(pbData32 == null) throw new ArgumentNullException("pbData32");
			if(pbData32.Length != 32) throw new ArgumentOutOfRangeException("pbData32");
			if(pbSeed32 == null) throw new ArgumentNullException("pbSeed32");
			if(pbSeed32.Length != 32) throw new ArgumentOutOfRangeException("pbSeed32");

			byte[] pbTrf32 = new byte[32];
			Array.Copy(pbData32, pbTrf32, 32);

			try
			{
				if(NativeLib.TransformKey256(pbTrf32, pbSeed32, uRounds))
					return CryptoUtil.HashSha256(pbTrf32);

				if(TransformKeyGCrypt(pbTrf32, pbSeed32, uRounds))
					return CryptoUtil.HashSha256(pbTrf32);

				TransformKeyManaged(pbTrf32, pbSeed32, uRounds);
				return CryptoUtil.HashSha256(pbTrf32);
			}
			finally { MemUtil.ZeroByteArray(pbTrf32); }
		}

		internal static void TransformKeyManaged(byte[] pbData32, byte[] pbSeed32,
			ulong uRounds)
		{
			if(uRounds == 0) return;

#if KeePassUAP
			KeyParameter kp = new KeyParameter(pbSeed32);
			AesEngine aes = new AesEngine();
			aes.Init(true, kp);

			for(ulong u = 0; u < uRounds; ++u)
			{
				aes.ProcessBlock(pbData32, 0, pbData32, 0);
				aes.ProcessBlock(pbData32, 16, pbData32, 16);
			}

			aes.Reset();
#else
			byte[] pbDataL = null, pbDataR = null;
			try
			{
				pbDataL = MemUtil.Mid(pbData32, 0, 16);
				pbDataR = MemUtil.Mid(pbData32, 16, 16);

				Exception exL = null, exR = null;

				Thread thL = new Thread(new ThreadStart(() => { exL =
					TransformKeyManagedHalf(pbDataL, pbSeed32, uRounds); }));
				Thread thR = new Thread(new ThreadStart(() => { exR =
					TransformKeyManagedHalf(pbDataR, pbSeed32, uRounds); }));
				thL.Start();
				thR.Start();
				thL.Join();
				thR.Join();

				if(exL != null) throw new Exception(null, exL);
				if(exR != null) throw new Exception(null, exR);

				Array.Copy(pbDataL, 0, pbData32, 0, 16);
				Array.Copy(pbDataR, 0, pbData32, 16, 16);
			}
			finally
			{
				MemUtil.ZeroByteArray(pbDataL);
				MemUtil.ZeroByteArray(pbDataR);
			}
#endif
		}

		private static Exception TransformKeyManagedHalf(byte[] pbData16,
			byte[] pbSeed32, ulong uRounds)
		{
			byte[] pbBuf = null;
			try
			{
				byte[] pbZero = new byte[AkcBufferBlocks * 16];
				pbBuf = new byte[AkcBufferBlocks * 16];

				using(SymmetricAlgorithm a = CryptoUtil.CreateAes(256, CipherMode.CBC,
					PaddingMode.None))
				{
					using(ICryptoTransform t = a.CreateEncryptor(pbSeed32, pbData16))
					{
						while(true)
						{
							ulong cBlocks = Math.Min(uRounds, (ulong)AkcBufferBlocks);
							int cBytes = (int)cBlocks << 4;

							t.TransformBlock(pbZero, 0, cBytes, pbBuf, 0);

							uRounds -= cBlocks;
							if(uRounds == 0)
							{
								if(cBytes == 0) { Debug.Assert(false); }
								else Array.Copy(pbBuf, cBytes - 16, pbData16, 0, 16);

								break;
							}
						}
					}
				}
			}
			catch(Exception ex) { Debug.Assert(false); return ex; }
			finally { MemUtil.ZeroByteArray(pbBuf); }

			return null;
		}

		public override KdfParameters GetBestParameters(uint uMilliseconds)
		{
			ulong uRounds;

			if(NativeLib.TransformKeyBenchmark256(uMilliseconds, out uRounds)) { }
			else if(TransformKeyGCryptBenchmark(uMilliseconds, out uRounds)) { }
			else
			{
				Exception exL = null, exR = null;
				ulong uRoundsL = 0, uRoundsR = 0;

				Thread thL = new Thread(new ThreadStart(() => { exL =
					TransformKeyManagedBenchmarkHalf(uMilliseconds, out uRoundsL); }));
				Thread thR = new Thread(new ThreadStart(() => { exR =
					TransformKeyManagedBenchmarkHalf(uMilliseconds, out uRoundsR); }));
				thL.Start();
				thR.Start();
				thL.Join();
				thR.Join();

				if(exL != null) throw new Exception(null, exL);
				if(exR != null) throw new Exception(null, exR);

				uRounds = (uRoundsL >> 1) + (uRoundsR >> 1);
			}

			KdfParameters p = GetDefaultParameters();
			if(uRounds != 0) p.SetUInt64(ParamRounds, uRounds);
			else { Debug.Assert(false); }
			return p;
		}

		private static Exception TransformKeyManagedBenchmarkHalf(uint uMilliseconds,
			out ulong uRounds)
		{
			uRounds = 0;

			try
			{
				const ulong cBlocks = AkcBufferBlocks;
				const int cBytes = checked((int)cBlocks * 16);

				byte[] pbData16 = new byte[16];
				byte[] pbSeed32 = new byte[32];
				byte[] pbZero = new byte[cBytes];
				byte[] pbBuf = new byte[cBytes];

				Random r = CryptoRandom.NewWeakRandom();
				r.NextBytes(pbData16);
				r.NextBytes(pbSeed32);

				using(SymmetricAlgorithm a = CryptoUtil.CreateAes(256, CipherMode.CBC,
					PaddingMode.None))
				{
					using(ICryptoTransform t = a.CreateEncryptor(pbSeed32, pbData16))
					{
						int tStart = Environment.TickCount;
						while((uint)(Environment.TickCount - tStart) <= uMilliseconds)
						{
							t.TransformBlock(pbZero, 0, cBytes, pbBuf, 0);

							uRounds += cBlocks;
							if(uRounds < cBlocks) // Overflow check
							{
								uRounds = ulong.MaxValue;
								break;
							}
						}
					}
				}
			}
			catch(Exception ex) { Debug.Assert(false); return ex; }

			return null;
		}
	}
}
