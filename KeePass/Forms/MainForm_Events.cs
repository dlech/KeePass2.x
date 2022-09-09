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

namespace KeePass.Forms
{
	public partial class MainForm : Form
	{
		/// <summary>
		/// Event that is fired after a database has been created.
		/// </summary>
		public event EventHandler<FileCreatedEventArgs> FileCreated;

		/// <summary>
		/// Event that is fired after a database has been opened.
		/// </summary>
		public event EventHandler<FileOpenedEventArgs> FileOpened;

		public event EventHandler<FileClosingEventArgs> FileClosingPre;
		public event EventHandler<FileClosingEventArgs> FileClosingPost;

		/// <summary>
		/// Event that is fired after a database has been closed.
		/// </summary>
		public event EventHandler<FileClosedEventArgs> FileClosed;

		/// <summary>
		/// If possible, use the FileSaving event instead.
		/// </summary>
		public event EventHandler<FileSavingEventArgs> FileSavingPre;

		/// <summary>
		/// Event that is fired before a database is being saved. By handling this
		/// event, you can abort the file saving operation.
		/// </summary>
		public event EventHandler<FileSavingEventArgs> FileSaving;

		/// <summary>
		/// Event that is fired after a database has been saved.
		/// </summary>
		public event EventHandler<FileSavedEventArgs> FileSaved;

		public event EventHandler<MasterKeyChangedEventArgs> MasterKeyChanged;

		public event EventHandler FormLoadPost;

		public event EventHandler<CancelEntryEventArgs> DefaultEntryAction;

		public event EventHandler UIStateUpdated;

		public event EventHandler<FocusEventArgs> FocusChanging;

		public event EventHandler UserActivityPost;
	}
}
