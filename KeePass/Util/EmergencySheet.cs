/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2019 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Windows.Forms;

using KeePass.App;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util.Archive;

using KeePassLib;
using KeePassLib.Delegates;
using KeePassLib.Keys;
using KeePassLib.Serialization;
using KeePassLib.Utility;

namespace KeePass.Util
{
	public static class EmergencySheet
	{
		public static void AskCreate(PwDatabase pd)
		{
			if((pd == null) || !pd.IsOpen) { Debug.Assert(false); return; }

			if(!Program.Config.UI.ShowEmSheetDialog) return;

			string str = KPRes.EmergencySheetInfo + MessageService.NewParagraph +
				KPRes.EmergencySheetRec + MessageService.NewParagraph +
				KPRes.EmergencySheetQ;

			VistaTaskDialog dlg = new VistaTaskDialog();
			dlg.CommandLinks = true;
			dlg.Content = str;
			dlg.DefaultButtonID = (int)DialogResult.OK;
			dlg.MainInstruction = KPRes.EmergencySheet;
			dlg.WindowTitle = PwDefs.ShortProductName;

			dlg.AddButton((int)DialogResult.OK, KPRes.Print,
				KPRes.EmergencySheetPrintInfo);
			dlg.AddButton((int)DialogResult.Cancel, KPRes.Skip, null);
			dlg.SetIcon(VtdCustomIcon.Question);

			bool b;
			if(dlg.ShowDialog()) b = (dlg.Result == (int)DialogResult.OK);
			else b = MessageService.AskYesNo(str);

			if(b) Print(pd);
		}

		public static void Print(PwDatabase pd)
		{
			if((pd == null) || !pd.IsOpen) { Debug.Assert(false); return; }

			try
			{
				string strName = UrlUtil.StripExtension(UrlUtil.GetFileName(
					pd.IOConnectionInfo.Path));
				if(strName.Length == 0) strName = KPRes.Database;

				string strHtml = GenerateHtml(pd, strName);
				PrintUtil.PrintHtml(strHtml);
			}
			catch(Exception ex)
			{
				MessageService.ShowWarning(ex);
			}
		}

		private static string GenerateHtml(PwDatabase pd, string strName)
		{
			bool bRtl = Program.Translation.Properties.RightToLeft;
			string strLogLeft = (bRtl ? "right" : "left");
			string strLogRight = (bRtl ? "left" : "right");

			GFunc<string, string> h = new GFunc<string, string>(StrUtil.StringToHtml);
			GFunc<string, string> ne = delegate(string str)
			{
				if(string.IsNullOrEmpty(str)) return "&nbsp;";
				return str;
			};
			GFunc<string, string> ltrPath = delegate(string str)
			{
				return (bRtl ? StrUtil.EnsureLtrPath(str) : str);
			};

			StringBuilder sb = new StringBuilder();
			sb.AppendLine("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\"");
			sb.AppendLine("\t\"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");

			sb.Append("<html xmlns=\"http://www.w3.org/1999/xhtml\"");
			string strLang = Program.Translation.Properties.Iso6391Code;
			if(string.IsNullOrEmpty(strLang)) strLang = "en";
			strLang = h(strLang);
			sb.Append(" lang=\"" + strLang + "\" xml:lang=\"" + strLang + "\"");
			if(bRtl) sb.Append(" dir=\"rtl\"");
			sb.AppendLine(">");

			sb.AppendLine("<head>");
			sb.AppendLine("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\" />");
			sb.Append("<title>");
			sb.Append(h(strName + " - " + KPRes.EmergencySheet));
			sb.AppendLine("</title>");
			sb.AppendLine("<meta http-equiv=\"expires\" content=\"0\" />");
			sb.AppendLine("<meta http-equiv=\"cache-control\" content=\"no-cache\" />");
			sb.AppendLine("<meta http-equiv=\"pragma\" content=\"no-cache\" />");

			sb.AppendLine("<style type=\"text/css\">");
			sb.AppendLine("/* <![CDATA[ */");

			string strFont = "\"Arial\", \"Tahoma\", \"Verdana\", sans-serif;";
			// https://sourceforge.net/p/keepass/discussion/329220/thread/f98dece5/
			if(Program.Translation.IsFor("fa"))
				strFont = "\"Tahoma\", \"Arial\", \"Verdana\", sans-serif;";

			sb.AppendLine("body, p, div, h1, h2, h3, h4, h5, h6, ol, ul, li, td, th, dd, dt, a {");
			sb.AppendLine("\tfont-family: " + strFont);
			sb.AppendLine("\tfont-size: 12pt;");
			sb.AppendLine("}");

			sb.AppendLine("body, p, div, table {");
			sb.AppendLine("\tcolor: #000000;");
			sb.AppendLine("\tbackground-color: #FFFFFF;");
			sb.AppendLine("}");

			sb.AppendLine("p, h3 {");
			sb.AppendLine("\tmargin: 0.5em 0em 0.5em 0em;");
			sb.AppendLine("}");

			sb.AppendLine("ol, ul {");
			sb.AppendLine("\tmargin-bottom: 0em;");
			sb.AppendLine("}");

			sb.AppendLine("h1, h2, h3 {");
			sb.AppendLine("\tfont-family: \"Verdana\", \"Arial\", \"Tahoma\", sans-serif;");
			sb.AppendLine("}");
			sb.AppendLine("h1, h2 {");
			sb.AppendLine("\ttext-align: center;");
			sb.AppendLine("\tmargin: 0pt 0pt 0pt 0pt;");
			sb.AppendLine("}");
			sb.AppendLine("h1 {");
			sb.AppendLine("\tfont-size: 2em;");
			sb.AppendLine("\tpadding: 3pt 0pt 0pt 0pt;");
			sb.AppendLine("}");
			sb.AppendLine("h2 {");
			sb.AppendLine("\tfont-size: 1.5em;");
			sb.AppendLine("\tpadding: 1pt 0pt 3pt 0pt;");
			sb.AppendLine("}");
			sb.AppendLine("h3 {");
			sb.AppendLine("\tfont-size: 1.2em;");
			// sb.AppendLine("\tpadding: 3pt 3pt 3pt 3pt;");
			// sb.AppendLine("\tcolor: #000000;");
			// sb.AppendLine("\tbackground-color: #EEEEEE;");
			// sb.AppendLine("\tbackground-image: -webkit-linear-gradient(top, #E2E2E2, #FAFAFA);");
			// sb.AppendLine("\tbackground-image: -moz-linear-gradient(top, #E2E2E2, #FAFAFA);");
			// sb.AppendLine("\tbackground-image: -ms-linear-gradient(top, #E2E2E2, #FAFAFA);");
			// sb.AppendLine("\tbackground-image: linear-gradient(to bottom, #E2E2E2, #FAFAFA);");
			sb.AppendLine("}");
			sb.AppendLine("h4 { font-size: 1em; }");
			sb.AppendLine("h5 { font-size: 0.89em; }");
			sb.AppendLine("h6 { font-size: 0.6em; }");

			sb.AppendLine("a:visited {");
			sb.AppendLine("\ttext-decoration: none;");
			sb.AppendLine("\tcolor: #0000DD;");
			sb.AppendLine("}");
			sb.AppendLine("a:active {");
			sb.AppendLine("\ttext-decoration: none;");
			sb.AppendLine("\tcolor: #6699FF;");
			sb.AppendLine("}");
			sb.AppendLine("a:link {");
			sb.AppendLine("\ttext-decoration: none;");
			sb.AppendLine("\tcolor: #0000DD;");
			sb.AppendLine("}");
			sb.AppendLine("a:hover {");
			sb.AppendLine("\ttext-decoration: underline;");
			sb.AppendLine("\tcolor: #6699FF;");
			sb.AppendLine("}");

			sb.AppendLine("img {");
			sb.AppendLine("\tborder: 0px none;");
			sb.AppendLine("}");

			sb.AppendLine(".withspc > li + li {");
			sb.AppendLine("\tmargin-top: 0.5em;");
			sb.AppendLine("}");

			sb.AppendLine("table.docheader {");
			sb.AppendLine("\twidth: 100%;");
			sb.AppendLine("\tbackground-color: #EEEEEE;");
			sb.AppendLine("\tmargin: 0px 0px 0px 0px;");
			sb.AppendLine("\tpadding: 0px 0px 0px 0px;");
			sb.AppendLine("\tborder: thin solid #808080;");
			// border-collapse is incompatible with border-radius
			// sb.AppendLine("\tborder-collapse: collapse;");
			sb.AppendLine("\t-webkit-border-radius: 5px;");
			sb.AppendLine("\t-moz-border-radius: 5px;");
			sb.AppendLine("\tborder-radius: 5px;");
			sb.AppendLine("}");

			sb.AppendLine("table.docheader tr td {");
			sb.AppendLine("\tvertical-align: middle;");
			sb.AppendLine("\tpadding: 0px 15px 0px 15px;");
			sb.AppendLine("}");

			sb.AppendLine("table.fillinline {");
			sb.AppendLine("\twidth: 100%;");
			sb.AppendLine("\tmargin: 0px 0px 0px 0px;");
			sb.AppendLine("\tpadding: 0px 0px 0px 0px;");
			sb.AppendLine("\tborder: thin solid #808080;");
			sb.AppendLine("\tborder-collapse: collapse;");
			sb.AppendLine("\tempty-cells: show;");
			sb.AppendLine("}");
			sb.AppendLine("table.fillinline tr td {");
			sb.AppendLine("\tpadding: 4pt 4pt 4pt 4pt;");
			sb.AppendLine("\tvertical-align: middle;");
			sb.AppendLine("\tword-break: break-all;");
			sb.AppendLine("\toverflow-wrap: break-word;");
			sb.AppendLine("\tword-wrap: break-word;");
			sb.AppendLine("}");
			// sb.AppendLine("span.fillinlinesym {");
			// sb.AppendLine("\tdisplay: inline-block;");
			// sb.AppendLine("\ttransform: scale(1.75, 1.75) translate(-0.5pt, -0.5pt);");
			// sb.AppendLine("}");

			// sb.AppendLine("@media print {");
			// sb.AppendLine(".scronly {");
			// sb.AppendLine("\tdisplay: none;");
			// sb.AppendLine("}");
			// sb.AppendLine("}");

			// Add the temporary content identifier
			sb.AppendLine("." + Program.TempFilesPool.TempContentTag + " {");
			sb.AppendLine("\tfont-size: 12pt;");
			sb.AppendLine("}");

			sb.AppendLine("/* ]]> */");
			sb.AppendLine("</style>");
			sb.AppendLine("</head><body>");

			ImageArchive ia = new ImageArchive();
			ia.Load(Properties.Resources.Images_App_HighRes);

			sb.AppendLine("<table class=\"docheader\" cellspacing=\"0\" cellpadding=\"0\"><tr>");
			sb.AppendLine("<td style=\"text-align: " + strLogLeft + ";\">");
			sb.AppendLine("<img src=\"" + GfxUtil.ImageToDataUri(ia.GetForObject(
				"KeePass")) + "\" width=\"48\" height=\"48\" alt=\"" +
				h(PwDefs.ShortProductName) + "\" /></td>");
			sb.AppendLine("<td style=\"text-align: center;\">");
			sb.AppendLine("<h1>" + h(PwDefs.ShortProductName) + "</h1>");
			sb.AppendLine("<h2>" + h(KPRes.EmergencySheet) + "</h2>");
			sb.AppendLine("</td>");
			sb.AppendLine("<td style=\"text-align: " + strLogRight + ";\">");
			sb.AppendLine("<img src=\"" + GfxUtil.ImageToDataUri(ia.GetForObject(
				"KOrganizer")) + "\" width=\"48\" height=\"48\" alt=\"" +
				h(KPRes.EmergencySheet) + "\" /></td>");
			sb.AppendLine("</tr></table>");

			sb.AppendLine("<p style=\"text-align: " + strLogRight + ";\">" +
				h(TimeUtil.ToDisplayStringDateOnly(DateTime.Now)) + "</p>");

			const string strFillInit = "<table class=\"fillinline\"><tr><td>";
			const string strFillInitLtr = "<table class=\"fillinline\"><tr><td dir=\"ltr\">";
			const string strFillEnd = "</td></tr></table>";
			string strFillSym = "<img src=\"" + GfxUtil.ImageToDataUri(
				Properties.Resources.B48x35_WritingHand) +
				"\" style=\"width: 1.3714em; height: 1em;" +
				(bRtl ? " transform: scaleX(-1);" : string.Empty) +
				"\" alt=\"&#x270D;\" />";
			string strFill = strFillInit + ne(string.Empty) +
				// "</td><td style=\"text-align: right;\"><span class=\"fillinlinesym\">&#x270D;</span>" +
				"</td><td style=\"text-align: " + strLogRight + ";\">" +
				strFillSym + strFillEnd;

			string strFillInitEx = (bRtl ? strFillInitLtr : strFillInit);

			IOConnectionInfo ioc = pd.IOConnectionInfo;
			sb.AppendLine("<p><strong>" + h(KPRes.DatabaseFile) + ":</strong></p>");
			sb.AppendLine(strFillInitEx + ne(h(ltrPath(ioc.Path))) + strFillEnd);

			// if(pd.Name.Length > 0)
			//	sb.AppendLine("<p><strong>" + h(KPRes.Name) + ":</strong> " +
			//		h(pd.Name) + "</p>");
			// if(pd.Description.Length > 0)
			//	sb.AppendLine("<p><strong>" + h(KPRes.Description) + ":</strong> " +
			//		h(pd.Description));

			sb.AppendLine("<p>" + h(KPRes.BackupDatabase) + " " +
				h(KPRes.BackupLocation) + "</p>");
			sb.AppendLine(strFill);

			CompositeKey ck = pd.MasterKey;
			if(ck.UserKeyCount > 0)
			{
				sb.AppendLine("<br />");
				sb.AppendLine("<h3>" + h(KPRes.MasterKey) + "</h3>");
				sb.AppendLine("<p>" + h(KPRes.MasterKeyComponents) + "</p>");
				sb.AppendLine("<ul>");

				foreach(IUserKey k in ck.UserKeys)
				{
					KcpPassword p = (k as KcpPassword);
					KcpKeyFile kf = (k as KcpKeyFile);
					KcpUserAccount a = (k as KcpUserAccount);
					KcpCustomKey c = (k as KcpCustomKey);

					if(p != null)
					{
						sb.AppendLine("<li><p><strong>" + h(KPRes.MasterPassword) +
							":</strong></p>");
						sb.AppendLine(strFill + "</li>");
					}
					else if(kf != null)
					{
						sb.AppendLine("<li><p><strong>" + h(KPRes.KeyFile) +
							":</strong></p>");
						sb.AppendLine(strFillInitEx + ne(h(ltrPath(kf.Path))) + strFillEnd);

						sb.AppendLine("<p>" + h(KPRes.BackupFile) + " " +
							h(KPRes.BackupLocation) + "</p>");
						sb.AppendLine(strFill);

						sb.AppendLine("</li>");
					}
					else if(a != null)
					{
						sb.AppendLine("<li><p><strong>" + h(KPRes.WindowsUserAccount) +
							":</strong></p>");
						sb.Append(strFillInitEx);
						try
						{
							sb.Append(ne(h(ltrPath(Environment.UserDomainName +
								"\\" + Environment.UserName))));
						}
						catch(Exception) { Debug.Assert(false); sb.Append(ne(string.Empty)); }
						sb.AppendLine(strFillEnd);

						sb.AppendLine("<p>" + h(KPRes.WindowsUserAccountBackup) + " " +
							h(KPRes.BackupLocation) + "</p>");
						sb.AppendLine(strFill + "</li>");
					}
					else if(c != null)
					{
						sb.AppendLine("<li><p><strong>" + h(KPRes.KeyProvider) +
							":</strong></p>");
						sb.AppendLine(strFillInitEx + ne(h(ltrPath(c.Name))) + strFillEnd);

						sb.AppendLine("</li>");
					}
					else
					{
						Debug.Assert(false);
						sb.AppendLine("<li><p><strong>" + h(KPRes.Unknown) + ".</strong></p></li>");
					}
				}

				sb.AppendLine("</ul>");
			}

			sb.AppendLine("<br />");
			sb.AppendLine("<h3>" + h(KPRes.InstrAndGenInfo) + "</h3>");

			sb.AppendLine("<ul class=\"withspc\">");

			sb.AppendLine("<li>" + h(KPRes.EmergencySheetInfo) + "</li>");
			// sb.AppendLine("<form class=\"scronly\" action=\"#\" onsubmit=\"javascript:window.print();\">");
			// sb.AppendLine("<input type=\"submit\" value=\"&#x1F5B6; " +
			//	h(KPRes.Print) + "\" />");
			// sb.AppendLine("</form></li>");

			sb.AppendLine("<li>" + h(KPRes.DataLoss) + "</li>");
			sb.AppendLine("<li>" + h(KPRes.LatestVersionWeb) + ": <a href=\"" +
				h(PwDefs.HomepageUrl) + "\" target=\"_blank\">" +
				h(PwDefs.HomepageUrl) + "</a>.</li>");
			sb.AppendLine("</ul>");

			sb.AppendLine("</body></html>");
			return sb.ToString();
		}
	}
}
