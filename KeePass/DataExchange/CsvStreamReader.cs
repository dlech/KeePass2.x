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
using System.Text;

using KeePassLib.Utility;

namespace KeePass.DataExchange
{
	public sealed class CsvStreamReader
	{
		private readonly CharStream m_sChars;
		private readonly bool m_bAllowUnquoted;

		[Obsolete]
		public CsvStreamReader(string strData) : this(strData, false)
		{
		}

		public CsvStreamReader(string strData, bool bAllowUnquoted)
		{
			m_sChars = new CharStream(strData);
			m_bAllowUnquoted = bAllowUnquoted;
		}

		public string[] ReadLine()
		{
			char chFirst = m_sChars.PeekChar();
			if(chFirst == char.MinValue) return null;
			if((chFirst == '\r') || (chFirst == '\n'))
			{
				m_sChars.ReadChar(); // Advance
				return MemUtil.EmptyArray<string>();
			}

			List<string> l = new List<string>();
			StringBuilder sb = new StringBuilder();
			bool bInField = false;

			while(true)
			{
				char ch = m_sChars.ReadChar();
				if(ch == char.MinValue) break;

				if(ch == '\"')
				{
					if(!bInField) bInField = true;
					else if(m_sChars.PeekChar() == '\"')
					{
						m_sChars.ReadChar();
						sb.Append('\"');
					}
					else
					{
						if(!m_bAllowUnquoted)
						{
							l.Add(sb.ToString());
							if(sb.Length != 0) sb.Remove(0, sb.Length);
						}

						bInField = false;
					}
				}
				else if(((ch == '\r') || (ch == '\n')) && !bInField) break;
				else if(bInField) sb.Append(ch);
				else if(m_bAllowUnquoted)
				{
					if(ch == ',')
					{
						l.Add(sb.ToString());
						if(sb.Length != 0) sb.Remove(0, sb.Length);
					}
					else sb.Append(ch);
				}
			}
			Debug.Assert(!bInField);
			Debug.Assert(m_bAllowUnquoted || (sb.Length == 0));
			if(m_bAllowUnquoted || (sb.Length != 0))
				l.Add(sb.ToString());

			return l.ToArray();
		}
	}
}
