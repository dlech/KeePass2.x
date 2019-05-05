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
using System.IO;
using System.Text;
using System.Windows.Forms;

using KeePass.App;
using KeePass.App.Configuration;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;
using KeePass.Util.XmlSerialization;

using KeePassLib;
using KeePassLib.Translation;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class LanguageForm : Form, IGwmWindow
	{
		private ImageList m_ilIcons = null;

		public bool CanCloseWithoutDataLoss { get { return true; } }

		public LanguageForm()
		{
			InitializeComponent();
			Program.Translation.ApplyTo(this);
		}

		public bool InitEx()
		{
			try
			{
				string strDir = UrlUtil.GetFileDirectory(WinUtil.GetExecutable(),
					false, true);
				List<string> l = UrlUtil.GetFilePaths(strDir, "*." +
					KPTranslation.FileExtension, SearchOption.TopDirectoryOnly);
				if(l.Count != 0)
				{
					string str = KPRes.LngInAppDir + MessageService.NewParagraph;

					const int cMaxFL = 6;
					for(int i = 0; i < Math.Min(l.Count, cMaxFL); ++i)
					{
						if(i == (cMaxFL - 1)) str += "...";
						else str += l[i];
						str += MessageService.NewLine;
					}
					str += MessageService.NewLine;

					str += KPRes.LngInAppDirNote + MessageService.NewParagraph;
					str += KPRes.LngInAppDirQ;

					if(MessageService.AskYesNo(str, PwDefs.ShortProductName, true,
						MessageBoxIcon.Warning))
					{
						WinUtil.OpenUrlDirectly(strDir);
						return false;
					}
				}
			}
			catch(Exception) { Debug.Assert(false); }

			return true;
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			GlobalWindowManager.AddWindow(this, this);

			BannerFactory.CreateBannerEx(this, m_bannerImage,
				Properties.Resources.B48x48_Keyboard_Layout,
				KPRes.SelectLanguage, KPRes.SelectLanguageDesc);
			this.Icon = AppIcons.Default;
			this.Text = KPRes.SelectLanguage;

			UIUtil.SetExplorerTheme(m_lvLanguages, true);

			List<Image> lImg = new List<Image>();
			lImg.Add(Properties.Resources.B16x16_Browser);

			m_ilIcons = UIUtil.BuildImageListUnscaled(lImg,
				DpiUtil.ScaleIntX(16), DpiUtil.ScaleIntY(16));
			m_lvLanguages.SmallImageList = m_ilIcons;

			m_lvLanguages.Columns.Add(KPRes.InstalledLanguages);
			m_lvLanguages.Columns.Add(KPRes.Version);
			m_lvLanguages.Columns.Add(KPRes.Author);
			m_lvLanguages.Columns.Add(KPRes.Contact);
			m_lvLanguages.Columns.Add(KPRes.File);

			KPTranslation trlEng = new KPTranslation();
			trlEng.Properties.NameEnglish = "English";
			trlEng.Properties.NameNative = "English";
			trlEng.Properties.ApplicationVersion = PwDefs.VersionString;
			trlEng.Properties.AuthorName = AppDefs.DefaultTrlAuthor;
			trlEng.Properties.AuthorContact = AppDefs.DefaultTrlContact;

			string strDirA = AceApplication.GetLanguagesDir(AceDir.App, false);
			string strDirASep = UrlUtil.EnsureTerminatingSeparator(strDirA, false);
			string strDirU = AceApplication.GetLanguagesDir(AceDir.User, false);

			List<KeyValuePair<string, KPTranslation>> lTrls =
				new List<KeyValuePair<string, KPTranslation>>();
			lTrls.Add(new KeyValuePair<string, KPTranslation>(string.Empty, trlEng));
			AddTranslations(strDirA, lTrls);
			if(WinUtil.IsAppX) AddTranslations(strDirU, lTrls);
			lTrls.Sort(LanguageForm.CompareTrlItems);

			foreach(KeyValuePair<string, KPTranslation> kvp in lTrls)
			{
				KPTranslationProperties p = kvp.Value.Properties;
				string strName = p.NameEnglish + " (" + p.NameNative + ")";
				string strVer = PwDefs.GetTranslationDisplayVersion(p.ApplicationVersion);
				bool bBuiltIn = ((kvp.Key.Length == 0) || (WinUtil.IsAppX &&
					kvp.Key.StartsWith(strDirASep, StrUtil.CaseIgnoreCmp)));

				ListViewItem lvi = m_lvLanguages.Items.Add(strName, 0);
				lvi.SubItems.Add(strVer);
				lvi.SubItems.Add(p.AuthorName);
				lvi.SubItems.Add(p.AuthorContact);
				lvi.SubItems.Add(bBuiltIn ? KPRes.BuiltInU : kvp.Key);
				lvi.Tag = kvp.Key;

				// try
				// {
				//	string nl = MessageService.NewLine;
				//	lvi.ToolTipText = strName + " " + strVer + nl + p.AuthorName +
				//		nl + p.AuthorContact + nl + nl + kvp.Key;
				// }
				// catch(Exception) { Debug.Assert(false); } // Too long?

				// if(kvp.Key.Equals(Program.Config.Application.GetLanguageFilePath(),
				//	StrUtil.CaseIgnoreCmp))
				//	UIUtil.SetFocusedItem(m_lvLanguages, lvi, true);
			}

			UIUtil.ResizeColumns(m_lvLanguages, new int[] { 5, 2, 5, 5, 3 }, true);
			UIUtil.SetFocus(m_lvLanguages, this);
		}

		private static void AddTranslations(string strDir,
			List<KeyValuePair<string, KPTranslation>> lTrls)
		{
			try
			{
				List<string> lFiles = UrlUtil.GetFilePaths(strDir, "*." +
					KPTranslation.FileExtension, SearchOption.TopDirectoryOnly);
				foreach(string strFilePath in lFiles)
				{
					try
					{
						XmlSerializerEx xs = new XmlSerializerEx(typeof(KPTranslation));
						KPTranslation t = KPTranslation.Load(strFilePath, xs);

						if(t != null)
							lTrls.Add(new KeyValuePair<string, KPTranslation>(
								strFilePath, t));
						else { Debug.Assert(false); }
					}
					catch(Exception ex) { MessageService.ShowWarning(ex); }
				}

				Debug.Assert(KPTranslation.FileExtension != KPTranslation.FileExtension1x);

				lFiles = UrlUtil.GetFilePaths(strDir, "*." +
					KPTranslation.FileExtension1x, SearchOption.TopDirectoryOnly);
				foreach(string strFilePath in lFiles)
				{
					KPTranslation t = new KPTranslation();
					t.Properties.NameEnglish = UrlUtil.StripExtension(
						UrlUtil.GetFileName(strFilePath));
					t.Properties.NameNative = KPRes.Incompatible;
					t.Properties.ApplicationVersion = "1.x";
					t.Properties.AuthorName = "?";
					t.Properties.AuthorContact = "?";

					lTrls.Add(new KeyValuePair<string, KPTranslation>(
						strFilePath, t));
				}
			}
			catch(Exception) { } // Directory might not exist or cause access violation
		}

		private static int CompareTrlItems(KeyValuePair<string, KPTranslation> a,
			KeyValuePair<string, KPTranslation> b)
		{
			KPTranslationProperties pA = a.Value.Properties;
			KPTranslationProperties pB = b.Value.Properties;

			int c = StrUtil.CompareNaturally(pA.NameEnglish, pB.NameEnglish);
			if(c != 0) return c;

			c = StrUtil.CompareNaturally(pA.NameNative, pB.NameNative);
			if(c != 0) return c;

			c = StrUtil.CompareNaturally(pA.ApplicationVersion, pB.ApplicationVersion);
			if(c != 0) return ((c < 0) ? 1 : -1); // Descending

			return string.Compare(a.Key, b.Key, StrUtil.CaseIgnoreCmp);
		}

		private void OnBtnClose(object sender, EventArgs e)
		{
		}

		private void OnLanguagesItemActivate(object sender, EventArgs e)
		{
			ListView.SelectedListViewItemCollection lvic = m_lvLanguages.SelectedItems;
			if((lvic == null) || (lvic.Count != 1)) return;

			string strSel = ((lvic[0].Tag as string) ?? string.Empty);

			if(strSel.EndsWith("." + KPTranslation.FileExtension1x, StrUtil.CaseIgnoreCmp))
			{
				string strMsg = strSel + MessageService.NewParagraph + KPRes.Lng1xSel +
					MessageService.NewParagraph + KPRes.Lng2xWeb + MessageService.NewLine;
				string strUrl = PwDefs.TranslationsUrl;
				string strVtd = strMsg + VistaTaskDialog.CreateLink(strUrl, strUrl);

				VistaTaskDialog vtd = new VistaTaskDialog();
				vtd.AddButton((int)DialogResult.Cancel, KPRes.Ok, null);
				vtd.Content = strVtd;
				vtd.DefaultButtonID = (int)DialogResult.Cancel;
				vtd.EnableHyperlinks = true;
				vtd.SetIcon(VtdIcon.Warning);
				vtd.WindowTitle = PwDefs.ShortProductName;

				if(!vtd.ShowDialog())
					MessageService.ShowWarning(strMsg + strUrl);
				return;
			}

			// The following creates confusion when the configured language
			// is different from the loaded language (which can occur when
			// the language file has been deleted/moved)
			// if(strSel.Equals(Program.Config.Application.GetLanguageFilePath(),
			//	StrUtil.CaseIgnoreCmp))
			//	return; // Is active already, do not close the dialog

			Program.Config.Application.SetLanguageFilePath(strSel);
			this.DialogResult = DialogResult.OK;
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			if(m_ilIcons != null)
			{
				m_lvLanguages.SmallImageList = null;
				m_ilIcons.Dispose();
				m_ilIcons = null;
			}
			else { Debug.Assert(false); }

			GlobalWindowManager.RemoveWindow(this);
		}

		private void OnBtnGetMore(object sender, EventArgs e)
		{
			WinUtil.OpenUrl(PwDefs.TranslationsUrl, null);
			this.DialogResult = DialogResult.Cancel;
		}

		private void OnBtnOpenFolder(object sender, EventArgs e)
		{
			try
			{
				AceDir d = (WinUtil.IsAppX ? AceDir.User : AceDir.App);

				// try
				// {
				//	string strU = AceApplication.GetLanguagesDir(AceDir.User, false);
				//	List<string> l = UrlUtil.GetFilePaths(strU, "*." +
				//		KPTranslation.FileExtension, SearchOption.TopDirectoryOnly);
				//	if(l.Count > 0) d = AceDir.User;
				// }
				// catch(Exception) { }

				string str = AceApplication.GetLanguagesDir(d, false);
				if(!Directory.Exists(str)) Directory.CreateDirectory(str);

				WinUtil.OpenUrlDirectly(str);
				this.DialogResult = DialogResult.Cancel;
			}
			catch(Exception ex) { MessageService.ShowWarning(ex); }
		}
	}
}
