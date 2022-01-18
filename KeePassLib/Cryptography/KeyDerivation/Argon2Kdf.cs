/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2022 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePassLib.Native;
using KeePassLib.Utility;

namespace KeePassLib.Cryptography.KeyDerivation
{
	public enum Argon2Type
	{
		// The values must be the same as in the Argon2 specification
		D = 0,
		ID = 2
	}

	public sealed partial class Argon2Kdf : KdfEngine
	{
		private static readonly PwUuid g_uuidD = new PwUuid(new byte[] {
			0xEF, 0x63, 0x6D, 0xDF, 0x8C, 0x29, 0x44, 0x4B,
			0x91, 0xF7, 0xA9, 0xA4, 0x03, 0xE3, 0x0A, 0x0C });
		private static readonly PwUuid g_uuidID = new PwUuid(new byte[] {
			0x9E, 0x29, 0x8B, 0x19, 0x56, 0xDB, 0x47, 0x73,
			0xB2, 0x3D, 0xFC, 0x3E, 0xC6, 0xF0, 0xA1, 0xE6 });

		public static readonly string ParamSalt = "S"; // Byte[]
		public static readonly string ParamParallelism = "P"; // UInt32
		public static readonly string ParamMemory = "M"; // UInt64
		public static readonly string ParamIterations = "I"; // UInt64
		public static readonly string ParamVersion = "V"; // UInt32
		public static readonly string ParamSecretKey = "K"; // Byte[]
		public static readonly string ParamAssocData = "A"; // Byte[]

		private const uint MinVersion = 0x10;
		private const uint MaxVersion = 0x13;

		private const int MinSalt = 8;
		private const int MaxSalt = int.MaxValue; // .NET limit; 2^32 - 1 in spec.

		internal const ulong MinIterations = 1;
		internal const ulong MaxIterations = uint.MaxValue;

		internal const ulong MinMemory = 1024 * 8; // For parallelism = 1
		// internal const ulong MaxMemory = (ulong)uint.MaxValue * 1024UL; // Spec.
		internal const ulong MaxMemory = int.MaxValue; // .NET limit

		internal const uint MinParallelism = 1;
		internal const uint MaxParallelism = (1 << 24) - 1;

		internal const ulong DefaultIterations = 2;
		internal const ulong DefaultMemory = 64 * 1024 * 1024; // 64 MB
		internal const uint DefaultParallelism = 2;

		private readonly Argon2Type m_t;

		public override PwUuid Uuid
		{
			get { return ((m_t == Argon2Type.D) ? g_uuidD : g_uuidID); }
		}

		public override string Name
		{
			get { return ((m_t == Argon2Type.D) ? "Argon2d" : "Argon2id"); }
		}

		public Argon2Kdf() : this(Argon2Type.D)
		{
		}

		public Argon2Kdf(Argon2Type t)
		{
			if((t != Argon2Type.D) && (t != Argon2Type.ID))
				throw new NotSupportedException();

			m_t = t;
		}

		public override KdfParameters GetDefaultParameters()
		{
			KdfParameters p = base.GetDefaultParameters();

			p.SetUInt32(ParamVersion, MaxVersion);

			p.SetUInt64(ParamIterations, DefaultIterations);
			p.SetUInt64(ParamMemory, DefaultMemory);
			p.SetUInt32(ParamParallelism, DefaultParallelism);

			return p;
		}

		public override void Randomize(KdfParameters p)
		{
			if(p == null) { Debug.Assert(false); return; }
			Debug.Assert(p.KdfUuid.Equals(this.Uuid));

			byte[] pb = CryptoRandom.Instance.GetRandomBytes(32);
			p.SetByteArray(ParamSalt, pb);
		}

		public override byte[] Transform(byte[] pbMsg, KdfParameters p)
		{
			if(pbMsg == null) throw new ArgumentNullException("pbMsg");
			if(p == null) throw new ArgumentNullException("p");

			byte[] pbSalt = p.GetByteArray(ParamSalt);
			if(pbSalt == null)
				throw new ArgumentNullException("p.Salt");
			if((pbSalt.Length < MinSalt) || (pbSalt.Length > MaxSalt))
				throw new ArgumentOutOfRangeException("p.Salt");

			uint uPar = p.GetUInt32(ParamParallelism, 0);
			if((uPar < MinParallelism) || (uPar > MaxParallelism))
				throw new ArgumentOutOfRangeException("p.Parallelism");

			ulong uMem = p.GetUInt64(ParamMemory, 0);
			if((uMem < MinMemory) || (uMem > MaxMemory))
				throw new ArgumentOutOfRangeException("p.Memory");

			ulong uIt = p.GetUInt64(ParamIterations, 0);
			if((uIt < MinIterations) || (uIt > MaxIterations))
				throw new ArgumentOutOfRangeException("p.Iterations");

			uint v = p.GetUInt32(ParamVersion, 0);
			if((v < MinVersion) || (v > MaxVersion))
				throw new ArgumentOutOfRangeException("p.Version");

			byte[] pbSecretKey = p.GetByteArray(ParamSecretKey);
			byte[] pbAssocData = p.GetByteArray(ParamAssocData);

			const int cbOut = 32;

			byte[] pbRet = Argon2Native(pbMsg, pbSalt, uPar, uMem,
				uIt, cbOut, v, pbSecretKey, pbAssocData);

			if(pbRet == null)
			{
				pbRet = Argon2Transform(pbMsg, pbSalt, uPar, uMem,
					uIt, cbOut, v, pbSecretKey, pbAssocData);

				if(uMem > (100UL * 1024UL * 1024UL)) GC.Collect();
			}

			return pbRet;
		}

		public override KdfParameters GetBestParameters(uint uMilliseconds)
		{
			KdfParameters p = GetDefaultParameters();
			Randomize(p);

			MaximizeParamUInt64(p, ParamIterations, MinIterations,
				MaxIterations, uMilliseconds, true);
			return p;
		}

		private byte[] Argon2Native(byte[] pbMsg, byte[] pbSalt, uint uParallel,
			ulong uMem, ulong uIt, int cbOut, uint uVersion, byte[] pbSecretKey,
			byte[] pbAssocData)
		{
			NativeBufferEx nbMsg = null, nbSalt = null, nbHash = null;
			try
			{
				// Secret key and assoc. data are unsupported by 'argon2_hash'
				if((pbSecretKey != null) && (pbSecretKey.Length != 0)) return null;
				if((pbAssocData != null) && (pbAssocData.Length != 0)) return null;

				int iType;
				if(m_t == Argon2Type.D) iType = 0;
				else if(m_t == Argon2Type.ID) iType = 2;
				else { Debug.Assert(false); return null; }

				nbMsg = new NativeBufferEx(pbMsg, true, true, 1);
				IntPtr cbMsg = new IntPtr(pbMsg.Length);
				nbSalt = new NativeBufferEx(pbSalt, true, true, 1);
				IntPtr cbSalt = new IntPtr(pbSalt.Length);
				uint m = checked((uint)(uMem / NbBlockSize));
				uint t = checked((uint)uIt);
				nbHash = new NativeBufferEx(cbOut, true, true, true, 1);
				IntPtr cbHash = new IntPtr(cbOut);

				bool b = false;
				if(NativeLib.IsUnix())
				{
					if(!MonoWorkarounds.IsRequired(100004)) return null;

					try
					{
						b = (NativeMethods.argon2_hash_u0(t, m, uParallel,
							nbMsg.Data, cbMsg, nbSalt.Data, cbSalt,
							nbHash.Data, cbHash, IntPtr.Zero, IntPtr.Zero,
							iType, uVersion) == 0);
					}
					catch(DllNotFoundException) { }
					catch(Exception) { Debug.Assert(false); }

					if(!b)
						b = (NativeMethods.argon2_hash_u1(t, m, uParallel,
							nbMsg.Data, cbMsg, nbSalt.Data, cbSalt,
							nbHash.Data, cbHash, IntPtr.Zero, IntPtr.Zero,
							iType, uVersion) == 0);
				}
				else // Windows
				{
					if(IntPtr.Size == 4)
						b = (NativeMethods.argon2_hash_w32(t, m, uParallel,
							nbMsg.Data, cbMsg, nbSalt.Data, cbSalt,
							nbHash.Data, cbHash, IntPtr.Zero, IntPtr.Zero,
							iType, uVersion) == 0);
					else
						b = (NativeMethods.argon2_hash_w64(t, m, uParallel,
							nbMsg.Data, cbMsg, nbSalt.Data, cbSalt,
							nbHash.Data, cbHash, IntPtr.Zero, IntPtr.Zero,
							iType, uVersion) == 0);
				}

				if(b)
				{
					byte[] pbHash = new byte[cbOut];
					nbHash.CopyTo(pbHash);
					return pbHash;
				}
			}
			catch(DllNotFoundException) { }
			catch(Exception) { Debug.Assert(false); }
			finally
			{
				if(nbMsg != null) nbMsg.Dispose();
				if(nbSalt != null) nbSalt.Dispose();
				if(nbHash != null) nbHash.Dispose();
			}

			return null;
		}
	}
}
