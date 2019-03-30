namespace KeePass.Forms
{
	partial class CsvImportForm
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
            this.m_tabMain = new System.Windows.Forms.TabControl();
            this.m_tabEnc = new System.Windows.Forms.TabPage();
            this.m_rtbEncPreview = new KeePass.UI.CustomRichTextBoxEx();
            this.m_lblEncPreview = new System.Windows.Forms.Label();
            this.m_cmbEnc = new System.Windows.Forms.ComboBox();
            this.m_lblEnc = new System.Windows.Forms.Label();
            this.m_tabStructure = new System.Windows.Forms.TabPage();
            this.m_grpSem = new System.Windows.Forms.GroupBox();
            this.m_grpFieldAdd = new System.Windows.Forms.GroupBox();
            this.m_btnFieldAdd = new System.Windows.Forms.Button();
            this.m_linkFieldFormat = new System.Windows.Forms.LinkLabel();
            this.m_cmbFieldFormat = new System.Windows.Forms.ComboBox();
            this.m_lblFieldFormat = new System.Windows.Forms.Label();
            this.m_tbFieldName = new System.Windows.Forms.TextBox();
            this.m_lblFieldName = new System.Windows.Forms.Label();
            this.m_cmbFieldType = new System.Windows.Forms.ComboBox();
            this.m_lblFieldType = new System.Windows.Forms.Label();
            this.m_btnFieldMoveDown = new System.Windows.Forms.Button();
            this.m_btnFieldMoveUp = new System.Windows.Forms.Button();
            this.m_btnFieldDel = new System.Windows.Forms.Button();
            this.m_lblFields = new System.Windows.Forms.Label();
            this.m_lvFields = new KeePass.UI.CustomListViewEx();
            this.m_grpSyntax = new System.Windows.Forms.GroupBox();
            this.m_cbIgnoreFirst = new System.Windows.Forms.CheckBox();
            this.m_cbTrim = new System.Windows.Forms.CheckBox();
            this.m_cmbTextQual = new System.Windows.Forms.ComboBox();
            this.m_lblTextQual = new System.Windows.Forms.Label();
            this.m_cbBackEscape = new System.Windows.Forms.CheckBox();
            this.m_lblFieldSep = new System.Windows.Forms.Label();
            this.m_cmbFieldSep = new System.Windows.Forms.ComboBox();
            this.m_cmbRecSep = new System.Windows.Forms.ComboBox();
            this.m_lblRecSep = new System.Windows.Forms.Label();
            this.m_tabPreview = new System.Windows.Forms.TabPage();
            this.m_cbMergeGroups = new System.Windows.Forms.CheckBox();
            this.m_lvImportPreview = new KeePass.UI.CustomListViewEx();
            this.m_btnOK = new System.Windows.Forms.Button();
            this.m_btnCancel = new System.Windows.Forms.Button();
            this.m_btnTabBack = new System.Windows.Forms.Button();
            this.m_btnTabNext = new System.Windows.Forms.Button();
            this.m_btnHelp = new System.Windows.Forms.Button();
            this.m_tabMain.SuspendLayout();
            this.m_tabEnc.SuspendLayout();
            this.m_tabStructure.SuspendLayout();
            this.m_grpSem.SuspendLayout();
            this.m_grpFieldAdd.SuspendLayout();
            this.m_grpSyntax.SuspendLayout();
            this.m_tabPreview.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_tabMain
            // 
            this.m_tabMain.Controls.Add(this.m_tabEnc);
            this.m_tabMain.Controls.Add(this.m_tabStructure);
            this.m_tabMain.Controls.Add(this.m_tabPreview);
            this.m_tabMain.Location = new System.Drawing.Point(16, 19);
            this.m_tabMain.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_tabMain.Name = "m_tabMain";
            this.m_tabMain.SelectedIndex = 0;
            this.m_tabMain.Size = new System.Drawing.Size(912, 711);
            this.m_tabMain.TabIndex = 2;
            this.m_tabMain.SelectedIndexChanged += new System.EventHandler(this.OnTabMainSelectedIndexChanged);
            // 
            // m_tabEnc
            // 
            this.m_tabEnc.Controls.Add(this.m_rtbEncPreview);
            this.m_tabEnc.Controls.Add(this.m_lblEncPreview);
            this.m_tabEnc.Controls.Add(this.m_cmbEnc);
            this.m_tabEnc.Controls.Add(this.m_lblEnc);
            this.m_tabEnc.Location = new System.Drawing.Point(4, 29);
            this.m_tabEnc.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_tabEnc.Name = "m_tabEnc";
            this.m_tabEnc.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_tabEnc.Size = new System.Drawing.Size(904, 678);
            this.m_tabEnc.TabIndex = 0;
            this.m_tabEnc.Text = "Encoding";
            this.m_tabEnc.UseVisualStyleBackColor = true;
            // 
            // m_rtbEncPreview
            // 
            this.m_rtbEncPreview.AcceptsTab = true;
            this.m_rtbEncPreview.DetectUrls = false;
            this.m_rtbEncPreview.Location = new System.Drawing.Point(12, 92);
            this.m_rtbEncPreview.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_rtbEncPreview.Name = "m_rtbEncPreview";
            this.m_rtbEncPreview.ReadOnly = true;
            this.m_rtbEncPreview.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.m_rtbEncPreview.Size = new System.Drawing.Size(875, 562);
            this.m_rtbEncPreview.TabIndex = 3;
            this.m_rtbEncPreview.Text = "";
            // 
            // m_lblEncPreview
            // 
            this.m_lblEncPreview.AutoSize = true;
            this.m_lblEncPreview.Location = new System.Drawing.Point(8, 68);
            this.m_lblEncPreview.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblEncPreview.Name = "m_lblEncPreview";
            this.m_lblEncPreview.Size = new System.Drawing.Size(95, 20);
            this.m_lblEncPreview.TabIndex = 2;
            this.m_lblEncPreview.Text = "Text preview:";
            // 
            // m_cmbEnc
            // 
            this.m_cmbEnc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_cmbEnc.FormattingEnabled = true;
            this.m_cmbEnc.Location = new System.Drawing.Point(121, 20);
            this.m_cmbEnc.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cmbEnc.Name = "m_cmbEnc";
            this.m_cmbEnc.Size = new System.Drawing.Size(396, 28);
            this.m_cmbEnc.TabIndex = 1;
            this.m_cmbEnc.SelectedIndexChanged += new System.EventHandler(this.OnEncSelectedIndexChanged);
            // 
            // m_lblEnc
            // 
            this.m_lblEnc.AutoSize = true;
            this.m_lblEnc.Location = new System.Drawing.Point(8, 25);
            this.m_lblEnc.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblEnc.Name = "m_lblEnc";
            this.m_lblEnc.Size = new System.Drawing.Size(105, 20);
            this.m_lblEnc.TabIndex = 0;
            this.m_lblEnc.Text = "Text encoding:";
            // 
            // m_tabStructure
            // 
            this.m_tabStructure.Controls.Add(this.m_grpSem);
            this.m_tabStructure.Controls.Add(this.m_grpSyntax);
            this.m_tabStructure.Location = new System.Drawing.Point(4, 25);
            this.m_tabStructure.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_tabStructure.Name = "m_tabStructure";
            this.m_tabStructure.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_tabStructure.Size = new System.Drawing.Size(904, 682);
            this.m_tabStructure.TabIndex = 1;
            this.m_tabStructure.Text = "Structure";
            this.m_tabStructure.UseVisualStyleBackColor = true;
            // 
            // m_grpSem
            // 
            this.m_grpSem.Controls.Add(this.m_grpFieldAdd);
            this.m_grpSem.Controls.Add(this.m_btnFieldMoveDown);
            this.m_grpSem.Controls.Add(this.m_btnFieldMoveUp);
            this.m_grpSem.Controls.Add(this.m_btnFieldDel);
            this.m_grpSem.Controls.Add(this.m_lblFields);
            this.m_grpSem.Controls.Add(this.m_lvFields);
            this.m_grpSem.Location = new System.Drawing.Point(8, 215);
            this.m_grpSem.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpSem.Name = "m_grpSem";
            this.m_grpSem.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpSem.Size = new System.Drawing.Size(883, 446);
            this.m_grpSem.TabIndex = 1;
            this.m_grpSem.TabStop = false;
            this.m_grpSem.Text = "Semantics";
            // 
            // m_grpFieldAdd
            // 
            this.m_grpFieldAdd.Controls.Add(this.m_btnFieldAdd);
            this.m_grpFieldAdd.Controls.Add(this.m_linkFieldFormat);
            this.m_grpFieldAdd.Controls.Add(this.m_cmbFieldFormat);
            this.m_grpFieldAdd.Controls.Add(this.m_lblFieldFormat);
            this.m_grpFieldAdd.Controls.Add(this.m_tbFieldName);
            this.m_grpFieldAdd.Controls.Add(this.m_lblFieldName);
            this.m_grpFieldAdd.Controls.Add(this.m_cmbFieldType);
            this.m_grpFieldAdd.Controls.Add(this.m_lblFieldType);
            this.m_grpFieldAdd.Location = new System.Drawing.Point(507, 220);
            this.m_grpFieldAdd.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpFieldAdd.Name = "m_grpFieldAdd";
            this.m_grpFieldAdd.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpFieldAdd.Size = new System.Drawing.Size(363, 211);
            this.m_grpFieldAdd.TabIndex = 5;
            this.m_grpFieldAdd.TabStop = false;
            this.m_grpFieldAdd.Text = "Add field";
            // 
            // m_btnFieldAdd
            // 
            this.m_btnFieldAdd.Location = new System.Drawing.Point(251, 159);
            this.m_btnFieldAdd.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnFieldAdd.Name = "m_btnFieldAdd";
            this.m_btnFieldAdd.Size = new System.Drawing.Size(100, 35);
            this.m_btnFieldAdd.TabIndex = 7;
            this.m_btnFieldAdd.Text = "&Add";
            this.m_btnFieldAdd.UseVisualStyleBackColor = true;
            this.m_btnFieldAdd.Click += new System.EventHandler(this.OnBtnFieldAdd);
            // 
            // m_linkFieldFormat
            // 
            this.m_linkFieldFormat.AutoSize = true;
            this.m_linkFieldFormat.Location = new System.Drawing.Point(312, 115);
            this.m_linkFieldFormat.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_linkFieldFormat.Name = "m_linkFieldFormat";
            this.m_linkFieldFormat.Size = new System.Drawing.Size(41, 20);
            this.m_linkFieldFormat.TabIndex = 6;
            this.m_linkFieldFormat.TabStop = true;
            this.m_linkFieldFormat.Text = "Help";
            this.m_linkFieldFormat.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnFieldFormatLinkClicked);
            // 
            // m_cmbFieldFormat
            // 
            this.m_cmbFieldFormat.FormattingEnabled = true;
            this.m_cmbFieldFormat.Location = new System.Drawing.Point(99, 111);
            this.m_cmbFieldFormat.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cmbFieldFormat.Name = "m_cmbFieldFormat";
            this.m_cmbFieldFormat.Size = new System.Drawing.Size(204, 28);
            this.m_cmbFieldFormat.TabIndex = 5;
            // 
            // m_lblFieldFormat
            // 
            this.m_lblFieldFormat.AutoSize = true;
            this.m_lblFieldFormat.Location = new System.Drawing.Point(8, 115);
            this.m_lblFieldFormat.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblFieldFormat.Name = "m_lblFieldFormat";
            this.m_lblFieldFormat.Size = new System.Drawing.Size(29, 20);
            this.m_lblFieldFormat.TabIndex = 4;
            this.m_lblFieldFormat.Text = "<>";
            // 
            // m_tbFieldName
            // 
            this.m_tbFieldName.Location = new System.Drawing.Point(99, 71);
            this.m_tbFieldName.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_tbFieldName.Name = "m_tbFieldName";
            this.m_tbFieldName.Size = new System.Drawing.Size(251, 27);
            this.m_tbFieldName.TabIndex = 3;
            // 
            // m_lblFieldName
            // 
            this.m_lblFieldName.AutoSize = true;
            this.m_lblFieldName.Location = new System.Drawing.Point(8, 75);
            this.m_lblFieldName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblFieldName.Name = "m_lblFieldName";
            this.m_lblFieldName.Size = new System.Drawing.Size(52, 20);
            this.m_lblFieldName.TabIndex = 2;
            this.m_lblFieldName.Text = "Name:";
            // 
            // m_cmbFieldType
            // 
            this.m_cmbFieldType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_cmbFieldType.FormattingEnabled = true;
            this.m_cmbFieldType.Location = new System.Drawing.Point(99, 29);
            this.m_cmbFieldType.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cmbFieldType.Name = "m_cmbFieldType";
            this.m_cmbFieldType.Size = new System.Drawing.Size(251, 28);
            this.m_cmbFieldType.TabIndex = 1;
            this.m_cmbFieldType.SelectedIndexChanged += new System.EventHandler(this.OnFieldTypeSelectedIndexChanged);
            // 
            // m_lblFieldType
            // 
            this.m_lblFieldType.AutoSize = true;
            this.m_lblFieldType.Location = new System.Drawing.Point(8, 34);
            this.m_lblFieldType.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblFieldType.Name = "m_lblFieldType";
            this.m_lblFieldType.Size = new System.Drawing.Size(43, 20);
            this.m_lblFieldType.TabIndex = 0;
            this.m_lblFieldType.Text = "Type:";
            // 
            // m_btnFieldMoveDown
            // 
            this.m_btnFieldMoveDown.Image = global::KeePass.Properties.Resources.B16x16_1DownArrow;
            this.m_btnFieldMoveDown.Location = new System.Drawing.Point(507, 148);
            this.m_btnFieldMoveDown.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnFieldMoveDown.Name = "m_btnFieldMoveDown";
            this.m_btnFieldMoveDown.Size = new System.Drawing.Size(100, 35);
            this.m_btnFieldMoveDown.TabIndex = 4;
            this.m_btnFieldMoveDown.UseVisualStyleBackColor = true;
            this.m_btnFieldMoveDown.Click += new System.EventHandler(this.OnBtnFieldMoveDown);
            // 
            // m_btnFieldMoveUp
            // 
            this.m_btnFieldMoveUp.Image = global::KeePass.Properties.Resources.B16x16_1UpArrow;
            this.m_btnFieldMoveUp.Location = new System.Drawing.Point(507, 102);
            this.m_btnFieldMoveUp.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnFieldMoveUp.Name = "m_btnFieldMoveUp";
            this.m_btnFieldMoveUp.Size = new System.Drawing.Size(100, 35);
            this.m_btnFieldMoveUp.TabIndex = 3;
            this.m_btnFieldMoveUp.UseVisualStyleBackColor = true;
            this.m_btnFieldMoveUp.Click += new System.EventHandler(this.OnBtnFieldMoveUp);
            // 
            // m_btnFieldDel
            // 
            this.m_btnFieldDel.Location = new System.Drawing.Point(507, 59);
            this.m_btnFieldDel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnFieldDel.Name = "m_btnFieldDel";
            this.m_btnFieldDel.Size = new System.Drawing.Size(100, 35);
            this.m_btnFieldDel.TabIndex = 2;
            this.m_btnFieldDel.Text = "&Delete";
            this.m_btnFieldDel.UseVisualStyleBackColor = true;
            this.m_btnFieldDel.Click += new System.EventHandler(this.OnBtnFieldDel);
            // 
            // m_lblFields
            // 
            this.m_lblFields.AutoSize = true;
            this.m_lblFields.Location = new System.Drawing.Point(8, 34);
            this.m_lblFields.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblFields.Name = "m_lblFields";
            this.m_lblFields.Size = new System.Drawing.Size(381, 20);
            this.m_lblFields.TabIndex = 0;
            this.m_lblFields.Text = "Specify the layout (fields and their order) of the CSV file:";
            // 
            // m_lvFields
            // 
            this.m_lvFields.FullRowSelect = true;
            this.m_lvFields.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.m_lvFields.HideSelection = false;
            this.m_lvFields.Location = new System.Drawing.Point(12, 59);
            this.m_lvFields.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_lvFields.Name = "m_lvFields";
            this.m_lvFields.ShowItemToolTips = true;
            this.m_lvFields.Size = new System.Drawing.Size(485, 370);
            this.m_lvFields.TabIndex = 1;
            this.m_lvFields.UseCompatibleStateImageBehavior = false;
            this.m_lvFields.View = System.Windows.Forms.View.Details;
            this.m_lvFields.SelectedIndexChanged += new System.EventHandler(this.OnFieldsSelectedIndexChanged);
            // 
            // m_grpSyntax
            // 
            this.m_grpSyntax.Controls.Add(this.m_cbIgnoreFirst);
            this.m_grpSyntax.Controls.Add(this.m_cbTrim);
            this.m_grpSyntax.Controls.Add(this.m_cmbTextQual);
            this.m_grpSyntax.Controls.Add(this.m_lblTextQual);
            this.m_grpSyntax.Controls.Add(this.m_cbBackEscape);
            this.m_grpSyntax.Controls.Add(this.m_lblFieldSep);
            this.m_grpSyntax.Controls.Add(this.m_cmbFieldSep);
            this.m_grpSyntax.Controls.Add(this.m_cmbRecSep);
            this.m_grpSyntax.Controls.Add(this.m_lblRecSep);
            this.m_grpSyntax.Location = new System.Drawing.Point(8, 20);
            this.m_grpSyntax.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpSyntax.Name = "m_grpSyntax";
            this.m_grpSyntax.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_grpSyntax.Size = new System.Drawing.Size(883, 186);
            this.m_grpSyntax.TabIndex = 0;
            this.m_grpSyntax.TabStop = false;
            this.m_grpSyntax.Text = "Syntax";
            // 
            // m_cbIgnoreFirst
            // 
            this.m_cbIgnoreFirst.AutoSize = true;
            this.m_cbIgnoreFirst.Location = new System.Drawing.Point(12, 112);
            this.m_cbIgnoreFirst.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cbIgnoreFirst.Name = "m_cbIgnoreFirst";
            this.m_cbIgnoreFirst.Size = new System.Drawing.Size(132, 24);
            this.m_cbIgnoreFirst.TabIndex = 7;
            this.m_cbIgnoreFirst.Text = "Ignore first row";
            this.m_cbIgnoreFirst.UseVisualStyleBackColor = true;
            // 
            // m_cbTrim
            // 
            this.m_cbTrim.AutoSize = true;
            this.m_cbTrim.Checked = true;
            this.m_cbTrim.CheckState = System.Windows.Forms.CheckState.Checked;
            this.m_cbTrim.Location = new System.Drawing.Point(12, 148);
            this.m_cbTrim.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cbTrim.Name = "m_cbTrim";
            this.m_cbTrim.Size = new System.Drawing.Size(459, 24);
            this.m_cbTrim.TabIndex = 8;
            this.m_cbTrim.Text = "Remove white space characters from the beginning/end of fields";
            this.m_cbTrim.UseVisualStyleBackColor = true;
            // 
            // m_cmbTextQual
            // 
            this.m_cmbTextQual.FormattingEnabled = true;
            this.m_cmbTextQual.Location = new System.Drawing.Point(121, 71);
            this.m_cmbTextQual.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cmbTextQual.Name = "m_cmbTextQual";
            this.m_cmbTextQual.Size = new System.Drawing.Size(148, 28);
            this.m_cmbTextQual.TabIndex = 5;
            this.m_cmbTextQual.SelectedIndexChanged += new System.EventHandler(this.OnTextQualSelectedIndexChanged);
            this.m_cmbTextQual.TextUpdate += new System.EventHandler(this.OnTextQualTextUpdate);
            // 
            // m_lblTextQual
            // 
            this.m_lblTextQual.AutoSize = true;
            this.m_lblTextQual.Location = new System.Drawing.Point(8, 75);
            this.m_lblTextQual.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblTextQual.Name = "m_lblTextQual";
            this.m_lblTextQual.Size = new System.Drawing.Size(98, 20);
            this.m_lblTextQual.TabIndex = 4;
            this.m_lblTextQual.Text = "Text qualifier:";
            // 
            // m_cbBackEscape
            // 
            this.m_cbBackEscape.AutoSize = true;
            this.m_cbBackEscape.Checked = true;
            this.m_cbBackEscape.CheckState = System.Windows.Forms.CheckState.Checked;
            this.m_cbBackEscape.Location = new System.Drawing.Point(331, 74);
            this.m_cbBackEscape.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cbBackEscape.Name = "m_cbBackEscape";
            this.m_cbBackEscape.Size = new System.Drawing.Size(257, 24);
            this.m_cbBackEscape.TabIndex = 6;
            this.m_cbBackEscape.Text = "Interpret \'\\\' as an escape character";
            this.m_cbBackEscape.UseVisualStyleBackColor = true;
            // 
            // m_lblFieldSep
            // 
            this.m_lblFieldSep.AutoSize = true;
            this.m_lblFieldSep.Location = new System.Drawing.Point(8, 34);
            this.m_lblFieldSep.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblFieldSep.Name = "m_lblFieldSep";
            this.m_lblFieldSep.Size = new System.Drawing.Size(111, 20);
            this.m_lblFieldSep.TabIndex = 0;
            this.m_lblFieldSep.Text = "Field separator:";
            // 
            // m_cmbFieldSep
            // 
            this.m_cmbFieldSep.FormattingEnabled = true;
            this.m_cmbFieldSep.Location = new System.Drawing.Point(121, 29);
            this.m_cmbFieldSep.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cmbFieldSep.Name = "m_cmbFieldSep";
            this.m_cmbFieldSep.Size = new System.Drawing.Size(148, 28);
            this.m_cmbFieldSep.TabIndex = 1;
            this.m_cmbFieldSep.SelectedIndexChanged += new System.EventHandler(this.OnFieldSepSelectedIndexChanged);
            this.m_cmbFieldSep.TextUpdate += new System.EventHandler(this.OnFieldSepTextUpdate);
            // 
            // m_cmbRecSep
            // 
            this.m_cmbRecSep.FormattingEnabled = true;
            this.m_cmbRecSep.Location = new System.Drawing.Point(457, 29);
            this.m_cmbRecSep.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cmbRecSep.Name = "m_cmbRecSep";
            this.m_cmbRecSep.Size = new System.Drawing.Size(148, 28);
            this.m_cmbRecSep.TabIndex = 3;
            this.m_cmbRecSep.SelectedIndexChanged += new System.EventHandler(this.OnRecSepSelectedIndexChanged);
            this.m_cmbRecSep.TextUpdate += new System.EventHandler(this.OnRecSepTextUpdate);
            // 
            // m_lblRecSep
            // 
            this.m_lblRecSep.AutoSize = true;
            this.m_lblRecSep.Location = new System.Drawing.Point(327, 34);
            this.m_lblRecSep.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.m_lblRecSep.Name = "m_lblRecSep";
            this.m_lblRecSep.Size = new System.Drawing.Size(126, 20);
            this.m_lblRecSep.TabIndex = 2;
            this.m_lblRecSep.Text = "Record separator:";
            // 
            // m_tabPreview
            // 
            this.m_tabPreview.Controls.Add(this.m_cbMergeGroups);
            this.m_tabPreview.Controls.Add(this.m_lvImportPreview);
            this.m_tabPreview.Location = new System.Drawing.Point(4, 25);
            this.m_tabPreview.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_tabPreview.Name = "m_tabPreview";
            this.m_tabPreview.Size = new System.Drawing.Size(904, 682);
            this.m_tabPreview.TabIndex = 2;
            this.m_tabPreview.Text = "Preview";
            this.m_tabPreview.UseVisualStyleBackColor = true;
            // 
            // m_cbMergeGroups
            // 
            this.m_cbMergeGroups.AutoSize = true;
            this.m_cbMergeGroups.Location = new System.Drawing.Point(8, 638);
            this.m_cbMergeGroups.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_cbMergeGroups.Name = "m_cbMergeGroups";
            this.m_cbMergeGroups.Size = new System.Drawing.Size(486, 24);
            this.m_cbMergeGroups.TabIndex = 1;
            this.m_cbMergeGroups.Text = "&Merge imported groups with groups already existing in the database";
            this.m_cbMergeGroups.UseVisualStyleBackColor = true;
            // 
            // m_lvImportPreview
            // 
            this.m_lvImportPreview.FullRowSelect = true;
            this.m_lvImportPreview.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.m_lvImportPreview.HideSelection = false;
            this.m_lvImportPreview.Location = new System.Drawing.Point(8, 20);
            this.m_lvImportPreview.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_lvImportPreview.Name = "m_lvImportPreview";
            this.m_lvImportPreview.ShowItemToolTips = true;
            this.m_lvImportPreview.Size = new System.Drawing.Size(881, 605);
            this.m_lvImportPreview.TabIndex = 0;
            this.m_lvImportPreview.UseCompatibleStateImageBehavior = false;
            this.m_lvImportPreview.View = System.Windows.Forms.View.Details;
            // 
            // m_btnOK
            // 
            this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_btnOK.Location = new System.Drawing.Point(719, 739);
            this.m_btnOK.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnOK.Name = "m_btnOK";
            this.m_btnOK.Size = new System.Drawing.Size(100, 35);
            this.m_btnOK.TabIndex = 0;
            this.m_btnOK.Text = "&Finish";
            this.m_btnOK.UseVisualStyleBackColor = true;
            this.m_btnOK.Click += new System.EventHandler(this.OnBtnOK);
            // 
            // m_btnCancel
            // 
            this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_btnCancel.Location = new System.Drawing.Point(827, 739);
            this.m_btnCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnCancel.Name = "m_btnCancel";
            this.m_btnCancel.Size = new System.Drawing.Size(100, 35);
            this.m_btnCancel.TabIndex = 1;
            this.m_btnCancel.Text = "Cancel";
            this.m_btnCancel.UseVisualStyleBackColor = true;
            // 
            // m_btnTabBack
            // 
            this.m_btnTabBack.Location = new System.Drawing.Point(511, 739);
            this.m_btnTabBack.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnTabBack.Name = "m_btnTabBack";
            this.m_btnTabBack.Size = new System.Drawing.Size(100, 35);
            this.m_btnTabBack.TabIndex = 4;
            this.m_btnTabBack.Text = "< &Back";
            this.m_btnTabBack.UseVisualStyleBackColor = true;
            this.m_btnTabBack.Click += new System.EventHandler(this.OnBtnTabBack);
            // 
            // m_btnTabNext
            // 
            this.m_btnTabNext.Location = new System.Drawing.Point(611, 739);
            this.m_btnTabNext.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnTabNext.Name = "m_btnTabNext";
            this.m_btnTabNext.Size = new System.Drawing.Size(100, 35);
            this.m_btnTabNext.TabIndex = 5;
            this.m_btnTabNext.Text = "&Next >";
            this.m_btnTabNext.UseVisualStyleBackColor = true;
            this.m_btnTabNext.Click += new System.EventHandler(this.OnBtnTabNext);
            // 
            // m_btnHelp
            // 
            this.m_btnHelp.Location = new System.Drawing.Point(15, 739);
            this.m_btnHelp.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.m_btnHelp.Name = "m_btnHelp";
            this.m_btnHelp.Size = new System.Drawing.Size(100, 35);
            this.m_btnHelp.TabIndex = 3;
            this.m_btnHelp.Text = "&Help";
            this.m_btnHelp.UseVisualStyleBackColor = true;
            this.m_btnHelp.Click += new System.EventHandler(this.OnBtnHelp);
            // 
            // CsvImportForm
            // 
            this.AcceptButton = this.m_btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.m_btnCancel;
            this.ClientSize = new System.Drawing.Size(944, 792);
            this.Controls.Add(this.m_btnHelp);
            this.Controls.Add(this.m_btnTabNext);
            this.Controls.Add(this.m_btnTabBack);
            this.Controls.Add(this.m_btnCancel);
            this.Controls.Add(this.m_btnOK);
            this.Controls.Add(this.m_tabMain);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CsvImportForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "<>";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.m_tabMain.ResumeLayout(false);
            this.m_tabEnc.ResumeLayout(false);
            this.m_tabEnc.PerformLayout();
            this.m_tabStructure.ResumeLayout(false);
            this.m_grpSem.ResumeLayout(false);
            this.m_grpSem.PerformLayout();
            this.m_grpFieldAdd.ResumeLayout(false);
            this.m_grpFieldAdd.PerformLayout();
            this.m_grpSyntax.ResumeLayout(false);
            this.m_grpSyntax.PerformLayout();
            this.m_tabPreview.ResumeLayout(false);
            this.m_tabPreview.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl m_tabMain;
		private System.Windows.Forms.TabPage m_tabEnc;
		private System.Windows.Forms.TabPage m_tabStructure;
		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.TabPage m_tabPreview;
		private System.Windows.Forms.ComboBox m_cmbEnc;
		private System.Windows.Forms.Label m_lblEnc;
		private KeePass.UI.CustomRichTextBoxEx m_rtbEncPreview;
		private System.Windows.Forms.Label m_lblEncPreview;
		private System.Windows.Forms.ComboBox m_cmbRecSep;
		private System.Windows.Forms.Label m_lblRecSep;
		private System.Windows.Forms.Label m_lblFieldSep;
		private System.Windows.Forms.ComboBox m_cmbFieldSep;
		private System.Windows.Forms.GroupBox m_grpSyntax;
		private System.Windows.Forms.GroupBox m_grpSem;
		private System.Windows.Forms.Button m_btnFieldDel;
		private System.Windows.Forms.Label m_lblFields;
		private KeePass.UI.CustomListViewEx m_lvFields;
		private System.Windows.Forms.GroupBox m_grpFieldAdd;
		private System.Windows.Forms.Label m_lblFieldFormat;
		private System.Windows.Forms.TextBox m_tbFieldName;
		private System.Windows.Forms.Label m_lblFieldName;
		private System.Windows.Forms.ComboBox m_cmbFieldType;
		private System.Windows.Forms.Label m_lblFieldType;
		private System.Windows.Forms.Button m_btnFieldMoveDown;
		private System.Windows.Forms.Button m_btnFieldMoveUp;
		private System.Windows.Forms.Button m_btnFieldAdd;
		private System.Windows.Forms.LinkLabel m_linkFieldFormat;
		private System.Windows.Forms.ComboBox m_cmbFieldFormat;
		private KeePass.UI.CustomListViewEx m_lvImportPreview;
		private System.Windows.Forms.CheckBox m_cbBackEscape;
		private System.Windows.Forms.ComboBox m_cmbTextQual;
		private System.Windows.Forms.Label m_lblTextQual;
		private System.Windows.Forms.CheckBox m_cbTrim;
		private System.Windows.Forms.Button m_btnTabBack;
		private System.Windows.Forms.Button m_btnTabNext;
		private System.Windows.Forms.CheckBox m_cbIgnoreFirst;
		private System.Windows.Forms.Button m_btnHelp;
		private System.Windows.Forms.CheckBox m_cbMergeGroups;
	}
}