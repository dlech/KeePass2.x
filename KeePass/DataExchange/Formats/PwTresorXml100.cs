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
	// 1.0.2001.157
	internal sealed class PwTresorXml100 : FileFormatProvider
	{
		private const string ElemGroup = "Group";

		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Passwort.Tresor XML"; } }
		public override string DefaultExtension { get { return "xml"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override void Import(PwDatabase pdStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			XmlDocument xd = XmlUtilEx.LoadXmlDocument(sInput, Encoding.Default);

			XmlNode xnRoot = xd.DocumentElement;

			foreach(XmlNode xn in xnRoot.ChildNodes)
			{
				if(xn.Name == ElemGroup)
					ReadGroup(xn, pdStorage.RootGroup, pdStorage);
				else { Debug.Assert(false); }
			}
		}

		private static void ReadGroup(XmlNode xnGroup, PwGroup pgParent, PwDatabase pd)
		{
			PwGroup pg = new PwGroup(true, true);
			pgParent.AddGroup(pg, true);

			foreach(XmlNode xn in xnGroup)
			{
				if(xn.Name == "groupname")
					pg.Name = XmlUtil.SafeInnerText(xn);
				else if(xn.Name == ElemGroup)
					ReadGroup(xn, pg, pd);
				else if(xn.Name == "PassItem")
					ReadEntry(xn, pg, pd);
				else { Debug.Assert(false); }
			}
		}

		private static void ReadEntry(XmlNode xnEntry, PwGroup pgParent, PwDatabase pd)
		{
			PwEntry pe = new PwEntry(true, true);
			pgParent.AddEntry(pe, true);

			foreach(XmlNode xn in xnEntry)
			{
				if(xn.Name == "itemname")
					ImportUtil.Add(pe, PwDefs.TitleField, xn, pd);
				else if(xn.Name == "username")
					ImportUtil.Add(pe, PwDefs.UserNameField, xn, pd);
				else if(xn.Name == "password")
					ImportUtil.Add(pe, PwDefs.PasswordField, xn, pd);
				else if(xn.Name == "url")
					ImportUtil.Add(pe, PwDefs.UrlField, xn, pd);
				else if(xn.Name == "description")
					ImportUtil.Add(pe, PwDefs.NotesField, xn, pd);
				else { Debug.Assert(false); }
			}
		}
	}
}
