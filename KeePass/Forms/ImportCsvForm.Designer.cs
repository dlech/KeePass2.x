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
			this.m_chTitle = new System.Windows.Forms.ColumnHeader();
			this.m_chUser = new System.Windows.Forms.ColumnHeader();
			this.m_chPassword = new System.Windows.Forms.ColumnHeader();
			this.m_chURL = new System.Windows.Forms.ColumnHeader();
			this.m_chNotes = new System.Windows.Forms.ColumnHeader();
			this.m_chCustom1 = new System.Windows.Forms.ColumnHeader();
			this.m_chCustom2 = new System.Windows.Forms.ColumnHeader();
			this.m_chCustom3 = new System.Windows.Forms.ColumnHeader();
			this.m_tbSourcePreview = new System.Windows.Forms.TextBox();
			this.m_lblSourcePreview = new System.Windows.Forms.Label();
			this.m_lvHeaderOrder = new System.Windows.Forms.ListView();
			this.m_chOrderTitle = new System.Windows.Forms.ColumnHeader();
			this.m_chOrderUser = new System.Windows.Forms.ColumnHeader();
			this.m_chOrderPassword = new System.Windows.Forms.ColumnHeader();
			this.m_chOrderURL = new System.Windows.Forms.ColumnHeader();
			this.m_chOrderNotes = new System.Windows.Forms.ColumnHeader();
			this.m_chOrderCustom1 = new System.Windows.Forms.ColumnHeader();
			this.m_chOrderCustom2 = new System.Windows.Forms.ColumnHeader();
			this.m_chOrderCustom3 = new System.Windows.Forms.ColumnHeader();
			this.m_chOrderIgnore0 = new System.Windows.Forms.ColumnHeader();
			this.m_chOrderIgnore1 = new System.Windows.Forms.ColumnHeader();
			this.m_chOrderIgnore2 = new System.Windows.Forms.ColumnHeader();
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
			this.m_btnOK.Location = new System.Drawing.Point(702, 421);
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
			this.m_btnCancel.Location = new System.Drawing.Point(702, 450);
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
			this.m_lblFileEncoding.Size = new System.Drawing.Size(74, 13);
			this.m_lblFileEncoding.TabIndex = 2;
			this.m_lblFileEncoding.Text = "File Encoding:";
			// 
			// m_cmbEncoding
			// 
			this.m_cmbEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbEncoding.FormattingEnabled = true;
			this.m_cmbEncoding.Items.AddRange(new object[] {
            "Default ANSI (System)",
            "ASCII",
            "UTF-7",
            "UTF-8",
            "UTF-32",
            "Unicode",
            "Big Endian Unicode"});
			this.m_cmbEncoding.Location = new System.Drawing.Point(113, 12);
			this.m_cmbEncoding.Name = "m_cmbEncoding";
			this.m_cmbEncoding.Size = new System.Drawing.Size(220, 21);
			this.m_cmbEncoding.TabIndex = 3;
			this.m_cmbEncoding.SelectedIndexChanged += new System.EventHandler(this.OnCmbEncodingSelectedIndexChanged);
			// 
			// m_lvPreview
			// 
			this.m_lvPreview.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.m_chTitle,
            this.m_chUser,
            this.m_chPassword,
            this.m_chURL,
            this.m_chNotes,
            this.m_chCustom1,
            this.m_chCustom2,
            this.m_chCustom3});
			this.m_lvPreview.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.m_lvPreview.Location = new System.Drawing.Point(12, 233);
			this.m_lvPreview.Name = "m_lvPreview";
			this.m_lvPreview.ShowItemToolTips = true;
			this.m_lvPreview.Size = new System.Drawing.Size(684, 240);
			this.m_lvPreview.TabIndex = 14;
			this.m_lvPreview.UseCompatibleStateImageBehavior = false;
			this.m_lvPreview.View = System.Windows.Forms.View.Details;
			// 
			// m_chTitle
			// 
			this.m_chTitle.Text = "Title";
			this.m_chTitle.Width = 120;
			// 
			// m_chUser
			// 
			this.m_chUser.Text = "User Name";
			this.m_chUser.Width = 120;
			// 
			// m_chPassword
			// 
			this.m_chPassword.Text = "Password";
			this.m_chPassword.Width = 120;
			// 
			// m_chURL
			// 
			this.m_chURL.Text = "URL";
			this.m_chURL.Width = 120;
			// 
			// m_chNotes
			// 
			this.m_chNotes.Text = "Notes";
			this.m_chNotes.Width = 120;
			// 
			// m_chCustom1
			// 
			this.m_chCustom1.Text = "Custom 1";
			this.m_chCustom1.Width = 120;
			// 
			// m_chCustom2
			// 
			this.m_chCustom2.Text = "Custom 2";
			this.m_chCustom2.Width = 120;
			// 
			// m_chCustom3
			// 
			this.m_chCustom3.Text = "Custom 3";
			this.m_chCustom3.Width = 120;
			// 
			// m_tbSourcePreview
			// 
			this.m_tbSourcePreview.Location = new System.Drawing.Point(113, 39);
			this.m_tbSourcePreview.Multiline = true;
			this.m_tbSourcePreview.Name = "m_tbSourcePreview";
			this.m_tbSourcePreview.ReadOnly = true;
			this.m_tbSourcePreview.Size = new System.Drawing.Size(664, 90);
			this.m_tbSourcePreview.TabIndex = 5;
			// 
			// m_lblSourcePreview
			// 
			this.m_lblSourcePreview.AutoSize = true;
			this.m_lblSourcePreview.Location = new System.Drawing.Point(9, 42);
			this.m_lblSourcePreview.Name = "m_lblSourcePreview";
			this.m_lblSourcePreview.Size = new System.Drawing.Size(70, 13);
			this.m_lblSourcePreview.TabIndex = 4;
			this.m_lblSourcePreview.Text = "Source Data:";
			// 
			// m_lvHeaderOrder
			// 
			this.m_lvHeaderOrder.AllowColumnReorder = true;
			this.m_lvHeaderOrder.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.m_chOrderTitle,
            this.m_chOrderUser,
            this.m_chOrderPassword,
            this.m_chOrderURL,
            this.m_chOrderNotes,
            this.m_chOrderCustom1,
            this.m_chOrderCustom2,
            this.m_chOrderCustom3,
            this.m_chOrderIgnore0,
            this.m_chOrderIgnore1,
            this.m_chOrderIgnore2});
			this.m_lvHeaderOrder.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.m_lvHeaderOrder.Location = new System.Drawing.Point(113, 135);
			this.m_lvHeaderOrder.Name = "m_lvHeaderOrder";
			this.m_lvHeaderOrder.Size = new System.Drawing.Size(664, 24);
			this.m_lvHeaderOrder.TabIndex = 7;
			this.m_lvHeaderOrder.UseCompatibleStateImageBehavior = false;
			this.m_lvHeaderOrder.View = System.Windows.Forms.View.Details;
			// 
			// m_chOrderTitle
			// 
			this.m_chOrderTitle.Text = "Title";
			this.m_chOrderTitle.Width = 40;
			// 
			// m_chOrderUser
			// 
			this.m_chOrderUser.Text = "User Name";
			this.m_chOrderUser.Width = 70;
			// 
			// m_chOrderPassword
			// 
			this.m_chOrderPassword.Text = "Password";
			this.m_chOrderPassword.Width = 65;
			// 
			// m_chOrderURL
			// 
			this.m_chOrderURL.Text = "URL";
			this.m_chOrderURL.Width = 42;
			// 
			// m_chOrderNotes
			// 
			this.m_chOrderNotes.Text = "Notes";
			this.m_chOrderNotes.Width = 48;
			// 
			// m_chOrderCustom1
			// 
			this.m_chOrderCustom1.Text = "Custom 1";
			// 
			// m_chOrderCustom2
			// 
			this.m_chOrderCustom2.Text = "Custom 2";
			// 
			// m_chOrderCustom3
			// 
			this.m_chOrderCustom3.Text = "Custom 3";
			// 
			// m_chOrderIgnore0
			// 
			this.m_chOrderIgnore0.Text = "(Ignore)";
			// 
			// m_chOrderIgnore1
			// 
			this.m_chOrderIgnore1.Text = "(Ignore)";
			// 
			// m_chOrderIgnore2
			// 
			this.m_chOrderIgnore2.Text = "(Ignore)";
			// 
			// m_lblOrder
			// 
			this.m_lblOrder.AutoSize = true;
			this.m_lblOrder.Location = new System.Drawing.Point(9, 141);
			this.m_lblOrder.Name = "m_lblOrder";
			this.m_lblOrder.Size = new System.Drawing.Size(95, 13);
			this.m_lblOrder.TabIndex = 6;
			this.m_lblOrder.Text = "Define Field Order:";
			// 
			// m_lblOrderHint
			// 
			this.m_lblOrderHint.AutoSize = true;
			this.m_lblOrderHint.Location = new System.Drawing.Point(110, 162);
			this.m_lblOrderHint.Name = "m_lblOrderHint";
			this.m_lblOrderHint.Size = new System.Drawing.Size(489, 13);
			this.m_lblOrderHint.TabIndex = 8;
			this.m_lblOrderHint.Text = "Drag&&drop the columns to define the order of the fields in the CSV file. (Ignore" +
				") columns will be ignored.";
			// 
			// m_lblPreview
			// 
			this.m_lblPreview.AutoSize = true;
			this.m_lblPreview.Location = new System.Drawing.Point(9, 217);
			this.m_lblPreview.Name = "m_lblPreview";
			this.m_lblPreview.Size = new System.Drawing.Size(127, 13);
			this.m_lblPreview.TabIndex = 12;
			this.m_lblPreview.Text = "Imported Entries Preview:";
			// 
			// m_lblDelimiter
			// 
			this.m_lblDelimiter.AutoSize = true;
			this.m_lblDelimiter.Location = new System.Drawing.Point(9, 187);
			this.m_lblDelimiter.Name = "m_lblDelimiter";
			this.m_lblDelimiter.Size = new System.Drawing.Size(75, 13);
			this.m_lblDelimiter.TabIndex = 9;
			this.m_lblDelimiter.Text = "Field Delimiter:";
			// 
			// m_tbSepChar
			// 
			this.m_tbSepChar.Location = new System.Drawing.Point(113, 184);
			this.m_tbSepChar.Name = "m_tbSepChar";
			this.m_tbSepChar.Size = new System.Drawing.Size(35, 20);
			this.m_tbSepChar.TabIndex = 10;
			this.m_tbSepChar.TextChanged += new System.EventHandler(this.OnDelimiterTextChanged);
			// 
			// m_cbDoubleQuoteToSingle
			// 
			this.m_cbDoubleQuoteToSingle.AutoSize = true;
			this.m_cbDoubleQuoteToSingle.Location = new System.Drawing.Point(296, 186);
			this.m_cbDoubleQuoteToSingle.Name = "m_cbDoubleQuoteToSingle";
			this.m_cbDoubleQuoteToSingle.Size = new System.Drawing.Size(247, 17);
			this.m_cbDoubleQuoteToSingle.TabIndex = 11;
			this.m_cbDoubleQuoteToSingle.Text = "Replace Double Quote (\"\") By Single Quote (\")";
			this.m_cbDoubleQuoteToSingle.UseVisualStyleBackColor = true;
			this.m_cbDoubleQuoteToSingle.CheckedChanged += new System.EventHandler(this.OnDoubleQuoteCheckedChanged);
			// 
			// m_pbRender
			// 
			this.m_pbRender.Location = new System.Drawing.Point(142, 217);
			this.m_pbRender.Name = "m_pbRender";
			this.m_pbRender.Size = new System.Drawing.Size(554, 13);
			this.m_pbRender.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			this.m_pbRender.TabIndex = 13;
			// 
			// m_btnPreviewRefresh
			// 
			this.m_btnPreviewRefresh.Location = new System.Drawing.Point(702, 233);
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
			this.ClientSize = new System.Drawing.Size(789, 485);
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
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Generic CSV Importer";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
			this.Load += new System.EventHandler(this.OnFormLoad);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.Label m_lblFileEncoding;
		private System.Windows.Forms.ComboBox m_cmbEncoding;
		private System.Windows.Forms.ListView m_lvPreview;
		private System.Windows.Forms.ColumnHeader m_chTitle;
		private System.Windows.Forms.ColumnHeader m_chUser;
		private System.Windows.Forms.ColumnHeader m_chPassword;
		private System.Windows.Forms.ColumnHeader m_chURL;
		private System.Windows.Forms.ColumnHeader m_chNotes;
		private System.Windows.Forms.TextBox m_tbSourcePreview;
		private System.Windows.Forms.Label m_lblSourcePreview;
		private System.Windows.Forms.ListView m_lvHeaderOrder;
		private System.Windows.Forms.ColumnHeader m_chOrderTitle;
		private System.Windows.Forms.ColumnHeader m_chOrderUser;
		private System.Windows.Forms.ColumnHeader m_chOrderPassword;
		private System.Windows.Forms.ColumnHeader m_chOrderURL;
		private System.Windows.Forms.ColumnHeader m_chOrderNotes;
		private System.Windows.Forms.Label m_lblOrder;
		private System.Windows.Forms.Label m_lblOrderHint;
		private System.Windows.Forms.Label m_lblPreview;
		private System.Windows.Forms.Label m_lblDelimiter;
		private System.Windows.Forms.TextBox m_tbSepChar;
		private System.Windows.Forms.CheckBox m_cbDoubleQuoteToSingle;
		private System.Windows.Forms.ProgressBar m_pbRender;
		private System.Windows.Forms.Button m_btnPreviewRefresh;
		private System.Windows.Forms.ColumnHeader m_chOrderCustom1;
		private System.Windows.Forms.ColumnHeader m_chOrderCustom2;
		private System.Windows.Forms.ColumnHeader m_chOrderCustom3;
		private System.Windows.Forms.ColumnHeader m_chOrderIgnore0;
		private System.Windows.Forms.ColumnHeader m_chOrderIgnore1;
		private System.Windows.Forms.ColumnHeader m_chOrderIgnore2;
		private System.Windows.Forms.ColumnHeader m_chCustom1;
		private System.Windows.Forms.ColumnHeader m_chCustom2;
		private System.Windows.Forms.ColumnHeader m_chCustom3;
	}
}