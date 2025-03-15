/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2025 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Text;
using System.Windows.Forms;

using KeePass.Resources;

using KeePassLib;

namespace KeePass.UI.ToolStripRendering
{
	internal sealed class SystemTsrFactory : TsrFactory
	{
		private readonly PwUuid m_uuid = new PwUuid(new byte[] {
			0x6B, 0xCD, 0x45, 0xFA, 0xA1, 0x3F, 0x71, 0xEC,
			0x7B, 0x5E, 0x97, 0x38, 0x8D, 0xB1, 0xCB, 0x09
		});

		public override PwUuid Uuid
		{
			get { return m_uuid; }
		}

		public override string Name
		{
			get { return (KPRes.System + " - " + KPRes.ClassicAdj); }
		}

		public override ToolStripRenderer CreateInstance()
		{
			// Checkboxes are rendered incorrectly
			// return new ToolStripSystemRenderer();

			return new SystemTsr();
		}
	}

	internal sealed class SystemTsr : ProExtTsr
	{
		private sealed class SystemTsrColorTable : SimpleColorTableEx
		{
			public SystemTsrColorTable() : base(
				SystemColors.MenuBar, SystemColors.Menu, SystemColors.Menu,
				SystemColors.Control, SystemColors.ControlDarkDark,
				SystemColors.Control, SystemColors.ControlDark,
				SystemColors.MenuHighlight, SystemColors.MenuHighlight)
			{
			}
		}

		protected override bool EnsureTextContrast
		{
			get { return false; } // Prevent color override by base class
		}

		public SystemTsr() : base(new SystemTsrColorTable())
		{
			this.MenuItemSelectedDisabledBackgroundColor = SystemColors.MenuHighlight;
			this.MenuItemSelectedDisabledBorderColor = SystemColors.MenuHighlight;
		}

		protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
		{
			ToolStripItem tsi = ((e != null) ? e.Item : null);

			if(tsi != null)
			{
				bool bDropDown = (tsi.OwnerItem != null);
				bool bCtxMenu = (tsi.Owner is ContextMenuStrip);

				Color clr = tsi.ForeColor;

				if(!tsi.Enabled && !tsi.Selected)
				{
					if(!UIUtil.IsHighContrast)
					{
						// Draw light "shadow"
						Rectangle r = e.TextRectangle;
						int dx = DpiUtil.ScaleIntX(128) / 128; // Force floor
						int dy = DpiUtil.ScaleIntY(128) / 128; // Force floor
						r.Offset(dx, dy);
						TextRenderer.DrawText(e.Graphics, e.Text, e.TextFont,
							r, SystemColors.HighlightText, e.TextFormat);
					}

					clr = SystemColors.GrayText;
				}
				else if(tsi.Selected && (bDropDown || bCtxMenu))
					clr = SystemColors.HighlightText;
				else if(UIUtil.ColorsEqual(clr, Control.DefaultForeColor))
					clr = SystemColors.MenuText;
				else
				{
					bool bDarkBack = this.IsDarkStyle;
					bool bDarkText = UIUtil.IsDarkColor(clr);

					if((bDarkBack && bDarkText) || (!bDarkBack && !bDarkText))
					{
						Debug.Assert(false);
						clr = SystemColors.MenuText;
					}
				}

				e.TextColor = clr;
			}
			else { Debug.Assert(false); }

			base.OnRenderItemText(e);
		}
	}
}
