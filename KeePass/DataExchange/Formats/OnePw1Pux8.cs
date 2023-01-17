﻿/*
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

using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;
using KeePass.Util.Archive;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	// 8.7.0 (1PUX version 3)
	internal sealed class OnePw1Pux8 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "1Password 1PUX"; } }
		public override string DefaultExtension { get { return "1pux"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			using(ZipArchiveEx za = new ZipArchiveEx(sInput))
			{
				string str;
				using(Stream s = za.OpenEntry("export.data"))
				{
					if(s == null) throw new FormatException();

					using(StreamReader sr = new StreamReader(s, StrUtil.Utf8, true))
					{
						str = sr.ReadToEnd();
					}
				}

				CharStream cs = new CharStream(str);
				JsonObject jo = new JsonObject(cs);
				ImportRoot(jo, za, pwStorage);
			}
		}

		private static void ImportRoot(JsonObject jo, ZipArchiveEx za, PwDatabase pd)
		{
			foreach(JsonObject joAcc in jo.GetValueArray<JsonObject>("accounts", true))
			{
				foreach(JsonObject joVault in joAcc.GetValueArray<JsonObject>("vaults", true))
				{
					PwGroup pg = pd.RootGroup;

					JsonObject joAttrs = joVault.GetValue<JsonObject>("attrs");
					if(joAttrs != null)
					{
						string strGroup = joAttrs.GetValue<string>("name");
						if(!string.IsNullOrEmpty(strGroup))
						{
							pg = new PwGroup(true, true);
							pg.Name = strGroup;
							pd.RootGroup.AddGroup(pg, true, false);

							string strNotes = joAttrs.GetValue<string>("desc");
							if(!string.IsNullOrEmpty(strNotes))
								pg.Notes = strNotes;
						}
						else { Debug.Assert(false); }
					}
					else { Debug.Assert(false); }

					foreach(JsonObject joItem in joVault.GetValueArray<JsonObject>("items", true))
						ImportItem(joItem, za, pg, pd);
				}
			}
		}

		private static void ImportItem(JsonObject jo, ZipArchiveEx za, PwGroup pg,
			PwDatabase pd)
		{
			PwEntry pe = new PwEntry(true, true);
			pg.AddEntry(pe, true, false);

			if(jo.GetValue<long>("favIndex", 0) != 0)
				pe.AddTag(PwDefs.FavoriteTag);

			ulong u = jo.GetValue<ulong>("createdAt", 0);
			if(u != 0) pe.CreationTime = TimeUtil.ConvertUnixTime(u);
			else { Debug.Assert(false); }

			u = jo.GetValue<ulong>("updatedAt", 0);
			if(u != 0) pe.LastModificationTime = TimeUtil.ConvertUnixTime(u);
			else { Debug.Assert(false); }

			JsonObject joOverview = jo.GetValue<JsonObject>("overview");
			if(joOverview != null)
			{
				ImportUtil.AppendToField(pe, PwDefs.TitleField,
					joOverview.GetValue<string>("title"), pd);

				string strUrl = joOverview.GetValue<string>("url");
				ImportUtil.AppendToField(pe, PwDefs.UrlField, strUrl, pd);

				foreach(JsonObject joUrl in joOverview.GetValueArray<JsonObject>("urls", true))
				{
					string str = joUrl.GetValue<string>("url");
					if(str != strUrl)
						ImportUtil.CreateFieldWithIndex(pe.Strings, PwDefs.UrlField,
							str, pd, false);
				}

				foreach(string strTag in joOverview.GetValueArray<string>("tags", true))
					pe.AddTag(strTag);
			}
			else { Debug.Assert(false); }

			JsonObject joDetails = jo.GetValue<JsonObject>("details");
			if(joDetails != null)
			{
				string str = joDetails.GetValue<string>("notesPlain");
				if(!string.IsNullOrEmpty(str))
					ImportUtil.AppendToField(pe, PwDefs.NotesField, str, pd);

				JsonObject joFile = joDetails.GetValue<JsonObject>("documentAttributes");
				if(joFile != null) ImportAttachment(joFile, za, pe);

				foreach(JsonObject joLF in joDetails.GetValueArray<JsonObject>("loginFields", true))
					ImportLoginField(joLF, za, pe, pd);

				foreach(JsonObject joSection in joDetails.GetValueArray<JsonObject>("sections", true))
				{
					string strSection = joSection.GetValue<string>("title");

					foreach(JsonObject joField in joSection.GetValueArray<JsonObject>("fields", true))
						ImportSectionField(joField, za, strSection, pe, pd);
				}
			}
			else { Debug.Assert(false); }
		}

		private static void ImportLoginField(JsonObject jo, ZipArchiveEx za,
			PwEntry pe, PwDatabase pd)
		{
			string strName;
			string strDsg = jo.GetValue<string>("designation");
			if(strDsg == "username")
				strName = PwDefs.UserNameField;
			else if(strDsg == "password")
				strName = PwDefs.PasswordField;
			else
			{
				strName = jo.GetValue<string>("name");
				if(string.IsNullOrEmpty(strName))
				{
					Debug.Assert(false);
					strName = PwDefs.NotesField;
				}
				else
				{
					string strM = ImportUtil.MapNameToStandardField(strName, true);
					if(!string.IsNullOrEmpty(strM)) strName = strM;
				}
			}

			string strValue = jo.GetValue<string>("value");
			ImportUtil.AppendToField(pe, strName, strValue, pd);
		}

		private static void ImportSectionField(JsonObject jo, ZipArchiveEx za,
			string strSection, PwEntry pe, PwDatabase pd)
		{
			JsonObject joValue = jo.GetValue<JsonObject>("value");
			if(joValue == null) { Debug.Assert(false); return; }

			if(joValue.GetValue<string>("reference") != null) return;

			JsonObject joFile = joValue.GetValue<JsonObject>("file");
			if(joFile != null)
			{
				ImportAttachment(joFile, za, pe);
				return;
			}

			string strName = (jo.GetValue<string>("title") ?? string.Empty);
			if(!string.IsNullOrEmpty(strSection))
			{
				if(strName.Length == 0) strName = strSection;
				else strName = strSection + " - " + strName;
			}
			else if(strName.Length == 0) { Debug.Assert(false); return; }
			else
			{
				string strM = ImportUtil.MapNameToStandardField(strName, false);
				if(!string.IsNullOrEmpty(strM)) strName = strM;
			}

			JsonObject joAddr = joValue.GetValue<JsonObject>("address");
			if(joAddr != null)
			{
				StringBuilder sb = new StringBuilder();
				Action<string> fAppend = delegate(string strN)
				{
					string strV = joAddr.GetValue<string>(strN);
					if(!string.IsNullOrEmpty(strV)) sb.AppendLine(strV);
				};

				fAppend("street");
				fAppend("zip");
				fAppend("city");
				fAppend("state");
				fAppend("country");

				ImportUtil.AppendToField(pe, strName, sb.ToString(), pd);
				return;
			}

			string str = joValue.GetValue<string>("concealed");
			if(str != null)
			{
				ImportUtil.AppendToField(pe, strName, str, pd);
				pe.Strings.EnableProtection(strName, true);
				return;
			}

			ulong u = joValue.GetValue<ulong>("date", 0);
			if(u != 0)
			{
				DateTime dt = TimeUtil.ConvertUnixTime(u);
				str = TimeUtil.ToDisplayString(dt);
				ImportUtil.AppendToField(pe, strName, str, pd);
				return;
			}

			JsonObject joEMail = joValue.GetValue<JsonObject>("email");
			if(joEMail != null)
			{
				ImportUtil.AppendToField(pe, strName, joEMail.GetValue<string>(
					"email_address"), pd);
				return;
			}

			u = joValue.GetValue<ulong>("monthYear", 0);
			if(u != 0)
			{
				str = (u / 100).ToString() + "-" +
					(u % 100).ToString().PadLeft(2, '0');
				ImportUtil.AppendToField(pe, strName, str, pd);
				return;
			}

			str = joValue.GetValue<string>("totp");
			if(str != null)
			{
				try { EntryUtil.ImportOtpAuth(pe, str, pd); }
				catch(Exception)
				{
					Debug.Assert(false);
					ImportUtil.AppendToField(pe, strName, str, pd);
				}
				return;
			}

			foreach(KeyValuePair<string, object> kvp in joValue.Items)
			{
				if(kvp.Value == null) continue; // E.g. null 'date'
				if(kvp.Value is JsonObject) { Debug.Assert(false); continue; }
				ImportUtil.AppendToField(pe, strName, kvp.Value.ToString(), pd);
			}
		}

		private static void ImportAttachment(JsonObject jo, ZipArchiveEx za,
			PwEntry pe)
		{
			string strFileName = jo.GetValue<string>("fileName");
			if(string.IsNullOrEmpty(strFileName)) { Debug.Assert(false); return; }

			string strDocId = jo.GetValue<string>("documentId");
			if(string.IsNullOrEmpty(strDocId)) { Debug.Assert(false); return; }

			byte[] pbData;
			using(Stream s = za.OpenEntry("files/" + strDocId + "__" + strFileName))
			{
				if(s == null) throw new FormatException();
				pbData = MemUtil.Read(s);
			}

			Debug.Assert(pbData.LongLength == jo.GetValue<long>("decryptedSize", -1));

			if(FileDialogsEx.CheckAttachmentSize(pbData.LongLength,
				KPRes.AttachFailed + MessageService.NewLine + strFileName))
				pe.Binaries.Set(strFileName, new ProtectedBinary(false, pbData));
		}
	}
}
