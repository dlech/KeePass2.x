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
			resources.ApplyResources(this.m_lblIntro, "m_lblIntro");
			this.m_lblIntro.Name = "m_lblIntro";
			// 
			// m_lblMultiInfo
			// 
			resources.ApplyResources(this.m_lblMultiInfo, "m_lblMultiInfo");
			this.m_lblMultiInfo.Name = "m_lblMultiInfo";
			// 
			// m_cbPassword
			// 
			resources.ApplyResources(this.m_cbPassword, "m_cbPassword");
			this.m_cbPassword.Name = "m_cbPassword";
			this.m_cbPassword.UseVisualStyleBackColor = true;
			this.m_cbPassword.CheckedChanged += new System.EventHandler(this.OnCheckedPassword);
			// 
			// m_tbPassword
			// 
			resources.ApplyResources(this.m_tbPassword, "m_tbPassword");
			this.m_tbPassword.Name = "m_tbPassword";
			this.m_tbPassword.UseSystemPasswordChar = true;
			// 
			// m_lblRepeatPassword
			// 
			resources.ApplyResources(this.m_lblRepeatPassword, "m_lblRepeatPassword");
			this.m_lblRepeatPassword.Name = "m_lblRepeatPassword";
			// 
			// m_tbRepeatPassword
			// 
			resources.ApplyResources(this.m_tbRepeatPassword, "m_tbRepeatPassword");
			this.m_tbRepeatPassword.Name = "m_tbRepeatPassword";
			this.m_tbRepeatPassword.UseSystemPasswordChar = true;
			// 
			// m_cbKeyFile
			// 
			resources.ApplyResources(this.m_cbKeyFile, "m_cbKeyFile");
			this.m_cbKeyFile.Name = "m_cbKeyFile";
			this.m_cbKeyFile.UseVisualStyleBackColor = true;
			this.m_cbKeyFile.CheckedChanged += new System.EventHandler(this.OnCheckedKeyFile);
			// 
			// m_cbUserAccount
			// 
			resources.ApplyResources(this.m_cbUserAccount, "m_cbUserAccount");
			this.m_cbUserAccount.Name = "m_cbUserAccount";
			this.m_cbUserAccount.UseVisualStyleBackColor = true;
			this.m_cbUserAccount.CheckedChanged += new System.EventHandler(this.OnWinUserCheckedChanged);
			// 
			// m_lblWindowsAccDesc
			// 
			resources.ApplyResources(this.m_lblWindowsAccDesc, "m_lblWindowsAccDesc");
			this.m_lblWindowsAccDesc.Name = "m_lblWindowsAccDesc";
			// 
			// m_lblKeyFileInfo
			// 
			resources.ApplyResources(this.m_lblKeyFileInfo, "m_lblKeyFileInfo");
			this.m_lblKeyFileInfo.Name = "m_lblKeyFileInfo";
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			resources.ApplyResources(this.m_btnCancel, "m_btnCancel");
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			this.m_btnCancel.Click += new System.EventHandler(this.OnBtnCancel);
			// 
			// m_btnCreate
			// 
			this.m_btnCreate.DialogResult = System.Windows.Forms.DialogResult.OK;
			resources.ApplyResources(this.m_btnCreate, "m_btnCreate");
			this.m_btnCreate.Name = "m_btnCreate";
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
			resources.ApplyResources(this.m_btnSaveKeyFile, "m_btnSaveKeyFile");
			this.m_btnSaveKeyFile.Name = "m_btnSaveKeyFile";
			this.m_ttRect.SetToolTip(this.m_btnSaveKeyFile, resources.GetString("m_btnSaveKeyFile.ToolTip"));
			this.m_btnSaveKeyFile.UseVisualStyleBackColor = true;
			this.m_btnSaveKeyFile.Click += new System.EventHandler(this.OnClickKeyFileCreate);
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
			// m_btnOpenKeyFile
			// 
			this.m_btnOpenKeyFile.Image = global::KeePass.Properties.Resources.B16x16_Folder_Blue_Open;
			resources.ApplyResources(this.m_btnOpenKeyFile, "m_btnOpenKeyFile");
			this.m_btnOpenKeyFile.Name = "m_btnOpenKeyFile";
			this.m_ttRect.SetToolTip(this.m_btnOpenKeyFile, resources.GetString("m_btnOpenKeyFile.ToolTip"));
			this.m_btnOpenKeyFile.UseVisualStyleBackColor = true;
			this.m_btnOpenKeyFile.Click += new System.EventHandler(this.OnClickKeyFileBrowse);
			// 
			// m_btnHelp
			// 
			resources.ApplyResources(this.m_btnHelp, "m_btnHelp");
			this.m_btnHelp.Name = "m_btnHelp";
			this.m_btnHelp.UseVisualStyleBackColor = true;
			// 
			// m_lblSeparator
			// 
			this.m_lblSeparator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			resources.ApplyResources(this.m_lblSeparator, "m_lblSeparator");
			this.m_lblSeparator.Name = "m_lblSeparator";
			// 
			// m_pbPasswordQuality
			// 
			resources.ApplyResources(this.m_pbPasswordQuality, "m_pbPasswordQuality");
			this.m_pbPasswordQuality.Maximum = 100;
			this.m_pbPasswordQuality.Minimum = 0;
			this.m_pbPasswordQuality.Name = "m_pbPasswordQuality";
			this.m_pbPasswordQuality.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			this.m_pbPasswordQuality.Value = 0;
			// 
			// m_lblEstimatedQuality
			// 
			resources.ApplyResources(this.m_lblEstimatedQuality, "m_lblEstimatedQuality");
			this.m_lblEstimatedQuality.Name = "m_lblEstimatedQuality";
			// 
			// m_lblQualityBits
			// 
			resources.ApplyResources(this.m_lblQualityBits, "m_lblQualityBits");
			this.m_lblQualityBits.Name = "m_lblQualityBits";
			// 
			// m_tbKeyFile
			// 
			resources.ApplyResources(this.m_tbKeyFile, "m_tbKeyFile");
			this.m_tbKeyFile.Name = "m_tbKeyFile";
			this.m_tbKeyFile.ReadOnly = true;
			// 
			// m_bannerImage
			// 
			resources.ApplyResources(this.m_bannerImage, "m_bannerImage");
			this.m_bannerImage.Name = "m_bannerImage";
			this.m_bannerImage.TabStop = false;
			// 
			// m_saveKeyFileDialog
			// 
			this.m_saveKeyFileDialog.DefaultExt = "key";
			resources.ApplyResources(this.m_saveKeyFileDialog, "m_saveKeyFileDialog");
			this.m_saveKeyFileDialog.RestoreDirectory = true;
			this.m_saveKeyFileDialog.SupportMultiDottedExtensions = true;
			// 
			// m_openKeyFileDialog
			// 
			resources.ApplyResources(this.m_openKeyFileDialog, "m_openKeyFileDialog");
			this.m_openKeyFileDialog.RestoreDirectory = true;
			this.m_openKeyFileDialog.SupportMultiDottedExtensions = true;
			// 
			// KeyCreationForm
			// 
			this.AcceptButton = this.m_btnCreate;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
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