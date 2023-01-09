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
using System.Globalization;
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
	// 1.22+
	internal sealed class KeyFolderXml1 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Key Folder XML"; } }
		public override string DefaultExtension { get { return "xml"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			using(StreamReader sr = new StreamReader(sInput, Encoding.UTF8, true))
			{
				XmlDocument d = XmlUtilEx.CreateXmlDocument();
				d.LoadXml(sr.ReadToEnd());

				ImportGroup(d.DocumentElement, pwStorage.RootGroup, pwStorage);
			}
		}

		private static void ImportGroup(XmlNode xnFolder, PwGroup pg, PwDatabase pd)
		{
			foreach(XmlNode xn in xnFolder.ChildNodes)
			{
				switch(xn.Name)
				{
					case "name":
						pg.Name = XmlUtil.SafeInnerText(xn);
						break;

					case "folder":
						PwGroup pgSub = new PwGroup(true, true);
						pg.AddGroup(pgSub, true);
						ImportGroup(xn, pgSub, pd);
						break;

					case "item":
						ImportEntry(xn, pg, pd);
						break;

					default:
						Debug.Assert(false); // Unknown node
						break;
				}
			}
		}

		private static void ImportEntry(XmlNode xnItem, PwGroup pg, PwDatabase pd)
		{
			PwEntry pe = new PwEntry(true, true);
			pg.AddEntry(pe, true);

			bool bFirstUrl = true;
			DateTime? odt;

			foreach(XmlNode xn in xnItem.ChildNodes)
			{
				switch(xn.Name)
				{
					case "name":
						ImportUtil.AppendToField(pe, PwDefs.TitleField,
							XmlUtil.SafeInnerText(xn), pd);
						break;

					case "username":
						ImportUtil.AppendToField(pe, PwDefs.UserNameField,
							XmlUtil.SafeInnerText(xn), pd);
						break;

					case "password":
						ImportUtil.AppendToField(pe, PwDefs.PasswordField,
							XmlUtil.SafeInnerText(xn), pd);
						break;

					case "link":
						string strType = GetChildValue(xn, "type");
						string strName = GetChildValue(xn, "name");
						string strUrl = GetChildValue(xn, "url");
						if(string.IsNullOrEmpty(strUrl)) continue;

						if(bFirstUrl)
						{
							strName = PwDefs.UrlField;
							bFirstUrl = false;
						}

						if(strType == "mail") strUrl = "mailto:" + strUrl;

						ImportUtil.CreateFieldWithIndex(pe.Strings, strName,
							strUrl, pd, false);
						break;

					case "note":
						ImportUtil.AppendToField(pe, PwDefs.NotesField,
							XmlUtil.SafeInnerText(xn), pd);
						break;

					case "created":
						odt = TryParseDate(xn);
						if(odt.HasValue) pe.CreationTime = odt.Value;
						break;

					case "changed":
						odt = TryParseDate(xn);
						if(odt.HasValue) pe.LastModificationTime = odt.Value;
						break;

					case "expire":
						odt = TryParseDate(xn);
						if(odt.HasValue)
						{
							pe.Expires = true;
							pe.ExpiryTime = odt.Value;
						}
						break;

					default:
						Debug.Assert(false); // Unknown node
						break;
				}
			}
		}

		private static string GetChildValue(XmlNode xn, string strChildName)
		{
			XmlNode xnChild = XmlUtil.FindMultiChild(xn.ChildNodes, strChildName, 0);
			if(xnChild == null) { Debug.Assert(false); return string.Empty; }
			return XmlUtil.SafeInnerText(xnChild);
		}

		private static DateTime? TryParseDate(XmlNode xn)
		{
			string str = XmlUtil.SafeInnerText(xn);
			if(string.IsNullOrEmpty(str)) { Debug.Assert(false); return null; }

			DateTime dt;
			if(!DateTime.TryParseExact(str, @"dd'.'MM'.'yyyy",
				CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dt))
			{
				Debug.Assert(false);
				if(!StrUtil.TryParseDateTime(str, out dt))
					return null;
			}

			return TimeUtil.ToUtc(dt, false);
		}
	}
}
