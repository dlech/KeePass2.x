namespace KeePass.Forms
{
	partial class OptionsForm
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
			this.m_bannerImage = new System.Windows.Forms.PictureBox();
			this.m_btnOK = new System.Windows.Forms.Button();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_tabMain = new System.Windows.Forms.TabControl();
			this.m_tabSecurity = new System.Windows.Forms.TabPage();
			this.m_lvSecurityOptions = new System.Windows.Forms.ListView();
			this.m_numClipClearTime = new System.Windows.Forms.NumericUpDown();
			this.m_cbClipClearTime = new System.Windows.Forms.CheckBox();
			this.m_numDefaultExpireDays = new System.Windows.Forms.NumericUpDown();
			this.m_cbDefaultExpireDays = new System.Windows.Forms.CheckBox();
			this.m_cbLockAfterTime = new System.Windows.Forms.CheckBox();
			this.m_numLockAfterTime = new System.Windows.Forms.NumericUpDown();
			this.m_tabPolicy = new System.Windows.Forms.TabPage();
			this.m_lvPolicy = new System.Windows.Forms.ListView();
			this.m_linkPolicyInfo = new System.Windows.Forms.LinkLabel();
			this.m_lblPolicyMore = new System.Windows.Forms.Label();
			this.m_lblPolicyRestart = new System.Windows.Forms.Label();
			this.m_lblPolicyIntro = new System.Windows.Forms.Label();
			this.m_tabGui = new System.Windows.Forms.TabPage();
			this.m_btnSelFont = new System.Windows.Forms.Button();
			this.m_lvGuiOptions = new System.Windows.Forms.ListView();
			this.m_lblBannerStyle = new System.Windows.Forms.Label();
			this.m_cmbBannerStyle = new System.Windows.Forms.ComboBox();
			this.m_tabIntegration = new System.Windows.Forms.TabPage();
			this.m_tbUrlOverride = new System.Windows.Forms.TextBox();
			this.m_cbUrlOverride = new System.Windows.Forms.CheckBox();
			this.m_cbSingleClickTrayAction = new System.Windows.Forms.CheckBox();
			this.m_cbAutoRun = new System.Windows.Forms.CheckBox();
			this.m_grpFileExt = new System.Windows.Forms.GroupBox();
			this.m_btnFileExtRemove = new System.Windows.Forms.Button();
			this.m_btnFileExtCreate = new System.Windows.Forms.Button();
			this.m_lblFileExtHint = new System.Windows.Forms.Label();
			this.m_grpHotKeys = new System.Windows.Forms.GroupBox();
			this.m_tbShowWindowHotKey = new System.Windows.Forms.TextBox();
			this.m_lblGlobalAutoTypeHotKey = new System.Windows.Forms.Label();
			this.m_lblRestoreHotKey = new System.Windows.Forms.Label();
			this.m_tbGlobalAutoType = new System.Windows.Forms.TextBox();
			this.m_tabAdvanced = new System.Windows.Forms.TabPage();
			this.m_lvAdvanced = new System.Windows.Forms.ListView();
			this.m_fontLists = new System.Windows.Forms.FontDialog();
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
			this.m_tabMain.SuspendLayout();
			this.m_tabSecurity.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numClipClearTime)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numDefaultExpireDays)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numLockAfterTime)).BeginInit();
			this.m_tabPolicy.SuspendLayout();
			this.m_tabGui.SuspendLayout();
			this.m_tabIntegration.SuspendLayout();
			this.m_grpFileExt.SuspendLayout();
			this.m_grpHotKeys.SuspendLayout();
			this.m_tabAdvanced.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_bannerImage
			// 
			this.m_bannerImage.Dock = System.Windows.Forms.DockStyle.Top;
			this.m_bannerImage.Location = new System.Drawing.Point(0, 0);
			this.m_bannerImage.Name = "m_bannerImage";
			this.m_bannerImage.Size = new System.Drawing.Size(586, 60);
			this.m_bannerImage.TabIndex = 0;
			this.m_bannerImage.TabStop = false;
			// 
			// m_btnOK
			// 
			this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_btnOK.Location = new System.Drawing.Point(418, 409);
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.Size = new System.Drawing.Size(75, 23);
			this.m_btnOK.TabIndex = 0;
			this.m_btnOK.Text = "&OK";
			this.m_btnOK.UseVisualStyleBackColor = true;
			this.m_btnOK.Click += new System.EventHandler(this.OnBtnOK);
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_btnCancel.Location = new System.Drawing.Point(499, 409);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 1;
			this.m_btnCancel.Text = "&Cancel";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			this.m_btnCancel.Click += new System.EventHandler(this.OnBtnCancel);
			// 
			// m_tabMain
			// 
			this.m_tabMain.Controls.Add(this.m_tabSecurity);
			this.m_tabMain.Controls.Add(this.m_tabPolicy);
			this.m_tabMain.Controls.Add(this.m_tabGui);
			this.m_tabMain.Controls.Add(this.m_tabIntegration);
			this.m_tabMain.Controls.Add(this.m_tabAdvanced);
			this.m_tabMain.Location = new System.Drawing.Point(12, 66);
			this.m_tabMain.Multiline = true;
			this.m_tabMain.Name = "m_tabMain";
			this.m_tabMain.SelectedIndex = 0;
			this.m_tabMain.Size = new System.Drawing.Size(562, 337);
			this.m_tabMain.TabIndex = 2;
			// 
			// m_tabSecurity
			// 
			this.m_tabSecurity.Controls.Add(this.m_lvSecurityOptions);
			this.m_tabSecurity.Controls.Add(this.m_numClipClearTime);
			this.m_tabSecurity.Controls.Add(this.m_cbClipClearTime);
			this.m_tabSecurity.Controls.Add(this.m_numDefaultExpireDays);
			this.m_tabSecurity.Controls.Add(this.m_cbDefaultExpireDays);
			this.m_tabSecurity.Controls.Add(this.m_cbLockAfterTime);
			this.m_tabSecurity.Controls.Add(this.m_numLockAfterTime);
			this.m_tabSecurity.Location = new System.Drawing.Point(4, 22);
			this.m_tabSecurity.Name = "m_tabSecurity";
			this.m_tabSecurity.Padding = new System.Windows.Forms.Padding(3);
			this.m_tabSecurity.Size = new System.Drawing.Size(554, 311);
			this.m_tabSecurity.TabIndex = 0;
			this.m_tabSecurity.Text = "Security";
			this.m_tabSecurity.UseVisualStyleBackColor = true;
			// 
			// m_lvSecurityOptions
			// 
			this.m_lvSecurityOptions.CheckBoxes = true;
			this.m_lvSecurityOptions.FullRowSelect = true;
			this.m_lvSecurityOptions.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.m_lvSecurityOptions.Location = new System.Drawing.Point(6, 81);
			this.m_lvSecurityOptions.Name = "m_lvSecurityOptions";
			this.m_lvSecurityOptions.ShowItemToolTips = true;
			this.m_lvSecurityOptions.Size = new System.Drawing.Size(542, 224);
			this.m_lvSecurityOptions.TabIndex = 6;
			this.m_lvSecurityOptions.UseCompatibleStateImageBehavior = false;
			this.m_lvSecurityOptions.View = System.Windows.Forms.View.Details;
			// 
			// m_numClipClearTime
			// 
			this.m_numClipClearTime.Location = new System.Drawing.Point(327, 34);
			this.m_numClipClearTime.Maximum = new decimal(new int[] {
            1209600,
            0,
            0,
            0});
			this.m_numClipClearTime.Name = "m_numClipClearTime";
			this.m_numClipClearTime.Size = new System.Drawing.Size(66, 20);
			this.m_numClipClearTime.TabIndex = 3;
			// 
			// m_cbClipClearTime
			// 
			this.m_cbClipClearTime.AutoSize = true;
			this.m_cbClipClearTime.Location = new System.Drawing.Point(6, 35);
			this.m_cbClipClearTime.Name = "m_cbClipClearTime";
			this.m_cbClipClearTime.Size = new System.Drawing.Size(194, 17);
			this.m_cbClipClearTime.TabIndex = 2;
			this.m_cbClipClearTime.Text = "Clipboard auto-clear time (seconds):";
			this.m_cbClipClearTime.UseVisualStyleBackColor = true;
			this.m_cbClipClearTime.CheckedChanged += new System.EventHandler(this.OnClipboardClearTimeCheckedChanged);
			// 
			// m_numDefaultExpireDays
			// 
			this.m_numDefaultExpireDays.Location = new System.Drawing.Point(327, 57);
			this.m_numDefaultExpireDays.Maximum = new decimal(new int[] {
            2920,
            0,
            0,
            0});
			this.m_numDefaultExpireDays.Name = "m_numDefaultExpireDays";
			this.m_numDefaultExpireDays.Size = new System.Drawing.Size(66, 20);
			this.m_numDefaultExpireDays.TabIndex = 5;
			// 
			// m_cbDefaultExpireDays
			// 
			this.m_cbDefaultExpireDays.AutoSize = true;
			this.m_cbDefaultExpireDays.Location = new System.Drawing.Point(6, 58);
			this.m_cbDefaultExpireDays.Name = "m_cbDefaultExpireDays";
			this.m_cbDefaultExpireDays.Size = new System.Drawing.Size(315, 17);
			this.m_cbDefaultExpireDays.TabIndex = 4;
			this.m_cbDefaultExpireDays.Text = "By default, new entries expire in the following number of days:";
			this.m_cbDefaultExpireDays.UseVisualStyleBackColor = true;
			this.m_cbDefaultExpireDays.CheckedChanged += new System.EventHandler(this.OnDefaultExpireDaysCheckedChanged);
			// 
			// m_cbLockAfterTime
			// 
			this.m_cbLockAfterTime.AutoSize = true;
			this.m_cbLockAfterTime.Location = new System.Drawing.Point(6, 12);
			this.m_cbLockAfterTime.Name = "m_cbLockAfterTime";
			this.m_cbLockAfterTime.Size = new System.Drawing.Size(287, 17);
			this.m_cbLockAfterTime.TabIndex = 0;
			this.m_cbLockAfterTime.Text = "Lock workspace after the following number of seconds:";
			this.m_cbLockAfterTime.UseVisualStyleBackColor = true;
			this.m_cbLockAfterTime.CheckedChanged += new System.EventHandler(this.OnLockAfterTimeCheckedChanged);
			// 
			// m_numLockAfterTime
			// 
			this.m_numLockAfterTime.Location = new System.Drawing.Point(327, 11);
			this.m_numLockAfterTime.Maximum = new decimal(new int[] {
            1209600,
            0,
            0,
            0});
			this.m_numLockAfterTime.Name = "m_numLockAfterTime";
			this.m_numLockAfterTime.Size = new System.Drawing.Size(66, 20);
			this.m_numLockAfterTime.TabIndex = 1;
			this.m_numLockAfterTime.ValueChanged += new System.EventHandler(this.OnLockAfterTimeValueChanged);
			// 
			// m_tabPolicy
			// 
			this.m_tabPolicy.Controls.Add(this.m_lvPolicy);
			this.m_tabPolicy.Controls.Add(this.m_linkPolicyInfo);
			this.m_tabPolicy.Controls.Add(this.m_lblPolicyMore);
			this.m_tabPolicy.Controls.Add(this.m_lblPolicyRestart);
			this.m_tabPolicy.Controls.Add(this.m_lblPolicyIntro);
			this.m_tabPolicy.Location = new System.Drawing.Point(4, 22);
			this.m_tabPolicy.Name = "m_tabPolicy";
			this.m_tabPolicy.Size = new System.Drawing.Size(554, 311);
			this.m_tabPolicy.TabIndex = 3;
			this.m_tabPolicy.Text = "Policy";
			this.m_tabPolicy.UseVisualStyleBackColor = true;
			// 
			// m_lvPolicy
			// 
			this.m_lvPolicy.CheckBoxes = true;
			this.m_lvPolicy.FullRowSelect = true;
			this.m_lvPolicy.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.m_lvPolicy.Location = new System.Drawing.Point(6, 42);
			this.m_lvPolicy.Name = "m_lvPolicy";
			this.m_lvPolicy.ShowItemToolTips = true;
			this.m_lvPolicy.Size = new System.Drawing.Size(538, 244);
			this.m_lvPolicy.TabIndex = 6;
			this.m_lvPolicy.UseCompatibleStateImageBehavior = false;
			this.m_lvPolicy.View = System.Windows.Forms.View.Details;
			// 
			// m_linkPolicyInfo
			// 
			this.m_linkPolicyInfo.AutoSize = true;
			this.m_linkPolicyInfo.Location = new System.Drawing.Point(134, 23);
			this.m_linkPolicyInfo.Name = "m_linkPolicyInfo";
			this.m_linkPolicyInfo.Size = new System.Drawing.Size(115, 13);
			this.m_linkPolicyInfo.TabIndex = 2;
			this.m_linkPolicyInfo.TabStop = true;
			this.m_linkPolicyInfo.Text = "Application Policy Help";
			this.m_linkPolicyInfo.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnPolicyInfoLinkClicked);
			// 
			// m_lblPolicyMore
			// 
			this.m_lblPolicyMore.AutoSize = true;
			this.m_lblPolicyMore.Location = new System.Drawing.Point(3, 23);
			this.m_lblPolicyMore.Name = "m_lblPolicyMore";
			this.m_lblPolicyMore.Size = new System.Drawing.Size(125, 13);
			this.m_lblPolicyMore.TabIndex = 1;
			this.m_lblPolicyMore.Text = "For more information see:";
			// 
			// m_lblPolicyRestart
			// 
			this.m_lblPolicyRestart.AutoSize = true;
			this.m_lblPolicyRestart.Location = new System.Drawing.Point(3, 289);
			this.m_lblPolicyRestart.Name = "m_lblPolicyRestart";
			this.m_lblPolicyRestart.Size = new System.Drawing.Size(234, 13);
			this.m_lblPolicyRestart.TabIndex = 7;
			this.m_lblPolicyRestart.Text = "Changing the policy requires restarting KeePass.";
			// 
			// m_lblPolicyIntro
			// 
			this.m_lblPolicyIntro.Location = new System.Drawing.Point(3, 9);
			this.m_lblPolicyIntro.Name = "m_lblPolicyIntro";
			this.m_lblPolicyIntro.Size = new System.Drawing.Size(548, 14);
			this.m_lblPolicyIntro.TabIndex = 0;
			this.m_lblPolicyIntro.Text = "The application policy defines which security-critical operations are allowed and" +
				" which aren\'t.";
			// 
			// m_tabGui
			// 
			this.m_tabGui.Controls.Add(this.m_btnSelFont);
			this.m_tabGui.Controls.Add(this.m_lvGuiOptions);
			this.m_tabGui.Controls.Add(this.m_lblBannerStyle);
			this.m_tabGui.Controls.Add(this.m_cmbBannerStyle);
			this.m_tabGui.Location = new System.Drawing.Point(4, 22);
			this.m_tabGui.Name = "m_tabGui";
			this.m_tabGui.Size = new System.Drawing.Size(554, 311);
			this.m_tabGui.TabIndex = 2;
			this.m_tabGui.Text = "Interface";
			this.m_tabGui.UseVisualStyleBackColor = true;
			// 
			// m_btnSelFont
			// 
			this.m_btnSelFont.Location = new System.Drawing.Point(12, 270);
			this.m_btnSelFont.Name = "m_btnSelFont";
			this.m_btnSelFont.Size = new System.Drawing.Size(105, 23);
			this.m_btnSelFont.TabIndex = 3;
			this.m_btnSelFont.Text = "Select List Font";
			this.m_btnSelFont.UseVisualStyleBackColor = true;
			this.m_btnSelFont.Click += new System.EventHandler(this.OnBtnSelListFont);
			// 
			// m_lvGuiOptions
			// 
			this.m_lvGuiOptions.CheckBoxes = true;
			this.m_lvGuiOptions.FullRowSelect = true;
			this.m_lvGuiOptions.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.m_lvGuiOptions.Location = new System.Drawing.Point(12, 12);
			this.m_lvGuiOptions.Name = "m_lvGuiOptions";
			this.m_lvGuiOptions.ShowItemToolTips = true;
			this.m_lvGuiOptions.Size = new System.Drawing.Size(529, 219);
			this.m_lvGuiOptions.TabIndex = 0;
			this.m_lvGuiOptions.UseCompatibleStateImageBehavior = false;
			this.m_lvGuiOptions.View = System.Windows.Forms.View.Details;
			// 
			// m_lblBannerStyle
			// 
			this.m_lblBannerStyle.AutoSize = true;
			this.m_lblBannerStyle.Location = new System.Drawing.Point(9, 246);
			this.m_lblBannerStyle.Name = "m_lblBannerStyle";
			this.m_lblBannerStyle.Size = new System.Drawing.Size(103, 13);
			this.m_lblBannerStyle.TabIndex = 1;
			this.m_lblBannerStyle.Text = "Dialog Banner Style:";
			// 
			// m_cmbBannerStyle
			// 
			this.m_cmbBannerStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbBannerStyle.FormattingEnabled = true;
			this.m_cmbBannerStyle.Location = new System.Drawing.Point(118, 243);
			this.m_cmbBannerStyle.Name = "m_cmbBannerStyle";
			this.m_cmbBannerStyle.Size = new System.Drawing.Size(193, 21);
			this.m_cmbBannerStyle.TabIndex = 2;
			this.m_cmbBannerStyle.SelectedValueChanged += new System.EventHandler(this.OnBannerStyleSelectedChanged);
			// 
			// m_tabIntegration
			// 
			this.m_tabIntegration.Controls.Add(this.m_tbUrlOverride);
			this.m_tabIntegration.Controls.Add(this.m_cbUrlOverride);
			this.m_tabIntegration.Controls.Add(this.m_cbSingleClickTrayAction);
			this.m_tabIntegration.Controls.Add(this.m_cbAutoRun);
			this.m_tabIntegration.Controls.Add(this.m_grpFileExt);
			this.m_tabIntegration.Controls.Add(this.m_grpHotKeys);
			this.m_tabIntegration.Location = new System.Drawing.Point(4, 22);
			this.m_tabIntegration.Name = "m_tabIntegration";
			this.m_tabIntegration.Size = new System.Drawing.Size(554, 311);
			this.m_tabIntegration.TabIndex = 4;
			this.m_tabIntegration.Text = "Integration";
			this.m_tabIntegration.UseVisualStyleBackColor = true;
			// 
			// m_tbUrlOverride
			// 
			this.m_tbUrlOverride.Location = new System.Drawing.Point(139, 240);
			this.m_tbUrlOverride.Name = "m_tbUrlOverride";
			this.m_tbUrlOverride.Size = new System.Drawing.Size(406, 20);
			this.m_tbUrlOverride.TabIndex = 5;
			// 
			// m_cbUrlOverride
			// 
			this.m_cbUrlOverride.AutoSize = true;
			this.m_cbUrlOverride.Location = new System.Drawing.Point(12, 242);
			this.m_cbUrlOverride.Name = "m_cbUrlOverride";
			this.m_cbUrlOverride.Size = new System.Drawing.Size(112, 17);
			this.m_cbUrlOverride.TabIndex = 4;
			this.m_cbUrlOverride.Text = "Override all URLs:";
			this.m_cbUrlOverride.UseVisualStyleBackColor = true;
			this.m_cbUrlOverride.CheckedChanged += new System.EventHandler(this.OnOverrideURLsCheckedChanged);
			// 
			// m_cbSingleClickTrayAction
			// 
			this.m_cbSingleClickTrayAction.AutoSize = true;
			this.m_cbSingleClickTrayAction.Location = new System.Drawing.Point(12, 219);
			this.m_cbSingleClickTrayAction.Name = "m_cbSingleClickTrayAction";
			this.m_cbSingleClickTrayAction.Size = new System.Drawing.Size(314, 17);
			this.m_cbSingleClickTrayAction.TabIndex = 3;
			this.m_cbSingleClickTrayAction.Text = "Single click instead of double click for default tray icon action";
			this.m_cbSingleClickTrayAction.UseVisualStyleBackColor = true;
			// 
			// m_cbAutoRun
			// 
			this.m_cbAutoRun.AutoSize = true;
			this.m_cbAutoRun.Location = new System.Drawing.Point(12, 195);
			this.m_cbAutoRun.Name = "m_cbAutoRun";
			this.m_cbAutoRun.Size = new System.Drawing.Size(250, 17);
			this.m_cbAutoRun.TabIndex = 2;
			this.m_cbAutoRun.Text = "Run KeePass at Windows startup (current user)";
			this.m_cbAutoRun.UseVisualStyleBackColor = true;
			this.m_cbAutoRun.CheckedChanged += new System.EventHandler(this.OnCheckedChangedAutoRun);
			// 
			// m_grpFileExt
			// 
			this.m_grpFileExt.Controls.Add(this.m_btnFileExtRemove);
			this.m_grpFileExt.Controls.Add(this.m_btnFileExtCreate);
			this.m_grpFileExt.Controls.Add(this.m_lblFileExtHint);
			this.m_grpFileExt.Location = new System.Drawing.Point(3, 95);
			this.m_grpFileExt.Name = "m_grpFileExt";
			this.m_grpFileExt.Size = new System.Drawing.Size(548, 82);
			this.m_grpFileExt.TabIndex = 1;
			this.m_grpFileExt.TabStop = false;
			this.m_grpFileExt.Text = "KDBX file association";
			// 
			// m_btnFileExtRemove
			// 
			this.m_btnFileExtRemove.Image = global::KeePass.Properties.Resources.B16x16_EditDelete;
			this.m_btnFileExtRemove.Location = new System.Drawing.Point(151, 51);
			this.m_btnFileExtRemove.Name = "m_btnFileExtRemove";
			this.m_btnFileExtRemove.Size = new System.Drawing.Size(135, 23);
			this.m_btnFileExtRemove.TabIndex = 2;
			this.m_btnFileExtRemove.Text = "&Remove Association";
			this.m_btnFileExtRemove.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.m_btnFileExtRemove.UseVisualStyleBackColor = true;
			this.m_btnFileExtRemove.Click += new System.EventHandler(this.OnBtnFileExtRemove);
			// 
			// m_btnFileExtCreate
			// 
			this.m_btnFileExtCreate.Image = global::KeePass.Properties.Resources.B16x16_FileNew;
			this.m_btnFileExtCreate.Location = new System.Drawing.Point(10, 51);
			this.m_btnFileExtCreate.Name = "m_btnFileExtCreate";
			this.m_btnFileExtCreate.Size = new System.Drawing.Size(131, 23);
			this.m_btnFileExtCreate.TabIndex = 1;
			this.m_btnFileExtCreate.Text = "Create &Association";
			this.m_btnFileExtCreate.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.m_btnFileExtCreate.UseVisualStyleBackColor = true;
			this.m_btnFileExtCreate.Click += new System.EventHandler(this.OnBtnFileExtCreate);
			// 
			// m_lblFileExtHint
			// 
			this.m_lblFileExtHint.Location = new System.Drawing.Point(7, 20);
			this.m_lblFileExtHint.Name = "m_lblFileExtHint";
			this.m_lblFileExtHint.Size = new System.Drawing.Size(535, 28);
			this.m_lblFileExtHint.TabIndex = 0;
			this.m_lblFileExtHint.Text = "KDBX files can be associated with KeePass. When you double-click a KDBX file in W" +
				"indows Explorer, they will automatically be opened by KeePass.";
			// 
			// m_grpHotKeys
			// 
			this.m_grpHotKeys.Controls.Add(this.m_tbShowWindowHotKey);
			this.m_grpHotKeys.Controls.Add(this.m_lblGlobalAutoTypeHotKey);
			this.m_grpHotKeys.Controls.Add(this.m_lblRestoreHotKey);
			this.m_grpHotKeys.Controls.Add(this.m_tbGlobalAutoType);
			this.m_grpHotKeys.Location = new System.Drawing.Point(3, 12);
			this.m_grpHotKeys.Name = "m_grpHotKeys";
			this.m_grpHotKeys.Size = new System.Drawing.Size(548, 77);
			this.m_grpHotKeys.TabIndex = 0;
			this.m_grpHotKeys.TabStop = false;
			this.m_grpHotKeys.Text = "System-wide hot keys";
			// 
			// m_tbShowWindowHotKey
			// 
			this.m_tbShowWindowHotKey.Location = new System.Drawing.Point(136, 45);
			this.m_tbShowWindowHotKey.Name = "m_tbShowWindowHotKey";
			this.m_tbShowWindowHotKey.Size = new System.Drawing.Size(150, 20);
			this.m_tbShowWindowHotKey.TabIndex = 3;
			// 
			// m_lblGlobalAutoTypeHotKey
			// 
			this.m_lblGlobalAutoTypeHotKey.AutoSize = true;
			this.m_lblGlobalAutoTypeHotKey.Location = new System.Drawing.Point(6, 22);
			this.m_lblGlobalAutoTypeHotKey.Name = "m_lblGlobalAutoTypeHotKey";
			this.m_lblGlobalAutoTypeHotKey.Size = new System.Drawing.Size(92, 13);
			this.m_lblGlobalAutoTypeHotKey.TabIndex = 0;
			this.m_lblGlobalAutoTypeHotKey.Text = "Global Auto-Type:";
			// 
			// m_lblRestoreHotKey
			// 
			this.m_lblRestoreHotKey.AutoSize = true;
			this.m_lblRestoreHotKey.Location = new System.Drawing.Point(6, 48);
			this.m_lblRestoreHotKey.Name = "m_lblRestoreHotKey";
			this.m_lblRestoreHotKey.Size = new System.Drawing.Size(124, 13);
			this.m_lblRestoreHotKey.TabIndex = 2;
			this.m_lblRestoreHotKey.Text = "Show KeePass Window:";
			// 
			// m_tbGlobalAutoType
			// 
			this.m_tbGlobalAutoType.Location = new System.Drawing.Point(136, 19);
			this.m_tbGlobalAutoType.Name = "m_tbGlobalAutoType";
			this.m_tbGlobalAutoType.Size = new System.Drawing.Size(150, 20);
			this.m_tbGlobalAutoType.TabIndex = 1;
			// 
			// m_tabAdvanced
			// 
			this.m_tabAdvanced.Controls.Add(this.m_lvAdvanced);
			this.m_tabAdvanced.Location = new System.Drawing.Point(4, 22);
			this.m_tabAdvanced.Name = "m_tabAdvanced";
			this.m_tabAdvanced.Padding = new System.Windows.Forms.Padding(3);
			this.m_tabAdvanced.Size = new System.Drawing.Size(554, 311);
			this.m_tabAdvanced.TabIndex = 1;
			this.m_tabAdvanced.Text = "Advanced";
			this.m_tabAdvanced.UseVisualStyleBackColor = true;
			// 
			// m_lvAdvanced
			// 
			this.m_lvAdvanced.CheckBoxes = true;
			this.m_lvAdvanced.FullRowSelect = true;
			this.m_lvAdvanced.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.m_lvAdvanced.Location = new System.Drawing.Point(6, 6);
			this.m_lvAdvanced.Name = "m_lvAdvanced";
			this.m_lvAdvanced.ShowItemToolTips = true;
			this.m_lvAdvanced.Size = new System.Drawing.Size(542, 299);
			this.m_lvAdvanced.TabIndex = 0;
			this.m_lvAdvanced.UseCompatibleStateImageBehavior = false;
			this.m_lvAdvanced.View = System.Windows.Forms.View.Details;
			// 
			// m_fontLists
			// 
			this.m_fontLists.FontMustExist = true;
			this.m_fontLists.ShowEffects = false;
			// 
			// OptionsForm
			// 
			this.AcceptButton = this.m_btnOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(586, 444);
			this.Controls.Add(this.m_tabMain);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOK);
			this.Controls.Add(this.m_bannerImage);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "OptionsForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Options";
			this.Load += new System.EventHandler(this.OnFormLoad);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
			this.m_tabMain.ResumeLayout(false);
			this.m_tabSecurity.ResumeLayout(false);
			this.m_tabSecurity.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numClipClearTime)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numDefaultExpireDays)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numLockAfterTime)).EndInit();
			this.m_tabPolicy.ResumeLayout(false);
			this.m_tabPolicy.PerformLayout();
			this.m_tabGui.ResumeLayout(false);
			this.m_tabGui.PerformLayout();
			this.m_tabIntegration.ResumeLayout(false);
			this.m_tabIntegration.PerformLayout();
			this.m_grpFileExt.ResumeLayout(false);
			this.m_grpHotKeys.ResumeLayout(false);
			this.m_grpHotKeys.PerformLayout();
			this.m_tabAdvanced.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PictureBox m_bannerImage;
		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.TabControl m_tabMain;
		private System.Windows.Forms.TabPage m_tabSecurity;
		private System.Windows.Forms.TabPage m_tabAdvanced;
		private System.Windows.Forms.TabPage m_tabGui;
		private System.Windows.Forms.ComboBox m_cmbBannerStyle;
		private System.Windows.Forms.Label m_lblBannerStyle;
		private System.Windows.Forms.CheckBox m_cbLockAfterTime;
		private System.Windows.Forms.NumericUpDown m_numLockAfterTime;
		private System.Windows.Forms.TabPage m_tabPolicy;
		private System.Windows.Forms.LinkLabel m_linkPolicyInfo;
		private System.Windows.Forms.Label m_lblPolicyMore;
		private System.Windows.Forms.Label m_lblPolicyRestart;
		private System.Windows.Forms.Label m_lblPolicyIntro;
		private System.Windows.Forms.ListView m_lvPolicy;
		private System.Windows.Forms.ListView m_lvGuiOptions;
		private System.Windows.Forms.FontDialog m_fontLists;
		private System.Windows.Forms.Button m_btnSelFont;
		private System.Windows.Forms.TabPage m_tabIntegration;
		private System.Windows.Forms.TextBox m_tbGlobalAutoType;
		private System.Windows.Forms.Label m_lblGlobalAutoTypeHotKey;
		private System.Windows.Forms.GroupBox m_grpHotKeys;
		private System.Windows.Forms.TextBox m_tbShowWindowHotKey;
		private System.Windows.Forms.Label m_lblRestoreHotKey;
		private System.Windows.Forms.CheckBox m_cbDefaultExpireDays;
		private System.Windows.Forms.NumericUpDown m_numDefaultExpireDays;
		private System.Windows.Forms.NumericUpDown m_numClipClearTime;
		private System.Windows.Forms.CheckBox m_cbClipClearTime;
		private System.Windows.Forms.Button m_btnFileExtRemove;
		private System.Windows.Forms.GroupBox m_grpFileExt;
		private System.Windows.Forms.Button m_btnFileExtCreate;
		private System.Windows.Forms.Label m_lblFileExtHint;
		private System.Windows.Forms.CheckBox m_cbAutoRun;
		private System.Windows.Forms.CheckBox m_cbSingleClickTrayAction;
		private System.Windows.Forms.ListView m_lvAdvanced;
		private System.Windows.Forms.TextBox m_tbUrlOverride;
		private System.Windows.Forms.CheckBox m_cbUrlOverride;
		private System.Windows.Forms.ListView m_lvSecurityOptions;
	}
}