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
using System.Threading;
using System.Windows.Forms;
using System.Globalization;
using System.Diagnostics;
using System.Security;
using System.Media;

using KeePass.App;
using KeePass.App.Configuration;
using KeePass.DataExchange;
using KeePass.Ecas;
using KeePass.Native;
using KeePass.Plugins;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;
using KeePass.Util.Spr;

using KeePassLib;
using KeePassLib.Collections;
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
		private NotifyIconEx m_ntfTray = null;

		private bool m_bFormLoading = false;
		private bool m_bCleanedUp = false;

		private bool m_bRestart = false;
		private ListSorter m_pListSorter = new ListSorter();
		private ListViewSortMenu m_lvsmMenu = null;

		private bool m_bDraggingEntries = false;

		private bool m_bBlockColumnUpdates = false;
		private bool m_bBlockEntrySelectionEvent = false;

		private bool m_bForceExitOnce = false;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public MainForm()
		{
			try
			{
				m_nTaskbarButtonMessage = NativeMethods.RegisterWindowMessage(
					"TaskbarButtonCreated");
				m_bTaskbarButtonMessage = (m_nTaskbarButtonMessage != 0);
			}
			catch(Exception)
			{
				m_nTaskbarButtonMessage = 0x1E8F46A7; // Unlikely to occur
				m_bTaskbarButtonMessage = false;
			}

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

			UIUtil.Initialize(false);

			InitializeComponent();
			Program.Translation.ApplyTo(this);
			Program.Translation.ApplyTo("KeePass.Forms.MainForm.m_menuMain", m_menuMain.Items);
			Program.Translation.ApplyTo("KeePass.Forms.MainForm.m_ctxPwList", m_ctxPwList.Items);
			Program.Translation.ApplyTo("KeePass.Forms.MainForm.m_ctxGroupList", m_ctxGroupList.Items);
			Program.Translation.ApplyTo("KeePass.Forms.MainForm.m_ctxTray", m_ctxTray.Items);

			m_asyncListUpdate = new AsyncPwListUpdate(m_lvEntries);

			m_splitHorizontal.InitEx(this.Controls, m_menuMain);
			m_splitVertical.InitEx(this.Controls, m_menuMain);

			AssignMenuShortcuts();

			if(NativeLib.IsUnix())
			{
				// Workaround for tab bar height bug in Mono
				// https://sourceforge.net/projects/keepass/forums/forum/329221/topic/4519750
				// https://bugs.launchpad.net/ubuntu/+source/keepass2/+bug/891029
				m_tabMain.Height += 5;
			}
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			m_bFormLoading = true;
			GlobalWindowManager.CustomizeControl(this);
			GlobalWindowManager.CustomizeControl(m_ctxTray);

			m_strNeverExpiresText = KPRes.NeverExpires;

			this.Text = PwDefs.ProductName;
			this.Icon = Properties.Resources.KeePass;
			m_imgFileSaveEnabled = Properties.Resources.B16x16_FileSave;
			m_imgFileSaveDisabled = Properties.Resources.B16x16_FileSave_Disabled;
			// m_imgFileSaveAllEnabled = Properties.Resources.B16x16_File_SaveAll;
			// m_imgFileSaveAllDisabled = Properties.Resources.B16x16_File_SaveAll_Disabled;
			m_ilCurrentIcons = m_ilClientIcons;

			m_ctxEntryOpenUrl.Text = KPRes.OpenCmd;

			m_ntfTray = new NotifyIconEx(this.components);
			m_ntfTray.ContextMenuStrip = m_ctxTray;
			m_ntfTray.Visible = true;
			m_ntfTray.SetHandlers(OnSystemTrayClick, OnSystemTrayDoubleClick,
				OnSystemTrayMouseDown);

			m_ctxTrayTray.Font = FontUtil.CreateFont(m_ctxTrayTray.Font, FontStyle.Bold);

			m_nLockTimerMax = (int)Program.Config.Security.WorkspaceLocking.LockAfterTime;
			m_nClipClearMax = Program.Config.Security.ClipboardClearAfterSeconds;

			NativeLib.AllowNative = Program.Config.Native.NativeKeyTransformations;

			m_ctxEntryPreviewContextMenu.Attach(m_richEntryView, this);

			m_dynCustomStrings = new DynamicMenu(m_ctxEntryCopyCustomString.DropDownItems);
			m_dynCustomStrings.MenuClick += this.OnCopyCustomString;

			m_dynCustomBinaries = new DynamicMenu(m_ctxEntryAttachments.DropDownItems);
			m_dynCustomBinaries.MenuClick += this.OnEntryBinaryView;

			m_dynShowEntriesByTagsEditMenu = new DynamicMenu(m_menuEditShowByTag.DropDownItems);
			m_dynShowEntriesByTagsEditMenu.MenuClick += this.OnShowEntriesByTag;

			m_dynShowEntriesByTagsToolBar = new DynamicMenu(m_tbEntryViewsDropDown.DropDownItems);
			m_dynShowEntriesByTagsToolBar.MenuClick += this.OnShowEntriesByTag;

			m_dynAddTag = new DynamicMenu(m_ctxEntrySelectedAddTag.DropDownItems);
			m_dynAddTag.MenuClick += this.OnAddEntryTag;

			m_dynRemoveTag = new DynamicMenu(m_ctxEntrySelectedRemoveTag.DropDownItems);
			m_dynRemoveTag.MenuClick += this.OnRemoveEntryTag;

			m_dynOpenUrl = new OpenWithMenu(m_ctxEntryUrl);

			EntryTemplates.Init(m_tbAddEntry);

			m_menuEdit.DropDownItems.Insert(0, new ToolStripSeparator());
			InsertToolStripItem(m_menuEdit, m_ctxEntrySelectAll, new EventHandler(OnEntrySelectAll), true);
			m_menuEdit.DropDownItems.Insert(0, new ToolStripSeparator());
			InsertToolStripItem(m_menuEdit, m_ctxEntryDelete, new EventHandler(OnEntryDelete), true);
			InsertToolStripItem(m_menuEdit, m_ctxEntryDuplicate, new EventHandler(OnEntryDuplicate), true);
			InsertToolStripItem(m_menuEdit, m_ctxEntryEdit, new EventHandler(OnEntryEdit), true);
			ToolStripMenuItem tsmiAddEntry = InsertToolStripItem(m_menuEdit,
				m_ctxEntryAdd, new EventHandler(OnEntryAdd), true);
			m_menuEdit.DropDownItems.Insert(0, new ToolStripSeparator());
			InsertToolStripItem(m_menuEdit, m_ctxGroupDelete, new EventHandler(OnGroupsDelete), true);
			InsertToolStripItem(m_menuEdit, m_ctxGroupEdit, new EventHandler(OnGroupsEdit), true);
			InsertToolStripItem(m_menuEdit, m_ctxGroupAdd, new EventHandler(OnGroupsAdd), true);

			UIUtil.AssignShortcut(tsmiAddEntry, Keys.Control | Keys.I);

			UIUtil.ConfigureTbButton(m_tbNewDatabase, KPRes.ToolBarNew, null, m_menuFileNew);
			UIUtil.ConfigureTbButton(m_tbOpenDatabase, KPRes.ToolBarOpen, null, m_menuFileOpenLocal);
			UIUtil.ConfigureTbButton(m_tbSaveDatabase, KPRes.Save, null, m_menuFileSave);
			UIUtil.ConfigureTbButton(m_tbSaveAll, KPRes.ToolBarSaveAll, null, null);
			UIUtil.ConfigureTbButton(m_tbAddEntry, KPRes.AddEntry, null, null);
			UIUtil.ConfigureTbButton(m_tbCopyUserName, KPRes.CopyUserFull, null, m_ctxEntryCopyUserName);
			UIUtil.ConfigureTbButton(m_tbCopyPassword, KPRes.CopyPasswordFull, null, m_ctxEntryCopyPassword);
			UIUtil.ConfigureTbButton(m_tbFind, KPRes.Find + "...", null, m_menuEditFind);
			UIUtil.ConfigureTbButton(m_tbEntryViewsDropDown, null, KPRes.ShowEntries, null);
			UIUtil.ConfigureTbButton(m_tbLockWorkspace, KPRes.LockMenuLock, null, m_menuFileLock);
			UIUtil.ConfigureTbButton(m_tbQuickFind, null, KPRes.SearchQuickPrompt +
				" (" + KPRes.KeyboardKeyCtrl + "+E)", null);
			UIUtil.ConfigureTbButton(m_tbCloseTab, StrUtil.RemoveAccelerator(
				KPRes.CloseButton), null, m_menuFileClose);

			CopyMenuItemText(m_tbAddEntryDefault, m_ctxEntryAdd);
			CopyMenuItemText(m_tbViewsShowAll, m_menuEditShowAllEntries);
			CopyMenuItemText(m_tbViewsShowExpired, m_menuEditShowExpired);

			UIUtil.EnableAutoCompletion(m_tbQuickFind, false);

			bool bVisible = Program.Config.MainWindow.ToolBar.Show;
			m_toolMain.Visible = bVisible;
			m_menuViewShowToolBar.Checked = bVisible;

			// Make a copy of the maximized setting (the configuration item might
			// get changed when the window's position/size is restored)
			bool bMaximizedSetting = Program.Config.MainWindow.Maximized;

			int wndX = Program.Config.MainWindow.X;
			int wndY = Program.Config.MainWindow.Y;
			int sizeX = Program.Config.MainWindow.Width;
			int sizeY = Program.Config.MainWindow.Height;
			bool bWndValid = ((wndX != -32000) && (wndY != -32000) &&
				(wndX != -64000) && (wndY != -64000));

			if((sizeX != AppDefs.InvalidWindowValue) &&
				(sizeY != AppDefs.InvalidWindowValue) && bWndValid)
				this.Size = new Size(sizeX, sizeY);

			Rectangle rectRestWindow = new Rectangle(wndX, wndY,
				this.Size.Width, this.Size.Height);
			bool bWndPartVisible = UIUtil.IsScreenAreaVisible(rectRestWindow);
			if((wndX != AppDefs.InvalidWindowValue) &&
				(wndY != AppDefs.InvalidWindowValue) && bWndValid && bWndPartVisible)
			{
				this.Location = new Point(wndX, wndY);
			}
			else
			{
				Rectangle rectScreen = Screen.PrimaryScreen.WorkingArea;
				this.Location = new Point((rectScreen.Width - this.Size.Width) / 2,
					(rectScreen.Height - this.Size.Height) / 2);
			}

			SetMainWindowLayout(Program.Config.MainWindow.Layout == AceMainWindowLayout.SideBySide);
			ShowEntryView(Program.Config.MainWindow.EntryView.Show);
			UpdateColumnsEx(false);

			AceMainWindow mw = Program.Config.MainWindow;

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

			m_lvsmMenu = new ListViewSortMenu(m_menuViewSortBy, m_lvEntries,
				new SortCommandHandler(this.SortPasswordList));

			m_menuViewAlwaysOnTop.Checked = mw.AlwaysOnTop;
			OnViewAlwaysOnTop(null, null);

			m_mruList.Initialize(this, m_menuFileRecent, m_menuFileSyncRecent);
			SerializeMruList(false);

			SetListFont(Program.Config.UI.StandardFont);

			m_ctxEntryColorLightRed.Image = UIUtil.CreateColorBitmap24(16, 16,
				AppDefs.NamedEntryColor.LightRed);
			m_ctxEntryColorLightGreen.Image = UIUtil.CreateColorBitmap24(16, 16,
				AppDefs.NamedEntryColor.LightGreen);
			m_ctxEntryColorLightBlue.Image = UIUtil.CreateColorBitmap24(16, 16,
				AppDefs.NamedEntryColor.LightBlue);
			m_ctxEntryColorLightYellow.Image = UIUtil.CreateColorBitmap24(16, 16,
				AppDefs.NamedEntryColor.LightYellow);

			// m_lvEntries.GridLines = mw.ShowGridLines;
			if(UIUtil.VistaStyleListsSupported)
			{
				// m_tvGroups.ItemHeight += 1;

				m_tvGroups.ShowRootLines = false;
				m_tvGroups.ShowLines = false;

				UIUtil.SetExplorerTheme(m_tvGroups.Handle);
				UIUtil.SetExplorerTheme(m_lvEntries.Handle);
			}

			// m_tvGroups.QueryToolTip = UIUtil.GetPwGroupToolTipTN;

			m_clrAlternateItemBgColor = UIUtil.GetAlternateColor(m_lvEntries.BackColor);

			m_statusPartProgress.Visible = false;

			if(bMaximizedSetting)
			{
				if((this.WindowState == FormWindowState.Normal) && !IsTrayed())
					UIUtil.SetWindowState(this, FormWindowState.Maximized);
			}

			try
			{
				float fSplitPos = mw.SplitterHorizontalFrac;
				if(fSplitPos == float.Epsilon) fSplitPos = 0.8333f;
				m_splitHorizontal.SplitterDistance = (int)Math.Round(fSplitPos *
					(double)m_splitHorizontal.Height);

				fSplitPos = mw.SplitterVerticalFrac;
				if(fSplitPos == float.Epsilon) fSplitPos = 0.25f;
				m_splitVertical.SplitterDistance = (int)Math.Round(fSplitPos *
					(double)m_splitVertical.Width);
			}
			catch(Exception) { Debug.Assert(false); }

			string strSearchTr = ((WinUtil.IsAtLeastWindowsVista ?
				string.Empty : " ") + KPRes.Search);
			UIUtil.SetCueBanner(m_tbQuickFind, strSearchTr);

#if DEBUG
			Program.Config.CustomConfig.SetBool("TestItem", true);

			Program.KeyProviderPool.Add(new KeePassLib.Keys.SampleKeyProvider());
#endif

			m_sessionLockNotifier.Install(this.OnSessionLock);
			IpcBroadcast.StartServer();

			int nInitColProvCount = Program.ColumnProviderPool.Count;

			m_pluginDefaultHost.Initialize(this, Program.CommandLineArgs,
				CipherPool.GlobalPool);
			m_pluginManager.Initialize(m_pluginDefaultHost);

			m_pluginManager.UnloadAllPlugins();
			if(AppPolicy.Current.Plugins)
				m_pluginManager.LoadAllPlugins(UrlUtil.GetFileDirectory(
					WinUtil.GetExecutable(), false, true));

			// Delete old files *after* loading plugins (when timestamps
			// of loaded plugins have been updated already)
			if(Program.Config.Application.Start.PluginCacheDeleteOld)
				PlgxCache.DeleteOldFilesAsync();

			if(Program.ColumnProviderPool.Count != nInitColProvCount)
				UpdateColumnsEx(false);

			HotKeyManager.Initialize(this);

			Keys kAutoTypeKey = (Keys)Program.Config.Integration.HotKeyGlobalAutoType;
			HotKeyManager.RegisterHotKey(AppDefs.GlobalHotKeyId.AutoType, kAutoTypeKey);
			Keys kAutoTypeSelKey = (Keys)Program.Config.Integration.HotKeySelectedAutoType;
			HotKeyManager.RegisterHotKey(AppDefs.GlobalHotKeyId.AutoTypeSelected, kAutoTypeSelKey);
			Keys kShowWindowKey = (Keys)Program.Config.Integration.HotKeyShowWindow;
			HotKeyManager.RegisterHotKey(AppDefs.GlobalHotKeyId.ShowWindow, kShowWindowKey);
			Keys kEntryMenuKey = (Keys)Program.Config.Integration.HotKeyEntryMenu;
			HotKeyManager.RegisterHotKey(AppDefs.GlobalHotKeyId.EntryMenu, kEntryMenuKey);

			m_statusClipboard.Visible = false;
			UpdateClipboardStatus();

			UpdateTrayIcon();
			UpdateTagsMenu(m_dynShowEntriesByTagsEditMenu, false, false,
				false, false); // Ensure popup arrow
			UpdateTagsMenu(m_dynRemoveTag, false, false, false, false);
			UpdateUIState(false);
			ApplyUICustomizations();

			ThreadPool.QueueUserWorkItem(new WaitCallback(OnFormLoadParallelAsync));

			Program.TriggerSystem.RaiseEvent(EcasEventIDs.AppInitPost);

			if(Program.CommandLineArgs.FileName != null)
				OpenDatabase(IocFromCommandLine(), KeyUtil.KeyFromCommandLine(
					Program.CommandLineArgs), false);
			else if(Program.Config.Application.Start.OpenLastFile)
			{
				IOConnectionInfo ioLastFile = Program.Config.Application.LastUsedFile;
				if(ioLastFile.Path.Length > 0)
					OpenDatabase(ioLastFile, null, false);
			}

			UpdateCheckEx.EnsureConfigured(this);
			if(Program.Config.Application.Start.CheckForUpdate)
				UpdateCheckEx.Run(false, null);
			// UpdateCheck.StartAsync(PwDefs.VersionUrl, m_statusPartInfo);

			ResetDefaultFocus(null);

			MinimizeToTrayAtStartIfEnabled(true);

			m_bFormLoading = false;
			NotifyUserActivity(); // Initialize locking timeout
			Program.TriggerSystem.RaiseEvent(EcasEventIDs.AppLoadPost);
		}

		private void OnFormShown(object sender, EventArgs e)
		{
			if(NativeLib.IsUnix()) // Workaround for Mono bug 620618
			{
				PwGroup pg = GetCurrentEntries();
				UpdateColumnsEx(false);
				UpdateUI(false, null, false, null, true, pg, false);
			}

			MinimizeToTrayAtStartIfEnabled(false);
		}

		private void OnFileNew(object sender, EventArgs e)
		{
			if(!AppPolicy.Try(AppPolicyId.NewFile)) return;
			if(!AppPolicy.Try(AppPolicyId.SaveFile)) return;

			SaveFileDialog sfd = UIUtil.CreateSaveFileDialog(KPRes.CreateNewDatabase,
				KPRes.NewDatabaseFileName, UIUtil.CreateFileTypeFilter(
				AppDefs.FileExtension.FileExt, KPRes.KdbxFiles, true), 1,
				AppDefs.FileExtension.FileExt, false, true);

			GlobalWindowManager.AddDialog(sfd);
			DialogResult dr = sfd.ShowDialog();
			GlobalWindowManager.RemoveDialog(sfd);

			string strPath = sfd.FileName;

			if(dr != DialogResult.OK) return;

			KeyCreationForm kcf = new KeyCreationForm();
			kcf.InitEx(IOConnectionInfo.FromPath(strPath), true);
			dr = kcf.ShowDialog();
			if((dr == DialogResult.Cancel) || (dr == DialogResult.Abort))
			{
				UIUtil.DestroyForm(kcf);
				return;
			}

			PwDocument dsPrevActive = m_docMgr.ActiveDocument;
			PwDatabase pd = m_docMgr.CreateNewDocument(true).Database;
			pd.New(IOConnectionInfo.FromPath(strPath), kcf.CompositeKey);

			UIUtil.DestroyForm(kcf);

			DatabaseSettingsForm dsf = new DatabaseSettingsForm();
			dsf.InitEx(true, pd);
			dr = dsf.ShowDialog();
			if((dr == DialogResult.Cancel) || (dr == DialogResult.Abort))
			{
				m_docMgr.CloseDatabase(pd);
				try { m_docMgr.ActiveDocument = dsPrevActive; }
				catch(Exception) { } // Fails if no database is open now
				UpdateUI(false, null, true, null, true, null, false);
				UIUtil.DestroyForm(dsf);
				return;
			}
			UIUtil.DestroyForm(dsf);

			// AutoEnableVisualHiding();

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
			pe.AutoType.Add(new AutoTypeAssociation(KPRes.TargetWindow,
				@"{USERNAME}{TAB}{PASSWORD}{TAB}{ENTER}"));
			pd.RootGroup.AddEntry(pe, true);

			pe = new PwEntry(true, true);
			pe.Strings.Set(PwDefs.TitleField, new ProtectedString(pd.MemoryProtection.ProtectTitle,
				KPRes.SampleEntry + " #2"));
			pe.Strings.Set(PwDefs.UserNameField, new ProtectedString(pd.MemoryProtection.ProtectUserName,
				"Michael321"));
			pe.Strings.Set(PwDefs.UrlField, new ProtectedString(pd.MemoryProtection.ProtectUrl,
				@"http://keepass.info/help/kb/kb090406_testform.html"));
			pe.Strings.Set(PwDefs.PasswordField, new ProtectedString(pd.MemoryProtection.ProtectPassword,
				"12345"));
			pe.AutoType.Add(new AutoTypeAssociation("Test Form - KeePass*", string.Empty));
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

			pd.CustomData.Set("Sample Custom Data 1", "0123456789");
			pd.CustomData.Set("Sample Custom Data 2", @"µy data");
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
			CloseDocument(null, false, false);
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
				PreSavingEx(pd);
				pd.Save(swLogger);
				PostSavingEx(true, pd, pd.IOConnectionInfo, swLogger);
			}
			catch(Exception exSave)
			{
				MessageService.ShowSaveWarning(pd.IOConnectionInfo, exSave, true);
				bSuccess = false;
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

			if(UIUtil.ShowDialogAndDestroy(dsf) == DialogResult.OK)
			{
				// if(pd.MemoryProtection.AutoEnableVisualHiding)
				// {
				//	AutoEnableVisualHiding();
				//	RefreshEntriesList();
				// }

				// Update tab bar (database color might have been changed),
				// update group list (recycle bin group might have been changed)
				UpdateUI(true, null, true, null, false, null, true);
				RefreshEntriesList(); // History items might have been deleted
			}
		}

		private void OnFileChangeMasterKey(object sender, EventArgs e)
		{
			UpdateUIState(ChangeMasterKey(null));
		}

		private void OnFilePrint(object sender, EventArgs e)
		{
			if(!m_docMgr.ActiveDatabase.IsOpen) return;
			PrintGroup(m_docMgr.ActiveDatabase.RootGroup);
		}

		private void OnFileLock(object sender, EventArgs e)
		{
			if(UIIsInteractionBlocked()) { Debug.Assert(false); return; }
			if(GlobalWindowManager.WindowCount != 0) return;

			PwDocument ds = m_docMgr.ActiveDocument;
			if(!IsFileLocked(ds)) // Lock
			{
				LockAllDocuments();
				if(m_bCleanedUp) return; // Exited instead of locking
			}
			else // Unlock
			{
				PwDatabase pd = ds.Database;
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
			NotifyUserActivity();
			if(UIIsInteractionBlocked()) { Debug.Assert(false); return; }

			if(GlobalWindowManager.CanCloseAllWindows)
				GlobalWindowManager.CloseAllWindows();

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
			// UpdateCheck.StartAsync(PwDefs.VersionUrl, null);
			UpdateCheckEx.Run(true, this);
		}

		private void OnHelpAbout(object sender, EventArgs e)
		{
			AboutForm abf = new AboutForm();
			UIUtil.ShowDialogAndDestroy(abf);
		}

		private void OnEntryCopyUserName(object sender, EventArgs e)
		{
			PwEntry pe = GetSelectedEntry(false);
			Debug.Assert(pe != null); if(pe == null) return;

			if(ClipboardUtil.CopyAndMinimize(pe.Strings.GetSafe(PwDefs.UserNameField),
				true, this, pe, m_docMgr.SafeFindContainerOf(pe)))
				StartClipboardCountdown();
		}

		private void OnEntryCopyPassword(object sender, EventArgs e)
		{
			PwEntry pe = GetSelectedEntry(false);
			Debug.Assert(pe != null); if(pe == null) return;

			if(EntryUtil.ExpireTanEntryIfOption(pe))
			{
				RefreshEntriesList();
				UpdateUIState(true);
			}

			if(ClipboardUtil.CopyAndMinimize(pe.Strings.GetSafe(PwDefs.PasswordField),
				true, this, pe, m_docMgr.SafeFindContainerOf(pe)))
				StartClipboardCountdown();
		}

		private void OnEntryOpenUrl(object sender, EventArgs e)
		{
			PerformDefaultUrlAction(null, true);
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
			fbd.Dispose();
		}

		private void OnEntryPerformAutoType(object sender, EventArgs e)
		{
			PwEntry pe = GetSelectedEntry(false);
			if(pe != null)
			{
				try
				{
					AutoType.PerformIntoPreviousWindow(this, pe,
						m_docMgr.SafeFindContainerOf(pe));
				}
				catch(Exception ex) { MessageService.ShowWarning(ex); }
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
				pwDb.MemoryProtection.ProtectUserName, pwDb.DefaultUserName));

			ProtectedString psAutoGen;
			PwGenerator.Generate(out psAutoGen, Program.Config.PasswordGenerator.AutoGeneratedPasswordsProfile,
				null, Program.PwGeneratorPool);
			psAutoGen = psAutoGen.WithProtection(pwDb.MemoryProtection.ProtectPassword);
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
			pwe.CustomIconUuid = pg.CustomIconUuid;

			// Temporarily assume that the entry is in pg; required for retrieving
			// the default auto-type sequence
			pwe.ParentGroup = pg;

			PwEntryForm pForm = new PwEntryForm();
			pForm.InitEx(pwe, PwEditMode.AddNewEntry, pwDb, m_ilCurrentIcons,
				false, false);
			if(UIUtil.ShowDialogAndDestroy(pForm) == DialogResult.OK)
			{
				pg.AddEntry(pwe, true);
				UpdateUI(false, null, pwDb.UINeedsIconUpdate, null, true,
					null, true, m_lvEntries);

				PwObjectList<PwEntry> vSelect = new PwObjectList<PwEntry>();
				vSelect.Add(pwe);
				SelectEntries(vSelect, true, true);

				EnsureVisibleEntry(pwe.Uuid);
			}
			else UpdateUI(false, null, pwDb.UINeedsIconUpdate, null,
				pwDb.UINeedsIconUpdate, null, false);
		}

		private void OnEntryEdit(object sender, EventArgs e)
		{
			EditSelectedEntry(false);
		}

		private void OnEntryDuplicate(object sender, EventArgs e)
		{
			PwDatabase pd = m_docMgr.ActiveDatabase;
			PwGroup pgSelected = GetSelectedGroup();

			PwEntry[] vSelected = GetSelectedEntries();
			if((vSelected == null) || (vSelected.Length == 0)) return;

			DuplicationForm dlg = new DuplicationForm();
			if(UIUtil.ShowDialogAndDestroy(dlg) != DialogResult.OK) return;

			PwObjectList<PwEntry> vNewEntries = new PwObjectList<PwEntry>();
			foreach(PwEntry pe in vSelected)
			{
				PwEntry peNew = pe.CloneDeep();
				peNew.SetUuid(new PwUuid(true), true); // Create new UUID

				if(dlg.AppendCopyToTitles && (pd != null))
				{
					string strTitle = peNew.Strings.ReadSafe(PwDefs.TitleField);
					peNew.Strings.Set(PwDefs.TitleField, new ProtectedString(
						pd.MemoryProtection.ProtectTitle, strTitle + " - " +
						KPRes.CopyOfItem));
				}

				if(dlg.ReplaceDataByFieldRefs && (pd != null))
				{
					string strUser = @"{REF:U@I:" + pe.Uuid.ToHexString() + @"}";
					peNew.Strings.Set(PwDefs.UserNameField, new ProtectedString(
						pd.MemoryProtection.ProtectUserName, strUser));

					string strPw = @"{REF:P@I:" + pe.Uuid.ToHexString() + @"}";
					peNew.Strings.Set(PwDefs.PasswordField, new ProtectedString(
						pd.MemoryProtection.ProtectPassword, strPw));
				}

				Debug.Assert(pe.ParentGroup == peNew.ParentGroup);
				PwGroup pg = (pe.ParentGroup ?? pgSelected);
				if((pg == null) && (pd != null)) pg = pd.RootGroup;
				if(pg == null) continue;

				pg.AddEntry(peNew, true, true);
				vNewEntries.Add(peNew);
			}

			AddEntriesToList(vNewEntries);
			SelectEntries(vNewEntries, true, false);

			if(!m_lvEntries.ShowGroups && (m_lvEntries.Items.Count >= 1))
				m_lvEntries.EnsureVisible(m_lvEntries.Items.Count - 1);
			else EnsureVisibleSelected(true);

			UpdateUIState(true, m_lvEntries);
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

			ResetDefaultFocus(m_lvEntries);
			UpdateUIState(false);
		}

		private void OnFormClosing(object sender, FormClosingEventArgs e)
		{
			if(UIIsInteractionBlocked()) { e.Cancel = true; return; }

			if(!m_bForceExitOnce) // If not executed by File->Exit
			{
				if((e.CloseReason != CloseReason.TaskManagerClosing) &&
					(e.CloseReason != CloseReason.WindowsShutDown))
				{
					if(Program.Config.MainWindow.CloseButtonMinimizesWindow)
					{
						SaveWindowPositionAndSize();

						e.Cancel = true;
						UIUtil.SetWindowState(this, FormWindowState.Minimized);
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

			GlobalWindowManager.CloseAllWindows();

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
				PwGroup pg = (tn.Tag as PwGroup);
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
			if(UIUtil.ShowDialogAndDestroy(lf) == DialogResult.OK)
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

			sf.InitEx(m_docMgr.ActiveDatabase, m_docMgr.ActiveDatabase.RootGroup);
			if(sf.ShowDialog() == DialogResult.OK)
			{
				PwGroup pg = sf.SearchResultsGroup;

				UpdateEntryList(pg, false);
				SelectFirstEntryIfNoneSelected();

				UpdateUIState(false);
				ShowSearchResultsStatusMessage();

				ResetDefaultFocus(m_lvEntries);
			}
			UIUtil.DestroyForm(sf);
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

			// Always defer deselection updates (to ignore item deselect
			// and reselect events when clicking on a selected item)
			if(m_lvEntries.SelectedIndices.Count == 0)
			{
				m_bUpdateUIStateOnce = true;
				return;
			}

			int nCurTicks = Environment.TickCount;
			int nTimeDiff = unchecked(nCurTicks - m_nLastSelChUpdateUIStateTicks);
			if(nTimeDiff < 100) m_bUpdateUIStateOnce = true; // Burst, defer update
			else UpdateUIState(false);
			m_nLastSelChUpdateUIStateTicks = nCurTicks;
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
						PwListItem pli = (lvi.Tag as PwListItem);
						if(pli != null) PerformDefaultAction(sender, e, pli.Entry, i);
						else { Debug.Assert(false); }
						break;
					}

					++i;
				}
			}
			else
			{
				PwListItem pli = (lvi.Tag as PwListItem);
				if(pli != null) PerformDefaultAction(sender, e, pli.Entry, 0);
				else { Debug.Assert(false); }
			}
		}

		private void OnQuickFindSelectedIndexChanged(object sender, EventArgs e)
		{
			if(m_bBlockQuickFind) return;
			m_bBlockQuickFind = true;

			string strSearch = m_tbQuickFind.Text; // Text, not selected index!

			lock(m_objQuickFindSync)
			{
				int iNow = Environment.TickCount;
				if(((iNow - m_iLastQuickFindTicks) <= 1000) &&
					(strSearch == m_strLastQuickSearch))
				{
					m_bBlockQuickFind = false;
					return;
				}

				m_iLastQuickFindTicks = iNow;
				m_strLastQuickSearch = strSearch;
			}

			string strGroupName = KPRes.SearchGroupName + " (\"" + strSearch + "\" ";
			strGroupName += KPRes.SearchResultsInSeparator + " ";
			strGroupName += m_docMgr.ActiveDatabase.RootGroup.Name + ")";

			// PerformQuickFind(strSearch, strGroupName, false);

			// Lookup in combobox for the current search
			int nExistsAlready = -1;
			for(int i = 0; i < m_tbQuickFind.Items.Count; ++i)
			{
				string strItemText = (string)m_tbQuickFind.Items[i];
				if(strItemText.Equals(strSearch, StringComparison.InvariantCultureIgnoreCase))
				{
					nExistsAlready = i;
					break;
				}
			}

			// Update the history items in the combobox
			if(nExistsAlready >= 0)
				m_tbQuickFind.Items.RemoveAt(nExistsAlready);
			else if(m_tbQuickFind.Items.Count >= 8)
				m_tbQuickFind.Items.RemoveAt(m_tbQuickFind.Items.Count - 1);

			m_tbQuickFind.Items.Insert(0, strSearch);

			// if(bDoSetText) m_tbQuickFind.Text = strSearch;
			m_tbQuickFind.SelectedIndex = 0;
			m_tbQuickFind.Select(0, strSearch.Length);

			// Asynchronous invokation allows to cleanly process
			// an Enter keypress before blocking the UI
			BeginInvoke(new PerformQuickFindDelegate(PerformQuickFind),
				strSearch, strGroupName, false);

			m_bBlockQuickFind = false;
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
			//	UIUtil.SetFocus(m_tvGroups, this);
			//	bHandled = true;
			// }

			if(bHandled) { e.Handled = true; e.SuppressKeyPress = true; }
		}

		private void OnQuickFindKeyUp(object sender, KeyEventArgs e)
		{
			bool bHandled = false;

			if((e.KeyCode == Keys.Return) || (e.KeyCode == Keys.Enter))
				bHandled = true;
			// else if(e.KeyCode == Keys.Tab)
			//	bHandled = true;

			if(bHandled) { e.Handled = true; e.SuppressKeyPress = true; }
		}

		private void OnToolsOptions(object sender, EventArgs e)
		{
			OptionsForm ofDlg = new OptionsForm();
			ofDlg.InitEx(m_ilCurrentIcons);

			Program.Config.Application.MostRecentlyUsed.MaxItemCount = m_mruList.MaxItemCount;

			if(ofDlg.ShowDialog() == DialogResult.OK)
			{
				m_nLockTimerMax = (int)Program.Config.Security.WorkspaceLocking.LockAfterTime;
				m_nClipClearMax = Program.Config.Security.ClipboardClearAfterSeconds;

				// m_lvEntries.GridLines = Program.Config.MainWindow.ShowGridLines;

				UIUtil.SetAlternatingBgColors(m_lvEntries, m_clrAlternateItemBgColor,
					Program.Config.MainWindow.EntryListAlternatingBgColors);

				m_mruList.MaxItemCount = Program.Config.Application.MostRecentlyUsed.MaxItemCount;
				SetListFont(Program.Config.UI.StandardFont);

				if(ofDlg.RequiresUIReinitialize) UIUtil.Initialize(true);

				AppConfigSerializer.Save(Program.Config);
				UpdateTrayIcon();
			}
			UIUtil.DestroyForm(ofDlg);

			UpdateUI(false, null, true, null, true, null, false); // Fonts changed
		}

		private void OnPwListItemDrag(object sender, ItemDragEventArgs e)
		{
			if(e.Item == null) return;
			PwListItem pli = (((ListViewItem)e.Item).Tag as PwListItem);
			if(pli == null) { Debug.Assert(false); return; }
			PwEntry pe = pli.Entry;

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
					if(lvs == lvHit.SubItem)
					{
						bool bDummy;
						strText = GetEntryFieldEx(pe, i, false, out bDummy);
						break;
					}

					++i;
				}
			}

			m_pgActiveAtDragStart = GetSelectedGroup();

			PwDatabase pd = m_docMgr.SafeFindContainerOf(pe);
			string strToTransfer = SprEngine.Compile(strText, new SprContext(
				pe, pd, SprCompileFlags.All));

			m_bDraggingEntries = true;
			this.DoDragDrop(strToTransfer, DragDropEffects.Copy | DragDropEffects.Move);
			m_bDraggingEntries = false;

			pe.Touch(false);
			UpdateUIState(false); // SprEngine.Compile might have modified the database
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

						pgSelected.AddEntry(pe, true, true);

						vNowInvisible.Add(pe);
					}
				}
				else if(e.Effect == DragDropEffects.Copy)
				{
					foreach(PwEntry pe in vSelected)
					{
						PwEntry peCopy = pe.CloneDeep();
						peCopy.SetUuid(new PwUuid(true), true); // Create new UUID

						pgSelected.AddEntry(peCopy, true, true);
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

				pgSelected.AddGroup(pgDragged, true, true);

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
			PwGroup pg = (e.Node.Tag as PwGroup);

			if((pg != null) && (e.Label != null))
			{
				pg.Name = e.Label;
				pg.Touch(true, false);
				UpdateUIState(true);
			}
		}

		private void OnGroupsAdd(object sender, EventArgs e)
		{
			TreeNode tn = m_tvGroups.SelectedNode;
			PwGroup pgParent;

			if(tn != null) pgParent = (tn.Tag as PwGroup);
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

			PwGroup pg = (tn.Tag as PwGroup);
			if(pg == null) { Debug.Assert(false); return; }

			pg.IsExpanded = true;
		}

		private void OnFileSynchronize(object sender, EventArgs e)
		{
			bool? b = ImportUtil.Synchronize(m_docMgr.ActiveDatabase, this, false, this);
			UpdateUI(false, null, true, null, true, null, false);
			if(b.HasValue) SetStatusEx(b.Value ? KPRes.SyncSuccess : KPRes.SyncFailed);
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

		private void OnPwListColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
		{
			UpdateColumnsEx(true);
		}

		private void OnFormResize(object sender, EventArgs e)
		{
			FormWindowState ws = this.WindowState;

			if(ws == FormWindowState.Minimized)
			{
				// For default value, also see options dialog
				if(Program.Config.Security.WorkspaceLocking.LockOnWindowMinimize)
				{
					// if(!IsFileLocked(null)) // Not locked currently
					//	OnFileLock(sender, e); // Lock

					if(IsAtLeastOneFileOpen())
					{
						LockAllDocuments();
						if(m_bCleanedUp) return; // Exited instead of locking
					}
				}

				if(Program.Config.MainWindow.MinimizeToTray)
					MinimizeToTray(true);
			}
			else if(ws == FormWindowState.Maximized)
				Program.Config.MainWindow.Maximized = true;
			else if(ws == FormWindowState.Normal)
				Program.Config.MainWindow.Maximized = false;

			if((ws == FormWindowState.Normal) || (ws == FormWindowState.Maximized))
			{
				if(Program.Config.MainWindow.EntryListAutoResizeColumns &&
					(m_lvEntries.View == View.Details))
					UIUtil.ResizeColumns(m_lvEntries, true);

				if((m_fwsLast == FormWindowState.Minimized) && IsFileLocked(null) &&
					!UIIsAutoUnlockBlocked())
					OnFileLock(sender, e); // Unlock

				if((m_fwsLast == FormWindowState.Minimized) && !IsFileLocked(null) &&
					Program.Config.MainWindow.FocusQuickFindOnRestore)
					ResetDefaultFocus(null);
			}

			m_fwsLast = ws;
		}

		private void OnTrayTray(object sender, EventArgs e)
		{
			if(!IsCommandTypeInvokable(null, AppCommandType.Window)) return;

			if((this.WindowState == FormWindowState.Minimized) && !IsTrayed())
			{
				if(Program.Config.MainWindow.Maximized)
					UIUtil.SetWindowState(this, FormWindowState.Maximized);
				else UIUtil.SetWindowState(this, FormWindowState.Normal);
				return;
			}

			bool bTray = !IsTrayed();
			MinimizeToTray(bTray);
			if(!bTray) EnsureVisibleForegroundWindow(false, false);
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

			if(m_bUpdateUIStateOnce)
				UpdateUIState(false); // Resets m_bUpdateUIStateOnce

			DateTime utcNow = DateTime.UtcNow; // Fast; DateTime internally uses UTC

			// Update the global inactivity timeout in every main timer tick,
			// *before* checking the timeout (otherwise KeePass could be locked
			// even though the user did something within the last second)
			UpdateGlobalLockTimeout(utcNow);

			if(Monitor.TryEnter(m_objLockTimerSync))
			{
				if(IsAtLeastOneFileOpen() && GlobalWindowManager.CanCloseAllWindows)
				{
					long lCurTicks = utcNow.Ticks;
					if((lCurTicks >= m_lLockAtTicks) || (lCurTicks >= m_lLockAtGlobalTicks))
					{
						if(Program.Config.Security.WorkspaceLocking.ExitInsteadOfLockingAfterTime)
							OnFileExit(sender, e);
						else LockAllDocuments(); // Might exit instead of locking
					}
				}
				else NotifyUserActivity(); // Unclosable dialog = activity

				Monitor.Exit(m_objLockTimerSync);
			}

			GlobalMutexPool.Refresh();
		}

		private void OnToolsPlugins(object sender, EventArgs e)
		{
			if(!AppPolicy.Try(AppPolicyId.Plugins)) return;

			PluginsForm pf = new PluginsForm();
			pf.InitEx(m_pluginManager);

			UIUtil.ShowDialogAndDestroy(pf);
		}

		private void OnGroupsEdit(object sender, EventArgs e)
		{
			PwGroup pg = GetSelectedGroup();
			Debug.Assert(pg != null); if(pg == null) return;

			PwDatabase pwDb = m_docMgr.ActiveDatabase;
			GroupForm gf = new GroupForm();
			gf.InitEx(pg, m_ilCurrentIcons, pwDb);

			if(UIUtil.ShowDialogAndDestroy(gf) == DialogResult.OK)
				UpdateUI(false, null, true, null, true, null, true);
			else UpdateUI(false, null, pwDb.UINeedsIconUpdate, null,
				pwDb.UINeedsIconUpdate, null, false);
		}

		private void OnEntryCopyURL(object sender, EventArgs e)
		{
			PwEntry[] v = GetSelectedEntries();
			Debug.Assert(v != null); if(v == null) return;

			if(ClipboardUtil.CopyAndMinimize(UrlsToString(v, true), true,
				this, null, null))
				StartClipboardCountdown();
		}

		private void OnEntryMassSetIcon(object sender, EventArgs e)
		{
			PwEntry[] vEntries = GetSelectedEntries();
			if((vEntries == null) || (vEntries.Length == 0)) return;

			IconPickerForm ipf = new IconPickerForm();
			ipf.InitEx(m_ilCurrentIcons, (uint)PwIcon.Count, m_docMgr.ActiveDatabase,
				(uint)vEntries[0].IconId, vEntries[0].CustomIconUuid);
			ipf.ShowDialog();

			foreach(PwEntry pe in vEntries)
			{
				if(ipf.ChosenCustomIconUuid != PwUuid.Zero)
					pe.CustomIconUuid = ipf.ChosenCustomIconUuid;
				else
				{
					pe.IconId = (PwIcon)ipf.ChosenIconId;
					pe.CustomIconUuid = PwUuid.Zero;
				}

				pe.Touch(true, false);
			}

			bool bUpdImg = m_docMgr.ActiveDatabase.UINeedsIconUpdate;
			RefreshEntriesList();
			UpdateUI(false, null, bUpdImg, null, false, null, true);
			UIUtil.DestroyForm(ipf);
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
			SortPasswordList(true, e.Column, null, true);
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
					// case Keys.E: OnEntryEdit(sender, e); break;
					case Keys.U:
						// PerformDefaultUrlAction(null, false);
						if(e.Shift) OnEntryCopyURL(sender, e);
						else OnEntryOpenUrl(sender, e);
						break;
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

			if(!bUnhandled) { e.Handled = true; e.SuppressKeyPress = true; }
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
					// case Keys.E: break;
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

			if(!bUnhandled) { e.Handled = true; e.SuppressKeyPress = true; }
		}

		private void OnGroupsFind(object sender, EventArgs e)
		{
			PwGroup pg = GetSelectedGroup();
			Debug.Assert(pg != null); if(pg == null) return;

			SearchForm sf = new SearchForm();
			sf.InitEx(m_docMgr.ActiveDatabase, pg);

			if(sf.ShowDialog() == DialogResult.OK)
			{
				PwGroup pgResults = sf.SearchResultsGroup;
				UpdateEntryList(pgResults, false);
				UpdateUIState(false);
				ShowSearchResultsStatusMessage();
			}
			UIUtil.DestroyForm(sf);
		}

		private void OnViewTanSimpleListClick(object sender, EventArgs e)
		{
			m_bSimpleTanView = m_menuViewTanSimpleList.Checked;
			UpdateEntryList(null, true);
		}

		private void OnViewTanIndicesClick(object sender, EventArgs e)
		{
			m_bShowTanIndices = m_menuViewTanIndices.Checked;
			RefreshEntriesList();
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
					ProtectedString psNew;
					PwGenerator.Generate(out psNew, pgf.SelectedProfile,
						pbAdditionalEntropy, Program.PwGeneratorPool);
					psNew = psNew.WithProtection(pwDb.MemoryProtection.ProtectPassword);
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
			UIUtil.DestroyForm(pgf);
		}

		private void OnToolsShowExpired(object sender, EventArgs e)
		{
			ShowExpiredEntries(false, true, false);
		}

		private void OnToolsTanWizard(object sender, EventArgs e)
		{
			PwDatabase pwDb = m_docMgr.ActiveDatabase;
			if(!pwDb.IsOpen) { Debug.Assert(false); return; }

			PwGroup pgSelected = GetSelectedGroup();
			if(pgSelected == null) return;

			TanWizardForm twf = new TanWizardForm();
			twf.InitEx(pwDb, pgSelected);

			if(UIUtil.ShowDialogAndDestroy(twf) == DialogResult.OK)
				UpdateUI(false, null, false, null, true, null, true);
		}

		private void OnSystemTrayClick(object sender, EventArgs e)
		{
			if((m_mbLastTrayMouseButtons & MouseButtons.Left) != MouseButtons.None)
				if(Program.Config.UI.TrayIcon.SingleClickDefault)
					OnTrayTray(sender, e);
		}

		private void OnSystemTrayDoubleClick(object sender, EventArgs e)
		{
			if((m_mbLastTrayMouseButtons & MouseButtons.Left) != MouseButtons.None)
				if(!Program.Config.UI.TrayIcon.SingleClickDefault)
					OnTrayTray(sender, e);
		}

		private void OnEntryViewLinkClicked(object sender, LinkClickedEventArgs e)
		{
			string strLink = e.LinkText;
			if(string.IsNullOrEmpty(strLink)) { Debug.Assert(false); return; }

			PwEntry pe = GetSelectedEntry(false);
			ProtectedBinary pb = ((pe != null) ? pe.Binaries.Get(strLink) : null);

			string strEntryUrl = string.Empty;
			if(pe != null)
				strEntryUrl = SprEngine.Compile(pe.Strings.ReadSafe(PwDefs.UrlField),
					GetEntryListSprContext(pe, m_docMgr.SafeFindContainerOf(pe)));

			if((pe != null) && (pe.ParentGroup != null) &&
				(pe.ParentGroup.Name == strLink))
			{
				UpdateUI(false, null, true, pe.ParentGroup, true, null, false);
				EnsureVisibleSelected(false);
				ResetDefaultFocus(m_lvEntries);
			}
			else if(strEntryUrl == strLink)
				PerformDefaultUrlAction(null, false);
			else if(pb != null)
				ExecuteBinaryEditView(strLink, pb);
			else if(strLink.StartsWith(SprEngine.StrRefStart, StrUtil.CaseIgnoreCmp) &&
				strLink.EndsWith(SprEngine.StrRefEnd, StrUtil.CaseIgnoreCmp))
			{
				// If multiple references are amalgamated, only use first one
				string strFirstRef = strLink;
				int iEnd = strLink.IndexOf(SprEngine.StrRefEnd, StrUtil.CaseIgnoreCmp);
				if(iEnd != (strLink.Length - SprEngine.StrRefEnd.Length))
					strFirstRef = strLink.Substring(0, iEnd + 1);

				char chScan, chWanted;
				PwEntry peRef = SprEngine.FindRefTarget(strFirstRef, GetEntryListSprContext(
					pe, m_docMgr.SafeFindContainerOf(pe)), out chScan, out chWanted);
				if(peRef != null)
				{
					UpdateUI(false, null, true, peRef.ParentGroup, true, null,
						false, m_lvEntries);
					PwObjectList<PwEntry> lSel = new PwObjectList<PwEntry>();
					lSel.Add(peRef);
					SelectEntries(lSel, true, true);
					EnsureVisibleSelected(false);
					ShowEntryDetails(peRef);
				}
			}
			else WinUtil.OpenUrl(strLink, pe);
		}

		private void OnEntryClipCopy(object sender, EventArgs e)
		{
			if(!m_docMgr.ActiveDatabase.IsOpen) return;
			PwEntry[] vSelected = GetSelectedEntries();
			if((vSelected == null) || (vSelected.Length == 0)) return;

			if(!AppPolicy.Try(AppPolicyId.CopyWholeEntries)) return;

			try
			{
				EntryUtil.CopyEntriesToClipboard(m_docMgr.ActiveDatabase, vSelected,
					this.Handle);
			}
			catch(Exception exCopy)
			{
				MessageService.ShowWarning(exCopy);
			}

			StartClipboardCountdown();
		}

		private void OnEntryClipPaste(object sender, EventArgs e)
		{
			if(!m_docMgr.ActiveDatabase.IsOpen) return;
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
			Color clrCur = ((pe != null) ? pe.BackgroundColor : Color.Empty);

			Color? clr = UIUtil.ShowColorDialog(clrCur);
			if(clr.HasValue) SetSelectedEntryColor(clr.Value);
		}

		private void OnPwListMouseDown(object sender, MouseEventArgs e)
		{
			m_ptLastEntriesMouseClick = e.Location;
		}

		private void OnToolsDbMaintenance(object sender, EventArgs e)
		{
			if(!m_docMgr.ActiveDatabase.IsOpen) return;

			DatabaseOperationsForm form = new DatabaseOperationsForm();
			form.InitEx(m_docMgr.ActiveDatabase);
			form.ShowDialog();

			// UpdateUIState(form.HasModifiedDatabase);
			bool bMod = form.HasModifiedDatabase;
			UpdateUI(false, null, bMod, null, bMod, null, bMod);
			UIUtil.DestroyForm(form);
		}

		private void OnCtxPwListOpening(object sender, CancelEventArgs e)
		{
			MainAppState s = UpdateUIEntryCtxState(null);

			m_dynCustomStrings.Clear();
			m_dynCustomBinaries.Clear();

			if(s.EntriesSelected != 1)
			{
				m_ctxEntryCopyCustomString.Visible = false;
				m_ctxEntryAttachments.Visible = false;
				return;
			}

			PwEntry pe = s.SelectedEntry;
			if(pe == null) { Debug.Assert(false); return; }

			uint uStrItems = 0;
			foreach(KeyValuePair<string, ProtectedString> kvp in pe.Strings)
			{
				if(PwDefs.IsStandardField(kvp.Key)) continue;

				m_dynCustomStrings.AddItem(kvp.Key,
					Properties.Resources.B16x16_KGPG_Info);
				++uStrItems;
			}
			m_ctxEntryCopyCustomString.Visible = (uStrItems > 0);

			LinkedList<KeyValuePair<string, Image>> lEditableBinaries =
				new LinkedList<KeyValuePair<string, Image>>();

			uint uBinItems = 0;
			foreach(KeyValuePair<string, ProtectedBinary> kvp in pe.Binaries)
			{
				Image imgIcon;

				// Try a fast classification
				BinaryDataClass bdc = BinaryDataClassifier.ClassifyUrl(kvp.Key);
				if((bdc == BinaryDataClass.Text) || (bdc == BinaryDataClass.RichText))
					imgIcon = Properties.Resources.B16x16_ASCII;
				else if(bdc == BinaryDataClass.Image)
					imgIcon = Properties.Resources.B16x16_Spreadsheet;
				else if(bdc == BinaryDataClass.WebDocument)
					imgIcon = Properties.Resources.B16x16_HTML;
				else imgIcon = Properties.Resources.B16x16_Binary;

				m_dynCustomBinaries.AddItem(kvp.Key, imgIcon);
				++uBinItems;

				if(DataEditorForm.SupportsDataType(bdc))
					lEditableBinaries.AddLast(new KeyValuePair<string, Image>(
						kvp.Key, imgIcon));
			}

			if(lEditableBinaries.Count > 0)
			{
				m_dynCustomBinaries.AddSeparator();

				foreach(KeyValuePair<string, Image> kvpEdit in lEditableBinaries)
					m_dynCustomBinaries.AddItem(string.Format(KPRes.EditObject + "...",
						kvpEdit.Key), kvpEdit.Value, new EditableBinaryAttachment(
						kvpEdit.Key));
			}

			m_ctxEntryAttachments.Visible = (uBinItems > 0);
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

						ProtectedString psNew;
						PwGenerator.Generate(out psNew, pgf.SelectedProfile,
							pbAdditionalEntropy, Program.PwGeneratorPool);
						psNew = psNew.WithProtection(pwDb.MemoryProtection.ProtectPassword);
						pe.Strings.Set(PwDefs.PasswordField, psNew);
					}

					UpdateUI(false, null, false, null, true, null, true);
				}
				UIUtil.DestroyForm(dlgCount);
			}
			UIUtil.DestroyForm(pgf);
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
			UIUtil.ShowDialogAndDestroy(hsf);
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
			bool? bResult = ImportUtil.Import(pwDb, out bAppendedToRootOnly, this);
			bool bModified = (bResult.HasValue ? bResult.Value : false);

			if(bAppendedToRootOnly && pwDb.IsOpen)
			{
				UpdateUI(false, null, true, pwDb.RootGroup, true, null, bModified);

				if(m_lvEntries.Items.Count > 0)
					m_lvEntries.EnsureVisible(m_lvEntries.Items.Count - 1);
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

			if(!bUnhandled) { e.Handled = true; e.SuppressKeyPress = true; }
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
				(e.KeyCode == Keys.Left) || (e.KeyCode == Keys.Right) ||
				(e.KeyCode == Keys.Home) || (e.KeyCode == Keys.End) ||
				((e.KeyCode >= Keys.A) && (e.KeyCode <= Keys.Z)) ||
				((e.KeyCode >= Keys.D0) && (e.KeyCode <= Keys.D9)) ||
				((e.KeyCode >= Keys.NumPad0) && (e.KeyCode <= Keys.NumPad9)))
			{
				UpdateUI(false, null, false, null, true, null, false);
			}
			else bUnhandled = true;

			if(!bUnhandled) { e.Handled = true; e.SuppressKeyPress = true; }
		}

		private void OnEntryUrlOpenInInternal(object sender, EventArgs e)
		{
			InternalBrowserForm ibf = new InternalBrowserForm();

			string strUrl = string.Empty;
			PwEntry pe = GetSelectedEntry(false);
			if(pe != null) strUrl = pe.Strings.ReadSafe(PwDefs.UrlField);

			ibf.InitEx(strUrl, m_docMgr.ActiveDatabase.RootGroup);
			UIUtil.ShowDialogAndDestroy(ibf);
		}

		private void OnTabMainSelectedIndexChanged(object sender, EventArgs e)
		{
			if(m_bBlockTabChanged) return;

			SaveWindowState();

			TabPage tbSelect = m_tabMain.SelectedTab;
			if(tbSelect == null) return;

			PwDocument ds = (tbSelect.Tag as PwDocument);
			MakeDocumentActive(ds);

			if(IsFileLocked(ds)) OnFileLock(sender, e); // Unlock
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
			bool? b = ImportUtil.Synchronize(m_docMgr.ActiveDatabase, this, true, this);
			UpdateUI(false, null, true, null, true, null, false);
			if(b.HasValue) SetStatusEx(b.Value ? KPRes.SyncSuccess : KPRes.SyncFailed);
		}

		private void OnCtxTrayOpening(object sender, CancelEventArgs e)
		{
			UpdateTrayState();
		}

		private void OnTrayLock(object sender, EventArgs e)
		{
			if(!IsCommandTypeInvokable(null, AppCommandType.Lock)) return;
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

		private void OnSystemTrayMouseDown(object sender, MouseEventArgs e)
		{
			m_mbLastTrayMouseButtons = e.Button;
		}

		private void OnFormActivated(object sender, EventArgs e)
		{
			NotifyUserActivity();

			if(m_vRedirectActivation.Count > 0)
			{
				Form f = m_vRedirectActivation.Peek();
				if(f != null) f.Activate();
				// SystemSounds.Beep.Play(); // Do not beep!
			}
		}

		private void OnToolsTriggers(object sender, EventArgs e)
		{
			EcasTriggersForm f = new EcasTriggersForm();
			if(!f.InitEx(Program.TriggerSystem, m_ilCurrentIcons)) return;
			UIUtil.ShowDialogAndDestroy(f);
		}

		private void OnTabMainMouseClick(object sender, MouseEventArgs e)
		{
			if(e.Button == MouseButtons.Middle)
			{
				for(int i = 0; i < m_tabMain.TabCount; ++i)
				{
					if(m_tabMain.GetTabRect(i).Contains(e.Location))
					{
						PwDocument pd = (m_tabMain.TabPages[i].Tag as PwDocument);
						if(pd == null) { Debug.Assert(false); return; }
						CloseDocument(pd, false, false);
						break;
					}
				}
			}
		}

		private void OnViewConfigColumns(object sender, EventArgs e)
		{
			ColumnsForm dlg = new ColumnsForm();
			if(UIUtil.ShowDialogAndDestroy(dlg) == DialogResult.OK)
			{
				UpdateColumnsEx(false);
				UpdateUI(false, null, false, null, true, null, false);
			}
		}

		private void OnEditShowByTagOpening(object sender, EventArgs e)
		{
			UpdateTagsMenu(m_dynShowEntriesByTagsEditMenu, false, false, false, false);
		}

		private void OnEntryViewsByTagOpening(object sender, EventArgs e)
		{
			UpdateTagsMenu(m_dynShowEntriesByTagsToolBar, true, true, false, false);
		}

		private void OnEntrySelectedNewTag(object sender, EventArgs e)
		{
			OnAddEntryTag(sender, new DynamicMenuEventArgs(string.Empty, string.Empty));
		}

		private void OnEntrySelectedAddTagOpening(object sender, EventArgs e)
		{
			UpdateTagsMenu(m_dynAddTag, true, false, true, false);
		}

		private void OnEntrySelectedRemoveTagOpening(object sender, EventArgs e)
		{
			UpdateTagsMenu(m_dynRemoveTag, false, false, true, true);
		}

		private void OnCtxEntryClipboardOpening(object sender, EventArgs e)
		{
			try // Might fail/throw due to clipboard access timeout
			{
				m_ctxEntryClipPaste.Enabled = Clipboard.ContainsData(
					EntryUtil.ClipFormatEntries);
			}
			catch(Exception) { m_ctxEntryClipPaste.Enabled = false; }
		}

		private void OnMenuEditOpening(object sender, EventArgs e)
		{
			MainAppState s = UpdateUIGroupCtxState(null);
			UpdateUIEntryCtxState(s);
			UpdateLinkedMenuItems();
		}

		private void OnCtxGroupListOpening(object sender, CancelEventArgs e)
		{
			UpdateUIGroupCtxState(null);
		}

		private void OnGroupsSort(object sender, EventArgs e)
		{
			SortSubGroups(false);
		}

		private void OnGroupsSortRec(object sender, EventArgs e)
		{
			SortSubGroups(true);
		}

		private void OnPwListClick(object sender, EventArgs e)
		{
			// The following ensures a faster update when multiple items
			// are automatically deselected when clicking on another item
			UpdateUIState(false);
		}

		private void OnGroupsEmpty(object sender, EventArgs e)
		{
			EmptyRecycleBin();
		}

		private void OnToolsDelDupEntries(object sender, EventArgs e)
		{
			PwDatabase pd = m_docMgr.ActiveDatabase;
			if((pd == null) || !pd.IsOpen) { Debug.Assert(false); return; }

			Form fOptDialog;
			IStatusLogger sl = StatusUtil.CreateStatusDialog(this, out fOptDialog,
				null, KPRes.Delete + "...", true, false);
			if(fOptDialog != null) RedirectActivationPush(fOptDialog);
			UIBlockInteraction(true);

			uint uDeleted = pd.DeleteDuplicateEntries(sl);

			if(fOptDialog != null) RedirectActivationPop();
			UIBlockInteraction(false);
			sl.EndLogging();

			UpdateUI(false, null, false, null, (uDeleted > 0), null, (uDeleted > 0));
			SetObjectsDeletedStatus(uDeleted, true);
		}

		private void OnToolsDelEmptyGroups(object sender, EventArgs e)
		{
			PwDatabase pd = m_docMgr.ActiveDatabase;
			if((pd == null) || !pd.IsOpen) { Debug.Assert(false); return; }

			uint uDeleted = pd.DeleteEmptyGroups();
			UpdateUI(false, null, (uDeleted > 0), null, false, null, (uDeleted > 0));
			SetObjectsDeletedStatus(uDeleted, true);
		}

		private void OnToolsDelUnusedIcons(object sender, EventArgs e)
		{
			PwDatabase pd = m_docMgr.ActiveDatabase;
			if((pd == null) || !pd.IsOpen) { Debug.Assert(false); return; }

			uint uDeleted = pd.DeleteUnusedCustomIcons();
			UpdateUI(false, null, (uDeleted > 0), null, (uDeleted > 0),
				null, (uDeleted > 0));
			SetObjectsDeletedStatus(uDeleted, true);
		}

		private void OnTrayExit(object sender, EventArgs e)
		{
			if(!IsCommandTypeInvokable(null, AppCommandType.Window)) return;
			OnFileExit(sender, e);
		}

		private void OnTrayOptions(object sender, EventArgs e)
		{
			if(!IsCommandTypeInvokable(null, AppCommandType.Window)) return;
			EnsureVisibleForegroundWindow(false, false); // Parent is main window, not tray
			OnToolsOptions(sender, e);
		}
	}
}
