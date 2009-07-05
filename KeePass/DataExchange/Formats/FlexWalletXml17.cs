/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2009 Dominik Reichl <dominik.reichl@t-online.de>

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
	internal sealed class FlexWalletXml17 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "FlexWallet XML 1.7"; } }
		public override string DefaultExtension { get { return "xml"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override Image SmallIcon
		{
			get { return KeePass.Properties.Resources.B16x16_Imp_FlexWallet; }
		}

		private const string ElemRoot = "FlexWallet";

		private const string ElemCategory = "Category";
		private const string ElemEntry = "Card";

		private const string ElemField = "Field";
		private const string ElemNotes = "Notes";

		private const string AttribData = "Description";

		public override void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			StreamReader sr = new StreamReader(sInput, Encoding.Default);
			string strDoc = sr.ReadToEnd();
			sr.Close();

			ImportFileString(strDoc, pwStorage);
		}

		private static void ImportFileString(string strXmlDoc, PwDatabase pwStorage)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(strXmlDoc);

			XmlElement xmlRoot = doc.DocumentElement;
			Debug.Assert(xmlRoot.Name == ElemRoot);

			foreach(XmlNode xmlChild in xmlRoot.ChildNodes)
			{
				if(xmlChild.Name == ElemCategory)
					ImportCategory(xmlChild, pwStorage);
				else { Debug.Assert(false); }
			}
		}

		private static void ImportCategory(XmlNode xmlNode, PwDatabase pwStorage)
		{
			string strName = null;
			try { strName = xmlNode.Attributes[AttribData].Value; }
			catch(Exception) { Debug.Assert(false); }
			if(string.IsNullOrEmpty(strName)) strName = KPRes.Group;

			PwGroup pg = new PwGroup(true, true, strName, PwIcon.Folder);
			pwStorage.RootGroup.AddGroup(pg, true);

			foreach(XmlNode xmlChild in xmlNode.ChildNodes)
			{
				if(xmlChild.Name == ElemEntry)
					ImportEntry(xmlChild, pg, pwStorage);
				else { Debug.Assert(false); }
			}
		}

		private static void ImportEntry(XmlNode xmlNode, PwGroup pg, PwDatabase pwStorage)
		{
			PwEntry pe = new PwEntry(true, true);

			string strTitle = null;
			try { strTitle = xmlNode.Attributes[AttribData].Value; }
			catch(Exception) { Debug.Assert(false); }
			if(!string.IsNullOrEmpty(strTitle))
				pe.Strings.Set(PwDefs.TitleField, new ProtectedString(
					pwStorage.MemoryProtection.ProtectTitle, strTitle));

			foreach(XmlNode xmlChild in xmlNode)
			{
				if(xmlChild.Name == ElemField)
				{
					string strName = null;
					try { strName = xmlChild.Attributes[AttribData].Value; }
					catch(Exception) { Debug.Assert(false); }
					if(string.IsNullOrEmpty(strName)) continue;

					string strValue = ImportUtil.SafeInnerText(xmlChild);
					pe.Strings.Set(strName, new ProtectedString(false, strValue));
				}
				else if(xmlChild.Name == ElemNotes)
					pe.Strings.Set(PwDefs.NotesField, new ProtectedString(
						pwStorage.MemoryProtection.ProtectNotes,
						ImportUtil.SafeInnerText(xmlChild)));
				else { Debug.Assert(false); }
			}

			RenameFields(pe);
			pg.AddEntry(pe, true);
		}

		private static void RenameFields(PwEntry pe)
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
					if(pe.Strings.Remove(strFrom) == false) { Debug.Assert(false); }
				}
			}
		}
	}
}
