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
using System.IO;
using System.Diagnostics;
using System.Drawing;

using KeePass.Resources;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	internal sealed class MozillaBookmarksJson100 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Mozilla Bookmarks JSON 1.00"; } }
		public override string DefaultExtension { get { return "json"; } }
		public override string ApplicationGroup { get { return KPRes.Browser; } }

		private const string m_strGroup = "children";

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

			if(strContent.Length == 0) return;

			CharStream cs = new CharStream(strContent);

			JsonObject jRoot = new JsonObject(cs);
			AddObject(pwStorage.RootGroup, jRoot, pwStorage, false);
			Debug.Assert(cs.PeekChar(true) == char.MinValue);
		}

		private static void AddObject(PwGroup pgStorage, JsonObject jObject,
			PwDatabase pwContext, bool bCreateSubGroups)
		{
			if(jObject.Items.ContainsKey(m_strGroup))
			{
				JsonArray jArray = jObject.Items[m_strGroup].Value as JsonArray;
				if(jArray == null) { Debug.Assert(false); return; }

				PwGroup pgNew;
				if(bCreateSubGroups)
				{
					pgNew = new PwGroup(true, true);
					pgStorage.AddGroup(pgNew, true);

					if(jObject.Items.ContainsKey("title"))
						pgNew.Name = ((jObject.Items["title"].Value != null) ?
							jObject.Items["title"].Value.ToString() : string.Empty);
				}
				else pgNew = pgStorage;

				foreach(JsonValue jValue in jArray.Values)
				{
					JsonObject objSub = jValue.Value as JsonObject;
					if(objSub != null) AddObject(pgNew, objSub, pwContext, true);
					else { Debug.Assert(false); }
				}

				return;
			}

			PwEntry pe = new PwEntry(true, true);

			SetString(pe, "Index", false, jObject, "index");
			SetString(pe, PwDefs.TitleField, pwContext.MemoryProtection.ProtectTitle,
				jObject, "title");
			SetString(pe, "ID", false, jObject, "id");
			SetString(pe, PwDefs.UrlField, pwContext.MemoryProtection.ProtectUrl,
				jObject, "uri");
			SetString(pe, "CharSet", false, jObject, "charset");

			if((pe.Strings.ReadSafe(PwDefs.TitleField).Length > 0) ||
				(pe.Strings.ReadSafe(PwDefs.UrlField).Length > 0))
				pgStorage.AddEntry(pe, true);
		}

		private static void SetString(PwEntry pe, string strEntryKey, bool bProtect,
			JsonObject jObject, string strObjectKey)
		{
			if(jObject.Items.ContainsKey(strObjectKey))
			{
				object obj = jObject.Items[strObjectKey].Value;
				if(obj == null) return;

				pe.Strings.Set(strEntryKey, new ProtectedString(bProtect, obj.ToString()));
			}
		}
	}
}
