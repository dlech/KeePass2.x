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
			this.components = new System.ComponentModel.Container();
			this.m_bannerImage = new System.Windows.Forms.PictureBox();
			this.m_btnOK = new System.Windows.Forms.Button();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_tbSearch = new System.Windows.Forms.TextBox();
			this.m_lblSearchFor = new System.Windows.Forms.Label();
			this.m_grpSearchIn = new System.Windows.Forms.GroupBox();
			this.m_cbHistory = new System.Windows.Forms.CheckBox();
			this.m_cbGroupPath = new System.Windows.Forms.CheckBox();
			this.m_cbStringName = new System.Windows.Forms.CheckBox();
			this.m_cbTags = new System.Windows.Forms.CheckBox();
			this.m_cbGroupName = new System.Windows.Forms.CheckBox();
			this.m_cbUuid = new System.Windows.Forms.CheckBox();
			this.m_cbStringsOther = new System.Windows.Forms.CheckBox();
			this.m_cbNotes = new System.Windows.Forms.CheckBox();
			this.m_cbUrl = new System.Windows.Forms.CheckBox();
			this.m_cbPassword = new System.Windows.Forms.CheckBox();
			this.m_cbUserName = new System.Windows.Forms.CheckBox();
			this.m_cbTitle = new System.Windows.Forms.CheckBox();
			this.m_grpOptions = new System.Windows.Forms.GroupBox();
			this.m_cbIgnoreGroupSettings = new System.Windows.Forms.CheckBox();
			this.m_cbDerefData = new System.Windows.Forms.CheckBox();
			this.m_cbExcludeExpired = new System.Windows.Forms.CheckBox();
			this.m_cbCaseSensitive = new System.Windows.Forms.CheckBox();
			this.m_lblProfile = new System.Windows.Forms.Label();
			this.m_cmbProfiles = new System.Windows.Forms.ComboBox();
			this.m_btnProfileAdd = new System.Windows.Forms.Button();
			this.m_btnProfileDelete = new System.Windows.Forms.Button();
			this.m_lblSep = new System.Windows.Forms.Label();
			this.m_rbModeSimple = new System.Windows.Forms.RadioButton();
			this.m_rbModeRegular = new System.Windows.Forms.RadioButton();
			this.m_rbModeXPath = new System.Windows.Forms.RadioButton();
			this.m_lblMode = new System.Windows.Forms.Label();
			this.m_ttMain = new System.Windows.Forms.ToolTip(this.components);
			this.m_btnHelp = new System.Windows.Forms.Button();
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
			this.m_bannerImage.Size = new System.Drawing.Size(374, 60);
			this.m_bannerImage.TabIndex = 0;
			this.m_bannerImage.TabStop = false;
			// 
			// m_btnOK
			// 
			this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_btnOK.Location = new System.Drawing.Point(206, 468);
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.Size = new System.Drawing.Size(75, 23);
			this.m_btnOK.TabIndex = 9;
			this.m_btnOK.Text = "&Find";
			this.m_btnOK.UseVisualStyleBackColor = true;
			this.m_btnOK.Click += new System.EventHandler(this.OnBtnOK);
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_btnCancel.Location = new System.Drawing.Point(287, 468);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 10;
			this.m_btnCancel.Text = "Cancel";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			// 
			// m_tbSearch
			// 
			this.m_tbSearch.Location = new System.Drawing.Point(72, 122);
			this.m_tbSearch.Name = "m_tbSearch";
			this.m_tbSearch.Size = new System.Drawing.Size(289, 20);
			this.m_tbSearch.TabIndex = 1;
			// 
			// m_lblSearchFor
			// 
			this.m_lblSearchFor.AutoSize = true;
			this.m_lblSearchFor.Location = new System.Drawing.Point(10, 125);
			this.m_lblSearchFor.Name = "m_lblSearchFor";
			this.m_lblSearchFor.Size = new System.Drawing.Size(56, 13);
			this.m_lblSearchFor.TabIndex = 0;
			this.m_lblSearchFor.Text = "Find &what:";
			// 
			// m_grpSearchIn
			// 
			this.m_grpSearchIn.Controls.Add(this.m_cbHistory);
			this.m_grpSearchIn.Controls.Add(this.m_cbGroupPath);
			this.m_grpSearchIn.Controls.Add(this.m_cbStringName);
			this.m_grpSearchIn.Controls.Add(this.m_cbTags);
			this.m_grpSearchIn.Controls.Add(this.m_cbGroupName);
			this.m_grpSearchIn.Controls.Add(this.m_cbUuid);
			this.m_grpSearchIn.Controls.Add(this.m_cbStringsOther);
			this.m_grpSearchIn.Controls.Add(this.m_cbNotes);
			this.m_grpSearchIn.Controls.Add(this.m_cbUrl);
			this.m_grpSearchIn.Controls.Add(this.m_cbPassword);
			this.m_grpSearchIn.Controls.Add(this.m_cbUserName);
			this.m_grpSearchIn.Controls.Add(this.m_cbTitle);
			this.m_grpSearchIn.Location = new System.Drawing.Point(13, 225);
			this.m_grpSearchIn.Name = "m_grpSearchIn";
			this.m_grpSearchIn.Size = new System.Drawing.Size(348, 113);
			this.m_grpSearchIn.TabIndex = 6;
			this.m_grpSearchIn.TabStop = false;
			this.m_grpSearchIn.Text = "Search in";
			// 
			// m_cbHistory
			// 
			this.m_cbHistory.AutoSize = true;
			this.m_cbHistory.Location = new System.Drawing.Point(234, 88);
			this.m_cbHistory.Name = "m_cbHistory";
			this.m_cbHistory.Size = new System.Drawing.Size(58, 17);
			this.m_cbHistory.TabIndex = 11;
			this.m_cbHistory.Text = "&History";
			this.m_cbHistory.UseVisualStyleBackColor = true;
			// 
			// m_cbGroupPath
			// 
			this.m_cbGroupPath.AutoSize = true;
			this.m_cbGroupPath.Location = new System.Drawing.Point(10, 88);
			this.m_cbGroupPath.Name = "m_cbGroupPath";
			this.m_cbGroupPath.Size = new System.Drawing.Size(79, 17);
			this.m_cbGroupPath.TabIndex = 9;
			this.m_cbGroupPath.Text = "&Group path";
			this.m_cbGroupPath.UseVisualStyleBackColor = true;
			// 
			// m_cbStringName
			// 
			this.m_cbStringName.AutoSize = true;
			this.m_cbStringName.Location = new System.Drawing.Point(10, 65);
			this.m_cbStringName.Name = "m_cbStringName";
			this.m_cbStringName.Size = new System.Drawing.Size(82, 17);
			this.m_cbStringName.TabIndex = 6;
			this.m_cbStringName.Text = "&String name";
			this.m_cbStringName.UseVisualStyleBackColor = true;
			// 
			// m_cbTags
			// 
			this.m_cbTags.AutoSize = true;
			this.m_cbTags.Location = new System.Drawing.Point(122, 65);
			this.m_cbTags.Name = "m_cbTags";
			this.m_cbTags.Size = new System.Drawing.Size(50, 17);
			this.m_cbTags.TabIndex = 7;
			this.m_cbTags.Text = "T&ags";
			this.m_cbTags.UseVisualStyleBackColor = true;
			// 
			// m_cbGroupName
			// 
			this.m_cbGroupName.AutoSize = true;
			this.m_cbGroupName.Location = new System.Drawing.Point(122, 88);
			this.m_cbGroupName.Name = "m_cbGroupName";
			this.m_cbGroupName.Size = new System.Drawing.Size(84, 17);
			this.m_cbGroupName.TabIndex = 10;
			this.m_cbGroupName.Text = "Group na&me";
			this.m_cbGroupName.UseVisualStyleBackColor = true;
			// 
			// m_cbUuid
			// 
			this.m_cbUuid.AutoSize = true;
			this.m_cbUuid.Location = new System.Drawing.Point(234, 65);
			this.m_cbUuid.Name = "m_cbUuid";
			this.m_cbUuid.Size = new System.Drawing.Size(53, 17);
			this.m_cbUuid.TabIndex = 8;
			this.m_cbUuid.Text = "UUI&D";
			this.m_cbUuid.UseVisualStyleBackColor = true;
			// 
			// m_cbStringsOther
			// 
			this.m_cbStringsOther.AutoSize = true;
			this.m_cbStringsOther.Location = new System.Drawing.Point(234, 42);
			this.m_cbStringsOther.Name = "m_cbStringsOther";
			this.m_cbStringsOther.Size = new System.Drawing.Size(85, 17);
			this.m_cbStringsOther.TabIndex = 5;
			this.m_cbStringsOther.Text = "&Other strings";
			this.m_cbStringsOther.UseVisualStyleBackColor = true;
			// 
			// m_cbNotes
			// 
			this.m_cbNotes.AutoSize = true;
			this.m_cbNotes.Location = new System.Drawing.Point(122, 42);
			this.m_cbNotes.Name = "m_cbNotes";
			this.m_cbNotes.Size = new System.Drawing.Size(54, 17);
			this.m_cbNotes.TabIndex = 4;
			this.m_cbNotes.Text = "&Notes";
			this.m_cbNotes.UseVisualStyleBackColor = true;
			// 
			// m_cbUrl
			// 
			this.m_cbUrl.AutoSize = true;
			this.m_cbUrl.Location = new System.Drawing.Point(10, 42);
			this.m_cbUrl.Name = "m_cbUrl";
			this.m_cbUrl.Size = new System.Drawing.Size(48, 17);
			this.m_cbUrl.TabIndex = 3;
			this.m_cbUrl.Text = "UR&L";
			this.m_cbUrl.UseVisualStyleBackColor = true;
			// 
			// m_cbPassword
			// 
			this.m_cbPassword.AutoSize = true;
			this.m_cbPassword.Location = new System.Drawing.Point(234, 19);
			this.m_cbPassword.Name = "m_cbPassword";
			this.m_cbPassword.Size = new System.Drawing.Size(72, 17);
			this.m_cbPassword.TabIndex = 2;
			this.m_cbPassword.Text = "&Password";
			this.m_cbPassword.UseVisualStyleBackColor = true;
			// 
			// m_cbUserName
			// 
			this.m_cbUserName.AutoSize = true;
			this.m_cbUserName.Location = new System.Drawing.Point(122, 19);
			this.m_cbUserName.Name = "m_cbUserName";
			this.m_cbUserName.Size = new System.Drawing.Size(77, 17);
			this.m_cbUserName.TabIndex = 1;
			this.m_cbUserName.Text = "&User name";
			this.m_cbUserName.UseVisualStyleBackColor = true;
			// 
			// m_cbTitle
			// 
			this.m_cbTitle.AutoSize = true;
			this.m_cbTitle.Location = new System.Drawing.Point(10, 19);
			this.m_cbTitle.Name = "m_cbTitle";
			this.m_cbTitle.Size = new System.Drawing.Size(46, 17);
			this.m_cbTitle.TabIndex = 0;
			this.m_cbTitle.Text = "&Title";
			this.m_cbTitle.UseVisualStyleBackColor = true;
			// 
			// m_grpOptions
			// 
			this.m_grpOptions.Controls.Add(this.m_cbIgnoreGroupSettings);
			this.m_grpOptions.Controls.Add(this.m_cbDerefData);
			this.m_grpOptions.Controls.Add(this.m_cbExcludeExpired);
			this.m_grpOptions.Controls.Add(this.m_cbCaseSensitive);
			this.m_grpOptions.Location = new System.Drawing.Point(13, 344);
			this.m_grpOptions.Name = "m_grpOptions";
			this.m_grpOptions.Size = new System.Drawing.Size(348, 113);
			this.m_grpOptions.TabIndex = 7;
			this.m_grpOptions.TabStop = false;
			this.m_grpOptions.Text = "Options";
			// 
			// m_cbIgnoreGroupSettings
			// 
			this.m_cbIgnoreGroupSettings.AutoSize = true;
			this.m_cbIgnoreGroupSettings.Location = new System.Drawing.Point(10, 65);
			this.m_cbIgnoreGroupSettings.Name = "m_cbIgnoreGroupSettings";
			this.m_cbIgnoreGroupSettings.Size = new System.Drawing.Size(177, 17);
			this.m_cbIgnoreGroupSettings.TabIndex = 2;
			this.m_cbIgnoreGroupSettings.Text = "&Ignore search settings of groups";
			this.m_cbIgnoreGroupSettings.UseVisualStyleBackColor = true;
			// 
			// m_cbDerefData
			// 
			this.m_cbDerefData.AutoSize = true;
			this.m_cbDerefData.Location = new System.Drawing.Point(10, 88);
			this.m_cbDerefData.Name = "m_cbDerefData";
			this.m_cbDerefData.Size = new System.Drawing.Size(175, 17);
			this.m_cbDerefData.TabIndex = 3;
			this.m_cbDerefData.Text = "Resol&ve field references in data";
			this.m_cbDerefData.UseVisualStyleBackColor = true;
			// 
			// m_cbExcludeExpired
			// 
			this.m_cbExcludeExpired.AutoSize = true;
			this.m_cbExcludeExpired.Location = new System.Drawing.Point(10, 42);
			this.m_cbExcludeExpired.Name = "m_cbExcludeExpired";
			this.m_cbExcludeExpired.Size = new System.Drawing.Size(135, 17);
			this.m_cbExcludeExpired.TabIndex = 1;
			this.m_cbExcludeExpired.Text = "&Exclude expired entries";
			this.m_cbExcludeExpired.UseVisualStyleBackColor = true;
			// 
			// m_cbCaseSensitive
			// 
			this.m_cbCaseSensitive.AutoSize = true;
			this.m_cbCaseSensitive.Location = new System.Drawing.Point(10, 19);
			this.m_cbCaseSensitive.Name = "m_cbCaseSensitive";
			this.m_cbCaseSensitive.Size = new System.Drawing.Size(94, 17);
			this.m_cbCaseSensitive.TabIndex = 0;
			this.m_cbCaseSensitive.Text = "&Case-sensitive";
			this.m_cbCaseSensitive.UseVisualStyleBackColor = true;
			// 
			// m_lblProfile
			// 
			this.m_lblProfile.AutoSize = true;
			this.m_lblProfile.Location = new System.Drawing.Point(10, 77);
			this.m_lblProfile.Name = "m_lblProfile";
			this.m_lblProfile.Size = new System.Drawing.Size(39, 13);
			this.m_lblProfile.TabIndex = 11;
			this.m_lblProfile.Text = "P&rofile:";
			// 
			// m_cmbProfiles
			// 
			this.m_cmbProfiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbProfiles.FormattingEnabled = true;
			this.m_cmbProfiles.Location = new System.Drawing.Point(55, 73);
			this.m_cmbProfiles.MaxDropDownItems = 16;
			this.m_cmbProfiles.Name = "m_cmbProfiles";
			this.m_cmbProfiles.Size = new System.Drawing.Size(244, 21);
			this.m_cmbProfiles.TabIndex = 12;
			this.m_cmbProfiles.SelectedIndexChanged += new System.EventHandler(this.OnProfilesSelectedIndexChanged);
			// 
			// m_btnProfileAdd
			// 
			this.m_btnProfileAdd.Location = new System.Drawing.Point(306, 72);
			this.m_btnProfileAdd.Name = "m_btnProfileAdd";
			this.m_btnProfileAdd.Size = new System.Drawing.Size(25, 23);
			this.m_btnProfileAdd.TabIndex = 13;
			this.m_btnProfileAdd.UseVisualStyleBackColor = true;
			this.m_btnProfileAdd.Click += new System.EventHandler(this.OnBtnProfileAdd);
			// 
			// m_btnProfileDelete
			// 
			this.m_btnProfileDelete.Location = new System.Drawing.Point(337, 72);
			this.m_btnProfileDelete.Name = "m_btnProfileDelete";
			this.m_btnProfileDelete.Size = new System.Drawing.Size(25, 23);
			this.m_btnProfileDelete.TabIndex = 14;
			this.m_btnProfileDelete.UseVisualStyleBackColor = true;
			this.m_btnProfileDelete.Click += new System.EventHandler(this.OnBtnProfileDelete);
			// 
			// m_lblSep
			// 
			this.m_lblSep.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.m_lblSep.Location = new System.Drawing.Point(0, 107);
			this.m_lblSep.Name = "m_lblSep";
			this.m_lblSep.Size = new System.Drawing.Size(374, 2);
			this.m_lblSep.TabIndex = 15;
			// 
			// m_rbModeSimple
			// 
			this.m_rbModeSimple.AutoSize = true;
			this.m_rbModeSimple.Location = new System.Drawing.Point(72, 153);
			this.m_rbModeSimple.Name = "m_rbModeSimple";
			this.m_rbModeSimple.Size = new System.Drawing.Size(109, 17);
			this.m_rbModeSimple.TabIndex = 3;
			this.m_rbModeSimple.TabStop = true;
			this.m_rbModeSimple.Text = "Simple e&xpression";
			this.m_rbModeSimple.UseVisualStyleBackColor = true;
			// 
			// m_rbModeRegular
			// 
			this.m_rbModeRegular.AutoSize = true;
			this.m_rbModeRegular.Location = new System.Drawing.Point(72, 176);
			this.m_rbModeRegular.Name = "m_rbModeRegular";
			this.m_rbModeRegular.Size = new System.Drawing.Size(115, 17);
			this.m_rbModeRegular.TabIndex = 4;
			this.m_rbModeRegular.TabStop = true;
			this.m_rbModeRegular.Text = "Regular expression";
			this.m_rbModeRegular.UseVisualStyleBackColor = true;
			// 
			// m_rbModeXPath
			// 
			this.m_rbModeXPath.AutoSize = true;
			this.m_rbModeXPath.Location = new System.Drawing.Point(72, 199);
			this.m_rbModeXPath.Name = "m_rbModeXPath";
			this.m_rbModeXPath.Size = new System.Drawing.Size(107, 17);
			this.m_rbModeXPath.TabIndex = 5;
			this.m_rbModeXPath.TabStop = true;
			this.m_rbModeXPath.Text = "XPath expression";
			this.m_rbModeXPath.UseVisualStyleBackColor = true;
			// 
			// m_lblMode
			// 
			this.m_lblMode.AutoSize = true;
			this.m_lblMode.Location = new System.Drawing.Point(10, 155);
			this.m_lblMode.Name = "m_lblMode";
			this.m_lblMode.Size = new System.Drawing.Size(37, 13);
			this.m_lblMode.TabIndex = 2;
			this.m_lblMode.Text = "Mode:";
			// 
			// m_btnHelp
			// 
			this.m_btnHelp.Location = new System.Drawing.Point(12, 468);
			this.m_btnHelp.Name = "m_btnHelp";
			this.m_btnHelp.Size = new System.Drawing.Size(75, 23);
			this.m_btnHelp.TabIndex = 8;
			this.m_btnHelp.Text = "Help";
			this.m_btnHelp.UseVisualStyleBackColor = true;
			this.m_btnHelp.Click += new System.EventHandler(this.OnBtnHelp);
			// 
			// SearchForm
			// 
			this.AcceptButton = this.m_btnOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(374, 503);
			this.Controls.Add(this.m_btnHelp);
			this.Controls.Add(this.m_lblMode);
			this.Controls.Add(this.m_rbModeXPath);
			this.Controls.Add(this.m_rbModeRegular);
			this.Controls.Add(this.m_rbModeSimple);
			this.Controls.Add(this.m_lblSep);
			this.Controls.Add(this.m_btnProfileDelete);
			this.Controls.Add(this.m_btnProfileAdd);
			this.Controls.Add(this.m_cmbProfiles);
			this.Controls.Add(this.m_lblProfile);
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
			this.Text = "<>";
			this.Load += new System.EventHandler(this.OnFormLoad);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
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
		private System.Windows.Forms.CheckBox m_cbNotes;
		private System.Windows.Forms.CheckBox m_cbUrl;
		private System.Windows.Forms.CheckBox m_cbPassword;
		private System.Windows.Forms.CheckBox m_cbUserName;
		private System.Windows.Forms.CheckBox m_cbTitle;
		private System.Windows.Forms.GroupBox m_grpOptions;
		private System.Windows.Forms.CheckBox m_cbCaseSensitive;
		private System.Windows.Forms.CheckBox m_cbStringsOther;
		private System.Windows.Forms.CheckBox m_cbExcludeExpired;
		private System.Windows.Forms.CheckBox m_cbGroupName;
		private System.Windows.Forms.CheckBox m_cbUuid;
		private System.Windows.Forms.CheckBox m_cbTags;
		private System.Windows.Forms.CheckBox m_cbDerefData;
		private System.Windows.Forms.CheckBox m_cbStringName;
		private System.Windows.Forms.CheckBox m_cbGroupPath;
		private System.Windows.Forms.CheckBox m_cbIgnoreGroupSettings;
		private System.Windows.Forms.Label m_lblProfile;
		private System.Windows.Forms.ComboBox m_cmbProfiles;
		private System.Windows.Forms.Button m_btnProfileAdd;
		private System.Windows.Forms.Button m_btnProfileDelete;
		private System.Windows.Forms.Label m_lblSep;
		private System.Windows.Forms.RadioButton m_rbModeSimple;
		private System.Windows.Forms.RadioButton m_rbModeRegular;
		private System.Windows.Forms.RadioButton m_rbModeXPath;
		private System.Windows.Forms.Label m_lblMode;
		private System.Windows.Forms.CheckBox m_cbHistory;
		private System.Windows.Forms.ToolTip m_ttMain;
		private System.Windows.Forms.Button m_btnHelp;
	}
}