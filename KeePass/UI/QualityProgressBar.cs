/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2021 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

using KeePass.App;
using KeePass.Util;

using KeePassLib.Native;

namespace KeePass.UI
{
	public sealed class QualityProgressBar : Control
	{
		public QualityProgressBar() : base()
		{
			if(Program.DesignMode) return;

			this.DoubleBuffered = true;
		}

		private int m_nMinimum = 0;
		[DefaultValue(0)]
		public int Minimum
		{
			get { return m_nMinimum; }
			set { m_nMinimum = value; Invalidate(); }
		}

		private int m_nMaximum = 100;
		[DefaultValue(100)]
		public int Maximum
		{
			get { return m_nMaximum; }
			set { m_nMaximum = value; Invalidate(); }
		}

		private int m_nPosition = 0;
		[DefaultValue(0)]
		public int Value
		{
			get { return m_nPosition; }
			set { m_nPosition = value; Invalidate(); }
		}

		private ProgressBarStyle m_pbsStyle = ProgressBarStyle.Continuous;
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ProgressBarStyle Style
		{
			get { return m_pbsStyle; }
			set { m_pbsStyle = value; Invalidate(); }
		}
		public bool ShouldSerializeStyle() { return false; }

		private string m_strText = string.Empty;
		[DefaultValue("")]
		public string ProgressText
		{
			get { return m_strText; }
			set
			{
				m_strText = value;
				UIUtil.AccSetName(this, value ?? string.Empty);
				Invalidate();
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			try { PaintPriv(e); }
			catch(Exception) { Debug.Assert(false); }
		}

		private void PaintPriv(PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			if(g == null) { base.OnPaint(e); return; }

			int nNormPos = m_nPosition - m_nMinimum;
			int nNormMax = m_nMaximum - m_nMinimum;
			if(nNormMax <= 0) { Debug.Assert(false); nNormMax = 100; }
			if(nNormPos < 0) { Debug.Assert(false); nNormPos = 0; }
			if(nNormPos > nNormMax) { Debug.Assert(false); nNormPos = nNormMax; }

			Rectangle rectClient = this.ClientRectangle;
			Rectangle rectDraw;
			VisualStyleElement vse = VisualStyleElement.ProgressBar.Bar.Normal;
			if(VisualStyleRenderer.IsSupported &&
				VisualStyleRenderer.IsElementDefined(vse))
			{
				VisualStyleRenderer vsr = new VisualStyleRenderer(vse);

				if(vsr.IsBackgroundPartiallyTransparent())
					vsr.DrawParentBackground(g, rectClient, this);

				vsr.DrawBackground(g, rectClient);

				rectDraw = vsr.GetBackgroundContentRectangle(g, rectClient);
			}
			else
			{
				g.FillRectangle(SystemBrushes.Control, rectClient);

				Pen penGray = SystemPens.ControlDark;
				Pen penWhite = SystemPens.ControlLight;
				g.DrawLine(penGray, 0, 0, rectClient.Width - 1, 0);
				g.DrawLine(penGray, 0, 0, 0, rectClient.Height - 1);
				g.DrawLine(penWhite, rectClient.Width - 1, 0,
					rectClient.Width - 1, rectClient.Height - 1);
				g.DrawLine(penWhite, 0, rectClient.Height - 1,
					rectClient.Width - 1, rectClient.Height - 1);

				rectDraw = new Rectangle(rectClient.X + 1, rectClient.Y + 1,
					rectClient.Width - 2, rectClient.Height - 2);
			}

			int nDrawWidth = (int)((float)rectDraw.Width * (float)nNormPos /
				(float)nNormMax);

			Color clrStart = AppDefs.ColorQualityLow;
			Color clrEnd = AppDefs.ColorQualityHigh;
			Color clrMid = AppDefs.ColorQualityMid;
			if(!this.Enabled)
			{
				clrStart = UIUtil.ColorToGrayscale(SystemColors.ControlDark);
				clrEnd = UIUtil.ColorToGrayscale(SystemColors.ControlLight);
				clrMid = UIUtil.ColorMiddle(clrStart, clrEnd);
			}

			bool bRtl = (this.RightToLeft == RightToLeft.Yes);
			if(bRtl)
			{
				Color clrTemp = clrStart;
				clrStart = clrEnd;
				clrEnd = clrTemp;
			}

			// Workaround for Windows <= XP
			Rectangle rectGrad = new Rectangle(rectDraw.X, rectDraw.Y,
				rectDraw.Width, rectDraw.Height);
			if(!WinUtil.IsAtLeastWindowsVista && !NativeLib.IsUnix())
				rectGrad.Inflate(1, 0);

			using(LinearGradientBrush brush = new LinearGradientBrush(rectGrad,
				clrStart, clrEnd, LinearGradientMode.Horizontal))
			{
				ColorBlend cb = new ColorBlend();
				cb.Colors = new Color[3] { clrStart, clrMid, clrEnd };
				cb.Positions = new float[3] { 0.0f, 0.5f, 1.0f };
				brush.InterpolationColors = cb;

				g.FillRectangle(brush, (bRtl ? (rectDraw.Width - nDrawWidth + 1) :
					rectDraw.Left), rectDraw.Top, nDrawWidth, rectDraw.Height);
			}

			PaintText(g, rectDraw, bRtl);
		}

		private void PaintText(Graphics g, Rectangle rectDraw, bool bRtl)
		{
			if(string.IsNullOrEmpty(m_strText)) return;

			Font f = (FontUtil.DefaultFont ?? this.Font);
			Color clrFG = UIUtil.ColorToGrayscale(this.ForeColor);
			Color clrBG = Color.FromArgb(clrFG.ToArgb() ^ 0x20FFFFFF);

			int dx = rectDraw.X;
			int dy = rectDraw.Y;
			int dw = rectDraw.Width;
			int dh = rectDraw.Height;

			if(!NativeLib.IsUnix() || !UIUtil.IsDarkColor(clrFG))
			{
				Rectangle rectGlow = rectDraw;
				rectGlow.Width = TextRenderer.MeasureText(g, m_strText, f).Width;
				rectGlow.X = ((dw - rectGlow.Width) / 2) + dx;

				// Instead of an ellipse, Mono draws a circle
				if(NativeLib.IsUnix())
					rectGlow.Inflate(rectGlow.Width * 2, rectGlow.Height * 2);
				else
					rectGlow.Inflate(rectGlow.Width / 2, rectGlow.Height / 2);

				using(GraphicsPath gpGlow = new GraphicsPath())
				{
					gpGlow.AddEllipse(rectGlow);

					using(PathGradientBrush pgbGlow = new PathGradientBrush(gpGlow))
					{
						pgbGlow.CenterPoint = new PointF((dw / 2.0f) + dx,
							(dh / 2.0f) + dy);
						pgbGlow.CenterColor = clrBG;
						pgbGlow.SurroundColors = new Color[] { Color.Transparent };

						Region rgOrgClip = g.Clip;
						g.SetClip(rectDraw);
						g.FillPath(pgbGlow, gpGlow);
						g.Clip = rgOrgClip;
					}
				}
			}

			// With ClearType on, text drawn using Graphics.DrawString
			// looks better than TextRenderer.DrawText;
			// https://sourceforge.net/p/keepass/discussion/329220/thread/06ef4466/
			// TextFormatFlags tff = (TextFormatFlags.HorizontalCenter | TextFormatFlags.SingleLine |
			//	TextFormatFlags.VerticalCenter);
			// TextRenderer.DrawText(g, m_strText, f, rectDraw, clrFG, tff);

			using(SolidBrush br = new SolidBrush(clrFG))
			{
				StringFormatFlags sff = (StringFormatFlags.FitBlackBox |
					StringFormatFlags.NoClip);
				if(bRtl) sff |= StringFormatFlags.DirectionRightToLeft;

				using(StringFormat sf = new StringFormat(sff))
				{
					sf.Alignment = StringAlignment.Center;
					sf.LineAlignment = StringAlignment.Center;

					RectangleF rf = new RectangleF(dx, dy, dw, dh);
					g.DrawString(m_strText, f, br, rf, sf);
				}
			}
		}

		protected override void OnPaintBackground(PaintEventArgs pevent)
		{
			// base.OnPaintBackground(pevent);
		}
	}
}
