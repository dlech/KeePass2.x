/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2007 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Reflection;
using System.Runtime.Remoting;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

using KeePass.App;
using KeePass.Resources;

using KeePassLib;
using KeePassLib.Utility;

namespace KeePass.Plugins
{
	public sealed class PluginInfo
	{
		public string FileName = string.Empty;
		public KeePassPlugin Interface = null;

		public string FileVersion = string.Empty;

		public string Name = string.Empty;
		public string Description = string.Empty;
		public string Author = string.Empty;
	}

	public sealed class PluginManager
	{
		private List<PluginInfo> m_vPlugins = new List<PluginInfo>();
		private IKeePassPluginHost m_host = null;

		public IEnumerable<PluginInfo> Plugins
		{
			get { return m_vPlugins; }
		}

		public uint PluginCount
		{
			get { return (uint)m_vPlugins.Count; }
		}

		public void Init(IKeePassPluginHost host)
		{
			Debug.Assert(host != null);
			m_host = host;
		}

		private static KeePassPlugin CreatePluginInstance(string strFilePath)
		{
			KeePassPlugin iPlugin = null;

			string strType = UrlUtil.GetFileName(strFilePath);
			strType = UrlUtil.StripExtension(strType) + "." + UrlUtil.StripExtension(strType);

			try
			{
				ObjectHandle oh = Activator.CreateInstanceFrom(strFilePath, strType);
				iPlugin = oh.Unwrap() as KeePassPlugin;
			}
			catch(Exception) { iPlugin = null; }

			return iPlugin;
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

			foreach(FileInfo fi in vFiles)
			{
				PluginInfo pi = new PluginInfo();
				pi.FileName = fi.FullName;

				try
				{
					FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(pi.FileName);
					if(fvi == null) throw new ArgumentNullException();

					if((fvi.ProductName == null) || (fvi.ProductName != AppDefs.PluginProductName))
						continue;

					pi.FileVersion = fvi.FileVersion;

					pi.Name = fvi.FileDescription;
					pi.Description = fvi.Comments;
					pi.Author = fvi.CompanyName;
				}
				catch(Exception) { continue; }

				pi.Interface = CreatePluginInstance(pi.FileName);

				if(pi.Interface != null)
				{
					try
					{
						if(pi.Interface.Initialize(m_host))
							m_vPlugins.Add(pi); // Add the plugin to the list
					}
					catch(Exception) { ShowLoadWarning(pi.FileName, false); }
				}
				else ShowLoadWarning(pi.FileName, true);
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
					catch(Exception) { }

					plugin.Interface = null;
				}
			}

			m_vPlugins.Clear();
		}

		private void ShowLoadWarning(string strFile, bool bVersionIncompatible)
		{
			string strMsg = KPRes.PluginFailedToLoad + "\r\n";
			strMsg += strFile;

			if(bVersionIncompatible)
				strMsg += "\r\n\r\n" + KPRes.PluginVersionIncompatible;

			MessageBox.Show(strMsg, PwDefs.ShortProductName, MessageBoxButtons.OK,
				MessageBoxIcon.Warning);
		}
	}
}
