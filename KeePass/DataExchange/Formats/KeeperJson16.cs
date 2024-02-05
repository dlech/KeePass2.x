/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2024 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePass.Resources;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	// 16.0.0+
	internal sealed class KeeperJson16 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Keeper JSON"; } }
		public override string DefaultExtension { get { return "json"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			using(StreamReader sr = new StreamReader(sInput, StrUtil.Utf8, true))
			{
				string str = sr.ReadToEnd();
				if(string.IsNullOrEmpty(str)) { Debug.Assert(false); return; }

				CharStream cs = new CharStream(str);
				JsonObject joRoot = new JsonObject(cs);

				JsonObject[] v = joRoot.GetValueArray<JsonObject>("records");
				ImportRecords(v, pwStorage);
			}
		}

		private static void ImportRecords(JsonObject[] v, PwDatabase pd)
		{
			if(v == null) { Debug.Assert(false); return; }

			Dictionary<string, PwGroup> dGroups = new Dictionary<string, PwGroup>();
			string strGroupSep = MemUtil.ByteArrayToHexString(Guid.NewGuid().ToByteArray());
			string strBackspCode = MemUtil.ByteArrayToHexString(Guid.NewGuid().ToByteArray());

			foreach(JsonObject jo in v)
			{
				if(jo == null) { Debug.Assert(false); continue; }

				PwEntry pe = new PwEntry(true, true);

				ImportUtil.AppendToField(pe, PwDefs.TitleField,
					jo.GetValue<string>("title"), pd);
				ImportUtil.AppendToField(pe, PwDefs.UserNameField,
					jo.GetValue<string>("login"), pd);
				ImportUtil.AppendToField(pe, PwDefs.PasswordField,
					jo.GetValue<string>("password"), pd);
				ImportUtil.AppendToField(pe, PwDefs.UrlField,
					jo.GetValue<string>("login_url"), pd);
				ImportUtil.AppendToField(pe, PwDefs.NotesField,
					jo.GetValue<string>("notes"), pd);

				JsonObject joCustom = jo.GetValue<JsonObject>("custom_fields");
				if(joCustom != null)
				{
					foreach(KeyValuePair<string, object> kvp in joCustom.Items)
					{
						string strValue = (kvp.Value as string);
						if(strValue == null) { Debug.Assert(false); continue; }

						if(kvp.Key == "TFC:Keeper")
						{
							try { EntryUtil.ImportOtpAuth(pe, strValue, pd); }
							catch(Exception) { Debug.Assert(false); }
						}
						else ImportUtil.AppendToField(pe, kvp.Key, strValue, pd);
					}
				}

				PwGroup pg = null;
				JsonObject[] vFolders = jo.GetValueArray<JsonObject>("folders");
				if((vFolders != null) && (vFolders.Length >= 1))
				{
					JsonObject joFolder = vFolders[0];
					if(joFolder != null)
					{
						string strGroup = joFolder.GetValue<string>("folder");
						if(!string.IsNullOrEmpty(strGroup))
						{
							strGroup = strGroup.Replace("\\\\", strBackspCode);
							strGroup = strGroup.Replace("\\", strGroupSep);
							strGroup = strGroup.Replace(strBackspCode, "\\");

							if(!dGroups.TryGetValue(strGroup, out pg))
							{
								pg = pd.RootGroup.FindCreateSubTree(strGroup,
									new string[] { strGroupSep }, true);
								dGroups[strGroup] = pg;
							}
						}
						else { Debug.Assert(false); }
					}
					else { Debug.Assert(false); }
				}
				if(pg == null) pg = pd.RootGroup;

				pg.AddEntry(pe, true);
			}
		}
	}
}
