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
using System.Xml;

using KeePass.Resources;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	// 3.2+
	internal sealed class DesktopKnoxXml32 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "DesktopKnox XML"; } }
		public override string DefaultExtension { get { return "xml"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override void Import(PwDatabase pdStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			XmlDocument xd = XmlUtilEx.LoadXmlDocument(sInput, StrUtil.Utf8);

			XmlElement xeRoot = xd.DocumentElement;
			Debug.Assert(xeRoot.Name == "SafeCatalog");

			Dictionary<string, PwGroup> dGroups = new Dictionary<string, PwGroup>();
			dGroups[string.Empty] = pdStorage.RootGroup;

			foreach(XmlNode xn in xeRoot.ChildNodes)
			{
				if(xn.Name == "SafeElement")
					ImportEntry(xn, pdStorage, dGroups);
				else { Debug.Assert(false); }
			}
		}

		private static void ImportEntry(XmlNode xnEntry, PwDatabase pd,
			Dictionary<string, PwGroup> dGroups)
		{
			PwEntry pe = new PwEntry(true, true);
			string strGroup = string.Empty;

			foreach(XmlNode xn in xnEntry)
			{
				if(xn.Name == "Category")
					strGroup = XmlUtil.SafeInnerText(xn);
				else if(xn.Name == "Title")
					ImportUtil.Add(pe, PwDefs.TitleField, xn, pd);
				else if(xn.Name == "Content")
					ImportUtil.Add(pe, PwDefs.NotesField, xn, pd);
			}

			PwGroup pg;
			dGroups.TryGetValue(strGroup, out pg);
			if(pg == null)
			{
				pg = new PwGroup(true, true);
				pg.Name = strGroup;
				dGroups[string.Empty].AddGroup(pg, true);
				dGroups[strGroup] = pg;
			}
			pg.AddEntry(pe, true);
		}
	}
}
