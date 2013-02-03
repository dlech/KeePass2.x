/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2013 Dominik Reichl <dominik.reichl@t-online.de>

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
using KeePass.Native;
using KeePass.Resources;
using KeePass.UI;
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
		AddNewEntry,
		EditExistingEntry,
		ViewReadOnlyEntry
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

		private bool m_bLockEnabledState = false;
		private bool m_bTouchedOnce = false;

		private bool m_bInitializing = false;
		private bool m_bForceClosing = false;

		private PwInputControlGroup m_icgPassword = new PwInputControlGroup();
		private ExpiryControlGroup m_cgExpiry = new ExpiryControlGroup();
		private RichTextBoxContextMenu m_ctxNotes = new RichTextBoxContextMenu();
		private Image m_imgPwGen = null;
		private Image m_imgStdExpire = null;

		private readonly string DeriveFromPrevious = "(" + KPRes.GenPwBasedOnPrevious + ")";
		private DynamicMenu m_dynGenProfiles;

		private const PwIcon m_pwObjectProtected = PwIcon.PaperLocked;
		private const PwIcon m_pwObjectPlainText = PwIcon.PaperNew;

		public event EventHandler<CancellableOperationEventArgs> EntrySaving;
		public event EventHandler EntrySaved;

		private const PwCompareOptions m_cmpOpt = (PwCompareOptions.NullEmptyEquivStd |
			PwCompareOptions.IgnoreTimes);

		public bool HasModifiedEntry
		{
			get
			{
				if((m_pwEntry == null) || (m_pwInitialEntry == null))
				{
					Debug.Assert(false);
					return true;
				}

				return !m_pwEntry.EqualsEntry(m_pwInitialEntry, m_cmpOpt,
					MemProtCmpMode.CustomOnly);
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

		public ContextMenuStrip AttachmentsContextMenu
		{
			get { return m_ctxBinAttach; }
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
			Program.Translation.ApplyTo("KeePass.Forms.PwEntryForm.m_ctxBinAttach", m_ctxBinAttach.Items);
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
			m_icgPassword.Attach(m_tbPassword, m_cbHidePassword, m_lblPasswordRepeat,
				m_tbRepeatPassword, m_lblQuality, m_pbQuality, m_lblQualityBitsText,
				this, bHideInitial, false);

			if(m_pwEntry.Expires)
			{
				m_dtExpireDateTime.Value = m_pwEntry.ExpiryTime;
				m_cbExpires.Checked = true;
			}
			else // Does not expire
			{
				m_dtExpireDateTime.Value = DateTime.Now.Date;
				m_cbExpires.Checked = false;
			}
			m_cgExpiry.Attach(m_cbExpires, m_dtExpireDateTime);

			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry)
			{
				m_tbTitle.ReadOnly = m_tbUserName.ReadOnly = m_tbPassword.ReadOnly =
					m_tbRepeatPassword.ReadOnly = m_tbUrl.ReadOnly =
					m_rtNotes.ReadOnly = true;

				m_btnIcon.Enabled = m_btnGenPw.Enabled = m_cbExpires.Enabled =
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

			nWidth = m_lvBinaries.ClientRectangle.Width / 2;
			m_lvBinaries.Columns.Add(KPRes.Attachments, nWidth);
			m_lvBinaries.Columns.Add(KPRes.Size, nWidth, HorizontalAlignment.Right);

			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry)
			{
				m_btnStrAdd.Enabled = m_btnStrEdit.Enabled =
					m_btnStrDelete.Enabled = m_btnStrMove.Enabled =
					m_btnBinAdd.Enabled = m_btnBinDelete.Enabled =
					m_btnBinView.Enabled = m_btnBinSave.Enabled = false;

				m_lvBinaries.LabelEdit = false;
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

				byte[] pb = m_icgPassword.GetPasswordUtf8();
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
				m_icgPassword.SetPassword(pb, bSetRepeatPw);
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

						if(kvpStr.Value.IsProtected)
							lvi.SubItems.Add(PwDefs.HiddenPassword);
						else
						{
							string strValue = StrUtil.MultiToSingleLine(
								kvpStr.Value.ReadString());
							lvi.SubItems.Add(strValue);
						}
					}
				}
				UIUtil.SetTopVisibleItem(m_lvStrings, iTopVisible);
			}
		}

		// Public for plugins
		public void UpdateEntryBinaries(bool bGuiToInternal)
		{
			UpdateEntryBinaries(bGuiToInternal, false, null);
		}

		public void UpdateEntryBinaries(bool bGuiToInternal, bool bUpdateState)
		{
			UpdateEntryBinaries(bGuiToInternal, bUpdateState, null);
		}

		public void UpdateEntryBinaries(bool bGuiToInternal, bool bUpdateState,
			string strFocusItem)
		{
			if(bGuiToInternal) { }
			else // Internal to GUI
			{
				int iTopVisible = UIUtil.GetTopVisibleItem(m_lvBinaries);
				m_lvBinaries.Items.Clear();
				foreach(KeyValuePair<string, ProtectedBinary> kvpBin in m_vBinaries)
				{
					PwIcon pwIcon = (kvpBin.Value.IsProtected ?
						m_pwObjectProtected : m_pwObjectPlainText);
					ListViewItem lvi = m_lvBinaries.Items.Add(kvpBin.Key, (int)pwIcon);
					lvi.SubItems.Add(StrUtil.FormatDataSizeKB(kvpBin.Value.Length));
				}
				UIUtil.SetTopVisibleItem(m_lvBinaries, iTopVisible);

				if(strFocusItem != null)
				{
					ListViewItem lvi = m_lvBinaries.FindItemWithText(strFocusItem,
						false, 0, false);
					if(lvi != null)
					{
						m_lvBinaries.EnsureVisible(lvi.Index);
						UIUtil.SetFocusedItem(m_lvBinaries, lvi, true);
					}
					else { Debug.Assert(false); }
				}
			}

			if(bUpdateState) EnableControlsEx();
		}

		internal static Image CreateColorButtonImage(Button btn, Color clr)
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
			m_lvAutoType.Columns.Add(KPRes.Sequence, nWidth);

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
			foreach(AutoTypeAssociation a in m_atConfig.Associations)
			{
				ListViewItem lvi = m_lvAutoType.Items.Add(a.WindowName, (int)PwIcon.List);
				lvi.SubItems.Add((a.Sequence.Length > 0) ? a.Sequence : strDefault);
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
			m_lvStrings.Columns[0].Width = dx / 2;
			m_lvStrings.Columns[1].Width = dx / 2;

			Debug.Assert(m_lvBinaries.Columns.Count == 2);
			dx = m_lvBinaries.ClientRectangle.Width;
			m_lvBinaries.Columns[0].Width = (dx * 4) / 5;
			m_lvBinaries.Columns[1].Width = dx / 5;

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
			StrUtil.NormalizeNewLines(m_pwInitialEntry.Strings, true);

			m_ttRect.SetToolTip(m_btnIcon, KPRes.SelectIcon);
			m_ttRect.SetToolTip(m_cbHidePassword, KPRes.TogglePasswordAsterisks);
			m_ttRect.SetToolTip(m_btnGenPw, KPRes.GeneratePassword);
			m_ttRect.SetToolTip(m_btnStandardExpires, KPRes.StandardExpireSelect);

			m_ttBalloon.SetToolTip(m_tbRepeatPassword, KPRes.PasswordRepeatHint);

			m_dynGenProfiles = new DynamicMenu(m_ctxPwGenProfiles.DropDownItems);
			m_dynGenProfiles.MenuClick += this.OnProfilesDynamicMenuClick;
			m_ctxNotes.Attach(m_rtNotes, this);

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

			BannerFactory.CreateBannerEx(this, m_bannerImage,
				KeePass.Properties.Resources.B48x48_KGPG_Sign, strTitle, strDesc);
			this.Icon = Properties.Resources.KeePass;
			this.Text = strTitle;

			m_imgPwGen = UIUtil.CreateDropDownImage(Properties.Resources.B16x16_Key_New);
			m_imgStdExpire = UIUtil.CreateDropDownImage(Properties.Resources.B16x16_History);

			UIUtil.SetButtonImage(m_btnTools,
				Properties.Resources.B16x16_Package_Settings, true);
			UIUtil.SetButtonImage(m_btnGenPw, m_imgPwGen, true);
			UIUtil.SetButtonImage(m_btnStandardExpires, m_imgStdExpire, true);

			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry)
				m_bLockEnabledState = true;

			// UIUtil.SetExplorerTheme(m_lvStrings, true);
			// UIUtil.SetExplorerTheme(m_lvBinaries, true);
			// UIUtil.SetExplorerTheme(m_lvAutoType, true);
			// UIUtil.SetExplorerTheme(m_lvHistory, true);

			UIUtil.PrepareStandardMultilineControl(m_rtNotes, true, true);

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
			UpdateEntryBinaries(false, false);

			if(PwDefs.IsTanEntry(m_pwEntry)) m_btnTools.Enabled = false;

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

			int nStringsSel = m_lvStrings.SelectedItems.Count;
			int nBinSel = m_lvBinaries.SelectedItems.Count;

			bool bBinEdit = false;
			if(nBinSel == 1)
			{
				string strBin = m_lvBinaries.SelectedItems[0].Text;
				ProtectedBinary pbSel = m_vBinaries.Get(strBin);
				if(pbSel != null)
				{
					byte[] pbBinData = pbSel.ReadData();
					BinaryDataClass bdc = BinaryDataClassifier.Classify(
						strBin, pbBinData);
					MemUtil.ZeroByteArray(pbBinData);
					if(DataEditorForm.SupportsDataType(bdc) && (m_pwEditMode !=
						PwEditMode.ViewReadOnlyEntry))
						bBinEdit = true;
				}
				else { Debug.Assert(false); }
			}
			m_btnBinView.Text = (bBinEdit ? StrUtil.RemoveAccelerator(
				KPRes.EditCmd) : KPRes.ViewCmd);

			m_btnBinView.Enabled = (nBinSel == 1);

			if(m_bLockEnabledState) return;

			m_btnStrEdit.Enabled = (nStringsSel == 1);
			m_btnStrDelete.Enabled = (nStringsSel >= 1);

			m_btnBinSave.Enabled = m_btnBinDelete.Enabled = (nBinSel >= 1);

			m_btnPickFgColor.Enabled = m_cbCustomForegroundColor.Checked;
			m_btnPickBgColor.Enabled = m_cbCustomBackgroundColor.Checked;

			bool bATEnabled = m_cbAutoTypeEnabled.Checked;
			m_lvAutoType.Enabled = m_btnAutoTypeAdd.Enabled =
				m_rbAutoTypeSeqInherit.Enabled = m_rbAutoTypeOverride.Enabled =
				m_cbAutoTypeObfuscation.Enabled = bATEnabled;

			if(!m_rbAutoTypeOverride.Checked)
				m_tbDefaultAutoTypeSeq.Enabled = m_btnAutoTypeEditDefault.Enabled = false;
			else
				m_tbDefaultAutoTypeSeq.Enabled = m_btnAutoTypeEditDefault.Enabled =
					bATEnabled;

			int nAutoTypeSel = m_lvAutoType.SelectedItems.Count;

			if(m_pwEditMode != PwEditMode.ViewReadOnlyEntry)
			{
				m_btnAutoTypeEdit.Enabled = (bATEnabled && (nAutoTypeSel == 1));
				m_btnAutoTypeDelete.Enabled = (bATEnabled && (nAutoTypeSel >= 1));
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

		private bool SaveEntry(PwEntry peTarget, bool bValidate)
		{
			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry) return true;

			if(bValidate && !m_icgPassword.ValidateData(true)) return false;

			if(this.EntrySaving != null)
			{
				CancellableOperationEventArgs eaCancel = new CancellableOperationEventArgs();
				this.EntrySaving(this, eaCancel);
				if(eaCancel.Cancel) return false;
			}

			peTarget.History = m_vHistory; // Must be called before CreateBackup()
			bool bCreateBackup = (m_pwEditMode != PwEditMode.AddNewEntry);
			if(bCreateBackup) peTarget.CreateBackup(null);

			peTarget.IconId = m_pwEntryIcon;
			peTarget.CustomIconUuid = m_pwCustomIconID;

			if(m_cbCustomForegroundColor.Checked)
				peTarget.ForegroundColor = m_clrForeground;
			else peTarget.ForegroundColor = Color.Empty;
			if(m_cbCustomBackgroundColor.Checked)
				peTarget.BackgroundColor = m_clrBackground;
			else peTarget.BackgroundColor = Color.Empty;

			peTarget.OverrideUrl = m_tbOverrideUrl.Text;

			List<string> vNewTags = StrUtil.StringToTags(m_tbTags.Text);
			peTarget.Tags.Clear();
			foreach(string strTag in vNewTags) peTarget.AddTag(strTag);

			peTarget.Expires = m_cgExpiry.Checked;
			if(peTarget.Expires) peTarget.ExpiryTime = m_cgExpiry.Value;

			UpdateEntryStrings(true, false);

			peTarget.Strings = m_vStrings;
			peTarget.Binaries = m_vBinaries;

			m_atConfig.Enabled = m_cbAutoTypeEnabled.Checked;
			m_atConfig.ObfuscationOptions = (m_cbAutoTypeObfuscation.Checked ?
				AutoTypeObfuscationOptions.UseClipboard :
				AutoTypeObfuscationOptions.None);

			SaveDefaultSeq();

			peTarget.AutoType = m_atConfig;

			peTarget.Touch(true, false); // Touch *after* backup
			if(object.ReferenceEquals(peTarget, m_pwEntry)) m_bTouchedOnce = true;

			StrUtil.NormalizeNewLines(peTarget.Strings, true);

			bool bUndoBackup = false;
			PwCompareOptions cmpOpt = m_cmpOpt;
			if(bCreateBackup) cmpOpt |= PwCompareOptions.IgnoreLastBackup;
			if(peTarget.EqualsEntry(m_pwInitialEntry, cmpOpt, MemProtCmpMode.CustomOnly))
			{
				// No modifications at all => restore last mod time and undo backup
				peTarget.LastModificationTime = m_pwInitialEntry.LastModificationTime;
				bUndoBackup = bCreateBackup;
			}
			else if(bCreateBackup)
			{
				// If only history items have been modified (deleted) => undo
				// backup, but without restoring the last mod time
				PwCompareOptions cmpOptNH = (m_cmpOpt | PwCompareOptions.IgnoreHistory);
				if(peTarget.EqualsEntry(m_pwInitialEntry, cmpOptNH, MemProtCmpMode.CustomOnly))
					bUndoBackup = true;
			}
			if(bUndoBackup) peTarget.History.RemoveAt(peTarget.History.UCount - 1);

			peTarget.MaintainBackups(m_pwDatabase);

			if(this.EntrySaved != null) this.EntrySaved(this, EventArgs.Empty);

			return true;
		}

		private void SaveDefaultSeq()
		{
			if(m_rbAutoTypeSeqInherit.Checked)
				m_atConfig.DefaultSequence = string.Empty;
			else if(m_rbAutoTypeOverride.Checked)
				m_atConfig.DefaultSequence = m_tbDefaultAutoTypeSeq.Text;
			else { Debug.Assert(false); }
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			if(SaveEntry(m_pwEntry, true)) m_bForceClosing = true;
			else this.DialogResult = DialogResult.None;
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
			m_bForceClosing = true;

			try
			{
				ushort usEsc = NativeMethods.GetAsyncKeyState((int)Keys.Escape);
				if((usEsc & 0x8000) != 0) m_bForceClosing = false;
			}
			catch(Exception) { Debug.Assert(KeePassLib.Native.NativeLib.IsUnix()); }
		}

		private void CleanUpEx()
		{
			m_dynGenProfiles.MenuClick -= this.OnProfilesDynamicMenuClick;

			if(m_pwEditMode != PwEditMode.ViewReadOnlyEntry)
				Program.Config.UI.Hiding.HideInEntryWindow = m_cbHidePassword.Checked;

			m_ctxNotes.Detach();
			m_icgPassword.Release();
			m_cgExpiry.Release();

			// Detach event handlers
			m_lvStrings.SmallImageList = null;
			m_lvBinaries.SmallImageList = null;
			m_lvAutoType.SmallImageList = null;
			m_lvHistory.SmallImageList = null;

			m_btnGenPw.Image = null;
			m_imgPwGen.Dispose();
			m_imgPwGen = null;
			m_btnStandardExpires.Image = null;
			m_imgStdExpire.Dispose();
			m_imgStdExpire = null;
		}

		private void OnBtnStrAdd(object sender, EventArgs e)
		{
			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry) return;

			UpdateEntryStrings(true, false);

			EditStringForm esf = new EditStringForm();
			esf.InitEx(m_vStrings, null, null, m_pwDatabase);

			if(UIUtil.ShowDialogAndDestroy(esf) == DialogResult.OK)
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
			if(UIUtil.ShowDialogAndDestroy(esf) == DialogResult.OK)
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
			m_ctxBinAttach.Show(m_btnBinAdd, new Point(0, m_btnBinAdd.Height));
		}

		private void OnBtnBinDelete(object sender, EventArgs e)
		{
			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry) return;

			UpdateEntryBinaries(true, false);

			ListView.SelectedListViewItemCollection lvsc = m_lvBinaries.SelectedItems;
			int nSelCount = lvsc.Count;
			if(nSelCount == 0) { Debug.Assert(false); return; }

			for(int i = 0; i < nSelCount; ++i)
				m_vBinaries.Remove(lvsc[nSelCount - i - 1].Text);

			UpdateEntryBinaries(false, true);
			ResizeColumnHeaders();
		}

		private void OnBtnBinSave(object sender, EventArgs e)
		{
			ListView.SelectedListViewItemCollection lvsc = m_lvBinaries.SelectedItems;

			int nSelCount = lvsc.Count;
			if(nSelCount == 0) { Debug.Assert(false); return; }

			if(nSelCount == 1)
			{
				SaveFileDialogEx sfd = UIUtil.CreateSaveFileDialog(KPRes.AttachmentSave,
					lvsc[0].Text, UIUtil.CreateFileTypeFilter(null, null, true), 1, null,
					AppDefs.FileDialogContext.Attachments);

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
				fbd.Dispose();
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

			byte[] pbData = pb.ReadData();
			try { File.WriteAllBytes(strFileName, pbData); }
			catch(Exception exWrite)
			{
				MessageService.ShowWarning(strFileName, exWrite);
			}
			MemUtil.ZeroByteArray(pbData);
		}

		private void OnBtnAutoTypeAdd(object sender, EventArgs e)
		{
			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry) return;

			EditAutoTypeItemForm dlg = new EditAutoTypeItemForm();
			dlg.InitEx(m_atConfig, -1, false, m_tbDefaultAutoTypeSeq.Text, m_vStrings);

			if(UIUtil.ShowDialogAndDestroy(dlg) == DialogResult.OK)
			{
				UpdateAutoTypeList();
				ResizeColumnHeaders();
			}
		}

		private void OnBtnAutoTypeEdit(object sender, EventArgs e)
		{
			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry) return;

			ListView.SelectedIndexCollection lvSel = m_lvAutoType.SelectedIndices;
			Debug.Assert(lvSel.Count == 1); if(lvSel.Count != 1) return;

			EditAutoTypeItemForm dlg = new EditAutoTypeItemForm();
			dlg.InitEx(m_atConfig, lvSel[0], false, m_tbDefaultAutoTypeSeq.Text,
				m_vStrings);

			if(UIUtil.ShowDialogAndDestroy(dlg) == DialogResult.OK)
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
					m_atConfig.RemoveAt(j);
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

			UIUtil.ShowDialogAndDestroy(pwf);
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

			m_pwEntry.RestoreFromBackup((uint)lvsi[0], m_pwDatabase);
			m_pwEntry.Touch(true, false);
			m_bTouchedOnce = true;
			this.DialogResult = DialogResult.OK; // Doesn't invoke OnBtnOK
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

		private void SetExpireIn(int nYears, int nMonths, int nDays)
		{
			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry) return;

			DateTime dt = DateTime.Now.Date;
			dt = dt.AddYears(nYears);
			dt = dt.AddMonths(nMonths);
			dt = dt.AddDays(nDays);

			DateTime dtPrevTime = m_cgExpiry.Value;
			dt = dt.AddHours(dtPrevTime.Hour);
			dt = dt.AddMinutes(dtPrevTime.Minute);
			dt = dt.AddSeconds(dtPrevTime.Second);

			m_cgExpiry.Checked = true;
			m_cgExpiry.Value = dt;

			EnableControlsEx();
		}

		private void OnMenuExpireNow(object sender, EventArgs e)
		{
			SetExpireIn(0, 0, 0);
		}

		private void OnMenuExpire1Week(object sender, EventArgs e)
		{
			SetExpireIn(0, 0, 7);
		}

		private void OnMenuExpire2Weeks(object sender, EventArgs e)
		{
			SetExpireIn(0, 0, 14);
		}

		private void OnMenuExpire1Month(object sender, EventArgs e)
		{
			SetExpireIn(0, 1, 0);
		}

		private void OnMenuExpire3Months(object sender, EventArgs e)
		{
			SetExpireIn(0, 3, 0);
		}

		private void OnMenuExpire6Months(object sender, EventArgs e)
		{
			SetExpireIn(0, 6, 0);
		}

		private void OnMenuExpire1Year(object sender, EventArgs e)
		{
			SetExpireIn(1, 0, 0);
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
					ClipboardUtil.Copy(m_vStrings.ReadSafe(strName), true, true,
						null, m_pwDatabase, this.Handle);
				}
			}
			else if(m_lvAutoType.Focused)
			{
				lvsc = m_lvAutoType.SelectedItems;
				if((lvsc != null) && (lvsc.Count > 0))
					ClipboardUtil.Copy(lvsc[0].SubItems[1].Text, true, true, null,
						m_pwDatabase, this.Handle);
			}
			else { Debug.Assert(false); }
		}

		private void OnBtnPickIcon(object sender, EventArgs e)
		{
			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry) return;

			IconPickerForm ipf = new IconPickerForm();
			ipf.InitEx(m_ilIcons, (uint)PwIcon.Count, m_pwDatabase,
				(uint)m_pwEntryIcon, m_pwCustomIconID);

			if(ipf.ShowDialog() == DialogResult.OK)
			{
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

			UIUtil.DestroyForm(ipf);
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

			SaveDefaultSeq();

			EditAutoTypeItemForm ef = new EditAutoTypeItemForm();
			ef.InitEx(m_atConfig, -1, true, m_tbDefaultAutoTypeSeq.Text, m_vStrings);

			if(UIUtil.ShowDialogAndDestroy(ef) == DialogResult.OK)
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
				string strPw = m_icgPassword.GetPassword();
				if((strPw.Length > 0) && (strText.Length > 0)) strPw += ", ";
				strPw += strText;

				string strRep = m_icgPassword.GetRepeat();
				if((strRep.Length > 0) && (strText.Length > 0)) strRep += ", ";
				strRep += strText;

				m_icgPassword.SetPasswords(strPw, strRep);
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

			byte[] pbCurPassword = m_icgPassword.GetPasswordUtf8();
			bool bAtLeastOneChar = (pbCurPassword.Length > 0);
			ProtectedString ps = new ProtectedString(true, pbCurPassword);
			Array.Clear(pbCurPassword, 0, pbCurPassword.Length);
			PwProfile opt = PwProfile.DeriveFromPassword(ps);

			pgf.InitEx(bAtLeastOneChar ? opt : null, true, false);
			// pgf.InitEx(null, true, false);

			if(pgf.ShowDialog() == DialogResult.OK)
			{
				byte[] pbEntropy = EntropyForm.CollectEntropyIfEnabled(pgf.SelectedProfile);
				ProtectedString psNew;
				PwGenerator.Generate(out psNew, pgf.SelectedProfile, pbEntropy,
					Program.PwGeneratorPool);

				byte[] pbNew = psNew.ReadUtf8();
				m_icgPassword.SetPassword(pbNew, true);
				MemUtil.ZeroByteArray(pbNew);
			}
			UIUtil.DestroyForm(pgf);

			EnableControlsEx();
		}

		private void OnProfilesDynamicMenuClick(object sender, DynamicMenuEventArgs e)
		{
			PwProfile pwp = null;
			if(e.ItemName == DeriveFromPrevious)
			{
				byte[] pbCur = m_icgPassword.GetPasswordUtf8();
				ProtectedString psCur = new ProtectedString(true, pbCur);
				MemUtil.ZeroByteArray(pbCur);
				pwp = PwProfile.DeriveFromPassword(psCur);
			}
			else
			{
				foreach(PwProfile pwgo in PwGeneratorUtil.GetAllProfiles(false))
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
				ProtectedString psNew;
				PwGenerator.Generate(out psNew, pwp, null, Program.PwGeneratorPool);
				byte[] pbNew = psNew.ReadUtf8();
				m_icgPassword.SetPassword(pbNew, true);
				MemUtil.ZeroByteArray(pbNew);
			}
			else { Debug.Assert(false); }
		}

		private void OnPwGenClick(object sender, EventArgs e)
		{
			m_dynGenProfiles.Clear();
			m_dynGenProfiles.AddItem(DeriveFromPrevious, Properties.Resources.B16x16_CompFile);

			m_dynGenProfiles.AddSeparator();
			foreach(PwProfile pwgo in PwGeneratorUtil.GetAllProfiles(true))
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
			ProtectedBinary pbinData = m_vBinaries.Get(strDataItem);
			if(pbinData == null) { Debug.Assert(false); return; }
			byte[] pbData = pbinData.ReadData();

			BinaryDataClass bdc = BinaryDataClassifier.Classify(strDataItem, pbData);
			if(DataEditorForm.SupportsDataType(bdc) && (m_pwEditMode !=
				PwEditMode.ViewReadOnlyEntry))
			{
				DataEditorForm def = new DataEditorForm();
				def.InitEx(strDataItem, pbData);
				def.ShowDialog();

				if(def.EditedBinaryData != null)
				{
					m_vBinaries.Set(strDataItem, new ProtectedBinary(
						pbinData.IsProtected, def.EditedBinaryData));
					UpdateEntryBinaries(false, true, strDataItem); // Update size
				}

				UIUtil.DestroyForm(def);
			}
			else
			{
				DataViewerForm dvf = new DataViewerForm();
				dvf.InitEx(strDataItem, pbData);
				UIUtil.ShowDialogAndDestroy(dvf);
			}
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

			OpenFileDialogEx dlg = UIUtil.CreateOpenFileDialog(null, strFlt, 1, null,
				false, AppDefs.FileDialogContext.Attachments);

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

			string strResult = string.Empty;
			if(dlg.ShowDialog() == DialogResult.OK) strResult = dlg.ResultReference;

			UIUtil.DestroyForm(dlg);
			return strResult;
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

			string strPw = m_icgPassword.GetPassword();
			string strRep = m_icgPassword.GetRepeat();
			m_icgPassword.SetPasswords(strPw + strRef, strRep + strRef);
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

		private bool m_bClosing = false; // Mono bug workaround
		private void OnFormClosing(object sender, FormClosingEventArgs e)
		{
			if(m_bClosing) return;
			m_bClosing = true;
			HandleFormClosing(e);
			m_bClosing = false;
		}

		private void HandleFormClosing(FormClosingEventArgs e)
		{
			bool bCancel = false;
			if(!m_bForceClosing && (m_pwEditMode != PwEditMode.ViewReadOnlyEntry))
			{
				PwEntry pe = m_pwInitialEntry.CloneDeep();
				SaveEntry(pe, false);

				bool bModified = !pe.EqualsEntry(m_pwInitialEntry, m_cmpOpt,
					MemProtCmpMode.CustomOnly);
				bModified |= !m_icgPassword.ValidateData(false);

				if(bModified)
				{
					DialogResult dr = MessageService.Ask(KPRes.SaveBeforeCloseQuestion,
						PwDefs.ShortProductName, MessageBoxButtons.YesNoCancel);
					if((dr == DialogResult.Yes) || (dr == DialogResult.OK))
					{
						bCancel = !SaveEntry(m_pwEntry, true);
						if(!bCancel) this.DialogResult = DialogResult.OK;
					}
					else if(dr == DialogResult.Cancel) bCancel = true;
				}
			}
			if(bCancel)
			{
				this.DialogResult = DialogResult.None;
				e.Cancel = true;
				return;
			}

			if(!m_bTouchedOnce) m_pwEntry.Touch(false, false);

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

		private void OnCtxBinImport(object sender, EventArgs e)
		{
			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry) return;

			OpenFileDialogEx ofd = UIUtil.CreateOpenFileDialog(KPRes.AttachFiles,
				UIUtil.CreateFileTypeFilter(null, null, true), 1, null, true,
				AppDefs.FileDialogContext.Attachments);

			if(ofd.ShowDialog() == DialogResult.OK)
				BinImportFiles(ofd.FileNames);
		}

		private void BinImportFiles(string[] vPaths)
		{
			if(vPaths == null) { Debug.Assert(false); return; }

			UpdateEntryBinaries(true, false);

			foreach(string strFile in vPaths)
			{
				if(string.IsNullOrEmpty(strFile)) { Debug.Assert(false); continue; }

				byte[] vBytes = null;
				string strMsg, strItem = UrlUtil.GetFileName(strFile);

				if(m_vBinaries.Get(strItem) != null)
				{
					strMsg = KPRes.AttachedExistsAlready + MessageService.NewLine +
						strItem + MessageService.NewParagraph + KPRes.AttachNewRename +
						MessageService.NewParagraph + KPRes.AttachNewRenameRemarks0 +
						MessageService.NewLine + KPRes.AttachNewRenameRemarks1 +
						MessageService.NewLine + KPRes.AttachNewRenameRemarks2;

					DialogResult dr = MessageService.Ask(strMsg, null,
						MessageBoxButtons.YesNoCancel);

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

			UpdateEntryBinaries(false, true);
			ResizeColumnHeaders();
		}

		private void OnCtxBinNew(object sender, EventArgs e)
		{
			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry) return;

			string strName;
			for(int i = 0; ; ++i)
			{
				strName = KPRes.New;
				if(i >= 1) strName += " (" + i.ToString() + ")";
				strName += ".rtf";

				if(m_vBinaries.Get(strName) == null) break;
			}

			ProtectedBinary pb = new ProtectedBinary();
			m_vBinaries.Set(strName, pb);
			UpdateEntryBinaries(false, true, strName);
			ResizeColumnHeaders();

			ListViewItem lviNew = m_lvBinaries.FindItemWithText(strName,
				false, 0, false);
			if(lviNew != null) lviNew.BeginEdit();
		}

		private void OnBinAfterLabelEdit(object sender, LabelEditEventArgs e)
		{
			string strNew = e.Label;

			e.CancelEdit = true; // In the case of success, we update it on our own

			if(string.IsNullOrEmpty(strNew)) return;
			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry) return;

			int iItem = e.Item;
			if((iItem < 0) || (iItem >= m_lvBinaries.Items.Count)) return;
			string strOld = m_lvBinaries.Items[iItem].Text;
			if(strNew == strOld) return;

			if(m_vBinaries.Get(strNew) != null)
			{
				MessageService.ShowWarning(KPRes.FieldNameExistsAlready);
				return;
			}

			ProtectedBinary pb = m_vBinaries.Get(strOld);
			if(pb == null) { Debug.Assert(false); return; }
			m_vBinaries.Remove(strOld);
			m_vBinaries.Set(strNew, pb);

			UpdateEntryBinaries(false, true, strNew);
		}

		private void BinDragAccept(DragEventArgs e)
		{
			if(e == null) { Debug.Assert(false); return; }

			IDataObject ido = e.Data;
			if((ido == null) || !ido.GetDataPresent(DataFormats.FileDrop))
				e.Effect = DragDropEffects.None;
			else e.Effect = DragDropEffects.Copy;
		}

		private void OnBinDragEnter(object sender, DragEventArgs e)
		{
			BinDragAccept(e);
		}

		private void OnBinDragOver(object sender, DragEventArgs e)
		{
			BinDragAccept(e);
		}

		private void OnBinDragDrop(object sender, DragEventArgs e)
		{
			try
			{
				BinImportFiles(e.Data.GetData(DataFormats.FileDrop) as string[]);
			}
			catch(Exception) { Debug.Assert(false); }
		}
	}
}
