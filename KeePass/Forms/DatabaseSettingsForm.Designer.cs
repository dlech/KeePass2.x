namespace KeePass.Forms
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DatabaseSettingsForm));
			this.m_bannerImage = new System.Windows.Forms.PictureBox();
			this.m_btnOK = new System.Windows.Forms.Button();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_grpEncAlgo = new System.Windows.Forms.GroupBox();
			this.m_lblEncAlgoDesc = new System.Windows.Forms.Label();
			this.m_cmbEncAlgo = new System.Windows.Forms.ComboBox();
			this.m_lblTransIntro = new System.Windows.Forms.Label();
			this.m_lblTransNum = new System.Windows.Forms.Label();
			this.m_lblTransInfo = new System.Windows.Forms.Label();
			this.m_grpKeyTrans = new System.Windows.Forms.GroupBox();
			this.m_lnkCompute1SecDelay = new System.Windows.Forms.LinkLabel();
			this.m_numEncRounds = new System.Windows.Forms.NumericUpDown();
			this.m_btnHelp = new System.Windows.Forms.Button();
			this.m_ttRect = new System.Windows.Forms.ToolTip(this.components);
			this.m_lblCompressionIntro = new System.Windows.Forms.Label();
			this.m_tabMain = new System.Windows.Forms.TabControl();
			this.m_tabGeneral = new System.Windows.Forms.TabPage();
			this.m_tbDefaultUser = new System.Windows.Forms.TextBox();
			this.m_lblDefaultUser = new System.Windows.Forms.Label();
			this.m_tbDbDesc = new System.Windows.Forms.TextBox();
			this.m_lblDbDesc = new System.Windows.Forms.Label();
			this.m_tbDbName = new System.Windows.Forms.TextBox();
			this.m_lblDbName = new System.Windows.Forms.Label();
			this.m_tabSecurity = new System.Windows.Forms.TabPage();
			this.m_lblSecIntro = new System.Windows.Forms.Label();
			this.m_tabProtection = new System.Windows.Forms.TabPage();
			this.m_cbAutoEnableHiding = new System.Windows.Forms.CheckBox();
			this.m_lblViewHint = new System.Windows.Forms.Label();
			this.m_lblMemProtEnable = new System.Windows.Forms.Label();
			this.m_lblMemProtHint = new System.Windows.Forms.Label();
			this.m_lblMemProtDesc = new System.Windows.Forms.Label();
			this.m_lbMemProt = new System.Windows.Forms.CheckedListBox();
			this.m_lblProtIntro = new System.Windows.Forms.Label();
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
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
			this.m_grpEncAlgo.SuspendLayout();
			this.m_grpKeyTrans.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numEncRounds)).BeginInit();
			this.m_tabMain.SuspendLayout();
			this.m_tabGeneral.SuspendLayout();
			this.m_tabSecurity.SuspendLayout();
			this.m_tabProtection.SuspendLayout();
			this.m_tabCompression.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_bannerImage
			// 
			resources.ApplyResources(this.m_bannerImage, "m_bannerImage");
			this.m_bannerImage.Name = "m_bannerImage";
			this.m_bannerImage.TabStop = false;
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
			// m_grpEncAlgo
			// 
			this.m_grpEncAlgo.Controls.Add(this.m_lblEncAlgoDesc);
			this.m_grpEncAlgo.Controls.Add(this.m_cmbEncAlgo);
			resources.ApplyResources(this.m_grpEncAlgo, "m_grpEncAlgo");
			this.m_grpEncAlgo.Name = "m_grpEncAlgo";
			this.m_grpEncAlgo.TabStop = false;
			// 
			// m_lblEncAlgoDesc
			// 
			resources.ApplyResources(this.m_lblEncAlgoDesc, "m_lblEncAlgoDesc");
			this.m_lblEncAlgoDesc.Name = "m_lblEncAlgoDesc";
			// 
			// m_cmbEncAlgo
			// 
			this.m_cmbEncAlgo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_cmbEncAlgo.FormattingEnabled = true;
			resources.ApplyResources(this.m_cmbEncAlgo, "m_cmbEncAlgo");
			this.m_cmbEncAlgo.Name = "m_cmbEncAlgo";
			// 
			// m_lblTransIntro
			// 
			resources.ApplyResources(this.m_lblTransIntro, "m_lblTransIntro");
			this.m_lblTransIntro.Name = "m_lblTransIntro";
			// 
			// m_lblTransNum
			// 
			resources.ApplyResources(this.m_lblTransNum, "m_lblTransNum");
			this.m_lblTransNum.Name = "m_lblTransNum";
			// 
			// m_lblTransInfo
			// 
			resources.ApplyResources(this.m_lblTransInfo, "m_lblTransInfo");
			this.m_lblTransInfo.Name = "m_lblTransInfo";
			// 
			// m_grpKeyTrans
			// 
			this.m_grpKeyTrans.Controls.Add(this.m_lnkCompute1SecDelay);
			this.m_grpKeyTrans.Controls.Add(this.m_numEncRounds);
			this.m_grpKeyTrans.Controls.Add(this.m_lblTransIntro);
			this.m_grpKeyTrans.Controls.Add(this.m_lblTransInfo);
			this.m_grpKeyTrans.Controls.Add(this.m_lblTransNum);
			resources.ApplyResources(this.m_grpKeyTrans, "m_grpKeyTrans");
			this.m_grpKeyTrans.Name = "m_grpKeyTrans";
			this.m_grpKeyTrans.TabStop = false;
			// 
			// m_lnkCompute1SecDelay
			// 
			resources.ApplyResources(this.m_lnkCompute1SecDelay, "m_lnkCompute1SecDelay");
			this.m_lnkCompute1SecDelay.Name = "m_lnkCompute1SecDelay";
			this.m_lnkCompute1SecDelay.TabStop = true;
			this.m_ttRect.SetToolTip(this.m_lnkCompute1SecDelay, resources.GetString("m_lnkCompute1SecDelay.ToolTip"));
			this.m_lnkCompute1SecDelay.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnLinkClicked1SecondDelayRounds);
			// 
			// m_numEncRounds
			// 
			resources.ApplyResources(this.m_numEncRounds, "m_numEncRounds");
			this.m_numEncRounds.Name = "m_numEncRounds";
			// 
			// m_btnHelp
			// 
			resources.ApplyResources(this.m_btnHelp, "m_btnHelp");
			this.m_btnHelp.Name = "m_btnHelp";
			this.m_btnHelp.UseVisualStyleBackColor = true;
			this.m_btnHelp.Click += new System.EventHandler(this.OnBtnHelp);
			// 
			// m_ttRect
			// 
			resources.ApplyResources(this.m_ttRect, "m_ttRect");
			// 
			// m_lblCompressionIntro
			// 
			resources.ApplyResources(this.m_lblCompressionIntro, "m_lblCompressionIntro");
			this.m_lblCompressionIntro.Name = "m_lblCompressionIntro";
			// 
			// m_tabMain
			// 
			this.m_tabMain.Controls.Add(this.m_tabGeneral);
			this.m_tabMain.Controls.Add(this.m_tabSecurity);
			this.m_tabMain.Controls.Add(this.m_tabProtection);
			this.m_tabMain.Controls.Add(this.m_tabCompression);
			resources.ApplyResources(this.m_tabMain, "m_tabMain");
			this.m_tabMain.Name = "m_tabMain";
			this.m_tabMain.SelectedIndex = 0;
			// 
			// m_tabGeneral
			// 
			this.m_tabGeneral.Controls.Add(this.m_tbDefaultUser);
			this.m_tabGeneral.Controls.Add(this.m_lblDefaultUser);
			this.m_tabGeneral.Controls.Add(this.m_tbDbDesc);
			this.m_tabGeneral.Controls.Add(this.m_lblDbDesc);
			this.m_tabGeneral.Controls.Add(this.m_tbDbName);
			this.m_tabGeneral.Controls.Add(this.m_lblDbName);
			resources.ApplyResources(this.m_tabGeneral, "m_tabGeneral");
			this.m_tabGeneral.Name = "m_tabGeneral";
			this.m_tabGeneral.UseVisualStyleBackColor = true;
			// 
			// m_tbDefaultUser
			// 
			resources.ApplyResources(this.m_tbDefaultUser, "m_tbDefaultUser");
			this.m_tbDefaultUser.Name = "m_tbDefaultUser";
			// 
			// m_lblDefaultUser
			// 
			resources.ApplyResources(this.m_lblDefaultUser, "m_lblDefaultUser");
			this.m_lblDefaultUser.Name = "m_lblDefaultUser";
			// 
			// m_tbDbDesc
			// 
			this.m_tbDbDesc.AcceptsReturn = true;
			this.m_tbDbDesc.AcceptsTab = true;
			resources.ApplyResources(this.m_tbDbDesc, "m_tbDbDesc");
			this.m_tbDbDesc.Name = "m_tbDbDesc";
			// 
			// m_lblDbDesc
			// 
			resources.ApplyResources(this.m_lblDbDesc, "m_lblDbDesc");
			this.m_lblDbDesc.Name = "m_lblDbDesc";
			// 
			// m_tbDbName
			// 
			resources.ApplyResources(this.m_tbDbName, "m_tbDbName");
			this.m_tbDbName.Name = "m_tbDbName";
			// 
			// m_lblDbName
			// 
			resources.ApplyResources(this.m_lblDbName, "m_lblDbName");
			this.m_lblDbName.Name = "m_lblDbName";
			// 
			// m_tabSecurity
			// 
			this.m_tabSecurity.Controls.Add(this.m_lblSecIntro);
			this.m_tabSecurity.Controls.Add(this.m_grpEncAlgo);
			this.m_tabSecurity.Controls.Add(this.m_grpKeyTrans);
			resources.ApplyResources(this.m_tabSecurity, "m_tabSecurity");
			this.m_tabSecurity.Name = "m_tabSecurity";
			this.m_tabSecurity.UseVisualStyleBackColor = true;
			// 
			// m_lblSecIntro
			// 
			resources.ApplyResources(this.m_lblSecIntro, "m_lblSecIntro");
			this.m_lblSecIntro.Name = "m_lblSecIntro";
			// 
			// m_tabProtection
			// 
			this.m_tabProtection.Controls.Add(this.m_cbAutoEnableHiding);
			this.m_tabProtection.Controls.Add(this.m_lblViewHint);
			this.m_tabProtection.Controls.Add(this.m_lblMemProtEnable);
			this.m_tabProtection.Controls.Add(this.m_lblMemProtHint);
			this.m_tabProtection.Controls.Add(this.m_lblMemProtDesc);
			this.m_tabProtection.Controls.Add(this.m_lbMemProt);
			this.m_tabProtection.Controls.Add(this.m_lblProtIntro);
			resources.ApplyResources(this.m_tabProtection, "m_tabProtection");
			this.m_tabProtection.Name = "m_tabProtection";
			this.m_tabProtection.UseVisualStyleBackColor = true;
			// 
			// m_cbAutoEnableHiding
			// 
			resources.ApplyResources(this.m_cbAutoEnableHiding, "m_cbAutoEnableHiding");
			this.m_cbAutoEnableHiding.Name = "m_cbAutoEnableHiding";
			this.m_cbAutoEnableHiding.UseVisualStyleBackColor = true;
			// 
			// m_lblViewHint
			// 
			resources.ApplyResources(this.m_lblViewHint, "m_lblViewHint");
			this.m_lblViewHint.Name = "m_lblViewHint";
			// 
			// m_lblMemProtEnable
			// 
			resources.ApplyResources(this.m_lblMemProtEnable, "m_lblMemProtEnable");
			this.m_lblMemProtEnable.Name = "m_lblMemProtEnable";
			// 
			// m_lblMemProtHint
			// 
			resources.ApplyResources(this.m_lblMemProtHint, "m_lblMemProtHint");
			this.m_lblMemProtHint.Name = "m_lblMemProtHint";
			// 
			// m_lblMemProtDesc
			// 
			resources.ApplyResources(this.m_lblMemProtDesc, "m_lblMemProtDesc");
			this.m_lblMemProtDesc.Name = "m_lblMemProtDesc";
			// 
			// m_lbMemProt
			// 
			this.m_lbMemProt.CheckOnClick = true;
			this.m_lbMemProt.FormattingEnabled = true;
			resources.ApplyResources(this.m_lbMemProt, "m_lbMemProt");
			this.m_lbMemProt.Name = "m_lbMemProt";
			// 
			// m_lblProtIntro
			// 
			resources.ApplyResources(this.m_lblProtIntro, "m_lblProtIntro");
			this.m_lblProtIntro.Name = "m_lblProtIntro";
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
			resources.ApplyResources(this.m_tabCompression, "m_tabCompression");
			this.m_tabCompression.Name = "m_tabCompression";
			this.m_tabCompression.UseVisualStyleBackColor = true;
			// 
			// m_lblHeaderCpAlgo
			// 
			resources.ApplyResources(this.m_lblHeaderCpAlgo, "m_lblHeaderCpAlgo");
			this.m_lblHeaderCpAlgo.Name = "m_lblHeaderCpAlgo";
			// 
			// m_lblCpGZipPerf
			// 
			resources.ApplyResources(this.m_lblCpGZipPerf, "m_lblCpGZipPerf");
			this.m_lblCpGZipPerf.Name = "m_lblCpGZipPerf";
			// 
			// m_lblCpGZipCp
			// 
			resources.ApplyResources(this.m_lblCpGZipCp, "m_lblCpGZipCp");
			this.m_lblCpGZipCp.Name = "m_lblCpGZipCp";
			// 
			// m_lblCpNonePerf
			// 
			resources.ApplyResources(this.m_lblCpNonePerf, "m_lblCpNonePerf");
			this.m_lblCpNonePerf.Name = "m_lblCpNonePerf";
			// 
			// m_lblCpNoneCp
			// 
			resources.ApplyResources(this.m_lblCpNoneCp, "m_lblCpNoneCp");
			this.m_lblCpNoneCp.Name = "m_lblCpNoneCp";
			// 
			// m_lblHeaderPerf
			// 
			resources.ApplyResources(this.m_lblHeaderPerf, "m_lblHeaderPerf");
			this.m_lblHeaderPerf.Name = "m_lblHeaderPerf";
			// 
			// m_lblHeaderCp
			// 
			resources.ApplyResources(this.m_lblHeaderCp, "m_lblHeaderCp");
			this.m_lblHeaderCp.Name = "m_lblHeaderCp";
			// 
			// m_rbGZip
			// 
			resources.ApplyResources(this.m_rbGZip, "m_rbGZip");
			this.m_rbGZip.Name = "m_rbGZip";
			this.m_rbGZip.TabStop = true;
			this.m_rbGZip.UseVisualStyleBackColor = true;
			// 
			// m_rbNone
			// 
			resources.ApplyResources(this.m_rbNone, "m_rbNone");
			this.m_rbNone.Name = "m_rbNone";
			this.m_rbNone.TabStop = true;
			this.m_rbNone.UseVisualStyleBackColor = true;
			// 
			// DatabaseSettingsForm
			// 
			this.AcceptButton = this.m_btnOK;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
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
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
			this.Load += new System.EventHandler(this.OnFormLoad);
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
			this.m_grpEncAlgo.ResumeLayout(false);
			this.m_grpEncAlgo.PerformLayout();
			this.m_grpKeyTrans.ResumeLayout(false);
			this.m_grpKeyTrans.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_numEncRounds)).EndInit();
			this.m_tabMain.ResumeLayout(false);
			this.m_tabGeneral.ResumeLayout(false);
			this.m_tabGeneral.PerformLayout();
			this.m_tabSecurity.ResumeLayout(false);
			this.m_tabSecurity.PerformLayout();
			this.m_tabProtection.ResumeLayout(false);
			this.m_tabProtection.PerformLayout();
			this.m_tabCompression.ResumeLayout(false);
			this.m_tabCompression.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PictureBox m_bannerImage;
		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.Button m_btnCancel;
		private System.Windows.Forms.GroupBox m_grpEncAlgo;
		private System.Windows.Forms.ComboBox m_cmbEncAlgo;
		private System.Windows.Forms.Label m_lblEncAlgoDesc;
		private System.Windows.Forms.Label m_lblTransNum;
		private System.Windows.Forms.Label m_lblTransIntro;
		private System.Windows.Forms.Label m_lblTransInfo;
		private System.Windows.Forms.GroupBox m_grpKeyTrans;
		private System.Windows.Forms.NumericUpDown m_numEncRounds;
		private System.Windows.Forms.Button m_btnHelp;
		private System.Windows.Forms.LinkLabel m_lnkCompute1SecDelay;
		private System.Windows.Forms.ToolTip m_ttRect;
		private System.Windows.Forms.Label m_lblCompressionIntro;
		private System.Windows.Forms.TabControl m_tabMain;
		private System.Windows.Forms.TabPage m_tabGeneral;
		private System.Windows.Forms.TabPage m_tabSecurity;
		private System.Windows.Forms.TabPage m_tabProtection;
		private System.Windows.Forms.TabPage m_tabCompression;
		private System.Windows.Forms.TextBox m_tbDbDesc;
		private System.Windows.Forms.Label m_lblDbDesc;
		private System.Windows.Forms.TextBox m_tbDbName;
		private System.Windows.Forms.Label m_lblDbName;
		private System.Windows.Forms.Label m_lblSecIntro;
		private System.Windows.Forms.Label m_lblProtIntro;
		private System.Windows.Forms.Label m_lblMemProtHint;
		private System.Windows.Forms.Label m_lblMemProtDesc;
		private System.Windows.Forms.CheckedListBox m_lbMemProt;
		private System.Windows.Forms.Label m_lblMemProtEnable;
		private System.Windows.Forms.RadioButton m_rbGZip;
		private System.Windows.Forms.RadioButton m_rbNone;
		private System.Windows.Forms.Label m_lblCpNonePerf;
		private System.Windows.Forms.Label m_lblCpNoneCp;
		private System.Windows.Forms.Label m_lblHeaderPerf;
		private System.Windows.Forms.Label m_lblHeaderCp;
		private System.Windows.Forms.Label m_lblCpGZipPerf;
		private System.Windows.Forms.Label m_lblCpGZipCp;
		private System.Windows.Forms.Label m_lblHeaderCpAlgo;
		private System.Windows.Forms.Label m_lblViewHint;
		private System.Windows.Forms.CheckBox m_cbAutoEnableHiding;
		private System.Windows.Forms.TextBox m_tbDefaultUser;
		private System.Windows.Forms.Label m_lblDefaultUser;
	}
}