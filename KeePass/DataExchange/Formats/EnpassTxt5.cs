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

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	// 5.3.0.1-6.0.4+
	internal sealed class EnpassTxt5 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Enpass TXT"; } }
		public override string DefaultExtension { get { return "txt"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override bool ImportAppendsToRootGroupOnly { get { return true; } }

		public override void Import(PwDatabase pdStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			string strData = MemUtil.ReadString(sInput, StrUtil.Utf8);

			PwGroup pg = pdStorage.RootGroup;
			const string strSep = ": "; // Both ": " and " : " are used

			strData = StrUtil.NormalizeNewLines(strData, false);

			int iSep = strData.IndexOf(strSep);
			if(iSep < 0) throw new FormatException();
			string strTitleKey = strData.Substring(0, iSep).Trim();
			if(strTitleKey.Length == 0) throw new FormatException();
			if(strTitleKey.IndexOf('\n') >= 0) throw new FormatException();

			PwEntry pe = null;
			string strName = PwDefs.TitleField;

			string[] vLines = strData.Split('\n');
			foreach(string strLine in vLines)
			{
				if(strLine == null) { Debug.Assert(false); continue; }
				// Do not trim strLine, otherwise strSep might not be detected

				string strValue = strLine;

				iSep = strLine.IndexOf(strSep);
				if(iSep >= 0)
				{
					string strCurName = strLine.Substring(0, iSep).Trim();
					strValue = strLine.Substring(iSep + strSep.Length);

					if(strCurName == strTitleKey)
					{
						FinishEntry(pe);

						pe = new PwEntry(true, true);
						pg.AddEntry(pe, true);

						strName = PwDefs.TitleField;
					}
					else if(strName == PwDefs.NotesField)
						strValue = strLine; // Restore
					else
					{
						strName = ImportUtil.MapName(strCurName, true);
						if(string.IsNullOrEmpty(strName))
						{
							Debug.Assert(false);
							strName = PwDefs.NotesField;
							strValue = strLine; // Restore
						}
					}
				}

				if(pe != null)
				{
					strValue = strValue.Trim();

					if(strValue.Length != 0)
						ImportUtil.AppendToField(pe, strName, strValue, pdStorage);
					else if(strName == PwDefs.NotesField)
					{
						ProtectedString ps = pe.Strings.GetSafe(strName);
						pe.Strings.Set(strName, ps + MessageService.NewLine);
					}
				}
				else { Debug.Assert(false); }
			}

			FinishEntry(pe);
		}

		private static void FinishEntry(PwEntry pe)
		{
			if(pe == null) return;

			List<string> lKeys = pe.Strings.GetKeys();
			foreach(string strKey in lKeys)
			{
				ProtectedString ps = pe.Strings.GetSafe(strKey);
				pe.Strings.Set(strKey, new ProtectedString(
					ps.IsProtected, ps.ReadString().Trim()));
			}
		}
	}
}
