/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2019 Dominik Reichl <dominik.reichl@t-online.de>

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
		private static readonly Color[] g_vColors = new Color[] {
			// Default should be first
			Color.Blue, Color.Red, Color.Lime, Color.Yellow

			// Color.Green.G == 128, Color.Lime.G == 255
		};
		internal static Color[] Colors { get { return g_vColors; } }

		private static Dictionary<string, Icon> g_dCache = new Dictionary<string, Icon>();
		private static readonly object g_oCacheSync = new object();

		public static Icon Default
		{
			get { return Get(AppIconType.Main, Size.Empty, g_vColors[0]); }
		}

		public static Icon Get(AppIconType t, Size sz, Color clr)
		{
			int w = Math.Min(Math.Max(sz.Width, 0), 256);
			int h = Math.Min(Math.Max(sz.Height, 0), 256);
			if((w == 0) || (h == 0))
			{
				Size szDef = UIUtil.GetIconSize();
				w = szDef.Width;
				h = szDef.Height;
			}

			Color c = RoundColor(clr);

			NumberFormatInfo nf = NumberFormatInfo.InvariantInfo;
			string strID = ((long)t).ToString(nf) + ":" + w.ToString(nf) + ":" +
				h.ToString(nf) + ":" + c.ToArgb().ToString(nf);

			Icon ico = null;
			lock(g_oCacheSync)
			{
				if(g_dCache.TryGetValue(strID, out ico)) return ico;
			}

			if(t == AppIconType.Main)
			{
				if(c == Color.Red)
					ico = Properties.Resources.KeePass_R;
				else if(c == Color.Lime)
					ico = Properties.Resources.KeePass_G;
				else if(c == Color.Yellow)
					ico = Properties.Resources.KeePass_Y;
				else
				{
					Debug.Assert(c == Color.Blue);
					ico = Properties.Resources.KeePass;
				}
			}
			else if(t == AppIconType.QuadNormal)
			{
				if(c == Color.Red)
					ico = Properties.Resources.QuadNormal_R;
				else if(c == Color.Lime)
					ico = Properties.Resources.QuadNormal_G;
				else if(c == Color.Yellow)
					ico = Properties.Resources.QuadNormal_Y;
				else
				{
					Debug.Assert(c == Color.Blue);
					ico = Properties.Resources.QuadNormal;
				}
			}
			else if(t == AppIconType.QuadLocked)
			{
				Debug.Assert(c == Color.Blue);
				ico = Properties.Resources.QuadLocked;
			}
			else { Debug.Assert(false); }

			Icon icoSc = null;
			if(ico != null) icoSc = new Icon(ico, w, h); // Preserves icon data
			else { Debug.Assert(false); }

			lock(g_oCacheSync) { g_dCache[strID] = icoSc; }
			return icoSc;
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
			int c = clr.ToArgb(); // Color name is irrelevant
			for(int i = 0; i < g_vColors.Length; ++i)
			{
				if(g_vColors[i].ToArgb() == c) return g_vColors[i]; // With name
			}

			if((clr.R == clr.B) && (clr.G == clr.B))
				return g_vColors[0]; // Gray => default

			int iColor = 0;
			int dMin = int.MaxValue;
			for(int i = 0; i < g_vColors.Length; ++i)
			{
				int d = ColorDist(clr, g_vColors[i]);
				if(d < dMin)
				{
					iColor = i;
					dMin = d;
				}
			}

			return g_vColors[iColor];
		}
	}
}
