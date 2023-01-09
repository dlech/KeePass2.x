/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2023 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Xml.Serialization;

using KeePass.Resources;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	// 1.0.5-1.3.4+
	internal sealed class PwExporterXml105 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Password Exporter XML"; } }
		public override string DefaultExtension { get { return "xml"; } }
		public override string ApplicationGroup { get { return KPRes.Browser; } }

		public override bool ImportAppendsToRootGroupOnly { get { return true; } }

		private const string ElemRoot = "xml";
		private const string ElemEntries = "entries";
		private const string ElemEntry = "entry";

		private const string AttrUser = "user";
		private const string AttrPassword = "password";
		private const string AttrURL = "host";
		private const string AttrUserFieldName = "userFieldName";
		private const string AttrPasswordFieldName = "passFieldName";

		private const string DbUserFieldName = "FieldID_UserName";
		private const string DbPasswordFieldName = "FieldID_Password";

		public override void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			StreamReader sr = new StreamReader(sInput, Encoding.Default);
			string strDoc = sr.ReadToEnd();
			sr.Close();

			// Fix '<' characters, for version 1.0.5
			int nIndex = strDoc.IndexOf('<');
			while(nIndex >= 0)
			{
				int nAttrib = strDoc.LastIndexOf("=\"", nIndex);
				int nElem = strDoc.LastIndexOf('>', nIndex);
				
				if(nAttrib > nElem)
				{
					strDoc = strDoc.Remove(nIndex, 1);
					strDoc = strDoc.Insert(nIndex, @"&lt;");
				}
				nIndex = strDoc.IndexOf('<', nIndex + 1);
			}

			// Fix '>' characters, for version 1.0.5
			nIndex = strDoc.IndexOf('>');
			while(nIndex >= 0)
			{
				char chPrev = strDoc[nIndex - 1];
				string strPrev4 = strDoc.Substring(nIndex - 3, 4);

				if((chPrev != '/') && (chPrev != '\"') && (strPrev4 != @"xml>") &&
					(strPrev4 != @"ies>"))
				{
					strDoc = strDoc.Remove(nIndex, 1);
					strDoc = strDoc.Insert(nIndex, @"&gt;");
				}
				nIndex = strDoc.IndexOf('>', nIndex + 1);
			}

			MemoryStream ms = new MemoryStream(StrUtil.Utf8.GetBytes(strDoc), false);
			XmlDocument xmlDoc = XmlUtilEx.CreateXmlDocument();
			xmlDoc.Load(ms);
			ms.Close();

			XmlNode xmlRoot = xmlDoc.DocumentElement;
			if(xmlRoot.Name != ElemRoot)
				throw new FormatException("Invalid root element!");

			foreach(XmlNode xmlChild in xmlRoot.ChildNodes)
			{
				if(xmlChild.Name == ElemEntries)
					ImportEntries(xmlChild, pwStorage);
				else { Debug.Assert(false); }
			}
		}

		private static void ImportEntries(XmlNode xmlNode, PwDatabase pwStorage)
		{
			foreach(XmlNode xmlChild in xmlNode)
			{
				if(xmlChild.Name == ElemEntry)
					ImportEntry(xmlChild, pwStorage);
				else { Debug.Assert(false); }
			}
		}

		private static void ImportEntry(XmlNode xmlNode, PwDatabase pwStorage)
		{
			PwEntry pe = new PwEntry(true, true);
			pwStorage.RootGroup.AddEntry(pe, true);

			XmlAttributeCollection col = xmlNode.Attributes;
			if(col == null) return;

			XmlNode xmlAttrib;
			xmlAttrib = col.GetNamedItem(AttrUser);
			if(xmlAttrib != null) pe.Strings.Set(PwDefs.UserNameField, new ProtectedString(
				pwStorage.MemoryProtection.ProtectUserName, PctDecode(xmlAttrib.Value)));
			else { Debug.Assert(false); }

			xmlAttrib = col.GetNamedItem(AttrPassword);
			if(xmlAttrib != null) pe.Strings.Set(PwDefs.PasswordField, new ProtectedString(
				pwStorage.MemoryProtection.ProtectPassword, PctDecode(xmlAttrib.Value)));
			else { Debug.Assert(false); }

			xmlAttrib = col.GetNamedItem(AttrURL);
			if(xmlAttrib != null) pe.Strings.Set(PwDefs.UrlField, new ProtectedString(
				pwStorage.MemoryProtection.ProtectUrl, PctDecode(xmlAttrib.Value)));
			else { Debug.Assert(false); }

			xmlAttrib = col.GetNamedItem(AttrUserFieldName);
			if(xmlAttrib != null) pe.Strings.Set(DbUserFieldName, new ProtectedString(
				false, PctDecode(xmlAttrib.Value)));
			else { Debug.Assert(false); }

			xmlAttrib = col.GetNamedItem(AttrPasswordFieldName);
			if(xmlAttrib != null) pe.Strings.Set(DbPasswordFieldName, new ProtectedString(
				false, PctDecode(xmlAttrib.Value)));
			else { Debug.Assert(false); }
		}

		// For version 1.3.4
		private static string PctDecode(string strText)
		{
			if(string.IsNullOrEmpty(strText)) return string.Empty;

			string str = strText;

			str = str.Replace("%3C", "<");
			str = str.Replace("%3E", ">");
			str = str.Replace("%22", "\"");
			str = str.Replace("%26", "&");
			str = str.Replace("%25", "%"); // Must be last

			return str;
		}
	}
}
