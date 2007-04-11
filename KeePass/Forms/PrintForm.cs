/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2007 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePass.UI;
using KeePass.Resources;

using KeePassLib;
using KeePassLib.Security;
using KeePassLib.Delegates;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class PrintForm : Form
	{
		private PwGroup m_pgDataSource = null;
		private bool m_bPrintMode = true;

		private bool m_bBlockPreviewRefresh = false;
		private string m_strGeneratedHTML = string.Empty;

		public string GeneratedHTML
		{
			get { return m_strGeneratedHTML; }
		}

		public void InitEx(PwGroup pgDataSource, bool bPrintMode)
		{
			Debug.Assert(pgDataSource != null); if(pgDataSource == null) throw new ArgumentNullException();

			m_pgDataSource = pgDataSource;
			m_bPrintMode = bPrintMode;
		}

		public PrintForm()
		{
			InitializeComponent();
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
				strTitle = KPRes.ExportHTML;
				strDesc = KPRes.ExportHTMLDesc;
			}

			m_bannerImage.Image = BannerFactory.CreateBanner(m_bannerImage.Width,
				m_bannerImage.Height, BannerFactory.BannerStyle.Default,
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

			m_bBlockPreviewRefresh = true;
			m_rbTabular.Checked = true;
			m_bBlockPreviewRefresh = false;

			if(!m_bPrintMode) // Export to HTML
			{
				m_btnConfigPrinter.Visible = m_btnPrintPreview.Visible = false;
				m_lblPreviewHint.Visible = false;
			}

			UpdateHTMLDocument();
			UpdateUIState();
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			if(m_bPrintMode) m_wbMain.Print();
			else m_strGeneratedHTML = m_wbMain.DocumentText;

			if(m_strGeneratedHTML == null) m_strGeneratedHTML = string.Empty;
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
		}

		private void UpdateUIState()
		{
			m_cbAutoType.Enabled = m_rbDetails.Checked;

			if(m_rbTabular.Checked) m_cbAutoType.Checked = false;
		}

		private void UpdateHTMLDocument()
		{
			if(m_bBlockPreviewRefresh) return;

			m_bBlockPreviewRefresh = true;

			StringBuilder sb = new StringBuilder();

			sb.Append("<html><head><meta><title>");
			sb.Append(m_pgDataSource.Name);
			sb.Append("</title></meta></head><body>");

			sb.Append("<h2>");
			sb.Append(m_pgDataSource.Name);
			sb.Append("</h2>");

			GroupHandler gh = null;
			EntryHandler eh = null;

			bool bGroup = m_cbGroups.Checked;
			bool bTitle = m_cbTitle.Checked, bUserName = m_cbUser.Checked;
			bool bPassword = m_cbPassword.Checked, bURL = m_cbUrl.Checked;
			bool bNotes = m_cbNotes.Checked;
			bool bCreation = m_cbCreation.Checked, bLastMod = m_cbLastMod.Checked;
			bool bLastAcc = m_cbLastAccess.Checked, bExpire = m_cbExpire.Checked;
			bool bAutoType = m_cbAutoType.Checked;

			bool bMonoPasswords = m_cbMonospaceForPasswords.Checked;
			if(m_rbMonospace.Checked) bMonoPasswords = false; // Monospaced anyway

			bool bSmallMono = m_cbSmallMono.Checked;

			string strFontInit = string.Empty, strFontExit = string.Empty;

			if(m_rbSerif.Checked)
			{
				strFontInit = "<font face=\"Times New Roman,Serif\"><small>";
				strFontExit = "</small></font>";
			}
			else if(m_rbSansSerif.Checked)
			{
				strFontInit = "<font face=\"Tahoma,MS Sans Serif,Sans Serif,Verdana\"><small>";
				strFontExit = "</small></font>";
			}
			else if(m_rbMonospace.Checked)
			{
				strFontInit = "<code>";
				if(bSmallMono) strFontInit += "<small>";

				strFontExit = (bSmallMono ? @"</small></code>" : @"</code>");
			}
			else { Debug.Assert(false); }

			if(m_rbTabular.Checked)
			{
				// int nFieldCount = (bTitle ? 1 : 0) + (bUserName ? 1 : 0);
				// nFieldCount += (bPassword ? 1 : 0) + (bURL ? 1 : 0) + (bNotes ? 1 : 0);

				string strCellPre = "<td align=\"left\" valign=\"top\">";
				string strCellPost = "</td>";

				strCellPre += strFontInit;
				strCellPost = strFontExit + strCellPost;

				sb.AppendLine("<table width=\"100%\">");

				sb.Append("<tr>");
				if(bGroup) sb.Append("<td><b><small>" + KPRes.Group + "</small></b></td>");
				if(bTitle) sb.Append("<td><b><small>" + KPRes.Title + "</small></b></td>");
				if(bUserName) sb.Append("<td><b><small>" + KPRes.UserName + "</small></b></td>");
				if(bPassword) sb.Append("<td><b><small>" + KPRes.Password + "</small></b></td>");
				if(bURL) sb.Append("<td><b><small>" + KPRes.URL + "</small></b></td>");
				if(bNotes) sb.Append("<td><b><small>" + KPRes.Notes + "</small></b></td>");
				if(bCreation) sb.Append("<td><b><small>" + KPRes.CreationTime + "</small></b></td>");
				if(bLastAcc) sb.Append("<td><b><small>" + KPRes.LastAccessTime + "</small></b></td>");
				if(bLastMod) sb.Append("<td><b><small>" + KPRes.LastModificationTime + "</small></b></td>");
				if(bExpire) sb.Append("<td><b><small>" + KPRes.ExpiryTime + "</small></b></td>");
				sb.AppendLine("</tr>");

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

					sb.Append(strCellPost);
					sb.AppendLine("</tr>");
					return true;
				};
			}
			else if(m_rbDetails.Checked)
			{
				sb.AppendLine("<table width=\"100%\">");

				eh = delegate(PwEntry pe)
				{
					if(bGroup) WriteDetailsLine(sb, KPRes.Group, pe.ParentGroup.Name, bSmallMono, bMonoPasswords, strFontInit, strFontExit);
					if(bTitle) WriteDetailsLine(sb, KPRes.Title, pe.Strings.ReadSafe(PwDefs.TitleField), bSmallMono, bMonoPasswords, strFontInit, strFontExit);
					if(bUserName) WriteDetailsLine(sb, KPRes.UserName, pe.Strings.ReadSafe(PwDefs.UserNameField), bSmallMono, bMonoPasswords, strFontInit, strFontExit);
					if(bPassword) WriteDetailsLine(sb, KPRes.Password, pe.Strings.ReadSafe(PwDefs.PasswordField), bSmallMono, bMonoPasswords, strFontInit, strFontExit);
					if(bURL) WriteDetailsLine(sb, KPRes.URL, pe.Strings.ReadSafe(PwDefs.UrlField), bSmallMono, bMonoPasswords, strFontInit, strFontExit);
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
						if((kvp.Key != PwDefs.TitleField) && (kvp.Key != PwDefs.UserNameField) &&
							(kvp.Key != PwDefs.PasswordField) && (kvp.Key != PwDefs.UrlField) &&
							(kvp.Key != PwDefs.NotesField))
						{
							WriteDetailsLine(sb, kvp, bSmallMono, bMonoPasswords, strFontInit, strFontExit);
						}
					}

					sb.AppendLine("<tr><td>&nbsp;</td></tr>");

					return true;
				};
			}
			else { Debug.Assert(false); }

			m_pgDataSource.TraverseTree(TraversalMethod.PreOrder, gh, eh);

			if(m_rbTabular.Checked)
			{
				sb.AppendLine("</table>");
			}
			else if(m_rbDetails.Checked)
			{
				sb.AppendLine("</table><br />");
			}

			sb.AppendLine("</body></html>");

			m_wbMain.AllowNavigation = true;
			m_wbMain.DocumentText = sb.ToString();

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
			sb.Append("<tr><td align=\"left\" valign=\"top\"><i><small>");
			sb.Append(StrUtil.StringToHtml(kvp.Key));
			sb.AppendLine(":</small></i></td>");

			sb.Append("<td align=\"left\" valign=\"top\">");

			if(bMonoPasswords && (kvp.Key == PwDefs.PasswordField))
				sb.Append(bSmallMono ? "<code><small>" : "<code>");
			else sb.Append(strFontInit);

			sb.Append(StrUtil.StringToHtml(kvp.Value.ReadString()));

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

		private void OnBtnConfigPage(object sender, EventArgs e)
		{
			m_wbMain.ShowPageSetupDialog();
			UpdateHTMLDocument();
		}

		private void OnBtnPrintPreview(object sender, EventArgs e)
		{
			m_wbMain.ShowPrintPreviewDialog();
			UpdateHTMLDocument();
		}

		private void OnTabSelectedIndexChanged(object sender, EventArgs e)
		{
			if(m_tabMain.SelectedIndex == 0) UpdateHTMLDocument();
		}

		private void OnTabularCheckedChanged(object sender, EventArgs e)
		{
			UpdateUIState();
		}

		private void OnLinkSelectAllFields(object sender, LinkLabelLinkClickedEventArgs e)
		{
			m_cbTitle.Checked = m_cbUser.Checked = m_cbPassword.Checked =
				m_cbUrl.Checked = m_cbNotes.Checked =
				m_cbCreation.Checked = m_cbLastAccess.Checked = m_cbLastMod.Checked =
				m_cbExpire.Checked = m_cbAutoType.Checked = m_cbGroups.Checked = true;
			UpdateUIState();
		}

		private void OnLinkDeselectAllFields(object sender, LinkLabelLinkClickedEventArgs e)
		{
			m_cbTitle.Checked = m_cbUser.Checked = m_cbPassword.Checked =
				m_cbUrl.Checked = m_cbNotes.Checked =
				m_cbCreation.Checked = m_cbLastAccess.Checked = m_cbLastMod.Checked =
				m_cbExpire.Checked = m_cbAutoType.Checked = m_cbGroups.Checked = false;
			UpdateUIState();
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			GlobalWindowManager.RemoveWindow(this);
		}
	}
}