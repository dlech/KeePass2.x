/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2011 Dominik Reichl <dominik.reichl@t-online.de>

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
	/// Tree traversal methods.
	/// </summary>
	public enum TraversalMethod
	{
		/// <summary>
		/// Don't traverse the tree.
		/// </summary>
		None = 0,

		/// <summary>
		/// Traverse the tree in pre-order mode, i.e. first visit all items
		/// in the current node, then visit all subnodes.
		/// </summary>
		PreOrder = 1
	}

#pragma warning disable 1591 // Missing XML comments warning
	/// <summary>
	/// Methods for merging password databases/entries.
	/// </summary>
	public enum PwMergeMethod
	{
		None = 0,
		OverwriteExisting = 1,
		KeepExisting = 2,
		OverwriteIfNewer = 3,
		CreateNewUuids = 4,
		Synchronize = 5
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
		CDRom,
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
		Tux,
		Feather,
		Apple,
		Wiki,
		Money,
		Certificate,
		BlackBerry,

		/// <summary>
		/// Virtual identifier -- represents the number of icons.
		/// </summary>
		Count
	}
#pragma warning restore 1591 // Missing XML comments warning
}
