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
using System.Text;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

using KeePass.App;
using KeePass.Resources;
using KeePass.DataExchange;
using KeePass.UI;
using KeePass.Util;
using KeePass.Plugins;

using KeePassLib;
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
		private ListViewGroup m_lvgLastEntryGroup = null;
		private bool m_bEntryGrouping = false;
		private DateTime m_dtCachedNow = DateTime.Now;
		private bool m_bOnlyTans = false;
		private Font m_fontExpired = new Font(FontFamily.GenericSansSerif, 8.25f);
		private Point m_ptLastEntriesMouseClick = new Point(0, 0);
		private RichTextBoxContextMenu m_ctxEntryPreviewContextMenu = new RichTextBoxContextMenu();
		private DynamicMenu m_dynCustomStrings;
		private bool m_bShowClassicSaveWarning = false;

		private string m_strLockedIoc = string.Empty;
		private LockedStateInfo m_lockedState = new LockedStateInfo();

		private MemoryProtectionConfig m_viewHideFields = new MemoryProtectionConfig();

		private bool[] m_vShowColumns = new bool[(int)AppDefs.ColumnID.Count];
		private MruList m_mruList = new MruList();

		private SessionLockNotifier m_sessionLockNotifier = new SessionLockNotifier();

		private PluginDefaultHost m_pluginDefaultHost = new PluginDefaultHost();
		private PluginManager m_pluginManager = new PluginManager();

		private int m_nLockTimerMax = 0;
		private int m_nLockTimerCur = 0;

		private int m_nClipClearMax = 0;
		private int m_nClipClearCur = -1;

		private string m_strNeverExpiresText = string.Empty;

		private bool m_bSimpleTanView = true;
		private bool m_bShowTanIndices = true;

		private Image m_imgFileSaveEnabled = null;
		private Image m_imgFileSaveDisabled = null;

		private bool m_bIsAutoTyping = false;

		private int m_nAppMessage = (int)Program.ApplicationMessage;

		private List<KeyValuePair<ToolStripItem, ToolStripItem>> m_vLinkedToolStripItems =
			new List<KeyValuePair<ToolStripItem, ToolStripItem>>();

		private FormWindowState m_fwsLast = FormWindowState.Normal;

		public enum KeeExportFormat : int
		{
			PlainXml = 0,
			Html,
			Kdb3,
			UseXsl
		}

		private class LockedStateInfo
		{
			public PwUuid SelectedGroupUUID = new PwUuid(false);
			public PwUuid TopVisibleGroup = new PwUuid(false);
			public PwUuid TopVisibleEntry = new PwUuid(false);
		}

		public PwDatabase Database { get { return m_pwDatabase; } }
		public ImageList ClientIcons { get { return m_ilClientIcons; } }

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

		/// <summary>
		/// Get a reference to the plugin host. This should not be used by plugins.
		/// Plugins should directly use the reference they get in the initialization
		/// function.
		/// </summary>
		public PluginDefaultHost PluginHost
		{
			get { return m_pluginDefaultHost; }
		}

		/// <summary>
		/// Check if the main window is trayed (i.e. only the tray icon is visible).
		/// </summary>
		/// <returns>Returns <c>true</c>, if the window is trayed.</returns>
		public bool IsTrayed()
		{
			return !this.Visible;
		}

		public bool IsFileLocked()
		{
			return (m_strLockedIoc.Length != 0);
		}

		private void CleanUpEx()
		{
			m_nClipClearCur = -1;
			if(AppConfigEx.GetBool(AppDefs.ConfigKeys.ClipboardAutoClearOnExit))
				ClipboardUtil.ClearIfOwner();

			SaveConfig();

			m_sessionLockNotifier.Uninstall();
			HotKeyManager.UnregisterAll();
			m_pluginManager.UnloadAllPlugins();

			EntryTemplates.Clear();
			m_dynCustomStrings.MenuClick -= this.OnCopyCustomString;

			m_ctxEntryPreviewContextMenu.Detach();
		}

		/// <summary>
		/// Save the current configuration. The configuration is saved using the
		/// cascading configuration files mechanism and the default paths are used.
		/// </summary>
		public void SaveConfig()
		{
			FormWindowState ws = this.WindowState;

			if(ws == FormWindowState.Normal)
			{
				AppConfigEx.SetValue(AppDefs.ConfigKeys.MainWindowPositionX, this.Location.X);
				AppConfigEx.SetValue(AppDefs.ConfigKeys.MainWindowPositionY, this.Location.Y);
				AppConfigEx.SetValue(AppDefs.ConfigKeys.MainWindowWidth, this.Size.Width);
				AppConfigEx.SetValue(AppDefs.ConfigKeys.MainWindowHeight, this.Size.Height);
			}

			if((ws == FormWindowState.Normal) || (ws == FormWindowState.Maximized))
			{
				AppConfigEx.SetValue(AppDefs.ConfigKeys.MainWindowHorzSplitter, m_splitHorizontal.SplitterDistance);
				AppConfigEx.SetValue(AppDefs.ConfigKeys.MainWindowVertSplitter, m_splitVertical.SplitterDistance);
			}

			AppConfigEx.SetValue(AppDefs.ConfigKeys.MainWindowMaximized,
				ws == FormWindowState.Maximized);

			AppConfigEx.SetValue(AppDefs.ConfigKeys.MainWindowLayoutSideBySide,
				(m_splitHorizontal.Orientation != Orientation.Horizontal));

			AppConfigEx.SetValue(AppDefs.ConfigKeys.TitleColumnWidth,
				(uint)m_lvEntries.Columns[(int)AppDefs.ColumnID.Title].Width);
			AppConfigEx.SetValue(AppDefs.ConfigKeys.UserNameColumnWidth,
				(uint)m_lvEntries.Columns[(int)AppDefs.ColumnID.UserName].Width);
			AppConfigEx.SetValue(AppDefs.ConfigKeys.PasswordColumnWidth,
				(uint)m_lvEntries.Columns[(int)AppDefs.ColumnID.Password].Width);
			AppConfigEx.SetValue(AppDefs.ConfigKeys.UrlColumnWidth,
				(uint)m_lvEntries.Columns[(int)AppDefs.ColumnID.Url].Width);
			AppConfigEx.SetValue(AppDefs.ConfigKeys.NotesColumnWidth,
				(uint)m_lvEntries.Columns[(int)AppDefs.ColumnID.Notes].Width);
			AppConfigEx.SetValue(AppDefs.ConfigKeys.CreationTimeColumnWidth,
				(uint)m_lvEntries.Columns[(int)AppDefs.ColumnID.CreationTime].Width);
			AppConfigEx.SetValue(AppDefs.ConfigKeys.LastAccessTimeColumnWidth,
				(uint)m_lvEntries.Columns[(int)AppDefs.ColumnID.LastAccessTime].Width);
			AppConfigEx.SetValue(AppDefs.ConfigKeys.LastModTimeColumnWidth,
				(uint)m_lvEntries.Columns[(int)AppDefs.ColumnID.LastModificationTime].Width);
			AppConfigEx.SetValue(AppDefs.ConfigKeys.ExpireTimeColumnWidth,
				(uint)m_lvEntries.Columns[(int)AppDefs.ColumnID.ExpiryTime].Width);
			AppConfigEx.SetValue(AppDefs.ConfigKeys.UuidColumnWidth,
				(uint)m_lvEntries.Columns[(int)AppDefs.ColumnID.Uuid].Width);
			AppConfigEx.SetValue(AppDefs.ConfigKeys.AttachmentColumnWidth,
				(uint)m_lvEntries.Columns[(int)AppDefs.ColumnID.Attachment].Width);

			Debug.Assert(m_bSimpleTanView == m_menuViewTanSimpleList.Checked);
			AppConfigEx.SetValue(AppDefs.ConfigKeys.TanSimpleList, m_bSimpleTanView);
			Debug.Assert(m_bShowTanIndices == m_menuViewTanIndices.Checked);
			AppConfigEx.SetValue(AppDefs.ConfigKeys.TanIndices, m_bShowTanIndices);

			AppConfigEx.SetValue(AppDefs.ConfigKeys.HideTitles, m_menuViewHideTitles.Checked);
			AppConfigEx.SetValue(AppDefs.ConfigKeys.HideUserNames, m_menuViewHideUserNames.Checked);
			AppConfigEx.SetValue(AppDefs.ConfigKeys.HidePasswords, m_menuViewHidePasswords.Checked);
			AppConfigEx.SetValue(AppDefs.ConfigKeys.HideUrls, m_menuViewHideURLs.Checked);
			AppConfigEx.SetValue(AppDefs.ConfigKeys.HideNotes, m_menuViewHideNotes.Checked);

			AppConfigEx.RemoveIndexedItems(AppDefs.ConfigKeys.MruItem.Key, m_mruList.ItemCount);
			AppConfigEx.SetValue(AppDefs.ConfigKeys.MruMaxItemCount, m_mruList.MaxItemCount);
			for(uint uMru = 0; uMru < m_mruList.ItemCount; ++uMru)
			{
				KeyValuePair<string, string> kvpMru = m_mruList.GetItem(uMru);
				AppConfigEx.SetValue(AppDefs.ConfigKeys.MruItem.Key + uMru.ToString(),
					kvpMru.Key + AppDefs.MruNameValueSplitter + kvpMru.Value);
			}

			AppConfigEx.SetValue(AppDefs.ConfigKeys.ShowGridLines, m_lvEntries.GridLines);

			AppConfigEx.SetValue(AppDefs.ConfigKeys.UseNativeForKeyEnc, NativeLib.AllowNative);

			// Saved by options dialog:
			// AppConfigEx.SetValue(AppDefs.ConfigKeys.LockAfterTime, (uint)m_nLockTimerMax);
			// AppConfigEx.SetValue(AppDefs.ConfigKeys.ClipboardAutoClearTime, m_nClipClearMax);

			AppConfigEx.SetValue(AppDefs.ConfigKeys.PolicyPlugins, AppPolicy.NewIsAllowed(AppPolicyFlag.Plugins));
			AppConfigEx.SetValue(AppDefs.ConfigKeys.PolicyExport, AppPolicy.NewIsAllowed(AppPolicyFlag.Export));
			AppConfigEx.SetValue(AppDefs.ConfigKeys.PolicyImport, AppPolicy.NewIsAllowed(AppPolicyFlag.Import));
			AppConfigEx.SetValue(AppDefs.ConfigKeys.PolicyPrint, AppPolicy.NewIsAllowed(AppPolicyFlag.Print));
			AppConfigEx.SetValue(AppDefs.ConfigKeys.PolicySaveDatabase, AppPolicy.NewIsAllowed(AppPolicyFlag.SaveDatabase));
			AppConfigEx.SetValue(AppDefs.ConfigKeys.PolicyAutoType, AppPolicy.NewIsAllowed(AppPolicyFlag.AutoType));
			AppConfigEx.SetValue(AppDefs.ConfigKeys.PolicyCopyToClipboard, AppPolicy.NewIsAllowed(AppPolicyFlag.CopyToClipboard));
			AppConfigEx.SetValue(AppDefs.ConfigKeys.PolicyDragDrop, AppPolicy.NewIsAllowed(AppPolicyFlag.DragDrop));

			AppConfigEx.Save();
		}

		public void UpdateUIState(bool bSetModified, bool bUpdateLists)
		{
			if(bUpdateLists)
			{
				UpdateGroupList(true, null);
				UpdateEntryList(null, true);
			}

			UpdateUIState(bSetModified);
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
		public void UpdateUIState(bool bSetModified)
		{
			NotifyUserActivity();

			bool bDatabaseOpened = m_pwDatabase.IsOpen;

			if(bDatabaseOpened && bSetModified)
				m_pwDatabase.Modified = true;

			bool bGroupsEnabled = m_tvGroups.Enabled;
			if(bGroupsEnabled && (!bDatabaseOpened))
			{
				m_tvGroups.BackColor = AppDefs.ColorControlDisabled;
				m_tvGroups.Enabled = false;
			}
			else if((!bGroupsEnabled) && bDatabaseOpened)
			{
				m_tvGroups.Enabled = true;
				m_tvGroups.BackColor = AppDefs.ColorControlNormal;
			}

			m_lvEntries.Enabled = bDatabaseOpened;

			int nEntriesCount = m_lvEntries.Items.Count;
			int nEntriesSelected = m_lvEntries.SelectedIndices.Count;

			m_statusPartSelected.Text = nEntriesSelected.ToString() +
				" " + KPRes.OfLower + " " + nEntriesCount.ToString() +
				" " + KPRes.SelectedLower;
			m_statusPartInfo.Text = KPRes.Ready;

			string strWindowText = string.Empty;
			if(IsFileLocked())
				strWindowText = PwDefs.ProductName + " - " +
					KPRes.WorkspaceLocked;
			else if(!bDatabaseOpened)
				strWindowText = PwDefs.ProductName;
			else
			{
				string strFileDesc;
				if(AppConfigEx.GetBool(AppDefs.ConfigKeys.ShowFullPathInTitleBar))
					strFileDesc = m_pwDatabase.IOConnectionInfo.Url;
				else strFileDesc = UrlUtil.GetFileName(m_pwDatabase.IOConnectionInfo.Url);

				int nMaxPathLen = 63 - PwDefs.ProductName.Length - 2 - 2;
				Debug.Assert(nMaxPathLen > 1);
				strFileDesc = UrlUtil.CompactPath(strFileDesc, (uint)nMaxPathLen);

				if(m_pwDatabase.Modified)
					strWindowText = PwDefs.ProductName + " [" + strFileDesc + "*]";
				else
					strWindowText = PwDefs.ProductName + " [" + strFileDesc + "]";
			}

			this.Text = strWindowText;

			if(IsFileLocked())
			{
				string strNtfText = strWindowText + "\r\n" +
					UrlUtil.GetFileName(m_strLockedIoc);
				if(strNtfText.Length > 64) strNtfText = strNtfText.Substring(0, 64);
				m_ntfTray.Text = strNtfText;
				m_ntfTray.Icon = Properties.Resources.QuadLocked;
			}
			else
			{
				m_ntfTray.Text = strWindowText;
				m_ntfTray.Icon = Properties.Resources.QuadNormal;
			}

			// Main menu
			m_menuFileClose.Enabled = m_menuFileSave.Enabled =
				m_menuFileSaveAsLocal.Enabled = m_menuFileSaveAsUrl.Enabled =
				m_menuFileDbSettings.Enabled = m_menuFileChangeMasterKey.Enabled =
				m_menuFilePrint.Enabled = bDatabaseOpened;

			m_menuFileLock.Enabled = m_tbLockWorkspace.Enabled =
				(bDatabaseOpened || IsFileLocked());

			m_menuEditFind.Enabled = m_menuToolsGeneratePwList.Enabled =
				m_menuToolsTanWizard.Enabled =
				m_menuEditShowAllEntries.Enabled = m_menuEditShowExpired.Enabled =
				m_menuToolsDbMaintenance.Enabled = bDatabaseOpened;

			m_menuFileImport.Enabled = bDatabaseOpened;

			m_menuFileExportXML.Enabled = m_menuFileExportHtml.Enabled =
				m_menuFileExportKdb3.Enabled = m_menuFileExportUseXsl.Enabled =
				bDatabaseOpened;

			m_menuFileSynchronize.Enabled = bDatabaseOpened &&
				m_pwDatabase.IOConnectionInfo.IsLocalFile() &&
				(m_pwDatabase.IOConnectionInfo.Url.Length > 0);

			m_ctxGroupAdd.Enabled = m_ctxGroupEdit.Enabled = m_ctxGroupDelete.Enabled =
				bDatabaseOpened;

			m_tbSaveDatabase.Enabled = m_tbFind.Enabled = m_tbViewsShowAll.Enabled =
				m_tbViewsShowExpired.Enabled = bDatabaseOpened;

			m_tbQuickFind.Enabled = bDatabaseOpened;

			m_ctxEntryAdd.Enabled = m_tbAddEntry.Enabled = bDatabaseOpened;
			m_ctxEntryEdit.Enabled = (nEntriesSelected == 1);
			m_ctxEntryDelete.Enabled = (nEntriesSelected > 0);

			m_ctxEntryCopyUserName.Enabled = m_ctxEntryCopyPassword.Enabled =
				m_tbCopyUserName.Enabled = m_tbCopyPassword.Enabled =
				m_ctxEntryCopyUrl.Enabled = m_ctxEntryPerformAutoType.Enabled =
				(nEntriesSelected == 1);

			m_ctxEntryOpenUrl.Enabled = m_ctxEntrySaveAttachedFiles.Enabled =
				m_ctxEntryDuplicate.Enabled = m_ctxEntryMassSetIcon.Enabled =
				(nEntriesSelected > 0);

			m_ctxEntryMoveToTop.Enabled = m_ctxEntryMoveToBottom.Enabled =
				(m_pListSorter.Column < 0) && (nEntriesSelected > 0);
			m_ctxEntryMoveOneDown.Enabled = m_ctxEntryMoveOneUp.Enabled =
				(m_pListSorter.Column < 0) && (nEntriesSelected == 1);

			m_ctxEntrySelectAll.Enabled = bDatabaseOpened && (nEntriesCount > 0);

			m_tbSaveDatabase.Image = m_pwDatabase.Modified ? m_imgFileSaveEnabled :
				m_imgFileSaveDisabled;

			m_ctxEntryClipCopy.Enabled = (nEntriesSelected > 0);
			m_ctxEntryClipPaste.Enabled = Clipboard.ContainsData(EntryUtil.ClipFormatEntries);

			m_ctxEntryColorStandard.Enabled = m_ctxEntryColorLightRed.Enabled =
				m_ctxEntryColorLightGreen.Enabled = m_ctxEntryColorLightBlue.Enabled =
				m_ctxEntryColorLightYellow.Enabled = m_ctxEntryColorCustom.Enabled =
				(nEntriesSelected > 0);

			PwEntry pe = GetSelectedEntry(true);
			ShowEntryDetails(pe);

			if(pe != null) m_ctxEntryPerformAutoType.Enabled = pe.AutoType.Enabled;

			string strLockUnlock = IsFileLocked() ? KPRes.LockMenuUnlock :
				KPRes.LockMenuLock;
			m_menuFileLock.Text = strLockUnlock;
			m_tbLockWorkspace.Text = strLockUnlock.Replace(@"&", string.Empty);

			UpdateLinkedMenuItems();
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
			if(m_nClipClearCur > 0)
				m_statusClipboard.Value = (m_nClipClearCur * 100) / m_nClipClearMax;
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

				string strText = KPRes.ClipboardClearInSeconds;
				strText = strText.Replace("[PARAM]", m_nClipClearMax.ToString());

				if(m_ntfTray.Visible)
					m_ntfTray.ShowBalloonTip(0, KPRes.ClipboardAutoClear,
						strText, ToolTipIcon.Info);
			}
		}

		/// <summary>
		/// Gets the focused or first selected entry.
		/// </summary>
		/// <returns></returns>
		public PwEntry GetSelectedEntry(bool bRequireSelected)
		{
			if(!m_pwDatabase.IsOpen) return null;

			if(!bRequireSelected)
			{
				ListViewItem lviFocused = m_lvEntries.FocusedItem;
				if(lviFocused != null) return (PwEntry)lviFocused.Tag;
			}

			ListView.SelectedListViewItemCollection coll = m_lvEntries.SelectedItems;
			if(coll.Count > 0)
			{
				ListViewItem lvi = coll[0];
				if(lvi != null) return (PwEntry)lvi.Tag;
			}

			return null;
		}

		/// <summary>
		/// Get all selected entries.
		/// </summary>
		/// <returns>A list of all selected entries.</returns>
		public PwEntry[] GetSelectedEntries()
		{
			if(!m_pwDatabase.IsOpen) return null;

			ListView.SelectedListViewItemCollection coll = m_lvEntries.SelectedItems;

			if((coll == null) || (coll.Count == 0)) return null;

			PwEntry[] vSelected = new PwEntry[coll.Count];
			for(int i = 0; i < coll.Count; i++)
				vSelected[i] = (PwEntry)coll[i].Tag;

			return vSelected;
		}

		/// <summary>
		/// Get the currently selected group. The selected <c>TreeNode</c> is
		/// automatically translated to a <c>PwGroup</c>.
		/// </summary>
		/// <returns>Selected <c>PwGroup</c>.</returns>
		public PwGroup GetSelectedGroup()
		{
			if(!m_pwDatabase.IsOpen) return null;

			TreeNode tn = m_tvGroups.SelectedNode;
			if(tn == null) return null;
			return (PwGroup)tn.Tag;
		}

		private ListViewItem AddEntryToList(PwEntry pe)
		{
			if(pe == null) return null;

			ListViewItem lvi = new ListViewItem();
			lvi.Tag = pe;

			PwIcon pwIcon = pe.Icon;
			if(pe.Expires && (pe.ExpiryTime < m_dtCachedNow))
			{
				pwIcon = PwIcon.Expired;
				lvi.Font = m_fontExpired;
			}

			if(m_bEntryGrouping)
			{
				PwGroup pgContainer = pe.ParentGroup;
				PwGroup pgLast = (m_lvgLastEntryGroup != null) ? (PwGroup)m_lvgLastEntryGroup.Tag : null;

				Debug.Assert(pgContainer != null);
				if(pgContainer != null)
				{
					if(pgContainer != pgLast)
					{
						m_lvgLastEntryGroup = new ListViewGroup(pgContainer.Name);
						m_lvgLastEntryGroup.Tag = pgContainer;

						m_lvEntries.Groups.Add(m_lvgLastEntryGroup);
					}

					lvi.Group = m_lvgLastEntryGroup;
				}
			}

			lvi.ImageIndex = (int)pwIcon;

			if(!pe.ForegroundColor.IsEmpty)
				lvi.ForeColor = pe.ForegroundColor;
			if(!pe.BackgroundColor.IsEmpty)
				lvi.BackColor = pe.BackgroundColor;

			m_bOnlyTans &= PwDefs.IsTanEntry(pe);
			if(m_bShowTanIndices && m_bOnlyTans)
			{
				string strIndex = pe.Strings.ReadSafe(PwDefs.TanIndexField);

				if(strIndex.Length > 0) lvi.Text = strIndex;
				else lvi.Text = PwDefs.TanTitle;

				m_lvEntries.Items.Add(lvi);
			}
			else
			{
				if(m_lvEntries.Columns[(int)AppDefs.ColumnID.Title].Width > 0)
				{
					if(m_viewHideFields.ProtectTitle) lvi.Text = PwDefs.HiddenPassword;
					else lvi.Text = pe.Strings.ReadSafe(PwDefs.TitleField);
				}
				m_lvEntries.Items.Add(lvi);
			}

			if(m_lvEntries.Columns[(int)AppDefs.ColumnID.UserName].Width > 0)
			{
				if(m_viewHideFields.ProtectUserName) lvi.SubItems.Add(PwDefs.HiddenPassword);
				else lvi.SubItems.Add(pe.Strings.ReadSafe(PwDefs.UserNameField));
			}
			else lvi.SubItems.Add(string.Empty);

			if(m_lvEntries.Columns[(int)AppDefs.ColumnID.Password].Width > 0)
			{
				if(m_viewHideFields.ProtectPassword) lvi.SubItems.Add(PwDefs.HiddenPassword);
				else lvi.SubItems.Add(pe.Strings.ReadSafe(PwDefs.PasswordField));
			}
			else lvi.SubItems.Add(string.Empty);

			if(m_lvEntries.Columns[(int)AppDefs.ColumnID.Url].Width > 0)
			{
				if(m_viewHideFields.ProtectUrl) lvi.SubItems.Add(PwDefs.HiddenPassword);
				else lvi.SubItems.Add(pe.Strings.ReadSafe(PwDefs.UrlField));
			}
			else lvi.SubItems.Add(string.Empty);

			if(m_lvEntries.Columns[(int)AppDefs.ColumnID.Notes].Width > 0)
			{
				if(m_viewHideFields.ProtectNotes) lvi.SubItems.Add(PwDefs.HiddenPassword);
				else lvi.SubItems.Add(pe.Strings.ReadSafe(PwDefs.NotesField));
			}
			else lvi.SubItems.Add(string.Empty);

			if(m_lvEntries.Columns[(int)AppDefs.ColumnID.CreationTime].Width > 0)
				lvi.SubItems.Add(pe.CreationTime.ToString());
			else lvi.SubItems.Add(string.Empty);

			if(m_lvEntries.Columns[(int)AppDefs.ColumnID.LastAccessTime].Width > 0)
				lvi.SubItems.Add(pe.LastAccessTime.ToString());
			else lvi.SubItems.Add(string.Empty);

			if(m_lvEntries.Columns[(int)AppDefs.ColumnID.LastModificationTime].Width > 0)
				lvi.SubItems.Add(pe.LastModificationTime.ToString());
			else lvi.SubItems.Add(string.Empty);

			if(m_lvEntries.Columns[(int)AppDefs.ColumnID.ExpiryTime].Width > 0)
			{
				if(pe.Expires) lvi.SubItems.Add(pe.ExpiryTime.ToString());
				else lvi.SubItems.Add(m_strNeverExpiresText);
			}
			else lvi.SubItems.Add(string.Empty);

			if(m_lvEntries.Columns[(int)AppDefs.ColumnID.Uuid].Width > 0)
				lvi.SubItems.Add(pe.Uuid.ToHexString());
			else lvi.SubItems.Add(string.Empty);

			if(m_lvEntries.Columns[(int)AppDefs.ColumnID.Attachment].Width > 0)
				lvi.SubItems.Add(pe.Binaries.UCount.ToString());
			else lvi.SubItems.Add(string.Empty);

			return lvi;
		}

		/// <summary>
		/// Update the group list. This function completely rebuilds the groups
		/// view. You must call this function after you made any changes to the
		/// groups structure of the currently opened database.
		/// </summary>
		/// <param name="bRestoreView">If this parameter is <c>true</c>, the
		/// previous 'view' of the group is restored (i.e. it tries to display
		/// the previously visible groups again).</param>
		/// <param name="pgNewSelected">If this parameter is <c>null</c>, the
		/// previously selected group is selected again (after the list was
		/// rebuilt). If this parameter is non-<c>null</c>, the specified
		/// <c>PwGroup</c> is selected after the function returns.</param>
		public void UpdateGroupList(bool bRestoreView, PwGroup pgNewSelected)
		{
			NotifyUserActivity();

			PwGroup pg = (pgNewSelected == null) ? GetSelectedGroup() : pgNewSelected;

			m_tvGroups.BeginUpdate();
			m_tvGroups.Nodes.Clear();

			TreeNode tnRoot = null;
			if(m_pwDatabase.RootGroup != null)
			{
				tnRoot = new TreeNode(m_pwDatabase.RootGroup.Name, (int)m_pwDatabase.RootGroup.Icon, (int)m_pwDatabase.RootGroup.Icon);
				tnRoot.Tag = m_pwDatabase.RootGroup;
				tnRoot.NodeFont = new Font(m_tvGroups.Font, FontStyle.Bold);
				m_tvGroups.Nodes.Add(tnRoot);
			}

			m_dtCachedNow = DateTime.Now;

			TreeNode tnSelected = null;
			RecursiveAddGroup(tnRoot, m_pwDatabase.RootGroup, pg, ref tnSelected);

			if(tnRoot != null) tnRoot.Expand();

			m_tvGroups.EndUpdate();

			if(tnSelected != null) m_tvGroups.SelectedNode = tnSelected;
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
		/// <c>UpdateEntriesList</c>.
		/// </summary>
		/// <param name="pgSelected">Group whose entries should be shown. If this
		/// parameter is <c>null</c>, the entries of the currently selected group
		/// (groups view) are displayed, otherwise the entries of the <c>pgSelected</c>
		/// group are displayed.</param>
		/// <param name="bRestoreView">If this parameter is <c>true</c>, the function
		/// tries to restore the previous view (i.e. it tries to show the previously
		/// visible entries again).</param>
		public void UpdateEntryList(PwGroup pgSelected, bool bRestoreView)
		{
			NotifyUserActivity();

			PwEntry[] vSelected = null;

			PwEntry peTop = null;
			ListViewItem lviTop;
			try
			{
				if(bRestoreView)
				{
					lviTop = m_lvEntries.TopItem;
					if(lviTop != null) peTop = (PwEntry)lviTop.Tag;
				}
			}
			catch(Exception) { }

			if(bRestoreView) vSelected = GetSelectedEntries();

			PwGroup pg = (pgSelected != null) ? pgSelected : GetSelectedGroup();

			m_lvEntries.BeginUpdate();
			m_lvEntries.Items.Clear();
			m_bOnlyTans = true;

			m_bEntryGrouping = (pg != null) ? pg.IsVirtual : false;
			m_lvgLastEntryGroup = null;
			m_lvEntries.ShowGroups = m_bEntryGrouping;

			lviTop = null;

			m_dtCachedNow = DateTime.Now;
			if(pg != null)
			{
				ListViewItem lvi;
				foreach(PwEntry pe in pg.Entries)
				{
					lvi = AddEntryToList(pe);

					if(bRestoreView)
					{
						if(vSelected != null)
						{
							if(Array.IndexOf(vSelected, pe) >= 0)
								lvi.Selected = true;
						}

						if(peTop == pe) lviTop = lvi;
					}
				}
			}

			if(bRestoreView && (lviTop != null))
				m_lvEntries.TopItem = lviTop;

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
			else // m_bSimpleTANView == false
			{
				if(view != View.Details)
					m_lvEntries.View = View.Details;
			}

			m_lvEntries.EndUpdate();
		}

		/// <summary>
		/// Refresh the entries list. All currently displayed entries are updated.
		/// If you made changes to the list that change the number of visible entries
		/// (like adding or removing an entry), you must use the <c>UpdatePasswordList</c>
		/// function instead.
		/// </summary>
		public void RefreshEntriesList()
		{
			int nItemCount = m_lvEntries.Items.Count;
			if(nItemCount <= 0) return;

			PwEntry[] vSelected = GetSelectedEntries();
			if(vSelected == null)
				vSelected = new PwEntry[1]{ new PwEntry(null, false, false) };

			PwEntry[] vList = new PwEntry[nItemCount];
			for(int iEnum = 0; iEnum < nItemCount; iEnum++)
				vList[iEnum] = (PwEntry)m_lvEntries.Items[iEnum].Tag;

			m_lvEntries.BeginUpdate();
			m_lvEntries.Items.Clear();

			m_dtCachedNow = DateTime.Now;
			for(int iAdd = 0; iAdd < nItemCount; iAdd++)
			{
				AddEntryToList(vList[iAdd]);

				if(Array.IndexOf(vSelected, vList[iAdd]) >= 0)
					m_lvEntries.Items[iAdd].Selected = true;
			}

			m_lvEntries.EndUpdate();
		}

		private void RecursiveAddGroup(TreeNode tnParent, PwGroup pgContainer, PwGroup pgFind, ref TreeNode tnFound)
		{
			if(pgContainer == null) return;

			TreeNodeCollection tnc;
			if(tnParent == null) tnc = m_tvGroups.Nodes;
			else tnc = tnParent.Nodes;

			foreach(PwGroup pg in pgContainer.Groups)
			{
				if(pg.Expires && (pg.ExpiryTime < m_dtCachedNow))
					pg.Icon = PwIcon.Expired;

				string strName = pg.Name;
				TreeNode tn = new TreeNode(strName, (int)pg.Icon, (int)pg.Icon);
				tn.Tag = pg;
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

		private void SortPasswordList(bool bEnableSorting, int nColumn, bool bUpdateEntryList)
		{
			if(bEnableSorting)
			{
				int nOldColumn = m_pListSorter.Column;
				SortOrder sortOrder = m_pListSorter.Order;

				if(nColumn == nOldColumn)
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

				ColumnHeader ch;
				if((nOldColumn >= 0) && (nOldColumn != nColumn))
				{
					ch = m_lvEntries.Columns[nOldColumn];
					ch.ImageIndex = -1;
				}

				if(nColumn >= 0)
				{
					ch = m_lvEntries.Columns[nColumn];

					if(sortOrder == SortOrder.None)
						ch.ImageIndex = -1;
					else if(sortOrder == SortOrder.Ascending)
						ch.ImageIndex = (int)PwIcon.SortUpArrow;
					else if(sortOrder == SortOrder.Descending)
						ch.ImageIndex = (int)PwIcon.SortDownArrow;
				}

				if(sortOrder != SortOrder.None)
				{
					m_pListSorter = new ListSorter(nColumn, sortOrder);
					m_lvEntries.ListViewItemSorter = m_pListSorter;
				}
				else
				{
					m_pListSorter = new ListSorter(-1, SortOrder.Ascending);
					m_lvEntries.ListViewItemSorter = null;

					if(bUpdateEntryList) UpdateEntryList(null, true);
				}
			}
			else // Disable sorting
			{
				m_pListSorter = new ListSorter(-1, SortOrder.Ascending);
				m_lvEntries.ListViewItemSorter = null;

				foreach(ColumnHeader ch in m_lvEntries.Columns)
					ch.ImageIndex = -1;

				if(bUpdateEntryList) UpdateEntryList(null, true);
			}
		}

		private void ShowEntryView(bool bShow)
		{
			m_menuViewShowEntryView.Checked = bShow;

			AppConfigEx.SetValue(AppDefs.ConfigKeys.ShowEntryView, bShow);

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

			Font f = WinUtil.FontIDToFont(AppConfigEx.GetValue(AppDefs.ConfigKeys.ListFont));
			string strFontFace = ((f != null) ? f.Name : "Microsoft Sans Serif");
			float fFontSize = ((f != null) ? f.SizeInPoints : 8);

			string strItemSeparator = (m_splitHorizontal.Orientation == Orientation.Horizontal) ?
				", " : "\\par ";

			StringBuilder sb = new StringBuilder();
			StrUtil.InitRtf(sb, strFontFace, fFontSize);

			sb.Append("\\b ");
			sb.Append(KPRes.Group);
			sb.Append(":\\b0  ");

			PwGroup pg = pe.ParentGroup;
			if(pg != null) sb.Append(StrUtil.MakeRtfString(pg.Name));

			sb.Append(strItemSeparator);
			sb.Append("\\b ");
			sb.Append(KPRes.Title);
			sb.Append(":\\b0  ");

			sb.Append(StrUtil.MakeRtfString(pe.Strings.ReadSafeEx(PwDefs.TitleField)));

			sb.Append(strItemSeparator);
			sb.Append("\\b ");
			sb.Append(KPRes.UserName);
			sb.Append(":\\b0  ");
			sb.Append(StrUtil.MakeRtfString(pe.Strings.ReadSafeEx(PwDefs.UserNameField)));

			sb.Append(strItemSeparator);
			sb.Append("\\b ");
			sb.Append(KPRes.Password);
			sb.Append(":\\b0  ");
			sb.Append(StrUtil.MakeRtfString(pe.Strings.ReadSafeEx(PwDefs.PasswordField)));

			sb.Append(strItemSeparator);
			sb.Append("\\b ");
			sb.Append(KPRes.URL);
			sb.Append(":\\b0  ");
			sb.Append(StrUtil.MakeRtfString(pe.Strings.ReadSafeEx(PwDefs.UrlField)));

			foreach(KeyValuePair<string, ProtectedString> kvp in pe.Strings)
			{
				if(PwDefs.IsStandardField(kvp.Key)) continue;

				sb.Append(strItemSeparator);
				sb.Append("\\b ");
				sb.Append(kvp.Key);
				sb.Append(":\\b0  ");
				sb.Append(StrUtil.MakeRtfString(kvp.Value.ReadString()));
			}

			sb.Append(strItemSeparator);
			sb.Append("\\b ");
			sb.Append(KPRes.CreationTime);
			sb.Append(":\\b0  ");
			sb.Append(StrUtil.MakeRtfString(TimeUtil.ToDisplayString(pe.CreationTime)));

			sb.Append(strItemSeparator);
			sb.Append("\\b ");
			sb.Append(KPRes.LastAccessTime);
			sb.Append(":\\b0  ");
			sb.Append(StrUtil.MakeRtfString(TimeUtil.ToDisplayString(pe.LastAccessTime)));

			sb.Append(strItemSeparator);
			sb.Append("\\b ");
			sb.Append(KPRes.LastModificationTime);
			sb.Append(":\\b0  ");
			sb.Append(StrUtil.MakeRtfString(TimeUtil.ToDisplayString(pe.LastModificationTime)));

			if(pe.Expires)
			{
				sb.Append(strItemSeparator);
				sb.Append("\\b ");
				sb.Append(KPRes.ExpiryTime);
				sb.Append(":\\b0  ");
				sb.Append(StrUtil.MakeRtfString(TimeUtil.ToDisplayString(pe.ExpiryTime)));
			}

			if(pe.Binaries.UCount > 0)
			{
				sb.Append(strItemSeparator);
				sb.Append("\\b ");
				sb.Append(KPRes.Attachments);
				sb.Append(":\\b0  ");
				sb.Append(pe.Binaries.UCount.ToString());
			}

			string strNotes = pe.Strings.ReadSafeEx(PwDefs.NotesField);
			if(strNotes.Length != 0)
			{
				sb.Append("\\par \\par ");

				strNotes = StrUtil.MakeRtfString(strNotes);

				strNotes = strNotes.Replace("<b>", "\\b ");
				strNotes = strNotes.Replace("</b>", "\\b0 ");
				strNotes = strNotes.Replace("<i>", "\\i ");
				strNotes = strNotes.Replace("</i>", "\\i0 ");
				strNotes = strNotes.Replace("<u>", "\\ul ");
				strNotes = strNotes.Replace("</u>", "\\ul0 ");

				sb.Append(strNotes);
			}

			sb.Append("\\pard }");
			m_richEntryView.Rtf = sb.ToString();
		}

		private void PerformDefaultAction(object sender, EventArgs e, PwEntry pe, AppDefs.ColumnID colID)
		{
			Debug.Assert(pe != null); if(pe == null) return;

			bool bMinimize = AppConfigEx.GetBool(AppDefs.ConfigKeys.MinimizeAfterCopy);
			Form frmMin = (bMinimize ? this : null);

			switch(colID)
			{
				case AppDefs.ColumnID.Title:
					if(PwDefs.IsTanEntry(pe))
						OnEntryCopyPassword(sender, e);
					else
						OnEntryEdit(sender, e);
					break;
				case AppDefs.ColumnID.UserName:
					OnEntryCopyUserName(sender, e);
					break;
				case AppDefs.ColumnID.Password:
					OnEntryCopyPassword(sender, e);
					break;
				case AppDefs.ColumnID.Url:
					OnEntryOpenURL(sender, e);
					break;
				case AppDefs.ColumnID.Notes:
					ClipboardUtil.CopyAndMinimize(pe.Strings.ReadSafe(PwDefs.NotesField),
						true, frmMin);
					StartClipboardCountdown();
					break;
				case AppDefs.ColumnID.CreationTime:
					ClipboardUtil.CopyAndMinimize(TimeUtil.ToDisplayString(pe.CreationTime),
						true, frmMin);
					StartClipboardCountdown();
					break;
				case AppDefs.ColumnID.LastAccessTime:
					ClipboardUtil.CopyAndMinimize(TimeUtil.ToDisplayString(pe.LastAccessTime),
						true, frmMin);
					StartClipboardCountdown();
					break;
				case AppDefs.ColumnID.LastModificationTime:
					ClipboardUtil.CopyAndMinimize(TimeUtil.ToDisplayString(pe.LastModificationTime),
						true, frmMin);
					StartClipboardCountdown();
					break;
				case AppDefs.ColumnID.ExpiryTime:
					if(pe.Expires)
						ClipboardUtil.CopyAndMinimize(TimeUtil.ToDisplayString(pe.ExpiryTime),
							true, frmMin);
					else
						ClipboardUtil.CopyAndMinimize(KPRes.NeverExpires,
							true, frmMin);
					StartClipboardCountdown();
					break;
				case AppDefs.ColumnID.Attachment:
					break;
				case AppDefs.ColumnID.Uuid:
					ClipboardUtil.CopyAndMinimize(pe.Uuid.ToHexString(),
						true, frmMin);
					StartClipboardCountdown();
					break;
				default:
					Debug.Assert(false);
					break;
			}
		}

		/// <summary>
		/// Do a quick find. All entries of the currently opened database are searched
		/// for a string and the results are automatically displayed in the main window.
		/// </summary>
		/// <param name="strSearch">String to search the entries for.</param>
		/// <param name="strGroupName">Group name of the group that receives the search
		/// results.</param>
		public void PerformQuickFind(string strSearch, string strGroupName)
		{
			Debug.Assert(strSearch != null); if(strSearch == null) return;
			Debug.Assert(strGroupName != null); if(strGroupName == null) return;

			PwGroup pg = new PwGroup(true, true, strGroupName, PwIcon.EMailSearch);
			pg.IsVirtual = true;

			PwEntry peDesc = new PwEntry(pg, true, true);
			peDesc.Icon = PwIcon.EMailSearch;
			pg.Entries.Add(peDesc);

			SearchParameters sp = new SearchParameters();
			sp.SearchText = strSearch;
			sp.SearchInAllStrings = true;
			m_pwDatabase.RootGroup.SearchEntries(sp, pg.Entries);

			string strItemsFound = (pg.Entries.UCount - 1).ToString() + " " + KPRes.SearchItemsFoundSmall;
			peDesc.Strings.Set(PwDefs.TitleField, new ProtectedString(false, strItemsFound));

			UpdateEntryList(pg, false);
			UpdateUIState(false);
		}

		private void ShowExpiredEntries(bool bOnlyIfExists, uint uSkipDays)
		{
			PwGroup pg = new PwGroup(true, true, KPRes.ExpiredEntries, PwIcon.Expired);
			pg.IsVirtual = true;

			PwEntry peDesc = new PwEntry(pg, true, true);
			peDesc.Icon = PwIcon.EMailSearch;
			pg.Entries.Add(peDesc);

			DateTime dtLimit = DateTime.Now;
			if(uSkipDays != 0)
				dtLimit = dtLimit.Add(new TimeSpan((int)uSkipDays, 0, 0, 0));

			EntryHandler eh = delegate(PwEntry pe)
			{
				if(pe.Expires && (pe.ExpiryTime <= dtLimit))
					pg.Entries.Add(pe);
				return true;
			};

			m_pwDatabase.RootGroup.TraverseTree(TraversalMethod.PreOrder, null, eh);

			string strItemsFound = (pg.Entries.UCount - 1).ToString() + " " + KPRes.SearchItemsFoundSmall;
			peDesc.Strings.Set(PwDefs.TitleField, new ProtectedString(false, strItemsFound));

			if(uSkipDays != 0)
				pg.Name = KPRes.SoonToExpireEntries;

			if((pg.Entries.UCount > 1) || (bOnlyIfExists == false))
			{
				UpdateEntryList(pg, false);
				UpdateUIState(false);
			}
			else
			{
				UpdateEntryList(null, true);
				UpdateUIState(false);
			}
		}

		private void PerformExport(KeeExportFormat fmt)
		{
			string strFile = null;
			PerformExport(fmt, ref strFile);
		}

		/// <summary>
		/// Export the currently opened database to a file.
		/// </summary>
		/// <param name="fmt">Export format.</param>
		/// <param name="strToFile">File to export the data to. If this parameter is
		/// <c>null</c>, a dialog is displayed which prompts the user to specify a
		/// location. After the function returns, this parameter contains the path to
		/// which the user has really exported the data to (or it is <c>null</c>, if
		/// the export has been cancelled).</param>
		public void PerformExport(KeeExportFormat fmt, ref string strToFile)
		{
			Debug.Assert(m_pwDatabase.IsOpen); if(!m_pwDatabase.IsOpen) return;
			if(!AppPolicy.Try(AppPolicyFlag.Export)) return;

			System.Xml.Xsl.XslCompiledTransform xsl = null;
			if(fmt == KeeExportFormat.UseXsl)
			{
				if(m_openXslFile.ShowDialog() != DialogResult.OK) return;
				string strXSLFile = m_openXslFile.FileName;

				xsl = new System.Xml.Xsl.XslCompiledTransform();

				try { xsl.Load(strXSLFile); }
				catch(Exception e)
				{
					string strMsg = strXSLFile + "\r\n\r\n" + KPRes.NoXSLFile;
					strMsg += "\r\n\r\n" + e.Message;
					MessageBox.Show(strMsg, PwDefs.ShortProductName, MessageBoxButtons.OK,
						MessageBoxIcon.Warning);
					return;
				}
			}

			string strSuggestion;
			if(m_pwDatabase.IOConnectionInfo.Url.Length > 0)
				strSuggestion = UrlUtil.StripExtension(UrlUtil.GetFileName(
					m_pwDatabase.IOConnectionInfo.Url));
			else strSuggestion = KPRes.Database;

			if(fmt == KeeExportFormat.PlainXml) strSuggestion += ".xml";
			else if(fmt == KeeExportFormat.Html) strSuggestion += ".html";
			else if(fmt == KeeExportFormat.Kdb3) strSuggestion += ".kdb";
			else if(fmt == KeeExportFormat.UseXsl) strSuggestion += ".*";
			else { Debug.Assert(false); }

			string strExt = UrlUtil.GetExtension(strSuggestion);
			string strPrevFilter = m_saveExportTo.Filter;
			m_saveExportTo.Filter = strExt.ToUpper() + " (*." + strExt + ")|*." +
				strExt + "|" + strPrevFilter;

			m_saveExportTo.FileName = strSuggestion;
			if((strToFile != null) || (m_saveExportTo.ShowDialog() == DialogResult.OK))
			{
				this.Update();
				Application.DoEvents();

				string strTargetFile = (strToFile != null) ? strToFile : m_saveExportTo.FileName;
				strToFile = strTargetFile;

				ShowWarningsLogger swLogger = CreateShowWarningsLogger();
				swLogger.StartLogging(KPRes.ExportingStatusMsg);

				if(fmt == KeeExportFormat.PlainXml)
				{
					Kdb4File kdb = new Kdb4File(m_pwDatabase);
					FileSaveResult fsr = kdb.Save(strTargetFile, Kdb4File.KdbFormat.PlainXml, swLogger);

					if(fsr.Code != FileSaveResultCode.Success)
					{
						MessageBox.Show(ResUtil.FileSaveResultToString(fsr),
							PwDefs.ShortProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					}
				}
				else if(fmt == KeeExportFormat.Html)
				{
					PrintForm dlg = new PrintForm();
					dlg.InitEx(m_pwDatabase.RootGroup, false);

					if(dlg.ShowDialog() == DialogResult.OK)
					{
						try
						{
							TextWriter tw = new StreamWriter(strTargetFile, false, Encoding.UTF8);
							tw.Write(dlg.GeneratedHTML);
							tw.Close();
						}
						catch(Exception twEx)
						{
							MessageBox.Show(ResUtil.FileSaveResultToString(new FileSaveResult(
								FileSaveResultCode.FileCreationFailed, twEx)),
								PwDefs.ShortProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
						}
					}
				}
				else if(fmt == KeeExportFormat.Kdb3)
				{
					Kdb3File kdb = new Kdb3File(m_pwDatabase, swLogger);
					FileSaveResult fsr = kdb.Save(strTargetFile);

					if(fsr.Code != FileSaveResultCode.Success)
					{
						MessageBox.Show(ResUtil.FileSaveResultToString(fsr),
							PwDefs.ShortProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					}
				}
				else if(fmt == KeeExportFormat.UseXsl)
				{
					string strTempFile = strTargetFile + ".";
					strTempFile += Guid.NewGuid().ToString() + ".xml";

					Kdb4File kdb = new Kdb4File(m_pwDatabase);
					FileSaveResult fsr = kdb.Save(strTempFile, Kdb4File.KdbFormat.PlainXml, swLogger);

					if(fsr.Code != FileSaveResultCode.Success)
					{
						MessageBox.Show(strTempFile + "\r\n\r\n" + ResUtil.FileSaveResultToString(fsr),
							PwDefs.ShortProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
					}
					else
					{
						try { xsl.Transform(strTempFile, strTargetFile); }
						catch(Exception e)
						{
							MessageBox.Show(e.Message, PwDefs.ShortProductName, MessageBoxButtons.OK,
								MessageBoxIcon.Warning);
						}
					}

					try { File.Delete(strTempFile); }
					catch(Exception) { }
				}
				else { Debug.Assert(false); }

				swLogger.EndLogging();
			}

			m_saveExportTo.Filter = strPrevFilter;
			UpdateUIState(false);
		}

		/// <summary>
		/// Open a database. This function opens the specified database and updates
		/// the user interface.
		/// </summary>
		/// <param name="strIOConnection">File name of the database to open.</param>
		public void OpenDatabase(string strIOConnection, CompositeKey cmpKey, bool bOpenLocal)
		{
			OnFileClose(null, null);
			if(m_pwDatabase.IsOpen) return;

			IOConnectionInfo ioc;
			if(strIOConnection == null)
			{
				if(bOpenLocal)
				{
					DialogResult dr = m_openDatabaseFile.ShowDialog();
					if(dr != DialogResult.OK) return;

					ioc = IOConnectionInfo.FromPath(m_openDatabaseFile.FileName);
				}
				else
				{
					IOConnectionForm iocf = new IOConnectionForm();
					iocf.InitEx(false, new IOConnectionInfo());
					if(iocf.ShowDialog() != DialogResult.OK) return;

					ioc = iocf.IOConnectionInfo;
				}
			}
			else
			{
				ioc = IOConnectionInfo.UnserializeFromString(strIOConnection);

				if((ioc.CredSaveMode != IOCredSaveMode.SaveCred) &&
					(!ioc.IsLocalFile()))
				{
					IOConnectionForm iocf = new IOConnectionForm();
					iocf.InitEx(false, ioc.CloneDeep());
					if(iocf.ShowDialog() != DialogResult.OK) return;

					ioc = iocf.IOConnectionInfo;
				}
			}

			if((ioc == null) || !ioc.CanProbablyAccess())
			{
				MessageBox.Show(KPRes.FileNotFoundError + "\r\n\r\n" +
					ioc.GetDisplayName(), PwDefs.ShortProductName,
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			if(cmpKey == null)
			{
				for(int iTry = 0; iTry < 3; ++iTry)
				{
					KeyPromptForm kpf = new KeyPromptForm();
					kpf.InitEx(ioc.GetDisplayName());
					DialogResult dr = kpf.ShowDialog();
					if((dr == DialogResult.Cancel) || (dr == DialogResult.Abort)) break;

					FileOpenResult fr = OpenDatabaseInternal(ioc, kpf.CompositeKey);

					if(fr.Code != FileOpenResultCode.InvalidFileStructure) break;
				}
			}
			else // cmpKey != null
			{
				OpenDatabaseInternal(ioc, cmpKey);
			}

			if(m_pwDatabase.IsOpen) // Opened successfully
			{
				string strName = m_pwDatabase.IOConnectionInfo.GetDisplayName();
				m_mruList.AddItem(strName, IOConnectionInfo.SerializeToString(
					m_pwDatabase.IOConnectionInfo));

				AutoEnableVisualHiding();

				if(AppConfigEx.GetBool(AppDefs.ConfigKeys.AutoOpenLastFile))
					AppConfigEx.SetValue(AppDefs.ConfigKeys.LastDatabase,
						IOConnectionInfo.SerializeToString(m_pwDatabase.IOConnectionInfo));
				else
					AppConfigEx.SetValue(AppDefs.ConfigKeys.LastDatabase, string.Empty);

				if(FileOpened != null) FileOpened(this, EventArgs.Empty);
			}

			UpdateGroupList(false, null);
			if(m_pwDatabase.IsOpen && AppConfigEx.GetBool(AppDefs.ConfigKeys.ShowSoonToExpireOnDbOpen))
				ShowExpiredEntries(true, 7);
			else if(m_pwDatabase.IsOpen && AppConfigEx.GetBool(AppDefs.ConfigKeys.ShowExpiredOnDbOpen))
				ShowExpiredEntries(true, 0);
			else
			{
				UpdateEntryList(null, false);
				UpdateUIState(false);
			}
		}

		private FileOpenResult OpenDatabaseInternal(IOConnectionInfo ioc,
			CompositeKey cmpKey)
		{
			ShowWarningsLogger swLogger = CreateShowWarningsLogger();
			swLogger.StartLogging(KPRes.OpeningDatabase);

			FileOpenResult fr = m_pwDatabase.OpenDatabase(ioc, cmpKey, swLogger);

			if(fr.Code != FileOpenResultCode.Success)
			{
				MessageBox.Show(ResUtil.FileOpenResultToString(fr) + "\r\n\r\n" +
					KPRes.WrongKeyDesc, PwDefs.ShortProductName,
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}

			swLogger.EndLogging();
			return fr;
		}

		private void AutoEnableVisualHiding()
		{
			// Turn on visual hiding if option is selected
			if(m_pwDatabase.MemoryProtection.AutoEnableVisualHiding)
			{
				if(m_pwDatabase.MemoryProtection.ProtectTitle && !m_viewHideFields.ProtectTitle)
					m_menuViewHideTitles.Checked = m_viewHideFields.ProtectTitle = true;
				if(m_pwDatabase.MemoryProtection.ProtectUserName && !m_viewHideFields.ProtectUserName)
					m_menuViewHideUserNames.Checked = m_viewHideFields.ProtectUserName = true;
				if(m_pwDatabase.MemoryProtection.ProtectPassword && !m_viewHideFields.ProtectPassword)
					m_menuViewHidePasswords.Checked = m_viewHideFields.ProtectPassword = true;
				if(m_pwDatabase.MemoryProtection.ProtectUrl && !m_viewHideFields.ProtectUrl)
					m_menuViewHideURLs.Checked = m_viewHideFields.ProtectUrl = true;
				if(m_pwDatabase.MemoryProtection.ProtectNotes && !m_viewHideFields.ProtectNotes)
					m_menuViewHideNotes.Checked = m_viewHideFields.ProtectNotes = true;
			}
		}

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
				if(((PwEntry)lvi.Tag).Uuid.EqualsValue(puSearch))
					return lvi;
			}

			return null;
		}

		private void PrintGroup(PwGroup pg)
		{
			Debug.Assert(pg != null); if(pg == null) return;
			if(!AppPolicy.Try(AppPolicyFlag.Print)) return;

			PrintForm pf = new PrintForm();
			pf.InitEx(pg, true);
			pf.ShowDialog();
		}

		private void ShowColumn(AppDefs.ColumnID colID, bool bShow)
		{
			m_vShowColumns[(int)colID] = bShow;

			if(bShow && (m_lvEntries.Columns[(int)colID].Width == 0))
			{
				m_lvEntries.Columns[(int)colID].Width = 100;
				RefreshEntriesList();
			}
			else if(!bShow && (m_lvEntries.Columns[(int)colID].Width != 0))
			{
				m_lvEntries.Columns[(int)colID].Width = 0;
			}

			switch(colID)
			{
				case AppDefs.ColumnID.Title: m_menuViewColumnsShowTitle.Checked = bShow; break;
				case AppDefs.ColumnID.UserName: m_menuViewColumnsShowUserName.Checked = bShow; break;
				case AppDefs.ColumnID.Password: m_menuViewColumnsShowPassword.Checked = bShow; break;
				case AppDefs.ColumnID.Url: m_menuViewColumnsShowUrl.Checked = bShow; break;
				case AppDefs.ColumnID.Notes: m_menuViewColumnsShowNotes.Checked = bShow; break;
				case AppDefs.ColumnID.CreationTime: m_menuViewColumnsShowCreation.Checked = bShow; break;
				case AppDefs.ColumnID.LastAccessTime: m_menuViewColumnsShowLastAccess.Checked = bShow; break;
				case AppDefs.ColumnID.LastModificationTime: m_menuViewColumnsShowLastMod.Checked = bShow; break;
				case AppDefs.ColumnID.ExpiryTime: m_menuViewColumnsShowExpire.Checked = bShow; break;
				case AppDefs.ColumnID.Uuid: m_menuViewColumnsShowUuid.Checked = bShow; break;
				case AppDefs.ColumnID.Attachment: m_menuViewColumnsShowAttachs.Checked = bShow; break;
				default: Debug.Assert(false); break;
			}
		}

		private void InsertToolStripItem(ToolStripMenuItem tsContainer, ToolStripMenuItem tsTemplate, EventHandler ev,
			bool bPermanentlyLinkToTemplate)
		{
			ToolStripMenuItem tsmi = new ToolStripMenuItem(tsTemplate.Text, tsTemplate.Image);
			tsmi.Click += ev;
			tsmi.ShortcutKeys = tsTemplate.ShortcutKeys;

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
		public void OnMruExecute(string strDisplayName, string strTag)
		{
			OpenDatabase(strTag, null, false);
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
			bool bWindowVisible = this.Visible;
			bool bTrayVisible = m_ntfTray.Visible;

			if(AppConfigEx.GetBool(AppDefs.ConfigKeys.ShowTrayIconOnlyIfTrayed))
				m_ntfTray.Visible = !bWindowVisible;
			else if(bWindowVisible && !bTrayVisible)
				m_ntfTray.Visible = true;
		}

		private void OnSessionLock(object sender, EventArgs e)
		{
			if(m_pwDatabase.IsOpen && !IsFileLocked())
			{
				if(AppConfigEx.GetBool(AppDefs.ConfigKeys.LockOnSessionLock))
					OnFileLock(sender, e);
			}
		}

		/// <summary>
		/// This function resets the internal user-inactivity timer.
		/// </summary>
		public void NotifyUserActivity()
		{
			m_nLockTimerCur = m_nLockTimerMax;
		}

		/// <summary>
		/// Move selected entries.
		/// </summary>
		/// <param name="nMove">Must be 2/-2 to move to top/bottom, 1/-1 to
		/// move one up/down.</param>
		private void MoveSelectedEntries(int nMove)
		{
			PwEntry[] vEntries = GetSelectedEntries();
			Debug.Assert(vEntries != null); if(vEntries == null) return;
			Debug.Assert(vEntries.Length > 0); if(vEntries.Length == 0) return;

			PwGroup pg = vEntries[0].ParentGroup;
			foreach(PwEntry pe in vEntries)
			{
				if(pe.ParentGroup != pg)
				{
					MessageBox.Show(KPRes.CannotMoveEntriesBcsGroup, PwDefs.ShortProductName,
						MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

			UpdateEntryList(null, true);
			UpdateUIState(true);
		}

		/// <summary>
		/// Create a new warnings logger object that logs directly into
		/// the main status bar until the first warning is shown (in that
		/// case a dialog is opened displaying the warning).
		/// </summary>
		/// <returns>Reference to the new logger object.</returns>
		public ShowWarningsLogger CreateShowWarningsLogger()
		{
			StatusBarLogger sl = new StatusBarLogger();
			sl.SetControls(m_statusPartInfo, m_statusPartProgress);

			return new ShowWarningsLogger(sl);
		}

		/// <summary>
		/// Overridden <c>WndProc</c> to handle global hot keys.
		/// </summary>
		/// <param name="m">Reference to the current Windows message.</param>
		protected override void WndProc(ref Message m)
		{
			if(m.Msg == HotKeyManager.WM_HOTKEY)
			{
				switch((int)m.WParam)
				{
					case (int)AppDefs.GlobalHotKeyID.AutoType:
						if(m_bIsAutoTyping) break;
						m_bIsAutoTyping = true;

						if(IsFileLocked())
						{
							IntPtr hPrevWnd = WinUtil.GetForegroundWindow();

							EnsureVisibleForegroundWindow();
							OnFileLock(null, null);

							WinUtil.EnsureForegroundWindow(hPrevWnd);
						}
						if(!m_pwDatabase.IsOpen) { m_bIsAutoTyping = false; break; }

						AutoType.PerformGlobal(m_pwDatabase, m_ilClientIcons);
						m_bIsAutoTyping = false;
						break;

					case (int)AppDefs.GlobalHotKeyID.ShowWindow:
						EnsureVisibleForegroundWindow();
						break;

					default:
						Debug.Assert(false);
						break;
				}
			}
			else if(m.Msg == m_nAppMessage) // KeePass application message
			{
				if(m.WParam == (IntPtr)Program.AppMessage.RestoreWindow)
					EnsureVisibleForegroundWindow();
			}

			base.WndProc(ref m);
		}

		private void EnsureVisibleForegroundWindow()
		{
			if(IsTrayed()) OnTrayTray(null, null);

			if(this.WindowState == FormWindowState.Minimized)
				this.WindowState = FormWindowState.Normal;

			this.BringToFront();
			this.Activate();
		}

		private void SetListFont(string strFontID)
		{
			Font f = WinUtil.FontIDToFont(strFontID);
			if(f != null)
			{
				m_tvGroups.Font = f;
				m_lvEntries.Font = f;
				m_richEntryView.Font = f;
			}

			m_fontExpired = new Font(m_tvGroups.Font, FontStyle.Strikeout);
		}

		private void SetSelectedEntryColor(Color clrBack)
		{
			if(m_pwDatabase.IsOpen == false) return;
			PwEntry[] vSelected = GetSelectedEntries();
			if((vSelected == null) || (vSelected.Length == 0)) return;

			foreach(PwEntry pe in vSelected)
				pe.BackgroundColor = clrBack;

			RefreshEntriesList();
			UpdateUIState(true);
		}

		private Image CreateColorBitmap(int nWidth, int nHeight, Color color)
		{
			Bitmap bmp = new Bitmap(nWidth, nHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			Graphics g = Graphics.FromImage(bmp);

			g.FillRectangle(new SolidBrush(color), 0, 0, nWidth, nHeight);
			return bmp;
		}

		private void OnCopyCustomString(object sender, DynamicMenuEventArgs e)
		{
			PwEntry pe = GetSelectedEntry(false);
			if(pe == null) return;

			ClipboardUtil.CopyAndMinimize(pe.Strings.ReadSafe(e.ItemName), true,
				AppConfigEx.GetBool(AppDefs.ConfigKeys.MinimizeAfterCopy) ?
				this : null);
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

			m_menuViewWindowsStacked.Checked = !bSideBySide;
			m_menuViewWindowsSideBySide.Checked = bSideBySide;
		}

		private static void UISelfTest()
		{
			Debug.Assert(((int)AppDefs.ColumnID.Title) == 0);
			Debug.Assert(((int)AppDefs.ColumnID.UserName) == 1);
		}

		private void AssignMenuShortcuts()
		{
			m_menuFileNew.ShortcutKeys = Keys.Control | Keys.N;
			m_menuFileOpenLocal.ShortcutKeys = Keys.Control | Keys.O;
			m_menuFileClose.ShortcutKeys = Keys.Control | Keys.W;
			m_menuFileSave.ShortcutKeys = Keys.Control | Keys.S;
			m_menuFilePrint.ShortcutKeys = Keys.Control | Keys.P;
			m_menuFileLock.ShortcutKeys = Keys.Control | Keys.L;

			m_menuEditFind.ShortcutKeys = Keys.Control | Keys.F;
		}

		private void SaveDatabaseAs(bool bOnline, object sender)
		{
			if(!m_pwDatabase.IsOpen) return;
			if(!AppPolicy.Try(AppPolicyFlag.SaveDatabase)) return;

			if(m_bShowClassicSaveWarning)
			{
				string strMessage = KPRes.Save1xAs2x + "\r\n\r\n" +
					KPRes.Save1xAs2xCompat + "\r\n\r\n" + KPRes.AskContinue;

				if(MessageBox.Show(strMessage, PwDefs.ShortProductName, MessageBoxButtons.YesNo,
					MessageBoxIcon.Question) == DialogResult.No)
				{
					return;
				}

				m_bShowClassicSaveWarning = false;
			}

			if(FileSaving != null)
			{
				FileSavingEventArgs args = new FileSavingEventArgs(true);
				FileSaving(sender, args);
				if(args.Cancel) return;
			}

			DialogResult dr;
			IOConnectionInfo ioc = new IOConnectionInfo();

			if(bOnline)
			{
				IOConnectionForm iocf = new IOConnectionForm();
				iocf.InitEx(true, m_pwDatabase.IOConnectionInfo.CloneDeep());

				dr = iocf.ShowDialog();
				ioc = iocf.IOConnectionInfo;
			}
			else
			{
				dr = m_saveDatabaseFile.ShowDialog();
				if(dr == DialogResult.OK)
					ioc = IOConnectionInfo.FromPath(m_saveDatabaseFile.FileName);
			}

			if(dr == DialogResult.OK)
			{
				ShowWarningsLogger swLogger = CreateShowWarningsLogger();
				swLogger.StartLogging(KPRes.SavingDatabase);

				FileSaveResult fsr = m_pwDatabase.SaveDatabaseTo(ioc, true, swLogger);

				if(fsr.Code != FileSaveResultCode.Success)
				{
					MessageBox.Show(ResUtil.FileSaveResultToString(fsr),
						PwDefs.ShortProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
				else
				{
					string strName = m_pwDatabase.IOConnectionInfo.GetDisplayName();
					string strPath = IOConnectionInfo.SerializeToString(
						m_pwDatabase.IOConnectionInfo);
					m_mruList.AddItem(strName, strPath);

					AppConfigEx.SetValue(AppDefs.ConfigKeys.LastDatabase, strPath);
				}

				swLogger.EndLogging();

				if(FileSaved != null)
				{
					FileSavedEventArgs args = new FileSavedEventArgs(fsr);
					FileSaved(sender, args);
				}
			}

			UpdateUIState(false);
		}

		public bool UIFileSave()
		{
			m_pwDatabase.Modified = true;
			OnFileSave(null, null);
			return !m_pwDatabase.Modified;
		}
	}
}
