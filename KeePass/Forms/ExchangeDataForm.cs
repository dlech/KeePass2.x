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
using System.IO;
using System.Text;
using System.Windows.Forms;

using KeePass.App;
using KeePass.DataExchange;
using KeePass.Resources;
using KeePass.UI;

using KeePassLib;
using KeePassLib.Serialization;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class ExchangeDataForm : Form
	{
		private bool m_bExport = false;
		private PwDatabase m_pd = null;
		private PwGroup m_pg = null;

		private ImageList m_ilFormats = null;

		private FileFormatProvider m_fmtCur = null; // Current selection
		private FileFormatProvider m_fmtFinal = null; // Returned as result
		public FileFormatProvider ResultFormat
		{
			get { return m_fmtFinal; }
		}

		private string[] m_vFiles = null;
		public string[] ResultFiles
		{
			get { return m_vFiles; }
		}

		private PwExportInfo m_piExport = null;
		internal PwExportInfo ExportInfo
		{
			get { return m_piExport; }
			set { m_piExport = value; }
		}

		private sealed class FormatGroupEx
		{
			private ListViewGroup m_lvg;
			public ListViewGroup Group { get { return m_lvg; } }

			private List<ListViewItem> m_lItems = new List<ListViewItem>();
			public List<ListViewItem> Items { get { return m_lItems; } }

			public FormatGroupEx(string strGroupName)
			{
				m_lvg = new ListViewGroup(strGroupName);
			}
		}

		public void InitEx(bool bExport, PwDatabase pd, PwGroup pg)
		{
			m_bExport = bExport;
			m_pd = pd;
			m_pg = pg;
		}

		public ExchangeDataForm()
		{
			InitializeComponent();
			GlobalWindowManager.InitializeForm(this);
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			if((m_pd == null) && (m_pg == null))
			{
				Debug.Assert(false);
				throw new InvalidOperationException();
			}

			GlobalWindowManager.AddWindow(this);

			string strTitle = (m_bExport ? KPRes.ExportFileTitle : KPRes.ImportFileTitle);
			string strDesc = (m_bExport ? KPRes.ExportFileDesc : KPRes.ImportFileDesc);
			Image img = (m_bExport ? Properties.Resources.B48x48_Folder_Txt :
				Properties.Resources.B48x48_Folder_Download);
			BannerFactory.CreateBannerEx(this, m_bannerImage, img, strTitle, strDesc);

			this.Icon = AppIcons.Default;
			this.Text = strTitle;

			UIUtil.ConfigureToolTip(m_ttRect);
			UIUtil.SetToolTip(m_ttRect, m_btnSelFile, StrUtil.TrimDots(
				KPRes.SelectFile, true), true);

			m_lvFormats.ShowGroups = true;

			int w = m_lvFormats.ClientSize.Width - UIUtil.GetVScrollBarWidth();
			m_lvFormats.Columns.Add(string.Empty, w - 1);

			List<Image> lImages = new List<Image>();
			Dictionary<string, FormatGroupEx> dictGroups =
				new Dictionary<string, FormatGroupEx>();

			foreach(FileFormatProvider f in Program.FileFormatPool)
			{
				if(m_bExport && !f.SupportsExport) continue;
				if(!m_bExport && !f.SupportsImport) continue;

				string strDisplayName = f.DisplayName;
				if(string.IsNullOrEmpty(strDisplayName)) { Debug.Assert(false); continue; }

				string strAppGroup = f.ApplicationGroup;
				if(string.IsNullOrEmpty(strAppGroup)) strAppGroup = KPRes.General;

				FormatGroupEx grp;
				if(!dictGroups.TryGetValue(strAppGroup, out grp))
				{
					grp = new FormatGroupEx(strAppGroup);
					dictGroups[strAppGroup] = grp;
				}

				ListViewItem lvi = new ListViewItem(strDisplayName);
				lvi.Group = grp.Group;
				lvi.Tag = f;

				img = f.SmallIcon;
				if(img == null)
				{
					string strExt = f.DefaultExtension;
					if(!string.IsNullOrEmpty(strExt))
						strExt = UIUtil.GetPrimaryFileTypeExt(strExt);
					if(!string.IsNullOrEmpty(strExt))
						img = FileIcons.GetImageForExtension(strExt, null);
				}
				if(img == null)
					img = Properties.Resources.B16x16_Folder_Inbox;

				int iImage = lImages.IndexOf(img);
				if(iImage < 0) { iImage = lImages.Count; lImages.Add(img); }
				lvi.ImageIndex = iImage;

				grp.Items.Add(lvi);
			}

			foreach(FormatGroupEx formatGroup in dictGroups.Values)
			{
				m_lvFormats.Groups.Add(formatGroup.Group);
				foreach(ListViewItem lvi in formatGroup.Items)
					m_lvFormats.Items.Add(lvi);
			}

			m_ilFormats = UIUtil.BuildImageListUnscaled(lImages,
				DpiUtil.ScaleIntX(16), DpiUtil.ScaleIntY(16));
			m_lvFormats.SmallImageList = m_ilFormats;

			if(m_bExport)
			{
				m_grpFiles.Text = KPRes.Destination;
				UIUtil.SetText(m_lblFiles, KPRes.File + ":");
				UIUtil.SetButtonImage(m_btnSelFile,
					Properties.Resources.B16x16_FileSaveAs, false);

				m_lnkFileFormats.Enabled = false;
				m_lnkFileFormats.Visible = false;
			}
			else // Import
			{
				m_grpFiles.Text = KPRes.Source;
				UIUtil.SetButtonImage(m_btnSelFile,
					Properties.Resources.B16x16_Folder_Yellow_Open, false);

				m_grpExport.Enabled = false;
				m_grpExportPost.Enabled = false;
			}

			m_cbExportMasterKeySpec.Checked = Program.Config.Defaults.ExportMasterKeySpec;
			m_cbExportParentGroups.Checked = Program.Config.Defaults.ExportParentGroups;
			m_cbExportPostOpen.Checked = Program.Config.Defaults.ExportPostOpen;
			m_cbExportPostShow.Checked = Program.Config.Defaults.ExportPostShow;

			UpdateUIState();
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			Program.Config.Defaults.ExportMasterKeySpec = m_cbExportMasterKeySpec.Checked;
			Program.Config.Defaults.ExportParentGroups = m_cbExportParentGroups.Checked;
			Program.Config.Defaults.ExportPostOpen = m_cbExportPostOpen.Checked;
			Program.Config.Defaults.ExportPostShow = m_cbExportPostShow.Checked;

			if(m_ilFormats != null)
			{
				m_lvFormats.SmallImageList = null; // Detach event handlers
				m_ilFormats.Dispose();
				m_ilFormats = null;
			}

			GlobalWindowManager.RemoveWindow(this);
		}

		private void OnLinkFileFormats(object sender, LinkLabelLinkClickedEventArgs e)
		{
			AppHelp.ShowHelp(AppDefs.HelpTopics.ImportExport, null);
		}

		private static bool CheckFilePath(string strPath)
		{
			if(string.IsNullOrEmpty(strPath)) { Debug.Assert(false); return false; }

			if(strPath.IndexOf(';') >= 0)
			{
				MessageService.ShowWarning(strPath, KPRes.FileNameContainsSemicolonError);
				return false;
			}

			return true;
		}

		private void OnBtnSelFile(object sender, EventArgs e)
		{
			UpdateUIState();
			if(m_fmtCur == null) { Debug.Assert(false); return; }
			if(!m_fmtCur.RequiresFile) return; // Break on double-click

			string strFormat = m_fmtCur.FormatName;
			if(string.IsNullOrEmpty(strFormat)) strFormat = KPRes.Data;

			string strExts = m_fmtCur.DefaultExtension;
			if(string.IsNullOrEmpty(strExts)) strExts = "export";
			string strPriExt = UIUtil.GetPrimaryFileTypeExt(strExts);
			if(strPriExt.Length == 0) strPriExt = "export"; // In case of "|"

			string strFilter = UIUtil.CreateFileTypeFilter(strExts, strFormat, true);

			if(!m_bExport) // Import
			{
				OpenFileDialogEx ofd = UIUtil.CreateOpenFileDialog(KPRes.Import +
					": " + strFormat, strFilter, 1, strPriExt, true,
					AppDefs.FileDialogContext.Import);

				if(ofd.ShowDialog() != DialogResult.OK) return;

				StringBuilder sb = new StringBuilder();
				foreach(string str in ofd.FileNames)
				{
					if(!CheckFilePath(str)) continue;

					if(sb.Length != 0) sb.Append(';');
					sb.Append(str);
				}

				string strFiles = sb.ToString();
				if(strFiles.Length >= m_tbFile.MaxLength)
				{
					MessageService.ShowWarning(KPRes.TooManyFilesError);
					return;
				}

				m_tbFile.Text = strFiles;
			}
			else // Export
			{
				SaveFileDialogEx sfd = UIUtil.CreateSaveFileDialog(KPRes.Export +
					": " + strFormat, null, strFilter, 1, strPriExt,
					AppDefs.FileDialogContext.Export);

				string strSuggestion = KPRes.Database;
				if((m_pd != null) && (m_pd.IOConnectionInfo.Path.Length > 0))
					strSuggestion = UrlUtil.StripExtension(UrlUtil.GetFileName(
						m_pd.IOConnectionInfo.Path));
				sfd.FileName = strSuggestion + "." + strPriExt;

				if(sfd.ShowDialog() != DialogResult.OK) return;

				string strFile = sfd.FileName;
				if(!CheckFilePath(strFile)) return;
				m_tbFile.Text = strFile;
			}

			UpdateUIState();
		}

		private void UpdateUIState()
		{
			ListView.SelectedListViewItemCollection lvsc = m_lvFormats.SelectedItems;
			m_fmtCur = (((lvsc != null) && (lvsc.Count == 1)) ?
				(lvsc[0].Tag as FileFormatProvider) : null);
			bool bFormat = (m_fmtCur != null);

			bool bFileReq = (bFormat && m_fmtCur.RequiresFile);
			UIUtil.SetEnabledFast(bFileReq, m_lblFiles, m_tbFile, m_btnSelFile);

			bool bExportExt = (m_bExport && (m_piExport != null));
			UIUtil.SetEnabledFast((bExportExt && bFormat && m_fmtCur.RequiresKey),
				m_cbExportMasterKeySpec, m_lblExportMasterKeySpec);

			UIUtil.SetEnabledFast((bExportExt && bFormat && (m_pd != null) &&
				(m_pg != m_pd.RootGroup)),
				m_cbExportParentGroups, m_lnkExportParentGroups);

			UIUtil.SetEnabledFast((bExportExt && bFileReq), m_cbExportPostOpen,
				m_cbExportPostShow);

			m_btnOK.Enabled = (bFormat && ((m_tbFile.Text.Length != 0) ||
				!m_fmtCur.RequiresFile));
		}

		private bool PrepareExchangeEx()
		{
			UpdateUIState();
			if(m_fmtCur == null) return false;

			string strFiles = m_tbFile.Text;
			string[] vFiles = strFiles.Split(new char[] { ';' },
				StringSplitOptions.RemoveEmptyEntries);

			if(m_fmtCur.RequiresFile)
			{
				if(vFiles.Length == 0) return false;

				foreach(string strFile in vFiles)
				{
					IOConnectionInfo ioc = IOConnectionInfo.FromPath(strFile);
					if(ioc.IsLocalFile() && !UrlUtil.IsAbsolutePath(strFile))
					{
						MessageService.ShowWarning(strFile, KPRes.FilePathFullReq);
						return false;
					}
				}

				// Allow only one file when exporting
				if(m_bExport && !CheckFilePath(strFiles)) return false;
			}
			else vFiles = MemUtil.EmptyArray<string>();

			if(m_piExport != null)
			{
				m_piExport.ExportMasterKeySpec = m_cbExportMasterKeySpec.Checked;
				m_piExport.ExportParentGroups = m_cbExportParentGroups.Checked;
				m_piExport.ExportPostOpen = m_cbExportPostOpen.Checked;
				m_piExport.ExportPostShow = m_cbExportPostShow.Checked;
			}

			m_fmtFinal = m_fmtCur;
			m_vFiles = vFiles;
			return true;
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			if(!PrepareExchangeEx()) this.DialogResult = DialogResult.None;
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
		}

		private void OnFormatsSelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateUIState();
		}

		private void OnImportFileTextChanged(object sender, EventArgs e)
		{
			UpdateUIState();
		}

		private void OnFormatsItemActivate(object sender, EventArgs e)
		{
			OnBtnSelFile(sender, e);
		}

		private void OnLinkExportParentGroups(object sender, LinkLabelLinkClickedEventArgs e)
		{
			AppHelp.ShowHelp(AppDefs.HelpTopics.ImportExport,
				AppDefs.HelpTopics.ImportExportParents);
		}
	}
}
