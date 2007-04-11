/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2007 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Diagnostics;
using System.Drawing;
using System.IO;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	public sealed class KeePassCsv1x : FormatImporter
	{
		public override string FormatName { get { return "KeePass CSV (1.x)"; } }
		public override string DefaultExtension { get { return "csv"; } }
		public override string AppGroup { get { return PwDefs.ShortProductName; } }

		public override bool AppendsToRootGroupOnly { get { return true; } }

		public override Image SmallIcon
		{
			get { return KeePass.Properties.Resources.B16x16_KeePass; }
		}

		public override void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			StreamReader sr = new StreamReader(sInput, Encoding.UTF8);
			string strFileContents = sr.ReadToEnd();
			sr.Close();

			CharStream csSource = new CharStream(strFileContents);

			while(true)
			{
				if(ReadEntry(pwStorage, csSource) == false)
					break;
			}
		}

		private static bool ReadEntry(PwDatabase pwStorage, CharStream csSource)
		{
			PwEntry pe = new PwEntry(pwStorage.RootGroup, true, true);

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

			pe.Strings.Set(PwDefs.TitleField, new ProtectedString(
				pwStorage.MemoryProtection.ProtectTitle, strTitle));
			pe.Strings.Set(PwDefs.UserNameField, new ProtectedString(
				pwStorage.MemoryProtection.ProtectUserName, strUser));
			pe.Strings.Set(PwDefs.PasswordField, new ProtectedString(
				pwStorage.MemoryProtection.ProtectPassword, strPassword));
			pe.Strings.Set(PwDefs.UrlField, new ProtectedString(
				pwStorage.MemoryProtection.ProtectUrl, strUrl));
			pe.Strings.Set(PwDefs.NotesField, new ProtectedString(
				pwStorage.MemoryProtection.ProtectNotes, strNotes));

			pwStorage.RootGroup.Entries.Add(pe);
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
		}
	}
}
