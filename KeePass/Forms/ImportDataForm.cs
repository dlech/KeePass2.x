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
using System.IO;

using KeePass.App;
using KeePass.DataExchange;
using KeePass.Resources;
using KeePass.UI;

using KeePassLib;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class ImportDataForm : Form
	{
		private PwDatabase m_pwDatabase = null;
		private FormatImporter m_fmtImp = null;

		private FormatImporter m_fmtFinal = null;
		private string[] m_vFiles = null;

		public FormatImporter ResultFormat
		{
			get { return m_fmtFinal; }
		}

		public string[] ResultFiles
		{
			get { return m_vFiles; }
		}

		public void InitEx(PwDatabase pwDatabase)
		{
			m_pwDatabase = pwDatabase;
		}

		public ImportDataForm()
		{
			InitializeComponent();
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			Debug.Assert(m_pwDatabase != null);
			if(m_pwDatabase == null) throw new InvalidOperationException();

			GlobalWindowManager.AddWindow(this);

			m_bannerImage.Image = BannerFactory.CreateBanner(m_bannerImage.Width,
				m_bannerImage.Height, BannerFactory.BannerStyle.Default,
				Properties.Resources.B48x48_Folder_Download, KPRes.ImportFileTitle,
				KPRes.ImportFileDesc);
			this.Icon = Properties.Resources.KeePass;

			this.Text = KPRes.ImportFileTitle;

			m_lvFormats.Columns.Add(string.Empty);
			m_lvFormats.ShowGroups = true;

			ImageList ilFormats = new ImageList();
			ilFormats.ColorDepth = ColorDepth.Depth32Bit;
			ilFormats.ImageSize = new Size(16, 16);

			ListViewGroup lvg = new ListViewGroup(KPRes.General);
			m_lvFormats.Groups.Add(lvg);
			foreach(FormatImporter f in ImporterPool.Importers)
			{
				if(f.AppGroup != lvg.Header)
				{
					lvg = new ListViewGroup(f.AppGroup);
					m_lvFormats.Groups.Add(lvg);
				}

				ListViewItem lvi = new ListViewItem(f.DisplayName);
				lvi.Group = lvg;
				lvi.Tag = f;

				ilFormats.Images.Add(f.SmallIcon);
				lvi.ImageIndex = ilFormats.Images.Count - 1;

				m_lvFormats.Items.Add(lvi);
			}

			m_lvFormats.SmallImageList = ilFormats;
			m_lvFormats.Columns[0].Width = m_lvFormats.ClientRectangle.Width - 1;

			UpdateUIState();
		}

		private void OnLinkFileFormats(object sender, LinkLabelLinkClickedEventArgs e)
		{
			AppHelp.ShowHelp(AppDefs.HelpTopics.ImportExport, null);
		}

		private void OnBtnSelFile(object sender, EventArgs e)
		{
			if(m_fmtImp == null) { Debug.Assert(false); return; }

			OpenFileDialog ofd = new OpenFileDialog();
			ofd.AddExtension = false;
			ofd.CheckFileExists = true;
			ofd.CheckPathExists = true;
			ofd.DefaultExt = m_fmtImp.DefaultExtension;
			ofd.Filter = m_fmtImp.FormatName + @" (*." + m_fmtImp.DefaultExtension +
				@")|*." + m_fmtImp.DefaultExtension + @"|" + KPRes.AllFiles +
				@" (*.*)|*.*";
			ofd.Multiselect = true;
			ofd.RestoreDirectory = true;
			ofd.Title = KPRes.Import + ": " + m_fmtImp.FormatName;
			ofd.ValidateNames = true;

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

			UpdateUIState();
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			if(!DoImport()) this.DialogResult = DialogResult.None;
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
		}

		private void UpdateUIState()
		{
			bool bFormatSelected = true;
			ListView.SelectedListViewItemCollection lvsc = m_lvFormats.SelectedItems;

			if((lvsc == null) || (lvsc.Count != 1)) bFormatSelected = false;

			if(bFormatSelected) m_fmtImp = lvsc[0].Tag as FormatImporter;
			else m_fmtImp = null;

			if(m_fmtImp != null)
				m_tbFile.Enabled = m_btnSelFile.Enabled = m_fmtImp.RequiresFile;
			else
				m_tbFile.Enabled = m_btnSelFile.Enabled = false;

			m_btnOK.Enabled = bFormatSelected && ((m_tbFile.Text.Length != 0) ||
				!m_fmtImp.RequiresFile);
		}

		private bool DoImport()
		{
			UpdateUIState();
			if(m_fmtImp == null) return true;

			m_fmtFinal = m_fmtImp;
			m_vFiles = m_tbFile.Text.Split(new char[]{ ';' },
				StringSplitOptions.RemoveEmptyEntries);

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
