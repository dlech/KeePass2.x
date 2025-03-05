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

using KeePass.Util;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	internal sealed class KeePassXml1x : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "KeePass XML (1.x)"; } }
		public override string DefaultExtension { get { return "xml"; } }
		public override string ApplicationGroup { get { return PwDefs.ShortProductName; } }

		public override bool SupportsUuids { get { return true; } }

		public override void Import(PwDatabase pdStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			XmlDocument xd = XmlUtilEx.LoadXmlDocument(sInput, StrUtil.Utf8);

			XmlNode xnRoot = xd.DocumentElement;
			Debug.Assert(xnRoot.Name == "pwlist");

			int nNodeCount = xnRoot.ChildNodes.Count;
			for(int i = 0; i < nNodeCount; ++i)
			{
				XmlNode xn = xnRoot.ChildNodes[i];

				if(xn.Name == "pwentry")
					ReadEntry(xn, pdStorage);
				else { Debug.Assert(false); }

				if(slLogger != null)
					slLogger.SetProgress((uint)(((i + 1) * 100) / nNodeCount));
			}
		}

		private static void ReadEntry(XmlNode xnEntry, PwDatabase pd)
		{
			PwEntry pe = new PwEntry(true, true);
			PwGroup pg = pd.RootGroup;

			string strAttachDesc = null, strAttachment = null;

			foreach(XmlNode xn in xnEntry)
			{
				if(xn.Name == "group")
				{
					XmlNode xnTree = xn.Attributes.GetNamedItem("tree");
					string strPreTree = ((xnTree != null) ? xnTree.Value : null);

					string strLast = XmlUtil.SafeInnerText(xn);
					string strGroup = (string.IsNullOrEmpty(strPreTree) ?
						strLast : (strPreTree + "\\" + strLast));

					pg = pd.RootGroup.FindCreateSubTree(strGroup,
						new string[1] { "\\" }, true);
				}
				else if(xn.Name == "title")
					ImportUtil.Add(pe, PwDefs.TitleField, xn, pd);
				else if(xn.Name == "username")
					ImportUtil.Add(pe, PwDefs.UserNameField, xn, pd);
				else if(xn.Name == "url")
					ImportUtil.Add(pe, PwDefs.UrlField, xn, pd);
				else if(xn.Name == "password")
					ImportUtil.Add(pe, PwDefs.PasswordField, xn, pd);
				else if(xn.Name == "notes")
					ImportUtil.Add(pe, PwDefs.NotesField, xn, pd);
				else if(xn.Name == "uuid")
					pe.SetUuid(new PwUuid(MemUtil.HexStringToByteArray(
						XmlUtil.SafeInnerText(xn))), false);
				else if(xn.Name == "image")
				{
					int i;
					if(int.TryParse(XmlUtil.SafeInnerText(xn), out i))
					{
						if((i >= 0) && (i < (int)PwIcon.Count))
							pe.IconId = (PwIcon)i;
						else { Debug.Assert(false); }
					}
					else { Debug.Assert(false); }
				}
				else if(xn.Name == "creationtime")
					pe.CreationTime = ParseTime(xn);
				else if(xn.Name == "lastmodtime")
					pe.LastModificationTime = ParseTime(xn);
				else if(xn.Name == "lastaccesstime")
					pe.LastAccessTime = ParseTime(xn);
				else if(xn.Name == "expiretime")
				{
					XmlNode xnExpires = xn.Attributes.GetNamedItem("expires");
					if((xnExpires != null) && StrUtil.StringToBool(xnExpires.Value))
					{
						pe.Expires = true;
						pe.ExpiryTime = ParseTime(xn);
					}
					else { Debug.Assert(ParseTime(xn).Year == 2999); }
				}
				else if(xn.Name == "attachdesc")
					strAttachDesc = XmlUtil.SafeInnerText(xn);
				else if(xn.Name == "attachment")
					strAttachment = XmlUtil.SafeInnerText(xn);
				else { Debug.Assert(false); }
			}

			if(!string.IsNullOrEmpty(strAttachDesc) && (strAttachment != null))
			{
				byte[] pbData = Convert.FromBase64String(strAttachment);
				ProtectedBinary pb = new ProtectedBinary(false, pbData);
				pe.Binaries.Set(strAttachDesc, pb);
			}

			pg.AddEntry(pe, true);
		}

		private static DateTime ParseTime(XmlNode xn)
		{
			string str = XmlUtil.SafeInnerText(xn);
			if(string.IsNullOrEmpty(str)) { Debug.Assert(false); return DateTime.UtcNow; }
			if(str == "0000-00-00T00:00:00") return DateTime.UtcNow;

			DateTime dt;
			if(DateTime.TryParse(str, out dt))
				return TimeUtil.ToUtc(dt, false);

			Debug.Assert(false);
			return DateTime.UtcNow;
		}
	}
}
