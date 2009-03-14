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
using System.Threading;
using System.Windows.Forms;
using System.Globalization;
using System.Diagnostics;
using System.Security;

using KeePass.App;
using KeePass.App.Configuration;
using KeePass.DataExchange;
using KeePass.Ecas;
using KeePass.Native;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;
using KeePass.Util.Spr;

using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Cryptography;
using KeePassLib.Cryptography.Cipher;
using KeePassLib.Cryptography.PasswordGenerator;
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
		private bool m_bRestart = false;
		private ListSorter m_pListSorter = new ListSorter();
		private bool m_bBlockQuickFind = false;

		private bool m_bDraggingEntries = false;

		private bool m_bDisableBlockingColumnSizing = false;
		private bool m_bBlockEntrySelectionEvent = false;
		private bool m_bFormLoading = false;

		private bool m_bForceExitOnce = false;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public MainForm()
		{
			string strIso6391 = Program.Translation.Properties.Iso6391Code;
			if(strIso6391.Length > 0)
			{
				try
				{
					CultureInfo ci = CultureInfo.CreateSpecificCulture(strIso6391);
					Application.CurrentCulture = ci;
					Thread.CurrentThread.CurrentCulture = ci;
					Thread.CurrentThread.CurrentUICulture = ci;
					Properties.Resources.Culture = ci;
				}
				catch(Exception) { Debug.Assert(false); }
			}

			UISelfTest();

			InitializeComponent();
			Program.Translation.ApplyTo(this);
			Program.Translation.ApplyTo("KeePass.Forms.MainForm.m_menuMain", m_menuMain.Items);
			Program.Translation.ApplyTo("KeePass.Forms.MainForm.m_ctxPwList", m_ctxPwList.Items);
			Program.Translation.ApplyTo("KeePass.Forms.MainForm.m_ctxGroupList", m_ctxGroupList.Items);
			Program.Translation.ApplyTo("KeePass.Forms.MainForm.m_ctxTray", m_ctxTray.Items);

			m_fontBoldUI = new Font(m_tabMain.Font, FontStyle.Bold);
			m_fontBoldTree = new Font(m_tvGroups.Font, FontStyle.Bold);
			m_fontItalicTree = new Font(m_tvGroups.Font, FontStyle.Italic);

			m_splitHorizontal.InitEx(this.Controls, m_menuMain);
			m_splitVertical.InitEx(this.Controls, m_menuMain);

			AssignMenuShortcuts();
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			m_bFormLoading = true;

			m_strNeverExpiresText = KPRes.NeverExpires;

			this.Icon = Properties.Resources.KeePass;
			m_imgFileSaveEnabled = Properties.Resources.B16x16_FileSave;
			m_imgFileSaveDisabled = Properties.Resources.B16x16_FileSave_Disabled;
			m_imgFileSaveAllEnabled = Properties.Resources.B16x16_File_SaveAll;
			m_imgFileSaveAllDisabled = Properties.Resources.B16x16_File_SaveAll_Disabled;
			m_ilCurrentIcons = m_ilClientIcons;

			m_nLockTimerMax = (int)Program.Config.Security.WorkspaceLocking.LockAfterTime;
			m_nClipClearMax = Program.Config.Security.ClipboardClearAfterSeconds;

			NativeLib.AllowNative = Program.Config.Native.NativeKeyTransformations;

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

			ConfigureTbButton(m_tbNewDatabase, KPRes.ToolBarNew, null);
			ConfigureTbButton(m_tbOpenDatabase, KPRes.ToolBarOpen, null);
			ConfigureTbButton(m_tbSaveDatabase, KPRes.Save, null);
			ConfigureTbButton(m_tbSaveAll, KPRes.ToolBarSaveAll, null);
			ConfigureTbButton(m_tbAddEntry, KPRes.AddEntry, null);
			ConfigureTbButton(m_tbAddEntryDefault, KPRes.AddEntryBtn, null);
			ConfigureTbButton(m_tbCopyUserName, KPRes.CopyUserFull, null);
			ConfigureTbButton(m_tbCopyPassword, KPRes.CopyPasswordFull, null);
			ConfigureTbButton(m_tbFind, KPRes.Search, null);
			ConfigureTbButton(m_tbViewsShowAll, KPRes.ShowAllEntries, null);
			ConfigureTbButton(m_tbViewsShowExpired, KPRes.ShowExpiredEntries, null);
			ConfigureTbButton(m_tbLockWorkspace, KPRes.LockMenuLock, null);
			ConfigureTbButton(m_tbQuickFind, null, KPRes.SearchQuickPrompt);

			bool bVisible = Program.Config.MainWindow.ToolBar.Show;
			m_toolMain.Visible = bVisible;
			m_menuViewShowToolBar.Checked = bVisible;

			int wndX = Program.Config.MainWindow.X;
			int wndY = Program.Config.MainWindow.Y;
			int sizeX = Program.Config.MainWindow.Width;
			int sizeY = Program.Config.MainWindow.Height;
			bool bWndValid = ((wndX != -32000) && (wndY != -32000) && (wndX != -64000) && (wndY != -64000));

			if((sizeX != AppDefs.InvalidWindowValue) &&
				(sizeY != AppDefs.InvalidWindowValue) && bWndValid)
				this.Size = new Size(sizeX, sizeY);

			Rectangle rectScreen = Screen.GetWorkingArea(this);

			if((wndX != AppDefs.InvalidWindowValue) &&
				(wndY != AppDefs.InvalidWindowValue) && bWndValid)
				this.Location = new Point(wndX, wndY);
			else
				this.Location = new Point((rectScreen.Width - this.Size.Width) / 2,
					(rectScreen.Height - this.Size.Height) / 2);

			SetMainWindowLayout(Program.Config.MainWindow.Layout == AceMainWindowLayout.SideBySide);

			ShowEntryView(Program.Config.MainWindow.EntryView.Show);

			ColumnHeader ch;
			int nDefaultWidth = m_lvEntries.ClientRectangle.Width / 5;
			AceMainWindow mw = Program.Config.MainWindow;

			m_bDisableBlockingColumnSizing = true;
			ch = m_lvEntries.Columns.Add(KPRes.Title);
			ch.Width = mw.ColumnsDict[PwDefs.TitleField].SafeGetWidth(nDefaultWidth);
			ch = m_lvEntries.Columns.Add(KPRes.UserName);
			ch.Width = mw.ColumnsDict[PwDefs.UserNameField].SafeGetWidth(nDefaultWidth);
			ch = m_lvEntries.Columns.Add(KPRes.Password);
			ch.Width = mw.ColumnsDict[PwDefs.PasswordField].SafeGetWidth(nDefaultWidth);
			ch = m_lvEntries.Columns.Add(KPRes.Url);
			ch.Width = mw.ColumnsDict[PwDefs.UrlField].SafeGetWidth(nDefaultWidth);
			ch = m_lvEntries.Columns.Add(KPRes.Notes);
			ch.Width = mw.ColumnsDict[PwDefs.NotesField].SafeGetWidth(nDefaultWidth);
			ch = m_lvEntries.Columns.Add(KPRes.CreationTime);
			ch.Width = mw.ColumnsDict[AppDefs.ColumnIdnCreationTime].SafeGetWidth(0);
			ch = m_lvEntries.Columns.Add(KPRes.LastAccessTime);
			ch.Width = mw.ColumnsDict[AppDefs.ColumnIdnLastAccessTime].SafeGetWidth(0);
			ch = m_lvEntries.Columns.Add(KPRes.LastModificationTime);
			ch.Width = mw.ColumnsDict[AppDefs.ColumnIdnLastModificationTime].SafeGetWidth(0);
			ch = m_lvEntries.Columns.Add(KPRes.ExpiryTime);
			ch.Width = mw.ColumnsDict[AppDefs.ColumnIdnExpiryTime].SafeGetWidth(0);
			ch = m_lvEntries.Columns.Add(KPRes.Uuid);
			ch.Width = mw.ColumnsDict[AppDefs.ColumnIdnUuid].SafeGetWidth(0);
			ch = m_lvEntries.Columns.Add(KPRes.Attachments);
			ch.Width = mw.ColumnsDict[AppDefs.ColumnIdnAttachment].SafeGetWidth(0);
			m_bDisableBlockingColumnSizing = false;

			RestoreDisplayIndex(mw, PwDefs.TitleField, AppDefs.ColumnId.Title);
			RestoreDisplayIndex(mw, PwDefs.UserNameField, AppDefs.ColumnId.UserName);
			RestoreDisplayIndex(mw, PwDefs.PasswordField, AppDefs.ColumnId.Password);
			RestoreDisplayIndex(mw, PwDefs.UrlField, AppDefs.ColumnId.Url);
			RestoreDisplayIndex(mw, PwDefs.NotesField, AppDefs.ColumnId.Notes);
			RestoreDisplayIndex(mw, AppDefs.ColumnIdnAttachment, AppDefs.ColumnId.Attachment);
			RestoreDisplayIndex(mw, AppDefs.ColumnIdnCreationTime, AppDefs.ColumnId.CreationTime);
			RestoreDisplayIndex(mw, AppDefs.ColumnIdnExpiryTime, AppDefs.ColumnId.ExpiryTime);
			RestoreDisplayIndex(mw, AppDefs.ColumnIdnLastAccessTime, AppDefs.ColumnId.LastAccessTime);
			RestoreDisplayIndex(mw, AppDefs.ColumnIdnLastModificationTime, AppDefs.ColumnId.LastModificationTime);
			RestoreDisplayIndex(mw, AppDefs.ColumnIdnUuid, AppDefs.ColumnId.Uuid);

			Debug.Assert(m_lvEntries.Columns.Count == (int)AppDefs.ColumnId.Count);

			for(int iCol = 0; iCol < (int)AppDefs.ColumnId.Count; ++iCol)
				m_vShowColumns[iCol] = (m_lvEntries.Columns[iCol].Width > 0);

			m_menuViewColumnsShowTitle.Checked = m_vShowColumns[(int)AppDefs.ColumnId.Title];
			m_menuViewColumnsShowUserName.Checked = m_vShowColumns[(int)AppDefs.ColumnId.UserName];
			m_menuViewColumnsShowPassword.Checked = m_vShowColumns[(int)AppDefs.ColumnId.Password];
			m_menuViewColumnsShowUrl.Checked = m_vShowColumns[(int)AppDefs.ColumnId.Url];
			m_menuViewColumnsShowNotes.Checked = m_vShowColumns[(int)AppDefs.ColumnId.Notes];
			m_menuViewColumnsShowCreation.Checked = m_vShowColumns[(int)AppDefs.ColumnId.CreationTime];
			m_menuViewColumnsShowLastAccess.Checked = m_vShowColumns[(int)AppDefs.ColumnId.LastAccessTime];
			m_menuViewColumnsShowLastMod.Checked = m_vShowColumns[(int)AppDefs.ColumnId.LastModificationTime];
			m_menuViewColumnsShowExpire.Checked = m_vShowColumns[(int)AppDefs.ColumnId.ExpiryTime];
			m_menuViewColumnsShowUuid.Checked = m_vShowColumns[(int)AppDefs.ColumnId.Uuid];
			m_menuViewColumnsShowAttachs.Checked = m_vShowColumns[(int)AppDefs.ColumnId.Attachment];

			m_menuViewHideTitles.Checked = m_viewHideFields.ProtectTitle =
				mw.ColumnsDict[PwDefs.TitleField].HideWithAsterisks;
			m_menuViewHideUserNames.Checked = m_viewHideFields.ProtectUserName =
				mw.ColumnsDict[PwDefs.UserNameField].HideWithAsterisks;
			m_menuViewHidePasswords.Checked = m_viewHideFields.ProtectPassword =
				mw.ColumnsDict[PwDefs.PasswordField].HideWithAsterisks;
			m_menuViewHideURLs.Checked = m_viewHideFields.ProtectUrl =
				mw.ColumnsDict[PwDefs.UrlField].HideWithAsterisks;
			m_menuViewHideNotes.Checked = m_viewHideFields.ProtectNotes =
				mw.ColumnsDict[PwDefs.NotesField].HideWithAsterisks;

			m_menuViewTanSimpleList.Checked = m_bSimpleTanView =
				mw.TanView.UseSimpleView;
			m_menuViewTanIndices.Checked = m_bShowTanIndices =
				mw.TanView.ShowIndices;

			m_menuViewShowEntriesOfSubGroups.Checked =
				Program.Config.MainWindow.ShowEntriesOfSubGroups;

			m_pListSorter = Program.Config.MainWindow.ListSorting;
			if((m_pListSorter.Column >= 0) && (m_pListSorter.Order != SortOrder.None))
				m_lvEntries.ListViewItemSorter = m_pListSorter;
			else m_pListSorter = new ListSorter();

			m_menuViewAlwaysOnTop.Checked = mw.AlwaysOnTop;
			OnViewAlwaysOnTop(null, null);

			m_mruList.Initialize(this, m_menuFileRecent);

			m_mruList.MaxItemCount = Program.Config.Application.MostRecentlyUsed.MaxItemCount;
			for(uint uMru = 0; uMru < (uint)Program.Config.Application.MostRecentlyUsed.Items.Count; ++uMru)
			{
				IOConnectionInfo ioMru = Program.Config.Application.MostRecentlyUsed.Items[(int)uMru];
				m_mruList.AddItem(ioMru.GetDisplayName(), ioMru.CloneDeep());
			}
			m_mruList.UpdateMenu();

			SetListFont(Program.Config.UI.StandardFont);

			m_ctxEntryColorLightRed.Image = UIUtil.CreateColorBitmap24(16, 16,
				AppDefs.NamedEntryColor.LightRed);
			m_ctxEntryColorLightGreen.Image = UIUtil.CreateColorBitmap24(16, 16,
				AppDefs.NamedEntryColor.LightGreen);
			m_ctxEntryColorLightBlue.Image = UIUtil.CreateColorBitmap24(16, 16,
				AppDefs.NamedEntryColor.LightBlue);
			m_ctxEntryColorLightYellow.Image = UIUtil.CreateColorBitmap24(16, 16,
				AppDefs.NamedEntryColor.LightYellow);

			m_lvEntries.GridLines = mw.ShowGridLines;

			m_statusPartProgress.Visible = false;

			if(mw.Maximized) this.WindowState = FormWindowState.Maximized;

			try
			{
				int nSplitPos = mw.SplitterHorizontalPosition;
				if(nSplitPos == AppDefs.InvalidWindowValue)
					nSplitPos = (m_splitHorizontal.ClientRectangle.Height * 5) / 6;
				m_splitHorizontal.SplitterDistance = nSplitPos;

				nSplitPos = mw.SplitterVerticalPosition;
				if(nSplitPos == AppDefs.InvalidWindowValue)
					nSplitPos = m_splitVertical.ClientRectangle.Width / 4;
				m_splitVertical.SplitterDistance = nSplitPos;
			}
			catch(Exception) { Debug.Assert(false); }

			string strSearchTr = ((WinUtil.IsAtLeastWindowsVista ?
				string.Empty : " ") + KPRes.Search);
			UIUtil.SetCueBanner(m_tbQuickFind, strSearchTr);

			m_sessionLockNotifier.Install(this.OnSessionLock);

			m_pluginDefaultHost.Initialize(this, Program.CommandLineArgs,
				CipherPool.GlobalPool);
			m_pluginManager.Initialize(m_pluginDefaultHost);

			m_pluginManager.UnloadAllPlugins();
			if(AppPolicy.Current.Plugins)
				m_pluginManager.LoadAllPlugins(UrlUtil.GetFileDirectory(WinUtil.GetExecutable(), false));

			HotKeyManager.ReceiverWindow = this.Handle;

			Keys kAutoTypeKey = (Keys)Program.Config.Integration.HotKeyGlobalAutoType;
			HotKeyManager.RegisterHotKey(AppDefs.GlobalHotKeyId.AutoType, kAutoTypeKey);
			Keys kShowWindowKey = (Keys)Program.Config.Integration.HotKeyShowWindow;
			HotKeyManager.RegisterHotKey(AppDefs.GlobalHotKeyId.ShowWindow, kShowWindowKey);
			Keys kEntryMenuKey = (Keys)Program.Config.Integration.HotKeyEntryMenu;
			HotKeyManager.RegisterHotKey(AppDefs.GlobalHotKeyId.EntryMenu, kEntryMenuKey);

			m_statusClipboard.Visible = false;
			UpdateClipboardStatus();

			UpdateTrayIcon();
			UpdateUIState(false);

			if(Program.CommandLineArgs.FileName != null)
				OpenDatabase(IOConnectionInfo.FromPath(Program.CommandLineArgs.FileName),
					KeyUtil.KeyFromCommandLine(Program.CommandLineArgs), false);
			else if(Program.Config.Application.Start.OpenLastFile)
			{
				IOConnectionInfo ioLastFile = Program.Config.Application.LastUsedFile;
				if(ioLastFile.Path.Length > 0)
					OpenDatabase(ioLastFile, null, false);
			}

			if(Program.Config.Application.Start.CheckForUpdate)
				CheckForUpdate.StartAsync(PwDefs.VersionUrl, m_statusPartInfo);

			ResetDefaultFocus(null);

			MinimizeToTrayAtStartIfEnabled(true);

			m_bFormLoading = false;
			Program.TriggerSystem.RaiseEvent(EcasEventIDs.AppLoadPost);
		}

		private void OnFormShown(object sender, EventArgs e)
		{
			MinimizeToTrayAtStartIfEnabled(false);
		}

		private void OnFileNew(object sender, EventArgs e)
		{
			if(!AppPolicy.Try(AppPolicyId.SaveFile)) return;

			// OnFileClose(sender, e);
			// if(m_pwDatabase.IsOpen) return;

			SaveFileDialog sfd = UIUtil.CreateSaveFileDialog(KPRes.CreateNewDatabase,
				KPRes.NewDatabaseFileName, UIUtil.CreateFileTypeFilter(
				AppDefs.FileExtension.FileExt, KPRes.KdbxFiles, true), 1,
				AppDefs.FileExtension.FileExt, false);

			GlobalWindowManager.AddDialog(sfd);
			DialogResult dr = sfd.ShowDialog();
			GlobalWindowManager.RemoveDialog(sfd);

			string strPath = sfd.FileName;

			if(dr != DialogResult.OK) return;

			KeyCreationForm kcf = new KeyCreationForm();
			kcf.InitEx(true, strPath);
			dr = kcf.ShowDialog();
			if((dr == DialogResult.Cancel) || (dr == DialogResult.Abort)) return;

			PwDocument dsPrevActive = m_docMgr.ActiveDocument;
			PwDatabase pd = m_docMgr.CreateNewDocument(true).Database;
			pd.New(IOConnectionInfo.FromPath(strPath), kcf.CompositeKey);

			DatabaseSettingsForm dsf = new DatabaseSettingsForm();
			dsf.InitEx(true, pd);
			dr = dsf.ShowDialog();
			if((dr == DialogResult.Cancel) || (dr == DialogResult.Abort))
			{
				m_docMgr.CloseDatabase(pd);
				try { m_docMgr.ActiveDocument = dsPrevActive; }
				catch(Exception) { } // Fails if no database is open now
				UpdateUI(false, null, true, null, true, null, false);
				return;
			}

			AutoEnableVisualHiding();

			PwGroup pg = new PwGroup(true, true, KPRes.General, PwIcon.Folder);
			pd.RootGroup.AddGroup(pg, true);

			pg = new PwGroup(true, true, KPRes.WindowsOS, PwIcon.DriveWindows);
			pd.RootGroup.AddGroup(pg, true);

			pg = new PwGroup(true, true, KPRes.Network, PwIcon.NetworkServer);
			pd.RootGroup.AddGroup(pg, true);

			pg = new PwGroup(true, true, KPRes.Internet, PwIcon.World);
			pd.RootGroup.AddGroup(pg, true);

			pg = new PwGroup(true, true, KPRes.EMail, PwIcon.EMail);
			pd.RootGroup.AddGroup(pg, true);

			pg = new PwGroup(true, true, KPRes.Homebanking, PwIcon.Homebanking);
			pd.RootGroup.AddGroup(pg, true);

			PwEntry pe = new PwEntry(true, true);
			pe.Strings.Set(PwDefs.TitleField, new ProtectedString(pd.MemoryProtection.ProtectTitle,
				KPRes.SampleEntry));
			pe.Strings.Set(PwDefs.UserNameField, new ProtectedString(pd.MemoryProtection.ProtectUserName,
				KPRes.UserName));
			pe.Strings.Set(PwDefs.UrlField, new ProtectedString(pd.MemoryProtection.ProtectUrl,
				@"http://www.somesite.com/"));
			pe.Strings.Set(PwDefs.PasswordField, new ProtectedString(pd.MemoryProtection.ProtectPassword,
				KPRes.Password));
			pe.Strings.Set(PwDefs.NotesField, new ProtectedString(pd.MemoryProtection.ProtectNotes,
				KPRes.Notes));
			pe.AutoType.Set(KPRes.TargetWindow, @"{USERNAME}{TAB}{PASSWORD}{TAB}{ENTER}");
			pd.RootGroup.AddEntry(pe, true);

#if DEBUG
			Random r = Program.GlobalRandom;
			for(uint iSamples = 0; iSamples < 1500; ++iSamples)
			{
				pg = pd.RootGroup.Groups.GetAt(iSamples % 5);

				pe = new PwEntry(true, true);

				pe.Strings.Set(PwDefs.TitleField, new ProtectedString(pd.MemoryProtection.ProtectTitle,
					Guid.NewGuid().ToString()));
				pe.Strings.Set(PwDefs.UserNameField, new ProtectedString(pd.MemoryProtection.ProtectUserName,
					Guid.NewGuid().ToString()));
				pe.Strings.Set(PwDefs.UrlField, new ProtectedString(pd.MemoryProtection.ProtectUrl,
					Guid.NewGuid().ToString()));
				pe.Strings.Set(PwDefs.PasswordField, new ProtectedString(pd.MemoryProtection.ProtectPassword,
					Guid.NewGuid().ToString()));
				pe.Strings.Set(PwDefs.NotesField, new ProtectedString(pd.MemoryProtection.ProtectNotes,
					Guid.NewGuid().ToString()));

				pe.IconId = (PwIcon)r.Next(0, (int)PwIcon.Count);

				pg.AddEntry(pe, true);
			}
#endif

			UpdateUI(true, null, true, null, true, null, true);

			if(this.FileCreated != null)
			{
				FileCreatedEventArgs ea = new FileCreatedEventArgs(pd);
				this.FileCreated(this, ea);
			}
		}

		private void OnFileOpen(object sender, EventArgs e)
		{
			OpenDatabase(null, null, true);
		}

		private void OnFileClose(object sender, EventArgs e)
		{
			CloseActiveDocument(false, false);
		}

		private void OnFileSave(object sender, EventArgs e)
		{
			PwDatabase pd = m_docMgr.ActiveDatabase;

			if(!pd.IsOpen) return;
			if(!AppPolicy.Try(AppPolicyId.SaveFile)) return;

			if((pd.IOConnectionInfo == null) || (pd.IOConnectionInfo.Path.Length == 0))
			{
				OnFileSaveAs(sender, e);
				return;
			}

			UIBlockInteraction(true);
			if(PreSaveValidate(pd) == false) { UIBlockInteraction(false); return; }

			Guid eventGuid = Guid.NewGuid();
			if(this.FileSaving != null)
			{
				FileSavingEventArgs args = new FileSavingEventArgs(false, false, pd, eventGuid);
				this.FileSaving(sender, args);
				if(args.Cancel) { UIBlockInteraction(false); return; }
			}
			Program.TriggerSystem.RaiseEvent(EcasEventIDs.SavingDatabaseFile,
				pd.IOConnectionInfo.Path);

			ShowWarningsLogger swLogger = CreateShowWarningsLogger();
			swLogger.StartLogging(KPRes.SavingDatabase, true);

			bool bSuccess = true;
			try
			{
				pd.Save(swLogger);

				PostSavingEx(true, pd, pd.IOConnectionInfo);
			}
			catch(Exception exSave)
			{
				bSuccess = false;
				MessageService.ShowSaveWarning(pd.IOConnectionInfo, exSave, true);
			}

			swLogger.EndLogging();

			// Immediately after the UIBlockInteraction call the form might
			// be closed and UpdateUIState might crash, if the order of the
			// two methods is swapped; so first update state, then unblock
			UpdateUIState(false);
			UIBlockInteraction(false); // Calls Application.DoEvents()

			if(this.FileSaved != null)
			{
				FileSavedEventArgs args = new FileSavedEventArgs(bSuccess, pd, eventGuid);
				this.FileSaved(sender, args);
			}
			if(bSuccess)
				Program.TriggerSystem.RaiseEvent(EcasEventIDs.SavedDatabaseFile,
					pd.IOConnectionInfo.Path);
		}

		private void OnFileSaveAs(object sender, EventArgs e)
		{
			SaveDatabaseAs(false, sender, false);
		}

		private void OnFileDbSettings(object sender, EventArgs e)
		{
			PwDatabase pd = m_docMgr.ActiveDatabase;
			DatabaseSettingsForm dsf = new DatabaseSettingsForm();
			dsf.InitEx(false, pd);

			if(dsf.ShowDialog() == DialogResult.OK)
			{
				if(pd.MemoryProtection.AutoEnableVisualHiding)
				{
					AutoEnableVisualHiding();
					RefreshEntriesList();
				}

				// Update group list, the recycle bin group might have been changed
				UpdateUI(false, null, true, null, false, null, true);
			}
		}

		private void OnFileChangeMasterKey(object sender, EventArgs e)
		{
			PwDatabase pd = m_docMgr.ActiveDatabase;

			KeyCreationForm kcf = new KeyCreationForm();
			kcf.InitEx(false, pd.IOConnectionInfo.GetDisplayName());
			if(kcf.ShowDialog() == DialogResult.OK)
			{
				pd.MasterKey = kcf.CompositeKey;
				MessageService.ShowInfo(KPRes.MasterKeyChanged, KPRes.MasterKeyChangedSavePrompt);
			}

			UpdateUIState(true);
		}

		private void OnFilePrint(object sender, EventArgs e)
		{
			if(!m_docMgr.ActiveDatabase.IsOpen) return;
			PrintGroup(m_docMgr.ActiveDatabase.RootGroup);
		}

		private void OnFileLock(object sender, EventArgs e)
		{
			if(UIIsInteractionBlocked()) { Debug.Assert(false); return; }

			PwDocument ds = m_docMgr.ActiveDocument;
			PwDatabase pd = ds.Database;

			if(!IsFileLocked(ds)) // Lock
			{
				if(!PrepareLock()) return; // Unable to lock
				LockAllDocuments();
			}
			else // Unlock
			{
				Debug.Assert(!pd.IsOpen);

				OpenDatabase(ds.LockedIoc, null, false);

				if(pd.IsOpen)
				{
					ds.LockedIoc = new IOConnectionInfo(); // Clear lock
					RestoreWindowState(pd);
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
			WinUtil.OpenUrl(PwDefs.HomepageUrl, null);
		}

		private void OnHelpDonate(object sender, EventArgs e)
		{
			WinUtil.OpenUrl(PwDefs.DonationsUrl, null);
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

			if(ClipboardUtil.CopyAndMinimize(pe.Strings.ReadSafe(PwDefs.UserNameField),
				true, Program.Config.MainWindow.MinimizeAfterClipboardCopy ?
				this : null, pe, m_docMgr.ActiveDatabase))
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

			if(ClipboardUtil.CopyAndMinimize(pe.Strings.ReadSafe(PwDefs.PasswordField),
				true, Program.Config.MainWindow.MinimizeAfterClipboardCopy ?
				this : null, pe, m_docMgr.ActiveDatabase))
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
			if((vSelected == null) || (vSelected.Length == 0)) return;

			FolderBrowserDialog fbd = UIUtil.CreateFolderBrowserDialog(KPRes.AttachmentsSave);

			GlobalWindowManager.AddDialog(fbd);
			if(fbd.ShowDialog() == DialogResult.OK)
				EntryUtil.SaveEntryAttachments(vSelected, fbd.SelectedPath);
			GlobalWindowManager.RemoveDialog(fbd);
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
			PwGroup pg = GetSelectedGroup();
			Debug.Assert(pg != null); if(pg == null) return;

			if(pg.IsVirtual)
			{
				MessageService.ShowWarning(KPRes.GroupCannotStoreEntries,
					KPRes.SelectDifferentGroup);
				return;
			}

			PwDatabase pwDb = m_docMgr.ActiveDatabase;
			PwEntry pwe = new PwEntry(true, true);
			pwe.Strings.Set(PwDefs.UserNameField, new ProtectedString(
				pwDb.MemoryProtection.ProtectUserName,
				pwDb.DefaultUserName));

			ProtectedString psAutoGen = new ProtectedString(
				pwDb.MemoryProtection.ProtectPassword);
			PwGenerator.Generate(psAutoGen, Program.Config.PasswordGenerator.AutoGeneratedPasswordsProfile,
				null);
			pwe.Strings.Set(PwDefs.PasswordField, psAutoGen);

			int nExpireDays = Program.Config.Defaults.NewEntryExpiresInDays;
			if(nExpireDays >= 0)
			{
				pwe.Expires = true;
				pwe.ExpiryTime = DateTime.Now.AddDays(nExpireDays);
			}

			if((pg.IconId != PwIcon.Folder) && (pg.IconId != PwIcon.FolderOpen) &&
				(pg.IconId != PwIcon.FolderPackage))
			{
				pwe.IconId = pg.IconId; // Inherit icon from group
			}

			// Temporarily assume that the entry is in pg; required for retrieving
			// the default auto-type sequence
			pwe.ParentGroup = pg;

			PwEntryForm pForm = new PwEntryForm();
			pForm.InitEx(pwe, PwEditMode.AddNewEntry, pwDb, m_ilCurrentIcons, false);
			if(pForm.ShowDialog() == DialogResult.OK)
			{
				pg.AddEntry(pwe, true);
				UpdateUI(false, null, false, null, true, null, true);
				EnsureVisibleEntry(pwe.Uuid);
			}
		}

		private void OnEntryEdit(object sender, EventArgs e)
		{
			PwEntry pe = GetSelectedEntry(false);
			if(pe == null) return; // Do not assert

			PwEntryForm pForm = new PwEntryForm();
			pForm.InitEx(pe, PwEditMode.EditExistingEntry, m_docMgr.ActiveDatabase,
				m_ilCurrentIcons, false);

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
			PwGroup pgSelected = GetSelectedGroup();

			PwEntry[] vSelected = GetSelectedEntries();
			if((vSelected == null) || (vSelected.Length == 0)) return;

			PwObjectList<PwEntry> vNewEntries = new PwObjectList<PwEntry>();
			foreach(PwEntry pe in vSelected)
			{
				PwEntry peNew = pe.CloneDeep();
				peNew.Uuid = new PwUuid(true); // Create new UUID

				ProtectedString psTitle = peNew.Strings.Get(PwDefs.TitleField);
				if(psTitle != null)
					peNew.Strings.Set(PwDefs.TitleField, new ProtectedString(
						psTitle.IsProtected, psTitle.ReadString() + " - " +
						KPRes.CopyOfItem));

				Debug.Assert(pe.ParentGroup == peNew.ParentGroup);
				PwGroup pg = (pe.ParentGroup ?? pgSelected);
				if((pg == null) && (m_docMgr.ActiveDocument != null))
					pg = m_docMgr.ActiveDatabase.RootGroup;
				if(pg == null) continue;

				pg.AddEntry(peNew, true);
				vNewEntries.Add(peNew);
			}

			AddEntriesToList(vNewEntries);
			SelectEntries(vNewEntries, true);

			if((m_lvEntries.ShowGroups == false) && (m_lvEntries.Items.Count >= 1))
				m_lvEntries.EnsureVisible(m_lvEntries.Items.Count - 1);
			else EnsureVisibleSelected(true);

			UpdateUIState(true);
		}

		private void OnEntryDelete(object sender, EventArgs e)
		{
			DeleteSelectedEntries();
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
			if(UIIsInteractionBlocked()) { e.Cancel = true; return; }

			if(!m_bForceExitOnce) // If not executed by File-Exit
			{
				if((e.CloseReason != CloseReason.TaskManagerClosing) &&
					(e.CloseReason != CloseReason.WindowsShutDown))
				{
					if(Program.Config.MainWindow.CloseButtonMinimizesWindow)
					{
						SaveWindowPositionAndSize();

						e.Cancel = true;
						this.WindowState = FormWindowState.Minimized;
						return;
					}
				}
			}
			m_bForceExitOnce = false; // Reset (flag works once only)

			if(CloseAllDocuments(true) == false)
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
				if(pg != m_docMgr.ActiveDatabase.RootGroup) { Debug.Assert(pg.ParentGroup != null); }

				m_tvGroups.SelectedNode = tn; // KPB 1757850

				pg.Touch(false);
				UpdateUI(false, null, false, pg, true, pg, false);
			}
		}

		private void OnMenuChangeLanguage(object sender, EventArgs e)
		{
			LanguageForm lf = new LanguageForm();
			if(lf.ShowDialog() == DialogResult.OK)
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
			PerformQuickFind(string.Empty, KPRes.AllEntriesTitle, true);
		}

		private void OnPwListFind(object sender, EventArgs e)
		{
			SearchForm sf = new SearchForm();

			sf.InitEx(m_docMgr.ActiveDatabase.RootGroup);
			if(sf.ShowDialog() == DialogResult.OK)
			{
				PwGroup pg = sf.SearchResultsGroup;

				UpdateEntryList(pg, false);
				SelectFirstEntryIfNoneSelected();

				UpdateUIState(false);
				ShowSearchResultsStatusMessage();

				ResetDefaultFocus(m_lvEntries);
			}
		}

		private void OnViewShowToolBar(object sender, EventArgs e)
		{
			bool b = m_menuViewShowToolBar.Checked;

			Program.Config.MainWindow.ToolBar.Show = b;
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
						if(i < (int)AppDefs.ColumnId.Count)
							PerformDefaultAction(sender, e, (PwEntry)lvi.Tag, (AppDefs.ColumnId)i);
						break;
					}

					++i;
				}
			}
			else PerformDefaultAction(sender, e, (PwEntry)lvi.Tag, AppDefs.ColumnId.Title);
		}

		private void OnQuickFindSelectedIndexChanged(object sender, EventArgs e)
		{
			if(m_bBlockQuickFind) return;

			string strSearch = m_tbQuickFind.Text; // Text, not selected index!
			string strGroupName = KPRes.SearchGroupName + " (\"" + strSearch + "\" ";
			strGroupName += KPRes.SearchResultsInSeparator + " ";
			strGroupName += m_docMgr.ActiveDatabase.RootGroup.Name + ")";

			PerformQuickFind(strSearch, strGroupName, false);

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
			bool bHandled = false;

			if((e.KeyCode == Keys.Return) || (e.KeyCode == Keys.Enter))
			{
				OnQuickFindSelectedIndexChanged(sender, e);
				bHandled = true;
			}
			// else if((e.KeyCode == Keys.Tab) && m_pwDatabase.IsOpen)
			// {
			//	this.ActiveControl = m_tvGroups;
			//	m_tvGroups.Focus();
			//	bHandled = true;
			// }

			if(bHandled)
			{
				e.Handled = true;
				e.SuppressKeyPress = true;
			}
		}

		private void OnQuickFindKeyUp(object sender, KeyEventArgs e)
		{
			bool bHandled = false;

			if((e.KeyCode == Keys.Return) || (e.KeyCode == Keys.Enter))
				bHandled = true;
			// else if(e.KeyCode == Keys.Tab)
			//	bHandled = true;

			if(bHandled)
			{
				e.Handled = true;
				e.SuppressKeyPress = true;
			}
		}

		private void OnToolsOptions(object sender, EventArgs e)
		{
			OptionsForm ofDlg = new OptionsForm();
			ofDlg.InitEx(m_ilCurrentIcons);

			if(ofDlg.ShowDialog() == DialogResult.OK)
			{
				m_nLockTimerMax = (int)Program.Config.Security.WorkspaceLocking.LockAfterTime;
				m_nClipClearMax = Program.Config.Security.ClipboardClearAfterSeconds;

				m_lvEntries.GridLines = Program.Config.MainWindow.ShowGridLines;

				SetListFont(Program.Config.UI.StandardFont);

				AppConfigSerializer.Save(Program.Config);
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
			PwEntry pe = (((ListViewItem)e.Item).Tag as PwEntry);
			if(pe == null) { Debug.Assert(false); return; }

			ListViewHitTestInfo lvHit = m_lvEntries.HitTest(m_ptLastEntriesMouseClick);
			ListViewItem lvi = lvHit.Item;
			if(lvi == null) return;

			string strText = string.Empty;

			if(!AppPolicy.Current.DragDrop)
				strText = AppPolicy.RequiredPolicyMessage(AppPolicyId.DragDrop);
			else
			{
				int i = 0;
				foreach(ListViewItem.ListViewSubItem lvs in lvi.SubItems)
				{
					if((lvs == lvHit.SubItem) && (i < (int)AppDefs.ColumnId.Count))
					{
						AppDefs.ColumnId cID = (AppDefs.ColumnId)i;

						switch(cID)
						{
							case AppDefs.ColumnId.Title:
								strText = pe.Strings.ReadSafe(PwDefs.TitleField);
								break;
							case AppDefs.ColumnId.UserName:
								strText = pe.Strings.ReadSafe(PwDefs.UserNameField);
								break;
							case AppDefs.ColumnId.Password:
								strText = pe.Strings.ReadSafe(PwDefs.PasswordField);
								break;
							case AppDefs.ColumnId.Url:
								strText = pe.Strings.ReadSafe(PwDefs.UrlField);
								break;
							case AppDefs.ColumnId.Notes:
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

			m_pgActiveAtDragStart = GetSelectedGroup();

			PwDatabase pd = ((m_docMgr.ActiveDocument != null) ? m_docMgr.ActiveDatabase : null);
			string strToTransfer = SprEngine.Compile(strText, false, pe, pd, false, false);

			m_bDraggingEntries = true;
			this.DoDragDrop(strToTransfer, DragDropEffects.Copy | DragDropEffects.Move);
			m_bDraggingEntries = false;

			pe.Touch(false);
		}

		private void OnGroupsListItemDrag(object sender, ItemDragEventArgs e)
		{
			TreeNode tn = (e.Item as TreeNode);
			if(tn == null) { Debug.Assert(false); return; }

			PwGroup pg = (tn.Tag as PwGroup);
			if(pg == null) { Debug.Assert(false); return; }

			if(pg == m_docMgr.ActiveDatabase.RootGroup) return;
			if(pg.ParentGroup == null) return;

			this.DoDragDrop(pg, DragDropEffects.Copy | DragDropEffects.Move);
			pg.Touch(false);
		}

		private void OnGroupsListDragDrop(object sender, DragEventArgs e)
		{
			TreeViewHitTestInfo tvhi = m_tvGroups.HitTest(m_tvGroups.PointToClient(new Point(e.X, e.Y)));

			if(tvhi.Node == null) return;
			PwGroup pgSelected = (tvhi.Node.Tag as PwGroup);
			Debug.Assert(pgSelected != null); if(pgSelected == null) return;

			if(m_bDraggingEntries)
			{
				PwEntry[] vSelected = GetSelectedEntries();
				if((vSelected == null) || (vSelected.Length == 0)) return;

				List<PwEntry> vNowInvisible = new List<PwEntry>();

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

						pgSelected.AddEntry(pe, true);

						vNowInvisible.Add(pe);
					}
				}
				else if(e.Effect == DragDropEffects.Copy)
				{
					foreach(PwEntry pe in vSelected)
					{
						PwEntry peCopy = pe.CloneDeep();
						peCopy.Uuid = new PwUuid(true); // Create new UUID

						pgSelected.AddEntry(peCopy, true);
					}
				}
				else { Debug.Assert(false); }

				RemoveEntriesFromList(vNowInvisible, true);
				UpdateUI(false, null, true, m_pgActiveAtDragStart, false, null, true);
				m_pgActiveAtDragStart = null;
			}
			else if(e.Data.GetDataPresent(typeof(PwGroup)))
			{
				PwGroup pgDragged = (e.Data.GetData(typeof(PwGroup)) as PwGroup);
				Debug.Assert(pgDragged != null);

				if((pgDragged == null) || (pgDragged == pgSelected) ||
					pgSelected.IsContainedIn(pgDragged))
				{
					UpdateUI(false, null, true, null, true, null, false);
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

					pgDragged.Uuid = new PwUuid(true);
					pgDragged.CreateNewItemUuids(true, true, true);
					pgDragged.TakeOwnership(true, true, true);
				}
				else { Debug.Assert(false); }

				pgSelected.AddGroup(pgDragged, true);

				pgSelected.IsExpanded = true;

				UpdateUI(false, null, true, pgDragged, true, null, true);
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
			else pgParent = m_docMgr.ActiveDatabase.RootGroup;

			if(pgParent == null) { Debug.Assert(false); return; }

			PwGroup pgNew = new PwGroup(true, true, KPRes.NewGroup, PwIcon.Folder);
			pgParent.AddGroup(pgNew, true);
			pgParent.IsExpanded = true;

			UpdateUI(false, null, true, pgNew, true, null, true);

			TreeNode tnNew = m_tvGroups.SelectedNode;
			if(tnNew != null) tnNew.BeginEdit();
		}

		private void OnGroupsDelete(object sender, EventArgs e)
		{
			DeleteSelectedGroup();
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
			bool bSuccess = ImportUtil.Synchronize(m_docMgr.ActiveDatabase, this, false);
			UpdateUI(false, null, true, null, true, null, false);
			SetStatusEx(bSuccess ? KPRes.SyncSuccess : KPRes.SyncFailed);
		}

		private void OnViewAlwaysOnTop(object sender, EventArgs e)
		{
			bool bTop = m_menuViewAlwaysOnTop.Checked;

			Program.Config.MainWindow.AlwaysOnTop = bTop;
			this.TopMost = bTop;
		}

		private void OnGroupsPrint(object sender, EventArgs e)
		{
			if(!m_docMgr.ActiveDatabase.IsOpen) return;
			PrintGroup(GetSelectedGroup());
		}

		private void OnViewShowColTitle(object sender, EventArgs e)
		{
			ShowColumn(AppDefs.ColumnId.Title, m_menuViewColumnsShowTitle.Checked);
		}

		private void OnViewShowColUser(object sender, EventArgs e)
		{
			ShowColumn(AppDefs.ColumnId.UserName, m_menuViewColumnsShowUserName.Checked);
		}

		private void OnViewShowColPassword(object sender, EventArgs e)
		{
			ShowColumn(AppDefs.ColumnId.Password, m_menuViewColumnsShowPassword.Checked);
		}

		private void OnViewShowColURL(object sender, EventArgs e)
		{
			ShowColumn(AppDefs.ColumnId.Url, m_menuViewColumnsShowUrl.Checked);
		}

		private void OnViewShowColNotes(object sender, EventArgs e)
		{
			ShowColumn(AppDefs.ColumnId.Notes, m_menuViewColumnsShowNotes.Checked);
		}

		private void OnViewShowColCreation(object sender, EventArgs e)
		{
			ShowColumn(AppDefs.ColumnId.CreationTime, m_menuViewColumnsShowCreation.Checked);
		}

		private void OnViewShowColLastAccess(object sender, EventArgs e)
		{
			ShowColumn(AppDefs.ColumnId.LastAccessTime, m_menuViewColumnsShowLastAccess.Checked);
		}

		private void OnViewShowColLastMod(object sender, EventArgs e)
		{
			ShowColumn(AppDefs.ColumnId.LastModificationTime, m_menuViewColumnsShowLastMod.Checked);
		}

		private void OnViewShowColExpire(object sender, EventArgs e)
		{
			ShowColumn(AppDefs.ColumnId.ExpiryTime, m_menuViewColumnsShowExpire.Checked);
		}

		private void OnViewShowColUUID(object sender, EventArgs e)
		{
			ShowColumn(AppDefs.ColumnId.Uuid, m_menuViewColumnsShowUuid.Checked);
		}

		private void OnViewShowColAttachs(object sender, EventArgs e)
		{
			ShowColumn(AppDefs.ColumnId.Attachment, m_menuViewColumnsShowAttachs.Checked);
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
				ShowColumn((AppDefs.ColumnId)e.ColumnIndex, false);
			}

			m_bDisableBlockingColumnSizing = false;
		}

		private void OnFormResize(object sender, EventArgs e)
		{
			FormWindowState ws = this.WindowState;

			if(ws == FormWindowState.Minimized)
			{
				// For default value, also see options dialog
				if(Program.Config.Security.WorkspaceLocking.LockOnWindowMinimize)
					if(IsFileLocked(null) == false) // Not locked currently
						OnFileLock(sender, e);

				if(Program.Config.MainWindow.MinimizeToTray)
					MinimizeToTray(true);
			}
			else if(ws == FormWindowState.Maximized)
				Program.Config.MainWindow.Maximized = true;
			else if(ws == FormWindowState.Normal)
			{
				Program.Config.MainWindow.Maximized = false;

				if((m_fwsLast == FormWindowState.Minimized) && IsFileLocked(null))
					OnFileLock(sender, e);
			}

			m_fwsLast = ws;
		}

		private void OnTrayTray(object sender, EventArgs e)
		{
			if((this.WindowState == FormWindowState.Minimized) && !IsTrayed())
			{
				if(Program.Config.MainWindow.Maximized) this.WindowState = FormWindowState.Maximized;
				else this.WindowState = FormWindowState.Normal;
				return;
			}

			MinimizeToTray(!IsTrayed());
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
				m_bAllowLockTimerMod = false;

				--m_nLockTimerCur;
				if(m_nLockTimerCur < 0) m_nLockTimerCur = 0;

				if(m_nLockTimerCur == 0)
				{
					NotifyUserActivity();

					if(Program.Config.Security.WorkspaceLocking.ExitInsteadOfLockingAfterTime)
						OnFileExit(sender, e);
					else LockAllDocuments();
				}

				m_bAllowLockTimerMod = true;
			}
		}

		private void OnToolsPlugins(object sender, EventArgs e)
		{
			if(!AppPolicy.Try(AppPolicyId.Plugins)) return;

			PluginsForm pf = new PluginsForm();
			pf.InitEx(m_pluginManager);

			pf.ShowDialog();
		}

		private void OnGroupsEdit(object sender, EventArgs e)
		{
			PwGroup pg = GetSelectedGroup();
			Debug.Assert(pg != null); if(pg == null) return;

			GroupForm gf = new GroupForm();
			gf.InitEx(pg, m_ilCurrentIcons, m_docMgr.ActiveDatabase);

			if(gf.ShowDialog() == DialogResult.OK)
				UpdateUI(false, null, true, null, true, null, true);
		}

		private void OnEntryCopyURL(object sender, EventArgs e)
		{
			PwEntry pe = GetSelectedEntry(false);
			Debug.Assert(pe != null); if(pe == null) return;

			if(ClipboardUtil.CopyAndMinimize(pe.Strings.ReadSafe(PwDefs.UrlField), true,
				Program.Config.MainWindow.MinimizeAfterClipboardCopy ?
				this : null, pe, m_docMgr.ActiveDatabase))
				StartClipboardCountdown();
		}

		private void OnEntryMassSetIcon(object sender, EventArgs e)
		{
			PwEntry[] vEntries = GetSelectedEntries();
			if((vEntries == null) || (vEntries.Length == 0)) return;

			IconPickerForm ipf = new IconPickerForm();
			ipf.InitEx(m_ilCurrentIcons, (uint)PwIcon.Count, m_docMgr.ActiveDatabase,
				(uint)vEntries[0].IconId, vEntries[0].CustomIconUuid);

			if(ipf.ShowDialog() == DialogResult.OK)
			{
				foreach(PwEntry pe in vEntries)
				{
					if(ipf.ChosenCustomIconUuid != PwUuid.Zero)
						pe.CustomIconUuid = ipf.ChosenCustomIconUuid;
					else
					{
						pe.IconId = (PwIcon)ipf.ChosenIconId;
						pe.CustomIconUuid = PwUuid.Zero;
					}
				}

				RefreshEntriesList();
				UpdateUIState(true);
			}
		}

		private void OnEntrySortTitle(object sender, EventArgs e)
		{
			SortPasswordList(true, (int)AppDefs.ColumnId.Title, true);
		}

		private void OnEntrySortUserName(object sender, EventArgs e)
		{
			SortPasswordList(true, (int)AppDefs.ColumnId.UserName, true);
		}

		private void OnEntrySortPassword(object sender, EventArgs e)
		{
			SortPasswordList(true, (int)AppDefs.ColumnId.Password, true);
		}

		private void OnEntrySortURL(object sender, EventArgs e)
		{
			SortPasswordList(true, (int)AppDefs.ColumnId.Url, true);
		}

		private void OnEntrySortNotes(object sender, EventArgs e)
		{
			SortPasswordList(true, (int)AppDefs.ColumnId.Notes, true);
		}

		private void OnEntrySortCreationTime(object sender, EventArgs e)
		{
			SortPasswordList(true, (int)AppDefs.ColumnId.CreationTime, true);
		}

		private void OnEntrySortLastMod(object sender, EventArgs e)
		{
			SortPasswordList(true, (int)AppDefs.ColumnId.LastModificationTime, true);
		}

		private void OnEntrySortLastAccess(object sender, EventArgs e)
		{
			SortPasswordList(true, (int)AppDefs.ColumnId.LastAccessTime, true);
		}

		private void OnEntrySortExpiration(object sender, EventArgs e)
		{
			SortPasswordList(true, (int)AppDefs.ColumnId.ExpiryTime, true);
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
			if(HandleMainWindowKeyMessage(e, true)) return;

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
			else if((e.KeyCode == Keys.Enter) || (e.KeyCode == Keys.Return))
				OnEntryEdit(sender, e);
			else if(e.KeyCode == Keys.Insert)
				OnEntryAdd(sender, e);
			else if(e.KeyCode == Keys.F2)
				OnEntryEdit(sender, e);
			else bUnhandled = true;

			if(!bUnhandled) e.Handled = true;
		}

		private void OnPwListKeyUp(object sender, KeyEventArgs e)
		{
			if(HandleMainWindowKeyMessage(e, false)) return;

			bool bUnhandled = false;

			if(e.Control)
			{
				switch(e.KeyCode)
				{
					case Keys.A: break;
					case Keys.C: break;
					case Keys.B: break;
					case Keys.E: break;
					case Keys.U: break;
					case Keys.V: break;
					default: bUnhandled = true; break;
				}
			}
			else if(e.Alt)
			{
				switch(e.KeyCode)
				{
					case Keys.Home: break;
					case Keys.Up: break;
					case Keys.Down: break;
					case Keys.End: break;
					default: bUnhandled = true; break;
				}
			}
			else if(e.KeyCode == Keys.Delete) { }
			else if((e.KeyCode == Keys.Enter) || (e.KeyCode == Keys.Return)) { }
			else if(e.KeyCode == Keys.Insert) { }
			else if(e.KeyCode == Keys.F2) { }
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
				ShowSearchResultsStatusMessage();
			}
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

		private void OnToolsPwGenerator(object sender, EventArgs e)
		{
			PwDatabase pwDb = m_docMgr.ActiveDatabase;
			PwGeneratorForm pgf = new PwGeneratorForm();

			pgf.InitEx(null, pwDb.IsOpen, IsTrayed());
			if(pgf.ShowDialog() == DialogResult.OK)
			{
				if(pwDb.IsOpen)
				{
					PwGroup pg = GetSelectedGroup();
					if(pg == null) pg = pwDb.RootGroup;

					PwEntry pe = new PwEntry(true, true);
					pg.AddEntry(pe, true);

					byte[] pbAdditionalEntropy = EntropyForm.CollectEntropyIfEnabled(
						pgf.SelectedProfile);
					ProtectedString psNew = new ProtectedString(pwDb.MemoryProtection.ProtectPassword);
					PwGenerator.Generate(psNew, pgf.SelectedProfile, pbAdditionalEntropy);
					pe.Strings.Set(PwDefs.PasswordField, psNew);

					UpdateUI(false, null, false, null, true, null, true);

					if(m_lvEntries.Items.Count > 0) // Select new entry
					{
						m_lvEntries.EnsureVisible(m_lvEntries.Items.Count - 1);
						for(int i = 0; i < (m_lvEntries.Items.Count - 1); ++i)
							m_lvEntries.Items[i].Selected = false; // Deselect
						m_lvEntries.Items[m_lvEntries.Items.Count - 1].Selected = true;
					}
				}
			}
		}

		private void OnToolsShowExpired(object sender, EventArgs e)
		{
			ShowExpiredEntries(false, 0);
		}

		private void OnToolsTanWizard(object sender, EventArgs e)
		{
			PwDatabase pwDb = m_docMgr.ActiveDatabase;
			if(!pwDb.IsOpen) { Debug.Assert(false); return; }

			PwGroup pgSelected = GetSelectedGroup();
			if(pgSelected == null) return;

			TanWizardForm twf = new TanWizardForm();
			twf.InitEx(pwDb, pgSelected);

			if(twf.ShowDialog() == DialogResult.OK)
				UpdateUI(false, null, false, null, true, null, true);
		}

		private void OnSystemTrayClick(object sender, EventArgs e)
		{
			if(Program.Config.UI.TrayIcon.SingleClickDefault)
				OnTrayTray(sender, e);
		}

		private void OnSystemTrayDoubleClick(object sender, EventArgs e)
		{
			if(!Program.Config.UI.TrayIcon.SingleClickDefault)
				OnTrayTray(sender, e);
		}

		private void OnEntryViewLinkClicked(object sender, LinkClickedEventArgs e)
		{
			PwEntry pe = GetSelectedEntry(false);

			if((pe != null) && (pe.ParentGroup != null) &&
				(pe.ParentGroup.Name == e.LinkText))
			{
				UpdateUI(false, null, true, pe.ParentGroup, true, null, false);
				EnsureVisibleSelected(false);
				ResetDefaultFocus(m_lvEntries);
			}
			else WinUtil.OpenUrl(e.LinkText, pe);
		}

		private void OnEntryClipCopy(object sender, EventArgs e)
		{
			if(m_docMgr.ActiveDatabase.IsOpen == false) return;
			PwEntry[] vSelected = GetSelectedEntries();
			if((vSelected == null) || (vSelected.Length == 0)) return;

			try
			{
				EntryUtil.CopyEntriesToClipboard(m_docMgr.ActiveDatabase, vSelected);
			}
			catch(Exception exCopy)
			{
				MessageService.ShowWarning(exCopy);
			}

			StartClipboardCountdown();
		}

		private void OnEntryClipPaste(object sender, EventArgs e)
		{
			if(m_docMgr.ActiveDatabase.IsOpen == false) return;
			PwGroup pg = GetSelectedGroup();
			if(pg == null) return;

			try { EntryUtil.PasteEntriesFromClipboard(m_docMgr.ActiveDatabase, pg); }
			catch(Exception exPaste)
			{
				MessageService.ShowWarning(exPaste);
			}

			UpdateUI(false, null, false, null, true, null, true);
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
			if(m_docMgr.ActiveDatabase.IsOpen == false) return;

			DatabaseOperationsForm form = new DatabaseOperationsForm();
			form.InitEx(m_docMgr.ActiveDatabase);
			form.ShowDialog();

			UpdateUIState(form.HasModifiedDatabase);
		}

		private void OnCtxPwListOpening(object sender, CancelEventArgs e)
		{
			m_dynCustomStrings.Clear();
			m_dynCustomBinaries.Clear();

			uint uSelected = GetSelectedEntriesCount();
			if(uSelected != 1)
			{
				m_ctxEntryCopyCustomString.Visible = false;
				m_ctxEntryBinaries.Visible = false;
				return;
			}

			PwEntry pe = GetSelectedEntry(true);
			if(pe == null) { Debug.Assert(false); return; }

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
			PwDatabase pwDb = m_docMgr.ActiveDatabase;
			if(!pwDb.IsOpen) return;

			PwGeneratorForm pgf = new PwGeneratorForm();

			pgf.InitEx(null, true, IsTrayed());
			if(pgf.ShowDialog() == DialogResult.OK)
			{
				PwGroup pg = GetSelectedGroup();
				if(pg == null) pg = pwDb.RootGroup;

				SingleLineEditForm dlgCount = new SingleLineEditForm();
				dlgCount.InitEx(KPRes.GenerateCount, KPRes.GenerateCountDesc,
					KPRes.GenerateCountLongDesc, Properties.Resources.B48x48_KGPG_Gen,
					string.Empty, null);
				if(dlgCount.ShowDialog() == DialogResult.OK)
				{
					uint uCount;
					if(!uint.TryParse(dlgCount.ResultString, out uCount))
						uCount = 1;

					byte[] pbAdditionalEntropy = EntropyForm.CollectEntropyIfEnabled(
						pgf.SelectedProfile);

					for(uint i = 0; i < uCount; ++i)
					{
						PwEntry pe = new PwEntry(true, true);
						pg.AddEntry(pe, true);

						ProtectedString psNew = new ProtectedString(pwDb.MemoryProtection.ProtectPassword);
						PwGenerator.Generate(psNew, pgf.SelectedProfile, pbAdditionalEntropy);
						pe.Strings.Set(PwDefs.PasswordField, psNew);
					}

					UpdateUI(false, null, false, null, true, null, true);
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
			SaveDatabaseAs(true, sender, false);
		}

		private void OnFileImport(object sender, EventArgs e)
		{
			PwDatabase pwDb = m_docMgr.ActiveDatabase;

			bool bAppendedToRootOnly;
			bool bModified = ImportUtil.ImportInto(pwDb, out bAppendedToRootOnly);

			if(bAppendedToRootOnly && pwDb.IsOpen)
			{
				UpdateGroupList(pwDb.RootGroup);
				UpdateEntryList(null, false);

				if(m_lvEntries.Items.Count > 0)
					m_lvEntries.EnsureVisible(m_lvEntries.Items.Count - 1);

				UpdateUIState(bModified);
			}
			else UpdateUI(false, null, true, null, true, null, bModified);
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
			if(HandleMainWindowKeyMessage(e, true)) return;

			bool bUnhandled = false;
			TreeNode tn = m_tvGroups.SelectedNode;

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
			else if(e.KeyCode == Keys.F2)
			{
				if(tn != null) tn.BeginEdit();
			}
			else bUnhandled = true;

			if(!bUnhandled) e.Handled = true;
		}

		private void OnGroupsKeyUp(object sender, KeyEventArgs e)
		{
			if(HandleMainWindowKeyMessage(e, false)) return;

			bool bUnhandled = false;

			if(e.Alt)
			{
				switch(e.KeyCode)
				{
					case Keys.Home: break;
					case Keys.Up: break;
					case Keys.Down: break;
					case Keys.End: break;
					default: bUnhandled = true; break;
				}
			}
			else if(e.KeyCode == Keys.Delete) { }
			else if(e.KeyCode == Keys.F2) { }
			else if((e.KeyCode == Keys.Up) || (e.KeyCode == Keys.Down) ||
				(e.KeyCode == Keys.Left) || (e.KeyCode == Keys.Down) ||
				(e.KeyCode == Keys.Home) || (e.KeyCode == Keys.End))
			{
				UpdateUI(false, null, false, null, true, null, false);
			}
			else bUnhandled = true;

			if(!bUnhandled) e.Handled = true;
		}

		private void OnEntryUrlOpenInInternal(object sender, EventArgs e)
		{
			InternalBrowserForm ibf = new InternalBrowserForm();

			string strUrl = string.Empty;
			PwEntry pe = GetSelectedEntry(false);
			if(pe != null) strUrl = pe.Strings.ReadSafe(PwDefs.UrlField);

			ibf.InitEx(strUrl, m_docMgr.ActiveDatabase.RootGroup);
			ibf.ShowDialog();
		}

		private void OnTabMainSelectedIndexChanged(object sender, EventArgs e)
		{
			if(m_bBlockTabChanged) return;

			SaveWindowState();

			TabPage tbSelect = m_tabMain.SelectedTab;
			if(tbSelect == null) return;

			PwDocument ds = (PwDocument)tbSelect.Tag;
			MakeDocumentActive(ds);

			if(IsFileLocked(ds)) OnFileLock(sender, e);
		}

		private void OnFileSaveAll(object sender, EventArgs e)
		{
			SaveAllDocuments();
		}

		private void OnFileSaveAsCopy(object sender, EventArgs e)
		{
			SaveDatabaseAs(false, sender, true);
		}

		private void OnEntrySelectedPrint(object sender, EventArgs e)
		{
			PrintGroup(GetSelectedEntriesAsGroup());
		}

		private void OnViewShowEntriesOfSubGroups(object sender, EventArgs e)
		{
			Program.Config.MainWindow.ShowEntriesOfSubGroups =
				m_menuViewShowEntriesOfSubGroups.Checked;
			UpdateUI(false, null, false, null, true, null, false);
		}

		private void OnFileSynchronizeUrl(object sender, EventArgs e)
		{
			bool bSuccess = ImportUtil.Synchronize(m_docMgr.ActiveDatabase, this, true);
			UpdateUI(false, null, true, null, true, null, false);
			SetStatusEx(bSuccess ? KPRes.SyncSuccess : KPRes.SyncFailed);
		}

		private void OnCtxTrayOpening(object sender, CancelEventArgs e)
		{
			UpdateUIState(false);
		}

		private void OnTrayLock(object sender, EventArgs e)
		{
			OnFileLock(sender, e);
		}

		private void OnFileExport(object sender, EventArgs e)
		{
			PerformExport(null, true);
		}

		private void OnGroupsExport(object sender, EventArgs e)
		{
			PerformExport(GetSelectedGroup(), true);
		}

		private void OnEntrySelectedExport(object sender, EventArgs e)
		{
			PerformExport(GetSelectedEntriesAsGroup(), false);
		}

		private void OnEntryViewKeyDown(object sender, KeyEventArgs e)
		{
			HandleMainWindowKeyMessage(e, true);
		}

		private void OnEntryViewKeyUp(object sender, KeyEventArgs e)
		{
			HandleMainWindowKeyMessage(e, false);
		}
	}
}
