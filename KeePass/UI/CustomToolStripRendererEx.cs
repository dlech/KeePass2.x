/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2009 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Drawing;
using System.Drawing.Drawing2D;

namespace KeePass.UI
{
	public sealed class CustomToolStripRendererEx : ToolStripProfessionalRenderer
	{
		public CustomToolStripRendererEx()
			: base(new CtsrColorTable())
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
					rect.Width, rect.Height, 2);
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
	}

	public sealed class CtsrColorTable : ProfessionalColorTable
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
}
