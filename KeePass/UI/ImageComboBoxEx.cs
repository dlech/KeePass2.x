﻿/*
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
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

using KeePassLib.Native;
using KeePassLib.Utility;

namespace KeePass.UI
{
	public sealed class ImageComboBoxEx : ComboBox
	{
		private List<Image> m_vImages = null;
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public List<Image> OrderedImageList
		{
			get { return m_vImages; }
			set { m_vImages = value; } // Null allowed
		}
		public bool ShouldSerializeOrderedImageList() { return false; }

		public ImageComboBoxEx() : base()
		{
			if(Program.DesignMode) return;
			if(NativeLib.IsUnix()) return;

			Debug.Assert(this.DrawMode == DrawMode.Normal);
			this.DrawMode = DrawMode.OwnerDrawVariable;

			this.DropDownHeight = GetStdItemHeight(null) * 12 + 2;
			this.MaxDropDownItems = 12;

			Debug.Assert(!this.Sorted);
		}

		private int GetStdItemHeight(Graphics g)
		{
			if(g == null)
				return Math.Max(18, TextRenderer.MeasureText("Wg", this.Font).Height);

			return Math.Max(18, TextRenderer.MeasureText(g, "Wg", this.Font).Height);
		}

		protected override void OnMeasureItem(MeasureItemEventArgs e)
		{
			base.OnMeasureItem(e);
			e.ItemHeight = GetStdItemHeight(e.Graphics);
		}

		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			Color clrBack = e.BackColor, clrFore = e.ForeColor;
			if((e.State & DrawItemState.Selected) != DrawItemState.None)
			{
				clrBack = SystemColors.Highlight;
				clrFore = SystemColors.HighlightText;
			}

			int nIdx = e.Index;
			Rectangle rectClip = e.Bounds;
			int dImg = rectClip.Height - 2;

			// Don't use RTL property of translation, as the parent (form)
			// may explicitly turn off the RTL mode
			bool bRtl = (this.RightToLeft == RightToLeft.Yes);

			Graphics g = e.Graphics;
			using(SolidBrush brBack = new SolidBrush(clrBack))
			{
				g.FillRectangle(brBack, rectClip);
			}

			Rectangle rectImg = new Rectangle(bRtl ? (rectClip.Right - dImg - 1) :
				(rectClip.Left + 1), rectClip.Top + 1, dImg, dImg);
			if((m_vImages != null) && (nIdx >= 0) && (nIdx < m_vImages.Count) &&
				(m_vImages[nIdx] != null))
			{
				GfxUtil.SetHighQuality(g);
				g.DrawImage(m_vImages[nIdx], rectImg);
			}

			Rectangle rectText = new Rectangle(bRtl ? (rectClip.Left + 1) :
				(rectClip.Left + dImg + 2 + 1), rectClip.Top + 1,
				rectClip.Width - dImg - 5, rectClip.Height - 2);
			TextFormatFlags tff = (TextFormatFlags.PreserveGraphicsClipping |
				TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix |
				TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter);
			if(bRtl) tff |= (TextFormatFlags.RightToLeft | TextFormatFlags.Right);
			string strText = string.Empty;
			if((nIdx >= 0) && (nIdx < this.Items.Count))
				strText = ((this.Items[nIdx] as string) ?? string.Empty);
			TextRenderer.DrawText(g, strText, e.Font, rectText, clrFore, clrBack, tff);

			if(((e.State & DrawItemState.Focus) != DrawItemState.None) &&
				((e.State & DrawItemState.NoFocusRect) == DrawItemState.None))
				e.DrawFocusRectangle();
		}
	}
}
