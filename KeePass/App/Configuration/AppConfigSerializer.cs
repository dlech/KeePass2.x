/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2008 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePassLib;
using KeePassLib.Utility;

namespace KeePass.App.Configuration
{
	public static class AppConfigSerializer
	{
		private static string m_strBaseName = null; // Null prop allowed

		private static string m_strCreateDir = null;
		private static string m_strEnforcedConfigFile = null;
		private static string m_strGlobalConfigFile = null;
		private static string m_strUserConfigFile = null;

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
					if(strFile.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
						strFile = strFile.Substring(0, strFile.Length - 4);
					else if(strFile.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
						strFile = strFile.Substring(0, strFile.Length - 4);
				}
				else // Base name != null
				{
					strFile = UrlUtil.GetFileDirectory(strFile, true) + m_strBaseName;
				}

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
						Assembly.GetExecutingAssembly().GetName().CodeBase), true);
				}

				if((!strUserDir.EndsWith(new string(Path.DirectorySeparatorChar, 1))) &&
					(!strUserDir.EndsWith("\\")) && (!strUserDir.EndsWith("/")))
				{
					strUserDir += new string(Path.DirectorySeparatorChar, 1);
				}

				m_strCreateDir = strUserDir + strBaseDirName;
				m_strUserConfigFile = m_strCreateDir + Path.DirectorySeparatorChar +
					strBaseDirName + ".config.xml";
			}

			Debug.Assert(m_strCreateDir != null); Debug.Assert(m_strCreateDir.Length > 0);
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

		private static AppConfigEx LoadConfigFileEx(string strFilePath)
		{
			if((strFilePath == null) || (strFilePath.Length == 0))
				return null;

			FileStream fs = null;
			AppConfigEx tConfig = null;
			XmlSerializer xmlSerial = new XmlSerializer(typeof(AppConfigEx));

			try
			{
				fs = new FileStream(strFilePath, FileMode.Open,
					FileAccess.Read, FileShare.Read);
				tConfig = (AppConfigEx)xmlSerial.Deserialize(fs);
			}
			catch(Exception) { } // Do not assert

			if(fs != null) { fs.Close(); fs = null; }

			return tConfig;
		}

		public static AppConfigEx Load()
		{
			AppConfigSerializer.GetConfigPaths();

			AppConfigEx cfgEnf = LoadConfigFileEx(m_strEnforcedConfigFile);
			if(cfgEnf != null)
			{
				cfgEnf.Meta.IsEnforcedConfiguration = true;
				return cfgEnf;
			}

			AppConfigEx cfgGlobal = LoadConfigFileEx(m_strGlobalConfigFile);
			AppConfigEx cfgUser = LoadConfigFileEx(m_strUserConfigFile);

			if((cfgGlobal == null) && (cfgUser == null)) return new AppConfigEx();
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
			tConfig.PrepareSave();

			XmlSerializer xmlSerial = new XmlSerializer(typeof(AppConfigEx));
			FileStream fs = null;
			bool bResult = true;

			// Temporarily remove user file preference (restore after saving)
			bool bConfigPref = tConfig.Meta.PreferUserConfiguration;
			if(bRemoveConfigPref) tConfig.Meta.PreferUserConfiguration = false;

			XmlWriterSettings xws = new XmlWriterSettings();
			xws.Encoding = Encoding.UTF8;
			xws.Indent = true;
			xws.IndentChars = "\t";

			try
			{
				fs = new FileStream(strFilePath, FileMode.Create,
					FileAccess.Write, FileShare.None);

				XmlWriter xw = XmlWriter.Create(fs, xws);

				xmlSerial.Serialize(xw, tConfig);

				xw.Close();
			}
			catch(Exception) { bResult = false; } // Do not assert

			if(fs != null) { fs.Close(); fs = null; }

			if(bRemoveConfigPref) tConfig.Meta.PreferUserConfiguration = bConfigPref;

			return bResult;
		}

		public static bool Save(AppConfigEx tConfig)
		{
			Debug.Assert(tConfig != null);
			if(tConfig == null) throw new ArgumentNullException("tConfig");

			AppConfigSerializer.GetConfigPaths();

			bool bPreferUser = false;
			AppConfigEx cfgGlobal = LoadConfigFileEx(m_strGlobalConfigFile);
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
