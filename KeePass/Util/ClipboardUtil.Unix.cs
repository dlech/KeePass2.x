/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2012 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Windows.Forms;
using System.Diagnostics;

using KeePassLib.Native;

namespace KeePass.Util
{
	public static partial class ClipboardUtil
	{
		private static string GetStringM()
		{
			return (NativeLib.RunConsoleApp("pbpaste", "-pboard general") ??
				string.Empty);
		}

		private static void SetStringM(string str)
		{
			NativeLib.RunConsoleApp("pbcopy", "-pboard general", str);
		}

		private static string GetStringU()
		{
			// string str = NativeLib.RunConsoleApp("xclip",
			//	"-out -selection clipboard");
			// if(str != null) return str;

			string str = NativeLib.RunConsoleApp("xsel",
				"--output --clipboard");
			if(str != null) return str;

			if(Clipboard.ContainsText())
				return (Clipboard.GetText() ?? string.Empty);

			return string.Empty;
		}

		private static void SetStringU(string str)
		{
			// string r = NativeLib.RunConsoleApp("xclip",
			//	"-in -selection clipboard", str);
			// if(r != null) return;

			if(string.IsNullOrEmpty(str))
			{
				NativeLib.RunConsoleApp("xsel", "--delete --clipboard");

				try { Clipboard.Clear(); }
				catch(Exception) { Debug.Assert(false); }

				return; // xsel with an empty input can hang
			}

			string r = NativeLib.RunConsoleApp("xsel",
				"--input --clipboard", str);
			if(r != null) return;

			try { Clipboard.SetText(str); }
			catch(Exception) { Debug.Assert(false); }
		}
	}
}
