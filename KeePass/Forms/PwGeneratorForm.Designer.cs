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
			this.m_btnOK.Location = new System.Drawing.Point(321, 470);
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
			this.m_btnCancel.Location = new System.Drawing.Point(402, 470);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 1;
			this.m_btnCancel.Text = "&Cancel";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			this.m_btnCancel.Click += new System.EventHandler(this.OnBtnCancel);
			// 
			// m_clbOptions
			// 
			this.m_clbOptions.CheckOnClick = true;
			this.m_clbOptions.FormattingEnabled = true;
			this.m_clbOptions.Items.AddRange(new object[] {
            "Upper-Case Characters (A, B, C, ...)",
            "Lower-Case Characters (a, b, c, ...)",
            "Numeric Characters (1, 2, 3, ...)",
            "Underline Character (_)",
            "Minus (-)",
            "Space ( )",
            "Special Characters (!, §, $, %, &, ...)",
            "Brackets ([, ], {, }, (, ), <, >)",
            "High ANSI Characters"});
			this.m_clbOptions.Location = new System.Drawing.Point(28, 87);
			this.m_clbOptions.Name = "m_clbOptions";
			this.m_clbOptions.Size = new System.Drawing.Size(411, 139);
			this.m_clbOptions.TabIndex = 4;
			this.m_clbOptions.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.OnCharacterTypesItemCheck);
			// 
			// m_rbStandardCharSet
			// 
			this.m_rbStandardCharSet.AutoSize = true;
			this.m_rbStandardCharSet.Checked = true;
			this.m_rbStandardCharSet.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
			this.m_rbStandardCharSet.Location = new System.Drawing.Point(9, 64);
			this.m_rbStandardCharSet.Name = "m_rbStandardCharSet";
			this.m_rbStandardCharSet.Size = new System.Drawing.Size(167, 17);
			this.m_rbStandardCharSet.TabIndex = 3;
			this.m_rbStandardCharSet.TabStop = true;
			this.m_rbStandardCharSet.Text = "Character Types To Use:";
			this.m_rbStandardCharSet.UseVisualStyleBackColor = true;
			this.m_rbStandardCharSet.CheckedChanged += new System.EventHandler(this.OnStandardCharSetCheckedChanged);
			// 
			// m_rbCustomCharSet
			// 
			this.m_rbCustomCharSet.AutoSize = true;
			this.m_rbCustomCharSet.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
			this.m_rbCustomCharSet.Location = new System.Drawing.Point(9, 233);
			this.m_rbCustomCharSet.Name = "m_rbCustomCharSet";
			this.m_rbCustomCharSet.Size = new System.Drawing.Size(202, 17);
			this.m_rbCustomCharSet.TabIndex = 5;
			this.m_rbCustomCharSet.TabStop = true;
			this.m_rbCustomCharSet.Text = "Only The Following Characters:";
			this.m_rbCustomCharSet.UseVisualStyleBackColor = true;
			this.m_rbCustomCharSet.CheckedChanged += new System.EventHandler(this.OnCustomCharSetCheckedChanged);
			// 
			// m_tbCustomCharSet
			// 
			this.m_tbCustomCharSet.Location = new System.Drawing.Point(28, 256);
			this.m_tbCustomCharSet.Name = "m_tbCustomCharSet";
			this.m_tbCustomCharSet.Size = new System.Drawing.Size(411, 20);
			this.m_tbCustomCharSet.TabIndex = 6;
			this.m_tbCustomCharSet.TextChanged += new System.EventHandler(this.OnCustomCharSetTextChanged);
			// 
			// m_lblNumGenChars
			// 
			this.m_lblNumGenChars.AutoSize = true;
			this.m_lblNumGenChars.Location = new System.Drawing.Point(6, 20);
			this.m_lblNumGenChars.Name = "m_lblNumGenChars";
			this.m_lblNumGenChars.Size = new System.Drawing.Size(157, 13);
			this.m_lblNumGenChars.TabIndex = 0;
			this.m_lblNumGenChars.Text = "Length of Generated Password:";
			// 
			// m_numGenChars
			// 
			this.m_numGenChars.Location = new System.Drawing.Point(169, 18);
			this.m_numGenChars.Maximum = new decimal(new int[] {
            2147483645,
            0,
            0,
            0});
			this.m_numGenChars.Name = "m_numGenChars";
			this.m_numGenChars.Size = new System.Drawing.Size(82, 20);
			this.m_numGenChars.TabIndex = 1;
			this.m_numGenChars.Value = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.m_numGenChars.ValueChanged += new System.EventHandler(this.OnNumGenCharsValueChanged);
			// 
			// m_bannerImage
			// 
			this.m_bannerImage.Dock = System.Windows.Forms.DockStyle.Top;
			this.m_bannerImage.Location = new System.Drawing.Point(0, 0);
			this.m_bannerImage.Name = "m_bannerImage";
			this.m_bannerImage.Size = new System.Drawing.Size(489, 60);
			this.m_bannerImage.TabIndex = 10;
			this.m_bannerImage.TabStop = false;
			// 
			// m_lblProfile
			// 
			this.m_lblProfile.AutoSize = true;
			this.m_lblProfile.Location = new System.Drawing.Point(6, 9);
			this.m_lblProfile.Name = "m_lblProfile";
			this.m_lblProfile.Size = new System.Drawing.Size(39, 13);
			this.m_lblProfile.TabIndex = 0;
			this.m_lblProfile.Text = "Profile:";
			// 
			// m_cmbProfiles
			// 
			this.m_cmbProfiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbProfiles.FormattingEnabled = true;
			this.m_cmbProfiles.Location = new System.Drawing.Point(51, 6);
			this.m_cmbProfiles.Name = "m_cmbProfiles";
			this.m_cmbProfiles.Size = new System.Drawing.Size(338, 21);
			this.m_cmbProfiles.TabIndex = 1;
			this.m_cmbProfiles.SelectedIndexChanged += new System.EventHandler(this.OnProfilesSelectedIndexChanged);
			// 
			// m_btnProfileAdd
			// 
			this.m_btnProfileAdd.Image = global::KeePass.Properties.Resources.B16x16_FileNew;
			this.m_btnProfileAdd.Location = new System.Drawing.Point(396, 6);
			this.m_btnProfileAdd.Name = "m_btnProfileAdd";
			this.m_btnProfileAdd.Size = new System.Drawing.Size(25, 23);
			this.m_btnProfileAdd.TabIndex = 2;
			this.m_ttMain.SetToolTip(this.m_btnProfileAdd, "Add the currently entered name to the list of profiles.");
			this.m_btnProfileAdd.UseVisualStyleBackColor = true;
			this.m_btnProfileAdd.Click += new System.EventHandler(this.OnBtnProfileAdd);
			// 
			// m_btnProfileRemove
			// 
			this.m_btnProfileRemove.Image = global::KeePass.Properties.Resources.B16x16_EditDelete;
			this.m_btnProfileRemove.Location = new System.Drawing.Point(426, 6);
			this.m_btnProfileRemove.Name = "m_btnProfileRemove";
			this.m_btnProfileRemove.Size = new System.Drawing.Size(25, 23);
			this.m_btnProfileRemove.TabIndex = 3;
			this.m_ttMain.SetToolTip(this.m_btnProfileRemove, "Remove the currently selected profile.");
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
			this.m_grpCurOpt.Location = new System.Drawing.Point(6, 31);
			this.m_grpCurOpt.Name = "m_grpCurOpt";
			this.m_grpCurOpt.Size = new System.Drawing.Size(445, 334);
			this.m_grpCurOpt.TabIndex = 4;
			this.m_grpCurOpt.TabStop = false;
			this.m_grpCurOpt.Text = "Current Options";
			// 
			// m_cbEntropy
			// 
			this.m_cbEntropy.AutoSize = true;
			this.m_cbEntropy.Location = new System.Drawing.Point(9, 41);
			this.m_cbEntropy.Name = "m_cbEntropy";
			this.m_cbEntropy.Size = new System.Drawing.Size(144, 17);
			this.m_cbEntropy.TabIndex = 2;
			this.m_cbEntropy.Text = "Collect additional entropy";
			this.m_cbEntropy.UseVisualStyleBackColor = true;
			// 
			// m_tbPattern
			// 
			this.m_tbPattern.Location = new System.Drawing.Point(28, 307);
			this.m_tbPattern.Name = "m_tbPattern";
			this.m_tbPattern.Size = new System.Drawing.Size(411, 20);
			this.m_tbPattern.TabIndex = 8;
			// 
			// m_rbPattern
			// 
			this.m_rbPattern.AutoSize = true;
			this.m_rbPattern.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
			this.m_rbPattern.Location = new System.Drawing.Point(9, 284);
			this.m_rbPattern.Name = "m_rbPattern";
			this.m_rbPattern.Size = new System.Drawing.Size(70, 17);
			this.m_rbPattern.TabIndex = 7;
			this.m_rbPattern.TabStop = true;
			this.m_rbPattern.Text = "Pattern:";
			this.m_rbPattern.UseVisualStyleBackColor = true;
			this.m_rbPattern.CheckedChanged += new System.EventHandler(this.OnPatternCheckedChanged);
			// 
			// m_btnHelp
			// 
			this.m_btnHelp.Location = new System.Drawing.Point(12, 470);
			this.m_btnHelp.Name = "m_btnHelp";
			this.m_btnHelp.Size = new System.Drawing.Size(75, 23);
			this.m_btnHelp.TabIndex = 3;
			this.m_btnHelp.Text = "&Help";
			this.m_btnHelp.UseVisualStyleBackColor = true;
			this.m_btnHelp.Click += new System.EventHandler(this.OnBtnHelp);
			// 
			// m_tabMain
			// 
			this.m_tabMain.Controls.Add(this.m_tabSettings);
			this.m_tabMain.Controls.Add(this.m_tabPreview);
			this.m_tabMain.Location = new System.Drawing.Point(12, 66);
			this.m_tabMain.Name = "m_tabMain";
			this.m_tabMain.SelectedIndex = 0;
			this.m_tabMain.Size = new System.Drawing.Size(465, 396);
			this.m_tabMain.TabIndex = 2;
			this.m_tabMain.SelectedIndexChanged += new System.EventHandler(this.OnTabMainSelectedIndexChanged);
			// 
			// m_tabSettings
			// 
			this.m_tabSettings.Controls.Add(this.m_lblProfile);
			this.m_tabSettings.Controls.Add(this.m_cmbProfiles);
			this.m_tabSettings.Controls.Add(this.m_grpCurOpt);
			this.m_tabSettings.Controls.Add(this.m_btnProfileAdd);
			this.m_tabSettings.Controls.Add(this.m_btnProfileRemove);
			this.m_tabSettings.Location = new System.Drawing.Point(4, 22);
			this.m_tabSettings.Name = "m_tabSettings";
			this.m_tabSettings.Padding = new System.Windows.Forms.Padding(3);
			this.m_tabSettings.Size = new System.Drawing.Size(457, 370);
			this.m_tabSettings.TabIndex = 0;
			this.m_tabSettings.Text = "Settings";
			this.m_tabSettings.UseVisualStyleBackColor = true;
			// 
			// m_tabPreview
			// 
			this.m_tabPreview.Controls.Add(this.m_pbPreview);
			this.m_tabPreview.Controls.Add(this.m_tbPreview);
			this.m_tabPreview.Controls.Add(this.m_lblPreview);
			this.m_tabPreview.Location = new System.Drawing.Point(4, 22);
			this.m_tabPreview.Name = "m_tabPreview";
			this.m_tabPreview.Padding = new System.Windows.Forms.Padding(3);
			this.m_tabPreview.Size = new System.Drawing.Size(457, 370);
			this.m_tabPreview.TabIndex = 1;
			this.m_tabPreview.Text = "Preview";
			this.m_tabPreview.UseVisualStyleBackColor = true;
			// 
			// m_pbPreview
			// 
			this.m_pbPreview.Location = new System.Drawing.Point(9, 49);
			this.m_pbPreview.Name = "m_pbPreview";
			this.m_pbPreview.Size = new System.Drawing.Size(442, 15);
			this.m_pbPreview.TabIndex = 1;
			// 
			// m_tbPreview
			// 
			this.m_tbPreview.Location = new System.Drawing.Point(9, 70);
			this.m_tbPreview.Multiline = true;
			this.m_tbPreview.Name = "m_tbPreview";
			this.m_tbPreview.ReadOnly = true;
			this.m_tbPreview.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.m_tbPreview.Size = new System.Drawing.Size(442, 294);
			this.m_tbPreview.TabIndex = 2;
			this.m_tbPreview.WordWrap = false;
			// 
			// m_lblPreview
			// 
			this.m_lblPreview.Location = new System.Drawing.Point(6, 12);
			this.m_lblPreview.Name = "m_lblPreview";
			this.m_lblPreview.Size = new System.Drawing.Size(445, 27);
			this.m_lblPreview.TabIndex = 0;
			this.m_lblPreview.Text = "Here you see a few generated passwords structurally equal to the ones that are ge" +
				"nerated when you click [OK].";
			// 
			// PwGeneratorForm
			// 
			this.AcceptButton = this.m_btnOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(489, 505);
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
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Password Generator";
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