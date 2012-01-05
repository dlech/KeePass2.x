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
using System.Diagnostics;

using KeePassLib;

namespace KeePass.Util
{
	/// <summary>
	/// Auto-type candidate context.
	/// </summary>
	public sealed class AutoTypeCtx
	{
		private string m_strSeq = string.Empty;
		public string Sequence
		{
			get { return m_strSeq; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strSeq = value;
			}
		}

		private PwEntry m_pe = null;
		public PwEntry Entry
		{
			get { return m_pe; }
			set { m_pe = value; }
		}

		private PwDatabase m_pd = null;
		public PwDatabase Database
		{
			get { return m_pd; }
			set { m_pd = value; }
		}

		public AutoTypeCtx() { }

		public AutoTypeCtx(string strSequence, PwEntry pe, PwDatabase pd)
		{
			if(strSequence == null) throw new ArgumentNullException("strSequence");

			m_strSeq = strSequence;
			m_pe = pe;
			m_pd = pd;
		}

		public AutoTypeCtx Clone()
		{
			return (AutoTypeCtx)this.MemberwiseClone();
		}
	}
}
