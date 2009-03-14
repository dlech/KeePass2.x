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
using System.IO;
using System.Drawing;

using KeePassLib.Collections;
using KeePassLib.Cryptography;
using KeePassLib.Cryptography.Cipher;
using KeePassLib.Delegates;
using KeePassLib.Interfaces;
using KeePassLib.Keys;
using KeePassLib.Security;
using KeePassLib.Serialization;
using KeePassLib.Utility;

namespace KeePassLib
{
	/// <summary>
	/// The core password manager class. It contains a number of groups, which
	/// contain the actual entries.
	/// </summary>
	public sealed class PwDatabase
	{
		private static bool m_bPrimaryCreated = false;

		// Initializations see Clear()
		private PwGroup m_pgRootGroup = null;
		private PwObjectList<PwDeletedObject> m_vDeletedObjects = new PwObjectList<PwDeletedObject>();

		private PwUuid m_uuidDataCipher = StandardAesEngine.AesUuid;
		private PwCompressionAlgorithm m_caCompression = PwCompressionAlgorithm.GZip;
		private ulong m_uKeyEncryptionRounds = PwDefs.DefaultKeyEncryptionRounds;

		private CompositeKey m_pwUserKey = null;
		private MemoryProtectionConfig m_memProtConfig = new MemoryProtectionConfig();

		private List<PwCustomIcon> m_vCustomIcons = new List<PwCustomIcon>();
		private bool m_bUINeedsIconUpdate = true;

		private string m_strName = string.Empty;
		private string m_strDesc = string.Empty;
		private string m_strDefaultUserName = string.Empty;
		private uint m_uMntncHistoryDays = 365;

		private IOConnectionInfo m_ioSource = new IOConnectionInfo();
		private bool m_bDatabaseOpened = false;
		private bool m_bModified = false;

		private PwUuid m_pwLastSelectedGroup = PwUuid.Zero;
		private PwUuid m_pwLastTopVisibleGroup = PwUuid.Zero;

		private bool m_bUseRecycleBin = true;
		private PwUuid m_pwRecycleBin = PwUuid.Zero;

		private byte[] m_pbHashOfFileOnDisk = null;
		private byte[] m_pbHashOfLastIO = null;

		private static string m_strLocalizedAppName = string.Empty;

		/// <summary>
		/// Get the root group that contains all groups and entries stored in the
		/// database.
		/// </summary>
		/// <returns>Root group. The return value is <c>null</c>, if no database
		/// has been opened.</returns>
		public PwGroup RootGroup
		{
			get { return m_pgRootGroup; }
			set
			{
				Debug.Assert(value != null);
				if(value == null) throw new ArgumentNullException("value");

				m_pgRootGroup = value;
			}
		}

		/// <summary>
		/// <c>IOConnection</c> of the currently opened database file.
		/// Is never <c>null</c>.
		/// </summary>
		public IOConnectionInfo IOConnectionInfo
		{
			get { return m_ioSource; }
		}

		/// <summary>
		/// If this is <c>true</c>, a database is currently open.
		/// </summary>
		public bool IsOpen
		{
			get { return m_bDatabaseOpened; }
		}

		/// <summary>
		/// Modification flag. If true, the class has been modified and the
		/// user interface should prompt the user to save the changes before
		/// closing the database for example.
		/// </summary>
		public bool Modified
		{
			get { return m_bModified; }
			set { m_bModified = value; }
		}

		/// <summary>
		/// The user key used for database encryption. This key must be created
		/// and set before using any of the database load/save functions.
		/// </summary>
		public CompositeKey MasterKey
		{
			get { return m_pwUserKey; }
			set
			{
				Debug.Assert(value != null); if(value == null) throw new ArgumentNullException("value");

				m_pwUserKey = value;
			}
		}

		/// <summary>
		/// Name of the database.
		/// </summary>
		public string Name
		{
			get { return m_strName; }
			set
			{
				Debug.Assert(value != null);
				if(value != null) m_strName = value;
			}
		}

		/// <summary>
		/// Database description.
		/// </summary>
		public string Description
		{
			get { return m_strDesc; }
			set
			{
				Debug.Assert(value != null);
				if(value != null) m_strDesc = value;
			}
		}

		/// <summary>
		/// Default user name used for new entries.
		/// </summary>
		public string DefaultUserName
		{
			get { return m_strDefaultUserName; }
			set
			{
				Debug.Assert(value != null);
				if(value != null) m_strDefaultUserName = value;
			}
		}

		/// <summary>
		/// Number of days until history entries are being deleted
		/// in a database maintenance operation.
		/// </summary>
		public uint MaintenanceHistoryDays
		{
			get { return m_uMntncHistoryDays; }
			set { m_uMntncHistoryDays = value; }
		}

		/// <summary>
		/// The encryption algorithm used to encrypt the data part of the database.
		/// </summary>
		public PwUuid DataCipherUuid
		{
			get { return m_uuidDataCipher; }
			set
			{
				Debug.Assert(value != null);
				if(value != null) m_uuidDataCipher = value;
			}
		}

		/// <summary>
		/// Compression algorithm used to encrypt the data part of the database.
		/// </summary>
		public PwCompressionAlgorithm Compression
		{
			get { return m_caCompression; }
			set { m_caCompression = value; }
		}

		/// <summary>
		/// Number of key transformation rounds (in order to make dictionary
		/// attacks harder).
		/// </summary>
		public ulong KeyEncryptionRounds
		{
			get { return m_uKeyEncryptionRounds; }
			set { m_uKeyEncryptionRounds = value; }
		}

		/// <summary>
		/// Memory protection configuration (for default fields).
		/// </summary>
		public MemoryProtectionConfig MemoryProtection
		{
			get { return m_memProtConfig; }
			set
			{
				Debug.Assert(value != null); if(value == null) throw new ArgumentNullException("value");
				
				m_memProtConfig = value;
			}
		}

		/// <summary>
		/// Get a list of all deleted objects.
		/// </summary>
		public PwObjectList<PwDeletedObject> DeletedObjects
		{
			get { return m_vDeletedObjects; }
		}

		/// <summary>
		/// Get all custom icons stored in this database.
		/// </summary>
		public List<PwCustomIcon> CustomIcons
		{
			get { return m_vCustomIcons; }
		}

		/// <summary>
		/// This is a dirty-flag for the UI. It is used to indicate when an
		/// icon list update is required.
		/// </summary>
		public bool UINeedsIconUpdate
		{
			get { return m_bUINeedsIconUpdate; }
			set { m_bUINeedsIconUpdate = value; }
		}

		public PwUuid LastSelectedGroup
		{
			get { return m_pwLastSelectedGroup; }
			set
			{
				Debug.Assert(value != null); if(value == null) throw new ArgumentNullException("value");
				m_pwLastSelectedGroup = value;
			}
		}

		public PwUuid LastTopVisibleGroup
		{
			get { return m_pwLastTopVisibleGroup; }
			set
			{
				Debug.Assert(value != null); if(value == null) throw new ArgumentNullException("value");
				m_pwLastTopVisibleGroup = value;
			}
		}

		public bool RecycleBinEnabled
		{
			get { return m_bUseRecycleBin; }
			set { m_bUseRecycleBin = value; }
		}

		public PwUuid RecycleBinUuid
		{
			get { return m_pwRecycleBin; }
			set
			{
				Debug.Assert(value != null); if(value == null) throw new ArgumentNullException("value");
				m_pwRecycleBin = value;
			}
		}

		/// <summary>
		/// Hash value of the primary file on disk (last read or last write).
		/// A call to <c>SaveAs</c> without making the saved file primary will
		/// not change this hash. May be <c>null</c>.
		/// </summary>
		public byte[] HashOfFileOnDisk
		{
			get { return m_pbHashOfFileOnDisk; }
		}

		public byte[] HashOfLastIO
		{
			get { return m_pbHashOfLastIO; }
		}

		/// <summary>
		/// Localized application name.
		/// </summary>
		public static string LocalizedAppName
		{
			get { return m_strLocalizedAppName; }
			set { Debug.Assert(value != null); m_strLocalizedAppName = value; }
		}

		/// <summary>
		/// Constructs an empty password manager object.
		/// </summary>
		public PwDatabase()
		{
			if(m_bPrimaryCreated == false)
			{
				m_bPrimaryCreated = true;
			}

			Clear();
		}

		private void Clear()
		{
			m_pgRootGroup = null;
			m_vDeletedObjects = new PwObjectList<PwDeletedObject>();

			m_uuidDataCipher = StandardAesEngine.AesUuid;
			m_caCompression = PwCompressionAlgorithm.GZip;

			m_uKeyEncryptionRounds = PwDefs.DefaultKeyEncryptionRounds;

			m_pwUserKey = null;
			m_memProtConfig = new MemoryProtectionConfig();

			m_vCustomIcons = new List<PwCustomIcon>();

			m_strName = string.Empty;
			m_strDesc = string.Empty;
			m_strDefaultUserName = string.Empty;

			m_bUINeedsIconUpdate = true;

			m_ioSource = new IOConnectionInfo();
			m_bDatabaseOpened = false;
			m_bModified = false;

			m_pwLastSelectedGroup = PwUuid.Zero;
			m_pwLastTopVisibleGroup = PwUuid.Zero;

			m_pbHashOfFileOnDisk = null;
			m_pbHashOfLastIO = null;
		}

		/// <summary>
		/// Initialize the class for managing a new database. Previously loaded
		/// data is deleted.
		/// </summary>
		/// <param name="ioConnection">IO connection of the new database.</param>
		/// <param name="pwKey">Key to open the database.</param>
		public void New(IOConnectionInfo ioConnection, CompositeKey pwKey)
		{
			Debug.Assert(ioConnection != null);
			if(ioConnection == null) throw new ArgumentNullException("ioConnection");
			Debug.Assert(pwKey != null);
			if(pwKey == null) throw new ArgumentNullException("pwKey");

			this.Close();

			m_ioSource = ioConnection;
			m_pwUserKey = pwKey;

			m_bDatabaseOpened = true;
			m_bModified = true;

			m_pgRootGroup = new PwGroup(true, true,
				UrlUtil.StripExtension(UrlUtil.GetFileName(ioConnection.Path)),
				PwIcon.FolderOpen);
			m_pgRootGroup.IsExpanded = true;
		}

		/// <summary>
		/// Open a database. The URL may point to any supported data source.
		/// </summary>
		/// <param name="ioSource">IO connection to load the database from.</param>
		/// <param name="pwKey">Key used to open the specified database.</param>
		/// <param name="slLogger">Logger, which gets all status messages.</param>
		public void Open(IOConnectionInfo ioSource, CompositeKey pwKey,
			IStatusLogger slLogger)
		{
			Debug.Assert(ioSource != null);
			if(ioSource == null) throw new ArgumentNullException("ioSource");
			Debug.Assert(pwKey != null);
			if(pwKey == null) throw new ArgumentNullException("pwKey");

			this.Close();

			try
			{
				m_pgRootGroup = new PwGroup(true, true, UrlUtil.StripExtension(
					UrlUtil.GetFileName(ioSource.Path)), PwIcon.FolderOpen);
				m_pgRootGroup.IsExpanded = true;

				m_pwUserKey = pwKey;

				m_bModified = false;

				Kdb4File kdb4 = new Kdb4File(this);
				Stream s = IOConnection.OpenRead(ioSource);
				kdb4.Load(s, Kdb4Format.Default, slLogger);
				s.Close();

				m_pbHashOfLastIO = kdb4.HashOfFileOnDisk;
				m_pbHashOfFileOnDisk = kdb4.HashOfFileOnDisk;
				Debug.Assert(m_pbHashOfFileOnDisk != null);

				m_bDatabaseOpened = true;
				m_ioSource = ioSource;
			}
			catch(Exception)
			{
				this.Clear();
				throw;
			}
		}

		/// <summary>
		/// Save the currently opened database. The file is written to the location
		/// it has been opened from.
		/// </summary>
		/// <param name="slLogger">Logger that recieves status information.</param>
		public void Save(IStatusLogger slLogger)
		{
#if DEBUG
			Debug.Assert(ValidateUuidUniqueness());
#endif

			bool bMadeUnhidden = UrlUtil.UnhideFile(m_ioSource.Path);

			Stream s = IOConnection.OpenWrite(m_ioSource);

			Kdb4File kdb = new Kdb4File(this);
			kdb.Save(s, null, Kdb4Format.Default, slLogger);

			if(bMadeUnhidden) UrlUtil.HideFile(m_ioSource.Path, true); // Hide again

			m_pbHashOfLastIO = kdb.HashOfFileOnDisk;
			m_pbHashOfFileOnDisk = kdb.HashOfFileOnDisk;
			Debug.Assert(m_pbHashOfFileOnDisk != null);

			m_bModified = false;
		}

		/// <summary>
		/// Save the currently opened database to a different location. If
		/// <paramref name="bIsPrimaryNow" /> is <c>true</c>, the specified
		/// location is made the default location for future saves
		/// using <c>SaveDatabase</c>.
		/// </summary>
		/// <param name="ioConnection">New location to serialize the database to.</param>
		/// <param name="bIsPrimaryNow">If <c>true</c>, the new location is made the
		/// standard location for the database. If <c>false</c>, a copy of the currently
		/// opened database is saved to the specified location, but it isn't
		/// made the default location (i.e. no lock files will be moved for
		/// example).</param>
		/// <param name="slLogger">Logger that recieves status information.</param>
		public void SaveAs(IOConnectionInfo ioConnection, bool bIsPrimaryNow,
			IStatusLogger slLogger)
		{
			Debug.Assert(ioConnection != null);
			if(ioConnection == null) throw new ArgumentNullException("ioConnection");

			IOConnectionInfo ioCurrent = m_ioSource; // Remember current
			m_ioSource = ioConnection;

			byte[] pbHashCopy = m_pbHashOfFileOnDisk;

			try { this.Save(slLogger); }
			catch(Exception)
			{
				m_ioSource = ioCurrent; // Restore
				m_pbHashOfFileOnDisk = pbHashCopy;

				m_pbHashOfLastIO = null;
				throw;
			}

			if(!bIsPrimaryNow)
			{
				m_ioSource = ioCurrent; // Restore
				m_pbHashOfFileOnDisk = pbHashCopy;
			}
		}

		/// <summary>
		/// Closes the currently opened database. No confirmation message is shown
		/// before closing. Unsaved changes will be lost.
		/// </summary>
		public void Close()
		{
			Clear();
		}

		/// <summary>
		/// Synchronize the current database with another one.
		/// </summary>
		/// <param name="pwSource">Input database to synchronize with. This input
		/// database is used to update the current one, but is not modified! You
		/// must copy the current object if you want a second instance of the
		/// synchronized database. The input database must not be seen as valid
		/// database any more after calling <c>Synchronize</c>.</param>
		/// <param name="mm">Merge method.</param>
		public void MergeIn(PwDatabase pwSource, PwMergeMethod mm)
		{
			if(mm == PwMergeMethod.CreateNewUuids)
			{
				pwSource.RootGroup.CreateNewItemUuids(true, true, true);
			}

			GroupHandler gh = delegate(PwGroup pg)
			{
				if(pg == pwSource.m_pgRootGroup) return true;

				PwGroup pgLocal = m_pgRootGroup.FindGroup(pg.Uuid, true);
				if(pgLocal == null)
				{
					PwGroup pgSourceParent = pg.ParentGroup;
					PwGroup pgLocalContainer;
					if(pgSourceParent == pwSource.m_pgRootGroup)
						pgLocalContainer = m_pgRootGroup;
					else
						pgLocalContainer = m_pgRootGroup.FindGroup(pgSourceParent.Uuid, true);
					Debug.Assert(pgLocalContainer != null);

					PwGroup pgNew = new PwGroup();
					pgNew.Uuid = pg.Uuid;
					pgNew.AssignProperties(pg, false);
					pgLocalContainer.AddGroup(pgNew, true);
				}
				else // pgLocal != null
				{
					Debug.Assert(mm != PwMergeMethod.CreateNewUuids);

					if(mm == PwMergeMethod.OverwriteExisting)
						pgLocal.AssignProperties(pg, false);
					else if((mm == PwMergeMethod.OverwriteIfNewer) ||
						(mm == PwMergeMethod.Synchronize))
					{
						pgLocal.AssignProperties(pg, true);
					}
					// else if(mm == PwMergeMethod.KeepExisting) ...
				}

				return true;
			};

			EntryHandler eh = delegate(PwEntry pe)
			{
				PwEntry peLocal = m_pgRootGroup.FindEntry(pe.Uuid, true);
				if(peLocal == null)
				{
					PwGroup pgSourceParent = pe.ParentGroup;
					PwGroup pgLocalContainer;
					if(pgSourceParent == pwSource.m_pgRootGroup)
						pgLocalContainer = m_pgRootGroup;
					else
						pgLocalContainer = m_pgRootGroup.FindGroup(pgSourceParent.Uuid, true);
					Debug.Assert(pgLocalContainer != null);

					PwEntry peNew = new PwEntry(false, false);
					peNew.Uuid = pe.Uuid;
					peNew.AssignProperties(pe, false, true);
					pgLocalContainer.AddEntry(peNew, true);
				}
				else // peLocal == null
				{
					Debug.Assert(mm != PwMergeMethod.CreateNewUuids);

					if(mm == PwMergeMethod.OverwriteExisting)
						peLocal.AssignProperties(pe, false, true);
					else if((mm == PwMergeMethod.OverwriteIfNewer) ||
						(mm == PwMergeMethod.Synchronize))
					{
						peLocal.AssignProperties(pe, true, true);
					}
					// else if(mm == PwMergeMethod.KeepExisting) ...
				}

				return true;
			};

			if(!pwSource.RootGroup.TraverseTree(TraversalMethod.PreOrder, gh, eh))
				throw new InvalidOperationException();

			if(mm == PwMergeMethod.Synchronize)
			{
				ApplyDeletions(pwSource.m_vDeletedObjects, true);
				ApplyDeletions(m_vDeletedObjects, false);
			}
		}

		/// <summary>
		/// Apply a list of deleted objects.
		/// </summary>
		/// <param name="listDelObjects">List of deleted objects.</param>
		private void ApplyDeletions(PwObjectList<PwDeletedObject> listDelObjects,
			bool bCopyDeletionInfoToLocal)
		{
			Debug.Assert(listDelObjects != null); if(listDelObjects == null) throw new ArgumentNullException("listDelObjects");

			LinkedList<PwGroup> listGroupsToDelete = new LinkedList<PwGroup>();
			LinkedList<PwEntry> listEntriesToDelete = new LinkedList<PwEntry>();

			GroupHandler gh = delegate(PwGroup pg)
			{
				if(pg == m_pgRootGroup) return true;

				foreach(PwDeletedObject pdo in listDelObjects)
				{
					if(pg.Uuid.EqualsValue(pdo.Uuid))
						if(pg.LastModificationTime < pdo.DeletionTime)
							listGroupsToDelete.AddLast(pg);
				}

				return true;
			};

			EntryHandler eh = delegate(PwEntry pe)
			{
				foreach(PwDeletedObject pdo in listDelObjects)
				{
					if(pe.Uuid.EqualsValue(pdo.Uuid))
						if(pe.LastModificationTime < pdo.DeletionTime)
							listEntriesToDelete.AddLast(pe);
				}

				return true;
			};

			m_pgRootGroup.TraverseTree(TraversalMethod.PreOrder, gh, eh);

			foreach(PwGroup pg in listGroupsToDelete)
				pg.ParentGroup.Groups.Remove(pg);
			foreach(PwEntry pe in listEntriesToDelete)
				pe.ParentGroup.Entries.Remove(pe);

			if(bCopyDeletionInfoToLocal)
			{
				foreach(PwDeletedObject pdoNew in listDelObjects)
				{
					bool bCopy = true;

					foreach(PwDeletedObject pdoLocal in m_vDeletedObjects)
					{
						if(pdoNew.Uuid.EqualsValue(pdoLocal.Uuid))
						{
							bCopy = false;

							if(pdoNew.DeletionTime > pdoLocal.DeletionTime)
								pdoLocal.DeletionTime = pdoNew.DeletionTime;

							break;
						}
					}

					if(bCopy) m_vDeletedObjects.Add(pdoNew);
				}
			}
		}

		/* /// <summary>
		/// Synchronize current database with another one.
		/// </summary>
		/// <param name="strFile">Source file.</param>
		public void Synchronize(string strFile)
		{
			PwDatabase pwSource = new PwDatabase();

			IOConnectionInfo ioc = IOConnectionInfo.FromPath(strFile);
			pwSource.Open(ioc, this.m_pwUserKey, null);

			MergeIn(pwSource, PwMergeMethod.Synchronize);
		} */

		/// <summary>
		/// Get the index of a custom icon.
		/// </summary>
		/// <param name="pwIconId">ID of the icon.</param>
		/// <returns>Index of the icon.</returns>
		public int GetCustomIconIndex(PwUuid pwIconId)
		{
			int nIndex = 0;

			foreach(PwCustomIcon pwci in m_vCustomIcons)
			{
				if(pwci.Uuid.EqualsValue(pwIconId))
					return nIndex;

				++nIndex;
			}

			// Debug.Assert(false); // Do not assert
			return -1;
		}

		/// <summary>
		/// Get a custom icon. This function can return <c>null</c>, if
		/// no cached image of the icon is available.
		/// </summary>
		/// <param name="pwIconId">ID of the icon.</param>
		/// <returns>Image data.</returns>
		public Image GetCustomIcon(PwUuid pwIconId)
		{
			int nIndex = GetCustomIconIndex(pwIconId);

			if(nIndex >= 0) return m_vCustomIcons[nIndex].Image;
			else { Debug.Assert(false); return null; }
		}

		public bool DeleteCustomIcons(List<PwUuid> vUuidsToDelete)
		{
			Debug.Assert(vUuidsToDelete != null);
			if(vUuidsToDelete == null) throw new ArgumentNullException("vUuidsToDelete");
			if(vUuidsToDelete.Count <= 0) return true;

			EntryHandler eh = delegate(PwEntry pe)
			{
				PwUuid uuidThis = pe.CustomIconUuid;

				if(uuidThis == PwUuid.Zero) return true;

				foreach(PwUuid uuidDelete in vUuidsToDelete)
				{
					if(uuidThis.EqualsValue(uuidDelete))
					{
						pe.CustomIconUuid = PwUuid.Zero;
						break;
					}
				}

				return true;
			};

			if(!m_pgRootGroup.TraverseTree(TraversalMethod.PreOrder, null, eh))
			{
				Debug.Assert(false);
				return false;
			}

			foreach(PwUuid pwUuid in vUuidsToDelete)
			{
				int nIndex = GetCustomIconIndex(pwUuid);

				if(nIndex >= 0) m_vCustomIcons.RemoveAt(nIndex);
			}

			return true;
		}

#if DEBUG
		private bool ValidateUuidUniqueness()
		{
			List<PwUuid> l = new List<PwUuid>();
			bool bAllUnique = true;

			GroupHandler gh = delegate(PwGroup pg)
			{
				foreach(PwUuid u in l)
					bAllUnique &= !pg.Uuid.EqualsValue(u);
				l.Add(pg.Uuid);
				return true;
			};

			EntryHandler eh = delegate(PwEntry pe)
			{
				foreach(PwUuid u in l)
					bAllUnique &= !pe.Uuid.EqualsValue(u);
				l.Add(pe.Uuid);
				return true;
			};

			m_pgRootGroup.TraverseTree(TraversalMethod.PreOrder, gh, eh);
			return bAllUnique;
		}
#endif
	}
}
