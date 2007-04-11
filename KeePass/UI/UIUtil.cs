/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2007 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePass.Native;
using KeePass.Util;

using KeePassLib;

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
	}
}
