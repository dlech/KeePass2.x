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
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	// 6.05-6.62+
	internal sealed class CodeWalletTxt605 : FileFormatProvider
	{
		private const string CwtSeparator = "*---------------------------------------------------";

		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "CodeWallet TXT"; } }
		public override string DefaultExtension { get { return "txt"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override bool ImportAppendsToRootGroupOnly { get { return true; } }

		public override void Import(PwDatabase pdStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			string strData = MemUtil.ReadString(sInput, Encoding.Unicode);

			strData = StrUtil.NormalizeNewLines(strData, false);

			Debug.Assert(CwtSeparator.IndexOf('$') < 0);
			strData = Regex.Replace(strData, "\\n+" + Regex.Escape(CwtSeparator),
				"\n" + CwtSeparator, RegexOptions.Singleline);

			string[] vLines = strData.Split('\n');

			PwEntry pe = null;
			bool bInTitle = false;
			string strFieldName = null;

			foreach(string strLine in vLines)
			{
				if(strLine == CwtSeparator)
				{
					if(!bInTitle)
					{
						pe = new PwEntry(true, true);
						pdStorage.RootGroup.AddEntry(pe, true);

						strFieldName = null;
					}

					bInTitle = !bInTitle;
				}
				else if(bInTitle)
					ImportUtil.Add(pe, PwDefs.TitleField, strLine, pdStorage);
				else if(pe != null)
				{
					string strFieldValue = strLine;

					int cchName = strLine.IndexOf(": ");
					if(cchName > 0)
					{
						strFieldName = ImportUtil.MapName(strLine.Substring(0,
							cchName), false);
						strFieldValue = strLine.Remove(0, cchName + 2);
					}

					if(!string.IsNullOrEmpty(strFieldName))
					{
						if((strFieldValue.Length == 0) && PwDefs.IsMultiLineField(
							strFieldName))
							ImportUtil.AppendToField(pe, strFieldName,
								MessageService.NewLine, pdStorage, string.Empty, false);
						else
							ImportUtil.AppendToField(pe, strFieldName,
								strFieldValue, pdStorage);
					}
				}
			}
		}
	}
}
