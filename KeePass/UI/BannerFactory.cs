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
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Windows.Forms;

using KeePass.App;

using KeePassLib.Utility;

namespace KeePass.UI
{
	public enum BannerStyle
	{
		Default = 0,
		WinXPLogin = 1,
		WinVistaBlack = 2,
		KeePassWin32 = 3,
		BlueCarbon = 4
	}

	public static class BannerFactory
	{
		private static Dictionary<string, Image> m_vImageCache = new Dictionary<string, Image>();
		private const int MaxCachedImages = 32;

		public static Image CreateBanner(int nWidth, int nHeight, BannerStyle bs, Image imgIcon, string strTitle, string strLine)
		{
			// imgIcon may be null.
			Debug.Assert(strTitle != null); if(strTitle == null) throw new ArgumentNullException("strTitle");
			Debug.Assert(strLine != null); if(strLine == null) throw new ArgumentNullException("strLine");

			string strImageID = nWidth.ToString() + "x" + nHeight.ToString() + ":";
			if(strTitle != null) strImageID += strTitle;
			strImageID += ":";
			if(strLine != null) strImageID += strLine;

			if(bs == BannerStyle.Default) bs = Program.Config.UI.BannerStyle;

			strImageID += ":" + ((uint)bs).ToString();

			// Try getting the banner from the banner cache.
			Image img;
			if(m_vImageCache.TryGetValue(strImageID, out img)) return img;

			// Banner not in cache already -> create new banner.
			img = new Bitmap(nWidth, nHeight, PixelFormat.Format24bppRgb);
			Graphics g = Graphics.FromImage(img);
			const int offsetIcon = 10;

			Color clrStart = Color.White;
			Color clrEnd = Color.LightBlue;
			float fAngle = 0.90f;

			if(bs == BannerStyle.BlueCarbon)
			{
				fAngle = 90.0f;

				clrStart = Color.LightGray;
				clrEnd = Color.Black;

				Rectangle rect = new Rectangle(0, 0, nWidth, (nHeight * 3) / 8);
				LinearGradientBrush washBrush = new LinearGradientBrush(rect, clrStart,
					clrEnd, fAngle, true);
				g.FillRectangle(washBrush, rect);

				clrStart = Color.FromArgb(0, 0, 32);
				clrEnd = Color.FromArgb(192, 192, 255);

				rect = new Rectangle(0, nHeight / 2, nWidth, (nHeight * 5) / 8);
				washBrush = new LinearGradientBrush(rect, clrStart,
					clrEnd, fAngle, true);
				g.FillRectangle(washBrush, rect);
			}
			else
			{
				if(bs == BannerStyle.WinXPLogin)
				{
					clrStart = Color.FromArgb(200, 208, 248);
					clrEnd = Color.FromArgb(40, 64, 216);
				}
				else if(bs == BannerStyle.WinVistaBlack)
				{
					clrStart = Color.FromArgb(151, 154, 173);
					clrEnd = Color.FromArgb(27, 27, 37);

					fAngle = 90.0f;
				}
				else if(bs == BannerStyle.KeePassWin32)
				{
					clrStart = Color.FromArgb(235, 235, 255);
					clrEnd = Color.FromArgb(192, 192, 255);
				}

				Rectangle rect = new Rectangle(0, 0, nWidth, nHeight);
				LinearGradientBrush washBrush = new LinearGradientBrush(rect, clrStart,
					clrEnd, fAngle, true);
				g.FillRectangle(washBrush, rect);
			}

			if(imgIcon != null)
			{
				int yIcon = (nHeight - imgIcon.Height) / 2;
				g.DrawImageUnscaled(imgIcon, offsetIcon, yIcon);

				ColorMatrix cm = new ColorMatrix();
				cm.Matrix33 = 0.1f;
				ImageAttributes ia = new ImageAttributes();
				ia.SetColorMatrix(cm);

				int w = imgIcon.Width * 3, h = imgIcon.Height * 3;
				int x = nWidth - w - 10, y = (nHeight - h) / 2;
				Rectangle rectDest = new Rectangle(x, y, w, h);
				g.DrawImage(imgIcon, rectDest, 0, 0, imgIcon.Width, imgIcon.Height,
					GraphicsUnit.Pixel, ia);
			}

			if((bs == BannerStyle.WinXPLogin) || (bs == BannerStyle.WinVistaBlack) || (bs == BannerStyle.BlueCarbon))
			{
				Rectangle rect = new Rectangle(0, nHeight - 2, 0, 2);

				rect.Width = nWidth / 2 + 1;
				rect.X = nWidth / 2;
				clrStart = Color.FromArgb(248, 136, 24);
				clrEnd = Color.White;
				LinearGradientBrush brushOrangeWhite = new LinearGradientBrush(rect, clrStart, clrEnd, 0.90f, true);
				g.FillRectangle(brushOrangeWhite, rect);

				rect.Width = nWidth / 2 + 1;
				rect.X = 0;
				clrStart = Color.White;
				clrEnd = Color.FromArgb(248, 136, 24);
				LinearGradientBrush brushWhiteOrange = new LinearGradientBrush(rect, clrStart, clrEnd, 0.90f, true);
				g.FillRectangle(brushWhiteOrange, rect);
			}
			else if(bs == BannerStyle.KeePassWin32)
			{
				// Black separator line
				Pen penBlack = new Pen(Color.Black);
				g.DrawLine(penBlack, 0, nHeight - 1, nWidth - 1, nHeight - 1);
			}

			Font font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold);

			Brush brush;
			if(bs == BannerStyle.KeePassWin32) brush = Brushes.Black;
			else brush = Brushes.White;

			float fx = 2 * offsetIcon, fy = 9.0f;
			if(imgIcon != null) fx += imgIcon.Width;
			g.DrawString(strTitle, font, brush, fx, fy);

			Font fontSmall = new Font(FontFamily.GenericSansSerif, 9, FontStyle.Regular);
			fx += offsetIcon;
			fy += offsetIcon * 2 + 2;
			g.DrawString(strLine, fontSmall, brush, fx, fy);

			if(m_vImageCache.Count >= MaxCachedImages)
			{
				foreach(string strKey in m_vImageCache.Keys)
				{
					m_vImageCache.Remove(strKey);
					break;
				}
			}

			// Save in cache.
			m_vImageCache[strImageID] = img;

			g.Dispose();
			return img;
		}
	}
}
