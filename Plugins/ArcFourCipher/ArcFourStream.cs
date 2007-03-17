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
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;

using KeePassLib.Cryptography;
using KeePassLib.Cryptography.Cipher;

namespace ArcFourCipher
{
	public sealed class ArcFourStream : Stream
	{
		private Stream m_sBaseStream = null;
		private bool m_bEncrypting = true;
		private CryptoRandomStream m_crs = null;

		public override bool CanRead
		{
			get { return !m_bEncrypting; }
		}

		public override bool CanWrite
		{
			get { return m_bEncrypting; }
		}

		public override bool CanSeek
		{
			get { return false; }
		}

		public override long Length
		{
			get { return m_sBaseStream.Length; }
		}

		public override long Position
		{
			get
			{
				return m_sBaseStream.Position;
			}
			set
			{
				throw new NotSupportedException("Setting the position / seeking is not supported.");
			}
		}

		public ArcFourStream(Stream sBaseStream, bool bEncrypting, byte[] pbKey, byte[] pbIV)
		{
			Debug.Assert(sBaseStream != null); if(sBaseStream == null) throw new ArgumentNullException("sBaseStream");
			m_sBaseStream = sBaseStream;

			m_bEncrypting = bEncrypting;

			MemoryStream ms = new MemoryStream();

			if(pbIV != null) ms.Write(pbIV, 0, pbIV.Length); else { Debug.Assert(false); }
			if(pbKey != null) ms.Write(pbKey, 0, pbKey.Length); else { Debug.Assert(false); }

			SHA256Managed sha256 = new SHA256Managed();
			byte[] pbFinalKey = sha256.ComputeHash(ms.ToArray());

			m_crs = new CryptoRandomStream(CrsAlgorithm.ArcFour, pbFinalKey);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if(count <= 0) return 0;

			int nRead = m_sBaseStream.Read(buffer, offset, count);
			if(nRead == 0) return 0;

			byte[] pbRandom = m_crs.GetRandomBytes((uint)nRead);
			for(int iPos = 0; iPos < pbRandom.Length; ++iPos)
				buffer[iPos + offset] ^= pbRandom[iPos];

			return nRead;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if(count <= 0) return;

			byte[] pbEnc = new byte[count];
			byte[] pbRandom = m_crs.GetRandomBytes((uint)count);

			for(int iPos = 0; iPos < count; ++iPos)
				pbEnc[iPos] = (byte)(buffer[iPos + offset] ^ pbRandom[iPos]);

			m_sBaseStream.Write(pbEnc, 0, count);
		}

		public override void Flush()
		{
			m_sBaseStream.Flush();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException("Seeking is not supported.");
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException("Setting the length is not supported.");
		}
	}
}
