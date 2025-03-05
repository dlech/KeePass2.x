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
	// 1.16
	internal sealed class Whisper32Csv116 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Whisper 32 CSV"; } }
		public override string DefaultExtension { get { return "csv"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override bool ImportAppendsToRootGroupOnly { get { return true; } }

		public override void Import(PwDatabase pdStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			string strDocument = MemUtil.ReadString(sInput, Encoding.Default);

			PwGroup pg = pdStorage.RootGroup;

			CharStream cs = new CharStream(strDocument);
			string[] vFields = new string[7];
			char[] vDateFieldSplitter = new char[] { '/' };
			char[] vDateZeroTrim = new char[] { '0' };

			bool bFirst = true;
			while(true)
			{
				bool bSubZero = false;
				for(int iField = 0; iField < vFields.Length; ++iField)
				{
					vFields[iField] = ReadCsvField(cs);

					if((iField > 0) && (vFields[iField] == null))
						bSubZero = true;
				}
				if(vFields[0] == null) break; // Import successful
				else if(bSubZero) throw new FormatException();

				if(bFirst)
				{
					bFirst = false; // Check first line once only

					if((vFields[0] != "ServiceName") || (vFields[1] != "UserName") ||
						(vFields[2] != "Password") || (vFields[3] != "Memo") ||
						(vFields[4] != "Expire") || (vFields[5] != "StartDate") ||
						(vFields[6] != "DaysToLive"))
					{
						Debug.Assert(false);
						throw new FormatException();
					}
					else continue;
				}

				PwEntry pe = new PwEntry(true, true);
				pg.AddEntry(pe, true);

				ImportUtil.Add(pe, PwDefs.TitleField, vFields[0], pdStorage);
				ImportUtil.Add(pe, PwDefs.UserNameField, vFields[1], pdStorage);
				ImportUtil.Add(pe, PwDefs.PasswordField, vFields[2], pdStorage);
				ImportUtil.Add(pe, PwDefs.NotesField, vFields[3], pdStorage);

				pe.Expires = (vFields[4] == "true");

				try
				{
					string[] vDateParts = vFields[5].Split(vDateFieldSplitter);
					DateTime dt = (new DateTime(
						int.Parse(vDateParts[2].TrimStart(vDateZeroTrim)),
						int.Parse(vDateParts[0].TrimStart(vDateZeroTrim)),
						int.Parse(vDateParts[1].TrimStart(vDateZeroTrim)),
						0, 0, 0, DateTimeKind.Local)).ToUniversalTime();
					pe.LastModificationTime = dt;
					pe.LastAccessTime = dt;
					pe.ExpiryTime = dt.AddDays(double.Parse(vFields[6]));
				}
				catch(Exception) { Debug.Assert(false); }

				ImportUtil.Add(pe, "Days To Live", vFields[6], pdStorage);
			}
		}

		private static string ReadCsvField(CharStream cs)
		{
			StringBuilder sbValue = new StringBuilder();
			char ch;

			while(true)
			{
				ch = cs.ReadChar();
				if(ch == char.MinValue) return null;
				else if(ch == '\"') break;
			}

			while(true)
			{
				ch = cs.ReadChar();

				if(ch == char.MinValue)
					return null;
				else if(ch == '\r')
					continue;
				else if(ch == '\"')
				{
					char chSucc = cs.ReadChar();

					if(chSucc == '\"') sbValue.Append('\"');
					else break;
				}
				else if(ch == '\n')
					sbValue.Append(MessageService.NewLine);
				else sbValue.Append(ch);
			}

			return sbValue.ToString();
		}
	}
}
