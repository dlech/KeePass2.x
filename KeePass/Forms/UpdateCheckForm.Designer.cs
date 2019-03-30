﻿namespace KeePass.Forms
{
	partial class UpdateCheckForm
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
            this.m_btnClose = new System.Windows.Forms.Button();
            this.m_bannerImage = new System.Windows.Forms.PictureBox();
            this.m_lvInfo = new KeePass.UI.CustomListViewEx();
            this.m_linkWeb = new System.Windows.Forms.LinkLabel();
            this.m_linkPlugins = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
            this.SuspendLayout();
            // 
            // m_btnClose
            // 
            this.m_btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_btnClose.Location = new System.Drawing.Point(679, 478);
            this.m_btnClose.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnClose.Name = "m_btnClose";
            this.m_btnClose.Size = new System.Drawing.Size(100, 35);
            this.m_btnClose.TabIndex = 0;
            this.m_btnClose.Text = "&Close";
            this.m_btnClose.UseVisualStyleBackColor = true;
            // 
            // m_bannerImage
            // 
            this.m_bannerImage.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_bannerImage.Location = new System.Drawing.Point(0, 0);
            this.m_bannerImage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_bannerImage.Name = "m_bannerImage";
            this.m_bannerImage.Size = new System.Drawing.Size(795, 92);
            this.m_bannerImage.TabIndex = 1;
            this.m_bannerImage.TabStop = false;
            // 
            // m_lvInfo
            // 
            this.m_lvInfo.FullRowSelect = true;
            this.m_lvInfo.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.m_lvInfo.HideSelection = false;
            this.m_lvInfo.Location = new System.Drawing.Point(16, 109);
            this.m_lvInfo.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_lvInfo.MultiSelect = false;
            this.m_lvInfo.Name = "m_lvInfo";
            this.m_lvInfo.ShowItemToolTips = true;
            this.m_lvInfo.Size = new System.Drawing.Size(761, 356);
            this.m_lvInfo.TabIndex = 1;
            this.m_lvInfo.UseCompatibleStateImageBehavior = false;
            this.m_lvInfo.View = System.Windows.Forms.View.Details;
            this.m_lvInfo.ItemActivate += new System.EventHandler(this.OnInfoItemActivate);
            // 
            // m_linkWeb
            // 
            this.m_linkWeb.AutoSize = true;
            this.m_linkWeb.Location = new System.Drawing.Point(12, 485);
            this.m_linkWeb.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_linkWeb.Name = "m_linkWeb";
            this.m_linkWeb.Size = new System.Drawing.Size(118, 20);
            this.m_linkWeb.TabIndex = 2;
            this.m_linkWeb.TabStop = true;
            this.m_linkWeb.Text = "KeePass Website";
            this.m_linkWeb.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnLinkWeb);
            // 
            // m_linkPlugins
            // 
            this.m_linkPlugins.AutoSize = true;
            this.m_linkPlugins.Location = new System.Drawing.Point(141, 485);
            this.m_linkPlugins.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_linkPlugins.Name = "m_linkPlugins";
            this.m_linkPlugins.Size = new System.Drawing.Size(92, 20);
            this.m_linkPlugins.TabIndex = 3;
            this.m_linkPlugins.TabStop = true;
            this.m_linkPlugins.Text = "Plugins Page";
            this.m_linkPlugins.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnLinkPlugins);
            // 
            // UpdateCheckForm
            // 
            this.AcceptButton = this.m_btnClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.m_btnClose;
            this.ClientSize = new System.Drawing.Size(795, 531);
            this.Controls.Add(this.m_linkPlugins);
            this.Controls.Add(this.m_linkWeb);
            this.Controls.Add(this.m_lvInfo);
            this.Controls.Add(this.m_bannerImage);
            this.Controls.Add(this.m_btnClose);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpdateCheckForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "<>";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
            this.Load += new System.EventHandler(this.OnFormLoad);
            ((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button m_btnClose;
		private System.Windows.Forms.PictureBox m_bannerImage;
		private KeePass.UI.CustomListViewEx m_lvInfo;
		private System.Windows.Forms.LinkLabel m_linkWeb;
		private System.Windows.Forms.LinkLabel m_linkPlugins;
	}
}