namespace KeePass.Forms
{
	partial class IOConnectionForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IOConnectionForm));
			this.m_bannerImage = new System.Windows.Forms.PictureBox();
			this.m_btnOK = new System.Windows.Forms.Button();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_lblUrl = new System.Windows.Forms.Label();
			this.m_tbUrl = new System.Windows.Forms.TextBox();
			this.m_lblUserName = new System.Windows.Forms.Label();
			this.m_tbUserName = new System.Windows.Forms.TextBox();
			this.m_lblPassword = new System.Windows.Forms.Label();
			this.m_tbPassword = new System.Windows.Forms.TextBox();
			this.m_lblSeparator = new System.Windows.Forms.Label();
			this.m_lblCredNote = new System.Windows.Forms.Label();
			this.m_btnHelp = new System.Windows.Forms.Button();
			this.m_ttInvalidUrl = new System.Windows.Forms.ToolTip(this.components);
			this.m_cmbCredSaveMode = new System.Windows.Forms.ComboBox();
			this.m_lblRemember = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
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
			// m_lblUrl
			// 
			resources.ApplyResources(this.m_lblUrl, "m_lblUrl");
			this.m_lblUrl.Name = "m_lblUrl";
			// 
			// m_tbUrl
			// 
			resources.ApplyResources(this.m_tbUrl, "m_tbUrl");
			this.m_tbUrl.Name = "m_tbUrl";
			// 
			// m_lblUserName
			// 
			resources.ApplyResources(this.m_lblUserName, "m_lblUserName");
			this.m_lblUserName.Name = "m_lblUserName";
			// 
			// m_tbUserName
			// 
			resources.ApplyResources(this.m_tbUserName, "m_tbUserName");
			this.m_tbUserName.Name = "m_tbUserName";
			// 
			// m_lblPassword
			// 
			resources.ApplyResources(this.m_lblPassword, "m_lblPassword");
			this.m_lblPassword.Name = "m_lblPassword";
			// 
			// m_tbPassword
			// 
			resources.ApplyResources(this.m_tbPassword, "m_tbPassword");
			this.m_tbPassword.Name = "m_tbPassword";
			this.m_tbPassword.UseSystemPasswordChar = true;
			// 
			// m_lblSeparator
			// 
			this.m_lblSeparator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			resources.ApplyResources(this.m_lblSeparator, "m_lblSeparator");
			this.m_lblSeparator.Name = "m_lblSeparator";
			// 
			// m_lblCredNote
			// 
			resources.ApplyResources(this.m_lblCredNote, "m_lblCredNote");
			this.m_lblCredNote.Name = "m_lblCredNote";
			// 
			// m_btnHelp
			// 
			resources.ApplyResources(this.m_btnHelp, "m_btnHelp");
			this.m_btnHelp.Name = "m_btnHelp";
			this.m_btnHelp.UseVisualStyleBackColor = true;
			this.m_btnHelp.Click += new System.EventHandler(this.OnBtnHelp);
			// 
			// m_ttInvalidUrl
			// 
			this.m_ttInvalidUrl.AutoPopDelay = 32000;
			this.m_ttInvalidUrl.InitialDelay = 250;
			this.m_ttInvalidUrl.IsBalloon = true;
			this.m_ttInvalidUrl.ReshowDelay = 100;
			// 
			// m_cmbCredSaveMode
			// 
			this.m_cmbCredSaveMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbCredSaveMode.FormattingEnabled = true;
			resources.ApplyResources(this.m_cmbCredSaveMode, "m_cmbCredSaveMode");
			this.m_cmbCredSaveMode.Name = "m_cmbCredSaveMode";
			// 
			// m_lblRemember
			// 
			resources.ApplyResources(this.m_lblRemember, "m_lblRemember");
			this.m_lblRemember.Name = "m_lblRemember";
			// 
			// IOConnectionForm
			// 
			this.AcceptButton = this.m_btnOK;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.Controls.Add(this.m_lblRemember);
			this.Controls.Add(this.m_cmbCredSaveMode);
			this.Controls.Add(this.m_btnHelp);
			this.Controls.Add(this.m_lblCredNote);
			this.Controls.Add(this.m_lblSeparator);
			this.Controls.Add(this.m_tbPassword);
			this.Controls.Add(this.m_lblPassword);
			this.Controls.Add(this.m_tbUserName);
			this.Controls.Add(this.m_lblUserName);
			this.Controls.Add(this.m_tbUrl);
			this.Controls.Add(this.m_lblUrl);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOK);
			this.Controls.Add(this.m_bannerImage);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "IOConnectionForm";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
			this.Load += new System.EventHandler(this.OnFormLoad);
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox m_bannerImage;
		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.Label m_lblUrl;
		private System.Windows.Forms.TextBox m_tbUrl;
		private System.Windows.Forms.Label m_lblUserName;
		private System.Windows.Forms.TextBox m_tbUserName;
		private System.Windows.Forms.Label m_lblPassword;
		private System.Windows.Forms.TextBox m_tbPassword;
		private System.Windows.Forms.Label m_lblSeparator;
		private System.Windows.Forms.Label m_lblCredNote;
		private System.Windows.Forms.Button m_btnHelp;
		private System.Windows.Forms.ToolTip m_ttInvalidUrl;
		private System.Windows.Forms.ComboBox m_cmbCredSaveMode;
		private System.Windows.Forms.Label m_lblRemember;
	}
}