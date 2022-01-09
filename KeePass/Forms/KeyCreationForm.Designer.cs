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
			this.m_tbPassword = new KeePass.UI.SecureTextBoxEx();
			this.m_lblRepeatPassword = new System.Windows.Forms.Label();
			this.m_tbRepeatPassword = new KeePass.UI.SecureTextBoxEx();
			this.m_cbKeyFile = new System.Windows.Forms.CheckBox();
			this.m_cbUserAccount = new System.Windows.Forms.CheckBox();
			this.m_lblWindowsAccDesc = new System.Windows.Forms.Label();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_btnOK = new System.Windows.Forms.Button();
			this.m_ttRect = new System.Windows.Forms.ToolTip(this.components);
			this.m_cbHidePassword = new System.Windows.Forms.CheckBox();
			this.m_btnSaveKeyFile = new System.Windows.Forms.Button();
			this.m_btnOpenKeyFile = new System.Windows.Forms.Button();
			this.m_btnHelp = new System.Windows.Forms.Button();
			this.m_lblSeparator = new System.Windows.Forms.Label();
			this.m_lblEstimatedQuality = new System.Windows.Forms.Label();
			this.m_lblQualityInfo = new System.Windows.Forms.Label();
			this.m_bannerImage = new System.Windows.Forms.PictureBox();
			this.m_cmbKeyFile = new System.Windows.Forms.ComboBox();
			this.m_lblWindowsAccDesc2 = new System.Windows.Forms.Label();
			this.m_picAccWarning = new System.Windows.Forms.PictureBox();
			this.m_cbExpert = new System.Windows.Forms.CheckBox();
			this.m_picKeyFileWarning = new System.Windows.Forms.PictureBox();
			this.m_lblKeyFileWarning = new System.Windows.Forms.Label();
			this.m_lnkKeyFile = new System.Windows.Forms.LinkLabel();
			this.m_lnkUserAccount = new System.Windows.Forms.LinkLabel();
			this.m_pbPasswordQuality = new KeePass.UI.QualityProgressBar();
			this.m_lblKeyFileInfo = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_picAccWarning)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_picKeyFileWarning)).BeginInit();
			this.SuspendLayout();
			// 
			// m_lblIntro
			// 
			this.m_lblIntro.Location = new System.Drawing.Point(9, 72);
			this.m_lblIntro.Name = "m_lblIntro";
			this.m_lblIntro.Size = new System.Drawing.Size(498, 13);
			this.m_lblIntro.TabIndex = 2;
			this.m_lblIntro.Text = "Specify the master key that will be used to encrypt the database.";
			// 
			// m_lblMultiInfo
			// 
			this.m_lblMultiInfo.Location = new System.Drawing.Point(9, 93);
			this.m_lblMultiInfo.Name = "m_lblMultiInfo";
			this.m_lblMultiInfo.Size = new System.Drawing.Size(498, 42);
			this.m_lblMultiInfo.TabIndex = 3;
			this.m_lblMultiInfo.Text = resources.GetString("m_lblMultiInfo.Text");
			// 
			// m_cbPassword
			// 
			this.m_cbPassword.AutoSize = true;
			this.m_cbPassword.Location = new System.Drawing.Point(12, 147);
			this.m_cbPassword.Name = "m_cbPassword";
			this.m_cbPassword.Size = new System.Drawing.Size(109, 17);
			this.m_cbPassword.TabIndex = 4;
			this.m_cbPassword.Text = "&Master password:";
			this.m_cbPassword.UseVisualStyleBackColor = true;
			this.m_cbPassword.CheckedChanged += new System.EventHandler(this.OnPasswordCheckedChanged);
			// 
			// m_tbPassword
			// 
			this.m_tbPassword.Location = new System.Drawing.Point(150, 145);
			this.m_tbPassword.Name = "m_tbPassword";
			this.m_tbPassword.Size = new System.Drawing.Size(319, 20);
			this.m_tbPassword.TabIndex = 5;
			this.m_tbPassword.UseSystemPasswordChar = true;
			// 
			// m_lblRepeatPassword
			// 
			this.m_lblRepeatPassword.AutoSize = true;
			this.m_lblRepeatPassword.Location = new System.Drawing.Point(28, 174);
			this.m_lblRepeatPassword.Name = "m_lblRepeatPassword";
			this.m_lblRepeatPassword.Size = new System.Drawing.Size(93, 13);
			this.m_lblRepeatPassword.TabIndex = 7;
			this.m_lblRepeatPassword.Text = "&Repeat password:";
			// 
			// m_tbRepeatPassword
			// 
			this.m_tbRepeatPassword.Location = new System.Drawing.Point(150, 171);
			this.m_tbRepeatPassword.Name = "m_tbRepeatPassword";
			this.m_tbRepeatPassword.Size = new System.Drawing.Size(319, 20);
			this.m_tbRepeatPassword.TabIndex = 8;
			this.m_tbRepeatPassword.UseSystemPasswordChar = true;
			// 
			// m_cbKeyFile
			// 
			this.m_cbKeyFile.AutoSize = true;
			this.m_cbKeyFile.Location = new System.Drawing.Point(12, 249);
			this.m_cbKeyFile.Name = "m_cbKeyFile";
			this.m_cbKeyFile.Size = new System.Drawing.Size(106, 17);
			this.m_cbKeyFile.TabIndex = 13;
			this.m_cbKeyFile.Text = "&Key file/provider:";
			this.m_cbKeyFile.UseVisualStyleBackColor = true;
			this.m_cbKeyFile.CheckedChanged += new System.EventHandler(this.OnKeyFileCheckedChanged);
			// 
			// m_cbUserAccount
			// 
			this.m_cbUserAccount.AutoSize = true;
			this.m_cbUserAccount.Location = new System.Drawing.Point(12, 391);
			this.m_cbUserAccount.Name = "m_cbUserAccount";
			this.m_cbUserAccount.Size = new System.Drawing.Size(135, 17);
			this.m_cbUserAccount.TabIndex = 20;
			this.m_cbUserAccount.Text = "&Windows user account";
			this.m_cbUserAccount.UseVisualStyleBackColor = true;
			this.m_cbUserAccount.CheckedChanged += new System.EventHandler(this.OnUserAccountCheckedChanged);
			// 
			// m_lblWindowsAccDesc
			// 
			this.m_lblWindowsAccDesc.Location = new System.Drawing.Point(28, 411);
			this.m_lblWindowsAccDesc.Name = "m_lblWindowsAccDesc";
			this.m_lblWindowsAccDesc.Size = new System.Drawing.Size(479, 27);
			this.m_lblWindowsAccDesc.TabIndex = 21;
			this.m_lblWindowsAccDesc.Text = "This source uses data of the current Windows user account. This data does not cha" +
				"nge when the account password changes.";
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_btnCancel.Location = new System.Drawing.Point(432, 542);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 1;
			this.m_btnCancel.Text = "Cancel";
			this.m_btnCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.m_btnCancel.UseVisualStyleBackColor = true;
			this.m_btnCancel.Click += new System.EventHandler(this.OnBtnCancel);
			// 
			// m_btnOK
			// 
			this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_btnOK.Location = new System.Drawing.Point(351, 542);
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.Size = new System.Drawing.Size(75, 23);
			this.m_btnOK.TabIndex = 0;
			this.m_btnOK.Text = "OK";
			this.m_btnOK.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.m_btnOK.UseVisualStyleBackColor = true;
			this.m_btnOK.Click += new System.EventHandler(this.OnBtnOK);
			// 
			// m_cbHidePassword
			// 
			this.m_cbHidePassword.Appearance = System.Windows.Forms.Appearance.Button;
			this.m_cbHidePassword.Location = new System.Drawing.Point(475, 143);
			this.m_cbHidePassword.Name = "m_cbHidePassword";
			this.m_cbHidePassword.Size = new System.Drawing.Size(32, 23);
			this.m_cbHidePassword.TabIndex = 6;
			this.m_cbHidePassword.Text = "***";
			this.m_cbHidePassword.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.m_cbHidePassword.UseVisualStyleBackColor = true;
			// 
			// m_btnSaveKeyFile
			// 
			this.m_btnSaveKeyFile.Image = global::KeePass.Properties.Resources.B15x14_FileNew;
			this.m_btnSaveKeyFile.Location = new System.Drawing.Point(341, 273);
			this.m_btnSaveKeyFile.Name = "m_btnSaveKeyFile";
			this.m_btnSaveKeyFile.Size = new System.Drawing.Size(80, 23);
			this.m_btnSaveKeyFile.TabIndex = 15;
			this.m_btnSaveKeyFile.Text = " &Create...";
			this.m_btnSaveKeyFile.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.m_btnSaveKeyFile.UseVisualStyleBackColor = true;
			this.m_btnSaveKeyFile.Click += new System.EventHandler(this.OnClickKeyFileCreate);
			// 
			// m_btnOpenKeyFile
			// 
			this.m_btnOpenKeyFile.Image = global::KeePass.Properties.Resources.B16x16_Folder_Blue_Open;
			this.m_btnOpenKeyFile.Location = new System.Drawing.Point(427, 273);
			this.m_btnOpenKeyFile.Name = "m_btnOpenKeyFile";
			this.m_btnOpenKeyFile.Size = new System.Drawing.Size(80, 23);
			this.m_btnOpenKeyFile.TabIndex = 16;
			this.m_btnOpenKeyFile.Text = " &Browse...";
			this.m_btnOpenKeyFile.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.m_btnOpenKeyFile.UseVisualStyleBackColor = true;
			this.m_btnOpenKeyFile.Click += new System.EventHandler(this.OnClickKeyFileBrowse);
			// 
			// m_btnHelp
			// 
			this.m_btnHelp.Location = new System.Drawing.Point(12, 542);
			this.m_btnHelp.Name = "m_btnHelp";
			this.m_btnHelp.Size = new System.Drawing.Size(75, 23);
			this.m_btnHelp.TabIndex = 25;
			this.m_btnHelp.Text = "&Help";
			this.m_btnHelp.UseVisualStyleBackColor = true;
			this.m_btnHelp.Click += new System.EventHandler(this.OnBtnHelp);
			// 
			// m_lblSeparator
			// 
			this.m_lblSeparator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.m_lblSeparator.Location = new System.Drawing.Point(0, 531);
			this.m_lblSeparator.Name = "m_lblSeparator";
			this.m_lblSeparator.Size = new System.Drawing.Size(519, 2);
			this.m_lblSeparator.TabIndex = 24;
			// 
			// m_lblEstimatedQuality
			// 
			this.m_lblEstimatedQuality.AutoSize = true;
			this.m_lblEstimatedQuality.Location = new System.Drawing.Point(28, 198);
			this.m_lblEstimatedQuality.Name = "m_lblEstimatedQuality";
			this.m_lblEstimatedQuality.Size = new System.Drawing.Size(89, 13);
			this.m_lblEstimatedQuality.TabIndex = 9;
			this.m_lblEstimatedQuality.Text = "Estimated quality:";
			// 
			// m_lblQualityInfo
			// 
			this.m_lblQualityInfo.Location = new System.Drawing.Point(422, 198);
			this.m_lblQualityInfo.Name = "m_lblQualityInfo";
			this.m_lblQualityInfo.Size = new System.Drawing.Size(50, 13);
			this.m_lblQualityInfo.TabIndex = 11;
			this.m_lblQualityInfo.Text = "0 ch.";
			this.m_lblQualityInfo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
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
			// m_cmbKeyFile
			// 
			this.m_cmbKeyFile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbKeyFile.FormattingEnabled = true;
			this.m_cmbKeyFile.Location = new System.Drawing.Point(150, 246);
			this.m_cmbKeyFile.MaxDropDownItems = 16;
			this.m_cmbKeyFile.Name = "m_cmbKeyFile";
			this.m_cmbKeyFile.Size = new System.Drawing.Size(356, 21);
			this.m_cmbKeyFile.TabIndex = 14;
			this.m_cmbKeyFile.SelectedIndexChanged += new System.EventHandler(this.OnKeyFileSelectedIndexChanged);
			// 
			// m_lblWindowsAccDesc2
			// 
			this.m_lblWindowsAccDesc2.Location = new System.Drawing.Point(53, 446);
			this.m_lblWindowsAccDesc2.Name = "m_lblWindowsAccDesc2";
			this.m_lblWindowsAccDesc2.Size = new System.Drawing.Size(454, 54);
			this.m_lblWindowsAccDesc2.TabIndex = 22;
			this.m_lblWindowsAccDesc2.Text = resources.GetString("m_lblWindowsAccDesc2.Text");
			// 
			// m_picAccWarning
			// 
			this.m_picAccWarning.Location = new System.Drawing.Point(31, 446);
			this.m_picAccWarning.Name = "m_picAccWarning";
			this.m_picAccWarning.Size = new System.Drawing.Size(16, 16);
			this.m_picAccWarning.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.m_picAccWarning.TabIndex = 22;
			this.m_picAccWarning.TabStop = false;
			// 
			// m_cbExpert
			// 
			this.m_cbExpert.AutoSize = true;
			this.m_cbExpert.Location = new System.Drawing.Point(12, 223);
			this.m_cbExpert.Name = "m_cbExpert";
			this.m_cbExpert.Size = new System.Drawing.Size(125, 17);
			this.m_cbExpert.TabIndex = 12;
			this.m_cbExpert.Text = "&Show expert options:";
			this.m_cbExpert.UseVisualStyleBackColor = true;
			this.m_cbExpert.CheckedChanged += new System.EventHandler(this.OnExpertCheckedChanged);
			// 
			// m_picKeyFileWarning
			// 
			this.m_picKeyFileWarning.Location = new System.Drawing.Point(31, 334);
			this.m_picKeyFileWarning.Name = "m_picKeyFileWarning";
			this.m_picKeyFileWarning.Size = new System.Drawing.Size(16, 16);
			this.m_picKeyFileWarning.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.m_picKeyFileWarning.TabIndex = 24;
			this.m_picKeyFileWarning.TabStop = false;
			// 
			// m_lblKeyFileWarning
			// 
			this.m_lblKeyFileWarning.Location = new System.Drawing.Point(53, 334);
			this.m_lblKeyFileWarning.Name = "m_lblKeyFileWarning";
			this.m_lblKeyFileWarning.Size = new System.Drawing.Size(454, 28);
			this.m_lblKeyFileWarning.TabIndex = 18;
			this.m_lblKeyFileWarning.Text = "If the key file is lost or its contents are changed, the database cannot be opene" +
				"d anymore. You should create a backup of the key file.";
			// 
			// m_lnkKeyFile
			// 
			this.m_lnkKeyFile.AutoSize = true;
			this.m_lnkKeyFile.Location = new System.Drawing.Point(53, 366);
			this.m_lnkKeyFile.Name = "m_lnkKeyFile";
			this.m_lnkKeyFile.Size = new System.Drawing.Size(159, 13);
			this.m_lnkKeyFile.TabIndex = 19;
			this.m_lnkKeyFile.TabStop = true;
			this.m_lnkKeyFile.Text = "More information about key files.";
			this.m_lnkKeyFile.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnKeyFileLinkClicked);
			// 
			// m_lnkUserAccount
			// 
			this.m_lnkUserAccount.AutoSize = true;
			this.m_lnkUserAccount.Location = new System.Drawing.Point(53, 504);
			this.m_lnkUserAccount.Name = "m_lnkUserAccount";
			this.m_lnkUserAccount.Size = new System.Drawing.Size(235, 13);
			this.m_lnkUserAccount.TabIndex = 23;
			this.m_lnkUserAccount.TabStop = true;
			this.m_lnkUserAccount.Text = "More information about Windows user accounts.";
			this.m_lnkUserAccount.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnUserAccountLinkClicked);
			// 
			// m_pbPasswordQuality
			// 
			this.m_pbPasswordQuality.Location = new System.Drawing.Point(150, 197);
			this.m_pbPasswordQuality.Name = "m_pbPasswordQuality";
			this.m_pbPasswordQuality.Size = new System.Drawing.Size(269, 16);
			this.m_pbPasswordQuality.TabIndex = 10;
			this.m_pbPasswordQuality.TabStop = false;
			// 
			// m_lblKeyFileInfo
			// 
			this.m_lblKeyFileInfo.Location = new System.Drawing.Point(28, 299);
			this.m_lblKeyFileInfo.Name = "m_lblKeyFileInfo";
			this.m_lblKeyFileInfo.Size = new System.Drawing.Size(479, 27);
			this.m_lblKeyFileInfo.TabIndex = 17;
			this.m_lblKeyFileInfo.Text = "A key file can be used as part of the master key; it does not store any database " +
				"data. If an attacker has access to the key file, it does not provide any protect" +
				"ion.";
			// 
			// KeyCreationForm
			// 
			this.AcceptButton = this.m_btnOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(519, 577);
			this.Controls.Add(this.m_lblKeyFileInfo);
			this.Controls.Add(this.m_lnkUserAccount);
			this.Controls.Add(this.m_lnkKeyFile);
			this.Controls.Add(this.m_lblKeyFileWarning);
			this.Controls.Add(this.m_picKeyFileWarning);
			this.Controls.Add(this.m_cbExpert);
			this.Controls.Add(this.m_picAccWarning);
			this.Controls.Add(this.m_lblWindowsAccDesc2);
			this.Controls.Add(this.m_cmbKeyFile);
			this.Controls.Add(this.m_lblQualityInfo);
			this.Controls.Add(this.m_lblEstimatedQuality);
			this.Controls.Add(this.m_pbPasswordQuality);
			this.Controls.Add(this.m_lblSeparator);
			this.Controls.Add(this.m_btnHelp);
			this.Controls.Add(this.m_lblWindowsAccDesc);
			this.Controls.Add(this.m_cbHidePassword);
			this.Controls.Add(this.m_bannerImage);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOK);
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
			this.Text = "<>";
			this.Load += new System.EventHandler(this.OnFormLoad);
			this.Shown += new System.EventHandler(this.OnFormShown);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_picAccWarning)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_picKeyFileWarning)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label m_lblIntro;
		private System.Windows.Forms.Label m_lblMultiInfo;
		private System.Windows.Forms.CheckBox m_cbPassword;
		private KeePass.UI.SecureTextBoxEx m_tbPassword;
		private System.Windows.Forms.Label m_lblRepeatPassword;
		private KeePass.UI.SecureTextBoxEx m_tbRepeatPassword;
		private System.Windows.Forms.CheckBox m_cbKeyFile;
		private System.Windows.Forms.Button m_btnOpenKeyFile;
		private System.Windows.Forms.Button m_btnSaveKeyFile;
		private System.Windows.Forms.CheckBox m_cbUserAccount;
		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.PictureBox m_bannerImage;
		private System.Windows.Forms.CheckBox m_cbHidePassword;
		private System.Windows.Forms.Label m_lblWindowsAccDesc;
		private System.Windows.Forms.ToolTip m_ttRect;
		private System.Windows.Forms.Button m_btnHelp;
		private System.Windows.Forms.Label m_lblSeparator;
		private KeePass.UI.QualityProgressBar m_pbPasswordQuality;
		private System.Windows.Forms.Label m_lblEstimatedQuality;
		private System.Windows.Forms.Label m_lblQualityInfo;
		private System.Windows.Forms.ComboBox m_cmbKeyFile;
		private System.Windows.Forms.Label m_lblWindowsAccDesc2;
		private System.Windows.Forms.PictureBox m_picAccWarning;
		private System.Windows.Forms.CheckBox m_cbExpert;
		private System.Windows.Forms.PictureBox m_picKeyFileWarning;
		private System.Windows.Forms.Label m_lblKeyFileWarning;
		private System.Windows.Forms.LinkLabel m_lnkKeyFile;
		private System.Windows.Forms.LinkLabel m_lnkUserAccount;
		private System.Windows.Forms.Label m_lblKeyFileInfo;
	}
}