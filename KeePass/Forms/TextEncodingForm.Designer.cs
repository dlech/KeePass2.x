﻿namespace KeePass.Forms
{
	partial class TextEncodingForm
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
            this.m_btnOK = new System.Windows.Forms.Button();
            this.m_btnCancel = new System.Windows.Forms.Button();
            this.m_lblSelEnc = new System.Windows.Forms.Label();
            this.m_cmbEnc = new System.Windows.Forms.ComboBox();
            this.m_lblPreview = new System.Windows.Forms.Label();
            this.m_rtbPreview = new KeePass.UI.CustomRichTextBoxEx();
            this.m_lblContext = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // m_btnOK
            // 
            this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_btnOK.Location = new System.Drawing.Point(612, 19);
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
            this.m_btnCancel.Location = new System.Drawing.Point(612, 62);
            this.m_btnCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnCancel.Name = "m_btnCancel";
            this.m_btnCancel.Size = new System.Drawing.Size(100, 35);
            this.m_btnCancel.TabIndex = 1;
            this.m_btnCancel.Text = "Cancel";
            this.m_btnCancel.UseVisualStyleBackColor = true;
            this.m_btnCancel.Click += new System.EventHandler(this.OnBtnCancel);
            // 
            // m_lblSelEnc
            // 
            this.m_lblSelEnc.AutoSize = true;
            this.m_lblSelEnc.Location = new System.Drawing.Point(12, 58);
            this.m_lblSelEnc.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblSelEnc.Name = "m_lblSelEnc";
            this.m_lblSelEnc.Size = new System.Drawing.Size(215, 20);
            this.m_lblSelEnc.TabIndex = 3;
            this.m_lblSelEnc.Text = "Select the encoding of the text:";
            // 
            // m_cmbEnc
            // 
            this.m_cmbEnc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_cmbEnc.FormattingEnabled = true;
            this.m_cmbEnc.Location = new System.Drawing.Point(16, 81);
            this.m_cmbEnc.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cmbEnc.MaxDropDownItems = 12;
            this.m_cmbEnc.Name = "m_cmbEnc";
            this.m_cmbEnc.Size = new System.Drawing.Size(512, 28);
            this.m_cmbEnc.TabIndex = 4;
            this.m_cmbEnc.SelectedIndexChanged += new System.EventHandler(this.OnEncSelectedIndexChanged);
            // 
            // m_lblPreview
            // 
            this.m_lblPreview.AutoSize = true;
            this.m_lblPreview.Location = new System.Drawing.Point(12, 132);
            this.m_lblPreview.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblPreview.Name = "m_lblPreview";
            this.m_lblPreview.Size = new System.Drawing.Size(95, 20);
            this.m_lblPreview.TabIndex = 5;
            this.m_lblPreview.Text = "Text preview:";
            // 
            // m_rtbPreview
            // 
            this.m_rtbPreview.AcceptsTab = true;
            this.m_rtbPreview.DetectUrls = false;
            this.m_rtbPreview.Location = new System.Drawing.Point(16, 158);
            this.m_rtbPreview.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_rtbPreview.Name = "m_rtbPreview";
            this.m_rtbPreview.ReadOnly = true;
            this.m_rtbPreview.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedBoth;
            this.m_rtbPreview.Size = new System.Drawing.Size(695, 482);
            this.m_rtbPreview.TabIndex = 6;
            this.m_rtbPreview.Text = "";
            this.m_rtbPreview.WordWrap = false;
            // 
            // m_lblContext
            // 
            this.m_lblContext.Location = new System.Drawing.Point(12, 20);
            this.m_lblContext.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblContext.Name = "m_lblContext";
            this.m_lblContext.Size = new System.Drawing.Size(592, 26);
            this.m_lblContext.TabIndex = 2;
            this.m_lblContext.Text = "<>";
            // 
            // TextEncodingForm
            // 
            this.AcceptButton = this.m_btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.m_btnCancel;
            this.ClientSize = new System.Drawing.Size(728, 659);
            this.Controls.Add(this.m_lblContext);
            this.Controls.Add(this.m_rtbPreview);
            this.Controls.Add(this.m_lblPreview);
            this.Controls.Add(this.m_cmbEnc);
            this.Controls.Add(this.m_lblSelEnc);
            this.Controls.Add(this.m_btnCancel);
            this.Controls.Add(this.m_btnOK);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TextEncodingForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Text Encoding";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.Label m_lblSelEnc;
		private System.Windows.Forms.ComboBox m_cmbEnc;
		private System.Windows.Forms.Label m_lblPreview;
		private KeePass.UI.CustomRichTextBoxEx m_rtbPreview;
		private System.Windows.Forms.Label m_lblContext;
	}
}