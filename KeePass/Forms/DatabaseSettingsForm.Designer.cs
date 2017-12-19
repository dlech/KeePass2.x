﻿namespace KeePass.Forms
{
	partial class DatabaseSettingsForm
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
			this.components = new System.ComponentModel.Container();
			this.m_bannerImage = new System.Windows.Forms.PictureBox();
			this.m_btnOK = new System.Windows.Forms.Button();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_grpEncAlgo = new System.Windows.Forms.GroupBox();
			this.m_lblEncAlgoDesc = new System.Windows.Forms.Label();
			this.m_cmbEncAlgo = new System.Windows.Forms.ComboBox();
			this.m_lblKdfIntro = new System.Windows.Forms.Label();
			this.m_lblKdfIt = new System.Windows.Forms.Label();
			this.m_lblKdfInfo = new System.Windows.Forms.Label();
			this.m_grpKeyTrans = new System.Windows.Forms.GroupBox();
			this.m_lblKdfPar = new System.Windows.Forms.Label();
			this.m_numKdfPar = new System.Windows.Forms.NumericUpDown();
			this.m_btnKdfTest = new System.Windows.Forms.Button();
			this.m_numKdfMem = new System.Windows.Forms.NumericUpDown();
			this.m_pbKdf = new System.Windows.Forms.ProgressBar();
			this.m_btnKdf1Sec = new System.Windows.Forms.Button();
			this.m_lblKdfMem = new System.Windows.Forms.Label();
			this.m_cmbKdfMem = new System.Windows.Forms.ComboBox();
			this.m_cmbKdf = new System.Windows.Forms.ComboBox();
			this.m_lblKdf = new System.Windows.Forms.Label();
			this.m_numKdfIt = new System.Windows.Forms.NumericUpDown();
			this.m_btnHelp = new System.Windows.Forms.Button();
			this.m_ttRect = new System.Windows.Forms.ToolTip(this.components);
			this.m_lblCompressionIntro = new System.Windows.Forms.Label();
			this.m_tabMain = new System.Windows.Forms.TabControl();
			this.m_tabGeneral = new System.Windows.Forms.TabPage();
			this.m_cbColor = new System.Windows.Forms.CheckBox();
			this.m_btnColor = new System.Windows.Forms.Button();
			this.m_tbDefaultUser = new System.Windows.Forms.TextBox();
			this.m_lblDefaultUser = new System.Windows.Forms.Label();
			this.m_tbDbDesc = new KeePass.UI.PromptedTextBox();
			this.m_lblDbDesc = new System.Windows.Forms.Label();
			this.m_tbDbName = new KeePass.UI.PromptedTextBox();
			this.m_lblDbName = new System.Windows.Forms.Label();
			this.m_tabSecurity = new System.Windows.Forms.TabPage();
			this.m_lblSecIntro = new System.Windows.Forms.Label();
			this.m_tabCompression = new System.Windows.Forms.TabPage();
			this.m_lblHeaderCpAlgo = new System.Windows.Forms.Label();
			this.m_lblCpGZipPerf = new System.Windows.Forms.Label();
			this.m_lblCpGZipCp = new System.Windows.Forms.Label();
			this.m_lblCpNonePerf = new System.Windows.Forms.Label();
			this.m_lblCpNoneCp = new System.Windows.Forms.Label();
			this.m_lblHeaderPerf = new System.Windows.Forms.Label();
			this.m_lblHeaderCp = new System.Windows.Forms.Label();
			this.m_rbGZip = new System.Windows.Forms.RadioButton();
			this.m_rbNone = new System.Windows.Forms.RadioButton();
			this.m_tabRecycleBin = new System.Windows.Forms.TabPage();
			this.m_cmbRecycleBin = new System.Windows.Forms.ComboBox();
			this.m_lblRecycleBinGroup = new System.Windows.Forms.Label();
			this.m_lblRecycleBinInfo = new System.Windows.Forms.Label();
			this.m_cbRecycleBin = new System.Windows.Forms.CheckBox();
			this.m_tabAdvanced = new System.Windows.Forms.TabPage();
			this.m_grpHistory = new System.Windows.Forms.GroupBox();
			this.m_numHistoryMaxSize = new System.Windows.Forms.NumericUpDown();
			this.m_numHistoryMaxItems = new System.Windows.Forms.NumericUpDown();
			this.m_cbHistoryMaxSize = new System.Windows.Forms.CheckBox();
			this.m_cbHistoryMaxItems = new System.Windows.Forms.CheckBox();
			this.m_grpMasterKey = new System.Windows.Forms.GroupBox();
			this.m_cbKeyForceOnce = new System.Windows.Forms.CheckBox();
			this.m_cbKeyForce = new System.Windows.Forms.CheckBox();
			this.m_cbKeyRec = new System.Windows.Forms.CheckBox();
			this.m_numKeyForceDays = new System.Windows.Forms.NumericUpDown();
			this.m_numKeyRecDays = new System.Windows.Forms.NumericUpDown();
			this.m_grpTemplates = new System.Windows.Forms.GroupBox();
			this.m_cmbEntryTemplates = new System.Windows.Forms.ComboBox();
			this.m_lblTemplatesHint = new System.Windows.Forms.Label();
			this.m_lblEntryTemplatesGroup = new System.Windows.Forms.Label();
			this.m_btnCancelOp = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
			this.m_grpEncAlgo.SuspendLayout();
			this.m_grpKeyTrans.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numKdfPar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numKdfMem)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numKdfIt)).BeginInit();
			this.m_tabMain.SuspendLayout();
			this.m_tabGeneral.SuspendLayout();
			this.m_tabSecurity.SuspendLayout();
			this.m_tabCompression.SuspendLayout();
			this.m_tabRecycleBin.SuspendLayout();
			this.m_tabAdvanced.SuspendLayout();
			this.m_grpHistory.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numHistoryMaxSize)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numHistoryMaxItems)).BeginInit();
			this.m_grpMasterKey.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numKeyForceDays)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numKeyRecDays)).BeginInit();
			this.m_grpTemplates.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_bannerImage
			// 
			this.m_bannerImage.Dock = System.Windows.Forms.DockStyle.Top;
			this.m_bannerImage.Location = new System.Drawing.Point(0, 0);
			this.m_bannerImage.Name = "m_bannerImage";
			this.m_bannerImage.Size = new System.Drawing.Size(486, 60);
			this.m_bannerImage.TabIndex = 0;
			this.m_bannerImage.TabStop = false;
			// 
			// m_btnOK
			// 
			this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_btnOK.Location = new System.Drawing.Point(318, 425);
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.Size = new System.Drawing.Size(75, 23);
			this.m_btnOK.TabIndex = 2;
			this.m_btnOK.Text = "OK";
			this.m_btnOK.UseVisualStyleBackColor = true;
			this.m_btnOK.Click += new System.EventHandler(this.OnBtnOK);
			// 
			// m_btnCancel
			// 
			this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_btnCancel.Location = new System.Drawing.Point(399, 425);
			this.m_btnCancel.Name = "m_btnCancel";
			this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
			this.m_btnCancel.TabIndex = 3;
			this.m_btnCancel.Text = "Cancel";
			this.m_btnCancel.UseVisualStyleBackColor = true;
			this.m_btnCancel.Click += new System.EventHandler(this.OnBtnCancel);
			// 
			// m_grpEncAlgo
			// 
			this.m_grpEncAlgo.Controls.Add(this.m_lblEncAlgoDesc);
			this.m_grpEncAlgo.Controls.Add(this.m_cmbEncAlgo);
			this.m_grpEncAlgo.Location = new System.Drawing.Point(6, 37);
			this.m_grpEncAlgo.Name = "m_grpEncAlgo";
			this.m_grpEncAlgo.Size = new System.Drawing.Size(440, 53);
			this.m_grpEncAlgo.TabIndex = 1;
			this.m_grpEncAlgo.TabStop = false;
			this.m_grpEncAlgo.Text = "Encryption";
			// 
			// m_lblEncAlgoDesc
			// 
			this.m_lblEncAlgoDesc.AutoSize = true;
			this.m_lblEncAlgoDesc.Location = new System.Drawing.Point(6, 22);
			this.m_lblEncAlgoDesc.Name = "m_lblEncAlgoDesc";
			this.m_lblEncAlgoDesc.Size = new System.Drawing.Size(169, 13);
			this.m_lblEncAlgoDesc.TabIndex = 0;
			this.m_lblEncAlgoDesc.Text = "Database file encryption algorithm:";
			// 
			// m_cmbEncAlgo
			// 
			this.m_cmbEncAlgo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbEncAlgo.FormattingEnabled = true;
			this.m_cmbEncAlgo.Location = new System.Drawing.Point(211, 19);
			this.m_cmbEncAlgo.Name = "m_cmbEncAlgo";
			this.m_cmbEncAlgo.Size = new System.Drawing.Size(219, 21);
			this.m_cmbEncAlgo.TabIndex = 1;
			// 
			// m_lblKdfIntro
			// 
			this.m_lblKdfIntro.Location = new System.Drawing.Point(6, 18);
			this.m_lblKdfIntro.Name = "m_lblKdfIntro";
			this.m_lblKdfIntro.Size = new System.Drawing.Size(428, 28);
			this.m_lblKdfIntro.TabIndex = 0;
			this.m_lblKdfIntro.Text = "The composite master key is transformed using a key derivation function. This add" +
				"s a work factor and makes dictionary and guessing attacks harder.";
			// 
			// m_lblKdfIt
			// 
			this.m_lblKdfIt.AutoSize = true;
			this.m_lblKdfIt.Location = new System.Drawing.Point(6, 80);
			this.m_lblKdfIt.Name = "m_lblKdfIt";
			this.m_lblKdfIt.Size = new System.Drawing.Size(53, 13);
			this.m_lblKdfIt.TabIndex = 3;
			this.m_lblKdfIt.Text = "Iterations:";
			// 
			// m_lblKdfInfo
			// 
			this.m_lblKdfInfo.Location = new System.Drawing.Point(6, 155);
			this.m_lblKdfInfo.Name = "m_lblKdfInfo";
			this.m_lblKdfInfo.Size = new System.Drawing.Size(428, 28);
			this.m_lblKdfInfo.TabIndex = 10;
			this.m_lblKdfInfo.Text = "The more iterations, the harder are dictionary and guessing attacks, but also dat" +
				"abase loading/saving takes more time.";
			// 
			// m_grpKeyTrans
			// 
			this.m_grpKeyTrans.Controls.Add(this.m_lblKdfPar);
			this.m_grpKeyTrans.Controls.Add(this.m_numKdfPar);
			this.m_grpKeyTrans.Controls.Add(this.m_btnKdfTest);
			this.m_grpKeyTrans.Controls.Add(this.m_numKdfMem);
			this.m_grpKeyTrans.Controls.Add(this.m_pbKdf);
			this.m_grpKeyTrans.Controls.Add(this.m_btnKdf1Sec);
			this.m_grpKeyTrans.Controls.Add(this.m_lblKdfMem);
			this.m_grpKeyTrans.Controls.Add(this.m_cmbKdfMem);
			this.m_grpKeyTrans.Controls.Add(this.m_cmbKdf);
			this.m_grpKeyTrans.Controls.Add(this.m_lblKdf);
			this.m_grpKeyTrans.Controls.Add(this.m_numKdfIt);
			this.m_grpKeyTrans.Controls.Add(this.m_lblKdfIntro);
			this.m_grpKeyTrans.Controls.Add(this.m_lblKdfInfo);
			this.m_grpKeyTrans.Controls.Add(this.m_lblKdfIt);
			this.m_grpKeyTrans.Location = new System.Drawing.Point(6, 96);
			this.m_grpKeyTrans.Name = "m_grpKeyTrans";
			this.m_grpKeyTrans.Size = new System.Drawing.Size(440, 218);
			this.m_grpKeyTrans.TabIndex = 2;
			this.m_grpKeyTrans.TabStop = false;
			this.m_grpKeyTrans.Text = "Key transformation";
			// 
			// m_lblKdfPar
			// 
			this.m_lblKdfPar.AutoSize = true;
			this.m_lblKdfPar.Location = new System.Drawing.Point(6, 132);
			this.m_lblKdfPar.Name = "m_lblKdfPar";
			this.m_lblKdfPar.Size = new System.Drawing.Size(59, 13);
			this.m_lblKdfPar.TabIndex = 8;
			this.m_lblKdfPar.Text = "Parallelism:";
			// 
			// m_numKdfPar
			// 
			this.m_numKdfPar.Location = new System.Drawing.Point(211, 131);
			this.m_numKdfPar.Name = "m_numKdfPar";
			this.m_numKdfPar.Size = new System.Drawing.Size(219, 20);
			this.m_numKdfPar.TabIndex = 9;
			this.m_numKdfPar.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// m_btnKdfTest
			// 
			this.m_btnKdfTest.Location = new System.Drawing.Point(356, 186);
			this.m_btnKdfTest.Name = "m_btnKdfTest";
			this.m_btnKdfTest.Size = new System.Drawing.Size(75, 23);
			this.m_btnKdfTest.TabIndex = 13;
			this.m_btnKdfTest.Text = "&Test";
			this.m_btnKdfTest.UseVisualStyleBackColor = true;
			this.m_btnKdfTest.Click += new System.EventHandler(this.OnBtnKdfTest);
			// 
			// m_numKdfMem
			// 
			this.m_numKdfMem.Location = new System.Drawing.Point(211, 105);
			this.m_numKdfMem.Name = "m_numKdfMem";
			this.m_numKdfMem.Size = new System.Drawing.Size(160, 20);
			this.m_numKdfMem.TabIndex = 6;
			this.m_numKdfMem.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// m_pbKdf
			// 
			this.m_pbKdf.Location = new System.Drawing.Point(9, 190);
			this.m_pbKdf.Name = "m_pbKdf";
			this.m_pbKdf.Size = new System.Drawing.Size(195, 15);
			this.m_pbKdf.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
			this.m_pbKdf.TabIndex = 11;
			this.m_pbKdf.Visible = false;
			// 
			// m_btnKdf1Sec
			// 
			this.m_btnKdf1Sec.Location = new System.Drawing.Point(210, 186);
			this.m_btnKdf1Sec.Name = "m_btnKdf1Sec";
			this.m_btnKdf1Sec.Size = new System.Drawing.Size(140, 23);
			this.m_btnKdf1Sec.TabIndex = 12;
			this.m_btnKdf1Sec.Text = "1 Second &Delay";
			this.m_btnKdf1Sec.UseVisualStyleBackColor = true;
			this.m_btnKdf1Sec.Click += new System.EventHandler(this.OnBtnKdf1Sec);
			// 
			// m_lblKdfMem
			// 
			this.m_lblKdfMem.AutoSize = true;
			this.m_lblKdfMem.Location = new System.Drawing.Point(6, 106);
			this.m_lblKdfMem.Name = "m_lblKdfMem";
			this.m_lblKdfMem.Size = new System.Drawing.Size(47, 13);
			this.m_lblKdfMem.TabIndex = 5;
			this.m_lblKdfMem.Text = "Memory:";
			// 
			// m_cmbKdfMem
			// 
			this.m_cmbKdfMem.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbKdfMem.FormattingEnabled = true;
			this.m_cmbKdfMem.Location = new System.Drawing.Point(377, 104);
			this.m_cmbKdfMem.MaxDropDownItems = 16;
			this.m_cmbKdfMem.Name = "m_cmbKdfMem";
			this.m_cmbKdfMem.Size = new System.Drawing.Size(53, 21);
			this.m_cmbKdfMem.TabIndex = 7;
			// 
			// m_cmbKdf
			// 
			this.m_cmbKdf.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbKdf.FormattingEnabled = true;
			this.m_cmbKdf.Location = new System.Drawing.Point(211, 51);
			this.m_cmbKdf.MaxDropDownItems = 16;
			this.m_cmbKdf.Name = "m_cmbKdf";
			this.m_cmbKdf.Size = new System.Drawing.Size(219, 21);
			this.m_cmbKdf.TabIndex = 2;
			this.m_cmbKdf.SelectedIndexChanged += new System.EventHandler(this.OnKdfSelectedIndexChanged);
			// 
			// m_lblKdf
			// 
			this.m_lblKdf.AutoSize = true;
			this.m_lblKdf.Location = new System.Drawing.Point(6, 54);
			this.m_lblKdf.Name = "m_lblKdf";
			this.m_lblKdf.Size = new System.Drawing.Size(118, 13);
			this.m_lblKdf.TabIndex = 1;
			this.m_lblKdf.Text = "Key derivation function:";
			// 
			// m_numKdfIt
			// 
			this.m_numKdfIt.Location = new System.Drawing.Point(211, 78);
			this.m_numKdfIt.Name = "m_numKdfIt";
			this.m_numKdfIt.Size = new System.Drawing.Size(219, 20);
			this.m_numKdfIt.TabIndex = 4;
			this.m_numKdfIt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// m_btnHelp
			// 
			this.m_btnHelp.Location = new System.Drawing.Point(11, 425);
			this.m_btnHelp.Name = "m_btnHelp";
			this.m_btnHelp.Size = new System.Drawing.Size(75, 23);
			this.m_btnHelp.TabIndex = 4;
			this.m_btnHelp.Text = "&Help";
			this.m_btnHelp.UseVisualStyleBackColor = true;
			this.m_btnHelp.Click += new System.EventHandler(this.OnBtnHelp);
			// 
			// m_lblCompressionIntro
			// 
			this.m_lblCompressionIntro.Location = new System.Drawing.Point(3, 12);
			this.m_lblCompressionIntro.Name = "m_lblCompressionIntro";
			this.m_lblCompressionIntro.Size = new System.Drawing.Size(435, 15);
			this.m_lblCompressionIntro.TabIndex = 0;
			this.m_lblCompressionIntro.Text = "Data compression reduces the size of the database.";
			// 
			// m_tabMain
			// 
			this.m_tabMain.Controls.Add(this.m_tabGeneral);
			this.m_tabMain.Controls.Add(this.m_tabSecurity);
			this.m_tabMain.Controls.Add(this.m_tabCompression);
			this.m_tabMain.Controls.Add(this.m_tabRecycleBin);
			this.m_tabMain.Controls.Add(this.m_tabAdvanced);
			this.m_tabMain.Location = new System.Drawing.Point(12, 67);
			this.m_tabMain.Name = "m_tabMain";
			this.m_tabMain.SelectedIndex = 0;
			this.m_tabMain.Size = new System.Drawing.Size(463, 346);
			this.m_tabMain.TabIndex = 0;
			// 
			// m_tabGeneral
			// 
			this.m_tabGeneral.Controls.Add(this.m_cbColor);
			this.m_tabGeneral.Controls.Add(this.m_btnColor);
			this.m_tabGeneral.Controls.Add(this.m_tbDefaultUser);
			this.m_tabGeneral.Controls.Add(this.m_lblDefaultUser);
			this.m_tabGeneral.Controls.Add(this.m_tbDbDesc);
			this.m_tabGeneral.Controls.Add(this.m_lblDbDesc);
			this.m_tabGeneral.Controls.Add(this.m_tbDbName);
			this.m_tabGeneral.Controls.Add(this.m_lblDbName);
			this.m_tabGeneral.Location = new System.Drawing.Point(4, 22);
			this.m_tabGeneral.Name = "m_tabGeneral";
			this.m_tabGeneral.Padding = new System.Windows.Forms.Padding(3);
			this.m_tabGeneral.Size = new System.Drawing.Size(455, 320);
			this.m_tabGeneral.TabIndex = 0;
			this.m_tabGeneral.Text = "General";
			this.m_tabGeneral.UseVisualStyleBackColor = true;
			// 
			// m_cbColor
			// 
			this.m_cbColor.AutoSize = true;
			this.m_cbColor.Location = new System.Drawing.Point(6, 295);
			this.m_cbColor.Name = "m_cbColor";
			this.m_cbColor.Size = new System.Drawing.Size(137, 17);
			this.m_cbColor.TabIndex = 6;
			this.m_cbColor.Text = "Custom database color:";
			this.m_cbColor.UseVisualStyleBackColor = true;
			this.m_cbColor.CheckedChanged += new System.EventHandler(this.OnColorCheckedChanged);
			// 
			// m_btnColor
			// 
			this.m_btnColor.Location = new System.Drawing.Point(176, 291);
			this.m_btnColor.Name = "m_btnColor";
			this.m_btnColor.Size = new System.Drawing.Size(48, 23);
			this.m_btnColor.TabIndex = 7;
			this.m_btnColor.UseVisualStyleBackColor = true;
			this.m_btnColor.Click += new System.EventHandler(this.OnBtnColor);
			// 
			// m_tbDefaultUser
			// 
			this.m_tbDefaultUser.Location = new System.Drawing.Point(177, 265);
			this.m_tbDefaultUser.Name = "m_tbDefaultUser";
			this.m_tbDefaultUser.Size = new System.Drawing.Size(269, 20);
			this.m_tbDefaultUser.TabIndex = 5;
			// 
			// m_lblDefaultUser
			// 
			this.m_lblDefaultUser.AutoSize = true;
			this.m_lblDefaultUser.Location = new System.Drawing.Point(3, 268);
			this.m_lblDefaultUser.Name = "m_lblDefaultUser";
			this.m_lblDefaultUser.Size = new System.Drawing.Size(168, 13);
			this.m_lblDefaultUser.TabIndex = 4;
			this.m_lblDefaultUser.Text = "Default user name for new entries:";
			// 
			// m_tbDbDesc
			// 
			this.m_tbDbDesc.AcceptsReturn = true;
			this.m_tbDbDesc.Location = new System.Drawing.Point(6, 57);
			this.m_tbDbDesc.Multiline = true;
			this.m_tbDbDesc.Name = "m_tbDbDesc";
			this.m_tbDbDesc.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.m_tbDbDesc.Size = new System.Drawing.Size(440, 201);
			this.m_tbDbDesc.TabIndex = 3;
			// 
			// m_lblDbDesc
			// 
			this.m_lblDbDesc.AutoSize = true;
			this.m_lblDbDesc.Location = new System.Drawing.Point(3, 41);
			this.m_lblDbDesc.Name = "m_lblDbDesc";
			this.m_lblDbDesc.Size = new System.Drawing.Size(110, 13);
			this.m_lblDbDesc.TabIndex = 2;
			this.m_lblDbDesc.Text = "Database description:";
			// 
			// m_tbDbName
			// 
			this.m_tbDbName.Location = new System.Drawing.Point(94, 9);
			this.m_tbDbName.Name = "m_tbDbName";
			this.m_tbDbName.Size = new System.Drawing.Size(352, 20);
			this.m_tbDbName.TabIndex = 0;
			// 
			// m_lblDbName
			// 
			this.m_lblDbName.AutoSize = true;
			this.m_lblDbName.Location = new System.Drawing.Point(3, 12);
			this.m_lblDbName.Name = "m_lblDbName";
			this.m_lblDbName.Size = new System.Drawing.Size(85, 13);
			this.m_lblDbName.TabIndex = 1;
			this.m_lblDbName.Text = "Database name:";
			// 
			// m_tabSecurity
			// 
			this.m_tabSecurity.Controls.Add(this.m_lblSecIntro);
			this.m_tabSecurity.Controls.Add(this.m_grpEncAlgo);
			this.m_tabSecurity.Controls.Add(this.m_grpKeyTrans);
			this.m_tabSecurity.Location = new System.Drawing.Point(4, 22);
			this.m_tabSecurity.Name = "m_tabSecurity";
			this.m_tabSecurity.Padding = new System.Windows.Forms.Padding(3);
			this.m_tabSecurity.Size = new System.Drawing.Size(455, 320);
			this.m_tabSecurity.TabIndex = 1;
			this.m_tabSecurity.Text = "Security";
			this.m_tabSecurity.UseVisualStyleBackColor = true;
			// 
			// m_lblSecIntro
			// 
			this.m_lblSecIntro.AutoSize = true;
			this.m_lblSecIntro.Location = new System.Drawing.Point(3, 12);
			this.m_lblSecIntro.Name = "m_lblSecIntro";
			this.m_lblSecIntro.Size = new System.Drawing.Size(277, 13);
			this.m_lblSecIntro.TabIndex = 0;
			this.m_lblSecIntro.Text = "On this page you can configure file-level security settings.";
			// 
			// m_tabCompression
			// 
			this.m_tabCompression.Controls.Add(this.m_lblHeaderCpAlgo);
			this.m_tabCompression.Controls.Add(this.m_lblCpGZipPerf);
			this.m_tabCompression.Controls.Add(this.m_lblCpGZipCp);
			this.m_tabCompression.Controls.Add(this.m_lblCpNonePerf);
			this.m_tabCompression.Controls.Add(this.m_lblCpNoneCp);
			this.m_tabCompression.Controls.Add(this.m_lblHeaderPerf);
			this.m_tabCompression.Controls.Add(this.m_lblHeaderCp);
			this.m_tabCompression.Controls.Add(this.m_rbGZip);
			this.m_tabCompression.Controls.Add(this.m_rbNone);
			this.m_tabCompression.Controls.Add(this.m_lblCompressionIntro);
			this.m_tabCompression.Location = new System.Drawing.Point(4, 22);
			this.m_tabCompression.Name = "m_tabCompression";
			this.m_tabCompression.Size = new System.Drawing.Size(455, 320);
			this.m_tabCompression.TabIndex = 2;
			this.m_tabCompression.Text = "Compression";
			this.m_tabCompression.UseVisualStyleBackColor = true;
			// 
			// m_lblHeaderCpAlgo
			// 
			this.m_lblHeaderCpAlgo.AutoSize = true;
			this.m_lblHeaderCpAlgo.Location = new System.Drawing.Point(23, 56);
			this.m_lblHeaderCpAlgo.Name = "m_lblHeaderCpAlgo";
			this.m_lblHeaderCpAlgo.Size = new System.Drawing.Size(50, 13);
			this.m_lblHeaderCpAlgo.TabIndex = 1;
			this.m_lblHeaderCpAlgo.Text = "Algorithm";
			// 
			// m_lblCpGZipPerf
			// 
			this.m_lblCpGZipPerf.AutoSize = true;
			this.m_lblCpGZipPerf.Location = new System.Drawing.Point(189, 101);
			this.m_lblCpGZipPerf.Name = "m_lblCpGZipPerf";
			this.m_lblCpGZipPerf.Size = new System.Drawing.Size(55, 13);
			this.m_lblCpGZipPerf.TabIndex = 9;
			this.m_lblCpGZipPerf.Text = "Very good";
			// 
			// m_lblCpGZipCp
			// 
			this.m_lblCpGZipCp.AutoSize = true;
			this.m_lblCpGZipCp.Location = new System.Drawing.Point(91, 101);
			this.m_lblCpGZipCp.Name = "m_lblCpGZipCp";
			this.m_lblCpGZipCp.Size = new System.Drawing.Size(52, 13);
			this.m_lblCpGZipCp.TabIndex = 8;
			this.m_lblCpGZipCp.Text = "Moderate";
			// 
			// m_lblCpNonePerf
			// 
			this.m_lblCpNonePerf.AutoSize = true;
			this.m_lblCpNonePerf.Location = new System.Drawing.Point(189, 78);
			this.m_lblCpNonePerf.Name = "m_lblCpNonePerf";
			this.m_lblCpNonePerf.Size = new System.Drawing.Size(52, 13);
			this.m_lblCpNonePerf.TabIndex = 6;
			this.m_lblCpNonePerf.Text = "Moderate";
			// 
			// m_lblCpNoneCp
			// 
			this.m_lblCpNoneCp.AutoSize = true;
			this.m_lblCpNoneCp.Location = new System.Drawing.Point(91, 78);
			this.m_lblCpNoneCp.Name = "m_lblCpNoneCp";
			this.m_lblCpNoneCp.Size = new System.Drawing.Size(83, 13);
			this.m_lblCpNoneCp.TabIndex = 5;
			this.m_lblCpNoneCp.Text = "No compression";
			// 
			// m_lblHeaderPerf
			// 
			this.m_lblHeaderPerf.AutoSize = true;
			this.m_lblHeaderPerf.Location = new System.Drawing.Point(189, 56);
			this.m_lblHeaderPerf.Name = "m_lblHeaderPerf";
			this.m_lblHeaderPerf.Size = new System.Drawing.Size(67, 13);
			this.m_lblHeaderPerf.TabIndex = 3;
			this.m_lblHeaderPerf.Text = "Performance";
			// 
			// m_lblHeaderCp
			// 
			this.m_lblHeaderCp.AutoSize = true;
			this.m_lblHeaderCp.Location = new System.Drawing.Point(91, 56);
			this.m_lblHeaderCp.Name = "m_lblHeaderCp";
			this.m_lblHeaderCp.Size = new System.Drawing.Size(67, 13);
			this.m_lblHeaderCp.TabIndex = 2;
			this.m_lblHeaderCp.Text = "Compression";
			// 
			// m_rbGZip
			// 
			this.m_rbGZip.AutoSize = true;
			this.m_rbGZip.Location = new System.Drawing.Point(6, 99);
			this.m_rbGZip.Name = "m_rbGZip";
			this.m_rbGZip.Size = new System.Drawing.Size(48, 17);
			this.m_rbGZip.TabIndex = 7;
			this.m_rbGZip.TabStop = true;
			this.m_rbGZip.Text = "GZip";
			this.m_rbGZip.UseVisualStyleBackColor = true;
			// 
			// m_rbNone
			// 
			this.m_rbNone.AutoSize = true;
			this.m_rbNone.Location = new System.Drawing.Point(6, 76);
			this.m_rbNone.Name = "m_rbNone";
			this.m_rbNone.Size = new System.Drawing.Size(51, 17);
			this.m_rbNone.TabIndex = 4;
			this.m_rbNone.TabStop = true;
			this.m_rbNone.Text = "None";
			this.m_rbNone.UseVisualStyleBackColor = true;
			// 
			// m_tabRecycleBin
			// 
			this.m_tabRecycleBin.Controls.Add(this.m_cmbRecycleBin);
			this.m_tabRecycleBin.Controls.Add(this.m_lblRecycleBinGroup);
			this.m_tabRecycleBin.Controls.Add(this.m_lblRecycleBinInfo);
			this.m_tabRecycleBin.Controls.Add(this.m_cbRecycleBin);
			this.m_tabRecycleBin.Location = new System.Drawing.Point(4, 22);
			this.m_tabRecycleBin.Name = "m_tabRecycleBin";
			this.m_tabRecycleBin.Padding = new System.Windows.Forms.Padding(3);
			this.m_tabRecycleBin.Size = new System.Drawing.Size(455, 320);
			this.m_tabRecycleBin.TabIndex = 4;
			this.m_tabRecycleBin.Text = "Recycle Bin";
			this.m_tabRecycleBin.UseVisualStyleBackColor = true;
			// 
			// m_cmbRecycleBin
			// 
			this.m_cmbRecycleBin.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbRecycleBin.FormattingEnabled = true;
			this.m_cmbRecycleBin.Location = new System.Drawing.Point(6, 99);
			this.m_cmbRecycleBin.Name = "m_cmbRecycleBin";
			this.m_cmbRecycleBin.Size = new System.Drawing.Size(440, 21);
			this.m_cmbRecycleBin.TabIndex = 3;
			// 
			// m_lblRecycleBinGroup
			// 
			this.m_lblRecycleBinGroup.AutoSize = true;
			this.m_lblRecycleBinGroup.Location = new System.Drawing.Point(3, 83);
			this.m_lblRecycleBinGroup.Name = "m_lblRecycleBinGroup";
			this.m_lblRecycleBinGroup.Size = new System.Drawing.Size(96, 13);
			this.m_lblRecycleBinGroup.TabIndex = 2;
			this.m_lblRecycleBinGroup.Text = "Recycle bin group:";
			// 
			// m_lblRecycleBinInfo
			// 
			this.m_lblRecycleBinInfo.Location = new System.Drawing.Point(22, 32);
			this.m_lblRecycleBinInfo.Name = "m_lblRecycleBinInfo";
			this.m_lblRecycleBinInfo.Size = new System.Drawing.Size(424, 40);
			this.m_lblRecycleBinInfo.TabIndex = 1;
			this.m_lblRecycleBinInfo.Text = "If this option is enabled, KeePass moves entries/groups to the recycle bin group " +
				"instead of deleting them. Deleting an entry/group from the recycle bin will perm" +
				"anently remove it.";
			// 
			// m_cbRecycleBin
			// 
			this.m_cbRecycleBin.AutoSize = true;
			this.m_cbRecycleBin.Location = new System.Drawing.Point(6, 12);
			this.m_cbRecycleBin.Name = "m_cbRecycleBin";
			this.m_cbRecycleBin.Size = new System.Drawing.Size(108, 17);
			this.m_cbRecycleBin.TabIndex = 0;
			this.m_cbRecycleBin.Text = "&Use a recycle bin";
			this.m_cbRecycleBin.UseVisualStyleBackColor = true;
			// 
			// m_tabAdvanced
			// 
			this.m_tabAdvanced.Controls.Add(this.m_grpHistory);
			this.m_tabAdvanced.Controls.Add(this.m_grpMasterKey);
			this.m_tabAdvanced.Controls.Add(this.m_grpTemplates);
			this.m_tabAdvanced.Location = new System.Drawing.Point(4, 22);
			this.m_tabAdvanced.Name = "m_tabAdvanced";
			this.m_tabAdvanced.Size = new System.Drawing.Size(455, 320);
			this.m_tabAdvanced.TabIndex = 5;
			this.m_tabAdvanced.Text = "Advanced";
			this.m_tabAdvanced.UseVisualStyleBackColor = true;
			// 
			// m_grpHistory
			// 
			this.m_grpHistory.Controls.Add(this.m_numHistoryMaxSize);
			this.m_grpHistory.Controls.Add(this.m_numHistoryMaxItems);
			this.m_grpHistory.Controls.Add(this.m_cbHistoryMaxSize);
			this.m_grpHistory.Controls.Add(this.m_cbHistoryMaxItems);
			this.m_grpHistory.Location = new System.Drawing.Point(6, 117);
			this.m_grpHistory.Name = "m_grpHistory";
			this.m_grpHistory.Size = new System.Drawing.Size(440, 74);
			this.m_grpHistory.TabIndex = 1;
			this.m_grpHistory.TabStop = false;
			this.m_grpHistory.Text = "Automatic entry history maintenance";
			// 
			// m_numHistoryMaxSize
			// 
			this.m_numHistoryMaxSize.Location = new System.Drawing.Point(349, 43);
			this.m_numHistoryMaxSize.Name = "m_numHistoryMaxSize";
			this.m_numHistoryMaxSize.Size = new System.Drawing.Size(81, 20);
			this.m_numHistoryMaxSize.TabIndex = 3;
			this.m_numHistoryMaxSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// m_numHistoryMaxItems
			// 
			this.m_numHistoryMaxItems.Location = new System.Drawing.Point(349, 18);
			this.m_numHistoryMaxItems.Name = "m_numHistoryMaxItems";
			this.m_numHistoryMaxItems.Size = new System.Drawing.Size(81, 20);
			this.m_numHistoryMaxItems.TabIndex = 1;
			this.m_numHistoryMaxItems.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// m_cbHistoryMaxSize
			// 
			this.m_cbHistoryMaxSize.AutoSize = true;
			this.m_cbHistoryMaxSize.Location = new System.Drawing.Point(9, 44);
			this.m_cbHistoryMaxSize.Name = "m_cbHistoryMaxSize";
			this.m_cbHistoryMaxSize.Size = new System.Drawing.Size(173, 17);
			this.m_cbHistoryMaxSize.TabIndex = 2;
			this.m_cbHistoryMaxSize.Text = "Limit history size per entry (MB):";
			this.m_cbHistoryMaxSize.UseVisualStyleBackColor = true;
			this.m_cbHistoryMaxSize.CheckedChanged += new System.EventHandler(this.OnHistoryMaxSizeCheckedChanged);
			// 
			// m_cbHistoryMaxItems
			// 
			this.m_cbHistoryMaxItems.AutoSize = true;
			this.m_cbHistoryMaxItems.Location = new System.Drawing.Point(9, 19);
			this.m_cbHistoryMaxItems.Name = "m_cbHistoryMaxItems";
			this.m_cbHistoryMaxItems.Size = new System.Drawing.Size(204, 17);
			this.m_cbHistoryMaxItems.TabIndex = 0;
			this.m_cbHistoryMaxItems.Text = "Limit number of history items per entry:";
			this.m_cbHistoryMaxItems.UseVisualStyleBackColor = true;
			this.m_cbHistoryMaxItems.CheckedChanged += new System.EventHandler(this.OnHistoryMaxItemsCheckedChanged);
			// 
			// m_grpMasterKey
			// 
			this.m_grpMasterKey.Controls.Add(this.m_cbKeyForceOnce);
			this.m_grpMasterKey.Controls.Add(this.m_cbKeyForce);
			this.m_grpMasterKey.Controls.Add(this.m_cbKeyRec);
			this.m_grpMasterKey.Controls.Add(this.m_numKeyForceDays);
			this.m_grpMasterKey.Controls.Add(this.m_numKeyRecDays);
			this.m_grpMasterKey.Location = new System.Drawing.Point(6, 197);
			this.m_grpMasterKey.Name = "m_grpMasterKey";
			this.m_grpMasterKey.Size = new System.Drawing.Size(440, 99);
			this.m_grpMasterKey.TabIndex = 2;
			this.m_grpMasterKey.TabStop = false;
			this.m_grpMasterKey.Text = "Master key";
			// 
			// m_cbKeyForceOnce
			// 
			this.m_cbKeyForceOnce.AutoSize = true;
			this.m_cbKeyForceOnce.Location = new System.Drawing.Point(9, 69);
			this.m_cbKeyForceOnce.Name = "m_cbKeyForceOnce";
			this.m_cbKeyForceOnce.Size = new System.Drawing.Size(268, 17);
			this.m_cbKeyForceOnce.TabIndex = 4;
			this.m_cbKeyForceOnce.Text = "Force changing the master key the next time (once)";
			this.m_cbKeyForceOnce.UseVisualStyleBackColor = true;
			// 
			// m_cbKeyForce
			// 
			this.m_cbKeyForce.AutoSize = true;
			this.m_cbKeyForce.Location = new System.Drawing.Point(9, 44);
			this.m_cbKeyForce.Name = "m_cbKeyForce";
			this.m_cbKeyForce.Size = new System.Drawing.Size(206, 17);
			this.m_cbKeyForce.TabIndex = 2;
			this.m_cbKeyForce.Text = "Force changing the master key (days):";
			this.m_cbKeyForce.UseVisualStyleBackColor = true;
			this.m_cbKeyForce.CheckedChanged += new System.EventHandler(this.OnKeyForceCheckedChanged);
			// 
			// m_cbKeyRec
			// 
			this.m_cbKeyRec.AutoSize = true;
			this.m_cbKeyRec.Location = new System.Drawing.Point(9, 19);
			this.m_cbKeyRec.Name = "m_cbKeyRec";
			this.m_cbKeyRec.Size = new System.Drawing.Size(239, 17);
			this.m_cbKeyRec.TabIndex = 0;
			this.m_cbKeyRec.Text = "Recommend changing the master key (days):";
			this.m_cbKeyRec.UseVisualStyleBackColor = true;
			this.m_cbKeyRec.CheckedChanged += new System.EventHandler(this.OnKeyRecCheckedChanged);
			// 
			// m_numKeyForceDays
			// 
			this.m_numKeyForceDays.Location = new System.Drawing.Point(349, 43);
			this.m_numKeyForceDays.Name = "m_numKeyForceDays";
			this.m_numKeyForceDays.Size = new System.Drawing.Size(81, 20);
			this.m_numKeyForceDays.TabIndex = 3;
			this.m_numKeyForceDays.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// m_numKeyRecDays
			// 
			this.m_numKeyRecDays.Location = new System.Drawing.Point(349, 18);
			this.m_numKeyRecDays.Name = "m_numKeyRecDays";
			this.m_numKeyRecDays.Size = new System.Drawing.Size(81, 20);
			this.m_numKeyRecDays.TabIndex = 1;
			this.m_numKeyRecDays.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// m_grpTemplates
			// 
			this.m_grpTemplates.Controls.Add(this.m_cmbEntryTemplates);
			this.m_grpTemplates.Controls.Add(this.m_lblTemplatesHint);
			this.m_grpTemplates.Controls.Add(this.m_lblEntryTemplatesGroup);
			this.m_grpTemplates.Location = new System.Drawing.Point(6, 7);
			this.m_grpTemplates.Name = "m_grpTemplates";
			this.m_grpTemplates.Size = new System.Drawing.Size(440, 104);
			this.m_grpTemplates.TabIndex = 0;
			this.m_grpTemplates.TabStop = false;
			this.m_grpTemplates.Text = "Templates";
			// 
			// m_cmbEntryTemplates
			// 
			this.m_cmbEntryTemplates.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbEntryTemplates.FormattingEnabled = true;
			this.m_cmbEntryTemplates.Location = new System.Drawing.Point(9, 35);
			this.m_cmbEntryTemplates.Name = "m_cmbEntryTemplates";
			this.m_cmbEntryTemplates.Size = new System.Drawing.Size(421, 21);
			this.m_cmbEntryTemplates.TabIndex = 1;
			// 
			// m_lblTemplatesHint
			// 
			this.m_lblTemplatesHint.Location = new System.Drawing.Point(6, 68);
			this.m_lblTemplatesHint.Name = "m_lblTemplatesHint";
			this.m_lblTemplatesHint.Size = new System.Drawing.Size(424, 28);
			this.m_lblTemplatesHint.TabIndex = 2;
			this.m_lblTemplatesHint.Text = "Click the drop-down arrow of the \'Add Entry\' toolbar button in the main window to" +
				" create a new entry based on a template in the group above.";
			// 
			// m_lblEntryTemplatesGroup
			// 
			this.m_lblEntryTemplatesGroup.AutoSize = true;
			this.m_lblEntryTemplatesGroup.Location = new System.Drawing.Point(6, 19);
			this.m_lblEntryTemplatesGroup.Name = "m_lblEntryTemplatesGroup";
			this.m_lblEntryTemplatesGroup.Size = new System.Drawing.Size(112, 13);
			this.m_lblEntryTemplatesGroup.TabIndex = 0;
			this.m_lblEntryTemplatesGroup.Text = "Entry templates group:";
			// 
			// m_btnCancelOp
			// 
			this.m_btnCancelOp.Location = new System.Drawing.Point(180, 425);
			this.m_btnCancelOp.Name = "m_btnCancelOp";
			this.m_btnCancelOp.Size = new System.Drawing.Size(132, 23);
			this.m_btnCancelOp.TabIndex = 1;
			this.m_btnCancelOp.Text = "&Cancel Operation";
			this.m_btnCancelOp.UseVisualStyleBackColor = true;
			this.m_btnCancelOp.Click += new System.EventHandler(this.OnBtnCancelOp);
			// 
			// DatabaseSettingsForm
			// 
			this.AcceptButton = this.m_btnOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.ClientSize = new System.Drawing.Size(486, 460);
			this.Controls.Add(this.m_btnCancelOp);
			this.Controls.Add(this.m_tabMain);
			this.Controls.Add(this.m_btnHelp);
			this.Controls.Add(this.m_bannerImage);
			this.Controls.Add(this.m_btnOK);
			this.Controls.Add(this.m_btnCancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "DatabaseSettingsForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "<DYN>";
			this.Load += new System.EventHandler(this.OnFormLoad);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
			this.m_grpEncAlgo.ResumeLayout(false);
			this.m_grpEncAlgo.PerformLayout();
			this.m_grpKeyTrans.ResumeLayout(false);
			this.m_grpKeyTrans.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numKdfPar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numKdfMem)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numKdfIt)).EndInit();
			this.m_tabMain.ResumeLayout(false);
			this.m_tabGeneral.ResumeLayout(false);
			this.m_tabGeneral.PerformLayout();
			this.m_tabSecurity.ResumeLayout(false);
			this.m_tabSecurity.PerformLayout();
			this.m_tabCompression.ResumeLayout(false);
			this.m_tabCompression.PerformLayout();
			this.m_tabRecycleBin.ResumeLayout(false);
			this.m_tabRecycleBin.PerformLayout();
			this.m_tabAdvanced.ResumeLayout(false);
			this.m_grpHistory.ResumeLayout(false);
			this.m_grpHistory.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numHistoryMaxSize)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numHistoryMaxItems)).EndInit();
			this.m_grpMasterKey.ResumeLayout(false);
			this.m_grpMasterKey.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numKeyForceDays)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.m_numKeyRecDays)).EndInit();
			this.m_grpTemplates.ResumeLayout(false);
			this.m_grpTemplates.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PictureBox m_bannerImage;
		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.GroupBox m_grpEncAlgo;
		private System.Windows.Forms.ComboBox m_cmbEncAlgo;
		private System.Windows.Forms.Label m_lblEncAlgoDesc;
		private System.Windows.Forms.Label m_lblKdfIt;
		private System.Windows.Forms.Label m_lblKdfIntro;
		private System.Windows.Forms.Label m_lblKdfInfo;
		private System.Windows.Forms.GroupBox m_grpKeyTrans;
		private System.Windows.Forms.NumericUpDown m_numKdfIt;
		private System.Windows.Forms.Button m_btnHelp;
		private System.Windows.Forms.ToolTip m_ttRect;
		private System.Windows.Forms.Label m_lblCompressionIntro;
		private System.Windows.Forms.TabControl m_tabMain;
		private System.Windows.Forms.TabPage m_tabGeneral;
		private System.Windows.Forms.TabPage m_tabSecurity;
		private System.Windows.Forms.TabPage m_tabCompression;
		private KeePass.UI.PromptedTextBox m_tbDbDesc;
		private System.Windows.Forms.Label m_lblDbDesc;
		private KeePass.UI.PromptedTextBox m_tbDbName;
		private System.Windows.Forms.Label m_lblDbName;
		private System.Windows.Forms.Label m_lblSecIntro;
		private System.Windows.Forms.RadioButton m_rbGZip;
		private System.Windows.Forms.RadioButton m_rbNone;
		private System.Windows.Forms.Label m_lblCpNonePerf;
		private System.Windows.Forms.Label m_lblCpNoneCp;
		private System.Windows.Forms.Label m_lblHeaderPerf;
		private System.Windows.Forms.Label m_lblHeaderCp;
		private System.Windows.Forms.Label m_lblCpGZipPerf;
		private System.Windows.Forms.Label m_lblCpGZipCp;
		private System.Windows.Forms.Label m_lblHeaderCpAlgo;
		private System.Windows.Forms.TextBox m_tbDefaultUser;
		private System.Windows.Forms.Label m_lblDefaultUser;
		private System.Windows.Forms.TabPage m_tabRecycleBin;
		private System.Windows.Forms.Label m_lblRecycleBinInfo;
		private System.Windows.Forms.CheckBox m_cbRecycleBin;
		private System.Windows.Forms.ComboBox m_cmbRecycleBin;
		private System.Windows.Forms.Label m_lblRecycleBinGroup;
		private System.Windows.Forms.TabPage m_tabAdvanced;
		private System.Windows.Forms.ComboBox m_cmbEntryTemplates;
		private System.Windows.Forms.Label m_lblEntryTemplatesGroup;
		private System.Windows.Forms.Label m_lblTemplatesHint;
		private System.Windows.Forms.GroupBox m_grpTemplates;
		private System.Windows.Forms.GroupBox m_grpMasterKey;
		private System.Windows.Forms.CheckBox m_cbKeyForce;
		private System.Windows.Forms.CheckBox m_cbKeyRec;
		private System.Windows.Forms.NumericUpDown m_numKeyForceDays;
		private System.Windows.Forms.NumericUpDown m_numKeyRecDays;
		private System.Windows.Forms.GroupBox m_grpHistory;
		private System.Windows.Forms.CheckBox m_cbHistoryMaxItems;
		private System.Windows.Forms.NumericUpDown m_numHistoryMaxSize;
		private System.Windows.Forms.NumericUpDown m_numHistoryMaxItems;
		private System.Windows.Forms.CheckBox m_cbHistoryMaxSize;
		private System.Windows.Forms.Button m_btnColor;
		private System.Windows.Forms.CheckBox m_cbColor;
		private System.Windows.Forms.ComboBox m_cmbKdf;
		private System.Windows.Forms.Label m_lblKdf;
		private System.Windows.Forms.Button m_btnKdf1Sec;
		private System.Windows.Forms.Label m_lblKdfMem;
		private System.Windows.Forms.ComboBox m_cmbKdfMem;
		private System.Windows.Forms.ProgressBar m_pbKdf;
		private System.Windows.Forms.NumericUpDown m_numKdfMem;
		private System.Windows.Forms.Button m_btnKdfTest;
		private System.Windows.Forms.Button m_btnCancelOp;
		private System.Windows.Forms.Label m_lblKdfPar;
		private System.Windows.Forms.NumericUpDown m_numKdfPar;
		private System.Windows.Forms.CheckBox m_cbKeyForceOnce;
	}
}