namespace KeePass.Forms
{
	partial class EditAutoTypeItemForm
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
			this.m_btnHelp = new System.Windows.Forms.Button();
			this.m_lblTargetWindow = new System.Windows.Forms.Label();
			this.m_lblTargetWindowInfo = new System.Windows.Forms.Label();
			this.m_lblKeystrokeSeq = new System.Windows.Forms.Label();
			this.m_lblKeySeqInsertInfo = new System.Windows.Forms.Label();
			this.m_lblSeparator = new System.Windows.Forms.Label();
			this.m_rbKeySeq = new System.Windows.Forms.RichTextBox();
			this.m_rtbPlaceholders = new System.Windows.Forms.RichTextBox();
			this.m_cmbWindow = new System.Windows.Forms.ComboBox();
			this.m_lblOpenHint = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
			this.SuspendLayout();
			// 
			// m_bannerImage
			// 
			this.m_bannerImage.Dock = System.Windows.Forms.DockStyle.Top;
			this.m_bannerImage.Location = new System.Drawing.Point(0, 0);
			this.m_bannerImage.Name = "m_bannerImage";
			this.m_bannerImage.Size = new System.Drawing.Size(511, 60);
			this.m_bannerImage.TabIndex = 0;
			this.m_bannerImage.TabStop = false;
			// 
			// m_btnOK
			// 
			this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_btnOK.Location = new System.Drawing.Point(343, 343);
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.Size = new System.Drawing.Size(75, 23);
			this.m_btnOK.TabIndex = 7;
			this.m_btnOK.Text = "&OK";
			this.m_btnOK.UseVisualStyleBackColor = true;
			this.m_btnOK.Click += new System.EventHandler(this.OnBtnOK);
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_btnCancel.Location = new System.Drawing.Point(424, 343);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 8;
			this.m_btnCancel.Text = "&Cancel";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			this.m_btnCancel.Click += new System.EventHandler(this.OnBtnCancel);
			// 
			// m_btnHelp
			// 
			this.m_btnHelp.Location = new System.Drawing.Point(12, 343);
			this.m_btnHelp.Name = "m_btnHelp";
			this.m_btnHelp.Size = new System.Drawing.Size(75, 23);
			this.m_btnHelp.TabIndex = 9;
			this.m_btnHelp.Text = "&Help";
			this.m_btnHelp.UseVisualStyleBackColor = true;
			this.m_btnHelp.Click += new System.EventHandler(this.OnBtnHelp);
			// 
			// m_lblTargetWindow
			// 
			this.m_lblTargetWindow.AutoSize = true;
			this.m_lblTargetWindow.Location = new System.Drawing.Point(9, 75);
			this.m_lblTargetWindow.Name = "m_lblTargetWindow";
			this.m_lblTargetWindow.Size = new System.Drawing.Size(83, 13);
			this.m_lblTargetWindow.TabIndex = 1;
			this.m_lblTargetWindow.Text = "Target Window:";
			// 
			// m_lblTargetWindowInfo
			// 
			this.m_lblTargetWindowInfo.AutoSize = true;
			this.m_lblTargetWindowInfo.Location = new System.Drawing.Point(125, 115);
			this.m_lblTargetWindowInfo.Name = "m_lblTargetWindowInfo";
			this.m_lblTargetWindowInfo.Size = new System.Drawing.Size(172, 13);
			this.m_lblTargetWindowInfo.TabIndex = 2;
			this.m_lblTargetWindowInfo.Text = "Simple placeholders are supported.";
			// 
			// m_lblKeystrokeSeq
			// 
			this.m_lblKeystrokeSeq.AutoSize = true;
			this.m_lblKeystrokeSeq.Location = new System.Drawing.Point(10, 143);
			this.m_lblKeystrokeSeq.Name = "m_lblKeystrokeSeq";
			this.m_lblKeystrokeSeq.Size = new System.Drawing.Size(109, 13);
			this.m_lblKeystrokeSeq.TabIndex = 3;
			this.m_lblKeystrokeSeq.Text = "Keystroke Sequence:";
			// 
			// m_lblKeySeqInsertInfo
			// 
			this.m_lblKeySeqInsertInfo.AutoSize = true;
			this.m_lblKeySeqInsertInfo.Location = new System.Drawing.Point(126, 163);
			this.m_lblKeySeqInsertInfo.Name = "m_lblKeySeqInsertInfo";
			this.m_lblKeySeqInsertInfo.Size = new System.Drawing.Size(94, 13);
			this.m_lblKeySeqInsertInfo.TabIndex = 5;
			this.m_lblKeySeqInsertInfo.Text = "Insert placeholder:";
			// 
			// m_lblSeparator
			// 
			this.m_lblSeparator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.m_lblSeparator.Location = new System.Drawing.Point(0, 333);
			this.m_lblSeparator.Name = "m_lblSeparator";
			this.m_lblSeparator.Size = new System.Drawing.Size(511, 2);
			this.m_lblSeparator.TabIndex = 10;
			// 
			// m_rbKeySeq
			// 
			this.m_rbKeySeq.DetectUrls = false;
			this.m_rbKeySeq.Font = new System.Drawing.Font("Courier New", 8.25F);
			this.m_rbKeySeq.HideSelection = false;
			this.m_rbKeySeq.Location = new System.Drawing.Point(129, 140);
			this.m_rbKeySeq.Multiline = false;
			this.m_rbKeySeq.Name = "m_rbKeySeq";
			this.m_rbKeySeq.Size = new System.Drawing.Size(371, 20);
			this.m_rbKeySeq.TabIndex = 4;
			this.m_rbKeySeq.Text = "";
			this.m_rbKeySeq.TextChanged += new System.EventHandler(this.OnTextChangedKeySeq);
			// 
			// m_rtbPlaceholders
			// 
			this.m_rtbPlaceholders.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.m_rtbPlaceholders.Location = new System.Drawing.Point(129, 182);
			this.m_rtbPlaceholders.Name = "m_rtbPlaceholders";
			this.m_rtbPlaceholders.ReadOnly = true;
			this.m_rtbPlaceholders.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
			this.m_rtbPlaceholders.Size = new System.Drawing.Size(371, 136);
			this.m_rtbPlaceholders.TabIndex = 6;
			this.m_rtbPlaceholders.Text = "";
			this.m_rtbPlaceholders.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.OnPlaceholdersLinkClicked);
			// 
			// m_cmbWindow
			// 
			this.m_cmbWindow.FormattingEnabled = true;
			this.m_cmbWindow.Location = new System.Drawing.Point(129, 72);
			this.m_cmbWindow.Name = "m_cmbWindow";
			this.m_cmbWindow.Size = new System.Drawing.Size(370, 21);
			this.m_cmbWindow.TabIndex = 0;
			this.m_cmbWindow.SelectedIndexChanged += new System.EventHandler(this.OnWindowSelectedIndexChanged);
			this.m_cmbWindow.TextUpdate += new System.EventHandler(this.OnWindowTextUpdate);
			// 
			// m_lblOpenHint
			// 
			this.m_lblOpenHint.AutoSize = true;
			this.m_lblOpenHint.Location = new System.Drawing.Point(125, 98);
			this.m_lblOpenHint.Name = "m_lblOpenHint";
			this.m_lblOpenHint.Size = new System.Drawing.Size(351, 13);
			this.m_lblOpenHint.TabIndex = 11;
			this.m_lblOpenHint.Text = "Click the drop-down button on the right to see currently opened windows.";
			// 
			// EditAutoTypeItemForm
			// 
			this.AcceptButton = this.m_btnOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(511, 378);
			this.Controls.Add(this.m_lblOpenHint);
			this.Controls.Add(this.m_cmbWindow);
			this.Controls.Add(this.m_rtbPlaceholders);
			this.Controls.Add(this.m_rbKeySeq);
			this.Controls.Add(this.m_lblSeparator);
			this.Controls.Add(this.m_lblKeySeqInsertInfo);
			this.Controls.Add(this.m_lblKeystrokeSeq);
			this.Controls.Add(this.m_lblTargetWindowInfo);
			this.Controls.Add(this.m_lblTargetWindow);
			this.Controls.Add(this.m_btnHelp);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOK);
			this.Controls.Add(this.m_bannerImage);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "EditAutoTypeItemForm";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Edit Auto-Type Item";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
			this.Load += new System.EventHandler(this.OnFormLoad);
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox m_bannerImage;
		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.Button m_btnHelp;
		private System.Windows.Forms.Label m_lblTargetWindow;
		private System.Windows.Forms.Label m_lblTargetWindowInfo;
		private System.Windows.Forms.Label m_lblKeystrokeSeq;
		private System.Windows.Forms.Label m_lblKeySeqInsertInfo;
		private System.Windows.Forms.Label m_lblSeparator;
		private System.Windows.Forms.RichTextBox m_rbKeySeq;
		private System.Windows.Forms.RichTextBox m_rtbPlaceholders;
		private System.Windows.Forms.ComboBox m_cmbWindow;
		private System.Windows.Forms.Label m_lblOpenHint;
	}
}