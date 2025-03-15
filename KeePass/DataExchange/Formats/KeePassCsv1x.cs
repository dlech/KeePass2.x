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

using KeePassLib;
using KeePassLib.Delegates;
using KeePassLib.Interfaces;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	internal sealed class KeePassCsv1x : FileFormatProvider
	{
		public override bool SupportsImport { get { return false; } }
		public override bool SupportsExport { get { return true; } }

		public override string FormatName { get { return "KeePass CSV (1.x)"; } }
		public override string DefaultExtension { get { return "csv"; } }
		public override string ApplicationGroup { get { return PwDefs.ShortProductName; } }

		/* public override void Import(PwDatabase pdStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			string strFileContents = MemUtil.ReadString(sInput, StrUtil.Utf8);
			CharStream csSource = new CharStream(strFileContents);

			while(true)
			{
				if(!ReadEntry(pdStorage, csSource)) break;
			}
		}

		private static bool ReadEntry(PwDatabase pd, CharStream csSource)
		{
			PwEntry pe = new PwEntry(true, true);

			string strTitle = ReadCsvField(csSource);
			if(strTitle == null) return false; // No entry available

			string strUser = ReadCsvField(csSource);
			if(strUser == null) throw new InvalidDataException();

			string strPassword = ReadCsvField(csSource);
			if(strPassword == null) throw new InvalidDataException();

			string strUrl = ReadCsvField(csSource);
			if(strUrl == null) throw new InvalidDataException();

			string strNotes = ReadCsvField(csSource);
			if(strNotes == null) throw new InvalidDataException();

			if((strTitle == "Account") && (strUser == "Login Name") &&
				(strPassword == "Password") && (strUrl == "Web Site") &&
				(strNotes == "Comments"))
			{
				return true; // Ignore header entry
			}

			ImportUtil.Add(pe, PwDefs.TitleField, strTitle, pd);
			ImportUtil.Add(pe, PwDefs.UserNameField, strUser, pd);
			ImportUtil.Add(pe, PwDefs.PasswordField, strPassword, pd);
			ImportUtil.Add(pe, PwDefs.UrlField, strUrl, pd);
			ImportUtil.Add(pe, PwDefs.NotesField, strNotes, pd);

			pd.RootGroup.AddEntry(pe, true);
			return true;
		}

		private static string ReadCsvField(CharStream csSource)
		{
			StringBuilder sb = new StringBuilder();
			bool bInField = false;

			while(true)
			{
				char ch = csSource.ReadChar();
				if(ch == char.MinValue)
					return null;

				if((ch == '\"') && !bInField)
					bInField = true;
				else if((ch == '\"') && bInField)
					break;
				else if(ch == '\\')
				{
					char chSub = csSource.ReadChar();
					if(chSub == char.MinValue)
						throw new InvalidDataException();

					sb.Append(chSub);
				}
				else if(bInField)
					sb.Append(ch);
			}

			return sb.ToString();
		} */

		public override bool Export(PwExportInfo pwExportInfo, Stream sOutput,
			IStatusLogger slLogger)
		{
			PwGroup pg = (pwExportInfo.DataGroup ?? ((pwExportInfo.ContextDatabase !=
				null) ? pwExportInfo.ContextDatabase.RootGroup : null));

			using(StreamWriter sw = new StreamWriter(sOutput, StrUtil.Utf8))
			{
				sw.Write("\"Account\",\"Login Name\",\"Password\",\"Web Site\",\"Comments\"\r\n");

				EntryHandler eh = delegate(PwEntry pe)
				{
					WriteCsvEntry(sw, pe);
					return true;
				};

				if(pg != null) pg.TraverseTree(TraversalMethod.PreOrder, null, eh);
			}

			return true;
		}

		private static void WriteCsvEntry(StreamWriter sw, PwEntry pe)
		{
			if(sw == null) { Debug.Assert(false); return; }
			if(pe == null) { Debug.Assert(false); return; }

			const string strSep = "\",\"";

			sw.Write("\"");
			WriteCsvString(sw, pe.Strings.ReadSafe(PwDefs.TitleField), strSep);
			WriteCsvString(sw, pe.Strings.ReadSafe(PwDefs.UserNameField), strSep);
			WriteCsvString(sw, pe.Strings.ReadSafe(PwDefs.PasswordField), strSep);
			WriteCsvString(sw, pe.Strings.ReadSafe(PwDefs.UrlField), strSep);
			WriteCsvString(sw, pe.Strings.ReadSafe(PwDefs.NotesField), "\"\r\n");
		}

		private static void WriteCsvString(StreamWriter sw, string strText,
			string strAppend)
		{
			string str = strText;
			if(!string.IsNullOrEmpty(str))
			{
				str = str.Replace("\\", "\\\\");
				str = str.Replace("\"", "\\\"");

				sw.Write(str);
			}

			if(!string.IsNullOrEmpty(strAppend)) sw.Write(strAppend);
		}
	}
}
