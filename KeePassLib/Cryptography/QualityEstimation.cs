/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2011 Dominik Reichl <dominik.reichl@t-online.de>

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

namespace KeePassLib.Cryptography
{
	/// <summary>
	/// A class that offers static functions to estimate the quality of
	/// passwords.
	/// </summary>
	public static class QualityEstimation
	{
		private enum CharSpaceBits : uint
		{
			Escape = 60,
			Alpha = 26,
			Number = 10,
			SimpleSpecial = 16,
			ExtendedSpecial = 17,
			High = 112
		}

		/// <summary>
		/// Estimate the quality of a password.
		/// </summary>
		/// <param name="vPasswordChars">Password to check.</param>
		/// <returns>Estimated bit-strength of the password.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if the input
		/// parameter is <c>null</c>.</exception>
		public static uint EstimatePasswordBits(char[] vPasswordChars)
		{
			Debug.Assert(vPasswordChars != null);
			if(vPasswordChars == null) throw new ArgumentNullException("vPasswordChars");

			bool bChLower = false, bChUpper = false, bChNumber = false;
			bool bChSimpleSpecial = false, bChExtSpecial = false, bChHigh = false;
			bool bChEscape = false;
			Dictionary<char, uint> vCharCounts = new Dictionary<char, uint>();
			Dictionary<int, uint> vDifferences = new Dictionary<int, uint>();
			double dblEffectiveLength = 0.0;

			for(int i = 0; i < vPasswordChars.Length; ++i) // Get character types
			{
				char tch = vPasswordChars[i];

				if(tch < ' ') bChEscape = true;
				if((tch >= 'A') && (tch <= 'Z')) bChUpper = true;
				if((tch >= 'a') && (tch <= 'z')) bChLower = true;
				if((tch >= '0') && (tch <= '9')) bChNumber = true;
				if((tch >= ' ') && (tch <= '/')) bChSimpleSpecial = true;
				if((tch >= ':') && (tch <= '@')) bChExtSpecial = true;
				if((tch >= '[') && (tch <= '`')) bChExtSpecial = true;
				if((tch >= '{') && (tch <= '~')) bChExtSpecial = true;
				if(tch > '~') bChHigh = true;

				double dblDiffFactor = 1.0;
				if(i >= 1)
				{
					int iDiff = (int)tch - (int)vPasswordChars[i - 1];

					if(vDifferences.ContainsKey(iDiff) == false)
						vDifferences.Add(iDiff, 1);
					else
					{
						vDifferences[iDiff] = vDifferences[iDiff] + 1;
						dblDiffFactor /= (double)vDifferences[iDiff];
					}
				}

				if(vCharCounts.ContainsKey(tch) == false)
				{
					vCharCounts.Add(tch, 1);
					dblEffectiveLength += dblDiffFactor;
				}
				else
				{
					vCharCounts[tch] = vCharCounts[tch] + 1;
					dblEffectiveLength += dblDiffFactor * (1.0 / (double)vCharCounts[tch]);
				}
			}

			uint charSpace = 0;
			if(bChEscape) charSpace += (uint)CharSpaceBits.Escape;
			if(bChUpper) charSpace += (uint)CharSpaceBits.Alpha;
			if(bChLower) charSpace += (uint)CharSpaceBits.Alpha;
			if(bChNumber) charSpace += (uint)CharSpaceBits.Number;
			if(bChSimpleSpecial) charSpace += (uint)CharSpaceBits.SimpleSpecial;
			if(bChExtSpecial) charSpace += (uint)CharSpaceBits.ExtendedSpecial;
			if(bChHigh) charSpace += (uint)CharSpaceBits.High;

			if(charSpace == 0) return 0;

			double dblBitsPerChar = Math.Log((double)charSpace) / Math.Log(2.0);
			double dblRating = dblBitsPerChar * dblEffectiveLength;

#if !KeePassLibSD
			char[] vLowerCopy = new char[vPasswordChars.Length];
			for(int ilc = 0; ilc < vLowerCopy.Length; ++ilc)
				vLowerCopy[ilc] = char.ToLower(vPasswordChars[ilc]);
			if(PopularPasswords.IsPopularPassword(vLowerCopy)) dblRating /= 8.0;
			Array.Clear(vLowerCopy, 0, vLowerCopy.Length);
#endif

			return (uint)Math.Ceiling(dblRating);
		}

		/// <summary>
		/// Estimate the quality of a password.
		/// </summary>
		/// <param name="pbUnprotectedUtf8">Password to check, UTF-8 encoded.</param>
		/// <returns>Estimated bit-strength of the password.</returns>
		public static uint EstimatePasswordBits(byte[] pbUnprotectedUtf8)
		{
			if(pbUnprotectedUtf8 == null) { Debug.Assert(false); return 0; }

			char[] vChars = Encoding.UTF8.GetChars(pbUnprotectedUtf8);
			uint uResult = EstimatePasswordBits(vChars);
			Array.Clear(vChars, 0, vChars.Length);

			return uResult;
		}
	}
}
