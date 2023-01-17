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
using System.Text;
using System.Threading;
using System.Windows.Forms;

using KeePass.App;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class GroupForm : Form
	{
		private PwGroup m_pwGroup = null;
		private bool m_bCreatingNew = false;
		private ImageList m_ilClientIcons = null;
		private PwDatabase m_pwDatabase = null;

		private PwIcon m_pwIconIndex = 0;
		private PwUuid m_pwCustomIconID = PwUuid.Zero;
		private StringDictionaryEx m_sdCustomData = null;

		private ExpiryControlGroup m_cgExpiry = new ExpiryControlGroup();

		private GroupFormTab m_gftInit = GroupFormTab.None;
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DefaultValue(GroupFormTab.None)]
		internal GroupFormTab InitialTab
		{
			// get { return m_gftInit; } // Internal, uncalled
			set { m_gftInit = value; }
		}

		[Obsolete]
		public void InitEx(PwGroup pg, ImageList ilClientIcons, PwDatabase pwDatabase)
		{
			InitEx(pg, false, ilClientIcons, pwDatabase);
		}

		public void InitEx(PwGroup pg, bool bCreatingNew, ImageList ilClientIcons,
			PwDatabase pwDatabase)
		{
			m_pwGroup = pg;
			m_bCreatingNew = bCreatingNew;
			m_ilClientIcons = ilClientIcons;
			m_pwDatabase = pwDatabase;
		}

		public GroupForm()
		{
			InitializeComponent();
			GlobalWindowManager.InitializeForm(this);
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			if(m_pwGroup == null) { Debug.Assert(false); throw new InvalidOperationException(); }
			if(m_pwDatabase == null) { Debug.Assert(false); throw new InvalidOperationException(); }

			GlobalWindowManager.AddWindow(this);

			string strTitle = (m_bCreatingNew ? KPRes.AddGroup : KPRes.EditGroup);
			BannerFactory.CreateBannerEx(this, m_bannerImage,
				Properties.Resources.B48x48_Folder_Txt, strTitle,
				(m_bCreatingNew ? KPRes.AddGroupDesc : KPRes.EditGroupDesc));
			this.Icon = AppIcons.Default;
			this.Text = strTitle;

			UIUtil.ConfigureToolTip(m_ttRect);
			UIUtil.SetToolTip(m_ttRect, m_btnIcon, KPRes.SelectIcon, true);
			UIUtil.SetToolTip(m_ttRect, m_btnAutoTypeEdit, KPRes.ConfigureKeystrokeSeq, true);

			AccessibilityEx.SetContext(m_tbDefaultAutoTypeSeq, m_rbAutoTypeOverride);

			m_tbName.Text = m_pwGroup.Name;

			m_pwIconIndex = m_pwGroup.IconId;
			m_pwCustomIconID = m_pwGroup.CustomIconUuid;

			if(!m_pwCustomIconID.Equals(PwUuid.Zero))
				UIUtil.SetButtonImage(m_btnIcon, DpiUtil.GetIcon(
					m_pwDatabase, m_pwCustomIconID), true);
			else
				UIUtil.SetButtonImage(m_btnIcon, m_ilClientIcons.Images[
					(int)m_pwIconIndex], true);

			UIUtil.SetMultilineText(m_tbNotes, m_pwGroup.Notes);

			if(m_pwGroup.Expires)
			{
				m_dtExpires.Value = TimeUtil.ToLocal(m_pwGroup.ExpiryTime, true);
				m_cbExpires.Checked = true;
			}
			else // Does not expire
			{
				m_dtExpires.Value = DateTime.Now.Date;
				m_cbExpires.Checked = false;
			}
			m_cgExpiry.Attach(m_cbExpires, m_dtExpires);

			TagUtil.MakeInheritedTagsLink(m_linkTagsInh, m_pwGroup.ParentGroup, this);
			m_tbTags.Text = StrUtil.TagsToString(m_pwGroup.Tags, true);
			TagUtil.MakeTagsButton(m_btnTags, m_tbTags, m_ttRect, m_pwGroup.ParentGroup,
				((m_pwDatabase != null) ? m_pwDatabase.RootGroup : null));

			m_tbUuid.Text = m_pwGroup.Uuid.ToHexString() + ", " +
				Convert.ToBase64String(m_pwGroup.Uuid.UuidBytes);

			PwGroup pgParent = m_pwGroup.ParentGroup;

			bool bParentSearching = ((pgParent != null) ?
				pgParent.GetSearchingEnabledInherited() :
				PwGroup.DefaultSearchingEnabled);
			UIUtil.MakeInheritableBoolComboBox(m_cmbEnableSearching,
				m_pwGroup.EnableSearching, bParentSearching);

			bool bParentAutoType = ((pgParent != null) ?
				pgParent.GetAutoTypeEnabledInherited() :
				PwGroup.DefaultAutoTypeEnabled);
			UIUtil.MakeInheritableBoolComboBox(m_cmbEnableAutoType,
				m_pwGroup.EnableAutoType, bParentAutoType);

			m_tbDefaultAutoTypeSeq.Text = m_pwGroup.GetAutoTypeSequenceInherited();

			if(m_pwGroup.DefaultAutoTypeSequence.Length == 0)
				m_rbAutoTypeInherit.Checked = true;
			else m_rbAutoTypeOverride.Checked = true;

			UIUtil.SetButtonImage(m_btnAutoTypeEdit,
				Properties.Resources.B16x16_Wizard, true);

			m_sdCustomData = m_pwGroup.CustomData.CloneDeep();
			UIUtil.StrDictListInit(m_lvCustomData);
			UIUtil.StrDictListUpdate(m_lvCustomData, m_sdCustomData, false);

			EnableControlsEx();

			ThreadPool.QueueUserWorkItem(delegate(object state)
			{
				try
				{
					string[] vSeq = m_pwDatabase.RootGroup.GetAutoTypeSequences(true);
					// Do not append, because long suggestions hide the start
					UIUtil.EnableAutoCompletion(m_tbDefaultAutoTypeSeq,
						false, vSeq); // Invokes
				}
				catch(Exception) { Debug.Assert(false); }
			});

			UIUtil.SetFocus(m_tbName, this);

			switch(m_gftInit)
			{
				case GroupFormTab.Properties:
					m_tabMain.SelectedTab = m_tabProperties; break;
				case GroupFormTab.AutoType:
					m_tabMain.SelectedTab = m_tabAutoType; break;
				case GroupFormTab.CustomData:
					m_tabMain.SelectedTab = m_tabCustomData; break;
				default: break;
			}
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			m_cgExpiry.Release();

			GlobalWindowManager.RemoveWindow(this);
		}

		private void EnableControlsEx()
		{
			m_tbDefaultAutoTypeSeq.Enabled = m_btnAutoTypeEdit.Enabled =
				!m_rbAutoTypeInherit.Checked;

			m_btnCDDel.Enabled = (m_lvCustomData.SelectedIndices.Count > 0);
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			m_pwGroup.Touch(true, false);

			m_pwGroup.Name = m_tbName.Text;
			m_pwGroup.IconId = m_pwIconIndex;
			m_pwGroup.CustomIconUuid = m_pwCustomIconID;
			m_pwGroup.Notes = m_tbNotes.Text;

			m_pwGroup.Expires = m_cgExpiry.Checked;
			m_pwGroup.ExpiryTime = m_cgExpiry.Value;

			m_pwGroup.Tags = StrUtil.StringToTags(m_tbTags.Text);

			m_pwGroup.EnableSearching = UIUtil.GetInheritableBoolComboBoxValue(m_cmbEnableSearching);
			m_pwGroup.EnableAutoType = UIUtil.GetInheritableBoolComboBoxValue(m_cmbEnableAutoType);

			if(m_rbAutoTypeInherit.Checked)
				m_pwGroup.DefaultAutoTypeSequence = string.Empty;
			else m_pwGroup.DefaultAutoTypeSequence = m_tbDefaultAutoTypeSeq.Text;

			m_pwGroup.CustomData = m_sdCustomData;
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
		}

		private void OnBtnIcon(object sender, EventArgs e)
		{
			IconPickerForm ipf = new IconPickerForm();
			ipf.InitEx(m_ilClientIcons, (uint)PwIcon.Count, m_pwDatabase,
				(uint)m_pwIconIndex, m_pwCustomIconID);

			if(ipf.ShowDialog() == DialogResult.OK)
			{
				m_pwIconIndex = (PwIcon)ipf.ChosenIconId;
				m_pwCustomIconID = ipf.ChosenCustomIconUuid;

				if(!m_pwCustomIconID.Equals(PwUuid.Zero))
					UIUtil.SetButtonImage(m_btnIcon, DpiUtil.GetIcon(
						m_pwDatabase, m_pwCustomIconID), true);
				else
					UIUtil.SetButtonImage(m_btnIcon, m_ilClientIcons.Images[
						(int)m_pwIconIndex], true);
			}

			UIUtil.DestroyForm(ipf);
		}

		private void OnAutoTypeInheritCheckedChanged(object sender, EventArgs e)
		{
			EnableControlsEx();
		}

		private void OnBtnAutoTypeEdit(object sender, EventArgs e)
		{
			// string strName = @"(" + KPRes.AutoType + @")";

			AutoTypeConfig atConfig = new AutoTypeConfig();
			atConfig.DefaultSequence = m_tbDefaultAutoTypeSeq.Text;

			EditAutoTypeItemForm dlg = new EditAutoTypeItemForm();
			dlg.InitEx(atConfig, -1, true, atConfig.DefaultSequence, null);

			if(dlg.ShowDialog() == DialogResult.OK)
				m_tbDefaultAutoTypeSeq.Text = atConfig.DefaultSequence;

			UIUtil.DestroyForm(dlg);
			EnableControlsEx();
		}

		private void OnCustomDataSelectedIndexChanged(object sender, EventArgs e)
		{
			EnableControlsEx();
		}

		private void OnBtnCDDel(object sender, EventArgs e)
		{
			UIUtil.StrDictListDeleteSel(m_lvCustomData, m_sdCustomData, false);
			UIUtil.SetFocus(m_lvCustomData, this);
			EnableControlsEx();
		}
	}
}
