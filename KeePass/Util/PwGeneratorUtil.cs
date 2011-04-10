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
using System.Text;
using System.Diagnostics;

using KeePass.App;
using KeePass.Resources;

using KeePassLib.Cryptography.PasswordGenerator;
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
			if(Program.Config.PasswordGenerator.UserProfiles.Count > 0) return;

			AddStdPattern(KPRes.RandomMacAddress, @"HH\-HH\-HH\-HH\-HH\-HH");

			string strHex = KPRes.HexKey;
			AddStdPattern(strHex.Replace(@"{PARAM}", "40"), @"h{10}");
			AddStdPattern(strHex.Replace(@"{PARAM}", "128"), @"h{32}");
			AddStdPattern(strHex.Replace(@"{PARAM}", "256"), @"h{64}");
		}

		private static void AddStdPattern(string strName, string strPattern)
		{
			PwProfile p = new PwProfile();

			p.Name = strName;
			p.CollectUserEntropy = false;
			p.GeneratorType = PasswordGeneratorType.Pattern;
			p.Pattern = strPattern;

			Program.Config.PasswordGenerator.UserProfiles.Add(p);
		}
	}
}
