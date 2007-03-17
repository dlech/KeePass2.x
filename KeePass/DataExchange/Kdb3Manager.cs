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

// If compiling for the ANSI version of KeePassLibC, define KDB3_ANSI.
// If compiling for the Unicode version of KeePassLibC, do not define KDB3_ANSI.
#define KDB3_ANSI

using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace KeePass.DataExchange
{
	/// <summary>
	/// Structure containing information about a password group. This structure
	/// doesn't contain any information about the entries stored in this group.
	/// </summary>
#if PocketPC || WindowsCE
	[StructLayout(LayoutKind.Sequential)]
#else
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
#endif
	public struct Kdb3Group
	{
		[MarshalAs(UnmanagedType.U4)]
		public UInt32 GroupID;

		[MarshalAs(UnmanagedType.U4)]
		public UInt32 ImageID;

#if KDB3_ANSI
		[MarshalAs(UnmanagedType.LPStr)]
		public string Name;
#else
		[MarshalAs(UnmanagedType.LPWStr)]
		public string Name;
#endif

		public Kdb3Time CreationTime;
		public Kdb3Time LastModificationTime;
		public Kdb3Time LastAccessTime;
		public Kdb3Time ExpirationTime;

		/// <summary>
		/// Indentation level of the group.
		/// </summary>
		[MarshalAs(UnmanagedType.U2)]
		public UInt16 Level;

#if VPF_ALIGN
		[MarshalAs(UnmanagedType.U2)]
		public UInt16 Dummy;
#endif

		/// <summary>
		/// Flags of the group (see <c>Kdb3GroupFlags</c>).
		/// </summary>
		[MarshalAs(UnmanagedType.U4)]
		public UInt32 Flags;
	}

	/// <summary>
	/// Password group flags.
	/// </summary>
	public enum Kdb3GroupFlags
	{
		Expanded = 1
	}

	/// <summary>
	/// Structure containing information about a password entry.
	/// </summary>
#if PocketPC || WindowsCE
	[StructLayout(LayoutKind.Sequential)]
#else
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
#endif
	public struct Kdb3Entry
	{
		public Kdb3Uuid UUID;

		[MarshalAs(UnmanagedType.U4)]
		public UInt32 GroupID;

		[MarshalAs(UnmanagedType.U4)]
		public UInt32 ImageID;

#if KDB3_ANSI
		[MarshalAs(UnmanagedType.LPStr)]
		public string Title;
		[MarshalAs(UnmanagedType.LPStr)]
		public string URL;
		[MarshalAs(UnmanagedType.LPStr)]
		public string UserName;
		[MarshalAs(UnmanagedType.U4)]
		public UInt32 PasswordLen;
		[MarshalAs(UnmanagedType.LPStr)]
		public string Password;
		[MarshalAs(UnmanagedType.LPStr)]
		public string Additional;
#else
		[MarshalAs(UnmanagedType.LPWStr)]
		public string Title;
		[MarshalAs(UnmanagedType.LPWStr)]
		public string URL;
		[MarshalAs(UnmanagedType.LPWStr)]
		public string UserName;
		[MarshalAs(UnmanagedType.U4)]
		public UInt32 PasswordLen;
		[MarshalAs(UnmanagedType.LPWStr)]
		public string Password;
		[MarshalAs(UnmanagedType.LPWStr)]
		public string Additional;
#endif

		public Kdb3Time CreationTime;
		public Kdb3Time LastModificationTime;
		public Kdb3Time LastAccessTime;
		public Kdb3Time ExpirationTime;

#if KDB3_ANSI
		[MarshalAs(UnmanagedType.LPStr)]
		public string BinaryDescription; // A string describing what is in BinaryData
#else
		[MarshalAs(UnmanagedType.LPWStr)]
		public string BinaryDescription; // A string describing what is in BinaryData
#endif

		public IntPtr BinaryData;

		[MarshalAs(UnmanagedType.U4)]
		public UInt32 BinaryDataLen;
	}

	/// <summary>
	/// Structure containing UUID bytes (16 bytes).
	/// </summary>
#if PocketPC || WindowsCE
	[StructLayout(LayoutKind.Sequential)]
#else
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
#endif
	public struct Kdb3Uuid
	{
		public Byte V0; public Byte V1; public Byte V2; public Byte V3;
		public Byte V4; public Byte V5; public Byte V6; public Byte V7;
		public Byte V8; public Byte V9; public Byte VA; public Byte VB;
		public Byte VC; public Byte VD; public Byte VE; public Byte VF;

		/// <summary>
		/// Convert UUID to a byte array of length 16.
		/// </summary>
		/// <returns>Byte array (16 bytes).</returns>
		public byte[] ToByteArray()
		{
			return new byte[]{ this.V0, this.V1, this.V2, this.V3, this.V4,
				this.V5, this.V6, this.V7, this.V8, this.V9, this.VA,
				this.VB, this.VC, this.VD, this.VE, this.VF };
		}

		/// <summary>
		/// Set the UUID using an array of 16 bytes.
		/// </summary>
		/// <param name="pb">Bytes to set the UUID to.</param>
		public void Set(byte[] pb)
		{
			Debug.Assert((pb != null) && (pb.Length == 16));
			if((pb == null) || (pb.Length != 16)) throw new ArgumentNullException();

			this.V0 = pb[0]; this.V1 = pb[1]; this.V2 = pb[2]; this.V3 = pb[3];
			this.V4 = pb[4]; this.V5 = pb[5]; this.V6 = pb[6]; this.V7 = pb[7];
			this.V8 = pb[8]; this.V9 = pb[9]; this.VA = pb[10]; this.VB = pb[11];
			this.VC = pb[12]; this.VD = pb[13]; this.VE = pb[14]; this.VF = pb[15];
		}
	}

	/// <summary>
	/// Structure containing time information in a compact, but still easily
	/// accessible form.
	/// </summary>
#if PocketPC || WindowsCE
	[StructLayout(LayoutKind.Sequential)]
#else
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
#endif
	public struct Kdb3Time
	{
		[MarshalAs(UnmanagedType.U2)]
		public UInt16 Year;
		[MarshalAs(UnmanagedType.U1)]
		public Byte Month;
		[MarshalAs(UnmanagedType.U1)]
		public Byte Day;
		[MarshalAs(UnmanagedType.U1)]
		public Byte Hour;
		[MarshalAs(UnmanagedType.U1)]
		public Byte Minute;
		[MarshalAs(UnmanagedType.U1)]
		public Byte Second;

#if VPF_ALIGN
		[MarshalAs(UnmanagedType.U1)]
		public Byte Dummy;
#endif

		/// <summary>
		/// Convert the current <c>Kdb3Time</c> object to a <c>DateTime</c> object.
		/// </summary>
		/// <returns></returns>
		public DateTime ToDateTime()
		{
			if((this.Year == 0) || (this.Month == 0) || (this.Day == 0))
				return DateTime.Now;

			return new DateTime((int)this.Year, (int)this.Month, (int)this.Day,
				(int)this.Hour, (int)this.Minute, (int)this.Second);
		}

		/// <summary>
		/// Copy data from a <c>DateTime object</c> to the current <c>Kdb3Time</c> object.
		/// </summary>
		/// <param name="dt">Data source.</param>
		public void Set(DateTime dt)
		{
			this.Year = (UInt16)dt.Year;
			this.Month = (Byte)dt.Month;
			this.Day = (Byte)dt.Day;
			this.Hour = (Byte)dt.Hour;
			this.Minute = (Byte)dt.Minute;
			this.Second = (Byte)dt.Second;
		}
	}

	/// <summary>
	/// Error codes for various functions (<c>OpenDatabase</c>, etc.).
	/// </summary>
	public enum Kdb3ErrorCode
	{
		Unknown = 0,
		Success,
		InvalidParam,
		NotEnoughMemory,
		InvalidKey,
		NoFileAccessRead,
		NoFileAccessWrite,
		FileErrorRead,
		FileErrorWrite,
		InvalidRandomSource,
		InvalidFileStructure,
		CryptoError,
		InvalidFileSize,
		InvalidFileSignature,
		InvalidFileHeader,
		NoFileAccessReadKey
	}

	/// <summary>
	/// Manager class for Kdb3 files. It can load/save databases, add/change/delete
	/// groups and entries, check for KeePassLibC library existence and version, etc.
	/// </summary>
	public sealed class Kdb3Manager
	{
		private const string DllFile = "KeePassLibC.dll";

#if KDB3_ANSI
		private const CharSet DllCharSet = CharSet.Ansi;
#else
		private const CharSet DllCharSet = CharSet.Unicode;
#endif

		private static bool m_bFirstInstance = true;
		private IntPtr m_pManager = IntPtr.Zero;

		[DllImport(DllFile)]
		private static extern UInt32 GetKeePassVersion();
		/// <summary>
		/// Get the KeePass version, which the KeePassLibC library supports.
		/// Examples: KeePass version 1.05 is encoded as 0x01000501, version
		/// 1.06 is 0x01000601.
		/// </summary>
		public static UInt32 KeePassVersion
		{
			get { return GetKeePassVersion(); }
		}

		[DllImport(DllFile, CharSet = DllCharSet)]
		private static extern IntPtr GetKeePassVersionString();
		/// <summary>
		/// Get the KeePass version, which the KeePassLibC library supports
		/// (the version is returned as a displayable string, if you need to
		/// compare versions: use the <c>KeePassVersion</c> property).
		/// </summary>
		public static string KeePassVersionString
		{
#if KDB3_ANSI
			get { return Marshal.PtrToStringAnsi(GetKeePassVersionString()); }
#else
			get { return Marshal.PtrToStringUni(GetKeePassVersionString()); }
#endif
		}

		[DllImport(DllFile)]
		private static extern UInt32 GetLibraryBuild();
		/// <summary>
		/// Get the library build version. This version has nothing to do with
		/// the supported KeePass version (see <c>KeePassVersion</c> property).
		/// </summary>
		public static UInt32 LibraryBuild
		{
			get { return GetLibraryBuild(); }
		}

		[DllImport(DllFile)]
		private static extern UInt32 GetNumberOfEntries(IntPtr pMgr);
		/// <summary>
		/// Get the number of entries in this manager instance.
		/// </summary>
		public UInt32 EntryCount
		{
			get { return GetNumberOfEntries(m_pManager); }
		}

		[DllImport(DllFile)]
		private static extern UInt32 GetNumberOfGroups(IntPtr pMgr);
		/// <summary>
		/// Get the number of groups in this manager instance.
		/// </summary>
		public UInt32 GroupCount
		{
			get { return GetNumberOfGroups(m_pManager); }
		}

		[DllImport(DllFile)]
		private static extern UInt32 GetKeyEncRounds(IntPtr pMgr);
		[DllImport(DllFile)]
		private static extern void SetKeyEncRounds(IntPtr pMgr, UInt32 dwRounds);
		/// <summary>
		/// Get or set the number of key transformation rounds (in order to
		/// make key-searching attacks harder).
		/// </summary>
		public UInt32 KeyTransformationRounds
		{
			get { return GetKeyEncRounds(m_pManager); }
			set { SetKeyEncRounds(m_pManager, value); }
		}

		[DllImport(DllFile)]
		private static extern void InitManager(out IntPtr ppMgr, bool bFirstInstance);
		[DllImport(DllFile)]
		private static extern void NewDatabase(IntPtr pMgr);
		/// <summary>
		/// Construct a new Kdb3 manager instance.
		/// </summary>
		public Kdb3Manager()
		{
			int nExpectedSize;

#if VPF_ALIGN
			bool bAligned = true;
#else
			bool bAligned = false;
#endif

			// Static structure layout assertions
			nExpectedSize = (bAligned ? 52 : 46);
			Kdb3Group g = new Kdb3Group();
			Debug.Assert(Marshal.SizeOf(g) == nExpectedSize);
			if(Marshal.SizeOf(g) != nExpectedSize)
				throw new FormatException("SizeOf(Kdb3Group) invalid!");

			nExpectedSize = (bAligned ? 92 : 88);
			Kdb3Entry e = new Kdb3Entry();
			Debug.Assert(Marshal.SizeOf(e) == nExpectedSize);
			if(Marshal.SizeOf(e) != nExpectedSize)
				throw new FormatException("SizeOf(Kdb3Entry) invalid!");
			
			Kdb3Uuid u = new Kdb3Uuid();
			Debug.Assert(Marshal.SizeOf(u) == 16);
			if(Marshal.SizeOf(u) != 16)
				throw new FormatException("SizeOf(Kdb3Uuid) invalid!");

			nExpectedSize = (bAligned ? 8 : 7);
			Kdb3Time t = new Kdb3Time();
			Debug.Assert(Marshal.SizeOf(t) == nExpectedSize);
			if(Marshal.SizeOf(t) != nExpectedSize)
				throw new FormatException("SizeOf(Kdb3Time) invalid!");

			Kdb3Manager.InitManager(out m_pManager, m_bFirstInstance);
			m_bFirstInstance = false;

			if(m_pManager == IntPtr.Zero)
				throw new InvalidOperationException("Failed to initialize manager! DLL installed?");

			Kdb3Manager.NewDatabase(m_pManager);
		}

		~Kdb3Manager()
		{
			Unload();
		}

		[DllImport(DllFile)]
		private static extern void DeleteManager(IntPtr pMgr);
		/// <summary>
		/// This function clears up all memory associated with the current
		/// manager instance. You should call this function shortly before
		/// the object is destroyed (i.e. when you finished working with it).
		/// </summary>
		public void Unload()
		{
			if(m_pManager != IntPtr.Zero)
			{
				Kdb3Manager.DeleteManager(m_pManager);
				m_pManager = IntPtr.Zero;
			}
		}

		[DllImport(DllFile, CharSet = DllCharSet)]
		private static extern Int32 SetMasterKey(IntPtr pMgr, string pszMasterKey, bool bDiskDrive, string pszSecondKey, IntPtr pARI, bool bOverwrite);
		/// <summary>
		/// Set the master key, which will be used when you call the
		/// <c>OpenDatabase</c> or <c>SaveDatabase</c> functions.
		/// </summary>
		/// <param name="strMasterKey">Master password or path to key file. Must not be <c>null</c>.</param>
		/// <param name="bDiskDrive">Indicates if a key file is used.</param>
		/// <param name="strSecondKey">Path to the key file when both master password
		/// and key file are used.</param>
		/// <param name="pARI">Random source interface. Set this to <c>IntPtr.Zero</c>
		/// if you want to open a KDB file normally. If you want to create a key file,
		/// <c>pARI</c> must point to a <c>CNewRandomInterface</c> (see KeePass Classic
		/// source code).</param>
		/// <param name="bOverwrite">Indicates if the target file should be overwritten when
		/// creating a new key file.</param>
		/// <returns>Error code (see <c>Kdb3ErrorCode</c>).</returns>
		public Kdb3ErrorCode SetMasterKey(string strMasterKey, bool bDiskDrive, string strSecondKey, IntPtr pARI, bool bOverwrite)
		{
			Debug.Assert(strMasterKey != null);
			if(strMasterKey == null) throw new ArgumentNullException("strMasterKey");

			return (Kdb3ErrorCode)Kdb3Manager.SetMasterKey(m_pManager, strMasterKey, bDiskDrive, strSecondKey, pARI, bOverwrite);
		}

		[DllImport(DllFile, CharSet = DllCharSet)]
		private static extern UInt32 GetNumberOfItemsInGroup(IntPtr pMgr, string pszGroup);
		/// <summary>
		/// Get the number of entries in a group.
		/// </summary>
		/// <param name="strGroupName">Group name, which identifies the group. Note
		/// that multiple groups can have the same name, in this case you'll need to
		/// use the other counting function which uses group IDs.</param>
		/// <returns>Number of entries in the specified group.</returns>
		public UInt32 GetNumberOfEntriesInGroup(string strGroupName)
		{
			Debug.Assert(strGroupName != null);
			if(strGroupName == null) throw new ArgumentNullException("strGroupName");

			return Kdb3Manager.GetNumberOfItemsInGroup(m_pManager, strGroupName);
		}

		[DllImport(DllFile)]
		private static extern UInt32 GetNumberOfItemsInGroupN(IntPtr pMgr, UInt32 idGroup);
		/// <summary>
		/// Get the number of entries in a group.
		/// </summary>
		/// <param name="uGroupID">Group ID.</param>
		/// <returns>Number of entries in the specified group.</returns>
		public UInt32 GetNumberOfEntriesInGroup(UInt32 uGroupID)
		{
			Debug.Assert((uGroupID != 0) && (uGroupID != UInt32.MaxValue));
			if((uGroupID == 0) || (uGroupID == UInt32.MaxValue))
				throw new ArgumentException("Invalid group ID!");

			return Kdb3Manager.GetNumberOfItemsInGroupN(m_pManager, uGroupID);
		}

		[DllImport(DllFile)]
		private static extern IntPtr LockEntryPassword(IntPtr pMgr, IntPtr pEntry);
		[DllImport(DllFile)]
		private static extern IntPtr UnlockEntryPassword(IntPtr pMgr, IntPtr pEntry);

		[DllImport(DllFile)]
		private static extern IntPtr GetEntry(IntPtr pMgr, UInt32 dwIndex);
		/// <summary>
		/// Get an entry.
		/// </summary>
		/// <param name="uIndex">Index of the entry. This index must be valid, otherwise
		/// an <c>IndexOutOfRangeException</c> is thrown.</param>
		/// <returns>The requested entry. Note that any modifications to this
		/// structure won't affect the internal data structures of the manager.</returns>
		public Kdb3Entry GetEntry(UInt32 uIndex)
		{
			Debug.Assert(uIndex < this.EntryCount);

			IntPtr p = Kdb3Manager.GetEntry(m_pManager, uIndex);
			if(p == IntPtr.Zero) throw new IndexOutOfRangeException("Entry doesn't exist.");

			Kdb3Manager.UnlockEntryPassword(m_pManager, p);
			Kdb3Entry kdbEntry = (Kdb3Entry)Marshal.PtrToStructure(p, typeof(Kdb3Entry));
			Kdb3Manager.LockEntryPassword(m_pManager, p);
			return kdbEntry;
		}

		/// <summary>
		/// Get an entry, without returning the password.
		/// </summary>
		/// <param name="uIndex">Index of the entry. This index must be valid, otherwise
		/// an <c>IndexOutOfRangeException</c> is thrown.</param>
		/// <returns>The requested entry. Note that any modifications to this
		/// structure won't affect the internal data structures of the manager.</returns>
		public Kdb3Entry GetEntryWithoutPassword(UInt32 uIndex)
		{
			Debug.Assert(uIndex < this.EntryCount);

			IntPtr p = Kdb3Manager.GetEntry(m_pManager, uIndex);
			if(p == IntPtr.Zero) throw new IndexOutOfRangeException("Entry doesn't exist.");

			Kdb3Entry kdbEntry = (Kdb3Entry)Marshal.PtrToStructure(p, typeof(Kdb3Entry));
			kdbEntry.Password = string.Empty;
			kdbEntry.PasswordLen = 0;
			return kdbEntry;
		}

		[DllImport(DllFile)]
		private static extern IntPtr GetEntryByGroup(IntPtr pMgr, UInt32 idGroup, UInt32 dwIndex);
		/// <summary>
		/// Get an entry in a specific group.
		/// </summary>
		/// <param name="uIndex">Index of the entry in the group. This index must
		/// be valid, otherwise an <c>IndexOutOfRangeException</c> is thrown.</param>
		/// <param name="uGroupID">ID of the group containing the entry.</param>
		/// <returns>The requested entry. Note that any modifications to this
		/// structure won't affect the internal data structures of the manager.</returns>
		public Kdb3Entry GetEntryByGroup(UInt32 uGroupID, UInt32 uIndex)
		{
			Debug.Assert((uGroupID != 0) && (uGroupID != UInt32.MaxValue));
			if((uGroupID == 0) || (uGroupID == UInt32.MaxValue))
				throw new ArgumentException("Invalid group ID!");

			Debug.Assert(uIndex < this.EntryCount);

			IntPtr p = Kdb3Manager.GetEntryByGroup(m_pManager, uGroupID, uIndex);
			if(p == IntPtr.Zero) throw new IndexOutOfRangeException();

			Kdb3Manager.UnlockEntryPassword(m_pManager, p);
			Kdb3Entry kdbEntry = (Kdb3Entry)Marshal.PtrToStructure(p, typeof(Kdb3Entry));
			Kdb3Manager.LockEntryPassword(m_pManager, p);
			return kdbEntry;
		}

		[DllImport(DllFile)]
		private static extern IntPtr GetGroup(IntPtr pMgr, UInt32 dwIndex);
		/// <summary>
		/// Get a group.
		/// </summary>
		/// <param name="uIndex">Index of the group. Must be valid, otherwise an
		/// <c>IndexOutOfRangeException</c> is thrown.</param>
		/// <returns>Group structure.</returns>
		public Kdb3Group GetGroup(UInt32 uIndex)
		{
			Debug.Assert(uIndex < this.GroupCount);

			IntPtr p = Kdb3Manager.GetGroup(m_pManager, uIndex);
			if(p == IntPtr.Zero) throw new IndexOutOfRangeException();

			return (Kdb3Group)Marshal.PtrToStructure(p, typeof(Kdb3Group));
		}

		[DllImport(DllFile, CharSet = DllCharSet)]
		private static extern Int32 OpenDatabase(IntPtr pMgr, string pszFile, IntPtr pRepair);
		/// <summary>
		/// Open a KDB3 database.
		/// </summary>
		/// <param name="strFile">File path of the database.</param>
		/// <param name="pRepairInfo">Pointer to a repair information structure. If
		/// you want to open a KDB3 file normally, set this parameter to
		/// <c>IntPtr.Zero</c>.</param>
		/// <returns>Error code (see <c>Kdb3ErrorCode</c>).</returns>
		public Kdb3ErrorCode OpenDatabase(string strFile, IntPtr pRepairInfo)
		{
			Debug.Assert(strFile != null);
			if(strFile == null) throw new ArgumentNullException("strFile");

			return (Kdb3ErrorCode)Kdb3Manager.OpenDatabase(m_pManager, strFile, pRepairInfo);
		}

		[DllImport(DllFile, CharSet = DllCharSet)]
		private static extern Int32 SaveDatabase(IntPtr pMgr, string pszFile);
		/// <summary>
		/// Save the current contents of the manager to a KDB3 file on disk.
		/// </summary>
		/// <param name="strFile">File to create.</param>
		/// <returns>Error code (see <c>Kdb3ErrorCode</c>).</returns>
		public Kdb3ErrorCode SaveDatabase(string strFile)
		{
			Debug.Assert(strFile != null);
			if(strFile == null) throw new ArgumentNullException("strFile");

			return (Kdb3ErrorCode)Kdb3Manager.SaveDatabase(m_pManager, strFile);
		}

		[DllImport(DllFile)]
		private static extern void GetNeverExpireTime(ref Kdb3Time pPwTime);
		/// <summary>
		/// Get the date/time object representing the 'Never Expires' status.
		/// </summary>
		/// <returns><c>DateTime</c> object.</returns>
		public DateTime GetNeverExpireTime()
		{
			Kdb3Time t = new Kdb3Time();
			Kdb3Manager.GetNeverExpireTime(ref t);

			return t.ToDateTime();
		}

		[DllImport(DllFile, CharSet = DllCharSet)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool AddGroup(IntPtr pMgr, ref Kdb3Group pTemplate);
		/// <summary>
		/// Add a new password group.
		/// </summary>
		/// <param name="pNewGroup">Template containing new group information.</param>
		/// <returns>Returns <c>true</c> if the group was created successfully.</returns>
		public bool AddGroup(ref Kdb3Group pNewGroup)
		{
			return Kdb3Manager.AddGroup(m_pManager, ref pNewGroup);
		}

		[DllImport(DllFile, CharSet = DllCharSet)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetGroup(IntPtr pMgr, UInt32 dwIndex, ref Kdb3Group pTemplate);
		/// <summary>
		/// Set/change a password group.
		/// </summary>
		/// <param name="uIndex">Index of the group to be changed.</param>
		/// <param name="pNewGroup">Template containing new group information.</param>
		/// <returns>Returns <c>true</c> if the group was created successfully.</returns>
		public bool SetGroup(UInt32 uIndex, ref Kdb3Group pNewGroup)
		{
			Debug.Assert(uIndex < this.GroupCount);

			return Kdb3Manager.SetGroup(m_pManager, uIndex, ref pNewGroup);
		}

		[DllImport(DllFile)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool DeleteGroupById(IntPtr pMgr, UInt32 uGroupId);
		/// <summary>
		/// Delete a password group.
		/// </summary>
		/// <param name="uGroupID">ID of the group to be deleted.</param>
		/// <returns>If the group has been deleted, the return value is <c>true</c>.</returns>
		public bool DeleteGroupByID(UInt32 uGroupID)
		{
			Debug.Assert((uGroupID != 0) && (uGroupID != UInt32.MaxValue));
			if((uGroupID == 0) || (uGroupID == UInt32.MaxValue))
				throw new ArgumentException("Invalid group ID!");

			return Kdb3Manager.DeleteGroupById(m_pManager, uGroupID);
		}

		[DllImport(DllFile, CharSet = DllCharSet)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool AddEntry(IntPtr pMgr, ref Kdb3Entry pTemplate);
		/// <summary>
		/// Add a new password entry.
		/// </summary>
		/// <param name="peNew">Template containing new entry information.</param>
		/// <returns>Returns <c>true</c> if the entry was created successfully.</returns>
		public bool AddEntry(ref Kdb3Entry peNew)
		{
			return Kdb3Manager.AddEntry(m_pManager, ref peNew);
		}

		[DllImport(DllFile, CharSet = DllCharSet)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetEntry(IntPtr pMgr, UInt32 dwIndex, ref Kdb3Entry pTemplate);
		/// <summary>
		/// Set/change a password entry.
		/// </summary>
		/// <param name="uIndex">Index of the entry to be changed.</param>
		/// <param name="peNew">Template containing new entry information.</param>
		/// <returns>Returns <c>true</c> if the entry was created successfully.</returns>
		public bool SetEntry(UInt32 uIndex, ref Kdb3Entry peNew)
		{
			Debug.Assert(uIndex < this.EntryCount);

			return Kdb3Manager.SetEntry(m_pManager, uIndex, ref peNew);
		}

		[DllImport(DllFile)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool DeleteEntry(IntPtr pMgr, UInt32 dwIndex);
		/// <summary>
		/// Delete a password entry.
		/// </summary>
		/// <param name="uIndex">Index of the entry.</param>
		/// <returns>If the entry has been deleted, the return value is <c>true</c>.</returns>
		public bool DeleteEntry(UInt32 uIndex)
		{
			Debug.Assert(uIndex < this.EntryCount);

			return Kdb3Manager.DeleteEntry(m_pManager, uIndex);
		}

		/// <summary>
		/// Helper function to extract file attachments.
		/// </summary>
		/// <param name="pMemory">Native memory pointer (as stored in the
		/// <c>BinaryData</c> member of <c>Kdb3Entry</c>.</param>
		/// <param name="uSize">Size in bytes of the memory block.</param>
		/// <returns>Managed byte array.</returns>
		public static byte[] ReadBinary(IntPtr pMemory, uint uSize)
		{
			Debug.Assert(pMemory != IntPtr.Zero);
			if(pMemory == IntPtr.Zero) throw new ArgumentNullException("pMemory");

			byte[] pb = new byte[uSize];
			if(uSize == 0) return pb;

			Marshal.Copy(pMemory, pb, 0, (int)uSize);
			return pb;
		}
	}
}
