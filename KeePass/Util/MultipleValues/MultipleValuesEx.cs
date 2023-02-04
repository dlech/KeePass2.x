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
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

using KeePass.Resources;
using KeePass.UI;

using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.Util.MultipleValues
{
	public static class MultipleValuesEx
	{
		private static string g_strCueBase = null;

		private static string g_strCue = null;
		public static string CueString
		{
			get { UpdateCues(); return g_strCue; }
		}

		private static ProtectedString g_psCue = null;
		public static ProtectedString CueProtectedString
		{
			get { UpdateCues(); return g_psCue; }
		}

		private static ProtectedString g_psCueEx = null;
		public static ProtectedString CueProtectedStringEx
		{
			get { UpdateCues(); return g_psCueEx; }
		}

		private static void UpdateCues()
		{
			string strBase = (KPRes.MultipleValues ?? string.Empty);
			if(strBase == g_strCueBase) return;

			g_strCue = "(" + strBase + ")";

			// Ensure compatibility with tags
			List<string> l = StrUtil.StringToTags(g_strCue);
			if(l.Count >= 2)
			{
				Debug.Assert(false); // The cue should be one tag
				StringBuilder sb = new StringBuilder();
				foreach(string str in l) sb.Append(str);
				g_strCue = sb.ToString();
			}

			g_psCue = new ProtectedString(false, g_strCue);
			g_psCueEx = new ProtectedString(true, g_strCue);

			g_strCueBase = strBase;
		}

		internal static Image CreateMultiImage(Size? osz)
		{
			Size sz = (osz.HasValue ? osz.Value : UIUtil.GetSmallIconSize());
			int w = sz.Width, h = sz.Height;

			Color clrGray = Color.FromArgb(255, 128, 128, 128);
			Image img = UIUtil.CreateColorBitmap24(w, h, clrGray);

			if((w < 8) || (h < 8)) return img;

			try
			{
				using(Graphics g = Graphics.FromImage(img))
				{
					Color clrBg = (UIUtil.IsDarkColor(SystemColors.ControlText) ?
						Color.White : Color.Black);
					int d = Math.Min(w, h) / 8;

					// using(SolidBrush brBg = new SolidBrush(clrBg))
					// {
					//	g.FillRectangle(brBg, d, d, w - (d << 1), h - (d << 1));
					// }
					// using(SolidBrush brGray = new SolidBrush(clrGray))
					// {
					//	g.FillRectangle(brGray, d << 1, d << 1, w - (d << 2),
					//		h - (d << 2));
					// }

					using(Pen pen = new Pen(clrBg))
					{
						g.SmoothingMode = SmoothingMode.None;

						Point[] v = new Point[5];
						for(int i = 0; i < d; ++i)
						{
							int pTL = d + i;
							int xR = w - 1 - pTL, yB = h - 1 - pTL;

							v[0] = new Point(pTL, pTL);
							v[1] = new Point(xR, pTL);
							v[2] = new Point(xR, yB);
							v[3] = new Point(pTL, yB);
							v[4] = v[0];

							g.DrawLines(pen, v);
						}
					}
				}
			}
			catch(Exception) { Debug.Assert(false); }

			return img;
		}

		internal static void ConfigureText(Control c, bool bHandleTextChange)
		{
			if(c == null) { Debug.Assert(false); return; }

			Color clrNormal = c.ForeColor;
			Color clrMulti = UIUtil.ColorTowards(clrNormal, (UIUtil.IsDarkColor(
				clrNormal) ? Color.White : Color.Black), 0.5);

			EventHandler eh = delegate(object sender, EventArgs e)
			{
				Control cEv = (sender as Control);
				if(cEv == null) { Debug.Assert(false); return; }
				Debug.Assert(cEv == c);

				bool bCue = (cEv.Text == MultipleValuesEx.CueString);
				cEv.ForeColor = (bCue ? clrMulti : clrNormal);
			};

			eh(c, EventArgs.Empty);

			if(bHandleTextChange) c.TextChanged += eh;
		}

		internal static void ConfigureText(ListViewItem lvi, int iSubItem)
		{
			if(lvi == null) { Debug.Assert(false); return; }
			if((iSubItem < 0) || (iSubItem >= lvi.SubItems.Count)) { Debug.Assert(false); return; }

			ListViewItem.ListViewSubItem lvsi = lvi.SubItems[iSubItem];
			if(lvsi.Text == MultipleValuesEx.CueString)
			{
				Color clrNormal = lvi.ForeColor;
				Color clrMulti = UIUtil.ColorTowards(clrNormal, (UIUtil.IsDarkColor(
					clrNormal) ? Color.White : Color.Black), 0.5);
				Debug.Assert(UIUtil.ColorsEqual(clrNormal, SystemColors.ControlText));

				Debug.Assert(lvi.UseItemStyleForSubItems); // Caller uses colors already?
				lvi.UseItemStyleForSubItems = false;
				lvsi.ForeColor = clrMulti;
			}
		}

		internal static void ConfigureState(CheckBox cb, bool bSetIndeterminate)
		{
			if(cb == null) { Debug.Assert(false); return; }

			// If the indeterminate state is used to represent multiple
			// values, the caller should not use it for other things
			Debug.Assert(!cb.ThreeState);

			cb.ThreeState = true;

			if(bSetIndeterminate) cb.CheckState = CheckState.Indeterminate;
		}
	}
}
