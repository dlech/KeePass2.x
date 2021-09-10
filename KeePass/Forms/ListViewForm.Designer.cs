namespace KeePass.Forms
{
	partial class ListViewForm
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
			this.m_lvMain = new KeePass.UI.CustomListViewEx();
			this.m_bannerImage = new System.Windows.Forms.PictureBox();
			this.m_lblInfo = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
			this.SuspendLayout();
			// 
			// m_lvMain
			// 
			this.m_lvMain.Activation = System.Windows.Forms.ItemActivation.OneClick;
			this.m_lvMain.FullRowSelect = true;
			this.m_lvMain.GridLines = true;
			this.m_lvMain.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.m_lvMain.HideSelection = false;
			this.m_lvMain.HotTracking = true;
			this.m_lvMain.HoverSelection = true;
			this.m_lvMain.Location = new System.Drawing.Point(12, 104);
			this.m_lvMain.MultiSelect = false;
			this.m_lvMain.Name = "m_lvMain";
			this.m_lvMain.ShowItemToolTips = true;
			this.m_lvMain.Size = new System.Drawing.Size(670, 418);
			this.m_lvMain.TabIndex = 1;
			this.m_lvMain.UseCompatibleStateImageBehavior = false;
			this.m_lvMain.View = System.Windows.Forms.View.Details;
			this.m_lvMain.ItemActivate += new System.EventHandler(this.OnListItemActivate);
			this.m_lvMain.Click += new System.EventHandler(this.OnListClick);
			// 
			// m_bannerImage
			// 
			this.m_bannerImage.Dock = System.Windows.Forms.DockStyle.Top;
			this.m_bannerImage.Location = new System.Drawing.Point(0, 0);
			this.m_bannerImage.Name = "m_bannerImage";
			this.m_bannerImage.Size = new System.Drawing.Size(694, 60);
			this.m_bannerImage.TabIndex = 3;
			this.m_bannerImage.TabStop = false;
			// 
			// m_lblInfo
			// 
			this.m_lblInfo.Location = new System.Drawing.Point(9, 72);
			this.m_lblInfo.Name = "m_lblInfo";
			this.m_lblInfo.Size = new System.Drawing.Size(673, 29);
			this.m_lblInfo.TabIndex = 0;
			this.m_lblInfo.Text = "<>";
			// 
			// ListViewForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(694, 534);
			this.Controls.Add(this.m_lblInfo);
			this.Controls.Add(this.m_bannerImage);
			this.Controls.Add(this.m_lvMain);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ListViewForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "<>";
			this.Load += new System.EventHandler(this.OnFormLoad);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private KeePass.UI.CustomListViewEx m_lvMain;
		private System.Windows.Forms.PictureBox m_bannerImage;
		private System.Windows.Forms.Label m_lblInfo;
	}
}