/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2020 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Drawing;
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
		private const string ElemRoot = "ROOT";

		private const string ElemGroup = "FOLDER";
		private const string ElemRecycleBin = "GARBAGE";
		private const string ElemEntry = "RECORD";

		private const string ElemName = "NAME";
		private const string ElemIcon = "ICON";

		private const string ElemFields = "FIELDS";
		private const string ElemField = "FIELD";
		private const string ElemID = "ID";
		private const string ElemType = "TYPE";
		private const string ElemValue = "VALUE";

		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Password Saver XML"; } }
		public override string DefaultExtension { get { return "xml"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override Image SmallIcon
		{
			get { return KeePass.Properties.Resources.B16x16_Imp_PwSaver; }
		}

		public override void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			StreamReader sr = new StreamReader(sInput, StrUtil.Utf8);
			string strDoc = sr.ReadToEnd();
			sr.Close();

			XmlDocument doc = XmlUtilEx.CreateXmlDocument();
			doc.LoadXml(strDoc);

			XmlElement xmlRoot = doc.DocumentElement;
			Debug.Assert(xmlRoot.Name == ElemRoot);

			PwGroup pgRoot = pwStorage.RootGroup;

			foreach(XmlNode xmlChild in xmlRoot.ChildNodes)
			{
				if(xmlChild.Name == ElemGroup)
					ImportGroup(xmlChild, pgRoot, pwStorage, false);
				else if(xmlChild.Name == ElemRecycleBin)
					ImportGroup(xmlChild, pgRoot, pwStorage, true);
				else if(xmlChild.Name == ElemEntry)
					ImportEntry(xmlChild, pgRoot, pwStorage);
				else { Debug.Assert(false); }
			}
		}

		private static void ImportGroup(XmlNode xn, PwGroup pgParent, PwDatabase pd,
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

			foreach(XmlNode xmlChild in xn.ChildNodes)
			{
				if(xmlChild.Name == ElemName)
					pg.Name = XmlUtil.SafeInnerText(xmlChild);
				else if(xmlChild.Name == ElemIcon)
					pg.IconId = GetIcon(XmlUtil.SafeInnerText(xmlChild));
				else if(xmlChild.Name == ElemGroup)
					ImportGroup(xmlChild, pg, pd, false);
				else if(xmlChild.Name == ElemEntry)
					ImportEntry(xmlChild, pg, pd);
				else { Debug.Assert(false); }
			}
		}

		private static void ImportEntry(XmlNode xn, PwGroup pgParent, PwDatabase pd)
		{
			PwEntry pe = new PwEntry(true, true);
			pgParent.AddEntry(pe, true);

			foreach(XmlNode xmlChild in xn.ChildNodes)
			{
				if(xmlChild.Name == ElemName)
					ImportUtil.AppendToField(pe, PwDefs.TitleField,
						XmlUtil.SafeInnerText(xmlChild), pd);
				else if(xmlChild.Name == ElemIcon)
					pe.IconId = GetIcon(XmlUtil.SafeInnerText(xmlChild));
				else if(xmlChild.Name == ElemFields)
					ImportFields(xmlChild, pe, pd);
				else { Debug.Assert(false); }
			}
		}

		private static void ImportFields(XmlNode xn, PwEntry pe, PwDatabase pd)
		{
			foreach(XmlNode xmlChild in xn.ChildNodes)
			{
				if(xmlChild.Name == ElemField)
					ImportField(xmlChild, pe, pd);
				else { Debug.Assert(false); }
			}
		}

		private static void ImportField(XmlNode xn, PwEntry pe, PwDatabase pd)
		{
			string strName = null;
			string strValue = null;

			foreach(XmlNode xmlChild in xn.ChildNodes)
			{
				if(xmlChild.Name == ElemID) { }
				else if(xmlChild.Name == ElemName)
					strName = XmlUtil.SafeInnerText(xmlChild);
				else if(xmlChild.Name == ElemType) { }
				else if(xmlChild.Name == ElemValue)
					strValue = XmlUtil.SafeInnerText(xmlChild);
				else { Debug.Assert(false); }
			}

			if(!string.IsNullOrEmpty(strName) && !string.IsNullOrEmpty(strValue))
			{
				string strF = ImportUtil.MapNameToStandardField(strName, true);
				if((strName == "Control Panel") || (strName == "Webmail Interface"))
					strF = PwDefs.UrlField;
				else if(strName == "FTP Address")
					strF = strName;
				else if(strName == "FTP Username")
					strF = "FTP User Name";
				else if(strName == "FTP Password")
					strF = strName;

				if(string.IsNullOrEmpty(strF)) strF = strName;

				ImportUtil.AppendToField(pe, strF, strValue, pd, null, true);
			}
		}

		private static PwIcon GetIcon(string strName)
		{
			if(string.IsNullOrEmpty(strName)) { Debug.Assert(false); return PwIcon.Key; }

			string str = strName.ToUpperInvariant();

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
