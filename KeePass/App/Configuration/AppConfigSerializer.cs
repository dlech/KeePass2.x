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
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

using KeePass.Resources;
using KeePass.Util;
using KeePass.Util.XmlSerialization;

using KeePassLib;
using KeePassLib.Serialization;
using KeePassLib.Utility;

namespace KeePass.App.Configuration
{
	public static class AppConfigSerializer
	{
		private static string m_strBaseName = null; // Null prop allowed

		private static string m_strCreateDir = null;
		private static string m_strCreateDirLocal = null;
		private static string m_strEnforcedConfigFile = null;
		private static string m_strGlobalConfigFile = null;
		private static string m_strUserConfigFile = null;

		public static string AppDataDirectory
		{
			get
			{
				AppConfigSerializer.GetConfigPaths();
				return m_strCreateDir;
			}
		}

		public static string LocalAppDataDirectory
		{
			get
			{
				AppConfigSerializer.GetConfigPaths();
				return m_strCreateDirLocal;
			}
		}

		/// <summary>
		/// Get/set the base name for the configuration. If this property is
		/// <c>null</c>, the class constructs names based on the current
		/// assembly and the product name.
		/// </summary>
		public static string BaseName
		{
			get { return m_strBaseName; }

			set
			{
				m_strBaseName = value;

				m_strCreateDir = null; // Invalidate paths
				m_strCreateDirLocal = null;
				m_strEnforcedConfigFile = null;
				m_strGlobalConfigFile = null;
				m_strUserConfigFile = null;
			}
		}

		private static XmlDocument m_xdEnforced = null;
		public static XmlDocument EnforcedConfigXml
		{
			get { return m_xdEnforced; }
		}

		private static void GetConfigPaths()
		{
			if(m_strGlobalConfigFile == null)
			{
				Assembly asm = Assembly.GetExecutingAssembly();
				Debug.Assert(asm != null); if(asm == null) return;

#if !KeePassLibSD
				string strFile = null;

				try { strFile = asm.Location; }
				catch(Exception) { }

				if(string.IsNullOrEmpty(strFile))
					strFile = UrlUtil.FileUrlToPath(asm.GetName().CodeBase);
#else
				string strFile = UrlUtil.FileUrlToPath(asm.GetName().CodeBase);
#endif
				Debug.Assert(strFile != null); if(strFile == null) return;

				if(string.IsNullOrEmpty(m_strBaseName))
				{
					// Remove assembly extension
					if(strFile.EndsWith(".exe", StrUtil.CaseIgnoreCmp))
						strFile = strFile.Substring(0, strFile.Length - 4);
					else if(strFile.EndsWith(".dll", StrUtil.CaseIgnoreCmp))
						strFile = strFile.Substring(0, strFile.Length - 4);
				}
				else // Base name != null
					strFile = UrlUtil.GetFileDirectory(strFile, true, false) + m_strBaseName;

				m_strGlobalConfigFile = strFile + ".config.xml";
				m_strEnforcedConfigFile = strFile + ".config.enforced.xml";
			}

			if(m_strUserConfigFile == null)
			{
				string strBaseDirName = PwDefs.ShortProductName;
				if((m_strBaseName != null) && (m_strBaseName.Length > 0))
					strBaseDirName = m_strBaseName;

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

				m_strCreateDir = strUserDir + strBaseDirName;
				m_strCreateDirLocal = strUserDirLocal + strBaseDirName;
				m_strUserConfigFile = UrlUtil.EnsureTerminatingSeparator(
					m_strCreateDir, false) + strBaseDirName + ".config.xml";
			}

			string strLocalOvr = Program.CommandLineArgs[
				AppDefs.CommandLineOptions.ConfigPathLocal];
			if(!string.IsNullOrEmpty(strLocalOvr))
			{
				string strWD = UrlUtil.EnsureTerminatingSeparator(
					WinUtil.GetWorkingDirectory(), false);
				m_strUserConfigFile = UrlUtil.MakeAbsolutePath(strWD +
					"Sentinel.txt", strLocalOvr);
				// Do not change m_strCreateDir, as it's returned from
				// the AppDataDirectory property
			}

			Debug.Assert(!string.IsNullOrEmpty(m_strCreateDir));
		}

		private static void EnsureAppDataDirAvailable(string strForFile)
		{
			if(string.IsNullOrEmpty(strForFile)) { Debug.Assert(false); return; }
			if(string.IsNullOrEmpty(m_strCreateDir)) { Debug.Assert(false); return; }

			string strPre = UrlUtil.EnsureTerminatingSeparator(m_strCreateDir, false);
			if(!strForFile.StartsWith(strPre, StrUtil.CaseIgnoreCmp)) return;

			try
			{
				if(!Directory.Exists(m_strCreateDir))
					Directory.CreateDirectory(m_strCreateDir);
			}
			catch(Exception) { Debug.Assert(false); }
		}

		private static XmlDocument LoadEnforcedConfigFile()
		{
#if DEBUG
			Stopwatch sw = Stopwatch.StartNew();
#endif

			m_xdEnforced = null;
			try
			{
				// Performance optimization
				if(!File.Exists(m_strEnforcedConfigFile)) return null;

				XmlDocument xmlDoc = XmlUtilEx.CreateXmlDocument();
				xmlDoc.Load(m_strEnforcedConfigFile);

				m_xdEnforced = xmlDoc;
				return xmlDoc;
			}
			catch(Exception) { Debug.Assert(false); }
#if DEBUG
			finally
			{
				sw.Stop();
			}
#endif

			return null;
		}

		private static AppConfigEx LoadConfigFileEx(string strFilePath,
			XmlDocument xdEnforced)
		{
			if(string.IsNullOrEmpty(strFilePath)) return null;

			AppConfigEx tConfig = null;
			XmlSerializerEx xmlSerial = new XmlSerializerEx(typeof(AppConfigEx));

			if(xdEnforced == null)
			{
				try
				{
					using(FileStream fs = new FileStream(strFilePath,
						FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						tConfig = (AppConfigEx)xmlSerial.Deserialize(fs);
					}
				}
				catch(Exception) { } // Do not assert
			}
			else // Enforced configuration
			{
				try
				{
					XmlDocument xd = XmlUtilEx.CreateXmlDocument();
					xd.Load(strFilePath);

					XmlUtil.MergeNodes(xd, xd.DocumentElement, xdEnforced.DocumentElement);

					using(MemoryStream msAsm = new MemoryStream())
					{
						xd.Save(msAsm);

						using(MemoryStream msRead = new MemoryStream(
							msAsm.ToArray(), false))
						{
							tConfig = (AppConfigEx)xmlSerial.Deserialize(msRead);
						}
					}
				}
				catch(FileNotFoundException) { }
				catch(Exception) { Debug.Assert(false); }
			}

			if(tConfig != null) tConfig.OnLoad();

			return tConfig;
		}

		public static AppConfigEx Load()
		{
			AppConfigSerializer.GetConfigPaths();

			// AppConfigEx cfgEnf = LoadConfigFileEx(m_strEnforcedConfigFile);
			// if(cfgEnf != null)
			// {
			//	cfgEnf.Meta.IsEnforcedConfiguration = true;
			//	return cfgEnf;
			// }
			XmlDocument xdEnforced = LoadEnforcedConfigFile();

			AppConfigEx cfgGlobal = LoadConfigFileEx(m_strGlobalConfigFile, xdEnforced);
			AppConfigEx cfgUser = LoadConfigFileEx(m_strUserConfigFile, xdEnforced);

			if((cfgGlobal == null) && (cfgUser == null))
			{
				if(xdEnforced != null)
				{
					XmlSerializerEx xmlSerial = new XmlSerializerEx(typeof(AppConfigEx));
					try
					{
						using(MemoryStream msEnf = new MemoryStream())
						{
							xdEnforced.Save(msEnf);

							using(MemoryStream msRead = new MemoryStream(
								msEnf.ToArray(), false))
							{
								AppConfigEx cfgEnf = (AppConfigEx)xmlSerial.Deserialize(msRead);
								cfgEnf.OnLoad();

								return cfgEnf;
							}
						}
					}
					catch(Exception) { Debug.Assert(false); }
				}

				AppConfigEx cfgNew = new AppConfigEx();
				cfgNew.OnLoad(); // Create defaults
				return cfgNew;
			}
			else if((cfgGlobal != null) && (cfgUser == null))
				return cfgGlobal;
			else if((cfgGlobal == null) && (cfgUser != null))
				return cfgUser;

			cfgUser.Meta.PreferUserConfiguration = cfgGlobal.Meta.PreferUserConfiguration;
			return (cfgGlobal.Meta.PreferUserConfiguration ? cfgUser : cfgGlobal);
		}

		private static bool SaveConfigFileEx(AppConfigEx tConfig,
			string strFilePath, bool bRemoveConfigPref)
		{
			tConfig.OnSavePre();

			// Temporarily remove user file preference (restore after saving)
			bool bConfigPref = tConfig.Meta.PreferUserConfiguration;
			if(bRemoveConfigPref) tConfig.Meta.PreferUserConfiguration = false;

			bool bResult = true;
			try
			{
				Debug.Assert(!string.IsNullOrEmpty(strFilePath));
				IOConnectionInfo iocPath = IOConnectionInfo.FromPath(strFilePath);

				using(FileTransactionEx ft = new FileTransactionEx(iocPath, true))
				{
					using(Stream s = ft.OpenWrite())
					{
						using(XmlWriter xw = XmlUtilEx.CreateXmlWriter(s))
						{
							XmlSerializerEx xs = new XmlSerializerEx(typeof(AppConfigEx));
							xs.Serialize(xw, tConfig);
						}
					}

					ft.CommitWrite();
				}
			}
			catch(Exception) { Debug.Assert(false); bResult = false; }

			if(bRemoveConfigPref) tConfig.Meta.PreferUserConfiguration = bConfigPref;

			tConfig.OnSavePost();
			return bResult;
		}

		public static bool Save(AppConfigEx tConfig)
		{
			Debug.Assert(tConfig != null);
			if(tConfig == null) throw new ArgumentNullException("tConfig");

			AppConfigSerializer.GetConfigPaths();

			bool bPreferUser = false;
			XmlDocument xdEnforced = LoadEnforcedConfigFile();
			AppConfigEx cfgGlobal = LoadConfigFileEx(m_strGlobalConfigFile, xdEnforced);
			if((cfgGlobal != null) && cfgGlobal.Meta.PreferUserConfiguration)
				bPreferUser = true;

			if(bPreferUser)
			{
				EnsureAppDataDirAvailable(m_strUserConfigFile);
				if(SaveConfigFileEx(tConfig, m_strUserConfigFile, true)) return true;

				if(SaveConfigFileEx(tConfig, m_strGlobalConfigFile, false)) return true;
			}
			else // Don't prefer user -- use global first
			{
				if(SaveConfigFileEx(tConfig, m_strGlobalConfigFile, false)) return true;

				EnsureAppDataDirAvailable(m_strUserConfigFile);
				if(SaveConfigFileEx(tConfig, m_strUserConfigFile, true)) return true;
			}

#if !KeePassLibSD
			if(Program.MainForm != null)
				Program.MainForm.SetStatusEx(KPRes.ConfigSaveFailed);
#endif

			return false;
		}
	}
}
