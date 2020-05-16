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
using KeePass.UI;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	internal sealed class TrueKeyCsv4 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "True Key CSV"; } }
		public override string DefaultExtension { get { return "csv"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override bool ImportAppendsToRootGroupOnly { get { return true; } }

		public override Image SmallIcon
		{
			get { return KeePass.Properties.Resources.B16x16_Imp_TrueKey; }
		}

		public override void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			StreamReader sr = new StreamReader(sInput, StrUtil.Utf8, true);
			string strData = sr.ReadToEnd();
			sr.Close();

			CsvOptions opt = new CsvOptions();
			opt.BackslashIsEscape = false;
			opt.TrimFields = true;

			CsvStreamReaderEx csv = new CsvStreamReaderEx(strData, opt);

			string[] vNames = csv.ReadLine();
			if((vNames == null) || (vNames.Length == 0)) { Debug.Assert(false); return; }

			for(int i = 0; i < vNames.Length; ++i)
				vNames[i] = (vNames[i] ?? string.Empty).ToLowerInvariant();

			while(true)
			{
				string[] v = csv.ReadLine();
				if(v == null) break;
				if(v.Length == 0) continue;

				PwEntry pe = new PwEntry(true, true);
				pwStorage.RootGroup.AddEntry(pe, true);

				Debug.Assert(v.Length == vNames.Length);
				int m = Math.Min(v.Length, vNames.Length);

				for(int i = 0; i < m; ++i)
				{
					string strValue = v[i];
					if(string.IsNullOrEmpty(strValue)) continue;

					string strName = vNames[i];
					string strTo = null;
					DateTime? odt;

					switch(strName)
					{
						case "autologin":
						case "protectedwithpassword":
						case "subdomainonly":
						case "type":
						case "tk_export_version":
							break; // Ignore

						case "kind":
							if(strValue.Equals("note", StrUtil.CaseIgnoreCmp))
								pe.IconId = PwIcon.Note;
							else if(strValue.Equals("identity", StrUtil.CaseIgnoreCmp) ||
								strValue.Equals("drivers", StrUtil.CaseIgnoreCmp) ||
								strValue.Equals("passport", StrUtil.CaseIgnoreCmp) ||
								strValue.Equals("ssn", StrUtil.CaseIgnoreCmp))
								pe.IconId = PwIcon.Identity;
							else if(strValue.Equals("cc", StrUtil.CaseIgnoreCmp))
								pe.IconId = PwIcon.Money;
							else if(strValue.Equals("membership", StrUtil.CaseIgnoreCmp))
								pe.IconId = PwIcon.UserKey;
							break;

						case "name":
						case "title":
							strTo = PwDefs.TitleField;
							break;

						case "cardholder":
						case "email":
						case "login":
							strTo = PwDefs.UserNameField;
							break;

						case "password":
							strTo = PwDefs.PasswordField;
							break;

						case "url":
						case "website":
							strTo = PwDefs.UrlField;
							break;

						case "document_content":
						case "memo":
						case "note":
							strTo = PwDefs.NotesField;
							break;

						case "city":
						case "company":
						case "country":
						case "number":
						case "state":
						case "street":
						case "telephone":
							strTo = (new string(char.ToUpperInvariant(strName[0]), 1)) +
								strName.Substring(1);
							break;

						case "deliveryplace":
							strTo = "Delivery Place";
							break;
						case "firstname":
							strTo = "First Name";
							break;
						case "lastname":
							strTo = "Last Name";
							break;
						case "memberid":
							strTo = "Member ID";
							break;
						case "phonenumber":
							strTo = "Phone Number";
							break;
						case "streetnumber":
							strTo = "Street Number";
							break;
						case "zipcode":
							strTo = "ZIP Code";
							break;

						case "dateofbirth":
							strTo = "Date of Birth";
							strValue = DateToString(strValue);
							break;

						case "expirationdate":
						case "expirydate":
							odt = ParseTime(strValue);
							if(odt.HasValue)
							{
								pe.Expires = true;
								pe.ExpiryTime = odt.Value;
							}
							break;

						case "favorite":
							if(StrUtil.StringToBoolEx(strValue).GetValueOrDefault(false))
								pe.AddTag("Favorite");
							break;

						case "gender":
							if((strValue == "0") || (strValue == "1"))
							{
								strTo = "Gender";
								strValue = ((strValue == "0") ? "Male" : "Female");
							}
							else { Debug.Assert(false); }
							break;

						case "hexcolor":
							if((strValue.Length == 6) && StrUtil.IsHexString(strValue, true))
							{
								Color c = Color.FromArgb(unchecked((int)(0xFF000000U |
									Convert.ToUInt32(strValue, 16))));

								Color cG = UIUtil.ColorToGrayscale(c);
								if(cG.B < 128) c = UIUtil.LightenColor(c, 0.5);

								pe.BackgroundColor = c;
							}
							else { Debug.Assert(false); }
							break;

						case "issuedate":
						case "issueddate":
							strTo = "Issue Date";
							strValue = DateToString(strValue);
							break;

						case "membersince":
							strTo = "Member Since";
							strValue = DateToString(strValue);
							break;

						default:
							Debug.Assert(false); // Unknown field
							break;
					}

					if(!string.IsNullOrEmpty(strTo))
						ImportUtil.AppendToField(pe, strTo, strValue, pwStorage);
				}
			}
		}

		private static DateTime? ParseTime(string strTime)
		{
			if(string.IsNullOrEmpty(strTime)) return null;

			DateTime dt;
			if(TimeUtil.TryDeserializeUtc(strTime, out dt)) return dt;

			Debug.Assert(false);
			return null;
		}

		private static string DateToString(string strTime)
		{
			if(string.IsNullOrEmpty(strTime)) return string.Empty;

			DateTime? odt = ParseTime(strTime);
			if(odt.HasValue)
			{
				DateTime dt = TimeUtil.ToLocal(odt.Value, false);
				return TimeUtil.ToDisplayStringDateOnly(dt);
			}

			return strTime;
		}
	}
}
