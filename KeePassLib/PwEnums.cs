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

namespace KeePassLib
{
	/// <summary>
	/// Compression algorithm specifiers.
	/// </summary>
	public enum PwCompressionAlgorithm : uint
	{
		/// <summary>
		/// No compression.
		/// </summary>
		None = 0,

		/// <summary>
		/// GZip compression.
		/// </summary>
		GZip,

		/// <summary>
		/// Virtual field: currently known number of algorithms. Should not be used
		/// by plugins or libraries -- it's used internally only.
		/// </summary>
		Count
	}

	/// <summary>
	/// Return values for a load-file operation.
	/// </summary>
	public enum FileOpenResultCode : uint
	{
		/// <summary>
		/// File has been loaded successfully.
		/// </summary>
		Success = 0,

		/// <summary>
		/// An unknown error occured.
		/// </summary>
		UnknownError,

		/// <summary>
		/// File not found. The file does not exist or the resource is currently unavailable.
		/// </summary>
		FileNotFound,

		/// <summary>
		/// Windows doesn't give our program permission to access the file.
		/// </summary>
		NoFileAccess,

		/// <summary>
		/// Input-output error (for example stream closed before completely reading the file).
		/// </summary>
		IOError,

		/// <summary>
		/// Invalid file structure, the file isn't a valid KDB file.
		/// </summary>
		InvalidFileStructure,

		/// <summary>
		/// File signature is incorrect, the selected file isn't a KDB file.
		/// </summary>
		InvalidFileSignature,

		/// <summary>
		/// File version is unknown, the KDB file cannot be loaded.
		/// </summary>
		UnknownFileVersion,

		/// <summary>
		/// An unknown encryption algorithm has been used to encrypt the KDB file.
		/// </summary>
		UnknownEncryptionAlgorithm,

		/// <summary>
		/// A security exception occured. This is an internal error.
		/// </summary>
		SecurityException,

		/// <summary>
		/// Invalid header.
		/// </summary>
		InvalidHeader,

		/// <summary>
		/// Invalid file format. The inner XML file is corrupted.
		/// </summary>
		InvalidFileFormat
	}

	/// <summary>
	/// Return values for a file-save operation.
	/// </summary>
	public enum FileSaveResultCode
	{
		/// <summary>
		/// File has successfully been saved. No data loss possible.
		/// </summary>
		Success = 0,

		/// <summary>
		/// An unknown error occured.
		/// </summary>
		UnknownError,

		/// <summary>
		/// Unable to create the specified file. Make sure you have write access
		/// to the specified file/path.
		/// </summary>
		FileCreationFailed,

		/// <summary>
		/// An input-output error occured. Possibly the medium has been removed
		/// before the file was written completely.
		/// </summary>
		IOException,

		/// <summary>
		/// A security exception occured. This exception occurs when a cryptographic
		/// service (for example random number generator) fails.
		/// </summary>
		SecurityException
	}

	/// <summary>
	/// Tree traversal methods.
	/// </summary>
	public enum TraversalMethod : uint
	{
		/// <summary>
		/// Don't traverse the tree.
		/// </summary>
		None = 0,

		/// <summary>
		/// Traverse the tree in pre-order mode, i.e. first visit all items
		/// in the current node, then visit all subnodes.
		/// </summary>
		PreOrder
	}

#pragma warning disable 1591 // Missing XML comments warning
	/// <summary>
	/// Methods for merging password databases/entries.
	/// </summary>
	public enum PwMergeMethod : uint
	{
		OverwriteExisting = 0,
		KeepExisting,
		OverwriteIfNewer,
		CreateNewUuids,
		Synchronize
	}
#pragma warning restore 1591 // Missing XML comments warning

#pragma warning disable 1591 // Missing XML comments warning
	/// <summary>
	/// Icon identifiers for groups and password entries.
	/// </summary>
	public enum PwIcon : uint
	{
		Key = 0,
		World,
		Warning,
		NetworkServer,
		MarkedDirectory,
		UserCommunication,
		Parts,
		Notepad,
		WorldSocket,
		Identity,
		PaperReady,
		Digicam,
		IRCommunication,
		MultiKeys,
		Energy,
		Scanner,
		WorldStar,
		CdRom,
		Monitor,
		EMail,
		Configuration,
		ClipboardReady,
		PaperNew,
		Screen,
		EnergyCareful,
		EMailBox,
		Disk,
		Drive,
		PaperQ,
		TerminalEncrypted,
		Console,
		Printer,
		ProgramIcons,
		Run,
		Settings,
		WorldComputer,
		Archive,
		Homebanking,
		DriveWindows,
		Clock,
		EMailSearch,
		PaperFlag,
		Memory,
		TrashBin,
		Note,
		Expired,
		Info,
		Package,
		Folder,
		FolderOpen,
		FolderPackage,
		LockOpen,
		PaperLocked,
		Checked,
		Pen,
		Thumbnail,
		Book,
		List,
		UserKey,
		Tool,
		Home,
		Star,
		None,
		SortUpArrow,
		SortDownArrow,

		/// <summary>
		/// Virtual identifier -- represents the number of icons.
		/// </summary>
		Count
	}
#pragma warning restore 1591 // Missing XML comments warning
}
