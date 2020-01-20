/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2020 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePassLib.Cryptography.PasswordGenerator
{
	internal static class CharSetBasedGenerator
	{
		internal static PwgError Generate(out ProtectedString psOut,
			PwProfile pwProfile, CryptoRandomStream crsRandomSource)
		{
			psOut = ProtectedString.Empty;
			if(pwProfile.Length == 0) return PwgError.Success;

			PwCharSet pcs = new PwCharSet(pwProfile.CharSet.ToString());
			if(!PwGenerator.PrepareCharSet(pcs, pwProfile))
				return PwgError.InvalidCharSet;

			char[] v = new char[pwProfile.Length];
			try
			{
				for(int i = 0; i < v.Length; ++i)
				{
					char ch = PwGenerator.GenerateCharacter(pcs, crsRandomSource);
					if(ch == char.MinValue)
						return PwgError.TooFewCharacters;

					v[i] = ch;
					if(pwProfile.NoRepeatingCharacters) pcs.Remove(ch);
				}

				byte[] pbUtf8 = StrUtil.Utf8.GetBytes(v);
				psOut = new ProtectedString(true, pbUtf8);
				MemUtil.ZeroByteArray(pbUtf8);
			}
			finally { MemUtil.ZeroArray<char>(v); }

			return PwgError.Success;
		}
	}
}
