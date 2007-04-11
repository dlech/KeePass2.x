namespace KeePass.Forms
{
	partial class TanWizardForm
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
			this.m_btnOK = new System.Windows.Forms.Button();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_bannerImage = new System.Windows.Forms.PictureBox();
			this.m_lblIntro = new System.Windows.Forms.Label();
			this.m_tbTANs = new System.Windows.Forms.TextBox();
			this.m_cbNumberTANs = new System.Windows.Forms.CheckBox();
			this.m_lblSeparator = new System.Windows.Forms.Label();
			this.m_numTANsIndex = new System.Windows.Forms.NumericUpDown();
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numTANsIndex)).BeginInit();
			this.SuspendLayout();
			// 
			// m_btnOK
			// 
			this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_btnOK.Location = new System.Drawing.Point(403, 423);
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.Size = new System.Drawing.Size(75, 23);
			this.m_btnOK.TabIndex = 3;
			this.m_btnOK.Text = "&OK";
			this.m_btnOK.UseVisualStyleBackColor = true;
			this.m_btnOK.Click += new System.EventHandler(this.OnBtnOK);
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_btnCancel.Location = new System.Drawing.Point(484, 423);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 4;
			this.m_btnCancel.Text = "&Cancel";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			this.m_btnCancel.Click += new System.EventHandler(this.OnBtnCancel);
			// 
			// m_bannerImage
			// 
			this.m_bannerImage.Dock = System.Windows.Forms.DockStyle.Top;
			this.m_bannerImage.Location = new System.Drawing.Point(0, 0);
			this.m_bannerImage.Name = "m_bannerImage";
			this.m_bannerImage.Size = new System.Drawing.Size(571, 60);
			this.m_bannerImage.TabIndex = 2;
			this.m_bannerImage.TabStop = false;
			// 
			// m_lblIntro
			// 
			this.m_lblIntro.Location = new System.Drawing.Point(12, 72);
			this.m_lblIntro.Name = "m_lblIntro";
			this.m_lblIntro.Size = new System.Drawing.Size(547, 29);
			this.m_lblIntro.TabIndex = 5;
			this.m_lblIntro.Text = "Paste your TANs into the edit window below. Formatting doesn\'t matter, all non-al" +
				"phanumerical characters are treated as separators. Alphanumerical strings are ad" +
				"ded as TAN entries.";
			// 
			// m_tbTANs
			// 
			this.m_tbTANs.AcceptsReturn = true;
			this.m_tbTANs.AcceptsTab = true;
			this.m_tbTANs.Location = new System.Drawing.Point(12, 104);
			this.m_tbTANs.Multiline = true;
			this.m_tbTANs.Name = "m_tbTANs";
			this.m_tbTANs.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.m_tbTANs.Size = new System.Drawing.Size(547, 271);
			this.m_tbTANs.TabIndex = 0;
			// 
			// m_cbNumberTANs
			// 
			this.m_cbNumberTANs.AutoSize = true;
			this.m_cbNumberTANs.Location = new System.Drawing.Point(12, 388);
			this.m_cbNumberTANs.Name = "m_cbNumberTANs";
			this.m_cbNumberTANs.Size = new System.Drawing.Size(275, 17);
			this.m_cbNumberTANs.TabIndex = 1;
			this.m_cbNumberTANs.Text = "Number TANs consecutively, starting from this value:";
			this.m_cbNumberTANs.UseVisualStyleBackColor = true;
			this.m_cbNumberTANs.CheckedChanged += new System.EventHandler(this.OnNumberTANsCheckedChanged);
			// 
			// m_lblSeparator
			// 
			this.m_lblSeparator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.m_lblSeparator.Location = new System.Drawing.Point(0, 416);
			this.m_lblSeparator.Name = "m_lblSeparator";
			this.m_lblSeparator.Size = new System.Drawing.Size(571, 2);
			this.m_lblSeparator.TabIndex = 6;
			// 
			// m_numTANsIndex
			// 
			this.m_numTANsIndex.Location = new System.Drawing.Point(293, 387);
			this.m_numTANsIndex.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
			this.m_numTANsIndex.Name = "m_numTANsIndex";
			this.m_numTANsIndex.Size = new System.Drawing.Size(72, 20);
			this.m_numTANsIndex.TabIndex = 2;
			// 
			// TanWizardForm
			// 
			this.AcceptButton = this.m_btnOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(571, 458);
			this.Controls.Add(this.m_numTANsIndex);
			this.Controls.Add(this.m_lblSeparator);
			this.Controls.Add(this.m_cbNumberTANs);
			this.Controls.Add(this.m_tbTANs);
			this.Controls.Add(this.m_lblIntro);
			this.Controls.Add(this.m_bannerImage);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOK);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "TanWizardForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "<>";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
			this.Load += new System.EventHandler(this.OnFormLoad);
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numTANsIndex)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.PictureBox m_bannerImage;
		private System.Windows.Forms.Label m_lblIntro;
		private System.Windows.Forms.TextBox m_tbTANs;
		private System.Windows.Forms.CheckBox m_cbNumberTANs;
		private System.Windows.Forms.Label m_lblSeparator;
		private System.Windows.Forms.NumericUpDown m_numTANsIndex;
	}
}