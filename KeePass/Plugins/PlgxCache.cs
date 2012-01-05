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
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading;

using KeePass.App.Configuration;
using KeePass.Util.Spr;

using KeePassLib;
using KeePassLib.Utility;

namespace KeePass.Plugins
{
	public static class PlgxCache
	{
		private const string CacheDirName = "PluginCache";

		private static string m_strAppID = null;

		private static string GetAppID()
		{
			if(m_strAppID != null) return m_strAppID;

			try
			{
				Assembly asm = Assembly.GetExecutingAssembly();
				AssemblyName n = asm.GetName();
				m_strAppID = n.Version.ToString(4);
			}
			catch(Exception)
			{
				Debug.Assert(false);
				m_strAppID = PwDefs.VersionString;
			}

#if DEBUG
			m_strAppID += "d";
#endif

			return m_strAppID;
		}

		public static string GetCacheRoot()
		{
			if(Program.Config.Application.PluginCachePath.Length > 0)
			{
				string strRoot = SprEngine.Compile(Program.Config.Application.PluginCachePath,
					null);
				if(!string.IsNullOrEmpty(strRoot))
				{
					if(strRoot.EndsWith(new string(Path.DirectorySeparatorChar, 1)))
						strRoot = strRoot.Substring(0, strRoot.Length - 1);
					return strRoot;
				}
			}

			string strDataDir = AppConfigSerializer.AppDataDirectory;
			// try
			// {
			//	DirectoryInfo diAppData = new DirectoryInfo(strDataDir);
			//	DirectoryInfo diRoot = diAppData.Root;
			//	DriveInfo di = new DriveInfo(diRoot.FullName);
			//	if(di.DriveType == DriveType.Network)
			//	{
			//		strDataDir = UrlUtil.EnsureTerminatingSeparator(
			//			Path.GetTempPath(), false);
			//		strDataDir = strDataDir.Substring(0, strDataDir.Length - 1);
			//	}
			// }
			// catch(Exception) { Debug.Assert(false); }

			return (strDataDir + Path.DirectorySeparatorChar + CacheDirName);
		}

		public static string GetCacheDirectory(PwUuid pwPluginUuid, bool bEnsureExists)
		{
			string strPlgID = Convert.ToBase64String(pwPluginUuid.UuidBytes,
				Base64FormattingOptions.None);
			strPlgID = StrUtil.AlphaNumericOnly(strPlgID);
			if(strPlgID.Length > 16) strPlgID = strPlgID.Substring(0, 16);

			string strDir = GetCacheRoot() + Path.DirectorySeparatorChar +
				strPlgID + "_" + GetAppID();

			if(bEnsureExists && !Directory.Exists(strDir))
				Directory.CreateDirectory(strDir);

			return strDir;
		}

		public static string GetCacheFile(PwUuid pwPluginUuid, bool bMustExist,
			bool bCreateDirectory)
		{
			if(pwPluginUuid == null) return null;

			byte[] pbID = new byte[(int)PwUuid.UuidSize];
			Array.Copy(pwPluginUuid.UuidBytes, 0, pbID, 0, pbID.Length);
			Array.Reverse(pbID);
			string strID = Convert.ToBase64String(pbID, Base64FormattingOptions.None);
			strID = StrUtil.AlphaNumericOnly(strID);
			if(strID.Length > 8) strID = strID.Substring(0, 8);

			string strDir = GetCacheDirectory(pwPluginUuid, bCreateDirectory);
			string strPlugin = strDir + Path.DirectorySeparatorChar + strID + ".dll";
			bool bExists = File.Exists(strPlugin);

			if(bMustExist && bExists)
			{
				try { File.SetLastAccessTime(strPlugin, DateTime.Now); }
				catch(Exception) { } // Might be locked by other KeePass instance
			}

			if(!bMustExist || bExists) return strPlugin;
			return null;
		}

		public static string AddCacheAssembly(string strAssemblyPath, PlgxPluginInfo plgx)
		{
			if(string.IsNullOrEmpty(strAssemblyPath)) { Debug.Assert(false); return null; }

			string strNewFile = GetCacheFile(plgx.FileUuid, false, true);
			File.Copy(strAssemblyPath, strNewFile, true);

			return strNewFile;
		}

		public static string AddCacheFile(string strNormalFile, PlgxPluginInfo plgx)
		{
			if(string.IsNullOrEmpty(strNormalFile)) { Debug.Assert(false); return null; }

			string strNewFile = UrlUtil.EnsureTerminatingSeparator(GetCacheDirectory(
				plgx.FileUuid, true), false) + UrlUtil.GetFileName(strNormalFile);
			File.Copy(strNormalFile, strNewFile, true);

			return strNewFile;
		}

		public static ulong GetUsedCacheSize()
		{
			string strRoot = GetCacheRoot();
			if(!Directory.Exists(strRoot)) return 0;

			DirectoryInfo di = new DirectoryInfo(strRoot);
			FileInfo[] vFiles = di.GetFiles("*", SearchOption.AllDirectories);
			if(vFiles == null) { Debug.Assert(false); return 0; }

			ulong uSize = 0;
			foreach(FileInfo fi in vFiles) { uSize += (ulong)fi.Length; }

			return uSize;
		}

		public static void Clear()
		{
			try
			{
				string strRoot = GetCacheRoot();
				if(!Directory.Exists(strRoot)) return;

				Directory.Delete(strRoot, true);
			}
			catch(Exception) { Debug.Assert(false); }
		}

		public static void DeleteOldFilesAsync()
		{
			ThreadPool.QueueUserWorkItem(new WaitCallback(PlgxCache.DeleteOldFilesSafe));
		}

		private static void DeleteOldFilesSafe(object stateInfo)
		{
			try { DeleteOldFilesFunc(); }
			catch(Exception) { Debug.Assert(false); }
		}

		private static void DeleteOldFilesFunc()
		{
			string strRoot = GetCacheRoot();
			if(!Directory.Exists(strRoot)) return;
			
			DirectoryInfo di = new DirectoryInfo(strRoot);
			foreach(DirectoryInfo diSub in di.GetDirectories("*",
				SearchOption.TopDirectoryOnly))
			{
				try
				{
					if(ContainsOnlyOldFiles(diSub))
						Directory.Delete(diSub.FullName, true);
				}
				catch(Exception) { Debug.Assert(false); }
			}
		}

		private static bool ContainsOnlyOldFiles(DirectoryInfo di)
		{
			if((di.Name == ".") || (di.Name == "..")) return false;

			FileInfo[] vFiles = di.GetFiles("*.dll", SearchOption.TopDirectoryOnly);
			bool bNew = false;
			foreach(FileInfo fi in vFiles)
				bNew |= ((DateTime.Now - fi.LastAccessTime).TotalDays < 62.0);

			return !bNew;
		}
	}
}
