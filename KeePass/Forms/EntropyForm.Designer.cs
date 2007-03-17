namespace KeePass.Forms
{
	partial class EntropyForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EntropyForm));
			this.m_btnOK = new System.Windows.Forms.Button();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_picRandom = new System.Windows.Forms.PictureBox();
			this.m_lblHint = new System.Windows.Forms.Label();
			this.m_lblGeneratedHint = new System.Windows.Forms.Label();
			this.m_pbGenerated = new KeePass.UI.QualityProgressBar();
			this.m_lblStatus = new System.Windows.Forms.Label();
			this.m_bannerImage = new System.Windows.Forms.PictureBox();
			this.m_grpMouse = new System.Windows.Forms.GroupBox();
			this.m_grpKeyboard = new System.Windows.Forms.GroupBox();
			this.m_lblKeysDesc = new System.Windows.Forms.Label();
			this.m_lblKeysIntro = new System.Windows.Forms.Label();
			this.m_tbEdit = new System.Windows.Forms.TextBox();
			((System.ComponentModel.ISupportInitialize)(this.m_picRandom)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
			this.m_grpMouse.SuspendLayout();
			this.m_grpKeyboard.SuspendLayout();
			this.SuspendLayout();
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
			// m_picRandom
			// 
			this.m_picRandom.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.m_picRandom.Image = global::KeePass.Properties.Resources.B284x231_Random;
			resources.ApplyResources(this.m_picRandom, "m_picRandom");
			this.m_picRandom.Name = "m_picRandom";
			this.m_picRandom.TabStop = false;
			this.m_picRandom.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnRandomMouseMove);
			// 
			// m_lblHint
			// 
			resources.ApplyResources(this.m_lblHint, "m_lblHint");
			this.m_lblHint.Name = "m_lblHint";
			// 
			// m_lblGeneratedHint
			// 
			resources.ApplyResources(this.m_lblGeneratedHint, "m_lblGeneratedHint");
			this.m_lblGeneratedHint.Name = "m_lblGeneratedHint";
			// 
			// m_pbGenerated
			// 
			resources.ApplyResources(this.m_pbGenerated, "m_pbGenerated");
			this.m_pbGenerated.Maximum = 100;
			this.m_pbGenerated.Minimum = 0;
			this.m_pbGenerated.Name = "m_pbGenerated";
			this.m_pbGenerated.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			this.m_pbGenerated.Value = 0;
			// 
			// m_lblStatus
			// 
			resources.ApplyResources(this.m_lblStatus, "m_lblStatus");
			this.m_lblStatus.Name = "m_lblStatus";
			// 
			// m_bannerImage
			// 
			resources.ApplyResources(this.m_bannerImage, "m_bannerImage");
			this.m_bannerImage.Name = "m_bannerImage";
			this.m_bannerImage.TabStop = false;
			// 
			// m_grpMouse
			// 
			this.m_grpMouse.Controls.Add(this.m_lblHint);
			this.m_grpMouse.Controls.Add(this.m_picRandom);
			this.m_grpMouse.Controls.Add(this.m_lblGeneratedHint);
			this.m_grpMouse.Controls.Add(this.m_pbGenerated);
			this.m_grpMouse.Controls.Add(this.m_lblStatus);
			resources.ApplyResources(this.m_grpMouse, "m_grpMouse");
			this.m_grpMouse.Name = "m_grpMouse";
			this.m_grpMouse.TabStop = false;
			// 
			// m_grpKeyboard
			// 
			this.m_grpKeyboard.Controls.Add(this.m_lblKeysDesc);
			this.m_grpKeyboard.Controls.Add(this.m_lblKeysIntro);
			this.m_grpKeyboard.Controls.Add(this.m_tbEdit);
			resources.ApplyResources(this.m_grpKeyboard, "m_grpKeyboard");
			this.m_grpKeyboard.Name = "m_grpKeyboard";
			this.m_grpKeyboard.TabStop = false;
			// 
			// m_lblKeysDesc
			// 
			resources.ApplyResources(this.m_lblKeysDesc, "m_lblKeysDesc");
			this.m_lblKeysDesc.Name = "m_lblKeysDesc";
			// 
			// m_lblKeysIntro
			// 
			resources.ApplyResources(this.m_lblKeysIntro, "m_lblKeysIntro");
			this.m_lblKeysIntro.Name = "m_lblKeysIntro";
			// 
			// m_tbEdit
			// 
			resources.ApplyResources(this.m_tbEdit, "m_tbEdit");
			this.m_tbEdit.Name = "m_tbEdit";
			// 
			// EntropyForm
			// 
			this.AcceptButton = this.m_btnOK;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.Controls.Add(this.m_grpKeyboard);
			this.Controls.Add(this.m_grpMouse);
			this.Controls.Add(this.m_bannerImage);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOK);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "EntropyForm";
			this.ShowInTaskbar = false;
			this.Load += new System.EventHandler(this.OnFormLoad);
			((System.ComponentModel.ISupportInitialize)(this.m_picRandom)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
			this.m_grpMouse.ResumeLayout(false);
			this.m_grpMouse.PerformLayout();
			this.m_grpKeyboard.ResumeLayout(false);
			this.m_grpKeyboard.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.PictureBox m_picRandom;
		private System.Windows.Forms.Label m_lblHint;
		private System.Windows.Forms.Label m_lblGeneratedHint;
		private KeePass.UI.QualityProgressBar m_pbGenerated;
		private System.Windows.Forms.Label m_lblStatus;
		private System.Windows.Forms.PictureBox m_bannerImage;
		private System.Windows.Forms.GroupBox m_grpMouse;
		private System.Windows.Forms.GroupBox m_grpKeyboard;
		private System.Windows.Forms.TextBox m_tbEdit;
		private System.Windows.Forms.Label m_lblKeysIntro;
		private System.Windows.Forms.Label m_lblKeysDesc;
	}
}