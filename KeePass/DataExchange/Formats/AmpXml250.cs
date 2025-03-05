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
using System.Text.RegularExpressions;
using System.Xml;

using KeePass.Resources;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	// 2.50-3.21+
	internal sealed class AmpXml250 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Alle meine Passworte XML"; } }
		public override string DefaultExtension { get { return "xml"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override void Import(PwDatabase pdStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			string strXml = MemUtil.ReadString(sInput, Encoding.Default);

			strXml = Regex.Replace(strXml, "<!DOCTYPE\\s+AmP_FILE\\s*\\[.*?\\]>",
				string.Empty, RegexOptions.Singleline);
			strXml = XmlUtil.DecodeHtmlEntities(strXml);

			XmlDocument xd = XmlUtilEx.LoadXmlDocumentFromString(strXml);

			XmlElement xmlRoot = xd.DocumentElement;
			Debug.Assert(xmlRoot.Name == "AmP_FILE");

			foreach(XmlNode xmlChild in xmlRoot.ChildNodes)
			{
				if(xmlChild.Name == "DATA")
					LoadDataNode(xmlChild, pdStorage, slLogger);
				else if(xmlChild.Name == "INFO") { }
				else { Debug.Assert(false); }
			}
		}

		private static void LoadDataNode(XmlNode xmlNode, PwDatabase pd,
			IStatusLogger slLogger)
		{
			uint uCat = 0, uCount = (uint)xmlNode.ChildNodes.Count;
			foreach(XmlNode xmlCategory in xmlNode.ChildNodes)
			{
				LoadCategoryNode(xmlCategory, pd);
				++uCat;
				ImportUtil.SetStatus(slLogger, (uCat * 100) / uCount);
			}
		}

		private static void LoadCategoryNode(XmlNode xmlNode, PwDatabase pd)
		{
			PwGroup pg = new PwGroup(true, true, xmlNode.Name, PwIcon.Folder);
			pd.RootGroup.AddGroup(pg, true);

			PwEntry pe = new PwEntry(true, true);

			foreach(XmlNode xmlChild in xmlNode)
			{
				string strInner = XmlUtil.SafeInnerText(xmlChild);
				if(strInner == "n/a") strInner = string.Empty;

				if(xmlChild.Name == "Kategorie")
				{
					// strInner may contain special characters, thus
					// update the group name now
					pg.Name = strInner;
				}
				else if(xmlChild.Name == "Bezeichnung")
				{
					AddEntryIfValid(pg, ref pe);

					Debug.Assert(strInner.Length > 0);
					ImportUtil.Add(pe, PwDefs.TitleField, strInner, pd);
				}
				else if(xmlChild.Name == "Benutzername")
					ImportUtil.Add(pe, PwDefs.UserNameField, strInner, pd);
				else if(xmlChild.Name == "Passwort1")
					ImportUtil.Add(pe, PwDefs.PasswordField, strInner, pd);
				else if(xmlChild.Name == "Passwort2")
				{
					if((strInner.Length > 0) && (strInner != "keins"))
						pe.Strings.Set(PwDefs.PasswordField + " 2", new ProtectedString(
							pd.MemoryProtection.ProtectPassword, strInner));
				}
				else if(xmlChild.Name == "Ablaufdatum")
				{
					if((strInner.Length > 0) && (strInner != "nie"))
					{
						try
						{
							DateTime dt = DateTime.Parse(strInner);
							pe.ExpiryTime = TimeUtil.ToUtc(dt, false);
							pe.Expires = true;
						}
						catch(Exception) { Debug.Assert(false); }
					}
				}
				else if(xmlChild.Name == "URL_Programm")
					ImportUtil.Add(pe, PwDefs.UrlField, strInner, pd);
				else if(xmlChild.Name == "Kommentar")
					ImportUtil.Add(pe, PwDefs.NotesField, strInner, pd);
				else if(xmlChild.Name == "Benutzerdefinierte_Felder")
					LoadCustomFields(xmlChild, pe, pd);
				else { Debug.Assert(false); }
			}

			AddEntryIfValid(pg, ref pe);
		}

		private static void AddEntryIfValid(PwGroup pgContainer, ref PwEntry pe)
		{
			try
			{
				if(pe == null) return;
				if(pe.Strings.ReadSafe(PwDefs.TitleField).Length == 0) return;

				pgContainer.AddEntry(pe, true);
			}
			finally { pe = new PwEntry(true, true); }
		}

		private static void LoadCustomFields(XmlNode xmlNode, PwEntry pe,
			PwDatabase pd)
		{
			string strKey = string.Empty;

			foreach(XmlNode xn in xmlNode.ChildNodes)
			{
				if(xn.Name == "name")
					strKey = XmlUtil.SafeInnerText(xn);
				else if(xn.Name == "wert")
				{
					if(strKey.Length == 0) { Debug.Assert(false); continue; }

					ImportUtil.Add(pe, strKey, xn, pd);
				}
			}
		}
	}
}
