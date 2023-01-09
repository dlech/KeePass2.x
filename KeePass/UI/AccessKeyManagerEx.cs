/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2023 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePassLib.Utility;

namespace KeePass.UI
{
	internal sealed class AccessKeyManagerEx
	{
		private readonly bool[] m_vUsedLetters = new bool[26];
		private readonly bool[] m_vUsedDigits = new bool[10];

#if DEBUG
		private static readonly char[] g_vKeybDep = new char[] {
			'\u00C4', '\u00D6', '\u00DC', '\u00E4', '\u00F6', '\u00FC'
		};
#endif

		private bool IsFree(char ch, out bool[] vUsed, out int iUsed)
		{
			if((ch >= 'A') && (ch <= 'Z'))
			{
				vUsed = m_vUsedLetters;
				iUsed = ch - 'A';
			}
			else if((ch >= 'a') && (ch <= 'z'))
			{
				vUsed = m_vUsedLetters;
				iUsed = ch - 'a';
			}
			else if((ch >= '0') && (ch <= '9'))
			{
				vUsed = m_vUsedDigits;
				iUsed = ch - '0';
			}
			else
			{
				vUsed = null;
				iUsed = -1;
				return false;
			}

			return !vUsed[iUsed];
		}

		public string CreateText(string strText, bool bEncode)
		{
			return CreateText(strText, bEncode, 0);
		}

		private string CreateText(string strText, bool bEncode, int iPrefStart)
		{
			if(string.IsNullOrEmpty(strText)) { Debug.Assert(false); return string.Empty; }

			if(bEncode) strText = StrUtil.EncodeMenuText(strText);
			else { Debug.Assert(strText.Replace("&&", string.Empty).IndexOf('&') < 0); }

			bool[] vUsed;
			int iUsed, i = iPrefStart, n = strText.Length;

			if((i < 0) || (i >= n)) { Debug.Assert(false); i = 0; iPrefStart = 0; }
			if(n == 0) { Debug.Assert(false); return string.Empty; }

			do
			{
				if(IsFree(strText[i], out vUsed, out iUsed))
				{
					vUsed[iUsed] = true;
					return strText.Insert(i, "&");
				}

				if(++i == n) i = 0;
			}
			while(i != iPrefStart);

			return strText;
		}

		public string CreateText(string strText, bool bEncode, string strParamPlh,
			string strParamValue)
		{
			if(string.IsNullOrEmpty(strText)) { Debug.Assert(false); return string.Empty; }
			if(string.IsNullOrEmpty(strParamPlh)) strParamPlh = "{PARAM}";
			if(strParamValue == null) { Debug.Assert(false); strParamValue = string.Empty; }

			int iParam = strText.IndexOf(strParamPlh);
			string str = strText.Replace(strParamPlh, strParamValue);

			return CreateText(str, bEncode, iParam);
		}

		public string RegisterText(string strText)
		{
			if(string.IsNullOrEmpty(strText)) { Debug.Assert(false); return string.Empty; }
			Debug.Assert(StrUtil.Count(strText.Replace("&&", string.Empty), "&") == 1);

			bool[] vUsed;
			int iUsed, iOffset = 0, n = strText.Length;

			do
			{
				int i = strText.IndexOf('&', iOffset);
				if(i < iOffset) { Debug.Assert(false); break; } // No access key

				int iNext = i + 1;
				if(iNext == n) { Debug.Assert(false); break; } // Lonely '&' at end

				char chNext = strText[iNext];
				if(chNext != '&')
				{
					if(IsFree(chNext, out vUsed, out iUsed))
						vUsed[iUsed] = true;
#if DEBUG
					else if(Array.IndexOf(g_vKeybDep, chNext) >= 0) { }
					else { Debug.Assert(false); } // Unknown or duplicate char.
#endif
					break;
				}

				iOffset = iNext + 1;
			}
			while(iOffset < n);

			return strText;
		}
	}
}
