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
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

using KeePass.Resources;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	// 2.6-16.0.7+
	internal sealed class PwDepotXml26 : FileFormatProvider
	{
		// Old versions used upper-case node names, whereas
		// newer versions use lower-case node names
		private const string ElemGroup = "group";

		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Password Depot XML"; } }
		public override string DefaultExtension { get { return "xml"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			StreamReader sr = new StreamReader(sInput, StrUtil.Utf8, true);
			string strData = sr.ReadToEnd();
			sr.Close();

			// Remove vertical tabulators
			strData = strData.Replace("\u000B", string.Empty);

			XmlDocument xd = XmlUtilEx.CreateXmlDocument();
			xd.LoadXml(strData);

			PwGroup pgRoot = pwStorage.RootGroup;
			Dictionary<string, IStructureItem> dItems = new Dictionary<string, IStructureItem>();
			string strFavs = null;

			foreach(XmlNode xn in xd.DocumentElement.ChildNodes)
			{
				string strName = xn.Name.ToLowerInvariant();

				if(strName == "passwords")
					ReadContainer(xn, pgRoot, pwStorage, dItems);
				else if(strName == "recyclebin")
				{
					PwGroup pg = new PwGroup(true, true, KPRes.RecycleBin +
						" (Password Depot)", PwIcon.TrashBin);
					pgRoot.AddGroup(pg, true);
					ReadContainer(xn, pg, pwStorage, dItems);
				}
				else if(strName == "favorites")
					strFavs = XmlUtil.SafeInnerText(xn).Trim();
				else
				{
					Debug.Assert((strName == "header") || (strName == "sitestoignore") ||
						(strName == "categories"));
				}
			}

			if(!string.IsNullOrEmpty(strFavs))
			{
				try
				{
					byte[] pbList = Convert.FromBase64String(strFavs);
					string strList = StrUtil.Utf8.GetString(pbList);
					string[] v = strList.Split(new char[] { '\r', '\n' },
						StringSplitOptions.RemoveEmptyEntries);
					foreach(string str in v)
					{
						IStructureItem si;
						if(!dItems.TryGetValue(str, out si)) { Debug.Assert(false); continue; }

						PwGroup pg = (si as PwGroup);
						if(pg != null) pg.Tags.Add(PwDefs.FavoriteTag);

						PwEntry pe = (si as PwEntry);
						if(pe != null) pe.AddTag(PwDefs.FavoriteTag);
					}
				}
				catch(Exception) { Debug.Assert(false); }
			}
		}

		private static void ReadContainer(XmlNode xnContainer, PwGroup pgParent,
			PwDatabase pd, Dictionary<string, IStructureItem> dItems)
		{
			foreach(XmlNode xn in xnContainer.ChildNodes)
			{
				string strName = xn.Name.ToLowerInvariant();

				if(strName == ElemGroup)
					ReadGroup(xn, pgParent, pd, dItems);
				else { Debug.Assert(false); }
			}
		}

		private static void ReadGroup(XmlNode xnGroup, PwGroup pgParent, PwDatabase pd,
			Dictionary<string, IStructureItem> dItems)
		{
			PwGroup pg = new PwGroup(true, true);
			pgParent.AddGroup(pg, true);

			string str = GetAttribute(xnGroup, "name");
			if(string.IsNullOrEmpty(str)) { Debug.Assert(false); str = KPRes.Group; }
			pg.Name = str;

			str = GetAttribute(xnGroup, "fingerprint");
			if(string.IsNullOrEmpty(str)) { Debug.Assert(false); }
			else if(dItems == null) { Debug.Assert(false); }
			else if(dItems.ContainsKey(str)) { } // Recycle bin UUID = primary group UUID
			else dItems[str] = pg;

			str = GetAttribute(xnGroup, "imageindex");
			if(!string.IsNullOrEmpty(str)) pg.IconId = MapIcon(str, false);

			foreach(XmlNode xn in xnGroup.ChildNodes)
			{
				string strName = xn.Name.ToLowerInvariant();

				if(strName == ElemGroup)
					ReadGroup(xn, pg, pd, dItems);
				else if(strName == "item")
					ReadEntry(xn, pg, pd, dItems);
				else { Debug.Assert(false); }
			}
		}

		private static void ReadEntry(XmlNode xnEntry, PwGroup pg, PwDatabase pd,
			Dictionary<string, IStructureItem> dItems)
		{
			PwEntry pe = new PwEntry(true, true);
			pg.AddEntry(pe, true);

			foreach(XmlNode xn in xnEntry.ChildNodes)
			{
				string strName = xn.Name.ToLowerInvariant();

				if(strName == "description")
					ImportUtil.AppendToField(pe, PwDefs.TitleField,
						XmlUtil.SafeInnerText(xn), pd);
				else if(strName == "username")
					ImportUtil.AppendToField(pe, PwDefs.UserNameField,
						XmlUtil.SafeInnerText(xn), pd);
				else if(strName == "password")
					ImportUtil.AppendToField(pe, PwDefs.PasswordField,
						XmlUtil.SafeInnerText(xn), pd);
				else if(strName == "url")
					ImportUtil.CreateFieldWithIndex(pe.Strings, PwDefs.UrlField,
						XmlUtil.SafeInnerText(xn), pd, false);
				else if(strName == "comment")
					ImportUtil.AppendToField(pe, PwDefs.NotesField,
						XmlUtil.SafeInnerText(xn), pd);
				else if(strName == "expirydate")
				{
					DateTime? odt = ReadTime(xn);
					if(odt.HasValue)
					{
						pe.ExpiryTime = odt.Value;
						pe.Expires = true;
					}
				}
				else if(strName == "created")
				{
					DateTime? odt = ReadTime(xn);
					if(odt.HasValue) pe.CreationTime = odt.Value;
				}
				else if(strName == "lastmodified")
				{
					DateTime? odt = ReadTime(xn);
					if(odt.HasValue) pe.LastModificationTime = odt.Value;
				}
				else if(strName == "lastaccessed")
				{
					DateTime? odt = ReadTime(xn);
					if(odt.HasValue) pe.LastAccessTime = odt.Value;
				}
				else if(strName == "usagecount")
				{
					ulong u;
					if(ulong.TryParse(XmlUtil.SafeInnerText(xn), out u))
						pe.UsageCount = u;
				}
				else if(strName == "tags")
				{
					List<string> l = StrUtil.StringToTags(XmlUtil.SafeInnerText(xn));
					StrUtil.AddTags(pe.Tags, l);
				}
				else if(strName == "fingerprint")
				{
					string str = XmlUtil.SafeInnerText(xn);
					if(str.Length == 0) { Debug.Assert(false); }
					else if(dItems == null) { } // History, ...
					else if(dItems.ContainsKey(str)) { Debug.Assert(false); }
					else dItems[str] = pe;
				}
				else if(strName == "template")
					pe.AutoType.DefaultSequence = MapAutoType(XmlUtil.SafeInnerText(xn));
				else if(strName == "imageindex")
					pe.IconId = MapIcon(XmlUtil.SafeInnerText(xn), true);
				else if(strName == "urls")
					ReadUrls(xn, pe, pd);
				else if(strName == "customfields")
					ReadCustomFields(xn, pe, pd);
				else if(strName == "hitems")
					ReadHistory(xn, pe, pd);
			}
		}

		private static void ReadUrls(XmlNode xnUrls, PwEntry pe, PwDatabase pd)
		{
			foreach(XmlNode xn in xnUrls.ChildNodes)
			{
				string strName = xn.Name.ToLowerInvariant();

				if(strName == "url")
					ImportUtil.CreateFieldWithIndex(pe.Strings, PwDefs.UrlField,
						XmlUtil.SafeInnerText(xn), pd, false);
				else { Debug.Assert(false); }
			}
		}

		private static void ReadCustomFields(XmlNode xnFields, PwEntry pe, PwDatabase pd)
		{
			foreach(XmlNode xn in xnFields.ChildNodes)
			{
				string strName = xn.Name.ToLowerInvariant();

				if(strName == "field")
					ReadCustomField(xn, pe, pd);
				else { Debug.Assert(false); }
			}
		}

		private static void ReadCustomField(XmlNode xnField, PwEntry pe, PwDatabase pd)
		{
			string strKey = string.Empty, strValue = string.Empty;

			foreach(XmlNode xn in xnField.ChildNodes)
			{
				string strName = xn.Name.ToLowerInvariant();

				if(strName == "name")
					strKey = XmlUtil.SafeInnerText(xn);
				else if(strName == "value")
					strValue = XmlUtil.SafeInnerText(xn);
				else { Debug.Assert(strName == "visible"); }
			}

			if(strKey == "IDS_InformationText")
				strKey = PwDefs.NotesField;
			else if(strKey.Length == 0)
			{
				Debug.Assert(false);
				strKey = Guid.NewGuid().ToString();
			}

			ImportUtil.AppendToField(pe, strKey, strValue, pd);
		}

		private static void ReadHistory(XmlNode xnHistory, PwEntry pe, PwDatabase pd)
		{
			PwGroup pgH = new PwGroup(true, true);

			foreach(XmlNode xn in xnHistory.ChildNodes)
			{
				string strName = xn.Name.ToLowerInvariant();

				if(strName == "hitem")
					ReadEntry(xn, pgH, pd, null);
				else { Debug.Assert(false); }
			}

			foreach(PwEntry peH in pgH.Entries)
			{
				if(peH.History.UCount != 0) { Debug.Assert(false); peH.History.Clear(); }

				peH.ParentGroup = null;
				peH.SetUuid(pe.Uuid, false);

				pe.History.Add(peH);
			}

			pe.History.Sort(EntryUtil.CompareLastMod);
		}

		private static PwIcon MapIcon(string strIconId, bool bEntryIcon)
		{
			PwIcon ico = (bEntryIcon ? PwIcon.Key : PwIcon.Folder);

			int idIcon;
			if(!int.TryParse(strIconId, out idIcon)) { Debug.Assert(false); return ico; }

			++idIcon; // In the icon picker dialog, all indices are + 1
			switch(idIcon)
			{
				// case 1: ico = PwIcon.Key; break; // 1, 2, 3
				case 4: ico = PwIcon.FolderOpen; break;
				case 5:
				case 6: ico = PwIcon.LockOpen; break;
				case 15:
				case 16: ico = PwIcon.EMail; break;
				case 17:
				case 18: ico = PwIcon.ProgramIcons; break;
				case 21:
				case 22: ico = PwIcon.World; break;
				case 23:
				case 24: ico = PwIcon.UserCommunication; break;
				case 25:
				case 26: ico = PwIcon.Money; break;
				case 27:
				case 28: ico = PwIcon.Star; break;
				case 39: ico = PwIcon.Disk; break;
				case 40: ico = PwIcon.Clock; break;
				case 41: ico = PwIcon.Money; break;
				case 42: ico = PwIcon.Star; break;
				case 47: ico = PwIcon.Folder; break;
				case 48:
				case 49: ico = PwIcon.TrashBin; break;
				case 53: ico = PwIcon.Identity; break;
				case 54: ico = PwIcon.Info; break;
				case 62:
				case 64: ico = PwIcon.PaperLocked; break;
				case 66: ico = PwIcon.Book; break;
				case 67: ico = PwIcon.Expired; break;
				case 68: ico = PwIcon.Warning; break;
				case 69: ico = PwIcon.MultiKeys; break;
				case 73: ico = PwIcon.PaperNew; break;
				case 75: ico = PwIcon.Home; break;
				case 77: ico = PwIcon.Archive; break;
				case 81: ico = PwIcon.Money; break;
				case 83: ico = PwIcon.Clock; break;
				case 85: ico = PwIcon.Star; break;
				case 86: ico = PwIcon.LockOpen; break;
				case 87: ico = PwIcon.Certificate; break;
				case 91: ico = PwIcon.World; break;
				case 92:
				case 93:
				case 94:
				case 95: ico = PwIcon.Note; break;
				case 97: ico = PwIcon.Drive; break;
				case 100: ico = PwIcon.UserCommunication; break;
				case 101: ico = PwIcon.Monitor; break;
				case 111:
				case 112: ico = PwIcon.Identity; break;
				case 113: ico = PwIcon.EMail; break;
				default: break;
			};

			return ico;
		}

		private static string MapAutoType(string str)
		{
			str = str.Replace('<', '{');
			str = str.Replace('>', '}');

			str = StrUtil.ReplaceCaseInsensitive(str, @"{USER}",
				@"{" + PwDefs.UserNameField.ToUpperInvariant() + @"}");
			str = StrUtil.ReplaceCaseInsensitive(str, @"{PASS}",
				@"{" + PwDefs.PasswordField.ToUpperInvariant() + @"}");
			str = StrUtil.ReplaceCaseInsensitive(str, @"{CLEAR}", @"{CLEARFIELD}");
			str = StrUtil.ReplaceCaseInsensitive(str, @"{ARROW_LEFT}", @"{LEFT}");
			str = StrUtil.ReplaceCaseInsensitive(str, @"{ARROW_UP}", @"{UP}");
			str = StrUtil.ReplaceCaseInsensitive(str, @"{ARROW_RIGHT}", @"{RIGHT}");
			str = StrUtil.ReplaceCaseInsensitive(str, @"{ARROW_DOWN}", @"{DOWN}");
			str = StrUtil.ReplaceCaseInsensitive(str, @"{DELAY=", @"{DELAY ");

			if(str.Equals(PwDefs.DefaultAutoTypeSequence, StrUtil.CaseIgnoreCmp))
				return string.Empty;
			return str;
		}

		private static DateTime? ReadTime(XmlNode xn)
		{
			DateTime? odt = ReadTimeRaw(xn);
			if(!odt.HasValue) return null;

			if(odt.Value.Year < 1950) return null;

			return odt.Value;
		}

		private static DateTime? ReadTimeRaw(XmlNode xn)
		{
			string strFormat = GetAttribute(xn, "fmt");
			string strTime = XmlUtil.SafeInnerText(xn);

			if(strTime == "00.00.0000") return null; // Parsing fails
			// Another dummy value is 30.12.1899 (handled by ReadTime)

			DateTime dt;
			if(!string.IsNullOrEmpty(strFormat))
			{
				strFormat = strFormat.Replace("mm.", "MM.");
				strFormat = strFormat.Replace('h', 'H');

				if(DateTime.TryParseExact(strTime, strFormat, null,
					DateTimeStyles.AssumeLocal, out dt))
					return TimeUtil.ToUtc(dt, false);
			}
			Debug.Assert(false);

			if(DateTime.TryParse(strTime, out dt))
				return TimeUtil.ToUtc(dt, false);

			return null;
		}

		private static string GetAttribute(XmlNode xn, string strAttrib)
		{
			Debug.Assert(strAttrib == strAttrib.ToLowerInvariant());

			try
			{
				XmlAttributeCollection xac = xn.Attributes;

				XmlNode xnAttrib = xac.GetNamedItem(strAttrib);
				if(xnAttrib != null) return xnAttrib.Value;

				xnAttrib = xac.GetNamedItem(strAttrib.ToUpperInvariant());
				if(xnAttrib != null) return xnAttrib.Value;
			}
			catch(Exception) { Debug.Assert(false); }

			return null;
		}
	}
}
