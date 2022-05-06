namespace KeePass.Forms
{
	partial class OtpGeneratorForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if(disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.m_btnOK = new System.Windows.Forms.Button();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_bannerImage = new System.Windows.Forms.PictureBox();
			this.m_tabMain = new System.Windows.Forms.TabControl();
			this.m_tabHotp = new System.Windows.Forms.TabPage();
			this.m_lblHotpCounterDefault = new System.Windows.Forms.Label();
			this.m_lnkHotpPlh = new System.Windows.Forms.LinkLabel();
			this.m_lblHotpUsage = new System.Windows.Forms.Label();
			this.m_lblHotpPreviewValue = new System.Windows.Forms.Label();
			this.m_lblHotpPreview = new System.Windows.Forms.Label();
			this.m_tbHotpCounter = new System.Windows.Forms.TextBox();
			this.m_lblHotpCounter = new System.Windows.Forms.Label();
			this.m_cmbHotpSecretEnc = new System.Windows.Forms.ComboBox();
			this.m_tbHotpSecret = new System.Windows.Forms.TextBox();
			this.m_lblHotpSecret = new System.Windows.Forms.Label();
			this.m_tabTotp = new System.Windows.Forms.TabPage();
			this.m_lnkTotpPlh = new System.Windows.Forms.LinkLabel();
			this.m_lblTotpUsage = new System.Windows.Forms.Label();
			this.m_lblTotpPreviewValue = new System.Windows.Forms.Label();
			this.m_lblTotpPreview = new System.Windows.Forms.Label();
			this.m_lblTotpAlgDefault = new System.Windows.Forms.Label();
			this.m_cmbTotpAlg = new System.Windows.Forms.ComboBox();
			this.m_lblTotpAlg = new System.Windows.Forms.Label();
			this.m_lblTotpPeriodDefault = new System.Windows.Forms.Label();
			this.m_tbTotpPeriod = new System.Windows.Forms.TextBox();
			this.m_lblTotpPeriod = new System.Windows.Forms.Label();
			this.m_lblTotpLengthDefault = new System.Windows.Forms.Label();
			this.m_tbTotpLength = new System.Windows.Forms.TextBox();
			this.m_lblTotpLength = new System.Windows.Forms.Label();
			this.m_cmbTotpSecretEnc = new System.Windows.Forms.ComboBox();
			this.m_tbTotpSecret = new System.Windows.Forms.TextBox();
			this.m_lblTotpSecret = new System.Windows.Forms.Label();
			this.m_btnImportOtpAuthUri = new System.Windows.Forms.Button();
			this.m_ttRect = new System.Windows.Forms.ToolTip(this.components);
			this.m_tMain = new System.Windows.Forms.Timer(this.components);
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
			this.m_tabMain.SuspendLayout();
			this.m_tabHotp.SuspendLayout();
			this.m_tabTotp.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_btnOK
			// 
			this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_btnOK.Location = new System.Drawing.Point(234, 337);
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
			this.m_btnCancel.Location = new System.Drawing.Point(315, 337);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 1;
			this.m_btnCancel.Text = "Cancel";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			this.m_btnCancel.Click += new System.EventHandler(this.OnBtnCancel);
			// 
			// m_bannerImage
			// 
			this.m_bannerImage.Dock = System.Windows.Forms.DockStyle.Top;
			this.m_bannerImage.Location = new System.Drawing.Point(0, 0);
			this.m_bannerImage.Name = "m_bannerImage";
			this.m_bannerImage.Size = new System.Drawing.Size(402, 60);
			this.m_bannerImage.TabIndex = 2;
			this.m_bannerImage.TabStop = false;
			// 
			// m_tabMain
			// 
			this.m_tabMain.Controls.Add(this.m_tabHotp);
			this.m_tabMain.Controls.Add(this.m_tabTotp);
			this.m_tabMain.Location = new System.Drawing.Point(12, 66);
			this.m_tabMain.Name = "m_tabMain";
			this.m_tabMain.SelectedIndex = 0;
			this.m_tabMain.Size = new System.Drawing.Size(380, 265);
			this.m_tabMain.TabIndex = 2;
			// 
			// m_tabHotp
			// 
			this.m_tabHotp.Controls.Add(this.m_lblHotpCounterDefault);
			this.m_tabHotp.Controls.Add(this.m_lnkHotpPlh);
			this.m_tabHotp.Controls.Add(this.m_lblHotpUsage);
			this.m_tabHotp.Controls.Add(this.m_lblHotpPreviewValue);
			this.m_tabHotp.Controls.Add(this.m_lblHotpPreview);
			this.m_tabHotp.Controls.Add(this.m_tbHotpCounter);
			this.m_tabHotp.Controls.Add(this.m_lblHotpCounter);
			this.m_tabHotp.Controls.Add(this.m_cmbHotpSecretEnc);
			this.m_tabHotp.Controls.Add(this.m_tbHotpSecret);
			this.m_tabHotp.Controls.Add(this.m_lblHotpSecret);
			this.m_tabHotp.Location = new System.Drawing.Point(4, 22);
			this.m_tabHotp.Name = "m_tabHotp";
			this.m_tabHotp.Padding = new System.Windows.Forms.Padding(3);
			this.m_tabHotp.Size = new System.Drawing.Size(372, 239);
			this.m_tabHotp.TabIndex = 0;
			this.m_tabHotp.Text = "HMAC-Based (HOTP)";
			this.m_tabHotp.UseVisualStyleBackColor = true;
			// 
			// m_lblHotpCounterDefault
			// 
			this.m_lblHotpCounterDefault.AutoSize = true;
			this.m_lblHotpCounterDefault.Location = new System.Drawing.Point(242, 54);
			this.m_lblHotpCounterDefault.Name = "m_lblHotpCounterDefault";
			this.m_lblHotpCounterDefault.Size = new System.Drawing.Size(19, 13);
			this.m_lblHotpCounterDefault.TabIndex = 5;
			this.m_lblHotpCounterDefault.Text = "<>";
			// 
			// m_lnkHotpPlh
			// 
			this.m_lnkHotpPlh.AutoSize = true;
			this.m_lnkHotpPlh.Location = new System.Drawing.Point(6, 216);
			this.m_lnkHotpPlh.Name = "m_lnkHotpPlh";
			this.m_lnkHotpPlh.Size = new System.Drawing.Size(19, 13);
			this.m_lnkHotpPlh.TabIndex = 9;
			this.m_lnkHotpPlh.TabStop = true;
			this.m_lnkHotpPlh.Text = "<>";
			this.m_lnkHotpPlh.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnPlhLinkClicked);
			// 
			// m_lblHotpUsage
			// 
			this.m_lblHotpUsage.Location = new System.Drawing.Point(6, 184);
			this.m_lblHotpUsage.Name = "m_lblHotpUsage";
			this.m_lblHotpUsage.Size = new System.Drawing.Size(358, 28);
			this.m_lblHotpUsage.TabIndex = 8;
			this.m_lblHotpUsage.Text = "<>";
			// 
			// m_lblHotpPreviewValue
			// 
			this.m_lblHotpPreviewValue.AutoSize = true;
			this.m_lblHotpPreviewValue.Location = new System.Drawing.Point(114, 145);
			this.m_lblHotpPreviewValue.Name = "m_lblHotpPreviewValue";
			this.m_lblHotpPreviewValue.Size = new System.Drawing.Size(19, 13);
			this.m_lblHotpPreviewValue.TabIndex = 7;
			this.m_lblHotpPreviewValue.Text = "<>";
			// 
			// m_lblHotpPreview
			// 
			this.m_lblHotpPreview.AutoSize = true;
			this.m_lblHotpPreview.Location = new System.Drawing.Point(6, 145);
			this.m_lblHotpPreview.Name = "m_lblHotpPreview";
			this.m_lblHotpPreview.Size = new System.Drawing.Size(48, 13);
			this.m_lblHotpPreview.TabIndex = 6;
			this.m_lblHotpPreview.Text = "Preview:";
			// 
			// m_tbHotpCounter
			// 
			this.m_tbHotpCounter.Location = new System.Drawing.Point(117, 51);
			this.m_tbHotpCounter.Name = "m_tbHotpCounter";
			this.m_tbHotpCounter.Size = new System.Drawing.Size(119, 20);
			this.m_tbHotpCounter.TabIndex = 4;
			// 
			// m_lblHotpCounter
			// 
			this.m_lblHotpCounter.AutoSize = true;
			this.m_lblHotpCounter.Location = new System.Drawing.Point(6, 54);
			this.m_lblHotpCounter.Name = "m_lblHotpCounter";
			this.m_lblHotpCounter.Size = new System.Drawing.Size(47, 13);
			this.m_lblHotpCounter.TabIndex = 3;
			this.m_lblHotpCounter.Text = "&Counter:";
			// 
			// m_cmbHotpSecretEnc
			// 
			this.m_cmbHotpSecretEnc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbHotpSecretEnc.FormattingEnabled = true;
			this.m_cmbHotpSecretEnc.Location = new System.Drawing.Point(245, 24);
			this.m_cmbHotpSecretEnc.Name = "m_cmbHotpSecretEnc";
			this.m_cmbHotpSecretEnc.Size = new System.Drawing.Size(116, 21);
			this.m_cmbHotpSecretEnc.TabIndex = 2;
			// 
			// m_tbHotpSecret
			// 
			this.m_tbHotpSecret.Location = new System.Drawing.Point(9, 25);
			this.m_tbHotpSecret.Name = "m_tbHotpSecret";
			this.m_tbHotpSecret.Size = new System.Drawing.Size(227, 20);
			this.m_tbHotpSecret.TabIndex = 1;
			// 
			// m_lblHotpSecret
			// 
			this.m_lblHotpSecret.AutoSize = true;
			this.m_lblHotpSecret.Location = new System.Drawing.Point(6, 9);
			this.m_lblHotpSecret.Name = "m_lblHotpSecret";
			this.m_lblHotpSecret.Size = new System.Drawing.Size(76, 13);
			this.m_lblHotpSecret.TabIndex = 0;
			this.m_lblHotpSecret.Text = "&Shared secret:";
			// 
			// m_tabTotp
			// 
			this.m_tabTotp.Controls.Add(this.m_lnkTotpPlh);
			this.m_tabTotp.Controls.Add(this.m_lblTotpUsage);
			this.m_tabTotp.Controls.Add(this.m_lblTotpPreviewValue);
			this.m_tabTotp.Controls.Add(this.m_lblTotpPreview);
			this.m_tabTotp.Controls.Add(this.m_lblTotpAlgDefault);
			this.m_tabTotp.Controls.Add(this.m_cmbTotpAlg);
			this.m_tabTotp.Controls.Add(this.m_lblTotpAlg);
			this.m_tabTotp.Controls.Add(this.m_lblTotpPeriodDefault);
			this.m_tabTotp.Controls.Add(this.m_tbTotpPeriod);
			this.m_tabTotp.Controls.Add(this.m_lblTotpPeriod);
			this.m_tabTotp.Controls.Add(this.m_lblTotpLengthDefault);
			this.m_tabTotp.Controls.Add(this.m_tbTotpLength);
			this.m_tabTotp.Controls.Add(this.m_lblTotpLength);
			this.m_tabTotp.Controls.Add(this.m_cmbTotpSecretEnc);
			this.m_tabTotp.Controls.Add(this.m_tbTotpSecret);
			this.m_tabTotp.Controls.Add(this.m_lblTotpSecret);
			this.m_tabTotp.Location = new System.Drawing.Point(4, 22);
			this.m_tabTotp.Name = "m_tabTotp";
			this.m_tabTotp.Padding = new System.Windows.Forms.Padding(3);
			this.m_tabTotp.Size = new System.Drawing.Size(372, 239);
			this.m_tabTotp.TabIndex = 1;
			this.m_tabTotp.Text = "Time-Based (TOTP)";
			this.m_tabTotp.UseVisualStyleBackColor = true;
			// 
			// m_lnkTotpPlh
			// 
			this.m_lnkTotpPlh.AutoSize = true;
			this.m_lnkTotpPlh.Location = new System.Drawing.Point(6, 216);
			this.m_lnkTotpPlh.Name = "m_lnkTotpPlh";
			this.m_lnkTotpPlh.Size = new System.Drawing.Size(19, 13);
			this.m_lnkTotpPlh.TabIndex = 15;
			this.m_lnkTotpPlh.TabStop = true;
			this.m_lnkTotpPlh.Text = "<>";
			this.m_lnkTotpPlh.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnPlhLinkClicked);
			// 
			// m_lblTotpUsage
			// 
			this.m_lblTotpUsage.Location = new System.Drawing.Point(6, 184);
			this.m_lblTotpUsage.Name = "m_lblTotpUsage";
			this.m_lblTotpUsage.Size = new System.Drawing.Size(358, 28);
			this.m_lblTotpUsage.TabIndex = 14;
			this.m_lblTotpUsage.Text = "<>";
			// 
			// m_lblTotpPreviewValue
			// 
			this.m_lblTotpPreviewValue.AutoSize = true;
			this.m_lblTotpPreviewValue.Location = new System.Drawing.Point(114, 145);
			this.m_lblTotpPreviewValue.Name = "m_lblTotpPreviewValue";
			this.m_lblTotpPreviewValue.Size = new System.Drawing.Size(19, 13);
			this.m_lblTotpPreviewValue.TabIndex = 13;
			this.m_lblTotpPreviewValue.Text = "<>";
			// 
			// m_lblTotpPreview
			// 
			this.m_lblTotpPreview.AutoSize = true;
			this.m_lblTotpPreview.Location = new System.Drawing.Point(6, 145);
			this.m_lblTotpPreview.Name = "m_lblTotpPreview";
			this.m_lblTotpPreview.Size = new System.Drawing.Size(48, 13);
			this.m_lblTotpPreview.TabIndex = 12;
			this.m_lblTotpPreview.Text = "Preview:";
			// 
			// m_lblTotpAlgDefault
			// 
			this.m_lblTotpAlgDefault.AutoSize = true;
			this.m_lblTotpAlgDefault.Location = new System.Drawing.Point(242, 106);
			this.m_lblTotpAlgDefault.Name = "m_lblTotpAlgDefault";
			this.m_lblTotpAlgDefault.Size = new System.Drawing.Size(19, 13);
			this.m_lblTotpAlgDefault.TabIndex = 11;
			this.m_lblTotpAlgDefault.Text = "<>";
			// 
			// m_cmbTotpAlg
			// 
			this.m_cmbTotpAlg.FormattingEnabled = true;
			this.m_cmbTotpAlg.Location = new System.Drawing.Point(117, 103);
			this.m_cmbTotpAlg.Name = "m_cmbTotpAlg";
			this.m_cmbTotpAlg.Size = new System.Drawing.Size(119, 21);
			this.m_cmbTotpAlg.TabIndex = 10;
			// 
			// m_lblTotpAlg
			// 
			this.m_lblTotpAlg.AutoSize = true;
			this.m_lblTotpAlg.Location = new System.Drawing.Point(6, 106);
			this.m_lblTotpAlg.Name = "m_lblTotpAlg";
			this.m_lblTotpAlg.Size = new System.Drawing.Size(53, 13);
			this.m_lblTotpAlg.TabIndex = 9;
			this.m_lblTotpAlg.Text = "&Algorithm:";
			// 
			// m_lblTotpPeriodDefault
			// 
			this.m_lblTotpPeriodDefault.AutoSize = true;
			this.m_lblTotpPeriodDefault.Location = new System.Drawing.Point(242, 80);
			this.m_lblTotpPeriodDefault.Name = "m_lblTotpPeriodDefault";
			this.m_lblTotpPeriodDefault.Size = new System.Drawing.Size(19, 13);
			this.m_lblTotpPeriodDefault.TabIndex = 8;
			this.m_lblTotpPeriodDefault.Text = "<>";
			// 
			// m_tbTotpPeriod
			// 
			this.m_tbTotpPeriod.Location = new System.Drawing.Point(117, 77);
			this.m_tbTotpPeriod.Name = "m_tbTotpPeriod";
			this.m_tbTotpPeriod.Size = new System.Drawing.Size(119, 20);
			this.m_tbTotpPeriod.TabIndex = 7;
			// 
			// m_lblTotpPeriod
			// 
			this.m_lblTotpPeriod.AutoSize = true;
			this.m_lblTotpPeriod.Location = new System.Drawing.Point(6, 80);
			this.m_lblTotpPeriod.Name = "m_lblTotpPeriod";
			this.m_lblTotpPeriod.Size = new System.Drawing.Size(89, 13);
			this.m_lblTotpPeriod.TabIndex = 6;
			this.m_lblTotpPeriod.Text = "&Period (seconds):";
			// 
			// m_lblTotpLengthDefault
			// 
			this.m_lblTotpLengthDefault.AutoSize = true;
			this.m_lblTotpLengthDefault.Location = new System.Drawing.Point(242, 54);
			this.m_lblTotpLengthDefault.Name = "m_lblTotpLengthDefault";
			this.m_lblTotpLengthDefault.Size = new System.Drawing.Size(19, 13);
			this.m_lblTotpLengthDefault.TabIndex = 5;
			this.m_lblTotpLengthDefault.Text = "<>";
			// 
			// m_tbTotpLength
			// 
			this.m_tbTotpLength.Location = new System.Drawing.Point(117, 51);
			this.m_tbTotpLength.Name = "m_tbTotpLength";
			this.m_tbTotpLength.Size = new System.Drawing.Size(119, 20);
			this.m_tbTotpLength.TabIndex = 4;
			// 
			// m_lblTotpLength
			// 
			this.m_lblTotpLength.AutoSize = true;
			this.m_lblTotpLength.Location = new System.Drawing.Point(6, 54);
			this.m_lblTotpLength.Name = "m_lblTotpLength";
			this.m_lblTotpLength.Size = new System.Drawing.Size(76, 13);
			this.m_lblTotpLength.TabIndex = 3;
			this.m_lblTotpLength.Text = "&Length (digits):";
			// 
			// m_cmbTotpSecretEnc
			// 
			this.m_cmbTotpSecretEnc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbTotpSecretEnc.FormattingEnabled = true;
			this.m_cmbTotpSecretEnc.Location = new System.Drawing.Point(245, 24);
			this.m_cmbTotpSecretEnc.Name = "m_cmbTotpSecretEnc";
			this.m_cmbTotpSecretEnc.Size = new System.Drawing.Size(116, 21);
			this.m_cmbTotpSecretEnc.TabIndex = 2;
			// 
			// m_tbTotpSecret
			// 
			this.m_tbTotpSecret.Location = new System.Drawing.Point(9, 25);
			this.m_tbTotpSecret.Name = "m_tbTotpSecret";
			this.m_tbTotpSecret.Size = new System.Drawing.Size(227, 20);
			this.m_tbTotpSecret.TabIndex = 1;
			// 
			// m_lblTotpSecret
			// 
			this.m_lblTotpSecret.AutoSize = true;
			this.m_lblTotpSecret.Location = new System.Drawing.Point(6, 9);
			this.m_lblTotpSecret.Name = "m_lblTotpSecret";
			this.m_lblTotpSecret.Size = new System.Drawing.Size(76, 13);
			this.m_lblTotpSecret.TabIndex = 0;
			this.m_lblTotpSecret.Text = "&Shared secret:";
			// 
			// m_btnImportOtpAuthUri
			// 
			this.m_btnImportOtpAuthUri.Location = new System.Drawing.Point(12, 337);
			this.m_btnImportOtpAuthUri.Name = "m_btnImportOtpAuthUri";
			this.m_btnImportOtpAuthUri.Size = new System.Drawing.Size(147, 23);
			this.m_btnImportOtpAuthUri.TabIndex = 3;
			this.m_btnImportOtpAuthUri.Text = "&Import \'otpauth://\' URI...";
			this.m_btnImportOtpAuthUri.UseVisualStyleBackColor = true;
			this.m_btnImportOtpAuthUri.Click += new System.EventHandler(this.OnBtnImportOtpAuthUri);
			// 
			// m_tMain
			// 
			this.m_tMain.Interval = 500;
			this.m_tMain.Tick += new System.EventHandler(this.OnTimerMainTick);
			// 
			// OtpGeneratorForm
			// 
			this.AcceptButton = this.m_btnOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(402, 372);
			this.Controls.Add(this.m_btnImportOtpAuthUri);
			this.Controls.Add(this.m_tabMain);
			this.Controls.Add(this.m_bannerImage);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOK);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "OtpGeneratorForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "<>";
			this.Load += new System.EventHandler(this.OnFormLoad);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
			this.m_tabMain.ResumeLayout(false);
			this.m_tabHotp.ResumeLayout(false);
			this.m_tabHotp.PerformLayout();
			this.m_tabTotp.ResumeLayout(false);
			this.m_tabTotp.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.PictureBox m_bannerImage;
		private System.Windows.Forms.TabControl m_tabMain;
		private System.Windows.Forms.TabPage m_tabHotp;
		private System.Windows.Forms.TabPage m_tabTotp;
		private System.Windows.Forms.Label m_lblHotpSecret;
		private System.Windows.Forms.TextBox m_tbHotpCounter;
		private System.Windows.Forms.Label m_lblHotpCounter;
		private System.Windows.Forms.ComboBox m_cmbHotpSecretEnc;
		private System.Windows.Forms.TextBox m_tbHotpSecret;
		private System.Windows.Forms.ComboBox m_cmbTotpSecretEnc;
		private System.Windows.Forms.TextBox m_tbTotpSecret;
		private System.Windows.Forms.Label m_lblTotpSecret;
		private System.Windows.Forms.Label m_lblTotpAlg;
		private System.Windows.Forms.Label m_lblTotpPeriodDefault;
		private System.Windows.Forms.TextBox m_tbTotpPeriod;
		private System.Windows.Forms.Label m_lblTotpPeriod;
		private System.Windows.Forms.Label m_lblTotpLengthDefault;
		private System.Windows.Forms.TextBox m_tbTotpLength;
		private System.Windows.Forms.Label m_lblTotpLength;
		private System.Windows.Forms.Label m_lblHotpPreviewValue;
		private System.Windows.Forms.Label m_lblHotpPreview;
		private System.Windows.Forms.Label m_lblTotpPreviewValue;
		private System.Windows.Forms.Label m_lblTotpPreview;
		private System.Windows.Forms.Label m_lblTotpAlgDefault;
		private System.Windows.Forms.ComboBox m_cmbTotpAlg;
		private System.Windows.Forms.Label m_lblHotpUsage;
		private System.Windows.Forms.LinkLabel m_lnkHotpPlh;
		private System.Windows.Forms.LinkLabel m_lnkTotpPlh;
		private System.Windows.Forms.Label m_lblTotpUsage;
		private System.Windows.Forms.Button m_btnImportOtpAuthUri;
		private System.Windows.Forms.Label m_lblHotpCounterDefault;
		private System.Windows.Forms.ToolTip m_ttRect;
		private System.Windows.Forms.Timer m_tMain;
	}
}