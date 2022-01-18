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
using System.Text;

namespace KeePass.DataExchange
{
	internal delegate T CsvTableObjectFunc<T>(string[] vContextRow);
	internal delegate void CsvTableObjectAction<T>(T t, string[] vContextRow);
	internal delegate void CsvTableDataHandler<T>(string strData, T tContext,
		string[] vContextRow);

	internal abstract class CsvTableObjectReader<T>
	{
		// For all streams
		private readonly CsvTableObjectFunc<T> m_fObjectNew;
		private readonly CsvTableObjectAction<T> m_fObjectCommit;
		private readonly Dictionary<string, CsvTableDataHandler<T>> m_dHandlers =
			new Dictionary<string, CsvTableDataHandler<T>>();

		// For the current stream
		private readonly Dictionary<string, int> m_dColumns =
			new Dictionary<string, int>();

		public CsvTableObjectReader(CsvTableObjectFunc<T> fObjectNew,
			CsvTableObjectAction<T> fObjectCommit)
		{
			m_fObjectNew = fObjectNew;
			m_fObjectCommit = fObjectCommit;
		}

		public void SetDataHandler(string strColumn, CsvTableDataHandler<T> f)
		{
			if(strColumn == null) { Debug.Assert(false); return; }

			m_dHandlers[strColumn] = f;
		}

		public void Read(CsvStreamReaderEx csr)
		{
			if(csr == null) { Debug.Assert(false); return; }

			string[] vHeader;
			while(true)
			{
				vHeader = csr.ReadLine();
				if(vHeader == null) return;
				if(vHeader.Length == 0) continue;
				if((vHeader.Length == 1) && (vHeader[0].Length == 0)) continue;
				break;
			}

			m_dColumns.Clear();

			CsvTableDataHandler<T>[] vHandlers = new CsvTableDataHandler<T>[vHeader.Length];
			for(int i = vHeader.Length - 1; i >= 0; --i)
			{
				string str = vHeader[i];

				m_dColumns[str] = i;

				CsvTableDataHandler<T> f;
				m_dHandlers.TryGetValue(str, out f);
				vHandlers[i] = f;
			}

			while(true)
			{
				string[] vRow = csr.ReadLine();
				if(vRow == null) break;
				if(vRow.Length == 0) continue;
				if((vRow.Length == 1) && (vRow[0].Length == 0)) continue;

				T t = ((m_fObjectNew != null) ? m_fObjectNew(vRow) : default(T));

				Debug.Assert(vRow.Length == vHandlers.Length);
				int m = Math.Min(vRow.Length, vHandlers.Length);
				for(int i = 0; i < m; ++i)
				{
					CsvTableDataHandler<T> f = vHandlers[i];
					if(f != null) f(vRow[i], t, vRow);
				}

				if(m_fObjectCommit != null) m_fObjectCommit(t, vRow);
			}
		}

		public string GetData(string[] vRow, string strColumn, string strDefault)
		{
			if(vRow == null) { Debug.Assert(false); return strDefault; }
			if(strColumn == null) { Debug.Assert(false); return strDefault; }

			int i;
			if(!m_dColumns.TryGetValue(strColumn, out i)) return strDefault;

			return ((i < vRow.Length) ? vRow[i] : strDefault);
		}
	}
}
