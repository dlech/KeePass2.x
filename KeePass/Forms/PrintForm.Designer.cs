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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PrintForm));
			this.m_tabMain = new System.Windows.Forms.TabControl();
			this.m_tabPreview = new System.Windows.Forms.TabPage();
			this.m_wbMain = new System.Windows.Forms.WebBrowser();
			this.m_lblPreviewHint = new System.Windows.Forms.Label();
			this.m_tabDataLayout = new System.Windows.Forms.TabPage();
			this.m_grpFont = new System.Windows.Forms.GroupBox();
			this.m_cbSmallMono = new System.Windows.Forms.CheckBox();
			this.m_cbMonospaceForPasswords = new System.Windows.Forms.CheckBox();
			this.m_rbMonospace = new System.Windows.Forms.RadioButton();
			this.m_rbSansSerif = new System.Windows.Forms.RadioButton();
			this.m_rbSerif = new System.Windows.Forms.RadioButton();
			this.m_grpFields = new System.Windows.Forms.GroupBox();
			this.m_cbGroups = new System.Windows.Forms.CheckBox();
			this.m_linkDeselectAllFields = new System.Windows.Forms.LinkLabel();
			this.m_linkSelectAllFields = new System.Windows.Forms.LinkLabel();
			this.m_cbAutoType = new System.Windows.Forms.CheckBox();
			this.m_cbLastAccess = new System.Windows.Forms.CheckBox();
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
			this.m_ilTabIcons = new System.Windows.Forms.ImageList(this.components);
			this.m_bannerImage = new System.Windows.Forms.PictureBox();
			this.m_btnOK = new System.Windows.Forms.Button();
			this.m_btnCancel = new System.Windows.Forms.Button();
			this.m_btnConfigPrinter = new System.Windows.Forms.Button();
			this.m_btnPrintPreview = new System.Windows.Forms.Button();
			this.m_tabMain.SuspendLayout();
			this.m_tabPreview.SuspendLayout();
			this.m_tabDataLayout.SuspendLayout();
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
			this.m_tabMain.ImageList = this.m_ilTabIcons;
			resources.ApplyResources(this.m_tabMain, "m_tabMain");
			this.m_tabMain.Name = "m_tabMain";
			this.m_tabMain.SelectedIndex = 0;
			this.m_tabMain.SelectedIndexChanged += new System.EventHandler(this.OnTabSelectedIndexChanged);
			// 
			// m_tabPreview
			// 
			this.m_tabPreview.Controls.Add(this.m_wbMain);
			this.m_tabPreview.Controls.Add(this.m_lblPreviewHint);
			resources.ApplyResources(this.m_tabPreview, "m_tabPreview");
			this.m_tabPreview.Name = "m_tabPreview";
			this.m_tabPreview.UseVisualStyleBackColor = true;
			// 
			// m_wbMain
			// 
			this.m_wbMain.AllowWebBrowserDrop = false;
			resources.ApplyResources(this.m_wbMain, "m_wbMain");
			this.m_wbMain.IsWebBrowserContextMenuEnabled = false;
			this.m_wbMain.MinimumSize = new System.Drawing.Size(20, 20);
			this.m_wbMain.Name = "m_wbMain";
			this.m_wbMain.WebBrowserShortcutsEnabled = false;
			// 
			// m_lblPreviewHint
			// 
			resources.ApplyResources(this.m_lblPreviewHint, "m_lblPreviewHint");
			this.m_lblPreviewHint.ForeColor = System.Drawing.Color.Brown;
			this.m_lblPreviewHint.Name = "m_lblPreviewHint";
			// 
			// m_tabDataLayout
			// 
			this.m_tabDataLayout.Controls.Add(this.m_grpFont);
			this.m_tabDataLayout.Controls.Add(this.m_grpFields);
			this.m_tabDataLayout.Controls.Add(this.m_grpLayout);
			resources.ApplyResources(this.m_tabDataLayout, "m_tabDataLayout");
			this.m_tabDataLayout.Name = "m_tabDataLayout";
			this.m_tabDataLayout.UseVisualStyleBackColor = true;
			// 
			// m_grpFont
			// 
			this.m_grpFont.Controls.Add(this.m_cbSmallMono);
			this.m_grpFont.Controls.Add(this.m_cbMonospaceForPasswords);
			this.m_grpFont.Controls.Add(this.m_rbMonospace);
			this.m_grpFont.Controls.Add(this.m_rbSansSerif);
			this.m_grpFont.Controls.Add(this.m_rbSerif);
			resources.ApplyResources(this.m_grpFont, "m_grpFont");
			this.m_grpFont.Name = "m_grpFont";
			this.m_grpFont.TabStop = false;
			// 
			// m_cbSmallMono
			// 
			resources.ApplyResources(this.m_cbSmallMono, "m_cbSmallMono");
			this.m_cbSmallMono.Name = "m_cbSmallMono";
			this.m_cbSmallMono.UseVisualStyleBackColor = true;
			// 
			// m_cbMonospaceForPasswords
			// 
			resources.ApplyResources(this.m_cbMonospaceForPasswords, "m_cbMonospaceForPasswords");
			this.m_cbMonospaceForPasswords.Checked = true;
			this.m_cbMonospaceForPasswords.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_cbMonospaceForPasswords.Name = "m_cbMonospaceForPasswords";
			this.m_cbMonospaceForPasswords.UseVisualStyleBackColor = true;
			// 
			// m_rbMonospace
			// 
			resources.ApplyResources(this.m_rbMonospace, "m_rbMonospace");
			this.m_rbMonospace.Name = "m_rbMonospace";
			this.m_rbMonospace.UseVisualStyleBackColor = true;
			// 
			// m_rbSansSerif
			// 
			resources.ApplyResources(this.m_rbSansSerif, "m_rbSansSerif");
			this.m_rbSansSerif.Checked = true;
			this.m_rbSansSerif.Name = "m_rbSansSerif";
			this.m_rbSansSerif.TabStop = true;
			this.m_rbSansSerif.UseVisualStyleBackColor = true;
			// 
			// m_rbSerif
			// 
			resources.ApplyResources(this.m_rbSerif, "m_rbSerif");
			this.m_rbSerif.Name = "m_rbSerif";
			this.m_rbSerif.UseVisualStyleBackColor = true;
			// 
			// m_grpFields
			// 
			this.m_grpFields.Controls.Add(this.m_cbGroups);
			this.m_grpFields.Controls.Add(this.m_linkDeselectAllFields);
			this.m_grpFields.Controls.Add(this.m_linkSelectAllFields);
			this.m_grpFields.Controls.Add(this.m_cbAutoType);
			this.m_grpFields.Controls.Add(this.m_cbLastAccess);
			this.m_grpFields.Controls.Add(this.m_cbLastMod);
			this.m_grpFields.Controls.Add(this.m_cbCreation);
			this.m_grpFields.Controls.Add(this.m_cbExpire);
			this.m_grpFields.Controls.Add(this.m_cbNotes);
			this.m_grpFields.Controls.Add(this.m_cbPassword);
			this.m_grpFields.Controls.Add(this.m_cbUrl);
			this.m_grpFields.Controls.Add(this.m_cbUser);
			this.m_grpFields.Controls.Add(this.m_cbTitle);
			resources.ApplyResources(this.m_grpFields, "m_grpFields");
			this.m_grpFields.Name = "m_grpFields";
			this.m_grpFields.TabStop = false;
			// 
			// m_cbGroups
			// 
			resources.ApplyResources(this.m_cbGroups, "m_cbGroups");
			this.m_cbGroups.Name = "m_cbGroups";
			this.m_cbGroups.UseVisualStyleBackColor = true;
			// 
			// m_linkDeselectAllFields
			// 
			resources.ApplyResources(this.m_linkDeselectAllFields, "m_linkDeselectAllFields");
			this.m_linkDeselectAllFields.Name = "m_linkDeselectAllFields";
			this.m_linkDeselectAllFields.TabStop = true;
			this.m_linkDeselectAllFields.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnLinkDeselectAllFields);
			// 
			// m_linkSelectAllFields
			// 
			resources.ApplyResources(this.m_linkSelectAllFields, "m_linkSelectAllFields");
			this.m_linkSelectAllFields.Name = "m_linkSelectAllFields";
			this.m_linkSelectAllFields.TabStop = true;
			this.m_linkSelectAllFields.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnLinkSelectAllFields);
			// 
			// m_cbAutoType
			// 
			resources.ApplyResources(this.m_cbAutoType, "m_cbAutoType");
			this.m_cbAutoType.Name = "m_cbAutoType";
			this.m_cbAutoType.UseVisualStyleBackColor = true;
			// 
			// m_cbLastAccess
			// 
			resources.ApplyResources(this.m_cbLastAccess, "m_cbLastAccess");
			this.m_cbLastAccess.Name = "m_cbLastAccess";
			this.m_cbLastAccess.UseVisualStyleBackColor = true;
			// 
			// m_cbLastMod
			// 
			resources.ApplyResources(this.m_cbLastMod, "m_cbLastMod");
			this.m_cbLastMod.Name = "m_cbLastMod";
			this.m_cbLastMod.UseVisualStyleBackColor = true;
			// 
			// m_cbCreation
			// 
			resources.ApplyResources(this.m_cbCreation, "m_cbCreation");
			this.m_cbCreation.Name = "m_cbCreation";
			this.m_cbCreation.UseVisualStyleBackColor = true;
			// 
			// m_cbExpire
			// 
			resources.ApplyResources(this.m_cbExpire, "m_cbExpire");
			this.m_cbExpire.Name = "m_cbExpire";
			this.m_cbExpire.UseVisualStyleBackColor = true;
			// 
			// m_cbNotes
			// 
			resources.ApplyResources(this.m_cbNotes, "m_cbNotes");
			this.m_cbNotes.Checked = true;
			this.m_cbNotes.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_cbNotes.Name = "m_cbNotes";
			this.m_cbNotes.UseVisualStyleBackColor = true;
			// 
			// m_cbPassword
			// 
			resources.ApplyResources(this.m_cbPassword, "m_cbPassword");
			this.m_cbPassword.Checked = true;
			this.m_cbPassword.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_cbPassword.Name = "m_cbPassword";
			this.m_cbPassword.UseVisualStyleBackColor = true;
			// 
			// m_cbUrl
			// 
			resources.ApplyResources(this.m_cbUrl, "m_cbUrl");
			this.m_cbUrl.Name = "m_cbUrl";
			this.m_cbUrl.UseVisualStyleBackColor = true;
			// 
			// m_cbUser
			// 
			resources.ApplyResources(this.m_cbUser, "m_cbUser");
			this.m_cbUser.Checked = true;
			this.m_cbUser.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_cbUser.Name = "m_cbUser";
			this.m_cbUser.UseVisualStyleBackColor = true;
			// 
			// m_cbTitle
			// 
			resources.ApplyResources(this.m_cbTitle, "m_cbTitle");
			this.m_cbTitle.Checked = true;
			this.m_cbTitle.CheckState = System.Windows.Forms.CheckState.Checked;
			this.m_cbTitle.Name = "m_cbTitle";
			this.m_cbTitle.UseVisualStyleBackColor = true;
			// 
			// m_grpLayout
			// 
			this.m_grpLayout.Controls.Add(this.m_lblDetailsInfo);
			this.m_grpLayout.Controls.Add(this.m_lblTabularInfo);
			this.m_grpLayout.Controls.Add(this.m_rbDetails);
			this.m_grpLayout.Controls.Add(this.m_rbTabular);
			resources.ApplyResources(this.m_grpLayout, "m_grpLayout");
			this.m_grpLayout.Name = "m_grpLayout";
			this.m_grpLayout.TabStop = false;
			// 
			// m_lblDetailsInfo
			// 
			resources.ApplyResources(this.m_lblDetailsInfo, "m_lblDetailsInfo");
			this.m_lblDetailsInfo.Name = "m_lblDetailsInfo";
			// 
			// m_lblTabularInfo
			// 
			resources.ApplyResources(this.m_lblTabularInfo, "m_lblTabularInfo");
			this.m_lblTabularInfo.Name = "m_lblTabularInfo";
			// 
			// m_rbDetails
			// 
			resources.ApplyResources(this.m_rbDetails, "m_rbDetails");
			this.m_rbDetails.Name = "m_rbDetails";
			this.m_rbDetails.TabStop = true;
			this.m_rbDetails.UseVisualStyleBackColor = true;
			// 
			// m_rbTabular
			// 
			resources.ApplyResources(this.m_rbTabular, "m_rbTabular");
			this.m_rbTabular.Name = "m_rbTabular";
			this.m_rbTabular.TabStop = true;
			this.m_rbTabular.UseVisualStyleBackColor = true;
			this.m_rbTabular.CheckedChanged += new System.EventHandler(this.OnTabularCheckedChanged);
			// 
			// m_ilTabIcons
			// 
			this.m_ilTabIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("m_ilTabIcons.ImageStream")));
			this.m_ilTabIcons.TransparentColor = System.Drawing.Color.Transparent;
			this.m_ilTabIcons.Images.SetKeyName(0, "B16x16_XMag.png");
			this.m_ilTabIcons.Images.SetKeyName(1, "B16x16_Configure.png");
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
			// m_btnConfigPrinter
			// 
			this.m_btnConfigPrinter.Image = global::KeePass.Properties.Resources.B16x16_EditCopy;
			resources.ApplyResources(this.m_btnConfigPrinter, "m_btnConfigPrinter");
			this.m_btnConfigPrinter.Name = "m_btnConfigPrinter";
			this.m_btnConfigPrinter.UseVisualStyleBackColor = true;
			this.m_btnConfigPrinter.Click += new System.EventHandler(this.OnBtnConfigPage);
			// 
			// m_btnPrintPreview
			// 
			this.m_btnPrintPreview.Image = global::KeePass.Properties.Resources.B16x16_FileQuickPrint;
			resources.ApplyResources(this.m_btnPrintPreview, "m_btnPrintPreview");
			this.m_btnPrintPreview.Name = "m_btnPrintPreview";
			this.m_btnPrintPreview.UseVisualStyleBackColor = true;
			this.m_btnPrintPreview.Click += new System.EventHandler(this.OnBtnPrintPreview);
			// 
			// PrintForm
			// 
			this.AcceptButton = this.m_btnOK;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnCancel;
			this.Controls.Add(this.m_btnCancel);
			this.Controls.Add(this.m_tabMain);
			this.Controls.Add(this.m_btnOK);
			this.Controls.Add(this.m_btnPrintPreview);
			this.Controls.Add(this.m_btnConfigPrinter);
			this.Controls.Add(this.m_bannerImage);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "PrintForm";
			this.ShowInTaskbar = false;
			this.Load += new System.EventHandler(this.OnFormLoad);
			this.m_tabMain.ResumeLayout(false);
			this.m_tabPreview.ResumeLayout(false);
			this.m_tabDataLayout.ResumeLayout(false);
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
		private System.Windows.Forms.ImageList m_ilTabIcons;
		private System.Windows.Forms.CheckBox m_cbExpire;
		private System.Windows.Forms.CheckBox m_cbAutoType;
		private System.Windows.Forms.CheckBox m_cbLastAccess;
		private System.Windows.Forms.CheckBox m_cbLastMod;
		private System.Windows.Forms.CheckBox m_cbCreation;
		private System.Windows.Forms.LinkLabel m_linkDeselectAllFields;
		private System.Windows.Forms.LinkLabel m_linkSelectAllFields;
		private System.Windows.Forms.CheckBox m_cbGroups;
	}
}