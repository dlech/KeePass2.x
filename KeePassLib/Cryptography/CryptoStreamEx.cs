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
using System.IO;
using System.Text;

#if !KeePassUAP
using System.Security.Cryptography;

namespace KeePassLib.Cryptography
{
	public sealed class CryptoStreamEx : CryptoStream
	{
		private ICryptoTransform m_t;
		private SymmetricAlgorithm m_a;

		public CryptoStreamEx(Stream s, ICryptoTransform t, CryptoStreamMode m,
			SymmetricAlgorithm a) : base(s, t, m)
		{
			m_t = t;
			m_a = a;
		}

		protected override void Dispose(bool disposing)
		{
			try { base.Dispose(disposing); }
			// Unnecessary exception from CryptoStream with
			// RijndaelManagedTransform when a stream hasn't been
			// read completely (e.g. incorrect master key)
			catch(CryptographicException) { }
			// Similar to above, at the beginning of the stream
			catch(IndexOutOfRangeException) { }
			catch(Exception) { Debug.Assert(false); }

			if(disposing)
			{
				try { if(m_t != null) { m_t.Dispose(); m_t = null; } }
				catch(Exception) { Debug.Assert(false); }

				// In .NET 2.0, SymmetricAlgorithm.Dispose() is not public
				try { if(m_a != null) { m_a.Clear(); m_a = null; } }
				catch(Exception) { Debug.Assert(false); }
			}
		}
	}
}
#endif
