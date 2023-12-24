/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2023 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePassLib;

namespace KeePass.DataExchange
{
	public sealed class PwExportInfo
	{
		private PwGroup m_pg;
		/// <summary>
		/// This group contains all entries and subgroups that should
		/// be exported. Is never <c>null</c>.
		/// </summary>
		public PwGroup DataGroup
		{
			get { return m_pg; }
			internal set { m_pg = value; }
		}

		private readonly PwDatabase m_pd;
		/// <summary>
		/// Optional context database reference. May be <c>null</c>.
		/// </summary>
		public PwDatabase ContextDatabase
		{
			get { return m_pd; }
		}

		private readonly bool m_bExpDel;
		/// <summary>
		/// Indicates whether deleted objects should be exported, if
		/// the data format supports it.
		/// </summary>
		public bool ExportDeletedObjects
		{
			get { return m_bExpDel; }
		}

		private bool m_bExportMasterKeySpec = false;
		internal bool ExportMasterKeySpec
		{
			get { return m_bExportMasterKeySpec; }
			set { m_bExportMasterKeySpec = value; }
		}

		private bool m_bExportParentGroups = false;
		internal bool ExportParentGroups
		{
			get { return m_bExportParentGroups; }
			set { m_bExportParentGroups = value; }
		}

		private bool m_bExportPostOpen = false;
		internal bool ExportPostOpen
		{
			get { return m_bExportPostOpen; }
			set { m_bExportPostOpen = value; }
		}

		private bool m_bExportPostShow = false;
		internal bool ExportPostShow
		{
			get { return m_bExportPostShow; }
			set { m_bExportPostShow = value; }
		}

		private readonly Dictionary<string, string> m_dictParams =
			new Dictionary<string, string>();
		public Dictionary<string, string> Parameters
		{
			get { return m_dictParams; }
		}

		public PwExportInfo(PwGroup pgDataSource, PwDatabase pwContextInfo) :
			this(pgDataSource, pwContextInfo, true)
		{
		}

		public PwExportInfo(PwGroup pgDataSource, PwDatabase pwContextInfo,
			bool bExportDeleted)
		{
			if(pgDataSource == null) throw new ArgumentNullException("pgDataSource");
			// pwContextInfo may be null

			m_pg = pgDataSource;
			m_pd = pwContextInfo;
			m_bExpDel = bExportDeleted;
		}
	}
}
