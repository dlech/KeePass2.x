namespace KeePass.Forms
{
	partial class PrintForm
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
            this.m_tabMain = new System.Windows.Forms.TabControl();
            this.m_tabPreview = new System.Windows.Forms.TabPage();
            this.m_wbMain = new System.Windows.Forms.WebBrowser();
            this.m_lblPreviewHint = new System.Windows.Forms.Label();
            this.m_tabDataLayout = new System.Windows.Forms.TabPage();
            this.m_grpSorting = new System.Windows.Forms.GroupBox();
            this.m_lblEntrySortHint = new System.Windows.Forms.Label();
            this.m_cmbSortEntries = new System.Windows.Forms.ComboBox();
            this.m_lblSortEntries = new System.Windows.Forms.Label();
            this.m_grpFont = new System.Windows.Forms.GroupBox();
            this.m_cbSmallMono = new System.Windows.Forms.CheckBox();
            this.m_cbMonospaceForPasswords = new System.Windows.Forms.CheckBox();
            this.m_rbMonospace = new System.Windows.Forms.RadioButton();
            this.m_rbSansSerif = new System.Windows.Forms.RadioButton();
            this.m_rbSerif = new System.Windows.Forms.RadioButton();
            this.m_grpFields = new System.Windows.Forms.GroupBox();
            this.m_cbUuid = new System.Windows.Forms.CheckBox();
            this.m_cbTags = new System.Windows.Forms.CheckBox();
            this.m_cbCustomStrings = new System.Windows.Forms.CheckBox();
            this.m_cbGroups = new System.Windows.Forms.CheckBox();
            this.m_linkDeselectAllFields = new System.Windows.Forms.LinkLabel();
            this.m_linkSelectAllFields = new System.Windows.Forms.LinkLabel();
            this.m_cbAutoType = new System.Windows.Forms.CheckBox();
            this.m_cbLastMod = new System.Windows.Forms.CheckBox();
            this.m_cbCreation = new System.Windows.Forms.CheckBox();
            this.m_cbExpire = new System.Windows.Forms.CheckBox();
            this.m_cbNotes = new System.Windows.Forms.CheckBox();
            this.m_cbPassword = new System.Windows.Forms.CheckBox();
            this.m_cbUrl = new System.Windows.Forms.CheckBox();
            this.m_cbUser = new System.Windows.Forms.CheckBox();
            this.m_cbTitle = new System.Windows.Forms.CheckBox();
            this.m_grpLayout = new System.Windows.Forms.GroupBox();
            this.m_lblDetailsInfo = new System.Windows.Forms.Label();
            this.m_lblTabularInfo = new System.Windows.Forms.Label();
            this.m_rbDetails = new System.Windows.Forms.RadioButton();
            this.m_rbTabular = new System.Windows.Forms.RadioButton();
            this.m_bannerImage = new System.Windows.Forms.PictureBox();
            this.m_btnOK = new System.Windows.Forms.Button();
            this.m_btnCancel = new System.Windows.Forms.Button();
            this.m_btnConfigPrinter = new System.Windows.Forms.Button();
            this.m_btnPrintPreview = new System.Windows.Forms.Button();
            this.m_tabMain.SuspendLayout();
            this.m_tabPreview.SuspendLayout();
            this.m_tabDataLayout.SuspendLayout();
            this.m_grpSorting.SuspendLayout();
            this.m_grpFont.SuspendLayout();
            this.m_grpFields.SuspendLayout();
            this.m_grpLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
            this.SuspendLayout();
            // 
            // m_tabMain
            // 
            this.m_tabMain.Controls.Add(this.m_tabPreview);
            this.m_tabMain.Controls.Add(this.m_tabDataLayout);
            this.m_tabMain.Location = new System.Drawing.Point(16, 101);
            this.m_tabMain.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_tabMain.Name = "m_tabMain";
            this.m_tabMain.SelectedIndex = 0;
            this.m_tabMain.Size = new System.Drawing.Size(801, 712);
            this.m_tabMain.TabIndex = 2;
            this.m_tabMain.SelectedIndexChanged += new System.EventHandler(this.OnTabSelectedIndexChanged);
            // 
            // m_tabPreview
            // 
            this.m_tabPreview.Controls.Add(this.m_wbMain);
            this.m_tabPreview.Controls.Add(this.m_lblPreviewHint);
            this.m_tabPreview.Location = new System.Drawing.Point(4, 29);
            this.m_tabPreview.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_tabPreview.Name = "m_tabPreview";
            this.m_tabPreview.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_tabPreview.Size = new System.Drawing.Size(793, 679);
            this.m_tabPreview.TabIndex = 0;
            this.m_tabPreview.Text = "Preview";
            this.m_tabPreview.UseVisualStyleBackColor = true;
            // 
            // m_wbMain
            // 
            this.m_wbMain.AllowWebBrowserDrop = false;
            this.m_wbMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_wbMain.IsWebBrowserContextMenuEnabled = false;
            this.m_wbMain.Location = new System.Drawing.Point(4, 34);
            this.m_wbMain.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_wbMain.MinimumSize = new System.Drawing.Size(27, 31);
            this.m_wbMain.Name = "m_wbMain";
            this.m_wbMain.ScriptErrorsSuppressed = true;
            this.m_wbMain.Size = new System.Drawing.Size(785, 640);
            this.m_wbMain.TabIndex = 1;
            this.m_wbMain.WebBrowserShortcutsEnabled = false;
            // 
            // m_lblPreviewHint
            // 
            this.m_lblPreviewHint.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_lblPreviewHint.ForeColor = System.Drawing.Color.Brown;
            this.m_lblPreviewHint.Location = new System.Drawing.Point(4, 5);
            this.m_lblPreviewHint.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblPreviewHint.Name = "m_lblPreviewHint";
            this.m_lblPreviewHint.Size = new System.Drawing.Size(785, 29);
            this.m_lblPreviewHint.TabIndex = 0;
            this.m_lblPreviewHint.Text = "Note that this preview is a layout preview only. To see a preview of the printed " +
    "document, click the \'Print Preview\' button.";
            // 
            // m_tabDataLayout
            // 
            this.m_tabDataLayout.Controls.Add(this.m_grpSorting);
            this.m_tabDataLayout.Controls.Add(this.m_grpFont);
            this.m_tabDataLayout.Controls.Add(this.m_grpFields);
            this.m_tabDataLayout.Controls.Add(this.m_grpLayout);
            this.m_tabDataLayout.Location = new System.Drawing.Point(4, 25);
            this.m_tabDataLayout.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_tabDataLayout.Name = "m_tabDataLayout";
            this.m_tabDataLayout.Size = new System.Drawing.Size(793, 683);
            this.m_tabDataLayout.TabIndex = 2;
            this.m_tabDataLayout.Text = "Layout";
            this.m_tabDataLayout.UseVisualStyleBackColor = true;
            // 
            // m_grpSorting
            // 
            this.m_grpSorting.Controls.Add(this.m_lblEntrySortHint);
            this.m_grpSorting.Controls.Add(this.m_cmbSortEntries);
            this.m_grpSorting.Controls.Add(this.m_lblSortEntries);
            this.m_grpSorting.Location = new System.Drawing.Point(13, 548);
            this.m_grpSorting.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpSorting.Name = "m_grpSorting";
            this.m_grpSorting.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpSorting.Size = new System.Drawing.Size(761, 106);
            this.m_grpSorting.TabIndex = 3;
            this.m_grpSorting.TabStop = false;
            this.m_grpSorting.Text = "Sorting";
            // 
            // m_lblEntrySortHint
            // 
            this.m_lblEntrySortHint.AutoSize = true;
            this.m_lblEntrySortHint.Location = new System.Drawing.Point(8, 71);
            this.m_lblEntrySortHint.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblEntrySortHint.Name = "m_lblEntrySortHint";
            this.m_lblEntrySortHint.Size = new System.Drawing.Size(521, 20);
            this.m_lblEntrySortHint.TabIndex = 2;
            this.m_lblEntrySortHint.Text = "Entries are sorted within their groups, i.e. the group structure is not broken up" +
    ".";
            // 
            // m_cmbSortEntries
            // 
            this.m_cmbSortEntries.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_cmbSortEntries.FormattingEnabled = true;
            this.m_cmbSortEntries.Location = new System.Drawing.Point(148, 29);
            this.m_cmbSortEntries.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cmbSortEntries.Name = "m_cmbSortEntries";
            this.m_cmbSortEntries.Size = new System.Drawing.Size(228, 28);
            this.m_cmbSortEntries.TabIndex = 1;
            // 
            // m_lblSortEntries
            // 
            this.m_lblSortEntries.AutoSize = true;
            this.m_lblSortEntries.Location = new System.Drawing.Point(8, 34);
            this.m_lblSortEntries.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblSortEntries.Name = "m_lblSortEntries";
            this.m_lblSortEntries.Size = new System.Drawing.Size(141, 20);
            this.m_lblSortEntries.TabIndex = 0;
            this.m_lblSortEntries.Text = "Sort entries by field:";
            // 
            // m_grpFont
            // 
            this.m_grpFont.Controls.Add(this.m_cbSmallMono);
            this.m_grpFont.Controls.Add(this.m_cbMonospaceForPasswords);
            this.m_grpFont.Controls.Add(this.m_rbMonospace);
            this.m_grpFont.Controls.Add(this.m_rbSansSerif);
            this.m_grpFont.Controls.Add(this.m_rbSerif);
            this.m_grpFont.Location = new System.Drawing.Point(13, 399);
            this.m_grpFont.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpFont.Name = "m_grpFont";
            this.m_grpFont.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpFont.Size = new System.Drawing.Size(761, 140);
            this.m_grpFont.TabIndex = 2;
            this.m_grpFont.TabStop = false;
            this.m_grpFont.Text = "Font";
            // 
            // m_cbSmallMono
            // 
            this.m_cbSmallMono.AutoSize = true;
            this.m_cbSmallMono.Location = new System.Drawing.Point(13, 101);
            this.m_cbSmallMono.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cbSmallMono.Name = "m_cbSmallMono";
            this.m_cbSmallMono.Size = new System.Drawing.Size(243, 24);
            this.m_cbSmallMono.TabIndex = 4;
            this.m_cbSmallMono.Text = "Use extra small monospace font";
            this.m_cbSmallMono.UseVisualStyleBackColor = true;
            // 
            // m_cbMonospaceForPasswords
            // 
            this.m_cbMonospaceForPasswords.AutoSize = true;
            this.m_cbMonospaceForPasswords.Checked = true;
            this.m_cbMonospaceForPasswords.CheckState = System.Windows.Forms.CheckState.Checked;
            this.m_cbMonospaceForPasswords.Location = new System.Drawing.Point(13, 66);
            this.m_cbMonospaceForPasswords.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cbMonospaceForPasswords.Name = "m_cbMonospaceForPasswords";
            this.m_cbMonospaceForPasswords.Size = new System.Drawing.Size(263, 24);
            this.m_cbMonospaceForPasswords.TabIndex = 3;
            this.m_cbMonospaceForPasswords.Text = "Use monospace font for passwords";
            this.m_cbMonospaceForPasswords.UseVisualStyleBackColor = true;
            // 
            // m_rbMonospace
            // 
            this.m_rbMonospace.AutoSize = true;
            this.m_rbMonospace.Font = new System.Drawing.Font("Courier New", 9F);
            this.m_rbMonospace.Location = new System.Drawing.Point(189, 29);
            this.m_rbMonospace.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_rbMonospace.Name = "m_rbMonospace";
            this.m_rbMonospace.Size = new System.Drawing.Size(101, 21);
            this.m_rbMonospace.TabIndex = 2;
            this.m_rbMonospace.Text = "Monospace";
            this.m_rbMonospace.UseVisualStyleBackColor = true;
            // 
            // m_rbSansSerif
            // 
            this.m_rbSansSerif.AutoSize = true;
            this.m_rbSansSerif.Checked = true;
            this.m_rbSansSerif.Font = new System.Drawing.Font("Tahoma", 9F);
            this.m_rbSansSerif.Location = new System.Drawing.Point(83, 29);
            this.m_rbSansSerif.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_rbSansSerif.Name = "m_rbSansSerif";
            this.m_rbSansSerif.Size = new System.Drawing.Size(89, 21);
            this.m_rbSansSerif.TabIndex = 1;
            this.m_rbSansSerif.TabStop = true;
            this.m_rbSansSerif.Text = "Sans-Serif";
            this.m_rbSansSerif.UseVisualStyleBackColor = true;
            // 
            // m_rbSerif
            // 
            this.m_rbSerif.AutoSize = true;
            this.m_rbSerif.Font = new System.Drawing.Font("Times New Roman", 9F);
            this.m_rbSerif.Location = new System.Drawing.Point(13, 29);
            this.m_rbSerif.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_rbSerif.Name = "m_rbSerif";
            this.m_rbSerif.Size = new System.Drawing.Size(56, 20);
            this.m_rbSerif.TabIndex = 0;
            this.m_rbSerif.Text = "Serif";
            this.m_rbSerif.UseVisualStyleBackColor = true;
            // 
            // m_grpFields
            // 
            this.m_grpFields.Controls.Add(this.m_cbUuid);
            this.m_grpFields.Controls.Add(this.m_cbTags);
            this.m_grpFields.Controls.Add(this.m_cbCustomStrings);
            this.m_grpFields.Controls.Add(this.m_cbGroups);
            this.m_grpFields.Controls.Add(this.m_linkDeselectAllFields);
            this.m_grpFields.Controls.Add(this.m_linkSelectAllFields);
            this.m_grpFields.Controls.Add(this.m_cbAutoType);
            this.m_grpFields.Controls.Add(this.m_cbLastMod);
            this.m_grpFields.Controls.Add(this.m_cbCreation);
            this.m_grpFields.Controls.Add(this.m_cbExpire);
            this.m_grpFields.Controls.Add(this.m_cbNotes);
            this.m_grpFields.Controls.Add(this.m_cbPassword);
            this.m_grpFields.Controls.Add(this.m_cbUrl);
            this.m_grpFields.Controls.Add(this.m_cbUser);
            this.m_grpFields.Controls.Add(this.m_cbTitle);
            this.m_grpFields.Location = new System.Drawing.Point(13, 211);
            this.m_grpFields.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpFields.Name = "m_grpFields";
            this.m_grpFields.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpFields.Size = new System.Drawing.Size(761, 179);
            this.m_grpFields.TabIndex = 1;
            this.m_grpFields.TabStop = false;
            this.m_grpFields.Text = "Fields";
            // 
            // m_cbUuid
            // 
            this.m_cbUuid.AutoSize = true;
            this.m_cbUuid.Location = new System.Drawing.Point(325, 100);
            this.m_cbUuid.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cbUuid.Name = "m_cbUuid";
            this.m_cbUuid.Size = new System.Drawing.Size(66, 24);
            this.m_cbUuid.TabIndex = 12;
            this.m_cbUuid.Text = "UUID";
            this.m_cbUuid.UseVisualStyleBackColor = true;
            // 
            // m_cbTags
            // 
            this.m_cbTags.AutoSize = true;
            this.m_cbTags.Location = new System.Drawing.Point(619, 65);
            this.m_cbTags.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cbTags.Name = "m_cbTags";
            this.m_cbTags.Size = new System.Drawing.Size(60, 24);
            this.m_cbTags.TabIndex = 9;
            this.m_cbTags.Text = "Tags";
            this.m_cbTags.UseVisualStyleBackColor = true;
            // 
            // m_cbCustomStrings
            // 
            this.m_cbCustomStrings.AutoSize = true;
            this.m_cbCustomStrings.Location = new System.Drawing.Point(141, 100);
            this.m_cbCustomStrings.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cbCustomStrings.Name = "m_cbCustomStrings";
            this.m_cbCustomStrings.Size = new System.Drawing.Size(162, 24);
            this.m_cbCustomStrings.TabIndex = 11;
            this.m_cbCustomStrings.Text = "Custom string fields";
            this.m_cbCustomStrings.UseVisualStyleBackColor = true;
            // 
            // m_cbGroups
            // 
            this.m_cbGroups.AutoSize = true;
            this.m_cbGroups.Location = new System.Drawing.Point(12, 100);
            this.m_cbGroups.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cbGroups.Name = "m_cbGroups";
            this.m_cbGroups.Size = new System.Drawing.Size(113, 24);
            this.m_cbGroups.TabIndex = 10;
            this.m_cbGroups.Text = "Group name";
            this.m_cbGroups.UseVisualStyleBackColor = true;
            // 
            // m_linkDeselectAllFields
            // 
            this.m_linkDeselectAllFields.AutoSize = true;
            this.m_linkDeselectAllFields.Location = new System.Drawing.Point(84, 140);
            this.m_linkDeselectAllFields.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_linkDeselectAllFields.Name = "m_linkDeselectAllFields";
            this.m_linkDeselectAllFields.Size = new System.Drawing.Size(88, 20);
            this.m_linkDeselectAllFields.TabIndex = 14;
            this.m_linkDeselectAllFields.TabStop = true;
            this.m_linkDeselectAllFields.Text = "Deselect All";
            this.m_linkDeselectAllFields.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnLinkDeselectAllFields);
            // 
            // m_linkSelectAllFields
            // 
            this.m_linkSelectAllFields.AutoSize = true;
            this.m_linkSelectAllFields.Location = new System.Drawing.Point(8, 140);
            this.m_linkSelectAllFields.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_linkSelectAllFields.Name = "m_linkSelectAllFields";
            this.m_linkSelectAllFields.Size = new System.Drawing.Size(71, 20);
            this.m_linkSelectAllFields.TabIndex = 13;
            this.m_linkSelectAllFields.TabStop = true;
            this.m_linkSelectAllFields.Text = "Select All";
            this.m_linkSelectAllFields.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnLinkSelectAllFields);
            // 
            // m_cbAutoType
            // 
            this.m_cbAutoType.AutoSize = true;
            this.m_cbAutoType.Location = new System.Drawing.Point(480, 65);
            this.m_cbAutoType.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cbAutoType.Name = "m_cbAutoType";
            this.m_cbAutoType.Size = new System.Drawing.Size(100, 24);
            this.m_cbAutoType.TabIndex = 8;
            this.m_cbAutoType.Text = "Auto-Type";
            this.m_cbAutoType.UseVisualStyleBackColor = true;
            // 
            // m_cbLastMod
            // 
            this.m_cbLastMod.AutoSize = true;
            this.m_cbLastMod.Location = new System.Drawing.Point(141, 65);
            this.m_cbLastMod.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cbLastMod.Name = "m_cbLastMod";
            this.m_cbLastMod.Size = new System.Drawing.Size(180, 24);
            this.m_cbLastMod.TabIndex = 6;
            this.m_cbLastMod.Text = "Last modification time";
            this.m_cbLastMod.UseVisualStyleBackColor = true;
            // 
            // m_cbCreation
            // 
            this.m_cbCreation.AutoSize = true;
            this.m_cbCreation.Location = new System.Drawing.Point(12, 65);
            this.m_cbCreation.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cbCreation.Name = "m_cbCreation";
            this.m_cbCreation.Size = new System.Drawing.Size(121, 24);
            this.m_cbCreation.TabIndex = 5;
            this.m_cbCreation.Text = "Creation time";
            this.m_cbCreation.UseVisualStyleBackColor = true;
            // 
            // m_cbExpire
            // 
            this.m_cbExpire.AutoSize = true;
            this.m_cbExpire.Location = new System.Drawing.Point(325, 65);
            this.m_cbExpire.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cbExpire.Name = "m_cbExpire";
            this.m_cbExpire.Size = new System.Drawing.Size(105, 24);
            this.m_cbExpire.TabIndex = 7;
            this.m_cbExpire.Text = "Expiry time";
            this.m_cbExpire.UseVisualStyleBackColor = true;
            // 
            // m_cbNotes
            // 
            this.m_cbNotes.AutoSize = true;
            this.m_cbNotes.Checked = true;
            this.m_cbNotes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.m_cbNotes.Location = new System.Drawing.Point(619, 29);
            this.m_cbNotes.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cbNotes.Name = "m_cbNotes";
            this.m_cbNotes.Size = new System.Drawing.Size(70, 24);
            this.m_cbNotes.TabIndex = 4;
            this.m_cbNotes.Text = "Notes";
            this.m_cbNotes.UseVisualStyleBackColor = true;
            // 
            // m_cbPassword
            // 
            this.m_cbPassword.AutoSize = true;
            this.m_cbPassword.Checked = true;
            this.m_cbPassword.CheckState = System.Windows.Forms.CheckState.Checked;
            this.m_cbPassword.Location = new System.Drawing.Point(325, 29);
            this.m_cbPassword.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cbPassword.Name = "m_cbPassword";
            this.m_cbPassword.Size = new System.Drawing.Size(92, 24);
            this.m_cbPassword.TabIndex = 2;
            this.m_cbPassword.Text = "Password";
            this.m_cbPassword.UseVisualStyleBackColor = true;
            // 
            // m_cbUrl
            // 
            this.m_cbUrl.AutoSize = true;
            this.m_cbUrl.Location = new System.Drawing.Point(480, 29);
            this.m_cbUrl.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cbUrl.Name = "m_cbUrl";
            this.m_cbUrl.Size = new System.Drawing.Size(57, 24);
            this.m_cbUrl.TabIndex = 3;
            this.m_cbUrl.Text = "URL";
            this.m_cbUrl.UseVisualStyleBackColor = true;
            // 
            // m_cbUser
            // 
            this.m_cbUser.AutoSize = true;
            this.m_cbUser.Checked = true;
            this.m_cbUser.CheckState = System.Windows.Forms.CheckState.Checked;
            this.m_cbUser.Location = new System.Drawing.Point(141, 29);
            this.m_cbUser.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cbUser.Name = "m_cbUser";
            this.m_cbUser.Size = new System.Drawing.Size(101, 24);
            this.m_cbUser.TabIndex = 1;
            this.m_cbUser.Text = "User name";
            this.m_cbUser.UseVisualStyleBackColor = true;
            // 
            // m_cbTitle
            // 
            this.m_cbTitle.AutoSize = true;
            this.m_cbTitle.Checked = true;
            this.m_cbTitle.CheckState = System.Windows.Forms.CheckState.Checked;
            this.m_cbTitle.Location = new System.Drawing.Point(12, 29);
            this.m_cbTitle.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cbTitle.Name = "m_cbTitle";
            this.m_cbTitle.Size = new System.Drawing.Size(60, 24);
            this.m_cbTitle.TabIndex = 0;
            this.m_cbTitle.Text = "Title";
            this.m_cbTitle.UseVisualStyleBackColor = true;
            // 
            // m_grpLayout
            // 
            this.m_grpLayout.Controls.Add(this.m_lblDetailsInfo);
            this.m_grpLayout.Controls.Add(this.m_lblTabularInfo);
            this.m_grpLayout.Controls.Add(this.m_rbDetails);
            this.m_grpLayout.Controls.Add(this.m_rbTabular);
            this.m_grpLayout.Location = new System.Drawing.Point(13, 15);
            this.m_grpLayout.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpLayout.Name = "m_grpLayout";
            this.m_grpLayout.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpLayout.Size = new System.Drawing.Size(761, 186);
            this.m_grpLayout.TabIndex = 0;
            this.m_grpLayout.TabStop = false;
            this.m_grpLayout.Text = "Layout";
            // 
            // m_lblDetailsInfo
            // 
            this.m_lblDetailsInfo.AutoSize = true;
            this.m_lblDetailsInfo.Location = new System.Drawing.Point(35, 148);
            this.m_lblDetailsInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblDetailsInfo.Name = "m_lblDetailsInfo";
            this.m_lblDetailsInfo.Size = new System.Drawing.Size(475, 20);
            this.m_lblDetailsInfo.TabIndex = 3;
            this.m_lblDetailsInfo.Text = "Arrange the entries in blocks. The fields selected below will be printed.";
            // 
            // m_lblTabularInfo
            // 
            this.m_lblTabularInfo.Location = new System.Drawing.Point(35, 60);
            this.m_lblTabularInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblTabularInfo.Name = "m_lblTabularInfo";
            this.m_lblTabularInfo.Size = new System.Drawing.Size(719, 42);
            this.m_lblTabularInfo.TabIndex = 1;
            this.m_lblTabularInfo.Text = "Arrange the entries in a tabular form. Each entry will occupy approximately one l" +
    "ine. The fields selected below will be printed; auto-type configuration is not p" +
    "rinted.";
            // 
            // m_rbDetails
            // 
            this.m_rbDetails.AutoSize = true;
            this.m_rbDetails.Location = new System.Drawing.Point(12, 118);
            this.m_rbDetails.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_rbDetails.Name = "m_rbDetails";
            this.m_rbDetails.Size = new System.Drawing.Size(76, 24);
            this.m_rbDetails.TabIndex = 2;
            this.m_rbDetails.TabStop = true;
            this.m_rbDetails.Text = "&Details";
            this.m_rbDetails.UseVisualStyleBackColor = true;
            // 
            // m_rbTabular
            // 
            this.m_rbTabular.AutoSize = true;
            this.m_rbTabular.Location = new System.Drawing.Point(12, 29);
            this.m_rbTabular.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_rbTabular.Name = "m_rbTabular";
            this.m_rbTabular.Size = new System.Drawing.Size(78, 24);
            this.m_rbTabular.TabIndex = 0;
            this.m_rbTabular.TabStop = true;
            this.m_rbTabular.Text = "&Tabular";
            this.m_rbTabular.UseVisualStyleBackColor = true;
            this.m_rbTabular.CheckedChanged += new System.EventHandler(this.OnTabularCheckedChanged);
            // 
            // m_bannerImage
            // 
            this.m_bannerImage.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_bannerImage.Location = new System.Drawing.Point(0, 0);
            this.m_bannerImage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_bannerImage.Name = "m_bannerImage";
            this.m_bannerImage.Size = new System.Drawing.Size(833, 92);
            this.m_bannerImage.TabIndex = 1;
            this.m_bannerImage.TabStop = false;
            // 
            // m_btnOK
            // 
            this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_btnOK.Location = new System.Drawing.Point(609, 826);
            this.m_btnOK.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnOK.Name = "m_btnOK";
            this.m_btnOK.Size = new System.Drawing.Size(100, 35);
            this.m_btnOK.TabIndex = 0;
            this.m_btnOK.Text = "&Print...";
            this.m_btnOK.UseVisualStyleBackColor = true;
            this.m_btnOK.Click += new System.EventHandler(this.OnBtnOK);
            // 
            // m_btnCancel
            // 
            this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_btnCancel.Location = new System.Drawing.Point(717, 826);
            this.m_btnCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnCancel.Name = "m_btnCancel";
            this.m_btnCancel.Size = new System.Drawing.Size(100, 35);
            this.m_btnCancel.TabIndex = 1;
            this.m_btnCancel.Text = "Cancel";
            this.m_btnCancel.UseVisualStyleBackColor = true;
            this.m_btnCancel.Click += new System.EventHandler(this.OnBtnCancel);
            // 
            // m_btnConfigPrinter
            // 
            this.m_btnConfigPrinter.Location = new System.Drawing.Point(16, 826);
            this.m_btnConfigPrinter.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnConfigPrinter.Name = "m_btnConfigPrinter";
            this.m_btnConfigPrinter.Size = new System.Drawing.Size(133, 35);
            this.m_btnConfigPrinter.TabIndex = 3;
            this.m_btnConfigPrinter.Text = "Page &Setup";
            this.m_btnConfigPrinter.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.m_btnConfigPrinter.UseVisualStyleBackColor = true;
            this.m_btnConfigPrinter.Click += new System.EventHandler(this.OnBtnConfigPage);
            // 
            // m_btnPrintPreview
            // 
            this.m_btnPrintPreview.Location = new System.Drawing.Point(157, 826);
            this.m_btnPrintPreview.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnPrintPreview.Name = "m_btnPrintPreview";
            this.m_btnPrintPreview.Size = new System.Drawing.Size(133, 35);
            this.m_btnPrintPreview.TabIndex = 4;
            this.m_btnPrintPreview.Text = "Print Pre&view";
            this.m_btnPrintPreview.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.m_btnPrintPreview.UseVisualStyleBackColor = true;
            this.m_btnPrintPreview.Click += new System.EventHandler(this.OnBtnPrintPreview);
            // 
            // PrintForm
            // 
            this.AcceptButton = this.m_btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.m_btnCancel;
            this.ClientSize = new System.Drawing.Size(833, 880);
            this.Controls.Add(this.m_btnCancel);
            this.Controls.Add(this.m_tabMain);
            this.Controls.Add(this.m_btnOK);
            this.Controls.Add(this.m_btnPrintPreview);
            this.Controls.Add(this.m_btnConfigPrinter);
            this.Controls.Add(this.m_bannerImage);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(79, 47);
            this.Name = "PrintForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Print";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.m_tabMain.ResumeLayout(false);
            this.m_tabPreview.ResumeLayout(false);
            this.m_tabDataLayout.ResumeLayout(false);
            this.m_grpSorting.ResumeLayout(false);
            this.m_grpSorting.PerformLayout();
            this.m_grpFont.ResumeLayout(false);
            this.m_grpFont.PerformLayout();
            this.m_grpFields.ResumeLayout(false);
            this.m_grpFields.PerformLayout();
            this.m_grpLayout.ResumeLayout(false);
            this.m_grpLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl m_tabMain;
		private System.Windows.Forms.TabPage m_tabPreview;
		private System.Windows.Forms.PictureBox m_bannerImage;
		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.WebBrowser m_wbMain;
		private System.Windows.Forms.TabPage m_tabDataLayout;
		private System.Windows.Forms.Button m_btnConfigPrinter;
		private System.Windows.Forms.Button m_btnPrintPreview;
		private System.Windows.Forms.GroupBox m_grpLayout;
		private System.Windows.Forms.RadioButton m_rbTabular;
		private System.Windows.Forms.RadioButton m_rbDetails;
		private System.Windows.Forms.Label m_lblDetailsInfo;
		private System.Windows.Forms.Label m_lblTabularInfo;
		private System.Windows.Forms.GroupBox m_grpFields;
		private System.Windows.Forms.CheckBox m_cbNotes;
		private System.Windows.Forms.CheckBox m_cbPassword;
		private System.Windows.Forms.CheckBox m_cbUrl;
		private System.Windows.Forms.CheckBox m_cbUser;
		private System.Windows.Forms.CheckBox m_cbTitle;
		private System.Windows.Forms.Label m_lblPreviewHint;
		private System.Windows.Forms.GroupBox m_grpFont;
		private System.Windows.Forms.CheckBox m_cbMonospaceForPasswords;
		private System.Windows.Forms.RadioButton m_rbMonospace;
		private System.Windows.Forms.RadioButton m_rbSansSerif;
		private System.Windows.Forms.RadioButton m_rbSerif;
		private System.Windows.Forms.CheckBox m_cbSmallMono;
		private System.Windows.Forms.CheckBox m_cbExpire;
		private System.Windows.Forms.CheckBox m_cbAutoType;
		private System.Windows.Forms.CheckBox m_cbLastMod;
		private System.Windows.Forms.CheckBox m_cbCreation;
		private System.Windows.Forms.LinkLabel m_linkDeselectAllFields;
		private System.Windows.Forms.LinkLabel m_linkSelectAllFields;
		private System.Windows.Forms.CheckBox m_cbGroups;
		private System.Windows.Forms.CheckBox m_cbCustomStrings;
		private System.Windows.Forms.GroupBox m_grpSorting;
		private System.Windows.Forms.ComboBox m_cmbSortEntries;
		private System.Windows.Forms.Label m_lblSortEntries;
		private System.Windows.Forms.Label m_lblEntrySortHint;
		private System.Windows.Forms.CheckBox m_cbTags;
		private System.Windows.Forms.CheckBox m_cbUuid;
	}
}