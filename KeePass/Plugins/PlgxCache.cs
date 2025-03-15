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
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

using KeePass.App.Configuration;
using KeePass.Util;
using KeePass.Util.Spr;

using KeePassLib;
using KeePassLib.Cryptography;
using KeePassLib.Native;
using KeePassLib.Utility;

namespace KeePass.Plugins
{
	public static class PlgxCache
	{
		private const string CacheFolder = "PluginCache";
		private const byte CacheVersion = 1;

		private static byte[] g_pbAppEnvID = null;
		// When changing this method, consider incrementing CacheVersion
		private static byte[] GetAppEnvID()
		{
			if(g_pbAppEnvID != null) return g_pbAppEnvID;

			using(MemoryStream ms = new MemoryStream())
			{
				using(BinaryWriter bw = new BinaryWriter(ms))
				{
					bw.Write(PwDefs.FileVersion64);

					if(Program.IsDevelopmentSnapshot())
					{
						try
						{
							byte[] pb = CryptoUtil.HashSha256(WinUtil.GetExecutable());
							bw.Write((byte)1);
							bw.Write(pb);
						}
						catch(Exception) { Debug.Assert(false); bw.Write(byte.MaxValue); }
					}
					else bw.Write((byte)0);

#if DEBUG
					bw.Write((byte)1);
#else
					bw.Write((byte)0);
#endif

					try
					{
						AssemblyName an = Assembly.GetExecutingAssembly().GetName();
						byte[] pb = (an.GetPublicKeyToken() ?? MemUtil.EmptyByteArray);
						bw.Write(pb.Length);
						bw.Write(pb);
					}
					catch(Exception) { Debug.Assert(false); bw.Write((int)-1); }

					bw.Write(MemUtil.VersionToUInt64(Environment.Version));
					bw.Write(IntPtr.Size);
					bw.Write((int)NativeLib.GetPlatformID());
				}

				byte[] pbID = ms.ToArray();
				g_pbAppEnvID = pbID;
				return pbID;
			}
		}

		public static string GetCacheRoot()
		{
			AceApplication aceApp = Program.Config.Application;
			string strRoot = aceApp.PluginCachePath;
			if(!string.IsNullOrEmpty(strRoot) && AppConfigEx.IsOptionEnforced(
				aceApp, "PluginCachePath"))
			{
				strRoot = SprEngine.Compile(strRoot, null);
				if(!string.IsNullOrEmpty(strRoot))
				{
					if(strRoot.EndsWith(new string(Path.DirectorySeparatorChar, 1)))
						strRoot = strRoot.Substring(0, strRoot.Length - 1);
					return strRoot;
				}
			}

			string strDataDir = AppConfigSerializer.LocalAppDataDirectory;
			// try
			// {
			//	DirectoryInfo diAppData = new DirectoryInfo(strDataDir);
			//	DirectoryInfo diRoot = diAppData.Root;
			//	DriveInfo di = new DriveInfo(diRoot.FullName);
			//	if(di.DriveType == DriveType.Network)
			//	{
			//		strDataDir = UrlUtil.EnsureTerminatingSeparator(
			//			UrlUtil.GetTempPath(), false);
			//		strDataDir = strDataDir.Substring(0, strDataDir.Length - 1);
			//	}
			// }
			// catch(Exception) { Debug.Assert(false); }

			return (UrlUtil.EnsureTerminatingSeparator(strDataDir, false) + CacheFolder);
		}

		public static string GetCacheDirectory(PlgxPluginInfo plgx, bool bEnsureExists)
		{
			if(plgx == null) { Debug.Assert(false); return null; }

			// When changing this method, consider incrementing CacheVersion
			byte[] pbHash;
			using(MemoryStream ms = new MemoryStream())
			{
				using(BinaryWriter bw = new BinaryWriter(ms, StrUtil.Utf8))
				{
					bw.Write(CacheVersion);
					bw.Write(plgx.BaseFileName); // Length-prefixed
					bw.Write(plgx.FileUuid.UuidBytes);
					bw.Write(GetAppEnvID());
				}

				pbHash = CryptoUtil.HashSha256(ms.ToArray());
			}

			string strHash = Convert.ToBase64String(pbHash);
			strHash = StrUtil.AlphaNumericOnly(strHash);
			if(strHash.Length > 20) strHash = strHash.Substring(0, 20);

			string strDir = GetCacheRoot() + Path.DirectorySeparatorChar + strHash;

			if(bEnsureExists && !Directory.Exists(strDir))
				Directory.CreateDirectory(strDir);

			return strDir;
		}

		public static string GetCacheFile(PlgxPluginInfo plgx, bool bMustExist,
			bool bCreateDirectory)
		{
			if(plgx == null) { Debug.Assert(false); return null; }

			// byte[] pbID = new byte[(int)PwUuid.UuidSize];
			// Array.Copy(pwPluginUuid.UuidBytes, 0, pbID, 0, pbID.Length);
			// Array.Reverse(pbID);
			// string strID = Convert.ToBase64String(pbID, Base64FormattingOptions.None);
			// strID = StrUtil.AlphaNumericOnly(strID);
			// if(strID.Length > 8) strID = strID.Substring(0, 8);

			string strFileName = StrUtil.AlphaNumericOnly(plgx.BaseFileName);
			if(strFileName.Length == 0) strFileName = "Plugin";
			strFileName += ".dll";

			string strDir = GetCacheDirectory(plgx, bCreateDirectory);
			string strPath = strDir + Path.DirectorySeparatorChar + strFileName;
			bool bExists = File.Exists(strPath);

			if(bMustExist && bExists)
			{
				try { File.SetLastAccessTimeUtc(strPath, DateTime.UtcNow); }
				catch(Exception) { } // Might be locked by other KeePass instance
			}

			if(!bMustExist || bExists) return strPath;
			return null;
		}

		public static string AddCacheAssembly(string strAssemblyPath, PlgxPluginInfo plgx)
		{
			if(string.IsNullOrEmpty(strAssemblyPath)) { Debug.Assert(false); return null; }

			string strNewFile = GetCacheFile(plgx, false, true);
			File.Copy(strAssemblyPath, strNewFile, true);

			return strNewFile;
		}

		public static string AddCacheFile(string strNormalFile, PlgxPluginInfo plgx)
		{
			if(string.IsNullOrEmpty(strNormalFile)) { Debug.Assert(false); return null; }

			string strNewFile = UrlUtil.EnsureTerminatingSeparator(GetCacheDirectory(
				plgx, true), false) + UrlUtil.GetFileName(strNormalFile);
			File.Copy(strNormalFile, strNewFile, true);

			return strNewFile;
		}

		public static ulong GetUsedCacheSize()
		{
			string strRoot = GetCacheRoot();
			if(!Directory.Exists(strRoot)) return 0;

			DirectoryInfo di = new DirectoryInfo(strRoot);
			List<FileInfo> lFiles = UrlUtil.GetFileInfos(di, "*",
				SearchOption.AllDirectories);

			ulong uSize = 0;
			foreach(FileInfo fi in lFiles) { uSize += (ulong)fi.Length; }

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

			List<FileInfo> lFiles = UrlUtil.GetFileInfos(di, "*.dll",
				SearchOption.TopDirectoryOnly);
			DateTime dtNow = DateTime.UtcNow;

			foreach(FileInfo fi in lFiles)
			{
				if((dtNow - fi.LastAccessTimeUtc).TotalDays < 62.0)
					return false;
			}

			return true;
		}
	}
}
