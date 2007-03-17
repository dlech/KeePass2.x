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

using KeePass.App;
using KeePass.UI;
using KeePass.Resources;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	/// <summary>
	/// Options dialog. In this dialog, global program options can be configured.
	/// </summary>
	public partial class OptionsForm : Form
	{
		private ImageList m_ilIcons;
		private BannerFactory.BannerStyle m_curBannerStyle = BannerFactory.BannerStyle.KeePassWin32;
		private bool m_bBlockUIUpdate = false;
		private bool m_bLoadingSettings = false;

		private CheckedLVItemDXList m_cdxSecurityOptions = new CheckedLVItemDXList();

		private CheckedLVItemDXList m_cdxGuiOptions = new CheckedLVItemDXList();
		private HotKeyControlEx m_hkGlobalAutoType = null;
		private HotKeyControlEx m_hkShowWindow = null;

		private Keys m_kPrevATHKKey = Keys.None;
		private Keys m_kPrevATHKMod = Keys.None;
		private Keys m_kPrevSWHKKey = Keys.None;
		private Keys m_kPrevSWHKMod = Keys.None;

		private CheckedLVItemDXList m_cdxAdvanced = new CheckedLVItemDXList();

		/// <summary>
		/// Default constructor.
		/// </summary>
		public OptionsForm()
		{
			InitializeComponent();
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

		private void CreateDialogBanner(BannerFactory.BannerStyle bsStyle)
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

			CreateDialogBanner(BannerFactory.BannerStyle.Default);
			m_cmbBannerStyle.SelectedIndex = (int)BannerFactory.BannerStyle.Default;

			bool bAdmin = AppConfigEx.HasWriteAccessToGlobal();
			if(bAdmin)
			{
				m_lblPolicyCurrentUserLevel.Text = KPRes.ConfigWriteGlobal;
				m_lblPolicyAffectHint.Text = KPRes.ConfigAffectAdmin;
			}
			else
			{
				m_lblPolicyCurrentUserLevel.Text = KPRes.ConfigWriteLocal;
				m_lblPolicyAffectHint.Text = KPRes.ConfigAffectUser;
			}

			int nWidth = m_lvPolicy.ClientRectangle.Width - 36;
			m_lvPolicy.Columns.Add(KPRes.Flag, nWidth / 4);
			m_lvPolicy.Columns.Add(KPRes.Description, (nWidth * 3) / 4);

			ListViewItem lvi;
			lvi = m_lvPolicy.Items.Add(KPRes.Plugins);
			lvi.SubItems.Add(KPRes.PolicyPluginsDesc);
			lvi = m_lvPolicy.Items.Add(KPRes.Export);
			lvi.SubItems.Add(KPRes.PolicyExportDesc);
			lvi = m_lvPolicy.Items.Add(KPRes.Import);
			lvi.SubItems.Add(KPRes.PolicyImportDesc);
			lvi = m_lvPolicy.Items.Add(KPRes.Print);
			lvi.SubItems.Add(KPRes.PolicyPrintDesc);
			lvi = m_lvPolicy.Items.Add(KPRes.SaveDatabase);
			lvi.SubItems.Add(KPRes.PolicySaveDatabaseDesc);
			lvi = m_lvPolicy.Items.Add(KPRes.AutoType);
			lvi.SubItems.Add(KPRes.PolicyAutoTypeDesc);
			lvi = m_lvPolicy.Items.Add(KPRes.Clipboard);
			lvi.SubItems.Add(KPRes.PolicyClipboardDesc);
			lvi = m_lvPolicy.Items.Add(KPRes.DragDrop);
			lvi.SubItems.Add(KPRes.PolicyDragDropDesc);

			m_hkGlobalAutoType = HotKeyControlEx.ReplaceTextBox(m_grpHotKeys, m_tbGlobalAutoType);
			m_hkShowWindow = HotKeyControlEx.ReplaceTextBox(m_grpHotKeys, m_tbShowWindowHotKey);

			LoadOptions();
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
			m_numLockAfterTime.Value = AppConfigEx.GetUInt(AppDefs.ConfigKeys.LockAfterTime);
			m_cbLockAfterTime.Checked = (m_numLockAfterTime.Value > 0);

			int nDefaultExpireDays = AppConfigEx.GetInt(AppDefs.ConfigKeys.DefaultExpireDays);
			if(nDefaultExpireDays < 0)
				m_cbDefaultExpireDays.Checked = false;
			else
			{
				m_cbDefaultExpireDays.Checked = true;
				try { m_numDefaultExpireDays.Value = nDefaultExpireDays; }
				catch(Exception) { Debug.Assert(false); }
			}

			int nClipClear = AppConfigEx.GetInt(AppDefs.ConfigKeys.ClipboardAutoClearTime);
			if(nClipClear >= 0)
			{
				m_cbClipClearTime.Checked = true;
				m_numClipClearTime.Value = nClipClear;
			}
			else m_cbClipClearTime.Checked = false;

			if(NativeLib.AllowNative && !NativeLib.IsLibraryInstalled())
				NativeLib.AllowNative = false;
			AppConfigEx.SetValue(AppDefs.ConfigKeys.UseNativeForKeyEnc,
				NativeLib.AllowNative);

			m_lvSecurityOptions.Columns.Add(string.Empty, 200); // Resize below

			ListViewGroup lvg = new ListViewGroup(KPRes.Options);
			m_lvSecurityOptions.Groups.Add(lvg);
			Debug.Assert(lvg.ListView == m_lvSecurityOptions);

			m_cdxSecurityOptions.CreateItem(AppDefs.ConfigKeys.LockOnMinimize,
				m_lvSecurityOptions, lvg, KPRes.LockOnMinimize);
			m_cdxSecurityOptions.CreateItem(AppDefs.ConfigKeys.LockOnSessionLock,
				m_lvSecurityOptions, lvg, KPRes.LockOnSessionLock);
			m_cdxSecurityOptions.CreateItem(AppDefs.ConfigKeys.ClipboardAutoClearOnExit,
				m_lvSecurityOptions, lvg, KPRes.ClipboardClearOnExit);
			m_cdxSecurityOptions.CreateItem(AppDefs.ConfigKeys.UseNativeForKeyEnc,
				m_lvSecurityOptions, lvg, KPRes.NativeLibUse);

			m_cdxSecurityOptions.UpdateData(false);
			m_lvSecurityOptions.Columns[0].Width = m_lvSecurityOptions.ClientRectangle.Width - 36;
		}

		private void LoadPolicyOptions()
		{
			Debug.Assert(m_lvPolicy.Items.Count == (int)AppPolicyFlag.Count);
			m_lvPolicy.Items[(int)AppPolicyFlag.Plugins].Checked = AppPolicy.NewIsAllowed(AppPolicyFlag.Plugins);
			m_lvPolicy.Items[(int)AppPolicyFlag.Export].Checked = AppPolicy.NewIsAllowed(AppPolicyFlag.Export);
			m_lvPolicy.Items[(int)AppPolicyFlag.Import].Checked = AppPolicy.NewIsAllowed(AppPolicyFlag.Import);
			m_lvPolicy.Items[(int)AppPolicyFlag.Print].Checked = AppPolicy.NewIsAllowed(AppPolicyFlag.Print);
			m_lvPolicy.Items[(int)AppPolicyFlag.SaveDatabase].Checked = AppPolicy.NewIsAllowed(AppPolicyFlag.SaveDatabase);
			m_lvPolicy.Items[(int)AppPolicyFlag.AutoType].Checked = AppPolicy.NewIsAllowed(AppPolicyFlag.AutoType);
			m_lvPolicy.Items[(int)AppPolicyFlag.CopyToClipboard].Checked = AppPolicy.NewIsAllowed(AppPolicyFlag.CopyToClipboard);
			m_lvPolicy.Items[(int)AppPolicyFlag.DragDrop].Checked = AppPolicy.NewIsAllowed(AppPolicyFlag.DragDrop);
		}

		private void LoadGuiOptions()
		{
			m_lvGuiOptions.Columns.Add(KPRes.Options, 200); // Resize below

			ListViewGroup lvg = new ListViewGroup(KPRes.MainWindow);
			m_lvGuiOptions.Groups.Add(lvg);
			Debug.Assert(lvg.ListView == m_lvGuiOptions);

			m_cdxGuiOptions.CreateItem(AppDefs.ConfigKeys.CloseButtonMinimizes, m_lvGuiOptions,
				lvg, KPRes.CloseButtonMinimizes);
			m_cdxGuiOptions.CreateItem(AppDefs.ConfigKeys.MinimizeToTray, m_lvGuiOptions,
				lvg, KPRes.MinimizeToTray);
			m_cdxGuiOptions.CreateItem(AppDefs.ConfigKeys.ShowTrayIconOnlyIfTrayed, m_lvGuiOptions,
				lvg, KPRes.ShowTrayOnlyIfTrayed);
			m_cdxGuiOptions.CreateItem(AppDefs.ConfigKeys.ShowFullPathInTitleBar, m_lvGuiOptions,
				lvg, KPRes.ShowFullPathInTitleBar);
			m_cdxGuiOptions.CreateItem(AppDefs.ConfigKeys.MinimizeAfterCopy, m_lvGuiOptions,
				lvg, KPRes.MinimizeAfterCopy);

			lvg = new ListViewGroup(KPRes.EntryList);
			m_lvGuiOptions.Groups.Add(lvg);
			m_cdxGuiOptions.CreateItem(AppDefs.ConfigKeys.ShowGridLines, m_lvGuiOptions,
				lvg, KPRes.ShowGridLines);

			m_cdxGuiOptions.UpdateData(false);
			m_lvGuiOptions.Columns[0].Width = m_lvGuiOptions.ClientRectangle.Width - 36;
		}

		private void LoadIntegrationOptions()
		{
			m_hkGlobalAutoType.HotKey = (Keys)AppConfigEx.GetULong(
				AppDefs.ConfigKeys.GlobalAutoTypeHotKey);
			m_hkGlobalAutoType.HotKeyModifiers = (Keys)AppConfigEx.GetULong(
				AppDefs.ConfigKeys.GlobalAutoTypeModifiers);
			m_hkGlobalAutoType.RenderHotKey();
			m_kPrevATHKKey = m_hkGlobalAutoType.HotKey;
			m_kPrevATHKMod = m_hkGlobalAutoType.HotKeyModifiers;

			m_hkShowWindow.HotKey = (Keys)AppConfigEx.GetULong(
				AppDefs.ConfigKeys.ShowWindowHotKey);
			m_hkShowWindow.HotKeyModifiers = (Keys)AppConfigEx.GetULong(
				AppDefs.ConfigKeys.ShowWindowHotKeyModifiers);
			m_hkShowWindow.RenderHotKey();
			m_kPrevSWHKKey = m_hkShowWindow.HotKey;
			m_kPrevSWHKMod = m_hkShowWindow.HotKeyModifiers;

			m_cbAutoRun.Checked = ShellUtil.GetStartWithWindows(AppDefs.AutoRunName);
			m_cbSingleClickTrayAction.Checked = AppConfigEx.GetBool(AppDefs.ConfigKeys.SingleClickForTrayAction);

			string strOverride = AppConfigEx.GetValue(AppDefs.ConfigKeys.UrlOverride);
			m_cbUrlOverride.Checked = (strOverride.Length > 0);
			m_tbUrlOverride.Text = strOverride;
		}

		private void LoadAdvancedOptions()
		{
			m_lvAdvanced.Columns.Add(string.Empty, 200); // Resize below

			ListViewGroup lvg = new ListViewGroup(KPRes.StartAndExit);
			m_lvAdvanced.Groups.Add(lvg);
			m_cdxAdvanced.CreateItem(AppDefs.ConfigKeys.AutoOpenLastFile, m_lvAdvanced,
				lvg, KPRes.AutoOpenLastFile);
			m_cdxAdvanced.CreateItem(AppDefs.ConfigKeys.AutoSaveOnExit, m_lvAdvanced,
				lvg, KPRes.AutoSaveAtExit);
			m_cdxAdvanced.CreateItem(AppDefs.ConfigKeys.LimitSingleInstance, m_lvAdvanced,
				lvg, KPRes.LimitSingleInstance);
			m_cdxAdvanced.CreateItem(AppDefs.ConfigKeys.AutoCheckForUpdate, m_lvAdvanced,
				lvg, KPRes.CheckForUpdAtStart);

			lvg = new ListViewGroup(KPRes.AfterDatabaseOpen);
			m_lvAdvanced.Groups.Add(lvg);
			m_cdxAdvanced.CreateItem(AppDefs.ConfigKeys.ShowExpiredOnDbOpen, m_lvAdvanced,
				lvg, KPRes.AutoShowExpiredEntries);
			m_cdxAdvanced.CreateItem(AppDefs.ConfigKeys.ShowSoonToExpireOnDbOpen, m_lvAdvanced,
				lvg, KPRes.AutoShowSoonToExpireEntries);

			lvg = new ListViewGroup(KPRes.Advanced);
			m_lvAdvanced.Groups.Add(lvg);
			m_cdxAdvanced.CreateItem(AppDefs.ConfigKeys.DefaultGeneratePw, m_lvAdvanced,
				lvg, KPRes.GenRandomPwForNewEntry);
			m_cdxAdvanced.CreateItem(AppDefs.ConfigKeys.SearchKeyFilesOnRemovable, m_lvAdvanced,
				lvg, KPRes.SearchKeyFilesOnRemovable);

			m_cdxAdvanced.UpdateData(false);
			m_lvAdvanced.Columns[0].Width = m_lvAdvanced.ClientRectangle.Width - 36;
		}

		private void SaveOptions()
		{
			bool bNewNative = AppConfigEx.GetBool(AppDefs.ConfigKeys.UseNativeForKeyEnc);
			if(bNewNative && !NativeLib.AllowNative)
			{
				if(NativeLib.IsLibraryInstalled() == false)
				{
					MessageBox.Show(KPRes.NoNativeLib + "\r\n\r\n" + KPRes.NoNativeLibHint,
						PwDefs.ShortProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					bNewNative = false;
				}
			}
			NativeLib.AllowNative = bNewNative;

			if(!m_cbLockAfterTime.Checked)
				AppConfigEx.SetValue(AppDefs.ConfigKeys.LockAfterTime, 0);
			else
				AppConfigEx.SetValue(AppDefs.ConfigKeys.LockAfterTime, (uint)m_numLockAfterTime.Value);

			if(m_cbDefaultExpireDays.Checked)
				AppConfigEx.SetValue(AppDefs.ConfigKeys.DefaultExpireDays,
					(int)m_numDefaultExpireDays.Value);
			else AppConfigEx.SetValue(AppDefs.ConfigKeys.DefaultExpireDays, -1);

			if(m_cbClipClearTime.Checked)
				AppConfigEx.SetValue(AppDefs.ConfigKeys.ClipboardAutoClearTime,
					(int)m_numClipClearTime.Value);
			else AppConfigEx.SetValue(AppDefs.ConfigKeys.ClipboardAutoClearTime, -1);

			m_cdxSecurityOptions.UpdateData(true);

			AppPolicy.NewAllow(AppPolicyFlag.Plugins, m_lvPolicy.Items[(int)AppPolicyFlag.Plugins].Checked);
			AppPolicy.NewAllow(AppPolicyFlag.Export, m_lvPolicy.Items[(int)AppPolicyFlag.Export].Checked);
			AppPolicy.NewAllow(AppPolicyFlag.Import, m_lvPolicy.Items[(int)AppPolicyFlag.Import].Checked);
			AppPolicy.NewAllow(AppPolicyFlag.Print, m_lvPolicy.Items[(int)AppPolicyFlag.Print].Checked);
			AppPolicy.NewAllow(AppPolicyFlag.SaveDatabase, m_lvPolicy.Items[(int)AppPolicyFlag.SaveDatabase].Checked);
			AppPolicy.NewAllow(AppPolicyFlag.AutoType, m_lvPolicy.Items[(int)AppPolicyFlag.AutoType].Checked);
			AppPolicy.NewAllow(AppPolicyFlag.CopyToClipboard, m_lvPolicy.Items[(int)AppPolicyFlag.CopyToClipboard].Checked);
			AppPolicy.NewAllow(AppPolicyFlag.DragDrop, m_lvPolicy.Items[(int)AppPolicyFlag.DragDrop].Checked);

			m_cdxGuiOptions.UpdateData(true);

			if(m_cmbBannerStyle.SelectedIndex != (int)BannerFactory.BannerStyle.Default)
				AppConfigEx.SetValue(AppDefs.ConfigKeys.BannerStyle, (uint)m_cmbBannerStyle.SelectedIndex);

			ChangeHotKey(ref m_kPrevATHKKey, ref m_kPrevATHKMod, m_hkGlobalAutoType,
				AppDefs.ConfigKeys.GlobalAutoTypeHotKey.Key,
				AppDefs.ConfigKeys.GlobalAutoTypeModifiers.Key,
				(int)AppDefs.GlobalHotKeyID.AutoType);
			ChangeHotKey(ref m_kPrevSWHKKey, ref m_kPrevSWHKMod, m_hkShowWindow,
				AppDefs.ConfigKeys.ShowWindowHotKey.Key,
				AppDefs.ConfigKeys.ShowWindowHotKeyModifiers.Key,
				(int)AppDefs.GlobalHotKeyID.ShowWindow);

			AppConfigEx.SetValue(AppDefs.ConfigKeys.SingleClickForTrayAction, m_cbSingleClickTrayAction.Checked);

			if(m_cbUrlOverride.Checked)
				AppConfigEx.SetValue(AppDefs.ConfigKeys.UrlOverride, m_tbUrlOverride.Text);
			else AppConfigEx.SetValue(AppDefs.ConfigKeys.UrlOverride, string.Empty);

			m_cdxAdvanced.UpdateData(true);
		}

		private static void ChangeHotKey(ref Keys kPrevHK, ref Keys kPrevMod,
			HotKeyControlEx hkControl, string strConfigKey, string strConfigMod,
			int nHotKeyID)
		{
			if((kPrevHK != hkControl.HotKey) || (kPrevMod != hkControl.HotKeyModifiers))
			{
				kPrevHK = hkControl.HotKey;
				AppConfigEx.SetValue(strConfigKey, (ulong)kPrevHK);

				kPrevMod = hkControl.HotKeyModifiers;
				AppConfigEx.SetValue(strConfigMod, (ulong)kPrevMod);

				HotKeyManager.UnregisterHotKey(nHotKeyID);
				if(kPrevHK != Keys.None)
					HotKeyManager.RegisterHotKey(nHotKeyID, kPrevHK, kPrevMod);
			}
		}

		private void UpdateUIState()
		{
			if(m_bBlockUIUpdate) return;
			m_bBlockUIUpdate = true;

			m_numLockAfterTime.Enabled = m_cbLockAfterTime.Checked;
			m_numDefaultExpireDays.Enabled = m_cbDefaultExpireDays.Checked;

			m_tbUrlOverride.Enabled = m_cbUrlOverride.Checked;

			m_bBlockUIUpdate = false;
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			SaveOptions();
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
		}

		private void OnBannerStyleSelectedChanged(object sender, EventArgs e)
		{
			int nIndex = m_cmbBannerStyle.SelectedIndex;
			
			BannerFactory.BannerStyle bs = (BannerFactory.BannerStyle)nIndex;
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
			Font fOld = WinUtil.FontIDToFont(AppConfigEx.GetValue(AppDefs.ConfigKeys.ListFont));
			if(fOld != null) m_fontLists.Font = fOld;

			if(m_fontLists.ShowDialog() == DialogResult.OK)
				AppConfigEx.SetValue(AppDefs.ConfigKeys.ListFont, WinUtil.FontToFontID(m_fontLists.Font));
		}

		private void OnDefaultExpireDaysCheckedChanged(object sender, EventArgs e)
		{
			UpdateUIState();
		}

		private void OnBtnFileExtCreate(object sender, EventArgs e)
		{
			ShellUtil.RegisterExtension(AppDefs.FileExtension.FileExt, AppDefs.FileExtension.ExtID,
				KPRes.FileExtName, WinUtil.GetExecutable(), PwDefs.ShortProductName, true);
		}

		private void OnBtnFileExtRemove(object sender, EventArgs e)
		{
			ShellUtil.UnregisterExtension(AppDefs.FileExtension.FileExt, AppDefs.FileExtension.ExtID);
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
	}
}
