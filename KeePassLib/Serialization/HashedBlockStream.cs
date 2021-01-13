/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2021 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.IO;
using System.Text;

using KeePassLib.Cryptography;
using KeePassLib.Native;
using KeePassLib.Utility;

#if KeePassLibSD
using KeePassLibSD;
#endif

namespace KeePassLib.Serialization
{
	public sealed class HashedBlockStream : Stream
	{
		private const int NbDefaultBufferSize = 1024 * 1024; // 1 MB

		private Stream m_sBase;
		private readonly bool m_bWriting;
		private readonly bool m_bVerify;

		private BinaryReader m_brInput = null;
		private BinaryWriter m_bwOutput = null;

		private bool m_bEos = false;
		private byte[] m_pbBuffer;
		private int m_iBufferPos = 0;

		private uint m_uBlockIndex = 0;

		public override bool CanRead
		{
			get { return !m_bWriting; }
		}

		public override bool CanSeek
		{
			get { return false; }
		}

		public override bool CanWrite
		{
			get { return m_bWriting; }
		}

		public override long Length
		{
			get { Debug.Assert(false); throw new NotSupportedException(); }
		}

		public override long Position
		{
			get { Debug.Assert(false); throw new NotSupportedException(); }
			set { Debug.Assert(false); throw new NotSupportedException(); }
		}

		public HashedBlockStream(Stream sBase, bool bWriting) :
			this(sBase, bWriting, 0, true)
		{
		}

		public HashedBlockStream(Stream sBase, bool bWriting, int nBufferSize) :
			this(sBase, bWriting, nBufferSize, true)
		{
		}

		public HashedBlockStream(Stream sBase, bool bWriting, int nBufferSize,
			bool bVerify)
		{
			if(sBase == null) throw new ArgumentNullException("sBase");
			if(nBufferSize < 0) throw new ArgumentOutOfRangeException("nBufferSize");

			if(nBufferSize == 0) nBufferSize = NbDefaultBufferSize;

			m_sBase = sBase;
			m_bWriting = bWriting;
			m_bVerify = bVerify;

			UTF8Encoding utf8 = StrUtil.Utf8;
			if(!m_bWriting) // Reading mode
			{
				if(!m_sBase.CanRead) throw new InvalidOperationException();

				m_brInput = new BinaryReader(m_sBase, utf8);

				m_pbBuffer = MemUtil.EmptyByteArray;
			}
			else // Writing mode
			{
				if(!m_sBase.CanWrite) throw new InvalidOperationException();

				m_bwOutput = new BinaryWriter(m_sBase, utf8);

				m_pbBuffer = new byte[nBufferSize];
			}
		}

		protected override void Dispose(bool disposing)
		{
			if(disposing && (m_sBase != null))
			{
				if(!m_bWriting) // Reading mode
				{
					m_brInput.Close();
					m_brInput = null;
				}
				else // Writing mode
				{
					if(m_iBufferPos == 0) // No data left in buffer
						WriteSafeBlock(); // Write terminating block
					else
					{
						WriteSafeBlock(); // Write remaining buffered data
						WriteSafeBlock(); // Write terminating block
					}

					Flush();
					m_bwOutput.Close();
					m_bwOutput = null;
				}

				m_sBase.Close();
				m_sBase = null;
			}

			SetBuffer(MemUtil.EmptyByteArray);

			base.Dispose(disposing);
		}

		private void SetBuffer(byte[] pb)
		{
			MemUtil.ZeroByteArray(m_pbBuffer); // Erase previous buffer

			m_pbBuffer = pb;
		}

		public override void Flush()
		{
			Debug.Assert(m_sBase != null); // Object should not be disposed
			if(m_bWriting && (m_bwOutput != null)) m_bwOutput.Flush();
		}

		public override long Seek(long lOffset, SeekOrigin soOrigin)
		{
			Debug.Assert(false);
			throw new NotSupportedException();
		}

		public override void SetLength(long lValue)
		{
			Debug.Assert(false);
			throw new NotSupportedException();
		}

		public override int Read(byte[] pbBuffer, int iOffset, int nCount)
		{
			if(m_bWriting) throw new InvalidOperationException();

			int nRemaining = nCount;
			while(nRemaining > 0)
			{
				if(m_iBufferPos == m_pbBuffer.Length)
				{
					if(!ReadSafeBlock())
						return (nCount - nRemaining); // Bytes actually read
				}

				int nCopy = Math.Min(m_pbBuffer.Length - m_iBufferPos, nRemaining);
				Debug.Assert(nCopy > 0);

				Array.Copy(m_pbBuffer, m_iBufferPos, pbBuffer, iOffset, nCopy);

				iOffset += nCopy;
				m_iBufferPos += nCopy;

				nRemaining -= nCopy;
			}

			return nCount;
		}

		private bool ReadSafeBlock()
		{
			if(m_bEos) return false; // End of stream reached already

			m_iBufferPos = 0;

			if(m_brInput.ReadUInt32() != m_uBlockIndex)
				throw new InvalidDataException();
			++m_uBlockIndex;

			byte[] pbStoredHash = m_brInput.ReadBytes(32);
			if((pbStoredHash == null) || (pbStoredHash.Length != 32))
				throw new InvalidDataException();

			int nBufferSize = 0;
			try { nBufferSize = m_brInput.ReadInt32(); }
			catch(NullReferenceException) // Mono bug workaround (LaunchPad 783268)
			{
				if(!NativeLib.IsUnix()) throw;
			}

			if(nBufferSize < 0)
				throw new InvalidDataException();

			if(nBufferSize == 0)
			{
				for(int iHash = 0; iHash < 32; ++iHash)
				{
					if(pbStoredHash[iHash] != 0)
						throw new InvalidDataException();
				}

				m_bEos = true;
				SetBuffer(MemUtil.EmptyByteArray);
				return false;
			}

			SetBuffer(m_brInput.ReadBytes(nBufferSize));
			if((m_pbBuffer == null) || ((m_pbBuffer.Length != nBufferSize) && m_bVerify))
				throw new InvalidDataException();

			if(m_bVerify)
			{
				byte[] pbComputedHash = CryptoUtil.HashSha256(m_pbBuffer);
				if((pbComputedHash == null) || (pbComputedHash.Length != 32))
					throw new InvalidOperationException();

				if(!MemUtil.ArraysEqual(pbStoredHash, pbComputedHash))
					throw new InvalidDataException();
			}

			return true;
		}

		public override void Write(byte[] pbBuffer, int iOffset, int nCount)
		{
			if(!m_bWriting) throw new InvalidOperationException();

			while(nCount > 0)
			{
				if(m_iBufferPos == m_pbBuffer.Length)
					WriteSafeBlock();

				int nCopy = Math.Min(m_pbBuffer.Length - m_iBufferPos, nCount);
				Debug.Assert(nCopy > 0);

				Array.Copy(pbBuffer, iOffset, m_pbBuffer, m_iBufferPos, nCopy);

				iOffset += nCopy;
				m_iBufferPos += nCopy;

				nCount -= nCopy;
			}
		}

		private void WriteSafeBlock()
		{
			m_bwOutput.Write(m_uBlockIndex);
			++m_uBlockIndex;

			if(m_iBufferPos != 0)
			{
				byte[] pbHash = CryptoUtil.HashSha256(m_pbBuffer, 0, m_iBufferPos);

				// For KeePassLibSD:
				// SHA256Managed sha256 = new SHA256Managed();
				// byte[] pbHash;
				// if(m_iBufferPos == m_pbBuffer.Length)
				//	pbHash = sha256.ComputeHash(m_pbBuffer);
				// else
				// {
				//	byte[] pbData = new byte[m_iBufferPos];
				//	Array.Copy(m_pbBuffer, 0, pbData, 0, m_iBufferPos);
				//	pbHash = sha256.ComputeHash(pbData);
				// }

				m_bwOutput.Write(pbHash);
			}
			else
			{
				m_bwOutput.Write((ulong)0); // Zero hash
				m_bwOutput.Write((ulong)0);
				m_bwOutput.Write((ulong)0);
				m_bwOutput.Write((ulong)0);
			}

			m_bwOutput.Write(m_iBufferPos);
			
			if(m_iBufferPos != 0)
				m_bwOutput.Write(m_pbBuffer, 0, m_iBufferPos);

			m_iBufferPos = 0;
		}
	}
}
