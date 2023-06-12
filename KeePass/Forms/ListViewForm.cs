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
using System.IO;
using System.Text;
using System.Windows.Forms;

using KeePass.App;
using KeePass.App.Configuration;
using KeePass.DataExchange;
using KeePass.DataExchange.Formats;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Delegates;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class ListViewForm : Form
	{
		private string m_strTitle = string.Empty;
		private string m_strSubTitle = string.Empty;
		private string m_strInfo = string.Empty;
		private Image m_imgIcon = null;
		private List<object> m_lData = new List<object>();
		private ImageList m_ilIcons = null;
		private Action<ListView> m_fInit = null;

		private bool[] m_vVisible = MemUtil.EmptyArray<bool>();

		private object m_resItem = null;
		public object ResultItem
		{
			get { return m_resItem; }
		}

		private object m_resGroup = null;
		public object ResultGroup
		{
			get { return m_resGroup; }
		}

		private bool m_bEnsureForeground = false;
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DefaultValue(false)]
		public bool EnsureForeground
		{
			get { return m_bEnsureForeground; }
			set { m_bEnsureForeground = value; }
		}

		private const LvfFlags DefaultLvfFlags = LvfFlags.Print | LvfFlags.Export |
			LvfFlags.Filter;
		private LvfFlags m_fl = DefaultLvfFlags;
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DefaultValue(DefaultLvfFlags)]
		public LvfFlags FlagsEx
		{
			get { return m_fl; }
			set { m_fl = value; }
		}

		private PwDatabase m_pd = null;
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DefaultValue((object)null)]
		public PwDatabase DatabaseEx
		{
			get { return m_pd; }
			set { Debug.Assert((value == null) || value.IsOpen); m_pd = value; }
		}

		public void InitEx(string strTitle, string strSubTitle,
			string strInfo, Image imgIcon, List<object> lData,
			ImageList ilIcons, Action<ListView> fInit)
		{
			m_strTitle = (strTitle ?? string.Empty);
			m_strSubTitle = (strSubTitle ?? string.Empty);
			m_strInfo = (strInfo ?? string.Empty);
			m_imgIcon = imgIcon;

			Debug.Assert((m_lData.Count == 0) && (lData != null));
			m_lData = (lData ?? new List<object>());

			Debug.Assert(m_ilIcons == null);
			m_ilIcons = ((ilIcons != null) ? UIUtil.CloneImageList(ilIcons, true) : null);

			m_fInit = fInit;

			m_vVisible = new bool[m_lData.Count];
		}

		public ListViewForm()
		{
			InitializeComponent();
			GlobalWindowManager.InitializeForm(this);
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			// Adjust controls before GlobalWindowManager.AddWindow
			// for better compatibility with plugins
			Debug.Assert(!m_lblInfo.AutoSize); // For RTL support
			m_lblInfo.Text = m_strInfo;
			if(m_strInfo.Length == 0)
			{
				int dY = m_lvMain.Top - m_lblInfo.Top;

				m_lblInfo.Visible = false;

				Rectangle r = m_lvMain.Bounds;
				m_lvMain.SetBounds(r.X, r.Y - dY, r.Width, r.Height + dY,
					BoundsSpecified.Y | BoundsSpecified.Height);
			}

			GlobalWindowManager.AddWindow(this);

			this.Text = m_strTitle;
			this.Icon = AppIcons.Default;

			BannerFactory.CreateBannerEx(this, m_bannerImage,
				m_imgIcon, m_strTitle, m_strSubTitle);

			Debug.Assert(!m_tstFilter.AcceptsReturn); // Ignored by form

			AccessKeyManagerEx akTB = new AccessKeyManagerEx();
			m_tsbPrint.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
			m_tsbPrint.Text = akTB.CreateText(KPRes.Print + "...", true);
			m_tsbExport.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
			m_tsbExport.Text = akTB.CreateText(KPRes.Export + "...", true);
			m_tslFilter.Text = akTB.CreateText(KPRes.Filter, true) + ":";
			UIUtil.SetCueBanner(m_tstFilter, "(" + KPRes.None + ")");

			AccessKeyManagerEx akExp = new AccessKeyManagerEx();

			ToolStripMenuItem tsmiExpCsv = new ToolStripMenuItem(
				akExp.CreateText("CSV...", true), Properties.Resources.B16x16_ASCII);
			tsmiExpCsv.Click += delegate(object senderD, EventArgs eD)
			{
				PerformExport("CSV");
			};
			m_tsbExport.DropDownItems.Add(tsmiExpCsv);

			ToolStripMenuItem tsmiExpHtml = new ToolStripMenuItem(
				akExp.CreateText("HTML...", true), Properties.Resources.B16x16_HTML);
			tsmiExpHtml.Click += delegate(object senderD, EventArgs eD)
			{
				PerformExport("HTML");
			};
			m_tsbExport.DropDownItems.Add(tsmiExpHtml);

			m_tsbPrint.Enabled = ((m_fl & LvfFlags.Print) != LvfFlags.None);
			UIUtil.SetEnabledFast(((m_fl & LvfFlags.Export) != LvfFlags.None),
				m_tsbExport, tsmiExpCsv, tsmiExpHtml);
			UIUtil.SetEnabledFast(((m_fl & LvfFlags.Filter) != LvfFlags.None),
				m_tslFilter, m_tstFilter);

			bool bHasSensitive = false;
			foreach(object o in m_lData)
			{
				LvfItem lvfi = (o as LvfItem);
				if(lvfi == null) continue;

				for(int i = lvfi.SubItems.Length - 1; i >= 0; --i)
				{
					if((lvfi.SubItems[i].Flags & LvfiFlags.Sensitive) != LvfiFlags.None)
					{
						bHasSensitive = true;
						break;
					}
				}

				if(bHasSensitive) break;
			}

			m_tsbHideSensitive.Text = SecureTextBoxEx.GetPasswordCharString(3);
			m_tsbHideSensitive.ToolTipText = KPRes.TogglePasswordAsterisks;
			m_tsbHideSensitive.Checked = (bHasSensitive &&
				(!AppPolicy.Current.UnhidePasswords ||
				Program.Config.MainWindow.IsColumnHidden(AceColumnType.Password)));
			m_tsbHideSensitive.CheckedChanged += delegate(object senderD, EventArgs eD)
			{
				if(!m_tsbHideSensitive.Checked && !AppPolicy.Try(AppPolicyId.UnhidePasswords))
					m_tsbHideSensitive.Checked = true;
				else UpdateListViewEx(true);
			};
			if(!bHasSensitive)
			{
				m_tsbHideSensitive.Enabled = false;
				m_tsbHideSensitive.Visible = false;
				m_tssSep1.Visible = false;
			}

			// Check all callers when changing defaults
			Debug.Assert(m_lvMain.Activation == ItemActivation.OneClick);
			Debug.Assert(m_lvMain.HotTracking);
			Debug.Assert(m_lvMain.HoverSelection);

			if((m_fl & LvfFlags.StandardTheme) != LvfFlags.None)
			{
				if(UISystemFonts.ListFont != null)
					m_lvMain.Font = UISystemFonts.ListFont;
			}
			else UIUtil.SetExplorerTheme(m_lvMain, true);

			if(m_ilIcons != null) m_lvMain.SmallImageList = m_ilIcons;

			if(m_fInit != null)
			{
				try { m_fInit(m_lvMain); }
				catch(Exception) { Debug.Assert(false); }
			}
			else { Debug.Assert(false); }

			UpdateListViewEx(false);

			if(m_bEnsureForeground)
			{
				this.BringToFront();
				this.Activate();
			}
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			Debug.Assert(m_lvMain.SmallImageList == m_ilIcons);
			if(m_ilIcons != null)
			{
				m_lvMain.SmallImageList = null; // Detach event handlers
				m_ilIcons.Dispose();
				m_ilIcons = null;
			}

			GlobalWindowManager.RemoveWindow(this);
		}

		private void UpdateListViewEx(bool bRestoreView)
		{
			bool bHideSensitive = m_tsbHideSensitive.Checked;

			string strFilter = m_tstFilter.Text;
			if((strFilter != null) && (strFilter.Length == 0)) strFilter = null;

			const StringComparison sc = StringComparison.InvariantCultureIgnoreCase;

			if(m_vVisible.Length != m_lData.Count)
			{
				Debug.Assert(false);
				m_vVisible = new bool[m_lData.Count];
			}
			Array.Clear(m_vVisible, 0, m_vVisible.Length);

			int iTop = (bRestoreView ? UIUtil.GetTopVisibleItem(m_lvMain) : -1);
			ListViewItem lviTop = ((iTop >= 0) ? m_lvMain.Items[iTop] : null);
			UIScrollInfo uisi = UIUtil.GetScrollInfo(m_lvMain, true);

			m_lvMain.BeginUpdateEx();
			m_lvMain.Items.Clear();
			m_lvMain.Groups.Clear();

			ListViewGroup lvgCur = null;
			for(int i = 0; i < m_lData.Count; ++i)
			{
				object o = m_lData[i];
				if(o == null) { Debug.Assert(false); continue; }

				LvfItem lvfi = (o as LvfItem);
				if(lvfi != null) lvfi.UpdateListViewItem(bHideSensitive);

				ListViewItem lvi = ((lvfi != null) ? lvfi.ListViewItem :
					(o as ListViewItem));
				if(lvi != null)
				{
					// Caller should not care about associations
					Debug.Assert(lvi.ListView == null);
					Debug.Assert(lvi.Group == null);
					Debug.Assert(lvi.SubItems.Count == m_lvMain.Columns.Count);

					if(strFilter == null) { }
					else if((lvgCur != null) && (lvgCur.Header.IndexOf(strFilter,
						sc) >= 0)) { }
					else
					{
						bool bMatch = false;
						int cSI = lvi.SubItems.Count;
						for(int iSI = 0; iSI < cSI; ++iSI)
						{
							if(GetText(lvi, lvfi, iSI).IndexOf(strFilter, sc) >= 0)
							{
								bMatch = true;
								break;
							}
						}
						if(!bMatch) continue;
					}

					m_lvMain.Items.Add(lvi);
					if(lvgCur != null) lvgCur.Items.Add(lvi);
					m_vVisible[i] = true;

					Debug.Assert(lvi.ListView == m_lvMain);
					Debug.Assert(lvi.Group == lvgCur);
					continue;
				}

				ListViewGroup lvg = (o as ListViewGroup);
				if(lvg != null)
				{
					// Caller should not care about associations
					Debug.Assert(lvg.ListView == null);

					m_lvMain.Groups.Add(lvg);
					lvgCur = lvg;
					m_vVisible[i] = true;

					Debug.Assert(lvg.ListView == m_lvMain);
					continue;
				}

				Debug.Assert(false); // Unknown data type
			}

			if((lviTop != null) && (lviTop.ListView == m_lvMain))
				UIUtil.Scroll(m_lvMain, uisi, false);
			m_lvMain.EndUpdateEx();
		}

		private static string GetText(ListViewItem lvi, LvfItem lvfi, int iSubItem)
		{
			Debug.Assert(lvi != null);
			Debug.Assert((lvfi == null) || (lvfi.ListViewItem == lvi));
			Debug.Assert((iSubItem >= 0) && (iSubItem < lvi.SubItems.Count));

			return (((lvfi != null) ? lvfi.SubItems[iSubItem].Text :
				lvi.SubItems[iSubItem].Text) ?? string.Empty);
		}

		private bool CreateResult()
		{
			ListView.SelectedListViewItemCollection lvic = m_lvMain.SelectedItems;
			if(lvic.Count != 1) { Debug.Assert(false); return false; }

			ListViewItem lvi = lvic[0];
			if(lvi == null) { Debug.Assert(false); return false; }

			m_resItem = lvi.Tag;
			m_resGroup = ((lvi.Group != null) ? lvi.Group.Tag : null);
			return true;
		}

		private void ProcessItemActivation()
		{
			if(this.DialogResult == DialogResult.OK) return; // Already closing

			if(CreateResult()) this.DialogResult = DialogResult.OK;
		}

		private void OnListItemActivate(object sender, EventArgs e)
		{
			ProcessItemActivation();
		}

		// The item activation handler has a slight delay when clicking an
		// item, thus as a performance optimization we additionally handle
		// item clicks
		private void OnListClick(object sender, EventArgs e)
		{
			if(m_lvMain.Activation == ItemActivation.OneClick)
				ProcessItemActivation();
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			Keys k = (keyData & Keys.KeyCode);
			if((k == Keys.Return) && m_tstFilter.Focused)
				return true; // Ignore it

			return base.ProcessDialogKey(keyData);
		}

		private void OnFilterTextChanged(object sender, EventArgs e)
		{
			UpdateListViewEx(false);
		}

		private void PerformExport(string strDefaultFormat)
		{
			if((m_fl & LvfFlags.Export) == LvfFlags.None) { Debug.Assert(false); return; }

			if(!AppPolicy.TryWithKey(AppPolicyId.Export, (m_pd == null), m_pd, KPRes.Export))
				return;

			try
			{
				const int iFilterCsv = 1;
				const int iFilterHtml = 2;
				string strFilter = UIUtil.CreateFileTypeFilter("csv", "CSV", false) +
					"|" + UIUtil.CreateFileTypeFilter("html|htm", "HTML", false);

				int iFilter = iFilterCsv;
				string strDefaultExt = "csv";
				if(string.IsNullOrEmpty(strDefaultFormat)) { }
				else if(strDefaultFormat == "CSV") { }
				else if(strDefaultFormat == "HTML")
				{
					iFilter = iFilterHtml;
					strDefaultExt = "html";
				}
				else { Debug.Assert(false); }

				string strSuggFileName = UrlUtil.GetSafeFileName(m_strTitle) +
					"." + strDefaultExt;

				SaveFileDialogEx dlg = UIUtil.CreateSaveFileDialog(KPRes.Export,
					strSuggFileName, strFilter, iFilter, strDefaultExt,
					AppDefs.FileDialogContext.Export);

				if(dlg.ShowDialog() != DialogResult.OK) return;

				string strFile = dlg.FileName;
				if(string.IsNullOrEmpty(strFile)) { Debug.Assert(false); return; }

				iFilter = dlg.FilterIndex;
				string strExt = UrlUtil.GetExtension(strFile);
				if(string.Equals(strExt, "csv", StrUtil.CaseIgnoreCmp))
					iFilter = iFilterCsv;
				else if(string.Equals(strExt, "html", StrUtil.CaseIgnoreCmp) ||
					string.Equals(strExt, "htm", StrUtil.CaseIgnoreCmp))
					iFilter = iFilterHtml;

				if(iFilter == iFilterHtml)
					PerformExportHtml(strFile);
				else
				{
					Debug.Assert(iFilter == iFilterCsv);
					PerformExportCsv(strFile);
				}
			}
			catch(Exception ex) { MessageService.ShowWarning(ex); }
		}

		private void PerformExport(GAction<ListViewGroup> fGroup,
			GAction<ListViewItem, LvfItem> fItem)
		{
			if(fItem == null) { Debug.Assert(false); return; }

			int c = m_lData.Count;
			if(c != m_vVisible.Length) { Debug.Assert(false); return; }

			ListViewGroup lvgLast = null;
			for(int i = 0; i < c; ++i)
			{
				if(!m_vVisible[i]) continue;

				object o = m_lData[i];

				ListViewItem lvi = (o as ListViewItem);
				LvfItem lvfi = (o as LvfItem);
				if((lvi != null) || (lvfi != null))
				{
					if(lvgLast != null)
					{
						if(fGroup != null) fGroup(lvgLast);
						lvgLast = null;
					}

					fItem(((lvfi != null) ? lvfi.ListViewItem : lvi), lvfi);
					continue;
				}

				ListViewGroup lvg = (o as ListViewGroup);
				if(lvg != null) { lvgLast = lvg; continue; }

				Debug.Assert(false); // Unknown data type
			}
		}

		private void PerformExportCsv(string strFile)
		{
			try
			{
				using(FileStream fs = new FileStream(strFile, FileMode.Create,
					FileAccess.Write, FileShare.None))
				{
					using(StreamWriter sw = new StreamWriter(fs, StrUtil.Utf8))
					{
						CsvStreamWriter csw = new CsvStreamWriter(sw);
						int[] vDI = UIUtil.GetDisplayIndices(m_lvMain);
						int c = vDI.Length;

						string[] vFields = new string[c];
						for(int i = 0; i < c; ++i)
							vFields[vDI[i]] = (m_lvMain.Columns[i].Text ?? string.Empty);
						csw.WriteLine(vFields);

						GAction<ListViewItem, LvfItem> fItem = delegate(ListViewItem lvi,
							LvfItem lvfi)
						{
							if(lvi.SubItems.Count == c)
							{
								for(int i = 0; i < c; ++i)
									vFields[vDI[i]] = GetText(lvi, lvfi, i);
							}
							else
							{
								Debug.Assert(false);
								for(int i = 0; i < c; ++i) vFields[i] = "?";
							}
							csw.WriteLine(vFields);
						};

						PerformExport(null, fItem);
					}
				}
			}
			catch(Exception ex) { MessageService.ShowWarning(strFile, ex); }
		}

		private void PerformExportHtml(string strFile)
		{
			try { File.WriteAllText(strFile, GetExportHtml(false), StrUtil.Utf8); }
			catch(Exception ex) { MessageService.ShowWarning(strFile, ex); }
		}

		private string GetExportHtml(bool bTemporary)
		{
			ImageList il = m_lvMain.SmallImageList;
			bool bRtl = (this.RightToLeft == RightToLeft.Yes);
			bool bStdColors = (UIUtil.ColorsEqual(m_lvMain.ForeColor, Color.Black) &&
				UIUtil.ColorsEqual(m_lvMain.BackColor, Color.White));
			NumberFormatInfo nfi = NumberFormatInfo.InvariantInfo;

			int[] vDI = UIUtil.GetDisplayIndices(m_lvMain);
			int c = vDI.Length;

			int[] vDIInv = new int[c];
			for(int i = 0; i < c; ++i) vDIInv[vDI[i]] = i;

			GFunc<string, string> h = new GFunc<string, string>(StrUtil.StringToHtml);

			StringBuilder sb = KeePassHtml2x.HtmlPart1ToHead(bRtl, m_strTitle);
			KeePassHtml2x.HtmlPart2ToStyle(sb);

			sb.AppendLine("body {");
			sb.AppendLine("\tcolor: #000000;");
			sb.AppendLine("\tbackground-color: #FFFFFF;");
			sb.AppendLine("\tfont-family: \"Tahoma\", \"Arial\", \"Verdana\", sans-serif;");
			sb.AppendLine("\tfont-size: 10pt;");
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
			sb.AppendLine("\ttext-align: " + (bRtl ? "right;" : "left;"));
			sb.AppendLine("\tvertical-align: top;");
			sb.AppendLine("\twhite-space: pre-wrap;");
			// sb.AppendLine("\tword-break: break-all;");
			sb.AppendLine("\toverflow-wrap: break-word;");
			sb.AppendLine("\tword-wrap: break-word;");
			sb.AppendLine("}");

			sb.AppendLine("th {");
			sb.AppendLine("\tfont-weight: bold;");
			sb.AppendLine("}");

			sb.AppendLine("td {");
			sb.AppendLine("\tborder-style: solid none;");
			sb.AppendLine("\tborder-width: 1px 0px;");
			sb.AppendLine("\tborder-top-color: #808080;");
			sb.AppendLine("\tborder-bottom-color: #808080;");
			sb.AppendLine("}");

			HorizontalAlignment[] vTextAlign = new HorizontalAlignment[c];
			for(int i = 0; i < c; ++i)
				vTextAlign[vDI[i]] = m_lvMain.Columns[i].TextAlign;
			for(int di = 0; di < c; ++di)
			{
				HorizontalAlignment ha = vTextAlign[di];
				if(ha == HorizontalAlignment.Left) continue;

				sb.Append("th:nth-of-type(");
				sb.Append((di + 1).ToString(nfi));
				sb.Append("), td:nth-of-type(");
				sb.Append((di + 1).ToString(nfi));
				sb.AppendLine(") {");

				if(ha == HorizontalAlignment.Center)
					sb.AppendLine("\ttext-align: center;");
				else if(ha == HorizontalAlignment.Right)
				{
					sb.AppendLine("\ttext-align: right;");
					if(((di + 1) < c) && (vTextAlign[di + 1] == HorizontalAlignment.Left))
						sb.AppendLine("\tpadding-right: 0.75em;");
				}
				else { Debug.Assert(false); }

				sb.AppendLine("}");
			}

			sb.AppendLine("img {");
			sb.AppendLine("\tdisplay: inline-block;");
			sb.AppendLine("\tmargin: 0px;");
			sb.AppendLine("\tpadding: 0px;");
			sb.AppendLine("\tborder: 0px none;");
			sb.AppendLine("\twidth: 1.1em;");
			sb.AppendLine("\theight: 1.1em;");
			sb.AppendLine("\tvertical-align: top;");
			sb.AppendLine("}");

			if(bTemporary)
			{
				// Add the temporary content identifier
				sb.AppendLine("." + Program.TempFilesPool.TempContentTag + " {");
				sb.AppendLine("\tfont-size: 10pt;");
				sb.AppendLine("}");
			}

			KeePassHtml2x.HtmlPart3ToBody(sb);

			sb.AppendLine("<h2>" + h(m_strTitle) + "</h2>");

			if(!string.IsNullOrEmpty(m_strSubTitle))
				sb.AppendLine("<p>" + h(m_strSubTitle) + "</p>");
			// if(!string.IsNullOrEmpty(m_strInfo))
			//	sb.AppendLine("<p>" + h(m_strInfo) + "</p>");

			long lTotalWidth = 0;
			for(int i = 0; i < c; ++i) lTotalWidth += m_lvMain.Columns[i].Width;
			if(lTotalWidth <= 0) { Debug.Assert(false); throw new InvalidOperationException(); }

			string[] vColCss = new string[c];
			string[] vColText = new string[c];
			double dUsedWP = 0;
			for(int i = c - 1; i >= 0; --i)
			{
				ColumnHeader ch = m_lvMain.Columns[i];

				double dWP;
				if(i == 0) dWP = 100.0 - dUsedWP;
				else dWP = 100.0 * (double)ch.Width / (double)lTotalWidth;
				dWP = Math.Round(dWP, 2);
				dUsedWP += dWP;

				vColCss[vDI[i]] = "width: " + dWP.ToString("F2", nfi) + "%;";
				vColText[vDI[i]] = h(ch.Text);
			}

			StringBuilder sbInitTable = new StringBuilder();
			sbInitTable.AppendLine("<table>");
			sbInitTable.AppendLine("<colgroup>");
			for(int di = 0; di < c; ++di)
				sbInitTable.AppendLine("<col style=\"" + vColCss[di] + "\" />");
			sbInitTable.AppendLine("</colgroup>");
			sbInitTable.AppendLine("<tr>");
			for(int di = 0; di < c; ++di)
				sbInitTable.AppendLine("<th>" + vColText[di] + "</th>");
			sbInitTable.AppendLine("</tr>");
			string strInitTable = sbInitTable.ToString();

			bool bInTable = false;
			Action<bool> fEnsureTable = delegate(bool bEnsure)
			{
				if(bEnsure == bInTable) return;

				if(bEnsure) sb.Append(strInitTable);
				else sb.AppendLine("</table>");

				bInTable = bEnsure;
			};

			GAction<ListViewGroup> fGroup = delegate(ListViewGroup lvg)
			{
				fEnsureTable(false);
				sb.Append("<h3>");
				sb.Append(h(lvg.Header));
				sb.AppendLine("</h3>");
			};

			GAction<ListViewItem, LvfItem> fItem = delegate(ListViewItem lvi, LvfItem lvfi)
			{
				fEnsureTable(true);
				sb.AppendLine("<tr>");

				if(lvi.SubItems.Count == c)
				{
					for(int di = 0; di < c; ++di)
					{
						int i = vDIInv[di];
						ListViewItem.ListViewSubItem lvsi = lvi.SubItems[i];

						sb.Append("<td");

						if(bStdColors && (!UIUtil.ColorsEqual(lvsi.ForeColor, Color.Black) ||
							!UIUtil.ColorsEqual(lvsi.BackColor, Color.White)))
						{
							sb.Append(" style=\"color: ");
							sb.Append(StrUtil.ColorToUnnamedHtml(lvsi.ForeColor, false));
							sb.Append("; background-color: ");
							sb.Append(StrUtil.ColorToUnnamedHtml(lvsi.BackColor, false));
							sb.Append(";\"");
						}

						sb.Append('>');

						if((i == 0) && (il != null))
						{
							int iImage = lvi.ImageIndex;
							if((iImage >= 0) && (iImage < il.Images.Count))
							{
								string str = GfxUtil.ImageToDataUri(il.Images[iImage]);
								if(!string.IsNullOrEmpty(str))
								{
									sb.Append("<img src=\"");
									sb.Append(str);
									sb.AppendLine("\"");
									sb.Append("alt=\"\" />&nbsp;");
								}
								else { Debug.Assert(false); }
							}
						}

						sb.Append(h(GetText(lvi, lvfi, i)));
						sb.AppendLine("</td>");
					}
				}
				else
				{
					Debug.Assert(false);
					for(int i = 0; i < c; ++i) sb.AppendLine("<td>?</td>");
				}

				sb.AppendLine("</tr>");
			};

			PerformExport(fGroup, fItem);
			fEnsureTable(false);

			KeePassHtml2x.HtmlPart4ToEnd(sb);
			return sb.ToString();
		}

		private void OnFilePrint(object sender, EventArgs e)
		{
			if((m_fl & LvfFlags.Print) == LvfFlags.None) { Debug.Assert(false); return; }

			if(!AppPolicy.TryWithKey(AppPolicyId.Print, ((m_pd != null) ?
				AppPolicy.Current.PrintNoKey : true), m_pd, KPRes.Print))
				return;

			try { PrintUtil.PrintHtml(GetExportHtml(true)); }
			catch(Exception ex) { MessageService.ShowWarning(ex); }
		}

		private void OnFileExport(object sender, EventArgs e)
		{
			PerformExport(null);
		}
	}
}
