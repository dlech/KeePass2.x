/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2008 Dominik Reichl <dominik.reichl@t-online.de>

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
	internal sealed class RoboFormHtml69 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "RoboForm HTML 6.9.82 (PassCards)"; } }
		public override string DefaultExtension { get { return "html"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override bool ImportAppendsToRootGroupOnly { get { return false; } }

		public override Image SmallIcon
		{
			get { return KeePass.Properties.Resources.B16x16_Imp_RoboForm; }
		}

		private const string m_strTitleTD = @"<TD class=caption";
		private const string m_strUrlTD = @"<TD class=subcaption";
		private const string m_strKeyTD = @"<TD class=field";
		private const string m_strValueTD = @"<TD class=wordbreakfield";

		public override void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			StreamReader sr = new StreamReader(sInput, Encoding.Unicode);
			string strData = sr.ReadToEnd();
			sr.Close();

			strData = strData.Replace(@"<WBR>", string.Empty);

			int nOffset = 0;
			while(true)
			{
				PwEntry pe = new PwEntry(true, true);

				int nTitleTD = strData.IndexOf(m_strTitleTD, nOffset);
				if(nTitleTD < 0) break;
				nOffset = nTitleTD + 1;

				string strTitle = StrUtil.XmlToString(StrUtil.GetStringBetween(
					strData, nTitleTD, @">", @"</TD>"));

				PwGroup pg = pwStorage.RootGroup;
				if(strTitle.IndexOf('\\') > 0)
				{
					int nLast = strTitle.LastIndexOf('\\');
					string strTree = strTitle.Substring(0, nLast);
					pg = pwStorage.RootGroup.FindCreateSubTree(strTree,
						new char[]{ '\\' });

					strTitle = strTitle.Substring(nLast + 1);
				}

				pe.Strings.Set(PwDefs.TitleField, new ProtectedString(
					pwStorage.MemoryProtection.ProtectTitle, strTitle));

				pg.AddEntry(pe, true);

				int nNextTitleTD = strData.IndexOf(m_strTitleTD, nOffset);
				if(nNextTitleTD < 0) nNextTitleTD = strData.Length + 1;

				int nUrlTD = strData.IndexOf(m_strUrlTD, nOffset);
				if((nUrlTD >= 0) && (nUrlTD < nNextTitleTD))
					pe.Strings.Set(PwDefs.UrlField, new ProtectedString(
						pwStorage.MemoryProtection.ProtectUrl,
						StrUtil.XmlToString(StrUtil.GetStringBetween(
							strData, nUrlTD, @">", @"</TD>"))));

				while(true)
				{
					int nKeyTD = strData.IndexOf(m_strKeyTD, nOffset);
					if((nKeyTD < 0) || (nKeyTD > nNextTitleTD)) break;

					int nValueTD = strData.IndexOf(m_strValueTD, nOffset);
					if((nValueTD < 0) || (nValueTD > nNextTitleTD)) break;

					string strKey = StrUtil.XmlToString(StrUtil.GetStringBetween(
						strData, nKeyTD, @">", @"</TD>"));
					string strValue = StrUtil.XmlToString(StrUtil.GetStringBetween(
						strData, nValueTD, @">", @"</TD>"));

					string strKeyMapped = ImportUtil.MapNameToStandardField(strKey);
					if((strKeyMapped == PwDefs.TitleField) ||
						(strKeyMapped == PwDefs.UrlField) ||
						(strKeyMapped.Length == 0) ||
						(pe.Strings.ReadSafe(strKeyMapped).Length > 0))
						strKeyMapped = strKey;

					pe.Strings.Set(strKeyMapped, new ProtectedString(
						pwStorage.MemoryProtection.GetProtection(strKeyMapped),
						strValue));

					nOffset = nValueTD + 1;
				}
			}
		}
	}
}
