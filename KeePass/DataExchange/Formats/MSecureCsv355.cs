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
using KeePass.Util;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	// 3.5.5-6.1+
	internal sealed class MSecureCsv355 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "mSecure CSV"; } }
		public override string DefaultExtension { get { return "csv"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override bool ImportAppendsToRootGroupOnly { get { return false; } }

		public override void Import(PwDatabase pdStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			byte[] pb = MemUtil.Read(sInput);
			int cC = MemUtil.Count(pb, (byte)',');
			int cSC = MemUtil.Count(pb, (byte)';');
			bool bVersion6 = (cC >= cSC);

			string str;
			if(bVersion6) str = StrUtil.Utf8.GetString(pb);
			else str = Encoding.Default.GetString(pb);

			CsvOptions opt = new CsvOptions();
			opt.FieldSeparator = (bVersion6 ? ',' : ';');

			// Backslashes are not escaped, even though "\\n" is used
			// to encode new-line characters
			opt.BackslashIsEscape = false;

			CsvStreamReaderEx csr = new CsvStreamReaderEx(str, opt);
			Dictionary<string, PwGroup> dGroups = new Dictionary<string, PwGroup>();

			while(true)
			{
				string[] vLine = csr.ReadLine();
				if(vLine == null) break;

				if(bVersion6) AddEntry6(vLine, pdStorage);
				else AddEntry3(vLine, pdStorage, dGroups);
			}
		}

		private static void AddEntry3(string[] vLine, PwDatabase pd,
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

			Append(pe, PwDefs.TitleField, vLine, 2, pd);
			Append(pe, PwDefs.NotesField, vLine, 3, pd);

			string strType = vLine[1];
			int i = 3;
			DateTime dt;

			switch(strType)
			{
				case "Bank Accounts":
					Append(pe, PwDefs.UserNameField, vLine, ++i, pd);
					Append(pe, PwDefs.PasswordField, vLine, ++i, pd);
					Append(pe, "Name", vLine, ++i, pd);
					Append(pe, "Branch", vLine, ++i, pd);
					Append(pe, "Phone No.", vLine, ++i, pd);

					pe.IconId = PwIcon.Money;
					break;

				case "Birthdays":
					Append(pe, PwDefs.UserNameField, vLine, ++i, pd);

					pe.IconId = PwIcon.UserCommunication;
					break;

				case "Calling Cards":
				case "Social Security":
				case "Voice Mail":
					Append(pe, PwDefs.UserNameField, vLine, ++i, pd);
					Append(pe, PwDefs.PasswordField, vLine, ++i, pd);

					pe.IconId = PwIcon.UserKey;
					break;

				case "Clothes Size":
					Append(pe, "Shirt Size", vLine, ++i, pd);
					Append(pe, "Pant Size", vLine, ++i, pd);
					Append(pe, "Shoe Size", vLine, ++i, pd);
					Append(pe, "Dress Size", vLine, ++i, pd);
					break;

				case "Combinations":
					Append(pe, PwDefs.PasswordField, vLine, ++i, pd);
					break;

				case "Credit Cards":
					Append(pe, PwDefs.UserNameField, vLine, ++i, pd);

					++i;
					if((vLine.Length > i) && StrUtil.TryParseDateTime(
						vLine[i], out dt))
					{
						pe.Expires = true;
						pe.ExpiryTime = TimeUtil.ToUtc(dt, false);
					}
					else Append(pe, "Expiration", vLine, i, pd);

					Append(pe, "Name", vLine, ++i, pd);
					Append(pe, PwDefs.PasswordField, vLine, ++i, pd);
					Append(pe, "Bank", vLine, ++i, pd);
					Append(pe, "Security Code", vLine, ++i, pd);

					pe.IconId = PwIcon.Money;
					break;

				case "Email Accounts":
					Append(pe, PwDefs.UserNameField, vLine, ++i, pd);
					Append(pe, PwDefs.PasswordField, vLine, ++i, pd);
					Append(pe, "POP3 Host", vLine, ++i, pd);
					Append(pe, "SMTP Host", vLine, ++i, pd);

					pe.IconId = PwIcon.EMail;
					break;

				case "Frequent Flyer":
					Append(pe, "Number", vLine, ++i, pd);
					Append(pe, PwDefs.UrlField, vLine, ++i, pd);
					Append(pe, PwDefs.UserNameField, vLine, ++i, pd);
					Append(pe, PwDefs.PasswordField, vLine, ++i, pd);
					Append(pe, "Mileage", vLine, ++i, pd);
					break;

				case "Identity":
					++i;
					Append(pe, PwDefs.UserNameField, vLine, i + 1, pd); // Last name
					Append(pe, PwDefs.UserNameField, vLine, i, pd); // First name
					++i;

					Append(pe, "Nick Name", vLine, ++i, pd);
					Append(pe, "Company", vLine, ++i, pd);
					Append(pe, PwDefs.UserNameField, vLine, ++i, pd);
					Append(pe, "Address", vLine, ++i, pd);
					Append(pe, "Address 2", vLine, ++i, pd);
					Append(pe, "City", vLine, ++i, pd);
					Append(pe, "State", vLine, ++i, pd);
					Append(pe, "Country", vLine, ++i, pd);
					Append(pe, "Zip", vLine, ++i, pd);
					Append(pe, "Home Phone", vLine, ++i, pd);
					Append(pe, "Office Phone", vLine, ++i, pd);
					Append(pe, "Mobile Phone", vLine, ++i, pd);
					Append(pe, "E-Mail", vLine, ++i, pd);
					Append(pe, "E-Mail 2", vLine, ++i, pd);
					Append(pe, "Skype", vLine, ++i, pd);
					Append(pe, PwDefs.UrlField, vLine, ++i, pd);

					pe.IconId = PwIcon.UserCommunication;
					break;

				case "Insurance":
					Append(pe, PwDefs.UserNameField, vLine, ++i, pd);
					Append(pe, "Group No.", vLine, ++i, pd);
					Append(pe, "Insured", vLine, ++i, pd);
					Append(pe, "Date", vLine, ++i, pd);
					Append(pe, "Phone No.", vLine, ++i, pd);

					pe.IconId = PwIcon.Certificate;
					break;

				case "Memberships":
					Append(pe, PwDefs.PasswordField, vLine, ++i, pd);
					Append(pe, PwDefs.UserNameField, vLine, ++i, pd);
					Append(pe, "Date", vLine, ++i, pd);

					pe.IconId = PwIcon.UserKey;
					break;

				case "Note":
					pe.IconId = PwIcon.Notepad;
					break;

				case "Passport":
					Append(pe, PwDefs.UserNameField, vLine, ++i, pd);
					Append(pe, PwDefs.PasswordField, vLine, ++i, pd);
					Append(pe, "Type", vLine, ++i, pd);
					Append(pe, "Issuing Country", vLine, ++i, pd);
					Append(pe, "Issuing Authority", vLine, ++i, pd);
					Append(pe, "Nationality", vLine, ++i, pd);

					++i;
					if((vLine.Length > i) && StrUtil.TryParseDateTime(
						vLine[i], out dt))
					{
						pe.Expires = true;
						pe.ExpiryTime = TimeUtil.ToUtc(dt, false);
					}
					else Append(pe, "Expiration", vLine, i, pd);

					Append(pe, "Place of Birth", vLine, ++i, pd);

					pe.IconId = PwIcon.Certificate;
					break;

				case "Prescriptions":
					Append(pe, "RX Number", vLine, ++i, pd);
					Append(pe, PwDefs.UserNameField, vLine, ++i, pd);
					Append(pe, "Doctor", vLine, ++i, pd);
					Append(pe, "Pharmacy", vLine, ++i, pd);
					Append(pe, "Phone No.", vLine, ++i, pd);
					break;

				case "Registration Codes":
					Append(pe, PwDefs.UserNameField, vLine, ++i, pd);
					Append(pe, "Date", vLine, ++i, pd);

					pe.IconId = PwIcon.UserKey;
					break;

				case "Unassigned":
					Append(pe, "Field 1", vLine, ++i, pd);
					Append(pe, "Field 2", vLine, ++i, pd);
					Append(pe, "Field 3", vLine, ++i, pd);
					Append(pe, "Field 4", vLine, ++i, pd);
					Append(pe, "Field 5", vLine, ++i, pd);
					Append(pe, "Field 6", vLine, ++i, pd);
					break;

				case "Vehicle Info":
					Append(pe, PwDefs.UserNameField, vLine, ++i, pd);
					Append(pe, PwDefs.PasswordField, vLine, ++i, pd);
					Append(pe, "Date Purchased", vLine, ++i, pd);
					Append(pe, "Tire Size", vLine, ++i, pd);
					break;

				case "Web Logins":
					Append(pe, PwDefs.UrlField, vLine, ++i, pd);
					Append(pe, PwDefs.UserNameField, vLine, ++i, pd);
					Append(pe, PwDefs.PasswordField, vLine, ++i, pd);
					break;

				default:
					Debug.Assert(false);
					break;
			}
			Debug.Assert((i + 1) == vLine.Length);
		}

		private static string Unescape(string str, bool bAllowNewLine)
		{
			if(string.IsNullOrEmpty(str)) return string.Empty;

			const string strBbnPlh = "L_6e/XMVY)<mWyDxN}0x#Dg9";
			str = str.Replace("\\\\n", strBbnPlh);

			if(bAllowNewLine) str = str.Replace("\\n", MessageService.NewLine);
			else
			{
				Debug.Assert(!str.Contains("\\n"));
				while(str.EndsWith("\\n")) { str = str.Substring(0, str.Length - 2); }
				str = str.Replace("\\n", ", ");
			}

			return str.Replace(strBbnPlh, "\\n");
		}

		private static void Append(PwEntry pe, string strFieldName,
			string[] vLine, int iIndex, PwDatabase pdContext)
		{
			if(iIndex >= vLine.Length) { Debug.Assert(false); return; }

			string strValue = vLine[iIndex];
			if(string.IsNullOrEmpty(strValue)) return;

			strValue = Unescape(strValue, PwDefs.IsMultiLineField(strFieldName));

			ImportUtil.AppendToField(pe, strFieldName, strValue, pdContext);
		}

		private static void AddEntry6(string[] vLine, PwDatabase pd)
		{
			if(vLine.Length < 4) return;

			PwEntry pe = new PwEntry(true, true);
			pd.RootGroup.AddEntry(pe, true);

			string[] v = Split(vLine[0]);
			Debug.Assert(v.Length == 2);
			ImportUtil.Add(pe, PwDefs.TitleField, Unescape(v[0], false), pd);

			pe.Tags = StrUtil.StringToTags(Unescape(vLine[2], false));

			ImportUtil.Add(pe, PwDefs.NotesField, Unescape(vLine[3], true), pd);

			for(int i = 4; i < vLine.Length; ++i)
			{
				v = Split(vLine[i]);

				if(v.Length == 3)
				{
					string strName = Unescape(v[0], false);
					if(strName.Length == 0) strName = PwDefs.NotesField;

					int iType;
					if(int.TryParse(v[1], out iType))
					{
						if(iType == 13)
						{
							try
							{
								EntryUtil.ImportOtpAuth(pe, v[2], pd);
								continue;
							}
							catch(Exception) { } // Import as field
						}

						string strStdName = null;
						switch(iType)
						{
							case 2: strStdName = PwDefs.UrlField; break;
							case 5:
							case 7: strStdName = PwDefs.UserNameField; break;
							case 8:
							case 9: strStdName = PwDefs.PasswordField; break;
							default: Debug.Assert((iType >= 0) && (iType <= 13)); break;
						}

						if(strStdName != null)
						{
							ProtectedString ps = pe.Strings.Get(strStdName);
							if((ps == null) || ps.IsEmpty) strName = strStdName;
						}
					}
					else { Debug.Assert(false); }

					string strValue = Unescape(v[2], PwDefs.IsMultiLineField(strName));

					ImportUtil.Add(pe, strName, strValue, pd);
				}
				else
				{
					Debug.Assert(false);
					ImportUtil.Add(pe, PwDefs.NotesField, Unescape(vLine[i], true), pd);
				}
			}
		}

		private static string[] Split(string str)
		{
			if(string.IsNullOrEmpty(str)) return new string[1] { string.Empty };

			const string strSepPlh = "J+*GB]-yct6WjmL-f}%p@Q4A";
			str = str.Replace("||", strSepPlh);

			string[] v = str.Split('|');
			if(v.Length == 0) { Debug.Assert(false); return new string[1] { string.Empty }; }

			for(int i = 0; i < v.Length; ++i)
				v[i] = v[i].Replace(strSepPlh, "|");

			return v;
		}
	}
}
