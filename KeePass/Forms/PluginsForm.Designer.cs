﻿namespace KeePass.Forms
{
	partial class PluginsForm
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
            this.m_btnClose = new System.Windows.Forms.Button();
            this.m_lvPlugins = new KeePass.UI.CustomListViewEx();
            this.m_lblSeparator = new System.Windows.Forms.Label();
            this.m_bannerImage = new System.Windows.Forms.PictureBox();
            this.m_grpPluginDesc = new System.Windows.Forms.GroupBox();
            this.m_lblSelectedPluginDesc = new System.Windows.Forms.Label();
            this.m_linkPlugins = new System.Windows.Forms.LinkLabel();
            this.m_grpCache = new System.Windows.Forms.GroupBox();
            this.m_cbCacheDeleteOld = new System.Windows.Forms.CheckBox();
            this.m_btnClearCache = new System.Windows.Forms.Button();
            this.m_lblCacheSize = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
            this.m_grpPluginDesc.SuspendLayout();
            this.m_grpCache.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_btnClose
            // 
            this.m_btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_btnClose.Location = new System.Drawing.Point(709, 662);
            this.m_btnClose.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnClose.Name = "m_btnClose";
            this.m_btnClose.Size = new System.Drawing.Size(100, 35);
            this.m_btnClose.TabIndex = 0;
            this.m_btnClose.Text = "&Close";
            this.m_btnClose.UseVisualStyleBackColor = true;
            this.m_btnClose.Click += new System.EventHandler(this.OnBtnClose);
            // 
            // m_lvPlugins
            // 
            this.m_lvPlugins.FullRowSelect = true;
            this.m_lvPlugins.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.m_lvPlugins.HideSelection = false;
            this.m_lvPlugins.Location = new System.Drawing.Point(16, 101);
            this.m_lvPlugins.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_lvPlugins.Name = "m_lvPlugins";
            this.m_lvPlugins.ShowItemToolTips = true;
            this.m_lvPlugins.Size = new System.Drawing.Size(792, 252);
            this.m_lvPlugins.TabIndex = 1;
            this.m_lvPlugins.UseCompatibleStateImageBehavior = false;
            this.m_lvPlugins.View = System.Windows.Forms.View.Details;
            this.m_lvPlugins.SelectedIndexChanged += new System.EventHandler(this.OnPluginListSelectedIndexChanged);
            // 
            // m_lblSeparator
            // 
            this.m_lblSeparator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.m_lblSeparator.Location = new System.Drawing.Point(0, 649);
            this.m_lblSeparator.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblSeparator.Name = "m_lblSeparator";
            this.m_lblSeparator.Size = new System.Drawing.Size(825, 2);
            this.m_lblSeparator.TabIndex = 4;
            // 
            // m_bannerImage
            // 
            this.m_bannerImage.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_bannerImage.Location = new System.Drawing.Point(0, 0);
            this.m_bannerImage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_bannerImage.Name = "m_bannerImage";
            this.m_bannerImage.Size = new System.Drawing.Size(825, 92);
            this.m_bannerImage.TabIndex = 0;
            this.m_bannerImage.TabStop = false;
            // 
            // m_grpPluginDesc
            // 
            this.m_grpPluginDesc.Controls.Add(this.m_lblSelectedPluginDesc);
            this.m_grpPluginDesc.Location = new System.Drawing.Point(16, 365);
            this.m_grpPluginDesc.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpPluginDesc.Name = "m_grpPluginDesc";
            this.m_grpPluginDesc.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpPluginDesc.Size = new System.Drawing.Size(793, 158);
            this.m_grpPluginDesc.TabIndex = 2;
            this.m_grpPluginDesc.TabStop = false;
            // 
            // m_lblSelectedPluginDesc
            // 
            this.m_lblSelectedPluginDesc.Location = new System.Drawing.Point(8, 25);
            this.m_lblSelectedPluginDesc.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblSelectedPluginDesc.Name = "m_lblSelectedPluginDesc";
            this.m_lblSelectedPluginDesc.Size = new System.Drawing.Size(777, 99);
            this.m_lblSelectedPluginDesc.TabIndex = 0;
            this.m_lblSelectedPluginDesc.Text = "<>";
            // 
            // m_linkPlugins
            // 
            this.m_linkPlugins.AutoSize = true;
            this.m_linkPlugins.Location = new System.Drawing.Point(12, 671);
            this.m_linkPlugins.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_linkPlugins.Name = "m_linkPlugins";
            this.m_linkPlugins.Size = new System.Drawing.Size(132, 20);
            this.m_linkPlugins.TabIndex = 5;
            this.m_linkPlugins.TabStop = true;
            this.m_linkPlugins.Text = "Get more plugins...";
            this.m_linkPlugins.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnPluginsLinkClicked);
            // 
            // m_grpCache
            // 
            this.m_grpCache.Controls.Add(this.m_cbCacheDeleteOld);
            this.m_grpCache.Controls.Add(this.m_btnClearCache);
            this.m_grpCache.Controls.Add(this.m_lblCacheSize);
            this.m_grpCache.Location = new System.Drawing.Point(16, 531);
            this.m_grpCache.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpCache.Name = "m_grpCache";
            this.m_grpCache.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpCache.Size = new System.Drawing.Size(793, 100);
            this.m_grpCache.TabIndex = 3;
            this.m_grpCache.TabStop = false;
            this.m_grpCache.Text = "Plugin Cache";
            // 
            // m_cbCacheDeleteOld
            // 
            this.m_cbCacheDeleteOld.AutoSize = true;
            this.m_cbCacheDeleteOld.Location = new System.Drawing.Point(12, 61);
            this.m_cbCacheDeleteOld.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cbCacheDeleteOld.Name = "m_cbCacheDeleteOld";
            this.m_cbCacheDeleteOld.Size = new System.Drawing.Size(304, 24);
            this.m_cbCacheDeleteOld.TabIndex = 2;
            this.m_cbCacheDeleteOld.Text = "Delete old files from cache automatically";
            this.m_cbCacheDeleteOld.UseVisualStyleBackColor = true;
            // 
            // m_btnClearCache
            // 
            this.m_btnClearCache.Location = new System.Drawing.Point(679, 29);
            this.m_btnClearCache.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnClearCache.Name = "m_btnClearCache";
            this.m_btnClearCache.Size = new System.Drawing.Size(100, 35);
            this.m_btnClearCache.TabIndex = 1;
            this.m_btnClearCache.Text = "C&lear";
            this.m_btnClearCache.UseVisualStyleBackColor = true;
            this.m_btnClearCache.Click += new System.EventHandler(this.OnBtnClearCache);
            // 
            // m_lblCacheSize
            // 
            this.m_lblCacheSize.Location = new System.Drawing.Point(8, 29);
            this.m_lblCacheSize.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblCacheSize.Name = "m_lblCacheSize";
            this.m_lblCacheSize.Size = new System.Drawing.Size(663, 28);
            this.m_lblCacheSize.TabIndex = 0;
            this.m_lblCacheSize.Text = "Data in cache (size):";
            // 
            // PluginsForm
            // 
            this.AcceptButton = this.m_btnClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.m_btnClose;
            this.ClientSize = new System.Drawing.Size(825, 718);
            this.Controls.Add(this.m_grpCache);
            this.Controls.Add(this.m_linkPlugins);
            this.Controls.Add(this.m_grpPluginDesc);
            this.Controls.Add(this.m_lblSeparator);
            this.Controls.Add(this.m_lvPlugins);
            this.Controls.Add(this.m_btnClose);
            this.Controls.Add(this.m_bannerImage);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PluginsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Plugins";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
            this.Load += new System.EventHandler(this.OnFormLoad);
            ((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
            this.m_grpPluginDesc.ResumeLayout(false);
            this.m_grpCache.ResumeLayout(false);
            this.m_grpCache.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox m_bannerImage;
		private System.Windows.Forms.Button m_btnClose;
		private KeePass.UI.CustomListViewEx m_lvPlugins;
		private System.Windows.Forms.Label m_lblSeparator;
		private System.Windows.Forms.GroupBox m_grpPluginDesc;
		private System.Windows.Forms.Label m_lblSelectedPluginDesc;
		private System.Windows.Forms.LinkLabel m_linkPlugins;
		private System.Windows.Forms.GroupBox m_grpCache;
		private System.Windows.Forms.Button m_btnClearCache;
		private System.Windows.Forms.Label m_lblCacheSize;
		private System.Windows.Forms.CheckBox m_cbCacheDeleteOld;
	}
}