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
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	// 1.7
	internal sealed class FlexWalletXml17 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "FlexWallet XML"; } }
		public override string DefaultExtension { get { return "xml"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		private const string ElemRoot = "FlexWallet";

		// In 1.7 the node names are Pascal-cased and in 2006 they are
		// lower-cased. Therefore, compare them case-insensitively.

		private const string ElemCategory = "Category";
		private const string ElemEntry = "Card";

		private const string ElemField = "Field";
		private const string ElemNotes = "Notes";

		private const string AttribData = "Description"; // 1.7
		private const string AttribName = "name"; // 2006

		public override void Import(PwDatabase pdStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			XmlDocument xd = XmlUtilEx.LoadXmlDocument(sInput, Encoding.Default);

			XmlElement xmlRoot = xd.DocumentElement;
			Debug.Assert(xmlRoot.Name == ElemRoot);

			foreach(XmlNode xmlChild in xmlRoot.ChildNodes)
			{
				if(xmlChild.Name.Equals(ElemCategory, StrUtil.CaseIgnoreCmp))
					ImportCategory(xmlChild, pdStorage.RootGroup, pdStorage);
				else { Debug.Assert(false); }
			}
		}

		private static void ImportCategory(XmlNode xmlNode, PwGroup pgContainer,
			PwDatabase pd)
		{
			string strName = ReadNameAttrib(xmlNode);
			if(string.IsNullOrEmpty(strName)) strName = KPRes.Group;

			PwGroup pg = new PwGroup(true, true, strName, PwIcon.Folder);
			pgContainer.AddGroup(pg, true);

			foreach(XmlNode xmlChild in xmlNode.ChildNodes)
			{
				if(xmlChild.Name.Equals(ElemEntry, StrUtil.CaseIgnoreCmp))
					ImportEntry(xmlChild, pg, pd);
				else if(xmlChild.Name.Equals(ElemCategory, StrUtil.CaseIgnoreCmp))
					ImportCategory(xmlChild, pg, pd);
				else { Debug.Assert(false); }
			}
		}

		private static void ImportEntry(XmlNode xmlNode, PwGroup pg, PwDatabase pd)
		{
			PwEntry pe = new PwEntry(true, true);

			string strTitle = ReadNameAttrib(xmlNode);
			if(!string.IsNullOrEmpty(strTitle))
				ImportUtil.Add(pe, PwDefs.TitleField, strTitle, pd);

			foreach(XmlNode xmlChild in xmlNode)
			{
				if(xmlChild.Name.Equals(ElemField, StrUtil.CaseIgnoreCmp))
				{
					string strName = ReadNameAttrib(xmlChild);
					if(string.IsNullOrEmpty(strName)) continue;

					strName = ImportUtil.MapName(strName, true);

					ImportUtil.Add(pe, strName, xmlChild, pd);
				}
				else if(xmlChild.Name.Equals(ElemNotes, StrUtil.CaseIgnoreCmp))
					ImportUtil.Add(pe, PwDefs.NotesField, xmlChild, pd);
				else { Debug.Assert(false); }
			}

			// RenameFields(pe);
			pg.AddEntry(pe, true);
		}

		private static string ReadNameAttrib(XmlNode xmlNode)
		{
			if(xmlNode == null) { Debug.Assert(false); return string.Empty; }

			try
			{
				if(xmlNode.Attributes.GetNamedItem(AttribData) != null) // 1.7
					return (xmlNode.Attributes[AttribData].Value ?? string.Empty);
				if(xmlNode.Attributes.GetNamedItem(AttribName) != null) // 2006
					return (xmlNode.Attributes[AttribName].Value ?? string.Empty);

				Debug.Assert(false);
			}
			catch(Exception) { Debug.Assert(false); }

			return string.Empty;
		}

		/* private static void RenameFields(PwEntry pe)
		{
			string[] vMap = new string[] {
				"Acct #", PwDefs.UserNameField,
				"Subject", PwDefs.UserNameField,
				"Location", PwDefs.UserNameField,
				"Combination", PwDefs.PasswordField,
				"Username", PwDefs.UserNameField,
				"Website", PwDefs.UrlField,
				"Serial #", PwDefs.PasswordField,
				"Product ID", PwDefs.UserNameField
			};

			Debug.Assert((vMap.Length % 2) == 0);
			for(int i = 0; i < vMap.Length; i += 2)
			{
				string strFrom = vMap[i], strTo = vMap[i + 1];

				if(pe.Strings.ReadSafe(strTo).Length > 0) continue;

				string strData = pe.Strings.ReadSafe(strFrom);
				if(strData.Length > 0)
				{
					pe.Strings.Set(strTo, new ProtectedString(false, strData));
					if(!pe.Strings.Remove(strFrom)) { Debug.Assert(false); }
				}
			}
		} */
	}
}
