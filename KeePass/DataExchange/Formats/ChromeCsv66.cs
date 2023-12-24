/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2023 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Text;

using KeePass.Resources;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	// 66-116+
	internal sealed class ChromeCsv66 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Google Chrome Passwords CSV"; } }
		public override string DefaultExtension { get { return "csv"; } }
		public override string ApplicationGroup { get { return KPRes.Browser; } }

		public override bool ImportAppendsToRootGroupOnly { get { return true; } }

		public override void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			StreamReader sr = new StreamReader(sInput, StrUtil.Utf8, true);
			string str = sr.ReadToEnd();
			sr.Close();

			CsvOptions opt = new CsvOptions();
			opt.BackslashIsEscape = false;

			CsvStreamReaderEx csr = new CsvStreamReaderEx(str, opt);

			CsvTableEntryReader tr = new CsvTableEntryReader(pwStorage);
			tr.SetDataAppend("name", PwDefs.TitleField);
			tr.SetDataAppend("username", PwDefs.UserNameField);
			tr.SetDataAppend("password", PwDefs.PasswordField);
			tr.SetDataAppend("url", PwDefs.UrlField);
			tr.SetDataAppend("note", PwDefs.NotesField);

			tr.Read(csr);
		}
	}
}
