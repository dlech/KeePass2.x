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
			this.m_bannerImage = new System.Windows.Forms.PictureBox();
			this.m_btnClose = new System.Windows.Forms.Button();
			this.m_btnMore = new System.Windows.Forms.Button();
			this.m_btnOpenFolder = new System.Windows.Forms.Button();
			this.m_lvLanguages = new KeePass.UI.CustomListViewEx();
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
			this.SuspendLayout();
			// 
			// m_bannerImage
			// 
			this.m_bannerImage.Dock = System.Windows.Forms.DockStyle.Top;
			this.m_bannerImage.Location = new System.Drawing.Point(0, 0);
			this.m_bannerImage.Name = "m_bannerImage";
			this.m_bannerImage.Size = new System.Drawing.Size(708, 60);
			this.m_bannerImage.TabIndex = 0;
			this.m_bannerImage.TabStop = false;
			// 
			// m_btnClose
			// 
			this.m_btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_btnClose.Location = new System.Drawing.Point(621, 321);
			this.m_btnClose.Name = "m_btnClose";
			this.m_btnClose.Size = new System.Drawing.Size(75, 23);
			this.m_btnClose.TabIndex = 1;
			this.m_btnClose.Text = "&Close";
			this.m_btnClose.UseVisualStyleBackColor = true;
			this.m_btnClose.Click += new System.EventHandler(this.OnBtnClose);
			// 
			// m_btnMore
			// 
			this.m_btnMore.Location = new System.Drawing.Point(13, 321);
			this.m_btnMore.Name = "m_btnMore";
			this.m_btnMore.Size = new System.Drawing.Size(134, 23);
			this.m_btnMore.TabIndex = 2;
			this.m_btnMore.Text = "Get More &Languages...";
			this.m_btnMore.UseVisualStyleBackColor = true;
			this.m_btnMore.Click += new System.EventHandler(this.OnBtnGetMore);
			// 
			// m_btnOpenFolder
			// 
			this.m_btnOpenFolder.Location = new System.Drawing.Point(153, 321);
			this.m_btnOpenFolder.Name = "m_btnOpenFolder";
			this.m_btnOpenFolder.Size = new System.Drawing.Size(83, 23);
			this.m_btnOpenFolder.TabIndex = 3;
			this.m_btnOpenFolder.Text = "Open &Folder";
			this.m_btnOpenFolder.UseVisualStyleBackColor = true;
			this.m_btnOpenFolder.Click += new System.EventHandler(this.OnBtnOpenFolder);
			// 
			// m_lvLanguages
			// 
			this.m_lvLanguages.Activation = System.Windows.Forms.ItemActivation.OneClick;
			this.m_lvLanguages.FullRowSelect = true;
			this.m_lvLanguages.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.m_lvLanguages.HideSelection = false;
			this.m_lvLanguages.Location = new System.Drawing.Point(13, 67);
			this.m_lvLanguages.MultiSelect = false;
			this.m_lvLanguages.Name = "m_lvLanguages";
			this.m_lvLanguages.ShowItemToolTips = true;
			this.m_lvLanguages.Size = new System.Drawing.Size(683, 248);
			this.m_lvLanguages.TabIndex = 0;
			this.m_lvLanguages.UseCompatibleStateImageBehavior = false;
			this.m_lvLanguages.View = System.Windows.Forms.View.Details;
			this.m_lvLanguages.ItemActivate += new System.EventHandler(this.OnLanguagesItemActivate);
			// 
			// LanguageForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnClose;
			this.ClientSize = new System.Drawing.Size(708, 356);
			this.Controls.Add(this.m_btnOpenFolder);
			this.Controls.Add(this.m_btnMore);
			this.Controls.Add(this.m_btnClose);
			this.Controls.Add(this.m_lvLanguages);
			this.Controls.Add(this.m_bannerImage);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LanguageForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "<>";
			this.Load += new System.EventHandler(this.OnFormLoad);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PictureBox m_bannerImage;
		private KeePass.UI.CustomListViewEx m_lvLanguages;
		private System.Windows.Forms.Button m_btnClose;
		private System.Windows.Forms.Button m_btnMore;
		private System.Windows.Forms.Button m_btnOpenFolder;
	}
}