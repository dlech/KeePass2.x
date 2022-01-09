/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2022 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	// 1.12-1.49.1+
	internal sealed class BitwardenJson112 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Bitwarden JSON"; } }
		public override string DefaultExtension { get { return "json"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			using(StreamReader sr = new StreamReader(sInput, StrUtil.Utf8, true))
			{
				string str = sr.ReadToEnd();
				if(!string.IsNullOrEmpty(str))
				{
					CharStream cs = new CharStream(str);
					ImportRoot(new JsonObject(cs), pwStorage);
				}
			}
		}

		private static void ImportRoot(JsonObject jo, PwDatabase pd)
		{
			if(jo.GetValue<bool>("encrypted", false))
				throw new FormatException(KPRes.NoEncNoCompress);

			Dictionary<string, PwGroup> dGroups = new Dictionary<string, PwGroup>();
			JsonObject[] vGroups = jo.GetValueArray<JsonObject>("folders");
			if(vGroups != null) ImportGroups(vGroups, dGroups, pd);
			dGroups[string.Empty] = pd.RootGroup; // Also when vGroups is null

			Dictionary<string, string> dCollections = new Dictionary<string, string>();
			JsonObject[] vCollections = jo.GetValueArray<JsonObject>("collections");
			if(vCollections != null) ImportCollections(vCollections, dCollections);

			JsonObject[] vEntries = jo.GetValueArray<JsonObject>("items");
			if(vEntries != null) ImportEntries(vEntries, dGroups, dCollections, pd);
		}

		private static void ImportGroups(JsonObject[] vGroups,
			Dictionary<string, PwGroup> dGroups, PwDatabase pd)
		{
			char[] vSep = new char[] { '/' };

			foreach(JsonObject jo in vGroups)
			{
				if(jo == null) { Debug.Assert(false); continue; }

				string strID = (jo.GetValue<string>("id") ?? string.Empty);
				string strName = (jo.GetValue<string>("name") ?? string.Empty);

				dGroups[strID] = pd.RootGroup.FindCreateSubTree(strName, vSep);
			}
		}

		private static void ImportCollections(JsonObject[] vCollections,
			Dictionary<string, string> dCollections)
		{
			foreach(JsonObject jo in vCollections)
			{
				if(jo == null) { Debug.Assert(false); continue; }

				string strID = jo.GetValue<string>("id");
				string strName = jo.GetValue<string>("name");

				if(!string.IsNullOrEmpty(strID) && !string.IsNullOrEmpty(strName))
					dCollections[strID] = strName.Replace("/", " / ");
				else { Debug.Assert(false); }
			}
		}

		private static void ImportEntries(JsonObject[] vEntries,
			Dictionary<string, PwGroup> dGroups, Dictionary<string, string> dCollections,
			PwDatabase pd)
		{
			foreach(JsonObject jo in vEntries)
			{
				if(jo == null) { Debug.Assert(false); continue; }

				ImportEntry(jo, dGroups, dCollections, pd);
			}
		}

		private static void ImportEntry(JsonObject jo, Dictionary<string, PwGroup> dGroups,
			Dictionary<string, string> dCollections, PwDatabase pd)
		{
			PwEntry pe = new PwEntry(true, true);

			PwGroup pg;
			string strGroupID = (jo.GetValue<string>("folderId") ?? string.Empty);
			dGroups.TryGetValue(strGroupID, out pg);
			if(pg == null) { Debug.Assert(false); pg = dGroups[string.Empty]; }
			pg.AddEntry(pe, true);

			string[] vCollections = jo.GetValueArray<string>("collectionIds");
			if(vCollections != null)
			{
				foreach(string strID in vCollections)
				{
					if(string.IsNullOrEmpty(strID)) { Debug.Assert(false); continue; }

					string strName;
					if(dCollections.TryGetValue(strID, out strName))
						pe.AddTag(strName);
					else { Debug.Assert(false); }
				}
			}

			ImportString(jo, "name", pe, PwDefs.TitleField, pd);
			ImportString(jo, "notes", pe, PwDefs.NotesField, pd);

			bool bFav = jo.GetValue<bool>("favorite", false);
			if(bFav) pe.AddTag(PwDefs.FavoriteTag);

			JsonObject joSub = jo.GetValue<JsonObject>("login");
			if(joSub != null) ImportLogin(joSub, pe, pd);

			joSub = jo.GetValue<JsonObject>("card");
			if(joSub != null) ImportCard(joSub, pe, pd);

			JsonObject[] v = jo.GetValueArray<JsonObject>("fields");
			if(v != null) ImportFields(v, pe, pd);

			joSub = jo.GetValue<JsonObject>("identity");
			if(joSub != null) ImportIdentity(joSub, pe, pd);
		}

		private static void ImportLogin(JsonObject jo, PwEntry pe, PwDatabase pd)
		{
			ImportString(jo, "username", pe, PwDefs.UserNameField, pd);
			ImportString(jo, "password", pe, PwDefs.PasswordField, pd);

			ImportString(jo, "totp", pe, "TOTP", pd);
			ProtectedString ps = pe.Strings.Get("TOTP");
			if(ps != null) pe.Strings.Set("TOTP", ps.WithProtection(true));

			JsonObject[] vUris = jo.GetValueArray<JsonObject>("uris");
			if(vUris != null)
			{
				int iUri = 1;
				foreach(JsonObject joUri in vUris)
				{
					if(joUri == null) { Debug.Assert(false); continue; }

					string str = joUri.GetValue<string>("uri");
					if(!string.IsNullOrEmpty(str))
					{
						ImportUtil.AppendToField(pe, ((iUri == 1) ?
							PwDefs.UrlField : ("URL " + iUri.ToString())),
							str, pd);
						++iUri;
					}
				}
			}
		}

		private static void ImportCard(JsonObject jo, PwEntry pe, PwDatabase pd)
		{
			ImportString(jo, "cardholderName", pe, PwDefs.UserNameField, pd);
			ImportString(jo, "brand", pe, "Brand", pd);
			ImportString(jo, "number", pe, PwDefs.UserNameField, pd);
			ImportString(jo, "code", pe, PwDefs.PasswordField, pd);

			int iYear, iMonth;
			string strYear = (jo.GetValue<string>("expYear") ?? string.Empty);
			int.TryParse(strYear, out iYear);
			if((iYear >= 1) && (iYear <= 9999))
			{
				string strMonth = (jo.GetValue<string>("expMonth") ?? string.Empty);
				int.TryParse(strMonth, out iMonth);
				if((iMonth <= 0) || (iMonth >= 13)) { Debug.Assert(false); iMonth = 1; }

				pe.Expires = true;
				pe.ExpiryTime = TimeUtil.ToUtc(new DateTime(iYear, iMonth, 1), false);
			}
			else { Debug.Assert(strYear.Length == 0); }
		}

		private static void ImportFields(JsonObject[] vFields, PwEntry pe, PwDatabase pd)
		{
			foreach(JsonObject jo in vFields)
			{
				if(jo == null) { Debug.Assert(false); continue; }

				string strName = jo.GetValue<string>("name");
				string strValue = jo.GetValue<string>("value");
				long lType = jo.GetValue<long>("type", 0);

				if(!string.IsNullOrEmpty(strName))
				{
					ImportUtil.AppendToField(pe, strName, strValue, pd);

					if((lType == 1) && !PwDefs.IsStandardField(strName))
					{
						ProtectedString ps = pe.Strings.Get(strName);
						if(ps == null) { Debug.Assert(false); }
						else pe.Strings.Set(strName, ps.WithProtection(true));
					}
				}
				else { Debug.Assert(false); }
			}
		}

		private static void ImportIdentity(JsonObject jo, PwEntry pe, PwDatabase pd)
		{
			ImportString(jo, "title", pe, PwDefs.UserNameField, pd, " ");
			ImportString(jo, "firstName", pe, PwDefs.UserNameField, pd, " ");
			ImportString(jo, "middleName", pe, PwDefs.UserNameField, pd, " ");
			ImportString(jo, "lastName", pe, PwDefs.UserNameField, pd, " ");
			ImportString(jo, "address1", pe, "Address 1", pd);
			ImportString(jo, "address2", pe, "Address 2", pd);
			ImportString(jo, "address3", pe, "Address 3", pd);
			ImportString(jo, "city", pe, "City", pd);
			ImportString(jo, "state", pe, "State / Province", pd);
			ImportString(jo, "postalCode", pe, "Zip / Postal Code", pd);
			ImportString(jo, "country", pe, "Country", pd);
			ImportString(jo, "company", pe, "Company", pd);

			string str = jo.GetValue<string>("email");
			if(!string.IsNullOrEmpty(str))
				ImportUtil.AppendToField(pe, PwDefs.UrlField, "mailto:" + str, pd);

			ImportString(jo, "phone", pe, "Phone", pd);
			ImportString(jo, "ssn", pe, "Social Security Number", pd);
			ImportString(jo, "username", pe, PwDefs.UserNameField, pd);
			ImportString(jo, "passportNumber", pe, "Passport Number", pd);
			ImportString(jo, "licenseNumber", pe, "License Number", pd);
		}

		private static void ImportString(JsonObject jo, string strJsonKey, PwEntry pe,
			string strFieldName, PwDatabase pd)
		{
			ImportString(jo, strJsonKey, pe, strFieldName, pd, null);
		}

		private static void ImportString(JsonObject jo, string strJsonKey, PwEntry pe,
			string strFieldName, PwDatabase pd, string strSep)
		{
			string str = jo.GetValue<string>(strJsonKey);
			if(string.IsNullOrEmpty(str)) return;

			ImportUtil.AppendToField(pe, strFieldName, str, pd, strSep, false);
		}
	}
}
