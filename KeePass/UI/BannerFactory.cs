/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2010 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.ComponentModel;

using KeePass.App;
using KeePass.Util;

using KeePassLib.Utility;

namespace KeePass.UI
{
	public enum BannerStyle
	{
		// [Browsable(false)]
		Default = 0,

		WinXPLogin = 1,
		WinVistaBlack = 2,
		KeePassWin32 = 3,
		BlueCarbon = 4
	}

	public sealed class BfBannerInfo
	{
		public int Width { get; private set; }
		public int Height { get; private set; }
		public BannerStyle Style { get; private set; }
		public Image Icon { get; private set; }
		public string TitleText { get; private set; }
		public string InfoText { get; private set; }

		public BfBannerInfo(int nWidth, int nHeight, BannerStyle bs,
			Image imgIcon, string strTitle, string strLine)
		{
			this.Width = nWidth;
			this.Height = nHeight;
			this.Style = bs;
			this.Icon = imgIcon;
			this.TitleText = strTitle;
			this.InfoText = strLine;
		}
	}

	public delegate Image BfBannerGenerator(BfBannerInfo bannerInfo);

	public static class BannerFactory
	{
		private static Dictionary<string, Image> m_vImageCache = new Dictionary<string, Image>();
		private const int MaxCachedImages = 32;

		private static BfBannerGenerator m_pCustomGen = null;
		public static BfBannerGenerator CustomGenerator
		{
			get { return m_pCustomGen; }
			set { m_pCustomGen = value; }
		}

		public static Image CreateBanner(int nWidth, int nHeight, BannerStyle bs,
			Image imgIcon, string strTitle, string strLine)
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

			Debug.Assert(img == null);
			if(m_pCustomGen != null)
				img = m_pCustomGen(new BfBannerInfo(nWidth, nHeight, bs, imgIcon,
					strTitle, strLine));

			if(img == null)
			{
				img = new Bitmap(nWidth, nHeight, PixelFormat.Format24bppRgb);
				Graphics g = Graphics.FromImage(img);
				int xIcon = DpiScaleInt(10, nHeight);

				bool bRtl = Program.Translation.Properties.RightToLeft;
				if(bRtl)
				{
					g.TranslateTransform(nWidth, 0.0f);
					g.ScaleTransform(-1.0f, 1.0f);
				}

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
					int wIconScaled = DpiScaleInt(imgIcon.Width, nHeight);
					int hIconScaled = DpiScaleInt(imgIcon.Height, nHeight);

					int yIcon = (nHeight - hIconScaled) / 2;
					if(hIconScaled == imgIcon.Height)
						g.DrawImageUnscaled(imgIcon, xIcon, yIcon);
					else
						g.DrawImage(imgIcon, xIcon, yIcon, wIconScaled, hIconScaled);

					ColorMatrix cm = new ColorMatrix();
					cm.Matrix33 = 0.1f;
					ImageAttributes ia = new ImageAttributes();
					ia.SetColorMatrix(cm);

					int w = wIconScaled * 3, h = hIconScaled * 3;
					int x = nWidth - w - xIcon, y = (nHeight - h) / 2;
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

				// Brush brush;
				Color clrText;
				if(bs == BannerStyle.KeePassWin32)
				{
					// brush = Brushes.Black;
					clrText = Color.Black;
				}
				else
				{
					// brush = Brushes.White;
					clrText = Color.White;
				}

				// float fx = 2 * xIcon, fy = 9.0f;
				int tx = 2 * xIcon, ty = DpiScaleInt(9, nHeight);
				if(imgIcon != null) tx += DpiScaleInt(imgIcon.Width, nHeight); // fx

				TextFormatFlags tff = TextFormatFlags.PreserveGraphicsClipping;
				if(bRtl) tff |= TextFormatFlags.RightToLeft;

				float fFontSize = DpiScaleFloat((12.0f * 96.0f) / g.DpiY, nHeight);
				using(Font font = new Font(FontFamily.GenericSansSerif,
					fFontSize, FontStyle.Bold))
				{
					int txs = (!bRtl ? tx : (nWidth - tx - TextRenderer.MeasureText(g,
						strTitle, font).Width));

					// g.DrawString(strTitle, font, brush, fx, fy);
					BannerFactory.DrawText(g, strTitle, font, new Point(txs, ty),
						clrText, tff, nWidth, nHeight);
				}

				tx += xIcon; // fx
				ty += xIcon * 2 + 2; // fy

				float fFontSizeSm = DpiScaleFloat((9.0f * 96.0f) / g.DpiY, nHeight);
				using(Font fontSmall = new Font(FontFamily.GenericSansSerif,
					fFontSizeSm, FontStyle.Regular))
				{
					int txl = (!bRtl ? tx : (nWidth - tx - TextRenderer.MeasureText(g,
						strLine, fontSmall).Width));

					// g.DrawString(strLine, fontSmall, brush, fx, fy);
					BannerFactory.DrawText(g, strLine, fontSmall, new Point(txl, ty),
						clrText, tff, nWidth, nHeight);
				}

				g.Dispose();
			}

			if(m_vImageCache.Count >= MaxCachedImages)
			{
				foreach(string strKey in m_vImageCache.Keys)
				{
					m_vImageCache.Remove(strKey);
					break; // Remove first item only
				}
			}

			// Save in cache
			m_vImageCache[strImageID] = img;

			return img;
		}

		private static void DrawText(Graphics g, string strText,
			Font font, Point pt, Color clrForeground, TextFormatFlags tff,
			int nWidth, int nHeight)
		{
			// On Windows 2000 the DrawText method taking a Point doesn't
			// work by design, see MSDN
			if(WinUtil.IsWindows2000)
				TextRenderer.DrawText(g, strText, font, new Rectangle(pt.X, pt.Y,
					nWidth - pt.X - 1, nHeight - pt.Y - 1), clrForeground, tff);
			else
				TextRenderer.DrawText(g, strText, font, pt, clrForeground, tff);
		}

		private static int DpiScaleInt(int x, int nBaseHeight)
		{
			return (int)Math.Round((double)(x * nBaseHeight) / 60.0);
		}

		private static float DpiScaleFloat(float x, int nBaseHeight)
		{
			return ((x * (float)nBaseHeight) / 60.0f);
		}
	}
}
