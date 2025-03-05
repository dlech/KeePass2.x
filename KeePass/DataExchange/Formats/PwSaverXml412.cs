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
	// 4.1.2+
	internal sealed class PwSaverXml412 : FileFormatProvider
	{
		private const string ElemGroup = "FOLDER";
		private const string ElemEntry = "RECORD";

		private const string ElemName = "NAME";
		private const string ElemIcon = "ICON";

		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Password Saver XML"; } }
		public override string DefaultExtension { get { return "xml"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override void Import(PwDatabase pdStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			XmlDocument xd = XmlUtilEx.LoadXmlDocument(sInput, StrUtil.Utf8);

			XmlNode xnRoot = xd.DocumentElement;
			Debug.Assert(xnRoot.Name == "ROOT");

			PwGroup pgRoot = pdStorage.RootGroup;

			foreach(XmlNode xn in xnRoot.ChildNodes)
			{
				if(xn.Name == ElemGroup)
					ImportGroup(xn, pgRoot, pdStorage, false);
				else if(xn.Name == "GARBAGE")
					ImportGroup(xn, pgRoot, pdStorage, true);
				else if(xn.Name == ElemEntry)
					ImportEntry(xn, pgRoot, pdStorage);
				else { Debug.Assert(false); }
			}
		}

		private static void ImportGroup(XmlNode xnGroup, PwGroup pgParent, PwDatabase pd,
			bool bIsRecycleBin)
		{
			PwGroup pg;
			if(!bIsRecycleBin)
			{
				pg = new PwGroup(true, true);
				pgParent.AddGroup(pg, true);
			}
			else
			{
				pg = pd.RootGroup.FindGroup(pd.RecycleBinUuid, true);

				if(pg == null)
				{
					pg = new PwGroup(true, true, KPRes.RecycleBin, PwIcon.TrashBin);
					pgParent.AddGroup(pg, true);

					pd.RecycleBinUuid = pg.Uuid;
					pd.RecycleBinChanged = DateTime.UtcNow;
				}
			}

			foreach(XmlNode xn in xnGroup.ChildNodes)
			{
				if(xn.Name == ElemName)
					pg.Name = XmlUtil.SafeInnerText(xn);
				else if(xn.Name == ElemIcon)
					pg.IconId = GetIcon(xn);
				else if(xn.Name == ElemGroup)
					ImportGroup(xn, pg, pd, false);
				else if(xn.Name == ElemEntry)
					ImportEntry(xn, pg, pd);
				else { Debug.Assert(false); }
			}
		}

		private static void ImportEntry(XmlNode xnEntry, PwGroup pgParent, PwDatabase pd)
		{
			PwEntry pe = new PwEntry(true, true);
			pgParent.AddEntry(pe, true);

			foreach(XmlNode xn in xnEntry.ChildNodes)
			{
				if(xn.Name == ElemName)
					ImportUtil.Add(pe, PwDefs.TitleField, xn, pd);
				else if(xn.Name == ElemIcon)
					pe.IconId = GetIcon(xn);
				else if(xn.Name == "FIELDS")
					ImportFields(xn, pe, pd);
				else { Debug.Assert(false); }
			}
		}

		private static void ImportFields(XmlNode xnFields, PwEntry pe, PwDatabase pd)
		{
			foreach(XmlNode xn in xnFields.ChildNodes)
			{
				if(xn.Name == "FIELD")
					ImportField(xn, pe, pd);
				else { Debug.Assert(false); }
			}
		}

		private static void ImportField(XmlNode xnField, PwEntry pe, PwDatabase pd)
		{
			string strName = null;
			string strValue = null;

			foreach(XmlNode xn in xnField.ChildNodes)
			{
				if(xn.Name == "ID") { }
				else if(xn.Name == ElemName)
					strName = XmlUtil.SafeInnerText(xn);
				else if(xn.Name == "TYPE") { }
				else if(xn.Name == "VALUE")
					strValue = XmlUtil.SafeInnerText(xn);
				else { Debug.Assert(false); }
			}

			if(!string.IsNullOrEmpty(strName) && !string.IsNullOrEmpty(strValue))
			{
				string strF;
				if((strName == "Control Panel") || (strName == "Webmail Interface"))
					strF = PwDefs.UrlField;
				else if(strName == "FTP Address")
					strF = strName;
				else if(strName == "FTP Username")
					strF = "FTP User Name";
				else if(strName == "FTP Password")
					strF = strName;
				else
					strF = ImportUtil.MapName(strName, true);

				ImportUtil.Add(pe, strF, strValue, pd);
			}
		}

		private static PwIcon GetIcon(XmlNode xn)
		{
			string str = XmlUtil.SafeInnerText(xn);
			if(string.IsNullOrEmpty(str)) { Debug.Assert(false); return PwIcon.Key; }

			str = str.ToUpperInvariant();

			if(str == "FOLDER") return PwIcon.Folder;
			if(str == "RECORD") return PwIcon.Key;
			if(str == "WEBSITE.ICO") return PwIcon.Home;
			if(str == "HOSTING.ICO") return PwIcon.NetworkServer;
			if(str == "DIALUP.ICO") return PwIcon.WorldSocket;
			if(str == "SHOPING.ICO") return PwIcon.ClipboardReady; // Sic
			if(str == "AUCTION.ICO") return PwIcon.Tool;
			if(str == "MESSENGER.ICO") return PwIcon.UserCommunication;
			if(str == "SOFTWARE_SERIALS.ICO") return PwIcon.CDRom;
			if(str == "CREDITCARD.ICO") return PwIcon.Identity;
			if(str == "MAILBOX.ICO") return PwIcon.EMailBox;

			Debug.Assert(false);
			return PwIcon.Key;
		}
	}
}
