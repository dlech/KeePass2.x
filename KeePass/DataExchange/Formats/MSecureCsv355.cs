/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2019 Dominik Reichl <dominik.reichl@t-online.de>

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
	// 3.5.5+
	internal sealed class MSecureCsv355 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "mSecure CSV"; } }
		public override string DefaultExtension { get { return "csv"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override bool ImportAppendsToRootGroupOnly { get { return false; } }

		public override Image SmallIcon
		{
			get { return KeePass.Properties.Resources.B16x16_Imp_MSecure; }
		}

		public override void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			StreamReader sr = new StreamReader(sInput, Encoding.Default, true);
			string str = sr.ReadToEnd();
			sr.Close();

			CsvOptions opt = new CsvOptions();

			// Backslashes are not escaped, even though "\\n" is used
			// to encode new-line characters
			opt.BackslashIsEscape = false;
			opt.FieldSeparator = ';';

			CsvStreamReaderEx csr = new CsvStreamReaderEx(str, opt);
			Dictionary<string, PwGroup> dGroups = new Dictionary<string, PwGroup>();

			while(true)
			{
				string[] vLine = csr.ReadLine();
				if(vLine == null) break;

				AddEntry(vLine, pwStorage, dGroups);
			}
		}

		private static void AddEntry(string[] vLine, PwDatabase pd,
			Dictionary<string, PwGroup> dGroups)
		{
			if(vLine.Length < 2) return;

			string strGroup = vLine[0];
			PwGroup pg;
			if(string.IsNullOrEmpty(strGroup))
				pg = pd.RootGroup;
			else
			{
				if(!dGroups.TryGetValue(strGroup, out pg))
				{
					pg = new PwGroup(true, true);
					pg.Name = strGroup;

					pd.RootGroup.AddGroup(pg, true);
					dGroups[strGroup] = pg;
				}
			}

			PwEntry pe = new PwEntry(true, true);
			pg.AddEntry(pe, true);

			MsAppend(pe, PwDefs.TitleField, vLine, 2, pd);
			MsAppend(pe, PwDefs.NotesField, vLine, 3, pd);

			string strType = vLine[1];
			int i = 3;
			DateTime dt;

			switch(strType)
			{
				case "Bank Accounts":
					MsAppend(pe, PwDefs.UserNameField, vLine, ++i, pd);
					MsAppend(pe, PwDefs.PasswordField, vLine, ++i, pd);
					MsAppend(pe, "Name", vLine, ++i, pd);
					MsAppend(pe, "Branch", vLine, ++i, pd);
					MsAppend(pe, "Phone No.", vLine, ++i, pd);

					pe.IconId = PwIcon.Money;
					break;

				case "Birthdays":
					MsAppend(pe, PwDefs.UserNameField, vLine, ++i, pd);

					pe.IconId = PwIcon.UserCommunication;
					break;

				case "Calling Cards":
				case "Social Security":
				case "Voice Mail":
					MsAppend(pe, PwDefs.UserNameField, vLine, ++i, pd);
					MsAppend(pe, PwDefs.PasswordField, vLine, ++i, pd);

					pe.IconId = PwIcon.UserKey;
					break;

				case "Clothes Size":
					MsAppend(pe, "Shirt Size", vLine, ++i, pd);
					MsAppend(pe, "Pant Size", vLine, ++i, pd);
					MsAppend(pe, "Shoe Size", vLine, ++i, pd);
					MsAppend(pe, "Dress Size", vLine, ++i, pd);
					break;

				case "Combinations":
					MsAppend(pe, PwDefs.PasswordField, vLine, ++i, pd);
					break;

				case "Credit Cards":
					MsAppend(pe, PwDefs.UserNameField, vLine, ++i, pd);

					++i;
					if((vLine.Length > i) && StrUtil.TryParseDateTime(
						vLine[i], out dt))
					{
						pe.Expires = true;
						pe.ExpiryTime = TimeUtil.ToUtc(dt, false);
					}
					else MsAppend(pe, "Expiration", vLine, i, pd);

					MsAppend(pe, "Name", vLine, ++i, pd);
					MsAppend(pe, PwDefs.PasswordField, vLine, ++i, pd);
					MsAppend(pe, "Bank", vLine, ++i, pd);
					MsAppend(pe, "Security Code", vLine, ++i, pd);

					pe.IconId = PwIcon.Money;
					break;

				case "Email Accounts":
					MsAppend(pe, PwDefs.UserNameField, vLine, ++i, pd);
					MsAppend(pe, PwDefs.PasswordField, vLine, ++i, pd);
					MsAppend(pe, "POP3 Host", vLine, ++i, pd);
					MsAppend(pe, "SMTP Host", vLine, ++i, pd);

					pe.IconId = PwIcon.EMail;
					break;

				case "Frequent Flyer":
					MsAppend(pe, "Number", vLine, ++i, pd);
					MsAppend(pe, PwDefs.UrlField, vLine, ++i, pd);
					MsAppend(pe, PwDefs.UserNameField, vLine, ++i, pd);
					MsAppend(pe, PwDefs.PasswordField, vLine, ++i, pd);
					MsAppend(pe, "Mileage", vLine, ++i, pd);
					break;

				case "Identity":
					++i;
					MsAppend(pe, PwDefs.UserNameField, vLine, i + 1, pd); // Last name
					MsAppend(pe, PwDefs.UserNameField, vLine, i, pd); // First name
					++i;

					MsAppend(pe, "Nick Name", vLine, ++i, pd);
					MsAppend(pe, "Company", vLine, ++i, pd);
					MsAppend(pe, PwDefs.UserNameField, vLine, ++i, pd);
					MsAppend(pe, "Address", vLine, ++i, pd);
					MsAppend(pe, "Address 2", vLine, ++i, pd);
					MsAppend(pe, "City", vLine, ++i, pd);
					MsAppend(pe, "State", vLine, ++i, pd);
					MsAppend(pe, "Country", vLine, ++i, pd);
					MsAppend(pe, "Zip", vLine, ++i, pd);
					MsAppend(pe, "Home Phone", vLine, ++i, pd);
					MsAppend(pe, "Office Phone", vLine, ++i, pd);
					MsAppend(pe, "Mobile Phone", vLine, ++i, pd);
					MsAppend(pe, "E-Mail", vLine, ++i, pd);
					MsAppend(pe, "E-Mail 2", vLine, ++i, pd);
					MsAppend(pe, "Skype", vLine, ++i, pd);
					MsAppend(pe, PwDefs.UrlField, vLine, ++i, pd);

					pe.IconId = PwIcon.UserCommunication;
					break;

				case "Insurance":
					MsAppend(pe, PwDefs.UserNameField, vLine, ++i, pd);
					MsAppend(pe, "Group No.", vLine, ++i, pd);
					MsAppend(pe, "Insured", vLine, ++i, pd);
					MsAppend(pe, "Date", vLine, ++i, pd);
					MsAppend(pe, "Phone No.", vLine, ++i, pd);

					pe.IconId = PwIcon.Certificate;
					break;

				case "Memberships":
					MsAppend(pe, PwDefs.PasswordField, vLine, ++i, pd);
					MsAppend(pe, PwDefs.UserNameField, vLine, ++i, pd);
					MsAppend(pe, "Date", vLine, ++i, pd);

					pe.IconId = PwIcon.UserKey;
					break;

				case "Note":
					pe.IconId = PwIcon.Notepad;
					break;

				case "Passport":
					MsAppend(pe, PwDefs.UserNameField, vLine, ++i, pd);
					MsAppend(pe, PwDefs.PasswordField, vLine, ++i, pd);
					MsAppend(pe, "Type", vLine, ++i, pd);
					MsAppend(pe, "Issuing Country", vLine, ++i, pd);
					MsAppend(pe, "Issuing Authority", vLine, ++i, pd);
					MsAppend(pe, "Nationality", vLine, ++i, pd);

					++i;
					if((vLine.Length > i) && StrUtil.TryParseDateTime(
						vLine[i], out dt))
					{
						pe.Expires = true;
						pe.ExpiryTime = TimeUtil.ToUtc(dt, false);
					}
					else MsAppend(pe, "Expiration", vLine, i, pd);

					MsAppend(pe, "Place of Birth", vLine, ++i, pd);

					pe.IconId = PwIcon.Certificate;
					break;

				case "Prescriptions":
					MsAppend(pe, "RX Number", vLine, ++i, pd);
					MsAppend(pe, PwDefs.UserNameField, vLine, ++i, pd);
					MsAppend(pe, "Doctor", vLine, ++i, pd);
					MsAppend(pe, "Pharmacy", vLine, ++i, pd);
					MsAppend(pe, "Phone No.", vLine, ++i, pd);
					break;

				case "Registration Codes":
					MsAppend(pe, PwDefs.UserNameField, vLine, ++i, pd);
					MsAppend(pe, "Date", vLine, ++i, pd);

					pe.IconId = PwIcon.UserKey;
					break;

				case "Unassigned":
					MsAppend(pe, "Field 1", vLine, ++i, pd);
					MsAppend(pe, "Field 2", vLine, ++i, pd);
					MsAppend(pe, "Field 3", vLine, ++i, pd);
					MsAppend(pe, "Field 4", vLine, ++i, pd);
					MsAppend(pe, "Field 5", vLine, ++i, pd);
					MsAppend(pe, "Field 6", vLine, ++i, pd);
					break;

				case "Vehicle Info":
					MsAppend(pe, PwDefs.UserNameField, vLine, ++i, pd);
					MsAppend(pe, PwDefs.PasswordField, vLine, ++i, pd);
					MsAppend(pe, "Date Purchased", vLine, ++i, pd);
					MsAppend(pe, "Tire Size", vLine, ++i, pd);
					break;

				case "Web Logins":
					MsAppend(pe, PwDefs.UrlField, vLine, ++i, pd);
					MsAppend(pe, PwDefs.UserNameField, vLine, ++i, pd);
					MsAppend(pe, PwDefs.PasswordField, vLine, ++i, pd);
					break;

				default:
					Debug.Assert(false);
					break;
			}
			Debug.Assert((i + 1) == vLine.Length);
		}

		private static void MsAppend(PwEntry pe, string strFieldName,
			string[] vLine, int iIndex, PwDatabase pdContext)
		{
			if(iIndex >= vLine.Length) { Debug.Assert(false); return; }

			string strValue = vLine[iIndex];
			if(string.IsNullOrEmpty(strValue)) return;

			strValue = strValue.Replace("\\r\\n", "\\n");
			strValue = strValue.Replace("\\r", "\\n");

			if(PwDefs.IsStandardField(strFieldName) &&
				(strFieldName != PwDefs.NotesField))
			{
				while(strValue.EndsWith("\\n"))
				{
					strValue = strValue.Substring(0, strValue.Length - 2);
				}

				strValue = strValue.Replace("\\n", ", ");
			}
			else strValue = strValue.Replace("\\n", MessageService.NewLine);

			ImportUtil.AppendToField(pe, strFieldName, strValue, pdContext);
		}
	}
}
