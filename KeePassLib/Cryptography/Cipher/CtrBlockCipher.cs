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
using System.Text;

using KeePassLib.Utility;

namespace KeePassLib.Cryptography.Cipher
{
	public abstract class CtrBlockCipher : IDisposable
	{
		private readonly byte[] m_pbBlock;
		private int m_iBlockPos;

		private bool m_bDisposed = false;

		public abstract int BlockSize
		{
			get;
		}

		public CtrBlockCipher()
		{
			int cb = this.BlockSize;
			if(cb <= 0) throw new InvalidOperationException("this.BlockSize");

			m_pbBlock = new byte[cb];
			m_iBlockPos = cb;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool bDisposing)
		{
			if(bDisposing)
			{
				MemUtil.ZeroByteArray(m_pbBlock);
				m_iBlockPos = m_pbBlock.Length;

				m_bDisposed = true;
			}
		}

		protected void InvalidateBlock()
		{
			m_iBlockPos = m_pbBlock.Length;
		}

		protected abstract void NextBlock(byte[] pbBlock);

		public void Encrypt(byte[] m, int iOffset, int cb)
		{
			if(m_bDisposed) throw new ObjectDisposedException(null);
			if(m == null) throw new ArgumentNullException("m");
			if(iOffset < 0) throw new ArgumentOutOfRangeException("iOffset");
			if(cb < 0) throw new ArgumentOutOfRangeException("cb");
			if(iOffset > (m.Length - cb)) throw new ArgumentOutOfRangeException("cb");

			int cbBlock = m_pbBlock.Length;

			while(cb > 0)
			{
				Debug.Assert(m_iBlockPos <= cbBlock);
				if(m_iBlockPos == cbBlock)
				{
					NextBlock(m_pbBlock);
					m_iBlockPos = 0;
				}

				int cbCopy = Math.Min(cbBlock - m_iBlockPos, cb);
				Debug.Assert(cbCopy > 0);

				MemUtil.XorArray(m_pbBlock, m_iBlockPos, m, iOffset, cbCopy);

				m_iBlockPos += cbCopy;
				iOffset += cbCopy;
				cb -= cbCopy;
			}
		}

		public void Decrypt(byte[] m, int iOffset, int cb)
		{
			Encrypt(m, iOffset, cb);
		}
	}
}
