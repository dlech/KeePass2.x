/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2010 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.IO;
using System.Windows.Forms;
using System.Drawing;

using KeePass.Resources;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Security;

namespace KeePass.DataExchange.Formats
{
	// 1.42
	internal sealed class PwGorillaCsv142 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Password Gorilla CSV"; } }
		public override string DefaultExtension { get { return "csv"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override Image SmallIcon
		{
			get { return KeePass.Properties.Resources.B16x16_Imp_PwGorilla; }
		}

		public override void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			StreamReader sr = new StreamReader(sInput, Encoding.Unicode);
			string strData = sr.ReadToEnd();
			sr.Close();

			string[] vLines = strData.Split(new char[]{ '\r', '\n' });

			foreach(string strLine in vLines)
			{
				if(strLine.Length == 0) continue;

				string[] vParts = strLine.Split(new char[]{ 'µ' });

				PwEntry pe = new PwEntry(true, true);
				PwGroup pgContainer = pwStorage.RootGroup;

				string strNotes = string.Empty;
				for(int i = 0; i < vParts.Length; ++i)
				{
					switch(i)
					{
						case 0: // Empty field
							break;
						case 1:
							pgContainer = pwStorage.RootGroup.FindCreateSubTree(
								vParts[i], new char[]{ '.' });
							break;
						case 2: pe.Strings.Set(PwDefs.TitleField, new ProtectedString(
							pwStorage.MemoryProtection.ProtectTitle, vParts[i]));
							break;
						case 3: pe.Strings.Set(PwDefs.UserNameField, new ProtectedString(
							pwStorage.MemoryProtection.ProtectUserName, vParts[i]));
							break;
						case 4: pe.Strings.Set(PwDefs.PasswordField, new ProtectedString(
							pwStorage.MemoryProtection.ProtectPassword, vParts[i]));
							break;
						case 5: strNotes += vParts[i].Replace("\\n", "\n");
							break;
						default:
							strNotes += @"µ" + vParts[i].Replace("\\n", "\n");
							break;
					}
				}

				pe.Strings.Set(PwDefs.NotesField, new ProtectedString(
					pwStorage.MemoryProtection.ProtectNotes, strNotes));

				pgContainer.AddEntry(pe, true);
			}
		}
	}
}
