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
using System.Globalization;
using System.IO;

using KeePass.App;
using KeePass.UI;
using KeePass.Resources;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Cryptography;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class PwEntryForm : Form
	{
		private PwEditMode m_pwEditMode = PwEditMode.Invalid;
		private PwDatabase m_pwDatabase = null;
		private bool m_bShowAdvancedByDefault = false;

		private PwEntry m_pwEntry = null;
		private ProtectedStringDictionary m_vStrings = null;
		private ProtectedBinaryDictionary m_vBinaries = null;
		private AutoTypeConfig m_atConfig = null;
		private PwObjectList<PwEntry> m_vHistory = null;
		private Color m_clrBackground;

		private PwIcon m_pwEntryIcon = 0;
		private ImageList m_ilIcons = null;

		private Color m_clrNormalBackColor = Color.White;
		private bool m_bRepeatPasswordFailed = false;
		private bool m_bLockEnabledState = false;

		private bool m_bInitializing = false;

		private SecureEdit m_secPassword = new SecureEdit();
		private SecureEdit m_secRepeat = new SecureEdit();
		private RichTextBoxContextMenu m_ctxNotes = new RichTextBoxContextMenu();

		private readonly string DeriveFromPrevious = "(" + KPRes.GenPwBasedOnPrevious + ")";

		private List<PasswordGenerationOptions> m_vPwGenOptions =
			new List<PasswordGenerationOptions>();
		private DynamicMenu m_dynGenProfiles;

		public enum PwEditMode : uint
		{
			Invalid = 0,
			AddNewEntry,
			EditExistingEntry,
			ViewReadOnlyEntry
		}

		public bool ShowAdvancedByDefault
		{
			set { m_bShowAdvancedByDefault = value; }
		}

		public PwEntryForm()
		{
			InitializeComponent();
		}

		public void InitEx(PwEntry pwEntry, PwEditMode pwMode, PwDatabase pwDatabase, ImageList ilIcons)
		{
			Debug.Assert(pwEntry != null); if(pwEntry == null) throw new ArgumentNullException();
			Debug.Assert(pwMode != PwEditMode.Invalid); if(pwMode == PwEditMode.Invalid) throw new ArgumentException();
			Debug.Assert(ilIcons != null); if(ilIcons == null) throw new ArgumentNullException();

			m_pwEntry = pwEntry;
			m_pwEditMode = pwMode;
			m_pwDatabase = pwDatabase;
			m_ilIcons = ilIcons;

			m_vStrings = m_pwEntry.Strings.CloneDeep();
			m_vBinaries = m_pwEntry.Binaries.CloneDeep();
			m_atConfig = m_pwEntry.AutoType.CloneDeep();
			m_vHistory = m_pwEntry.History.CloneDeep();
		}

		private void InitEntryTab()
		{
			m_pwEntryIcon = m_pwEntry.Icon;
			m_btnIcon.Image = m_ilIcons.Images[(int)m_pwEntryIcon];

			bool bHideInitial = m_cbHidePassword.Checked;
			m_secPassword.Attach(m_tbPassword, ProcessTextChangedPassword, bHideInitial);
			m_secRepeat.Attach(m_tbRepeatPassword, ProcessTextChangedRepeatPw, bHideInitial);

			m_tbTitle.Text = m_vStrings.ReadSafe(PwDefs.TitleField);
			m_tbUserName.Text = m_vStrings.ReadSafe(PwDefs.UserNameField);

			byte[] pb = m_vStrings.GetSafe(PwDefs.PasswordField).ReadUTF8();
			m_secPassword.SetPassword(pb);
			m_secRepeat.SetPassword(pb);
			MemUtil.ZeroByteArray(pb);

			m_tbUrl.Text = m_vStrings.ReadSafe(PwDefs.UrlField);
			m_rtNotes.Text = m_vStrings.ReadSafe(PwDefs.NotesField);

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

				m_btnOK.Enabled = false;
			}
		}

		private void InitAdvancedTab()
		{
			m_lvStrings.SmallImageList = m_ilIcons;
			m_lvBinaries.SmallImageList = m_ilIcons;

			int nWidth = m_lvStrings.ClientRectangle.Width / 2;
			m_lvStrings.Columns.Add(KPRes.FieldName, nWidth);
			m_lvStrings.Columns.Add(KPRes.FieldValue, nWidth);

			nWidth = m_lvBinaries.ClientRectangle.Width / 2;
			m_lvBinaries.Columns.Add(KPRes.FieldName, nWidth);
			m_lvBinaries.Columns.Add(KPRes.FieldValue, nWidth);

			UpdateStringsList();
			UpdateBinariesList();

			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry)
			{
				m_btnStrAdd.Enabled = m_btnStrEdit.Enabled =
					m_btnStrDelete.Enabled = m_btnStrMove.Enabled =
					m_btnBinAdd.Enabled = m_btnBinDelete.Enabled =
					m_btnBinSave.Enabled = false;
			}
		}

		private void UpdateStringsList()
		{
			m_lvStrings.Items.Clear();

			foreach(KeyValuePair<string, ProtectedString> kvpStr in m_vStrings)
			{
				if(!PwDefs.IsStandardField(kvpStr.Key))
				{
					PwIcon pwIcon = kvpStr.Value.IsProtected ? PwIcon.PaperLocked : PwIcon.PaperNew;

					ListViewItem lvi = m_lvStrings.Items.Add(kvpStr.Key, (int)pwIcon);

					if(!kvpStr.Value.IsViewable) lvi.SubItems.Add("********");
					else lvi.SubItems.Add(kvpStr.Value.ReadString());
				}
			}
		}

		private void UpdateBinariesList()
		{
			m_lvBinaries.Items.Clear();

			foreach(KeyValuePair<string, ProtectedBinary> kvpBin in m_vBinaries)
			{
				ListViewItem lvi = m_lvBinaries.Items.Add(kvpBin.Key, (int)PwIcon.PaperNew);

				if(!kvpBin.Value.IsViewable) lvi.SubItems.Add("********");
				else lvi.SubItems.Add(kvpBin.Value.ReadData().ToString());
			}
		}

		private Image CreateColorButtonImage(Button btn, Color color)
		{
			Bitmap bmp = new Bitmap(btn.ClientRectangle.Width, btn.ClientRectangle.Height);
			Graphics g = Graphics.FromImage(bmp);
			g.FillRectangle(new SolidBrush(color), 0, 0, bmp.Width, bmp.Height);
			return bmp;
		}

		private void InitPropertiesTab()
		{
			m_clrBackground = m_pwEntry.BackgroundColor;

			if(m_clrBackground != Color.Empty)
				m_btnPickBgColor.Image = CreateColorButtonImage(m_btnPickBgColor,
					m_clrBackground);

			m_cbCustomBackgroundColor.Checked = (m_clrBackground != Color.Empty);

			m_tbOverrideUrl.Text = m_pwEntry.OverrideUrl;

			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry)
			{
				m_cbCustomBackgroundColor.Enabled = false;
				m_btnPickBgColor.Enabled = false;
				m_tbOverrideUrl.ReadOnly = true;
			}
		}

		private void InitAutoTypeTab()
		{
			m_lvAutoType.SmallImageList = m_ilIcons;

			m_cbAutoTypeEnabled.Checked = m_atConfig.Enabled;

			string strDefaultSeq = m_atConfig.DefaultSequence;
			if(strDefaultSeq.Length > 0) m_rbAutoTypeOverride.Checked = true;
			else m_rbAutoTypeSeqInherit.Checked = true;

			if(strDefaultSeq.Length == 0)
			{
				PwGroup pg = m_pwEntry.ParentGroup;
				if(pg != null)
				{
					strDefaultSeq = pg.DefaultAutoTypeSequence;

					if(strDefaultSeq.Length == 0)
					{
						if(PwDefs.IsTanEntry(m_pwEntry))
							strDefaultSeq = PwDefs.DefaultAutoTypeSequenceTan;
						else strDefaultSeq = PwDefs.DefaultAutoTypeSequence;
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
				m_cbAutoTypeEnabled.Enabled = m_rbAutoTypeSeqInherit.Enabled =
					m_rbAutoTypeOverride.Enabled = m_btnAutoTypeAdd.Enabled =
					m_btnAutoTypeDelete.Enabled = m_btnAutoTypeEdit.Enabled = false;
				m_tbDefaultAutoTypeSeq.Enabled = m_btnAutoTypeEditDefault.Enabled =
					false;
			}
		}

		private void UpdateAutoTypeList()
		{
			m_lvAutoType.Items.Clear();

			foreach(KeyValuePair<string, string> kvp in m_atConfig.WindowSequencePairs)
			{
				ListViewItem lvi = m_lvAutoType.Items.Add(kvp.Key, (int)PwIcon.List);
				lvi.SubItems.Add(kvp.Value);
			}
		}

		private void InitHistoryTab()
		{
			m_lvHistory.SmallImageList = m_ilIcons;

			int nWidth = m_lvHistory.ClientRectangle.Width / 3;
			m_lvHistory.Columns.Add(KPRes.Version, nWidth);
			m_lvHistory.Columns.Add(KPRes.UserName, nWidth);
			m_lvHistory.Columns.Add(KPRes.Password, nWidth);

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
				ListViewItem lvi = m_lvHistory.Items.Add(TimeUtil.ToDisplayString(pe.LastAccessTime),
					(int)pe.Icon);

				lvi.SubItems.Add(pe.Strings.ReadSafeEx(PwDefs.UserNameField));
				lvi.SubItems.Add(pe.Strings.ReadSafeEx(PwDefs.PasswordField));
			}
		}

		private void ResizeColumnHeaders()
		{
			Debug.Assert(m_lvStrings.Columns.Count == 2);
			int dx = m_lvStrings.ClientRectangle.Width;
			m_lvStrings.Columns[0].Width = m_lvStrings.Columns[1].Width = dx / 2;

			Debug.Assert(m_lvBinaries.Columns.Count == 2);
			dx = m_lvBinaries.ClientRectangle.Width;
			m_lvBinaries.Columns[0].Width = m_lvBinaries.Columns[1].Width = dx / 2;

			Debug.Assert(m_lvAutoType.Columns.Count == 2);
			dx = m_lvAutoType.ClientRectangle.Width;
			m_lvAutoType.Columns[0].Width = m_lvAutoType.Columns[1].Width = dx / 2;

			Debug.Assert(m_lvHistory.Columns.Count == 3);
			dx = m_lvHistory.ClientRectangle.Width;
			m_lvHistory.Columns[0].Width = m_lvHistory.Columns[1].Width =
				m_lvHistory.Columns[2].Width = dx / 3;
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			Debug.Assert(m_pwEntry != null); if(m_pwEntry == null) throw new ArgumentNullException();
			Debug.Assert(m_pwEditMode != PwEditMode.Invalid); if(m_pwEditMode == PwEditMode.Invalid) throw new ArgumentException();
			Debug.Assert(m_pwDatabase != null); if(m_pwDatabase == null) throw new ArgumentNullException();
			Debug.Assert(m_ilIcons != null); if(m_ilIcons == null) throw new ArgumentNullException();

			GlobalWindowManager.AddWindow(this);

			m_clrNormalBackColor = m_tbPassword.BackColor;
			m_dynGenProfiles = new DynamicMenu(m_ctxPwGenProfiles);
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
				BannerFactory.BannerStyle.Default,
				KeePass.Properties.Resources.B48x48_KGPG_Sign,
				strTitle, strDesc);
			this.Icon = Properties.Resources.KeePass;
			this.Text = strTitle;

			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry)
				m_bLockEnabledState = true;

			m_bInitializing = true;

			if(AppConfigEx.GetBool(AppDefs.ConfigKeys.RememberHidingInDialogs))
				m_cbHidePassword.Checked = AppConfigEx.GetBool(
					AppDefs.ConfigKeys.HidePasswordInEntryForm.Key,
					AppConfigEx.GetBool(AppDefs.ConfigKeys.HidePasswords));
			else
				m_cbHidePassword.Checked = AppConfigEx.GetBool(
					AppDefs.ConfigKeys.HidePasswords);

			InitEntryTab();
			InitAdvancedTab();
			InitPropertiesTab();
			InitAutoTypeTab();
			InitHistoryTab();
			m_bInitializing = false;

			m_btnIcon.Image = m_ilIcons.Images[(int)m_pwEntry.Icon];

			if(m_bShowAdvancedByDefault)
				m_tabMain.SelectedTab = m_tabAdvanced;

			ResizeColumnHeaders();
			EnableControlsEx();

			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry)
				m_btnCancel.Focus();
			else m_tbTitle.Focus();
		}

		private void EnableControlsEx()
		{
			if(m_bInitializing) return;

			byte[] pb = m_secPassword.ToUTF8();
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

			m_btnPickBgColor.Enabled = m_cbCustomBackgroundColor.Checked;

			m_lvAutoType.Enabled = m_btnAutoTypeAdd.Enabled = m_btnAutoTypeDelete.Enabled =
				m_rbAutoTypeSeqInherit.Enabled =
				m_rbAutoTypeOverride.Enabled =
				m_cbAutoTypeEnabled.Checked;

			if(!m_rbAutoTypeOverride.Checked)
				m_tbDefaultAutoTypeSeq.Enabled = m_btnAutoTypeEditDefault.Enabled = false;
			else
				m_tbDefaultAutoTypeSeq.Enabled = m_btnAutoTypeEditDefault.Enabled =
					m_cbAutoTypeEnabled.Checked;

			int nAutoTypeSel = m_lvAutoType.SelectedItems.Count;

			m_btnAutoTypeEdit.Enabled = (nAutoTypeSel == 1);

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

		private void SaveEntry()
		{
			if(m_pwEditMode == PwEditMode.ViewReadOnlyEntry) return;

			m_pwEntry.Touch(true);

			m_pwEntry.History = m_vHistory; // Must be called before CreateBackup()
			if(m_pwEditMode != PwEditMode.AddNewEntry)
				m_pwEntry.CreateBackup();

			m_pwEntry.Icon = m_pwEntryIcon;

			if(m_cbCustomBackgroundColor.Checked)
				m_pwEntry.BackgroundColor = m_clrBackground;
			else m_pwEntry.BackgroundColor = Color.Empty;

			m_pwEntry.OverrideUrl = m_tbOverrideUrl.Text;

			m_pwEntry.Expires = m_cbExpires.Checked;
			m_pwEntry.ExpiryTime = m_dtExpireDateTime.Value;

			m_vStrings.Set(PwDefs.TitleField, new ProtectedString(m_pwDatabase.MemoryProtection.ProtectTitle,
				m_tbTitle.Text));
			m_vStrings.Set(PwDefs.UserNameField, new ProtectedString(m_pwDatabase.MemoryProtection.ProtectUserName,
				m_tbUserName.Text));

			byte[] pb = m_secPassword.ToUTF8();
			m_vStrings.Set(PwDefs.PasswordField, new ProtectedString(m_pwDatabase.MemoryProtection.ProtectPassword,
				pb));
			MemUtil.ZeroByteArray(pb);

			m_vStrings.Set(PwDefs.UrlField, new ProtectedString(m_pwDatabase.MemoryProtection.ProtectUrl,
				m_tbUrl.Text));
			m_vStrings.Set(PwDefs.NotesField, new ProtectedString(m_pwDatabase.MemoryProtection.ProtectNotes,
				m_rtNotes.Text));

			m_pwEntry.Strings = m_vStrings;
			m_pwEntry.Binaries = m_vBinaries;

			m_atConfig.Enabled = m_cbAutoTypeEnabled.Checked;
			if(m_rbAutoTypeSeqInherit.Checked) m_atConfig.DefaultSequence = string.Empty;
			else if(m_rbAutoTypeOverride.Checked)
				m_atConfig.DefaultSequence = m_tbDefaultAutoTypeSeq.Text;
			else { Debug.Assert(false); }

			m_pwEntry.AutoType = m_atConfig;
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

			SaveEntry();

			CleanUpEx();
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
			m_pwEntry.Touch(false);

			CleanUpEx();
		}

		private void CleanUpEx()
		{
			m_dynGenProfiles.MenuClick -= this.OnProfilesDynamicMenuClick;

			if(m_pwEditMode != PwEditMode.ViewReadOnlyEntry)
				AppConfigEx.SetValue(AppDefs.ConfigKeys.HidePasswordInEntryForm, m_cbHidePassword.Checked);

			m_ctxNotes.Detach();
			m_secPassword.Detach();
			m_secRepeat.Detach();
		}

		private void OnCheckedHidePassword(object sender, EventArgs e)
		{
			if(m_bInitializing) return;
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
			EditStringForm esf = new EditStringForm();

			esf.InitEx(m_vStrings, null, null);
			if(esf.ShowDialog() == DialogResult.OK)
			{
				UpdateStringsList();
				ResizeColumnHeaders();
			}
		}

		private void OnBtnStrEdit(object sender, EventArgs e)
		{
			EditStringForm esf = new EditStringForm();

			ListView.SelectedListViewItemCollection vSel = m_lvStrings.SelectedItems;
			if(vSel.Count <= 0) return;

			string strName = vSel[0].Text;
			ProtectedString psValue = m_vStrings.Get(strName);

			esf.InitEx(m_vStrings, strName, psValue);
			if(esf.ShowDialog() == DialogResult.OK)
				UpdateStringsList();
		}

		private void OnBtnStrDelete(object sender, EventArgs e)
		{
			ListView.SelectedIndexCollection vSel = m_lvStrings.SelectedIndices;

			bool bPostResize = (vSel.Count > 0);

			for(int i = 0; i < vSel.Count; i++)
			{
				int j = vSel.Count - i - 1;

				if(m_pwEntry.Strings.Remove(m_lvStrings.Items[j].Text))
				{
					m_lvStrings.Items.RemoveAt(j);
				}
				else { Debug.Assert(false); }
			}

			if(bPostResize) ResizeColumnHeaders();
		}

		private void OnBtnBinAdd(object sender, EventArgs e)
		{
			if(m_dlgAttachFile.ShowDialog() == DialogResult.OK)
			{
				foreach(string strFile in m_dlgAttachFile.FileNames)
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
							string strFileName = UrlUtil.StripExtension(strItem) + ".";
							string strExtension = UrlUtil.GetExtension(strItem);

							int nTry = 0;
							while(true)
							{
								string strNewName = strFileName + nTry.ToString() + strExtension;
								if(m_vBinaries.Get(strNewName) == null)
								{
									strItem = strNewName;
									break;
								}

								nTry++;
							}
						}
					}

					try
					{
						vBytes = File.ReadAllBytes(strFile);

						ProtectedBinary pb = new ProtectedBinary(false, vBytes);
						m_vBinaries.Set(strItem, pb);
					}
					catch(Exception exAttach)
					{
						MessageService.ShowWarning(KPRes.AttachFailed, strFile, exAttach);
					}
				}

				UpdateBinariesList();
				ResizeColumnHeaders();
			}
		}

		private void OnBtnBinDelete(object sender, EventArgs e)
		{
			ListView.SelectedListViewItemCollection lvsc = m_lvBinaries.SelectedItems;

			int nSelCount = lvsc.Count;
			if(nSelCount == 0) { Debug.Assert(false); return; }

			for(int i = 0; i < nSelCount; i++)
			{
				int j = nSelCount - i - 1;

				m_vBinaries.Remove(lvsc[j].Text);
			}

			UpdateBinariesList();
			ResizeColumnHeaders();
		}

		private void OnBtnBinSave(object sender, EventArgs e)
		{
			ListView.SelectedListViewItemCollection lvsc = m_lvBinaries.SelectedItems;

			int nSelCount = lvsc.Count;
			if(nSelCount == 0) { Debug.Assert(false); return; }

			if(nSelCount == 1)
			{
				m_dlgSaveAttachedFile.FileName = lvsc[0].Text;

				if(m_dlgSaveAttachedFile.ShowDialog() == DialogResult.OK)
				{
					SaveAttachmentTo(lvsc[0], m_dlgSaveAttachedFile.FileName, false);
				}
			}
			else // nSelCount > 1
			{
				if(m_dlgSaveAttachedFiles.ShowDialog() == DialogResult.OK)
				{
					string strRootPath = UrlUtil.EnsureTerminatingSeparator(m_dlgSaveAttachedFiles.SelectedPath, false);

					foreach(ListViewItem lvi in lvsc)
					{
						SaveAttachmentTo(lvi, strRootPath + lvi.Text, true);
					}
				}
			}
		}

		private void SaveAttachmentTo(ListViewItem lvi, string strFileName, bool bConfirmOverwrite)
		{
			Debug.Assert(lvi != null); if(lvi == null) throw new ArgumentNullException();
			Debug.Assert(strFileName != null); if(strFileName == null) throw new ArgumentNullException();

			if(File.Exists(strFileName))
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
			Debug.Assert(pe != null); if(pe == null) throw new ArgumentNullException();

			PwEntryForm pwf = new PwEntryForm();
			pwf.InitEx(pe, PwEditMode.ViewReadOnlyEntry, m_pwDatabase, m_ilIcons);

			pwf.ShowDialog();
		}

		private void OnBtnHistoryDelete(object sender, EventArgs e)
		{
			Debug.Assert(m_vHistory.UCount == m_lvHistory.Items.Count);

			ListView.SelectedIndexCollection lvsi = m_lvHistory.SelectedIndices;
			int nSelCount = lvsi.Count;

			if(nSelCount == 0) return;

			for(int i = 0; i < lvsi.Count; i++)
			{
				int j = nSelCount - i - 1;
				m_vHistory.Remove(m_vHistory.GetAt((uint)lvsi[j]));
			}

			UpdateHistoryList();
			ResizeColumnHeaders();
		}

		private void OnBtnHistoryRestore(object sender, EventArgs e)
		{
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
				if(lvsc.Count > 0) ClipboardUtil.Copy(lvsc[0].SubItems[1].Text, true);
			}
			else if(m_lvBinaries.Focused)
			{
				lvsc = m_lvBinaries.SelectedItems;
				if(lvsc.Count > 0) ClipboardUtil.Copy(lvsc[0].SubItems[1].Text, true);
			}
			else if(m_lvAutoType.Focused)
			{
				lvsc = m_lvAutoType.SelectedItems;
				if(lvsc.Count > 0) ClipboardUtil.Copy(lvsc[0].SubItems[1].Text, true);
			}
			else { Debug.Assert(false); }
		}

		private void OnBtnPickIcon(object sender, EventArgs e)
		{
			IconPickerForm ipf = new IconPickerForm();

			ipf.InitEx(m_ilIcons, (uint)m_pwEntryIcon);

			if(ipf.ShowDialog() == DialogResult.OK)
			{
				m_pwEntryIcon = (PwIcon)ipf.ChosenImageIndex;

				m_btnIcon.Image = m_ilIcons.Images[(int)m_pwEntryIcon];
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
			m_atConfig.DefaultSequence = m_tbDefaultAutoTypeSeq.Text;

			EditAutoTypeItemForm ef = new EditAutoTypeItemForm();
			ef.InitEx(m_atConfig, m_vStrings, "(" + KPRes.Default + ")", true);

			if(ef.ShowDialog() == DialogResult.OK)
			{
				m_tbDefaultAutoTypeSeq.Text = m_atConfig.DefaultSequence;
			}
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

			if(strStandardField == PwDefs.TitleField) m_tbTitle.Text = strText;
			else if(strStandardField == PwDefs.UserNameField) m_tbUserName.Text = strText;
			else if(strStandardField == PwDefs.PasswordField) m_tbPassword.Text = strText;
			else if(strStandardField == PwDefs.UrlField) m_tbUrl.Text = strText;
			else if(strStandardField == PwDefs.NotesField) m_rtNotes.Text = strText;
			else { Debug.Assert(false); }

			m_vStrings.Remove(lvi.Text);
			UpdateStringsList();
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

		private void OnBtnHelp(object sender, EventArgs e)
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

		private void OnNotesLinkClicked(object sender, LinkClickedEventArgs e)
		{
			WinUtil.OpenUrlInNewBrowser(e.LinkText, m_pwEntry);
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

			byte[] pbCurPassword = m_secPassword.ToUTF8();
			bool bAtLeastOneChar = (pbCurPassword.Length > 0);
			ProtectedString ps = new ProtectedString(true, pbCurPassword);
			Array.Clear(pbCurPassword, 0, pbCurPassword.Length);
			PasswordGenerationOptions opt = PasswordGenerator.DeriveOptionsFromPassword(ps);

			pgf.InitEx(bAtLeastOneChar ? opt : null, true);
			if(pgf.ShowDialog() == DialogResult.OK)
			{
				byte[] pbEntropy = EntropyForm.CollectEntropyIfEnabled(pgf.SelectedOptions);
				ProtectedString psNew = PasswordGenerator.Generate(pgf.SelectedOptions, true, pbEntropy);

				byte[] pbNew = psNew.ReadUTF8();
				m_secPassword.SetPassword(pbNew);
				m_secRepeat.SetPassword(pbNew);
				Array.Clear(pbNew, 0, pbNew.Length);
			}

			EnableControlsEx();
		}

		private void OnProfilesDynamicMenuClick(object sender, DynamicMenuEventArgs e)
		{
			foreach(PasswordGenerationOptions pwgo in m_vPwGenOptions)
			{
				if(pwgo.ProfileName == e.ItemName)
				{
					ProtectedString ps = PasswordGenerator.Generate(pwgo, m_pwDatabase.MemoryProtection.ProtectPassword, null);
					byte[] pbNew = ps.ReadUTF8();
					m_secPassword.SetPassword(pbNew);
					m_secRepeat.SetPassword(pbNew);
					Array.Clear(pbNew, 0, pbNew.Length);
					break;
				}
			}
		}

		private void OnPwGenClick(object sender, EventArgs e)
		{
			m_vPwGenOptions.Clear();

			byte[] pbCurUTF8 = m_secPassword.ToUTF8();
			ProtectedString psCur = new ProtectedString(true, pbCurUTF8);
			Array.Clear(pbCurUTF8, 0, pbCurUTF8.Length);
			PasswordGenerationOptions pwgoDerive = PasswordGenerator.DeriveOptionsFromPassword(psCur);
			pwgoDerive.ProfileName = DeriveFromPrevious;
			m_vPwGenOptions.Add(pwgoDerive);

			PwGeneratorUtil.AddStandardProfilesIfNoneAvailable();

			uint uProfileIndex = 0;
			while(true)
			{
				string strProfile = AppConfigEx.GetValue(AppDefs.ConfigKeys.PwGenProfile.Key +
					uProfileIndex.ToString(), null);
				if(strProfile == null) break;

				PasswordGenerationOptions pwgo =
					PasswordGenerationOptions.UnserializeFromString(strProfile);
				if(pwgo.ProfileName.Length == 0) { Debug.Assert(false); ++uProfileIndex; continue; }

				m_vPwGenOptions.Add(pwgo);
				++uProfileIndex;
			}

			m_dynGenProfiles.Clear();
			m_dynGenProfiles.AddItem(DeriveFromPrevious, KeePass.Properties.Resources.B16x16_CompFile);

			if(m_vPwGenOptions.Count > 0) m_dynGenProfiles.AddSeparator();
			foreach(PasswordGenerationOptions pwgo in m_vPwGenOptions)
			{
				if(pwgo.ProfileName != DeriveFromPrevious)
					m_dynGenProfiles.AddItem(pwgo.ProfileName,
						KeePass.Properties.Resources.B16x16_KOrganizer);
			}

			m_ctxPwGen.Show(m_btnGenPw, new Point(0, m_btnGenPw.Height));
		}

		private void OnPickBackgroundColor(object sender, EventArgs e)
		{
			m_dlgColorSel.Color = m_clrBackground;

			if(m_dlgColorSel.ShowDialog() == DialogResult.OK)
			{
				m_clrBackground = m_dlgColorSel.Color;
				m_btnPickBgColor.Image = CreateColorButtonImage(m_btnPickBgColor,
					m_clrBackground);
			}
		}

		private void OnCustomBackgroundColorCheckedChanged(object sender, EventArgs e)
		{
			EnableControlsEx();
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			GlobalWindowManager.RemoveWindow(this);
		}
	}
}
