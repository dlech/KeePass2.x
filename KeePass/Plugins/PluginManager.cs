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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

using KeePass.App;
using KeePass.Resources;
using KeePass.Plugins;

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

			string strPath = strDirectory;
			if(Directory.Exists(strPath) == false)
			{
				Debug.Assert(false);
				return;
			}

			DirectoryInfo di = new DirectoryInfo(strPath);
			
			FileInfo[] vFiles = di.GetFiles("*.dll", SearchOption.AllDirectories);
			LoadPlugins(vFiles);

			vFiles = di.GetFiles("*.exe", SearchOption.AllDirectories);
			LoadPlugins(vFiles);
		}

		private void LoadPlugins(FileInfo[] vFiles)
		{
			foreach(FileInfo fi in vFiles)
			{
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

				try
				{
					PluginInfo pi = new PluginInfo(fi.FullName, fvi);

					pi.Interface = CreatePluginInstance(pi.FilePath);

					if(pi.Interface.Initialize(m_host) == false)
						continue; // Fail without error

					m_vPlugins.Add(pi);
				}
				catch(Exception excp)
				{
					MessageService.ShowWarning(KPRes.PluginFailedToLoad +
						MessageService.NewLine + fi.FullName, excp);
				}
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

		private static Plugin CreatePluginInstance(string strFilePath)
		{
			Debug.Assert(strFilePath != null);
			if(strFilePath == null) throw new ArgumentNullException("strFilePath");

			string strType = UrlUtil.GetFileName(strFilePath);
			strType = UrlUtil.StripExtension(strType) + "." +
				UrlUtil.GetExtension("." + UrlUtil.StripExtension(strType)) +
				"Ext";

			ObjectHandle oh = Activator.CreateInstanceFrom(strFilePath, strType);
			
			Plugin plugin = oh.Unwrap() as Plugin;
			if(plugin == null) throw new FileLoadException();
			return plugin;
		}
	}
}
