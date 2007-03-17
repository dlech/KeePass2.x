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
using System.Windows.Forms;

using KeePass.Resources;

using KeePassLib;

using Microsoft.Win32;

namespace KeePass.Util
{
	public static class ShellUtil
	{
		public static void RegisterExtension(string strFileExt, string strExtID,
			string strFullExtName, string strAppPath, string strAppName,
			bool bShowSuccessMessage)
		{
			try
			{
				RegistryKey kClassesRoot = Registry.ClassesRoot;

				try { kClassesRoot.CreateSubKey("." + strFileExt); }
				catch(Exception) { }
				RegistryKey kFileExt = kClassesRoot.OpenSubKey("." + strFileExt, true);

				kFileExt.SetValue(string.Empty, strExtID, RegistryValueKind.String);

				try { kClassesRoot.CreateSubKey(strExtID); }
				catch(Exception) { }
				RegistryKey kExtInfo = kClassesRoot.OpenSubKey(strExtID, true);

				kExtInfo.SetValue(string.Empty, strFullExtName, RegistryValueKind.String);

				try { kExtInfo.CreateSubKey("DefaultIcon"); }
				catch(Exception) { }
				RegistryKey kIcon = kExtInfo.OpenSubKey("DefaultIcon", true);

				if(strAppPath.IndexOfAny(new char[]{ ' ', '\t' }) < 0)
					kIcon.SetValue(string.Empty, strAppPath + ",0", RegistryValueKind.String);
				else
					kIcon.SetValue(string.Empty, "\"" + strAppPath + "\",0", RegistryValueKind.String);

				try { kExtInfo.CreateSubKey("shell"); }
				catch(Exception) { }
				RegistryKey kShell = kExtInfo.OpenSubKey("shell", true);

				try { kShell.CreateSubKey("open"); }
				catch(Exception) { }
				RegistryKey kShellOpen = kShell.OpenSubKey("open", true);

				kShellOpen.SetValue(string.Empty, "&Open with " + strAppName, RegistryValueKind.String);

				try { kShellOpen.CreateSubKey("command"); }
				catch(Exception) { }
				RegistryKey kShellCommand = kShellOpen.OpenSubKey("command", true);

				kShellCommand.SetValue(string.Empty, "\"" + strAppPath + "\" \"%1\"", RegistryValueKind.String);

				if(bShowSuccessMessage)
					MessageBox.Show(KPRes.FileExtInstallSuccess, PwDefs.ShortProductName,
						MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch(Exception)
			{
				MessageBox.Show(KPRes.FileExtInstallFailed, PwDefs.ShortProductName,
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		public static void UnregisterExtension(string strFileExt, string strExtID)
		{
			try
			{
				RegistryKey kClassesRoot = Registry.ClassesRoot;

				kClassesRoot.DeleteSubKeyTree("." + strFileExt);
				kClassesRoot.DeleteSubKeyTree(strExtID);
			}
			catch(Exception) { }
		}

		private const string AutoRunKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";

		public static void SetStartWithWindows(string strAppName, string strAppPath, bool bAutoStart)
		{
			try
			{
				if(bAutoStart)
					Registry.SetValue("HKEY_CURRENT_USER\\" + AutoRunKey, strAppName,
						strAppPath, RegistryValueKind.String);
				else
				{
					RegistryKey kRun = Registry.CurrentUser.OpenSubKey(AutoRunKey, true);
					kRun.DeleteValue(strAppName);
				}
			}
			catch(Exception) { }
		}

		public static bool GetStartWithWindows(string strAppName)
		{
			try
			{
				string strNotFound = Guid.NewGuid().ToString();
				string strResult = Registry.GetValue("HKEY_CURRENT_USER\\" + AutoRunKey,
					strAppName, strNotFound) as string;

				if((strResult != null) && (strResult != strNotFound) && (strResult != string.Empty))
					return true;
			}
			catch(Exception) { }

			return false;
		}
	}
}
