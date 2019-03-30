namespace KeePass.Forms
{
	partial class FieldPickerForm
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
            this.m_lvFields = new System.Windows.Forms.ListView();
            this.m_btnCancel = new System.Windows.Forms.Button();
            this.m_bannerImage = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
            this.SuspendLayout();
            // 
            // m_lvFields
            // 
            this.m_lvFields.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.m_lvFields.FullRowSelect = true;
            this.m_lvFields.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.m_lvFields.HideSelection = false;
            this.m_lvFields.HotTracking = true;
            this.m_lvFields.HoverSelection = true;
            this.m_lvFields.Location = new System.Drawing.Point(16, 111);
            this.m_lvFields.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_lvFields.MultiSelect = false;
            this.m_lvFields.Name = "m_lvFields";
            this.m_lvFields.ShowItemToolTips = true;
            this.m_lvFields.Size = new System.Drawing.Size(561, 549);
            this.m_lvFields.TabIndex = 1;
            this.m_lvFields.UseCompatibleStateImageBehavior = false;
            this.m_lvFields.View = System.Windows.Forms.View.Details;
            this.m_lvFields.ItemActivate += new System.EventHandler(this.OnFieldItemActivate);
            this.m_lvFields.Click += new System.EventHandler(this.OnFieldClick);
            // 
            // m_btnCancel
            // 
            this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_btnCancel.Location = new System.Drawing.Point(479, 671);
            this.m_btnCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnCancel.Name = "m_btnCancel";
            this.m_btnCancel.Size = new System.Drawing.Size(100, 35);
            this.m_btnCancel.TabIndex = 2;
            this.m_btnCancel.Text = "Cancel";
            this.m_btnCancel.UseVisualStyleBackColor = true;
            // 
            // m_bannerImage
            // 
            this.m_bannerImage.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_bannerImage.Location = new System.Drawing.Point(0, 0);
            this.m_bannerImage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_bannerImage.Name = "m_bannerImage";
            this.m_bannerImage.Size = new System.Drawing.Size(595, 92);
            this.m_bannerImage.TabIndex = 3;
            this.m_bannerImage.TabStop = false;
            // 
            // FieldPickerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.m_btnCancel;
            this.ClientSize = new System.Drawing.Size(595, 725);
            this.Controls.Add(this.m_bannerImage);
            this.Controls.Add(this.m_btnCancel);
            this.Controls.Add(this.m_lvFields);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FieldPickerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "<>";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
            this.Load += new System.EventHandler(this.OnFormLoad);
            ((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListView m_lvFields;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.PictureBox m_bannerImage;
	}
}