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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

using KeePass.App;
using KeePass.App.Configuration;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util.Spr;

using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Delegates;
using KeePassLib.Interfaces;
using KeePassLib.Native;
using KeePassLib.Resources;
using KeePassLib.Security;
using KeePassLib.Translation;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class PrintForm : Form
	{
		private PwGroup m_pgDataSource = null;
		private PwDatabase m_pdContext = null;
		private ImageList m_ilClientIcons = null;
		private bool m_bPrintMode = true;
		private int m_nDefaultSortColumn = -1;

		private int m_iBlockPreviewRefresh = 0;
		private Control m_cPreBlock = null;

		private ImageList m_ilTabIcons = null;

		private string m_strGeneratedHtml = string.Empty;
		public string GeneratedHtml
		{
			get { return m_strGeneratedHtml; }
		}

		private static string g_strCodeItS = null;
		private static string g_strCodeItE = null;

		private sealed class PfOptions
		{
			public bool MonoPasswords = true;
			public bool SmallMono = false;
			public int SprMode = 0;

			public string CellInit = string.Empty;
			public string CellExit = string.Empty;

			public string FontInit = string.Empty;
			public string FontExit = string.Empty;

			public bool Rtl = false;

			public PwEntry Entry = null;
			public PwDatabase Database = null;
			public ImageList ClientIcons = null;
			public SprContext SprContext = null;

			public DateTime Now = DateTime.UtcNow;

			public PfOptions() { }

			public PfOptions CloneShallow()
			{
				return (PfOptions)this.MemberwiseClone();
			}
		}

		public void InitEx(PwGroup pgDataSource, PwDatabase pdContext,
			ImageList ilClientIcons, bool bPrintMode, int nDefaultSortColumn)
		{
			Debug.Assert(pgDataSource != null); if(pgDataSource == null) throw new ArgumentNullException("pgDataSource");

			m_pgDataSource = pgDataSource;
			m_pdContext = pdContext;
			m_ilClientIcons = ilClientIcons;
			m_bPrintMode = bPrintMode;
			m_nDefaultSortColumn = nDefaultSortColumn;
		}

		public PrintForm()
		{
			InitializeComponent();
			Program.Translation.ApplyTo(this);
		}

		private void CreateDialogBanner()
		{
			string strTitle, strDesc;
			if(m_bPrintMode)
			{
				strTitle = KPRes.Print;
				strDesc = KPRes.PrintDesc;
			}
			else // HTML export mode
			{
				strTitle = KPRes.ExportHtml;
				strDesc = KPRes.ExportHtmlDesc;
			}

			BannerFactory.CreateBannerEx(this, m_bannerImage,
				Properties.Resources.B48x48_FilePrint, strTitle, strDesc);
			this.Text = strTitle;
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			if(m_pgDataSource == null) { Debug.Assert(false); throw new InvalidOperationException(); }

			++m_iBlockPreviewRefresh;

			GlobalWindowManager.AddWindow(this);

			this.Icon = AppIcons.Default;
			CreateDialogBanner();

			List<Image> lTabImg = new List<Image>();
			lTabImg.Add(Properties.Resources.B16x16_XMag);
			lTabImg.Add(Properties.Resources.B16x16_Configure);

			m_ilTabIcons = UIUtil.BuildImageListUnscaled(lTabImg,
				DpiUtil.ScaleIntX(16), DpiUtil.ScaleIntY(16));
			m_tabMain.ImageList = m_ilTabIcons;

			m_tabPreview.ImageIndex = 0;
			m_tabDataLayout.ImageIndex = 1;

			UIUtil.SetButtonImage(m_btnConfigPrinter,
				Properties.Resources.B16x16_EditCopy, true);
			UIUtil.SetButtonImage(m_btnPrintPreview,
				Properties.Resources.B16x16_FileQuickPrint, true);

			FontUtil.AssignDefaultBold(m_rbTabular);
			FontUtil.AssignDefaultBold(m_rbDetails);

			Debug.Assert(!m_cmbSpr.Sorted);
			m_cmbSpr.Items.Add(KPRes.ReplaceNo);
			m_cmbSpr.Items.Add(KPRes.Replace + " (" + KPRes.Slow + ")");
			m_cmbSpr.Items.Add(KPRes.BothForms + " (" + KPRes.Slow + ")");
			m_cmbSpr.SelectedIndex = 0;

			if(!m_bPrintMode) m_btnOK.Text = KPRes.Export;

			m_rbTabular.Checked = true;

			m_cmbSortEntries.Items.Add("(" + KPRes.None + ")");
			m_cmbSortEntries.Items.Add(KPRes.Title);
			m_cmbSortEntries.Items.Add(KPRes.UserName);
			m_cmbSortEntries.Items.Add(KPRes.Password);
			m_cmbSortEntries.Items.Add(KPRes.Url);
			m_cmbSortEntries.Items.Add(KPRes.Notes);

			AceColumnType colType = AceColumnType.Count;
			List<AceColumn> vCols = Program.Config.MainWindow.EntryListColumns;
			if((m_nDefaultSortColumn >= 0) && (m_nDefaultSortColumn < vCols.Count))
				colType = vCols[m_nDefaultSortColumn].Type;

			int nSortSel = 0;
			if(colType == AceColumnType.Title) nSortSel = 1;
			else if(colType == AceColumnType.UserName) nSortSel = 2;
			else if(colType == AceColumnType.Password) nSortSel = 3;
			else if(colType == AceColumnType.Url) nSortSel = 4;
			else if(colType == AceColumnType.Notes) nSortSel = 5;
			m_cmbSortEntries.SelectedIndex = nSortSel;

			if(!m_bPrintMode) // Export to HTML
			{
				m_btnConfigPrinter.Visible = m_btnPrintPreview.Visible = false;
				m_lblPreviewHint.Visible = false;
			}

			Program.TempFilesPool.AddWebBrowserPrintContent();

			--m_iBlockPreviewRefresh;
			UpdateWebBrowser(true);
			UpdateUIState();
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			if(m_bPrintMode)
			{
				UpdateWebBrowser(false);

				try { m_wbMain.ShowPrintDialog(); } // Throws in Mono 1.2.6+
				catch(NotImplementedException)
				{
					MessageService.ShowWarning(KLRes.FrameworkNotImplExcp);
				}
				catch(Exception ex) { MessageService.ShowWarning(ex); }
			}
			else m_strGeneratedHtml = (GenerateHtmlDocument() ?? string.Empty);
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
		}

		private void UpdateUIState()
		{
			bool bTabular = m_rbTabular.Checked;
			bool bDetails = m_rbDetails.Checked;

			UIUtil.SetEnabled(m_cbAutoType, bDetails);
			UIUtil.SetEnabled(m_cbCustomStrings, bDetails);
			if(bTabular)
			{
				UIUtil.SetChecked(m_cbAutoType, false);
				UIUtil.SetChecked(m_cbCustomStrings, false);
			}

			bool bIcon = m_cbIcon.Checked;
			if(m_ilClientIcons == null)
			{
				UIUtil.SetChecked(m_cbIcon, false);
				UIUtil.SetEnabled(m_cbIcon, false);
				bIcon = false;
			}

			UIUtil.SetEnabled(m_cbTitle, !bIcon);
			if(bIcon) UIUtil.SetChecked(m_cbTitle, true);
		}

		private void UIBlockInteraction(bool bBlock)
		{
			if(bBlock) m_cPreBlock = UIUtil.GetActiveControl(this);

			this.UseWaitCursor = bBlock;

			// this.Enabled = !bBlock; // Prevents wait cursor
			m_tabMain.Enabled = !bBlock;
			m_btnConfigPrinter.Enabled = !bBlock;
			m_btnPrintPreview.Enabled = !bBlock;
			m_btnOK.Enabled = !bBlock;
			m_btnCancel.Enabled = !bBlock;

			try { m_wbMain.Visible = !bBlock; }
			catch(Exception) { Debug.Assert(NativeLib.IsUnix()); }

			if(!bBlock && (m_cPreBlock != null)) UIUtil.SetFocus(m_cPreBlock, this);
		}

		/* private void ShowWaitDocument()
		{
			StringBuilder sbW = new StringBuilder();
			sbW.AppendLine("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\"");
			sbW.AppendLine("\t\"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
			sbW.AppendLine("<html xmlns=\"http://www.w3.org/1999/xhtml\">");
			sbW.AppendLine("<head>");
			sbW.AppendLine("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\" />");
			sbW.AppendLine("<title>...</title>");
			sbW.AppendLine("</head><body><br /><br />");
			sbW.AppendLine("<h1 style=\"text-align: center;\">&#8987;</h1>");
			sbW.AppendLine("</body></html>");

			try { UIUtil.SetWebBrowserDocument(m_wbMain, sbW.ToString()); }
			catch(Exception) { Debug.Assert(NativeLib.IsUnix()); } // Throws in Mono 2.0+
		} */

		private void UpdateWebBrowser(bool bInitial)
		{
			if(m_iBlockPreviewRefresh > 0) return;

			++m_iBlockPreviewRefresh;
			if(!bInitial) UIBlockInteraction(true);
			// ShowWaitDocument();

			string strHtml = GenerateHtmlDocument();

			try { UIUtil.SetWebBrowserDocument(m_wbMain, strHtml); }
			catch(Exception) { Debug.Assert(NativeLib.IsUnix()); } // Throws in Mono 2.0+
			try { m_wbMain.AllowNavigation = false; }
			catch(Exception) { Debug.Assert(false); }

			if(!bInitial) UIBlockInteraction(false);
			--m_iBlockPreviewRefresh;
		}

		private string GenerateHtmlDocument()
		{
			PwGroup pgDataSource = m_pgDataSource.CloneDeep(); // Sorting, ...

			int nSortEntries = m_cmbSortEntries.SelectedIndex;
			string strSortFieldName = null;
			if(nSortEntries == 0) { } // No sort
			else if(nSortEntries == 1) strSortFieldName = PwDefs.TitleField;
			else if(nSortEntries == 2) strSortFieldName = PwDefs.UserNameField;
			else if(nSortEntries == 3) strSortFieldName = PwDefs.PasswordField;
			else if(nSortEntries == 4) strSortFieldName = PwDefs.UrlField;
			else if(nSortEntries == 5) strSortFieldName = PwDefs.NotesField;
			else { Debug.Assert(false); }
			if(strSortFieldName != null)
				SortGroupEntriesRecursive(pgDataSource, strSortFieldName);

			bool bGroup = m_cbGroups.Checked;
			bool bTitle = m_cbTitle.Checked, bUserName = m_cbUser.Checked;
			bool bPassword = m_cbPassword.Checked, bUrl = m_cbUrl.Checked;
			bool bNotes = m_cbNotes.Checked;
			bool bCreation = m_cbCreation.Checked, bLastMod = m_cbLastMod.Checked;
			// bool bLastAcc = m_cbLastAccess.Checked;
			bool bExpire = m_cbExpire.Checked;
			bool bAutoType = m_cbAutoType.Checked;
			bool bTags = m_cbTags.Checked;
			bool bCustomStrings = m_cbCustomStrings.Checked;
			bool bUuid = m_cbUuid.Checked;

			PfOptions p = new PfOptions();
			p.MonoPasswords = m_cbMonospaceForPasswords.Checked;
			if(m_rbMonospace.Checked) p.MonoPasswords = false; // Monospace anyway
			p.SmallMono = m_cbSmallMono.Checked;
			p.SprMode = m_cmbSpr.SelectedIndex;
			p.Rtl = (this.RightToLeft == RightToLeft.Yes);
			p.Database = m_pdContext;
			if(m_cbIcon.Checked) p.ClientIcons = m_ilClientIcons;

			if(m_rbSerif.Checked)
			{
				p.FontInit = "<span class=\"fserif\">";
				p.FontExit = "</span>";
			}
			else if(m_rbSansSerif.Checked)
			{
				p.FontInit = string.Empty;
				p.FontExit = string.Empty;
			}
			else if(m_rbMonospace.Checked)
			{
				p.FontInit = (p.SmallMono ? "<code><small>" : "<code>");
				p.FontExit = (p.SmallMono ? "</small></code>" : "</code>");
			}
			else { Debug.Assert(false); }

			GFunc<string, string> h = new GFunc<string, string>(StrUtil.StringToHtml);
			GFunc<string, string> c = delegate(string strRaw)
			{
				return CompileText(strRaw, p, true, false);
			};
			GFunc<string, string> cs = delegate(string strRaw)
			{
				return CompileText(strRaw, p, true, true);
			};

			StringBuilder sb = new StringBuilder();

			sb.AppendLine("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\"");
			sb.AppendLine("\t\"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");

			sb.Append("<html xmlns=\"http://www.w3.org/1999/xhtml\"");
			string strLang = Program.Translation.Properties.Iso6391Code;
			if(string.IsNullOrEmpty(strLang)) strLang = "en";
			strLang = h(strLang);
			sb.Append(" lang=\"" + strLang + "\" xml:lang=\"" + strLang + "\"");
			if(p.Rtl) sb.Append(" dir=\"rtl\"");
			sb.AppendLine(">");

			sb.AppendLine("<head>");
			sb.AppendLine("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\" />");
			sb.Append("<title>");
			sb.Append(h(pgDataSource.Name));
			sb.AppendLine("</title>");
			sb.AppendLine("<meta http-equiv=\"expires\" content=\"0\" />");
			sb.AppendLine("<meta http-equiv=\"cache-control\" content=\"no-cache\" />");
			sb.AppendLine("<meta http-equiv=\"pragma\" content=\"no-cache\" />");

			sb.AppendLine("<style type=\"text/css\">");
			sb.AppendLine("/* <![CDATA[ */");

			sb.AppendLine("body, p, div, h1, h2, h3, h4, h5, h6, ol, ul, li, td, th, dd, dt, a {");
			sb.AppendLine("\tfont-family: \"Tahoma\", \"MS Sans Serif\", \"Sans Serif\", \"Verdana\", sans-serif;");
			sb.AppendLine("\tfont-size: 10pt;");
			sb.AppendLine("}");

			sb.AppendLine("span.fserif {");
			sb.AppendLine("\tfont-family: \"Times New Roman\", serif;");
			sb.AppendLine("}");

			sb.AppendLine("h1 { font-size: 2em; }");
			sb.AppendLine("h2 {");
			sb.AppendLine("\tfont-size: 1.5em;");
			sb.AppendLine("\tcolor: #000000;");
			sb.AppendLine("\tbackground-color: #D0D0D0;");
			sb.AppendLine("\tpadding-left: 2pt;");
			sb.AppendLine("\tpadding-right: 2pt;"); // RTL support
			sb.AppendLine("}");
			sb.AppendLine("h3 {");
			sb.AppendLine("\tfont-size: 1.2em;");
			sb.AppendLine("\tcolor: #000000;");
			sb.AppendLine("\tbackground-color: #D0D0D0;");
			sb.AppendLine("\tpadding-left: 2pt;");
			sb.AppendLine("\tpadding-right: 2pt;"); // RTL support
			sb.AppendLine("}");
			sb.AppendLine("h4 { font-size: 1em; }");
			sb.AppendLine("h5 { font-size: 0.89em; }");
			sb.AppendLine("h6 { font-size: 0.6em; }");

			sb.AppendLine("table {");
			sb.AppendLine("\twidth: 100%;");
			sb.AppendLine("\ttable-layout: fixed;");
			sb.AppendLine("}");

			sb.AppendLine("th {");
			sb.AppendLine("\ttext-align: " + (p.Rtl ? "right;" : "left;"));
			sb.AppendLine("\tvertical-align: top;");
			sb.AppendLine("\tfont-weight: bold;");
			sb.AppendLine("}");

			sb.AppendLine("td {");
			sb.AppendLine("\ttext-align: " + (p.Rtl ? "right;" : "left;"));
			sb.AppendLine("\tvertical-align: top;");
			sb.AppendLine("}");

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

			sb.AppendLine(".field_name {");
			sb.AppendLine("\t-webkit-hyphens: auto;");
			sb.AppendLine("\t-moz-hyphens: auto;");
			sb.AppendLine("\t-ms-hyphens: auto;");
			sb.AppendLine("\thyphens: auto;");
			sb.AppendLine("}");
			sb.AppendLine(".field_data {");
			// sb.AppendLine("\tword-break: break-all;");
			sb.AppendLine("\toverflow-wrap: break-word;");
			sb.AppendLine("\tword-wrap: break-word;");
			sb.AppendLine("}");

			sb.AppendLine(".icon_cli {");
			sb.AppendLine("\tdisplay: inline-block;");
			sb.AppendLine("\tmargin: 0px 0px 0px 0px;");
			sb.AppendLine("\tpadding: 0px 0px 0px 0px;");
			sb.AppendLine("\tborder: 0px none;");
			sb.AppendLine("\twidth: 1.1em;");
			sb.AppendLine("\theight: 1.1em;");
			sb.AppendLine("\tvertical-align: top;");
			sb.AppendLine("}");

			// Add the temporary content identifier
			sb.AppendLine("." + Program.TempFilesPool.TempContentTag + " {");
			sb.AppendLine("\tfont-size: 10pt;");
			sb.AppendLine("}");

			sb.AppendLine("/* ]]> */");
			sb.AppendLine("</style>");
			sb.AppendLine("</head><body>");

			sb.AppendLine("<h2>" + h(pgDataSource.Name) + "</h2>");
			WriteGroupNotes(sb, pgDataSource);

			EntryHandler ehInit = delegate(PwEntry pe)
			{
				p.Entry = pe;

				if(p.SprMode != 0)
					p.SprContext = new SprContext(pe, p.Database,
						SprCompileFlags.NonActive, false, false);
				else { Debug.Assert(p.SprContext == null); }

				Application.DoEvents();
				return true;
			};

			EntryHandler eh = null;
			string strTableInit = "<table>";
			PwGroup pgLast = null;

			if(m_rbTabular.Checked)
			{
				int nEquiCols = 0;
				if(bGroup) ++nEquiCols;
				if(bTitle) ++nEquiCols;
				if(bUserName) ++nEquiCols;
				if(bPassword) ++nEquiCols;
				if(bUrl) ++nEquiCols;
				if(bNotes) nEquiCols += 2;
				if(bCreation) ++nEquiCols;
				// if(bLastAcc) ++nEquiCols;
				if(bLastMod) ++nEquiCols;
				if(bExpire) ++nEquiCols;
				if(bTags) ++nEquiCols;
				if(bUuid) ++nEquiCols;
				if(nEquiCols == 0) nEquiCols = 1;

				string strColWidth = (100.0f / (float)nEquiCols).ToString(
					"F2", NumberFormatInfo.InvariantInfo);
				string strColWidth2 = (200.0f / (float)nEquiCols).ToString(
					"F2", NumberFormatInfo.InvariantInfo);

				string strHTdInit = "<th class=\"field_name\" style=\"width: " +
					strColWidth + "%;\">";
				string strHTdInit2 = "<th class=\"field_name\" style=\"width: " +
					strColWidth2 + "%;\">";
				string strHTdExit = "</th>";
				string strDataTdInit = "<td class=\"field_data\">";
				string strDataTdExit = "</td>";

				p.CellInit = strDataTdInit + p.FontInit;
				p.CellExit = p.FontExit + strDataTdExit;

				StringBuilder sbH = new StringBuilder();
				sbH.AppendLine();
				sbH.Append("<tr>");
				if(bGroup) sbH.AppendLine(strHTdInit + h(KPRes.Group) + strHTdExit);
				if(bTitle) sbH.AppendLine(strHTdInit + h(KPRes.Title) + strHTdExit);
				if(bUserName) sbH.AppendLine(strHTdInit + h(KPRes.UserName) + strHTdExit);
				if(bPassword) sbH.AppendLine(strHTdInit + h(KPRes.Password) + strHTdExit);
				if(bUrl) sbH.AppendLine(strHTdInit + h(KPRes.Url) + strHTdExit);
				if(bNotes) sbH.AppendLine(strHTdInit2 + h(KPRes.Notes) + strHTdExit);
				if(bCreation) sbH.AppendLine(strHTdInit + h(KPRes.CreationTime) + strHTdExit);
				// if(bLastAcc) sbH.AppendLine(strHTdInit + h(KPRes.LastAccessTime) + strHTdExit);
				if(bLastMod) sbH.AppendLine(strHTdInit + h(KPRes.LastModificationTime) + strHTdExit);
				if(bExpire) sbH.AppendLine(strHTdInit + h(KPRes.ExpiryTime) + strHTdExit);
				if(bTags) sbH.AppendLine(strHTdInit + h(KPRes.Tags) + strHTdExit);
				if(bUuid) sbH.AppendLine(strHTdInit + h(KPRes.Uuid) + strHTdExit);
				sbH.Append("</tr>"); // No terminating \r\n

				strTableInit += sbH.ToString();
				sb.AppendLine(strTableInit);

				eh = delegate(PwEntry pe)
				{
					ehInit(pe);

					sb.AppendLine("<tr>");

					WriteTabularIf(bGroup, sb, c(pe.ParentGroup.Name), p);
					WriteTabularIf(bTitle, sb, pe, PwDefs.TitleField, p);
					WriteTabularIf(bUserName, sb, pe, PwDefs.UserNameField, p);

					if(bPassword)
					{
						if(p.MonoPasswords)
							sb.Append(strDataTdInit + (p.SmallMono ?
								"<code><small>" : "<code>"));
						else sb.Append(p.CellInit);

						string strInner = cs(pe.Strings.ReadSafe(PwDefs.PasswordField));
						if(strInner.Length == 0) strInner = "&nbsp;";
						sb.Append(strInner);

						if(p.MonoPasswords)
							sb.AppendLine((p.SmallMono ? "</small></code>" :
								"</code>") + strDataTdExit);
						else sb.AppendLine(p.CellExit);
					}

					// WriteTabularIf(bUrl, sb, pe, PwDefs.UrlField, p);
					WriteTabularIf(bUrl, sb, MakeUrlLink(pe.Strings.ReadSafe(
						PwDefs.UrlField), p), p);

					WriteTabularIf(bNotes, sb, pe, PwDefs.NotesField, p);

					WriteTabularIf(bCreation, sb, h(TimeUtil.ToDisplayString(
						pe.CreationTime)), p);
					// WriteTabularIf(bLastAcc, sb, h(TimeUtil.ToDisplayString(
					//	pe.LastAccessTime)), p);
					WriteTabularIf(bLastMod, sb, h(TimeUtil.ToDisplayString(
						pe.LastModificationTime)), p);
					WriteTabularIf(bExpire, sb, h(pe.Expires ? TimeUtil.ToDisplayString(
						pe.ExpiryTime) : KPRes.NeverExpires), p);

					WriteTabularIf(bTags, sb, h(StrUtil.TagsToString(pe.Tags, true)), p);

					WriteTabularIf(bUuid, sb, pe.Uuid.ToHexString(), p);

					sb.AppendLine("</tr>");
					return true;
				};
			}
			else if(m_rbDetails.Checked)
			{
				sb.AppendLine(strTableInit);

				if(pgDataSource.Entries.UCount == 0)
					sb.AppendLine(@"<tr><td>&nbsp;</td></tr>");

				eh = delegate(PwEntry pe)
				{
					ehInit(pe);

					if((pgLast != null) && (pgLast == pe.ParentGroup))
						sb.AppendLine("<tr><td colspan=\"2\"><hr /></td></tr>");

					if(bGroup) WriteDetailsLine(sb, KPRes.Group, pe.ParentGroup.Name, p);
					if(bTitle)
					{
						PfOptions pSub = p.CloneShallow();
						pSub.FontInit = MakeIconImg(pe.IconId, pe.CustomIconUuid, pe,
							p) + pSub.FontInit + "<b>";
						pSub.FontExit = "</b>" + pSub.FontExit;

						WriteDetailsLine(sb, KPRes.Title, pe.Strings.ReadSafe(
							PwDefs.TitleField), pSub);
					}
					if(bUserName) WriteDetailsLine(sb, KPRes.UserName, pe.Strings.ReadSafe(
						PwDefs.UserNameField), p);
					if(bPassword) WriteDetailsLine(sb, KPRes.Password, pe.Strings.ReadSafe(
						PwDefs.PasswordField), p);
					if(bUrl) WriteDetailsLine(sb, KPRes.Url, pe.Strings.ReadSafe(
						PwDefs.UrlField), p);
					if(bNotes) WriteDetailsLine(sb, KPRes.Notes, pe.Strings.ReadSafe(
						PwDefs.NotesField), p);
					if(bCreation) WriteDetailsLine(sb, KPRes.CreationTime, TimeUtil.ToDisplayString(
						pe.CreationTime), p);
					// if(bLastAcc) WriteDetailsLine(sb, KPRes.LastAccessTime, TimeUtil.ToDisplayString(
					//	pe.LastAccessTime), p);
					if(bLastMod) WriteDetailsLine(sb, KPRes.LastModificationTime, TimeUtil.ToDisplayString(
						pe.LastModificationTime), p);
					if(bExpire) WriteDetailsLine(sb, KPRes.ExpiryTime, (pe.Expires ? TimeUtil.ToDisplayString(
						pe.ExpiryTime) : KPRes.NeverExpires), p);

					if(bAutoType)
					{
						foreach(AutoTypeAssociation a in pe.AutoType.Associations)
							WriteDetailsLine(sb, KPRes.AutoType, a.WindowName +
								": " + a.Sequence, p);
					}

					if(bTags) WriteDetailsLine(sb, KPRes.Tags, StrUtil.TagsToString(
						pe.Tags, true), p);
					if(bUuid) WriteDetailsLine(sb, KPRes.Uuid, pe.Uuid.ToHexString(), p);

					foreach(KeyValuePair<string, ProtectedString> kvp in pe.Strings)
					{
						if(bCustomStrings && !PwDefs.IsStandardField(kvp.Key))
							WriteDetailsLine(sb, kvp, p);
					}

					pgLast = pe.ParentGroup;
					return true;
				};
			}
			else { Debug.Assert(false); }

			GroupHandler gh = delegate(PwGroup pg)
			{
				if(pg.Entries.UCount == 0) return true;

				sb.Append("</table><br /><br /><h3>"); // "</table><br /><hr /><h3>"
				// sb.Append(MakeIconImg(pg.IconId, pg.CustomIconUuid, pg, p));
				sb.Append(h(pg.GetFullPath(" - ", false)));
				sb.AppendLine("</h3>");
				WriteGroupNotes(sb, pg);
				sb.AppendLine(strTableInit);

				return true;
			};

			pgDataSource.TraverseTree(TraversalMethod.PreOrder, gh, eh);

			if(m_rbTabular.Checked)
				sb.AppendLine("</table>");
			else if(m_rbDetails.Checked)
				sb.AppendLine("</table><br />");

			sb.AppendLine("</body></html>");
			return sb.ToString();
		}

		private static string CompileText(string strText, PfOptions p, bool bToHtml,
			bool bNbsp)
		{
			string str = strText;

			if(g_strCodeItS == null)
			{
				g_strCodeItS = (new PwUuid(true)).ToHexString();
				g_strCodeItE = (new PwUuid(true)).ToHexString();
			}

			if((p.SprMode != 0) && (p.SprContext != null))
			{
				string strPre = str;
				str = SprEngine.Compile(str, p.SprContext);

				if((p.SprMode == 2) && (str != strPre))
				{
					if(bToHtml) str += " - " + g_strCodeItS + strPre + g_strCodeItE;
					else str += " - " + strPre;
				}
			}

			if(bToHtml) str = StrUtil.StringToHtml(str, bNbsp);

			str = str.Replace(g_strCodeItS, "<i>");
			str = str.Replace(g_strCodeItE, "</i>");
			return str;
		}

		private static void WriteTabularIf(bool bCondition, StringBuilder sb,
			PwEntry pe, string strField, PfOptions p)
		{
			if(!bCondition) return;

			string str = CompileText(pe.Strings.ReadSafe(strField), p, true, false);

			if(strField == PwDefs.TitleField)
				str = MakeIconImg(pe.IconId, pe.CustomIconUuid, pe, p) + str;

			WriteTabularIf(bCondition, sb, str, p);
		}

		private static void WriteTabularIf(bool bCondition, StringBuilder sb,
			string strValue, PfOptions p)
		{
			if(!bCondition) return;

			sb.Append(p.CellInit);

			if(strValue.Length > 0) sb.Append(strValue); // Don't HTML-encode
			else sb.Append(@"&nbsp;");

			sb.AppendLine(p.CellExit);
		}

		private static void WriteDetailsLine(StringBuilder sb,
			KeyValuePair<string, ProtectedString> kvp, PfOptions p)
		{
			sb.Append("<tr><td class=\"field_name\" style=\"width: 20%;\"><i>");
			sb.Append(StrUtil.StringToHtml(kvp.Key));
			sb.AppendLine(":</i></td>");

			sb.Append("<td class=\"field_data\" style=\"width: 80%;\">");

			bool bUrl = (kvp.Key == KPRes.Url);
			bool bPassword = (kvp.Key == KPRes.Password);
			bool bCode = (p.MonoPasswords && bPassword);

			if(bCode) sb.Append(p.SmallMono ? "<code><small>" : "<code>");
			else sb.Append(p.FontInit);

			if(bUrl && !kvp.Value.IsEmpty)
				sb.Append(MakeUrlLink(kvp.Value.ReadString(), p));
			else
			{
				string strInner = CompileText(kvp.Value.ReadString(), p,
					true, bPassword);
				if(strInner.Length == 0) strInner = "&nbsp;";
				sb.Append(strInner);
			}

			if(bCode) sb.Append(p.SmallMono ? "</small></code>" : "</code>");
			else sb.Append(p.FontExit);

			sb.AppendLine("</td></tr>");
		}

		private static void WriteDetailsLine(StringBuilder sb, string strIndex,
			string strValue, PfOptions p)
		{
			if(string.IsNullOrEmpty(strValue)) return;

			KeyValuePair<string, ProtectedString> kvp = new KeyValuePair<string, ProtectedString>(
				strIndex, new ProtectedString(false, strValue));
			WriteDetailsLine(sb, kvp, p);
		}

		private static string MakeIconImg(PwIcon i, PwUuid ci, ITimeLogger tl, PfOptions p)
		{
			if(p.ClientIcons == null) return string.Empty;

			Image img = null;

			if((tl != null) && tl.Expires && (tl.ExpiryTime <= p.Now))
			{
				i = PwIcon.Expired;
				ci = null;
			}

			PwDatabase pd = p.Database;
			if((ci != null) && !ci.Equals(PwUuid.Zero) && (pd != null))
			{
				int cix = pd.GetCustomIconIndex(ci);
				if(cix >= 0)
				{
					cix += (int)PwIcon.Count;
					if(cix < p.ClientIcons.Images.Count)
						img = p.ClientIcons.Images[cix];
					else { Debug.Assert(false); }
				}
				else { Debug.Assert(false); }
			}

			int ix = (int)i;
			if((img == null) && (ix >= 0) && (ix < p.ClientIcons.Images.Count))
				img = p.ClientIcons.Images[ix];

			string strData = GfxUtil.ImageToDataUri(img);
			if(string.IsNullOrEmpty(strData)) { Debug.Assert(false); return string.Empty; }

			StringBuilder sb = new StringBuilder();
			sb.Append("<img src=\"");
			sb.Append(strData);
			sb.AppendLine("\"");
			sb.Append("class=\"icon_cli\" alt=\"\" />&nbsp;");

			return sb.ToString();
		}

		private static string MakeUrlLink(string strRawUrl, PfOptions p)
		{
			if(string.IsNullOrEmpty(strRawUrl)) return string.Empty;

			string strCmp = CompileText(strRawUrl, p, true, false);

			string strHRef = strCmp;
			if(p.SprMode == 2) // Use only Spr-compiled URL for HRef, not both
			{
				PfOptions pSub = p.CloneShallow();
				pSub.SprMode = 1;
				strHRef = CompileText(strRawUrl, pSub, true, false);
			}
			// Do not Spr-compile URL for HRef when p.SprMode == 0, because
			// this could unexpectedly disclose external data

			return ("<a href=\"" + strHRef + "\">" + p.FontInit + strCmp +
				p.FontExit + "</a>");
		}

		private static void WriteGroupNotes(StringBuilder sb, PwGroup pg)
		{
			string str = pg.Notes.Trim();
			if(str.Length == 0) return;

			// No <p>...</p> due to padding/margin
			sb.AppendLine("<table><tr><td>" + StrUtil.StringToHtml(str) +
				"</td></tr></table><br />");
		}

		private static void SortGroupEntriesRecursive(PwGroup pg, string strFieldName)
		{
			PwEntryComparer cmp = new PwEntryComparer(strFieldName, true, true);
			pg.Entries.Sort(cmp);

			foreach(PwGroup pgSub in pg.Groups)
			{
				SortGroupEntriesRecursive(pgSub, strFieldName);
			}
		}

		private void OnBtnConfigPage(object sender, EventArgs e)
		{
			UpdateWebBrowser(false);

			try { m_wbMain.ShowPageSetupDialog(); } // Throws in Mono 1.2.6+
			catch(NotImplementedException)
			{
				MessageService.ShowWarning(KLRes.FrameworkNotImplExcp);
			}
			catch(Exception ex) { MessageService.ShowWarning(ex); }
		}

		private void OnBtnPrintPreview(object sender, EventArgs e)
		{
			UpdateWebBrowser(false);

			try { m_wbMain.ShowPrintPreviewDialog(); } // Throws in Mono 1.2.6+
			catch(NotImplementedException)
			{
				MessageService.ShowWarning(KLRes.FrameworkNotImplExcp);
			}
			catch(Exception ex) { MessageService.ShowWarning(ex); }
		}

		private void OnTabSelectedIndexChanged(object sender, EventArgs e)
		{
			if(m_tabMain.SelectedIndex == 0) UpdateWebBrowser(false);
		}

		private void OnTabularCheckedChanged(object sender, EventArgs e)
		{
			UpdateUIState();
		}

		private CheckBox[] GetAllFields()
		{
			return new CheckBox[] {
				m_cbTitle, m_cbUser, m_cbPassword, m_cbUrl, m_cbNotes,
				m_cbCreation, m_cbLastMod, m_cbExpire, // m_cbLastAccess
				m_cbAutoType, m_cbTags,
				m_cbIcon, m_cbCustomStrings, m_cbGroups, m_cbUuid
			};
		}

		private void OnLinkSelectAllFields(object sender, LinkLabelLinkClickedEventArgs e)
		{
			foreach(CheckBox cb in GetAllFields()) { cb.Checked = true; }
			UpdateUIState();
		}

		private void OnLinkDeselectAllFields(object sender, LinkLabelLinkClickedEventArgs e)
		{
			foreach(CheckBox cb in GetAllFields()) { cb.Checked = false; }
			UpdateUIState();
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			if(m_ilTabIcons != null)
			{
				m_tabMain.ImageList = null;
				m_ilTabIcons.Dispose();
				m_ilTabIcons = null;
			}
			else { Debug.Assert(false); }

			GlobalWindowManager.RemoveWindow(this);
		}

		private void OnFormClosing(object sender, FormClosingEventArgs e)
		{
			if(m_iBlockPreviewRefresh > 0) e.Cancel = true;
		}

		private void OnIconCheckedChanged(object sender, EventArgs e)
		{
			UpdateUIState();
		}
	}
}
