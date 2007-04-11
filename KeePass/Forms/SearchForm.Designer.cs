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
			this.m_bannerImage.Dock = System.Windows.Forms.DockStyle.Top;
			this.m_bannerImage.Location = new System.Drawing.Point(0, 0);
			this.m_bannerImage.Name = "m_bannerImage";
			this.m_bannerImage.Size = new System.Drawing.Size(351, 60);
			this.m_bannerImage.TabIndex = 0;
			this.m_bannerImage.TabStop = false;
			// 
			// m_btnOK
			// 
			this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_btnOK.Location = new System.Drawing.Point(183, 254);
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.Size = new System.Drawing.Size(75, 23);
			this.m_btnOK.TabIndex = 4;
			this.m_btnOK.Text = "&OK";
			this.m_btnOK.UseVisualStyleBackColor = true;
			this.m_btnOK.Click += new System.EventHandler(this.OnBtnOK);
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_btnCancel.Location = new System.Drawing.Point(264, 254);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 5;
			this.m_btnCancel.Text = "&Cancel";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			this.m_btnCancel.Click += new System.EventHandler(this.OnBtnCancel);
			// 
			// m_tbSearch
			// 
			this.m_tbSearch.Location = new System.Drawing.Point(77, 69);
			this.m_tbSearch.Name = "m_tbSearch";
			this.m_tbSearch.Size = new System.Drawing.Size(262, 20);
			this.m_tbSearch.TabIndex = 0;
			// 
			// m_lblSearchFor
			// 
			this.m_lblSearchFor.AutoSize = true;
			this.m_lblSearchFor.Location = new System.Drawing.Point(12, 72);
			this.m_lblSearchFor.Name = "m_lblSearchFor";
			this.m_lblSearchFor.Size = new System.Drawing.Size(59, 13);
			this.m_lblSearchFor.TabIndex = 1;
			this.m_lblSearchFor.Text = "Search for:";
			// 
			// m_grpSearchIn
			// 
			this.m_grpSearchIn.Controls.Add(this.m_cbAllFields);
			this.m_grpSearchIn.Controls.Add(this.m_cbNotes);
			this.m_grpSearchIn.Controls.Add(this.m_cbURL);
			this.m_grpSearchIn.Controls.Add(this.m_cbPassword);
			this.m_grpSearchIn.Controls.Add(this.m_cbUserName);
			this.m_grpSearchIn.Controls.Add(this.m_cbTitle);
			this.m_grpSearchIn.Location = new System.Drawing.Point(12, 95);
			this.m_grpSearchIn.Name = "m_grpSearchIn";
			this.m_grpSearchIn.Size = new System.Drawing.Size(327, 87);
			this.m_grpSearchIn.TabIndex = 2;
			this.m_grpSearchIn.TabStop = false;
			this.m_grpSearchIn.Text = "Search in";
			// 
			// m_cbAllFields
			// 
			this.m_cbAllFields.AutoSize = true;
			this.m_cbAllFields.Location = new System.Drawing.Point(6, 19);
			this.m_cbAllFields.Name = "m_cbAllFields";
			this.m_cbAllFields.Size = new System.Drawing.Size(97, 17);
			this.m_cbAllFields.TabIndex = 0;
			this.m_cbAllFields.Text = "&All String Fields";
			this.m_cbAllFields.UseVisualStyleBackColor = true;
			this.m_cbAllFields.CheckedChanged += new System.EventHandler(this.OnCheckedAllFields);
			// 
			// m_cbNotes
			// 
			this.m_cbNotes.AutoSize = true;
			this.m_cbNotes.Location = new System.Drawing.Point(109, 65);
			this.m_cbNotes.Name = "m_cbNotes";
			this.m_cbNotes.Size = new System.Drawing.Size(54, 17);
			this.m_cbNotes.TabIndex = 5;
			this.m_cbNotes.Text = "Note&s";
			this.m_cbNotes.UseVisualStyleBackColor = true;
			// 
			// m_cbURL
			// 
			this.m_cbURL.AutoSize = true;
			this.m_cbURL.Location = new System.Drawing.Point(109, 42);
			this.m_cbURL.Name = "m_cbURL";
			this.m_cbURL.Size = new System.Drawing.Size(48, 17);
			this.m_cbURL.TabIndex = 3;
			this.m_cbURL.Text = "&URL";
			this.m_cbURL.UseVisualStyleBackColor = true;
			// 
			// m_cbPassword
			// 
			this.m_cbPassword.AutoSize = true;
			this.m_cbPassword.Location = new System.Drawing.Point(109, 19);
			this.m_cbPassword.Name = "m_cbPassword";
			this.m_cbPassword.Size = new System.Drawing.Size(72, 17);
			this.m_cbPassword.TabIndex = 1;
			this.m_cbPassword.Text = "&Password";
			this.m_cbPassword.UseVisualStyleBackColor = true;
			// 
			// m_cbUserName
			// 
			this.m_cbUserName.AutoSize = true;
			this.m_cbUserName.Location = new System.Drawing.Point(6, 65);
			this.m_cbUserName.Name = "m_cbUserName";
			this.m_cbUserName.Size = new System.Drawing.Size(79, 17);
			this.m_cbUserName.TabIndex = 4;
			this.m_cbUserName.Text = "User &Name";
			this.m_cbUserName.UseVisualStyleBackColor = true;
			// 
			// m_cbTitle
			// 
			this.m_cbTitle.AutoSize = true;
			this.m_cbTitle.Location = new System.Drawing.Point(6, 42);
			this.m_cbTitle.Name = "m_cbTitle";
			this.m_cbTitle.Size = new System.Drawing.Size(46, 17);
			this.m_cbTitle.TabIndex = 2;
			this.m_cbTitle.Text = "&Title";
			this.m_cbTitle.UseVisualStyleBackColor = true;
			// 
			// m_grpOptions
			// 
			this.m_grpOptions.Controls.Add(this.m_cbCaseSensitive);
			this.m_grpOptions.Location = new System.Drawing.Point(12, 188);
			this.m_grpOptions.Name = "m_grpOptions";
			this.m_grpOptions.Size = new System.Drawing.Size(327, 44);
			this.m_grpOptions.TabIndex = 3;
			this.m_grpOptions.TabStop = false;
			this.m_grpOptions.Text = "Options";
			// 
			// m_cbCaseSensitive
			// 
			this.m_cbCaseSensitive.AutoSize = true;
			this.m_cbCaseSensitive.Location = new System.Drawing.Point(6, 19);
			this.m_cbCaseSensitive.Name = "m_cbCaseSensitive";
			this.m_cbCaseSensitive.Size = new System.Drawing.Size(159, 17);
			this.m_cbCaseSensitive.TabIndex = 0;
			this.m_cbCaseSensitive.Text = "Case-Sensitive Comparisons";
			this.m_cbCaseSensitive.UseVisualStyleBackColor = true;
			// 
			// SearchForm
			// 
			this.AcceptButton = this.m_btnOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(351, 289);
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
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Search";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
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