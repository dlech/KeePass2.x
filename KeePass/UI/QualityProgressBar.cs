/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2012 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Drawing;
using System.Drawing.Drawing2D;

namespace KeePass.UI
{
	public class QualityProgressBar : Control
	{
		private int m_nMinimum = 0, m_nMaximum = 100, m_nPosition = 0;
		private ProgressBarStyle m_pbsStyle = ProgressBarStyle.Continuous;

		public QualityProgressBar() : base()
		{
			this.DoubleBuffered = true;
		}

		public int Minimum
		{
			get { return m_nMinimum; }
			set { m_nMinimum = value; this.Invalidate(); }
		}

		public int Maximum
		{
			get { return m_nMaximum; }
			set { m_nMaximum = value; this.Invalidate(); }
		}

		public int Value
		{
			get { return m_nPosition; }
			set { m_nPosition = value; this.Invalidate(); }
		}

		public ProgressBarStyle Style
		{
			get { return m_pbsStyle; }
			set { m_pbsStyle = value; }
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			if(g == null) { base.OnPaint(e); return; }

			int nNormalizedPos = m_nPosition - m_nMinimum;
			int nNormalizedMax = m_nMaximum - m_nMinimum;

			Rectangle rectClient = this.ClientRectangle;
			Rectangle rectDraw;
			if(VisualStyleRenderer.IsSupported)
			{
				VisualStyleRenderer vsr = new VisualStyleRenderer(
					VisualStyleElement.ProgressBar.Bar.Normal);
				vsr.DrawBackground(g, rectClient);

				rectDraw = vsr.GetBackgroundContentRectangle(g, rectClient);
			}
			else
			{
				g.FillRectangle(SystemBrushes.Control, rectClient);

				g.DrawLine(Pens.Gray, 0, 0, rectClient.Width - 1, 0);
				g.DrawLine(Pens.Gray, 0, 0, 0, rectClient.Height - 1);
				g.DrawLine(Pens.White, rectClient.Width - 1, 0,
					rectClient.Width - 1, rectClient.Height - 1);
				g.DrawLine(Pens.White, 0, rectClient.Height - 1,
					rectClient.Width - 1, rectClient.Height - 1);

				rectDraw = new Rectangle(rectClient.X + 1, rectClient.Y + 1,
					rectClient.Width - 2, rectClient.Height - 2);
			}

			Rectangle rectGradient = new Rectangle(rectDraw.X, rectDraw.Y,
				rectDraw.Width, rectDraw.Height);
			if((rectGradient.Width & 1) == 0) ++rectGradient.Width;

			int nDrawWidth = (int)((float)rectDraw.Width * ((float)nNormalizedPos /
				(float)nNormalizedMax));

			Color clrStart = Color.FromArgb(255, 128, 0);
			Color clrEnd = Color.FromArgb(0, 255, 0);
			if(!this.Enabled)
			{
				clrStart = UIUtil.ColorToGrayscale(SystemColors.ControlDark);
				clrEnd = UIUtil.ColorToGrayscale(SystemColors.Control);
			}

			bool bRtl = (this.RightToLeft == RightToLeft.Yes);
			if(bRtl)
			{
				Color clrTemp = clrStart;
				clrStart = clrEnd;
				clrEnd = clrTemp;
			}
			
			using(LinearGradientBrush brush = new LinearGradientBrush(rectGradient,
				clrStart, clrEnd, LinearGradientMode.Horizontal))
			{
				g.FillRectangle(brush, (bRtl ? (rectDraw.Width - nDrawWidth + 1) :
					rectDraw.Left), rectDraw.Top, nDrawWidth, rectDraw.Height);
			}
		}

		protected override void OnPaintBackground(PaintEventArgs pEvent)
		{
			// base.OnPaintBackground(pevent);
		}
	}
}
