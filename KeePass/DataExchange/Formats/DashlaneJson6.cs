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
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using KeePass.Resources;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	// 6.2039.0+
	internal sealed class DashlaneJson6 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Dashlane JSON (\u2265 6)"; } }
		public override string DefaultExtension { get { return "json"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override bool ImportAppendsToRootGroupOnly { get { return true; } }

		public override void Import(PwDatabase pdStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			string str = MemUtil.ReadString(sInput, StrUtil.Utf8);
			if(string.IsNullOrEmpty(str)) return;

			CharStream cs = new CharStream(str);
			ImportRoot(new JsonObject(cs), pdStorage);
		}

		private static void ImportRoot(JsonObject jo, PwDatabase pd)
		{
			foreach(string strType in jo.Items.Keys)
			{
				if(strType == null) { Debug.Assert(false); continue; }
				string strTypeNorm = strType.Trim().ToLowerInvariant();

				JsonObject[] vEntries = jo.GetValueArray<JsonObject>(strType);
				if(vEntries == null) { Debug.Assert(false); continue; }

				foreach(JsonObject joEntry in vEntries)
				{
					if(joEntry == null) { Debug.Assert(false); continue; }
					ImportEntry(joEntry, pd, strTypeNorm);
				}
			}
		}

		private static void ImportEntry(JsonObject jo, PwDatabase pd, string strTypeNorm)
		{
			PwEntry pe = new PwEntry(true, true);
			pd.RootGroup.AddEntry(pe, true);

			if(strTypeNorm.StartsWith("paymentmean"))
				strTypeNorm = "paymentmean";

			switch(strTypeNorm)
			{
				case "bankstatement":
				case "fiscalstatement":
					pe.IconId = PwIcon.Homebanking;
					break;

				case "driverlicence":
				case "idcard":
				case "passport":
				case "socialsecuritystatement":
					pe.IconId = PwIcon.Identity;
					break;

				case "email":
					pe.IconId = PwIcon.EMail;
					break;

				case "identity":
					pe.IconId = PwIcon.UserCommunication;
					break;

				case "paymentmean":
					pe.IconId = PwIcon.Money;
					break;

				default:
					Debug.Assert(strTypeNorm == "authentifiant");
					break;
			}

			foreach(KeyValuePair<string, object> kvp in jo.Items)
			{
				string strValue = (kvp.Value as string);
				if(strValue == null)
				{
					Debug.Assert(false);
					if(kvp.Value != null) strValue = kvp.Value.ToString();
				}
				if(strValue == null) { Debug.Assert(false); continue; }

				// Ignore GUIDs
				if(Regex.IsMatch(strValue, "^\\{\\w{8}-\\w{4}-\\w{4}-\\w{4}-\\w{12}\\}$",
					RegexOptions.Singleline))
					continue;

				string strKey = kvp.Key;
				if(strKey == null) { Debug.Assert(false); continue; }
				strKey = strKey.Trim();
				if(string.IsNullOrEmpty(strKey)) { Debug.Assert(false); continue; }
				if(strKey.StartsWith("BankAccount", StrUtil.CaseIgnoreCmp) &&
					(strKey.Length > 11))
					strKey = strKey.Substring(11);
				if(strKey.IndexOf("Number", StrUtil.CaseIgnoreCmp) >= 0)
					strKey = PwDefs.UserNameField;
				strKey = (new string(char.ToUpper(strKey[0]), 1)) +
					strKey.Substring(1);

				string strNorm = strKey.ToLowerInvariant();
				switch(strNorm)
				{
					case "fullname":
					case "name":
					case "socialsecurityfullname":
						strKey = PwDefs.TitleField;
						break;

					case "secondarylogin":
						strKey = KPRes.UserName;
						break;

					case "domain":
						strKey = PwDefs.UrlField;
						break;

					case "expiredate":
						DateTime? odt = ParseDate(strValue);
						if(odt.HasValue)
						{
							pe.Expires = true;
							pe.ExpiryTime = odt.Value;
							strValue = string.Empty;
						}
						break;

					default:
						if(!strNorm.Contains("date") && !strNorm.Contains("time"))
							strKey = ImportUtil.MapName(strKey, true);
						break;
				}

				if(strKey == PwDefs.UrlField)
					strValue = ImportUtil.FixUrl(strValue);
				else if(strNorm.Contains("time"))
					strValue = TryConvertTime(strValue);

				if(!string.IsNullOrEmpty(strValue))
					ImportUtil.Add(pe, strKey, strValue, pd);
			}
		}

		private static DateTime? ParseDate(string str)
		{
			if(string.IsNullOrEmpty(str)) { Debug.Assert(false); return null; }

			DateTime dt;
			if(DateTime.TryParseExact(str.Trim(), "yyyy'-'M'-'d",
				NumberFormatInfo.InvariantInfo, DateTimeStyles.AssumeLocal, out dt))
				return TimeUtil.ToUtc(dt, false);

			Debug.Assert(false);
			return null;
		}

		private static string TryConvertTime(string str)
		{
			if(string.IsNullOrEmpty(str)) return string.Empty;

			try
			{
				if(Regex.IsMatch(str, "^\\d+$", RegexOptions.Singleline))
				{
					ulong u;
					if(ulong.TryParse(str, out u))
					{
						DateTime dt = TimeUtil.ConvertUnixTime(u);
						if(dt > TimeUtil.UnixRoot)
							return TimeUtil.ToDisplayString(dt);
					}
					else { Debug.Assert(false); }
				}
			}
			catch(Exception) { Debug.Assert(false); }

			return str;
		}
	}
}
