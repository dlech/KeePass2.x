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
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using KeePass.App;
using KeePass.Native;
using KeePass.Resources;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Utility;

namespace KeePass.UI
{
	public static class UIUtil
	{
		public static void RtfSetSelectionLink(RichTextBox richTextBox)
		{
			try
			{
				NativeMethods.CHARFORMAT2 cf = new NativeMethods.CHARFORMAT2();
				cf.cbSize = (UInt32)Marshal.SizeOf(cf);

				cf.dwMask = NativeMethods.CFM_LINK;
				cf.dwEffects = NativeMethods.CFE_LINK;

				IntPtr wParam = (IntPtr)NativeMethods.SCF_SELECTION;
				IntPtr lParam = Marshal.AllocCoTaskMem(Marshal.SizeOf(cf));
				Marshal.StructureToPtr(cf, lParam, false);

				NativeMethods.SendMessage(richTextBox.Handle,
					NativeMethods.EM_SETCHARFORMAT, wParam, lParam);

				Marshal.FreeCoTaskMem(lParam);
			}
			catch(Exception) { Debug.Assert(false); }
		}

		public static Image CreateColorBitmap24(int nWidth, int nHeight, Color color)
		{
			Bitmap bmp = new Bitmap(nWidth, nHeight, PixelFormat.Format24bppRgb);

			using(SolidBrush sb = new SolidBrush(color))
			{
				using(Graphics g = Graphics.FromImage(bmp))
				{
					g.FillRectangle(sb, 0, 0, nWidth, nHeight);
				}
			}

			return bmp;
		}

		public static ImageList BuildImageList(List<PwCustomIcon> vImages,
			int nWidth, int nHeight)
		{
			ImageList imgList = new ImageList();

			imgList.ImageSize = new Size(nWidth, nHeight);
			imgList.ColorDepth = ColorDepth.Depth32Bit;

			foreach(PwCustomIcon pwci in vImages)
			{
				Image imgNew = pwci.Image;

				if(imgNew == null) { Debug.Assert(false); continue; }

				if((imgNew.Width != nWidth) || (imgNew.Height != nHeight))
					imgNew = new Bitmap(imgNew, new Size(nWidth, nHeight));

				imgList.Images.Add(imgNew);
			}

			return imgList;
		}

		public static ImageList ConvertImageList24(ImageList vSourceImages,
			int nWidth, int nHeight, Color clrBack)
		{
			ImageList vNew = new ImageList();
			vNew.ImageSize = new Size(nWidth, nHeight);
			vNew.ColorDepth = ColorDepth.Depth24Bit;

			SolidBrush brushBk = new SolidBrush(clrBack);

			foreach(Image img in vSourceImages.Images)
			{
				Bitmap bmpNew = new Bitmap(nWidth, nHeight, PixelFormat.Format24bppRgb);

				using(Graphics g = Graphics.FromImage(bmpNew))
				{
					g.FillRectangle(brushBk, 0, 0, nWidth, nHeight);

					if((img.Width == nWidth) && (img.Height == nHeight))
						g.DrawImageUnscaled(img, 0, 0);
					else
					{
						g.InterpolationMode = InterpolationMode.High;
						g.DrawImage(img, 0, 0, nWidth, nHeight);
					}
				}

				vNew.Images.Add(bmpNew);
			}

			return vNew;
		}

		public static ImageList CloneImageList(ImageList ilSource, bool bCloneImages)
		{
			Debug.Assert(ilSource != null); if(ilSource == null) throw new ArgumentNullException("ilSource");

			ImageList ilNew = new ImageList();
			ilNew.ColorDepth = ilSource.ColorDepth;
			ilNew.ImageSize = ilSource.ImageSize;

			foreach(Image img in ilSource.Images)
			{
				if(bCloneImages) ilNew.Images.Add((Image)img.Clone());
				else ilNew.Images.Add(img);
			}

			return ilNew;
		}

		public static bool DrawAnimatedRects(Rectangle rectFrom, Rectangle rectTo)
		{
			bool bResult;

			try
			{
				NativeMethods.RECT rnFrom = new NativeMethods.RECT(rectFrom);
				NativeMethods.RECT rnTo = new NativeMethods.RECT(rectTo);

				bResult = NativeMethods.DrawAnimatedRects(IntPtr.Zero,
					NativeMethods.IDANI_CAPTION, ref rnFrom, ref rnTo);
			}
			catch(Exception) { Debug.Assert(false); bResult = false; }

			return bResult;
		}

		private static void SetCueBanner(IntPtr hWnd, string strText)
		{
			Debug.Assert(strText != null); if(strText == null) throw new ArgumentNullException("strText");

			try
			{
				IntPtr pText = Marshal.StringToHGlobalUni(strText);
				NativeMethods.SendMessage(hWnd, NativeMethods.EM_SETCUEBANNER,
					IntPtr.Zero, pText);
				Marshal.FreeHGlobal(pText); pText = IntPtr.Zero;
			}
			catch(Exception) { Debug.Assert(false); }
		}

		public static void SetCueBanner(TextBox tb, string strText)
		{
			SetCueBanner(tb.Handle, strText);
		}

		public static void SetCueBanner(ToolStripTextBox tb, string strText)
		{
			SetCueBanner(tb.TextBox, strText);
		}

		public static void SetCueBanner(ToolStripComboBox tb, string strText)
		{
			try
			{
				NativeMethods.COMBOBOXINFO cbi = new NativeMethods.COMBOBOXINFO();
				cbi.cbSize = Marshal.SizeOf(cbi);

				NativeMethods.GetComboBoxInfo(tb.ComboBox.Handle, ref cbi);

				SetCueBanner(cbi.hwndEdit, strText);
			}
			catch(Exception) { Debug.Assert(false); }
		}

		public static Bitmap CreateScreenshot()
		{
			Screen s = Screen.PrimaryScreen;
			Bitmap bmp = new Bitmap(s.Bounds.Width, s.Bounds.Height);

			using(Graphics g = Graphics.FromImage(bmp))
			{
				g.CopyFromScreen(s.Bounds.Location, new Point(0, 0),
					s.Bounds.Size);
			}

			return bmp;
		}

		public static void DimImage(Image bmp)
		{
			using(Brush b = new SolidBrush(Color.FromArgb(192, Color.Black)))
			{
				using(Graphics g = Graphics.FromImage(bmp))
				{
					g.FillRectangle(b, 0, 0, bmp.Width, bmp.Height);
				}
			}
		}

		public static void PrepareStandardMultilineControl(RichTextBox rtb)
		{
			Debug.Assert(rtb != null); if(rtb == null) throw new ArgumentNullException("rtb");

			try
			{
				int nStyle = NativeMethods.GetWindowStyle(rtb.Handle);

				if((nStyle & NativeMethods.ES_WANTRETURN) == 0)
				{
					NativeMethods.SetWindowLong(rtb.Handle, NativeMethods.GWL_STYLE,
						nStyle | NativeMethods.ES_WANTRETURN);

					Debug.Assert((NativeMethods.GetWindowStyle(rtb.Handle) &
						NativeMethods.ES_WANTRETURN) != 0);
				}
			}
			catch(Exception) { }
		}

		/// <summary>
		/// Fill a <c>ListView</c> with password entries.
		/// </summary>
		/// <param name="lv"><c>ListView</c> to fill.</param>
		/// <param name="vEntries">Entries.</param>
		/// <param name="vColumns">Columns of the <c>ListView</c>. The first
		/// parameter of the key-value pair is the internal string field name,
		/// and the second one the text displayed in the column header.</param>
		public static void CreateEntryList(ListView lv, IEnumerable<PwEntry> vEntries,
			List<KeyValuePair<string, string>> vColumns, ImageList ilIcons)
		{
			if(lv == null) throw new ArgumentNullException("lv");
			if(vEntries == null) throw new ArgumentNullException("vEntries");
			if(vColumns == null) throw new ArgumentNullException("vColumns");
			if(vColumns.Count == 0) throw new ArgumentException();

			lv.BeginUpdate();

			lv.Items.Clear();
			lv.Columns.Clear();
			lv.ShowGroups = true;
			lv.SmallImageList = ilIcons;

			foreach(KeyValuePair<string, string> kvp in vColumns)
			{
				lv.Columns.Add(kvp.Value);
			}

			DocumentManagerEx dm = Program.MainForm.DocumentManager;
			ListViewGroup lvg = new ListViewGroup(Guid.NewGuid().ToString());
			DateTime dtNow = DateTime.Now;
			bool bFirstEntry = true;

			foreach(PwEntry pe in vEntries)
			{
				if(pe == null) { Debug.Assert(false); continue; }

				if(pe.ParentGroup != null)
				{
					string strGroup = pe.ParentGroup.GetFullPath();

					if(strGroup != lvg.Header)
					{
						lvg = new ListViewGroup(strGroup, HorizontalAlignment.Left);
						lv.Groups.Add(lvg);
					}
				}

				ListViewItem lvi = new ListViewItem(AppDefs.GetEntryField(pe, vColumns[0].Key));

				if(pe.Expires && (pe.ExpiryTime <= dtNow))
					lvi.ImageIndex = (int)PwIcon.Expired;
				else if(pe.CustomIconUuid == PwUuid.Zero)
					lvi.ImageIndex = (int)pe.IconId;
				else
				{
					lvi.ImageIndex = (int)pe.IconId;

					foreach(DocumentStateEx ds in dm.Documents)
					{
						int nInx = ds.Database.GetCustomIconIndex(pe.CustomIconUuid);
						if(nInx > -1)
						{
							ilIcons.Images.Add((Image)ds.Database.GetCustomIcon(
								pe.CustomIconUuid).Clone());
							lvi.ImageIndex = ilIcons.Images.Count - 1;
							break;
						}
					}
				}

				for(int iCol = 1; iCol < vColumns.Count; ++iCol)
				{
					lvi.SubItems.Add(AppDefs.GetEntryField(pe, vColumns[iCol].Key));
				}

				lvi.Tag = pe;

				lv.Items.Add(lvi);
				lvg.Items.Add(lvi);

				if(bFirstEntry)
				{
					lvi.Selected = true;
					lvi.Focused = true;

					bFirstEntry = false;
				}
			}

			int nColWidth = (lv.ClientRectangle.Width - GetVScrollBarWidth()) /
				vColumns.Count;
			foreach(ColumnHeader ch in lv.Columns)
			{
				ch.Width = nColWidth;
			}

			lv.EndUpdate();
		}

		public static int GetVScrollBarWidth()
		{
			try { return SystemInformation.VerticalScrollBarWidth; }
			catch(Exception) { Debug.Assert(false); }

			return 18; // Default theme on Windows Vista
		}

		public static string CreateFileTypeFilter(string strExtension, string strDescription,
			bool bIncludeAllFiles)
		{
			string str = string.Empty;

			if((strExtension != null) && (strExtension.Length > 0) &&
				(strDescription != null) && (strDescription.Length > 0))
			{
				str += strDescription + @" (*." + strExtension + @")|*." + strExtension;
			}

			if(bIncludeAllFiles)
			{
				if(str.Length > 0) str += @"|";

				str += KPRes.AllFiles + @" (*.*)|*.*";
			}

			return str;
		}

		public static OpenFileDialog CreateOpenFileDialog(string strTitle, string strFilter,
			int iFilterIndex, string strDefaultExt, bool bMultiSelect, bool bRestoreDirectory)
		{
			OpenFileDialog ofd = new OpenFileDialog();

			ofd.CheckFileExists = true;
			ofd.CheckPathExists = true;
			
			if((strDefaultExt != null) && (strDefaultExt.Length > 0))
				ofd.DefaultExt = strDefaultExt;

			ofd.DereferenceLinks = true;

			if((strFilter != null) && (strFilter.Length > 0))
			{
				ofd.Filter = strFilter;

				if(iFilterIndex > 0) ofd.FilterIndex = iFilterIndex;
			}

			ofd.Multiselect = bMultiSelect;
			ofd.ReadOnlyChecked = false;
			ofd.RestoreDirectory = bRestoreDirectory;
			ofd.ShowHelp = false;
			ofd.ShowReadOnly = false;
			ofd.SupportMultiDottedExtensions = false;

			if((strTitle != null) && (strTitle.Length > 0))
				ofd.Title = strTitle;

			ofd.ValidateNames = true;

			return ofd;
		}

		public static SaveFileDialog CreateSaveFileDialog(string strTitle,
			string strSuggestedFileName, string strFilter, int iFilterIndex,
			string strDefaultExt, bool bRestoreDirectory)
		{
			SaveFileDialog sfd = new SaveFileDialog();

			sfd.AddExtension = true;
			sfd.CheckFileExists = false;
			sfd.CheckPathExists = true;
			sfd.CreatePrompt = false;

			if((strDefaultExt != null) && (strDefaultExt.Length > 0))
				sfd.DefaultExt = strDefaultExt;

			sfd.DereferenceLinks = true;

			if((strSuggestedFileName != null) && (strSuggestedFileName.Length > 0))
				sfd.FileName = strSuggestedFileName;

			if((strFilter != null) && (strFilter.Length > 0))
			{
				sfd.Filter = strFilter;

				if(iFilterIndex > 0) sfd.FilterIndex = iFilterIndex;
			}

			sfd.OverwritePrompt = true;
			sfd.RestoreDirectory = bRestoreDirectory;
			sfd.ShowHelp = false;
			sfd.SupportMultiDottedExtensions = false;

			if((strTitle != null) && (strTitle.Length > 0))
				sfd.Title = strTitle;

			sfd.ValidateNames = true;

			return sfd;
		}

		public static FolderBrowserDialog CreateFolderBrowserDialog(string strDescription)
		{
			FolderBrowserDialog fbd = new FolderBrowserDialog();

			if((strDescription != null) && (strDescription.Length > 0))
				fbd.Description = strDescription;

			fbd.ShowNewFolderButton = true;

			return fbd;
		}

		public static void SetGroupNodeToolTip(TreeNode tn, PwGroup pg)
		{
			if((tn == null) || (pg == null)) { Debug.Assert(false); return; }

			if(pg.Notes.Length > 0)
			{
				string str = pg.Name + MessageService.NewParagraph + pg.Notes;

				try { tn.ToolTipText = str; }
				catch(Exception) { Debug.Assert(false); }
			}
		}
	}
}
