/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2020 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Drawing;
using System.IO;
using System.Text;

using KeePass.Resources;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	// 20.0.7+
	internal sealed class SteganosCsv20 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Steganos Password Manager CSV"; } }
		public override string DefaultExtension { get { return "csv"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override Image SmallIcon
		{
			get { return KeePass.Properties.Resources.B16x16_Imp_Steganos20; }
		}

		public override void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			StreamReader sr = new StreamReader(sInput, StrUtil.Utf8, true);
			string strData = sr.ReadToEnd();
			sr.Close();

			CsvOptions opt = new CsvOptions();
			opt.BackslashIsEscape = false;
			opt.TextQualifier = char.MinValue;
			opt.TrimFields = true;

			CsvStreamReaderEx csv = new CsvStreamReaderEx(strData, opt);

			string strMapIgnore = Guid.NewGuid().ToString();
			string strMapGroup = Guid.NewGuid().ToString();
			string strMapTags = Guid.NewGuid().ToString();
			string strMapLastMod = Guid.NewGuid().ToString();
			string strMapEMail = Guid.NewGuid().ToString();

			Dictionary<string, string> dMaps = new Dictionary<string, string>(
				StrUtil.CaseIgnoreComparer);
			dMaps["title"] = PwDefs.TitleField;
			dMaps["type"] = strMapIgnore;
			dMaps["username_field"] = strMapIgnore;
			dMaps["username"] = PwDefs.UserNameField;
			dMaps["password_field"] = strMapIgnore;
			dMaps["password"] = PwDefs.PasswordField;
			dMaps["url"] = PwDefs.UrlField;
			dMaps["category"] = strMapGroup;
			dMaps["note"] = PwDefs.NotesField;
			dMaps["autofill"] = strMapIgnore;
			dMaps["autofillenabled"] = strMapIgnore;
			dMaps["last_password_change"] = strMapIgnore;
			dMaps["lastmodified"] = strMapLastMod;
			dMaps["iban"] = PwDefs.UserNameField;
			dMaps["bic"] = "BIC";
			dMaps["banking_pin"] = PwDefs.PasswordField;
			dMaps["card_number"] = PwDefs.UserNameField;
			dMaps["card_holder"] = "Card Holder";
			dMaps["card_pin"] = PwDefs.PasswordField;
			dMaps["card_verification_code"] = "Verification Code";
			dMaps["valid_from"] = "Valid From";
			dMaps["valid_thru"] = "Valid To";
			dMaps["name"] = PwDefs.UserNameField;
			dMaps["firstname"] = PwDefs.UserNameField;
			dMaps["street"] = PwDefs.NotesField;
			dMaps["houseno"] = PwDefs.NotesField;
			dMaps["zip"] = PwDefs.NotesField;
			dMaps["city"] = PwDefs.NotesField;
			dMaps["mobile_phone"] = PwDefs.NotesField;
			dMaps["phone"] = PwDefs.NotesField;
			dMaps["email"] = strMapEMail;
			dMaps["birthday"] = "Birthday";
			dMaps["tags"] = strMapTags;
			dMaps["keyword"] = strMapTags;

			string[] vNames = csv.ReadLine();
			if((vNames == null) || (vNames.Length == 0)) { Debug.Assert(false); return; }

			for(int i = 0; i < vNames.Length; ++i)
			{
				string str = vNames[i];

				if(string.IsNullOrEmpty(str)) { Debug.Assert(false); str = strMapIgnore; }
				else
				{
					string strMapped = null;
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
					{
						List<string> lTags = StrUtil.StringToTags(strValue);
						foreach(string strTag in lTags) pe.AddTag(strTag);
					}
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
