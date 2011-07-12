/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2011 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Reflection;
using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

using KeePass.Resources;
using KeePass.Util;

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

				m_strCreateDir = null;
				m_strCreateDirLocal = null;
				m_strEnforcedConfigFile = null; // Invalidate paths
				m_strGlobalConfigFile = null;
				m_strUserConfigFile = null;
			}
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

				if((strFile == null) || (strFile.Length == 0))
					strFile = UrlUtil.FileUrlToPath(asm.GetName().CodeBase);
#else
				string strFile = UrlUtil.FileUrlToPath(asm.GetName().CodeBase);
#endif
				Debug.Assert(strFile != null); if(strFile == null) return;

				if((m_strBaseName == null) || (m_strBaseName.Length == 0))
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
				m_strUserConfigFile = m_strCreateDir + Path.DirectorySeparatorChar +
					strBaseDirName + ".config.xml";
			}

			Debug.Assert(!string.IsNullOrEmpty(m_strCreateDir));
		}

		private static void EnsureAppDataDirAvailable()
		{
			Debug.Assert((m_strCreateDir != null) && (m_strCreateDir.Length > 0));
			if((m_strCreateDir == null) || (m_strCreateDir.Length == 0)) return;

			try
			{
				if(Directory.Exists(m_strCreateDir) == false)
					Directory.CreateDirectory(m_strCreateDir);
			}
			catch(Exception) { Debug.Assert(false); }
		}

		private static XmlDocument LoadEnforcedConfigFile()
		{
			try
			{
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.Load(m_strEnforcedConfigFile);
				return xmlDoc;
			}
			catch(Exception) { }

			return null;
		}

		private static AppConfigEx LoadConfigFileEx(string strFilePath,
			XmlDocument xdEnforced)
		{
			if(string.IsNullOrEmpty(strFilePath)) return null;

			AppConfigEx tConfig = null;
			XmlSerializer xmlSerial = new XmlSerializer(typeof(AppConfigEx));

			if(xdEnforced == null)
			{
				FileStream fs = null;
				try
				{
					fs = new FileStream(strFilePath, FileMode.Open,
						FileAccess.Read, FileShare.Read);
					tConfig = (AppConfigEx)xmlSerial.Deserialize(fs);
				}
				catch(Exception) { } // Do not assert

				if(fs != null) { fs.Close(); fs = null; }
			}
			else // Enforced configuration
			{
				try
				{
					XmlDocument xd = new XmlDocument();
					xd.Load(strFilePath);

					XmlUtil.MergeNodes(xd, xd.DocumentElement, xdEnforced.DocumentElement);

					MemoryStream msAsm = new MemoryStream();
					xd.Save(msAsm);
					MemoryStream msRead = new MemoryStream(msAsm.ToArray(), false);

					tConfig = (AppConfigEx)xmlSerial.Deserialize(msRead);
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
					XmlSerializer xmlSerial = new XmlSerializer(typeof(AppConfigEx));
					try
					{
						MemoryStream msEnf = new MemoryStream();
						xdEnforced.Save(msEnf);
						MemoryStream msRead = new MemoryStream(msEnf.ToArray(), false);
						
						AppConfigEx cfgEnf = (AppConfigEx)xmlSerial.Deserialize(msRead);
						cfgEnf.OnLoad();
						return cfgEnf;
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

			XmlSerializer xmlSerial = new XmlSerializer(typeof(AppConfigEx));
			bool bResult = true;

			// FileStream fs = null;
			IOConnectionInfo iocPath = IOConnectionInfo.FromPath(strFilePath);
			FileTransactionEx fts = new FileTransactionEx(iocPath, true);
			Stream fs = null;

			// Temporarily remove user file preference (restore after saving)
			bool bConfigPref = tConfig.Meta.PreferUserConfiguration;
			if(bRemoveConfigPref) tConfig.Meta.PreferUserConfiguration = false;

			XmlWriterSettings xws = new XmlWriterSettings();
			xws.Encoding = new UTF8Encoding(false);
			xws.Indent = true;
			xws.IndentChars = "\t";

			try
			{
				// fs = new FileStream(strFilePath, FileMode.Create,
				//	FileAccess.Write, FileShare.None);
				fs = fts.OpenWrite();
				if(fs == null) throw new InvalidOperationException();

				XmlWriter xw = XmlWriter.Create(fs, xws);
				xmlSerial.Serialize(xw, tConfig);
				xw.Close();
			}
			catch(Exception) { bResult = false; } // Do not assert

			if(fs != null) { fs.Close(); fs = null; }
			if(bResult)
			{
				try { fts.CommitWrite(); }
				catch(Exception) { Debug.Assert(false); }
			}

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
				EnsureAppDataDirAvailable();
				if(SaveConfigFileEx(tConfig, m_strUserConfigFile, true)) return true;

				if(SaveConfigFileEx(tConfig, m_strGlobalConfigFile, false)) return true;
			}
			else // Don't prefer user -- use global first
			{
				if(SaveConfigFileEx(tConfig, m_strGlobalConfigFile, false)) return true;

				EnsureAppDataDirAvailable();
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
