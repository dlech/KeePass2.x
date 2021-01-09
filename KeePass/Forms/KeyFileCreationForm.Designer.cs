namespace KeePass.Forms
{
	partial class KeyFileCreationForm
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
			this.m_bannerImage = new System.Windows.Forms.PictureBox();
			this.m_btnOK = new System.Windows.Forms.Button();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_rbCreate = new System.Windows.Forms.RadioButton();
			this.m_lblNewFormat = new System.Windows.Forms.Label();
			this.m_cmbNewFormat = new System.Windows.Forms.ComboBox();
			this.m_cbNewEntropy = new System.Windows.Forms.CheckBox();
			this.m_rbRecreate = new System.Windows.Forms.RadioButton();
			this.m_lblRecFormat = new System.Windows.Forms.Label();
			this.m_cmbRecFormat = new System.Windows.Forms.ComboBox();
			this.m_lblRecKeyHash = new System.Windows.Forms.Label();
			this.m_tbRecKeyHash = new System.Windows.Forms.TextBox();
			this.m_lblRecKey = new System.Windows.Forms.Label();
			this.m_tbRecKey = new System.Windows.Forms.TextBox();
			this.m_lblSeparator = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
			this.SuspendLayout();
			// 
			// m_bannerImage
			// 
			this.m_bannerImage.Dock = System.Windows.Forms.DockStyle.Top;
			this.m_bannerImage.Location = new System.Drawing.Point(0, 0);
			this.m_bannerImage.Name = "m_bannerImage";
			this.m_bannerImage.Size = new System.Drawing.Size(456, 60);
			this.m_bannerImage.TabIndex = 0;
			this.m_bannerImage.TabStop = false;
			// 
			// m_btnOK
			// 
			this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_btnOK.Location = new System.Drawing.Point(288, 371);
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
			this.m_btnCancel.Location = new System.Drawing.Point(369, 371);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 1;
			this.m_btnCancel.Text = "Cancel";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			// 
			// m_rbCreate
			// 
			this.m_rbCreate.AutoSize = true;
			this.m_rbCreate.Location = new System.Drawing.Point(13, 71);
			this.m_rbCreate.Name = "m_rbCreate";
			this.m_rbCreate.Size = new System.Drawing.Size(188, 17);
			this.m_rbCreate.TabIndex = 2;
			this.m_rbCreate.TabStop = true;
			this.m_rbCreate.Text = "&Create a new key file (random key)";
			this.m_rbCreate.UseVisualStyleBackColor = true;
			// 
			// m_lblNewFormat
			// 
			this.m_lblNewFormat.AutoSize = true;
			this.m_lblNewFormat.Location = new System.Drawing.Point(29, 95);
			this.m_lblNewFormat.Name = "m_lblNewFormat";
			this.m_lblNewFormat.Size = new System.Drawing.Size(79, 13);
			this.m_lblNewFormat.TabIndex = 3;
			this.m_lblNewFormat.Text = "&Format version:";
			// 
			// m_cmbNewFormat
			// 
			this.m_cmbNewFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbNewFormat.FormattingEnabled = true;
			this.m_cmbNewFormat.Location = new System.Drawing.Point(32, 113);
			this.m_cmbNewFormat.Name = "m_cmbNewFormat";
			this.m_cmbNewFormat.Size = new System.Drawing.Size(411, 21);
			this.m_cmbNewFormat.TabIndex = 4;
			// 
			// m_cbNewEntropy
			// 
			this.m_cbNewEntropy.AutoSize = true;
			this.m_cbNewEntropy.Location = new System.Drawing.Point(32, 144);
			this.m_cbNewEntropy.Name = "m_cbNewEntropy";
			this.m_cbNewEntropy.Size = new System.Drawing.Size(296, 17);
			this.m_cbNewEntropy.TabIndex = 5;
			this.m_cbNewEntropy.Text = "&Show dialog for collecting user input as additional entropy";
			this.m_cbNewEntropy.UseVisualStyleBackColor = true;
			// 
			// m_rbRecreate
			// 
			this.m_rbRecreate.AutoSize = true;
			this.m_rbRecreate.Location = new System.Drawing.Point(13, 169);
			this.m_rbRecreate.Name = "m_rbRecreate";
			this.m_rbRecreate.Size = new System.Drawing.Size(220, 17);
			this.m_rbRecreate.TabIndex = 6;
			this.m_rbRecreate.TabStop = true;
			this.m_rbRecreate.Text = "&Recreate a key file from a printed backup";
			this.m_rbRecreate.UseVisualStyleBackColor = true;
			// 
			// m_lblRecFormat
			// 
			this.m_lblRecFormat.AutoSize = true;
			this.m_lblRecFormat.Location = new System.Drawing.Point(29, 193);
			this.m_lblRecFormat.Name = "m_lblRecFormat";
			this.m_lblRecFormat.Size = new System.Drawing.Size(172, 13);
			this.m_lblRecFormat.TabIndex = 7;
			this.m_lblRecFormat.Text = "F&ormat version (in the \'Meta\' node):";
			// 
			// m_cmbRecFormat
			// 
			this.m_cmbRecFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbRecFormat.FormattingEnabled = true;
			this.m_cmbRecFormat.Location = new System.Drawing.Point(32, 211);
			this.m_cmbRecFormat.Name = "m_cmbRecFormat";
			this.m_cmbRecFormat.Size = new System.Drawing.Size(411, 21);
			this.m_cmbRecFormat.TabIndex = 8;
			// 
			// m_lblRecKeyHash
			// 
			this.m_lblRecKeyHash.AutoSize = true;
			this.m_lblRecKeyHash.Location = new System.Drawing.Point(29, 243);
			this.m_lblRecKeyHash.Name = "m_lblRecKeyHash";
			this.m_lblRecKeyHash.Size = new System.Drawing.Size(229, 13);
			this.m_lblRecKeyHash.TabIndex = 9;
			this.m_lblRecKeyHash.Text = "Key data &hash (in the \'Hash\' attribute, optional):";
			// 
			// m_tbRecKeyHash
			// 
			this.m_tbRecKeyHash.Location = new System.Drawing.Point(32, 261);
			this.m_tbRecKeyHash.Name = "m_tbRecKeyHash";
			this.m_tbRecKeyHash.Size = new System.Drawing.Size(411, 20);
			this.m_tbRecKeyHash.TabIndex = 10;
			// 
			// m_lblRecKey
			// 
			this.m_lblRecKey.AutoSize = true;
			this.m_lblRecKey.Location = new System.Drawing.Point(29, 292);
			this.m_lblRecKey.Name = "m_lblRecKey";
			this.m_lblRecKey.Size = new System.Drawing.Size(144, 13);
			this.m_lblRecKey.TabIndex = 11;
			this.m_lblRecKey.Text = "&Key data (in the \'Data\' node):";
			// 
			// m_tbRecKey
			// 
			this.m_tbRecKey.AcceptsReturn = true;
			this.m_tbRecKey.Location = new System.Drawing.Point(32, 310);
			this.m_tbRecKey.Multiline = true;
			this.m_tbRecKey.Name = "m_tbRecKey";
			this.m_tbRecKey.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.m_tbRecKey.Size = new System.Drawing.Size(411, 37);
			this.m_tbRecKey.TabIndex = 12;
			// 
			// m_lblSeparator
			// 
			this.m_lblSeparator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.m_lblSeparator.Location = new System.Drawing.Point(0, 360);
			this.m_lblSeparator.Name = "m_lblSeparator";
			this.m_lblSeparator.Size = new System.Drawing.Size(456, 2);
			this.m_lblSeparator.TabIndex = 13;
			// 
			// KeyFileCreationForm
			// 
			this.AcceptButton = this.m_btnOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(456, 406);
			this.Controls.Add(this.m_lblSeparator);
			this.Controls.Add(this.m_tbRecKey);
			this.Controls.Add(this.m_lblRecKey);
			this.Controls.Add(this.m_tbRecKeyHash);
			this.Controls.Add(this.m_lblRecKeyHash);
			this.Controls.Add(this.m_cmbRecFormat);
			this.Controls.Add(this.m_lblRecFormat);
			this.Controls.Add(this.m_rbRecreate);
			this.Controls.Add(this.m_cbNewEntropy);
			this.Controls.Add(this.m_cmbNewFormat);
			this.Controls.Add(this.m_lblNewFormat);
			this.Controls.Add(this.m_rbCreate);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOK);
			this.Controls.Add(this.m_bannerImage);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "KeyFileCreationForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "<DYN>";
			this.Load += new System.EventHandler(this.OnFormLoad);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox m_bannerImage;
		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.RadioButton m_rbCreate;
		private System.Windows.Forms.Label m_lblNewFormat;
		private System.Windows.Forms.ComboBox m_cmbNewFormat;
		private System.Windows.Forms.CheckBox m_cbNewEntropy;
		private System.Windows.Forms.RadioButton m_rbRecreate;
		private System.Windows.Forms.Label m_lblRecFormat;
		private System.Windows.Forms.ComboBox m_cmbRecFormat;
		private System.Windows.Forms.Label m_lblRecKeyHash;
		private System.Windows.Forms.TextBox m_tbRecKeyHash;
		private System.Windows.Forms.Label m_lblRecKey;
		private System.Windows.Forms.TextBox m_tbRecKey;
		private System.Windows.Forms.Label m_lblSeparator;
	}
}