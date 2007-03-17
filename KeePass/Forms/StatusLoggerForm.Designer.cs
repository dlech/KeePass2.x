namespace KeePass.Forms
{
	partial class StatusLoggerForm
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StatusLoggerForm));
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_lvMessages = new System.Windows.Forms.ListView();
			this.m_ilLogTypes = new System.Windows.Forms.ImageList(this.components);
			this.m_pbProgress = new System.Windows.Forms.ProgressBar();
			this.m_tbDetails = new System.Windows.Forms.TextBox();
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
			// m_lvMessages
			// 
			this.m_lvMessages.FullRowSelect = true;
			this.m_lvMessages.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			resources.ApplyResources(this.m_lvMessages, "m_lvMessages");
			this.m_lvMessages.MultiSelect = false;
			this.m_lvMessages.Name = "m_lvMessages";
			this.m_lvMessages.SmallImageList = this.m_ilLogTypes;
			this.m_lvMessages.UseCompatibleStateImageBehavior = false;
			this.m_lvMessages.View = System.Windows.Forms.View.Details;
			this.m_lvMessages.SelectedIndexChanged += new System.EventHandler(this.OnMessagesSelectedIndexChanged);
			// 
			// m_ilLogTypes
			// 
			this.m_ilLogTypes.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("m_ilLogTypes.ImageStream")));
			this.m_ilLogTypes.TransparentColor = System.Drawing.Color.Transparent;
			this.m_ilLogTypes.Images.SetKeyName(0, "B16x16_MessageBox_Info.png");
			this.m_ilLogTypes.Images.SetKeyName(1, "B16x16_MessageBox_Warning.png");
			this.m_ilLogTypes.Images.SetKeyName(2, "B16x16_MessageBox_Critical.png");
			this.m_ilLogTypes.Images.SetKeyName(3, "C62_Empty.png");
			// 
			// m_pbProgress
			// 
			resources.ApplyResources(this.m_pbProgress, "m_pbProgress");
			this.m_pbProgress.Name = "m_pbProgress";
			// 
			// m_tbDetails
			// 
			resources.ApplyResources(this.m_tbDetails, "m_tbDetails");
			this.m_tbDetails.Name = "m_tbDetails";
			this.m_tbDetails.ReadOnly = true;
			// 
			// StatusLoggerForm
			// 
			this.AcceptButton = this.m_btnCancel;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.Controls.Add(this.m_tbDetails);
			this.Controls.Add(this.m_pbProgress);
			this.Controls.Add(this.m_lvMessages);
			this.Controls.Add(this.m_btnCancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "StatusLoggerForm";
			this.ShowInTaskbar = false;
			this.Load += new System.EventHandler(this.OnFormLoad);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.ListView m_lvMessages;
		private System.Windows.Forms.ProgressBar m_pbProgress;
		private System.Windows.Forms.TextBox m_tbDetails;
		private System.Windows.Forms.ImageList m_ilLogTypes;
	}
}