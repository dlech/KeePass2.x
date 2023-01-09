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
using System.Xml.XPath;

using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	// KasperskyPwMgrXml50 derives from this

	// 5.0.4.232-8.4.4+
	internal class StickyPwXml50 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Sticky Password XML"; } }
		public override string DefaultExtension { get { return "xml"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			using(XmlReader xr = XmlUtilEx.CreateXmlReader(sInput))
			{
				XPathDocument xpDoc = new XPathDocument(xr);
				XPathNavigator xpNav = xpDoc.CreateNavigator();

				Dictionary<string, PwGroup> dGroups = ImportGroups(xpNav, pwStorage);

				ImportLogins(xpNav, dGroups, pwStorage);
				ImportMemos(xpNav, dGroups, pwStorage);
			}
		}

		private static Dictionary<string, PwGroup> ImportGroups(XPathNavigator xpNav,
			PwDatabase pd)
		{
			List<PwGroup> l = new List<PwGroup>(); // Original order
			Dictionary<string, PwGroup> d = new Dictionary<string, PwGroup>();
			Dictionary<PwGroup, string> dParents = new Dictionary<PwGroup, string>();

			XPathNodeIterator it = xpNav.Select(
				"/root/Database/Groups/Group | /root/Database/SecureMemoGroups/Group");
			while(it.MoveNext())
			{
				XPathNavigator xpGroup = it.Current;
				PwGroup pg = new PwGroup(true, true);

				string strName = xpGroup.GetAttribute("Name", string.Empty);
				if(string.IsNullOrEmpty(strName)) { Debug.Assert(false); strName = KPRes.Group; }
				pg.Name = strName;

				string strID = xpGroup.GetAttribute("ID", string.Empty);
				if(string.IsNullOrEmpty(strID)) { Debug.Assert(false); continue; }
				Debug.Assert(!d.ContainsKey(strID));

				string strParentID = xpGroup.GetAttribute("ParentID", string.Empty);

				l.Add(pg);
				d[strID] = pg;
				if(!string.IsNullOrEmpty(strParentID)) dParents[pg] = strParentID;
				else { Debug.Assert(false); }
			}

			foreach(PwGroup pg in l)
			{
				string strParentID;
				if(dParents.TryGetValue(pg, out strParentID))
				{
					PwGroup pgParent;
					if(d.TryGetValue(strParentID, out pgParent))
					{
						if((pgParent != pg) && !pgParent.IsContainedIn(pg))
						{
							pgParent.AddGroup(pg, true);
							continue;
						}
						else { Debug.Assert(false); }
					}
					// else { Debug.Assert(false); } // May be negative ID
				}
				else { Debug.Assert(false); }

				pd.RootGroup.AddGroup(pg, true);
			}

			d[string.Empty] = pd.RootGroup;
			return d;
		}

		private static void ImportLogins(XPathNavigator xpNav,
			Dictionary<string, PwGroup> dGroups, PwDatabase pd)
		{
			XPathNodeIterator it = xpNav.Select("/root/Database/Logins/Login");
			while(it.MoveNext())
			{
				XPathNavigator xpLogin = it.Current;
				PwEntry pe = new PwEntry(true, true);

				pe.Strings.Set(PwDefs.UserNameField, new ProtectedString(
					pd.MemoryProtection.ProtectUserName,
					xpLogin.GetAttribute("Name", string.Empty)));
				pe.Strings.Set(PwDefs.PasswordField, new ProtectedString(
					pd.MemoryProtection.ProtectPassword,
					xpLogin.GetAttribute("Password", string.Empty)));

				ImportTimes(xpLogin, pe);

				string strID = xpLogin.GetAttribute("ID", string.Empty);

				XPathNavigator xpAccLogin = (!string.IsNullOrEmpty(strID) ?
					xpNav.SelectSingleNode(
					"/root/Database/Accounts/Account/LoginLinks/Login[@SourceLoginID='" +
					strID + "']/../..") : null);
				if(xpAccLogin == null) { Debug.Assert(false); }
				else
				{
					Debug.Assert(xpAccLogin.Name == "Account");

					pe.Strings.Set(PwDefs.TitleField, new ProtectedString(
						pd.MemoryProtection.ProtectTitle,
						xpAccLogin.GetAttribute("Name", string.Empty)));
					pe.Strings.Set(PwDefs.UrlField, new ProtectedString(
						pd.MemoryProtection.ProtectUrl,
						xpAccLogin.GetAttribute("Link", string.Empty)));

					string strNotes = xpAccLogin.GetAttribute("Comments", string.Empty);
					strNotes = strNotes.Replace("/n", Environment.NewLine);
					pe.Strings.Set(PwDefs.NotesField, new ProtectedString(
						pd.MemoryProtection.ProtectNotes, strNotes));
				}

				AddToParent(xpAccLogin, pe, dGroups);
			}
		}

		private static void ImportMemos(XPathNavigator xpNav,
			Dictionary<string, PwGroup> dGroups, PwDatabase pd)
		{
			XPathNodeIterator it = xpNav.Select("/root/Database/SecureMemos/SecureMemo");
			while(it.MoveNext())
			{
				XPathNavigator xpMemo = it.Current;
				PwEntry pe = new PwEntry(true, true);

				pe.IconId = PwIcon.PaperNew;

				pe.Strings.Set(PwDefs.TitleField, new ProtectedString(
					pd.MemoryProtection.ProtectTitle,
					xpMemo.GetAttribute("Name", string.Empty)));

				ImportTimes(xpMemo, pe);

				try
				{
					string strMemoHex = xpMemo.Value;
					byte[] pbMemo = MemUtil.HexStringToByteArray(strMemoHex);
					string strMemoRtf = Encoding.Unicode.GetString(pbMemo);

					pe.Binaries.Set(KPRes.Notes + ".rtf", new ProtectedBinary(
						false, StrUtil.Utf8.GetBytes(strMemoRtf)));
				}
				catch(Exception) { Debug.Assert(false); }

				AddToParent(xpMemo, pe, dGroups);
			}
		}

		private static void AddToParent(XPathNavigator xpNav, PwEntry pe,
			Dictionary<string, PwGroup> dGroups)
		{
			if(xpNav != null)
			{
				string strParentID = xpNav.GetAttribute("ParentID", string.Empty);
				if(!string.IsNullOrEmpty(strParentID))
				{
					PwGroup pgParent;
					if(dGroups.TryGetValue(strParentID, out pgParent))
					{
						pgParent.AddEntry(pe, true);
						return;
					}
				}
				else { Debug.Assert(false); }
			}

			dGroups[string.Empty].AddEntry(pe, true);
		}

		private static DateTime? GetTime(XPathNavigator xpNav, string strAttrib)
		{
			try
			{
				string str = xpNav.GetAttribute(strAttrib, string.Empty);
				if(!string.IsNullOrEmpty(str))
					return TimeUtil.ToUtc(XmlConvert.ToDateTime(str,
						XmlDateTimeSerializationMode.Local), false);
			}
			catch(Exception) { Debug.Assert(false); }

			return null;
		}

		private static void ImportTimes(XPathNavigator xpNav, PwEntry pe)
		{
			DateTime? odt = GetTime(xpNav, "CreatedDate");
			if(odt.HasValue) pe.CreationTime = odt.Value;
			else { Debug.Assert(false); }

			odt = GetTime(xpNav, "ModifiedDate");
			if(odt.HasValue) pe.LastModificationTime = odt.Value;
			else { Debug.Assert(false); }

			odt = GetTime(xpNav, "ExpirationDate");
			if(odt.HasValue)
			{
				pe.Expires = true;
				pe.ExpiryTime = odt.Value;
			}
		}
	}
}
