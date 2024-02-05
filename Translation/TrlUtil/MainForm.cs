/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2024 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

using KeePass.App;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;
using KeePass.Util.XmlSerialization;

using KeePassLib;
using KeePassLib.Delegates;
using KeePassLib.Resources;
using KeePassLib.Serialization;
using KeePassLib.Translation;
using KeePassLib.Utility;

using TrlUtil.App;

using NativeLib = KeePassLib.Native.NativeLib;

namespace TrlUtil
{
	public partial class MainForm : Form
	{
		private readonly KPTranslation m_trl = new KPTranslation();
		private string m_strFile = string.Empty;

		private readonly ImageList m_ilStr = new ImageList();

		private const string m_strFileFilter = "KeePass Translation (*.lngx)|*.lngx|All Files (*.*)|*.*";
		private static readonly string[] m_vEmpty = new string[2] {
			@"<DYN>", @"<>" };

		private KPControlCustomization m_kpccLast = null;
		private readonly Dictionary<string, ListViewItem> m_dStrings =
			new Dictionary<string, ListViewItem>();
		private readonly Dictionary<string, TreeNode> m_dControls =
			new Dictionary<string, TreeNode>();

		private const int m_inxWindow = 6;
		private const int m_inxMissing = 1;
		private const int m_inxOk = 4;
		private const int m_inxWarning = 5;

		private bool m_bModified = false;
		private uint m_uBlockTabAuto = 0;

		private PreviewForm m_prev = new PreviewForm();

		private static readonly char[] g_vNewLine = new char[] { '\r', '\n' };

		private delegate void ImportFn(KPTranslation trlInto, IOConnectionInfo ioc);

		public MainForm()
		{
			InitializeComponent();
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			this.Icon = Properties.Resources.KeePass;

			UIUtil.AssignShortcut(m_menuFileOpen, Keys.Control | Keys.O);
			UIUtil.AssignShortcut(m_menuFileSave, Keys.Control | Keys.S);
			UIUtil.AssignShortcut(m_menuEditNextUntrl, Keys.Control | Keys.U);
			UIUtil.AssignShortcut(m_menuEditMoveUnusedToDialog, Keys.Control | Keys.M);

			UIUtil.ConfigureTbButton(m_tbOpen, m_menuFileOpen.Text, null, m_menuFileOpen);
			UIUtil.ConfigureTbButton(m_tbSave, m_menuFileSave.Text, null, m_menuFileSave);
			UIUtil.ConfigureTbButton(m_tbNextUntrl, m_menuEditNextUntrl.Text, null, m_menuEditNextUntrl);
			UIUtil.ConfigureTbButton(m_tbMoveUnusedToDialog, m_menuEditMoveUnusedToDialog.Text,
				null, m_menuEditMoveUnusedToDialog);

			m_trl.Forms = FormTrlMgr.CreateListOfCurrentVersion();
			m_rtbUnusedText.SimpleTextOnly = true;

			string strSearchTr = ((WinUtil.IsAtLeastWindowsVista ?
				string.Empty : " ") + "Search in active tab...");
			UIUtil.SetCueBanner(m_tbFind, strSearchTr);

			CreateStringTableUI();
			UpdateControlTree();

			UpdatePreviewForm();
			if(m_prev != null) m_prev.Show();
			else { Debug.Assert(false); }

			try { this.DoubleBuffered = true; }
			catch(Exception) { Debug.Assert(false); }

			UpdateUIState();
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

			m_trl.StringTables.Clear();

			GFunc<string, KPStringTable> fAddST = delegate(string strName)
			{
				KPStringTable kpst = new KPStringTable();
				kpst.Name = strName;
				m_trl.StringTables.Add(kpst);
				return kpst;
			};

			GAction<KPStringTable, ToolStrip, string> fAddMI = delegate(
				KPStringTable kpst, ToolStrip ts, string strGroupName)
			{
				ListViewGroup g = new ListViewGroup(strGroupName);
				m_lvStrings.Groups.Add(g);
				TrlAddMenuItems(kpst, g, ts.Items);
			};

			KeePass.Forms.MainForm mf = new KeePass.Forms.MainForm();
			KeePass.Forms.PwEntryForm ef = new KeePass.Forms.PwEntryForm();
			KeePass.Forms.DataEditorForm df = new KeePass.Forms.DataEditorForm();
			KeePass.Forms.EcasTriggersForm tf = new KeePass.Forms.EcasTriggersForm();

			KPStringTable kpstP = fAddST("KeePass.Resources.KPRes");
			KPStringTable kpstL = fAddST("KeePassLib.Resources.KLRes");
			KPStringTable kpstMM = fAddST("KeePass.Forms.MainForm.m_menuMain");
			KPStringTable kpstMG = fAddST("KeePass.Forms.MainForm.m_ctxGroupList");
			KPStringTable kpstME = fAddST("KeePass.Forms.MainForm.m_ctxPwList");
			KPStringTable kpstMT = fAddST("KeePass.Forms.MainForm.m_ctxTray");
			KPStringTable kpstET = fAddST("KeePass.Forms.PwEntryForm.m_ctxTools");
			KPStringTable kpstED = fAddST("KeePass.Forms.PwEntryForm.m_ctxDefaultTimes");
			KPStringTable kpstES = fAddST("KeePass.Forms.PwEntryForm.m_ctxStr");
			KPStringTable kpstEB = fAddST("KeePass.Forms.PwEntryForm.m_ctxBinAttach");
			KPStringTable kpstEA = fAddST("KeePass.Forms.PwEntryForm.m_ctxAutoType");
			KPStringTable kpstEH = fAddST("KeePass.Forms.PwEntryForm.m_ctxHst");
			KPStringTable kpstDM = fAddST("KeePass.Forms.DataEditorForm.m_menuMain");
			KPStringTable kpstTT = fAddST("KeePass.Forms.EcasTriggersForm.m_ctxTools");
			KPStringTable kpstS = fAddST("KeePassLib.Resources.KSRes");

			TrlAddResClass(kpstP, typeof(KPRes), KPRes.GetKeyNames(), "KeePass Strings");
			TrlAddResClass(kpstL, typeof(KLRes), KLRes.GetKeyNames(), "KeePassLib Strings");

			fAddMI(kpstMM, mf.MainMenu, "Main Menu Commands");
			fAddMI(kpstMG, mf.GroupContextMenu, "Group Context Menu Commands");
			fAddMI(kpstME, mf.EntryContextMenu, "Entry Context Menu Commands");
			fAddMI(kpstMT, mf.TrayContextMenu, "System Tray Context Menu Commands");
			fAddMI(kpstET, ef.ToolsContextMenu, "Entry Tools Context Menu Commands");
			fAddMI(kpstED, ef.DefaultTimesContextMenu, "Default Times Context Menu Commands");
			fAddMI(kpstES, ef.StringsContextMenu, "Strings Context Menu Commands");
			fAddMI(kpstEB, ef.AttachmentsContextMenu, "Entry Attachments Context Menu Commands");
			fAddMI(kpstEA, ef.AutoTypeContextMenu, "Auto-Type Context Menu Commands");
			fAddMI(kpstEH, ef.HistoryContextMenu, "History Context Menu Commands");
			fAddMI(kpstDM, df.MainMenuEx, "Data Editor Menu Commands");
			fAddMI(kpstTT, tf.ToolsContextMenu, "Ecas Trigger Tools Context Menu Commands");

			TrlAddResClass(kpstS, typeof(KSRes), KSRes.GetKeyNames(), "KeePassLibSD Strings");
		}

		private void TrlAddResClass(KPStringTable kpst, Type tRes,
			string[] vKeyNames, string strGroupName)
		{
			ListViewGroup lvg = new ListViewGroup(strGroupName);
			m_lvStrings.Groups.Add(lvg);

			foreach(string strKey in vKeyNames)
			{
				PropertyInfo pi = tRes.GetProperty(strKey);
				MethodInfo mi = pi.GetGetMethod();
				if(mi.ReturnType != typeof(string))
				{
					MessageBox.Show(this, "Return type is not string:\r\n" +
						strKey, TuDefs.ProductName + ": Fatal Error!",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				string strEng = (mi.Invoke(null, null) as string);
				if(strEng == null)
				{
					MessageBox.Show(this, "English string is null:\r\n" +
						strKey, TuDefs.ProductName + ": Fatal Error!",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				KPStringTableItem it = new KPStringTableItem();
				it.Name = strKey;
				it.ValueEnglish = strEng;
				kpst.Strings.Add(it);

				ListViewItem lvi = new ListViewItem();
				lvi.Group = lvg;
				lvi.Text = strKey;
				lvi.SubItems.Add(strEng);
				lvi.SubItems.Add(string.Empty);
				lvi.Tag = it;
				lvi.ImageIndex = 0;

				m_lvStrings.Items.Add(lvi);
				m_dStrings[kpst.Name + "." + strKey] = lvi;
			}
		}

		private void TrlAddMenuItems(KPStringTable kpst, ListViewGroup grp,
			ToolStripItemCollection tsic)
		{
			foreach(ToolStripItem tsi in tsic)
			{
				if((tsi.Text.Length != 0) && (!tsi.Text.StartsWith("<") ||
					!tsi.Text.EndsWith(">")))
				{
					KPStringTableItem kpstItem = new KPStringTableItem();
					kpstItem.Name = tsi.Name;
					kpstItem.ValueEnglish = tsi.Text;
					kpst.Strings.Add(kpstItem);

					ListViewItem lvi = new ListViewItem();
					lvi.Group = grp;
					lvi.Text = tsi.Name;
					lvi.SubItems.Add(tsi.Text);
					lvi.SubItems.Add(string.Empty);
					lvi.Tag = kpstItem;
					lvi.ImageIndex = 0;

					m_lvStrings.Items.Add(lvi);
					m_dStrings[kpst.Name + "." + kpstItem.Name] = lvi;
				}

				ToolStripMenuItem tsmi = (tsi as ToolStripMenuItem);
				if(tsmi != null) TrlAddMenuItems(kpst, grp, tsmi.DropDownItems);
			}
		}

		private void UpdateStringTableUI()
		{
			foreach(ListViewItem lvi in m_lvStrings.Items)
			{
				KPStringTableItem kpstItem = (lvi.Tag as KPStringTableItem);
				Debug.Assert(kpstItem != null);
				if(kpstItem == null) continue;

				lvi.SubItems[2].Text = kpstItem.Value;
			}
		}

		private void UpdateControlTree()
		{
			FormTrlMgr.RenderToTreeControl(m_trl.Forms, m_tvControls, m_dControls);
			UpdateStatusImages(null);

			m_tvControls.SelectedNode = m_tvControls.Nodes[0];
		}

		private void UpdateUIState()
		{
			bool bTrlTab = ((m_tabMain.SelectedTab == m_tabStrings) ||
				(m_tabMain.SelectedTab == m_tabDialogs));

			m_menuEditNextUntrl.Enabled = bTrlTab;
			m_tbNextUntrl.Enabled = bTrlTab;

			bool b = CanMoveUnusedToDialog();
			m_menuEditMoveUnusedToDialog.Enabled = b;
			m_tbMoveUnusedToDialog.Enabled = b;

			m_tbFind.Enabled = bTrlTab;

			string str = TuDefs.ProductName + " " + PwDefs.VersionString;
			if(!string.IsNullOrEmpty(m_strFile))
			{
				string strFile = UrlUtil.GetFileName(m_strFile);
				if(!string.IsNullOrEmpty(strFile))
					str = strFile + " - " + str;
			}
			this.Text = str;
		}

		private void OnLinkLangCodeClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			WinUtil.OpenUrl("https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes", null);
		}

		private void SetCurrentFile(string strFilePath)
		{
			if(strFilePath == null) { Debug.Assert(false); strFilePath = string.Empty; }

			m_strFile = strFilePath;
			Program.Config.Application.LastUsedFile = IOConnectionInfo.FromPath(strFilePath);

			UpdateUIState(); // Update title bar
		}

		private static void InitFileDialog(FileDialogEx dlg)
		{
			if(dlg == null) { Debug.Assert(false); return; }

			IOConnectionInfo ioDir = Program.Config.Application.LastUsedFile;
			if(!string.IsNullOrEmpty(ioDir.Path) && ioDir.IsLocalFile())
			{
				string strDir = UrlUtil.GetFileDirectory(ioDir.Path, false, true);
				if(Directory.Exists(strDir)) dlg.InitialDirectory = strDir;
			}
		}

		private void OnFileOpen(object sender, EventArgs e)
		{
			OpenFileDialogEx ofd = UIUtil.CreateOpenFileDialog("Open KeePass Translation",
				m_strFileFilter, 1, null, false, string.Empty);
			InitFileDialog(ofd);

			if(ofd.ShowDialog() != DialogResult.OK) return;

			KPTranslation kpTrl;
			try
			{
				XmlSerializerEx xs = new XmlSerializerEx(typeof(KPTranslation));
				kpTrl = KPTranslation.Load(ofd.FileName, xs);
			}
			catch(Exception ex)
			{
				MessageBox.Show(this, ex.Message, TuDefs.ProductName,
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			SetCurrentFile(ofd.FileName);

			StringBuilder sbUnusedText = new StringBuilder();
			if(kpTrl.UnusedText.Length > 0)
			{
				if(kpTrl.UnusedText.EndsWith("\r") || kpTrl.UnusedText.EndsWith("\n"))
					sbUnusedText.Append(kpTrl.UnusedText);
				else sbUnusedText.AppendLine(kpTrl.UnusedText);
			}

			m_trl.Properties = kpTrl.Properties;
			foreach(KPStringTable kpstNew in kpTrl.StringTables)
			{
				foreach(KPStringTable kpstInto in m_trl.StringTables)
				{
					if(kpstInto.Name == kpstNew.Name)
						MergeInStringTable(kpstInto, kpstNew, sbUnusedText);
				}
			}

			FormTrlMgr.MergeForms(m_trl.Forms, kpTrl.Forms, sbUnusedText);

			m_tbNameEng.Text = m_trl.Properties.NameEnglish;
			m_tbNameLcl.Text = m_trl.Properties.NameNative;
			m_tbLangID.Text = m_trl.Properties.Iso6391Code;
			m_tbAuthorName.Text = m_trl.Properties.AuthorName;
			m_tbAuthorContact.Text = m_trl.Properties.AuthorContact;
			m_cbRtl.Checked = m_trl.Properties.RightToLeft;

			m_rtbUnusedText.Text = sbUnusedText.ToString();

			UpdateStringTableUI();
			UpdateStatusImages(null);
			UpdatePreviewForm();
			if(m_tabMain.SelectedTab == m_tabValidation)
				PerformValidation(null);
		}

		private void MergeInStringTable(KPStringTable tbInto, KPStringTable tbSource,
			StringBuilder sbUnusedText)
		{
			foreach(KPStringTableItem kpSrc in tbSource.Strings)
			{
				bool bHasAssigned = false;
				foreach(KPStringTableItem kpDst in tbInto.Strings)
				{
					if(kpSrc.Name == kpDst.Name)
					{
						if(kpSrc.Value.Length > 0)
						{
							kpDst.Value = kpSrc.Value;
							bHasAssigned = true;
						}
					}
				}

				if(!bHasAssigned)
				{
					string strTrimmed = kpSrc.Value.Trim();
					if(strTrimmed.Length > 0) sbUnusedText.AppendLine(strTrimmed);
				}
			}
		}

		private void UpdateTranslationObject()
		{
			m_trl.Properties.Application = PwDefs.ProductName;
			m_trl.Properties.ApplicationVersion = PwDefs.VersionString;
			m_trl.Properties.Generator = TuDefs.ProductName;

			PwUuid pwUuid = new PwUuid(true);
			m_trl.Properties.FileUuid = pwUuid.ToHexString();

			m_trl.Properties.LastModified = DateTime.UtcNow.ToString("u");

			m_trl.Properties.NameEnglish = StrUtil.SafeXmlString(m_tbNameEng.Text);
			m_trl.Properties.NameNative = StrUtil.SafeXmlString(m_tbNameLcl.Text);
			m_trl.Properties.Iso6391Code = StrUtil.SafeXmlString(m_tbLangID.Text);
			m_trl.Properties.AuthorName = StrUtil.SafeXmlString(m_tbAuthorName.Text);
			m_trl.Properties.AuthorContact = StrUtil.SafeXmlString(m_tbAuthorContact.Text);
			m_trl.Properties.RightToLeft = m_cbRtl.Checked;

			m_trl.UnusedText = m_rtbUnusedText.Text;
		}

		private void UpdateStatusImages(TreeNodeCollection vtn)
		{
			if(vtn == null) vtn = m_tvControls.Nodes;

			foreach(TreeNode tn in vtn)
			{
				KPFormCustomization kpfc = (tn.Tag as KPFormCustomization);
				KPControlCustomization kpcc = (tn.Tag as KPControlCustomization);

				if(kpfc != null)
				{
					tn.ImageIndex = m_inxWindow;
					tn.SelectedImageIndex = m_inxWindow;
				}
				else if(kpcc != null)
				{
					int iCurrentImage = tn.ImageIndex, iNewImage;

					if(Array.IndexOf<string>(m_vEmpty, kpcc.TextEnglish) >= 0)
						iNewImage = ((kpcc.Text.Length == 0) ? m_inxOk : m_inxWarning);
					else if((kpcc.TextEnglish.Length > 0) && (kpcc.Text.Length > 0))
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
			if(string.IsNullOrEmpty(m_strFile))
			{
				OnFileSaveAs(sender, e);
				return;
			}

			try
			{
				UpdateTranslationObject();

				XmlSerializerEx xs = new XmlSerializerEx(typeof(KPTranslation));
				KPTranslation.Save(m_trl, m_strFile, xs);

				m_bModified = false;
				PerformValidation(m_strFile);
			}
			catch(Exception ex)
			{
				MessageBox.Show(this, ex.Message, TuDefs.ProductName,
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		/* private void ShowValidationWarning(string strText)
		{
			if(string.IsNullOrEmpty(strText)) { Debug.Assert(false); return; }

			int r = VistaTaskDialog.ShowMessageBoxEx(strText, "Validation Warning",
				TuDefs.ProductName, VtdIcon.Warning, this, "Continue saving",
				(int)DialogResult.OK, "Cancel", (int)DialogResult.Cancel);
			if(r < 0)
			{
				string str = strText + MessageService.NewParagraph +
					"Click [OK] to continue saving.";
				r = (int)MessageBox.Show(this, "Validation Warning!" +
					MessageService.NewParagraph + str, TuDefs.ProductName,
					MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
			}

			if(r == (int)DialogResult.Cancel) throw new OperationCanceledException();
		} */

		private void OnFormClosing(object sender, FormClosingEventArgs e)
		{
			if(m_bModified)
			{
				if(MessageService.AskYesNo("Save changes before closing the file?",
					TuDefs.ProductName))
					OnFileSave(sender, e);
				else m_bModified = false;
			}
			if(m_bModified) e.Cancel = true;
		}

		private void OnFileExit(object sender, EventArgs e)
		{
			Close();
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

			KPStringTableItem kpstItem = (lvsic[0].Tag as KPStringTableItem);
			Debug.Assert(kpstItem != null);
			if(kpstItem == null) return;

			UIUtil.SetMultilineText(m_tbStrEng, lvsic[0].SubItems[1].Text);
			m_tbStrTrl.Text = lvsic[0].SubItems[2].Text;
		}

		private void OnStrKeyDown(object sender, KeyEventArgs e)
		{
			if(e.KeyCode == Keys.Return) // Return == Enter
			{
				UIUtil.SetHandled(e, true);

				ListView.SelectedListViewItemCollection lvsic =
					m_lvStrings.SelectedItems;
				if(lvsic.Count != 1) return;

				KPStringTableItem kpstItem = (lvsic[0].Tag as KPStringTableItem);
				if(kpstItem == null)
				{
					Debug.Assert(false);
					return;
				}

				kpstItem.Value = StrUtil.SafeXmlString(m_tbStrTrl.Text);
				UpdateStringTableUI();

				int iIndex = lvsic[0].Index;
				if(iIndex < m_lvStrings.Items.Count - 1)
				{
					lvsic[0].Selected = false;
					UIUtil.SetFocusedItem(m_lvStrings, m_lvStrings.Items[
						iIndex + 1], true);
				}

				m_bModified = true;
			}
		}

		private void OnStrKeyUp(object sender, KeyEventArgs e)
		{
			if(e.KeyCode == Keys.Return) // Return == Enter
				UIUtil.SetHandled(e, true);
		}

		private void OnFileSaveAs(object sender, EventArgs e)
		{
			SaveFileDialogEx sfd = UIUtil.CreateSaveFileDialog("Save KeePass Translation",
				m_tbNameEng.Text + ".lngx", m_strFileFilter, 1, "lngx", string.Empty);
			InitFileDialog(sfd);

			if(sfd.ShowDialog() != DialogResult.OK) return;

			SetCurrentFile(sfd.FileName);

			OnFileSave(sender, e);
		}

		private void OnStrDoubleClick(object sender, EventArgs e)
		{
			UIUtil.SetFocus(m_tbStrTrl, this);
		}

		private void OnCustomControlsAfterSelect(object sender, TreeViewEventArgs e)
		{
			ShowCustomControlProps(e.Node.Tag as KPControlCustomization);
		}

		private void ShowCustomControlProps(KPControlCustomization kpcc)
		{
			m_kpccLast = kpcc;

			m_grpControl.Enabled = (kpcc != null);

			if(kpcc == null)
			{
				m_tbCtrlEngText.Text = string.Empty;
				m_tbCtrlTrlText.Text = string.Empty;

				m_tbLayoutX.Text = string.Empty;
				m_tbLayoutY.Text = string.Empty;
				m_tbLayoutW.Text = string.Empty;
				m_tbLayoutH.Text = string.Empty;
			}
			else
			{
				UIUtil.SetMultilineText(m_tbCtrlEngText, m_kpccLast.TextEnglish);
				m_tbCtrlTrlText.Text = m_kpccLast.Text;

				m_tbLayoutX.Text = KpccLayout.ToControlRelativeString(m_kpccLast.Layout.X);
				m_tbLayoutY.Text = KpccLayout.ToControlRelativeString(m_kpccLast.Layout.Y);
				m_tbLayoutW.Text = KpccLayout.ToControlRelativeString(m_kpccLast.Layout.Width);
				m_tbLayoutH.Text = KpccLayout.ToControlRelativeString(m_kpccLast.Layout.Height);
			}

			UpdateUIState();
			UpdatePreviewForm();
		}

		private void OnCtrlTrlTextChanged(object sender, EventArgs e)
		{
			string strText = StrUtil.SafeXmlString(m_tbCtrlTrlText.Text);
			if((m_kpccLast != null) && (m_kpccLast.Text != strText))
			{
				m_kpccLast.Text = strText;
				m_bModified = true;
			}

			UpdateStatusImages(null);
			UpdateUIState();
			UpdatePreviewForm();
		}

		private void OnLayoutXTextChanged(object sender, EventArgs e)
		{
			if(m_kpccLast != null)
			{
				m_kpccLast.Layout.SetControlRelativeValue(
					KpccLayout.LayoutParameterEx.X, m_tbLayoutX.Text);

				UpdatePreviewForm();
			}
		}

		private void OnLayoutYTextChanged(object sender, EventArgs e)
		{
			if(m_kpccLast != null)
			{
				m_kpccLast.Layout.SetControlRelativeValue(
					KpccLayout.LayoutParameterEx.Y, m_tbLayoutY.Text);

				UpdatePreviewForm();
			}
		}

		private void OnLayoutWidthTextChanged(object sender, EventArgs e)
		{
			if(m_kpccLast != null)
			{
				m_kpccLast.Layout.SetControlRelativeValue(
					KpccLayout.LayoutParameterEx.Width, m_tbLayoutW.Text);

				UpdatePreviewForm();
			}
		}

		private void OnLayoutHeightTextChanged(object sender, EventArgs e)
		{
			if(m_kpccLast != null)
			{
				m_kpccLast.Layout.SetControlRelativeValue(
					KpccLayout.LayoutParameterEx.Height, m_tbLayoutH.Text);

				UpdatePreviewForm();
			}
		}

		private void OnCtrlTrlTextKeyDown(object sender, KeyEventArgs e)
		{
			if(e.KeyCode == Keys.Return) // Return == Enter
			{
				UIUtil.SetHandled(e, true);

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
			if(e.KeyCode == Keys.Return) // Return == Enter
				UIUtil.SetHandled(e, true);
		}

		private void UpdatePreviewForm()
		{
			TreeNode tn = m_tvControls.SelectedNode;
			if(tn == null) return;
			if(tn.Parent != null) tn = tn.Parent;
			string strFormName = tn.Text;

			foreach(KPFormCustomization kpfc in m_trl.Forms)
			{
				if(kpfc.FullName.EndsWith(strFormName))
				{
					UpdatePreviewForm(kpfc);
					break;
				}
			}

			Activate(); // The preview form sometimes steals the focus
		}

		private void UpdatePreviewForm(KPFormCustomization kpfc)
		{
			if(m_prev == null) { Debug.Assert(false); return; }

			List<TabControl> lTabControls = new List<TabControl>();

			m_prev.SuspendLayout();

			m_prev.CopyForm(kpfc.FormEnglish, delegate(Control c)
			{
				TabControl tc = (c as TabControl);
				if(tc != null) lTabControls.Add(tc);
			});
			kpfc.ApplyTo(m_prev);

			string strName = ((m_kpccLast != null) ? m_kpccLast.Name : null);
			m_prev.EnsureControlPageVisible(strName);

			m_prev.ResumeLayout(true);

			foreach(TabControl tc in lTabControls)
			{
				tc.SelectedIndexChanged += delegate(object sender, EventArgs e)
				{
					m_prev.ShowAccelerators();
				};
			}
			m_prev.ShowAccelerators();
		}

		private void OnBtnClearUnusedText(object sender, EventArgs e)
		{
			m_rtbUnusedText.Text = string.Empty;
		}

		private void OnImport1xLng(object sender, EventArgs e)
		{
			PerformImport("lng", "KeePass 1.x LNG File", Import1xLng);
		}

		private static void Import1xLng(KPTranslation trlInto, IOConnectionInfo ioc)
		{
			TrlImport.Import1xLng(trlInto, ioc.Path);
		}

		private void PerformImport(string strFileExt, string strFileDesc, ImportFn f)
		{
			OpenFileDialogEx ofd = UIUtil.CreateOpenFileDialog("Import " + strFileDesc,
				strFileDesc + " (*." + strFileExt + ")|*." + strFileExt +
				"|All Files (*.*)|*.*", 1, strFileExt, false,
				AppDefs.FileDialogContext.Import);

			if(ofd.ShowDialog() != DialogResult.OK) return;

			try { f(m_trl, IOConnectionInfo.FromPath(ofd.FileName)); }
			catch(Exception ex)
			{
				MessageBox.Show(this, ex.Message, TuDefs.ProductName,
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}

			UpdateStringTableUI();
			UpdateControlTree();
			UpdatePreviewForm();
		}

		private void PerformQuickFind()
		{
			string str = m_tbFind.Text;
			if(string.IsNullOrEmpty(str)) return;

			bool bResult = true;
			if(m_tabMain.SelectedTab == m_tabStrings)
				bResult = PerformQuickFindStrings(str);
			else if(m_tabMain.SelectedTab == m_tabDialogs)
				bResult = PerformQuickFindDialogs(str);

			if(!bResult) m_tbFind.BackColor = AppDefs.ColorEditError;
		}

		private bool PerformQuickFindStrings(string strFind)
		{
			int nItems = m_lvStrings.Items.Count;
			if(nItems == 0) return false;

			ListViewItem lviStart = m_lvStrings.FocusedItem;
			int iOffset = ((lviStart != null) ? (lviStart.Index + 1) : 0);

			for(int i = 0; i < nItems; ++i)
			{
				int j = ((iOffset + i) % nItems);
				ListViewItem lvi = m_lvStrings.Items[j];
				foreach(ListViewItem.ListViewSubItem lvsi in lvi.SubItems)
				{
					if(lvsi.Text.IndexOf(strFind, StrUtil.CaseIgnoreCmp) >= 0)
					{
						UIUtil.SetFocusedItem(m_lvStrings, lvi, false);
						m_lvStrings.SelectedItems.Clear();
						lvi.Selected = true;

						m_lvStrings.EnsureVisible(j);
						return true;
					}
				}
			}

			return false;
		}

		private bool PerformQuickFindDialogs(string strFind)
		{
			List<TreeNode> vNodes = new List<TreeNode>();
			List<string> vValues = new List<string>();
			GetControlTreeItems(m_tvControls.Nodes, vNodes, vValues);

			int iOffset = vNodes.IndexOf(m_tvControls.SelectedNode) + 1;

			for(int i = 0; i < vNodes.Count; ++i)
			{
				int j = ((iOffset + i) % vNodes.Count);

				if(vValues[j].IndexOf(strFind, StrUtil.CaseIgnoreCmp) >= 0)
				{
					m_tvControls.SelectedNode = vNodes[j];
					return true;
				}
			}

			return false;
		}

		private void GetControlTreeItems(TreeNodeCollection tnBase,
			List<TreeNode> vNodes, List<string> vValues)
		{
			foreach(TreeNode tn in tnBase)
			{
				KPFormCustomization kpfc = (tn.Tag as KPFormCustomization);
				KPControlCustomization kpcc = (tn.Tag as KPControlCustomization);

				vNodes.Add(tn);
				if(kpfc != null)
					vValues.Add(kpfc.Window.Name + "©" + kpfc.Window.TextEnglish +
						"©" + kpfc.Window.Text);
				else if(kpcc != null)
					vValues.Add(kpcc.Name + "©" + kpcc.TextEnglish + "©" + kpcc.Text);
				else vValues.Add(tn.Text);

				GetControlTreeItems(tn.Nodes, vNodes, vValues);
			}
		}

		private void OnFindKeyDown(object sender, KeyEventArgs e)
		{
			if(e.KeyCode == Keys.Return) // Return == Enter
			{
				UIUtil.SetHandled(e, true);
				PerformQuickFind();
			}
		}

		private void OnFindKeyUp(object sender, KeyEventArgs e)
		{
			if(e.KeyCode == Keys.Return) // Return == Enter
				UIUtil.SetHandled(e, true);
		}

		private void OnFindTextChanged(object sender, EventArgs e)
		{
			m_tbFind.ResetBackColor();
		}

		private void OnTabMainSelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateUIState();

			if(m_uBlockTabAuto != 0) return;

			if(m_tabMain.SelectedTab == m_tabValidation)
				PerformValidation(null);
		}

		private void OnImport2xNoChecks(object sender, EventArgs e)
		{
			FormTrlMgr.IgnoreBaseHash = true;
			OnFileOpen(sender, EventArgs.Empty);
			FormTrlMgr.IgnoreBaseHash = false;
		}

		private void OnImportPo(object sender, EventArgs e)
		{
			PerformImport("po", "PO File", ImportPo);
		}

		private static void ImportPo(KPTranslation trlInto, IOConnectionInfo ioc)
		{
			TrlImport.ImportPo(trlInto, ioc.Path);
		}

		private void OnEditNextUntrl(object sender, EventArgs e)
		{
			if(m_tabMain.SelectedTab == m_tabStrings)
			{
				int nItems = m_lvStrings.Items.Count;
				if(nItems == 0) { Debug.Assert(false); return; }

				ListViewItem lviStart = m_lvStrings.FocusedItem;
				int iOffset = ((lviStart != null) ? (lviStart.Index + 1) : 0);

				for(int i = 0; i < nItems; ++i)
				{
					int j = ((iOffset + i) % nItems);
					ListViewItem lvi = m_lvStrings.Items[j];
					KPStringTableItem kpstItem = (lvi.Tag as KPStringTableItem);
					if(kpstItem == null) { Debug.Assert(false); continue; }

					if(string.IsNullOrEmpty(kpstItem.Value) &&
						!string.IsNullOrEmpty(kpstItem.ValueEnglish))
					{
						m_lvStrings.EnsureVisible(j);
						UIUtil.SetFocusedItem(m_lvStrings, lvi, true);
						UIUtil.SetFocus(m_tbStrTrl, this);
						return;
					}
				}
			}
			else if(m_tabMain.SelectedTab == m_tabDialogs)
			{
				List<TreeNode> vNodes = new List<TreeNode>();
				List<string> vValues = new List<string>();
				GetControlTreeItems(m_tvControls.Nodes, vNodes, vValues);

				int iOffset = vNodes.IndexOf(m_tvControls.SelectedNode) + 1;

				for(int i = 0; i < vNodes.Count; ++i)
				{
					int j = ((iOffset + i) % vNodes.Count);
					TreeNode tn = vNodes[j];

					string strEng = null, strText = null;
					KPControlCustomization kpcc = (tn.Tag as KPControlCustomization);
					if(kpcc != null)
					{
						strEng = kpcc.TextEnglish;
						strText = kpcc.Text;
					}

					if(string.IsNullOrEmpty(strEng) || (Array.IndexOf<string>(
						m_vEmpty, strEng) >= 0))
						strText = "Dummy";

					if(string.IsNullOrEmpty(strText))
					{
						m_tvControls.SelectedNode = tn;
						UIUtil.SetFocus(m_tbCtrlTrlText, this);
						return;
					}
				}
			}
			else { Debug.Assert(false); return; } // Unsupported tab

			// MessageService.ShowInfo("No untranslated strings found on the current tab page.");
			MessageBox.Show(this, "No untranslated strings found on the current tab page.",
				TuDefs.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void ValidateTranslation(List<string> lErrors)
		{
			/* string strCode = m_trl.Properties.Iso6391Code;
			if(!string.IsNullOrEmpty(strCode))
			{
				try
				{
					CultureInfo ci = CultureInfo.CreateSpecificCulture(strCode);
					if(ci == null) throw new Exception("Culture is unknown.");
				}
				catch(Exception ex)
				{
					string strInfo = string.Empty;
					try
					{
						StringBuilder sb = new StringBuilder();
						sb.AppendLine("On the current system, the following cultures are known:");

						List<CultureInfo> l = new List<CultureInfo>(
							CultureInfo.GetCultures(CultureTypes.AllCultures));
						l.Sort(delegate(CultureInfo x, CultureInfo y)
						{
							return string.Compare(x.EnglishName, y.EnglishName, StrUtil.CaseIgnoreCmp);
						});

						foreach(CultureInfo ci in l)
							sb.AppendLine(ci.Name + "\t- " + ci.EnglishName);

						strInfo = MessageService.NewParagraph + sb.ToString();
					}
					catch(Exception) { Debug.Assert(false); }

					lErrors.Add("Error when trying to parse \"" + strCode +
						"\" as ISO 639-1 language code:" + MessageService.NewLine +
						ex.Message.Trim() + strInfo);
				}
			} */

			string[] vCaseSensWords = new string[] { PwDefs.ShortProductName };

			bool bRtl = m_trl.Properties.RightToLeft;
			string np = MessageService.NewParagraph;

			foreach(KPStringTable kpst in m_trl.StringTables)
			{
				foreach(KPStringTableItem kpi in kpst.Strings)
				{
					string strEn = kpi.ValueEnglish;
					string strTrl = kpi.Value;
					if(string.IsNullOrEmpty(strEn) || string.IsNullOrEmpty(strTrl)) continue;

					string strLoc = np + "(string table item '" + kpst.Name +
						"." + kpi.Name + "').";

					// Check case-sensitive words
					foreach(string strWord in vCaseSensWords)
					{
						bool bWordEn = (strEn.IndexOf(strWord) >= 0);
						if(!bWordEn)
						{
							Debug.Assert(strEn.IndexOf(strWord, StrUtil.CaseIgnoreCmp) < 0);
						}
						if(bWordEn && (strTrl.IndexOf(strWord) < 0))
							lErrors.Add("The English string" + np + "\"" + strEn + "\"" + np +
								"contains the case-sensitive word '" + strWord +
								"', but the translated string does not:" + np +
								"\"" + strTrl + "\"" + strLoc);
					}

					// Check 3 dots
					bool bEllEn = (strEn.EndsWith("...") || strEn.EndsWith(@"…"));
					bool bEllTrl = (strTrl.EndsWith("...") || strTrl.EndsWith(@"…"));
					if(bEllEn && !bEllTrl && !bRtl) // Check doesn't support RTL
						lErrors.Add("The English string" + np + "\"" + strEn + "\"" + np +
							"ends with 3 dots, but the translated string does not:" +
							np + "\"" + strTrl + "\"" + strLoc);
				}
			}
		}

		private void PerformValidation(string strFileSaved)
		{
			List<string> lErrors = new List<string>();
			try
			{
				UpdateTranslationObject();

				ValidateTranslation(lErrors);
				AccelKeysCheck.Validate(m_trl, lErrors);

				uint uUntrlStr = 0;
				foreach(KPStringTable kpst in m_trl.StringTables)
				{
					foreach(KPStringTableItem kpi in kpst.Strings)
					{
						if(string.IsNullOrEmpty(kpi.Value) &&
							!string.IsNullOrEmpty(kpi.ValueEnglish))
							++uUntrlStr;
					}
				}
				if(uUntrlStr != 0)
					lErrors.Add("There are " + uUntrlStr.ToString() +
						" untranslated strings on the 'String Tables' tab.");

				uint uUntrlCtrl = 0;
				Action<KPControlCustomization> fCheckCtrl = delegate(
					KPControlCustomization kpcc)
				{
					if(string.IsNullOrEmpty(kpcc.TextEnglish) ||
						(Array.IndexOf<string>(m_vEmpty, kpcc.TextEnglish) >= 0))
						return;
					if(string.IsNullOrEmpty(kpcc.Text)) ++uUntrlCtrl;
				};
				foreach(KPFormCustomization kpfc in m_trl.Forms)
				{
					fCheckCtrl(kpfc.Window);
					foreach(KPControlCustomization kpcc in kpfc.Controls)
						fCheckCtrl(kpcc);
				}
				if(uUntrlCtrl != 0)
					lErrors.Add("There are " + uUntrlCtrl.ToString() +
						" untranslated texts on the 'Dialogs' tab.");

				if(!string.IsNullOrEmpty(m_trl.UnusedText))
					lErrors.Add("It is recommended to clear the 'Unused Text' tab.");
			}
			catch(Exception ex) { Debug.Assert(false); lErrors.Add(ex.Message); }

			RichTextBuilder rb = new RichTextBuilder();

			if(!string.IsNullOrEmpty(strFileSaved))
			{
				rb.AppendLine("File has been saved to:", FontStyle.Bold);
				rb.AppendLine(strFileSaved);
				rb.AppendLine();
			}

			rb.AppendLine("TrlUtil has detected " + lErrors.Count.ToString() +
				" problem(s).", FontStyle.Bold);

			string strSep = new string('=', 72);

			foreach(string strError in lErrors)
			{
				string str = (strError ?? string.Empty).Trim();
				if(str.Length == 0) { Debug.Assert(false); str = "<Unknown>."; }

				rb.AppendLine();
				rb.AppendLine(strSep);
				rb.AppendLine();
				rb.AppendLine(str);
			}

			rb.Build(m_rtbValidation);

			if(!string.IsNullOrEmpty(strFileSaved))
				UIUtil.RtfLinkifyText(m_rtbValidation, strFileSaved, false);

			foreach(KPStringTable kpst in m_trl.StringTables)
			{
				foreach(KPStringTableItem kpi in kpst.Strings)
					UIUtil.RtfLinkifyText(m_rtbValidation, kpst.Name + "." +
						kpi.Name, false, true);
			}

			foreach(string str in m_dControls.Keys)
				UIUtil.RtfLinkifyText(m_rtbValidation, str, false, true);

			UIUtil.RtfLinkifyText(m_rtbValidation, m_tabUnusedText.Text, false, true);

			m_rtbValidation.Select(0, 0);

			if(m_tabMain.SelectedTab != m_tabValidation)
			{
				++m_uBlockTabAuto;
				m_tabMain.SelectedTab = m_tabValidation;
				--m_uBlockTabAuto;
			}
		}

		private void OnValidationLinkClicked(object sender, LinkClickedEventArgs e)
		{
			try
			{
				string str = e.LinkText;
				if(string.IsNullOrEmpty(str)) { Debug.Assert(false); return; }

				if(str == m_strFile)
				{
					string strDir = UrlUtil.GetFileDirectory(str, true, true);
					if(!NativeLib.IsUnix() || (strDir.IndexOfAny(new char[] {
						'\\', '\"', '\'' }) < 0)) // Open safe paths only
					{
						ProcessStartInfo psi = new ProcessStartInfo();
						psi.FileName = strDir;
						psi.UseShellExecute = true;

						Process p = Process.Start(psi);
						if(p != null) p.Dispose();
					}
					return;
				}

				ListViewItem lvi;
				if(m_dStrings.TryGetValue(str, out lvi))
				{
					UIUtil.SetFocusedItem(m_lvStrings, lvi, true);
					lvi.EnsureVisible();
					m_tabMain.SelectedTab = m_tabStrings;
					UIUtil.SetFocus(m_lvStrings, this);
					return;
				}

				TreeNode tn;
				if(m_dControls.TryGetValue(str, out tn))
				{
					m_tvControls.SelectedNode = tn;

					if(tn.Parent != null) tn.Parent.EnsureVisible();
					tn.EnsureVisible();

					m_tabMain.SelectedTab = m_tabDialogs;
					UIUtil.SetFocus(m_tvControls, this);
					return;
				}

				if(str == m_tabUnusedText.Text)
				{
					m_tabMain.SelectedTab = m_tabUnusedText;
					UIUtil.SetFocus(m_rtbUnusedText, this);
					return;
				}

				Debug.Assert(false);
			}
			catch(Exception) { Debug.Assert(false); }
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			if(m_prev != null)
			{
				m_prev.Close();
				m_prev.Dispose();
				m_prev = null;
			}
			else { Debug.Assert(false); }
		}

		private bool CanMoveUnusedToDialog()
		{
			TextBox tb = m_tbCtrlTrlText;
			if(!tb.Enabled || tb.ReadOnly || (tb.TextLength != 0)) return false;

			RichTextBox rtb = m_rtbUnusedText;
			if(!rtb.Enabled || rtb.ReadOnly) return false;

			string str = (rtb.SelectedText ?? string.Empty).Trim();
			if(str.Length == 0) return false;
			if(str.IndexOfAny(g_vNewLine) >= 0) return false;

			return true;
		}

		private void OnEditMoveUnusedToDialog(object sender, EventArgs e)
		{
			if(!CanMoveUnusedToDialog()) { Debug.Assert(false); return; }

			TextBox tb = m_tbCtrlTrlText;
			RichTextBox rtb = m_rtbUnusedText;

			tb.Text = (rtb.SelectedText ?? string.Empty).Trim();
			rtb.SelectedText = string.Empty;

			m_tabMain.SelectedTab = m_tabDialogs;
			UIUtil.SetFocus(tb, this);
		}

		private void OnUnusedTextSelectionChanged(object sender, EventArgs e)
		{
			UpdateUIState();
		}
	}
}
