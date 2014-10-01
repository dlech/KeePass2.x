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
using System.Collections.Specialized;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Diagnostics;

using KeePass.Native;
using KeePass.Util;

using KeePassLib.Utility;

using NativeLib = KeePassLib.Native.NativeLib;

namespace KeePass.UI
{
	public static class DpiUtil
	{
		private const int StdDpi = 96;

		private static bool m_bInitialized = false;

		private static int m_nDpiX = StdDpi;
		private static int m_nDpiY = StdDpi;

		private static double m_dScaleX = 1.0;
		private static double m_dScaleY = 1.0;

		private static InterpolationMode m_im = InterpolationMode.HighQualityBicubic;

		public static bool ScalingRequired
		{
			get
			{
				if(Program.DesignMode) return false;

				EnsureInitialized();
				return ((m_nDpiX != StdDpi) || (m_nDpiY != StdDpi));
			}
		}

		private static void EnsureInitialized()
		{
			if(m_bInitialized) return;
			if(NativeLib.IsUnix()) { m_bInitialized = true; return; }

			try
			{
				IntPtr hDC = NativeMethods.GetDC(IntPtr.Zero);
				if(hDC != IntPtr.Zero)
				{
					m_nDpiX = NativeMethods.GetDeviceCaps(hDC,
						NativeMethods.LOGPIXELSX);
					m_nDpiY = NativeMethods.GetDeviceCaps(hDC,
						NativeMethods.LOGPIXELSY);
					if((m_nDpiX <= 0) || (m_nDpiY <= 0))
					{
						Debug.Assert(false);
						m_nDpiX = StdDpi;
						m_nDpiY = StdDpi;
					}

					if(NativeMethods.ReleaseDC(IntPtr.Zero, hDC) != 1)
					{
						Debug.Assert(false);
					}
				}
				else { Debug.Assert(false); }
			}
			catch(Exception) { Debug.Assert(false); }

			m_dScaleX = (double)m_nDpiX / (double)StdDpi;
			m_dScaleY = (double)m_nDpiY / (double)StdDpi;

			int nDpiPct = (int)Math.Round(m_dScaleX * 100.0);
			if((nDpiPct % 100) == 0)
				m_im = InterpolationMode.NearestNeighbor;
			else if(nDpiPct < 100)
				m_im = InterpolationMode.HighQualityBilinear;
			else
				m_im = InterpolationMode.HighQualityBicubic;

			m_bInitialized = true;
		}

		public static void ConfigureProcess()
		{
			if(NativeLib.IsUnix()) return;

			// try
			// {
			//	ConfigurationManager.AppSettings.Set(
			//		"EnableWindowsFormsHighDpiAutoResizing", "true");
			// }
			// catch(Exception) { Debug.Assert(false); }
#if DEBUG
			// Ensure that the .config file enables high DPI features
			string strExeConfig = WinUtil.GetExecutable() + ".config";
			if(File.Exists(strExeConfig))
			{
				string strCM = "System.Configuration.ConfigurationManager, ";
				strCM += "System.Configuration, Version=";
				strCM += Environment.Version.Major.ToString() + ".0.0.0, ";
				strCM += "Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

				Type tCM = Type.GetType(strCM, false);
				if(tCM != null)
				{
					PropertyInfo pi = tCM.GetProperty("AppSettings",
						(BindingFlags.Public | BindingFlags.Static));
					if(pi != null)
					{
						NameValueCollection nvc = (pi.GetValue(null, null) as
							NameValueCollection);
						if(nvc != null)
						{
							Debug.Assert(string.Equals(nvc.Get(
								"EnableWindowsFormsHighDpiAutoResizing"),
								"true", StrUtil.CaseIgnoreCmp));
						}
						else { Debug.Assert(false); }
					}
					else { Debug.Assert(false); }
				}
				else { Debug.Assert(false); } // Assembly should be loaded
			}
#endif

			try
			{
				if(WinUtil.IsAtLeastWindowsVista)
					NativeMethods.SetProcessDPIAware();
			}
			catch(Exception) { Debug.Assert(false); }
		}

		internal static void Configure(ToolStrip ts)
		{
			if(ts == null) { Debug.Assert(false); return; }
			if(!DpiUtil.ScalingRequired) return;

			Size sz = ts.ImageScalingSize;
			if((sz.Width == 16) && (sz.Height == 16))
			{
				sz.Width = ScaleIntX(16);
				sz.Height = ScaleIntY(16);

				ts.ImageScalingSize = sz;
			}
			else
			{
				Debug.Assert(((sz.Width == ScaleIntX(16)) &&
					(sz.Height == ScaleIntY(16))), sz.ToString());
			}
		}

		public static int ScaleIntX(int i)
		{
			EnsureInitialized();
			return (int)Math.Round((double)i * m_dScaleX);
		}

		public static int ScaleIntY(int i)
		{
			EnsureInitialized();
			return (int)Math.Round((double)i * m_dScaleY);
		}

		public static Image ScaleImage(Image img, bool bForceNewObject)
		{
			if(img == null) { Debug.Assert(false); return null; }

			// EnsureInitialized(); // Done by ScaleIntX

			int w = img.Width;
			int h = img.Height;
			int sw = ScaleIntX(w);
			int sh = ScaleIntY(h);

			if((w == sw) && (h == sh) && !bForceNewObject)
				return img;

			Bitmap bmp = new Bitmap(sw, sh, img.PixelFormat);
			using(Graphics g = Graphics.FromImage(bmp))
			{
				g.SmoothingMode = SmoothingMode.HighQuality;
				g.InterpolationMode = m_im;

				RectangleF rSource = new RectangleF(0, 0, w, h);
				RectangleF rDest = new RectangleF(0, 0, sw, sh);

				// Prevent border drawing bug
				rSource.Offset(-0.5f, -0.5f);

				g.DrawImage(img, rDest, rSource, GraphicsUnit.Pixel);
			}

			return bmp;
		}

		internal static void ScaleToolStripItems(ToolStripItem[] vItems,
			int[] vWidths)
		{
			if(vItems == null) { Debug.Assert(false); return; }
			if(vWidths == null) { Debug.Assert(false); return; }
			if(vItems.Length != vWidths.Length) { Debug.Assert(false); return; }

			for(int i = 0; i < vItems.Length; ++i)
			{
				ToolStripItem tsi = vItems[i];
				if(tsi == null) { Debug.Assert(false); continue; }

				int nWidth = vWidths[i];
				int nWidthScaled = ScaleIntX(nWidth);
				Debug.Assert(nWidth >= 0);

				if(nWidth == nWidthScaled)
				{
					Debug.Assert(tsi.Width == nWidth);
					continue;
				}

				try
				{
					int w = tsi.Width;

					// .NET scales some ToolStripItems, some not
					Debug.Assert(((w == nWidth) || (w == nWidthScaled)),
						tsi.Name + ": w = " + w.ToString());

					if(Math.Abs(w - nWidth) < Math.Abs(w - nWidthScaled))
						tsi.Width = nWidthScaled;
				}
				catch(Exception) { Debug.Assert(false); }
			}
		}
	}
}
