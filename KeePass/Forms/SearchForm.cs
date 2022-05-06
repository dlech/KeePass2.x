/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2022 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Text;
using System.Windows.Forms;

using KeePass.App;
using KeePass.App.Configuration;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Interfaces;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class SearchForm : Form
	{
		private readonly string ProfileCustom = "(" + KPRes.Custom + ")";

		// private PwDatabase m_pdContext = null;
		private PwGroup m_pgRoot = null;

		private uint m_uBlockProfileAuto = 0;

		private PwGroup m_pgResults = null;
		public PwGroup SearchResultsGroup
		{
			get { return m_pgResults; }
		}

		private SearchParameters m_spResult = null;
		internal SearchParameters SearchResultParameters
		{
			get { return m_spResult; }
		}

		private string m_strInitProfile = string.Empty;
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DefaultValue("")]
		internal string InitProfile
		{
			get { return m_strInitProfile; }
			set { m_strInitProfile = value; }
		}

		public SearchForm()
		{
			InitializeComponent();
			GlobalWindowManager.InitializeForm(this);
		}

		/// <summary>
		/// Initialize the form. Must be called before the dialog is displayed.
		/// </summary>
		public void InitEx(PwDatabase pdContext, PwGroup pgRoot)
		{
			// m_pdContext = pdContext;
			m_pgRoot = pgRoot;
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			if(m_pgRoot == null) { Debug.Assert(false); throw new InvalidOperationException(); }

			GlobalWindowManager.AddWindow(this);

			string strTitle = KPRes.SearchTitle;
			string strDesc = KPRes.SearchDesc2;
			if((m_pgRoot.ParentGroup != null) && !string.IsNullOrEmpty(m_pgRoot.Name))
			{
				strTitle += " (" + KPRes.SelectedGroup + ")";
				strDesc = KPRes.Group + ": '" + m_pgRoot.Name + "'.";
			}

			BannerFactory.CreateBannerEx(this, m_bannerImage,
				Properties.Resources.B48x48_XMag, strTitle, strDesc);
			this.Icon = AppIcons.Default;
			this.Text = KPRes.SearchTitle;

			UIUtil.SetButtonImage(m_btnProfileAdd,
				Properties.Resources.B16x16_FileSaveAs, false);
			UIUtil.SetButtonImage(m_btnProfileDelete,
				Properties.Resources.B16x16_EditDelete, true);

			UIUtil.SetText(m_cbDerefData, m_cbDerefData.Text + " (" + KPRes.Slow + ")");

			UIUtil.ConfigureToolTip(m_ttMain);
			UIUtil.SetToolTip(m_ttMain, m_btnProfileAdd, KPRes.ProfileSaveDesc, false);
			UIUtil.SetToolTip(m_ttMain, m_btnProfileDelete, KPRes.ProfileDeleteDesc, false);

			AccessibilityEx.SetName(m_btnProfileAdd, KPRes.ProfileSave);
			AccessibilityEx.SetName(m_btnProfileDelete, KPRes.ProfileDelete);

			SearchParameters sp = (!string.IsNullOrEmpty(m_strInitProfile) ?
				Program.Config.Search.FindProfile(m_strInitProfile) : null);
			Debug.Assert(string.IsNullOrEmpty(m_strInitProfile) || (sp != null));
			UpdateProfilesList((sp != null) ? sp.Name : ProfileCustom);
			SetSearchParameters(sp ?? Program.Config.Search.LastUsedProfile);

			m_tbSearch.TextChanged += this.OnProfilePropertyChanged;
			m_rbModeSimple.CheckedChanged += this.OnProfilePropertyChanged;
			m_rbModeRegular.CheckedChanged += this.OnProfilePropertyChanged;
			m_rbModeXPath.CheckedChanged += this.OnProfilePropertyChanged;

			CheckBox[] v = new CheckBox[] {
				m_cbTitle, m_cbUserName, m_cbPassword, m_cbUrl, m_cbNotes,
				m_cbStringsOther, m_cbStringName, m_cbTags, m_cbUuid,
				m_cbGroupPath, m_cbGroupName, m_cbHistory,
				m_cbCaseSensitive, m_cbExcludeExpired, m_cbIgnoreGroupSettings,
				m_cbDerefData
			};
			foreach(CheckBox cb in v) cb.CheckedChanged += this.OnProfilePropertyChanged;

			UpdateUIState();
			m_tbSearch.SelectAll();
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			Program.Config.Search.LastUsedProfile = GetSearchParameters();

			GlobalWindowManager.RemoveWindow(this);
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			SearchParameters sp = GetSearchParameters();

			Form fOptDialog;
			IStatusLogger sl = StatusUtil.CreateStatusDialog(this, out fOptDialog,
				null, KPRes.SearchingOp + "...", true, false);
			// if(fOptDialog != null) Program.MainForm.RedirectActivationPush(fOptDialog);
			this.Enabled = false;

			PwGroup pgResults = null;
			Exception exFind = null;
			try { pgResults = SearchUtil.Find(sp, m_pgRoot, sl); }
			catch(Exception ex) { exFind = ex; }

			this.Enabled = true;
			// if(fOptDialog != null) Program.MainForm.RedirectActivationPop();
			sl.EndLogging();

			if(exFind != null)
			{
				MessageService.ShowWarning(sp.SearchString, exFind);
				this.DialogResult = DialogResult.None;
			}
			else if(pgResults != null)
			{
				m_pgResults = pgResults;
				m_spResult = sp;
			}
			else
			{
				Debug.Assert(false);
				this.DialogResult = DialogResult.None;
			}
		}

		private void UpdateProfilesList(string strSelect)
		{
			++m_uBlockProfileAuto;

			List<object> l = new List<object>();
			int iSel = 0;

			l.Add(ProfileCustom);

			Program.Config.Search.SortProfiles();
			foreach(SearchParameters sp in Program.Config.Search.UserProfiles)
			{
				string strName = sp.Name;
				if(strName == strSelect) iSel = l.Count;
				l.Add(strName);
			}

			m_cmbProfiles.Items.Clear();
			m_cmbProfiles.Items.AddRange(l.ToArray());

			Debug.Assert((strSelect == ProfileCustom) || (iSel != 0));
			m_cmbProfiles.SelectedIndex = iSel;

			--m_uBlockProfileAuto;
		}

		private void SetSearchParameters(SearchParameters sp)
		{
			if(sp == null) { Debug.Assert(false); sp = new SearchParameters(); }

			++m_uBlockProfileAuto;

			Debug.Assert((m_cmbProfiles.Text == ProfileCustom) ||
				(m_cmbProfiles.Text == sp.Name));

			m_tbSearch.Text = sp.SearchString;

			if(sp.SearchMode == PwSearchMode.Regular)
				m_rbModeRegular.Checked = true;
			else if(sp.SearchMode == PwSearchMode.XPath)
				m_rbModeXPath.Checked = true;
			else
			{
				Debug.Assert(sp.SearchMode == PwSearchMode.Simple);
				m_rbModeSimple.Checked = true;
			}

			m_cbTitle.Checked = sp.SearchInTitles;
			m_cbUserName.Checked = sp.SearchInUserNames;
			m_cbPassword.Checked = sp.SearchInPasswords;
			m_cbUrl.Checked = sp.SearchInUrls;
			m_cbNotes.Checked = sp.SearchInNotes;
			m_cbStringsOther.Checked = sp.SearchInOther;
			m_cbStringName.Checked = sp.SearchInStringNames;
			m_cbTags.Checked = sp.SearchInTags;
			m_cbUuid.Checked = sp.SearchInUuids;
			m_cbGroupPath.Checked = sp.SearchInGroupPaths;
			m_cbGroupName.Checked = sp.SearchInGroupNames;
			m_cbHistory.Checked = sp.SearchInHistory;

			StringComparison sc = sp.ComparisonMode;
			m_cbCaseSensitive.Checked = ((sc != StringComparison.CurrentCultureIgnoreCase) &&
				(sc != StringComparison.InvariantCultureIgnoreCase) &&
				(sc != StringComparison.OrdinalIgnoreCase));

			m_cbExcludeExpired.Checked = sp.ExcludeExpired;
			m_cbIgnoreGroupSettings.Checked = !sp.RespectEntrySearchingDisabled;

			string strTrf = SearchUtil.GetTransformation(sp);
			m_cbDerefData.Checked = (strTrf == SearchUtil.StrTrfDeref);

			--m_uBlockProfileAuto;
		}

		private SearchParameters GetSearchParameters()
		{
			SearchParameters sp = new SearchParameters();

			sp.Name = m_cmbProfiles.Text;
			sp.SearchString = m_tbSearch.Text;

			if(m_rbModeRegular.Checked)
				sp.SearchMode = PwSearchMode.Regular;
			else if(m_rbModeXPath.Checked)
				sp.SearchMode = PwSearchMode.XPath;
			else
			{
				Debug.Assert(m_rbModeSimple.Checked);
				sp.SearchMode = PwSearchMode.Simple;
			}

			sp.SearchInTitles = m_cbTitle.Checked;
			sp.SearchInUserNames = m_cbUserName.Checked;
			sp.SearchInPasswords = m_cbPassword.Checked;
			sp.SearchInUrls = m_cbUrl.Checked;
			sp.SearchInNotes = m_cbNotes.Checked;
			sp.SearchInOther = m_cbStringsOther.Checked;
			sp.SearchInStringNames = m_cbStringName.Checked;
			sp.SearchInTags = m_cbTags.Checked;
			sp.SearchInUuids = m_cbUuid.Checked;
			sp.SearchInGroupPaths = m_cbGroupPath.Checked;
			sp.SearchInGroupNames = m_cbGroupName.Checked;
			sp.SearchInHistory = m_cbHistory.Checked;

			sp.ComparisonMode = (m_cbCaseSensitive.Checked ?
				StringComparison.InvariantCulture :
				StringComparison.InvariantCultureIgnoreCase);

			sp.ExcludeExpired = m_cbExcludeExpired.Checked;
			sp.RespectEntrySearchingDisabled = !m_cbIgnoreGroupSettings.Checked;

			SearchUtil.SetTransformation(sp, (m_cbDerefData.Checked ?
				SearchUtil.StrTrfDeref : string.Empty));

			return sp;
		}

		private void UpdateUIState()
		{
			++m_uBlockProfileAuto;

			bool bCustom = (m_cmbProfiles.Text == ProfileCustom);
			bool bXPath = m_rbModeXPath.Checked;

			m_btnProfileDelete.Enabled = !bCustom;

			m_cbStringName.Enabled = !bXPath;
			if(bXPath) m_cbStringName.Checked = true;

			m_cbUuid.Enabled = !bXPath;
			if(bXPath) m_cbUuid.Checked = true;

			m_cbGroupPath.Enabled = !bXPath;
			if(bXPath) m_cbGroupPath.Checked = false;

			bool bGroupPath = m_cbGroupPath.Checked;
			m_cbGroupName.Enabled = !bGroupPath;
			if(bGroupPath) m_cbGroupName.Checked = true;

			m_cbCaseSensitive.Enabled = !bXPath;
			if(bXPath) m_cbCaseSensitive.Checked = true;

			m_cbDerefData.Enabled = !bXPath;
			if(bXPath) m_cbDerefData.Checked = false;

			--m_uBlockProfileAuto;
		}

		private void OnProfilesSelectedIndexChanged(object sender, EventArgs e)
		{
			if(m_uBlockProfileAuto != 0) return;

			string strName = m_cmbProfiles.Text;
			if(strName != ProfileCustom)
				SetSearchParameters(Program.Config.Search.FindProfile(strName));

			UpdateUIState();
		}

		private void OnProfilePropertyChanged(object sender, EventArgs e)
		{
			if(m_uBlockProfileAuto != 0) return;

			Debug.Assert((m_cmbProfiles.Items.Count != 0) && !m_cmbProfiles.Sorted &&
				((m_cmbProfiles.Items[0] as string) == ProfileCustom));
			if(m_cmbProfiles.Items.Count != 0)
				m_cmbProfiles.SelectedIndex = 0;

			UpdateUIState();
		}

		private void OnBtnProfileAdd(object sender, EventArgs e)
		{
			List<string> lNames = new List<string>();
			foreach(SearchParameters sp in Program.Config.Search.UserProfiles)
				lNames.Add(sp.Name);

			SingleLineEditForm dlg = new SingleLineEditForm();
			dlg.InitEx(KPRes.ProfileSave, KPRes.ProfileSaveDesc,
				KPRes.ProfileSavePrompt, Properties.Resources.B48x48_KMag,
				string.Empty, lNames.ToArray());

			if(dlg.ShowDialog() == DialogResult.OK)
			{
				string strName = dlg.ResultString;

				if(string.IsNullOrEmpty(strName) || (strName == ProfileCustom))
					MessageService.ShowWarning(KPRes.FieldNameInvalid);
				else
				{
					SearchParameters sp = GetSearchParameters();
					sp.Name = strName;

					AceSearch aceSearch = Program.Config.Search;
					int i = aceSearch.FindProfileIndex(strName);
					if(i >= 0) aceSearch.UserProfiles[i] = sp;
					else aceSearch.UserProfiles.Add(sp);

					UpdateProfilesList(strName);
					UpdateUIState();
				}
			}
			UIUtil.DestroyForm(dlg);
		}

		private void OnBtnProfileDelete(object sender, EventArgs e)
		{
			string strName = m_cmbProfiles.Text;
			if(strName == ProfileCustom) { Debug.Assert(false); return; }

			AceSearch aceSearch = Program.Config.Search;
			int i = aceSearch.FindProfileIndex(strName);
			if(i >= 0)
			{
				aceSearch.UserProfiles.RemoveAt(i);

				UpdateProfilesList(ProfileCustom);
				UpdateUIState();
			}
			else { Debug.Assert(false); }
		}

		private void OnBtnHelp(object sender, EventArgs e)
		{
			AppHelp.ShowHelp(AppDefs.HelpTopics.Search, null);
		}
	}
}
