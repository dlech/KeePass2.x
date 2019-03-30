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
            this.m_tabNotes = new System.Windows.Forms.TabPage();
            this.m_lblNotesHint = new System.Windows.Forms.Label();
            this.m_tbNotes = new System.Windows.Forms.TextBox();
            this.m_tabBehavior = new System.Windows.Forms.TabPage();
            this.m_cmbEnableSearching = new System.Windows.Forms.ComboBox();
            this.m_cmbEnableAutoType = new System.Windows.Forms.ComboBox();
            this.m_lblEnableSearching = new System.Windows.Forms.Label();
            this.m_lblEnableAutoType = new System.Windows.Forms.Label();
            this.m_tabAutoType = new System.Windows.Forms.TabPage();
            this.m_btnAutoTypeEdit = new System.Windows.Forms.Button();
            this.m_rbAutoTypeOverride = new System.Windows.Forms.RadioButton();
            this.m_rbAutoTypeInherit = new System.Windows.Forms.RadioButton();
            this.m_lblAutoTypeDesc = new System.Windows.Forms.Label();
            this.m_tbDefaultAutoTypeSeq = new System.Windows.Forms.TextBox();
            this.m_tabCustomData = new System.Windows.Forms.TabPage();
            this.m_btnCDDel = new System.Windows.Forms.Button();
            this.m_lvCustomData = new KeePass.UI.CustomListViewEx();
            ((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
            this.m_tabMain.SuspendLayout();
            this.m_tabGeneral.SuspendLayout();
            this.m_tabNotes.SuspendLayout();
            this.m_tabBehavior.SuspendLayout();
            this.m_tabAutoType.SuspendLayout();
            this.m_tabCustomData.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_lblName
            // 
            this.m_lblName.AutoSize = true;
            this.m_lblName.Location = new System.Drawing.Point(4, 18);
            this.m_lblName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblName.Name = "m_lblName";
            this.m_lblName.Size = new System.Drawing.Size(52, 20);
            this.m_lblName.TabIndex = 1;
            this.m_lblName.Text = "Name:";
            // 
            // m_bannerImage
            // 
            this.m_bannerImage.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_bannerImage.Location = new System.Drawing.Point(0, 0);
            this.m_bannerImage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_bannerImage.Name = "m_bannerImage";
            this.m_bannerImage.Size = new System.Drawing.Size(517, 92);
            this.m_bannerImage.TabIndex = 1;
            this.m_bannerImage.TabStop = false;
            // 
            // m_btnOK
            // 
            this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_btnOK.Location = new System.Drawing.Point(293, 366);
            this.m_btnOK.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnOK.Name = "m_btnOK";
            this.m_btnOK.Size = new System.Drawing.Size(100, 35);
            this.m_btnOK.TabIndex = 1;
            this.m_btnOK.Text = "OK";
            this.m_btnOK.UseVisualStyleBackColor = true;
            this.m_btnOK.Click += new System.EventHandler(this.OnBtnOK);
            // 
            // m_btnCancel
            // 
            this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_btnCancel.Location = new System.Drawing.Point(401, 366);
            this.m_btnCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnCancel.Name = "m_btnCancel";
            this.m_btnCancel.Size = new System.Drawing.Size(100, 35);
            this.m_btnCancel.TabIndex = 2;
            this.m_btnCancel.Text = "Cancel";
            this.m_btnCancel.UseVisualStyleBackColor = true;
            this.m_btnCancel.Click += new System.EventHandler(this.OnBtnCancel);
            // 
            // m_tbName
            // 
            this.m_tbName.Location = new System.Drawing.Point(80, 18);
            this.m_tbName.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_tbName.Name = "m_tbName";
            this.m_tbName.Size = new System.Drawing.Size(380, 27);
            this.m_tbName.TabIndex = 0;
            // 
            // m_lblIcon
            // 
            this.m_lblIcon.AutoSize = true;
            this.m_lblIcon.Location = new System.Drawing.Point(4, 60);
            this.m_lblIcon.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblIcon.Name = "m_lblIcon";
            this.m_lblIcon.Size = new System.Drawing.Size(40, 20);
            this.m_lblIcon.TabIndex = 2;
            this.m_lblIcon.Text = "Icon:";
            // 
            // m_btnIcon
            // 
            this.m_btnIcon.Location = new System.Drawing.Point(80, 58);
            this.m_btnIcon.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnIcon.Name = "m_btnIcon";
            this.m_btnIcon.Size = new System.Drawing.Size(44, 35);
            this.m_btnIcon.TabIndex = 3;
            this.m_btnIcon.UseVisualStyleBackColor = true;
            this.m_btnIcon.Click += new System.EventHandler(this.OnBtnIcon);
            // 
            // m_tabMain
            // 
            this.m_tabMain.Controls.Add(this.m_tabGeneral);
            this.m_tabMain.Controls.Add(this.m_tabNotes);
            this.m_tabMain.Controls.Add(this.m_tabBehavior);
            this.m_tabMain.Controls.Add(this.m_tabAutoType);
            this.m_tabMain.Controls.Add(this.m_tabCustomData);
            this.m_tabMain.Location = new System.Drawing.Point(16, 101);
            this.m_tabMain.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_tabMain.Name = "m_tabMain";
            this.m_tabMain.SelectedIndex = 0;
            this.m_tabMain.Size = new System.Drawing.Size(485, 255);
            this.m_tabMain.TabIndex = 0;
            // 
            // m_tabGeneral
            // 
            this.m_tabGeneral.Controls.Add(this.m_dtExpires);
            this.m_tabGeneral.Controls.Add(this.m_cbExpires);
            this.m_tabGeneral.Controls.Add(this.m_lblName);
            this.m_tabGeneral.Controls.Add(this.m_btnIcon);
            this.m_tabGeneral.Controls.Add(this.m_tbName);
            this.m_tabGeneral.Controls.Add(this.m_lblIcon);
            this.m_tabGeneral.Location = new System.Drawing.Point(4, 29);
            this.m_tabGeneral.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_tabGeneral.Name = "m_tabGeneral";
            this.m_tabGeneral.Size = new System.Drawing.Size(477, 222);
            this.m_tabGeneral.TabIndex = 0;
            this.m_tabGeneral.Text = "General";
            this.m_tabGeneral.UseVisualStyleBackColor = true;
            // 
            // m_dtExpires
            // 
            this.m_dtExpires.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.m_dtExpires.Location = new System.Drawing.Point(116, 161);
            this.m_dtExpires.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_dtExpires.Name = "m_dtExpires";
            this.m_dtExpires.Size = new System.Drawing.Size(344, 27);
            this.m_dtExpires.TabIndex = 5;
            // 
            // m_cbExpires
            // 
            this.m_cbExpires.AutoSize = true;
            this.m_cbExpires.Location = new System.Drawing.Point(8, 161);
            this.m_cbExpires.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cbExpires.Name = "m_cbExpires";
            this.m_cbExpires.Size = new System.Drawing.Size(81, 24);
            this.m_cbExpires.TabIndex = 4;
            this.m_cbExpires.Text = "Expires:";
            this.m_cbExpires.UseVisualStyleBackColor = true;
            // 
            // m_tabNotes
            // 
            this.m_tabNotes.Controls.Add(this.m_lblNotesHint);
            this.m_tabNotes.Controls.Add(this.m_tbNotes);
            this.m_tabNotes.Location = new System.Drawing.Point(4, 25);
            this.m_tabNotes.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_tabNotes.Name = "m_tabNotes";
            this.m_tabNotes.Size = new System.Drawing.Size(477, 226);
            this.m_tabNotes.TabIndex = 2;
            this.m_tabNotes.Text = "Notes";
            this.m_tabNotes.UseVisualStyleBackColor = true;
            // 
            // m_lblNotesHint
            // 
            this.m_lblNotesHint.AutoSize = true;
            this.m_lblNotesHint.Location = new System.Drawing.Point(4, 186);
            this.m_lblNotesHint.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblNotesHint.Name = "m_lblNotesHint";
            this.m_lblNotesHint.Size = new System.Drawing.Size(237, 20);
            this.m_lblNotesHint.TabIndex = 1;
            this.m_lblNotesHint.Text = "Notes are shown in group tooltips.";
            // 
            // m_tbNotes
            // 
            this.m_tbNotes.AcceptsReturn = true;
            this.m_tbNotes.Location = new System.Drawing.Point(8, 15);
            this.m_tbNotes.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_tbNotes.Multiline = true;
            this.m_tbNotes.Name = "m_tbNotes";
            this.m_tbNotes.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.m_tbNotes.Size = new System.Drawing.Size(453, 163);
            this.m_tbNotes.TabIndex = 0;
            // 
            // m_tabBehavior
            // 
            this.m_tabBehavior.Controls.Add(this.m_cmbEnableSearching);
            this.m_tabBehavior.Controls.Add(this.m_cmbEnableAutoType);
            this.m_tabBehavior.Controls.Add(this.m_lblEnableSearching);
            this.m_tabBehavior.Controls.Add(this.m_lblEnableAutoType);
            this.m_tabBehavior.Location = new System.Drawing.Point(4, 25);
            this.m_tabBehavior.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_tabBehavior.Name = "m_tabBehavior";
            this.m_tabBehavior.Size = new System.Drawing.Size(477, 226);
            this.m_tabBehavior.TabIndex = 3;
            this.m_tabBehavior.Text = "Behavior";
            this.m_tabBehavior.UseVisualStyleBackColor = true;
            // 
            // m_cmbEnableSearching
            // 
            this.m_cmbEnableSearching.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_cmbEnableSearching.FormattingEnabled = true;
            this.m_cmbEnableSearching.Location = new System.Drawing.Point(12, 114);
            this.m_cmbEnableSearching.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cmbEnableSearching.Name = "m_cmbEnableSearching";
            this.m_cmbEnableSearching.Size = new System.Drawing.Size(444, 28);
            this.m_cmbEnableSearching.TabIndex = 3;
            // 
            // m_cmbEnableAutoType
            // 
            this.m_cmbEnableAutoType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_cmbEnableAutoType.FormattingEnabled = true;
            this.m_cmbEnableAutoType.Location = new System.Drawing.Point(12, 39);
            this.m_cmbEnableAutoType.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cmbEnableAutoType.Name = "m_cmbEnableAutoType";
            this.m_cmbEnableAutoType.Size = new System.Drawing.Size(444, 28);
            this.m_cmbEnableAutoType.TabIndex = 1;
            // 
            // m_lblEnableSearching
            // 
            this.m_lblEnableSearching.AutoSize = true;
            this.m_lblEnableSearching.Location = new System.Drawing.Point(8, 89);
            this.m_lblEnableSearching.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblEnableSearching.Name = "m_lblEnableSearching";
            this.m_lblEnableSearching.Size = new System.Drawing.Size(212, 20);
            this.m_lblEnableSearching.TabIndex = 2;
            this.m_lblEnableSearching.Text = "Searching entries in this group:";
            // 
            // m_lblEnableAutoType
            // 
            this.m_lblEnableAutoType.AutoSize = true;
            this.m_lblEnableAutoType.Location = new System.Drawing.Point(8, 14);
            this.m_lblEnableAutoType.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblEnableAutoType.Name = "m_lblEnableAutoType";
            this.m_lblEnableAutoType.Size = new System.Drawing.Size(239, 20);
            this.m_lblEnableAutoType.TabIndex = 0;
            this.m_lblEnableAutoType.Text = "Auto-Type for entries in this group:";
            // 
            // m_tabAutoType
            // 
            this.m_tabAutoType.Controls.Add(this.m_btnAutoTypeEdit);
            this.m_tabAutoType.Controls.Add(this.m_rbAutoTypeOverride);
            this.m_tabAutoType.Controls.Add(this.m_rbAutoTypeInherit);
            this.m_tabAutoType.Controls.Add(this.m_lblAutoTypeDesc);
            this.m_tabAutoType.Controls.Add(this.m_tbDefaultAutoTypeSeq);
            this.m_tabAutoType.Location = new System.Drawing.Point(4, 25);
            this.m_tabAutoType.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_tabAutoType.Name = "m_tabAutoType";
            this.m_tabAutoType.Size = new System.Drawing.Size(477, 226);
            this.m_tabAutoType.TabIndex = 1;
            this.m_tabAutoType.Text = "Auto-Type";
            this.m_tabAutoType.UseVisualStyleBackColor = true;
            // 
            // m_btnAutoTypeEdit
            // 
            this.m_btnAutoTypeEdit.Location = new System.Drawing.Point(420, 82);
            this.m_btnAutoTypeEdit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnAutoTypeEdit.Name = "m_btnAutoTypeEdit";
            this.m_btnAutoTypeEdit.Size = new System.Drawing.Size(43, 35);
            this.m_btnAutoTypeEdit.TabIndex = 3;
            this.m_btnAutoTypeEdit.UseVisualStyleBackColor = true;
            this.m_btnAutoTypeEdit.Click += new System.EventHandler(this.OnBtnAutoTypeEdit);
            // 
            // m_rbAutoTypeOverride
            // 
            this.m_rbAutoTypeOverride.AutoSize = true;
            this.m_rbAutoTypeOverride.Location = new System.Drawing.Point(12, 51);
            this.m_rbAutoTypeOverride.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_rbAutoTypeOverride.Name = "m_rbAutoTypeOverride";
            this.m_rbAutoTypeOverride.Size = new System.Drawing.Size(207, 24);
            this.m_rbAutoTypeOverride.TabIndex = 1;
            this.m_rbAutoTypeOverride.TabStop = true;
            this.m_rbAutoTypeOverride.Text = "Override default sequence:";
            this.m_rbAutoTypeOverride.UseVisualStyleBackColor = true;
            // 
            // m_rbAutoTypeInherit
            // 
            this.m_rbAutoTypeInherit.AutoSize = true;
            this.m_rbAutoTypeInherit.Location = new System.Drawing.Point(12, 15);
            this.m_rbAutoTypeInherit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_rbAutoTypeInherit.Name = "m_rbAutoTypeInherit";
            this.m_rbAutoTypeInherit.Size = new System.Drawing.Size(385, 24);
            this.m_rbAutoTypeInherit.TabIndex = 0;
            this.m_rbAutoTypeInherit.TabStop = true;
            this.m_rbAutoTypeInherit.Text = "Inherit default auto-type sequence from parent group";
            this.m_rbAutoTypeInherit.UseVisualStyleBackColor = true;
            this.m_rbAutoTypeInherit.CheckedChanged += new System.EventHandler(this.OnAutoTypeInheritCheckedChanged);
            // 
            // m_lblAutoTypeDesc
            // 
            this.m_lblAutoTypeDesc.Location = new System.Drawing.Point(35, 121);
            this.m_lblAutoTypeDesc.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblAutoTypeDesc.Name = "m_lblAutoTypeDesc";
            this.m_lblAutoTypeDesc.Size = new System.Drawing.Size(428, 41);
            this.m_lblAutoTypeDesc.TabIndex = 4;
            this.m_lblAutoTypeDesc.Text = "All subgroups and entries in the current group that inherit the group\'s auto-type" +
    " sequence will use the one entered above.";
            // 
            // m_tbDefaultAutoTypeSeq
            // 
            this.m_tbDefaultAutoTypeSeq.Location = new System.Drawing.Point(39, 86);
            this.m_tbDefaultAutoTypeSeq.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_tbDefaultAutoTypeSeq.Name = "m_tbDefaultAutoTypeSeq";
            this.m_tbDefaultAutoTypeSeq.Size = new System.Drawing.Size(372, 27);
            this.m_tbDefaultAutoTypeSeq.TabIndex = 2;
            // 
            // m_tabCustomData
            // 
            this.m_tabCustomData.Controls.Add(this.m_btnCDDel);
            this.m_tabCustomData.Controls.Add(this.m_lvCustomData);
            this.m_tabCustomData.Location = new System.Drawing.Point(4, 25);
            this.m_tabCustomData.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_tabCustomData.Name = "m_tabCustomData";
            this.m_tabCustomData.Size = new System.Drawing.Size(477, 226);
            this.m_tabCustomData.TabIndex = 4;
            this.m_tabCustomData.Text = "Plugin Data";
            this.m_tabCustomData.UseVisualStyleBackColor = true;
            // 
            // m_btnCDDel
            // 
            this.m_btnCDDel.Location = new System.Drawing.Point(364, 168);
            this.m_btnCDDel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnCDDel.Name = "m_btnCDDel";
            this.m_btnCDDel.Size = new System.Drawing.Size(100, 35);
            this.m_btnCDDel.TabIndex = 1;
            this.m_btnCDDel.Text = "&Delete";
            this.m_btnCDDel.UseVisualStyleBackColor = true;
            this.m_btnCDDel.Click += new System.EventHandler(this.OnBtnCDDel);
            // 
            // m_lvCustomData
            // 
            this.m_lvCustomData.FullRowSelect = true;
            this.m_lvCustomData.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.m_lvCustomData.HideSelection = false;
            this.m_lvCustomData.Location = new System.Drawing.Point(8, 15);
            this.m_lvCustomData.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_lvCustomData.Name = "m_lvCustomData";
            this.m_lvCustomData.ShowItemToolTips = true;
            this.m_lvCustomData.Size = new System.Drawing.Size(453, 142);
            this.m_lvCustomData.TabIndex = 0;
            this.m_lvCustomData.UseCompatibleStateImageBehavior = false;
            this.m_lvCustomData.View = System.Windows.Forms.View.Details;
            this.m_lvCustomData.SelectedIndexChanged += new System.EventHandler(this.OnCustomDataSelectedIndexChanged);
            // 
            // GroupForm
            // 
            this.AcceptButton = this.m_btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.m_btnCancel;
            this.ClientSize = new System.Drawing.Size(517, 420);
            this.Controls.Add(this.m_tabMain);
            this.Controls.Add(this.m_btnCancel);
            this.Controls.Add(this.m_btnOK);
            this.Controls.Add(this.m_bannerImage);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GroupForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "<DYN>";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
            this.Load += new System.EventHandler(this.OnFormLoad);
            ((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
            this.m_tabMain.ResumeLayout(false);
            this.m_tabGeneral.ResumeLayout(false);
            this.m_tabGeneral.PerformLayout();
            this.m_tabNotes.ResumeLayout(false);
            this.m_tabNotes.PerformLayout();
            this.m_tabBehavior.ResumeLayout(false);
            this.m_tabBehavior.PerformLayout();
            this.m_tabAutoType.ResumeLayout(false);
            this.m_tabAutoType.PerformLayout();
            this.m_tabCustomData.ResumeLayout(false);
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
		private System.Windows.Forms.TabPage m_tabNotes;
		private System.Windows.Forms.TextBox m_tbNotes;
		private System.Windows.Forms.TabPage m_tabBehavior;
		private System.Windows.Forms.Label m_lblEnableSearching;
		private System.Windows.Forms.Label m_lblEnableAutoType;
		private System.Windows.Forms.ComboBox m_cmbEnableSearching;
		private System.Windows.Forms.ComboBox m_cmbEnableAutoType;
		private System.Windows.Forms.Label m_lblNotesHint;
		private System.Windows.Forms.TabPage m_tabCustomData;
		private System.Windows.Forms.Button m_btnCDDel;
		private KeePass.UI.CustomListViewEx m_lvCustomData;
	}
}