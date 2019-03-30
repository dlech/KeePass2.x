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
            this.m_bannerImage.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_bannerImage.Location = new System.Drawing.Point(0, 0);
            this.m_bannerImage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_bannerImage.Name = "m_bannerImage";
            this.m_bannerImage.Size = new System.Drawing.Size(504, 92);
            this.m_bannerImage.TabIndex = 0;
            this.m_bannerImage.TabStop = false;
            // 
            // m_btnOK
            // 
            this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_btnOK.Location = new System.Drawing.Point(280, 691);
            this.m_btnOK.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnOK.Name = "m_btnOK";
            this.m_btnOK.Size = new System.Drawing.Size(100, 35);
            this.m_btnOK.TabIndex = 0;
            this.m_btnOK.Text = "OK";
            this.m_btnOK.UseVisualStyleBackColor = true;
            this.m_btnOK.Click += new System.EventHandler(this.OnBtnOK);
            // 
            // m_btnCancel
            // 
            this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_btnCancel.Location = new System.Drawing.Point(388, 691);
            this.m_btnCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnCancel.Name = "m_btnCancel";
            this.m_btnCancel.Size = new System.Drawing.Size(100, 35);
            this.m_btnCancel.TabIndex = 1;
            this.m_btnCancel.Text = "Cancel";
            this.m_btnCancel.UseVisualStyleBackColor = true;
            this.m_btnCancel.Click += new System.EventHandler(this.OnBtnCancel);
            // 
            // m_grpFormat
            // 
            this.m_grpFormat.Controls.Add(this.m_lvFormats);
            this.m_grpFormat.Location = new System.Drawing.Point(16, 111);
            this.m_grpFormat.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpFormat.Name = "m_grpFormat";
            this.m_grpFormat.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpFormat.Size = new System.Drawing.Size(472, 458);
            this.m_grpFormat.TabIndex = 2;
            this.m_grpFormat.TabStop = false;
            this.m_grpFormat.Text = "Format";
            // 
            // m_lvFormats
            // 
            this.m_lvFormats.FullRowSelect = true;
            this.m_lvFormats.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.m_lvFormats.HideSelection = false;
            this.m_lvFormats.Location = new System.Drawing.Point(12, 29);
            this.m_lvFormats.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_lvFormats.MultiSelect = false;
            this.m_lvFormats.Name = "m_lvFormats";
            this.m_lvFormats.Size = new System.Drawing.Size(447, 410);
            this.m_lvFormats.TabIndex = 0;
            this.m_lvFormats.UseCompatibleStateImageBehavior = false;
            this.m_lvFormats.View = System.Windows.Forms.View.Details;
            this.m_lvFormats.ItemActivate += new System.EventHandler(this.OnFormatsItemActivate);
            this.m_lvFormats.SelectedIndexChanged += new System.EventHandler(this.OnFormatsSelectedIndexChanged);
            // 
            // m_grpFile
            // 
            this.m_grpFile.Controls.Add(this.m_lnkFileFormats);
            this.m_grpFile.Controls.Add(this.m_btnSelFile);
            this.m_grpFile.Controls.Add(this.m_tbFile);
            this.m_grpFile.Controls.Add(this.m_lblFile);
            this.m_grpFile.Location = new System.Drawing.Point(16, 578);
            this.m_grpFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpFile.Name = "m_grpFile";
            this.m_grpFile.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpFile.Size = new System.Drawing.Size(472, 105);
            this.m_grpFile.TabIndex = 3;
            this.m_grpFile.TabStop = false;
            this.m_grpFile.Text = "File";
            // 
            // m_lnkFileFormats
            // 
            this.m_lnkFileFormats.AutoSize = true;
            this.m_lnkFileFormats.Location = new System.Drawing.Point(147, 69);
            this.m_lnkFileFormats.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lnkFileFormats.Name = "m_lnkFileFormats";
            this.m_lnkFileFormats.Size = new System.Drawing.Size(258, 20);
            this.m_lnkFileFormats.TabIndex = 3;
            this.m_lnkFileFormats.TabStop = true;
            this.m_lnkFileFormats.Text = "Help: Configuring source applications";
            this.m_lnkFileFormats.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnLinkFileFormats);
            // 
            // m_btnSelFile
            // 
            this.m_btnSelFile.Location = new System.Drawing.Point(421, 26);
            this.m_btnSelFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnSelFile.Name = "m_btnSelFile";
            this.m_btnSelFile.Size = new System.Drawing.Size(43, 35);
            this.m_btnSelFile.TabIndex = 2;
            this.m_btnSelFile.UseVisualStyleBackColor = true;
            this.m_btnSelFile.Click += new System.EventHandler(this.OnBtnSelFile);
            // 
            // m_tbFile
            // 
            this.m_tbFile.Location = new System.Drawing.Point(151, 29);
            this.m_tbFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_tbFile.Name = "m_tbFile";
            this.m_tbFile.Size = new System.Drawing.Size(261, 27);
            this.m_tbFile.TabIndex = 1;
            this.m_tbFile.TextChanged += new System.EventHandler(this.OnImportFileTextChanged);
            // 
            // m_lblFile
            // 
            this.m_lblFile.AutoSize = true;
            this.m_lblFile.Location = new System.Drawing.Point(8, 34);
            this.m_lblFile.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblFile.Name = "m_lblFile";
            this.m_lblFile.Size = new System.Drawing.Size(29, 20);
            this.m_lblFile.TabIndex = 0;
            this.m_lblFile.Text = "<>";
            // 
            // ExchangeDataForm
            // 
            this.AcceptButton = this.m_btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.m_btnCancel;
            this.ClientSize = new System.Drawing.Size(504, 745);
            this.Controls.Add(this.m_grpFile);
            this.Controls.Add(this.m_grpFormat);
            this.Controls.Add(this.m_btnCancel);
            this.Controls.Add(this.m_btnOK);
            this.Controls.Add(this.m_bannerImage);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExchangeDataForm";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "<DYN>";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
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
		private KeePass.UI.CustomListViewEx m_lvFormats;
		private System.Windows.Forms.GroupBox m_grpFile;
		private System.Windows.Forms.Button m_btnSelFile;
		private System.Windows.Forms.TextBox m_tbFile;
		private System.Windows.Forms.Label m_lblFile;
		private System.Windows.Forms.LinkLabel m_lnkFileFormats;
	}
}