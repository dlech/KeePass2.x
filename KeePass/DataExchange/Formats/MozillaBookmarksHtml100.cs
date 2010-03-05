/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2010 Dominik Reichl <dominik.reichl@t-online.de>

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
	internal sealed class MozillaBookmarksHtml100 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Mozilla Bookmarks HTML 1.00"; } }
		public override string DefaultExtension { get { return "html"; } }
		public override string ApplicationGroup { get { return KPRes.Browser; } }

		public override bool ImportAppendsToRootGroupOnly { get { return true; } }

		public override Image SmallIcon
		{
			get { return KeePass.Properties.Resources.B16x16_ASCII; }
		}

		public override void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			StreamReader sr = new StreamReader(sInput, Encoding.UTF8);
			string strContent = sr.ReadToEnd();
			sr.Close();

			if(strContent.IndexOf(@"<!DOCTYPE NETSCAPE-Bookmark-file-1>") < 0)
				throw new FormatException("Invalid DOCTYPE!");

			strContent = strContent.Replace(@"<!DOCTYPE NETSCAPE-Bookmark-file-1>", string.Empty);
			strContent = strContent.Replace(@"<HR>", string.Empty);
			strContent = strContent.Replace(@"<p>", string.Empty);
			strContent = strContent.Replace(@"<DD>", string.Empty);
			strContent = strContent.Replace(@"<DL>", string.Empty);
			strContent = strContent.Replace(@"</DL>", string.Empty);
			strContent = strContent.Replace(@"<DT>", string.Empty);

			int nOffset = strContent.IndexOf('&');
			while(nOffset >= 0)
			{
				string str4 = strContent.Substring(nOffset, 4);
				string str5 = strContent.Substring(nOffset, 5);
				string str6 = strContent.Substring(nOffset, 6);

				if((str6 != @"&nbsp;") && (str5 != @"&amp;") && (str4 != @"&lt;") &&
					(str4 != @"&gt;") && (str5 != @"&#39;") && (str6 != @"&quot;"))
				{
					strContent = strContent.Remove(nOffset, 1);
					strContent = strContent.Insert(nOffset, @"&amp;");
				}
				else nOffset = strContent.IndexOf('&', nOffset + 1);
			}

			strContent = "<RootSentinel>" + strContent + "</META></RootSentinel>";

			byte[] pbFixedData = Encoding.UTF8.GetBytes(strContent);
			MemoryStream msFixed = new MemoryStream(pbFixedData, false);

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(msFixed);
			msFixed.Close();

			XmlNode xmlRoot = xmlDoc.DocumentElement;
			foreach(XmlNode xmlChild in xmlRoot)
			{
				if(xmlChild.Name == "META")
					ImportLinksFlat(xmlChild, pwStorage);
			}
		}

		private static void ImportLinksFlat(XmlNode xmlNode, PwDatabase pwStorage)
		{
			foreach(XmlNode xmlChild in xmlNode)
			{
				if(xmlChild.Name == "A")
				{
					try
					{
						PwEntry pe = new PwEntry(true, true);

						pe.Strings.Set(PwDefs.TitleField, new ProtectedString(
							pwStorage.MemoryProtection.ProtectTitle,
							xmlChild.InnerText));

						pe.Strings.Set(PwDefs.UrlField, new ProtectedString(
							pwStorage.MemoryProtection.ProtectUrl,
							xmlChild.Attributes.GetNamedItem("HREF").Value));

						pe.Strings.Set("RDF_ID", new ProtectedString(
							false, xmlChild.Attributes.GetNamedItem("ID").Value));

						pwStorage.RootGroup.AddEntry(pe, true);
					}
					catch(Exception) { Debug.Assert(false); }
				}
			}
		}
	}
}
