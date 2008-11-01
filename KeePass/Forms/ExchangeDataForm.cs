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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

using KeePass.App;
using KeePass.DataExchange;
using KeePass.Resources;
using KeePass.UI;

using KeePassLib;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class ExchangeDataForm : Form
	{
		private bool m_bExport = false;
		private PwDatabase m_pwDatabaseInfo = null;
		private PwGroup m_pgRootInfo = null;

		private FileFormatProvider m_fmtCur = null; // Current selection

		private FileFormatProvider m_fmtFinal = null; // Returned as result
		private string[] m_vFiles = null;

		internal sealed class FormatGroupEx
		{
			private ListViewGroup m_lvg;
			public ListViewGroup Group { get { return m_lvg; } }

			private List<ListViewItem> m_vItems = new List<ListViewItem>();
			public List<ListViewItem> Items { get { return m_vItems; } }

			public FormatGroupEx(string strGroupName)
			{
				m_lvg = new ListViewGroup(strGroupName);
			}
		}

		public FileFormatProvider ResultFormat
		{
			get { return m_fmtFinal; }
		}

		public string[] ResultFiles
		{
			get { return m_vFiles; }
		}

		public void InitEx(bool bExport, PwDatabase pwDatabaseInfo, PwGroup pgRootInfo)
		{
			m_bExport = bExport;
			m_pwDatabaseInfo = pwDatabaseInfo;
			m_pgRootInfo = pgRootInfo;
		}

		public ExchangeDataForm()
		{
			InitializeComponent();
			Program.Translation.ApplyTo(this);
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			Debug.Assert((m_pwDatabaseInfo != null) || (m_pgRootInfo != null));
			if((m_pwDatabaseInfo == null) && (m_pgRootInfo == null))
				throw new InvalidOperationException();

			GlobalWindowManager.AddWindow(this);

			string strWndTitle = (m_bExport ? KPRes.ExportFileTitle : KPRes.ImportFileTitle);
			string strWndDesc = (m_bExport ? KPRes.ExportFileDesc : KPRes.ImportFileDesc);
			Bitmap bmpBanner = (m_bExport ? Properties.Resources.B48x48_Folder_Txt :
				Properties.Resources.B48x48_Folder_Download);
			m_bannerImage.Image = BannerFactory.CreateBanner(m_bannerImage.Width,
				m_bannerImage.Height, BannerStyle.Default, bmpBanner, strWndTitle, strWndDesc);
			this.Icon = Properties.Resources.KeePass;

			this.Text = strWndTitle;

			if(m_bExport)
			{
				m_lblFile.Text = KPRes.ExportToPrompt;
				m_btnSelFile.Image = KeePass.Properties.Resources.B16x16_FileSaveAs;

				m_lnkFileFormats.Enabled = false;
				m_lnkFileFormats.Visible = false;
			}
			else // Import mode
			{
				m_lblFile.Text = KPRes.ImportFilesPrompt;
				m_btnSelFile.Image = KeePass.Properties.Resources.B16x16_Folder_Yellow_Open;
			}

			m_lvFormats.ShowGroups = true;
			m_lvFormats.Columns.Add(string.Empty);
			m_lvFormats.Columns[0].Width = m_lvFormats.ClientRectangle.Width - 1;

			ImageList ilFormats = new ImageList();
			ilFormats.ColorDepth = ColorDepth.Depth32Bit;
			ilFormats.ImageSize = new Size(16, 16);

			Dictionary<string, FormatGroupEx> dictGroups =
				new Dictionary<string, FormatGroupEx>();

			foreach(FileFormatProvider f in Program.FileFormatPool)
			{
				if(m_bExport && (f.SupportsExport == false)) continue;
				if((m_bExport == false) && (f.SupportsImport == false)) continue;

				string strDisplayName = f.DisplayName;
				if((strDisplayName == null) || (strDisplayName.Length == 0))
					continue;

				string strAppGroup = f.ApplicationGroup;
				if((strAppGroup == null) || (strAppGroup.Length == 0))
					strAppGroup = KPRes.General;

				FormatGroupEx grp;
				if(!dictGroups.TryGetValue(strAppGroup, out grp))
				{
					grp = new FormatGroupEx(strAppGroup);
					dictGroups.Add(strAppGroup, grp);
				}

				ListViewItem lvi = new ListViewItem(strDisplayName);
				lvi.Group = grp.Group;
				lvi.Tag = f;

				Image imgSmallIcon = f.SmallIcon;
				if(imgSmallIcon == null)
					imgSmallIcon = KeePass.Properties.Resources.B16x16_Folder_Inbox;

				ilFormats.Images.Add(imgSmallIcon);
				lvi.ImageIndex = ilFormats.Images.Count - 1;

				grp.Items.Add(lvi);
			}

			foreach(FormatGroupEx formatGroup in dictGroups.Values)
			{
				m_lvFormats.Groups.Add(formatGroup.Group);
				foreach(ListViewItem lvi in formatGroup.Items)
					m_lvFormats.Items.Add(lvi);
			}

			m_lvFormats.SmallImageList = ilFormats;

			CustomizeForScreenReader();
			UpdateUIState();
		}

		private void CustomizeForScreenReader()
		{
			if(!Program.Config.UI.OptimizeForScreenReader) return;

			m_btnSelFile.Text = KPRes.SelectFile;
		}

		private void OnLinkFileFormats(object sender, LinkLabelLinkClickedEventArgs e)
		{
			AppHelp.ShowHelp(AppDefs.HelpTopics.ImportExport, null);
		}

		private void OnBtnSelFile(object sender, EventArgs e)
		{
			if(m_fmtCur == null) { Debug.Assert(false); return; }

			string strFormat = m_fmtCur.FormatName;
			if((strFormat == null) || (strFormat.Length == 0))
				strFormat = KPRes.Data;

			string strExt = m_fmtCur.DefaultExtension;
			if((strExt == null) || (strExt.Length == 0))
				strExt = "export";

			string strFilter = UIUtil.CreateFileTypeFilter(strExt, strFormat, true);

			if(m_bExport == false) // Import
			{
				OpenFileDialog ofd = UIUtil.CreateOpenFileDialog(KPRes.Import + ": " +
					strFormat, strFilter, 1, strExt, true, true);

				if(ofd.ShowDialog() != DialogResult.OK) return;

				StringBuilder sb = new StringBuilder();
				foreach(string str in ofd.FileNames)
				{
					if(sb.Length > 0) sb.Append(';');

					if(str.IndexOf(';') >= 0)
						MessageService.ShowWarning(str, KPRes.FileNameContainsSemicolonError);
					else sb.Append(str);
				}

				string strFiles = sb.ToString();
				if(strFiles.Length < m_tbFile.MaxLength)
					m_tbFile.Text = strFiles;
				else
					MessageService.ShowWarning(KPRes.TooManyFilesError);
			}
			else // Export
			{
				SaveFileDialog sfd = UIUtil.CreateSaveFileDialog(KPRes.Export + ": " +
					strFormat, null, strFilter, 1, strExt, false);

				string strSuggestion;
				if((m_pwDatabaseInfo != null) &&
					(m_pwDatabaseInfo.IOConnectionInfo.Path.Length > 0))
				{
					strSuggestion = UrlUtil.StripExtension(UrlUtil.GetFileName(
						m_pwDatabaseInfo.IOConnectionInfo.Path));
				}
				else strSuggestion = KPRes.Database;

				if((m_fmtCur != null) && (m_fmtCur.DefaultExtension != null) &&
					(m_fmtCur.DefaultExtension.Length > 0))
				{
					strSuggestion += "." + m_fmtCur.DefaultExtension;
				}
				else strSuggestion += ".export";

				sfd.FileName = strSuggestion;

				if(sfd.ShowDialog() != DialogResult.OK) return;

				m_tbFile.Text = sfd.FileName;
			}

			UpdateUIState();
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			if(!PrepareExchangeEx()) this.DialogResult = DialogResult.None;
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
		}

		private void UpdateUIState()
		{
			bool bFormatSelected = true;
			ListView.SelectedListViewItemCollection lvsc = m_lvFormats.SelectedItems;

			if((lvsc == null) || (lvsc.Count != 1)) bFormatSelected = false;

			if(bFormatSelected) m_fmtCur = lvsc[0].Tag as FileFormatProvider;
			else m_fmtCur = null;

			if(m_fmtCur != null)
				m_tbFile.Enabled = m_btnSelFile.Enabled = m_fmtCur.RequiresFile;
			else
				m_tbFile.Enabled = m_btnSelFile.Enabled = false;

			m_btnOK.Enabled = (bFormatSelected && ((m_tbFile.Text.Length != 0) ||
				!m_fmtCur.RequiresFile));
		}

		private bool PrepareExchangeEx()
		{
			UpdateUIState();
			if(m_fmtCur == null) return false;

			m_fmtFinal = m_fmtCur;
			m_vFiles = m_tbFile.Text.Split(new char[]{ ';' },
				StringSplitOptions.RemoveEmptyEntries);

			if(m_bExport) { Debug.Assert(m_vFiles.Length <= 1); }

			return true;
		}

		private void OnFormatsSelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateUIState();
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			GlobalWindowManager.RemoveWindow(this);
		}

		private void OnImportFileTextChanged(object sender, EventArgs e)
		{
			UpdateUIState();
		}
	}
}
