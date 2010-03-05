/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2010 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using KeePass.App;
using KeePass.UI;
using KeePass.Resources;

using KeePassLib;
using KeePassLib.Security;
using KeePassLib.Delegates;
using KeePassLib.Resources;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class PrintForm : Form
	{
		private PwGroup m_pgDataSource = null;
		private bool m_bPrintMode = true;
		private int m_nDefaultSortColumn = -1;

		private bool m_bBlockPreviewRefresh = false;
		private string m_strGeneratedHtml = string.Empty;

		public string GeneratedHtml
		{
			get { return m_strGeneratedHtml; }
		}

		public void InitEx(PwGroup pgDataSource, bool bPrintMode, int nDefaultSortColumn)
		{
			Debug.Assert(pgDataSource != null); if(pgDataSource == null) throw new ArgumentNullException("pgDataSource");

			m_pgDataSource = pgDataSource;
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

			m_bannerImage.Image = BannerFactory.CreateBanner(m_bannerImage.Width,
				m_bannerImage.Height, BannerStyle.Default,
				Properties.Resources.B48x48_FilePrint, strTitle,
				strDesc);

			this.Text = strTitle;
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			Debug.Assert(m_pgDataSource != null); if(m_pgDataSource == null) throw new ArgumentException();

			GlobalWindowManager.AddWindow(this);

			this.Icon = Properties.Resources.KeePass;
			CreateDialogBanner();

			UIUtil.AssignFontDefaultBold(m_rbTabular);
			UIUtil.AssignFontDefaultBold(m_rbDetails);

			if(!m_bPrintMode) m_btnOK.Text = KPRes.Export;

			m_bBlockPreviewRefresh = true;
			m_rbTabular.Checked = true;

			m_cmbSortEntries.Items.Add("(" + KPRes.None + ")");
			m_cmbSortEntries.Items.Add(KPRes.Title);
			m_cmbSortEntries.Items.Add(KPRes.UserName);
			m_cmbSortEntries.Items.Add(KPRes.Password);
			m_cmbSortEntries.Items.Add(KPRes.Url);
			m_cmbSortEntries.Items.Add(KPRes.Notes);
			int nSortSel = 0;
			if(m_nDefaultSortColumn == (int)AppDefs.ColumnId.Title) nSortSel = 1;
			else if(m_nDefaultSortColumn == (int)AppDefs.ColumnId.UserName) nSortSel = 2;
			else if(m_nDefaultSortColumn == (int)AppDefs.ColumnId.Password) nSortSel = 3;
			else if(m_nDefaultSortColumn == (int)AppDefs.ColumnId.Url) nSortSel = 4;
			else if(m_nDefaultSortColumn == (int)AppDefs.ColumnId.Notes) nSortSel = 5;
			m_cmbSortEntries.SelectedIndex = nSortSel;
			m_bBlockPreviewRefresh = false;

			if(!m_bPrintMode) // Export to HTML
			{
				m_btnConfigPrinter.Visible = m_btnPrintPreview.Visible = false;
				m_lblPreviewHint.Visible = false;
			}

			UpdateHtmlDocument();
			UpdateUIState();
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			UpdateHtmlDocument();

			if(m_bPrintMode)
			{
				try { m_wbMain.Print(); } // Throws in Mono 1.2.6+
				catch(NotImplementedException)
				{
					MessageService.ShowWarning(KLRes.FrameworkNotImplExcp);
				}
				catch(Exception ex) { MessageService.ShowWarning(ex); }
			}
			else m_strGeneratedHtml = m_wbMain.DocumentText;

			if(m_strGeneratedHtml == null) m_strGeneratedHtml = string.Empty;
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
		}

		private void UpdateUIState()
		{
			m_cbAutoType.Enabled = m_cbCustomStrings.Enabled = m_rbDetails.Checked;

			if(m_rbTabular.Checked)
			{
				m_cbAutoType.Checked = false;
				m_cbCustomStrings.Checked = false;
			}
		}

		private void UpdateHtmlDocument()
		{
			if(m_bBlockPreviewRefresh) return;

			m_bBlockPreviewRefresh = true;

			PwGroup pgDataSource = m_pgDataSource.CloneDeep();

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

			StringBuilder sb = new StringBuilder();

			sb.AppendLine("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\"");
			sb.AppendLine("\t\"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");

			sb.AppendLine("<html xmlns=\"http://www.w3.org/1999/xhtml\">");
			sb.AppendLine("<head>");
			sb.AppendLine("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\" />");
			sb.Append("<title>");
			sb.Append(StrUtil.StringToHtml(pgDataSource.Name));
			sb.AppendLine("</title>");
			sb.AppendLine("<meta http-equiv=\"expires\" content=\"0\" />");
			sb.AppendLine("<meta http-equiv=\"cache-control\" content=\"no-cache\" />");
			sb.AppendLine("<meta http-equiv=\"pragma\" content=\"no-cache\" />");

			sb.AppendLine("<style type=\"text/css\"><!--");

			sb.AppendLine("body, p, div, h1, h2, h3, h4, h5, h6, ol, ul, li, td, th, dd, dt, a {");
			sb.AppendLine("\tfont-family: Tahoma, MS Sans Serif, Sans Serif, Verdana, sans-serif;");
			sb.AppendLine("}");
			sb.AppendLine("span.fserif {");
			sb.AppendLine("\tfont-family: Times New Roman, serif;");
			sb.AppendLine("}");
			sb.AppendLine("td {");
			sb.AppendLine("\ttext-align: left;");
			sb.AppendLine("\tvertical-align: top;");
			sb.AppendLine("}");

			sb.AppendLine("--></style>");

			sb.AppendLine("</head><body>");

			sb.AppendLine("<h2>" + StrUtil.StringToHtml(pgDataSource.Name) + "</h2>");

			GroupHandler gh = null;
			EntryHandler eh = null;

			bool bGroup = m_cbGroups.Checked;
			bool bTitle = m_cbTitle.Checked, bUserName = m_cbUser.Checked;
			bool bPassword = m_cbPassword.Checked, bURL = m_cbUrl.Checked;
			bool bNotes = m_cbNotes.Checked;
			bool bCreation = m_cbCreation.Checked, bLastMod = m_cbLastMod.Checked;
			bool bLastAcc = m_cbLastAccess.Checked, bExpire = m_cbExpire.Checked;
			bool bAutoType = m_cbAutoType.Checked;
			bool bCustomStrings = m_cbCustomStrings.Checked;

			bool bMonoPasswords = m_cbMonospaceForPasswords.Checked;
			if(m_rbMonospace.Checked) bMonoPasswords = false; // Monospaced anyway

			bool bSmallMono = m_cbSmallMono.Checked;

			string strFontInit = string.Empty, strFontExit = string.Empty;

			if(m_rbSerif.Checked)
			{
				strFontInit = "<span class=\"fserif\"><small>";
				strFontExit = "</small></span>";
			}
			else if(m_rbSansSerif.Checked)
			{
				strFontInit = "<small>";
				strFontExit = "</small>";
			}
			else if(m_rbMonospace.Checked)
			{
				strFontInit = (bSmallMono ? "<code><small>" : "<code>");
				strFontExit = (bSmallMono ? "</small></code>" : "</code>");
			}
			else { Debug.Assert(false); }

			string strTableInit = "<table width=\"100%\">";

			if(m_rbTabular.Checked)
			{
				// int nFieldCount = (bTitle ? 1 : 0) + (bUserName ? 1 : 0);
				// nFieldCount += (bPassword ? 1 : 0) + (bURL ? 1 : 0) + (bNotes ? 1 : 0);

				string strCellPre = "<td>" + strFontInit;
				string strCellPost = strFontExit + "</td>";

				sb.AppendLine(strTableInit);

				string strHTdInit = "<td><b><small>";
				string strHTdExit = "</small></b></td>";

				StringBuilder sbH = new StringBuilder();
				sbH.Append("<tr>");
				if(bGroup) sbH.AppendLine(strHTdInit + StrUtil.StringToHtml(KPRes.Group) + strHTdExit);
				if(bTitle) sbH.AppendLine(strHTdInit + StrUtil.StringToHtml(KPRes.Title) + strHTdExit);
				if(bUserName) sbH.AppendLine(strHTdInit + StrUtil.StringToHtml(KPRes.UserName) + strHTdExit);
				if(bPassword) sbH.AppendLine(strHTdInit + StrUtil.StringToHtml(KPRes.Password) + strHTdExit);
				if(bURL) sbH.AppendLine(strHTdInit + StrUtil.StringToHtml(KPRes.Url) + strHTdExit);
				if(bNotes) sbH.AppendLine(strHTdInit + StrUtil.StringToHtml(KPRes.Notes) + strHTdExit);
				if(bCreation) sbH.AppendLine(strHTdInit + StrUtil.StringToHtml(KPRes.CreationTime) + strHTdExit);
				if(bLastAcc) sbH.AppendLine(strHTdInit + StrUtil.StringToHtml(KPRes.LastAccessTime) + strHTdExit);
				if(bLastMod) sbH.AppendLine(strHTdInit + StrUtil.StringToHtml(KPRes.LastModificationTime) + strHTdExit);
				if(bExpire) sbH.AppendLine(strHTdInit + StrUtil.StringToHtml(KPRes.ExpiryTime) + strHTdExit);
				sbH.AppendLine("</tr>");

				string strH = sbH.ToString();
				sb.Append(strH);
				strTableInit += strH;

				eh = delegate(PwEntry pe)
				{
					sb.AppendLine("<tr>");

					WriteTabularIf(bGroup, sb, pe.ParentGroup.Name, strCellPre, strCellPost);
					WriteTabularIf(bTitle, sb, pe, PwDefs.TitleField, strCellPre, strCellPost);
					WriteTabularIf(bUserName, sb, pe, PwDefs.UserNameField, strCellPre, strCellPost);

					if(bPassword)
					{
						if(bMonoPasswords)
							sb.Append(bSmallMono ? "<td><code><small>" : "<td><code>");
						else sb.Append(strCellPre);

						string strInner = StrUtil.StringToHtml(pe.Strings.ReadSafe(PwDefs.PasswordField));
						if(strInner.Length > 0) sb.Append(strInner);
						else sb.Append(@"&nbsp;");

						if(bMonoPasswords)
							sb.AppendLine(bSmallMono ? "</small></code></td>" : "</code></td>");
						else sb.AppendLine(strCellPost);
					}

					WriteTabularIf(bURL, sb, pe, PwDefs.UrlField, strCellPre, strCellPost);
					WriteTabularIf(bNotes, sb, pe, PwDefs.NotesField, strCellPre, strCellPost);

					WriteTabularIf(bCreation, sb, pe.CreationTime.ToString(), strCellPre, strCellPost);
					WriteTabularIf(bLastAcc, sb, pe.LastAccessTime.ToString(), strCellPre, strCellPost);
					WriteTabularIf(bLastMod, sb, pe.LastModificationTime.ToString(), strCellPre, strCellPost);
					WriteTabularIf(bExpire, sb, pe.ExpiryTime.ToString(), strCellPre, strCellPost);

					sb.AppendLine("</tr>");
					return true;
				};
			}
			else if(m_rbDetails.Checked)
			{
				sb.AppendLine(strTableInit);

				eh = delegate(PwEntry pe)
				{
					if(bGroup) WriteDetailsLine(sb, KPRes.Group, pe.ParentGroup.Name, bSmallMono, bMonoPasswords, strFontInit, strFontExit);
					if(bTitle) WriteDetailsLine(sb, KPRes.Title, pe.Strings.ReadSafe(PwDefs.TitleField), bSmallMono, bMonoPasswords, strFontInit + "<b>", "</b>" + strFontExit);
					if(bUserName) WriteDetailsLine(sb, KPRes.UserName, pe.Strings.ReadSafe(PwDefs.UserNameField), bSmallMono, bMonoPasswords, strFontInit, strFontExit);
					if(bPassword) WriteDetailsLine(sb, KPRes.Password, pe.Strings.ReadSafe(PwDefs.PasswordField), bSmallMono, bMonoPasswords, strFontInit, strFontExit);
					if(bURL) WriteDetailsLine(sb, KPRes.Url, pe.Strings.ReadSafe(PwDefs.UrlField), bSmallMono, bMonoPasswords, strFontInit, strFontExit);
					if(bNotes) WriteDetailsLine(sb, KPRes.Notes, pe.Strings.ReadSafe(PwDefs.NotesField), bSmallMono, bMonoPasswords, strFontInit, strFontExit);
					if(bCreation) WriteDetailsLine(sb, KPRes.CreationTime, pe.CreationTime.ToString(), bSmallMono, bMonoPasswords, strFontInit, strFontExit);
					if(bLastAcc) WriteDetailsLine(sb, KPRes.LastAccessTime, pe.LastAccessTime.ToString(), bSmallMono, bMonoPasswords, strFontInit, strFontExit);
					if(bLastMod) WriteDetailsLine(sb, KPRes.LastModificationTime, pe.LastModificationTime.ToString(), bSmallMono, bMonoPasswords, strFontInit, strFontExit);
					if(bExpire) WriteDetailsLine(sb, KPRes.ExpiryTime, pe.ExpiryTime.ToString(), bSmallMono, bMonoPasswords, strFontInit, strFontExit);

					if(bAutoType)
					{
						foreach(KeyValuePair<string, string> kvp in pe.AutoType.WindowSequencePairs)
							WriteDetailsLine(sb, KPRes.AutoType, kvp.Key + ": " + kvp.Value, bSmallMono, bMonoPasswords, strFontInit, strFontExit);
					}

					foreach(KeyValuePair<string, ProtectedString> kvp in pe.Strings)
					{
						if(bCustomStrings && !PwDefs.IsStandardField(kvp.Key))
							WriteDetailsLine(sb, kvp, bSmallMono, bMonoPasswords, strFontInit, strFontExit);
					}

					sb.AppendLine(@"<tr><td>&nbsp;</td></tr>");

					return true;
				};
			}
			else { Debug.Assert(false); }

			gh = delegate(PwGroup pg)
			{
				if(pg.Entries.UCount == 0) return true;

				sb.Append("</table><br /><hr /><h3>");
				sb.Append(StrUtil.StringToHtml(pg.GetFullPath(" - ", false)));
				sb.AppendLine("</h3>");
				sb.AppendLine(strTableInit);

				return true;
			};

			pgDataSource.TraverseTree(TraversalMethod.PreOrder, gh, eh);

			if(m_rbTabular.Checked)
				sb.AppendLine("</table>");
			else if(m_rbDetails.Checked)
				sb.AppendLine("</table><br />");

			sb.AppendLine("</body></html>");

			try { UIUtil.SetWebBrowserDocument(m_wbMain, sb.ToString()); }
			catch(Exception) { } // Throws in Mono 2.0+

			m_bBlockPreviewRefresh = false;
		}

		private static void WriteTabularIf(bool bCondition, StringBuilder sb,
			PwEntry pe, string strIndex, string strCellPre, string strCellPost)
		{
			if(!bCondition) return;

			sb.Append(strCellPre);

			string strInner = StrUtil.StringToHtml(pe.Strings.ReadSafe(strIndex));
			if(strInner.Length > 0) sb.Append(strInner);
			else sb.Append(@"&nbsp;");

			sb.AppendLine(strCellPost);
		}

		private static void WriteTabularIf(bool bCondition, StringBuilder sb,
			string strValue, string strCellPre, string strCellPost)
		{
			if(!bCondition) return;

			sb.Append(strCellPre);

			if(strValue.Length > 0) sb.Append(strValue);
			else sb.Append(@"&nbsp;");

			sb.AppendLine(strCellPost);
		}

		private static void WriteDetailsLine(StringBuilder sb,
			KeyValuePair<string, ProtectedString> kvp, bool bSmallMono,
			bool bMonoPasswords, string strFontInit, string strFontExit)
		{
			sb.Append("<tr><td><i><small>");
			sb.Append(StrUtil.StringToHtml(kvp.Key));
			sb.AppendLine(":</small></i></td>");

			sb.Append("<td>");

			if(bMonoPasswords && (kvp.Key == PwDefs.PasswordField))
				sb.Append(bSmallMono ? "<code><small>" : "<code>");
			else sb.Append(strFontInit);

			string strInner = StrUtil.StringToHtml(kvp.Value.ReadString());
			if(strInner.Length > 0) sb.Append(strInner);
			else sb.Append("&nbsp;");

			if(kvp.Key == PwDefs.PasswordField)
				sb.Append(bSmallMono ? "</small></code>" : "</code>");
			else sb.Append(strFontExit);

			sb.AppendLine("</td></tr>");
		}

		private static void WriteDetailsLine(StringBuilder sb, string strIndex,
			string strValue, bool bSmallMono, bool bMonoPasswords, string strFontInit,
			string strFontExit)
		{
			KeyValuePair<string, ProtectedString> kvp = new KeyValuePair<string, ProtectedString>(strIndex,
				new ProtectedString(false, strValue));

			WriteDetailsLine(sb, kvp, bSmallMono, bMonoPasswords, strFontInit, strFontExit);
		}

		private void SortGroupEntriesRecursive(PwGroup pg, string strFieldName)
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
			UpdateHtmlDocument();

			try { m_wbMain.ShowPageSetupDialog(); } // Throws in Mono 1.2.6+
			catch(NotImplementedException)
			{
				MessageService.ShowWarning(KLRes.FrameworkNotImplExcp);
			}
			catch(Exception ex) { MessageService.ShowWarning(ex); }
		}

		private void OnBtnPrintPreview(object sender, EventArgs e)
		{
			UpdateHtmlDocument();

			try { m_wbMain.ShowPrintPreviewDialog(); } // Throws in Mono 1.2.6+
			catch(NotImplementedException)
			{
				MessageService.ShowWarning(KLRes.FrameworkNotImplExcp);
			}
			catch(Exception ex) { MessageService.ShowWarning(ex); }
		}

		private void OnTabSelectedIndexChanged(object sender, EventArgs e)
		{
			if(m_tabMain.SelectedIndex == 0) UpdateHtmlDocument();
		}

		private void OnTabularCheckedChanged(object sender, EventArgs e)
		{
			UpdateUIState();
		}

		private void OnLinkSelectAllFields(object sender, LinkLabelLinkClickedEventArgs e)
		{
			m_cbTitle.Checked = m_cbUser.Checked = m_cbPassword.Checked =
				m_cbUrl.Checked = m_cbNotes.Checked = m_cbCreation.Checked =
				m_cbLastAccess.Checked = m_cbLastMod.Checked = m_cbExpire.Checked =
				m_cbAutoType.Checked = m_cbGroups.Checked = m_cbCustomStrings.Checked = true;
			UpdateUIState();
		}

		private void OnLinkDeselectAllFields(object sender, LinkLabelLinkClickedEventArgs e)
		{
			m_cbTitle.Checked = m_cbUser.Checked = m_cbPassword.Checked =
				m_cbUrl.Checked = m_cbNotes.Checked = m_cbCreation.Checked =
				m_cbLastAccess.Checked = m_cbLastMod.Checked = m_cbExpire.Checked =
				m_cbAutoType.Checked = m_cbGroups.Checked = m_cbCustomStrings.Checked = false;
			UpdateUIState();
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			GlobalWindowManager.RemoveWindow(this);
		}
	}
}