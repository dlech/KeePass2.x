﻿namespace KeePass.Forms
{
	partial class EcasConditionForm
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
            this.m_btnCancel = new System.Windows.Forms.Button();
            this.m_btnOK = new System.Windows.Forms.Button();
            this.m_lblCondition = new System.Windows.Forms.Label();
            this.m_cmbConditions = new System.Windows.Forms.ComboBox();
            this.m_cbNegate = new System.Windows.Forms.CheckBox();
            this.m_dgvParams = new System.Windows.Forms.DataGridView();
            this.m_lblParamHint = new System.Windows.Forms.Label();
            this.m_lblSep = new System.Windows.Forms.Label();
            this.m_btnHelp = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.m_dgvParams)).BeginInit();
            this.SuspendLayout();
            // 
            // m_btnCancel
            // 
            this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_btnCancel.Location = new System.Drawing.Point(596, 475);
            this.m_btnCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnCancel.Name = "m_btnCancel";
            this.m_btnCancel.Size = new System.Drawing.Size(100, 35);
            this.m_btnCancel.TabIndex = 6;
            this.m_btnCancel.Text = "Cancel";
            this.m_btnCancel.UseVisualStyleBackColor = true;
            this.m_btnCancel.Click += new System.EventHandler(this.OnBtnCancel);
            // 
            // m_btnOK
            // 
            this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_btnOK.Location = new System.Drawing.Point(488, 475);
            this.m_btnOK.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnOK.Name = "m_btnOK";
            this.m_btnOK.Size = new System.Drawing.Size(100, 35);
            this.m_btnOK.TabIndex = 5;
            this.m_btnOK.Text = "OK";
            this.m_btnOK.UseVisualStyleBackColor = true;
            this.m_btnOK.Click += new System.EventHandler(this.OnBtnOK);
            // 
            // m_lblCondition
            // 
            this.m_lblCondition.AutoSize = true;
            this.m_lblCondition.Location = new System.Drawing.Point(12, 71);
            this.m_lblCondition.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblCondition.Name = "m_lblCondition";
            this.m_lblCondition.Size = new System.Drawing.Size(77, 20);
            this.m_lblCondition.TabIndex = 8;
            this.m_lblCondition.Text = "Condition:";
            // 
            // m_cmbConditions
            // 
            this.m_cmbConditions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_cmbConditions.FormattingEnabled = true;
            this.m_cmbConditions.Location = new System.Drawing.Point(16, 95);
            this.m_cmbConditions.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cmbConditions.Name = "m_cmbConditions";
            this.m_cmbConditions.Size = new System.Drawing.Size(679, 28);
            this.m_cmbConditions.TabIndex = 0;
            this.m_cmbConditions.SelectedIndexChanged += new System.EventHandler(this.OnConditionsSelectedIndexChanged);
            // 
            // m_cbNegate
            // 
            this.m_cbNegate.AutoSize = true;
            this.m_cbNegate.Location = new System.Drawing.Point(16, 22);
            this.m_cbNegate.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cbNegate.Name = "m_cbNegate";
            this.m_cbNegate.Size = new System.Drawing.Size(286, 24);
            this.m_cbNegate.TabIndex = 7;
            this.m_cbNegate.Text = "&Not (negate result of condition below)";
            this.m_cbNegate.UseVisualStyleBackColor = true;
            // 
            // m_dgvParams
            // 
            this.m_dgvParams.AllowUserToAddRows = false;
            this.m_dgvParams.AllowUserToDeleteRows = false;
            this.m_dgvParams.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.m_dgvParams.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.m_dgvParams.Location = new System.Drawing.Point(16, 138);
            this.m_dgvParams.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_dgvParams.Name = "m_dgvParams";
            this.m_dgvParams.Size = new System.Drawing.Size(680, 259);
            this.m_dgvParams.TabIndex = 1;
            // 
            // m_lblParamHint
            // 
            this.m_lblParamHint.Location = new System.Drawing.Point(12, 414);
            this.m_lblParamHint.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblParamHint.Name = "m_lblParamHint";
            this.m_lblParamHint.Size = new System.Drawing.Size(684, 22);
            this.m_lblParamHint.TabIndex = 2;
            this.m_lblParamHint.Text = "<>";
            // 
            // m_lblSep
            // 
            this.m_lblSep.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.m_lblSep.Location = new System.Drawing.Point(0, 461);
            this.m_lblSep.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblSep.Name = "m_lblSep";
            this.m_lblSep.Size = new System.Drawing.Size(713, 2);
            this.m_lblSep.TabIndex = 3;
            // 
            // m_btnHelp
            // 
            this.m_btnHelp.Location = new System.Drawing.Point(16, 475);
            this.m_btnHelp.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnHelp.Name = "m_btnHelp";
            this.m_btnHelp.Size = new System.Drawing.Size(100, 35);
            this.m_btnHelp.TabIndex = 4;
            this.m_btnHelp.Text = "&Help";
            this.m_btnHelp.UseVisualStyleBackColor = true;
            this.m_btnHelp.Click += new System.EventHandler(this.OnBtnHelp);
            // 
            // EcasConditionForm
            // 
            this.AcceptButton = this.m_btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.m_btnCancel;
            this.ClientSize = new System.Drawing.Size(712, 529);
            this.Controls.Add(this.m_btnHelp);
            this.Controls.Add(this.m_lblSep);
            this.Controls.Add(this.m_lblParamHint);
            this.Controls.Add(this.m_dgvParams);
            this.Controls.Add(this.m_cbNegate);
            this.Controls.Add(this.m_btnCancel);
            this.Controls.Add(this.m_btnOK);
            this.Controls.Add(this.m_lblCondition);
            this.Controls.Add(this.m_cmbConditions);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EcasConditionForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "<>";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
            this.Load += new System.EventHandler(this.OnFormLoad);
            ((System.ComponentModel.ISupportInitialize)(this.m_dgvParams)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.Label m_lblCondition;
		private System.Windows.Forms.ComboBox m_cmbConditions;
		private System.Windows.Forms.CheckBox m_cbNegate;
		private System.Windows.Forms.DataGridView m_dgvParams;
		private System.Windows.Forms.Label m_lblParamHint;
		private System.Windows.Forms.Label m_lblSep;
		private System.Windows.Forms.Button m_btnHelp;
	}
}