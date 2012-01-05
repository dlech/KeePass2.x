/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2012 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

using KeePass.App;
using KeePass.App.Configuration;
using KeePass.UI;
using KeePass.Resources;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Translation;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class LanguageForm : Form, IGwmWindow
	{
		public bool CanCloseWithoutDataLoss { get { return true; } }

		public LanguageForm()
		{
			InitializeComponent();
			Program.Translation.ApplyTo(this);
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			GlobalWindowManager.AddWindow(this, this);

			BannerFactory.CreateBannerEx(this, m_bannerImage,
				Properties.Resources.B48x48_Keyboard_Layout,
				KPRes.SelectLanguage, KPRes.SelectLanguageDesc);
			this.Icon = Properties.Resources.KeePass;
			this.Text = KPRes.SelectLanguage;

			int nWidth = m_lvLanguages.ClientRectangle.Width / 4;
			m_lvLanguages.Columns.Add(KPRes.AvailableLanguages, nWidth);
			m_lvLanguages.Columns.Add(KPRes.Version, nWidth);
			m_lvLanguages.Columns.Add(KPRes.Author, nWidth);
			m_lvLanguages.Columns.Add(KPRes.Contact, nWidth);

			ListViewItem lvi = m_lvLanguages.Items.Add("English", 0);
			lvi.SubItems.Add(PwDefs.VersionString);
			lvi.SubItems.Add(AppDefs.DefaultTrlAuthor);
			lvi.SubItems.Add(AppDefs.DefaultTrlContact);

			List<string> vList = new List<string>();
			GetAvailableTranslations(AppConfigSerializer.AppDataDirectory, vList);
			GetAvailableTranslations(AppConfigSerializer.LocalAppDataDirectory, vList);

			string strExe = WinUtil.GetExecutable();
			string strPath = UrlUtil.GetFileDirectory(strExe, false, true);
			GetAvailableTranslations(strPath, vList);
		}

		private void GetAvailableTranslations(string strPath, List<string> vList)
		{
			try
			{
				DirectoryInfo di = new DirectoryInfo(strPath);
				FileInfo[] vFiles = di.GetFiles();

				foreach(FileInfo fi in vFiles)
				{
					string strFullName = fi.FullName;

					if(strFullName.ToLower().EndsWith("." + KPTranslation.FileExtension))
					{
						string strFileName = UrlUtil.GetFileName(strFullName);

						bool bFound = false;
						foreach(string strExisting in vList)
						{
							if(strExisting.Equals(strFileName, StrUtil.CaseIgnoreCmp))
							{
								bFound = true;
								break;
							}
						}
						if(bFound) continue;

						try
						{
							KPTranslation kpTrl = KPTranslation.LoadFromFile(strFullName);

							ListViewItem lvi = m_lvLanguages.Items.Add(
								kpTrl.Properties.NameEnglish, 0);
							lvi.SubItems.Add(kpTrl.Properties.ApplicationVersion);
							lvi.SubItems.Add(kpTrl.Properties.AuthorName);
							lvi.SubItems.Add(kpTrl.Properties.AuthorContact);
							lvi.Tag = strFileName;

							vList.Add(strFileName);
						}
						catch(Exception ex)
						{
							MessageService.ShowWarning(ex.Message);
						}
					}
				}
			}
			catch(Exception) { } // Directory might not exist or cause access violation
		}

		private void OnBtnClose(object sender, EventArgs e)
		{
		}

		private void OnLanguagesItemActivate(object sender, EventArgs e)
		{
			ListView.SelectedListViewItemCollection lvic = m_lvLanguages.SelectedItems;
			if((lvic == null) || (lvic.Count != 1)) return;

			if(lvic[0].Index == 0) // First item selected = English
			{
				if(Program.Config.Application.LanguageFile.Length == 0)
					return; // Is English already

				Program.Config.Application.LanguageFile = string.Empty;
			}
			else
			{
				string strSelID = (lvic[0].Tag as string);
				if(strSelID == Program.Config.Application.LanguageFile) return;

				Program.Config.Application.LanguageFile = strSelID;
			}

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			GlobalWindowManager.RemoveWindow(this);
		}

		private void OnBtnGetMore(object sender, EventArgs e)
		{
			WinUtil.OpenUrl(PwDefs.TranslationsUrl, null);
		}
	}
}
