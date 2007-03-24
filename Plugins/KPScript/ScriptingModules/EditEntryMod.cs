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

using KeePass.Util;

using KeePassLib;
using KeePassLib.Delegates;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KPScript.ScriptingModules
{
	public static class EditEntryMod
	{
		private const string CmdAddEntry = "addentry";

		public static bool ProcessCommand(string strCommand, CommandLineArgs args,
			PwDatabase pwDatabase, out bool bNeedsSave)
		{
			bNeedsSave = false;

			if(strCommand == CmdAddEntry)
				bNeedsSave = AddEntry(pwDatabase, args);
			else return false;

			return true;
		}

		private static bool AddEntry(PwDatabase pwDb, CommandLineArgs args)
		{
			PwEntry pe = new PwEntry(pwDb.RootGroup, true, true);
			pwDb.RootGroup.Entries.Add(pe);

			SetEntryString(pe, PwDefs.TitleField, args, pwDb);
			SetEntryString(pe, PwDefs.UserNameField, args, pwDb);
			SetEntryString(pe, PwDefs.PasswordField, args, pwDb);
			SetEntryString(pe, PwDefs.UrlField, args, pwDb);
			SetEntryString(pe, PwDefs.NotesField, args, pwDb);

			return true;
		}

		private static void SetEntryString(PwEntry pe, string strID,
			CommandLineArgs args, PwDatabase pwDb)
		{
			string str = args[strID];
			if(str == null) return;

			pe.Strings.Set(strID, new ProtectedString(false, str));
		}
	}
}
