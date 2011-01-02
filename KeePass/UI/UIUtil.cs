/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2011 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Windows.Forms.VisualStyles;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;
using System.IO;

using KeePass.App;
using KeePass.App.Configuration;
using KeePass.Native;
using KeePass.Resources;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Delegates;
using KeePassLib.Interfaces;
using KeePassLib.Utility;

namespace KeePass.UI
{
	public static class UIUtil
	{
		private static ToolStripRenderer m_tsrOverride = null;
		public static ToolStripRenderer ToolStripRendererOverride
		{
			get { return m_tsrOverride; }
			set { m_tsrOverride = value; }
		}

		private static bool m_bVistaStyleLists = false;
		public static bool VistaStyleListsSupported
		{
			get { return m_bVistaStyleLists; }
		}

		public static void Initialize(bool bReinitialize)
		{
			bool bVisualStyles = true;
			try { bVisualStyles = VisualStyleRenderer.IsSupported; }
			catch(Exception) { Debug.Assert(false); bVisualStyles = false; }

			if(m_tsrOverride != null) ToolStripManager.Renderer = m_tsrOverride;
			else if(Program.Config.UI.UseCustomToolStripRenderer && bVisualStyles)
			{
				ToolStripManager.Renderer = new CustomToolStripRendererEx();
				Debug.Assert(ToolStripManager.RenderMode == ToolStripManagerRenderMode.Custom);
			}
			else if(bReinitialize)
				ToolStripManager.Renderer = new ToolStripProfessionalRenderer();

			m_bVistaStyleLists = (WinUtil.IsAtLeastWindowsVista &&
				(Environment.Version.Major >= 2));
		}

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

		public static void RtfLinkifyExtUrls(RichTextBox richTextBox, bool bResetSelection)
		{
			const string strProto = "cmd://";

			try
			{
				string strText = richTextBox.Text;

				int nOffset = 0;
				while(nOffset < strText.Length)
				{
					int nStart = strText.IndexOf(strProto, nOffset, StrUtil.CaseIgnoreCmp);
					if(nStart < 0) break;

					richTextBox.Select(nStart, UrlUtil.GetUrlLength(strText, nStart));
					RtfSetSelectionLink(richTextBox);

					nOffset = nStart + 1;
				}

				if(bResetSelection) richTextBox.Select(0, 0);
			}
			catch(Exception) { Debug.Assert(false); }
		}

		public static void RtfLinkifyText(RichTextBox rtb, string strLinkText,
			bool bResetTempSelection)
		{
			if(rtb == null) throw new ArgumentNullException("rtb");
			if(string.IsNullOrEmpty(strLinkText)) return; // No assert

			try
			{
				string strText = rtb.Text;
				int nStart = strText.IndexOf(strLinkText);

				if(nStart >= 0)
				{
					rtb.Select(nStart, strLinkText.Length);
					RtfSetSelectionLink(rtb);

					if(bResetTempSelection) rtb.Select(0, 0);
				}
			}
			catch(Exception) { Debug.Assert(false); }
		}

		public static Image CreateColorBitmap24(int nWidth, int nHeight, Color color)
		{
			Bitmap bmp = new Bitmap(nWidth, nHeight, PixelFormat.Format24bppRgb);

			using(SolidBrush br = new SolidBrush(color))
			{
				using(Graphics g = Graphics.FromImage(bmp))
				{
					g.FillRectangle(br, 0, 0, nWidth, nHeight);
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

			List<Image> lImages = BuildImageListEx(vImages, nWidth, nHeight);
			if((lImages != null) && (lImages.Count > 0))
				imgList.Images.AddRange(lImages.ToArray());

			return imgList;
		}

		public static List<Image> BuildImageListEx(List<PwCustomIcon> vImages,
			int nWidth, int nHeight)
		{
			List<Image> lImages = new List<Image>();

			foreach(PwCustomIcon pwci in vImages)
			{
				Image imgNew = pwci.Image;
				if(imgNew == null) { Debug.Assert(false); continue; }

				if((imgNew.Width != nWidth) || (imgNew.Height != nHeight))
					imgNew = new Bitmap(imgNew, new Size(nWidth, nHeight));

				lImages.Add(imgNew);
			}

			return lImages;
		}

		public static ImageList ConvertImageList24(List<Image> vImages,
			int nWidth, int nHeight, Color clrBack)
		{
			ImageList ilNew = new ImageList();
			ilNew.ImageSize = new Size(nWidth, nHeight);
			ilNew.ColorDepth = ColorDepth.Depth24Bit;

			SolidBrush brushBk = new SolidBrush(clrBack);

			List<Image> vNewImages = new List<Image>();
			foreach(Image img in vImages)
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

				vNewImages.Add(bmpNew);
			}
			ilNew.Images.AddRange(vNewImages.ToArray());

			return ilNew;
		}

		public static ImageList CloneImageList(ImageList ilSource, bool bCloneImages)
		{
			Debug.Assert(ilSource != null); if(ilSource == null) throw new ArgumentNullException("ilSource");

			ImageList ilNew = new ImageList();
			ilNew.ColorDepth = ilSource.ColorDepth;
			ilNew.ImageSize = ilSource.ImageSize;

			foreach(Image img in ilSource.Images)
			{
				if(bCloneImages) ilNew.Images.Add(new Bitmap(img));
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
			try
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
			catch(Exception) { } // Throws on Cocoa and Quartz

			return null;
		}

		public static void DimImage(Image bmp)
		{
			if(bmp == null) { Debug.Assert(false); return; }

			using(Brush b = new SolidBrush(Color.FromArgb(192, Color.Black)))
			{
				using(Graphics g = Graphics.FromImage(bmp))
				{
					g.FillRectangle(b, 0, 0, bmp.Width, bmp.Height);
				}
			}
		}

		public static void PrepareStandardMultilineControl(RichTextBox rtb,
			bool bSimpleTextOnly)
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

			CustomRichTextBoxEx crtb = (rtb as CustomRichTextBoxEx);
			if(crtb != null) crtb.SimpleTextOnly = bSimpleTextOnly;
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

					foreach(PwDocument ds in dm.Documents)
					{
						int nInx = ds.Database.GetCustomIconIndex(pe.CustomIconUuid);
						if(nInx > -1)
						{
							ilIcons.Images.Add(new Bitmap(ds.Database.GetCustomIcon(
								pe.CustomIconUuid)));
							lvi.ImageIndex = ilIcons.Images.Count - 1;
							break;
						}
					}
				}

				for(int iCol = 1; iCol < vColumns.Count; ++iCol)
					lvi.SubItems.Add(AppDefs.GetEntryField(pe, vColumns[iCol].Key));

				if(!pe.ForegroundColor.IsEmpty)
					lvi.ForeColor = pe.ForegroundColor;
				if(!pe.BackgroundColor.IsEmpty)
					lvi.BackColor = pe.BackgroundColor;

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
			// ofd.SupportMultiDottedExtensions = false; // Default

			if((strTitle != null) && (strTitle.Length > 0))
				ofd.Title = strTitle;

			ofd.ValidateNames = true;

			return ofd;
		}

		public static SaveFileDialog CreateSaveFileDialog(string strTitle,
			string strSuggestedFileName, string strFilter, int iFilterIndex,
			string strDefaultExt, bool bRestoreDirectory)
		{
			return CreateSaveFileDialog(strTitle, strSuggestedFileName, strFilter,
				iFilterIndex, strDefaultExt, bRestoreDirectory, false);
		}

		public static SaveFileDialog CreateSaveFileDialog(string strTitle,
			string strSuggestedFileName, string strFilter, int iFilterIndex,
			string strDefaultExt, bool bRestoreDirectory, bool bIsDatabaseFile)
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
			// sfd.SupportMultiDottedExtensions = false; // Default

			if((strTitle != null) && (strTitle.Length > 0))
				sfd.Title = strTitle;

			sfd.ValidateNames = true;

			if(bIsDatabaseFile && (Program.Config.Defaults.FileSaveAsDirectory.Length > 0))
				sfd.InitialDirectory = Program.Config.Defaults.FileSaveAsDirectory;

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

		private static ColorDialog CreateColorDialog(Color clrDefault)
		{
			ColorDialog dlg = new ColorDialog();

			dlg.AllowFullOpen = true;
			dlg.AnyColor = true;
			if(!clrDefault.IsEmpty) dlg.Color = clrDefault;
			dlg.FullOpen = true;
			dlg.ShowHelp = false;
			// dlg.SolidColorOnly = false;

			try
			{
				string strColors = Program.Config.Defaults.CustomColors;
				if(!string.IsNullOrEmpty(strColors))
				{
					int[] vColors = StrUtil.DeserializeIntArray(strColors);
					if((vColors != null) && (vColors.Length > 0))
						dlg.CustomColors = vColors;
				}
			}
			catch(Exception) { Debug.Assert(false); }

			return dlg;
		}

		private static void SaveCustomColors(ColorDialog dlg)
		{
			if(dlg == null) { Debug.Assert(false); return; }

			try
			{
				int[] vColors = dlg.CustomColors;
				if((vColors == null) || (vColors.Length == 0))
					Program.Config.Defaults.CustomColors = string.Empty;
				else
					Program.Config.Defaults.CustomColors =
						StrUtil.SerializeIntArray(vColors);
			}
			catch(Exception) { Debug.Assert(false); }
		}

		public static Color? ShowColorDialog(Color clrDefault)
		{
			ColorDialog dlg = CreateColorDialog(clrDefault);

			GlobalWindowManager.AddDialog(dlg);
			DialogResult dr = dlg.ShowDialog();
			GlobalWindowManager.RemoveDialog(dlg);

			SaveCustomColors(dlg);

			if(dr == DialogResult.OK) return dlg.Color;
			return null;
		}

		public static FontDialog CreateFontDialog(bool bEffects)
		{
			FontDialog dlg = new FontDialog();

			dlg.FontMustExist = true;
			dlg.ShowEffects = bEffects;

			return dlg;
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

		public static Color LightenColor(Color clrBase, double dblFactor)
		{
			if((dblFactor <= 0.0) || (dblFactor > 1.0)) return clrBase;

			unchecked
			{
				byte r = (byte)((double)(255 - clrBase.R) * dblFactor + clrBase.R);
				byte g = (byte)((double)(255 - clrBase.G) * dblFactor + clrBase.G);
				byte b = (byte)((double)(255 - clrBase.B) * dblFactor + clrBase.B);
				return Color.FromArgb((int)r, (int)g, (int)b);
			}
		}

		public static Color DarkenColor(Color clrBase, double dblFactor)
		{
			if((dblFactor <= 0.0) || (dblFactor > 1.0)) return clrBase;

			unchecked
			{
				byte r = (byte)((double)clrBase.R - ((double)clrBase.R * dblFactor));
				byte g = (byte)((double)clrBase.G - ((double)clrBase.G * dblFactor));
				byte b = (byte)((double)clrBase.B - ((double)clrBase.B * dblFactor));
				return Color.FromArgb((int)r, (int)g, (int)b);
			}
		}

		public static void MoveSelectedItemsInternalOne<T>(ListView lv,
			PwObjectList<T> v, bool bUp)
			where T : class, IDeepCloneable<T>
		{
			if(lv == null) throw new ArgumentNullException("lv");
			if(v == null) throw new ArgumentNullException("v");
			if(lv.Items.Count != (int)v.UCount) throw new ArgumentException();

			ListView.SelectedListViewItemCollection lvsic = lv.SelectedItems;
			if(lvsic.Count == 0) return;

			int nStart = (bUp ? 0 : (lvsic.Count - 1));
			int nEnd = (bUp ? lvsic.Count : -1);
			for(int i = nStart; i != nEnd; i += (bUp ? 1 : -1))
				v.MoveOne(lvsic[i].Tag as T, bUp);
		}

		public static void DeleteSelectedItems<T>(ListView lv, PwObjectList<T>
			vInternalList)
			where T : class, IDeepCloneable<T>
		{
			if(lv == null) throw new ArgumentNullException("lv");
			if(vInternalList == null) throw new ArgumentNullException("vInternalList");

			ListView.SelectedIndexCollection lvsic = lv.SelectedIndices;
			int nSelectedCount = lvsic.Count;
			for(int i = 0; i < nSelectedCount; ++i)
			{
				int nIndex = lvsic[nSelectedCount - i - 1];
				if(vInternalList.Remove(lv.Items[nIndex].Tag as T))
					lv.Items.RemoveAt(nIndex);
				else { Debug.Assert(false); }
			}
		}

		public static object[] GetSelectedItemTags(ListView lv)
		{
			if(lv == null) throw new ArgumentNullException("lv");

			ListView.SelectedListViewItemCollection lvsic = lv.SelectedItems;
			object[] p = new object[lvsic.Count];
			for(int i = 0; i < lvsic.Count; ++i) p[i] = lvsic[i].Tag;
			return p;
		}

		public static void SelectItems(ListView lv, object[] vItemTags)
		{
			if(lv == null) throw new ArgumentNullException("lv");
			if(vItemTags == null) throw new ArgumentNullException("vItemTags");

			for(int i = 0; i < lv.Items.Count; ++i)
				if(Array.IndexOf<object>(vItemTags, lv.Items[i].Tag) >= 0)
					lv.Items[i].Selected = true;
		}

		public static void SetWebBrowserDocument(WebBrowser wb, string strDocumentText)
		{
			string strContent = (strDocumentText ?? string.Empty);

			wb.AllowNavigation = true;
			wb.DocumentText = strContent;

			// Wait for document being loaded
			for(int i = 0; i < 50; ++i)
			{
				if(wb.DocumentText == strContent) break;

				Thread.Sleep(20);
				Application.DoEvents();
			}
		}

		public static string GetWebBrowserDocument(WebBrowser wb)
		{
			return wb.DocumentText;
		}

		public static void SetExplorerTheme(IntPtr hWnd)
		{
			if(hWnd == IntPtr.Zero) { Debug.Assert(false); return; }

			try { NativeMethods.SetWindowTheme(hWnd, "explorer", null); }
			catch(Exception) { } // Not supported on older operating systems
		}

		public static Image LoadImage(byte[] pb)
		{
			if(pb == null) throw new ArgumentNullException("pb");

			MemoryStream ms = new MemoryStream(pb, false);

			try { return Image.FromStream(ms); }
			catch(Exception)
			{
				Image imgIco = TryLoadIco(pb);
				if(imgIco != null) return imgIco;

				throw;
			}
		}

		private static Image TryLoadIco(byte[] pb)
		{
			MemoryStream ms = new MemoryStream(pb, false);

			try { return (new Icon(ms)).ToBitmap(); }
			catch(Exception) { }

			return null;
		}

		public static void SetShield(Button btn, bool bSetShield)
		{
			if(btn == null) throw new ArgumentNullException("btn");

			try
			{
				if(btn.FlatStyle != FlatStyle.System)
				{
					Debug.Assert(false);
					btn.FlatStyle = FlatStyle.System;
				}

				IntPtr h = btn.Handle;
				if(h == IntPtr.Zero) { Debug.Assert(false); return; }

				NativeMethods.SendMessage(h, NativeMethods.BCM_SETSHIELD,
					IntPtr.Zero, (IntPtr)(bSetShield ? 1 : 0));
			}
			catch(Exception) { Debug.Assert(false); }
		}

		public static void ConfigureTbButton(ToolStripItem tb, string strText,
			string strTooltip)
		{
			if(strText != null) tb.Text = strText;

			if(strTooltip != null)
				tb.ToolTipText = StrUtil.RemoveAccelerator(strTooltip);
			else if(strText != null)
				tb.ToolTipText = StrUtil.RemoveAccelerator(strText);
		}

		public static void CreateGroupList(PwGroup pgContainer, ComboBox cmb,
			Dictionary<int, PwUuid> outCreatedItems, PwUuid uuidToSelect,
			out int iSelectIndex)
		{
			iSelectIndex = -1;

			if(pgContainer == null) { Debug.Assert(false); return; }
			if(cmb == null) { Debug.Assert(false); return; }
			// Do not clear the combobox!

			int iSelectInner = -1;
			GroupHandler gh = delegate(PwGroup pg)
			{
				string str = new string(' ', Math.Abs(8 * ((int)pg.GetLevel() - 1)));
				str += pg.Name;

				if((uuidToSelect != null) && pg.Uuid.EqualsValue(uuidToSelect))
					iSelectInner = cmb.Items.Count;

				if(outCreatedItems != null)
					outCreatedItems[cmb.Items.Count] = pg.Uuid;

				cmb.Items.Add(str);
				return true;
			};

			pgContainer.TraverseTree(TraversalMethod.PreOrder, gh, null);
			iSelectIndex = iSelectInner;
		}

		public static void MakeInheritableBoolComboBox(ComboBox cmb, bool? bSelect,
			bool bInheritedState)
		{
			if(cmb == null) { Debug.Assert(false); return; }

			cmb.Items.Clear();
			cmb.Items.Add(KPRes.InheritSettingFromParent + " (" + (bInheritedState ?
				KPRes.Enabled : KPRes.Disabled) + ")");
			cmb.Items.Add(KPRes.Enabled);
			cmb.Items.Add(KPRes.Disabled);

			if(bSelect.HasValue) cmb.SelectedIndex = (bSelect.Value ? 1 : 2);
			else cmb.SelectedIndex = 0;
		}

		public static bool? GetInheritableBoolComboBoxValue(ComboBox cmb)
		{
			if(cmb == null) { Debug.Assert(false); return null; }

			if(cmb.SelectedIndex == 1) return true;
			if(cmb.SelectedIndex == 2) return false;
			return null;
		}

		public static void SetEnabled(Control c, bool bEnabled)
		{
			if(c == null) { Debug.Assert(false); return; }

			if(c.Enabled != bEnabled) c.Enabled = bEnabled;
		}

		public static void SetChecked(CheckBox cb, bool bChecked)
		{
			if(cb == null) { Debug.Assert(false); return; }

			if(cb.Checked != bChecked) cb.Checked = bChecked;
		}

		public static void SetChecked(ToolStripMenuItem tsmi, bool bChecked)
		{
			if(tsmi == null) { Debug.Assert(false); return; }

			if(tsmi.Checked != bChecked) tsmi.Checked = bChecked;
		}

		public static void ResizeColumns(ListView lv, bool bBlockUIUpdate)
		{
			if(lv == null) { Debug.Assert(false); return; }

			int nColumns = 0;
			foreach(ColumnHeader ch in lv.Columns)
			{
				if(ch.Width > 0) ++nColumns;
			}
			if(nColumns == 0) return;

			int cx = (lv.ClientSize.Width - 1) / nColumns;
			int cx0 = (lv.ClientSize.Width - 1) - (cx * (nColumns - 1));
			if((cx0 <= 0) || (cx <= 0)) return;

			if(bBlockUIUpdate) lv.BeginUpdate();

			bool bFirst = true;
			foreach(ColumnHeader ch in lv.Columns)
			{
				int nCurWidth = ch.Width;
				if(nCurWidth == 0) continue;
				if(bFirst && (nCurWidth == cx0)) { bFirst = false; continue; }
				if(!bFirst && (nCurWidth == cx)) continue;

				ch.Width = (bFirst ? cx0 : cx);
				bFirst = false;
			}

			if(bBlockUIUpdate) lv.EndUpdate();
		}

		public static bool ColorsEqual(Color c1, Color c2)
		{
			return ((c1.R == c2.R) && (c1.G == c2.G) && (c1.B == c2.B) &&
				(c1.A == c2.A));
		}

		public static Color GetAlternateColor(Color clrBase)
		{
			if(ColorsEqual(clrBase, Color.White)) return Color.FromArgb(238, 238, 255);

			float b = clrBase.GetBrightness();
			if(b >= 0.5) return UIUtil.DarkenColor(clrBase, 0.1);
			return UIUtil.LightenColor(clrBase, 0.25);
		}

		public static void SetAlternatingBgColors(ListView lv, Color clrAlternate,
			bool bAlternate)
		{
			if(lv == null) throw new ArgumentNullException("lv");

			Color clrBg = lv.BackColor;

			// Items in sorted groups don't have display-ordered indices
			// (in contrast to a non-grouped sorted list)
			if(lv.ShowGroups && (lv.ListViewItemSorter != null))
				bAlternate = false;

			for(int i = 0; i < lv.Items.Count; ++i)
			{
				ListViewItem lvi = lv.Items[i];
				Debug.Assert(lvi.Index == i);
				Debug.Assert(lvi.UseItemStyleForSubItems);

				if(!bAlternate)
				{
					if(ColorsEqual(lvi.BackColor, clrAlternate))
						lvi.BackColor = clrBg;
				}
				else if(((i & 1) == 0) && ColorsEqual(lvi.BackColor, clrAlternate))
					lvi.BackColor = clrBg;
				else if(((i & 1) == 1) && ColorsEqual(lvi.BackColor, clrBg))
					lvi.BackColor = clrAlternate;
			}
		}

		public static bool SetSortIcon(ListView lv, int iColumn, SortOrder so)
		{
			if(lv == null) { Debug.Assert(false); return false; }

			try
			{
				IntPtr hHeader = NativeMethods.SendMessage(lv.Handle,
					NativeMethods.LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);

				for(int i = 0; i < lv.Columns.Count; ++i)
				{
					int nGetMsg = ((WinUtil.IsWindows2000 || WinUtil.IsWindowsXP ||
						WinUtil.IsAtLeastWindowsVista) ? NativeMethods.HDM_GETITEMW :
						NativeMethods.HDM_GETITEMA);
					int nSetMsg = ((WinUtil.IsWindows2000 || WinUtil.IsWindowsXP ||
						WinUtil.IsAtLeastWindowsVista) ? NativeMethods.HDM_SETITEMW :
						NativeMethods.HDM_SETITEMA);
					IntPtr pColIndex = new IntPtr(i);

					NativeMethods.HDITEM hdItem = new NativeMethods.HDITEM();
					hdItem.mask = NativeMethods.HDI_FORMAT;

					if(NativeMethods.SendMessageHDItem(hHeader, nGetMsg, pColIndex,
						ref hdItem) == IntPtr.Zero) { Debug.Assert(false); }

					if((i != iColumn) || (so == SortOrder.None))
						hdItem.fmt &= (~NativeMethods.HDF_SORTUP &
							~NativeMethods.HDF_SORTDOWN);
					else
					{
						if(so == SortOrder.Ascending)
						{
							hdItem.fmt &= ~NativeMethods.HDF_SORTDOWN;
							hdItem.fmt |= NativeMethods.HDF_SORTUP;
						}
						else // SortOrder.Descending
						{
							hdItem.fmt &= ~NativeMethods.HDF_SORTUP;
							hdItem.fmt |= NativeMethods.HDF_SORTDOWN;
						}
					}

					Debug.Assert(hdItem.mask == NativeMethods.HDI_FORMAT);
					if(NativeMethods.SendMessageHDItem(hHeader, nSetMsg, pColIndex,
						ref hdItem) == IntPtr.Zero) { Debug.Assert(false); }
				}
			}
			catch(Exception) { Debug.Assert(false); return false; }

			return true;
		}

		public static Color ColorToGrayscale(Color clr)
		{
			int l = (int)((0.3f * clr.R) + (0.59f * clr.G) + (0.11f * clr.B));
			if(l >= 256) l = 255;
			return Color.FromArgb(l, l, l);
		}

		public static Color ColorTowards(Color clr, Color clrBase, double dblFactor)
		{
			int l = (int)((0.3f * clrBase.R) + (0.59f * clrBase.G) + (0.11f * clrBase.B));

			if(l < 128) return DarkenColor(clr, dblFactor);
			return LightenColor(clr, dblFactor);
		}

		public static Color ColorTowardsGrayscale(Color clr, Color clrBase, double dblFactor)
		{
			return ColorToGrayscale(ColorTowards(clr, clrBase, dblFactor));
		}

		public static GraphicsPath CreateRoundedRectangle(int x, int y, int dx, int dy,
			int r)
		{
			try
			{
				GraphicsPath gp = new GraphicsPath();

				gp.AddLine(x + r, y, x + dx - (r * 2), y);
				gp.AddArc(x + dx - (r * 2), y, r * 2, r * 2, 270.0f, 90.0f);
				gp.AddLine(x + dx, y + r, x + dx, y + dy - (r * 2));
				gp.AddArc(x + dx - (r * 2), y + dy - (r * 2), r * 2, r * 2, 0.0f, 90.0f);
				gp.AddLine(x + dx - (r * 2), y + dy, x + r, y + dy);
				gp.AddArc(x, y + dy - (r * 2), r * 2, r * 2, 90.0f, 90.0f);
				gp.AddLine(x, y + dy - (r * 2), x, y + r);
				gp.AddArc(x, y, r * 2, r * 2, 180.0f, 90.0f);

				gp.CloseFigure();
				return gp;
			}
			catch(Exception) { Debug.Assert(false); }

			return null;
		}

		public static Control GetActiveControl(ContainerControl cc)
		{
			if(cc == null) { Debug.Assert(false); return null; }

			try
			{
				Control c = cc.ActiveControl;
				if(c == cc) return c;

				ContainerControl ccSub = (c as ContainerControl);
				if(ccSub != null) return GetActiveControl(ccSub);
				else return c;
			}
			catch(Exception) { Debug.Assert(false); }

			return null;
		}

		public static void ApplyKeyUIFlags(ulong aceUIFlags, CheckBox cbPassword,
			CheckBox cbKeyFile, CheckBox cbUserAccount, CheckBox cbHidePassword)
		{
			if((aceUIFlags & (ulong)AceKeyUIFlags.EnablePassword) != 0)
				UIUtil.SetEnabled(cbPassword, true);
			if((aceUIFlags & (ulong)AceKeyUIFlags.EnableKeyFile) != 0)
				UIUtil.SetEnabled(cbKeyFile, true);
			if((aceUIFlags & (ulong)AceKeyUIFlags.EnableUserAccount) != 0)
				UIUtil.SetEnabled(cbUserAccount, true);
			if((aceUIFlags & (ulong)AceKeyUIFlags.EnableHidePassword) != 0)
				UIUtil.SetEnabled(cbHidePassword, true);

			if((aceUIFlags & (ulong)AceKeyUIFlags.CheckPassword) != 0)
				UIUtil.SetChecked(cbPassword, true);
			if((aceUIFlags & (ulong)AceKeyUIFlags.CheckKeyFile) != 0)
				UIUtil.SetChecked(cbKeyFile, true);
			if((aceUIFlags & (ulong)AceKeyUIFlags.CheckUserAccount) != 0)
				UIUtil.SetChecked(cbUserAccount, true);
			if((aceUIFlags & (ulong)AceKeyUIFlags.CheckHidePassword) != 0)
				UIUtil.SetChecked(cbHidePassword, true);

			if((aceUIFlags & (ulong)AceKeyUIFlags.UncheckPassword) != 0)
				UIUtil.SetChecked(cbPassword, false);
			if((aceUIFlags & (ulong)AceKeyUIFlags.UncheckKeyFile) != 0)
				UIUtil.SetChecked(cbKeyFile, false);
			if((aceUIFlags & (ulong)AceKeyUIFlags.UncheckUserAccount) != 0)
				UIUtil.SetChecked(cbUserAccount, false);
			if((aceUIFlags & (ulong)AceKeyUIFlags.UncheckHidePassword) != 0)
				UIUtil.SetChecked(cbHidePassword, false);

			if((aceUIFlags & (ulong)AceKeyUIFlags.DisablePassword) != 0)
				UIUtil.SetEnabled(cbPassword, false);
			if((aceUIFlags & (ulong)AceKeyUIFlags.DisableKeyFile) != 0)
				UIUtil.SetEnabled(cbKeyFile, false);
			if((aceUIFlags & (ulong)AceKeyUIFlags.DisableUserAccount) != 0)
				UIUtil.SetEnabled(cbUserAccount, false);
			if((aceUIFlags & (ulong)AceKeyUIFlags.DisableHidePassword) != 0)
				UIUtil.SetEnabled(cbHidePassword, false);
		}

		public static int GetTopVisibleItem(ListView lv)
		{
			if(lv == null) { Debug.Assert(false); return -1; }

			int nRes = -1;
			try
			{
				if((lv.Items.Count > 0) && (lv.TopItem != null))
					nRes = lv.TopItem.Index;
			}
			catch(Exception) { Debug.Assert(false); }

			return nRes;
		}

		public static void SetTopVisibleItem(ListView lv, int iIndex)
		{
			if(lv == null) { Debug.Assert(false); return; }
			if((iIndex < 0) || (iIndex >= lv.Items.Count)) return; // No assert

			lv.EnsureVisible(lv.Items.Count - 1);
			lv.EnsureVisible(iIndex);
		}

		/// <summary>
		/// Test whether a screen area is at least partially visible.
		/// </summary>
		/// <param name="rect">Area to test.</param>
		/// <returns>Returns <c>true</c>, if the area is at least partially
		/// visible. Otherwise, <c>false</c> is returned.</returns>
		public static bool IsScreenAreaVisible(Rectangle rect)
		{
			try
			{
				foreach(Screen scr in Screen.AllScreens)
				{
					Rectangle scrBounds = scr.Bounds;

					if((rect.Left > scrBounds.Right) || (rect.Right < scrBounds.Left) ||
						(rect.Top > scrBounds.Bottom) || (rect.Bottom < scrBounds.Top)) { }
					else return true;
				}
			}
			catch(Exception) { Debug.Assert(false); return true; }

			return false;
		}

		public static void SetButtonImage(Button btn, Image img, bool b16To15)
		{
			if(btn == null) { Debug.Assert(false); return; }
			if(img == null) { Debug.Assert(false); return; }

			if(b16To15 && (btn.Height == 23) && (img.Height == 16))
			{
				Bitmap bmp = new Bitmap(img.Width, 15, PixelFormat.Format32bppArgb);
				using(Graphics g = Graphics.FromImage(bmp))
				{
					using(SolidBrush sb = new SolidBrush(Color.Transparent))
					{
						g.FillRectangle(sb, 0, 0, img.Width, 15);
					}

					g.InterpolationMode = InterpolationMode.HighQualityBicubic;
					g.SmoothingMode = SmoothingMode.HighQuality;

					g.DrawImage(img, 0, 0, img.Width, 15);
				}

				btn.Image = bmp;
			}
			else btn.Image = img;
		}
	}
}
