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
using System.IO;
using System.Text;

using KeePassLib.Utility;

namespace KeePass.DataExchange
{
	// Compatible with RFC 4180
	internal sealed class CsvStreamWriter
	{
		private readonly StreamWriter m_sw;

		public CsvStreamWriter(StreamWriter sw)
		{
			if(sw == null) throw new ArgumentNullException("sw");

			m_sw = sw;
		}

		private static readonly char[] g_vQuReq = new char[] {
			'\t', '\n', '\r', '\"', ',', ';' };
		private static bool AreQuotesRequired(string str)
		{
			if(str == null) { Debug.Assert(false); return false; }

			int cc = str.Length;
			if(cc == 0) return false;

			if(char.IsWhiteSpace(str[0]) || char.IsWhiteSpace(str[cc - 1]))
				return true;

			return (str.IndexOfAny(g_vQuReq) >= 0);
		}

		public void WriteLine(string[] vFields)
		{
			if(vFields == null) { Debug.Assert(false); return; }

			for(int i = 0; i < vFields.Length; ++i)
			{
				if(i != 0) m_sw.Write(',');

				string str = vFields[i];
				Debug.Assert(str != null);
				if(!string.IsNullOrEmpty(str))
				{
					if(AreQuotesRequired(str))
					{
						m_sw.Write('\"');

						string strEsc = StrUtil.NormalizeNewLines(str, true);
						strEsc = strEsc.Replace("\"", "\"\"");
						m_sw.Write(strEsc);

						m_sw.Write('\"');
					}
					else m_sw.Write(str);
				}
			}

			m_sw.Write("\r\n"); // m_sw.WriteLine uses m_sw.NewLine
		}
	}
}
