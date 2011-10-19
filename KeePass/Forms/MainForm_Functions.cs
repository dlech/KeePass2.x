/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2011 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Text;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

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
using KeePassLib.Cryptography;
using KeePassLib.Delegates;
using KeePassLib.Interfaces;
using KeePassLib.Keys;
using KeePassLib.Utility;
using KeePassLib.Security;
using KeePassLib.Serialization;

namespace KeePass.Forms
{
	public partial class MainForm : Form
	{
		private DocumentManagerEx m_docMgr = new DocumentManagerEx();

		private ListViewGroup m_lvgLastEntryGroup = null;
		private bool m_bEntryGrouping = false;
		private DateTime m_dtCachedNow = DateTime.Now;
		private bool m_bOnlyTans = false;
		private Font m_fontExpired = null;
		private Font m_fontBoldUI = null;
		private Font m_fontBoldTree = null;
		private Font m_fontItalicTree = null;
		private Color m_clrAlternateItemBgColor = Color.Empty;
		private Point m_ptLastEntriesMouseClick = new Point(0, 0);
		private RichTextBoxContextMenu m_ctxEntryPreviewContextMenu = new RichTextBoxContextMenu();
		private DynamicMenu m_dynCustomStrings;
		private DynamicMenu m_dynCustomBinaries;
		private DynamicMenu m_dynShowEntriesByTagsEditMenu;
		private DynamicMenu m_dynShowEntriesByTagsToolBar;
		private DynamicMenu m_dynAddTag;
		private DynamicMenu m_dynRemoveTag;
		private OpenWithMenu m_dynOpenUrl;

		private AsyncPwListUpdate m_asyncListUpdate = null;

		private MruList m_mruList = new MruList();

		private SessionLockNotifier m_sessionLockNotifier = new SessionLockNotifier();

		private DefaultPluginHost m_pluginDefaultHost = new DefaultPluginHost();
		private PluginManager m_pluginManager = new PluginManager();

		private object m_objLockTimerSync = new object();
		private int m_nLockTimerMax = 0;
		// private volatile int m_nLockTimerCur = 0;
		private long m_lLockAtTicks = long.MaxValue;
		private uint m_uLastInputTime = uint.MaxValue;
		private long m_lLockAtGlobalTicks = long.MaxValue;

		private bool m_bBlockQuickFind = false;
		private object m_objQuickFindSync = new object();
		private int m_iLastQuickFindTicks = Environment.TickCount - 1500;
		private string m_strLastQuickSearch = string.Empty;

		private ToolStripSeparator m_tsSepCustomToolBar = null;
		private List<ToolStripButton> m_vCustomToolBarButtons = new List<ToolStripButton>();

		private int m_nClipClearMax = 0;
		private int m_nClipClearCur = -1;

		private string m_strNeverExpiresText = string.Empty;

		private bool m_bSimpleTanView = true;
		private bool m_bShowTanIndices = true;

		private Image m_imgFileSaveEnabled = null;
		private Image m_imgFileSaveDisabled = null;
		// private Image m_imgFileSaveAllEnabled = null;
		// private Image m_imgFileSaveAllDisabled = null;
		private ImageList m_ilCurrentIcons = null;

		private KeyValuePair<Color, Icon> m_kvpIcoMain =
			new KeyValuePair<Color, Icon>(Color.Empty, null);
		private KeyValuePair<Color, Icon> m_kvpIcoTrayNormal =
			new KeyValuePair<Color, Icon>(Color.Empty, null);
		private KeyValuePair<Color, Icon> m_kvpIcoTrayLocked =
			new KeyValuePair<Color, Icon>(Color.Empty, null);

		private List<Image> m_lTabImages = new List<Image>();
		private ImageList m_ilTabImages = null;

		private bool m_bIsAutoTyping = false;
		private bool m_bBlockTabChanged = false;
		private bool m_bForceSave = false;
		private volatile uint m_uUIBlocked = 0;
		private uint m_uUnlockAutoBlocked = 0;
		private MouseButtons m_mbLastTrayMouseButtons = MouseButtons.None;

		private bool m_bUpdateUIStateOnce = false;
		private int m_nLastSelChUpdateUIStateTicks = 0;

		private readonly int m_nAppMessage = Program.ApplicationMessage;
		private readonly int m_nTaskbarButtonMessage;
		private bool m_bTaskbarButtonMessage;

		private List<KeyValuePair<ToolStripItem, ToolStripItem>> m_vLinkedToolStripItems =
			new List<KeyValuePair<ToolStripItem, ToolStripItem>>();

		private FormWindowState m_fwsLast = FormWindowState.Normal;
		private PwGroup m_pgActiveAtDragStart = null;

		private Stack<Form> m_vRedirectActivation = new Stack<Form>();

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

		/// <summary>
		/// Get a reference to the plugin host. This should not be used by plugins.
		/// Plugins should directly use the reference they get in the initialization
		/// function.
		/// </summary>
		public IPluginHost PluginHost
		{
			get { return m_pluginDefaultHost; }
		}

		internal PluginManager PluginManager { get { return m_pluginManager; } }

		private sealed class EditableBinaryAttachment
		{
			private string m_strName;
			public string Name { get { return m_strName; } }

			public EditableBinaryAttachment(string strName)
			{
				m_strName = strName;
			}
		}

		private struct MainAppState
		{
			public bool FileLocked;
			public bool DatabaseOpened;
			public int EntriesCount;
			public int EntriesSelected;
			public bool EnableLockCmd;
			public bool NoWindowShown;
			public PwEntry SelectedEntry;
			public bool CanCopyUserName;
			public bool CanCopyPassword;
			public bool IsOneTan;
			public string LockUnlock;
		}

		private enum AppCommandType
		{
			Window = 0,
			Lock = 1
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

		private void CleanUpEx()
		{
			m_asyncListUpdate.CancelPendingUpdatesAsync();

			Program.TriggerSystem.RaiseEvent(EcasEventIDs.AppExit);

			m_nClipClearCur = -1;
			if(Program.Config.Security.ClipboardClearOnExit)
				ClipboardUtil.ClearIfOwner();

			m_pluginManager.UnloadAllPlugins(); // Before SaveConfig

			// Just unregister the events; no need to remove the buttons
			foreach(ToolStripButton tbCustom in m_vCustomToolBarButtons)
				tbCustom.Click -= OnCustomToolBarButtonClicked;
			m_vCustomToolBarButtons.Clear();

			SaveConfig();

			m_sessionLockNotifier.Uninstall();
			HotKeyManager.UnregisterAll();

			EntryTemplates.Release();

			m_dynCustomBinaries.MenuClick -= this.OnEntryBinaryView;
			m_dynCustomStrings.MenuClick -= this.OnCopyCustomString;
			m_dynShowEntriesByTagsEditMenu.MenuClick -= this.OnShowEntriesByTag;
			m_dynShowEntriesByTagsToolBar.MenuClick -= this.OnShowEntriesByTag;
			m_dynAddTag.MenuClick -= this.OnAddEntryTag;
			m_dynRemoveTag.MenuClick -= this.OnRemoveEntryTag;
			m_dynOpenUrl.Destroy();

			m_ctxEntryPreviewContextMenu.Detach();

			m_lvsmMenu.Release();

			m_ntfTray.Visible = false;

			Debug.Assert(m_vRedirectActivation.Count == 0);
			Debug.Assert(m_uUIBlocked == 0);
			Debug.Assert(m_uUnlockAutoBlocked == 0);
			this.Visible = false;

			if(m_fontBoldUI != null) { m_fontBoldUI.Dispose(); m_fontBoldUI = null; }
			if(m_fontBoldTree != null) { m_fontBoldTree.Dispose(); m_fontBoldTree = null; }
			if(m_fontItalicTree != null) { m_fontItalicTree.Dispose(); m_fontItalicTree = null; }

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

			AppConfigSerializer.Save(Program.Config);
		}

		private void SaveWindowPositionAndSize()
		{
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
					(float)m_splitHorizontal.SplitterDistance /
					(float)m_splitHorizontal.Height;
				Program.Config.MainWindow.SplitterVerticalFrac =
					(float)m_splitVertical.SplitterDistance /
					(float)m_splitVertical.Width;
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
		/// <param name="bSetModified">If this parameter is <c>true</c>, the currently
		/// opened database is marked as modified.</param>
		private void UpdateUIState(bool bSetModified)
		{
			UpdateUIState(bSetModified, null);
		}

		private void UpdateUIState(bool bSetModified, Control cOptFocus)
		{
			NotifyUserActivity();
			m_bUpdateUIStateOnce = false; // We do it now

			MainAppState s = GetMainAppState();

			if(s.DatabaseOpened && bSetModified)
				m_docMgr.ActiveDatabase.Modified = true;

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

			m_lvEntries.Enabled = s.DatabaseOpened;

			m_statusPartSelected.Text = s.EntriesSelected.ToString() +
				" " + KPRes.OfLower + " " + s.EntriesCount.ToString() +
				" " + KPRes.SelectedLower;
			SetStatusEx(null);

			string strWindowText = string.Empty;
			string strNtfText = string.Empty;

			if(s.FileLocked)
			{
				IOConnectionInfo iocLck = m_docMgr.ActiveDocument.LockedIoc;

				strWindowText = iocLck.Path + @" [" + KPRes.Locked +
					@"] - " + PwDefs.ProductName;

				string strNtfPre = PwDefs.ShortProductName + " - " +
					KPRes.WorkspaceLocked + MessageService.NewLine;

				strNtfText = strNtfPre + WinUtil.CompactPath(iocLck.Path, 63 -
					strNtfPre.Length);

				Icon icoDisposable, icoAssignable;
				CreateColorizedIcon(Properties.Resources.QuadLocked, false,
					ref m_kvpIcoTrayLocked, out icoAssignable, out icoDisposable);
				m_ntfTray.Icon = icoAssignable;
				if(icoDisposable != null) icoDisposable.Dispose();

				TaskbarList.SetOverlayIcon(this, Properties.Resources.LockOverlay,
					KPRes.Locked);
				NativeMethods.EnableWindowPeekPreview(this.Handle, false);
			}
			else if(s.DatabaseOpened == false)
			{
				strWindowText = PwDefs.ProductName;
				strNtfText = strWindowText;

				Icon icoDisposable, icoAssignable;
				CreateColorizedIcon(Properties.Resources.QuadNormal, false,
					ref m_kvpIcoTrayNormal, out icoAssignable, out icoDisposable);
				m_ntfTray.Icon = icoAssignable;
				if(icoDisposable != null) icoDisposable.Dispose();

				TaskbarList.SetOverlayIcon(this, null, string.Empty);
				NativeMethods.EnableWindowPeekPreview(this.Handle, true);
			}
			else // Database open and not locked
			{
				string strFileDesc;
				if(Program.Config.MainWindow.ShowFullPathInTitle)
					strFileDesc = m_docMgr.ActiveDatabase.IOConnectionInfo.GetDisplayName();
				else
					strFileDesc = UrlUtil.GetFileName(
						m_docMgr.ActiveDatabase.IOConnectionInfo.Path);

				strFileDesc = WinUtil.CompactPath(strFileDesc, 63 -
					PwDefs.ProductName.Length - 4);

				if(m_docMgr.ActiveDatabase.Modified)
					strWindowText = strFileDesc + "* - " + PwDefs.ProductName;
				else
					strWindowText = strFileDesc + " - " + PwDefs.ProductName;

				string strNtfPre = PwDefs.ProductName + MessageService.NewLine;
				strNtfText = strNtfPre + WinUtil.CompactPath(
					m_docMgr.ActiveDatabase.IOConnectionInfo.Path, 63 - strNtfPre.Length);

				Icon icoDisposable, icoAssignable;
				CreateColorizedIcon(Properties.Resources.QuadNormal, false,
					ref m_kvpIcoTrayNormal, out icoAssignable, out icoDisposable);
				m_ntfTray.Icon = icoAssignable;
				if(icoDisposable != null) icoDisposable.Dispose();

				TaskbarList.SetOverlayIcon(this, null, string.Empty);
				NativeMethods.EnableWindowPeekPreview(this.Handle, true);
			}

			// Clip the strings again (it could be that a translator used
			// a string in KPRes that is too long to be displayed)
			this.Text = StrUtil.CompactString3Dots(strWindowText, 63);
			m_ntfTray.Text = StrUtil.CompactString3Dots(strNtfText, 63);

			Icon icoToDispose, icoToAssign;
			if(CreateColorizedIcon(Properties.Resources.KeePass, true,
				ref m_kvpIcoMain, out icoToAssign, out icoToDispose))
				this.Icon = icoToAssign;
			if(icoToDispose != null) icoToDispose.Dispose();

			// Main menu
			m_menuFileSaveAs.Enabled = m_menuFileSaveAsLocal.Enabled =
				m_menuFileSaveAsUrl.Enabled = m_menuFileDbSettings.Enabled =
				m_menuFileChangeMasterKey.Enabled = m_menuFilePrint.Enabled =
				s.DatabaseOpened;

			m_menuFileClose.Enabled = (s.DatabaseOpened || s.FileLocked);

			m_menuFileLock.Enabled = m_tbLockWorkspace.Enabled = s.EnableLockCmd;

			m_menuEditFind.Enabled = m_menuToolsGeneratePwList.Enabled =
				m_menuToolsTanWizard.Enabled =
				m_menuEditShowAllEntries.Enabled = m_menuEditShowExpired.Enabled =
				m_menuEditShowByTag.Enabled = s.DatabaseOpened;
			m_menuToolsDbMaintenance.Enabled = m_menuToolsDbDelDupEntries.Enabled =
				m_menuToolsDbDelEmptyGroups.Enabled = m_menuToolsDbDelUnusedIcons.Enabled =
				s.DatabaseOpened;

			m_menuFileImport.Enabled = m_menuFileExport.Enabled = s.DatabaseOpened;

			m_menuFileSync.Enabled = m_menuFileSyncFile.Enabled =
				m_menuFileSyncUrl.Enabled = (s.DatabaseOpened &&
				m_docMgr.ActiveDatabase.IOConnectionInfo.IsLocalFile() &&
				(m_docMgr.ActiveDatabase.IOConnectionInfo.Path.Length > 0));

			m_tbFind.Enabled = m_tbViewsShowAll.Enabled = m_tbViewsShowExpired.Enabled =
				s.DatabaseOpened;
			m_tbEntryViewsDropDown.Enabled = s.DatabaseOpened;

			if(Program.Config.MainWindow.DisableSaveIfNotModified)
			{
				m_tbSaveDatabase.Image = m_imgFileSaveEnabled;
				m_menuFileSave.Enabled = m_tbSaveDatabase.Enabled = (s.DatabaseOpened &&
					m_docMgr.ActiveDatabase.Modified);
			}
			else
			{
				m_tbSaveDatabase.Image = (m_docMgr.ActiveDatabase.Modified ?
					m_imgFileSaveEnabled : m_imgFileSaveDisabled);
				m_menuFileSave.Enabled = m_tbSaveDatabase.Enabled = s.DatabaseOpened;
			}

			m_tbQuickFind.Enabled = s.DatabaseOpened;

			m_tbAddEntry.Enabled = s.DatabaseOpened;

			PwEntry pe = s.SelectedEntry;
			ShowEntryDetails(pe);

			m_tbCopyUserName.Enabled = s.CanCopyUserName;
			m_tbCopyPassword.Enabled = s.CanCopyPassword;

			m_menuFileLock.Text = s.LockUnlock;
			m_tbLockWorkspace.Text = m_tbLockWorkspace.ToolTipText =
				m_ctxTrayLock.Text = StrUtil.RemoveAccelerator(s.LockUnlock);

			m_tabMain.Visible = m_tbSaveAll.Visible = m_tbCloseTab.Visible =
				(m_docMgr.DocumentCount > 1);
			m_tbCloseTab.Enabled = (s.DatabaseOpened || s.FileLocked);

			bool bAtLeastOneModified = false;
			foreach(TabPage tabPage in m_tabMain.TabPages)
			{
				PwDocument dsPage = (PwDocument)tabPage.Tag;
				bAtLeastOneModified |= dsPage.Database.Modified;

				string strTabText = tabPage.Text;
				if(dsPage.Database.Modified && !strTabText.EndsWith("*"))
					tabPage.Text += "*";
				else if(!dsPage.Database.Modified && strTabText.EndsWith("*"))
					tabPage.Text = strTabText.Substring(0, strTabText.Length - 1);
			}

			// m_tbSaveAll.Image = (bAtLeastOneModified ? m_imgFileSaveAllEnabled :
			//	m_imgFileSaveAllDisabled);
			m_tbSaveAll.Enabled = bAtLeastOneModified;

			UpdateUITabs();
			UpdateLinkedMenuItems();

			if(cOptFocus != null) ResetDefaultFocus(cOptFocus);

			if(this.UIStateUpdated != null) this.UIStateUpdated(this, EventArgs.Empty);
			Program.TriggerSystem.RaiseEvent(EcasEventIDs.UpdatedUIState);
		}

		private MainAppState UpdateUIGroupCtxState(MainAppState? stOpt)
		{
			MainAppState s = (stOpt.HasValue ? stOpt.Value : GetMainAppState());

			PwGroup pg = GetSelectedGroup();
			PwGroup pgParent = ((pg != null) ? pg.ParentGroup : null);
			PwDatabase pd = m_docMgr.ActiveDatabase;
			if((pd != null) && !pd.IsOpen) pd = null; // Null if closed
			PwGroup pgRoot = ((pd != null) ? pd.RootGroup : null);

			bool bChildOps = (s.DatabaseOpened && (pg != pgRoot));
			bool bMoveOps = (bChildOps && (pgParent != null) &&
				(pgParent.Groups.UCount > 1));
			uint uSubGroups = 0, uSubEntries = 0;
			if(pg != null) pg.GetCounts(true, out uSubGroups, out uSubEntries);

			m_ctxGroupAdd.Enabled = m_ctxGroupEdit.Enabled = m_ctxGroupFind.Enabled =
				m_ctxGroupPrint.Enabled = s.DatabaseOpened;

			m_ctxGroupDelete.Enabled = bChildOps;

			m_ctxGroupMoveToTop.Enabled = m_ctxGroupMoveOneUp.Enabled =
				(bMoveOps && (pgParent.Groups.IndexOf(pg) >= 1));
			m_ctxGroupMoveOneDown.Enabled = m_ctxGroupMoveToBottom.Enabled =
				(bMoveOps && (pgParent.Groups.IndexOf(pg) < ((int)pgParent.Groups.UCount - 1)));

			m_ctxGroupSort.Enabled = ((pg != null) && (pg.Groups.UCount > 1));
			m_ctxGroupSortRec.Enabled = (uSubGroups > 1);

			bool bShowEmpty = false, bEnableEmpty = false;
			if((pd != null) && pd.RecycleBinEnabled)
			{
				PwGroup pgRecycleBin = pd.RootGroup.FindGroup(pd.RecycleBinUuid, true);
				bShowEmpty = ((pgRecycleBin != null) && (pg == pgRecycleBin));
				if(bShowEmpty)
					bEnableEmpty = ((pg.Groups.UCount > 0) || (pg.Entries.UCount > 0));
			}
			m_ctxGroupEmpty.Enabled = bEnableEmpty;
			m_ctxGroupEmpty.Visible = bShowEmpty;

			return s;
		}

		private MainAppState UpdateUIEntryCtxState(MainAppState? stOpt)
		{
			MainAppState s = (stOpt.HasValue ? stOpt.Value : GetMainAppState());

			m_ctxEntryAdd.Enabled = s.DatabaseOpened;
			m_ctxEntryEdit.Enabled = (s.EntriesSelected == 1);
			m_ctxEntryDelete.Enabled = (s.EntriesSelected > 0);

			m_ctxEntryDuplicate.Enabled = m_ctxEntryMassSetIcon.Enabled =
				m_ctxEntrySelectedPrint.Enabled = m_ctxEntrySelectedExport.Enabled =
				(s.EntriesSelected > 0);

			m_ctxEntryMoveToTop.Enabled = m_ctxEntryMoveToBottom.Enabled =
				((m_pListSorter.Column < 0) && (s.EntriesSelected > 0));
			m_ctxEntryMoveOneDown.Enabled = m_ctxEntryMoveOneUp.Enabled =
				((m_pListSorter.Column < 0) && (s.EntriesSelected == 1));

			m_ctxEntrySelectAll.Enabled = (s.DatabaseOpened && (s.EntriesCount > 0));

			m_ctxEntryClipCopy.Enabled = (s.EntriesSelected > 0);
			// For the 'Paste' command, see the menu drop-down opening handler

			m_ctxEntryColorStandard.Enabled = m_ctxEntryColorLightRed.Enabled =
				m_ctxEntryColorLightGreen.Enabled = m_ctxEntryColorLightBlue.Enabled =
				m_ctxEntryColorLightYellow.Enabled = m_ctxEntryColorCustom.Enabled =
				(s.EntriesSelected > 0);
			m_ctxEntrySelectedNewTag.Enabled = (s.EntriesSelected > 0);

			PwEntry pe = s.SelectedEntry;

			m_ctxEntryCopyUserName.Enabled = s.CanCopyUserName;
			m_ctxEntryCopyPassword.Enabled = s.CanCopyPassword;
			m_ctxEntryUrlOpenInInternal.Enabled = ((s.EntriesSelected == 1) &&
				(pe != null) && !pe.Strings.GetSafe(PwDefs.UrlField).IsEmpty);
			m_ctxEntryOpenUrl.Enabled = m_ctxEntryCopyUrl.Enabled =
				((s.EntriesSelected > 1) || ((pe != null) &&
				!pe.Strings.GetSafe(PwDefs.UrlField).IsEmpty));

			if(pe != null)
			{
				m_ctxEntryPerformAutoType.Enabled = (pe.GetAutoTypeEnabled() &&
					(s.EntriesSelected == 1));
				m_ctxEntrySaveAttachedFiles.Enabled = ((pe.Binaries.UCount > 0) ||
					(s.EntriesSelected >= 2));
			}
			else // pe == null
			{
				m_ctxEntryPerformAutoType.Enabled = false;
				m_ctxEntrySaveAttachedFiles.Enabled = false;
			}

			m_ctxEntrySaveAttachedFiles.Visible = m_ctxEntrySaveAttachedFiles.Enabled;

			m_ctxEntryCopyUserName.Visible = !s.IsOneTan;
			m_ctxEntryUrl.Visible = !s.IsOneTan;
			m_ctxEntryCopyPassword.Text = (s.IsOneTan ? KPRes.CopyTanMenu :
				KPRes.CopyPasswordMenu);

			return s;
		}

		private MainAppState GetMainAppState()
		{
			MainAppState s = new MainAppState();
			s.FileLocked = IsFileLocked(null);
			s.DatabaseOpened = m_docMgr.ActiveDatabase.IsOpen;
			s.EntriesCount = (s.DatabaseOpened ? m_lvEntries.Items.Count : 0);
			s.EntriesSelected = (s.DatabaseOpened ? m_lvEntries.SelectedIndices.Count : 0);
			s.EnableLockCmd = (s.DatabaseOpened || s.FileLocked);
			s.NoWindowShown = (GlobalWindowManager.WindowCount == 0);
			s.SelectedEntry = GetSelectedEntry(true);
			s.CanCopyUserName = ((s.EntriesSelected == 1) && (s.SelectedEntry != null) &&
				!s.SelectedEntry.Strings.GetSafe(PwDefs.UserNameField).IsEmpty);
			s.CanCopyPassword = ((s.EntriesSelected == 1) && (s.SelectedEntry != null) &&
				!s.SelectedEntry.Strings.GetSafe(PwDefs.PasswordField).IsEmpty);

			s.IsOneTan = (s.EntriesSelected == 1);
			if(s.SelectedEntry != null)
				s.IsOneTan &= PwDefs.IsTanEntry(s.SelectedEntry);
			else s.IsOneTan = false;

			s.LockUnlock = (s.FileLocked ? KPRes.LockMenuUnlock : KPRes.LockMenuLock);

			return s;
		}

		/// <summary>
		/// Set the main status bar text.
		/// </summary>
		/// <param name="strStatusText">New status bar text.</param>
		public void SetStatusEx(string strStatusText)
		{
			if(strStatusText == null) m_statusPartInfo.Text = KPRes.Ready;
			else m_statusPartInfo.Text = strStatusText;
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
			if((coll == null) || (coll.Count == 0)) return null;

			PwEntry[] vSelected = new PwEntry[coll.Count];
			for(int i = 0; i < coll.Count; ++i)
				vSelected[i] = ((PwListItem)coll[i].Tag).Entry;

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
		/// Get the currently selected group. The selected <c>TreeNode</c> is
		/// automatically translated to a <c>PwGroup</c>.
		/// </summary>
		/// <returns>Selected <c>PwGroup</c>.</returns>
		public PwGroup GetSelectedGroup()
		{
			if(!m_docMgr.ActiveDatabase.IsOpen) return null;

			TreeNode tn = m_tvGroups.SelectedNode;
			if(tn == null) return null;
			return (tn.Tag as PwGroup);
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

				if(pe.CustomIconUuid.EqualsValue(PwUuid.Zero))
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
							pgContainer.GetFullPath());
						m_lvgLastEntryGroup.Tag = pgContainer;

						m_lvEntries.Groups.Add(m_lvgLastEntryGroup);
					}

					lvi.Group = m_lvgLastEntryGroup;
				}
			}

			if(!pe.ForegroundColor.IsEmpty)
				lvi.ForeColor = pe.ForegroundColor;
			else if(lviTarget != null) lvi.ForeColor = m_lvEntries.ForeColor;
			else { Debug.Assert(UIUtil.ColorsEqual(lvi.ForeColor, m_lvEntries.ForeColor)); }

			if(!pe.BackgroundColor.IsEmpty)
				lvi.BackColor = pe.BackgroundColor;
			// else if(Program.Config.MainWindow.EntryListAlternatingBgColors &&
			//	((m_lvEntries.Items.Count & 1) == 1))
			//	lvi.BackColor = m_clrAlternateItemBgColor;
			else if(lviTarget != null) lvi.BackColor = m_lvEntries.BackColor;
			else { Debug.Assert(UIUtil.ColorsEqual(lvi.BackColor, m_lvEntries.BackColor)); }

			bool bAsync;

			// m_bOnlyTans &= PwDefs.IsTanEntry(pe);
			if(m_bShowTanIndices && m_bOnlyTans)
			{
				string strIndex = pe.Strings.ReadSafe(PwDefs.TanIndexField);

				if(strIndex.Length > 0) lvi.Text = strIndex;
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

		private void AddEntriesToList(PwObjectList<PwEntry> vEntries)
		{
			if(vEntries == null) { Debug.Assert(false); return; }

			m_bEntryGrouping = m_lvEntries.ShowGroups;

			ListViewStateEx lvseCachedState = new ListViewStateEx(m_lvEntries);
			foreach(PwEntry pe in vEntries)
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

			Debug.Assert(lvseCachedState.CompareTo(m_lvEntries));

			UIUtil.SetAlternatingBgColors(m_lvEntries, m_clrAlternateItemBgColor,
				Program.Config.MainWindow.EntryListAlternatingBgColors);
		}

		/// <summary>
		/// Update the group list. This function completely rebuilds the groups
		/// view. You must call this function after you made any changes to the
		/// groups structure of the currently opened database.
		/// </summary>
		/// <param name="pgNewSelected">If this parameter is <c>null</c>, the
		/// previously selected group is selected again (after the list was
		/// rebuilt). If this parameter is non-<c>null</c>, the specified
		/// <c>PwGroup</c> is selected after the function returns.</param>
		private void UpdateGroupList(PwGroup pgNewSelected)
		{
			NotifyUserActivity();

			PwDatabase pwDb = m_docMgr.ActiveDatabase;
			PwGroup pg = (pgNewSelected ?? GetSelectedGroup());

			UpdateImageLists();

			m_tvGroups.BeginUpdate();
			m_tvGroups.Nodes.Clear();

			m_dtCachedNow = DateTime.Now;

			TreeNode tnRoot = null;
			if(pwDb.RootGroup != null)
			{
				int nIconID = ((!pwDb.RootGroup.CustomIconUuid.EqualsValue(PwUuid.Zero)) ?
					((int)PwIcon.Count + pwDb.GetCustomIconIndex(
					pwDb.RootGroup.CustomIconUuid)) : (int)pwDb.RootGroup.IconId);
				if(pwDb.RootGroup.Expires && (pwDb.RootGroup.ExpiryTime <= m_dtCachedNow))
					nIconID = (int)PwIcon.Expired;

				tnRoot = new TreeNode(pwDb.RootGroup.Name, // + GetGroupSuffixText(pwDb.RootGroup),
					nIconID, nIconID);

				tnRoot.Tag = pwDb.RootGroup;
				if(m_fontBoldTree != null) tnRoot.NodeFont = m_fontBoldTree;
				UIUtil.SetGroupNodeToolTip(tnRoot, pwDb.RootGroup);

				m_tvGroups.Nodes.Add(tnRoot);
			}

			TreeNode tnSelected = null;
			RecursiveAddGroup(tnRoot, pwDb.RootGroup, pg, ref tnSelected);

			if(tnRoot != null) tnRoot.Expand();

			m_tvGroups.EndUpdate();

			if(tnSelected != null)
			{
				// Ensure all parent tree nodes are expanded
				List<TreeNode> lParents = new List<TreeNode>();
				TreeNode tnUp = tnSelected;
				while(true)
				{
					tnUp = tnUp.Parent;
					if(tnUp == null) break;
					lParents.Add(tnUp);
				}
				for(int i = (lParents.Count - 1); i >= 0; --i)
					lParents[i].Expand();

				m_tvGroups.SelectedNode = tnSelected;
			}
			else if(m_tvGroups.Nodes.Count > 0)
				m_tvGroups.SelectedNode = m_tvGroups.Nodes[0];
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

			UpdateImageLists();

			PwEntry peTop = GetTopEntry(), peFocused = GetSelectedEntry(false);
			PwEntry[] vSelected = GetSelectedEntries();
			int iScrollY = NativeMethods.GetScrollPosY(m_lvEntries.Handle);

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

			m_lvEntries.BeginUpdate();
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
				if(bOnlyUpdateCurrentlyShown && !m_lvEntries.ShowGroups &&
					EntryUtil.EntriesHaveSameParent(pwlSource))
				{
					// Just reorder, don't enable grouping
					EntryUtil.ReorderEntriesAsInDatabase(pwlSource,
						m_docMgr.ActiveDatabase);
					peTop = null; // Don't scroll to previous top item
				}
				else m_bEntryGrouping |= pg.IsVirtual;
			}
			m_lvEntries.ShowGroups = m_bEntryGrouping;

			int nTopIndex = -1;
			ListViewItem lviFocused = null;

			m_dtCachedNow = DateTime.Now;
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

					if(pe == peTop) nTopIndex = m_lvEntries.Items.Count - 1;
					if(pe == peFocused) lviFocused = lvi;
				}
			}

			Debug.Assert(lvseCachedState.CompareTo(m_lvEntries));

			UIUtil.SetAlternatingBgColors(m_lvEntries, m_clrAlternateItemBgColor,
				Program.Config.MainWindow.EntryListAlternatingBgColors);

			Debug.Assert(m_bEntryGrouping == m_lvEntries.ShowGroups);
			if(m_lvEntries.ShowGroups)
			{
				// Test nTopIndex to ensure we're not scrolling an unrelated list
				if((nTopIndex >= 0) && (iScrollY > 0))
					NativeMethods.Scroll(m_lvEntries, 0, iScrollY);
			}
			else
			{
				if(nTopIndex >= 0)
				{
					m_lvEntries.EnsureVisible(m_lvEntries.Items.Count - 1);
					m_lvEntries.EnsureVisible(nTopIndex);
				}
			}

			if(lviFocused != null)
			{
				try { m_lvEntries.FocusedItem = lviFocused; } // .NET
				catch(Exception)
				{
					try { lviFocused.Focused = true; } // Mono
					catch(Exception) { Debug.Assert(false); }
				}
			}

			View view = m_lvEntries.View;
			if(m_bSimpleTanView)
			{
				if(m_lvEntries.Items.Count == 0)
					m_lvEntries.View = View.Details;
				else if(m_bOnlyTans && (view != View.List))
				{
					// SortPasswordList(false, 0, false);
					m_lvEntries.View = View.List;
				}
				else if(!m_bOnlyTans && (view != View.Details))
					m_lvEntries.View = View.Details;
			}
			else // m_bSimpleTanView == false
			{
				if(view != View.Details)
					m_lvEntries.View = View.Details;
			}

			m_lvEntries.EndUpdate();

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
			UpdateImageLists(); // Important

			m_lvEntries.BeginUpdate();
			m_dtCachedNow = DateTime.Now;

			foreach(ListViewItem lvi in m_lvEntries.Items)
			{
				SetListEntry(((PwListItem)lvi.Tag).Entry, lvi);
			}

			UIUtil.SetAlternatingBgColors(m_lvEntries, m_clrAlternateItemBgColor,
				Program.Config.MainWindow.EntryListAlternatingBgColors);
			m_lvEntries.EndUpdate();
		}

		private PwEntry GetTopEntry()
		{
			if(m_lvEntries.Items.Count == 0) return null;

			PwEntry peTop = null;
			try
			{
				ListViewItem lviTop = m_lvEntries.TopItem;
				if(lviTop != null) peTop = ((PwListItem)lviTop.Tag).Entry;
			}
			catch(Exception) { }

			return peTop;
		}

		private void RecursiveAddGroup(TreeNode tnParent, PwGroup pgContainer,
			PwGroup pgFind, ref TreeNode tnFound)
		{
			if(pgContainer == null) return;

			TreeNodeCollection tnc;
			if(tnParent == null) tnc = m_tvGroups.Nodes;
			else tnc = tnParent.Nodes;

			PwDatabase pd = m_docMgr.ActiveDatabase;
			foreach(PwGroup pg in pgContainer.Groups)
			{
				bool bExpired = (pg.Expires && (pg.ExpiryTime <= m_dtCachedNow));
				string strName = pg.Name; // + GetGroupSuffixText(pg);

				int nIconID = ((!pg.CustomIconUuid.EqualsValue(PwUuid.Zero)) ?
					((int)PwIcon.Count + pd.GetCustomIconIndex(pg.CustomIconUuid)) :
					(int)pg.IconId);
				if(bExpired) nIconID = (int)PwIcon.Expired;

				TreeNode tn = new TreeNode(strName, nIconID, nIconID);
				tn.Tag = pg;
				UIUtil.SetGroupNodeToolTip(tn, pg);

				if(pd.RecycleBinEnabled && pg.Uuid.EqualsValue(pd.RecycleBinUuid) &&
					(m_fontItalicTree != null))
					tn.NodeFont = m_fontItalicTree;
				else if(bExpired && (m_fontExpired != null))
					tn.NodeFont = m_fontExpired;

				tnc.Add(tn);

				RecursiveAddGroup(tn, pg, pgFind, ref tnFound);

				if(tn.Nodes.Count > 0)
				{
					if((tn.IsExpanded) && (!pg.IsExpanded)) tn.Collapse();
					else if((!tn.IsExpanded) && (pg.IsExpanded)) tn.Expand();
				}

				if(pg == pgFind) tnFound = tn;
			}
		}

		private void SortPasswordList(bool bEnableSorting, int nColumn,
			SortOrder? soForce, bool bUpdateEntryList)
		{
			AceColumnType colType = GetAceColumn(nColumn).Type;

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

					// Workaround for XP bug
					if(bUpdateEntryList && !KeePassLib.Native.NativeLib.IsUnix() &&
						WinUtil.IsWindowsXP)
						UpdateEntryList(null, true); // Only required on XP
				}
				else
				{
					m_pListSorter = new ListSorter();
					m_lvEntries.ListViewItemSorter = null;

					if(bUpdateEntryList) UpdateEntryList(null, true);
				}
			}
			else // Disable sorting
			{
				m_pListSorter = new ListSorter();
				m_lvEntries.ListViewItemSorter = null;

				if(bUpdateEntryList) UpdateEntryList(null, true);
			}

			UpdateColumnSortingIcons();
			UIUtil.SetAlternatingBgColors(m_lvEntries, m_clrAlternateItemBgColor,
				Program.Config.MainWindow.EntryListAlternatingBgColors);
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
				KeePassLib.Native.NativeLib.IsUnix())
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
			m_menuViewShowEntryView.Checked = bShow;

			Program.Config.MainWindow.EntryView.Show = bShow;

			m_richEntryView.Visible = bShow;
			m_splitHorizontal.Panel2Collapsed = !bShow;
		}

		private void ShowEntryDetails(PwEntry pe)
		{
			if(pe == null)
			{
				m_richEntryView.Text = string.Empty;
				return;
			}

			RichTextBuilder rb = new RichTextBuilder();

			AceFont af = Program.Config.UI.StandardFont;
			Font fontUI = (UISystemFonts.ListFont ?? m_lvEntries.Font);
			// string strFontFace = (af.OverrideUIDefault ? af.Family : fontUI.Name);
			// float fFontSize = (af.OverrideUIDefault ? af.ToFont().SizeInPoints : fontUI.SizeInPoints);
			if(af.OverrideUIDefault) rb.DefaultFont = af.ToFont();
			else rb.DefaultFont = fontUI;

			string strItemSeparator = ((m_splitHorizontal.Orientation == Orientation.Horizontal) ?
				", " : Environment.NewLine);

			// StringBuilder sb = new StringBuilder();
			// StrUtil.InitRtf(sb, strFontFace, fFontSize);

			rb.Append(KPRes.Group, FontStyle.Bold, null, null, ":", " ");
			int nGroupUrlStart = KPRes.Group.Length + 2;
			PwGroup pg = pe.ParentGroup;
			if(pg != null) rb.Append(pg.Name);

			EvAppendEntryField(rb, strItemSeparator, KPRes.Title,
				IsColumnHidden(AceColumnType.Title) ? PwDefs.HiddenPassword :
				pe.Strings.ReadSafe(PwDefs.TitleField), pe);
			EvAppendEntryField(rb, strItemSeparator, KPRes.UserName,
				IsColumnHidden(AceColumnType.UserName) ? PwDefs.HiddenPassword :
				pe.Strings.ReadSafe(PwDefs.UserNameField), pe);
			EvAppendEntryField(rb, strItemSeparator, KPRes.Password,
				IsColumnHidden(AceColumnType.Password) ? PwDefs.HiddenPassword :
				pe.Strings.ReadSafe(PwDefs.PasswordField), pe);
			EvAppendEntryField(rb, strItemSeparator, KPRes.Url,
				IsColumnHidden(AceColumnType.Url) ? PwDefs.HiddenPassword :
				pe.Strings.ReadSafe(PwDefs.UrlField), pe);

			bool bHideCustom = Program.Config.MainWindow.EntryView.HideProtectedCustomStrings;
			foreach(KeyValuePair<string, ProtectedString> kvp in pe.Strings)
			{
				if(PwDefs.IsStandardField(kvp.Key)) continue;

				string strCustomValue = (bHideCustom ? pe.Strings.ReadSafeEx(kvp.Key) :
					pe.Strings.ReadSafe(kvp.Key));
				EvAppendEntryField(rb, strItemSeparator, kvp.Key, strCustomValue, pe);
			}

			EvAppendEntryField(rb, strItemSeparator, KPRes.CreationTime,
				TimeUtil.ToDisplayString(pe.CreationTime), null);
			EvAppendEntryField(rb, strItemSeparator, KPRes.LastAccessTime,
				TimeUtil.ToDisplayString(pe.LastAccessTime), null);
			EvAppendEntryField(rb, strItemSeparator, KPRes.LastModificationTime,
				TimeUtil.ToDisplayString(pe.LastModificationTime), null);

			if(pe.Expires)
				EvAppendEntryField(rb, strItemSeparator, KPRes.ExpiryTime,
					TimeUtil.ToDisplayString(pe.ExpiryTime), null);

			if(pe.Binaries.UCount > 0)
			{
				StringBuilder sbBinaries = new StringBuilder();

				foreach(KeyValuePair<string, ProtectedBinary> kvpBin in pe.Binaries)
				{
					if(sbBinaries.Length > 0) sbBinaries.Append(", ");
					sbBinaries.Append(kvpBin.Key);
				}

				EvAppendEntryField(rb, strItemSeparator, KPRes.Attachments,
					sbBinaries.ToString(), null);
			}

			EvAppendEntryField(rb, strItemSeparator, KPRes.UrlOverride,
				pe.OverrideUrl, pe);
			EvAppendEntryField(rb, strItemSeparator, KPRes.Tags,
				StrUtil.TagsToString(pe.Tags, true), null);

			string strNotes = (IsColumnHidden(AceColumnType.Notes) ?
				PwDefs.HiddenPassword : pe.Strings.ReadSafe(PwDefs.NotesField));
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
			rb.Build(m_richEntryView);

			UIUtil.RtfLinkifyReferences(m_richEntryView, false);

			Debug.Assert(m_richEntryView.HideSelection); // Flicker otherwise
			if(pg != null)
			{
				m_richEntryView.Select(nGroupUrlStart, pg.Name.Length);
				UIUtil.RtfSetSelectionLink(m_richEntryView);
			}

			// Linkify the URL
			string strUrl = SprEngine.Compile(pe.Strings.ReadSafe(PwDefs.UrlField),
				GetEntryListSprContext(pe, m_docMgr.SafeFindContainerOf(pe)));
			if(strUrl != PwDefs.HiddenPassword)
				UIUtil.RtfLinkifyText(m_richEntryView, strUrl, false);

			// Linkify the attachments
			foreach(KeyValuePair<string, ProtectedBinary> kvpBin in pe.Binaries)
				UIUtil.RtfLinkifyText(m_richEntryView, kvpBin.Key, false);

			m_richEntryView.Select(0, 0);
		}

		private void EvAppendEntryField(RichTextBuilder rb,
			string strItemSeparator, string strName, string strValue,
			PwEntry peSprCompile)
		{
			if(string.IsNullOrEmpty(strValue)) return;

			rb.Append(strName, FontStyle.Bold, strItemSeparator, null, ":", " ");

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
					rb.Append(strCmp + " - ");
					rb.Append(strValue, FontStyle.Italic);
				}
			}
		}

		private void PerformDefaultAction(object sender, EventArgs e, PwEntry pe,
			int colID)
		{
			Debug.Assert(pe != null); if(pe == null) return;

			if(this.DefaultEntryAction != null)
			{
				CancelEntryEventArgs args = new CancelEntryEventArgs(pe, colID);
				this.DefaultEntryAction(sender, args);
				if(args.Cancel) return;
			}

			bool bCnt = false;

			AceColumn col = GetAceColumn(colID);
			AceColumnType colType = col.Type;
			switch(colType)
			{
				case AceColumnType.Title:
					if(PwDefs.IsTanEntry(pe)) OnEntryCopyPassword(sender, e);
					else OnEntryEdit(sender, e);
					break;
				case AceColumnType.UserName:
					OnEntryCopyUserName(sender, e);
					break;
				case AceColumnType.Password:
					OnEntryCopyPassword(sender, e);
					break;
				case AceColumnType.Url:
					PerformDefaultUrlAction(null, false);
					break;
				case AceColumnType.Notes:
					bCnt = ClipboardUtil.CopyAndMinimize(pe.Strings.GetSafe(
						PwDefs.NotesField), true, this, pe, m_docMgr.ActiveDatabase);
					break;
				case AceColumnType.CreationTime:
					bCnt = ClipboardUtil.CopyAndMinimize(TimeUtil.ToDisplayString(
						pe.CreationTime), true, this, pe, null);
					break;
				case AceColumnType.LastAccessTime:
					bCnt = ClipboardUtil.CopyAndMinimize(TimeUtil.ToDisplayString(
						pe.LastAccessTime), true, this, pe, null);
					break;
				case AceColumnType.LastModificationTime:
					bCnt = ClipboardUtil.CopyAndMinimize(TimeUtil.ToDisplayString(
						pe.LastModificationTime), true, this, pe, null);
					break;
				case AceColumnType.ExpiryTime:
					if(pe.Expires)
						bCnt = ClipboardUtil.CopyAndMinimize(TimeUtil.ToDisplayString(
							pe.ExpiryTime), true, this, pe, null);
					else
						bCnt = ClipboardUtil.CopyAndMinimize(m_strNeverExpiresText,
							true, this, pe, null);
					break;
				case AceColumnType.Attachment:
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
					bCnt = ClipboardUtil.CopyAndMinimize(StrUtil.TagsToString(pe.Tags, true),
						true, this, pe, null);
					break;
				case AceColumnType.ExpiryTimeDateOnly:
					if(pe.Expires)
						bCnt = ClipboardUtil.CopyAndMinimize(TimeUtil.ToDisplayStringDateOnly(
							pe.ExpiryTime), true, this, pe, null);
					else
						bCnt = ClipboardUtil.CopyAndMinimize(m_strNeverExpiresText,
							true, this, pe, null);
					break;
				case AceColumnType.Size:
					bCnt = ClipboardUtil.CopyAndMinimize(StrUtil.FormatDataSizeKB(
						pe.GetSize()), true, this, pe, null);
					break;
				case AceColumnType.HistoryCount:
					EditSelectedEntry(true);
					break;
				default:
					Debug.Assert(false);
					break;
			}

			if(bCnt) StartClipboardCountdown();
		}

		internal void PerformDefaultUrlAction(PwEntry[] vOptEntries, bool bForceOpen)
		{
			PwEntry[] v = (vOptEntries ?? GetSelectedEntries());
			Debug.Assert(v != null); if(v == null) return;

			bool bCopy = Program.Config.MainWindow.CopyUrlsInsteadOfOpening;
			if(bForceOpen) bCopy = false;

			if(bCopy)
			{
				// bool bMinimize = Program.Config.MainWindow.MinimizeAfterClipboardCopy;
				// Form frmMin = (bMinimize ? this : null);

				if(ClipboardUtil.CopyAndMinimize(UrlsToString(v, true), true, this, null, null))
					StartClipboardCountdown();
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

			if(pe.Binaries.UCount == 0) return;

			foreach(KeyValuePair<string, ProtectedBinary> kvp in pe.Binaries)
			{
				ExecuteBinaryEditView(kvp.Key, kvp.Value);
				break;
			}
		}

		private void ExecuteBinaryEditView(string strBinName, ProtectedBinary pb)
		{
			BinaryDataClass bdc = BinaryDataClassifier.Classify(strBinName,
				pb.ReadData());
			DynamicMenuEventArgs args = new DynamicMenuEventArgs(strBinName,
				DataEditorForm.SupportsDataType(bdc) ?
				new EditableBinaryAttachment(strBinName) : null);
			OnEntryBinaryView(null, args);
		}

		private string UrlsToString(PwEntry[] vEntries, bool bActive)
		{
			if((vEntries == null) || (vEntries.Length == 0)) return string.Empty;

			StringBuilder sb = new StringBuilder();
			foreach(PwEntry pe in vEntries)
			{
				if(sb.Length > 0) sb.Append(MessageService.NewLine);

				PwDatabase pd = m_docMgr.SafeFindContainerOf(pe);

				string strUrl = pe.Strings.ReadSafe(PwDefs.UrlField);
				strUrl = SprEngine.Compile(strUrl, new SprContext(pe, pd,
					(bActive ? SprCompileFlags.All : SprCompileFlags.NonActive)));

				sb.Append(strUrl);
			}

			UpdateUIState(false); // SprEngine.Compile might have modified the database
			return sb.ToString();
		}

		private delegate void PerformQuickFindDelegate(string strSearch,
			string strGroupName, bool bForceShowExpired);

		/// <summary>
		/// Do a quick find. All entries of the currently opened database are searched
		/// for a string and the results are automatically displayed in the main window.
		/// </summary>
		/// <param name="strSearch">String to search the entries for.</param>
		/// <param name="strGroupName">Group name of the group that receives the search
		/// results.</param>
		private void PerformQuickFind(string strSearch, string strGroupName,
			bool bForceShowExpired)
		{
			Debug.Assert(strSearch != null); if(strSearch == null) return;
			Debug.Assert(strGroupName != null); if(strGroupName == null) return;

			PwGroup pg = new PwGroup(true, true, strGroupName, PwIcon.EMailSearch);
			pg.IsVirtual = true;

			SearchParameters sp = new SearchParameters();
			sp.SearchString = strSearch;
			sp.SearchInTitles = sp.SearchInUserNames =
				sp.SearchInUrls = sp.SearchInNotes = sp.SearchInOther =
				sp.SearchInUuids = sp.SearchInGroupNames = sp.SearchInTags = true;
			sp.SearchInPasswords = Program.Config.MainWindow.QuickFindSearchInPasswords;

			if(!bForceShowExpired)
				sp.ExcludeExpired = Program.Config.MainWindow.QuickFindExcludeExpired;

			SearchUtil.SetTransformation(sp, (Program.Config.MainWindow.QuickFindDerefData ?
				SearchUtil.StrTrfDeref : string.Empty));

			StatusBarLogger sl = null;
			bool bBlock = (strSearch.Length > 0); // Showing all is fast
			if(bBlock)
			{
				Application.DoEvents(); // Finalize UI messages before blocking
				UIBlockInteraction(true);
				sl = CreateStatusBarLogger();
				sl.StartLogging(KPRes.SearchingOp + "...", false);
			}

			AutoAdjustMemProtSettings(m_docMgr.ActiveDatabase, sp);
			m_docMgr.ActiveDatabase.RootGroup.SearchEntries(sp, pg.Entries, sl);

			if(bBlock)
			{
				sl.EndLogging();
				UIBlockInteraction(false);
			}

			UpdateEntryList(pg, false);
			SelectFirstEntryIfNoneSelected();

			UpdateUIState(false);
			ShowSearchResultsStatusMessage();

			if(Program.Config.MainWindow.FocusResultsAfterQuickFind &&
				(pg.Entries.UCount > 0))
			{
				ResetDefaultFocus(m_lvEntries);
			}
		}

		private void ShowExpiredEntries(bool bOnlyIfExists, uint uSkipDays)
		{
			PwGroup pg = new PwGroup(true, true, KPRes.ExpiredEntries, PwIcon.Expired);
			pg.IsVirtual = true;

			DateTime dtLimit = DateTime.Now;
			if(uSkipDays != 0)
				dtLimit = dtLimit.Add(new TimeSpan((int)uSkipDays, 0, 0, 0));

			EntryHandler eh = delegate(PwEntry pe)
			{
				if(PwDefs.IsTanEntry(pe)) return true; // Exclude TANs
				if(pe.Expires && (pe.ExpiryTime <= dtLimit))
					pg.AddEntry(pe, false);
				return true;
			};

			m_docMgr.ActiveDatabase.RootGroup.TraverseTree(TraversalMethod.PreOrder, null, eh);

			if(uSkipDays != 0) pg.Name = KPRes.SoonToExpireEntries;

			if((pg.Entries.UCount > 0) || !bOnlyIfExists)
			{
				UpdateEntryList(pg, false);
				UpdateUIState(false);
			}
			else
			{
				UpdateEntryList(null, false);
				UpdateUIState(false);
			}

			ShowSearchResultsStatusMessage();
		}

		public void PerformExport(PwGroup pgDataSource, bool bExportDeleted)
		{
			Debug.Assert(m_docMgr.ActiveDatabase.IsOpen); if(!m_docMgr.ActiveDatabase.IsOpen) return;

			if(!AppPolicy.Try(AppPolicyId.Export)) return;

			PwDatabase pd = m_docMgr.ActiveDatabase;
			if((pd == null) || !pd.IsOpen) return;
			if(!AppPolicy.Current.ExportNoKey)
			{
				if(!KeyUtil.ReAskKey(pd, true)) return;
			}

			PwGroup pg = (pgDataSource ?? pd.RootGroup);
			PwExportInfo pwInfo = new PwExportInfo(pg, pd, bExportDeleted);

			MessageService.ExternalIncrementMessageCount();
			ShowWarningsLogger swLogger = CreateShowWarningsLogger();
			swLogger.StartLogging(KPRes.ExportingStatusMsg, true);

			ExportUtil.Export(pwInfo, swLogger);

			swLogger.EndLogging();
			MessageService.ExternalDecrementMessageCount();
			UpdateUIState(false);
		}

		internal static IOConnectionInfo CompleteConnectionInfo(IOConnectionInfo ioc,
			bool bSave, bool bCanRememberCred, bool bTestConnection, bool bForceShow)
		{
			if(ioc == null) { Debug.Assert(false); return null; }
			if(!bForceShow && ((ioc.CredSaveMode == IOCredSaveMode.SaveCred) ||
				ioc.IsLocalFile()))
				return ioc.CloneDeep();

			IOConnectionForm dlg = new IOConnectionForm();
			dlg.InitEx(bSave, ioc.CloneDeep(), bCanRememberCred, bTestConnection);
			if(UIUtil.ShowDialogNotValue(dlg, DialogResult.OK)) return null;

			IOConnectionInfo iocResult = dlg.IOConnectionInfo;
			UIUtil.DestroyForm(dlg);
			return iocResult;
		}

		private sealed class OdKpfConstructParams
		{
			public IOConnectionInfo IOConnectionInfo = null;
			public bool CanExit = false;
			public bool SecureDesktopMode = false; // Must be false by default
		}

		private static Form OdKpfConstruct(object objParam)
		{
			OdKpfConstructParams p = (objParam as OdKpfConstructParams);
			if(p == null) { Debug.Assert(false); return null; }

			KeyPromptForm kpf = new KeyPromptForm();
			kpf.InitEx(p.IOConnectionInfo, p.CanExit, true);
			kpf.SecureDesktopMode = p.SecureDesktopMode;
			return kpf;
		}

		private sealed class OdKpfResult
		{
			public CompositeKey Key = null;
			public bool ShowHelpAfterClose = false;
			public bool HasClosedWithExit = false;
		}

		private static object OdKpfBuildResult(Form f)
		{
			KeyPromptForm kpf = (f as KeyPromptForm);
			if(kpf == null) { Debug.Assert(false); return null; }

			OdKpfResult kpfResult = new OdKpfResult();
			kpfResult.Key = kpf.CompositeKey;
			kpfResult.ShowHelpAfterClose = kpf.ShowHelpAfterClose;
			kpfResult.HasClosedWithExit = kpf.HasClosedWithExit;

			return kpfResult;
		}

		/// <summary>
		/// Open a database. This function opens the specified database and updates
		/// the user interface.
		/// </summary>
		public void OpenDatabase(IOConnectionInfo ioConnection, CompositeKey cmpKey,
			bool bOpenLocal)
		{
			// OnFileClose(null, null);
			// if(m_docMgr.ActiveDatabase.IsOpen) return;

			if(m_bFormLoading && Program.Config.Application.Start.MinimizedAndLocked &&
				(ioConnection != null) && (ioConnection.Path.Length > 0))
			{
				PwDocument ds = m_docMgr.CreateNewDocument(true);
				ds.LockedIoc = ioConnection.CloneDeep();
				UpdateUI(true, ds, true, null, true, null, false);
				return;
			}

			IOConnectionInfo ioc;
			if(ioConnection == null)
			{
				if(bOpenLocal)
				{
					OpenFileDialog ofdDb = UIUtil.CreateOpenFileDialog(KPRes.OpenDatabaseFile,
						UIUtil.CreateFileTypeFilter(AppDefs.FileExtension.FileExt,
						KPRes.KdbxFiles, true), 1, null, false, false);

					GlobalWindowManager.AddDialog(ofdDb);
					DialogResult dr = ofdDb.ShowDialog();
					GlobalWindowManager.RemoveDialog(ofdDb);
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
			if(cmpKey == null)
			{
				for(int iTry = 0; iTry < 3; ++iTry)
				{
					OdKpfConstructParams kpfParams = new OdKpfConstructParams();
					kpfParams.IOConnectionInfo = ioc;
					kpfParams.CanExit = IsFileLocked(null);

					DialogResult dr;
					OdKpfResult kpfResult;

					if(Program.Config.Security.MasterKeyOnSecureDesktop &&
						WinUtil.IsAtLeastWindows2000 &&
						!KeePassLib.Native.NativeLib.IsUnix())
					{
						kpfParams.SecureDesktopMode = true;

						ProtectedDialog dlg = new ProtectedDialog(OdKpfConstruct,
							OdKpfBuildResult);
						object objResult;
						dr = dlg.ShowDialog(out objResult, kpfParams);
						if(dr == DialogResult.None) { Debug.Assert(false); dr = DialogResult.Cancel; }
						
						kpfResult = (objResult as OdKpfResult);
						if(kpfResult == null) { Debug.Assert(false); continue; }

						if(kpfResult.ShowHelpAfterClose)
							AppHelp.ShowHelp(AppDefs.HelpTopics.KeySources, null);
					}
					else // Show dialog on normal desktop
					{
						Form dlg = OdKpfConstruct(kpfParams);
						dr = dlg.ShowDialog();

						kpfResult = (OdKpfBuildResult(dlg) as OdKpfResult);
						UIUtil.DestroyForm(dlg);
						if(kpfResult == null) { Debug.Assert(false); continue; }
					}

					if(dr == DialogResult.Cancel) break;
					else if(kpfResult.HasClosedWithExit)
					{
						Debug.Assert(dr == DialogResult.Abort);
						OnFileExit(null, null);
						return;
					}

					pwOpenedDb = OpenDatabaseInternal(ioc, kpfResult.Key);
					if(pwOpenedDb != null) break;
				}
			}
			else // cmpKey != null
			{
				pwOpenedDb = OpenDatabaseInternal(ioc, cmpKey);
			}

			if((pwOpenedDb == null) || !pwOpenedDb.IsOpen)
			{
				UpdateUIState(false); // Reset status bar text
				return;
			}

			string strName = pwOpenedDb.IOConnectionInfo.GetDisplayName();
			m_mruList.AddItem(strName, pwOpenedDb.IOConnectionInfo.CloneDeep(), true);

			PwDocument dsExisting = m_docMgr.FindDocument(pwOpenedDb);
			if(dsExisting != null) m_docMgr.ActiveDocument = dsExisting;

			bool bCorrectDbActive = (m_docMgr.ActiveDocument.Database == pwOpenedDb);
			Debug.Assert(bCorrectDbActive);

			// AutoEnableVisualHiding();

			SetLastUsedFile(pwOpenedDb.IOConnectionInfo);
			RememberKeyFilePath(pwOpenedDb);

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

			if((pwOpenedDb.MasterKeyChangeForce >= 0) &&
				((DateTime.Now - pwOpenedDb.MasterKeyChanged).Days >=
				pwOpenedDb.MasterKeyChangeForce))
			{
				while(true)
				{
					MessageService.ShowInfo(pwOpenedDb.IOConnectionInfo.GetDisplayName() +
						MessageService.NewParagraph + KPRes.MasterKeyChangeForce +
						MessageService.NewParagraph + KPRes.MasterKeyChangeInfo);
					if(ChangeMasterKey(pwOpenedDb))
					{
						UpdateUIState(true);
						break;
					}
					if(!AppPolicy.Current.ChangeMasterKey) break; // Prevent endless loop
				}
			}
			else if((pwOpenedDb.MasterKeyChangeRec >= 0) &&
				((DateTime.Now - pwOpenedDb.MasterKeyChanged).Days >=
				pwOpenedDb.MasterKeyChangeRec))
			{
				if(MessageService.AskYesNo(pwOpenedDb.IOConnectionInfo.GetDisplayName() +
					MessageService.NewParagraph + KPRes.MasterKeyChangeRec +
					MessageService.NewParagraph + KPRes.MasterKeyChangeQ))
					UpdateUIState(ChangeMasterKey(pwOpenedDb));
			}

			if(this.FileOpened != null)
			{
				FileOpenedEventArgs ea = new FileOpenedEventArgs(pwOpenedDb);
				this.FileOpened(this, ea);
			}
			Program.TriggerSystem.RaiseEvent(EcasEventIDs.OpenedDatabaseFile,
				pwOpenedDb.IOConnectionInfo.Path);

			if(bCorrectDbActive && pwOpenedDb.IsOpen &&
				Program.Config.Application.FileOpening.ShowSoonToExpireEntries)
			{
				ShowExpiredEntries(true, 7);

				// Avoid view being destroyed by the unlocking routine
				pwOpenedDb.LastSelectedGroup = PwUuid.Zero;
			}
			else if(bCorrectDbActive && pwOpenedDb.IsOpen &&
				Program.Config.Application.FileOpening.ShowExpiredEntries)
			{
				ShowExpiredEntries(true, 0);

				// Avoid view being destroyed by the unlocking routine
				pwOpenedDb.LastSelectedGroup = PwUuid.Zero;
			}

			if(Program.Config.MainWindow.MinimizeAfterOpeningDatabase)
				UIUtil.SetWindowState(this, FormWindowState.Minimized);

			ResetDefaultFocus(null);
		}

		private PwDatabase OpenDatabaseInternal(IOConnectionInfo ioc, CompositeKey cmpKey)
		{
			PerformSelfTest();

			ShowWarningsLogger swLogger = CreateShowWarningsLogger();
			swLogger.StartLogging(KPRes.OpeningDatabase, true);

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

			try
			{
				pwDb.Open(ioc, cmpKey, swLogger);

#if DEBUG
				byte[] pbDiskDirect = WinUtil.HashFile(ioc);
				Debug.Assert(MemUtil.ArraysEqual(pbDiskDirect, pwDb.HashOfFileOnDisk));
#endif
			}
			catch(Exception ex)
			{
				MessageService.ShowLoadWarning(ioc.GetDisplayName(), ex);
				pwDb = null;
			}

			swLogger.EndLogging();

			if(pwDb == null)
			{
				if(ds == null) m_docMgr.CloseDatabase(m_docMgr.ActiveDatabase);
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

		// private static void AutoEnableVisualHiding() // Remove static when implementing
		// {
			// KPF 1802197

			// Turn on visual hiding if option is selected
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
		// }

		private TreeNode GuiFindGroup(PwUuid puSearch, TreeNode tnContainer)
		{
			Debug.Assert(puSearch != null);
			if(puSearch == null) return null;

			if(m_tvGroups.Nodes.Count == 0) return null;

			if(tnContainer == null) tnContainer = m_tvGroups.Nodes[0];

			foreach(TreeNode tn in tnContainer.Nodes)
			{
				if(((PwGroup)tn.Tag).Uuid.EqualsValue(puSearch))
					return tn;

				if(tn != tnContainer)
				{
					TreeNode tnSub = GuiFindGroup(puSearch, tn);
					if(tnSub != null) return tnSub;
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
				if(((PwListItem)lvi.Tag).Entry.Uuid.EqualsValue(puSearch))
					return lvi;
			}

			return null;
		}

		private void PrintGroup(PwGroup pg)
		{
			Debug.Assert(pg != null); if(pg == null) return;
			if(!AppPolicy.Try(AppPolicyId.Print)) return;

			PwDatabase pd = m_docMgr.ActiveDatabase;
			if((pd == null) || !pd.IsOpen) return;
			if(!AppPolicy.Current.PrintNoKey)
			{
				if(!KeyUtil.ReAskKey(pd, true)) return;
			}

			PrintForm pf = new PrintForm();
			pf.InitEx(pg, true, m_pListSorter.Column);
			UIUtil.ShowDialogAndDestroy(pf);
		}

		private void InsertToolStripItem(ToolStripMenuItem tsContainer, ToolStripMenuItem tsTemplate, EventHandler ev,
			bool bPermanentlyLinkToTemplate)
		{
			ToolStripMenuItem tsmi = new ToolStripMenuItem(tsTemplate.Text, tsTemplate.Image);
			tsmi.Click += ev;

			Debug.Assert(tsTemplate.ShortcutKeys == Keys.None);
			// tsmi.ShortcutKeys = tsTemplate.ShortcutKeys;
			tsmi.ShowShortcutKeys = tsTemplate.ShowShortcutKeys;

			string strKeys = tsTemplate.ShortcutKeyDisplayString;
			if(!string.IsNullOrEmpty(strKeys))
				tsmi.ShortcutKeyDisplayString = strKeys;

			if(bPermanentlyLinkToTemplate)
				m_vLinkedToolStripItems.Add(new KeyValuePair<ToolStripItem, ToolStripItem>(
					tsTemplate, tsmi));

			tsContainer.DropDownItems.Insert(0, tsmi);
		}

		/// <summary>
		/// Set the linked menu item's Enabled state to the state of their parents.
		/// </summary>
		public void UpdateLinkedMenuItems()
		{
			foreach(KeyValuePair<ToolStripItem, ToolStripItem> kvp in m_vLinkedToolStripItems)
				kvp.Value.Enabled = kvp.Key.Enabled;
		}

		/// <summary>
		/// Handler function that is called when an MRU item is clicked (see MRU interface).
		/// </summary>
		public void OnMruExecute(string strDisplayName, object oTag)
		{
			Debug.Assert(oTag is IOConnectionInfo);
			OpenDatabase(oTag as IOConnectionInfo, null, false);
		}

		public void OnMruExecute2(string strDisplayName, object oTag)
		{
			Debug.Assert(oTag is IOConnectionInfo);
			IOConnectionInfo ioc = (oTag as IOConnectionInfo);
			ioc = CompleteConnectionInfo(ioc, false, true, true, false);
			if(ioc == null) return;

			bool? b = ImportUtil.Synchronize(m_docMgr.ActiveDatabase, this, ioc, false, this);
			UpdateUI(false, null, true, null, true, null, false);
			if(b.HasValue) SetStatusEx(b.Value ? KPRes.SyncSuccess : KPRes.SyncFailed);
		}

		/// <summary>
		/// Handler function that is called when the MRU list must be cleared (see MRU interface).
		/// </summary>
		public void OnMruClear()
		{
			m_mruList.Clear();
			m_mruList.UpdateMenu();
		}

		/// <summary>
		/// Function to update the tray icon based on the current window state.
		/// </summary>
		public void UpdateTrayIcon()
		{
			if(m_ntfTray == null) { Debug.Assert(false); return; } // Required

			bool bWindowVisible = this.Visible;
			bool bTrayVisible = m_ntfTray.Visible;

			if(Program.Config.UI.TrayIcon.ShowOnlyIfTrayed)
				m_ntfTray.Visible = !bWindowVisible;
			else if(bWindowVisible && !bTrayVisible)
				m_ntfTray.Visible = true;
		}

		private void OnSessionLock(object sender, SessionLockEventArgs e)
		{
			if((e.Reason == SessionLockReason.RemoteControlChange) &&
				Program.Config.Security.WorkspaceLocking.LockOnRemoteControlChange) { }
			else if(((e.Reason == SessionLockReason.Lock) || (e.Reason == SessionLockReason.Ending)) &&
				Program.Config.Security.WorkspaceLocking.LockOnSessionSwitch) { }
			else if((e.Reason == SessionLockReason.Suspend) &&
				Program.Config.Security.WorkspaceLocking.LockOnSuspend) { }
			else return;

			if(IsAtLeastOneFileOpen()) LockAllDocuments();
		}

		/// <summary>
		/// This function resets the internal user-inactivity timer.
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
		/// <param name="nMove">Must be 2/-2 to move to top/bottom, 1/-1 to
		/// move one up/down.</param>
		private void MoveSelectedEntries(int nMove)
		{
			// Don't allow moving when sorting is enabled
			if((m_pListSorter.Column >= 0) && (m_pListSorter.Order != SortOrder.None))
				return;

			PwEntry[] vEntries = GetSelectedEntries();
			Debug.Assert(vEntries != null); if(vEntries == null) return;
			Debug.Assert(vEntries.Length > 0); if(vEntries.Length == 0) return;

			DateTime dtNow = DateTime.Now;
			PwGroup pg = vEntries[0].ParentGroup;
			foreach(PwEntry pe in vEntries)
			{
				if(pe.ParentGroup != pg)
				{
					MessageService.ShowWarning(KPRes.CannotMoveEntriesBcsGroup);
					return;
				}
			}

			if(nMove == -1)
			{
				Debug.Assert(vEntries.Length == 1);
				pg.Entries.MoveOne(vEntries[0], false);
			}
			else if(nMove == 1)
			{
				Debug.Assert(vEntries.Length == 1);
				pg.Entries.MoveOne(vEntries[0], true);
			}
			else if(nMove == -2) pg.Entries.MoveTopBottom(vEntries, false);
			else if(nMove == 2) pg.Entries.MoveTopBottom(vEntries, true);
			else { Debug.Assert(false); }

			if((nMove == -1) || (nMove == 1)) vEntries[0].LocationChanged = dtNow;
			else if((nMove == -2) || (nMove == 2))
			{
				foreach(PwEntry peUpd in vEntries) peUpd.LocationChanged = dtNow;
			}
			else { Debug.Assert(false); }

			UpdateEntryList(null, false);
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
			switch(wParam)
			{
				case AppDefs.GlobalHotKeyId.AutoType:
					ExecuteGlobalAutoType();
					break;

				case AppDefs.GlobalHotKeyId.AutoTypeSelected:
					ExecuteEntryAutoType();
					break;

				case AppDefs.GlobalHotKeyId.ShowWindow:
					bool bWndVisible = ((this.WindowState != FormWindowState.Minimized) &&
						!IsTrayed());
					EnsureVisibleForegroundWindow(true, true);
					if(bWndVisible && IsFileLocked(null))
						OnFileLock(null, EventArgs.Empty); // Unlock
					break;

				case AppDefs.GlobalHotKeyId.EntryMenu:
					EntryMenu.Show();
					break;

				default:
					Debug.Assert(false);
					break;
			}
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

			base.WndProc(ref m);
		}

		public void ProcessAppMessage(IntPtr wParam, IntPtr lParam)
		{
			NotifyUserActivity();

			if(wParam == (IntPtr)Program.AppMessage.RestoreWindow)
				EnsureVisibleForegroundWindow(true, true);
			else if(wParam == (IntPtr)Program.AppMessage.Exit)
				OnFileExit(null, EventArgs.Empty);
			else if(wParam == (IntPtr)Program.AppMessage.IpcByFile)
				IpcUtilEx.ProcessGlobalMessage(lParam.ToInt32(), this);
			else if(wParam == (IntPtr)Program.AppMessage.AutoType)
				ExecuteGlobalAutoType();
			else if(wParam == (IntPtr)Program.AppMessage.Lock)
				LockAllDocuments();
			else if(wParam == (IntPtr)Program.AppMessage.Unlock)
			{
				if(IsFileLocked(null))
				{
					EnsureVisibleForegroundWindow(false, false);
					OnFileLock(null, EventArgs.Empty); // Unlock
				}
			}
		}

		public void ExecuteGlobalAutoType()
		{
			if(m_bIsAutoTyping) return;
			m_bIsAutoTyping = true;

			if(IsAtLeastOneFileOpen() == false)
			{
				try
				{
					IntPtr hPrevWnd = NativeMethods.GetForegroundWindowHandle();

					EnsureVisibleForegroundWindow(false, false);

					// The window restoration function above maybe
					// restored the window already, therefore only
					// try to unlock if it's locked *now*
					if(IsFileLocked(null)) OnFileLock(null, EventArgs.Empty);

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
					m_ilCurrentIcons);
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
			if(bUntray && IsTrayed()) MinimizeToTray(false);

			if(bRestoreWindow && (this.WindowState == FormWindowState.Minimized))
				UIUtil.SetWindowState(this, FormWindowState.Normal);

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

		private void SetListFont(AceFont font)
		{
			if((font != null) && font.OverrideUIDefault)
			{
				m_tvGroups.Font = font.ToFont();
				m_lvEntries.Font = font.ToFont();
				m_richEntryView.Font = font.ToFont();

				Program.Config.UI.StandardFont = font;
			}
			else
			{
				if(UIUtil.VistaStyleListsSupported)
				{
					Font fontUI = UISystemFonts.ListFont;
					m_tvGroups.Font = fontUI;
					m_lvEntries.Font = fontUI;
					m_richEntryView.Font = fontUI;
				}

				Program.Config.UI.StandardFont.OverrideUIDefault = false;
			}

			m_fontExpired = FontUtil.CreateFont(m_lvEntries.Font, FontStyle.Strikeout);
			m_fontBoldUI = FontUtil.CreateFont(m_tabMain.Font, FontStyle.Bold);
			m_fontBoldTree = FontUtil.CreateFont(m_lvEntries.Font, FontStyle.Bold);
			m_fontItalicTree = FontUtil.CreateFont(m_lvEntries.Font, FontStyle.Italic);
		}

		private void SetSelectedEntryColor(Color clrBack)
		{
			if(m_docMgr.ActiveDatabase.IsOpen == false) return;
			PwEntry[] vSelected = GetSelectedEntries();
			if((vSelected == null) || (vSelected.Length == 0)) return;

			foreach(PwEntry pe in vSelected)
			{
				pe.BackgroundColor = clrBack;
				pe.Touch(true, false);
			}

			RefreshEntriesList();
			UpdateUIState(true);
		}

		private void OnCopyCustomString(object sender, DynamicMenuEventArgs e)
		{
			PwEntry pe = GetSelectedEntry(false);
			if(pe == null) return;

			if(ClipboardUtil.CopyAndMinimize(pe.Strings.GetSafe(e.ItemName), true,
				this, pe, m_docMgr.ActiveDatabase))
				StartClipboardCountdown();
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
			m_menuFileNew.ShortcutKeys = (Keys.Control | Keys.N);
			m_menuFileOpenLocal.ShortcutKeys = (Keys.Control | Keys.O);
			m_menuFileClose.ShortcutKeys = (Keys.Control | Keys.W);
			m_menuFileSave.ShortcutKeys = (Keys.Control | Keys.S);
			m_menuFilePrint.ShortcutKeys = (Keys.Control | Keys.P);
			m_menuFileLock.ShortcutKeys = (Keys.Control | Keys.L);

			m_menuEditFind.ShortcutKeys = (Keys.Control | Keys.F);
			// m_ctxEntryAdd.ShortcutKeys = Keys.Control | Keys.N;

			// m_menuViewHidePasswords.ShortcutKeys = (Keys.Control | Keys.H);
			// m_menuViewHideUserNames.ShortcutKeys = (Keys.Control | Keys.J);

			m_menuHelpContents.ShortcutKeys = Keys.F1;

			m_ctxGroupFind.ShortcutKeys = (Keys.Control | Keys.Shift | Keys.F);

			string strCtrl = KPRes.KeyboardKeyCtrl, strAlt = KPRes.KeyboardKeyAlt;

			m_ctxEntryCopyUserName.ShortcutKeyDisplayString = strCtrl + "+B";
			m_ctxEntryCopyPassword.ShortcutKeyDisplayString = strCtrl + "+C";
			m_ctxEntryPerformAutoType.ShortcutKeyDisplayString = strCtrl + "+V";
			m_ctxEntryAdd.ShortcutKeyDisplayString = "Ins";
			m_ctxEntryEdit.ShortcutKeyDisplayString = "Return";
			m_ctxEntryDelete.ShortcutKeyDisplayString = "Del";
			m_ctxEntrySelectAll.ShortcutKeyDisplayString = strCtrl + "+A";

			m_ctxEntryMoveToTop.ShortcutKeyDisplayString = strAlt + "+Home";
			m_ctxEntryMoveOneUp.ShortcutKeyDisplayString = strAlt + "+Up";
			m_ctxEntryMoveOneDown.ShortcutKeyDisplayString = strAlt + "+Down";
			m_ctxEntryMoveToBottom.ShortcutKeyDisplayString = strAlt + "+End";

			m_ctxGroupDelete.ShortcutKeyDisplayString = "Del";

			m_ctxGroupMoveToTop.ShortcutKeyDisplayString = strAlt + "+Home";
			m_ctxGroupMoveOneUp.ShortcutKeyDisplayString = strAlt + "+Up";
			m_ctxGroupMoveOneDown.ShortcutKeyDisplayString = strAlt + "+Down";
			m_ctxGroupMoveToBottom.ShortcutKeyDisplayString = strAlt + "+End";
		}

		private void AssignMenuShortcutsOpt()
		{
			try
			{
				string str = KPRes.KeyboardKeyCtrl + "+U";
				bool b = Program.Config.MainWindow.CopyUrlsInsteadOfOpening;
				m_ctxEntryOpenUrl.ShortcutKeyDisplayString = (b ? null : str);
				m_ctxEntryCopyUrl.ShortcutKeyDisplayString = (b ? str : null);
			}
			catch(Exception) { Debug.Assert(false); }
		}

		private void SaveDatabaseAs(bool bOnline, object sender, bool bCopy)
		{
			if(!m_docMgr.ActiveDatabase.IsOpen) return;
			if(!AppPolicy.Try(AppPolicyId.SaveFile)) return;

			PwDatabase pd = m_docMgr.ActiveDatabase;

			Guid eventGuid = Guid.NewGuid();
			if(this.FileSaving != null)
			{
				FileSavingEventArgs args = new FileSavingEventArgs(true, bCopy, pd, eventGuid);
				this.FileSaving(sender, args);
				if(args.Cancel) return;
			}

			DialogResult dr;
			IOConnectionInfo ioc = new IOConnectionInfo();

			if(bOnline)
			{
				IOConnectionForm iocf = new IOConnectionForm();
				iocf.InitEx(true, pd.IOConnectionInfo.CloneDeep(), true, true);

				dr = iocf.ShowDialog();
				ioc = iocf.IOConnectionInfo;
				UIUtil.DestroyForm(iocf);
			}
			else
			{
				SaveFileDialog sfdDb = UIUtil.CreateSaveFileDialog(KPRes.SaveDatabase,
					UrlUtil.GetFileName(pd.IOConnectionInfo.Path),
					UIUtil.CreateFileTypeFilter(AppDefs.FileExtension.FileExt,
					KPRes.KdbxFiles, true), 1, AppDefs.FileExtension.FileExt, false, true);

				GlobalWindowManager.AddDialog(sfdDb);
				dr = sfdDb.ShowDialog();
				GlobalWindowManager.RemoveDialog(sfdDb);

				if(dr == DialogResult.OK)
					ioc = IOConnectionInfo.FromPath(sfdDb.FileName);
			}

			if(dr == DialogResult.OK)
			{
				Program.TriggerSystem.RaiseEvent(EcasEventIDs.SavingDatabaseFile,
					ioc.Path);

				UIBlockInteraction(true);

				ShowWarningsLogger swLogger = CreateShowWarningsLogger();
				swLogger.StartLogging(KPRes.SavingDatabase, true);

				bool bSuccess = true;
				try
				{
					PreSavingEx(pd);
					pd.SaveAs(ioc, !bCopy, swLogger);
					PostSavingEx(!bCopy, pd, ioc, swLogger);
				}
				catch(Exception exSaveAs)
				{
					MessageService.ShowSaveWarning(ioc, exSaveAs, true);
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
						ioc.Path);
			}
		}

		private void PreSavingEx(PwDatabase pwDatabase)
		{
			PerformSelfTest();

			pwDatabase.UseFileTransactions = Program.Config.Application.UseTransactedFileWrites;

			AceColumn col = Program.Config.MainWindow.FindColumn(AceColumnType.Title);
			if((col != null) && !col.HideWithAsterisks)
				pwDatabase.MemoryProtection.ProtectTitle = false;
			col = Program.Config.MainWindow.FindColumn(AceColumnType.UserName);
			if((col != null) && !col.HideWithAsterisks)
				pwDatabase.MemoryProtection.ProtectUserName = false;
			col = Program.Config.MainWindow.FindColumn(AceColumnType.Url);
			if((col != null) && !col.HideWithAsterisks)
				pwDatabase.MemoryProtection.ProtectUrl = false;
			col = Program.Config.MainWindow.FindColumn(AceColumnType.Notes);
			if((col != null) && !col.HideWithAsterisks)
				pwDatabase.MemoryProtection.ProtectNotes = false;

			if(pwDatabase == m_docMgr.ActiveDatabase) SaveWindowState();
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

				m_mruList.AddItem(ioc.GetDisplayName(), ioc.CloneDeep(), true);

				SetLastUsedFile(ioc);

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

			RememberKeyFilePath(pwDatabase);
			WinUtil.FlushStorageBuffers(ioc.Path, true);
		}

		public bool UIFileSave(bool bForceSave)
		{
			Control cFocus = UIUtil.GetActiveControl(this);

			m_docMgr.ActiveDatabase.Modified = true;

			m_bForceSave = bForceSave;
			OnFileSave(null, null);
			m_bForceSave = false;

			if(cFocus != null) ResetDefaultFocus(cFocus);
			return !m_docMgr.ActiveDatabase.Modified;
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

		private void UpdateImageLists()
		{
			if(!m_docMgr.ActiveDatabase.UINeedsIconUpdate) return;
			m_docMgr.ActiveDatabase.UINeedsIconUpdate = false;

			ImageList imgList = new ImageList();
			imgList.ImageSize = new Size(16, 16);
			imgList.ColorDepth = ColorDepth.Depth32Bit;

			// foreach(Image img in m_ilClientIcons.Images) imgList.Images.Add(img);
			List<Image> lStdImages = new List<Image>();
			foreach(Image imgStd in m_ilClientIcons.Images) lStdImages.Add(imgStd);
			imgList.Images.AddRange(lStdImages.ToArray());

			Debug.Assert(imgList.Images.Count == (int)PwIcon.Count);

			// ImageList imgListCustom = UIUtil.BuildImageList(
			//	m_docMgr.ActiveDatabase.CustomIcons, 16, 16);
			// foreach(Image imgCustom in imgListCustom.Images)
			//	imgList.Images.Add(imgCustom); // Breaks alpha partially
			List<Image> lCustom = UIUtil.BuildImageListEx(
				m_docMgr.ActiveDatabase.CustomIcons, 16, 16);
			if((lCustom != null) && (lCustom.Count > 0))
				imgList.Images.AddRange(lCustom.ToArray());

			m_ilCurrentIcons = imgList;

			if(UIUtil.VistaStyleListsSupported)
			{
				m_tvGroups.ImageList = imgList;
				m_lvEntries.SmallImageList = imgList;
			}
			else
			{
				List<Image> vAllImages = new List<Image>();
				foreach(Image imgClient in m_ilClientIcons.Images)
					vAllImages.Add(imgClient);
				vAllImages.AddRange(lCustom);
				Debug.Assert(imgList.Images.Count == vAllImages.Count);

				ImageList imgSafe = UIUtil.ConvertImageList24(vAllImages, 16, 16,
					AppDefs.ColorControlNormal);
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

			if(iMove == 2)
				pgParent.Groups.MoveTopBottom(pgAffected, true);
			else if(iMove == 1)
				pgParent.Groups.MoveOne(pgMove, true);
			else if(iMove == -1)
				pgParent.Groups.MoveOne(pgMove, false);
			else if(iMove == -2)
				pgParent.Groups.MoveTopBottom(pgAffected, false);
			else { Debug.Assert(false); }

			pgMove.LocationChanged = DateTime.Now;
			UpdateUI(false, null, true, null, true, null, true);
		}

		private void OnEntryBinaryView(object sender, DynamicMenuEventArgs e)
		{
			PwEntry pe = GetSelectedEntry(false);
			if(pe == null) { Debug.Assert(false); return; }

			EditableBinaryAttachment eba = (e.Tag as EditableBinaryAttachment);

			ProtectedBinary pbData = pe.Binaries.Get((eba != null) ? eba.Name :
				e.ItemName);
			if(pbData == null) { Debug.Assert(false); return; }

			if(eba == null) // Not editable
			{
				DataViewerForm dvf = new DataViewerForm();
				dvf.InitEx(e.ItemName, pbData.ReadData());
				UIUtil.ShowDialogAndDestroy(dvf);
			}
			else
			{
				DataEditorForm def = new DataEditorForm();
				def.InitEx(eba.Name, pbData.ReadData());
				def.ShowDialog();

				if(def.EditedBinaryData != null) // User changed the data
				{
					pe.Binaries.Set(eba.Name, new ProtectedBinary(false,
						def.EditedBinaryData));
					pe.Touch(true, false);

					RefreshEntriesList();
					UpdateUIState(true);
				}

				UIUtil.DestroyForm(def);
			}
		}

		private void SaveWindowState()
		{
			PwDatabase pd = m_docMgr.ActiveDatabase;

			PwGroup pgSelected = GetSelectedGroup();
			if(pgSelected != null)
				pd.LastSelectedGroup = new PwUuid(pgSelected.Uuid.UuidBytes);

			TreeNode tnTop = m_tvGroups.TopNode;
			if(tnTop != null)
			{
				PwGroup pgTop = (tnTop.Tag as PwGroup);
				pd.LastTopVisibleGroup = new PwUuid(pgTop.Uuid.UuidBytes);
			}

			PwEntry peTop = GetTopEntry();
			if((peTop != null) && (pgSelected != null))
				pgSelected.LastTopVisibleEntry = new PwUuid(peTop.Uuid.UuidBytes);
		}

		private void RestoreWindowState(PwDatabase pd)
		{
			PwGroup pgSelect = null;

			if(!pd.LastSelectedGroup.EqualsValue(PwUuid.Zero))
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
			{
				m_lvEntries.EnsureVisible(m_lvEntries.Items.Count - 1);
				m_lvEntries.EnsureVisible(lviTop.Index);
			}
		}

		private void CloseDocument(PwDocument ds, bool bLocking, bool bExiting)
		{
			if(ds == null) ds = m_docMgr.ActiveDocument;

			PwDatabase pd = ds.Database;
			IOConnectionInfo ioClosing = pd.IOConnectionInfo.CloneDeep();

			if(pd.Modified) // Implies pd.IsOpen
			{
				if(Program.Config.Application.FileClosing.AutoSave)
				{
					OnFileSave(null, EventArgs.Empty);
					if(pd.Modified) return;
				}
				else
				{
					FileSaveOrigin fso = FileSaveOrigin.Closing;
					if(bLocking) fso = FileSaveOrigin.Locking;
					if(bExiting) fso = FileSaveOrigin.Exiting;

					DialogResult dr = FileDialogsEx.ShowFileSaveQuestion(
						pd.IOConnectionInfo.GetDisplayName(), fso, this.Handle);

					if(dr == DialogResult.Cancel) return;
					else if(dr == DialogResult.Yes)
					{
						OnFileSave(null, EventArgs.Empty);
						if(pd.Modified) return;
					}
					else if(dr == DialogResult.No) { } // Changes are lost
				}
			}

			pd.Close();
			if(!bLocking) m_docMgr.CloseDatabase(pd);

			m_tbQuickFind.Items.Clear();
			m_tbQuickFind.Text = string.Empty;

			if(!bLocking)
			{
				m_docMgr.ActiveDatabase.UINeedsIconUpdate = true;
				UpdateUI(true, null, true, null, true, null, false);
			}

			// NativeMethods.ClearIconicBitmaps(this.Handle);

			if(FileClosed != null)
			{
				FileClosedEventArgs fcea = new FileClosedEventArgs(ioClosing);
				FileClosed(null, fcea);
			}
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
			if(!PrepareLock()) return; // Unable to lock

			SaveWindowState();

			PwDocument dsPrevActive = m_docMgr.ActiveDocument;

			foreach(PwDocument ds in m_docMgr.Documents)
			{
				PwDatabase pd = ds.Database;

				if(!pd.IsOpen) continue; // Nothing to lock

				IOConnectionInfo ioIoc = pd.IOConnectionInfo;
				Debug.Assert(ioIoc != null);

				m_docMgr.ActiveDocument = ds;

				CloseDocument(null, true, false);
				if(pd.IsOpen) return;

				ds.LockedIoc = ioIoc;
			}

			m_docMgr.ActiveDocument = dsPrevActive;
			UpdateUI(true, null, true, null, true, null, false);

			if(Program.Config.MainWindow.MinimizeAfterLocking &&
				!IsAtLeastOneFileOpen())
				UIUtil.SetWindowState(this, FormWindowState.Minimized);
		}

		private void SaveAllDocuments()
		{
			PwDocument dsPrevActive = m_docMgr.ActiveDocument;

			foreach(PwDocument ds in m_docMgr.Documents)
			{
				PwDatabase pd = ds.Database;

				if(!IsFileLocked(ds) && pd.Modified)
				{
					m_docMgr.ActiveDocument = ds;
					OnFileSave(null, EventArgs.Empty);
				}
			}

			m_docMgr.ActiveDocument = dsPrevActive;
			UpdateUI(false, null, true, null, true, null, false);
		}

		private bool CloseAllDocuments(bool bExiting)
		{
			if(UIIsInteractionBlocked()) { Debug.Assert(false); return false; }

			bool bProcessedAll = false, bSuccess = true;
			while(bProcessedAll == false)
			{
				bProcessedAll = true;

				foreach(PwDocument ds in m_docMgr.Documents)
				{
					if(ds.Database.IsOpen)
					{
						m_docMgr.ActiveDocument = ds;
						CloseDocument(null, false, bExiting);

						if(ds.Database.IsOpen)
						{
							bSuccess = false;
							break;
						}

						bProcessedAll = false;
						break;
					}
				}

				if(bSuccess == false) break;
			}

			return bSuccess;
		}

		private void RecreateUITabs()
		{
			m_bBlockTabChanged = true;

			m_tabMain.TabPages.Clear();

			m_tabMain.ImageList = null;
			if(m_ilTabImages != null) m_ilTabImages.Dispose();
			foreach(Image img in m_lTabImages) img.Dispose();
			m_lTabImages.Clear();

			List<TabPage> lPages = new List<TabPage>();
			for(int i = 0; i < m_docMgr.Documents.Count; ++i)
			{
				PwDocument ds = m_docMgr.Documents[i];

				TabPage tb = new TabPage();
				tb.Tag = ds;

				string strName, strTip;
				GetTabText(ds, out strName, out strTip);

				tb.Text = strName;
				tb.ToolTipText = strTip;

				if((ds == null) || (ds.Database == null) || !ds.Database.IsOpen ||
					UIUtil.ColorsEqual(ds.Database.Color, Color.Empty)) { }
				else
				{
					m_lTabImages.Add(UIUtil.CreateTabColorImage(ds.Database.Color,
						m_tabMain));
					tb.ImageIndex = m_lTabImages.Count - 1;
				}

				lPages.Add(tb);
			}

			int qSize = ((m_lTabImages.Count > 0) ? m_lTabImages[0].Height : 15);
			m_ilTabImages = UIUtil.BuildImageListUnscaled(m_lTabImages, qSize, qSize);
			m_tabMain.ImageList = m_ilTabImages;

			m_tabMain.TabPages.AddRange(lPages.ToArray());

			// m_tabMain.SelectedTab.Font = m_fontBoldUI;

			m_bBlockTabChanged = false;
		}

		private void SelectUITab()
		{
			m_bBlockTabChanged = true;

			PwDocument dsSelect = m_docMgr.ActiveDocument;
			foreach(TabPage tb in m_tabMain.TabPages)
			{
				if((PwDocument)tb.Tag == dsSelect)
				{
					m_tabMain.SelectedTab = tb;
					break;
				}
			}

			m_bBlockTabChanged = false;
		}

		public void MakeDocumentActive(PwDocument ds)
		{
			if(ds == null) { Debug.Assert(false); return; }

			ds.Database.UINeedsIconUpdate = true;

			UpdateUI(false, ds, true, null, true, null, false);

			RestoreWindowState(ds.Database);
			UpdateUIState(false);
		}

		private void GetTabText(PwDocument dsInfo, out string strName,
			out string strTip)
		{
			if(!IsFileLocked(dsInfo))
			{
				strTip = dsInfo.Database.IOConnectionInfo.Path;
				strName = UrlUtil.GetFileName(strTip);

				if(dsInfo.Database.Modified) strName += "*";

				if(dsInfo.Database.IsOpen)
				{
					if(dsInfo.Database.Name.Length > 0)
						strTip += "\r\n" + dsInfo.Database.Name;
				}
			}
			else // Locked
			{
				strTip = dsInfo.LockedIoc.Path;
				strName = UrlUtil.GetFileName(strTip);
				strName += " [" + KPRes.Locked + "]";
			}
		}

		private void UpdateUITabs()
		{
			foreach(TabPage tb in m_tabMain.TabPages)
			{
				PwDocument ds = (PwDocument)tb.Tag;
				string strName, strTip;

				GetTabText(ds, out strName, out strTip);

				tb.Text = strName;
				tb.ToolTipText = strTip;
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

			UpdateImageLists();

			if(bUpdateGroupList) UpdateGroupList(pgSelect);

			if(bUpdateEntryList) UpdateEntryList(pgEntrySource, false);
			else { Debug.Assert(pgEntrySource == null); }

			UpdateUIState(bSetModified, cOptFocus);
		}

		private void ShowSearchResultsStatusMessage()
		{
			string strItemsFound = m_lvEntries.Items.Count.ToString() + " " +
				KPRes.SearchItemsFoundSmall;

			SetStatusEx(strItemsFound);
		}

		private void MinimizeToTray(bool bMinimize)
		{
			this.Visible = !bMinimize;

			if(bMinimize == false) // Restore
			{
				// EnsureVisibleForegroundWindow(false, false); // Don't!

				if(this.WindowState == FormWindowState.Minimized)
					UIUtil.SetWindowState(this, (Program.Config.MainWindow.Maximized ?
						FormWindowState.Maximized : FormWindowState.Normal));
				else if(IsFileLocked(null) && !UIIsAutoUnlockBlocked())
					OnFileLock(null, EventArgs.Empty); // Unlock

				if(Program.Config.MainWindow.FocusQuickFindOnUntray)
					ResetDefaultFocus(null);
			}

			UpdateTrayIcon();
		}

		private void MinimizeToTrayAtStartIfEnabled(bool bFormLoading)
		{
			if(Program.Config.Application.Start.MinimizedAndLocked ||
				(Program.CommandLineArgs[AppDefs.CommandLineOptions.Minimize] != null))
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
				Program.Config.MainWindow.MinimizeToTray && (bFormLoading == false) &&
				IsAtLeastOneFileOpen())
			{
				MinimizeToTray(true);
			}
		}

		private void SelectFirstEntryIfNoneSelected()
		{
			if((m_lvEntries.Items.Count > 0) &&
				(m_lvEntries.SelectedIndices.Count == 0))
			{
				m_lvEntries.Items[0].Selected = true;
				m_lvEntries.Items[0].Focused = true;
			}
		}

		private void SelectEntries(PwObjectList<PwEntry> lEntries,
			bool bDeselectOthers, bool bFocusFirst)
		{
			m_bBlockEntrySelectionEvent = true;

			bool bFirst = true;
			for(int i = 0; i < m_lvEntries.Items.Count; ++i)
			{
				PwEntry pe = ((PwListItem)m_lvEntries.Items[i].Tag).Entry;
				if(pe == null) { Debug.Assert(false); continue; }

				bool bFound = false;
				foreach(PwEntry peFocus in lEntries)
				{
					if(pe == peFocus)
					{
						m_lvEntries.Items[i].Selected = true;

						if(bFirst && bFocusFirst)
						{
							m_lvEntries.Items[i].Focused = true;
							bFirst = false;
						}

						bFound = true;
						break;
					}
				}

				if(bDeselectOthers && !bFound)
					m_lvEntries.Items[i].Selected = false;
			}

			m_bBlockEntrySelectionEvent = false;
		}

		private PwGroup GetCurrentEntries()
		{
			PwGroup pg = new PwGroup(true, true);
			pg.IsVirtual = true;

			if(m_lvEntries.ShowGroups == false)
			{
				foreach(ListViewItem lvi in m_lvEntries.Items)
					pg.AddEntry(((PwListItem)lvi.Tag).Entry, false);
			}
			else // Groups
			{
				foreach(ListViewGroup lvg in m_lvEntries.Groups)
					foreach(ListViewItem lvi in lvg.Items)
						pg.AddEntry(((PwListItem)lvi.Tag).Entry, false);
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

		private void EnsureVisibleSelected(bool bLastMatchingEntry)
		{
			PwEntry pe = GetSelectedEntry(true, bLastMatchingEntry);
			if(pe == null) return;

			EnsureVisibleEntry(pe.Uuid);
		}

		private void RemoveEntriesFromList(List<PwEntry> lEntries, bool bLockUIUpdate)
		{
			Debug.Assert(lEntries != null); if(lEntries == null) return;
			if(lEntries.Count == 0) return;

			RemoveEntriesFromList(lEntries.ToArray(), bLockUIUpdate);
		}

		private void RemoveEntriesFromList(PwEntry[] vEntries, bool bLockUIUpdate)
		{
			Debug.Assert(vEntries != null); if(vEntries == null) return;
			if(vEntries.Length == 0) return;

			if(bLockUIUpdate) m_lvEntries.BeginUpdate();

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

			// Refresh alternating item background colors
			UIUtil.SetAlternatingBgColors(m_lvEntries, m_clrAlternateItemBgColor,
				Program.Config.MainWindow.EntryListAlternatingBgColors);

			if(bLockUIUpdate) m_lvEntries.EndUpdate();
		}

		private bool PreSaveValidate(PwDatabase pd)
		{
			if(m_bForceSave) return true;

			byte[] pbOnDisk = WinUtil.HashFile(pd.IOConnectionInfo);

			if((pbOnDisk != null) && (pd.HashOfFileOnDisk != null) &&
				!MemUtil.ArraysEqual(pbOnDisk, pd.HashOfFileOnDisk))
			{
				DialogResult dr = AskIfSynchronizeInstead(pd.IOConnectionInfo);
				if(dr == DialogResult.Yes) // Synchronize
				{
					bool? b = ImportUtil.Synchronize(pd, this, pd.IOConnectionInfo,
						true, this);
					UpdateUI(false, null, true, null, true, null, false);
					if(b.HasValue) SetStatusEx(b.Value ? KPRes.SyncSuccess : KPRes.SyncFailed);
					return false;
				}
				else if(dr == DialogResult.Cancel) return false;
				else { Debug.Assert(dr == DialogResult.No); }
			}

			return true;
		}

		private DialogResult AskIfSynchronizeInstead(IOConnectionInfo ioc)
		{
			VistaTaskDialog dlg = new VistaTaskDialog(this.Handle);

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
			dlg.AddButton((int)DialogResult.Cancel, KPRes.CancelCmd, KPRes.FileSaveQOpCancel);

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

		private void ActivateNextDocumentEx()
		{
			if(m_tabMain.TabPages.Count > 1)
				m_tabMain.SelectedIndex = ((m_tabMain.SelectedIndex + 1) %
					m_tabMain.TabPages.Count);
		}

		private bool HandleMainWindowKeyMessage(KeyEventArgs e, bool bDown)
		{
			if(e == null) { Debug.Assert(false); return false; }

			bool bHandled = false;

			if(e.Control)
			{
				if(e.KeyCode == Keys.Tab)
				{
					if(bDown) ActivateNextDocumentEx();
					bHandled = true;
				}
				// When changing Ctrl+E, also change the tooltip of the quick search box
				else if(e.KeyCode == Keys.E) // Standard key for quick searches
				{
					ResetDefaultFocus(m_tbQuickFind.Control); // In both cases (RTF)
					bHandled = true;
				}
				else if(e.KeyCode == Keys.H)
				{
					if(bDown) ToggleFieldAsterisks(AceColumnType.Password);
					bHandled = true;
				}
				else if(e.KeyCode == Keys.J)
				{
					if(bDown) ToggleFieldAsterisks(AceColumnType.UserName);
					bHandled = true;
				}
			}

			if(bHandled) { e.Handled = true; e.SuppressKeyPress = true; }
			return bHandled;
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
				try { ResetDefaultFocus(null); } // Set focus on unblock
				catch(Exception) { Debug.Assert(false); }
			}

			Application.DoEvents(); // Allow controls update/redraw
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
			else { Debug.Assert(pgRecycleBin.Uuid.EqualsValue(pdContext.RecycleBinUuid)); }
		}

		private void DeleteSelectedEntries()
		{
			PwEntry[] vSelected = GetSelectedEntries();
			if((vSelected == null) || (vSelected.Length == 0)) return;

			PwDatabase pd = m_docMgr.ActiveDatabase;
			PwGroup pgRecycleBin = pd.RootGroup.FindGroup(pd.RecycleBinUuid, true);
			bool bShiftPressed = ((Control.ModifierKeys & Keys.Shift) != Keys.None);

			bool bAtLeastOnePermanent = false;
			if(pd.RecycleBinEnabled == false) bAtLeastOnePermanent = true;
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

			if(bAtLeastOnePermanent)
			{
				bool bSingle = (vSelected.Length == 1);

				VistaTaskDialog dlg = new VistaTaskDialog(this.Handle);
				dlg.CommandLinks = false;
				dlg.Content = EntryUtil.CreateSummaryList(null, vSelected);
				dlg.MainInstruction = (bSingle ? KPRes.DeleteEntriesQuestionSingle :
					KPRes.DeleteEntriesQuestion);
				dlg.SetIcon(VtdCustomIcon.Question);
				dlg.WindowTitle = PwDefs.ShortProductName;
				dlg.AddButton((int)DialogResult.OK, KPRes.DeleteCmd, null);
				dlg.AddButton((int)DialogResult.Cancel, KPRes.CancelCmd, null);

				if(dlg.ShowDialog())
				{
					if(dlg.Result == (int)DialogResult.Cancel) return;
				}
				else
				{
					if(!MessageService.AskYesNo(bSingle ? KPRes.DeleteEntriesQuestionSingle :
						KPRes.DeleteEntriesQuestion, bSingle ? KPRes.DeleteEntriesTitleSingle :
						KPRes.DeleteEntriesTitle))
						return;
				}
			}

			bool bUpdateGroupList = false;
			DateTime dtNow = DateTime.Now;
			foreach(PwEntry pe in vSelected)
			{
				PwGroup pgParent = pe.ParentGroup;
				if(pgParent == null) continue; // Can't remove

				pgParent.Entries.Remove(pe);

				bool bPermanent = false;
				if(pd.RecycleBinEnabled == false) bPermanent = true;
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
					pe.Touch(false);
				}
			}

			RemoveEntriesFromList(vSelected, true);
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
			if(pd.RecycleBinEnabled == false) bPermanent = true;
			else if(bShiftPressed) bPermanent = true;
			else if(pgRecycleBin == null) { }
			else if(pg == pgRecycleBin) bPermanent = true;
			else if(pg.IsContainedIn(pgRecycleBin)) bPermanent = true;
			else if(pgRecycleBin.IsContainedIn(pg)) bPermanent = true;

			if(bPermanent)
			{
				VistaTaskDialog dlg = new VistaTaskDialog(this.Handle);
				dlg.CommandLinks = false;
				dlg.Content = KPRes.DeleteGroupInfo + EntryUtil.CreateSummaryList(pg, true);
				dlg.MainInstruction = KPRes.DeleteGroupQuestion;
				dlg.SetIcon(VtdCustomIcon.Question);
				dlg.WindowTitle = PwDefs.ShortProductName;
				dlg.AddButton((int)DialogResult.OK, KPRes.DeleteCmd, null);
				dlg.AddButton((int)DialogResult.Cancel, KPRes.CancelCmd, null);

				if(dlg.ShowDialog())
				{
					if(dlg.Result == (int)DialogResult.Cancel) return;
				}
				else
				{
					string strText = KPRes.DeleteGroupInfo + MessageService.NewParagraph +
						KPRes.DeleteGroupQuestion;
					if(!MessageService.AskYesNo(strText, KPRes.DeleteGroupTitle))
						return;
				}
			}

			pgParent.Groups.Remove(pg);

			if(bPermanent)
			{
				pg.DeleteAllObjects(pd);

				PwDeletedObject pdo = new PwDeletedObject(pg.Uuid, DateTime.Now);
				pd.DeletedObjects.Add(pdo);
			}
			else // Recycle
			{
				bool bDummy = false;
				EnsureRecycleBin(ref pgRecycleBin, pd, ref bDummy);

				pgRecycleBin.AddGroup(pg, true, true);
				pg.Touch(false);
			}

			UpdateUI(false, null, true, pgParent, true, null, true);
		}

		private void EmptyRecycleBin()
		{
			PwDatabase pd = m_docMgr.ActiveDatabase;
			PwGroup pg = pd.RootGroup.FindGroup(pd.RecycleBinUuid, true);
			if(pg == null) { Debug.Assert(false); return; }
			if(pg != GetSelectedGroup()) { Debug.Assert(false); return; }

			VistaTaskDialog dlg = new VistaTaskDialog(this.Handle);
			dlg.CommandLinks = false;
			dlg.Content = EntryUtil.CreateSummaryList(pg, false);
			dlg.MainInstruction = KPRes.EmptyRecycleBinQuestion;
			dlg.SetIcon(VtdCustomIcon.Question);
			dlg.WindowTitle = PwDefs.ShortProductName;
			dlg.AddButton((int)DialogResult.OK, KPRes.DeleteCmd, null);
			dlg.AddButton((int)DialogResult.Cancel, KPRes.CancelCmd, null);

			if(dlg.ShowDialog())
			{
				if(dlg.Result == (int)DialogResult.Cancel) return;
			}
			else
			{
				if(!MessageService.AskYesNo(KPRes.EmptyRecycleBinQuestion))
					return;
			}

			pg.DeleteAllObjects(pd);

			UpdateUI(false, null, true, pg, true, null, true);
		}


		// private static bool GroupOnlyContainsTans(PwGroup pg, bool bAllowSubgroups)
		// {
		//	if(!bAllowSubgroups && (pg.Groups.UCount > 0))
		//		return false;
		//
		//	foreach(PwEntry pe in pg.Entries)
		//	{
		//		if(!PwDefs.IsTanEntry(pe)) return false;
		//	}
		//
		//	return true;
		// }

		// private static string GetGroupSuffixText(PwGroup pg)
		// {
		// if(pg == null) { Debug.Assert(false); return string.Empty; }
		// if(pg.Entries.UCount == 0) return string.Empty;
		// if(GroupOnlyContainsTans(pg, true) == false) return string.Empty;

		// DateTime dtNow = DateTime.Now;
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

			if(m_vCustomToolBarButtons.Count == 0)
			{
				m_tsSepCustomToolBar = new ToolStripSeparator();
				m_toolMain.Items.Add(m_tsSepCustomToolBar);
			}

			ToolStripButton btn = new ToolStripButton(strName);
			btn.Tag = strID;
			btn.Click += OnCustomToolBarButtonClicked;
			if(!string.IsNullOrEmpty(strDesc)) btn.ToolTipText = strDesc;

			m_toolMain.Items.Add(btn);
			m_vCustomToolBarButtons.Add(btn);
		}

		public void RemoveCustomToolBarButton(string strID)
		{
			if(string.IsNullOrEmpty(strID)) { Debug.Assert(false); return; } // No throw

			foreach(ToolStripButton tb in m_vCustomToolBarButtons)
			{
				string str = (tb.Tag as string);
				if(string.IsNullOrEmpty(str)) { Debug.Assert(false); continue; }

				if(str.Equals(strID, StrUtil.CaseIgnoreCmp))
				{
					tb.Click -= OnCustomToolBarButtonClicked;
					m_toolMain.Items.Remove(tb);
					m_vCustomToolBarButtons.Remove(tb);
					break;
				}
			}

			if((m_vCustomToolBarButtons.Count == 0) && (m_tsSepCustomToolBar != null))
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

			Program.TriggerSystem.RaiseEvent(EcasEventIDs.CustomTbButtonClicked, strID);
		}

		private IOConnectionInfo IocFromCommandLine()
		{
			CommandLineArgs args = Program.CommandLineArgs;
			IOConnectionInfo ioc = IOConnectionInfo.FromPath(args.FileName);

			if(ioc.IsLocalFile()) // Expand relative path to absolute
				ioc.Path = UrlUtil.MakeAbsolutePath(UrlUtil.EnsureTerminatingSeparator(
					Directory.GetCurrentDirectory(), false) + "Sentinel", ioc.Path);

			if(args[AppDefs.CommandLineOptions.IoCredFromRecent] != null)
			{
				for(uint u = 0; u < m_mruList.ItemCount; ++u)
				{
					KeyValuePair<string, object> kvp = m_mruList.GetItem(u);
					if(kvp.Value == null) { Debug.Assert(false); continue; }
					IOConnectionInfo iocMru = (kvp.Value as IOConnectionInfo);
					if(iocMru == null) { Debug.Assert(false); continue; }

					if(iocMru.Path.Equals(ioc.Path, StrUtil.CaseIgnoreCmp))
					{
						ioc.UserName = iocMru.UserName;
						ioc.Password = iocMru.Password;
						ioc.CredSaveMode = iocMru.CredSaveMode;
						break;
					}
				}
			}

			string strUserName = args[AppDefs.CommandLineOptions.IoCredUserName];
			if(strUserName != null) ioc.UserName = strUserName;

			string strPassword = args[AppDefs.CommandLineOptions.IoCredPassword];
			if(strPassword != null) ioc.Password = strPassword;

			return ioc;
		}

		private static void SetLastUsedFile(IOConnectionInfo ioc)
		{
			if(ioc == null) { Debug.Assert(false); return; }

			if(Program.Config.Application.Start.OpenLastFile)
				Program.Config.Application.LastUsedFile = ioc.CloneDeep();
			else
				Program.Config.Application.LastUsedFile = new IOConnectionInfo();
		}

		private static void RememberKeyFilePath(PwDatabase pwDb)
		{
			if(pwDb == null) { Debug.Assert(false); return; }

			IUserKey kcpFile = pwDb.MasterKey.GetUserKey(typeof(KcpKeyFile));
			IUserKey kcpCustom = pwDb.MasterKey.GetUserKey(typeof(KcpCustomKey));

			if(kcpFile != null)
				Program.Config.Defaults.SetKeySource(pwDb.IOConnectionInfo,
					((KcpKeyFile)kcpFile).Path, true);
			else if(kcpCustom != null)
				Program.Config.Defaults.SetKeySource(pwDb.IOConnectionInfo,
					((KcpCustomKey)kcpCustom).Name, false);
			else
				Program.Config.Defaults.SetKeySource(pwDb.IOConnectionInfo,
					null, false);
		}

		private void SerializeMruList(bool bStore)
		{
			AceMru aceMru = Program.Config.Application.MostRecentlyUsed;

			if(bStore)
			{
				aceMru.MaxItemCount = m_mruList.MaxItemCount;

				aceMru.Items.Clear();
				// Count <= max is not guaranteed, therefore take minimum of both
				for(uint uMru = 0; uMru < Math.Min(m_mruList.MaxItemCount,
					m_mruList.ItemCount); ++uMru)
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

				for(int iMru = 0; iMru < Math.Min((int)m_mruList.MaxItemCount,
					aceMru.Items.Count); ++iMru)
				{
					IOConnectionInfo ioMru = aceMru.Items[iMru];
					m_mruList.AddItem(ioMru.GetDisplayName(), ioMru.CloneDeep(), false);
				}

				m_mruList.UpdateMenu();
			}
		}

		public void RedirectActivationPush(Form formTarget)
		{
			m_vRedirectActivation.Push(formTarget);
		}

		public void RedirectActivationPop()
		{
			if(m_vRedirectActivation.Count == 0) { Debug.Assert(false); return; }
			m_vRedirectActivation.Pop();
		}

		private bool ChangeMasterKey(PwDatabase pdOf)
		{
			if(!AppPolicy.Try(AppPolicyId.ChangeMasterKey)) return false;

			PwDatabase pd = (pdOf ?? m_docMgr.ActiveDatabase);
			if((pd == null) || !pd.IsOpen) { Debug.Assert(false); return false; }

			if(!AppPolicy.Current.ChangeMasterKeyNoKey)
			{
				if(!KeyUtil.ReAskKey(pd, true)) return false;
			}

			KeyCreationForm kcf = new KeyCreationForm();
			kcf.InitEx(pd.IOConnectionInfo, false);

			if(UIUtil.ShowDialogNotValue(kcf, DialogResult.OK)) return false; // No UI update

			pd.MasterKey = kcf.CompositeKey;
			pd.MasterKeyChanged = DateTime.Now;

			MessageService.ShowInfoEx(PwDefs.ShortProductName + " - " +
				KPRes.KeyChanged, KPRes.MasterKeyChanged, KPRes.MasterKeyChangedSavePrompt);

			UIUtil.DestroyForm(kcf);
			return true; // No UI update
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

				for(int i = 0; i < Math.Min(m_lvEntries.Columns.Count,
					vDisplayIndices.Length); ++i)
				{
					int nIdx = vDisplayIndices[i];
					if((nIdx >= 0) && (nIdx < m_lvEntries.Columns.Count))
						m_lvEntries.Columns[i].DisplayIndex = nIdx;
				}

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
					if(!bFormatForDisplay) str = pe.Strings.ReadSafe(PwDefs.NotesField);
					else str = StrUtil.MultiToSingleLine(pe.Strings.ReadSafe(PwDefs.NotesField));
					break;
				case AceColumnType.CreationTime: str = TimeUtil.ToDisplayString(pe.CreationTime); break;
				case AceColumnType.LastAccessTime: str = TimeUtil.ToDisplayString(pe.LastAccessTime); break;
				case AceColumnType.LastModificationTime: str = TimeUtil.ToDisplayString(pe.LastModificationTime); break;
				case AceColumnType.ExpiryTime:
					if(pe.Expires) str = TimeUtil.ToDisplayString(pe.ExpiryTime);
					else str = m_strNeverExpiresText;
					break;
				case AceColumnType.Uuid: str = pe.Uuid.ToHexString(); break;
				case AceColumnType.Attachment: str = pe.Binaries.KeysToString(); break;
				case AceColumnType.CustomString:
					if(!bFormatForDisplay) str = pe.Strings.ReadSafe(col.CustomName);
					else str = StrUtil.MultiToSingleLine(pe.Strings.ReadSafe(col.CustomName));
					break;
				case AceColumnType.PluginExt:
					if(!bFormatForDisplay) str = Program.ColumnProviderPool.GetCellData(col.CustomName, pe);
					else str = StrUtil.MultiToSingleLine(Program.ColumnProviderPool.GetCellData(col.CustomName, pe));
					break;
				case AceColumnType.OverrideUrl: str = pe.OverrideUrl; break;
				case AceColumnType.Tags:
					str = StrUtil.TagsToString(pe.Tags, true);
					break;
				case AceColumnType.ExpiryTimeDateOnly:
					if(pe.Expires) str = TimeUtil.ToDisplayStringDateOnly(pe.ExpiryTime);
					else str = m_strNeverExpiresText;
					break;
				case AceColumnType.Size:
					str = StrUtil.FormatDataSizeKB(pe.GetSize());
					break;
				case AceColumnType.HistoryCount:
					str = pe.History.UCount.ToString();
					break;
				default: Debug.Assert(false); str = string.Empty; break;
			}

			if(Program.Config.MainWindow.EntryListShowDerefData)
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

		private static bool IsColumnHidden(AceColumnType t)
		{
			List<AceColumn> l = Program.Config.MainWindow.EntryListColumns;
			bool bHidden = (t == AceColumnType.Password);

			foreach(AceColumn c in l)
			{
				if(c.Type == t) { bHidden = c.HideWithAsterisks; break; }
			}

			return bHidden;
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
		}

		private void EditSelectedEntry(bool bSwitchToHistoryTab)
		{
			PwEntry pe = GetSelectedEntry(false);
			if(pe == null) return; // Do not assert

			PwDatabase pwDb = m_docMgr.ActiveDatabase;
			PwEntryForm pForm = new PwEntryForm();
			pForm.InitEx(pe, PwEditMode.EditExistingEntry, pwDb, m_ilCurrentIcons,
				false, false);

			pForm.InitSwitchToHistoryTab = bSwitchToHistoryTab;

			if(pForm.ShowDialog() == DialogResult.OK)
			{
				bool bUpdImg = pwDb.UINeedsIconUpdate;
				RefreshEntriesList(); // Update entry
				UpdateUI(false, null, bUpdImg, null, false, null, pForm.HasModifiedEntry);
			}
			else
			{
				bool bUpdImg = pwDb.UINeedsIconUpdate;
				RefreshEntriesList(); // Update last access time
				UpdateUI(false, null, bUpdImg, null, false, null, false);
			}
			UIUtil.DestroyForm(pForm);
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

		private void OnShowEntriesByTag(object sender, DynamicMenuEventArgs e)
		{
			string strTag = (e.Tag as string);
			if(strTag == null) { Debug.Assert(false); return; }
			if(strTag.Length == 0) return; // No assert

			PwDatabase pd = m_docMgr.ActiveDatabase;
			if((pd == null) || !pd.IsOpen) { Debug.Assert(false); return; }

			PwObjectList<PwEntry> vEntries = new PwObjectList<PwEntry>();
			pd.RootGroup.FindEntriesByTag(strTag, vEntries, true);

			PwGroup pgResults = new PwGroup(true, true);
			pgResults.IsVirtual = true;
			foreach(PwEntry pe in vEntries) pgResults.AddEntry(pe, false);

			UpdateUI(false, null, false, null, true, pgResults, false);
		}

		private void UpdateTagsMenu(DynamicMenu dm, bool bWithSeparator, bool bPrefixTag,
			bool bRequireEntrySelected, bool bEnableOnlySelected)
		{
			if(dm == null) { Debug.Assert(false); return; }
			dm.Clear();

			if(bWithSeparator) dm.AddSeparator();

			string strNoTags = "(" + KPRes.TagsNotFound + ")";
			PwDatabase pd = m_docMgr.ActiveDatabase;
			if(!pd.IsOpen)
			{
				ToolStripMenuItem tsmi = dm.AddItem(strNoTags, null, string.Empty);
				tsmi.Enabled = false;
				return;
			}

			List<string> vTags = pd.RootGroup.BuildEntryTagsList(true);
			string strPrefix = KPRes.Tag + ": ";
			Image imgIcon = Properties.Resources.B16x16_KNotes;
			bool bEnable = (m_lvEntries.SelectedIndices.Count > 0);

			List<string> vEnabledTags = null;
			if(bEnableOnlySelected)
			{
				PwGroup pgSel = GetSelectedEntriesAsGroup();
				vEnabledTags = pgSel.BuildEntryTagsList(true);
			}

			for(int i = 0; i < vTags.Count; ++i)
			{
				string strTag = vTags[i];
				ToolStripMenuItem tsmi = dm.AddItem(bPrefixTag ? (strPrefix + strTag) :
					strTag, imgIcon, strTag);

				if(bRequireEntrySelected && !bEnable) tsmi.Enabled = false;
				else if(vEnabledTags != null)
				{
					if(vEnabledTags.IndexOf(strTag) < 0) tsmi.Enabled = false;
				}
			}

			if(vTags.Count == 0)
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

			if(!string.IsNullOrEmpty(strTag))
				AddOrRemoveTagsToFromSelectedEntries(strTag, true);
		}

		private void OnRemoveEntryTag(object sender, DynamicMenuEventArgs e)
		{
			string strTag = (e.Tag as string);
			if(strTag == null) { Debug.Assert(false); return; }
			if(strTag.Length == 0) return;

			AddOrRemoveTagsToFromSelectedEntries(strTag, false);
		}

		private void AddOrRemoveTagsToFromSelectedEntries(string strTags, bool bAdd)
		{
			if(strTags == null) { Debug.Assert(false); return; }
			if(strTags.Length == 0) return;

			PwDatabase pd = m_docMgr.ActiveDatabase;
			PwEntry[] vEntries = GetSelectedEntries();
			if((vEntries == null) || (vEntries.Length == 0)) return;

			List<string> vToProcess = StrUtil.StringToTags(strTags);
			List<PwEntry> vModified = new List<PwEntry>();

			foreach(string strTag in vToProcess)
			{
				foreach(PwEntry pe in vEntries)
				{
					if(bAdd && !pe.HasTag(strTag)) // Add tag
					{
						if(vModified.IndexOf(pe) < 0) // Backup entries only once
						{
							pe.CreateBackup(pd);
							vModified.Add(pe);
						}

						if(!pe.AddTag(strTag)) { Debug.Assert(false); }
					}
					else if(!bAdd && pe.HasTag(strTag)) // Remove tag
					{
						if(vModified.IndexOf(pe) < 0) // Backup entries only once
						{
							pe.CreateBackup(pd);
							vModified.Add(pe);
						}

						if(!pe.RemoveTag(strTag)) { Debug.Assert(false); }
					}
				}
			}

			foreach(PwEntry pe in vModified) { pe.Touch(true, false); }

			RefreshEntriesList();
			UpdateUIState(vModified.Count > 0);
		}

		internal static void AutoAdjustMemProtSettings(PwDatabase pd,
			SearchParameters sp)
		{
			if(pd == null) { Debug.Assert(false); return; }

			if(sp != null)
			{
				if(sp.SearchInTitles) pd.MemoryProtection.ProtectTitle = false;
				if(sp.SearchInUserNames) pd.MemoryProtection.ProtectUserName = false;
				// if(sp.SearchInPasswords) pd.MemoryProtection.ProtectPassword = false;
				if(sp.SearchInUrls) pd.MemoryProtection.ProtectUrl = false;
				if(sp.SearchInNotes) pd.MemoryProtection.ProtectNotes = false;
			}
		}

		private static bool? m_bCachedSelfTestResult = null;
		private static bool PerformSelfTest()
		{
			if(m_bCachedSelfTestResult.HasValue)
				return m_bCachedSelfTestResult.Value;

			bool bResult = true;
			try { SelfTest.Perform(); }
			catch(Exception exSelfTest)
			{
				MessageService.ShowWarning(KPRes.SelfTestFailed, exSelfTest);
				bResult = false;
			}

			m_bCachedSelfTestResult = bResult;
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

		private bool CreateColorizedIcon(Icon icoBase, bool bLargeIcon,
			ref KeyValuePair<Color, Icon> kvpStore, out Icon icoAssignable,
			out Icon icoDisposable)
		{
			PwDatabase pd = m_docMgr.ActiveDatabase;
			Color clrNew = Color.Empty;
			if((pd == null) || !pd.IsOpen || pd.Color.IsEmpty || (pd.Color.A < 255)) { }
			else clrNew = pd.Color;

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
					UIUtil.CreateColorizedIcon(icoBase, clrNew,
						bLargeIcon ? 0 : 32));
				icoAssignable = kvpStore.Value;
			}

			return true;
		}

		private void SetObjectsDeletedStatus(uint uDeleted, bool bDbMntnc)
		{
			string str = (StrUtil.ReplaceCaseInsensitive(KPRes.ObjectsDeleted,
				@"{PARAM}", uDeleted.ToString()) + ".");
			SetStatusEx(str);
			if(!bDbMntnc || !Program.Config.UI.ShowDbMntncResultsDialog) return;

			VistaTaskDialog dlg = new VistaTaskDialog(this.Handle);
			dlg.CommandLinks = false;
			dlg.Content = str;
			dlg.SetIcon(VtdIcon.Information);
			dlg.VerificationText = KPRes.DialogNoShowAgain;
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
		}

		private static void OnFormLoadParallelAsync(object stateInfo)
		{
			try
			{
				string strShInstUtil = UrlUtil.GetFileDirectory(
					WinUtil.GetExecutable(), true, false) + AppDefs.ShInstUtil;

				// Unblock the application such that the user isn't
				// prompted next time anymore
				WinUtil.RemoveZoneIdentifier(WinUtil.GetExecutable());
				WinUtil.RemoveZoneIdentifier(AppHelp.LocalHelpFile);
				WinUtil.RemoveZoneIdentifier(strShInstUtil);
			}
			catch(Exception) { Debug.Assert(false); }
		}

		private bool IsCommandTypeInvokable(MainAppState? sContext, AppCommandType t)
		{
			MainAppState s = (sContext.HasValue ? sContext.Value : GetMainAppState());

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

			m_ctxTrayTray.Enabled = IsCommandTypeInvokable(s, AppCommandType.Window);
			m_ctxTrayOptions.Enabled = (IsCommandTypeInvokable(s,
				AppCommandType.Window) && bOptions);
			m_ctxTrayLock.Enabled = IsCommandTypeInvokable(s, AppCommandType.Lock);
			m_ctxTrayFileExit.Enabled = IsCommandTypeInvokable(s, AppCommandType.Window);
		}

		internal static SprContext GetEntryListSprContext(PwEntry pe,
			PwDatabase pd)
		{
			SprContext ctx = new SprContext(pe, pd, SprCompileFlags.Deref);
			ctx.ForcePlainTextPasswords = false;
			return ctx;
		}
	}
}
