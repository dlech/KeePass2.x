/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2021 Dominik Reichl <dominik.reichl@t-online.de>

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
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	// 19.6.28+
	internal sealed class PwAgentXml3 : FileFormatProvider
	{
		private const string ElemGroup = "group";
		private const string ElemEntry = "item";

		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Password Agent XML"; } }
		public override string DefaultExtension { get { return "xml"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override Image SmallIcon
		{
			get { return KeePass.Properties.Resources.B16x16_Imp_PwAgent; }
		}

		public override void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			byte[] pb = MemUtil.Read(sInput);

			if(ImportOld(pwStorage, pb)) return;

			XmlDocument d = XmlUtilEx.CreateXmlDocument();
			using(MemoryStream ms = new MemoryStream(pb, false))
			{
				using(StreamReader sr = new StreamReader(ms, StrUtil.Utf8, true))
				{
					d.Load(sr);
				}
			}

			XmlNode xmlRoot = d.DocumentElement;
			Debug.Assert(xmlRoot.Name == "data");

			foreach(XmlNode xmlChild in xmlRoot.ChildNodes)
			{
				if(xmlChild.Name == ElemGroup)
					ReadGroup(xmlChild, pwStorage.RootGroup, pwStorage);
				else if(xmlChild.Name == ElemEntry)
					ReadEntry(xmlChild, pwStorage.RootGroup, pwStorage);
				else { Debug.Assert(false); }
			}
		}

		internal static bool ImportOld(PwDatabase pwStorage, byte[] pb)
		{
			// Version 2 uses ANSI encoding, version 3 uses UTF-8
			// encoding (with BOM)
			XmlDocument dAnsi = XmlUtilEx.CreateXmlDocument();
			try
			{
				using(MemoryStream ms = new MemoryStream(pb, false))
				{
					using(StreamReader sr = new StreamReader(ms, Encoding.Default, true))
					{
						dAnsi.Load(sr);

						XmlElement xmlRoot = dAnsi.DocumentElement;

						// Version 2 has three "ver_*" attributes,
						// version 3 has a "version" attribute
						if(xmlRoot.Attributes["ver_major"] == null)
							return false;
					}
				}
			}
			catch(Exception) { Debug.Assert(false); return false; }

			PwAgentXml2.Import(pwStorage, dAnsi);
			return true;
		}

		private static void ReadGroup(XmlNode xmlNode, PwGroup pgParent, PwDatabase pwStorage)
		{
			PwGroup pg = new PwGroup(true, true);
			pgParent.AddGroup(pg, true);

			ReadDate(pg, xmlNode.Attributes["dateModified"], true, true, false);

			foreach(XmlNode xmlChild in xmlNode)
			{
				if(xmlChild.Name == "title")
					pg.Name = XmlUtil.SafeInnerText(xmlChild);
				else if(xmlChild.Name == ElemGroup)
					ReadGroup(xmlChild, pg, pwStorage);
				else if(xmlChild.Name == ElemEntry)
					ReadEntry(xmlChild, pg, pwStorage);
				else { Debug.Assert(false); }
			}
		}

		private static void ReadEntry(XmlNode xmlNode, PwGroup pgParent, PwDatabase pwStorage)
		{
			PwEntry pe = new PwEntry(true, true);
			pgParent.AddEntry(pe, true);

			ReadDate(pe, xmlNode.Attributes["dateModified"], true, true, false);

			foreach(XmlNode xmlChild in xmlNode)
			{
				string str = XmlUtil.SafeInnerText(xmlChild);

				if(xmlChild.Name == "title")
					ImportUtil.AppendToField(pe, PwDefs.TitleField, str, pwStorage);
				else if(xmlChild.Name == "userId")
					ImportUtil.AppendToField(pe, PwDefs.UserNameField, str, pwStorage);
				else if(xmlChild.Name == "password")
					ImportUtil.AppendToField(pe, PwDefs.PasswordField, str, pwStorage);
				else if(xmlChild.Name == "note")
					ImportUtil.AppendToField(pe, PwDefs.NotesField, str, pwStorage);
				else if(xmlChild.Name == "link")
					ImportUtil.AppendToField(pe, PwDefs.UrlField, str, pwStorage);
				else if(xmlChild.Name == "dateAdded")
					ReadDate(pe, xmlChild, true, false, false);
				else if(xmlChild.Name == "dateModified")
					ReadDate(pe, xmlChild, false, true, false);
				else if(xmlChild.Name == "dateExpires")
					ReadDate(pe, xmlChild, false, false, true);
				else if(xmlChild.Name == "attachment")
					ReadAttachment(xmlChild, pe);
				else { Debug.Assert(false); }
			}
		}

		private static void ReadAttachment(XmlNode xmlNode, PwEntry pe)
		{
			XmlNode xnName = xmlNode.SelectSingleNode("fileName");
			XmlNode xnValue = xmlNode.SelectSingleNode("fileBytes");

			string strName = XmlUtil.SafeInnerText(xnName);
			if(strName.Length == 0) { Debug.Assert(false); strName = Guid.NewGuid().ToString(); }

			string strValue = XmlUtil.SafeInnerText(xnValue);
			if(strValue.Length == 0) { Debug.Assert(false); return; }

			pe.Binaries.Set(strName, new ProtectedBinary(false,
				Convert.FromBase64String(strValue)));
		}

		private static void ReadDate(ITimeLogger tl, XmlNode xn,
			bool bSetCreation, bool bSetLastMod, bool bSetExpiry)
		{
			if(tl == null) { Debug.Assert(false); return; }
			if(xn == null) { Debug.Assert(false); return; }

			try
			{
				XmlAttribute xa = (xn as XmlAttribute);
				string str = ((xa != null) ? xa.Value : xn.InnerText);
				if(string.IsNullOrEmpty(str)) return; // No assert

				DateTime dt = TimeUtil.ToUtc(XmlConvert.ToDateTime(
					str, XmlDateTimeSerializationMode.Utc), true);

				if(bSetCreation) tl.CreationTime = dt;
				if(bSetLastMod) tl.LastModificationTime = dt;
				if(bSetExpiry)
				{
					tl.Expires = true;
					tl.ExpiryTime = dt;
				}
			}
			catch(Exception) { Debug.Assert(false); }
		}
	}
}
