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
	public static class ReportingMod
	{
		private const string CmdListGroups = "listgroups";
		private const string CmdListEntries = "listentries";

		public static bool ProcessCommand(string strCommand, CommandLineArgs args,
			PwDatabase pwDatabase, out bool bNeedsSave)
		{
			bNeedsSave = false;

			if(strCommand == CmdListGroups)
				ListGroups(pwDatabase);
			else if(strCommand == CmdListEntries)
				ListEntries(pwDatabase);
			else return false;

			return true;
		}

		private static void ListGroups(PwDatabase pwDb)
		{
			GroupHandler gh = delegate(PwGroup pg)
			{
				Console.WriteLine("UUID: " + pg.Uuid.ToHexString());
				Console.WriteLine("N: " + pg.Name);
				Console.WriteLine("DATS: " + pg.DefaultAutoTypeSequence);
				Console.WriteLine("I: " + ((uint)pg.Icon).ToString());
				Console.WriteLine("TC: " + pg.CreationTime.ToString("s"));
				Console.WriteLine("TLA: " + pg.LastAccessTime.ToString("s"));
				Console.WriteLine("TLM: " + pg.LastModificationTime.ToString("s"));
				Console.WriteLine("TE: " + pg.ExpiryTime.ToString("s"));
				Console.WriteLine("EXP: " + (pg.Expires ? "True" : "False"));
				Console.WriteLine("UC: " + pg.UsageCount.ToString());
				Console.WriteLine("IE: " + (pg.IsExpanded ? "True" : "False"));

				Console.WriteLine();
				return true;
			};

			pwDb.RootGroup.TraverseTree(TraversalMethod.PreOrder, gh, null);
		}

		private static void ListEntries(PwDatabase pwDb)
		{
			EntryHandler eh = delegate(PwEntry pe)
			{
				Console.WriteLine("UUID: " + pe.Uuid.ToHexString());
				Console.WriteLine("GRPU: " + pe.ParentGroup.Uuid.ToHexString());
				Console.WriteLine("GRPN: " + pe.ParentGroup.Name);

				foreach(KeyValuePair<string, ProtectedString> kvp in pe.Strings)
				{
					Console.WriteLine("S: " + kvp.Key + " = " + kvp.Value.ReadString());
				}

				Console.WriteLine();
				return true;
			};

			pwDb.RootGroup.TraverseTree(TraversalMethod.PreOrder, null, eh);
		}
	}
}
