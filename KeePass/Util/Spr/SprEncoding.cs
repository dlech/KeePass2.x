/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2018 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePassLib.Native;
using KeePassLib.Utility;

namespace KeePass.Util.Spr
{
	internal static class SprEncoding
	{
		internal static string EncodeAsAutoTypeSequence(string str)
		{
			if(str == null) { Debug.Assert(false); return string.Empty; }

			str = SprEncoding.EscapeAutoTypeBrackets(str);

			str = str.Replace(@"[", @"{[}");
			str = str.Replace(@"]", @"{]}");

			str = str.Replace(@"+", @"{+}");
			str = str.Replace(@"%", @"{%}");
			str = str.Replace(@"~", @"{~}");
			str = str.Replace(@"(", @"{(}");
			str = str.Replace(@")", @"{)}");

			str = str.Replace(@"^", @"{^}");

			return str;
		}

		private static string EscapeAutoTypeBrackets(string str)
		{
			char chOpen = '\u25A1';
			while(str.IndexOf(chOpen) >= 0) ++chOpen;

			char chClose = chOpen;
			++chClose;
			while(str.IndexOf(chClose) >= 0) ++chClose;

			str = str.Replace('{', chOpen);
			str = str.Replace('}', chClose);

			str = str.Replace(new string(chOpen, 1), @"{{}");
			str = str.Replace(new string(chClose, 1), @"{}}");

			return str;
		}

		internal static string EncodeForCommandLine(string strRaw)
		{
			if(strRaw == null) { Debug.Assert(false); return string.Empty; }

			if(MonoWorkarounds.IsRequired(3471228285U) && NativeLib.IsUnix())
			{
				string str = strRaw;

				str = str.Replace("\\", "\\\\");
				str = str.Replace("\"", "\\\"");
				str = str.Replace("\'", "\\\'");
				str = str.Replace("\u0060", "\\\u0060"); // Grave accent
				str = str.Replace("$", "\\$");
				str = str.Replace("&", "\\&");
				str = str.Replace("<", "\\<");
				str = str.Replace(">", "\\>");
				str = str.Replace("|", "\\|");
				str = str.Replace(";", "\\;");
				str = str.Replace("(", "\\(");
				str = str.Replace(")", "\\)");

				return str;
			}

			// See SHELLEXECUTEINFO structure documentation
			return strRaw.Replace("\"", "\"\"\"");
		}
	}
}
