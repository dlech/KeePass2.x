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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KeyPromptForm));
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
			this.m_openKeyFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.m_cmbKeyFile = new System.Windows.Forms.ComboBox();
			this.m_timerKeyFileFiller = new System.Windows.Forms.Timer(this.components);
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
			this.SuspendLayout();
			// 
			// m_cbUserAccount
			// 
			resources.ApplyResources(this.m_cbUserAccount, "m_cbUserAccount");
			this.m_cbUserAccount.Name = "m_cbUserAccount";
			this.m_cbUserAccount.UseVisualStyleBackColor = true;
			// 
			// m_btnOpenKeyFile
			// 
			this.m_btnOpenKeyFile.Image = global::KeePass.Properties.Resources.B16x16_Folder_Blue_Open;
			resources.ApplyResources(this.m_btnOpenKeyFile, "m_btnOpenKeyFile");
			this.m_btnOpenKeyFile.Name = "m_btnOpenKeyFile";
			this.m_ttRect.SetToolTip(this.m_btnOpenKeyFile, resources.GetString("m_btnOpenKeyFile.ToolTip"));
			this.m_btnOpenKeyFile.UseVisualStyleBackColor = true;
			this.m_btnOpenKeyFile.Click += new System.EventHandler(this.OnClickKeyFileBrowse);
			// 
			// m_cbKeyFile
			// 
			resources.ApplyResources(this.m_cbKeyFile, "m_cbKeyFile");
			this.m_cbKeyFile.Name = "m_cbKeyFile";
			this.m_cbKeyFile.UseVisualStyleBackColor = true;
			this.m_cbKeyFile.CheckedChanged += new System.EventHandler(this.OnCheckedKeyFile);
			// 
			// m_tbPassword
			// 
			resources.ApplyResources(this.m_tbPassword, "m_tbPassword");
			this.m_tbPassword.Name = "m_tbPassword";
			this.m_tbPassword.UseSystemPasswordChar = true;
			// 
			// m_cbPassword
			// 
			resources.ApplyResources(this.m_cbPassword, "m_cbPassword");
			this.m_cbPassword.Name = "m_cbPassword";
			this.m_cbPassword.UseVisualStyleBackColor = true;
			this.m_cbPassword.CheckedChanged += new System.EventHandler(this.OnCheckedPassword);
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
			// m_bannerImage
			// 
			resources.ApplyResources(this.m_bannerImage, "m_bannerImage");
			this.m_bannerImage.Name = "m_bannerImage";
			this.m_bannerImage.TabStop = false;
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
			// m_ttRect
			// 
			resources.ApplyResources(this.m_ttRect, "m_ttRect");
			// 
			// m_btnHelp
			// 
			resources.ApplyResources(this.m_btnHelp, "m_btnHelp");
			this.m_btnHelp.Name = "m_btnHelp";
			this.m_btnHelp.UseVisualStyleBackColor = true;
			this.m_btnHelp.Click += new System.EventHandler(this.OnBtnHelp);
			// 
			// m_lblSeparator
			// 
			this.m_lblSeparator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			resources.ApplyResources(this.m_lblSeparator, "m_lblSeparator");
			this.m_lblSeparator.Name = "m_lblSeparator";
			// 
			// m_openKeyFileDialog
			// 
			resources.ApplyResources(this.m_openKeyFileDialog, "m_openKeyFileDialog");
			this.m_openKeyFileDialog.RestoreDirectory = true;
			this.m_openKeyFileDialog.SupportMultiDottedExtensions = true;
			// 
			// m_cmbKeyFile
			// 
			this.m_cmbKeyFile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbKeyFile.FormattingEnabled = true;
			resources.ApplyResources(this.m_cmbKeyFile, "m_cmbKeyFile");
			this.m_cmbKeyFile.Name = "m_cmbKeyFile";
			this.m_cmbKeyFile.SelectedIndexChanged += new System.EventHandler(this.OnKeyFileSelectedIndexChanged);
			// 
			// m_timerKeyFileFiller
			// 
			this.m_timerKeyFileFiller.Enabled = true;
			this.m_timerKeyFileFiller.Tick += new System.EventHandler(this.OnKeyFileFillerTimerTick);
			// 
			// KeyPromptForm
			// 
			this.AcceptButton = this.m_btnOK;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
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
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "KeyPromptForm";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Load += new System.EventHandler(this.OnFormLoad);
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
		private System.Windows.Forms.OpenFileDialog m_openKeyFileDialog;
		private System.Windows.Forms.ComboBox m_cmbKeyFile;
		private System.Windows.Forms.Timer m_timerKeyFileFiller;
	}
}