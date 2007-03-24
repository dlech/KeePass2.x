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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EntryListForm));
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_lvEntries = new System.Windows.Forms.ListView();
			this.m_bannerImage = new System.Windows.Forms.PictureBox();
			this.m_lblText = new System.Windows.Forms.Label();
			this.m_btnOK = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
			this.SuspendLayout();
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			resources.ApplyResources(this.m_btnCancel, "m_btnCancel");
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			this.m_btnCancel.Click += new System.EventHandler(this.OnBtnCancel);
			// 
			// m_lvEntries
			// 
			this.m_lvEntries.FullRowSelect = true;
			this.m_lvEntries.GridLines = true;
			this.m_lvEntries.HideSelection = false;
			resources.ApplyResources(this.m_lvEntries, "m_lvEntries");
			this.m_lvEntries.MultiSelect = false;
			this.m_lvEntries.Name = "m_lvEntries";
			this.m_lvEntries.ShowItemToolTips = true;
			this.m_lvEntries.UseCompatibleStateImageBehavior = false;
			this.m_lvEntries.View = System.Windows.Forms.View.Details;
			this.m_lvEntries.ItemActivate += new System.EventHandler(this.OnEntriesItemActivate);
			this.m_lvEntries.SelectedIndexChanged += new System.EventHandler(this.OnEntriesSelectedIndexChanged);
			// 
			// m_bannerImage
			// 
			resources.ApplyResources(this.m_bannerImage, "m_bannerImage");
			this.m_bannerImage.Name = "m_bannerImage";
			this.m_bannerImage.TabStop = false;
			// 
			// m_lblText
			// 
			resources.ApplyResources(this.m_lblText, "m_lblText");
			this.m_lblText.Name = "m_lblText";
			// 
			// m_btnOK
			// 
			this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			resources.ApplyResources(this.m_btnOK, "m_btnOK");
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.UseVisualStyleBackColor = true;
			this.m_btnOK.Click += new System.EventHandler(this.OnBtnOK);
			// 
			// EntryListForm
			// 
			this.AcceptButton = this.m_btnOK;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.Controls.Add(this.m_btnOK);
			this.Controls.Add(this.m_lblText);
			this.Controls.Add(this.m_bannerImage);
			this.Controls.Add(this.m_lvEntries);
			this.Controls.Add(this.m_btnCancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "EntryListForm";
			this.ShowInTaskbar = false;
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
			this.Load += new System.EventHandler(this.OnFormLoad);
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.ListView m_lvEntries;
		private System.Windows.Forms.PictureBox m_bannerImage;
		private System.Windows.Forms.Label m_lblText;
		private System.Windows.Forms.Button m_btnOK;
	}
}