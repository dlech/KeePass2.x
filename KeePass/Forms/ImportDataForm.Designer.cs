namespace KeePass.Forms
{
	partial class ImportDataForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportDataForm));
			this.m_bannerImage = new System.Windows.Forms.PictureBox();
			this.m_btnOK = new System.Windows.Forms.Button();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_grpFormat = new System.Windows.Forms.GroupBox();
			this.m_lvFormats = new System.Windows.Forms.ListView();
			this.m_grpFile = new System.Windows.Forms.GroupBox();
			this.m_lnkFileFormats = new System.Windows.Forms.LinkLabel();
			this.m_btnSelFile = new System.Windows.Forms.Button();
			this.m_tbFile = new System.Windows.Forms.TextBox();
			this.m_lblFile = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
			this.m_grpFormat.SuspendLayout();
			this.m_grpFile.SuspendLayout();
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
			// m_grpFormat
			// 
			this.m_grpFormat.Controls.Add(this.m_lvFormats);
			resources.ApplyResources(this.m_grpFormat, "m_grpFormat");
			this.m_grpFormat.Name = "m_grpFormat";
			this.m_grpFormat.TabStop = false;
			// 
			// m_lvFormats
			// 
			this.m_lvFormats.FullRowSelect = true;
			this.m_lvFormats.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.m_lvFormats.HideSelection = false;
			resources.ApplyResources(this.m_lvFormats, "m_lvFormats");
			this.m_lvFormats.MultiSelect = false;
			this.m_lvFormats.Name = "m_lvFormats";
			this.m_lvFormats.UseCompatibleStateImageBehavior = false;
			this.m_lvFormats.View = System.Windows.Forms.View.Details;
			this.m_lvFormats.SelectedIndexChanged += new System.EventHandler(this.OnFormatsSelectedIndexChanged);
			// 
			// m_grpFile
			// 
			this.m_grpFile.Controls.Add(this.m_lnkFileFormats);
			this.m_grpFile.Controls.Add(this.m_btnSelFile);
			this.m_grpFile.Controls.Add(this.m_tbFile);
			this.m_grpFile.Controls.Add(this.m_lblFile);
			resources.ApplyResources(this.m_grpFile, "m_grpFile");
			this.m_grpFile.Name = "m_grpFile";
			this.m_grpFile.TabStop = false;
			// 
			// m_lnkFileFormats
			// 
			resources.ApplyResources(this.m_lnkFileFormats, "m_lnkFileFormats");
			this.m_lnkFileFormats.Name = "m_lnkFileFormats";
			this.m_lnkFileFormats.TabStop = true;
			this.m_lnkFileFormats.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnLinkFileFormats);
			// 
			// m_btnSelFile
			// 
			this.m_btnSelFile.Image = global::KeePass.Properties.Resources.B16x16_Folder_Yellow_Open;
			resources.ApplyResources(this.m_btnSelFile, "m_btnSelFile");
			this.m_btnSelFile.Name = "m_btnSelFile";
			this.m_btnSelFile.UseVisualStyleBackColor = true;
			this.m_btnSelFile.Click += new System.EventHandler(this.OnBtnSelFile);
			// 
			// m_tbFile
			// 
			resources.ApplyResources(this.m_tbFile, "m_tbFile");
			this.m_tbFile.Name = "m_tbFile";
			// 
			// m_lblFile
			// 
			resources.ApplyResources(this.m_lblFile, "m_lblFile");
			this.m_lblFile.Name = "m_lblFile";
			// 
			// ImportDataForm
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.m_grpFile);
			this.Controls.Add(this.m_grpFormat);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOK);
			this.Controls.Add(this.m_bannerImage);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ImportDataForm";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Load += new System.EventHandler(this.OnFormLoad);
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
			this.m_grpFormat.ResumeLayout(false);
			this.m_grpFile.ResumeLayout(false);
			this.m_grpFile.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PictureBox m_bannerImage;
		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.GroupBox m_grpFormat;
		private System.Windows.Forms.ListView m_lvFormats;
		private System.Windows.Forms.GroupBox m_grpFile;
		private System.Windows.Forms.Button m_btnSelFile;
		private System.Windows.Forms.TextBox m_tbFile;
		private System.Windows.Forms.Label m_lblFile;
		private System.Windows.Forms.LinkLabel m_lnkFileFormats;
	}
}