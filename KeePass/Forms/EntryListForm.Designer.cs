namespace KeePass.Forms
{
	partial class EntryListForm
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
            this.m_btnCancel = new System.Windows.Forms.Button();
            this.m_lvEntries = new KeePass.UI.CustomListViewEx();
            this.m_bannerImage = new System.Windows.Forms.PictureBox();
            this.m_lblText = new System.Windows.Forms.Label();
            this.m_btnOK = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
            this.SuspendLayout();
            // 
            // m_btnCancel
            // 
            this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_btnCancel.Location = new System.Drawing.Point(555, 449);
            this.m_btnCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnCancel.Name = "m_btnCancel";
            this.m_btnCancel.Size = new System.Drawing.Size(100, 35);
            this.m_btnCancel.TabIndex = 2;
            this.m_btnCancel.Text = "Cancel";
            this.m_btnCancel.UseVisualStyleBackColor = true;
            this.m_btnCancel.Click += new System.EventHandler(this.OnBtnCancel);
            // 
            // m_lvEntries
            // 
            this.m_lvEntries.FullRowSelect = true;
            this.m_lvEntries.GridLines = true;
            this.m_lvEntries.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.m_lvEntries.HideSelection = false;
            this.m_lvEntries.Location = new System.Drawing.Point(16, 160);
            this.m_lvEntries.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_lvEntries.MultiSelect = false;
            this.m_lvEntries.Name = "m_lvEntries";
            this.m_lvEntries.ShowItemToolTips = true;
            this.m_lvEntries.Size = new System.Drawing.Size(637, 278);
            this.m_lvEntries.TabIndex = 0;
            this.m_lvEntries.UseCompatibleStateImageBehavior = false;
            this.m_lvEntries.View = System.Windows.Forms.View.Details;
            this.m_lvEntries.ItemActivate += new System.EventHandler(this.OnEntriesItemActivate);
            this.m_lvEntries.SelectedIndexChanged += new System.EventHandler(this.OnEntriesSelectedIndexChanged);
            // 
            // m_bannerImage
            // 
            this.m_bannerImage.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_bannerImage.Location = new System.Drawing.Point(0, 0);
            this.m_bannerImage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_bannerImage.Name = "m_bannerImage";
            this.m_bannerImage.Size = new System.Drawing.Size(671, 92);
            this.m_bannerImage.TabIndex = 3;
            this.m_bannerImage.TabStop = false;
            // 
            // m_lblText
            // 
            this.m_lblText.Location = new System.Drawing.Point(12, 111);
            this.m_lblText.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblText.Name = "m_lblText";
            this.m_lblText.Size = new System.Drawing.Size(643, 45);
            this.m_lblText.TabIndex = 3;
            this.m_lblText.Text = "<>";
            // 
            // m_btnOK
            // 
            this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_btnOK.Location = new System.Drawing.Point(447, 449);
            this.m_btnOK.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnOK.Name = "m_btnOK";
            this.m_btnOK.Size = new System.Drawing.Size(100, 35);
            this.m_btnOK.TabIndex = 1;
            this.m_btnOK.Text = "OK";
            this.m_btnOK.UseVisualStyleBackColor = true;
            this.m_btnOK.Click += new System.EventHandler(this.OnBtnOK);
            // 
            // EntryListForm
            // 
            this.AcceptButton = this.m_btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.m_btnCancel;
            this.ClientSize = new System.Drawing.Size(671, 502);
            this.Controls.Add(this.m_btnOK);
            this.Controls.Add(this.m_lblText);
            this.Controls.Add(this.m_bannerImage);
            this.Controls.Add(this.m_lvEntries);
            this.Controls.Add(this.m_btnCancel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EntryListForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "<>";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
            this.Load += new System.EventHandler(this.OnFormLoad);
            ((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button m_btnCancel;
		private KeePass.UI.CustomListViewEx m_lvEntries;
		private System.Windows.Forms.PictureBox m_bannerImage;
		private System.Windows.Forms.Label m_lblText;
		private System.Windows.Forms.Button m_btnOK;
	}
}