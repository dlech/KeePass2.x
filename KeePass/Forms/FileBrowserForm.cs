/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2025 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Text;
using System.Windows.Forms;

using KeePass.App;
using KeePass.Native;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class FileBrowserForm : Form
	{
		private bool m_bSaveMode = false;
		private string m_strTitle = PwDefs.ShortProductName;
		private string m_strHint = string.Empty;
		private string m_strContext = null;

		private readonly List<Image> m_lFolderImages = new List<Image>();
		private ImageList m_ilFolders = null;
		private readonly List<Image> m_lFileImages = new List<Image>();
		private ImageList m_ilFiles = null;

		private int m_nIconDim = DpiUtil.ScaleIntY(16);
		private uint m_uBlockNameChangeAuto = 0;

		private const string StrDummyNode = "66913D76EA3F4F2A8B1A0899B7322EC3";

		private string m_strSuggestedFile = string.Empty;
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DefaultValue("")]
		public string SuggestedFile
		{
			get { return m_strSuggestedFile; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strSuggestedFile = value;
			}
		}

		private string m_strSelectedFile = null;
		public string SelectedFile
		{
			get { return m_strSelectedFile; }
		}

		public void InitEx(bool bSaveMode, string strTitle, string strHint,
			string strContext)
		{
			m_bSaveMode = bSaveMode;
			m_strTitle = (strTitle ?? string.Empty);
			m_strHint = (strHint ?? string.Empty);
			m_strContext = strContext;
		}

		public FileBrowserForm()
		{
			InitializeComponent();
			GlobalWindowManager.InitializeForm(this);
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			GlobalWindowManager.AddWindow(this);

			this.Icon = AppIcons.Default;
			this.Text = m_strTitle;

			Debug.Assert(m_nIconDim == m_tvFolders.ItemHeight);
			m_nIconDim = m_tvFolders.ItemHeight;

			if(UIUtil.VistaStyleListsSupported)
			{
				UIUtil.SetExplorerTheme(m_tvFolders, true);
				UIUtil.SetExplorerTheme(m_lvFiles, true);
			}

			Debug.Assert(!m_lblHint.AutoSize); // For RTL support
			m_lblHint.Text = m_strHint;

			if(UIUtil.ColorsEqual(m_lblHint.ForeColor, Color.Black))
				m_lblHint.ForeColor = Color.FromArgb(96, 96, 96);

			int nWidth = m_lvFiles.ClientSize.Width - UIUtil.GetVScrollBarWidth();
			m_lvFiles.Columns.Add(KPRes.Name, (nWidth * 3) / 4);
			m_lvFiles.Columns.Add(KPRes.Size, nWidth / 4, HorizontalAlignment.Right);

			InitialPopulateFolders();

			string strWorkDir = Program.Config.Application.GetWorkingDirectory(m_strContext);
			if(string.IsNullOrEmpty(strWorkDir))
				strWorkDir = WinUtil.GetHomeDirectory();

			string strSugg = m_strSuggestedFile;
			if(!string.IsNullOrEmpty(strSugg))
			{
				if(UrlUtil.IsAbsolutePath(strSugg))
					strWorkDir = UrlUtil.GetFileDirectory(strSugg, false, true);

				++m_uBlockNameChangeAuto;
				m_tbFileName.Text = UrlUtil.GetFileName(strSugg);
				--m_uBlockNameChangeAuto;
			}

			BrowseToFolder(strWorkDir);

			EnableControlsEx();
			UIUtil.SetFocus(m_tbFileName, this);
			m_tbFileName.SelectAll();
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			m_tvFolders.Nodes.Clear();
			m_lvFiles.Items.Clear();
			m_tvFolders.ImageList = null;
			m_lvFiles.SmallImageList = null;

			if(m_ilFolders != null) { m_ilFolders.Dispose(); m_ilFolders = null; }
			if(m_ilFiles != null) { m_ilFiles.Dispose(); m_ilFiles = null; }

			foreach(Image img in m_lFolderImages) img.Dispose();
			m_lFolderImages.Clear();
			foreach(Image img in m_lFileImages) img.Dispose();
			m_lFileImages.Clear();

			GlobalWindowManager.RemoveWindow(this);
		}

		private void EnableControlsEx()
		{
			bool bOK = !string.IsNullOrEmpty(m_tbFileName.Text);

			try
			{
				string strFile = GetSelectedFile();
				if(!string.IsNullOrEmpty(strFile) && Directory.Exists(strFile))
				{
					bOK = true;
					m_btnOK.Text = KPRes.OpenCmd;
				}
				else m_btnOK.Text = (m_bSaveMode ? KPRes.SaveCmd : KPRes.OpenCmd);
			}
			catch(Exception) { Debug.Assert(false); }

			m_btnOK.Enabled = bOK;
		}

		private void InitialPopulateFolders()
		{
			List<TreeNode> l = new List<TreeNode>();

			string str = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
			if(!string.IsNullOrEmpty(str))
			{
				TreeNode tn = CreateFolderNode(str, false, null);
				if(tn != null) l.Add(tn);
			}

			str = Environment.GetEnvironmentVariable("USERPROFILE");
			if(!string.IsNullOrEmpty(str))
			{
				TreeNode tn = CreateFolderNode(str, false, null);
				if(tn != null) l.Add(tn);
			}

			DriveInfo[] vDrives = DriveInfo.GetDrives();
			foreach(DriveInfo drv in vDrives)
			{
				try
				{
					DirectoryInfo di = drv.RootDirectory;
					TreeNode tn = CreateFolderNode(di.FullName, true, drv);
					if(tn != null) l.Add(tn);
				}
				catch(Exception) { Debug.Assert(false); }
			}

			RebuildFolderImageList();
			m_tvFolders.Nodes.AddRange(l.ToArray());
		}

		private void GetObjectProps(string strPath, DriveInfo drvHint,
			out Image img, ref string strDisplayName)
		{
			GetObjectPropsUnscaled(strPath, drvHint, out img, ref strDisplayName);

			if(img != null)
			{
				if((img.Width != m_nIconDim) || (img.Height != m_nIconDim))
				{
					Image imgScaled = GfxUtil.ScaleImage(img, m_nIconDim,
						m_nIconDim, ScaleTransformFlags.UIIcon);
					img.Dispose(); // Dispose unscaled version
					img = imgScaled;
				}
			}
		}

		private void GetObjectPropsUnscaled(string strPath, DriveInfo drvHint,
			out Image img, ref string strDisplayName)
		{
			try
			{
				string strName;
				NativeMethods.SHGetFileInfo(strPath, m_nIconDim, m_nIconDim,
					out img, out strName);

				if(!string.IsNullOrEmpty(strName) && (strName.IndexOf(
					Path.DirectorySeparatorChar) < 0))
					strDisplayName = strName;

				if(img != null) return;
			}
			catch(Exception) { Debug.Assert(false); }

			ImageList.ImageCollection icons = Program.MainForm.ClientIcons.Images;

			if((strPath.Length <= 3) && (drvHint != null))
			{
				switch(drvHint.DriveType)
				{
					case DriveType.Fixed:
						img = new Bitmap(icons[(int)PwIcon.Drive]);
						break;
					case DriveType.CDRom:
						img = new Bitmap(icons[(int)PwIcon.CDRom]);
						break;
					case DriveType.Network:
						img = new Bitmap(icons[(int)PwIcon.NetworkServer]);
						break;
					case DriveType.Ram:
						img = new Bitmap(icons[(int)PwIcon.Memory]);
						break;
					case DriveType.Removable:
						img = new Bitmap(icons[(int)PwIcon.Disk]);
						break;
					default:
						img = new Bitmap(icons[(int)PwIcon.Folder]);
						break;
				}

				return;
			}

			img = UIUtil.GetFileIcon(strPath, m_nIconDim, m_nIconDim);
			if(img != null) return;

			if(Directory.Exists(strPath))
				img = new Bitmap(icons[(int)PwIcon.Folder]);
			else if(File.Exists(strPath))
				img = new Bitmap(icons[(int)PwIcon.PaperNew]);
			else
			{
				Debug.Assert(false);
				img = new Bitmap(icons[(int)PwIcon.Star]);
			}
		}

		private TreeNode CreateFolderNode(string strDir, bool bForcePlusMinus,
			DriveInfo drvHint)
		{
			try
			{
				DirectoryInfo di = new DirectoryInfo(strDir);

				Image img;
				string strText = di.Name;
				GetObjectProps(di.FullName, drvHint, out img, ref strText);

				int iImage = m_lFolderImages.Count;
				m_lFolderImages.Add(img);

				TreeNode tn = new TreeNode(strText, iImage, iImage);
				tn.Tag = di.FullName;

				InitNodePlusMinus(tn, di, bForcePlusMinus);
				return tn;
			}
			catch(Exception) { Debug.Assert(false); }

			return null;
		}

		private static void InitNodePlusMinus(TreeNode tn, DirectoryInfo di,
			bool bForce)
		{
			if(!bForce)
			{
				try
				{
					DirectoryInfo[] vDirs = di.GetDirectories();
					bool bFoundDir = false;
					foreach(DirectoryInfo diSub in vDirs)
					{
						if(!IsValidFileSystemObject(diSub)) continue;

						bFoundDir = true;
						break;
					}

					if(!bFoundDir) return;
				}
				catch(Exception) { return; } // Usually unauthorized
			}

			tn.Nodes.Add(StrDummyNode);
			tn.Collapse();
		}

		private void RebuildFolderImageList()
		{
			ImageList il = UIUtil.BuildImageListUnscaled(m_lFolderImages,
				m_nIconDim, m_nIconDim);
			m_tvFolders.ImageList = il;

			if(m_ilFolders != null) m_ilFolders.Dispose();
			m_ilFolders = il;
		}

		private void BuildFilesList(DirectoryInfo di)
		{
			m_lvFiles.BeginUpdate();
			m_lvFiles.Items.Clear();

			m_lvFiles.SmallImageList = null;
			if(m_ilFiles != null) { m_ilFiles.Dispose(); m_ilFiles = null; }
			foreach(Image img in m_lFileImages) img.Dispose();
			m_lFileImages.Clear();

			DirectoryInfo[] vDirs;
			FileInfo[] vFiles;
			try
			{
				vDirs = di.GetDirectories();
				vFiles = di.GetFiles();
			}
			catch(Exception) { m_lvFiles.EndUpdate(); return; } // Unauthorized

			Comparison<ListViewItem> fCmp = ((x, y) => StrUtil.CompareNaturally(x.Text, y.Text));

			List<ListViewItem> lDirItems = new List<ListViewItem>();
			foreach(DirectoryInfo diSub in vDirs)
				AddFileItem(diSub, m_lFileImages, lDirItems, -1);
			lDirItems.Sort(fCmp);

			List<ListViewItem> lFileItems = new List<ListViewItem>();
			foreach(FileInfo fi in vFiles)
				AddFileItem(fi, m_lFileImages, lFileItems, fi.Length);
			lFileItems.Sort(fCmp);

			m_ilFiles = UIUtil.BuildImageListUnscaled(m_lFileImages, m_nIconDim, m_nIconDim);
			m_lvFiles.SmallImageList = m_ilFiles;

			m_lvFiles.Items.AddRange(lDirItems.ToArray());
			m_lvFiles.Items.AddRange(lFileItems.ToArray());
			m_lvFiles.EndUpdate();

			EnableControlsEx();
		}

		private static bool IsValidFileSystemObject(FileSystemInfo fsi)
		{
			if(fsi == null) { Debug.Assert(false); return false; }

			string strName = fsi.Name;
			if(string.IsNullOrEmpty(strName) || (strName == ".") ||
				(strName == "..")) return false;
			if(strName.EndsWith(".lnk", StrUtil.CaseIgnoreCmp)) return false;
			if(strName.EndsWith(".url", StrUtil.CaseIgnoreCmp)) return false;

			FileAttributes fa = fsi.Attributes;
			if((long)(fa & FileAttributes.ReparsePoint) != 0) return false;
			if(((long)(fa & FileAttributes.System) != 0) &&
				((long)(fa & FileAttributes.Hidden) != 0)) return false;

			return true;
		}

		private void AddFileItem(FileSystemInfo fsi, List<Image> lImages,
			List<ListViewItem> lItems, long lFileLength)
		{
			if(!IsValidFileSystemObject(fsi)) return;

			Image img;
			string strText = fsi.Name;
			GetObjectProps(fsi.FullName, null, out img, ref strText);

			lImages.Add(img);

			ListViewItem lvi = new ListViewItem(strText, lImages.Count - 1);
			lvi.Tag = fsi.FullName;

			lvi.SubItems.Add((lFileLength < 0) ? string.Empty :
				StrUtil.FormatDataSizeKB((ulong)lFileLength));

			lItems.Add(lvi);
		}

		private string GetSelectedDirectory()
		{
			TreeNode tn = m_tvFolders.SelectedNode;
			if(tn == null) return null;

			string str = (tn.Tag as string);
			Debug.Assert(!string.IsNullOrEmpty(str));
			return str;
		}

		private string GetSelectedFile()
		{
			ListView.SelectedListViewItemCollection lvsic = m_lvFiles.SelectedItems;
			if((lvsic == null) || (lvsic.Count != 1)) return null;

			string str = (lvsic[0].Tag as string);
			Debug.Assert(!string.IsNullOrEmpty(str));
			return str;
		}

		private bool TryOKEx()
		{
			try
			{
				string strSelFile = GetSelectedFile();
				if(!string.IsNullOrEmpty(strSelFile) && Directory.Exists(strSelFile))
				{
					TreeNode tn = m_tvFolders.SelectedNode;
					if(tn == null) { Debug.Assert(false); return false; }

					if(!tn.IsExpanded) tn.Expand();

					foreach(TreeNode tnSub in tn.Nodes)
					{
						string strSub = (tnSub.Tag as string);
						if(string.IsNullOrEmpty(strSub)) { Debug.Assert(false); continue; }

						if(strSub.Equals(strSelFile, StrUtil.CaseIgnoreCmp))
						{
							m_tvFolders.SelectedNode = tnSub;
							tnSub.EnsureVisible();
							return false; // Success, but not a file selection!
						}
					}

					Debug.Assert(false);
					return false;
				}

				string strDir = GetSelectedDirectory();
				if(string.IsNullOrEmpty(strDir)) return false;
				string strFile = m_tbFileName.Text;
				if(string.IsNullOrEmpty(strFile)) return false;

				string strPath = Path.Combine(strDir, strFile);
				bool bExists = File.Exists(strPath);

				if(!m_bSaveMode && !bExists)
					throw new FileNotFoundException();

				if(m_bSaveMode && bExists)
				{
					string strNL = MessageService.NewLine;
					if(!MessageService.AskYesNo(KPRes.FileExistsAlready + strNL +
						strPath + strNL + strNL + KPRes.OverwriteExistingFileQuestion))
						return false;
				}

				m_strSelectedFile = strPath;
				Program.Config.Application.SetWorkingDirectory(m_strContext,
					UrlUtil.GetFileDirectory(strPath, false, true)); // May be != strDir
				return true;
			}
			catch(Exception ex) { MessageService.ShowWarning(ex); }

			return false;
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			if(!TryOKEx()) this.DialogResult = DialogResult.None;
		}

		private void OnFilesItemActivate(object sender, EventArgs e)
		{
			m_btnOK.PerformClick();
		}

		private void OnFoldersBeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			TreeNode tn = e.Node;
			if(tn == null) { Debug.Assert(false); e.Cancel = true; return; }

			if((tn.Nodes.Count == 1) && (tn.Nodes[0].Text == StrDummyNode))
			{
				tn.Nodes.Clear();
				List<TreeNode> lNodes = new List<TreeNode>();

				try
				{
					DirectoryInfo di = new DirectoryInfo(tn.Tag as string);
					DirectoryInfo[] vSubDirs = di.GetDirectories();
					foreach(DirectoryInfo diSub in vSubDirs)
					{
						if(!IsValidFileSystemObject(diSub)) continue;

						TreeNode tnSub = CreateFolderNode(diSub.FullName, false, null);
						if(tnSub != null) lNodes.Add(tnSub);
					}
				}
				catch(Exception) { Debug.Assert(false); }

				RebuildFolderImageList();
				lNodes.Sort((x, y) => StrUtil.CompareNaturally(x.Text, y.Text));
				tn.Nodes.AddRange(lNodes.ToArray());
			}
		}

		private void BrowseToFolder(string strPath)
		{
			try
			{
				DirectoryInfo di = new DirectoryInfo(strPath);
				string[] vPath = di.FullName.Split(Path.DirectorySeparatorChar);
				if((vPath == null) || (vPath.Length == 0)) { Debug.Assert(false); return; }

				TreeNode tn = null;
				string str = string.Empty;
				for(int i = 0; i < vPath.Length; ++i)
				{
					if(i != 0) str = UrlUtil.EnsureTerminatingSeparator(str, false);
					str += vPath[i];
					if(i == 0) str = UrlUtil.EnsureTerminatingSeparator(str, false);

					TreeNodeCollection tnc = ((tn != null) ? tn.Nodes : m_tvFolders.Nodes);
					tn = null;

					foreach(TreeNode tnSub in tnc)
					{
						string strSub = (tnSub.Tag as string);
						if(string.IsNullOrEmpty(strSub)) { Debug.Assert(false); continue; }

						if(strSub.Equals(str, StrUtil.CaseIgnoreCmp))
						{
							tn = tnSub;
							break;
						}
					}

					if(tn == null) { Debug.Assert(false); break; }

					if((i != (vPath.Length - 1)) && !tn.IsExpanded) tn.Expand();
				}

				if(tn != null)
				{
					m_tvFolders.SelectedNode = tn;
					tn.EnsureVisible();
				}
				else { Debug.Assert(false); }
			}
			catch(Exception) { Debug.Assert(false); }
		}

		private void OnFoldersAfterSelect(object sender, TreeViewEventArgs e)
		{
			TreeNode tn = e.Node;
			string strPath = ((tn != null) ? (tn.Tag as string) : null);
			if(string.IsNullOrEmpty(strPath)) { Debug.Assert(false); return; }

			try { BuildFilesList(new DirectoryInfo(strPath)); }
			catch(Exception) { Debug.Assert(false); }
		}

		private void OnFilesSelectedIndexChanged(object sender, EventArgs e)
		{
			string strFile = GetSelectedFile();
			if(!string.IsNullOrEmpty(strFile))
			{
				try
				{
					if(!Directory.Exists(strFile))
					{
						++m_uBlockNameChangeAuto;
						m_tbFileName.Text = UrlUtil.GetFileName(strFile);
						--m_uBlockNameChangeAuto;
					}
				}
				catch(Exception) { Debug.Assert(false); }
			}

			EnableControlsEx();
		}

		private void OnFileNameTextChanged(object sender, EventArgs e)
		{
			if(m_uBlockNameChangeAuto != 0) return;

			UIUtil.DeselectAllItems(m_lvFiles);

			EnableControlsEx();
		}
	}
}
