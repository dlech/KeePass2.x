/*
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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

using KeePass.App;
using KeePass.App.Configuration;
using KeePass.DataExchange.Formats;
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

		private ImageList m_ilTabIcons = null;
		private FontControlGroup m_fcgMain = null;
		private FontControlGroup m_fcgPassword = null;

		private uint m_uBlockUpdateUIState = 0;
		private uint m_uBlockPreviewRefresh = 0;
		private Control m_cPreBlock = null;

		private string m_strGeneratedHtml = string.Empty;
		public string GeneratedHtml
		{
			get { return m_strGeneratedHtml; }
		}

		private static string g_strCodeItS = null;
		private static string g_strCodeItE = null;

		private static readonly string[] g_vMainFonts = new string[] {
			"Tahoma", "Arial", "Microsoft Sans Serif", "Noto Sans",
			"Verdana", "DejaVu Sans" // "Liberation Sans"
		};
		private static readonly string[] g_vMonoFonts = new string[] {
			"Courier New", "Noto Mono", "DejaVu Sans Mono",
			"Lucida Console" // "Liberation Mono"
		};

		private sealed class PfOptions
		{
			public Color ColorPU = Color.Empty;
			public Color ColorPL = Color.Empty;
			public Color ColorPD = Color.Empty;
			public Color ColorPO = Color.Empty;

			public int SprMode = 0;

			public string CellInit = string.Empty;
			public string CellExit = string.Empty;

			public string ValueInit = string.Empty;
			public string ValueExit = string.Empty;

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
			GlobalWindowManager.InitializeForm(this);
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

			++m_uBlockUpdateUIState;
			++m_uBlockPreviewRefresh;

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

			FontUtil.AssignDefaultBold(m_rbLayTable);
			FontUtil.AssignDefaultBold(m_rbLayBlocks);

			m_rbLayTable.Checked = true;

			m_cbColorP.Checked = true;
			using(RtlAwareResizeScope r = new RtlAwareResizeScope(
				m_lblColorPU, m_lblColorPL, m_lblColorPD, m_lblColorPO))
			{
				m_lblColorPU.Text = "\u27A5 ABC...:";
				m_lblColorPL.Text = "\u27A5 abc...:";
				m_lblColorPD.Text = "\u27A5 012...:";
				m_lblColorPO.Text = "\u27A5 !$%...:";
			}
			m_btnColorPU.SelectedColor = Color.FromArgb(0, 0, 255);
			m_btnColorPL.SelectedColor = Color.FromArgb(0, 0, 0);
			m_btnColorPD.SelectedColor = Color.FromArgb(0, 128, 0);
			m_btnColorPO.SelectedColor = Color.FromArgb(192, 0, 0);

			m_fcgMain = new FontControlGroup(m_cbMainFont, m_btnMainFont,
				null, GetDefaultFont(false));
			m_fcgPassword = new FontControlGroup(m_cbPasswordFont, m_btnPasswordFont,
				null, GetDefaultFont(true));

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

			Debug.Assert(!m_cmbSpr.Sorted);
			m_cmbSpr.Items.Add(KPRes.ReplaceNo);
			m_cmbSpr.Items.Add(KPRes.Replace + " (" + KPRes.Slow + ")");
			m_cmbSpr.Items.Add(KPRes.BothForms + " (" + KPRes.Slow + ")");
			m_cmbSpr.SelectedIndex = 0;

			UIUtil.SetButtonImage(m_btnConfigPrinter,
				Properties.Resources.B16x16_EditCopy, true);
			UIUtil.SetButtonImage(m_btnPrintPreview,
				Properties.Resources.B16x16_FileQuickPrint, true);

			if(!m_bPrintMode) // Export to HTML
			{
				m_lblPreviewHint.Visible = false;
				m_btnConfigPrinter.Visible = false;
				m_btnPrintPreview.Visible = false;
				m_btnOK.Text = KPRes.Export;
			}

			Program.TempFilesPool.AddWebBrowserPrintContent();

			--m_uBlockUpdateUIState;
			UpdateUIState(); // May adjust some states
			--m_uBlockPreviewRefresh;
			UpdateWebBrowser(true);
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
			else m_strGeneratedHtml = (GenerateHtmlDocument(false) ?? string.Empty);
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
		}

		private void UpdateUIState()
		{
			if(m_uBlockUpdateUIState != 0) return;
			++m_uBlockUpdateUIState;

			bool bLayBlocks = m_rbLayBlocks.Checked;

			bool bIcon = m_cbIcon.Checked;
			// Here (not in OnFormLoad), due to 'Select All' impl.
			if(m_ilClientIcons == null)
			{
				UIUtil.SetEnabled(m_cbIcon, false);
				UIUtil.SetChecked(m_cbIcon, false);
				bIcon = false;
			}

			UIUtil.SetEnabled(m_cbTitle, !bIcon);
			if(bIcon) UIUtil.SetChecked(m_cbTitle, true);

			UIUtil.SetEnabled(m_cbAutoType, bLayBlocks);
			UIUtil.SetEnabled(m_cbCustomStrings, bLayBlocks);
			if(!bLayBlocks)
			{
				UIUtil.SetChecked(m_cbAutoType, false);
				UIUtil.SetChecked(m_cbCustomStrings, false);
			}

			if((m_pgDataSource != null) && m_pgDataSource.IsVirtual)
			{
				UIUtil.SetEnabled(m_cbGroups, false);
				UIUtil.SetChecked(m_cbGroups, false);
			}

			UIUtil.SetEnabledFast(m_cbColorP.Checked,
				m_lblColorPU, m_btnColorPU, m_lblColorPL, m_btnColorPL,
				m_lblColorPD, m_btnColorPD, m_lblColorPO, m_btnColorPO);

			--m_uBlockUpdateUIState;
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
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("<!DOCTYPE html>");
			sb.AppendLine("<html xmlns=\"http://www.w3.org/1999/xhtml\">");
			sb.AppendLine("<head>");
			sb.AppendLine("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />");
			sb.AppendLine("<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\" />");
			sb.AppendLine("<title>...</title>");
			sb.AppendLine("</head><body><br /><br />");
			sb.AppendLine("<h1 style=\"text-align: center;\">&#8987;</h1>");
			sb.AppendLine("</body></html>");

			try { UIUtil.SetWebBrowserDocument(m_wbMain, sb.ToString()); }
			catch(Exception) { Debug.Assert(NativeLib.IsUnix()); } // Throws in Mono 2.0+
		} */

		private void UpdateWebBrowser(bool bInitial)
		{
			if(m_uBlockPreviewRefresh != 0) return;

			++m_uBlockPreviewRefresh;
			if(!bInitial) UIBlockInteraction(true);
			// ShowWaitDocument();

			string strHtml = GenerateHtmlDocument(true);

			try { UIUtil.SetWebBrowserDocument(m_wbMain, strHtml); }
			catch(Exception) { Debug.Assert(NativeLib.IsUnix()); } // Throws in Mono 2.0+
			try { m_wbMain.AllowNavigation = false; }
			catch(Exception) { Debug.Assert(false); }

			if(!bInitial) UIBlockInteraction(false);
			--m_uBlockPreviewRefresh;
		}

		private string GenerateHtmlDocument(bool bTemporary)
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

			bool bTabular = m_rbLayTable.Checked;
			bool bBlocks = m_rbLayBlocks.Checked;

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

			if(m_cbColorP.Checked)
			{
				p.ColorPU = m_btnColorPU.SelectedColor;
				p.ColorPL = m_btnColorPL.SelectedColor;
				p.ColorPD = m_btnColorPD.SelectedColor;
				p.ColorPO = m_btnColorPO.SelectedColor;
			}

			p.SprMode = m_cmbSpr.SelectedIndex;
			p.Rtl = (this.RightToLeft == RightToLeft.Yes);
			p.Database = m_pdContext;
			if(m_cbIcon.Checked) p.ClientIcons = m_ilClientIcons;

			GFunc<string, string> h = new GFunc<string, string>(StrUtil.StringToHtml);
			GFunc<string, string> c = delegate(string strRaw)
			{
				return CompileToHtml(strRaw, p, false);
			};

			StringBuilder sb = KeePassHtml2x.HtmlPart1ToHead(p.Rtl, pgDataSource.Name);
			KeePassHtml2x.HtmlPart2ToStyle(sb);

			sb.AppendLine("body {");
			sb.AppendLine("\tcolor: #000000;");
			sb.AppendLine("\tbackground-color: #FFFFFF;");
			sb.Append(GetFontCss(false, "\t"));
			sb.AppendLine("}");

			sb.AppendLine("h2, h3 {");
			sb.AppendLine("\tcolor: #000000;");
			sb.AppendLine("\tbackground-color: #D0D0D0;");
			sb.AppendLine("\tborder: 1px solid #808080;");
			sb.AppendLine("\tpadding-left: 2pt;");
			sb.AppendLine("\tpadding-right: 2pt;"); // RTL support
			sb.AppendLine("}");

			sb.AppendLine("table, th, td {");
			sb.AppendLine("\tborder: 0px none;");
			sb.AppendLine("\tborder-collapse: collapse;");
			sb.AppendLine("}");

			sb.AppendLine("table {");
			sb.AppendLine("\twidth: 100%;");
			sb.AppendLine("\ttable-layout: fixed;");
			sb.AppendLine("\tempty-cells: show;");
			sb.AppendLine("}");

			sb.AppendLine("th, td {");
			sb.AppendLine("\ttext-align: " + (p.Rtl ? "right;" : "left;"));
			sb.AppendLine("\tvertical-align: top;");
			sb.AppendLine("}");

			sb.AppendLine("th {");
			sb.AppendLine("\tfont-weight: bold;");
			sb.AppendLine("}");

			sb.AppendLine("table.t_entries_t tr th, table.t_entries_b tr td:first-of-type {");
			sb.AppendLine("\t-webkit-hyphens: auto;");
			sb.AppendLine("\t-moz-hyphens: auto;");
			sb.AppendLine("\t-ms-hyphens: auto;");
			sb.AppendLine("\thyphens: auto;");
			sb.AppendLine("}");

			sb.AppendLine("table.t_entries_t tr td, table.t_entries_b tr td:last-of-type {");
			// sb.AppendLine("\tword-break: break-all;");
			sb.AppendLine("\toverflow-wrap: break-word;");
			sb.AppendLine("\tword-wrap: break-word;");
			sb.AppendLine("}");

			sb.AppendLine("table.t_entries_t tr td {");
			sb.AppendLine("\tborder-style: solid none;");
			sb.AppendLine("\tborder-width: 1px 0px;");
			sb.AppendLine("\tborder-top-color: #808080;");
			sb.AppendLine("\tborder-bottom-color: #808080;");
			sb.AppendLine("}");

			sb.AppendLine("table.t_entries_b tr td:first-of-type {");
			sb.AppendLine("\twidth: 20%;");
			sb.AppendLine("}");
			sb.AppendLine("table.t_entries_b tr td:last-of-type {");
			sb.AppendLine("\twidth: 80%;");
			sb.AppendLine("}");

			sb.AppendLine("hr {");
			sb.AppendLine("\theight: 0px;");
			sb.AppendLine("\tpadding: 0px;");
			sb.AppendLine("\tborder-style: none none solid none;");
			sb.AppendLine("\tborder-width: 0px 0px 1px 0px;");
			sb.AppendLine("\tborder-bottom-color: #808080;");
			sb.AppendLine("}");

			sb.AppendLine("a {");
			sb.AppendLine("\tcolor: #0000DD;");
			sb.AppendLine("\ttext-decoration: none;");
			sb.AppendLine("}");
			sb.AppendLine("a:hover, a:active {");
			sb.AppendLine("\tcolor: #6699FF;");
			sb.AppendLine("\ttext-decoration: underline;");
			sb.AppendLine("}");

			sb.AppendLine(".icon_cli {");
			sb.AppendLine("\tdisplay: inline-block;");
			sb.AppendLine("\tmargin: 0px;");
			sb.AppendLine("\tpadding: 0px;");
			sb.AppendLine("\tborder: 0px none;");
			sb.AppendLine("\twidth: 1.1em;");
			sb.AppendLine("\theight: 1.1em;");
			sb.AppendLine("\tvertical-align: top;");
			sb.AppendLine("}");

			sb.AppendLine(".f_password {");
			sb.Append(GetFontCss(true, "\t"));
			sb.AppendLine("\twhite-space: pre-wrap;");
			sb.AppendLine("\ttab-size: 4;");
			sb.AppendLine("}");

			sb.AppendLine(".f_password_u {");
			sb.Append("\tcolor: ");
			sb.Append(StrUtil.ColorToUnnamedHtml(p.ColorPU, false));
			sb.AppendLine(";");
			sb.AppendLine("}");

			sb.AppendLine(".f_password_l {");
			sb.Append("\tcolor: ");
			sb.Append(StrUtil.ColorToUnnamedHtml(p.ColorPL, false));
			sb.AppendLine(";");
			sb.AppendLine("}");

			sb.AppendLine(".f_password_d {");
			sb.Append("\tcolor: ");
			sb.Append(StrUtil.ColorToUnnamedHtml(p.ColorPD, false));
			sb.AppendLine(";");
			sb.AppendLine("}");

			sb.AppendLine(".f_password_o {");
			sb.Append("\tcolor: ");
			sb.Append(StrUtil.ColorToUnnamedHtml(p.ColorPO, false));
			sb.AppendLine(";");
			sb.AppendLine("}");

			if(bTemporary)
			{
				// Add the temporary content identifier
				sb.AppendLine("." + Program.TempFilesPool.TempContentTag + " {");
				sb.AppendLine("\tfont-size: 10pt;");
				sb.AppendLine("}");
			}

			KeePassHtml2x.HtmlPart3ToBody(sb);

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
			string strTableInit = "<table>"; // Customized below
			PwGroup pgLast = null;

			if(bTabular)
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

				NumberFormatInfo nfi = NumberFormatInfo.InvariantInfo;
				string strColWidth = (100.0f / (float)nEquiCols).ToString("F2", nfi);
				string strColWidth2 = (200.0f / (float)nEquiCols).ToString("F2", nfi);

				StringBuilder sbTI = new StringBuilder();
				sbTI.AppendLine("<table class=\"t_entries_t\">");
				sbTI.AppendLine("<tr>");
				GAction<string, bool> fAppendTH = delegate(string strColumn, bool bLarge)
				{
					sbTI.Append("<th style=\"width: ");
					sbTI.Append(bLarge ? strColWidth2 : strColWidth);
					sbTI.Append("%;\">");
					sbTI.Append(h(strColumn));
					sbTI.AppendLine("</th>");
				};
				if(bGroup) fAppendTH(KPRes.Group, false);
				if(bTitle) fAppendTH(KPRes.Title, false);
				if(bUserName) fAppendTH(KPRes.UserName, false);
				if(bPassword) fAppendTH(KPRes.Password, false);
				if(bUrl) fAppendTH(KPRes.Url, false);
				if(bNotes) fAppendTH(KPRes.Notes, true);
				if(bCreation) fAppendTH(KPRes.CreationTime, false);
				// if(bLastAcc) fAppendTH(KPRes.LastAccessTime, false);
				if(bLastMod) fAppendTH(KPRes.LastModificationTime, false);
				if(bExpire) fAppendTH(KPRes.ExpiryTime, false);
				if(bTags) fAppendTH(KPRes.Tags, false);
				if(bUuid) fAppendTH(KPRes.Uuid, false);
				sbTI.Append("</tr>"); // No terminating new-line
				strTableInit = sbTI.ToString();

				sb.AppendLine(strTableInit);

				p.CellInit = "<td>";
				p.CellExit = "</td>";

				eh = delegate(PwEntry pe)
				{
					ehInit(pe);

					sb.AppendLine("<tr>");

					WriteTabularIf(bGroup, sb, c(pe.ParentGroup.Name), p);
					WriteTabularIf(bTitle, sb, pe, PwDefs.TitleField, p);
					WriteTabularIf(bUserName, sb, pe, PwDefs.UserNameField, p);
					WriteTabularIf(bPassword, sb, pe, PwDefs.PasswordField, p);

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
			else if(bBlocks)
			{
				strTableInit = "<table class=\"t_entries_b\">";

				sb.AppendLine(strTableInit);

				if(pgDataSource.Entries.UCount == 0)
				{
					// Validator: "Table column 2 established by element 'td'
					// has no cells beginning in it."
					// sb.AppendLine("<tr><td colspan=\"2\">&nbsp;</td></tr>");
					sb.AppendLine("<tr><td>&nbsp;</td><td>&nbsp;</td></tr>");
				}

				eh = delegate(PwEntry pe)
				{
					ehInit(pe);

					if((pgLast != null) && (pgLast == pe.ParentGroup))
						sb.AppendLine("<tr><td colspan=\"2\"><hr /></td></tr>");

					if(bGroup) WriteBlockLine(sb, KPRes.Group, pe.ParentGroup.Name, p);
					if(bTitle)
					{
						PfOptions pSub = p.CloneShallow();
						pSub.ValueInit = pSub.ValueInit + MakeIconImg(pe.IconId,
							pe.CustomIconUuid, pe, p) + "<b>";
						pSub.ValueExit = "</b>" + pSub.ValueExit;

						WriteBlockLine(sb, KPRes.Title, pe.Strings.ReadSafe(
							PwDefs.TitleField), pSub);
					}
					if(bUserName) WriteBlockLine(sb, KPRes.UserName, pe.Strings.ReadSafe(
						PwDefs.UserNameField), p);
					if(bPassword) WriteBlockLine(sb, KPRes.Password, pe.Strings.ReadSafe(
						PwDefs.PasswordField), p);
					if(bUrl) WriteBlockLine(sb, KPRes.Url, pe.Strings.ReadSafe(
						PwDefs.UrlField), p);
					if(bNotes) WriteBlockLine(sb, KPRes.Notes, pe.Strings.ReadSafe(
						PwDefs.NotesField), p);
					if(bCreation) WriteBlockLine(sb, KPRes.CreationTime, TimeUtil.ToDisplayString(
						pe.CreationTime), p);
					// if(bLastAcc) WriteBlockLine(sb, KPRes.LastAccessTime, TimeUtil.ToDisplayString(
					//	pe.LastAccessTime), p);
					if(bLastMod) WriteBlockLine(sb, KPRes.LastModificationTime, TimeUtil.ToDisplayString(
						pe.LastModificationTime), p);
					if(bExpire) WriteBlockLine(sb, KPRes.ExpiryTime, (pe.Expires ? TimeUtil.ToDisplayString(
						pe.ExpiryTime) : KPRes.NeverExpires), p);

					if(bAutoType)
					{
						foreach(AutoTypeAssociation a in pe.AutoType.Associations)
							WriteBlockLine(sb, KPRes.AutoType, a.WindowName +
								": " + a.Sequence, p);
					}

					if(bTags) WriteBlockLine(sb, KPRes.Tags, StrUtil.TagsToString(
						pe.Tags, true), p);
					if(bUuid) WriteBlockLine(sb, KPRes.Uuid, pe.Uuid.ToHexString(), p);

					foreach(KeyValuePair<string, ProtectedString> kvp in pe.Strings)
					{
						if(bCustomStrings && !PwDefs.IsStandardField(kvp.Key))
							WriteBlockLine(sb, kvp, p);
					}

					pgLast = pe.ParentGroup;
					return true;
				};
			}
			else { Debug.Assert(false); } // Unknown layout

			GroupHandler gh = delegate(PwGroup pg)
			{
				if(pg.Entries.UCount == 0) return true;

				sb.AppendLine("</table><br />");
				sb.Append("<h3>");
				// sb.Append(MakeIconImg(pg.IconId, pg.CustomIconUuid, pg, p));
				sb.Append(h(pg.GetFullPath(true, false)));
				sb.AppendLine("</h3>");
				WriteGroupNotes(sb, pg);
				sb.AppendLine(strTableInit);

				return true;
			};

			pgDataSource.TraverseTree(TraversalMethod.PreOrder, gh, eh);

			if(bTabular || bBlocks) sb.AppendLine("</table>");
			else { Debug.Assert(false); }

			KeePassHtml2x.HtmlPart4ToEnd(sb);

			string strDoc = sb.ToString();
#if DEBUG
			XmlUtilEx.ValidateXml(strDoc, true);
#endif
			return strDoc;
		}

		private AceFont GetDefaultFont(bool bPassword)
		{
			const float fSize = 10.0f;

			string[] v = (bPassword ? g_vMonoFonts : g_vMainFonts);
			foreach(string strFamily in v)
			{
				if(FontUtil.IsInstalled(strFamily))
				{
					AceFont af = new AceFont();
					af.Family = strFamily;
					af.Size = fSize;
					af.GraphicsUnit = GraphicsUnit.Point;
					return af;
				}
			}

			Debug.Assert(false);
			AceFont afUI = new AceFont(m_lblEntrySortHint.Font, false);
			afUI.Size = fSize;
			afUI.GraphicsUnit = GraphicsUnit.Point;
			return afUI;
		}

		private string GetFontCss(bool bPassword, string strIndent)
		{
			string strFallback = (bPassword ? "monospace" : "sans-serif");

			AceFont af = (bPassword ? m_fcgPassword.SelectedFont : m_fcgMain.SelectedFont);
			if(af.OverrideUIDefault) return af.ToCss(strIndent, strFallback, true);

			af = GetDefaultFont(bPassword);

			StringBuilder sb = new StringBuilder();

			string[] v = (bPassword ? g_vMonoFonts : g_vMainFonts);
			Debug.Assert((v != null) && (v.Length != 0));
			sb.Append(strIndent);
			sb.Append("font-family: \"");
			sb.Append(string.Join("\", \"", v));
			sb.Append("\", ");
			sb.Append(strFallback);
			sb.AppendLine(";");

			if(!bPassword)
			{
				sb.Append(strIndent);
				sb.Append("font-size: ");
				sb.Append(af.Size.ToString(NumberFormatInfo.InvariantInfo));
				sb.Append(GfxUtil.GraphicsUnitToString(af.GraphicsUnit));
				sb.AppendLine(";");
			}
			// else inherit size from main font

			return sb.ToString();
		}

		private static string CompileToHtml(string strText, PfOptions p, bool bPassword)
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
					str += " \u2013 " + g_strCodeItS + strPre + g_strCodeItE;
			}

			if(bPassword) str = PasswordToHtml(str, p);
			else str = StrUtil.StringToHtml(str.Trim());

			str = str.Replace(g_strCodeItS, "<i>");
			str = str.Replace(g_strCodeItE, "</i>");
			return str;
		}

		private static void WriteTabularIf(bool bCondition, StringBuilder sb,
			PwEntry pe, string strField, PfOptions p)
		{
			if(!bCondition) return;

			bool bPassword = (strField == PwDefs.PasswordField);

			string str = CompileToHtml(pe.Strings.ReadSafe(strField), p, bPassword);

			if(strField == PwDefs.TitleField)
				str = MakeIconImg(pe.IconId, pe.CustomIconUuid, pe, p) + str;

			if(bPassword)
			{
				if(p.CellInit == "<td>")
				{
					p = p.CloneShallow();
					p.CellInit = "<td class=\"f_password\">";
				}
				else { Debug.Assert(false); }
			}

			WriteTabularIf(bCondition, sb, str, p);
		}

		private static void WriteTabularIf(bool bCondition, StringBuilder sb,
			string strValueHtml, PfOptions p)
		{
			if(!bCondition) return;

			sb.Append(p.CellInit);
			sb.Append(p.ValueInit);

			if(strValueHtml.Length != 0) sb.Append(strValueHtml);
			else sb.Append(@"&nbsp;");

			sb.Append(p.ValueExit);
			sb.AppendLine(p.CellExit);
		}

		private static void WriteBlockLine(StringBuilder sb,
			KeyValuePair<string, ProtectedString> kvp, PfOptions p)
		{
			bool bUrl = (kvp.Key == KPRes.Url);
			bool bPassword = (kvp.Key == KPRes.Password);

			sb.Append("<tr><td>");
			sb.Append(StrUtil.StringToHtml(kvp.Key));
			sb.AppendLine(":</td>");

			sb.Append(bPassword ? "<td class=\"f_password\">" : "<td>");
			sb.Append(p.ValueInit);

			if(bUrl && !kvp.Value.IsEmpty)
				sb.Append(MakeUrlLink(kvp.Value.ReadString(), p));
			else
			{
				string strInner = CompileToHtml(kvp.Value.ReadString(), p, bPassword);
				if(strInner.Length == 0) strInner = "&nbsp;";
				sb.Append(strInner);
			}

			sb.Append(p.ValueExit);
			sb.AppendLine("</td></tr>");
		}

		private static void WriteBlockLine(StringBuilder sb, string strIndex,
			string strValue, PfOptions p)
		{
			if(string.IsNullOrEmpty(strValue)) return;

			KeyValuePair<string, ProtectedString> kvp = new KeyValuePair<string, ProtectedString>(
				strIndex, new ProtectedString(false, strValue));
			WriteBlockLine(sb, kvp, p);
		}

		private static string MakeIconImg(PwIcon i, PwUuid ci, ITimeLogger tl, PfOptions p)
		{
			if(p.ClientIcons == null) return string.Empty; // Optional

			if((tl != null) && tl.Expires && (tl.ExpiryTime <= p.Now))
			{
				i = PwIcon.Expired;
				ci = null;
			}

			Image img = null;
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

			return ("<img src=\"" + strData + "\"" + Environment.NewLine +
				"class=\"icon_cli\" alt=\"\" />&nbsp;");
		}

		private static string MakeUrlLink(string strRawUrl, PfOptions p)
		{
			if(string.IsNullOrEmpty(strRawUrl)) return string.Empty;

			string strCmp = CompileToHtml(strRawUrl, p, false);

			string strHRef = strCmp;
			if(p.SprMode == 2) // Use only Spr-compiled URL for HRef, not both
			{
				PfOptions pSub = p.CloneShallow();
				pSub.SprMode = 1;
				strHRef = CompileToHtml(strRawUrl, pSub, false);
			}
			// Do not Spr-compile URL for HRef when p.SprMode == 0, because
			// this could unexpectedly disclose external data

			return ("<a href=\"" + strHRef + "\">" + strCmp + "</a>");
		}

		private static void WriteGroupNotes(StringBuilder sb, PwGroup pg)
		{
			string str = pg.Notes.Trim();
			if(str.Length == 0) return;

			// No <p>...</p> due to padding/margin
			sb.Append("<table><tr><td>");
			sb.Append(StrUtil.StringToHtml(str));
			sb.AppendLine("</td></tr></table><br />");
		}

		private static string PasswordToHtml(string str, PfOptions p)
		{
			if(string.IsNullOrEmpty(str)) return string.Empty;

			if(UIUtil.ColorsEqual(p.ColorPU, Color.Empty))
				return StrUtil.StringToHtml(str); // White-space pre. via CSS

			List<KeyValuePair<int, UnicodeCategory>> lAll = StrUtil.GetCategoryGroups(str);

			List<KeyValuePair<int, UnicodeCategory>> l = new List<KeyValuePair<int, UnicodeCategory>>();
			UnicodeCategory ucLast = UnicodeCategory.UppercaseLetter;
			for(int i = 0; i < lAll.Count; ++i)
			{
				KeyValuePair<int, UnicodeCategory> kvp = lAll[i];

				UnicodeCategory uc = kvp.Value;
				if((uc != UnicodeCategory.UppercaseLetter) &&
					(uc != UnicodeCategory.LowercaseLetter) &&
					(uc != UnicodeCategory.DecimalDigitNumber))
					uc = UnicodeCategory.OtherNotAssigned;

				if((uc != ucLast) || (i == 0))
				{
					l.Add(new KeyValuePair<int, UnicodeCategory>(kvp.Key, uc));
					ucLast = uc;
				}
			}

			StringBuilder sb = new StringBuilder();

			for(int iGroup = 0; iGroup < l.Count; ++iGroup)
			{
				KeyValuePair<int, UnicodeCategory> kvp = l[iGroup];
				int iNext = (((iGroup + 1) < l.Count) ? l[iGroup + 1].Key : str.Length);
				string strGroup = str.Substring(kvp.Key, iNext - kvp.Key);

				sb.AppendLine("<span");
				sb.Append("class=\"f_password_");
				switch(kvp.Value)
				{
					case UnicodeCategory.UppercaseLetter: sb.Append('u'); break;
					case UnicodeCategory.LowercaseLetter: sb.Append('l'); break;
					case UnicodeCategory.DecimalDigitNumber: sb.Append('d'); break;
					default: sb.Append('o'); break;
				}
				sb.Append("\">");
				sb.Append(StrUtil.StringToHtml(strGroup)); // White-space pre. via CSS
				sb.Append("</span>");
			}

			return sb.ToString();
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
			Debug.Assert(m_uBlockUpdateUIState == 0);
			Debug.Assert(m_uBlockPreviewRefresh == 0);

			if(m_ilTabIcons != null)
			{
				m_tabMain.ImageList = null;
				m_ilTabIcons.Dispose();
				m_ilTabIcons = null;
			}
			else { Debug.Assert(false); }

			m_fcgMain.Dispose();
			m_fcgPassword.Dispose();

			GlobalWindowManager.RemoveWindow(this);
		}

		private void OnFormClosing(object sender, FormClosingEventArgs e)
		{
			if(m_uBlockPreviewRefresh != 0) e.Cancel = true;
		}

		private void OnIconCheckedChanged(object sender, EventArgs e)
		{
			UpdateUIState();
		}

		private void OnColorPCheckedChanged(object sender, EventArgs e)
		{
			UpdateUIState();
		}
	}
}
