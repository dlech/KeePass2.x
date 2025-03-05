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
	// 1.44 & Pro 1.07
	internal sealed class AnyPwCsv144 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Any Password CSV"; } }
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
			Debug.Assert((l.Count == 6) || (l.Count == 7));
			if(l.Count < 6) return;
			bool bIsPro = (l.Count >= 7); // Std exports 6 fields only

			PwEntry pe = new PwEntry(true, true);
			pd.RootGroup.AddEntry(pe, true);

			ImportUtil.Add(pe, PwDefs.TitleField, ParseCsvWord(l[0], false), pd);
			ImportUtil.Add(pe, PwDefs.UserNameField, ParseCsvWord(l[1], false), pd);
			ImportUtil.Add(pe, PwDefs.PasswordField, ParseCsvWord(l[2], false), pd);
			ImportUtil.Add(pe, PwDefs.UrlField, ParseCsvWord(l[3], false), pd);

			int p = 3;
			if(bIsPro)
				ImportUtil.Add(pe, KPRes.Custom, ParseCsvWord(l[++p], false), pd);

			ImportUtil.Add(pe, PwDefs.NotesField, ParseCsvWord(l[++p], true), pd);

			DateTime dt;
			if(DateTime.TryParse(ParseCsvWord(l[++p], false), out dt))
				pe.CreationTime = pe.LastModificationTime = pe.LastAccessTime =
					TimeUtil.ToUtc(dt, false);
			else { Debug.Assert(false); }
		}

		private static string ParseCsvWord(string strWord, bool bFixCodes)
		{
			string str = strWord.Trim();

			if((str.Length >= 2) && str.StartsWith("\"") && str.EndsWith("\""))
				str = str.Substring(1, str.Length - 2);

			str = str.Replace("\"\"", "\"");

			if(bFixCodes)
			{
				str = str.Replace("<13>", string.Empty);
				str = str.Replace("<10>", "\r\n");
			}

			return str;
		}
	}
}
