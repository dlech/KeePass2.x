namespace KeePass.Forms
{
	partial class ExchangeDataForm
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
			this.m_grpFormat = new System.Windows.Forms.GroupBox();
			this.m_lvFormats = new KeePass.UI.CustomListViewEx();
			this.m_grpFiles = new System.Windows.Forms.GroupBox();
			this.m_lnkFileFormats = new System.Windows.Forms.LinkLabel();
			this.m_btnSelFile = new System.Windows.Forms.Button();
			this.m_tbFile = new System.Windows.Forms.TextBox();
			this.m_lblFiles = new System.Windows.Forms.Label();
			this.m_tabMain = new System.Windows.Forms.TabControl();
			this.m_tabGeneral = new System.Windows.Forms.TabPage();
			this.m_tabOptions = new System.Windows.Forms.TabPage();
			this.m_grpExport = new System.Windows.Forms.GroupBox();
			this.m_lblExportMasterKeySpec = new System.Windows.Forms.Label();
			this.m_cbExportMasterKeySpec = new System.Windows.Forms.CheckBox();
			this.m_lnkExportParentGroups = new System.Windows.Forms.LinkLabel();
			this.m_cbExportParentGroups = new System.Windows.Forms.CheckBox();
			this.m_grpExportPost = new System.Windows.Forms.GroupBox();
			this.m_cbExportPostShow = new System.Windows.Forms.CheckBox();
			this.m_cbExportPostOpen = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
			this.m_grpFormat.SuspendLayout();
			this.m_grpFiles.SuspendLayout();
			this.m_tabMain.SuspendLayout();
			this.m_tabGeneral.SuspendLayout();
			this.m_tabOptions.SuspendLayout();
			this.m_grpExport.SuspendLayout();
			this.m_grpExportPost.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_bannerImage
			// 
			this.m_bannerImage.Dock = System.Windows.Forms.DockStyle.Top;
			this.m_bannerImage.Location = new System.Drawing.Point(0, 0);
			this.m_bannerImage.Name = "m_bannerImage";
			this.m_bannerImage.Size = new System.Drawing.Size(404, 60);
			this.m_bannerImage.TabIndex = 0;
			this.m_bannerImage.TabStop = false;
			// 
			// m_btnOK
			// 
			this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_btnOK.Location = new System.Drawing.Point(236, 494);
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.Size = new System.Drawing.Size(75, 23);
			this.m_btnOK.TabIndex = 0;
			this.m_btnOK.Text = "OK";
			this.m_btnOK.UseVisualStyleBackColor = true;
			this.m_btnOK.Click += new System.EventHandler(this.OnBtnOK);
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_btnCancel.Location = new System.Drawing.Point(317, 494);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 1;
			this.m_btnCancel.Text = "Cancel";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			this.m_btnCancel.Click += new System.EventHandler(this.OnBtnCancel);
			// 
			// m_grpFormat
			// 
			this.m_grpFormat.Controls.Add(this.m_lvFormats);
			this.m_grpFormat.Location = new System.Drawing.Point(6, 6);
			this.m_grpFormat.Name = "m_grpFormat";
			this.m_grpFormat.Size = new System.Drawing.Size(359, 310);
			this.m_grpFormat.TabIndex = 0;
			this.m_grpFormat.TabStop = false;
			this.m_grpFormat.Text = "Format";
			// 
			// m_lvFormats
			// 
			this.m_lvFormats.FullRowSelect = true;
			this.m_lvFormats.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.m_lvFormats.HideSelection = false;
			this.m_lvFormats.Location = new System.Drawing.Point(9, 18);
			this.m_lvFormats.MultiSelect = false;
			this.m_lvFormats.Name = "m_lvFormats";
			this.m_lvFormats.Size = new System.Drawing.Size(341, 282);
			this.m_lvFormats.TabIndex = 0;
			this.m_lvFormats.UseCompatibleStateImageBehavior = false;
			this.m_lvFormats.View = System.Windows.Forms.View.Details;
			this.m_lvFormats.ItemActivate += new System.EventHandler(this.OnFormatsItemActivate);
			this.m_lvFormats.SelectedIndexChanged += new System.EventHandler(this.OnFormatsSelectedIndexChanged);
			// 
			// m_grpFiles
			// 
			this.m_grpFiles.Controls.Add(this.m_lnkFileFormats);
			this.m_grpFiles.Controls.Add(this.m_btnSelFile);
			this.m_grpFiles.Controls.Add(this.m_tbFile);
			this.m_grpFiles.Controls.Add(this.m_lblFiles);
			this.m_grpFiles.Location = new System.Drawing.Point(6, 322);
			this.m_grpFiles.Name = "m_grpFiles";
			this.m_grpFiles.Size = new System.Drawing.Size(359, 68);
			this.m_grpFiles.TabIndex = 1;
			this.m_grpFiles.TabStop = false;
			this.m_grpFiles.Text = "<>";
			// 
			// m_lnkFileFormats
			// 
			this.m_lnkFileFormats.AutoSize = true;
			this.m_lnkFileFormats.Location = new System.Drawing.Point(46, 45);
			this.m_lnkFileFormats.Name = "m_lnkFileFormats";
			this.m_lnkFileFormats.Size = new System.Drawing.Size(182, 13);
			this.m_lnkFileFormats.TabIndex = 3;
			this.m_lnkFileFormats.TabStop = true;
			this.m_lnkFileFormats.Text = "Help: Configuring source applications";
			this.m_lnkFileFormats.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnLinkFileFormats);
			// 
			// m_btnSelFile
			// 
			this.m_btnSelFile.Location = new System.Drawing.Point(319, 17);
			this.m_btnSelFile.Name = "m_btnSelFile";
			this.m_btnSelFile.Size = new System.Drawing.Size(32, 23);
			this.m_btnSelFile.TabIndex = 2;
			this.m_btnSelFile.UseVisualStyleBackColor = true;
			this.m_btnSelFile.Click += new System.EventHandler(this.OnBtnSelFile);
			// 
			// m_tbFile
			// 
			this.m_tbFile.Location = new System.Drawing.Point(49, 19);
			this.m_tbFile.Name = "m_tbFile";
			this.m_tbFile.Size = new System.Drawing.Size(264, 20);
			this.m_tbFile.TabIndex = 1;
			this.m_tbFile.TextChanged += new System.EventHandler(this.OnImportFileTextChanged);
			// 
			// m_lblFiles
			// 
			this.m_lblFiles.AutoSize = true;
			this.m_lblFiles.Location = new System.Drawing.Point(6, 22);
			this.m_lblFiles.Name = "m_lblFiles";
			this.m_lblFiles.Size = new System.Drawing.Size(37, 13);
			this.m_lblFiles.TabIndex = 0;
			this.m_lblFiles.Text = "File(s):";
			// 
			// m_tabMain
			// 
			this.m_tabMain.Controls.Add(this.m_tabGeneral);
			this.m_tabMain.Controls.Add(this.m_tabOptions);
			this.m_tabMain.Location = new System.Drawing.Point(12, 66);
			this.m_tabMain.Name = "m_tabMain";
			this.m_tabMain.SelectedIndex = 0;
			this.m_tabMain.Size = new System.Drawing.Size(381, 422);
			this.m_tabMain.TabIndex = 2;
			// 
			// m_tabGeneral
			// 
			this.m_tabGeneral.Controls.Add(this.m_grpFormat);
			this.m_tabGeneral.Controls.Add(this.m_grpFiles);
			this.m_tabGeneral.Location = new System.Drawing.Point(4, 22);
			this.m_tabGeneral.Name = "m_tabGeneral";
			this.m_tabGeneral.Padding = new System.Windows.Forms.Padding(3);
			this.m_tabGeneral.Size = new System.Drawing.Size(373, 396);
			this.m_tabGeneral.TabIndex = 0;
			this.m_tabGeneral.Text = "General";
			this.m_tabGeneral.UseVisualStyleBackColor = true;
			// 
			// m_tabOptions
			// 
			this.m_tabOptions.Controls.Add(this.m_grpExport);
			this.m_tabOptions.Controls.Add(this.m_grpExportPost);
			this.m_tabOptions.Location = new System.Drawing.Point(4, 22);
			this.m_tabOptions.Name = "m_tabOptions";
			this.m_tabOptions.Padding = new System.Windows.Forms.Padding(3);
			this.m_tabOptions.Size = new System.Drawing.Size(373, 396);
			this.m_tabOptions.TabIndex = 1;
			this.m_tabOptions.Text = "Options";
			this.m_tabOptions.UseVisualStyleBackColor = true;
			// 
			// m_grpExport
			// 
			this.m_grpExport.Controls.Add(this.m_lblExportMasterKeySpec);
			this.m_grpExport.Controls.Add(this.m_cbExportMasterKeySpec);
			this.m_grpExport.Controls.Add(this.m_lnkExportParentGroups);
			this.m_grpExport.Controls.Add(this.m_cbExportParentGroups);
			this.m_grpExport.Location = new System.Drawing.Point(6, 6);
			this.m_grpExport.Name = "m_grpExport";
			this.m_grpExport.Size = new System.Drawing.Size(359, 128);
			this.m_grpExport.TabIndex = 0;
			this.m_grpExport.TabStop = false;
			this.m_grpExport.Text = "Export";
			// 
			// m_lblExportMasterKeySpec
			// 
			this.m_lblExportMasterKeySpec.Location = new System.Drawing.Point(25, 39);
			this.m_lblExportMasterKeySpec.Name = "m_lblExportMasterKeySpec";
			this.m_lblExportMasterKeySpec.Size = new System.Drawing.Size(328, 42);
			this.m_lblExportMasterKeySpec.TabIndex = 1;
			this.m_lblExportMasterKeySpec.Text = "If activated, a dialog will be shown that allows to specify a master key for prot" +
				"ecting the exported data. If deactivated, the master key of the current database" +
				" will be used.";
			// 
			// m_cbExportMasterKeySpec
			// 
			this.m_cbExportMasterKeySpec.AutoSize = true;
			this.m_cbExportMasterKeySpec.Location = new System.Drawing.Point(9, 19);
			this.m_cbExportMasterKeySpec.Name = "m_cbExportMasterKeySpec";
			this.m_cbExportMasterKeySpec.Size = new System.Drawing.Size(149, 17);
			this.m_cbExportMasterKeySpec.TabIndex = 0;
			this.m_cbExportMasterKeySpec.Text = "&Use a different master key";
			this.m_cbExportMasterKeySpec.UseVisualStyleBackColor = true;
			// 
			// m_lnkExportParentGroups
			// 
			this.m_lnkExportParentGroups.AutoSize = true;
			this.m_lnkExportParentGroups.Location = new System.Drawing.Point(25, 105);
			this.m_lnkExportParentGroups.Name = "m_lnkExportParentGroups";
			this.m_lnkExportParentGroups.Size = new System.Drawing.Size(85, 13);
			this.m_lnkExportParentGroups.TabIndex = 3;
			this.m_lnkExportParentGroups.TabStop = true;
			this.m_lnkExportParentGroups.Text = "More information";
			this.m_lnkExportParentGroups.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnLinkExportParentGroups);
			// 
			// m_cbExportParentGroups
			// 
			this.m_cbExportParentGroups.AutoSize = true;
			this.m_cbExportParentGroups.Location = new System.Drawing.Point(9, 85);
			this.m_cbExportParentGroups.Name = "m_cbExportParentGroups";
			this.m_cbExportParentGroups.Size = new System.Drawing.Size(179, 17);
			this.m_cbExportParentGroups.TabIndex = 2;
			this.m_cbExportParentGroups.Text = "&Additionally export parent groups";
			this.m_cbExportParentGroups.UseVisualStyleBackColor = true;
			// 
			// m_grpExportPost
			// 
			this.m_grpExportPost.Controls.Add(this.m_cbExportPostShow);
			this.m_grpExportPost.Controls.Add(this.m_cbExportPostOpen);
			this.m_grpExportPost.Location = new System.Drawing.Point(6, 140);
			this.m_grpExportPost.Name = "m_grpExportPost";
			this.m_grpExportPost.Size = new System.Drawing.Size(359, 66);
			this.m_grpExportPost.TabIndex = 1;
			this.m_grpExportPost.TabStop = false;
			this.m_grpExportPost.Text = "After exporting";
			// 
			// m_cbExportPostShow
			// 
			this.m_cbExportPostShow.AutoSize = true;
			this.m_cbExportPostShow.Location = new System.Drawing.Point(9, 42);
			this.m_cbExportPostShow.Name = "m_cbExportPostShow";
			this.m_cbExportPostShow.Size = new System.Drawing.Size(201, 17);
			this.m_cbExportPostShow.TabIndex = 1;
			this.m_cbExportPostShow.Text = "&Show exported file (with file manager)";
			this.m_cbExportPostShow.UseVisualStyleBackColor = true;
			// 
			// m_cbExportPostOpen
			// 
			this.m_cbExportPostOpen.AutoSize = true;
			this.m_cbExportPostOpen.Location = new System.Drawing.Point(9, 19);
			this.m_cbExportPostOpen.Name = "m_cbExportPostOpen";
			this.m_cbExportPostOpen.Size = new System.Drawing.Size(248, 17);
			this.m_cbExportPostOpen.TabIndex = 0;
			this.m_cbExportPostOpen.Text = "&Open exported file (with associated application)";
			this.m_cbExportPostOpen.UseVisualStyleBackColor = true;
			// 
			// ExchangeDataForm
			// 
			this.AcceptButton = this.m_btnOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(404, 529);
			this.Controls.Add(this.m_tabMain);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOK);
			this.Controls.Add(this.m_bannerImage);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ExchangeDataForm";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "<DYN>";
			this.Load += new System.EventHandler(this.OnFormLoad);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
			this.m_grpFormat.ResumeLayout(false);
			this.m_grpFiles.ResumeLayout(false);
			this.m_grpFiles.PerformLayout();
			this.m_tabMain.ResumeLayout(false);
			this.m_tabGeneral.ResumeLayout(false);
			this.m_tabOptions.ResumeLayout(false);
			this.m_grpExport.ResumeLayout(false);
			this.m_grpExport.PerformLayout();
			this.m_grpExportPost.ResumeLayout(false);
			this.m_grpExportPost.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PictureBox m_bannerImage;
		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.GroupBox m_grpFormat;
		private KeePass.UI.CustomListViewEx m_lvFormats;
		private System.Windows.Forms.GroupBox m_grpFiles;
		private System.Windows.Forms.Button m_btnSelFile;
		private System.Windows.Forms.TextBox m_tbFile;
		private System.Windows.Forms.Label m_lblFiles;
		private System.Windows.Forms.LinkLabel m_lnkFileFormats;
		private System.Windows.Forms.TabControl m_tabMain;
		private System.Windows.Forms.TabPage m_tabGeneral;
		private System.Windows.Forms.TabPage m_tabOptions;
		private System.Windows.Forms.GroupBox m_grpExportPost;
		private System.Windows.Forms.CheckBox m_cbExportPostOpen;
		private System.Windows.Forms.CheckBox m_cbExportPostShow;
		private System.Windows.Forms.GroupBox m_grpExport;
		private System.Windows.Forms.CheckBox m_cbExportParentGroups;
		private System.Windows.Forms.LinkLabel m_lnkExportParentGroups;
		private System.Windows.Forms.Label m_lblExportMasterKeySpec;
		private System.Windows.Forms.CheckBox m_cbExportMasterKeySpec;
	}
}