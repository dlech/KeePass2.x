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
	// 0.4.1
	internal sealed class KeePassXXml041 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "KeePassX XML"; } }
		public override string DefaultExtension { get { return "xml"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override bool SupportsUuids { get { return false; } }

		private const string ElemGroup = "group";
		private const string ElemTitle = "title";
		private const string ElemIcon = "icon";

		public override void Import(PwDatabase pdStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			XmlDocument xd = XmlUtilEx.LoadXmlDocument(sInput, StrUtil.Utf8);

			XmlNode xnRoot = xd.DocumentElement;
			Debug.Assert(xnRoot.Name == "database");

			Stack<PwGroup> vGroups = new Stack<PwGroup>();
			vGroups.Push(pdStorage.RootGroup);

			int nNodeCount = xnRoot.ChildNodes.Count;
			for(int i = 0; i < nNodeCount; ++i)
			{
				XmlNode xn = xnRoot.ChildNodes[i];

				if(xn.Name == ElemGroup)
					ReadGroup(xn, vGroups, pdStorage);
				else { Debug.Assert(false); }

				if(slLogger != null)
					slLogger.SetProgress((uint)(((i + 1) * 100) / nNodeCount));
			}
		}

		private static PwIcon ReadIcon(XmlNode xn, PwIcon piDefault)
		{
			int i;
			if(StrUtil.TryParseInt(XmlUtil.SafeInnerText(xn), out i))
			{
				if((i >= 0) && (i < (int)PwIcon.Count)) return (PwIcon)i;
			}
			else { Debug.Assert(false); }

			return piDefault;
		}

		private static void ReadGroup(XmlNode xnGroup, Stack<PwGroup> vGroups,
			PwDatabase pd)
		{
			if(vGroups.Count == 0) { Debug.Assert(false); return; }
			PwGroup pgParent = vGroups.Peek();

			PwGroup pg = new PwGroup(true, true);
			pgParent.AddGroup(pg, true);
			vGroups.Push(pg);

			foreach(XmlNode xn in xnGroup)
			{
				if(xn.Name == ElemTitle)
					pg.Name = XmlUtil.SafeInnerText(xn);
				else if(xn.Name == ElemIcon)
					pg.IconId = ReadIcon(xn, pg.IconId);
				else if(xn.Name == ElemGroup)
					ReadGroup(xn, vGroups, pd);
				else if(xn.Name == "entry")
					ReadEntry(xn, pg, pd);
				else { Debug.Assert(false); }
			}

			vGroups.Pop();
		}

		private static void ReadEntry(XmlNode xnEntry, PwGroup pgParent, PwDatabase pd)
		{
			PwEntry pe = new PwEntry(true, true);
			pgParent.AddEntry(pe, true);

			string strAttachDesc = null, strAttachment = null;

			foreach(XmlNode xn in xnEntry)
			{
				if(xn.Name == ElemTitle)
					ImportUtil.Add(pe, PwDefs.TitleField, xn, pd);
				else if(xn.Name == "username")
					ImportUtil.Add(pe, PwDefs.UserNameField, xn, pd);
				else if(xn.Name == "url")
					ImportUtil.Add(pe, PwDefs.UrlField, xn, pd);
				else if(xn.Name == "password")
					ImportUtil.Add(pe, PwDefs.PasswordField, xn, pd);
				else if(xn.Name == "comment")
					ImportUtil.Add(pe, PwDefs.NotesField, FilterSpecial(xn), pd);
				else if(xn.Name == ElemIcon)
					pe.IconId = ReadIcon(xn, pe.IconId);
				else if(xn.Name == "creation")
					pe.CreationTime = ParseTime(XmlUtil.SafeInnerText(xn));
				else if(xn.Name == "lastmod")
					pe.LastModificationTime = ParseTime(XmlUtil.SafeInnerText(xn));
				else if(xn.Name == "lastaccess")
					pe.LastAccessTime = ParseTime(XmlUtil.SafeInnerText(xn));
				else if(xn.Name == "expire")
				{
					string strDate = XmlUtil.SafeInnerText(xn);
					pe.Expires = (strDate != "Never");
					if(pe.Expires) pe.ExpiryTime = ParseTime(strDate);
				}
				else if(xn.Name == "bindesc")
					strAttachDesc = XmlUtil.SafeInnerText(xn);
				else if(xn.Name == "bin")
					strAttachment = XmlUtil.SafeInnerText(xn);
				else { Debug.Assert(false); }
			}

			if(!string.IsNullOrEmpty(strAttachDesc) && (strAttachment != null))
			{
				byte[] pbData = Convert.FromBase64String(strAttachment);
				ProtectedBinary pb = new ProtectedBinary(false, pbData);
				pe.Binaries.Set(strAttachDesc, pb);
			}
		}

		private static DateTime ParseTime(string str)
		{
			if(string.IsNullOrEmpty(str)) { Debug.Assert(false); return DateTime.UtcNow; }
			if(str == "0000-00-00T00:00:00") return DateTime.UtcNow;

			DateTime dt;
			if(DateTime.TryParse(str, out dt))
				return TimeUtil.ToUtc(dt, false);

			Debug.Assert(false);
			return DateTime.UtcNow;
		}

		private static string FilterSpecial(XmlNode xn)
		{
			string str = XmlUtil.SafeInnerText(xn);
			
			str = str.Replace("<br/>", MessageService.NewLine);
			str = str.Replace("<br />", MessageService.NewLine);

			return StrUtil.XmlToString(str);
		}
	}
}
