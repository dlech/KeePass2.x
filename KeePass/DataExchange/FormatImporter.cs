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
using System.IO;
using System.Drawing;

using KeePass.Resources;

using KeePassLib;
using KeePassLib.Interfaces;

namespace KeePass.DataExchange
{
	public abstract class FormatImporter
	{
		public abstract string FormatName { get; }

		public virtual string DisplayName
		{
			get { return this.FormatName; }
		}

		public abstract string DefaultExtension { get; }

		public abstract string AppGroup { get; }

		public virtual Image SmallIcon
		{
			get { return KeePass.Properties.Resources.B16x16_Folder_Inbox; }
		}

		public virtual bool RequiresFile
		{
			get { return true; }
		}

		public virtual bool SupportsUuids
		{
			get { return false; }
		}

		public virtual bool RequiresKey
		{
			get { return false; }
		}

		public virtual bool TryBeginImports()
		{
			return true;
		}

		/// <summary>
		/// Import a stream into a database. Throws an exception if an error
		/// occurs.
		/// </summary>
		/// <param name="pwStorage">Data storage into which the data will be imported.</param>
		/// <param name="sInput">Input stream to read the data from.</param>
		/// <param name="slLogger">Status logger. May be <c>null</c>.</param>
		public abstract void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger);
	}
}
