namespace KeePass.Forms
{
	partial class LanguageForm
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LanguageForm));
			this.m_bannerImage = new System.Windows.Forms.PictureBox();
			this.m_lvLanguages = new System.Windows.Forms.ListView();
			this.m_ilLanguages = new System.Windows.Forms.ImageList(this.components);
			this.m_btnClose = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
			this.SuspendLayout();
			// 
			// m_bannerImage
			// 
			resources.ApplyResources(this.m_bannerImage, "m_bannerImage");
			this.m_bannerImage.Name = "m_bannerImage";
			this.m_bannerImage.TabStop = false;
			// 
			// m_lvLanguages
			// 
			this.m_lvLanguages.Activation = System.Windows.Forms.ItemActivation.OneClick;
			this.m_lvLanguages.FullRowSelect = true;
			this.m_lvLanguages.GridLines = true;
			this.m_lvLanguages.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.m_lvLanguages.HideSelection = false;
			resources.ApplyResources(this.m_lvLanguages, "m_lvLanguages");
			this.m_lvLanguages.MultiSelect = false;
			this.m_lvLanguages.Name = "m_lvLanguages";
			this.m_lvLanguages.SmallImageList = this.m_ilLanguages;
			this.m_lvLanguages.UseCompatibleStateImageBehavior = false;
			this.m_lvLanguages.View = System.Windows.Forms.View.Details;
			this.m_lvLanguages.ItemActivate += new System.EventHandler(this.OnLanguagesItemActivate);
			// 
			// m_ilLanguages
			// 
			this.m_ilLanguages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("m_ilLanguages.ImageStream")));
			this.m_ilLanguages.TransparentColor = System.Drawing.Color.Transparent;
			this.m_ilLanguages.Images.SetKeyName(0, "B16x16_Browser.png");
			// 
			// m_btnClose
			// 
			this.m_btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			resources.ApplyResources(this.m_btnClose, "m_btnClose");
			this.m_btnClose.Name = "m_btnClose";
			this.m_btnClose.UseVisualStyleBackColor = true;
			this.m_btnClose.Click += new System.EventHandler(this.OnBtnClose);
			// 
			// LanguageForm
			// 
			this.AcceptButton = this.m_btnClose;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnClose;
			this.Controls.Add(this.m_btnClose);
			this.Controls.Add(this.m_lvLanguages);
			this.Controls.Add(this.m_bannerImage);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LanguageForm";
			this.ShowInTaskbar = false;
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
			this.Load += new System.EventHandler(this.OnFormLoad);
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PictureBox m_bannerImage;
		private System.Windows.Forms.ListView m_lvLanguages;
		private System.Windows.Forms.Button m_btnClose;
		private System.Windows.Forms.ImageList m_ilLanguages;
	}
}