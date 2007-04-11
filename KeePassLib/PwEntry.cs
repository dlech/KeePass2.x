/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2007 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Xml;
using System.Drawing;

using KeePassLib.Collections;
using KeePassLib.Interfaces;
using KeePassLib.Security;

namespace KeePassLib
{
	/// <summary>
	/// A class representing a password entry. A password entry consists of several
	/// fields like title, user name, password, etc. Each password entry has a
	/// unique ID (UUID).
	/// </summary>
	public sealed class PwEntry : ITimeLogger, IDeepClonable<PwEntry>
	{
		private PwUuid m_uuid = PwUuid.Zero;
		private PwGroup m_pParentGroup = null;

		private ProtectedStringDictionary m_listStrings = new ProtectedStringDictionary();
		private ProtectedBinaryDictionary m_listBinaries = new ProtectedBinaryDictionary();
		private AutoTypeConfig m_listAutoType = new AutoTypeConfig();
		private PwObjectList<PwEntry> m_listHistory = new PwObjectList<PwEntry>();

		private PwIcon m_pwIcon = PwIcon.Key;
		private PwUuid m_pwCustomIconID = PwUuid.Zero;

		private Color m_clrForeground = Color.Empty;
		private Color m_clrBackground = Color.Empty;

		private DateTime m_tCreation = PwDefs.DtDefaultNow;
		private DateTime m_tLastMod = PwDefs.DtDefaultNow;
		private DateTime m_tLastAccess = PwDefs.DtDefaultNow;
		private DateTime m_tExpire = PwDefs.DtDefaultNow;
		private bool m_bExpires = false;
		private ulong m_uUsageCount = 0;

		private string m_strOverrideUrl = string.Empty;

		/// <summary>
		/// UUID of this entry.
		/// </summary>
		public PwUuid Uuid
		{
			get { return m_uuid; }
			set
			{
				Debug.Assert(value != null); if(value == null) throw new ArgumentNullException();

				m_uuid = value;
			}
		}

		/// <summary>
		/// Reference to a group which contains the current entry.
		/// </summary>
		public PwGroup ParentGroup
		{
			get { return m_pParentGroup; }
			set { m_pParentGroup = value; }
		}

		/// <summary>
		/// Get or set all entry strings.
		/// </summary>
		public ProtectedStringDictionary Strings
		{
			get { return m_listStrings; }
			set
			{
				Debug.Assert(value != null); if(value == null) throw new ArgumentNullException();
				
				m_listStrings = value;
			}
		}

		/// <summary>
		/// Get or set all entry binaries.
		/// </summary>
		public ProtectedBinaryDictionary Binaries
		{
			get { return m_listBinaries; }
			set
			{
				Debug.Assert(value != null); if(value == null) throw new ArgumentNullException();
				
				m_listBinaries = value;
			}
		}

		/// <summary>
		/// Get or set all auto-type window/keystroke sequence associations.
		/// </summary>
		public AutoTypeConfig AutoType
		{
			get { return m_listAutoType; }
			set
			{
				Debug.Assert(value != null); if(value == null) throw new ArgumentNullException();
				
				m_listAutoType = value;
			}
		}

		/// <summary>
		/// Get all previous versions of this entry (backups).
		/// </summary>
		public PwObjectList<PwEntry> History
		{
			get { return m_listHistory; }
			set
			{
				Debug.Assert(value != null); if(value == null) throw new ArgumentNullException();

				m_listHistory = value;
			}
		}

		/// <summary>
		/// Image ID specifying the icon that will be used for this entry.
		/// </summary>
		public PwIcon IconID
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
				Debug.Assert(value != null); if(value == null) throw new ArgumentNullException();

				m_pwCustomIconID = value;
			}
		}

		/// <summary>
		/// Get or set the foreground color of this entry.
		/// </summary>
		public Color ForegroundColor
		{
			get { return m_clrForeground; }
			set { m_clrForeground = value; }
		}

		/// <summary>
		/// Get or set the background color of this entry.
		/// </summary>
		public Color BackgroundColor
		{
			get { return m_clrBackground; }
			set { m_clrBackground = value; }
		}

		/// <summary>
		/// The date/time when this entry was created.
		/// </summary>
		public DateTime CreationTime
		{
			get { return m_tCreation; }
			set { m_tCreation = value; }
		}

		/// <summary>
		/// The date/time when this entry was last accessed (read).
		/// </summary>
		public DateTime LastAccessTime
		{
			get { return m_tLastAccess; }
			set { m_tLastAccess = value; }
		}

		/// <summary>
		/// The date/time when this entry was last modified.
		/// </summary>
		public DateTime LastModificationTime
		{
			get { return m_tLastMod; }
			set { m_tLastMod = value; }
		}

		/// <summary>
		/// The date/time when this entry expires. Use the <c>Expires</c> property
		/// to specify if the entry does actually expire or not.
		/// </summary>
		public DateTime ExpiryTime
		{
			get { return m_tExpire; }
			set { m_tExpire = value; }
		}

		/// <summary>
		/// Specifies whether the entry expires or not.
		/// </summary>
		public bool Expires
		{
			get { return m_bExpires; }
			set { m_bExpires = value; }
		}

		/// <summary>
		/// Get or set the usage count of the entry. To increase the usage
		/// count by one, use the <c>Touch</c> function.
		/// </summary>
		public ulong UsageCount
		{
			get { return m_uUsageCount; }
			set { m_uUsageCount = value; }
		}

		/// <summary>
		/// Entry-specific override URL. If this string is non-empty,
		/// </summary>
		public string OverrideUrl
		{
			get { return m_strOverrideUrl; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");

				m_strOverrideUrl = value;
			}
		}

		/// <summary>
		/// Construct a new, empty password entry. Member variables will be initialized
		/// to their default values.
		/// </summary>
		/// <param name="pwParentGroup">Reference to the containing group, this
		/// parameter may be <c>null</c> and set later manually.</param>
		/// <param name="bCreateNewUUID">If <c>true</c>, a new UUID will be created
		/// for this entry. If <c>false</c>, the UUID is zero and you must set it
		/// manually later.</param>
		/// <param name="bSetTimes">If <c>true</c>, the creation, last modification
		/// and last access times will be set to the current system time. The expire
		/// time is set to never (<c>DtInfinity</c>).</param>
		public PwEntry(PwGroup pwParentGroup, bool bCreateNewUUID, bool bSetTimes)
		{
			m_pParentGroup = pwParentGroup;

			if(bCreateNewUUID) m_uuid = new PwUuid(true);

			if(bSetTimes)
			{
				m_tCreation = m_tLastMod = m_tLastAccess = DateTime.Now;
				// m_tExpire == PwDefs.DtInfinity
			}
		}

		/// <summary>
		/// Clone the current entry. The returned entry is an exact value copy
		/// of the current entry (including UUID and parent group reference).
		/// All mutable members are cloned.
		/// </summary>
		/// <returns>Exact value clone. All references to mutable values changed.</returns>
		public PwEntry CloneDeep()
		{
			PwEntry peNew = new PwEntry(this.m_pParentGroup, false, false);

			peNew.m_uuid = this.m_uuid; // PwUUID is immutable

			peNew.m_listStrings = this.m_listStrings.CloneDeep();
			peNew.m_listBinaries = this.m_listBinaries.CloneDeep();
			peNew.m_listAutoType = this.m_listAutoType.CloneDeep();
			peNew.m_listHistory = this.m_listHistory.CloneDeep();

			peNew.m_pwIcon = this.m_pwIcon;
			peNew.m_pwCustomIconID = this.m_pwCustomIconID;

			peNew.m_clrForeground = this.m_clrForeground;
			peNew.m_clrBackground = this.m_clrBackground;

			peNew.m_tCreation = this.m_tCreation;
			peNew.m_tLastMod = this.m_tLastMod;
			peNew.m_tLastAccess = this.m_tLastAccess;
			peNew.m_tExpire = this.m_tExpire;
			peNew.m_bExpires = this.m_bExpires;
			peNew.m_uUsageCount = this.m_uUsageCount;

			peNew.m_strOverrideUrl = this.m_strOverrideUrl;

			return peNew;
		}

		/// <summary>
		/// Touch the entry. This function updates the internal last access
		/// time. If the <paramref name="bModified" /> parameter is <c>true</c>,
		/// the last modification time gets updated, too.
		/// </summary>
		/// <param name="bModified">Modify last modification time.</param>
		public void Touch(bool bModified)
		{
			m_tLastAccess = DateTime.Now;
			m_uUsageCount++;

			if(bModified) m_tLastMod = m_tLastAccess;

			if(m_pParentGroup != null) m_pParentGroup.Touch(bModified);
		}

		/// <summary>
		/// Create a backup of this entry. The backup item doesn't contain any
		/// history items.
		/// </summary>
		public void CreateBackup()
		{
			PwEntry peCopy = this.CloneDeep();
			peCopy.History = new PwObjectList<PwEntry>(); // Remove history

			this.m_listHistory.Add(peCopy);
		}

		/// <summary>
		/// Restore an entry snapshot from backups.
		/// </summary>
		/// <param name="uBackupIndex">Index of the backup item, to which
		/// should be reverted.</param>
		public void RestoreFromBackup(uint uBackupIndex)
		{
			Debug.Assert(uBackupIndex < m_listHistory.UCount);
			if(uBackupIndex >= m_listHistory.UCount)
				throw new ArgumentOutOfRangeException("uBackupIndex");

			PwEntry pe = m_listHistory.GetAt(uBackupIndex);
			Debug.Assert(pe != null); if(pe == null) throw new InvalidOperationException();

			CreateBackup();
			AssignProperties(pe, false, false);
		}

		/// <summary>
		/// Assign properties to the current entry based on a template entry.
		/// </summary>
		/// <param name="peTemplate">Template entry. Must not be <c>null</c>.</param>
		/// <param name="bOnlyIfNewer">Only set the properties of the template entry
		/// if it is newer than the current one.</param>
		/// <param name="bIncludeHistory">If <c>true</c>, the history will be
		/// copied, too.</param>
		public void AssignProperties(PwEntry peTemplate, bool bOnlyIfNewer,
			bool bIncludeHistory)
		{
			Debug.Assert(peTemplate != null); if(peTemplate == null) throw new ArgumentNullException();

			// Template UUID should be the same as the current one
			Debug.Assert(m_uuid.EqualsValue(peTemplate.m_uuid));
			m_uuid = peTemplate.m_uuid;

			if(bOnlyIfNewer)
			{
				if(peTemplate.m_tLastMod < this.m_tLastMod) return;
			}

			m_listStrings = peTemplate.m_listStrings;
			m_listBinaries = peTemplate.m_listBinaries;
			m_listAutoType = peTemplate.m_listAutoType;

			if(bIncludeHistory) m_listHistory = peTemplate.m_listHistory;

			m_pwIcon = peTemplate.m_pwIcon;
			m_pwCustomIconID = peTemplate.m_pwCustomIconID;

			m_clrForeground = peTemplate.m_clrForeground;
			m_clrBackground = peTemplate.m_clrBackground;

			m_tCreation = peTemplate.m_tCreation;
			m_tLastMod = peTemplate.m_tLastMod;
			m_tLastAccess = peTemplate.m_tLastAccess;
			m_tExpire = peTemplate.m_tExpire;
			m_bExpires = peTemplate.m_bExpires;
			m_uUsageCount = peTemplate.m_uUsageCount;

			m_strOverrideUrl = peTemplate.m_strOverrideUrl;
		}
	}
}
