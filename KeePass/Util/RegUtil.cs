/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2025 Dominik Reichl <dominik.reichl@t-online.de>

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

using Microsoft.Win32;

using KeePassLib.Native;
using KeePassLib.Utility;

namespace KeePass.Util
{
	internal static class RegUtil
	{
		public static T GetValue<T>(string strKeyName, string strValueName)
		{
			return GetValue<T>(strKeyName, strValueName, default(T));
		}

		public static T GetValue<T>(string strKeyName, string strValueName,
			T tDefault)
		{
			if(string.IsNullOrEmpty(strKeyName)) { Debug.Assert(false); return tDefault; }

			try
			{
				return MemUtil.ConvertObject<T>(Registry.GetValue(strKeyName,
					strValueName, tDefault), tDefault);
			}
			catch(Exception) { Debug.Assert(NativeLib.IsUnix()); }

			return tDefault;
		}

		public static T GetValue<T>(RegistryKey rk, string strValueName)
		{
			return GetValue<T>(rk, strValueName, default(T));
		}

		public static T GetValue<T>(RegistryKey rk, string strValueName,
			T tDefault)
		{
			if(rk == null) { Debug.Assert(false); return tDefault; }

			try
			{
				return MemUtil.ConvertObject<T>(rk.GetValue(strValueName,
					tDefault), tDefault);
			}
			catch(Exception) { Debug.Assert(NativeLib.IsUnix()); }

			return tDefault;
		}

		public static RegistryKey OpenSubKey(RegistryKey rk, string strKeyName)
		{
			return OpenSubKey(rk, strKeyName, false); // Cf. RegistryKey.OpenSubKey
		}

		public static RegistryKey OpenSubKey(RegistryKey rk, string strKeyName,
			bool bWritable)
		{
			if(rk == null) { Debug.Assert(false); return null; }
			if(strKeyName == null) { Debug.Assert(false); return null; }

			try { return rk.OpenSubKey(strKeyName, bWritable); }
			catch(Exception) { Debug.Assert(NativeLib.IsUnix()); }

			return null;
		}
	}
}
