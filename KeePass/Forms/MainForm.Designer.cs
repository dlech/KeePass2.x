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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.m_ctxGroupList = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.m_ctxGroupAdd = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxGroupSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_ctxGroupEdit = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxGroupDelete = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxGroupSep1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_ctxGroupFind = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxGroupSep2 = new System.Windows.Forms.ToolStripSeparator();
			this.m_ctxGroupPrint = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ilClientIcons = new System.Windows.Forms.ImageList(this.components);
			this.m_ctxPwList = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.m_ctxEntryCopyUserName = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntryCopyPassword = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntryUrl = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntryOpenUrl = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntryCopyUrl = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntryCopyCustomString = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntrySaveAttachedFiles = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntrySep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_ctxEntryPerformAutoType = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntrySep1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_ctxEntryAdd = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntryEdit = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntryDuplicate = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntryDelete = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntryMassModify = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntrySetColor = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntryColorStandard = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntryColorSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_ctxEntryColorLightRed = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntryColorLightGreen = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntryColorLightBlue = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntryColorLightYellow = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntryColorSep1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_ctxEntryColorCustom = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntryMMSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_ctxEntryMassSetIcon = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntrySelectAll = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntrySep2 = new System.Windows.Forms.ToolStripSeparator();
			this.m_ctxEntryClipboard = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntryClipCopy = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntryClipPaste = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntryRearrangePopup = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntryMoveToTop = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntryMoveOneUp = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntryMoveOneDown = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntryMoveToBottom = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntryRearrangeSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_ctxEntrySortUnsorted = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntryRearrangeSep1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_ctxEntrySortListByTitle = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntrySortListByUserName = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntrySortListByPassword = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntrySortListByURL = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntrySortListByNotes = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntrySortListByCreationTime = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntrySortListByLastModTime = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntrySortListByLastAccessTime = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxEntrySortListByExpirationTime = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuMain = new System.Windows.Forms.MenuStrip();
			this.m_menuFile = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileNew = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileOpen = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileOpenLocal = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileOpenUrl = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileRecent = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileClose = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuFileSave = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileSaveAs = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileSaveAsLocal = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileSaveAsUrl = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileSep1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuFileDbSettings = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileChangeMasterKey = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileSep2 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuFilePrint = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileSep3 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuFileImport = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileExport = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileExportUseXsl = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileExportSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuFileExportXML = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileExportHtml = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileExportSep1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuFileExportKdb3 = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileSynchronize = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileSep4 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuFileLock = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuFileExit = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEdit = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEditShowAllEntries = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEditShowExpired = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuEditSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuEditFind = new System.Windows.Forms.ToolStripMenuItem();
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
			this.m_menuViewHiding = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewHideTitles = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewHideUserNames = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewHidePasswords = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewHideURLs = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewHideNotes = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewSep3 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuViewTanOptions = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewTanSimpleList = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewTanIndices = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewColumns = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewColumnsShowTitle = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewColumnsShowUserName = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewColumnsShowPassword = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewColumnsShowUrl = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewColumnsShowNotes = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewColumnsShowSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuViewColumnsShowCreation = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewColumnsShowLastAccess = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewColumnsShowLastMod = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewColumnsShowExpire = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewColumnsShowSep1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuViewColumnsShowUuid = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuViewColumnsShowAttachs = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuTools = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuToolsPwGenerator = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuToolsGeneratePwList = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuToolsSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuToolsTanWizard = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuToolsDbMaintenance = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuToolsSep1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuToolsOptions = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuToolsPlugins = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuHelp = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuHelpHomepage = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuHelpDonate = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuHelpSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuHelpContents = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuHelpSelectSource = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuHelpSep1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuHelpCheckForUpdate = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuHelpSep2 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuHelpAbout = new System.Windows.Forms.ToolStripMenuItem();
			this.m_toolMain = new System.Windows.Forms.ToolStrip();
			this.m_tbNewDatabase = new System.Windows.Forms.ToolStripButton();
			this.m_tbOpenDatabase = new System.Windows.Forms.ToolStripButton();
			this.m_tbSaveDatabase = new System.Windows.Forms.ToolStripButton();
			this.m_tbSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_tbAddEntry = new System.Windows.Forms.ToolStripSplitButton();
			this.m_tbAddEntryDefault = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tbSep1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_tbCopyUserName = new System.Windows.Forms.ToolStripButton();
			this.m_tbCopyPassword = new System.Windows.Forms.ToolStripButton();
			this.m_tbSep4 = new System.Windows.Forms.ToolStripSeparator();
			this.m_tbFind = new System.Windows.Forms.ToolStripButton();
			this.m_tbEntryViewsDropDown = new System.Windows.Forms.ToolStripDropDownButton();
			this.m_tbViewsShowAll = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tbViewsShowExpired = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tbSep2 = new System.Windows.Forms.ToolStripSeparator();
			this.m_tbLockWorkspace = new System.Windows.Forms.ToolStripButton();
			this.m_tbSep3 = new System.Windows.Forms.ToolStripSeparator();
			this.m_tbQuickFind = new System.Windows.Forms.ToolStripComboBox();
			this.m_statusMain = new System.Windows.Forms.StatusStrip();
			this.m_statusPartSelected = new System.Windows.Forms.ToolStripStatusLabel();
			this.m_statusPartInfo = new System.Windows.Forms.ToolStripStatusLabel();
			this.m_statusPartProgress = new System.Windows.Forms.ToolStripProgressBar();
			this.m_statusClipboard = new System.Windows.Forms.ToolStripProgressBar();
			this.m_openDatabaseFile = new System.Windows.Forms.OpenFileDialog();
			this.m_saveDatabaseFile = new System.Windows.Forms.SaveFileDialog();
			this.m_saveExportTo = new System.Windows.Forms.SaveFileDialog();
			this.m_openImportFile = new System.Windows.Forms.OpenFileDialog();
			this.m_ntfTray = new System.Windows.Forms.NotifyIcon(this.components);
			this.m_ctxTray = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.m_ctxTrayTray = new System.Windows.Forms.ToolStripMenuItem();
			this.m_timerMain = new System.Windows.Forms.Timer(this.components);
			this.m_folderSaveAttachments = new System.Windows.Forms.FolderBrowserDialog();
			this.m_openXslFile = new System.Windows.Forms.OpenFileDialog();
			this.m_colorDlg = new System.Windows.Forms.ColorDialog();
			this.m_splitHorizontal = new KeePass.UI.CustomSplitContainerEx();
			this.m_splitVertical = new KeePass.UI.CustomSplitContainerEx();
			this.m_tvGroups = new System.Windows.Forms.TreeView();
			this.m_lvEntries = new System.Windows.Forms.ListView();
			this.m_richEntryView = new System.Windows.Forms.RichTextBox();
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
            this.m_ctxGroupAdd,
            this.m_ctxGroupSep0,
            this.m_ctxGroupEdit,
            this.m_ctxGroupDelete,
            this.m_ctxGroupSep1,
            this.m_ctxGroupFind,
            this.m_ctxGroupSep2,
            this.m_ctxGroupPrint});
			this.m_ctxGroupList.Name = "m_ctxGroupList";
			resources.ApplyResources(this.m_ctxGroupList, "m_ctxGroupList");
			// 
			// m_ctxGroupAdd
			// 
			this.m_ctxGroupAdd.Image = global::KeePass.Properties.Resources.B16x16_Folder_Sent_Mail;
			this.m_ctxGroupAdd.Name = "m_ctxGroupAdd";
			resources.ApplyResources(this.m_ctxGroupAdd, "m_ctxGroupAdd");
			this.m_ctxGroupAdd.Click += new System.EventHandler(this.OnGroupsAdd);
			// 
			// m_ctxGroupSep0
			// 
			this.m_ctxGroupSep0.Name = "m_ctxGroupSep0";
			resources.ApplyResources(this.m_ctxGroupSep0, "m_ctxGroupSep0");
			// 
			// m_ctxGroupEdit
			// 
			this.m_ctxGroupEdit.Image = global::KeePass.Properties.Resources.B16x16_Folder_Txt;
			this.m_ctxGroupEdit.Name = "m_ctxGroupEdit";
			resources.ApplyResources(this.m_ctxGroupEdit, "m_ctxGroupEdit");
			this.m_ctxGroupEdit.Click += new System.EventHandler(this.OnGroupsEdit);
			// 
			// m_ctxGroupDelete
			// 
			this.m_ctxGroupDelete.Image = global::KeePass.Properties.Resources.B16x16_Folder_Locked;
			this.m_ctxGroupDelete.Name = "m_ctxGroupDelete";
			resources.ApplyResources(this.m_ctxGroupDelete, "m_ctxGroupDelete");
			this.m_ctxGroupDelete.Click += new System.EventHandler(this.OnGroupsDelete);
			// 
			// m_ctxGroupSep1
			// 
			this.m_ctxGroupSep1.Name = "m_ctxGroupSep1";
			resources.ApplyResources(this.m_ctxGroupSep1, "m_ctxGroupSep1");
			// 
			// m_ctxGroupFind
			// 
			this.m_ctxGroupFind.Image = global::KeePass.Properties.Resources.B16x16_XMag;
			this.m_ctxGroupFind.Name = "m_ctxGroupFind";
			resources.ApplyResources(this.m_ctxGroupFind, "m_ctxGroupFind");
			this.m_ctxGroupFind.Click += new System.EventHandler(this.OnGroupsFind);
			// 
			// m_ctxGroupSep2
			// 
			this.m_ctxGroupSep2.Name = "m_ctxGroupSep2";
			resources.ApplyResources(this.m_ctxGroupSep2, "m_ctxGroupSep2");
			// 
			// m_ctxGroupPrint
			// 
			this.m_ctxGroupPrint.Image = global::KeePass.Properties.Resources.B16x16_FilePrint;
			this.m_ctxGroupPrint.Name = "m_ctxGroupPrint";
			resources.ApplyResources(this.m_ctxGroupPrint, "m_ctxGroupPrint");
			this.m_ctxGroupPrint.Click += new System.EventHandler(this.OnGroupsPrint);
			// 
			// m_ilClientIcons
			// 
			this.m_ilClientIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("m_ilClientIcons.ImageStream")));
			this.m_ilClientIcons.TransparentColor = System.Drawing.Color.Transparent;
			this.m_ilClientIcons.Images.SetKeyName(0, "C00_Password.png");
			this.m_ilClientIcons.Images.SetKeyName(1, "C01_Package_Network.png");
			this.m_ilClientIcons.Images.SetKeyName(2, "C02_MessageBox_Warning.png");
			this.m_ilClientIcons.Images.SetKeyName(3, "C03_Server.png");
			this.m_ilClientIcons.Images.SetKeyName(4, "C04_Klipper.png");
			this.m_ilClientIcons.Images.SetKeyName(5, "C05_Edu_Languages.png");
			this.m_ilClientIcons.Images.SetKeyName(6, "C06_KCMDF.png");
			this.m_ilClientIcons.Images.SetKeyName(7, "C07_Kate.png");
			this.m_ilClientIcons.Images.SetKeyName(8, "C08_Socket.png");
			this.m_ilClientIcons.Images.SetKeyName(9, "C09_Identity.png");
			this.m_ilClientIcons.Images.SetKeyName(10, "C10_Kontact.png");
			this.m_ilClientIcons.Images.SetKeyName(11, "C11_Camera.png");
			this.m_ilClientIcons.Images.SetKeyName(12, "C12_IRKickFlash.png");
			this.m_ilClientIcons.Images.SetKeyName(13, "C13_KGPG_Key3.png");
			this.m_ilClientIcons.Images.SetKeyName(14, "C14_Laptop_Power.png");
			this.m_ilClientIcons.Images.SetKeyName(15, "C15_Scanner.png");
			this.m_ilClientIcons.Images.SetKeyName(16, "C16_Mozilla_Firebird.png");
			this.m_ilClientIcons.Images.SetKeyName(17, "C17_CDROM_Unmount.png");
			this.m_ilClientIcons.Images.SetKeyName(18, "C18_Display.png");
			this.m_ilClientIcons.Images.SetKeyName(19, "C19_Mail_Generic.png");
			this.m_ilClientIcons.Images.SetKeyName(20, "C20_Misc.png");
			this.m_ilClientIcons.Images.SetKeyName(21, "C21_KOrganizer.png");
			this.m_ilClientIcons.Images.SetKeyName(22, "C22_ASCII.png");
			this.m_ilClientIcons.Images.SetKeyName(23, "C23_Icons.png");
			this.m_ilClientIcons.Images.SetKeyName(24, "C24_Connect_Established.png");
			this.m_ilClientIcons.Images.SetKeyName(25, "C25_Folder_Mail.png");
			this.m_ilClientIcons.Images.SetKeyName(26, "C26_FileSave.png");
			this.m_ilClientIcons.Images.SetKeyName(27, "C27_NFS_Unmount.png");
			this.m_ilClientIcons.Images.SetKeyName(28, "C28_QuickTime.png");
			this.m_ilClientIcons.Images.SetKeyName(29, "C29_KGPG_Term.png");
			this.m_ilClientIcons.Images.SetKeyName(30, "C30_Konsole.png");
			this.m_ilClientIcons.Images.SetKeyName(31, "C31_FilePrint.png");
			this.m_ilClientIcons.Images.SetKeyName(32, "C32_FSView.png");
			this.m_ilClientIcons.Images.SetKeyName(33, "C33_Run.png");
			this.m_ilClientIcons.Images.SetKeyName(34, "C34_Configure.png");
			this.m_ilClientIcons.Images.SetKeyName(35, "C35_KRFB.png");
			this.m_ilClientIcons.Images.SetKeyName(36, "C36_Ark.png");
			this.m_ilClientIcons.Images.SetKeyName(37, "C37_KPercentage.png");
			this.m_ilClientIcons.Images.SetKeyName(38, "C38_Samba_Unmount.png");
			this.m_ilClientIcons.Images.SetKeyName(39, "C39_History.png");
			this.m_ilClientIcons.Images.SetKeyName(40, "C40_Mail_Find.png");
			this.m_ilClientIcons.Images.SetKeyName(41, "C41_VectorGfx.png");
			this.m_ilClientIcons.Images.SetKeyName(42, "C42_KCMMemory.png");
			this.m_ilClientIcons.Images.SetKeyName(43, "C43_EditTrash.png");
			this.m_ilClientIcons.Images.SetKeyName(44, "C44_KNotes.png");
			this.m_ilClientIcons.Images.SetKeyName(45, "C45_Cancel.png");
			this.m_ilClientIcons.Images.SetKeyName(46, "C46_Help.png");
			this.m_ilClientIcons.Images.SetKeyName(47, "C47_KPackage.png");
			this.m_ilClientIcons.Images.SetKeyName(48, "C48_Folder.png");
			this.m_ilClientIcons.Images.SetKeyName(49, "C49_Folder_Blue_Open.png");
			this.m_ilClientIcons.Images.SetKeyName(50, "C50_Folder_Tar.png");
			this.m_ilClientIcons.Images.SetKeyName(51, "C51_Decrypted.png");
			this.m_ilClientIcons.Images.SetKeyName(52, "C52_Encrypted.png");
			this.m_ilClientIcons.Images.SetKeyName(53, "C53_Apply.png");
			this.m_ilClientIcons.Images.SetKeyName(54, "C54_Signature.png");
			this.m_ilClientIcons.Images.SetKeyName(55, "C55_Thumbnail.png");
			this.m_ilClientIcons.Images.SetKeyName(56, "C56_KAddressBook.png");
			this.m_ilClientIcons.Images.SetKeyName(57, "C57_View_Text.png");
			this.m_ilClientIcons.Images.SetKeyName(58, "C58_KGPG.png");
			this.m_ilClientIcons.Images.SetKeyName(59, "C59_Package_Development.png");
			this.m_ilClientIcons.Images.SetKeyName(60, "C60_KFM_Home.png");
			this.m_ilClientIcons.Images.SetKeyName(61, "C61_Services.png");
			this.m_ilClientIcons.Images.SetKeyName(62, "C62_Empty.png");
			this.m_ilClientIcons.Images.SetKeyName(63, "C63_SortUp.png");
			this.m_ilClientIcons.Images.SetKeyName(64, "C64_SortDown.png");
			// 
			// m_ctxPwList
			// 
			this.m_ctxPwList.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_ctxEntryCopyUserName,
            this.m_ctxEntryCopyPassword,
            this.m_ctxEntryUrl,
            this.m_ctxEntryCopyCustomString,
            this.m_ctxEntrySaveAttachedFiles,
            this.m_ctxEntrySep0,
            this.m_ctxEntryPerformAutoType,
            this.m_ctxEntrySep1,
            this.m_ctxEntryAdd,
            this.m_ctxEntryEdit,
            this.m_ctxEntryDuplicate,
            this.m_ctxEntryDelete,
            this.m_ctxEntryMassModify,
            this.m_ctxEntrySelectAll,
            this.m_ctxEntrySep2,
            this.m_ctxEntryClipboard,
            this.m_ctxEntryRearrangePopup});
			this.m_ctxPwList.Name = "m_ctxPwList";
			resources.ApplyResources(this.m_ctxPwList, "m_ctxPwList");
			this.m_ctxPwList.Opening += new System.ComponentModel.CancelEventHandler(this.OnCtxPwListOpening);
			// 
			// m_ctxEntryCopyUserName
			// 
			this.m_ctxEntryCopyUserName.Image = global::KeePass.Properties.Resources.B16x16_Personal;
			this.m_ctxEntryCopyUserName.Name = "m_ctxEntryCopyUserName";
			resources.ApplyResources(this.m_ctxEntryCopyUserName, "m_ctxEntryCopyUserName");
			this.m_ctxEntryCopyUserName.Click += new System.EventHandler(this.OnEntryCopyUserName);
			// 
			// m_ctxEntryCopyPassword
			// 
			this.m_ctxEntryCopyPassword.Image = global::KeePass.Properties.Resources.B16x16_KGPG_Info;
			this.m_ctxEntryCopyPassword.Name = "m_ctxEntryCopyPassword";
			resources.ApplyResources(this.m_ctxEntryCopyPassword, "m_ctxEntryCopyPassword");
			this.m_ctxEntryCopyPassword.Click += new System.EventHandler(this.OnEntryCopyPassword);
			// 
			// m_ctxEntryUrl
			// 
			this.m_ctxEntryUrl.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_ctxEntryOpenUrl,
            this.m_ctxEntryCopyUrl});
			this.m_ctxEntryUrl.Name = "m_ctxEntryUrl";
			resources.ApplyResources(this.m_ctxEntryUrl, "m_ctxEntryUrl");
			// 
			// m_ctxEntryOpenUrl
			// 
			this.m_ctxEntryOpenUrl.Image = global::KeePass.Properties.Resources.B16x16_FTP;
			this.m_ctxEntryOpenUrl.Name = "m_ctxEntryOpenUrl";
			resources.ApplyResources(this.m_ctxEntryOpenUrl, "m_ctxEntryOpenUrl");
			this.m_ctxEntryOpenUrl.Click += new System.EventHandler(this.OnEntryOpenUrl);
			// 
			// m_ctxEntryCopyUrl
			// 
			this.m_ctxEntryCopyUrl.Image = global::KeePass.Properties.Resources.B16x16_EditCopy;
			this.m_ctxEntryCopyUrl.Name = "m_ctxEntryCopyUrl";
			resources.ApplyResources(this.m_ctxEntryCopyUrl, "m_ctxEntryCopyUrl");
			this.m_ctxEntryCopyUrl.Click += new System.EventHandler(this.OnEntryCopyURL);
			// 
			// m_ctxEntryCopyCustomString
			// 
			this.m_ctxEntryCopyCustomString.Name = "m_ctxEntryCopyCustomString";
			resources.ApplyResources(this.m_ctxEntryCopyCustomString, "m_ctxEntryCopyCustomString");
			// 
			// m_ctxEntrySaveAttachedFiles
			// 
			this.m_ctxEntrySaveAttachedFiles.Image = global::KeePass.Properties.Resources.B16x16_Attach;
			this.m_ctxEntrySaveAttachedFiles.Name = "m_ctxEntrySaveAttachedFiles";
			resources.ApplyResources(this.m_ctxEntrySaveAttachedFiles, "m_ctxEntrySaveAttachedFiles");
			this.m_ctxEntrySaveAttachedFiles.Click += new System.EventHandler(this.OnEntrySaveAttachments);
			// 
			// m_ctxEntrySep0
			// 
			this.m_ctxEntrySep0.Name = "m_ctxEntrySep0";
			resources.ApplyResources(this.m_ctxEntrySep0, "m_ctxEntrySep0");
			// 
			// m_ctxEntryPerformAutoType
			// 
			this.m_ctxEntryPerformAutoType.Image = global::KeePass.Properties.Resources.B16x16_KRec_Record;
			this.m_ctxEntryPerformAutoType.Name = "m_ctxEntryPerformAutoType";
			resources.ApplyResources(this.m_ctxEntryPerformAutoType, "m_ctxEntryPerformAutoType");
			this.m_ctxEntryPerformAutoType.Click += new System.EventHandler(this.OnEntryPerformAutoType);
			// 
			// m_ctxEntrySep1
			// 
			this.m_ctxEntrySep1.Name = "m_ctxEntrySep1";
			resources.ApplyResources(this.m_ctxEntrySep1, "m_ctxEntrySep1");
			// 
			// m_ctxEntryAdd
			// 
			this.m_ctxEntryAdd.Image = global::KeePass.Properties.Resources.B16x16_KGPG_Import;
			this.m_ctxEntryAdd.Name = "m_ctxEntryAdd";
			resources.ApplyResources(this.m_ctxEntryAdd, "m_ctxEntryAdd");
			this.m_ctxEntryAdd.Click += new System.EventHandler(this.OnEntryAdd);
			// 
			// m_ctxEntryEdit
			// 
			this.m_ctxEntryEdit.Image = global::KeePass.Properties.Resources.B16x16_KGPG_Sign;
			this.m_ctxEntryEdit.Name = "m_ctxEntryEdit";
			resources.ApplyResources(this.m_ctxEntryEdit, "m_ctxEntryEdit");
			this.m_ctxEntryEdit.Click += new System.EventHandler(this.OnEntryEdit);
			// 
			// m_ctxEntryDuplicate
			// 
			this.m_ctxEntryDuplicate.Image = global::KeePass.Properties.Resources.B16x16_KGPG_Key2;
			this.m_ctxEntryDuplicate.Name = "m_ctxEntryDuplicate";
			resources.ApplyResources(this.m_ctxEntryDuplicate, "m_ctxEntryDuplicate");
			this.m_ctxEntryDuplicate.Click += new System.EventHandler(this.OnEntryDuplicate);
			// 
			// m_ctxEntryDelete
			// 
			this.m_ctxEntryDelete.Image = global::KeePass.Properties.Resources.B16x16_DeleteEntry;
			this.m_ctxEntryDelete.Name = "m_ctxEntryDelete";
			resources.ApplyResources(this.m_ctxEntryDelete, "m_ctxEntryDelete");
			this.m_ctxEntryDelete.Click += new System.EventHandler(this.OnEntryDelete);
			// 
			// m_ctxEntryMassModify
			// 
			this.m_ctxEntryMassModify.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_ctxEntrySetColor,
            this.m_ctxEntryMMSep0,
            this.m_ctxEntryMassSetIcon});
			this.m_ctxEntryMassModify.Name = "m_ctxEntryMassModify";
			resources.ApplyResources(this.m_ctxEntryMassModify, "m_ctxEntryMassModify");
			// 
			// m_ctxEntrySetColor
			// 
			this.m_ctxEntrySetColor.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_ctxEntryColorStandard,
            this.m_ctxEntryColorSep0,
            this.m_ctxEntryColorLightRed,
            this.m_ctxEntryColorLightGreen,
            this.m_ctxEntryColorLightBlue,
            this.m_ctxEntryColorLightYellow,
            this.m_ctxEntryColorSep1,
            this.m_ctxEntryColorCustom});
			this.m_ctxEntrySetColor.Name = "m_ctxEntrySetColor";
			resources.ApplyResources(this.m_ctxEntrySetColor, "m_ctxEntrySetColor");
			// 
			// m_ctxEntryColorStandard
			// 
			this.m_ctxEntryColorStandard.Name = "m_ctxEntryColorStandard";
			resources.ApplyResources(this.m_ctxEntryColorStandard, "m_ctxEntryColorStandard");
			this.m_ctxEntryColorStandard.Click += new System.EventHandler(this.OnEntryColorStandard);
			// 
			// m_ctxEntryColorSep0
			// 
			this.m_ctxEntryColorSep0.Name = "m_ctxEntryColorSep0";
			resources.ApplyResources(this.m_ctxEntryColorSep0, "m_ctxEntryColorSep0");
			// 
			// m_ctxEntryColorLightRed
			// 
			this.m_ctxEntryColorLightRed.Name = "m_ctxEntryColorLightRed";
			resources.ApplyResources(this.m_ctxEntryColorLightRed, "m_ctxEntryColorLightRed");
			this.m_ctxEntryColorLightRed.Click += new System.EventHandler(this.OnEntryColorLightRed);
			// 
			// m_ctxEntryColorLightGreen
			// 
			this.m_ctxEntryColorLightGreen.Name = "m_ctxEntryColorLightGreen";
			resources.ApplyResources(this.m_ctxEntryColorLightGreen, "m_ctxEntryColorLightGreen");
			this.m_ctxEntryColorLightGreen.Click += new System.EventHandler(this.OnEntryColorLightGreen);
			// 
			// m_ctxEntryColorLightBlue
			// 
			this.m_ctxEntryColorLightBlue.Name = "m_ctxEntryColorLightBlue";
			resources.ApplyResources(this.m_ctxEntryColorLightBlue, "m_ctxEntryColorLightBlue");
			this.m_ctxEntryColorLightBlue.Click += new System.EventHandler(this.OnEntryColorLightBlue);
			// 
			// m_ctxEntryColorLightYellow
			// 
			this.m_ctxEntryColorLightYellow.Name = "m_ctxEntryColorLightYellow";
			resources.ApplyResources(this.m_ctxEntryColorLightYellow, "m_ctxEntryColorLightYellow");
			this.m_ctxEntryColorLightYellow.Click += new System.EventHandler(this.OnEntryColorLightYellow);
			// 
			// m_ctxEntryColorSep1
			// 
			this.m_ctxEntryColorSep1.Name = "m_ctxEntryColorSep1";
			resources.ApplyResources(this.m_ctxEntryColorSep1, "m_ctxEntryColorSep1");
			// 
			// m_ctxEntryColorCustom
			// 
			this.m_ctxEntryColorCustom.Name = "m_ctxEntryColorCustom";
			resources.ApplyResources(this.m_ctxEntryColorCustom, "m_ctxEntryColorCustom");
			this.m_ctxEntryColorCustom.Click += new System.EventHandler(this.OnEntryColorCustom);
			// 
			// m_ctxEntryMMSep0
			// 
			this.m_ctxEntryMMSep0.Name = "m_ctxEntryMMSep0";
			resources.ApplyResources(this.m_ctxEntryMMSep0, "m_ctxEntryMMSep0");
			// 
			// m_ctxEntryMassSetIcon
			// 
			this.m_ctxEntryMassSetIcon.Image = global::KeePass.Properties.Resources.B16x16_Spreadsheet;
			this.m_ctxEntryMassSetIcon.Name = "m_ctxEntryMassSetIcon";
			resources.ApplyResources(this.m_ctxEntryMassSetIcon, "m_ctxEntryMassSetIcon");
			this.m_ctxEntryMassSetIcon.Click += new System.EventHandler(this.OnEntryMassSetIcon);
			// 
			// m_ctxEntrySelectAll
			// 
			this.m_ctxEntrySelectAll.Image = global::KeePass.Properties.Resources.B16x16_KGPG_Key3;
			this.m_ctxEntrySelectAll.Name = "m_ctxEntrySelectAll";
			resources.ApplyResources(this.m_ctxEntrySelectAll, "m_ctxEntrySelectAll");
			this.m_ctxEntrySelectAll.Click += new System.EventHandler(this.OnEntrySelectAll);
			// 
			// m_ctxEntrySep2
			// 
			this.m_ctxEntrySep2.Name = "m_ctxEntrySep2";
			resources.ApplyResources(this.m_ctxEntrySep2, "m_ctxEntrySep2");
			// 
			// m_ctxEntryClipboard
			// 
			this.m_ctxEntryClipboard.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_ctxEntryClipCopy,
            this.m_ctxEntryClipPaste});
			this.m_ctxEntryClipboard.Name = "m_ctxEntryClipboard";
			resources.ApplyResources(this.m_ctxEntryClipboard, "m_ctxEntryClipboard");
			// 
			// m_ctxEntryClipCopy
			// 
			this.m_ctxEntryClipCopy.Name = "m_ctxEntryClipCopy";
			resources.ApplyResources(this.m_ctxEntryClipCopy, "m_ctxEntryClipCopy");
			this.m_ctxEntryClipCopy.Click += new System.EventHandler(this.OnEntryClipCopy);
			// 
			// m_ctxEntryClipPaste
			// 
			this.m_ctxEntryClipPaste.Name = "m_ctxEntryClipPaste";
			resources.ApplyResources(this.m_ctxEntryClipPaste, "m_ctxEntryClipPaste");
			this.m_ctxEntryClipPaste.Click += new System.EventHandler(this.OnEntryClipPaste);
			// 
			// m_ctxEntryRearrangePopup
			// 
			this.m_ctxEntryRearrangePopup.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_ctxEntryMoveToTop,
            this.m_ctxEntryMoveOneUp,
            this.m_ctxEntryMoveOneDown,
            this.m_ctxEntryMoveToBottom,
            this.m_ctxEntryRearrangeSep0,
            this.m_ctxEntrySortUnsorted,
            this.m_ctxEntryRearrangeSep1,
            this.m_ctxEntrySortListByTitle,
            this.m_ctxEntrySortListByUserName,
            this.m_ctxEntrySortListByPassword,
            this.m_ctxEntrySortListByURL,
            this.m_ctxEntrySortListByNotes,
            this.m_ctxEntrySortListByCreationTime,
            this.m_ctxEntrySortListByLastModTime,
            this.m_ctxEntrySortListByLastAccessTime,
            this.m_ctxEntrySortListByExpirationTime});
			this.m_ctxEntryRearrangePopup.Name = "m_ctxEntryRearrangePopup";
			resources.ApplyResources(this.m_ctxEntryRearrangePopup, "m_ctxEntryRearrangePopup");
			// 
			// m_ctxEntryMoveToTop
			// 
			this.m_ctxEntryMoveToTop.Image = global::KeePass.Properties.Resources.B16x16_2UpArrow;
			this.m_ctxEntryMoveToTop.Name = "m_ctxEntryMoveToTop";
			resources.ApplyResources(this.m_ctxEntryMoveToTop, "m_ctxEntryMoveToTop");
			this.m_ctxEntryMoveToTop.Click += new System.EventHandler(this.OnEntryMoveToTop);
			// 
			// m_ctxEntryMoveOneUp
			// 
			this.m_ctxEntryMoveOneUp.Image = global::KeePass.Properties.Resources.B16x16_1UpArrow;
			this.m_ctxEntryMoveOneUp.Name = "m_ctxEntryMoveOneUp";
			resources.ApplyResources(this.m_ctxEntryMoveOneUp, "m_ctxEntryMoveOneUp");
			this.m_ctxEntryMoveOneUp.Click += new System.EventHandler(this.OnEntryMoveOneUp);
			// 
			// m_ctxEntryMoveOneDown
			// 
			this.m_ctxEntryMoveOneDown.Image = global::KeePass.Properties.Resources.B16x16_1DownArrow;
			this.m_ctxEntryMoveOneDown.Name = "m_ctxEntryMoveOneDown";
			resources.ApplyResources(this.m_ctxEntryMoveOneDown, "m_ctxEntryMoveOneDown");
			this.m_ctxEntryMoveOneDown.Click += new System.EventHandler(this.OnEntryMoveOneDown);
			// 
			// m_ctxEntryMoveToBottom
			// 
			this.m_ctxEntryMoveToBottom.Image = global::KeePass.Properties.Resources.B16x16_2DownArrow;
			this.m_ctxEntryMoveToBottom.Name = "m_ctxEntryMoveToBottom";
			resources.ApplyResources(this.m_ctxEntryMoveToBottom, "m_ctxEntryMoveToBottom");
			this.m_ctxEntryMoveToBottom.Click += new System.EventHandler(this.OnEntryMoveToBottom);
			// 
			// m_ctxEntryRearrangeSep0
			// 
			this.m_ctxEntryRearrangeSep0.Name = "m_ctxEntryRearrangeSep0";
			resources.ApplyResources(this.m_ctxEntryRearrangeSep0, "m_ctxEntryRearrangeSep0");
			// 
			// m_ctxEntrySortUnsorted
			// 
			this.m_ctxEntrySortUnsorted.Image = global::KeePass.Properties.Resources.B16x16_Reload_Page;
			this.m_ctxEntrySortUnsorted.Name = "m_ctxEntrySortUnsorted";
			resources.ApplyResources(this.m_ctxEntrySortUnsorted, "m_ctxEntrySortUnsorted");
			this.m_ctxEntrySortUnsorted.Click += new System.EventHandler(this.OnEntrySortUnsorted);
			// 
			// m_ctxEntryRearrangeSep1
			// 
			this.m_ctxEntryRearrangeSep1.Name = "m_ctxEntryRearrangeSep1";
			resources.ApplyResources(this.m_ctxEntryRearrangeSep1, "m_ctxEntryRearrangeSep1");
			// 
			// m_ctxEntrySortListByTitle
			// 
			this.m_ctxEntrySortListByTitle.Image = global::KeePass.Properties.Resources.B16x16_Reload_Page;
			this.m_ctxEntrySortListByTitle.Name = "m_ctxEntrySortListByTitle";
			resources.ApplyResources(this.m_ctxEntrySortListByTitle, "m_ctxEntrySortListByTitle");
			this.m_ctxEntrySortListByTitle.Click += new System.EventHandler(this.OnEntrySortTitle);
			// 
			// m_ctxEntrySortListByUserName
			// 
			this.m_ctxEntrySortListByUserName.Image = global::KeePass.Properties.Resources.B16x16_Reload_Page;
			this.m_ctxEntrySortListByUserName.Name = "m_ctxEntrySortListByUserName";
			resources.ApplyResources(this.m_ctxEntrySortListByUserName, "m_ctxEntrySortListByUserName");
			this.m_ctxEntrySortListByUserName.Click += new System.EventHandler(this.OnEntrySortUserName);
			// 
			// m_ctxEntrySortListByPassword
			// 
			this.m_ctxEntrySortListByPassword.Image = global::KeePass.Properties.Resources.B16x16_Reload_Page;
			this.m_ctxEntrySortListByPassword.Name = "m_ctxEntrySortListByPassword";
			resources.ApplyResources(this.m_ctxEntrySortListByPassword, "m_ctxEntrySortListByPassword");
			this.m_ctxEntrySortListByPassword.Click += new System.EventHandler(this.OnEntrySortPassword);
			// 
			// m_ctxEntrySortListByURL
			// 
			this.m_ctxEntrySortListByURL.Image = global::KeePass.Properties.Resources.B16x16_Reload_Page;
			this.m_ctxEntrySortListByURL.Name = "m_ctxEntrySortListByURL";
			resources.ApplyResources(this.m_ctxEntrySortListByURL, "m_ctxEntrySortListByURL");
			this.m_ctxEntrySortListByURL.Click += new System.EventHandler(this.OnEntrySortURL);
			// 
			// m_ctxEntrySortListByNotes
			// 
			this.m_ctxEntrySortListByNotes.Image = global::KeePass.Properties.Resources.B16x16_Reload_Page;
			this.m_ctxEntrySortListByNotes.Name = "m_ctxEntrySortListByNotes";
			resources.ApplyResources(this.m_ctxEntrySortListByNotes, "m_ctxEntrySortListByNotes");
			this.m_ctxEntrySortListByNotes.Click += new System.EventHandler(this.OnEntrySortNotes);
			// 
			// m_ctxEntrySortListByCreationTime
			// 
			this.m_ctxEntrySortListByCreationTime.Image = global::KeePass.Properties.Resources.B16x16_History;
			this.m_ctxEntrySortListByCreationTime.Name = "m_ctxEntrySortListByCreationTime";
			resources.ApplyResources(this.m_ctxEntrySortListByCreationTime, "m_ctxEntrySortListByCreationTime");
			this.m_ctxEntrySortListByCreationTime.Click += new System.EventHandler(this.OnEntrySortCreationTime);
			// 
			// m_ctxEntrySortListByLastModTime
			// 
			this.m_ctxEntrySortListByLastModTime.Image = global::KeePass.Properties.Resources.B16x16_History;
			this.m_ctxEntrySortListByLastModTime.Name = "m_ctxEntrySortListByLastModTime";
			resources.ApplyResources(this.m_ctxEntrySortListByLastModTime, "m_ctxEntrySortListByLastModTime");
			this.m_ctxEntrySortListByLastModTime.Click += new System.EventHandler(this.OnEntrySortLastMod);
			// 
			// m_ctxEntrySortListByLastAccessTime
			// 
			this.m_ctxEntrySortListByLastAccessTime.Image = global::KeePass.Properties.Resources.B16x16_History;
			this.m_ctxEntrySortListByLastAccessTime.Name = "m_ctxEntrySortListByLastAccessTime";
			resources.ApplyResources(this.m_ctxEntrySortListByLastAccessTime, "m_ctxEntrySortListByLastAccessTime");
			this.m_ctxEntrySortListByLastAccessTime.Click += new System.EventHandler(this.OnEntrySortLastAccess);
			// 
			// m_ctxEntrySortListByExpirationTime
			// 
			this.m_ctxEntrySortListByExpirationTime.Image = global::KeePass.Properties.Resources.B16x16_History;
			this.m_ctxEntrySortListByExpirationTime.Name = "m_ctxEntrySortListByExpirationTime";
			resources.ApplyResources(this.m_ctxEntrySortListByExpirationTime, "m_ctxEntrySortListByExpirationTime");
			this.m_ctxEntrySortListByExpirationTime.Click += new System.EventHandler(this.OnEntrySortExpiration);
			// 
			// m_menuMain
			// 
			this.m_menuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuFile,
            this.m_menuEdit,
            this.m_menuView,
            this.m_menuTools,
            this.m_menuHelp});
			resources.ApplyResources(this.m_menuMain, "m_menuMain");
			this.m_menuMain.Name = "m_menuMain";
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
            this.m_menuFileSynchronize,
            this.m_menuFileSep4,
            this.m_menuFileLock,
            this.m_menuFileExit});
			this.m_menuFile.Name = "m_menuFile";
			resources.ApplyResources(this.m_menuFile, "m_menuFile");
			// 
			// m_menuFileNew
			// 
			this.m_menuFileNew.Image = global::KeePass.Properties.Resources.B16x16_FileNew;
			this.m_menuFileNew.Name = "m_menuFileNew";
			resources.ApplyResources(this.m_menuFileNew, "m_menuFileNew");
			this.m_menuFileNew.Click += new System.EventHandler(this.OnFileNew);
			// 
			// m_menuFileOpen
			// 
			this.m_menuFileOpen.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuFileOpenLocal,
            this.m_menuFileOpenUrl});
			this.m_menuFileOpen.Name = "m_menuFileOpen";
			resources.ApplyResources(this.m_menuFileOpen, "m_menuFileOpen");
			// 
			// m_menuFileOpenLocal
			// 
			this.m_menuFileOpenLocal.Image = global::KeePass.Properties.Resources.B16x16_Folder_Yellow_Open;
			this.m_menuFileOpenLocal.Name = "m_menuFileOpenLocal";
			resources.ApplyResources(this.m_menuFileOpenLocal, "m_menuFileOpenLocal");
			this.m_menuFileOpenLocal.Click += new System.EventHandler(this.OnFileOpen);
			// 
			// m_menuFileOpenUrl
			// 
			this.m_menuFileOpenUrl.Image = global::KeePass.Properties.Resources.B16x16_Browser;
			this.m_menuFileOpenUrl.Name = "m_menuFileOpenUrl";
			resources.ApplyResources(this.m_menuFileOpenUrl, "m_menuFileOpenUrl");
			this.m_menuFileOpenUrl.Click += new System.EventHandler(this.OnFileOpenUrl);
			// 
			// m_menuFileRecent
			// 
			this.m_menuFileRecent.Name = "m_menuFileRecent";
			resources.ApplyResources(this.m_menuFileRecent, "m_menuFileRecent");
			// 
			// m_menuFileClose
			// 
			this.m_menuFileClose.Image = global::KeePass.Properties.Resources.B16x16_Folder_Locked;
			this.m_menuFileClose.Name = "m_menuFileClose";
			resources.ApplyResources(this.m_menuFileClose, "m_menuFileClose");
			this.m_menuFileClose.Click += new System.EventHandler(this.OnFileClose);
			// 
			// m_menuFileSep0
			// 
			this.m_menuFileSep0.Name = "m_menuFileSep0";
			resources.ApplyResources(this.m_menuFileSep0, "m_menuFileSep0");
			// 
			// m_menuFileSave
			// 
			this.m_menuFileSave.Image = global::KeePass.Properties.Resources.B16x16_FileSave;
			this.m_menuFileSave.Name = "m_menuFileSave";
			resources.ApplyResources(this.m_menuFileSave, "m_menuFileSave");
			this.m_menuFileSave.Click += new System.EventHandler(this.OnFileSave);
			// 
			// m_menuFileSaveAs
			// 
			this.m_menuFileSaveAs.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuFileSaveAsLocal,
            this.m_menuFileSaveAsUrl});
			this.m_menuFileSaveAs.Name = "m_menuFileSaveAs";
			resources.ApplyResources(this.m_menuFileSaveAs, "m_menuFileSaveAs");
			// 
			// m_menuFileSaveAsLocal
			// 
			this.m_menuFileSaveAsLocal.Image = global::KeePass.Properties.Resources.B16x16_FileSaveAs;
			this.m_menuFileSaveAsLocal.Name = "m_menuFileSaveAsLocal";
			resources.ApplyResources(this.m_menuFileSaveAsLocal, "m_menuFileSaveAsLocal");
			this.m_menuFileSaveAsLocal.Click += new System.EventHandler(this.OnFileSaveAs);
			// 
			// m_menuFileSaveAsUrl
			// 
			this.m_menuFileSaveAsUrl.Image = global::KeePass.Properties.Resources.B16x16_Browser;
			this.m_menuFileSaveAsUrl.Name = "m_menuFileSaveAsUrl";
			resources.ApplyResources(this.m_menuFileSaveAsUrl, "m_menuFileSaveAsUrl");
			this.m_menuFileSaveAsUrl.Click += new System.EventHandler(this.OnFileSaveAsUrl);
			// 
			// m_menuFileSep1
			// 
			this.m_menuFileSep1.Name = "m_menuFileSep1";
			resources.ApplyResources(this.m_menuFileSep1, "m_menuFileSep1");
			// 
			// m_menuFileDbSettings
			// 
			this.m_menuFileDbSettings.Image = global::KeePass.Properties.Resources.B16x16_Package_Development;
			this.m_menuFileDbSettings.Name = "m_menuFileDbSettings";
			resources.ApplyResources(this.m_menuFileDbSettings, "m_menuFileDbSettings");
			this.m_menuFileDbSettings.Click += new System.EventHandler(this.OnFileDbSettings);
			// 
			// m_menuFileChangeMasterKey
			// 
			this.m_menuFileChangeMasterKey.Image = global::KeePass.Properties.Resources.B16x16_File_Locked;
			this.m_menuFileChangeMasterKey.Name = "m_menuFileChangeMasterKey";
			resources.ApplyResources(this.m_menuFileChangeMasterKey, "m_menuFileChangeMasterKey");
			this.m_menuFileChangeMasterKey.Click += new System.EventHandler(this.OnFileChangeMasterKey);
			// 
			// m_menuFileSep2
			// 
			this.m_menuFileSep2.Name = "m_menuFileSep2";
			resources.ApplyResources(this.m_menuFileSep2, "m_menuFileSep2");
			// 
			// m_menuFilePrint
			// 
			this.m_menuFilePrint.Image = global::KeePass.Properties.Resources.B16x16_FilePrint;
			this.m_menuFilePrint.Name = "m_menuFilePrint";
			resources.ApplyResources(this.m_menuFilePrint, "m_menuFilePrint");
			this.m_menuFilePrint.Click += new System.EventHandler(this.OnFilePrint);
			// 
			// m_menuFileSep3
			// 
			this.m_menuFileSep3.Name = "m_menuFileSep3";
			resources.ApplyResources(this.m_menuFileSep3, "m_menuFileSep3");
			// 
			// m_menuFileImport
			// 
			this.m_menuFileImport.Image = global::KeePass.Properties.Resources.B16x16_Folder_Inbox;
			this.m_menuFileImport.Name = "m_menuFileImport";
			resources.ApplyResources(this.m_menuFileImport, "m_menuFileImport");
			this.m_menuFileImport.Click += new System.EventHandler(this.OnFileImport);
			// 
			// m_menuFileExport
			// 
			this.m_menuFileExport.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuFileExportUseXsl,
            this.m_menuFileExportSep0,
            this.m_menuFileExportXML,
            this.m_menuFileExportHtml,
            this.m_menuFileExportSep1,
            this.m_menuFileExportKdb3});
			this.m_menuFileExport.Name = "m_menuFileExport";
			resources.ApplyResources(this.m_menuFileExport, "m_menuFileExport");
			// 
			// m_menuFileExportUseXsl
			// 
			this.m_menuFileExportUseXsl.Image = global::KeePass.Properties.Resources.B16x16_CompFile;
			this.m_menuFileExportUseXsl.Name = "m_menuFileExportUseXsl";
			resources.ApplyResources(this.m_menuFileExportUseXsl, "m_menuFileExportUseXsl");
			this.m_menuFileExportUseXsl.Click += new System.EventHandler(this.OnMenuFileExportUseXsl);
			// 
			// m_menuFileExportSep0
			// 
			this.m_menuFileExportSep0.Name = "m_menuFileExportSep0";
			resources.ApplyResources(this.m_menuFileExportSep0, "m_menuFileExportSep0");
			// 
			// m_menuFileExportXML
			// 
			this.m_menuFileExportXML.Image = global::KeePass.Properties.Resources.B16x16_Binary;
			this.m_menuFileExportXML.Name = "m_menuFileExportXML";
			resources.ApplyResources(this.m_menuFileExportXML, "m_menuFileExportXML");
			this.m_menuFileExportXML.Click += new System.EventHandler(this.OnMenuFileExportXml);
			// 
			// m_menuFileExportHtml
			// 
			this.m_menuFileExportHtml.Image = global::KeePass.Properties.Resources.B16x16_HTML;
			this.m_menuFileExportHtml.Name = "m_menuFileExportHtml";
			resources.ApplyResources(this.m_menuFileExportHtml, "m_menuFileExportHtml");
			this.m_menuFileExportHtml.Click += new System.EventHandler(this.OnMenuFileExportHTML);
			// 
			// m_menuFileExportSep1
			// 
			this.m_menuFileExportSep1.Name = "m_menuFileExportSep1";
			resources.ApplyResources(this.m_menuFileExportSep1, "m_menuFileExportSep1");
			// 
			// m_menuFileExportKdb3
			// 
			this.m_menuFileExportKdb3.Name = "m_menuFileExportKdb3";
			resources.ApplyResources(this.m_menuFileExportKdb3, "m_menuFileExportKdb3");
			this.m_menuFileExportKdb3.Click += new System.EventHandler(this.OnMenuFileExportKdb3);
			// 
			// m_menuFileSynchronize
			// 
			this.m_menuFileSynchronize.Image = global::KeePass.Properties.Resources.B16x16_Reload_Page;
			this.m_menuFileSynchronize.Name = "m_menuFileSynchronize";
			resources.ApplyResources(this.m_menuFileSynchronize, "m_menuFileSynchronize");
			this.m_menuFileSynchronize.Click += new System.EventHandler(this.OnFileSynchronize);
			// 
			// m_menuFileSep4
			// 
			this.m_menuFileSep4.Name = "m_menuFileSep4";
			resources.ApplyResources(this.m_menuFileSep4, "m_menuFileSep4");
			// 
			// m_menuFileLock
			// 
			this.m_menuFileLock.Image = global::KeePass.Properties.Resources.B16x16_LockWorkspace;
			this.m_menuFileLock.Name = "m_menuFileLock";
			resources.ApplyResources(this.m_menuFileLock, "m_menuFileLock");
			this.m_menuFileLock.Click += new System.EventHandler(this.OnFileLock);
			// 
			// m_menuFileExit
			// 
			this.m_menuFileExit.Image = global::KeePass.Properties.Resources.B16x16_Exit;
			this.m_menuFileExit.Name = "m_menuFileExit";
			resources.ApplyResources(this.m_menuFileExit, "m_menuFileExit");
			this.m_menuFileExit.Click += new System.EventHandler(this.OnFileExit);
			// 
			// m_menuEdit
			// 
			this.m_menuEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuEditShowAllEntries,
            this.m_menuEditShowExpired,
            this.m_menuEditSep0,
            this.m_menuEditFind});
			this.m_menuEdit.Name = "m_menuEdit";
			resources.ApplyResources(this.m_menuEdit, "m_menuEdit");
			// 
			// m_menuEditShowAllEntries
			// 
			this.m_menuEditShowAllEntries.Image = global::KeePass.Properties.Resources.B16x16_KGPG_Key3;
			this.m_menuEditShowAllEntries.Name = "m_menuEditShowAllEntries";
			resources.ApplyResources(this.m_menuEditShowAllEntries, "m_menuEditShowAllEntries");
			this.m_menuEditShowAllEntries.Click += new System.EventHandler(this.OnShowAllEntries);
			// 
			// m_menuEditShowExpired
			// 
			this.m_menuEditShowExpired.Image = global::KeePass.Properties.Resources.B16x16_History_Clear;
			this.m_menuEditShowExpired.Name = "m_menuEditShowExpired";
			resources.ApplyResources(this.m_menuEditShowExpired, "m_menuEditShowExpired");
			this.m_menuEditShowExpired.Click += new System.EventHandler(this.OnToolsShowExpired);
			// 
			// m_menuEditSep0
			// 
			this.m_menuEditSep0.Name = "m_menuEditSep0";
			resources.ApplyResources(this.m_menuEditSep0, "m_menuEditSep0");
			// 
			// m_menuEditFind
			// 
			this.m_menuEditFind.Image = global::KeePass.Properties.Resources.B16x16_XMag;
			this.m_menuEditFind.Name = "m_menuEditFind";
			resources.ApplyResources(this.m_menuEditFind, "m_menuEditFind");
			this.m_menuEditFind.Click += new System.EventHandler(this.OnPwListFind);
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
            this.m_menuViewHiding,
            this.m_menuViewSep3,
            this.m_menuViewTanOptions,
            this.m_menuViewColumns});
			this.m_menuView.Name = "m_menuView";
			resources.ApplyResources(this.m_menuView, "m_menuView");
			// 
			// m_menuChangeLanguage
			// 
			this.m_menuChangeLanguage.Image = global::KeePass.Properties.Resources.B16x16_Keyboard_Layout;
			this.m_menuChangeLanguage.Name = "m_menuChangeLanguage";
			resources.ApplyResources(this.m_menuChangeLanguage, "m_menuChangeLanguage");
			this.m_menuChangeLanguage.Click += new System.EventHandler(this.OnMenuChangeLanguage);
			// 
			// m_menuViewSep0
			// 
			this.m_menuViewSep0.Name = "m_menuViewSep0";
			resources.ApplyResources(this.m_menuViewSep0, "m_menuViewSep0");
			// 
			// m_menuViewShowToolBar
			// 
			this.m_menuViewShowToolBar.CheckOnClick = true;
			this.m_menuViewShowToolBar.Name = "m_menuViewShowToolBar";
			resources.ApplyResources(this.m_menuViewShowToolBar, "m_menuViewShowToolBar");
			this.m_menuViewShowToolBar.Click += new System.EventHandler(this.OnViewShowToolBar);
			// 
			// m_menuViewShowEntryView
			// 
			this.m_menuViewShowEntryView.CheckOnClick = true;
			this.m_menuViewShowEntryView.Name = "m_menuViewShowEntryView";
			resources.ApplyResources(this.m_menuViewShowEntryView, "m_menuViewShowEntryView");
			this.m_menuViewShowEntryView.Click += new System.EventHandler(this.OnViewShowEntryView);
			// 
			// m_menuViewWindowLayout
			// 
			this.m_menuViewWindowLayout.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuViewWindowsStacked,
            this.m_menuViewWindowsSideBySide});
			this.m_menuViewWindowLayout.Name = "m_menuViewWindowLayout";
			resources.ApplyResources(this.m_menuViewWindowLayout, "m_menuViewWindowLayout");
			// 
			// m_menuViewWindowsStacked
			// 
			this.m_menuViewWindowsStacked.Image = global::KeePass.Properties.Resources.B16x16_Window_2Horz1Vert;
			this.m_menuViewWindowsStacked.Name = "m_menuViewWindowsStacked";
			resources.ApplyResources(this.m_menuViewWindowsStacked, "m_menuViewWindowsStacked");
			this.m_menuViewWindowsStacked.Click += new System.EventHandler(this.OnViewWindowsStacked);
			// 
			// m_menuViewWindowsSideBySide
			// 
			this.m_menuViewWindowsSideBySide.Image = global::KeePass.Properties.Resources.B16x16_Window_3Horz;
			this.m_menuViewWindowsSideBySide.Name = "m_menuViewWindowsSideBySide";
			resources.ApplyResources(this.m_menuViewWindowsSideBySide, "m_menuViewWindowsSideBySide");
			this.m_menuViewWindowsSideBySide.Click += new System.EventHandler(this.OnViewWindowsSideBySide);
			// 
			// m_menuViewSep1
			// 
			this.m_menuViewSep1.Name = "m_menuViewSep1";
			resources.ApplyResources(this.m_menuViewSep1, "m_menuViewSep1");
			// 
			// m_menuViewAlwaysOnTop
			// 
			this.m_menuViewAlwaysOnTop.CheckOnClick = true;
			this.m_menuViewAlwaysOnTop.Name = "m_menuViewAlwaysOnTop";
			resources.ApplyResources(this.m_menuViewAlwaysOnTop, "m_menuViewAlwaysOnTop");
			this.m_menuViewAlwaysOnTop.Click += new System.EventHandler(this.OnViewAlwaysOnTop);
			// 
			// m_menuViewSep2
			// 
			this.m_menuViewSep2.Name = "m_menuViewSep2";
			resources.ApplyResources(this.m_menuViewSep2, "m_menuViewSep2");
			// 
			// m_menuViewHiding
			// 
			this.m_menuViewHiding.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuViewHideTitles,
            this.m_menuViewHideUserNames,
            this.m_menuViewHidePasswords,
            this.m_menuViewHideURLs,
            this.m_menuViewHideNotes});
			this.m_menuViewHiding.Name = "m_menuViewHiding";
			resources.ApplyResources(this.m_menuViewHiding, "m_menuViewHiding");
			// 
			// m_menuViewHideTitles
			// 
			this.m_menuViewHideTitles.CheckOnClick = true;
			this.m_menuViewHideTitles.Name = "m_menuViewHideTitles";
			resources.ApplyResources(this.m_menuViewHideTitles, "m_menuViewHideTitles");
			this.m_menuViewHideTitles.Click += new System.EventHandler(this.OnClickHideTitles);
			// 
			// m_menuViewHideUserNames
			// 
			this.m_menuViewHideUserNames.CheckOnClick = true;
			this.m_menuViewHideUserNames.Name = "m_menuViewHideUserNames";
			resources.ApplyResources(this.m_menuViewHideUserNames, "m_menuViewHideUserNames");
			this.m_menuViewHideUserNames.Click += new System.EventHandler(this.OnClickHideUserNames);
			// 
			// m_menuViewHidePasswords
			// 
			this.m_menuViewHidePasswords.CheckOnClick = true;
			this.m_menuViewHidePasswords.Name = "m_menuViewHidePasswords";
			resources.ApplyResources(this.m_menuViewHidePasswords, "m_menuViewHidePasswords");
			this.m_menuViewHidePasswords.Click += new System.EventHandler(this.OnClickHidePasswords);
			// 
			// m_menuViewHideURLs
			// 
			this.m_menuViewHideURLs.CheckOnClick = true;
			this.m_menuViewHideURLs.Name = "m_menuViewHideURLs";
			resources.ApplyResources(this.m_menuViewHideURLs, "m_menuViewHideURLs");
			this.m_menuViewHideURLs.Click += new System.EventHandler(this.OnClickHideURLs);
			// 
			// m_menuViewHideNotes
			// 
			this.m_menuViewHideNotes.CheckOnClick = true;
			this.m_menuViewHideNotes.Name = "m_menuViewHideNotes";
			resources.ApplyResources(this.m_menuViewHideNotes, "m_menuViewHideNotes");
			this.m_menuViewHideNotes.Click += new System.EventHandler(this.OnClickHideNotes);
			// 
			// m_menuViewSep3
			// 
			this.m_menuViewSep3.Name = "m_menuViewSep3";
			resources.ApplyResources(this.m_menuViewSep3, "m_menuViewSep3");
			// 
			// m_menuViewTanOptions
			// 
			this.m_menuViewTanOptions.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuViewTanSimpleList,
            this.m_menuViewTanIndices});
			this.m_menuViewTanOptions.Name = "m_menuViewTanOptions";
			resources.ApplyResources(this.m_menuViewTanOptions, "m_menuViewTanOptions");
			// 
			// m_menuViewTanSimpleList
			// 
			this.m_menuViewTanSimpleList.CheckOnClick = true;
			this.m_menuViewTanSimpleList.Name = "m_menuViewTanSimpleList";
			resources.ApplyResources(this.m_menuViewTanSimpleList, "m_menuViewTanSimpleList");
			this.m_menuViewTanSimpleList.Click += new System.EventHandler(this.OnViewTanSimpleListClick);
			// 
			// m_menuViewTanIndices
			// 
			this.m_menuViewTanIndices.CheckOnClick = true;
			this.m_menuViewTanIndices.Name = "m_menuViewTanIndices";
			resources.ApplyResources(this.m_menuViewTanIndices, "m_menuViewTanIndices");
			this.m_menuViewTanIndices.Click += new System.EventHandler(this.OnViewTanIndicesClick);
			// 
			// m_menuViewColumns
			// 
			this.m_menuViewColumns.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuViewColumnsShowTitle,
            this.m_menuViewColumnsShowUserName,
            this.m_menuViewColumnsShowPassword,
            this.m_menuViewColumnsShowUrl,
            this.m_menuViewColumnsShowNotes,
            this.m_menuViewColumnsShowSep0,
            this.m_menuViewColumnsShowCreation,
            this.m_menuViewColumnsShowLastAccess,
            this.m_menuViewColumnsShowLastMod,
            this.m_menuViewColumnsShowExpire,
            this.m_menuViewColumnsShowSep1,
            this.m_menuViewColumnsShowUuid,
            this.m_menuViewColumnsShowAttachs});
			this.m_menuViewColumns.Name = "m_menuViewColumns";
			resources.ApplyResources(this.m_menuViewColumns, "m_menuViewColumns");
			// 
			// m_menuViewColumnsShowTitle
			// 
			this.m_menuViewColumnsShowTitle.CheckOnClick = true;
			this.m_menuViewColumnsShowTitle.Name = "m_menuViewColumnsShowTitle";
			resources.ApplyResources(this.m_menuViewColumnsShowTitle, "m_menuViewColumnsShowTitle");
			this.m_menuViewColumnsShowTitle.Click += new System.EventHandler(this.OnViewShowColTitle);
			// 
			// m_menuViewColumnsShowUserName
			// 
			this.m_menuViewColumnsShowUserName.CheckOnClick = true;
			this.m_menuViewColumnsShowUserName.Name = "m_menuViewColumnsShowUserName";
			resources.ApplyResources(this.m_menuViewColumnsShowUserName, "m_menuViewColumnsShowUserName");
			this.m_menuViewColumnsShowUserName.Click += new System.EventHandler(this.OnViewShowColUser);
			// 
			// m_menuViewColumnsShowPassword
			// 
			this.m_menuViewColumnsShowPassword.CheckOnClick = true;
			this.m_menuViewColumnsShowPassword.Name = "m_menuViewColumnsShowPassword";
			resources.ApplyResources(this.m_menuViewColumnsShowPassword, "m_menuViewColumnsShowPassword");
			this.m_menuViewColumnsShowPassword.Click += new System.EventHandler(this.OnViewShowColPassword);
			// 
			// m_menuViewColumnsShowUrl
			// 
			this.m_menuViewColumnsShowUrl.CheckOnClick = true;
			this.m_menuViewColumnsShowUrl.Name = "m_menuViewColumnsShowUrl";
			resources.ApplyResources(this.m_menuViewColumnsShowUrl, "m_menuViewColumnsShowUrl");
			this.m_menuViewColumnsShowUrl.Click += new System.EventHandler(this.OnViewShowColURL);
			// 
			// m_menuViewColumnsShowNotes
			// 
			this.m_menuViewColumnsShowNotes.CheckOnClick = true;
			this.m_menuViewColumnsShowNotes.Name = "m_menuViewColumnsShowNotes";
			resources.ApplyResources(this.m_menuViewColumnsShowNotes, "m_menuViewColumnsShowNotes");
			this.m_menuViewColumnsShowNotes.Click += new System.EventHandler(this.OnViewShowColNotes);
			// 
			// m_menuViewColumnsShowSep0
			// 
			this.m_menuViewColumnsShowSep0.Name = "m_menuViewColumnsShowSep0";
			resources.ApplyResources(this.m_menuViewColumnsShowSep0, "m_menuViewColumnsShowSep0");
			// 
			// m_menuViewColumnsShowCreation
			// 
			this.m_menuViewColumnsShowCreation.CheckOnClick = true;
			this.m_menuViewColumnsShowCreation.Name = "m_menuViewColumnsShowCreation";
			resources.ApplyResources(this.m_menuViewColumnsShowCreation, "m_menuViewColumnsShowCreation");
			this.m_menuViewColumnsShowCreation.Click += new System.EventHandler(this.OnViewShowColCreation);
			// 
			// m_menuViewColumnsShowLastAccess
			// 
			this.m_menuViewColumnsShowLastAccess.CheckOnClick = true;
			this.m_menuViewColumnsShowLastAccess.Name = "m_menuViewColumnsShowLastAccess";
			resources.ApplyResources(this.m_menuViewColumnsShowLastAccess, "m_menuViewColumnsShowLastAccess");
			this.m_menuViewColumnsShowLastAccess.Click += new System.EventHandler(this.OnViewShowColLastAccess);
			// 
			// m_menuViewColumnsShowLastMod
			// 
			this.m_menuViewColumnsShowLastMod.CheckOnClick = true;
			this.m_menuViewColumnsShowLastMod.Name = "m_menuViewColumnsShowLastMod";
			resources.ApplyResources(this.m_menuViewColumnsShowLastMod, "m_menuViewColumnsShowLastMod");
			this.m_menuViewColumnsShowLastMod.Click += new System.EventHandler(this.OnViewShowColLastMod);
			// 
			// m_menuViewColumnsShowExpire
			// 
			this.m_menuViewColumnsShowExpire.CheckOnClick = true;
			this.m_menuViewColumnsShowExpire.Name = "m_menuViewColumnsShowExpire";
			resources.ApplyResources(this.m_menuViewColumnsShowExpire, "m_menuViewColumnsShowExpire");
			this.m_menuViewColumnsShowExpire.Click += new System.EventHandler(this.OnViewShowColExpire);
			// 
			// m_menuViewColumnsShowSep1
			// 
			this.m_menuViewColumnsShowSep1.Name = "m_menuViewColumnsShowSep1";
			resources.ApplyResources(this.m_menuViewColumnsShowSep1, "m_menuViewColumnsShowSep1");
			// 
			// m_menuViewColumnsShowUuid
			// 
			this.m_menuViewColumnsShowUuid.CheckOnClick = true;
			this.m_menuViewColumnsShowUuid.Name = "m_menuViewColumnsShowUuid";
			resources.ApplyResources(this.m_menuViewColumnsShowUuid, "m_menuViewColumnsShowUuid");
			this.m_menuViewColumnsShowUuid.Click += new System.EventHandler(this.OnViewShowColUUID);
			// 
			// m_menuViewColumnsShowAttachs
			// 
			this.m_menuViewColumnsShowAttachs.CheckOnClick = true;
			this.m_menuViewColumnsShowAttachs.Name = "m_menuViewColumnsShowAttachs";
			resources.ApplyResources(this.m_menuViewColumnsShowAttachs, "m_menuViewColumnsShowAttachs");
			this.m_menuViewColumnsShowAttachs.Click += new System.EventHandler(this.OnViewShowColAttachs);
			// 
			// m_menuTools
			// 
			this.m_menuTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuToolsPwGenerator,
            this.m_menuToolsGeneratePwList,
            this.m_menuToolsSep0,
            this.m_menuToolsTanWizard,
            this.m_menuToolsDbMaintenance,
            this.m_menuToolsSep1,
            this.m_menuToolsOptions,
            this.m_menuToolsPlugins});
			this.m_menuTools.Name = "m_menuTools";
			resources.ApplyResources(this.m_menuTools, "m_menuTools");
			// 
			// m_menuToolsPwGenerator
			// 
			this.m_menuToolsPwGenerator.Image = global::KeePass.Properties.Resources.B16x16_KGPG_Gen;
			this.m_menuToolsPwGenerator.Name = "m_menuToolsPwGenerator";
			resources.ApplyResources(this.m_menuToolsPwGenerator, "m_menuToolsPwGenerator");
			this.m_menuToolsPwGenerator.Click += new System.EventHandler(this.OnToolsPwGenerator);
			// 
			// m_menuToolsGeneratePwList
			// 
			this.m_menuToolsGeneratePwList.Image = global::KeePass.Properties.Resources.B16x16_KGPG_Gen;
			this.m_menuToolsGeneratePwList.Name = "m_menuToolsGeneratePwList";
			resources.ApplyResources(this.m_menuToolsGeneratePwList, "m_menuToolsGeneratePwList");
			this.m_menuToolsGeneratePwList.Click += new System.EventHandler(this.OnToolsGeneratePasswordList);
			// 
			// m_menuToolsSep0
			// 
			this.m_menuToolsSep0.Name = "m_menuToolsSep0";
			resources.ApplyResources(this.m_menuToolsSep0, "m_menuToolsSep0");
			// 
			// m_menuToolsTanWizard
			// 
			this.m_menuToolsTanWizard.Image = global::KeePass.Properties.Resources.B16x16_Wizard;
			this.m_menuToolsTanWizard.Name = "m_menuToolsTanWizard";
			resources.ApplyResources(this.m_menuToolsTanWizard, "m_menuToolsTanWizard");
			this.m_menuToolsTanWizard.Click += new System.EventHandler(this.OnToolsTANWizard);
			// 
			// m_menuToolsDbMaintenance
			// 
			this.m_menuToolsDbMaintenance.Image = global::KeePass.Properties.Resources.B16x16_Package_Settings;
			this.m_menuToolsDbMaintenance.Name = "m_menuToolsDbMaintenance";
			resources.ApplyResources(this.m_menuToolsDbMaintenance, "m_menuToolsDbMaintenance");
			this.m_menuToolsDbMaintenance.Click += new System.EventHandler(this.OnToolsDbMaintenance);
			// 
			// m_menuToolsSep1
			// 
			this.m_menuToolsSep1.Name = "m_menuToolsSep1";
			resources.ApplyResources(this.m_menuToolsSep1, "m_menuToolsSep1");
			// 
			// m_menuToolsOptions
			// 
			this.m_menuToolsOptions.Image = global::KeePass.Properties.Resources.B16x16_Misc;
			this.m_menuToolsOptions.Name = "m_menuToolsOptions";
			resources.ApplyResources(this.m_menuToolsOptions, "m_menuToolsOptions");
			this.m_menuToolsOptions.Click += new System.EventHandler(this.OnToolsOptions);
			// 
			// m_menuToolsPlugins
			// 
			this.m_menuToolsPlugins.Image = global::KeePass.Properties.Resources.B16x16_BlockDevice;
			this.m_menuToolsPlugins.Name = "m_menuToolsPlugins";
			resources.ApplyResources(this.m_menuToolsPlugins, "m_menuToolsPlugins");
			this.m_menuToolsPlugins.Click += new System.EventHandler(this.OnToolsPlugins);
			// 
			// m_menuHelp
			// 
			this.m_menuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuHelpHomepage,
            this.m_menuHelpDonate,
            this.m_menuHelpSep0,
            this.m_menuHelpContents,
            this.m_menuHelpSelectSource,
            this.m_menuHelpSep1,
            this.m_menuHelpCheckForUpdate,
            this.m_menuHelpSep2,
            this.m_menuHelpAbout});
			this.m_menuHelp.Name = "m_menuHelp";
			resources.ApplyResources(this.m_menuHelp, "m_menuHelp");
			// 
			// m_menuHelpHomepage
			// 
			this.m_menuHelpHomepage.Image = global::KeePass.Properties.Resources.B16x16_Folder_Home;
			this.m_menuHelpHomepage.Name = "m_menuHelpHomepage";
			resources.ApplyResources(this.m_menuHelpHomepage, "m_menuHelpHomepage");
			this.m_menuHelpHomepage.Click += new System.EventHandler(this.OnHelpHomepage);
			// 
			// m_menuHelpDonate
			// 
			this.m_menuHelpDonate.Image = global::KeePass.Properties.Resources.B16x16_Identity;
			this.m_menuHelpDonate.Name = "m_menuHelpDonate";
			resources.ApplyResources(this.m_menuHelpDonate, "m_menuHelpDonate");
			this.m_menuHelpDonate.Click += new System.EventHandler(this.OnHelpDonate);
			// 
			// m_menuHelpSep0
			// 
			this.m_menuHelpSep0.Name = "m_menuHelpSep0";
			resources.ApplyResources(this.m_menuHelpSep0, "m_menuHelpSep0");
			// 
			// m_menuHelpContents
			// 
			this.m_menuHelpContents.Image = global::KeePass.Properties.Resources.B16x16_Toggle_Log;
			this.m_menuHelpContents.Name = "m_menuHelpContents";
			resources.ApplyResources(this.m_menuHelpContents, "m_menuHelpContents");
			this.m_menuHelpContents.Click += new System.EventHandler(this.OnHelpContents);
			// 
			// m_menuHelpSelectSource
			// 
			this.m_menuHelpSelectSource.Image = global::KeePass.Properties.Resources.B16x16_KOrganizer;
			this.m_menuHelpSelectSource.Name = "m_menuHelpSelectSource";
			resources.ApplyResources(this.m_menuHelpSelectSource, "m_menuHelpSelectSource");
			this.m_menuHelpSelectSource.Click += new System.EventHandler(this.OnHelpSelectSource);
			// 
			// m_menuHelpSep1
			// 
			this.m_menuHelpSep1.Name = "m_menuHelpSep1";
			resources.ApplyResources(this.m_menuHelpSep1, "m_menuHelpSep1");
			// 
			// m_menuHelpCheckForUpdate
			// 
			this.m_menuHelpCheckForUpdate.Image = global::KeePass.Properties.Resources.B16x16_FTP;
			this.m_menuHelpCheckForUpdate.Name = "m_menuHelpCheckForUpdate";
			resources.ApplyResources(this.m_menuHelpCheckForUpdate, "m_menuHelpCheckForUpdate");
			this.m_menuHelpCheckForUpdate.Click += new System.EventHandler(this.OnHelpCheckForUpdate);
			// 
			// m_menuHelpSep2
			// 
			this.m_menuHelpSep2.Name = "m_menuHelpSep2";
			resources.ApplyResources(this.m_menuHelpSep2, "m_menuHelpSep2");
			// 
			// m_menuHelpAbout
			// 
			this.m_menuHelpAbout.Image = global::KeePass.Properties.Resources.B16x16_Help;
			this.m_menuHelpAbout.Name = "m_menuHelpAbout";
			resources.ApplyResources(this.m_menuHelpAbout, "m_menuHelpAbout");
			this.m_menuHelpAbout.Click += new System.EventHandler(this.OnHelpAbout);
			// 
			// m_toolMain
			// 
			this.m_toolMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tbNewDatabase,
            this.m_tbOpenDatabase,
            this.m_tbSaveDatabase,
            this.m_tbSep0,
            this.m_tbAddEntry,
            this.m_tbSep1,
            this.m_tbCopyUserName,
            this.m_tbCopyPassword,
            this.m_tbSep4,
            this.m_tbFind,
            this.m_tbEntryViewsDropDown,
            this.m_tbSep2,
            this.m_tbLockWorkspace,
            this.m_tbSep3,
            this.m_tbQuickFind});
			resources.ApplyResources(this.m_toolMain, "m_toolMain");
			this.m_toolMain.Name = "m_toolMain";
			// 
			// m_tbNewDatabase
			// 
			this.m_tbNewDatabase.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tbNewDatabase.Image = global::KeePass.Properties.Resources.B16x16_FileNew;
			resources.ApplyResources(this.m_tbNewDatabase, "m_tbNewDatabase");
			this.m_tbNewDatabase.Name = "m_tbNewDatabase";
			this.m_tbNewDatabase.Click += new System.EventHandler(this.OnFileNew);
			// 
			// m_tbOpenDatabase
			// 
			this.m_tbOpenDatabase.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tbOpenDatabase.Image = global::KeePass.Properties.Resources.B16x16_Folder_Yellow_Open;
			resources.ApplyResources(this.m_tbOpenDatabase, "m_tbOpenDatabase");
			this.m_tbOpenDatabase.Name = "m_tbOpenDatabase";
			this.m_tbOpenDatabase.Click += new System.EventHandler(this.OnFileOpen);
			// 
			// m_tbSaveDatabase
			// 
			this.m_tbSaveDatabase.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tbSaveDatabase.Image = global::KeePass.Properties.Resources.B16x16_FileSave;
			resources.ApplyResources(this.m_tbSaveDatabase, "m_tbSaveDatabase");
			this.m_tbSaveDatabase.Name = "m_tbSaveDatabase";
			this.m_tbSaveDatabase.Click += new System.EventHandler(this.OnFileSave);
			// 
			// m_tbSep0
			// 
			this.m_tbSep0.Name = "m_tbSep0";
			resources.ApplyResources(this.m_tbSep0, "m_tbSep0");
			// 
			// m_tbAddEntry
			// 
			this.m_tbAddEntry.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tbAddEntry.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tbAddEntryDefault});
			this.m_tbAddEntry.Image = global::KeePass.Properties.Resources.B16x16_KGPG_Import;
			resources.ApplyResources(this.m_tbAddEntry, "m_tbAddEntry");
			this.m_tbAddEntry.Name = "m_tbAddEntry";
			this.m_tbAddEntry.ButtonClick += new System.EventHandler(this.OnEntryAdd);
			// 
			// m_tbAddEntryDefault
			// 
			this.m_tbAddEntryDefault.Image = global::KeePass.Properties.Resources.B16x16_KGPG_Import;
			this.m_tbAddEntryDefault.Name = "m_tbAddEntryDefault";
			resources.ApplyResources(this.m_tbAddEntryDefault, "m_tbAddEntryDefault");
			this.m_tbAddEntryDefault.Click += new System.EventHandler(this.OnEntryAdd);
			// 
			// m_tbSep1
			// 
			this.m_tbSep1.Name = "m_tbSep1";
			resources.ApplyResources(this.m_tbSep1, "m_tbSep1");
			// 
			// m_tbCopyUserName
			// 
			this.m_tbCopyUserName.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tbCopyUserName.Image = global::KeePass.Properties.Resources.B16x16_Personal;
			resources.ApplyResources(this.m_tbCopyUserName, "m_tbCopyUserName");
			this.m_tbCopyUserName.Name = "m_tbCopyUserName";
			this.m_tbCopyUserName.Click += new System.EventHandler(this.OnEntryCopyUserName);
			// 
			// m_tbCopyPassword
			// 
			this.m_tbCopyPassword.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tbCopyPassword.Image = global::KeePass.Properties.Resources.B16x16_KGPG_Info;
			resources.ApplyResources(this.m_tbCopyPassword, "m_tbCopyPassword");
			this.m_tbCopyPassword.Name = "m_tbCopyPassword";
			this.m_tbCopyPassword.Click += new System.EventHandler(this.OnEntryCopyPassword);
			// 
			// m_tbSep4
			// 
			this.m_tbSep4.Name = "m_tbSep4";
			resources.ApplyResources(this.m_tbSep4, "m_tbSep4");
			// 
			// m_tbFind
			// 
			this.m_tbFind.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tbFind.Image = global::KeePass.Properties.Resources.B16x16_XMag;
			resources.ApplyResources(this.m_tbFind, "m_tbFind");
			this.m_tbFind.Name = "m_tbFind";
			this.m_tbFind.Click += new System.EventHandler(this.OnPwListFind);
			// 
			// m_tbEntryViewsDropDown
			// 
			this.m_tbEntryViewsDropDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tbEntryViewsDropDown.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tbViewsShowAll,
            this.m_tbViewsShowExpired});
			this.m_tbEntryViewsDropDown.Image = global::KeePass.Properties.Resources.B16x16_View_Detailed;
			resources.ApplyResources(this.m_tbEntryViewsDropDown, "m_tbEntryViewsDropDown");
			this.m_tbEntryViewsDropDown.Name = "m_tbEntryViewsDropDown";
			// 
			// m_tbViewsShowAll
			// 
			this.m_tbViewsShowAll.Image = global::KeePass.Properties.Resources.B16x16_KGPG_Key3;
			this.m_tbViewsShowAll.Name = "m_tbViewsShowAll";
			resources.ApplyResources(this.m_tbViewsShowAll, "m_tbViewsShowAll");
			this.m_tbViewsShowAll.Click += new System.EventHandler(this.OnShowAllEntries);
			// 
			// m_tbViewsShowExpired
			// 
			this.m_tbViewsShowExpired.Image = global::KeePass.Properties.Resources.B16x16_History_Clear;
			this.m_tbViewsShowExpired.Name = "m_tbViewsShowExpired";
			resources.ApplyResources(this.m_tbViewsShowExpired, "m_tbViewsShowExpired");
			this.m_tbViewsShowExpired.Click += new System.EventHandler(this.OnToolsShowExpired);
			// 
			// m_tbSep2
			// 
			this.m_tbSep2.Name = "m_tbSep2";
			resources.ApplyResources(this.m_tbSep2, "m_tbSep2");
			// 
			// m_tbLockWorkspace
			// 
			this.m_tbLockWorkspace.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tbLockWorkspace.Image = global::KeePass.Properties.Resources.B16x16_LockWorkspace;
			resources.ApplyResources(this.m_tbLockWorkspace, "m_tbLockWorkspace");
			this.m_tbLockWorkspace.Name = "m_tbLockWorkspace";
			this.m_tbLockWorkspace.Click += new System.EventHandler(this.OnFileLock);
			// 
			// m_tbSep3
			// 
			this.m_tbSep3.Name = "m_tbSep3";
			resources.ApplyResources(this.m_tbSep3, "m_tbSep3");
			// 
			// m_tbQuickFind
			// 
			this.m_tbQuickFind.Name = "m_tbQuickFind";
			resources.ApplyResources(this.m_tbQuickFind, "m_tbQuickFind");
			this.m_tbQuickFind.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnQuickFindKeyDown);
			this.m_tbQuickFind.SelectedIndexChanged += new System.EventHandler(this.OnQuickFindSelectedIndexChanged);
			this.m_tbQuickFind.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnQuickFindKeyUp);
			// 
			// m_statusMain
			// 
			this.m_statusMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
			this.m_statusMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_statusPartSelected,
            this.m_statusPartInfo,
            this.m_statusPartProgress,
            this.m_statusClipboard});
			resources.ApplyResources(this.m_statusMain, "m_statusMain");
			this.m_statusMain.Name = "m_statusMain";
			// 
			// m_statusPartSelected
			// 
			resources.ApplyResources(this.m_statusPartSelected, "m_statusPartSelected");
			this.m_statusPartSelected.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
			this.m_statusPartSelected.Name = "m_statusPartSelected";
			// 
			// m_statusPartInfo
			// 
			this.m_statusPartInfo.Name = "m_statusPartInfo";
			resources.ApplyResources(this.m_statusPartInfo, "m_statusPartInfo");
			this.m_statusPartInfo.Spring = true;
			// 
			// m_statusPartProgress
			// 
			resources.ApplyResources(this.m_statusPartProgress, "m_statusPartProgress");
			this.m_statusPartProgress.Name = "m_statusPartProgress";
			this.m_statusPartProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			// 
			// m_statusClipboard
			// 
			this.m_statusClipboard.Name = "m_statusClipboard";
			resources.ApplyResources(this.m_statusClipboard, "m_statusClipboard");
			this.m_statusClipboard.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			// 
			// m_openDatabaseFile
			// 
			this.m_openDatabaseFile.FileName = "*.kdbx";
			resources.ApplyResources(this.m_openDatabaseFile, "m_openDatabaseFile");
			this.m_openDatabaseFile.SupportMultiDottedExtensions = true;
			// 
			// m_saveDatabaseFile
			// 
			resources.ApplyResources(this.m_saveDatabaseFile, "m_saveDatabaseFile");
			this.m_saveDatabaseFile.SupportMultiDottedExtensions = true;
			// 
			// m_saveExportTo
			// 
			resources.ApplyResources(this.m_saveExportTo, "m_saveExportTo");
			this.m_saveExportTo.SupportMultiDottedExtensions = true;
			// 
			// m_openImportFile
			// 
			this.m_openImportFile.FileName = "*.*";
			resources.ApplyResources(this.m_openImportFile, "m_openImportFile");
			this.m_openImportFile.SupportMultiDottedExtensions = true;
			// 
			// m_ntfTray
			// 
			this.m_ntfTray.ContextMenuStrip = this.m_ctxTray;
			resources.ApplyResources(this.m_ntfTray, "m_ntfTray");
			this.m_ntfTray.Click += new System.EventHandler(this.OnSystemTrayClick);
			this.m_ntfTray.DoubleClick += new System.EventHandler(this.OnSystemTrayDoubleClick);
			// 
			// m_ctxTray
			// 
			this.m_ctxTray.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_ctxTrayTray});
			this.m_ctxTray.Name = "m_ctxTray";
			resources.ApplyResources(this.m_ctxTray, "m_ctxTray");
			// 
			// m_ctxTrayTray
			// 
			this.m_ctxTrayTray.Name = "m_ctxTrayTray";
			resources.ApplyResources(this.m_ctxTrayTray, "m_ctxTrayTray");
			this.m_ctxTrayTray.Click += new System.EventHandler(this.OnTrayTray);
			// 
			// m_timerMain
			// 
			this.m_timerMain.Enabled = true;
			this.m_timerMain.Interval = 1000;
			this.m_timerMain.Tick += new System.EventHandler(this.OnTimerMainTick);
			// 
			// m_folderSaveAttachments
			// 
			resources.ApplyResources(this.m_folderSaveAttachments, "m_folderSaveAttachments");
			// 
			// m_openXslFile
			// 
			this.m_openXslFile.DefaultExt = "xsl";
			resources.ApplyResources(this.m_openXslFile, "m_openXslFile");
			this.m_openXslFile.RestoreDirectory = true;
			// 
			// m_colorDlg
			// 
			this.m_colorDlg.AnyColor = true;
			this.m_colorDlg.FullOpen = true;
			// 
			// m_splitHorizontal
			// 
			resources.ApplyResources(this.m_splitHorizontal, "m_splitHorizontal");
			this.m_splitHorizontal.Name = "m_splitHorizontal";
			// 
			// m_splitHorizontal.Panel1
			// 
			this.m_splitHorizontal.Panel1.Controls.Add(this.m_splitVertical);
			// 
			// m_splitHorizontal.Panel2
			// 
			this.m_splitHorizontal.Panel2.Controls.Add(this.m_richEntryView);
			this.m_splitHorizontal.TabStop = false;
			// 
			// m_splitVertical
			// 
			resources.ApplyResources(this.m_splitVertical, "m_splitVertical");
			this.m_splitVertical.Name = "m_splitVertical";
			// 
			// m_splitVertical.Panel1
			// 
			this.m_splitVertical.Panel1.Controls.Add(this.m_tvGroups);
			// 
			// m_splitVertical.Panel2
			// 
			this.m_splitVertical.Panel2.Controls.Add(this.m_lvEntries);
			this.m_splitVertical.TabStop = false;
			// 
			// m_tvGroups
			// 
			this.m_tvGroups.AllowDrop = true;
			this.m_tvGroups.ContextMenuStrip = this.m_ctxGroupList;
			resources.ApplyResources(this.m_tvGroups, "m_tvGroups");
			this.m_tvGroups.HideSelection = false;
			this.m_tvGroups.HotTracking = true;
			this.m_tvGroups.ImageList = this.m_ilClientIcons;
			this.m_tvGroups.LabelEdit = true;
			this.m_tvGroups.Name = "m_tvGroups";
			this.m_tvGroups.ShowNodeToolTips = true;
			this.m_tvGroups.ShowRootLines = false;
			this.m_tvGroups.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.OnGroupsAfterCollapse);
			this.m_tvGroups.DragDrop += new System.Windows.Forms.DragEventHandler(this.OnGroupsListDragDrop);
			this.m_tvGroups.DragOver += new System.Windows.Forms.DragEventHandler(this.OnGroupsListDragOver);
			this.m_tvGroups.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.OnGroupsAfterLabelEdit);
			this.m_tvGroups.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.OnGroupListClickNode);
			this.m_tvGroups.DragEnter += new System.Windows.Forms.DragEventHandler(this.OnGroupsListDragEnter);
			this.m_tvGroups.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.OnGroupsAfterExpand);
			this.m_tvGroups.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.OnGroupsListItemDrag);
			// 
			// m_lvEntries
			// 
			this.m_lvEntries.AllowColumnReorder = true;
			this.m_lvEntries.ContextMenuStrip = this.m_ctxPwList;
			resources.ApplyResources(this.m_lvEntries, "m_lvEntries");
			this.m_lvEntries.FullRowSelect = true;
			this.m_lvEntries.GridLines = true;
			this.m_lvEntries.HideSelection = false;
			this.m_lvEntries.Name = "m_lvEntries";
			this.m_lvEntries.ShowItemToolTips = true;
			this.m_lvEntries.SmallImageList = this.m_ilClientIcons;
			this.m_lvEntries.UseCompatibleStateImageBehavior = false;
			this.m_lvEntries.View = System.Windows.Forms.View.Details;
			this.m_lvEntries.ColumnWidthChanged += new System.Windows.Forms.ColumnWidthChangedEventHandler(this.OnPwListColumnWidthChanged);
			this.m_lvEntries.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.OnPwListMouseDoubleClick);
			this.m_lvEntries.SelectedIndexChanged += new System.EventHandler(this.OnPwListSelectedIndexChanged);
			this.m_lvEntries.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnPwListKeyDown);
			this.m_lvEntries.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.OnPwListColumnClick);
			this.m_lvEntries.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.OnPwListItemDrag);
			this.m_lvEntries.ColumnWidthChanging += new System.Windows.Forms.ColumnWidthChangingEventHandler(this.OnPwListColumnWidthChanging);
			this.m_lvEntries.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnPwListMouseDown);
			// 
			// m_richEntryView
			// 
			resources.ApplyResources(this.m_richEntryView, "m_richEntryView");
			this.m_richEntryView.Name = "m_richEntryView";
			this.m_richEntryView.ReadOnly = true;
			this.m_richEntryView.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.OnEntryViewLinkClicked);
			// 
			// MainForm
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.m_splitHorizontal);
			this.Controls.Add(this.m_statusMain);
			this.Controls.Add(this.m_toolMain);
			this.Controls.Add(this.m_menuMain);
			this.MainMenuStrip = this.m_menuMain;
			this.Name = "MainForm";
			this.Resize += new System.EventHandler(this.OnFormResize);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
			this.Load += new System.EventHandler(this.OnFormLoad);
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

		private System.Windows.Forms.MenuStrip m_menuMain;
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
		private System.Windows.Forms.ToolStripMenuItem m_menuEdit;
		private System.Windows.Forms.ToolStripMenuItem m_menuView;
		private System.Windows.Forms.ToolStripMenuItem m_menuTools;
		private System.Windows.Forms.ToolStripMenuItem m_menuHelp;
		private System.Windows.Forms.ToolStripMenuItem m_menuHelpHomepage;
		private System.Windows.Forms.ToolStripMenuItem m_menuHelpDonate;
		private System.Windows.Forms.ToolStripSeparator m_menuHelpSep0;
		private System.Windows.Forms.ToolStripMenuItem m_menuHelpContents;
		private System.Windows.Forms.ToolStripSeparator m_menuHelpSep1;
		private System.Windows.Forms.ToolStripMenuItem m_menuHelpCheckForUpdate;
		private System.Windows.Forms.ToolStripSeparator m_menuHelpSep2;
		private System.Windows.Forms.ToolStripMenuItem m_menuHelpAbout;
		private System.Windows.Forms.ToolStrip m_toolMain;
		private System.Windows.Forms.ToolStripButton m_tbNewDatabase;
		private System.Windows.Forms.ToolStripButton m_tbOpenDatabase;
		private System.Windows.Forms.RichTextBox m_richEntryView;
		private KeePass.UI.CustomSplitContainerEx m_splitHorizontal;
		private KeePass.UI.CustomSplitContainerEx m_splitVertical;
		private System.Windows.Forms.TreeView m_tvGroups;
		private System.Windows.Forms.StatusStrip m_statusMain;
		private System.Windows.Forms.ToolStripStatusLabel m_statusPartSelected;
		private System.Windows.Forms.ToolStripStatusLabel m_statusPartInfo;
		private System.Windows.Forms.ListView m_lvEntries;
		private System.Windows.Forms.OpenFileDialog m_openDatabaseFile;
		private System.Windows.Forms.ToolStripButton m_tbSaveDatabase;
		private System.Windows.Forms.ContextMenuStrip m_ctxPwList;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryCopyUserName;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryCopyPassword;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntrySaveAttachedFiles;
		private System.Windows.Forms.ToolStripSeparator m_ctxEntrySep0;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryPerformAutoType;
		private System.Windows.Forms.ToolStripSeparator m_ctxEntrySep1;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryAdd;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryEdit;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryDuplicate;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryDelete;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryMassModify;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntrySelectAll;
		private System.Windows.Forms.ToolStripSeparator m_ctxEntrySep2;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryRearrangePopup;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryMoveToTop;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryMoveOneUp;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryMoveOneDown;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryMoveToBottom;
		private System.Windows.Forms.ToolStripSeparator m_ctxEntryRearrangeSep0;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntrySortListByTitle;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntrySortListByUserName;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntrySortListByPassword;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntrySortListByURL;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntrySortListByNotes;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntrySortListByCreationTime;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntrySortListByLastModTime;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntrySortListByLastAccessTime;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntrySortListByExpirationTime;
		private System.Windows.Forms.ImageList m_ilClientIcons;
		private System.Windows.Forms.ToolStripMenuItem m_menuChangeLanguage;
		private System.Windows.Forms.SaveFileDialog m_saveDatabaseFile;
		private System.Windows.Forms.ToolStripSeparator m_tbSep0;
		private System.Windows.Forms.ToolStripMenuItem m_menuEditFind;
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
		private System.Windows.Forms.ToolStripMenuItem m_menuViewHiding;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewHideTitles;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewHideUserNames;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewHidePasswords;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewHideURLs;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewHideNotes;
		private System.Windows.Forms.ToolStripSeparator m_menuViewSep1;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileExportXML;
		private System.Windows.Forms.SaveFileDialog m_saveExportTo;
		private System.Windows.Forms.OpenFileDialog m_openImportFile;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileSynchronize;
		private System.Windows.Forms.ContextMenuStrip m_ctxGroupList;
		private System.Windows.Forms.ToolStripMenuItem m_ctxGroupAdd;
		private System.Windows.Forms.ToolStripSeparator m_ctxGroupSep0;
		private System.Windows.Forms.ToolStripMenuItem m_ctxGroupEdit;
		private System.Windows.Forms.ToolStripMenuItem m_ctxGroupDelete;
		private System.Windows.Forms.ToolStripProgressBar m_statusPartProgress;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileExportHtml;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewAlwaysOnTop;
		private System.Windows.Forms.ToolStripSeparator m_menuViewSep2;
		private System.Windows.Forms.ToolStripSeparator m_ctxGroupSep1;
		private System.Windows.Forms.ToolStripMenuItem m_ctxGroupPrint;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewColumns;
		private System.Windows.Forms.ToolStripSeparator m_menuViewSep3;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewColumnsShowTitle;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewColumnsShowUserName;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewColumnsShowPassword;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewColumnsShowUrl;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewColumnsShowNotes;
		private System.Windows.Forms.ToolStripSeparator m_menuViewColumnsShowSep0;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewColumnsShowCreation;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewColumnsShowLastAccess;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewColumnsShowLastMod;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewColumnsShowExpire;
		private System.Windows.Forms.ToolStripSeparator m_menuViewColumnsShowSep1;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewColumnsShowUuid;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewColumnsShowAttachs;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileRecent;
		private System.Windows.Forms.NotifyIcon m_ntfTray;
		private System.Windows.Forms.ContextMenuStrip m_ctxTray;
		private System.Windows.Forms.ToolStripMenuItem m_ctxTrayTray;
		private System.Windows.Forms.Timer m_timerMain;
		private System.Windows.Forms.ToolStripMenuItem m_menuToolsPlugins;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryUrl;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryOpenUrl;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryCopyUrl;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryMassSetIcon;
		private System.Windows.Forms.FolderBrowserDialog m_folderSaveAttachments;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntrySortUnsorted;
		private System.Windows.Forms.ToolStripSeparator m_ctxEntryRearrangeSep1;
		private System.Windows.Forms.ToolStripMenuItem m_ctxGroupFind;
		private System.Windows.Forms.ToolStripSeparator m_ctxGroupSep2;
		private System.Windows.Forms.ToolStripSeparator m_menuFileExportSep0;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileExportKdb3;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewTanOptions;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewTanSimpleList;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewTanIndices;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileExportUseXsl;
		private System.Windows.Forms.ToolStripSeparator m_menuFileExportSep1;
		private System.Windows.Forms.OpenFileDialog m_openXslFile;
		private System.Windows.Forms.ToolStripMenuItem m_menuToolsPwGenerator;
		private System.Windows.Forms.ToolStripSeparator m_menuToolsSep0;
		private System.Windows.Forms.ToolStripMenuItem m_menuToolsTanWizard;
		private System.Windows.Forms.ToolStripMenuItem m_menuEditShowAllEntries;
		private System.Windows.Forms.ToolStripMenuItem m_menuEditShowExpired;
		private System.Windows.Forms.ToolStripSeparator m_menuEditSep0;
		private System.Windows.Forms.ToolStripProgressBar m_statusClipboard;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryClipboard;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryClipCopy;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryClipPaste;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntrySetColor;
		private System.Windows.Forms.ToolStripSeparator m_ctxEntryMMSep0;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryColorStandard;
		private System.Windows.Forms.ToolStripSeparator m_ctxEntryColorSep0;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryColorLightRed;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryColorLightGreen;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryColorLightBlue;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryColorLightYellow;
		private System.Windows.Forms.ToolStripSeparator m_ctxEntryColorSep1;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryColorCustom;
		private System.Windows.Forms.ColorDialog m_colorDlg;
		private System.Windows.Forms.ToolStripDropDownButton m_tbEntryViewsDropDown;
		private System.Windows.Forms.ToolStripMenuItem m_tbViewsShowAll;
		private System.Windows.Forms.ToolStripMenuItem m_tbViewsShowExpired;
		private System.Windows.Forms.ToolStripSplitButton m_tbAddEntry;
		private System.Windows.Forms.ToolStripMenuItem m_tbAddEntryDefault;
		private System.Windows.Forms.ToolStripMenuItem m_menuToolsDbMaintenance;
		private System.Windows.Forms.ToolStripMenuItem m_ctxEntryCopyCustomString;
		private System.Windows.Forms.ToolStripMenuItem m_menuToolsGeneratePwList;
		private System.Windows.Forms.ToolStripSeparator m_menuToolsSep1;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewWindowLayout;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewWindowsStacked;
		private System.Windows.Forms.ToolStripMenuItem m_menuViewWindowsSideBySide;
		private System.Windows.Forms.ToolStripButton m_tbCopyUserName;
		private System.Windows.Forms.ToolStripButton m_tbCopyPassword;
		private System.Windows.Forms.ToolStripSeparator m_tbSep4;
		private System.Windows.Forms.ToolStripMenuItem m_menuHelpSelectSource;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileOpen;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileOpenLocal;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileOpenUrl;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileSaveAs;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileSaveAsLocal;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileSaveAsUrl;
		private System.Windows.Forms.ToolStripMenuItem m_menuFileImport;
	}
}

