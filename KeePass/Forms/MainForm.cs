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
using System.Threading;
using System.Windows.Forms;
using System.Globalization;
using System.Diagnostics;
using System.Security;

using KeePass.App;
using KeePass.DataExchange;
using KeePass.Native;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Cryptography;
using KeePassLib.Cryptography.Cipher;
using KeePassLib.Interfaces;
using KeePassLib.Utility;
using KeePassLib.Security;
using KeePassLib.Delegates;
using KeePassLib.Serialization;

using NativeLib = KeePassLib.Native.NativeLib;

namespace KeePass.Forms
{
	/// <summary>
	/// KeePass main window.
	/// </summary>
	public partial class MainForm : Form, IMruExecuteHandler, IUIOperations
	{
		private PwDatabase m_pwDatabase = new PwDatabase();
		private bool m_bRestart = false;
		private ListSorter m_pListSorter = new ListSorter(-1, SortOrder.Ascending);
		private bool m_bBlockQuickFind = false;

		private bool m_bDraggingEntries = false;

		private bool m_bDisableBlockingColumnSizing = false;
		private bool m_bBlockEntrySelectionEvent = false;

		private bool m_bForceExitOnce = false;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public MainForm()
		{
			CryptoRandom.Initialize();

			string strLang = AppConfigEx.GetValue(AppDefs.ConfigKeys.Language);
			if(strLang != null)
			{
				CultureInfo ci = CultureInfo.CreateSpecificCulture(strLang);
				Application.CurrentCulture = ci;
				Thread.CurrentThread.CurrentCulture = ci;
				Thread.CurrentThread.CurrentUICulture = ci;
				Properties.Resources.Culture = ci;
			}

			UISelfTest();

			InitializeComponent();

			m_splitHorizontal.InitEx(this.Controls, m_menuMain);
			m_splitVertical.InitEx(this.Controls, m_menuMain);

			AssignMenuShortcuts();
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			m_strNeverExpiresText = KPRes.NeverExpires;

			this.Icon = Properties.Resources.KeePass;
			m_imgFileSaveEnabled = Properties.Resources.B16x16_FileSave;
			m_imgFileSaveDisabled = Properties.Resources.B16x16_FileSave_Disabled;

			m_nLockTimerMax = (int)AppConfigEx.GetUInt(AppDefs.ConfigKeys.LockAfterTime);
			m_nClipClearMax = AppConfigEx.GetInt(AppDefs.ConfigKeys.ClipboardAutoClearTime);

			NativeLib.AllowNative = AppConfigEx.GetBool(AppDefs.ConfigKeys.UseNativeForKeyEnc);

			m_ctxEntryPreviewContextMenu.Attach(m_richEntryView);

			m_dynCustomStrings = new DynamicMenu(m_ctxEntryCopyCustomString);
			m_dynCustomStrings.MenuClick += this.OnCopyCustomString;

			m_dynCustomBinaries = new DynamicMenu(m_ctxEntryBinaries);
			m_dynCustomBinaries.MenuClick += this.OnEntryBinaryView;

			SelfTestResult st = SelfTest.Perform();
			if(st != SelfTestResult.Success)
				MessageService.ShowFatal(KPRes.SelfTestFailed);

			EntryTemplates.Init(m_tbAddEntry);

			m_menuEdit.DropDownItems.Insert(0, new ToolStripSeparator());
			InsertToolStripItem(m_menuEdit, m_ctxEntrySelectAll, new EventHandler(OnEntrySelectAll), true);
			m_menuEdit.DropDownItems.Insert(0, new ToolStripSeparator());
			InsertToolStripItem(m_menuEdit, m_ctxEntryDelete, new EventHandler(OnEntryDelete), true);
			InsertToolStripItem(m_menuEdit, m_ctxEntryDuplicate, new EventHandler(OnEntryDuplicate), true);
			InsertToolStripItem(m_menuEdit, m_ctxEntryEdit, new EventHandler(OnEntryEdit), true);
			InsertToolStripItem(m_menuEdit, m_ctxEntryAdd, new EventHandler(OnEntryAdd), true);
			m_menuEdit.DropDownItems.Insert(0, new ToolStripSeparator());
			InsertToolStripItem(m_menuEdit, m_ctxGroupDelete, new EventHandler(OnGroupsDelete), true);
			InsertToolStripItem(m_menuEdit, m_ctxGroupEdit, new EventHandler(OnGroupsEdit), true);
			InsertToolStripItem(m_menuEdit, m_ctxGroupAdd, new EventHandler(OnGroupsAdd), true);

			bool bVisible = AppConfigEx.GetBool(AppDefs.ConfigKeys.ShowToolBar);
			m_toolMain.Visible = bVisible;
			m_menuViewShowToolBar.Checked = bVisible;

			int wndX = AppConfigEx.GetInt(AppDefs.ConfigKeys.MainWindowPositionX);
			int wndY = AppConfigEx.GetInt(AppDefs.ConfigKeys.MainWindowPositionY);
			int sizeX = AppConfigEx.GetInt(AppDefs.ConfigKeys.MainWindowWidth);
			int sizeY = AppConfigEx.GetInt(AppDefs.ConfigKeys.MainWindowHeight);
			bool bWndValid = ((wndX != -32000) && (wndY != -32000) && (wndX != -64000) && (wndY != -64000));

			if((sizeX != -16381) && (sizeY != -16381) && bWndValid)
				this.Size = new Size(sizeX, sizeY);

			Rectangle rectScreen = Screen.GetWorkingArea(this);

			if((wndX != -16381) && (wndY != -16381) && bWndValid)
				this.Location = new Point(wndX, wndY);
			else
				this.Location = new Point((rectScreen.Width - this.Size.Width) / 2,
					(rectScreen.Height - this.Size.Height) / 2);

			SetMainWindowLayout(AppConfigEx.GetBool(AppDefs.ConfigKeys.MainWindowLayoutSideBySide));

			try
			{
				int nSplitPos = AppConfigEx.GetInt(AppDefs.ConfigKeys.MainWindowHorzSplitter);
				if(nSplitPos == -128) nSplitPos = (m_splitHorizontal.ClientRectangle.Height * 5) / 6;
				m_splitHorizontal.SplitterDistance = nSplitPos;

				nSplitPos = AppConfigEx.GetInt(AppDefs.ConfigKeys.MainWindowVertSplitter);
				if(nSplitPos == -128) nSplitPos = m_splitVertical.ClientRectangle.Width / 4;
				m_splitVertical.SplitterDistance = nSplitPos;
			}
			catch(Exception) { Debug.Assert(false); }

			ShowEntryView(AppConfigEx.GetBool(AppDefs.ConfigKeys.ShowEntryView));

			ColumnHeader ch;
			uint uDefaultWidth = (uint)(m_lvEntries.ClientRectangle.Width / 5);

			m_bDisableBlockingColumnSizing = true;
			ch = m_lvEntries.Columns.Add(KPRes.Title);
			ch.Width = (int)AppConfigEx.GetUInt(AppDefs.ConfigKeys.TitleColumnWidth.Key, uDefaultWidth);
			ch = m_lvEntries.Columns.Add(KPRes.UserName);
			ch.Width = (int)AppConfigEx.GetUInt(AppDefs.ConfigKeys.UserNameColumnWidth.Key, uDefaultWidth);
			ch = m_lvEntries.Columns.Add(KPRes.Password);
			ch.Width = (int)AppConfigEx.GetUInt(AppDefs.ConfigKeys.PasswordColumnWidth.Key, uDefaultWidth);
			ch = m_lvEntries.Columns.Add(KPRes.URL);
			ch.Width = (int)AppConfigEx.GetUInt(AppDefs.ConfigKeys.UrlColumnWidth.Key, uDefaultWidth);
			ch = m_lvEntries.Columns.Add(KPRes.Notes);
			ch.Width = (int)AppConfigEx.GetUInt(AppDefs.ConfigKeys.NotesColumnWidth.Key, uDefaultWidth);
			ch = m_lvEntries.Columns.Add(KPRes.CreationTime);
			ch.Width = (int)AppConfigEx.GetUInt(AppDefs.ConfigKeys.CreationTimeColumnWidth.Key, 0);
			ch = m_lvEntries.Columns.Add(KPRes.LastAccessTime);
			ch.Width = (int)AppConfigEx.GetUInt(AppDefs.ConfigKeys.LastAccessTimeColumnWidth.Key, 0);
			ch = m_lvEntries.Columns.Add(KPRes.LastModificationTime);
			ch.Width = (int)AppConfigEx.GetUInt(AppDefs.ConfigKeys.LastModTimeColumnWidth.Key, 0);
			ch = m_lvEntries.Columns.Add(KPRes.ExpiryTime);
			ch.Width = (int)AppConfigEx.GetUInt(AppDefs.ConfigKeys.ExpireTimeColumnWidth.Key, 0);
			ch = m_lvEntries.Columns.Add(KPRes.UUID);
			ch.Width = (int)AppConfigEx.GetUInt(AppDefs.ConfigKeys.UuidColumnWidth.Key, 0);
			ch = m_lvEntries.Columns.Add(KPRes.Attachments);
			ch.Width = (int)AppConfigEx.GetUInt(AppDefs.ConfigKeys.AttachmentColumnWidth.Key, 0);
			m_bDisableBlockingColumnSizing = false;

			Debug.Assert(m_lvEntries.Columns.Count == (int)AppDefs.ColumnID.Count);

			for(int iCol = 0; iCol < (int)AppDefs.ColumnID.Count; ++iCol)
				m_vShowColumns[iCol] = (m_lvEntries.Columns[iCol].Width > 0);

			m_menuViewColumnsShowTitle.Checked = m_vShowColumns[(int)AppDefs.ColumnID.Title];
			m_menuViewColumnsShowUserName.Checked = m_vShowColumns[(int)AppDefs.ColumnID.UserName];
			m_menuViewColumnsShowPassword.Checked = m_vShowColumns[(int)AppDefs.ColumnID.Password];
			m_menuViewColumnsShowUrl.Checked = m_vShowColumns[(int)AppDefs.ColumnID.Url];
			m_menuViewColumnsShowNotes.Checked = m_vShowColumns[(int)AppDefs.ColumnID.Notes];
			m_menuViewColumnsShowCreation.Checked = m_vShowColumns[(int)AppDefs.ColumnID.CreationTime];
			m_menuViewColumnsShowLastAccess.Checked = m_vShowColumns[(int)AppDefs.ColumnID.LastAccessTime];
			m_menuViewColumnsShowLastMod.Checked = m_vShowColumns[(int)AppDefs.ColumnID.LastModificationTime];
			m_menuViewColumnsShowExpire.Checked = m_vShowColumns[(int)AppDefs.ColumnID.ExpiryTime];
			m_menuViewColumnsShowUuid.Checked = m_vShowColumns[(int)AppDefs.ColumnID.Uuid];
			m_menuViewColumnsShowAttachs.Checked = m_vShowColumns[(int)AppDefs.ColumnID.Attachment];

			m_menuViewHideTitles.Checked = m_viewHideFields.ProtectTitle =
				AppConfigEx.GetBool(AppDefs.ConfigKeys.HideTitles);
			m_menuViewHideUserNames.Checked = m_viewHideFields.ProtectUserName =
				AppConfigEx.GetBool(AppDefs.ConfigKeys.HideUserNames);
			m_menuViewHidePasswords.Checked = m_viewHideFields.ProtectPassword =
				AppConfigEx.GetBool(AppDefs.ConfigKeys.HidePasswords);
			m_menuViewHideURLs.Checked = m_viewHideFields.ProtectUrl =
				AppConfigEx.GetBool(AppDefs.ConfigKeys.HideUrls);
			m_menuViewHideNotes.Checked = m_viewHideFields.ProtectNotes =
				AppConfigEx.GetBool(AppDefs.ConfigKeys.HideNotes);

			m_menuViewTanSimpleList.Checked = m_bSimpleTanView =
				AppConfigEx.GetBool(AppDefs.ConfigKeys.TanSimpleList);
			m_menuViewTanIndices.Checked = m_bShowTanIndices =
				AppConfigEx.GetBool(AppDefs.ConfigKeys.TanIndices);

			m_menuViewAlwaysOnTop.Checked = AppConfigEx.GetBool(AppDefs.ConfigKeys.AlwaysOnTop);
			OnViewAlwaysOnTop(null, null);

			m_mruList.Initialize(this, m_menuFileRecent);

			m_mruList.MaxItemCount = AppConfigEx.GetUInt(AppDefs.ConfigKeys.MruMaxItemCount);
			for(uint uMru = 0; ; ++uMru)
			{
				string strMruItem = AppConfigEx.GetValue(AppDefs.ConfigKeys.MruItem.Key +
					uMru.ToString(), null);
				if(strMruItem == null) break;

				string[] vMruParts = strMruItem.Split(new string[]{
					AppDefs.MruNameValueSplitter }, StringSplitOptions.None);
				if(vMruParts.Length == 2)
					m_mruList.AddItem(vMruParts[0], vMruParts[1]);
				else { Debug.Assert(false); }
			}
			m_mruList.UpdateMenu();

			SetListFont(AppConfigEx.GetValue(AppDefs.ConfigKeys.ListFont));

			m_ctxEntryColorLightRed.Image = UIUtil.CreateColorBitmap24(16, 16,
				AppDefs.NamedEntryColor.LightRed);
			m_ctxEntryColorLightGreen.Image = UIUtil.CreateColorBitmap24(16, 16,
				AppDefs.NamedEntryColor.LightGreen);
			m_ctxEntryColorLightBlue.Image = UIUtil.CreateColorBitmap24(16, 16,
				AppDefs.NamedEntryColor.LightBlue);
			m_ctxEntryColorLightYellow.Image = UIUtil.CreateColorBitmap24(16, 16,
				AppDefs.NamedEntryColor.LightYellow);

			m_lvEntries.GridLines = AppConfigEx.GetBool(AppDefs.ConfigKeys.ShowGridLines);

			m_statusPartProgress.Visible = false;

			if(AppConfigEx.GetBool(AppDefs.ConfigKeys.MainWindowMaximized))
				this.WindowState = FormWindowState.Maximized;

			m_sessionLockNotifier.Install(OnSessionLock);

			AppPolicy.CurrentAllowAll(true);
			AppPolicy.CurrentAllow(AppPolicyFlag.Plugins, AppConfigEx.GetBool(AppDefs.ConfigKeys.PolicyPlugins));
			AppPolicy.CurrentAllow(AppPolicyFlag.Export, AppConfigEx.GetBool(AppDefs.ConfigKeys.PolicyExport));
			AppPolicy.CurrentAllow(AppPolicyFlag.Import, AppConfigEx.GetBool(AppDefs.ConfigKeys.PolicyImport));
			AppPolicy.CurrentAllow(AppPolicyFlag.Print, AppConfigEx.GetBool(AppDefs.ConfigKeys.PolicyPrint));
			AppPolicy.CurrentAllow(AppPolicyFlag.SaveDatabase, AppConfigEx.GetBool(AppDefs.ConfigKeys.PolicySaveDatabase));
			AppPolicy.CurrentAllow(AppPolicyFlag.AutoType, AppConfigEx.GetBool(AppDefs.ConfigKeys.PolicyAutoType));
			AppPolicy.CurrentAllow(AppPolicyFlag.CopyToClipboard, AppConfigEx.GetBool(AppDefs.ConfigKeys.PolicyCopyToClipboard));
			AppPolicy.CurrentAllow(AppPolicyFlag.DragDrop, AppConfigEx.GetBool(AppDefs.ConfigKeys.PolicyDragDrop));

			m_pluginDefaultHost.Initialize(this, m_pwDatabase,
				Program.CommandLineArgs, CipherPool.GlobalPool);
			m_pluginManager.Initialize(m_pluginDefaultHost);

			m_pluginManager.UnloadAllPlugins();
			if(AppPolicy.IsAllowed(AppPolicyFlag.Plugins))
				m_pluginManager.LoadAllPlugins(UrlUtil.GetFileDirectory(WinUtil.GetExecutable(), false));

			HotKeyManager.ReceiverWindow = this.Handle;

			Keys kAutoTypeKey = (Keys)AppConfigEx.GetULong(AppDefs.ConfigKeys.GlobalAutoTypeHotKey);
			Keys kAutoTypeMod = (Keys)AppConfigEx.GetULong(AppDefs.ConfigKeys.GlobalAutoTypeModifiers);
			HotKeyManager.RegisterHotKey(AppDefs.GlobalHotKeyID.AutoType, kAutoTypeKey, kAutoTypeMod);

			Keys kShowWindowKey = (Keys)AppConfigEx.GetULong(AppDefs.ConfigKeys.ShowWindowHotKey);
			Keys kShowWindowMod = (Keys)AppConfigEx.GetULong(AppDefs.ConfigKeys.ShowWindowHotKeyModifiers);
			HotKeyManager.RegisterHotKey(AppDefs.GlobalHotKeyID.ShowWindow,
				kShowWindowKey, kShowWindowMod);

			m_statusClipboard.Visible = false;
			UpdateClipboardStatus();

			UpdateTrayIcon();
			UpdateUIState(false);

			if(Program.CommandLineArgs.FileName != null)
				OpenDatabase(IOConnectionInfo.SerializeToString(
					IOConnectionInfo.FromPath(Program.CommandLineArgs.FileName)),
					KeyUtil.KeyFromCommandLine(), false);
			else if(AppConfigEx.GetBool(AppDefs.ConfigKeys.AutoOpenLastFile))
			{
				string strLastFile = AppConfigEx.GetValue(AppDefs.ConfigKeys.LastDatabase);
				if(strLastFile.Length > 0)
					OpenDatabase(strLastFile, null, false);
			}

			if(AppConfigEx.GetBool(AppDefs.ConfigKeys.AutoCheckForUpdate))
				CheckForUpdate.StartAsync(PwDefs.VersionUrl, m_statusPartInfo);

			ResetDefaultFocus();
		}

		private void OnFileNew(object sender, EventArgs e)
		{
			if(!AppPolicy.Try(AppPolicyFlag.SaveDatabase)) return;

			OnFileClose(sender, e);
			if(m_pwDatabase.IsOpen) return;

			string strPrevTitle = m_saveDatabaseFile.Title;
			string strPrevDefault = m_saveDatabaseFile.FileName;
			m_saveDatabaseFile.Title = KPRes.CreateNewDatabase;
			m_saveDatabaseFile.FileName = KPRes.NewDatabaseKDBFileName;

			GlobalWindowManager.AddDialog(m_saveDatabaseFile);
			DialogResult dr = m_saveDatabaseFile.ShowDialog();
			GlobalWindowManager.RemoveDialog(m_saveDatabaseFile);

			string strPath = m_saveDatabaseFile.FileName;
			m_saveDatabaseFile.FileName = strPrevDefault;
			m_saveDatabaseFile.Title = strPrevTitle;

			if(dr != DialogResult.OK) return;

			KeyCreationForm kcf = new KeyCreationForm();
			kcf.InitEx(true, strPath);
			dr = kcf.ShowDialog();
			if((dr == DialogResult.Cancel) || (dr == DialogResult.Abort)) return;

			m_pwDatabase.New(IOConnectionInfo.FromPath(strPath),
				kcf.CompositeKey);

			DatabaseSettingsForm dsf = new DatabaseSettingsForm();
			dsf.InitEx(true, m_pwDatabase);
			dr = dsf.ShowDialog();
			if((dr == DialogResult.Cancel) || (dr == DialogResult.Abort))
			{
				m_pwDatabase.Close();
				UpdateGroupList(false, null);
				UpdateEntryList(null, false);
				UpdateUIState(false);
				return;
			}

			AutoEnableVisualHiding();

			PwGroup pg = new PwGroup(true, true, KPRes.General, PwIcon.Folder);
			m_pwDatabase.RootGroup.Groups.Add(pg);
			pg.ParentGroup = m_pwDatabase.RootGroup;

			pg = new PwGroup(true, true, KPRes.WindowsOS, PwIcon.DriveWindows);
			m_pwDatabase.RootGroup.Groups.Add(pg);
			pg.ParentGroup = m_pwDatabase.RootGroup;

			pg = new PwGroup(true, true, KPRes.Network, PwIcon.NetworkServer);
			m_pwDatabase.RootGroup.Groups.Add(pg);
			pg.ParentGroup = m_pwDatabase.RootGroup;

			pg = new PwGroup(true, true, KPRes.Internet, PwIcon.World);
			m_pwDatabase.RootGroup.Groups.Add(pg);
			pg.ParentGroup = m_pwDatabase.RootGroup;

			pg = new PwGroup(true, true, KPRes.EMail, PwIcon.EMail);
			m_pwDatabase.RootGroup.Groups.Add(pg);
			pg.ParentGroup = m_pwDatabase.RootGroup;

			pg = new PwGroup(true, true, KPRes.Homebanking, PwIcon.Homebanking);
			m_pwDatabase.RootGroup.Groups.Add(pg);
			pg.ParentGroup = m_pwDatabase.RootGroup;

			PwEntry pe = new PwEntry(m_pwDatabase.RootGroup, true, true);
			pe.Strings.Set(PwDefs.TitleField, new ProtectedString(m_pwDatabase.MemoryProtection.ProtectTitle,
				KPRes.SampleEntry));
			pe.Strings.Set(PwDefs.UserNameField, new ProtectedString(m_pwDatabase.MemoryProtection.ProtectUserName,
				KPRes.UserName));
			pe.Strings.Set(PwDefs.UrlField, new ProtectedString(m_pwDatabase.MemoryProtection.ProtectUrl,
				@"http://www.somesite.com/"));
			pe.Strings.Set(PwDefs.PasswordField, new ProtectedString(m_pwDatabase.MemoryProtection.ProtectPassword,
				KPRes.Password));
			pe.Strings.Set(PwDefs.NotesField, new ProtectedString(m_pwDatabase.MemoryProtection.ProtectNotes,
				KPRes.Notes));
			pe.AutoType.Set(KPRes.TargetWindow, @"{USERNAME}{TAB}{PASSWORD}{TAB}{ENTER}");
			m_pwDatabase.RootGroup.Entries.Add(pe);

#if DEBUG
			Random r = Program.GlobalRandom;

			for(uint iSamples = 0; iSamples < 1500; ++iSamples)
			{
				pg = m_pwDatabase.RootGroup.Groups.GetAt(iSamples % 5);

				pe = new PwEntry(pg, true, true);

				pe.Strings.Set(PwDefs.TitleField, new ProtectedString(m_pwDatabase.MemoryProtection.ProtectTitle,
					Guid.NewGuid().ToString()));
				pe.Strings.Set(PwDefs.UserNameField, new ProtectedString(m_pwDatabase.MemoryProtection.ProtectUserName,
					Guid.NewGuid().ToString()));
				pe.Strings.Set(PwDefs.UrlField, new ProtectedString(m_pwDatabase.MemoryProtection.ProtectUrl,
					Guid.NewGuid().ToString()));
				pe.Strings.Set(PwDefs.PasswordField, new ProtectedString(m_pwDatabase.MemoryProtection.ProtectPassword,
					Guid.NewGuid().ToString()));
				pe.Strings.Set(PwDefs.NotesField, new ProtectedString(m_pwDatabase.MemoryProtection.ProtectNotes,
					Guid.NewGuid().ToString()));

				pe.IconID = (PwIcon)r.Next(0, (int)PwIcon.Count);

				pg.Entries.Add(pe);
			}
#endif

			UpdateGroupList(false, null);
			UpdateEntryList(null, false);
			UpdateUIState(true);
		}

		private void OnFileOpen(object sender, EventArgs e)
		{
			OpenDatabase(null, null, true);
		}

		private void OnFileClose(object sender, EventArgs e)
		{
			if(!m_pwDatabase.IsOpen) return;

			if(m_pwDatabase.Modified)
			{
				if(AppConfigEx.GetBool(AppDefs.ConfigKeys.AutoSaveOnExit))
				{
					OnFileSave(sender, e);
					if(m_pwDatabase.Modified) return;
				}
				else
				{
					string strMessage = KPRes.DatabaseModified + MessageService.NewParagraph +
						KPRes.SaveBeforeCloseQuestion;
					DialogResult dr = MessageService.Ask(strMessage,
						KPRes.SaveBeforeCloseTitle, MessageBoxButtons.YesNoCancel);

					if(dr == DialogResult.Cancel) return;
					else if(dr == DialogResult.Yes)
					{
						OnFileSave(sender, e);
						if(m_pwDatabase.Modified) return;
					}
					else if(dr == DialogResult.No) { } // Changes are lost
				}
			}

			m_pwDatabase.Close();

			m_tbQuickFind.Items.Clear();
			m_tbQuickFind.Text = "";

			if(FileClosed != null) FileClosed(sender, EventArgs.Empty);

			UpdateGroupList(false, null);
			UpdateEntryList(null, false);
			UpdateUIState(false);
		}

		private void OnFileSave(object sender, EventArgs e)
		{
			if(!m_pwDatabase.IsOpen) return;
			if(!AppPolicy.Try(AppPolicyFlag.SaveDatabase)) return;

			if((m_pwDatabase.IOConnectionInfo == null) ||
				(m_pwDatabase.IOConnectionInfo.Url.Length == 0))
			{
				OnFileSaveAs(sender, e);
				return;
			}

			if(FileSaving != null)
			{
				FileSavingEventArgs args = new FileSavingEventArgs(false);
				FileSaving(sender, args);
				if(args.Cancel) return;
			}

			ShowWarningsLogger swLogger = CreateShowWarningsLogger();
			swLogger.StartLogging(KPRes.SavingDatabase);

			bool bSuccess = true;
			try
			{
				m_pwDatabase.Save(swLogger);
			
				string strName = m_pwDatabase.IOConnectionInfo.GetDisplayName();
				m_mruList.AddItem(strName, IOConnectionInfo.SerializeToString(
					m_pwDatabase.IOConnectionInfo));
			}
			catch(Exception exSave)
			{
				bSuccess = false;
				MessageService.ShowSaveWarning(m_pwDatabase.IOConnectionInfo, exSave);
			}

			swLogger.EndLogging();

			if(this.FileSaved != null)
			{
				FileSavedEventArgs args = new FileSavedEventArgs(bSuccess);
				this.FileSaved(sender, args);
			}

			UpdateUIState(false);
		}

		private void OnFileSaveAs(object sender, EventArgs e)
		{
			SaveDatabaseAs(false, sender);
		}

		private void OnFileDbSettings(object sender, EventArgs e)
		{
			DatabaseSettingsForm dsf = new DatabaseSettingsForm();
			dsf.InitEx(false, m_pwDatabase);

			if(dsf.ShowDialog() == DialogResult.OK)
			{
				if(m_pwDatabase.MemoryProtection.AutoEnableVisualHiding)
				{
					AutoEnableVisualHiding();
					RefreshEntriesList();
				}

				UpdateUIState(true);
			}
		}

		private void OnFileChangeMasterKey(object sender, EventArgs e)
		{
			KeyCreationForm kcf = new KeyCreationForm();
			kcf.InitEx(false, m_pwDatabase.IOConnectionInfo.GetDisplayName());
			if(kcf.ShowDialog() == DialogResult.OK)
			{
				m_pwDatabase.MasterKey = kcf.CompositeKey;
				MessageService.ShowInfo(KPRes.MasterKeyChanged, KPRes.MasterKeyChangedSavePrompt);
			}

			UpdateUIState(true);
		}

		private void OnFilePrint(object sender, EventArgs e)
		{
			if(!m_pwDatabase.IsOpen) return;
			PrintGroup(m_pwDatabase.RootGroup);
		}

		private void OnFileLock(object sender, EventArgs e)
		{
			if(!IsFileLocked()) // Lock
			{
				if(!m_pwDatabase.IsOpen) return; // Nothing to lock

				if(!PrepareLock()) return; // Unable to lock

				string strIoc = IOConnectionInfo.SerializeToString(
					m_pwDatabase.IOConnectionInfo);
				Debug.Assert(strIoc != null);

				PwGroup pgSelected = GetSelectedGroup();
				if(pgSelected != null)
					m_lockedState.SelectedGroupUUID = new PwUuid(pgSelected.Uuid.UuidBytes);

				// PwEntry[] vSelectedEntries = GetSelectedEntries();
				// m_lockedState.SelectedEntries.Clear();
				// foreach(PwEntry peSelected in vSelectedEntries)
				//	m_lockedState.SelectedEntries.Add(new PwUUID(peSelected.UUID.UUIDBytes));

				TreeNode tnTop = m_tvGroups.TopNode;
				if(tnTop != null)
				{
					pgSelected = tnTop.Tag as PwGroup;
					m_lockedState.TopVisibleGroup = new PwUuid(pgSelected.Uuid.UuidBytes);
				}

				ListViewItem lviTop = m_lvEntries.TopItem;
				if(lviTop != null)
				{
					PwEntry peTop = lviTop.Tag as PwEntry;
					m_lockedState.TopVisibleEntry = new PwUuid(peTop.Uuid.UuidBytes);
				}

				OnFileClose(sender, e);
				if(m_pwDatabase.IsOpen) return;

				m_strLockedIoc = strIoc;
			}
			else // Unlock
			{
				Debug.Assert(!m_pwDatabase.IsOpen);

				OpenDatabase(m_strLockedIoc, null, false);

				if(m_pwDatabase.IsOpen)
				{
					m_strLockedIoc = string.Empty;

					PwGroup pgSelect = m_pwDatabase.RootGroup.FindGroup(m_lockedState.SelectedGroupUUID, true);
					UpdateGroupList(false, pgSelect);
					UpdateEntryList(pgSelect, false);

					TreeNode tnTop = GuiFindGroup(m_lockedState.TopVisibleGroup, null);
					if(tnTop != null) m_tvGroups.TopNode = tnTop;

					ListViewItem lviTop = GuiFindEntry(m_lockedState.TopVisibleEntry);
					if(lviTop != null) m_lvEntries.TopItem = lviTop;
				}
			}

			if(this.Visible) UpdateUIState(false);
		}

		private void OnFileExit(object sender, EventArgs e)
		{
			m_bForceExitOnce = true;
			this.Close();
		}

		private void OnHelpHomepage(object sender, EventArgs e)
		{
			WinUtil.OpenUrlInNewBrowser(PwDefs.HomepageUrl, null);
		}

		private void OnHelpDonate(object sender, EventArgs e)
		{
			WinUtil.OpenUrlInNewBrowser(PwDefs.DonationsUrl, null);
		}

		private void OnHelpContents(object sender, EventArgs e)
		{
			AppHelp.ShowHelp(null, null);
		}

		private void OnHelpCheckForUpdate(object sender, EventArgs e)
		{
			CheckForUpdate.StartAsync(PwDefs.VersionUrl, null);
		}

		private void OnHelpAbout(object sender, EventArgs e)
		{
			AboutForm abf = new AboutForm();
			abf.ShowDialog();
		}

		private void OnEntryCopyUserName(object sender, EventArgs e)
		{
			PwEntry pe = GetSelectedEntry(false);
			Debug.Assert(pe != null); if(pe == null) return;

			ClipboardUtil.CopyAndMinimize(pe.Strings.ReadSafe(PwDefs.UserNameField),
				true, AppConfigEx.GetBool(AppDefs.ConfigKeys.MinimizeAfterCopy) ? this : null);
			StartClipboardCountdown();
		}

		private void OnEntryCopyPassword(object sender, EventArgs e)
		{
			PwEntry pe = GetSelectedEntry(false);
			Debug.Assert(pe != null); if(pe == null) return;

			if(PwDefs.IsTanEntry(pe))
			{
				pe.ExpiryTime = DateTime.Now;
				pe.Expires = true;
				RefreshEntriesList();
				UpdateUIState(true);
			}

			ClipboardUtil.CopyAndMinimize(pe.Strings.ReadSafe(PwDefs.PasswordField),
				true, AppConfigEx.GetBool(AppDefs.ConfigKeys.MinimizeAfterCopy) ? this : null);
			StartClipboardCountdown();
		}

		private void OnEntryOpenUrl(object sender, EventArgs e)
		{
			PwEntry pe = GetSelectedEntry(false);
			Debug.Assert(pe != null); if(pe == null) return;

			WinUtil.OpenEntryUrl(pe);
		}

		private void OnEntrySaveAttachments(object sender, EventArgs e)
		{
			PwEntry[] vSelected = GetSelectedEntries();

			GlobalWindowManager.AddDialog(m_folderSaveAttachments);
			if(m_folderSaveAttachments.ShowDialog() == DialogResult.OK)
				EntryUtil.SaveEntryAttachments(vSelected, m_folderSaveAttachments.SelectedPath);
			GlobalWindowManager.RemoveDialog(m_folderSaveAttachments);
		}

		private void OnEntryPerformAutoType(object sender, EventArgs e)
		{
			PwEntry pe = GetSelectedEntry(false);
			if(pe != null)
			{
				try { AutoType.PerformIntoPreviousWindow(this.Handle, pe); }
				catch(Exception ex)
				{
					MessageService.ShowWarning(ex);
				}
			}
		}

		private void OnEntryAdd(object sender, EventArgs e)
		{
			PwEntryForm pForm = new PwEntryForm();

			PwGroup pg = GetSelectedGroup();
			Debug.Assert(pg != null); if(pg == null) return;

			if(pg.IsVirtual)
			{
				MessageService.ShowWarning(KPRes.GroupCannotStoreEntries,
					KPRes.SelectDifferentGroup);
				return;
			}

			PwEntry pwe = new PwEntry(pg, true, true);
			pwe.Strings.Set(PwDefs.UserNameField, new ProtectedString(
				m_pwDatabase.MemoryProtection.ProtectUserName,
				m_pwDatabase.DefaultUserName));

			if(AppConfigEx.GetBool(AppDefs.ConfigKeys.DefaultGeneratePw))
				pwe.Strings.Set(PwDefs.PasswordField, PasswordGenerator.Generate(
					new PasswordGenerationOptions(),
					m_pwDatabase.MemoryProtection.ProtectPassword, null));

			int nExpireDays = AppConfigEx.GetInt(AppDefs.ConfigKeys.DefaultExpireDays);
			if(nExpireDays >= 0)
			{
				pwe.Expires = true;
				pwe.ExpiryTime = DateTime.Now.AddDays(nExpireDays);
			}

			if((pg.IconID != PwIcon.Folder) && (pg.IconID != PwIcon.FolderOpen) &&
				(pg.IconID != PwIcon.FolderPackage))
			{
				pwe.IconID = pg.IconID; // Inherit icon from group
			}

			pForm.InitEx(pwe, PwEditMode.AddNewEntry, m_pwDatabase,
				m_ilClientIcons, false);

			if(pForm.ShowDialog() == DialogResult.OK)
			{
				pg.Entries.Add(pwe);

				UpdateEntryList(pg, true);
				UpdateUIState(true);
			}
		}

		private void OnEntryEdit(object sender, EventArgs e)
		{
			PwEntry pe = GetSelectedEntry(false);
			if(pe == null) return;

			PwEntryForm pForm = new PwEntryForm();
			pForm.InitEx(pe, PwEditMode.EditExistingEntry, m_pwDatabase,
				m_ilClientIcons, false);

			if(pForm.ShowDialog() == DialogResult.OK)
			{
				RefreshEntriesList(); // Update entry
				UpdateUIState(pForm.HasModifiedEntry);
			}
			else
			{
				RefreshEntriesList(); // Update last access time
				UpdateUIState(false);
			}
		}

		private void OnEntryDuplicate(object sender, EventArgs e)
		{
			PwGroup pg = GetSelectedGroup();
			PwEntry[] vSelected = GetSelectedEntries();

			Debug.Assert((pg != null) && (vSelected != null));
			if((pg == null) || (vSelected == null)) return;

			foreach(PwEntry pe in vSelected)
			{
				PwEntry peNew = pe.CloneDeep();
				peNew.Uuid = new PwUuid(true); // Create new UUID
				pg.Entries.Add(peNew);
			}

			UpdateEntryList(pg, false);
			m_lvEntries.EnsureVisible(m_lvEntries.Items.Count - 1);

			UpdateUIState(true);
		}

		private void OnEntryDelete(object sender, EventArgs e)
		{
			PwEntry[] vSelected = GetSelectedEntries();

			if((vSelected == null) || (vSelected.Length == 0)) return;

			string strText = KPRes.DeleteEntriesInfo + MessageService.NewParagraph +
				KPRes.DeleteEntriesQuestion;
			if(!MessageService.AskYesNo(strText, KPRes.DeleteEntriesCaption))
				return;

			DateTime dtNow = DateTime.Now;
			foreach(PwEntry pe in vSelected)
			{
				if(pe.ParentGroup == null) continue; // Can't remove

				pe.ParentGroup.Entries.Remove(pe);

				PwDeletedObject pdo = new PwDeletedObject();
				pdo.Uuid = pe.Uuid;
				pdo.DeletionTime = dtNow;
				m_pwDatabase.DeletedObjects.Add(pdo);
			}

			UpdateEntryList(null, true);
			UpdateUIState(true);
		}

		private void OnEntrySelectAll(object sender, EventArgs e)
		{
			m_bBlockEntrySelectionEvent = true;
			foreach(ListViewItem lvi in m_lvEntries.Items)
			{
				lvi.Selected = true;
			}
			m_bBlockEntrySelectionEvent = false;

			UpdateUIState(false);
		}

		private void OnFormClosing(object sender, FormClosingEventArgs e)
		{
			if(!m_bForceExitOnce) // If not executed by File-Exit
			{
				if(AppConfigEx.GetBool(AppDefs.ConfigKeys.CloseButtonMinimizes))
				{
					e.Cancel = true;
					this.WindowState = FormWindowState.Minimized;
					return;
				}
			}
			m_bForceExitOnce = false;

			OnFileClose(sender, e);
			if(m_pwDatabase.IsOpen)
			{
				e.Cancel = true;
				return;
			}

			CleanUpEx(); // Saves configuration and cleans up all resources

			if(m_bRestart) WinUtil.Restart();
		}

		private void OnGroupsListClickNode(object sender, TreeNodeMouseClickEventArgs e)
		{
			if(e.Button == MouseButtons.Right)
			{
				m_tvGroups.SelectedNode = e.Node;
				return;
			}

			if(e.Button != MouseButtons.Left) return;

			TreeNode tn = e.Node;
			if((tn != null) && (tn.Tag != null))
			{
				PwGroup pg = (PwGroup)tn.Tag;
				Debug.Assert(pg != null); if(pg == null) return;
				if(pg != m_pwDatabase.RootGroup) { Debug.Assert(pg.ParentGroup != null); }

				pg.Touch(false);
				UpdateEntryList(pg, true);
				UpdateUIState(false);
			}
		}

		private void OnMenuChangeLanguage(object sender, EventArgs e)
		{
			LanguageForm lf = new LanguageForm();
			DialogResult dr = lf.ShowDialog();

			if(dr == DialogResult.OK)
			{
				string str = KPRes.LanguageSelected + MessageService.NewParagraph +
					KPRes.RestartKeePassQuestion;

				if(MessageService.AskYesNo(str))
				{
					m_bRestart = true;
					OnFileExit(sender, e);
				}
			}
		}

		private void OnShowAllEntries(object sender, EventArgs e)
		{
			PerformQuickFind("", KPRes.AllEntriesTitle);
		}

		private void OnPwListFind(object sender, EventArgs e)
		{
			SearchForm sf = new SearchForm();

			sf.InitEx(m_pwDatabase.RootGroup);
			if(sf.ShowDialog() == DialogResult.OK)
			{
				PwGroup pg = sf.SearchResultsGroup;
				UpdateEntryList(pg, false);
				UpdateUIState(false);
			}
		}

		private void OnViewShowToolBar(object sender, EventArgs e)
		{
			bool b = m_menuViewShowToolBar.Checked;

			AppConfigEx.SetValue(AppDefs.ConfigKeys.ShowToolBar, b);
			m_toolMain.Visible = b;
		}

		private void OnViewShowEntryView(object sender, EventArgs e)
		{
			ShowEntryView(m_menuViewShowEntryView.Checked);
		}

		private void OnPwListSelectedIndexChanged(object sender, EventArgs e)
		{
			if(m_bBlockEntrySelectionEvent) return;
			UpdateUIState(false);
		}

		private void OnPwListMouseDoubleClick(object sender, MouseEventArgs e)
		{
			ListViewHitTestInfo lvHit = m_lvEntries.HitTest(e.Location);
			ListViewItem lvi = lvHit.Item;

			if(lvHit.SubItem != null)
			{
				int i = 0;
				foreach(ListViewItem.ListViewSubItem lvs in lvi.SubItems)
				{
					if(lvs == lvHit.SubItem)
					{
						if(i < (int)AppDefs.ColumnID.Count)
							PerformDefaultAction(sender, e, (PwEntry)lvi.Tag, (AppDefs.ColumnID)i);
						break;
					}

					++i;
				}
			}
			else PerformDefaultAction(sender, e, (PwEntry)lvi.Tag, AppDefs.ColumnID.Title);
		}

		private void OnQuickFindSelectedIndexChanged(object sender, EventArgs e)
		{
			if(m_bBlockQuickFind) return;

			string strSearch = m_tbQuickFind.Text;
			string strGroupName = KPRes.SearchGroupName + " (\"" + strSearch + "\" ";
			strGroupName += KPRes.SearchResultsInSeparator + " ";
			strGroupName += m_pwDatabase.RootGroup.Name + ")";

			PerformQuickFind(strSearch, strGroupName);

			// Lookup in combobox for the current search
			int nExistsAlready = -1;
			for(int i = 0; i < m_tbQuickFind.Items.Count; i++)
			{
				string strItemText = (string)m_tbQuickFind.Items[i];
				if(strItemText.Equals(strSearch, StringComparison.InvariantCultureIgnoreCase))
				{
					nExistsAlready = i;
					break;
				}
			}

			// Update the history items in the combobox
			bool bDoSetText = false;
			if(nExistsAlready >= 0)
			{
				m_tbQuickFind.Items.RemoveAt(nExistsAlready);
				bDoSetText = true;
			}
			else if(m_tbQuickFind.Items.Count >= 8)
				m_tbQuickFind.Items.RemoveAt(m_tbQuickFind.Items.Count - 1);

			m_tbQuickFind.Items.Insert(0, strSearch);

			if(bDoSetText)
			{
				m_bBlockQuickFind = true;
				m_tbQuickFind.Text = strSearch;
				m_bBlockQuickFind = false;
			}
		}

		private void OnQuickFindKeyDown(object sender, KeyEventArgs e)
		{
			if((e.KeyCode == Keys.Return) || (e.KeyCode == Keys.Enter))
			{
				OnQuickFindSelectedIndexChanged(sender, e);

				e.Handled = true;
				e.SuppressKeyPress = true;
			}
		}

		private void OnQuickFindKeyUp(object sender, KeyEventArgs e)
		{
			if((e.KeyCode == Keys.Return) || (e.KeyCode == Keys.Enter))
			{
				e.Handled = true;
				e.SuppressKeyPress = true;
			}
		}

		private void OnToolsOptions(object sender, EventArgs e)
		{
			string strCurFont = AppConfigEx.GetValue(AppDefs.ConfigKeys.ListFont);

			OptionsForm ofDlg = new OptionsForm();
			ofDlg.InitEx(m_ilClientIcons);

			if(ofDlg.ShowDialog() == DialogResult.OK)
			{
				m_nLockTimerMax = (int)AppConfigEx.GetUInt(AppDefs.ConfigKeys.LockAfterTime);
				m_nClipClearMax = AppConfigEx.GetInt(AppDefs.ConfigKeys.ClipboardAutoClearTime);

				m_lvEntries.GridLines = AppConfigEx.GetBool(AppDefs.ConfigKeys.ShowGridLines);

				string strNewFont = AppConfigEx.GetValue(AppDefs.ConfigKeys.ListFont);
				if(strCurFont != strNewFont)
					SetListFont(strNewFont);

				AppConfigEx.Save();
				UpdateTrayIcon();
			}

			UpdateUIState(false);
		}

		private void OnClickHideTitles(object sender, EventArgs e)
		{
			m_viewHideFields.ProtectTitle = m_menuViewHideTitles.Checked;
			RefreshEntriesList();
			UpdateUIState(false);
		}

		private void OnClickHideUserNames(object sender, EventArgs e)
		{
			m_viewHideFields.ProtectUserName = m_menuViewHideUserNames.Checked;
			RefreshEntriesList();
			UpdateUIState(false);
		}

		private void OnClickHidePasswords(object sender, EventArgs e)
		{
			m_viewHideFields.ProtectPassword = m_menuViewHidePasswords.Checked;
			RefreshEntriesList();
			UpdateUIState(false);
		}

		private void OnClickHideURLs(object sender, EventArgs e)
		{
			m_viewHideFields.ProtectUrl = m_menuViewHideURLs.Checked;
			RefreshEntriesList();
			UpdateUIState(false);
		}

		private void OnClickHideNotes(object sender, EventArgs e)
		{
			m_viewHideFields.ProtectNotes = m_menuViewHideNotes.Checked;
			RefreshEntriesList();
			UpdateUIState(false);
		}

		private void OnPwListItemDrag(object sender, ItemDragEventArgs e)
		{
			if(e.Item == null) return;
			PwEntry pe = ((ListViewItem)e.Item).Tag as PwEntry;
			if(pe == null) { Debug.Assert(false); return; }

			ListViewHitTestInfo lvHit = m_lvEntries.HitTest(m_ptLastEntriesMouseClick);
			ListViewItem lvi = lvHit.Item;
			if(lvi == null) return;

			string strText = string.Empty;
			if(!AppPolicy.IsAllowed(AppPolicyFlag.DragDrop))
				strText = AppPolicy.RequiredPolicyMessage(AppPolicyFlag.DragDrop);
			else
			{
				int i = 0;
				foreach(ListViewItem.ListViewSubItem lvs in lvi.SubItems)
				{
					if((lvs == lvHit.SubItem) && (i < (int)AppDefs.ColumnID.Count))
					{
						AppDefs.ColumnID cID = (AppDefs.ColumnID)i;

						switch(cID)
						{
							case AppDefs.ColumnID.Title:
								strText = pe.Strings.ReadSafe(PwDefs.TitleField);
								break;
							case AppDefs.ColumnID.UserName:
								strText = pe.Strings.ReadSafe(PwDefs.UserNameField);
								break;
							case AppDefs.ColumnID.Password:
								strText = pe.Strings.ReadSafe(PwDefs.PasswordField);
								break;
							case AppDefs.ColumnID.Url:
								strText = pe.Strings.ReadSafe(PwDefs.UrlField);
								break;
							case AppDefs.ColumnID.Notes:
								strText = pe.Strings.ReadSafe(PwDefs.NotesField);
								break;
							default:
								strText = lvs.Text;
								break;
						}

						break;
					}

					++i;
				}
			}

			m_bDraggingEntries = true;
			this.DoDragDrop(strText, DragDropEffects.Copy | DragDropEffects.Move);
			m_bDraggingEntries = false;
		}

		private void OnGroupsListItemDrag(object sender, ItemDragEventArgs e)
		{
			TreeNode tn = (TreeNode)e.Item;
			if(tn == null) return;

			PwGroup pg = (PwGroup)tn.Tag;
			if(pg == null) { Debug.Assert(false); return; }

			if(pg == m_pwDatabase.RootGroup) return;
			if(pg.ParentGroup == null) return;

			this.DoDragDrop(pg, DragDropEffects.Copy | DragDropEffects.Move);
		}

		private void OnGroupsListDragDrop(object sender, DragEventArgs e)
		{
			TreeViewHitTestInfo tvhi = m_tvGroups.HitTest(m_tvGroups.PointToClient(new Point(e.X, e.Y)));

			if(tvhi.Node == null) return;
			PwGroup pgSelected = tvhi.Node.Tag as PwGroup;
			Debug.Assert(pgSelected != null); if(pgSelected == null) return;

			if(m_bDraggingEntries)
			{
				PwEntry[] vSelected = GetSelectedEntries();

				if((vSelected == null) || (vSelected.Length == 0))
					return;

				if(e.Effect == DragDropEffects.Move)
				{
					foreach(PwEntry pe in vSelected)
					{
						PwGroup pgParent = pe.ParentGroup;
						if(pgParent == pgSelected) continue;

						if(pgParent != null) // Remove from parent
						{
							if(!pgParent.Entries.Remove(pe)) { Debug.Assert(false); }
						}

						pe.ParentGroup = pgSelected; // Update parent link
						pgSelected.Entries.Add(pe); // Assign to new group
					}
				}
				else if(e.Effect == DragDropEffects.Copy)
				{
					foreach(PwEntry pe in vSelected)
					{
						PwEntry peCopy = pe.CloneDeep();
						peCopy.Uuid = new PwUuid(true); // Create new UUID

						peCopy.ParentGroup = pgSelected; // Update parent link
						pgSelected.Entries.Add(peCopy); // Assign to new group
					}
				}
				else { Debug.Assert(false); }

				UpdateUIState(true, true);
			}
			else if(e.Data.GetDataPresent(typeof(PwGroup)))
			{
				PwGroup pgDragged = e.Data.GetData(typeof(PwGroup)) as PwGroup;
				Debug.Assert(pgDragged != null);

				if((pgDragged == null) || (pgDragged == pgSelected))
				{
					UpdateUIState(false, true);
					return;
				}

				if(e.Effect == DragDropEffects.Move)
				{
					PwGroup pgParent = pgDragged.ParentGroup;
					Debug.Assert(pgParent != null);
					if(pgParent != null)
					{
						if(!pgParent.Groups.Remove(pgDragged)) { Debug.Assert(false); }
					}
				}
				else if(e.Effect == DragDropEffects.Copy)
				{
					pgDragged = pgDragged.CloneDeep();
				}
				else { Debug.Assert(false); }

				pgDragged.ParentGroup = pgSelected; // Update parent link
				pgSelected.Groups.Add(pgDragged); // Assign to new group

				pgSelected.IsExpanded = true;

				UpdateGroupList(true, pgDragged);
				UpdateEntryList(null, true);
				UpdateUIState(true);
			}
		}

		private void OnGroupsListDragEnter(object sender, DragEventArgs e)
		{
			OnGroupsListDragOver(sender, e);
		}

		private void OnGroupsListDragOver(object sender, DragEventArgs e)
		{
			if(m_bDraggingEntries || e.Data.GetDataPresent(typeof(PwGroup)))
			{
				Point pt = m_tvGroups.PointToClient(new Point(e.X, e.Y));
				TreeNode tn = m_tvGroups.GetNodeAt(pt);

				if(tn == null) e.Effect = DragDropEffects.None;
				else
				{
					if((e.KeyState & 8) != 0) e.Effect = DragDropEffects.Copy;
					else e.Effect = DragDropEffects.Move;

					m_tvGroups.SelectedNode = tn;
				}
			}
			else // No known format
				e.Effect = DragDropEffects.None;
		}

		private void OnGroupsAfterLabelEdit(object sender, NodeLabelEditEventArgs e)
		{
			if(e.Node == null) return;
			PwGroup pg = (PwGroup)e.Node.Tag;

			if((pg != null) && (e.Label != null))
			{
				pg.Name = e.Label;
				UpdateUIState(true);
			}
		}

		private void OnGroupsAdd(object sender, EventArgs e)
		{
			TreeNode tn = m_tvGroups.SelectedNode;
			PwGroup pgParent;

			if(tn != null) pgParent = tn.Tag as PwGroup;
			else pgParent = m_pwDatabase.RootGroup;

			if(pgParent == null) { Debug.Assert(false); return; }

			PwGroup pgNew = new PwGroup(true, true, KPRes.NewGroup, PwIcon.Folder);
			pgNew.ParentGroup = pgParent;
			pgParent.Groups.Add(pgNew);
			pgParent.IsExpanded = true;

			UpdateGroupList(true, pgNew);
			UpdateEntryList(null, false);
			UpdateUIState(true);

			TreeNode tnNew = m_tvGroups.SelectedNode;
			if(tnNew != null) tnNew.BeginEdit();
		}

		private void OnGroupsDelete(object sender, EventArgs e)
		{
			PwGroup pg = GetSelectedGroup();
			if(pg == null) { Debug.Assert(false); return; }

			PwGroup pgParent = pg.ParentGroup;
			if(pgParent != null)
			{
				string strText = KPRes.DeleteGroupInfo + MessageService.NewParagraph +
					KPRes.DeleteGroupQuestion;

				if(!MessageService.AskYesNo(strText, KPRes.DeleteGroupCaption))
					return;

				pgParent.Groups.Remove(pg);

				PwDeletedObject pdo = new PwDeletedObject();
				pdo.Uuid = pg.Uuid;
				pdo.DeletionTime = DateTime.Now;
				m_pwDatabase.DeletedObjects.Add(pdo);

				UpdateGroupList(true, null);
				UpdateEntryList(null, true);
				UpdateUIState(true);
			}
		}

		private void OnGroupsAfterCollapse(object sender, TreeViewEventArgs e)
		{
			TreeNode tn = e.Node;
			if(tn == null) return;

			PwGroup pg = tn.Tag as PwGroup;
			if(pg == null) { Debug.Assert(false); return; }

			pg.IsExpanded = false;
		}

		private void OnGroupsAfterExpand(object sender, TreeViewEventArgs e)
		{
			TreeNode tn = e.Node;
			if(tn == null) return;

			PwGroup pg = tn.Tag as PwGroup;
			if(pg == null) { Debug.Assert(false); return; }

			pg.IsExpanded = true;
		}

		private void OnFileSynchronize(object sender, EventArgs e)
		{
			bool bSuccess = ImportUtil.Synchronize(m_pwDatabase, this);
			UpdateUIState(false, true);
			SetStatusEx(bSuccess ? KPRes.SyncSuccess : KPRes.SyncFailed);
		}

		private void OnMenuFileExportXml(object sender, EventArgs e)
		{
			PerformExport(KeeExportFormat.PlainXml);
		}

		private void OnMenuFileExportHTML(object sender, EventArgs e)
		{
			PerformExport(KeeExportFormat.Html);
		}

		private void OnViewAlwaysOnTop(object sender, EventArgs e)
		{
			bool bTop = m_menuViewAlwaysOnTop.Checked;

			AppConfigEx.SetValue(AppDefs.ConfigKeys.AlwaysOnTop, bTop);
			this.TopMost = bTop;
		}

		private void OnGroupsPrint(object sender, EventArgs e)
		{
			if(!m_pwDatabase.IsOpen) return;
			PrintGroup(GetSelectedGroup());
		}

		private void OnViewShowColTitle(object sender, EventArgs e)
		{
			ShowColumn(AppDefs.ColumnID.Title, m_menuViewColumnsShowTitle.Checked);
		}

		private void OnViewShowColUser(object sender, EventArgs e)
		{
			ShowColumn(AppDefs.ColumnID.UserName, m_menuViewColumnsShowUserName.Checked);
		}

		private void OnViewShowColPassword(object sender, EventArgs e)
		{
			ShowColumn(AppDefs.ColumnID.Password, m_menuViewColumnsShowPassword.Checked);
		}

		private void OnViewShowColURL(object sender, EventArgs e)
		{
			ShowColumn(AppDefs.ColumnID.Url, m_menuViewColumnsShowUrl.Checked);
		}

		private void OnViewShowColNotes(object sender, EventArgs e)
		{
			ShowColumn(AppDefs.ColumnID.Notes, m_menuViewColumnsShowNotes.Checked);
		}

		private void OnViewShowColCreation(object sender, EventArgs e)
		{
			ShowColumn(AppDefs.ColumnID.CreationTime, m_menuViewColumnsShowCreation.Checked);
		}

		private void OnViewShowColLastAccess(object sender, EventArgs e)
		{
			ShowColumn(AppDefs.ColumnID.LastAccessTime, m_menuViewColumnsShowCreation.Checked);
		}

		private void OnViewShowColLastMod(object sender, EventArgs e)
		{
			ShowColumn(AppDefs.ColumnID.LastModificationTime, m_menuViewColumnsShowLastMod.Checked);
		}

		private void OnViewShowColExpire(object sender, EventArgs e)
		{
			ShowColumn(AppDefs.ColumnID.ExpiryTime, m_menuViewColumnsShowExpire.Checked);
		}

		private void OnViewShowColUUID(object sender, EventArgs e)
		{
			ShowColumn(AppDefs.ColumnID.Uuid, m_menuViewColumnsShowUuid.Checked);
		}

		private void OnViewShowColAttachs(object sender, EventArgs e)
		{
			ShowColumn(AppDefs.ColumnID.Attachment, m_menuViewColumnsShowAttachs.Checked);
		}

		private void OnPwListColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
		{
			if(m_bDisableBlockingColumnSizing) return;

			m_bDisableBlockingColumnSizing = true;

			if(!m_vShowColumns[e.ColumnIndex])
				e.Cancel = true;

			m_bDisableBlockingColumnSizing = false;
		}

		private void OnPwListColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
		{
			if(m_bDisableBlockingColumnSizing) return;

			m_bDisableBlockingColumnSizing = true;

			if(!m_vShowColumns[e.ColumnIndex])
			{
				m_lvEntries.Columns[e.ColumnIndex].Width = 0; // Restore size
			}
			else if(m_lvEntries.Columns[e.ColumnIndex].Width == 0)
			{
				ShowColumn((AppDefs.ColumnID)e.ColumnIndex, false);
			}

			m_bDisableBlockingColumnSizing = false;
		}

		private void OnFormResize(object sender, EventArgs e)
		{
			FormWindowState ws = this.WindowState;

			if(ws == FormWindowState.Minimized)
			{
				// For default value, also see options dialog
				if(AppConfigEx.GetBool(AppDefs.ConfigKeys.LockOnMinimize))
					if(IsFileLocked() == false) // Not locked currently
						OnFileLock(sender, e);

				if(AppConfigEx.GetBool(AppDefs.ConfigKeys.MinimizeToTray))
					OnTrayTray(sender, e); // Send to tray
			}
			else if(ws == FormWindowState.Maximized)
				AppConfigEx.SetValue(AppDefs.ConfigKeys.MainWindowMaximized, true);
			else if(ws == FormWindowState.Normal)
			{
				AppConfigEx.SetValue(AppDefs.ConfigKeys.MainWindowMaximized, false);

				if((m_fwsLast == FormWindowState.Minimized) && IsFileLocked())
					OnFileLock(sender, e);
			}

			m_fwsLast = ws;
		}

		private void OnTrayTray(object sender, EventArgs e)
		{
			bool bTrayed = IsTrayed();
			
			this.Visible = bTrayed; // Swap visible flag

			if(bTrayed) // If the window was trayed
			{
				if(AppConfigEx.GetBool(AppDefs.ConfigKeys.LockOnMinimize))
					OnFileLock(sender, e); // Unlock

				if(this.WindowState == FormWindowState.Minimized)
					this.WindowState = AppConfigEx.GetBool(AppDefs.ConfigKeys.MainWindowMaximized) ?
						FormWindowState.Maximized : FormWindowState.Normal;
			}

			UpdateTrayIcon();
		}

		private void OnTimerMainTick(object sender, EventArgs e)
		{
			if(m_nClipClearCur > 0)
			{
				--m_nClipClearCur;
				UpdateClipboardStatus();
			}
			else if(m_nClipClearCur == 0)
			{
				m_nClipClearCur = -1;
				UpdateClipboardStatus();

				ClipboardUtil.ClearIfOwner();
				UpdateUIState(false);
			}

			if(!GlobalWindowManager.CanCloseAllWindows) return;
			if((m_nLockTimerMax > 0) && m_bAllowLockTimerMod)
			{
				--m_nLockTimerCur;
				if(m_nLockTimerCur < 0) m_nLockTimerCur = 0;

				if((m_nLockTimerCur == 0) && m_pwDatabase.IsOpen && !IsFileLocked())
				{
					m_bAllowLockTimerMod = false;
					OnFileLock(sender, e);
					NotifyUserActivity();
					m_bAllowLockTimerMod = true;
				}
			}
		}

		private void OnToolsPlugins(object sender, EventArgs e)
		{
			if(!AppPolicy.Try(AppPolicyFlag.Plugins)) return;

			PluginsForm pf = new PluginsForm();
			pf.InitEx(m_pluginManager);

			pf.ShowDialog();
		}

		private void OnGroupsEdit(object sender, EventArgs e)
		{
			PwGroup pg = GetSelectedGroup();
			Debug.Assert(pg != null); if(pg == null) return;

			GroupForm gf = new GroupForm();
			gf.InitEx(pg, m_ilClientIcons, m_pwDatabase);

			if(gf.ShowDialog() == DialogResult.OK)
			{
				UpdateGroupList(true, null);
				UpdateEntryList(null, true);
				UpdateUIState(true);
			}
		}

		private void OnEntryCopyURL(object sender, EventArgs e)
		{
			PwEntry pe = GetSelectedEntry(false);
			Debug.Assert(pe != null); if(pe == null) return;

			ClipboardUtil.CopyAndMinimize(pe.Strings.ReadSafe(PwDefs.UrlField), true,
				AppConfigEx.GetBool(AppDefs.ConfigKeys.MinimizeAfterCopy) ? this : null);
			StartClipboardCountdown();
		}

		private void OnEntryMassSetIcon(object sender, EventArgs e)
		{
			PwEntry[] vEntries = GetSelectedEntries();
			Debug.Assert(vEntries != null); if(vEntries == null) return;

			if(vEntries.Length == 0) return;

			IconPickerForm ipf = new IconPickerForm();
			ipf.InitEx(m_ilClientIcons, m_pwDatabase, (uint)vEntries[0].IconID,
				vEntries[0].CustomIconUuid);

			if(ipf.ShowDialog() == DialogResult.OK)
			{
				foreach(PwEntry pe in vEntries)
				{
					if(ipf.ChosenCustomIconUuid != PwUuid.Zero)
						pe.CustomIconUuid = ipf.ChosenCustomIconUuid;
					else
					{
						pe.IconID = (PwIcon)ipf.ChosenIconID;
						pe.CustomIconUuid = PwUuid.Zero;
					}
				}

				RefreshEntriesList();
				UpdateUIState(true);
			}
		}

		private void OnEntrySortTitle(object sender, EventArgs e)
		{
			SortPasswordList(true, (int)AppDefs.ColumnID.Title, true);
		}

		private void OnEntrySortUserName(object sender, EventArgs e)
		{
			SortPasswordList(true, (int)AppDefs.ColumnID.UserName, true);
		}

		private void OnEntrySortPassword(object sender, EventArgs e)
		{
			SortPasswordList(true, (int)AppDefs.ColumnID.Password, true);
		}

		private void OnEntrySortURL(object sender, EventArgs e)
		{
			SortPasswordList(true, (int)AppDefs.ColumnID.Url, true);
		}

		private void OnEntrySortNotes(object sender, EventArgs e)
		{
			SortPasswordList(true, (int)AppDefs.ColumnID.Notes, true);
		}

		private void OnEntrySortCreationTime(object sender, EventArgs e)
		{
			SortPasswordList(true, (int)AppDefs.ColumnID.CreationTime, true);
		}

		private void OnEntrySortLastMod(object sender, EventArgs e)
		{
			SortPasswordList(true, (int)AppDefs.ColumnID.LastModificationTime, true);
		}

		private void OnEntrySortLastAccess(object sender, EventArgs e)
		{
			SortPasswordList(true, (int)AppDefs.ColumnID.LastAccessTime, true);
		}

		private void OnEntrySortExpiration(object sender, EventArgs e)
		{
			SortPasswordList(true, (int)AppDefs.ColumnID.ExpiryTime, true);
		}

		private void OnEntrySortUnsorted(object sender, EventArgs e)
		{
			SortPasswordList(false, 0, true);
		}

		private void OnEntryMoveToTop(object sender, EventArgs e)
		{
			MoveSelectedEntries(2);
		}

		private void OnEntryMoveOneUp(object sender, EventArgs e)
		{
			MoveSelectedEntries(1);
		}

		private void OnEntryMoveOneDown(object sender, EventArgs e)
		{
			MoveSelectedEntries(-1);
		}

		private void OnEntryMoveToBottom(object sender, EventArgs e)
		{
			MoveSelectedEntries(-2);
		}

		private void OnPwListColumnClick(object sender, ColumnClickEventArgs e)
		{
			SortPasswordList(true, e.Column, true);
		}

		private void OnPwListKeyDown(object sender, KeyEventArgs e)
		{
			bool bUnhandled = false;

			if(e.Control)
			{
				switch(e.KeyCode)
				{
					case Keys.A: OnEntrySelectAll(sender, e); break;
					case Keys.C: OnEntryCopyPassword(sender, e); break;
					case Keys.B: OnEntryCopyUserName(sender, e); break;
					case Keys.E: OnEntryEdit(sender, e); break;
					case Keys.U: OnEntryOpenUrl(sender, e); break;
					case Keys.V: OnEntryPerformAutoType(sender, e); break;
					default: bUnhandled = true; break;
				}
			}
			else if(e.Alt)
			{
				switch(e.KeyCode)
				{
					case Keys.Home: OnEntryMoveToTop(sender, e); break;
					case Keys.Up: OnEntryMoveOneUp(sender, e); break;
					case Keys.Down: OnEntryMoveOneDown(sender, e); break;
					case Keys.End: OnEntryMoveToBottom(sender, e); break;
					default: bUnhandled = true; break;
				}
			}
			else if(e.KeyCode == Keys.Delete)
				OnEntryDelete(sender, e);
			else bUnhandled = true;

			if(!bUnhandled) e.Handled = true;
		}

		private void OnGroupsFind(object sender, EventArgs e)
		{
			PwGroup pg = GetSelectedGroup();
			Debug.Assert(pg != null); if(pg == null) return;

			SearchForm sf = new SearchForm();
			sf.InitEx(pg);

			if(sf.ShowDialog() == DialogResult.OK)
			{
				PwGroup pgResults = sf.SearchResultsGroup;
				UpdateEntryList(pgResults, false);
				UpdateUIState(false);
			}
		}

		private void OnMenuFileExportKdb3(object sender, EventArgs e)
		{
			PerformExport(KeeExportFormat.Kdb3);
		}

		private void OnViewTanSimpleListClick(object sender, EventArgs e)
		{
			m_bSimpleTanView = m_menuViewTanSimpleList.Checked;
			UpdateEntryList(null, true);
		}

		private void OnViewTanIndicesClick(object sender, EventArgs e)
		{
			m_bShowTanIndices = m_menuViewTanIndices.Checked;
			UpdateEntryList(null, true);
		}

		private void OnMenuFileExportUseXsl(object sender, EventArgs e)
		{
			PerformExport(KeeExportFormat.UseXsl);
		}

		private void OnToolsPwGenerator(object sender, EventArgs e)
		{
			PwGeneratorForm pgf = new PwGeneratorForm();

			pgf.InitEx(null, m_pwDatabase.IsOpen);
			if(pgf.ShowDialog() == DialogResult.OK)
			{
				if(m_pwDatabase.IsOpen)
				{
					PwGroup pg = GetSelectedGroup();
					if(pg == null) pg = m_pwDatabase.RootGroup;

					PwEntry pe = new PwEntry(pg, true, true);
					pg.Entries.Add(pe);

					byte[] pbAdditionalEntropy = EntropyForm.CollectEntropyIfEnabled(
						pgf.SelectedOptions);
					pe.Strings.Set(PwDefs.PasswordField, PasswordGenerator.Generate(
						pgf.SelectedOptions,
						m_pwDatabase.MemoryProtection.ProtectPassword, pbAdditionalEntropy));

					UpdateEntryList(null, true);
				}
			}
		}

		private void OnToolsShowExpired(object sender, EventArgs e)
		{
			ShowExpiredEntries(false, 0);
		}

		private void OnToolsTanWizard(object sender, EventArgs e)
		{
			if(!m_pwDatabase.IsOpen) { Debug.Assert(false); return; }

			PwGroup pgSelected = GetSelectedGroup();
			if(pgSelected == null) return;

			TanWizardForm twf = new TanWizardForm();
			twf.InitEx(m_pwDatabase, pgSelected);

			if(twf.ShowDialog() == DialogResult.OK)
			{
				UpdateEntryList(null, true);
				UpdateUIState(true);
			}
		}

		private void OnSystemTrayClick(object sender, EventArgs e)
		{
			if(AppConfigEx.GetBool(AppDefs.ConfigKeys.SingleClickForTrayAction))
				OnTrayTray(sender, e);
		}

		private void OnSystemTrayDoubleClick(object sender, EventArgs e)
		{
			if(!AppConfigEx.GetBool(AppDefs.ConfigKeys.SingleClickForTrayAction))
				OnTrayTray(sender, e);
		}

		private void OnEntryViewLinkClicked(object sender, LinkClickedEventArgs e)
		{
			PwEntry pe = GetSelectedEntry(false);
			WinUtil.OpenUrlInNewBrowser(e.LinkText, pe);
		}

		private void OnEntryClipCopy(object sender, EventArgs e)
		{
			if(m_pwDatabase.IsOpen == false) return;
			PwEntry[] vSelected = GetSelectedEntries();
			if(vSelected == null) return;

			try { EntryUtil.CopyEntriesToClipboard(m_pwDatabase, vSelected); }
			catch(Exception exCopy)
			{
				MessageService.ShowWarning(exCopy);
			}

			StartClipboardCountdown();
		}

		private void OnEntryClipPaste(object sender, EventArgs e)
		{
			if(m_pwDatabase.IsOpen == false) return;
			PwGroup pg = GetSelectedGroup();
			if(pg == null) return;

			try { EntryUtil.PasteEntriesFromClipboard(m_pwDatabase, pg); }
			catch(Exception exPaste)
			{
				MessageService.ShowWarning(exPaste);
			}

			UpdateEntryList(null, true);
			UpdateUIState(true);
		}

		private void OnEntryColorStandard(object sender, EventArgs e)
		{
			SetSelectedEntryColor(Color.Empty);
		}

		private void OnEntryColorLightRed(object sender, EventArgs e)
		{
			SetSelectedEntryColor(AppDefs.NamedEntryColor.LightRed);
		}

		private void OnEntryColorLightGreen(object sender, EventArgs e)
		{
			SetSelectedEntryColor(AppDefs.NamedEntryColor.LightGreen);
		}

		private void OnEntryColorLightBlue(object sender, EventArgs e)
		{
			SetSelectedEntryColor(AppDefs.NamedEntryColor.LightBlue);
		}

		private void OnEntryColorLightYellow(object sender, EventArgs e)
		{
			SetSelectedEntryColor(AppDefs.NamedEntryColor.LightYellow);
		}

		private void OnEntryColorCustom(object sender, EventArgs e)
		{
			PwEntry pe = GetSelectedEntry(false);
			if((pe != null) && !pe.BackgroundColor.IsEmpty)
				m_colorDlg.Color = pe.BackgroundColor;

			GlobalWindowManager.AddDialog(m_colorDlg);
			if(m_colorDlg.ShowDialog() == DialogResult.OK)
				SetSelectedEntryColor(m_colorDlg.Color);
			GlobalWindowManager.RemoveDialog(m_colorDlg);
		}

		private void OnPwListMouseDown(object sender, MouseEventArgs e)
		{
			m_ptLastEntriesMouseClick = e.Location;
		}

		private void OnToolsDbMaintenance(object sender, EventArgs e)
		{
			if(m_pwDatabase.IsOpen == false) return;

			DatabaseOperationsForm form = new DatabaseOperationsForm();
			form.InitEx(m_pwDatabase);
			form.ShowDialog();

			UpdateUIState(true);
		}

		private void OnCtxPwListOpening(object sender, CancelEventArgs e)
		{
			m_dynCustomStrings.Clear();
			m_dynCustomBinaries.Clear();

			PwEntry[] vSelected = GetSelectedEntries();
			if((vSelected == null) || (vSelected.Length != 1))
			{
				m_ctxEntryCopyCustomString.Visible = false;
				m_ctxEntryBinaries.Visible = false;
				return;
			}

			PwEntry pe = vSelected[0];

			uint uStrItems = 0;
			foreach(KeyValuePair<string, ProtectedString> kvp in pe.Strings)
			{
				if(PwDefs.IsStandardField(kvp.Key)) continue;

				m_dynCustomStrings.AddItem(kvp.Key,
					KeePass.Properties.Resources.B16x16_KGPG_Info);
				++uStrItems;
			}
			m_ctxEntryCopyCustomString.Visible = (uStrItems > 0);

			uint uBinItems = 0;
			foreach(KeyValuePair<string, ProtectedBinary> kvp in pe.Binaries)
			{
				Image imgIcon;

				// Try a fast classification
				BinaryDataClass bdc = BinaryDataClassifier.ClassifyUrl(kvp.Key);
				if(bdc == BinaryDataClass.Text)
					imgIcon = KeePass.Properties.Resources.B16x16_ASCII;
				else if(bdc == BinaryDataClass.Image)
					imgIcon = KeePass.Properties.Resources.B16x16_Spreadsheet;
				else if(bdc == BinaryDataClass.WebDocument)
					imgIcon = KeePass.Properties.Resources.B16x16_HTML;
				else imgIcon = KeePass.Properties.Resources.B16x16_Binary;

				m_dynCustomBinaries.AddItem(kvp.Key, imgIcon);
				++uBinItems;
			}
			m_ctxEntryBinaries.Visible = (uBinItems > 0);
		}

		private void OnToolsGeneratePasswordList(object sender, EventArgs e)
		{
			if(!m_pwDatabase.IsOpen) return;

			PwGeneratorForm pgf = new PwGeneratorForm();

			pgf.InitEx(null, true);
			if(pgf.ShowDialog() == DialogResult.OK)
			{
				PwGroup pg = GetSelectedGroup();
				if(pg == null) pg = m_pwDatabase.RootGroup;

				SingleLineEditForm dlgCount = new SingleLineEditForm();
				dlgCount.InitEx(KPRes.GenerateCount, KPRes.GenerateCountDesc,
					KPRes.GenerateCountLongDesc, KeePass.Properties.Resources.B48x48_KGPG_Gen);
				if(dlgCount.ShowDialog() == DialogResult.OK)
				{
					uint uCount;
					if(!uint.TryParse(dlgCount.ResultString, out uCount))
						uCount = 1;

					byte[] pbAdditionalEntropy = EntropyForm.CollectEntropyIfEnabled(
						pgf.SelectedOptions);

					for(uint i = 0; i < uCount; ++i)
					{
						PwEntry pe = new PwEntry(pg, true, true);
						pg.Entries.Add(pe);

						pe.Strings.Set(PwDefs.PasswordField, PasswordGenerator.Generate(
							pgf.SelectedOptions,
							m_pwDatabase.MemoryProtection.ProtectPassword, pbAdditionalEntropy));
					}

					UpdateEntryList(null, true);
				}
			}
		}

		private void OnViewWindowsSideBySide(object sender, EventArgs e)
		{
			SetMainWindowLayout(true);
		}

		private void OnViewWindowsStacked(object sender, EventArgs e)
		{
			SetMainWindowLayout(false);
		}

		private void OnHelpSelectSource(object sender, EventArgs e)
		{
			HelpSourceForm hsf = new HelpSourceForm();
			hsf.ShowDialog();
		}

		private void OnFileOpenUrl(object sender, EventArgs e)
		{
			OpenDatabase(null, null, false);
		}

		private void OnFileSaveAsUrl(object sender, EventArgs e)
		{
			SaveDatabaseAs(true, sender);
		}

		private void OnFileImport(object sender, EventArgs e)
		{
			bool bAppendedToRootOnly;
			ImportUtil.ImportInto(m_pwDatabase, out bAppendedToRootOnly);

			if(bAppendedToRootOnly && m_pwDatabase.IsOpen)
			{
				UpdateGroupList(true, m_pwDatabase.RootGroup);
				UpdateEntryList(null, false);
				m_lvEntries.EnsureVisible(m_lvEntries.Items.Count - 1);
				UpdateUIState(true, false);
			}
			else UpdateUIState(true, true);
		}

		private void OnGroupsMoveToTop(object sender, EventArgs e)
		{
			MoveSelectedGroup(2);
		}

		private void OnGroupsMoveOneUp(object sender, EventArgs e)
		{
			MoveSelectedGroup(1);
		}

		private void OnGroupsMoveOneDown(object sender, EventArgs e)
		{
			MoveSelectedGroup(-1);
		}

		private void OnGroupsMoveToBottom(object sender, EventArgs e)
		{
			MoveSelectedGroup(-2);
		}

		private void OnGroupsKeyDown(object sender, KeyEventArgs e)
		{
			bool bUnhandled = false;

			if(e.Alt)
			{
				switch(e.KeyCode)
				{
					case Keys.Home: OnGroupsMoveToTop(sender, e); break;
					case Keys.Up: OnGroupsMoveOneUp(sender, e); break;
					case Keys.Down: OnGroupsMoveOneDown(sender, e); break;
					case Keys.End: OnGroupsMoveToBottom(sender, e); break;
					default: bUnhandled = true; break;
				}
			}
			else if(e.KeyCode == Keys.Delete)
				OnGroupsDelete(sender, e);
			else bUnhandled = true;

			if(!bUnhandled) e.Handled = true;
		}
	}
}
