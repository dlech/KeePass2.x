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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionsForm));
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
			this.m_lblPolicyAffectHint = new System.Windows.Forms.Label();
			this.m_lblPolicyCurrentUserLevel = new System.Windows.Forms.Label();
			this.m_lblPolicyStatus = new System.Windows.Forms.Label();
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
			resources.ApplyResources(this.m_bannerImage, "m_bannerImage");
			this.m_bannerImage.Name = "m_bannerImage";
			this.m_bannerImage.TabStop = false;
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
			// m_tabMain
			// 
			this.m_tabMain.Controls.Add(this.m_tabSecurity);
			this.m_tabMain.Controls.Add(this.m_tabPolicy);
			this.m_tabMain.Controls.Add(this.m_tabGui);
			this.m_tabMain.Controls.Add(this.m_tabIntegration);
			this.m_tabMain.Controls.Add(this.m_tabAdvanced);
			resources.ApplyResources(this.m_tabMain, "m_tabMain");
			this.m_tabMain.Name = "m_tabMain";
			this.m_tabMain.SelectedIndex = 0;
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
			resources.ApplyResources(this.m_tabSecurity, "m_tabSecurity");
			this.m_tabSecurity.Name = "m_tabSecurity";
			this.m_tabSecurity.UseVisualStyleBackColor = true;
			// 
			// m_lvSecurityOptions
			// 
			this.m_lvSecurityOptions.CheckBoxes = true;
			this.m_lvSecurityOptions.FullRowSelect = true;
			this.m_lvSecurityOptions.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			resources.ApplyResources(this.m_lvSecurityOptions, "m_lvSecurityOptions");
			this.m_lvSecurityOptions.Name = "m_lvSecurityOptions";
			this.m_lvSecurityOptions.UseCompatibleStateImageBehavior = false;
			this.m_lvSecurityOptions.View = System.Windows.Forms.View.Details;
			// 
			// m_numClipClearTime
			// 
			resources.ApplyResources(this.m_numClipClearTime, "m_numClipClearTime");
			this.m_numClipClearTime.Name = "m_numClipClearTime";
			// 
			// m_cbClipClearTime
			// 
			resources.ApplyResources(this.m_cbClipClearTime, "m_cbClipClearTime");
			this.m_cbClipClearTime.Name = "m_cbClipClearTime";
			this.m_cbClipClearTime.UseVisualStyleBackColor = true;
			// 
			// m_numDefaultExpireDays
			// 
			resources.ApplyResources(this.m_numDefaultExpireDays, "m_numDefaultExpireDays");
			this.m_numDefaultExpireDays.Maximum = new decimal(new int[] {
            2920,
            0,
            0,
            0});
			this.m_numDefaultExpireDays.Name = "m_numDefaultExpireDays";
			// 
			// m_cbDefaultExpireDays
			// 
			resources.ApplyResources(this.m_cbDefaultExpireDays, "m_cbDefaultExpireDays");
			this.m_cbDefaultExpireDays.Name = "m_cbDefaultExpireDays";
			this.m_cbDefaultExpireDays.UseVisualStyleBackColor = true;
			this.m_cbDefaultExpireDays.CheckedChanged += new System.EventHandler(this.OnDefaultExpireDaysCheckedChanged);
			// 
			// m_cbLockAfterTime
			// 
			resources.ApplyResources(this.m_cbLockAfterTime, "m_cbLockAfterTime");
			this.m_cbLockAfterTime.Name = "m_cbLockAfterTime";
			this.m_cbLockAfterTime.UseVisualStyleBackColor = true;
			this.m_cbLockAfterTime.CheckedChanged += new System.EventHandler(this.OnLockAfterTimeCheckedChanged);
			// 
			// m_numLockAfterTime
			// 
			resources.ApplyResources(this.m_numLockAfterTime, "m_numLockAfterTime");
			this.m_numLockAfterTime.Maximum = new decimal(new int[] {
            1209600,
            0,
            0,
            0});
			this.m_numLockAfterTime.Name = "m_numLockAfterTime";
			this.m_numLockAfterTime.ValueChanged += new System.EventHandler(this.OnLockAfterTimeValueChanged);
			// 
			// m_tabPolicy
			// 
			this.m_tabPolicy.Controls.Add(this.m_lblPolicyAffectHint);
			this.m_tabPolicy.Controls.Add(this.m_lblPolicyCurrentUserLevel);
			this.m_tabPolicy.Controls.Add(this.m_lblPolicyStatus);
			this.m_tabPolicy.Controls.Add(this.m_lvPolicy);
			this.m_tabPolicy.Controls.Add(this.m_linkPolicyInfo);
			this.m_tabPolicy.Controls.Add(this.m_lblPolicyMore);
			this.m_tabPolicy.Controls.Add(this.m_lblPolicyRestart);
			this.m_tabPolicy.Controls.Add(this.m_lblPolicyIntro);
			resources.ApplyResources(this.m_tabPolicy, "m_tabPolicy");
			this.m_tabPolicy.Name = "m_tabPolicy";
			this.m_tabPolicy.UseVisualStyleBackColor = true;
			// 
			// m_lblPolicyAffectHint
			// 
			resources.ApplyResources(this.m_lblPolicyAffectHint, "m_lblPolicyAffectHint");
			this.m_lblPolicyAffectHint.Name = "m_lblPolicyAffectHint";
			// 
			// m_lblPolicyCurrentUserLevel
			// 
			resources.ApplyResources(this.m_lblPolicyCurrentUserLevel, "m_lblPolicyCurrentUserLevel");
			this.m_lblPolicyCurrentUserLevel.Name = "m_lblPolicyCurrentUserLevel";
			// 
			// m_lblPolicyStatus
			// 
			resources.ApplyResources(this.m_lblPolicyStatus, "m_lblPolicyStatus");
			this.m_lblPolicyStatus.Name = "m_lblPolicyStatus";
			// 
			// m_lvPolicy
			// 
			this.m_lvPolicy.CheckBoxes = true;
			this.m_lvPolicy.FullRowSelect = true;
			this.m_lvPolicy.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			resources.ApplyResources(this.m_lvPolicy, "m_lvPolicy");
			this.m_lvPolicy.Name = "m_lvPolicy";
			this.m_lvPolicy.UseCompatibleStateImageBehavior = false;
			this.m_lvPolicy.View = System.Windows.Forms.View.Details;
			// 
			// m_linkPolicyInfo
			// 
			resources.ApplyResources(this.m_linkPolicyInfo, "m_linkPolicyInfo");
			this.m_linkPolicyInfo.Name = "m_linkPolicyInfo";
			this.m_linkPolicyInfo.TabStop = true;
			// 
			// m_lblPolicyMore
			// 
			resources.ApplyResources(this.m_lblPolicyMore, "m_lblPolicyMore");
			this.m_lblPolicyMore.Name = "m_lblPolicyMore";
			// 
			// m_lblPolicyRestart
			// 
			resources.ApplyResources(this.m_lblPolicyRestart, "m_lblPolicyRestart");
			this.m_lblPolicyRestart.Name = "m_lblPolicyRestart";
			// 
			// m_lblPolicyIntro
			// 
			resources.ApplyResources(this.m_lblPolicyIntro, "m_lblPolicyIntro");
			this.m_lblPolicyIntro.Name = "m_lblPolicyIntro";
			// 
			// m_tabGui
			// 
			this.m_tabGui.Controls.Add(this.m_btnSelFont);
			this.m_tabGui.Controls.Add(this.m_lvGuiOptions);
			this.m_tabGui.Controls.Add(this.m_lblBannerStyle);
			this.m_tabGui.Controls.Add(this.m_cmbBannerStyle);
			resources.ApplyResources(this.m_tabGui, "m_tabGui");
			this.m_tabGui.Name = "m_tabGui";
			this.m_tabGui.UseVisualStyleBackColor = true;
			// 
			// m_btnSelFont
			// 
			resources.ApplyResources(this.m_btnSelFont, "m_btnSelFont");
			this.m_btnSelFont.Name = "m_btnSelFont";
			this.m_btnSelFont.UseVisualStyleBackColor = true;
			this.m_btnSelFont.Click += new System.EventHandler(this.OnBtnSelListFont);
			// 
			// m_lvGuiOptions
			// 
			this.m_lvGuiOptions.CheckBoxes = true;
			this.m_lvGuiOptions.FullRowSelect = true;
			this.m_lvGuiOptions.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			resources.ApplyResources(this.m_lvGuiOptions, "m_lvGuiOptions");
			this.m_lvGuiOptions.Name = "m_lvGuiOptions";
			this.m_lvGuiOptions.UseCompatibleStateImageBehavior = false;
			this.m_lvGuiOptions.View = System.Windows.Forms.View.Details;
			// 
			// m_lblBannerStyle
			// 
			resources.ApplyResources(this.m_lblBannerStyle, "m_lblBannerStyle");
			this.m_lblBannerStyle.Name = "m_lblBannerStyle";
			// 
			// m_cmbBannerStyle
			// 
			this.m_cmbBannerStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbBannerStyle.FormattingEnabled = true;
			this.m_cmbBannerStyle.Items.AddRange(new object[] {
            resources.GetString("m_cmbBannerStyle.Items"),
            resources.GetString("m_cmbBannerStyle.Items1"),
            resources.GetString("m_cmbBannerStyle.Items2"),
            resources.GetString("m_cmbBannerStyle.Items3"),
            resources.GetString("m_cmbBannerStyle.Items4")});
			resources.ApplyResources(this.m_cmbBannerStyle, "m_cmbBannerStyle");
			this.m_cmbBannerStyle.Name = "m_cmbBannerStyle";
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
			resources.ApplyResources(this.m_tabIntegration, "m_tabIntegration");
			this.m_tabIntegration.Name = "m_tabIntegration";
			this.m_tabIntegration.UseVisualStyleBackColor = true;
			// 
			// m_tbUrlOverride
			// 
			resources.ApplyResources(this.m_tbUrlOverride, "m_tbUrlOverride");
			this.m_tbUrlOverride.Name = "m_tbUrlOverride";
			// 
			// m_cbUrlOverride
			// 
			resources.ApplyResources(this.m_cbUrlOverride, "m_cbUrlOverride");
			this.m_cbUrlOverride.Name = "m_cbUrlOverride";
			this.m_cbUrlOverride.UseVisualStyleBackColor = true;
			this.m_cbUrlOverride.CheckedChanged += new System.EventHandler(this.OnOverrideURLsCheckedChanged);
			// 
			// m_cbSingleClickTrayAction
			// 
			resources.ApplyResources(this.m_cbSingleClickTrayAction, "m_cbSingleClickTrayAction");
			this.m_cbSingleClickTrayAction.Name = "m_cbSingleClickTrayAction";
			this.m_cbSingleClickTrayAction.UseVisualStyleBackColor = true;
			// 
			// m_cbAutoRun
			// 
			resources.ApplyResources(this.m_cbAutoRun, "m_cbAutoRun");
			this.m_cbAutoRun.Name = "m_cbAutoRun";
			this.m_cbAutoRun.UseVisualStyleBackColor = true;
			this.m_cbAutoRun.CheckedChanged += new System.EventHandler(this.OnCheckedChangedAutoRun);
			// 
			// m_grpFileExt
			// 
			this.m_grpFileExt.Controls.Add(this.m_btnFileExtRemove);
			this.m_grpFileExt.Controls.Add(this.m_btnFileExtCreate);
			this.m_grpFileExt.Controls.Add(this.m_lblFileExtHint);
			resources.ApplyResources(this.m_grpFileExt, "m_grpFileExt");
			this.m_grpFileExt.Name = "m_grpFileExt";
			this.m_grpFileExt.TabStop = false;
			// 
			// m_btnFileExtRemove
			// 
			this.m_btnFileExtRemove.Image = global::KeePass.Properties.Resources.B16x16_EditDelete;
			resources.ApplyResources(this.m_btnFileExtRemove, "m_btnFileExtRemove");
			this.m_btnFileExtRemove.Name = "m_btnFileExtRemove";
			this.m_btnFileExtRemove.UseVisualStyleBackColor = true;
			this.m_btnFileExtRemove.Click += new System.EventHandler(this.OnBtnFileExtRemove);
			// 
			// m_btnFileExtCreate
			// 
			this.m_btnFileExtCreate.Image = global::KeePass.Properties.Resources.B16x16_FileNew;
			resources.ApplyResources(this.m_btnFileExtCreate, "m_btnFileExtCreate");
			this.m_btnFileExtCreate.Name = "m_btnFileExtCreate";
			this.m_btnFileExtCreate.UseVisualStyleBackColor = true;
			this.m_btnFileExtCreate.Click += new System.EventHandler(this.OnBtnFileExtCreate);
			// 
			// m_lblFileExtHint
			// 
			resources.ApplyResources(this.m_lblFileExtHint, "m_lblFileExtHint");
			this.m_lblFileExtHint.Name = "m_lblFileExtHint";
			// 
			// m_grpHotKeys
			// 
			this.m_grpHotKeys.Controls.Add(this.m_tbShowWindowHotKey);
			this.m_grpHotKeys.Controls.Add(this.m_lblGlobalAutoTypeHotKey);
			this.m_grpHotKeys.Controls.Add(this.m_lblRestoreHotKey);
			this.m_grpHotKeys.Controls.Add(this.m_tbGlobalAutoType);
			resources.ApplyResources(this.m_grpHotKeys, "m_grpHotKeys");
			this.m_grpHotKeys.Name = "m_grpHotKeys";
			this.m_grpHotKeys.TabStop = false;
			// 
			// m_tbShowWindowHotKey
			// 
			resources.ApplyResources(this.m_tbShowWindowHotKey, "m_tbShowWindowHotKey");
			this.m_tbShowWindowHotKey.Name = "m_tbShowWindowHotKey";
			// 
			// m_lblGlobalAutoTypeHotKey
			// 
			resources.ApplyResources(this.m_lblGlobalAutoTypeHotKey, "m_lblGlobalAutoTypeHotKey");
			this.m_lblGlobalAutoTypeHotKey.Name = "m_lblGlobalAutoTypeHotKey";
			// 
			// m_lblRestoreHotKey
			// 
			resources.ApplyResources(this.m_lblRestoreHotKey, "m_lblRestoreHotKey");
			this.m_lblRestoreHotKey.Name = "m_lblRestoreHotKey";
			// 
			// m_tbGlobalAutoType
			// 
			resources.ApplyResources(this.m_tbGlobalAutoType, "m_tbGlobalAutoType");
			this.m_tbGlobalAutoType.Name = "m_tbGlobalAutoType";
			// 
			// m_tabAdvanced
			// 
			this.m_tabAdvanced.Controls.Add(this.m_lvAdvanced);
			resources.ApplyResources(this.m_tabAdvanced, "m_tabAdvanced");
			this.m_tabAdvanced.Name = "m_tabAdvanced";
			this.m_tabAdvanced.UseVisualStyleBackColor = true;
			// 
			// m_lvAdvanced
			// 
			this.m_lvAdvanced.CheckBoxes = true;
			this.m_lvAdvanced.FullRowSelect = true;
			this.m_lvAdvanced.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			resources.ApplyResources(this.m_lvAdvanced, "m_lvAdvanced");
			this.m_lvAdvanced.Name = "m_lvAdvanced";
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
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.Controls.Add(this.m_tabMain);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOK);
			this.Controls.Add(this.m_bannerImage);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "OptionsForm";
			this.ShowInTaskbar = false;
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
			this.Load += new System.EventHandler(this.OnFormLoad);
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
		private System.Windows.Forms.Label m_lblPolicyStatus;
		private System.Windows.Forms.Label m_lblPolicyAffectHint;
		private System.Windows.Forms.Label m_lblPolicyCurrentUserLevel;
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