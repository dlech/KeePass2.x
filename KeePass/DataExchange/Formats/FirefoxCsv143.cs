/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2026 Dominik Reichl <dominik.reichl@t-online.de>

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
	// 143.0.1+
	internal sealed class FirefoxCsv143 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Mozilla Firefox Passwords CSV"; } }
		public override string DefaultExtension { get { return "csv"; } }
		public override string ApplicationGroup { get { return KPRes.Browser; } }

		public override bool ImportAppendsToRootGroupOnly { get { return true; } }

		public override void Import(PwDatabase pdStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			string str = MemUtil.ReadString(sInput, StrUtil.Utf8);

			CsvOptions opt = new CsvOptions();
			opt.BackslashIsEscape = false;

			CsvStreamReaderEx csr = new CsvStreamReaderEx(str, opt);

			CsvTableEntryReader tr = new CsvTableEntryReader(pdStorage);
			tr.SetDataHandler("url", (strData, pe, vContextRow) =>
			{
				if(string.IsNullOrEmpty(strData)) { Debug.Assert(false); return; }
				ImportUtil.Add(pe, PwDefs.TitleField, UrlUtil.RemoveScheme(
					strData), pdStorage);
				ImportUtil.Add(pe, PwDefs.UrlField, strData, pdStorage);
			});
			tr.SetDataAdd("username", PwDefs.UserNameField);
			tr.SetDataAdd("password", PwDefs.PasswordField);
			tr.SetDataHandler("timeCreated", (strData, pe, vContextRow) =>
			{
				pe.CreationTime = ParseTime(strData);
			});
			tr.SetDataHandler("timeLastUsed", (strData, pe, vContextRow) =>
			{
				pe.LastAccessTime = ParseTime(strData);
			});
			tr.SetDataHandler("timePasswordChanged", (strData, pe, vContextRow) =>
			{
				pe.LastModificationTime = ParseTime(strData);
			});

			tr.Read(csr);
		}

		private static DateTime ParseTime(string strData)
		{
			double t;
			if(StrUtil.TryParseDoubleInvariant(strData, out t))
				return TimeUtil.ConvertUnixTime(t);

			Debug.Assert(false);
			return DateTime.UtcNow;
		}
	}
}
