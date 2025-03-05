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

using KeePassLib;

namespace KeePass.UI.ToolStripRendering
{
	internal sealed class Win81TsrFactory : TsrFactory
	{
		private readonly PwUuid m_uuid = new PwUuid(new byte[] {
			0xEF, 0x5B, 0x4B, 0xE8, 0x49, 0xD1, 0x5E, 0x71,
			0x65, 0xE0, 0x26, 0x3B, 0x03, 0xBD, 0x8C, 0x3B
		});

		public override PwUuid Uuid
		{
			get { return m_uuid; }
		}

		public override string Name
		{
			get { return "Windows 8.1"; }
		}

		public override bool IsSupported()
		{
			return !UIUtil.IsHighContrast;
		}

		public override ToolStripRenderer CreateInstance()
		{
			return new Win81Tsr();
		}
	}

	internal sealed class Win81Tsr : ProExtTsr
	{
		private sealed class Win81TsrColorTable : SimpleColorTableEx
		{
			public Win81TsrColorTable() : base(
				Color.FromArgb(245, 246, 247), Color.FromArgb(240, 240, 240),
				Color.FromArgb(240, 240, 240),
				Color.FromArgb(184, 216, 249), Color.FromArgb(98, 163, 229),
				Color.FromArgb(213, 231, 248), Color.FromArgb(122, 177, 232),
				Color.FromArgb(209, 226, 242), Color.FromArgb(120, 174, 229))
			{
			}

			public override Color CheckBackground
			{
				get { return Color.FromArgb(192, 221, 235); }
			}

			public override Color CheckPressedBackground
			{
				get { return Color.FromArgb(168, 210, 236); }
			}

			public override Color CheckSelectedBackground
			{
				get { return Color.FromArgb(168, 210, 236); }
			}

			public override Color GripDark
			{
				get { return Color.FromArgb(187, 188, 189); }
			}

			public override Color GripLight
			{
				get { return Color.FromArgb(252, 252, 253); }
			}

			public override Color MenuBorder
			{
				get { return Color.FromArgb(151, 151, 151); }
			}

			public override Color OverflowButtonGradientBegin
			{
				get { return Color.FromArgb(245, 245, 245); }
			}

			public override Color OverflowButtonGradientEnd
			{
				get { return Color.FromArgb(229, 229, 229); }
			}

			public override Color OverflowButtonGradientMiddle
			{
				get { return Color.FromArgb(237, 237, 237); }
			}

			public override Color SeparatorDark
			{
				// Menu separators are (215, 215, 215),
				// toolbar separators are (135, 135, 136)
				get { return Color.FromArgb(175, 175, 175); }
			}

			public override Color SeparatorLight
			{
				get { return Color.FromArgb(248, 249, 249); }
			}
		}

		public Win81Tsr() : base(new Win81TsrColorTable())
		{
			this.MenuItemSelectedDisabledBackgroundColor = Color.FromArgb(225, 225, 225);
			this.MenuItemSelectedDisabledBorderColor = Color.FromArgb(174, 174, 174);
		}
	}
}
