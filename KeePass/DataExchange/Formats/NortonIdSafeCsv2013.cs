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
using System.IO;
using System.Text;

using KeePass.Resources;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	// 2013.4.0.10
	internal sealed class NortonIdSafeCsv2013 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Norton Identity Safe CSV"; } }
		public override string DefaultExtension { get { return "csv"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override void Import(PwDatabase pdStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			string strData = MemUtil.ReadString(sInput, Encoding.Unicode);

			CsvOptions opt = new CsvOptions();
			opt.BackslashIsEscape = false;

			CsvStreamReaderEx csv = new CsvStreamReaderEx(strData, opt);

			while(true)
			{
				string[] v = csv.ReadLine();
				if(v == null) break;
				if(v.Length < 5) continue;

				if(v[0].Equals("url", StrUtil.CaseIgnoreCmp) &&
					v[1].Equals("username", StrUtil.CaseIgnoreCmp) &&
					v[2].Equals("password", StrUtil.CaseIgnoreCmp))
					continue; // Header

				PwGroup pg = pdStorage.RootGroup;
				string strGroup = v[4];
				if(!string.IsNullOrEmpty(strGroup))
					pg = pg.FindCreateGroup(strGroup, true);

				PwEntry pe = new PwEntry(true, true);
				pg.AddEntry(pe, true);

				ImportUtil.Add(pe, PwDefs.UrlField, v[0], pdStorage);
				ImportUtil.Add(pe, PwDefs.UserNameField, v[1], pdStorage);
				ImportUtil.Add(pe, PwDefs.PasswordField, v[2], pdStorage);
				ImportUtil.Add(pe, PwDefs.TitleField, v[3], pdStorage);
			}
		}
	}
}
