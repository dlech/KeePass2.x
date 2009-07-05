/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2009 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePass.App;
using KeePass.App.Configuration;
using KeePass.UI;
using KeePass.Resources;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Utility;

using NativeLib = KeePassLib.Native.NativeLib;

namespace KeePass.Forms
{
	/// <summary>
	/// Options dialog. In this dialog, global program options can be configured.
	/// </summary>
	public partial class OptionsForm : Form
	{
		private ImageList m_ilIcons;
		private BannerStyle m_curBannerStyle = BannerStyle.KeePassWin32;
		private bool m_bBlockUIUpdate = false;
		private bool m_bLoadingSettings = false;

		private CheckedLVItemDXList m_cdxSecurityOptions = new CheckedLVItemDXList();
		private CheckedLVItemDXList m_cdxPolicy = new CheckedLVItemDXList();
		private CheckedLVItemDXList m_cdxGuiOptions = new CheckedLVItemDXList();
		private HotKeyControlEx m_hkGlobalAutoType = null;
		private HotKeyControlEx m_hkShowWindow = null;

		private Keys m_kPrevATHKKey = Keys.None;
		private Keys m_kPrevSWHKKey = Keys.None;

		private CheckedLVItemDXList m_cdxAdvanced = new CheckedLVItemDXList();

		/// <summary>
		/// Default constructor.
		/// </summary>
		public OptionsForm()
		{
			InitializeComponent();
			Program.Translation.ApplyTo(this);
		}

		/// <summary>
		/// Initialize the dialog. This function must be called before the dialog
		/// is displayed.
		/// </summary>
		/// <param name="ilIcons">Image list to use for displaying images.</param>
		public void InitEx(ImageList ilIcons)
		{
			Debug.Assert(ilIcons != null);
			m_ilIcons = ilIcons;
		}

		private void CreateDialogBanner(BannerStyle bsStyle)
		{
			if(bsStyle == m_curBannerStyle) return;

			m_curBannerStyle = bsStyle;

			m_bannerImage.Image = BannerFactory.CreateBanner(m_bannerImage.Width,
				m_bannerImage.Height, bsStyle,
				Properties.Resources.B48x48_KCMSystem, KPRes.Options,
				KPRes.OptionsDesc);
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			GlobalWindowManager.AddWindow(this);

			this.Icon = Properties.Resources.KeePass;

			Debug.Assert(m_ilIcons != null);
			if(m_ilIcons != null)
			{
				m_tabMain.ImageList = m_ilIcons;

				m_tabSecurity.ImageIndex = (int)PwIcon.TerminalEncrypted;
				m_tabPolicy.ImageIndex = (int)PwIcon.List;
				m_tabGui.ImageIndex = (int)PwIcon.Screen;
				m_tabIntegration.ImageIndex = (int)PwIcon.Console;
				m_tabAdvanced.ImageIndex = (int)PwIcon.ClipboardReady;
			}

			uint uTab = Program.Config.Defaults.OptionsTabIndex;
			if(uTab < (uint)m_tabMain.TabPages.Count)
				m_tabMain.SelectedTab = m_tabMain.TabPages[(int)uTab];

			m_cmbBannerStyle.Items.Add("(" + KPRes.CurrentStyle + ")");
			m_cmbBannerStyle.Items.Add("WinXP Login");
			m_cmbBannerStyle.Items.Add("WinVista Black");
			m_cmbBannerStyle.Items.Add("KeePass Win32");
			m_cmbBannerStyle.Items.Add("Blue Carbon");

			CreateDialogBanner(BannerStyle.Default);
			m_cmbBannerStyle.SelectedIndex = (int)BannerStyle.Default;

			int nWidth = m_lvPolicy.ClientRectangle.Width - 36;
			m_lvPolicy.Columns.Add(KPRes.Feature, nWidth / 4);
			m_lvPolicy.Columns.Add(KPRes.Description, (nWidth * 3) / 4);

			m_hkGlobalAutoType = HotKeyControlEx.ReplaceTextBox(m_grpHotKeys, m_tbGlobalAutoType);
			m_hkShowWindow = HotKeyControlEx.ReplaceTextBox(m_grpHotKeys, m_tbShowWindowHotKey);

			if(NativeLib.IsUnix() == false)
			{
				UIUtil.SetShield(m_btnFileExtCreate, true);
				UIUtil.SetShield(m_btnFileExtRemove, true);
			}
			else // Unix
			{
				m_hkGlobalAutoType.Enabled = m_hkShowWindow.Enabled = false;
				m_btnFileExtCreate.Enabled = m_btnFileExtRemove.Enabled = false;
				m_cbAutoRun.Enabled = false;
			}

			LoadOptions();

			if(Program.Config.Meta.IsEnforcedConfiguration)
				m_lvPolicy.Enabled = false;

			UpdateUIState();
		}

		private void LoadOptions()
		{
			m_bLoadingSettings = true;

			LoadSecurityOptions();
			LoadPolicyOptions();
			LoadGuiOptions();
			LoadIntegrationOptions();
			LoadAdvancedOptions();

			m_bLoadingSettings = false;
		}

		private void LoadSecurityOptions()
		{
			m_numLockAfterTime.Value = Program.Config.Security.WorkspaceLocking.LockAfterTime;
			m_cbLockAfterTime.Checked = (m_numLockAfterTime.Value > 0);

			int nDefaultExpireDays = Program.Config.Defaults.NewEntryExpiresInDays;
			if(nDefaultExpireDays < 0)
				m_cbDefaultExpireDays.Checked = false;
			else
			{
				m_cbDefaultExpireDays.Checked = true;
				try { m_numDefaultExpireDays.Value = nDefaultExpireDays; }
				catch(Exception) { Debug.Assert(false); }
			}

			int nClipClear = Program.Config.Security.ClipboardClearAfterSeconds;
			if(nClipClear >= 0)
			{
				m_cbClipClearTime.Checked = true;
				m_numClipClearTime.Value = nClipClear;
			}
			else m_cbClipClearTime.Checked = false;

			m_lvSecurityOptions.Columns.Add(string.Empty, 200); // Resize below

			ListViewGroup lvg = new ListViewGroup(KPRes.Options);
			m_lvSecurityOptions.Groups.Add(lvg);
			Debug.Assert(lvg.ListView == m_lvSecurityOptions);

			m_cdxSecurityOptions.CreateItem(Program.Config.Security.WorkspaceLocking,
				"LockOnWindowMinimize", m_lvSecurityOptions, lvg, KPRes.LockOnMinimize);
			m_cdxSecurityOptions.CreateItem(Program.Config.Security.WorkspaceLocking,
				"LockOnSessionLock", m_lvSecurityOptions, lvg, KPRes.LockOnSessionLock);
			m_cdxSecurityOptions.CreateItem(Program.Config.Security, "ClipboardClearOnExit",
				m_lvSecurityOptions, lvg, KPRes.ClipboardClearOnExit);
			m_cdxSecurityOptions.CreateItem(Program.Config.Security.WorkspaceLocking,
				"ExitInsteadOfLockingAfterTime", m_lvSecurityOptions, lvg, KPRes.ExitInsteadOfLockingAfterTime);

			if(NativeLib.IsLibraryInstalled())
				m_cdxSecurityOptions.CreateItem(Program.Config.Native, "NativeKeyTransformations",
					m_lvSecurityOptions, lvg, KPRes.NativeLibUse);

			m_cdxSecurityOptions.CreateItem(Program.Config.Security,
				"UseClipboardViewerIgnoreFormat", m_lvSecurityOptions, lvg,
				KPRes.ClipboardViewerIgnoreFormat + " " + KPRes.NotRecommended);

			m_cdxSecurityOptions.UpdateData(false);
			m_lvSecurityOptions.Columns[0].Width = m_lvSecurityOptions.ClientRectangle.Width - 36;
		}

		private void LoadPolicyOption(string strPropertyName, string strDisplayName,
			string strDisplayDesc)
		{
			ListViewItem lvi = m_cdxPolicy.CreateItem(Program.Config.Security.Policy,
				strPropertyName, m_lvPolicy, null, strDisplayName + "*");
			lvi.SubItems.Add(strDisplayDesc);
		}

		private void LoadPolicyOptions()
		{
			LoadPolicyOption("Plugins", KPRes.Plugins, KPRes.PolicyPluginsDesc);
			LoadPolicyOption("Export", KPRes.Export, KPRes.PolicyExportDesc);
			LoadPolicyOption("Import", KPRes.Import, KPRes.PolicyImportDesc);
			LoadPolicyOption("Print", KPRes.Print, KPRes.PolicyPrintDesc);
			LoadPolicyOption("SaveFile", KPRes.SaveDatabase, KPRes.PolicySaveDatabaseDesc);
			LoadPolicyOption("AutoType", KPRes.AutoType, KPRes.PolicyAutoTypeDesc);
			LoadPolicyOption("CopyToClipboard", KPRes.Copy, KPRes.PolicyClipboardDesc);
			LoadPolicyOption("DragDrop", KPRes.DragDrop, KPRes.PolicyDragDropDesc);

			m_cdxPolicy.UpdateData(false);
		}

		private void LoadGuiOptions()
		{
			m_lvGuiOptions.Columns.Add(KPRes.Options, 200); // Resize below

			ListViewGroup lvg = new ListViewGroup(KPRes.MainWindow);
			m_lvGuiOptions.Groups.Add(lvg);
			Debug.Assert(lvg.ListView == m_lvGuiOptions);

			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "CloseButtonMinimizesWindow",
				m_lvGuiOptions, lvg, KPRes.CloseButtonMinimizes);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "MinimizeToTray",
				m_lvGuiOptions, lvg, KPRes.MinimizeToTray);
			m_cdxGuiOptions.CreateItem(Program.Config.UI.TrayIcon, "ShowOnlyIfTrayed",
				m_lvGuiOptions, lvg, KPRes.ShowTrayOnlyIfTrayed);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "ShowFullPathInTitle",
				m_lvGuiOptions, lvg, KPRes.ShowFullPathInTitleBar);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "MinimizeAfterClipboardCopy",
				m_lvGuiOptions, lvg, KPRes.MinimizeAfterCopy);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "MinimizeAfterLocking",
				m_lvGuiOptions, lvg, KPRes.MinimizeAfterLocking);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "MinimizeAfterOpeningDatabase",
				m_lvGuiOptions, lvg, KPRes.MinimizeAfterOpeningDatabase);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "QuickFindExcludeExpired",
				m_lvGuiOptions, lvg, KPRes.QuickSearchExcludeExpired);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "FocusResultsAfterQuickFind",
				m_lvGuiOptions, lvg, KPRes.FocusResultsAfterQuickFind);

			lvg = new ListViewGroup(KPRes.EntryList);
			m_lvGuiOptions.Groups.Add(lvg);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "ShowGridLines",
				m_lvGuiOptions, lvg, KPRes.ShowGridLines);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "EntryListAutoResizeColumns",
				m_lvGuiOptions, lvg, KPRes.EntryListAutoResizeColumns);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "CopyUrlsInsteadOfOpening",
				m_lvGuiOptions, lvg, KPRes.CopyUrlsInsteadOfOpening);

			m_cdxGuiOptions.UpdateData(false);
			m_lvGuiOptions.Columns[0].Width = m_lvGuiOptions.ClientRectangle.Width - 36;

			try { m_numMruCount.Value = Program.Config.Application.MostRecentlyUsed.MaxItemCount; }
			catch(Exception) { Debug.Assert(false); m_numMruCount.Value = AceMru.DefaultMaxItemCount; }
		}

		private void LoadIntegrationOptions()
		{
			Keys kAT = (Keys)Program.Config.Integration.HotKeyGlobalAutoType;
			m_hkGlobalAutoType.HotKey = (kAT & Keys.KeyCode);
			m_hkGlobalAutoType.HotKeyModifiers = (kAT & Keys.Modifiers);
			m_hkGlobalAutoType.RenderHotKey();
			m_kPrevATHKKey = (m_hkGlobalAutoType.HotKey | m_hkGlobalAutoType.HotKeyModifiers);

			Keys kSW = (Keys)Program.Config.Integration.HotKeyShowWindow;
			m_hkShowWindow.HotKey = (kSW & Keys.KeyCode);
			m_hkShowWindow.HotKeyModifiers = (kSW & Keys.Modifiers);
			m_hkShowWindow.RenderHotKey();
			m_kPrevSWHKKey = (m_hkShowWindow.HotKey | m_hkShowWindow.HotKeyModifiers);

			m_cbAutoRun.Checked = ShellUtil.GetStartWithWindows(AppDefs.AutoRunName);
			m_cbSingleClickTrayAction.Checked = Program.Config.UI.TrayIcon.SingleClickDefault;

			string strOverride = Program.Config.Integration.UrlOverride;
			m_cbUrlOverride.Checked = (strOverride.Length > 0);
			m_tbUrlOverride.Text = strOverride;
		}

		private void LoadAdvancedOptions()
		{
			m_lvAdvanced.Columns.Add(string.Empty, 200); // Resize below

			ListViewGroup lvg = new ListViewGroup(KPRes.StartAndExit);
			m_lvAdvanced.Groups.Add(lvg);
			m_cdxAdvanced.CreateItem(Program.Config.Application.Start, "OpenLastFile",
				m_lvAdvanced, lvg, KPRes.AutoRememberOpenLastFile);
			m_cdxAdvanced.CreateItem(Program.Config.Integration, "LimitToSingleInstance",
				m_lvAdvanced, lvg, KPRes.LimitSingleInstance);
			m_cdxAdvanced.CreateItem(Program.Config.Application.Start, "CheckForUpdate",
				m_lvAdvanced, lvg, KPRes.CheckForUpdAtStart);
			m_cdxAdvanced.CreateItem(Program.Config.Application.Start, "MinimizedAndLocked",
				m_lvAdvanced, lvg, KPRes.StartMinimizedAndLocked);
			m_cdxAdvanced.CreateItem(Program.Config.Application.FileClosing, "AutoSave",
				m_lvAdvanced, lvg, KPRes.AutoSaveAtExit);

			lvg = new ListViewGroup(KPRes.AfterDatabaseOpen);
			m_lvAdvanced.Groups.Add(lvg);
			m_cdxAdvanced.CreateItem(Program.Config.Application.FileOpening, "ShowExpiredEntries",
				m_lvAdvanced, lvg, KPRes.AutoShowExpiredEntries);
			m_cdxAdvanced.CreateItem(Program.Config.Application.FileOpening, "ShowSoonToExpireEntries",
				m_lvAdvanced, lvg, KPRes.AutoShowSoonToExpireEntries);

			lvg = new ListViewGroup(KPRes.AutoType);
			m_lvAdvanced.Groups.Add(lvg);
			m_cdxAdvanced.CreateItem(Program.Config.Integration, "AutoTypePrependInitSequenceForIE",
				m_lvAdvanced, lvg, KPRes.AutoTypePrependInitSeqForIE);
			m_cdxAdvanced.CreateItem(Program.Config.Integration, "AutoTypeReleaseAltWithKeyPress",
				m_lvAdvanced, lvg, KPRes.AutoTypeReleaseAltWithKeyPress);

			lvg = new ListViewGroup(KPRes.Advanced);
			m_lvAdvanced.Groups.Add(lvg);
			m_cdxAdvanced.CreateItem(Program.Config.Integration, "SearchKeyFilesOnRemovableMedia",
				m_lvAdvanced, lvg, KPRes.SearchKeyFilesOnRemovable);
			m_cdxAdvanced.CreateItem(Program.Config.Defaults, "RememberKeyFilePaths",
				m_lvAdvanced, lvg, KPRes.RememberKeyFilePaths);
			m_cdxAdvanced.CreateItem(Program.Config.UI.Hiding, "SeparateHidingSettings",
				m_lvAdvanced, lvg, KPRes.RememberHidingSettings);
			m_cdxAdvanced.CreateItem(Program.Config.UI, "OptimizeForScreenReader",
				m_lvAdvanced, lvg, KPRes.OptimizeForScreenReader);

			m_cdxAdvanced.UpdateData(false);
			m_lvAdvanced.Columns[0].Width = m_lvAdvanced.ClientRectangle.Width - 36;
		}

		private bool ValidateOptions()
		{
			bool bAltMod = false;
			bAltMod |= ((m_hkGlobalAutoType.HotKeyModifiers == Keys.Alt) ||
				(m_hkGlobalAutoType.HotKeyModifiers == (Keys.Alt | Keys.Shift)));
			bAltMod |= ((m_hkShowWindow.HotKeyModifiers == Keys.Alt) ||
				(m_hkShowWindow.HotKeyModifiers == (Keys.Alt | Keys.Shift)));

			if(bAltMod)
			{
				if(!MessageService.AskYesNo(KPRes.HotKeyAltOnly + MessageService.NewParagraph +
					KPRes.HotKeyAltOnlyHint + MessageService.NewParagraph +
					KPRes.HotKeyAltOnlyQuestion, null, false))
					return false;
			}

			return true;
		}

		private void SaveOptions()
		{
			if(!m_cbLockAfterTime.Checked)
				Program.Config.Security.WorkspaceLocking.LockAfterTime = 0;
			else
				Program.Config.Security.WorkspaceLocking.LockAfterTime =
					(uint)m_numLockAfterTime.Value;

			if(m_cbDefaultExpireDays.Checked)
				Program.Config.Defaults.NewEntryExpiresInDays =
					(int)m_numDefaultExpireDays.Value;
			else Program.Config.Defaults.NewEntryExpiresInDays = -1;

			if(m_cbClipClearTime.Checked)
				Program.Config.Security.ClipboardClearAfterSeconds =
					(int)m_numClipClearTime.Value;
			else Program.Config.Security.ClipboardClearAfterSeconds = -1;

			m_cdxSecurityOptions.UpdateData(true);

			NativeLib.AllowNative = Program.Config.Native.NativeKeyTransformations;

			m_cdxPolicy.UpdateData(true);
			m_cdxGuiOptions.UpdateData(true);

			if(m_cmbBannerStyle.SelectedIndex != (int)BannerStyle.Default)
				Program.Config.UI.BannerStyle = (BannerStyle)
					m_cmbBannerStyle.SelectedIndex;

			Program.Config.Application.MostRecentlyUsed.MaxItemCount =
				(uint)m_numMruCount.Value;

			ChangeHotKey(ref m_kPrevATHKKey, m_hkGlobalAutoType, true,
				AppDefs.GlobalHotKeyId.AutoType);
			ChangeHotKey(ref m_kPrevSWHKKey, m_hkShowWindow, false,
				AppDefs.GlobalHotKeyId.ShowWindow);

			Program.Config.UI.TrayIcon.SingleClickDefault = m_cbSingleClickTrayAction.Checked;

			if(m_cbUrlOverride.Checked)
				Program.Config.Integration.UrlOverride = m_tbUrlOverride.Text;
			else Program.Config.Integration.UrlOverride = string.Empty;

			m_cdxAdvanced.UpdateData(true);
		}

		private void CleanUpEx()
		{
			int nTab = m_tabMain.SelectedIndex;
			if((nTab >= 0) && (nTab < m_tabMain.TabPages.Count))
				Program.Config.Defaults.OptionsTabIndex = (uint)nTab;
		}

		private static void ChangeHotKey(ref Keys kPrevHK, HotKeyControlEx hkControl,
			bool bAutoTypeHotKey, int nHotKeyID)
		{
			Keys kNew = (hkControl.HotKey | hkControl.HotKeyModifiers);
			if(kPrevHK != kNew)
			{
				kPrevHK = kNew;

				if(bAutoTypeHotKey)
					Program.Config.Integration.HotKeyGlobalAutoType = (ulong)kNew;
				else
					Program.Config.Integration.HotKeyShowWindow = (ulong)kNew;

				HotKeyManager.UnregisterHotKey(nHotKeyID);
				if(kPrevHK != Keys.None)
					HotKeyManager.RegisterHotKey(nHotKeyID, kPrevHK);
			}
		}

		private void UpdateUIState()
		{
			if(m_bBlockUIUpdate) return;
			m_bBlockUIUpdate = true;

			m_numLockAfterTime.Enabled = m_cbLockAfterTime.Checked;
			m_numDefaultExpireDays.Enabled = m_cbDefaultExpireDays.Checked;
			m_numClipClearTime.Enabled = m_cbClipClearTime.Checked;

			m_tbUrlOverride.Enabled = m_cbUrlOverride.Checked;

			m_bBlockUIUpdate = false;
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			if(!ValidateOptions()) { this.DialogResult = DialogResult.None; return; }

			SaveOptions();
			CleanUpEx();
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
			CleanUpEx();
		}

		private void OnBannerStyleSelectedChanged(object sender, EventArgs e)
		{
			int nIndex = m_cmbBannerStyle.SelectedIndex;
			
			BannerStyle bs = (BannerStyle)nIndex;
			CreateDialogBanner(bs);
		}

		private void OnLockAfterTimeCheckedChanged(object sender, EventArgs e)
		{
			UpdateUIState();
		}

		private void OnLockAfterTimeValueChanged(object sender, EventArgs e)
		{
			UpdateUIState();
		}

		private void OnBtnSelListFont(object sender, EventArgs e)
		{
			AceFont fOld = Program.Config.UI.StandardFont;
			if(fOld.OverrideUIDefault) m_fontLists.Font = fOld.ToFont();

			if(m_fontLists.ShowDialog() == DialogResult.OK)
			{
				Program.Config.UI.StandardFont = new AceFont(m_fontLists.Font);
				Program.Config.UI.StandardFont.OverrideUIDefault = true;
			}
		}

		private void OnDefaultExpireDaysCheckedChanged(object sender, EventArgs e)
		{
			UpdateUIState();
		}

		private void OnBtnFileExtCreate(object sender, EventArgs e)
		{
			// ShellUtil.RegisterExtension(AppDefs.FileExtension.FileExt, AppDefs.FileExtension.ExtId,
			//	KPRes.FileExtName, WinUtil.GetExecutable(), PwDefs.ShortProductName, true);
			WinUtil.RunElevated(WinUtil.GetExecutable(), "/" +
				AppDefs.CommandLineOptions.FileExtRegister, false);
		}

		private void OnBtnFileExtRemove(object sender, EventArgs e)
		{
			// ShellUtil.UnregisterExtension(AppDefs.FileExtension.FileExt,
			//	AppDefs.FileExtension.ExtId);
			WinUtil.RunElevated(WinUtil.GetExecutable(), "/" +
				AppDefs.CommandLineOptions.FileExtUnregister, false);
		}

		private void OnCheckedChangedAutoRun(object sender, EventArgs e)
		{
			if(m_bLoadingSettings) return;

			bool bRequested = m_cbAutoRun.Checked;
			bool bCurrent = ShellUtil.GetStartWithWindows(AppDefs.AutoRunName);

			if(bRequested != bCurrent)
			{
				string strPath = WinUtil.GetExecutable().Trim();
				if(strPath.StartsWith("\"") == false)
					strPath = "\"" + strPath + "\"";
				ShellUtil.SetStartWithWindows(AppDefs.AutoRunName, strPath,
					bRequested);

				bool bNew = ShellUtil.GetStartWithWindows(AppDefs.AutoRunName);

				if(bNew != bRequested)
					m_cbAutoRun.Checked = bNew;
			}
		}

		private void OnOverrideURLsCheckedChanged(object sender, EventArgs e)
		{
			UpdateUIState();
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			GlobalWindowManager.RemoveWindow(this);
		}

		private void OnPolicyInfoLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			AppHelp.ShowHelp(AppDefs.HelpTopics.AppPolicy, null);
		}

		private void OnClipboardClearTimeCheckedChanged(object sender, EventArgs e)
		{
			UpdateUIState();
		}

		private void OnBtnTriggers(object sender, EventArgs e)
		{
			EcasTriggersForm f = new EcasTriggersForm();
			f.InitEx(Program.TriggerSystem, m_ilIcons);
			f.ShowDialog();
		}
	}
}
