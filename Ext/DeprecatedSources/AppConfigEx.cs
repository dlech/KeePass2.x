/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2009 Dominik Reichl <dominik.reichl@t-online.de>

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

#if KeePassLibSD
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Diagnostics;

using KeePassLib;

#if KeePassLibSD
using KeePassLibSD;
#endif

namespace KeePassLib.Utility
{
	/// <summary>
	/// Implements a cascading configuration class (singleton). Application settings
	/// are first tried to be loaded from the global configuration file. If the
	/// requested configuration key isn't found, it is loaded from the user
	/// configuration file (stored in the user directory).
	/// </summary>
	public static class AppConfigEx
	{
		private static SortedDictionary<string, string> m_cfgCurrent =
			new SortedDictionary<string, string>();

		private static bool m_bModified = false;

		private static string m_strGlobalConfigFile = null;
		private static string m_strUserConfigFile = null;
		private static string m_strCreateDir = null;

		private static string m_strBaseName = null;

		private const string RootElementName = "Configuration";
		private const string ConfigItemElement = "Item";
		private const string ConfigItemName = "Name";
		private const string ConfigItemValue = "Value";

		private const string ValTrue = "True";
		private const string ValFalse = "False";

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

				m_strGlobalConfigFile = null; // Invalidate paths
				m_strUserConfigFile = null;
				m_strCreateDir = null;
			}
		}

		/// <summary>
		/// Load configuration. This method will load configuration information
		/// from several different files (global configuration file in application
		/// directory, configuration file in user directory, etc.) and mix them
		/// into one in-memory configuration pool.
		/// <param name="strBaseName">Base name for the configuration files and
		/// directories. For example, this could be "KeePass".</param>
		/// </summary>
		public static void Load()
		{
			GetConfigPaths();

			SortedDictionary<string, string> dictGlobal;
			LoadFile(m_strGlobalConfigFile, out dictGlobal);
			Debug.Assert(dictGlobal != null);

			SortedDictionary<string, string> dictUser;
			LoadFile(m_strUserConfigFile, out dictUser);
			Debug.Assert(dictUser != null);

			// Combine global and user options. Global options override user options.
			m_cfgCurrent = dictGlobal;
			foreach(KeyValuePair<string, string> kvp in dictUser)
			{
				if(!m_cfgCurrent.ContainsKey(kvp.Key))
					m_cfgCurrent[kvp.Key] = kvp.Value;
			}

			m_bModified = false;
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
		}

		private static void EnsureAppDataDirAvailable()
		{
			Debug.Assert(m_strCreateDir != null);
			if(m_strCreateDir == null) return;

			if(!Directory.Exists(m_strCreateDir))
			{
				try { Directory.CreateDirectory(m_strCreateDir); }
				catch(Exception) { Debug.Assert(false); }
			}
		}

		private static bool LoadFile(string strFile, out SortedDictionary<string, string> vConfig)
		{
			XmlDocument doc = new XmlDocument();
			XmlNode xmlField;
			string strValue;

			vConfig = new SortedDictionary<string, string>();

			try { doc.Load(strFile); }
			catch(Exception) { return false; }

			XmlElement el = doc.DocumentElement;
			if(el == null) return false;
			if(el.Name != RootElementName) return false;
			if(el.ChildNodes.Count <= 0) return true;

			foreach(XmlNode xmlChild in el.ChildNodes)
			{
				Debug.Assert(xmlChild != null); if(xmlChild == null) continue;

				xmlField = xmlChild.Attributes.GetNamedItem(ConfigItemName);
				if(xmlField == null) continue;

				strValue = xmlChild.InnerText;
				if(strValue != null) vConfig[xmlField.Value] = strValue;
				else vConfig[xmlField.Value] = string.Empty;
			}

			return true;
		}

		/// <summary>
		/// Save the current configuration to files. File cascading is
		/// handled by this function. This function first tries to write
		/// to the global configuration file, if this fails it tries the
		/// configuration file in the user's application directory.
		/// </summary>
		/// <returns>Returns <c>true</c>, if the configuration was saved
		/// successfully, <c>false</c> otherwise.</returns>
		public static bool Save()
		{
			if(m_bModified == false) return true;

			GetConfigPaths();

			if(SaveFile(m_strGlobalConfigFile)) return true;

			EnsureAppDataDirAvailable();
			if(SaveFile(m_strUserConfigFile)) return true;

			return false;
		}

		private static bool SaveFile(string strFile)
		{
			if(strFile == null) return false;

			XmlTextWriter xtw;
			try { xtw = new XmlTextWriter(strFile, Encoding.UTF8); }
			catch(Exception) { return false; }
			Debug.Assert(xtw != null); if(xtw == null) return false;

			xtw.WriteStartDocument();
			xtw.WriteWhitespace("\r\n");
			xtw.WriteStartElement(RootElementName);
			xtw.WriteWhitespace("\r\n");

			Debug.Assert(m_cfgCurrent != null);
			foreach(KeyValuePair<string, string> kvp in m_cfgCurrent)
			{
				xtw.WriteWhitespace("\t");
				xtw.WriteStartElement(ConfigItemElement);
				xtw.WriteAttributeString(ConfigItemName, kvp.Key);

				// xtw.WriteAttributeString(ConfigItemValue, kvp.Value);
				xtw.WriteString(kvp.Value);
				
				xtw.WriteEndElement();
				xtw.WriteWhitespace("\r\n");
			}

			xtw.WriteEndElement();
			xtw.WriteWhitespace("\r\n");
			xtw.WriteEndDocument();
			xtw.Close();

			return true;
		}

		/// <summary>
		/// Get the status of the global configuration file.
		/// </summary>
		/// <returns>Returns <c>true</c> if you can write to the
		/// global configuration file.</returns>
		public static bool HasWriteAccessToGlobal()
		{
			GetConfigPaths();

			StreamWriter sw = null;
			try { sw = File.AppendText(m_strGlobalConfigFile); }
			catch(Exception) { }

			if(sw != null)
			{
				sw.Close();
				return true;
			}

			return false;
		}

		/// <summary>
		/// Get a configuration string.
		/// </summary>
		/// <param name="strField">Name of the configuration item.</param>
		/// <returns>Configuration item value. Returns an empty string (<c>""</c>) if
		/// the named item could not be found.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if <paramref name="strField" />
		/// is <c>null</c>.</exception>
		public static string GetValue(string strField)
		{
			Debug.Assert(strField != null); if(strField == null) throw new ArgumentNullException();

			string str;
			if(m_cfgCurrent.TryGetValue(strField, out str)) return str;

			return string.Empty;
		}

		/// <summary>
		/// Get a configuration string.
		/// </summary>
		/// <param name="strField">Name of the configuration item.</param>
		/// <param name="strDefault">Default string to return if the named item
		/// doesn't exist. May be <c>null</c>.</param>
		/// <returns>Configuration item value. Returns <paramref name="strDefault" />
		/// if the configuration item doesn't exist.</returns>
		public static string GetValue(string strField, string strDefault)
		{
			Debug.Assert(strField != null); if(strField == null) throw new ArgumentNullException();

			string str;
			if(m_cfgCurrent.TryGetValue(strField, out str)) return str;

			return strDefault;
		}

		/// <summary>
		/// Get a configuration string.
		/// </summary>
		/// <param name="kvp">Key/value pair. Key = field name, value = default
		/// value to return if the key isn't found.</param>
		/// <returns>Configuration item value or the default value.</returns>
		public static string GetValue(KeyValuePair<string, string> kvp)
		{
			string str;
			if(m_cfgCurrent.TryGetValue(kvp.Key, out str)) return str;

			return kvp.Value;
		}

		/// <summary>
		/// Set a configuration item's value. The item is created if it doesn't
		/// exist yet. It is overwritten if it exists already.
		/// </summary>
		/// <param name="strField">Name of the configuration entry.</param>
		/// <param name="strValue">Value of the configuration entry.</param>
		/// <exception cref="System.ArgumentNullException">Thrown if one of
		/// the parameters is <c>null</c>.</exception>
		public static void SetValue(string strField, string strValue)
		{
			Debug.Assert(strField != null); // Exception will be thrown by GetValue
			Debug.Assert(strValue != null); if(strValue == null) throw new ArgumentNullException();

			string strExisting = GetValue(strField, null);

			if(strExisting == null) // Key doesn't exist yet
			{
				m_cfgCurrent[strField] = strValue;
				m_bModified = true;
			}
			else // strExisting != null, i.e. key exists already
			{
				if(!strValue.Equals(strExisting))
				{
					m_cfgCurrent[strField] = strValue;
					m_bModified = true;
				}
			}
		}

		public static void SetValue(KeyValuePair<string, string> kvpField, string strValue)
		{
			SetValue(kvpField.Key, strValue);
		}

		/// <summary>
		/// Get a boolean value from the current configuration.
		/// </summary>
		/// <param name="strField">Name of the field to get.</param>
		/// <param name="bDefaultIfNotFound">Default value that is returned
		/// if the specified field doesn't exist.</param>
		/// <returns>Returns boolean value.</returns>
		public static bool GetBool(string strField, bool bDefaultIfNotFound)
		{
			Debug.Assert(strField != null); if(strField == null) throw new ArgumentNullException();

			string str;
			if(m_cfgCurrent.TryGetValue(strField, out str))
			{
				if(str.Equals(ValTrue)) return true;
				if(str.Equals(ValFalse)) return false;
			}

			return bDefaultIfNotFound;
		}

		public static bool GetBool(KeyValuePair<string, string> kvp)
		{
			Debug.Assert((kvp.Value == ValTrue) || (kvp.Value == ValFalse));

			string str;
			if(m_cfgCurrent.TryGetValue(kvp.Key, out str))
			{
				if(str.Equals(ValTrue)) return true;
				if(str.Equals(ValFalse)) return false;
			}

			return (kvp.Value == ValTrue);
		}

		/// <summary>
		/// Set a configuration item's value.
		/// </summary>
		/// <param name="strField">Name of the item.</param>
		/// <param name="bValue">Value of the item.</param>
		public static void SetValue(string strField, bool bValue)
		{
			SetValue(strField, bValue ? ValTrue : ValFalse);
		}

		public static void SetValue(KeyValuePair<string, string> kvpField, bool bValue)
		{
			SetValue(kvpField.Key, bValue);
		}

		/// <summary>
		/// Get an integer value from the current configuration.
		/// </summary>
		/// <param name="strField">Name of the configuration item.</param>
		/// <param name="nDefaultIfNotFound">Default value that is returned if
		/// the specified item cannot be found.</param>
		/// <returns>An integer.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if <paramref name="strField" />
		/// is <c>null</c>.</exception>
		public static int GetInt(string strField, int nDefaultIfNotFound)
		{
			Debug.Assert(strField != null);
			if(strField == null) throw new ArgumentNullException("strField");

			string str;
			if(m_cfgCurrent.TryGetValue(strField, out str))
			{
				int nValue;
				if(StrUtil.TryParseInt(str, out nValue)) return nValue;
			}

			return nDefaultIfNotFound;
		}

		public static int GetInt(KeyValuePair<string, string> kvp)
		{
			int nValue;
			string str;
			if(m_cfgCurrent.TryGetValue(kvp.Key, out str))
			{
				if(StrUtil.TryParseInt(str, out nValue)) return nValue;
				else { Debug.Assert(false); }
			}

			if(StrUtil.TryParseInt(kvp.Value, out nValue)) return nValue;
			else { Debug.Assert(false); }

			return 0;
		}

		/// <summary>
		/// Get an unsigned integer value from the current configuration.
		/// </summary>
		/// <param name="strField">Name of the configuration item.</param>
		/// <param name="uDefaultIfNotFound">Default value that is returned if
		/// the specified item cannot be found.</param>
		/// <returns>An unsigned integer.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if <paramref name="strField" />
		/// is <c>null</c>.</exception>
		public static uint GetUInt(string strField, uint uDefaultIfNotFound)
		{
			Debug.Assert(strField != null);
			if(strField == null) throw new ArgumentNullException("strField");

			string str;
			if(m_cfgCurrent.TryGetValue(strField, out str))
			{
				uint uValue;
				if(StrUtil.TryParseUInt(str, out uValue)) return uValue;
			}

			return uDefaultIfNotFound;
		}

		public static uint GetUInt(KeyValuePair<string, string> kvp)
		{
			uint uValue;
			string str;
			if(m_cfgCurrent.TryGetValue(kvp.Key, out str))
			{
				if(StrUtil.TryParseUInt(str, out uValue)) return uValue;
				else { Debug.Assert(false); }
			}

			if(StrUtil.TryParseUInt(kvp.Value, out uValue)) return uValue;
			else { Debug.Assert(false); }

			return 0;
		}

		/// <summary>
		/// Get an unsigned long integer value from the current configuration.
		/// </summary>
		/// <param name="strField">Name of the configuration item.</param>
		/// <param name="uDefaultIfNotFound">Default value that is returned if
		/// the specified item cannot be found.</param>
		/// <returns>An unsigned long integer.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if <paramref name="strField" />
		/// is <c>null</c>.</exception>
		public static ulong GetULong(string strField, ulong uDefaultIfNotFound)
		{
			Debug.Assert(strField != null);
			if(strField == null) throw new ArgumentNullException("strField");

			string str;
			if(m_cfgCurrent.TryGetValue(strField, out str))
			{
				ulong uValue;
				if(StrUtil.TryParseULong(str, out uValue)) return uValue;
			}

			return uDefaultIfNotFound;
		}

		public static ulong GetULong(KeyValuePair<string, string> kvp)
		{
			ulong uValue;
			string str;
			if(m_cfgCurrent.TryGetValue(kvp.Key, out str))
			{
				if(StrUtil.TryParseULong(str, out uValue)) return uValue;
				else { Debug.Assert(false); }
			}

			if(StrUtil.TryParseULong(kvp.Value, out uValue)) return uValue;
			else { Debug.Assert(false); }

			return 0;
		}

		/// <summary>
		/// Set a configuration item's value.
		/// </summary>
		/// <param name="strField">Name of the configuration item.</param>
		/// <param name="nValue">Value of the configuration item.</param>
		public static void SetValue(string strField, int nValue)
		{
			SetValue(strField, nValue.ToString());
		}

		public static void SetValue(KeyValuePair<string, string> kvpField, int nValue)
		{
			SetValue(kvpField.Key, nValue.ToString());
		}

		/// <summary>
		/// Set a configuration item's value.
		/// </summary>
		/// <param name="strField">Name of the configuration item.</param>
		/// <param name="uValue">Value of the configuration item.</param>
		public static void SetValue(string strField, uint uValue)
		{
			SetValue(strField, uValue.ToString());
		}

		public static void SetValue(KeyValuePair<string, string> kvpField, uint uValue)
		{
			SetValue(kvpField.Key, uValue.ToString());
		}

		/// <summary>
		/// Set a configuration item's value.
		/// </summary>
		/// <param name="strField">Name of the configuration item.</param>
		/// <param name="uValue">Value of the configuration item.</param>
		public static void SetValue(string strField, ulong uValue)
		{
			SetValue(strField, uValue.ToString());
		}

		public static void SetValue(KeyValuePair<string, string> kvpField, ulong uValue)
		{
			SetValue(kvpField.Key, uValue.ToString());
		}

		/// <summary>
		/// Remove all fields indexed by a prefix and consecutive numbers.
		/// All fields starting with the prefix will be removed. Index
		/// is zero-based.
		/// </summary>
		/// <param name="strFieldPrefix">Field prefix.</param>
		/// <param name="uStartIndex">First index.</param>
		public static void RemoveIndexedItems(string strFieldPrefix, uint uStartIndex)
		{
			uint uPos = uStartIndex;
			string strField;

			while(true)
			{
				strField = strFieldPrefix + uPos.ToString();

				if(GetValue(strField, null) != null)
				{
					m_cfgCurrent.Remove(strField);
					Debug.Assert(GetValue(strField, null) == null);

					++uPos;
				}
				else break;
			}
		}
	}
}

#endif // KeePassLibSD
