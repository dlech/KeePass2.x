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
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;

using KeePass.Plugins;

using KeePassLib;

namespace KeePass.Forms
{
	public partial class MainForm : Form
	{
		/// <summary>
		/// Event that is fired after a database has been opened.
		/// </summary>
		public event EventHandler FileOpened;

		/// <summary>
		/// Event that is fired after a database has been closed.
		/// </summary>
		public event EventHandler FileClosed;

		/// <summary>
		/// Event handler definition for file-saving events.
		/// </summary>
		/// <param name="sender">Reserved for future use.</param>
		/// <param name="e">Information about the file-saving event.</param>
		public delegate void FileSavingEventHandler(object sender, FileSavingEventArgs e);

		/// <summary>
		/// Event that is fired before a database is being saved. By handling this
		/// event, you can abort the file-saving operation.
		/// </summary>
		public event FileSavingEventHandler FileSaving;

		/// <summary>
		/// Event arguments structure for the file-saving event.
		/// </summary>
		public sealed class FileSavingEventArgs : CancellableOperationEventArgs
		{
			private readonly bool m_bSaveAs;

			/// <summary>
			/// Flag that determines if the user is performing a 'Save As' operation.
			/// If this flag is <c>false</c>, the operation is a standard 'Save' operation.
			/// </summary>
			public bool SaveAs { get { return m_bSaveAs; } }

			/// <summary>
			/// Default constructor.
			/// </summary>
			/// <param name="bSaveAs">See <c>SaveAs</c> property.</param>
			public FileSavingEventArgs(bool bSaveAs)
			{
				m_bSaveAs = bSaveAs;
			}
		}

		/// <summary>
		/// Event handler definition for file-saved events.
		/// </summary>
		/// <param name="sender">Reserved for future use.</param>
		/// <param name="e">Information about the file-saved event.</param>
		public delegate void FileSavedEventHandler(object sender, FileSavedEventArgs e);

		/// <summary>
		/// Event that is fired after a database has been saved.
		/// </summary>
		public event FileSavedEventHandler FileSaved;

		/// <summary>
		/// Event arguments structure for the file-saved event.
		/// </summary>
		public sealed class FileSavedEventArgs : EventArgs
		{
			private readonly FileSaveResult m_fsr;

			/// <summary>
			/// Specifies the result of the attempt to save the database. If
			/// this property is <c>FileSaveResult.Success</c>, the database
			/// has been saved successfully.
			/// </summary>
			public FileSaveResult FileSaveResult { get { return m_fsr; } }

			/// <summary>
			/// Default constructor.
			/// </summary>
			/// <param name="fsr">See <c>FileSaveResult</c> property.</param>
			public FileSavedEventArgs(FileSaveResult fsr)
			{
				m_fsr = fsr;
			}
		}
	}
}
