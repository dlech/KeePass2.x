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
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

using KeePass.UI;
using KeePass.Util;
using KeePass.Util.XmlSerialization;

using KeePassLib;
using KeePassLib.Serialization;
using KeePassLib.Utility;

namespace KeePass.App.Configuration
{
	public static class AppConfigSerializer
	{
		private static string g_strBaseName = null; // Null allowed

		private static string g_strAppDataDir = null;
		private static string g_strAppDataLocalDir = null;
		private static string g_strEnforcedConfigFile = null;
		private static string g_strGlobalConfigFile = null;
		private static string g_strUserConfigFile = null;

		private const string g_strFileSuffix = ".config.xml";
		private const string g_strFileEnfSuffix = ".config.enforced.xml";

		/// <summary>
		/// Get our folder in the application data directory.
		/// </summary>
		public static string AppDataDirectory
		{
			get { GetConfigPaths(); return g_strAppDataDir; }
		}

		/// <summary>
		/// Get our folder in the local application data directory.
		/// </summary>
		public static string LocalAppDataDirectory
		{
			get { GetConfigPaths(); return g_strAppDataLocalDir; }
		}

		internal static string EnforcedConfigFile
		{
			get { GetConfigPaths(); return g_strEnforcedConfigFile; }
		}

		/// <summary>
		/// Get/set the base name for the configuration. If this property is
		/// <c>null</c>, the class constructs names based on the current
		/// assembly and the product name.
		/// </summary>
		public static string BaseName
		{
			get { return g_strBaseName; }

			set
			{
				g_strBaseName = value;

				// Invalidate paths
				g_strAppDataDir = null;
				g_strAppDataLocalDir = null;
				g_strEnforcedConfigFile = null;
				g_strGlobalConfigFile = null;
				g_strUserConfigFile = null;
			}
		}

		private static XmlDocument g_xdEnforced = null;
		public static XmlDocument EnforcedConfigXml
		{
			get { return g_xdEnforced; }
			internal set { g_xdEnforced = value; }
		}

		private static void GetConfigPaths()
		{
			if(g_strGlobalConfigFile == null)
			{
				Assembly asm = Assembly.GetExecutingAssembly();
				if(asm == null) { Debug.Assert(false); return; }

#if !KeePassLibSD
				string strFile = null;

				try { strFile = asm.Location; }
				catch(Exception) { }

				if(string.IsNullOrEmpty(strFile))
					strFile = UrlUtil.FileUrlToPath(asm.GetName().CodeBase);
#else
				string strFile = UrlUtil.FileUrlToPath(asm.GetName().CodeBase);
#endif
				if(string.IsNullOrEmpty(strFile)) { Debug.Assert(false); strFile = string.Empty; }

				if(!string.IsNullOrEmpty(g_strBaseName))
					strFile = UrlUtil.GetFileDirectory(strFile, true, false) + g_strBaseName;
				else
				{
					if(strFile.EndsWith(".exe", StrUtil.CaseIgnoreCmp) ||
						strFile.EndsWith(".dll", StrUtil.CaseIgnoreCmp))
						strFile = strFile.Substring(0, strFile.Length - 4);
				}

				g_strGlobalConfigFile = strFile + g_strFileSuffix;
				g_strEnforcedConfigFile = strFile + g_strFileEnfSuffix;
			}

			if(g_strUserConfigFile == null)
			{
				string strBaseName = (!string.IsNullOrEmpty(g_strBaseName) ?
					g_strBaseName : PwDefs.ShortProductName);

				string strUserDir;
				try
				{
					strUserDir = Environment.GetFolderPath(
						Environment.SpecialFolder.ApplicationData);
				}
				catch(Exception)
				{
					strUserDir = UrlUtil.GetFileDirectory(UrlUtil.FileUrlToPath(
						Assembly.GetExecutingAssembly().GetName().CodeBase), true, false);
				}
				strUserDir = UrlUtil.EnsureTerminatingSeparator(strUserDir, false);

				string strUserDirLocal;
				try
				{
					strUserDirLocal = Environment.GetFolderPath(
						Environment.SpecialFolder.LocalApplicationData);
				}
				catch(Exception) { strUserDirLocal = strUserDir; }
				strUserDirLocal = UrlUtil.EnsureTerminatingSeparator(strUserDirLocal, false);

				g_strAppDataDir = strUserDir + strBaseName;
				g_strAppDataLocalDir = strUserDirLocal + strBaseName;
				g_strUserConfigFile = UrlUtil.EnsureTerminatingSeparator(
					g_strAppDataDir, false) + strBaseName + g_strFileSuffix;

				string strLocalOvr = Program.CommandLineArgs[
					AppDefs.CommandLineOptions.ConfigPathLocal];
				if(!string.IsNullOrEmpty(strLocalOvr))
				{
					string strWD = UrlUtil.EnsureTerminatingSeparator(
						WinUtil.GetWorkingDirectory(), false);
					g_strUserConfigFile = UrlUtil.MakeAbsolutePath(strWD +
						"Sentinel.txt", strLocalOvr);
					// Do not change g_strAppDataDir, as it's returned from
					// the AppDataDirectory property
				}

				Debug.Assert(!string.IsNullOrEmpty(g_strAppDataDir));
			}
		}

		private static void EnsureDirOfFileExists(string strFile)
		{
			if(string.IsNullOrEmpty(strFile)) { Debug.Assert(false); return; }

			try
			{
				string strDir = UrlUtil.GetFileDirectory(strFile, false, true);
				if(!Directory.Exists(strDir))
					Directory.CreateDirectory(strDir);
			}
			catch(Exception) { Debug.Assert(false); }
		}

		internal static XmlDocument LoadEnforced(bool bSetPrimary)
		{
			GetConfigPaths();

			if(bSetPrimary)
			{
				Debug.Assert(g_xdEnforced == null);
				g_xdEnforced = null;
			}

#if DEBUG
			Stopwatch sw = Stopwatch.StartNew();
#endif
			try
			{
				if(!File.Exists(g_strEnforcedConfigFile)) return null;

				XmlDocument xd = XmlUtilEx.LoadXmlDocument(g_strEnforcedConfigFile,
					StrUtil.Utf8);

				if(bSetPrimary) g_xdEnforced = xd;
				return xd;
			}
			catch(Exception ex)
			{
				FileDialogsEx.ShowConfigError(g_strEnforcedConfigFile, ex, false, false);
			}
#if DEBUG
			finally
			{
				sw.Stop();
			}
#endif

			return null;
		}

		private static AppConfigEx Load(string strFilePath, XmlDocument xdEnforced)
		{
			if(string.IsNullOrEmpty(strFilePath)) { Debug.Assert(false); return null; }

			AppConfigEx cfg = null;
			try
			{
				if(!File.Exists(strFilePath)) return null;

				XmlSerializerEx xs = new XmlSerializerEx(typeof(AppConfigEx));

				if(xdEnforced == null)
				{
					using(FileStream fs = new FileStream(strFilePath,
						FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						cfg = (AppConfigEx)xs.Deserialize(fs);
					}
				}
				else // Enforced configuration
				{
					XmlDocument xd = XmlUtilEx.LoadXmlDocument(strFilePath, StrUtil.Utf8);

					XmContext ctx = new XmContext(xd, AppConfigEx.GetNodeOptions,
						AppConfigEx.GetNodeKey);
					XmlUtil.MergeElements(xd.DocumentElement, xdEnforced.DocumentElement,
						"/" + xd.DocumentElement.Name, null, ctx);

					using(MemoryStream msW = new MemoryStream())
					{
						xd.Save(msW);

						using(MemoryStream msR = new MemoryStream(msW.ToArray(), false))
						{
							cfg = (AppConfigEx)xs.Deserialize(msR);
						}
					}
				}
			}
			catch(Exception ex)
			{
				FileDialogsEx.ShowConfigError(strFilePath, ex, false, true);
			}

			if(cfg != null) cfg.OnLoad();
			return cfg;
		}

		public static AppConfigEx Load()
		{
			GetConfigPaths();

			// AppConfigEx cfgEnf = Load(g_strEnforcedConfigFile);
			// if(cfgEnf != null)
			// {
			//	cfgEnf.Meta.IsEnforcedConfiguration = true;
			//	return cfgEnf;
			// }
			XmlDocument xdEnforced = LoadEnforced(true);

			AppConfigEx cfgGlobal = Load(g_strGlobalConfigFile, xdEnforced);
			if((cfgGlobal != null) && !cfgGlobal.Meta.PreferUserConfiguration)
				return cfgGlobal; // Do not show error for corrupted local configuration

			AppConfigEx cfgUser = Load(g_strUserConfigFile, xdEnforced);

			if((cfgGlobal == null) && (cfgUser == null))
			{
				if(xdEnforced != null)
				{
					// Create an empty configuration file and merge it with
					// the enforced configuration; this ensures that merge
					// behaviors (like the node mode 'Remove') work as intended
					try
					{
						string strFile = Program.TempFilesPool.GetTempFileName("xml");
						File.WriteAllText(strFile, AppConfigEx.GetEmptyXml(),
							StrUtil.Utf8);

						AppConfigEx cfg = Load(strFile, xdEnforced);
						if(cfg != null) return cfg;
						Debug.Assert(false);
					}
					catch(Exception) { Debug.Assert(false); }
				}

				AppConfigEx cfgNew = new AppConfigEx();
				cfgNew.OnLoad(); // Create defaults
				return cfgNew;
			}
			if((cfgGlobal != null) && (cfgUser == null)) return cfgGlobal;
			if((cfgGlobal == null) && (cfgUser != null)) return cfgUser;

			cfgUser.Meta.PreferUserConfiguration = cfgGlobal.Meta.PreferUserConfiguration;
			return (cfgGlobal.Meta.PreferUserConfiguration ? cfgUser : cfgGlobal);
		}

		internal static void SaveEx(AppConfigEx cfg, Stream s)
		{
			if(cfg == null) throw new ArgumentNullException("cfg");
			if(s == null) throw new ArgumentNullException("s");

			cfg.OnSavePre();

			// Temporarily remove user file preference (restore after saving)
			bool bPrefUser = cfg.Meta.PreferUserConfiguration;
			cfg.Meta.PreferUserConfiguration = false;

			try
			{
				using(XmlWriter xw = XmlUtilEx.CreateXmlWriter(s))
				{
					XmlSerializerEx xs = new XmlSerializerEx(typeof(AppConfigEx));
					xs.Serialize(xw, cfg);
				}
			}
			finally
			{
				cfg.Meta.PreferUserConfiguration = bPrefUser;
				cfg.OnSavePost();
			}
		}

		private static bool Save(AppConfigEx cfg, string strFilePath)
		{
			if(string.IsNullOrEmpty(strFilePath)) { Debug.Assert(false); return false; }

			try
			{
				IOConnectionInfo iocPath = IOConnectionInfo.FromPath(strFilePath);

				using(FileTransactionEx ft = new FileTransactionEx(iocPath,
					cfg.Application.UseTransactedConfigWrites))
				{
					using(Stream s = ft.OpenWrite())
					{
						SaveEx(cfg, s);
					}

					ft.CommitWrite();
				}

				return true;
			}
			catch(Exception ex)
			{
				FileDialogsEx.ShowConfigError(strFilePath, ex, true, false);
			}

			return false;
		}

		public static bool Save(AppConfigEx cfg)
		{
			if(cfg == null) { Debug.Assert(false); return false; }
			if(!cfg.Application.ConfigSave) return false;

			GetConfigPaths();

			if(cfg.Meta.PreferUserConfiguration)
			{
				EnsureDirOfFileExists(g_strUserConfigFile);
				if(Save(cfg, g_strUserConfigFile)) return true;

				if(Save(cfg, g_strGlobalConfigFile)) return true;
			}
			else // Prefer global
			{
				if(Save(cfg, g_strGlobalConfigFile)) return true;

				EnsureDirOfFileExists(g_strUserConfigFile);
				if(Save(cfg, g_strUserConfigFile)) return true;
			}

			return false;
		}

		internal static bool Save()
		{
			return Save(Program.Config);
		}

		internal static string TryCreateBackup(string strFilePath)
		{
			if(string.IsNullOrEmpty(strFilePath)) { Debug.Assert(false); return null; }

			try
			{
				if(!File.Exists(strFilePath)) return null;

				string strToS = UrlUtil.EnsureTerminatingSeparator(
					UrlUtil.GetTempPath(), false) + PwDefs.ShortProductName +
					"_" + DateTime.Now.ToString(@"yyMMdd'_'HHmmss");
				NumberFormatInfo nfi = NumberFormatInfo.InvariantInfo;

				for(int i = 1; i < 100; ++i)
				{
					string strTo = strToS + ((i == 1) ? string.Empty :
						("_" + i.ToString(nfi))) + g_strFileSuffix;

					if(File.Exists(strTo)) continue;

					File.Copy(strFilePath, strTo, true);
					return strTo;
				}

				Debug.Assert(false);
			}
			catch(Exception) { Debug.Assert(false); }

			return null;
		}

		internal static void CreateBackupIfNecessary()
		{
			AppConfigEx cfg = Program.Config;
			ulong uVersion = cfg.Meta.GetVersion();
			bool b = false;

			// if(!string.IsNullOrEmpty(cfg.Application.HelpUrl) &&
			//	!AppConfigEx.IsOptionEnforced(cfg.Application, "HelpUrl"))
			//	b = true; // Help URL might be deleted by auto-disable
			// else
			if(uVersion < 0x0002003600000000UL) // < 2.54
			{
				// The default content mode of the following elements has been
				// changed in KeePass 2.54
				if(AppConfigEx.IsOptionEnforced(cfg.Application, "TriggerSystem") ||
					AppConfigEx.IsOptionEnforced(cfg.Integration, "UrlSchemeOverrides") ||
					AppConfigEx.IsOptionEnforced(cfg.PasswordGenerator, "UserProfiles"))
					b = true;
			}

			if(b)
			{
				GetConfigPaths();
				TryCreateBackup(cfg.Meta.PreferUserConfiguration ?
					g_strUserConfigFile : g_strGlobalConfigFile);
			}
		}
	}
}
