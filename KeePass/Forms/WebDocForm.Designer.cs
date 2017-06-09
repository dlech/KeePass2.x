namespace KeePass.Forms
{
	partial class WebDocForm
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
			this.m_wbMain = new System.Windows.Forms.WebBrowser();
			this.SuspendLayout();
			// 
			// m_wbMain
			// 
			this.m_wbMain.AllowWebBrowserDrop = false;
			this.m_wbMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_wbMain.IsWebBrowserContextMenuEnabled = false;
			this.m_wbMain.Location = new System.Drawing.Point(0, 0);
			this.m_wbMain.MinimumSize = new System.Drawing.Size(20, 20);
			this.m_wbMain.Name = "m_wbMain";
			this.m_wbMain.ScriptErrorsSuppressed = true;
			this.m_wbMain.Size = new System.Drawing.Size(541, 445);
			this.m_wbMain.TabIndex = 0;
			this.m_wbMain.WebBrowserShortcutsEnabled = false;
			this.m_wbMain.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.OnWebNavigating);
			// 
			// WebDocForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(541, 445);
			this.Controls.Add(this.m_wbMain);
			this.MinimizeBox = false;
			this.Name = "WebDocForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "<>";
			this.Load += new System.EventHandler(this.OnFormLoad);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.WebBrowser m_wbMain;
	}
}