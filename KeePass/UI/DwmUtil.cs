/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2014 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Diagnostics;

using KeePass.Native;
using KeePass.Util;

namespace KeePass.UI
{
	public static class DwmUtil
	{
		// private const uint DWMWA_FORCE_ICONIC_REPRESENTATION = 7;
		// private const uint DWMWA_HAS_ICONIC_BITMAP = 10;
		private const uint DWMWA_DISALLOW_PEEK = 11;

		// public const int WM_DWMSENDICONICTHUMBNAIL = 0x0323;
		// public const int WM_DWMSENDICONICLIVEPREVIEWBITMAP = 0x0326;

		// [DllImport("DwmApi.dll")]
		// private static extern Int32 DwmExtendFrameIntoClientArea(IntPtr hWnd,
		//	ref MARGINS pMarInset);

		// [DllImport("DwmApi.dll")]
		// private static extern Int32 DwmIsCompositionEnabled(ref Int32 pfEnabled);

		// [DllImport("DwmApi.dll")]
		// private static extern int DwmInvalidateIconicBitmaps(IntPtr hWnd);

		[DllImport("DwmApi.dll", EntryPoint = "DwmSetWindowAttribute")]
		private static extern int DwmSetWindowAttributeInt(IntPtr hWnd,
			uint dwAttribute, [In] ref int pvAttribute, uint cbAttribute);

		// [DllImport("DwmApi.dll")]
		// private static extern int DwmSetIconicThumbnail(IntPtr hWnd,
		//	IntPtr hBmp, uint dwSITFlags);

		// [DllImport("DwmApi.dll")]
		// private static extern int DwmSetIconicLivePreviewBitmap(IntPtr hWnd,
		//	IntPtr hBmp, IntPtr pptClient, uint dwSITFlags);

		public static void EnableWindowPeekPreview(Form f, bool bEnable)
		{
			int iNoPeek = (bEnable ? 0 : 1);

			try
			{
				IntPtr h = f.Handle;
				if(h == IntPtr.Zero) { Debug.Assert(false); return; }

				DwmSetWindowAttributeInt(h, DWMWA_DISALLOW_PEEK, ref iNoPeek, 4);
				// DwmInvalidateIconicBitmaps(h);

				// EnableCustomPreviews(f, !bEnable);
			}
			catch(Exception) { Debug.Assert(!WinUtil.IsAtLeastWindowsVista); }
		}

		/* private static void EnableCustomPreviews(Form f, bool bEnable)
		{
			int s = (bEnable ? 1 : 0);

			try
			{
				IntPtr h = f.Handle;
				if(h == IntPtr.Zero) { Debug.Assert(false); return; }

				DwmSetWindowAttributeInt(h, DWMWA_HAS_ICONIC_BITMAP, ref s, 4);
				DwmSetWindowAttributeInt(h, DWMWA_FORCE_ICONIC_REPRESENTATION, ref s, 4);
			}
			catch(Exception) { Debug.Assert(!WinUtil.IsAtLeastWindowsVista); }
		}

		public static void SetThumbnailIcon(Form f, Icon ico, IntPtr lParam)
		{
			Image img = null;
			Bitmap bmp = null;
			IntPtr hBmp = IntPtr.Zero;
			try
			{
				if((f.WindowState != FormWindowState.Minimized) &&
					f.Visible)
					return;

				IntPtr h = f.Handle;
				if(h == IntPtr.Zero) { Debug.Assert(false); return; }

				img = UIUtil.ExtractVistaIcon(ico);
				if(img == null) img = ico.ToBitmap();
				if(img == null) { Debug.Assert(false); return; }

				long lThumbInfo = lParam.ToInt64();
				int iThumbWidth = (int)((lThumbInfo >> 16) & 0xFFFF);
				int iThumbHeight = (int)(lThumbInfo & 0xFFFF);

				if((iThumbWidth <= 0) || (iThumbHeight <= 0)) return;
				if(iThumbWidth > 512) iThumbWidth = 512;
				if(iThumbHeight > 512) iThumbHeight = 512;

				int iImgW = img.Width;
				int iImgH = img.Height;
				if(iImgW > iThumbWidth)
				{
					float fRatio = (float)iThumbWidth / (float)iImgW;
					iImgW = iThumbWidth;
					iImgH = (int)((float)iImgH * fRatio);
				}
				if(iImgH > iThumbHeight)
				{
					float fRatio = (float)iThumbHeight / (float)iImgH;
					iImgW = (int)((float)iImgW * fRatio);
					iImgH = iThumbHeight;
				}
				if((iImgW <= 0) || (iImgH <= 0)) { Debug.Assert(false); return; }
				if(iImgW > iThumbWidth) { Debug.Assert(false); iImgW = iThumbWidth; }
				if(iImgH > iThumbHeight) { Debug.Assert(false); iImgH = iThumbHeight; }

				iImgW = Math.Min(iImgW, 64);
				iImgH = Math.Min(iImgH, 64);

				int iImgX = (iThumbWidth - iImgW) / 2;
				int iImgY = (iThumbHeight - iImgH) / 2;

				bmp = new Bitmap(iThumbWidth, iThumbHeight, PixelFormat.Format32bppArgb);
				using(Graphics g = Graphics.FromImage(bmp))
				{
					g.Clear(Color.Black);

					g.InterpolationMode = InterpolationMode.HighQualityBicubic;
					g.SmoothingMode = SmoothingMode.HighQuality;

					g.DrawImage(img, iImgX, iImgY, iImgW, iImgH);
				}

				hBmp = bmp.GetHbitmap();
				DwmSetIconicThumbnail(h, hBmp, 0);
			}
			catch(Exception) { Debug.Assert(!WinUtil.IsAtLeastWindowsVista); }
			finally
			{
				if(hBmp != IntPtr.Zero)
				{
					try { NativeMethods.DeleteObject(hBmp); }
					catch(Exception) { }
				}
				if(bmp != null) bmp.Dispose();
				if(img != null) img.Dispose();
			}
		} */
	}
}
