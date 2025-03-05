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
using System.Windows.Forms;

using KeePass.Native;
using KeePass.Resources;

using KeePassLib;
using KeePassLib.Utility;

using Microsoft.Win32;

namespace KeePass.Util
{
	public static class ShellUtil
	{
		public static void RegisterExtension(string strFileExt, string strExtId,
			string strFullExtName, string strAppPath, string strAppName,
			bool bShowSuccessMessage)
		{
			try
			{
				const RegistryValueKind rvkS = RegistryValueKind.String;
				RegistryKey rkCR = Registry.ClassesRoot;

				using(RegistryKey rk = rkCR.CreateSubKey("." + strFileExt))
				{
					rk.SetValue(string.Empty, strExtId, rvkS);
				}

				using(RegistryKey rkType = rkCR.CreateSubKey(strExtId))
				{
					rkType.SetValue(string.Empty, strFullExtName, rvkS);

					using(RegistryKey rk = rkType.CreateSubKey("DefaultIcon"))
					{
						if(strAppPath.IndexOfAny(new char[] { ' ', '\t' }) < 0)
							rk.SetValue(string.Empty, strAppPath + ",0", rvkS);
						else
							rk.SetValue(string.Empty, "\"" + strAppPath + "\",0", rvkS);
					}

					using(RegistryKey rkS = rkType.CreateSubKey("shell"))
					{
						using(RegistryKey rkSO = rkS.CreateSubKey("open"))
						{
							rkSO.SetValue(string.Empty, "&Open with " + strAppName, rvkS);

							using(RegistryKey rk = rkSO.CreateSubKey("command"))
							{
								rk.SetValue(string.Empty, "\"" + strAppPath + "\" \"%1\"", rvkS);
							}
						}
					}
				}

				ShChangeNotify();

				if(bShowSuccessMessage)
					MessageService.ShowInfo(KPRes.FileExtInstallSuccess);
			}
			catch(Exception)
			{
				MessageService.ShowWarning(KPRes.FileExtInstallFailed);
			}
		}

		public static void UnregisterExtension(string strFileExt, string strExtId)
		{
			try
			{
				RegistryKey rkCR = Registry.ClassesRoot;
				rkCR.DeleteSubKeyTree("." + strFileExt);
				rkCR.DeleteSubKeyTree(strExtId);

				ShChangeNotify();
			}
			catch(Exception) { Debug.Assert(false); }
		}

		private static void ShChangeNotify()
		{
			try
			{
				NativeMethods.SHChangeNotify(NativeMethods.SHCNE_ASSOCCHANGED,
					NativeMethods.SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
			}
			catch(Exception) { Debug.Assert(false); }
		}

		private const string AutoRunKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
		public static void SetStartWithWindows(string strAppName, string strAppPath,
			bool bAutoStart)
		{
			const string strKey = "HKEY_CURRENT_USER\\" + AutoRunKey;

			try
			{
				if(bAutoStart)
					Registry.SetValue(strKey, strAppName, strAppPath,
						RegistryValueKind.String);
				else
				{
					using(RegistryKey rk = Registry.CurrentUser.OpenSubKey(
						AutoRunKey, true))
					{
						rk.DeleteValue(strAppName);
					}
				}
			}
			catch(Exception ex) { MessageService.ShowWarning(strKey, ex); }
		}

		public static bool GetStartWithWindows(string strAppName)
		{
			return !string.IsNullOrEmpty(RegUtil.GetValue<string>(
				"HKEY_CURRENT_USER\\" + AutoRunKey, strAppName));
		}

		/* private const string PreLoadKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
		public static void RegisterPreLoad(string strAppName, string strAppPath,
			string strCmdLineOptions, bool bPreLoad)
		{
			try
			{
				if(bPreLoad)
				{
					string strValue = strAppPath;
					if(!string.IsNullOrEmpty(strCmdLineOptions))
						strValue += " " + strCmdLineOptions;

					Registry.SetValue("HKEY_LOCAL_MACHINE\\" + PreLoadKey, strAppName,
						strValue, RegistryValueKind.String);
				}
				else
				{
					using(RegistryKey rk = Registry.LocalMachine.OpenSubKey(
						PreLoadKey, true))
					{
						rk.DeleteValue(strAppName);
					}
				}
			}
			catch(Exception) { Debug.Assert(false); }
		} */
	}
}
