/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2026 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePass.Resources;
using KeePass.Util.Spr;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Utility;

namespace KeePass.Util
{
	public static class SearchUtil
	{
		internal const string StrTrfDeref = "Deref";

		internal static void PrepareSerialize(SearchParameters sp)
		{
			if(sp == null) { Debug.Assert(false); return; }

			AdjustCulture(sp, false);

			sp.DataTransformation = GetTransformation(sp);
		}

		internal static void FinishDeserialize(SearchParameters sp)
		{
			if(sp == null) { Debug.Assert(false); return; }

			AdjustCulture(sp, true);

			SetTransformation(sp, sp.DataTransformation);
		}

		private static void AdjustCulture(SearchParameters sp, bool bDeserializing)
		{
			// The SearchForm class currently only supports the current culture

			StringComparison sc = sp.ComparisonMode;
			Debug.Assert(bDeserializing || (sc == StringComparison.CurrentCulture) ||
				(sc == StringComparison.CurrentCultureIgnoreCase));

			sp.ComparisonMode = (StrUtil.GetIgnoreCase(sc) ?
				StringComparison.CurrentCultureIgnoreCase :
				StringComparison.CurrentCulture);
		}

		internal static string GetTransformation(SearchParameters spIn)
		{
			if(spIn == null) { Debug.Assert(false); return string.Empty; }

			if(spIn.DataTransformationFn == null) return string.Empty;
			return StrTrfDeref;
		}

		internal static void SetTransformation(SearchParameters spOut,
			string strTrf)
		{
			if(spOut == null) { Debug.Assert(false); return; }
			if(strTrf == null) { Debug.Assert(false); return; }

			if(strTrf == StrTrfDeref)
				spOut.DataTransformationFn = SprEngine.DerefFn;
			else spOut.DataTransformationFn = null;
		}

		internal static PwGroup CreateResultsGroup(string strNotes)
		{
			PwGroup pg = new PwGroup(true, true, "(" + KPRes.SearchGroupName + ")",
				PwIcon.EMailSearch);
			pg.IsVirtual = true;

			if(!string.IsNullOrEmpty(strNotes)) pg.Notes = strNotes;

			return pg;
		}

		internal static PwGroup Find(SearchParameters sp, PwGroup pgRoot,
			IStatusLogger sl)
		{
			if(sp == null) { Debug.Assert(false); throw new ArgumentNullException("sp"); }
			if(pgRoot == null) { Debug.Assert(false); throw new ArgumentNullException("pgRoot"); }

			string strN = null;
			if(!string.IsNullOrEmpty(sp.SearchString))
				strN = "'" + sp.SearchString + "' " + KPRes.SearchResultsInSeparator +
					" " + pgRoot.GetFullPath(true, true) + ".";

			PwGroup pgResults = CreateResultsGroup(strN);
			pgRoot.SearchEntries(sp, pgResults.Entries, sl);
			return pgResults;
		}
	}
}
