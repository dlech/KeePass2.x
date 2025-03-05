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
using System.IO;
using System.Text;

using KeePass.Resources;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	// 1.4
	internal sealed class PVaultTxt14 : FileFormatProvider
	{
		private const string InitGroup = "************";
		private const string InitNewEntry = "----------------------";

		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Personal Vault TXT"; } }
		public override string DefaultExtension { get { return "txt"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override void Import(PwDatabase pdStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			string strData = MemUtil.ReadString(sInput, Encoding.Default);

			string[] vLines = strData.Split(new char[] { '\r', '\n' });

			Dictionary<string, string> dMap = new Dictionary<string, string>
			{
				{ "Account:      ", PwDefs.TitleField },
				{ "User Name:    ", PwDefs.UserNameField },
				{ "Password:     ", PwDefs.PasswordField },
				{ "Hyperlink:    ", PwDefs.UrlField },
				{ "Email:        ", "E-Mail" },
				{ "Comments:     ", PwDefs.NotesField },
				{ "              ", PwDefs.NotesField }
			};

			PwGroup pg = pdStorage.RootGroup;
			PwEntry pe = new PwEntry(true, true);

			foreach(string strLine in vLines)
			{
				if(strLine.StartsWith(InitGroup))
				{
					string strGroup = strLine.Remove(0, InitGroup.Length);
					if(strGroup.Length > InitGroup.Length)
						strGroup = strGroup.Substring(0, strGroup.Length - InitGroup.Length);

					pg = pdStorage.RootGroup.FindCreateGroup(strGroup, true);

					pe = new PwEntry(true, true);
					pg.AddEntry(pe, true);
				}
				else if(strLine.StartsWith(InitNewEntry))
				{
					pe = new PwEntry(true, true);
					pg.AddEntry(pe, true);
				}
				else
				{
					foreach(KeyValuePair<string, string> kvp in dMap)
					{
						if(strLine.StartsWith(kvp.Key))
						{
							ImportUtil.Add(pe, kvp.Value, strLine.Remove(0,
								kvp.Key.Length), pdStorage);
							break;
						}
					}
				}
			}
		}
	}
}
