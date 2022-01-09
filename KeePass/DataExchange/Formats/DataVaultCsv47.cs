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
using System.Diagnostics;
using System.IO;
using System.Text;

using KeePass.Resources;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Security;

namespace KeePass.DataExchange.Formats
{
	// 4.7.35-6.2.7+
	internal sealed class DataVaultCsv47 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "DataVault CSV"; } }
		public override string DefaultExtension { get { return "csv"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }
		
		public override bool ImportAppendsToRootGroupOnly { get { return true; } }

		public override void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			string strData;
			using(StreamReader sr = new StreamReader(sInput, Encoding.Default))
			{
				strData = sr.ReadToEnd();
			}

			// Fix new-line sequences
			strData = strData.Replace("\r\r\n", "\r\n");

			CsvStreamReader csv = new CsvStreamReader(strData, false);
			while(true)
			{
				string[] v = csv.ReadLine();
				if(v == null) break;
				if(v.Length == 0) continue;

				PwEntry pe = new PwEntry(true, true);
				pwStorage.RootGroup.AddEntry(pe, true);

				ImportUtil.AppendToField(pe, PwDefs.TitleField, v[0], pwStorage);

				int p = 1;
				while((p + 1) < v.Length)
				{
					string strMapped = ImportUtil.MapNameToStandardField(v[p], true);
					string strKey = (string.IsNullOrEmpty(strMapped) ? v[p] : strMapped);
					string strValue = v[p + 1];

					p += 2;

					if(strKey.Length == 0)
					{
						if(strValue.Length == 0) continue;

						Debug.Assert(false);
						strKey = PwDefs.NotesField;
					}

					ImportUtil.AppendToField(pe, strKey, strValue, pwStorage);
				}

				if((p < v.Length) && !string.IsNullOrEmpty(v[p]))
					ImportUtil.AppendToField(pe, PwDefs.NotesField, v[p], pwStorage);
			}
		}
	}
}
