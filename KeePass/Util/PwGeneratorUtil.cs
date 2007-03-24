/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2007 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Diagnostics;

using KeePass.App;

using KeePassLib.Cryptography;
using KeePassLib.Utility;

namespace KeePass.Util
{
	public static class PwGeneratorUtil
	{
		/// <summary>
		/// Add standard profiles if none are available. If there is
		/// at least one profile available, this function does nothing.
		/// </summary>
		public static void AddStandardProfilesIfNoneAvailable()
		{
			string strPrefix = AppDefs.ConfigKeys.PwGenProfile.Key;
			string strFirst = AppConfigEx.GetValue(strPrefix + "0", null);
			if(strFirst != null) return;

			uint i = 0;
			AddStdPattern(i++, "Random MAC Address", @"HH\-HH\-HH\-HH\-HH\-HH");
			AddStdPattern(i++, "40-bit Hex Key", @"h{10}");
			AddStdPattern(i++, "128-bit Hex Key", @"h{32}");
			AddStdPattern(i++, "256-bit Hex Key", @"h{64}");
		}

		private static void AddStdPattern(uint uIndex, string strName,
			string strPattern)
		{
			string strPrefix = AppDefs.ConfigKeys.PwGenProfile.Key;
			PasswordGenerationOptions pwgo = new PasswordGenerationOptions();

			pwgo.CollectUserEntropy = false;
			pwgo.GeneratorType = PasswordGeneratorType.Pattern;
			pwgo.Pattern = strPattern;
			pwgo.ProfileName = strName;

			AppConfigEx.SetValue(strPrefix + uIndex.ToString(),
				PasswordGenerationOptions.SerializeToString(pwgo));
		}
	}
}
