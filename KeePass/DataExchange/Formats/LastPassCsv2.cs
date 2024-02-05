/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2024 Dominik Reichl <dominik.reichl@t-online.de>

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
	// 2.0.2-4.65.0.5+
	internal sealed class LastPassCsv2 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "LastPass CSV"; } }
		public override string DefaultExtension { get { return "csv"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override bool ImportAppendsToRootGroupOnly { get { return false; } }

		public override void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			StreamReader sr = new StreamReader(sInput, StrUtil.Utf8, true);
			string strData = sr.ReadToEnd();
			sr.Close();

			// The Chrome extension of LastPass 4.1.35 encodes some
			// special characters as XML entities; the web version and
			// the Firefox extension do not do this
			strData = strData.Replace(@"&lt;", @"<");
			strData = strData.Replace(@"&gt;", @">");
			strData = strData.Replace(@"&amp;", @"&");

			CsvOptions opt = new CsvOptions();
			opt.BackslashIsEscape = false;

			CsvStreamReaderEx csr = new CsvStreamReaderEx(strData, opt);
			ImportCsv(csr, pwStorage);
		}

		private static void ImportCsv(CsvStreamReaderEx csr, PwDatabase pd)
		{
			CsvTableEntryReader ctr = new CsvTableEntryReader(pd);

			const string strColUrl = "url";

			Predicate<string[]> fIsSecNote = delegate(string[] vRow)
			{
				string str = ctr.GetData(vRow, strColUrl, string.Empty);
				return str.Equals("http://sn", StrUtil.CaseIgnoreCmp);
			};

			ctr.SetDataAppend("name", PwDefs.TitleField);
			ctr.SetDataAppend("username", PwDefs.UserNameField);
			ctr.SetDataAppend("password", PwDefs.PasswordField);

			ctr.SetDataHandler(strColUrl, delegate(string str, PwEntry pe,
				string[] vContextRow)
			{
				if(!fIsSecNote(vContextRow))
					ImportUtil.AppendToField(pe, PwDefs.UrlField, str, pd);
			});

			ctr.SetDataHandler("extra", delegate(string str, PwEntry pe,
				string[] vContextRow)
			{
				if(fIsSecNote(vContextRow) && str.StartsWith("NoteType:",
					StrUtil.CaseIgnoreCmp))
				{
					AddNoteFields(pe, str, pd);
					return;
				}

				ImportUtil.AppendToField(pe, PwDefs.NotesField, str, pd);
			});

			ctr.SetDataHandler("grouping", delegate(string str, PwEntry pe,
				string[] vContextRow)
			{
				if(str.Length == 0) return;

				PwGroup pg = pd.RootGroup.FindCreateSubTree(str,
					new string[1] { "\\" }, true);
				pg.AddEntry(pe, true);
			});

			ctr.SetDataHandler("fav", delegate(string str, PwEntry pe,
				string[] vContextRow)
			{
				if(StrUtil.StringToBool(str)) pe.AddTag(PwDefs.FavoriteTag);
			});

			ctr.Read(csr);
		}

		private static void AddNoteFields(PwEntry pe, string strNotes,
			PwDatabase pd)
		{
			string strData = StrUtil.NormalizeNewLines(strNotes, false);
			string[] vLines = strData.Split('\n');

			string strFieldName = PwDefs.NotesField;
			bool bNotesFound = false;
			foreach(string strLine in vLines)
			{
				int iFieldLen = strLine.IndexOf(':');
				int iDataOffset = 0;
				if((iFieldLen > 0) && !bNotesFound)
				{
					string strRaw = strLine.Substring(0, iFieldLen).Trim();
					string strField = ImportUtil.MapNameToStandardField(strRaw, false);
					if(string.IsNullOrEmpty(strField)) strField = strRaw;

					if(strField.Length != 0)
					{
						strFieldName = strField;
						iDataOffset = iFieldLen + 1;

						bNotesFound |= (strRaw == "Notes"); // Not PwDefs.NotesField
					}
				}

				ImportUtil.AppendToField(pe, strFieldName, strLine.Substring(
					iDataOffset), pd);
			}
		}
	}
}
