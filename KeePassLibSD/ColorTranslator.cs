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
using System.Drawing;
using System.Diagnostics;

namespace KeePassLib
{
	public static class ColorTranslator
	{
		public static Color FromHtml(string strColor)
		{
			string str = strColor.Trim(new char[]{ '\r', '\n', ' ', '\t', '#' });
			str = str.ToUpper();

			if(str.Length != 6)
			{
				Debug.Assert(false);
				return Color.Empty;
			}

			int[] pb = new int[6];
			for(int i = 0; i < 6; ++i)
			{
				if((str[i] >= '0') && (str[i] <= '9')) pb[i] = str[i] - '0';
				else pb[i] = (str[i] - 'A') + 10;
			}

			int r = (pb[0] << 4) | pb[1];
			int g = (pb[2] << 4) | pb[3];
			int b = (pb[4] << 4) | pb[5];

			return Color.FromArgb(r, g, b);
		}
	}
}
