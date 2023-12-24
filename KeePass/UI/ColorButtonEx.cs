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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using KeePass.Resources;

namespace KeePass.UI
{
	public sealed class ColorButtonEx : Button
	{
		private Image m_img = null;
		private ToolTip m_tt = null;

		private CustomContextMenuEx m_ctx = null;
		private readonly List<ColorMenuItem> m_lMenuItems = new List<ColorMenuItem>();

		private Color[] m_vColors = null;
		[Browsable(false)]
		[DefaultValue((object)null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Color[] Colors
		{
			get { return m_vColors; }
			set { if(!Program.DesignMode) m_vColors = value; }
		}
		public bool ShouldSerializeColors() { return false; }

		private Color m_clr = Color.Empty;
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Color SelectedColor
		{
			get { return m_clr; }
			set
			{
				if(Program.DesignMode) return;

				m_clr = value;

				bool bEmpty = UIUtil.ColorsEqual(m_clr, Color.Empty);
				Debug.Assert(bEmpty || (m_vColors == null) ||
					(Array.IndexOf(m_vColors, m_clr) >= 0));

				Image imgNew = (!bEmpty ? UIUtil.CreateColorBitmap24(this,
					m_clr) : null);
				UIUtil.OverwriteButtonImage(this, ref m_img, imgNew);

				UpdateToolTipEx();
			}
		}
		public bool ShouldSerializeSelectedColor() { return false; }

		public ColorButtonEx() : base()
		{
			if(Program.DesignMode) return;

			Debug.Assert(this.ImageAlign == ContentAlignment.MiddleCenter);
			Debug.Assert(this.Text == string.Empty); // Foreground color?
			Debug.Assert(this.TextImageRelation == TextImageRelation.Overlay);

			m_tt = new ToolTip();
			UIUtil.ConfigureToolTip(m_tt);
			UpdateToolTipEx();
		}

		protected override void Dispose(bool disposing)
		{
			if(disposing)
			{
				UIUtil.DisposeButtonImage(this, ref m_img);

				if(m_tt != null)
				{
					m_tt.Dispose();
					m_tt = null;
				}

				DisposeColorMenu();
			}

			base.Dispose(disposing);
		}

		private void DisposeColorMenu()
		{
			if(m_ctx == null) return;

			foreach(ColorMenuItem mi in m_lMenuItems)
				mi.Click -= this.OnColorMenuItemClick;
			m_lMenuItems.Clear();

			m_ctx.Dispose();
			m_ctx = null;
		}

		private void ShowColorMenu()
		{
			DisposeColorMenu();

			if(m_vColors == null) { Debug.Assert(false); return; }

			int cColors = m_vColors.Length;
			if(cColors == 0) { Debug.Assert(false); return; }

			int nBreakAt = Math.Max((int)Math.Sqrt(0.1 + cColors), 1);
			int qSize = (int)((20.0f * this.Height) / 23.0f + 0.01f);

			for(int i = 0; i < cColors; ++i)
			{
				ColorMenuItem mi = new ColorMenuItem(m_vColors[i], qSize);
				mi.Click += this.OnColorMenuItemClick;

				if(((i % nBreakAt) == 0) && (i != 0))
					mi.Break = true;

				m_lMenuItems.Add(mi);
			}

			m_ctx = new CustomContextMenuEx();
			m_ctx.MenuItems.AddRange(m_lMenuItems.ToArray());
			m_ctx.ShowEx(this);
		}

		private void UpdateToolTipEx()
		{
			string str = KPRes.SelectColor + " \u2013 " + KPRes.Selected +
				": " + UIUtil.ColorToString(m_clr);
			UIUtil.SetToolTip(m_tt, this, str, true);
		}

		protected override void OnClick(EventArgs e)
		{
			// base.OnClick(e);

			if(m_vColors != null) ShowColorMenu();
			else
			{
				Color? oclr = UIUtil.ShowColorDialog(m_clr);
				if(oclr.HasValue) this.SelectedColor = oclr.Value;
			}
		}

		private void OnColorMenuItemClick(object sender, EventArgs e)
		{
			ColorMenuItem mi = (sender as ColorMenuItem);
			if(mi == null) { Debug.Assert(false); return; }

			this.SelectedColor = mi.Color;
		}
	}
}
