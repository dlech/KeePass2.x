/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2008 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Security;
using System.Security.Cryptography;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;

using KeePassLib.Utility;

namespace KeePassLib.Cryptography
{
	/// <summary>
	/// Cryptographically strong random number generator. The returned values
	/// are unpredictable and cannot be reproduced.
	/// <c>CryptoRandom</c> is a singleton class.
	/// </summary>
	public sealed class CryptoRandom
	{
		private static CryptoRandom m_pInstance = null;

		private byte[] m_pbSystemData = null;
		private byte[] m_pbCspData = null;
		private uint m_uCounter;
		private RNGCryptoServiceProvider m_rng = null;

		public static CryptoRandom Instance
		{
			get
			{
				if(m_pInstance != null) return m_pInstance;

				m_pInstance = new CryptoRandom();
				return m_pInstance;
			}
		}

		private CryptoRandom()
		{
			m_rng = new RNGCryptoServiceProvider();

			Random r = new Random();
			m_uCounter = (uint)r.Next();

			this.GetSystemData(); // System only, not CSP
		}

		private void GetSystemData()
		{
			byte[] pb;
			MemoryStream ms = new MemoryStream();

			pb = MemUtil.UInt32ToBytes((uint)Environment.TickCount);
			ms.Write(pb, 0, pb.Length);

			pb = TimeUtil.PackTime(DateTime.Now);
			ms.Write(pb, 0, pb.Length);

#if !KeePassLibSD
			Point pt = Cursor.Position;
			pb = MemUtil.UInt32ToBytes((uint)pt.X);
			ms.Write(pb, 0, pb.Length);
			pb = MemUtil.UInt32ToBytes((uint)pt.Y);
			ms.Write(pb, 0, pb.Length);
#endif

			Random r = new Random();
			pb = MemUtil.UInt32ToBytes((uint)r.Next());
			ms.Write(pb, 0, pb.Length);

			pb = MemUtil.UInt32ToBytes((uint)Environment.OSVersion.Platform);
			ms.Write(pb, 0, pb.Length);

#if !KeePassLibSD
			try
			{
				pb = MemUtil.UInt32ToBytes((uint)Environment.ProcessorCount);
				ms.Write(pb, 0, pb.Length);
				pb = MemUtil.UInt64ToBytes((ulong)Environment.WorkingSet);
				ms.Write(pb, 0, pb.Length);

				Version v = Environment.OSVersion.Version;
				int nv = (v.Major << 28) + (v.MajorRevision << 24) +
					(v.Minor << 20) + (v.MinorRevision << 16) +
					(v.Revision << 12) + v.Build;
				pb = MemUtil.UInt32ToBytes((uint)nv);
				ms.Write(pb, 0, pb.Length);

				Process p = Process.GetCurrentProcess();
				pb = MemUtil.UInt64ToBytes((ulong)p.Handle.ToInt64());
				ms.Write(pb, 0, pb.Length);
				pb = MemUtil.UInt32ToBytes((uint)p.HandleCount);
				ms.Write(pb, 0, pb.Length);
				pb = MemUtil.UInt32ToBytes((uint)p.Id);
				ms.Write(pb, 0, pb.Length);
				pb = MemUtil.UInt64ToBytes((ulong)p.NonpagedSystemMemorySize64);
				ms.Write(pb, 0, pb.Length);
				pb = MemUtil.UInt64ToBytes((ulong)p.PagedMemorySize64);
				ms.Write(pb, 0, pb.Length);
				pb = MemUtil.UInt64ToBytes((ulong)p.PagedSystemMemorySize64);
				ms.Write(pb, 0, pb.Length);
				pb = MemUtil.UInt64ToBytes((ulong)p.PeakPagedMemorySize64);
				ms.Write(pb, 0, pb.Length);
				pb = MemUtil.UInt64ToBytes((ulong)p.PeakVirtualMemorySize64);
				ms.Write(pb, 0, pb.Length);
				pb = MemUtil.UInt64ToBytes((ulong)p.PeakWorkingSet64);
				ms.Write(pb, 0, pb.Length);
				pb = MemUtil.UInt64ToBytes((ulong)p.PrivateMemorySize64);
				ms.Write(pb, 0, pb.Length);
				pb = MemUtil.UInt64ToBytes((ulong)p.StartTime.ToBinary());
				ms.Write(pb, 0, pb.Length);
				pb = MemUtil.UInt64ToBytes((ulong)p.VirtualMemorySize64);
				ms.Write(pb, 0, pb.Length);
				pb = MemUtil.UInt64ToBytes((ulong)p.WorkingSet64);
				ms.Write(pb, 0, pb.Length);

				// Not supported in Mono 1.2.6:
				// pb = MemUtil.UInt32ToBytes((uint)p.SessionId);
				// ms.Write(pb, 0, pb.Length);
			}
			catch(Exception) { }
#endif

			pb = Guid.NewGuid().ToByteArray();
			ms.Write(pb, 0, pb.Length);

			m_pbSystemData = ms.ToArray();
		}

		private void GetCspData()
		{
			m_pbCspData = new byte[32];
			m_rng.GetBytes(m_pbCspData);
		}

		private byte[] GenerateRandom256()
		{
			unchecked { m_uCounter += 13; }
			this.GetCspData(); // CSP only, not system

			byte[] pbCounter = MemUtil.UInt32ToBytes(m_uCounter);

			MemoryStream ms = new MemoryStream();
			ms.Write(m_pbSystemData, 0, m_pbSystemData.Length);
			ms.Write(pbCounter, 0, pbCounter.Length);
			ms.Write(m_pbCspData, 0, m_pbCspData.Length);
			byte[] pbFinal = ms.ToArray();
			Debug.Assert(pbFinal.Length == (m_pbSystemData.Length +
				m_pbCspData.Length + pbCounter.Length));

			SHA256Managed sha256 = new SHA256Managed();
			return sha256.ComputeHash(pbFinal);
		}

		/// <summary>
		/// Get a number of cryptographically strong random bytes.
		/// </summary>
		/// <param name="uRequestedBytes">Number of requested random bytes.</param>
		/// <returns>A byte array consisting of <paramref name="nRequestedBytes" />
		/// random bytes.</returns>
		public byte[] GetRandomBytes(uint uRequestedBytes)
		{
			if(uRequestedBytes == 0) return new byte[0]; // Allow zero-length array

			byte[] pbRes = new byte[uRequestedBytes];
			long lPos = 0;

			while(uRequestedBytes != 0)
			{
				byte[] pbRandom256 = this.GenerateRandom256();
				Debug.Assert(pbRandom256.Length == 32);

				long lCopy = (uint)((uRequestedBytes < 32) ? uRequestedBytes : 32);

#if !KeePassLibSD
				Array.Copy(pbRandom256, 0, pbRes, lPos, lCopy);
#else
				Array.Copy(pbRandom256, 0, pbRes, (int)lPos, (int)lCopy);
#endif

				lPos += lCopy;
				uRequestedBytes -= (uint)lCopy;
			}

			Debug.Assert((int)lPos == pbRes.Length);
			return pbRes;
		}
	}
}
