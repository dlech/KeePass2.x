namespace KeePass.Forms
{
	partial class ImportMethodForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportMethodForm));
			this.m_btnOK = new System.Windows.Forms.Button();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_lblIntro = new System.Windows.Forms.Label();
			this.m_bannerImage = new System.Windows.Forms.PictureBox();
			this.m_radioCreateNew = new System.Windows.Forms.RadioButton();
			this.m_lblCreateNewHint = new System.Windows.Forms.Label();
			this.m_radioKeepExisting = new System.Windows.Forms.RadioButton();
			this.m_lblExistingHint = new System.Windows.Forms.Label();
			this.m_radioOverwrite = new System.Windows.Forms.RadioButton();
			this.m_lblOverwriteHint = new System.Windows.Forms.Label();
			this.m_radioOverwriteIfNewer = new System.Windows.Forms.RadioButton();
			this.m_lblOverwriteIfNewerHint = new System.Windows.Forms.Label();
			this.m_radioSynchronize = new System.Windows.Forms.RadioButton();
			this.m_lblSynchronizeHint = new System.Windows.Forms.Label();
			this.m_lblSeparator = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
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
			// m_lblIntro
			// 
			resources.ApplyResources(this.m_lblIntro, "m_lblIntro");
			this.m_lblIntro.Name = "m_lblIntro";
			// 
			// m_bannerImage
			// 
			resources.ApplyResources(this.m_bannerImage, "m_bannerImage");
			this.m_bannerImage.Name = "m_bannerImage";
			this.m_bannerImage.TabStop = false;
			// 
			// m_radioCreateNew
			// 
			resources.ApplyResources(this.m_radioCreateNew, "m_radioCreateNew");
			this.m_radioCreateNew.Name = "m_radioCreateNew";
			this.m_radioCreateNew.TabStop = true;
			this.m_radioCreateNew.UseVisualStyleBackColor = true;
			// 
			// m_lblCreateNewHint
			// 
			resources.ApplyResources(this.m_lblCreateNewHint, "m_lblCreateNewHint");
			this.m_lblCreateNewHint.Name = "m_lblCreateNewHint";
			// 
			// m_radioKeepExisting
			// 
			resources.ApplyResources(this.m_radioKeepExisting, "m_radioKeepExisting");
			this.m_radioKeepExisting.Name = "m_radioKeepExisting";
			this.m_radioKeepExisting.TabStop = true;
			this.m_radioKeepExisting.UseVisualStyleBackColor = true;
			// 
			// m_lblExistingHint
			// 
			resources.ApplyResources(this.m_lblExistingHint, "m_lblExistingHint");
			this.m_lblExistingHint.Name = "m_lblExistingHint";
			// 
			// m_radioOverwrite
			// 
			resources.ApplyResources(this.m_radioOverwrite, "m_radioOverwrite");
			this.m_radioOverwrite.Name = "m_radioOverwrite";
			this.m_radioOverwrite.TabStop = true;
			this.m_radioOverwrite.UseVisualStyleBackColor = true;
			// 
			// m_lblOverwriteHint
			// 
			resources.ApplyResources(this.m_lblOverwriteHint, "m_lblOverwriteHint");
			this.m_lblOverwriteHint.Name = "m_lblOverwriteHint";
			// 
			// m_radioOverwriteIfNewer
			// 
			resources.ApplyResources(this.m_radioOverwriteIfNewer, "m_radioOverwriteIfNewer");
			this.m_radioOverwriteIfNewer.Name = "m_radioOverwriteIfNewer";
			this.m_radioOverwriteIfNewer.TabStop = true;
			this.m_radioOverwriteIfNewer.UseVisualStyleBackColor = true;
			// 
			// m_lblOverwriteIfNewerHint
			// 
			resources.ApplyResources(this.m_lblOverwriteIfNewerHint, "m_lblOverwriteIfNewerHint");
			this.m_lblOverwriteIfNewerHint.Name = "m_lblOverwriteIfNewerHint";
			// 
			// m_radioSynchronize
			// 
			resources.ApplyResources(this.m_radioSynchronize, "m_radioSynchronize");
			this.m_radioSynchronize.Name = "m_radioSynchronize";
			this.m_radioSynchronize.TabStop = true;
			this.m_radioSynchronize.UseVisualStyleBackColor = true;
			// 
			// m_lblSynchronizeHint
			// 
			resources.ApplyResources(this.m_lblSynchronizeHint, "m_lblSynchronizeHint");
			this.m_lblSynchronizeHint.Name = "m_lblSynchronizeHint";
			// 
			// m_lblSeparator
			// 
			this.m_lblSeparator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			resources.ApplyResources(this.m_lblSeparator, "m_lblSeparator");
			this.m_lblSeparator.Name = "m_lblSeparator";
			// 
			// ImportMethodForm
			// 
			this.AcceptButton = this.m_btnOK;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.Controls.Add(this.m_lblSeparator);
			this.Controls.Add(this.m_lblSynchronizeHint);
			this.Controls.Add(this.m_radioSynchronize);
			this.Controls.Add(this.m_lblOverwriteIfNewerHint);
			this.Controls.Add(this.m_radioOverwriteIfNewer);
			this.Controls.Add(this.m_lblOverwriteHint);
			this.Controls.Add(this.m_radioOverwrite);
			this.Controls.Add(this.m_lblExistingHint);
			this.Controls.Add(this.m_radioKeepExisting);
			this.Controls.Add(this.m_lblCreateNewHint);
			this.Controls.Add(this.m_radioCreateNew);
			this.Controls.Add(this.m_bannerImage);
			this.Controls.Add(this.m_lblIntro);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOK);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ImportMethodForm";
			this.ShowInTaskbar = false;
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
			this.Load += new System.EventHandler(this.OnFormLoad);
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.Label m_lblIntro;
		private System.Windows.Forms.PictureBox m_bannerImage;
		private System.Windows.Forms.RadioButton m_radioCreateNew;
		private System.Windows.Forms.Label m_lblCreateNewHint;
		private System.Windows.Forms.RadioButton m_radioKeepExisting;
		private System.Windows.Forms.Label m_lblExistingHint;
		private System.Windows.Forms.RadioButton m_radioOverwrite;
		private System.Windows.Forms.Label m_lblOverwriteHint;
		private System.Windows.Forms.RadioButton m_radioOverwriteIfNewer;
		private System.Windows.Forms.Label m_lblOverwriteIfNewerHint;
		private System.Windows.Forms.RadioButton m_radioSynchronize;
		private System.Windows.Forms.Label m_lblSynchronizeHint;
		private System.Windows.Forms.Label m_lblSeparator;
	}
}