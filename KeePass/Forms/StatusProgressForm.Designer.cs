namespace KeePass.Forms
{
	partial class StatusProgressForm
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
            this.m_pbTotal = new System.Windows.Forms.ProgressBar();
            this.m_btnCancel = new System.Windows.Forms.Button();
            this.m_lblTotal = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // m_pbTotal
            // 
            this.m_pbTotal.Location = new System.Drawing.Point(16, 45);
            this.m_pbTotal.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_pbTotal.Name = "m_pbTotal";
            this.m_pbTotal.Size = new System.Drawing.Size(504, 25);
            this.m_pbTotal.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.m_pbTotal.TabIndex = 0;
            // 
            // m_btnCancel
            // 
            this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_btnCancel.Location = new System.Drawing.Point(421, 91);
            this.m_btnCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnCancel.Name = "m_btnCancel";
            this.m_btnCancel.Size = new System.Drawing.Size(100, 35);
            this.m_btnCancel.TabIndex = 1;
            this.m_btnCancel.Text = "Cancel";
            this.m_btnCancel.UseVisualStyleBackColor = true;
            this.m_btnCancel.Click += new System.EventHandler(this.OnBtnCancel);
            // 
            // m_lblTotal
            // 
            this.m_lblTotal.Location = new System.Drawing.Point(12, 14);
            this.m_lblTotal.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblTotal.Name = "m_lblTotal";
            this.m_lblTotal.Size = new System.Drawing.Size(509, 26);
            this.m_lblTotal.TabIndex = 2;
            // 
            // StatusProgressForm
            // 
            this.AcceptButton = this.m_btnCancel;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.m_btnCancel;
            this.ClientSize = new System.Drawing.Size(537, 145);
            this.Controls.Add(this.m_lblTotal);
            this.Controls.Add(this.m_btnCancel);
            this.Controls.Add(this.m_pbTotal);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "StatusProgressForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "<>";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ProgressBar m_pbTotal;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.Label m_lblTotal;
	}
}