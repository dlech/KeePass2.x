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
using System.Text.RegularExpressions;

using KeePass.Resources;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	// 2.50, 2.60 and 2.70
	internal sealed class PpKeeperHtml270 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Passphrase Keeper HTML"; } }
		public override string DefaultExtension { get { return "html|htm"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override bool ImportAppendsToRootGroupOnly { get { return true; } }

		private const string g_strTdStart = "<td class=\"c0\" nowrap>";
		private const string g_strTdEnd = "</td>";

		private const string g_strModifiedField = "{0530D298-F983-454C-B5A3-BFB0775844D1}";
		private const string g_strModifiedHdrStart = "Modified";

		public override void Import(PwDatabase pdStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			string strData = MemUtil.ReadString(sInput, Encoding.Default);

			// Normalize 2.70 files
			strData = strData.Replace("<td class=\"c1\" nowrap>", g_strTdStart);
			strData = strData.Replace("<td class=\"c2\" nowrap>", g_strTdStart);
			strData = strData.Replace("<td class=\"c3\" nowrap>", g_strTdStart);
			strData = strData.Replace("<td class=\"c4\" nowrap>", g_strTdStart);
			strData = strData.Replace("<td class=\"c5\" nowrap>", g_strTdStart);
			strData = strData.Replace("<td class=\"c6\" nowrap>", g_strTdStart);

			// Additionally support old versions
			string[] vRepl = new string[5] {
				// 2.60
				"<td nowrap align=\"center\" bgcolor=\"#[0-9a-fA-F]{6}\"><font color=\"#[0-9a-fA-F]{6}\" face=\"[^\"]*\">",

				// 2.50 and 2.60
				"<td nowrap align=\"(center|right)\" bgcolor=\"#[0-9a-fA-F]{6}\"><font color=\"#[0-9a-fA-F]{6}\"\\s*>",
				"<td nowrap bgcolor=\"#[0-9a-fA-F]{6}\"><font color=\"#[0-9a-fA-F]{6}\"\\s*>",
				"<td nowrap align=\"(center|right)\" bgcolor=\"#[0-9a-fA-F]{6}\"><b>",
				"<td nowrap bgcolor=\"#[0-9a-fA-F]{6}\"><b>"
			};
			foreach(string strRepl in vRepl)
				strData = Regex.Replace(strData, strRepl, g_strTdStart);
			strData = strData.Replace("</font></td>\r\n", g_strTdEnd + "\r\n");

			int nOffset = 0;

			PwEntry peHeader;
			if(!ReadEntry(out peHeader, strData, ref nOffset, pdStorage))
			{
				Debug.Assert(false);
				return;
			}

			while((nOffset >= 0) && (nOffset < strData.Length))
			{
				PwEntry pe;
				if(!ReadEntry(out pe, strData, ref nOffset, pdStorage))
				{
					Debug.Assert(false);
					break;
				}
				if(pe == null) break;

				pdStorage.RootGroup.AddEntry(pe, true);
			}
		}

		private static bool ReadEntry(out PwEntry pe, string strData,
			ref int nOffset, PwDatabase pd)
		{
			const string sS = g_strTdStart, sE = g_strTdEnd;

			pe = new PwEntry(true, true);

			if(!ReadString(strData, ref nOffset, sS, sE, pe, null, pd))
			{
				pe = null;
				return true;
			}
			if(!ReadString(strData, ref nOffset, sS, sE, pe, PwDefs.TitleField, pd))
				return false;
			if(!ReadString(strData, ref nOffset, sS, sE, pe, PwDefs.UserNameField, pd))
				return false;
			if(!ReadString(strData, ref nOffset, sS, sE, pe, PwDefs.PasswordField, pd))
				return false;
			if(!ReadString(strData, ref nOffset, sS, sE, pe, PwDefs.UrlField, pd))
				return false;
			if(!ReadString(strData, ref nOffset, sS, sE, pe, PwDefs.NotesField, pd))
				return false;
			if(!ReadString(strData, ref nOffset, sS, sE, pe, g_strModifiedField, pd))
				return false;

			return true;
		}

		private static bool ReadString(string strData, ref int nOffset,
			string strStart, string strEnd, PwEntry pe, string strFieldName,
			PwDatabase pd)
		{
			nOffset = strData.IndexOf(strStart, nOffset);
			if(nOffset < 0) return false;

			string strRawValue = StrUtil.GetStringBetween(strData, nOffset,
				strStart, strEnd);

			string strValue = strRawValue.Trim();
			if(strValue == "<br>") strValue = string.Empty;
			strValue = strValue.Replace("\r", string.Empty);
			strValue = strValue.Replace("\n", string.Empty);
			strValue = strValue.Replace("<br>", MessageService.NewLine);

			if(strFieldName == g_strModifiedField)
			{
				DateTime dt = ReadModified(strValue);
				pe.CreationTime = dt;
				pe.LastModificationTime = dt;
			}
			else if(strFieldName != null)
				ImportUtil.Add(pe, strFieldName, strValue, pd);

			nOffset += strStart.Length + strRawValue.Length + strEnd.Length;
			return true;
		}

		private static DateTime ReadModified(string strValue)
		{
			if(strValue == null) { Debug.Assert(false); return DateTime.UtcNow; }
			if(strValue.StartsWith(g_strModifiedHdrStart)) return DateTime.UtcNow;

			string[] vParts = strValue.Split(new char[] { ' ', ':', '/' },
				StringSplitOptions.RemoveEmptyEntries);
			if(vParts.Length != 6) { Debug.Assert(false); return DateTime.UtcNow; }

			try
			{
				return (new DateTime(int.Parse(vParts[2]), int.Parse(vParts[0]),
					int.Parse(vParts[1]), int.Parse(vParts[3]), int.Parse(vParts[4]),
					int.Parse(vParts[5]), DateTimeKind.Local)).ToUniversalTime();
			}
			catch(Exception) { Debug.Assert(false); }

			return DateTime.UtcNow;
		}
	}
}
