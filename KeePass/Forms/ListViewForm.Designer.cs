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
			this.m_bannerImage = new System.Windows.Forms.PictureBox();
			this.m_lblInfo = new System.Windows.Forms.Label();
			this.m_tsMain = new KeePass.UI.CustomToolStripEx();
			this.m_tsbPrint = new System.Windows.Forms.ToolStripButton();
			this.m_tsbExport = new System.Windows.Forms.ToolStripSplitButton();
			this.m_tssSep0 = new System.Windows.Forms.ToolStripSeparator();
			this.m_tsbHideSensitive = new System.Windows.Forms.ToolStripButton();
			this.m_tssSep1 = new System.Windows.Forms.ToolStripSeparator();
			this.m_tslFilter = new System.Windows.Forms.ToolStripLabel();
			this.m_tstFilter = new System.Windows.Forms.ToolStripTextBox();
			this.m_lvMain = new KeePass.UI.CustomListViewEx();
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
			this.m_tsMain.SuspendLayout();
			this.SuspendLayout();
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
			this.m_lblInfo.Location = new System.Drawing.Point(9, 90);
			this.m_lblInfo.Name = "m_lblInfo";
			this.m_lblInfo.Size = new System.Drawing.Size(673, 29);
			this.m_lblInfo.TabIndex = 2;
			this.m_lblInfo.Text = "<>";
			// 
			// m_tsMain
			// 
			this.m_tsMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsbPrint,
            this.m_tsbExport,
            this.m_tssSep0,
            this.m_tsbHideSensitive,
            this.m_tssSep1,
            this.m_tslFilter,
            this.m_tstFilter});
			this.m_tsMain.Location = new System.Drawing.Point(0, 60);
			this.m_tsMain.Name = "m_tsMain";
			this.m_tsMain.Size = new System.Drawing.Size(694, 25);
			this.m_tsMain.TabIndex = 1;
			this.m_tsMain.TabStop = true;
			// 
			// m_tsbPrint
			// 
			this.m_tsbPrint.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbPrint.Image = global::KeePass.Properties.Resources.B16x16_FilePrint;
			this.m_tsbPrint.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbPrint.Name = "m_tsbPrint";
			this.m_tsbPrint.Size = new System.Drawing.Size(23, 22);
			this.m_tsbPrint.Click += new System.EventHandler(this.OnFilePrint);
			// 
			// m_tsbExport
			// 
			this.m_tsbExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_tsbExport.Image = global::KeePass.Properties.Resources.B16x16_Folder_Outbox;
			this.m_tsbExport.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbExport.Name = "m_tsbExport";
			this.m_tsbExport.Size = new System.Drawing.Size(32, 22);
			this.m_tsbExport.ButtonClick += new System.EventHandler(this.OnFileExport);
			// 
			// m_tssSep0
			// 
			this.m_tssSep0.Name = "m_tssSep0";
			this.m_tssSep0.Size = new System.Drawing.Size(6, 25);
			// 
			// m_tsbHideSensitive
			// 
			this.m_tsbHideSensitive.CheckOnClick = true;
			this.m_tsbHideSensitive.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.m_tsbHideSensitive.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_tsbHideSensitive.Name = "m_tsbHideSensitive";
			this.m_tsbHideSensitive.Size = new System.Drawing.Size(27, 22);
			this.m_tsbHideSensitive.Text = "<>";
			// 
			// m_tssSep1
			// 
			this.m_tssSep1.Name = "m_tssSep1";
			this.m_tssSep1.Size = new System.Drawing.Size(6, 25);
			// 
			// m_tslFilter
			// 
			this.m_tslFilter.Name = "m_tslFilter";
			this.m_tslFilter.Size = new System.Drawing.Size(23, 22);
			this.m_tslFilter.Text = "<>";
			// 
			// m_tstFilter
			// 
			this.m_tstFilter.Name = "m_tstFilter";
			this.m_tstFilter.Size = new System.Drawing.Size(121, 25);
			this.m_tstFilter.TextChanged += new System.EventHandler(this.OnFilterTextChanged);
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
			this.m_lvMain.Location = new System.Drawing.Point(12, 124);
			this.m_lvMain.MultiSelect = false;
			this.m_lvMain.Name = "m_lvMain";
			this.m_lvMain.ShowItemToolTips = true;
			this.m_lvMain.Size = new System.Drawing.Size(670, 398);
			this.m_lvMain.TabIndex = 0;
			this.m_lvMain.UseCompatibleStateImageBehavior = false;
			this.m_lvMain.View = System.Windows.Forms.View.Details;
			this.m_lvMain.ItemActivate += new System.EventHandler(this.OnListItemActivate);
			this.m_lvMain.Click += new System.EventHandler(this.OnListClick);
			// 
			// ListViewForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(694, 534);
			this.Controls.Add(this.m_tsMain);
			this.Controls.Add(this.m_bannerImage);
			this.Controls.Add(this.m_lblInfo);
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
			this.m_tsMain.ResumeLayout(false);
			this.m_tsMain.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private KeePass.UI.CustomListViewEx m_lvMain;
		private System.Windows.Forms.PictureBox m_bannerImage;
		private System.Windows.Forms.Label m_lblInfo;
		private KeePass.UI.CustomToolStripEx m_tsMain;
		private System.Windows.Forms.ToolStripButton m_tsbPrint;
		private System.Windows.Forms.ToolStripSplitButton m_tsbExport;
		private System.Windows.Forms.ToolStripSeparator m_tssSep0;
		private System.Windows.Forms.ToolStripLabel m_tslFilter;
		private System.Windows.Forms.ToolStripTextBox m_tstFilter;
		private System.Windows.Forms.ToolStripSeparator m_tssSep1;
		private System.Windows.Forms.ToolStripButton m_tsbHideSensitive;
	}
}