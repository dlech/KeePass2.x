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
using System.Xml.Serialization;

using KeePassLib;
using KeePassLib.Utility;

namespace KeePass.App.Configuration
{
	public sealed class AceSearch
	{
		public AceSearch()
		{
		}

		private SearchParameters m_spLast = new SearchParameters();
		public SearchParameters LastUsedProfile
		{
			get { return m_spLast; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_spLast = value;
			}
		}

		private List<SearchParameters> m_lUserProfiles = new List<SearchParameters>();
		[XmlArrayItem("Profile")]
		public List<SearchParameters> UserProfiles
		{
			get { return m_lUserProfiles; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_lUserProfiles = value;
			}
		}

		internal int FindProfileIndex(string strName)
		{
			for(int i = 0; i < m_lUserProfiles.Count; ++i)
			{
				if(m_lUserProfiles[i].Name == strName) return i;
			}

			return -1;
		}

		internal SearchParameters FindProfile(string strName)
		{
			int i = FindProfileIndex(strName);
			return ((i >= 0) ? m_lUserProfiles[i] : null);
		}

		internal void SortProfiles()
		{
			m_lUserProfiles.Sort(AceSearch.CompareProfilesByName);
		}

		private static int CompareProfilesByName(SearchParameters spA, SearchParameters spB)
		{
			if(spA == null) { Debug.Assert(false); return ((spB == null) ? 0 : -1); }
			if(spB == null) { Debug.Assert(false); return 1; }

			return StrUtil.CompareNaturally(spA.Name, spB.Name);
		}
	}
}
