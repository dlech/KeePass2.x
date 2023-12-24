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
using System.Diagnostics;
using System.IO;
using System.Text;

using KeePass.Resources;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	// 20.0.7-22.3.1+
	internal sealed class SteganosCsv20 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Steganos Password Manager CSV"; } }
		public override string DefaultExtension { get { return "csv"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			StreamReader sr = new StreamReader(sInput, StrUtil.Utf8, true);
			string strData = sr.ReadToEnd();
			sr.Close();

			CsvOptions opt = new CsvOptions();
			opt.BackslashIsEscape = false;
			// opt.TextQualifier = char.MinValue; // For 20.0.7, not 22.3.1
			opt.TrimFields = true;

			CsvStreamReaderEx csv = new CsvStreamReaderEx(strData, opt);

			string strMapIgnore = Guid.NewGuid().ToString();
			string strMapGroup = Guid.NewGuid().ToString();
			string strMapTags = Guid.NewGuid().ToString();
			string strMapLastMod = Guid.NewGuid().ToString();
			string strMapEMail = Guid.NewGuid().ToString();

			Dictionary<string, string> dMaps = new Dictionary<string, string>(
				StrUtil.CaseIgnoreComparer)
			{
				{ "title", PwDefs.TitleField },
				{ "type", strMapIgnore },
				{ "username_field", strMapIgnore },
				{ "username", PwDefs.UserNameField },
				{ "password_field", strMapIgnore },
				{ "password", PwDefs.PasswordField },
				{ "url", PwDefs.UrlField },
				{ "category", strMapGroup },
				{ "note", PwDefs.NotesField },
				{ "autofill", strMapIgnore },
				{ "autofillenabled", strMapIgnore },
				{ "last_password_change", strMapIgnore },
				{ "lastmodified", strMapLastMod },
				{ "iban", PwDefs.UserNameField },
				{ "bic", "BIC" },
				{ "banking_pin", PwDefs.PasswordField },
				{ "card_number", PwDefs.UserNameField },
				{ "card_holder", "Card Holder" },
				{ "card_pin", PwDefs.PasswordField },
				{ "card_verification_code", "Verification Code" },
				{ "valid_from", "Valid From" },
				{ "valid_thru", "Valid To" },
				{ "name", PwDefs.UserNameField },
				{ "firstname", PwDefs.UserNameField },
				{ "street", PwDefs.NotesField },
				{ "houseno", PwDefs.NotesField },
				{ "zip", PwDefs.NotesField },
				{ "city", PwDefs.NotesField },
				{ "mobile_phone", PwDefs.NotesField },
				{ "phone", PwDefs.NotesField },
				{ "email", strMapEMail },
				{ "birthday", "Birthday" },
				{ "tags", strMapTags },
				{ "keyword", strMapTags }
			};

			string[] vNames = csv.ReadLine();
			if((vNames == null) || (vNames.Length == 0)) { Debug.Assert(false); return; }

			for(int i = 0; i < vNames.Length; ++i)
			{
				string str = vNames[i];

				if(string.IsNullOrEmpty(str)) { Debug.Assert(false); str = strMapIgnore; }
				else
				{
					string strMapped;
					dMaps.TryGetValue(str, out strMapped);

					if(string.IsNullOrEmpty(strMapped))
					{
						Debug.Assert(false);
						strMapped = ImportUtil.MapNameToStandardField(str, true);
						if(string.IsNullOrEmpty(strMapped)) strMapped = PwDefs.NotesField;
					}

					str = strMapped;
				}

				vNames[i] = str;
			}

			Dictionary<string, PwGroup> dGroups = new Dictionary<string, PwGroup>();

			while(true)
			{
				string[] v = csv.ReadLine();
				if(v == null) break;
				if(v.Length == 0) continue;

				PwEntry pe = new PwEntry(true, true);
				PwGroup pg = pwStorage.RootGroup;

				for(int i = 0; i < v.Length; ++i)
				{
					string strValue = v[i];
					if(string.IsNullOrEmpty(strValue)) continue;

					strValue = strValue.Replace(@"<COMMA>", ",");
					strValue = strValue.Replace(@"<-N/L-/>", "\n");

					strValue = StrUtil.NormalizeNewLines(strValue, true);

					string strName = ((i < vNames.Length) ? vNames[i] : PwDefs.NotesField);

					if(strName == strMapIgnore) { }
					else if(strName == strMapGroup)
					{
						dGroups.TryGetValue(strValue, out pg);
						if(pg == null)
						{
							pg = new PwGroup(true, true);
							pg.Name = strValue;

							pwStorage.RootGroup.AddGroup(pg, true);
							dGroups[strValue] = pg;
						}
					}
					else if(strName == strMapTags)
						StrUtil.AddTags(pe.Tags, StrUtil.StringToTags(strValue));
					else if(strName == strMapLastMod)
					{
						double dUnix;
						if(double.TryParse(strValue, out dUnix))
							pe.LastModificationTime = TimeUtil.ConvertUnixTime(dUnix);
						else { Debug.Assert(false); }
					}
					else if(strName == strMapEMail)
						ImportUtil.AppendToField(pe, PwDefs.UrlField,
							"mailto:" + strValue, pwStorage);
					else ImportUtil.AppendToField(pe, strName, strValue, pwStorage);
				}

				pg.AddEntry(pe, true);
			}
		}
	}
}
