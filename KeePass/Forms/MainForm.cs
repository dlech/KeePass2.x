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
using System.Text;
using System.Threading;
using System.Windows.Forms;

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
using KeePassLib.Keys;
using KeePassLib.Security;
using KeePassLib.Serialization;
using KeePassLib.Utility;

using NativeLib = KeePassLib.Native.NativeLib;

namespace KeePass.Forms
{
	/// <summary>
	/// KeePass main window.
	/// </summary>
	public partial class MainForm : Form, IMruExecuteHandler, IUIOperations
	{
		private NotifyIconEx m_ntfTray = null;

		private bool m_bFormLoaded = false;
		private bool m_bFormShown = false;
		private bool m_bCleanedUp = false;

		private bool m_bRestart = false;
		private ListSorter m_pListSorter = new ListSorter();
		private ListViewSortMenu m_lvsmMenu = null;
		private ListViewGroupingMenu m_lvgmMenu = null;

		private bool m_bDraggingGroup = false;
		private bool m_bDraggingEntries = false;

		private bool m_bBlockColumnUpdates = false;
		private uint m_uBlockEntrySelectionEvent = 0;

		private bool m_bForceExitOnce = false;

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
				Debug.Assert(NativeLib.IsUnix());
				m_nTaskbarButtonMessage = 0x1E8F46A7; // Unlikely to occur
				m_bTaskbarButtonMessage = false;
			}

			string strIso6391 = Program.Translation.Properties.Iso6391Code;
			if(!string.IsNullOrEmpty(strIso6391))
			{
				try
				{
					CultureInfo ci = CultureInfo.CreateSpecificCulture(strIso6391);
					if(ci != null)
					{
						Application.CurrentCulture = ci;
						Thread.CurrentThread.CurrentCulture = ci;
						Thread.CurrentThread.CurrentUICulture = ci;
						Properties.Resources.Culture = ci;
					}
				}
				catch(Exception) { Debug.Assert(false); }
			}

			UIUtil.Initialize(false);

			InitializeComponent();

			m_asyncListUpdate = new AsyncPwListUpdate(m_lvEntries);

			m_splitHorizontal.InitEx(this.Controls, m_menuMain);
			m_splitVertical.InitEx(this.Controls, m_menuMain);

			if(!Program.DesignMode)
			{
				if(MonoWorkarounds.IsRequired(891029)) m_tabMain.Height += 5;

				m_menuMain.SuspendLayout();
				m_ctxGroupList.SuspendLayout();
				m_ctxPwList.SuspendLayout();
				m_ctxTray.SuspendLayout();

				GlobalWindowManager.InitializeForm(this);
				Program.Translation.ApplyTo("KeePass.Forms.MainForm.m_menuMain", m_menuMain.Items);
				Program.Translation.ApplyTo("KeePass.Forms.MainForm.m_ctxPwList", m_ctxPwList.Items);
				Program.Translation.ApplyTo("KeePass.Forms.MainForm.m_ctxGroupList", m_ctxGroupList.Items);
				Program.Translation.ApplyTo("KeePass.Forms.MainForm.m_ctxTray", m_ctxTray.Items);

				AssignMenuShortcuts();
				// Do not construct context menus here, otherwise TrlUtil sees them

				m_ctxTray.ResumeLayout(true);
				m_ctxPwList.ResumeLayout(true);
				m_ctxGroupList.ResumeLayout(true);
				m_menuMain.ResumeLayout(true);
			}
		}

		private bool m_bFormLoadCalled = false;
		private void OnFormLoad(object sender, EventArgs e)
		{
			if(m_bFormLoadCalled && MonoWorkarounds.IsRequired(3574233558U)) return;
			m_bFormLoadCalled = true;

			m_bFormLoaded = false;
			GlobalWindowManager.CustomizeControl(this);
			GlobalWindowManager.CustomizeFormHandleCreated(this, true, true);

			this.Text = PwDefs.ShortProductName;
			this.Icon = AppIcons.Default;
			m_imgFileSaveEnabled = Properties.Resources.B16x16_FileSave;
			m_imgFileSaveDisabled = Properties.Resources.B16x16_FileSave_Disabled;
			// m_imgFileSaveAllEnabled = Properties.Resources.B16x16_File_SaveAll;
			// m_imgFileSaveAllDisabled = Properties.Resources.B16x16_File_SaveAll_Disabled;

#if DEBUG
			ConstructDebugMenu();
#endif
			ConstructContextMenus();

			// m_ilCurrentIcons = m_ilClientIcons;
			UpdateImageLists(true);

			m_ntfTray = new NotifyIconEx(this.components);
			m_ntfTray.ContextMenuStrip = m_ctxTray;
			m_ntfTray.Visible = true;
			m_ntfTray.SetHandlers(this.OnSystemTrayClick, this.OnSystemTrayDoubleClick,
				this.OnSystemTrayMouseDown);

			m_ctxTrayTray.Font = FontUtil.CreateFont(m_ctxTrayTray.Font, FontStyle.Bold);

			m_nLockTimerMax = (int)Program.Config.Security.WorkspaceLocking.LockAfterTime;
			m_nClipClearMax = Program.Config.Security.ClipboardClearAfterSeconds;

			NativeLib.AllowNative = Program.Config.Native.NativeKeyTransformations;

			m_ctxEntryPreviewContextMenu.Attach(m_richEntryView, this);

			m_dynStringsMenu = new DynamicMenu(m_menuEntryCopyString.DropDownItems);
			m_dynStringsMenu.MenuClick += this.OnCopyCustomString;
			m_dynStringsCtx = new DynamicMenu(m_ctxEntryCopyString.DropDownItems);
			m_dynStringsCtx.MenuClick += this.OnCopyCustomString;

			m_dynBinariesMenu = new DynamicMenu(m_menuEntryAttachments.DropDownItems);
			m_dynBinariesMenu.MenuClick += this.OnEntryBinaryOpen;
			m_dynBinariesCtx = new DynamicMenu(m_ctxEntryAttachments.DropDownItems);
			m_dynBinariesCtx.MenuClick += this.OnEntryBinaryOpen;

			m_dynFindTagsMenu = new DynamicMenu(m_menuFindTag.DropDownItems);
			m_dynFindTagsMenu.MenuClick += this.OnShowEntriesByTag;
			m_dynFindTagsToolBar = new DynamicMenu(m_tbEntryViewsDropDown.DropDownItems);
			m_dynFindTagsToolBar.MenuClick += this.OnShowEntriesByTag;

			m_dynTagAddMenu = new DynamicMenu(m_menuEntryTagAdd.DropDownItems);
			m_dynTagAddMenu.MenuClick += this.OnAddEntryTag;
			m_dynTagAddCtx = new DynamicMenu(m_ctxEntryTagAdd.DropDownItems);
			m_dynTagAddCtx.MenuClick += this.OnAddEntryTag;

			m_dynTagRemoveMenu = new DynamicMenu(m_menuEntryTagRemove.DropDownItems);
			m_dynTagRemoveMenu.MenuClick += this.OnRemoveEntryTag;
			m_dynTagRemoveCtx = new DynamicMenu(m_ctxEntryTagRemove.DropDownItems);
			m_dynTagRemoveCtx.MenuClick += this.OnRemoveEntryTag;

			m_dynMoveToGroupMenu = new DynamicMenu(m_menuEntryMoveToGroup.DropDownItems);
			m_dynMoveToGroupMenu.MenuClick += this.OnEntryMoveToGroup;
			m_dynMoveToGroupCtx = new DynamicMenu(m_ctxEntryMoveToGroup.DropDownItems);
			m_dynMoveToGroupCtx.MenuClick += this.OnEntryMoveToGroup;

			m_dynAutoTypeAdvMenu = new DynamicMenu(m_menuEntryAutoTypeAdv.DropDownItems);
			m_dynAutoTypeAdvMenu.MenuClick += this.OnEntryPerformAutoTypeAdv;
			m_dynAutoTypeAdvCtx = new DynamicMenu(m_ctxEntryAutoTypeAdv.DropDownItems);
			m_dynAutoTypeAdvCtx.MenuClick += this.OnEntryPerformAutoTypeAdv;

			string[] vAdvSeq = new string[] {
				@"{USERNAME}", @"{USERNAME}{ENTER}",
				@"{PASSWORD}", @"{PASSWORD}{ENTER}",
				@"{USERNAME}{TAB}{PASSWORD}",
				@"{USERNAME}{TAB}{PASSWORD}{ENTER}",
				@"{USERNAME}{TAB}{TAB}{PASSWORD}",
				@"{USERNAME}{TAB}{TAB}{PASSWORD}{ENTER}"
			};
			Bitmap bmpAutoType = Properties.Resources.B16x16_KTouch;
			foreach(string strAdvSeq in vAdvSeq)
			{
				m_dynAutoTypeAdvMenu.AddItem(strAdvSeq, bmpAutoType);
				m_dynAutoTypeAdvCtx.AddItem(strAdvSeq, bmpAutoType);
			}

			m_dynOpenUrlMenu = new OpenWithMenu(m_menuEntryUrl);
			m_dynOpenUrlCtx = new OpenWithMenu(m_ctxEntryUrl);
			m_dynOpenUrlToolBar = new OpenWithMenu(m_tbOpenUrl);

			EntryTemplates.Init(m_tbAddEntry);

			UIUtil.ConfigureTbButton(m_tbNewDatabase, KPRes.ToolBarNew, null, m_menuFileNew);
			UIUtil.ConfigureTbButton(m_tbOpenDatabase, KPRes.ToolBarOpen, null, m_menuFileOpenLocal);
			UIUtil.ConfigureTbButton(m_tbSaveDatabase, KPRes.Save, null, m_menuFileSave);
			UIUtil.ConfigureTbButton(m_tbSaveAll, KPRes.ToolBarSaveAll, null, null);
			UIUtil.ConfigureTbButton(m_tbAddEntry, KPRes.AddEntry, null, m_menuEntryAdd);
			UIUtil.ConfigureTbButton(m_tbCopyUserName, KPRes.CopyUserName, null, m_menuEntryCopyUserName);
			UIUtil.ConfigureTbButton(m_tbCopyPassword, KPRes.CopyPasswordMenu, null, m_menuEntryCopyPassword);
			UIUtil.ConfigureTbButton(m_tbOpenUrl, KPRes.OpenUrl, null, m_menuEntryOpenUrl);
			UIUtil.ConfigureTbButton(m_tbCopyUrl, KPRes.CopyUrls, null, m_menuEntryCopyUrl);
			UIUtil.ConfigureTbButton(m_tbAutoType, KPRes.PerformAutoType, null, m_menuEntryPerformAutoType);
			UIUtil.ConfigureTbButton(m_tbFind, KPRes.Find, null, m_menuFindInDatabase);
			UIUtil.ConfigureTbButton(m_tbEntryViewsDropDown, KPRes.FindEntries, null, null);
			UIUtil.ConfigureTbButton(m_tbLockWorkspace, KPRes.LockMenuLock, null, m_menuFileLock);
			UIUtil.ConfigureTbButton(m_tbQuickFind, null, KPRes.SearchQuickPrompt +
				" (" + KPRes.KeyboardKeyCtrl + "+E)", null);
			UIUtil.ConfigureTbButton(m_tbCloseTab, KPRes.Close, null, m_menuFileClose);

			CopyMenuItemText(m_tbAddEntryDefault, m_menuEntryAdd, null);
			CopyMenuItemText(m_tbOpenUrlDefault, m_menuEntryOpenUrl, KPRes.OpenUrl);
			CopyMenuItemText(m_tbViewsShowAll, m_menuFindAll, null);
			CopyMenuItemText(m_tbViewsShowExpired, m_menuFindExp, null);

			UIUtil.EnableAutoCompletion(m_tbQuickFind, false);

			bool bVisible = Program.Config.MainWindow.ToolBar.Show;
			m_toolMain.Visible = bVisible;
			UIUtil.SetChecked(m_menuViewShowToolBar, bVisible);

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
			{
				if(MonoWorkarounds.IsRequired(686017))
				{
					sizeX = Math.Max(250, sizeX);
					sizeY = Math.Max(250, sizeY);
				}

				this.Size = new Size(sizeX, sizeY);
			}
			if(MonoWorkarounds.IsRequired(686017))
				this.MinimumSize = new Size(250, 250);

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

			m_bSimpleTanView = mw.TanView.UseSimpleView;
			UIUtil.SetChecked(m_menuViewTanSimpleList, m_bSimpleTanView);
			m_bShowTanIndices = mw.TanView.ShowIndices;
			UIUtil.SetChecked(m_menuViewTanIndices, m_bShowTanIndices);

			UIUtil.SetChecked(m_menuViewShowEntriesOfSubGroups,
				Program.Config.MainWindow.ShowEntriesOfSubGroups);

			CustomContextMenuStripEx ctxHeader = new CustomContextMenuStripEx();
			ToolStripMenuItem tsmiCfgCol = new ToolStripMenuItem(m_menuViewConfigColumns.Text,
				m_menuViewConfigColumns.Image, new EventHandler(this.OnViewConfigColumns));
			ctxHeader.Items.Add(tsmiCfgCol);
			m_lvEntries.HeaderContextMenuStrip = ctxHeader;

			m_pListSorter = Program.Config.MainWindow.ListSorting;
			if((m_pListSorter.Column >= 0) && (m_pListSorter.Order != SortOrder.None))
				m_lvEntries.ListViewItemSorter = m_pListSorter;
			else m_pListSorter = new ListSorter();

			m_lvsmMenu = new ListViewSortMenu(m_menuViewSortBy, m_lvEntries,
				new SortCommandHandler(this.SortPasswordList));
			m_lvgmMenu = new ListViewGroupingMenu(m_menuViewEntryListGrouping, this);

			if(MonoWorkarounds.IsRequired(1716) && (NativeLib.GetDesktopType() ==
				DesktopType.Cinnamon))
			{
				mw.AlwaysOnTop = false;
				UIUtil.SetEnabledFast(false, m_menuViewAlwaysOnTop);
			}
			UIUtil.SetChecked(m_menuViewAlwaysOnTop, mw.AlwaysOnTop);
			EnsureAlwaysOnTopOpt();

			m_mruList.Initialize(this, m_menuFileRecent, m_menuFileSyncRecent);
			m_mruList.MarkOpened = true;
			SerializeMruList(false);

			SetListFont(Program.Config.UI.StandardFont);

			int w = DpiUtil.ScaleIntX(16), h = DpiUtil.ScaleIntY(16);
			Image imgC = UIUtil.CreateColorBitmap24(w, h, AppDefs.NamedEntryColor.LightRed);
			m_milMain.SetImage(m_menuEntryColorLightRed, imgC);
			imgC = UIUtil.CreateColorBitmap24(w, h, AppDefs.NamedEntryColor.LightGreen);
			m_milMain.SetImage(m_menuEntryColorLightGreen, imgC);
			imgC = UIUtil.CreateColorBitmap24(w, h, AppDefs.NamedEntryColor.LightBlue);
			m_milMain.SetImage(m_menuEntryColorLightBlue, imgC);
			imgC = UIUtil.CreateColorBitmap24(w, h, AppDefs.NamedEntryColor.LightYellow);
			m_milMain.SetImage(m_menuEntryColorLightYellow, imgC);

			Debug.Assert(!m_tvGroups.ShowRootLines); // See designer
			// m_lvEntries.GridLines = mw.ShowGridLines;
			if(UIUtil.VistaStyleListsSupported)
			{
				// m_tvGroups.ItemHeight += 1;
				// m_tvGroups.ShowLines = false; // Option-dep., see CustomTreeViewEx

				UIUtil.SetExplorerTheme(m_tvGroups.Handle);
				UIUtil.SetExplorerTheme(m_lvEntries.Handle);
			}

			// m_tvGroups.QueryToolTip = UIUtil.GetPwGroupToolTipTN;

			UpdateAlternatingBgColor();

			m_statusPartProgress.Visible = false;

			if(bMaximizedSetting)
			{
				if((this.WindowState == FormWindowState.Normal) && !IsTrayed())
				{
					// bool bVis = this.Visible;
					// if(bVis) this.Visible = false;

					UIUtil.SetWindowState(this, FormWindowState.Maximized);

					// if(bVis) this.Visible = true;
				}
			}

			try
			{
				double dSplitPos = mw.SplitterHorizontalFrac;
				if(dSplitPos == double.Epsilon) dSplitPos = 0.8333;
				if(MonoWorkarounds.IsRequired(686017))
					m_splitHorizontal.Panel1MinSize = 35;
				m_splitHorizontal.SplitterDistanceFrac = dSplitPos;

				dSplitPos = mw.SplitterVerticalFrac;
				if(dSplitPos == double.Epsilon) dSplitPos = 0.25;
				m_splitVertical.SplitterDistanceFrac = dSplitPos;
			}
			catch(Exception) { Debug.Assert(false); }

			string strSearchTr = ((WinUtil.IsAtLeastWindowsVista ?
				string.Empty : " ") + KPRes.Search);
			UIUtil.SetCueBanner(m_tbQuickFind, strSearchTr);

#if DEBUG
			Program.Config.CustomConfig.SetBool("TestItem1", true);
			Program.Config.CustomConfig.SetULong("TestItem2", 13);
			Program.Config.CustomConfig.SetString("TestItem3", "TestValue");

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
			{
				ToolStripItemCollection tsicM = m_menuTools.DropDownItems;
				int ciM = tsicM.Count;
				ToolStripItem tsiPrevM = ((ciM >= 1) ? tsicM[ciM - 1] : null);

				ToolStripItemCollection tsicGM = m_menuGroup.DropDownItems;
				int ciGM = tsicGM.Count;
				ToolStripItem tsiPrevGM = ((ciGM >= 1) ? tsicGM[ciGM - 1] : null);

				ToolStripItemCollection tsicEM = m_menuEntry.DropDownItems;
				int ciEM = tsicEM.Count;
				ToolStripItem tsiPrevEM = ((ciEM >= 1) ? tsicEM[ciEM - 1] : null);

				ToolStripItemCollection tsicGC = m_ctxGroupList.Items;
				int ciGC = tsicGC.Count;
				ToolStripItem tsiPrevGC = ((ciGC >= 1) ? tsicGC[ciGC - 1] : null);

				ToolStripItemCollection tsicEC = m_ctxPwList.Items;
				int ciEC = tsicEC.Count;
				ToolStripItem tsiPrevEC = ((ciEC >= 1) ? tsicEC[ciEC - 1] : null);

				ToolStripItemCollection tsicT = m_ctxTray.Items;
				ToolStripItem tsiPrevT = m_ctxTrayOptions;

				m_pluginManager.LoadAllPlugins();

				m_pluginManager.AddMenuItems(PluginMenuType.Main, tsicM, tsiPrevM);
				m_pluginManager.AddMenuItems(PluginMenuType.Group, tsicGM, tsiPrevGM);
				m_pluginManager.AddMenuItems(PluginMenuType.Entry, tsicEM, tsiPrevEM);
				m_pluginManager.AddMenuItems(PluginMenuType.Group, tsicGC, tsiPrevGC);
				m_pluginManager.AddMenuItems(PluginMenuType.Entry, tsicEC, tsiPrevEC);
				m_pluginManager.AddMenuItems(PluginMenuType.Tray, tsicT, tsiPrevT);
			}

			// Delete old files *after* loading plugins (when timestamps
			// of loaded plugins have been updated already)
			if(Program.Config.Application.Start.PluginCacheDeleteOld)
				PlgxCache.DeleteOldFilesAsync();

			if(Program.ColumnProviderPool.Count != nInitColProvCount)
				UpdateColumnsEx(false);

			HotKeyManager.Initialize(this);

			HotKeyManager.RegisterHotKey(AppDefs.GlobalHotKeyId.AutoType,
				(Keys)Program.Config.Integration.HotKeyGlobalAutoType);
			HotKeyManager.RegisterHotKey(AppDefs.GlobalHotKeyId.AutoTypePassword,
				(Keys)Program.Config.Integration.HotKeyGlobalAutoTypePassword);
			HotKeyManager.RegisterHotKey(AppDefs.GlobalHotKeyId.AutoTypeSelected,
				(Keys)Program.Config.Integration.HotKeySelectedAutoType);
			HotKeyManager.RegisterHotKey(AppDefs.GlobalHotKeyId.ShowWindow,
				(Keys)Program.Config.Integration.HotKeyShowWindow);
			HotKeyManager.RegisterHotKey(AppDefs.GlobalHotKeyId.EntryMenu,
				(Keys)Program.Config.Integration.HotKeyEntryMenu);

			m_statusClipboard.Visible = false;
			UpdateClipboardStatus();

			ToolStripItem[] vSbItems = new ToolStripItem[] {
				m_statusPartSelected, m_statusPartProgress, m_statusClipboard };
			int[] vStdSbWidths = new int[] { 140, 150, 100 };
			DpiUtil.ScaleToolStripItems(vSbItems, vStdSbWidths);

			// Workaround for .NET ToolStrip height bug;
			// https://sourceforge.net/p/keepass/discussion/329220/thread/19e7c256/
			Debug.Assert((m_toolMain.Height == 25) || DpiUtil.ScalingRequired ||
				MonoWorkarounds.IsRequired(100001));
			m_toolMain.LockHeight(true);

			UpdateTrayIcon(false);
			UpdateFindProfilesMenu(m_menuFindProfiles, true);
			UpdateFindProfilesMenu(m_ctxGroupFindProfiles, true);
			UpdateTagsMenu(m_dynFindTagsMenu, false, false, TagsMenuMode.EnsurePopupOnly);
			UpdateTagsMenu(m_dynTagRemoveMenu, false, false, TagsMenuMode.EnsurePopupOnly);
			UpdateTagsMenu(m_dynTagRemoveCtx, false, false, TagsMenuMode.EnsurePopupOnly);
			UpdateEntryMoveMenu(m_dynMoveToGroupMenu, true);
			UpdateEntryMoveMenu(m_dynMoveToGroupCtx, true);
			UpdateUIState(false);
			ApplyUICustomizations();
			MonoWorkarounds.ApplyTo(this);

			ThreadPool.QueueUserWorkItem(new WaitCallback(OnFormLoadParallelAsync));

			HotKeyManager.CheckCtrlAltA(this);

			Program.TriggerSystem.CheckTriggers();
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

			m_bFormLoaded = true;
			NotifyUserActivity(); // Initialize locking timeout

			if(this.FormLoadPost != null)
				this.FormLoadPost(this, EventArgs.Empty);
			Program.TriggerSystem.RaiseEvent(EcasEventIDs.AppLoadPost);
		}

		private void OnFormShown(object sender, EventArgs e)
		{
			m_bFormShown = true;

			if(MonoWorkarounds.IsRequired(620618))
			{
				PwGroup pg = GetCurrentEntries();
				UpdateColumnsEx(false);
				UpdateUI(false, null, false, null, true, pg, false);
			}

			MinimizeToTrayAtStartIfEnabled(false);

			// Workaround for .NET bug: the active control is correct,
			// but it's not focused;
			// https://sourceforge.net/p/keepass/discussion/329220/thread/71948bbd52/
			if(!NativeLib.IsUnix())
			{
				Control c = UIUtil.GetActiveControl(this);
				if((c != null) && !c.Focused && this.Visible && this.Enabled &&
					(this.WindowState != FormWindowState.Minimized) &&
					(NativeMethods.GetForegroundWindowHandle() == this.Handle))
				{
					Debug.Assert(IsPrimaryControlActive());
					Debug.Assert((m_splitHorizontal.Orientation == Orientation.Vertical) &&
						(m_splitVertical.Orientation == Orientation.Vertical)); // See discussion
					UIUtil.SetFocus(c, this, true);
					Debug.Assert(c.Focused);
				}
			}
		}

		private void OnFileNew(object sender, EventArgs e)
		{
			if(!AppPolicy.Try(AppPolicyId.NewFile)) return;
			if(!AppPolicy.Try(AppPolicyId.SaveFile)) return;

			bool bInfoDialogs = ((Program.Config.UI.UIFlags &
				(ulong)AceUIFlags.HideNewDbInfoDialogs) == 0);
			if(bInfoDialogs)
			{
				if(!FileDialogsEx.ShowNewDatabaseIntro(this)) return;
			}

			string strExt = AppDefs.FileExtension.FileExt;
			SaveFileDialogEx sfd = UIUtil.CreateSaveFileDialog(
				KPRes.CreateNewDatabase2, KPRes.Database + "." + strExt,
				UIUtil.CreateFileTypeFilter(strExt, KPRes.KdbxFiles, true),
				1, strExt, AppDefs.FileDialogContext.Database);

			GlobalWindowManager.AddDialog(sfd.FileDialog);
			DialogResult dr = sfd.ShowDialog();
			GlobalWindowManager.RemoveDialog(sfd.FileDialog);
			if(dr != DialogResult.OK) return;

			string strPath = sfd.FileName;
			IOConnectionInfo ioc = IOConnectionInfo.FromPath(strPath);

			KeyCreationFormResult kcfr;
			dr = KeyCreationForm.ShowDialog(ioc, true, out kcfr);
			if((dr != DialogResult.OK) || (kcfr == null)) return;

			PwDocument dsPrevActive = m_docMgr.ActiveDocument;
			PwDatabase pd = m_docMgr.CreateNewDocument(true).Database;
			pd.New(ioc, kcfr.CompositeKey);

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

			AutoEnableVisualHiding(true);

			PwGroup pg = new PwGroup(true, true, KPRes.General, PwIcon.Folder);
			// for(int i = 0; i < 30; ++i) pg.CustomData.Set("Test" + i.ToString("D2"), "12345");
			pd.RootGroup.AddGroup(pg, true);

			pg = new PwGroup(true, true, KPRes.WindowsOS, PwIcon.DriveWindows);
			pd.RootGroup.AddGroup(pg, true);

			pg = new PwGroup(true, true, KPRes.Network, PwIcon.NetworkServer);
			pd.RootGroup.AddGroup(pg, true);

			pg = new PwGroup(true, true, KPRes.Internet, PwIcon.World);
			// pg.CustomData.Set("GroupTestItem", "TestValue");
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
				PwDefs.HomepageUrl));
			pe.Strings.Set(PwDefs.PasswordField, new ProtectedString(pd.MemoryProtection.ProtectPassword,
				KPRes.Password));
			pe.Strings.Set(PwDefs.NotesField, new ProtectedString(pd.MemoryProtection.ProtectNotes,
				KPRes.Notes));
			pe.AutoType.Add(new AutoTypeAssociation(KPRes.TargetWindow,
				@"{USERNAME}{TAB}{PASSWORD}{TAB}{ENTER}"));
			// for(int i = 0; i < 30; ++i) pe.CustomData.Set("Test" + i.ToString("D2"), "12345");
			pd.RootGroup.AddEntry(pe, true);

			pe = new PwEntry(true, true);
			pe.Strings.Set(PwDefs.TitleField, new ProtectedString(pd.MemoryProtection.ProtectTitle,
				KPRes.SampleEntry + " #2"));
			pe.Strings.Set(PwDefs.UserNameField, new ProtectedString(pd.MemoryProtection.ProtectUserName,
				"Michael321"));
			pe.Strings.Set(PwDefs.UrlField, new ProtectedString(pd.MemoryProtection.ProtectUrl,
				PwDefs.HelpUrl + "kb/testform.html"));
			pe.Strings.Set(PwDefs.PasswordField, new ProtectedString(pd.MemoryProtection.ProtectPassword,
				"12345"));
			pe.AutoType.Add(new AutoTypeAssociation("*Test Form - KeePass*", string.Empty));
			pd.RootGroup.AddEntry(pe, true);

#if DEBUG
			Random r = Program.GlobalRandom;
			long lTimeMin = (new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)).ToBinary();
			long lTimeMax = (new DateTime(2030, 11, 25, 11, 58, 58, DateTimeKind.Utc)).ToBinary();
			Debug.Assert(lTimeMin < lTimeMax);
			PwProfile prf = new PwProfile();
			prf.CharSet = new PwCharSet(PwCharSet.UpperCase + PwCharSet.LowerCase +
				PwCharSet.Digits + PwCharSet.PrintableAsciiSpecial);
			prf.GeneratorType = PasswordGeneratorType.CharSet;
			for(uint iSamples = 0; iSamples < 1500; ++iSamples)
			{
				pg = pd.RootGroup.Groups.GetAt(iSamples % 5);

				pe = new PwEntry(true, true);

				ProtectedString ps;
				PwGenerator.Generate(out ps, prf, null, null);
				pe.Strings.Set(PwDefs.TitleField, ps.WithProtection(
					pd.MemoryProtection.ProtectTitle));
				PwGenerator.Generate(out ps, prf, null, null);
				pe.Strings.Set(PwDefs.UserNameField, ps.WithProtection(
					pd.MemoryProtection.ProtectUserName));
				PwGenerator.Generate(out ps, prf, null, null);
				pe.Strings.Set(PwDefs.UrlField, ps.WithProtection(
					pd.MemoryProtection.ProtectUrl));
				PwGenerator.Generate(out ps, prf, null, null);
				pe.Strings.Set(PwDefs.PasswordField, ps.WithProtection(
					pd.MemoryProtection.ProtectPassword));
				PwGenerator.Generate(out ps, prf, null, null);
				pe.Strings.Set(PwDefs.NotesField, ps.WithProtection(
					pd.MemoryProtection.ProtectNotes));

				pe.CreationTime = DateTime.FromBinary(lTimeMin + (long)(r.NextDouble() *
					(lTimeMax - lTimeMin)));
				pe.LastModificationTime = DateTime.FromBinary(lTimeMin + (long)(r.NextDouble() *
					(lTimeMax - lTimeMin)));
				pe.LastAccessTime = DateTime.FromBinary(lTimeMin + (long)(r.NextDouble() *
					(lTimeMax - lTimeMin)));
				pe.ExpiryTime = DateTime.FromBinary(lTimeMin + (long)(r.NextDouble() *
					(lTimeMax - lTimeMin)));
				pe.LocationChanged = DateTime.FromBinary(lTimeMin + (long)(r.NextDouble() *
					(lTimeMax - lTimeMin)));

				pe.IconId = (PwIcon)r.Next(0, (int)PwIcon.Count);

				pg.AddEntry(pe, true);
			}

			pd.CustomData.Set("Sample Custom Data 1", "0123456789", null);
			pd.CustomData.Set("Sample Custom Data 2", "\u00B5y data", null);

			// pd.PublicCustomData.SetString("Sample Custom Data", "Sample Value");
#endif

			EmergencySheet.AskCreate(pd);
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
			CloseDocument(null, false, false, false, true);
		}

		// Public for plugins
		public void SaveDatabase(PwDatabase pdToSave, object sender)
		{
			PwDatabase pd = (pdToSave ?? m_docMgr.ActiveDatabase);

			if(!pd.IsOpen) return;
			if(!AppPolicy.Try(AppPolicyId.SaveFile)) return;

			if((pd.IOConnectionInfo == null) || (pd.IOConnectionInfo.Path.Length == 0))
			{
				SaveDatabaseAs(pd, null, false, sender, false);
				return;
			}

			UIBlockInteraction(true);

			Guid eventGuid = Guid.NewGuid();
			if(this.FileSavingPre != null)
			{
				FileSavingEventArgs args = new FileSavingEventArgs(false, false, pd, eventGuid);
				this.FileSavingPre(sender, args);
				if(args.Cancel) { UIBlockInteraction(false); return; }
			}

			if(!PreSaveValidate(pd)) { UIBlockInteraction(false); return; }

			if(this.FileSaving != null)
			{
				FileSavingEventArgs args = new FileSavingEventArgs(false, false, pd, eventGuid);
				this.FileSaving(sender, args);
				if(args.Cancel) { UIBlockInteraction(false); return; }
			}
			Program.TriggerSystem.RaiseEvent(EcasEventIDs.SavingDatabaseFile,
				EcasProperty.Database, pd);

			ShutdownBlocker sdb = new ShutdownBlocker(this.Handle, KPRes.SavingDatabase);
			ShowWarningsLogger swLogger = CreateShowWarningsLogger();
			swLogger.StartLogging(KPRes.SavingDatabase, true);
			m_sCancellable.Push(swLogger);

			bool bSuccess = true;
			try
			{
				PreSavingEx(pd, pd.IOConnectionInfo);
				pd.Save(swLogger);
				PostSavingEx(true, pd, pd.IOConnectionInfo, swLogger);
			}
			catch(Exception exSave)
			{
				MessageService.ShowSaveWarning(pd.IOConnectionInfo, exSave, true);
				bSuccess = false;
			}

			m_sCancellable.Pop();
			swLogger.EndLogging();
			sdb.Dispose();

			// Immediately after the UIBlockInteraction call the form might
			// be closed and UpdateUIState might crash, if the order of the
			// two methods is swapped; so first update state, then unblock
			UpdateUIState(false);
			UIBlockInteraction(false);

			if(this.FileSaved != null)
			{
				FileSavedEventArgs args = new FileSavedEventArgs(bSuccess, pd, eventGuid);
				this.FileSaved(sender, args);
			}
			if(bSuccess)
				Program.TriggerSystem.RaiseEvent(EcasEventIDs.SavedDatabaseFile,
					EcasProperty.Database, pd);
		}

		private void OnFileSave(object sender, EventArgs e)
		{
			SaveDatabase(null, sender);
		}

		private void OnFileSaveAs(object sender, EventArgs e)
		{
			SaveDatabaseAs(null, null, false, sender, false);
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
				//	AutoEnableVisualHiding(false);
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
			ChangeMasterKey(null);
		}

		private void OnFilePrint(object sender, EventArgs e)
		{
			if(!m_docMgr.ActiveDatabase.IsOpen) return;
			PrintGroup(m_docMgr.ActiveDatabase.RootGroup);
		}

		private void OnFileLock(object sender, EventArgs e)
		{
			if(!IsCommandTypeInvokable(null, AppCommandType.Lock)) { Debug.Assert(false); return; }

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
			Close();
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
			if(pe == null) { Debug.Assert(false); return; }

			if(ClipboardUtil.CopyAndMinimize(pe.Strings.GetSafe(PwDefs.UserNameField),
				true, this, pe, m_docMgr.SafeFindContainerOf(pe)))
				StartClipboardCountdown();
		}

		private void OnEntryCopyPassword(object sender, EventArgs e)
		{
			PwEntry pe = GetSelectedEntry(false);
			if(pe == null) { Debug.Assert(false); return; }

			if(EntryUtil.ExpireTanEntryIfOption(pe, m_docMgr.ActiveDatabase))
			{
				RefreshEntriesList();
				UpdateUIState(false); // Modified flag set by expiry method
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
			PerformAutoType(null);
		}

		private void OnEntryAdd(object sender, EventArgs e)
		{
			AddEntryEx(sender, null, null, null);
		}

		private void OnEntryEdit(object sender, EventArgs e)
		{
			EditSelectedEntry(PwEntryFormTab.None);
		}

		private void OnEntryDuplicate(object sender, EventArgs e)
		{
			PwDatabase pd = m_docMgr.ActiveDatabase;
			PwGroup pgSelected = GetSelectedGroup();

			PwEntry[] vSelected = GetSelectedEntries();
			if((vSelected == null) || (vSelected.Length == 0)) return;

			DuplicationForm dlg = new DuplicationForm();
			if(UIUtil.ShowDialogAndDestroy(dlg) != DialogResult.OK) return;

			PwObjectList<PwEntry> lNewEntries = new PwObjectList<PwEntry>();
			foreach(PwEntry pe in vSelected)
			{
				PwEntry peNew = pe.Duplicate();

				dlg.ApplyTo(peNew, pe, pd);

				Debug.Assert(pe.ParentGroup == peNew.ParentGroup);
				PwGroup pg = (pe.ParentGroup ?? pgSelected);
				if((pg == null) && (pd != null)) pg = pd.RootGroup;
				if(pg == null) continue;

				pg.AddEntry(peNew, true, true);
				lNewEntries.Add(peNew);
			}

			AddEntriesToList(lNewEntries);
			SelectEntries(lNewEntries, true, true, true, false);
			UpdateUIState(true, m_lvEntries);
		}

		private void OnEntryDelete(object sender, EventArgs e)
		{
			DeleteSelectedEntries();
		}

		private void OnEntrySelectAll(object sender, EventArgs e)
		{
			++m_uBlockEntrySelectionEvent;
			foreach(ListViewItem lvi in m_lvEntries.Items)
			{
				lvi.Selected = true;
			}
			--m_uBlockEntrySelectionEvent;

			ResetDefaultFocus(m_lvEntries);
			UpdateUIState(false);
		}

		private void OnFormClosing(object sender, FormClosingEventArgs e)
		{
			if(UIIsInteractionBlocked()) { e.Cancel = true; return; }

			if(!m_bForceExitOnce) // If not executed by 'File' -> 'Exit'
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

			GlobalWindowManager.CloseAllWindows();

			m_docMgr.RememberActiveDocument();
			if(!CloseAllDocuments(true))
			{
				e.Cancel = true;
				UpdateUI(true, null, true, null, true, null, false);
				return;
			}

			// When shutting down, it can happen that only OnFormClosing
			// is called without the form actually being closed afterwards,
			// thus we must update the UI in this case now
			if((e.CloseReason == CloseReason.TaskManagerClosing) ||
				(e.CloseReason == CloseReason.WindowsShutDown))
				UpdateUI(true, null, true, null, true, null, false);
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			CleanUpEx(); // Saves configuration and cleans up all resources

			if(m_bRestart) WinUtil.Restart();
		}

		private void OnGroupsNodeClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if(e.Button == MouseButtons.Right)
			{
				m_tvGroups.SelectedNode = e.Node;
				return;
			}

			if(e.Button != MouseButtons.Left) return;

			TreeNode tn = e.Node;
			if(tn != null)
			{
				PwGroup pg = (tn.Tag as PwGroup);
				if(pg == null) { Debug.Assert(false); return; }
				Debug.Assert((pg.ParentGroup != null) || (pg == m_docMgr.ActiveDatabase.RootGroup));

				m_tvGroups.SelectedNode = tn; // KPB 1757850

				pg.Touch(false);
				UpdateUI(false, null, false, pg, true, pg, false);
			}
		}

		private void OnMenuChangeLanguage(object sender, EventArgs e)
		{
			LanguageForm lf = new LanguageForm();
			if(!lf.InitEx()) return;

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

		private void OnFindInDatabase(object sender, EventArgs e)
		{
			PerformSearchDialog(null, false);
		}

		private void OnViewShowToolBar(object sender, EventArgs e)
		{
			Debug.Assert(m_bFormLoaded); // The following toggles!
			bool b = !Program.Config.MainWindow.ToolBar.Show;

			Program.Config.MainWindow.ToolBar.Show = b;
			m_toolMain.Visible = b;
			UIUtil.SetChecked(m_menuViewShowToolBar, b);
		}

		private void OnViewShowEntryView(object sender, EventArgs e)
		{
			Debug.Assert(m_bFormLoaded); // The following toggles!
			ShowEntryView(!Program.Config.MainWindow.EntryView.Show);
		}

		private void OnPwListSelectedIndexChanged(object sender, EventArgs e)
		{
			if(m_uBlockEntrySelectionEvent > 0) return;

			// Always defer deselection updates (to ignore item deselect
			// and reselect events when clicking on a selected item)
			if(m_lvEntries.SelectedIndices.Count == 0)
			{
				m_bUpdateUIStateOnce = true;
				return;
			}

			int nCurTicks = Environment.TickCount;
			int nTimeDiff = unchecked(nCurTicks - m_nLastSelChUpdateUIStateTicks);
			if(nTimeDiff < 100) // Burst, defer update
			{
				m_bUpdateUIStateOnce = true;

				m_nLastSelChUpdateUIStateTicks = nCurTicks;
			}
			else
			{
				UpdateUIState(false);

				// Update time (not to nCurTicks, as UpdateUIState might take
				// longer than 100 ms, which would prevent any deferral)
				m_nLastSelChUpdateUIStateTicks = Environment.TickCount;
			}
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
			if(m_uBlockQuickFind != 0) return;
			++m_uBlockQuickFind;

			string strSearch = m_tbQuickFind.Text; // Text, not selected index!

			lock(m_objQuickFindSync)
			{
				int iNow = Environment.TickCount;
				if(((iNow - m_iLastQuickFindTicks) <= 1000) &&
					(strSearch == m_strLastQuickSearch))
				{
					--m_uBlockQuickFind;
					return;
				}

				m_iLastQuickFindTicks = iNow;
				m_strLastQuickSearch = strSearch;
			}

			// Lookup in combobox for the current search
			int nExistsAlready = -1;
			for(int i = 0; i < m_tbQuickFind.Items.Count; ++i)
			{
				string strItemText = (string)m_tbQuickFind.Items[i];
				if(strItemText.Equals(strSearch, StrUtil.CaseIgnoreCmp))
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

			// Asynchronous invocation allows to cleanly process
			// an Enter keypress before blocking the UI
			BeginInvoke(new PerformSearchQuickDelegate(this.PerformSearchQuick),
				strSearch, false, true);

			--m_uBlockQuickFind;
		}

		private void OnQuickFindKeyDown(object sender, KeyEventArgs e)
		{
			if(e.KeyCode == Keys.Return) // Return == Enter
			{
				UIUtil.SetHandled(e, true);
				OnQuickFindSelectedIndexChanged(sender, e);
			}
		}

		private void OnQuickFindKeyUp(object sender, KeyEventArgs e)
		{
			if(e.KeyCode == Keys.Return) // Return == Enter
				UIUtil.SetHandled(e, true);
		}

		private void OnToolsOptions(object sender, EventArgs e)
		{
			OptionsForm ofDlg = new OptionsForm();
			ofDlg.InitEx(m_ilCurrentIcons, IsTrayed());

			Program.Config.Application.MostRecentlyUsed.MaxItemCount = m_mruList.MaxItemCount;

			if(ofDlg.ShowDialog() == DialogResult.OK)
			{
				m_nLockTimerMax = (int)Program.Config.Security.WorkspaceLocking.LockAfterTime;
				m_nClipClearMax = Program.Config.Security.ClipboardClearAfterSeconds;

				m_tvGroups.ApplyOptions();
				// m_lvEntries.GridLines = Program.Config.MainWindow.ShowGridLines;

				UpdateAlternatingBgColor();
				UIUtil.SetAlternatingBgColors(m_lvEntries, m_clrAlternateItemBgColor,
					Program.Config.MainWindow.EntryListAlternatingBgColors);

				m_mruList.MaxItemCount = Program.Config.Application.MostRecentlyUsed.MaxItemCount;
				SetListFont(Program.Config.UI.StandardFont);

				if(ofDlg.RequiresUIReinitialize)
				{
					UIUtil.Initialize(true);

					if(MonoWorkarounds.IsRequired())
					{
						m_menuMain.Invalidate();
						m_toolMain.Invalidate();
						m_statusMain.Invalidate();
					}
				}

				GlobalWindowManager.CustomizeFormHandleCreated(this, null, true);

				AppConfigSerializer.Save(Program.Config);
				UpdateTrayIcon(true);
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

			PwDatabase pd = m_docMgr.SafeFindContainerOf(pe);
			string strToTransfer = SprEngine.Compile(strText, new SprContext(
				pe, pd, SprCompileFlags.All));

			m_pgActiveAtDragStart = GetSelectedGroup();

			m_bDraggingEntries = true;
			this.DoDragDrop(strToTransfer, DragDropEffects.Copy | DragDropEffects.Move);
			m_bDraggingEntries = false;

			pe.Touch(false);
			UpdateUIState(false); // SprEngine.Compile might have modified the database
		}

		private void OnGroupsItemDrag(object sender, ItemDragEventArgs e)
		{
			TreeNode tn = (e.Item as TreeNode);
			if(tn == null) { Debug.Assert(false); return; }

			PwGroup pg = (tn.Tag as PwGroup);
			if(pg == null) { Debug.Assert(false); return; }

			if(pg == m_docMgr.ActiveDatabase.RootGroup) return;
			if(pg.ParentGroup == null) return;

			m_pgActiveAtDragStart = pg;

			m_bDraggingGroup = true;
			this.DoDragDrop(pg, DragDropEffects.Copy | DragDropEffects.Move);
			m_bDraggingGroup = false;

			pg.Touch(false);
		}

		private void OnGroupsDragDrop(object sender, DragEventArgs e)
		{
			TreeViewHitTestInfo tvhi = m_tvGroups.HitTest(m_tvGroups.PointToClient(
				new Point(e.X, e.Y)));
			if(tvhi.Node == null) return;

			PwGroup pgSelected = (tvhi.Node.Tag as PwGroup);
			if(pgSelected == null) { Debug.Assert(false); return; }

			if(m_bDraggingEntries)
				MoveOrCopySelectedEntries(pgSelected, e.Effect);
			else if(e.Data.GetDataPresent(typeof(PwGroup)))
			{
				PwGroup pgDragged = (e.Data.GetData(typeof(PwGroup)) as PwGroup);
				Debug.Assert(pgDragged != null);

				if((pgDragged == null) || (pgDragged == pgSelected) ||
					pgSelected.IsContainedIn(pgDragged) ||
					!pgSelected.CanAddGroup(pgDragged))
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
						if(!pgParent.Groups.Remove(pgDragged)) { Debug.Assert(false); return; }

						pgDragged.PreviousParentGroup = pgParent.Uuid;
						// pgDragged.Touch(true, false);
					}
				}
				else if(e.Effect == DragDropEffects.Copy)
					pgDragged = pgDragged.Duplicate();
				else { Debug.Assert(false); }

				pgSelected.AddGroup(pgDragged, true, true);
				pgSelected.IsExpanded = true;

				UpdateUI(false, null, true, pgDragged, true, null, true);
			}
		}

		private void OnGroupsDragEnter(object sender, DragEventArgs e)
		{
			OnGroupsDragOver(sender, e);
		}

		private void OnGroupsDragOver(object sender, DragEventArgs e)
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

		private void OnGroupsDragLeave(object sender, EventArgs e)
		{
			SetSelectedGroup(m_pgActiveAtDragStart, true);
		}

		private void OnGroupAdd(object sender, EventArgs e)
		{
			TreeNode tn = m_tvGroups.SelectedNode;
			PwDatabase pd = m_docMgr.ActiveDatabase;
			PwGroup pgParent;
			if(tn != null) pgParent = (tn.Tag as PwGroup);
			else pgParent = pd.RootGroup;
			if(pgParent == null) { Debug.Assert(false); return; }

			PwGroup pgNew = new PwGroup(true, true, KPRes.NewGroup, PwIcon.Folder);
			try { pgParent.AddGroup(pgNew, true); } // Add immediately for correct inheritance
			catch(Exception ex) { MessageService.ShowWarning(ex); return; }

			GroupForm gf = new GroupForm();
			gf.InitEx(pgNew, true, m_ilCurrentIcons, pd);

			if(UIUtil.ShowDialogAndDestroy(gf) == DialogResult.OK)
			{
				pgParent.IsExpanded = true;
				UpdateUI(false, null, true, pgNew, true, null, true);
			}
			else pgParent.Groups.Remove(pgNew);
		}

		private void OnGroupDelete(object sender, EventArgs e)
		{
			DeleteSelectedGroup();
		}

		private static void HandleGroupExpandCollapse(TreeViewEventArgs e, bool bExpand)
		{
			if(e == null) { Debug.Assert(false); return; }
			TreeNode tn = e.Node;
			if(tn == null) { Debug.Assert(false); return; }
			PwGroup pg = (tn.Tag as PwGroup);
			if(pg == null) { Debug.Assert(false); return; }

			pg.IsExpanded = bExpand;
		}

		private void OnGroupsAfterCollapse(object sender, TreeViewEventArgs e)
		{
			HandleGroupExpandCollapse(e, false);
		}

		private void OnGroupsAfterExpand(object sender, TreeViewEventArgs e)
		{
			HandleGroupExpandCollapse(e, true);
		}

		private void OnFileSynchronize(object sender, EventArgs e)
		{
			bool? b = ImportUtil.Synchronize(m_docMgr.ActiveDatabase, this, false, this);
			UpdateUI(false, null, true, null, true, null, false);
			if(b.HasValue) SetStatusEx(b.Value ? KPRes.SyncSuccess : KPRes.SyncFailed);
		}

		private void OnViewAlwaysOnTop(object sender, EventArgs e)
		{
			Debug.Assert(m_bFormLoaded); // The following toggles!
			bool bTop = !Program.Config.MainWindow.AlwaysOnTop;

			Program.Config.MainWindow.AlwaysOnTop = bTop;
			UIUtil.SetChecked(m_menuViewAlwaysOnTop, bTop);
			EnsureAlwaysOnTopOpt();
		}

		private void OnGroupPrint(object sender, EventArgs e)
		{
			PrintGroup(GetSelectedGroup());
		}

		private void OnPwListColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
		{
			UpdateColumnsEx(true);
		}

		private void OnFormResize(object sender, EventArgs e)
		{
			// OnFormResize may be called before OnFormLoad (e.g. on high
			// DPI or when running "START /MIN KeePass.exe"); we must ignore
			// this (otherwise e.g. the maximized state gets corrupted)
			if(!m_bFormLoadCalled) return;

			FormWindowState ws = this.WindowState;
			FormWindowState wsLast = m_fwsLast;
			m_fwsLast = ws; // Save now, as auto. below may raise event again

			if((ws == FormWindowState.Normal) || (ws == FormWindowState.Maximized))
				m_oszClientLast = this.ClientSize;

			if(ws == FormWindowState.Maximized)
				Program.Config.MainWindow.Maximized = true;
			else if(ws == FormWindowState.Normal)
				Program.Config.MainWindow.Maximized = false;

			DwmUtil.EnableWindowPeekPreview(this);

			if(UIIsWindowStateAutoBlocked() || UIIsInteractionBlocked())
				return;

			if((ws == FormWindowState.Minimized) && (wsLast != ws))
			{
				if(Program.Config.Security.WorkspaceLocking.LockOnWindowMinimize &&
					IsAtLeastOneFileOpen())
				{
					LockAllDocuments();
					if(m_bCleanedUp) return; // Exited instead of locking
				}

				if(Program.Config.MainWindow.MinimizeToTray)
					MinimizeToTray(true);
			}
			else if((ws == FormWindowState.Normal) || (ws == FormWindowState.Maximized))
			{
				NativeMethods.SyncTopMost(this);

				if(Program.Config.MainWindow.EntryListAutoResizeColumns &&
					(m_lvEntries.View == View.Details))
					UIUtil.ResizeColumns(m_lvEntries, true);

				if((wsLast == FormWindowState.Minimized) && IsFileLocked(null) &&
					!UIIsAutoUnlockBlocked())
					OnFileLock(sender, e); // Unlock

				if((wsLast == FormWindowState.Minimized) && !IsFileLocked(null) &&
					Program.Config.MainWindow.FocusQuickFindOnRestore)
					ResetDefaultFocus(null);
			}
		}

		private void OnTrayTray(object sender, EventArgs e)
		{
			if(GlobalWindowManager.ActivateTopWindow()) return;
			if(!IsCommandTypeInvokable(null, AppCommandType.Window))
			{
				// If a non-Form window is being displayed, activate it
				// indirectly by activating the main window
				if(GlobalWindowManager.WindowCount > 0)
				{
					try { this.Activate(); }
					catch(Exception) { Debug.Assert(false); }
					return;
				}

				try // Show error notification
				{
					NotifyIcon ni = this.MainNotifyIcon;
					if(ni != null)
					{
						string str = (m_statusPartInfo.Text ?? string.Empty);
						if(str == m_strStatusReady) str = string.Empty;
						if(str.Length != 0) str += MessageService.NewLine;
						str += KPRes.WaitPlease + "...";
						str = StrUtil.CompactString3Dots(str,
							NativeMethods.NOTIFYICONDATA_INFO_SIZE - 1);

						ni.ShowBalloonTip(15000, PwDefs.ShortProductName,
							str, ToolTipIcon.Warning);
					}
				}
				catch(Exception) { Debug.Assert(false); }
				return;
			}

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
			// Prevent UI automations during auto-type
			if(SendInputEx.IsSending) return;

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

			if(m_csLockTimer.TryEnter())
			{
				bool bAuto = (m_bFormShown && !m_bDraggingGroup && !m_bDraggingEntries &&
					!UIIsInteractionBlocked());

				if(!bAuto || !GlobalWindowManager.CanCloseAllWindows)
					NotifyUserActivity(); // Unclosable dialog = activity
				else if(IsAtLeastOneFileOpen())
				{
					long lCurTicks = utcNow.Ticks;
					if((lCurTicks >= m_lLockAtTicks) || (lCurTicks >= m_lLockAtGlobalTicks))
					{
						bool bBlock = MonoWorkarounds.IsRequired(1527);
						if(bBlock) BlockMainTimer(true);

						if(Program.Config.Security.WorkspaceLocking.ExitInsteadOfLockingAfterTime)
							OnFileExit(sender, e);
						else LockAllDocuments(); // Might exit instead of locking

						if(bBlock) BlockMainTimer(false);
					}
				}

				if(bAuto && (GlobalWindowManager.WindowCount == 0))
					Program.TriggerSystem.RaiseEvent(EcasEventIDs.TimePeriodic);

				m_csLockTimer.Exit();
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

		private void OnGroupEdit(object sender, EventArgs e)
		{
			EditSelectedGroup(GroupFormTab.None);
		}

		private void OnEntryCopyURL(object sender, EventArgs e)
		{
			PerformDefaultUrlAction(null, false);
		}

		private void OnEntryMassSetIcon(object sender, EventArgs e)
		{
			PwDatabase pd = m_docMgr.ActiveDatabase;
			if((pd == null) || !pd.IsOpen) { Debug.Assert(false); return; }

			PwEntry[] vEntries = GetSelectedEntries();
			if((vEntries == null) || (vEntries.Length == 0)) return;

			IconPickerForm ipf = new IconPickerForm();
			ipf.InitEx(m_ilCurrentIcons, (uint)PwIcon.Count, pd,
				(uint)vEntries[0].IconId, vEntries[0].CustomIconUuid);

			bool bSetIcons = (ipf.ShowDialog() == DialogResult.OK);
			if(bSetIcons)
			{
				PwIcon pi = (PwIcon)ipf.ChosenIconId;
				PwUuid pu = ipf.ChosenCustomIconUuid;
				bool bCustom = !pu.Equals(PwUuid.Zero);

				foreach(PwEntry pe in vEntries)
				{
					if(bCustom)
					{
						if(pe.CustomIconUuid.Equals(pu)) continue;

						pe.CreateBackup(pd);
						pe.CustomIconUuid = pu;
					}
					else
					{
						if((pe.IconId == pi) && pe.CustomIconUuid.Equals(pu)) continue;

						pe.CreateBackup(pd);
						pe.IconId = pi;
						pe.CustomIconUuid = PwUuid.Zero;
					}

					pe.Touch(true, false);
				}
			}
			UIUtil.DestroyForm(ipf);

			bool bUpdImg = pd.UINeedsIconUpdate;
			bool bModified = (bSetIcons || bUpdImg);
			if(bModified) RefreshEntriesList();
			UpdateUI(false, null, bUpdImg, null, false, null, bModified);
		}

		private void OnEntryMoveToTop(object sender, EventArgs e)
		{
			MoveSelectedEntries(-2);
		}

		private void OnEntryMoveOneUp(object sender, EventArgs e)
		{
			MoveSelectedEntries(-1);
		}

		private void OnEntryMoveOneDown(object sender, EventArgs e)
		{
			MoveSelectedEntries(1);
		}

		private void OnEntryMoveToBottom(object sender, EventArgs e)
		{
			MoveSelectedEntries(2);
		}

		private void OnPwListColumnClick(object sender, ColumnClickEventArgs e)
		{
			SortPasswordList(true, e.Column, null, true);
		}

		private void OnPwListKeyDown(object sender, KeyEventArgs e)
		{
			if(HandleMoveKeyMessage(e, true, true)) return;

			bool bHandled = true;

			if(e.Control)
			{
				switch(e.KeyCode)
				{
					case Keys.A: OnEntrySelectAll(sender, e); break;
					case Keys.C:
					case Keys.Insert:
						if(e.Shift) OnEntryClipCopy(sender, e);
						else OnEntryCopyPassword(sender, e);
						break;
					case Keys.V:
						if(e.Shift) OnEntryClipPaste(sender, e);
						else OnEntryPerformAutoType(sender, e);
						break;
					default: bHandled = false; break;
				}
			}
			else if(e.Alt) bHandled = false;
			else if(e.KeyCode == Keys.Delete)
				OnEntryDelete(sender, e);
			else if(e.KeyCode == Keys.Return) // Return == Enter
				OnEntryEdit(sender, e);
			else if(e.KeyCode == Keys.Insert)
			{
				if(e.Shift) OnEntryClipPaste(sender, e);
				else OnEntryAdd(sender, e);
			}
			else if(e.KeyCode == Keys.F2)
				OnEntryEdit(sender, e);
			else bHandled = false;

			if(bHandled) UIUtil.SetHandled(e, true);
		}

		private void OnPwListKeyUp(object sender, KeyEventArgs e)
		{
			if(HandleMoveKeyMessage(e, false, true)) return;

			bool bHandled = true;

			if(e.Control)
			{
				switch(e.KeyCode)
				{
					case Keys.A: break;
					case Keys.C: break;
					case Keys.Insert: break;
					case Keys.V: break;
					default: bHandled = false; break;
				}
			}
			else if(e.Alt) bHandled = false;
			else if(e.KeyCode == Keys.Delete) { }
			else if(e.KeyCode == Keys.Return) { } // Return == Enter
			else if(e.KeyCode == Keys.Insert) { }
			else if(e.KeyCode == Keys.F2) { }
			else bHandled = false;

			if(bHandled) UIUtil.SetHandled(e, true);
		}

		private void OnFindInGroup(object sender, EventArgs e)
		{
			PerformSearchDialog(null, true);
		}

		private void OnViewTanSimpleListClick(object sender, EventArgs e)
		{
			Debug.Assert(m_bFormLoaded); // The following toggles!
			m_bSimpleTanView = !m_bSimpleTanView;
			UIUtil.SetChecked(m_menuViewTanSimpleList, m_bSimpleTanView);

			UpdateEntryList(null, true);
			UpdateUIState(false);
		}

		private void OnViewTanIndicesClick(object sender, EventArgs e)
		{
			Debug.Assert(m_bFormLoaded); // The following toggles!
			m_bShowTanIndices = !m_bShowTanIndices;
			UIUtil.SetChecked(m_menuViewTanIndices, m_bShowTanIndices);

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
					bool bAcceptAlways = false;
					string strError;
					ProtectedString psNew = PwGeneratorUtil.GenerateAcceptable(
						pgf.SelectedProfile, pbAdditionalEntropy, pe, pwDb,
						true, ref bAcceptAlways, out strError);

					if(string.IsNullOrEmpty(strError))
					{
						pe.Strings.Set(PwDefs.PasswordField, psNew.WithProtection(
							pwDb.MemoryProtection.ProtectPassword));

						UpdateUI(false, null, false, null, true, null, true, m_lvEntries);
						SelectEntry(pe, true, true, true, true);
					}
					else pg.Entries.Remove(pe);
				}
			}
			UIUtil.DestroyForm(pgf);
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
			{
				if(Program.Config.UI.TrayIcon.SingleClickDefault)
					OnTrayTray(sender, e);
			}
		}

		private void OnSystemTrayDoubleClick(object sender, EventArgs e)
		{
			if((m_mbLastTrayMouseButtons & MouseButtons.Left) != MouseButtons.None)
			{
				if(!Program.Config.UI.TrayIcon.SingleClickDefault)
					OnTrayTray(sender, e);
			}
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
				ShowSelectedEntryParentGroup();
				ResetDefaultFocus(m_lvEntries);
			}
			else if(strEntryUrl == strLink)
				PerformDefaultUrlAction(null, null);
			else if(pb != null)
				ExecuteBinaryOpen(pe, strLink);
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
					SelectEntry(peRef, true, true, true, true);
				}
			}
			else WinUtil.OpenUrl(strLink, pe);
		}

		private void OnEntryClipCopy(object sender, EventArgs e)
		{
			CopySelectedObjects(typeof(PwEntry), true);
		}

		private void OnEntryClipCopyPlain(object sender, EventArgs e)
		{
			CopySelectedObjects(typeof(PwEntry), false);
		}

		private void OnEntryClipPaste(object sender, EventArgs e)
		{
			PwDatabase pd = m_docMgr.ActiveDatabase;
			if((pd == null) || !pd.IsOpen) return;

			PwGroup pg = GetSelectedGroup();
			if(pg == null) return;

			PwObjectList<PwEntry> l = null;
			try { EntryUtil.PasteEntriesFromClipboard(pd, pg, out l); }
			catch(Exception ex) { MessageService.ShowWarning(ex); }

			bool b = ((l != null) && (l.UCount != 0));
			UpdateUI(false, null, false, null, b, null, b);
			if(b) SelectEntries(l, true, true, true, true);
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
			UpdateUIEntryCtxState();
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

					PwObjectList<PwEntry> l = new PwObjectList<PwEntry>();
					bool bAcceptAlways = false;
					for(uint i = 0; i < uCount; ++i)
					{
						PwEntry pe = new PwEntry(true, true);
						pg.AddEntry(pe, true);

						string strError;
						ProtectedString psNew = PwGeneratorUtil.GenerateAcceptable(
							pgf.SelectedProfile, pbAdditionalEntropy, pe, pwDb,
							true, ref bAcceptAlways, out strError);

						if(!string.IsNullOrEmpty(strError))
						{
							pg.Entries.Remove(pe);
							break;
						}

						pe.Strings.Set(PwDefs.PasswordField, psNew.WithProtection(
							pwDb.MemoryProtection.ProtectPassword));

						l.Add(pe);
					}

					UpdateUI(false, null, false, null, true, null, true, m_lvEntries);
					SelectEntries(l, true, true, true, true);
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

		private void OnFileOpenUrl(object sender, EventArgs e)
		{
			OpenDatabase(null, null, false);
		}

		private void OnFileSaveAsUrl(object sender, EventArgs e)
		{
			SaveDatabaseAs(null, null, true, sender, false);
		}

		private void OnFileImport(object sender, EventArgs e)
		{
			PwDatabase pwDb = m_docMgr.ActiveDatabase;

			bool bAppendedToRootOnly;
			bool? ob = ImportUtil.Import(pwDb, out bAppendedToRootOnly, this);
			bool bModified = ob.GetValueOrDefault(false);

			if(bAppendedToRootOnly && pwDb.IsOpen)
			{
				UpdateUI(false, null, true, pwDb.RootGroup, true, null, bModified);

				if(m_lvEntries.Items.Count > 0)
					m_lvEntries.EnsureVisible(m_lvEntries.Items.Count - 1);
			}
			else UpdateUI(false, null, true, null, true, null, bModified);
		}

		private void OnGroupMoveToTop(object sender, EventArgs e)
		{
			MoveSelectedGroup(-2);
		}

		private void OnGroupMoveOneUp(object sender, EventArgs e)
		{
			MoveSelectedGroup(-1);
		}

		private void OnGroupMoveOneDown(object sender, EventArgs e)
		{
			MoveSelectedGroup(1);
		}

		private void OnGroupMoveToBottom(object sender, EventArgs e)
		{
			MoveSelectedGroup(2);
		}

		private void OnGroupsKeyDown(object sender, KeyEventArgs e)
		{
			if(HandleMoveKeyMessage(e, true, false)) return;

			bool bHandled = true;

			if(e.Alt) bHandled = false;
			else if(e.KeyCode == Keys.Delete)
				OnGroupDelete(sender, e);
			else if(e.KeyCode == Keys.F2)
				OnGroupEdit(sender, e);
			else bHandled = false;

			if(bHandled) UIUtil.SetHandled(e, true);
			else m_kLastUnhandledGroupsKey = e.KeyCode;
		}

		private void OnGroupsKeyUp(object sender, KeyEventArgs e)
		{
			OnGroupsKeyUpPriv(sender, e);
			m_kLastUnhandledGroupsKey = Keys.None; // Always reset
		}

		private void OnGroupsKeyUpPriv(object sender, KeyEventArgs e)
		{
			if(HandleMoveKeyMessage(e, false, false)) return;

			bool bHandled = true;

			if(e.Alt) bHandled = false;
			else if(e.KeyCode == Keys.Delete) { }
			else if(e.KeyCode == Keys.F2) { }
			else if((e.KeyCode == Keys.Up) || (e.KeyCode == Keys.Down) ||
				(e.KeyCode == Keys.Left) || (e.KeyCode == Keys.Right) ||
				(e.KeyCode == Keys.Home) || (e.KeyCode == Keys.End) ||
				((e.KeyCode >= Keys.A) && (e.KeyCode <= Keys.Z)) ||
				((e.KeyCode >= Keys.D0) && (e.KeyCode <= Keys.D9)) ||
				((e.KeyCode >= Keys.NumPad0) && (e.KeyCode <= Keys.NumPad9)))
			{
				// It is possible to receive a key up event even though the
				// key down event went to a different control (e.g. a menu
				// item); do not do anything in this case
				if(e.KeyCode == m_kLastUnhandledGroupsKey)
					UpdateUI(false, null, false, null, true, null, false);
			}
			else bHandled = false;

			if(bHandled) UIUtil.SetHandled(e, true);
		}

		private void OnTabMainSelectedIndexChanged(object sender, EventArgs e)
		{
			if(m_uTabChangeBlocked != 0) return;

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
			SaveDatabaseAs(null, null, false, sender, true);
		}

		private void OnEntryPrint(object sender, EventArgs e)
		{
			PrintGroup(GetSelectedEntriesAsGroup());
		}

		private void OnViewShowEntriesOfSubGroups(object sender, EventArgs e)
		{
			Debug.Assert(m_bFormLoaded); // The following toggles!
			bool b = !Program.Config.MainWindow.ShowEntriesOfSubGroups;
			Program.Config.MainWindow.ShowEntriesOfSubGroups = b;
			UIUtil.SetChecked(m_menuViewShowEntriesOfSubGroups, b);

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
			OnFileLock(sender, e);
		}

		private void OnFileExport(object sender, EventArgs e)
		{
			PerformExport(null, true);
		}

		private void OnGroupExport(object sender, EventArgs e)
		{
			PerformExport(GetSelectedGroup(), true);
		}

		private void OnEntryExport(object sender, EventArgs e)
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

		private bool m_bInFormActivated = false;
		private void OnFormActivated(object sender, EventArgs e)
		{
			if(m_bInFormActivated) { Debug.Assert(false); return; }

			m_bInFormActivated = true;
			try
			{
				NotifyUserActivity();

				// if(m_vRedirectActivation.Count > 0)
				// {
				//	Form f = m_vRedirectActivation.Peek();
				//	if(f != null) f.Activate();
				//	// SystemSounds.Beep.Play(); // Do not beep!
				// }
				// // else EnsureAlwaysOnTopOpt();

				Form fTop = GlobalWindowManager.TopWindow;
				if((fTop != null) && (fTop != this))
					fTop.Activate();
				else
				{
					if(MonoWorkarounds.IsRequired(1760))
					{
						Control c = UIUtil.GetActiveControl(this);
						if(((c == null) || (c == this)) && (m_cLastActive != null))
							UIUtil.SetFocus(m_cLastActive, this, false);
					}
				}
			}
			catch(Exception) { Debug.Assert(false); }
			finally { m_bInFormActivated = false; }
		}

		private void OnFormDeactivate(object sender, EventArgs e)
		{
			if(MonoWorkarounds.IsRequired(1760))
			{
				Control c = UIUtil.GetActiveControl(this);
				if((c != null) && (c != this)) m_cLastActive = c;
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
						if(pd == null) { Debug.Assert(false); break; }

						CloseDocument(pd, false, false, false, true);
						break;
					}
				}
			}
		}

		private void OnViewConfigColumns(object sender, EventArgs e)
		{
			UpdateColumnsEx(true); // Save display indices

			ColumnsForm dlg = new ColumnsForm();
			if(UIUtil.ShowDialogAndDestroy(dlg) == DialogResult.OK)
			{
				UpdateColumnsEx(false);
				UpdateUI(false, null, false, null, true, null, false);
			}
		}

		private void OnFindTagOpening(object sender, EventArgs e)
		{
			UpdateTagsMenu(m_dynFindTagsMenu, false, false, TagsMenuMode.All);
		}

		private void OnEntryViewsByTagOpening(object sender, EventArgs e)
		{
			UpdateTagsMenu(m_dynFindTagsToolBar, true, true, TagsMenuMode.All);
		}

		private void OnEntrySelectedNewTag(object sender, EventArgs e)
		{
			OnAddEntryTag(sender, new DynamicMenuEventArgs(string.Empty, string.Empty));
		}

		private void OnCtxGroupListOpening(object sender, CancelEventArgs e)
		{
			UpdateUIGroupCtxState();
		}

		private void OnGroupSort(object sender, EventArgs e)
		{
			SortSubGroups(false);
		}

		private void OnGroupSortRec(object sender, EventArgs e)
		{
			SortSubGroups(true);
		}

		private void OnPwListClick(object sender, EventArgs e)
		{
			// The following ensures a faster update when multiple items
			// are automatically deselected when clicking on another item
			UpdateUIState(false);
		}

		private void OnGroupEmptyRB(object sender, EventArgs e)
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
			// if(fOptDialog != null) RedirectActivationPush(fOptDialog);
			UIBlockInteraction(true);

			uint uDeleted = pd.DeleteDuplicateEntries(sl);

			// if(fOptDialog != null) RedirectActivationPop();
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
			OnToolsOptions(sender, e);
		}

		private void OnTrayGenPw(object sender, EventArgs e)
		{
			if(!IsCommandTypeInvokable(null, AppCommandType.Window)) return;
			OnToolsPwGenerator(sender, e);
		}

		private void OnGroupDuplicate(object sender, EventArgs e)
		{
			PwGroup pgBase = GetSelectedGroup();
			if(pgBase == null) { Debug.Assert(false); return; }
			PwGroup pgParent = pgBase.ParentGroup;
			if(pgParent == null) { Debug.Assert(false); return; }

			DuplicationForm dlg = new DuplicationForm();
			if(UIUtil.ShowDialogAndDestroy(dlg) != DialogResult.OK) return;

			PwDatabase pd = m_docMgr.ActiveDatabase;
			PwGroup pg = pgBase.Duplicate();

			pg.ParentGroup = pgParent;
			int iBase = pgParent.Groups.IndexOf(pgBase);
			if(iBase < 0) { Debug.Assert(false); iBase = (int)pgParent.Groups.UCount; }
			else ++iBase;
			pgParent.Groups.Insert((uint)iBase, pg);

			PwObjectList<PwEntry> lBase = pgBase.GetEntries(true);
			PwObjectList<PwEntry> lNew = pg.GetEntries(true);
			if((lBase != null) && (lNew != null) && (lBase.UCount == lNew.UCount))
			{
				for(uint u = 0; u < lBase.UCount; ++u)
					dlg.ApplyTo(lNew.GetAt(u), lBase.GetAt(u), pd);
			}
			else { Debug.Assert(false); }

			UpdateUI(false, null, true, pg, true, null, true);
		}

		private void OnToolsXmlRep(object sender, EventArgs e)
		{
			PwDatabase pd = m_docMgr.ActiveDatabase;
			if((pd == null) || !pd.IsOpen) { Debug.Assert(false); return; }

			XmlReplaceForm dlg = new XmlReplaceForm();
			dlg.InitEx(pd);

			if(UIUtil.ShowDialogAndDestroy(dlg) == DialogResult.OK)
				UpdateUI(false, null, true, null, true, null, false);
		}

		private void OnGroupExpand(object sender, EventArgs e)
		{
			TreeNode tn = m_tvGroups.SelectedNode;
			if(tn == null) { Debug.Assert(false); return; }

			TreeNode tnTop = m_tvGroups.TopNode;
			m_tvGroups.BeginUpdate();

			tn.ExpandAll();

			if(tnTop != null) m_tvGroups.TopNode = tnTop;
			m_tvGroups.EndUpdate();
		}

		private void OnGroupCollapse(object sender, EventArgs e)
		{
			TreeNode tn = m_tvGroups.SelectedNode;
			if(tn == null) { Debug.Assert(false); return; }

			TreeNode tnTop = m_tvGroups.TopNode;
			m_tvGroups.BeginUpdate();

			if(tn.Parent != null) tn.Collapse(false);
			else
			{
				foreach(TreeNode tnSub in tn.Nodes)
					tnSub.Collapse(false);
			}

			if(tnTop != null) m_tvGroups.TopNode = tnTop;
			m_tvGroups.EndUpdate();
		}

		private void OnFindAll(object sender, EventArgs e)
		{
			PerformSearchQuick(string.Empty, true, false);
		}

		private void OnFindExp(object sender, EventArgs e)
		{
			ShowExpiredEntries(false, true, 0, false, true);
		}

		private void OnFindExp1(object sender, EventArgs e)
		{
			ShowExpiredEntries(false, false, 1, false, true);
		}

		private void OnFindExp2(object sender, EventArgs e)
		{
			ShowExpiredEntries(false, false, 2, false, true);
		}

		private void OnFindExp3(object sender, EventArgs e)
		{
			ShowExpiredEntries(false, false, 3, false, true);
		}

		private void OnFindExp7(object sender, EventArgs e)
		{
			ShowExpiredEntries(false, false, 7, false, true);
		}

		private void OnFindExp14(object sender, EventArgs e)
		{
			ShowExpiredEntries(false, false, 14, false, true);
		}

		private void OnFindExp30(object sender, EventArgs e)
		{
			ShowExpiredEntries(false, false, 30, true, true);
		}

		private void OnFindExp60(object sender, EventArgs e)
		{
			ShowExpiredEntries(false, false, 60, true, true);
		}

		private void OnFindExpInF(object sender, EventArgs e)
		{
			ShowExpiredEntries(false, false, int.MaxValue, false, true);
		}

		private void OnFindParentGroup(object sender, EventArgs e)
		{
			ShowSelectedEntryParentGroup();
		}

		private void OnFindDupPasswords(object sender, EventArgs e)
		{
			if(CreateAndShowEntryList(EntryUtil.FindDuplicatePasswords,
				KPRes.SearchingOp + "...", Properties.Resources.B48x48_KGPG_Key2,
				KPRes.DuplicatePasswords, KPRes.DuplicatePasswordsList, null,
				false, false) == 0)
				MessageService.ShowInfo(KPRes.DuplicatePasswordsNone);
		}

		private void OnFindSimPasswordsP(object sender, EventArgs e)
		{
			CreateAndShowEntryList(EntryUtil.FindSimilarPasswordsP,
				KPRes.SearchingOp + "...", Properties.Resources.B48x48_KGPG_Key2,
				KPRes.SimilarPasswords, KPRes.SimilarPasswordsList2,
				KPRes.SimilarPasswordsNoDup, true, false);
		}

		private void OnFindSimPasswordsC(object sender, EventArgs e)
		{
			CreateAndShowEntryList(EntryUtil.FindSimilarPasswordsC,
				KPRes.SearchingOp + "...", Properties.Resources.B48x48_KGPG_Key2,
				KPRes.SimilarPasswords, KPRes.ClusterCenters2,
				KPRes.ClusterCentersDesc, true, true);
		}

		private void OnFindPwQualityReport(object sender, EventArgs e)
		{
			CreateAndShowEntryList(EntryUtil.CreatePwQualityList,
				KPRes.SearchingOp + "...", Properties.Resources.B48x48_KOrganizer,
				KPRes.PasswordQuality, KPRes.PasswordQualityReport2, null, true, false);
		}

		private void OnFindLarge(object sender, EventArgs e)
		{
			CreateAndShowEntryList(EntryUtil.FindLargeEntries,
				KPRes.SearchingOp + "...", Properties.Resources.B48x48_Ark,
				KPRes.LargeEntries, KPRes.LargeEntriesList, null, true, false);
		}

		private void OnFindLastMod(object sender, EventArgs e)
		{
			CreateAndShowEntryList(EntryUtil.FindLastModEntries,
				KPRes.SearchingOp + "...", Properties.Resources.B48x48_KGPG_Sign,
				KPRes.LastModified, KPRes.LastModifiedEntriesList, null, true, false);
		}

		private void OnTrayCancel(object sender, EventArgs e)
		{
			if(m_sCancellable.Count == 0) { Debug.Assert(sender == null); return; }

			ShowWarningsLogger sl = m_sCancellable.Peek();
			if(sl != null) sl.SetCancelled(true);
			else { Debug.Assert(false); }
		}

		private void OnGroupDropDownOpening(object sender, EventArgs e)
		{
			UpdateUIGroupMenuState(null, true);
		}

		private void OnEntryDropDownOpening(object sender, EventArgs e)
		{
			UpdateUIEntryMenuState(null, true);
		}

		private void OnEntryTagAddOpening(object sender, EventArgs e)
		{
			ToolStripMenuItem tsmi = (sender as ToolStripMenuItem);
			Debug.Assert((tsmi == m_menuEntryTagAdd) || (tsmi == m_ctxEntryTagAdd));

			// If tsmi is invalid, both menus will be updated
			if(tsmi != m_ctxEntryTagAdd)
				UpdateTagsMenu(m_dynTagAddMenu, true, false, TagsMenuMode.Add);
			if(tsmi != m_menuEntryTagAdd)
				UpdateTagsMenu(m_dynTagAddCtx, true, false, TagsMenuMode.Add);
		}

		private void OnEntryTagRemoveOpening(object sender, EventArgs e)
		{
			ToolStripMenuItem tsmi = (sender as ToolStripMenuItem);
			Debug.Assert((tsmi == m_menuEntryTagRemove) || (tsmi == m_ctxEntryTagRemove));

			// If tsmi is invalid, both menus will be updated
			if(tsmi != m_ctxEntryTagRemove)
				UpdateTagsMenu(m_dynTagRemoveMenu, false, false, TagsMenuMode.Remove);
			if(tsmi != m_menuEntryTagRemove)
				UpdateTagsMenu(m_dynTagRemoveCtx, false, false, TagsMenuMode.Remove);
		}

		private void OnEntryMoveToGroupOpening(object sender, EventArgs e)
		{
			ToolStripMenuItem tsmi = (sender as ToolStripMenuItem);
			Debug.Assert((tsmi == m_menuEntryMoveToGroup) || (tsmi == m_ctxEntryMoveToGroup));

			// If tsmi is invalid, both menus will be updated
			if(tsmi != m_ctxEntryMoveToGroup)
				UpdateEntryMoveMenu(m_dynMoveToGroupMenu, false);
			if(tsmi != m_menuEntryMoveToGroup)
				UpdateEntryMoveMenu(m_dynMoveToGroupCtx, false);
		}

		private void OnGroupDXOpening(object sender, EventArgs e)
		{
			Debug.Assert(sender == m_menuGroupDX);

			bool bPaste = m_docMgr.ActiveDatabase.IsOpen;
			try // Might fail/throw due to clipboard access timeout
			{
				bPaste &= ClipboardUtil.ContainsData(EntryUtil.ClipFormatGroup);
			}
			catch(Exception) { bPaste = false; }
			UIUtil.SetEnabledFast(bPaste, m_menuGroupClipPaste);
		}

		private void OnEntryDXOpening(object sender, EventArgs e)
		{
			Debug.Assert(sender == m_menuEntryDX);

			bool bPaste = m_docMgr.ActiveDatabase.IsOpen;
			try // Might fail/throw due to clipboard access timeout
			{
				bPaste &= ClipboardUtil.ContainsData(EntryUtil.ClipFormatEntries);
			}
			catch(Exception) { bPaste = false; }
			UIUtil.SetEnabledFast(bPaste, m_menuEntryClipPaste);
		}

		private void OnFilePrintEmSheet(object sender, EventArgs e)
		{
			EmergencySheet.Print(m_docMgr.ActiveDatabase);
		}

		private void OnEntryExpiresNow(object sender, EventArgs e)
		{
			SetSelectedEntryExpiry(true);
		}

		private void OnEntryExpiresNever(object sender, EventArgs e)
		{
			SetSelectedEntryExpiry(false);
		}

		private void OnFileFind(object sender, EventArgs e)
		{
			FileSearchEx.FindDatabaseFiles(this, null);
		}

		private void OnFileFindInFolder(object sender, EventArgs e)
		{
			using(FolderBrowserDialog dlg = UIUtil.CreateFolderBrowserDialog(null))
			{
				dlg.ShowNewFolderButton = false;

				GlobalWindowManager.AddDialog(dlg);
				DialogResult dr = dlg.ShowDialog();
				GlobalWindowManager.RemoveDialog(dlg);

				if(dr == DialogResult.OK)
					FileSearchEx.FindDatabaseFiles(this, dlg.SelectedPath);
			}
		}

		private void OnFindProfilesOpening(object sender, EventArgs e)
		{
			UpdateFindProfilesMenu(m_menuFindProfiles, false);
		}

		private void OnCtxGroupFindProfilesOpening(object sender, EventArgs e)
		{
			UpdateFindProfilesMenu(m_ctxGroupFindProfiles, false);
		}

		private void OnGroupClipCopy(object sender, EventArgs e)
		{
			CopySelectedObjects(typeof(PwGroup), true);
		}

		private void OnGroupClipCopyPlain(object sender, EventArgs e)
		{
			CopySelectedObjects(typeof(PwGroup), false);
		}

		private void OnGroupClipPaste(object sender, EventArgs e)
		{
			PwDatabase pd = m_docMgr.ActiveDatabase;
			if((pd == null) || !pd.IsOpen) { Debug.Assert(false); return; }

			PwGroup pg = GetSelectedGroup();
			if(pg == null) return;

			PwGroup pgNew = null;
			try { pgNew = EntryUtil.PasteGroupFromClipboard(pd, pg); }
			catch(Exception ex) { MessageService.ShowWarning(ex); }

			bool b = (pgNew != null);
			UpdateUI(false, null, b, pgNew, b, null, b);
		}

		private void OnFilePrintKeyFile(object sender, EventArgs e)
		{
			EmergencySheet.Print(m_docMgr.ActiveDatabase, false, true);
		}

		private void OnToolsRecreateKeyFile(object sender, EventArgs e)
		{
			PwDatabase pd = m_docMgr.ActiveDatabase;

			KeyFileCreationForm dlg = new KeyFileCreationForm();
			dlg.InitEx(((pd != null) && pd.IsOpen) ? pd.IOConnectionInfo : null);
			dlg.RecreateOnly = true;

			UIUtil.ShowDialogAndDestroy(dlg);
		}

		private void OnGroupMoveToPreviousParent(object sender, EventArgs e)
		{
			MoveToPreviousParentGroup(false);
		}

		private void OnEntryMoveToPreviousParent(object sender, EventArgs e)
		{
			MoveToPreviousParentGroup(true);
		}
	}
}
