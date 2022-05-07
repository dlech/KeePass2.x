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
			this.m_grpAdv = new System.Windows.Forms.GroupBox();
			this.m_lblSpr = new System.Windows.Forms.Label();
			this.m_cmbSpr = new System.Windows.Forms.ComboBox();
			this.m_grpSorting = new System.Windows.Forms.GroupBox();
			this.m_lblEntrySortHint = new System.Windows.Forms.Label();
			this.m_cmbSortEntries = new System.Windows.Forms.ComboBox();
			this.m_lblSortEntries = new System.Windows.Forms.Label();
			this.m_grpStyle = new System.Windows.Forms.GroupBox();
			this.m_lblColorPO = new System.Windows.Forms.Label();
			this.m_lblColorPD = new System.Windows.Forms.Label();
			this.m_lblColorPL = new System.Windows.Forms.Label();
			this.m_lblColorPU = new System.Windows.Forms.Label();
			this.m_btnColorPO = new KeePass.UI.ColorButtonEx();
			this.m_btnColorPD = new KeePass.UI.ColorButtonEx();
			this.m_btnColorPL = new KeePass.UI.ColorButtonEx();
			this.m_btnColorPU = new KeePass.UI.ColorButtonEx();
			this.m_cbColorP = new System.Windows.Forms.CheckBox();
			this.m_btnPasswordFont = new System.Windows.Forms.Button();
			this.m_btnMainFont = new System.Windows.Forms.Button();
			this.m_cbPasswordFont = new System.Windows.Forms.CheckBox();
			this.m_cbMainFont = new System.Windows.Forms.CheckBox();
			this.m_grpFields = new System.Windows.Forms.GroupBox();
			this.m_cbIcon = new System.Windows.Forms.CheckBox();
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
			this.m_lblLayBlocks = new System.Windows.Forms.Label();
			this.m_lblLayTable = new System.Windows.Forms.Label();
			this.m_rbLayBlocks = new System.Windows.Forms.RadioButton();
			this.m_rbLayTable = new System.Windows.Forms.RadioButton();
			this.m_bannerImage = new System.Windows.Forms.PictureBox();
			this.m_btnOK = new System.Windows.Forms.Button();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_btnConfigPrinter = new System.Windows.Forms.Button();
			this.m_btnPrintPreview = new System.Windows.Forms.Button();
			this.m_tabMain.SuspendLayout();
			this.m_tabPreview.SuspendLayout();
			this.m_tabDataLayout.SuspendLayout();
			this.m_grpAdv.SuspendLayout();
			this.m_grpSorting.SuspendLayout();
			this.m_grpStyle.SuspendLayout();
			this.m_grpFields.SuspendLayout();
			this.m_grpLayout.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
			this.SuspendLayout();
			// 
			// m_tabMain
			// 
			this.m_tabMain.Controls.Add(this.m_tabPreview);
			this.m_tabMain.Controls.Add(this.m_tabDataLayout);
			this.m_tabMain.Location = new System.Drawing.Point(12, 66);
			this.m_tabMain.Name = "m_tabMain";
			this.m_tabMain.SelectedIndex = 0;
			this.m_tabMain.Size = new System.Drawing.Size(601, 463);
			this.m_tabMain.TabIndex = 2;
			this.m_tabMain.SelectedIndexChanged += new System.EventHandler(this.OnTabSelectedIndexChanged);
			// 
			// m_tabPreview
			// 
			this.m_tabPreview.Controls.Add(this.m_wbMain);
			this.m_tabPreview.Controls.Add(this.m_lblPreviewHint);
			this.m_tabPreview.Location = new System.Drawing.Point(4, 22);
			this.m_tabPreview.Name = "m_tabPreview";
			this.m_tabPreview.Padding = new System.Windows.Forms.Padding(3);
			this.m_tabPreview.Size = new System.Drawing.Size(593, 437);
			this.m_tabPreview.TabIndex = 0;
			this.m_tabPreview.Text = "Preview";
			this.m_tabPreview.UseVisualStyleBackColor = true;
			// 
			// m_wbMain
			// 
			this.m_wbMain.AllowWebBrowserDrop = false;
			this.m_wbMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.m_wbMain.IsWebBrowserContextMenuEnabled = false;
			this.m_wbMain.Location = new System.Drawing.Point(3, 22);
			this.m_wbMain.MinimumSize = new System.Drawing.Size(20, 20);
			this.m_wbMain.Name = "m_wbMain";
			this.m_wbMain.ScriptErrorsSuppressed = true;
			this.m_wbMain.Size = new System.Drawing.Size(587, 412);
			this.m_wbMain.TabIndex = 1;
			this.m_wbMain.WebBrowserShortcutsEnabled = false;
			// 
			// m_lblPreviewHint
			// 
			this.m_lblPreviewHint.Dock = System.Windows.Forms.DockStyle.Top;
			this.m_lblPreviewHint.ForeColor = System.Drawing.Color.Brown;
			this.m_lblPreviewHint.Location = new System.Drawing.Point(3, 3);
			this.m_lblPreviewHint.Name = "m_lblPreviewHint";
			this.m_lblPreviewHint.Size = new System.Drawing.Size(587, 19);
			this.m_lblPreviewHint.TabIndex = 0;
			this.m_lblPreviewHint.Text = "Note that this preview is a layout preview only. To see a preview of the printed " +
				"document, click the \'Print Preview\' button.";
			// 
			// m_tabDataLayout
			// 
			this.m_tabDataLayout.Controls.Add(this.m_grpAdv);
			this.m_tabDataLayout.Controls.Add(this.m_grpSorting);
			this.m_tabDataLayout.Controls.Add(this.m_grpStyle);
			this.m_tabDataLayout.Controls.Add(this.m_grpFields);
			this.m_tabDataLayout.Controls.Add(this.m_grpLayout);
			this.m_tabDataLayout.Location = new System.Drawing.Point(4, 22);
			this.m_tabDataLayout.Name = "m_tabDataLayout";
			this.m_tabDataLayout.Size = new System.Drawing.Size(593, 437);
			this.m_tabDataLayout.TabIndex = 2;
			this.m_tabDataLayout.Text = "Layout";
			this.m_tabDataLayout.UseVisualStyleBackColor = true;
			// 
			// m_grpAdv
			// 
			this.m_grpAdv.Controls.Add(this.m_lblSpr);
			this.m_grpAdv.Controls.Add(this.m_cmbSpr);
			this.m_grpAdv.Location = new System.Drawing.Point(299, 330);
			this.m_grpAdv.Name = "m_grpAdv";
			this.m_grpAdv.Size = new System.Drawing.Size(282, 52);
			this.m_grpAdv.TabIndex = 4;
			this.m_grpAdv.TabStop = false;
			this.m_grpAdv.Text = "Advanced";
			// 
			// m_lblSpr
			// 
			this.m_lblSpr.AutoSize = true;
			this.m_lblSpr.Location = new System.Drawing.Point(6, 21);
			this.m_lblSpr.Name = "m_lblSpr";
			this.m_lblSpr.Size = new System.Drawing.Size(71, 13);
			this.m_lblSpr.TabIndex = 0;
			this.m_lblSpr.Text = "Place&holders:";
			// 
			// m_cmbSpr
			// 
			this.m_cmbSpr.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbSpr.FormattingEnabled = true;
			this.m_cmbSpr.Location = new System.Drawing.Point(111, 18);
			this.m_cmbSpr.Name = "m_cmbSpr";
			this.m_cmbSpr.Size = new System.Drawing.Size(161, 21);
			this.m_cmbSpr.TabIndex = 1;
			// 
			// m_grpSorting
			// 
			this.m_grpSorting.Controls.Add(this.m_lblEntrySortHint);
			this.m_grpSorting.Controls.Add(this.m_cmbSortEntries);
			this.m_grpSorting.Controls.Add(this.m_lblSortEntries);
			this.m_grpSorting.Location = new System.Drawing.Point(299, 240);
			this.m_grpSorting.Name = "m_grpSorting";
			this.m_grpSorting.Size = new System.Drawing.Size(282, 84);
			this.m_grpSorting.TabIndex = 3;
			this.m_grpSorting.TabStop = false;
			this.m_grpSorting.Text = "Sorting";
			// 
			// m_lblEntrySortHint
			// 
			this.m_lblEntrySortHint.Location = new System.Drawing.Point(6, 46);
			this.m_lblEntrySortHint.Name = "m_lblEntrySortHint";
			this.m_lblEntrySortHint.Size = new System.Drawing.Size(266, 28);
			this.m_lblEntrySortHint.TabIndex = 2;
			this.m_lblEntrySortHint.Text = "Entries are sorted within their groups, i.e. the group structure is not broken up" +
				".";
			// 
			// m_cmbSortEntries
			// 
			this.m_cmbSortEntries.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbSortEntries.FormattingEnabled = true;
			this.m_cmbSortEntries.Location = new System.Drawing.Point(111, 18);
			this.m_cmbSortEntries.Name = "m_cmbSortEntries";
			this.m_cmbSortEntries.Size = new System.Drawing.Size(161, 21);
			this.m_cmbSortEntries.TabIndex = 1;
			// 
			// m_lblSortEntries
			// 
			this.m_lblSortEntries.AutoSize = true;
			this.m_lblSortEntries.Location = new System.Drawing.Point(6, 21);
			this.m_lblSortEntries.Name = "m_lblSortEntries";
			this.m_lblSortEntries.Size = new System.Drawing.Size(99, 13);
			this.m_lblSortEntries.TabIndex = 0;
			this.m_lblSortEntries.Text = "S&ort entries by field:";
			// 
			// m_grpStyle
			// 
			this.m_grpStyle.Controls.Add(this.m_lblColorPO);
			this.m_grpStyle.Controls.Add(this.m_lblColorPD);
			this.m_grpStyle.Controls.Add(this.m_lblColorPL);
			this.m_grpStyle.Controls.Add(this.m_lblColorPU);
			this.m_grpStyle.Controls.Add(this.m_btnColorPO);
			this.m_grpStyle.Controls.Add(this.m_btnColorPD);
			this.m_grpStyle.Controls.Add(this.m_btnColorPL);
			this.m_grpStyle.Controls.Add(this.m_btnColorPU);
			this.m_grpStyle.Controls.Add(this.m_cbColorP);
			this.m_grpStyle.Controls.Add(this.m_btnPasswordFont);
			this.m_grpStyle.Controls.Add(this.m_btnMainFont);
			this.m_grpStyle.Controls.Add(this.m_cbPasswordFont);
			this.m_grpStyle.Controls.Add(this.m_cbMainFont);
			this.m_grpStyle.Location = new System.Drawing.Point(10, 240);
			this.m_grpStyle.Name = "m_grpStyle";
			this.m_grpStyle.Size = new System.Drawing.Size(283, 187);
			this.m_grpStyle.TabIndex = 2;
			this.m_grpStyle.TabStop = false;
			this.m_grpStyle.Text = "Style";
			// 
			// m_lblColorPO
			// 
			this.m_lblColorPO.AutoSize = true;
			this.m_lblColorPO.Location = new System.Drawing.Point(215, 38);
			this.m_lblColorPO.Name = "m_lblColorPO";
			this.m_lblColorPO.Size = new System.Drawing.Size(19, 13);
			this.m_lblColorPO.TabIndex = 7;
			this.m_lblColorPO.Text = "<>";
			// 
			// m_lblColorPD
			// 
			this.m_lblColorPD.AutoSize = true;
			this.m_lblColorPD.Location = new System.Drawing.Point(152, 38);
			this.m_lblColorPD.Name = "m_lblColorPD";
			this.m_lblColorPD.Size = new System.Drawing.Size(19, 13);
			this.m_lblColorPD.TabIndex = 5;
			this.m_lblColorPD.Text = "<>";
			// 
			// m_lblColorPL
			// 
			this.m_lblColorPL.AutoSize = true;
			this.m_lblColorPL.Location = new System.Drawing.Point(89, 38);
			this.m_lblColorPL.Name = "m_lblColorPL";
			this.m_lblColorPL.Size = new System.Drawing.Size(19, 13);
			this.m_lblColorPL.TabIndex = 3;
			this.m_lblColorPL.Text = "<>";
			// 
			// m_lblColorPU
			// 
			this.m_lblColorPU.AutoSize = true;
			this.m_lblColorPU.Location = new System.Drawing.Point(26, 38);
			this.m_lblColorPU.Name = "m_lblColorPU";
			this.m_lblColorPU.Size = new System.Drawing.Size(19, 13);
			this.m_lblColorPU.TabIndex = 1;
			this.m_lblColorPU.Text = "<>";
			// 
			// m_btnColorPO
			// 
			this.m_btnColorPO.Location = new System.Drawing.Point(218, 56);
			this.m_btnColorPO.Name = "m_btnColorPO";
			this.m_btnColorPO.Size = new System.Drawing.Size(56, 23);
			this.m_btnColorPO.TabIndex = 8;
			this.m_btnColorPO.UseVisualStyleBackColor = true;
			// 
			// m_btnColorPD
			// 
			this.m_btnColorPD.Location = new System.Drawing.Point(155, 56);
			this.m_btnColorPD.Name = "m_btnColorPD";
			this.m_btnColorPD.Size = new System.Drawing.Size(57, 23);
			this.m_btnColorPD.TabIndex = 6;
			this.m_btnColorPD.UseVisualStyleBackColor = true;
			// 
			// m_btnColorPL
			// 
			this.m_btnColorPL.Location = new System.Drawing.Point(92, 56);
			this.m_btnColorPL.Name = "m_btnColorPL";
			this.m_btnColorPL.Size = new System.Drawing.Size(57, 23);
			this.m_btnColorPL.TabIndex = 4;
			this.m_btnColorPL.UseVisualStyleBackColor = true;
			// 
			// m_btnColorPU
			// 
			this.m_btnColorPU.Location = new System.Drawing.Point(29, 56);
			this.m_btnColorPU.Name = "m_btnColorPU";
			this.m_btnColorPU.Size = new System.Drawing.Size(57, 23);
			this.m_btnColorPU.TabIndex = 2;
			this.m_btnColorPU.UseVisualStyleBackColor = true;
			// 
			// m_cbColorP
			// 
			this.m_cbColorP.AutoSize = true;
			this.m_cbColorP.Location = new System.Drawing.Point(10, 20);
			this.m_cbColorP.Name = "m_cbColorP";
			this.m_cbColorP.Size = new System.Drawing.Size(167, 17);
			this.m_cbColorP.TabIndex = 0;
			this.m_cbColorP.Text = "Colori&ze password characters:";
			this.m_cbColorP.UseVisualStyleBackColor = true;
			this.m_cbColorP.CheckedChanged += new System.EventHandler(this.OnColorPCheckedChanged);
			// 
			// m_btnPasswordFont
			// 
			this.m_btnPasswordFont.Location = new System.Drawing.Point(29, 152);
			this.m_btnPasswordFont.Name = "m_btnPasswordFont";
			this.m_btnPasswordFont.Size = new System.Drawing.Size(245, 23);
			this.m_btnPasswordFont.TabIndex = 12;
			this.m_btnPasswordFont.Text = "<>";
			this.m_btnPasswordFont.UseVisualStyleBackColor = true;
			// 
			// m_btnMainFont
			// 
			this.m_btnMainFont.Location = new System.Drawing.Point(29, 104);
			this.m_btnMainFont.Name = "m_btnMainFont";
			this.m_btnMainFont.Size = new System.Drawing.Size(245, 23);
			this.m_btnMainFont.TabIndex = 10;
			this.m_btnMainFont.Text = "<>";
			this.m_btnMainFont.UseVisualStyleBackColor = true;
			// 
			// m_cbPasswordFont
			// 
			this.m_cbPasswordFont.AutoSize = true;
			this.m_cbPasswordFont.Location = new System.Drawing.Point(10, 133);
			this.m_cbPasswordFont.Name = "m_cbPasswordFont";
			this.m_cbPasswordFont.Size = new System.Drawing.Size(133, 17);
			this.m_cbPasswordFont.TabIndex = 11;
			this.m_cbPasswordFont.Text = "Custom passwo&rd font:";
			this.m_cbPasswordFont.UseVisualStyleBackColor = true;
			// 
			// m_cbMainFont
			// 
			this.m_cbMainFont.AutoSize = true;
			this.m_cbMainFont.Location = new System.Drawing.Point(10, 85);
			this.m_cbMainFont.Name = "m_cbMainFont";
			this.m_cbMainFont.Size = new System.Drawing.Size(110, 17);
			this.m_cbMainFont.TabIndex = 9;
			this.m_cbMainFont.Text = "Custom main font:";
			this.m_cbMainFont.UseVisualStyleBackColor = true;
			// 
			// m_grpFields
			// 
			this.m_grpFields.Controls.Add(this.m_cbIcon);
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
			this.m_grpFields.Location = new System.Drawing.Point(10, 119);
			this.m_grpFields.Name = "m_grpFields";
			this.m_grpFields.Size = new System.Drawing.Size(571, 115);
			this.m_grpFields.TabIndex = 1;
			this.m_grpFields.TabStop = false;
			this.m_grpFields.Text = "Fields";
			// 
			// m_cbIcon
			// 
			this.m_cbIcon.AutoSize = true;
			this.m_cbIcon.Location = new System.Drawing.Point(10, 66);
			this.m_cbIcon.Name = "m_cbIcon";
			this.m_cbIcon.Size = new System.Drawing.Size(47, 17);
			this.m_cbIcon.TabIndex = 10;
			this.m_cbIcon.Text = "&Icon";
			this.m_cbIcon.UseVisualStyleBackColor = true;
			this.m_cbIcon.CheckedChanged += new System.EventHandler(this.OnIconCheckedChanged);
			// 
			// m_cbUuid
			// 
			this.m_cbUuid.AutoSize = true;
			this.m_cbUuid.Location = new System.Drawing.Point(360, 66);
			this.m_cbUuid.Name = "m_cbUuid";
			this.m_cbUuid.Size = new System.Drawing.Size(53, 17);
			this.m_cbUuid.TabIndex = 13;
			this.m_cbUuid.Text = "UUI&D";
			this.m_cbUuid.UseVisualStyleBackColor = true;
			// 
			// m_cbTags
			// 
			this.m_cbTags.AutoSize = true;
			this.m_cbTags.Location = new System.Drawing.Point(464, 43);
			this.m_cbTags.Name = "m_cbTags";
			this.m_cbTags.Size = new System.Drawing.Size(50, 17);
			this.m_cbTags.TabIndex = 9;
			this.m_cbTags.Text = "T&ags";
			this.m_cbTags.UseVisualStyleBackColor = true;
			// 
			// m_cbCustomStrings
			// 
			this.m_cbCustomStrings.AutoSize = true;
			this.m_cbCustomStrings.Location = new System.Drawing.Point(107, 66);
			this.m_cbCustomStrings.Name = "m_cbCustomStrings";
			this.m_cbCustomStrings.Size = new System.Drawing.Size(116, 17);
			this.m_cbCustomStrings.TabIndex = 11;
			this.m_cbCustomStrings.Text = "Custom string &fields";
			this.m_cbCustomStrings.UseVisualStyleBackColor = true;
			// 
			// m_cbGroups
			// 
			this.m_cbGroups.AutoSize = true;
			this.m_cbGroups.Location = new System.Drawing.Point(245, 66);
			this.m_cbGroups.Name = "m_cbGroups";
			this.m_cbGroups.Size = new System.Drawing.Size(84, 17);
			this.m_cbGroups.TabIndex = 12;
			this.m_cbGroups.Text = "&Group name";
			this.m_cbGroups.UseVisualStyleBackColor = true;
			// 
			// m_linkDeselectAllFields
			// 
			this.m_linkDeselectAllFields.AutoSize = true;
			this.m_linkDeselectAllFields.Location = new System.Drawing.Point(63, 90);
			this.m_linkDeselectAllFields.Name = "m_linkDeselectAllFields";
			this.m_linkDeselectAllFields.Size = new System.Drawing.Size(63, 13);
			this.m_linkDeselectAllFields.TabIndex = 15;
			this.m_linkDeselectAllFields.TabStop = true;
			this.m_linkDeselectAllFields.Text = "Deselect All";
			this.m_linkDeselectAllFields.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnLinkDeselectAllFields);
			// 
			// m_linkSelectAllFields
			// 
			this.m_linkSelectAllFields.AutoSize = true;
			this.m_linkSelectAllFields.Location = new System.Drawing.Point(6, 90);
			this.m_linkSelectAllFields.Name = "m_linkSelectAllFields";
			this.m_linkSelectAllFields.Size = new System.Drawing.Size(51, 13);
			this.m_linkSelectAllFields.TabIndex = 14;
			this.m_linkSelectAllFields.TabStop = true;
			this.m_linkSelectAllFields.Text = "Select All";
			this.m_linkSelectAllFields.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnLinkSelectAllFields);
			// 
			// m_cbAutoType
			// 
			this.m_cbAutoType.AutoSize = true;
			this.m_cbAutoType.Location = new System.Drawing.Point(360, 43);
			this.m_cbAutoType.Name = "m_cbAutoType";
			this.m_cbAutoType.Size = new System.Drawing.Size(75, 17);
			this.m_cbAutoType.TabIndex = 8;
			this.m_cbAutoType.Text = "Auto-Typ&e";
			this.m_cbAutoType.UseVisualStyleBackColor = true;
			// 
			// m_cbLastMod
			// 
			this.m_cbLastMod.AutoSize = true;
			this.m_cbLastMod.Location = new System.Drawing.Point(107, 43);
			this.m_cbLastMod.Name = "m_cbLastMod";
			this.m_cbLastMod.Size = new System.Drawing.Size(127, 17);
			this.m_cbLastMod.TabIndex = 6;
			this.m_cbLastMod.Text = "Last &modification time";
			this.m_cbLastMod.UseVisualStyleBackColor = true;
			// 
			// m_cbCreation
			// 
			this.m_cbCreation.AutoSize = true;
			this.m_cbCreation.Location = new System.Drawing.Point(10, 43);
			this.m_cbCreation.Name = "m_cbCreation";
			this.m_cbCreation.Size = new System.Drawing.Size(87, 17);
			this.m_cbCreation.TabIndex = 5;
			this.m_cbCreation.Text = "&Creation time";
			this.m_cbCreation.UseVisualStyleBackColor = true;
			// 
			// m_cbExpire
			// 
			this.m_cbExpire.AutoSize = true;
			this.m_cbExpire.Location = new System.Drawing.Point(245, 43);
			this.m_cbExpire.Name = "m_cbExpire";
			this.m_cbExpire.Size = new System.Drawing.Size(76, 17);
			this.m_cbExpire.TabIndex = 7;
			this.m_cbExpire.Text = "E&xpiry time";
			this.m_cbExpire.UseVisualStyleBackColor = true;
			// 
			// m_cbNotes
			// 
			this.m_cbNotes.AutoSize = true;
			this.m_cbNotes.Checked = true;
			this.m_cbNotes.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_cbNotes.Location = new System.Drawing.Point(464, 20);
			this.m_cbNotes.Name = "m_cbNotes";
			this.m_cbNotes.Size = new System.Drawing.Size(54, 17);
			this.m_cbNotes.TabIndex = 4;
			this.m_cbNotes.Text = "&Notes";
			this.m_cbNotes.UseVisualStyleBackColor = true;
			// 
			// m_cbPassword
			// 
			this.m_cbPassword.AutoSize = true;
			this.m_cbPassword.Checked = true;
			this.m_cbPassword.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_cbPassword.Location = new System.Drawing.Point(245, 20);
			this.m_cbPassword.Name = "m_cbPassword";
			this.m_cbPassword.Size = new System.Drawing.Size(72, 17);
			this.m_cbPassword.TabIndex = 2;
			this.m_cbPassword.Text = "Pass&word";
			this.m_cbPassword.UseVisualStyleBackColor = true;
			// 
			// m_cbUrl
			// 
			this.m_cbUrl.AutoSize = true;
			this.m_cbUrl.Location = new System.Drawing.Point(360, 20);
			this.m_cbUrl.Name = "m_cbUrl";
			this.m_cbUrl.Size = new System.Drawing.Size(48, 17);
			this.m_cbUrl.TabIndex = 3;
			this.m_cbUrl.Text = "UR&L";
			this.m_cbUrl.UseVisualStyleBackColor = true;
			// 
			// m_cbUser
			// 
			this.m_cbUser.AutoSize = true;
			this.m_cbUser.Checked = true;
			this.m_cbUser.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_cbUser.Location = new System.Drawing.Point(107, 20);
			this.m_cbUser.Name = "m_cbUser";
			this.m_cbUser.Size = new System.Drawing.Size(77, 17);
			this.m_cbUser.TabIndex = 1;
			this.m_cbUser.Text = "&User name";
			this.m_cbUser.UseVisualStyleBackColor = true;
			// 
			// m_cbTitle
			// 
			this.m_cbTitle.AutoSize = true;
			this.m_cbTitle.Checked = true;
			this.m_cbTitle.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_cbTitle.Location = new System.Drawing.Point(10, 20);
			this.m_cbTitle.Name = "m_cbTitle";
			this.m_cbTitle.Size = new System.Drawing.Size(46, 17);
			this.m_cbTitle.TabIndex = 0;
			this.m_cbTitle.Text = "&Title";
			this.m_cbTitle.UseVisualStyleBackColor = true;
			// 
			// m_grpLayout
			// 
			this.m_grpLayout.Controls.Add(this.m_lblLayBlocks);
			this.m_grpLayout.Controls.Add(this.m_lblLayTable);
			this.m_grpLayout.Controls.Add(this.m_rbLayBlocks);
			this.m_grpLayout.Controls.Add(this.m_rbLayTable);
			this.m_grpLayout.Location = new System.Drawing.Point(10, 10);
			this.m_grpLayout.Name = "m_grpLayout";
			this.m_grpLayout.Size = new System.Drawing.Size(571, 103);
			this.m_grpLayout.TabIndex = 0;
			this.m_grpLayout.TabStop = false;
			this.m_grpLayout.Text = "Layout";
			// 
			// m_lblLayBlocks
			// 
			this.m_lblLayBlocks.AutoSize = true;
			this.m_lblLayBlocks.Location = new System.Drawing.Point(26, 78);
			this.m_lblLayBlocks.Name = "m_lblLayBlocks";
			this.m_lblLayBlocks.Size = new System.Drawing.Size(315, 13);
			this.m_lblLayBlocks.TabIndex = 3;
			this.m_lblLayBlocks.Text = "Arrange entry fields in block form. One or more lines for each field.";
			// 
			// m_lblLayTable
			// 
			this.m_lblLayTable.AutoSize = true;
			this.m_lblLayTable.Location = new System.Drawing.Point(26, 38);
			this.m_lblLayTable.Name = "m_lblLayTable";
			this.m_lblLayTable.Size = new System.Drawing.Size(296, 13);
			this.m_lblLayTable.TabIndex = 1;
			this.m_lblLayTable.Text = "Arrange entry fields in tabular form. One column for each field.";
			// 
			// m_rbLayBlocks
			// 
			this.m_rbLayBlocks.AutoSize = true;
			this.m_rbLayBlocks.Location = new System.Drawing.Point(10, 59);
			this.m_rbLayBlocks.Name = "m_rbLayBlocks";
			this.m_rbLayBlocks.Size = new System.Drawing.Size(57, 17);
			this.m_rbLayBlocks.TabIndex = 2;
			this.m_rbLayBlocks.TabStop = true;
			this.m_rbLayBlocks.Text = "Bloc&ks";
			this.m_rbLayBlocks.UseVisualStyleBackColor = true;
			// 
			// m_rbLayTable
			// 
			this.m_rbLayTable.AutoSize = true;
			this.m_rbLayTable.Location = new System.Drawing.Point(10, 19);
			this.m_rbLayTable.Name = "m_rbLayTable";
			this.m_rbLayTable.Size = new System.Drawing.Size(57, 17);
			this.m_rbLayTable.TabIndex = 0;
			this.m_rbLayTable.TabStop = true;
			this.m_rbLayTable.Text = "Ta&bles";
			this.m_rbLayTable.UseVisualStyleBackColor = true;
			this.m_rbLayTable.CheckedChanged += new System.EventHandler(this.OnTabularCheckedChanged);
			// 
			// m_bannerImage
			// 
			this.m_bannerImage.Dock = System.Windows.Forms.DockStyle.Top;
			this.m_bannerImage.Location = new System.Drawing.Point(0, 0);
			this.m_bannerImage.Name = "m_bannerImage";
			this.m_bannerImage.Size = new System.Drawing.Size(625, 60);
			this.m_bannerImage.TabIndex = 1;
			this.m_bannerImage.TabStop = false;
			// 
			// m_btnOK
			// 
			this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_btnOK.Location = new System.Drawing.Point(457, 537);
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.Size = new System.Drawing.Size(75, 23);
			this.m_btnOK.TabIndex = 0;
			this.m_btnOK.Text = "&Print...";
			this.m_btnOK.UseVisualStyleBackColor = true;
			this.m_btnOK.Click += new System.EventHandler(this.OnBtnOK);
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_btnCancel.Location = new System.Drawing.Point(538, 537);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 1;
			this.m_btnCancel.Text = "Cancel";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			this.m_btnCancel.Click += new System.EventHandler(this.OnBtnCancel);
			// 
			// m_btnConfigPrinter
			// 
			this.m_btnConfigPrinter.Location = new System.Drawing.Point(12, 537);
			this.m_btnConfigPrinter.Name = "m_btnConfigPrinter";
			this.m_btnConfigPrinter.Size = new System.Drawing.Size(100, 23);
			this.m_btnConfigPrinter.TabIndex = 3;
			this.m_btnConfigPrinter.Text = "Page &Setup";
			this.m_btnConfigPrinter.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.m_btnConfigPrinter.UseVisualStyleBackColor = true;
			this.m_btnConfigPrinter.Click += new System.EventHandler(this.OnBtnConfigPage);
			// 
			// m_btnPrintPreview
			// 
			this.m_btnPrintPreview.Location = new System.Drawing.Point(118, 537);
			this.m_btnPrintPreview.Name = "m_btnPrintPreview";
			this.m_btnPrintPreview.Size = new System.Drawing.Size(100, 23);
			this.m_btnPrintPreview.TabIndex = 4;
			this.m_btnPrintPreview.Text = "Print Pre&view";
			this.m_btnPrintPreview.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.m_btnPrintPreview.UseVisualStyleBackColor = true;
			this.m_btnPrintPreview.Click += new System.EventHandler(this.OnBtnPrintPreview);
			// 
			// PrintForm
			// 
			this.AcceptButton = this.m_btnOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(625, 572);
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_tabMain);
			this.Controls.Add(this.m_btnOK);
			this.Controls.Add(this.m_btnPrintPreview);
			this.Controls.Add(this.m_btnConfigPrinter);
			this.Controls.Add(this.m_bannerImage);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(64, 32);
			this.Name = "PrintForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Print";
			this.Load += new System.EventHandler(this.OnFormLoad);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
			this.m_tabMain.ResumeLayout(false);
			this.m_tabPreview.ResumeLayout(false);
			this.m_tabDataLayout.ResumeLayout(false);
			this.m_grpAdv.ResumeLayout(false);
			this.m_grpAdv.PerformLayout();
			this.m_grpSorting.ResumeLayout(false);
			this.m_grpSorting.PerformLayout();
			this.m_grpStyle.ResumeLayout(false);
			this.m_grpStyle.PerformLayout();
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
		private System.Windows.Forms.RadioButton m_rbLayTable;
		private System.Windows.Forms.RadioButton m_rbLayBlocks;
		private System.Windows.Forms.Label m_lblLayBlocks;
		private System.Windows.Forms.Label m_lblLayTable;
		private System.Windows.Forms.GroupBox m_grpFields;
		private System.Windows.Forms.CheckBox m_cbNotes;
		private System.Windows.Forms.CheckBox m_cbPassword;
		private System.Windows.Forms.CheckBox m_cbUrl;
		private System.Windows.Forms.CheckBox m_cbUser;
		private System.Windows.Forms.CheckBox m_cbTitle;
		private System.Windows.Forms.Label m_lblPreviewHint;
		private System.Windows.Forms.GroupBox m_grpStyle;
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
		private System.Windows.Forms.GroupBox m_grpAdv;
		private System.Windows.Forms.CheckBox m_cbIcon;
		private System.Windows.Forms.ComboBox m_cmbSpr;
		private System.Windows.Forms.Label m_lblSpr;
		private System.Windows.Forms.CheckBox m_cbMainFont;
		private System.Windows.Forms.CheckBox m_cbPasswordFont;
		private System.Windows.Forms.Button m_btnPasswordFont;
		private System.Windows.Forms.Button m_btnMainFont;
		private System.Windows.Forms.CheckBox m_cbColorP;
		private KeePass.UI.ColorButtonEx m_btnColorPU;
		private KeePass.UI.ColorButtonEx m_btnColorPO;
		private KeePass.UI.ColorButtonEx m_btnColorPD;
		private KeePass.UI.ColorButtonEx m_btnColorPL;
		private System.Windows.Forms.Label m_lblColorPU;
		private System.Windows.Forms.Label m_lblColorPO;
		private System.Windows.Forms.Label m_lblColorPD;
		private System.Windows.Forms.Label m_lblColorPL;
	}
}