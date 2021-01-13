/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2021 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Text;

namespace KeePass.Util
{
	public static class TextSimilarity
	{
		public static int LevenshteinDistance(string s, string t)
		{
			if(s == null) { Debug.Assert(false); throw new ArgumentNullException("s"); }
			if(t == null) { Debug.Assert(false); throw new ArgumentNullException("t"); }

			int m = s.Length, n = t.Length;
			if(m <= 0) return n;
			if(n <= 0) return m;

			int[,] d = new int[m + 1, n + 1];

			for(int k = 0; k <= m; ++k) d[k, 0] = k;
			for(int l = 1; l <= n; ++l) d[0, l] = l;

			for(int i = 1; i <= m; ++i)
			{
				char s_i = s[i - 1];

				for(int j = 1; j <= n; ++j)
				{
					int dSubst = ((s_i == t[j - 1]) ? 0 : 1);

					// Insertion, deletion and substitution
					d[i, j] = Math.Min(d[i, j - 1] + 1, Math.Min(
						d[i - 1, j] + 1, d[i - 1, j - 1] + dSubst));
				}
			}

			return d[m, n];
		}

		/* internal static void Test()
		{
			string[] v = new string[] {
				"Y00000000Y", "Y00001111Y", "Y11110000Y", "Y11111111Y",
				"YZZZZZZZAY", "YZZZZZZZBY"
			};
			int n = v.Length;

			float[,] mx = new float[n, n];
			for(int i = 0; i < n; ++i)
			{
				for(int j = i; j < n; ++j)
				{
					float f = (float)LevenshteinDistance(v[i], v[j]) /
						(float)Math.Max(v[i].Length, v[j].Length);
					float p = (1.0f - f) * 100.0f;
					mx[i, j] = p;
					mx[j, i] = p;
				}
			}

			float[] s = new float[n];
			for(int i = 0; i < n; ++i)
			{
				for(int j = 0; j < n; ++j)
					s[i] += mx[i, j];
				s[i] /= n;
			}
		} */
	}
}
