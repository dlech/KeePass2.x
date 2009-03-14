/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2009 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Drawing;
using System.Diagnostics;
using System.IO;

using KeePass.Resources;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	internal sealed class PpKeeperHtml270 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Passphrase Keeper HTML 2.70"; } }
		public override string DefaultExtension { get { return "html"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override bool ImportAppendsToRootGroupOnly { get { return true; } }

		public override Image SmallIcon
		{
			get { return KeePass.Properties.Resources.B16x16_Imp_Whisper32; }
		}

		private const string m_strStartTd = "<td class=\"c0\" nowrap>";
		private const string m_strEndTd = @"</td>";

		private const string m_strModifiedField = @"{0530D298-F983-454c-B5A3-BFB0775844D1}";

		private const string m_strModifiedHeader = "Modified";

		public override void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			StreamReader sr = new StreamReader(sInput, Encoding.Default);
			string strData = sr.ReadToEnd();
			sr.Close();

			const string strNormalized = "<td class=\"c0\" nowrap>";
			strData = strData.Replace("<td class=\"c1\" nowrap>", strNormalized);
			strData = strData.Replace("<td class=\"c2\" nowrap>", strNormalized);
			strData = strData.Replace("<td class=\"c3\" nowrap>", strNormalized);
			strData = strData.Replace("<td class=\"c4\" nowrap>", strNormalized);
			strData = strData.Replace("<td class=\"c5\" nowrap>", strNormalized);
			strData = strData.Replace("<td class=\"c6\" nowrap>", strNormalized);

			int nOffset = 0;

			PwEntry peHeader;
			if(ReadEntry(out peHeader, strData, ref nOffset, pwStorage) == false)
			{
				Debug.Assert(false);
				return;
			}

			while((nOffset >= 0) && (nOffset < strData.Length))
			{
				PwEntry pe;
				if(ReadEntry(out pe, strData, ref nOffset, pwStorage) == false)
				{
					Debug.Assert(false);
					break;
				}
				if(pe == null) break;

				pwStorage.RootGroup.AddEntry(pe, true);
			}
		}

		private static bool ReadEntry(out PwEntry pe, string strData, ref int nOffset,
			PwDatabase pd)
		{
			pe = new PwEntry(true, true);

			if(!ReadString(strData, ref nOffset, m_strStartTd, m_strEndTd, pe, null, false))
			{
				pe = null;
				return true;
			}
			if(!ReadString(strData, ref nOffset, m_strStartTd, m_strEndTd, pe,
				PwDefs.TitleField, pd.MemoryProtection.ProtectTitle))
				return false;
			if(!ReadString(strData, ref nOffset, m_strStartTd, m_strEndTd, pe,
				PwDefs.UserNameField, pd.MemoryProtection.ProtectUserName))
				return false;
			if(!ReadString(strData, ref nOffset, m_strStartTd, m_strEndTd, pe,
				PwDefs.PasswordField, pd.MemoryProtection.ProtectPassword))
				return false;
			if(!ReadString(strData, ref nOffset, m_strStartTd, m_strEndTd, pe,
				PwDefs.UrlField, pd.MemoryProtection.ProtectUrl))
				return false;
			if(!ReadString(strData, ref nOffset, m_strStartTd, m_strEndTd, pe,
				PwDefs.NotesField, pd.MemoryProtection.ProtectNotes))
				return false;
			if(!ReadString(strData, ref nOffset, m_strStartTd, m_strEndTd, pe,
				m_strModifiedField, false))
				return false;

			return true;
		}

		private static bool ReadString(string strData, ref int nOffset,
			string strStart, string strEnd, PwEntry pe, string strFieldName,
			bool bProtect)
		{
			nOffset = strData.IndexOf(strStart, nOffset);
			if(nOffset < 0) return false;

			string strRawValue = StrUtil.GetStringBetween(strData, nOffset,
				strStart, strEnd);

			string strValue = strRawValue.Trim();
			if(strValue == @"<br>") strValue = string.Empty;
			strValue = strValue.Replace("\r", string.Empty);
			strValue = strValue.Replace("\n", string.Empty);
			strValue = strValue.Replace(@"<br>", MessageService.NewLine);

			if((strFieldName != null) && (strFieldName == m_strModifiedField))
			{
				DateTime dt = ReadModified(strValue);
				pe.CreationTime = dt;
				pe.LastModificationTime = dt;
			}
			else if(strFieldName != null)
				pe.Strings.Set(strFieldName, new ProtectedString(bProtect, strValue));

			nOffset += strStart.Length + strRawValue.Length + strEnd.Length;
			return true;
		}

		private static DateTime ReadModified(string strValue)
		{
			if(strValue == null) { Debug.Assert(false); return DateTime.Now; }
			if(strValue == m_strModifiedHeader) return DateTime.Now;

			string[] vParts = strValue.Split(new char[]{ ' ', ':', '/' },
				StringSplitOptions.RemoveEmptyEntries);
			if(vParts.Length != 6) { Debug.Assert(false); return DateTime.Now; }

			try
			{
				return new DateTime(int.Parse(vParts[2]), int.Parse(vParts[0]),
					int.Parse(vParts[1]), int.Parse(vParts[3]), int.Parse(vParts[4]),
					int.Parse(vParts[5]));
			}
			catch(Exception) { Debug.Assert(false); }

			return DateTime.Now;
		}
	}
}
