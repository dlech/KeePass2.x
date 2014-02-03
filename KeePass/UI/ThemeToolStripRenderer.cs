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

/*
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Drawing;
using System.Diagnostics;

// Parts and states:
// http://msdn.microsoft.com/en-us/library/bb773210%28VS.85%29.aspx
// How to Implement a Custom ToolStripRenderer:
// http://msdn.microsoft.com/en-us/library/ms229720.aspx

namespace KeePass.UI
{
	public sealed class ThemeToolStripRenderer : ToolStripSystemRenderer
	{
		private const string VsClassMenu = "MENU";

		private enum VsMenuPart : int
		{
			MenuItemTmSchema = 1,
			MenuDropDownTmSchema = 2,
			MenuBarItemTmSchema = 3,
			MenuBarDropDownTmSchema = 4,
			ChevronTmSchema = 5,
			SeparatorTmSchema = 6,
			BarBackground = 7,
			BarItem = 8,
			PopupBackground = 9,
			PopupBorders = 10,
			PopupCheck = 11,
			PopupCheckBackground = 12,
			PopupGutter = 13,
			PopupItem = 14,
			PopupSeparator = 15,
			PopupSubMenu = 16,
			SystemClose = 17,
			SystemMaximize = 18,
			SystemMinimize = 19,
			SystemRestore = 20
		}

		private enum VsMenuState : int
		{
			Default = 0,

			MbActive = 1,
			MbInactive = 2,

			MbiNormal = 1,
			MbiHot = 2,
			MbiPushed = 3,
			MbiDisabled = 4,
			MbiDisabledHot = 5,
			MbiDisabledPushed = 6,

			McCheckMarkNormal = 1,
			McCheckMarkDisabled = 2,
			McBulletNormal = 3,
			McBulletDisabled = 4,

			McbDisabled = 1,
			McbNormal = 2,
			McbBitmap = 3,

			MpiNormal = 1,
			MpiHot = 2,
			MpiDisabled = 3,
			MpiDisabledHot = 4,

			MsmNormal = 1,
			MsmDisabled = 2
		};

		private VisualStyleRenderer m_renderer = null;

		public ThemeToolStripRenderer() : base()
		{
			m_renderer = new VisualStyleRenderer(VsClassMenu,
				(int)VsMenuPart.BarBackground, (int)VsMenuState.MbActive);
		}

		public static void AttachTo(ToolStrip ts)
		{
			if(ts == null) throw new ArgumentNullException("ts");

			if(!VisualStyleRenderer.IsSupported) return;

			try { ts.Renderer = new ThemeToolStripRenderer(); }
			catch(Exception) { Debug.Assert(false); }
		}

		private static VsMenuState GetItemState(ToolStripItem tsi)
		{
			bool bEnabled = tsi.Enabled;
			bool bPressed = tsi.Pressed;
			bool bHot = tsi.Selected;

			if(tsi.Owner.IsDropDown)
			{
				if(bEnabled)
					return (bHot ? VsMenuState.MpiHot : VsMenuState.MpiNormal);

				return (bHot ? VsMenuState.MpiDisabledHot : VsMenuState.MpiDisabled);
			}
			else
			{
				if(tsi.Pressed)
					return (bEnabled ? VsMenuState.MbiPushed : VsMenuState.MbiDisabledPushed);
	
				if(bEnabled)
					return (bHot ? VsMenuState.MbiHot : VsMenuState.MbiNormal);

				return (bHot ? VsMenuState.MbiDisabledHot : VsMenuState.MbiDisabled);
			}
		}

		protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
		{
			base.OnRenderArrow(e);
		}

		protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
		{
			base.OnRenderButtonBackground(e);
		}

		protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e)
		{
			base.OnRenderDropDownButtonBackground(e);
		}

		protected override void OnRenderGrip(ToolStripGripRenderEventArgs e)
		{
			base.OnRenderGrip(e);
		}

		protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
		{
			base.OnRenderImageMargin(e);
		}

		protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
		{
			base.OnRenderItemCheck(e);
		}

		protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
		{
			base.OnRenderItemImage(e);
		}

		protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
		{
			base.OnRenderItemText(e);
		}

		protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
		{
			m_renderer.SetParameters(VsClassMenu, (int)(e.Item.Owner.IsDropDown ?
				VsMenuPart.PopupItem : VsMenuPart.BarItem), (int)GetItemState(e.Item));

			Rectangle rc = e.Item.ContentRectangle;
			if(!e.Item.Owner.IsDropDown) rc.Inflate(-1, 0);

			m_renderer.DrawBackground(e.Graphics, rc);
		}

		protected override void OnRenderOverflowButtonBackground(ToolStripItemRenderEventArgs e)
		{
			base.OnRenderOverflowButtonBackground(e);
		}

		protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
		{
			base.OnRenderSeparator(e);
		}

		protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
		{
			m_renderer.SetParameters(VsClassMenu, (int)VsMenuPart.PopupBackground, 0);
			m_renderer.DrawBackground(e.Graphics, e.AffectedBounds, e.AffectedBounds);
		}

		protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
		{
			m_renderer.SetParameters(VsClassMenu, (int)VsMenuPart.PopupBorders, 0);

			if(e.ToolStrip.IsDropDown)
			{
				Region rOrgClip = e.Graphics.Clip.Clone();
				e.Graphics.ExcludeClip(m_renderer.GetBackgroundRegion(e.Graphics,
					e.ToolStrip.ClientRectangle));
				m_renderer.DrawBackground(e.Graphics, e.ToolStrip.ClientRectangle, e.AffectedBounds);
				e.Graphics.Clip = rOrgClip;
			}
		}

		protected override void OnRenderToolStripContentPanelBackground(ToolStripContentPanelRenderEventArgs e)
		{
			base.OnRenderToolStripContentPanelBackground(e);
		}

		protected override void OnRenderToolStripPanelBackground(ToolStripPanelRenderEventArgs e)
		{
			base.OnRenderToolStripPanelBackground(e);
		}
	}
}
*/
