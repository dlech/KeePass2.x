namespace KeePass.Forms
{
	partial class HelpSourceForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HelpSourceForm));
			this.m_bannerImage = new System.Windows.Forms.PictureBox();
			this.m_btnOK = new System.Windows.Forms.Button();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_radioLocal = new System.Windows.Forms.RadioButton();
			this.m_radioOnline = new System.Windows.Forms.RadioButton();
			this.m_lblLocal = new System.Windows.Forms.Label();
			this.m_lblOnline = new System.Windows.Forms.Label();
			this.m_lblIntro = new System.Windows.Forms.Label();
			this.m_lblSeparator = new System.Windows.Forms.Label();
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
			// m_radioLocal
			// 
			resources.ApplyResources(this.m_radioLocal, "m_radioLocal");
			this.m_radioLocal.Name = "m_radioLocal";
			this.m_radioLocal.TabStop = true;
			this.m_radioLocal.UseVisualStyleBackColor = true;
			// 
			// m_radioOnline
			// 
			resources.ApplyResources(this.m_radioOnline, "m_radioOnline");
			this.m_radioOnline.Name = "m_radioOnline";
			this.m_radioOnline.TabStop = true;
			this.m_radioOnline.UseVisualStyleBackColor = true;
			// 
			// m_lblLocal
			// 
			resources.ApplyResources(this.m_lblLocal, "m_lblLocal");
			this.m_lblLocal.Name = "m_lblLocal";
			// 
			// m_lblOnline
			// 
			resources.ApplyResources(this.m_lblOnline, "m_lblOnline");
			this.m_lblOnline.Name = "m_lblOnline";
			// 
			// m_lblIntro
			// 
			resources.ApplyResources(this.m_lblIntro, "m_lblIntro");
			this.m_lblIntro.Name = "m_lblIntro";
			// 
			// m_lblSeparator
			// 
			this.m_lblSeparator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			resources.ApplyResources(this.m_lblSeparator, "m_lblSeparator");
			this.m_lblSeparator.Name = "m_lblSeparator";
			// 
			// HelpSourceForm
			// 
			this.AcceptButton = this.m_btnOK;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.Controls.Add(this.m_lblSeparator);
			this.Controls.Add(this.m_lblIntro);
			this.Controls.Add(this.m_lblOnline);
			this.Controls.Add(this.m_lblLocal);
			this.Controls.Add(this.m_radioOnline);
			this.Controls.Add(this.m_radioLocal);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOK);
			this.Controls.Add(this.m_bannerImage);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "HelpSourceForm";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
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
		private System.Windows.Forms.RadioButton m_radioLocal;
		private System.Windows.Forms.RadioButton m_radioOnline;
		private System.Windows.Forms.Label m_lblLocal;
		private System.Windows.Forms.Label m_lblOnline;
		private System.Windows.Forms.Label m_lblIntro;
		private System.Windows.Forms.Label m_lblSeparator;
	}
}