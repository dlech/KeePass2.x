/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2026 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Text;
using System.Windows.Forms;

using KeePass.Native;

namespace KeePass.UI
{
	public sealed class CustomTextBoxEx : TextBox
	{
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		internal event EventHandler<CancelEventArgs> PasteEx;

		protected override void WndProc(ref Message m)
		{
			if((m.Msg == NativeMethods.WM_PASTE) && (this.PasteEx != null))
			{
				CancelEventArgs e = new CancelEventArgs();
				this.PasteEx(this, e);
				if(e.Cancel) return;
			}

			base.WndProc(ref m);
		}
	}
}
