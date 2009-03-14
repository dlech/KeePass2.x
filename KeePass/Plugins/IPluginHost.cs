/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2009 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Drawing;
using System.Diagnostics;

using KeePass.DataExchange;
using KeePass.Forms;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Cryptography.Cipher;
using KeePassLib.Keys;

namespace KeePass.Plugins
{
	public interface IPluginHost
	{
		/// <summary>
		/// Reference to the KeePass main window.
		/// </summary>
		MainForm MainWindow { get; }

		/// <summary>
		/// Reference to the currently opened database.
		/// </summary>
		PwDatabase Database { get; }

		/// <summary>
		/// Reference to the command-line arguments.
		/// </summary>
		CommandLineArgs CommandLineArgs { get; }

		/// <summary>
		/// Reference to the global cipher pool. When implementing
		/// an encryption algorithm, use this pool to register it.
		/// </summary>
		CipherPool CipherPool { get; }

		KeyProviderPool KeyProviderPool { get; }

		FileFormatPool FileFormatPool { get; }
	}
}
