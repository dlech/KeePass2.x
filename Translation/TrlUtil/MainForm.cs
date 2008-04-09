/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2008 Dominik Reichl <dominik.reichl@t-online.de>

  This program is free software; you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation; either version 2 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, write to the Free Software
  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;

using KeePass.Resources;
using KeePassLib.Resources;
using KeePassLib.Translation;
using KeePassLib.Utility;

namespace TrlUtil
{
	public partial class MainForm : Form
	{
		private KPTranslation m_trl = new KPTranslation();
		private string m_strFile = string.Empty;

		private ImageList m_ilStr = new ImageList();

		private const string m_strFileFilter = "KeePass Translation (*.lngx)|*.lngx|All Files (*.*)|*.*";

		private KPControlCustomization m_kpccLast = null;

		private const int m_inxWindow = 6;
		private const int m_inxMissing = 1;
		private const int m_inxOk = 4;
		private const int m_inxWarning = 5;

		private bool m_bModified = false;

		public MainForm()
		{
			InitializeComponent();
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			this.CreateStringTableUI();

			m_trl.FormsList = FormTrlMgr.CreateListOfCurrentVersion();
			this.UpdateControlTree();
		}

		private void CreateStringTableUI()
		{
			int nWidth = m_lvStrings.ClientSize.Width - 20;

			m_ilStr.ColorDepth = ColorDepth.Depth32Bit;
			m_ilStr.ImageSize = new Size(16, 16);
			m_ilStr.Images.Add(Properties.Resources.B16x16_Binary);
			m_ilStr.Images.Add(Properties.Resources.B16x16_KRec_Record);
			m_ilStr.Images.Add(Properties.Resources.B16x16_LedGreen);
			m_ilStr.Images.Add(Properties.Resources.B16x16_LedLightBlue);
			m_ilStr.Images.Add(Properties.Resources.B16x16_LedLightGreen);
			m_ilStr.Images.Add(Properties.Resources.B16x16_LedOrange);
			m_ilStr.Images.Add(Properties.Resources.B16x16_View_Remove);

			m_lvStrings.SmallImageList = m_ilStr;
			m_tvControls.ImageList = m_ilStr;

			m_lvStrings.Columns.Add("ID", nWidth / 5);
			m_lvStrings.Columns.Add("English", (nWidth * 2) / 5);
			m_lvStrings.Columns.Add("Translated", (nWidth * 2) / 5);

			m_trl.StringTablesList.Clear();
			KPStringTable kpstP = new KPStringTable();
			kpstP.Name = "KeePass.Resources.KPRes";
			m_trl.StringTablesList.Add(kpstP);
			KPStringTable kpstL = new KPStringTable();
			kpstL.Name = "KeePassLib.Resources.KLRes";
			m_trl.StringTablesList.Add(kpstL);
			KPStringTable kpstM = new KPStringTable();
			kpstM.Name = "KeePass.Forms.MainForm.m_menuMain";
			m_trl.StringTablesList.Add(kpstM);
			KPStringTable kpstE = new KPStringTable();
			kpstE.Name = "KeePass.Forms.MainForm.m_ctxPwList";
			m_trl.StringTablesList.Add(kpstE);
			KPStringTable kpstG = new KPStringTable();
			kpstG.Name = "KeePass.Forms.MainForm.m_ctxGroupList";
			m_trl.StringTablesList.Add(kpstG);
			KPStringTable kpstT = new KPStringTable();
			kpstT.Name = "KeePass.Forms.MainForm.m_ctxTray";
			m_trl.StringTablesList.Add(kpstT);

			Type tKP = typeof(KPRes);
			ListViewGroup lvg = new ListViewGroup("KeePass Strings");
			m_lvStrings.Groups.Add(lvg);
			foreach(string strKey in KPRes.GetKeyNames())
			{
				PropertyInfo pi = tKP.GetProperty(strKey);
				MethodInfo mi = pi.GetGetMethod();
				if(mi.ReturnType != typeof(string))
				{
					MessageBox.Show("Return type is not string:\r\n" +
						strKey, "TrlUtil: Fatal error!", MessageBoxButtons.OK,
						MessageBoxIcon.Error);
					return;
				}

				string strEng = mi.Invoke(null, null) as string;
				if(strEng == null)
				{
					MessageBox.Show("English string is null:\r\n" +
						strKey, "TrlUtil: Fatal error!", MessageBoxButtons.OK,
						MessageBoxIcon.Error);
					return;
				}

				KPStringTableItem kpstItem = new KPStringTableItem();
				kpstItem.Name = strKey;
				kpstP.StringsList.Add(kpstItem);

				ListViewItem lvi = new ListViewItem();
				lvi.Group = lvg;
				lvi.Text = strKey;
				lvi.SubItems.Add(strEng);
				lvi.SubItems.Add(string.Empty);
				lvi.Tag = kpstItem;
				lvi.ImageIndex = 0;
				m_lvStrings.Items.Add(lvi);
			}

			Type tKL = typeof(KLRes);
			lvg = new ListViewGroup("KeePass Library Strings");
			m_lvStrings.Groups.Add(lvg);
			foreach(string strLibKey in KLRes.GetKeyNames())
			{
				PropertyInfo pi = tKL.GetProperty(strLibKey);
				MethodInfo mi = pi.GetGetMethod();
				if(mi.ReturnType != typeof(string))
				{
					MessageBox.Show("Return type is not string:\r\n" +
						strLibKey, "TrlUtil: Fatal error!", MessageBoxButtons.OK,
						MessageBoxIcon.Error);
					return;
				}

				string strEng = mi.Invoke(null, null) as string;
				if(strEng == null)
				{
					MessageBox.Show("English string is null:\r\n" +
						strLibKey, "TrlUtil: Fatal error!", MessageBoxButtons.OK,
						MessageBoxIcon.Error);
					return;
				}

				KPStringTableItem kpstItem = new KPStringTableItem();
				kpstItem.Name = strLibKey;
				kpstL.StringsList.Add(kpstItem);

				ListViewItem lvi = new ListViewItem();
				lvi.Group = lvg;
				lvi.Text = strLibKey;
				lvi.SubItems.Add(strEng);
				lvi.SubItems.Add(string.Empty);
				lvi.Tag = kpstItem;
				lvi.ImageIndex = 0;
				m_lvStrings.Items.Add(lvi);
			}

			lvg = new ListViewGroup("Main Menu Commands");
			m_lvStrings.Groups.Add(lvg);
			KeePass.Forms.MainForm mf = new KeePass.Forms.MainForm();
			TrlAddMenuCommands(kpstM, lvg, mf.MainMenu.Items);

			lvg = new ListViewGroup("Entry Context Menu Commands");
			m_lvStrings.Groups.Add(lvg);
			TrlAddMenuCommands(kpstE, lvg, mf.EntryContextMenu.Items);

			lvg = new ListViewGroup("Group Context Menu Commands");
			m_lvStrings.Groups.Add(lvg);
			TrlAddMenuCommands(kpstG, lvg, mf.GroupContextMenu.Items);

			lvg = new ListViewGroup("System Tray Context Menu Commands");
			m_lvStrings.Groups.Add(lvg);
			TrlAddMenuCommands(kpstT, lvg, mf.TrayContextMenu.Items);
		}

		private void TrlAddMenuCommands(KPStringTable kpst, ListViewGroup grp,
			ToolStripItemCollection tsic)
		{
			foreach(ToolStripItem tsi in tsic)
			{
				if(tsi.Text.Length == 0) continue;

				KPStringTableItem kpstItem = new KPStringTableItem();
				kpstItem.Name = tsi.Name;
				kpst.StringsList.Add(kpstItem);

				ListViewItem lvi = new ListViewItem();
				lvi.Group = grp;
				lvi.Text = tsi.Name;
				lvi.SubItems.Add(tsi.Text);
				lvi.SubItems.Add(string.Empty);
				lvi.Tag = kpstItem;
				lvi.ImageIndex = 0;
				m_lvStrings.Items.Add(lvi);

				ToolStripMenuItem tsmi = tsi as ToolStripMenuItem;
				if(tsmi != null) TrlAddMenuCommands(kpst, grp, tsmi.DropDownItems);
			}
		}

		private void UpdateStringTableUI()
		{
			foreach(ListViewItem lvi in m_lvStrings.Items)
			{
				KPStringTableItem kpstItem  = lvi.Tag as KPStringTableItem;
				Debug.Assert(kpstItem != null);
				if(kpstItem == null) continue;

				lvi.SubItems[2].Text = kpstItem.Value;
			}
		}

		private void UpdateControlTree()
		{
			FormTrlMgr.RenderToTreeControl(m_trl.FormsList, m_tvControls);
			UpdateStatusImages(null);
		}

		private void OnLinkLangCodeClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			try
			{
				Process.Start(@"http://en.wikipedia.org/wiki/List_of_ISO_639-1_codes");
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message, "TrlUtil",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private void OnFileOpen(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.CheckFileExists = true;
			ofd.CheckPathExists = true;
			ofd.Filter = m_strFileFilter;
			ofd.FilterIndex = 1;
			ofd.Multiselect = false;
			ofd.RestoreDirectory = false;
			ofd.Title = "Open KeePass Translation";
			ofd.ValidateNames = true;
			if(ofd.ShowDialog() != DialogResult.OK)
				return;

			KPTranslation kpTrl = null;
			try { kpTrl = KPTranslation.LoadFromFile(ofd.FileName); }
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message, "TrlUtil", MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				return;
			}

			m_strFile = ofd.FileName;

			m_trl.Properties = kpTrl.Properties;
			foreach(KPStringTable kpstNew in kpTrl.StringTablesList)
			{
				foreach(KPStringTable kpstInto in m_trl.StringTablesList)
				{
					if(kpstInto.Name == kpstNew.Name)
						MergeInStringTable(kpstInto, kpstNew);
				}
			}

			FormTrlMgr.MergeForms(m_trl.FormsList, kpTrl.FormsList);

			m_tbNameEng.Text = m_trl.Properties.NameEnglish;
			m_tbNameLcl.Text = m_trl.Properties.NameNative;
			m_tbLangID.Text = m_trl.Properties.Iso6391Code;
			m_tbAuthorName.Text = m_trl.Properties.AuthorName;
			m_tbAuthorContact.Text = m_trl.Properties.AuthorContact;
			this.UpdateStringTableUI();
			this.UpdateStatusImages(null);
		}

		private void MergeInStringTable(KPStringTable tbInto, KPStringTable tbSource)
		{
			foreach(KPStringTableItem kpSrc in tbSource.StringsList)
			{
				foreach(KPStringTableItem kpDst in tbInto.StringsList)
				{
					if(kpSrc.Name == kpDst.Name)
					{
						if(kpSrc.Value.Length > 0)
							kpDst.Value = kpSrc.Value;
					}
				}
			}
		}

		private void UpdateInternalTranslation()
		{
			m_trl.Properties.NameEnglish = StrUtil.SafeXmlString(m_tbNameEng.Text);
			m_trl.Properties.NameNative = StrUtil.SafeXmlString(m_tbNameLcl.Text);
			m_trl.Properties.Iso6391Code = StrUtil.SafeXmlString(m_tbLangID.Text);
			m_trl.Properties.AuthorName = StrUtil.SafeXmlString(m_tbAuthorName.Text);
			m_trl.Properties.AuthorContact = StrUtil.SafeXmlString(m_tbAuthorContact.Text);
		}

		private void UpdateStatusImages(TreeNodeCollection vtn)
		{
			if(vtn == null) vtn = m_tvControls.Nodes;

			foreach(TreeNode tn in vtn)
			{
				KPFormCustomization kpfc = tn.Tag as KPFormCustomization;
				KPControlCustomization kpcc = tn.Tag as KPControlCustomization;

				if(kpfc != null)
				{
					tn.ImageIndex = m_inxWindow;
					tn.SelectedImageIndex = m_inxWindow;
				}
				else if(kpcc != null)
				{
					int iCurrentImage = tn.ImageIndex, iNewImage;

					if((kpcc.TextEnglish.Length > 0) && (kpcc.Text.Length > 0))
						iNewImage = m_inxOk;
					else if((kpcc.TextEnglish.Length > 0) && (kpcc.Text.Length == 0))
						iNewImage = m_inxMissing;
					else if((kpcc.TextEnglish.Length == 0) && (kpcc.Text.Length == 0))
						iNewImage = m_inxOk;
					else if((kpcc.TextEnglish.Length == 0) && (kpcc.Text.Length > 0))
						iNewImage = m_inxWarning;
					else
						iNewImage = m_inxWarning;

					if(iNewImage != iCurrentImage)
					{
						tn.ImageIndex = iNewImage;
						tn.SelectedImageIndex = iNewImage;
					}
				}
				else { Debug.Assert(false); }

				if(tn.Nodes != null) UpdateStatusImages(tn.Nodes);
			}
		}

		private void OnFileSave(object sender, EventArgs e)
		{
			UpdateInternalTranslation();

			if(m_strFile.Length == 0)
			{
				OnFileSaveAs(sender, e);
				return;
			}

			try
			{
				KPTranslation.SaveToFile(m_trl, m_strFile);
				m_bModified = false;
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message, "TrlUtil", MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
			}
		}

		private void OnFileExit(object sender, EventArgs e)
		{
			if(m_bModified) OnFileSaveAs(sender, e);

			this.Close();
		}

		private void OnStringsSelectedIndexChanged(object sender, EventArgs e)
		{
			ListView.SelectedListViewItemCollection lvsic =
				m_lvStrings.SelectedItems;
			if(lvsic.Count != 1)
			{
				m_tbStrEng.Text = string.Empty;
				m_tbStrTrl.Text = string.Empty;
				return;
			}

			KPStringTableItem kpstItem = lvsic[0].Tag as KPStringTableItem;
			Debug.Assert(kpstItem != null);
			if(kpstItem == null) return;

			m_tbStrEng.Text = lvsic[0].SubItems[1].Text;
			m_tbStrTrl.Text = lvsic[0].SubItems[2].Text;
		}

		private void OnStrKeyDown(object sender, KeyEventArgs e)
		{
			if((e.KeyCode == Keys.Return) || (e.KeyCode == Keys.Enter))
			{
				e.Handled = true;

				ListView.SelectedListViewItemCollection lvsic =
					m_lvStrings.SelectedItems;
				if(lvsic.Count != 1) return;

				KPStringTableItem kpstItem = lvsic[0].Tag as KPStringTableItem;
				if(kpstItem == null)
				{
					Debug.Assert(false);
					return;
				}

				kpstItem.Value = StrUtil.SafeXmlString(m_tbStrTrl.Text);
				this.UpdateStringTableUI();

				int iIndex = lvsic[0].Index;
				if(iIndex < m_lvStrings.Items.Count - 1)
				{
					lvsic[0].Selected = false;
					m_lvStrings.Items[iIndex + 1].Selected = true;
					m_lvStrings.FocusedItem = m_lvStrings.Items[iIndex + 1];
				}

				m_bModified = true;
			}
		}

		private void OnStrKeyUp(object sender, KeyEventArgs e)
		{
			if((e.KeyCode == Keys.Return) || (e.KeyCode == Keys.Enter))
				e.Handled = true;
		}

		private void OnFileSaveAs(object sender, EventArgs e)
		{
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.AddExtension = true;
			sfd.DefaultExt = "lngx";
			sfd.FileName = m_tbNameEng.Text + ".lngx";
			sfd.Filter = m_strFileFilter;
			sfd.FilterIndex = 1;
			sfd.OverwritePrompt = true;
			sfd.RestoreDirectory = false;
			sfd.Title = "Save KeePass Translation";
			sfd.ValidateNames = true;

			if(sfd.ShowDialog() != DialogResult.OK) return;

			m_strFile = sfd.FileName;

			OnFileSave(sender, e);
		}

		private void OnStrDoubleClick(object sender, EventArgs e)
		{
			this.ActiveControl = m_tbStrTrl;
		}

		private void OnCustomControlsAfterSelect(object sender, TreeViewEventArgs e)
		{
			ShowCustomControlProps(e.Node.Tag as KPControlCustomization);
		}

		private void ShowCustomControlProps(KPControlCustomization kpcc)
		{
			if(kpcc == null) return; // No assert

			m_kpccLast = kpcc;

			m_tbCtrlEngText.Text = m_kpccLast.TextEnglish;
			m_tbCtrlTrlText.Text = m_kpccLast.Text;

			m_tbLayoutX.Text = KpccLayout.ToControlRelativeString(m_kpccLast.Layout.X);
			m_tbLayoutY.Text = KpccLayout.ToControlRelativeString(m_kpccLast.Layout.Y);
			m_tbLayoutW.Text = KpccLayout.ToControlRelativeString(m_kpccLast.Layout.Width);
			m_tbLayoutH.Text = KpccLayout.ToControlRelativeString(m_kpccLast.Layout.Height);
		}

		private void OnCtrlTrlTextChanged(object sender, EventArgs e)
		{
			string strText = m_tbCtrlTrlText.Text;
			if((m_kpccLast != null) && (m_kpccLast.Text != strText))
			{
				m_kpccLast.Text = StrUtil.SafeXmlString(m_tbCtrlTrlText.Text);
				m_bModified = true;
			}

			UpdateStatusImages(null);
		}

		private void OnLayoutXTextChanged(object sender, EventArgs e)
		{
			if(m_kpccLast != null)
			{
				m_kpccLast.Layout.SetControlRelativeValue(
					KpccLayout.LayoutParameterEx.X, m_tbLayoutX.Text);
				m_bModified = true;
			}
		}

		private void OnLayoutYTextChanged(object sender, EventArgs e)
		{
			if(m_kpccLast != null)
			{
				m_kpccLast.Layout.SetControlRelativeValue(
					KpccLayout.LayoutParameterEx.Y, m_tbLayoutY.Text);
				m_bModified = true;
			}
		}

		private void OnLayoutWidthTextChanged(object sender, EventArgs e)
		{
			if(m_kpccLast != null)
			{
				m_kpccLast.Layout.SetControlRelativeValue(
					KpccLayout.LayoutParameterEx.Width, m_tbLayoutW.Text);
				m_bModified = true;
			}
		}

		private void OnLayoutHeightTextChanged(object sender, EventArgs e)
		{
			if(m_kpccLast != null)
			{
				m_kpccLast.Layout.SetControlRelativeValue(
					KpccLayout.LayoutParameterEx.Height, m_tbLayoutH.Text);
				m_bModified = true;
			}
		}

		private void OnCtrlTrlTextKeyDown(object sender, KeyEventArgs e)
		{
			if((e.KeyCode == Keys.Return) || (e.KeyCode == Keys.Enter))
			{
				e.Handled = true;

				TreeNode tn = m_tvControls.SelectedNode;
				if(tn == null) return;

				try
				{
					TreeNode tnNew = tn.NextNode;
					if(tnNew != null) m_tvControls.SelectedNode = tnNew;
				}
				catch(Exception) { Debug.Assert(false); }
			}
		}

		private void OnCtrlTrlTextKeyUp(object sender, KeyEventArgs e)
		{
			if((e.KeyCode == Keys.Return) || (e.KeyCode == Keys.Enter))
				e.Handled = true;
		}
	}
}
