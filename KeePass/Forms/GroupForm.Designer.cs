namespace KeePass.Forms
{
	partial class GroupForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GroupForm));
			this.m_lblName = new System.Windows.Forms.Label();
			this.m_bannerImage = new System.Windows.Forms.PictureBox();
			this.m_btnOK = new System.Windows.Forms.Button();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_tbName = new System.Windows.Forms.TextBox();
			this.m_lblIcon = new System.Windows.Forms.Label();
			this.m_btnIcon = new System.Windows.Forms.Button();
			this.m_tabMain = new System.Windows.Forms.TabControl();
			this.m_tabGeneral = new System.Windows.Forms.TabPage();
			this.m_dtExpires = new System.Windows.Forms.DateTimePicker();
			this.m_cbExpires = new System.Windows.Forms.CheckBox();
			this.m_tabAutoType = new System.Windows.Forms.TabPage();
			this.m_btnAutoTypeEdit = new System.Windows.Forms.Button();
			this.m_rbAutoTypeOverride = new System.Windows.Forms.RadioButton();
			this.m_rbAutoTypeInherit = new System.Windows.Forms.RadioButton();
			this.m_lblAutoTypeDesc = new System.Windows.Forms.Label();
			this.m_tbDefaultAutoTypeSeq = new System.Windows.Forms.TextBox();
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
			this.m_tabMain.SuspendLayout();
			this.m_tabGeneral.SuspendLayout();
			this.m_tabAutoType.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_lblName
			// 
			resources.ApplyResources(this.m_lblName, "m_lblName");
			this.m_lblName.Name = "m_lblName";
			// 
			// m_bannerImage
			// 
			resources.ApplyResources(this.m_bannerImage, "m_bannerImage");
			this.m_bannerImage.Name = "m_bannerImage";
			this.m_bannerImage.TabStop = false;
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
			// m_tbName
			// 
			resources.ApplyResources(this.m_tbName, "m_tbName");
			this.m_tbName.Name = "m_tbName";
			// 
			// m_lblIcon
			// 
			resources.ApplyResources(this.m_lblIcon, "m_lblIcon");
			this.m_lblIcon.Name = "m_lblIcon";
			// 
			// m_btnIcon
			// 
			resources.ApplyResources(this.m_btnIcon, "m_btnIcon");
			this.m_btnIcon.Name = "m_btnIcon";
			this.m_btnIcon.UseVisualStyleBackColor = true;
			this.m_btnIcon.Click += new System.EventHandler(this.OnBtnIcon);
			// 
			// m_tabMain
			// 
			this.m_tabMain.Controls.Add(this.m_tabGeneral);
			this.m_tabMain.Controls.Add(this.m_tabAutoType);
			resources.ApplyResources(this.m_tabMain, "m_tabMain");
			this.m_tabMain.Name = "m_tabMain";
			this.m_tabMain.SelectedIndex = 0;
			// 
			// m_tabGeneral
			// 
			this.m_tabGeneral.Controls.Add(this.m_dtExpires);
			this.m_tabGeneral.Controls.Add(this.m_cbExpires);
			this.m_tabGeneral.Controls.Add(this.m_lblName);
			this.m_tabGeneral.Controls.Add(this.m_btnIcon);
			this.m_tabGeneral.Controls.Add(this.m_tbName);
			this.m_tabGeneral.Controls.Add(this.m_lblIcon);
			resources.ApplyResources(this.m_tabGeneral, "m_tabGeneral");
			this.m_tabGeneral.Name = "m_tabGeneral";
			this.m_tabGeneral.UseVisualStyleBackColor = true;
			// 
			// m_dtExpires
			// 
			this.m_dtExpires.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			resources.ApplyResources(this.m_dtExpires, "m_dtExpires");
			this.m_dtExpires.Name = "m_dtExpires";
			this.m_dtExpires.ValueChanged += new System.EventHandler(this.OnExpiresValueChanged);
			// 
			// m_cbExpires
			// 
			resources.ApplyResources(this.m_cbExpires, "m_cbExpires");
			this.m_cbExpires.Name = "m_cbExpires";
			this.m_cbExpires.UseVisualStyleBackColor = true;
			// 
			// m_tabAutoType
			// 
			this.m_tabAutoType.Controls.Add(this.m_btnAutoTypeEdit);
			this.m_tabAutoType.Controls.Add(this.m_rbAutoTypeOverride);
			this.m_tabAutoType.Controls.Add(this.m_rbAutoTypeInherit);
			this.m_tabAutoType.Controls.Add(this.m_lblAutoTypeDesc);
			this.m_tabAutoType.Controls.Add(this.m_tbDefaultAutoTypeSeq);
			resources.ApplyResources(this.m_tabAutoType, "m_tabAutoType");
			this.m_tabAutoType.Name = "m_tabAutoType";
			this.m_tabAutoType.UseVisualStyleBackColor = true;
			// 
			// m_btnAutoTypeEdit
			// 
			this.m_btnAutoTypeEdit.Image = global::KeePass.Properties.Resources.B16x16_Wizard;
			resources.ApplyResources(this.m_btnAutoTypeEdit, "m_btnAutoTypeEdit");
			this.m_btnAutoTypeEdit.Name = "m_btnAutoTypeEdit";
			this.m_btnAutoTypeEdit.UseVisualStyleBackColor = true;
			this.m_btnAutoTypeEdit.Click += new System.EventHandler(this.OnBtnAutoTypeEdit);
			// 
			// m_rbAutoTypeOverride
			// 
			resources.ApplyResources(this.m_rbAutoTypeOverride, "m_rbAutoTypeOverride");
			this.m_rbAutoTypeOverride.Name = "m_rbAutoTypeOverride";
			this.m_rbAutoTypeOverride.TabStop = true;
			this.m_rbAutoTypeOverride.UseVisualStyleBackColor = true;
			// 
			// m_rbAutoTypeInherit
			// 
			resources.ApplyResources(this.m_rbAutoTypeInherit, "m_rbAutoTypeInherit");
			this.m_rbAutoTypeInherit.Name = "m_rbAutoTypeInherit";
			this.m_rbAutoTypeInherit.TabStop = true;
			this.m_rbAutoTypeInherit.UseVisualStyleBackColor = true;
			this.m_rbAutoTypeInherit.CheckedChanged += new System.EventHandler(this.OnAutoTypeInheritCheckedChanged);
			// 
			// m_lblAutoTypeDesc
			// 
			resources.ApplyResources(this.m_lblAutoTypeDesc, "m_lblAutoTypeDesc");
			this.m_lblAutoTypeDesc.Name = "m_lblAutoTypeDesc";
			// 
			// m_tbDefaultAutoTypeSeq
			// 
			resources.ApplyResources(this.m_tbDefaultAutoTypeSeq, "m_tbDefaultAutoTypeSeq");
			this.m_tbDefaultAutoTypeSeq.Name = "m_tbDefaultAutoTypeSeq";
			// 
			// GroupForm
			// 
			this.AcceptButton = this.m_btnOK;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.Controls.Add(this.m_tabMain);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOK);
			this.Controls.Add(this.m_bannerImage);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "GroupForm";
			this.ShowInTaskbar = false;
			this.Load += new System.EventHandler(this.OnFormLoad);
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
			this.m_tabMain.ResumeLayout(false);
			this.m_tabGeneral.ResumeLayout(false);
			this.m_tabGeneral.PerformLayout();
			this.m_tabAutoType.ResumeLayout(false);
			this.m_tabAutoType.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label m_lblName;
		private System.Windows.Forms.PictureBox m_bannerImage;
		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.TextBox m_tbName;
		private System.Windows.Forms.Label m_lblIcon;
		private System.Windows.Forms.Button m_btnIcon;
		private System.Windows.Forms.TabControl m_tabMain;
		private System.Windows.Forms.TabPage m_tabGeneral;
		private System.Windows.Forms.TabPage m_tabAutoType;
		private System.Windows.Forms.Label m_lblAutoTypeDesc;
		private System.Windows.Forms.TextBox m_tbDefaultAutoTypeSeq;
		private System.Windows.Forms.CheckBox m_cbExpires;
		private System.Windows.Forms.DateTimePicker m_dtExpires;
		private System.Windows.Forms.RadioButton m_rbAutoTypeInherit;
		private System.Windows.Forms.RadioButton m_rbAutoTypeOverride;
		private System.Windows.Forms.Button m_btnAutoTypeEdit;
	}
}