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
