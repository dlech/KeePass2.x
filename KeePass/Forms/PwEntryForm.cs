/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2011 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Diagnostics;
using System.Globalization;
using System.IO;

using KeePass.App;
using KeePass.App.Configuration;
using KeePass.UI;
using KeePass.Resources;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Cryptography;
using KeePassLib.Cryptography.PasswordGenerator;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public enum PwEditMode
	{
		Invalid = 0,
		AddNewEntry = 1,
		EditExistingEntry = 2,
		ViewReadOnlyEntry = 3
	}

	public partial class PwEntryForm : Form
	{
		private PwEditMode m_pwEditMode = PwEditMode.Invalid;
		private PwDatabase m_pwDatabase = null;
		private bool m_bShowAdvancedByDefault = false;
		private bool m_bSelectFullTitle = false;

		private PwEntry m_pwEntry = null;
		private PwEntry m_pwInitialEntry = null;
		private ProtectedStringDictionary m_vStrings = null;
		private ProtectedBinaryDictionary m_vBinaries = null;
		private AutoTypeConfig m_atConfig = null;
		private PwObjectList<PwEntry> m_vHistory = null;
		private Color m_clrForeground = Color.Empty;
		private Color m_clrBackground = Color.Empty;

		private PwIcon m_pwEntryIcon = PwIcon.Key;
		private PwUuid m_pwCustomIconID = PwUuid.Zero;
		private ImageList m_ilIcons = null;

		private Color m_clrNormalBackColor = Color.White;
		private bool m_bRepeatPasswordFailed = false;
		private bool m_bLockEnabledState = false;

		private bool m_bInitializing = false;

		private SecureEdit m_secPassword = new SecureEdit();
		private SecureEdit m_secRepeat = new SecureEdit();
		private RichTextBoxContextMenu m_ctxNotes = new RichTextBoxContextMenu();

		private readonly string DeriveFromPrevious = "(" + KPRes.GenPwBasedOnPrevious + ")";
		private DynamicMenu m_dynGenProfiles;

		private const PwIcon m_pwObjectProtected = PwIcon.PaperLocked;
		private const PwIcon m_pwObjectPlainText = PwIcon.PaperNew;

		public event EventHandler<CancellableOperationEventArgs> EntrySaving;
		public event EventHandler EntrySaved;

		public bool HasModifiedEntry
		{
			get
			{
				if((m_pwEntry == null) || (m_pwInitialEntry == null))
				{
					Debug.Assert(false);
					return true;
				}

				return !m_pwEntry.EqualsEntry(m_pwInitialEntry, false, true, true,
					false, false);
			}
		}

		public PwEntry EntryRef { get { return m_pwEntry; } }
		public ProtectedStringDictionary EntryStrings { get { return m_vStrings; } }
		public ProtectedBinaryDictionary EntryBinaries { get { return m_vBinaries; } }

		public ContextMenuStrip ToolsContextMenu
		{
			get { return m_ctxTools; }
		}

		public ContextMenuStrip DefaultTimesContextMenu
		{
			get { return m_ctxDefaultTimes; }
		}

		public ContextMenuStrip ListOperationsContextMenu
		{
			get { return m_ctxListOperations; }
		}

		public ContextMenuStrip PasswordGeneratorContextMenu
		{
			get { return m_ctxPwGen; }
		}

		public ContextMenuStrip StandardStringMovementContextMenu
		{
			get { return m_ctxStrMoveToStandard; }
		}

		private bool m_bInitSwitchToHistory = false;
		internal bool InitSwitchToHistoryTab
		{
			// get { return m_bInitSwitchToHistory; } // Internal, uncalled
			set { m_bInitSwitchToHistory = value; }
		}

		public PwEntryForm()
		{
			InitializeComponent();

			Program.Translation.ApplyTo(this);
			Program.Translation.ApplyTo("KeePass.Forms.PwEntryForm.m_ctxTools", m_ctxTools.Items);
			Program.Translation.ApplyTo("KeePass.Forms.PwEntryForm.m_ctxDefaultTimes", m_ctxDefaultTimes.Items);
			Program.Translation.ApplyTo("KeePass.Forms.PwEntryForm.m_ctxListOperations", m_ctxListOperations.Items);
			Program.Translation.ApplyTo("KeePass.Forms.PwEntryForm.m_ctxPwGen", m_ctxPwGen.Items);
			Program.Translation.ApplyTo("KeePass.Forms.PwEntryForm.m_ctxStrMoveToStandard", m_ctxStrMoveToStandard.Items);
		}

		public void InitEx(PwEntry pwEntry, PwEditMode pwMode, PwDatabase pwDatabase,
			ImageList ilIcons, bool bShowAdvancedByDefault, bool bSelectFullTitle)
		{
			Debug.Assert(pwEntry != null); if(pwEntry == null) throw new ArgumentNullException("pwEntry");
			Debug.Assert(pwMode != PwEditMode.Invalid); if(pwMode == PwEditMode.Invalid) throw new ArgumentException();
			Debug.Assert(ilIcons != null); if(ilIcons == null) throw new ArgumentNullException("ilIcons");

			m_pwEntry = pwEntry;
			m_pwEditMode = pwMode;
			m_pwDatabase = pwDatabase;
			m_ilIcons = ilIcons;
			m_bShowAdvancedByDefault = bShowAdvancedByDefault;
			m_bSelectFullTitle = bSelectFullTitle;

			m_vStrings = m_pwEntry.Strings.CloneDeep();
			m_vBinaries = m_pwEntry.Binaries.CloneDeep();
			m_atConfig = m_pwEntry.AutoType.CloneDeep();
			m_vHistory = m_pwEntry.History.CloneDeep();
		}

		private void InitEntryTab()
		{
			m_pwEntryIcon = m_pwEntry.IconId;
			m_pwCustomIconID = m_pwEntry.CustomIconUuid;

			if(m_pwCustomIconID != PwUuid.Zero)
			{
				// int nInx = (int)PwIcon.Count + m_pwDatabase.GetCustomIconIndex(m_pwCustomIconID);
				// if((nInx > -1) && (nInx < m_ilIcons.Images.Count))
				//	m_btnIcon.Image = m_ilIcons.Images[nInx];
				// else m_btnIcon.Image = m_ilIcons.Images[(int)m_pwEntryIcon];

				Image imgCustom = m_pwDatabase.GetCustomIcon(m_pwCustomIconID);
				// m_btnIcon.Image = (imgCustom ?? m_ilIcons.Images[(int)m_pwEntryIcon]);
				UIUtil.SetButtonImage(m_btnIcon, (imgCustom ?? m_ilIcons.Images[
					(int)m_pwEntryIcon]), true);
			}
			else
			{
				// m_btnIcon.Image = m_ilIcons.Images[(int)m_pwEntryIcon];
				UIUtil.SetButtonImage(m_btnIcon, m_ilIcons.Images[
					(int)m_pwEntryIcon], true);
			}

			bool bHideInitial = m_cbHidePassword.Checked;
			m_secPassword.Attach(m_tbPassword, ProcessTextChangedPassword, bHideInitial);
			m_secRepeat.Attach(m_tbRepeatPassword, ProcessTextChangedRepeatPw, bHideInitial);

			m_dtExpireDateTime.CustomFormat = DateTimeFormatInfo.CurrentInfo.ShortDatePattern +
				" " + DateTimeFormatInfo.CurrentInfo.LongTimePattern;

			if(m_pwEntry.Expires)
			{
				m_dtExpireDateTime.Value = m_pwEntry.ExpiryTime;
				m_cbExpires.Checked = true;
			}
			else // Does not expire
			{
				m_dtExpireDateTime.Value = DateTime.Now;
				m_cbExpires.Checked = false;
			}

			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry)
			{
				m_tbTitle.ReadOnly = m_tbUserName.ReadOnly = m_tbPassword.ReadOnly =
					m_tbRepeatPassword.ReadOnly = m_tbUrl.ReadOnly =
					m_rtNotes.ReadOnly = true;

				m_btnIcon.Enabled = m_btnGenPw.Enabled =
					m_tbRepeatPassword.Enabled = m_cbExpires.Enabled =
					m_dtExpireDateTime.Enabled =
					m_btnStandardExpires.Enabled = false;

				m_rtNotes.SelectAll();
				m_rtNotes.BackColor = m_rtNotes.SelectionBackColor =
					AppDefs.ColorControlDisabled;
				m_rtNotes.DeselectAll();

				m_ctxToolsUrlSelApp.Enabled = m_ctxToolsUrlSelDoc.Enabled = false;
				m_ctxToolsFieldRefsInTitle.Enabled = m_ctxToolsFieldRefsInUserName.Enabled =
					m_ctxToolsFieldRefsInPassword.Enabled = m_ctxToolsFieldRefsInUrl.Enabled =
					m_ctxToolsFieldRefsInNotes.Enabled = false;
				m_ctxToolsFieldRefs.Enabled = false;

				m_btnOK.Enabled = false;
			}

			// Show URL in blue, if it's black in the current visual theme
			if(m_tbUrl.ForeColor.ToArgb() == Color.Black.ToArgb())
				m_tbUrl.ForeColor = Color.Blue;
		}

		private void InitAdvancedTab()
		{
			m_lvStrings.SmallImageList = m_ilIcons;
			m_lvBinaries.SmallImageList = m_ilIcons;

			int nWidth = m_lvStrings.ClientRectangle.Width / 2;
			m_lvStrings.Columns.Add(KPRes.FieldName, nWidth);
			m_lvStrings.Columns.Add(KPRes.FieldValue, nWidth);

			nWidth = m_lvBinaries.ClientRectangle.Width;
			m_lvBinaries.Columns.Add(KPRes.Attachments, nWidth);
			// m_lvBinaries.Columns.Add(KPRes.FieldValue, nWidth);

			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry)
			{
				m_btnStrAdd.Enabled = m_btnStrEdit.Enabled =
					m_btnStrDelete.Enabled = m_btnStrMove.Enabled =
					m_btnBinAdd.Enabled = m_btnBinDelete.Enabled =
					m_btnBinView.Enabled = m_btnBinSave.Enabled = false;
			}
		}

		// Public for plugins
		public void UpdateEntryStrings(bool bGuiToInternal, bool bSetRepeatPw)
		{
			if(bGuiToInternal)
			{
				m_vStrings.Set(PwDefs.TitleField, new ProtectedString(m_pwDatabase.MemoryProtection.ProtectTitle,
					m_tbTitle.Text));
				m_vStrings.Set(PwDefs.UserNameField, new ProtectedString(m_pwDatabase.MemoryProtection.ProtectUserName,
					m_tbUserName.Text));

				byte[] pb = m_secPassword.ToUtf8();
				m_vStrings.Set(PwDefs.PasswordField, new ProtectedString(m_pwDatabase.MemoryProtection.ProtectPassword,
					pb));
				MemUtil.ZeroByteArray(pb);

				m_vStrings.Set(PwDefs.UrlField, new ProtectedString(m_pwDatabase.MemoryProtection.ProtectUrl,
					m_tbUrl.Text));
				m_vStrings.Set(PwDefs.NotesField, new ProtectedString(m_pwDatabase.MemoryProtection.ProtectNotes,
					m_rtNotes.Text));
			}
			else // Internal to GUI
			{
				m_tbTitle.Text = m_vStrings.ReadSafe(PwDefs.TitleField);
				m_tbUserName.Text = m_vStrings.ReadSafe(PwDefs.UserNameField);

				byte[] pb = m_vStrings.GetSafe(PwDefs.PasswordField).ReadUtf8();
				m_secPassword.SetPassword(pb);
				if(bSetRepeatPw) m_secRepeat.SetPassword(pb);
				MemUtil.ZeroByteArray(pb);

				m_tbUrl.Text = m_vStrings.ReadSafe(PwDefs.UrlField);
				m_rtNotes.Text = m_vStrings.ReadSafe(PwDefs.NotesField);

				int iTopVisible = UIUtil.GetTopVisibleItem(m_lvStrings);
				m_lvStrings.Items.Clear();
				foreach(KeyValuePair<string, ProtectedString> kvpStr in m_vStrings)
				{
					if(!PwDefs.IsStandardField(kvpStr.Key))
					{
						PwIcon pwIcon = (kvpStr.Value.IsProtected ? m_pwObjectProtected :
							m_pwObjectPlainText);

						ListViewItem lvi = m_lvStrings.Items.Add(kvpStr.Key, (int)pwIcon);

						if(!kvpStr.Value.IsViewable) lvi.SubItems.Add("********");
						else lvi.SubItems.Add(kvpStr.Value.ReadString());
					}
				}
				UIUtil.SetTopVisibleItem(m_lvStrings, iTopVisible);
			}
		}

		// Public for plugins
		public void UpdateEntryBinaries(bool bGuiToInternal)
		{
			if(bGuiToInternal) { }
			else // Internal to GUI
			{
				int iTopVisible = UIUtil.GetTopVisibleItem(m_lvBinaries);
				m_lvBinaries.Items.Clear();
				foreach(KeyValuePair<string, ProtectedBinary> kvpBin in m_vBinaries)
				{
					PwIcon pwIcon = (kvpBin.Value.IsProtected ? m_pwObjectProtected :
						m_pwObjectPlainText);
					m_lvBinaries.Items.Add(kvpBin.Key, (int)pwIcon);
				}
				UIUtil.SetTopVisibleItem(m_lvBinaries, iTopVisible);
			}
		}

		private static Image CreateColorButtonImage(Button btn, Color clr)
		{
			return UIUtil.CreateColorBitmap24(btn.ClientRectangle.Width - 8,
				btn.ClientRectangle.Height - 8, clr);
		}

		private void InitPropertiesTab()
		{
			m_clrForeground = m_pwEntry.ForegroundColor;
			m_clrBackground = m_pwEntry.BackgroundColor;

			if(m_clrForeground != Color.Empty)
				UIUtil.SetButtonImage(m_btnPickFgColor, CreateColorButtonImage(
					m_btnPickFgColor, m_clrForeground), false);
			if(m_clrBackground != Color.Empty)
				UIUtil.SetButtonImage(m_btnPickBgColor, CreateColorButtonImage(
					m_btnPickBgColor, m_clrBackground), false);

			m_cbCustomForegroundColor.Checked = (m_clrForeground != Color.Empty);
			m_cbCustomBackgroundColor.Checked = (m_clrBackground != Color.Empty);

			m_tbOverrideUrl.Text = m_pwEntry.OverrideUrl;
			m_tbTags.Text = StrUtil.TagsToString(m_pwEntry.Tags, true);

			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry)
			{
				m_cbCustomForegroundColor.Enabled = false;
				m_cbCustomBackgroundColor.Enabled = false;
				m_btnPickFgColor.Enabled = false;
				m_btnPickBgColor.Enabled = false;
				m_tbOverrideUrl.ReadOnly = true;
				m_tbTags.ReadOnly = true;
			}

			m_tbUuid.Text = m_pwEntry.Uuid.ToHexString();
		}

		private void InitAutoTypeTab()
		{
			m_lvAutoType.SmallImageList = m_ilIcons;

			m_cbAutoTypeEnabled.Checked = m_atConfig.Enabled;
			m_cbAutoTypeObfuscation.Checked = !(m_atConfig.ObfuscationOptions ==
				AutoTypeObfuscationOptions.None);

			string strDefaultSeq = m_atConfig.DefaultSequence;
			if(strDefaultSeq.Length > 0) m_rbAutoTypeOverride.Checked = true;
			else m_rbAutoTypeSeqInherit.Checked = true;

			if(strDefaultSeq.Length == 0)
			{
				PwGroup pg = m_pwEntry.ParentGroup;
				if(pg != null)
				{
					strDefaultSeq = pg.GetAutoTypeSequenceInherited();

					if(strDefaultSeq.Length == 0)
					{
						if(PwDefs.IsTanEntry(m_pwEntry))
							strDefaultSeq = PwDefs.DefaultAutoTypeSequenceTan;
						else
							strDefaultSeq = PwDefs.DefaultAutoTypeSequence;
					}
				}
			}
			m_tbDefaultAutoTypeSeq.Text = strDefaultSeq;

			int nWidth = m_lvAutoType.ClientRectangle.Width / 2;
			m_lvAutoType.Columns.Add(KPRes.TargetWindow, nWidth);
			m_lvAutoType.Columns.Add(KPRes.KeystrokeSequence, nWidth);

			UpdateAutoTypeList();

			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry)
			{
				m_cbAutoTypeEnabled.Enabled = m_cbAutoTypeObfuscation.Enabled =
					m_rbAutoTypeSeqInherit.Enabled =
					m_rbAutoTypeOverride.Enabled = m_btnAutoTypeAdd.Enabled =
					m_btnAutoTypeDelete.Enabled = m_btnAutoTypeEdit.Enabled = false;

				m_tbDefaultAutoTypeSeq.Enabled = m_btnAutoTypeEditDefault.Enabled =
					false;
			}
		}

		private void UpdateAutoTypeList()
		{
			m_lvAutoType.Items.Clear();

			string strDefault = "(" + KPRes.Default + ")";
			foreach(KeyValuePair<string, string> kvp in m_atConfig.WindowSequencePairs)
			{
				ListViewItem lvi = m_lvAutoType.Items.Add(kvp.Key, (int)PwIcon.List);
				lvi.SubItems.Add((kvp.Value.Length > 0) ? kvp.Value : strDefault);
			}
		}

		private void InitHistoryTab()
		{
			m_lvHistory.SmallImageList = m_ilIcons;

			m_lvHistory.Columns.Add(KPRes.Version);
			m_lvHistory.Columns.Add(KPRes.Title);
			m_lvHistory.Columns.Add(KPRes.UserName);
			m_lvHistory.Columns.Add(KPRes.Size, 72, HorizontalAlignment.Right);

			UpdateHistoryList();

			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry)
			{
				m_btnHistoryDelete.Enabled = m_btnHistoryRestore.Enabled =
					m_btnHistoryView.Enabled = false;
			}
		}

		private void UpdateHistoryList()
		{
			m_lvHistory.Items.Clear();

			foreach(PwEntry pe in m_vHistory)
			{
				ListViewItem lvi = m_lvHistory.Items.Add(TimeUtil.ToDisplayString(
					pe.LastModificationTime), (int)pe.IconId);

				lvi.SubItems.Add(pe.Strings.ReadSafe(PwDefs.TitleField));
				lvi.SubItems.Add(pe.Strings.ReadSafe(PwDefs.UserNameField));
				lvi.SubItems.Add(StrUtil.FormatDataSizeKB(pe.GetSize()));
			}
		}

		private void ResizeColumnHeaders()
		{
			Debug.Assert(m_lvStrings.Columns.Count == 2);
			int dx = m_lvStrings.ClientRectangle.Width;
			m_lvStrings.Columns[0].Width = m_lvStrings.Columns[1].Width = dx / 2;

			Debug.Assert(m_lvBinaries.Columns.Count == 1);
			dx = m_lvBinaries.ClientRectangle.Width;
			m_lvBinaries.Columns[0].Width = dx;

			Debug.Assert(m_lvAutoType.Columns.Count == 2);
			dx = m_lvAutoType.ClientRectangle.Width;
			m_lvAutoType.Columns[0].Width = m_lvAutoType.Columns[1].Width = dx / 2;

			Debug.Assert(m_lvHistory.Columns.Count == 4);
			dx = m_lvHistory.ClientRectangle.Width;
			int dt = dx / 85;
			m_lvHistory.Columns[0].Width = (dx * 2) / 7 + dt;
			m_lvHistory.Columns[1].Width = (dx * 2) / 7;
			m_lvHistory.Columns[2].Width = ((dx * 2) / 7) - (dt * 2);
			m_lvHistory.Columns[3].Width = (dx / 7) + dt;
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			Debug.Assert(m_pwEntry != null); if(m_pwEntry == null) throw new InvalidOperationException();
			Debug.Assert(m_pwEditMode != PwEditMode.Invalid); if(m_pwEditMode == PwEditMode.Invalid) throw new ArgumentException();
			Debug.Assert(m_pwDatabase != null); if(m_pwDatabase == null) throw new InvalidOperationException();
			Debug.Assert(m_ilIcons != null); if(m_ilIcons == null) throw new InvalidOperationException();

			GlobalWindowManager.AddWindow(this);
			GlobalWindowManager.CustomizeControl(m_ctxTools);
			GlobalWindowManager.CustomizeControl(m_ctxPwGen);
			GlobalWindowManager.CustomizeControl(m_ctxStrMoveToStandard);

			m_pwInitialEntry = m_pwEntry.CloneDeep();

			m_ttRect.SetToolTip(m_btnIcon, KPRes.SelectIcon);
			m_ttRect.SetToolTip(m_cbHidePassword, KPRes.TogglePasswordAsterisks);
			m_ttRect.SetToolTip(m_btnGenPw, KPRes.GeneratePassword);
			m_ttRect.SetToolTip(m_btnStandardExpires, KPRes.StandardExpireSelect);

			m_clrNormalBackColor = m_tbPassword.BackColor;
			m_dynGenProfiles = new DynamicMenu(m_ctxPwGenProfiles.DropDownItems);
			m_dynGenProfiles.MenuClick += this.OnProfilesDynamicMenuClick;
			m_ctxNotes.Attach(m_rtNotes);

			string strTitle = string.Empty, strDesc = string.Empty;
			if(m_pwEditMode == PwEditMode.AddNewEntry)
			{
				strTitle = KPRes.AddEntry;
				strDesc = KPRes.AddEntryDesc;
			}
			else if(m_pwEditMode == PwEditMode.EditExistingEntry)
			{
				strTitle = KPRes.EditEntry;
				strDesc = KPRes.EditEntryDesc;
			}
			else if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry)
			{
				strTitle = KPRes.ViewEntry;
				strDesc = KPRes.ViewEntryDesc;
			}
			else { Debug.Assert(false); }

			int w = m_bannerImage.ClientRectangle.Width;
			int h = m_bannerImage.ClientRectangle.Height;
			m_bannerImage.Image = BannerFactory.CreateBanner(w, h,
				BannerStyle.Default,
				KeePass.Properties.Resources.B48x48_KGPG_Sign,
				strTitle, strDesc);
			this.Icon = Properties.Resources.KeePass;
			this.Text = strTitle;

			UIUtil.SetButtonImage(m_btnTools,
				Properties.Resources.B16x16_Package_Settings, true);
			UIUtil.SetButtonImage(m_btnStandardExpires,
				Properties.Resources.B16x16_History, true);

			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry)
				m_bLockEnabledState = true;

			UIUtil.PrepareStandardMultilineControl(m_rtNotes, true);

			m_bInitializing = true;

			bool bForceHide = !AppPolicy.Current.UnhidePasswords;
			if(Program.Config.UI.Hiding.SeparateHidingSettings)
				m_cbHidePassword.Checked = (Program.Config.UI.Hiding.HideInEntryWindow || bForceHide);
			else
			{
				AceColumn colPw = Program.Config.MainWindow.FindColumn(AceColumnType.Password);
				m_cbHidePassword.Checked = (((colPw != null) ? colPw.HideWithAsterisks :
					true) || bForceHide);
			}

			InitEntryTab();
			InitAdvancedTab();
			InitPropertiesTab();
			InitAutoTypeTab();
			InitHistoryTab();

			UpdateEntryStrings(false, true);
			UpdateEntryBinaries(false);

			if(PwDefs.IsTanEntry(m_pwEntry))
				m_btnTools.Enabled = false;

			CustomizeForScreenReader();

			m_bInitializing = false;

			if(m_bInitSwitchToHistory) // Before 'Advanced' tab switch
				m_tabMain.SelectedTab = m_tabHistory;
			else if(m_bShowAdvancedByDefault)
				m_tabMain.SelectedTab = m_tabAdvanced;

			ResizeColumnHeaders();
			EnableControlsEx();

			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry)
				m_btnCancel.Select();
			else
			{
				if(m_bSelectFullTitle) m_tbTitle.Select(0, m_tbTitle.TextLength);
				else m_tbTitle.Select(0, 0);

				m_tbTitle.Select();
			}
		}

		private void CustomizeForScreenReader()
		{
			if(!Program.Config.UI.OptimizeForScreenReader) return;

			m_btnIcon.Text = KPRes.PickIcon;
			m_cbHidePassword.Text = KPRes.HideUsingAsterisks;
			m_btnGenPw.Text = m_ttRect.GetToolTip(m_btnGenPw);
			m_btnStandardExpires.Text = m_ttRect.GetToolTip(m_btnStandardExpires);

			m_btnPickFgColor.Text = KPRes.SelectColor;
			m_btnPickBgColor.Text = KPRes.SelectColor;
		}

		private void EnableControlsEx()
		{
			if(m_bInitializing) return;

			byte[] pb = m_secPassword.ToUtf8();
			uint uBits = QualityEstimation.EstimatePasswordBits(pb);
			MemUtil.ZeroByteArray(pb);
			m_lblQualityBitsText.Text = uBits.ToString() + " " + KPRes.Bits;
			int iPos = (int)((100 * uBits) / (256 / 2));
			if(iPos < 0) iPos = 0; else if(iPos > 100) iPos = 100;
			m_pbQuality.Value = iPos;

			bool bHidePassword = m_cbHidePassword.Checked;
			m_secPassword.EnableProtection(bHidePassword);
			m_secRepeat.EnableProtection(bHidePassword);

			if(m_bLockEnabledState) return;

			int nStringsSel = m_lvStrings.SelectedItems.Count;
			m_btnStrEdit.Enabled = (nStringsSel == 1);
			m_btnStrDelete.Enabled = (nStringsSel >= 1);

			int nBinSel = m_lvBinaries.SelectedItems.Count;
			m_btnBinSave.Enabled = m_btnBinDelete.Enabled = (nBinSel >= 1);
			m_btnBinView.Enabled = (nBinSel == 1);

			m_btnPickFgColor.Enabled = m_cbCustomForegroundColor.Checked;
			m_btnPickBgColor.Enabled = m_cbCustomBackgroundColor.Checked;

			m_lvAutoType.Enabled = m_btnAutoTypeAdd.Enabled =
				m_rbAutoTypeSeqInherit.Enabled = m_rbAutoTypeOverride.Enabled =
				m_cbAutoTypeObfuscation.Enabled = m_cbAutoTypeEnabled.Checked;

			if(!m_rbAutoTypeOverride.Checked)
				m_tbDefaultAutoTypeSeq.Enabled = m_btnAutoTypeEditDefault.Enabled = false;
			else
				m_tbDefaultAutoTypeSeq.Enabled = m_btnAutoTypeEditDefault.Enabled =
					m_cbAutoTypeEnabled.Checked;

			int nAutoTypeSel = m_lvAutoType.SelectedItems.Count;

			if(m_pwEditMode != PwEditMode.ViewReadOnlyEntry)
			{
				m_btnAutoTypeEdit.Enabled = (nAutoTypeSel == 1);
				m_btnAutoTypeDelete.Enabled = (nAutoTypeSel >= 1);
			}

			int nAccumSel = nStringsSel + nBinSel + nAutoTypeSel;
			m_menuListCtxCopyFieldValue.Enabled = (nAccumSel != 0);

			int nHistorySel = m_lvHistory.SelectedIndices.Count;
			m_btnHistoryRestore.Enabled = (nHistorySel == 1);
			m_btnHistoryDelete.Enabled = m_btnHistoryView.Enabled = (nHistorySel >= 1);

			m_menuListCtxMoveStandardTitle.Enabled = m_menuListCtxMoveStandardUser.Enabled =
				m_menuListCtxMoveStandardPassword.Enabled = m_menuListCtxMoveStandardURL.Enabled =
				m_menuListCtxMoveStandardNotes.Enabled = m_btnStrMove.Enabled =
				(nStringsSel == 1);
		}

		private bool SaveEntry()
		{
			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry) return true;

			if(this.EntrySaving != null)
			{
				CancellableOperationEventArgs eaCancel = new CancellableOperationEventArgs();
				this.EntrySaving(this, eaCancel);
				if(eaCancel.Cancel) return false;
			}

			m_pwEntry.History = m_vHistory; // Must be called before CreateBackup()
			bool bCreateBackup = (m_pwEditMode != PwEditMode.AddNewEntry);
			if(bCreateBackup) m_pwEntry.CreateBackup();

			m_pwEntry.IconId = m_pwEntryIcon;
			m_pwEntry.CustomIconUuid = m_pwCustomIconID;

			if(m_cbCustomForegroundColor.Checked)
				m_pwEntry.ForegroundColor = m_clrForeground;
			else m_pwEntry.ForegroundColor = Color.Empty;
			if(m_cbCustomBackgroundColor.Checked)
				m_pwEntry.BackgroundColor = m_clrBackground;
			else m_pwEntry.BackgroundColor = Color.Empty;

			m_pwEntry.OverrideUrl = m_tbOverrideUrl.Text;

			List<string> vNewTags = StrUtil.StringToTags(m_tbTags.Text);
			m_pwEntry.Tags.Clear();
			foreach(string strTag in vNewTags) m_pwEntry.AddTag(strTag);

			m_pwEntry.Expires = m_cbExpires.Checked;
			if(m_pwEntry.Expires) m_pwEntry.ExpiryTime = m_dtExpireDateTime.Value;

			UpdateEntryStrings(true, false);

			m_pwEntry.Strings = m_vStrings;
			m_pwEntry.Binaries = m_vBinaries;

			m_atConfig.Enabled = m_cbAutoTypeEnabled.Checked;
			m_atConfig.ObfuscationOptions = (m_cbAutoTypeObfuscation.Checked ?
				AutoTypeObfuscationOptions.UseClipboard :
				AutoTypeObfuscationOptions.None);

			if(m_rbAutoTypeSeqInherit.Checked) m_atConfig.DefaultSequence = string.Empty;
			else if(m_rbAutoTypeOverride.Checked)
				m_atConfig.DefaultSequence = m_tbDefaultAutoTypeSeq.Text;
			else { Debug.Assert(false); }

			m_pwEntry.AutoType = m_atConfig;

			m_pwEntry.Touch(true, false); // Touch *after* backup

			if(m_pwEntry.EqualsEntry(m_pwInitialEntry, false, true, true, false,
				bCreateBackup))
			{
				m_pwEntry.LastModificationTime = m_pwInitialEntry.LastModificationTime;

				if(bCreateBackup)
					m_pwEntry.History.Remove(m_pwEntry.History.GetAt(
						m_pwEntry.History.UCount - 1)); // Undo backup
			}

			if(this.EntrySaved != null) this.EntrySaved(this, EventArgs.Empty);

			return true;
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			// Immediately close if we're just viewing an entry
			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry) return;

			if(m_secPassword.ContentsEqualTo(m_secRepeat) == false)
			{
				m_bRepeatPasswordFailed = true;

				m_tbRepeatPassword.BackColor = AppDefs.ColorEditError;
				m_ttValidationError.Show(KPRes.PasswordRepeatFailed, m_tbRepeatPassword);

				this.DialogResult = DialogResult.None;
				return;
			}

			if(!SaveEntry()) this.DialogResult = DialogResult.None;
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
			m_pwEntry.Touch(false);
		}

		private void CleanUpEx()
		{
			m_dynGenProfiles.MenuClick -= this.OnProfilesDynamicMenuClick;

			if(m_pwEditMode != PwEditMode.ViewReadOnlyEntry)
				Program.Config.UI.Hiding.HideInEntryWindow = m_cbHidePassword.Checked;

			m_ctxNotes.Detach();
			m_secPassword.Detach();
			m_secRepeat.Detach();
		}

		private void OnCheckedHidePassword(object sender, EventArgs e)
		{
			if(m_bInitializing) return;

			if(!m_cbHidePassword.Checked && !AppPolicy.Try(AppPolicyId.UnhidePasswords))
			{
				m_cbHidePassword.Checked = true;
				return;
			}

			ProcessTextChangedRepeatPw(sender, e); // Clear red warning color
			EnableControlsEx();
		}

		private void ProcessTextChangedPassword(object sender, EventArgs e)
		{
			if(m_bRepeatPasswordFailed)
			{
				m_tbPassword.BackColor = m_clrNormalBackColor;
				m_tbRepeatPassword.BackColor = m_clrNormalBackColor;
				m_bRepeatPasswordFailed = false;
			}

			EnableControlsEx();
		}

		private void ProcessTextChangedRepeatPw(object sender, EventArgs e)
		{
			if(m_bRepeatPasswordFailed)
			{
				m_tbPassword.BackColor = m_clrNormalBackColor;
				m_tbRepeatPassword.BackColor = m_clrNormalBackColor;
				m_bRepeatPasswordFailed = false;
			}
		}

		private void OnBtnStrAdd(object sender, EventArgs e)
		{
			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry) return;

			UpdateEntryStrings(true, false);

			EditStringForm esf = new EditStringForm();
			esf.InitEx(m_vStrings, null, null, m_pwDatabase);

			if(esf.ShowDialog() == DialogResult.OK)
			{
				UpdateEntryStrings(false, false);
				ResizeColumnHeaders();
			}
		}

		private void OnBtnStrEdit(object sender, EventArgs e)
		{
			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry) return;

			ListView.SelectedListViewItemCollection vSel = m_lvStrings.SelectedItems;
			if(vSel.Count <= 0) return;

			UpdateEntryStrings(true, false);

			string strName = vSel[0].Text;
			ProtectedString psValue = m_vStrings.Get(strName);
			Debug.Assert(psValue != null);

			EditStringForm esf = new EditStringForm();
			esf.InitEx(m_vStrings, strName, psValue, m_pwDatabase);
			if(esf.ShowDialog() == DialogResult.OK)
				UpdateEntryStrings(false, false);
		}

		private void OnBtnStrDelete(object sender, EventArgs e)
		{
			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry) return;

			UpdateEntryStrings(true, false);

			ListView.SelectedListViewItemCollection lvsicSel = m_lvStrings.SelectedItems;
			for(int i = 0; i < lvsicSel.Count; ++i)
			{
				if(!m_vStrings.Remove(lvsicSel[i].Text)) { Debug.Assert(false); }
			}

			if(lvsicSel.Count > 0)
			{
				UpdateEntryStrings(false, false);
				ResizeColumnHeaders();
			}
		}

		private void OnBtnBinAdd(object sender, EventArgs e)
		{
			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry) return;

			OpenFileDialog ofd = UIUtil.CreateOpenFileDialog(KPRes.AttachFiles,
				UIUtil.CreateFileTypeFilter(null, null, true), 1, null, true, true);

			if(ofd.ShowDialog() == DialogResult.OK)
			{
				UpdateEntryBinaries(true);

				foreach(string strFile in ofd.FileNames)
				{
					byte[] vBytes = null;
					string strMsg, strItem = UrlUtil.GetFileName(strFile);

					if(m_vBinaries.Get(strItem) != null)
					{
						strMsg = KPRes.AttachedExistsAlready + MessageService.NewLine +
							strItem + MessageService.NewParagraph + KPRes.AttachNewRename +
							MessageService.NewParagraph + KPRes.AttachNewRenameRemarks0 +
							MessageService.NewLine + KPRes.AttachNewRenameRemarks1 +
							MessageService.NewLine + KPRes.AttachNewRenameRemarks2;

						DialogResult dr = MessageService.Ask(strMsg, null, MessageBoxButtons.YesNoCancel);

						if(dr == DialogResult.Cancel) continue;
						else if(dr == DialogResult.Yes)
						{
							string strFileName = UrlUtil.StripExtension(strItem);
							string strExtension = "." + UrlUtil.GetExtension(strItem);

							int nTry = 0;
							while(true)
							{
								string strNewName = strFileName + nTry.ToString() + strExtension;
								if(m_vBinaries.Get(strNewName) == null)
								{
									strItem = strNewName;
									break;
								}

								++nTry;
							}
						}
					}

					try
					{
						vBytes = File.ReadAllBytes(strFile);
						vBytes = DataEditorForm.ConvertAttachment(strItem, vBytes);

						if(vBytes != null)
						{
							ProtectedBinary pb = new ProtectedBinary(false, vBytes);
							m_vBinaries.Set(strItem, pb);
						}
					}
					catch(Exception exAttach)
					{
						MessageService.ShowWarning(KPRes.AttachFailed, strFile, exAttach);
					}
				}

				UpdateEntryBinaries(false);
				ResizeColumnHeaders();
			}
		}

		private void OnBtnBinDelete(object sender, EventArgs e)
		{
			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry) return;

			UpdateEntryBinaries(true);

			ListView.SelectedListViewItemCollection lvsc = m_lvBinaries.SelectedItems;
			int nSelCount = lvsc.Count;
			if(nSelCount == 0) { Debug.Assert(false); return; }

			for(int i = 0; i < nSelCount; ++i)
				m_vBinaries.Remove(lvsc[nSelCount - i - 1].Text);

			UpdateEntryBinaries(false);
			ResizeColumnHeaders();
		}

		private void OnBtnBinSave(object sender, EventArgs e)
		{
			ListView.SelectedListViewItemCollection lvsc = m_lvBinaries.SelectedItems;

			int nSelCount = lvsc.Count;
			if(nSelCount == 0) { Debug.Assert(false); return; }

			if(nSelCount == 1)
			{
				SaveFileDialog sfd = UIUtil.CreateSaveFileDialog(KPRes.AttachmentSave,
					lvsc[0].Text, UIUtil.CreateFileTypeFilter(null, null, true), 1, null, false);

				if(sfd.ShowDialog() == DialogResult.OK)
					SaveAttachmentTo(lvsc[0], sfd.FileName, false);
			}
			else // nSelCount > 1
			{
				FolderBrowserDialog fbd = UIUtil.CreateFolderBrowserDialog(KPRes.AttachmentsSave);

				if(fbd.ShowDialog() == DialogResult.OK)
				{
					string strRootPath = UrlUtil.EnsureTerminatingSeparator(fbd.SelectedPath, false);

					foreach(ListViewItem lvi in lvsc)
						SaveAttachmentTo(lvi, strRootPath + lvi.Text, true);
				}
			}
		}

		private void SaveAttachmentTo(ListViewItem lvi, string strFileName,
			bool bConfirmOverwrite)
		{
			Debug.Assert(lvi != null); if(lvi == null) throw new ArgumentNullException("lvi");
			Debug.Assert(strFileName != null); if(strFileName == null) throw new ArgumentNullException("strFileName");

			if(bConfirmOverwrite && File.Exists(strFileName))
			{
				string strMsg = KPRes.FileExistsAlready + MessageService.NewLine +
					strFileName + MessageService.NewParagraph +
					KPRes.OverwriteExistingFileQuestion;

				if(MessageService.AskYesNo(strMsg) == false)
					return;
			}

			ProtectedBinary pb = m_vBinaries.Get(lvi.Text);
			Debug.Assert(pb != null); if(pb == null) throw new ArgumentException();

			try { File.WriteAllBytes(strFileName, pb.ReadData()); }
			catch(Exception exWrite)
			{
				MessageService.ShowWarning(strFileName, exWrite);
			}
		}

		private void OnBtnAutoTypeAdd(object sender, EventArgs e)
		{
			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry) return;

			EditAutoTypeItemForm dlg = new EditAutoTypeItemForm();
			dlg.InitEx(m_atConfig, m_vStrings, null, false);

			if(dlg.ShowDialog() == DialogResult.OK)
			{
				UpdateAutoTypeList();
				ResizeColumnHeaders();
			}
		}

		private void OnBtnAutoTypeEdit(object sender, EventArgs e)
		{
			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry) return;

			EditAutoTypeItemForm dlg = new EditAutoTypeItemForm();

			ListView.SelectedListViewItemCollection lvSel = m_lvAutoType.SelectedItems;
			Debug.Assert(lvSel.Count == 1); if(lvSel.Count != 1) return;

			string strOriginalName = lvSel[0].Text;
			dlg.InitEx(m_atConfig, m_vStrings, strOriginalName, false);

			if(dlg.ShowDialog() == DialogResult.OK)
				UpdateAutoTypeList();
		}

		private void OnBtnAutoTypeDelete(object sender, EventArgs e)
		{
			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry) return;

			int j, nItemCount = m_lvAutoType.Items.Count;

			for(int i = 0; i < nItemCount; ++i)
			{
				j = nItemCount - i - 1;

				if(m_lvAutoType.Items[j].Selected)
					m_atConfig.Remove(m_lvAutoType.Items[j].Text);
			}

			UpdateAutoTypeList();
			ResizeColumnHeaders();
		}

		private void OnBtnHistoryView(object sender, EventArgs e)
		{
			Debug.Assert(m_vHistory.UCount == m_lvHistory.Items.Count);

			ListView.SelectedIndexCollection lvsi = m_lvHistory.SelectedIndices;
			if(lvsi.Count != 1) { Debug.Assert(false); return; }

			PwEntry pe = m_vHistory.GetAt((uint)lvsi[0]);
			if(pe == null) { Debug.Assert(false); return; }

			PwEntryForm pwf = new PwEntryForm();
			pwf.InitEx(pe, PwEditMode.ViewReadOnlyEntry, m_pwDatabase,
				m_ilIcons, false, false);

			pwf.ShowDialog();
		}

		private void OnBtnHistoryDelete(object sender, EventArgs e)
		{
			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry) return;

			Debug.Assert(m_vHistory.UCount == m_lvHistory.Items.Count);

			ListView.SelectedIndexCollection lvsi = m_lvHistory.SelectedIndices;
			int nSelCount = lvsi.Count;

			if(nSelCount == 0) return;

			for(int i = 0; i < lvsi.Count; ++i)
				m_vHistory.Remove(m_vHistory.GetAt((uint)lvsi[nSelCount - i - 1]));

			UpdateHistoryList();
			ResizeColumnHeaders();
		}

		private void OnBtnHistoryRestore(object sender, EventArgs e)
		{
			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry) return;

			Debug.Assert(m_vHistory.UCount == m_lvHistory.Items.Count);

			ListView.SelectedIndexCollection lvsi = m_lvHistory.SelectedIndices;
			if(lvsi.Count != 1) { Debug.Assert(false); return; }

			m_pwEntry.RestoreFromBackup((uint)lvsi[0]);
			this.DialogResult = DialogResult.OK;
		}

		private void OnHistorySelectedIndexChanged(object sender, EventArgs e)
		{
			EnableControlsEx();
		}

		private void OnStringsSelectedIndexChanged(object sender, EventArgs e)
		{
			EnableControlsEx();
		}

		private void OnBinariesSelectedIndexChanged(object sender, EventArgs e)
		{
			EnableControlsEx();
		}

		private void SetExpireDays(int nDays)
		{
			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry) return;

			m_cbExpires.Checked = true;

			DateTime dtNow = DateTime.Now;
			DateTime dtNew = dtNow.AddDays(nDays);
			m_dtExpireDateTime.Value = m_dtExpireDateTime.Value = dtNew;

			EnableControlsEx();
		}

		private void OnMenuExpireNow(object sender, EventArgs e)
		{
			SetExpireDays(0);
		}

		private void OnMenuExpire1Week(object sender, EventArgs e)
		{
			SetExpireDays(7);
		}

		private void OnMenuExpire2Weeks(object sender, EventArgs e)
		{
			SetExpireDays(14);
		}

		private void OnMenuExpire1Month(object sender, EventArgs e)
		{
			SetExpireDays(30);
		}

		private void OnMenuExpire3Months(object sender, EventArgs e)
		{
			SetExpireDays(91);
		}

		private void OnMenuExpire6Months(object sender, EventArgs e)
		{
			SetExpireDays(182);
		}

		private void OnMenuExpire1Year(object sender, EventArgs e)
		{
			SetExpireDays(365);
		}

		private void OnBtnStandardExpiresClick(object sender, EventArgs e)
		{
			m_ctxDefaultTimes.Show(m_btnStandardExpires, 0, m_btnStandardExpires.Height);
		}

		private void OnCtxCopyFieldValue(object sender, EventArgs e)
		{
			ListView.SelectedListViewItemCollection lvsc;

			if(m_lvStrings.Focused)
			{
				lvsc = m_lvStrings.SelectedItems;
				if((lvsc != null) && (lvsc.Count > 0))
				{
					string strName = lvsc[0].Text;
					ClipboardUtil.Copy(m_vStrings.GetSafe(strName), true, null, m_pwDatabase);
				}
			}
			else if(m_lvAutoType.Focused)
			{
				lvsc = m_lvAutoType.SelectedItems;
				if((lvsc != null) && (lvsc.Count > 0))
					ClipboardUtil.Copy(lvsc[0].SubItems[1].Text, true, null, m_pwDatabase);
			}
			else { Debug.Assert(false); }
		}

		private void OnBtnPickIcon(object sender, EventArgs e)
		{
			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry) return;

			IconPickerForm ipf = new IconPickerForm();
			ipf.InitEx(m_ilIcons, (uint)PwIcon.Count, m_pwDatabase,
				(uint)m_pwEntryIcon, m_pwCustomIconID);
			ipf.ShowDialog();

			if(ipf.ChosenCustomIconUuid != PwUuid.Zero) // Custom icon
			{
				m_pwCustomIconID = ipf.ChosenCustomIconUuid;
				UIUtil.SetButtonImage(m_btnIcon, m_pwDatabase.GetCustomIcon(
					m_pwCustomIconID), true);
			}
			else // Standard icon
			{
				m_pwEntryIcon = (PwIcon)ipf.ChosenIconId;
				m_pwCustomIconID = PwUuid.Zero;
				UIUtil.SetButtonImage(m_btnIcon, m_ilIcons.Images[
					(int)m_pwEntryIcon], true);
			}
		}

		private void OnAutoTypeSeqInheritCheckedChanged(object sender, EventArgs e)
		{
			EnableControlsEx();
		}

		private void OnAutoTypeEnableCheckedChanged(object sender, EventArgs e)
		{
			EnableControlsEx();
		}

		private void OnBtnAutoTypeEditDefault(object sender, EventArgs e)
		{
			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry) return;

			m_atConfig.DefaultSequence = m_tbDefaultAutoTypeSeq.Text;

			EditAutoTypeItemForm ef = new EditAutoTypeItemForm();
			ef.InitEx(m_atConfig, m_vStrings, "(" + KPRes.Default + ")", true);

			if(ef.ShowDialog() == DialogResult.OK)
				m_tbDefaultAutoTypeSeq.Text = m_atConfig.DefaultSequence;
		}

		private void OnCtxMoveToTitle(object sender, EventArgs e)
		{
			MoveSelectedStringTo(PwDefs.TitleField);
		}

		private void OnCtxMoveToUserName(object sender, EventArgs e)
		{
			MoveSelectedStringTo(PwDefs.UserNameField);
		}

		private void OnCtxMoveToPassword(object sender, EventArgs e)
		{
			MoveSelectedStringTo(PwDefs.PasswordField);
		}

		private void OnCtxMoveToURL(object sender, EventArgs e)
		{
			MoveSelectedStringTo(PwDefs.UrlField);
		}

		private void OnCtxMoveToNotes(object sender, EventArgs e)
		{
			MoveSelectedStringTo(PwDefs.NotesField);
		}

		private void MoveSelectedStringTo(string strStandardField)
		{
			ListView.SelectedListViewItemCollection lvsic = m_lvStrings.SelectedItems;
			Debug.Assert(lvsic.Count == 1); if(lvsic.Count != 1) return;

			ListViewItem lvi = lvsic[0];
			string strText = m_vStrings.ReadSafe(lvi.Text);

			if(strStandardField == PwDefs.TitleField)
			{
				if((m_tbTitle.TextLength > 0) && (strText.Length > 0))
					m_tbTitle.Text += ", ";
				m_tbTitle.Text += strText;
			}
			else if(strStandardField == PwDefs.UserNameField)
			{
				if((m_tbUserName.TextLength > 0) && (strText.Length > 0))
					m_tbUserName.Text += ", ";
				m_tbUserName.Text += strText;
			}
			else if(strStandardField == PwDefs.PasswordField)
			{
				string strPw = Encoding.UTF8.GetString(m_secPassword.ToUtf8());
				if((strPw.Length > 0) && (strText.Length > 0))
					strPw += ", ";
				m_tbPassword.Text = (strPw + strText);

				string strRep = Encoding.UTF8.GetString(m_secRepeat.ToUtf8());
				if((strRep.Length > 0) && (strText.Length > 0))
					strRep += ", ";
				m_tbRepeatPassword.Text = (strRep + strText);
			}
			else if(strStandardField == PwDefs.UrlField)
			{
				if((m_tbUrl.TextLength > 0) && (strText.Length > 0))
					m_tbUrl.Text += ", ";
				m_tbUrl.Text += strText;
			}
			else if(strStandardField == PwDefs.NotesField)
			{
				if((m_rtNotes.TextLength > 0) && (strText.Length > 0))
					m_rtNotes.Text += MessageService.NewParagraph;
				m_rtNotes.Text += strText;
			}
			else { Debug.Assert(false); }

			UpdateEntryStrings(true, false);
			m_vStrings.Remove(lvi.Text);
			UpdateEntryStrings(false, false);
			EnableControlsEx();
		}

		private void OnBtnStrMove(object sender, EventArgs e)
		{
			m_ctxStrMoveToStandard.Show(m_btnStrMove, 0, m_btnStrMove.Height);
		}

		private void OnExpireDateTimeChanged(object sender, EventArgs e)
		{
			m_cbExpires.Checked = true;
			EnableControlsEx();
		}

		private void OnNotesLinkClicked(object sender, LinkClickedEventArgs e)
		{
			WinUtil.OpenUrl(e.LinkText, m_pwEntry);
		}

		private void OnAutoTypeSelectedIndexChanged(object sender, EventArgs e)
		{
			EnableControlsEx();
		}

		private void OnAutoTypeItemActivate(object sender, EventArgs e)
		{
			OnBtnAutoTypeEdit(sender, e);
		}

		private void OnStringsItemActivate(object sender, EventArgs e)
		{
			OnBtnStrEdit(sender, e);
		}

		private void OnPwGenOpen(object sender, EventArgs e)
		{
			PwGeneratorForm pgf = new PwGeneratorForm();

			byte[] pbCurPassword = m_secPassword.ToUtf8();
			bool bAtLeastOneChar = (pbCurPassword.Length > 0);
			ProtectedString ps = new ProtectedString(true, pbCurPassword);
			Array.Clear(pbCurPassword, 0, pbCurPassword.Length);
			PwProfile opt = PwProfile.DeriveFromPassword(ps);

			pgf.InitEx(bAtLeastOneChar ? opt : null, true, false);
			if(pgf.ShowDialog() == DialogResult.OK)
			{
				byte[] pbEntropy = EntropyForm.CollectEntropyIfEnabled(pgf.SelectedProfile);
				ProtectedString psNew = new ProtectedString(true);
				PwGenerator.Generate(psNew, pgf.SelectedProfile, pbEntropy,
					Program.PwGeneratorPool);

				byte[] pbNew = psNew.ReadUtf8();
				m_secPassword.SetPassword(pbNew);
				m_secRepeat.SetPassword(pbNew);
				Array.Clear(pbNew, 0, pbNew.Length);
			}

			EnableControlsEx();
		}

		private void OnProfilesDynamicMenuClick(object sender, DynamicMenuEventArgs e)
		{
			PwProfile pwp = null;
			if(e.ItemName == DeriveFromPrevious)
			{
				byte[] pbCur = m_secPassword.ToUtf8();
				ProtectedString psCur = new ProtectedString(true, pbCur);
				Array.Clear(pbCur, 0, pbCur.Length);
				pwp = PwProfile.DeriveFromPassword(psCur);
			}
			else
			{
				foreach(PwProfile pwgo in Program.Config.PasswordGenerator.UserProfiles)
				{
					if(pwgo.Name == e.ItemName)
					{
						pwp = pwgo;
						break;
					}
				}
			}

			if(pwp != null)
			{
				ProtectedString psNew = new ProtectedString(true);
				PwGenerator.Generate(psNew, pwp, null, Program.PwGeneratorPool);
				byte[] pbNew = psNew.ReadUtf8();
				m_secPassword.SetPassword(pbNew);
				m_secRepeat.SetPassword(pbNew);
				Array.Clear(pbNew, 0, pbNew.Length);
			}
			else { Debug.Assert(false); }
		}

		private void OnPwGenClick(object sender, EventArgs e)
		{
			m_dynGenProfiles.Clear();
			m_dynGenProfiles.AddItem(DeriveFromPrevious, Properties.Resources.B16x16_CompFile);

			PwGeneratorUtil.AddStandardProfilesIfNoneAvailable();

			if(Program.Config.PasswordGenerator.UserProfiles.Count > 0)
				m_dynGenProfiles.AddSeparator();
			foreach(PwProfile pwgo in Program.Config.PasswordGenerator.UserProfiles)
			{
				if(pwgo.Name != DeriveFromPrevious)
					m_dynGenProfiles.AddItem(pwgo.Name,
						Properties.Resources.B16x16_KOrganizer);
			}

			m_ctxPwGen.Show(m_btnGenPw, new Point(0, m_btnGenPw.Height));
		}

		private void OnPickForegroundColor(object sender, EventArgs e)
		{
			Color? clr = UIUtil.ShowColorDialog(m_clrForeground);
			if(clr.HasValue)
			{
				m_clrForeground = clr.Value;
				UIUtil.SetButtonImage(m_btnPickFgColor, CreateColorButtonImage(
					m_btnPickFgColor, m_clrForeground), false);
			}
		}

		private void OnPickBackgroundColor(object sender, EventArgs e)
		{
			Color? clr = UIUtil.ShowColorDialog(m_clrBackground);
			if(clr.HasValue)
			{
				m_clrBackground = clr.Value;
				UIUtil.SetButtonImage(m_btnPickBgColor, CreateColorButtonImage(
					m_btnPickBgColor, m_clrBackground), false);
			}
		}

		private void OnCustomForegroundColorCheckedChanged(object sender, EventArgs e)
		{
			EnableControlsEx();
		}

		private void OnCustomBackgroundColorCheckedChanged(object sender, EventArgs e)
		{
			EnableControlsEx();
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			GlobalWindowManager.RemoveWindow(this);
		}

		private void OnAutoTypeObfuscationLink(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if(e.Button == MouseButtons.Left)
				AppHelp.ShowHelp(AppDefs.HelpTopics.AutoTypeObfuscation, null);
		}

		private void OnAutoTypeObfuscationCheckedChanged(object sender, EventArgs e)
		{
			if(m_bInitializing) return;

			if(m_cbAutoTypeObfuscation.Checked == false) return;

			MessageService.ShowInfo(KPRes.AutoTypeObfuscationHint,
				KPRes.DocumentationHint);
		}

		private void OnBtnBinView(object sender, EventArgs e)
		{
			ListView.SelectedListViewItemCollection lvsic = m_lvBinaries.SelectedItems;
			if((lvsic == null) || (lvsic.Count != 1)) return;

			string strDataItem = lvsic[0].Text;
			ProtectedBinary pbData = m_vBinaries.Get(strDataItem);
			if(pbData == null) return;

			DataViewerForm dvf = new DataViewerForm();
			dvf.InitEx(strDataItem, pbData.ReadData());

			dvf.ShowDialog();
		}

		private void OnBtnTools(object sender, EventArgs e)
		{
			m_ctxTools.Show(m_btnTools, 0, m_btnTools.Height);
		}

		private void OnCtxToolsHelp(object sender, EventArgs e)
		{
			if(m_tabMain.SelectedTab == m_tabAdvanced)
				AppHelp.ShowHelp(AppDefs.HelpTopics.Entry, AppDefs.HelpTopics.EntryStrings);
			else if(m_tabMain.SelectedTab == m_tabAutoType)
				AppHelp.ShowHelp(AppDefs.HelpTopics.Entry, AppDefs.HelpTopics.EntryAutoType);
			else if(m_tabMain.SelectedTab == m_tabHistory)
				AppHelp.ShowHelp(AppDefs.HelpTopics.Entry, AppDefs.HelpTopics.EntryHistory);
			else
				AppHelp.ShowHelp(AppDefs.HelpTopics.Entry, null);
		}

		private void OnCtxUrlHelp(object sender, EventArgs e)
		{
			AppHelp.ShowHelp(AppDefs.HelpTopics.UrlField, null);
		}

		private void SelectFileAsUrl(string strFilter)
		{
			string strFlt = string.Empty;
			if(strFilter != null) strFlt += strFilter;
			strFlt += KPRes.AllFiles + @" (*.*)|*.*";

			OpenFileDialog dlg = UIUtil.CreateOpenFileDialog(null, strFlt, 1, null,
				false, false);

			if(dlg.ShowDialog() == DialogResult.OK)
				m_tbUrl.Text = "cmd://\"" + dlg.FileName + "\"";
		}

		private void OnCtxUrlSelApp(object sender, EventArgs e)
		{
			SelectFileAsUrl(KPRes.Application + @" (*.exe, *.com, *.bat, *.cmd)|" +
				@"*.exe;*.com;*.bat;*.cmd|");
		}

		private void OnCtxUrlSelDoc(object sender, EventArgs e)
		{
			SelectFileAsUrl(null);
		}

		private string CreateFieldReference()
		{
			FieldRefForm dlg = new FieldRefForm();
			dlg.InitEx(m_pwDatabase.RootGroup, m_ilIcons);

			if(dlg.ShowDialog() == DialogResult.OK) return dlg.ResultReference;

			return string.Empty;
		}

		private void OnFieldRefInTitle(object sender, EventArgs e)
		{
			m_tbTitle.Text += CreateFieldReference();
		}

		private void OnFieldRefInUserName(object sender, EventArgs e)
		{
			m_tbUserName.Text += CreateFieldReference();
		}

		private void OnFieldRefInPassword(object sender, EventArgs e)
		{
			string strRef = CreateFieldReference();
			if(strRef.Length == 0) return;

			string strPw = Encoding.UTF8.GetString(m_secPassword.ToUtf8());
			string strRep = Encoding.UTF8.GetString(m_secRepeat.ToUtf8());

			m_secPassword.SetPassword(Encoding.UTF8.GetBytes(strPw + strRef));
			m_secRepeat.SetPassword(Encoding.UTF8.GetBytes(strRep + strRef));
		}

		private void OnFieldRefInUrl(object sender, EventArgs e)
		{
			m_tbUrl.Text += CreateFieldReference();
		}

		private void OnFieldRefInNotes(object sender, EventArgs e)
		{
			string strRef = CreateFieldReference();

			if(m_rtNotes.Text.Length == 0) m_rtNotes.Text = strRef;
			else m_rtNotes.Text += "\r\n" + strRef;
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if(((keyData == Keys.Return) || (keyData == Keys.Enter)) && m_rtNotes.Focused)
				return false; // Forward to RichTextBox

			return base.ProcessDialogKey(keyData);
		}

		private void OnFormClosing(object sender, FormClosingEventArgs e)
		{
			/* VistaTaskDialog dlg = new VistaTaskDialog(this.Handle);
			dlg.AddButton((int)DialogResult.Yes, KPRes.Yes, null);
			dlg.AddButton((int)DialogResult.No, KPRes.No, null);
			dlg.CommandLinks = false;
			dlg.Content = KPRes.CloseDialogWarning;
			dlg.MainInstruction = KPRes.CloseDialogConfirmation;
			dlg.VerificationText = KPRes.DialogNoShowAgain;
			dlg.WindowTitle = PwDefs.ShortProductName;

			if(dlg.ShowDialog())
			{
				if((dlg.Result == (int)DialogResult.No) ||
					(dlg.Result == (int)DialogResult.Cancel))
				{
					e.Cancel = true;
					return;
				}
			} */

			CleanUpEx();
		}

		private void OnBinariesItemActivate(object sender, EventArgs e)
		{
			OnBtnBinView(sender, e);
		}

		private void OnHistoryItemActivate(object sender, EventArgs e)
		{
			OnBtnHistoryView(sender, e);
		}
	}
}
