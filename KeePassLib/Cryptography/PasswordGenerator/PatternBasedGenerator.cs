/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2019 Dominik Reichl <dominik.reichl@t-online.de>

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
	internal static class PatternBasedGenerator
	{
		internal static PwgError Generate(out ProtectedString psOut,
			PwProfile pwProfile, CryptoRandomStream crsRandomSource)
		{
			psOut = ProtectedString.Empty;

			string strPattern = pwProfile.Pattern;
			if(string.IsNullOrEmpty(strPattern)) return PwgError.Success;

			CharStream cs = new CharStream(strPattern);
			LinkedList<char> llGenerated = new LinkedList<char>();
			PwCharSet pcs = new PwCharSet();

			while(true)
			{
				char ch = cs.ReadChar();
				if(ch == char.MinValue) break;

				pcs.Clear();

				if(ch == '\\')
				{
					ch = cs.ReadChar();
					if(ch == char.MinValue) return PwgError.InvalidPattern;

					pcs.Add(ch); // Allow "{...}" support and char check
				}
				else if(ch == '[')
				{
					if(!ReadCustomCharSet(cs, pcs))
						return PwgError.InvalidPattern;
				}
				else
				{
					if(!pcs.AddCharSet(ch))
						return PwgError.InvalidPattern;
				}

				int nCount = 1;
				if(cs.PeekChar() == '{')
				{
					nCount = ReadCount(cs);
					if(nCount < 0) return PwgError.InvalidPattern;
				}

				for(int i = 0; i < nCount; ++i)
				{
					if(!PwGenerator.PrepareCharSet(pcs, pwProfile))
						return PwgError.InvalidCharSet;
					if(pwProfile.NoRepeatingCharacters)
					{
						foreach(char chUsed in llGenerated)
							pcs.Remove(chUsed);
					}

					char chGen = PwGenerator.GenerateCharacter(pcs,
						crsRandomSource);
					if(chGen == char.MinValue) return PwgError.TooFewCharacters;

					llGenerated.AddLast(chGen);
				}
			}

			if(llGenerated.Count == 0) return PwgError.Success;

			char[] v = new char[llGenerated.Count];
			llGenerated.CopyTo(v, 0);

			if(pwProfile.PatternPermutePassword)
				PwGenerator.Shuffle(v, crsRandomSource);

			byte[] pbUtf8 = StrUtil.Utf8.GetBytes(v);
			psOut = new ProtectedString(true, pbUtf8);
			MemUtil.ZeroByteArray(pbUtf8);

			MemUtil.ZeroArray<char>(v);
			return PwgError.Success;
		}

		private static bool ReadCustomCharSet(CharStream cs, PwCharSet pcsOut)
		{
			Debug.Assert(cs.PeekChar() != '['); // Consumed already
			Debug.Assert(pcsOut.Size == 0);

			bool bAdd = true;
			while(true)
			{
				char ch = cs.ReadChar();
				if(ch == char.MinValue) return false;
				if(ch == ']') break;

				if(ch == '\\')
				{
					ch = cs.ReadChar();
					if(ch == char.MinValue) return false;

					if(bAdd) pcsOut.Add(ch);
					else pcsOut.Remove(ch);
				}
				else if(ch == '^')
				{
					if(bAdd) bAdd = false;
					else return false; // '^' toggles the mode only once
				}
				else
				{
					PwCharSet pcs = new PwCharSet();
					if(!pcs.AddCharSet(ch)) return false;

					if(bAdd) pcsOut.Add(pcs.ToString());
					else pcsOut.Remove(pcs.ToString());
				}
			}

			return true;
		}

		private static int ReadCount(CharStream cs)
		{
			if(cs.ReadChar() != '{') { Debug.Assert(false); return -1; }

			// Ensure not empty
			char chFirst = cs.PeekChar();
			if((chFirst < '0') || (chFirst > '9')) return -1;

			long n = 0;
			while(true)
			{
				char ch = cs.ReadChar();
				if(ch == '}') break;

				if((ch >= '0') && (ch <= '9'))
				{
					n = (n * 10L) + (long)(ch - '0');
					if(n > int.MaxValue) return -1;
				}
				else return -1;
			}

			return (int)n;
		}
	}
}
