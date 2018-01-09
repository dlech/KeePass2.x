/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2018 Dominik Reichl <dominik.reichl@t-online.de>

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
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	// 5.3.0.1+
	internal sealed class EnpassTxt5 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Enpass TXT"; } }
		public override string DefaultExtension { get { return "txt"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override bool ImportAppendsToRootGroupOnly { get { return true; } }

		public override Image SmallIcon
		{
			get { return KeePass.Properties.Resources.B16x16_Imp_Enpass; }
		}

		public override void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			StreamReader sr = new StreamReader(sInput, StrUtil.Utf8, true);
			string strData = sr.ReadToEnd();
			sr.Close();

			const string strKvpSep = " : ";
			PwGroup pg = pwStorage.RootGroup;

			strData = StrUtil.NormalizeNewLines(strData, false);
			strData += ("\n\nTitle" + strKvpSep);

			string[] vLines = strData.Split('\n');
			bool bLastWasEmpty = true;
			string strName = PwDefs.TitleField;
			PwEntry pe = null;
			string strNotes = string.Empty;

			// Do not trim spaces, because these are part of the
			// key-value separator
			char[] vTrimLine = new char[] { '\t', '\r', '\n' };

			foreach(string strLine in vLines)
			{
				if(strLine == null) { Debug.Assert(false); continue; }

				string str = strLine.Trim(vTrimLine);
				string strValue = str;

				int iKvpSep = str.IndexOf(strKvpSep);
				if(iKvpSep >= 0)
				{
					string strOrgName = str.Substring(0, iKvpSep).Trim();
					strValue = str.Substring(iKvpSep + strKvpSep.Length).Trim();

					// If an entry doesn't have any notes, the next entry
					// may start without an empty line; in this case we
					// detect the new entry by the field name "Title"
					// (which apparently is not translated by Enpass)
					if(bLastWasEmpty || (strOrgName == "Title"))
					{
						if(pe != null)
						{
							strNotes = strNotes.Trim();
							pe.Strings.Set(PwDefs.NotesField, new ProtectedString(
								pwStorage.MemoryProtection.ProtectNotes, strNotes));
							strNotes = string.Empty;

							pg.AddEntry(pe, true);
						}

						pe = new PwEntry(true, true);
					}

					strName = ImportUtil.MapNameToStandardField(strOrgName, true);
					if(string.IsNullOrEmpty(strName))
					{
						strName = strOrgName;

						if(string.IsNullOrEmpty(strName))
						{
							Debug.Assert(false);
							strName = PwDefs.NotesField;
						}
					}
				}

				if(strName == PwDefs.NotesField)
				{
					if(strNotes.Length > 0) strNotes += MessageService.NewLine;
					strNotes += strValue;
				}
				else ImportUtil.AppendToField(pe, strName, strValue, pwStorage);

				bLastWasEmpty = (str.Length == 0);
			}
		}
	}
}
