namespace KeePass.Forms
{
	partial class MainForm
	{
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Verwendete Ressourcen bereinigen.
		/// </summary>
		/// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
		protected override void Dispose(bool disposing)
		{
			if(disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Vom Windows Form-Designer generierter Code

		/// <summary>
		/// Erforderliche Methode für die Designerunterstützung.
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.m_ctxGroupList = new KeePass.UI.CustomContextMenuStripEx(this.components);
			this.m_ctxGroupFind = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxGroupFindProfiles = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxPwList = new KeePass.UI.CustomContextMenuStripEx(this.components);
			this.m_ctxEntryUrl = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntryOtherData = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntryAttachments = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntrySep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_ctxEntryAutoTypeAdv = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntrySep1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_ctxEntryEditQuick = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntryTagAdd = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntryTagRemove = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntrySep2 = new System.Windows.Forms.ToolStripSeparator();
			this.m_ctxEntryRearrange = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntryMoveToGroup = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuMain = new KeePass.UI.CustomMenuStripEx();
			this.m_menuFile = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileNew = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileOpen = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileOpenLocal = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileOpenUrl = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileOpenSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuFileFind = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileFindInFolder = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileRecent = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileRecentDummy = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileClose = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuFileSave = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileSaveAs = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileSaveAsLocal = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileSaveAsUrl = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileSaveAsSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuFileSaveAsCopy = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileSep1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuFileDbSettings = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileChangeMasterKey = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileSep2 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuFilePrint = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFilePrintDatabase = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFilePrintSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuFilePrintEmSheet = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFilePrintKeyFile = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileSep3 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuFileImport = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileExport = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileSync = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileSyncFile = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileSyncUrl = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileSyncSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuFileSyncRecent = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileSyncRecentDummy = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileSep4 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuFileLock = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileExit = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuGroup = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuGroupAdd = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuGroupEdit = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuGroupDuplicate = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuGroupDelete = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuGroupEmptyRB = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuGroupSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuGroupRearrange = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuGroupMoveToTop = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuGroupMoveOneUp = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuGroupMoveOneDown = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuGroupMoveToBottom = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuGroupMoveSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuGroupMoveToPreviousParent = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuGroupMoveSep1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuGroupSort = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuGroupSortRec = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuGroupMoveSep2 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuGroupExpand = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuGroupCollapse = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuGroupDX = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuGroupClipCopy = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuGroupClipCopyPlain = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuGroupClipPaste = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuGroupDXSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuGroupPrint = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuGroupExport = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntry = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryCopyUserName = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryCopyPassword = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryUrl = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryOpenUrl = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryCopyUrl = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryOtherData = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryAttachments = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntrySep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuEntryPerformAutoType = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryAutoTypeAdv = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntrySep1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuEntryAdd = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryEdit = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryEditQuick = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryIcon = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryColor = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryColorStandard = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryColorSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuEntryColorLightRed = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryColorLightGreen = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryColorLightBlue = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryColorLightYellow = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryColorSep1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuEntryColorCustom = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryEditQuickSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuEntryTagAdd = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryTagNew = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryTagRemove = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryEditQuickSep1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuEntryExpiresNow = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryExpiresNever = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryEditQuickSep2 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuEntryOtpGenSettings = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryDuplicate = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryDelete = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntrySep2 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuEntrySelectAll = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntrySep3 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuEntryRearrange = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryMoveToTop = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryMoveOneUp = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryMoveOneDown = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryMoveToBottom = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryRearrangeSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuEntryMoveToGroup = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryMoveToPreviousParent = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryDX = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryClipCopy = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryClipCopyPlain = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryClipPaste = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryDXSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuEntryPrint = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEntryExport = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFind = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFindInDatabase = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFindInGroup = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFindProfiles = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFindSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuFindTag = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFindSep1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuFindAll = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFindParentGroup = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFindSep2 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuFindExp = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFindExpIn = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFindExp1 = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFindExp2 = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFindExp3 = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFindExp7 = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFindExp14 = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFindExp30 = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFindExp60 = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFindExpInSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuFindExpInF = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFindSep3 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuFindLastMod = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFindLarge = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFindSep4 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuFindDupPasswords = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFindSimPasswordsP = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFindSimPasswordsC = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFindPwQuality = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuView = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuChangeLanguage = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuViewShowToolBar = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewShowEntryView = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewWindowLayout = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewWindowsStacked = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewWindowsSideBySide = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewSep1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuViewAlwaysOnTop = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewSep2 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuViewConfigColumns = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewSortBy = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewTanOptions = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewTanSimpleList = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewTanIndices = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewEntryListGrouping = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewSep3 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuViewShowEntriesOfSubGroups = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuTools = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuToolsPwGenerator = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuToolsGeneratePwList = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuToolsSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuToolsTanWizard = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuToolsDb = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuToolsDbMaintenance = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuToolsDbSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuToolsDbDelDupEntries = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuToolsDbDelEmptyGroups = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuToolsDbDelUnusedIcons = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuToolsDbSep1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuToolsDbXmlRep = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuToolsAdv = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuToolsRecreateKeyFile = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuToolsSep1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuToolsTriggers = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuToolsPlugins = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuToolsSep2 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuToolsOptions = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuHelp = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuHelpContents = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuHelpSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuHelpWebsite = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuHelpDonate = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuHelpSep1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuHelpCheckForUpdates = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuHelpSep2 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuHelpAbout = new System.Windows.Forms.ToolStripMenuItem();
			this.m_toolMain = new KeePass.UI.CustomToolStripEx();
			this.m_tbNewDatabase = new System.Windows.Forms.ToolStripButton();
			this.m_tbOpenDatabase = new System.Windows.Forms.ToolStripButton();
			this.m_tbSaveDatabase = new System.Windows.Forms.ToolStripButton();
			this.m_tbSaveAll = new System.Windows.Forms.ToolStripButton();
			this.m_tbSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_tbAddEntry = new System.Windows.Forms.ToolStripSplitButton();
			this.m_tbAddEntryDefault = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tbSep1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_tbCopyUserName = new System.Windows.Forms.ToolStripButton();
			this.m_tbCopyPassword = new System.Windows.Forms.ToolStripButton();
			this.m_tbOpenUrl = new System.Windows.Forms.ToolStripSplitButton();
			this.m_tbOpenUrlDefault = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tbCopyUrl = new System.Windows.Forms.ToolStripButton();
			this.m_tbAutoType = new System.Windows.Forms.ToolStripButton();
			this.m_tbSep4 = new System.Windows.Forms.ToolStripSeparator();
			this.m_tbFind = new System.Windows.Forms.ToolStripButton();
			this.m_tbEntryViewsDropDown = new System.Windows.Forms.ToolStripDropDownButton();
			this.m_tbViewsShowAll = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tbViewsShowExpired = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tbSep2 = new System.Windows.Forms.ToolStripSeparator();
			this.m_tbLockWorkspace = new System.Windows.Forms.ToolStripButton();
			this.m_tbSep3 = new System.Windows.Forms.ToolStripSeparator();
			this.m_tbQuickFind = new System.Windows.Forms.ToolStripComboBox();
			this.m_tbCloseTab = new System.Windows.Forms.ToolStripButton();
			this.m_statusMain = new System.Windows.Forms.StatusStrip();
			this.m_statusPartSelected = new System.Windows.Forms.ToolStripStatusLabel();
			this.m_statusPartInfo = new System.Windows.Forms.ToolStripStatusLabel();
			this.m_statusPartProgress = new System.Windows.Forms.ToolStripProgressBar();
			this.m_statusClipboard = new System.Windows.Forms.ToolStripProgressBar();
			this.m_ctxTray = new KeePass.UI.CustomContextMenuStripEx(this.components);
			this.m_ctxTrayTray = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxTraySep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_ctxTrayGenPw = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxTraySep1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_ctxTrayOptions = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxTraySep2 = new System.Windows.Forms.ToolStripSeparator();
			this.m_ctxTrayCancel = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxTrayLock = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxTrayFileExit = new System.Windows.Forms.ToolStripMenuItem();
			this.m_timerMain = new System.Windows.Forms.Timer(this.components);
			this.m_tabMain = new System.Windows.Forms.TabControl();
			this.m_splitHorizontal = new KeePass.UI.CustomSplitContainerEx();
			this.m_splitVertical = new KeePass.UI.CustomSplitContainerEx();
			this.m_tvGroups = new KeePass.UI.CustomTreeViewEx();
			this.m_lvEntries = new KeePass.UI.CustomListViewEx();
			this.m_richEntryView = new KeePass.UI.CustomRichTextBoxEx();
			this.m_menuFindHistory = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxGroupList.SuspendLayout();
			this.m_ctxPwList.SuspendLayout();
			this.m_menuMain.SuspendLayout();
			this.m_toolMain.SuspendLayout();
			this.m_statusMain.SuspendLayout();
			this.m_ctxTray.SuspendLayout();
			this.m_splitHorizontal.Panel1.SuspendLayout();
			this.m_splitHorizontal.Panel2.SuspendLayout();
			this.m_splitHorizontal.SuspendLayout();
			this.m_splitVertical.Panel1.SuspendLayout();
			this.m_splitVertical.Panel2.SuspendLayout();
			this.m_splitVertical.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_ctxGroupList
			// 
			this.m_ctxGroupList.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_ctxGroupFind,
            this.m_ctxGroupFindProfiles});
			this.m_ctxGroupList.Name = "m_ctxGroupList";
			this.m_ctxGroupList.Size = new System.Drawing.Size(180, 48);
			this.m_ctxGroupList.Opening += new System.ComponentModel.CancelEventHandler(this.OnCtxGroupListOpening);
			// 
			// m_ctxGroupFind
			// 
			this.m_ctxGroupFind.Image = global::KeePass.Properties.Resources.B16x16_XMag;
			this.m_ctxGroupFind.Name = "m_ctxGroupFind";
			this.m_ctxGroupFind.Size = new System.Drawing.Size(179, 22);
			this.m_ctxGroupFind.Text = "&Find in This Group...";
			this.m_ctxGroupFind.Click += new System.EventHandler(this.OnFindInGroup);
			// 
			// m_ctxGroupFindProfiles
			// 
			this.m_ctxGroupFindProfiles.Name = "m_ctxGroupFindProfiles";
			this.m_ctxGroupFindProfiles.Size = new System.Drawing.Size(179, 22);
			this.m_ctxGroupFindProfiles.Text = "&Search Profiles";
			this.m_ctxGroupFindProfiles.DropDownOpening += new System.EventHandler(this.OnCtxGroupFindProfilesOpening);
			// 
			// m_ctxPwList
			// 
			this.m_ctxPwList.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_ctxEntryUrl,
            this.m_ctxEntryOtherData,
            this.m_ctxEntryAttachments,
            this.m_ctxEntrySep0,
            this.m_ctxEntryAutoTypeAdv,
            this.m_ctxEntrySep1,
            this.m_ctxEntryEditQuick,
            this.m_ctxEntrySep2,
            this.m_ctxEntryRearrange});
			this.m_ctxPwList.Name = "m_ctxPwList";
			this.m_ctxPwList.Size = new System.Drawing.Size(162, 154);
			this.m_ctxPwList.Opening += new System.ComponentModel.CancelEventHandler(this.OnCtxPwListOpening);
			// 
			// m_ctxEntryUrl
			// 
			this.m_ctxEntryUrl.Name = "m_ctxEntryUrl";
			this.m_ctxEntryUrl.Size = new System.Drawing.Size(161, 22);
			this.m_ctxEntryUrl.Text = "<URL(s)>";
			// 
			// m_ctxEntryOtherData
			// 
			this.m_ctxEntryOtherData.Name = "m_ctxEntryOtherData";
			this.m_ctxEntryOtherData.Size = new System.Drawing.Size(161, 22);
			this.m_ctxEntryOtherData.Text = "<OtherData>";
			// 
			// m_ctxEntryAttachments
			// 
			this.m_ctxEntryAttachments.Name = "m_ctxEntryAttachments";
			this.m_ctxEntryAttachments.Size = new System.Drawing.Size(161, 22);
			this.m_ctxEntryAttachments.Text = "<Attachments>";
			// 
			// m_ctxEntrySep0
			// 
			this.m_ctxEntrySep0.Name = "m_ctxEntrySep0";
			this.m_ctxEntrySep0.Size = new System.Drawing.Size(158, 6);
			// 
			// m_ctxEntryAutoTypeAdv
			// 
			this.m_ctxEntryAutoTypeAdv.Name = "m_ctxEntryAutoTypeAdv";
			this.m_ctxEntryAutoTypeAdv.Size = new System.Drawing.Size(161, 22);
			this.m_ctxEntryAutoTypeAdv.Text = "<AutoTypeAdv>";
			// 
			// m_ctxEntrySep1
			// 
			this.m_ctxEntrySep1.Name = "m_ctxEntrySep1";
			this.m_ctxEntrySep1.Size = new System.Drawing.Size(158, 6);
			// 
			// m_ctxEntryEditQuick
			// 
			this.m_ctxEntryEditQuick.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_ctxEntryTagAdd,
            this.m_ctxEntryTagRemove});
			this.m_ctxEntryEditQuick.Name = "m_ctxEntryEditQuick";
			this.m_ctxEntryEditQuick.Size = new System.Drawing.Size(161, 22);
			this.m_ctxEntryEditQuick.Text = "<EditQuick>";
			// 
			// m_ctxEntryTagAdd
			// 
			this.m_ctxEntryTagAdd.Name = "m_ctxEntryTagAdd";
			this.m_ctxEntryTagAdd.Size = new System.Drawing.Size(151, 22);
			this.m_ctxEntryTagAdd.Text = "<AddTag>";
			this.m_ctxEntryTagAdd.DropDownOpening += new System.EventHandler(this.OnEntryTagAddOpening);
			// 
			// m_ctxEntryTagRemove
			// 
			this.m_ctxEntryTagRemove.Name = "m_ctxEntryTagRemove";
			this.m_ctxEntryTagRemove.Size = new System.Drawing.Size(151, 22);
			this.m_ctxEntryTagRemove.Text = "<RemoveTag>";
			this.m_ctxEntryTagRemove.DropDownOpening += new System.EventHandler(this.OnEntryTagRemoveOpening);
			// 
			// m_ctxEntrySep2
			// 
			this.m_ctxEntrySep2.Name = "m_ctxEntrySep2";
			this.m_ctxEntrySep2.Size = new System.Drawing.Size(158, 6);
			// 
			// m_ctxEntryRearrange
			// 
			this.m_ctxEntryRearrange.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_ctxEntryMoveToGroup});
			this.m_ctxEntryRearrange.Name = "m_ctxEntryRearrange";
			this.m_ctxEntryRearrange.Size = new System.Drawing.Size(161, 22);
			this.m_ctxEntryRearrange.Text = "<Rearrange>";
			// 
			// m_ctxEntryMoveToGroup
			// 
			this.m_ctxEntryMoveToGroup.Name = "m_ctxEntryMoveToGroup";
			this.m_ctxEntryMoveToGroup.Size = new System.Drawing.Size(165, 22);
			this.m_ctxEntryMoveToGroup.Text = "<MoveToGroup>";
			this.m_ctxEntryMoveToGroup.DropDownOpening += new System.EventHandler(this.OnEntryMoveToGroupOpening);
			// 
			// m_menuMain
			// 
			this.m_menuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuFile,
            this.m_menuGroup,
            this.m_menuEntry,
            this.m_menuFind,
            this.m_menuView,
            this.m_menuTools,
            this.m_menuHelp});
			this.m_menuMain.Location = new System.Drawing.Point(0, 0);
			this.m_menuMain.Name = "m_menuMain";
			this.m_menuMain.Size = new System.Drawing.Size(654, 24);
			this.m_menuMain.TabIndex = 0;
			// 
			// m_menuFile
			// 
			this.m_menuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuFileNew,
            this.m_menuFileOpen,
            this.m_menuFileRecent,
            this.m_menuFileClose,
            this.m_menuFileSep0,
            this.m_menuFileSave,
            this.m_menuFileSaveAs,
            this.m_menuFileSep1,
            this.m_menuFileDbSettings,
            this.m_menuFileChangeMasterKey,
            this.m_menuFileSep2,
            this.m_menuFilePrint,
            this.m_menuFileSep3,
            this.m_menuFileImport,
            this.m_menuFileExport,
            this.m_menuFileSync,
            this.m_menuFileSep4,
            this.m_menuFileLock,
            this.m_menuFileExit});
			this.m_menuFile.Name = "m_menuFile";
			this.m_menuFile.Size = new System.Drawing.Size(37, 20);
			this.m_menuFile.Text = "&File";
			// 
			// m_menuFileNew
			// 
			this.m_menuFileNew.Image = global::KeePass.Properties.Resources.B16x16_FileNew;
			this.m_menuFileNew.Name = "m_menuFileNew";
			this.m_menuFileNew.Size = new System.Drawing.Size(185, 22);
			this.m_menuFileNew.Text = "&New...";
			this.m_menuFileNew.Click += new System.EventHandler(this.OnFileNew);
			// 
			// m_menuFileOpen
			// 
			this.m_menuFileOpen.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuFileOpenLocal,
            this.m_menuFileOpenUrl,
            this.m_menuFileOpenSep0,
            this.m_menuFileFind,
            this.m_menuFileFindInFolder});
			this.m_menuFileOpen.Name = "m_menuFileOpen";
			this.m_menuFileOpen.Size = new System.Drawing.Size(185, 22);
			this.m_menuFileOpen.Text = "&Open";
			// 
			// m_menuFileOpenLocal
			// 
			this.m_menuFileOpenLocal.Image = global::KeePass.Properties.Resources.B16x16_Folder_Yellow_Open;
			this.m_menuFileOpenLocal.Name = "m_menuFileOpenLocal";
			this.m_menuFileOpenLocal.Size = new System.Drawing.Size(189, 22);
			this.m_menuFileOpenLocal.Text = "Open &File...";
			this.m_menuFileOpenLocal.Click += new System.EventHandler(this.OnFileOpen);
			// 
			// m_menuFileOpenUrl
			// 
			this.m_menuFileOpenUrl.Image = global::KeePass.Properties.Resources.B16x16_Browser;
			this.m_menuFileOpenUrl.Name = "m_menuFileOpenUrl";
			this.m_menuFileOpenUrl.Size = new System.Drawing.Size(189, 22);
			this.m_menuFileOpenUrl.Text = "Open &URL...";
			this.m_menuFileOpenUrl.Click += new System.EventHandler(this.OnFileOpenUrl);
			// 
			// m_menuFileOpenSep0
			// 
			this.m_menuFileOpenSep0.Name = "m_menuFileOpenSep0";
			this.m_menuFileOpenSep0.Size = new System.Drawing.Size(186, 6);
			// 
			// m_menuFileFind
			// 
			this.m_menuFileFind.Image = global::KeePass.Properties.Resources.B16x16_XMag;
			this.m_menuFileFind.Name = "m_menuFileFind";
			this.m_menuFileFind.Size = new System.Drawing.Size(189, 22);
			this.m_menuFileFind.Text = "F&ind Files...";
			this.m_menuFileFind.Click += new System.EventHandler(this.OnFileFind);
			// 
			// m_menuFileFindInFolder
			// 
			this.m_menuFileFindInFolder.Image = global::KeePass.Properties.Resources.B16x16_XMag;
			this.m_menuFileFindInFolder.Name = "m_menuFileFindInFolder";
			this.m_menuFileFindInFolder.Size = new System.Drawing.Size(189, 22);
			this.m_menuFileFindInFolder.Text = "Fi&nd Files (In Folder)...";
			this.m_menuFileFindInFolder.Click += new System.EventHandler(this.OnFileFindInFolder);
			// 
			// m_menuFileRecent
			// 
			this.m_menuFileRecent.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuFileRecentDummy});
			this.m_menuFileRecent.Name = "m_menuFileRecent";
			this.m_menuFileRecent.Size = new System.Drawing.Size(185, 22);
			this.m_menuFileRecent.Text = "Open &Recent";
			// 
			// m_menuFileRecentDummy
			// 
			this.m_menuFileRecentDummy.Name = "m_menuFileRecentDummy";
			this.m_menuFileRecentDummy.Size = new System.Drawing.Size(90, 22);
			this.m_menuFileRecentDummy.Text = "<>";
			// 
			// m_menuFileClose
			// 
			this.m_menuFileClose.Image = global::KeePass.Properties.Resources.B16x16_File_Close;
			this.m_menuFileClose.Name = "m_menuFileClose";
			this.m_menuFileClose.Size = new System.Drawing.Size(185, 22);
			this.m_menuFileClose.Text = "&Close";
			this.m_menuFileClose.Click += new System.EventHandler(this.OnFileClose);
			// 
			// m_menuFileSep0
			// 
			this.m_menuFileSep0.Name = "m_menuFileSep0";
			this.m_menuFileSep0.Size = new System.Drawing.Size(182, 6);
			// 
			// m_menuFileSave
			// 
			this.m_menuFileSave.Image = global::KeePass.Properties.Resources.B16x16_FileSave;
			this.m_menuFileSave.Name = "m_menuFileSave";
			this.m_menuFileSave.Size = new System.Drawing.Size(185, 22);
			this.m_menuFileSave.Text = "&Save";
			this.m_menuFileSave.Click += new System.EventHandler(this.OnFileSave);
			// 
			// m_menuFileSaveAs
			// 
			this.m_menuFileSaveAs.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuFileSaveAsLocal,
            this.m_menuFileSaveAsUrl,
            this.m_menuFileSaveAsSep0,
            this.m_menuFileSaveAsCopy});
			this.m_menuFileSaveAs.Name = "m_menuFileSaveAs";
			this.m_menuFileSaveAs.Size = new System.Drawing.Size(185, 22);
			this.m_menuFileSaveAs.Text = "Save &As";
			// 
			// m_menuFileSaveAsLocal
			// 
			this.m_menuFileSaveAsLocal.Image = global::KeePass.Properties.Resources.B16x16_FileSaveAs;
			this.m_menuFileSaveAsLocal.Name = "m_menuFileSaveAsLocal";
			this.m_menuFileSaveAsLocal.Size = new System.Drawing.Size(173, 22);
			this.m_menuFileSaveAsLocal.Text = "Save to &File...";
			this.m_menuFileSaveAsLocal.Click += new System.EventHandler(this.OnFileSaveAs);
			// 
			// m_menuFileSaveAsUrl
			// 
			this.m_menuFileSaveAsUrl.Image = global::KeePass.Properties.Resources.B16x16_Browser;
			this.m_menuFileSaveAsUrl.Name = "m_menuFileSaveAsUrl";
			this.m_menuFileSaveAsUrl.Size = new System.Drawing.Size(173, 22);
			this.m_menuFileSaveAsUrl.Text = "Save to &URL...";
			this.m_menuFileSaveAsUrl.Click += new System.EventHandler(this.OnFileSaveAsUrl);
			// 
			// m_menuFileSaveAsSep0
			// 
			this.m_menuFileSaveAsSep0.Name = "m_menuFileSaveAsSep0";
			this.m_menuFileSaveAsSep0.Size = new System.Drawing.Size(170, 6);
			// 
			// m_menuFileSaveAsCopy
			// 
			this.m_menuFileSaveAsCopy.Image = global::KeePass.Properties.Resources.B16x16_FileSaveAs;
			this.m_menuFileSaveAsCopy.Name = "m_menuFileSaveAsCopy";
			this.m_menuFileSaveAsCopy.Size = new System.Drawing.Size(173, 22);
			this.m_menuFileSaveAsCopy.Text = "Save &Copy to File...";
			this.m_menuFileSaveAsCopy.Click += new System.EventHandler(this.OnFileSaveAsCopy);
			// 
			// m_menuFileSep1
			// 
			this.m_menuFileSep1.Name = "m_menuFileSep1";
			this.m_menuFileSep1.Size = new System.Drawing.Size(182, 6);
			// 
			// m_menuFileDbSettings
			// 
			this.m_menuFileDbSettings.Image = global::KeePass.Properties.Resources.B16x16_Package_Development;
			this.m_menuFileDbSettings.Name = "m_menuFileDbSettings";
			this.m_menuFileDbSettings.Size = new System.Drawing.Size(185, 22);
			this.m_menuFileDbSettings.Text = "&Database Settings...";
			this.m_menuFileDbSettings.Click += new System.EventHandler(this.OnFileDbSettings);
			// 
			// m_menuFileChangeMasterKey
			// 
			this.m_menuFileChangeMasterKey.Image = global::KeePass.Properties.Resources.B16x16_File_Locked;
			this.m_menuFileChangeMasterKey.Name = "m_menuFileChangeMasterKey";
			this.m_menuFileChangeMasterKey.Size = new System.Drawing.Size(185, 22);
			this.m_menuFileChangeMasterKey.Text = "Change &Master Key...";
			this.m_menuFileChangeMasterKey.Click += new System.EventHandler(this.OnFileChangeMasterKey);
			// 
			// m_menuFileSep2
			// 
			this.m_menuFileSep2.Name = "m_menuFileSep2";
			this.m_menuFileSep2.Size = new System.Drawing.Size(182, 6);
			// 
			// m_menuFilePrint
			// 
			this.m_menuFilePrint.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuFilePrintDatabase,
            this.m_menuFilePrintSep0,
            this.m_menuFilePrintEmSheet,
            this.m_menuFilePrintKeyFile});
			this.m_menuFilePrint.Name = "m_menuFilePrint";
			this.m_menuFilePrint.Size = new System.Drawing.Size(185, 22);
			this.m_menuFilePrint.Text = "&Print";
			// 
			// m_menuFilePrintDatabase
			// 
			this.m_menuFilePrintDatabase.Image = global::KeePass.Properties.Resources.B16x16_FilePrint;
			this.m_menuFilePrintDatabase.Name = "m_menuFilePrintDatabase";
			this.m_menuFilePrintDatabase.Size = new System.Drawing.Size(202, 22);
			this.m_menuFilePrintDatabase.Text = "&Print...";
			this.m_menuFilePrintDatabase.Click += new System.EventHandler(this.OnFilePrint);
			// 
			// m_menuFilePrintSep0
			// 
			this.m_menuFilePrintSep0.Name = "m_menuFilePrintSep0";
			this.m_menuFilePrintSep0.Size = new System.Drawing.Size(199, 6);
			// 
			// m_menuFilePrintEmSheet
			// 
			this.m_menuFilePrintEmSheet.Image = global::KeePass.Properties.Resources.B16x16_KOrganizer;
			this.m_menuFilePrintEmSheet.Name = "m_menuFilePrintEmSheet";
			this.m_menuFilePrintEmSheet.Size = new System.Drawing.Size(202, 22);
			this.m_menuFilePrintEmSheet.Text = "Print &Emergency Sheet...";
			this.m_menuFilePrintEmSheet.Click += new System.EventHandler(this.OnFilePrintEmSheet);
			// 
			// m_menuFilePrintKeyFile
			// 
			this.m_menuFilePrintKeyFile.Image = global::KeePass.Properties.Resources.B16x16_KOrganizer;
			this.m_menuFilePrintKeyFile.Name = "m_menuFilePrintKeyFile";
			this.m_menuFilePrintKeyFile.Size = new System.Drawing.Size(202, 22);
			this.m_menuFilePrintKeyFile.Text = "Print &Key File Backup...";
			this.m_menuFilePrintKeyFile.Click += new System.EventHandler(this.OnFilePrintKeyFile);
			// 
			// m_menuFileSep3
			// 
			this.m_menuFileSep3.Name = "m_menuFileSep3";
			this.m_menuFileSep3.Size = new System.Drawing.Size(182, 6);
			// 
			// m_menuFileImport
			// 
			this.m_menuFileImport.Image = global::KeePass.Properties.Resources.B16x16_Folder_Inbox;
			this.m_menuFileImport.Name = "m_menuFileImport";
			this.m_menuFileImport.Size = new System.Drawing.Size(185, 22);
			this.m_menuFileImport.Text = "&Import...";
			this.m_menuFileImport.Click += new System.EventHandler(this.OnFileImport);
			// 
			// m_menuFileExport
			// 
			this.m_menuFileExport.Image = global::KeePass.Properties.Resources.B16x16_Folder_Outbox;
			this.m_menuFileExport.Name = "m_menuFileExport";
			this.m_menuFileExport.Size = new System.Drawing.Size(185, 22);
			this.m_menuFileExport.Text = "&Export...";
			this.m_menuFileExport.Click += new System.EventHandler(this.OnFileExport);
			// 
			// m_menuFileSync
			// 
			this.m_menuFileSync.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuFileSyncFile,
            this.m_menuFileSyncUrl,
            this.m_menuFileSyncSep0,
            this.m_menuFileSyncRecent});
			this.m_menuFileSync.Name = "m_menuFileSync";
			this.m_menuFileSync.Size = new System.Drawing.Size(185, 22);
			this.m_menuFileSync.Text = "S&ynchronize";
			// 
			// m_menuFileSyncFile
			// 
			this.m_menuFileSyncFile.Image = global::KeePass.Properties.Resources.B16x16_Reload_Page;
			this.m_menuFileSyncFile.Name = "m_menuFileSyncFile";
			this.m_menuFileSyncFile.RightToLeftAutoMirrorImage = true;
			this.m_menuFileSyncFile.Size = new System.Drawing.Size(197, 22);
			this.m_menuFileSyncFile.Text = "Synchronize with &File...";
			this.m_menuFileSyncFile.Click += new System.EventHandler(this.OnFileSynchronize);
			// 
			// m_menuFileSyncUrl
			// 
			this.m_menuFileSyncUrl.Image = global::KeePass.Properties.Resources.B16x16_Reload_Page;
			this.m_menuFileSyncUrl.Name = "m_menuFileSyncUrl";
			this.m_menuFileSyncUrl.RightToLeftAutoMirrorImage = true;
			this.m_menuFileSyncUrl.Size = new System.Drawing.Size(197, 22);
			this.m_menuFileSyncUrl.Text = "Synchronize with &URL...";
			this.m_menuFileSyncUrl.Click += new System.EventHandler(this.OnFileSynchronizeUrl);
			// 
			// m_menuFileSyncSep0
			// 
			this.m_menuFileSyncSep0.Name = "m_menuFileSyncSep0";
			this.m_menuFileSyncSep0.Size = new System.Drawing.Size(194, 6);
			// 
			// m_menuFileSyncRecent
			// 
			this.m_menuFileSyncRecent.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuFileSyncRecentDummy});
			this.m_menuFileSyncRecent.Name = "m_menuFileSyncRecent";
			this.m_menuFileSyncRecent.Size = new System.Drawing.Size(197, 22);
			this.m_menuFileSyncRecent.Text = "&Recent Files";
			// 
			// m_menuFileSyncRecentDummy
			// 
			this.m_menuFileSyncRecentDummy.Name = "m_menuFileSyncRecentDummy";
			this.m_menuFileSyncRecentDummy.Size = new System.Drawing.Size(90, 22);
			this.m_menuFileSyncRecentDummy.Text = "<>";
			// 
			// m_menuFileSep4
			// 
			this.m_menuFileSep4.Name = "m_menuFileSep4";
			this.m_menuFileSep4.Size = new System.Drawing.Size(182, 6);
			// 
			// m_menuFileLock
			// 
			this.m_menuFileLock.Image = global::KeePass.Properties.Resources.B16x16_LockWorkspace;
			this.m_menuFileLock.Name = "m_menuFileLock";
			this.m_menuFileLock.Size = new System.Drawing.Size(185, 22);
			this.m_menuFileLock.Text = "&Lock Workspace";
			this.m_menuFileLock.Click += new System.EventHandler(this.OnFileLock);
			// 
			// m_menuFileExit
			// 
			this.m_menuFileExit.Image = global::KeePass.Properties.Resources.B16x16_Exit;
			this.m_menuFileExit.Name = "m_menuFileExit";
			this.m_menuFileExit.Size = new System.Drawing.Size(185, 22);
			this.m_menuFileExit.Text = "E&xit";
			this.m_menuFileExit.Click += new System.EventHandler(this.OnFileExit);
			// 
			// m_menuGroup
			// 
			this.m_menuGroup.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuGroupAdd,
            this.m_menuGroupEdit,
            this.m_menuGroupDuplicate,
            this.m_menuGroupDelete,
            this.m_menuGroupEmptyRB,
            this.m_menuGroupSep0,
            this.m_menuGroupRearrange,
            this.m_menuGroupDX});
			this.m_menuGroup.Name = "m_menuGroup";
			this.m_menuGroup.Size = new System.Drawing.Size(52, 20);
			this.m_menuGroup.Text = "&Group";
			this.m_menuGroup.DropDownOpening += new System.EventHandler(this.OnGroupDropDownOpening);
			// 
			// m_menuGroupAdd
			// 
			this.m_menuGroupAdd.Image = global::KeePass.Properties.Resources.B16x16_Folder_New_Ex;
			this.m_menuGroupAdd.Name = "m_menuGroupAdd";
			this.m_menuGroupAdd.Size = new System.Drawing.Size(171, 22);
			this.m_menuGroupAdd.Text = "&Add Group...";
			this.m_menuGroupAdd.Click += new System.EventHandler(this.OnGroupAdd);
			// 
			// m_menuGroupEdit
			// 
			this.m_menuGroupEdit.Image = global::KeePass.Properties.Resources.B16x16_Folder_Txt;
			this.m_menuGroupEdit.Name = "m_menuGroupEdit";
			this.m_menuGroupEdit.Size = new System.Drawing.Size(171, 22);
			this.m_menuGroupEdit.Text = "&Edit Group...";
			this.m_menuGroupEdit.Click += new System.EventHandler(this.OnGroupEdit);
			// 
			// m_menuGroupDuplicate
			// 
			this.m_menuGroupDuplicate.Image = global::KeePass.Properties.Resources.B16x16_Folder_2;
			this.m_menuGroupDuplicate.Name = "m_menuGroupDuplicate";
			this.m_menuGroupDuplicate.Size = new System.Drawing.Size(171, 22);
			this.m_menuGroupDuplicate.Text = "D&uplicate Group...";
			this.m_menuGroupDuplicate.Click += new System.EventHandler(this.OnGroupDuplicate);
			// 
			// m_menuGroupDelete
			// 
			this.m_menuGroupDelete.Image = global::KeePass.Properties.Resources.B16x16_Folder_Locked;
			this.m_menuGroupDelete.Name = "m_menuGroupDelete";
			this.m_menuGroupDelete.Size = new System.Drawing.Size(171, 22);
			this.m_menuGroupDelete.Text = "&Delete Group";
			this.m_menuGroupDelete.Click += new System.EventHandler(this.OnGroupDelete);
			// 
			// m_menuGroupEmptyRB
			// 
			this.m_menuGroupEmptyRB.Image = global::KeePass.Properties.Resources.B16x16_Trashcan_Full;
			this.m_menuGroupEmptyRB.Name = "m_menuGroupEmptyRB";
			this.m_menuGroupEmptyRB.Size = new System.Drawing.Size(171, 22);
			this.m_menuGroupEmptyRB.Text = "E&mpty Recycle Bin";
			this.m_menuGroupEmptyRB.Click += new System.EventHandler(this.OnGroupEmptyRB);
			// 
			// m_menuGroupSep0
			// 
			this.m_menuGroupSep0.Name = "m_menuGroupSep0";
			this.m_menuGroupSep0.Size = new System.Drawing.Size(168, 6);
			// 
			// m_menuGroupRearrange
			// 
			this.m_menuGroupRearrange.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuGroupMoveToTop,
            this.m_menuGroupMoveOneUp,
            this.m_menuGroupMoveOneDown,
            this.m_menuGroupMoveToBottom,
            this.m_menuGroupMoveSep0,
            this.m_menuGroupMoveToPreviousParent,
            this.m_menuGroupMoveSep1,
            this.m_menuGroupSort,
            this.m_menuGroupSortRec,
            this.m_menuGroupMoveSep2,
            this.m_menuGroupExpand,
            this.m_menuGroupCollapse});
			this.m_menuGroupRearrange.Name = "m_menuGroupRearrange";
			this.m_menuGroupRearrange.Size = new System.Drawing.Size(171, 22);
			this.m_menuGroupRearrange.Text = "&Rearrange";
			// 
			// m_menuGroupMoveToTop
			// 
			this.m_menuGroupMoveToTop.Image = global::KeePass.Properties.Resources.B16x16_2UpArrow;
			this.m_menuGroupMoveToTop.Name = "m_menuGroupMoveToTop";
			this.m_menuGroupMoveToTop.Size = new System.Drawing.Size(199, 22);
			this.m_menuGroupMoveToTop.Text = "Move Group to &Top";
			this.m_menuGroupMoveToTop.Click += new System.EventHandler(this.OnGroupMoveToTop);
			// 
			// m_menuGroupMoveOneUp
			// 
			this.m_menuGroupMoveOneUp.Image = global::KeePass.Properties.Resources.B16x16_1UpArrow;
			this.m_menuGroupMoveOneUp.Name = "m_menuGroupMoveOneUp";
			this.m_menuGroupMoveOneUp.Size = new System.Drawing.Size(199, 22);
			this.m_menuGroupMoveOneUp.Text = "Move Group One &Up";
			this.m_menuGroupMoveOneUp.Click += new System.EventHandler(this.OnGroupMoveOneUp);
			// 
			// m_menuGroupMoveOneDown
			// 
			this.m_menuGroupMoveOneDown.Image = global::KeePass.Properties.Resources.B16x16_1DownArrow;
			this.m_menuGroupMoveOneDown.Name = "m_menuGroupMoveOneDown";
			this.m_menuGroupMoveOneDown.Size = new System.Drawing.Size(199, 22);
			this.m_menuGroupMoveOneDown.Text = "Move Group One &Down";
			this.m_menuGroupMoveOneDown.Click += new System.EventHandler(this.OnGroupMoveOneDown);
			// 
			// m_menuGroupMoveToBottom
			// 
			this.m_menuGroupMoveToBottom.Image = global::KeePass.Properties.Resources.B16x16_2DownArrow;
			this.m_menuGroupMoveToBottom.Name = "m_menuGroupMoveToBottom";
			this.m_menuGroupMoveToBottom.Size = new System.Drawing.Size(199, 22);
			this.m_menuGroupMoveToBottom.Text = "Move Group to &Bottom";
			this.m_menuGroupMoveToBottom.Click += new System.EventHandler(this.OnGroupMoveToBottom);
			// 
			// m_menuGroupMoveSep0
			// 
			this.m_menuGroupMoveSep0.Name = "m_menuGroupMoveSep0";
			this.m_menuGroupMoveSep0.Size = new System.Drawing.Size(196, 6);
			// 
			// m_menuGroupMoveToPreviousParent
			// 
			this.m_menuGroupMoveToPreviousParent.Image = global::KeePass.Properties.Resources.B16x16_Undo;
			this.m_menuGroupMoveToPreviousParent.Name = "m_menuGroupMoveToPreviousParent";
			this.m_menuGroupMoveToPreviousParent.Size = new System.Drawing.Size(199, 22);
			this.m_menuGroupMoveToPreviousParent.Text = "<>";
			this.m_menuGroupMoveToPreviousParent.Click += new System.EventHandler(this.OnGroupMoveToPreviousParent);
			// 
			// m_menuGroupMoveSep1
			// 
			this.m_menuGroupMoveSep1.Name = "m_menuGroupMoveSep1";
			this.m_menuGroupMoveSep1.Size = new System.Drawing.Size(196, 6);
			// 
			// m_menuGroupSort
			// 
			this.m_menuGroupSort.Image = global::KeePass.Properties.Resources.B16x16_KaboodleLoop;
			this.m_menuGroupSort.Name = "m_menuGroupSort";
			this.m_menuGroupSort.Size = new System.Drawing.Size(199, 22);
			this.m_menuGroupSort.Text = "&Sort Direct Subgroups";
			this.m_menuGroupSort.Click += new System.EventHandler(this.OnGroupSort);
			// 
			// m_menuGroupSortRec
			// 
			this.m_menuGroupSortRec.Image = global::KeePass.Properties.Resources.B16x16_KaboodleLoop;
			this.m_menuGroupSortRec.Name = "m_menuGroupSortRec";
			this.m_menuGroupSortRec.Size = new System.Drawing.Size(199, 22);
			this.m_menuGroupSortRec.Text = "Sort &Recursively";
			this.m_menuGroupSortRec.Click += new System.EventHandler(this.OnGroupSortRec);
			// 
			// m_menuGroupMoveSep2
			// 
			this.m_menuGroupMoveSep2.Name = "m_menuGroupMoveSep2";
			this.m_menuGroupMoveSep2.Size = new System.Drawing.Size(196, 6);
			// 
			// m_menuGroupExpand
			// 
			this.m_menuGroupExpand.Image = global::KeePass.Properties.Resources.B16x16_Folder_Blue_Open;
			this.m_menuGroupExpand.Name = "m_menuGroupExpand";
			this.m_menuGroupExpand.Size = new System.Drawing.Size(199, 22);
			this.m_menuGroupExpand.Text = "&Expand Recursively";
			this.m_menuGroupExpand.Click += new System.EventHandler(this.OnGroupExpand);
			// 
			// m_menuGroupCollapse
			// 
			this.m_menuGroupCollapse.Image = global::KeePass.Properties.Resources.B16x16_Folder;
			this.m_menuGroupCollapse.Name = "m_menuGroupCollapse";
			this.m_menuGroupCollapse.Size = new System.Drawing.Size(199, 22);
			this.m_menuGroupCollapse.Text = "&Collapse Recursively";
			this.m_menuGroupCollapse.Click += new System.EventHandler(this.OnGroupCollapse);
			// 
			// m_menuGroupDX
			// 
			this.m_menuGroupDX.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuGroupClipCopy,
            this.m_menuGroupClipCopyPlain,
            this.m_menuGroupClipPaste,
            this.m_menuGroupDXSep0,
            this.m_menuGroupPrint,
            this.m_menuGroupExport});
			this.m_menuGroupDX.Name = "m_menuGroupDX";
			this.m_menuGroupDX.Size = new System.Drawing.Size(171, 22);
			this.m_menuGroupDX.Text = "Data E&xchange";
			this.m_menuGroupDX.DropDownOpening += new System.EventHandler(this.OnGroupDXOpening);
			// 
			// m_menuGroupClipCopy
			// 
			this.m_menuGroupClipCopy.Image = global::KeePass.Properties.Resources.B16x16_EditCopy;
			this.m_menuGroupClipCopy.Name = "m_menuGroupClipCopy";
			this.m_menuGroupClipCopy.Size = new System.Drawing.Size(217, 22);
			this.m_menuGroupClipCopy.Text = "&Copy Group (Encrypted)";
			this.m_menuGroupClipCopy.Click += new System.EventHandler(this.OnGroupClipCopy);
			// 
			// m_menuGroupClipCopyPlain
			// 
			this.m_menuGroupClipCopyPlain.Image = global::KeePass.Properties.Resources.B16x16_EditCopy;
			this.m_menuGroupClipCopyPlain.Name = "m_menuGroupClipCopyPlain";
			this.m_menuGroupClipCopyPlain.Size = new System.Drawing.Size(217, 22);
			this.m_menuGroupClipCopyPlain.Text = "C&opy Group (Unencrypted)";
			this.m_menuGroupClipCopyPlain.Click += new System.EventHandler(this.OnGroupClipCopyPlain);
			// 
			// m_menuGroupClipPaste
			// 
			this.m_menuGroupClipPaste.Image = global::KeePass.Properties.Resources.B16x16_EditPaste;
			this.m_menuGroupClipPaste.Name = "m_menuGroupClipPaste";
			this.m_menuGroupClipPaste.Size = new System.Drawing.Size(217, 22);
			this.m_menuGroupClipPaste.Text = "&Paste Group";
			this.m_menuGroupClipPaste.Click += new System.EventHandler(this.OnGroupClipPaste);
			// 
			// m_menuGroupDXSep0
			// 
			this.m_menuGroupDXSep0.Name = "m_menuGroupDXSep0";
			this.m_menuGroupDXSep0.Size = new System.Drawing.Size(214, 6);
			// 
			// m_menuGroupPrint
			// 
			this.m_menuGroupPrint.Image = global::KeePass.Properties.Resources.B16x16_FilePrint;
			this.m_menuGroupPrint.Name = "m_menuGroupPrint";
			this.m_menuGroupPrint.Size = new System.Drawing.Size(217, 22);
			this.m_menuGroupPrint.Text = "P&rint Group...";
			this.m_menuGroupPrint.Click += new System.EventHandler(this.OnGroupPrint);
			// 
			// m_menuGroupExport
			// 
			this.m_menuGroupExport.Image = global::KeePass.Properties.Resources.B16x16_Folder_Outbox;
			this.m_menuGroupExport.Name = "m_menuGroupExport";
			this.m_menuGroupExport.Size = new System.Drawing.Size(217, 22);
			this.m_menuGroupExport.Text = "&Export Group...";
			this.m_menuGroupExport.Click += new System.EventHandler(this.OnGroupExport);
			// 
			// m_menuEntry
			// 
			this.m_menuEntry.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuEntryCopyUserName,
            this.m_menuEntryCopyPassword,
            this.m_menuEntryUrl,
            this.m_menuEntryOtherData,
            this.m_menuEntryAttachments,
            this.m_menuEntrySep0,
            this.m_menuEntryPerformAutoType,
            this.m_menuEntryAutoTypeAdv,
            this.m_menuEntrySep1,
            this.m_menuEntryAdd,
            this.m_menuEntryEdit,
            this.m_menuEntryEditQuick,
            this.m_menuEntryDuplicate,
            this.m_menuEntryDelete,
            this.m_menuEntrySep2,
            this.m_menuEntrySelectAll,
            this.m_menuEntrySep3,
            this.m_menuEntryRearrange,
            this.m_menuEntryDX});
			this.m_menuEntry.Name = "m_menuEntry";
			this.m_menuEntry.Size = new System.Drawing.Size(46, 20);
			this.m_menuEntry.Text = "&Entry";
			this.m_menuEntry.DropDownOpening += new System.EventHandler(this.OnEntryDropDownOpening);
			// 
			// m_menuEntryCopyUserName
			// 
			this.m_menuEntryCopyUserName.Image = global::KeePass.Properties.Resources.B16x16_Personal;
			this.m_menuEntryCopyUserName.Name = "m_menuEntryCopyUserName";
			this.m_menuEntryCopyUserName.Size = new System.Drawing.Size(175, 22);
			this.m_menuEntryCopyUserName.Text = "Copy &User Name";
			this.m_menuEntryCopyUserName.Click += new System.EventHandler(this.OnEntryCopyUserName);
			// 
			// m_menuEntryCopyPassword
			// 
			this.m_menuEntryCopyPassword.Image = global::KeePass.Properties.Resources.B16x16_KGPG_Info;
			this.m_menuEntryCopyPassword.Name = "m_menuEntryCopyPassword";
			this.m_menuEntryCopyPassword.Size = new System.Drawing.Size(175, 22);
			this.m_menuEntryCopyPassword.Text = "<>";
			this.m_menuEntryCopyPassword.Click += new System.EventHandler(this.OnEntryCopyPassword);
			// 
			// m_menuEntryUrl
			// 
			this.m_menuEntryUrl.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuEntryOpenUrl,
            this.m_menuEntryCopyUrl});
			this.m_menuEntryUrl.Name = "m_menuEntryUrl";
			this.m_menuEntryUrl.Size = new System.Drawing.Size(175, 22);
			this.m_menuEntryUrl.Text = "UR&L(s)";
			// 
			// m_menuEntryOpenUrl
			// 
			this.m_menuEntryOpenUrl.Image = global::KeePass.Properties.Resources.B16x16_FTP;
			this.m_menuEntryOpenUrl.Name = "m_menuEntryOpenUrl";
			this.m_menuEntryOpenUrl.Size = new System.Drawing.Size(103, 22);
			this.m_menuEntryOpenUrl.Text = "&Open";
			this.m_menuEntryOpenUrl.Click += new System.EventHandler(this.OnEntryOpenUrl);
			// 
			// m_menuEntryCopyUrl
			// 
			this.m_menuEntryCopyUrl.Image = global::KeePass.Properties.Resources.B16x16_EditCopyUrl;
			this.m_menuEntryCopyUrl.Name = "m_menuEntryCopyUrl";
			this.m_menuEntryCopyUrl.Size = new System.Drawing.Size(103, 22);
			this.m_menuEntryCopyUrl.Text = "&Copy";
			this.m_menuEntryCopyUrl.Click += new System.EventHandler(this.OnEntryCopyURL);
			// 
			// m_menuEntryOtherData
			// 
			this.m_menuEntryOtherData.Name = "m_menuEntryOtherData";
			this.m_menuEntryOtherData.Size = new System.Drawing.Size(175, 22);
			this.m_menuEntryOtherData.Text = "&Other Data";
			// 
			// m_menuEntryAttachments
			// 
			this.m_menuEntryAttachments.Name = "m_menuEntryAttachments";
			this.m_menuEntryAttachments.Size = new System.Drawing.Size(175, 22);
			this.m_menuEntryAttachments.Text = "Attach&ments";
			// 
			// m_menuEntrySep0
			// 
			this.m_menuEntrySep0.Name = "m_menuEntrySep0";
			this.m_menuEntrySep0.Size = new System.Drawing.Size(172, 6);
			// 
			// m_menuEntryPerformAutoType
			// 
			this.m_menuEntryPerformAutoType.Image = global::KeePass.Properties.Resources.B16x16_KTouch;
			this.m_menuEntryPerformAutoType.Name = "m_menuEntryPerformAutoType";
			this.m_menuEntryPerformAutoType.Size = new System.Drawing.Size(175, 22);
			this.m_menuEntryPerformAutoType.Text = "Perform Auto-&Type";
			this.m_menuEntryPerformAutoType.Click += new System.EventHandler(this.OnEntryPerformAutoType);
			// 
			// m_menuEntryAutoTypeAdv
			// 
			this.m_menuEntryAutoTypeAdv.Name = "m_menuEntryAutoTypeAdv";
			this.m_menuEntryAutoTypeAdv.Size = new System.Drawing.Size(175, 22);
			this.m_menuEntryAutoTypeAdv.Text = "Per&form Auto-Type";
			// 
			// m_menuEntrySep1
			// 
			this.m_menuEntrySep1.Name = "m_menuEntrySep1";
			this.m_menuEntrySep1.Size = new System.Drawing.Size(172, 6);
			// 
			// m_menuEntryAdd
			// 
			this.m_menuEntryAdd.Image = global::KeePass.Properties.Resources.B16x16_KGPG_Import;
			this.m_menuEntryAdd.Name = "m_menuEntryAdd";
			this.m_menuEntryAdd.Size = new System.Drawing.Size(175, 22);
			this.m_menuEntryAdd.Text = "&Add Entry...";
			this.m_menuEntryAdd.Click += new System.EventHandler(this.OnEntryAdd);
			// 
			// m_menuEntryEdit
			// 
			this.m_menuEntryEdit.Image = global::KeePass.Properties.Resources.B16x16_KGPG_Sign;
			this.m_menuEntryEdit.Name = "m_menuEntryEdit";
			this.m_menuEntryEdit.Size = new System.Drawing.Size(175, 22);
			this.m_menuEntryEdit.Text = "<>";
			this.m_menuEntryEdit.Click += new System.EventHandler(this.OnEntryEdit);
			// 
			// m_menuEntryEditQuick
			// 
			this.m_menuEntryEditQuick.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuEntryIcon,
            this.m_menuEntryColor,
            this.m_menuEntryEditQuickSep0,
            this.m_menuEntryTagAdd,
            this.m_menuEntryTagRemove,
            this.m_menuEntryEditQuickSep1,
            this.m_menuEntryExpiresNow,
            this.m_menuEntryExpiresNever,
            this.m_menuEntryEditQuickSep2,
            this.m_menuEntryOtpGenSettings});
			this.m_menuEntryEditQuick.Name = "m_menuEntryEditQuick";
			this.m_menuEntryEditQuick.Size = new System.Drawing.Size(175, 22);
			this.m_menuEntryEditQuick.Text = "<>";
			// 
			// m_menuEntryIcon
			// 
			this.m_menuEntryIcon.Image = global::KeePass.Properties.Resources.B16x16_Spreadsheet;
			this.m_menuEntryIcon.Name = "m_menuEntryIcon";
			this.m_menuEntryIcon.Size = new System.Drawing.Size(204, 22);
			this.m_menuEntryIcon.Text = "&Icon...";
			this.m_menuEntryIcon.Click += new System.EventHandler(this.OnEntryMassSetIcon);
			// 
			// m_menuEntryColor
			// 
			this.m_menuEntryColor.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuEntryColorStandard,
            this.m_menuEntryColorSep0,
            this.m_menuEntryColorLightRed,
            this.m_menuEntryColorLightGreen,
            this.m_menuEntryColorLightBlue,
            this.m_menuEntryColorLightYellow,
            this.m_menuEntryColorSep1,
            this.m_menuEntryColorCustom});
			this.m_menuEntryColor.Name = "m_menuEntryColor";
			this.m_menuEntryColor.Size = new System.Drawing.Size(204, 22);
			this.m_menuEntryColor.Text = "&Color";
			// 
			// m_menuEntryColorStandard
			// 
			this.m_menuEntryColorStandard.Name = "m_menuEntryColorStandard";
			this.m_menuEntryColorStandard.Size = new System.Drawing.Size(157, 22);
			this.m_menuEntryColorStandard.Text = "&Standard";
			this.m_menuEntryColorStandard.Click += new System.EventHandler(this.OnEntryColorStandard);
			// 
			// m_menuEntryColorSep0
			// 
			this.m_menuEntryColorSep0.Name = "m_menuEntryColorSep0";
			this.m_menuEntryColorSep0.Size = new System.Drawing.Size(154, 6);
			// 
			// m_menuEntryColorLightRed
			// 
			this.m_menuEntryColorLightRed.Name = "m_menuEntryColorLightRed";
			this.m_menuEntryColorLightRed.Size = new System.Drawing.Size(157, 22);
			this.m_menuEntryColorLightRed.Text = "Light &Red";
			this.m_menuEntryColorLightRed.Click += new System.EventHandler(this.OnEntryColorLightRed);
			// 
			// m_menuEntryColorLightGreen
			// 
			this.m_menuEntryColorLightGreen.Name = "m_menuEntryColorLightGreen";
			this.m_menuEntryColorLightGreen.Size = new System.Drawing.Size(157, 22);
			this.m_menuEntryColorLightGreen.Text = "Light &Green";
			this.m_menuEntryColorLightGreen.Click += new System.EventHandler(this.OnEntryColorLightGreen);
			// 
			// m_menuEntryColorLightBlue
			// 
			this.m_menuEntryColorLightBlue.Name = "m_menuEntryColorLightBlue";
			this.m_menuEntryColorLightBlue.Size = new System.Drawing.Size(157, 22);
			this.m_menuEntryColorLightBlue.Text = "Light &Blue";
			this.m_menuEntryColorLightBlue.Click += new System.EventHandler(this.OnEntryColorLightBlue);
			// 
			// m_menuEntryColorLightYellow
			// 
			this.m_menuEntryColorLightYellow.Name = "m_menuEntryColorLightYellow";
			this.m_menuEntryColorLightYellow.Size = new System.Drawing.Size(157, 22);
			this.m_menuEntryColorLightYellow.Text = "Light &Yellow";
			this.m_menuEntryColorLightYellow.Click += new System.EventHandler(this.OnEntryColorLightYellow);
			// 
			// m_menuEntryColorSep1
			// 
			this.m_menuEntryColorSep1.Name = "m_menuEntryColorSep1";
			this.m_menuEntryColorSep1.Size = new System.Drawing.Size(154, 6);
			// 
			// m_menuEntryColorCustom
			// 
			this.m_menuEntryColorCustom.Name = "m_menuEntryColorCustom";
			this.m_menuEntryColorCustom.Size = new System.Drawing.Size(157, 22);
			this.m_menuEntryColorCustom.Text = "&Custom Color...";
			this.m_menuEntryColorCustom.Click += new System.EventHandler(this.OnEntryColorCustom);
			// 
			// m_menuEntryEditQuickSep0
			// 
			this.m_menuEntryEditQuickSep0.Name = "m_menuEntryEditQuickSep0";
			this.m_menuEntryEditQuickSep0.Size = new System.Drawing.Size(201, 6);
			// 
			// m_menuEntryTagAdd
			// 
			this.m_menuEntryTagAdd.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuEntryTagNew});
			this.m_menuEntryTagAdd.Name = "m_menuEntryTagAdd";
			this.m_menuEntryTagAdd.Size = new System.Drawing.Size(204, 22);
			this.m_menuEntryTagAdd.Text = "&Add Tag";
			this.m_menuEntryTagAdd.DropDownOpening += new System.EventHandler(this.OnEntryTagAddOpening);
			// 
			// m_menuEntryTagNew
			// 
			this.m_menuEntryTagNew.Image = global::KeePass.Properties.Resources.B16x16_KNotes;
			this.m_menuEntryTagNew.Name = "m_menuEntryTagNew";
			this.m_menuEntryTagNew.Size = new System.Drawing.Size(128, 22);
			this.m_menuEntryTagNew.Text = "&New Tag...";
			this.m_menuEntryTagNew.Click += new System.EventHandler(this.OnEntrySelectedNewTag);
			// 
			// m_menuEntryTagRemove
			// 
			this.m_menuEntryTagRemove.Name = "m_menuEntryTagRemove";
			this.m_menuEntryTagRemove.Size = new System.Drawing.Size(204, 22);
			this.m_menuEntryTagRemove.Text = "&Remove Tag";
			this.m_menuEntryTagRemove.DropDownOpening += new System.EventHandler(this.OnEntryTagRemoveOpening);
			// 
			// m_menuEntryEditQuickSep1
			// 
			this.m_menuEntryEditQuickSep1.Name = "m_menuEntryEditQuickSep1";
			this.m_menuEntryEditQuickSep1.Size = new System.Drawing.Size(201, 6);
			// 
			// m_menuEntryExpiresNow
			// 
			this.m_menuEntryExpiresNow.Image = global::KeePass.Properties.Resources.B16x16_History_Clear;
			this.m_menuEntryExpiresNow.Name = "m_menuEntryExpiresNow";
			this.m_menuEntryExpiresNow.Size = new System.Drawing.Size(204, 22);
			this.m_menuEntryExpiresNow.Text = "&Expires: Now";
			this.m_menuEntryExpiresNow.Click += new System.EventHandler(this.OnEntryExpiresNow);
			// 
			// m_menuEntryExpiresNever
			// 
			this.m_menuEntryExpiresNever.Image = global::KeePass.Properties.Resources.B16x16_History_Clear;
			this.m_menuEntryExpiresNever.Name = "m_menuEntryExpiresNever";
			this.m_menuEntryExpiresNever.Size = new System.Drawing.Size(204, 22);
			this.m_menuEntryExpiresNever.Text = "Expires: &Never";
			this.m_menuEntryExpiresNever.Click += new System.EventHandler(this.OnEntryExpiresNever);
			// 
			// m_menuEntryEditQuickSep2
			// 
			this.m_menuEntryEditQuickSep2.Name = "m_menuEntryEditQuickSep2";
			this.m_menuEntryEditQuickSep2.Size = new System.Drawing.Size(201, 6);
			// 
			// m_menuEntryOtpGenSettings
			// 
			this.m_menuEntryOtpGenSettings.Image = global::KeePass.Properties.Resources.B16x16_KGPG_Gen;
			this.m_menuEntryOtpGenSettings.Name = "m_menuEntryOtpGenSettings";
			this.m_menuEntryOtpGenSettings.Size = new System.Drawing.Size(204, 22);
			this.m_menuEntryOtpGenSettings.Text = "&OTP Generator Settings...";
			this.m_menuEntryOtpGenSettings.Click += new System.EventHandler(this.OnEntryOtpGenSettings);
			// 
			// m_menuEntryDuplicate
			// 
			this.m_menuEntryDuplicate.Image = global::KeePass.Properties.Resources.B16x16_KGPG_Key2;
			this.m_menuEntryDuplicate.Name = "m_menuEntryDuplicate";
			this.m_menuEntryDuplicate.Size = new System.Drawing.Size(175, 22);
			this.m_menuEntryDuplicate.Text = "<>";
			this.m_menuEntryDuplicate.Click += new System.EventHandler(this.OnEntryDuplicate);
			// 
			// m_menuEntryDelete
			// 
			this.m_menuEntryDelete.Image = global::KeePass.Properties.Resources.B16x16_DeleteEntry;
			this.m_menuEntryDelete.Name = "m_menuEntryDelete";
			this.m_menuEntryDelete.Size = new System.Drawing.Size(175, 22);
			this.m_menuEntryDelete.Text = "<>";
			this.m_menuEntryDelete.Click += new System.EventHandler(this.OnEntryDelete);
			// 
			// m_menuEntrySep2
			// 
			this.m_menuEntrySep2.Name = "m_menuEntrySep2";
			this.m_menuEntrySep2.Size = new System.Drawing.Size(172, 6);
			// 
			// m_menuEntrySelectAll
			// 
			this.m_menuEntrySelectAll.Name = "m_menuEntrySelectAll";
			this.m_menuEntrySelectAll.Size = new System.Drawing.Size(175, 22);
			this.m_menuEntrySelectAll.Text = "&Select All";
			this.m_menuEntrySelectAll.Click += new System.EventHandler(this.OnEntrySelectAll);
			// 
			// m_menuEntrySep3
			// 
			this.m_menuEntrySep3.Name = "m_menuEntrySep3";
			this.m_menuEntrySep3.Size = new System.Drawing.Size(172, 6);
			// 
			// m_menuEntryRearrange
			// 
			this.m_menuEntryRearrange.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuEntryMoveToTop,
            this.m_menuEntryMoveOneUp,
            this.m_menuEntryMoveOneDown,
            this.m_menuEntryMoveToBottom,
            this.m_menuEntryRearrangeSep0,
            this.m_menuEntryMoveToGroup,
            this.m_menuEntryMoveToPreviousParent});
			this.m_menuEntryRearrange.Name = "m_menuEntryRearrange";
			this.m_menuEntryRearrange.Size = new System.Drawing.Size(175, 22);
			this.m_menuEntryRearrange.Text = "&Rearrange";
			// 
			// m_menuEntryMoveToTop
			// 
			this.m_menuEntryMoveToTop.Image = global::KeePass.Properties.Resources.B16x16_2UpArrow;
			this.m_menuEntryMoveToTop.Name = "m_menuEntryMoveToTop";
			this.m_menuEntryMoveToTop.Size = new System.Drawing.Size(154, 22);
			this.m_menuEntryMoveToTop.Text = "<>";
			this.m_menuEntryMoveToTop.Click += new System.EventHandler(this.OnEntryMoveToTop);
			// 
			// m_menuEntryMoveOneUp
			// 
			this.m_menuEntryMoveOneUp.Image = global::KeePass.Properties.Resources.B16x16_1UpArrow;
			this.m_menuEntryMoveOneUp.Name = "m_menuEntryMoveOneUp";
			this.m_menuEntryMoveOneUp.Size = new System.Drawing.Size(154, 22);
			this.m_menuEntryMoveOneUp.Text = "<>";
			this.m_menuEntryMoveOneUp.Click += new System.EventHandler(this.OnEntryMoveOneUp);
			// 
			// m_menuEntryMoveOneDown
			// 
			this.m_menuEntryMoveOneDown.Image = global::KeePass.Properties.Resources.B16x16_1DownArrow;
			this.m_menuEntryMoveOneDown.Name = "m_menuEntryMoveOneDown";
			this.m_menuEntryMoveOneDown.Size = new System.Drawing.Size(154, 22);
			this.m_menuEntryMoveOneDown.Text = "<>";
			this.m_menuEntryMoveOneDown.Click += new System.EventHandler(this.OnEntryMoveOneDown);
			// 
			// m_menuEntryMoveToBottom
			// 
			this.m_menuEntryMoveToBottom.Image = global::KeePass.Properties.Resources.B16x16_2DownArrow;
			this.m_menuEntryMoveToBottom.Name = "m_menuEntryMoveToBottom";
			this.m_menuEntryMoveToBottom.Size = new System.Drawing.Size(154, 22);
			this.m_menuEntryMoveToBottom.Text = "<>";
			this.m_menuEntryMoveToBottom.Click += new System.EventHandler(this.OnEntryMoveToBottom);
			// 
			// m_menuEntryRearrangeSep0
			// 
			this.m_menuEntryRearrangeSep0.Name = "m_menuEntryRearrangeSep0";
			this.m_menuEntryRearrangeSep0.Size = new System.Drawing.Size(151, 6);
			// 
			// m_menuEntryMoveToGroup
			// 
			this.m_menuEntryMoveToGroup.Name = "m_menuEntryMoveToGroup";
			this.m_menuEntryMoveToGroup.Size = new System.Drawing.Size(154, 22);
			this.m_menuEntryMoveToGroup.Text = "Move to &Group";
			this.m_menuEntryMoveToGroup.DropDownOpening += new System.EventHandler(this.OnEntryMoveToGroupOpening);
			// 
			// m_menuEntryMoveToPreviousParent
			// 
			this.m_menuEntryMoveToPreviousParent.Image = global::KeePass.Properties.Resources.B16x16_Undo;
			this.m_menuEntryMoveToPreviousParent.Name = "m_menuEntryMoveToPreviousParent";
			this.m_menuEntryMoveToPreviousParent.Size = new System.Drawing.Size(154, 22);
			this.m_menuEntryMoveToPreviousParent.Text = "<>";
			this.m_menuEntryMoveToPreviousParent.Click += new System.EventHandler(this.OnEntryMoveToPreviousParent);
			// 
			// m_menuEntryDX
			// 
			this.m_menuEntryDX.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuEntryClipCopy,
            this.m_menuEntryClipCopyPlain,
            this.m_menuEntryClipPaste,
            this.m_menuEntryDXSep0,
            this.m_menuEntryPrint,
            this.m_menuEntryExport});
			this.m_menuEntryDX.Name = "m_menuEntryDX";
			this.m_menuEntryDX.Size = new System.Drawing.Size(175, 22);
			this.m_menuEntryDX.Text = "Data E&xchange";
			this.m_menuEntryDX.DropDownOpening += new System.EventHandler(this.OnEntryDXOpening);
			// 
			// m_menuEntryClipCopy
			// 
			this.m_menuEntryClipCopy.Image = global::KeePass.Properties.Resources.B16x16_EditCopy;
			this.m_menuEntryClipCopy.Name = "m_menuEntryClipCopy";
			this.m_menuEntryClipCopy.Size = new System.Drawing.Size(90, 22);
			this.m_menuEntryClipCopy.Text = "<>";
			this.m_menuEntryClipCopy.Click += new System.EventHandler(this.OnEntryClipCopy);
			// 
			// m_menuEntryClipCopyPlain
			// 
			this.m_menuEntryClipCopyPlain.Image = global::KeePass.Properties.Resources.B16x16_EditCopy;
			this.m_menuEntryClipCopyPlain.Name = "m_menuEntryClipCopyPlain";
			this.m_menuEntryClipCopyPlain.Size = new System.Drawing.Size(90, 22);
			this.m_menuEntryClipCopyPlain.Text = "<>";
			this.m_menuEntryClipCopyPlain.Click += new System.EventHandler(this.OnEntryClipCopyPlain);
			// 
			// m_menuEntryClipPaste
			// 
			this.m_menuEntryClipPaste.Image = global::KeePass.Properties.Resources.B16x16_EditPaste;
			this.m_menuEntryClipPaste.Name = "m_menuEntryClipPaste";
			this.m_menuEntryClipPaste.Size = new System.Drawing.Size(90, 22);
			this.m_menuEntryClipPaste.Text = "<>";
			this.m_menuEntryClipPaste.Click += new System.EventHandler(this.OnEntryClipPaste);
			// 
			// m_menuEntryDXSep0
			// 
			this.m_menuEntryDXSep0.Name = "m_menuEntryDXSep0";
			this.m_menuEntryDXSep0.Size = new System.Drawing.Size(87, 6);
			// 
			// m_menuEntryPrint
			// 
			this.m_menuEntryPrint.Image = global::KeePass.Properties.Resources.B16x16_FilePrint;
			this.m_menuEntryPrint.Name = "m_menuEntryPrint";
			this.m_menuEntryPrint.Size = new System.Drawing.Size(90, 22);
			this.m_menuEntryPrint.Text = "<>";
			this.m_menuEntryPrint.Click += new System.EventHandler(this.OnEntryPrint);
			// 
			// m_menuEntryExport
			// 
			this.m_menuEntryExport.Image = global::KeePass.Properties.Resources.B16x16_Folder_Outbox;
			this.m_menuEntryExport.Name = "m_menuEntryExport";
			this.m_menuEntryExport.Size = new System.Drawing.Size(90, 22);
			this.m_menuEntryExport.Text = "<>";
			this.m_menuEntryExport.Click += new System.EventHandler(this.OnEntryExport);
			// 
			// m_menuFind
			// 
			this.m_menuFind.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuFindInDatabase,
            this.m_menuFindInGroup,
            this.m_menuFindProfiles,
            this.m_menuFindSep0,
            this.m_menuFindTag,
            this.m_menuFindSep1,
            this.m_menuFindAll,
            this.m_menuFindParentGroup,
            this.m_menuFindSep2,
            this.m_menuFindExp,
            this.m_menuFindExpIn,
            this.m_menuFindSep3,
            this.m_menuFindLastMod,
            this.m_menuFindHistory,
            this.m_menuFindLarge,
            this.m_menuFindSep4,
            this.m_menuFindDupPasswords,
            this.m_menuFindSimPasswordsP,
            this.m_menuFindSimPasswordsC,
            this.m_menuFindPwQuality});
			this.m_menuFind.Name = "m_menuFind";
			this.m_menuFind.Size = new System.Drawing.Size(42, 20);
			this.m_menuFind.Text = "F&ind";
			// 
			// m_menuFindInDatabase
			// 
			this.m_menuFindInDatabase.Image = global::KeePass.Properties.Resources.B16x16_XMag;
			this.m_menuFindInDatabase.Name = "m_menuFindInDatabase";
			this.m_menuFindInDatabase.Size = new System.Drawing.Size(230, 22);
			this.m_menuFindInDatabase.Text = "&Find...";
			this.m_menuFindInDatabase.Click += new System.EventHandler(this.OnFindInDatabase);
			// 
			// m_menuFindInGroup
			// 
			this.m_menuFindInGroup.Image = global::KeePass.Properties.Resources.B16x16_XMag;
			this.m_menuFindInGroup.Name = "m_menuFindInGroup";
			this.m_menuFindInGroup.Size = new System.Drawing.Size(230, 22);
			this.m_menuFindInGroup.Text = "F&ind in Selected Group...";
			this.m_menuFindInGroup.Click += new System.EventHandler(this.OnFindInGroup);
			// 
			// m_menuFindProfiles
			// 
			this.m_menuFindProfiles.Name = "m_menuFindProfiles";
			this.m_menuFindProfiles.Size = new System.Drawing.Size(230, 22);
			this.m_menuFindProfiles.Text = "&Search Profiles";
			this.m_menuFindProfiles.DropDownOpening += new System.EventHandler(this.OnFindProfilesOpening);
			// 
			// m_menuFindSep0
			// 
			this.m_menuFindSep0.Name = "m_menuFindSep0";
			this.m_menuFindSep0.Size = new System.Drawing.Size(227, 6);
			// 
			// m_menuFindTag
			// 
			this.m_menuFindTag.Name = "m_menuFindTag";
			this.m_menuFindTag.Size = new System.Drawing.Size(230, 22);
			this.m_menuFindTag.Text = "&Tag";
			this.m_menuFindTag.DropDownOpening += new System.EventHandler(this.OnFindTagOpening);
			// 
			// m_menuFindSep1
			// 
			this.m_menuFindSep1.Name = "m_menuFindSep1";
			this.m_menuFindSep1.Size = new System.Drawing.Size(227, 6);
			// 
			// m_menuFindAll
			// 
			this.m_menuFindAll.Image = global::KeePass.Properties.Resources.B16x16_KGPG_Key3;
			this.m_menuFindAll.Name = "m_menuFindAll";
			this.m_menuFindAll.Size = new System.Drawing.Size(230, 22);
			this.m_menuFindAll.Text = "&All";
			this.m_menuFindAll.Click += new System.EventHandler(this.OnFindAll);
			// 
			// m_menuFindParentGroup
			// 
			this.m_menuFindParentGroup.Image = global::KeePass.Properties.Resources.B16x16_Folder_Blue_Open;
			this.m_menuFindParentGroup.Name = "m_menuFindParentGroup";
			this.m_menuFindParentGroup.Size = new System.Drawing.Size(230, 22);
			this.m_menuFindParentGroup.Text = "Selected Entry\'s &Group";
			this.m_menuFindParentGroup.Click += new System.EventHandler(this.OnFindParentGroup);
			// 
			// m_menuFindSep2
			// 
			this.m_menuFindSep2.Name = "m_menuFindSep2";
			this.m_menuFindSep2.Size = new System.Drawing.Size(227, 6);
			// 
			// m_menuFindExp
			// 
			this.m_menuFindExp.Image = global::KeePass.Properties.Resources.B16x16_History_Clear;
			this.m_menuFindExp.Name = "m_menuFindExp";
			this.m_menuFindExp.Size = new System.Drawing.Size(230, 22);
			this.m_menuFindExp.Text = "&Expired";
			this.m_menuFindExp.Click += new System.EventHandler(this.OnFindExp);
			// 
			// m_menuFindExpIn
			// 
			this.m_menuFindExpIn.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuFindExp1,
            this.m_menuFindExp2,
            this.m_menuFindExp3,
            this.m_menuFindExp7,
            this.m_menuFindExp14,
            this.m_menuFindExp30,
            this.m_menuFindExp60,
            this.m_menuFindExpInSep0,
            this.m_menuFindExpInF});
			this.m_menuFindExpIn.Name = "m_menuFindExpIn";
			this.m_menuFindExpIn.Size = new System.Drawing.Size(230, 22);
			this.m_menuFindExpIn.Text = "E&xpiring In";
			// 
			// m_menuFindExp1
			// 
			this.m_menuFindExp1.Image = global::KeePass.Properties.Resources.B16x16_History_Clear;
			this.m_menuFindExp1.Name = "m_menuFindExp1";
			this.m_menuFindExp1.Size = new System.Drawing.Size(124, 22);
			this.m_menuFindExp1.Text = "&1 Day";
			this.m_menuFindExp1.Click += new System.EventHandler(this.OnFindExp1);
			// 
			// m_menuFindExp2
			// 
			this.m_menuFindExp2.Image = global::KeePass.Properties.Resources.B16x16_History_Clear;
			this.m_menuFindExp2.Name = "m_menuFindExp2";
			this.m_menuFindExp2.Size = new System.Drawing.Size(124, 22);
			this.m_menuFindExp2.Text = "&2 Days";
			this.m_menuFindExp2.Click += new System.EventHandler(this.OnFindExp2);
			// 
			// m_menuFindExp3
			// 
			this.m_menuFindExp3.Image = global::KeePass.Properties.Resources.B16x16_History_Clear;
			this.m_menuFindExp3.Name = "m_menuFindExp3";
			this.m_menuFindExp3.Size = new System.Drawing.Size(124, 22);
			this.m_menuFindExp3.Text = "&3 Days";
			this.m_menuFindExp3.Click += new System.EventHandler(this.OnFindExp3);
			// 
			// m_menuFindExp7
			// 
			this.m_menuFindExp7.Image = global::KeePass.Properties.Resources.B16x16_History_Clear;
			this.m_menuFindExp7.Name = "m_menuFindExp7";
			this.m_menuFindExp7.Size = new System.Drawing.Size(124, 22);
			this.m_menuFindExp7.Text = "1 &Week";
			this.m_menuFindExp7.Click += new System.EventHandler(this.OnFindExp7);
			// 
			// m_menuFindExp14
			// 
			this.m_menuFindExp14.Image = global::KeePass.Properties.Resources.B16x16_History_Clear;
			this.m_menuFindExp14.Name = "m_menuFindExp14";
			this.m_menuFindExp14.Size = new System.Drawing.Size(124, 22);
			this.m_menuFindExp14.Text = "2 W&eeks";
			this.m_menuFindExp14.Click += new System.EventHandler(this.OnFindExp14);
			// 
			// m_menuFindExp30
			// 
			this.m_menuFindExp30.Image = global::KeePass.Properties.Resources.B16x16_History_Clear;
			this.m_menuFindExp30.Name = "m_menuFindExp30";
			this.m_menuFindExp30.Size = new System.Drawing.Size(124, 22);
			this.m_menuFindExp30.Text = "1 &Month";
			this.m_menuFindExp30.Click += new System.EventHandler(this.OnFindExp30);
			// 
			// m_menuFindExp60
			// 
			this.m_menuFindExp60.Image = global::KeePass.Properties.Resources.B16x16_History_Clear;
			this.m_menuFindExp60.Name = "m_menuFindExp60";
			this.m_menuFindExp60.Size = new System.Drawing.Size(124, 22);
			this.m_menuFindExp60.Text = "2 M&onths";
			this.m_menuFindExp60.Click += new System.EventHandler(this.OnFindExp60);
			// 
			// m_menuFindExpInSep0
			// 
			this.m_menuFindExpInSep0.Name = "m_menuFindExpInSep0";
			this.m_menuFindExpInSep0.Size = new System.Drawing.Size(121, 6);
			// 
			// m_menuFindExpInF
			// 
			this.m_menuFindExpInF.Image = global::KeePass.Properties.Resources.B16x16_History_Clear;
			this.m_menuFindExpInF.Name = "m_menuFindExpInF";
			this.m_menuFindExpInF.Size = new System.Drawing.Size(124, 22);
			this.m_menuFindExpInF.Text = "&Future";
			this.m_menuFindExpInF.Click += new System.EventHandler(this.OnFindExpInF);
			// 
			// m_menuFindSep3
			// 
			this.m_menuFindSep3.Name = "m_menuFindSep3";
			this.m_menuFindSep3.Size = new System.Drawing.Size(227, 6);
			// 
			// m_menuFindLastMod
			// 
			this.m_menuFindLastMod.Image = global::KeePass.Properties.Resources.B16x16_History;
			this.m_menuFindLastMod.Name = "m_menuFindLastMod";
			this.m_menuFindLastMod.Size = new System.Drawing.Size(230, 22);
			this.m_menuFindLastMod.Text = "Last &Modified Entries...";
			this.m_menuFindLastMod.Click += new System.EventHandler(this.OnFindLastMod);
			// 
			// m_menuFindLarge
			// 
			this.m_menuFindLarge.Image = global::KeePass.Properties.Resources.B16x16_HwInfo;
			this.m_menuFindLarge.Name = "m_menuFindLarge";
			this.m_menuFindLarge.Size = new System.Drawing.Size(230, 22);
			this.m_menuFindLarge.Text = "&Large Entries...";
			this.m_menuFindLarge.Click += new System.EventHandler(this.OnFindLarge);
			// 
			// m_menuFindSep4
			// 
			this.m_menuFindSep4.Name = "m_menuFindSep4";
			this.m_menuFindSep4.Size = new System.Drawing.Size(227, 6);
			// 
			// m_menuFindDupPasswords
			// 
			this.m_menuFindDupPasswords.Image = global::KeePass.Properties.Resources.B16x16_KGPG_Key2;
			this.m_menuFindDupPasswords.Name = "m_menuFindDupPasswords";
			this.m_menuFindDupPasswords.Size = new System.Drawing.Size(230, 22);
			this.m_menuFindDupPasswords.Text = "&Duplicate Passwords...";
			this.m_menuFindDupPasswords.Click += new System.EventHandler(this.OnFindDupPasswords);
			// 
			// m_menuFindSimPasswordsP
			// 
			this.m_menuFindSimPasswordsP.Image = global::KeePass.Properties.Resources.B16x16_KGPG_Key2;
			this.m_menuFindSimPasswordsP.Name = "m_menuFindSimPasswordsP";
			this.m_menuFindSimPasswordsP.Size = new System.Drawing.Size(230, 22);
			this.m_menuFindSimPasswordsP.Text = "Similar Passwords (&Pairs)...";
			this.m_menuFindSimPasswordsP.Click += new System.EventHandler(this.OnFindSimPasswordsP);
			// 
			// m_menuFindSimPasswordsC
			// 
			this.m_menuFindSimPasswordsC.Image = global::KeePass.Properties.Resources.B16x16_KGPG_Key2;
			this.m_menuFindSimPasswordsC.Name = "m_menuFindSimPasswordsC";
			this.m_menuFindSimPasswordsC.Size = new System.Drawing.Size(230, 22);
			this.m_menuFindSimPasswordsC.Text = "Similar Passwords (&Clusters)...";
			this.m_menuFindSimPasswordsC.Click += new System.EventHandler(this.OnFindSimPasswordsC);
			// 
			// m_menuFindPwQuality
			// 
			this.m_menuFindPwQuality.Image = global::KeePass.Properties.Resources.B16x16_KOrganizer;
			this.m_menuFindPwQuality.Name = "m_menuFindPwQuality";
			this.m_menuFindPwQuality.Size = new System.Drawing.Size(230, 22);
			this.m_menuFindPwQuality.Text = "Password &Quality...";
			this.m_menuFindPwQuality.Click += new System.EventHandler(this.OnFindPwQualityReport);
			// 
			// m_menuView
			// 
			this.m_menuView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuChangeLanguage,
            this.m_menuViewSep0,
            this.m_menuViewShowToolBar,
            this.m_menuViewShowEntryView,
            this.m_menuViewWindowLayout,
            this.m_menuViewSep1,
            this.m_menuViewAlwaysOnTop,
            this.m_menuViewSep2,
            this.m_menuViewConfigColumns,
            this.m_menuViewSortBy,
            this.m_menuViewTanOptions,
            this.m_menuViewEntryListGrouping,
            this.m_menuViewSep3,
            this.m_menuViewShowEntriesOfSubGroups});
			this.m_menuView.Name = "m_menuView";
			this.m_menuView.Size = new System.Drawing.Size(44, 20);
			this.m_menuView.Text = "&View";
			// 
			// m_menuChangeLanguage
			// 
			this.m_menuChangeLanguage.Image = global::KeePass.Properties.Resources.B16x16_Keyboard_Layout;
			this.m_menuChangeLanguage.Name = "m_menuChangeLanguage";
			this.m_menuChangeLanguage.Size = new System.Drawing.Size(215, 22);
			this.m_menuChangeLanguage.Text = "Change &Language...";
			this.m_menuChangeLanguage.Click += new System.EventHandler(this.OnMenuChangeLanguage);
			// 
			// m_menuViewSep0
			// 
			this.m_menuViewSep0.Name = "m_menuViewSep0";
			this.m_menuViewSep0.Size = new System.Drawing.Size(212, 6);
			// 
			// m_menuViewShowToolBar
			// 
			this.m_menuViewShowToolBar.Name = "m_menuViewShowToolBar";
			this.m_menuViewShowToolBar.Size = new System.Drawing.Size(215, 22);
			this.m_menuViewShowToolBar.Text = "Show &Toolbar";
			this.m_menuViewShowToolBar.Click += new System.EventHandler(this.OnViewShowToolBar);
			// 
			// m_menuViewShowEntryView
			// 
			this.m_menuViewShowEntryView.Name = "m_menuViewShowEntryView";
			this.m_menuViewShowEntryView.Size = new System.Drawing.Size(215, 22);
			this.m_menuViewShowEntryView.Text = "Show &Entry View";
			this.m_menuViewShowEntryView.Click += new System.EventHandler(this.OnViewShowEntryView);
			// 
			// m_menuViewWindowLayout
			// 
			this.m_menuViewWindowLayout.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuViewWindowsStacked,
            this.m_menuViewWindowsSideBySide});
			this.m_menuViewWindowLayout.Name = "m_menuViewWindowLayout";
			this.m_menuViewWindowLayout.Size = new System.Drawing.Size(215, 22);
			this.m_menuViewWindowLayout.Text = "&Window Layout";
			// 
			// m_menuViewWindowsStacked
			// 
			this.m_menuViewWindowsStacked.Image = global::KeePass.Properties.Resources.B16x16_Window_2Horz1Vert;
			this.m_menuViewWindowsStacked.Name = "m_menuViewWindowsStacked";
			this.m_menuViewWindowsStacked.Size = new System.Drawing.Size(137, 22);
			this.m_menuViewWindowsStacked.Text = "&Stacked";
			this.m_menuViewWindowsStacked.Click += new System.EventHandler(this.OnViewWindowsStacked);
			// 
			// m_menuViewWindowsSideBySide
			// 
			this.m_menuViewWindowsSideBySide.Image = global::KeePass.Properties.Resources.B16x16_Window_3Horz;
			this.m_menuViewWindowsSideBySide.Name = "m_menuViewWindowsSideBySide";
			this.m_menuViewWindowsSideBySide.Size = new System.Drawing.Size(137, 22);
			this.m_menuViewWindowsSideBySide.Text = "Side &by Side";
			this.m_menuViewWindowsSideBySide.Click += new System.EventHandler(this.OnViewWindowsSideBySide);
			// 
			// m_menuViewSep1
			// 
			this.m_menuViewSep1.Name = "m_menuViewSep1";
			this.m_menuViewSep1.Size = new System.Drawing.Size(212, 6);
			// 
			// m_menuViewAlwaysOnTop
			// 
			this.m_menuViewAlwaysOnTop.Name = "m_menuViewAlwaysOnTop";
			this.m_menuViewAlwaysOnTop.Size = new System.Drawing.Size(215, 22);
			this.m_menuViewAlwaysOnTop.Text = "&Always on Top";
			this.m_menuViewAlwaysOnTop.Click += new System.EventHandler(this.OnViewAlwaysOnTop);
			// 
			// m_menuViewSep2
			// 
			this.m_menuViewSep2.Name = "m_menuViewSep2";
			this.m_menuViewSep2.Size = new System.Drawing.Size(212, 6);
			// 
			// m_menuViewConfigColumns
			// 
			this.m_menuViewConfigColumns.Image = global::KeePass.Properties.Resources.B16x16_View_Detailed;
			this.m_menuViewConfigColumns.Name = "m_menuViewConfigColumns";
			this.m_menuViewConfigColumns.Size = new System.Drawing.Size(215, 22);
			this.m_menuViewConfigColumns.Text = "&Configure Columns...";
			this.m_menuViewConfigColumns.Click += new System.EventHandler(this.OnViewConfigColumns);
			// 
			// m_menuViewSortBy
			// 
			this.m_menuViewSortBy.Name = "m_menuViewSortBy";
			this.m_menuViewSortBy.Size = new System.Drawing.Size(215, 22);
			this.m_menuViewSortBy.Text = "&Sort By";
			// 
			// m_menuViewTanOptions
			// 
			this.m_menuViewTanOptions.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuViewTanSimpleList,
            this.m_menuViewTanIndices});
			this.m_menuViewTanOptions.Name = "m_menuViewTanOptions";
			this.m_menuViewTanOptions.Size = new System.Drawing.Size(215, 22);
			this.m_menuViewTanOptions.Text = "TAN &View Options";
			// 
			// m_menuViewTanSimpleList
			// 
			this.m_menuViewTanSimpleList.Name = "m_menuViewTanSimpleList";
			this.m_menuViewTanSimpleList.Size = new System.Drawing.Size(295, 22);
			this.m_menuViewTanSimpleList.Text = "Use &Simple List View for TAN-Only Groups";
			this.m_menuViewTanSimpleList.Click += new System.EventHandler(this.OnViewTanSimpleListClick);
			// 
			// m_menuViewTanIndices
			// 
			this.m_menuViewTanIndices.Name = "m_menuViewTanIndices";
			this.m_menuViewTanIndices.Size = new System.Drawing.Size(295, 22);
			this.m_menuViewTanIndices.Text = "Show TAN &Indices in Entry Titles";
			this.m_menuViewTanIndices.Click += new System.EventHandler(this.OnViewTanIndicesClick);
			// 
			// m_menuViewEntryListGrouping
			// 
			this.m_menuViewEntryListGrouping.Name = "m_menuViewEntryListGrouping";
			this.m_menuViewEntryListGrouping.Size = new System.Drawing.Size(215, 22);
			this.m_menuViewEntryListGrouping.Text = "&Grouping in Entry List";
			// 
			// m_menuViewSep3
			// 
			this.m_menuViewSep3.Name = "m_menuViewSep3";
			this.m_menuViewSep3.Size = new System.Drawing.Size(212, 6);
			// 
			// m_menuViewShowEntriesOfSubGroups
			// 
			this.m_menuViewShowEntriesOfSubGroups.Name = "m_menuViewShowEntriesOfSubGroups";
			this.m_menuViewShowEntriesOfSubGroups.Size = new System.Drawing.Size(215, 22);
			this.m_menuViewShowEntriesOfSubGroups.Text = "Show Entries of Su&bgroups";
			this.m_menuViewShowEntriesOfSubGroups.Click += new System.EventHandler(this.OnViewShowEntriesOfSubGroups);
			// 
			// m_menuTools
			// 
			this.m_menuTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuToolsPwGenerator,
            this.m_menuToolsGeneratePwList,
            this.m_menuToolsSep0,
            this.m_menuToolsTanWizard,
            this.m_menuToolsDb,
            this.m_menuToolsAdv,
            this.m_menuToolsSep1,
            this.m_menuToolsTriggers,
            this.m_menuToolsPlugins,
            this.m_menuToolsSep2,
            this.m_menuToolsOptions});
			this.m_menuTools.Name = "m_menuTools";
			this.m_menuTools.Size = new System.Drawing.Size(46, 20);
			this.m_menuTools.Text = "&Tools";
			// 
			// m_menuToolsPwGenerator
			// 
			this.m_menuToolsPwGenerator.Image = global::KeePass.Properties.Resources.B16x16_Key_New;
			this.m_menuToolsPwGenerator.Name = "m_menuToolsPwGenerator";
			this.m_menuToolsPwGenerator.Size = new System.Drawing.Size(204, 22);
			this.m_menuToolsPwGenerator.Text = "&Generate Password...";
			this.m_menuToolsPwGenerator.Click += new System.EventHandler(this.OnToolsPwGenerator);
			// 
			// m_menuToolsGeneratePwList
			// 
			this.m_menuToolsGeneratePwList.Image = global::KeePass.Properties.Resources.B16x16_KGPG_Gen;
			this.m_menuToolsGeneratePwList.Name = "m_menuToolsGeneratePwList";
			this.m_menuToolsGeneratePwList.Size = new System.Drawing.Size(204, 22);
			this.m_menuToolsGeneratePwList.Text = "Generate Password &List...";
			this.m_menuToolsGeneratePwList.Click += new System.EventHandler(this.OnToolsGeneratePasswordList);
			// 
			// m_menuToolsSep0
			// 
			this.m_menuToolsSep0.Name = "m_menuToolsSep0";
			this.m_menuToolsSep0.Size = new System.Drawing.Size(201, 6);
			// 
			// m_menuToolsTanWizard
			// 
			this.m_menuToolsTanWizard.Image = global::KeePass.Properties.Resources.B16x16_Wizard;
			this.m_menuToolsTanWizard.Name = "m_menuToolsTanWizard";
			this.m_menuToolsTanWizard.Size = new System.Drawing.Size(204, 22);
			this.m_menuToolsTanWizard.Text = "&TAN Wizard...";
			this.m_menuToolsTanWizard.Click += new System.EventHandler(this.OnToolsTanWizard);
			// 
			// m_menuToolsDb
			// 
			this.m_menuToolsDb.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuToolsDbMaintenance,
            this.m_menuToolsDbSep0,
            this.m_menuToolsDbDelDupEntries,
            this.m_menuToolsDbDelEmptyGroups,
            this.m_menuToolsDbDelUnusedIcons,
            this.m_menuToolsDbSep1,
            this.m_menuToolsDbXmlRep});
			this.m_menuToolsDb.Name = "m_menuToolsDb";
			this.m_menuToolsDb.Size = new System.Drawing.Size(204, 22);
			this.m_menuToolsDb.Text = "&Database Tools";
			// 
			// m_menuToolsDbMaintenance
			// 
			this.m_menuToolsDbMaintenance.Image = global::KeePass.Properties.Resources.B16x16_Package_Settings;
			this.m_menuToolsDbMaintenance.Name = "m_menuToolsDbMaintenance";
			this.m_menuToolsDbMaintenance.Size = new System.Drawing.Size(226, 22);
			this.m_menuToolsDbMaintenance.Text = "Database &Maintenance...";
			this.m_menuToolsDbMaintenance.Click += new System.EventHandler(this.OnToolsDbMaintenance);
			// 
			// m_menuToolsDbSep0
			// 
			this.m_menuToolsDbSep0.Name = "m_menuToolsDbSep0";
			this.m_menuToolsDbSep0.Size = new System.Drawing.Size(223, 6);
			// 
			// m_menuToolsDbDelDupEntries
			// 
			this.m_menuToolsDbDelDupEntries.Image = global::KeePass.Properties.Resources.B16x16_DeleteEntry;
			this.m_menuToolsDbDelDupEntries.Name = "m_menuToolsDbDelDupEntries";
			this.m_menuToolsDbDelDupEntries.Size = new System.Drawing.Size(226, 22);
			this.m_menuToolsDbDelDupEntries.Text = "Delete &Duplicate Entries";
			this.m_menuToolsDbDelDupEntries.Click += new System.EventHandler(this.OnToolsDelDupEntries);
			// 
			// m_menuToolsDbDelEmptyGroups
			// 
			this.m_menuToolsDbDelEmptyGroups.Image = global::KeePass.Properties.Resources.B16x16_Folder_Locked;
			this.m_menuToolsDbDelEmptyGroups.Name = "m_menuToolsDbDelEmptyGroups";
			this.m_menuToolsDbDelEmptyGroups.Size = new System.Drawing.Size(226, 22);
			this.m_menuToolsDbDelEmptyGroups.Text = "Delete Empty &Groups";
			this.m_menuToolsDbDelEmptyGroups.Click += new System.EventHandler(this.OnToolsDelEmptyGroups);
			// 
			// m_menuToolsDbDelUnusedIcons
			// 
			this.m_menuToolsDbDelUnusedIcons.Image = global::KeePass.Properties.Resources.B16x16_Trashcan_Full;
			this.m_menuToolsDbDelUnusedIcons.Name = "m_menuToolsDbDelUnusedIcons";
			this.m_menuToolsDbDelUnusedIcons.Size = new System.Drawing.Size(226, 22);
			this.m_menuToolsDbDelUnusedIcons.Text = "Delete Unused Custom &Icons";
			this.m_menuToolsDbDelUnusedIcons.Click += new System.EventHandler(this.OnToolsDelUnusedIcons);
			// 
			// m_menuToolsDbSep1
			// 
			this.m_menuToolsDbSep1.Name = "m_menuToolsDbSep1";
			this.m_menuToolsDbSep1.Size = new System.Drawing.Size(223, 6);
			// 
			// m_menuToolsDbXmlRep
			// 
			this.m_menuToolsDbXmlRep.Image = global::KeePass.Properties.Resources.B16x16_Binary;
			this.m_menuToolsDbXmlRep.Name = "m_menuToolsDbXmlRep";
			this.m_menuToolsDbXmlRep.Size = new System.Drawing.Size(226, 22);
			this.m_menuToolsDbXmlRep.Text = "&XML Replace...";
			this.m_menuToolsDbXmlRep.Click += new System.EventHandler(this.OnToolsXmlRep);
			// 
			// m_menuToolsAdv
			// 
			this.m_menuToolsAdv.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuToolsRecreateKeyFile});
			this.m_menuToolsAdv.Name = "m_menuToolsAdv";
			this.m_menuToolsAdv.Size = new System.Drawing.Size(204, 22);
			this.m_menuToolsAdv.Text = "&Advanced Tools";
			// 
			// m_menuToolsRecreateKeyFile
			// 
			this.m_menuToolsRecreateKeyFile.Image = global::KeePass.Properties.Resources.B16x16_KGPG_Info;
			this.m_menuToolsRecreateKeyFile.Name = "m_menuToolsRecreateKeyFile";
			this.m_menuToolsRecreateKeyFile.Size = new System.Drawing.Size(285, 22);
			this.m_menuToolsRecreateKeyFile.Text = "&Recreate Key File From Printed Backup...";
			this.m_menuToolsRecreateKeyFile.Click += new System.EventHandler(this.OnToolsRecreateKeyFile);
			// 
			// m_menuToolsSep1
			// 
			this.m_menuToolsSep1.Name = "m_menuToolsSep1";
			this.m_menuToolsSep1.Size = new System.Drawing.Size(201, 6);
			// 
			// m_menuToolsTriggers
			// 
			this.m_menuToolsTriggers.Image = global::KeePass.Properties.Resources.B16x16_Make_KDevelop;
			this.m_menuToolsTriggers.Name = "m_menuToolsTriggers";
			this.m_menuToolsTriggers.Size = new System.Drawing.Size(204, 22);
			this.m_menuToolsTriggers.Text = "T&riggers...";
			this.m_menuToolsTriggers.Click += new System.EventHandler(this.OnToolsTriggers);
			// 
			// m_menuToolsPlugins
			// 
			this.m_menuToolsPlugins.Image = global::KeePass.Properties.Resources.B16x16_BlockDevice;
			this.m_menuToolsPlugins.Name = "m_menuToolsPlugins";
			this.m_menuToolsPlugins.Size = new System.Drawing.Size(204, 22);
			this.m_menuToolsPlugins.Text = "&Plugins...";
			this.m_menuToolsPlugins.Click += new System.EventHandler(this.OnToolsPlugins);
			// 
			// m_menuToolsSep2
			// 
			this.m_menuToolsSep2.Name = "m_menuToolsSep2";
			this.m_menuToolsSep2.Size = new System.Drawing.Size(201, 6);
			// 
			// m_menuToolsOptions
			// 
			this.m_menuToolsOptions.Image = global::KeePass.Properties.Resources.B16x16_Misc;
			this.m_menuToolsOptions.Name = "m_menuToolsOptions";
			this.m_menuToolsOptions.Size = new System.Drawing.Size(204, 22);
			this.m_menuToolsOptions.Text = "&Options...";
			this.m_menuToolsOptions.Click += new System.EventHandler(this.OnToolsOptions);
			// 
			// m_menuHelp
			// 
			this.m_menuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuHelpContents,
            this.m_menuHelpSep0,
            this.m_menuHelpWebsite,
            this.m_menuHelpDonate,
            this.m_menuHelpSep1,
            this.m_menuHelpCheckForUpdates,
            this.m_menuHelpSep2,
            this.m_menuHelpAbout});
			this.m_menuHelp.Name = "m_menuHelp";
			this.m_menuHelp.Size = new System.Drawing.Size(44, 20);
			this.m_menuHelp.Text = "&Help";
			// 
			// m_menuHelpContents
			// 
			this.m_menuHelpContents.Image = global::KeePass.Properties.Resources.B16x16_Toggle_Log;
			this.m_menuHelpContents.Name = "m_menuHelpContents";
			this.m_menuHelpContents.Size = new System.Drawing.Size(171, 22);
			this.m_menuHelpContents.Text = "&Help Contents";
			this.m_menuHelpContents.Click += new System.EventHandler(this.OnHelpContents);
			// 
			// m_menuHelpSep0
			// 
			this.m_menuHelpSep0.Name = "m_menuHelpSep0";
			this.m_menuHelpSep0.Size = new System.Drawing.Size(168, 6);
			// 
			// m_menuHelpWebsite
			// 
			this.m_menuHelpWebsite.Image = global::KeePass.Properties.Resources.B16x16_Folder_Home;
			this.m_menuHelpWebsite.Name = "m_menuHelpWebsite";
			this.m_menuHelpWebsite.Size = new System.Drawing.Size(171, 22);
			this.m_menuHelpWebsite.Text = "KeePass &Website";
			this.m_menuHelpWebsite.Click += new System.EventHandler(this.OnHelpHomepage);
			// 
			// m_menuHelpDonate
			// 
			this.m_menuHelpDonate.Image = global::KeePass.Properties.Resources.B16x16_Identity;
			this.m_menuHelpDonate.Name = "m_menuHelpDonate";
			this.m_menuHelpDonate.Size = new System.Drawing.Size(171, 22);
			this.m_menuHelpDonate.Text = "&Donate...";
			this.m_menuHelpDonate.Click += new System.EventHandler(this.OnHelpDonate);
			// 
			// m_menuHelpSep1
			// 
			this.m_menuHelpSep1.Name = "m_menuHelpSep1";
			this.m_menuHelpSep1.Size = new System.Drawing.Size(168, 6);
			// 
			// m_menuHelpCheckForUpdates
			// 
			this.m_menuHelpCheckForUpdates.Image = global::KeePass.Properties.Resources.B16x16_FTP;
			this.m_menuHelpCheckForUpdates.Name = "m_menuHelpCheckForUpdates";
			this.m_menuHelpCheckForUpdates.Size = new System.Drawing.Size(171, 22);
			this.m_menuHelpCheckForUpdates.Text = "&Check for Updates";
			this.m_menuHelpCheckForUpdates.Click += new System.EventHandler(this.OnHelpCheckForUpdate);
			// 
			// m_menuHelpSep2
			// 
			this.m_menuHelpSep2.Name = "m_menuHelpSep2";
			this.m_menuHelpSep2.Size = new System.Drawing.Size(168, 6);
			// 
			// m_menuHelpAbout
			// 
			this.m_menuHelpAbout.Image = global::KeePass.Properties.Resources.B16x16_Help;
			this.m_menuHelpAbout.Name = "m_menuHelpAbout";
			this.m_menuHelpAbout.Size = new System.Drawing.Size(171, 22);
			this.m_menuHelpAbout.Text = "&About KeePass";
			this.m_menuHelpAbout.Click += new System.EventHandler(this.OnHelpAbout);
			// 
			// m_toolMain
			// 
			this.m_toolMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tbNewDatabase,
            this.m_tbOpenDatabase,
            this.m_tbSaveDatabase,
            this.m_tbSaveAll,
            this.m_tbSep0,
            this.m_tbAddEntry,
            this.m_tbSep1,
            this.m_tbCopyUserName,
            this.m_tbCopyPassword,
            this.m_tbOpenUrl,
            this.m_tbCopyUrl,
            this.m_tbAutoType,
            this.m_tbSep4,
            this.m_tbFind,
            this.m_tbEntryViewsDropDown,
            this.m_tbSep2,
            this.m_tbLockWorkspace,
            this.m_tbSep3,
            this.m_tbQuickFind,
            this.m_tbCloseTab});
			this.m_toolMain.Location = new System.Drawing.Point(0, 24);
			this.m_toolMain.Name = "m_toolMain";
			this.m_toolMain.Size = new System.Drawing.Size(654, 25);
			this.m_toolMain.TabIndex = 1;
			this.m_toolMain.TabStop = true;
			// 
			// m_tbNewDatabase
			// 
			this.m_tbNewDatabase.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tbNewDatabase.Image = global::KeePass.Properties.Resources.B16x16_FileNew;
			this.m_tbNewDatabase.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tbNewDatabase.Name = "m_tbNewDatabase";
			this.m_tbNewDatabase.Size = new System.Drawing.Size(23, 22);
			this.m_tbNewDatabase.Click += new System.EventHandler(this.OnFileNew);
			// 
			// m_tbOpenDatabase
			// 
			this.m_tbOpenDatabase.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tbOpenDatabase.Image = global::KeePass.Properties.Resources.B16x16_Folder_Yellow_Open;
			this.m_tbOpenDatabase.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tbOpenDatabase.Name = "m_tbOpenDatabase";
			this.m_tbOpenDatabase.Size = new System.Drawing.Size(23, 22);
			this.m_tbOpenDatabase.Click += new System.EventHandler(this.OnFileOpen);
			// 
			// m_tbSaveDatabase
			// 
			this.m_tbSaveDatabase.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tbSaveDatabase.Image = global::KeePass.Properties.Resources.B16x16_FileSave;
			this.m_tbSaveDatabase.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tbSaveDatabase.Name = "m_tbSaveDatabase";
			this.m_tbSaveDatabase.Size = new System.Drawing.Size(23, 22);
			this.m_tbSaveDatabase.Click += new System.EventHandler(this.OnFileSave);
			// 
			// m_tbSaveAll
			// 
			this.m_tbSaveAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tbSaveAll.Image = global::KeePass.Properties.Resources.B16x16_File_SaveAll;
			this.m_tbSaveAll.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tbSaveAll.Name = "m_tbSaveAll";
			this.m_tbSaveAll.Size = new System.Drawing.Size(23, 22);
			this.m_tbSaveAll.Click += new System.EventHandler(this.OnFileSaveAll);
			// 
			// m_tbSep0
			// 
			this.m_tbSep0.Name = "m_tbSep0";
			this.m_tbSep0.Size = new System.Drawing.Size(6, 25);
			// 
			// m_tbAddEntry
			// 
			this.m_tbAddEntry.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tbAddEntry.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tbAddEntryDefault});
			this.m_tbAddEntry.Image = global::KeePass.Properties.Resources.B16x16_KGPG_Import;
			this.m_tbAddEntry.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tbAddEntry.Name = "m_tbAddEntry";
			this.m_tbAddEntry.Size = new System.Drawing.Size(32, 22);
			this.m_tbAddEntry.ButtonClick += new System.EventHandler(this.OnEntryAdd);
			// 
			// m_tbAddEntryDefault
			// 
			this.m_tbAddEntryDefault.Image = global::KeePass.Properties.Resources.B16x16_KGPG_Import;
			this.m_tbAddEntryDefault.Name = "m_tbAddEntryDefault";
			this.m_tbAddEntryDefault.Size = new System.Drawing.Size(90, 22);
			this.m_tbAddEntryDefault.Text = "<>";
			this.m_tbAddEntryDefault.Click += new System.EventHandler(this.OnEntryAdd);
			// 
			// m_tbSep1
			// 
			this.m_tbSep1.Name = "m_tbSep1";
			this.m_tbSep1.Size = new System.Drawing.Size(6, 25);
			// 
			// m_tbCopyUserName
			// 
			this.m_tbCopyUserName.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tbCopyUserName.Image = global::KeePass.Properties.Resources.B16x16_Personal;
			this.m_tbCopyUserName.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tbCopyUserName.Name = "m_tbCopyUserName";
			this.m_tbCopyUserName.Size = new System.Drawing.Size(23, 22);
			this.m_tbCopyUserName.Click += new System.EventHandler(this.OnEntryCopyUserName);
			// 
			// m_tbCopyPassword
			// 
			this.m_tbCopyPassword.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tbCopyPassword.Image = global::KeePass.Properties.Resources.B16x16_KGPG_Info;
			this.m_tbCopyPassword.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tbCopyPassword.Name = "m_tbCopyPassword";
			this.m_tbCopyPassword.Size = new System.Drawing.Size(23, 22);
			this.m_tbCopyPassword.Click += new System.EventHandler(this.OnEntryCopyPassword);
			// 
			// m_tbOpenUrl
			// 
			this.m_tbOpenUrl.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tbOpenUrl.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tbOpenUrlDefault});
			this.m_tbOpenUrl.Image = global::KeePass.Properties.Resources.B16x16_FTP;
			this.m_tbOpenUrl.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tbOpenUrl.Name = "m_tbOpenUrl";
			this.m_tbOpenUrl.Size = new System.Drawing.Size(32, 22);
			this.m_tbOpenUrl.ButtonClick += new System.EventHandler(this.OnEntryOpenUrl);
			// 
			// m_tbOpenUrlDefault
			// 
			this.m_tbOpenUrlDefault.Image = global::KeePass.Properties.Resources.B16x16_FTP;
			this.m_tbOpenUrlDefault.Name = "m_tbOpenUrlDefault";
			this.m_tbOpenUrlDefault.Size = new System.Drawing.Size(90, 22);
			this.m_tbOpenUrlDefault.Text = "<>";
			this.m_tbOpenUrlDefault.Click += new System.EventHandler(this.OnEntryOpenUrl);
			// 
			// m_tbCopyUrl
			// 
			this.m_tbCopyUrl.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tbCopyUrl.Image = global::KeePass.Properties.Resources.B16x16_EditCopyUrl;
			this.m_tbCopyUrl.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tbCopyUrl.Name = "m_tbCopyUrl";
			this.m_tbCopyUrl.Size = new System.Drawing.Size(23, 22);
			this.m_tbCopyUrl.Click += new System.EventHandler(this.OnEntryCopyURL);
			// 
			// m_tbAutoType
			// 
			this.m_tbAutoType.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tbAutoType.Image = global::KeePass.Properties.Resources.B16x16_KTouch;
			this.m_tbAutoType.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tbAutoType.Name = "m_tbAutoType";
			this.m_tbAutoType.Size = new System.Drawing.Size(23, 22);
			this.m_tbAutoType.Click += new System.EventHandler(this.OnEntryPerformAutoType);
			// 
			// m_tbSep4
			// 
			this.m_tbSep4.Name = "m_tbSep4";
			this.m_tbSep4.Size = new System.Drawing.Size(6, 25);
			// 
			// m_tbFind
			// 
			this.m_tbFind.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tbFind.Image = global::KeePass.Properties.Resources.B16x16_XMag;
			this.m_tbFind.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tbFind.Name = "m_tbFind";
			this.m_tbFind.Size = new System.Drawing.Size(23, 22);
			this.m_tbFind.Click += new System.EventHandler(this.OnFindInDatabase);
			// 
			// m_tbEntryViewsDropDown
			// 
			this.m_tbEntryViewsDropDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tbEntryViewsDropDown.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tbViewsShowAll,
            this.m_tbViewsShowExpired});
			this.m_tbEntryViewsDropDown.Image = global::KeePass.Properties.Resources.B16x16_KGPG_Key3;
			this.m_tbEntryViewsDropDown.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tbEntryViewsDropDown.Name = "m_tbEntryViewsDropDown";
			this.m_tbEntryViewsDropDown.Size = new System.Drawing.Size(29, 22);
			this.m_tbEntryViewsDropDown.DropDownOpening += new System.EventHandler(this.OnEntryViewsByTagOpening);
			// 
			// m_tbViewsShowAll
			// 
			this.m_tbViewsShowAll.Image = global::KeePass.Properties.Resources.B16x16_KGPG_Key3;
			this.m_tbViewsShowAll.Name = "m_tbViewsShowAll";
			this.m_tbViewsShowAll.Size = new System.Drawing.Size(90, 22);
			this.m_tbViewsShowAll.Text = "<>";
			this.m_tbViewsShowAll.Click += new System.EventHandler(this.OnFindAll);
			// 
			// m_tbViewsShowExpired
			// 
			this.m_tbViewsShowExpired.Image = global::KeePass.Properties.Resources.B16x16_History_Clear;
			this.m_tbViewsShowExpired.Name = "m_tbViewsShowExpired";
			this.m_tbViewsShowExpired.Size = new System.Drawing.Size(90, 22);
			this.m_tbViewsShowExpired.Text = "<>";
			this.m_tbViewsShowExpired.Click += new System.EventHandler(this.OnFindExp);
			// 
			// m_tbSep2
			// 
			this.m_tbSep2.Name = "m_tbSep2";
			this.m_tbSep2.Size = new System.Drawing.Size(6, 25);
			// 
			// m_tbLockWorkspace
			// 
			this.m_tbLockWorkspace.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tbLockWorkspace.Image = global::KeePass.Properties.Resources.B16x16_LockWorkspace;
			this.m_tbLockWorkspace.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tbLockWorkspace.Name = "m_tbLockWorkspace";
			this.m_tbLockWorkspace.Size = new System.Drawing.Size(23, 22);
			this.m_tbLockWorkspace.Click += new System.EventHandler(this.OnFileLock);
			// 
			// m_tbSep3
			// 
			this.m_tbSep3.Name = "m_tbSep3";
			this.m_tbSep3.Size = new System.Drawing.Size(6, 25);
			// 
			// m_tbQuickFind
			// 
			this.m_tbQuickFind.Name = "m_tbQuickFind";
			this.m_tbQuickFind.Size = new System.Drawing.Size(121, 25);
			this.m_tbQuickFind.SelectedIndexChanged += new System.EventHandler(this.OnQuickFindSelectedIndexChanged);
			this.m_tbQuickFind.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnQuickFindKeyUp);
			this.m_tbQuickFind.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnQuickFindKeyDown);
			// 
			// m_tbCloseTab
			// 
			this.m_tbCloseTab.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.m_tbCloseTab.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tbCloseTab.Image = global::KeePass.Properties.Resources.B16x16_File_Close;
			this.m_tbCloseTab.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tbCloseTab.Name = "m_tbCloseTab";
			this.m_tbCloseTab.Size = new System.Drawing.Size(23, 22);
			this.m_tbCloseTab.Click += new System.EventHandler(this.OnFileClose);
			// 
			// m_statusMain
			// 
			this.m_statusMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
			this.m_statusMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_statusPartSelected,
            this.m_statusPartInfo,
            this.m_statusPartProgress,
            this.m_statusClipboard});
			this.m_statusMain.Location = new System.Drawing.Point(0, 464);
			this.m_statusMain.Name = "m_statusMain";
			this.m_statusMain.Size = new System.Drawing.Size(654, 22);
			this.m_statusMain.TabIndex = 3;
			// 
			// m_statusPartSelected
			// 
			this.m_statusPartSelected.AutoSize = false;
			this.m_statusPartSelected.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
			this.m_statusPartSelected.Name = "m_statusPartSelected";
			this.m_statusPartSelected.Size = new System.Drawing.Size(140, 17);
			this.m_statusPartSelected.Text = "0 entries";
			this.m_statusPartSelected.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// m_statusPartInfo
			// 
			this.m_statusPartInfo.Name = "m_statusPartInfo";
			this.m_statusPartInfo.Size = new System.Drawing.Size(245, 17);
			this.m_statusPartInfo.Spring = true;
			this.m_statusPartInfo.Text = "Ready.";
			this.m_statusPartInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// m_statusPartProgress
			// 
			this.m_statusPartProgress.AutoSize = false;
			this.m_statusPartProgress.Name = "m_statusPartProgress";
			this.m_statusPartProgress.Size = new System.Drawing.Size(150, 16);
			this.m_statusPartProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			// 
			// m_statusClipboard
			// 
			this.m_statusClipboard.AutoSize = false;
			this.m_statusClipboard.Name = "m_statusClipboard";
			this.m_statusClipboard.Size = new System.Drawing.Size(100, 16);
			this.m_statusClipboard.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			// 
			// m_ctxTray
			// 
			this.m_ctxTray.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_ctxTrayTray,
            this.m_ctxTraySep0,
            this.m_ctxTrayGenPw,
            this.m_ctxTraySep1,
            this.m_ctxTrayOptions,
            this.m_ctxTraySep2,
            this.m_ctxTrayCancel,
            this.m_ctxTrayLock,
            this.m_ctxTrayFileExit});
			this.m_ctxTray.Name = "m_ctxTray";
			this.m_ctxTray.Size = new System.Drawing.Size(184, 154);
			this.m_ctxTray.Opening += new System.ComponentModel.CancelEventHandler(this.OnCtxTrayOpening);
			// 
			// m_ctxTrayTray
			// 
			this.m_ctxTrayTray.Image = global::KeePass.Properties.Resources.B16x16_View_Detailed;
			this.m_ctxTrayTray.Name = "m_ctxTrayTray";
			this.m_ctxTrayTray.Size = new System.Drawing.Size(183, 22);
			this.m_ctxTrayTray.Text = "&Tray / Untray";
			this.m_ctxTrayTray.Click += new System.EventHandler(this.OnTrayTray);
			// 
			// m_ctxTraySep0
			// 
			this.m_ctxTraySep0.Name = "m_ctxTraySep0";
			this.m_ctxTraySep0.Size = new System.Drawing.Size(180, 6);
			// 
			// m_ctxTrayGenPw
			// 
			this.m_ctxTrayGenPw.Image = global::KeePass.Properties.Resources.B16x16_Key_New;
			this.m_ctxTrayGenPw.Name = "m_ctxTrayGenPw";
			this.m_ctxTrayGenPw.Size = new System.Drawing.Size(183, 22);
			this.m_ctxTrayGenPw.Text = "&Generate Password...";
			this.m_ctxTrayGenPw.Click += new System.EventHandler(this.OnTrayGenPw);
			// 
			// m_ctxTraySep1
			// 
			this.m_ctxTraySep1.Name = "m_ctxTraySep1";
			this.m_ctxTraySep1.Size = new System.Drawing.Size(180, 6);
			// 
			// m_ctxTrayOptions
			// 
			this.m_ctxTrayOptions.Image = global::KeePass.Properties.Resources.B16x16_Misc;
			this.m_ctxTrayOptions.Name = "m_ctxTrayOptions";
			this.m_ctxTrayOptions.Size = new System.Drawing.Size(183, 22);
			this.m_ctxTrayOptions.Text = "&Options...";
			this.m_ctxTrayOptions.Click += new System.EventHandler(this.OnTrayOptions);
			// 
			// m_ctxTraySep2
			// 
			this.m_ctxTraySep2.Name = "m_ctxTraySep2";
			this.m_ctxTraySep2.Size = new System.Drawing.Size(180, 6);
			// 
			// m_ctxTrayCancel
			// 
			this.m_ctxTrayCancel.Image = global::KeePass.Properties.Resources.B16x16_Error;
			this.m_ctxTrayCancel.Name = "m_ctxTrayCancel";
			this.m_ctxTrayCancel.Size = new System.Drawing.Size(183, 22);
			this.m_ctxTrayCancel.Text = "&Cancel";
			this.m_ctxTrayCancel.Click += new System.EventHandler(this.OnTrayCancel);
			// 
			// m_ctxTrayLock
			// 
			this.m_ctxTrayLock.Image = global::KeePass.Properties.Resources.B16x16_LockWorkspace;
			this.m_ctxTrayLock.Name = "m_ctxTrayLock";
			this.m_ctxTrayLock.Size = new System.Drawing.Size(183, 22);
			this.m_ctxTrayLock.Text = "<>";
			this.m_ctxTrayLock.Click += new System.EventHandler(this.OnTrayLock);
			// 
			// m_ctxTrayFileExit
			// 
			this.m_ctxTrayFileExit.Image = global::KeePass.Properties.Resources.B16x16_Exit;
			this.m_ctxTrayFileExit.Name = "m_ctxTrayFileExit";
			this.m_ctxTrayFileExit.Size = new System.Drawing.Size(183, 22);
			this.m_ctxTrayFileExit.Text = "E&xit";
			this.m_ctxTrayFileExit.Click += new System.EventHandler(this.OnTrayExit);
			// 
			// m_timerMain
			// 
			this.m_timerMain.Enabled = true;
			this.m_timerMain.Interval = 1000;
			this.m_timerMain.Tick += new System.EventHandler(this.OnTimerMainTick);
			// 
			// m_tabMain
			// 
			this.m_tabMain.Dock = System.Windows.Forms.DockStyle.Top;
			this.m_tabMain.Location = new System.Drawing.Point(0, 49);
			this.m_tabMain.Name = "m_tabMain";
			this.m_tabMain.SelectedIndex = 0;
			this.m_tabMain.ShowToolTips = true;
			this.m_tabMain.Size = new System.Drawing.Size(654, 22);
			this.m_tabMain.TabIndex = 2;
			this.m_tabMain.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnTabMainMouseClick);
			this.m_tabMain.SelectedIndexChanged += new System.EventHandler(this.OnTabMainSelectedIndexChanged);
			// 
			// m_splitHorizontal
			// 
			this.m_splitHorizontal.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_splitHorizontal.Location = new System.Drawing.Point(0, 71);
			this.m_splitHorizontal.Name = "m_splitHorizontal";
			this.m_splitHorizontal.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// m_splitHorizontal.Panel1
			// 
			this.m_splitHorizontal.Panel1.Controls.Add(this.m_splitVertical);
			// 
			// m_splitHorizontal.Panel2
			// 
			this.m_splitHorizontal.Panel2.Controls.Add(this.m_richEntryView);
			this.m_splitHorizontal.Size = new System.Drawing.Size(654, 393);
			this.m_splitHorizontal.SplitterDistance = 306;
			this.m_splitHorizontal.TabIndex = 2;
			this.m_splitHorizontal.TabStop = false;
			// 
			// m_splitVertical
			// 
			this.m_splitVertical.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_splitVertical.Location = new System.Drawing.Point(0, 0);
			this.m_splitVertical.Name = "m_splitVertical";
			// 
			// m_splitVertical.Panel1
			// 
			this.m_splitVertical.Panel1.Controls.Add(this.m_tvGroups);
			// 
			// m_splitVertical.Panel2
			// 
			this.m_splitVertical.Panel2.Controls.Add(this.m_lvEntries);
			this.m_splitVertical.Size = new System.Drawing.Size(654, 306);
			this.m_splitVertical.SplitterDistance = 177;
			this.m_splitVertical.TabIndex = 0;
			this.m_splitVertical.TabStop = false;
			// 
			// m_tvGroups
			// 
			this.m_tvGroups.AllowDrop = true;
			this.m_tvGroups.ContextMenuStrip = this.m_ctxGroupList;
			this.m_tvGroups.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_tvGroups.HideSelection = false;
			this.m_tvGroups.HotTracking = true;
			this.m_tvGroups.Location = new System.Drawing.Point(0, 0);
			this.m_tvGroups.Name = "m_tvGroups";
			this.m_tvGroups.ShowNodeToolTips = true;
			this.m_tvGroups.ShowRootLines = false;
			this.m_tvGroups.Size = new System.Drawing.Size(177, 306);
			this.m_tvGroups.TabIndex = 0;
			this.m_tvGroups.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.OnGroupsAfterCollapse);
			this.m_tvGroups.DragLeave += new System.EventHandler(this.OnGroupsDragLeave);
			this.m_tvGroups.DragDrop += new System.Windows.Forms.DragEventHandler(this.OnGroupsDragDrop);
			this.m_tvGroups.DragEnter += new System.Windows.Forms.DragEventHandler(this.OnGroupsDragEnter);
			this.m_tvGroups.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnGroupsKeyUp);
			this.m_tvGroups.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.OnGroupsNodeClick);
			this.m_tvGroups.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnGroupsKeyDown);
			this.m_tvGroups.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.OnGroupsAfterExpand);
			this.m_tvGroups.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.OnGroupsItemDrag);
			this.m_tvGroups.DragOver += new System.Windows.Forms.DragEventHandler(this.OnGroupsDragOver);
			// 
			// m_lvEntries
			// 
			this.m_lvEntries.AllowColumnReorder = true;
			this.m_lvEntries.ContextMenuStrip = this.m_ctxPwList;
			this.m_lvEntries.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_lvEntries.FullRowSelect = true;
			this.m_lvEntries.HideSelection = false;
			this.m_lvEntries.Location = new System.Drawing.Point(0, 0);
			this.m_lvEntries.Name = "m_lvEntries";
			this.m_lvEntries.ShowItemToolTips = true;
			this.m_lvEntries.Size = new System.Drawing.Size(473, 306);
			this.m_lvEntries.TabIndex = 0;
			this.m_lvEntries.UseCompatibleStateImageBehavior = false;
			this.m_lvEntries.View = System.Windows.Forms.View.Details;
			this.m_lvEntries.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.OnPwListMouseDoubleClick);
			this.m_lvEntries.ColumnWidthChanged += new System.Windows.Forms.ColumnWidthChangedEventHandler(this.OnPwListColumnWidthChanged);
			this.m_lvEntries.SelectedIndexChanged += new System.EventHandler(this.OnPwListSelectedIndexChanged);
			this.m_lvEntries.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.OnPwListColumnClick);
			this.m_lvEntries.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnPwListMouseDown);
			this.m_lvEntries.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnPwListKeyUp);
			this.m_lvEntries.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnPwListKeyDown);
			this.m_lvEntries.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.OnPwListItemDrag);
			this.m_lvEntries.Click += new System.EventHandler(this.OnPwListClick);
			// 
			// m_richEntryView
			// 
			this.m_richEntryView.DetectUrls = false;
			this.m_richEntryView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_richEntryView.Location = new System.Drawing.Point(0, 0);
			this.m_richEntryView.Name = "m_richEntryView";
			this.m_richEntryView.ReadOnly = true;
			this.m_richEntryView.Size = new System.Drawing.Size(654, 83);
			this.m_richEntryView.TabIndex = 0;
			this.m_richEntryView.Text = "";
			this.m_richEntryView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnEntryViewKeyDown);
			this.m_richEntryView.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.OnEntryViewLinkClicked);
			this.m_richEntryView.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnEntryViewKeyUp);
			// 
			// m_menuFindHistory
			// 
			this.m_menuFindHistory.Image = global::KeePass.Properties.Resources.B16x16_History;
			this.m_menuFindHistory.Name = "m_menuFindHistory";
			this.m_menuFindHistory.Size = new System.Drawing.Size(230, 22);
			this.m_menuFindHistory.Text = "&History...";
			this.m_menuFindHistory.Click += new System.EventHandler(this.OnFindHistory);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(654, 486);
			this.Controls.Add(this.m_splitHorizontal);
			this.Controls.Add(this.m_statusMain);
			this.Controls.Add(this.m_tabMain);
			this.Controls.Add(this.m_toolMain);
			this.Controls.Add(this.m_menuMain);
			this.MainMenuStrip = this.m_menuMain;
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "<>";
			this.Deactivate += new System.EventHandler(this.OnFormDeactivate);
			this.Load += new System.EventHandler(this.OnFormLoad);
			this.Shown += new System.EventHandler(this.OnFormShown);
			this.Activated += new System.EventHandler(this.OnFormActivated);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
			this.Resize += new System.EventHandler(this.OnFormResize);
			this.m_ctxGroupList.ResumeLayout(false);
			this.m_ctxPwList.ResumeLayout(false);
			this.m_menuMain.ResumeLayout(false);
			this.m_menuMain.PerformLayout();
			this.m_toolMain.ResumeLayout(false);
			this.m_toolMain.PerformLayout();
			this.m_statusMain.ResumeLayout(false);
			this.m_statusMain.PerformLayout();
			this.m_ctxTray.ResumeLayout(false);
			this.m_splitHorizontal.Panel1.ResumeLayout(false);
			this.m_splitHorizontal.Panel2.ResumeLayout(false);
			this.m_splitHorizontal.ResumeLayout(false);
			this.m_splitVertical.Panel1.ResumeLayout(false);
			this.m_splitVertical.Panel2.ResumeLayout(false);
			this.m_splitVertical.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private KeePass.UI.CustomMenuStripEx m_menuMain;
		private System.Windows.Forms.ToolStripMenuItem m_menuFile;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileNew;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileClose;
		private System.Windows.Forms.ToolStripSeparator m_menuFileSep0;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileSave;
		private System.Windows.Forms.ToolStripSeparator m_menuFileSep1;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileDbSettings;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileChangeMasterKey;
		private System.Windows.Forms.ToolStripSeparator m_menuFileSep2;
		private System.Windows.Forms.ToolStripMenuItem m_menuFilePrint;
		private System.Windows.Forms.ToolStripSeparator m_menuFileSep3;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileExport;
		private System.Windows.Forms.ToolStripSeparator m_menuFileSep4;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileLock;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileExit;
		private System.Windows.Forms.ToolStripMenuItem m_menuView;
		private System.Windows.Forms.ToolStripMenuItem m_menuTools;
		private System.Windows.Forms.ToolStripMenuItem m_menuHelp;
		private System.Windows.Forms.ToolStripMenuItem m_menuHelpWebsite;
		private System.Windows.Forms.ToolStripMenuItem m_menuHelpDonate;
		private System.Windows.Forms.ToolStripSeparator m_menuHelpSep0;
		private System.Windows.Forms.ToolStripMenuItem m_menuHelpContents;
		private System.Windows.Forms.ToolStripSeparator m_menuHelpSep1;
		private System.Windows.Forms.ToolStripMenuItem m_menuHelpCheckForUpdates;
		private System.Windows.Forms.ToolStripSeparator m_menuHelpSep2;
		private System.Windows.Forms.ToolStripMenuItem m_menuHelpAbout;
		private KeePass.UI.CustomToolStripEx m_toolMain;
		private System.Windows.Forms.ToolStripButton m_tbNewDatabase;
		private System.Windows.Forms.ToolStripButton m_tbOpenDatabase;
		private KeePass.UI.CustomRichTextBoxEx m_richEntryView;
		private KeePass.UI.CustomSplitContainerEx m_splitHorizontal;
		private KeePass.UI.CustomSplitContainerEx m_splitVertical;
		private KeePass.UI.CustomTreeViewEx m_tvGroups;
		private System.Windows.Forms.StatusStrip m_statusMain;
		private System.Windows.Forms.ToolStripStatusLabel m_statusPartSelected;
		private System.Windows.Forms.ToolStripStatusLabel m_statusPartInfo;
		private KeePass.UI.CustomListViewEx m_lvEntries;
		private System.Windows.Forms.ToolStripButton m_tbSaveDatabase;
		private KeePass.UI.CustomContextMenuStripEx m_ctxPwList;
		private System.Windows.Forms.ToolStripSeparator m_ctxEntrySep0;
		private System.Windows.Forms.ToolStripSeparator m_ctxEntrySep1;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryEditQuick;
		private System.Windows.Forms.ToolStripSeparator m_ctxEntrySep2;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryRearrange;
		private System.Windows.Forms.ToolStripMenuItem m_menuChangeLanguage;
		private System.Windows.Forms.ToolStripSeparator m_tbSep0;
		private System.Windows.Forms.ToolStripSeparator m_menuViewSep0;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewShowToolBar;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewShowEntryView;
		private System.Windows.Forms.ToolStripSeparator m_tbSep1;
		private System.Windows.Forms.ToolStripButton m_tbFind;
		private System.Windows.Forms.ToolStripSeparator m_tbSep2;
		private System.Windows.Forms.ToolStripButton m_tbLockWorkspace;
		private System.Windows.Forms.ToolStripSeparator m_tbSep3;
		private System.Windows.Forms.ToolStripComboBox m_tbQuickFind;
		private System.Windows.Forms.ToolStripMenuItem m_menuToolsOptions;
		private System.Windows.Forms.ToolStripSeparator m_menuViewSep1;
		private KeePass.UI.CustomContextMenuStripEx m_ctxGroupList;
		private System.Windows.Forms.ToolStripProgressBar m_statusPartProgress;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewAlwaysOnTop;
		private System.Windows.Forms.ToolStripSeparator m_menuViewSep2;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewConfigColumns;
		private System.Windows.Forms.ToolStripSeparator m_menuViewSep3;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileRecent;
		private KeePass.UI.CustomContextMenuStripEx m_ctxTray;
		private System.Windows.Forms.ToolStripMenuItem m_ctxTrayTray;
		private System.Windows.Forms.Timer m_timerMain;
		private System.Windows.Forms.ToolStripMenuItem m_menuToolsPlugins;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryUrl;
		private System.Windows.Forms.ToolStripMenuItem m_ctxGroupFind;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewTanOptions;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewTanSimpleList;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewTanIndices;
		private System.Windows.Forms.ToolStripMenuItem m_menuToolsPwGenerator;
		private System.Windows.Forms.ToolStripSeparator m_menuToolsSep0;
		private System.Windows.Forms.ToolStripMenuItem m_menuToolsTanWizard;
		private System.Windows.Forms.ToolStripProgressBar m_statusClipboard;
		private System.Windows.Forms.ToolStripDropDownButton m_tbEntryViewsDropDown;
		private System.Windows.Forms.ToolStripMenuItem m_tbViewsShowAll;
		private System.Windows.Forms.ToolStripMenuItem m_tbViewsShowExpired;
		private System.Windows.Forms.ToolStripSplitButton m_tbAddEntry;
		private System.Windows.Forms.ToolStripMenuItem m_tbAddEntryDefault;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryOtherData;
		private System.Windows.Forms.ToolStripMenuItem m_menuToolsGeneratePwList;
		private System.Windows.Forms.ToolStripSeparator m_menuToolsSep1;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewWindowLayout;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewWindowsStacked;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewWindowsSideBySide;
		private System.Windows.Forms.ToolStripButton m_tbCopyUserName;
		private System.Windows.Forms.ToolStripButton m_tbCopyPassword;
		private System.Windows.Forms.ToolStripSeparator m_tbSep4;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileOpen;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileOpenLocal;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileOpenUrl;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileSaveAs;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileSaveAsLocal;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileSaveAsUrl;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileImport;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryAttachments;
		private System.Windows.Forms.ToolStripSeparator m_ctxTraySep0;
		private System.Windows.Forms.ToolStripMenuItem m_ctxTrayFileExit;
		private System.Windows.Forms.TabControl m_tabMain;
		private System.Windows.Forms.ToolStripButton m_tbSaveAll;
		private System.Windows.Forms.ToolStripButton m_tbCloseTab;
		private System.Windows.Forms.ToolStripSeparator m_menuFileSaveAsSep0;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileSaveAsCopy;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewShowEntriesOfSubGroups;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileSync;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileSyncFile;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileSyncUrl;
		private System.Windows.Forms.ToolStripMenuItem m_ctxTrayLock;
		private System.Windows.Forms.ToolStripSeparator m_ctxTraySep1;
		private System.Windows.Forms.ToolStripSeparator m_menuFileSyncSep0;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileSyncRecent;
		private System.Windows.Forms.ToolStripSeparator m_menuToolsSep2;
		private System.Windows.Forms.ToolStripMenuItem m_menuToolsTriggers;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryTagAdd;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryTagRemove;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewSortBy;
		private System.Windows.Forms.ToolStripMenuItem m_menuToolsDb;
		private System.Windows.Forms.ToolStripMenuItem m_menuToolsDbMaintenance;
		private System.Windows.Forms.ToolStripMenuItem m_menuToolsDbDelDupEntries;
		private System.Windows.Forms.ToolStripSeparator m_menuToolsDbSep0;
		private System.Windows.Forms.ToolStripMenuItem m_menuToolsDbDelEmptyGroups;
		private System.Windows.Forms.ToolStripMenuItem m_menuToolsDbDelUnusedIcons;
		private System.Windows.Forms.ToolStripMenuItem m_ctxTrayOptions;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewEntryListGrouping;
		private System.Windows.Forms.ToolStripSeparator m_ctxTraySep2;
		private System.Windows.Forms.ToolStripMenuItem m_ctxTrayGenPw;
		private System.Windows.Forms.ToolStripSplitButton m_tbOpenUrl;
		private System.Windows.Forms.ToolStripMenuItem m_tbOpenUrlDefault;
		private System.Windows.Forms.ToolStripButton m_tbCopyUrl;
		private System.Windows.Forms.ToolStripButton m_tbAutoType;
		private System.Windows.Forms.ToolStripSeparator m_menuToolsDbSep1;
		private System.Windows.Forms.ToolStripMenuItem m_menuToolsDbXmlRep;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileRecentDummy;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileSyncRecentDummy;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryAutoTypeAdv;
		private System.Windows.Forms.ToolStripMenuItem m_ctxTrayCancel;
		private System.Windows.Forms.ToolStripMenuItem m_menuFind;
		private System.Windows.Forms.ToolStripMenuItem m_menuFindInDatabase;
		private System.Windows.Forms.ToolStripSeparator m_menuFindSep0;
		private System.Windows.Forms.ToolStripMenuItem m_menuFindAll;
		private System.Windows.Forms.ToolStripSeparator m_menuFindSep2;
		private System.Windows.Forms.ToolStripMenuItem m_menuFindExp;
		private System.Windows.Forms.ToolStripSeparator m_menuFindSep3;
		private System.Windows.Forms.ToolStripMenuItem m_menuFindDupPasswords;
		private System.Windows.Forms.ToolStripMenuItem m_menuFindSimPasswordsP;
		private System.Windows.Forms.ToolStripMenuItem m_menuFindSimPasswordsC;
		private System.Windows.Forms.ToolStripMenuItem m_menuFindPwQuality;
		private System.Windows.Forms.ToolStripSeparator m_menuFindSep4;
		private System.Windows.Forms.ToolStripMenuItem m_menuFindLarge;
		private System.Windows.Forms.ToolStripMenuItem m_menuFindLastMod;
		private System.Windows.Forms.ToolStripMenuItem m_menuFindTag;
		private System.Windows.Forms.ToolStripSeparator m_menuFindSep1;
		private System.Windows.Forms.ToolStripMenuItem m_menuFindInGroup;
		private System.Windows.Forms.ToolStripMenuItem m_menuGroup;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntry;
		private System.Windows.Forms.ToolStripMenuItem m_menuFindParentGroup;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntrySelectAll;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryDelete;
		private System.Windows.Forms.ToolStripMenuItem m_menuGroupAdd;
		private System.Windows.Forms.ToolStripMenuItem m_menuGroupEdit;
		private System.Windows.Forms.ToolStripMenuItem m_menuGroupDuplicate;
		private System.Windows.Forms.ToolStripMenuItem m_menuGroupDelete;
		private System.Windows.Forms.ToolStripMenuItem m_menuGroupEmptyRB;
		private System.Windows.Forms.ToolStripSeparator m_menuGroupSep0;
		private System.Windows.Forms.ToolStripMenuItem m_menuGroupRearrange;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryAdd;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryEdit;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryDuplicate;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryCopyUserName;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryCopyPassword;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryUrl;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryOpenUrl;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryCopyUrl;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryOtherData;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryAttachments;
		private System.Windows.Forms.ToolStripSeparator m_menuEntrySep0;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryPerformAutoType;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryAutoTypeAdv;
		private System.Windows.Forms.ToolStripSeparator m_menuEntrySep1;
		private System.Windows.Forms.ToolStripSeparator m_menuEntrySep2;
		private System.Windows.Forms.ToolStripSeparator m_menuEntrySep3;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryEditQuick;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryColor;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryColorStandard;
		private System.Windows.Forms.ToolStripSeparator m_menuEntryColorSep0;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryColorLightRed;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryColorLightGreen;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryColorLightBlue;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryColorLightYellow;
		private System.Windows.Forms.ToolStripSeparator m_menuEntryColorSep1;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryColorCustom;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryDX;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryIcon;
		private System.Windows.Forms.ToolStripSeparator m_menuEntryEditQuickSep0;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryTagAdd;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryTagRemove;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryTagNew;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryPrint;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryExport;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryRearrange;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryMoveToGroup;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryMoveToGroup;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryClipCopy;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryClipPaste;
		private System.Windows.Forms.ToolStripSeparator m_menuEntryDXSep0;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryMoveToTop;
		private System.Windows.Forms.ToolStripSeparator m_menuEntryRearrangeSep0;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryMoveOneUp;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryMoveOneDown;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryMoveToBottom;
		private System.Windows.Forms.ToolStripMenuItem m_menuFindExpIn;
		private System.Windows.Forms.ToolStripMenuItem m_menuFindExp1;
		private System.Windows.Forms.ToolStripMenuItem m_menuFindExp2;
		private System.Windows.Forms.ToolStripMenuItem m_menuFindExp3;
		private System.Windows.Forms.ToolStripMenuItem m_menuFindExp7;
		private System.Windows.Forms.ToolStripMenuItem m_menuFindExp14;
		private System.Windows.Forms.ToolStripMenuItem m_menuFindExp30;
		private System.Windows.Forms.ToolStripMenuItem m_menuFindExp60;
		private System.Windows.Forms.ToolStripSeparator m_menuFindExpInSep0;
		private System.Windows.Forms.ToolStripMenuItem m_menuFindExpInF;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryClipCopyPlain;
		private System.Windows.Forms.ToolStripMenuItem m_menuFilePrintDatabase;
		private System.Windows.Forms.ToolStripMenuItem m_menuFilePrintEmSheet;
		private System.Windows.Forms.ToolStripSeparator m_menuEntryEditQuickSep1;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryExpiresNow;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryExpiresNever;
		private System.Windows.Forms.ToolStripMenuItem m_menuGroupMoveToTop;
		private System.Windows.Forms.ToolStripMenuItem m_menuGroupMoveOneUp;
		private System.Windows.Forms.ToolStripMenuItem m_menuGroupMoveOneDown;
		private System.Windows.Forms.ToolStripMenuItem m_menuGroupMoveToBottom;
		private System.Windows.Forms.ToolStripSeparator m_menuGroupMoveSep0;
		private System.Windows.Forms.ToolStripMenuItem m_menuGroupSort;
		private System.Windows.Forms.ToolStripMenuItem m_menuGroupSortRec;
		private System.Windows.Forms.ToolStripSeparator m_menuGroupMoveSep1;
		private System.Windows.Forms.ToolStripMenuItem m_menuGroupExpand;
		private System.Windows.Forms.ToolStripMenuItem m_menuGroupCollapse;
		private System.Windows.Forms.ToolStripSeparator m_menuFileOpenSep0;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileFind;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileFindInFolder;
		private System.Windows.Forms.ToolStripMenuItem m_menuFindProfiles;
		private System.Windows.Forms.ToolStripMenuItem m_ctxGroupFindProfiles;
		private System.Windows.Forms.ToolStripMenuItem m_menuGroupDX;
		private System.Windows.Forms.ToolStripMenuItem m_menuGroupPrint;
		private System.Windows.Forms.ToolStripMenuItem m_menuGroupExport;
		private System.Windows.Forms.ToolStripMenuItem m_menuGroupClipCopy;
		private System.Windows.Forms.ToolStripSeparator m_menuGroupDXSep0;
		private System.Windows.Forms.ToolStripMenuItem m_menuGroupClipCopyPlain;
		private System.Windows.Forms.ToolStripMenuItem m_menuGroupClipPaste;
		private System.Windows.Forms.ToolStripSeparator m_menuFilePrintSep0;
		private System.Windows.Forms.ToolStripMenuItem m_menuFilePrintKeyFile;
		private System.Windows.Forms.ToolStripMenuItem m_menuToolsAdv;
		private System.Windows.Forms.ToolStripMenuItem m_menuToolsRecreateKeyFile;
		private System.Windows.Forms.ToolStripSeparator m_menuGroupMoveSep2;
		private System.Windows.Forms.ToolStripMenuItem m_menuGroupMoveToPreviousParent;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryMoveToPreviousParent;
		private System.Windows.Forms.ToolStripSeparator m_menuEntryEditQuickSep2;
		private System.Windows.Forms.ToolStripMenuItem m_menuEntryOtpGenSettings;
		private System.Windows.Forms.ToolStripMenuItem m_menuFindHistory;
	}
}

