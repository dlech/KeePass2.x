/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2022 Dominik Reichl <dominik.reichl@t-online.de>

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
	// 9.0.2+
	internal sealed class KasperskyPwMgrTxt90 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Kaspersky Password Manager TXT"; } }
		public override string DefaultExtension { get { return "txt"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			string strAll;
			using(StreamReader sr = new StreamReader(sInput, Encoding.UTF8, true))
			{
				strAll = sr.ReadToEnd();
			}

			// Fix new-line sequences and normalize to '\n'
			strAll = strAll.Replace("\r\r\n", "\r\n");
			strAll = StrUtil.NormalizeNewLines(strAll, false);

			string[] vBlocks = strAll.Split(new string[] { "\n\n---\n\n" },
				StringSplitOptions.None);
			PwGroup pg = pwStorage.RootGroup;

			foreach(string strBlock in vBlocks)
				ImportBlock(pwStorage, ref pg, strBlock);
		}

		private static void ImportBlock(PwDatabase pd, ref PwGroup pg, string strBlock)
		{
			if(strBlock == null) { Debug.Assert(false); return; }
			strBlock = strBlock.Trim();
			if(strBlock.Length == 0) return;

			string strFirstLine = strBlock;
			int iNL = strBlock.IndexOf('\n');
			if(iNL >= 0) strFirstLine = strBlock.Substring(0, iNL).Trim();

			if(strFirstLine.IndexOf(':') < 0)
			{
				if(strFirstLine.Length == 0) { Debug.Assert(false); pg = pd.RootGroup; }
				else
				{
					pg = new PwGroup(true, true);
					pg.Name = strFirstLine;

					if(strFirstLine.Equals("Websites", StrUtil.CaseIgnoreCmp))
						pg.IconId = PwIcon.World;
					else if(strFirstLine.Equals("Applications", StrUtil.CaseIgnoreCmp))
						pg.IconId = PwIcon.Run;
					else if(strFirstLine.Equals("Notes", StrUtil.CaseIgnoreCmp))
						pg.IconId = PwIcon.Note;

					pd.RootGroup.AddGroup(pg, true);
				}

				if(iNL < 0) return; // Group without entry
				strBlock = strBlock.Substring(iNL + 1).Trim();
			}

			if(strBlock.Length != 0)
			{
				string[] vLines = strBlock.Split('\n');
				ImportEntry(pd, pg, vLines);
			}
		}

		private static void ImportEntry(PwDatabase pd, PwGroup pg, string[] vLines)
		{
			PwEntry pe = new PwEntry(true, true);
			bool bFoundData = false;

			StringBuilder sbNotes = new StringBuilder();
			bool bInNotes = false;

			foreach(string strLine in vLines)
			{
				if(bInNotes) { sbNotes.AppendLine(strLine); continue; }

				int iSep = strLine.IndexOf(':');
				if(iSep < 0)
				{
					Debug.Assert(false);
					string str = strLine.Trim();
					if(str.Length != 0) sbNotes.AppendLine(str);
					continue;
				}

				string strKey = strLine.Substring(0, iSep).Trim();
				if(strKey.Length == 0)
				{
					Debug.Assert(false);
					strKey = PwDefs.NotesField;
				}

				string strValue = strLine.Substring(iSep + 1).Trim();

				bFoundData |= (strValue.Length != 0);

				string strKeyL = strKey.ToLowerInvariant();
				switch(strKeyL)
				{
					case "website name":
					case "login name": // Name of a user/password pair
					case "application":
					case "name": // Secure note
						strKey = PwDefs.TitleField;
						break;

					case "login":
						strKey = PwDefs.UserNameField;
						break;

					case "password":
						strKey = PwDefs.PasswordField;
						break;

					case "website url":
						strKey = PwDefs.UrlField;
						break;

					case "comment":
					case "text": // Secure note
						strKey = PwDefs.NotesField;
						break;

					default:
						Debug.Assert(false);
						string strMapped = ImportUtil.MapNameToStandardField(
							strKey, true);
						if(!string.IsNullOrEmpty(strMapped))
							strKey = strMapped;
						break;
				}

				if(strKey == PwDefs.NotesField)
				{
					sbNotes.AppendLine(strValue);
					bInNotes = true;
				}
				else ImportUtil.AppendToField(pe, strKey, strValue, pd);
			}

			string strNotes = sbNotes.ToString().Trim();
			ImportUtil.AppendToField(pe, PwDefs.NotesField, strNotes, pd);

			if(bFoundData) pg.AddEntry(pe, true);
		}
	}
}
