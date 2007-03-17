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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditAutoTypeItemForm));
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
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
			this.SuspendLayout();
			// 
			// m_bannerImage
			// 
			resources.ApplyResources(this.m_bannerImage, "m_bannerImage");
			this.m_bannerImage.Name = "m_bannerImage";
			this.m_bannerImage.TabStop = false;
			// 
			// m_btnOK
			// 
			this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			resources.ApplyResources(this.m_btnOK, "m_btnOK");
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.UseVisualStyleBackColor = true;
			this.m_btnOK.Click += new System.EventHandler(this.OnBtnOK);
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			resources.ApplyResources(this.m_btnCancel, "m_btnCancel");
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			this.m_btnCancel.Click += new System.EventHandler(this.OnBtnCancel);
			// 
			// m_btnHelp
			// 
			resources.ApplyResources(this.m_btnHelp, "m_btnHelp");
			this.m_btnHelp.Name = "m_btnHelp";
			this.m_btnHelp.UseVisualStyleBackColor = true;
			this.m_btnHelp.Click += new System.EventHandler(this.OnBtnHelp);
			// 
			// m_lblTargetWindow
			// 
			resources.ApplyResources(this.m_lblTargetWindow, "m_lblTargetWindow");
			this.m_lblTargetWindow.Name = "m_lblTargetWindow";
			// 
			// m_lblTargetWindowInfo
			// 
			resources.ApplyResources(this.m_lblTargetWindowInfo, "m_lblTargetWindowInfo");
			this.m_lblTargetWindowInfo.Name = "m_lblTargetWindowInfo";
			// 
			// m_lblKeystrokeSeq
			// 
			resources.ApplyResources(this.m_lblKeystrokeSeq, "m_lblKeystrokeSeq");
			this.m_lblKeystrokeSeq.Name = "m_lblKeystrokeSeq";
			// 
			// m_lblKeySeqInsertInfo
			// 
			resources.ApplyResources(this.m_lblKeySeqInsertInfo, "m_lblKeySeqInsertInfo");
			this.m_lblKeySeqInsertInfo.Name = "m_lblKeySeqInsertInfo";
			// 
			// m_lblSeparator
			// 
			this.m_lblSeparator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			resources.ApplyResources(this.m_lblSeparator, "m_lblSeparator");
			this.m_lblSeparator.Name = "m_lblSeparator";
			// 
			// m_rbKeySeq
			// 
			this.m_rbKeySeq.DetectUrls = false;
			resources.ApplyResources(this.m_rbKeySeq, "m_rbKeySeq");
			this.m_rbKeySeq.HideSelection = false;
			this.m_rbKeySeq.Name = "m_rbKeySeq";
			this.m_rbKeySeq.TextChanged += new System.EventHandler(this.OnTextChangedKeySeq);
			// 
			// m_rtbPlaceholders
			// 
			this.m_rtbPlaceholders.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			resources.ApplyResources(this.m_rtbPlaceholders, "m_rtbPlaceholders");
			this.m_rtbPlaceholders.Name = "m_rtbPlaceholders";
			this.m_rtbPlaceholders.ReadOnly = true;
			this.m_rtbPlaceholders.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.OnPlaceholdersLinkClicked);
			// 
			// m_cmbWindow
			// 
			this.m_cmbWindow.FormattingEnabled = true;
			resources.ApplyResources(this.m_cmbWindow, "m_cmbWindow");
			this.m_cmbWindow.Name = "m_cmbWindow";
			this.m_cmbWindow.SelectedIndexChanged += new System.EventHandler(this.OnWindowSelectedIndexChanged);
			this.m_cmbWindow.TextUpdate += new System.EventHandler(this.OnWindowTextUpdate);
			// 
			// EditAutoTypeItemForm
			// 
			this.AcceptButton = this.m_btnOK;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
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
	}
}