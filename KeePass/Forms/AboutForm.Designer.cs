namespace KeePass.Forms
{
	partial class AboutForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutForm));
			this.m_bannerImage = new System.Windows.Forms.PictureBox();
			this.m_lblCopyright = new System.Windows.Forms.Label();
			this.m_lblOsi = new System.Windows.Forms.Label();
			this.m_lblGpl = new System.Windows.Forms.Label();
			this.m_linkHomepage = new System.Windows.Forms.LinkLabel();
			this.m_linkHelp = new System.Windows.Forms.LinkLabel();
			this.m_linkLicense = new System.Windows.Forms.LinkLabel();
			this.m_linkAcknowledgements = new System.Windows.Forms.LinkLabel();
			this.m_linkDonate = new System.Windows.Forms.LinkLabel();
			this.m_btnOK = new System.Windows.Forms.Button();
			this.m_lvComponents = new System.Windows.Forms.ListView();
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
			this.SuspendLayout();
			// 
			// m_bannerImage
			// 
			resources.ApplyResources(this.m_bannerImage, "m_bannerImage");
			this.m_bannerImage.Name = "m_bannerImage";
			this.m_bannerImage.TabStop = false;
			// 
			// m_lblCopyright
			// 
			resources.ApplyResources(this.m_lblCopyright, "m_lblCopyright");
			this.m_lblCopyright.Name = "m_lblCopyright";
			// 
			// m_lblOsi
			// 
			resources.ApplyResources(this.m_lblOsi, "m_lblOsi");
			this.m_lblOsi.Name = "m_lblOsi";
			// 
			// m_lblGpl
			// 
			resources.ApplyResources(this.m_lblGpl, "m_lblGpl");
			this.m_lblGpl.Name = "m_lblGpl";
			// 
			// m_linkHomepage
			// 
			resources.ApplyResources(this.m_linkHomepage, "m_linkHomepage");
			this.m_linkHomepage.Name = "m_linkHomepage";
			this.m_linkHomepage.TabStop = true;
			this.m_linkHomepage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnLinkHomepage);
			// 
			// m_linkHelp
			// 
			resources.ApplyResources(this.m_linkHelp, "m_linkHelp");
			this.m_linkHelp.Name = "m_linkHelp";
			this.m_linkHelp.TabStop = true;
			this.m_linkHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnLinkHelpFile);
			// 
			// m_linkLicense
			// 
			resources.ApplyResources(this.m_linkLicense, "m_linkLicense");
			this.m_linkLicense.Name = "m_linkLicense";
			this.m_linkLicense.TabStop = true;
			this.m_linkLicense.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnLinkLicenseFile);
			// 
			// m_linkAcknowledgements
			// 
			resources.ApplyResources(this.m_linkAcknowledgements, "m_linkAcknowledgements");
			this.m_linkAcknowledgements.Name = "m_linkAcknowledgements";
			this.m_linkAcknowledgements.TabStop = true;
			this.m_linkAcknowledgements.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnLinkAcknowledgements);
			// 
			// m_linkDonate
			// 
			resources.ApplyResources(this.m_linkDonate, "m_linkDonate");
			this.m_linkDonate.Name = "m_linkDonate";
			this.m_linkDonate.TabStop = true;
			this.m_linkDonate.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnLinkDonate);
			// 
			// m_btnOK
			// 
			this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			resources.ApplyResources(this.m_btnOK, "m_btnOK");
			this.m_btnOK.Name = "m_btnOK";
			this.m_btnOK.UseVisualStyleBackColor = true;
			// 
			// m_lvComponents
			// 
			this.m_lvComponents.FullRowSelect = true;
			this.m_lvComponents.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			resources.ApplyResources(this.m_lvComponents, "m_lvComponents");
			this.m_lvComponents.Name = "m_lvComponents";
			this.m_lvComponents.UseCompatibleStateImageBehavior = false;
			this.m_lvComponents.View = System.Windows.Forms.View.Details;
			// 
			// AboutForm
			// 
			this.AcceptButton = this.m_btnOK;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.m_btnOK;
			this.Controls.Add(this.m_lvComponents);
			this.Controls.Add(this.m_btnOK);
			this.Controls.Add(this.m_linkDonate);
			this.Controls.Add(this.m_linkAcknowledgements);
			this.Controls.Add(this.m_linkLicense);
			this.Controls.Add(this.m_linkHelp);
			this.Controls.Add(this.m_linkHomepage);
			this.Controls.Add(this.m_lblGpl);
			this.Controls.Add(this.m_lblOsi);
			this.Controls.Add(this.m_lblCopyright);
			this.Controls.Add(this.m_bannerImage);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AboutForm";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Load += new System.EventHandler(this.OnFormLoad);
			((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox m_bannerImage;
		private System.Windows.Forms.Label m_lblCopyright;
		private System.Windows.Forms.Label m_lblOsi;
		private System.Windows.Forms.Label m_lblGpl;
		private System.Windows.Forms.LinkLabel m_linkHomepage;
		private System.Windows.Forms.LinkLabel m_linkHelp;
		private System.Windows.Forms.LinkLabel m_linkLicense;
		private System.Windows.Forms.LinkLabel m_linkAcknowledgements;
		private System.Windows.Forms.LinkLabel m_linkDonate;
		private System.Windows.Forms.Button m_btnOK;
		private System.Windows.Forms.ListView m_lvComponents;
	}
}