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
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using KeePass.App;
using KeePass.App.Configuration;
using KeePass.Native;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;
using KeePass.Util.MultipleValues;
using KeePass.Util.Spr;

using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Cryptography;
using KeePassLib.Delegates;
using KeePassLib.Security;
using KeePassLib.Utility;

using NativeLib = KeePassLib.Native.NativeLib;

namespace KeePass.Forms
{
	public partial class PwEntryForm : Form
	{
		private PwEditMode m_pwEditMode = PwEditMode.Invalid;
		private PwDatabase m_pwDatabase = null;
		private bool m_bSelectFullTitle = false;

		private PwEntry m_pwEntry = null;
		private PwEntry m_pwInitialEntry = null;
		private ProtectedStringDictionary m_vStrings = null;
		private ProtectedBinaryDictionary m_vBinaries = null;
		private AutoTypeConfig m_atConfig = null;
		private PwObjectList<PwEntry> m_vHistory = null;
		private Color m_clrForeground = Color.Empty;
		private Color m_clrBackground = Color.Empty;
		private StringDictionaryEx m_sdCustomData = null;

		private PwIcon m_pwEntryIcon = PwIcon.Key;
		private PwUuid m_pwCustomIconID = PwUuid.Zero;
		private ImageList m_ilIcons = null;
		private List<PwUuid> m_lOrgCustomIconIDs = new List<PwUuid>();

		private bool m_bTouchedOnce = false;

		private bool m_bInitializing = true;
		private bool m_bForceClosing = false;
		private bool m_bUrlOverrideWarning = false;

		private PwInputControlGroup m_icgPassword = new PwInputControlGroup();
		private PwGeneratorMenu m_pgmPassword = null;
		private RichTextBoxContextMenu m_ctxNotes = new RichTextBoxContextMenu();
		private ExpiryControlGroup m_cgExpiry = new ExpiryControlGroup();
		private Image m_imgTools = null;
		private Image m_imgStdExpire = null;
		private Image m_imgColorFg = null;
		private Image m_imgColorBg = null;
		private List<Image> m_lOverrideUrlIcons = new List<Image>();

		private CustomContextMenuStripEx m_ctxBinOpen = null;
		private DynamicMenu m_dynBinOpen = null;

		private const PwIcon m_pwObjectProtected = PwIcon.PaperLocked;
		private const PwIcon m_pwObjectPlainText = PwIcon.PaperNew;

		private const PwCompareOptions m_cmpOpt = (PwCompareOptions.NullEmptyEquivStd |
			PwCompareOptions.IgnoreTimes);

		private enum ListSelRestore
		{
			None = 0,
			ByIndex,
			ByRef
		}

		public event EventHandler<CancellableOperationEventArgs> EntrySaving;
		public event EventHandler EntrySaved;

		public PwEditMode EditModeEx
		{
			get { return m_pwEditMode; }
		}

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

		// For backward compatibility with plugins
		[Obsolete]
		public ContextMenuStrip PasswordGeneratorContextMenu
		{
			get { return m_ctxTools; }
		}

		public ContextMenuStrip DefaultTimesContextMenu
		{
			get { return m_ctxDefaultTimes; }
		}

		public ContextMenuStrip StringsContextMenu
		{
			get { return m_ctxStr; }
		}

		public ContextMenuStrip AttachmentsContextMenu
		{
			get { return m_ctxBinAttach; }
		}

		public ContextMenuStrip AutoTypeContextMenu
		{
			get { return m_ctxAutoType; }
		}

		private PwEntryFormTab m_eftInit = PwEntryFormTab.None;
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DefaultValue((object)PwEntryFormTab.None)]
		internal PwEntryFormTab InitialTab
		{
			// get { return m_eftInit; } // Internal, uncalled
			set { m_eftInit = value; }
		}

		private MultipleValuesEntryContext m_mvec = null;
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DefaultValue((object)null)]
		public MultipleValuesEntryContext MultipleValuesEntryContext
		{
			get { return m_mvec; }
			set { m_mvec = value; }
		}

		public PwEntryForm()
		{
			InitializeComponent();

			SecureTextBoxEx.InitEx(ref m_tbPassword);
			SecureTextBoxEx.InitEx(ref m_tbRepeatPassword);

			GlobalWindowManager.InitializeForm(this);
			Program.Translation.ApplyTo("KeePass.Forms.PwEntryForm.m_ctxTools", m_ctxTools.Items);
			Program.Translation.ApplyTo("KeePass.Forms.PwEntryForm.m_ctxDefaultTimes", m_ctxDefaultTimes.Items);
			Program.Translation.ApplyTo("KeePass.Forms.PwEntryForm.m_ctxStr", m_ctxStr.Items);
			Program.Translation.ApplyTo("KeePass.Forms.PwEntryForm.m_ctxBinAttach", m_ctxBinAttach.Items);
			Program.Translation.ApplyTo("KeePass.Forms.PwEntryForm.m_ctxAutoType", m_ctxAutoType.Items);
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
			if(bShowAdvancedByDefault) m_eftInit = PwEntryFormTab.Advanced;
			m_bSelectFullTitle = bSelectFullTitle;

			m_vStrings = m_pwEntry.Strings.CloneDeep();
			NormalizeStrings(m_vStrings, pwDatabase);

			m_vBinaries = m_pwEntry.Binaries.CloneDeep();
			m_atConfig = m_pwEntry.AutoType.CloneDeep();
			m_vHistory = m_pwEntry.History.CloneDeep();

			m_lOrgCustomIconIDs.Clear();
			if(m_pwDatabase != null)
			{
				foreach(PwCustomIcon ci in m_pwDatabase.CustomIcons)
					m_lOrgCustomIconIDs.Add(ci.Uuid);
			}
		}

		private void InitEntryTab()
		{
			m_pwEntryIcon = m_pwEntry.IconId;
			m_pwCustomIconID = m_pwEntry.CustomIconUuid;

			// The user may have deleted the custom icon (using the
			// icon dialog accessible through the entry dialog and
			// then opening a history entry)
			if(!m_pwCustomIconID.Equals(PwUuid.Zero) &&
				(m_pwDatabase.GetCustomIconIndex(m_pwCustomIconID) >= 0))
			{
				// int nInx = (int)PwIcon.Count + m_pwDatabase.GetCustomIconIndex(m_pwCustomIconID);
				// if((nInx > -1) && (nInx < m_ilIcons.Images.Count))
				//	m_btnIcon.Image = m_ilIcons.Images[nInx];
				// else m_btnIcon.Image = m_ilIcons.Images[(int)m_pwEntryIcon];

				Image imgCustom = DpiUtil.GetIcon(m_pwDatabase, m_pwCustomIconID);
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
				m_tbRepeatPassword, m_lblQuality, m_pbQuality, m_lblQualityInfo,
				m_ttRect, this, bHideInitial, false);
			m_icgPassword.ContextDatabase = m_pwDatabase;
			m_icgPassword.ContextEntry = m_pwEntry;
			m_icgPassword.IsSprVariant = true;

			GFunc<PwEntry> fGetContextEntry = delegate()
			{
				return PwEntry.CreateVirtual(m_pwEntry.ParentGroup ??
					new PwGroup(true, true), m_vStrings);
			};
			m_pgmPassword = new PwGeneratorMenu(m_btnGenPw, m_ttRect, m_icgPassword,
				fGetContextEntry, m_pwDatabase, (m_mvec != null));

			m_cbQualityCheck.Image = GfxUtil.ScaleImage(Properties.Resources.B16x16_MessageBox_Info,
				DpiUtil.ScaleIntX(12), DpiUtil.ScaleIntY(12), ScaleTransformFlags.UIIcon);
			m_cbQualityCheck.Checked = m_pwEntry.QualityCheck;
			OnQualityCheckCheckedChanged(null, EventArgs.Empty);
			if((Program.Config.UI.UIFlags & (ulong)AceUIFlags.HidePwQuality) != 0)
				m_cbQualityCheck.Visible = false;

			if(m_pwEntry.Expires)
			{
				m_dtExpireDateTime.Value = TimeUtil.ToLocal(m_pwEntry.ExpiryTime, true);
				UIUtil.SetChecked(m_cbExpires, true);
			}
			else // Does not expire
			{
				m_dtExpireDateTime.Value = DateTime.Now.Date;
				UIUtil.SetChecked(m_cbExpires, false);
			}
			m_cgExpiry.Attach(m_cbExpires, m_dtExpireDateTime);

			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry)
			{
				m_tbTitle.ReadOnly = m_tbUserName.ReadOnly = m_tbPassword.ReadOnly =
					m_tbRepeatPassword.ReadOnly = m_tbUrl.ReadOnly =
					m_rtNotes.ReadOnly = true;

				UIUtil.SetEnabledFast(false, m_btnIcon, m_btnGenPw, m_cbQualityCheck,
					m_cbExpires, m_dtExpireDateTime, m_btnStandardExpires);

				// m_rtNotes.SelectAll();
				// m_rtNotes.BackColor = m_rtNotes.SelectionBackColor =
				//	AppDefs.ColorControlDisabled;
				// m_rtNotes.DeselectAll();
			}

			// Show URL in blue, if it's black in the current visual theme
			if(UIUtil.ColorsEqual(m_tbUrl.ForeColor, Color.Black))
				m_tbUrl.ForeColor = Color.Blue;
		}

		private void InitAdvancedTab()
		{
			m_lvStrings.SmallImageList = m_ilIcons;
			// m_lvBinaries.SmallImageList = m_ilIcons;

			int nWidth = m_lvStrings.ClientSize.Width / 2;
			m_lvStrings.Columns.Add(KPRes.Name, nWidth);
			m_lvStrings.Columns.Add(KPRes.Value, nWidth);

			nWidth = m_lvBinaries.ClientSize.Width / 2;
			m_lvBinaries.Columns.Add(KPRes.Attachments, nWidth);
			m_lvBinaries.Columns.Add(KPRes.Size, nWidth, HorizontalAlignment.Right);

			UIUtil.AssignShortcut(m_ctxStrCopyItem, Keys.Control | Keys.C);
			UIUtil.AssignShortcut(m_ctxStrPasteItem, Keys.Control | Keys.V);
			UIUtil.AssignShortcut(m_ctxStrSelectAll, Keys.Control | Keys.A);

			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry)
			{
				m_btnStrAdd.Enabled = false;
				m_btnStrEdit.Text = KPRes.ViewCmd;
				m_lvBinaries.LabelEdit = false;
			}
			if((m_pwEditMode == PwEditMode.ViewReadOnlyEntry) || (m_mvec != null))
				m_btnBinAdd.Enabled = false;
		}

		// Public for plugins
		public void UpdateEntryStrings(bool bGuiToInternal, bool bSetRepeatPw)
		{
			UpdateEntryStrings(bGuiToInternal, bSetRepeatPw, false);
		}

		public void UpdateEntryStrings(bool bGuiToInternal, bool bSetRepeatPw,
			bool bUpdateState)
		{
			if(bGuiToInternal)
			{
				m_vStrings.Set(PwDefs.TitleField, new ProtectedString(
					m_pwDatabase.MemoryProtection.ProtectTitle, m_tbTitle.Text));
				m_vStrings.Set(PwDefs.UserNameField, new ProtectedString(
					m_pwDatabase.MemoryProtection.ProtectUserName, m_tbUserName.Text));
				m_vStrings.Set(PwDefs.PasswordField, m_tbPassword.TextEx.WithProtection(
					m_pwDatabase.MemoryProtection.ProtectPassword));
				m_vStrings.Set(PwDefs.UrlField, new ProtectedString(
					m_pwDatabase.MemoryProtection.ProtectUrl, m_tbUrl.Text));
				m_vStrings.Set(PwDefs.NotesField, new ProtectedString(
					m_pwDatabase.MemoryProtection.ProtectNotes, m_rtNotes.Text));

				NormalizeStrings(m_vStrings, m_pwDatabase);
			}
			else // Internal to GUI
			{
				m_tbTitle.Text = m_vStrings.ReadSafe(PwDefs.TitleField);
				m_tbUserName.Text = m_vStrings.ReadSafe(PwDefs.UserNameField);

				ProtectedString ps = m_vStrings.GetSafe(PwDefs.PasswordField);
				m_icgPassword.SetPassword(ps, bSetRepeatPw);

				m_tbUrl.Text = m_vStrings.ReadSafe(PwDefs.UrlField);
				m_rtNotes.Text = m_vStrings.ReadSafe(PwDefs.NotesField);

				ProtectedString psCue = MultipleValuesEx.CueProtectedString;

				UIScrollInfo s = UIUtil.GetScrollInfo(m_lvStrings, true);
				m_lvStrings.BeginUpdate();
				m_lvStrings.Items.Clear();
				foreach(KeyValuePair<string, ProtectedString> kvp in m_vStrings)
				{
					if(PwDefs.IsStandardField(kvp.Key)) continue;

					bool bProt = kvp.Value.IsProtected;
					PwIcon pwIcon = (bProt ? m_pwObjectProtected : m_pwObjectPlainText);

					ListViewItem lvi = m_lvStrings.Items.Add(kvp.Key, (int)pwIcon);

					if((m_mvec != null) && kvp.Value.Equals(psCue, false))
					{
						lvi.SubItems.Add(MultipleValuesEx.CueString);
						MultipleValuesEx.ConfigureText(lvi, 1);
					}
					else if(bProt)
						lvi.SubItems.Add(PwDefs.HiddenPassword);
					else
						lvi.SubItems.Add(StrUtil.MultiToSingleLine(
							kvp.Value.ReadString()));
				}
				UIUtil.Scroll(m_lvStrings, s, false);
				m_lvStrings.EndUpdate();
			}

			if(bUpdateState) EnableControlsEx();
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
				UIScrollInfo s = UIUtil.GetScrollInfo(m_lvBinaries, true);
				m_lvBinaries.BeginUpdate();
				m_lvBinaries.Items.Clear();
				foreach(KeyValuePair<string, ProtectedBinary> kvpBin in m_vBinaries)
				{
					// PwIcon pwIcon = (kvpBin.Value.IsProtected ?
					//	m_pwObjectProtected : m_pwObjectPlainText);
					ListViewItem lvi = m_lvBinaries.Items.Add(kvpBin.Key); // , (int)pwIcon);
					lvi.SubItems.Add(StrUtil.FormatDataSizeKB(kvpBin.Value.Length));
				}
				FileIcons.UpdateImages(m_lvBinaries, null);
				UIUtil.Scroll(m_lvBinaries, s, false);

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

				m_lvBinaries.EndUpdate();
			}

			if(bUpdateState) EnableControlsEx();
		}

		private void InitPropertiesTab()
		{
			m_clrForeground = m_pwEntry.ForegroundColor;
			m_clrBackground = m_pwEntry.BackgroundColor;

			if(!UIUtil.ColorsEqual(m_clrForeground, Color.Empty))
				UIUtil.OverwriteButtonImage(m_btnPickFgColor, ref m_imgColorFg,
					UIUtil.CreateColorBitmap24(m_btnPickFgColor, m_clrForeground));
			if(!UIUtil.ColorsEqual(m_clrBackground, Color.Empty))
				UIUtil.OverwriteButtonImage(m_btnPickBgColor, ref m_imgColorBg,
					UIUtil.CreateColorBitmap24(m_btnPickBgColor, m_clrBackground));

			UIUtil.SetChecked(m_cbCustomForegroundColor, !UIUtil.ColorsEqual(
				m_clrForeground, Color.Empty));
			UIUtil.SetChecked(m_cbCustomBackgroundColor, !UIUtil.ColorsEqual(
				m_clrBackground, Color.Empty));

			TagUtil.MakeInheritedTagsLink(m_linkTagsInh, m_pwEntry.ParentGroup, this);
			m_tbTags.Text = StrUtil.TagsToString(m_pwEntry.Tags, true);
			TagUtil.MakeTagsButton(m_btnTags, m_tbTags, m_ttRect, m_pwEntry.ParentGroup,
				((m_pwDatabase != null) ? m_pwDatabase.RootGroup : null));

			// https://sourceforge.net/p/keepass/discussion/329220/thread/f98dece5/
			if(Program.Translation.Properties.RightToLeft)
				m_cmbOverrideUrl.RightToLeft = RightToLeft.No;
			m_cmbOverrideUrl.Text = m_pwEntry.OverrideUrl;

			m_sdCustomData = m_pwEntry.CustomData.CloneDeep();
			UIUtil.StrDictListInit(m_lvCustomData);
			UIUtil.StrDictListUpdate(m_lvCustomData, m_sdCustomData, (m_mvec != null));

#if DEBUG
			m_lvCustomData.KeyDown += delegate(object sender, KeyEventArgs e)
			{
				if(e.KeyData == (Keys.Control | Keys.F5))
				{
					UIUtil.SetHandled(e, true);
					m_sdCustomData.Set("Example_Constant", "Constant value");
					m_sdCustomData.Set("Example_Random", Program.GlobalRandom.Next().ToString());
					UIUtil.StrDictListUpdate(m_lvCustomData, m_sdCustomData, (m_mvec != null));
				}
			};
			m_lvCustomData.KeyUp += delegate(object sender, KeyEventArgs e)
			{
				if(e.KeyData == (Keys.Control | Keys.F5))
					UIUtil.SetHandled(e, true);
			};
#endif

			m_tbUuid.Text = m_pwEntry.Uuid.ToHexString() + ", " +
				Convert.ToBase64String(m_pwEntry.Uuid.UuidBytes);

			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry)
			{
				m_cbCustomForegroundColor.Enabled = false;
				m_cbCustomBackgroundColor.Enabled = false;
				m_tbTags.ReadOnly = true;
				m_btnTags.Enabled = false;
				m_cmbOverrideUrl.Enabled = false;
			}
		}

		private void InitAutoTypeTab()
		{
			UIUtil.SetChecked(m_cbAutoTypeEnabled, m_atConfig.Enabled);
			UIUtil.SetChecked(m_cbAutoTypeObfuscation, (m_atConfig.ObfuscationOptions !=
				AutoTypeObfuscationOptions.None));

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

			m_lvAutoType.SmallImageList = m_ilIcons;

			int nWidth = m_lvAutoType.ClientRectangle.Width / 2;
			m_lvAutoType.Columns.Add(KPRes.TargetWindow, nWidth);
			m_lvAutoType.Columns.Add(KPRes.Sequence, nWidth);

			UpdateAutoTypeList(ListSelRestore.None, null, false, false);

			// UIUtil.AssignShortcut(m_ctxAutoTypeCopySeq, Keys.Control | Keys.Shift | Keys.C);
			UIUtil.AssignShortcut(m_ctxAutoTypeCopyItem, Keys.Control | Keys.C);
			UIUtil.AssignShortcut(m_ctxAutoTypePasteItem, Keys.Control | Keys.V);
			UIUtil.AssignShortcut(m_ctxAutoTypeDup, Keys.Control | Keys.K);
			UIUtil.AssignShortcut(m_ctxAutoTypeSelectAll, Keys.Control | Keys.A);

			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry)
				m_cbAutoTypeEnabled.Enabled = false;
		}

		private void UpdateAutoTypeList(ListSelRestore r, AutoTypeAssociation aToSel,
			bool bScrollToEnd, bool bUpdateState)
		{
			UIScrollInfo uiScroll = (!bScrollToEnd ? UIUtil.GetScrollInfo(
				m_lvAutoType, true) : null);
			int s = m_lvAutoType.SelectedIndices.Count;

			int[] vSel = null;
			List<AutoTypeAssociation> lSel = new List<AutoTypeAssociation>();
			if(aToSel != null) lSel.Add(aToSel);

			if((r == ListSelRestore.ByIndex) && (s > 0))
			{
				vSel = new int[s];
				m_lvAutoType.SelectedIndices.CopyTo(vSel, 0);
			}
			else if(r == ListSelRestore.ByRef)
			{
				foreach(ListViewItem lviSel in m_lvAutoType.SelectedItems)
				{
					AutoTypeAssociation a = (lviSel.Tag as AutoTypeAssociation);
					if(a == null) { Debug.Assert(false); }
					else lSel.Add(a);
				}
			}

			m_lvAutoType.BeginUpdate();
			m_lvAutoType.Items.Clear();

			string strDefault = "(" + KPRes.Default + ")";
			foreach(AutoTypeAssociation a in m_atConfig.Associations)
			{
				ListViewItem lvi = m_lvAutoType.Items.Add(a.WindowName, (int)PwIcon.List);
				lvi.SubItems.Add((a.Sequence.Length > 0) ? a.Sequence : strDefault);

				lvi.Tag = a;

				foreach(AutoTypeAssociation aSel in lSel)
				{
					if(object.ReferenceEquals(a, aSel)) lvi.Selected = true;
				}
			}

			if(vSel != null)
			{
				foreach(int iSel in vSel)
					m_lvAutoType.Items[iSel].Selected = true;
			}

			if(bScrollToEnd)
			{
				int c = m_lvAutoType.Items.Count;
				if(c != 0) m_lvAutoType.EnsureVisible(c - 1);
			}
			else UIUtil.Scroll(m_lvAutoType, uiScroll, true);

			m_lvAutoType.EndUpdate();

			if(bUpdateState)
			{
				EnableControlsEx();
				ResizeColumnHeaders();
			}
		}

		private void InitHistoryTab()
		{
			m_lblCreatedData.Text = TimeUtil.ToDisplayString(m_pwEntry.CreationTime);
			m_lblModifiedData.Text = TimeUtil.ToDisplayString(m_pwEntry.LastModificationTime);

			m_lvHistory.SmallImageList = m_ilIcons;

			m_lvHistory.Columns.Add(KPRes.Version);
			m_lvHistory.Columns.Add(KPRes.Title);
			m_lvHistory.Columns.Add(KPRes.UserName);
			m_lvHistory.Columns.Add(KPRes.Size, 72, HorizontalAlignment.Right);

			UpdateHistoryList(false);

			if((m_pwEditMode == PwEditMode.ViewReadOnlyEntry) || (m_mvec != null))
			{
				m_lblPrev.Enabled = false;
				m_lvHistory.Enabled = false;
			}
		}

		private void UpdateHistoryList(bool bUpdateState)
		{
			UIScrollInfo s = UIUtil.GetScrollInfo(m_lvHistory, true);

			ImageList ilIcons = m_lvHistory.SmallImageList;
			int ci = ((ilIcons != null) ? ilIcons.Images.Count : 0);

			m_lvHistory.BeginUpdate();
			m_lvHistory.Items.Clear();

			foreach(PwEntry pe in m_vHistory)
			{
				ListViewItem lvi = m_lvHistory.Items.Add(TimeUtil.ToDisplayString(
					pe.LastModificationTime));

				int idxIcon = (int)pe.IconId;
				PwUuid pu = pe.CustomIconUuid;
				if(!pu.Equals(PwUuid.Zero))
				{
					// The user may have deleted the custom icon (using
					// the icon dialog accessible through this entry
					// dialog); continuing to show the deleted custom
					// icon would be confusing
					int idxNew = m_pwDatabase.GetCustomIconIndex(pu);
					if(idxNew >= 0) // Icon not deleted
					{
						int idxOrg = m_lOrgCustomIconIDs.IndexOf(pu);
						if(idxOrg >= 0) idxIcon = (int)PwIcon.Count + idxOrg;
						else { Debug.Assert(false); }
					}
				}
				if(idxIcon < ci) lvi.ImageIndex = idxIcon;
				else { Debug.Assert(false); }

				lvi.SubItems.Add(pe.Strings.ReadSafe(PwDefs.TitleField));
				lvi.SubItems.Add(pe.Strings.ReadSafe(PwDefs.UserNameField));
				lvi.SubItems.Add(StrUtil.FormatDataSizeKB(pe.GetSize()));
			}

			UIUtil.Scroll(m_lvHistory, s, true);
			m_lvHistory.EndUpdate();

			if(bUpdateState) EnableControlsEx();
		}

		private void ResizeColumnHeaders()
		{
			UIUtil.ResizeColumns(m_lvStrings, true);
			UIUtil.ResizeColumns(m_lvBinaries, new int[] { 4, 1 }, true);
			UIUtil.ResizeColumns(m_lvAutoType, true);
			UIUtil.ResizeColumns(m_lvHistory, new int[] { 21, 20, 18, 11 }, true);
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			if(m_pwEntry == null) { Debug.Assert(false); throw new InvalidOperationException(); }
			if(m_pwEditMode == PwEditMode.Invalid) { Debug.Assert(false); throw new InvalidOperationException(); }
			if(m_pwDatabase == null) { Debug.Assert(false); throw new InvalidOperationException(); }
			if(m_ilIcons == null) { Debug.Assert(false); throw new InvalidOperationException(); }

			m_bInitializing = true;

			// If there is an intermediate form, the custom icons
			// in the image list may be outdated
			Form fTop = GlobalWindowManager.TopWindow;
			Debug.Assert(fTop != this); // Before adding ourself
			if((fTop != null) && (fTop != Program.MainForm))
				m_lOrgCustomIconIDs.Clear();

			GlobalWindowManager.AddWindow(this);

			m_pwInitialEntry = m_pwEntry.CloneDeep();
			NormalizeStrings(m_pwInitialEntry.Strings, m_pwDatabase);

			UIUtil.ConfigureToolTip(m_ttRect);
			UIUtil.SetToolTip(m_ttRect, m_btnIcon, KPRes.SelectIcon, true);
			UIUtil.SetToolTip(m_ttRect, m_cbQualityCheck, KPRes.QualityCheckToggle, true);
			UIUtil.SetToolTip(m_ttRect, m_btnStandardExpires, KPRes.StandardExpireSelect, true);
			UIUtil.SetToolTip(m_ttRect, m_btnAutoTypeUp, KPRes.MoveUp, true);
			UIUtil.SetToolTip(m_ttRect, m_btnAutoTypeDown, KPRes.MoveDown, true);

			UIUtil.ConfigureToolTip(m_ttBalloon);
			UIUtil.SetToolTip(m_ttBalloon, m_tbRepeatPassword, KPRes.PasswordRepeatHint, false);

			UIUtil.AccSetName(m_dtExpireDateTime, m_cbExpires);
			UIUtil.AccSetName(m_btnPickFgColor, KPRes.SelectColor);
			UIUtil.AccSetName(m_btnPickBgColor, KPRes.SelectColor);
			UIUtil.AccSetName(m_tbDefaultAutoTypeSeq, m_rbAutoTypeOverride);

			m_ctxNotes.Attach(m_rtNotes, this);

			m_ctxBinOpen = new CustomContextMenuStripEx();
			m_ctxBinOpen.Opening += this.OnCtxBinOpenOpening;
			m_dynBinOpen = new DynamicMenu(m_ctxBinOpen.Items);
			m_dynBinOpen.MenuClick += this.OnDynBinOpen;
			m_btnBinOpen.SplitDropDownMenu = m_ctxBinOpen;

			string strTitle = string.Empty, strDesc = string.Empty;
			switch(m_pwEditMode)
			{
				case PwEditMode.AddNewEntry:
					strTitle = KPRes.AddEntry;
					strDesc = KPRes.AddEntryDesc;
					break;
				case PwEditMode.EditExistingEntry:
					if(m_mvec != null)
					{
						strTitle = KPRes.EditEntries + " (" +
							m_mvec.Entries.Length.ToString() + ")";
						strDesc = KPRes.EditEntriesDesc;
					}
					else
					{
						strTitle = KPRes.EditEntry;
						strDesc = KPRes.EditEntryDesc;
					}
					break;
				case PwEditMode.ViewReadOnlyEntry:
					strTitle = KPRes.ViewEntryReadOnly;
					strDesc = KPRes.ViewEntryDesc;
					break;
				default:
					Debug.Assert(false);
					break;
			}

			BannerFactory.CreateBannerEx(this, m_bannerImage,
				KeePass.Properties.Resources.B48x48_KGPG_Sign, strTitle, strDesc);
			this.Icon = AppIcons.Default;
			this.Text = strTitle;

			// m_btnTools.Text += " \u23F7 \u25BC \u25BE \u2BC6 \uD83D\uDF83";
			// m_btnTools.Width += DpiUtil.ScaleIntX(60);

			m_imgStdExpire = UIUtil.CreateDropDownImage(Properties.Resources.B16x16_History);

			Image imgOrg = Properties.Resources.B16x16_Package_Settings;
			Image imgSc = UIUtil.SetButtonImage(m_btnTools, imgOrg, true);
			if(!object.ReferenceEquals(imgOrg, imgSc))
				m_imgTools = imgSc; // Only dispose scaled image
			
			imgSc = UIUtil.SetButtonImage(m_btnStandardExpires, m_imgStdExpire, true);
			UIUtil.OverwriteIfNotEqual(ref m_imgStdExpire, imgSc);

			// UIUtil.SetExplorerTheme(m_lvStrings, true);
			// UIUtil.SetExplorerTheme(m_lvBinaries, true);
			// UIUtil.SetExplorerTheme(m_lvCustomData, true);
			// UIUtil.SetExplorerTheme(m_lvAutoType, true);
			// UIUtil.SetExplorerTheme(m_lvHistory, true);

			UIUtil.PrepareStandardMultilineControl(m_rtNotes, true, true);

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

			UpdateEntryStrings(false, true, false);
			UpdateEntryBinaries(false, false);

			if(m_mvec != null)
			{
				MultipleValuesEx.ConfigureText(m_tbTitle, true);
				MultipleValuesEx.ConfigureText(m_tbUserName, true);
				MultipleValuesEx.ConfigureText(m_tbPassword, true);
				MultipleValuesEx.ConfigureText(m_tbRepeatPassword, true);
				m_cbQualityCheck.Enabled = false;
				MultipleValuesEx.ConfigureText(m_tbUrl, true);
				MultipleValuesEx.ConfigureText(m_rtNotes, true);
				if(m_mvec.MultiExpiry)
					MultipleValuesEx.ConfigureState(m_cbExpires, true);

				UIUtil.SetEnabledFast(false, m_grpAttachments, m_lvBinaries);

				if(m_mvec.MultiFgColor)
					MultipleValuesEx.ConfigureState(m_cbCustomForegroundColor, true);
				if(m_mvec.MultiBgColor)
					MultipleValuesEx.ConfigureState(m_cbCustomBackgroundColor, true);
				MultipleValuesEx.ConfigureText(m_tbTags, true);
				MultipleValuesEx.ConfigureText(m_cmbOverrideUrl, true);
				m_tbUuid.Text = MultipleValuesEx.CueString;
				UIUtil.SetEnabledFast(false, m_lblUuid, m_tbUuid);

				if(m_mvec.MultiAutoTypeEnabled)
					MultipleValuesEx.ConfigureState(m_cbAutoTypeEnabled, true);
				MultipleValuesEx.ConfigureText(m_tbDefaultAutoTypeSeq, true);
				if(m_mvec.MultiAutoTypeObf)
					MultipleValuesEx.ConfigureState(m_cbAutoTypeObfuscation, true);

				m_lblCreatedData.Text = MultipleValuesEx.CueString;
				UIUtil.SetEnabledFast(false, m_lblCreated, m_lblCreatedData);
				m_lblModifiedData.Text = MultipleValuesEx.CueString;
				UIUtil.SetEnabledFast(false, m_lblModified, m_lblModifiedData);
			}

			m_bInitializing = false;

			ResizeColumnHeaders();
			EnableControlsEx();

			ThreadPool.QueueUserWorkItem(delegate(object state)
			{
				try
				{
					InitUserNameSuggestions();
					InitOverridesBox();

					string[] vSeq = m_pwDatabase.RootGroup.GetAutoTypeSequences(true);
					// Do not append, because long suggestions hide the start
					UIUtil.EnableAutoCompletion(m_tbDefaultAutoTypeSeq,
						false, vSeq); // Invokes
				}
				catch(Exception) { Debug.Assert(false); }
			});

			if(MonoWorkarounds.IsRequired(2140)) Application.DoEvents();

			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry)
			{
				UIUtil.SetEnabledFast(false, m_ctxToolsUrlSelApp, m_ctxToolsUrlSelDoc,
					m_ctxToolsFieldRefs, m_ctxToolsFieldRefsInTitle,
					m_ctxToolsFieldRefsInUserName, m_ctxToolsFieldRefsInPassword,
					m_ctxToolsFieldRefsInUrl, m_ctxToolsFieldRefsInNotes);

				UIUtil.SetFocus(m_btnCancel, this);
				m_btnOK.Enabled = false;
			}
			else
			{
				if(m_bSelectFullTitle) m_tbTitle.Select(0, m_tbTitle.TextLength);
				else m_tbTitle.Select(0, 0);

				UIUtil.SetFocus(m_tbTitle, this);
			}

			switch(m_eftInit)
			{
				case PwEntryFormTab.Advanced:
					m_tabMain.SelectedTab = m_tabAdvanced; break;
				case PwEntryFormTab.Properties:
					m_tabMain.SelectedTab = m_tabProperties; break;
				case PwEntryFormTab.AutoType:
					m_tabMain.SelectedTab = m_tabAutoType; break;
				case PwEntryFormTab.History:
					m_tabMain.SelectedTab = m_tabHistory; break;
				default: break;
			}
		}

		private void EnableControlsEx()
		{
			if(m_bInitializing) return;

			bool bEdit = (m_pwEditMode != PwEditMode.ViewReadOnlyEntry);
			bool bMulti = (m_mvec != null);

			int nStrings = m_lvStrings.Items.Count;
			int nStringsSel = m_lvStrings.SelectedIndices.Count;
			int nBinSel = m_lvBinaries.SelectedIndices.Count;
			int nAutoType = m_lvAutoType.Items.Count;
			int nAutoTypeSel = m_lvAutoType.SelectedIndices.Count;
			int nHistorySel = m_lvHistory.SelectedIndices.Count;

			bool bAutoType = (m_cbAutoTypeEnabled.CheckState != CheckState.Unchecked);
			bool bAutoTypeCustomEdit = (bEdit && !bMulti && bAutoType);

			m_btnStrEdit.Enabled = (nStringsSel == 1); // Supports read-only
			m_btnStrDelete.Enabled = (bEdit && (nStringsSel >= 1));
			UIUtil.SetEnabledFast((nStringsSel >= 1), m_ctxStrCopyName,
				m_ctxStrCopyValue, m_ctxStrCopyItem);
			m_ctxStrSelectAll.Enabled = (nStrings != 0);
			UIUtil.SetEnabledFast((bEdit && !bMulti && (nStringsSel >= 1)),
				m_ctxStrMoveTo, m_ctxStrMoveToTitle, m_ctxStrMoveToUserName,
				m_ctxStrMoveToPassword, m_ctxStrMoveToUrl, m_ctxStrMoveToNotes);

			m_btnBinDelete.Enabled = (bEdit && (nBinSel >= 1));
			m_btnBinOpen.Enabled = (nBinSel == 1);
			m_btnBinSave.Enabled = (nBinSel >= 1);

			m_btnPickFgColor.Enabled = (bEdit &&
				(m_cbCustomForegroundColor.CheckState == CheckState.Checked));
			m_btnPickBgColor.Enabled = (bEdit &&
				(m_cbCustomBackgroundColor.CheckState == CheckState.Checked));

			bool bUrlEmpty = (m_tbUrl.TextLength == 0);
			bool bUrlOverrideEmpty = (m_cmbOverrideUrl.Text.Length == 0);
			bool bWarn = (bUrlEmpty && !bUrlOverrideEmpty);
			if(bWarn != m_bUrlOverrideWarning)
			{
				if(bWarn) m_cmbOverrideUrl.BackColor = AppDefs.ColorEditError;
				else m_cmbOverrideUrl.ResetBackColor();

				UIUtil.SetToolTip(m_ttBalloon, m_cmbOverrideUrl, (bWarn ?
					KPRes.UrlFieldEmptyFirstTab : string.Empty), false);

				m_bUrlOverrideWarning = bWarn;
			}

			m_btnCDDel.Enabled = (bEdit && (m_lvCustomData.SelectedIndices.Count > 0));

			UIUtil.SetEnabledFast((bEdit && bAutoType), m_rbAutoTypeSeqInherit,
				m_rbAutoTypeOverride);
			UIUtil.SetEnabledFast((bEdit && bAutoType && m_rbAutoTypeOverride.Checked),
				m_tbDefaultAutoTypeSeq, m_btnAutoTypeEditDefault);

			UIUtil.SetEnabledFast((!bMulti && bAutoType), m_lblCustomAutoType,
				m_lvAutoType, m_btnAutoTypeMore);
			m_btnAutoTypeAdd.Enabled = bAutoTypeCustomEdit;
			m_btnAutoTypeEdit.Enabled = (bAutoTypeCustomEdit && (nAutoTypeSel == 1));
			UIUtil.SetEnabledFast((bAutoTypeCustomEdit && (nAutoTypeSel >= 1)),
				m_btnAutoTypeDelete, m_btnAutoTypeUp, m_btnAutoTypeDown);
			UIUtil.SetEnabledFast((nAutoTypeSel >= 1), m_ctxAutoTypeCopyWnd,
				m_ctxAutoTypeCopySeq, m_ctxAutoTypeCopyItem);
			m_ctxAutoTypeDup.Enabled = (bAutoTypeCustomEdit && (nAutoTypeSel >= 1));
			m_ctxAutoTypeSelectAll.Enabled = (nAutoType != 0);

			m_cbAutoTypeObfuscation.Enabled = (bEdit && bAutoType);

			UIUtil.SetEnabledFast((bEdit && !bMulti && (nHistorySel == 1)),
				m_btnHistoryView, m_btnHistoryRestore);
			m_btnHistoryDelete.Enabled = (bEdit && !bMulti && (nHistorySel >= 1));
		}

		private bool SaveEntry(PwEntry peTarget, bool bValidate)
		{
			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry) { Debug.Assert(false); return true; }

			if(bValidate && !m_icgPassword.ValidateData(true)) return false;

			bool bPri = object.ReferenceEquals(peTarget, m_pwEntry);

			if((this.EntrySaving != null) && bPri)
			{
				CancellableOperationEventArgs e = new CancellableOperationEventArgs();
				this.EntrySaving(this, e);
				if(e.Cancel) return false;
			}

			peTarget.History = m_vHistory; // Must be assigned before CreateBackup()
			// Create a backup only if bPri, because it modifies m_vHistory;
			// https://sourceforge.net/p/keepass/bugs/2062/
			bool bCreateBackup = ((m_pwEditMode != PwEditMode.AddNewEntry) && bPri);
			if(bCreateBackup) peTarget.CreateBackup(null);

			UpdateEntryStrings(true, false, false);
			peTarget.Strings = m_vStrings;

			peTarget.IconId = m_pwEntryIcon;
			peTarget.CustomIconUuid = m_pwCustomIconID;

			peTarget.QualityCheck = m_cbQualityCheck.Checked;

			peTarget.Expires = m_cgExpiry.Checked;
			if(peTarget.Expires) peTarget.ExpiryTime = m_cgExpiry.Value;

			peTarget.Binaries = m_vBinaries;

			peTarget.ForegroundColor = (m_cbCustomForegroundColor.Checked ?
				m_clrForeground : Color.Empty);
			peTarget.BackgroundColor = (m_cbCustomBackgroundColor.Checked ?
				m_clrBackground : Color.Empty);

			peTarget.Tags = StrUtil.StringToTags(m_tbTags.Text);
			peTarget.OverrideUrl = m_cmbOverrideUrl.Text;
			peTarget.CustomData = m_sdCustomData;

			m_atConfig.Enabled = m_cbAutoTypeEnabled.Checked;
			SaveDefaultSeq();
			m_atConfig.ObfuscationOptions = (m_cbAutoTypeObfuscation.Checked ?
				AutoTypeObfuscationOptions.UseClipboard : AutoTypeObfuscationOptions.None);
			peTarget.AutoType = m_atConfig;

			peTarget.Touch(true, false); // Touch *after* backup
			if(bPri) m_bTouchedOnce = true;

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

			if(bPri) peTarget.MaintainBackups(m_pwDatabase);

			if(m_mvec != null)
			{
				m_mvec.MultiExpiry = (m_cbExpires.CheckState == CheckState.Indeterminate);
				m_mvec.MultiFgColor = (m_cbCustomForegroundColor.CheckState == CheckState.Indeterminate);
				m_mvec.MultiBgColor = (m_cbCustomBackgroundColor.CheckState == CheckState.Indeterminate);
				m_mvec.MultiAutoTypeEnabled = (m_cbAutoTypeEnabled.CheckState == CheckState.Indeterminate);
				m_mvec.MultiAutoTypeObf = (m_cbAutoTypeObfuscation.CheckState == CheckState.Indeterminate);
			}

			if((this.EntrySaved != null) && bPri)
				this.EntrySaved(this, EventArgs.Empty);

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
			catch(Exception) { Debug.Assert(NativeLib.IsUnix()); }
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			if(!m_bTouchedOnce) m_pwEntry.Touch(false, false);

			if(m_pwEditMode != PwEditMode.ViewReadOnlyEntry)
				Program.Config.UI.Hiding.HideInEntryWindow = m_cbHidePassword.Checked;

			m_icgPassword.Release();

			if(m_pgmPassword != null)
			{
				m_pgmPassword.Dispose();
				m_pgmPassword = null;
			}
			else { Debug.Assert(false); }

			m_ctxNotes.Detach();
			m_cgExpiry.Release();

			m_btnBinOpen.SplitDropDownMenu = null;
			m_dynBinOpen.MenuClick -= this.OnDynBinOpen;
			m_dynBinOpen.Clear();
			m_ctxBinOpen.Opening -= this.OnCtxBinOpenOpening;
			m_ctxBinOpen.Dispose();

			m_cmbOverrideUrl.OrderedImageList = null;
			foreach(Image img in m_lOverrideUrlIcons)
			{
				if(img != null) img.Dispose();
			}
			m_lOverrideUrlIcons.Clear();

			// Detach event handlers
			m_lvStrings.SmallImageList = null;
			// m_lvBinaries.SmallImageList = null;
			FileIcons.ReleaseImages(m_lvBinaries);
			m_lvAutoType.SmallImageList = null;
			m_lvHistory.SmallImageList = null;

			if(m_imgTools != null) // Only dispose scaled image
				UIUtil.DisposeButtonImage(m_btnTools, ref m_imgTools);

			UIUtil.DisposeButtonImage(m_btnStandardExpires, ref m_imgStdExpire);
			UIUtil.DisposeButtonImage(m_btnPickFgColor, ref m_imgColorFg);
			UIUtil.DisposeButtonImage(m_btnPickBgColor, ref m_imgColorBg);

			GlobalWindowManager.RemoveWindow(this);
		}

		private void OnBtnStrAdd(object sender, EventArgs e)
		{
			UpdateEntryStrings(true, false, false);

			EditStringForm esf = new EditStringForm();
			esf.InitEx(m_vStrings, null, null, m_pwDatabase);

			if(UIUtil.ShowDialogAndDestroy(esf) == DialogResult.OK)
			{
				UpdateEntryStrings(false, false, true);
				ResizeColumnHeaders();
			}
		}

		private void OnBtnStrEdit(object sender, EventArgs e)
		{
			ListView.SelectedListViewItemCollection vSel = m_lvStrings.SelectedItems;
			if(vSel.Count <= 0) return;

			UpdateEntryStrings(true, false, false);

			string strName = vSel[0].Text;
			ProtectedString psValue = m_vStrings.Get(strName);
			if(psValue == null) { Debug.Assert(false); return; }

			EditStringForm esf = new EditStringForm();
			esf.InitEx(m_vStrings, strName, psValue, m_pwDatabase);
			esf.ReadOnlyEx = (m_pwEditMode == PwEditMode.ViewReadOnlyEntry);
			esf.MultipleValuesEntryContext = m_mvec;

			if(UIUtil.ShowDialogAndDestroy(esf) == DialogResult.OK)
				UpdateEntryStrings(false, false, true);
		}

		private void OnBtnStrDelete(object sender, EventArgs e)
		{
			UpdateEntryStrings(true, false, false);

			ListView.SelectedListViewItemCollection lvsc = m_lvStrings.SelectedItems;
			foreach(ListViewItem lvi in lvsc)
			{
				if(!m_vStrings.Remove(lvi.Text)) { Debug.Assert(false); }
			}

			if(lvsc.Count > 0)
			{
				UpdateEntryStrings(false, false, true);
				ResizeColumnHeaders();
			}
		}

		private void OnBtnBinAdd(object sender, EventArgs e)
		{
			m_ctxBinAttach.ShowEx(m_btnBinAdd);
		}

		private void OnBtnBinDelete(object sender, EventArgs e)
		{
			UpdateEntryBinaries(true, false);

			ListView.SelectedListViewItemCollection lvsc = m_lvBinaries.SelectedItems;
			foreach(ListViewItem lvi in lvsc)
			{
				if(!m_vBinaries.Remove(lvi.Text)) { Debug.Assert(false); }
			}

			if(lvsc.Count > 0)
			{
				UpdateEntryBinaries(false, true);
				ResizeColumnHeaders();
			}
		}

		private void OnBtnBinSave(object sender, EventArgs e)
		{
			ListView.SelectedListViewItemCollection lvsc = m_lvBinaries.SelectedItems;

			int nSelCount = lvsc.Count;
			if(nSelCount == 0) { Debug.Assert(false); return; }

			if(nSelCount == 1)
			{
				SaveFileDialogEx sfd = UIUtil.CreateSaveFileDialog(KPRes.AttachmentSave,
					UrlUtil.GetSafeFileName(lvsc[0].Text), UIUtil.CreateFileTypeFilter(
					null, null, true), 1, null, AppDefs.FileDialogContext.Attachments);

				if(sfd.ShowDialog() == DialogResult.OK)
					SaveAttachmentTo(lvsc[0], sfd.FileName, false);
			}
			else // nSelCount > 1
			{
				FolderBrowserDialog fbd = UIUtil.CreateFolderBrowserDialog(KPRes.AttachmentsSave);

				if(fbd.ShowDialog() == DialogResult.OK)
				{
					string strRootPath = UrlUtil.EnsureTerminatingSeparator(
						fbd.SelectedPath, false);

					foreach(ListViewItem lvi in lvsc)
						SaveAttachmentTo(lvi, strRootPath + UrlUtil.GetSafeFileName(
							lvi.Text), true);
				}
				fbd.Dispose();
			}
		}

		private void SaveAttachmentTo(ListViewItem lvi, string strFile,
			bool bConfirmOverwrite)
		{
			if(lvi == null) { Debug.Assert(false); return; }
			if(string.IsNullOrEmpty(strFile)) { Debug.Assert(false); return; }

			if(bConfirmOverwrite && File.Exists(strFile))
			{
				string strMsg = KPRes.FileExistsAlready + MessageService.NewLine +
					strFile + MessageService.NewParagraph +
					KPRes.OverwriteExistingFileQuestion;

				if(!MessageService.AskYesNo(strMsg)) return;
			}

			ProtectedBinary pb = m_vBinaries.Get(lvi.Text);
			if(pb == null) { Debug.Assert(false); return; }

			byte[] pbData = pb.ReadData();
			try { File.WriteAllBytes(strFile, pbData); }
			catch(Exception exWrite)
			{
				MessageService.ShowWarning(strFile, exWrite);
			}
			if(pb.IsProtected) MemUtil.ZeroByteArray(pbData);
		}

		private void OnBtnAutoTypeAdd(object sender, EventArgs e)
		{
			EditAutoTypeItemForm dlg = new EditAutoTypeItemForm();
			dlg.InitEx(m_atConfig, -1, false, m_tbDefaultAutoTypeSeq.Text, m_vStrings);

			if(UIUtil.ShowDialogAndDestroy(dlg) == DialogResult.OK)
			{
				AutoTypeAssociation a = null;
				if(m_atConfig.AssociationsCount > 0)
					a = m_atConfig.GetAt(m_atConfig.AssociationsCount - 1);
				else { Debug.Assert(false); }

				UpdateAutoTypeList(ListSelRestore.None, a, true, true);
			}
		}

		private void OnBtnAutoTypeEdit(object sender, EventArgs e)
		{
			ListView.SelectedIndexCollection lvsic = m_lvAutoType.SelectedIndices;
			if(lvsic.Count != 1) { Debug.Assert(false); return; }

			EditAutoTypeItemForm dlg = new EditAutoTypeItemForm();
			dlg.InitEx(m_atConfig, lvsic[0], false, m_tbDefaultAutoTypeSeq.Text,
				m_vStrings);

			if(UIUtil.ShowDialogAndDestroy(dlg) == DialogResult.OK)
				UpdateAutoTypeList(ListSelRestore.ByIndex, null, false, true);
		}

		private void OnBtnAutoTypeDelete(object sender, EventArgs e)
		{
			for(int i = m_lvAutoType.Items.Count - 1; i >= 0; --i)
			{
				if(m_lvAutoType.Items[i].Selected)
					m_atConfig.RemoveAt(i);
			}

			UpdateAutoTypeList(ListSelRestore.None, null, false, true);
		}

		private void OnBtnHistoryView(object sender, EventArgs e)
		{
			Debug.Assert(m_vHistory.UCount == m_lvHistory.Items.Count);

			ListView.SelectedIndexCollection lvsic = m_lvHistory.SelectedIndices;
			if(lvsic.Count != 1) { Debug.Assert(false); return; }

			PwEntry pe = m_vHistory.GetAt((uint)lvsic[0]);
			if(pe == null) { Debug.Assert(false); return; }

			PwEntryForm pwf = new PwEntryForm();
			pwf.InitEx(pe, PwEditMode.ViewReadOnlyEntry, m_pwDatabase,
				m_ilIcons, false, false);

			UIUtil.ShowDialogAndDestroy(pwf);
		}

		private void OnBtnHistoryDelete(object sender, EventArgs e)
		{
			Debug.Assert(m_vHistory.UCount == m_lvHistory.Items.Count);

			ListView.SelectedIndexCollection lvsic = m_lvHistory.SelectedIndices;
			int n = lvsic.Count; // Getting Count sends a message
			if(n == 0) return;

			// LVSIC: one access by index requires O(n) time, thus copy
			// all to an array (which requires O(1) for each element)
			int[] v = new int[n];
			lvsic.CopyTo(v, 0);

			for(int i = 0; i < n; ++i)
				m_vHistory.RemoveAt((uint)v[n - i - 1]);

			UpdateHistoryList(true);
			ResizeColumnHeaders();
		}

		private void OnBtnHistoryRestore(object sender, EventArgs e)
		{
			Debug.Assert(m_vHistory.UCount == m_lvHistory.Items.Count);

			ListView.SelectedIndexCollection lvsic = m_lvHistory.SelectedIndices;
			if(lvsic.Count != 1) { Debug.Assert(false); return; }

			m_pwEntry.RestoreFromBackup((uint)lvsic[0], m_pwDatabase);
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
			DateTime dt = DateTime.Now; // Not UTC
			if((nYears != 0) || (nMonths != 0) || (nDays != 0))
			{
				dt = dt.Date; // Remove time part
				dt = dt.AddYears(nYears);
				dt = dt.AddMonths(nMonths);
				dt = dt.AddDays(nDays);

				DateTime dtPrev = TimeUtil.ToLocal(m_cgExpiry.Value, false);
				dt = dt.AddHours(dtPrev.Hour);
				dt = dt.AddMinutes(dtPrev.Minute);
				dt = dt.AddSeconds(dtPrev.Second);
			}
			// else do not change the time part of dt

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
			m_ctxDefaultTimes.ShowEx(m_btnStandardExpires);
		}

		private const string ClipFmtStrings = "EntryStrings";
		private void PerformStrCopy(bool bNames, bool bValues)
		{
			if(!bNames && !bValues) { Debug.Assert(false); return; }

			StringBuilder sb = new StringBuilder();
			MemoryStream ms = new MemoryStream();

			foreach(ListViewItem lvi in m_lvStrings.SelectedItems)
			{
				string strName = (lvi.Text ?? string.Empty);
				Debug.Assert(m_vStrings.Exists(strName));
				ProtectedString psValue = m_vStrings.GetSafe(strName);

				if(sb.Length != 0) sb.AppendLine();

				if(bNames && bValues)
				{
					byte[] pbName = StrUtil.Utf8.GetBytes(strName);
					byte[] pbValue = psValue.ReadUtf8();

					ms.WriteByte(1);
					MemUtil.Write(ms, MemUtil.Int32ToBytes(pbName.Length));
					MemUtil.Write(ms, pbName);
					MemUtil.Write(ms, MemUtil.Int32ToBytes(pbValue.Length));
					MemUtil.Write(ms, pbValue);
					ms.WriteByte((byte)(psValue.IsProtected ? 1 : 0));
				}
				else if(bNames) sb.Append(strName);
				else sb.Append(psValue.ReadString());
			}

			if(bNames && bValues)
			{
				ms.WriteByte(0);
				byte[] pb = MemUtil.Compress(ms.ToArray());
				ClipboardUtil.Copy(pb, ClipFmtStrings, false, this.Handle);
			}
			else ClipboardUtil.Copy(sb.ToString(), false, false, null, null, this.Handle);

			ms.Dispose();
		}

		private void PerformStrPaste()
		{
			UpdateEntryStrings(true, false, false);

			try
			{
				byte[] pb = ClipboardUtil.GetData(ClipFmtStrings);
				if((pb == null) || (pb.Length == 0)) throw new FormatException();

				pb = MemUtil.Decompress(pb);

				using(MemoryStream ms = new MemoryStream(pb, false))
				{
					while(true)
					{
						int t = ms.ReadByte();
						if(t == 0) break;
						if(t == 1)
						{
							int cbName = MemUtil.BytesToInt32(MemUtil.Read(ms, 4));
							string strName = StrUtil.Utf8.GetString(MemUtil.Read(ms, cbName));
							int cbValue = MemUtil.BytesToInt32(MemUtil.Read(ms, 4));
							byte[] pbValue = MemUtil.Read(ms, cbValue);
							bool bProt = ((ms.ReadByte() & 1) != 0);

							for(int i = 1; i < int.MaxValue; ++i)
							{
								string strNameFull = ((i == 1) ? strName :
									(strName + " (" + i.ToString() + ")"));
								if(!m_vStrings.Exists(strNameFull))
								{
									m_vStrings.Set(strNameFull, new ProtectedString(
										bProt, pbValue));
									break;
								}
							}
						}
						else throw new FormatException();
					}
				}
			}
			catch(Exception ex) { MessageService.ShowWarning(ex); }

			UpdateEntryStrings(false, false, true);
		}

		private const string ClipFmtAutoType = "EntryAutoTypeAssociations";
		private void PerformAutoTypeCopy(bool bWindows, bool bSequences)
		{
			if(!bWindows && !bSequences) { Debug.Assert(false); return; }

			StringBuilder sb = new StringBuilder();
			MemoryStream ms = new MemoryStream();

			foreach(ListViewItem lvi in m_lvAutoType.SelectedItems)
			{
				AutoTypeAssociation a = (lvi.Tag as AutoTypeAssociation);
				if(a == null) { Debug.Assert(false); continue; }

				if(sb.Length != 0) sb.AppendLine();

				if(bWindows && bSequences)
				{
					byte[] pbWnd = StrUtil.Utf8.GetBytes(a.WindowName);
					byte[] pbSeq = StrUtil.Utf8.GetBytes(a.Sequence);

					ms.WriteByte(1);
					MemUtil.Write(ms, MemUtil.Int32ToBytes(pbWnd.Length));
					MemUtil.Write(ms, pbWnd);
					MemUtil.Write(ms, MemUtil.Int32ToBytes(pbSeq.Length));
					MemUtil.Write(ms, pbSeq);
				}
				else if(bWindows) sb.Append(a.WindowName);
				else sb.Append(a.Sequence);
			}

			if(bWindows && bSequences)
			{
				ms.WriteByte(0);
				byte[] pb = MemUtil.Compress(ms.ToArray());
				ClipboardUtil.Copy(pb, ClipFmtAutoType, false, this.Handle);
			}
			else ClipboardUtil.Copy(sb.ToString(), false, false, null, null, this.Handle);

			ms.Dispose();
		}

		private void PerformAutoTypePaste()
		{
			try
			{
				byte[] pb = ClipboardUtil.GetData(ClipFmtAutoType);
				if((pb == null) || (pb.Length == 0)) throw new FormatException();

				pb = MemUtil.Decompress(pb);

				using(MemoryStream ms = new MemoryStream(pb, false))
				{
					while(true)
					{
						int t = ms.ReadByte();
						if(t == 0) break;
						if(t == 1)
						{
							int cbWnd = MemUtil.BytesToInt32(MemUtil.Read(ms, 4));
							string strWnd = StrUtil.Utf8.GetString(MemUtil.Read(ms, cbWnd));
							int cbSeq = MemUtil.BytesToInt32(MemUtil.Read(ms, 4));
							string strSeq = StrUtil.Utf8.GetString(MemUtil.Read(ms, cbSeq));

							m_atConfig.Add(new AutoTypeAssociation(strWnd, strSeq));
						}
						else throw new FormatException();
					}
				}
			}
			catch(Exception ex) { MessageService.ShowWarning(ex); }

			UpdateAutoTypeList(ListSelRestore.None, null, true, true);
		}

		private void OnCtxAutoTypeCopyWnd(object sender, EventArgs e)
		{
			PerformAutoTypeCopy(true, false);
		}

		private void OnCtxAutoTypeCopySeq(object sender, EventArgs e)
		{
			PerformAutoTypeCopy(false, true);
		}

		private void OnCtxAutoTypeCopyItem(object sender, EventArgs e)
		{
			PerformAutoTypeCopy(true, true);
		}

		private void OnCtxAutoTypePasteItem(object sender, EventArgs e)
		{
			PerformAutoTypePaste();
		}

		private void OnBtnPickIcon(object sender, EventArgs e)
		{
			IconPickerForm ipf = new IconPickerForm();
			ipf.InitEx(m_ilIcons, (uint)PwIcon.Count, m_pwDatabase,
				(uint)m_pwEntryIcon, m_pwCustomIconID);

			if(ipf.ShowDialog() == DialogResult.OK)
			{
				m_pwEntryIcon = (PwIcon)ipf.ChosenIconId;
				m_pwCustomIconID = ipf.ChosenCustomIconUuid;

				if(!m_pwCustomIconID.Equals(PwUuid.Zero))
					UIUtil.SetButtonImage(m_btnIcon, DpiUtil.GetIcon(
						m_pwDatabase, m_pwCustomIconID), true);
				else
					UIUtil.SetButtonImage(m_btnIcon, m_ilIcons.Images[
						(int)m_pwEntryIcon], true);
			}

			UIUtil.DestroyForm(ipf);

			UpdateHistoryList(true); // User may have deleted a custom icon
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
			SaveDefaultSeq();

			EditAutoTypeItemForm ef = new EditAutoTypeItemForm();
			ef.InitEx(m_atConfig, -1, true, m_tbDefaultAutoTypeSeq.Text, m_vStrings);

			if(UIUtil.ShowDialogAndDestroy(ef) == DialogResult.OK)
				m_tbDefaultAutoTypeSeq.Text = m_atConfig.DefaultSequence;
		}

		private void OnCtxStrMoveToTitle(object sender, EventArgs e)
		{
			PerformStrMove(PwDefs.TitleField);
		}

		private void OnCtxStrMoveToUserName(object sender, EventArgs e)
		{
			PerformStrMove(PwDefs.UserNameField);
		}

		private void OnCtxStrMoveToPassword(object sender, EventArgs e)
		{
			PerformStrMove(PwDefs.PasswordField);
		}

		private void OnCtxStrMoveToUrl(object sender, EventArgs e)
		{
			PerformStrMove(PwDefs.UrlField);
		}

		private void OnCtxStrMoveToNotes(object sender, EventArgs e)
		{
			PerformStrMove(PwDefs.NotesField);
		}

		private void PerformStrMove(string strFieldTo)
		{
			List<string> lFieldsFrom = new List<string>();
			foreach(ListViewItem lvi in m_lvStrings.SelectedItems)
			{
				string str = lvi.Text;
				if(!string.IsNullOrEmpty(str)) lFieldsFrom.Add(str);
				else { Debug.Assert(false); }
			}

			GAction<TextBoxBase, string, bool> fAppend = delegate(TextBoxBase tb,
				string strAppend, bool bMultiLine)
			{
				string str = strAppend;
				if(tb.TextLength != 0)
					str = (bMultiLine ? MessageService.NewParagraph : ", ") + str;
				tb.Text += str;
			};

			foreach(string strFieldFrom in lFieldsFrom)
			{
				Debug.Assert(m_vStrings.Exists(strFieldFrom));
				string strValue = m_vStrings.ReadSafe(strFieldFrom);

				if(PwDefs.IsStandardField(strFieldTo) && (strFieldTo != PwDefs.NotesField))
					strValue = StrUtil.MultiToSingleLine(strValue).Trim();

				if(string.IsNullOrEmpty(strValue)) continue;

				if(strFieldTo == PwDefs.TitleField)
					fAppend(m_tbTitle, strValue, false);
				else if(strFieldTo == PwDefs.UserNameField)
					fAppend(m_tbUserName, strValue, false);
				else if(strFieldTo == PwDefs.PasswordField)
				{
					ProtectedString psP = m_icgPassword.GetPasswordEx();
					if(!psP.IsEmpty) psP += ", ";
					psP += strValue;

					ProtectedString psR = m_icgPassword.GetRepeatEx();
					if(!psR.IsEmpty) psR += ", ";
					psR += strValue;

					m_icgPassword.SetPasswords(psP, psR);
				}
				else if(strFieldTo == PwDefs.UrlField)
					fAppend(m_tbUrl, strValue, false);
				else if(strFieldTo == PwDefs.NotesField)
					fAppend(m_rtNotes, strValue, true);
				else { Debug.Assert(false); }
			}

			UpdateEntryStrings(true, false, false);
			foreach(string str in lFieldsFrom) { m_vStrings.Remove(str); }
			UpdateEntryStrings(false, false, true);
		}

		private void OnBtnStrMore(object sender, EventArgs e)
		{
			m_ctxStr.ShowEx(m_btnStrMore);
		}

		private void OnAutoTypeSelectedIndexChanged(object sender, EventArgs e)
		{
			EnableControlsEx();
		}

		private void OnAutoTypeItemActivate(object sender, EventArgs e)
		{
			if(m_btnAutoTypeEdit.Enabled) OnBtnAutoTypeEdit(sender, e);
		}

		private void OnStringsItemActivate(object sender, EventArgs e)
		{
			OnBtnStrEdit(sender, e);
		}

		private void OnPickForegroundColor(object sender, EventArgs e)
		{
			Color? clr = UIUtil.ShowColorDialog(m_clrForeground);
			if(clr.HasValue)
			{
				m_clrForeground = clr.Value;
				UIUtil.OverwriteButtonImage(m_btnPickFgColor, ref m_imgColorFg,
					UIUtil.CreateColorBitmap24(m_btnPickFgColor, m_clrForeground));
			}
		}

		private void OnPickBackgroundColor(object sender, EventArgs e)
		{
			Color? clr = UIUtil.ShowColorDialog(m_clrBackground);
			if(clr.HasValue)
			{
				m_clrBackground = clr.Value;
				UIUtil.OverwriteButtonImage(m_btnPickBgColor, ref m_imgColorBg,
					UIUtil.CreateColorBitmap24(m_btnPickBgColor, m_clrBackground));
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

		private void OnAutoTypeObfuscationLink(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if(e.Button == MouseButtons.Left)
				AppHelp.ShowHelp(AppDefs.HelpTopics.AutoTypeObfuscation, null);
		}

		private void OnAutoTypeObfuscationCheckedChanged(object sender, EventArgs e)
		{
			if(m_bInitializing) return;
			if(m_cbAutoTypeObfuscation.CheckState != CheckState.Checked) return;
			if((Program.Config.UI.UIFlags & (ulong)AceUIFlags.HideAutoTypeObfInfo) != 0)
				return;

			MessageService.ShowInfo(KPRes.AutoTypeObfuscationHint,
				KPRes.DocumentationHint);
		}

		private bool GetSelBin(out string strDataItem, out ProtectedBinary pb)
		{
			strDataItem = null;
			pb = null;

			ListView.SelectedListViewItemCollection lvsic = m_lvBinaries.SelectedItems;
			if((lvsic == null) || (lvsic.Count != 1)) return false; // No assert

			strDataItem = lvsic[0].Text;
			pb = m_vBinaries.Get(strDataItem);
			if(pb == null) { Debug.Assert(false); return false; }

			return true;
		}

		private void OpenSelBin(BinaryDataOpenOptions optBase)
		{
			string strDataItem;
			ProtectedBinary pb;
			if(!GetSelBin(out strDataItem, out pb)) return;

			BinaryDataOpenOptions opt = ((optBase != null) ? optBase.CloneDeep() :
				new BinaryDataOpenOptions());
			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry)
			{
				if(optBase == null)
					opt.Handler = BinaryDataHandler.InternalViewer;

				opt.ReadOnly = true;
			}

			ProtectedBinary pbMod = BinaryDataUtil.Open(strDataItem, pb, opt);
			if(pbMod != null)
			{
				m_vBinaries.Set(strDataItem, pbMod);
				UpdateEntryBinaries(false, true, strDataItem); // Update size
			}
		}

		private void OnBtnBinOpen(object sender, EventArgs e)
		{
			OpenSelBin(null);
		}

		private void OnDynBinOpen(object sender, DynamicMenuEventArgs e)
		{
			if(e == null) { Debug.Assert(false); return; }

			BinaryDataOpenOptions opt = (e.Tag as BinaryDataOpenOptions);
			if(opt == null) { Debug.Assert(false); return; }

			OpenSelBin(opt);
		}

		private void OnCtxBinOpenOpening(object sender, CancelEventArgs e)
		{
			string strDataItem;
			ProtectedBinary pb;
			if(!GetSelBin(out strDataItem, out pb))
			{
				e.Cancel = true;
				return;
			}

			BinaryDataUtil.BuildOpenWithMenu(m_dynBinOpen, strDataItem, pb,
				(m_pwEditMode == PwEditMode.ViewReadOnlyEntry));
		}

		private void OnBtnTools(object sender, EventArgs e)
		{
			m_ctxTools.ShowEx(m_btnTools);
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

			OpenFileDialogEx dlg = UIUtil.CreateOpenFileDialog(null, strFlt, 1,
				null, false, AppDefs.FileDialogContext.Attachments);

			if(dlg.ShowDialog() == DialogResult.OK)
				m_tbUrl.Text = "cmd://\"" + SprEncoding.EncodeForCommandLine(
					dlg.FileName) + "\"";
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

		private string CreateFieldReference(string strDefaultRef)
		{
			FieldRefForm dlg = new FieldRefForm();
			dlg.InitEx(m_pwDatabase.RootGroup, m_ilIcons, strDefaultRef);

			string strResult = string.Empty;
			if(dlg.ShowDialog() == DialogResult.OK)
				strResult = dlg.ResultReference;

			UIUtil.DestroyForm(dlg);
			return strResult;
		}

		private void CreateFieldReferenceIn(Control c, string strDefaultRef,
			string strSep)
		{
			if(c == null) { Debug.Assert(false); return; }

			string strRef = CreateFieldReference(strDefaultRef);
			if(string.IsNullOrEmpty(strRef)) return;

			string strPre = (c.Text ?? string.Empty);
			if((m_mvec != null) && (strPre == MultipleValuesEx.CueString))
				strPre = string.Empty;

			if(strPre.Length == 0) strSep = string.Empty;

			c.Text = strPre + strSep + strRef;
		}

		private void OnFieldRefInTitle(object sender, EventArgs e)
		{
			CreateFieldReferenceIn(m_tbTitle, PwDefs.TitleField, string.Empty);
		}

		private void OnFieldRefInUserName(object sender, EventArgs e)
		{
			CreateFieldReferenceIn(m_tbUserName, PwDefs.UserNameField, string.Empty);
		}

		private void OnFieldRefInPassword(object sender, EventArgs e)
		{
			string strRef = CreateFieldReference(PwDefs.PasswordField);
			if(string.IsNullOrEmpty(strRef)) return;

			ProtectedString psP = m_icgPassword.GetPasswordEx();
			ProtectedString psR = m_icgPassword.GetRepeatEx();

			if((m_mvec != null) && psP.Equals(MultipleValuesEx.CueProtectedString, false))
			{
				psP = ProtectedString.EmptyEx;
				psR = ProtectedString.EmptyEx;
			}

			m_icgPassword.SetPasswords(psP + strRef, psR + strRef);
		}

		private void OnFieldRefInUrl(object sender, EventArgs e)
		{
			CreateFieldReferenceIn(m_tbUrl, PwDefs.UrlField, string.Empty);
		}

		private void OnFieldRefInNotes(object sender, EventArgs e)
		{
			CreateFieldReferenceIn(m_rtNotes, PwDefs.NotesField, "\r\n");
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
					string strTitle = pe.Strings.ReadSafe(PwDefs.TitleField).Trim();
					string strHeader = ((strTitle.Length == 0) ? string.Empty :
						(KPRes.Entry + @" '" + strTitle + @"'"));
					string strText = KPRes.SaveBeforeCloseEntry;

					if(m_mvec != null)
					{
						strHeader = string.Empty;
						strText = KPRes.SaveBeforeCloseQuestion;
					}

					VistaTaskDialog dlg = new VistaTaskDialog();
					dlg.CommandLinks = false;
					dlg.Content = strText;
					dlg.MainInstruction = strHeader;
					dlg.WindowTitle = PwDefs.ShortProductName;
					dlg.SetIcon(VtdCustomIcon.Question);
					dlg.AddButton((int)DialogResult.Yes, KPRes.YesCmd, null);
					dlg.AddButton((int)DialogResult.No, KPRes.NoCmd, null);
					dlg.AddButton((int)DialogResult.Cancel, KPRes.Cancel, null);
					dlg.DefaultButtonID = (int)DialogResult.Yes;

					DialogResult dr;
					if(dlg.ShowDialog(this)) dr = (DialogResult)dlg.Result;
					else dr = MessageService.Ask(strText, PwDefs.ShortProductName,
						MessageBoxButtons.YesNoCancel);

					if((dr == DialogResult.Yes) || (dr == DialogResult.OK))
					{
						bCancel = !SaveEntry(m_pwEntry, true);
						if(!bCancel) this.DialogResult = DialogResult.OK;
					}
					else if((dr == DialogResult.Cancel) || (dr == DialogResult.None))
						bCancel = true;
				}
			}

			if(bCancel)
			{
				e.Cancel = true;
				this.DialogResult = DialogResult.None;
			}
		}

		private void OnBinariesItemActivate(object sender, EventArgs e)
		{
			OnBtnBinOpen(sender, e);
		}

		private void OnHistoryItemActivate(object sender, EventArgs e)
		{
			OnBtnHistoryView(sender, e);
		}

		private void OnCtxBinImport(object sender, EventArgs e)
		{
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

				string strItem = UrlUtil.GetFileName(strFile);
				if(m_vBinaries.Get(strItem) != null)
				{
					string strMsg = KPRes.AttachedExistsAlready + MessageService.NewLine +
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
					if(!FileDialogsEx.CheckAttachmentSize(strFile, KPRes.AttachFailed +
						MessageService.NewParagraph + strFile))
						continue;

					byte[] vBytes = File.ReadAllBytes(strFile);
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

		private static void BinDragAccept(DragEventArgs e)
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

		private void InitOverridesBox()
		{
			List<KeyValuePair<string, Image>> l = new List<KeyValuePair<string, Image>>();

			AddOverrideUrlItem(l, "cmd://{INTERNETEXPLORER} \"{URL}\"",
				AppLocator.InternetExplorerPath);
			AddOverrideUrlItem(l, "cmd://{INTERNETEXPLORER} -private \"{URL}\"",
				AppLocator.InternetExplorerPath);
			AddOverrideUrlItem(l, "microsoft-edge:{URL}",
				AppLocator.EdgePath);
			AddOverrideUrlItem(l, "cmd://{FIREFOX} \"{URL}\"",
				AppLocator.FirefoxPath);
			AddOverrideUrlItem(l, "cmd://{FIREFOX} -private-window \"{URL}\"",
				AppLocator.FirefoxPath);
			AddOverrideUrlItem(l, "cmd://{GOOGLECHROME} \"{URL}\"",
				AppLocator.ChromePath);
			AddOverrideUrlItem(l, "cmd://{GOOGLECHROME} --incognito \"{URL}\"",
				AppLocator.ChromePath);
			AddOverrideUrlItem(l, "cmd://{OPERA} \"{URL}\"",
				AppLocator.OperaPath);
			AddOverrideUrlItem(l, "cmd://{OPERA} --private \"{URL}\"",
				AppLocator.OperaPath);
			AddOverrideUrlItem(l, "cmd://{SAFARI} \"{URL}\"",
				AppLocator.SafariPath);

			Debug.Assert(m_cmbOverrideUrl.InvokeRequired ||
				MonoWorkarounds.IsRequired(373134));
			VoidDelegate f = delegate()
			{
				try
				{
					Debug.Assert(!m_cmbOverrideUrl.InvokeRequired);
					foreach(KeyValuePair<string, Image> kvp in l)
					{
						m_cmbOverrideUrl.Items.Add(kvp.Key);
						m_lOverrideUrlIcons.Add(kvp.Value);
					}

					m_cmbOverrideUrl.OrderedImageList = m_lOverrideUrlIcons;
				}
				catch(Exception) { Debug.Assert(false); }
			};
			m_cmbOverrideUrl.Invoke(f);
		}

		private void AddOverrideUrlItem(List<KeyValuePair<string, Image>> l,
			string strOverride, string strIconPath)
		{
			if(string.IsNullOrEmpty(strOverride)) { Debug.Assert(false); return; }

			int w = DpiUtil.ScaleIntX(16);
			int h = DpiUtil.ScaleIntY(16);

			Image img = null;
			string str = UrlUtil.GetQuotedAppPath(strIconPath ?? string.Empty);
			str = str.Trim();
			try
			{
				if((str.Length > 0) && File.Exists(str))
				{
					// img = UIUtil.GetFileIcon(str, w, h);
					img = FileIcons.GetImageForPath(str, new Size(w, h), true, true);
				}
			}
			catch(Exception) { Debug.Assert(false); }

			if(img == null)
				img = GfxUtil.ScaleImage(m_ilIcons.Images[(int)PwIcon.Console],
					w, h, ScaleTransformFlags.UIIcon);

			l.Add(new KeyValuePair<string, Image>(strOverride, img));
		}

		private void OnBtnAutoTypeUp(object sender, EventArgs e)
		{
			MoveSelectedAutoTypeItems(false);
		}

		private void OnBtnAutoTypeDown(object sender, EventArgs e)
		{
			MoveSelectedAutoTypeItems(true);
		}

		private void MoveSelectedAutoTypeItems(bool bDown)
		{
			int n = m_lvAutoType.Items.Count;
			int s = m_lvAutoType.SelectedIndices.Count;
			if(s == 0) return;

			int[] v = new int[s];
			m_lvAutoType.SelectedIndices.CopyTo(v, 0);
			Array.Sort(v);

			if((bDown && (v[s - 1] >= (n - 1))) || (!bDown && (v[0] <= 0)))
				return; // Moving as a block is not possible

			int iStart = (bDown ? (s - 1) : 0);
			int iExcl = (bDown ? -1 : s);
			int iStep = (bDown ? -1 : 1);

			for(int i = iStart; i != iExcl; i += iStep)
			{
				int p = v[i];

				AutoTypeAssociation a = m_atConfig.GetAt(p);
				if(bDown)
				{
					Debug.Assert(p < (n - 1));
					m_atConfig.RemoveAt(p);
					m_atConfig.Insert(p + 1, a);
				}
				else // Up
				{
					Debug.Assert(p > 0);
					m_atConfig.RemoveAt(p);
					m_atConfig.Insert(p - 1, a);
				}
			}

			UpdateAutoTypeList(ListSelRestore.ByRef, null, false, true);
		}

		private void OnCustomDataSelectedIndexChanged(object sender, EventArgs e)
		{
			EnableControlsEx();
		}

		private void OnBtnCDDel(object sender, EventArgs e)
		{
			UIUtil.StrDictListDeleteSel(m_lvCustomData, m_sdCustomData, (m_mvec != null));
			UIUtil.SetFocus(m_lvCustomData, this);
			EnableControlsEx();
		}

		private static void NormalizeStrings(ProtectedStringDictionary d,
			PwDatabase pd)
		{
			if(d == null) { Debug.Assert(false); return; }

			MemoryProtectionConfig mp = ((pd != null) ? pd.MemoryProtection :
				(new MemoryProtectionConfig()));

			string str = d.ReadSafe(PwDefs.NotesField);
			d.Set(PwDefs.NotesField, new ProtectedString(mp.ProtectNotes,
				StrUtil.NormalizeNewLines(str, true)));

			// Custom strings are normalized by the string editing form
		}

		private void InitUserNameSuggestions()
		{
			try
			{
				AceColumn c = Program.Config.MainWindow.FindColumn(
					AceColumnType.UserName);
				if((c == null) || c.HideWithAsterisks) return;

				GFunc<PwEntry, string> f = delegate(PwEntry pe)
				{
					string str = pe.Strings.ReadSafe(PwDefs.UserNameField);
					return ((str.Length != 0) ? str : null);
				};

				string[] v = m_pwDatabase.RootGroup.CollectEntryStrings(f, true);

				// Do not append, because it breaks Ctrl+A;
				// https://sourceforge.net/p/keepass/discussion/329220/thread/4f626b91/
				UIUtil.EnableAutoCompletion(m_tbUserName, false, v); // Invokes
			}
			catch(Exception) { Debug.Assert(false); }
		}

		private void OnUrlTextChanged(object sender, EventArgs e)
		{
			EnableControlsEx(); // URL override warning
		}

		private void OnUrlOverrideTextChanged(object sender, EventArgs e)
		{
			EnableControlsEx(); // URL override warning
		}

		private void OnQualityCheckCheckedChanged(object sender, EventArgs e)
		{
			m_icgPassword.QualityEnabled = m_cbQualityCheck.Checked;
		}

		private void OnCtxStrOpening(object sender, CancelEventArgs e)
		{
			bool bEdit = (m_pwEditMode != PwEditMode.ViewReadOnlyEntry);
			m_ctxStrPasteItem.Enabled = (bEdit &&
				ClipboardUtil.ContainsData(ClipFmtStrings));
		}

		private void OnCtxAutoTypeOpening(object sender, CancelEventArgs e)
		{
			bool bEdit = (m_pwEditMode != PwEditMode.ViewReadOnlyEntry);
			m_ctxAutoTypePasteItem.Enabled = (bEdit &&
				ClipboardUtil.ContainsData(ClipFmtAutoType));
		}

		private void OnCtxStrCopyName(object sender, EventArgs e)
		{
			PerformStrCopy(true, false);
		}

		private void OnCtxStrCopyValue(object sender, EventArgs e)
		{
			PerformStrCopy(false, true);
		}

		private void OnCtxStrCopyItem(object sender, EventArgs e)
		{
			PerformStrCopy(true, true);
		}

		private void OnCtxStrPasteItem(object sender, EventArgs e)
		{
			PerformStrPaste();
		}

		private void OnCtxAutoTypeDup(object sender, EventArgs e)
		{
			foreach(ListViewItem lvi in m_lvAutoType.SelectedItems)
			{
				AutoTypeAssociation a = (lvi.Tag as AutoTypeAssociation);
				if(a == null) { Debug.Assert(false); continue; }

				m_atConfig.Add(a.CloneDeep());
			}

			UpdateAutoTypeList(ListSelRestore.None, null, true, true);
		}

		private void OnBtnAutoTypeMore(object sender, EventArgs e)
		{
			m_ctxAutoType.ShowEx(m_btnAutoTypeMore);
		}

		private void OnCtxStrSelectAll(object sender, EventArgs e)
		{
			m_lvStrings.BeginUpdate();
			foreach(ListViewItem lvi in m_lvStrings.Items)
				lvi.Selected = true;
			m_lvStrings.EndUpdate();
		}

		private void OnCtxAutoTypeSelectAll(object sender, EventArgs e)
		{
			m_lvAutoType.BeginUpdate();
			foreach(ListViewItem lvi in m_lvAutoType.Items)
				lvi.Selected = true;
			m_lvAutoType.EndUpdate();
		}
	}
}
