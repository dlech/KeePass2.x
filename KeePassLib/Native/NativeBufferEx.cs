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
using System.Runtime.InteropServices;
using System.Text;

using KeePassLib.Utility;

namespace KeePassLib.Native
{
	internal sealed class NativeBufferEx : IDisposable
	{
		// private readonly int m_cbMemory;
		private IntPtr m_pbMemory; // Unaligned

		private readonly int m_cbData;
		private IntPtr m_pbData; // Aligned

		private readonly bool m_bZeroOnDispose;

		public IntPtr Data
		{
			get { return m_pbData; }
		}

		public NativeBufferEx(int cbData, bool bZeroOnConstruct,
			bool bZeroOnDispose, bool bThrowExcp, int cbAlignment)
		{
			try
			{
				if(cbData < 0) throw new ArgumentOutOfRangeException("cbData");

				if(cbAlignment == 0) cbAlignment = IntPtr.Size;
				int cbAM1 = cbAlignment - 1;
				if((cbAlignment < 0) || ((cbAlignment & cbAM1) != 0)) // Power of 2
					throw new ArgumentOutOfRangeException("cbAlignment");

				int cb = cbData + cbAM1;
				if(cb < 0) throw new OverflowException();

				IntPtr pb = Marshal.AllocCoTaskMem(cb);
				if(pb == IntPtr.Zero) throw new OutOfMemoryException();

				// m_cbMemory = cb;
				m_pbMemory = pb;
				m_cbData = cbData;
				m_bZeroOnDispose = bZeroOnDispose;

				if(IntPtr.Size >= 8)
				{
					long lAM1 = cbAM1; // '~' on Int64, not on Int32
					m_pbData = new IntPtr((pb.ToInt64() + lAM1) & ~lAM1);
				}
				else
					m_pbData = new IntPtr((pb.ToInt32() + cbAM1) & ~cbAM1);

				if(bZeroOnConstruct) MemUtil.ZeroMemory(m_pbData, cbData);
			}
			catch(Exception)
			{
				Debug.Assert(false);
				Dispose(true);
				if(bThrowExcp) throw;
			}
		}

		public NativeBufferEx(byte[] pbInitialData, bool bZeroOnDispose,
			bool bThrowExcp, int cbAlignment) :
			this(((pbInitialData != null) ? pbInitialData.Length : 0),
				false, bZeroOnDispose, bThrowExcp, cbAlignment)
		{
			try
			{
				if(pbInitialData == null) throw new ArgumentNullException("pbInitialData");

				if(m_pbData != IntPtr.Zero)
					Marshal.Copy(pbInitialData, 0, m_pbData, m_cbData);
			}
			catch(Exception)
			{
				Debug.Assert(false);
				Dispose(true);
				if(bThrowExcp) throw;
			}
		}

		~NativeBufferEx()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool bDisposing)
		{
			if(m_pbData != IntPtr.Zero)
			{
				if(m_bZeroOnDispose) MemUtil.ZeroMemory(m_pbData, m_cbData);
				m_pbData = IntPtr.Zero;
			}

			if(m_pbMemory != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(m_pbMemory);
				m_pbMemory = IntPtr.Zero;
			}
		}

		public void CopyTo(byte[] pb)
		{
			if(pb == null) throw new ArgumentNullException("pb");
			if(pb.Length != m_cbData) throw new ArgumentOutOfRangeException("pb");
			if(m_pbData == IntPtr.Zero) throw new ObjectDisposedException(null);

			Marshal.Copy(m_pbData, pb, 0, m_cbData);
		}
	}
}
