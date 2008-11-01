namespace KeePass.Forms
{
	partial class ImportCsvForm
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
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_lblFileEncoding = new System.Windows.Forms.Label();
			this.m_cmbEncoding = new System.Windows.Forms.ComboBox();
			this.m_lvPreview = new System.Windows.Forms.ListView();
			this.m_tbSourcePreview = new System.Windows.Forms.TextBox();
			this.m_lblSourcePreview = new System.Windows.Forms.Label();
			this.m_lvHeaderOrder = new System.Windows.Forms.ListView();
			this.m_lblOrder = new System.Windows.Forms.Label();
			this.m_lblOrderHint = new System.Windows.Forms.Label();
			this.m_lblPreview = new System.Windows.Forms.Label();
			this.m_lblDelimiter = new System.Windows.Forms.Label();
			this.m_tbSepChar = new System.Windows.Forms.TextBox();
			this.m_cbDoubleQuoteToSingle = new System.Windows.Forms.CheckBox();
			this.m_pbRender = new System.Windows.Forms.ProgressBar();
			this.m_btnPreviewRefresh = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// m_btnOK
			// 
			this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_btnOK.Location = new System.Drawing.Point(702, 450);
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.Size = new System.Drawing.Size(75, 23);
			this.m_btnOK.TabIndex = 0;
			this.m_btnOK.Text = "&Import";
			this.m_btnOK.UseVisualStyleBackColor = true;
			this.m_btnOK.Click += new System.EventHandler(this.OnBtnOK);
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_btnCancel.Location = new System.Drawing.Point(702, 479);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 1;
			this.m_btnCancel.Text = "&Cancel";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			this.m_btnCancel.Click += new System.EventHandler(this.OnBtnCancel);
			// 
			// m_lblFileEncoding
			// 
			this.m_lblFileEncoding.AutoSize = true;
			this.m_lblFileEncoding.Location = new System.Drawing.Point(9, 15);
			this.m_lblFileEncoding.Name = "m_lblFileEncoding";
			this.m_lblFileEncoding.Size = new System.Drawing.Size(73, 13);
			this.m_lblFileEncoding.TabIndex = 2;
			this.m_lblFileEncoding.Text = "File encoding:";
			// 
			// m_cmbEncoding
			// 
			this.m_cmbEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbEncoding.FormattingEnabled = true;
			this.m_cmbEncoding.Location = new System.Drawing.Point(105, 12);
			this.m_cmbEncoding.Name = "m_cmbEncoding";
			this.m_cmbEncoding.Size = new System.Drawing.Size(228, 21);
			this.m_cmbEncoding.TabIndex = 3;
			this.m_cmbEncoding.SelectedIndexChanged += new System.EventHandler(this.OnCmbEncodingSelectedIndexChanged);
			// 
			// m_lvPreview
			// 
			this.m_lvPreview.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.m_lvPreview.Location = new System.Drawing.Point(12, 262);
			this.m_lvPreview.Name = "m_lvPreview";
			this.m_lvPreview.ShowItemToolTips = true;
			this.m_lvPreview.Size = new System.Drawing.Size(684, 240);
			this.m_lvPreview.TabIndex = 14;
			this.m_lvPreview.UseCompatibleStateImageBehavior = false;
			this.m_lvPreview.View = System.Windows.Forms.View.Details;
			// 
			// m_tbSourcePreview
			// 
			this.m_tbSourcePreview.AcceptsReturn = true;
			this.m_tbSourcePreview.Location = new System.Drawing.Point(105, 39);
			this.m_tbSourcePreview.Multiline = true;
			this.m_tbSourcePreview.Name = "m_tbSourcePreview";
			this.m_tbSourcePreview.ReadOnly = true;
			this.m_tbSourcePreview.Size = new System.Drawing.Size(672, 90);
			this.m_tbSourcePreview.TabIndex = 5;
			// 
			// m_lblSourcePreview
			// 
			this.m_lblSourcePreview.AutoSize = true;
			this.m_lblSourcePreview.Location = new System.Drawing.Point(9, 42);
			this.m_lblSourcePreview.Name = "m_lblSourcePreview";
			this.m_lblSourcePreview.Size = new System.Drawing.Size(68, 13);
			this.m_lblSourcePreview.TabIndex = 4;
			this.m_lblSourcePreview.Text = "Source data:";
			// 
			// m_lvHeaderOrder
			// 
			this.m_lvHeaderOrder.AllowColumnReorder = true;
			this.m_lvHeaderOrder.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.m_lvHeaderOrder.Location = new System.Drawing.Point(105, 135);
			this.m_lvHeaderOrder.Name = "m_lvHeaderOrder";
			this.m_lvHeaderOrder.Size = new System.Drawing.Size(672, 53);
			this.m_lvHeaderOrder.TabIndex = 7;
			this.m_lvHeaderOrder.UseCompatibleStateImageBehavior = false;
			this.m_lvHeaderOrder.View = System.Windows.Forms.View.Details;
			// 
			// m_lblOrder
			// 
			this.m_lblOrder.AutoSize = true;
			this.m_lblOrder.Location = new System.Drawing.Point(9, 141);
			this.m_lblOrder.Name = "m_lblOrder";
			this.m_lblOrder.Size = new System.Drawing.Size(90, 13);
			this.m_lblOrder.TabIndex = 6;
			this.m_lblOrder.Text = "Define field order:";
			// 
			// m_lblOrderHint
			// 
			this.m_lblOrderHint.AutoSize = true;
			this.m_lblOrderHint.Location = new System.Drawing.Point(102, 191);
			this.m_lblOrderHint.Name = "m_lblOrderHint";
			this.m_lblOrderHint.Size = new System.Drawing.Size(499, 13);
			this.m_lblOrderHint.TabIndex = 8;
			this.m_lblOrderHint.Text = "Drag&&drop the columns to define the order of the fields in the CSV file. \"(Ignor" +
				"e)\" columns will be ignored.";
			// 
			// m_lblPreview
			// 
			this.m_lblPreview.AutoSize = true;
			this.m_lblPreview.Location = new System.Drawing.Point(9, 246);
			this.m_lblPreview.Name = "m_lblPreview";
			this.m_lblPreview.Size = new System.Drawing.Size(125, 13);
			this.m_lblPreview.TabIndex = 12;
			this.m_lblPreview.Text = "Imported entries preview:";
			// 
			// m_lblDelimiter
			// 
			this.m_lblDelimiter.AutoSize = true;
			this.m_lblDelimiter.Location = new System.Drawing.Point(9, 216);
			this.m_lblDelimiter.Name = "m_lblDelimiter";
			this.m_lblDelimiter.Size = new System.Drawing.Size(73, 13);
			this.m_lblDelimiter.TabIndex = 9;
			this.m_lblDelimiter.Text = "Field delimiter:";
			// 
			// m_tbSepChar
			// 
			this.m_tbSepChar.Location = new System.Drawing.Point(105, 213);
			this.m_tbSepChar.Name = "m_tbSepChar";
			this.m_tbSepChar.Size = new System.Drawing.Size(35, 20);
			this.m_tbSepChar.TabIndex = 10;
			this.m_tbSepChar.TextChanged += new System.EventHandler(this.OnDelimiterTextChanged);
			// 
			// m_cbDoubleQuoteToSingle
			// 
			this.m_cbDoubleQuoteToSingle.AutoSize = true;
			this.m_cbDoubleQuoteToSingle.Location = new System.Drawing.Point(165, 215);
			this.m_cbDoubleQuoteToSingle.Name = "m_cbDoubleQuoteToSingle";
			this.m_cbDoubleQuoteToSingle.Size = new System.Drawing.Size(238, 17);
			this.m_cbDoubleQuoteToSingle.TabIndex = 11;
			this.m_cbDoubleQuoteToSingle.Text = "Replace double quote (\"\") by single quote (\")";
			this.m_cbDoubleQuoteToSingle.UseVisualStyleBackColor = true;
			this.m_cbDoubleQuoteToSingle.CheckedChanged += new System.EventHandler(this.OnDoubleQuoteCheckedChanged);
			// 
			// m_pbRender
			// 
			this.m_pbRender.Location = new System.Drawing.Point(142, 246);
			this.m_pbRender.Name = "m_pbRender";
			this.m_pbRender.Size = new System.Drawing.Size(554, 13);
			this.m_pbRender.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			this.m_pbRender.TabIndex = 13;
			// 
			// m_btnPreviewRefresh
			// 
			this.m_btnPreviewRefresh.Location = new System.Drawing.Point(702, 262);
			this.m_btnPreviewRefresh.Name = "m_btnPreviewRefresh";
			this.m_btnPreviewRefresh.Size = new System.Drawing.Size(75, 23);
			this.m_btnPreviewRefresh.TabIndex = 15;
			this.m_btnPreviewRefresh.Text = "&Refresh";
			this.m_btnPreviewRefresh.UseVisualStyleBackColor = true;
			this.m_btnPreviewRefresh.Click += new System.EventHandler(this.OnBtnPreviewRefresh);
			// 
			// ImportCsvForm
			// 
			this.AcceptButton = this.m_btnOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(789, 514);
			this.Controls.Add(this.m_btnPreviewRefresh);
			this.Controls.Add(this.m_pbRender);
			this.Controls.Add(this.m_cbDoubleQuoteToSingle);
			this.Controls.Add(this.m_tbSepChar);
			this.Controls.Add(this.m_lblDelimiter);
			this.Controls.Add(this.m_lblPreview);
			this.Controls.Add(this.m_lblOrderHint);
			this.Controls.Add(this.m_lblOrder);
			this.Controls.Add(this.m_lvHeaderOrder);
			this.Controls.Add(this.m_lblSourcePreview);
			this.Controls.Add(this.m_tbSourcePreview);
			this.Controls.Add(this.m_lvPreview);
			this.Controls.Add(this.m_cmbEncoding);
			this.Controls.Add(this.m_lblFileEncoding);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_btnOK);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ImportCsvForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Generic CSV Importer";
			this.Load += new System.EventHandler(this.OnFormLoad);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.Label m_lblFileEncoding;
		private System.Windows.Forms.ComboBox m_cmbEncoding;
		private System.Windows.Forms.ListView m_lvPreview;
		private System.Windows.Forms.TextBox m_tbSourcePreview;
		private System.Windows.Forms.Label m_lblSourcePreview;
		private System.Windows.Forms.ListView m_lvHeaderOrder;
		private System.Windows.Forms.Label m_lblOrder;
		private System.Windows.Forms.Label m_lblOrderHint;
		private System.Windows.Forms.Label m_lblPreview;
		private System.Windows.Forms.Label m_lblDelimiter;
		private System.Windows.Forms.TextBox m_tbSepChar;
		private System.Windows.Forms.CheckBox m_cbDoubleQuoteToSingle;
		private System.Windows.Forms.ProgressBar m_pbRender;
		private System.Windows.Forms.Button m_btnPreviewRefresh;
	}
}