namespace KeePass.Forms
{
	partial class KeyCreationForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KeyCreationForm));
			this.m_lblIntro = new System.Windows.Forms.Label();
			this.m_lblMultiInfo = new System.Windows.Forms.Label();
			this.m_cbPassword = new System.Windows.Forms.CheckBox();
			this.m_tbPassword = new System.Windows.Forms.TextBox();
			this.m_lblRepeatPassword = new System.Windows.Forms.Label();
			this.m_tbRepeatPassword = new System.Windows.Forms.TextBox();
			this.m_cbKeyFile = new System.Windows.Forms.CheckBox();
			this.m_cbUserAccount = new System.Windows.Forms.CheckBox();
			this.m_lblWindowsAccDesc = new System.Windows.Forms.Label();
			this.m_lblKeyFileInfo = new System.Windows.Forms.Label();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_btnCreate = new System.Windows.Forms.Button();
			this.m_ttRect = new System.Windows.Forms.ToolTip(this.components);
			this.m_btnSaveKeyFile = new System.Windows.Forms.Button();
			this.m_cbHidePassword = new System.Windows.Forms.CheckBox();
			this.m_btnOpenKeyFile = new System.Windows.Forms.Button();
			this.m_btnHelp = new System.Windows.Forms.Button();
			this.m_lblSeparator = new System.Windows.Forms.Label();
			this.m_pbPasswordQuality = new KeePass.UI.QualityProgressBar();
			this.m_lblEstimatedQuality = new System.Windows.Forms.Label();
			this.m_lblQualityBits = new System.Windows.Forms.Label();
			this.m_tbKeyFile = new System.Windows.Forms.TextBox();
			this.m_bannerImage = new System.Windows.Forms.PictureBox();
			this.m_saveKeyFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.m_openKeyFileDialog = new System.Windows.Forms.OpenFileDialog();
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
			this.SuspendLayout();
			// 
			// m_lblIntro
			// 
			this.m_lblIntro.Location = new System.Drawing.Point(9, 72);
			this.m_lblIntro.Name = "m_lblIntro";
			this.m_lblIntro.Size = new System.Drawing.Size(457, 29);
			this.m_lblIntro.TabIndex = 18;
			this.m_lblIntro.Text = "You are creating a new password database.  Specify the composite master key, whic" +
				"h will be used to encrypt the database.";
			// 
			// m_lblMultiInfo
			// 
			this.m_lblMultiInfo.Location = new System.Drawing.Point(9, 110);
			this.m_lblMultiInfo.Name = "m_lblMultiInfo";
			this.m_lblMultiInfo.Size = new System.Drawing.Size(457, 42);
			this.m_lblMultiInfo.TabIndex = 19;
			this.m_lblMultiInfo.Text = resources.GetString("m_lblMultiInfo.Text");
			// 
			// m_cbPassword
			// 
			this.m_cbPassword.AutoSize = true;
			this.m_cbPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
			this.m_cbPassword.Location = new System.Drawing.Point(12, 164);
			this.m_cbPassword.Name = "m_cbPassword";
			this.m_cbPassword.Size = new System.Drawing.Size(126, 17);
			this.m_cbPassword.TabIndex = 20;
			this.m_cbPassword.Text = "Master Password:";
			this.m_cbPassword.UseVisualStyleBackColor = true;
			this.m_cbPassword.CheckedChanged += new System.EventHandler(this.OnCheckedPassword);
			// 
			// m_tbPassword
			// 
			this.m_tbPassword.Location = new System.Drawing.Point(144, 162);
			this.m_tbPassword.Name = "m_tbPassword";
			this.m_tbPassword.Size = new System.Drawing.Size(322, 20);
			this.m_tbPassword.TabIndex = 0;
			this.m_tbPassword.UseSystemPasswordChar = true;
			// 
			// m_lblRepeatPassword
			// 
			this.m_lblRepeatPassword.AutoSize = true;
			this.m_lblRepeatPassword.Location = new System.Drawing.Point(28, 191);
			this.m_lblRepeatPassword.Name = "m_lblRepeatPassword";
			this.m_lblRepeatPassword.Size = new System.Drawing.Size(93, 13);
			this.m_lblRepeatPassword.TabIndex = 2;
			this.m_lblRepeatPassword.Text = "Repeat password:";
			// 
			// m_tbRepeatPassword
			// 
			this.m_tbRepeatPassword.Location = new System.Drawing.Point(144, 188);
			this.m_tbRepeatPassword.Name = "m_tbRepeatPassword";
			this.m_tbRepeatPassword.Size = new System.Drawing.Size(322, 20);
			this.m_tbRepeatPassword.TabIndex = 3;
			this.m_tbRepeatPassword.UseSystemPasswordChar = true;
			// 
			// m_cbKeyFile
			// 
			this.m_cbKeyFile.AutoSize = true;
			this.m_cbKeyFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
			this.m_cbKeyFile.Location = new System.Drawing.Point(12, 240);
			this.m_cbKeyFile.Name = "m_cbKeyFile";
			this.m_cbKeyFile.Size = new System.Drawing.Size(75, 17);
			this.m_cbKeyFile.TabIndex = 7;
			this.m_cbKeyFile.Text = "Key File:";
			this.m_cbKeyFile.UseVisualStyleBackColor = true;
			this.m_cbKeyFile.CheckedChanged += new System.EventHandler(this.OnCheckedKeyFile);
			// 
			// m_cbUserAccount
			// 
			this.m_cbUserAccount.AutoSize = true;
			this.m_cbUserAccount.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
			this.m_cbUserAccount.Location = new System.Drawing.Point(12, 288);
			this.m_cbUserAccount.Name = "m_cbUserAccount";
			this.m_cbUserAccount.Size = new System.Drawing.Size(158, 17);
			this.m_cbUserAccount.TabIndex = 12;
			this.m_cbUserAccount.Text = "Windows User Account";
			this.m_cbUserAccount.UseVisualStyleBackColor = true;
			this.m_cbUserAccount.CheckedChanged += new System.EventHandler(this.OnWinUserCheckedChanged);
			// 
			// m_lblWindowsAccDesc
			// 
			this.m_lblWindowsAccDesc.Location = new System.Drawing.Point(28, 308);
			this.m_lblWindowsAccDesc.Name = "m_lblWindowsAccDesc";
			this.m_lblWindowsAccDesc.Size = new System.Drawing.Size(479, 29);
			this.m_lblWindowsAccDesc.TabIndex = 13;
			this.m_lblWindowsAccDesc.Text = "This source uses the logon credentials of the current Windows user. These credent" +
				"ials don\'t change if the Windows logon password changes.";
			// 
			// m_lblKeyFileInfo
			// 
			this.m_lblKeyFileInfo.Location = new System.Drawing.Point(28, 265);
			this.m_lblKeyFileInfo.Name = "m_lblKeyFileInfo";
			this.m_lblKeyFileInfo.Size = new System.Drawing.Size(438, 16);
			this.m_lblKeyFileInfo.TabIndex = 11;
			this.m_lblKeyFileInfo.Text = "Create a new key file or browse your disks for an existing one.";
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_btnCancel.Location = new System.Drawing.Point(432, 357);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 15;
			this.m_btnCancel.Text = "&Cancel";
			this.m_btnCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.m_btnCancel.UseVisualStyleBackColor = true;
			this.m_btnCancel.Click += new System.EventHandler(this.OnBtnCancel);
			// 
			// m_btnCreate
			// 
			this.m_btnCreate.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_btnCreate.Location = new System.Drawing.Point(351, 357);
			this.m_btnCreate.Name = "m_btnCreate";
			this.m_btnCreate.Size = new System.Drawing.Size(75, 23);
			this.m_btnCreate.TabIndex = 14;
			this.m_btnCreate.Text = "&OK";
			this.m_btnCreate.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.m_btnCreate.UseVisualStyleBackColor = true;
			this.m_btnCreate.Click += new System.EventHandler(this.OnBtnOK);
			// 
			// m_ttRect
			// 
			this.m_ttRect.AutomaticDelay = 250;
			this.m_ttRect.AutoPopDelay = 5000;
			this.m_ttRect.InitialDelay = 250;
			this.m_ttRect.ReshowDelay = 50;
			// 
			// m_btnSaveKeyFile
			// 
			this.m_btnSaveKeyFile.Image = global::KeePass.Properties.Resources.B15x14_FileNew;
			this.m_btnSaveKeyFile.Location = new System.Drawing.Point(351, 236);
			this.m_btnSaveKeyFile.Name = "m_btnSaveKeyFile";
			this.m_btnSaveKeyFile.Size = new System.Drawing.Size(75, 23);
			this.m_btnSaveKeyFile.TabIndex = 9;
			this.m_btnSaveKeyFile.Text = " C&reate";
			this.m_btnSaveKeyFile.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.m_ttRect.SetToolTip(this.m_btnSaveKeyFile, "Create a new key file.");
			this.m_btnSaveKeyFile.UseVisualStyleBackColor = true;
			this.m_btnSaveKeyFile.Click += new System.EventHandler(this.OnClickKeyFileCreate);
			// 
			// m_cbHidePassword
			// 
			this.m_cbHidePassword.Appearance = System.Windows.Forms.Appearance.Button;
			this.m_cbHidePassword.Image = global::KeePass.Properties.Resources.B17x05_3BlackDots;
			this.m_cbHidePassword.Location = new System.Drawing.Point(475, 160);
			this.m_cbHidePassword.Name = "m_cbHidePassword";
			this.m_cbHidePassword.Size = new System.Drawing.Size(32, 23);
			this.m_cbHidePassword.TabIndex = 1;
			this.m_ttRect.SetToolTip(this.m_cbHidePassword, "Show/hide password using asterisks.");
			this.m_cbHidePassword.UseVisualStyleBackColor = true;
			this.m_cbHidePassword.CheckedChanged += new System.EventHandler(this.OnCheckedHidePassword);
			// 
			// m_btnOpenKeyFile
			// 
			this.m_btnOpenKeyFile.Image = global::KeePass.Properties.Resources.B16x16_Folder_Blue_Open;
			this.m_btnOpenKeyFile.Location = new System.Drawing.Point(432, 236);
			this.m_btnOpenKeyFile.Name = "m_btnOpenKeyFile";
			this.m_btnOpenKeyFile.Size = new System.Drawing.Size(75, 23);
			this.m_btnOpenKeyFile.TabIndex = 10;
			this.m_btnOpenKeyFile.Text = " &Browse";
			this.m_btnOpenKeyFile.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.m_ttRect.SetToolTip(this.m_btnOpenKeyFile, "Use an existing file as key file.");
			this.m_btnOpenKeyFile.UseVisualStyleBackColor = true;
			this.m_btnOpenKeyFile.Click += new System.EventHandler(this.OnClickKeyFileBrowse);
			// 
			// m_btnHelp
			// 
			this.m_btnHelp.Location = new System.Drawing.Point(12, 357);
			this.m_btnHelp.Name = "m_btnHelp";
			this.m_btnHelp.Size = new System.Drawing.Size(75, 23);
			this.m_btnHelp.TabIndex = 16;
			this.m_btnHelp.Text = "&Help";
			this.m_btnHelp.UseVisualStyleBackColor = true;
			// 
			// m_lblSeparator
			// 
			this.m_lblSeparator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.m_lblSeparator.Location = new System.Drawing.Point(0, 346);
			this.m_lblSeparator.Name = "m_lblSeparator";
			this.m_lblSeparator.Size = new System.Drawing.Size(519, 2);
			this.m_lblSeparator.TabIndex = 17;
			// 
			// m_pbPasswordQuality
			// 
			this.m_pbPasswordQuality.Location = new System.Drawing.Point(144, 214);
			this.m_pbPasswordQuality.Maximum = 100;
			this.m_pbPasswordQuality.Minimum = 0;
			this.m_pbPasswordQuality.Name = "m_pbPasswordQuality";
			this.m_pbPasswordQuality.Size = new System.Drawing.Size(266, 14);
			this.m_pbPasswordQuality.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			this.m_pbPasswordQuality.TabIndex = 5;
			this.m_pbPasswordQuality.Value = 0;
			// 
			// m_lblEstimatedQuality
			// 
			this.m_lblEstimatedQuality.AutoSize = true;
			this.m_lblEstimatedQuality.Location = new System.Drawing.Point(28, 215);
			this.m_lblEstimatedQuality.Name = "m_lblEstimatedQuality";
			this.m_lblEstimatedQuality.Size = new System.Drawing.Size(89, 13);
			this.m_lblEstimatedQuality.TabIndex = 4;
			this.m_lblEstimatedQuality.Text = "Estimated quality:";
			// 
			// m_lblQualityBits
			// 
			this.m_lblQualityBits.Location = new System.Drawing.Point(413, 214);
			this.m_lblQualityBits.Name = "m_lblQualityBits";
			this.m_lblQualityBits.Size = new System.Drawing.Size(53, 13);
			this.m_lblQualityBits.TabIndex = 6;
			this.m_lblQualityBits.Text = "9999 bits";
			this.m_lblQualityBits.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// m_tbKeyFile
			// 
			this.m_tbKeyFile.Location = new System.Drawing.Point(144, 237);
			this.m_tbKeyFile.Name = "m_tbKeyFile";
			this.m_tbKeyFile.ReadOnly = true;
			this.m_tbKeyFile.Size = new System.Drawing.Size(201, 20);
			this.m_tbKeyFile.TabIndex = 8;
			// 
			// m_bannerImage
			// 
			this.m_bannerImage.Dock = System.Windows.Forms.DockStyle.Top;
			this.m_bannerImage.Location = new System.Drawing.Point(0, 0);
			this.m_bannerImage.Name = "m_bannerImage";
			this.m_bannerImage.Size = new System.Drawing.Size(519, 60);
			this.m_bannerImage.TabIndex = 15;
			this.m_bannerImage.TabStop = false;
			// 
			// m_saveKeyFileDialog
			// 
			this.m_saveKeyFileDialog.DefaultExt = "key";
			this.m_saveKeyFileDialog.Filter = "Key Files (*.key)|*.key|All Files (*.*)|*.*";
			this.m_saveKeyFileDialog.RestoreDirectory = true;
			this.m_saveKeyFileDialog.SupportMultiDottedExtensions = true;
			this.m_saveKeyFileDialog.Title = "Create Key File";
			// 
			// m_openKeyFileDialog
			// 
			this.m_openKeyFileDialog.Filter = "Key Files (*.key)|*.key|All Files (*.*)|*.*";
			this.m_openKeyFileDialog.RestoreDirectory = true;
			this.m_openKeyFileDialog.SupportMultiDottedExtensions = true;
			this.m_openKeyFileDialog.Title = "Use existing file as key file";
			// 
			// KeyCreationForm
			// 
			this.AcceptButton = this.m_btnCreate;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(519, 392);
			this.Controls.Add(this.m_tbKeyFile);
			this.Controls.Add(this.m_lblQualityBits);
			this.Controls.Add(this.m_lblEstimatedQuality);
			this.Controls.Add(this.m_pbPasswordQuality);
			this.Controls.Add(this.m_lblSeparator);
			this.Controls.Add(this.m_btnHelp);
			this.Controls.Add(this.m_lblKeyFileInfo);
			this.Controls.Add(this.m_lblWindowsAccDesc);
			this.Controls.Add(this.m_cbHidePassword);
			this.Controls.Add(this.m_bannerImage);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnCreate);
			this.Controls.Add(this.m_cbUserAccount);
			this.Controls.Add(this.m_btnSaveKeyFile);
			this.Controls.Add(this.m_btnOpenKeyFile);
			this.Controls.Add(this.m_cbKeyFile);
			this.Controls.Add(this.m_tbRepeatPassword);
			this.Controls.Add(this.m_lblRepeatPassword);
			this.Controls.Add(this.m_tbPassword);
			this.Controls.Add(this.m_cbPassword);
			this.Controls.Add(this.m_lblMultiInfo);
			this.Controls.Add(this.m_lblIntro);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "KeyCreationForm";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Create New Password Database - Step 1";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
			this.Load += new System.EventHandler(this.OnFormLoad);
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label m_lblIntro;
		private System.Windows.Forms.Label m_lblMultiInfo;
		private System.Windows.Forms.CheckBox m_cbPassword;
		private System.Windows.Forms.TextBox m_tbPassword;
		private System.Windows.Forms.Label m_lblRepeatPassword;
		private System.Windows.Forms.TextBox m_tbRepeatPassword;
		private System.Windows.Forms.CheckBox m_cbKeyFile;
		private System.Windows.Forms.Button m_btnOpenKeyFile;
		private System.Windows.Forms.Button m_btnSaveKeyFile;
		private System.Windows.Forms.CheckBox m_cbUserAccount;
		private System.Windows.Forms.Button m_btnCreate;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.PictureBox m_bannerImage;
		private System.Windows.Forms.CheckBox m_cbHidePassword;
		private System.Windows.Forms.Label m_lblWindowsAccDesc;
		private System.Windows.Forms.Label m_lblKeyFileInfo;
		private System.Windows.Forms.ToolTip m_ttRect;
		private System.Windows.Forms.Button m_btnHelp;
		private System.Windows.Forms.Label m_lblSeparator;
		private KeePass.UI.QualityProgressBar m_pbPasswordQuality;
		private System.Windows.Forms.Label m_lblEstimatedQuality;
		private System.Windows.Forms.Label m_lblQualityBits;
		private System.Windows.Forms.TextBox m_tbKeyFile;
		private System.Windows.Forms.SaveFileDialog m_saveKeyFileDialog;
		private System.Windows.Forms.OpenFileDialog m_openKeyFileDialog;
	}
}