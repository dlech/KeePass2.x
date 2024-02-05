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

using KeePassLib;

namespace KeePass.DataExchange
{
	internal sealed class CsvTableEntryReader : CsvTableObjectReader<PwEntry>
	{
		private readonly PwDatabase m_pd;

		public CsvTableEntryReader(PwDatabase pdContext) :
			base(CsvTableEntryReader.EntryNew, CsvTableEntryReader.EntryCommit(pdContext))
		{
			m_pd = pdContext;
		}

		private static PwEntry EntryNew(string[] vContextRow)
		{
			return new PwEntry(true, true);
		}

		private static CsvTableObjectAction<PwEntry> EntryCommit(PwDatabase pdContext)
		{
			CsvTableObjectAction<PwEntry> f = delegate(PwEntry pe, string[] vContextRow)
			{
				if(pe == null) { Debug.Assert(false); return; }

				if(pe.ParentGroup == null)
				{
					PwGroup pg = ((pdContext != null) ? pdContext.RootGroup : null);
					if(pg != null) pg.AddEntry(pe, true);
					else { Debug.Assert(false); }
				}
			};

			return f;
		}

		public void SetDataAppend(string strColumn, string strStringName)
		{
			if(string.IsNullOrEmpty(strStringName)) { Debug.Assert(false); return; }

			CsvTableDataHandler<PwEntry> f = delegate(string strData,
				PwEntry peContext, string[] vContextRow)
			{
				ImportUtil.AppendToField(peContext, strStringName, strData, m_pd);
			};

			SetDataHandler(strColumn, f);
		}
	}
}
