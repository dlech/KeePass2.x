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
	// 3.02-3.30+
	internal sealed class PwSafeXml302 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Password Safe XML"; } }
		public override string DefaultExtension { get { return "xml"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		private sealed class DatePasswordPair
		{
			public DateTime Time = DateTime.UtcNow;
			public string Password = string.Empty;
		}

		public override void Import(PwDatabase pdStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			string strData = MemUtil.ReadString(sInput, StrUtil.Utf8);

			if(strData.StartsWith("<?xml version=\"1.0\" encoding=\"UTF-8\"?>",
				StrUtil.CaseIgnoreCmp) && (strData.IndexOf(
				"WhatSaved=\"Password Safe V3.29\"", StrUtil.CaseIgnoreCmp) >= 0))
			{
				// Fix broken XML exported by Password Safe 3.29;
				// this has been fixed in 3.30
				strData = strData.Replace("<DefaultUsername<![CDATA[",
					"<DefaultUsername><![CDATA[");
				strData = strData.Replace("<DefaultSymbols<![CDATA[",
					"<DefaultSymbols><![CDATA[");
			}

			XmlDocument xmlDoc = XmlUtilEx.LoadXmlDocumentFromString(strData);
			XmlNode xmlRoot = xmlDoc.DocumentElement;

			string strLineBreak = "\n";
			try
			{
				XmlAttributeCollection xac = xmlRoot.Attributes;
				XmlNode xmlBreak = xac.GetNamedItem("delimiter");
				string strBreak = xmlBreak.Value;

				if(!string.IsNullOrEmpty(strBreak))
					strLineBreak = strBreak;
				else { Debug.Assert(false); }
			}
			catch(Exception) { Debug.Assert(false); }

			foreach(XmlNode xmlChild in xmlRoot.ChildNodes)
			{
				if(xmlChild.Name == "entry")
					ImportEntry(xmlChild, pdStorage, strLineBreak);
			}

			XmlNode xnUse = xmlRoot.SelectSingleNode("Preferences/UseDefaultUser");
			if(xnUse != null)
			{
				string strUse = XmlUtil.SafeInnerText(xnUse);
				if(StrUtil.StringToBool(strUse))
				{
					XmlNode xn = xmlRoot.SelectSingleNode("Preferences/DefaultUsername");
					if((xn != null) && (pdStorage.DefaultUserName.Length == 0))
					{
						pdStorage.DefaultUserName = XmlUtil.SafeInnerText(xn);
						if(pdStorage.DefaultUserName.Length != 0)
							pdStorage.DefaultUserNameChanged = DateTime.UtcNow;
					}
				}
			}
		}

		private static void ImportEntry(XmlNode xmlNode, PwDatabase pd,
			string strLineBreak)
		{
			Debug.Assert(xmlNode != null); if(xmlNode == null) return;

			PwEntry pe = new PwEntry(true, true);
			string strGroupName = string.Empty;

			List<DatePasswordPair> listHistory = null;

			foreach(XmlNode xmlChild in xmlNode.ChildNodes)
			{
				if(xmlChild.Name == "group")
					strGroupName = XmlUtil.SafeInnerText(xmlChild);
				else if(xmlChild.Name == "title")
					ImportUtil.Add(pe, PwDefs.TitleField, xmlChild, pd);
				else if(xmlChild.Name == "username")
					ImportUtil.Add(pe, PwDefs.UserNameField, xmlChild, pd);
				else if(xmlChild.Name == "password")
					ImportUtil.Add(pe, PwDefs.PasswordField, xmlChild, pd);
				else if(xmlChild.Name == "url")
					ImportUtil.Add(pe, PwDefs.UrlField, xmlChild, pd);
				else if(xmlChild.Name == "notes")
					ImportUtil.Add(pe, PwDefs.NotesField, XmlUtil.SafeInnerText(
						xmlChild, strLineBreak), pd);
				else if(xmlChild.Name == "email")
					ImportUtil.Add(pe, "E-Mail", xmlChild, pd);
				else if(xmlChild.Name == "ctime")
					pe.CreationTime = ReadDateTime(xmlChild);
				else if(xmlChild.Name == "atime")
					pe.LastAccessTime = ReadDateTime(xmlChild);
				else if(xmlChild.Name == "ltime")
				{
					pe.ExpiryTime = ReadDateTime(xmlChild);
					pe.Expires = true;
				}
				else if(xmlChild.Name == "pmtime") // = last mod.
					pe.LastModificationTime = ReadDateTime(xmlChild);
				else if(xmlChild.Name == "rmtime") // = last mod.
					pe.LastModificationTime = ReadDateTime(xmlChild);
				else if(xmlChild.Name == "ctimex")
					pe.CreationTime = ReadDateTimeX(xmlChild);
				else if(xmlChild.Name == "atimex")
					pe.LastAccessTime = ReadDateTimeX(xmlChild);
				else if(xmlChild.Name == "xtimex") // Yes, inconsistent
				{
					pe.ExpiryTime = ReadDateTimeX(xmlChild);
					pe.Expires = true;
				}
				else if(xmlChild.Name == "pmtimex") // = last mod.
					pe.LastModificationTime = ReadDateTimeX(xmlChild);
				else if(xmlChild.Name == "rmtimex") // = last mod.
					pe.LastModificationTime = ReadDateTimeX(xmlChild);
				else if(xmlChild.Name == "autotype")
					pe.AutoType.DefaultSequence = XmlUtil.SafeInnerText(xmlChild);
				else if(xmlChild.Name == "runcommand")
					pe.OverrideUrl = XmlUtil.SafeInnerText(xmlChild);
				else if(xmlChild.Name == "pwhistory")
					listHistory = ReadEntryHistory(xmlChild);
			}

			if(listHistory != null)
			{
				string strPassword = pe.Strings.ReadSafe(PwDefs.PasswordField);
				DateTime dtLastMod = pe.LastModificationTime;

				foreach(DatePasswordPair dpp in listHistory)
				{
					pe.Strings.Set(PwDefs.PasswordField, new ProtectedString(
						pd.MemoryProtection.ProtectPassword, dpp.Password));
					pe.LastModificationTime = dpp.Time;

					pe.CreateBackup(null);
				}
				// Maintain backups manually now (backups from the imported file
				// might have been out of order)
				pe.MaintainBackups(pd);

				pe.Strings.Set(PwDefs.PasswordField, new ProtectedString(
					pd.MemoryProtection.ProtectPassword, strPassword));
				pe.LastModificationTime = dtLastMod;
			}

			PwGroup pgContainer = pd.RootGroup;
			if(strGroupName.Length != 0)
				pgContainer = pd.RootGroup.FindCreateSubTree(strGroupName,
					new string[1] { "." }, true);
			pgContainer.AddEntry(pe, true);
			pgContainer.IsExpanded = true;
		}

		private static DateTime ReadDateTime(XmlNode xmlNode)
		{
			Debug.Assert(xmlNode != null); if(xmlNode == null) return DateTime.UtcNow;

			int[] vTimeParts = new int[6];
			DateTime dtTemp;
			foreach(XmlNode xmlChild in xmlNode.ChildNodes)
			{
				if(xmlChild.Name == "date")
				{
					if(DateTime.TryParse(XmlUtil.SafeInnerText(xmlChild), out dtTemp))
					{
						vTimeParts[0] = dtTemp.Year;
						vTimeParts[1] = dtTemp.Month;
						vTimeParts[2] = dtTemp.Day;
					}
				}
				else if(xmlChild.Name == "time")
				{
					if(DateTime.TryParse(XmlUtil.SafeInnerText(xmlChild), out dtTemp))
					{
						vTimeParts[3] = dtTemp.Hour;
						vTimeParts[4] = dtTemp.Minute;
						vTimeParts[5] = dtTemp.Second;
					}
				}
				else { Debug.Assert(false); }
			}

			return (new DateTime(vTimeParts[0], vTimeParts[1], vTimeParts[2],
				vTimeParts[3], vTimeParts[4], vTimeParts[5],
				DateTimeKind.Local)).ToUniversalTime();
		}

		private static DateTime ReadDateTimeX(XmlNode xmlNode)
		{
			string strDate = XmlUtil.SafeInnerText(xmlNode);

			DateTime dt;
			if(StrUtil.TryParseDateTime(strDate, out dt))
				return TimeUtil.ToUtc(dt, false);

			Debug.Assert(false);
			return DateTime.UtcNow;
		}

		private static List<DatePasswordPair> ReadEntryHistory(XmlNode xmlNode)
		{
			List<DatePasswordPair> list = null;

			foreach(XmlNode xmlChild in xmlNode)
			{
				if(xmlChild.Name == "history_entries")
					list = ReadEntryHistoryContainer(xmlChild);
			}

			return list;
		}

		private static List<DatePasswordPair> ReadEntryHistoryContainer(XmlNode xmlNode)
		{
			List<DatePasswordPair> list = new List<DatePasswordPair>();

			foreach(XmlNode xmlChild in xmlNode)
			{
				if(xmlChild.Name == "history_entry")
					list.Add(ReadEntryHistoryItem(xmlChild));
			}

			return list;
		}

		private static DatePasswordPair ReadEntryHistoryItem(XmlNode xmlNode)
		{
			DatePasswordPair dpp = new DatePasswordPair();

			foreach(XmlNode xmlChild in xmlNode)
			{
				if(xmlChild.Name == "changed")
					dpp.Time = ReadDateTime(xmlChild);
				else if(xmlChild.Name == "changedx")
					dpp.Time = ReadDateTimeX(xmlChild);
				else if(xmlChild.Name == "oldpassword")
					dpp.Password = XmlUtil.SafeInnerText(xmlChild);
			}

			return dpp;
		}
	}
}
