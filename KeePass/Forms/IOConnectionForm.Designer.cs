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
            this.m_bannerImage = new System.Windows.Forms.PictureBox();
            this.m_btnOK = new System.Windows.Forms.Button();
            this.m_btnCancel = new System.Windows.Forms.Button();
            this.m_lblUrl = new System.Windows.Forms.Label();
            this.m_tbUrl = new System.Windows.Forms.TextBox();
            this.m_lblUserName = new System.Windows.Forms.Label();
            this.m_tbUserName = new System.Windows.Forms.TextBox();
            this.m_lblPassword = new System.Windows.Forms.Label();
            this.m_tbPassword = new System.Windows.Forms.TextBox();
            this.m_lblCredNote = new System.Windows.Forms.Label();
            this.m_btnHelp = new System.Windows.Forms.Button();
            this.m_cmbCredSaveMode = new System.Windows.Forms.ComboBox();
            this.m_lblRemember = new System.Windows.Forms.Label();
            this.m_lblUrlExamples = new System.Windows.Forms.Label();
            this.m_lblUrlHints = new System.Windows.Forms.Label();
            this.m_tabMain = new System.Windows.Forms.TabControl();
            this.m_tabConn = new System.Windows.Forms.TabPage();
            this.m_tabAdv = new System.Windows.Forms.TabPage();
            this.m_pnlAdv = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
            this.m_tabMain.SuspendLayout();
            this.m_tabConn.SuspendLayout();
            this.m_tabAdv.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_bannerImage
            // 
            this.m_bannerImage.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_bannerImage.Location = new System.Drawing.Point(0, 0);
            this.m_bannerImage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_bannerImage.Name = "m_bannerImage";
            this.m_bannerImage.Size = new System.Drawing.Size(596, 92);
            this.m_bannerImage.TabIndex = 0;
            this.m_bannerImage.TabStop = false;
            // 
            // m_btnOK
            // 
            this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_btnOK.Location = new System.Drawing.Point(372, 485);
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
            this.m_btnCancel.Location = new System.Drawing.Point(480, 485);
            this.m_btnCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnCancel.Name = "m_btnCancel";
            this.m_btnCancel.Size = new System.Drawing.Size(100, 35);
            this.m_btnCancel.TabIndex = 1;
            this.m_btnCancel.Text = "Cancel";
            this.m_btnCancel.UseVisualStyleBackColor = true;
            this.m_btnCancel.Click += new System.EventHandler(this.OnBtnCancel);
            // 
            // m_lblUrl
            // 
            this.m_lblUrl.AutoSize = true;
            this.m_lblUrl.Location = new System.Drawing.Point(8, 22);
            this.m_lblUrl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblUrl.Name = "m_lblUrl";
            this.m_lblUrl.Size = new System.Drawing.Size(38, 20);
            this.m_lblUrl.TabIndex = 0;
            this.m_lblUrl.Text = "URL:";
            // 
            // m_tbUrl
            // 
            this.m_tbUrl.Location = new System.Drawing.Point(111, 19);
            this.m_tbUrl.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_tbUrl.Name = "m_tbUrl";
            this.m_tbUrl.Size = new System.Drawing.Size(425, 27);
            this.m_tbUrl.TabIndex = 1;
            // 
            // m_lblUserName
            // 
            this.m_lblUserName.AutoSize = true;
            this.m_lblUserName.Location = new System.Drawing.Point(8, 145);
            this.m_lblUserName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblUserName.Name = "m_lblUserName";
            this.m_lblUserName.Size = new System.Drawing.Size(82, 20);
            this.m_lblUserName.TabIndex = 4;
            this.m_lblUserName.Text = "User name:";
            // 
            // m_tbUserName
            // 
            this.m_tbUserName.Location = new System.Drawing.Point(111, 140);
            this.m_tbUserName.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_tbUserName.Name = "m_tbUserName";
            this.m_tbUserName.Size = new System.Drawing.Size(160, 27);
            this.m_tbUserName.TabIndex = 5;
            // 
            // m_lblPassword
            // 
            this.m_lblPassword.AutoSize = true;
            this.m_lblPassword.Location = new System.Drawing.Point(280, 145);
            this.m_lblPassword.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblPassword.Name = "m_lblPassword";
            this.m_lblPassword.Size = new System.Drawing.Size(73, 20);
            this.m_lblPassword.TabIndex = 6;
            this.m_lblPassword.Text = "Password:";
            // 
            // m_tbPassword
            // 
            this.m_tbPassword.Location = new System.Drawing.Point(375, 140);
            this.m_tbPassword.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_tbPassword.Name = "m_tbPassword";
            this.m_tbPassword.Size = new System.Drawing.Size(161, 27);
            this.m_tbPassword.TabIndex = 7;
            this.m_tbPassword.UseSystemPasswordChar = true;
            // 
            // m_lblCredNote
            // 
            this.m_lblCredNote.Location = new System.Drawing.Point(8, 189);
            this.m_lblCredNote.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblCredNote.Name = "m_lblCredNote";
            this.m_lblCredNote.Size = new System.Drawing.Size(529, 40);
            this.m_lblCredNote.TabIndex = 8;
            this.m_lblCredNote.Text = "The credentials you enter here are used to authenticate you against the server. D" +
    "o not enter your KeePass database master password.";
            // 
            // m_btnHelp
            // 
            this.m_btnHelp.Location = new System.Drawing.Point(16, 485);
            this.m_btnHelp.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnHelp.Name = "m_btnHelp";
            this.m_btnHelp.Size = new System.Drawing.Size(100, 35);
            this.m_btnHelp.TabIndex = 3;
            this.m_btnHelp.Text = "&Help";
            this.m_btnHelp.UseVisualStyleBackColor = true;
            this.m_btnHelp.Click += new System.EventHandler(this.OnBtnHelp);
            // 
            // m_cmbCredSaveMode
            // 
            this.m_cmbCredSaveMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_cmbCredSaveMode.FormattingEnabled = true;
            this.m_cmbCredSaveMode.Location = new System.Drawing.Point(113, 248);
            this.m_cmbCredSaveMode.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cmbCredSaveMode.Name = "m_cmbCredSaveMode";
            this.m_cmbCredSaveMode.Size = new System.Drawing.Size(423, 28);
            this.m_cmbCredSaveMode.TabIndex = 10;
            // 
            // m_lblRemember
            // 
            this.m_lblRemember.AutoSize = true;
            this.m_lblRemember.Location = new System.Drawing.Point(8, 252);
            this.m_lblRemember.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblRemember.Name = "m_lblRemember";
            this.m_lblRemember.Size = new System.Drawing.Size(85, 20);
            this.m_lblRemember.TabIndex = 9;
            this.m_lblRemember.Text = "Remember:";
            // 
            // m_lblUrlExamples
            // 
            this.m_lblUrlExamples.AutoSize = true;
            this.m_lblUrlExamples.Location = new System.Drawing.Point(107, 99);
            this.m_lblUrlExamples.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblUrlExamples.Name = "m_lblUrlExamples";
            this.m_lblUrlExamples.Size = new System.Drawing.Size(392, 20);
            this.m_lblUrlExamples.TabIndex = 3;
            this.m_lblUrlExamples.Text = "Example: ftp://ftp.someserver.com/pub/MyDatabase.kdbx";
            // 
            // m_lblUrlHints
            // 
            this.m_lblUrlHints.Location = new System.Drawing.Point(107, 54);
            this.m_lblUrlHints.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblUrlHints.Name = "m_lblUrlHints";
            this.m_lblUrlHints.Size = new System.Drawing.Size(428, 45);
            this.m_lblUrlHints.TabIndex = 2;
            this.m_lblUrlHints.Text = "The complete URL must be specified, including protocol, server and full path to t" +
    "he file.";
            // 
            // m_tabMain
            // 
            this.m_tabMain.Controls.Add(this.m_tabConn);
            this.m_tabMain.Controls.Add(this.m_tabAdv);
            this.m_tabMain.Location = new System.Drawing.Point(17, 112);
            this.m_tabMain.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_tabMain.Name = "m_tabMain";
            this.m_tabMain.SelectedIndex = 0;
            this.m_tabMain.Size = new System.Drawing.Size(564, 361);
            this.m_tabMain.TabIndex = 2;
            // 
            // m_tabConn
            // 
            this.m_tabConn.Controls.Add(this.m_tbUrl);
            this.m_tabConn.Controls.Add(this.m_lblUrlHints);
            this.m_tabConn.Controls.Add(this.m_lblUrl);
            this.m_tabConn.Controls.Add(this.m_lblUrlExamples);
            this.m_tabConn.Controls.Add(this.m_lblUserName);
            this.m_tabConn.Controls.Add(this.m_lblRemember);
            this.m_tabConn.Controls.Add(this.m_tbUserName);
            this.m_tabConn.Controls.Add(this.m_cmbCredSaveMode);
            this.m_tabConn.Controls.Add(this.m_lblPassword);
            this.m_tabConn.Controls.Add(this.m_tbPassword);
            this.m_tabConn.Controls.Add(this.m_lblCredNote);
            this.m_tabConn.Location = new System.Drawing.Point(4, 29);
            this.m_tabConn.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_tabConn.Name = "m_tabConn";
            this.m_tabConn.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_tabConn.Size = new System.Drawing.Size(556, 328);
            this.m_tabConn.TabIndex = 0;
            this.m_tabConn.Text = "Connection";
            this.m_tabConn.UseVisualStyleBackColor = true;
            // 
            // m_tabAdv
            // 
            this.m_tabAdv.Controls.Add(this.m_pnlAdv);
            this.m_tabAdv.Location = new System.Drawing.Point(4, 25);
            this.m_tabAdv.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_tabAdv.Name = "m_tabAdv";
            this.m_tabAdv.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_tabAdv.Size = new System.Drawing.Size(556, 332);
            this.m_tabAdv.TabIndex = 1;
            this.m_tabAdv.Text = "Advanced";
            this.m_tabAdv.UseVisualStyleBackColor = true;
            // 
            // m_pnlAdv
            // 
            this.m_pnlAdv.AutoScroll = true;
            this.m_pnlAdv.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_pnlAdv.Location = new System.Drawing.Point(5, 9);
            this.m_pnlAdv.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_pnlAdv.Name = "m_pnlAdv";
            this.m_pnlAdv.Size = new System.Drawing.Size(539, 303);
            this.m_pnlAdv.TabIndex = 0;
            // 
            // IOConnectionForm
            // 
            this.AcceptButton = this.m_btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.m_btnCancel;
            this.ClientSize = new System.Drawing.Size(596, 539);
            this.Controls.Add(this.m_tabMain);
            this.Controls.Add(this.m_btnHelp);
            this.Controls.Add(this.m_btnCancel);
            this.Controls.Add(this.m_btnOK);
            this.Controls.Add(this.m_bannerImage);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "IOConnectionForm";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "<DYN>";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
            this.Load += new System.EventHandler(this.OnFormLoad);
            ((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
            this.m_tabMain.ResumeLayout(false);
            this.m_tabConn.ResumeLayout(false);
            this.m_tabConn.PerformLayout();
            this.m_tabAdv.ResumeLayout(false);
            this.ResumeLayout(false);

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
		private System.Windows.Forms.Label m_lblCredNote;
		private System.Windows.Forms.Button m_btnHelp;
		private System.Windows.Forms.ComboBox m_cmbCredSaveMode;
		private System.Windows.Forms.Label m_lblRemember;
		private System.Windows.Forms.Label m_lblUrlExamples;
		private System.Windows.Forms.Label m_lblUrlHints;
		private System.Windows.Forms.TabControl m_tabMain;
		private System.Windows.Forms.TabPage m_tabConn;
		private System.Windows.Forms.TabPage m_tabAdv;
		private System.Windows.Forms.Panel m_pnlAdv;
	}
}