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
using KeePassLib.Collections;
using KeePassLib.Delegates;
using KeePassLib.Security;

namespace KeePass.Forms
{
	/// <summary>
	/// Form in which the user can configure search parameters. This
	/// dialog performs the search itself and returns the result
	/// in the <c>SearchResultsGroup</c> property.
	/// </summary>
	public partial class SearchForm : Form
	{
		private PwGroup m_pgRoot = null;
		private PwGroup m_pgResultsGroup = null;

		/// <summary>
		/// After closing the dialog, this property contains the search results.
		/// </summary>
		public PwGroup SearchResultsGroup
		{
			get { return m_pgResultsGroup; }
		}

		/// <summary>
		/// Default constructor.
		/// </summary>
		public SearchForm()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Initialize the form. Must be called before the dialog is displayed.
		/// </summary>
		/// <param name="pwRoot">Data source group. This group will be searched.</param>
		public void InitEx(PwGroup pwRoot)
		{
			m_pgRoot = pwRoot;
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			Debug.Assert(m_pgRoot != null); if(m_pgRoot == null) throw new ArgumentNullException();

			m_bannerImage.Image = BannerFactory.CreateBanner(m_bannerImage.Width,
				m_bannerImage.Height, BannerFactory.BannerStyle.Default,
				Properties.Resources.B48x48_XMag, KPRes.SearchTitle,
				KPRes.SearchDesc);
			this.Icon = Properties.Resources.KeePass;

			m_cbAllFields.Checked = false;
			m_cbTitle.Checked = m_cbUserName.Checked = m_cbPassword.Checked =
				m_cbURL.Checked = m_cbNotes.Checked = true;

			EnableUserControls();
			m_tbSearch.Focus();
		}

		private void EnableUserControls()
		{
			if(m_cbAllFields.Checked)
			{
				m_cbTitle.Checked = m_cbUserName.Checked = m_cbPassword.Checked =
					m_cbURL.Checked = m_cbNotes.Checked = true;

				m_cbTitle.Enabled = m_cbUserName.Enabled = m_cbPassword.Enabled =
					m_cbURL.Enabled = m_cbNotes.Enabled = false;
			}
			else
			{
				m_cbTitle.Enabled = m_cbUserName.Enabled = m_cbPassword.Enabled =
					m_cbURL.Enabled = m_cbNotes.Enabled = true;
			}
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			SearchParameters sp = new SearchParameters();

			sp.SearchText = m_tbSearch.Text;
			sp.SearchInTitles = m_cbTitle.Checked;
			sp.SearchInUserNames = m_cbUserName.Checked;
			sp.SearchInPasswords = m_cbPassword.Checked;
			sp.SearchInUrls = m_cbURL.Checked;
			sp.SearchInNotes = m_cbNotes.Checked;
			sp.SearchInAllStrings = m_cbAllFields.Checked;

			sp.StringCompare = m_cbCaseSensitive.Checked ?
				StringComparison.InvariantCulture :
				StringComparison.InvariantCultureIgnoreCase;

			string strGroupName = KPRes.SearchGroupName + " (\"" + sp.SearchText + "\" ";
			strGroupName += KPRes.SearchResultsInSeparator + " ";
			strGroupName += m_pgRoot.Name + ")";
			PwGroup pgResults = new PwGroup(true, true, strGroupName, PwIcon.EMailSearch);
			pgResults.IsVirtual = true;

			PwEntry peDesc = new PwEntry(pgResults, true, true);
			peDesc.Icon = PwIcon.EMailSearch;
			pgResults.Entries.Add(peDesc);
			PwObjectList<PwEntry> listResults = pgResults.Entries;

			m_pgRoot.SearchEntries(sp, listResults);

			string strItemsFound = (listResults.UCount - 1).ToString() + " " + KPRes.SearchItemsFoundSmall;
			peDesc.Strings.Set(PwDefs.TitleField, new ProtectedString(false, strItemsFound));

			m_pgResultsGroup = pgResults;
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
		}

		private void OnCheckedAllFields(object sender, EventArgs e)
		{
			EnableUserControls();
		}
	}
}