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
using System.Text.RegularExpressions;
using System.Windows.Forms;

using KeePass.App;
using KeePass.DataExchange;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class AboutForm : Form, IGwmWindow
	{
		private CustomContextMenuStripEx m_ctxComponents = null;

		public bool CanCloseWithoutDataLoss { get { return true; } }

		public AboutForm()
		{
			InitializeComponent();
			GlobalWindowManager.InitializeForm(this);
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			GlobalWindowManager.AddWindow(this, this);

			string strVersion = GetMainVersion();

			string strTitle = PwDefs.ProductName;
			string strDesc = KPRes.Version + " " + strVersion;

			Icon icoSc = AppIcons.Get(AppIconType.Main, new Size(
				DpiUtil.ScaleIntX(48), DpiUtil.ScaleIntY(48)), Color.Empty);
			BannerFactory.CreateBannerEx(this, m_bannerImage, icoSc.ToBitmap(),
				strTitle, strDesc);
			this.Icon = AppIcons.Default;

			Debug.Assert(!m_lblCopyright.AutoSize); // For RTL support
			m_lblCopyright.Text = PwDefs.Copyright + ".";

			try { BuildComponentsList(strVersion); }
			catch(Exception) { Debug.Assert(false); }

			UIUtil.SetExplorerTheme(m_lvComponents, false);
			UIUtil.ResizeColumns(m_lvComponents, true);
			BuildComponentsContextMenu();

			UIUtil.SetFocus(m_btnOK, this);
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			m_lvComponents.ContextMenuStrip = null;
			if(m_ctxComponents != null)
			{
				m_ctxComponents.Dispose();
				m_ctxComponents = null;
			}

			GlobalWindowManager.RemoveWindow(this);
		}

		private static string GetMainVersion()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(PwDefs.VersionString);

			if(Program.IsDevelopmentSnapshot())
			{
				sb.Append(" - Dev.");

				try
				{
					FileInfo fi = new FileInfo(WinUtil.GetExecutable());
					string strDate = fi.LastWriteTimeUtc.ToString("yyMMdd");

					sb.Append(' ');
					sb.Append(strDate);
				}
				catch(Exception) { Debug.Assert(false); }
			}

			const string strParamPlh = @"{PARAM}";
			string strBits = KPRes.BitsA;
			if(strBits.IndexOf(strParamPlh) >= 0)
			{
				sb.Append(" (");
				sb.Append(strBits.Replace(strParamPlh, (IntPtr.Size * 8).ToString()));
				sb.Append(')');
			}
			else { Debug.Assert(false); }

			return sb.ToString();
		}

		private void BuildComponentsList(string strMainVersion)
		{
			string strValueColumn = KPRes.Version + "/" + KPRes.Status;
			if(Regex.IsMatch(strValueColumn, "\\s"))
				strValueColumn = KPRes.Version + " / " + KPRes.Status;

			m_lvComponents.Columns.Add(KPRes.Component, 100);
			m_lvComponents.Columns.Add(strValueColumn, 100);

			string strExe = WinUtil.GetExecutable();
			string strDir = UrlUtil.GetFileDirectory(strExe, true, false);

			AddComponentItem(PwDefs.ShortProductName, strMainVersion, strExe);

			string strXsl = UrlUtil.EnsureTerminatingSeparator(strDir +
				AppDefs.XslFilesDir, false);
			bool b = File.Exists(strXsl + AppDefs.XslFileHtmlFull);
			b &= File.Exists(strXsl + AppDefs.XslFileHtmlLight);
			b &= File.Exists(strXsl + AppDefs.XslFileHtmlTabular);
			AddComponentItem(KPRes.XslStylesheetsKdbx, (b ? KPRes.Installed :
				KPRes.NotInstalled), (b ? strXsl : null));

			b = KdbFile.IsLibraryInstalled();
			string strVer = (b ? (KdbManager.KeePassVersionString + " - 0x" +
				KdbManager.LibraryBuild.ToString("X4")) : KPRes.NotInstalled);
			string strPath = null;
			if(b)
			{
				string str = strDir + ((IntPtr.Size == 4) ? KdbManager.DllFile32 :
					KdbManager.DllFile64);
				if(File.Exists(str)) strPath = str;
				else { Debug.Assert(false); } // Somewhere else?
			}
			AddComponentItem(KPRes.KeePassLibCLong, strVer, strPath);
		}

		private void AddComponentItem(string strName, string strVersion, string strPath)
		{
			if(string.IsNullOrEmpty(strName)) { Debug.Assert(false); return; }
			if(strVersion == null) { Debug.Assert(false); strVersion = string.Empty; }

			ListViewItem lvi = new ListViewItem(strName);
			lvi.SubItems.Add(strVersion);

			if(!string.IsNullOrEmpty(strPath))
			{
				lvi.ToolTipText = strName + MessageService.NewLine + strPath;
				lvi.Tag = strPath;
			}

			m_lvComponents.Items.Add(lvi);
		}

		private void BuildComponentsContextMenu()
		{
			m_ctxComponents = new CustomContextMenuStripEx();
			m_ctxComponents.SuspendLayout();

			ToolStripMenuItem tsmiShow = new ToolStripMenuItem(KPRes.ShowWithFileManager,
				Properties.Resources.B16x16_Folder_Yellow_Open);
			tsmiShow.Click += this.OnComponentShow;
			m_ctxComponents.Items.Add(tsmiShow);

			m_ctxComponents.Items.Add(new ToolStripSeparator());

			ToolStripMenuItem tsmiCopyVersion = new ToolStripMenuItem(
				KPRes.CopyObject.Replace(@"{PARAM}", m_lvComponents.Columns[1].Text),
				Properties.Resources.B16x16_EditCopy);
			tsmiCopyVersion.Click += this.OnComponentCopyVersion;
			m_ctxComponents.Items.Add(tsmiCopyVersion);

			ToolStripMenuItem tsmiCopyPath = new ToolStripMenuItem(
				KPRes.CopyObject.Replace(@"{PARAM}", KPRes.Path),
				Properties.Resources.B16x16_EditCopyLink);
			tsmiCopyPath.Click += this.OnComponentCopyTag;
			m_ctxComponents.Items.Add(tsmiCopyPath);

			m_ctxComponents.Opening += delegate(object sender, CancelEventArgs e)
			{
				ListViewItem lviSel = GetSelectedComponent();
				bool bSel = (lviSel != null);
				bool bTag = (bSel && (lviSel.Tag != null));

				tsmiShow.Enabled = bTag;
				tsmiCopyVersion.Enabled = bSel;
				tsmiCopyPath.Enabled = bTag;
			};

			m_ctxComponents.ResumeLayout(true);
			m_lvComponents.ContextMenuStrip = m_ctxComponents;
		}

		private void OnLinkHomepage(object sender, LinkLabelLinkClickedEventArgs e)
		{
			WinUtil.OpenUrl(PwDefs.HomepageUrl, null);
			Close();
		}

		private void OnLinkHelpFile(object sender, LinkLabelLinkClickedEventArgs e)
		{
			AppHelp.ShowHelp(null, null);
			Close();
		}

		private void OnLinkLicenseFile(object sender, LinkLabelLinkClickedEventArgs e)
		{
			AppHelp.ShowHelp(AppDefs.HelpTopics.License, null, true);
			Close();
		}

		private void OnLinkAcknowledgements(object sender, LinkLabelLinkClickedEventArgs e)
		{
			AppHelp.ShowHelp(AppDefs.HelpTopics.Acknowledgements, null, true);
			Close();
		}

		private void OnLinkDonate(object sender, LinkLabelLinkClickedEventArgs e)
		{
			WinUtil.OpenUrl(PwDefs.DonationsUrl, null);
			Close();
		}

		private ListViewItem GetSelectedComponent()
		{
			ListView.SelectedListViewItemCollection lvsic = m_lvComponents.SelectedItems;
			if((lvsic == null) || (lvsic.Count != 1)) return null;
			return lvsic[0];
		}

		private void OnComponentShow(object sender, EventArgs e)
		{
			ListViewItem lvi = GetSelectedComponent();
			if(lvi == null) { Debug.Assert(false); return; }

			string strPath = (lvi.Tag as string);
			if(string.IsNullOrEmpty(strPath)) return;

			if(File.Exists(strPath))
				WinUtil.ShowFileInFileManager(strPath, true);
			else WinUtil.OpenUrlDirectly(strPath);
		}

		private void OnComponentCopyVersion(object sender, EventArgs e)
		{
			ListViewItem lvi = GetSelectedComponent();
			if(lvi == null) { Debug.Assert(false); return; }

			string str = (lvi.SubItems[1].Text ?? string.Empty);
			ClipboardUtil.Copy(str, false, false, null, null, this.Handle);
		}

		private void OnComponentCopyTag(object sender, EventArgs e)
		{
			ListViewItem lvi = GetSelectedComponent();
			if(lvi == null) { Debug.Assert(false); return; }

			string str = ((lvi.Tag as string) ?? string.Empty);
			ClipboardUtil.Copy(str, false, false, null, null, this.Handle);
		}
	}
}
