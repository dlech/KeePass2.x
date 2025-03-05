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
	internal sealed class Win10TsrFactory : TsrFactory
	{
		private readonly PwUuid m_uuid = new PwUuid(new byte[] {
			0x39, 0xE5, 0x05, 0x04, 0xB6, 0x56, 0x14, 0xE7,
			0x4F, 0x13, 0x68, 0x51, 0x85, 0xB3, 0x87, 0xC6
		});

		public override PwUuid Uuid
		{
			get { return m_uuid; }
		}

		public override string Name
		{
			get { return "Windows 10"; } // Version 1511
		}

		public override bool IsSupported()
		{
			return !UIUtil.IsHighContrast;
		}

		public override ToolStripRenderer CreateInstance()
		{
			return new Win10Tsr();
		}
	}

	internal sealed class Win10Tsr : ProExtTsr
	{
		private sealed class Win10TsrColorTable : SimpleColorTableEx
		{
			public Win10TsrColorTable() : base(
				Color.FromArgb(255, 255, 255), Color.FromArgb(242, 242, 242),
				Color.FromArgb(240, 240, 240),
				Color.FromArgb(204, 232, 255), Color.FromArgb(153, 209, 255),
				Color.FromArgb(229, 243, 255), Color.FromArgb(204, 232, 255),
				Color.FromArgb(145, 201, 247), Color.FromArgb(145, 201, 247))
			{
			}

			public override Color CheckBackground
			{
				get { return Color.FromArgb(144, 200, 246); }
			}

			public override Color CheckPressedBackground
			{
				get { return Color.FromArgb(86, 176, 250); }
			}

			public override Color CheckSelectedBackground
			{
				get { return Color.FromArgb(86, 176, 250); }
			}

			public override Color GripDark
			{
				get { return Color.FromArgb(195, 195, 195); }
			}

			public override Color GripLight
			{
				get { return Color.FromArgb(228, 228, 228); }
			}

			public override Color MenuBorder
			{
				get { return Color.FromArgb(204, 204, 204); }
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
				// toolbar separators are (140, 140, 140)
				get { return Color.FromArgb(177, 177, 177); }
			}
		}

		public Win10Tsr() : base(new Win10TsrColorTable())
		{
			// In image area (228, 228, 228), in text area (230, 230, 230)
			this.MenuItemSelectedDisabledBackgroundColor = Color.FromArgb(229, 229, 229);
			this.MenuItemSelectedDisabledBorderColor = Color.FromArgb(229, 229, 229);
		}
	}
}
