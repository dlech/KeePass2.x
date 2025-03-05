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
using System.Text;
using System.Xml;

using KeePass.Util;

using KeePassLib;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	// 2.3.4-2.6.2+
	internal static class PwAgentXml2
	{
		private const string ElemGroup = "group";

		internal static void Import(PwDatabase pd, XmlDocument d)
		{
			XmlNode xnRoot = d.DocumentElement;
			Debug.Assert(xnRoot.Name == "data");

			foreach(XmlNode xn in xnRoot.ChildNodes)
			{
				if(xn.Name == ElemGroup)
					ReadGroup(xn, pd.RootGroup, pd);
				else { Debug.Assert(false); }
			}
		}

		private static void ReadGroup(XmlNode xnGroup, PwGroup pgParent, PwDatabase pd)
		{
			PwGroup pg = new PwGroup(true, true);
			pgParent.AddGroup(pg, true);

			foreach(XmlNode xn in xnGroup)
			{
				if(xn.Name == "name")
					pg.Name = XmlUtil.SafeInnerText(xn);
				else if(xn.Name == ElemGroup)
					ReadGroup(xn, pg, pd);
				else if(xn.Name == "entry")
					ReadEntry(xn, pg, pd);
				else { Debug.Assert(false); }
			}
		}

		private static void ReadEntry(XmlNode xnEntry, PwGroup pgParent, PwDatabase pd)
		{
			PwEntry pe = new PwEntry(true, true);
			pgParent.AddEntry(pe, true);

			DateTime dt;
			foreach(XmlNode xn in xnEntry)
			{
				if(xn.Name == "name")
					ImportUtil.Add(pe, PwDefs.TitleField, xn, pd);
				else if(xn.Name == "type")
					pe.IconId = ((XmlUtil.SafeInnerText(xn) != "1") ?
						PwIcon.Key : PwIcon.PaperNew);
				else if(xn.Name == "account")
					ImportUtil.Add(pe, PwDefs.UserNameField, xn, pd);
				else if(xn.Name == "password")
					ImportUtil.Add(pe, PwDefs.PasswordField, xn, pd);
				else if(xn.Name == "link")
					ImportUtil.Add(pe, PwDefs.UrlField, xn, pd);
				else if(xn.Name == "note")
					ImportUtil.Add(pe, PwDefs.NotesField, xn, pd);
				else if(xn.Name == "date_added")
				{
					if(ParseDate(xn, out dt)) pe.CreationTime = dt;
				}
				else if(xn.Name == "date_modified")
				{
					if(ParseDate(xn, out dt)) pe.LastModificationTime = dt;
				}
				else if(xn.Name == "date_expire")
				{
					if(ParseDate(xn, out dt)) { pe.ExpiryTime = dt; pe.Expires = true; }
				}
				else { Debug.Assert(false); }
			}
		}

		private static bool ParseDate(XmlNode xn, out DateTime dtOut)
		{
			string strDate = XmlUtil.SafeInnerText(xn);
			if(strDate.Length == 0)
			{
				dtOut = default(DateTime);
				return false;
			}

			if(DateTime.TryParse(strDate, out dtOut))
			{
				dtOut = TimeUtil.ToUtc(dtOut, false);
				return true;
			}

			Debug.Assert(false);
			return false;
		}
	}
}
