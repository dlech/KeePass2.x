/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2017 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Diagnostics;

namespace KeePass.UI
{
	public sealed class BackgroundForm : Form
	{
		public BackgroundForm(Bitmap bmpBackground, Screen sc)
		{
			Screen s = (sc ?? Screen.PrimaryScreen);

			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.FormBorderStyle = FormBorderStyle.None;

			this.StartPosition = FormStartPosition.Manual;
			this.Location = s.Bounds.Location;
			this.Size = s.Bounds.Size;

			this.DoubleBuffered = true;
			this.BackColor = Color.Black;

			if(bmpBackground != null) this.BackgroundImage = bmpBackground;
		}
	}
}
