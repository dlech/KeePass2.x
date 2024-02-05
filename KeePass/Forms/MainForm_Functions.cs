/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2024 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
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
using KeePass.Util.Archive;
using KeePass.Util.MultipleValues;
using KeePass.Util.Spr;

using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Cryptography;
using KeePassLib.Cryptography.Cipher;
using KeePassLib.Cryptography.KeyDerivation;
using KeePassLib.Delegates;
using KeePassLib.Interfaces;
using KeePassLib.Keys;
using KeePassLib.Security;
using KeePassLib.Serialization;
using KeePassLib.Utility;

using NativeLib = KeePassLib.Native.NativeLib;

namespace KeePass.Forms
{
	public partial class MainForm : Form
	{
		private readonly DocumentManagerEx m_docMgr = new DocumentManagerEx();

		private ListViewGroup m_lvgLastEntryGroup = null;
		private bool m_bEntryGrouping = false;
		private DateTime m_dtCachedNow = DateTime.UtcNow;
		private bool m_bOnlyTans = false;
		private string m_strLastEntryViewRtf = null;
		// private Font m_fontBoldUI = null;
		private Font m_fontExpired = null;
		private Font m_fontBoldTree = null;
		private Font m_fontItalicTree = null;
		private Point m_ptLastEntriesMouseClick = new Point(0, 0);
		private readonly RichTextBoxContextMenu m_ctxEntryPreviewContextMenu = new RichTextBoxContextMenu();
		private DynamicMenu m_dynStringsMenu;
		private DynamicMenu m_dynStringsCtx;
		private DynamicMenu m_dynBinariesMenu;
		private DynamicMenu m_dynBinariesCtx;
		private DynamicMenu m_dynFindTagsMenu;
		private DynamicMenu m_dynFindTagsToolBar;
		private DynamicMenu m_dynTagAddMenu;
		private DynamicMenu m_dynTagAddCtx;
		private DynamicMenu m_dynTagRemoveMenu;
		private DynamicMenu m_dynTagRemoveCtx;
		private DynamicMenu m_dynMoveToGroupMenu;
		private DynamicMenu m_dynMoveToGroupCtx;
		private DynamicMenu m_dynAutoTypeAdvMenu;
		private DynamicMenu m_dynAutoTypeAdvCtx;
		private OpenWithMenu m_dynOpenUrlMenu;
		private OpenWithMenu m_dynOpenUrlCtx;
		private OpenWithMenu m_dynOpenUrlToolBar;
		private readonly MenuItemLinks m_milMain = new MenuItemLinks();

		private ToolStripMenuItem m_tsmiAutoTypeHotpMenu = null;
		private ToolStripMenuItem m_tsmiAutoTypeHotpCtx = null;
		private ToolStripMenuItem m_tsmiAutoTypeTotpMenu = null;
		private ToolStripMenuItem m_tsmiAutoTypeTotpCtx = null;

		private readonly EntryDataCommand m_edcCopyTotp = new EntryDataCommand(
			EntryDataCommandType.CopyValue, EntryUtil.TotpPlh,
			(new FormatException()).Message);
		private readonly EntryDataCommand m_edcShowTotp = new EntryDataCommand(
			EntryDataCommandType.ShowValue, EntryUtil.TotpPlh,
			(new FormatException()).Message);

		private readonly AsyncPwListUpdate m_asyncListUpdate;

		private readonly MruList m_mruList = new MruList();

		private readonly SessionLockNotifier m_sessionLockNotifier = new SessionLockNotifier();

		private readonly DefaultPluginHost m_pluginDefaultHost = new DefaultPluginHost();
		private readonly PluginManager m_pluginManager = new PluginManager();

		private readonly CriticalSectionEx m_csLockTimer = new CriticalSectionEx();
		private int m_nLockTimerMax = 0;
		// private volatile int m_nLockTimerCur = 0;
		private long m_lLockAtTicks = long.MaxValue;
		private uint m_uLastInputTime = uint.MaxValue;
		private long m_lLockAtGlobalTicks = long.MaxValue;

		private uint m_uBlockQuickFind = 0;
		private readonly object m_objQuickFindSync = new object();
		private int m_iLastQuickFindTicks = Environment.TickCount - 1500;
		private string m_strLastQuickSearch = string.Empty;

		private ToolStripSeparator m_tsSepCustomToolBar = null;
		private readonly List<ToolStripButton> m_lCustomToolBarButtons = new List<ToolStripButton>();

		private int m_nClipClearMax = 0;
		private int m_nClipClearCur = -1;

		private readonly string m_strNeverExpires = KPRes.NeverExpires;
		private readonly string m_strStatusReady = KPRes.Ready;
		private readonly string m_strNoneP = "(" + KPRes.None + ")";

		private bool m_bSimpleTanView = true;
		private bool m_bShowTanIndices = true;

		private Image m_imgFileSaveEnabled = null;
		private Image m_imgFileSaveDisabled = null;
		// private Image m_imgFileSaveAllEnabled = null;
		// private Image m_imgFileSaveAllDisabled = null;
		private List<Image> m_lStdClientImages = null;
		private ImageList m_ilCurrentIcons = null;

		// private KeyValuePair<Color, Icon> m_kvpIcoMain =
		//	new KeyValuePair<Color, Icon>(Color.Empty, null);
		// private KeyValuePair<Color, Icon> m_kvpIcoTrayNormal =
		//	new KeyValuePair<Color, Icon>(Color.Empty, null);
		// private KeyValuePair<Color, Icon> m_kvpIcoTrayLocked =
		//	new KeyValuePair<Color, Icon>(Color.Empty, null);

		private readonly List<Image> m_lTabImages = new List<Image>();
		private ImageList m_ilTabImages = null;

		private bool m_bIsAutoTyping = false;
		private uint m_uTabChangeBlocked = 0;
		private uint m_uForceSave = 0;
		private uint m_uUIBlocked = 0;
		private uint m_uUnlockAutoBlocked = 0;
		private MouseButtons m_mbLastTrayMouseButtons = MouseButtons.None;
		private uint m_uWindowStateAutoBlocked = 0;
		private uint m_uMainTimerBlocked = 0;
		private bool m_bHasBlockedShowWindow = false;

		private bool m_bUpdateUIStateOnce = false;
		private int m_nLastSelChUpdateUIStateTicks = 0;

		private readonly int m_nAppMessage = Program.ApplicationMessage;
		private readonly int m_nTaskbarButtonMessage;
		private bool m_bTaskbarButtonMessage;

		private FormWindowState m_fwsLast = FormWindowState.Normal;
		private Control m_cLastActive = null;
		private PwGroup m_pgActiveAtDragStart = null;
		private readonly Stack<ShowWarningsLogger> m_sCancellable = new Stack<ShowWarningsLogger>();
		private PwEntry m_peMarkedForComparison = null;

		// private Stack<Form> m_vRedirectActivation = new Stack<Form>();

		private Size? m_oszClientLast = null;
		internal Size LastClientSize
		{
			get
			{
				if(m_oszClientLast.HasValue) return m_oszClientLast.Value;
				return this.ClientSize;
			}
		}

		public DocumentManagerEx DocumentManager { get { return m_docMgr; } }
		public PwDatabase ActiveDatabase { get { return m_docMgr.ActiveDatabase; } }
		public ImageList ClientIcons { get { return m_ilCurrentIcons; } }

		/// <summary>
		/// Get a reference to the main menu.
		/// </summary>
		public MenuStrip MainMenu { get { return m_menuMain; } }
		/// <summary>
		/// Get a reference to the 'Tools' popup menu in the main menu. It is
		/// recommended that you use this reference instead of searching the
		/// main menu for the 'Tools' item.
		/// </summary>
		public ToolStripMenuItem ToolsMenu { get { return m_menuTools; } }

		public ContextMenuStrip EntryContextMenu { get { return m_ctxPwList; } }
		public ContextMenuStrip GroupContextMenu { get { return m_ctxGroupList; } }
		public ContextMenuStrip TrayContextMenu { get { return m_ctxTray; } }

		public ToolStripProgressBar MainProgressBar { get { return m_statusPartProgress; } }
		public NotifyIcon MainNotifyIcon { get { return m_ntfTray.NotifyIcon; } }

		public MruList FileMruList { get { return m_mruList; } }

		internal PluginManager PluginManager { get { return m_pluginManager; } }
		internal bool HasFormLoaded { get { return m_bFormLoaded; } }

		internal sealed class MainAppState
		{
			public bool FileLocked;
			public bool DatabaseOpened;
			public int EntriesCount;
			public int EntriesSelected;
			public bool EnableLockCmd;
			public bool NoWindowShown;
			public PwEntry SelectedEntry;
			public bool CanCopyUserName = false;
			public bool CanCopyPassword = false;
			public bool CanOpenUrl = false;
			public bool CanPerformAutoType = false;
			public bool IsOneTan = false;
			public string LockUnlock;
		}

		internal enum AppCommandType
		{
			Window = 0,
			Lock = 1
		}

		private enum EntryDataCommandType
		{
			None = 0,
			CopyField,
			CopyValue,
			ShowValue
		}

		private sealed class EntryDataCommand
		{
			public readonly EntryDataCommandType Type;
			public readonly string Param;
			public readonly string ErrorOnSprFailure;

			public EntryDataCommand(EntryDataCommandType t, string strParam,
				string strErrorOnSprFailure)
			{
				this.Type = t;
				this.Param = strParam;
				this.ErrorOnSprFailure = strErrorOnSprFailure;
			}
		}

		/// <summary>
		/// Check if the main window is trayed (i.e. only the tray icon is visible).
		/// </summary>
		/// <returns>Returns <c>true</c>, if the window is trayed.</returns>
		public bool IsTrayed()
		{
			return !this.Visible;
		}

		public bool IsFileLocked(PwDocument ds)
		{
			if(ds == null) ds = m_docMgr.ActiveDocument;

			return (ds.LockedIoc.Path.Length != 0);
		}

		public bool IsAtLeastOneFileOpen()
		{
			foreach(PwDocument ds in m_docMgr.Documents)
			{
				if(ds.Database.IsOpen) return true;
			}

			return false;
		}

		private void LoadPlugins()
		{
			int cColumnProvs = Program.ColumnProviderPool.Count;

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

			if(Program.ColumnProviderPool.Count != cColumnProvs)
				UpdateColumnsEx(false);

			// Delete old files *after* loading plugins (when timestamps
			// of loaded plugins have been updated already)
			if(Program.Config.Application.Start.PluginCacheDeleteOld)
				PlgxCache.DeleteOldFilesAsync();
		}

		private void CleanUpEx()
		{
			m_asyncListUpdate.CancelPendingUpdatesAsync();

			Program.TriggerSystem.RaiseEvent(EcasEventIDs.AppExit);

			MonoWorkarounds.Release(this);
			GlobalWindowManager.CustomizeFormHandleCreated(this, false, false);

			m_nClipClearCur = -1;
			if(Program.Config.Security.ClipboardClearOnExit)
				ClipboardUtil.ClearIfOwner();

			m_pluginManager.UnloadAllPlugins(); // Before saving the configuration

			// Just unregister the events; no need to remove the buttons
			foreach(ToolStripButton tbCustom in m_lCustomToolBarButtons)
				tbCustom.Click -= OnCustomToolBarButtonClicked;
			m_lCustomToolBarButtons.Clear();

			SaveConfig(); // After unloading plugins

			m_sessionLockNotifier.Uninstall();
			HotKeyManager.UnregisterAll();

			EntryTemplates.Release();

			m_dynStringsMenu.MenuClick -= this.OnEntryStringClick;
			m_dynStringsCtx.MenuClick -= this.OnEntryStringClick;
			m_dynBinariesMenu.MenuClick -= this.OnEntryBinaryClick;
			m_dynBinariesCtx.MenuClick -= this.OnEntryBinaryClick;
			m_dynFindTagsMenu.MenuClick -= this.OnShowEntriesByTag;
			m_dynFindTagsToolBar.MenuClick -= this.OnShowEntriesByTag;
			m_dynTagAddMenu.MenuClick -= this.OnAddEntryTag;
			m_dynTagAddCtx.MenuClick -= this.OnAddEntryTag;
			m_dynTagRemoveMenu.MenuClick -= this.OnRemoveEntryTag;
			m_dynTagRemoveCtx.MenuClick -= this.OnRemoveEntryTag;
			m_dynMoveToGroupMenu.MenuClick -= this.OnEntryMoveToGroup;
			m_dynMoveToGroupCtx.MenuClick -= this.OnEntryMoveToGroup;
			m_dynAutoTypeAdvMenu.MenuClick -= this.OnEntryPerformAutoTypeAdv;
			m_dynAutoTypeAdvCtx.MenuClick -= this.OnEntryPerformAutoTypeAdv;
			m_dynOpenUrlMenu.Destroy();
			m_dynOpenUrlCtx.Destroy();
			m_dynOpenUrlToolBar.Destroy();

			m_ctxEntryPreviewContextMenu.Detach();

			m_lvsmMenu.Release();
			m_lvgmMenu.Release();

			m_mruList.Release();

			m_ntfTray.Visible = false;

			// Debug.Assert(m_vRedirectActivation.Count == 0);
			Debug.Assert(m_uUIBlocked == 0);
			Debug.Assert(m_uUnlockAutoBlocked == 0);
			Debug.Assert(m_sCancellable.Count == 0);
			Debug.Assert(!m_bBlockColumnUpdates);
			Debug.Assert(m_uBlockGroupSelectionEvent == 0);
			Debug.Assert(m_uBlockEntrySelectionEvent == 0);
			this.Visible = false;

			// if(m_fontBoldUI != null) { m_fontBoldUI.Dispose(); m_fontBoldUI = null; }
			// if(m_fontBoldTree != null) { m_fontBoldTree.Dispose(); m_fontBoldTree = null; }
			// if(m_fontItalicTree != null) { m_fontItalicTree.Dispose(); m_fontItalicTree = null; }

			m_asyncListUpdate.WaitAll();
			m_bCleanedUp = true;
		}

		/// <summary>
		/// Save the current configuration. The configuration is saved using the
		/// cascading configuration files mechanism and the default paths are used.
		/// </summary>
		public void SaveConfig()
		{
			SaveWindowPositionAndSize();

			AceMainWindow mw = Program.Config.MainWindow;

			mw.Layout = ((m_splitHorizontal.Orientation != Orientation.Horizontal) ?
				AceMainWindowLayout.SideBySide : AceMainWindowLayout.Default);

			UpdateColumnsEx(true);

			Debug.Assert(m_bSimpleTanView == m_menuViewTanSimpleList.Checked);
			mw.TanView.UseSimpleView = m_bSimpleTanView;
			Debug.Assert(m_bShowTanIndices == m_menuViewTanIndices.Checked);
			mw.TanView.ShowIndices = m_bShowTanIndices;

			Program.Config.MainWindow.ListSorting = m_pListSorter;

			SerializeMruList(true);

			// mw.ShowGridLines = m_lvEntries.GridLines;

			AppConfigSerializer.Save();
		}

		private void SaveWindowPositionAndSize()
		{
			if(!m_bFormLoadCalled) { Debug.Assert(false); return; }

			FormWindowState ws = this.WindowState;

			if(ws == FormWindowState.Normal)
			{
				Program.Config.MainWindow.X = this.Location.X;
				Program.Config.MainWindow.Y = this.Location.Y;
				Program.Config.MainWindow.Width = this.Size.Width;
				Program.Config.MainWindow.Height = this.Size.Height;
			}

			if((ws == FormWindowState.Normal) || (ws == FormWindowState.Maximized))
			{
				Program.Config.MainWindow.SplitterHorizontalFrac =
					m_splitHorizontal.SplitterDistanceFrac;
				Program.Config.MainWindow.SplitterVerticalFrac =
					m_splitVertical.SplitterDistanceFrac;
			}

			// Program.Config.MainWindow.Maximized = (ws == FormWindowState.Maximized);
		}

		/// <summary>
		/// Update the UI state, i.e. enable/disable menu items depending on the state
		/// of the database (open, closed, locked, modified) and the selected items in
		/// the groups and entries list. You must call this function after all
		/// state-changing operations. For example, if you add a new entry the state
		/// needs to be updated (as the database has been modified) and you must call
		/// this function.
		/// </summary>
		/// <param name="bSetModified">If this parameter is <c>true</c>, the
		/// currently open database is marked as modified.</param>
		private void UpdateUIState(bool bSetModified)
		{
			UpdateUIState(bSetModified, null);
		}

		private void UpdateUIState(bool bSetModified, Control cOptFocus)
		{
			// For instance when running "START /MIN KeePass.exe",
			// we can get called before OnFormLoad has been called
			if(!m_bFormLoadCalled) return;

			NotifyUserActivity();
			m_bUpdateUIStateOnce = false; // We do it now

			MainAppState s = GetMainAppState();
			PwDatabase pd = m_docMgr.ActiveDatabase;
			PwEntry pe = s.SelectedEntry;
			ulong uif = Program.Config.UI.UIFlags;

			if(s.DatabaseOpened && bSetModified) pd.Modified = true;

			bool bGroupsEnabled = m_tvGroups.Enabled;
			if(bGroupsEnabled && !s.DatabaseOpened)
			{
				m_tvGroups.BackColor = AppDefs.ColorControlDisabled;
				m_tvGroups.Enabled = false;
			}
			else if(!bGroupsEnabled && s.DatabaseOpened)
			{
				m_tvGroups.Enabled = true;
				m_tvGroups.BackColor = AppDefs.ColorControlNormal;
			}

			if(Program.Config.MainWindow.EntrySelGroupSel && (pe != null))
			{
				PwGroup pgInitial = GetSelectedGroup();
				PwGroup pgToSel = pe.ParentGroup;
				if((pgToSel != null) && (pgToSel != pgInitial))
					SelectGroup(pgToSel, false);
			}

			m_lvEntries.Enabled = s.DatabaseOpened;

			m_statusPartSelected.Text = s.EntriesSelected.ToString() +
				" " + KPRes.OfLower + " " + s.EntriesCount.ToString() +
				" " + KPRes.SelectedLower;
			SetStatusEx(null);

			string strWindowText = PwDefs.ShortProductName;
			string strNtfText = PwDefs.ShortProductName;
			// int qSmall = UIUtil.GetSmallIconSize().Width;

			const int cchNtf = NativeMethods.NOTIFYICONDATA_TIP_SIZE - 1;

			if(s.FileLocked)
			{
				IOConnectionInfo iocLck = m_docMgr.ActiveDocument.LockedIoc;

				string strWindowEnd = @" [" + KPRes.Locked + @"] - " +
					PwDefs.ShortProductName;
				string strFileDesc = iocLck.Path;
				if(!Program.Config.MainWindow.ShowFullPathInTitle)
					strFileDesc = UrlUtil.GetFileName(strFileDesc);
				int iMaxChars = cchNtf - strWindowEnd.Length;
				if(iMaxChars >= 0)
					strFileDesc = WinUtil.CompactPath(strFileDesc, iMaxChars);
				else { Debug.Assert(false); }
				strWindowText = strFileDesc + strWindowEnd;

				string strNtfPre = PwDefs.ShortProductName + " - " +
					KPRes.WorkspaceLocked + MessageService.NewLine;
				strFileDesc = iocLck.Path;
				iMaxChars = cchNtf - strNtfPre.Length;
				if(iMaxChars >= 0)
					strFileDesc = WinUtil.CompactPath(strFileDesc, iMaxChars);
				else { Debug.Assert(false); }
				strNtfText = strNtfPre + strFileDesc;

				// Icon icoDisposable, icoAssignable;
				// CreateColorizedIcon(Properties.Resources.QuadLocked, qSmall,
				//	ref m_kvpIcoTrayLocked, out icoAssignable, out icoDisposable);
				// m_ntfTray.Icon = icoAssignable;
				// if(icoDisposable != null) icoDisposable.Dispose();
				m_ntfTray.Icon = CreateColorizedIcon(AppIconType.QuadLocked, true);

				TaskbarList.SetOverlayIcon(this, Properties.Resources.LockOverlay,
					KPRes.Locked);
			}
			else if(!s.DatabaseOpened)
			{
				// Icon icoDisposable, icoAssignable;
				// CreateColorizedIcon(Properties.Resources.QuadNormal, qSmall,
				//	ref m_kvpIcoTrayNormal, out icoAssignable, out icoDisposable);
				// m_ntfTray.Icon = icoAssignable;
				// if(icoDisposable != null) icoDisposable.Dispose();
				m_ntfTray.Icon = CreateColorizedIcon(AppIconType.QuadNormal, true);

				TaskbarList.SetOverlayIcon(this, null, string.Empty);
			}
			else // Database open and not locked
			{
				string strWindowEnd = (pd.Modified ? "* - " : " - ") +
					PwDefs.ShortProductName;
				string strFileDesc = pd.IOConnectionInfo.Path;
				if(!Program.Config.MainWindow.ShowFullPathInTitle)
					strFileDesc = UrlUtil.GetFileName(strFileDesc);
				strFileDesc = WinUtil.CompactPath(strFileDesc, cchNtf -
					strWindowEnd.Length);
				strWindowText = strFileDesc + strWindowEnd;

				string strNtfPre = PwDefs.ShortProductName + MessageService.NewLine;
				strNtfText = strNtfPre + WinUtil.CompactPath(
					pd.IOConnectionInfo.Path, cchNtf - strNtfPre.Length);

				// Icon icoDisposable, icoAssignable;
				// CreateColorizedIcon(Properties.Resources.QuadNormal, qSmall,
				//	ref m_kvpIcoTrayNormal, out icoAssignable, out icoDisposable);
				// m_ntfTray.Icon = icoAssignable;
				// if(icoDisposable != null) icoDisposable.Dispose();
				m_ntfTray.Icon = CreateColorizedIcon(AppIconType.QuadNormal, true);

				TaskbarList.SetOverlayIcon(this, null, string.Empty);
			}

			DwmUtil.EnableWindowPeekPreview(this);

			bool bFormShownRaised = false;
			if(MonoWorkarounds.IsRequired(801414))
				bFormShownRaised = MonoWorkarounds.ExchangeFormShownRaised(this, false);

			// Clip the strings again (it could be that a translator used
			// a string in KPRes that is too long to be displayed)
			this.Text = StrUtil.CompactString3Dots(strWindowText, cchNtf);

			if(MonoWorkarounds.IsRequired(801414))
				MonoWorkarounds.ExchangeFormShownRaised(this, bFormShownRaised);

			strNtfText = StrUtil.EncodeToolTipText(strNtfText);
			m_ntfTray.Text = StrUtil.CompactString3Dots(strNtfText, cchNtf);

			// Icon icoToDispose, icoToAssign;
			// if(CreateColorizedIcon(Properties.Resources.KeePass, 0,
			//	ref m_kvpIcoMain, out icoToAssign, out icoToDispose))
			//	this.Icon = icoToAssign;
			// if(icoToDispose != null) icoToDispose.Dispose();
			this.Icon = CreateColorizedIcon(AppIconType.Main, false);

			UIUtil.SetEnabledFast(s.DatabaseOpened || s.FileLocked,
				m_menuFileClose, m_tbCloseTab);
			UIUtil.SetEnabledFast(s.DatabaseOpened, m_menuFileSaveAs,
				m_menuFileSaveAsLocal, m_menuFileSaveAsUrl, m_menuFileSaveAsCopy,
				m_menuFileChangeMasterKey, m_menuFilePrint,
				m_menuFilePrintDatabase, m_menuFilePrintEmSheet,
				m_menuFileImport, m_menuFileExport);
			UIUtil.SetEnabledFast(s.DatabaseOpened && pd.MasterKey.ContainsType(
				typeof(KcpKeyFile)), m_menuFilePrintKeyFile);
			UIUtil.SetEnabledFast(s.DatabaseOpened && ((uif &
				(ulong)AceUIFlags.DisableDbSettings) == 0), m_menuFileDbSettings);
			UIUtil.SetEnabledFast(s.DatabaseOpened, m_menuFileSync,
				m_menuFileSyncFile, m_menuFileSyncUrl, m_menuFileSyncRecent);
			UIUtil.SetEnabledFast(s.EnableLockCmd, m_menuFileLock, m_tbLockWorkspace);

			// Enable/disable menu items for correct shortcut keys handling
			UpdateUIGroupMenuState(s, false);
			UpdateUIEntryMenuState(s, false);

			UIUtil.SetEnabledFast(s.DatabaseOpened, m_menuFindInDatabase,
				m_menuFindInGroup, m_menuFindProfiles, m_menuFindTag, m_menuFindAll);
			UIUtil.SetEnabledFast((s.EntriesSelected == 1), m_menuFindParentGroup);
			UIUtil.SetEnabledFast(s.DatabaseOpened, m_menuFindExp, m_menuFindExpIn,
				m_menuFindExp1, m_menuFindExp2, m_menuFindExp3, m_menuFindExp7,
				m_menuFindExp14, m_menuFindExp30, m_menuFindExp60, m_menuFindExpInF);
			UIUtil.SetEnabledFast(s.DatabaseOpened, m_menuFindLastMod,
				m_menuFindHistory, m_menuFindLarge, m_menuFindDupPasswords,
				m_menuFindSimPasswordsP, m_menuFindSimPasswordsC,
				m_menuFindPwQuality);

			UIUtil.SetEnabledFast(s.DatabaseOpened, m_menuToolsGeneratePwList,
				m_menuToolsTanWizard, m_menuToolsDbMaintenance,
				m_menuToolsDbDelDupEntries, m_menuToolsDbDelEmptyGroups,
				m_menuToolsDbDelUnusedIcons);
			UIUtil.SetEnabledFast(s.DatabaseOpened && ((uif &
				(ulong)AceUIFlags.DisableXmlReplace) == 0), m_menuToolsDbXmlRep);

			UIUtil.SetEnabledFast(s.DatabaseOpened, m_tbAddEntry, m_tbFind,
				m_tbEntryViewsDropDown, m_tbViewsShowAll, m_tbViewsShowExpired,
				m_tbQuickFind);

			Image imgSave;
			bool bEnableSave;
			if(Program.Config.MainWindow.DisableSaveIfNotModified)
			{
				imgSave = m_imgFileSaveEnabled;
				bEnableSave = (s.DatabaseOpened && pd.Modified);
			}
			else
			{
				imgSave = ((!s.DatabaseOpened || pd.Modified) ?
					m_imgFileSaveEnabled : m_imgFileSaveDisabled);
				bEnableSave = s.DatabaseOpened;
			}
			m_menuFileSave.Image = imgSave;
			m_tbSaveDatabase.Image = imgSave;
			UIUtil.SetEnabledFast(bEnableSave, m_menuFileSave, m_tbSaveDatabase);

			m_tbCopyUserName.Enabled = s.CanCopyUserName;
			m_tbCopyPassword.Enabled = s.CanCopyPassword;
			UIUtil.SetEnabledFast(s.CanOpenUrl, m_tbOpenUrl,
				m_tbOpenUrlDefault, m_tbCopyUrl);
			m_tbAutoType.Enabled = s.CanPerformAutoType;

			m_menuFileLock.Text = s.LockUnlock;
			string strLockText = StrUtil.RemoveAccelerator(s.LockUnlock);
			m_tbLockWorkspace.Text = m_ctxTrayLock.Text = strLockText;
			m_tbLockWorkspace.ToolTipText = strLockText + " (" +
				m_menuFileLock.ShortcutKeyDisplayString + ")";

			bool bMultiDoc = (m_docMgr.DocumentCount > 1);
			m_tabMain.Visible = bMultiDoc;
			m_tbSaveAll.Visible = bMultiDoc;
			m_tbCloseTab.Visible = (bMultiDoc ||
				!Program.Config.MainWindow.HideCloseDatabaseButton);

			bool bAtLeastOneModified = false;
			foreach(PwDocument pwDoc in m_docMgr.Documents)
			{
				if((pwDoc == null) || (pwDoc.Database == null)) { Debug.Assert(false); continue; }
				bAtLeastOneModified |= pwDoc.Database.Modified;
			}
			// m_tbSaveAll.Image = (bAtLeastOneModified ? m_imgFileSaveAllEnabled :
			//	m_imgFileSaveAllDisabled);
			m_tbSaveAll.Enabled = bAtLeastOneModified;

			ShowEntryDetails(pe);
			UpdateUITabs();

			if(cOptFocus != null) ResetDefaultFocus(cOptFocus);

			if(this.UIStateUpdated != null) this.UIStateUpdated(this, EventArgs.Empty);
			// Program.TriggerSystem.RaiseEvent(EcasEventIDs.UpdatedUIState);
		}

		private void UpdateUIGroupState(MainAppState sMain, bool bMenuVisible,
			bool bContextMenu)
		{
			MainAppState s = (sMain ?? GetMainAppState());

			PwGroup pg = GetSelectedGroup();
			PwGroup pgParent = ((pg != null) ? pg.ParentGroup : null);
			PwDatabase pd = m_docMgr.ActiveDatabase;
			if(!s.DatabaseOpened) pd = null; // Null if closed
			PwGroup pgRoot = ((pd != null) ? pd.RootGroup : null);

			bool bChildOps = (s.DatabaseOpened && (pg != pgRoot));
			bool bMoveOps = (bChildOps && (pgParent != null) &&
				(pgParent.Groups.UCount > 1));
			uint uSubGroups = 0, uSubEntries;
			if(pg != null) pg.GetCounts(true, out uSubGroups, out uSubEntries);

			SuspendLayoutScope sls = new SuspendLayoutScope(true, m_menuMain,
				m_ctxGroupList);

			UIUtil.SetEnabledFast(s.DatabaseOpened, m_menuGroupAdd, m_menuGroupEdit);
			UIUtil.SetEnabledFast(bChildOps, m_menuGroupDuplicate, m_menuGroupDelete);

			UIUtil.SetEnabledFast(s.DatabaseOpened, m_menuGroupRearrange);
			UIUtil.SetEnabledFast((bMoveOps && (pgParent.Groups.IndexOf(pg) >= 1)),
				m_menuGroupMoveToTop, m_menuGroupMoveOneUp);
			UIUtil.SetEnabledFast((bMoveOps && (pgParent.Groups.IndexOf(pg) <
				((int)pgParent.Groups.UCount - 1))), m_menuGroupMoveOneDown,
				m_menuGroupMoveToBottom);

			if(bMenuVisible)
				UpdateMoveToPreviousParentGroupUI(pg, null, m_menuGroupMoveToPreviousParent);

			UIUtil.SetEnabledFast(((pg != null) && (pg.Groups.UCount > 1)), m_menuGroupSort);
			UIUtil.SetEnabledFast((uSubGroups > 1), m_menuGroupSortRec);

			UIUtil.SetEnabledFast((uSubGroups > 0), m_menuGroupExpand, m_menuGroupCollapse);

			bool bShowEmpty = false, bEnableEmpty = false;
			Debug.Assert(m_menuGroupEmptyRB.ShortcutKeys == Keys.None); // For bMenuVisible
			if(bMenuVisible && s.DatabaseOpened && pd.RecycleBinEnabled)
			{
				PwGroup pgRB = pd.RootGroup.FindGroup(pd.RecycleBinUuid, true);

				bShowEmpty = (pgRB != null);
				if(bContextMenu) bShowEmpty &= (pg == pgRB);

				if(bShowEmpty)
					bEnableEmpty = ((pgRB.Groups.UCount > 0) || (pgRB.Entries.UCount > 0));
			}
			m_menuGroupEmptyRB.Enabled = bEnableEmpty;
			m_milMain.SetCopyAvailable(m_menuGroupEmptyRB, bShowEmpty);

			if(bContextMenu)
				UIUtil.SetEnabledFast(s.DatabaseOpened, m_ctxGroupFind, m_ctxGroupFindProfiles);

			// 'Paste Group' is updated in the menu opening handler
			UIUtil.SetEnabledFast(s.DatabaseOpened, m_menuGroupDX, m_menuGroupClipCopy,
				m_menuGroupClipCopyPlain, m_menuGroupPrint, m_menuGroupExport);

			sls.Dispose();
		}

		private void UpdateUIGroupMenuState(MainAppState sMain, bool bMenuVisible)
		{
			UpdateUIGroupState(sMain, bMenuVisible, false);
		}

		private void UpdateUIGroupCtxState()
		{
			UpdateUIGroupState(null, true, true);
		}

		private void UpdateUIEntryState(MainAppState sMain, bool bMenuVisible,
			bool bContextMenu, DynamicMenu dynStrings, DynamicMenu dynBinaries)
		{
			MainAppState s = (sMain ?? GetMainAppState());
			PwEntry pe = s.SelectedEntry;
			bool bEntrySel = (s.EntriesSelected != 0);
			bool bEntrySel1 = (s.EntriesSelected == 1);
			bool bEntrySelM = (s.EntriesSelected >= 2);

			PwEntry[] vSel = (bMenuVisible ? GetSelectedEntries() : null);

			SuspendLayoutScope sls = new SuspendLayoutScope(true, m_menuMain,
				m_ctxPwList);

			m_menuEntryCopyUserName.Enabled = s.CanCopyUserName;
			m_milMain.SetCopyAvailable(m_menuEntryCopyUserName, !s.IsOneTan);

			m_menuEntryCopyPassword.Enabled = s.CanCopyPassword;
			if(bMenuVisible)
				m_menuEntryCopyPassword.Text = (s.IsOneTan ? KPRes.CopyTanMenu :
					KPRes.CopyPasswordMenu);

			UIUtil.SetEnabledFast(s.CanOpenUrl, m_menuEntryUrl, m_menuEntryOpenUrl,
				m_menuEntryCopyUrl);

			if(bMenuVisible)
			{
				AccessKeyManagerEx akStr = new AccessKeyManagerEx();
				AccessKeyManagerEx akBin = new AccessKeyManagerEx();

				string strBinSave = akBin.CreateText(KPRes.SaveFilesTo + "...", true);

				bool bStr = true, bHotp = false, bTotp = false, bBin1 = false;
				dynStrings.Clear();
				dynBinaries.Clear();

				if(bEntrySel1 && (pe != null))
				{
					Image imgCopy = Properties.Resources.B16x16_EditCopy;
					Image imgCopyP = Properties.Resources.B16x16_KGPG_Info;
					Image imgCopyOtp = Properties.Resources.B16x16_KGPG_Gen;
					Image imgShowOtp = Properties.Resources.B16x16_File_Locked;

					string strCopyTitle = akStr.RegisterText(KPRes.CopyTitle);
					string strCopyNotes = akStr.RegisterText(KPRes.CopyNotes);
					string strCopyHmacOtp = akStr.RegisterText(KPRes.CopyHmacOtp);
					string strShowHmacOtp = akStr.RegisterText(KPRes.ShowHmacOtp);
					string strCopyTimeOtp = akStr.RegisterText(KPRes.CopyTimeOtp);
					string strShowTimeOtp = akStr.RegisterText(KPRes.ShowTimeOtp);

					dynStrings.AddItem(strCopyTitle, imgCopy, new EntryDataCommand(
						EntryDataCommandType.CopyField, PwDefs.TitleField, null));
					dynStrings.AddItem(strCopyNotes, imgCopy, new EntryDataCommand(
						EntryDataCommandType.CopyField, PwDefs.NotesField, null));

					bool bFirstCustom = true;
					foreach(KeyValuePair<string, ProtectedString> kvp in pe.Strings)
					{
						if(PwDefs.IsStandardField(kvp.Key)) continue;

						if(kvp.Key.StartsWith(EntryUtil.HotpPrefix))
							bHotp = true;
						else if(kvp.Key.StartsWith(EntryUtil.TotpPrefix))
							bTotp = true;
						else
						{
							if(bFirstCustom)
							{
								dynStrings.AddSeparator();
								bFirstCustom = false;
							}

							string str = akStr.CreateText(KPRes.CopyObject, true,
								null, kvp.Key);
							Image img = (kvp.Value.IsProtected ? imgCopyP : imgCopy);

							dynStrings.AddItem(str, img, new EntryDataCommand(
								EntryDataCommandType.CopyField, kvp.Key, null));
						}
					}

					dynStrings.AddSeparator();

					string strError = (new FormatException()).Message;
					ToolStripMenuItem tsmiCopyHotp = dynStrings.AddItem(
						strCopyHmacOtp, imgCopyOtp, new EntryDataCommand(
							EntryDataCommandType.CopyValue, EntryUtil.HotpPlh, strError));
					ToolStripMenuItem tsmiShowHotp = dynStrings.AddItem(
						strShowHmacOtp, imgShowOtp, new EntryDataCommand(
							EntryDataCommandType.ShowValue, EntryUtil.HotpPlh, strError));

					ToolStripMenuItem tsmiCopyTotp = dynStrings.AddItem(
						strCopyTimeOtp, imgCopyOtp, m_edcCopyTotp);
					UIUtil.AssignShortcut(tsmiCopyTotp, Keys.Control | Keys.T, null, true);
					ToolStripMenuItem tsmiShowTotp = dynStrings.AddItem(
						strShowTimeOtp, imgShowOtp, m_edcShowTotp);
					UIUtil.AssignShortcut(tsmiShowTotp, Keys.Control | Keys.Shift | Keys.T, null, true);

					UIUtil.SetEnabledFast(bHotp, tsmiCopyHotp, tsmiShowHotp);
					UIUtil.SetEnabledFast(bTotp, tsmiCopyTotp, tsmiShowTotp);

					foreach(KeyValuePair<string, ProtectedBinary> kvp in pe.Binaries)
					{
						string str = akBin.CreateText(kvp.Key, true);
						Image img = FileIcons.GetImageForName(kvp.Key, null);

						EntryBinaryDataContext ctxBin = new EntryBinaryDataContext();
						ctxBin.Entry = pe;
						ctxBin.Name = kvp.Key;

						dynBinaries.AddItem(str, img, ctxBin);
						bBin1 = true;
					}
				}
				else
				{
					bStr = false;
					dynStrings.AddItem(m_strNoneP, null).Enabled = false;
				}

				bool bBinM = bBin1;
				if(!bBinM) // No attachment in primary entry => search others
				{
					ulong cBins = 0;
					foreach(PwEntry peSel in (vSel ?? MemUtil.EmptyArray<PwEntry>()))
					{
						if(peSel == null) { Debug.Assert(false); continue; }
						cBins += peSel.Binaries.UCount;
					}
					bBinM = (cBins != 0);

					dynBinaries.AddItem("(" + KPRes.Count + ": " +
						cBins.ToString() + ")", null).Enabled = false;
				}

				dynBinaries.AddSeparator();
				dynBinaries.AddItem(strBinSave, Properties.Resources.B16x16_Attach,
					EntryBinaryDataContext.SaveAll).Enabled = bBinM;

				m_menuEntryOtherData.Enabled = bStr;
				// m_milMain.SetCopyAvailable(m_menuEntryOtherData, bStr);
				m_menuEntryAttachments.Enabled = bBinM;
				m_milMain.SetCopyAvailable(m_menuEntryAttachments, bBinM);

				m_menuEntryAutoTypeAdv.Enabled = s.CanPerformAutoType;
				m_menuEntryAutoTypeAdv.Available = Program.Config.UI.ShowAdvAutoTypeCommands;
				foreach(ToolStripItem tsi in m_menuEntryAutoTypeAdv.DropDownItems)
				{
					tsi.Enabled = s.CanPerformAutoType;
				}
				foreach(ToolStripItem tsi in m_ctxEntryAutoTypeAdv.DropDownItems)
				{
					tsi.Enabled = s.CanPerformAutoType;
				}
				UIUtil.SetEnabledFast(bHotp && s.CanPerformAutoType,
					m_tsmiAutoTypeHotpMenu, m_tsmiAutoTypeHotpCtx);
				UIUtil.SetEnabledFast(bTotp && s.CanPerformAutoType,
					m_tsmiAutoTypeTotpMenu, m_tsmiAutoTypeTotpCtx);
			}

			m_menuEntryPerformAutoType.Enabled = s.CanPerformAutoType;

			UIUtil.SetEnabledFast(s.DatabaseOpened, m_menuEntryAdd);
			UIUtil.SetEnabledFast(bEntrySel, m_menuEntryEdit,
				m_menuEntryEditQuick, m_menuEntryIcon,
				m_menuEntryColor, m_menuEntryColorStandard,
				m_menuEntryColorLightRed, m_menuEntryColorLightGreen,
				m_menuEntryColorLightBlue, m_menuEntryColorLightYellow,
				m_menuEntryColorCustom, m_menuEntryTagAdd, m_menuEntryTagRemove,
				m_menuEntryTagNew, m_menuEntryExpiresNow, m_menuEntryExpiresNever,
				m_menuEntryDuplicate, m_menuEntryDelete);
			UIUtil.SetEnabledFast(bEntrySel1, m_menuEntryOtpGenSettings);

			if(bMenuVisible)
			{
				m_menuEntryEdit.Text = (bEntrySelM ? KPRes.EditEntriesCmd :
					KPRes.EditEntryCmd);
				m_menuEntryEditQuick.Text = (bEntrySelM ? KPRes.EditEntriesQuickCmd :
					KPRes.EditEntryQuickCmd);
				m_menuEntryDuplicate.Text = (bEntrySelM ? KPRes.DuplicateEntriesCmd :
					KPRes.DuplicateEntryCmd);
				m_menuEntryDelete.Text = (bEntrySelM ? KPRes.DeleteEntriesCmd :
					KPRes.DeleteEntryCmd);
			}

			UIUtil.SetEnabledFast((s.DatabaseOpened && (s.EntriesCount > 0)),
				m_menuEntrySelectAll);

			UIUtil.SetEnabledFast(bEntrySel, m_menuEntryRearrange, m_menuEntryMoveToGroup,
				// (bEntrySel && (m_pListSorter.Column < 0)), // Message box
				m_menuEntryMoveToTop, m_menuEntryMoveOneUp, m_menuEntryMoveOneDown,
				m_menuEntryMoveToBottom);

			if(bMenuVisible)
			{
				m_menuEntryMoveToTop.Text = (bEntrySelM ? KPRes.MoveEntriesTopCmd :
					KPRes.MoveEntryTopCmd);
				m_menuEntryMoveOneUp.Text = (bEntrySelM ? KPRes.MoveEntriesUpCmd :
					KPRes.MoveEntryUpCmd);
				m_menuEntryMoveOneDown.Text = (bEntrySelM ? KPRes.MoveEntriesDownCmd :
					KPRes.MoveEntryDownCmd);
				m_menuEntryMoveToBottom.Text = (bEntrySelM ? KPRes.MoveEntriesBottomCmd :
					KPRes.MoveEntryBottomCmd);

				UpdateMoveToPreviousParentGroupUI(null, vSel, m_menuEntryMoveToPreviousParent);
			}

			UIUtil.SetEnabledFast(bEntrySel, m_menuEntryCompare);

			UIUtil.SetEnabledFast(s.DatabaseOpened, m_menuEntryDX);
			// 'Paste Entry' is updated in the menu opening handler
			// and its keyboard shortcut is handled manually (i.e.
			// it's independent of the menu item state)
			UIUtil.SetEnabledFast(bEntrySel, m_menuEntryClipCopy,
				m_menuEntryClipCopyPlain, m_menuEntryPrint, m_menuEntryExport);

			if(bMenuVisible)
			{
				m_menuEntryClipCopy.Text = (bEntrySelM ? KPRes.CopyEntriesECmd :
					KPRes.CopyEntryECmd);
				m_menuEntryClipCopyPlain.Text = (bEntrySelM ? KPRes.CopyEntriesUCmd :
					KPRes.CopyEntryUCmd);
				m_menuEntryPrint.Text = (bEntrySelM ? KPRes.PrintEntriesCmd :
					KPRes.PrintEntryCmd);
				m_menuEntryExport.Text = (bEntrySelM ? KPRes.ExportEntriesCmd :
					KPRes.ExportEntryCmd);
			}

			sls.Dispose();
		}

		private void UpdateUIEntryMenuState(MainAppState sMain, bool bMenuVisible)
		{
			UpdateUIEntryState(sMain, bMenuVisible, false, m_dynStringsMenu,
				m_dynBinariesMenu);
		}

		private void UpdateUIEntryCtxState()
		{
			UpdateUIEntryState(null, true, true, m_dynStringsCtx,
				m_dynBinariesCtx);
		}

		private MainAppState GetMainAppState()
		{
			PwDatabase pd = m_docMgr.ActiveDatabase;
			PwEntry pe = GetSelectedEntry(true);

			MainAppState s = new MainAppState();

			s.FileLocked = IsFileLocked(null);
			s.DatabaseOpened = ((pd != null) && pd.IsOpen);
			s.EntriesCount = (s.DatabaseOpened ? m_lvEntries.Items.Count : 0);
			s.EntriesSelected = (s.DatabaseOpened ? m_lvEntries.SelectedIndices.Count : 0);
			s.EnableLockCmd = (s.DatabaseOpened || s.FileLocked);
			s.NoWindowShown = (GlobalWindowManager.WindowCount == 0);
			s.SelectedEntry = pe;

			if(pe != null)
			{
				if(s.EntriesSelected == 1)
				{
					s.CanCopyUserName = !pe.Strings.GetSafe(PwDefs.UserNameField).IsEmpty;
					s.CanCopyPassword = !pe.Strings.GetSafe(PwDefs.PasswordField).IsEmpty;
					s.CanPerformAutoType = pe.GetAutoTypeEnabled();
					s.IsOneTan = PwDefs.IsTanEntry(pe);
				}

				s.CanOpenUrl = ((s.EntriesSelected > 1) ||
					!pe.Strings.GetSafe(PwDefs.UrlField).IsEmpty);
			}

			s.LockUnlock = (s.FileLocked ? KPRes.LockMenuUnlock : KPRes.LockMenuLock);

			return s;
		}

		/// <summary>
		/// Set the main status bar text.
		/// </summary>
		/// <param name="strStatusText">New status bar text.</param>
		public void SetStatusEx(string strStatusText)
		{
			m_statusPartInfo.Text = (strStatusText ?? m_strStatusReady);
		}

		private void UpdateClipboardStatus()
		{
			// Fix values in case the maximum time has been changed in the
			// options while the countdown is running
			if((m_nClipClearMax < 0) && (m_nClipClearCur > 0))
				m_nClipClearCur = 0;
			else if((m_nClipClearCur > m_nClipClearMax) && (m_nClipClearMax >= 0))
				m_nClipClearCur = m_nClipClearMax;

			if((m_nClipClearCur > 0) && (m_nClipClearMax > 0))
				m_statusClipboard.Value = ((m_nClipClearCur * 100) / m_nClipClearMax);
			else if(m_nClipClearCur == 0)
				m_statusClipboard.Visible = false;
		}

		/// <summary>
		/// Start the clipboard countdown (set the current tick count to the
		/// maximum value and decrease it each second -- at 0 the clipboard
		/// is cleared automatically). This function is asynchronous.
		/// </summary>
		public void StartClipboardCountdown()
		{
			if(m_nClipClearMax >= 0)
			{
				m_nClipClearCur = m_nClipClearMax;

				m_statusClipboard.Visible = true;
				UpdateClipboardStatus();

				string strText = KPRes.ClipboardDataCopied + " " +
					KPRes.ClipboardClearInSeconds + ".";
				strText = strText.Replace(@"[PARAM]", m_nClipClearMax.ToString());

				SetStatusEx(strText);

				// if(m_ntfTray.Visible)
				//	m_ntfTray.ShowBalloonTip(0, KPRes.ClipboardAutoClear,
				//		strText, ToolTipIcon.Info);
			}
		}

		/// <summary>
		/// Gets the focused or first selected entry.
		/// </summary>
		/// <returns>Matching entry or <c>null</c>.</returns>
		public PwEntry GetSelectedEntry(bool bRequireSelected)
		{
			return GetSelectedEntry(bRequireSelected, false);
		}

		public PwEntry GetSelectedEntry(bool bRequireSelected, bool bGetLastSelectedEntry)
		{
			if(!m_docMgr.ActiveDatabase.IsOpen) return null;

			if(!bRequireSelected)
			{
				ListViewItem lviFocused = m_lvEntries.FocusedItem;
				if(lviFocused != null) return ((PwListItem)lviFocused.Tag).Entry;
			}

			ListView.SelectedListViewItemCollection coll = m_lvEntries.SelectedItems;
			if(coll.Count > 0)
			{
				ListViewItem lvi = coll[bGetLastSelectedEntry ? (coll.Count - 1) : 0];
				if(lvi != null) return ((PwListItem)lvi.Tag).Entry;
			}

			return null;
		}

		/// <summary>
		/// Get all selected entries.
		/// </summary>
		/// <returns>A list of all selected entries.</returns>
		public PwEntry[] GetSelectedEntries()
		{
			if(!m_docMgr.ActiveDatabase.IsOpen) return null;

			ListView.SelectedListViewItemCollection coll = m_lvEntries.SelectedItems;
			if(coll == null) { Debug.Assert(false); return null; }
			int n = coll.Count; // Getting Count sends a message
			if(n == 0) return null;

			PwEntry[] vSelected = new PwEntry[n];
			int i = 0;
			// LVSLVIC: one access by index requires O(n) time, thus use
			// enumerator instead (which requires O(1) for each element)
			foreach(ListViewItem lvi in coll)
			{
				if(i >= n) { Debug.Assert(false); break; }

				vSelected[i] = ((PwListItem)lvi.Tag).Entry;
				++i;
			}
			Debug.Assert(i == n);

			return vSelected;
		}

		public uint GetSelectedEntriesCount()
		{
			if(!m_docMgr.ActiveDatabase.IsOpen) return 0;

			return (uint)m_lvEntries.SelectedIndices.Count;
		}

		public PwGroup GetSelectedEntriesAsGroup()
		{
			PwGroup pg = new PwGroup(true, true);
			pg.IsVirtual = true;

			// Copying group properties would confuse users
			// PwGroup pgSel = GetSelectedGroup();
			// if(pgSel != null)
			// {
			//	pg.Name = pgSel.Name;
			//	pg.IconId = pgSel.IconId;
			//	pg.CustomIconUuid = pgSel.CustomIconUuid;
			// }

			PwEntry[] vSel = GetSelectedEntries();
			if((vSel == null) || (vSel.Length == 0)) return pg;

			foreach(PwEntry pe in vSel) pg.AddEntry(pe, false);

			return pg;
		}

		/// <summary>
		/// Get the currently selected group (in the group tree).
		/// </summary>
		public PwGroup GetSelectedGroup()
		{
			if(!m_docMgr.ActiveDatabase.IsOpen) return null;

			TreeNode tn = m_tvGroups.SelectedNode;
			Debug.Assert((tn == null) || (tn.Tag is PwGroup));
			return ((tn != null) ? (tn.Tag as PwGroup) : null);
		}

		private void SelectGroup(PwGroup pg, bool bUpdateUI)
		{
			if(pg == null) { Debug.Assert(false); return; }

			SelectGroup(GuiFindGroup(pg.Uuid, null), bUpdateUI);
		}

		private void SelectGroup(TreeNode tn, bool bUpdateUI)
		{
			if(tn == null) { Debug.Assert(false); return; }

			Debug.Assert(tn.Tag is PwGroup);
			if(bUpdateUI)
			{
				PwGroup pg = (tn.Tag as PwGroup);
				if(pg != null) pg.Touch(false);
			}

			++m_uBlockGroupSelectionEvent;

			if(tn != m_tvGroups.SelectedNode) m_tvGroups.SelectedNode = tn;
			tn.EnsureVisible();
			if(bUpdateUI) UpdateUI(false, null, false, null, true, null, false);

			--m_uBlockGroupSelectionEvent;
		}

		/// <summary>
		/// Create or set an entry list view item.
		/// </summary>
		/// <param name="pe">Entry.</param>
		/// <param name="lviTarget">If <c>null</c>, a new list view item is
		/// created and added to the list (a group is created if necessary).
		/// If not <c>null</c>, the properties are stored in this item (no
		/// list view group is created and the list view item is not added
		/// to the list).</param>
		/// <returns>Created or modified list view item.</returns>
		private ListViewItem SetListEntry(PwEntry pe, ListViewItem lviTarget)
		{
			if(pe == null) { Debug.Assert(false); return null; }

			ListViewItem lvi = (lviTarget ?? new ListViewItem());

			PwListItem pli = new PwListItem(pe);
			if(lviTarget == null) lvi.Tag = pli; // Lock below (when adding it)
			else
			{
				lock(m_asyncListUpdate.ListEditSyncObject) { lvi.Tag = pli; }
			}

			int iIndexHint = ((lviTarget != null) ? lviTarget.Index :
				m_lvEntries.Items.Count);

			if(pe.Expires && (pe.ExpiryTime <= m_dtCachedNow))
			{
				lvi.ImageIndex = (int)PwIcon.Expired;
				if(m_fontExpired != null) lvi.Font = m_fontExpired;
			}
			else // Not expired
			{
				// Reset font, if item was expired previously (i.e. has expired font)
				if((lviTarget != null) && (lvi.ImageIndex == (int)PwIcon.Expired))
					lvi.Font = m_lvEntries.Font;

				if(pe.CustomIconUuid.Equals(PwUuid.Zero))
					lvi.ImageIndex = (int)pe.IconId;
				else
					lvi.ImageIndex = (int)PwIcon.Count +
						m_docMgr.ActiveDatabase.GetCustomIconIndex(pe.CustomIconUuid);
			}

			if(m_bEntryGrouping && (lviTarget == null))
			{
				PwGroup pgContainer = pe.ParentGroup;
				PwGroup pgLast = ((m_lvgLastEntryGroup != null) ?
					(PwGroup)m_lvgLastEntryGroup.Tag : null);

				Debug.Assert(pgContainer != null);
				if(pgContainer != null)
				{
					if(pgContainer != pgLast)
					{
						m_lvgLastEntryGroup = new ListViewGroup(
							pgContainer.GetFullPath(true, false));
						m_lvgLastEntryGroup.Tag = pgContainer;

						m_lvEntries.Groups.Add(m_lvgLastEntryGroup);
					}

					lvi.Group = m_lvgLastEntryGroup;
				}
			}

			if(!UIUtil.ColorsEqual(pe.ForegroundColor, Color.Empty))
				lvi.ForeColor = pe.ForegroundColor;
			else if(lviTarget != null) lvi.ForeColor = m_lvEntries.ForeColor;
			else { Debug.Assert(UIUtil.ColorsEqual(lvi.ForeColor, m_lvEntries.ForeColor)); }

			if(!UIUtil.ColorsEqual(pe.BackgroundColor, Color.Empty))
				lvi.BackColor = pe.BackgroundColor;
			else if(lviTarget != null) lvi.BackColor = m_lvEntries.BackColor;
			else { Debug.Assert(UIUtil.ColorsEqual(lvi.BackColor, m_lvEntries.BackColor)); }

			bool bAsync;

			// m_bOnlyTans &= PwDefs.IsTanEntry(pe);
			if(m_bShowTanIndices && m_bOnlyTans)
			{
				string strIndex = pe.Strings.ReadSafe(PwDefs.TanIndexField);

				// KPF 1151
				if(Program.Config.MainWindow.EntryListShowDerefData &&
					SprEngine.MightDeref(strIndex))
					strIndex = AsyncPwListUpdate.SprCompileFn(strIndex, pli);

				if(strIndex.Length != 0) lvi.Text = strIndex;
				else lvi.Text = PwDefs.TanTitle;
			}
			else
			{
				string strMain = GetEntryFieldEx(pe, 0, true, out bAsync);
				lvi.Text = strMain;
				if(bAsync)
					m_asyncListUpdate.Queue(strMain, pli, iIndexHint, 0,
						AsyncPwListUpdate.SprCompileFn);
			}

			int nColumns = m_lvEntries.Columns.Count;
			if(lviTarget == null)
			{
				for(int iColumn = 1; iColumn < nColumns; ++iColumn)
				{
					string strSub = GetEntryFieldEx(pe, iColumn, true, out bAsync);
					lvi.SubItems.Add(strSub);
					if(bAsync)
						m_asyncListUpdate.Queue(strSub, pli, iIndexHint, iColumn,
							AsyncPwListUpdate.SprCompileFn);
				}
			}
			else
			{
				int nSubItems = lvi.SubItems.Count;
				for(int iColumn = 1; iColumn < nColumns; ++iColumn)
				{
					string strSub = GetEntryFieldEx(pe, iColumn, true, out bAsync);

					if(iColumn < nSubItems) lvi.SubItems[iColumn].Text = strSub;
					else lvi.SubItems.Add(strSub);

					if(bAsync)
						m_asyncListUpdate.Queue(strSub, pli, iIndexHint, iColumn,
							AsyncPwListUpdate.SprCompileFn);
				}

				Debug.Assert(lvi.SubItems.Count == nColumns);
			}

			if(lviTarget == null)
			{
				lock(m_asyncListUpdate.ListEditSyncObject)
				{
					m_lvEntries.Items.Add(lvi);
				}
			}
			return lvi;
		}

		private void AddEntriesToList(PwObjectList<PwEntry> lEntries)
		{
			if(lEntries == null) { Debug.Assert(false); return; }

			m_bEntryGrouping = m_lvEntries.ShowGroups;

			ListViewStateEx lvseCachedState = new ListViewStateEx(m_lvEntries);
			m_lvEntries.BeginUpdateEx();

			foreach(PwEntry pe in lEntries)
			{
				if(pe == null) { Debug.Assert(false); continue; }

				if(m_bEntryGrouping)
				{
					PwGroup pg = pe.ParentGroup;

					foreach(ListViewGroup lvg in m_lvEntries.Groups)
					{
						PwGroup pgList = (lvg.Tag as PwGroup);
						Debug.Assert(pgList != null);
						if((pgList != null) && (pg == pgList))
						{
							m_lvgLastEntryGroup = lvg;
							break;
						}
					}
				}

				SetListEntry(pe, null);
			}

			m_lvEntries.EndUpdateEx();
			Debug.Assert(lvseCachedState.CompareTo(m_lvEntries));
		}

		private void UpdateGroupList(PwGroup pgToSelect)
		{
			NotifyUserActivity();

			PwDatabase pd = m_docMgr.ActiveDatabase;
			PwGroup pgSel = (pgToSelect ?? GetSelectedGroup());

			PwGroup pgTop = null;
			TreeNode tnTop = m_tvGroups.TopNode;
			if(tnTop != null) pgTop = (tnTop.Tag as PwGroup);
			tnTop = null;

			++m_uBlockGroupSelectionEvent;
			m_tvGroups.BeginUpdate();
			m_tvGroups.Nodes.Clear();

			UpdateImageLists(false);

			m_dtCachedNow = DateTime.UtcNow;

			TreeNode tnSel = null;
			TreeNode tnRoot = (pd.IsOpen ? GuiAddGroupRec(m_tvGroups.Nodes,
				pd.RootGroup, pgSel, ref tnSel, pgTop, ref tnTop) : null);

			if(tnRoot != null) tnRoot.Expand();

			if(tnSel != null)
			{
				// Ensure all parent tree nodes are expanded
				List<TreeNode> lParents = new List<TreeNode>();
				TreeNode tnUp = tnSel.Parent;
				while(tnUp != null)
				{
					lParents.Add(tnUp);
					tnUp = tnUp.Parent;
				}
				for(int i = (lParents.Count - 1); i >= 0; --i)
					lParents[i].Expand();

				m_tvGroups.SelectedNode = tnSel;
			}
			else if(tnRoot != null) m_tvGroups.SelectedNode = tnRoot;

			// Restore view *after* changing the selection
			if(tnTop != null) m_tvGroups.TopNode = tnTop;

			m_tvGroups.EndUpdate();
			--m_uBlockGroupSelectionEvent;
		}

		private TreeNode GuiAddGroupRec(TreeNodeCollection tncContainer,
			PwGroup pg, PwGroup pgFind1, ref TreeNode tnFound1,
			PwGroup pgFind2, ref TreeNode tnFound2)
		{
			if(tncContainer == null) { Debug.Assert(false); return null; }
			if(pg == null) { Debug.Assert(false); return null; }

			PwDatabase pd = m_docMgr.ActiveDatabase;
			bool bExpired = (pg.Expires && (pg.ExpiryTime <= m_dtCachedNow));
			string strName = pg.Name; // + GetGroupSuffixText(pg);

			int nIconID = ((!pg.CustomIconUuid.Equals(PwUuid.Zero)) ?
				((int)PwIcon.Count + pd.GetCustomIconIndex(pg.CustomIconUuid)) :
				(int)pg.IconId);
			if(bExpired) nIconID = (int)PwIcon.Expired;

			TreeNode tn = new TreeNode(strName, nIconID, nIconID);
			tn.Tag = pg;
			UIUtil.SetGroupNodeToolTip(tn, pg);

			if((pg == pd.RootGroup) && (m_fontBoldTree != null))
				tn.NodeFont = m_fontBoldTree;
			else if(pd.RecycleBinEnabled && pg.Uuid.Equals(pd.RecycleBinUuid) &&
				(m_fontItalicTree != null))
				tn.NodeFont = m_fontItalicTree;
			else if(bExpired && (m_fontExpired != null))
				tn.NodeFont = m_fontExpired;

			tncContainer.Add(tn);

			foreach(PwGroup pgSub in pg.Groups)
				GuiAddGroupRec(tn.Nodes, pgSub, pgFind1, ref tnFound1,
					pgFind2, ref tnFound2);

			if(tn.Nodes.Count != 0)
			{
				bool bExpanded = tn.IsExpanded;
				if(bExpanded && !pg.IsExpanded) tn.Collapse();
				else if(!bExpanded && pg.IsExpanded) tn.Expand();
			}

			if(pg == pgFind1) tnFound1 = tn;
			if(pg == pgFind2) tnFound2 = tn;
			return tn;
		}

		/// <summary>
		/// Update the entries list. This function completely rebuilds the entries
		/// list. You must call this function after you've made any changes to
		/// the entries of the currently selected group. Note that if you only
		/// made small changes (like editing an existing entry), the
		/// <c>RefreshEntriesList</c> function could be a better choice, as it only
		/// updates currently listed items and doesn't rebuild the whole list as
		/// <c>UpdateEntryList</c>.
		/// </summary>
		/// <param name="pgSelected">Group whose entries should be shown. If this
		/// parameter is <c>null</c>, the entries of the currently selected group
		/// (groups view) are displayed, otherwise the entries of the <c>pgSelected</c>
		/// group are displayed.</param>
		private void UpdateEntryList(PwGroup pgSelected, bool bOnlyUpdateCurrentlyShown)
		{
			NotifyUserActivity();

			UpdateImageLists(false);

			PwEntry peTop = GetTopEntry(), peFocused = GetSelectedEntry(false);
			PwEntry[] vSelected = GetSelectedEntries();
			int iScrollY = NativeMethods.GetScrollPos(m_lvEntries.Handle,
				NativeMethods.ScrollBarDirection.SB_VERT);

			bool bSubEntries = Program.Config.MainWindow.ShowEntriesOfSubGroups;

			PwGroup pg = (pgSelected ?? GetSelectedGroup());

			if(bOnlyUpdateCurrentlyShown)
			{
				Debug.Assert(pgSelected == null);
				pg = GetCurrentEntries();
			}

			PwObjectList<PwEntry> pwlSource = ((pg != null) ?
				pg.GetEntries(bSubEntries) : new PwObjectList<PwEntry>());

			m_bOnlyTans = ListContainsOnlyTans(pwlSource);

			m_asyncListUpdate.CancelPendingUpdatesAsync();

			++m_uBlockEntrySelectionEvent; // KPB 1698
			m_lvEntries.BeginUpdateEx();

			lock(m_asyncListUpdate.ListEditSyncObject)
			{
				m_lvEntries.Items.Clear();
			}
			m_lvEntries.Groups.Clear();
			m_lvgLastEntryGroup = null;

			// m_bEntryGrouping = (((pg != null) ? pg.IsVirtual : false) || bSubEntries);
			m_bEntryGrouping = bSubEntries;
			if(pg != null)
			{
				PwDatabase pd = m_docMgr.ActiveDatabase;
				if(bOnlyUpdateCurrentlyShown && !m_lvEntries.ShowGroups &&
					EntryUtil.EntriesHaveSameParent(pwlSource) && pd.IsOpen)
				{
					// Just reorder, don't enable grouping
					EntryUtil.ReorderEntriesAsInDatabase(pwlSource, pd);
					peTop = null; // Don't scroll to previous top item
				}
				else m_bEntryGrouping |= pg.IsVirtual;
			}
			int iLg = Program.Config.MainWindow.ListGrouping;
			if((iLg & (int)AceListGrouping.Primary) == (int)AceListGrouping.On)
				m_bEntryGrouping = true;
			else if((iLg & (int)AceListGrouping.Primary) == (int)AceListGrouping.Off)
				m_bEntryGrouping = false;
			m_lvEntries.ShowGroups = m_bEntryGrouping;

			m_dtCachedNow = DateTime.UtcNow;

			ListViewItem lviTop = null, lviFocused = null;
			ListViewStateEx lvseCachedState = new ListViewStateEx(m_lvEntries);

			if(pg != null)
			{
				foreach(PwEntry pe in pwlSource)
				{
					ListViewItem lvi = SetListEntry(pe, null);

					if(vSelected != null)
					{
						if(Array.IndexOf(vSelected, pe) >= 0)
							lvi.Selected = true;
					}

					if(pe == peTop) lviTop = lvi;
					if(pe == peFocused) lviFocused = lvi;
				}
			}

			Debug.Assert(lvseCachedState.CompareTo(m_lvEntries));

			if(lviFocused != null)
				UIUtil.SetFocusedItem(m_lvEntries, lviFocused, false);

			Debug.Assert(m_bEntryGrouping == m_lvEntries.ShowGroups);
			if(UIUtil.GetGroupsEnabled(m_lvEntries))
			{
				// Test lviTop to ensure we're not scrolling an unrelated list
				if((lviTop != null) && (iScrollY > 0))
					NativeMethods.Scroll(m_lvEntries, 0, iScrollY);
				lviTop = null; // Prevent normal scrolling below
			}

			m_lvEntries.EndUpdateEx(lviTop);
			--m_uBlockEntrySelectionEvent;

			// Switch the view *after* EndUpdate, otherwise drawing bug;
			// https://sourceforge.net/p/keepass/bugs/1651/
			if(m_bSimpleTanView && (m_lvEntries.Items.Count != 0))
				UIUtil.SetView(m_lvEntries, (m_bOnlyTans ? View.List : View.Details));
			else UIUtil.SetView(m_lvEntries, View.Details);

			// Resize columns *after* EndUpdate, otherwise sizing problem
			// caused by the scrollbar
			if(Program.Config.MainWindow.EntryListAutoResizeColumns &&
				(m_lvEntries.View == View.Details))
				UIUtil.ResizeColumns(m_lvEntries, true);
		}

		/// <summary>
		/// Refresh the entries list. All currently displayed entries are updated.
		/// If you made changes to the list that change the number of visible entries
		/// (like adding or removing an entry), you must use the <c>UpdateEntryList</c>
		/// function instead.
		/// </summary>
		public void RefreshEntriesList()
		{
			UpdateImageLists(false); // Important

			m_lvEntries.BeginUpdateEx();
			m_dtCachedNow = DateTime.UtcNow;

			foreach(ListViewItem lvi in m_lvEntries.Items)
			{
				SetListEntry(((PwListItem)lvi.Tag).Entry, lvi);
			}

			m_lvEntries.EndUpdateEx();
		}

		private PwEntry GetTopEntry()
		{
			PwEntry peTop = null;
			try
			{
				int idxTop = UIUtil.GetTopVisibleItem(m_lvEntries);
				if(idxTop >= 0)
					peTop = ((PwListItem)m_lvEntries.Items[idxTop].Tag).Entry;
			}
			catch(Exception) { Debug.Assert(false); }

			return peTop;
		}

		private void SortPasswordList(bool bEnableSorting, int nColumn,
			SortOrder? soForce, bool bUpdateEntryList)
		{
			AceColumnType colType = GetAceColumn(nColumn).Type;
			bool bReset = false;

			if(bEnableSorting)
			{
				bool bSortTimes = AceColumn.IsTimeColumn(colType);
				bool bSortNaturally = (colType != AceColumnType.Uuid);

				int nOldColumn = m_pListSorter.Column;
				SortOrder sortOrder = m_pListSorter.Order;

				if(soForce.HasValue) sortOrder = soForce.Value;
				else if(nColumn == nOldColumn)
				{
					if(sortOrder == SortOrder.None)
						sortOrder = SortOrder.Ascending;
					else if(sortOrder == SortOrder.Ascending)
						sortOrder = SortOrder.Descending;
					else if(sortOrder == SortOrder.Descending)
						sortOrder = SortOrder.None;
					else { Debug.Assert(false); }
				}
				else sortOrder = SortOrder.Ascending;

				if(sortOrder != SortOrder.None)
				{
					m_pListSorter = new ListSorter(nColumn, sortOrder,
						bSortNaturally, bSortTimes);
					m_lvEntries.ListViewItemSorter = m_pListSorter;
				}
				else bReset = true;
			}
			else bReset = true;

			if(bReset)
			{
				m_pListSorter = new ListSorter();
				m_lvEntries.ListViewItemSorter = null;
			}

			if(bUpdateEntryList) UpdateEntryList(null, true);

			UpdateColumnSortingIcons();
			UpdateUI(false, null, false, null, false, null, false); // KPB 1134
		}

		private void UpdateColumnSortingIcons()
		{
			if(UIUtil.SetSortIcon(m_lvEntries, m_pListSorter.Column,
				m_pListSorter.Order)) return;

			// if(m_lvEntries.SmallImageList == null) return;

			if(m_pListSorter.Column < 0) { Debug.Assert(m_lvEntries.ListViewItemSorter == null); }

			string strAsc = "  \u2191"; // Must have same length
			string strDsc = "  \u2193"; // Must have same length
			if(WinUtil.IsWindows9x || WinUtil.IsWindows2000 || WinUtil.IsWindowsXP ||
				NativeLib.IsUnix())
			{
				strAsc = @"  ^";
				strDsc = @"  v";
			}
			else if(WinUtil.IsAtLeastWindowsVista)
			{
				strAsc = "  \u25B3";
				strDsc = "  \u25BD";
			}

			foreach(ColumnHeader ch in m_lvEntries.Columns)
			{
				string strCur = ch.Text, strNew = null;

				if(strCur.EndsWith(strAsc) || strCur.EndsWith(strDsc))
				{
					strNew = strCur.Substring(0, strCur.Length - strAsc.Length);
					strCur = strNew;
				}

				if((ch.Index == m_pListSorter.Column) &&
					(m_pListSorter.Order != SortOrder.None))
				{
					if(m_pListSorter.Order == SortOrder.Ascending)
						strNew = strCur + strAsc;
					else if(m_pListSorter.Order == SortOrder.Descending)
						strNew = strCur + strDsc;
				}

				if(strNew != null) ch.Text = strNew;
			}
		}

		private void ShowEntryView(bool bShow)
		{
			UIUtil.SetChecked(m_menuViewShowEntryView, bShow);

			Program.Config.MainWindow.EntryView.Show = bShow;

			m_richEntryView.Visible = bShow;
			m_splitHorizontal.Panel2Collapsed = !bShow;
		}

		private void ShowEntryDetails(PwEntry pe)
		{
			if(pe == null)
			{
				m_richEntryView.Text = string.Empty;
				m_strLastEntryViewRtf = null;
				return;
			}

			RichTextBuilder rb = new RichTextBuilder();

			AceFont af = Program.Config.UI.StandardFont;
			Font fontUI = (UISystemFonts.ListFont ?? m_lvEntries.Font);
			// string strFontFace = (af.OverrideUIDefault ? af.Family : fontUI.Name);
			// float fFontSize = (af.OverrideUIDefault ? af.ToFont().SizeInPoints : fontUI.SizeInPoints);
			if(af.OverrideUIDefault) rb.DefaultFont = af.ToFont();
			else rb.DefaultFont = fontUI;

			bool bHorz = (m_splitHorizontal.Orientation == Orientation.Horizontal);
			string strItemSeparator = (bHorz ? ". " : Environment.NewLine);
			string strItemTerminator = (bHorz ? "." : string.Empty);

			List<KeyValuePair<string, bool>> lLinks = new List<KeyValuePair<string, bool>>();
			GAction<string, string> fSetLink = delegate(string strPre, string strLink)
			{
				if(!string.IsNullOrEmpty(strPre))
					lLinks.Add(new KeyValuePair<string, bool>(strPre, false));
				if(!string.IsNullOrEmpty(strLink))
					lLinks.Add(new KeyValuePair<string, bool>(strLink, true));
			};

			// StringBuilder sb = new StringBuilder();
			// StrUtil.InitRtf(sb, strFontFace, fFontSize);

			rb.Append(KPRes.Group, FontStyle.Bold, null, null, ":", " ");
			PwGroup pg = pe.ParentGroup;
			if(pg != null)
			{
				rb.Append(pg.Name);
				fSetLink(KPRes.Group + ": ", pg.Name);
			}

			AceMainWindow mw = Program.Config.MainWindow;
			EvAppendEntryField(rb, strItemSeparator, KPRes.Title,
				mw.IsColumnHidden(AceColumnType.Title) ? PwDefs.HiddenPassword :
				pe.Strings.ReadSafe(PwDefs.TitleField), pe, null);
			EvAppendEntryField(rb, strItemSeparator, KPRes.UserName,
				mw.IsColumnHidden(AceColumnType.UserName) ? PwDefs.HiddenPassword :
				pe.Strings.ReadSafe(PwDefs.UserNameField), pe, null);
			EvAppendEntryField(rb, strItemSeparator, KPRes.Password,
				mw.IsColumnHidden(AceColumnType.Password) ? PwDefs.HiddenPassword :
				pe.Strings.ReadSafe(PwDefs.PasswordField), pe, null);
			EvAppendEntryField(rb, strItemSeparator, KPRes.Url,
				mw.IsColumnHidden(AceColumnType.Url) ? PwDefs.HiddenPassword :
				pe.Strings.ReadSafe(PwDefs.UrlField), pe, fSetLink);

			if(pe.Expires)
				EvAppendEntryField(rb, strItemSeparator, KPRes.ExpiryTime,
					TimeUtil.ToDisplayString(pe.ExpiryTime), null, null);

			if(pe.Binaries.UCount != 0)
			{
				EvAppendEntryField(rb, strItemSeparator, KPRes.Attachments,
					pe.Binaries.KeysToString(), null, null);

				fSetLink(KPRes.Attachments + ": ", null);
				foreach(KeyValuePair<string, ProtectedBinary> kvp in pe.Binaries)
					fSetLink(null, kvp.Key);
			}

			EvAppendEntryField(rb, strItemSeparator, KPRes.Tags,
				TagUtil.TagsInheritedToString(pe.Tags, pe.ParentGroup), null, null);
			EvAppendEntryField(rb, strItemSeparator, KPRes.UrlOverride,
				pe.OverrideUrl, pe, null);

			EvAppendEntryField(rb, strItemSeparator, KPRes.CreationTime,
				TimeUtil.ToDisplayString(pe.CreationTime), null, null);
			EvAppendEntryField(rb, strItemSeparator, KPRes.LastModificationTime,
				TimeUtil.ToDisplayString(pe.LastModificationTime), null, null);
			if((Program.Config.UI.UIFlags & (ulong)AceUIFlags.ShowLastAccessTime) != 0)
				EvAppendEntryField(rb, strItemSeparator, KPRes.LastAccessTime,
					TimeUtil.ToDisplayString(pe.LastAccessTime), null, null);

			foreach(KeyValuePair<string, ProtectedString> kvp in pe.Strings)
			{
				if(PwDefs.IsStandardField(kvp.Key)) continue;

				string strCustomValue = (mw.ShouldHideCustomString(kvp.Key,
					kvp.Value) ? PwDefs.HiddenPassword : kvp.Value.ReadString());
				EvAppendEntryField(rb, strItemSeparator, kvp.Key, strCustomValue, pe, null);
			}

			rb.Append(strItemTerminator);

			string strNotes = (mw.IsColumnHidden(AceColumnType.Notes) ?
				PwDefs.HiddenPassword : pe.Strings.ReadSafe(PwDefs.NotesField).Trim());
			if(strNotes.Length != 0)
			{
				rb.AppendLine();
				rb.AppendLine();

				KeyValuePair<string, string> kvpBold = RichTextBuilder.GetStyleIdCodes(
					FontStyle.Bold);
				KeyValuePair<string, string> kvpItalic = RichTextBuilder.GetStyleIdCodes(
					FontStyle.Italic);
				KeyValuePair<string, string> kvpUnderline = RichTextBuilder.GetStyleIdCodes(
					FontStyle.Underline);

				strNotes = strNotes.Replace(@"<b>", kvpBold.Key);
				strNotes = strNotes.Replace(@"</b>", kvpBold.Value);
				strNotes = strNotes.Replace(@"<i>", kvpItalic.Key);
				strNotes = strNotes.Replace(@"</i>", kvpItalic.Value);
				strNotes = strNotes.Replace(@"<u>", kvpUnderline.Key);
				strNotes = strNotes.Replace(@"</u>", kvpUnderline.Value);

				rb.Append(strNotes);
			}

			// sb.Append("\\pard }");
			// m_richEntryView.Rtf = sb.ToString();

			// If the RTF is the same, do not change the selection/view;
			// https://sourceforge.net/p/keepass/discussion/329220/thread/509843717a/
			if(!rb.Build(m_richEntryView, false, ref m_strLastEntryViewRtf))
				return;

			UIUtil.RtfLinkifyUrls(m_richEntryView); // Before manual URL link.
			UIUtil.RtfLinkifyReferences(m_richEntryView, false);
			UIUtil.RtfLinkifyTexts(m_richEntryView, lLinks, true); // After auto. URL link.
			m_richEntryView.Select(0, 0);
		}

		private void EvAppendEntryField(RichTextBuilder rb,
			string strItemSeparator, string strName, string strRawValue,
			PwEntry peSprCompile, GAction<string, string> fSetLink)
		{
			if(strRawValue == null) { Debug.Assert(false); return; }

			string strValue = strRawValue.Trim();
			if(strValue.Length == 0) return;

			rb.Append(strName, FontStyle.Bold, strItemSeparator, null, ":", " ");

			string strLink = strValue;

			if((peSprCompile == null) || !SprEngine.MightDeref(strValue))
				rb.Append(strValue);
			else
			{
				string strCmp = SprEngine.Compile(strValue,
					GetEntryListSprContext(peSprCompile,
					m_docMgr.SafeFindContainerOf(peSprCompile)));
				if(strCmp == strValue) rb.Append(strValue);
				else
				{
					strCmp = strCmp.Trim();

					rb.Append(strCmp);
					rb.Append(" - ");
					rb.Append(strValue, FontStyle.Italic);

					strLink = strCmp;
				}
			}

			if((fSetLink != null) && (strLink != PwDefs.HiddenPassword))
				fSetLink(strName + ": ", strLink);
		}

		private void PerformDefaultAction(object sender, EventArgs e, PwEntry pe,
			int colID)
		{
			if(pe == null) { Debug.Assert(false); return; }

			if(this.DefaultEntryAction != null)
			{
				CancelEntryEventArgs args = new CancelEntryEventArgs(pe, colID);
				this.DefaultEntryAction(sender, args);
				if(args.Cancel) return;
			}

			bool bShift = ((Control.ModifierKeys & Keys.Shift) != Keys.None);
			bool bCnt = false;

			AceColumn col = GetAceColumn(colID);
			AceColumnType colType = col.Type;
			switch(colType)
			{
				case AceColumnType.Title:
					if(bShift)
						bCnt = ClipboardUtil.CopyAndMinimize(pe.Strings.GetSafe(
							PwDefs.TitleField), true, this, pe, m_docMgr.ActiveDatabase);
					else
					{
						if(PwDefs.IsTanEntry(pe)) OnEntryCopyPassword(sender, e);
						else OnEntryEdit(sender, e);
					}
					break;
				case AceColumnType.UserName:
					OnEntryCopyUserName(sender, e);
					break;
				case AceColumnType.Password:
					OnEntryCopyPassword(sender, e);
					break;
				case AceColumnType.Url:
					PerformDefaultUrlAction(null, null);
					break;
				case AceColumnType.Notes:
					bCnt = ClipboardUtil.CopyAndMinimize(pe.Strings.GetSafe(
						PwDefs.NotesField), true, this, pe, m_docMgr.ActiveDatabase);
					break;
				case AceColumnType.CreationTime:
					bCnt = ClipboardUtil.CopyAndMinimize(TimeUtil.ToDisplayString(
						pe.CreationTime), true, this, pe, null);
					break;
				case AceColumnType.LastModificationTime:
					bCnt = ClipboardUtil.CopyAndMinimize(TimeUtil.ToDisplayString(
						pe.LastModificationTime), true, this, pe, null);
					break;
				case AceColumnType.LastAccessTime:
					bCnt = ClipboardUtil.CopyAndMinimize(TimeUtil.ToDisplayString(
						pe.LastAccessTime), true, this, pe, null);
					break;
				case AceColumnType.ExpiryTime:
					if(pe.Expires)
						bCnt = ClipboardUtil.CopyAndMinimize(TimeUtil.ToDisplayString(
							pe.ExpiryTime), true, this, pe, null);
					else
						bCnt = ClipboardUtil.CopyAndMinimize(m_strNeverExpires,
							true, this, pe, null);
					break;
				case AceColumnType.Attachment:
				case AceColumnType.AttachmentCount:
					PerformDefaultAttachmentAction();
					break;
				case AceColumnType.Uuid:
					bCnt = ClipboardUtil.CopyAndMinimize(pe.Uuid.ToHexString(),
						true, this, pe, null);
					break;
				case AceColumnType.CustomString:
					bCnt = ClipboardUtil.CopyAndMinimize(pe.Strings.ReadSafe(
						col.CustomName), true, this, pe, m_docMgr.ActiveDatabase);
					break;
				case AceColumnType.PluginExt:
					if(Program.ColumnProviderPool.SupportsCellAction(col.CustomName))
						Program.ColumnProviderPool.PerformCellAction(col.CustomName, pe);
					else
						bCnt = ClipboardUtil.CopyAndMinimize(
							Program.ColumnProviderPool.GetCellData(col.CustomName, pe),
							true, this, pe, m_docMgr.ActiveDatabase);
					break;
				case AceColumnType.OverrideUrl:
					bCnt = ClipboardUtil.CopyAndMinimize(pe.OverrideUrl,
						true, this, pe, null);
					break;
				case AceColumnType.Tags:
					bCnt = ClipboardUtil.CopyAndMinimize(TagUtil.TagsInheritedToString(
						pe.Tags, pe.ParentGroup), true, this, pe, null);
					break;
				case AceColumnType.ExpiryTimeDateOnly:
					if(pe.Expires)
						bCnt = ClipboardUtil.CopyAndMinimize(TimeUtil.ToDisplayStringDateOnly(
							pe.ExpiryTime), true, this, pe, null);
					else
						bCnt = ClipboardUtil.CopyAndMinimize(m_strNeverExpires,
							true, this, pe, null);
					break;
				case AceColumnType.Size:
					bCnt = ClipboardUtil.CopyAndMinimize(StrUtil.FormatDataSizeKB(
						pe.GetSize()), true, this, pe, null);
					break;
				case AceColumnType.HistoryCount:
					EditSelectedEntry(PwEntryFormTab.History);
					break;
				case AceColumnType.LastPasswordModTime:
					bCnt = ClipboardUtil.CopyAndMinimize(TimeUtil.ToDisplayString(
						EntryUtil.GetLastPasswordModTime(pe)), true, this, pe, null);
					break;
				case AceColumnType.AutoTypeEnabled:
					object oBlocker;
					AutoType.GetEnabledText(pe, out oBlocker);
					if((oBlocker == null) || (oBlocker is PwEntry))
						EditSelectedEntry(PwEntryFormTab.AutoType);
					else
					{
						PwGroup pg = (oBlocker as PwGroup);
						if(pg != null)
						{
							UpdateUI(false, null, true, pg, true, null, false);
							EditSelectedGroup(GroupFormTab.AutoType);
						}
						else { Debug.Assert(false); }
					}
					break;
				case AceColumnType.AutoTypeSequences:
					EditSelectedEntry(PwEntryFormTab.AutoType);
					break;
				default:
					Debug.Assert(false);
					break;
			}

			if(bCnt) StartClipboardCountdown();
		}

		// obForceOpen true: open, false: copy, null: by option and UI
		internal void PerformDefaultUrlAction(PwEntry[] vOptEntries, bool? obForceOpen)
		{
			PwEntry[] v = (vOptEntries ?? GetSelectedEntries());
			if((v == null) || (v.Length == 0)) { Debug.Assert(false); return; }

			bool bCopy = Program.Config.MainWindow.CopyUrlsInsteadOfOpening;
			if(obForceOpen.HasValue) bCopy = !obForceOpen.Value;
			else if((Control.ModifierKeys & Keys.Shift) != Keys.None)
				bCopy = !bCopy;

			if(bCopy)
			{
				bool bCnt;
				// When copying multiple URLs, the entry list is refreshed twice:
				// once by UrlsToString (required for the case that copying to
				// the clipboard fails) and once by CopyAndMinimize
				if((v.Length == 1) && (v[0] != null))
					bCnt = ClipboardUtil.CopyAndMinimize(v[0].Strings.GetSafe(PwDefs.UrlField),
						true, this, v[0], m_docMgr.SafeFindContainerOf(v[0]));
				else
				{
					string str = UrlsToString(v, true, true);
					bCnt = ClipboardUtil.CopyAndMinimize(str, true, this, null, null);
				}

				if(bCnt) StartClipboardCountdown();
			}
			else // Open
			{
				foreach(PwEntry pe in v) WinUtil.OpenEntryUrl(pe);
			}
		}

		private void PerformDefaultAttachmentAction()
		{
			PwEntry pe = GetSelectedEntry(false);
			if(pe == null) return;

			foreach(KeyValuePair<string, ProtectedBinary> kvp in pe.Binaries)
			{
				ExecuteBinaryOpen(pe, kvp.Key);
				break;
			}
		}

		private void ExecuteBinaryOpen(PwEntry pe, string strBinName)
		{
			EntryBinaryDataContext ctx = new EntryBinaryDataContext();
			ctx.Entry = pe;
			ctx.Name = strBinName;

			OnEntryBinaryClick(null, new DynamicMenuEventArgs(strBinName, ctx));
		}

		private string UrlsToString(PwEntry[] vEntries, bool bActive, bool bTouch)
		{
			if((vEntries == null) || (vEntries.Length == 0)) return string.Empty;

			StringBuilder sb = new StringBuilder();
			for(int i = 0; i < vEntries.Length; ++i)
			{
				PwEntry pe = vEntries[i];
				if(pe == null) { Debug.Assert(false); continue; }

				if(i != 0) sb.AppendLine();

				PwDatabase pd = m_docMgr.SafeFindContainerOf(pe);

				string strUrl = pe.Strings.ReadSafe(PwDefs.UrlField);
				strUrl = SprEngine.Compile(strUrl, new SprContext(pe, pd,
					(bActive ? SprCompileFlags.All : SprCompileFlags.NonActive)));

				if(bTouch) pe.Touch(false);

				sb.Append(strUrl);
			}

			if(bActive || bTouch)
			{
				// SprEngine.Compile might have modified the database
				RefreshEntriesList();
				UpdateUIState(false);
			}

			return sb.ToString();
		}

		private delegate void PerformSearchQuickDelegate(string strSearch,
			bool bForceShowExpired, bool bRespectEntrySearchingDisabled);

		private void PerformSearchQuick(string strSearch, bool bForceShowExpired,
			bool bRespectEntrySearchingDisabled)
		{
			if(strSearch == null) { Debug.Assert(false); strSearch = string.Empty; }

			SearchParameters sp = new SearchParameters();

			if(strSearch.StartsWith("//") && strSearch.EndsWith("//") &&
				(strSearch.Length > 4))
			{
				sp.SearchString = strSearch.Substring(2, strSearch.Length - 4);
				sp.SearchMode = PwSearchMode.Regular;
			}
			else sp.SearchString = strSearch;

			sp.SearchInPasswords = Program.Config.MainWindow.QuickFindSearchInPasswords;
			sp.SearchInTags = sp.SearchInUuids = sp.SearchInGroupPaths =
				sp.SearchInGroupNames = true;

			sp.ExcludeExpired = (!bForceShowExpired &&
				Program.Config.MainWindow.QuickFindExcludeExpired);
			sp.RespectEntrySearchingDisabled = bRespectEntrySearchingDisabled;

			SearchUtil.SetTransformation(sp, (Program.Config.MainWindow.QuickFindDerefData ?
				SearchUtil.StrTrfDeref : string.Empty));

			PerformSearch(sp, false, Program.Config.MainWindow.FocusResultsAfterQuickFind);
		}

		private void ShowExpiredEntries(bool bOnlyIfExists, bool bShowExpired,
			int iExpInDays, bool bInMonths30, bool bFocusEntryList)
		{
			if(!bShowExpired && (iExpInDays <= 0)) return;
			Debug.Assert(iExpInDays >= 0); // 0 = do not show expiring soon

			PwDatabase pd = m_docMgr.ActiveDatabase;
			if((pd == null) || !pd.IsOpen || (pd.RootGroup == null)) return;

			PwGroup pgResults = new PwGroup(true, true, string.Empty, PwIcon.Expired);
			pgResults.IsVirtual = true;

			bool bExpInP = bShowExpired; // Past
			bool bExpInF = (iExpInDays == int.MaxValue); // Future
			bool bExpInD = ((iExpInDays > 0) && !bExpInF);

			DateTime dtNow = DateTime.UtcNow;
			DateTime dtLimit = dtNow;
			if(bExpInD)
			{
				try
				{
					if(bInMonths30 && ((iExpInDays % 30) == 0))
						dtLimit = dtNow.AddMonths(iExpInDays / 30);
					else
					{
						Debug.Assert(!bInMonths30);
						dtLimit = dtNow.AddDays(iExpInDays);
					}

					dtLimit = TimeUtil.ToLocal(dtLimit, false);
					dtLimit = dtLimit.Date.Add(new TimeSpan(23, 59, 59));
					dtLimit = TimeUtil.ToUtc(dtLimit, false);
				}
				catch(Exception) { Debug.Assert(false); }
			}

			EntryHandler eh = delegate(PwEntry pe)
			{
				if(!pe.Expires) return true;
				if(!pe.GetSearchingEnabled()) return true;
				if(PwDefs.IsTanEntry(pe)) return true; // Exclude TANs

				int iRelNow = pe.ExpiryTime.CompareTo(dtNow);

				if((bExpInP && (iRelNow <= 0)) ||
					(bExpInD && (pe.ExpiryTime <= dtLimit) && (iRelNow > 0)) ||
					(bExpInF && (iRelNow > 0)))
					pgResults.AddEntry(pe, false, false);
				return true;
			};

			pd.RootGroup.TraverseTree(TraversalMethod.PreOrder, null, eh);

			if((pgResults.Entries.UCount != 0) || !bOnlyIfExists)
			{
				SearchParameters sp = new SearchParameters();
				sp.RespectEntrySearchingDisabled = true;

				ShowSearchResults(pgResults, sp, pd.RootGroup, bFocusEntryList);
			}
		}

		public void PerformExport(PwGroup pgDataSource, bool bExportDeleted)
		{
			PwDatabase pd = m_docMgr.ActiveDatabase;
			if((pd == null) || !pd.IsOpen) { Debug.Assert(false); return; }

			if(!AppPolicy.Try(AppPolicyId.Export)) return;

			PwGroup pg = (pgDataSource ?? pd.RootGroup);
			PwExportInfo pwInfo = new PwExportInfo(pg, pd, bExportDeleted);

			MessageService.ExternalIncrementMessageCount();
			ShowWarningsLogger swLogger = CreateShowWarningsLogger();

			ExportUtil.Export(pwInfo, swLogger);

			MessageService.ExternalDecrementMessageCount();
			UpdateUIState(false);
		}

		internal static IOConnectionInfo CompleteConnectionInfo(IOConnectionInfo ioc,
			bool bSave, bool bCanRememberCred, bool bTestConnection, bool bForceShow)
		{
			if(ioc == null) { Debug.Assert(false); return null; }
			if(!bForceShow && ((ioc.CredSaveMode == IOCredSaveMode.SaveCred) ||
				ioc.IsLocalFile() || ioc.IsComplete))
				return ioc.CloneDeep();

			IOConnectionForm dlg = new IOConnectionForm();
			dlg.InitEx(bSave, ioc, bCanRememberCred, bTestConnection);
			if(UIUtil.ShowDialogNotValue(dlg, DialogResult.OK)) return null;

			IOConnectionInfo iocResult = dlg.IOConnectionInfo;
			UIUtil.DestroyForm(dlg);
			return iocResult;
		}

		internal IOConnectionInfo CompleteConnectionInfoUsingMru(IOConnectionInfo ioc)
		{
			if(ioc == null) { Debug.Assert(false); return null; }
			if(ioc.Password.Length > 0) return ioc;

			for(uint u = 0; u < m_mruList.ItemCount; ++u)
			{
				IOConnectionInfo iocMru = (m_mruList.GetItem(u).Value as IOConnectionInfo);
				if(iocMru == null) { Debug.Assert(false); continue; }

				if(iocMru.Path.Equals(ioc.Path, StrUtil.CaseIgnoreCmp))
				{
					if((ioc.UserName.Length > 0) && !ioc.UserName.Equals(
						iocMru.UserName, StrUtil.CaseIgnoreCmp))
						continue;

					return iocMru.CloneDeep();
				}
			}

			return ioc;
		}

		/// <summary>
		/// Open a database. This function opens the specified database and
		/// updates the user interface.
		/// </summary>
		public void OpenDatabase(IOConnectionInfo ioConnection, CompositeKey cmpKey,
			bool bOpenLocal)
		{
			if(!IsCommandTypeInvokable(null, AppCommandType.Window)) { Debug.Assert(false); return; }

			if((ioConnection != null) && (ioConnection.Path.Length > 0) &&
				ioConnection.IsLocalFile() && !UrlUtil.IsAbsolutePath(ioConnection.Path))
			{
				ioConnection = ioConnection.CloneDeep();
				ioConnection.Path = UrlUtil.MakeAbsolutePath(WinUtil.GetExecutable(),
					ioConnection.Path);
			}

			if(!m_bFormLoaded && Program.Config.Application.Start.MinimizedAndLocked &&
				(ioConnection != null) && (ioConnection.Path.Length > 0))
			{
				PwDocument ds = m_docMgr.CreateNewDocument(true);
				ds.LockedIoc = ioConnection.CloneDeep();
				UpdateUI(true, ds, true, null, true, null, false);
				return;
			}

			SaveWindowState(); // KPF 1093

			IOConnectionInfo ioc;
			if(ioConnection == null)
			{
				if(bOpenLocal)
				{
					OpenFileDialogEx ofdDb = UIUtil.CreateOpenFileDialog(KPRes.OpenDatabaseFile,
						UIUtil.CreateFileTypeFilter(AppDefs.FileExtension.FileExt,
						KPRes.KdbxFiles, true), 1, null, false,
						AppDefs.FileDialogContext.Database);

					GlobalWindowManager.AddDialog(ofdDb.FileDialog);
					DialogResult dr = ofdDb.ShowDialog();
					GlobalWindowManager.RemoveDialog(ofdDb.FileDialog);
					if(dr != DialogResult.OK) return;

					ioc = IOConnectionInfo.FromPath(ofdDb.FileName);
				}
				else
				{
					ioc = CompleteConnectionInfo(new IOConnectionInfo(), false,
						true, true, true);
					if(ioc == null) return;
				}
			}
			else // ioConnection != null
			{
				ioc = CompleteConnectionInfo(ioConnection, false, true, true, false);
				if(ioc == null) return;
			}

			if(!ioc.CanProbablyAccess())
			{
				MessageService.ShowWarning(ioc.GetDisplayName(), KPRes.FileNotFoundError);
				return;
			}

			if(OpenDatabaseRestoreIfOpened(ioc)) return;

			PwDatabase pwOpenedDb = null;
			bool bAbort;
			if(cmpKey == null)
			{
				for(int iTry = 0; iTry < Program.Config.Security.MasterKeyTries; ++iTry)
				{
					KeyPromptFormResult r;
					DialogResult dr = KeyPromptForm.ShowDialog(ioc, IsFileLocked(null),
						null, out r);

					if(r == null) { Debug.Assert(false); break; }
					if(r.HasClosedWithExit)
					{
						Debug.Assert(dr == DialogResult.Abort);
						OnFileExit(null, EventArgs.Empty);
						return;
					}
					if(dr != DialogResult.OK) break;

					pwOpenedDb = OpenDatabaseInternal(ioc, r.CompositeKey, out bAbort);
					if((pwOpenedDb != null) || bAbort) break;
				}
			}
			else // cmpKey != null
			{
				pwOpenedDb = OpenDatabaseInternal(ioc, cmpKey, out bAbort);
			}

			if((pwOpenedDb == null) || !pwOpenedDb.IsOpen)
			{
				UpdateUIState(false); // Reset status bar text
				return;
			}

			string strName = pwOpenedDb.IOConnectionInfo.GetDisplayName();
			m_mruList.AddItem(strName, pwOpenedDb.IOConnectionInfo.CloneDeep());

			PwDocument dsExisting = m_docMgr.FindDocument(pwOpenedDb);
			if(dsExisting != null) m_docMgr.ActiveDocument = dsExisting;

			bool bCorrectDbActive = (m_docMgr.ActiveDocument.Database == pwOpenedDb);
			Debug.Assert(bCorrectDbActive);

			AutoEnableVisualHiding(true);

			// SetLastUsedFile(pwOpenedDb.IOConnectionInfo);
			RememberKeySources(pwOpenedDb);

			PwGroup pgRestoreSelect = null;
			if(bCorrectDbActive)
			{
				m_docMgr.ActiveDocument.LockedIoc = new IOConnectionInfo(); // Clear

				pgRestoreSelect = pwOpenedDb.RootGroup.FindGroup(
					pwOpenedDb.LastSelectedGroup, true);
			}

			UpdateUI(true, null, true, pgRestoreSelect, true, null, false);
			if(bCorrectDbActive)
			{
				SetTopVisibleGroup(pwOpenedDb.LastTopVisibleGroup);
				if(pgRestoreSelect != null)
					SetTopVisibleEntry(pgRestoreSelect.LastTopVisibleEntry);
			}
			UpdateColumnSortingIcons();

			long lMkDays = (DateTime.UtcNow - pwOpenedDb.MasterKeyChanged).Days;

			bool bMkChangeForce = ((pwOpenedDb.MasterKeyChangeForce >= 0) &&
				(lMkDays >= pwOpenedDb.MasterKeyChangeForce));
			bMkChangeForce |= pwOpenedDb.MasterKeyChangeForceOnce;
			bMkChangeForce |= KeyUtil.HasKeyExpired(pwOpenedDb,
				Program.Config.Security.MasterKeyExpiryForce,
				"Configuration/Security/MasterKeyExpiryForce:");

			bool bMkChangeRec = ((pwOpenedDb.MasterKeyChangeRec >= 0) &&
				(lMkDays >= pwOpenedDb.MasterKeyChangeRec));
			bMkChangeRec |= KeyUtil.HasKeyExpired(pwOpenedDb,
				Program.Config.Security.MasterKeyExpiryRec,
				"Configuration/Security/MasterKeyExpiryRec:");

			if(bMkChangeForce)
			{
				// Updating the UI might trigger an auto-save
				pwOpenedDb.MasterKeyChangeForceOnce = false;

				while(true)
				{
					MessageService.ShowInfo(pwOpenedDb.IOConnectionInfo.GetDisplayName() +
						MessageService.NewParagraph + KPRes.MasterKeyChangeForce +
						MessageService.NewParagraph + KPRes.MasterKeyChangeInfo);
					if(ChangeMasterKey(pwOpenedDb)) break;
					if(!AppPolicy.Current.ChangeMasterKey) break; // Prevent endless loop
				}
			}
			else if(bMkChangeRec)
			{
				if(MessageService.AskYesNo(pwOpenedDb.IOConnectionInfo.GetDisplayName() +
					MessageService.NewParagraph + KPRes.MasterKeyChangeRec +
					MessageService.NewParagraph + KPRes.MasterKeyChangeQ))
					ChangeMasterKey(pwOpenedDb);
			}

			KdfParameters kdfParams = pwOpenedDb.KdfParameters;
			if(KeyUtil.KdfAdjustWeakParameters(ref kdfParams, pwOpenedDb.IOConnectionInfo))
			{
				pwOpenedDb.KdfParameters = kdfParams;
				UpdateUIState(true);
			}

			if(FixDuplicateUuids(pwOpenedDb, pwOpenedDb.IOConnectionInfo))
				UpdateUIState(false); // Already marked as modified

			if(this.FileOpened != null)
				this.FileOpened(this, new FileOpenedEventArgs(pwOpenedDb));
			Program.TriggerSystem.RaiseEvent(EcasEventIDs.OpenedDatabaseFile,
				EcasProperty.Database, pwOpenedDb);

			if(bCorrectDbActive && pwOpenedDb.IsOpen)
			{
				ShowExpiredEntries(true,
					Program.Config.Application.FileOpening.ShowExpiredEntries,
					(Program.Config.Application.FileOpening.ShowSoonToExpireEntries ?
					Program.Config.Application.ExpirySoonDays : 0), false, false);

				// Avoid view being destroyed by the unlocking routine
				pwOpenedDb.LastSelectedGroup = PwUuid.Zero;
			}

			if(Program.Config.MainWindow.MinimizeAfterOpeningDatabase)
				UIUtil.SetWindowState(this, FormWindowState.Minimized);

			ResetDefaultFocus(null);
		}

		private PwDatabase OpenDatabaseInternal(IOConnectionInfo ioc,
			CompositeKey cmpKey, out bool bAbort)
		{
			bAbort = false;

			PerformSelfTest();

			PwDocument ds = null;
			string strPathNrm = ioc.Path.Trim().ToLower();
			for(int iScan = 0; iScan < m_docMgr.Documents.Count; ++iScan)
			{
				if(m_docMgr.Documents[iScan].LockedIoc.Path.Trim().ToLower() == strPathNrm)
					ds = m_docMgr.Documents[iScan];
				else if(m_docMgr.Documents[iScan].Database.IOConnectionInfo.Path == strPathNrm)
					ds = m_docMgr.Documents[iScan];
			}

			PwDatabase pwDb;
			if(ds == null) pwDb = m_docMgr.CreateNewDocument(true).Database;
			else pwDb = ds.Database;

			UIBlockInteraction(true);
			ShowWarningsLogger swLogger = CreateShowWarningsLogger();
			swLogger.StartLogging(KPRes.OpeningDatabase2, true);
			m_sCancellable.Push(swLogger);

			Exception ex = null;
			try
			{
				pwDb.Open(ioc, cmpKey, swLogger);

#if DEBUG
				byte[] pbDiskDirect = WinUtil.HashFile(ioc);
				Debug.Assert(MemUtil.ArraysEqual(pbDiskDirect, pwDb.HashOfFileOnDisk));
#endif
			}
			catch(Exception exLoad)
			{
				ex = exLoad;
				pwDb = null;
			}

			m_sCancellable.Pop();
			swLogger.EndLogging();
			UIBlockInteraction(false);

			if(pwDb == null)
			{
				if(ds == null) m_docMgr.CloseDatabase(m_docMgr.ActiveDatabase);
			}

			if(ex != null)
			{
				string strMsg = MessageService.GetLoadWarningMessage(
					ioc.GetDisplayName(), ex,
					(Program.CommandLineArgs[AppDefs.CommandLineOptions.Debug] != null));

				bool bShowStd = true;
				if(!ioc.IsLocalFile())
				{
					VistaTaskDialog vtd = new VistaTaskDialog();
					vtd.CommandLinks = false;
					vtd.Content = strMsg;
					vtd.DefaultButtonID = (int)DialogResult.Cancel;
					// vtd.VerificationText = KPRes.CredSpecifyDifferent;
					vtd.WindowTitle = PwDefs.ShortProductName;

					vtd.SetIcon(VtdIcon.Warning);
					vtd.AddButton((int)DialogResult.Cancel, KPRes.Ok, null);
					vtd.AddButton((int)DialogResult.Retry,
						KPRes.CredSpecifyDifferent, null);

					if(vtd.ShowDialog(this))
					{
						bShowStd = false;

						// if(vtd.ResultVerificationChecked)
						if(vtd.Result == (int)DialogResult.Retry)
						{
							IOConnectionInfo iocNew = ioc.CloneDeep();
							// iocNew.ClearCredentials(false);
							iocNew.CredSaveMode = IOCredSaveMode.NoSave;
							iocNew.IsComplete = false;
							// iocNew.Password = string.Empty;

							OpenDatabase(iocNew, null, false);

							bAbort = true;
						}
					}
				}

				if(bShowStd) MessageService.ShowWarning(strMsg);
				// MessageService.ShowLoadWarning(ioc.GetDisplayName(), ex,
				//	(Program.CommandLineArgs[AppDefs.CommandLineOptions.Debug] != null));
			}

			return pwDb;
		}

		private bool OpenDatabaseRestoreIfOpened(IOConnectionInfo ioc)
		{
			if(ioc == null) { Debug.Assert(false); return false; }

			string strPathNrm = ioc.Path.Trim().ToLower();

			foreach(PwDocument ds in m_docMgr.Documents)
			{
				if(((ds.LockedIoc == null) || (ds.LockedIoc.Path.Length == 0)) &&
					(ds.Database.IOConnectionInfo.Path.Trim().ToLower() == strPathNrm))
				{
					MakeDocumentActive(ds);
					return true;
				}
			}

			return false;
		}

		private static void AutoEnableVisualHiding(bool bNewOpenDatabase)
		{
			// // KPF 1802197
			// // Turn on visual hiding if option is selected
			// if(m_docMgr.ActiveDatabase.MemoryProtection.AutoEnableVisualHiding)
			// {
			//	if(m_docMgr.ActiveDatabase.MemoryProtection.ProtectTitle && !m_viewHideFields.ProtectTitle)
			//		m_menuViewHideTitles.Checked = m_viewHideFields.ProtectTitle = true;
			//	if(m_docMgr.ActiveDatabase.MemoryProtection.ProtectUserName && !m_viewHideFields.ProtectUserName)
			//		m_menuViewHideUserNames.Checked = m_viewHideFields.ProtectUserName = true;
			//	if(m_docMgr.ActiveDatabase.MemoryProtection.ProtectPassword && !m_viewHideFields.ProtectPassword)
			//		m_menuViewHidePasswords.Checked = m_viewHideFields.ProtectPassword = true;
			//	if(m_docMgr.ActiveDatabase.MemoryProtection.ProtectUrl && !m_viewHideFields.ProtectUrl)
			//		m_menuViewHideURLs.Checked = m_viewHideFields.ProtectUrl = true;
			//	if(m_docMgr.ActiveDatabase.MemoryProtection.ProtectNotes && !m_viewHideFields.ProtectNotes)
			//		m_menuViewHideNotes.Checked = m_viewHideFields.ProtectNotes = true;
			// }

			if(bNewOpenDatabase && !Program.Config.UI.Hiding.RememberHidingPasswordsMain)
			{
				List<AceColumn> l = Program.Config.MainWindow.FindColumns(
					AceColumnType.Password);
				foreach(AceColumn c in l) c.HideWithAsterisks = true; // Reset
			}
		}

		private TreeNode GuiFindGroup(PwUuid puSearch, TreeNode tnContainer)
		{
			if(puSearch == null) { Debug.Assert(false); return null; }

			if(tnContainer == null)
			{
				if(m_tvGroups.Nodes.Count == 0) return null;
				tnContainer = m_tvGroups.Nodes[0];
			}

			PwGroup pg = (tnContainer.Tag as PwGroup);
			if(pg != null)
			{
				if(pg.Uuid.Equals(puSearch)) return tnContainer;
			}
			else { Debug.Assert(false); }

			foreach(TreeNode tn in tnContainer.Nodes)
			{
				if(tn != tnContainer)
				{
					TreeNode tnRet = GuiFindGroup(puSearch, tn);
					if(tnRet != null) return tnRet;
				}
				else { Debug.Assert(false); }
			}

			return null;
		}

		private ListViewItem GuiFindEntry(PwUuid puSearch)
		{
			Debug.Assert(puSearch != null);
			if(puSearch == null) return null;

			foreach(ListViewItem lvi in m_lvEntries.Items)
			{
				if(((PwListItem)lvi.Tag).Entry.Uuid.Equals(puSearch))
					return lvi;
			}

			return null;
		}

		private void PrintGroup(PwGroup pg)
		{
			if(pg == null) { Debug.Assert(false); return; }

			PwDatabase pd = m_docMgr.ActiveDatabase;
			if((pd == null) || !pd.IsOpen) return;

			if(!AppPolicy.TryWithKey(AppPolicyId.Print, AppPolicy.Current.PrintNoKey,
				pd, KPRes.Print))
				return;

			// Printing selected entries should not always reveal parent
			// group information (group notes, ...)
			// pg = ExportUtil.WithParentGroups(pg, pd);

			// Changing the root group of the database breaks Spr compilation
			// PwGroup pgOrgRoot = pd.RootGroup;
			// pd.RootGroup = pg;

			PrintForm pf = new PrintForm();
			pf.InitEx(pg, pd, m_ilCurrentIcons, true, m_pListSorter.Column);
			UIUtil.ShowDialogAndDestroy(pf);

			// pd.RootGroup = pgOrgRoot;
		}

		public void OnMruExecute(string strDisplayName, object oTag,
			ToolStripMenuItem tsmiParent)
		{
			if(tsmiParent == null) { Debug.Assert(false); return; }

			IOConnectionInfo ioc = (oTag as IOConnectionInfo);
			if(ioc == null) { Debug.Assert(false); return; }

			if(tsmiParent == m_menuFileRecent)
				OpenDatabase(ioc, null, false);
			else if(tsmiParent == m_menuFileSyncRecent)
			{
				PwDatabase pd = m_docMgr.ActiveDatabase;
				if((pd == null) || !pd.IsOpen) { Debug.Assert(false); return; }

				ioc = CompleteConnectionInfo(ioc, false, true, true, false);
				if(ioc == null) { Debug.Assert(false); return; }

				bool? ob = ImportUtil.Synchronize(pd, this, ioc, false, this);
				UpdateUISyncPost(ob);
			}
			else { Debug.Assert(false); }
		}

		public void OnMruClear()
		{
			m_mruList.Clear();
		}

		// For plugins
		public void UpdateTrayIcon()
		{
			UpdateTrayIcon(true);
		}

		private void UpdateTrayIcon(bool bRefreshIcon)
		{
			if(m_ntfTray == null) { Debug.Assert(false); return; } // Required

			bool bWindowVisible = this.Visible;
			bool bTrayVisible = m_ntfTray.Visible;

			if(Program.Config.UI.TrayIcon.ShowOnlyIfTrayedEx)
				m_ntfTray.Visible = !bWindowVisible;
			else if(bWindowVisible && !bTrayVisible)
				m_ntfTray.Visible = true;

			if(bRefreshIcon) m_ntfTray.RefreshShellIcon();
		}

		private void OnSessionLock(object sender, SessionLockEventArgs e)
		{
			// Shutdown is handled through OnFormClosing (the default
			// WndProc of the Form class tries to close the window when
			// receiving WM_QUERYENDSESSION or WM_ENDSESSION)

			if((e.Reason == SessionLockReason.RemoteControlChange) &&
				Program.Config.Security.WorkspaceLocking.LockOnRemoteControlChange) { }
			else if(((e.Reason == SessionLockReason.Lock) ||
				(e.Reason == SessionLockReason.Ending) ||
				(e.Reason == SessionLockReason.UserSwitch)) &&
				Program.Config.Security.WorkspaceLocking.LockOnSessionSwitch) { }
			else if((e.Reason == SessionLockReason.Suspend) &&
				Program.Config.Security.WorkspaceLocking.LockOnSuspend) { }
			else return;

			if(IsAtLeastOneFileOpen()) LockAllDocuments();
		}

		/// <summary>
		/// Reset the internal user inactivity timers.
		/// </summary>
		public void NotifyUserActivity()
		{
			// m_nLockTimerCur = m_nLockTimerMax;

			if(m_nLockTimerMax == 0) m_lLockAtTicks = long.MaxValue;
			else
			{
				DateTime utcLockAt = DateTime.UtcNow;
				utcLockAt = utcLockAt.AddSeconds((double)m_nLockTimerMax);
				m_lLockAtTicks = utcLockAt.Ticks;
			}

			Program.TriggerSystem.NotifyUserActivity();

			if(this.UserActivityPost != null)
				this.UserActivityPost(null, EventArgs.Empty);
		}

		private void UpdateGlobalLockTimeout(DateTime utcNow)
		{
			uint uLockGlobal = Program.Config.Security.WorkspaceLocking.LockAfterGlobalTime;
			if(uLockGlobal == 0) { m_lLockAtGlobalTicks = long.MaxValue; return; }

			uint? uLastInputTime = NativeMethods.GetLastInputTime();
			if(!uLastInputTime.HasValue) return;

			if(uLastInputTime.Value != m_uLastInputTime)
			{
				DateTime utcLockAt = utcNow.AddSeconds((double)uLockGlobal);
				m_lLockAtGlobalTicks = utcLockAt.Ticks;

				m_uLastInputTime = uLastInputTime.Value;
			}
		}

		/// <summary>
		/// Move selected entries.
		/// </summary>
		/// <param name="iMove">Must be -2/2 to move to top/bottom, -1/1 to
		/// move one up/down.</param>
		private void MoveSelectedEntries(int iMove)
		{
			if((m_pListSorter.Column >= 0) && (m_pListSorter.Order != SortOrder.None))
			{
				if(MessageService.AskYesNo(KPRes.AutoSortNoRearrange +
					MessageService.NewParagraph + KPRes.AutoSortDeactivateQ))
					SortPasswordList(false, 0, null, true);
				return;
			}

			PwEntry[] vEntries = GetSelectedEntries();
			if((vEntries == null) || (vEntries.Length == 0)) { Debug.Assert(false); return; }

			PwGroup pg = vEntries[0].ParentGroup;
			if(pg == null) { Debug.Assert(false); return; }
			foreach(PwEntry pe in vEntries)
			{
				if(pe.ParentGroup != pg)
				{
					MessageService.ShowWarning(KPRes.CannotMoveEntriesBcsGroup);
					return;
				}
			}

			if((iMove == -1) || (iMove == 1))
				pg.Entries.MoveOne(vEntries, (iMove < 0));
			else if((iMove == -2) || (iMove == 2))
				pg.Entries.MoveTopBottom(vEntries, (iMove < 0));
			else { Debug.Assert(false); return; }

			DateTime dtNow = DateTime.UtcNow;
			foreach(PwEntry peUpd in vEntries) peUpd.LocationChanged = dtNow;

			// Blocking prevents correct scrolling when groups are enabled
			// and the scroll position is near the end
			// m_lvEntries.BeginUpdateEx();

			bool bScrollOne = ((Math.Abs(iMove) == 1) &&
				!UIUtil.GetGroupsEnabled(m_lvEntries));

			int iTop = UIUtil.GetTopVisibleItem(m_lvEntries);
			ListView.SelectedIndexCollection lvsic = m_lvEntries.SelectedIndices;
			if(lvsic.Count > 0)
			{
				int pCrit = lvsic[(iMove < 0) ? 0 : (lvsic.Count - 1)];

				if(iMove < 0)
				{
					if(pCrit <= iTop) bScrollOne = false;
					else if((pCrit - iTop) <= 3) { } // Auto-scroll
					else bScrollOne = false;
				}
				else // iMove > 0
				{
					int nVisible = UIUtil.GetMaxVisibleItemCount(m_lvEntries);
					if(pCrit < (iTop + nVisible - 4)) bScrollOne = false;
				}
			}
			else { Debug.Assert(false); bScrollOne = false; }

			if(bScrollOne)
			{
				// if(UIUtil.GetGroupsEnabled(m_lvEntries))
				// {
				//	if(m_lvEntries.Items.Count > 0)
				//	{
				//		int dy = m_lvEntries.Items[0].Bounds.Height;
				//		if(dy > 1)
				//			NativeMethods.Scroll(m_lvEntries, 0,
				//				iMove * (dy + (dy / 4))); // With spacing added
				//	}
				// }

				// if(!UIUtil.GetGroupsEnabled(m_lvEntries))
				// {
				iTop += iMove;
				iTop = Math.Max(Math.Min(iTop, m_lvEntries.Items.Count - 1), 0);
				UIUtil.SetTopVisibleItem(m_lvEntries, iTop, false);
				// }
			}

			UpdateEntryList(null, false);
			EnsureVisibleSelected(iMove > 0); // In all cases

			// m_lvEntries.EndUpdateEx();
			UpdateUIState(true);
		}

		public StatusBarLogger CreateStatusBarLogger()
		{
			StatusBarLogger sl = new StatusBarLogger();
			sl.SetControls(m_statusPartInfo, m_statusPartProgress);
			return sl;
		}

		/// <summary>
		/// Create a new warnings logger object that logs directly into
		/// the main status bar until the first warning is shown (in that
		/// case a dialog is opened displaying the warning).
		/// </summary>
		/// <returns>Reference to the new logger object.</returns>
		public ShowWarningsLogger CreateShowWarningsLogger()
		{
			StatusBarLogger sl = CreateStatusBarLogger();
			return new ShowWarningsLogger(sl, this);
		}

		internal void HandleHotKey(int wParam)
		{
			if(HotKeyManager.HandleHotKeyIntoSelf(wParam)) return;

			if(wParam == AppDefs.GlobalHotKeyId.AutoType)
				ExecuteGlobalAutoType();
			else if(wParam == AppDefs.GlobalHotKeyId.AutoTypePassword)
				ExecuteGlobalAutoType(@"{PASSWORD}");
			else if(wParam == AppDefs.GlobalHotKeyId.AutoTypeSelected)
				ExecuteEntryAutoType();
			else if(wParam == AppDefs.GlobalHotKeyId.ShowWindow)
			{
				bool bWndVisible = ((this.WindowState != FormWindowState.Minimized) &&
					!IsTrayed());
				EnsureVisibleForegroundWindow(true, true);
				if(bWndVisible && IsFileLocked(null))
					OnFileLock(null, EventArgs.Empty); // Unlock
			}
			else if(wParam == AppDefs.GlobalHotKeyId.EntryMenu)
				EntryMenu.Show();
			else { Debug.Assert(false); }
		}

		protected override void WndProc(ref Message m)
		{
			if(m.Msg == NativeMethods.WM_HOTKEY)
			{
				NotifyUserActivity();
				HandleHotKey((int)m.WParam);
			}
			else if((m.Msg == m_nAppMessage) && (m_nAppMessage != 0))
				ProcessAppMessage(m.WParam, m.LParam);
			else if(m.Msg == NativeMethods.WM_SYSCOMMAND)
			{
				if((m.WParam == (IntPtr)NativeMethods.SC_MINIMIZE) ||
					(m.WParam == (IntPtr)NativeMethods.SC_MAXIMIZE))
				{
					SaveWindowPositionAndSize();
				}
			}
			else if(m.Msg == NativeMethods.WM_SETTINGCHANGE)
				AccessibilityEx.OnSystemSettingChange();
			else if((m.Msg == NativeMethods.WM_POWERBROADCAST) &&
				((m.WParam == (IntPtr)NativeMethods.PBT_APMQUERYSUSPEND) ||
				(m.WParam == (IntPtr)NativeMethods.PBT_APMSUSPEND)))
			{
				OnSessionLock(null, new SessionLockEventArgs(SessionLockReason.Suspend));
			}
			else if((m.Msg == m_nTaskbarButtonMessage) && m_bTaskbarButtonMessage)
			{
				m_bTaskbarButtonMessage = false;
				UpdateUIState(false, null); // Set overlay icon
				m_bTaskbarButtonMessage = true;
			}
			else if(m.Msg == DwmUtil.WM_DWMSENDICONICTHUMBNAIL)
			{
				DwmUtil.SetIconicThumbnail(this, AppIcons.Default, ref m);
				return;
			}
			else if(m.Msg == DwmUtil.WM_DWMSENDICONICLIVEPREVIEWBITMAP)
			{
				DwmUtil.SetIconicPreview(this, AppIcons.Default, ref m);
				return;
			}
			else if(m.Msg == NativeMethods.WM_WINDOWPOSCHANGING)
			{
				// Prevent showing the form prematurely;
				// https://sourceforge.net/p/keepass/discussion/329220/thread/3b696041f8/
				if(m_bFormLoadCalled && !m_bFormLoaded && (m.LParam != IntPtr.Zero))
				{
					NativeMethods.WINDOWPOS wp = (NativeMethods.WINDOWPOS)Marshal.PtrToStructure(
						m.LParam, typeof(NativeMethods.WINDOWPOS));
					if((wp.flags & NativeMethods.SWP_SHOWWINDOW) != 0)
					{
						wp.flags ^= NativeMethods.SWP_SHOWWINDOW;
						Marshal.StructureToPtr(wp, m.LParam, false);

						m_bHasBlockedShowWindow = true;
					}
				}
			}

			base.WndProc(ref m);
		}

		public void ProcessAppMessage(IntPtr wParam, IntPtr lParam)
		{
			NotifyUserActivity();

			switch(wParam.ToInt64())
			{
				case (long)Program.AppMessage.RestoreWindow:
					EnsureVisibleForegroundWindow(true, true);
					break;
				case (long)Program.AppMessage.Exit:
					OnFileExit(null, EventArgs.Empty);
					break;
				case (long)Program.AppMessage.IpcByFile:
					IpcUtilEx.ProcessGlobalMessage((int)lParam.ToInt64(), this, false);
					break;
				case (long)Program.AppMessage.AutoType:
					ExecuteGlobalAutoType();
					break;
				case (long)Program.AppMessage.Lock:
					LockAllDocuments();
					break;
				case (long)Program.AppMessage.Unlock:
					if(IsFileLocked(null))
					{
						EnsureVisibleForegroundWindow(false, false);
						OnFileLock(null, EventArgs.Empty); // Unlock
					}
					break;
				case (long)Program.AppMessage.AutoTypeSelected:
					ExecuteEntryAutoType();
					break;
				case (long)Program.AppMessage.Cancel:
					OnTrayCancel(null, EventArgs.Empty);
					break;
				case (long)Program.AppMessage.AutoTypePassword:
					ExecuteGlobalAutoType(@"{PASSWORD}");
					break;
				case (long)Program.AppMessage.IpcByFile1:
					IpcUtilEx.ProcessGlobalMessage((int)lParam.ToInt64(), this, true);
					break;
				default:
					Debug.Assert(false);
					break;
			}
		}

		public void ExecuteGlobalAutoType()
		{
			ExecuteGlobalAutoType(null);
		}

		private void ExecuteGlobalAutoType(string strSeq)
		{
			if(m_bIsAutoTyping) return;
			m_bIsAutoTyping = true;

			if(!IsAtLeastOneFileOpen())
			{
				try
				{
					IntPtr hPrevWnd = NativeMethods.GetForegroundWindowHandle();

					EnsureVisibleForegroundWindow(false, false);

					if(!IsCommandTypeInvokable(null, AppCommandType.Lock))
					{
						m_bIsAutoTyping = false;
						return;
					}

					// The window restoration function above maybe
					// restored the window already, therefore only
					// try to unlock if it's locked *now*
					if(IsFileLocked(null))
					{
						// https://sourceforge.net/p/keepass/bugs/1163/
						bool bFirst = true;
						EventHandler<GwmWindowEventArgs> eh = delegate(object sender,
							GwmWindowEventArgs e)
						{
							if(!bFirst) return;
							bFirst = false;
							GlobalWindowManager.ActivateTopWindow();
						};
						GlobalWindowManager.WindowAdded += eh;

						OnFileLock(null, EventArgs.Empty);

						GlobalWindowManager.WindowAdded -= eh;
					}

					NativeMethods.EnsureForegroundWindow(hPrevWnd);
				}
				catch(Exception exAT)
				{
					MessageService.ShowWarning(exAT);
				}
			}
			if(!IsAtLeastOneFileOpen()) { m_bIsAutoTyping = false; return; }

			try
			{
				AutoType.PerformGlobal(m_docMgr.GetOpenDatabases(),
					m_ilCurrentIcons, strSeq);
			}
			catch(Exception exGlobal)
			{
				MessageService.ShowWarning(exGlobal);
			}

			m_bIsAutoTyping = false;
		}

		private void ExecuteEntryAutoType()
		{
			try
			{
				IntPtr hFG = NativeMethods.GetForegroundWindowHandle();
				if(!AutoType.IsValidAutoTypeWindow(hFG, true)) return;
			}
			catch(Exception) { Debug.Assert(false); return; }

			PwEntry peSel = GetSelectedEntry(true);
			if(peSel != null)
				AutoType.PerformIntoCurrentWindow(peSel,
					m_docMgr.SafeFindContainerOf(peSel));
			else
			{
				EnsureVisibleForegroundWindow(true, true);
				MessageService.ShowWarning(KPRes.AutoTypeSelectedNoEntry,
					KPRes.AutoTypeGlobalHint);
			}
		}

		public void EnsureVisibleForegroundWindow(bool bUntray, bool bRestoreWindow)
		{
			if(GlobalWindowManager.ActivateTopWindow()) return;

			if(bUntray && IsTrayed()) MinimizeToTray(false);

			if(bRestoreWindow && (this.WindowState == FormWindowState.Minimized))
				RestoreWindowEx();

			UIUtil.EnsureInsideScreen(this);

			try
			{
				if(this.Visible) // && (this.WindowState != FormWindowState.Minimized)
				{
					this.BringToFront();
					this.Activate();
				}
			}
			catch(Exception) { Debug.Assert(false); }
		}

		private void SetListFont(AceFont af)
		{
			if((af != null) && af.OverrideUIDefault)
			{
				m_tvGroups.Font = af.ToFont();
				m_lvEntries.Font = af.ToFont();
				m_richEntryView.Font = af.ToFont();

				Program.Config.UI.StandardFont = af;
			}
			else
			{
				if(UIUtil.VistaStyleListsSupported)
				{
					Font f = UISystemFonts.ListFont;
					m_tvGroups.Font = f;
					m_lvEntries.Font = f;
					m_richEntryView.Font = f;
				}
				else
				{
					m_tvGroups.ResetFont();
					m_lvEntries.ResetFont();
					m_richEntryView.ResetFont();
				}

				Program.Config.UI.StandardFont.OverrideUIDefault = false;
			}

			// Font fUI = m_tabMain.Font;
			Font fList = m_lvEntries.Font;

			// m_fontBoldUI = FontUtil.CreateFont(fUI, fUI.Style | FontStyle.Bold);
			m_fontExpired = FontUtil.CreateFont(fList, fList.Style | FontStyle.Strikeout);
			m_fontBoldTree = FontUtil.CreateFont(fList, fList.Style | FontStyle.Bold);
			m_fontItalicTree = FontUtil.CreateFont(fList, fList.Style | FontStyle.Italic);
		}

		private void SetSelectedEntryColor(Color clrBack)
		{
			PwDatabase pd = m_docMgr.ActiveDatabase;
			if((pd == null) || !pd.IsOpen) { Debug.Assert(false); return; }

			PwEntry[] vSelected = GetSelectedEntries();
			if((vSelected == null) || (vSelected.Length == 0)) return;

			bool bMod = false;
			foreach(PwEntry pe in vSelected)
			{
				if(UIUtil.ColorsEqual(pe.BackgroundColor, clrBack)) continue;

				pe.CreateBackup(pd);
				pe.BackgroundColor = clrBack;
				pe.Touch(true, false);

				bMod = true;
			}

			SelectEntries(new PwObjectList<PwEntry>(), true, false, false,
				false); // Ensure color visible
			RefreshEntriesList();
			UpdateUIState(bMod);
		}

		private void OnEntryStringClick(object sender, DynamicMenuEventArgs e)
		{
			try
			{
				EntryDataCommand edc = (e.Tag as EntryDataCommand);
				if(edc == null) { Debug.Assert(false); return; }

				PwEntry pe = GetSelectedEntry(false);
				if(pe == null) return;

				PwDatabase pd = m_docMgr.ActiveDatabase;
				string strToCopy = null;

				switch(edc.Type)
				{
					case EntryDataCommandType.CopyField:
						strToCopy = pe.Strings.ReadSafe(edc.Param);
						break;

					case EntryDataCommandType.CopyValue:
						strToCopy = edc.Param;
						break;

					case EntryDataCommandType.ShowValue:
						SprContext ctx = new SprContext(pe, pd, SprCompileFlags.All);
						string str = SprEngine.Compile(edc.Param, ctx);

						if(!string.IsNullOrEmpty(edc.ErrorOnSprFailure) &&
							(string.IsNullOrEmpty(str) || (str == edc.Param)))
							MessageService.ShowWarning(edc.ErrorOnSprFailure);
						else
						{
							// if(!VistaTaskDialog.ShowMessageBox(null, str,
							//	PwDefs.ShortProductName, VtdIcon.Information, this))
							MessageService.ShowInfo(str);
						}

						RefreshEntriesList(); // Spr compilation
						UpdateUIState(false);
						break;

					default:
						Debug.Assert(false);
						break;
				}

				if(strToCopy != null)
				{
					if(ClipboardUtil.CopyAndMinimize(strToCopy, true, this, pe, pd))
						StartClipboardCountdown();
				}
			}
			catch(Exception ex) { MessageService.ShowWarning(ex.Message); }
		}

		private void SetMainWindowLayout(bool bSideBySide)
		{
			if(!bSideBySide && (m_splitHorizontal.Orientation != Orientation.Horizontal))
			{
				m_splitHorizontal.Orientation = Orientation.Horizontal;
				UpdateUIState(false);
			}
			else if(bSideBySide && (m_splitHorizontal.Orientation != Orientation.Vertical))
			{
				m_splitHorizontal.Orientation = Orientation.Vertical;
				UpdateUIState(false);
			}

			UIUtil.SetChecked(m_menuViewWindowsStacked, !bSideBySide);
			UIUtil.SetChecked(m_menuViewWindowsSideBySide, bSideBySide);
		}

		private void AssignMenuShortcuts()
		{
			bool bMoveMono = MonoWorkarounds.IsRequired(1245);
			Keys kMoveMod = (bMoveMono ? (Keys.Control | Keys.Shift) : Keys.Alt);

			UIUtil.AssignShortcut(m_menuFileNew, Keys.Control | Keys.N);
			UIUtil.AssignShortcut(m_menuFileOpenLocal, Keys.Control | Keys.O);
			UIUtil.AssignShortcut(m_menuFileOpenUrl, Keys.Control | Keys.Shift | Keys.O);
			UIUtil.AssignShortcut(m_menuFileClose, Keys.Control | Keys.W);
			UIUtil.AssignShortcut(m_menuFileSave, Keys.Control | Keys.S);
			UIUtil.AssignShortcut(m_menuFilePrintDatabase, Keys.Control | Keys.P);
			UIUtil.AssignShortcut(m_menuFileSyncFile, Keys.Control | Keys.R);
			UIUtil.AssignShortcut(m_menuFileSyncUrl, Keys.Control | Keys.Shift | Keys.R);
			UIUtil.AssignShortcut(m_menuFileLock, Keys.Control | Keys.L);
			UIUtil.AssignShortcut(m_menuFileExit, Keys.Control | Keys.Q);

			UIUtil.AssignShortcut(m_menuGroupDelete, Keys.Delete, null, true);

			UIUtil.AssignShortcut(m_menuGroupMoveToTop, (bMoveMono ?
				Keys.F5 : Keys.Home) | kMoveMod, null, true);
			UIUtil.AssignShortcut(m_menuGroupMoveOneUp, (bMoveMono ?
				Keys.F6 : Keys.Up) | kMoveMod, null, true);
			UIUtil.AssignShortcut(m_menuGroupMoveOneDown, (bMoveMono ?
				Keys.F7 : Keys.Down) | kMoveMod, null, true);
			UIUtil.AssignShortcut(m_menuGroupMoveToBottom, (bMoveMono ?
				Keys.F8 : Keys.End) | kMoveMod, null, true);
			// UIUtil.AssignShortcut(m_menuGroupSort, Keys.Control | Keys.Decimal);
			// UIUtil.AssignShortcut(m_menuGroupSortRec, Keys.Control | Keys.Shift | Keys.Decimal); // (Control+)Shift+Decimal = Delete
			UIUtil.AssignShortcut(m_menuGroupExpand, Keys.Control | Keys.Multiply);
			UIUtil.AssignShortcut(m_menuGroupCollapse, Keys.Control | Keys.Divide);

			UIUtil.AssignShortcut(m_menuGroupPrint, Keys.Control | Keys.Shift | Keys.P);

			UIUtil.AssignShortcut(m_menuEntryCopyUserName, Keys.Control | Keys.B);
			UIUtil.AssignShortcut(m_menuEntryCopyPassword, Keys.Control | Keys.C,
				null, true);
			UIUtil.AssignShortcut(m_menuEntryOpenUrl, Keys.Control | Keys.U);
			UIUtil.AssignShortcut(m_menuEntryCopyUrl, Keys.Control | Keys.Shift | Keys.U);
			UIUtil.AssignShortcut(m_menuEntryPerformAutoType, Keys.Control | Keys.V,
				null, true);

			UIUtil.AssignShortcut(m_menuEntryAdd, Keys.Control | Keys.I);
			UIUtil.AssignShortcut(m_menuEntryEdit, Keys.Return, null, true);
			UIUtil.AssignShortcut(m_menuEntryDuplicate, Keys.Control | Keys.K);
			UIUtil.AssignShortcut(m_menuEntryDelete, Keys.Delete, null, true);

			UIUtil.AssignShortcut(m_menuEntrySelectAll, Keys.Control | Keys.A,
				null, true);

			UIUtil.AssignShortcut(m_menuEntryMoveToTop, (bMoveMono ?
				Keys.F5 : Keys.Home) | kMoveMod, null, true);
			UIUtil.AssignShortcut(m_menuEntryMoveOneUp, (bMoveMono ?
				Keys.F6 : Keys.Up) | kMoveMod, null, true);
			UIUtil.AssignShortcut(m_menuEntryMoveOneDown, (bMoveMono ?
				Keys.F7 : Keys.Down) | kMoveMod, null, true);
			UIUtil.AssignShortcut(m_menuEntryMoveToBottom, (bMoveMono ?
				Keys.F8 : Keys.End) | kMoveMod, null, true);

			UIUtil.AssignShortcut(m_menuEntryCompare2, Keys.Control | Keys.D, null, true);
			UIUtil.AssignShortcut(m_menuEntryCompareMark, Keys.Control | Keys.Shift | Keys.D,
				null, true);
			UIUtil.AssignShortcut(m_menuEntryCompare1, Keys.Control | Keys.D, null, true);

			UIUtil.AssignShortcut(m_menuEntryClipCopy, Keys.Control | Keys.Shift | Keys.C,
				null, true);
			UIUtil.AssignShortcut(m_menuEntryClipPaste, Keys.Control | Keys.Shift | Keys.V,
				null, true);

			UIUtil.AssignShortcut(m_menuFindInDatabase, Keys.Control | Keys.F);
			UIUtil.AssignShortcut(m_menuFindInGroup, Keys.Control | Keys.Shift | Keys.F,
				m_ctxGroupFind, false);

			UIUtil.AssignShortcut(m_menuFindParentGroup, Keys.Control | Keys.G);

			// UIUtil.AssignShortcut(m_menuViewHidePasswords, Keys.Control | Keys.H);
			// UIUtil.AssignShortcut(m_menuViewHideUserNames, Keys.Control | Keys.J);

			UIUtil.AssignShortcut(m_menuHelpContents, Keys.F1);
		}

		private void ConstructContextMenus()
		{
			SuspendLayoutScope sls = new SuspendLayoutScope(true, m_ctxGroupList,
				m_ctxPwList);

			ToolStripItemCollection tsicGC = m_ctxGroupList.Items;
			tsicGC.Insert(0, new ToolStripSeparator());
			m_milMain.CreateCopy(tsicGC, null, false, m_menuGroupRearrange);
			tsicGC.Insert(0, new ToolStripSeparator());
			m_milMain.CreateCopy(tsicGC, null, false, m_menuGroupEmptyRB);
			m_milMain.CreateCopy(tsicGC, null, false, m_menuGroupDelete);
			m_milMain.CreateCopy(tsicGC, null, false, m_menuGroupDuplicate);
			m_milMain.CreateCopy(tsicGC, null, false, m_menuGroupEdit);
			m_milMain.CreateCopy(tsicGC, null, false, m_menuGroupAdd);
			// tsicGC.Add(new ToolStripSeparator());
			// m_milMain.CreateCopy(tsicGC, null, true, m_menuGroupPrint);
			// m_milMain.CreateCopy(tsicGC, null, true, m_menuGroupExport);

			ToolStripItemCollection tsicEC = m_ctxPwList.Items;
			m_milMain.CreateCopy(tsicEC, null, false, m_menuEntryCopyPassword);
			m_milMain.CreateCopy(tsicEC, null, false, m_menuEntryCopyUserName);
			m_milMain.CreateLink(m_ctxEntryUrl, m_menuEntryUrl, false);

			ToolStripItemCollection tsicECUrl = m_ctxEntryUrl.DropDownItems;
			m_milMain.CreateCopy(tsicECUrl, null, true, m_menuEntryOpenUrl);
			m_milMain.CreateCopy(tsicECUrl, null, true, m_menuEntryCopyUrl);

			m_milMain.CreateLink(m_ctxEntryOtherData, m_menuEntryOtherData, false);
			m_milMain.CreateLink(m_ctxEntryAttachments, m_menuEntryAttachments, false);
			// m_milMain.CreateCopy(tsicEC, m_ctxEntryAttachments, true, m_menuEntrySaveAttachedFiles);

			m_milMain.CreateCopy(tsicEC, m_ctxEntryAutoTypeAdv, false, m_menuEntryPerformAutoType);
			m_milMain.CreateLink(m_ctxEntryAutoTypeAdv, m_menuEntryAutoTypeAdv, false);

			m_milMain.CreateCopy(tsicEC, m_ctxEntryEditQuick, false, m_menuEntryAdd);
			m_milMain.CreateCopy(tsicEC, m_ctxEntryEditQuick, false, m_menuEntryEdit);
			m_milMain.CreateLink(m_ctxEntryEditQuick, m_menuEntryEditQuick, false);

			ToolStripItemCollection tsicECQuick = m_ctxEntryEditQuick.DropDownItems;
			tsicECQuick.Insert(0, new ToolStripSeparator());
			m_milMain.CreateCopy(tsicECQuick, null, false, m_menuEntryColor);
			m_milMain.CreateCopy(tsicECQuick, null, false, m_menuEntryIcon);

			m_milMain.CreateLink(m_ctxEntryTagAdd, m_menuEntryTagAdd, false);
			m_milMain.CreateLink(m_ctxEntryTagRemove, m_menuEntryTagRemove, false);

			ToolStripItemCollection tsicECTagAdd = m_ctxEntryTagAdd.DropDownItems;
			m_milMain.CreateCopy(tsicECTagAdd, null, false, m_menuEntryTagNew);

			tsicECQuick.Add(new ToolStripSeparator());
			m_milMain.CreateCopy(tsicECQuick, null, true, m_menuEntryExpiresNow);
			m_milMain.CreateCopy(tsicECQuick, null, true, m_menuEntryExpiresNever);
			tsicECQuick.Add(new ToolStripSeparator());
			m_milMain.CreateCopy(tsicECQuick, null, true, m_menuEntryOtpGenSettings);

			m_milMain.CreateCopy(tsicEC, m_ctxEntryEditQuick, true, m_menuEntryDelete);
			m_milMain.CreateCopy(tsicEC, m_ctxEntryEditQuick, true, m_menuEntryDuplicate);

			ToolStripSeparator ts = new ToolStripSeparator();
			tsicEC.Insert(tsicEC.IndexOf(m_ctxEntryRearrange), ts);
			m_milMain.CreateCopy(tsicEC, ts, false, m_menuEntrySelectAll);

			m_milMain.CreateLink(m_ctxEntryRearrange, m_menuEntryRearrange, false);
			m_milMain.CreateLink(m_ctxEntryMoveToGroup, m_menuEntryMoveToGroup, false);

			ToolStripItemCollection tsicECMove = m_ctxEntryRearrange.DropDownItems;
			tsicECMove.Insert(0, new ToolStripSeparator());
			m_milMain.CreateCopy(tsicECMove, null, false, m_menuEntryMoveToBottom);
			m_milMain.CreateCopy(tsicECMove, null, false, m_menuEntryMoveOneDown);
			m_milMain.CreateCopy(tsicECMove, null, false, m_menuEntryMoveOneUp);
			m_milMain.CreateCopy(tsicECMove, null, false, m_menuEntryMoveToTop);
			m_milMain.CreateCopy(tsicECMove, null, true, m_menuEntryMoveToPreviousParent);

			sls.Dispose();
		}

		private static void CopyMenuItemText(ToolStripMenuItem tsmiTarget,
			ToolStripMenuItem tsmiCopyFrom, string strTextOpt)
		{
			tsmiTarget.Text = (strTextOpt ?? tsmiCopyFrom.Text);

			string strSh = tsmiCopyFrom.ShortcutKeyDisplayString;
			if(!string.IsNullOrEmpty(strSh))
				tsmiTarget.ShortcutKeyDisplayString = strSh;
		}

		// Public for plugins
		public void SaveDatabaseAs(PwDatabase pdToSave, IOConnectionInfo iocTo,
			bool bOnline, object sender, bool bCopy)
		{
			PwDatabase pd = (pdToSave ?? m_docMgr.ActiveDatabase);

			if(!pd.IsOpen) return;
			if(!AppPolicy.Try(AppPolicyId.SaveFile)) return;

			Guid eventGuid = Guid.NewGuid();
			if(this.FileSavingPre != null)
			{
				FileSavingEventArgs args = new FileSavingEventArgs(true, bCopy, pd, eventGuid);
				this.FileSavingPre(sender, args);
				if(args.Cancel) return;
			}
			if(this.FileSaving != null)
			{
				FileSavingEventArgs args = new FileSavingEventArgs(true, bCopy, pd, eventGuid);
				this.FileSaving(sender, args);
				if(args.Cancel) return;
			}

			DialogResult dr;
			IOConnectionInfo ioc = iocTo;

			if((ioc != null) && (ioc.Path.Length > 0))
			{
				dr = DialogResult.OK; // Caller (plugin) specified target file
			}
			else if(bOnline)
			{
				IOConnectionForm iocf = new IOConnectionForm();
				iocf.InitEx(true, pd.IOConnectionInfo, true, true);

				dr = iocf.ShowDialog();
				ioc = iocf.IOConnectionInfo;
				UIUtil.DestroyForm(iocf);
			}
			else
			{
				SaveFileDialogEx sfdDb = UIUtil.CreateSaveFileDialog(KPRes.SaveDatabase,
					UrlUtil.GetFileName(pd.IOConnectionInfo.Path),
					UIUtil.CreateFileTypeFilter(AppDefs.FileExtension.FileExt,
					KPRes.KdbxFiles, true), 1, AppDefs.FileExtension.FileExt,
					AppDefs.FileDialogContext.Database);

				GlobalWindowManager.AddDialog(sfdDb.FileDialog);
				dr = sfdDb.ShowDialog();
				GlobalWindowManager.RemoveDialog(sfdDb.FileDialog);

				if(dr == DialogResult.OK)
					ioc = IOConnectionInfo.FromPath(sfdDb.FileName);
			}

			if(dr == DialogResult.OK)
			{
				EcasPropertyDictionary dProps = new EcasPropertyDictionary();
				dProps.Set(EcasProperty.IOConnectionInfo, ioc);
				dProps.Set(EcasProperty.Database, pd);
				Program.TriggerSystem.RaiseEvent(EcasEventIDs.SavingDatabaseFile,
					dProps);

				UIBlockInteraction(true);

				ShutdownBlocker sdb = new ShutdownBlocker(this.Handle, KPRes.SavingDatabase);
				ShowWarningsLogger swLogger = CreateShowWarningsLogger();
				swLogger.StartLogging(KPRes.SavingDatabase, true);
				m_sCancellable.Push(swLogger);

				bool bSuccess = true;
				try
				{
					PreSavingEx(pd, ioc);
					pd.SaveAs(ioc, !bCopy, swLogger);
					PostSavingEx(!bCopy, pd, ioc, swLogger);
				}
				catch(Exception exSaveAs)
				{
					MessageService.ShowSaveWarning(ioc, exSaveAs, true);
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
					this.FileSaved(sender, new FileSavedEventArgs(bSuccess, pd, eventGuid));
				if(bSuccess)
					Program.TriggerSystem.RaiseEvent(EcasEventIDs.SavedDatabaseFile,
						dProps);
			}
		}

		private void PreSavingEx(PwDatabase pd, IOConnectionInfo ioc)
		{
			PerformSelfTest();

			FixDuplicateUuids(pd, ioc);

			pd.UseFileTransactions = Program.Config.Application.UseTransactedFileWrites;
			pd.UseFileLocks = Program.Config.Application.UseFileLocks;

			// AceColumn col = Program.Config.MainWindow.FindColumn(AceColumnType.Title);
			// if((col != null) && !col.HideWithAsterisks)
			//	pd.MemoryProtection.ProtectTitle = false;
			// col = Program.Config.MainWindow.FindColumn(AceColumnType.UserName);
			// if((col != null) && !col.HideWithAsterisks)
			//	pd.MemoryProtection.ProtectUserName = false;
			// col = Program.Config.MainWindow.FindColumn(AceColumnType.Url);
			// if((col != null) && !col.HideWithAsterisks)
			//	pd.MemoryProtection.ProtectUrl = false;
			// col = Program.Config.MainWindow.FindColumn(AceColumnType.Notes);
			// if((col != null) && !col.HideWithAsterisks)
			//	pd.MemoryProtection.ProtectNotes = false;

			if(pd == m_docMgr.ActiveDatabase) SaveWindowState();
		}

		private void PostSavingEx(bool bPrimary, PwDatabase pwDatabase,
			IOConnectionInfo ioc, IStatusLogger sl)
		{
			if(ioc == null) { Debug.Assert(false); return; }

			byte[] pbIO = null;
			if(Program.Config.Application.VerifyWrittenFileAfterSaving)
			{
				pbIO = WinUtil.HashFile(ioc);
				Debug.Assert((pbIO != null) && (pwDatabase.HashOfLastIO != null));
				if(!MemUtil.ArraysEqual(pbIO, pwDatabase.HashOfLastIO))
					MessageService.ShowWarning(ioc.GetDisplayName(),
						KPRes.FileVerifyHashFail, KPRes.FileVerifyHashFailRec);
			}

			if(bPrimary)
			{
#if DEBUG
				Debug.Assert(MemUtil.ArraysEqual(pbIO, pwDatabase.HashOfFileOnDisk));

				try
				{
					PwDatabase pwCheck = new PwDatabase();
					pwCheck.Open(ioc.CloneDeep(), pwDatabase.MasterKey, null);

					Debug.Assert(MemUtil.ArraysEqual(pwDatabase.HashOfLastIO,
						pwCheck.HashOfLastIO));

					uint uGroups1, uGroups2, uEntries1, uEntries2;
					pwDatabase.RootGroup.GetCounts(true, out uGroups1, out uEntries1);
					pwCheck.RootGroup.GetCounts(true, out uGroups2, out uEntries2);
					Debug.Assert((uGroups1 == uGroups2) && (uEntries1 == uEntries2));
				}
				catch(Exception exVerify) { Debug.Assert(false, exVerify.Message); }
#endif

				m_mruList.AddItem(ioc.GetDisplayName(), ioc.CloneDeep());

				// SetLastUsedFile(ioc);

				// if(Program.Config.Application.CreateBackupFileAfterSaving && bHashValid)
				// {
				//	try { pwDatabase.CreateBackupFile(sl); }
				//	catch(Exception exBackup)
				//	{
				//		MessageService.ShowWarning(KPRes.FileBackupFailed, exBackup);
				//	}
				// }

				// ulong uTotalBinSize = 0;
				// EntryHandler ehCnt = delegate(PwEntry pe)
				// {
				//	foreach(KeyValuePair<string, ProtectedBinary> kvpCnt in pe.Binaries)
				//	{
				//		uTotalBinSize += kvpCnt.Value.Length;
				//	}
				//
				//	return true;
				// };
				// pwDatabase.RootGroup.TraverseTree(TraversalMethod.PreOrder, null, ehCnt);
			}

			RememberKeySources(pwDatabase);
			WinUtil.FlushStorageBuffers(ioc.Path, true);
		}

		public bool UIFileSave(bool bForceSave)
		{
			Control cFocus = UIUtil.GetActiveControl(this);

			PwDatabase pd = m_docMgr.ActiveDatabase;
			pd.Modified = true;

			if(bForceSave) ++m_uForceSave;
			OnFileSave(null, EventArgs.Empty);
			if(bForceSave) --m_uForceSave;

			if(cFocus != null) ResetDefaultFocus(cFocus);
			return !pd.Modified;
		}

		public void ResetDefaultFocus(Control cExplicit)
		{
			Control c = cExplicit;

			if(c == null)
			{
				// QuickFind must be the first choice (see e.g.
				// the option FocusQuickFindOnUntray)
				if(m_tbQuickFind.Visible && m_tbQuickFind.Enabled)
					c = m_tbQuickFind.Control;
				else if(m_lvEntries.Visible && m_lvEntries.Enabled)
					c = m_lvEntries;
				else if(m_tvGroups.Visible && m_tvGroups.Enabled)
					c = m_tvGroups;
				else if(m_richEntryView.Visible && m_richEntryView.Enabled)
					c = m_richEntryView;
				else c = m_lvEntries;
			}

			if(this.FocusChanging != null)
			{
				FocusEventArgs ea = new FocusEventArgs(cExplicit, c);
				this.FocusChanging(null, ea);
				if(ea.Cancel) return;
			}

			UIUtil.SetFocus(c, this);
		}

		private static bool PrepareLock()
		{
			if(GlobalWindowManager.WindowCount == 0) return true;

			if(GlobalWindowManager.CanCloseAllWindows)
			{
				GlobalWindowManager.CloseAllWindows();
				return true;
			}

			return false;
		}

		private void UpdateImageLists(bool bForce)
		{
			if(!bForce)
			{
				if(!m_docMgr.ActiveDatabase.UINeedsIconUpdate) return;
				m_docMgr.ActiveDatabase.UINeedsIconUpdate = false;
			}

			int cx = DpiUtil.ScaleIntX(16);
			int cy = DpiUtil.ScaleIntY(16);

			// // foreach(Image img in m_ilClientIcons.Images) imgList.Images.Add(img);
			// List<Image> lStdImages = new List<Image>();
			// foreach(Image imgStd in m_ilClientIcons.Images)
			// {
			//	// Plugins may supply DPI-scaled images by changing m_ilClientIcons
			//	bool bStd = (imgStd.Height == 16);
			//	lStdImages.Add(bStd ? DpiUtil.ScaleImage(imgStd, false) : imgStd);
			// }

			if(m_lStdClientImages == null)
			{
				ImageArchive arStd = new ImageArchive();
				arStd.Load(DpiUtil.ScalingRequired ?
					Properties.Resources.Images_Client_HighRes :
					Properties.Resources.Images_Client_16);

				m_lStdClientImages = arStd.GetImages(cx, cy, true);
			}

			// ImageList imgListCustom = UIUtil.BuildImageList(
			//	m_docMgr.ActiveDatabase.CustomIcons, cx, cy);
			// foreach(Image imgCustom in imgListCustom.Images)
			//	imgList.Images.Add(imgCustom); // Breaks alpha partially
			List<Image> lCustom = UIUtil.BuildImageListEx(
				m_docMgr.ActiveDatabase.CustomIcons, cx, cy);

			List<Image> lAll = new List<Image>(m_lStdClientImages);
			lAll.AddRange(lCustom);

			ImageList imgList = new ImageList();
			imgList.ImageSize = new Size(cx, cy);
			imgList.ColorDepth = ColorDepth.Depth32Bit;

			imgList.Images.AddRange(lAll.ToArray());
			Debug.Assert(imgList.Images.Count == ((int)PwIcon.Count + lCustom.Count));

			m_ilCurrentIcons = imgList;

			if(UIUtil.VistaStyleListsSupported)
			{
				m_tvGroups.ImageList = imgList;
				m_lvEntries.SmallImageList = imgList;
			}
			else
			{
				ImageList imgSafe = UIUtil.ConvertImageList24(lAll,
					cx, cy, AppDefs.ColorControlNormal);
				m_tvGroups.ImageList = imgSafe; // TreeView doesn't fully support alpha on < Vista
				m_lvEntries.SmallImageList = ((WinUtil.IsAtLeastWindowsVista ||
					WinUtil.IsWindowsXP) ? imgList : imgSafe);
			}
		}

		private void MoveSelectedGroup(int iMove)
		{
			PwGroup pgMove = GetSelectedGroup();
			if(pgMove == null) { Debug.Assert(false); return; }

			PwGroup pgParent = pgMove.ParentGroup;
			if(pgParent == null) return;

			PwGroup[] pgAffected = new PwGroup[] { pgMove };

			if(iMove == -2)
				pgParent.Groups.MoveTopBottom(pgAffected, true);
			else if(iMove == -1)
				pgParent.Groups.MoveOne(pgMove, true);
			else if(iMove == 1)
				pgParent.Groups.MoveOne(pgMove, false);
			else if(iMove == 2)
				pgParent.Groups.MoveTopBottom(pgAffected, false);
			else { Debug.Assert(false); return; }

			pgMove.LocationChanged = DateTime.UtcNow;
			UpdateUI(false, null, true, null, true, null, true);
		}

		private void SaveAllAttachments()
		{
			PwEntry[] v = GetSelectedEntries();
			if((v == null) || (v.Length == 0)) return;

			using(FolderBrowserDialog fbd = UIUtil.CreateFolderBrowserDialog(
				KPRes.AttachmentsSave))
			{
				GlobalWindowManager.AddDialog(fbd);
				if(fbd.ShowDialog() == DialogResult.OK)
					EntryUtil.SaveEntryAttachments(v, fbd.SelectedPath);
				GlobalWindowManager.RemoveDialog(fbd);
			}
		}

		private void OnEntryBinaryClick(object sender, DynamicMenuEventArgs e)
		{
			if(e == null) { Debug.Assert(false); return; }

			EntryBinaryDataContext ctx = (e.Tag as EntryBinaryDataContext);
			if(ctx == null) { Debug.Assert(false); return; }

			if(ctx == EntryBinaryDataContext.SaveAll)
			{
				SaveAllAttachments();
				return;
			}

			PwEntry pe = ctx.Entry;
			if(pe == null) { Debug.Assert(false); return; }
			Debug.Assert(object.ReferenceEquals(pe, GetSelectedEntry(true)));

			if(string.IsNullOrEmpty(ctx.Name)) { Debug.Assert(false); return; }
			ProtectedBinary pb = pe.Binaries.Get(ctx.Name);
			if(pb == null) { Debug.Assert(false); return; }

			ProtectedBinary pbMod = BinaryDataUtil.Open(ctx.Name, pb, ctx.Options);
			if(pbMod != null)
			{
				PwDatabase pd = m_docMgr.FindContainerOf(pe);
				Debug.Assert(object.ReferenceEquals(pd, this.ActiveDatabase));
				pe.CreateBackup(pd);

				pe.Binaries.Set(ctx.Name, pbMod);
				pe.Touch(true, false);

				RefreshEntriesList();
				UpdateUIState(true);
			}
		}

		private void SaveWindowState()
		{
			PwDatabase pd = m_docMgr.ActiveDatabase;

			PwGroup pgSel = GetSelectedGroup();
			if(pgSel != null) pd.LastSelectedGroup = pgSel.Uuid;

			TreeNode tnTop = m_tvGroups.TopNode;
			if(tnTop != null)
			{
				PwGroup pgTop = (tnTop.Tag as PwGroup);
				if(pgTop != null) pd.LastTopVisibleGroup = pgTop.Uuid;
				else { Debug.Assert(false); }
			}

			PwEntry peTop = GetTopEntry();
			if((peTop != null) && (pgSel != null))
				pgSel.LastTopVisibleEntry = peTop.Uuid;
		}

		private void RestoreWindowState(PwDatabase pd)
		{
			PwGroup pgSelect = null;

			if(!pd.LastSelectedGroup.Equals(PwUuid.Zero))
			{
				pgSelect = pd.RootGroup.FindGroup(pd.LastSelectedGroup, true);
				UpdateGroupList(pgSelect);
				UpdateEntryList(pgSelect, false);
			}

			SetTopVisibleGroup(pd.LastTopVisibleGroup);
			if(pgSelect != null) SetTopVisibleEntry(pgSelect.LastTopVisibleEntry);
		}

		private void SetTopVisibleGroup(PwUuid uuidGroup)
		{
			TreeNode tnTop = GuiFindGroup(uuidGroup, null);
			if(tnTop != null) m_tvGroups.TopNode = tnTop;
		}

		private void SetTopVisibleEntry(PwUuid uuidEntry)
		{
			ListViewItem lviTop = GuiFindEntry(uuidEntry);
			if(lviTop != null)
				UIUtil.SetTopVisibleItem(m_lvEntries, lviTop.Index, false);
		}

		internal void CloseDocument(PwDocument dsToClose, bool bLocking,
			bool bExiting, bool bEcas, bool bUpdateUI)
		{
			PwDocument ds = (dsToClose ?? m_docMgr.ActiveDocument);
			PwDatabase pd = ds.Database;
			bool bIsActive = (ds == m_docMgr.ActiveDocument);

			FileEventFlags f = FileEventFlags.None;
			if(bLocking) f |= FileEventFlags.Locking;
			if(bExiting) f |= FileEventFlags.Exiting;
			if(bEcas) f |= FileEventFlags.Ecas;

			Program.TriggerSystem.RaiseEvent(EcasEventIDs.ClosingDatabaseFilePre,
				EcasProperty.Database, pd);
			if(this.FileClosingPre != null)
			{
				FileClosingEventArgs fcea = new FileClosingEventArgs(pd, f);
				this.FileClosingPre(null, fcea);
				if(fcea.Cancel) return;
			}

			if(pd.Modified) // Implies pd.IsOpen
			{
				bool bInvokeSave = false;

				// https://sourceforge.net/p/keepass/discussion/329220/thread/c3c823c6/
				bool bCanAutoSave = AppPolicy.Current.SaveFile;

				if(Program.Config.Application.FileClosing.AutoSave && bCanAutoSave)
					bInvokeSave = true;
				else
				{
					FileSaveOrigin fso = FileSaveOrigin.Closing;
					if(bLocking) fso = FileSaveOrigin.Locking;
					if(bExiting) fso = FileSaveOrigin.Exiting;

					DialogResult dr = FileDialogsEx.ShowFileSaveQuestion(
						pd.IOConnectionInfo.GetDisplayName(), fso);

					if(dr == DialogResult.Cancel) return;
					else if(dr == DialogResult.Yes) bInvokeSave = true;
					else if(dr == DialogResult.No) { } // Changes are lost
				}

				if(bInvokeSave)
				{
					SaveDatabase(pd, null);
					if(pd.Modified) return;
				}
			}

			Program.TriggerSystem.RaiseEvent(EcasEventIDs.ClosingDatabaseFilePost,
				EcasProperty.Database, pd);
			if(this.FileClosingPost != null)
			{
				FileClosingEventArgs fcea = new FileClosingEventArgs(pd, f);
				this.FileClosingPost(null, fcea);
				if(fcea.Cancel) return;
			}

			IOConnectionInfo ioClosing = pd.IOConnectionInfo.CloneDeep();

			pd.Close();
			if(!bLocking) m_docMgr.CloseDatabase(pd);

			if(bIsActive)
			{
				if((Program.Config.UI.UIFlags & (ulong)AceUIFlags.NoQuickSearchClear) == 0)
				{
					++m_uBlockQuickFind;
					m_tbQuickFind.Items.Clear();
					m_tbQuickFind.Text = string.Empty;
					--m_uBlockQuickFind;
				}

				if(!bLocking)
				{
					m_docMgr.ActiveDatabase.UINeedsIconUpdate = true;
					ResetDefaultFocus(null);
				}
			}
			if(bUpdateUI)
				UpdateUI(true, null, true, null, true, null, false);

			// NativeMethods.ClearIconicBitmaps(this.Handle);
			Program.TempFilesPool.Clear(TempClearFlags.ContentTaggedFiles);

			if(this.FileClosed != null)
				this.FileClosed(null, new FileClosedEventArgs(ioClosing, f));
		}

		// Public for plugins
		public void LockAllDocuments()
		{
			NotifyUserActivity();

			if(Program.Config.Security.WorkspaceLocking.AlwaysExitInsteadOfLocking)
			{
				OnFileExit(null, EventArgs.Empty);
				return;
			}

			if(UIIsInteractionBlocked()) { Debug.Assert(false); return; }
			if(!PrepareLock()) return; // Tries to close windows

			SaveWindowState();

			List<PwDocument> lDocs = m_docMgr.GetDocuments(int.MaxValue);
			foreach(PwDocument ds in lDocs)
			{
				PwDatabase pd = ds.Database;
				if(!pd.IsOpen) continue; // Nothing to lock

				IOConnectionInfo ioIoc = pd.IOConnectionInfo;
				Debug.Assert(ioIoc != null);

				CloseDocument(ds, true, false, false, false);
				if(pd.IsOpen) continue;

				ds.LockedIoc = ioIoc;
			}

			UpdateUI(true, null, true, null, true, null, false);

			if(Program.Config.MainWindow.MinimizeAfterLocking &&
				!IsAtLeastOneFileOpen())
				UIUtil.SetWindowState(this, FormWindowState.Minimized);
		}

		private void SaveAllDocuments()
		{
			List<PwDocument> lDocs = m_docMgr.GetDocuments(int.MaxValue);
			foreach(PwDocument ds in lDocs)
			{
				PwDatabase pd = ds.Database;
				if(!IsFileLocked(ds) && pd.Modified)
					SaveDatabase(pd, null);
			}

			UpdateUI(false, null, true, null, true, null, false);
		}

		// Does not update the UI (for performance when exiting)
		private bool CloseAllDocuments(bool bExiting)
		{
			if(UIIsInteractionBlocked()) { Debug.Assert(false); return false; }

			while(true)
			{
				List<PwDocument> lDocs = m_docMgr.GetDocuments(int.MaxValue);
				if(lDocs.Count <= 0) { Debug.Assert(false); break; }

				PwDocument ds = lDocs[0];
				if((lDocs.Count == 1) && !ds.Database.IsOpen) break;

				CloseDocument(ds, false, bExiting, false, false);
				if(ds.Database.IsOpen) return false;
			}

			return true;
		}

		private void RecreateUITabs()
		{
			++m_uTabChangeBlocked;
			m_tabMain.SuspendLayout();

			m_tabMain.TabPages.Clear();

			m_tabMain.ImageList = null;
			if(m_ilTabImages != null)
			{
				m_ilTabImages.Dispose();
				m_ilTabImages = null;
			}
			foreach(Image img in m_lTabImages) img.Dispose();
			m_lTabImages.Clear();

			bool bImgs = !MonoWorkarounds.IsRequired(891029);

			List<TabPage> lPages = new List<TabPage>();
			for(int i = 0; i < m_docMgr.Documents.Count; ++i)
			{
				PwDocument ds = m_docMgr.Documents[i];
				if(ds == null) { Debug.Assert(false); continue; }

				TabPage tb = new TabPage();
				tb.Tag = ds;

				string strName, strTip;
				GetTabText(ds, out strName, out strTip);
				tb.Text = strName;
				tb.ToolTipText = strTip;

				if(bImgs && (ds.Database != null) && ds.Database.IsOpen &&
					!UIUtil.ColorsEqual(ds.Database.Color, Color.Empty))
				{
					Image img = UIUtil.CreateTabColorImage(AppIcons.RoundColor(
						ds.Database.Color), m_tabMain);
					if(img != null)
					{
						m_lTabImages.Add(img);
						tb.ImageIndex = m_lTabImages.Count - 1;
					}
					else { Debug.Assert(false); }
				}

				lPages.Add(tb);
			}

			if(m_lTabImages.Count > 0)
			{
				int qSize = m_lTabImages[0].Height;
				m_ilTabImages = UIUtil.BuildImageListUnscaled(m_lTabImages, qSize, qSize);
				m_tabMain.ImageList = m_ilTabImages;
			}

			m_tabMain.TabPages.AddRange(lPages.ToArray());

			// m_tabMain.SelectedTab.Font = m_fontBoldUI;

			m_tabMain.ResumeLayout();
			--m_uTabChangeBlocked;
		}

		private void SelectUITab()
		{
			++m_uTabChangeBlocked;

			PwDocument dsSelect = m_docMgr.ActiveDocument;
			foreach(TabPage tb in m_tabMain.TabPages)
			{
				if((PwDocument)tb.Tag == dsSelect)
				{
					m_tabMain.SelectedTab = tb;
					break;
				}
			}

			--m_uTabChangeBlocked;
		}

		public void MakeDocumentActive(PwDocument ds)
		{
			if(ds == null) { Debug.Assert(false); return; }

			ds.Database.UINeedsIconUpdate = true;

			UpdateUI(false, ds, true, null, true, null, false);

			RestoreWindowState(ds.Database);
			UpdateUIState(false);
		}

		private void GetTabText(PwDocument ds, out string strName, out string strTip)
		{
			if(ds == null) { Debug.Assert(false); strName = string.Empty; strTip = string.Empty; return; }

			bool bLocked = IsFileLocked(ds);
			PwDatabase pd = (bLocked ? null : ds.Database);
			if((pd == null) || !pd.IsOpen) bLocked = true;
			string strPath = (bLocked ? ds.LockedIoc.Path : pd.IOConnectionInfo.Path);

			strName = strPath;
			// if(!Program.Config.MainWindow.ShowFullPathOnTab)
			strName = UrlUtil.GetFileName(strName);

			strTip = strPath;

			if(bLocked) strName += " [" + KPRes.Locked + "]";
			else
			{
				// if(Program.Config.MainWindow.ShowDatabaseNameOnTab && (pd.Name.Length != 0))
				//	strName += " (" + pd.Name + ")";

				if(pd.Modified) strName += "*";

				if(pd.Name.Length != 0)
					strTip += "\r\n\r\n" + pd.Name;
				if(pd.Description.Length != 0)
					strTip += "\r\n\r\n" + pd.Description;
			}

			strTip = StrUtil.EncodeToolTipText(strTip);
		}

		private void UpdateUITabs()
		{
			foreach(TabPage tp in m_tabMain.TabPages)
			{
				PwDocument ds = (tp.Tag as PwDocument);
				if(ds == null) { Debug.Assert(false); continue; }

				string strName, strTip;
				GetTabText(ds, out strName, out strTip);
				tp.Text = strName;
				tp.ToolTipText = strTip;
			}
		}

		public void UpdateUI(bool bRecreateTabBar, PwDocument dsSelect,
			bool bUpdateGroupList, PwGroup pgSelect, bool bUpdateEntryList,
			PwGroup pgEntrySource, bool bSetModified)
		{
			UpdateUI(bRecreateTabBar, dsSelect, bUpdateGroupList, pgSelect,
				bUpdateEntryList, pgEntrySource, bSetModified, null);
		}

		public void UpdateUI(bool bRecreateTabBar, PwDocument dsSelect,
			bool bUpdateGroupList, PwGroup pgSelect, bool bUpdateEntryList,
			PwGroup pgEntrySource, bool bSetModified, Control cOptFocus)
		{
			if(bRecreateTabBar) RecreateUITabs();

			if(dsSelect != null) m_docMgr.ActiveDocument = dsSelect;
			SelectUITab();

			UpdateImageLists(false);

			if(bUpdateGroupList) UpdateGroupList(pgSelect);

			if(bUpdateEntryList) UpdateEntryList(pgEntrySource, false);
			else { Debug.Assert(pgEntrySource == null); }

			UpdateUIState(bSetModified, cOptFocus);
		}

		private void ShowSearchResults(PwGroup pgResults, SearchParameters sp,
			PwGroup pgRoot, bool bFocusEntryList)
		{
			if(pgResults == null) { Debug.Assert(false); return; }

			UpdateEntryList(pgResults, false);
			SelectFirstEntryIfNoneSelected();

			if(bFocusEntryList) ResetDefaultFocus(m_lvEntries);

			UpdateUIState(false);

			PwGroup pgSkipped = null;
			if((sp != null) && sp.RespectEntrySearchingDisabled)
				pgSkipped = pgRoot;
			ShowSearchResultsStatusMessage(pgSkipped);
		}

		private void ShowSearchResultsStatusMessage(PwGroup pgSearchSkippedRoot)
		{
			const string strParam = "{PARAM}";

			StringBuilder sb = new StringBuilder();

			int n = m_lvEntries.Items.Count;
			if(n == 1) sb.Append(KPRes.SearchEntriesFound1);
			else
			{
				string strFound = KPRes.SearchEntriesFound;
				string str = StrUtil.ReplaceCaseInsensitive(strFound,
					strParam, n.ToString());
				if(str == strFound) { Debug.Assert(false); return; }
				sb.Append(str);
			}

			if(pgSearchSkippedRoot != null)
			{
				List<PwGroup> lSkipped = pgSearchSkippedRoot.GetTopSearchSkippedGroups();

				if(lSkipped.Count > 0)
				{
					sb.Append(", ");

					if(lSkipped.Count == 1) sb.Append(KPRes.GroupsSkipped1);
					else
					{
						string strSkipped = KPRes.GroupsSkipped;
						string str = StrUtil.ReplaceCaseInsensitive(strSkipped,
							strParam, lSkipped.Count.ToString());
						if(str == strSkipped) { Debug.Assert(false); return; }
						sb.Append(str);
					}

					sb.Append(" (");

					int m = Math.Min(lSkipped.Count, 3);
					for(int i = 0; i < m; ++i)
					{
						if(i > 0) sb.Append(", ");
						sb.Append('\'');
						sb.Append(StrUtil.EncodeMenuText(lSkipped[i].Name.Trim()));
						sb.Append('\'');
					}
					if(m < lSkipped.Count) sb.Append(", ...");

					sb.Append(')');
				}
			}

			sb.Append('.');
			SetStatusEx(sb.ToString());
		}

		private void MinimizeToTray(bool bMinimize)
		{
			if(bMinimize) // Order of Visible and ShowInTaskbar is important
			{
				// if(MonoWorkarounds.IsRequired(649266)) this.ShowInTaskbar = false;
				this.Visible = false;
			}
			else
			{
				this.Visible = true;
				// if(MonoWorkarounds.IsRequired(649266)) this.ShowInTaskbar = true;
			}

			if(bMinimize)
			{
				if(Program.Config.Security.WorkspaceLocking.LockOnWindowMinimizeToTray &&
					!IsFileLocked(null))
					OnFileLock(null, EventArgs.Empty);
			}
			else // Restore
			{
				// EnsureVisibleForegroundWindow(false, false); // Don't!

				if(this.WindowState == FormWindowState.Minimized)
					RestoreWindowEx();
				else if(IsFileLocked(null) && !UIIsAutoUnlockBlocked())
					OnFileLock(null, EventArgs.Empty); // Unlock

				if(Program.Config.MainWindow.FocusQuickFindOnUntray)
					ResetDefaultFocus(null);
			}

			UpdateTrayIcon(false);
		}

		private bool GetStartMinimized()
		{
			return (Program.Config.Application.Start.MinimizedAndLocked ||
				(Program.CommandLineArgs[AppDefs.CommandLineOptions.Minimize] != null));
		}

		private void MinimizeToTrayAtStartIfEnabled(bool bFormLoading)
		{
			if(GetStartMinimized())
			{
				if(bFormLoading)
					UIUtil.SetWindowState(this, FormWindowState.Minimized);
				else
				{
					// The following isn't required anymore, because the
					// TaskbarButtonCreated message is handled

					// Set the lock overlay icon again (the first time
					// Windows ignores the call, maybe because the window
					// wasn't fully constructed at that time yet)
					// if(IsFileLocked(null))
					//	TaskbarList.SetOverlayIcon(this,
					//		Properties.Resources.LockOverlay, KPRes.Locked);
				}

				if(Program.Config.MainWindow.MinimizeToTray) MinimizeToTray(true);
				else if(!bFormLoading)
					UIUtil.SetWindowState(this, FormWindowState.Minimized);
			}

			// Remove taskbar item
			if(Program.Config.MainWindow.MinimizeAfterOpeningDatabase &&
				Program.Config.MainWindow.MinimizeToTray && !bFormLoading &&
				IsAtLeastOneFileOpen())
			{
				MinimizeToTray(true);
			}
		}

		private void SelectFirstEntryIfNoneSelected()
		{
			if((m_lvEntries.Items.Count > 0) &&
				(m_lvEntries.SelectedIndices.Count == 0))
				UIUtil.SetFocusedItem(m_lvEntries, m_lvEntries.Items[0], true);
		}

		public void SelectEntries(PwObjectList<PwEntry> lEntries,
			bool bDeselectOthers, bool bFocusFirst)
		{
			SelectEntries(lEntries, bDeselectOthers, bFocusFirst, false, false);
		}

		private void SelectEntries(PwObjectList<PwEntry> lEntries,
			bool bDeselectOthers, bool bFocusFirst, bool bEnsureVisible,
			bool bUpdateUIState)
		{
			if(lEntries == null) { Debug.Assert(false); lEntries = new PwObjectList<PwEntry>(); }

			Dictionary<PwEntry, bool> d = new Dictionary<PwEntry, bool>();
			foreach(PwEntry pe in lEntries) d[pe] = true;

			bool bDoFocus = bFocusFirst;

			++m_uBlockEntrySelectionEvent;
			try
			{
				foreach(ListViewItem lvi in m_lvEntries.Items)
				{
					if(lvi == null) { Debug.Assert(false); continue; }

					PwListItem pli = (lvi.Tag as PwListItem);
					if(pli == null) { Debug.Assert(false); continue; }

					PwEntry pe = pli.Entry;
					if(pe == null) { Debug.Assert(false); continue; }

					if(d.ContainsKey(pe))
					{
						lvi.Selected = true;

						if(bDoFocus)
						{
							UIUtil.SetFocusedItem(m_lvEntries, lvi, false);
							bDoFocus = false;
						}
					}
					else if(bDeselectOthers) lvi.Selected = false;
				}
			}
			catch(Exception) { Debug.Assert(false); }
			--m_uBlockEntrySelectionEvent;

			if(bEnsureVisible) EnsureVisibleSelected(null);
			if(bUpdateUIState) UpdateUIState(false);
		}

		private void SelectEntry(PwEntry pe, bool bDeselectOthers, bool bFocus,
			bool bEnsureVisible, bool bUpdateUIState)
		{
			PwObjectList<PwEntry> l = new PwObjectList<PwEntry>();
			if(pe != null) l.Add(pe);
			else { Debug.Assert(false); }

			SelectEntries(l, bDeselectOthers, bFocus, bEnsureVisible, bUpdateUIState);
		}

		internal PwGroup GetCurrentEntries()
		{
			PwGroup pg = new PwGroup(true, true);
			pg.IsVirtual = true;

			if(!m_lvEntries.ShowGroups)
			{
				foreach(ListViewItem lvi in m_lvEntries.Items)
					pg.AddEntry(((PwListItem)lvi.Tag).Entry, false);
			}
			else // Groups
			{
				foreach(ListViewGroup lvg in m_lvEntries.Groups)
				{
					foreach(ListViewItem lvi in lvg.Items)
						pg.AddEntry(((PwListItem)lvi.Tag).Entry, false);
				}
			}

			return pg;
		}

		// Public for plugins
		public void EnsureVisibleEntry(PwUuid uuid)
		{
			ListViewItem lvi = GuiFindEntry(uuid);
			if(lvi == null) { Debug.Assert(false); return; }

			m_lvEntries.EnsureVisible(lvi.Index);
		}

		internal void EnsureVisibleSelected(bool? obLast)
		{
			// PwEntry pe = GetSelectedEntry(true, bLast);
			// if(pe == null) return;
			// EnsureVisibleEntry(pe.Uuid);

			ListView.SelectedIndexCollection lvsic = m_lvEntries.SelectedIndices;
			if(lvsic == null) { Debug.Assert(false); return; }
			int n = lvsic.Count;
			if(n == 0) return;

			// Getting lvsic[n - 1] sends n messages
			if(obLast.HasValue)
				m_lvEntries.EnsureVisible(obLast.Value ? lvsic[n - 1] : lvsic[0]);
			else
			{
				int iFirst = lvsic[0];
				if(n != 1) m_lvEntries.EnsureVisible(lvsic[n - 1]);
				m_lvEntries.EnsureVisible(iFirst);
			}
		}

		private void RemoveEntriesFromList(PwEntry[] vEntries)
		{
			if(vEntries == null) { Debug.Assert(false); return; }
			if(vEntries.Length == 0) return;

			if(MonoWorkarounds.IsRequired(1690) && (m_lvEntries.Items.Count != 0))
				UIUtil.SetFocusedItem(m_lvEntries, m_lvEntries.Items[0], false);

			++m_uBlockEntrySelectionEvent;
			m_lvEntries.BeginUpdateEx();

			lock(m_asyncListUpdate.ListEditSyncObject)
			{
				for(int i = m_lvEntries.Items.Count - 1; i >= 0; --i)
				{
					PwEntry pe = ((PwListItem)m_lvEntries.Items[i].Tag).Entry;
					Debug.Assert(pe != null);

					if(Array.IndexOf<PwEntry>(vEntries, pe) >= 0)
						m_lvEntries.Items.RemoveAt(i);
				}
			}

			m_lvEntries.EndUpdateEx();
			--m_uBlockEntrySelectionEvent;
		}

		internal void UpdateUISyncPost(bool? obResult)
		{
			if(!obResult.HasValue) return;

			UpdateUI(false, null, true, null, true, null, false);
			SetStatusEx(obResult.Value ? KPRes.SyncSuccess : KPRes.SyncFailed);
		}

		private bool PreSaveValidate(PwDatabase pd)
		{
			if(m_uForceSave > 0) return true;

			byte[] pbOnDisk = WinUtil.HashFile(pd.IOConnectionInfo);

			if((pbOnDisk != null) && (pd.HashOfFileOnDisk != null) &&
				!MemUtil.ArraysEqual(pbOnDisk, pd.HashOfFileOnDisk))
			{
				DialogResult dr = DialogResult.Yes;
				if(!Program.Config.Application.SaveForceSync)
					dr = AskIfSynchronizeInstead(pd.IOConnectionInfo);

				if(dr == DialogResult.Yes) // Synchronize
				{
					bool? ob = ImportUtil.Synchronize(pd, this, pd.IOConnectionInfo,
						true, this);
					UpdateUISyncPost(ob);
					return false;
				}
				else if(dr == DialogResult.Cancel) return false;
				else { Debug.Assert(dr == DialogResult.No); }
			}

			return true;
		}

		private static DialogResult AskIfSynchronizeInstead(IOConnectionInfo ioc)
		{
			VistaTaskDialog dlg = new VistaTaskDialog();

			string strText = string.Empty;
			if(ioc.GetDisplayName().Length > 0)
				strText += ioc.GetDisplayName() + MessageService.NewParagraph;
			strText += KPRes.FileChanged;

			dlg.CommandLinks = true;
			dlg.WindowTitle = PwDefs.ShortProductName;
			dlg.Content = strText;
			dlg.SetIcon(VtdCustomIcon.Question);

			dlg.MainInstruction = KPRes.OverwriteExistingFileQuestion;
			dlg.AddButton((int)DialogResult.Yes, KPRes.Synchronize, KPRes.FileChangedSync);
			dlg.AddButton((int)DialogResult.No, KPRes.Overwrite, KPRes.FileChangedOverwrite);
			dlg.AddButton((int)DialogResult.Cancel, KPRes.Cancel, KPRes.FileSaveQOpCancel);

			DialogResult dr;
			if(dlg.ShowDialog()) dr = (DialogResult)dlg.Result;
			else
			{
				strText += MessageService.NewParagraph;
				strText += @"[" + KPRes.Yes + @"]: " + KPRes.Synchronize + @". " +
					KPRes.FileChangedSync + MessageService.NewParagraph;
				strText += @"[" + KPRes.No + @"]: " + KPRes.Overwrite + @". " +
					KPRes.FileChangedOverwrite + MessageService.NewParagraph;
				strText += @"[" + KPRes.Cancel + @"]: " + KPRes.FileSaveQOpCancel;

				dr = MessageService.Ask(strText, PwDefs.ShortProductName,
					MessageBoxButtons.YesNoCancel);
			}

			return dr;
		}

		private void ActivateNextDocumentEx(int iDir)
		{
			int n = m_tabMain.TabPages.Count;
			if(n > 1)
				m_tabMain.SelectedIndex = ((m_tabMain.SelectedIndex +
					iDir + n) % n);
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			bool bDown;
			if(!NativeMethods.GetKeyMessageState(ref msg, out bDown))
				return base.ProcessCmdKey(ref msg, keyData);

			if(keyData == Keys.Escape) // No modifiers
			{
				// Control c = UIUtil.GetActiveControl(this); // Returns wrong control
				// bool bMainMenu = (c == m_menuMain);
				bool bMainMenu = ((msg.HWnd == m_menuMain.Handle) &&
					(UIUtil.GetSelectedItem(m_menuMain.Items) != null));

				if(!bMainMenu)
				{
					if(bDown && IsCommandTypeInvokable(null, AppCommandType.Window))
					{
						AceEscAction a = Program.Config.MainWindow.EscAction;

						if(a == AceEscAction.Lock)
							LockAllDocuments();
						else if(a == AceEscAction.Minimize)
							UIUtil.SetWindowState(this, FormWindowState.Minimized);
						else if(a == AceEscAction.MinimizeToTray)
						{
							if(!IsTrayed()) MinimizeToTray(true);
						}
						else if(a == AceEscAction.Exit)
							OnFileExit(null, EventArgs.Empty);
						else { Debug.Assert(a == AceEscAction.None); }
					}

					return true;
				}
			}

			KeyEventArgs e = new KeyEventArgs(keyData);
			if(HandleMainWindowKeyMessage(e, bDown)) return true;

			return base.ProcessCmdKey(ref msg, keyData);
		}

		private bool HandleMainWindowKeyMessage(KeyEventArgs e, bool bDown)
		{
			if(e == null) { Debug.Assert(false); return false; }

			Keys kc = e.KeyCode;
			bool bCtrl = e.Control, bAlt = e.Alt, bShift = e.Shift;
			bool bHandled = true;

			// Enforce that Alt is up (e.g. on Polish systems AltGr+E,
			// i.e. Ctrl+Alt+E, is used to enter a character)
			if(bCtrl && !bAlt)
			{
				if(kc == Keys.Tab)
				{
					if(bDown) ActivateNextDocumentEx(bShift ? -1 : 1);
				}
				// When changing Ctrl+E, also change the tooltip of the quick search box
				else if(kc == Keys.E) // Standard key for quick searches
					ResetDefaultFocus(m_tbQuickFind.Control); // Down and up (RichTextBox)
				else if(kc == Keys.H)
				{
					if(bDown) ToggleFieldAsterisks(AceColumnType.Password);
				}
				else if(kc == Keys.J)
				{
					if(bDown) ToggleFieldAsterisks(AceColumnType.UserName);
				}
				else bHandled = false;
			}
			else if(!bCtrl && !bAlt && (kc == Keys.F3))
			{
				if(bDown) OnFindInDatabase(null, EventArgs.Empty);
			}
			else bHandled = false;

			if(bHandled) UIUtil.SetHandled(e, true);
			return bHandled;
		}

		private bool HandleMoveKeyMessage(KeyEventArgs e, bool bDown, bool bEntry)
		{
			// Mono does not raise key events while Alt is down
			bool bMoveMod = e.Alt;
			if(MonoWorkarounds.IsRequired(1245)) bMoveMod = (e.Control && e.Shift);
			if(!bMoveMod) return false;

			// Mono raises key events *after* changing the selection,
			// thus we cannot use any navigation keys
			Keys[] vMove = new Keys[] { Keys.Home, Keys.Up, Keys.Down, Keys.End };
			if(MonoWorkarounds.IsRequired(1245))
				vMove = new Keys[] { Keys.F5, Keys.F6, Keys.F7, Keys.F8 };
			int m = Array.IndexOf<Keys>(vMove, e.KeyCode);
			if(m < 0) return false;

			if(bDown)
			{
				EventHandler[] vHandlers;
				if(bEntry)
					vHandlers = new EventHandler[] { OnEntryMoveToTop,
						OnEntryMoveOneUp, OnEntryMoveOneDown, OnEntryMoveToBottom };
				else
					vHandlers = new EventHandler[] { OnGroupMoveToTop,
						OnGroupMoveOneUp, OnGroupMoveOneDown, OnGroupMoveToBottom };

				vHandlers[m].Invoke(null, e);
			}

			UIUtil.SetHandled(e, true);
			return true;
		}

		public bool UIIsInteractionBlocked()
		{
			return (m_uUIBlocked > 0);
		}

		public void UIBlockInteraction(bool bBlock)
		{
			NotifyUserActivity();

			if(bBlock) ++m_uUIBlocked;
			else if(m_uUIBlocked > 0) --m_uUIBlocked;
			else { Debug.Assert(false); }

			bool bNotBlocked = !UIIsInteractionBlocked();
			this.Enabled = bNotBlocked;

			if(bNotBlocked)
			{
				try { if(!IsPrimaryControlActive()) ResetDefaultFocus(null); }
				catch(Exception) { Debug.Assert(false); }
			}
			else // Blocked
			{
				// Redraw the UI, but only when the UI is blocked (otherwise
				// the Application.DoEvents call could indirectly run
				// triggers/automations now, which could result in problems)
				UIUtil.DoEventsByTime(true);
			}
		}

		public bool UIIsAutoUnlockBlocked()
		{
			return (m_uUnlockAutoBlocked > 0);
		}

		public void UIBlockAutoUnlock(bool bBlock)
		{
			if(bBlock) ++m_uUnlockAutoBlocked;
			else if(m_uUnlockAutoBlocked > 0) --m_uUnlockAutoBlocked;
			else { Debug.Assert(false); }
		}

		public bool UIIsWindowStateAutoBlocked()
		{
			return (m_uWindowStateAutoBlocked > 0);
		}

		public void UIBlockWindowStateAuto(bool bBlock)
		{
			if(bBlock) ++m_uWindowStateAutoBlocked;
			else if(m_uWindowStateAutoBlocked > 0) --m_uWindowStateAutoBlocked;
			else { Debug.Assert(false); }
		}

		private static void EnsureRecycleBin(ref PwGroup pgRecycleBin,
			PwDatabase pdContext, ref bool bGroupListUpdateRequired)
		{
			if(pdContext == null) { Debug.Assert(false); return; }

			if(pgRecycleBin == pdContext.RootGroup)
			{
				Debug.Assert(false);
				pgRecycleBin = null;
			}

			if(pgRecycleBin == null)
			{
				pgRecycleBin = new PwGroup(true, true, KPRes.RecycleBin,
					PwIcon.TrashBin);
				pgRecycleBin.EnableAutoType = false;
				pgRecycleBin.EnableSearching = false;
				pgRecycleBin.IsExpanded = !Program.Config.Defaults.RecycleBinCollapse;
				pdContext.RootGroup.AddGroup(pgRecycleBin, true);

				pdContext.RecycleBinUuid = pgRecycleBin.Uuid;

				bGroupListUpdateRequired = true;
			}
			else { Debug.Assert(pgRecycleBin.Uuid.Equals(pdContext.RecycleBinUuid)); }
		}

		private void DeleteSelectedEntries()
		{
			PwEntry[] vSelected = GetSelectedEntries();
			if((vSelected == null) || (vSelected.Length == 0)) return;

			PwDatabase pd = m_docMgr.ActiveDatabase;
			PwGroup pgRecycleBin = pd.RootGroup.FindGroup(pd.RecycleBinUuid, true);
			bool bShiftPressed = ((Control.ModifierKeys & Keys.Shift) != Keys.None);

			bool bAtLeastOnePermanent = false;
			if(!pd.RecycleBinEnabled) bAtLeastOnePermanent = true;
			else if(bShiftPressed) bAtLeastOnePermanent = true;
			else if(pgRecycleBin == null) { } // Not permanent
			else
			{
				foreach(PwEntry peEnum in vSelected)
				{
					if((peEnum.ParentGroup == pgRecycleBin) ||
						peEnum.ParentGroup.IsContainedIn(pgRecycleBin))
					{
						bAtLeastOnePermanent = true;
						break;
					}
				}
			}

			bool bSingle = (vSelected.Length == 1);
			string strSummary = EntryUtil.CreateSummaryList(null, vSelected);

			if(bAtLeastOnePermanent)
			{
				VistaTaskDialog dlg = new VistaTaskDialog();
				dlg.CommandLinks = false;
				dlg.Content = strSummary;
				dlg.MainInstruction = (bSingle ? KPRes.DeleteEntriesQuestionSingle :
					KPRes.DeleteEntriesQuestion);
				dlg.SetIcon(VtdCustomIcon.Question);
				dlg.WindowTitle = PwDefs.ShortProductName;
				dlg.AddButton((int)DialogResult.OK, KPRes.DeleteCmd, null);
				dlg.AddButton((int)DialogResult.Cancel, KPRes.Cancel, null);

				if(dlg.ShowDialog())
				{
					if(dlg.Result == (int)DialogResult.Cancel) return;
				}
				else
				{
					string strText = (bSingle ? KPRes.DeleteEntriesQuestionSingle :
						KPRes.DeleteEntriesQuestion);
					if(strSummary.Length > 0)
						strText += MessageService.NewParagraph + strSummary;

					if(!MessageService.AskYesNo(strText, (bSingle ?
						KPRes.DeleteEntriesTitleSingle : KPRes.DeleteEntriesTitle)))
						return;
				}
			}
			else if(Program.Config.UI.ShowRecycleConfirmDialog)
			{
				VistaTaskDialog dlg = new VistaTaskDialog();
				dlg.CommandLinks = false;
				dlg.Content = strSummary;
				dlg.MainInstruction = (bSingle ? KPRes.RecycleEntryConfirmSingle :
					KPRes.RecycleEntryConfirm);
				dlg.SetIcon(VtdCustomIcon.Question);
				dlg.VerificationText = UIUtil.GetDialogNoShowAgainText(KPRes.Yes);
				dlg.WindowTitle = PwDefs.ShortProductName;
				dlg.AddButton((int)DialogResult.OK, KPRes.YesCmd, null);
				dlg.AddButton((int)DialogResult.Cancel, KPRes.NoCmd, null);

				if(dlg.ShowDialog())
				{
					if(dlg.Result == (int)DialogResult.Cancel) return;

					if(dlg.ResultVerificationChecked)
						Program.Config.UI.ShowRecycleConfirmDialog = false;
				}
				else
				{
					string strText = (bSingle ? KPRes.RecycleEntryConfirmSingle :
						KPRes.RecycleEntryConfirm);
					if(strSummary.Length > 0)
						strText += MessageService.NewParagraph + strSummary;

					if(!MessageService.AskYesNo(strText, (bSingle ?
						KPRes.DeleteEntriesTitleSingle : KPRes.DeleteEntriesTitle)))
						return;
				}
			}

			bool bUpdateGroupList = false;
			DateTime dtNow = DateTime.UtcNow;
			foreach(PwEntry pe in vSelected)
			{
				PwGroup pgParent = pe.ParentGroup;
				if(pgParent == null) continue; // Can't remove

				if(!pgParent.Entries.Remove(pe)) { Debug.Assert(false); continue; }

				bool bPermanent = false;
				if(!pd.RecycleBinEnabled) bPermanent = true;
				else if(bShiftPressed) bPermanent = true;
				else if(pgRecycleBin == null) { } // Recycle
				else if(pgParent == pgRecycleBin) bPermanent = true;
				else if(pgParent.IsContainedIn(pgRecycleBin)) bPermanent = true;

				if(bPermanent)
				{
					PwDeletedObject pdo = new PwDeletedObject(pe.Uuid, dtNow);
					pd.DeletedObjects.Add(pdo);
				}
				else // Recycle
				{
					EnsureRecycleBin(ref pgRecycleBin, pd, ref bUpdateGroupList);

					pgRecycleBin.AddEntry(pe, true, true);
					pe.PreviousParentGroup = pgParent.Uuid;
					pe.Touch(false);
				}
			}

			RemoveEntriesFromList(vSelected);
			UpdateUI(false, null, bUpdateGroupList, null, false, null, true);
		}

		private void DeleteSelectedGroup()
		{
			PwGroup pg = GetSelectedGroup();
			if(pg == null) { Debug.Assert(false); return; }

			PwGroup pgParent = pg.ParentGroup;
			if(pgParent == null) return; // Can't remove virtual or root group

			PwDatabase pd = m_docMgr.ActiveDatabase;
			PwGroup pgRecycleBin = pd.RootGroup.FindGroup(pd.RecycleBinUuid, true);
			bool bShiftPressed = ((Control.ModifierKeys & Keys.Shift) != Keys.None);

			bool bPermanent = false;
			if(!pd.RecycleBinEnabled) bPermanent = true;
			else if(bShiftPressed) bPermanent = true;
			else if(pgRecycleBin == null) { }
			else if(pg == pgRecycleBin) bPermanent = true;
			else if(pg.IsContainedIn(pgRecycleBin)) bPermanent = true;
			else if(pgRecycleBin.IsContainedIn(pg)) bPermanent = true;

			string strContent = EntryUtil.CreateSummaryList(pg, false);
			if(strContent.Length > 0)
				strContent = KPRes.DeleteGroupInfo + MessageService.NewParagraph +
					strContent;

			if(bPermanent)
			{
				VistaTaskDialog dlg = new VistaTaskDialog();
				dlg.CommandLinks = false;
				dlg.Content = strContent;
				dlg.MainInstruction = KPRes.DeleteGroupQuestion;
				dlg.SetIcon(VtdCustomIcon.Question);
				dlg.WindowTitle = PwDefs.ShortProductName;
				dlg.AddButton((int)DialogResult.OK, KPRes.DeleteCmd, null);
				dlg.AddButton((int)DialogResult.Cancel, KPRes.Cancel, null);

				if(dlg.ShowDialog())
				{
					if(dlg.Result == (int)DialogResult.Cancel) return;
				}
				else
				{
					string strText = KPRes.DeleteGroupQuestion;
					if(strContent.Length > 0)
						strText += MessageService.NewParagraph + strContent;

					if(!MessageService.AskYesNo(strText, KPRes.DeleteGroupTitle))
						return;
				}
			}
			else if(Program.Config.UI.ShowRecycleConfirmDialog)
			{
				VistaTaskDialog dlg = new VistaTaskDialog();
				dlg.CommandLinks = false;
				dlg.Content = strContent;
				dlg.MainInstruction = KPRes.RecycleGroupConfirm;
				dlg.SetIcon(VtdCustomIcon.Question);
				dlg.VerificationText = UIUtil.GetDialogNoShowAgainText(KPRes.Yes);
				dlg.WindowTitle = PwDefs.ShortProductName;
				dlg.AddButton((int)DialogResult.OK, KPRes.YesCmd, null);
				dlg.AddButton((int)DialogResult.Cancel, KPRes.NoCmd, null);

				if(dlg.ShowDialog())
				{
					if(dlg.Result == (int)DialogResult.Cancel) return;

					if(dlg.ResultVerificationChecked)
						Program.Config.UI.ShowRecycleConfirmDialog = false;
				}
				else
				{
					string strText = KPRes.RecycleGroupConfirm;
					if(strContent.Length > 0)
						strText += MessageService.NewParagraph + strContent;

					if(!MessageService.AskYesNo(strText, KPRes.DeleteGroupTitle))
						return;
				}
			}

			if(!pgParent.Groups.Remove(pg)) { Debug.Assert(false); return; }

			if(bPermanent)
			{
				pg.DeleteAllObjects(pd);

				PwDeletedObject pdo = new PwDeletedObject(pg.Uuid, DateTime.UtcNow);
				pd.DeletedObjects.Add(pdo);
			}
			else // Recycle
			{
				bool bDummy = false;
				EnsureRecycleBin(ref pgRecycleBin, pd, ref bDummy);

				try
				{
					pgRecycleBin.AddGroup(pg, true, true);
					pg.PreviousParentGroup = pgParent.Uuid;
				}
				catch(Exception ex)
				{
					if(pgRecycleBin.Groups.IndexOf(pg) < 0)
						pgParent.AddGroup(pg, true, true); // Undo removal
					MessageService.ShowWarning(ex);
				}

				pg.Touch(false);
			}

			UpdateUI(false, null, true, pgParent, true, null, true);
		}

		private void EmptyRecycleBin()
		{
			PwDatabase pd = m_docMgr.ActiveDatabase;
			if((pd == null) || !pd.IsOpen) { Debug.Assert(false); return; }

			PwGroup pg = pd.RootGroup.FindGroup(pd.RecycleBinUuid, true);
			if(pg == null) { Debug.Assert(false); return; }

			string strSummary = EntryUtil.CreateSummaryList(pg, false);

			VistaTaskDialog dlg = new VistaTaskDialog();
			dlg.CommandLinks = false;
			dlg.Content = strSummary;
			dlg.MainInstruction = KPRes.EmptyRecycleBinQuestion;
			dlg.SetIcon(VtdCustomIcon.Question);
			dlg.WindowTitle = PwDefs.ShortProductName;
			dlg.AddButton((int)DialogResult.OK, KPRes.DeleteCmd, null);
			dlg.AddButton((int)DialogResult.Cancel, KPRes.Cancel, null);

			if(dlg.ShowDialog())
			{
				if(dlg.Result == (int)DialogResult.Cancel) return;
			}
			else
			{
				string strText = KPRes.EmptyRecycleBinQuestion;
				if(strSummary.Length > 0)
					strText += MessageService.NewParagraph + strSummary;

				if(!MessageService.AskYesNo(strText))
					return;
			}

			pg.DeleteAllObjects(pd);

			UpdateUI(false, null, true, pg, true, null, true);
		}

		// private static string GetGroupSuffixText(PwGroup pg)
		// {
		// if(pg == null) { Debug.Assert(false); return string.Empty; }
		// if(pg.Entries.UCount == 0) return string.Empty;
		// if(!GroupOnlyContainsTans(pg, true)) return string.Empty;
		// DateTime dtNow = DateTime.UtcNow;
		// uint uValid = 0;
		// foreach(PwEntry pe in pg.Entries)
		// {
		//	if(pe.Expires && (pe.ExpiryTime <= dtNow)) { }
		//	else ++uValid;
		// }
		// return (" (" + uValid.ToString() + "/" + pg.Entries.UCount.ToString() + ")");
		// }

		public void AddCustomToolBarButton(string strID, string strName, string strDesc)
		{
			if(string.IsNullOrEmpty(strID)) { Debug.Assert(false); return; } // No throw
			if(string.IsNullOrEmpty(strName)) { Debug.Assert(false); return; } // No throw

			if(m_lCustomToolBarButtons.Count == 0)
			{
				m_tsSepCustomToolBar = new ToolStripSeparator();
				m_toolMain.Items.Add(m_tsSepCustomToolBar);
			}

			ToolStripButton btn = new ToolStripButton(strName);
			btn.Tag = strID;
			btn.Click += OnCustomToolBarButtonClicked;
			if(!string.IsNullOrEmpty(strDesc)) btn.ToolTipText = strDesc;

			m_toolMain.Items.Add(btn);
			m_lCustomToolBarButtons.Add(btn);
		}

		public void RemoveCustomToolBarButton(string strID)
		{
			if(string.IsNullOrEmpty(strID)) { Debug.Assert(false); return; } // No throw

			foreach(ToolStripButton tb in m_lCustomToolBarButtons)
			{
				string str = (tb.Tag as string);
				if(string.IsNullOrEmpty(str)) { Debug.Assert(false); continue; }

				if(str.Equals(strID, StrUtil.CaseIgnoreCmp))
				{
					tb.Click -= OnCustomToolBarButtonClicked;
					m_toolMain.Items.Remove(tb);
					m_lCustomToolBarButtons.Remove(tb);
					break;
				}
			}

			if((m_lCustomToolBarButtons.Count == 0) && (m_tsSepCustomToolBar != null))
			{
				m_toolMain.Items.Remove(m_tsSepCustomToolBar);
				m_tsSepCustomToolBar = null;
			}
		}

		private void OnCustomToolBarButtonClicked(object sender, EventArgs e)
		{
			ToolStripButton btn = (sender as ToolStripButton);
			if(btn == null) { Debug.Assert(false); return; }

			string strID = (btn.Tag as string);
			if(string.IsNullOrEmpty(strID)) { Debug.Assert(false); return; }

			Program.TriggerSystem.RaiseEvent(EcasEventIDs.CustomTbButtonClicked,
				EcasProperty.CommandID, strID);
		}

		internal IOConnectionInfo IocFromCommandLine()
		{
			CommandLineArgs args = Program.CommandLineArgs;
			IOConnectionInfo ioc = IOConnectionInfo.FromPath(args.FileName ??
				string.Empty);

			if(ioc.IsLocalFile()) // Expand relative path to absolute
				ioc.Path = UrlUtil.MakeAbsolutePath(UrlUtil.EnsureTerminatingSeparator(
					WinUtil.GetWorkingDirectory(), false) + "Sentinel", ioc.Path);

			// Set the user name, which acts as a filter for the MRU items
			string strUserName = args[AppDefs.CommandLineOptions.IoCredUserName];
			if(strUserName != null) ioc.UserName = strUserName;

			if(args[AppDefs.CommandLineOptions.IoCredFromRecent] != null)
				ioc = CompleteConnectionInfoUsingMru(ioc);

			// Override the password
			string strPassword = args[AppDefs.CommandLineOptions.IoCredPassword];
			if(strPassword != null) ioc.Password = strPassword;

			string strComplete = args[AppDefs.CommandLineOptions.IoCredIsComplete];
			if(strComplete != null) ioc.IsComplete = true;

			return ioc;
		}

		private static void RememberKeySources(PwDatabase pwDb)
		{
			if(pwDb == null) { Debug.Assert(false); return; }

			Program.Config.Defaults.SetKeySources(pwDb.IOConnectionInfo,
				pwDb.MasterKey);
		}

		private void SerializeMruList(bool bStore)
		{
			AceMru aceMru = Program.Config.Application.MostRecentlyUsed;

			if(bStore)
			{
				if(!m_mruList.IsValid) { Debug.Assert(false); return; }

				aceMru.MaxItemCount = m_mruList.MaxItemCount;

				aceMru.Items.Clear();
				// Count <= max is not guaranteed, therefore take minimum of both
				uint uMax = Math.Min(m_mruList.MaxItemCount, m_mruList.ItemCount);
				for(uint uMru = 0; uMru < uMax; ++uMru)
				{
					KeyValuePair<string, object> kvpMru = m_mruList.GetItem(uMru);
					IOConnectionInfo ioMru = (kvpMru.Value as IOConnectionInfo);
					if(ioMru != null) aceMru.Items.Add(ioMru);
					else { Debug.Assert(false); }
				}
			}
			else // Load
			{
				m_mruList.MaxItemCount = aceMru.MaxItemCount;

				int nMax = Math.Min((int)m_mruList.MaxItemCount, aceMru.Items.Count);
				for(int iMru = 0; iMru < nMax; ++iMru)
				{
					IOConnectionInfo ioMru = aceMru.Items[nMax - iMru - 1];
					m_mruList.AddItem(ioMru.GetDisplayName(), ioMru.CloneDeep());
				}
			}
		}

		[Obsolete]
		public void RedirectActivationPush(Form formTarget)
		{
			// m_vRedirectActivation.Push(formTarget);
		}

		[Obsolete]
		public void RedirectActivationPop()
		{
			// if(m_vRedirectActivation.Count == 0) { Debug.Assert(false); return; }
			// m_vRedirectActivation.Pop();
		}

		private bool ChangeMasterKey(PwDatabase pdOf)
		{
			PwDatabase pd = (pdOf ?? m_docMgr.ActiveDatabase);
			if((pd == null) || !pd.IsOpen) { Debug.Assert(false); return false; }

			if(!AppPolicy.TryWithKey(AppPolicyId.ChangeMasterKey,
				AppPolicy.Current.ChangeMasterKeyNoKey, pd, null))
				return false;

			KeyCreationFormResult r;
			DialogResult dr = KeyCreationForm.ShowDialog(pd.IOConnectionInfo, false, out r);
			if((dr != DialogResult.OK) || (r == null)) return false;

			pd.MasterKey = r.CompositeKey;
			pd.MasterKeyChanged = DateTime.UtcNow;
			// pd.MasterKeyChangeForceOnce = false;
			pd.Modified = true;

			if(this.MasterKeyChanged != null)
				this.MasterKeyChanged(null, new MasterKeyChangedEventArgs(pd));

			UpdateUIState(false); // Show modified state in the UI

			string str = KPRes.MasterKeyChanged + MessageService.NewParagraph +
				KPRes.MasterKeyChangedSave + MessageService.NewParagraph +
				KPRes.FileSaveQ;

			VistaTaskDialog dlg = new VistaTaskDialog();
			dlg.CommandLinks = false;
			dlg.Content = str;
			dlg.DefaultButtonID = (int)DialogResult.OK;
			dlg.MainInstruction = KPRes.MasterKeyChangedShort;
			dlg.WindowTitle = PwDefs.ShortProductName;

			dlg.SetIcon(VtdCustomIcon.Question);
			dlg.AddButton((int)DialogResult.OK, KPRes.Save, null);
			dlg.AddButton((int)DialogResult.Cancel, KPRes.NotNow, null);

			bool b;
			if(dlg.ShowDialog(this)) b = (dlg.Result == (int)DialogResult.OK);
			else b = MessageService.AskYesNo(str);

			if(b)
			{
				SaveDatabase(pd, null);

				// Show emergency sheet creation prompt only when the
				// database was saved successfully, otherwise the sheet
				// could contain incorrect information
				if(!pd.Modified) EmergencySheet.AskCreate(pd);
			}

			return true;
		}

		private void UpdateColumnsEx(bool bGuiToInternal)
		{
			if(m_bBlockColumnUpdates) return;
			m_bBlockColumnUpdates = true;

			if(!bGuiToInternal) // Internal to GUI
			{
				m_asyncListUpdate.CancelPendingUpdatesAsync();

				m_lvEntries.BeginUpdate();
				lock(m_asyncListUpdate.ListEditSyncObject)
				{
					m_lvEntries.Items.Clear();
				}
				m_lvEntries.Columns.Clear();

				if(Program.Config.MainWindow.EntryListColumns.Count == 0)
				{
					int nWidth = (m_lvEntries.ClientRectangle.Width -
						UIUtil.GetVScrollBarWidth()) / 5;
					EntryListAddColumn(AceColumnType.Title, nWidth, false);
					EntryListAddColumn(AceColumnType.UserName, nWidth, false);
					EntryListAddColumn(AceColumnType.Password, nWidth, true);
					EntryListAddColumn(AceColumnType.Url, nWidth, false);
					EntryListAddColumn(AceColumnType.Notes, nWidth, false);
				}

				int nDefaultWidth = m_lvEntries.ClientRectangle.Width /
					Program.Config.MainWindow.EntryListColumns.Count;
				foreach(AceColumn col in Program.Config.MainWindow.EntryListColumns)
				{
					ColumnHeader ch = m_lvEntries.Columns.Add(col.GetDisplayName(),
						col.SafeGetWidth(nDefaultWidth));

					HorizontalAlignment ha;
					if(col.Type == AceColumnType.PluginExt)
						ha = Program.ColumnProviderPool.GetTextAlign(col.CustomName);
					else ha = AceColumn.GetTextAlign(col.Type);
					if(ha != HorizontalAlignment.Left) ch.TextAlign = ha;
				}

				int[] vDisplayIndices = StrUtil.DeserializeIntArray(
					Program.Config.MainWindow.EntryListColumnDisplayOrder);
				UIUtil.SetDisplayIndices(m_lvEntries, vDisplayIndices);

				m_lvEntries.EndUpdate();
				UpdateColumnSortingIcons();
			}
			else // GUI to internal
			{
				List<AceColumn> l = Program.Config.MainWindow.EntryListColumns;
				if(l.Count == m_lvEntries.Columns.Count)
				{
					int[] vDisplayIndices = new int[l.Count];
					for(int i = 0; i < l.Count; ++i)
					{
						l[i].Width = m_lvEntries.Columns[i].Width;
						vDisplayIndices[i] = m_lvEntries.Columns[i].DisplayIndex;
					}

					Program.Config.MainWindow.EntryListColumnDisplayOrder =
						StrUtil.SerializeIntArray(vDisplayIndices);
				}
				else { Debug.Assert(false); }
			}

			m_bBlockColumnUpdates = false;
		}

		private static void EntryListAddColumn(AceColumnType t, int nWidth, bool bHide)
		{
			AceColumn c = new AceColumn(t, string.Empty, bHide, nWidth);
			Program.Config.MainWindow.EntryListColumns.Add(c);
		}

		private string GetEntryFieldEx(PwEntry pe, int iColumnID,
			bool bFormatForDisplay, out bool bRequestAsync)
		{
			List<AceColumn> l = Program.Config.MainWindow.EntryListColumns;
			if((iColumnID < 0) || (iColumnID >= l.Count))
			{
				Debug.Assert(false);
				bRequestAsync = false;
				return string.Empty;
			}

			AceColumn col = l[iColumnID];
			if(bFormatForDisplay && col.HideWithAsterisks)
			{
				bRequestAsync = false;
				return PwDefs.HiddenPassword;
			}

			string str;
			switch(col.Type)
			{
				case AceColumnType.Title: str = pe.Strings.ReadSafe(PwDefs.TitleField); break;
				case AceColumnType.UserName: str = pe.Strings.ReadSafe(PwDefs.UserNameField); break;
				case AceColumnType.Password: str = pe.Strings.ReadSafe(PwDefs.PasswordField); break;
				case AceColumnType.Url: str = pe.Strings.ReadSafe(PwDefs.UrlField); break;
				case AceColumnType.Notes:
					str = pe.Strings.ReadSafe(PwDefs.NotesField);
					if(bFormatForDisplay) str = StrUtil.MultiToSingleLine(str);
					break;
				case AceColumnType.CreationTime:
					str = TimeUtil.ToDisplayString(pe.CreationTime);
					break;
				case AceColumnType.LastModificationTime:
					str = TimeUtil.ToDisplayString(pe.LastModificationTime);
					break;
				case AceColumnType.LastAccessTime:
					str = TimeUtil.ToDisplayString(pe.LastAccessTime);
					break;
				case AceColumnType.ExpiryTime:
					if(pe.Expires) str = TimeUtil.ToDisplayString(pe.ExpiryTime);
					else str = m_strNeverExpires;
					break;
				case AceColumnType.Uuid: str = pe.Uuid.ToHexString(); break;
				case AceColumnType.Attachment: str = pe.Binaries.KeysToString(); break;
				case AceColumnType.CustomString:
					str = pe.Strings.ReadSafe(col.CustomName);
					if(bFormatForDisplay) str = StrUtil.MultiToSingleLine(str);
					break;
				case AceColumnType.PluginExt:
					str = Program.ColumnProviderPool.GetCellData(col.CustomName, pe);
					if(bFormatForDisplay) str = StrUtil.MultiToSingleLine(str);
					break;
				case AceColumnType.OverrideUrl: str = pe.OverrideUrl; break;
				case AceColumnType.Tags:
					str = TagUtil.TagsInheritedToString(pe.Tags, pe.ParentGroup);
					break;
				case AceColumnType.ExpiryTimeDateOnly:
					if(pe.Expires) str = TimeUtil.ToDisplayStringDateOnly(pe.ExpiryTime);
					else str = m_strNeverExpires;
					break;
				case AceColumnType.Size:
					str = StrUtil.FormatDataSizeKB(pe.GetSize());
					break;
				case AceColumnType.HistoryCount:
					str = pe.History.UCount.ToString();
					break;
				case AceColumnType.AttachmentCount:
					uint uc = pe.Binaries.UCount;
					str = ((uc > 0) ? uc.ToString() : string.Empty);
					break;
				case AceColumnType.LastPasswordModTime:
					str = TimeUtil.ToDisplayString(EntryUtil.GetLastPasswordModTime(pe));
					break;
				case AceColumnType.AutoTypeEnabled:
					object oBlocker;
					str = AutoType.GetEnabledText(pe, out oBlocker);
					break;
				case AceColumnType.AutoTypeSequences:
					str = AutoType.GetSequencesText(pe);
					break;
				default: Debug.Assert(false); str = string.Empty; break;
			}

			if(Program.Config.MainWindow.EntryListShowDerefData && bFormatForDisplay)
			{
				switch(col.Type)
				{
					case AceColumnType.Title:
					case AceColumnType.UserName:
					case AceColumnType.Password:
					case AceColumnType.Url:
					case AceColumnType.Notes:
					case AceColumnType.CustomString:
						bRequestAsync = SprEngine.MightDeref(str);
						break;
					default: bRequestAsync = false; break;
				}

				if(!Program.Config.MainWindow.EntryListShowDerefDataAsync &&
					bRequestAsync)
				{
					PwListItem pli = new PwListItem(pe);
					str = AsyncPwListUpdate.SprCompileFn(str, pli);
					bRequestAsync = false;
				}
			}
			else bRequestAsync = false;

			return str;
		}

		private static AceColumn GetAceColumn(int nColID)
		{
			List<AceColumn> v = Program.Config.MainWindow.EntryListColumns;
			if((nColID < 0) || (nColID >= v.Count)) { Debug.Assert(false); return new AceColumn(); }

			return v[nColID];
		}

		private void ToggleFieldAsterisks(AceColumnType colType)
		{
			List<AceColumn> l = Program.Config.MainWindow.EntryListColumns;
			foreach(AceColumn c in l)
			{
				if(c.Type == colType)
				{
					if((colType == AceColumnType.Password) && c.HideWithAsterisks &&
						!AppPolicy.Try(AppPolicyId.UnhidePasswords))
						return;

					c.HideWithAsterisks = !c.HideWithAsterisks;
				}
			}

			RefreshEntriesList();
			UpdateUIState(false); // Update entry view
		}

		private void EditSelectedEntry(PwEntryFormTab eftInit)
		{
			PwDatabase pd = m_docMgr.ActiveDatabase;
			if((pd == null) || !pd.IsOpen) { Debug.Assert(false); return; }

			MultipleValuesEntryContext mvec = null;
			bool bMod;
			try
			{
				PwEntry pe;
				PwEntry[] v = GetSelectedEntries();
				if((v == null) || (v.Length <= 1))
				{
					pe = GetSelectedEntry(false);
					if(pe == null) return; // Do not assert
				}
				else
				{
					mvec = new MultipleValuesEntryContext(v, pd, out pe);

					// if(pd.UINeedsIconUpdate)
					// {
					//	RefreshEntriesList();
					//	UpdateUI(false, null, true, null, false, null, false);
					// }
					UpdateImageLists(false); // Image indices remain the same
				}

				PwEntryForm dlg = new PwEntryForm();
				dlg.InitEx(pe, PwEditMode.EditExistingEntry, pd, m_ilCurrentIcons,
					false, false);
				dlg.InitialTab = eftInit;
				dlg.MultipleValuesEntryContext = mvec;

				bool bOK = (dlg.ShowDialog() == DialogResult.OK);
				bMod = (bOK && dlg.HasModifiedEntry);
				UIUtil.DestroyForm(dlg);

				// Check bOK instead of bMod (dialog mod. check ignores multi states)
				if(bOK && (mvec != null))
				{
					if(mvec.ApplyChanges()) bMod = true;
				}
			}
			finally { if(mvec != null) mvec.Dispose(); }

			bool bUpdImg = pd.UINeedsIconUpdate; // Refreshing entries resets it
			RefreshEntriesList(); // Last access time
			UpdateUI(false, null, bUpdImg, null, false, null, bMod);

			if(Program.Config.Application.AutoSaveAfterEntryEdit && bMod)
				SaveDatabase(pd, null);
		}

		private void EditSelectedGroup(GroupFormTab gftInit)
		{
			PwDatabase pd = m_docMgr.ActiveDatabase;
			if((pd == null) || !pd.IsOpen) { Debug.Assert(false); return; }

			PwGroup pg = GetSelectedGroup();
			if(pg == null) { Debug.Assert(false); return; }

			GroupForm gf = new GroupForm();
			gf.InitEx(pg, false, m_ilCurrentIcons, pd);
			gf.InitialTab = gftInit;

			if(UIUtil.ShowDialogAndDestroy(gf) == DialogResult.OK)
				UpdateUI(false, null, true, null, true, null, true);
			else UpdateUI(false, null, pd.UINeedsIconUpdate, null,
				pd.UINeedsIconUpdate, null, false);
		}

		private static bool ListContainsOnlyTans(PwObjectList<PwEntry> vEntries)
		{
			if(vEntries == null) { Debug.Assert(false); return true; }

			foreach(PwEntry pe in vEntries)
			{
				if(!PwDefs.IsTanEntry(pe)) return false;
			}

			return true;
		}

		private void PerformSearch(SearchParameters sp, bool bInGroup,
			bool bFocusEntryList)
		{
			if(sp == null) { Debug.Assert(false); return; }

			PwDatabase pd = m_docMgr.ActiveDatabase;
			if((pd == null) || !pd.IsOpen) { Debug.Assert(false); return; }

			PwGroup pgRoot = (bInGroup ? GetSelectedGroup() : null);
			if(pgRoot == null) pgRoot = pd.RootGroup;
			if(pgRoot == null) { Debug.Assert(false); return; }

			Application.DoEvents(); // Handle UI messages before blocking
			UIBlockInteraction(true);
			StatusBarLogger sl = CreateStatusBarLogger();
			sl.StartLogging(KPRes.SearchingOp + "...", false);

			PwGroup pgResults = null;
			Exception exFind = null;
			try { pgResults = SearchUtil.Find(sp, pgRoot, sl); }
			catch(Exception ex) { exFind = ex; }

			sl.EndLogging();
			UIBlockInteraction(false);

			if(exFind != null)
				MessageService.ShowWarning(sp.SearchString, exFind);
			else ShowSearchResults(pgResults, sp, pgRoot, bFocusEntryList);
		}

		private void PerformSearchDialog(string strProfile, bool bInGroup)
		{
			PwDatabase pd = m_docMgr.ActiveDatabase;
			if((pd == null) || !pd.IsOpen) { Debug.Assert(false); return; }

			PwGroup pgRoot = (bInGroup ? GetSelectedGroup() : null);
			if(pgRoot == null) pgRoot = pd.RootGroup;
			if(pgRoot == null) { Debug.Assert(false); return; }

			SearchForm sf = new SearchForm();
			sf.InitEx(pd, pgRoot);
			sf.InitProfile = strProfile;

			if(sf.ShowDialog() == DialogResult.OK)
				ShowSearchResults(sf.SearchResultsGroup, sf.SearchResultParameters,
					pgRoot, true);
			UIUtil.DestroyForm(sf);
		}

		private static string[] g_vProfileCmdTexts = null;
		private void UpdateFindProfilesMenu(ToolStripMenuItem tsmiProfiles,
			bool bEnsurePopupOnly)
		{
			if(tsmiProfiles == null) { Debug.Assert(false); return; }

			ToolStripItemCollection tsic = tsmiProfiles.DropDownItems;
			tsic.Clear();

			if(bEnsurePopupOnly)
			{
				tsic.Add(new ToolStripMenuItem(KPRes.None));
				return;
			}

			if(g_vProfileCmdTexts == null)
			{
				AccessKeyManagerEx akCmd = new AccessKeyManagerEx();
				GFunc<string, string, bool, string> f = delegate(string str,
					string strSuffix, bool bDots)
				{
					str = StrUtil.CommandToText(str);
					if(!string.IsNullOrEmpty(strSuffix))
						str += " (" + strSuffix + ")";
					if(bDots) str += "...";
					return akCmd.CreateText(str, true);
				};

				g_vProfileCmdTexts = new string[4];
				g_vProfileCmdTexts[0] = f(KPRes.Find, null, false);
				g_vProfileCmdTexts[1] = f(KPRes.Find, KPRes.SelectedGroup, false);
				g_vProfileCmdTexts[2] = f(KPRes.OpenCmd, null, true);
				g_vProfileCmdTexts[3] = f(KPRes.OpenCmd, KPRes.SelectedGroup, true);
			}

			List<ToolStripItem> lTsi = new List<ToolStripItem>();
			AccessKeyManagerEx akPrf = new AccessKeyManagerEx();

			AceSearch aceSearch = Program.Config.Search;
			aceSearch.SortProfiles();

			foreach(SearchParameters sp in aceSearch.UserProfiles)
			{
				ToolStripMenuItem tsmi = new ToolStripMenuItem(
					akPrf.CreateText(sp.Name, true));
				lTsi.Add(tsmi);

				ToolStripMenuItem tsmiFD = new ToolStripMenuItem(
					g_vProfileCmdTexts[0], Properties.Resources.B16x16_XMag,
					this.OnFindProfileFind);
				tsmiFD.Tag = sp.Name;
				tsmi.DropDownItems.Add(tsmiFD);

				ToolStripMenuItem tsmiFG = new ToolStripMenuItem(
					g_vProfileCmdTexts[1], Properties.Resources.B16x16_XMag,
					this.OnFindProfileFindInGroup);
				tsmiFG.Tag = sp.Name;
				tsmi.DropDownItems.Add(tsmiFG);

				tsmi.DropDownItems.Add(new ToolStripSeparator());

				ToolStripMenuItem tsmiOD = new ToolStripMenuItem(
					g_vProfileCmdTexts[2], Properties.Resources.B16x16_Misc,
					this.OnFindProfileOpen);
				tsmiOD.Tag = sp.Name;
				tsmi.DropDownItems.Add(tsmiOD);

				ToolStripMenuItem tsmiOG = new ToolStripMenuItem(
					g_vProfileCmdTexts[3], Properties.Resources.B16x16_Misc,
					this.OnFindProfileOpenInGroup);
				tsmiOG.Tag = sp.Name;
				tsmi.DropDownItems.Add(tsmiOG);
			}

			if(lTsi.Count == 0)
			{
				ToolStripMenuItem tsmi = new ToolStripMenuItem(
					StrUtil.EncodeMenuText("(" + KPRes.None + ")"));
				tsmi.Enabled = false;
				lTsi.Add(tsmi);
			}

			tsic.AddRange(lTsi.ToArray());
		}

		private void PerformSearchMenu(object sender, bool bInGroup, bool bOpen)
		{
			ToolStripMenuItem tsmi = (sender as ToolStripMenuItem);
			if(tsmi == null) { Debug.Assert(false); return; }

			string strProfile = (tsmi.Tag as string);
			if(string.IsNullOrEmpty(strProfile)) { Debug.Assert(false); return; }

			if(bOpen) PerformSearchDialog(strProfile, bInGroup);
			else
			{
				SearchParameters sp = Program.Config.Search.FindProfile(strProfile);
				PerformSearch(sp, bInGroup, true);
			}
		}

		private void OnFindProfileFind(object sender, EventArgs e)
		{
			PerformSearchMenu(sender, false, false);
		}

		private void OnFindProfileFindInGroup(object sender, EventArgs e)
		{
			PerformSearchMenu(sender, true, false);
		}

		private void OnFindProfileOpen(object sender, EventArgs e)
		{
			PerformSearchMenu(sender, false, true);
		}

		private void OnFindProfileOpenInGroup(object sender, EventArgs e)
		{
			PerformSearchMenu(sender, true, true);
		}

		private void OnShowEntriesByTag(object sender, DynamicMenuEventArgs e)
		{
			if(e == null) { Debug.Assert(false); return; }
			ShowEntriesByTag((e.Tag as string), true);
		}

		internal void ShowEntriesByTag(string strTag, bool bFocusEntryList)
		{
			if(strTag == null) { Debug.Assert(false); return; }
			if(strTag.Length == 0) return; // No assert (call from trigger)

			PwDatabase pd = m_docMgr.ActiveDatabase;
			if((pd == null) || !pd.IsOpen) return; // No assert (call from trigger)

			PwObjectList<PwEntry> l = new PwObjectList<PwEntry>();
			pd.RootGroup.FindEntriesByTag(strTag, l, true);

			PwGroup pgResults = new PwGroup(true, true);
			pgResults.IsVirtual = true;
			foreach(PwEntry pe in l) pgResults.AddEntry(pe, false, false);

			ShowSearchResults(pgResults, null, pd.RootGroup, bFocusEntryList);
		}

		private enum TagsMenuMode
		{
			All = 0,
			Add = 1,
			Remove = 2,

			EnsurePopupOnly = 127 // Ensure drawing of menu popup arrow
		}

		private void UpdateTagsMenu(DynamicMenu dm, bool bWithSeparator, bool bPrefixTag,
			TagsMenuMode tmm)
		{
			if(dm == null) { Debug.Assert(false); return; }
			dm.Clear();

			if(bWithSeparator) dm.AddSeparator();

			string strNoTags = "(" + KPRes.TagsNotFound + ")";
			PwDatabase pd = m_docMgr.ActiveDatabase;
			if(!pd.IsOpen || (tmm == TagsMenuMode.EnsurePopupOnly))
			{
				ToolStripMenuItem tsmi = dm.AddItem(strNoTags, null, string.Empty);
				tsmi.Enabled = false;
				return;
			}

			bool bReqEntrySel = ((tmm == TagsMenuMode.Add) || (tmm == TagsMenuMode.Remove));
			PwGroup pgSel = GetSelectedEntriesAsGroup();
			uint uSelCount = pgSel.Entries.UCount;
			bool bForceDisabled = (bReqEntrySel && (uSelCount == 0));

			IDictionary<string, uint> dAllTags = pd.RootGroup.BuildEntryTagsDict(false);
			List<string> lAllTags = new List<string>(dAllTags.Keys);
			lAllTags.Sort(StrUtil.CompareNaturally);

			Dictionary<string, bool> dEnabledTags = null;
			if((tmm == TagsMenuMode.Add) && (uSelCount > 0))
			{
				dEnabledTags = new Dictionary<string, bool>();
				List<string> lIntersect = pgSel.Entries.GetAt(0).Tags;
				for(uint u = 1; u < uSelCount; ++u)
					lIntersect = new List<string>(MemUtil.Intersect(lIntersect,
						pgSel.Entries.GetAt(u).Tags, null));
				foreach(string strTag in MemUtil.Except(lAllTags, lIntersect, null))
					dEnabledTags[strTag] = true;
			}
			else if(tmm == TagsMenuMode.Remove)
			{
				dEnabledTags = new Dictionary<string, bool>();
				List<string> lSelectedTags = pgSel.BuildEntryTagsList(false, false);
				foreach(string strTag in lSelectedTags)
					dEnabledTags[strTag] = true;
			}

			string strPrefix = StrUtil.EncodeMenuText(KPRes.Tag + ": ");
			Image imgIcon = Properties.Resources.B16x16_KNotes;
			AccessKeyManagerEx ak = new AccessKeyManagerEx();

			foreach(string strTag in lAllTags)
			{
				string strText = ak.CreateText(strTag, true);
				if(bPrefixTag) strText = strPrefix + strText;

				ToolStripMenuItem tsmi = dm.AddItem(strText, imgIcon, strTag);
				if(tmm == TagsMenuMode.All)
				{
					uint uCount;
					dAllTags.TryGetValue(strTag, out uCount);
					tsmi.ShortcutKeyDisplayString = "(" + uCount.ToString() + ")";
				}

				if(bForceDisabled) tsmi.Enabled = false;
				else if(dEnabledTags != null)
				{
					if(!dEnabledTags.ContainsKey(strTag)) tsmi.Enabled = false;
				}
			}

			if(lAllTags.Count == 0)
			{
				ToolStripMenuItem tsmi = dm.AddItem(strNoTags, null, string.Empty);
				tsmi.Enabled = false;
			}
		}

		private void OnAddEntryTag(object sender, DynamicMenuEventArgs e)
		{
			string strTag = (e.Tag as string);
			if(strTag == null) { Debug.Assert(false); return; }

			if(strTag.Length == 0)
			{
				SingleLineEditForm dlg = new SingleLineEditForm();
				dlg.InitEx(KPRes.TagNew, KPRes.TagAddNew, KPRes.Name + ":",
					Properties.Resources.B48x48_KMag, string.Empty, null);

				if(UIUtil.ShowDialogNotValue(dlg, DialogResult.OK)) return;
				strTag = dlg.ResultString;
				UIUtil.DestroyForm(dlg);
			}

			AddOrRemoveTagsToFromSelectedEntries(strTag, true);
		}

		private void OnRemoveEntryTag(object sender, DynamicMenuEventArgs e)
		{
			AddOrRemoveTagsToFromSelectedEntries((e.Tag as string), false);
		}

		private void AddOrRemoveTagsToFromSelectedEntries(string strTags, bool bAdd)
		{
			if(strTags == null) { Debug.Assert(false); return; }
			if(strTags.Length == 0) return;

			PwDatabase pd = m_docMgr.ActiveDatabase;
			PwEntry[] vEntries = GetSelectedEntries();
			if((vEntries == null) || (vEntries.Length == 0)) return;

			List<string> lToProcess = StrUtil.StringToTags(strTags);
			bool bModified = false;

			foreach(PwEntry pe in vEntries)
			{
				List<string> l = pe.Tags;
				int cBefore = l.Count;

				if(bAdd)
					l = new List<string>(MemUtil.Union<string>(l, lToProcess, null));
				else
					l = new List<string>(MemUtil.Except<string>(l, lToProcess, null));

				if(l.Count != cBefore)
				{
					pe.CreateBackup(pd);

					pe.Tags = l;
					pe.Touch(true, false);

					bModified = true;
				}
			}

			if(bModified) RefreshEntriesList();
			UpdateUIState(bModified);
		}

		private static bool? g_bCachedSelfTestResult = null;
		private static bool PerformSelfTest()
		{
			if(g_bCachedSelfTestResult.HasValue)
				return g_bCachedSelfTestResult.Value;

			bool bResult = true;
			try { SelfTest.Perform(); }
			catch(Exception ex)
			{
				MessageService.ShowWarning(KPRes.SelfTestFailed, ex);
				bResult = false;
			}

			g_bCachedSelfTestResult = bResult;
			return bResult;
		}

		private void SortSubGroups(bool bRecursive)
		{
			PwGroup pg = GetSelectedGroup();
			if(pg == null) return;

			if(!bRecursive && (pg.Groups.UCount <= 1)) return; // Nothing to do
			if(pg.Groups.UCount == 0) return; // Nothing to do

			pg.SortSubGroups(bRecursive);
			UpdateUI(false, null, true, null, false, null, true);
		}

		/* private bool CreateColorizedIcon(Icon icoBase, int qSize,
			ref KeyValuePair<Color, Icon> kvpStore, out Icon icoAssignable,
			out Icon icoDisposable)
		{
			PwDatabase pd = m_docMgr.ActiveDatabase;

			Color clrNew = Color.Empty;
			if((pd != null) && pd.IsOpen && !UIUtil.ColorsEqual(pd.Color, Color.Empty))
				clrNew = pd.Color;

			if(UIUtil.ColorsEqual(clrNew, kvpStore.Key))
			{
				icoDisposable = null;
				icoAssignable = (kvpStore.Value ?? icoBase);
				return false;
			}

			icoDisposable = kvpStore.Value;
			if(UIUtil.ColorsEqual(clrNew, Color.Empty))
			{
				kvpStore = new KeyValuePair<Color, Icon>(Color.Empty, null);
				icoAssignable = icoBase;
			}
			else
			{
				kvpStore = new KeyValuePair<Color, Icon>(clrNew,
					UIUtil.CreateColorizedIcon(icoBase, clrNew, qSize));
				icoAssignable = kvpStore.Value;
			}

			return true;
		} */

		private Icon CreateColorizedIcon(AppIconType t, bool bSmall)
		{
			PwDatabase pd = m_docMgr.ActiveDatabase;

			Color clr = Color.Empty;
			if((pd != null) && pd.IsOpen) clr = pd.Color;

			Size sz = (bSmall ? UIUtil.GetSmallIconSize() : UIUtil.GetIconSize());

			return AppIcons.Get(t, sz, clr);
		}

		private void SetObjectsDeletedStatus(uint uDeleted, bool bDbMntnc)
		{
			string str = (KPRes.ObjectsDeleted.Replace("{PARAM}",
				uDeleted.ToString()) + ".");
			SetStatusEx(str);
			if(!bDbMntnc || !Program.Config.UI.ShowDbMntncResultsDialog) return;

			VistaTaskDialog dlg = new VistaTaskDialog();
			dlg.CommandLinks = false;
			dlg.Content = str;
			dlg.SetIcon(VtdIcon.Information);
			dlg.VerificationText = UIUtil.GetDialogNoShowAgainText(null);
			dlg.WindowTitle = PwDefs.ShortProductName;
			if(dlg.ShowDialog())
			{
				if(dlg.ResultVerificationChecked)
					Program.Config.UI.ShowDbMntncResultsDialog = false;
			}
			else MessageService.ShowInfo(str);
		}

		private void ApplyUICustomizations()
		{
			ulong u = Program.Config.UI.UIFlags;
			if((u & (ulong)AceUIFlags.DisableOptions) != 0)
				m_menuToolsOptions.Enabled = false;
			if((u & (ulong)AceUIFlags.DisablePlugins) != 0)
				m_menuToolsPlugins.Enabled = false;
			if((u & (ulong)AceUIFlags.DisableTriggers) != 0)
				m_menuToolsTriggers.Enabled = false;
			if((u & (ulong)AceUIFlags.DisableUpdateCheck) != 0)
				m_menuHelpCheckForUpdates.Enabled = false;
		}

		private static void OnFormLoadParallelAsync(object stateInfo)
		{
			try
			{
				PopularPasswords.Add(Properties.Resources.MostPopularPasswords, true);

				// Unblock the application such that the user isn't
				// prompted next time anymore
				string strShInstUtil = UrlUtil.GetFileDirectory(
					WinUtil.GetExecutable(), true, false) + AppDefs.FileNames.ShInstUtil;
				WinUtil.RemoveZoneIdentifier(WinUtil.GetExecutable());
				WinUtil.RemoveZoneIdentifier(AppHelp.LocalHelpFile);
				WinUtil.RemoveZoneIdentifier(strShInstUtil);

				// https://stackoverflow.com/questions/26256917/how-can-i-prevent-my-application-from-causing-a-0xc0000142-error-in-csc-exe
				using(MemoryStream ms = new MemoryStream())
				{
					XmlUtilEx.Serialize<IpcParamEx>(ms, new IpcParamEx());
				}
				// using(MemoryStream ms = new MemoryStream())
				// {
				//	XmlUtilEx.Serialize<AppConfigEx>(ms, Program.Config);
				// }

				FileTransactionEx.ClearOld();
			}
			catch(Exception) { Debug.Assert(false); }

			// If the app configuration file is corrupted, an exception may
			// be thrown in the try block above (XML serializer, ...),
			// thus check it in a separate try block
			try { Program.CheckExeConfig(); }
			catch(Exception) { Debug.Assert(false); }
		}

		internal bool IsCommandTypeInvokable(MainAppState sContext, AppCommandType t)
		{
			MainAppState s = (sContext ?? GetMainAppState());

			if(t == AppCommandType.Window)
				return (s.NoWindowShown && !UIIsInteractionBlocked());
			if(t == AppCommandType.Lock)
				return (s.NoWindowShown && !UIIsInteractionBlocked() && s.EnableLockCmd);

			return false;
		}

		private void UpdateTrayState()
		{
			UpdateUIState(false); // Update tray lock text

			ulong u = Program.Config.UI.UIFlags;
			bool bOptions = ((u & (ulong)AceUIFlags.DisableOptions) == 0);

			MainAppState s = GetMainAppState();
			bool bInvkWnd = IsCommandTypeInvokable(s, AppCommandType.Window);

			m_ctxTrayTray.Enabled = bInvkWnd;
			m_ctxTrayGenPw.Enabled = bInvkWnd;
			m_ctxTrayOptions.Enabled = (bInvkWnd && bOptions);
			m_ctxTrayCancel.Enabled = (m_sCancellable.Count != 0);
			m_ctxTrayLock.Enabled = IsCommandTypeInvokable(s, AppCommandType.Lock);
			m_ctxTrayFileExit.Enabled = bInvkWnd;
		}

		internal static SprContext GetEntryListSprContext(PwEntry pe,
			PwDatabase pd)
		{
			SprContext ctx = new SprContext(pe, pd, SprCompileFlags.Deref);
			ctx.ForcePlainTextPasswords = false;
			return ctx;
		}

		private void EnsureAlwaysOnTopOpt()
		{
			bool bWish = Program.Config.MainWindow.AlwaysOnTop;
			if(NativeLib.IsUnix()) { this.TopMost = bWish; return; }

			// Workaround for issue reported in KPB 3475997
			this.TopMost = false;
			if(bWish) this.TopMost = true;
		}

		private bool IsPrimaryControlActive()
		{
			try
			{
				// return (m_lvEntries.Focused || m_tvGroups.Focused ||
				//	m_richEntryView.Focused || m_tbQuickFind.Focused);

				Control c = UIUtil.GetActiveControl(this);
				if(c == null) return false;

				return ((c == m_lvEntries) || (c == m_tvGroups) ||
					(c == m_richEntryView) || (c == m_tbQuickFind.Control));
			}
			catch(Exception) { Debug.Assert(false); }

			return false;
		}

		protected override void SetVisibleCore(bool value)
		{
			if(MonoWorkarounds.IsRequired(3574233558U))
			{
				if(!value && m_bFormShown && (this.WindowState ==
					FormWindowState.Minimized))
				{
					// Mono destroys window when trying to hide minimized
					// window; so, restore it before hiding
					RestoreWindowEx();
					Application.DoEvents();
					Thread.Sleep(250);
				}

				if(m_bFormShown) m_menuMain.Visible = value;
			}

			base.SetVisibleCore(value);
		}

		private void MoveOrCopySelectedEntries(PwGroup pgTo, DragDropEffects e)
		{
			PwEntry[] vSelected = GetSelectedEntries();
			if((vSelected == null) || (vSelected.Length == 0)) return;

			PwGroup pgSafeView = (m_pgActiveAtDragStart ?? new PwGroup());
			bool bFullUpdateView = false;
			List<PwEntry> lNowInvisible = new List<PwEntry>();

			if(e == DragDropEffects.Move)
			{
				foreach(PwEntry pe in vSelected)
				{
					PwGroup pgParent = pe.ParentGroup;
					if(pgParent == null) { Debug.Assert(false); continue; }
					if(pgParent == pgTo) continue;

					if(!pgParent.Entries.Remove(pe)) { Debug.Assert(false); continue; }

					pgTo.AddEntry(pe, true, true);

					// pe.CreateBackup(m_docMgr.ActiveDatabase);
					pe.PreviousParentGroup = pgParent.Uuid;
					// pe.Touch(true, false);

					if(pe.IsContainedIn(pgSafeView)) bFullUpdateView = true;
					else lNowInvisible.Add(pe);
				}
			}
			else if(e == DragDropEffects.Copy)
			{
				foreach(PwEntry pe in vSelected)
				{
					PwEntry peCopy = pe.Duplicate();

					pgTo.AddEntry(peCopy, true, true);

					if(peCopy.IsContainedIn(pgSafeView)) bFullUpdateView = true;
				}
			}
			else { Debug.Assert(false); }

			if(!bFullUpdateView)
			{
				RemoveEntriesFromList(lNowInvisible.ToArray());
				UpdateUI(false, null, true, m_pgActiveAtDragStart, false, null, true);
			}
			else UpdateUI(false, null, true, m_pgActiveAtDragStart, true, null, true);

			m_pgActiveAtDragStart = null;
		}

		private void UpdateEntryMoveMenu(DynamicMenu dm, bool bDummyOnly)
		{
			if(dm == null) { Debug.Assert(false); return; }

			dm.Clear();

			PwDatabase pd = m_docMgr.ActiveDatabase;
			PwGroup pgRoot = (((pd != null) && pd.IsOpen) ? pd.RootGroup : null);
			if((pgRoot == null) || bDummyOnly)
			{
				ToolStripMenuItem tsmi = dm.AddItem(m_strNoneP, null);
				tsmi.Enabled = false;
				return;
			}

			AccessKeyManagerEx ak = new AccessKeyManagerEx();
			GroupHandler gh = delegate(PwGroup pg)
			{
				string strText = ak.CreateText(pg.Name, true);
				strText = strText.PadLeft(((int)pg.GetDepth() << 2) + strText.Length);

				int nIconID = ((!pg.CustomIconUuid.Equals(PwUuid.Zero)) ?
					((int)PwIcon.Count + pd.GetCustomIconIndex(
					pg.CustomIconUuid)) : (int)pg.IconId);

				ToolStripMenuItem tsmi = dm.AddItem(strText,
					m_ilCurrentIcons.Images[nIconID], pg);

				if(pd.RecycleBinEnabled && pg.Uuid.Equals(pd.RecycleBinUuid) &&
					(m_fontItalicTree != null))
					tsmi.Font = m_fontItalicTree;

				return true;
			};

			gh(pgRoot);
			pgRoot.TraverseTree(TraversalMethod.PreOrder, gh, null);
		}

		private void OnEntryMoveToGroup(object sender, DynamicMenuEventArgs e)
		{
			PwGroup pgTo = (e.Tag as PwGroup);
			if(pgTo == null) { Debug.Assert(false); return; }

			m_pgActiveAtDragStart = GetSelectedGroup();
			MoveOrCopySelectedEntries(pgTo, DragDropEffects.Move);
		}

		private bool FixDuplicateUuids(PwDatabase pd, IOConnectionInfo ioc)
		{
			if(pd == null) { Debug.Assert(false); return false; }

			if(!pd.HasDuplicateUuids()) return false;

			string str = string.Empty;
			if(ioc != null)
			{
				string strFile = ioc.GetDisplayName();
				if(!string.IsNullOrEmpty(strFile))
					str += strFile + MessageService.NewParagraph;
			}

			str += KPRes.UuidDupInDb + MessageService.NewParagraph +
				KPRes.CorruptionByExt + MessageService.NewParagraph +
				KPRes.UuidFix;

			if(VistaTaskDialog.ShowMessageBoxEx(str, null,
				PwDefs.ShortProductName, VtdIcon.Warning, null,
				KPRes.RepairCmd, (int)DialogResult.Cancel, null, 0) < 0)
				MessageService.ShowWarning(str);

			pd.FixDuplicateUuids();
			pd.Modified = true;
			return true;
		}

		private void BlockMainTimer(bool bBlock)
		{
			if(bBlock)
			{
				m_timerMain.Enabled = false;
				++m_uMainTimerBlocked;
			}
			else
			{
				if(m_uMainTimerBlocked == 0) { Debug.Assert(false); return; }

				m_timerMain.Enabled = true;
				--m_uMainTimerBlocked;
			}
		}

		private void ShowSelectedEntryParentGroup()
		{
			PwEntry pe = GetSelectedEntry(false);
			if(pe == null) return;

			PwGroup pg = pe.ParentGroup;
			if(pg == null) { Debug.Assert(false); return; }

			UpdateUI(false, null, true, pg, true, null, false);

			TreeNode tn = m_tvGroups.SelectedNode;
			if(tn != null) tn.EnsureVisible();

			EnsureVisibleSelected(false);
		}

		private void PerformAutoType(string strSeq)
		{
			PwEntry pe = GetSelectedEntry(false);
			if(pe != null)
			{
				try
				{
					AutoType.PerformIntoPreviousWindow(this, pe,
						m_docMgr.SafeFindContainerOf(pe), strSeq);
				}
				catch(Exception ex) { MessageService.ShowWarning(ex); }
			}
		}

		private void OnEntryPerformAutoTypeAdv(object sender, DynamicMenuEventArgs e)
		{
			if(e == null) { Debug.Assert(false); return; }

			string strSeq = (e.Tag as string);
			if(string.IsNullOrEmpty(strSeq)) { Debug.Assert(false); return; }

			PerformAutoType(strSeq);
		}

		private int CreateAndShowEntryList(EntryReportDelegate f, string strStatus,
			Image imgIcon, string strTitle, string strSubTitle, string strNote,
			LvfFlags flAdd, LvfFlags flRemove, bool bForceDialog, bool bDisableSorting)
		{
			if(f == null) { Debug.Assert(false); return -1; }

			PwDatabase pd = m_docMgr.ActiveDatabase;
			if((pd == null) || !pd.IsOpen) { Debug.Assert(false); return -1; }

			Form fOptDialog;
			IStatusLogger sl = StatusUtil.CreateStatusDialog(this, out fOptDialog,
				null, (strStatus ?? "..."), true, false);
			UIBlockInteraction(true);

			Action<ListView> fInit = null;
			List<object> l = null;

			string strExcp = null;
			try { l = f(pd, sl, out fInit); }
			catch(Exception ex) { strExcp = ex.Message; }

			UIBlockInteraction(false);
			sl.EndLogging();

			if(!string.IsNullOrEmpty(strExcp))
				MessageService.ShowWarning(strExcp);

			if(l == null) return -1;
			if((l.Count == 0) && !bForceDialog) return 0;

			ListViewForm dlg = new ListViewForm();
			dlg.InitEx(strTitle, strSubTitle, strNote, imgIcon, l,
				m_ilCurrentIcons, fInit);
			dlg.FlagsEx = ((dlg.FlagsEx | flAdd) & ~flRemove);
			dlg.DatabaseEx = pd;
			UIUtil.ShowDialogAndDestroy(dlg, this);

			PwGroup pg = (dlg.ResultGroup as PwGroup);
			PwEntry pe = (dlg.ResultItem as PwEntry);
			if((pg == null) && (pe == null))
				pg = (dlg.ResultItem as PwGroup);

			if((pg != null) || (pe != null))
			{
				if(pg != null)
					UpdateUI(false, null, false, null, true, pg, false, m_lvEntries);
				else
					UpdateUI(false, null, true, pe.ParentGroup, true, null, false, m_lvEntries);

				if(bDisableSorting) SortPasswordList(false, 0, null, true);

				if(pe != null) SelectEntry(pe, true, true, true, false);
				else SelectFirstEntryIfNoneSelected();

				UpdateUIState(false); // For selected entry
			}

			return l.Count;
		}

		private void CopySelectedObjects(Type t, bool bEncrypt)
		{
			PwDatabase pd = m_docMgr.ActiveDatabase;
			if((pd == null) || !pd.IsOpen) { Debug.Assert(false); return; }

			if(!AppPolicy.Try(AppPolicyId.CopyWholeEntries)) return;

			try
			{
				if(t == typeof(PwGroup))
				{
					PwGroup pg = GetSelectedGroup();
					if(pg == null) { Debug.Assert(false); return; }

					EntryUtil.CopyGroupToClipboard(pd, pg, this.Handle, bEncrypt);
				}
				else if(t == typeof(PwEntry))
				{
					PwEntry[] v = GetSelectedEntries();
					if((v == null) || (v.Length == 0)) { Debug.Assert(false); return; }

					EntryUtil.CopyEntriesToClipboard(pd, v, this.Handle, bEncrypt);
				}
				else { Debug.Assert(false); return; }

				StartClipboardCountdown();
			}
			catch(Exception ex) { MessageService.ShowWarning(ex); }
		}

		private void SetSelectedEntryExpiry(bool bExpNow)
		{
			PwDatabase pd = m_docMgr.ActiveDatabase;
			if((pd == null) || !pd.IsOpen) { Debug.Assert(false); return; }

			PwEntry[] v = GetSelectedEntries();
			if((v == null) || (v.Length == 0)) return;

			DateTime dt = DateTime.UtcNow.AddSeconds(-1);
			bool bMod = false;

			foreach(PwEntry pe in v)
			{
				if(pe == null) { Debug.Assert(false); continue; }
				if(!bExpNow && !pe.Expires) continue;

				pe.CreateBackup(pd);

				pe.Expires = bExpNow;
				if(bExpNow) pe.ExpiryTime = dt;

				pe.Touch(true, false);
				bMod = true;
			}

			RefreshEntriesList();
			UpdateUIState(bMod);
		}

		internal void AddEntryEx(object sender, GFunc<PwEntry> fNewEntry,
			Action<PwEntry> fAddPre, Action<PwEntry> fAddPost)
		{
			PwDatabase pd = m_docMgr.ActiveDatabase;
			if((pd == null) || !pd.IsOpen) { Debug.Assert(false); return; }

			PwGroup pg = GetSelectedGroup();
			if((pg == null) || pg.IsVirtual)
			{
				MessageService.ShowWarning(KPRes.GroupCannotStoreEntries,
					KPRes.SelectDifferentGroup);
				return;
			}

			PwEntry pe;
			if(fNewEntry != null)
			{
				pe = fNewEntry();
				if(pe == null) { Debug.Assert(false); return; }
			}
			else
			{
				pe = new PwEntry(true, true);

				pe.IconId = PwDefs.GroupIconToEntryIcon(pg.IconId);
				pe.CustomIconUuid = pg.CustomIconUuid;
			}

			// Temporarily assume that the entry is in pg; required for retrieving
			// the default auto-type sequence and other things
			pe.ParentGroup = pg;

			PwGroup pgT = EntryTemplates.GetTemplatesGroup(pd);
			Debug.Assert(pe.ParentGroup != null);
			if((pgT == null) || !pe.IsContainedIn(pgT)) // No auto. for new template
			{
				if(pe.Strings.GetSafe(PwDefs.UserNameField).IsEmpty)
					pe.Strings.Set(PwDefs.UserNameField, new ProtectedString(
						pd.MemoryProtection.ProtectUserName, pd.DefaultUserName));

				if(pe.Strings.GetSafe(PwDefs.PasswordField).IsEmpty)
					PwGeneratorUtil.GenerateAuto(pe, pd);

				if(!pe.Expires)
				{
					int nExpireDays = Program.Config.Defaults.NewEntryExpiresInDays;
					if(nExpireDays >= 0)
					{
						pe.Expires = true;
						pe.ExpiryTime = DateTime.UtcNow.AddDays(nExpireDays);
					}
				}
			}

			if(fAddPre != null) fAddPre(pe);

			bool bSelectFullTitle = !pe.Strings.GetSafe(PwDefs.TitleField).IsEmpty;

			PwEntryForm pForm = new PwEntryForm();
			pForm.InitEx(pe, PwEditMode.AddNewEntry, pd, m_ilCurrentIcons,
				false, bSelectFullTitle);
			if(UIUtil.ShowDialogAndDestroy(pForm) == DialogResult.OK)
			{
				pg.AddEntry(pe, true);
				UpdateUI(false, null, pd.UINeedsIconUpdate, null, true,
					null, true, m_lvEntries);
				SelectEntry(pe, true, true, true, true);

				if(Program.Config.Application.AutoSaveAfterEntryEdit)
					SaveDatabase(pd, sender);

				if(fAddPost != null) fAddPost(pe);
			}
			else UpdateUI(false, null, pd.UINeedsIconUpdate, null,
				pd.UINeedsIconUpdate, null, false);
		}

#if DEBUG
		private void ConstructDebugMenu()
		{
			ToolStripMenuItem tsmiDebug = new ToolStripMenuItem("De&bug");
			m_menuTools.DropDownItems.Insert(m_menuTools.DropDownItems.IndexOf(
				m_menuToolsAdv) + 1, tsmiDebug);

			ToolStripMenuItem tsmi = new ToolStripMenuItem("&GC Collect",
				Properties.Resources.B16x16_Reload_Page);
			tsmi.Click += delegate(object sender, EventArgs e)
			{
				long cbBefore = GC.GetTotalMemory(false);
				GC.Collect();
				long cbAfter = GC.GetTotalMemory(true);
				MessageService.ShowInfo("Before:" + MessageService.NewLine +
					StrUtil.FormatDataSizeKB((ulong)cbBefore),
					"After:" + MessageService.NewLine +
					StrUtil.FormatDataSizeKB((ulong)cbAfter));
			};
			tsmiDebug.DropDownItems.Add(tsmi);

			tsmiDebug.DropDownItems.Add(new ToolStripSeparator());

			tsmi = new ToolStripMenuItem("Set Database &Custom Data Items (KDBX 4.1)",
				Properties.Resources.B16x16_BlockDevice);
			tsmi.Click += delegate(object sender, EventArgs e)
			{
				PwDatabase pd = m_docMgr.ActiveDatabase;
				if((pd == null) || !pd.IsOpen) return;

				Random r = Program.GlobalRandom;
				byte[] pb = new byte[12];
				Action<string> f = delegate(string str)
				{
					r.NextBytes(pb);
					pd.CustomData.Set("Test_" + str, Convert.ToBase64String(pb),
						DateTime.UtcNow.AddSeconds(-(r.Next() % (60 * 60 * 24 * 7))));
				};

				for(char ch = 'A'; ch < 'E'; ++ch) f(ch.ToString());

				UpdateUIState(true);
			};
			tsmiDebug.DropDownItems.Add(tsmi);
		}
#endif

		private static PwGroup GetPreviousParentGroupSafeI(IStructureItem it,
			PwGroup pgRoot)
		{
			if(it == null) { Debug.Assert(false); return null; }

			PwGroup pgParent = it.ParentGroup;
			if(pgParent == null) return null; // Cannot move root group

			PwUuid puPrev = it.PreviousParentGroup;
			if(puPrev.Equals(PwUuid.Zero)) return null;

			PwGroup pgPrev = pgRoot.FindGroup(puPrev, true);
			if(pgPrev == null) return null;
			if(pgPrev == pgParent) return null;

			return pgPrev;
		}

		private static PwGroup GetPreviousParentGroupSafe(PwGroup pg, PwGroup pgRoot)
		{
			PwGroup pgPrev = GetPreviousParentGroupSafeI(pg, pgRoot);
			if(pgPrev == null) return null;
			if(pgPrev == pg) { Debug.Assert(false); return null; } // Cannot move into itself
			if(pgPrev.IsContainedIn(pg)) return null;
			if(!pgPrev.CanAddGroup(pg)) return null;

			return pgPrev;
		}

		private static PwGroup GetPreviousParentGroupSafe(PwEntry pe, PwGroup pgRoot)
		{
			return GetPreviousParentGroupSafeI(pe, pgRoot);
		}

		private void GetPreviousParentGroupCmdInfo(PwGroup pg, PwEntry[] v,
			out string strCommand, out bool bEnabled, out bool bRecycleAtLeast1)
		{
			strCommand = KPRes.MoveToPreviousParentGroup;
			bEnabled = false;
			bRecycleAtLeast1 = false;

			PwDatabase pd = m_docMgr.ActiveDatabase;
			if((pd == null) || !pd.IsOpen) return;

			PwGroup pgRoot = pd.RootGroup;

			bool bRestore = true, bRecycle = false;
			PwGroup pgRecBin = null;
			if(pd.RecycleBinEnabled)
				pgRecBin = pgRoot.FindGroup(pd.RecycleBinUuid, true);

			GFunc<IStructureItem, PwGroup, bool> fCheck = delegate(
				IStructureItem it, PwGroup pgPrev)
			{
				if(it == null) { Debug.Assert(false); return false; }
				if(pgPrev == null) return false;
				Debug.Assert(pgPrev.Uuid.Equals(it.PreviousParentGroup));

				PwGroup pgParent = it.ParentGroup;
				if(pgParent == null) { Debug.Assert(false); return false; }

				bool bInRecNow = ((pgRecBin != null) && ((pgParent == pgRecBin) ||
					pgParent.IsContainedIn(pgRecBin)));
				bool bInRecThen = ((pgRecBin != null) && ((pgPrev == pgRecBin) ||
					pgPrev.IsContainedIn(pgRecBin)));

				bRestore &= (bInRecNow && !bInRecThen);
				bRecycle |= bInRecThen; // Move into/within

				return true;
			};

			if(pg != null)
			{
				PwGroup pgPrev = GetPreviousParentGroupSafe(pg, pgRoot);
				if(!fCheck(pg, pgPrev)) return;
			}
			else if((v != null) && (v.Length != 0)) // Empty => disable
			{
				foreach(PwEntry pe in v)
				{
					PwGroup pgPrev = GetPreviousParentGroupSafe(pe, pgRoot);
					if(!fCheck(pe, pgPrev)) return;
				}
			}
			else return;

			if(bRestore) strCommand += " (" + KPRes.Restore + ")";
			bEnabled = true;
			bRecycleAtLeast1 = bRecycle;
		}

		private void UpdateMoveToPreviousParentGroupUI(PwGroup pg, PwEntry[] v,
			ToolStripMenuItem tsmi)
		{
			if(tsmi == null) { Debug.Assert(false); return; }

			string strCommand;
			bool bEnabled, bRecycle;
			GetPreviousParentGroupCmdInfo(pg, v, out strCommand, out bEnabled,
				out bRecycle);

			tsmi.Text = strCommand;
			tsmi.Enabled = bEnabled;
		}

		private bool ConfirmMoveToPreviousParentGroup(PwGroup pg, PwEntry[] v)
		{
			string strCommand;
			bool bEnabled, bRecycle;
			GetPreviousParentGroupCmdInfo(pg, v, out strCommand, out bEnabled,
				out bRecycle);

			if(!bEnabled) { Debug.Assert(false); return false; }
			if(bRecycle)
			{
				string strMsg = KPRes.RecycleMoveInfo + MessageService.NewParagraph +
					KPRes.AskContinue;
				if(!MessageService.AskYesNo(strMsg, PwDefs.ShortProductName, false))
					return false;
			}

			return true;
		}

		private void MoveToPreviousParentGroup(bool bEntry)
		{
			PwDatabase pd = m_docMgr.ActiveDatabase;
			if((pd == null) || !pd.IsOpen) { Debug.Assert(false); return; }

			PwGroup pgRoot = pd.RootGroup;

			if(bEntry)
			{
				PwEntry[] v = GetSelectedEntries();
				if((v == null) || (v.Length == 0)) { Debug.Assert(false); return; }

				if(!ConfirmMoveToPreviousParentGroup(null, v)) return;

				foreach(PwEntry pe in v)
				{
					PwGroup pgPrev = GetPreviousParentGroupSafe(pe, pgRoot);
					if(pgPrev == null) { Debug.Assert(false); continue; }

					PwGroup pgParent = pe.ParentGroup;
					if(!pgParent.Entries.Remove(pe)) { Debug.Assert(false); continue; }

					pgPrev.AddEntry(pe, true, true);
					pe.PreviousParentGroup = pgParent.Uuid;
				}
			}
			else // Group
			{
				PwGroup pg = GetSelectedGroup();

				if(!ConfirmMoveToPreviousParentGroup(pg, null)) return;

				PwGroup pgPrev = GetPreviousParentGroupSafe(pg, pgRoot);
				if(pgPrev == null) { Debug.Assert(false); return; }

				PwGroup pgParent = pg.ParentGroup;
				if(!pgParent.Groups.Remove(pg)) { Debug.Assert(false); return; }

				pgPrev.AddGroup(pg, true, true);
				pg.PreviousParentGroup = pgParent.Uuid;
			}

			UpdateUI(false, null, !bEntry, null, true, null, true);
		}

		private void RestoreWindowEx()
		{
			UIUtil.SetWindowState(this, (Program.Config.MainWindow.Maximized ?
				FormWindowState.Maximized : FormWindowState.Normal));
		}

		private PwEntry GetEntryMarkedForComparison(out PwGroup pg, out PwDatabase pd)
		{
			pg = null;
			pd = null;

			PwEntry pe = m_peMarkedForComparison;
			if(pe == null) return null;

			pg = pe.ParentGroup;
			if((pg == null) || (pg.Entries.IndexOf(pe) < 0)) return null;

			pd = m_docMgr.FindContainerOf(pe);
			if(pd == null) return null;

			return pe;
		}
	}
}
