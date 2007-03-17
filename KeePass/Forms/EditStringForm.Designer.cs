namespace KeePass.Forms
{
	partial class EditStringForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditStringForm));
			this.m_bannerImage = new System.Windows.Forms.PictureBox();
			this.m_lblStringValueDesc = new System.Windows.Forms.Label();
			this.m_lblStringIDDesc = new System.Windows.Forms.Label();
			this.m_lblIDIntro = new System.Windows.Forms.Label();
			this.m_tbStringName = new System.Windows.Forms.TextBox();
			this.m_richStringValue = new System.Windows.Forms.RichTextBox();
			this.m_lblSeparator = new System.Windows.Forms.Label();
			this.m_btnOK = new System.Windows.Forms.Button();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_btnHelp = new System.Windows.Forms.Button();
			this.m_cbProtect = new System.Windows.Forms.CheckBox();
			this.m_lblValidationInfo = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
			this.SuspendLayout();
			// 
			// m_bannerImage
			// 
			resources.ApplyResources(this.m_bannerImage, "m_bannerImage");
			this.m_bannerImage.Name = "m_bannerImage";
			this.m_bannerImage.TabStop = false;
			// 
			// m_lblStringValueDesc
			// 
			resources.ApplyResources(this.m_lblStringValueDesc, "m_lblStringValueDesc");
			this.m_lblStringValueDesc.Name = "m_lblStringValueDesc";
			// 
			// m_lblStringIDDesc
			// 
			resources.ApplyResources(this.m_lblStringIDDesc, "m_lblStringIDDesc");
			this.m_lblStringIDDesc.Name = "m_lblStringIDDesc";
			// 
			// m_lblIDIntro
			// 
			resources.ApplyResources(this.m_lblIDIntro, "m_lblIDIntro");
			this.m_lblIDIntro.Name = "m_lblIDIntro";
			// 
			// m_tbStringName
			// 
			resources.ApplyResources(this.m_tbStringName, "m_tbStringName");
			this.m_tbStringName.Name = "m_tbStringName";
			this.m_tbStringName.TextChanged += new System.EventHandler(this.OnTextChangedName);
			// 
			// m_richStringValue
			// 
			resources.ApplyResources(this.m_richStringValue, "m_richStringValue");
			this.m_richStringValue.Name = "m_richStringValue";
			// 
			// m_lblSeparator
			// 
			this.m_lblSeparator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			resources.ApplyResources(this.m_lblSeparator, "m_lblSeparator");
			this.m_lblSeparator.Name = "m_lblSeparator";
			// 
			// m_btnOK
			// 
			this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			resources.ApplyResources(this.m_btnOK, "m_btnOK");
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.UseVisualStyleBackColor = true;
			this.m_btnOK.Click += new System.EventHandler(this.OnBtnOK);
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			resources.ApplyResources(this.m_btnCancel, "m_btnCancel");
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			this.m_btnCancel.Click += new System.EventHandler(this.OnBtnCancel);
			// 
			// m_btnHelp
			// 
			resources.ApplyResources(this.m_btnHelp, "m_btnHelp");
			this.m_btnHelp.Name = "m_btnHelp";
			this.m_btnHelp.UseVisualStyleBackColor = true;
			this.m_btnHelp.Click += new System.EventHandler(this.OnBtnHelp);
			// 
			// m_cbProtect
			// 
			resources.ApplyResources(this.m_cbProtect, "m_cbProtect");
			this.m_cbProtect.Name = "m_cbProtect";
			this.m_cbProtect.UseVisualStyleBackColor = true;
			// 
			// m_lblValidationInfo
			// 
			this.m_lblValidationInfo.ForeColor = System.Drawing.Color.Crimson;
			resources.ApplyResources(this.m_lblValidationInfo, "m_lblValidationInfo");
			this.m_lblValidationInfo.Name = "m_lblValidationInfo";
			// 
			// EditStringForm
			// 
			this.AcceptButton = this.m_btnOK;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.Controls.Add(this.m_lblValidationInfo);
			this.Controls.Add(this.m_cbProtect);
			this.Controls.Add(this.m_btnHelp);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOK);
			this.Controls.Add(this.m_lblSeparator);
			this.Controls.Add(this.m_richStringValue);
			this.Controls.Add(this.m_tbStringName);
			this.Controls.Add(this.m_lblIDIntro);
			this.Controls.Add(this.m_lblStringIDDesc);
			this.Controls.Add(this.m_lblStringValueDesc);
			this.Controls.Add(this.m_bannerImage);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "EditStringForm";
			this.ShowInTaskbar = false;
			this.Load += new System.EventHandler(this.OnFormLoad);
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox m_bannerImage;
		private System.Windows.Forms.Label m_lblStringValueDesc;
		private System.Windows.Forms.Label m_lblStringIDDesc;
		private System.Windows.Forms.Label m_lblIDIntro;
		private System.Windows.Forms.TextBox m_tbStringName;
		private System.Windows.Forms.RichTextBox m_richStringValue;
		private System.Windows.Forms.Label m_lblSeparator;
		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.Button m_btnHelp;
		private System.Windows.Forms.CheckBox m_cbProtect;
		private System.Windows.Forms.Label m_lblValidationInfo;
	}
}