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
using System.Text;
using System.Windows.Forms;

using KeePass.App;
using KeePass.App.Configuration;
using KeePass.Resources;
using KeePass.UI;
using KeePass.UI.ToolStripRendering;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Delegates;
using KeePassLib.Serialization;
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
		private BannerStyle m_bsCurrent = BannerStyle.Light;
		private bool m_bBlockUIUpdate = false;
		private bool m_bLoadingSettings = false;

		private CheckedLVItemDXList m_cdxSecurityOptions = null;
		private CheckedLVItemDXList m_cdxPolicy = null;
		private CheckedLVItemDXList m_cdxGuiOptions = null;
		private CheckedLVItemDXList m_cdxAdvanced = null;

		private readonly Dictionary<int, string> m_dTsrUuids = new Dictionary<int, string>();

		private FontControlGroup m_fcgList = null;
		private FontControlGroup m_fcgPassword = null;

		private Keys m_kPrevAT = Keys.None;
		private Keys m_kPrevATP = Keys.None;
		private Keys m_kPrevATS = Keys.None;
		private Keys m_kPrevSW = Keys.None;

		private string m_strInitialTsRenderer = string.Empty;
		public bool RequiresUIReinitialize
		{
			get { return (Program.Config.UI.ToolStripRenderer != m_strInitialTsRenderer); }
		}

		public OptionsForm()
		{
			InitializeComponent();
			GlobalWindowManager.InitializeForm(this);
		}

		public void InitEx(ImageList ilIcons)
		{
			InitEx(ilIcons, false);
		}

		public void InitEx(ImageList ilIcons, bool bForceInTaskbar)
		{
			Debug.Assert(ilIcons != null);
			m_ilIcons = ilIcons;

			// Set ShowInTaskbar immediately, not later, otherwise the form
			// can disappear:
			// https://sourceforge.net/p/keepass/discussion/329220/thread/c95b5644/
			if(bForceInTaskbar) this.ShowInTaskbar = true;
		}

		private void CreateDialogBanner(BannerStyle bs)
		{
			if(bs == m_bsCurrent) return;
			m_bsCurrent = bs;

			BannerStyle bsPrev = Program.Config.UI.BannerStyle;
			if(bs != BannerStyle.Default) Program.Config.UI.BannerStyle = bs;

			BannerFactory.CreateBannerEx(this, m_bannerImage,
				Properties.Resources.B48x48_KCMSystem, KPRes.Options,
				KPRes.OptionsDesc);

			if(bs != BannerStyle.Default) Program.Config.UI.BannerStyle = bsPrev;
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			// Can be invoked by tray command; don't use CenterParent
			Debug.Assert(this.StartPosition == FormStartPosition.CenterScreen);

			// When multiline is enabled, tabs added by plugins can result
			// in multiple tab rows, cropping the tab content at the bottom;
			// https://sourceforge.net/p/keepass/discussion/329220/thread/a17a2734/
			Debug.Assert(!m_tabMain.Multiline);

			GlobalWindowManager.AddWindow(this);

			this.Icon = AppIcons.Default;

			Debug.Assert(m_ilIcons != null);
			if(m_ilIcons != null)
			{
				m_tabMain.ImageList = m_ilIcons;

				m_tabSecurity.ImageIndex = (int)PwIcon.TerminalEncrypted;
				m_tabPolicy.ImageIndex = (int)PwIcon.List;
				m_tabGui1.ImageIndex = (int)PwIcon.Screen;
				m_tabGui2.ImageIndex = (int)PwIcon.Screen;
				m_tabIntegration.ImageIndex = (int)PwIcon.Console;
				m_tabAdvanced.ImageIndex = (int)PwIcon.ClipboardReady;
			}

			uint uTab = Program.Config.Defaults.OptionsTabIndex;
			if(uTab < (uint)m_tabMain.TabPages.Count)
				m_tabMain.SelectedTab = m_tabMain.TabPages[(int)uTab];

			m_tabPolicy.Text = KPRes.Policy;
			UIUtil.SetText(m_cbLockAfterTime, KPRes.LockAfterTime + ":");
			UIUtil.SetText(m_cbLockAfterGlobalTime, KPRes.LockAfterGlobalTime + ":");
			UIUtil.SetText(m_cbClipClearTime, KPRes.ClipboardClearTime + ":");
			UIUtil.SetText(m_cbDefaultExpireDays, KPRes.ExpiryDefaultDays + ":");

			Debug.Assert(!m_cmbMenuStyle.Sorted);
			m_cmbMenuStyle.Items.Add(KPRes.Automatic + " (" + KPRes.Recommended + ")");
			m_cmbMenuStyle.Items.Add(new string('-', 24));
			int nTsrs = 2, iTsrSel = 0;
			foreach(TsrFactory fTsr in TsrPool.Factories)
			{
				string strName = (fTsr.Name ?? string.Empty);
				if(!fTsr.IsSupported())
					strName += " (" + KPRes.IncompatibleEnv + ")";

				string strUuid = Convert.ToBase64String(fTsr.Uuid.UuidBytes);
				if(Program.Config.UI.ToolStripRenderer == strUuid)
					iTsrSel = nTsrs;

				m_cmbMenuStyle.Items.Add(strName);
				m_dTsrUuids[nTsrs++] = strUuid;
			}
			Debug.Assert(m_cmbMenuStyle.Items.Count == nTsrs);
			UIUtil.AdjustDropDownWidth(m_cmbMenuStyle);
			m_cmbMenuStyle.SelectedIndex = iTsrSel;
			if(AppConfigEx.IsOptionEnforced(Program.Config.UI, "ToolStripRenderer"))
				UIUtil.SetEnabledFast(false, m_lblMenuStyle, m_cmbMenuStyle);

			GAction<BannerStyle, string> fAddBannerStyle = delegate(
				BannerStyle bs, string strDisplay)
			{
				Debug.Assert(m_cmbBannerStyle.Items.Count == (long)bs);
				m_cmbBannerStyle.Items.Add(strDisplay);
			};

			Debug.Assert(!m_cmbBannerStyle.Sorted);
			fAddBannerStyle(BannerStyle.Default, KPRes.CurrentStyle);
			fAddBannerStyle(BannerStyle.Blue, KPRes.Blue);
			fAddBannerStyle(BannerStyle.Dark, KPRes.Dark);
			fAddBannerStyle(BannerStyle.Light, KPRes.Light);
			fAddBannerStyle(BannerStyle.BlueCarbon, "Blue Carbon");

			CreateDialogBanner(BannerStyle.Default); // Default forces generation
			m_cmbBannerStyle.SelectedIndex = (int)BannerStyle.Default;
			if((BannerFactory.CustomGenerator != null) ||
				AppConfigEx.IsOptionEnforced(Program.Config.UI, "BannerStyle"))
				UIUtil.SetEnabledFast(false, m_lblBannerStyle, m_cmbBannerStyle);

			FontUtil.SetDefaultFont(m_lblFileExtHint);
			AceFont afDefault = new AceFont(FontUtil.DefaultFont, false);
			AceFont afMono = new AceFont(FontUtil.GetDefaultMonoFont(m_lblFileExtHint), false);

			m_fcgList = new FontControlGroup(m_cbListFont, m_btnListFont,
				Program.Config.UI.StandardFont, afDefault);
			m_fcgPassword = new FontControlGroup(m_cbPasswordFont, m_btnPasswordFont,
				Program.Config.UI.PasswordFont, afMono);

			AceEscAction aEscCur = Program.Config.MainWindow.EscAction;
			int iEscSel = (int)AceEscAction.Lock;
			GAction<AceEscAction, string> fAddEscAction = delegate(
				AceEscAction aEsc, string strDisplay)
			{
				if(aEsc == aEscCur) iEscSel = m_cmbEscAction.Items.Count;
				Debug.Assert(m_cmbEscAction.Items.Count == (long)aEsc);
				m_cmbEscAction.Items.Add(strDisplay);
			};

			Debug.Assert(!m_cmbEscAction.Sorted);
			fAddEscAction(AceEscAction.None, KPRes.Ignore);
			fAddEscAction(AceEscAction.Lock, KPRes.LockWorkspace);
			fAddEscAction(AceEscAction.Minimize, KPRes.Minimize);
			fAddEscAction(AceEscAction.MinimizeToTray, KPRes.MinimizeToTrayStc);
			fAddEscAction(AceEscAction.Exit, KPRes.Exit);

			m_cmbEscAction.SelectedIndex = iEscSel;
			if(AppConfigEx.IsOptionEnforced(Program.Config.MainWindow, "EscAction"))
				UIUtil.SetEnabledFast(false, m_lblEscAction, m_cmbEscAction);

			int nWidth = m_lvPolicy.ClientSize.Width - UIUtil.GetVScrollBarWidth();
			m_lvPolicy.Columns.Add(KPRes.Feature, (nWidth * 10) / 29);
			m_lvPolicy.Columns.Add(KPRes.Description, (nWidth * 19) / 29);

			UIUtil.ConfigureToolTip(m_ttRect);
			UIUtil.SetToolTip(m_ttRect, m_cbClipClearTime, KPRes.ClipboardClearDesc +
				MessageService.NewParagraph + KPRes.ClipboardOptionME, false);

			AccessibilityEx.SetContext(m_numLockAfterTime, m_cbLockAfterTime);
			AccessibilityEx.SetContext(m_numLockAfterGlobalTime, m_cbLockAfterGlobalTime);
			AccessibilityEx.SetContext(m_numClipClearTime, m_cbClipClearTime);
			AccessibilityEx.SetContext(m_numDefaultExpireDays, m_cbDefaultExpireDays);

			AccessibilityEx.SetContext(m_linkSecOptEx, m_lblSecOpt);
			AccessibilityEx.SetContext(m_linkSecOptAdm, m_lblSecOpt);

			if(!NativeLib.IsUnix())
			{
				UIUtil.SetShield(m_btnFileExtCreate, true);
				UIUtil.SetShield(m_btnFileExtRemove, true);

				m_linkHotKeyHelp.Visible = false;
			}
			else // Unix
			{
				m_hkAutoType.TextNone = KPRes.External;
				m_hkAutoTypePassword.TextNone = KPRes.External;
				m_hkAutoTypeSelected.TextNone = KPRes.External;
				m_hkShowWindow.TextNone = KPRes.External;

				m_hkAutoType.Enabled = m_hkAutoTypePassword.Enabled =
					m_hkAutoTypeSelected.Enabled = m_hkShowWindow.Enabled = false;
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
			AppConfigEx cfg = Program.Config;
			AceDefaults aceDef = cfg.Defaults;
			AceSecurity aceSec = cfg.Security;
			AceWorkspaceLocking aceWL = aceSec.WorkspaceLocking;

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

			int nDefaultExpireDays = aceDef.NewEntryExpiresInDays;
			if(nDefaultExpireDays < 0)
				m_cbDefaultExpireDays.Checked = false;
			else
			{
				m_cbDefaultExpireDays.Checked = true;
				try { m_numDefaultExpireDays.Value = nDefaultExpireDays; }
				catch(Exception) { Debug.Assert(false); }
			}
			if(AppConfigEx.IsOptionEnforced(aceDef, "NewEntryExpiresInDays"))
				m_cbDefaultExpireDays.Enabled = false;

			int nClipClear = aceSec.ClipboardClearAfterSeconds;
			if(nClipClear >= 0)
			{
				m_cbClipClearTime.Checked = true;
				m_numClipClearTime.Value = nClipClear;
			}
			else m_cbClipClearTime.Checked = false;
			if(AppConfigEx.IsOptionEnforced(aceSec, "ClipboardClearAfterSeconds"))
				m_cbClipClearTime.Enabled = false;

			m_lvSecurityOptions.Columns.Add(string.Empty); // Resize below

			ListViewGroup lvg = new ListViewGroup(KPRes.General);
			m_lvSecurityOptions.Groups.Add(lvg);
			Debug.Assert(lvg.ListView == m_lvSecurityOptions);

			m_cdxSecurityOptions = new CheckedLVItemDXList(m_lvSecurityOptions, true);

			bool? obNoSEv = null; // Allow read-only by enforced config
			string strSEvSuffix = string.Empty;
			if(MonoWorkarounds.IsRequired(1378))
			{
				obNoSEv = true;
				strSEvSuffix = " (" + KPRes.UnsupportedByMono + ")";
			}

			bool? obNoWin = null; // Allow read-only by enforced config
			if(NativeLib.IsUnix()) obNoWin = true;

			m_cdxSecurityOptions.CreateItem(aceWL, "LockOnWindowMinimize",
				lvg, KPRes.LockOnMinimizeTaskbar);
			m_cdxSecurityOptions.CreateItem(aceWL, "LockOnWindowMinimizeToTray",
				lvg, KPRes.LockOnMinimizeTray);
			m_cdxSecurityOptions.CreateItem(aceWL, "LockOnSessionSwitch",
				lvg, KPRes.LockOnSessionSwitch + strSEvSuffix, obNoSEv);
			m_cdxSecurityOptions.CreateItem(aceWL, "LockOnSuspend",
				lvg, KPRes.LockOnSuspend + strSEvSuffix, obNoSEv);
			m_cdxSecurityOptions.CreateItem(aceWL, "LockOnRemoteControlChange",
				lvg, KPRes.LockOnRemoteControlChange + strSEvSuffix, obNoSEv);
			m_cdxSecurityOptions.CreateItem(aceWL, "ExitInsteadOfLockingAfterTime",
				lvg, KPRes.ExitInsteadOfLockingAfterTime);
			m_cdxSecurityOptions.CreateItem(aceWL, "AlwaysExitInsteadOfLocking",
				lvg, KPRes.ExitInsteadOfLockingAlways);

			lvg = new ListViewGroup(KPRes.ClipboardMain);
			m_lvSecurityOptions.Groups.Add(lvg);

			Action<ListViewItem> fClipME = delegate(ListViewItem lvi)
			{
				if(lvi == null) { Debug.Assert(false); return; }
				string str = lvi.Text;
				if(string.IsNullOrEmpty(str)) { Debug.Assert(false); return; }
				lvi.ToolTipText = str + "." + MessageService.NewParagraph +
					KPRes.ClipboardOptionME;
			};

			fClipME(m_cdxSecurityOptions.CreateItem(aceSec, "ClipboardClearOnExit",
				lvg, KPRes.ClipboardClearOnExit));
			fClipME(m_cdxSecurityOptions.CreateItem(aceSec, "ClipboardNoPersist",
				lvg, KPRes.ClipboardNoPersist));
			fClipME(m_cdxSecurityOptions.CreateItem(aceSec, "UseClipboardViewerIgnoreFormat",
				lvg, KPRes.ClipboardViewerIgnoreFormat // + " " + KPRes.NotRecommended
				));

			lvg = new ListViewGroup(KPRes.Advanced);
			m_lvSecurityOptions.Groups.Add(lvg);

			if(NativeLib.IsLibraryInstalled())
				m_cdxSecurityOptions.CreateItem(cfg.Native, "NativeKeyTransformations",
					lvg, KPRes.NativeLibUse);

			m_cdxSecurityOptions.CreateItem(aceSec, "KeyTransformWeakWarning",
				lvg, KPRes.KeyTransformWeakWarning);
			m_cdxSecurityOptions.CreateItem(aceSec, "MasterKeyOnSecureDesktop",
				lvg, KPRes.MasterKeyOnSecureDesktop, obNoWin);
			m_cdxSecurityOptions.CreateItem(aceSec, "ClearKeyCommandLineParams",
				lvg, KPRes.ClearKeyCmdLineParams);
			m_cdxSecurityOptions.CreateItem(aceSec.MasterPassword, "RememberWhileOpen",
				lvg, KPRes.MasterPasswordRmbWhileOpen);

			m_cdxSecurityOptions.UpdateData(false);
			UIUtil.ResizeColumns(m_lvSecurityOptions, true);
		}

		private void LoadPolicyOption(string strPropertyName, AppPolicyId p)
		{
			Debug.Assert(p.ToString() == strPropertyName);

			ListViewItem lvi = m_cdxPolicy.CreateItem(Program.Config.Security.Policy,
				strPropertyName, null, AppPolicy.GetName(p) + " *");
			lvi.SubItems.Add(AppPolicy.GetDesc(p));
		}

		private void LoadPolicyOptions()
		{
			m_cdxPolicy = new CheckedLVItemDXList(m_lvPolicy, true);

			LoadPolicyOption("Plugins", AppPolicyId.Plugins);
			LoadPolicyOption("Export", AppPolicyId.Export);
			// LoadPolicyOption("ExportNoKey", AppPolicyId.ExportNoKey);
			LoadPolicyOption("Import", AppPolicyId.Import);
			LoadPolicyOption("Print", AppPolicyId.Print);
			LoadPolicyOption("PrintNoKey", AppPolicyId.PrintNoKey);
			LoadPolicyOption("NewFile", AppPolicyId.NewFile);
			LoadPolicyOption("SaveFile", AppPolicyId.SaveFile);
			LoadPolicyOption("AutoType", AppPolicyId.AutoType);
			LoadPolicyOption("AutoTypeWithoutContext", AppPolicyId.AutoTypeWithoutContext);
			LoadPolicyOption("CopyToClipboard", AppPolicyId.CopyToClipboard);
			LoadPolicyOption("CopyWholeEntries", AppPolicyId.CopyWholeEntries);
			LoadPolicyOption("DragDrop", AppPolicyId.DragDrop);
			LoadPolicyOption("UnhidePasswords", AppPolicyId.UnhidePasswords);
			LoadPolicyOption("ChangeMasterKey", AppPolicyId.ChangeMasterKey);
			LoadPolicyOption("ChangeMasterKeyNoKey", AppPolicyId.ChangeMasterKeyNoKey);
			LoadPolicyOption("EditTriggers", AppPolicyId.EditTriggers);

			m_cdxPolicy.UpdateData(false);
		}

		private void LoadGuiOptions()
		{
			m_strInitialTsRenderer = Program.Config.UI.ToolStripRenderer;

			bool? obNoMin = null;
			if(MonoWorkarounds.IsRequired(1418)) obNoMin = true;
			bool? obNoFocus = null;
			if(MonoWorkarounds.IsRequired(1976)) obNoFocus = true;

			m_lvGuiOptions.Columns.Add(KPRes.Options); // Resize below

			ListViewGroup lvg = new ListViewGroup(KPRes.MainWindow);
			m_lvGuiOptions.Groups.Add(lvg);
			Debug.Assert(lvg.ListView == m_lvGuiOptions);

			m_cdxGuiOptions = new CheckedLVItemDXList(m_lvGuiOptions, true);

			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "MinimizeToTray",
				lvg, KPRes.MinimizeToTray);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "DropToBackAfterClipboardCopy",
				lvg, KPRes.DropToBackOnCopy);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "MinimizeAfterClipboardCopy",
				lvg, KPRes.MinimizeAfterCopy);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "MinimizeAfterAutoType",
				lvg, KPRes.MinimizeAfterAutoType);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "MinimizeAfterLocking",
				lvg, KPRes.MinimizeAfterLocking);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "MinimizeAfterOpeningDatabase",
				lvg, KPRes.MinimizeAfterOpeningDatabase, obNoMin);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "CloseButtonMinimizesWindow",
				lvg, KPRes.CloseButtonMinimizes);
			// m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "EscMinimizesToTray",
			//	lvg, KPRes.EscMinimizesToTray);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "ShowFullPathInTitle",
				lvg, KPRes.ShowFullPathInTitleBar);
			// m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "ShowFullPathOnTab",
			//	lvg, KPRes.ShowFullPathOnFileTab);
			// m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "ShowDatabaseNameOnTab",
			//	lvg, KPRes.ShowDatabaseNameOnFileTab);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "DisableSaveIfNotModified",
				lvg, KPRes.DisableSaveIfNotModified);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "HideCloseDatabaseButton",
				lvg, KPRes.HideCloseDatabaseTb);
			m_cdxGuiOptions.CreateItem(Program.Config.UI, "ShowAdvAutoTypeCommands",
				lvg, KPRes.ShowAdvAutoTypeCommands);

			lvg = new ListViewGroup(KPRes.EntryList);
			m_lvGuiOptions.Groups.Add(lvg);
			// m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "ShowGridLines",
			//	m_lvGuiOptions, lvg, KPRes.ShowGridLines);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "EntryListAutoResizeColumns",
				lvg, KPRes.EntryListAutoResizeColumns);
			// m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "EntryListAlternatingBgColors",
			//	lvg, KPRes.AlternatingBgColors);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "CopyUrlsInsteadOfOpening",
				lvg, KPRes.CopyUrlsInsteadOfOpening);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "EntrySelGroupSel",
				lvg, KPRes.EntrySelGroupSel);

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

			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "EntryListShowDerefDataAndRefs",
				lvg, KPRes.ShowDerefDataAndRefs);

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
				lvg, KPRes.FocusQuickFindOnRestore, obNoFocus);
			m_cdxGuiOptions.CreateItem(Program.Config.MainWindow, "FocusQuickFindOnUntray",
				lvg, KPRes.FocusQuickFindOnUntray, obNoFocus);

			lvg = new ListViewGroup(KPRes.TrayIcon);
			m_lvGuiOptions.Groups.Add(lvg);
			// m_cdxGuiOptions.CreateItem(Program.Config.UI.TrayIcon, "ShowOnlyIfTrayedEx",
			//	lvg, KPRes.ShowTrayOnlyIfTrayed);
			m_cdxGuiOptions.CreateItem(Program.Config.UI.TrayIcon, "GrayIcon",
				lvg, KPRes.TrayIconGray);
			m_cdxGuiOptions.CreateItem(Program.Config.UI.TrayIcon, "SingleClickDefault",
				lvg, KPRes.TrayIconSingleClick);

			lvg = new ListViewGroup(KPRes.Dialogs);
			m_lvGuiOptions.Groups.Add(lvg);
			m_cdxGuiOptions.CreateItem(Program.Config.UI, "ShowRecycleConfirmDialog",
				lvg, KPRes.RecycleShowConfirm);
			m_cdxGuiOptions.CreateItem(Program.Config.UI, "ShowDbMntncResultsDialog",
				lvg, KPRes.DbMntncResults);
			m_cdxGuiOptions.CreateItem(Program.Config.UI, "ShowEmSheetDialog",
				lvg, KPRes.EmergencySheetAsk);
			m_cdxGuiOptions.CreateItem(Program.Config.UI, "ShowDbOpenUnkVerDialog",
				lvg, KPRes.DatabaseOpenUnknownVersionAsk);

			lvg = new ListViewGroup(KPRes.Advanced);
			m_lvGuiOptions.Groups.Add(lvg);
			m_cdxGuiOptions.CreateItem(Program.Config.UI, "RepeatPasswordOnlyWhenHidden",
				lvg, KPRes.RepeatOnlyWhenHidden);
			// m_cdxGuiOptions.CreateItem(Program.Config.UI, "UseCustomToolStripRenderer",
			//	lvg, KPRes.UseCustomToolStripRenderer);
			m_cdxGuiOptions.CreateItem(Program.Config.UI, "TreeViewShowLines",
				lvg, KPRes.TreeViewShowLines);
			m_cdxGuiOptions.CreateItem(Program.Config.UI, "ForceSystemFontUnix",
				lvg, KPRes.ForceSystemFontUnix);

			m_cdxGuiOptions.UpdateData(false);
			UIUtil.ResizeColumns(m_lvGuiOptions, true);

			try { m_numMruCount.Value = Program.Config.Application.MostRecentlyUsed.MaxItemCount; }
			catch(Exception) { Debug.Assert(false); m_numMruCount.Value = AceMru.DefaultMaxItemCount; }
			if(AppConfigEx.IsOptionEnforced(Program.Config.Application.MostRecentlyUsed, "MaxItemCount"))
				UIUtil.SetEnabledFast(false, m_lblMruCount, m_numMruCount);

			Debug.Assert(!m_cmbAltColor.Sorted);
			m_cmbAltColor.Items.Add(KPRes.Off);
			m_cmbAltColor.Items.Add(KPRes.On + ", " + KPRes.DefaultColor);
			m_cmbAltColor.Items.Add(KPRes.On + ", " + KPRes.CustomColor + ":");

			UIUtil.AdjustDropDownWidth(m_cmbAltColor);

			int c = Program.Config.MainWindow.EntryListAlternatingBgColor;
			if(Program.Config.MainWindow.EntryListAlternatingBgColors)
				m_cmbAltColor.SelectedIndex = ((c != 0) ? 2 : 1);
			else m_cmbAltColor.SelectedIndex = 0;
			m_btnAltColor.SelectedColor = ((c != 0) ? Color.FromArgb(c) :
				UIUtil.GetAlternateColor(m_lvGuiOptions.BackColor));
			if(AppConfigEx.IsOptionEnforced(Program.Config.MainWindow, "EntryListAlternatingBgColors") ||
				AppConfigEx.IsOptionEnforced(Program.Config.MainWindow, "EntryListAlternatingBgColor"))
				UIUtil.SetEnabledFast(false, m_lblAltColor, m_cmbAltColor, m_btnAltColor);

			if(AppConfigEx.IsOptionEnforced(Program.Config.UI, "StandardFont"))
				m_fcgList.Enabled = false;
			if(AppConfigEx.IsOptionEnforced(Program.Config.UI, "PasswordFont") ||
				MonoWorkarounds.IsRequired(5795))
				m_fcgPassword.Enabled = false;
		}

		private void LoadIntegrationOptions()
		{
			Keys kAT = (Keys)Program.Config.Integration.HotKeyGlobalAutoType;
			m_hkAutoType.HotKey = kAT;
			m_kPrevAT = m_hkAutoType.HotKey; // Adjusted one
			if(AppConfigEx.IsOptionEnforced(Program.Config.Integration, "HotKeyGlobalAutoType"))
				UIUtil.SetEnabledFast(false, m_lblAutoType, m_hkAutoType);

			Keys kATP = (Keys)Program.Config.Integration.HotKeyGlobalAutoTypePassword;
			m_hkAutoTypePassword.HotKey = kATP;
			m_kPrevATP = m_hkAutoTypePassword.HotKey; // Adjusted one
			if(AppConfigEx.IsOptionEnforced(Program.Config.Integration, "HotKeyGlobalAutoTypePassword"))
				UIUtil.SetEnabledFast(false, m_lblAutoTypePassword, m_hkAutoTypePassword);

			Keys kATS = (Keys)Program.Config.Integration.HotKeySelectedAutoType;
			m_hkAutoTypeSelected.HotKey = kATS;
			m_kPrevATS = m_hkAutoTypeSelected.HotKey; // Adjusted one
			if(AppConfigEx.IsOptionEnforced(Program.Config.Integration, "HotKeySelectedAutoType"))
				UIUtil.SetEnabledFast(false, m_lblAutoTypeSelected, m_hkAutoTypeSelected);

			Keys kSW = (Keys)Program.Config.Integration.HotKeyShowWindow;
			m_hkShowWindow.HotKey = kSW;
			m_kPrevSW = m_hkShowWindow.HotKey; // Adjusted one
			if(AppConfigEx.IsOptionEnforced(Program.Config.Integration, "HotKeyShowWindow"))
				UIUtil.SetEnabledFast(false, m_lblShowWindow, m_hkShowWindow);

			m_cbAutoRun.Checked = ShellUtil.GetStartWithWindows(AppDefs.AutoRunName);

			// m_cbSingleClickTrayAction.Checked = Program.Config.UI.TrayIcon.SingleClickDefault;
			// if(AppConfigEx.IsOptionEnforced(Program.Config.UI.TrayIcon, "SingleClickDefault"))
			//	m_cbSingleClickTrayAction.Enabled = false;
		}

		private void LoadAdvancedOptions()
		{
			bool? obNoMin = null;
			if(MonoWorkarounds.IsRequired(1418)) obNoMin = true;

			m_lvAdvanced.Columns.Add(string.Empty); // Resize below

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
				lvg, KPRes.StartMinimizedAndLocked, obNoMin);
			m_cdxAdvanced.CreateItem(Program.Config.Application.FileClosing, "AutoSave",
				lvg, KPRes.AutoSaveAtExit);
			m_cdxAdvanced.CreateItem(Program.Config.Application, "AutoSaveAfterEntryEdit",
				lvg, KPRes.AutoSaveAfterEntryEdit);

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
			m_cdxAdvanced.CreateItem(Program.Config.Integration, "AutoTypeMatchNormDashes",
				lvg, KPRes.ConsiderDashesEq + " (-, \u2010, \u2011, \u2012, \u2013, \u2014, \u2015, \u2212)");
			m_cdxAdvanced.CreateItem(Program.Config.Integration, "AutoTypeExpiredCanMatch",
				lvg, KPRes.ExpiredEntriesCanMatch);
			m_cdxAdvanced.CreateItem(Program.Config.Integration, "AutoTypeAlwaysShowSelDialog",
				lvg, KPRes.AutoTypeAlwaysShowSelDialog);

			lvg = new ListViewGroup(KPRes.AutoType + " - " + KPRes.SendingNoun);
			m_lvAdvanced.Groups.Add(lvg);
			m_cdxAdvanced.CreateItem(Program.Config.Integration, "AutoTypePrependInitSequenceForIE",
				lvg, KPRes.AutoTypePrependInitSeqForIE);
			m_cdxAdvanced.CreateItem(Program.Config.Integration, "AutoTypeReleaseAltWithKeyPress",
				lvg, KPRes.AutoTypeReleaseAltWithKeyPress);
			m_cdxAdvanced.CreateItem(Program.Config.Integration, "AutoTypeAdjustKeyboardLayout",
				lvg, KPRes.SameKeybLayout);
			m_cdxAdvanced.CreateItem(Program.Config.Integration, "AutoTypeAllowInterleaved",
				lvg, KPRes.InterleavedKeySending);
			m_cdxAdvanced.CreateItem(Program.Config.Integration, "AutoTypeCancelOnWindowChange",
				lvg, KPRes.AutoTypeCancelOnWindowChange);
			m_cdxAdvanced.CreateItem(Program.Config.Integration, "AutoTypeCancelOnTitleChange",
				lvg, KPRes.AutoTypeCancelOnTitleChange);

			lvg = new ListViewGroup(KPRes.IOConnectionLong);
			m_lvAdvanced.Groups.Add(lvg);
			m_cdxAdvanced.CreateItem(Program.Config.Application, "VerifyWrittenFileAfterSaving",
				lvg, KPRes.VerifyWrittenFileAfterSave);
			m_cdxAdvanced.CreateItem(Program.Config.Application, "UseTransactedFileWrites",
				lvg, KPRes.UseTransactedDatabaseWrites);
			m_cdxAdvanced.CreateItem(Program.Config.Application, "UseTransactedConfigWrites",
				lvg, KPRes.UseTransactedConfigWrites);
			m_cdxAdvanced.CreateItem(Program.Config.Application, "FileTxExtra",
				lvg, KPRes.FileTxExtra + " (" + KPRes.Slow + ")");
			m_cdxAdvanced.CreateItem(Program.Config.Application, "UseFileLocks",
				lvg, KPRes.UseFileLocks + " " + KPRes.NotRecommended);
			m_cdxAdvanced.CreateItem(Program.Config.Application, "SaveForceSync",
				lvg, KPRes.SaveForceSync);
			m_cdxAdvanced.CreateItem(Program.Config.Security, "SslCertsAcceptInvalid",
				lvg, KPRes.SslCertsAcceptInvalid);

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
			m_cdxAdvanced.CreateItem(Program.Config.UI.Hiding, "RememberHidingPasswordsMain",
				lvg, KPRes.RememberHidingPasswordsMain);
			m_cdxAdvanced.CreateItem(Program.Config.UI.Hiding, "SeparateHidingSettings",
				lvg, KPRes.RememberHidingPasswordsEntry);
			m_cdxAdvanced.CreateItem(Program.Config.UI.Hiding, "UnhideButtonAlsoUnhidesSource",
				lvg, KPRes.UnhideSourceCharactersToo);
			m_cdxAdvanced.CreateItem(Program.Config.Defaults, "TanExpiresOnUse",
				lvg, KPRes.TanExpiresOnUse);
			m_cdxAdvanced.CreateItem(Program.Config.Defaults, "RecycleBinCollapse",
				lvg, KPRes.RecycleBinCollapse);
			m_cdxAdvanced.CreateItem(Program.Config.UI, "SecureDesktopPlaySound",
				lvg, KPRes.SecDeskPlaySound);
			m_cdxAdvanced.CreateItem(Program.Config.UI, "OptimizeForScreenReader",
				lvg, KPRes.OptimizeForScreenReader);

			m_cdxAdvanced.UpdateData(false);
			UIUtil.ResizeColumns(m_lvAdvanced, true);

			if(AppConfigEx.IsOptionEnforced(Program.Config.Integration, "ProxyType") ||
				AppConfigEx.IsOptionEnforced(Program.Config.Integration, "ProxyAddress"))
				m_btnProxy.Enabled = false;
		}

		private bool ValidateOptions()
		{
			GFunc<HotKeyControlEx, bool> fAltMod = delegate(HotKeyControlEx c)
			{
				Keys m = (c.HotKey & Keys.Modifiers);
				return ((m == Keys.Alt) || (m == (Keys.Alt | Keys.Shift)));
			};

			if(fAltMod(m_hkAutoType) || fAltMod(m_hkAutoTypePassword) ||
				fAltMod(m_hkAutoTypeSelected) || fAltMod(m_hkShowWindow))
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

			string strUuid;
			m_dTsrUuids.TryGetValue(m_cmbMenuStyle.SelectedIndex, out strUuid);
			Program.Config.UI.ToolStripRenderer = (strUuid ?? string.Empty);

			if(m_cmbBannerStyle.SelectedIndex != (int)BannerStyle.Default)
				Program.Config.UI.BannerStyle = (BannerStyle)
					m_cmbBannerStyle.SelectedIndex;

			Program.Config.UI.StandardFont = m_fcgList.SelectedFont;
			Program.Config.UI.PasswordFont = m_fcgPassword.SelectedFont;

			Program.Config.MainWindow.EscAction =
				(AceEscAction)m_cmbEscAction.SelectedIndex;

			Program.Config.Application.MostRecentlyUsed.MaxItemCount =
				(uint)m_numMruCount.Value;

			int i = m_cmbAltColor.SelectedIndex;
			Debug.Assert(Color.Empty.ToArgb() == 0);
			Program.Config.MainWindow.EntryListAlternatingBgColors = (i != 0);
			Program.Config.MainWindow.EntryListAlternatingBgColor =
				((i == 2) ? m_btnAltColor.SelectedColor.ToArgb() : 0);

			ChangeHotKey(ref m_kPrevAT, m_hkAutoType,
				AppDefs.GlobalHotKeyId.AutoType);
			ChangeHotKey(ref m_kPrevATP, m_hkAutoTypePassword,
				AppDefs.GlobalHotKeyId.AutoTypePassword);
			ChangeHotKey(ref m_kPrevATS, m_hkAutoTypeSelected,
				AppDefs.GlobalHotKeyId.AutoTypeSelected);
			ChangeHotKey(ref m_kPrevSW, m_hkShowWindow,
				AppDefs.GlobalHotKeyId.ShowWindow);

			// Program.Config.UI.TrayIcon.SingleClickDefault = m_cbSingleClickTrayAction.Checked;

			m_cdxAdvanced.UpdateData(true);

			Program.Config.Apply(AceApplyFlags.All);
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			int nTab = m_tabMain.SelectedIndex;
			if((nTab >= 0) && (nTab < m_tabMain.TabPages.Count))
				Program.Config.Defaults.OptionsTabIndex = (uint)nTab;

			m_tabMain.ImageList = null; // Detach event handlers

			m_cdxSecurityOptions.Release();
			m_cdxPolicy.Release();
			m_cdxGuiOptions.Release();
			m_cdxAdvanced.Release();

			m_fcgList.Dispose();
			m_fcgPassword.Dispose();

			AppConfigEx.ClearXmlPathCache();

			GlobalWindowManager.RemoveWindow(this);
		}

		private static void ChangeHotKey(ref Keys kPrev, HotKeyControlEx hkControl,
			int nHotKeyID)
		{
			Keys kNew = hkControl.HotKey;
			if(kNew == kPrev) return;

			kPrev = kNew;

			if(nHotKeyID == AppDefs.GlobalHotKeyId.AutoType)
				Program.Config.Integration.HotKeyGlobalAutoType = (long)kNew;
			else if(nHotKeyID == AppDefs.GlobalHotKeyId.AutoTypePassword)
				Program.Config.Integration.HotKeyGlobalAutoTypePassword = (long)kNew;
			else if(nHotKeyID == AppDefs.GlobalHotKeyId.AutoTypeSelected)
				Program.Config.Integration.HotKeySelectedAutoType = (long)kNew;
			else if(nHotKeyID == AppDefs.GlobalHotKeyId.ShowWindow)
				Program.Config.Integration.HotKeyShowWindow = (long)kNew;
			else { Debug.Assert(false); }

			HotKeyManager.UnregisterHotKey(nHotKeyID);
			if(kNew != Keys.None)
				HotKeyManager.RegisterHotKey(nHotKeyID, kNew);
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

			m_btnAltColor.Enabled = (m_cmbAltColor.Enabled &&
				(m_cmbAltColor.SelectedIndex == 2));

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
			CreateDialogBanner((BannerStyle)m_cmbBannerStyle.SelectedIndex);
		}

		private void OnLockAfterTimeCheckedChanged(object sender, EventArgs e)
		{
			UpdateUIState();
		}

		private void OnDefaultExpireDaysCheckedChanged(object sender, EventArgs e)
		{
			UpdateUIState();
		}

		private void OnBtnFileExtCreate(object sender, EventArgs e)
		{
			// ShellUtil.RegisterExtension(AppDefs.FileExtension.FileExt, AppDefs.FileExtension.FileExtId,
			//	KPRes.FileExtName2, WinUtil.GetExecutable(), PwDefs.ShortProductName, true);
			WinUtil.RunElevated(WinUtil.GetExecutable(), "-" +
				AppDefs.CommandLineOptions.FileExtRegister, false);
		}

		private void OnBtnFileExtRemove(object sender, EventArgs e)
		{
			// ShellUtil.UnregisterExtension(AppDefs.FileExtension.FileExt,
			//	AppDefs.FileExtension.FileExtId);
			WinUtil.RunElevated(WinUtil.GetExecutable(), "-" +
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
				if(!strPath.StartsWith("\"")) strPath = "\"" + strPath + "\"";
				ShellUtil.SetStartWithWindows(AppDefs.AutoRunName, strPath,
					bRequested);

				bool bNew = ShellUtil.GetStartWithWindows(AppDefs.AutoRunName);

				if(bNew != bRequested)
					m_cbAutoRun.Checked = bNew;
			}
		}

		private void OnPolicyInfoLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			AppHelp.ShowHelp(AppDefs.HelpTopics.AppPolicy, null);
		}

		private void OnClipboardClearTimeCheckedChanged(object sender, EventArgs e)
		{
			UpdateUIState();
		}

		private void OnBtnUrlOverrides(object sender, EventArgs e)
		{
			UIUtil.ShowDialogAndDestroy(new UrlOverridesForm());
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
			UIUtil.ShowDialogAndDestroy(new ProxyForm());
		}

		private void OnBtnHelpSource(object sender, EventArgs e)
		{
			UIUtil.ShowDialogAndDestroy(new HelpSourceForm());
		}

		private void OnSecOptExLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			AppHelp.ShowHelp(AppDefs.HelpTopics.Security, AppDefs.HelpTopics.SecurityOptEx);
		}

		private void OnSecOptAdmLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			AppHelp.ShowHelp(AppDefs.HelpTopics.Security, AppDefs.HelpTopics.SecurityOptAdm);
		}

		private void OnGuiDarkLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			AppHelp.ShowHelp(AppDefs.HelpTopics.FaqTech, AppDefs.HelpTopics.FaqTechGuiDark);
		}

		private void OnGuiFontLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			AppHelp.ShowHelp(AppDefs.HelpTopics.FaqTech, AppDefs.HelpTopics.FaqTechGuiFont);
		}

		private void OnAltColorSelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateUIState();
		}
	}
}
