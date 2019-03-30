namespace KeePass.Forms
{
	partial class KeyPromptForm
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
            this.m_cbUserAccount = new System.Windows.Forms.CheckBox();
            this.m_btnOpenKeyFile = new System.Windows.Forms.Button();
            this.m_cbKeyFile = new System.Windows.Forms.CheckBox();
            this.m_tbPassword = new System.Windows.Forms.TextBox();
            this.m_cbPassword = new System.Windows.Forms.CheckBox();
            this.m_btnOK = new System.Windows.Forms.Button();
            this.m_btnCancel = new System.Windows.Forms.Button();
            this.m_bannerImage = new System.Windows.Forms.PictureBox();
            this.m_cbHidePassword = new System.Windows.Forms.CheckBox();
            this.m_ttRect = new System.Windows.Forms.ToolTip(this.components);
            this.m_btnHelp = new System.Windows.Forms.Button();
            this.m_lblSeparator = new System.Windows.Forms.Label();
            this.m_cmbKeyFile = new System.Windows.Forms.ComboBox();
            this.m_btnExit = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
            this.SuspendLayout();
            // 
            // m_cbUserAccount
            // 
            this.m_cbUserAccount.AutoSize = true;
            this.m_cbUserAccount.Location = new System.Drawing.Point(16, 192);
            this.m_cbUserAccount.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cbUserAccount.Name = "m_cbUserAccount";
            this.m_cbUserAccount.Size = new System.Drawing.Size(183, 24);
            this.m_cbUserAccount.TabIndex = 5;
            this.m_cbUserAccount.Text = "Windows &User Account";
            this.m_cbUserAccount.UseVisualStyleBackColor = true;
            // 
            // m_btnOpenKeyFile
            // 
            this.m_btnOpenKeyFile.Image = global::KeePass.Properties.Resources.B16x16_Folder_Blue_Open;
            this.m_btnOpenKeyFile.Location = new System.Drawing.Point(497, 149);
            this.m_btnOpenKeyFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnOpenKeyFile.Name = "m_btnOpenKeyFile";
            this.m_btnOpenKeyFile.Size = new System.Drawing.Size(43, 35);
            this.m_btnOpenKeyFile.TabIndex = 4;
            this.m_btnOpenKeyFile.UseVisualStyleBackColor = true;
            this.m_btnOpenKeyFile.Click += new System.EventHandler(this.OnClickKeyFileBrowse);
            // 
            // m_cbKeyFile
            // 
            this.m_cbKeyFile.AutoSize = true;
            this.m_cbKeyFile.Location = new System.Drawing.Point(16, 154);
            this.m_cbKeyFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cbKeyFile.Name = "m_cbKeyFile";
            this.m_cbKeyFile.Size = new System.Drawing.Size(85, 24);
            this.m_cbKeyFile.TabIndex = 2;
            this.m_cbKeyFile.Text = "&Key File:";
            this.m_cbKeyFile.UseVisualStyleBackColor = true;
            this.m_cbKeyFile.CheckedChanged += new System.EventHandler(this.OnCheckedKeyFile);
            // 
            // m_tbPassword
            // 
            this.m_tbPassword.Location = new System.Drawing.Point(192, 114);
            this.m_tbPassword.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_tbPassword.Name = "m_tbPassword";
            this.m_tbPassword.Size = new System.Drawing.Size(296, 27);
            this.m_tbPassword.TabIndex = 0;
            this.m_tbPassword.UseSystemPasswordChar = true;
            // 
            // m_cbPassword
            // 
            this.m_cbPassword.AutoSize = true;
            this.m_cbPassword.Location = new System.Drawing.Point(16, 115);
            this.m_cbPassword.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cbPassword.Name = "m_cbPassword";
            this.m_cbPassword.Size = new System.Drawing.Size(144, 24);
            this.m_cbPassword.TabIndex = 10;
            this.m_cbPassword.Text = "Master &Password:";
            this.m_cbPassword.UseVisualStyleBackColor = true;
            this.m_cbPassword.CheckedChanged += new System.EventHandler(this.OnCheckedPassword);
            // 
            // m_btnOK
            // 
            this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_btnOK.Location = new System.Drawing.Point(332, 272);
            this.m_btnOK.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnOK.Name = "m_btnOK";
            this.m_btnOK.Size = new System.Drawing.Size(100, 35);
            this.m_btnOK.TabIndex = 6;
            this.m_btnOK.Text = "OK";
            this.m_btnOK.UseVisualStyleBackColor = true;
            this.m_btnOK.Click += new System.EventHandler(this.OnBtnOK);
            // 
            // m_btnCancel
            // 
            this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_btnCancel.Location = new System.Drawing.Point(440, 272);
            this.m_btnCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnCancel.Name = "m_btnCancel";
            this.m_btnCancel.Size = new System.Drawing.Size(100, 35);
            this.m_btnCancel.TabIndex = 7;
            this.m_btnCancel.Text = "Cancel";
            this.m_btnCancel.UseVisualStyleBackColor = true;
            this.m_btnCancel.Click += new System.EventHandler(this.OnBtnCancel);
            // 
            // m_bannerImage
            // 
            this.m_bannerImage.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_bannerImage.Location = new System.Drawing.Point(0, 0);
            this.m_bannerImage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_bannerImage.Name = "m_bannerImage";
            this.m_bannerImage.Size = new System.Drawing.Size(556, 92);
            this.m_bannerImage.TabIndex = 24;
            this.m_bannerImage.TabStop = false;
            // 
            // m_cbHidePassword
            // 
            this.m_cbHidePassword.Appearance = System.Windows.Forms.Appearance.Button;
            this.m_cbHidePassword.Location = new System.Drawing.Point(497, 111);
            this.m_cbHidePassword.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cbHidePassword.Name = "m_cbHidePassword";
            this.m_cbHidePassword.Size = new System.Drawing.Size(43, 35);
            this.m_cbHidePassword.TabIndex = 1;
            this.m_cbHidePassword.Text = "***";
            this.m_cbHidePassword.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.m_cbHidePassword.UseVisualStyleBackColor = true;
            this.m_cbHidePassword.CheckedChanged += new System.EventHandler(this.OnCheckedHidePassword);
            // 
            // m_ttRect
            // 
            this.m_ttRect.StripAmpersands = true;
            // 
            // m_btnHelp
            // 
            this.m_btnHelp.Location = new System.Drawing.Point(16, 272);
            this.m_btnHelp.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnHelp.Name = "m_btnHelp";
            this.m_btnHelp.Size = new System.Drawing.Size(100, 35);
            this.m_btnHelp.TabIndex = 8;
            this.m_btnHelp.Text = "&Help";
            this.m_btnHelp.UseVisualStyleBackColor = true;
            this.m_btnHelp.Click += new System.EventHandler(this.OnBtnHelp);
            // 
            // m_lblSeparator
            // 
            this.m_lblSeparator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.m_lblSeparator.Location = new System.Drawing.Point(0, 252);
            this.m_lblSeparator.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblSeparator.Name = "m_lblSeparator";
            this.m_lblSeparator.Size = new System.Drawing.Size(556, 2);
            this.m_lblSeparator.TabIndex = 9;
            // 
            // m_cmbKeyFile
            // 
            this.m_cmbKeyFile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_cmbKeyFile.FormattingEnabled = true;
            this.m_cmbKeyFile.Location = new System.Drawing.Point(192, 151);
            this.m_cmbKeyFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cmbKeyFile.Name = "m_cmbKeyFile";
            this.m_cmbKeyFile.Size = new System.Drawing.Size(296, 28);
            this.m_cmbKeyFile.TabIndex = 3;
            this.m_cmbKeyFile.SelectedIndexChanged += new System.EventHandler(this.OnKeyFileSelectedIndexChanged);
            // 
            // m_btnExit
            // 
            this.m_btnExit.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this.m_btnExit.Location = new System.Drawing.Point(124, 272);
            this.m_btnExit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnExit.Name = "m_btnExit";
            this.m_btnExit.Size = new System.Drawing.Size(100, 35);
            this.m_btnExit.TabIndex = 25;
            this.m_btnExit.Text = "E&xit";
            this.m_btnExit.UseVisualStyleBackColor = true;
            this.m_btnExit.Click += new System.EventHandler(this.OnBtnExit);
            // 
            // KeyPromptForm
            // 
            this.AcceptButton = this.m_btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.m_btnCancel;
            this.ClientSize = new System.Drawing.Size(556, 326);
            this.Controls.Add(this.m_btnExit);
            this.Controls.Add(this.m_cmbKeyFile);
            this.Controls.Add(this.m_lblSeparator);
            this.Controls.Add(this.m_btnHelp);
            this.Controls.Add(this.m_cbHidePassword);
            this.Controls.Add(this.m_bannerImage);
            this.Controls.Add(this.m_btnCancel);
            this.Controls.Add(this.m_btnOK);
            this.Controls.Add(this.m_cbUserAccount);
            this.Controls.Add(this.m_btnOpenKeyFile);
            this.Controls.Add(this.m_cbKeyFile);
            this.Controls.Add(this.m_tbPassword);
            this.Controls.Add(this.m_cbPassword);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "KeyPromptForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "<>";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.Shown += new System.EventHandler(this.OnFormShown);
            ((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.CheckBox m_cbUserAccount;
		private System.Windows.Forms.Button m_btnOpenKeyFile;
		private System.Windows.Forms.CheckBox m_cbKeyFile;
		private System.Windows.Forms.TextBox m_tbPassword;
		private System.Windows.Forms.CheckBox m_cbPassword;
		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.PictureBox m_bannerImage;
		private System.Windows.Forms.CheckBox m_cbHidePassword;
		private System.Windows.Forms.ToolTip m_ttRect;
		private System.Windows.Forms.Button m_btnHelp;
		private System.Windows.Forms.Label m_lblSeparator;
		private System.Windows.Forms.ComboBox m_cmbKeyFile;
		private System.Windows.Forms.Button m_btnExit;
	}
}