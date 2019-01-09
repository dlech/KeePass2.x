/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2019 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using System.Windows.Forms;

using KeePass.App;
using KeePass.App.Configuration;
using KeePass.Plugins;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Cryptography;
using KeePassLib.Delegates;
using KeePassLib.Interfaces;
using KeePassLib.Native;
using KeePassLib.Utility;

namespace KeePass.Plugins
{
	internal sealed class PluginManager : IEnumerable<PluginInfo>
	{
		private List<PluginInfo> m_vPlugins = new List<PluginInfo>();
		private IPluginHost m_host = null;

		private static string g_strUserDir = string.Empty;
		internal static string UserDirectory
		{
			get { return g_strUserDir; }
		}

		public void Initialize(IPluginHost host)
		{
			Debug.Assert(host != null);
			m_host = host;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return m_vPlugins.GetEnumerator();
		}
		
		public IEnumerator<PluginInfo> GetEnumerator()
		{
			return m_vPlugins.GetEnumerator();
		}

		internal void LoadAllPlugins()
		{
			string[] vExclNames = new string[] {
				AppDefs.FileNames.Program, AppDefs.FileNames.XmlSerializers,
				AppDefs.FileNames.NativeLib32, AppDefs.FileNames.NativeLib64,
				AppDefs.FileNames.ShInstUtil
			};

			string strAppDir = UrlUtil.GetFileDirectory(WinUtil.GetExecutable(),
				false, true);
			LoadAllPlugins(strAppDir, SearchOption.TopDirectoryOnly, vExclNames);
			g_strUserDir = strAppDir; // Preliminary, see below

			if(WinUtil.IsAppX)
			{
				string str = UrlUtil.EnsureTerminatingSeparator(
					AppConfigSerializer.AppDataDirectory, false) + AppDefs.PluginsDir;
				LoadAllPlugins(str, SearchOption.AllDirectories, vExclNames);

				g_strUserDir = str;
			}
			else if(!NativeLib.IsUnix())
			{
				string str = UrlUtil.EnsureTerminatingSeparator(strAppDir,
					false) + AppDefs.PluginsDir;
				LoadAllPlugins(str, SearchOption.AllDirectories, vExclNames);

				g_strUserDir = str;
			}
			else // Unix
			{
				try
				{
					DirectoryInfo diPlgRoot = new DirectoryInfo(strAppDir);
					foreach(DirectoryInfo diSub in diPlgRoot.GetDirectories())
					{
						if(diSub == null) { Debug.Assert(false); continue; }

						if(string.Equals(diSub.Name, AppDefs.PluginsDir,
							StrUtil.CaseIgnoreCmp))
						{
							LoadAllPlugins(diSub.FullName, SearchOption.AllDirectories,
								vExclNames);

							g_strUserDir = diSub.FullName;
						}
					}
				}
				catch(Exception) { Debug.Assert(false); }
			}
		}

		public void LoadAllPlugins(string strDir, SearchOption so, string[] vExclNames)
		{
			Debug.Assert(m_host != null);

			try
			{
				if(!Directory.Exists(strDir)) return; // No assert

				List<string> lDlls = UrlUtil.GetFilePaths(strDir, "*.dll", so);
				FilterList(lDlls, vExclNames);

				List<string> lExes = UrlUtil.GetFilePaths(strDir, "*.exe", so);
				FilterList(lExes, vExclNames);

				List<string> lPlgxs = UrlUtil.GetFilePaths(strDir, "*." +
					PlgxPlugin.PlgxExtension, so);
				FilterList(lPlgxs, vExclNames);

				FilterLists(lDlls, lExes, lPlgxs);

				LoadPlugins(lDlls, null, null, true);
				LoadPlugins(lExes, null, null, true);

				if(lPlgxs.Count != 0)
				{
					OnDemandStatusDialog dlgStatus = new OnDemandStatusDialog(true, null);
					dlgStatus.StartLogging(PwDefs.ShortProductName, false);

					try
					{
						foreach(string strFile in lPlgxs)
							PlgxPlugin.Load(strFile, dlgStatus);
					}
					finally { dlgStatus.EndLogging(); }
				}
			}
			catch(Exception) { Debug.Assert(false); } // Path access violation
		}

		public void LoadPlugin(string strFilePath, string strTypeName,
			string strDisplayFilePath, bool bSkipCacheFile)
		{
			if(strFilePath == null) throw new ArgumentNullException("strFilePath");

			List<string> l = new List<string>();
			l.Add(strFilePath);

			LoadPlugins(l, strTypeName, strDisplayFilePath, bSkipCacheFile);
		}

		private void LoadPlugins(List<string> lFiles, string strTypeName,
			string strDisplayFilePath, bool bSkipCacheFiles)
		{
			string strCacheRoot = UrlUtil.EnsureTerminatingSeparator(
				PlgxCache.GetCacheRoot(), false);

			foreach(string strFile in lFiles)
			{
				if(bSkipCacheFiles && strFile.StartsWith(strCacheRoot,
					StrUtil.CaseIgnoreCmp))
					continue;

				FileVersionInfo fvi = null;
				try
				{
					fvi = FileVersionInfo.GetVersionInfo(strFile);

					if((fvi == null) || (fvi.ProductName == null) ||
						(fvi.ProductName != AppDefs.PluginProductName))
					{
						continue;
					}
				}
				catch(Exception) { continue; }

				Exception exShowStd = null;
				try
				{
					string strHash = Convert.ToBase64String(CryptoUtil.HashSha256(
						strFile), Base64FormattingOptions.None);

					PluginInfo pi = new PluginInfo(strFile, fvi, strDisplayFilePath);
					pi.Interface = CreatePluginInstance(pi.FilePath, strTypeName);

					CheckCompatibility(strHash, pi.Interface);
					// CheckCompatibilityRefl(strFile);

					if(!pi.Interface.Initialize(m_host))
						continue; // Fail without error

					m_vPlugins.Add(pi);
				}
				catch(BadImageFormatException exBif)
				{
					if(Is1xPlugin(strFile))
						MessageService.ShowWarning(KPRes.PluginIncompatible +
							MessageService.NewLine + strFile + MessageService.NewParagraph +
							KPRes.Plugin1x + MessageService.NewParagraph + KPRes.Plugin1xHint);
					else exShowStd = exBif;
				}
				catch(Exception exLoad)
				{
					if(Program.CommandLineArgs[AppDefs.CommandLineOptions.Debug] != null)
						MessageService.ShowWarningExcp(strFile, exLoad);
					else exShowStd = exLoad;
				}

				if(exShowStd != null)
					ShowLoadError(strFile, exShowStd, null);
			}
		}

		internal static void ShowLoadError(string strPath, Exception ex,
			IStatusLogger slStatus)
		{
			if(string.IsNullOrEmpty(strPath)) { Debug.Assert(false); return; }

			if(slStatus != null)
				slStatus.SetText(KPRes.PluginLoadFailed, LogStatusType.Info);

			bool bShowExcp = (Program.CommandLineArgs[
				AppDefs.CommandLineOptions.Debug] != null);

			string strMsg = KPRes.PluginIncompatible + MessageService.NewLine +
				strPath + MessageService.NewParagraph + KPRes.PluginUpdateHint;
			string strExcp = ((ex != null) ? StrUtil.FormatException(ex).Trim() : null);

			VistaTaskDialog vtd = new VistaTaskDialog();
			vtd.Content = strMsg;
			vtd.ExpandedByDefault = ((strExcp != null) && bShowExcp);
			vtd.ExpandedInformation = strExcp;
			vtd.WindowTitle = PwDefs.ShortProductName;
			vtd.SetIcon(VtdIcon.Warning);

			if(!vtd.ShowDialog())
			{
				if(!bShowExcp) MessageService.ShowWarning(strMsg);
				else MessageService.ShowWarningExcp(strPath, ex);
			}
		}

		public void UnloadAllPlugins()
		{
			foreach(PluginInfo plugin in m_vPlugins)
			{
				Debug.Assert(plugin.Interface != null);
				if(plugin.Interface != null)
				{
					try { plugin.Interface.Terminate(); }
					catch(Exception) { Debug.Assert(false); }
				}
			}

			m_vPlugins.Clear();
		}

		private static Plugin CreatePluginInstance(string strFilePath,
			string strTypeName)
		{
			Debug.Assert(strFilePath != null);
			if(strFilePath == null) throw new ArgumentNullException("strFilePath");

			string strType;
			if(string.IsNullOrEmpty(strTypeName))
			{
				strType = UrlUtil.GetFileName(strFilePath);
				strType = UrlUtil.StripExtension(strType) + "." +
					UrlUtil.StripExtension(strType) + "Ext";
			}
			else strType = strTypeName + "." + strTypeName + "Ext";

			ObjectHandle oh = Activator.CreateInstanceFrom(strFilePath, strType);

			Plugin plugin = (oh.Unwrap() as Plugin);
			if(plugin == null) throw new FileLoadException();
			return plugin;
		}

		private static bool Is1xPlugin(string strFile)
		{
			try
			{
				byte[] pbFile = File.ReadAllBytes(strFile);
				byte[] pbSig = StrUtil.Utf8.GetBytes("KpCreateInstance");
				string strData = MemUtil.ByteArrayToHexString(pbFile);
				string strSig = MemUtil.ByteArrayToHexString(pbSig);

				return (strData.IndexOf(strSig) >= 0);
			}
			catch(Exception) { Debug.Assert(false); }

			return false;
		}

		private static void FilterList(List<string> l, string[] vExclNames)
		{
			if((l == null) || (vExclNames == null)) { Debug.Assert(false); return; }

			for(int i = l.Count - 1; i >= 0; --i)
			{
				string strName = UrlUtil.GetFileName(l[i]);
				if(string.IsNullOrEmpty(strName))
				{
					Debug.Assert(false);
					l.RemoveAt(i);
					continue;
				}

				// Ignore satellite assemblies
				if(strName.EndsWith(".resources.dll", StrUtil.CaseIgnoreCmp))
				{
					l.RemoveAt(i);
					continue;
				}

				foreach(string strExcl in vExclNames)
				{
					if(string.IsNullOrEmpty(strExcl)) { Debug.Assert(false); continue; }

					if(strName.Equals(strExcl, StrUtil.CaseIgnoreCmp))
					{
						l.RemoveAt(i);
						break;
					}
				}
			}
		}

		private static void FilterLists(List<string> lDlls, List<string> lExes,
			List<string> lPlgxs)
		{
			bool bPreferDll = Program.IsStableAssembly();

			for(int i = lDlls.Count - 1; i >= 0; --i)
			{
				string strDllPre = UrlUtil.StripExtension(lDlls[i]);

				for(int j = lPlgxs.Count - 1; j >= 0; --j)
				{
					string strPlgxPre = UrlUtil.StripExtension(lPlgxs[j]);

					if(string.Equals(strDllPre, strPlgxPre, StrUtil.CaseIgnoreCmp))
					{
						if(bPreferDll) lPlgxs.RemoveAt(j);
						else lDlls.RemoveAt(i);

						break;
					}
				}
			}
		}

		private static void CheckRefs(Module m, int iMdTokenType,
			GAction<Module, int> fCheck)
		{
			if((m == null) || (fCheck == null)) { Debug.Assert(false); return; }
			if((iMdTokenType & 0x00FFFFFF) != 0)
			{
				Debug.Assert(false); // Not a valid MetadataTokenType
				return;
			}
			if((iMdTokenType < 0) || (iMdTokenType == 0x7F000000))
			{
				Debug.Assert(false); // Loop below would need to be adjusted
				return;
			}

			try
			{
				// https://msdn.microsoft.com/en-us/library/ms404456(v=vs.100).aspx
				// https://docs.microsoft.com/en-us/dotnet/standard/metadata-and-self-describing-components
				int s = iMdTokenType | 1; // RID = 0 <=> 'nil token'
				int e = iMdTokenType | 0x00FFFFFF;

				for(int i = s; i <= e; ++i) fCheck(m, i);
			}
			catch(ArgumentOutOfRangeException) { } // End of metadata table
			catch(ArgumentException) { Debug.Assert(false); }
			// Other exceptions indicate an unresolved reference
		}

		private static void CheckTypeRef(Module m, int iMdToken)
		{
			// ResolveType should throw exception for unresolvable token
			// if(m.ResolveType(iMdToken) == null) { Debug.Assert(false); }

			// ResolveType should throw exception for unresolvable token
			Type t = m.ResolveType(iMdToken);
			if(t == null) { Debug.Assert(false); return; }

			if(t.Assembly == typeof(PluginManager).Assembly)
			{
				if(t.IsNotPublic || t.IsNestedPrivate || t.IsNestedAssembly ||
					t.IsNestedFamANDAssem)
					throw new UnauthorizedAccessException("Ref.: " + t.ToString() + ".");
			}
		}

		private static void CheckMemberRef(Module m, int iMdToken)
		{
			// ResolveMember should throw exception for unresolvable token
			// if(m.ResolveMember(iMdToken) == null) { Debug.Assert(false); }

			// ResolveMember should throw exception for unresolvable token
			MemberInfo mi = m.ResolveMember(iMdToken);
			if(mi == null) { Debug.Assert(false); return; }

			if(mi.Module == typeof(PluginManager).Module)
			{
				MethodBase mb = (mi as MethodBase);
				if(mb != null)
				{
					if(mb.IsPrivate || mb.IsAssembly || mb.IsFamilyAndAssembly)
						ThrowRefAccessExcp(mb);
					return;
				}

				FieldInfo fi = (mi as FieldInfo);
				if(fi != null)
				{
					if(fi.IsPrivate || fi.IsAssembly || fi.IsFamilyAndAssembly)
						ThrowRefAccessExcp(fi);
					return;
				}

				Debug.Assert(false); // Unknown member reference type
			}
		}

		private static void ThrowRefAccessExcp(MemberInfo mi)
		{
			string str = "Ref.: ";

			try
			{
				Type t = mi.DeclaringType;
				if(t != null) str += t.ToString() + " -> ";
			}
			catch(Exception) { Debug.Assert(false); }

			throw new MemberAccessException(str + mi.ToString() + ".");
		}

		private static void CheckCompatibilityPriv(Plugin p)
		{
			// When trying to resolve a non-existing token, Mono
			// terminates the whole process with a SIGABRT instead
			// of just throwing an ArgumentOutOfRangeException
			if(MonoWorkarounds.IsRequired(9604)) return;

			Assembly asm = p.GetType().Assembly;
			if(asm == typeof(PluginManager).Assembly) { Debug.Assert(false); return; }

			foreach(Module m in asm.GetModules())
			{
				// MetadataTokenType.TypeRef = 0x01000000
				CheckRefs(m, 0x01000000, CheckTypeRef);

				// MetadataTokenType.MemberRef = 0x0A000000
				CheckRefs(m, 0x0A000000, CheckMemberRef);
			}
		}

		private static void CheckCompatibility(string strHash, Plugin p)
		{
			AceApplication aceApp = Program.Config.Application;
			// bool? ob = aceApp.GetPluginCompat(strHash);
			// if(ob.HasValue) return ob.Value;
			if(aceApp.IsPluginCompat(strHash)) return;

			CheckCompatibilityPriv(p);

			aceApp.SetPluginCompat(strHash);
		}

		/* private static void CheckCompatibilityRefl(string strFile)
		{
			ResolveEventHandler eh = delegate(object sender, ResolveEventArgs e)
			{
				string strName = e.Name;
				if(strName.Equals("KeePass", StrUtil.CaseIgnoreCmp) ||
					strName.StartsWith("KeePass,", StrUtil.CaseIgnoreCmp))
					return Assembly.ReflectionOnlyLoadFrom(WinUtil.GetExecutable());

				return Assembly.ReflectionOnlyLoad(strName);
			};

			AppDomain d = AppDomain.CurrentDomain;
			d.ReflectionOnlyAssemblyResolve += eh;
			try
			{
				Assembly asm = Assembly.ReflectionOnlyLoadFrom(strFile);
				asm.GetTypes();
			}
			finally { d.ReflectionOnlyAssemblyResolve -= eh; }
		} */

		internal void AddMenuItems(PluginMenuType t, ToolStripItemCollection c,
			ToolStripItem tsiPrev)
		{
			if(c == null) { Debug.Assert(false); return; }

			List<ToolStripItem> l = new List<ToolStripItem>();
			foreach(PluginInfo pi in m_vPlugins)
			{
				if(pi == null) { Debug.Assert(false); continue; }

				Plugin p = pi.Interface;
				if(p == null) { Debug.Assert(false); continue; }

				ToolStripMenuItem tsmi = p.GetMenuItem(t);
				if(tsmi != null)
				{
					// string strTip = tsmi.ToolTipText;
					// if((strTip == null) || (strTip == tsmi.Text))
					//	strTip = string.Empty;
					// if(strTip.Length != 0) strTip += MessageService.NewParagraph;
					// strTip += KPRes.Plugin + ": " + pi.Name;
					// tsmi.ToolTipText = strTip;

					l.Add(tsmi);
				}
			}
			if(l.Count == 0) return;

			int iPrev = ((tsiPrev != null) ? c.IndexOf(tsiPrev) : -1);
			if(iPrev < 0) { Debug.Assert(false); iPrev = c.Count - 1; }
			int iIns = iPrev + 1;

			l.Sort(PluginManager.CompareToolStripItems);
			if((iPrev >= 0) && (iPrev < c.Count) && !(c[iPrev] is ToolStripSeparator))
				l.Insert(0, new ToolStripSeparator());
			if((iIns < c.Count) && !(c[iIns] is ToolStripSeparator))
				l.Add(new ToolStripSeparator());

			if(iIns == c.Count) c.AddRange(l.ToArray());
			else
			{
				for(int i = 0; i < l.Count; ++i)
					c.Insert(iIns + i, l[i]);
			}
		}

		private static int CompareToolStripItems(ToolStripItem x,
			ToolStripItem y)
		{
			return string.Compare(x.Text, y.Text, StrUtil.CaseIgnoreCmp);
		}
	}
}
