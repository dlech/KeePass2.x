/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2020 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

using KeePass.UI;
using KeePass.Util;
using KeePass.Util.XmlSerialization;

using KeePassLib.Delegates;
using KeePassLib.Native;
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

		private AceMeta m_meta = null;
		public AceMeta Meta
		{
			get
			{
				if(m_meta == null) m_meta = new AceMeta();
				return m_meta;
			}
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_meta = value;
			}
		}

		private AceApplication m_aceApp = null;
		public AceApplication Application
		{
			get
			{
				if(m_aceApp == null) m_aceApp = new AceApplication();
				return m_aceApp;
			}
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

		private AceMainWindow m_uiMainWindow = null;
		public AceMainWindow MainWindow
		{
			get
			{
				if(m_uiMainWindow == null) m_uiMainWindow = new AceMainWindow();
				return m_uiMainWindow;
			}
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

		private AceDefaults m_def = null;
		public AceDefaults Defaults
		{
			get
			{
				if(m_def == null) m_def = new AceDefaults();
				return m_def;
			}
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_def = value;
			}
		}

		private AceIntegration m_int = null;
		public AceIntegration Integration
		{
			get
			{
				if(m_int == null) m_int = new AceIntegration();
				return m_int;
			}
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_int = value;
			}
		}

		private AceCustomConfig m_cc = null;
		[XmlIgnore]
		public AceCustomConfig CustomConfig
		{
			get
			{
				if(m_cc == null) m_cc = new AceCustomConfig();
				return m_cc;
			}
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
			get
			{
				return this.CustomConfig.Serialize(); // m_cc might be null
			}
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				this.CustomConfig.Deserialize(value); // m_cc might be null
			}
		}

		/// <summary>
		/// Prepare for saving the configuration to disk. None of the
		/// modifications in this method need to be rolled back
		/// (for rollback, use <c>OnSavePre</c> / <c>OnSavePost</c>).
		/// </summary>
		private void PrepareSave()
		{
			AceMeta aceMeta = this.Meta; // m_meta might be null
			AceApplication aceApp = this.Application; // m_aceApp might be null
			AceDefaults aceDef = this.Defaults; // m_def might be null

			aceMeta.OmitItemsWithDefaultValues = true;
			aceMeta.DpiFactorX = DpiUtil.FactorX; // For new (not loaded) cfgs.
			aceMeta.DpiFactorY = DpiUtil.FactorY;

			aceApp.LastUsedFile.ClearCredentials(true);

			foreach(IOConnectionInfo iocMru in aceApp.MostRecentlyUsed.Items)
				iocMru.ClearCredentials(true);

			if(aceDef.RememberKeySources == false)
				aceDef.KeySources.Clear();

			aceApp.TriggerSystem = Program.TriggerSystem;

			SearchUtil.PrepareForSerialize(aceDef.SearchParameters);

			const int m = 64; // Maximum number of compatibility items
			List<string> l = aceApp.PluginCompatibility;
			if(l.Count > m) l.RemoveRange(m, l.Count - m); // See reg.
		}

		internal void OnLoad()
		{
			AceMainWindow aceMW = this.MainWindow; // m_uiMainWindow might be null
			AceDefaults aceDef = this.Defaults; // m_def might be null

			// aceInt.UrlSchemeOverrides.SetDefaultsIfEmpty();

			ObfuscateCred(false);
			ChangePathsRelAbs(true);

			// Remove invalid columns
			List<AceColumn> vColumns = aceMW.EntryListColumns;
			int i = 0;
			while(i < vColumns.Count)
			{
				if(((int)vColumns[i].Type < 0) || ((int)vColumns[i].Type >=
					(int)AceColumnType.Count))
					vColumns.RemoveAt(i);
				else ++i;
			}

			SearchUtil.FinishDeserialize(aceDef.SearchParameters);
			DpiScale();

			if(aceMW.EscMinimizesToTray) // For backward compatibility
			{
				aceMW.EscMinimizesToTray = false; // Default value
				aceMW.EscAction = AceEscAction.MinimizeToTray;
			}

			if(NativeLib.IsUnix())
			{
				this.Security.MasterKeyOnSecureDesktop = false;

				AceIntegration aceInt = this.Integration;
				aceInt.HotKeyGlobalAutoType = (ulong)Keys.None;
				aceInt.HotKeyGlobalAutoTypePassword = (ulong)Keys.None;
				aceInt.HotKeySelectedAutoType = (ulong)Keys.None;
				aceInt.HotKeyShowWindow = (ulong)Keys.None;
			}

			if(MonoWorkarounds.IsRequired(1378))
			{
				AceWorkspaceLocking aceWL = this.Security.WorkspaceLocking;
				aceWL.LockOnSessionSwitch = false;
				aceWL.LockOnSuspend = false;
				aceWL.LockOnRemoteControlChange = false;
			}

			if(MonoWorkarounds.IsRequired(1418))
			{
				aceMW.MinimizeAfterOpeningDatabase = false;
				this.Application.Start.MinimizedAndLocked = false;
			}
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
			AceApplication aceApp = this.Application; // m_aceApp might be null
			AceDefaults aceDef = this.Defaults; // m_def might be null

			ChangePathRelAbs(aceApp.LastUsedFile, bMakeAbsolute);

			foreach(IOConnectionInfo iocMru in aceApp.MostRecentlyUsed.Items)
				ChangePathRelAbs(iocMru, bMakeAbsolute);

			List<string> lWDKeys = aceApp.GetWorkingDirectoryContexts();
			foreach(string strWDKey in lWDKeys)
				aceApp.SetWorkingDirectory(strWDKey, ChangePathRelAbsStr(
					aceApp.GetWorkingDirectory(strWDKey), bMakeAbsolute));

			foreach(AceKeyAssoc kfp in aceDef.KeySources)
			{
				kfp.DatabasePath = ChangePathRelAbsStr(kfp.DatabasePath, bMakeAbsolute);
				kfp.KeyFilePath = ChangePathRelAbsStr(kfp.KeyFilePath, bMakeAbsolute);
			}
		}

		private static void ChangePathRelAbs(IOConnectionInfo ioc, bool bMakeAbsolute)
		{
			if(ioc == null) { Debug.Assert(false); return; }

			if(!ioc.IsLocalFile()) return;

			// Update path separators for current system
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
			if(strPath.Length == 0) return strPath;

			IOConnectionInfo ioc = IOConnectionInfo.FromPath(strPath);
			ChangePathRelAbs(ioc, bMakeAbsolute);
			return ioc.Path;
		}

		private void ObfuscateCred(bool bObf)
		{
			AceApplication aceApp = this.Application; // m_aceApp might be null
			AceIntegration aceInt = this.Integration; // m_int might be null

			if(aceApp.LastUsedFile == null) { Debug.Assert(false); }
			else aceApp.LastUsedFile.Obfuscate(bObf);

			foreach(IOConnectionInfo iocMru in aceApp.MostRecentlyUsed.Items)
			{
				if(iocMru == null) { Debug.Assert(false); }
				else iocMru.Obfuscate(bObf);
			}

			if(bObf) aceInt.ProxyUserName = StrUtil.Obfuscate(aceInt.ProxyUserName);
			else aceInt.ProxyUserName = StrUtil.Deobfuscate(aceInt.ProxyUserName);

			if(bObf) aceInt.ProxyPassword = StrUtil.Obfuscate(aceInt.ProxyPassword);
			else aceInt.ProxyPassword = StrUtil.Deobfuscate(aceInt.ProxyPassword);
		}

		private void DpiScale()
		{
			AceMeta aceMeta = this.Meta; // m_meta might be null
			double dCfgX = aceMeta.DpiFactorX, dCfgY = aceMeta.DpiFactorY;
			double dScrX = DpiUtil.FactorX, dScrY = DpiUtil.FactorY;

			if((dScrX == dCfgX) && (dScrY == dCfgY)) return;

			// When this method returns, all positions and sizes are in pixels
			// for the current screen DPI
			aceMeta.DpiFactorX = dScrX;
			aceMeta.DpiFactorY = dScrY;

			// Backward compatibility; configuration files created by KeePass
			// 2.37 and earlier do not contain DpiFactor* values, they default
			// to 0.0 and all positions and sizes are in pixels for the current
			// screen DPI; so, do not perform any DPI scaling in this case
			if((dCfgX == 0.0) || (dCfgY == 0.0)) return;

			double sX = dScrX / dCfgX, sY = dScrY / dCfgY;
			GFunc<int, int> fX = delegate(int x)
			{
				return (int)Math.Round((double)x * sX);
			};
			GFunc<int, int> fY = delegate(int y)
			{
				return (int)Math.Round((double)y * sY);
			};
			GFunc<string, string> fWsr = delegate(string strRect)
			{
				return UIUtil.ScaleWindowScreenRect(strRect, sX, sY);
			};
			GFunc<string, string> fVX = delegate(string strArray)
			{
				if(string.IsNullOrEmpty(strArray)) return strArray;

				try
				{
					int[] v = StrUtil.DeserializeIntArray(strArray);
					if(v == null) { Debug.Assert(false); return strArray; }

					for(int i = 0; i < v.Length; ++i)
						v[i] = (int)Math.Round((double)v[i] * sX);

					return StrUtil.SerializeIntArray(v);
				}
				catch(Exception) { Debug.Assert(false); }

				return strArray;
			};
			GAction<AceFont> fFont = delegate(AceFont f)
			{
				if(f == null) { Debug.Assert(false); return; }

				if(f.GraphicsUnit == GraphicsUnit.Pixel)
					f.Size = (float)(f.Size * sY);
			};

			AceMainWindow mw = this.MainWindow;
			AceUI ui = this.UI;

			if(mw.X != AppDefs.InvalidWindowValue) mw.X = fX(mw.X);
			if(mw.Y != AppDefs.InvalidWindowValue) mw.Y = fY(mw.Y);
			if(mw.Width != AppDefs.InvalidWindowValue) mw.Width = fX(mw.Width);
			if(mw.Height != AppDefs.InvalidWindowValue) mw.Height = fY(mw.Height);

			foreach(AceColumn c in mw.EntryListColumns)
			{
				if(c.Width >= 0) c.Width = fX(c.Width);
			}

			ui.DataViewerRect = fWsr(ui.DataViewerRect);
			ui.DataEditorRect = fWsr(ui.DataEditorRect);
			ui.CharPickerRect = fWsr(ui.CharPickerRect);
			ui.AutoTypeCtxRect = fWsr(ui.AutoTypeCtxRect);
			ui.AutoTypeCtxColumnWidths = fVX(ui.AutoTypeCtxColumnWidths);

			fFont(ui.StandardFont);
			fFont(ui.PasswordFont);
			fFont(ui.DataEditorFont);
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

		public void Apply(AceApplyFlags f)
		{
			AceApplication aceApp = this.Application; // m_aceApp might be null
			AceSecurity aceSec = this.Security; // m_sec might be null
			AceIntegration aceInt = this.Integration; // m_int might be null

			if((f & AceApplyFlags.Proxy) != AceApplyFlags.None)
				IOConnection.SetProxy(aceInt.ProxyType, aceInt.ProxyAddress,
					aceInt.ProxyPort, aceInt.ProxyAuthType, aceInt.ProxyUserName,
					aceInt.ProxyPassword);

			if((f & AceApplyFlags.Ssl) != AceApplyFlags.None)
				IOConnection.SslCertsAcceptInvalid = aceSec.SslCertsAcceptInvalid;

			if((f & AceApplyFlags.FileTransactions) != AceApplyFlags.None)
				FileTransactionEx.ExtraSafe = aceApp.FileTxExtra;
		}
	}

	[Flags]
	public enum AceApplyFlags
	{
		None = 0,
		Proxy = 0x1,
		Ssl = 0x2,
		FileTransactions = 0x4,

		All = 0x7FFF
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

		private double m_dDpiFactorX = 0.0; // See AppConfigEx.DpiScale()
		[DefaultValue(0.0)]
		public double DpiFactorX
		{
			get { return m_dDpiFactorX; }
			set { m_dDpiFactorX = value; }
		}

		private double m_dDpiFactorY = 0.0; // See AppConfigEx.DpiScale()
		[DefaultValue(0.0)]
		public double DpiFactorY
		{
			get { return m_dDpiFactorY; }
			set { m_dDpiFactorY = value; }
		}
	}
}
