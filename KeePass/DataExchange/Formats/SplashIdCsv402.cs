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
using System.Text;
using System.Drawing;
using System.IO;
using System.Diagnostics;

using KeePass.Resources;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	// 3.4-5.3+, types from web 2016-12 (version 8.1.1.925)
	internal sealed class SplashIdCsv402 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "SplashID CSV"; } }
		public override string DefaultExtension { get { return "csv"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }
		
		public override bool ImportAppendsToRootGroupOnly { get { return false; } }

		public override Image SmallIcon
		{
			get { return KeePass.Properties.Resources.B16x16_Imp_SplashID; }
		}

		private const string StrHeader = "SplashID Export File";

		private static SplashIdMapping[] m_vMappings = null;
		private static SplashIdMapping[] SplashIdMappings
		{
			get
			{
				if(m_vMappings != null) return m_vMappings;

				m_vMappings = new SplashIdMapping[] {
					new SplashIdMapping("Addresses", PwIcon.UserCommunication,
						new string[] { PwDefs.TitleField, // Name
							PwDefs.NotesField, // Address
							PwDefs.NotesField, // Address 2
							PwDefs.NotesField, // City
							PwDefs.NotesField, // State
							PwDefs.NotesField, // Zip Code
							PwDefs.NotesField, // Country
							PwDefs.UserNameField, // Email
							PwDefs.NotesField }), // Phone
					new SplashIdMapping("Bank Accounts", PwIcon.Homebanking,
						new string[] { PwDefs.TitleField, PwDefs.UserNameField,
							PwDefs.PasswordField, "Name", "Branch", "Phone #" }),
					new SplashIdMapping("Bank Accts", PwIcon.Homebanking,
						new string[] { PwDefs.TitleField, PwDefs.UserNameField,
							PwDefs.PasswordField, "Name", "Branch", "Phone #" }),
					new SplashIdMapping("Birthdays", PwIcon.UserCommunication,
						new string[] { PwDefs.TitleField, PwDefs.UserNameField }),
					new SplashIdMapping("Calling Cards", PwIcon.UserKey,
						new string[] { PwDefs.TitleField, PwDefs.UserNameField,
							PwDefs.PasswordField }),
					new SplashIdMapping("Clothes Size", PwIcon.UserCommunication,
						new string[] { PwDefs.TitleField, "Shirt Size", "Pant Size",
							"Shoe Size", "Dress Size", "Ring Size" }),
					new SplashIdMapping("Combinations", PwIcon.Key,
						new string[] { PwDefs.TitleField, PwDefs.PasswordField }),
					new SplashIdMapping("Credit Cards", PwIcon.Money,
						new string[] { PwDefs.TitleField, PwDefs.UserNameField,
							"Expiry Date", "Name", PwDefs.PasswordField, "Bank" }),
					new SplashIdMapping("Email Accounts", PwIcon.EMail,
						new string[] { PwDefs.TitleField, PwDefs.UserNameField,
							PwDefs.PasswordField, "POP3 Host", "SMTP Host" }),
					new SplashIdMapping("Email Accts", PwIcon.EMail,
						new string[] { PwDefs.TitleField, PwDefs.UserNameField,
							PwDefs.PasswordField, "POP3 Host", "SMTP Host" }),
					new SplashIdMapping("Emergency Info", PwIcon.UserCommunication,
						new string[] { PwDefs.TitleField, PwDefs.UserNameField }),
					new SplashIdMapping("Files", PwIcon.PaperNew,
						new string[] { PwDefs.TitleField, PwDefs.NotesField,
							PwDefs.UserNameField, "Date" }),
					new SplashIdMapping("Frequent Flyer", PwIcon.PaperQ,
						new string[] { PwDefs.TitleField, PwDefs.UserNameField,
							"Name", "Date" }),
					new SplashIdMapping("Identification", PwIcon.UserKey,
						new string[] { PwDefs.TitleField, PwDefs.PasswordField,
							PwDefs.UserNameField, "Date" }),
					new SplashIdMapping("Insurance", PwIcon.ClipboardReady,
						new string[] { PwDefs.TitleField, PwDefs.PasswordField,
							PwDefs.UserNameField, "Insured", "Date", "Phone #" }),
					new SplashIdMapping("Memberships", PwIcon.UserKey,
						new string[] { PwDefs.TitleField, PwDefs.PasswordField,
							PwDefs.UserNameField, "Date" }),
					new SplashIdMapping("Phone Numbers", PwIcon.UserCommunication,
						new string[] { PwDefs.TitleField, PwDefs.UserNameField }),
					new SplashIdMapping("Prescriptions", PwIcon.ClipboardReady,
						new string[] { PwDefs.TitleField, PwDefs.PasswordField,
							PwDefs.UserNameField, "Doctor", "Pharmacy", "Phone #" }),
					new SplashIdMapping("Serial Numbers", PwIcon.Key,
						new string[] { PwDefs.TitleField, PwDefs.PasswordField,
							"Date", "Reseller" }),
					new SplashIdMapping("Servers", PwIcon.NetworkServer,
						new string[] { PwDefs.TitleField, PwDefs.UserNameField,
							PwDefs.PasswordField, PwDefs.UrlField }),
					new SplashIdMapping("Vehicle Info", PwIcon.PaperReady,
						new string[] { PwDefs.TitleField, PwDefs.UserNameField,
							PwDefs.PasswordField, "Insurance", "Year" }),
					new SplashIdMapping("Vehicles", PwIcon.PaperReady,
						new string[] { PwDefs.TitleField, PwDefs.UserNameField,
							PwDefs.PasswordField, "Insurance", "Year" }),
					new SplashIdMapping("Voice Mail", PwIcon.IRCommunication,
						new string[] { PwDefs.TitleField, PwDefs.UserNameField,
							PwDefs.PasswordField }),
					new SplashIdMapping("Web Logins", PwIcon.Key,
						new string[] { PwDefs.TitleField, PwDefs.UserNameField,
							PwDefs.PasswordField, PwDefs.UrlField })
				};
				return m_vMappings;
			}
		}

		public override void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			StreamReader sr = new StreamReader(sInput, Encoding.Default, true);
			string strData = sr.ReadToEnd();
			sr.Close();

			CsvOptions o = new CsvOptions();
			o.BackslashIsEscape = false;

			CsvStreamReaderEx csv = new CsvStreamReaderEx(strData, o);

			SortedDictionary<string, PwGroup> dictGroups =
				new SortedDictionary<string, PwGroup>();
			while(true)
			{
				string[] vLine = csv.ReadLine();
				if(vLine == null) break;
				if(vLine.Length == 0) continue;

				if(vLine.Length == 1)
				{
					Debug.Assert(vLine[0] == StrHeader);
					continue;
				}

				// Support old version 3.4
				if(vLine.Length == 9)
				{
					string[] v = new string[13];
					for(int i = 0; i < 7; ++i) v[i] = vLine[i];
					for(int i = 7; i < 11; ++i) v[i] = string.Empty;
					v[11] = vLine[7];
					v[12] = vLine[8];

					vLine = v;
				}

				if(vLine.Length == 13)
					ProcessCsvLine(vLine, pwStorage, dictGroups);
				else { Debug.Assert(false); }
			}
		}

		private static void ProcessCsvLine(string[] vLine, PwDatabase pwStorage,
			SortedDictionary<string, PwGroup> dictGroups)
		{
			string strType = ParseCsvWord(vLine[0]);

			string strGroupName = ParseCsvWord(vLine[12]); // + " - " + strType;
			if(strGroupName.Length == 0) strGroupName = strType;

			SplashIdMapping mp = null;
			foreach(SplashIdMapping mpFind in SplashIdCsv402.SplashIdMappings)
			{
				if(mpFind.TypeName == strType)
				{
					mp = mpFind;
					break;
				}
			}

			PwIcon pwIcon = ((mp != null) ? mp.Icon : PwIcon.Key);

			PwGroup pg = null;
			if(dictGroups.ContainsKey(strGroupName))
				pg = dictGroups[strGroupName];
			else
			{
				// PwIcon pwGroupIcon = ((pwIcon == PwIcon.Key) ?
				//	PwIcon.FolderOpen : pwIcon);
				// pg = new PwGroup(true, true, strGroupName, pwGroupIcon);

				pg = new PwGroup(true, true);
				pg.Name = strGroupName;

				pwStorage.RootGroup.AddGroup(pg, true);
				dictGroups[strGroupName] = pg;
			}

			PwEntry pe = new PwEntry(true, true);
			pg.AddEntry(pe, true);

			pe.IconId = pwIcon;

			List<string> vTags = StrUtil.StringToTags(strType);
			foreach(string strTag in vTags) { pe.AddTag(strTag); }

			for(int iField = 0; iField < 9; ++iField)
			{
				string strData = ParseCsvWord(vLine[iField + 1]);
				if(strData.Length == 0) continue;

				string strLookup = ((mp != null) ? mp.FieldNames[iField] :
					null);
				string strField = (strLookup ?? ("Field " + (iField + 1).ToString()));

				ImportUtil.AppendToField(pe, strField, strData, pwStorage);
			}

			ImportUtil.AppendToField(pe, PwDefs.NotesField, ParseCsvWord(vLine[11]),
				pwStorage);

			DateTime? odt = TimeUtil.ParseUSTextDate(ParseCsvWord(vLine[10]),
				DateTimeKind.Local);
			if(odt.HasValue)
			{
				DateTime dt = TimeUtil.ToUtc(odt.Value, false);
				pe.LastAccessTime = dt;
				pe.LastModificationTime = dt;
			}
		}

		private static string ParseCsvWord(string strWord)
		{
			if(strWord == null) { Debug.Assert(false); return string.Empty; }

			string str = strWord;
			str = str.Replace('\u000B', '\n'); // 0x0B = new line
			str = StrUtil.NormalizeNewLines(str, true);
			return str;
		}

		private sealed class SplashIdMapping
		{
			private readonly string m_strTypeName;
			public string TypeName
			{
				get { return m_strTypeName; }
			}

			private readonly PwIcon m_pwIcon;
			public PwIcon Icon
			{
				get { return m_pwIcon; }
			}

			private string[] m_vFieldNames = new string[9];
			public string[] FieldNames
			{
				get { return m_vFieldNames; }
			}

			public SplashIdMapping(string strTypeName, PwIcon pwIcon, string[] vFieldNames)
			{
				Debug.Assert(strTypeName != null);
				if(strTypeName == null) throw new ArgumentNullException("strTypeName");

				Debug.Assert(vFieldNames != null);
				if(vFieldNames == null) throw new ArgumentNullException("vFieldNames");

				m_strTypeName = strTypeName;
				m_pwIcon = pwIcon;

				int nMin = Math.Min(m_vFieldNames.Length, vFieldNames.Length);
				for(int i = 0; i < nMin; ++i)
					m_vFieldNames[i] = vFieldNames[i];
			}
		}
	}
}
