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
using KeePassLib.Security;

namespace KeePass.DataExchange.Formats
{
	internal sealed class RevelationXml04 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Revelation XML"; } }
		public override string DefaultExtension { get { return "xml"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override void Import(PwDatabase pdStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			XmlDocument xd = XmlUtilEx.LoadXmlDocument(sInput, StrUtil.Utf8);

			ProcessEntries(pdStorage, pdStorage.RootGroup,
				xd.DocumentElement.ChildNodes);
		}

		private static void ProcessEntries(PwDatabase pd, PwGroup pgParent,
			XmlNodeList xlNodes)
		{
			foreach(XmlNode xmlChild in xlNodes)
			{
				if(xmlChild.Name == "entry")
				{
					XmlNode xnType = xmlChild.Attributes.GetNamedItem("type");
					if(xnType == null) { Debug.Assert(false); }
					else
					{
						if(xnType.Value == "folder")
							ImportGroup(pd, pgParent, xmlChild);
						else ImportEntry(pd, pgParent, xmlChild);
					}
				}
			}
		}

		private static void ImportGroup(PwDatabase pd, PwGroup pgParent, XmlNode xmlNode)
		{
			PwGroup pg = new PwGroup(true, true);
			pgParent.AddGroup(pg, true);

			foreach(XmlNode xmlChild in xmlNode.ChildNodes)
			{
				if(xmlChild.Name == "name")
					pg.Name = XmlUtil.SafeInnerText(xmlChild);
				else if(xmlChild.Name == "description")
					pg.Notes = XmlUtil.SafeInnerText(xmlChild);
				else if(xmlChild.Name == "entry") { }
				else if(xmlChild.Name == "updated")
					pg.LastModificationTime = ImportTime(xmlChild);
				else { Debug.Assert(false); }
			}

			ProcessEntries(pd, pg, xmlNode.ChildNodes);
		}

		private static void ImportEntry(PwDatabase pd, PwGroup pgParent, XmlNode xmlNode)
		{
			PwEntry pe = new PwEntry(true, true);
			pgParent.AddEntry(pe, true);

			foreach(XmlNode xmlChild in xmlNode.ChildNodes)
			{
				if(xmlChild.Name == "name")
					ImportUtil.Add(pe, PwDefs.TitleField, xmlChild, pd);
				else if(xmlChild.Name == "description")
					ImportUtil.Add(pe, PwDefs.NotesField, xmlChild, pd);
				else if(xmlChild.Name == "updated")
					pe.LastModificationTime = ImportTime(xmlChild);
				else if(xmlChild.Name == "field")
				{
					XmlNode xnName = xmlChild.Attributes.GetNamedItem("id");
					if(xnName == null) { Debug.Assert(false); }
					else
					{
						string strName = MapFieldName(xnName.Value);
						ImportUtil.Add(pe, strName, xmlChild, pd);
					}
				}
				else { Debug.Assert(false); }
			}
		}

		private static string MapFieldName(string strFieldName)
		{
			switch(strFieldName)
			{
				case "creditcard-cardnumber":
				case "generic-username":
				case "generic-location":
				case "phone-phonenumber":
					return PwDefs.UserNameField;
				case "generic-code":
				case "generic-password":
				case "generic-pin":
					return PwDefs.PasswordField;
				case "generic-hostname":
				case "generic-url":
					return PwDefs.UrlField;
				case "creditcard-cardtype":
					return "Card Type";
				case "creditcard-expirydate":
					return KPRes.ExpiryTime;
				case "creditcard-ccv":
					return "CCV Number";
				case "generic-certificate":
					return "Certificate";
				case "generic-keyfile":
					return "Key File";
				case "generic-database":
					return KPRes.Database;
				case "generic-email":
					return KPRes.EMail;
				case "generic-port":
					return "Port";
				case "generic-domain":
					return "Domain";
				default: Debug.Assert(false); break;
			}

			return strFieldName;
		}

		private static DateTime ImportTime(XmlNode xn)
		{
			string str = XmlUtil.SafeInnerText(xn);

			double dtUnix;
			if(!double.TryParse(str, out dtUnix)) { Debug.Assert(false); }
			else return TimeUtil.ConvertUnixTime(dtUnix);

			return DateTime.UtcNow;
		}
	}
}
