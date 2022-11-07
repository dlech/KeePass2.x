/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2022 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Windows.Forms;

using KeePass.Util;

using KeePassLib;
using KeePassLib.Delegates;
using KeePassLib.Keys;
using KeePassLib.Security;
using KeePassLib.Serialization;

namespace KeePass.Forms
{
	// The resource builder uses the first class in a file.
	// If there is another class before a designed class, a
	// MissingManifestResourceException may be thrown at run-time.
	// https://web.archive.org/web/20150208051033/https://support.microsoft.com/kb/318603

	public sealed class DvfContextEventArgs : CancellableOperationEventArgs
	{
		private readonly DataViewerForm m_form;
		public DataViewerForm Form { get { return m_form; } }

		private readonly byte[] m_pbData;
		public byte[] Data { get { return m_pbData; } }

		private readonly string m_strDataDesc;
		public string DataDescription { get { return m_strDataDesc; } }

		private readonly ToolStripComboBox m_cbViewers;
		public ToolStripComboBox ViewersComboBox { get { return m_cbViewers; } }

		public DvfContextEventArgs(DataViewerForm form, byte[] pbData,
			string strDataDesc, ToolStripComboBox cbViewers)
		{
			m_form = form;
			m_pbData = pbData;
			m_strDataDesc = strDataDesc;
			m_cbViewers = cbViewers;
		}
	}

	public sealed class FpField
	{
		private readonly string m_strName;
		public string Name { get { return m_strName; } }

		private readonly ProtectedString m_psValue;
		public ProtectedString Value { get { return m_psValue; } }

		private readonly string m_strGroup;
		public string Group { get { return m_strGroup; } }

		public FpField(string strName, ProtectedString psValue, string strGroup)
		{
			m_strName = (strName ?? string.Empty);
			m_psValue = (psValue ?? ProtectedString.Empty);
			m_strGroup = (strGroup ?? string.Empty);
		}
	}

	internal enum GroupFormTab
	{
		None = 0,
		General,
		Properties,
		AutoType,
		CustomData
	}

	internal sealed class KeyCreationFormResult
	{
		public CompositeKey CompositeKey = null;

		internal GAction InvokeAfterClose = null; // Handled by ShowDialog
	}

	internal sealed class KeyPromptFormResult
	{
		public CompositeKey CompositeKey = null;
		public bool HasClosedWithExit = false;

		internal GAction InvokeAfterClose = null; // Handled by ShowDialog
	}

	/* public sealed class LvfCommand
	{
		private readonly string m_strText;
		public string Text
		{
			get { return m_strText; }
		}

		private readonly Action<ListView> m_fAction;
		public Action<ListView> Action
		{
			get { return m_fAction; }
		}

		public LvfCommand(string strText, Action<ListView> fAction)
		{
			if(strText == null) throw new ArgumentNullException("strText");
			if(fAction == null) throw new ArgumentNullException("fAction");

			m_strText = strText;
			m_fAction = fAction;
		}
	} */

	public enum PwEditMode
	{
		Invalid = 0,
		AddNewEntry,
		EditExistingEntry,
		ViewReadOnlyEntry
	}

	internal enum PwEntryFormTab
	{
		None = 0,
		General,
		Advanced,
		Properties,
		AutoType,
		History
	}

	[Flags]
	public enum FileEventFlags
	{
		None = 0,
		Exiting = 1,
		Locking = 2,
		Ecas = 4
	}

	public sealed class FileCreatedEventArgs : EventArgs
	{
		private readonly PwDatabase m_pd;
		public PwDatabase Database { get { return m_pd; } }

		public FileCreatedEventArgs(PwDatabase pd)
		{
			m_pd = pd;
		}
	}

	public sealed class FileOpenedEventArgs : EventArgs
	{
		private readonly PwDatabase m_pd;
		public PwDatabase Database { get { return m_pd; } }

		public FileOpenedEventArgs(PwDatabase pd)
		{
			m_pd = pd;
		}
	}

	public sealed class FileSavingEventArgs : CancellableOperationEventArgs
	{
		private readonly bool m_bSaveAs;
		/// <summary>
		/// Flag that indicates whether the user is performing a 'Save As' operation.
		/// If this flag is <c>false</c>, the operation is a 'Save' operation.
		/// </summary>
		public bool SaveAs { get { return m_bSaveAs; } }

		private readonly bool m_bCopy;
		public bool Copy { get { return m_bCopy; } }

		private readonly PwDatabase m_pd;
		public PwDatabase Database { get { return m_pd; } }

		private readonly Guid m_eventGuid;
		public Guid EventGuid { get { return m_eventGuid; } }

		public FileSavingEventArgs(bool bSaveAs, bool bCopy, PwDatabase pd,
			Guid eventGuid)
		{
			m_bSaveAs = bSaveAs;
			m_bCopy = bCopy;
			m_pd = pd;
			m_eventGuid = eventGuid;
		}
	}

	public sealed class FileSavedEventArgs : EventArgs
	{
		private readonly bool m_bSuccess;
		/// <summary>
		/// Specifies the result of the attempt to save the database.
		/// If this property is <c>true</c>, the database has been saved
		/// successfully.
		/// </summary>
		public bool Success { get { return m_bSuccess; } }

		private readonly PwDatabase m_pd;
		public PwDatabase Database { get { return m_pd; } }

		private readonly Guid m_eventGuid;
		public Guid EventGuid { get { return m_eventGuid; } }

		public FileSavedEventArgs(bool bSuccess, PwDatabase pd, Guid eventGuid)
		{
			m_bSuccess = bSuccess;
			m_pd = pd;
			m_eventGuid = eventGuid;
		}
	}

	public sealed class FileClosingEventArgs : CancellableOperationEventArgs
	{
		private readonly PwDatabase m_pd;
		public PwDatabase Database { get { return m_pd; } }

		private readonly FileEventFlags m_f;
		public FileEventFlags Flags { get { return m_f; } }

		public FileClosingEventArgs(PwDatabase pd, FileEventFlags f)
		{
			m_pd = pd;
			m_f = f;
		}
	}

	public sealed class FileClosedEventArgs : EventArgs
	{
		private readonly IOConnectionInfo m_ioc;
		public IOConnectionInfo IOConnectionInfo { get { return m_ioc; } }

		private readonly FileEventFlags m_f;
		public FileEventFlags Flags { get { return m_f; } }

		public FileClosedEventArgs(IOConnectionInfo ioc, FileEventFlags f)
		{
			m_ioc = ioc;
			m_f = f;
		}
	}

	public sealed class MasterKeyChangedEventArgs : EventArgs
	{
		private readonly PwDatabase m_pd;
		public PwDatabase Database { get { return m_pd; } }

		public MasterKeyChangedEventArgs(PwDatabase pd)
		{
			m_pd = pd;
		}
	}

	public sealed class CancelEntryEventArgs : CancellableOperationEventArgs
	{
		private readonly PwEntry m_pe;
		public PwEntry Entry { get { return m_pe; } }

		private readonly int m_idColumn;
		public int ColumnId { get { return m_idColumn; } }

		public CancelEntryEventArgs(PwEntry pe, int idColumn)
		{
			m_pe = pe;
			m_idColumn = idColumn;
		}
	}

	public sealed class FocusEventArgs : CancellableOperationEventArgs
	{
		private readonly Control m_cNewRequested;
		public Control RequestedControl { get { return m_cNewRequested; } }

		private readonly Control m_cNewFocusing;
		public Control FocusingControl { get { return m_cNewFocusing; } }

		public FocusEventArgs(Control cRequested, Control cFocusing)
		{
			m_cNewRequested = cRequested;
			m_cNewFocusing = cFocusing;
		}
	}
}
