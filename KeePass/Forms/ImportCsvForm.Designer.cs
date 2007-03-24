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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportCsvForm));
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
			// m_lblFileEncoding
			// 
			resources.ApplyResources(this.m_lblFileEncoding, "m_lblFileEncoding");
			this.m_lblFileEncoding.Name = "m_lblFileEncoding";
			// 
			// m_cmbEncoding
			// 
			this.m_cmbEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbEncoding.FormattingEnabled = true;
			this.m_cmbEncoding.Items.AddRange(new object[] {
            resources.GetString("m_cmbEncoding.Items"),
            resources.GetString("m_cmbEncoding.Items1"),
            resources.GetString("m_cmbEncoding.Items2"),
            resources.GetString("m_cmbEncoding.Items3"),
            resources.GetString("m_cmbEncoding.Items4"),
            resources.GetString("m_cmbEncoding.Items5"),
            resources.GetString("m_cmbEncoding.Items6")});
			resources.ApplyResources(this.m_cmbEncoding, "m_cmbEncoding");
			this.m_cmbEncoding.Name = "m_cmbEncoding";
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
			resources.ApplyResources(this.m_lvPreview, "m_lvPreview");
			this.m_lvPreview.Name = "m_lvPreview";
			this.m_lvPreview.ShowItemToolTips = true;
			this.m_lvPreview.UseCompatibleStateImageBehavior = false;
			this.m_lvPreview.View = System.Windows.Forms.View.Details;
			// 
			// m_chTitle
			// 
			resources.ApplyResources(this.m_chTitle, "m_chTitle");
			// 
			// m_chUser
			// 
			resources.ApplyResources(this.m_chUser, "m_chUser");
			// 
			// m_chPassword
			// 
			resources.ApplyResources(this.m_chPassword, "m_chPassword");
			// 
			// m_chURL
			// 
			resources.ApplyResources(this.m_chURL, "m_chURL");
			// 
			// m_chNotes
			// 
			resources.ApplyResources(this.m_chNotes, "m_chNotes");
			// 
			// m_chCustom1
			// 
			resources.ApplyResources(this.m_chCustom1, "m_chCustom1");
			// 
			// m_chCustom2
			// 
			resources.ApplyResources(this.m_chCustom2, "m_chCustom2");
			// 
			// m_chCustom3
			// 
			resources.ApplyResources(this.m_chCustom3, "m_chCustom3");
			// 
			// m_tbSourcePreview
			// 
			resources.ApplyResources(this.m_tbSourcePreview, "m_tbSourcePreview");
			this.m_tbSourcePreview.Name = "m_tbSourcePreview";
			this.m_tbSourcePreview.ReadOnly = true;
			// 
			// m_lblSourcePreview
			// 
			resources.ApplyResources(this.m_lblSourcePreview, "m_lblSourcePreview");
			this.m_lblSourcePreview.Name = "m_lblSourcePreview";
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
			resources.ApplyResources(this.m_lvHeaderOrder, "m_lvHeaderOrder");
			this.m_lvHeaderOrder.Name = "m_lvHeaderOrder";
			this.m_lvHeaderOrder.UseCompatibleStateImageBehavior = false;
			this.m_lvHeaderOrder.View = System.Windows.Forms.View.Details;
			// 
			// m_chOrderTitle
			// 
			resources.ApplyResources(this.m_chOrderTitle, "m_chOrderTitle");
			// 
			// m_chOrderUser
			// 
			resources.ApplyResources(this.m_chOrderUser, "m_chOrderUser");
			// 
			// m_chOrderPassword
			// 
			resources.ApplyResources(this.m_chOrderPassword, "m_chOrderPassword");
			// 
			// m_chOrderURL
			// 
			resources.ApplyResources(this.m_chOrderURL, "m_chOrderURL");
			// 
			// m_chOrderNotes
			// 
			resources.ApplyResources(this.m_chOrderNotes, "m_chOrderNotes");
			// 
			// m_chOrderCustom1
			// 
			resources.ApplyResources(this.m_chOrderCustom1, "m_chOrderCustom1");
			// 
			// m_chOrderCustom2
			// 
			resources.ApplyResources(this.m_chOrderCustom2, "m_chOrderCustom2");
			// 
			// m_chOrderCustom3
			// 
			resources.ApplyResources(this.m_chOrderCustom3, "m_chOrderCustom3");
			// 
			// m_chOrderIgnore0
			// 
			resources.ApplyResources(this.m_chOrderIgnore0, "m_chOrderIgnore0");
			// 
			// m_chOrderIgnore1
			// 
			resources.ApplyResources(this.m_chOrderIgnore1, "m_chOrderIgnore1");
			// 
			// m_chOrderIgnore2
			// 
			resources.ApplyResources(this.m_chOrderIgnore2, "m_chOrderIgnore2");
			// 
			// m_lblOrder
			// 
			resources.ApplyResources(this.m_lblOrder, "m_lblOrder");
			this.m_lblOrder.Name = "m_lblOrder";
			// 
			// m_lblOrderHint
			// 
			resources.ApplyResources(this.m_lblOrderHint, "m_lblOrderHint");
			this.m_lblOrderHint.Name = "m_lblOrderHint";
			// 
			// m_lblPreview
			// 
			resources.ApplyResources(this.m_lblPreview, "m_lblPreview");
			this.m_lblPreview.Name = "m_lblPreview";
			// 
			// m_lblDelimiter
			// 
			resources.ApplyResources(this.m_lblDelimiter, "m_lblDelimiter");
			this.m_lblDelimiter.Name = "m_lblDelimiter";
			// 
			// m_tbSepChar
			// 
			resources.ApplyResources(this.m_tbSepChar, "m_tbSepChar");
			this.m_tbSepChar.Name = "m_tbSepChar";
			this.m_tbSepChar.TextChanged += new System.EventHandler(this.OnDelimiterTextChanged);
			// 
			// m_cbDoubleQuoteToSingle
			// 
			resources.ApplyResources(this.m_cbDoubleQuoteToSingle, "m_cbDoubleQuoteToSingle");
			this.m_cbDoubleQuoteToSingle.Name = "m_cbDoubleQuoteToSingle";
			this.m_cbDoubleQuoteToSingle.UseVisualStyleBackColor = true;
			this.m_cbDoubleQuoteToSingle.CheckedChanged += new System.EventHandler(this.OnDoubleQuoteCheckedChanged);
			// 
			// m_pbRender
			// 
			resources.ApplyResources(this.m_pbRender, "m_pbRender");
			this.m_pbRender.Name = "m_pbRender";
			this.m_pbRender.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			// 
			// m_btnPreviewRefresh
			// 
			resources.ApplyResources(this.m_btnPreviewRefresh, "m_btnPreviewRefresh");
			this.m_btnPreviewRefresh.Name = "m_btnPreviewRefresh";
			this.m_btnPreviewRefresh.UseVisualStyleBackColor = true;
			this.m_btnPreviewRefresh.Click += new System.EventHandler(this.OnBtnPreviewRefresh);
			// 
			// ImportCsvForm
			// 
			this.AcceptButton = this.m_btnOK;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
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