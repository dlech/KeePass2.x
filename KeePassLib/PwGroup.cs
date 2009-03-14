/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2009 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Text.RegularExpressions;

using KeePassLib.Collections;
using KeePassLib.Delegates;
using KeePassLib.Interfaces;
using KeePassLib.Security;

namespace KeePassLib
{
	/// <summary>
	/// A group containing several password entries.
	/// </summary>
	public sealed class PwGroup : ITimeLogger, IDeepClonable<PwGroup>
	{
		private PwObjectList<PwGroup> m_listGroups = new PwObjectList<PwGroup>();
		private PwObjectList<PwEntry> m_listEntries = new PwObjectList<PwEntry>();
		private PwGroup m_pParentGroup = null;

		private PwUuid m_uuid = PwUuid.Zero;
		private string m_strName = string.Empty;
		private string m_strNotes = string.Empty;

		private PwIcon m_pwIcon = PwIcon.Folder;
		private PwUuid m_pwCustomIconID = PwUuid.Zero;

		private DateTime m_tCreation = PwDefs.DtDefaultNow;
		private DateTime m_tLastMod = PwDefs.DtDefaultNow;
		private DateTime m_tLastAccess = PwDefs.DtDefaultNow;
		private DateTime m_tExpire = PwDefs.DtDefaultNow;
		private bool m_bExpires = false;
		private ulong m_uUsageCount = 0;

		private bool m_bIsExpanded = true;
		private bool m_bVirtual = false;

		private string m_strDefaultAutoTypeSequence = string.Empty;

		private PwUuid m_pwLastTopVisibleEntry = PwUuid.Zero;

		/// <summary>
		/// UUID of this group.
		/// </summary>
		public PwUuid Uuid
		{
			get { return m_uuid; }
			set
			{
				Debug.Assert(value != null); if(value == null) throw new ArgumentNullException("value");
				
				m_uuid = value;
			}
		}

		/// <summary>
		/// The name of this group. Cannot be <c>null</c>.
		/// </summary>
		public string Name
		{
			get { return m_strName; }
			set
			{
				Debug.Assert(value != null); if(value == null) throw new ArgumentNullException("value");
				
				m_strName = value;
			}
		}

		/// <summary>
		/// Comments about this group. Cannot be <c>null</c>.
		/// </summary>
		public string Notes
		{
			get { return m_strNotes; }
			set
			{
				Debug.Assert(value != null); if(value == null) throw new ArgumentNullException("value");

				m_strNotes = value;
			}
		}

		/// <summary>
		/// Icon of the group.
		/// </summary>
		public PwIcon IconId
		{
			get { return m_pwIcon; }
			set { m_pwIcon = value; }
		}

		/// <summary>
		/// Get the custom icon ID. This value is 0, if no custom icon is
		/// being used (i.e. the icon specified by the <c>IconID</c> property
		/// should be displayed).
		/// </summary>
		public PwUuid CustomIconUuid
		{
			get { return m_pwCustomIconID; }
			set
			{
				Debug.Assert(value != null); if(value == null) throw new ArgumentNullException("value");

				m_pwCustomIconID = value;
			}
		}

		/// <summary>
		/// Reference to the group to which this group belongs. May be <c>null</c>.
		/// </summary>
		public PwGroup ParentGroup
		{
			get { return m_pParentGroup; }

			/// Plugins: use PwGroup.AddGroup instead.
			internal set { Debug.Assert(value != this); m_pParentGroup = value; }
		}

		/// <summary>
		/// A flag that specifies if the group is shown as expanded or
		/// collapsed in the user interface.
		/// </summary>
		public bool IsExpanded
		{
			get { return m_bIsExpanded; }
			set { m_bIsExpanded = value; }
		}

		/// <summary>
		/// The date/time when this group was created.
		/// </summary>
		public DateTime CreationTime
		{
			get { return m_tCreation; }
			set { m_tCreation = value; }
		}

		/// <summary>
		/// The date/time when this group was last modified.
		/// </summary>
		public DateTime LastModificationTime
		{
			get { return m_tLastMod; }
			set { m_tLastMod = value; }
		}

		/// <summary>
		/// The date/time when this group was last accessed (read).
		/// </summary>
		public DateTime LastAccessTime
		{
			get { return m_tLastAccess; }
			set { m_tLastAccess = value; }
		}

		/// <summary>
		/// The date/time when this group expires. A value of <c>DtInfinity</c>
		/// means that the entry never expires.
		/// </summary>
		public DateTime ExpiryTime
		{
			get { return m_tExpire; }
			set { m_tExpire = value; }
		}

		/// <summary>
		/// Flag that determines if the group expires.
		/// </summary>
		public bool Expires
		{
			get { return m_bExpires; }
			set { m_bExpires = value; }
		}

		/// <summary>
		/// Get or set the usage count of the group. To increase the usage
		/// count by one, use the <c>Touch</c> function.
		/// </summary>
		public ulong UsageCount
		{
			get { return m_uUsageCount; }
			set { m_uUsageCount = value; }
		}

		/// <summary>
		/// Get a list of subgroups in this group.
		/// </summary>
		public PwObjectList<PwGroup> Groups
		{
			get { return m_listGroups; }
		}

		/// <summary>
		/// Get a list of entries in this group.
		/// </summary>
		public PwObjectList<PwEntry> Entries
		{
			get { return m_listEntries; }
		}

		/// <summary>
		/// A flag specifying whether this group is virtual or not. Virtual
		/// groups can contain links to entries stored in other groups.
		/// Note that this flag has to be interpreted and set by the calling
		/// code; it won't prevent you from accessing and modifying the list
		/// of entries in this group in any way.
		/// </summary>
		public bool IsVirtual
		{
			get { return m_bVirtual; }
			set { m_bVirtual = value; }
		}

		/// <summary>
		/// Default auto-type keystroke sequence for all entries in
		/// this group. This property can be an empty string, which
		/// means that the value should be inherited from the parent.
		/// </summary>
		public string DefaultAutoTypeSequence
		{
			get { return m_strDefaultAutoTypeSequence; }
			set
			{
				Debug.Assert(value != null); if(value == null) throw new ArgumentNullException("value");
				m_strDefaultAutoTypeSequence = value;
			}
		}

		public PwUuid LastTopVisibleEntry
		{
			get { return m_pwLastTopVisibleEntry; }
			set
			{
				Debug.Assert(value != null); if(value == null) throw new ArgumentNullException("value");
				m_pwLastTopVisibleEntry = value;
			}
		}

		/// <summary>
		/// Construct a new, empty group.
		/// </summary>
		public PwGroup()
		{
		}

		/// <summary>
		/// Construct a new, empty group.
		/// </summary>
		/// <param name="bCreateNewUuid">Create a new UUID for this group.</param>
		/// <param name="bSetTimes">Set creation, last access and last modification times to the current time.</param>
		public PwGroup(bool bCreateNewUuid, bool bSetTimes)
		{
			if(bCreateNewUuid) m_uuid = new PwUuid(true);

			if(bSetTimes)
			{
				m_tCreation = m_tLastMod = m_tLastAccess = DateTime.Now;
			}
		}

		/// <summary>
		/// Construct a new group.
		/// </summary>
		/// <param name="bCreateNewUuid">Create a new UUID for this group.</param>
		/// <param name="bSetTimes">Set creation, last access and last modification times to the current time.</param>
		/// <param name="strName">Name of the new group.</param>
		/// <param name="pwIcon">Icon of the new group.</param>
		public PwGroup(bool bCreateNewUuid, bool bSetTimes, string strName, PwIcon pwIcon)
		{
			if(bCreateNewUuid) m_uuid = new PwUuid(true);

			if(bSetTimes)
			{
				m_tCreation = m_tLastMod = m_tLastAccess = DateTime.Now;
			}

			if(strName != null) m_strName = strName;

			m_pwIcon = pwIcon;
		}

		/// <summary>
		/// Deeply clone the current group. The returned group will be an exact
		/// value copy of the current object (including UUID, etc.).
		/// </summary>
		/// <returns>Exact value copy of the current <c>PwGroup</c> object.</returns>
		public PwGroup CloneDeep()
		{
			PwGroup pg = new PwGroup(false, false);

			pg.m_listGroups = m_listGroups.CloneDeep();
			pg.m_listEntries = m_listEntries.CloneDeep();
			pg.m_pParentGroup = m_pParentGroup;

			pg.m_uuid = m_uuid; // PwUUID is immutable

			pg.m_strName = m_strName;
			pg.m_strNotes = m_strNotes;

			pg.m_pwIcon = m_pwIcon;
			pg.m_pwCustomIconID = m_pwCustomIconID;

			pg.m_tCreation = m_tCreation;
			pg.m_tExpire = m_tExpire;
			pg.m_tLastAccess = m_tLastAccess;
			pg.m_tLastMod = m_tLastMod;
			pg.m_bExpires = m_bExpires;
			pg.m_uUsageCount = m_uUsageCount;

			pg.m_bIsExpanded = m_bIsExpanded;
			pg.m_bVirtual = m_bVirtual;

			pg.m_strDefaultAutoTypeSequence = m_strDefaultAutoTypeSequence;

			pg.m_pwLastTopVisibleEntry = m_pwLastTopVisibleEntry;

			return pg;
		}

		/// <summary>
		/// Assign properties to the current group based on a template group.
		/// </summary>
		/// <param name="pgTemplate">Template group. Must not be <c>null</c>.</param>
		/// <param name="bOnlyIfNewer">Only set the properties of the template group
		/// if it is newer than the current one.</param>
		public void AssignProperties(PwGroup pgTemplate, bool bOnlyIfNewer)
		{
			Debug.Assert(pgTemplate != null); if(pgTemplate == null) throw new ArgumentNullException("pgTemplate");

			// Template UUID should be the same as the current one
			Debug.Assert(m_uuid.EqualsValue(pgTemplate.m_uuid));

			if(bOnlyIfNewer)
			{
				if(pgTemplate.m_tLastMod < m_tLastMod) return;
			}

			m_strName = pgTemplate.m_strName;
			m_strNotes = pgTemplate.m_strNotes;

			m_pwIcon = pgTemplate.m_pwIcon;
			m_pwCustomIconID = pgTemplate.m_pwCustomIconID;

			m_tCreation = pgTemplate.m_tCreation;
			m_tLastMod = pgTemplate.m_tLastMod;
			m_tLastAccess = pgTemplate.m_tLastAccess;
			m_tExpire = pgTemplate.m_tExpire;
			m_bExpires = pgTemplate.m_bExpires;
			m_uUsageCount = pgTemplate.m_uUsageCount;

			m_strDefaultAutoTypeSequence = pgTemplate.m_strDefaultAutoTypeSequence;

			m_pwLastTopVisibleEntry = pgTemplate.m_pwLastTopVisibleEntry;
		}

		/// <summary>
		/// Touch the group. This function updates the internal last access
		/// time. If the <paramref name="bModified" /> parameter is <c>true</c>,
		/// the last modification time gets updated, too.
		/// </summary>
		/// <param name="bModified"></param>
		public void Touch(bool bModified)
		{
			m_tLastAccess = DateTime.Now;
			++m_uUsageCount;

			if(bModified) m_tLastMod = m_tLastAccess;

			if(m_pParentGroup != null) m_pParentGroup.Touch(bModified);
		}

		/// <summary>
		/// Get number of groups and entries in the current group. This function
		/// can also traverse through all subgroups and accumulate their counts
		/// (recursive mode).
		/// </summary>
		/// <param name="bRecursive">If this parameter is <c>true</c>, all
		/// subgroups and entries in subgroups will be counted and added to
		/// the returned value. If it is <c>false</c>, only the number of
		/// subgroups and entries of the current group is returned.</param>
		/// <param name="uNumGroups">Number of subgroups.</param>
		/// <param name="uNumEntries">Number of entries.</param>
		public void GetCounts(bool bRecursive, out uint uNumGroups, out uint uNumEntries)
		{
			if(bRecursive)
			{
				uint uTotalGroups = m_listGroups.UCount;
				uint uTotalEntries = m_listEntries.UCount;
				uint uSubGroupCount, uSubEntryCount;

				foreach(PwGroup pg in m_listGroups)
				{
					pg.GetCounts(true, out uSubGroupCount, out uSubEntryCount);

					uTotalGroups += uSubGroupCount;
					uTotalEntries += uSubEntryCount;
				}

				uNumGroups = uTotalGroups;
				uNumEntries = uTotalEntries;
			}
			else // !bRecursive
			{
				uNumGroups = m_listGroups.UCount;
				uNumEntries = m_listEntries.UCount;
			}
		}

		/// <summary>
		/// Traverse the group/entry tree in the current group. Various traversal
		/// methods are available.
		/// </summary>
		/// <param name="tm">Specifies the traversal method.</param>
		/// <param name="groupHandler">Function that performs an action on
		/// the currently visited group (see <c>GroupHandler</c> for more).
		/// This parameter may be <c>null</c>, in this case the tree is traversed but
		/// you don't get notifications for each visited group.</param>
		/// <param name="entryHandler">Function that performs an action on
		/// the currently visited entry (see <c>EntryHandler</c> for more).
		/// This parameter may be <c>null</c>.</param>
		/// <returns>Returns <c>true</c> if all entries and groups have been
		/// traversed. If the traversal has been canceled by one of the two
		/// handlers, the return value is <c>false</c>.</returns>
		public bool TraverseTree(TraversalMethod tm, GroupHandler groupHandler, EntryHandler entryHandler)
		{
			bool bRet = false;

			switch(tm)
			{
				case TraversalMethod.None:
					bRet = true;
					break;
				case TraversalMethod.PreOrder:
					bRet = PreOrderTraverseTree(groupHandler, entryHandler);
					break;
				default:
					Debug.Assert(false);
					break;
			}

			return bRet;
		}

		private bool PreOrderTraverseTree(GroupHandler groupHandler, EntryHandler entryHandler)
		{
			if(entryHandler != null)
			{
				foreach(PwEntry pe in m_listEntries)
				{
					if(!entryHandler(pe)) return false;
				}
			}

			if(groupHandler != null)
			{
				foreach(PwGroup pg in m_listGroups)
				{
					if(!groupHandler(pg)) return false;

					pg.PreOrderTraverseTree(groupHandler, entryHandler);
				}
			}
			else // groupHandler == null
			{
				foreach(PwGroup pg in m_listGroups)
				{
					pg.PreOrderTraverseTree(null, entryHandler);
				}
			}

			return true;
		}

		/// <summary>
		/// Pack all groups into one flat linked list of references (recursively).
		/// Temporary IDs (<c>TemporaryID</c> field) and levels (<c>TemporaryLevel</c>)
		/// are assigned automatically.
		/// </summary>
		/// <returns>Flat list of all groups.</returns>
		public LinkedList<PwGroup> GetFlatGroupList()
		{
			LinkedList<PwGroup> list = new LinkedList<PwGroup>();

			foreach(PwGroup pg in m_listGroups)
			{
				list.AddLast(pg);

				if(pg.Groups.UCount != 0)
					LinearizeGroupRecursive(list, pg, 1);
			}

			return list;
		}

		private void LinearizeGroupRecursive(LinkedList<PwGroup> list, PwGroup pg, ushort uLevel)
		{
			Debug.Assert(pg != null); if(pg == null) return;

			foreach(PwGroup pwg in pg.Groups)
			{
				list.AddLast(pwg);

				if(pwg.Groups.UCount != 0)
					LinearizeGroupRecursive(list, pwg, (ushort)(uLevel + 1));
			}
		}

		/// <summary>
		/// Pack all entries into one flat linked list of references. Temporary
		/// group IDs are assigned automatically.
		/// </summary>
		/// <param name="flatGroupList">A flat group list created by
		/// <c>GetFlatGroupList</c>.</param>
		/// <returns>Flat list of all entries.</returns>
		public static LinkedList<PwEntry> GetFlatEntryList(LinkedList<PwGroup> flatGroupList)
		{
			Debug.Assert(flatGroupList != null); if(flatGroupList == null) return null;

			LinkedList<PwEntry> list = new LinkedList<PwEntry>();
			foreach(PwGroup pg in flatGroupList)
			{
				foreach(PwEntry pe in pg.Entries)
					list.AddLast(pe);
			}

			return list;
		}

		/// <summary>
		/// Enable protection of a specific string field type.
		/// </summary>
		/// <param name="strFieldName">Name of the string field to protect or unprotect.</param>
		/// <param name="bEnable">Enable protection or not.</param>
		/// <returns>Returns <c>true</c>, if the operation completed successfully,
		/// otherwise <c>false</c>.</returns>
		public bool EnableStringFieldProtection(string strFieldName, bool bEnable)
		{
			Debug.Assert(strFieldName != null);

			EntryHandler eh = delegate(PwEntry pe)
			{
				// Enable protection of current string
				ProtectedString ps = pe.Strings.Get(strFieldName);
				if(ps != null) ps.EnableProtection(bEnable);

				// Do the same for all history items
				foreach(PwEntry peHistory in pe.History)
				{
					ProtectedString psHistory = peHistory.Strings.Get(strFieldName);
					if(psHistory != null) psHistory.EnableProtection(bEnable);
				}

				return true;
			};

			return this.PreOrderTraverseTree(null, eh);
		}

		/// <summary>
		/// Search this group and all groups in the current one for entries.
		/// </summary>
		/// <param name="searchParams">Specifies the search method.</param>
		/// <param name="listStorage">Entry list in which the search results will
		/// be stored.</param>
		public void SearchEntries(SearchParameters searchParams, PwObjectList<PwEntry> listStorage)
		{
			Debug.Assert(searchParams != null); if(searchParams == null) throw new ArgumentNullException("searchParams");
			Debug.Assert(listStorage != null); if(listStorage == null) throw new ArgumentNullException("listStorage");

			string strSearch = searchParams.SearchString;
			Debug.Assert(strSearch != null); if(strSearch == null) throw new ArgumentException();

			StringComparison scType = searchParams.ComparisonMode;
			Regex rx = null;
			EntryHandler eh = null;

			bool bTitle = searchParams.SearchInTitles;
			bool bUserName = searchParams.SearchInUserNames;
			bool bPassword = searchParams.SearchInPasswords;
			bool bUrl = searchParams.SearchInUrls;
			bool bNotes = searchParams.SearchInNotes;
			bool bOther = searchParams.SearchInOther;
			bool bUuids = searchParams.SearchInUuids;
			bool bExcludeExpired = searchParams.ExcludeExpired;

			DateTime dtNow = DateTime.Now;

			if(searchParams.RegularExpression)
			{
				RegexOptions ro = RegexOptions.Compiled;
				if((scType == StringComparison.CurrentCultureIgnoreCase) ||
					(scType == StringComparison.InvariantCultureIgnoreCase) ||
					(scType == StringComparison.OrdinalIgnoreCase))
				{
					ro |= RegexOptions.IgnoreCase;
				}

				rx = new Regex(strSearch, ro);
			}

			if(strSearch.Length <= 0) // Report all
			{
				eh = delegate(PwEntry pe)
				{
					if(bExcludeExpired && pe.Expires && (dtNow > pe.ExpiryTime))
						return true; // Skip

					listStorage.Add(pe);
					return true;
				};
			}
			else
			{
				eh = delegate(PwEntry pe)
				{
					if(bExcludeExpired && pe.Expires && (dtNow > pe.ExpiryTime))
						return true; // Skip

					uint uInitialResults = listStorage.UCount;

					foreach(KeyValuePair<string, ProtectedString> kvp in pe.Strings)
					{
						string strKey = kvp.Key;

						if(strKey == PwDefs.TitleField)
							SearchEvalAdd(bTitle, strSearch, kvp.Value.ReadString(),
								scType, rx, pe, listStorage);
						else if(strKey == PwDefs.UserNameField)
							SearchEvalAdd(bUserName, strSearch, kvp.Value.ReadString(),
								scType, rx, pe, listStorage);
						else if(strKey == PwDefs.PasswordField)
							SearchEvalAdd(bPassword, strSearch, kvp.Value.ReadString(),
								scType, rx, pe, listStorage);
						else if(strKey == PwDefs.UrlField)
							SearchEvalAdd(bUrl, strSearch, kvp.Value.ReadString(),
								scType, rx, pe, listStorage);
						else if(strKey == PwDefs.NotesField)
							SearchEvalAdd(bNotes, strSearch, kvp.Value.ReadString(),
								scType, rx, pe, listStorage);
						else if(bOther)
							SearchEvalAdd(true, strSearch, kvp.Value.ReadString(),
								scType, rx, pe, listStorage);

						// An entry can match only once => break if we have added it
						if(listStorage.UCount > uInitialResults) break;
					}

					if((listStorage.UCount == uInitialResults) && bUuids)
						SearchEvalAdd(true, strSearch, pe.Uuid.ToHexString(),
							scType, rx, pe, listStorage);

					return true;
				};
			}

			PreOrderTraverseTree(null, eh);
		}

		private static void SearchEvalAdd(bool bIf, string strSearch, string strDataField,
			StringComparison scType, Regex rx, PwEntry pe, PwObjectList<PwEntry> lResults)
		{
			if(bIf == false) return;

			if(rx == null)
			{
				if(strDataField.IndexOf(strSearch, scType) >= 0)
					lResults.Add(pe);
			}
			else // Regular expression
			{
				if(rx.IsMatch(strDataField)) lResults.Add(pe);
			}
		}

		/// <summary>
		/// Find a group.
		/// </summary>
		/// <param name="uuid">UUID identifying the group the caller is looking for.</param>
		/// <param name="bSearchRecursive">If <c>true</c>, the search is recursive.</param>
		/// <returns>Returns reference to found group, otherwise <c>null</c>.</returns>
		public PwGroup FindGroup(PwUuid uuid, bool bSearchRecursive)
		{
			// Do not assert on PwUuid.Zero
			if(this.m_uuid.EqualsValue(uuid)) return this;

			if(bSearchRecursive)
			{
				PwGroup pgRec;
				foreach(PwGroup pg in m_listGroups)
				{
					pgRec = pg.FindGroup(uuid, true);
					if(pgRec != null) return pgRec;
				}
			}
			else // Not recursive
			{
				foreach(PwGroup pg in m_listGroups)
				{
					if(pg.m_uuid.EqualsValue(uuid))
						return pg;
				}
			}

			return null;
		}

		/// <summary>
		/// Try to find a subgroup and create it, if it doesn't exist yet.
		/// </summary>
		/// <param name="strName">Name of the subgroup.</param>
		/// <param name="bCreateIfNotFound">If the group isn't found: create it.</param>
		/// <returns>Returns a reference to the requested group or <c>null</c> if
		/// it doesn't exist and shouldn't be created.</returns>
		public PwGroup FindCreateGroup(string strName, bool bCreateIfNotFound)
		{
			Debug.Assert(strName != null); if(strName == null) throw new ArgumentNullException("strName");

			foreach(PwGroup pg in m_listGroups)
			{
				if(pg.Name == strName) return pg;
			}

			if(!bCreateIfNotFound) return null;

			PwGroup pgNew = new PwGroup(true, true, strName, PwIcon.Folder);
			AddGroup(pgNew, true);
			return pgNew;
		}

		/// <summary>
		/// Find an entry.
		/// </summary>
		/// <param name="uuid">UUID identifying the entry the caller is looking for.</param>
		/// <param name="bSearchRecursive">If <c>true</c>, the search is recursive.</param>
		/// <returns>Returns reference to found entry, otherwise <c>null</c>.</returns>
		public PwEntry FindEntry(PwUuid uuid, bool bSearchRecursive)
		{
			foreach(PwEntry pe in m_listEntries)
			{
				if(pe.Uuid.EqualsValue(uuid)) return pe;
			}

			if(bSearchRecursive)
			{
				PwEntry peSub;
				foreach(PwGroup pg in m_listGroups)
				{
					peSub = pg.FindEntry(uuid, true);
					if(peSub != null) return peSub;
				}
			}

			return null;
		}

		/// <summary>
		/// Get the full path of a group.
		/// </summary>
		/// <returns>Full path of the group.</returns>
		public string GetFullPath()
		{
			return this.GetFullPath(".", false);
		}

		/// <summary>
		/// Get the full path of a group.
		/// </summary>
		/// <param name="strSeparator">String that separates the group
		/// names.</param>
		/// <returns>Full path of the group.</returns>
		public string GetFullPath(string strSeparator, bool bIncludeTopMostGroup)
		{
			Debug.Assert(strSeparator != null);
			if(strSeparator == null) throw new ArgumentNullException("strSeparator");

			string strPath = m_strName;

			PwGroup pg = m_pParentGroup;
			while(pg != null)
			{
				if((!bIncludeTopMostGroup) && (pg.m_pParentGroup == null))
					break;

				strPath = pg.Name + strSeparator + strPath;

				pg = pg.m_pParentGroup;
			}

			return strPath;
		}

		/// <summary>
		/// Assign new UUIDs to groups and entries.
		/// </summary>
		/// <param name="bNewGroups">Create new UUIDs for subgroups.</param>
		/// <param name="bNewEntries">Create new UUIDs for entries.</param>
		/// <param name="bRecursive">Recursive tree traversal.</param>
		public void CreateNewItemUuids(bool bNewGroups, bool bNewEntries, bool bRecursive)
		{
			if(bNewGroups)
			{
				foreach(PwGroup pg in m_listGroups)
					pg.Uuid = new PwUuid(true);
			}

			if(bNewEntries)
			{
				foreach(PwEntry pe in m_listEntries)
					pe.Uuid = new PwUuid(true);
			}

			if(bRecursive)
			{
				foreach(PwGroup pg in m_listGroups)
					pg.CreateNewItemUuids(bNewGroups, bNewEntries, true);
			}
		}

		public void TakeOwnership(bool bTakeSubGroups, bool bTakeEntries, bool bRecursive)
		{
			if(bTakeSubGroups)
			{
				foreach(PwGroup pg in m_listGroups)
					pg.ParentGroup = this;
			}

			if(bTakeEntries)
			{
				foreach(PwEntry pe in m_listEntries)
					pe.ParentGroup = this;
			}

			if(bRecursive)
			{
				foreach(PwGroup pg in m_listGroups)
					pg.TakeOwnership(bTakeSubGroups, bTakeEntries, true);
			}
		}

		/// <summary>
		/// Find/create a subtree of groups.
		/// </summary>
		/// <param name="strTree">Tree string.</param>
		/// <param name="vSeparators">Separators that delimit groups in the
		/// <c>strTree</c> parameter.</param>
		/// <returns></returns>
		public PwGroup FindCreateSubTree(string strTree, char[] vSeparators)
		{
			Debug.Assert(strTree != null); if(strTree == null) return this;
			if(strTree.Length == 0) return this;

			string[] vGroups = strTree.Split(vSeparators);
			if((vGroups == null) || (vGroups.Length == 0)) return this;

			PwGroup pgContainer = this;
			for(int nGroup = 0; nGroup < vGroups.Length; ++nGroup)
			{
				if((vGroups[nGroup] == null) || (vGroups[nGroup].Length == 0))
					continue;

				bool bFound = false;
				foreach(PwGroup pg in pgContainer.Groups)
				{
					if(pg.Name == vGroups[nGroup])
					{
						pgContainer = pg;
						bFound = true;
						break;
					}
				}

				if(!bFound)
				{
					PwGroup pg = new PwGroup(true, true, vGroups[nGroup], PwIcon.Folder);

					pgContainer.AddGroup(pg, true);

					pgContainer = pg;
				}
			}

			return pgContainer;
		}

		/// <summary>
		/// Get the level of the group (i.e. the number of parent groups).
		/// </summary>
		/// <returns>Number of parent groups.</returns>
		public uint GetLevel()
		{
			PwGroup pg = this.ParentGroup;
			uint uLevel = 0;

			while(pg != null)
			{
				pg = pg.ParentGroup;
				++uLevel;
			}

			return uLevel;
		}

		public string GetAutoTypeSequenceInherited()
		{
			if(m_strDefaultAutoTypeSequence.Length > 0)
				return m_strDefaultAutoTypeSequence;

			if(m_pParentGroup != null)
				return m_pParentGroup.GetAutoTypeSequenceInherited();

			return string.Empty;
		}

		public PwObjectList<PwEntry> GetEntries(bool bIncludeSubGroupEntries)
		{
			if(bIncludeSubGroupEntries == false)
				return m_listEntries;

			PwObjectList<PwEntry> list = m_listEntries.CloneShallow();
			foreach(PwGroup pgSub in m_listGroups)
			{
				list.Add(pgSub.GetEntries(true));
			}

			return list;
		}

		public bool IsContainedIn(PwGroup pgContainer)
		{
			PwGroup pgCur = m_pParentGroup;
			while(pgCur != null)
			{
				if(pgCur == pgContainer) return true;

				pgCur = pgCur.m_pParentGroup;
			}

			return false;
		}

		/// <summary>
		/// Add a subgroup to this group.
		/// </summary>
		/// <param name="subGroup">Group to be added. Must not be <c>null</c>.</param>
		/// <param name="bTakeOwnership">If this parameter is <c>true</c>, the
		/// parent group reference of the subgroup will be set to the current
		/// group (i.e. the current group takes ownership of the subgroup).</param>
		public void AddGroup(PwGroup subGroup, bool bTakeOwnership)
		{
			if(subGroup == null) throw new ArgumentNullException("subGroup");

			m_listGroups.Add(subGroup);

			if(bTakeOwnership) subGroup.ParentGroup = this;
		}

		/// <summary>
		/// Add an entry to this group.
		/// </summary>
		/// <param name="pe">Entry to be added. Must not be <c>null</c>.</param>
		/// <param name="bTakeOwnership">If this parameter is <c>true</c>, the
		/// parent group reference of the entry will be set to the current
		/// group (i.e. the current group takes ownership of the entry).</param>
		public void AddEntry(PwEntry pe, bool bTakeOwnership)
		{
			if(pe == null) throw new ArgumentNullException("pe");

			m_listEntries.Add(pe);

			// Do not remove the entry from its previous parent group,
			// only assign it to the new one
			if(bTakeOwnership) pe.ParentGroup = this;
		}
	}
}
