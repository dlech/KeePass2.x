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
using System.Reflection;
using System.Text;

using KeePass.Util;

using KeePassLib;
using KeePassLib.Delegates;
using KeePassLib.Native;

namespace KeePass.Util
{
	// Instead of the following, additional clipboard formats are used;
	// https://sourceforge.net/p/keepass/patches/120/
	/* public static partial class ClipboardUtil
	{
		private static bool SetStringUwp(string str)
		{
			if(string.IsNullOrEmpty(str)) return false; // Use Windows API

			try
			{
				GFunc<Type, object, bool> f = delegate(Type tDP, object oDP)
				{
					MethodInfo mi = tDP.GetMethod("SetText", BindingFlags.Public |
						BindingFlags.Instance);
					if(mi == null) { Debug.Assert(false); return false; }
					mi.Invoke(oDP, new object[] { str });

					return true;
				};

				return SetDataUwp(f);
			}
			catch(Exception) { Debug.Assert(false); }

			return false;
		}

		// private static bool SetDataUwp(byte[] pbToCopy, string strFormat)
		// {
		//	if(pbToCopy == null) { Debug.Assert(false); return false; }
		//	if(pbToCopy.Length == 0) return false; // Use Windows API
		//	if(string.IsNullOrEmpty(strFormat)) { Debug.Assert(false); return false; }
		//	try
		//	{
		//		GFunc<Type, object, bool> f = delegate(Type tDP, object oDP)
		//		{
		//			MethodInfo mi = tDP.GetMethod("SetData", BindingFlags.Public |
		//				BindingFlags.Instance);
		//			if(mi == null) { Debug.Assert(false); return false; }
		//			mi.Invoke(oDP, new object[] { strFormat, pbToCopy });
		//			return true;
		//		};
		//		return SetDataUwp(f);
		//	}
		//	catch(Exception) { Debug.Assert(false); }
		//	return false;
		// }

		private static bool SetDataUwp(GFunc<Type, object, bool> fSetData)
		{
			if(fSetData == null) { Debug.Assert(false); return false; }
			if(!WinUtil.IsAtLeastWindows10) return false;
			if(!Program.Config.Security.ClipboardNoPersist) return false;

			const string strDT = "Windows.ApplicationModel.DataTransfer";
			const string strC = strDT + ".Clipboard";
			const string strCCO = strDT + ".ClipboardContentOptions";
			const string strDP = strDT + ".DataPackage";
			const BindingFlags bfStatic = (BindingFlags.Public | BindingFlags.Static);
			const BindingFlags bfInst = (BindingFlags.Public | BindingFlags.Instance);

			Type tC = NativeLib.GetUwpType(strC);
			if(tC == null) { Debug.Assert(Environment.Version.Major < 4); return false; }

			MethodInfo miH = tC.GetMethod("IsHistoryEnabled", bfStatic);
			if(miH == null) return false; // >= Windows 10 1809 only
			bool bHistory = (bool)miH.Invoke(null, null);

			MethodInfo miR = tC.GetMethod("IsRoamingEnabled", bfStatic);
			if(miR == null) return false; // >= Windows 10 1809 only
			bool bRoaming = (bool)miR.Invoke(null, null);

			if(!bHistory && !bRoaming) return false;

			Type tCCO = NativeLib.GetUwpType(strCCO);
			if(tCCO == null) { Debug.Assert(false); return false; }

			object oCCO = Activator.CreateInstance(tCCO);
			if(oCCO == null) { Debug.Assert(false); return false; }

			PropertyInfo piH = tCCO.GetProperty("IsAllowedInHistory", bfInst);
			if(piH == null) { Debug.Assert(false); return false; }
			piH.SetValue(oCCO, false, null);

			PropertyInfo piR = tCCO.GetProperty("IsRoamable", bfInst);
			if(piR == null) { Debug.Assert(false); return false; }
			piR.SetValue(oCCO, false, null);

			Type tDP = NativeLib.GetUwpType(strDP);
			if(tDP == null) { Debug.Assert(false); return false; }

			object oDP = Activator.CreateInstance(tDP);
			if(oDP == null) { Debug.Assert(false); return false; }

			AttachIgnoreFormatUwp(tDP, oDP);
			if(!fSetData(tDP, oDP)) return false;

			MethodInfo miSet = tC.GetMethod("SetContentWithOptions", bfStatic);
			if(miSet == null) { Debug.Assert(false); return false; }
			if(!InvokeAndRetry(delegate()
			{
				return (bool)miSet.Invoke(null, new object[] { oDP, oCCO });
			})) return false;

			MethodInfo miFlush = tC.GetMethod("Flush", bfStatic);
			if(miFlush != null)
			{
				// The Flush method gives up too fast
				InvokeAndRetry(delegate()
				{
					miFlush.Invoke(null, null); // May throw exception
					return true;
				});
			}
			else { Debug.Assert(false); }

			return true;
		}

		private static void AttachIgnoreFormatUwp(Type tDP, object oDP)
		{
#warning Make sure that the option's default value is false;
#warning see the documentation/comment of the option.
			if(!Program.Config.Security.UseClipboardViewerIgnoreFormat) return;

			MethodInfo mi = tDP.GetMethod("SetData", BindingFlags.Public |
				BindingFlags.Instance);
			if(mi == null) { Debug.Assert(false); return; }

			// Objects are typically wrapped in a structure (containing
			// a type identifier and the length), i.e. the data stored
			// in the clipboard will differ from the source data;
			// strings seem to be an exception
			// byte[] pb = new byte[2];
			// mi.Invoke(oDP, new object[] { CfnViewerIgnore, pb });

			// The following results in two zero bytes in the clipboard
			mi.Invoke(oDP, new object[] { CfnViewerIgnore, string.Empty });
		}
	} */
}
