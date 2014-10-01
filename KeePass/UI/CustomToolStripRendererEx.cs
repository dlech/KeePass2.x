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
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace KeePass.UI
{
	public sealed class CustomToolStripRendererEx : ToolStripProfessionalRenderer
	{
		private sealed class CtsrColorTable : ProfessionalColorTable
		{
			private const double m_dblLight = 0.75;
			private const double m_dblDark = 0.05;

			internal static Color StartGradient(Color clr)
			{
				return UIUtil.LightenColor(clr, m_dblLight);
			}

			internal static Color EndGradient(Color clr)
			{
				return UIUtil.DarkenColor(clr, m_dblDark);
			}

			public override Color ButtonPressedGradientBegin
			{
				get { return StartGradient(this.ButtonPressedGradientMiddle); }
			}

			public override Color ButtonPressedGradientEnd
			{
				get { return EndGradient(this.ButtonPressedGradientMiddle); }
			}

			public override Color ButtonSelectedGradientBegin
			{
				get { return StartGradient(this.ButtonSelectedGradientMiddle); }
			}

			public override Color ButtonSelectedGradientEnd
			{
				get { return EndGradient(this.ButtonSelectedGradientMiddle); }
			}

			public override Color ImageMarginGradientBegin
			{
				get { return StartGradient(this.ImageMarginGradientMiddle); }
			}

			public override Color ImageMarginGradientEnd
			{
				get { return EndGradient(this.ImageMarginGradientMiddle); }
			}

			/* public override Color MenuItemPressedGradientBegin
			{
				get { return StartGradient(this.MenuItemPressedGradientMiddle); }
			}

			public override Color MenuItemPressedGradientEnd
			{
				get { return EndGradient(this.MenuItemPressedGradientMiddle); }
			} */

			public override Color MenuItemSelectedGradientBegin
			{
				get { return StartGradient(this.MenuItemSelected); }
			}

			public override Color MenuItemSelectedGradientEnd
			{
				get { return EndGradient(this.MenuItemSelected); }
			}
		}

		public CustomToolStripRendererEx() : base(new CtsrColorTable())
		{
		}

		protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
		{
			if(((e.Item.Owner is ContextMenuStrip) || (e.Item.OwnerItem != null)) &&
				e.Item.Selected)
			{
				Rectangle rect = e.Item.ContentRectangle;
				rect.Offset(0, -1);
				rect.Height += 1;

				Color clrStart = CtsrColorTable.StartGradient(this.ColorTable.MenuItemSelected);
				Color clrEnd = CtsrColorTable.EndGradient(this.ColorTable.MenuItemSelected);
				Color clrBorder = this.ColorTable.MenuItemBorder;

				if(!e.Item.Enabled)
				{
					Color clrBase = this.ColorTable.MenuStripGradientEnd;
					clrStart = UIUtil.ColorTowardsGrayscale(clrStart, clrBase, 0.5);
					clrEnd = UIUtil.ColorTowardsGrayscale(clrEnd, clrBase, 0.2);
					clrBorder = UIUtil.ColorTowardsGrayscale(clrBorder, clrBase, 0.2);
				}

				LinearGradientBrush br = new LinearGradientBrush(rect,
					clrStart, clrEnd, LinearGradientMode.Vertical);
				Pen p = new Pen(clrBorder);

				SmoothingMode smOrg = e.Graphics.SmoothingMode;
				e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
				
				GraphicsPath gp = UIUtil.CreateRoundedRectangle(rect.X, rect.Y,
					rect.Width, rect.Height, DpiUtil.ScaleIntY(2));
				if(gp != null)
				{
					e.Graphics.FillPath(br, gp);
					e.Graphics.DrawPath(p, gp);
					gp.Dispose();
				}
				else // Shouldn't ever happen...
				{
					e.Graphics.FillRectangle(br, rect);
					e.Graphics.DrawRectangle(p, rect);
				}

				e.Graphics.SmoothingMode = smOrg;

				p.Dispose();
				br.Dispose();
			}
			else base.OnRenderMenuItemBackground(e);
		}

		protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
		{
			Image imgToDispose = null;
			try
			{
				Graphics g = e.Graphics;
				Image imgOrg = e.Image;
				Rectangle rOrg = e.ImageRectangle;
				ToolStripItem tsi = e.Item;

				Image img = imgOrg;
				Rectangle r = rOrg;
				Debug.Assert(r.Width == r.Height);
				Debug.Assert(DpiUtil.ScalingRequired || (r.Size ==
					((img != null) ? img.Size : new Size(3, 5))));

				// Override the .NET checkmark bitmap
				ToolStripMenuItem tsmi = (tsi as ToolStripMenuItem);
				if((tsmi != null) && tsmi.Checked && (tsmi.Image == null))
					img = Properties.Resources.B16x16_MenuCheck;

				if(tsi != null)
				{
					Rectangle rContent = tsi.ContentRectangle;
					Debug.Assert(rContent.Contains(r) || DpiUtil.ScalingRequired);
					r.Intersect(rContent);
					if(r.Height < r.Width) r.Width = r.Height;
				}
				else { Debug.Assert(false); }

				if((img != null) && (r.Size != img.Size))
				{
					img = UIUtil.CreateScaledImage(img, r.Width, r.Height);
					imgToDispose = img;
				}

				if((img != imgOrg) || (r != rOrg))
				{
					ToolStripItemImageRenderEventArgs eNew =
						new ToolStripItemImageRenderEventArgs(g, tsi, img, r);
					base.OnRenderItemCheck(eNew);
					return;
				}

				/* ToolStripMenuItem tsmi = (tsi as ToolStripMenuItem);
				if((tsmi != null) && tsmi.Checked && (r.Width > 0) &&
					(r.Height > 0) && (img != null) &&
					((img.Width != r.Width) || (img.Height != r.Height)))
				{
					Rectangle rContent = tsmi.ContentRectangle;
					r.Intersect(rContent);
					if(r.Height < r.Width)
						r.Width = r.Height;

					ProfessionalColorTable ct = this.ColorTable;

					Color clrBorder = ct.ButtonSelectedBorder;

					Color clrBG = ct.CheckBackground;
					if(tsmi.Selected) clrBG = ct.CheckSelectedBackground;
					if(tsmi.Pressed) clrBG = ct.CheckPressedBackground;

					Color clrFG = ((UIUtil.ColorToGrayscale(clrBG).R >= 128) ?
						Color.Black : Color.White);

					using(SolidBrush sb = new SolidBrush(clrBG))
					{
						g.FillRectangle(sb, r);
					}
					using(Pen p = new Pen(clrBorder))
					{
						g.DrawRectangle(p, r.X, r.Y, r.Width - 1, r.Height - 1);
					}

					ControlPaint.DrawMenuGlyph(g, r, MenuGlyph.Checkmark,
						clrFG, Color.Transparent);

					// if((img.Width == r.Width) && (img.Height == r.Height))
					//	g.DrawImage(img, r);
					// else
					// {
					//	Image imgScaled = UIUtil.CreateScaledImage(img,
					//		r.Width, r.Height);
					//	g.DrawImage(imgScaled, r);
					//	imgScaled.Dispose();
					// }

					return;
				} */

				/* if((img != null) && (r.Width > 0) && (r.Height > 0) &&
					((img.Width != r.Width) || (img.Height != r.Height)) &&
					(tsi != null))
				{
					// This should only happen on high DPI
					Debug.Assert(DpiUtil.ScalingRequired);

					Image imgScaled = UIUtil.CreateScaledImage(img,
						r.Width, r.Height);
					// Image imgScaled = new Bitmap(r.Width, r.Height,
					//	PixelFormat.Format32bppArgb);
					// using(Graphics gScaled = Graphics.FromImage(imgScaled))
					// {
					//	gScaled.Clear(Color.Transparent);

					//	Color clrFG = ((UIUtil.ColorToGrayscale(
					//		this.ColorTable.CheckBackground).R >= 128) ?
					//		Color.FromArgb(18, 24, 163) : Color.White);

					//	Rectangle rGlyph = new Rectangle(0, 0, r.Width, r.Height);
					//	// rGlyph.Inflate(-r.Width / 12, -r.Height / 12);

					//	ControlPaint.DrawMenuGlyph(gScaled, rGlyph,
					//		MenuGlyph.Bullet, clrFG, Color.Transparent);
					// }

					ToolStripItemImageRenderEventArgs eMod =
						new ToolStripItemImageRenderEventArgs(g, e.Item,
							imgScaled, r);
					base.OnRenderItemCheck(eMod);

					imgScaled.Dispose();
					return;
				} */
			}
			catch(Exception) { Debug.Assert(false); }
			finally
			{
				if(imgToDispose != null) imgToDispose.Dispose();
			}

			base.OnRenderItemCheck(e);
		}
	}
}
