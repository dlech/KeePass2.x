/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2025 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Text;

using KeePass.Resources;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	// 7.0
	internal sealed class PwKeeperCsv70 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Password Keeper CSV"; } }
		public override string DefaultExtension { get { return "csv"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override bool ImportAppendsToRootGroupOnly { get { return true; } }

		public override void Import(PwDatabase pdStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			string strData = MemUtil.ReadString(sInput, Encoding.Default);

			string[] vLines = strData.Split(new char[] { '\r', '\n' });

			foreach(string strLine in vLines)
			{
				if(strLine.Length > 5) ProcessCsvLine(strLine, pdStorage);
			}
		}

		private static void ProcessCsvLine(string strLine, PwDatabase pd)
		{
			List<string> l = ImportUtil.SplitCsvLine(strLine, ",");
			if(l.Count != 5)
			{
				Debug.Assert(false);
				throw new FormatException();
			}

			PwEntry pe = new PwEntry(true, true);
			pd.RootGroup.AddEntry(pe, true);

			ImportUtil.Add(pe, PwDefs.TitleField, ProcessCsvWord(l[0]), pd);
			ImportUtil.Add(pe, PwDefs.UserNameField, ProcessCsvWord(l[1]), pd);
			ImportUtil.Add(pe, PwDefs.PasswordField, ProcessCsvWord(l[2]), pd);
			ImportUtil.Add(pe, PwDefs.UrlField, ProcessCsvWord(l[3]), pd);
			ImportUtil.Add(pe, PwDefs.NotesField, ProcessCsvWord(l[4]), pd);
		}

		private static string ProcessCsvWord(string strWord)
		{
			if(strWord == null) return string.Empty;
			if(strWord.Length < 2) return strWord;

			if(strWord.StartsWith("\"") && strWord.EndsWith("\""))
				return strWord.Substring(1, strWord.Length - 2);

			return strWord;
		}
	}
}
