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
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace KeePass.UI.ToolStripRendering
{
	internal abstract class SimpleColorTableEx : ProfessionalColorTable
	{
		private readonly Color m_clrBarBack;
		private readonly Color m_clrMenuBack;
		private readonly Color m_clrImageBack;

		private readonly Color m_clrItemActiveBack;
		private readonly Color m_clrItemActiveBorder;
		private readonly Color m_clrItemSelBack;
		private readonly Color m_clrItemSelBorder;

		private readonly Color m_clrSubItemSelBack;
		private readonly Color m_clrSubItemSelBorder;

		public SimpleColorTableEx(Color clrBarBack, Color clrMenuBack,
			Color clrImageBack,
			Color clrItemActiveBack, Color clrItemActiveBorder,
			Color clrItemSelBack, Color clrItemSelBorder,
			Color clrSubItemSelBack, Color clrSubItemSelBorder)
		{
			m_clrBarBack = clrBarBack;
			m_clrMenuBack = clrMenuBack;
			m_clrImageBack = clrImageBack;

			m_clrItemActiveBack = clrItemActiveBack;
			m_clrItemActiveBorder = clrItemActiveBorder;
			m_clrItemSelBack = clrItemSelBack;
			m_clrItemSelBorder = clrItemSelBorder;

			m_clrSubItemSelBack = clrSubItemSelBack;
			m_clrSubItemSelBorder = clrSubItemSelBorder;
		}

		public override Color ButtonCheckedGradientBegin
		{
			get { return m_clrItemActiveBack; }
		}

		public override Color ButtonCheckedGradientEnd
		{
			get { return m_clrItemActiveBack; }
		}

		public override Color ButtonCheckedGradientMiddle
		{
			get { return m_clrItemActiveBack; }
		}

		public override Color ButtonCheckedHighlight
		{
			get { return m_clrItemActiveBack; } // Untested
		}

		public override Color ButtonCheckedHighlightBorder
		{
			get { return m_clrItemActiveBorder; } // Untested
		}

		public override Color ButtonPressedBorder
		{
			get { return m_clrItemActiveBorder; }
		}

		public override Color ButtonPressedGradientBegin
		{
			get { return m_clrItemActiveBack; }
		}

		public override Color ButtonPressedGradientEnd
		{
			get { return m_clrItemActiveBack; }
		}

		public override Color ButtonPressedGradientMiddle
		{
			get { return m_clrItemActiveBack; }
		}

		public override Color ButtonPressedHighlight
		{
			get { return m_clrItemActiveBack; } // Untested
		}

		public override Color ButtonPressedHighlightBorder
		{
			get { return m_clrItemActiveBorder; } // Untested
		}

		public override Color ButtonSelectedBorder
		{
			get { return m_clrItemSelBorder; }
		}

		public override Color ButtonSelectedGradientBegin
		{
			get { return m_clrItemSelBack; }
		}

		public override Color ButtonSelectedGradientEnd
		{
			get { return m_clrItemSelBack; }
		}

		public override Color ButtonSelectedGradientMiddle
		{
			get { return m_clrItemSelBack; }
		}

		public override Color ButtonSelectedHighlight
		{
			get { return m_clrItemSelBack; } // Untested
		}

		public override Color ButtonSelectedHighlightBorder
		{
			get { return m_clrItemSelBorder; }
		}

		public override Color CheckBackground
		{
			get { return m_clrMenuBack; }
		}

		public override Color CheckPressedBackground
		{
			get { return m_clrMenuBack; }
		}

		public override Color CheckSelectedBackground
		{
			get { return m_clrMenuBack; }
		}

		public override Color GripDark
		{
			get { return SystemColors.ControlDark; }
		}

		public override Color GripLight
		{
			get { return SystemColors.ControlLight; }
		}

		public override Color ImageMarginGradientBegin
		{
			get { return m_clrImageBack; }
		}

		public override Color ImageMarginGradientEnd
		{
			get { return m_clrImageBack; }
		}

		public override Color ImageMarginGradientMiddle
		{
			get { return m_clrImageBack; }
		}

		public override Color ImageMarginRevealedGradientBegin
		{
			get { return m_clrImageBack; }
		}

		public override Color ImageMarginRevealedGradientEnd
		{
			get { return m_clrImageBack; }
		}

		public override Color ImageMarginRevealedGradientMiddle
		{
			get { return m_clrImageBack; }
		}

		public override Color MenuBorder
		{
			get { return SystemColors.ActiveBorder; }
		}

		public override Color MenuItemBorder
		{
			get { return m_clrSubItemSelBorder; }
		}

		public override Color MenuItemPressedGradientBegin
		{
			// Used by pressed root menu items and inactive drop-down
			// arrow buttons in toolbar comboboxes (?!)
			get { return m_clrItemActiveBack; }
		}

		public override Color MenuItemPressedGradientEnd
		{
			get { return m_clrItemActiveBack; }
		}

		public override Color MenuItemPressedGradientMiddle
		{
			get { return m_clrItemActiveBack; }
		}

		public override Color MenuItemSelected
		{
			get { return m_clrSubItemSelBack; }
		}

		public override Color MenuItemSelectedGradientBegin
		{
			get { return m_clrItemSelBack; }
		}

		public override Color MenuItemSelectedGradientEnd
		{
			get { return m_clrItemSelBack; }
		}

		public override Color MenuStripGradientBegin
		{
			get { return m_clrBarBack; }
		}

		public override Color MenuStripGradientEnd
		{
			get { return m_clrBarBack; }
		}

		public override Color OverflowButtonGradientBegin
		{
			get { return SystemColors.ControlLight; }
		}

		public override Color OverflowButtonGradientEnd
		{
			get { return SystemColors.ControlDark; }
		}

		public override Color OverflowButtonGradientMiddle
		{
			get { return SystemColors.Control; }
		}

		public override Color RaftingContainerGradientBegin
		{
			get { return m_clrMenuBack; } // Untested
		}

		public override Color RaftingContainerGradientEnd
		{
			get { return m_clrMenuBack; } // Untested
		}

		public override Color SeparatorDark
		{
			// SeparatorDark is used for both the menu and the toolbar
			get { return SystemColors.ControlDark; }
		}

		public override Color SeparatorLight
		{
			get { return m_clrBarBack; }
		}

		public override Color StatusStripGradientBegin
		{
			get { return m_clrMenuBack; }
		}

		public override Color StatusStripGradientEnd
		{
			get { return m_clrMenuBack; }
		}

		public override Color ToolStripBorder
		{
			get { return m_clrMenuBack; }
		}

		public override Color ToolStripContentPanelGradientBegin
		{
			get { return m_clrMenuBack; } // Untested
		}

		public override Color ToolStripContentPanelGradientEnd
		{
			get { return m_clrMenuBack; } // Untested
		}

		public override Color ToolStripDropDownBackground
		{
			get { return m_clrMenuBack; }
		}

		public override Color ToolStripGradientBegin
		{
			get { return m_clrBarBack; }
		}

		public override Color ToolStripGradientEnd
		{
			get { return m_clrBarBack; }
		}

		public override Color ToolStripGradientMiddle
		{
			get { return m_clrBarBack; }
		}

		public override Color ToolStripPanelGradientBegin
		{
			get { return m_clrBarBack; } // Untested
		}

		public override Color ToolStripPanelGradientEnd
		{
			get { return m_clrBarBack; } // Untested
		}
	}
}
