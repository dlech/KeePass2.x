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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PwEntryForm));
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
			this.m_tbPassword = new System.Windows.Forms.TextBox();
			this.m_tbRepeatPassword = new System.Windows.Forms.TextBox();
			this.m_tbUrl = new System.Windows.Forms.TextBox();
			this.m_rtNotes = new System.Windows.Forms.RichTextBox();
			this.m_cbExpires = new System.Windows.Forms.CheckBox();
			this.m_dtExpireDateTime = new System.Windows.Forms.DateTimePicker();
			this.m_ctxDefaultTimes = new System.Windows.Forms.ContextMenuStrip(this.components);
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
			this.m_btnOK = new System.Windows.Forms.Button();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_lblQualityBitsText = new System.Windows.Forms.Label();
			this.m_ttRect = new System.Windows.Forms.ToolTip(this.components);
			this.m_btnGenPw = new System.Windows.Forms.Button();
			this.m_cbHidePassword = new System.Windows.Forms.CheckBox();
			this.m_btnStandardExpires = new System.Windows.Forms.Button();
			this.m_lblSeparator = new System.Windows.Forms.Label();
			this.m_ttBalloon = new System.Windows.Forms.ToolTip(this.components);
			this.m_ttValidationError = new System.Windows.Forms.ToolTip(this.components);
			this.m_tabMain = new System.Windows.Forms.TabControl();
			this.m_tabEntry = new System.Windows.Forms.TabPage();
			this.m_pbQuality = new KeePass.UI.QualityProgressBar();
			this.m_tabAdvanced = new System.Windows.Forms.TabPage();
			this.m_grpAttachments = new System.Windows.Forms.GroupBox();
			this.m_btnBinSave = new System.Windows.Forms.Button();
			this.m_btnBinDelete = new System.Windows.Forms.Button();
			this.m_btnBinAdd = new System.Windows.Forms.Button();
			this.m_lvBinaries = new System.Windows.Forms.ListView();
			this.m_ctxListOperations = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.m_menuListCtxCopyFieldValue = new System.Windows.Forms.ToolStripMenuItem();
			this.m_grpStringFields = new System.Windows.Forms.GroupBox();
			this.m_btnStrMove = new System.Windows.Forms.Button();
			this.m_btnStrAdd = new System.Windows.Forms.Button();
			this.m_btnStrEdit = new System.Windows.Forms.Button();
			this.m_btnStrDelete = new System.Windows.Forms.Button();
			this.m_lvStrings = new System.Windows.Forms.ListView();
			this.m_tabProperties = new System.Windows.Forms.TabPage();
			this.m_tbOverrideUrl = new System.Windows.Forms.TextBox();
			this.m_lblOverrideUrl = new System.Windows.Forms.Label();
			this.m_cbCustomBackgroundColor = new System.Windows.Forms.CheckBox();
			this.m_btnPickBgColor = new System.Windows.Forms.Button();
			this.m_tabAutoType = new System.Windows.Forms.TabPage();
			this.m_btnAutoTypeEditDefault = new System.Windows.Forms.Button();
			this.m_rbAutoTypeOverride = new System.Windows.Forms.RadioButton();
			this.m_rbAutoTypeSeqInherit = new System.Windows.Forms.RadioButton();
			this.m_lblCustomAutoType = new System.Windows.Forms.Label();
			this.m_cbAutoTypeEnabled = new System.Windows.Forms.CheckBox();
			this.m_tbDefaultAutoTypeSeq = new System.Windows.Forms.TextBox();
			this.m_btnAutoTypeEdit = new System.Windows.Forms.Button();
			this.m_btnAutoTypeAdd = new System.Windows.Forms.Button();
			this.m_btnAutoTypeDelete = new System.Windows.Forms.Button();
			this.m_lvAutoType = new System.Windows.Forms.ListView();
			this.m_tabHistory = new System.Windows.Forms.TabPage();
			this.m_btnHistoryDelete = new System.Windows.Forms.Button();
			this.m_btnHistoryRestore = new System.Windows.Forms.Button();
			this.m_btnHistoryView = new System.Windows.Forms.Button();
			this.m_lvHistory = new System.Windows.Forms.ListView();
			this.m_btnHelp = new System.Windows.Forms.Button();
			this.m_bannerImage = new System.Windows.Forms.PictureBox();
			this.m_dlgAttachFile = new System.Windows.Forms.OpenFileDialog();
			this.m_dlgSaveAttachedFile = new System.Windows.Forms.SaveFileDialog();
			this.m_dlgSaveAttachedFiles = new System.Windows.Forms.FolderBrowserDialog();
			this.m_ctxStrMoveToStandard = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.m_menuListCtxMoveStandardTitle = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuListCtxMoveStandardUser = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuListCtxMoveStandardPassword = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuListCtxMoveStandardURL = new System.Windows.Forms.ToolStripMenuItem();
			this.m_menuListCtxMoveStandardNotes = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxPwGen = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.m_ctxPwGenOpen = new System.Windows.Forms.ToolStripMenuItem();
			this.m_ctxPwGenSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_ctxPwGenProfiles = new System.Windows.Forms.ToolStripMenuItem();
			this.m_dlgColorSel = new System.Windows.Forms.ColorDialog();
			this.m_ctxDefaultTimes.SuspendLayout();
			this.m_tabMain.SuspendLayout();
			this.m_tabEntry.SuspendLayout();
			this.m_tabAdvanced.SuspendLayout();
			this.m_grpAttachments.SuspendLayout();
			this.m_ctxListOperations.SuspendLayout();
			this.m_grpStringFields.SuspendLayout();
			this.m_tabProperties.SuspendLayout();
			this.m_tabAutoType.SuspendLayout();
			this.m_tabHistory.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
			this.m_ctxStrMoveToStandard.SuspendLayout();
			this.m_ctxPwGen.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_lblUserName
			// 
			resources.ApplyResources(this.m_lblUserName, "m_lblUserName");
			this.m_lblUserName.Name = "m_lblUserName";
			// 
			// m_lblPassword
			// 
			resources.ApplyResources(this.m_lblPassword, "m_lblPassword");
			this.m_lblPassword.Name = "m_lblPassword";
			// 
			// m_lblTitle
			// 
			resources.ApplyResources(this.m_lblTitle, "m_lblTitle");
			this.m_lblTitle.Name = "m_lblTitle";
			// 
			// m_lblPasswordRepeat
			// 
			resources.ApplyResources(this.m_lblPasswordRepeat, "m_lblPasswordRepeat");
			this.m_lblPasswordRepeat.Name = "m_lblPasswordRepeat";
			// 
			// m_lblUrl
			// 
			resources.ApplyResources(this.m_lblUrl, "m_lblUrl");
			this.m_lblUrl.Name = "m_lblUrl";
			// 
			// m_lblNotes
			// 
			resources.ApplyResources(this.m_lblNotes, "m_lblNotes");
			this.m_lblNotes.Name = "m_lblNotes";
			// 
			// m_lblQuality
			// 
			resources.ApplyResources(this.m_lblQuality, "m_lblQuality");
			this.m_lblQuality.Name = "m_lblQuality";
			// 
			// m_tbTitle
			// 
			resources.ApplyResources(this.m_tbTitle, "m_tbTitle");
			this.m_tbTitle.Name = "m_tbTitle";
			// 
			// m_btnIcon
			// 
			resources.ApplyResources(this.m_btnIcon, "m_btnIcon");
			this.m_btnIcon.Name = "m_btnIcon";
			this.m_ttRect.SetToolTip(this.m_btnIcon, resources.GetString("m_btnIcon.ToolTip"));
			this.m_btnIcon.UseVisualStyleBackColor = true;
			this.m_btnIcon.Click += new System.EventHandler(this.OnBtnPickIcon);
			// 
			// m_lblIcon
			// 
			resources.ApplyResources(this.m_lblIcon, "m_lblIcon");
			this.m_lblIcon.Name = "m_lblIcon";
			// 
			// m_tbUserName
			// 
			resources.ApplyResources(this.m_tbUserName, "m_tbUserName");
			this.m_tbUserName.Name = "m_tbUserName";
			// 
			// m_tbPassword
			// 
			resources.ApplyResources(this.m_tbPassword, "m_tbPassword");
			this.m_tbPassword.Name = "m_tbPassword";
			// 
			// m_tbRepeatPassword
			// 
			resources.ApplyResources(this.m_tbRepeatPassword, "m_tbRepeatPassword");
			this.m_tbRepeatPassword.Name = "m_tbRepeatPassword";
			this.m_ttBalloon.SetToolTip(this.m_tbRepeatPassword, resources.GetString("m_tbRepeatPassword.ToolTip"));
			// 
			// m_tbUrl
			// 
			resources.ApplyResources(this.m_tbUrl, "m_tbUrl");
			this.m_tbUrl.Name = "m_tbUrl";
			// 
			// m_rtNotes
			// 
			this.m_rtNotes.AcceptsTab = true;
			resources.ApplyResources(this.m_rtNotes, "m_rtNotes");
			this.m_rtNotes.Name = "m_rtNotes";
			this.m_rtNotes.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.OnNotesLinkClicked);
			// 
			// m_cbExpires
			// 
			resources.ApplyResources(this.m_cbExpires, "m_cbExpires");
			this.m_cbExpires.Name = "m_cbExpires";
			this.m_cbExpires.UseVisualStyleBackColor = true;
			// 
			// m_dtExpireDateTime
			// 
			this.m_dtExpireDateTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			resources.ApplyResources(this.m_dtExpireDateTime, "m_dtExpireDateTime");
			this.m_dtExpireDateTime.Name = "m_dtExpireDateTime";
			this.m_dtExpireDateTime.ValueChanged += new System.EventHandler(this.OnExpireDateTimeChanged);
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
			resources.ApplyResources(this.m_ctxDefaultTimes, "m_ctxDefaultTimes");
			// 
			// m_menuExpireNow
			// 
			this.m_menuExpireNow.Name = "m_menuExpireNow";
			resources.ApplyResources(this.m_menuExpireNow, "m_menuExpireNow");
			this.m_menuExpireNow.Click += new System.EventHandler(this.OnMenuExpireNow);
			// 
			// m_menuExpireSep0
			// 
			this.m_menuExpireSep0.Name = "m_menuExpireSep0";
			resources.ApplyResources(this.m_menuExpireSep0, "m_menuExpireSep0");
			// 
			// m_menuExpire1Week
			// 
			this.m_menuExpire1Week.Name = "m_menuExpire1Week";
			resources.ApplyResources(this.m_menuExpire1Week, "m_menuExpire1Week");
			this.m_menuExpire1Week.Click += new System.EventHandler(this.OnMenuExpire1Week);
			// 
			// m_menuExpire2Weeks
			// 
			this.m_menuExpire2Weeks.Name = "m_menuExpire2Weeks";
			resources.ApplyResources(this.m_menuExpire2Weeks, "m_menuExpire2Weeks");
			this.m_menuExpire2Weeks.Click += new System.EventHandler(this.OnMenuExpire2Weeks);
			// 
			// m_menuExpireSep1
			// 
			this.m_menuExpireSep1.Name = "m_menuExpireSep1";
			resources.ApplyResources(this.m_menuExpireSep1, "m_menuExpireSep1");
			// 
			// m_menuExpire1Month
			// 
			this.m_menuExpire1Month.Name = "m_menuExpire1Month";
			resources.ApplyResources(this.m_menuExpire1Month, "m_menuExpire1Month");
			this.m_menuExpire1Month.Click += new System.EventHandler(this.OnMenuExpire1Month);
			// 
			// m_menuExpire3Months
			// 
			this.m_menuExpire3Months.Name = "m_menuExpire3Months";
			resources.ApplyResources(this.m_menuExpire3Months, "m_menuExpire3Months");
			this.m_menuExpire3Months.Click += new System.EventHandler(this.OnMenuExpire3Months);
			// 
			// m_menuExpire6Months
			// 
			this.m_menuExpire6Months.Name = "m_menuExpire6Months";
			resources.ApplyResources(this.m_menuExpire6Months, "m_menuExpire6Months");
			this.m_menuExpire6Months.Click += new System.EventHandler(this.OnMenuExpire6Months);
			// 
			// m_menuExpireSep2
			// 
			this.m_menuExpireSep2.Name = "m_menuExpireSep2";
			resources.ApplyResources(this.m_menuExpireSep2, "m_menuExpireSep2");
			// 
			// m_menuExpire1Year
			// 
			this.m_menuExpire1Year.Name = "m_menuExpire1Year";
			resources.ApplyResources(this.m_menuExpire1Year, "m_menuExpire1Year");
			this.m_menuExpire1Year.Click += new System.EventHandler(this.OnMenuExpire1Year);
			// 
			// m_btnOK
			// 
			this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			resources.ApplyResources(this.m_btnOK, "m_btnOK");
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.UseVisualStyleBackColor = true;
			this.m_btnOK.Click += new System.EventHandler(this.OnBtnOK);
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			resources.ApplyResources(this.m_btnCancel, "m_btnCancel");
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			this.m_btnCancel.Click += new System.EventHandler(this.OnBtnCancel);
			// 
			// m_lblQualityBitsText
			// 
			resources.ApplyResources(this.m_lblQualityBitsText, "m_lblQualityBitsText");
			this.m_lblQualityBitsText.Name = "m_lblQualityBitsText";
			// 
			// m_ttRect
			// 
			resources.ApplyResources(this.m_ttRect, "m_ttRect");
			// 
			// m_btnGenPw
			// 
			this.m_btnGenPw.Image = global::KeePass.Properties.Resources.B15x13_KGPG_Gen;
			resources.ApplyResources(this.m_btnGenPw, "m_btnGenPw");
			this.m_btnGenPw.Name = "m_btnGenPw";
			this.m_ttRect.SetToolTip(this.m_btnGenPw, resources.GetString("m_btnGenPw.ToolTip"));
			this.m_btnGenPw.UseVisualStyleBackColor = true;
			this.m_btnGenPw.Click += new System.EventHandler(this.OnPwGenClick);
			// 
			// m_cbHidePassword
			// 
			resources.ApplyResources(this.m_cbHidePassword, "m_cbHidePassword");
			this.m_cbHidePassword.Image = global::KeePass.Properties.Resources.B17x05_3BlackDots;
			this.m_cbHidePassword.Name = "m_cbHidePassword";
			this.m_ttRect.SetToolTip(this.m_cbHidePassword, resources.GetString("m_cbHidePassword.ToolTip"));
			this.m_cbHidePassword.UseVisualStyleBackColor = true;
			this.m_cbHidePassword.CheckedChanged += new System.EventHandler(this.OnCheckedHidePassword);
			// 
			// m_btnStandardExpires
			// 
			this.m_btnStandardExpires.ContextMenuStrip = this.m_ctxDefaultTimes;
			this.m_btnStandardExpires.Image = global::KeePass.Properties.Resources.B16x16_History;
			resources.ApplyResources(this.m_btnStandardExpires, "m_btnStandardExpires");
			this.m_btnStandardExpires.Name = "m_btnStandardExpires";
			this.m_ttRect.SetToolTip(this.m_btnStandardExpires, resources.GetString("m_btnStandardExpires.ToolTip"));
			this.m_btnStandardExpires.UseVisualStyleBackColor = true;
			this.m_btnStandardExpires.Click += new System.EventHandler(this.OnBtnStandardExpiresClick);
			// 
			// m_lblSeparator
			// 
			this.m_lblSeparator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			resources.ApplyResources(this.m_lblSeparator, "m_lblSeparator");
			this.m_lblSeparator.Name = "m_lblSeparator";
			// 
			// m_ttBalloon
			// 
			resources.ApplyResources(this.m_ttBalloon, "m_ttBalloon");
			// 
			// m_ttValidationError
			// 
			this.m_ttValidationError.AutoPopDelay = 32000;
			this.m_ttValidationError.InitialDelay = 250;
			this.m_ttValidationError.ReshowDelay = 100;
			this.m_ttValidationError.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Warning;
			this.m_ttValidationError.ToolTipTitle = "Validation Warning";
			// 
			// m_tabMain
			// 
			this.m_tabMain.Controls.Add(this.m_tabEntry);
			this.m_tabMain.Controls.Add(this.m_tabAdvanced);
			this.m_tabMain.Controls.Add(this.m_tabProperties);
			this.m_tabMain.Controls.Add(this.m_tabAutoType);
			this.m_tabMain.Controls.Add(this.m_tabHistory);
			resources.ApplyResources(this.m_tabMain, "m_tabMain");
			this.m_tabMain.Name = "m_tabMain";
			this.m_tabMain.SelectedIndex = 0;
			// 
			// m_tabEntry
			// 
			this.m_tabEntry.Controls.Add(this.m_lblTitle);
			this.m_tabEntry.Controls.Add(this.m_rtNotes);
			this.m_tabEntry.Controls.Add(this.m_cbExpires);
			this.m_tabEntry.Controls.Add(this.m_lblQualityBitsText);
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
			resources.ApplyResources(this.m_tabEntry, "m_tabEntry");
			this.m_tabEntry.Name = "m_tabEntry";
			this.m_tabEntry.UseVisualStyleBackColor = true;
			// 
			// m_pbQuality
			// 
			resources.ApplyResources(this.m_pbQuality, "m_pbQuality");
			this.m_pbQuality.Maximum = 100;
			this.m_pbQuality.Minimum = 0;
			this.m_pbQuality.Name = "m_pbQuality";
			this.m_pbQuality.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			this.m_pbQuality.Value = 50;
			// 
			// m_tabAdvanced
			// 
			this.m_tabAdvanced.Controls.Add(this.m_grpAttachments);
			this.m_tabAdvanced.Controls.Add(this.m_grpStringFields);
			resources.ApplyResources(this.m_tabAdvanced, "m_tabAdvanced");
			this.m_tabAdvanced.Name = "m_tabAdvanced";
			this.m_tabAdvanced.UseVisualStyleBackColor = true;
			// 
			// m_grpAttachments
			// 
			this.m_grpAttachments.Controls.Add(this.m_btnBinSave);
			this.m_grpAttachments.Controls.Add(this.m_btnBinDelete);
			this.m_grpAttachments.Controls.Add(this.m_btnBinAdd);
			this.m_grpAttachments.Controls.Add(this.m_lvBinaries);
			resources.ApplyResources(this.m_grpAttachments, "m_grpAttachments");
			this.m_grpAttachments.Name = "m_grpAttachments";
			this.m_grpAttachments.TabStop = false;
			// 
			// m_btnBinSave
			// 
			resources.ApplyResources(this.m_btnBinSave, "m_btnBinSave");
			this.m_btnBinSave.Name = "m_btnBinSave";
			this.m_btnBinSave.UseVisualStyleBackColor = true;
			this.m_btnBinSave.Click += new System.EventHandler(this.OnBtnBinSave);
			// 
			// m_btnBinDelete
			// 
			resources.ApplyResources(this.m_btnBinDelete, "m_btnBinDelete");
			this.m_btnBinDelete.Name = "m_btnBinDelete";
			this.m_btnBinDelete.UseVisualStyleBackColor = true;
			this.m_btnBinDelete.Click += new System.EventHandler(this.OnBtnBinDelete);
			// 
			// m_btnBinAdd
			// 
			resources.ApplyResources(this.m_btnBinAdd, "m_btnBinAdd");
			this.m_btnBinAdd.Name = "m_btnBinAdd";
			this.m_btnBinAdd.UseVisualStyleBackColor = true;
			this.m_btnBinAdd.Click += new System.EventHandler(this.OnBtnBinAdd);
			// 
			// m_lvBinaries
			// 
			this.m_lvBinaries.ContextMenuStrip = this.m_ctxListOperations;
			this.m_lvBinaries.FullRowSelect = true;
			this.m_lvBinaries.HideSelection = false;
			resources.ApplyResources(this.m_lvBinaries, "m_lvBinaries");
			this.m_lvBinaries.Name = "m_lvBinaries";
			this.m_lvBinaries.UseCompatibleStateImageBehavior = false;
			this.m_lvBinaries.View = System.Windows.Forms.View.Details;
			this.m_lvBinaries.SelectedIndexChanged += new System.EventHandler(this.OnBinariesSelectedIndexChanged);
			// 
			// m_ctxListOperations
			// 
			this.m_ctxListOperations.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuListCtxCopyFieldValue});
			this.m_ctxListOperations.Name = "m_ctxListOperations";
			resources.ApplyResources(this.m_ctxListOperations, "m_ctxListOperations");
			// 
			// m_menuListCtxCopyFieldValue
			// 
			this.m_menuListCtxCopyFieldValue.Image = global::KeePass.Properties.Resources.B16x16_EditCopy;
			this.m_menuListCtxCopyFieldValue.Name = "m_menuListCtxCopyFieldValue";
			resources.ApplyResources(this.m_menuListCtxCopyFieldValue, "m_menuListCtxCopyFieldValue");
			this.m_menuListCtxCopyFieldValue.Click += new System.EventHandler(this.OnCtxCopyFieldValue);
			// 
			// m_grpStringFields
			// 
			this.m_grpStringFields.Controls.Add(this.m_btnStrMove);
			this.m_grpStringFields.Controls.Add(this.m_btnStrAdd);
			this.m_grpStringFields.Controls.Add(this.m_btnStrEdit);
			this.m_grpStringFields.Controls.Add(this.m_btnStrDelete);
			this.m_grpStringFields.Controls.Add(this.m_lvStrings);
			resources.ApplyResources(this.m_grpStringFields, "m_grpStringFields");
			this.m_grpStringFields.Name = "m_grpStringFields";
			this.m_grpStringFields.TabStop = false;
			// 
			// m_btnStrMove
			// 
			resources.ApplyResources(this.m_btnStrMove, "m_btnStrMove");
			this.m_btnStrMove.Name = "m_btnStrMove";
			this.m_btnStrMove.UseVisualStyleBackColor = true;
			this.m_btnStrMove.Click += new System.EventHandler(this.OnBtnStrMove);
			// 
			// m_btnStrAdd
			// 
			resources.ApplyResources(this.m_btnStrAdd, "m_btnStrAdd");
			this.m_btnStrAdd.Name = "m_btnStrAdd";
			this.m_btnStrAdd.UseVisualStyleBackColor = true;
			this.m_btnStrAdd.Click += new System.EventHandler(this.OnBtnStrAdd);
			// 
			// m_btnStrEdit
			// 
			resources.ApplyResources(this.m_btnStrEdit, "m_btnStrEdit");
			this.m_btnStrEdit.Name = "m_btnStrEdit";
			this.m_btnStrEdit.UseVisualStyleBackColor = true;
			this.m_btnStrEdit.Click += new System.EventHandler(this.OnBtnStrEdit);
			// 
			// m_btnStrDelete
			// 
			resources.ApplyResources(this.m_btnStrDelete, "m_btnStrDelete");
			this.m_btnStrDelete.Name = "m_btnStrDelete";
			this.m_btnStrDelete.UseVisualStyleBackColor = true;
			this.m_btnStrDelete.Click += new System.EventHandler(this.OnBtnStrDelete);
			// 
			// m_lvStrings
			// 
			this.m_lvStrings.ContextMenuStrip = this.m_ctxListOperations;
			this.m_lvStrings.FullRowSelect = true;
			this.m_lvStrings.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.m_lvStrings.HideSelection = false;
			resources.ApplyResources(this.m_lvStrings, "m_lvStrings");
			this.m_lvStrings.Name = "m_lvStrings";
			this.m_lvStrings.UseCompatibleStateImageBehavior = false;
			this.m_lvStrings.View = System.Windows.Forms.View.Details;
			this.m_lvStrings.ItemActivate += new System.EventHandler(this.OnStringsItemActivate);
			this.m_lvStrings.SelectedIndexChanged += new System.EventHandler(this.OnStringsSelectedIndexChanged);
			// 
			// m_tabProperties
			// 
			this.m_tabProperties.Controls.Add(this.m_tbOverrideUrl);
			this.m_tabProperties.Controls.Add(this.m_lblOverrideUrl);
			this.m_tabProperties.Controls.Add(this.m_cbCustomBackgroundColor);
			this.m_tabProperties.Controls.Add(this.m_btnPickBgColor);
			resources.ApplyResources(this.m_tabProperties, "m_tabProperties");
			this.m_tabProperties.Name = "m_tabProperties";
			this.m_tabProperties.UseVisualStyleBackColor = true;
			// 
			// m_tbOverrideUrl
			// 
			resources.ApplyResources(this.m_tbOverrideUrl, "m_tbOverrideUrl");
			this.m_tbOverrideUrl.Name = "m_tbOverrideUrl";
			// 
			// m_lblOverrideUrl
			// 
			resources.ApplyResources(this.m_lblOverrideUrl, "m_lblOverrideUrl");
			this.m_lblOverrideUrl.Name = "m_lblOverrideUrl";
			// 
			// m_cbCustomBackgroundColor
			// 
			resources.ApplyResources(this.m_cbCustomBackgroundColor, "m_cbCustomBackgroundColor");
			this.m_cbCustomBackgroundColor.Name = "m_cbCustomBackgroundColor";
			this.m_cbCustomBackgroundColor.UseVisualStyleBackColor = true;
			this.m_cbCustomBackgroundColor.CheckedChanged += new System.EventHandler(this.OnCustomBackgroundColorCheckedChanged);
			// 
			// m_btnPickBgColor
			// 
			resources.ApplyResources(this.m_btnPickBgColor, "m_btnPickBgColor");
			this.m_btnPickBgColor.Name = "m_btnPickBgColor";
			this.m_btnPickBgColor.UseVisualStyleBackColor = true;
			this.m_btnPickBgColor.Click += new System.EventHandler(this.OnPickBackgroundColor);
			// 
			// m_tabAutoType
			// 
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
			resources.ApplyResources(this.m_tabAutoType, "m_tabAutoType");
			this.m_tabAutoType.Name = "m_tabAutoType";
			this.m_tabAutoType.UseVisualStyleBackColor = true;
			// 
			// m_btnAutoTypeEditDefault
			// 
			resources.ApplyResources(this.m_btnAutoTypeEditDefault, "m_btnAutoTypeEditDefault");
			this.m_btnAutoTypeEditDefault.Name = "m_btnAutoTypeEditDefault";
			this.m_btnAutoTypeEditDefault.UseVisualStyleBackColor = true;
			this.m_btnAutoTypeEditDefault.Click += new System.EventHandler(this.OnBtnAutoTypeEditDefault);
			// 
			// m_rbAutoTypeOverride
			// 
			resources.ApplyResources(this.m_rbAutoTypeOverride, "m_rbAutoTypeOverride");
			this.m_rbAutoTypeOverride.Name = "m_rbAutoTypeOverride";
			this.m_rbAutoTypeOverride.TabStop = true;
			this.m_rbAutoTypeOverride.UseVisualStyleBackColor = true;
			// 
			// m_rbAutoTypeSeqInherit
			// 
			resources.ApplyResources(this.m_rbAutoTypeSeqInherit, "m_rbAutoTypeSeqInherit");
			this.m_rbAutoTypeSeqInherit.Name = "m_rbAutoTypeSeqInherit";
			this.m_rbAutoTypeSeqInherit.TabStop = true;
			this.m_rbAutoTypeSeqInherit.UseVisualStyleBackColor = true;
			this.m_rbAutoTypeSeqInherit.CheckedChanged += new System.EventHandler(this.OnAutoTypeSeqInheritCheckedChanged);
			// 
			// m_lblCustomAutoType
			// 
			resources.ApplyResources(this.m_lblCustomAutoType, "m_lblCustomAutoType");
			this.m_lblCustomAutoType.Name = "m_lblCustomAutoType";
			// 
			// m_cbAutoTypeEnabled
			// 
			resources.ApplyResources(this.m_cbAutoTypeEnabled, "m_cbAutoTypeEnabled");
			this.m_cbAutoTypeEnabled.Name = "m_cbAutoTypeEnabled";
			this.m_cbAutoTypeEnabled.UseVisualStyleBackColor = true;
			this.m_cbAutoTypeEnabled.CheckedChanged += new System.EventHandler(this.OnAutoTypeEnableCheckedChanged);
			// 
			// m_tbDefaultAutoTypeSeq
			// 
			resources.ApplyResources(this.m_tbDefaultAutoTypeSeq, "m_tbDefaultAutoTypeSeq");
			this.m_tbDefaultAutoTypeSeq.Name = "m_tbDefaultAutoTypeSeq";
			// 
			// m_btnAutoTypeEdit
			// 
			resources.ApplyResources(this.m_btnAutoTypeEdit, "m_btnAutoTypeEdit");
			this.m_btnAutoTypeEdit.Name = "m_btnAutoTypeEdit";
			this.m_btnAutoTypeEdit.UseVisualStyleBackColor = true;
			this.m_btnAutoTypeEdit.Click += new System.EventHandler(this.OnBtnAutoTypeEdit);
			// 
			// m_btnAutoTypeAdd
			// 
			resources.ApplyResources(this.m_btnAutoTypeAdd, "m_btnAutoTypeAdd");
			this.m_btnAutoTypeAdd.Name = "m_btnAutoTypeAdd";
			this.m_btnAutoTypeAdd.UseVisualStyleBackColor = true;
			this.m_btnAutoTypeAdd.Click += new System.EventHandler(this.OnBtnAutoTypeAdd);
			// 
			// m_btnAutoTypeDelete
			// 
			resources.ApplyResources(this.m_btnAutoTypeDelete, "m_btnAutoTypeDelete");
			this.m_btnAutoTypeDelete.Name = "m_btnAutoTypeDelete";
			this.m_btnAutoTypeDelete.UseVisualStyleBackColor = true;
			this.m_btnAutoTypeDelete.Click += new System.EventHandler(this.OnBtnAutoTypeDelete);
			// 
			// m_lvAutoType
			// 
			this.m_lvAutoType.ContextMenuStrip = this.m_ctxListOperations;
			this.m_lvAutoType.FullRowSelect = true;
			this.m_lvAutoType.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.m_lvAutoType.HideSelection = false;
			resources.ApplyResources(this.m_lvAutoType, "m_lvAutoType");
			this.m_lvAutoType.Name = "m_lvAutoType";
			this.m_lvAutoType.UseCompatibleStateImageBehavior = false;
			this.m_lvAutoType.View = System.Windows.Forms.View.Details;
			this.m_lvAutoType.ItemActivate += new System.EventHandler(this.OnAutoTypeItemActivate);
			this.m_lvAutoType.SelectedIndexChanged += new System.EventHandler(this.OnAutoTypeSelectedIndexChanged);
			// 
			// m_tabHistory
			// 
			this.m_tabHistory.Controls.Add(this.m_btnHistoryDelete);
			this.m_tabHistory.Controls.Add(this.m_btnHistoryRestore);
			this.m_tabHistory.Controls.Add(this.m_btnHistoryView);
			this.m_tabHistory.Controls.Add(this.m_lvHistory);
			resources.ApplyResources(this.m_tabHistory, "m_tabHistory");
			this.m_tabHistory.Name = "m_tabHistory";
			this.m_tabHistory.UseVisualStyleBackColor = true;
			// 
			// m_btnHistoryDelete
			// 
			resources.ApplyResources(this.m_btnHistoryDelete, "m_btnHistoryDelete");
			this.m_btnHistoryDelete.Name = "m_btnHistoryDelete";
			this.m_btnHistoryDelete.UseVisualStyleBackColor = true;
			this.m_btnHistoryDelete.Click += new System.EventHandler(this.OnBtnHistoryDelete);
			// 
			// m_btnHistoryRestore
			// 
			resources.ApplyResources(this.m_btnHistoryRestore, "m_btnHistoryRestore");
			this.m_btnHistoryRestore.Name = "m_btnHistoryRestore";
			this.m_btnHistoryRestore.UseVisualStyleBackColor = true;
			this.m_btnHistoryRestore.Click += new System.EventHandler(this.OnBtnHistoryRestore);
			// 
			// m_btnHistoryView
			// 
			resources.ApplyResources(this.m_btnHistoryView, "m_btnHistoryView");
			this.m_btnHistoryView.Name = "m_btnHistoryView";
			this.m_btnHistoryView.UseVisualStyleBackColor = true;
			this.m_btnHistoryView.Click += new System.EventHandler(this.OnBtnHistoryView);
			// 
			// m_lvHistory
			// 
			this.m_lvHistory.FullRowSelect = true;
			this.m_lvHistory.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.m_lvHistory.HideSelection = false;
			resources.ApplyResources(this.m_lvHistory, "m_lvHistory");
			this.m_lvHistory.Name = "m_lvHistory";
			this.m_lvHistory.UseCompatibleStateImageBehavior = false;
			this.m_lvHistory.View = System.Windows.Forms.View.Details;
			this.m_lvHistory.SelectedIndexChanged += new System.EventHandler(this.OnHistorySelectedIndexChanged);
			// 
			// m_btnHelp
			// 
			resources.ApplyResources(this.m_btnHelp, "m_btnHelp");
			this.m_btnHelp.Name = "m_btnHelp";
			this.m_btnHelp.UseVisualStyleBackColor = true;
			this.m_btnHelp.Click += new System.EventHandler(this.OnBtnHelp);
			// 
			// m_bannerImage
			// 
			resources.ApplyResources(this.m_bannerImage, "m_bannerImage");
			this.m_bannerImage.Name = "m_bannerImage";
			this.m_bannerImage.TabStop = false;
			// 
			// m_dlgAttachFile
			// 
			this.m_dlgAttachFile.AddExtension = false;
			this.m_dlgAttachFile.Multiselect = true;
			this.m_dlgAttachFile.RestoreDirectory = true;
			this.m_dlgAttachFile.SupportMultiDottedExtensions = true;
			resources.ApplyResources(this.m_dlgAttachFile, "m_dlgAttachFile");
			// 
			// m_dlgSaveAttachedFile
			// 
			resources.ApplyResources(this.m_dlgSaveAttachedFile, "m_dlgSaveAttachedFile");
			// 
			// m_dlgSaveAttachedFiles
			// 
			resources.ApplyResources(this.m_dlgSaveAttachedFiles, "m_dlgSaveAttachedFiles");
			// 
			// m_ctxStrMoveToStandard
			// 
			this.m_ctxStrMoveToStandard.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_menuListCtxMoveStandardTitle,
            this.m_menuListCtxMoveStandardUser,
            this.m_menuListCtxMoveStandardPassword,
            this.m_menuListCtxMoveStandardURL,
            this.m_menuListCtxMoveStandardNotes});
			this.m_ctxStrMoveToStandard.Name = "m_ctxStrMoveToStandard";
			resources.ApplyResources(this.m_ctxStrMoveToStandard, "m_ctxStrMoveToStandard");
			// 
			// m_menuListCtxMoveStandardTitle
			// 
			this.m_menuListCtxMoveStandardTitle.Name = "m_menuListCtxMoveStandardTitle";
			resources.ApplyResources(this.m_menuListCtxMoveStandardTitle, "m_menuListCtxMoveStandardTitle");
			this.m_menuListCtxMoveStandardTitle.Click += new System.EventHandler(this.OnCtxMoveToTitle);
			// 
			// m_menuListCtxMoveStandardUser
			// 
			this.m_menuListCtxMoveStandardUser.Name = "m_menuListCtxMoveStandardUser";
			resources.ApplyResources(this.m_menuListCtxMoveStandardUser, "m_menuListCtxMoveStandardUser");
			this.m_menuListCtxMoveStandardUser.Click += new System.EventHandler(this.OnCtxMoveToUserName);
			// 
			// m_menuListCtxMoveStandardPassword
			// 
			this.m_menuListCtxMoveStandardPassword.Name = "m_menuListCtxMoveStandardPassword";
			resources.ApplyResources(this.m_menuListCtxMoveStandardPassword, "m_menuListCtxMoveStandardPassword");
			this.m_menuListCtxMoveStandardPassword.Click += new System.EventHandler(this.OnCtxMoveToPassword);
			// 
			// m_menuListCtxMoveStandardURL
			// 
			this.m_menuListCtxMoveStandardURL.Name = "m_menuListCtxMoveStandardURL";
			resources.ApplyResources(this.m_menuListCtxMoveStandardURL, "m_menuListCtxMoveStandardURL");
			this.m_menuListCtxMoveStandardURL.Click += new System.EventHandler(this.OnCtxMoveToURL);
			// 
			// m_menuListCtxMoveStandardNotes
			// 
			this.m_menuListCtxMoveStandardNotes.Name = "m_menuListCtxMoveStandardNotes";
			resources.ApplyResources(this.m_menuListCtxMoveStandardNotes, "m_menuListCtxMoveStandardNotes");
			this.m_menuListCtxMoveStandardNotes.Click += new System.EventHandler(this.OnCtxMoveToNotes);
			// 
			// m_ctxPwGen
			// 
			this.m_ctxPwGen.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_ctxPwGenOpen,
            this.m_ctxPwGenSep0,
            this.m_ctxPwGenProfiles});
			this.m_ctxPwGen.Name = "m_ctxPwGen";
			resources.ApplyResources(this.m_ctxPwGen, "m_ctxPwGen");
			// 
			// m_ctxPwGenOpen
			// 
			this.m_ctxPwGenOpen.Image = global::KeePass.Properties.Resources.B16x16_KGPG_Gen;
			this.m_ctxPwGenOpen.Name = "m_ctxPwGenOpen";
			resources.ApplyResources(this.m_ctxPwGenOpen, "m_ctxPwGenOpen");
			this.m_ctxPwGenOpen.Click += new System.EventHandler(this.OnPwGenOpen);
			// 
			// m_ctxPwGenSep0
			// 
			this.m_ctxPwGenSep0.Name = "m_ctxPwGenSep0";
			resources.ApplyResources(this.m_ctxPwGenSep0, "m_ctxPwGenSep0");
			// 
			// m_ctxPwGenProfiles
			// 
			this.m_ctxPwGenProfiles.Name = "m_ctxPwGenProfiles";
			resources.ApplyResources(this.m_ctxPwGenProfiles, "m_ctxPwGenProfiles");
			// 
			// m_dlgColorSel
			// 
			this.m_dlgColorSel.AnyColor = true;
			this.m_dlgColorSel.FullOpen = true;
			// 
			// PwEntryForm
			// 
			this.AcceptButton = this.m_btnOK;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.Controls.Add(this.m_btnHelp);
			this.Controls.Add(this.m_tabMain);
			this.Controls.Add(this.m_lblSeparator);
			this.Controls.Add(this.m_bannerImage);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOK);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "PwEntryForm";
			this.ShowInTaskbar = false;
			this.Load += new System.EventHandler(this.OnFormLoad);
			this.m_ctxDefaultTimes.ResumeLayout(false);
			this.m_tabMain.ResumeLayout(false);
			this.m_tabEntry.ResumeLayout(false);
			this.m_tabEntry.PerformLayout();
			this.m_tabAdvanced.ResumeLayout(false);
			this.m_grpAttachments.ResumeLayout(false);
			this.m_ctxListOperations.ResumeLayout(false);
			this.m_grpStringFields.ResumeLayout(false);
			this.m_tabProperties.ResumeLayout(false);
			this.m_tabProperties.PerformLayout();
			this.m_tabAutoType.ResumeLayout(false);
			this.m_tabAutoType.PerformLayout();
			this.m_tabHistory.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
			this.m_ctxStrMoveToStandard.ResumeLayout(false);
			this.m_ctxPwGen.ResumeLayout(false);
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
		private System.Windows.Forms.TextBox m_tbPassword;
		private System.Windows.Forms.TextBox m_tbRepeatPassword;
		private KeePass.UI.QualityProgressBar m_pbQuality;
		private System.Windows.Forms.TextBox m_tbUrl;
		private System.Windows.Forms.RichTextBox m_rtNotes;
		private System.Windows.Forms.CheckBox m_cbExpires;
		private System.Windows.Forms.DateTimePicker m_dtExpireDateTime;
		private System.Windows.Forms.Button m_btnStandardExpires;
		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.CheckBox m_cbHidePassword;
		private System.Windows.Forms.Button m_btnGenPw;
		private System.Windows.Forms.Label m_lblQualityBitsText;
		private System.Windows.Forms.PictureBox m_bannerImage;
		private System.Windows.Forms.ToolTip m_ttRect;
		private System.Windows.Forms.Label m_lblSeparator;
		private System.Windows.Forms.ToolTip m_ttBalloon;
		private System.Windows.Forms.ToolTip m_ttValidationError;
		private System.Windows.Forms.TabControl m_tabMain;
		private System.Windows.Forms.TabPage m_tabEntry;
		private System.Windows.Forms.TabPage m_tabAdvanced;
		private System.Windows.Forms.TabPage m_tabAutoType;
		private System.Windows.Forms.TabPage m_tabHistory;
		private System.Windows.Forms.GroupBox m_grpAttachments;
		private System.Windows.Forms.Button m_btnBinSave;
		private System.Windows.Forms.Button m_btnBinDelete;
		private System.Windows.Forms.Button m_btnBinAdd;
		private System.Windows.Forms.ListView m_lvBinaries;
		private System.Windows.Forms.GroupBox m_grpStringFields;
		private System.Windows.Forms.Button m_btnStrEdit;
		private System.Windows.Forms.Button m_btnStrDelete;
		private System.Windows.Forms.Button m_btnStrAdd;
		private System.Windows.Forms.ListView m_lvStrings;
		private System.Windows.Forms.Button m_btnHelp;
		private System.Windows.Forms.Button m_btnAutoTypeEdit;
		private System.Windows.Forms.Button m_btnAutoTypeAdd;
		private System.Windows.Forms.Button m_btnAutoTypeDelete;
		private System.Windows.Forms.ListView m_lvAutoType;
		private System.Windows.Forms.Button m_btnHistoryRestore;
		private System.Windows.Forms.Button m_btnHistoryView;
		private System.Windows.Forms.ListView m_lvHistory;
		private System.Windows.Forms.Button m_btnHistoryDelete;
		private System.Windows.Forms.ContextMenuStrip m_ctxDefaultTimes;
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
		private System.Windows.Forms.ContextMenuStrip m_ctxListOperations;
		private System.Windows.Forms.ToolStripMenuItem m_menuListCtxCopyFieldValue;
		private System.Windows.Forms.OpenFileDialog m_dlgAttachFile;
		private System.Windows.Forms.SaveFileDialog m_dlgSaveAttachedFile;
		private System.Windows.Forms.FolderBrowserDialog m_dlgSaveAttachedFiles;
		private System.Windows.Forms.TextBox m_tbDefaultAutoTypeSeq;
		private System.Windows.Forms.CheckBox m_cbAutoTypeEnabled;
		private System.Windows.Forms.Label m_lblCustomAutoType;
		private System.Windows.Forms.RadioButton m_rbAutoTypeOverride;
		private System.Windows.Forms.RadioButton m_rbAutoTypeSeqInherit;
		private System.Windows.Forms.Button m_btnAutoTypeEditDefault;
		private System.Windows.Forms.Button m_btnStrMove;
		private System.Windows.Forms.ContextMenuStrip m_ctxStrMoveToStandard;
		private System.Windows.Forms.ToolStripMenuItem m_menuListCtxMoveStandardTitle;
		private System.Windows.Forms.ToolStripMenuItem m_menuListCtxMoveStandardUser;
		private System.Windows.Forms.ToolStripMenuItem m_menuListCtxMoveStandardPassword;
		private System.Windows.Forms.ToolStripMenuItem m_menuListCtxMoveStandardURL;
		private System.Windows.Forms.ToolStripMenuItem m_menuListCtxMoveStandardNotes;
		private System.Windows.Forms.ContextMenuStrip m_ctxPwGen;
		private System.Windows.Forms.ToolStripMenuItem m_ctxPwGenOpen;
		private System.Windows.Forms.ToolStripSeparator m_ctxPwGenSep0;
		private System.Windows.Forms.ToolStripMenuItem m_ctxPwGenProfiles;
		private System.Windows.Forms.TabPage m_tabProperties;
		private System.Windows.Forms.Button m_btnPickBgColor;
		private System.Windows.Forms.ColorDialog m_dlgColorSel;
		private System.Windows.Forms.CheckBox m_cbCustomBackgroundColor;
		private System.Windows.Forms.TextBox m_tbOverrideUrl;
		private System.Windows.Forms.Label m_lblOverrideUrl;

	}
}