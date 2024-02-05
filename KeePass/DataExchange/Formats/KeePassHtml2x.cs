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
using System.IO;
using System.Text;
using System.Windows.Forms;

using KeePass.Forms;
using KeePass.Resources;
using KeePass.UI;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	internal sealed class KeePassHtml2x : FileFormatProvider
	{
		public override bool SupportsImport { get { return false; } }
		public override bool SupportsExport { get { return true; } }

		public override string FormatName { get { return KPRes.CustomizableHtml; } }
		public override string DefaultExtension { get { return "html|htm"; } }
		public override string ApplicationGroup { get { return KPRes.General; } }

		public override bool Export(PwExportInfo pwExportInfo, Stream sOutput,
			IStatusLogger slLogger)
		{
			ImageList il = null;
			MainForm mf = Program.MainForm;
			if((mf != null) && (mf.ActiveDatabase == pwExportInfo.ContextDatabase))
				il = mf.ClientIcons;

			PrintForm dlg = new PrintForm();
			dlg.InitEx(pwExportInfo.DataGroup, pwExportInfo.ContextDatabase, il,
				false, -1);

			bool bResult = false;
			try
			{
				if(dlg.ShowDialog() == DialogResult.OK)
				{
					byte[] pb = StrUtil.Utf8.GetBytes(dlg.GeneratedHtml);
					sOutput.Write(pb, 0, pb.Length);
					sOutput.Close();

					bResult = true;
				}
			}
			finally { UIUtil.DestroyForm(dlg); }

			return bResult;
		}

		internal static StringBuilder HtmlPart1ToHead(bool bRtl, string strTitle)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("<!DOCTYPE html>");

			sb.Append("<html xmlns=\"http://www.w3.org/1999/xhtml\"");
			string strLang = Program.Translation.Properties.Iso6391Code;
			if(string.IsNullOrEmpty(strLang)) strLang = "en";
			strLang = StrUtil.StringToHtml(strLang);
			sb.Append(" xml:lang=\"" + strLang + "\" lang=\"" + strLang + "\"");
			if(bRtl) sb.Append(" dir=\"rtl\"");
			sb.AppendLine(">");

			sb.AppendLine("<head>");
			sb.AppendLine("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />");
			sb.AppendLine("<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\" />");
			sb.AppendLine("<meta http-equiv=\"expires\" content=\"0\" />");
			sb.AppendLine("<meta http-equiv=\"cache-control\" content=\"no-cache\" />");
			sb.AppendLine("<meta http-equiv=\"pragma\" content=\"no-cache\" />");

			sb.Append("<title>");
			sb.Append(StrUtil.StringToHtml(strTitle));
			sb.AppendLine("</title>");

			return sb;
		}

		internal static void HtmlPart2ToStyle(StringBuilder sb)
		{
			sb.AppendLine("<style type=\"text/css\">");
			sb.AppendLine("/* <![CDATA[ */");
		}

		internal static void HtmlPart3ToBody(StringBuilder sb)
		{
			sb.AppendLine("/* ]]> */");
			sb.AppendLine("</style>");
			sb.AppendLine("</head>");
			sb.AppendLine("<body>");
		}

		internal static void HtmlPart4ToEnd(StringBuilder sb)
		{
			sb.AppendLine("</body>");
			sb.AppendLine("</html>");
		}
	}
}
