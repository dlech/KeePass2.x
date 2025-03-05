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
	// 4.50
	internal sealed class PinsTxt450 : FileFormatProvider
	{
		private const string FirstLine = "\"Category\"\t\"System\"\t\"User\"\t" +
			"\"Password\"\t\"URL/Comments\"\t\"Custom\"\t\"Start date\"\t\"Expires\"\t" +
			"\"More info\"";
		private const string FieldSeparator = "\"\t\"";

		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "PINs TXT"; } }
		public override string DefaultExtension { get { return "txt"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override void Import(PwDatabase pdStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			string strData = MemUtil.ReadString(sInput, Encoding.Default);

			string[] vLines = strData.Split(new char[] { '\r', '\n' });

			bool bFirst = true;
			foreach(string strLine in vLines)
			{
				if(bFirst)
				{
					if(strLine != FirstLine)
						throw new FormatException("Format error. First line is invalid. Read the documentation.");

					bFirst = false;
				}
				else if(strLine.Length > 5) ImportLine(strLine, pdStorage);
			}
		}

		private static void ImportLine(string strLine, PwDatabase pd)
		{
			string[] vParts = strLine.Split(new string[] { FieldSeparator },
				StringSplitOptions.None);
			Debug.Assert(vParts.Length == 9);
			if(vParts.Length != 9)
				throw new FormatException("Line:\r\n" + strLine);

			vParts[0] = vParts[0].Remove(0, 1);
			vParts[8] = vParts[8].Substring(0, vParts[8].Length - 1);

			vParts[8] = vParts[8].Replace("||", "\r\n");

			PwGroup pg = pd.RootGroup.FindCreateGroup(vParts[0], true);
			PwEntry pe = new PwEntry(true, true);
			pg.AddEntry(pe, true);

			ImportUtil.Add(pe, PwDefs.TitleField, vParts[1], pd);
			ImportUtil.Add(pe, PwDefs.UserNameField, vParts[2], pd);
			ImportUtil.Add(pe, PwDefs.PasswordField, vParts[3], pd);
			ImportUtil.Add(pe, PwDefs.UrlField, vParts[4], pd);

			if(vParts[5].Length > 0)
				pe.Strings.Set("Custom", new ProtectedString(false, vParts[5]));

			DateTime dt;
			if((vParts[6].Length > 0) && DateTime.TryParse(vParts[6], out dt))
				pe.CreationTime = pe.LastModificationTime = pe.LastAccessTime =
					TimeUtil.ToUtc(dt, false);

			if((vParts[7].Length > 0) && DateTime.TryParse(vParts[7], out dt))
			{
				pe.ExpiryTime = TimeUtil.ToUtc(dt, false);
				pe.Expires = true;
			}

			ImportUtil.Add(pe, PwDefs.NotesField, vParts[8], pd);
		}
	}
}
