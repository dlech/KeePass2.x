/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2012 Dominik Reichl <dominik.reichl@t-online.de>

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

namespace KeePassLib.Serialization
{
	public sealed class BinaryReaderEx : BinaryReader
	{
		private string m_strReadExcp = null;
		public string ReadExceptionText
		{
			get { return m_strReadExcp; }
			set { m_strReadExcp = value; }
		}

		public BinaryReaderEx(Stream input, Encoding encoding,
			string strReadExceptionText) :
			base(input, encoding)
		{
			m_strReadExcp = strReadExceptionText;
		}

		public override byte[] ReadBytes(int count)
		{
			try
			{
				byte[] pb = base.ReadBytes(count);
				if((pb == null) || (pb.Length != count))
				{
					if(m_strReadExcp != null) throw new IOException(m_strReadExcp);
					else throw new EndOfStreamException();
				}

				return pb;
			}
			catch(Exception)
			{
				if(m_strReadExcp != null) throw new IOException(m_strReadExcp);
				else throw;
			}
		}
	}
}
