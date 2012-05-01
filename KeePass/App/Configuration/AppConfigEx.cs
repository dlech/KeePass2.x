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
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;

using KeePass.UI;
using KeePass.Util;
using KeePass.Util.XmlSerialization;

using KeePassLib.Serialization;
using KeePassLib.Utility;

namespace KeePass.App.Configuration
{
	[XmlType(TypeName = "Configuration")]
	public sealed class AppConfigEx
	{
		public AppConfigEx()
		{
		}

		private AceMeta m_meta = new AceMeta();
		public AceMeta Meta
		{
			get { return m_meta; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_meta = value;
			}
		}

		private AceApplication m_aceApp = new AceApplication();
		public AceApplication Application
		{
			get { return m_aceApp; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_aceApp = value;
			}
		}

		private AceLogging m_aceLogging = null;
		public AceLogging Logging
		{
			get
			{
				if(m_aceLogging == null) m_aceLogging = new AceLogging();
				return m_aceLogging;
			}
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_aceLogging = value;
			}
		}

		private AceMainWindow m_uiMainWindow = new AceMainWindow();
		public AceMainWindow MainWindow
		{
			get { return m_uiMainWindow; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_uiMainWindow = value;
			}
		}

		private AceUI m_aceUI = null;
		public AceUI UI
		{
			get
			{
				if(m_aceUI == null) m_aceUI = new AceUI();
				return m_aceUI;
			}
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_aceUI = value;
			}
		}

		private AceSecurity m_sec = null;
		public AceSecurity Security
		{
			get
			{
				if(m_sec == null) m_sec = new AceSecurity();
				return m_sec;
			}
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_sec = value;
			}
		}

		private AceNative m_native = null;
		public AceNative Native
		{
			get
			{
				if(m_native == null) m_native = new AceNative();
				return m_native;
			}
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_native = value;
			}
		}

		private AcePasswordGenerator m_pwGen = null;
		public AcePasswordGenerator PasswordGenerator
		{
			get
			{
				if(m_pwGen == null) m_pwGen = new AcePasswordGenerator();
				return m_pwGen;
			}
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_pwGen = value;
			}
		}

		private AceDefaults m_def = new AceDefaults();
		public AceDefaults Defaults
		{
			get { return m_def; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_def = value;
			}
		}

		private AceIntegration m_int = new AceIntegration();
		public AceIntegration Integration
		{
			get { return m_int; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_int = value;
			}
		}

		private AceCustomConfig m_cc = new AceCustomConfig();
		[XmlIgnore]
		public AceCustomConfig CustomConfig
		{
			get { return m_cc; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_cc = value;
			}
		}

		[XmlArray("Custom")]
		[XmlArrayItem("Item")]
		public AceKvp[] CustomSerialized
		{
			get { return m_cc.Serialize(); }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_cc.Deserialize(value);
			}
		}

		/// <summary>
		/// Prepare for saving the configuration to disk. None of the
		/// modifications in this method need to be rolled back
		/// (for rollback, use <c>OnSavePre</c> / <c>OnSavePost</c>).
		/// </summary>
		private void PrepareSave()
		{
			m_meta.OmitItemsWithDefaultValues = true;

			m_aceApp.LastUsedFile.ClearCredentials(true);

			foreach(IOConnectionInfo iocMru in m_aceApp.MostRecentlyUsed.Items)
				iocMru.ClearCredentials(true);

			if(m_def.RememberKeySources == false)
				m_def.KeySources.Clear();

			m_aceApp.TriggerSystem = Program.TriggerSystem;

			SearchUtil.PrepareForSerialize(m_def.SearchParameters);
		}

		internal void OnLoad()
		{
			// m_int.UrlSchemeOverrides.SetDefaultsIfEmpty();

			ObfuscateCred(false);
			ChangePathsRelAbs(true);

			// Remove invalid columns
			List<AceColumn> vColumns = m_uiMainWindow.EntryListColumns;
			int i = 0;
			while(i < vColumns.Count)
			{
				if(((int)vColumns[i].Type < 0) || ((int)vColumns[i].Type >=
					(int)AceColumnType.Count))
					vColumns.RemoveAt(i);
				else ++i;
			}

			SearchUtil.FinishDeserialize(m_def.SearchParameters);
		}

		internal void OnSavePre()
		{
			PrepareSave();

			ChangePathsRelAbs(false);
			ObfuscateCred(true);
		}

		internal void OnSavePost()
		{
			ObfuscateCred(false);
			ChangePathsRelAbs(true);
		}

		private void ChangePathsRelAbs(bool bMakeAbsolute)
		{
			ChangePathRelAbs(m_aceApp.LastUsedFile, bMakeAbsolute);

			foreach(IOConnectionInfo iocMru in m_aceApp.MostRecentlyUsed.Items)
				ChangePathRelAbs(iocMru, bMakeAbsolute);

			foreach(AceKeyAssoc kfp in m_def.KeySources)
			{
				if(kfp.DatabasePath.Length > 0)
					kfp.DatabasePath = ChangePathRelAbsStr(kfp.DatabasePath, bMakeAbsolute);

				if(kfp.KeyFilePath.Length > 0)
					kfp.KeyFilePath = ChangePathRelAbsStr(kfp.KeyFilePath, bMakeAbsolute);
			}
		}

		private static void ChangePathRelAbs(IOConnectionInfo ioc, bool bMakeAbsolute)
		{
			if(ioc == null) { Debug.Assert(false); return; }

			if(!ioc.IsLocalFile()) return;

			// Update path separators for current system
			if(!UrlUtil.IsUncPath(ioc.Path))
				ioc.Path = UrlUtil.ConvertSeparators(ioc.Path);

			string strBase = WinUtil.GetExecutable();
			bool bIsAbs = UrlUtil.IsAbsolutePath(ioc.Path);

			if(bMakeAbsolute && !bIsAbs)
				ioc.Path = UrlUtil.MakeAbsolutePath(strBase, ioc.Path);
			else if(!bMakeAbsolute && bIsAbs)
				ioc.Path = UrlUtil.MakeRelativePath(strBase, ioc.Path);
		}

		private static string ChangePathRelAbsStr(string strPath, bool bMakeAbsolute)
		{
			if(strPath == null) { Debug.Assert(false); return string.Empty; }

			IOConnectionInfo ioc = IOConnectionInfo.FromPath(strPath);
			ChangePathRelAbs(ioc, bMakeAbsolute);
			return ioc.Path;
		}

		private void ObfuscateCred(bool bObf)
		{
			if(m_aceApp.LastUsedFile == null) { Debug.Assert(false); }
			else m_aceApp.LastUsedFile.Obfuscate(bObf);

			foreach(IOConnectionInfo iocMru in m_aceApp.MostRecentlyUsed.Items)
			{
				if(iocMru == null) { Debug.Assert(false); }
				else iocMru.Obfuscate(bObf);
			}

			if(bObf) m_int.ProxyUserName = StrUtil.Obfuscate(m_int.ProxyUserName);
			else m_int.ProxyUserName = StrUtil.Deobfuscate(m_int.ProxyUserName);

			if(bObf) m_int.ProxyPassword = StrUtil.Obfuscate(m_int.ProxyPassword);
			else m_int.ProxyPassword = StrUtil.Deobfuscate(m_int.ProxyPassword);
		}

		private static Dictionary<object, string> m_dictXmlPathCache =
			new Dictionary<object, string>();
		public static bool IsOptionEnforced(object pContainer, PropertyInfo pi)
		{
			if(pContainer == null) { Debug.Assert(false); return false; }
			if(pi == null) { Debug.Assert(false); return false; }

			XmlDocument xdEnforced = AppConfigSerializer.EnforcedConfigXml;
			if(xdEnforced == null) return false;

			string strObjPath;
			if(!m_dictXmlPathCache.TryGetValue(pContainer, out strObjPath))
			{
				strObjPath = XmlUtil.GetObjectXmlPath(Program.Config, pContainer);
				if(string.IsNullOrEmpty(strObjPath)) { Debug.Assert(false); return false; }

				m_dictXmlPathCache[pContainer] = strObjPath;
			}

			string strProp = XmlSerializerEx.GetXmlName(pi);
			if(string.IsNullOrEmpty(strProp)) { Debug.Assert(false); return false; }

			string strPre = strObjPath;
			if(!strPre.EndsWith("/")) strPre += "/";
			string strXPath = strPre + strProp;

			XmlNode xn = xdEnforced.SelectSingleNode(strXPath);
			return (xn != null);
		}

		public static bool IsOptionEnforced(object pContainer, string strPropertyName)
		{
			if(pContainer == null) { Debug.Assert(false); return false; }
			if(string.IsNullOrEmpty(strPropertyName)) { Debug.Assert(false); return false; }

			// To improve performance (avoid type queries), check here, too
			XmlDocument xdEnforced = AppConfigSerializer.EnforcedConfigXml;
			if(xdEnforced == null) return false;

			Type tContainer = pContainer.GetType();
			PropertyInfo pi = tContainer.GetProperty(strPropertyName);
			return IsOptionEnforced(pContainer, pi);
		}

		public static void ClearXmlPathCache()
		{
			m_dictXmlPathCache.Clear();
		}
	}

	public sealed class AceMeta
	{
		public AceMeta()
		{
		}

		private bool m_bPrefLocalCfg = false;
		public bool PreferUserConfiguration
		{
			get { return m_bPrefLocalCfg; }
			set { m_bPrefLocalCfg = value; }
		}

		// private bool m_bIsEnforced = false;
		// [XmlIgnore]
		// public bool IsEnforcedConfiguration
		// {
		//	get { return m_bIsEnforced; }
		//	set { m_bIsEnforced = value; }
		// }

		private bool m_bOmitDefaultValues = true;
		// Informational property only (like an XML comment);
		// currently doesn't have any effect (the XmlSerializer
		// always omits default values, independent of this
		// property)
		public bool OmitItemsWithDefaultValues
		{
			get { return m_bOmitDefaultValues; }
			set { m_bOmitDefaultValues = value; }
		}
	}
}
