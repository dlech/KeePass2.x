namespace KeePass.Forms
{
	partial class DatabaseOperationsForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DatabaseOperationsForm));
			this.m_btnClose = new System.Windows.Forms.Button();
			this.m_bannerImage = new System.Windows.Forms.PictureBox();
			this.m_grpHistoryDelete = new System.Windows.Forms.GroupBox();
			this.m_lblEntryHistoryWarning = new System.Windows.Forms.Label();
			this.m_btnHistoryEntriesDelete = new System.Windows.Forms.Button();
			this.m_lblDays = new System.Windows.Forms.Label();
			this.m_numHistoryDays = new System.Windows.Forms.NumericUpDown();
			this.m_lblHistoryEntriesDays = new System.Windows.Forms.Label();
			this.m_lblDeleteHistoryEntries = new System.Windows.Forms.Label();
			this.m_lblTrashIcon = new System.Windows.Forms.Label();
			this.m_tabMain = new System.Windows.Forms.TabControl();
			this.m_tabCleanUp = new System.Windows.Forms.TabPage();
			this.m_pbStatus = new System.Windows.Forms.ProgressBar();
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
			this.m_grpHistoryDelete.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numHistoryDays)).BeginInit();
			this.m_tabMain.SuspendLayout();
			this.m_tabCleanUp.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_btnClose
			// 
			this.m_btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			resources.ApplyResources(this.m_btnClose, "m_btnClose");
			this.m_btnClose.Name = "m_btnClose";
			this.m_btnClose.UseVisualStyleBackColor = true;
			this.m_btnClose.Click += new System.EventHandler(this.OnBtnClose);
			// 
			// m_bannerImage
			// 
			resources.ApplyResources(this.m_bannerImage, "m_bannerImage");
			this.m_bannerImage.Name = "m_bannerImage";
			this.m_bannerImage.TabStop = false;
			// 
			// m_grpHistoryDelete
			// 
			this.m_grpHistoryDelete.Controls.Add(this.m_lblEntryHistoryWarning);
			this.m_grpHistoryDelete.Controls.Add(this.m_btnHistoryEntriesDelete);
			this.m_grpHistoryDelete.Controls.Add(this.m_lblDays);
			this.m_grpHistoryDelete.Controls.Add(this.m_numHistoryDays);
			this.m_grpHistoryDelete.Controls.Add(this.m_lblHistoryEntriesDays);
			this.m_grpHistoryDelete.Controls.Add(this.m_lblDeleteHistoryEntries);
			this.m_grpHistoryDelete.Controls.Add(this.m_lblTrashIcon);
			resources.ApplyResources(this.m_grpHistoryDelete, "m_grpHistoryDelete");
			this.m_grpHistoryDelete.Name = "m_grpHistoryDelete";
			this.m_grpHistoryDelete.TabStop = false;
			// 
			// m_lblEntryHistoryWarning
			// 
			resources.ApplyResources(this.m_lblEntryHistoryWarning, "m_lblEntryHistoryWarning");
			this.m_lblEntryHistoryWarning.Name = "m_lblEntryHistoryWarning";
			// 
			// m_btnHistoryEntriesDelete
			// 
			resources.ApplyResources(this.m_btnHistoryEntriesDelete, "m_btnHistoryEntriesDelete");
			this.m_btnHistoryEntriesDelete.Name = "m_btnHistoryEntriesDelete";
			this.m_btnHistoryEntriesDelete.UseVisualStyleBackColor = true;
			this.m_btnHistoryEntriesDelete.Click += new System.EventHandler(this.OnBtnDelete);
			// 
			// m_lblDays
			// 
			resources.ApplyResources(this.m_lblDays, "m_lblDays");
			this.m_lblDays.Name = "m_lblDays";
			// 
			// m_numHistoryDays
			// 
			resources.ApplyResources(this.m_numHistoryDays, "m_numHistoryDays");
			this.m_numHistoryDays.Maximum = new decimal(new int[] {
            3650,
            0,
            0,
            0});
			this.m_numHistoryDays.Name = "m_numHistoryDays";
			this.m_numHistoryDays.Value = new decimal(new int[] {
            365,
            0,
            0,
            0});
			// 
			// m_lblHistoryEntriesDays
			// 
			resources.ApplyResources(this.m_lblHistoryEntriesDays, "m_lblHistoryEntriesDays");
			this.m_lblHistoryEntriesDays.Name = "m_lblHistoryEntriesDays";
			// 
			// m_lblDeleteHistoryEntries
			// 
			resources.ApplyResources(this.m_lblDeleteHistoryEntries, "m_lblDeleteHistoryEntries");
			this.m_lblDeleteHistoryEntries.Name = "m_lblDeleteHistoryEntries";
			// 
			// m_lblTrashIcon
			// 
			this.m_lblTrashIcon.Image = global::KeePass.Properties.Resources.B32x32_Trashcan_Full;
			resources.ApplyResources(this.m_lblTrashIcon, "m_lblTrashIcon");
			this.m_lblTrashIcon.Name = "m_lblTrashIcon";
			// 
			// m_tabMain
			// 
			this.m_tabMain.Controls.Add(this.m_tabCleanUp);
			resources.ApplyResources(this.m_tabMain, "m_tabMain");
			this.m_tabMain.Name = "m_tabMain";
			this.m_tabMain.SelectedIndex = 0;
			// 
			// m_tabCleanUp
			// 
			this.m_tabCleanUp.Controls.Add(this.m_grpHistoryDelete);
			resources.ApplyResources(this.m_tabCleanUp, "m_tabCleanUp");
			this.m_tabCleanUp.Name = "m_tabCleanUp";
			this.m_tabCleanUp.UseVisualStyleBackColor = true;
			// 
			// m_pbStatus
			// 
			resources.ApplyResources(this.m_pbStatus, "m_pbStatus");
			this.m_pbStatus.Name = "m_pbStatus";
			this.m_pbStatus.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			// 
			// DatabaseOperationsForm
			// 
			this.AcceptButton = this.m_btnClose;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnClose;
			this.Controls.Add(this.m_pbStatus);
			this.Controls.Add(this.m_tabMain);
			this.Controls.Add(this.m_bannerImage);
			this.Controls.Add(this.m_btnClose);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "DatabaseOperationsForm";
			this.ShowInTaskbar = false;
			this.Load += new System.EventHandler(this.OnFormLoad);
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
			this.m_grpHistoryDelete.ResumeLayout(false);
			this.m_grpHistoryDelete.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numHistoryDays)).EndInit();
			this.m_tabMain.ResumeLayout(false);
			this.m_tabCleanUp.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button m_btnClose;
		private System.Windows.Forms.PictureBox m_bannerImage;
		private System.Windows.Forms.GroupBox m_grpHistoryDelete;
		private System.Windows.Forms.Label m_lblTrashIcon;
		private System.Windows.Forms.NumericUpDown m_numHistoryDays;
		private System.Windows.Forms.Label m_lblHistoryEntriesDays;
		private System.Windows.Forms.Label m_lblDeleteHistoryEntries;
		private System.Windows.Forms.Button m_btnHistoryEntriesDelete;
		private System.Windows.Forms.Label m_lblDays;
		private System.Windows.Forms.TabControl m_tabMain;
		private System.Windows.Forms.TabPage m_tabCleanUp;
		private System.Windows.Forms.Label m_lblEntryHistoryWarning;
		private System.Windows.Forms.ProgressBar m_pbStatus;
	}
}