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

using KeePassLib;
using KeePassLib.Interfaces;
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

		public override void Import(PwDatabase pdStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			string strDoc = MemUtil.ReadString(sInput, Encoding.Default);

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
			nIndex = strDoc.IndexOf('>', 3);
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

			XmlDocument xmlDoc = XmlUtilEx.LoadXmlDocumentFromString(strDoc);

			XmlNode xmlRoot = xmlDoc.DocumentElement;
			if(xmlRoot.Name != "xml")
				throw new FormatException("Invalid root element!");

			foreach(XmlNode xmlChild in xmlRoot.ChildNodes)
			{
				if(xmlChild.Name == "entries")
					ImportEntries(xmlChild, pdStorage);
				else { Debug.Assert(false); }
			}
		}

		private static void ImportEntries(XmlNode xmlNode, PwDatabase pd)
		{
			foreach(XmlNode xmlChild in xmlNode)
			{
				if(xmlChild.Name == "entry")
					ImportEntry(xmlChild, pd);
				else { Debug.Assert(false); }
			}
		}

		private static void ImportEntry(XmlNode xmlNode, PwDatabase pd)
		{
			PwEntry pe = new PwEntry(true, true);
			pd.RootGroup.AddEntry(pe, true);

			XmlAttributeCollection xac = xmlNode.Attributes;
			if(xac == null) return;

			ImportString(pe, PwDefs.UserNameField, xac, "user", pd);
			ImportString(pe, PwDefs.PasswordField, xac, "password", pd);
			ImportString(pe, PwDefs.UrlField, xac, "host", pd);
			ImportString(pe, "FieldID_UserName", xac, "userFieldName", pd);
			ImportString(pe, "FieldID_Password", xac, "passFieldName", pd);
		}

		private static void ImportString(PwEntry pe, string strFieldName,
			XmlAttributeCollection xac, string strAttribName, PwDatabase pd)
		{
			XmlNode xn = xac.GetNamedItem(strAttribName);
			if(xn != null)
				ImportUtil.Add(pe, strFieldName, PctDecode(xn.Value), pd);
			else { Debug.Assert(false); }
		}

		// For version 1.3.4
		private static string PctDecode(string str)
		{
			if(string.IsNullOrEmpty(str)) return string.Empty;

			str = str.Replace("%3C", "<");
			str = str.Replace("%3E", ">");
			str = str.Replace("%22", "\"");
			str = str.Replace("%26", "&");
			str = str.Replace("%25", "%"); // Must be last

			return str;
		}
	}
}
