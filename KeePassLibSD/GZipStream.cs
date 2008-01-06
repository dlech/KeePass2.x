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
using System.Collections.Generic;
using System.Text;
using System.IO;

using ICSharpCode.SharpZipLib.GZip;

namespace KeePassLibSD
{
	public enum CompressionMode
	{
		Compress = 0,
		Decompress
	}

	public sealed class GZipStream : Stream
	{
		private Stream m_gzStream = null;
		private CompressionMode m_cprMode;

		public GZipStream(Stream sBase, CompressionMode cm)
		{
			if(cm == CompressionMode.Compress)
				m_gzStream = new GZipOutputStream(sBase);
			else
				m_gzStream = new GZipInputStream(sBase);

			m_cprMode = cm;
		}

		public override bool CanRead
		{
			get { return m_gzStream.CanRead; }
		}

		public override bool CanSeek
		{
			get { return m_gzStream.CanSeek; }
		}

		public override bool CanWrite
		{
			get { return m_gzStream.CanWrite; }
		}

		public override long Length
		{
			get { return m_gzStream.Length; }
		}

		public override long Position
		{
			get
			{
				return m_gzStream.Position;
			}
			set
			{
				m_gzStream.Position = value;
			}
		}

		public override void Flush()
		{
			m_gzStream.Flush();
		}

		public override void Close()
		{
			if(m_gzStream == null) return;

			if(m_cprMode == CompressionMode.Compress)
			{
				m_gzStream.Flush();
				((GZipOutputStream)m_gzStream).Finish();
			}

			m_gzStream.Close();
			m_gzStream = null;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return m_gzStream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			m_gzStream.SetLength(value);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return m_gzStream.Read(buffer, offset, count);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			m_gzStream.Write(buffer, offset, count);
		}
	}
}
