namespace KeePass.Forms
{
	partial class IconPickerForm
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
			this.m_btnOK = new System.Windows.Forms.Button();
			this.m_radioStandard = new System.Windows.Forms.RadioButton();
			this.m_radioCustom = new System.Windows.Forms.RadioButton();
			this.m_lblSeparator = new System.Windows.Forms.Label();
			this.m_btnCustomAdd = new System.Windows.Forms.Button();
			this.m_btnCustomDelete = new System.Windows.Forms.Button();
			this.m_btnCustomExport = new System.Windows.Forms.Button();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_lblFind = new System.Windows.Forms.Label();
			this.m_tbFind = new System.Windows.Forms.TextBox();
			this.m_lvCustomIcons = new KeePass.UI.CustomListViewEx();
			this.m_lvIcons = new KeePass.UI.CustomListViewEx();
			this.SuspendLayout();
			// 
			// m_btnOK
			// 
			this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_btnOK.Location = new System.Drawing.Point(338, 437);
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.Size = new System.Drawing.Size(75, 23);
			this.m_btnOK.TabIndex = 0;
			this.m_btnOK.Text = "OK";
			this.m_btnOK.UseVisualStyleBackColor = true;
			this.m_btnOK.Click += new System.EventHandler(this.OnBtnOK);
			// 
			// m_radioStandard
			// 
			this.m_radioStandard.AutoSize = true;
			this.m_radioStandard.Location = new System.Drawing.Point(12, 11);
			this.m_radioStandard.Name = "m_radioStandard";
			this.m_radioStandard.Size = new System.Drawing.Size(114, 17);
			this.m_radioStandard.TabIndex = 2;
			this.m_radioStandard.TabStop = true;
			this.m_radioStandard.Text = "Use &standard icon:";
			this.m_radioStandard.UseVisualStyleBackColor = true;
			this.m_radioStandard.CheckedChanged += new System.EventHandler(this.OnStandardRadioCheckedChanged);
			// 
			// m_radioCustom
			// 
			this.m_radioCustom.AutoSize = true;
			this.m_radioCustom.Location = new System.Drawing.Point(12, 241);
			this.m_radioCustom.Name = "m_radioCustom";
			this.m_radioCustom.Size = new System.Drawing.Size(257, 17);
			this.m_radioCustom.TabIndex = 4;
			this.m_radioCustom.TabStop = true;
			this.m_radioCustom.Text = "Use &custom icon (stored in the current database):";
			this.m_radioCustom.UseVisualStyleBackColor = true;
			// 
			// m_lblSeparator
			// 
			this.m_lblSeparator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.m_lblSeparator.Location = new System.Drawing.Point(-1, 430);
			this.m_lblSeparator.Name = "m_lblSeparator";
			this.m_lblSeparator.Size = new System.Drawing.Size(507, 2);
			this.m_lblSeparator.TabIndex = 11;
			// 
			// m_btnCustomAdd
			// 
			this.m_btnCustomAdd.Location = new System.Drawing.Point(31, 398);
			this.m_btnCustomAdd.Name = "m_btnCustomAdd";
			this.m_btnCustomAdd.Size = new System.Drawing.Size(75, 23);
			this.m_btnCustomAdd.TabIndex = 6;
			this.m_btnCustomAdd.Text = "&Add...";
			this.m_btnCustomAdd.UseVisualStyleBackColor = true;
			this.m_btnCustomAdd.Click += new System.EventHandler(this.OnBtnCustomAdd);
			// 
			// m_btnCustomDelete
			// 
			this.m_btnCustomDelete.Location = new System.Drawing.Point(112, 398);
			this.m_btnCustomDelete.Name = "m_btnCustomDelete";
			this.m_btnCustomDelete.Size = new System.Drawing.Size(75, 23);
			this.m_btnCustomDelete.TabIndex = 7;
			this.m_btnCustomDelete.Text = "&Delete";
			this.m_btnCustomDelete.UseVisualStyleBackColor = true;
			this.m_btnCustomDelete.Click += new System.EventHandler(this.OnBtnCustomRemove);
			// 
			// m_btnCustomExport
			// 
			this.m_btnCustomExport.Location = new System.Drawing.Point(193, 398);
			this.m_btnCustomExport.Name = "m_btnCustomExport";
			this.m_btnCustomExport.Size = new System.Drawing.Size(75, 23);
			this.m_btnCustomExport.TabIndex = 8;
			this.m_btnCustomExport.Text = "&Export...";
			this.m_btnCustomExport.UseVisualStyleBackColor = true;
			this.m_btnCustomExport.Click += new System.EventHandler(this.OnBtnCustomSave);
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_btnCancel.Location = new System.Drawing.Point(419, 437);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 1;
			this.m_btnCancel.Text = "Cancel";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			// 
			// m_lblFind
			// 
			this.m_lblFind.AutoSize = true;
			this.m_lblFind.Location = new System.Drawing.Point(303, 403);
			this.m_lblFind.Name = "m_lblFind";
			this.m_lblFind.Size = new System.Drawing.Size(30, 13);
			this.m_lblFind.TabIndex = 9;
			this.m_lblFind.Text = "&Find:";
			// 
			// m_tbFind
			// 
			this.m_tbFind.Location = new System.Drawing.Point(339, 400);
			this.m_tbFind.Name = "m_tbFind";
			this.m_tbFind.Size = new System.Drawing.Size(154, 20);
			this.m_tbFind.TabIndex = 10;
			// 
			// m_lvCustomIcons
			// 
			this.m_lvCustomIcons.HideSelection = false;
			this.m_lvCustomIcons.LabelEdit = true;
			this.m_lvCustomIcons.Location = new System.Drawing.Point(32, 264);
			this.m_lvCustomIcons.Name = "m_lvCustomIcons";
			this.m_lvCustomIcons.ShowItemToolTips = true;
			this.m_lvCustomIcons.Size = new System.Drawing.Size(461, 128);
			this.m_lvCustomIcons.TabIndex = 5;
			this.m_lvCustomIcons.UseCompatibleStateImageBehavior = false;
			this.m_lvCustomIcons.View = System.Windows.Forms.View.List;
			this.m_lvCustomIcons.ItemActivate += new System.EventHandler(this.OnCustomIconsItemActivate);
			this.m_lvCustomIcons.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.OnCustomIconsAfterLabelEdit);
			this.m_lvCustomIcons.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.OnCustomIconsItemSelectionChanged);
			this.m_lvCustomIcons.BeforeLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.OnCustomIconsBeforeLabelEdit);
			// 
			// m_lvIcons
			// 
			this.m_lvIcons.HideSelection = false;
			this.m_lvIcons.Location = new System.Drawing.Point(32, 34);
			this.m_lvIcons.MultiSelect = false;
			this.m_lvIcons.Name = "m_lvIcons";
			this.m_lvIcons.Size = new System.Drawing.Size(461, 196);
			this.m_lvIcons.TabIndex = 3;
			this.m_lvIcons.UseCompatibleStateImageBehavior = false;
			this.m_lvIcons.View = System.Windows.Forms.View.List;
			this.m_lvIcons.ItemActivate += new System.EventHandler(this.OnIconsItemActivate);
			this.m_lvIcons.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.OnIconsItemSelectionChanged);
			// 
			// IconPickerForm
			// 
			this.AcceptButton = this.m_btnOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(506, 472);
			this.Controls.Add(this.m_tbFind);
			this.Controls.Add(this.m_lblFind);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnCustomExport);
			this.Controls.Add(this.m_btnCustomDelete);
			this.Controls.Add(this.m_btnCustomAdd);
			this.Controls.Add(this.m_lblSeparator);
			this.Controls.Add(this.m_radioCustom);
			this.Controls.Add(this.m_radioStandard);
			this.Controls.Add(this.m_lvCustomIcons);
			this.Controls.Add(this.m_btnOK);
			this.Controls.Add(this.m_lvIcons);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "IconPickerForm";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Icon Picker";
			this.Load += new System.EventHandler(this.OnFormLoad);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private KeePass.UI.CustomListViewEx m_lvIcons;
		private System.Windows.Forms.Button m_btnOK;
		private KeePass.UI.CustomListViewEx m_lvCustomIcons;
		private System.Windows.Forms.RadioButton m_radioStandard;
		private System.Windows.Forms.RadioButton m_radioCustom;
		private System.Windows.Forms.Label m_lblSeparator;
		private System.Windows.Forms.Button m_btnCustomAdd;
		private System.Windows.Forms.Button m_btnCustomDelete;
		private System.Windows.Forms.Button m_btnCustomExport;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.Label m_lblFind;
		private System.Windows.Forms.TextBox m_tbFind;
	}
}