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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Runtime.Remoting;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

using KeePass.App;
using KeePass.Resources;
using KeePass.Plugins;
using KeePass.UI;

using KeePassLib;
using KeePassLib.Utility;

namespace KeePass.Plugins
{
	internal sealed class PluginManager : IEnumerable<PluginInfo>
	{
		private List<PluginInfo> m_vPlugins = new List<PluginInfo>();
		private IPluginHost m_host = null;

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

		public void LoadAllPlugins(string strDirectory)
		{
			Debug.Assert(m_host != null);

			try
			{
				string strPath = strDirectory;
				if(Directory.Exists(strPath) == false)
				{
					Debug.Assert(false);
					return;
				}

				DirectoryInfo di = new DirectoryInfo(strPath);

				FileInfo[] vFiles = di.GetFiles("*.dll", SearchOption.AllDirectories);
				LoadPlugins(vFiles, null, null, true);

				vFiles = di.GetFiles("*.exe", SearchOption.AllDirectories);
				LoadPlugins(vFiles, null, null, true);

				vFiles = di.GetFiles("*." + PlgxPlugin.PlgxExtension, SearchOption.AllDirectories);
				if(vFiles.Length > 0)
				{
					OnDemandStatusDialog dlgStatus = new OnDemandStatusDialog(true, null);
					dlgStatus.StartLogging(PwDefs.ShortProductName, false);

					foreach(FileInfo fi in vFiles) PlgxPlugin.Load(fi.FullName, dlgStatus);

					dlgStatus.EndLogging();
				}
			}
			catch(Exception) { Debug.Assert(false); } // Path access violation
		}

		public void LoadPlugin(string strFilePath, string strTypeName,
			string strDisplayFilePath, bool bSkipCacheFile)
		{
			if(strFilePath == null) throw new ArgumentNullException("strFilePath");

			LoadPlugins(new FileInfo[] { new FileInfo(strFilePath) }, strTypeName,
				strDisplayFilePath, bSkipCacheFile);
		}

		private void LoadPlugins(FileInfo[] vFiles, string strTypeName,
			string strDisplayFilePath, bool bSkipCacheFiles)
		{
			string strCacheRoot = PlgxCache.GetCacheRoot();

			foreach(FileInfo fi in vFiles)
			{
				if(bSkipCacheFiles && fi.FullName.StartsWith(strCacheRoot,
					StrUtil.CaseIgnoreCmp))
					continue;

				FileVersionInfo fvi = null;
				try
				{
					fvi = FileVersionInfo.GetVersionInfo(fi.FullName);

					if((fvi == null) || (fvi.ProductName == null) ||
						(fvi.ProductName != AppDefs.PluginProductName))
					{
						continue;
					}
				}
				catch(Exception) { continue; }

				bool bShowStandardError = false;
				try
				{
					PluginInfo pi = new PluginInfo(fi.FullName, fvi, strDisplayFilePath);

					pi.Interface = CreatePluginInstance(pi.FilePath, strTypeName);

					if(pi.Interface.Initialize(m_host) == false)
						continue; // Fail without error

					m_vPlugins.Add(pi);
				}
				catch(BadImageFormatException)
				{
					if(Is1xPlugin(fi.FullName))
						MessageService.ShowWarning(KPRes.PluginIncompatible +
							MessageService.NewLine + fi.FullName + MessageService.NewParagraph +
							KPRes.Plugin1x + MessageService.NewParagraph + KPRes.Plugin1xHint);
					else bShowStandardError = true;
				}
				catch(Exception exLoad)
				{
					if(Program.CommandLineArgs[AppDefs.CommandLineOptions.Debug] != null)
						MessageService.ShowWarningExcp(fi.FullName, exLoad);
					else bShowStandardError = true;
				}

				if(bShowStandardError)
					MessageService.ShowWarning(KPRes.PluginIncompatible +
						MessageService.NewLine + fi.FullName + MessageService.NewParagraph +
						KPRes.PluginUpdateHint);
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
	}
}
