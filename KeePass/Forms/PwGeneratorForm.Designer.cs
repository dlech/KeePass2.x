namespace KeePass.Forms
{
	partial class PwGeneratorForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PwGeneratorForm));
			this.m_btnOK = new System.Windows.Forms.Button();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_clbOptions = new System.Windows.Forms.CheckedListBox();
			this.m_rbStandardCharSet = new System.Windows.Forms.RadioButton();
			this.m_rbCustomCharSet = new System.Windows.Forms.RadioButton();
			this.m_tbCustomCharSet = new System.Windows.Forms.TextBox();
			this.m_lblNumGenChars = new System.Windows.Forms.Label();
			this.m_numGenChars = new System.Windows.Forms.NumericUpDown();
			this.m_bannerImage = new System.Windows.Forms.PictureBox();
			this.m_lblProfile = new System.Windows.Forms.Label();
			this.m_cmbProfiles = new System.Windows.Forms.ComboBox();
			this.m_btnProfileAdd = new System.Windows.Forms.Button();
			this.m_btnProfileRemove = new System.Windows.Forms.Button();
			this.m_ttMain = new System.Windows.Forms.ToolTip(this.components);
			this.m_grpCurOpt = new System.Windows.Forms.GroupBox();
			this.m_cbEntropy = new System.Windows.Forms.CheckBox();
			this.m_tbPattern = new System.Windows.Forms.TextBox();
			this.m_rbPattern = new System.Windows.Forms.RadioButton();
			this.m_btnHelp = new System.Windows.Forms.Button();
			this.m_tabMain = new System.Windows.Forms.TabControl();
			this.m_tabSettings = new System.Windows.Forms.TabPage();
			this.m_tabPreview = new System.Windows.Forms.TabPage();
			this.m_pbPreview = new System.Windows.Forms.ProgressBar();
			this.m_tbPreview = new System.Windows.Forms.TextBox();
			this.m_lblPreview = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.m_numGenChars)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
			this.m_grpCurOpt.SuspendLayout();
			this.m_tabMain.SuspendLayout();
			this.m_tabSettings.SuspendLayout();
			this.m_tabPreview.SuspendLayout();
			this.SuspendLayout();
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
			// m_clbOptions
			// 
			this.m_clbOptions.CheckOnClick = true;
			this.m_clbOptions.FormattingEnabled = true;
			this.m_clbOptions.Items.AddRange(new object[] {
            resources.GetString("m_clbOptions.Items"),
            resources.GetString("m_clbOptions.Items1"),
            resources.GetString("m_clbOptions.Items2"),
            resources.GetString("m_clbOptions.Items3"),
            resources.GetString("m_clbOptions.Items4"),
            resources.GetString("m_clbOptions.Items5"),
            resources.GetString("m_clbOptions.Items6"),
            resources.GetString("m_clbOptions.Items7"),
            resources.GetString("m_clbOptions.Items8")});
			resources.ApplyResources(this.m_clbOptions, "m_clbOptions");
			this.m_clbOptions.Name = "m_clbOptions";
			this.m_clbOptions.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.OnCharacterTypesItemCheck);
			// 
			// m_rbStandardCharSet
			// 
			resources.ApplyResources(this.m_rbStandardCharSet, "m_rbStandardCharSet");
			this.m_rbStandardCharSet.Checked = true;
			this.m_rbStandardCharSet.Name = "m_rbStandardCharSet";
			this.m_rbStandardCharSet.TabStop = true;
			this.m_rbStandardCharSet.UseVisualStyleBackColor = true;
			this.m_rbStandardCharSet.CheckedChanged += new System.EventHandler(this.OnStandardCharSetCheckedChanged);
			// 
			// m_rbCustomCharSet
			// 
			resources.ApplyResources(this.m_rbCustomCharSet, "m_rbCustomCharSet");
			this.m_rbCustomCharSet.Name = "m_rbCustomCharSet";
			this.m_rbCustomCharSet.TabStop = true;
			this.m_rbCustomCharSet.UseVisualStyleBackColor = true;
			this.m_rbCustomCharSet.CheckedChanged += new System.EventHandler(this.OnCustomCharSetCheckedChanged);
			// 
			// m_tbCustomCharSet
			// 
			resources.ApplyResources(this.m_tbCustomCharSet, "m_tbCustomCharSet");
			this.m_tbCustomCharSet.Name = "m_tbCustomCharSet";
			this.m_tbCustomCharSet.TextChanged += new System.EventHandler(this.OnCustomCharSetTextChanged);
			// 
			// m_lblNumGenChars
			// 
			resources.ApplyResources(this.m_lblNumGenChars, "m_lblNumGenChars");
			this.m_lblNumGenChars.Name = "m_lblNumGenChars";
			// 
			// m_numGenChars
			// 
			resources.ApplyResources(this.m_numGenChars, "m_numGenChars");
			this.m_numGenChars.Maximum = new decimal(new int[] {
            2147483645,
            0,
            0,
            0});
			this.m_numGenChars.Name = "m_numGenChars";
			this.m_numGenChars.Value = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.m_numGenChars.ValueChanged += new System.EventHandler(this.OnNumGenCharsValueChanged);
			// 
			// m_bannerImage
			// 
			resources.ApplyResources(this.m_bannerImage, "m_bannerImage");
			this.m_bannerImage.Name = "m_bannerImage";
			this.m_bannerImage.TabStop = false;
			// 
			// m_lblProfile
			// 
			resources.ApplyResources(this.m_lblProfile, "m_lblProfile");
			this.m_lblProfile.Name = "m_lblProfile";
			// 
			// m_cmbProfiles
			// 
			this.m_cmbProfiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbProfiles.FormattingEnabled = true;
			resources.ApplyResources(this.m_cmbProfiles, "m_cmbProfiles");
			this.m_cmbProfiles.Name = "m_cmbProfiles";
			this.m_cmbProfiles.SelectedIndexChanged += new System.EventHandler(this.OnProfilesSelectedIndexChanged);
			// 
			// m_btnProfileAdd
			// 
			this.m_btnProfileAdd.Image = global::KeePass.Properties.Resources.B16x16_FileNew;
			resources.ApplyResources(this.m_btnProfileAdd, "m_btnProfileAdd");
			this.m_btnProfileAdd.Name = "m_btnProfileAdd";
			this.m_ttMain.SetToolTip(this.m_btnProfileAdd, resources.GetString("m_btnProfileAdd.ToolTip"));
			this.m_btnProfileAdd.UseVisualStyleBackColor = true;
			this.m_btnProfileAdd.Click += new System.EventHandler(this.OnBtnProfileAdd);
			// 
			// m_btnProfileRemove
			// 
			this.m_btnProfileRemove.Image = global::KeePass.Properties.Resources.B16x16_EditDelete;
			resources.ApplyResources(this.m_btnProfileRemove, "m_btnProfileRemove");
			this.m_btnProfileRemove.Name = "m_btnProfileRemove";
			this.m_ttMain.SetToolTip(this.m_btnProfileRemove, resources.GetString("m_btnProfileRemove.ToolTip"));
			this.m_btnProfileRemove.UseVisualStyleBackColor = true;
			this.m_btnProfileRemove.Click += new System.EventHandler(this.OnBtnProfileRemove);
			// 
			// m_grpCurOpt
			// 
			this.m_grpCurOpt.Controls.Add(this.m_cbEntropy);
			this.m_grpCurOpt.Controls.Add(this.m_tbPattern);
			this.m_grpCurOpt.Controls.Add(this.m_rbPattern);
			this.m_grpCurOpt.Controls.Add(this.m_lblNumGenChars);
			this.m_grpCurOpt.Controls.Add(this.m_clbOptions);
			this.m_grpCurOpt.Controls.Add(this.m_rbStandardCharSet);
			this.m_grpCurOpt.Controls.Add(this.m_rbCustomCharSet);
			this.m_grpCurOpt.Controls.Add(this.m_tbCustomCharSet);
			this.m_grpCurOpt.Controls.Add(this.m_numGenChars);
			resources.ApplyResources(this.m_grpCurOpt, "m_grpCurOpt");
			this.m_grpCurOpt.Name = "m_grpCurOpt";
			this.m_grpCurOpt.TabStop = false;
			// 
			// m_cbEntropy
			// 
			resources.ApplyResources(this.m_cbEntropy, "m_cbEntropy");
			this.m_cbEntropy.Name = "m_cbEntropy";
			this.m_cbEntropy.UseVisualStyleBackColor = true;
			// 
			// m_tbPattern
			// 
			resources.ApplyResources(this.m_tbPattern, "m_tbPattern");
			this.m_tbPattern.Name = "m_tbPattern";
			// 
			// m_rbPattern
			// 
			resources.ApplyResources(this.m_rbPattern, "m_rbPattern");
			this.m_rbPattern.Name = "m_rbPattern";
			this.m_rbPattern.TabStop = true;
			this.m_rbPattern.UseVisualStyleBackColor = true;
			this.m_rbPattern.CheckedChanged += new System.EventHandler(this.OnPatternCheckedChanged);
			// 
			// m_btnHelp
			// 
			resources.ApplyResources(this.m_btnHelp, "m_btnHelp");
			this.m_btnHelp.Name = "m_btnHelp";
			this.m_btnHelp.UseVisualStyleBackColor = true;
			this.m_btnHelp.Click += new System.EventHandler(this.OnBtnHelp);
			// 
			// m_tabMain
			// 
			this.m_tabMain.Controls.Add(this.m_tabSettings);
			this.m_tabMain.Controls.Add(this.m_tabPreview);
			resources.ApplyResources(this.m_tabMain, "m_tabMain");
			this.m_tabMain.Name = "m_tabMain";
			this.m_tabMain.SelectedIndex = 0;
			this.m_tabMain.SelectedIndexChanged += new System.EventHandler(this.OnTabMainSelectedIndexChanged);
			// 
			// m_tabSettings
			// 
			this.m_tabSettings.Controls.Add(this.m_lblProfile);
			this.m_tabSettings.Controls.Add(this.m_cmbProfiles);
			this.m_tabSettings.Controls.Add(this.m_grpCurOpt);
			this.m_tabSettings.Controls.Add(this.m_btnProfileAdd);
			this.m_tabSettings.Controls.Add(this.m_btnProfileRemove);
			resources.ApplyResources(this.m_tabSettings, "m_tabSettings");
			this.m_tabSettings.Name = "m_tabSettings";
			this.m_tabSettings.UseVisualStyleBackColor = true;
			// 
			// m_tabPreview
			// 
			this.m_tabPreview.Controls.Add(this.m_pbPreview);
			this.m_tabPreview.Controls.Add(this.m_tbPreview);
			this.m_tabPreview.Controls.Add(this.m_lblPreview);
			resources.ApplyResources(this.m_tabPreview, "m_tabPreview");
			this.m_tabPreview.Name = "m_tabPreview";
			this.m_tabPreview.UseVisualStyleBackColor = true;
			// 
			// m_pbPreview
			// 
			resources.ApplyResources(this.m_pbPreview, "m_pbPreview");
			this.m_pbPreview.Name = "m_pbPreview";
			// 
			// m_tbPreview
			// 
			resources.ApplyResources(this.m_tbPreview, "m_tbPreview");
			this.m_tbPreview.Name = "m_tbPreview";
			this.m_tbPreview.ReadOnly = true;
			// 
			// m_lblPreview
			// 
			resources.ApplyResources(this.m_lblPreview, "m_lblPreview");
			this.m_lblPreview.Name = "m_lblPreview";
			// 
			// PwGeneratorForm
			// 
			this.AcceptButton = this.m_btnOK;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.Controls.Add(this.m_tabMain);
			this.Controls.Add(this.m_btnHelp);
			this.Controls.Add(this.m_bannerImage);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOK);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "PwGeneratorForm";
			this.ShowInTaskbar = false;
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
			this.Load += new System.EventHandler(this.OnFormLoad);
			((System.ComponentModel.ISupportInitialize)(this.m_numGenChars)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
			this.m_grpCurOpt.ResumeLayout(false);
			this.m_grpCurOpt.PerformLayout();
			this.m_tabMain.ResumeLayout(false);
			this.m_tabSettings.ResumeLayout(false);
			this.m_tabSettings.PerformLayout();
			this.m_tabPreview.ResumeLayout(false);
			this.m_tabPreview.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.CheckedListBox m_clbOptions;
		private System.Windows.Forms.RadioButton m_rbStandardCharSet;
		private System.Windows.Forms.RadioButton m_rbCustomCharSet;
		private System.Windows.Forms.TextBox m_tbCustomCharSet;
		private System.Windows.Forms.Label m_lblNumGenChars;
		private System.Windows.Forms.NumericUpDown m_numGenChars;
		private System.Windows.Forms.PictureBox m_bannerImage;
		private System.Windows.Forms.Label m_lblProfile;
		private System.Windows.Forms.ComboBox m_cmbProfiles;
		private System.Windows.Forms.Button m_btnProfileAdd;
		private System.Windows.Forms.ToolTip m_ttMain;
		private System.Windows.Forms.Button m_btnProfileRemove;
		private System.Windows.Forms.GroupBox m_grpCurOpt;
		private System.Windows.Forms.TextBox m_tbPattern;
		private System.Windows.Forms.RadioButton m_rbPattern;
		private System.Windows.Forms.CheckBox m_cbEntropy;
		private System.Windows.Forms.Button m_btnHelp;
		private System.Windows.Forms.TabControl m_tabMain;
		private System.Windows.Forms.TabPage m_tabSettings;
		private System.Windows.Forms.TabPage m_tabPreview;
		private System.Windows.Forms.Label m_lblPreview;
		private System.Windows.Forms.TextBox m_tbPreview;
		private System.Windows.Forms.ProgressBar m_pbPreview;
	}
}