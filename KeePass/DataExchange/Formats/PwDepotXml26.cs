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
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Drawing;

using KeePass.Resources;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Security;

namespace KeePass.DataExchange.Formats
{
	public sealed class PwDepotXml26 : FormatImporter
	{
		private const string ElemHeader = "HEADER";
		private const string ElemContainer = "PASSWORDS";

		private const string ElemGroup = "GROUP";
		private const string AttribGroupName = "NAME";

		private const string ElemEntry = "ITEM";
		private const string ElemEntryName = "DESCRIPTION";
		private const string ElemEntryUser = "USERNAME";
		private const string ElemEntryPassword = "PASSWORD";
		private const string ElemEntryURL = "URL";
		private const string ElemEntryNotes = "COMMENT";
		private const string ElemEntryLastModTime = "LASTMODIFIED";
		private const string ElemEntryExpireTime = "EXPIRYDATE";
		private const string ElemEntryImportance = "IMPORTANCE";
		private const string ElemEntryAutoType = "TEMPLATE";
		private const string ElemEntryCustom = "CUSTOMFIELDS";

		private const string ElemCustomField = "FIELD";
		private const string ElemCustomFieldName = "NAME";
		private const string ElemCustomFieldValue = "VALUE";

		public override string FormatName { get { return "Password Depot XML 2.6"; } }
		public override string DefaultExtension { get { return "xml"; } }
		public override string AppGroup { get { return KPRes.PasswordManagers; } }

		public override Image SmallIcon
		{
			get { return KeePass.Properties.Resources.B16x16_Imp_PwDepot; }
		}

		public override void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(sInput);

			XmlNode xmlRoot = xmlDoc.DocumentElement;

			foreach(XmlNode xmlChild in xmlRoot.ChildNodes)
			{
				if(xmlChild.Name == ElemHeader) { } // Unsupported
				else if(xmlChild.Name == ElemContainer)
					ReadContainer(xmlChild, pwStorage);
				else { Debug.Assert(false); }
			}
		}

		private static void ReadContainer(XmlNode xmlNode, PwDatabase pwStorage)
		{
			foreach(XmlNode xmlChild in xmlNode.ChildNodes)
			{
				if(xmlChild.Name == ElemGroup)
					ReadGroup(xmlChild, pwStorage.RootGroup, pwStorage);
				else { Debug.Assert(false); }
			}
		}

		private static void ReadGroup(XmlNode xmlNode, PwGroup pgParent, PwDatabase pwStorage)
		{
			PwGroup pg = new PwGroup(true, true);

			pg.ParentGroup = pgParent;
			pgParent.Groups.Add(pg);

			try
			{
				XmlAttributeCollection xac = xmlNode.Attributes;
				pg.Name = xac.GetNamedItem(AttribGroupName).Value;
			}
			catch(Exception) { }

			foreach(XmlNode xmlChild in xmlNode)
			{
				if(xmlChild.Name == ElemGroup)
					ReadGroup(xmlChild, pg, pwStorage);
				else if(xmlChild.Name == ElemEntry)
					ReadEntry(xmlChild, pg, pwStorage);
				else { Debug.Assert(false); }
			}
		}

		private static void ReadEntry(XmlNode xmlNode, PwGroup pgParent,
			PwDatabase pwStorage)
		{
			PwEntry pe = new PwEntry(pgParent, true, true);
			pgParent.Entries.Add(pe);

			DateTime dt;
			foreach(XmlNode xmlChild in xmlNode)
			{
				if(xmlChild.Name == ElemEntryName)
					pe.Strings.Set(PwDefs.TitleField, new ProtectedString(
						pwStorage.MemoryProtection.ProtectTitle,
						ImportUtil.SafeInnerText(xmlChild)));
				else if(xmlChild.Name == ElemEntryUser)
					pe.Strings.Set(PwDefs.UserNameField, new ProtectedString(
						pwStorage.MemoryProtection.ProtectUserName,
						ImportUtil.SafeInnerText(xmlChild)));
				else if(xmlChild.Name == ElemEntryPassword)
					pe.Strings.Set(PwDefs.PasswordField, new ProtectedString(
						pwStorage.MemoryProtection.ProtectPassword,
						ImportUtil.SafeInnerText(xmlChild)));
				else if(xmlChild.Name == ElemEntryURL)
					pe.Strings.Set(PwDefs.UrlField, new ProtectedString(
						pwStorage.MemoryProtection.ProtectUrl,
						ImportUtil.SafeInnerText(xmlChild)));
				else if(xmlChild.Name == ElemEntryNotes)
					pe.Strings.Set(PwDefs.NotesField, new ProtectedString(
						pwStorage.MemoryProtection.ProtectNotes,
						ImportUtil.SafeInnerText(xmlChild)));
				else if(xmlChild.Name == ElemEntryLastModTime)
				{
					if(DateTime.TryParse(ImportUtil.SafeInnerText(xmlChild), out dt))
						pe.LastModificationTime = dt;
				}
				else if(xmlChild.Name == ElemEntryExpireTime)
				{
					if(DateTime.TryParse(ImportUtil.SafeInnerText(xmlChild), out dt))
						pe.ExpiryTime = dt;
				}
				else if(xmlChild.Name == ElemEntryImportance) { } // Unsupported
				else if(xmlChild.Name == ElemEntryAutoType)
					pe.AutoType.DefaultSequence = ImportUtil.SafeInnerText(xmlChild);
				else if(xmlChild.Name == ElemEntryCustom)
					ReadCustomContainer(xmlChild, pe);
				else { Debug.Assert(false); }
			}
		}

		private static void ReadCustomContainer(XmlNode xmlNode, PwEntry pe)
		{
			foreach(XmlNode xmlChild in xmlNode)
			{
				if(xmlChild.Name == ElemCustomField)
					ReadCustomField(xmlChild, pe);
				else { Debug.Assert(false); }
			}
		}

		private static void ReadCustomField(XmlNode xmlNode, PwEntry pe)
		{
			string strName = string.Empty, strValue = string.Empty;

			foreach(XmlNode xmlChild in xmlNode)
			{
				if(xmlChild.Name == ElemCustomFieldName)
					strName = ImportUtil.SafeInnerText(xmlChild);
				else if(xmlChild.Name == ElemCustomFieldValue)
					strValue = ImportUtil.SafeInnerText(xmlChild);
				else { Debug.Assert(false); }
			}

			if((strName.Length == 0) || PwDefs.IsStandardField(strName))
				pe.Strings.Set(Guid.NewGuid().ToString(), new ProtectedString(false, strValue));
			else
				pe.Strings.Set(strName, new ProtectedString(false, strValue));
		}
	}
}
