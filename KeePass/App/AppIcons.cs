/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2023 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text;

using KeePass.UI;

namespace KeePass.App
{
	public enum AppIconType
	{
		None = 0,
		Main,
		QuadNormal,
		QuadLocked
	}

	public static class AppIcons
	{
		private static readonly Dictionary<string, Icon> g_dCache = new Dictionary<string, Icon>();
		private static readonly object g_oCacheSync = new object();

		private const int g_hMain = 225; // Average hue of the main icon in degrees
		private static readonly Color g_clrMain = UIUtil.ColorFromHsv(g_hMain, 1, 1);

		private static Color[] g_vColors = null;
		internal static Color[] Colors
		{
			get
			{
				if(g_vColors == null)
				{
					List<Color> l = new List<Color>();

					for(int h = 0; h < 360; h += 15)
						l.Add(UIUtil.ColorFromHsv((h + g_hMain) % 360, 1, 1));

					g_vColors = l.ToArray();

#if DEBUG
					Color[] vInvariant = new Color[] { g_clrMain,
						Color.Red, Color.Lime, Color.Blue, Color.Yellow };
					foreach(Color clr in vInvariant)
					{
						Debug.Assert(UIUtil.ColorsEqual(RoundColor(clr), clr));
					}
#endif
				}

				return g_vColors;
			}
		}

		public static Icon Default
		{
			get { return Get(AppIconType.Main, Size.Empty, Color.Empty); }
		}

		public static Icon Get(AppIconType t, Size sz, Color clr)
		{
			int w = Math.Min(Math.Max(sz.Width, 0), 256);
			int h = Math.Min(Math.Max(sz.Height, 0), 256);
			if((w == 0) || (h == 0))
			{
				Size szDefault = UIUtil.GetIconSize();
				w = szDefault.Width;
				h = szDefault.Height;
			}

			Color c = clr;
			if(!UIUtil.ColorsEqual(c, Color.Empty)) c = RoundColor(c);

			NumberFormatInfo nf = NumberFormatInfo.InvariantInfo;
			string strID = ((long)t).ToString(nf) + ":" + w.ToString(nf) + ":" +
				h.ToString(nf) + ":" + c.ToArgb().ToString(nf);

			Icon ico = null;
			lock(g_oCacheSync)
			{
				if(g_dCache.TryGetValue(strID, out ico)) return ico;
			}

			if(t == AppIconType.Main)
				ico = Properties.Resources.KeePass;
			else if(t == AppIconType.QuadNormal)
				ico = Properties.Resources.QuadNormal;
			else if(t == AppIconType.QuadLocked)
			{
				ico = Properties.Resources.QuadLocked;

				Debug.Assert(UIUtil.ColorsEqual(c, Color.Empty));
				c = Color.Empty; // This icon should not be recolored
			}
			else { Debug.Assert(false); }

			if((ico != null) && !UIUtil.ColorsEqual(c, Color.Empty))
				ico = IconColorizer.Recolor(ico, c);

			// Select requested resolution
			if(ico != null) ico = new Icon(ico, w, h); // Preserves icon data

			Debug.Assert(ico != null);
			lock(g_oCacheSync) { g_dCache[strID] = ico; }
			return ico;
		}

		private static int ColorDist(Color c1, Color c2)
		{
			int dR = (int)c1.R - (int)c2.R;
			int dG = (int)c1.G - (int)c2.G;
			int dB = (int)c1.B - (int)c2.B;
			return ((dR * dR) + (dG * dG) + (dB * dB));
		}

		/// <summary>
		/// Round <paramref name="clr" /> to the nearest supported color.
		/// </summary>
		public static Color RoundColor(Color clr)
		{
			Debug.Assert(!UIUtil.ColorsEqual(clr, Color.Empty));
			if((clr.R == clr.B) && (clr.G == clr.B))
				return g_clrMain; // Gray => default

			Color[] v = AppIcons.Colors;

			int c = clr.ToArgb();
			for(int i = 0; i < v.Length; ++i)
			{
				if(v[i].ToArgb() == c) return clr;
			}

			int iMin = 0, dMin = int.MaxValue;
			for(int i = 0; i < v.Length; ++i)
			{
				int d = ColorDist(clr, v[i]);
				if(d < dMin)
				{
					iMin = i;
					dMin = d;
				}
			}
			return v[iMin];
		}
	}
}
