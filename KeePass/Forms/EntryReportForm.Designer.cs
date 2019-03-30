namespace KeePass.Forms
{
	partial class EntryReportForm
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
            this.m_lvEntries = new KeePass.UI.CustomListViewEx();
            this.SuspendLayout();
            // 
            // m_lvEntries
            // 
            this.m_lvEntries.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_lvEntries.FullRowSelect = true;
            this.m_lvEntries.GridLines = true;
            this.m_lvEntries.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.m_lvEntries.HideSelection = false;
            this.m_lvEntries.Location = new System.Drawing.Point(0, 0);
            this.m_lvEntries.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_lvEntries.Name = "m_lvEntries";
            this.m_lvEntries.ShowItemToolTips = true;
            this.m_lvEntries.Size = new System.Drawing.Size(816, 634);
            this.m_lvEntries.TabIndex = 0;
            this.m_lvEntries.UseCompatibleStateImageBehavior = false;
            this.m_lvEntries.View = System.Windows.Forms.View.Details;
            // 
            // EntryReportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(816, 634);
            this.Controls.Add(this.m_lvEntries);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MinimizeBox = false;
            this.Name = "EntryReportForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "<>";
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.ResumeLayout(false);

		}

		#endregion

		private KeePass.UI.CustomListViewEx m_lvEntries;

	}
}