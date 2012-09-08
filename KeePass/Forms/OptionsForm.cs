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

		private CheckedLVItemDXList m_cdxSecurityOptions = null;
		private CheckedLVItemDXList m_cdxPolicy = null;
		private CheckedLVItemDXList m_cdxGuiOptions = null;
		private CheckedLVItemDXList m_cdxAdvanced = null;

		private HotKeyControlEx m_hkGlobalAutoType = null;
		private HotKeyControlEx m_hkSelectedAutoType = null;
		private HotKeyControlEx m_hkShowWindow = null;
		private Keys m_kPrevATHKKey = Keys.None;
		private Keys m_kPrevATSHKKey = Keys.None;
		private Keys m_kPrevSWHKKey = Keys.None;

		private AceUrlSchemeOverrides m_aceUrlSchemeOverrides = null;

		private bool m_bInitialTsRenderer = true;
		public bool RequiresUIReinitialize
		{
			get { return Program.Config.UI.UseCustomToolStripRenderer != m_bInitialTsRenderer; }
		}

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
			// Can be invoked by tray command; don't use CenterParent
			Debug.Assert(this.StartPosition == FormStartPosition.CenterScreen);

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

			m_aceUrlSchemeOverrides = Program.Config.Integration.UrlSchemeOverrides.CloneDeep();

			m_cmbBannerStyle.Items.Add("(" + KPRes.CurrentStyle + ")");
			m_cmbBannerStyle.Items.Add("WinXP Login");
			m_cmbBannerStyle.Items.Add("WinVista Black");
			m_cmbBannerStyle.Items.Add("KeePass Win32");
			m_cmbBannerStyle.Items.Add("Blue Carbon");

			CreateDialogBanner(BannerStyle.Default); // Default forces generation
			m_cmbBannerStyle.SelectedIndex = (int)BannerStyle.Default;
			if((BannerFactory.CustomGenerator != null) ||
				AppConfigEx.IsOptionEnforced(Program.Config.UI, "BannerStyle"))
			{
				m_lblBannerStyle.Enabled = false;
				m_cmbBannerStyle.Enabled = false;
			}

			int nWidth = m_lvPolicy.ClientRectangle.Width - UIUtil.GetVScrollBarWidth() - 1;
			m_lvPolicy.Columns.Add(KPRes.Feature, (nWidth * 10) / 29);
			m_lvPolicy.Columns.Add(KPRes.Description, (nWidth * 19) / 29);

			m_hkGlobalAutoType = HotKeyControlEx.ReplaceTextBox(m_grpHotKeys,
				m_tbGlobalAutoType, true);
			m_hkSelectedAutoType = HotKeyControlEx.ReplaceTextBox(m_grpHotKeys,
				m_tbSelAutoTypeHotKey, true);
			m_hkShowWindow = HotKeyControlEx.ReplaceTextBox(m_grpHotKeys,
				m_tbShowWindowHotKey, true);

			if(!NativeLib.IsUnix())
			{
				UIUtil.SetShield(m_btnFileExtCreate, true);
				UIUtil.SetShield(m_btnFileExtRemove, true);

				m_linkHotKeyHelp.Visible = false;
			}
			else // Unix
			{
				Program.Config.Integration.HotKeyGlobalAutoType = (ulong)Keys.None;
				Program.Config.Integration.HotKeySelectedAutoType = (ulong)Keys.None;
				Program.Config.Integration.HotKeyShowWindow = (ulong)Keys.None;

				m_hkGlobalAutoType.Enabled = m_hkSelectedAutoType.Enabled =
					m_hkShowWindow.Enabled = false;
				m_btnFileExtCreate.Enabled = m_btnFileExtRemove.Enabled = false;
				m_cbAutoRun.Enabled = false;
			}

			UIUtil.SetExplorerTheme(m_lvSecurityOptions, false);
			UIUtil.SetExplorerTheme(m_lvPolicy, false);
			UIUtil.SetExplorerTheme(m_lvGuiOptions, false);
			UIUtil.SetExplorerTheme(m_lvAdvanced, false);

			AppConfigEx.ClearXmlPathCache();

			LoadOptions();

			// if(Program.Config.Meta.IsEnforcedConfiguration)
			//	m_lvPolicy.Enabled = false;

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
			AceWorkspaceLocking aceWL = Program.Config.Security.WorkspaceLocking;

			uint uLockTime = aceWL.LockAfterTime;
			bool bLockTime = (uLockTime > 0);
			m_numLockAfterTime.Value = (bLockTime ? uLockTime : 300);
			m_cbLockAfterTime.Checked = bLockTime;
			if(AppConfigEx.IsOptionEnforced(aceWL, "LockAfterTime"))
				m_cbLockAfterTime.Enabled = false;

			uLockTime = aceWL.LockAfterGlobalTime;
			bLockTime = (uLockTime > 0);
			m_numLockAfterGlobalTime.Value = (bLockTime ? uLockTime : 240);
			m_cbLockAfterGlobalTime.Checked = bLockTime;
			if(AppConfigEx.IsOptionEnforced(aceWL, "LockAfterGlobalTime"))
				m_cbLockAfterGlobalTime.Enabled = false;

			int nDefaultExpireDays = Program.Config.Defaults.NewEntryExpiresInDays;
			if(nDefaultExpireDays < 0)
				m_cbDefaultExpireDays.Checked = false;
			else
			{
				m_cbDefaultExpireDays.Checked = true;
				try { m_numDefaultExpireDays.Value = nDefaultExpireDays; }
				catch(Exception) { Debug.Assert(false); }
			}
			if(AppConfigEx.IsOptionEnforced(Program.Config.Defaults, "NewEntryExpiresInDays"))
				m_cbDefaultExpireDays.Enabled = false;

			int nClipClear = Program.Config.Security.ClipboardClearAfterSeconds;
			if(nClipClear >= 0)
			{
				m_cbClipClearTime.Checked = true;
				m_numClipClearTime.Value = nClipClear;
			}
			else m_cbClipClearTime.Checked = false;
			if(AppConfigEx.IsOptionEnforced(Program.Config.Security, "ClipboardClearAfterSeconds"))
				m_cbClipClearTime.Enabled = false;

			m_lvSecurityOptions.Columns.Add(string.Empty, 200); // Resize below

			ListViewGroup lvg = new ListViewGroup(KPRes.Options);
			m_lvSecurityOptions.Groups.Add(lvg);
			Debug.Assert(lvg.ListView == m_lvSecurityOptions);

			m_cdxSecurityOptions = new CheckedLVItemDXList(m_lvSecurityOptions, true);

			m_cdxSecurityOptions.CreateItem(aceWL, "LockOnWindowMinimize",
				lvg, KPRes.LockOnMinimize);
			m_cdxSecurityOptions.CreateItem(aceWL, "LockOnSessionSwitch",
				lvg, KPRes.LockOnSessionSwitch);
			m_cdxSecurityOptions.CreateItem(aceWL, "LockOnSuspend",
				lvg, KPRes.LockOnSuspend);
			m_cdxSecurityOptions.CreateItem(aceWL, "LockOnRemoteControlChange",
				lvg, KPRes.LockOnRemoteControlChange);
			m_cdxSecurityOptions.CreateItem(aceWL, "ExitInsteadOfLockingAfterTime",
				lvg, KPRes.ExitInsteadOfLockingAfterTime);
			m_cdxSecurityOptions.CreateItem(aceWL, "AlwaysExitInsteadOfLocking",
				lvg, KPRes.ExitInsteadOfLockingAlways);
			m_cdxSecurityOptions.CreateItem(Program.Config.Security, "ClipboardClearOnExit",
				lvg, KPRes.ClipboardClearOnExit);
			m_cdxSecurityOptions.CreateItem(Program.Config.Security,
				"UseClipboardViewerIgnoreFormat", lvg,
				KPRes.ClipboardViewerIgnoreFormat + " " + KPRes.NotRecommended);

			if(NativeLib.IsLibraryInstalled())
				m_cdxSecurityOptions.CreateItem(Program.Config.Native, "NativeKeyTransformations",
					lvg, KPRes.NativeLibUse);

			m_cdxSecurityOptions.CreateItem(Program.Config.Security, "MasterKeyOnSecureDesktop",
				lvg, KPRes.MasterKeyOnSecureDesktop);
			m_cdxSecurityOptions.CreateItem(Program.Config.Security, "ClearKeyCommandLineParams",
				lvg, KPRes.ClearKeyCmdLineParams);

			m_cdxSecurityOptions.UpdateData(false);
			m_lvSecurityOptions.Columns[0].Width = m_lvSecurityOptions.ClientRectangle.Width -
				UIUtil.GetVScrollBarWidth() - 1;
		}

		private void LoadPolicyOption(string strPropertyName, string strDisplayName,
			string strDisplayDesc)
		{
			ListViewItem lvi = m_cdxPolicy.CreateItem(Program.Config.Security.Policy,
				strPropertyName, null, strDisplayName + "*");
			lvi.SubItems.Add(strDisplayDesc);
		}

		private void LoadPolicyOptions()
		{
			m_cdxPolicy = new CheckedLVItemDXList(m_lvPolicy, true);

			LoadPolicyOption("Plugins", KPRes.Plugins, KPRes.PolicyPluginsDesc);
			LoadPolicyOption("Export", KPRes.Export, KPRes.PolicyExportDesc);
			LoadPolicyOption("ExportNoKey", KPRes.Export + " - " + KPRes.NoKeyRepeat,
				KPRes.PolicyExportNoKeyDesc);
			LoadPolicyOption("Import", KPRes.Import, KPRes.PolicyImportDesc);
			LoadPolicyOption("Print", KPRes.Print, KPRes.PolicyPrintDesc);
			LoadPolicyOption("PrintNoKey", KPRes.Print + " - " + KPRes.NoKeyRepeat,
				KPRes.PolicyPrintNoKeyDesc);
			LoadPolicyOption("NewFile", KPRes.NewDatabase, KPRes.PolicyNewDatabaseDesc);
			LoadPolicyOption("SaveFile", KPRes.SaveDatabase, KPRes.PolicySaveDatabaseDesc);
			LoadPolicyOption("AutoType", KPRes.AutoType, KPRes.PolicyAutoTypeDesc);
			LoadPolicyOption("AutoTypeWithoutContext", KPRes.AutoType + " - " +
				KPRes.WithoutContext, KPRes.PolicyAutoTypeWithoutContextDesc);
			LoadPolicyOption("CopyToClipboard", KPRes.Copy, KPRes.PolicyClipboardDesc);
			LoadPolicyOption("CopyWholeEntries", KPRes.CopyWholeEntries, KPRes.PolicyCopyWholeEntriesDesc);
			LoadPolicyOption("DragDrop", KPRes.DragDrop, KPRes.PolicyDragDropDesc);
			LoadPolicyOption("UnhidePasswords", KPRes.UnhidePasswords, KPRes.UnhidePasswordsDesc);
			LoadPolicyOption("ChangeMasterKey", KPRes.ChangeMasterKey, KPRes.PolicyChangeMasterKey);
			LoadPolicyOption("ChangeMasterKeyNoKey", KPRes.ChangeMasterKey + " - " +
				KPRes.NoKeyRepeat, KPRes.PolicyChangeMasterKeyNoKeyDesc);
			LoadPolicyOption("EditTriggers", KPRes.TriggersEdit, KPRes.PolicyTriggersEditDesc);

			m_cdxPolicy.UpdateData(false);
		}

		private void LoadGuiOptions()
		{
			m_bInitialTsRenderer = Program.Config.UI.UseCustomToolStripRenderer;

			m_lvGuiOptions.Columns.Add(KPRes.Options, 200); // Resize below

			ListViewGroup lvg = new ListViewGroup(KPRes.MainWindow);
			m_lvGuiOptions.Groups.Add(lvg);
			Debug.Assert(lvg.ListView == m_lvGuiOptions);

			m_cdxGuiOptions = new CheckedLVItemDXList(m_lvGuiOptions, true);

			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "CloseButtonMinimizesWindow",
				lvg, KPRes.CloseButtonMinimizes);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "MinimizeToTray",
				lvg, KPRes.MinimizeToTray);
			m_cdxGuiOptions.CreateItem(Program.Config.UI.TrayIcon, "ShowOnlyIfTrayed",
				lvg, KPRes.ShowTrayOnlyIfTrayed);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "ShowFullPathInTitle",
				lvg, KPRes.ShowFullPathInTitleBar);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "DropToBackAfterClipboardCopy",
				lvg, KPRes.DropToBackOnCopy);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "MinimizeAfterClipboardCopy",
				lvg, KPRes.MinimizeAfterCopy);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "MinimizeAfterLocking",
				lvg, KPRes.MinimizeAfterLocking);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "MinimizeAfterOpeningDatabase",
				lvg, KPRes.MinimizeAfterOpeningDatabase);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "DisableSaveIfNotModified",
				lvg, KPRes.DisableSaveIfNotModified);

			lvg = new ListViewGroup(KPRes.EntryList);
			m_lvGuiOptions.Groups.Add(lvg);
			// m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "ShowGridLines",
			//	m_lvGuiOptions, lvg, KPRes.ShowGridLines);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "EntryListAutoResizeColumns",
				lvg, KPRes.EntryListAutoResizeColumns);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "EntryListAlternatingBgColors",
				lvg, KPRes.AlternatingBgColors);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "CopyUrlsInsteadOfOpening",
				lvg, KPRes.CopyUrlsInsteadOfOpening);

			if(!Program.Config.MainWindow.EntryListShowDerefData)
			{
				Debug.Assert(!Program.Config.MainWindow.EntryListShowDerefDataAsync);
				Program.Config.MainWindow.EntryListShowDerefDataAsync = false;
			}
			ListViewItem lviDeref = m_cdxGuiOptions.CreateItem(
				Program.Config.MainWindow, "EntryListShowDerefData",
				lvg, KPRes.ShowDerefData + " (" + KPRes.Slow + ")");
			ListViewItem lviDerefAsync = m_cdxGuiOptions.CreateItem(
				Program.Config.MainWindow, "EntryListShowDerefDataAsync",
				lvg, KPRes.ShowDerefDataAsync + " (" + KPRes.IncompatibleWithSorting + ")");
			m_cdxGuiOptions.AddLink(lviDeref, lviDerefAsync, CheckItemLinkType.UncheckedUnchecked);
			m_cdxGuiOptions.AddLink(lviDerefAsync, lviDeref, CheckItemLinkType.CheckedChecked);

			// lvg = new ListViewGroup(KPRes.EntryView);
			// m_lvGuiOptions.Groups.Add(lvg);
			// m_cdxGuiOptions.CreateItem(Program.Config.MainWindow.EntryView, "HideProtectedCustomStrings",
			//	lvg, KPRes.EntryViewHideProtectedCustomStrings);

			lvg = new ListViewGroup(KPRes.QuickSearchTb);
			m_lvGuiOptions.Groups.Add(lvg);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "QuickFindSearchInPasswords",
				lvg, KPRes.QuickSearchInPwFields);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "QuickFindExcludeExpired",
				lvg, KPRes.QuickSearchExclExpired);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "QuickFindDerefData",
				lvg, KPRes.QuickSearchDerefData + " (" + KPRes.Slow + ")");
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "FocusResultsAfterQuickFind",
				lvg, KPRes.FocusResultsAfterQuickSearch);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "FocusQuickFindOnRestore",
				lvg, KPRes.FocusQuickFindOnRestore);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "FocusQuickFindOnUntray",
				lvg, KPRes.FocusQuickFindOnUntray);

			lvg = new ListViewGroup(KPRes.Advanced);
			m_lvGuiOptions.Groups.Add(lvg);
			m_cdxGuiOptions.CreateItem(Program.Config.UI, "RepeatPasswordOnlyWhenHidden",
				lvg, KPRes.RepeatOnlyWhenHidden);
			m_cdxGuiOptions.CreateItem(Program.Config.UI, "UseCustomToolStripRenderer",
				lvg, KPRes.UseCustomToolStripRenderer);
			m_cdxGuiOptions.CreateItem(Program.Config.UI, "ForceSystemFontUnix",
				lvg, KPRes.ForceSystemFontUnix);
			m_cdxGuiOptions.CreateItem(Program.Config.UI, "ShowDbMntncResultsDialog",
				lvg, KPRes.DbMntncResults);

			m_cdxGuiOptions.UpdateData(false);
			m_lvGuiOptions.Columns[0].Width = m_lvGuiOptions.ClientRectangle.Width -
				UIUtil.GetVScrollBarWidth() - 1;

			try { m_numMruCount.Value = Program.Config.Application.MostRecentlyUsed.MaxItemCount; }
			catch(Exception) { Debug.Assert(false); m_numMruCount.Value = AceMru.DefaultMaxItemCount; }
			if(AppConfigEx.IsOptionEnforced(Program.Config.Application.MostRecentlyUsed, "MaxItemCount"))
			{
				m_lblMruCount.Enabled = false;
				m_numMruCount.Enabled = false;
			}

			if(AppConfigEx.IsOptionEnforced(Program.Config.UI, "StandardFont"))
				m_btnSelFont.Enabled = false;
			if(AppConfigEx.IsOptionEnforced(Program.Config.UI, "PasswordFont"))
				m_btnSelPwFont.Enabled = false;
		}

		private void LoadIntegrationOptions()
		{
			Keys kAT = (Keys)Program.Config.Integration.HotKeyGlobalAutoType;
			m_hkGlobalAutoType.HotKey = (kAT & Keys.KeyCode);
			m_hkGlobalAutoType.HotKeyModifiers = (kAT & Keys.Modifiers);
			m_hkGlobalAutoType.RenderHotKey();
			m_kPrevATHKKey = (m_hkGlobalAutoType.HotKey | m_hkGlobalAutoType.HotKeyModifiers);
			if(AppConfigEx.IsOptionEnforced(Program.Config.Integration, "HotKeyGlobalAutoType"))
				m_hkGlobalAutoType.Enabled = false;

			Keys kATS = (Keys)Program.Config.Integration.HotKeySelectedAutoType;
			m_hkSelectedAutoType.HotKey = (kATS & Keys.KeyCode);
			m_hkSelectedAutoType.HotKeyModifiers = (kATS & Keys.Modifiers);
			m_hkSelectedAutoType.RenderHotKey();
			m_kPrevATSHKKey = (m_hkSelectedAutoType.HotKey | m_hkSelectedAutoType.HotKeyModifiers);
			if(AppConfigEx.IsOptionEnforced(Program.Config.Integration, "HotKeySelectedAutoType"))
				m_hkSelectedAutoType.Enabled = false;

			Keys kSW = (Keys)Program.Config.Integration.HotKeyShowWindow;
			m_hkShowWindow.HotKey = (kSW & Keys.KeyCode);
			m_hkShowWindow.HotKeyModifiers = (kSW & Keys.Modifiers);
			m_hkShowWindow.RenderHotKey();
			m_kPrevSWHKKey = (m_hkShowWindow.HotKey | m_hkShowWindow.HotKeyModifiers);
			if(AppConfigEx.IsOptionEnforced(Program.Config.Integration, "HotKeyShowWindow"))
				m_hkShowWindow.Enabled = false;

			m_cbAutoRun.Checked = ShellUtil.GetStartWithWindows(AppDefs.AutoRunName);

			m_cbSingleClickTrayAction.Checked = Program.Config.UI.TrayIcon.SingleClickDefault;
			if(AppConfigEx.IsOptionEnforced(Program.Config.UI.TrayIcon, "SingleClickDefault"))
				m_cbSingleClickTrayAction.Enabled = false;

			string strOverride = Program.Config.Integration.UrlOverride;
			m_cbUrlOverride.Checked = (strOverride.Length > 0);
			m_tbUrlOverride.Text = strOverride;
			if(AppConfigEx.IsOptionEnforced(Program.Config.Integration, "UrlOverride"))
				m_cbUrlOverride.Enabled = false;

			if(AppConfigEx.IsOptionEnforced(Program.Config.Integration, "UrlSchemeOverrides"))
				m_btnSchemeOverrides.Enabled = false;
		}

		private void LoadAdvancedOptions()
		{
			m_lvAdvanced.Columns.Add(string.Empty, 200); // Resize below

			m_cdxAdvanced = new CheckedLVItemDXList(m_lvAdvanced, true);

			ListViewGroup lvg = new ListViewGroup(KPRes.StartAndExit);
			m_lvAdvanced.Groups.Add(lvg);
			m_cdxAdvanced.CreateItem(Program.Config.Application.Start, "OpenLastFile",
				lvg, KPRes.AutoRememberOpenLastFile);
			m_cdxAdvanced.CreateItem(Program.Config.Integration, "LimitToSingleInstance",
				lvg, KPRes.LimitSingleInstance);
			m_cdxAdvanced.CreateItem(Program.Config.Application.Start, "CheckForUpdate",
				lvg, KPRes.CheckForUpdAtStart);
			m_cdxAdvanced.CreateItem(Program.Config.Application.Start, "MinimizedAndLocked",
				lvg, KPRes.StartMinimizedAndLocked);
			m_cdxAdvanced.CreateItem(Program.Config.Application.FileClosing, "AutoSave",
				lvg, KPRes.AutoSaveAtExit);

			lvg = new ListViewGroup(KPRes.AfterDatabaseOpen);
			m_lvAdvanced.Groups.Add(lvg);
			m_cdxAdvanced.CreateItem(Program.Config.Application.FileOpening, "ShowExpiredEntries",
				lvg, KPRes.AutoShowExpiredEntries);
			m_cdxAdvanced.CreateItem(Program.Config.Application.FileOpening, "ShowSoonToExpireEntries",
				lvg, KPRes.AutoShowSoonToExpireEntries);

			lvg = new ListViewGroup(KPRes.AutoType);
			m_lvAdvanced.Groups.Add(lvg);
			m_cdxAdvanced.CreateItem(Program.Config.Integration, "AutoTypeMatchByTitle",
				lvg, KPRes.AutoTypeMatchByTitle);
			m_cdxAdvanced.CreateItem(Program.Config.Integration, "AutoTypeMatchByUrlInTitle",
				lvg, KPRes.AutoTypeMatchByUrlInTitle);
			m_cdxAdvanced.CreateItem(Program.Config.Integration, "AutoTypeMatchByUrlHostInTitle",
				lvg, KPRes.AutoTypeMatchByUrlHostInTitle);
			m_cdxAdvanced.CreateItem(Program.Config.Integration, "AutoTypeMatchByTagInTitle",
				lvg, KPRes.AutoTypeMatchByTagInTitle);
			m_cdxAdvanced.CreateItem(Program.Config.Integration, "AutoTypePrependInitSequenceForIE",
				lvg, KPRes.AutoTypePrependInitSeqForIE);
			m_cdxAdvanced.CreateItem(Program.Config.Integration, "AutoTypeReleaseAltWithKeyPress",
				lvg, KPRes.AutoTypeReleaseAltWithKeyPress);
			m_cdxAdvanced.CreateItem(Program.Config.Integration, "AutoTypeAdjustKeyboardLayout",
				lvg, KPRes.SameKeybLayout);
			m_cdxAdvanced.CreateItem(Program.Config.Integration, "AutoTypeCancelOnWindowChange",
				lvg, KPRes.AutoTypeCancelOnWindowChange);
			m_cdxAdvanced.CreateItem(Program.Config.Integration, "AutoTypeCancelOnTitleChange",
				lvg, KPRes.AutoTypeCancelOnTitleChange);

			lvg = new ListViewGroup(KPRes.Advanced);
			m_lvAdvanced.Groups.Add(lvg);

			if(!Program.Config.Integration.SearchKeyFiles)
				Program.Config.Integration.SearchKeyFilesOnRemovableMedia = false;
			ListViewItem lviSearch = m_cdxAdvanced.CreateItem(
				Program.Config.Integration, "SearchKeyFiles",
				lvg, KPRes.SearchKeyFiles);
			ListViewItem lviSearchRmv = m_cdxAdvanced.CreateItem(
				Program.Config.Integration, "SearchKeyFilesOnRemovableMedia",
				lvg, KPRes.SearchKeyFilesAlsoOnRemovable);
			m_cdxAdvanced.AddLink(lviSearch, lviSearchRmv, CheckItemLinkType.UncheckedUnchecked);
			m_cdxAdvanced.AddLink(lviSearchRmv, lviSearch, CheckItemLinkType.CheckedChecked);

			m_cdxAdvanced.CreateItem(Program.Config.Defaults, "RememberKeySources",
				lvg, KPRes.RememberKeySources);
			m_cdxAdvanced.CreateItem(Program.Config.Application, "RememberWorkingDirectories",
				lvg, KPRes.RememberWorkingDirectories);
			m_cdxAdvanced.CreateItem(Program.Config.UI.Hiding, "SeparateHidingSettings",
				lvg, KPRes.RememberHidingSettings);
			m_cdxAdvanced.CreateItem(Program.Config.UI.Hiding, "UnhideButtonAlsoUnhidesSource",
				lvg, KPRes.UnhideSourceCharactersToo);
			m_cdxAdvanced.CreateItem(Program.Config.Application, "VerifyWrittenFileAfterSaving",
				lvg, KPRes.VerifyWrittenFileAfterSave);
			m_cdxAdvanced.CreateItem(Program.Config.Application, "UseTransactedFileWrites",
				lvg, KPRes.UseTransactedDatabaseWrites);
			m_cdxAdvanced.CreateItem(Program.Config.Application, "UseFileLocks",
				lvg, KPRes.UseFileLocks + " " + KPRes.NotRecommended);
			m_cdxAdvanced.CreateItem(Program.Config.Defaults, "TanExpiresOnUse",
				lvg, KPRes.TanExpiresOnUse);
			m_cdxAdvanced.CreateItem(Program.Config.Defaults, "RecycleBinCollapse",
				lvg, KPRes.RecycleBinCollapse);
			m_cdxAdvanced.CreateItem(Program.Config.UI, "SecureDesktopPlaySound",
				lvg, KPRes.SecDeskPlaySound);
			m_cdxAdvanced.CreateItem(Program.Config.UI, "OptimizeForScreenReader",
				lvg, KPRes.OptimizeForScreenReader);

			m_cdxAdvanced.UpdateData(false);
			m_lvAdvanced.Columns[0].Width = m_lvAdvanced.ClientRectangle.Width -
				UIUtil.GetVScrollBarWidth() - 1;

			if(AppConfigEx.IsOptionEnforced(Program.Config.Integration, "ProxyType") ||
				AppConfigEx.IsOptionEnforced(Program.Config.Integration, "ProxyAddress"))
				m_btnProxy.Enabled = false;
		}

		private bool ValidateOptions()
		{
			m_hkGlobalAutoType.ResetIfModifierOnly();
			m_hkSelectedAutoType.ResetIfModifierOnly();
			m_hkShowWindow.ResetIfModifierOnly();

			bool bAltMod = false;
			bAltMod |= ((m_hkGlobalAutoType.HotKeyModifiers == Keys.Alt) ||
				(m_hkGlobalAutoType.HotKeyModifiers == (Keys.Alt | Keys.Shift)));
			bAltMod |= ((m_hkSelectedAutoType.HotKeyModifiers == Keys.Alt) ||
				(m_hkSelectedAutoType.HotKeyModifiers == (Keys.Alt | Keys.Shift)));
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

			if(!m_cbLockAfterGlobalTime.Checked)
				Program.Config.Security.WorkspaceLocking.LockAfterGlobalTime = 0;
			else
				Program.Config.Security.WorkspaceLocking.LockAfterGlobalTime =
					(uint)m_numLockAfterGlobalTime.Value;

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

			ChangeHotKey(ref m_kPrevATHKKey, m_hkGlobalAutoType,
				AppDefs.GlobalHotKeyId.AutoType);
			ChangeHotKey(ref m_kPrevATSHKKey, m_hkSelectedAutoType,
				AppDefs.GlobalHotKeyId.AutoTypeSelected);
			ChangeHotKey(ref m_kPrevSWHKKey, m_hkShowWindow,
				AppDefs.GlobalHotKeyId.ShowWindow);

			Program.Config.UI.TrayIcon.SingleClickDefault = m_cbSingleClickTrayAction.Checked;

			if(m_cbUrlOverride.Checked)
				Program.Config.Integration.UrlOverride = m_tbUrlOverride.Text;
			else Program.Config.Integration.UrlOverride = string.Empty;

			m_cdxAdvanced.UpdateData(true);

			Program.Config.Integration.UrlSchemeOverrides = m_aceUrlSchemeOverrides;
		}

		private void CleanUpEx()
		{
			int nTab = m_tabMain.SelectedIndex;
			if((nTab >= 0) && (nTab < m_tabMain.TabPages.Count))
				Program.Config.Defaults.OptionsTabIndex = (uint)nTab;

			m_cdxSecurityOptions.Release();
			m_cdxPolicy.Release();
			m_cdxGuiOptions.Release();
			m_cdxAdvanced.Release();

			AppConfigEx.ClearXmlPathCache();
		}

		private static void ChangeHotKey(ref Keys kPrevHK, HotKeyControlEx hkControl,
			int nHotKeyID)
		{
			Keys kNew = (hkControl.HotKey | hkControl.HotKeyModifiers);
			if(kPrevHK != kNew)
			{
				kPrevHK = kNew;

				if(nHotKeyID == AppDefs.GlobalHotKeyId.AutoType)
					Program.Config.Integration.HotKeyGlobalAutoType = (ulong)kNew;
				else if(nHotKeyID == AppDefs.GlobalHotKeyId.AutoTypeSelected)
					Program.Config.Integration.HotKeySelectedAutoType = (ulong)kNew;
				else if(nHotKeyID == AppDefs.GlobalHotKeyId.ShowWindow)
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

			m_numLockAfterTime.Enabled = (m_cbLockAfterTime.Checked &&
				m_cbLockAfterTime.Enabled);

			if(WinUtil.IsWindows9x || NativeLib.IsUnix())
			{
				m_cbLockAfterGlobalTime.Checked = false;
				m_cbLockAfterGlobalTime.Enabled = false;
				m_numLockAfterGlobalTime.Enabled = false;
			}
			else
				m_numLockAfterGlobalTime.Enabled = (m_cbLockAfterGlobalTime.Checked &&
					m_cbLockAfterGlobalTime.Enabled);

			m_numDefaultExpireDays.Enabled = (m_cbDefaultExpireDays.Checked &&
				m_cbDefaultExpireDays.Enabled);
			m_numClipClearTime.Enabled = (m_cbClipClearTime.Checked &&
				m_cbClipClearTime.Enabled);

			m_tbUrlOverride.Enabled = (m_cbUrlOverride.Checked &&
				m_cbUrlOverride.Enabled);

			m_bBlockUIUpdate = false;
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			if(!ValidateOptions()) { this.DialogResult = DialogResult.None; return; }

			SaveOptions();
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
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

		private void OnBtnSelListFont(object sender, EventArgs e)
		{
			FontDialog dlg = UIUtil.CreateFontDialog(false);

			AceFont fOld = Program.Config.UI.StandardFont;
			if(fOld.OverrideUIDefault) dlg.Font = fOld.ToFont();
			else
			{
				try { dlg.Font = m_tbUrlOverride.Font; }
				catch(Exception) { Debug.Assert(false); }
			}

			if(dlg.ShowDialog() == DialogResult.OK)
			{
				Program.Config.UI.StandardFont = new AceFont(dlg.Font);
				Program.Config.UI.StandardFont.OverrideUIDefault = true;
			}
			dlg.Dispose();
		}

		private void OnBtnSelPwFont(object sender, EventArgs e)
		{
			FontDialog dlg = UIUtil.CreateFontDialog(false);

			AceFont fOld = Program.Config.UI.PasswordFont;
			if(fOld.OverrideUIDefault) dlg.Font = fOld.ToFont();
			else if(FontUtil.MonoFont != null) dlg.Font = FontUtil.MonoFont;
			else
			{
				try
				{
					dlg.Font = new Font(FontFamily.GenericMonospace,
						m_tbUrlOverride.Font.SizeInPoints);
				}
				catch(Exception) { Debug.Assert(false); }
			}

			if(dlg.ShowDialog() == DialogResult.OK)
			{
				Program.Config.UI.PasswordFont = new AceFont(dlg.Font);
				Program.Config.UI.PasswordFont.OverrideUIDefault = true;
			}
			dlg.Dispose();
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

		private void OnBtnUrlSchemeOverrides(object sender, EventArgs e)
		{
			UrlSchemesForm dlg = new UrlSchemesForm();
			dlg.InitEx(m_aceUrlSchemeOverrides);
			UIUtil.ShowDialogAndDestroy(dlg);
		}

		private void OnFormClosing(object sender, FormClosingEventArgs e)
		{
			CleanUpEx();
		}

		private void OnHotKeyHelpLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			AppHelp.ShowHelp(AppDefs.HelpTopics.Setup, AppDefs.HelpTopics.SetupMono);
		}

		private void OnLockAfterGlobalTimeCheckedChanged(object sender, EventArgs e)
		{
			UpdateUIState();
		}

		private void OnBtnProxy(object sender, EventArgs e)
		{
			ProxyForm dlg = new ProxyForm();
			UIUtil.ShowDialogAndDestroy(dlg);
		}
	}
}
