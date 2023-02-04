namespace KeePass.Forms
{
	partial class PwEntryForm
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
			this.m_lblUserName = new System.Windows.Forms.Label();
			this.m_lblPassword = new System.Windows.Forms.Label();
			this.m_lblTitle = new System.Windows.Forms.Label();
			this.m_lblPasswordRepeat = new System.Windows.Forms.Label();
			this.m_lblUrl = new System.Windows.Forms.Label();
			this.m_lblNotes = new System.Windows.Forms.Label();
			this.m_lblQuality = new System.Windows.Forms.Label();
			this.m_tbTitle = new System.Windows.Forms.TextBox();
			this.m_btnIcon = new System.Windows.Forms.Button();
			this.m_lblIcon = new System.Windows.Forms.Label();
			this.m_tbUserName = new System.Windows.Forms.TextBox();
			this.m_tbUrl = new System.Windows.Forms.TextBox();
			this.m_cbExpires = new System.Windows.Forms.CheckBox();
			this.m_dtExpireDateTime = new System.Windows.Forms.DateTimePicker();
			this.m_btnOK = new System.Windows.Forms.Button();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_lblQualityInfo = new System.Windows.Forms.Label();
			this.m_ttRect = new System.Windows.Forms.ToolTip(this.components);
			this.m_btnGenPw = new System.Windows.Forms.Button();
			this.m_cbHidePassword = new System.Windows.Forms.CheckBox();
			this.m_lblSeparator = new System.Windows.Forms.Label();
			this.m_ttBalloon = new System.Windows.Forms.ToolTip(this.components);
			this.m_tabMain = new System.Windows.Forms.TabControl();
			this.m_tabEntry = new System.Windows.Forms.TabPage();
			this.m_cbQualityCheck = new System.Windows.Forms.CheckBox();
			this.m_rtNotes = new KeePass.UI.CustomRichTextBoxEx();
			this.m_pbQuality = new KeePass.UI.QualityProgressBar();
			this.m_tbRepeatPassword = new KeePass.UI.SecureTextBoxEx();
			this.m_tbPassword = new KeePass.UI.SecureTextBoxEx();
			this.m_btnStandardExpires = new System.Windows.Forms.Button();
			this.m_ctxDefaultTimes = new KeePass.UI.CustomContextMenuStripEx(this.components);
			this.m_menuExpireNow = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuExpireSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuExpire1Week = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuExpire2Weeks = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuExpireSep1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuExpire1Month = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuExpire3Months = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuExpire6Months = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuExpireSep2 = new System.Windows.Forms.ToolStripSeparator();
			this.m_menuExpire1Year = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tabAdvanced = new System.Windows.Forms.TabPage();
			this.m_grpAttachments = new System.Windows.Forms.GroupBox();
			this.m_btnBinOpen = new KeePass.UI.SplitButtonEx();
			this.m_btnBinSave = new System.Windows.Forms.Button();
			this.m_btnBinDelete = new System.Windows.Forms.Button();
			this.m_btnBinAdd = new System.Windows.Forms.Button();
			this.m_lvBinaries = new KeePass.UI.CustomListViewEx();
			this.m_grpStringFields = new System.Windows.Forms.GroupBox();
			this.m_btnStrMore = new System.Windows.Forms.Button();
			this.m_btnStrAdd = new System.Windows.Forms.Button();
			this.m_btnStrEdit = new System.Windows.Forms.Button();
			this.m_btnStrDelete = new System.Windows.Forms.Button();
			this.m_lvStrings = new KeePass.UI.CustomListViewEx();
			this.m_ctxStr = new KeePass.UI.CustomContextMenuStripEx(this.components);
			this.m_ctxStrCopyName = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxStrCopyValue = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxStrSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_ctxStrCopyItem = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxStrPasteItem = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxStrSep1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_ctxStrSelectAll = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxStrSep2 = new System.Windows.Forms.ToolStripSeparator();
			this.m_ctxStrMoveTo = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxStrMoveToTitle = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxStrMoveToUserName = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxStrMoveToPassword = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxStrMoveToUrl = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxStrMoveToNotes = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxStrSep3 = new System.Windows.Forms.ToolStripSeparator();
			this.m_ctxStrOtpGen = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tabProperties = new System.Windows.Forms.TabPage();
			this.m_btnTags = new System.Windows.Forms.Button();
			this.m_linkTagsInh = new System.Windows.Forms.LinkLabel();
			this.m_lblCustomData = new System.Windows.Forms.Label();
			this.m_btnCDDel = new System.Windows.Forms.Button();
			this.m_lvCustomData = new KeePass.UI.CustomListViewEx();
			this.m_tbTags = new System.Windows.Forms.TextBox();
			this.m_lblTags = new System.Windows.Forms.Label();
			this.m_btnPickFgColor = new KeePass.UI.ColorButtonEx();
			this.m_cbCustomForegroundColor = new System.Windows.Forms.CheckBox();
			this.m_tbUuid = new System.Windows.Forms.TextBox();
			this.m_lblUuid = new System.Windows.Forms.Label();
			this.m_lblOverrideUrl = new System.Windows.Forms.Label();
			this.m_cbCustomBackgroundColor = new System.Windows.Forms.CheckBox();
			this.m_btnPickBgColor = new KeePass.UI.ColorButtonEx();
			this.m_cmbOverrideUrl = new KeePass.UI.ImageComboBoxEx();
			this.m_tabAutoType = new System.Windows.Forms.TabPage();
			this.m_btnAutoTypeMore = new System.Windows.Forms.Button();
			this.m_btnAutoTypeDown = new System.Windows.Forms.Button();
			this.m_btnAutoTypeUp = new System.Windows.Forms.Button();
			this.m_linkAutoTypeObfuscation = new System.Windows.Forms.LinkLabel();
			this.m_cbAutoTypeObfuscation = new System.Windows.Forms.CheckBox();
			this.m_btnAutoTypeEditDefault = new System.Windows.Forms.Button();
			this.m_rbAutoTypeOverride = new System.Windows.Forms.RadioButton();
			this.m_rbAutoTypeSeqInherit = new System.Windows.Forms.RadioButton();
			this.m_lblCustomAutoType = new System.Windows.Forms.Label();
			this.m_cbAutoTypeEnabled = new System.Windows.Forms.CheckBox();
			this.m_tbDefaultAutoTypeSeq = new System.Windows.Forms.TextBox();
			this.m_btnAutoTypeEdit = new System.Windows.Forms.Button();
			this.m_btnAutoTypeAdd = new System.Windows.Forms.Button();
			this.m_btnAutoTypeDelete = new System.Windows.Forms.Button();
			this.m_lvAutoType = new KeePass.UI.CustomListViewEx();
			this.m_ctxAutoType = new KeePass.UI.CustomContextMenuStripEx(this.components);
			this.m_ctxAutoTypeCopyWnd = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxAutoTypeCopySeq = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxAutoTypeSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_ctxAutoTypeCopyItem = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxAutoTypePasteItem = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxAutoTypeDup = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxAutoTypeSep1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_ctxAutoTypeSelectAll = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tabHistory = new System.Windows.Forms.TabPage();
			this.m_btnHistoryCompare = new System.Windows.Forms.Button();
			this.m_lblPasswordModifiedData = new System.Windows.Forms.Label();
			this.m_lblPasswordModified = new System.Windows.Forms.Label();
			this.m_lblVersions = new System.Windows.Forms.Label();
			this.m_lblModifiedData = new System.Windows.Forms.Label();
			this.m_lblModified = new System.Windows.Forms.Label();
			this.m_lblCreatedData = new System.Windows.Forms.Label();
			this.m_lblCreated = new System.Windows.Forms.Label();
			this.m_btnHistoryDelete = new System.Windows.Forms.Button();
			this.m_btnHistoryView = new System.Windows.Forms.Button();
			this.m_btnHistoryRestore = new System.Windows.Forms.Button();
			this.m_lvHistory = new KeePass.UI.CustomListViewEx();
			this.m_btnTools = new System.Windows.Forms.Button();
			this.m_bannerImage = new System.Windows.Forms.PictureBox();
			this.m_ctxTools = new KeePass.UI.CustomContextMenuStripEx(this.components);
			this.m_ctxToolsHelp = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxToolsSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_ctxToolsCopyInitialPassword = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxToolsSep1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_ctxToolsUrlHelp = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxToolsUrlSelApp = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxToolsUrlSelDoc = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxToolsSep2 = new System.Windows.Forms.ToolStripSeparator();
			this.m_ctxToolsFieldRefs = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxToolsFieldRefsInTitle = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxToolsFieldRefsInUserName = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxToolsFieldRefsInPassword = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxToolsFieldRefsInUrl = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxToolsFieldRefsInNotes = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxToolsSep3 = new System.Windows.Forms.ToolStripSeparator();
			this.m_ctxToolsOtpGen = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxBinAttach = new KeePass.UI.CustomContextMenuStripEx(this.components);
			this.m_ctxBinImportFile = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxBinSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_ctxBinNew = new System.Windows.Forms.ToolStripMenuItem();
			this.m_tabMain.SuspendLayout();
			this.m_tabEntry.SuspendLayout();
			this.m_ctxDefaultTimes.SuspendLayout();
			this.m_tabAdvanced.SuspendLayout();
			this.m_grpAttachments.SuspendLayout();
			this.m_grpStringFields.SuspendLayout();
			this.m_ctxStr.SuspendLayout();
			this.m_tabProperties.SuspendLayout();
			this.m_tabAutoType.SuspendLayout();
			this.m_ctxAutoType.SuspendLayout();
			this.m_tabHistory.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
			this.m_ctxTools.SuspendLayout();
			this.m_ctxBinAttach.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_lblUserName
			// 
			this.m_lblUserName.AutoSize = true;
			this.m_lblUserName.Location = new System.Drawing.Point(6, 40);
			this.m_lblUserName.Name = "m_lblUserName";
			this.m_lblUserName.Size = new System.Drawing.Size(61, 13);
			this.m_lblUserName.TabIndex = 4;
			this.m_lblUserName.Text = "&User name:";
			// 
			// m_lblPassword
			// 
			this.m_lblPassword.AutoSize = true;
			this.m_lblPassword.Location = new System.Drawing.Point(6, 67);
			this.m_lblPassword.Name = "m_lblPassword";
			this.m_lblPassword.Size = new System.Drawing.Size(56, 13);
			this.m_lblPassword.TabIndex = 6;
			this.m_lblPassword.Text = "&Password:";
			// 
			// m_lblTitle
			// 
			this.m_lblTitle.AutoSize = true;
			this.m_lblTitle.Location = new System.Drawing.Point(6, 13);
			this.m_lblTitle.Name = "m_lblTitle";
			this.m_lblTitle.Size = new System.Drawing.Size(30, 13);
			this.m_lblTitle.TabIndex = 0;
			this.m_lblTitle.Text = "&Title:";
			// 
			// m_lblPasswordRepeat
			// 
			this.m_lblPasswordRepeat.AutoSize = true;
			this.m_lblPasswordRepeat.Location = new System.Drawing.Point(6, 94);
			this.m_lblPasswordRepeat.Name = "m_lblPasswordRepeat";
			this.m_lblPasswordRepeat.Size = new System.Drawing.Size(45, 13);
			this.m_lblPasswordRepeat.TabIndex = 9;
			this.m_lblPasswordRepeat.Text = "&Repeat:";
			// 
			// m_lblUrl
			// 
			this.m_lblUrl.AutoSize = true;
			this.m_lblUrl.Location = new System.Drawing.Point(6, 144);
			this.m_lblUrl.Name = "m_lblUrl";
			this.m_lblUrl.Size = new System.Drawing.Size(32, 13);
			this.m_lblUrl.TabIndex = 16;
			this.m_lblUrl.Text = "UR&L:";
			// 
			// m_lblNotes
			// 
			this.m_lblNotes.AutoSize = true;
			this.m_lblNotes.Location = new System.Drawing.Point(6, 171);
			this.m_lblNotes.Name = "m_lblNotes";
			this.m_lblNotes.Size = new System.Drawing.Size(38, 13);
			this.m_lblNotes.TabIndex = 18;
			this.m_lblNotes.Text = "&Notes:";
			// 
			// m_lblQuality
			// 
			this.m_lblQuality.AutoSize = true;
			this.m_lblQuality.Location = new System.Drawing.Point(6, 119);
			this.m_lblQuality.Name = "m_lblQuality";
			this.m_lblQuality.Size = new System.Drawing.Size(42, 13);
			this.m_lblQuality.TabIndex = 12;
			this.m_lblQuality.Text = "Quality:";
			// 
			// m_tbTitle
			// 
			this.m_tbTitle.Location = new System.Drawing.Point(81, 10);
			this.m_tbTitle.Name = "m_tbTitle";
			this.m_tbTitle.Size = new System.Drawing.Size(294, 20);
			this.m_tbTitle.TabIndex = 1;
			// 
			// m_btnIcon
			// 
			this.m_btnIcon.Location = new System.Drawing.Point(424, 8);
			this.m_btnIcon.Name = "m_btnIcon";
			this.m_btnIcon.Size = new System.Drawing.Size(32, 23);
			this.m_btnIcon.TabIndex = 3;
			this.m_btnIcon.UseVisualStyleBackColor = true;
			this.m_btnIcon.Click += new System.EventHandler(this.OnBtnPickIcon);
			// 
			// m_lblIcon
			// 
			this.m_lblIcon.AutoSize = true;
			this.m_lblIcon.Location = new System.Drawing.Point(387, 13);
			this.m_lblIcon.Name = "m_lblIcon";
			this.m_lblIcon.Size = new System.Drawing.Size(31, 13);
			this.m_lblIcon.TabIndex = 2;
			this.m_lblIcon.Text = "&Icon:";
			// 
			// m_tbUserName
			// 
			this.m_tbUserName.Location = new System.Drawing.Point(81, 37);
			this.m_tbUserName.Name = "m_tbUserName";
			this.m_tbUserName.Size = new System.Drawing.Size(374, 20);
			this.m_tbUserName.TabIndex = 5;
			// 
			// m_tbUrl
			// 
			this.m_tbUrl.Location = new System.Drawing.Point(81, 141);
			this.m_tbUrl.Name = "m_tbUrl";
			this.m_tbUrl.Size = new System.Drawing.Size(374, 20);
			this.m_tbUrl.TabIndex = 17;
			this.m_tbUrl.TextChanged += new System.EventHandler(this.OnUrlTextChanged);
			// 
			// m_cbExpires
			// 
			this.m_cbExpires.AutoSize = true;
			this.m_cbExpires.Location = new System.Drawing.Point(9, 315);
			this.m_cbExpires.Name = "m_cbExpires";
			this.m_cbExpires.Size = new System.Drawing.Size(63, 17);
			this.m_cbExpires.TabIndex = 20;
			this.m_cbExpires.Text = "&Expires:";
			this.m_cbExpires.UseVisualStyleBackColor = true;
			// 
			// m_dtExpireDateTime
			// 
			this.m_dtExpireDateTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.m_dtExpireDateTime.Location = new System.Drawing.Point(81, 313);
			this.m_dtExpireDateTime.Name = "m_dtExpireDateTime";
			this.m_dtExpireDateTime.Size = new System.Drawing.Size(336, 20);
			this.m_dtExpireDateTime.TabIndex = 21;
			// 
			// m_btnOK
			// 
			this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_btnOK.Location = new System.Drawing.Point(313, 453);
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.Size = new System.Drawing.Size(80, 23);
			this.m_btnOK.TabIndex = 0;
			this.m_btnOK.Text = "OK";
			this.m_btnOK.UseVisualStyleBackColor = true;
			this.m_btnOK.Click += new System.EventHandler(this.OnBtnOK);
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_btnCancel.Location = new System.Drawing.Point(399, 453);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(80, 23);
			this.m_btnCancel.TabIndex = 1;
			this.m_btnCancel.Text = "Cancel";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			this.m_btnCancel.Click += new System.EventHandler(this.OnBtnCancel);
			// 
			// m_lblQualityInfo
			// 
			this.m_lblQualityInfo.Location = new System.Drawing.Point(371, 119);
			this.m_lblQualityInfo.Name = "m_lblQualityInfo";
			this.m_lblQualityInfo.Size = new System.Drawing.Size(50, 13);
			this.m_lblQualityInfo.TabIndex = 14;
			this.m_lblQualityInfo.Text = "0 ch.";
			this.m_lblQualityInfo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// m_btnGenPw
			// 
			this.m_btnGenPw.Location = new System.Drawing.Point(424, 89);
			this.m_btnGenPw.Name = "m_btnGenPw";
			this.m_btnGenPw.Size = new System.Drawing.Size(32, 23);
			this.m_btnGenPw.TabIndex = 11;
			this.m_btnGenPw.UseVisualStyleBackColor = true;
			// 
			// m_cbHidePassword
			// 
			this.m_cbHidePassword.Appearance = System.Windows.Forms.Appearance.Button;
			this.m_cbHidePassword.Location = new System.Drawing.Point(424, 62);
			this.m_cbHidePassword.Name = "m_cbHidePassword";
			this.m_cbHidePassword.Size = new System.Drawing.Size(32, 23);
			this.m_cbHidePassword.TabIndex = 8;
			this.m_cbHidePassword.Text = "***";
			this.m_cbHidePassword.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.m_cbHidePassword.UseVisualStyleBackColor = true;
			// 
			// m_lblSeparator
			// 
			this.m_lblSeparator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.m_lblSeparator.Location = new System.Drawing.Point(0, 445);
			this.m_lblSeparator.Name = "m_lblSeparator";
			this.m_lblSeparator.Size = new System.Drawing.Size(489, 2);
			this.m_lblSeparator.TabIndex = 3;
			// 
			// m_ttBalloon
			// 
			this.m_ttBalloon.IsBalloon = true;
			// 
			// m_tabMain
			// 
			this.m_tabMain.Controls.Add(this.m_tabEntry);
			this.m_tabMain.Controls.Add(this.m_tabAdvanced);
			this.m_tabMain.Controls.Add(this.m_tabProperties);
			this.m_tabMain.Controls.Add(this.m_tabAutoType);
			this.m_tabMain.Controls.Add(this.m_tabHistory);
			this.m_tabMain.Location = new System.Drawing.Point(8, 66);
			this.m_tabMain.Name = "m_tabMain";
			this.m_tabMain.SelectedIndex = 0;
			this.m_tabMain.Size = new System.Drawing.Size(475, 368);
			this.m_tabMain.TabIndex = 2;
			this.m_tabMain.SelectedIndexChanged += new System.EventHandler(this.OnTabMainSelectedIndexChanged);
			// 
			// m_tabEntry
			// 
			this.m_tabEntry.Controls.Add(this.m_cbQualityCheck);
			this.m_tabEntry.Controls.Add(this.m_lblTitle);
			this.m_tabEntry.Controls.Add(this.m_rtNotes);
			this.m_tabEntry.Controls.Add(this.m_cbExpires);
			this.m_tabEntry.Controls.Add(this.m_lblQualityInfo);
			this.m_tabEntry.Controls.Add(this.m_tbUrl);
			this.m_tabEntry.Controls.Add(this.m_btnGenPw);
			this.m_tabEntry.Controls.Add(this.m_cbHidePassword);
			this.m_tabEntry.Controls.Add(this.m_pbQuality);
			this.m_tabEntry.Controls.Add(this.m_lblIcon);
			this.m_tabEntry.Controls.Add(this.m_btnIcon);
			this.m_tabEntry.Controls.Add(this.m_tbRepeatPassword);
			this.m_tabEntry.Controls.Add(this.m_tbPassword);
			this.m_tabEntry.Controls.Add(this.m_lblNotes);
			this.m_tabEntry.Controls.Add(this.m_dtExpireDateTime);
			this.m_tabEntry.Controls.Add(this.m_btnStandardExpires);
			this.m_tabEntry.Controls.Add(this.m_tbTitle);
			this.m_tabEntry.Controls.Add(this.m_lblUrl);
			this.m_tabEntry.Controls.Add(this.m_lblQuality);
			this.m_tabEntry.Controls.Add(this.m_lblPassword);
			this.m_tabEntry.Controls.Add(this.m_lblPasswordRepeat);
			this.m_tabEntry.Controls.Add(this.m_lblUserName);
			this.m_tabEntry.Controls.Add(this.m_tbUserName);
			this.m_tabEntry.Location = new System.Drawing.Point(4, 22);
			this.m_tabEntry.Name = "m_tabEntry";
			this.m_tabEntry.Padding = new System.Windows.Forms.Padding(3);
			this.m_tabEntry.Size = new System.Drawing.Size(467, 342);
			this.m_tabEntry.TabIndex = 0;
			this.m_tabEntry.Text = "General";
			this.m_tabEntry.UseVisualStyleBackColor = true;
			// 
			// m_cbQualityCheck
			// 
			this.m_cbQualityCheck.Appearance = System.Windows.Forms.Appearance.Button;
			this.m_cbQualityCheck.Location = new System.Drawing.Point(424, 116);
			this.m_cbQualityCheck.Name = "m_cbQualityCheck";
			this.m_cbQualityCheck.Size = new System.Drawing.Size(32, 20);
			this.m_cbQualityCheck.TabIndex = 15;
			this.m_cbQualityCheck.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.m_cbQualityCheck.UseVisualStyleBackColor = true;
			this.m_cbQualityCheck.CheckedChanged += new System.EventHandler(this.OnQualityCheckCheckedChanged);
			// 
			// m_rtNotes
			// 
			this.m_rtNotes.Location = new System.Drawing.Point(81, 168);
			this.m_rtNotes.Name = "m_rtNotes";
			this.m_rtNotes.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.m_rtNotes.Size = new System.Drawing.Size(375, 139);
			this.m_rtNotes.TabIndex = 19;
			this.m_rtNotes.Text = "";
			// 
			// m_pbQuality
			// 
			this.m_pbQuality.Location = new System.Drawing.Point(81, 118);
			this.m_pbQuality.Name = "m_pbQuality";
			this.m_pbQuality.Size = new System.Drawing.Size(287, 16);
			this.m_pbQuality.TabIndex = 13;
			this.m_pbQuality.TabStop = false;
			// 
			// m_tbRepeatPassword
			// 
			this.m_tbRepeatPassword.Location = new System.Drawing.Point(81, 91);
			this.m_tbRepeatPassword.Name = "m_tbRepeatPassword";
			this.m_tbRepeatPassword.Size = new System.Drawing.Size(337, 20);
			this.m_tbRepeatPassword.TabIndex = 10;
			// 
			// m_tbPassword
			// 
			this.m_tbPassword.Location = new System.Drawing.Point(81, 64);
			this.m_tbPassword.Name = "m_tbPassword";
			this.m_tbPassword.Size = new System.Drawing.Size(337, 20);
			this.m_tbPassword.TabIndex = 7;
			// 
			// m_btnStandardExpires
			// 
			this.m_btnStandardExpires.ContextMenuStrip = this.m_ctxDefaultTimes;
			this.m_btnStandardExpires.Location = new System.Drawing.Point(424, 311);
			this.m_btnStandardExpires.Name = "m_btnStandardExpires";
			this.m_btnStandardExpires.Size = new System.Drawing.Size(32, 23);
			this.m_btnStandardExpires.TabIndex = 22;
			this.m_btnStandardExpires.UseVisualStyleBackColor = true;
			this.m_btnStandardExpires.Click += new System.EventHandler(this.OnBtnStandardExpiresClick);
			// 
			// m_ctxDefaultTimes
			// 
			this.m_ctxDefaultTimes.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuExpireNow,
            this.m_menuExpireSep0,
            this.m_menuExpire1Week,
            this.m_menuExpire2Weeks,
            this.m_menuExpireSep1,
            this.m_menuExpire1Month,
            this.m_menuExpire3Months,
            this.m_menuExpire6Months,
            this.m_menuExpireSep2,
            this.m_menuExpire1Year});
			this.m_ctxDefaultTimes.Name = "m_ctxDefaultTimes";
			this.m_ctxDefaultTimes.Size = new System.Drawing.Size(125, 176);
			// 
			// m_menuExpireNow
			// 
			this.m_menuExpireNow.Name = "m_menuExpireNow";
			this.m_menuExpireNow.Size = new System.Drawing.Size(124, 22);
			this.m_menuExpireNow.Text = "&Now";
			this.m_menuExpireNow.Click += new System.EventHandler(this.OnMenuExpireNow);
			// 
			// m_menuExpireSep0
			// 
			this.m_menuExpireSep0.Name = "m_menuExpireSep0";
			this.m_menuExpireSep0.Size = new System.Drawing.Size(121, 6);
			// 
			// m_menuExpire1Week
			// 
			this.m_menuExpire1Week.Name = "m_menuExpire1Week";
			this.m_menuExpire1Week.Size = new System.Drawing.Size(124, 22);
			this.m_menuExpire1Week.Text = "&1 Week";
			this.m_menuExpire1Week.Click += new System.EventHandler(this.OnMenuExpire1Week);
			// 
			// m_menuExpire2Weeks
			// 
			this.m_menuExpire2Weeks.Name = "m_menuExpire2Weeks";
			this.m_menuExpire2Weeks.Size = new System.Drawing.Size(124, 22);
			this.m_menuExpire2Weeks.Text = "&2 Weeks";
			this.m_menuExpire2Weeks.Click += new System.EventHandler(this.OnMenuExpire2Weeks);
			// 
			// m_menuExpireSep1
			// 
			this.m_menuExpireSep1.Name = "m_menuExpireSep1";
			this.m_menuExpireSep1.Size = new System.Drawing.Size(121, 6);
			// 
			// m_menuExpire1Month
			// 
			this.m_menuExpire1Month.Name = "m_menuExpire1Month";
			this.m_menuExpire1Month.Size = new System.Drawing.Size(124, 22);
			this.m_menuExpire1Month.Text = "1 &Month";
			this.m_menuExpire1Month.Click += new System.EventHandler(this.OnMenuExpire1Month);
			// 
			// m_menuExpire3Months
			// 
			this.m_menuExpire3Months.Name = "m_menuExpire3Months";
			this.m_menuExpire3Months.Size = new System.Drawing.Size(124, 22);
			this.m_menuExpire3Months.Text = "&3 Months";
			this.m_menuExpire3Months.Click += new System.EventHandler(this.OnMenuExpire3Months);
			// 
			// m_menuExpire6Months
			// 
			this.m_menuExpire6Months.Name = "m_menuExpire6Months";
			this.m_menuExpire6Months.Size = new System.Drawing.Size(124, 22);
			this.m_menuExpire6Months.Text = "&6 Months";
			this.m_menuExpire6Months.Click += new System.EventHandler(this.OnMenuExpire6Months);
			// 
			// m_menuExpireSep2
			// 
			this.m_menuExpireSep2.Name = "m_menuExpireSep2";
			this.m_menuExpireSep2.Size = new System.Drawing.Size(121, 6);
			// 
			// m_menuExpire1Year
			// 
			this.m_menuExpire1Year.Name = "m_menuExpire1Year";
			this.m_menuExpire1Year.Size = new System.Drawing.Size(124, 22);
			this.m_menuExpire1Year.Text = "1 &Year";
			this.m_menuExpire1Year.Click += new System.EventHandler(this.OnMenuExpire1Year);
			// 
			// m_tabAdvanced
			// 
			this.m_tabAdvanced.Controls.Add(this.m_grpAttachments);
			this.m_tabAdvanced.Controls.Add(this.m_grpStringFields);
			this.m_tabAdvanced.Location = new System.Drawing.Point(4, 22);
			this.m_tabAdvanced.Name = "m_tabAdvanced";
			this.m_tabAdvanced.Padding = new System.Windows.Forms.Padding(3);
			this.m_tabAdvanced.Size = new System.Drawing.Size(467, 342);
			this.m_tabAdvanced.TabIndex = 1;
			this.m_tabAdvanced.Text = "Advanced";
			this.m_tabAdvanced.UseVisualStyleBackColor = true;
			// 
			// m_grpAttachments
			// 
			this.m_grpAttachments.Controls.Add(this.m_btnBinOpen);
			this.m_grpAttachments.Controls.Add(this.m_btnBinSave);
			this.m_grpAttachments.Controls.Add(this.m_btnBinDelete);
			this.m_grpAttachments.Controls.Add(this.m_btnBinAdd);
			this.m_grpAttachments.Controls.Add(this.m_lvBinaries);
			this.m_grpAttachments.Location = new System.Drawing.Point(6, 174);
			this.m_grpAttachments.Name = "m_grpAttachments";
			this.m_grpAttachments.Size = new System.Drawing.Size(455, 162);
			this.m_grpAttachments.TabIndex = 1;
			this.m_grpAttachments.TabStop = false;
			this.m_grpAttachments.Text = "File attachments";
			// 
			// m_btnBinOpen
			// 
			this.m_btnBinOpen.Location = new System.Drawing.Point(374, 104);
			this.m_btnBinOpen.Name = "m_btnBinOpen";
			this.m_btnBinOpen.Size = new System.Drawing.Size(75, 23);
			this.m_btnBinOpen.TabIndex = 3;
			this.m_btnBinOpen.Text = "Ope&n";
			this.m_btnBinOpen.UseVisualStyleBackColor = true;
			this.m_btnBinOpen.Click += new System.EventHandler(this.OnBtnBinOpen);
			// 
			// m_btnBinSave
			// 
			this.m_btnBinSave.Location = new System.Drawing.Point(374, 133);
			this.m_btnBinSave.Name = "m_btnBinSave";
			this.m_btnBinSave.Size = new System.Drawing.Size(75, 23);
			this.m_btnBinSave.TabIndex = 4;
			this.m_btnBinSave.Text = "&Save";
			this.m_btnBinSave.UseVisualStyleBackColor = true;
			this.m_btnBinSave.Click += new System.EventHandler(this.OnBtnBinSave);
			// 
			// m_btnBinDelete
			// 
			this.m_btnBinDelete.Location = new System.Drawing.Point(374, 48);
			this.m_btnBinDelete.Name = "m_btnBinDelete";
			this.m_btnBinDelete.Size = new System.Drawing.Size(75, 23);
			this.m_btnBinDelete.TabIndex = 2;
			this.m_btnBinDelete.Text = "De&lete";
			this.m_btnBinDelete.UseVisualStyleBackColor = true;
			this.m_btnBinDelete.Click += new System.EventHandler(this.OnBtnBinDelete);
			// 
			// m_btnBinAdd
			// 
			this.m_btnBinAdd.Location = new System.Drawing.Point(374, 19);
			this.m_btnBinAdd.Name = "m_btnBinAdd";
			this.m_btnBinAdd.Size = new System.Drawing.Size(75, 23);
			this.m_btnBinAdd.TabIndex = 1;
			this.m_btnBinAdd.Text = "A&ttach";
			this.m_btnBinAdd.UseVisualStyleBackColor = true;
			this.m_btnBinAdd.Click += new System.EventHandler(this.OnBtnBinAdd);
			// 
			// m_lvBinaries
			// 
			this.m_lvBinaries.AllowDrop = true;
			this.m_lvBinaries.FullRowSelect = true;
			this.m_lvBinaries.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.m_lvBinaries.HideSelection = false;
			this.m_lvBinaries.LabelEdit = true;
			this.m_lvBinaries.Location = new System.Drawing.Point(6, 20);
			this.m_lvBinaries.Name = "m_lvBinaries";
			this.m_lvBinaries.ShowItemToolTips = true;
			this.m_lvBinaries.Size = new System.Drawing.Size(362, 135);
			this.m_lvBinaries.TabIndex = 0;
			this.m_lvBinaries.UseCompatibleStateImageBehavior = false;
			this.m_lvBinaries.View = System.Windows.Forms.View.Details;
			this.m_lvBinaries.ItemActivate += new System.EventHandler(this.OnBinariesItemActivate);
			this.m_lvBinaries.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.OnBinAfterLabelEdit);
			this.m_lvBinaries.SelectedIndexChanged += new System.EventHandler(this.OnBinariesSelectedIndexChanged);
			this.m_lvBinaries.DragDrop += new System.Windows.Forms.DragEventHandler(this.OnBinDragDrop);
			this.m_lvBinaries.DragEnter += new System.Windows.Forms.DragEventHandler(this.OnBinDragEnter);
			this.m_lvBinaries.DragOver += new System.Windows.Forms.DragEventHandler(this.OnBinDragOver);
			// 
			// m_grpStringFields
			// 
			this.m_grpStringFields.Controls.Add(this.m_btnStrMore);
			this.m_grpStringFields.Controls.Add(this.m_btnStrAdd);
			this.m_grpStringFields.Controls.Add(this.m_btnStrEdit);
			this.m_grpStringFields.Controls.Add(this.m_btnStrDelete);
			this.m_grpStringFields.Controls.Add(this.m_lvStrings);
			this.m_grpStringFields.Location = new System.Drawing.Point(6, 6);
			this.m_grpStringFields.Name = "m_grpStringFields";
			this.m_grpStringFields.Size = new System.Drawing.Size(455, 162);
			this.m_grpStringFields.TabIndex = 0;
			this.m_grpStringFields.TabStop = false;
			this.m_grpStringFields.Text = "String fields";
			// 
			// m_btnStrMore
			// 
			this.m_btnStrMore.Location = new System.Drawing.Point(374, 133);
			this.m_btnStrMore.Name = "m_btnStrMore";
			this.m_btnStrMore.Size = new System.Drawing.Size(75, 23);
			this.m_btnStrMore.TabIndex = 4;
			this.m_btnStrMore.Text = "&More";
			this.m_btnStrMore.UseVisualStyleBackColor = true;
			this.m_btnStrMore.Click += new System.EventHandler(this.OnBtnStrMore);
			// 
			// m_btnStrAdd
			// 
			this.m_btnStrAdd.Location = new System.Drawing.Point(374, 19);
			this.m_btnStrAdd.Name = "m_btnStrAdd";
			this.m_btnStrAdd.Size = new System.Drawing.Size(75, 23);
			this.m_btnStrAdd.TabIndex = 1;
			this.m_btnStrAdd.Text = "&Add";
			this.m_btnStrAdd.UseVisualStyleBackColor = true;
			this.m_btnStrAdd.Click += new System.EventHandler(this.OnBtnStrAdd);
			// 
			// m_btnStrEdit
			// 
			this.m_btnStrEdit.Location = new System.Drawing.Point(374, 48);
			this.m_btnStrEdit.Name = "m_btnStrEdit";
			this.m_btnStrEdit.Size = new System.Drawing.Size(75, 23);
			this.m_btnStrEdit.TabIndex = 2;
			this.m_btnStrEdit.Text = "&Edit";
			this.m_btnStrEdit.UseVisualStyleBackColor = true;
			this.m_btnStrEdit.Click += new System.EventHandler(this.OnBtnStrEdit);
			// 
			// m_btnStrDelete
			// 
			this.m_btnStrDelete.Location = new System.Drawing.Point(374, 77);
			this.m_btnStrDelete.Name = "m_btnStrDelete";
			this.m_btnStrDelete.Size = new System.Drawing.Size(75, 23);
			this.m_btnStrDelete.TabIndex = 3;
			this.m_btnStrDelete.Text = "&Delete";
			this.m_btnStrDelete.UseVisualStyleBackColor = true;
			this.m_btnStrDelete.Click += new System.EventHandler(this.OnBtnStrDelete);
			// 
			// m_lvStrings
			// 
			this.m_lvStrings.ContextMenuStrip = this.m_ctxStr;
			this.m_lvStrings.FullRowSelect = true;
			this.m_lvStrings.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.m_lvStrings.HideSelection = false;
			this.m_lvStrings.Location = new System.Drawing.Point(6, 20);
			this.m_lvStrings.Name = "m_lvStrings";
			this.m_lvStrings.ShowItemToolTips = true;
			this.m_lvStrings.Size = new System.Drawing.Size(362, 135);
			this.m_lvStrings.TabIndex = 0;
			this.m_lvStrings.UseCompatibleStateImageBehavior = false;
			this.m_lvStrings.View = System.Windows.Forms.View.Details;
			this.m_lvStrings.ItemActivate += new System.EventHandler(this.OnStringsItemActivate);
			this.m_lvStrings.SelectedIndexChanged += new System.EventHandler(this.OnStringsSelectedIndexChanged);
			// 
			// m_ctxStr
			// 
			this.m_ctxStr.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_ctxStrCopyName,
            this.m_ctxStrCopyValue,
            this.m_ctxStrSep0,
            this.m_ctxStrCopyItem,
            this.m_ctxStrPasteItem,
            this.m_ctxStrSep1,
            this.m_ctxStrSelectAll,
            this.m_ctxStrSep2,
            this.m_ctxStrMoveTo,
            this.m_ctxStrSep3,
            this.m_ctxStrOtpGen});
			this.m_ctxStr.Name = "m_ctxStr";
			this.m_ctxStr.Size = new System.Drawing.Size(205, 182);
			this.m_ctxStr.Opening += new System.ComponentModel.CancelEventHandler(this.OnCtxStrOpening);
			// 
			// m_ctxStrCopyName
			// 
			this.m_ctxStrCopyName.Image = global::KeePass.Properties.Resources.B16x16_EditCopy;
			this.m_ctxStrCopyName.Name = "m_ctxStrCopyName";
			this.m_ctxStrCopyName.Size = new System.Drawing.Size(204, 22);
			this.m_ctxStrCopyName.Text = "Copy &Name(s)";
			this.m_ctxStrCopyName.Click += new System.EventHandler(this.OnCtxStrCopyName);
			// 
			// m_ctxStrCopyValue
			// 
			this.m_ctxStrCopyValue.Image = global::KeePass.Properties.Resources.B16x16_EditCopy;
			this.m_ctxStrCopyValue.Name = "m_ctxStrCopyValue";
			this.m_ctxStrCopyValue.Size = new System.Drawing.Size(204, 22);
			this.m_ctxStrCopyValue.Text = "Copy &Value(s)";
			this.m_ctxStrCopyValue.Click += new System.EventHandler(this.OnCtxStrCopyValue);
			// 
			// m_ctxStrSep0
			// 
			this.m_ctxStrSep0.Name = "m_ctxStrSep0";
			this.m_ctxStrSep0.Size = new System.Drawing.Size(201, 6);
			// 
			// m_ctxStrCopyItem
			// 
			this.m_ctxStrCopyItem.Image = global::KeePass.Properties.Resources.B16x16_EditCopy;
			this.m_ctxStrCopyItem.Name = "m_ctxStrCopyItem";
			this.m_ctxStrCopyItem.Size = new System.Drawing.Size(204, 22);
			this.m_ctxStrCopyItem.Text = "&Copy Item(s)";
			this.m_ctxStrCopyItem.Click += new System.EventHandler(this.OnCtxStrCopyItem);
			// 
			// m_ctxStrPasteItem
			// 
			this.m_ctxStrPasteItem.Image = global::KeePass.Properties.Resources.B16x16_EditPaste;
			this.m_ctxStrPasteItem.Name = "m_ctxStrPasteItem";
			this.m_ctxStrPasteItem.Size = new System.Drawing.Size(204, 22);
			this.m_ctxStrPasteItem.Text = "&Paste Item(s)";
			this.m_ctxStrPasteItem.Click += new System.EventHandler(this.OnCtxStrPasteItem);
			// 
			// m_ctxStrSep1
			// 
			this.m_ctxStrSep1.Name = "m_ctxStrSep1";
			this.m_ctxStrSep1.Size = new System.Drawing.Size(201, 6);
			// 
			// m_ctxStrSelectAll
			// 
			this.m_ctxStrSelectAll.Name = "m_ctxStrSelectAll";
			this.m_ctxStrSelectAll.Size = new System.Drawing.Size(204, 22);
			this.m_ctxStrSelectAll.Text = "&Select All";
			this.m_ctxStrSelectAll.Click += new System.EventHandler(this.OnCtxStrSelectAll);
			// 
			// m_ctxStrSep2
			// 
			this.m_ctxStrSep2.Name = "m_ctxStrSep2";
			this.m_ctxStrSep2.Size = new System.Drawing.Size(201, 6);
			// 
			// m_ctxStrMoveTo
			// 
			this.m_ctxStrMoveTo.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_ctxStrMoveToTitle,
            this.m_ctxStrMoveToUserName,
            this.m_ctxStrMoveToPassword,
            this.m_ctxStrMoveToUrl,
            this.m_ctxStrMoveToNotes});
			this.m_ctxStrMoveTo.Name = "m_ctxStrMoveTo";
			this.m_ctxStrMoveTo.Size = new System.Drawing.Size(204, 22);
			this.m_ctxStrMoveTo.Text = "&Move To";
			// 
			// m_ctxStrMoveToTitle
			// 
			this.m_ctxStrMoveToTitle.Name = "m_ctxStrMoveToTitle";
			this.m_ctxStrMoveToTitle.Size = new System.Drawing.Size(132, 22);
			this.m_ctxStrMoveToTitle.Text = "&Title";
			this.m_ctxStrMoveToTitle.Click += new System.EventHandler(this.OnCtxStrMoveToTitle);
			// 
			// m_ctxStrMoveToUserName
			// 
			this.m_ctxStrMoveToUserName.Name = "m_ctxStrMoveToUserName";
			this.m_ctxStrMoveToUserName.Size = new System.Drawing.Size(132, 22);
			this.m_ctxStrMoveToUserName.Text = "&User Name";
			this.m_ctxStrMoveToUserName.Click += new System.EventHandler(this.OnCtxStrMoveToUserName);
			// 
			// m_ctxStrMoveToPassword
			// 
			this.m_ctxStrMoveToPassword.Name = "m_ctxStrMoveToPassword";
			this.m_ctxStrMoveToPassword.Size = new System.Drawing.Size(132, 22);
			this.m_ctxStrMoveToPassword.Text = "&Password";
			this.m_ctxStrMoveToPassword.Click += new System.EventHandler(this.OnCtxStrMoveToPassword);
			// 
			// m_ctxStrMoveToUrl
			// 
			this.m_ctxStrMoveToUrl.Name = "m_ctxStrMoveToUrl";
			this.m_ctxStrMoveToUrl.Size = new System.Drawing.Size(132, 22);
			this.m_ctxStrMoveToUrl.Text = "UR&L";
			this.m_ctxStrMoveToUrl.Click += new System.EventHandler(this.OnCtxStrMoveToUrl);
			// 
			// m_ctxStrMoveToNotes
			// 
			this.m_ctxStrMoveToNotes.Name = "m_ctxStrMoveToNotes";
			this.m_ctxStrMoveToNotes.Size = new System.Drawing.Size(132, 22);
			this.m_ctxStrMoveToNotes.Text = "&Notes";
			this.m_ctxStrMoveToNotes.Click += new System.EventHandler(this.OnCtxStrMoveToNotes);
			// 
			// m_ctxStrSep3
			// 
			this.m_ctxStrSep3.Name = "m_ctxStrSep3";
			this.m_ctxStrSep3.Size = new System.Drawing.Size(201, 6);
			// 
			// m_ctxStrOtpGen
			// 
			this.m_ctxStrOtpGen.Image = global::KeePass.Properties.Resources.B16x16_KGPG_Gen;
			this.m_ctxStrOtpGen.Name = "m_ctxStrOtpGen";
			this.m_ctxStrOtpGen.Size = new System.Drawing.Size(204, 22);
			this.m_ctxStrOtpGen.Text = "&OTP Generator Settings...";
			this.m_ctxStrOtpGen.Click += new System.EventHandler(this.OnCtxStrOtpGen);
			// 
			// m_tabProperties
			// 
			this.m_tabProperties.Controls.Add(this.m_btnTags);
			this.m_tabProperties.Controls.Add(this.m_linkTagsInh);
			this.m_tabProperties.Controls.Add(this.m_lblCustomData);
			this.m_tabProperties.Controls.Add(this.m_btnCDDel);
			this.m_tabProperties.Controls.Add(this.m_lvCustomData);
			this.m_tabProperties.Controls.Add(this.m_tbTags);
			this.m_tabProperties.Controls.Add(this.m_lblTags);
			this.m_tabProperties.Controls.Add(this.m_btnPickFgColor);
			this.m_tabProperties.Controls.Add(this.m_cbCustomForegroundColor);
			this.m_tabProperties.Controls.Add(this.m_tbUuid);
			this.m_tabProperties.Controls.Add(this.m_lblUuid);
			this.m_tabProperties.Controls.Add(this.m_lblOverrideUrl);
			this.m_tabProperties.Controls.Add(this.m_cbCustomBackgroundColor);
			this.m_tabProperties.Controls.Add(this.m_btnPickBgColor);
			this.m_tabProperties.Controls.Add(this.m_cmbOverrideUrl);
			this.m_tabProperties.Location = new System.Drawing.Point(4, 22);
			this.m_tabProperties.Name = "m_tabProperties";
			this.m_tabProperties.Size = new System.Drawing.Size(467, 342);
			this.m_tabProperties.TabIndex = 4;
			this.m_tabProperties.Text = "Properties";
			this.m_tabProperties.UseVisualStyleBackColor = true;
			// 
			// m_btnTags
			// 
			this.m_btnTags.Location = new System.Drawing.Point(424, 90);
			this.m_btnTags.Name = "m_btnTags";
			this.m_btnTags.Size = new System.Drawing.Size(32, 23);
			this.m_btnTags.TabIndex = 7;
			this.m_btnTags.UseVisualStyleBackColor = true;
			// 
			// m_linkTagsInh
			// 
			this.m_linkTagsInh.Location = new System.Drawing.Point(199, 74);
			this.m_linkTagsInh.Name = "m_linkTagsInh";
			this.m_linkTagsInh.Size = new System.Drawing.Size(260, 14);
			this.m_linkTagsInh.TabIndex = 4;
			this.m_linkTagsInh.TabStop = true;
			this.m_linkTagsInh.Text = "<DYN>";
			this.m_linkTagsInh.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// m_lblCustomData
			// 
			this.m_lblCustomData.AutoSize = true;
			this.m_lblCustomData.Location = new System.Drawing.Point(6, 175);
			this.m_lblCustomData.Name = "m_lblCustomData";
			this.m_lblCustomData.Size = new System.Drawing.Size(63, 13);
			this.m_lblCustomData.TabIndex = 10;
			this.m_lblCustomData.Text = "&Plugin data:";
			// 
			// m_btnCDDel
			// 
			this.m_btnCDDel.Location = new System.Drawing.Point(381, 192);
			this.m_btnCDDel.Name = "m_btnCDDel";
			this.m_btnCDDel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCDDel.TabIndex = 12;
			this.m_btnCDDel.Text = "&Delete";
			this.m_btnCDDel.UseVisualStyleBackColor = true;
			this.m_btnCDDel.Click += new System.EventHandler(this.OnBtnCDDel);
			// 
			// m_lvCustomData
			// 
			this.m_lvCustomData.FullRowSelect = true;
			this.m_lvCustomData.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.m_lvCustomData.HideSelection = false;
			this.m_lvCustomData.Location = new System.Drawing.Point(9, 193);
			this.m_lvCustomData.Name = "m_lvCustomData";
			this.m_lvCustomData.ShowItemToolTips = true;
			this.m_lvCustomData.Size = new System.Drawing.Size(366, 101);
			this.m_lvCustomData.TabIndex = 11;
			this.m_lvCustomData.UseCompatibleStateImageBehavior = false;
			this.m_lvCustomData.View = System.Windows.Forms.View.Details;
			this.m_lvCustomData.SelectedIndexChanged += new System.EventHandler(this.OnCustomDataSelectedIndexChanged);
			// 
			// m_tbTags
			// 
			this.m_tbTags.Location = new System.Drawing.Point(9, 92);
			this.m_tbTags.Name = "m_tbTags";
			this.m_tbTags.Size = new System.Drawing.Size(409, 20);
			this.m_tbTags.TabIndex = 6;
			// 
			// m_lblTags
			// 
			this.m_lblTags.AutoSize = true;
			this.m_lblTags.Location = new System.Drawing.Point(6, 74);
			this.m_lblTags.Name = "m_lblTags";
			this.m_lblTags.Size = new System.Drawing.Size(34, 13);
			this.m_lblTags.TabIndex = 5;
			this.m_lblTags.Text = "&Tags:";
			// 
			// m_btnPickFgColor
			// 
			this.m_btnPickFgColor.Location = new System.Drawing.Point(165, 9);
			this.m_btnPickFgColor.Name = "m_btnPickFgColor";
			this.m_btnPickFgColor.Size = new System.Drawing.Size(48, 23);
			this.m_btnPickFgColor.TabIndex = 1;
			this.m_btnPickFgColor.UseVisualStyleBackColor = true;
			// 
			// m_cbCustomForegroundColor
			// 
			this.m_cbCustomForegroundColor.AutoSize = true;
			this.m_cbCustomForegroundColor.Location = new System.Drawing.Point(9, 13);
			this.m_cbCustomForegroundColor.Name = "m_cbCustomForegroundColor";
			this.m_cbCustomForegroundColor.Size = new System.Drawing.Size(144, 17);
			this.m_cbCustomForegroundColor.TabIndex = 0;
			this.m_cbCustomForegroundColor.Text = "Custom &foreground color:";
			this.m_cbCustomForegroundColor.UseVisualStyleBackColor = true;
			this.m_cbCustomForegroundColor.CheckedChanged += new System.EventHandler(this.OnCustomForegroundColorCheckedChanged);
			// 
			// m_tbUuid
			// 
			this.m_tbUuid.Location = new System.Drawing.Point(49, 309);
			this.m_tbUuid.Name = "m_tbUuid";
			this.m_tbUuid.ReadOnly = true;
			this.m_tbUuid.Size = new System.Drawing.Size(407, 20);
			this.m_tbUuid.TabIndex = 14;
			// 
			// m_lblUuid
			// 
			this.m_lblUuid.AutoSize = true;
			this.m_lblUuid.Location = new System.Drawing.Point(6, 312);
			this.m_lblUuid.Name = "m_lblUuid";
			this.m_lblUuid.Size = new System.Drawing.Size(37, 13);
			this.m_lblUuid.TabIndex = 13;
			this.m_lblUuid.Text = "&UUID:";
			// 
			// m_lblOverrideUrl
			// 
			this.m_lblOverrideUrl.AutoSize = true;
			this.m_lblOverrideUrl.Location = new System.Drawing.Point(6, 124);
			this.m_lblOverrideUrl.Name = "m_lblOverrideUrl";
			this.m_lblOverrideUrl.Size = new System.Drawing.Size(222, 13);
			this.m_lblOverrideUrl.TabIndex = 8;
			this.m_lblOverrideUrl.Text = "O&verride URL (e.g. to use a specific browser):";
			// 
			// m_cbCustomBackgroundColor
			// 
			this.m_cbCustomBackgroundColor.AutoSize = true;
			this.m_cbCustomBackgroundColor.Location = new System.Drawing.Point(9, 42);
			this.m_cbCustomBackgroundColor.Name = "m_cbCustomBackgroundColor";
			this.m_cbCustomBackgroundColor.Size = new System.Drawing.Size(150, 17);
			this.m_cbCustomBackgroundColor.TabIndex = 2;
			this.m_cbCustomBackgroundColor.Text = "Custom &background color:";
			this.m_cbCustomBackgroundColor.UseVisualStyleBackColor = true;
			this.m_cbCustomBackgroundColor.CheckedChanged += new System.EventHandler(this.OnCustomBackgroundColorCheckedChanged);
			// 
			// m_btnPickBgColor
			// 
			this.m_btnPickBgColor.Location = new System.Drawing.Point(165, 38);
			this.m_btnPickBgColor.Name = "m_btnPickBgColor";
			this.m_btnPickBgColor.Size = new System.Drawing.Size(48, 23);
			this.m_btnPickBgColor.TabIndex = 3;
			this.m_btnPickBgColor.UseVisualStyleBackColor = true;
			// 
			// m_cmbOverrideUrl
			// 
			this.m_cmbOverrideUrl.IntegralHeight = false;
			this.m_cmbOverrideUrl.Location = new System.Drawing.Point(9, 142);
			this.m_cmbOverrideUrl.MaxDropDownItems = 16;
			this.m_cmbOverrideUrl.Name = "m_cmbOverrideUrl";
			this.m_cmbOverrideUrl.Size = new System.Drawing.Size(447, 21);
			this.m_cmbOverrideUrl.TabIndex = 9;
			this.m_cmbOverrideUrl.TextChanged += new System.EventHandler(this.OnUrlOverrideTextChanged);
			// 
			// m_tabAutoType
			// 
			this.m_tabAutoType.Controls.Add(this.m_btnAutoTypeMore);
			this.m_tabAutoType.Controls.Add(this.m_btnAutoTypeDown);
			this.m_tabAutoType.Controls.Add(this.m_btnAutoTypeUp);
			this.m_tabAutoType.Controls.Add(this.m_linkAutoTypeObfuscation);
			this.m_tabAutoType.Controls.Add(this.m_cbAutoTypeObfuscation);
			this.m_tabAutoType.Controls.Add(this.m_btnAutoTypeEditDefault);
			this.m_tabAutoType.Controls.Add(this.m_rbAutoTypeOverride);
			this.m_tabAutoType.Controls.Add(this.m_rbAutoTypeSeqInherit);
			this.m_tabAutoType.Controls.Add(this.m_lblCustomAutoType);
			this.m_tabAutoType.Controls.Add(this.m_cbAutoTypeEnabled);
			this.m_tabAutoType.Controls.Add(this.m_tbDefaultAutoTypeSeq);
			this.m_tabAutoType.Controls.Add(this.m_btnAutoTypeEdit);
			this.m_tabAutoType.Controls.Add(this.m_btnAutoTypeAdd);
			this.m_tabAutoType.Controls.Add(this.m_btnAutoTypeDelete);
			this.m_tabAutoType.Controls.Add(this.m_lvAutoType);
			this.m_tabAutoType.Location = new System.Drawing.Point(4, 22);
			this.m_tabAutoType.Name = "m_tabAutoType";
			this.m_tabAutoType.Size = new System.Drawing.Size(467, 342);
			this.m_tabAutoType.TabIndex = 2;
			this.m_tabAutoType.Text = "Auto-Type";
			this.m_tabAutoType.UseVisualStyleBackColor = true;
			// 
			// m_btnAutoTypeMore
			// 
			this.m_btnAutoTypeMore.Location = new System.Drawing.Point(382, 285);
			this.m_btnAutoTypeMore.Name = "m_btnAutoTypeMore";
			this.m_btnAutoTypeMore.Size = new System.Drawing.Size(75, 23);
			this.m_btnAutoTypeMore.TabIndex = 12;
			this.m_btnAutoTypeMore.Text = "&More";
			this.m_btnAutoTypeMore.UseVisualStyleBackColor = true;
			this.m_btnAutoTypeMore.Click += new System.EventHandler(this.OnBtnAutoTypeMore);
			// 
			// m_btnAutoTypeDown
			// 
			this.m_btnAutoTypeDown.Image = global::KeePass.Properties.Resources.B16x16_1DownArrow;
			this.m_btnAutoTypeDown.Location = new System.Drawing.Point(382, 249);
			this.m_btnAutoTypeDown.Name = "m_btnAutoTypeDown";
			this.m_btnAutoTypeDown.Size = new System.Drawing.Size(75, 23);
			this.m_btnAutoTypeDown.TabIndex = 11;
			this.m_btnAutoTypeDown.UseVisualStyleBackColor = true;
			this.m_btnAutoTypeDown.Click += new System.EventHandler(this.OnBtnAutoTypeDown);
			// 
			// m_btnAutoTypeUp
			// 
			this.m_btnAutoTypeUp.Image = global::KeePass.Properties.Resources.B16x16_1UpArrow;
			this.m_btnAutoTypeUp.Location = new System.Drawing.Point(382, 220);
			this.m_btnAutoTypeUp.Name = "m_btnAutoTypeUp";
			this.m_btnAutoTypeUp.Size = new System.Drawing.Size(75, 23);
			this.m_btnAutoTypeUp.TabIndex = 10;
			this.m_btnAutoTypeUp.UseVisualStyleBackColor = true;
			this.m_btnAutoTypeUp.Click += new System.EventHandler(this.OnBtnAutoTypeUp);
			// 
			// m_linkAutoTypeObfuscation
			// 
			this.m_linkAutoTypeObfuscation.AutoSize = true;
			this.m_linkAutoTypeObfuscation.Location = new System.Drawing.Point(208, 317);
			this.m_linkAutoTypeObfuscation.Name = "m_linkAutoTypeObfuscation";
			this.m_linkAutoTypeObfuscation.Size = new System.Drawing.Size(68, 13);
			this.m_linkAutoTypeObfuscation.TabIndex = 14;
			this.m_linkAutoTypeObfuscation.TabStop = true;
			this.m_linkAutoTypeObfuscation.Text = "What is this?";
			this.m_linkAutoTypeObfuscation.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnAutoTypeObfuscationLink);
			// 
			// m_cbAutoTypeObfuscation
			// 
			this.m_cbAutoTypeObfuscation.AutoSize = true;
			this.m_cbAutoTypeObfuscation.Location = new System.Drawing.Point(9, 316);
			this.m_cbAutoTypeObfuscation.Name = "m_cbAutoTypeObfuscation";
			this.m_cbAutoTypeObfuscation.Size = new System.Drawing.Size(193, 17);
			this.m_cbAutoTypeObfuscation.TabIndex = 13;
			this.m_cbAutoTypeObfuscation.Text = "Two-&channel auto-type obfuscation";
			this.m_cbAutoTypeObfuscation.UseVisualStyleBackColor = true;
			this.m_cbAutoTypeObfuscation.CheckedChanged += new System.EventHandler(this.OnAutoTypeObfuscationCheckedChanged);
			// 
			// m_btnAutoTypeEditDefault
			// 
			this.m_btnAutoTypeEditDefault.Location = new System.Drawing.Point(382, 82);
			this.m_btnAutoTypeEditDefault.Name = "m_btnAutoTypeEditDefault";
			this.m_btnAutoTypeEditDefault.Size = new System.Drawing.Size(75, 23);
			this.m_btnAutoTypeEditDefault.TabIndex = 4;
			this.m_btnAutoTypeEditDefault.Text = "Edi&t";
			this.m_btnAutoTypeEditDefault.UseVisualStyleBackColor = true;
			this.m_btnAutoTypeEditDefault.Click += new System.EventHandler(this.OnBtnAutoTypeEditDefault);
			// 
			// m_rbAutoTypeOverride
			// 
			this.m_rbAutoTypeOverride.AutoSize = true;
			this.m_rbAutoTypeOverride.Location = new System.Drawing.Point(9, 63);
			this.m_rbAutoTypeOverride.Name = "m_rbAutoTypeOverride";
			this.m_rbAutoTypeOverride.Size = new System.Drawing.Size(153, 17);
			this.m_rbAutoTypeOverride.TabIndex = 2;
			this.m_rbAutoTypeOverride.TabStop = true;
			this.m_rbAutoTypeOverride.Text = "O&verride default sequence:";
			this.m_rbAutoTypeOverride.UseVisualStyleBackColor = true;
			// 
			// m_rbAutoTypeSeqInherit
			// 
			this.m_rbAutoTypeSeqInherit.AutoSize = true;
			this.m_rbAutoTypeSeqInherit.Location = new System.Drawing.Point(9, 42);
			this.m_rbAutoTypeSeqInherit.Name = "m_rbAutoTypeSeqInherit";
			this.m_rbAutoTypeSeqInherit.Size = new System.Drawing.Size(239, 17);
			this.m_rbAutoTypeSeqInherit.TabIndex = 1;
			this.m_rbAutoTypeSeqInherit.TabStop = true;
			this.m_rbAutoTypeSeqInherit.Text = "&Inherit default auto-type sequence from group";
			this.m_rbAutoTypeSeqInherit.UseVisualStyleBackColor = true;
			this.m_rbAutoTypeSeqInherit.CheckedChanged += new System.EventHandler(this.OnAutoTypeSeqInheritCheckedChanged);
			// 
			// m_lblCustomAutoType
			// 
			this.m_lblCustomAutoType.AutoSize = true;
			this.m_lblCustomAutoType.Location = new System.Drawing.Point(6, 118);
			this.m_lblCustomAutoType.Name = "m_lblCustomAutoType";
			this.m_lblCustomAutoType.Size = new System.Drawing.Size(219, 13);
			this.m_lblCustomAutoType.TabIndex = 5;
			this.m_lblCustomAutoType.Text = "&Use custom sequences for specific windows:";
			// 
			// m_cbAutoTypeEnabled
			// 
			this.m_cbAutoTypeEnabled.AutoSize = true;
			this.m_cbAutoTypeEnabled.Location = new System.Drawing.Point(9, 13);
			this.m_cbAutoTypeEnabled.Name = "m_cbAutoTypeEnabled";
			this.m_cbAutoTypeEnabled.Size = new System.Drawing.Size(166, 17);
			this.m_cbAutoTypeEnabled.TabIndex = 0;
			this.m_cbAutoTypeEnabled.Text = "E&nable auto-type for this entry";
			this.m_cbAutoTypeEnabled.UseVisualStyleBackColor = true;
			this.m_cbAutoTypeEnabled.CheckedChanged += new System.EventHandler(this.OnAutoTypeEnableCheckedChanged);
			// 
			// m_tbDefaultAutoTypeSeq
			// 
			this.m_tbDefaultAutoTypeSeq.Location = new System.Drawing.Point(28, 84);
			this.m_tbDefaultAutoTypeSeq.Name = "m_tbDefaultAutoTypeSeq";
			this.m_tbDefaultAutoTypeSeq.Size = new System.Drawing.Size(348, 20);
			this.m_tbDefaultAutoTypeSeq.TabIndex = 3;
			// 
			// m_btnAutoTypeEdit
			// 
			this.m_btnAutoTypeEdit.Location = new System.Drawing.Point(382, 162);
			this.m_btnAutoTypeEdit.Name = "m_btnAutoTypeEdit";
			this.m_btnAutoTypeEdit.Size = new System.Drawing.Size(75, 23);
			this.m_btnAutoTypeEdit.TabIndex = 8;
			this.m_btnAutoTypeEdit.Text = "&Edit";
			this.m_btnAutoTypeEdit.UseVisualStyleBackColor = true;
			this.m_btnAutoTypeEdit.Click += new System.EventHandler(this.OnBtnAutoTypeEdit);
			// 
			// m_btnAutoTypeAdd
			// 
			this.m_btnAutoTypeAdd.Location = new System.Drawing.Point(382, 133);
			this.m_btnAutoTypeAdd.Name = "m_btnAutoTypeAdd";
			this.m_btnAutoTypeAdd.Size = new System.Drawing.Size(75, 23);
			this.m_btnAutoTypeAdd.TabIndex = 7;
			this.m_btnAutoTypeAdd.Text = "&Add";
			this.m_btnAutoTypeAdd.UseVisualStyleBackColor = true;
			this.m_btnAutoTypeAdd.Click += new System.EventHandler(this.OnBtnAutoTypeAdd);
			// 
			// m_btnAutoTypeDelete
			// 
			this.m_btnAutoTypeDelete.Location = new System.Drawing.Point(382, 191);
			this.m_btnAutoTypeDelete.Name = "m_btnAutoTypeDelete";
			this.m_btnAutoTypeDelete.Size = new System.Drawing.Size(75, 23);
			this.m_btnAutoTypeDelete.TabIndex = 9;
			this.m_btnAutoTypeDelete.Text = "&Delete";
			this.m_btnAutoTypeDelete.UseVisualStyleBackColor = true;
			this.m_btnAutoTypeDelete.Click += new System.EventHandler(this.OnBtnAutoTypeDelete);
			// 
			// m_lvAutoType
			// 
			this.m_lvAutoType.ContextMenuStrip = this.m_ctxAutoType;
			this.m_lvAutoType.FullRowSelect = true;
			this.m_lvAutoType.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.m_lvAutoType.HideSelection = false;
			this.m_lvAutoType.Location = new System.Drawing.Point(9, 134);
			this.m_lvAutoType.Name = "m_lvAutoType";
			this.m_lvAutoType.ShowItemToolTips = true;
			this.m_lvAutoType.Size = new System.Drawing.Size(367, 173);
			this.m_lvAutoType.TabIndex = 6;
			this.m_lvAutoType.UseCompatibleStateImageBehavior = false;
			this.m_lvAutoType.View = System.Windows.Forms.View.Details;
			this.m_lvAutoType.ItemActivate += new System.EventHandler(this.OnAutoTypeItemActivate);
			this.m_lvAutoType.SelectedIndexChanged += new System.EventHandler(this.OnAutoTypeSelectedIndexChanged);
			// 
			// m_ctxAutoType
			// 
			this.m_ctxAutoType.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_ctxAutoTypeCopyWnd,
            this.m_ctxAutoTypeCopySeq,
            this.m_ctxAutoTypeSep0,
            this.m_ctxAutoTypeCopyItem,
            this.m_ctxAutoTypePasteItem,
            this.m_ctxAutoTypeDup,
            this.m_ctxAutoTypeSep1,
            this.m_ctxAutoTypeSelectAll});
			this.m_ctxAutoType.Name = "m_ctxAutoType";
			this.m_ctxAutoType.Size = new System.Drawing.Size(198, 148);
			this.m_ctxAutoType.Opening += new System.ComponentModel.CancelEventHandler(this.OnCtxAutoTypeOpening);
			// 
			// m_ctxAutoTypeCopyWnd
			// 
			this.m_ctxAutoTypeCopyWnd.Image = global::KeePass.Properties.Resources.B16x16_EditCopy;
			this.m_ctxAutoTypeCopyWnd.Name = "m_ctxAutoTypeCopyWnd";
			this.m_ctxAutoTypeCopyWnd.Size = new System.Drawing.Size(197, 22);
			this.m_ctxAutoTypeCopyWnd.Text = "Copy &Target Window(s)";
			this.m_ctxAutoTypeCopyWnd.Click += new System.EventHandler(this.OnCtxAutoTypeCopyWnd);
			// 
			// m_ctxAutoTypeCopySeq
			// 
			this.m_ctxAutoTypeCopySeq.Image = global::KeePass.Properties.Resources.B16x16_EditCopy;
			this.m_ctxAutoTypeCopySeq.Name = "m_ctxAutoTypeCopySeq";
			this.m_ctxAutoTypeCopySeq.Size = new System.Drawing.Size(197, 22);
			this.m_ctxAutoTypeCopySeq.Text = "Copy &Sequence(s)";
			this.m_ctxAutoTypeCopySeq.Click += new System.EventHandler(this.OnCtxAutoTypeCopySeq);
			// 
			// m_ctxAutoTypeSep0
			// 
			this.m_ctxAutoTypeSep0.Name = "m_ctxAutoTypeSep0";
			this.m_ctxAutoTypeSep0.Size = new System.Drawing.Size(194, 6);
			// 
			// m_ctxAutoTypeCopyItem
			// 
			this.m_ctxAutoTypeCopyItem.Image = global::KeePass.Properties.Resources.B16x16_EditCopy;
			this.m_ctxAutoTypeCopyItem.Name = "m_ctxAutoTypeCopyItem";
			this.m_ctxAutoTypeCopyItem.Size = new System.Drawing.Size(197, 22);
			this.m_ctxAutoTypeCopyItem.Text = "&Copy Item(s)";
			this.m_ctxAutoTypeCopyItem.Click += new System.EventHandler(this.OnCtxAutoTypeCopyItem);
			// 
			// m_ctxAutoTypePasteItem
			// 
			this.m_ctxAutoTypePasteItem.Image = global::KeePass.Properties.Resources.B16x16_EditPaste;
			this.m_ctxAutoTypePasteItem.Name = "m_ctxAutoTypePasteItem";
			this.m_ctxAutoTypePasteItem.Size = new System.Drawing.Size(197, 22);
			this.m_ctxAutoTypePasteItem.Text = "&Paste Item(s)";
			this.m_ctxAutoTypePasteItem.Click += new System.EventHandler(this.OnCtxAutoTypePasteItem);
			// 
			// m_ctxAutoTypeDup
			// 
			this.m_ctxAutoTypeDup.Image = global::KeePass.Properties.Resources.B16x16_EditShred;
			this.m_ctxAutoTypeDup.Name = "m_ctxAutoTypeDup";
			this.m_ctxAutoTypeDup.Size = new System.Drawing.Size(197, 22);
			this.m_ctxAutoTypeDup.Text = "&Duplicate Item(s)";
			this.m_ctxAutoTypeDup.Click += new System.EventHandler(this.OnCtxAutoTypeDup);
			// 
			// m_ctxAutoTypeSep1
			// 
			this.m_ctxAutoTypeSep1.Name = "m_ctxAutoTypeSep1";
			this.m_ctxAutoTypeSep1.Size = new System.Drawing.Size(194, 6);
			// 
			// m_ctxAutoTypeSelectAll
			// 
			this.m_ctxAutoTypeSelectAll.Name = "m_ctxAutoTypeSelectAll";
			this.m_ctxAutoTypeSelectAll.Size = new System.Drawing.Size(197, 22);
			this.m_ctxAutoTypeSelectAll.Text = "Select &All";
			this.m_ctxAutoTypeSelectAll.Click += new System.EventHandler(this.OnCtxAutoTypeSelectAll);
			// 
			// m_tabHistory
			// 
			this.m_tabHistory.Controls.Add(this.m_btnHistoryCompare);
			this.m_tabHistory.Controls.Add(this.m_lblPasswordModifiedData);
			this.m_tabHistory.Controls.Add(this.m_lblPasswordModified);
			this.m_tabHistory.Controls.Add(this.m_lblVersions);
			this.m_tabHistory.Controls.Add(this.m_lblModifiedData);
			this.m_tabHistory.Controls.Add(this.m_lblModified);
			this.m_tabHistory.Controls.Add(this.m_lblCreatedData);
			this.m_tabHistory.Controls.Add(this.m_lblCreated);
			this.m_tabHistory.Controls.Add(this.m_btnHistoryDelete);
			this.m_tabHistory.Controls.Add(this.m_btnHistoryView);
			this.m_tabHistory.Controls.Add(this.m_btnHistoryRestore);
			this.m_tabHistory.Controls.Add(this.m_lvHistory);
			this.m_tabHistory.Location = new System.Drawing.Point(4, 22);
			this.m_tabHistory.Name = "m_tabHistory";
			this.m_tabHistory.Size = new System.Drawing.Size(467, 342);
			this.m_tabHistory.TabIndex = 3;
			this.m_tabHistory.Text = "History";
			this.m_tabHistory.UseVisualStyleBackColor = true;
			// 
			// m_btnHistoryCompare
			// 
			this.m_btnHistoryCompare.Location = new System.Drawing.Point(89, 307);
			this.m_btnHistoryCompare.Name = "m_btnHistoryCompare";
			this.m_btnHistoryCompare.Size = new System.Drawing.Size(75, 23);
			this.m_btnHistoryCompare.TabIndex = 9;
			this.m_btnHistoryCompare.Text = "&Compare";
			this.m_btnHistoryCompare.UseVisualStyleBackColor = true;
			this.m_btnHistoryCompare.Click += new System.EventHandler(this.OnBtnHistoryCompare);
			// 
			// m_lblPasswordModifiedData
			// 
			this.m_lblPasswordModifiedData.AutoSize = true;
			this.m_lblPasswordModifiedData.Location = new System.Drawing.Point(110, 57);
			this.m_lblPasswordModifiedData.Name = "m_lblPasswordModifiedData";
			this.m_lblPasswordModifiedData.Size = new System.Drawing.Size(19, 13);
			this.m_lblPasswordModifiedData.TabIndex = 5;
			this.m_lblPasswordModifiedData.Text = "<>";
			// 
			// m_lblPasswordModified
			// 
			this.m_lblPasswordModified.AutoSize = true;
			this.m_lblPasswordModified.Location = new System.Drawing.Point(6, 57);
			this.m_lblPasswordModified.Name = "m_lblPasswordModified";
			this.m_lblPasswordModified.Size = new System.Drawing.Size(98, 13);
			this.m_lblPasswordModified.TabIndex = 4;
			this.m_lblPasswordModified.Text = "Password modified:";
			// 
			// m_lblVersions
			// 
			this.m_lblVersions.AutoSize = true;
			this.m_lblVersions.Location = new System.Drawing.Point(6, 79);
			this.m_lblVersions.Name = "m_lblVersions";
			this.m_lblVersions.Size = new System.Drawing.Size(50, 13);
			this.m_lblVersions.TabIndex = 6;
			this.m_lblVersions.Text = "V&ersions:";
			// 
			// m_lblModifiedData
			// 
			this.m_lblModifiedData.AutoSize = true;
			this.m_lblModifiedData.Location = new System.Drawing.Point(110, 35);
			this.m_lblModifiedData.Name = "m_lblModifiedData";
			this.m_lblModifiedData.Size = new System.Drawing.Size(19, 13);
			this.m_lblModifiedData.TabIndex = 3;
			this.m_lblModifiedData.Text = "<>";
			// 
			// m_lblModified
			// 
			this.m_lblModified.AutoSize = true;
			this.m_lblModified.Location = new System.Drawing.Point(6, 35);
			this.m_lblModified.Name = "m_lblModified";
			this.m_lblModified.Size = new System.Drawing.Size(50, 13);
			this.m_lblModified.TabIndex = 2;
			this.m_lblModified.Text = "Modified:";
			// 
			// m_lblCreatedData
			// 
			this.m_lblCreatedData.AutoSize = true;
			this.m_lblCreatedData.Location = new System.Drawing.Point(110, 13);
			this.m_lblCreatedData.Name = "m_lblCreatedData";
			this.m_lblCreatedData.Size = new System.Drawing.Size(19, 13);
			this.m_lblCreatedData.TabIndex = 1;
			this.m_lblCreatedData.Text = "<>";
			// 
			// m_lblCreated
			// 
			this.m_lblCreated.AutoSize = true;
			this.m_lblCreated.Location = new System.Drawing.Point(6, 13);
			this.m_lblCreated.Name = "m_lblCreated";
			this.m_lblCreated.Size = new System.Drawing.Size(47, 13);
			this.m_lblCreated.TabIndex = 0;
			this.m_lblCreated.Text = "Created:";
			// 
			// m_btnHistoryDelete
			// 
			this.m_btnHistoryDelete.Location = new System.Drawing.Point(170, 307);
			this.m_btnHistoryDelete.Name = "m_btnHistoryDelete";
			this.m_btnHistoryDelete.Size = new System.Drawing.Size(75, 23);
			this.m_btnHistoryDelete.TabIndex = 10;
			this.m_btnHistoryDelete.Text = "&Delete";
			this.m_btnHistoryDelete.UseVisualStyleBackColor = true;
			this.m_btnHistoryDelete.Click += new System.EventHandler(this.OnBtnHistoryDelete);
			// 
			// m_btnHistoryView
			// 
			this.m_btnHistoryView.Location = new System.Drawing.Point(8, 307);
			this.m_btnHistoryView.Name = "m_btnHistoryView";
			this.m_btnHistoryView.Size = new System.Drawing.Size(75, 23);
			this.m_btnHistoryView.TabIndex = 8;
			this.m_btnHistoryView.Text = "&View";
			this.m_btnHistoryView.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.m_btnHistoryView.UseVisualStyleBackColor = true;
			this.m_btnHistoryView.Click += new System.EventHandler(this.OnBtnHistoryView);
			// 
			// m_btnHistoryRestore
			// 
			this.m_btnHistoryRestore.Location = new System.Drawing.Point(382, 307);
			this.m_btnHistoryRestore.Name = "m_btnHistoryRestore";
			this.m_btnHistoryRestore.Size = new System.Drawing.Size(75, 23);
			this.m_btnHistoryRestore.TabIndex = 11;
			this.m_btnHistoryRestore.Text = "&Restore";
			this.m_btnHistoryRestore.UseVisualStyleBackColor = true;
			this.m_btnHistoryRestore.Click += new System.EventHandler(this.OnBtnHistoryRestore);
			// 
			// m_lvHistory
			// 
			this.m_lvHistory.FullRowSelect = true;
			this.m_lvHistory.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.m_lvHistory.HideSelection = false;
			this.m_lvHistory.Location = new System.Drawing.Point(9, 95);
			this.m_lvHistory.Name = "m_lvHistory";
			this.m_lvHistory.ShowItemToolTips = true;
			this.m_lvHistory.Size = new System.Drawing.Size(447, 206);
			this.m_lvHistory.TabIndex = 7;
			this.m_lvHistory.UseCompatibleStateImageBehavior = false;
			this.m_lvHistory.View = System.Windows.Forms.View.Details;
			this.m_lvHistory.ItemActivate += new System.EventHandler(this.OnHistoryItemActivate);
			this.m_lvHistory.SelectedIndexChanged += new System.EventHandler(this.OnHistorySelectedIndexChanged);
			// 
			// m_btnTools
			// 
			this.m_btnTools.Location = new System.Drawing.Point(10, 453);
			this.m_btnTools.Name = "m_btnTools";
			this.m_btnTools.Size = new System.Drawing.Size(80, 23);
			this.m_btnTools.TabIndex = 4;
			this.m_btnTools.Text = "T&ools";
			this.m_btnTools.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.m_btnTools.UseVisualStyleBackColor = true;
			this.m_btnTools.Click += new System.EventHandler(this.OnBtnTools);
			// 
			// m_bannerImage
			// 
			this.m_bannerImage.Dock = System.Windows.Forms.DockStyle.Top;
			this.m_bannerImage.Location = new System.Drawing.Point(0, 0);
			this.m_bannerImage.Name = "m_bannerImage";
			this.m_bannerImage.Size = new System.Drawing.Size(489, 60);
			this.m_bannerImage.TabIndex = 16;
			this.m_bannerImage.TabStop = false;
			// 
			// m_ctxTools
			// 
			this.m_ctxTools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_ctxToolsHelp,
            this.m_ctxToolsSep0,
            this.m_ctxToolsCopyInitialPassword,
            this.m_ctxToolsSep1,
            this.m_ctxToolsUrlHelp,
            this.m_ctxToolsUrlSelApp,
            this.m_ctxToolsUrlSelDoc,
            this.m_ctxToolsSep2,
            this.m_ctxToolsFieldRefs,
            this.m_ctxToolsSep3,
            this.m_ctxToolsOtpGen});
			this.m_ctxTools.Name = "m_ctxTools";
			this.m_ctxTools.Size = new System.Drawing.Size(234, 182);
			// 
			// m_ctxToolsHelp
			// 
			this.m_ctxToolsHelp.Image = global::KeePass.Properties.Resources.B16x16_Help;
			this.m_ctxToolsHelp.Name = "m_ctxToolsHelp";
			this.m_ctxToolsHelp.Size = new System.Drawing.Size(233, 22);
			this.m_ctxToolsHelp.Text = "&Help";
			this.m_ctxToolsHelp.Click += new System.EventHandler(this.OnCtxToolsHelp);
			// 
			// m_ctxToolsSep0
			// 
			this.m_ctxToolsSep0.Name = "m_ctxToolsSep0";
			this.m_ctxToolsSep0.Size = new System.Drawing.Size(230, 6);
			// 
			// m_ctxToolsCopyInitialPassword
			// 
			this.m_ctxToolsCopyInitialPassword.Image = global::KeePass.Properties.Resources.B16x16_KGPG_Info;
			this.m_ctxToolsCopyInitialPassword.Name = "m_ctxToolsCopyInitialPassword";
			this.m_ctxToolsCopyInitialPassword.Size = new System.Drawing.Size(233, 22);
			this.m_ctxToolsCopyInitialPassword.Text = "Copy Initial &Password";
			this.m_ctxToolsCopyInitialPassword.Click += new System.EventHandler(this.OnCtxToolsCopyInitialPassword);
			// 
			// m_ctxToolsSep1
			// 
			this.m_ctxToolsSep1.Name = "m_ctxToolsSep1";
			this.m_ctxToolsSep1.Size = new System.Drawing.Size(230, 6);
			// 
			// m_ctxToolsUrlHelp
			// 
			this.m_ctxToolsUrlHelp.Image = global::KeePass.Properties.Resources.B16x16_Help;
			this.m_ctxToolsUrlHelp.Name = "m_ctxToolsUrlHelp";
			this.m_ctxToolsUrlHelp.Size = new System.Drawing.Size(233, 22);
			this.m_ctxToolsUrlHelp.Text = "&URL Field: Help";
			this.m_ctxToolsUrlHelp.Click += new System.EventHandler(this.OnCtxUrlHelp);
			// 
			// m_ctxToolsUrlSelApp
			// 
			this.m_ctxToolsUrlSelApp.Image = global::KeePass.Properties.Resources.B16x16_View_Detailed;
			this.m_ctxToolsUrlSelApp.Name = "m_ctxToolsUrlSelApp";
			this.m_ctxToolsUrlSelApp.Size = new System.Drawing.Size(233, 22);
			this.m_ctxToolsUrlSelApp.Text = "URL Field: Select &Application...";
			this.m_ctxToolsUrlSelApp.Click += new System.EventHandler(this.OnCtxUrlSelApp);
			// 
			// m_ctxToolsUrlSelDoc
			// 
			this.m_ctxToolsUrlSelDoc.Image = global::KeePass.Properties.Resources.B16x16_CompFile;
			this.m_ctxToolsUrlSelDoc.Name = "m_ctxToolsUrlSelDoc";
			this.m_ctxToolsUrlSelDoc.Size = new System.Drawing.Size(233, 22);
			this.m_ctxToolsUrlSelDoc.Text = "URL Field: Select &Document...";
			this.m_ctxToolsUrlSelDoc.Click += new System.EventHandler(this.OnCtxUrlSelDoc);
			// 
			// m_ctxToolsSep2
			// 
			this.m_ctxToolsSep2.Name = "m_ctxToolsSep2";
			this.m_ctxToolsSep2.Size = new System.Drawing.Size(230, 6);
			// 
			// m_ctxToolsFieldRefs
			// 
			this.m_ctxToolsFieldRefs.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_ctxToolsFieldRefsInTitle,
            this.m_ctxToolsFieldRefsInUserName,
            this.m_ctxToolsFieldRefsInPassword,
            this.m_ctxToolsFieldRefsInUrl,
            this.m_ctxToolsFieldRefsInNotes});
			this.m_ctxToolsFieldRefs.Name = "m_ctxToolsFieldRefs";
			this.m_ctxToolsFieldRefs.Size = new System.Drawing.Size(233, 22);
			this.m_ctxToolsFieldRefs.Text = "Insert Field &Reference";
			// 
			// m_ctxToolsFieldRefsInTitle
			// 
			this.m_ctxToolsFieldRefsInTitle.Name = "m_ctxToolsFieldRefsInTitle";
			this.m_ctxToolsFieldRefsInTitle.Size = new System.Drawing.Size(173, 22);
			this.m_ctxToolsFieldRefsInTitle.Text = "In &Title Field";
			this.m_ctxToolsFieldRefsInTitle.Click += new System.EventHandler(this.OnFieldRefInTitle);
			// 
			// m_ctxToolsFieldRefsInUserName
			// 
			this.m_ctxToolsFieldRefsInUserName.Name = "m_ctxToolsFieldRefsInUserName";
			this.m_ctxToolsFieldRefsInUserName.Size = new System.Drawing.Size(173, 22);
			this.m_ctxToolsFieldRefsInUserName.Text = "In &User Name Field";
			this.m_ctxToolsFieldRefsInUserName.Click += new System.EventHandler(this.OnFieldRefInUserName);
			// 
			// m_ctxToolsFieldRefsInPassword
			// 
			this.m_ctxToolsFieldRefsInPassword.Name = "m_ctxToolsFieldRefsInPassword";
			this.m_ctxToolsFieldRefsInPassword.Size = new System.Drawing.Size(173, 22);
			this.m_ctxToolsFieldRefsInPassword.Text = "In &Password Field";
			this.m_ctxToolsFieldRefsInPassword.Click += new System.EventHandler(this.OnFieldRefInPassword);
			// 
			// m_ctxToolsFieldRefsInUrl
			// 
			this.m_ctxToolsFieldRefsInUrl.Name = "m_ctxToolsFieldRefsInUrl";
			this.m_ctxToolsFieldRefsInUrl.Size = new System.Drawing.Size(173, 22);
			this.m_ctxToolsFieldRefsInUrl.Text = "In UR&L Field";
			this.m_ctxToolsFieldRefsInUrl.Click += new System.EventHandler(this.OnFieldRefInUrl);
			// 
			// m_ctxToolsFieldRefsInNotes
			// 
			this.m_ctxToolsFieldRefsInNotes.Name = "m_ctxToolsFieldRefsInNotes";
			this.m_ctxToolsFieldRefsInNotes.Size = new System.Drawing.Size(173, 22);
			this.m_ctxToolsFieldRefsInNotes.Text = "In &Notes Field";
			this.m_ctxToolsFieldRefsInNotes.Click += new System.EventHandler(this.OnFieldRefInNotes);
			// 
			// m_ctxToolsSep3
			// 
			this.m_ctxToolsSep3.Name = "m_ctxToolsSep3";
			this.m_ctxToolsSep3.Size = new System.Drawing.Size(230, 6);
			// 
			// m_ctxToolsOtpGen
			// 
			this.m_ctxToolsOtpGen.Image = global::KeePass.Properties.Resources.B16x16_KGPG_Gen;
			this.m_ctxToolsOtpGen.Name = "m_ctxToolsOtpGen";
			this.m_ctxToolsOtpGen.Size = new System.Drawing.Size(233, 22);
			this.m_ctxToolsOtpGen.Text = "&OTP Generator Settings...";
			this.m_ctxToolsOtpGen.Click += new System.EventHandler(this.OnCtxToolsOtpGen);
			// 
			// m_ctxBinAttach
			// 
			this.m_ctxBinAttach.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_ctxBinImportFile,
            this.m_ctxBinSep0,
            this.m_ctxBinNew});
			this.m_ctxBinAttach.Name = "m_ctxBinAttach";
			this.m_ctxBinAttach.Size = new System.Drawing.Size(212, 54);
			// 
			// m_ctxBinImportFile
			// 
			this.m_ctxBinImportFile.Image = global::KeePass.Properties.Resources.B16x16_Folder_Yellow_Open;
			this.m_ctxBinImportFile.Name = "m_ctxBinImportFile";
			this.m_ctxBinImportFile.Size = new System.Drawing.Size(211, 22);
			this.m_ctxBinImportFile.Text = "Attach &File(s)...";
			this.m_ctxBinImportFile.Click += new System.EventHandler(this.OnCtxBinImport);
			// 
			// m_ctxBinSep0
			// 
			this.m_ctxBinSep0.Name = "m_ctxBinSep0";
			this.m_ctxBinSep0.Size = new System.Drawing.Size(208, 6);
			// 
			// m_ctxBinNew
			// 
			this.m_ctxBinNew.Image = global::KeePass.Properties.Resources.B16x16_FileNew;
			this.m_ctxBinNew.Name = "m_ctxBinNew";
			this.m_ctxBinNew.Size = new System.Drawing.Size(211, 22);
			this.m_ctxBinNew.Text = "&Create Empty Attachment";
			this.m_ctxBinNew.Click += new System.EventHandler(this.OnCtxBinNew);
			// 
			// PwEntryForm
			// 
			this.AcceptButton = this.m_btnOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(489, 486);
			this.Controls.Add(this.m_tabMain);
			this.Controls.Add(this.m_lblSeparator);
			this.Controls.Add(this.m_bannerImage);
			this.Controls.Add(this.m_btnTools);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOK);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "PwEntryForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "<DYN>";
			this.Load += new System.EventHandler(this.OnFormLoad);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
			this.m_tabMain.ResumeLayout(false);
			this.m_tabEntry.ResumeLayout(false);
			this.m_tabEntry.PerformLayout();
			this.m_ctxDefaultTimes.ResumeLayout(false);
			this.m_tabAdvanced.ResumeLayout(false);
			this.m_grpAttachments.ResumeLayout(false);
			this.m_grpStringFields.ResumeLayout(false);
			this.m_ctxStr.ResumeLayout(false);
			this.m_tabProperties.ResumeLayout(false);
			this.m_tabProperties.PerformLayout();
			this.m_tabAutoType.ResumeLayout(false);
			this.m_tabAutoType.PerformLayout();
			this.m_ctxAutoType.ResumeLayout(false);
			this.m_tabHistory.ResumeLayout(false);
			this.m_tabHistory.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
			this.m_ctxTools.ResumeLayout(false);
			this.m_ctxBinAttach.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label m_lblUserName;
		private System.Windows.Forms.Label m_lblPassword;
		private System.Windows.Forms.Label m_lblTitle;
		private System.Windows.Forms.Label m_lblPasswordRepeat;
		private System.Windows.Forms.Label m_lblUrl;
		private System.Windows.Forms.Label m_lblNotes;
		private System.Windows.Forms.Label m_lblQuality;
		private System.Windows.Forms.TextBox m_tbTitle;
		private System.Windows.Forms.Button m_btnIcon;
		private System.Windows.Forms.Label m_lblIcon;
		private System.Windows.Forms.TextBox m_tbUserName;
		private KeePass.UI.SecureTextBoxEx m_tbPassword;
		private KeePass.UI.SecureTextBoxEx m_tbRepeatPassword;
		private KeePass.UI.QualityProgressBar m_pbQuality;
		private System.Windows.Forms.TextBox m_tbUrl;
		private KeePass.UI.CustomRichTextBoxEx m_rtNotes;
		private System.Windows.Forms.CheckBox m_cbExpires;
		private System.Windows.Forms.DateTimePicker m_dtExpireDateTime;
		private System.Windows.Forms.Button m_btnStandardExpires;
		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.CheckBox m_cbHidePassword;
		private System.Windows.Forms.Button m_btnGenPw;
		private System.Windows.Forms.Label m_lblQualityInfo;
		private System.Windows.Forms.PictureBox m_bannerImage;
		private System.Windows.Forms.ToolTip m_ttRect;
		private System.Windows.Forms.Label m_lblSeparator;
		private System.Windows.Forms.ToolTip m_ttBalloon;
		private System.Windows.Forms.TabControl m_tabMain;
		private System.Windows.Forms.TabPage m_tabEntry;
		private System.Windows.Forms.TabPage m_tabAdvanced;
		private System.Windows.Forms.TabPage m_tabAutoType;
		private System.Windows.Forms.TabPage m_tabHistory;
		private System.Windows.Forms.GroupBox m_grpAttachments;
		private System.Windows.Forms.Button m_btnBinSave;
		private System.Windows.Forms.Button m_btnBinDelete;
		private System.Windows.Forms.Button m_btnBinAdd;
		private KeePass.UI.CustomListViewEx m_lvBinaries;
		private System.Windows.Forms.GroupBox m_grpStringFields;
		private System.Windows.Forms.Button m_btnStrEdit;
		private System.Windows.Forms.Button m_btnStrDelete;
		private System.Windows.Forms.Button m_btnStrAdd;
		private KeePass.UI.CustomListViewEx m_lvStrings;
		private System.Windows.Forms.Button m_btnTools;
		private System.Windows.Forms.Button m_btnAutoTypeEdit;
		private System.Windows.Forms.Button m_btnAutoTypeAdd;
		private System.Windows.Forms.Button m_btnAutoTypeDelete;
		private KeePass.UI.CustomListViewEx m_lvAutoType;
		private System.Windows.Forms.Button m_btnHistoryRestore;
		private System.Windows.Forms.Button m_btnHistoryView;
		private KeePass.UI.CustomListViewEx m_lvHistory;
		private System.Windows.Forms.Button m_btnHistoryDelete;
		private KeePass.UI.CustomContextMenuStripEx m_ctxDefaultTimes;
		private System.Windows.Forms.ToolStripMenuItem m_menuExpireNow;
		private System.Windows.Forms.ToolStripSeparator m_menuExpireSep0;
		private System.Windows.Forms.ToolStripMenuItem m_menuExpire1Week;
		private System.Windows.Forms.ToolStripMenuItem m_menuExpire2Weeks;
		private System.Windows.Forms.ToolStripSeparator m_menuExpireSep1;
		private System.Windows.Forms.ToolStripMenuItem m_menuExpire1Month;
		private System.Windows.Forms.ToolStripMenuItem m_menuExpire3Months;
		private System.Windows.Forms.ToolStripMenuItem m_menuExpire6Months;
		private System.Windows.Forms.ToolStripSeparator m_menuExpireSep2;
		private System.Windows.Forms.ToolStripMenuItem m_menuExpire1Year;
		private KeePass.UI.CustomContextMenuStripEx m_ctxAutoType;
		private System.Windows.Forms.ToolStripMenuItem m_ctxAutoTypeCopySeq;
		private System.Windows.Forms.TextBox m_tbDefaultAutoTypeSeq;
		private System.Windows.Forms.CheckBox m_cbAutoTypeEnabled;
		private System.Windows.Forms.Label m_lblCustomAutoType;
		private System.Windows.Forms.RadioButton m_rbAutoTypeOverride;
		private System.Windows.Forms.RadioButton m_rbAutoTypeSeqInherit;
		private System.Windows.Forms.Button m_btnAutoTypeEditDefault;
		private System.Windows.Forms.Button m_btnStrMore;
		private System.Windows.Forms.TabPage m_tabProperties;
		private KeePass.UI.ColorButtonEx m_btnPickBgColor;
		private System.Windows.Forms.CheckBox m_cbCustomBackgroundColor;
		private System.Windows.Forms.Label m_lblOverrideUrl;
		private System.Windows.Forms.LinkLabel m_linkAutoTypeObfuscation;
		private System.Windows.Forms.CheckBox m_cbAutoTypeObfuscation;
		private KeePass.UI.SplitButtonEx m_btnBinOpen;
		private System.Windows.Forms.TextBox m_tbUuid;
		private System.Windows.Forms.Label m_lblUuid;
		private KeePass.UI.CustomContextMenuStripEx m_ctxTools;
		private System.Windows.Forms.ToolStripMenuItem m_ctxToolsHelp;
		private System.Windows.Forms.ToolStripSeparator m_ctxToolsSep0;
		private System.Windows.Forms.ToolStripMenuItem m_ctxToolsUrlHelp;
		private System.Windows.Forms.ToolStripMenuItem m_ctxToolsUrlSelApp;
		private System.Windows.Forms.ToolStripMenuItem m_ctxToolsUrlSelDoc;
		private System.Windows.Forms.ToolStripSeparator m_ctxToolsSep1;
		private System.Windows.Forms.ToolStripMenuItem m_ctxToolsFieldRefs;
		private System.Windows.Forms.ToolStripMenuItem m_ctxToolsFieldRefsInTitle;
		private System.Windows.Forms.ToolStripMenuItem m_ctxToolsFieldRefsInUserName;
		private System.Windows.Forms.ToolStripMenuItem m_ctxToolsFieldRefsInPassword;
		private System.Windows.Forms.ToolStripMenuItem m_ctxToolsFieldRefsInUrl;
		private System.Windows.Forms.ToolStripMenuItem m_ctxToolsFieldRefsInNotes;
		private KeePass.UI.ColorButtonEx m_btnPickFgColor;
		private System.Windows.Forms.CheckBox m_cbCustomForegroundColor;
		private System.Windows.Forms.TextBox m_tbTags;
		private System.Windows.Forms.Label m_lblTags;
		private KeePass.UI.CustomContextMenuStripEx m_ctxBinAttach;
		private System.Windows.Forms.ToolStripMenuItem m_ctxBinImportFile;
		private System.Windows.Forms.ToolStripSeparator m_ctxBinSep0;
		private System.Windows.Forms.ToolStripMenuItem m_ctxBinNew;
		private KeePass.UI.ImageComboBoxEx m_cmbOverrideUrl;
		private System.Windows.Forms.Button m_btnAutoTypeDown;
		private System.Windows.Forms.Button m_btnAutoTypeUp;
		private System.Windows.Forms.Label m_lblCustomData;
		private System.Windows.Forms.Button m_btnCDDel;
		private KeePass.UI.CustomListViewEx m_lvCustomData;
		private System.Windows.Forms.Label m_lblModifiedData;
		private System.Windows.Forms.Label m_lblModified;
		private System.Windows.Forms.Label m_lblCreatedData;
		private System.Windows.Forms.Label m_lblCreated;
		private System.Windows.Forms.Label m_lblVersions;
		private System.Windows.Forms.LinkLabel m_linkTagsInh;
		private System.Windows.Forms.Button m_btnTags;
		private System.Windows.Forms.CheckBox m_cbQualityCheck;
		private KeePass.UI.CustomContextMenuStripEx m_ctxStr;
		private System.Windows.Forms.ToolStripSeparator m_ctxStrSep0;
		private System.Windows.Forms.ToolStripMenuItem m_ctxStrMoveTo;
		private System.Windows.Forms.ToolStripMenuItem m_ctxStrMoveToTitle;
		private System.Windows.Forms.ToolStripMenuItem m_ctxStrMoveToUserName;
		private System.Windows.Forms.ToolStripMenuItem m_ctxStrMoveToPassword;
		private System.Windows.Forms.ToolStripMenuItem m_ctxStrMoveToUrl;
		private System.Windows.Forms.ToolStripMenuItem m_ctxStrMoveToNotes;
		private System.Windows.Forms.ToolStripMenuItem m_ctxStrCopyName;
		private System.Windows.Forms.ToolStripMenuItem m_ctxStrCopyValue;
		private System.Windows.Forms.ToolStripSeparator m_ctxStrSep1;
		private System.Windows.Forms.ToolStripMenuItem m_ctxStrCopyItem;
		private System.Windows.Forms.ToolStripMenuItem m_ctxStrPasteItem;
		private System.Windows.Forms.ToolStripMenuItem m_ctxAutoTypeCopyWnd;
		private System.Windows.Forms.ToolStripSeparator m_ctxAutoTypeSep0;
		private System.Windows.Forms.ToolStripMenuItem m_ctxAutoTypeCopyItem;
		private System.Windows.Forms.ToolStripMenuItem m_ctxAutoTypePasteItem;
		private System.Windows.Forms.ToolStripMenuItem m_ctxAutoTypeDup;
		private System.Windows.Forms.Button m_btnAutoTypeMore;
		private System.Windows.Forms.ToolStripMenuItem m_ctxStrSelectAll;
		private System.Windows.Forms.ToolStripSeparator m_ctxStrSep2;
		private System.Windows.Forms.ToolStripSeparator m_ctxAutoTypeSep1;
		private System.Windows.Forms.ToolStripMenuItem m_ctxAutoTypeSelectAll;
		private System.Windows.Forms.ToolStripSeparator m_ctxStrSep3;
		private System.Windows.Forms.ToolStripMenuItem m_ctxStrOtpGen;
		private System.Windows.Forms.ToolStripSeparator m_ctxToolsSep2;
		private System.Windows.Forms.ToolStripMenuItem m_ctxToolsOtpGen;
		private System.Windows.Forms.Label m_lblPasswordModified;
		private System.Windows.Forms.Label m_lblPasswordModifiedData;
		private System.Windows.Forms.ToolStripMenuItem m_ctxToolsCopyInitialPassword;
		private System.Windows.Forms.ToolStripSeparator m_ctxToolsSep3;
		private System.Windows.Forms.Button m_btnHistoryCompare;
	}
}