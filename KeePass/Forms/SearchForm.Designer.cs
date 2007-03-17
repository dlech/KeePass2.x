namespace KeePass.Forms
{
	partial class SearchForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SearchForm));
			this.m_bannerImage = new System.Windows.Forms.PictureBox();
			this.m_btnOK = new System.Windows.Forms.Button();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_tbSearch = new System.Windows.Forms.TextBox();
			this.m_lblSearchFor = new System.Windows.Forms.Label();
			this.m_grpSearchIn = new System.Windows.Forms.GroupBox();
			this.m_cbAllFields = new System.Windows.Forms.CheckBox();
			this.m_cbNotes = new System.Windows.Forms.CheckBox();
			this.m_cbURL = new System.Windows.Forms.CheckBox();
			this.m_cbPassword = new System.Windows.Forms.CheckBox();
			this.m_cbUserName = new System.Windows.Forms.CheckBox();
			this.m_cbTitle = new System.Windows.Forms.CheckBox();
			this.m_grpOptions = new System.Windows.Forms.GroupBox();
			this.m_cbCaseSensitive = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
			this.m_grpSearchIn.SuspendLayout();
			this.m_grpOptions.SuspendLayout();
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
			// m_tbSearch
			// 
			resources.ApplyResources(this.m_tbSearch, "m_tbSearch");
			this.m_tbSearch.Name = "m_tbSearch";
			// 
			// m_lblSearchFor
			// 
			resources.ApplyResources(this.m_lblSearchFor, "m_lblSearchFor");
			this.m_lblSearchFor.Name = "m_lblSearchFor";
			// 
			// m_grpSearchIn
			// 
			this.m_grpSearchIn.Controls.Add(this.m_cbAllFields);
			this.m_grpSearchIn.Controls.Add(this.m_cbNotes);
			this.m_grpSearchIn.Controls.Add(this.m_cbURL);
			this.m_grpSearchIn.Controls.Add(this.m_cbPassword);
			this.m_grpSearchIn.Controls.Add(this.m_cbUserName);
			this.m_grpSearchIn.Controls.Add(this.m_cbTitle);
			resources.ApplyResources(this.m_grpSearchIn, "m_grpSearchIn");
			this.m_grpSearchIn.Name = "m_grpSearchIn";
			this.m_grpSearchIn.TabStop = false;
			// 
			// m_cbAllFields
			// 
			resources.ApplyResources(this.m_cbAllFields, "m_cbAllFields");
			this.m_cbAllFields.Name = "m_cbAllFields";
			this.m_cbAllFields.UseVisualStyleBackColor = true;
			this.m_cbAllFields.CheckedChanged += new System.EventHandler(this.OnCheckedAllFields);
			// 
			// m_cbNotes
			// 
			resources.ApplyResources(this.m_cbNotes, "m_cbNotes");
			this.m_cbNotes.Name = "m_cbNotes";
			this.m_cbNotes.UseVisualStyleBackColor = true;
			// 
			// m_cbURL
			// 
			resources.ApplyResources(this.m_cbURL, "m_cbURL");
			this.m_cbURL.Name = "m_cbURL";
			this.m_cbURL.UseVisualStyleBackColor = true;
			// 
			// m_cbPassword
			// 
			resources.ApplyResources(this.m_cbPassword, "m_cbPassword");
			this.m_cbPassword.Name = "m_cbPassword";
			this.m_cbPassword.UseVisualStyleBackColor = true;
			// 
			// m_cbUserName
			// 
			resources.ApplyResources(this.m_cbUserName, "m_cbUserName");
			this.m_cbUserName.Name = "m_cbUserName";
			this.m_cbUserName.UseVisualStyleBackColor = true;
			// 
			// m_cbTitle
			// 
			resources.ApplyResources(this.m_cbTitle, "m_cbTitle");
			this.m_cbTitle.Name = "m_cbTitle";
			this.m_cbTitle.UseVisualStyleBackColor = true;
			// 
			// m_grpOptions
			// 
			this.m_grpOptions.Controls.Add(this.m_cbCaseSensitive);
			resources.ApplyResources(this.m_grpOptions, "m_grpOptions");
			this.m_grpOptions.Name = "m_grpOptions";
			this.m_grpOptions.TabStop = false;
			// 
			// m_cbCaseSensitive
			// 
			resources.ApplyResources(this.m_cbCaseSensitive, "m_cbCaseSensitive");
			this.m_cbCaseSensitive.Name = "m_cbCaseSensitive";
			this.m_cbCaseSensitive.UseVisualStyleBackColor = true;
			// 
			// SearchForm
			// 
			this.AcceptButton = this.m_btnOK;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.Controls.Add(this.m_grpOptions);
			this.Controls.Add(this.m_grpSearchIn);
			this.Controls.Add(this.m_lblSearchFor);
			this.Controls.Add(this.m_tbSearch);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOK);
			this.Controls.Add(this.m_bannerImage);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SearchForm";
			this.ShowInTaskbar = false;
			this.Load += new System.EventHandler(this.OnFormLoad);
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
			this.m_grpSearchIn.ResumeLayout(false);
			this.m_grpSearchIn.PerformLayout();
			this.m_grpOptions.ResumeLayout(false);
			this.m_grpOptions.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox m_bannerImage;
		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.TextBox m_tbSearch;
		private System.Windows.Forms.Label m_lblSearchFor;
		private System.Windows.Forms.GroupBox m_grpSearchIn;
		private System.Windows.Forms.CheckBox m_cbAllFields;
		private System.Windows.Forms.CheckBox m_cbNotes;
		private System.Windows.Forms.CheckBox m_cbURL;
		private System.Windows.Forms.CheckBox m_cbPassword;
		private System.Windows.Forms.CheckBox m_cbUserName;
		private System.Windows.Forms.CheckBox m_cbTitle;
		private System.Windows.Forms.GroupBox m_grpOptions;
		private System.Windows.Forms.CheckBox m_cbCaseSensitive;
	}
}