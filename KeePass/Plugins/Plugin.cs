/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2021 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace KeePass.Plugins
{
	/// <summary>
	/// KeePass plugin base class. All KeePass plugins must derive
	/// from this class.
	/// </summary>
	public abstract class Plugin
	{
		/// <summary>
		/// The <c>Initialize</c> method is called by KeePass when
		/// you should initialize your plugin.
		/// </summary>
		/// <param name="host">Plugin host interface. Through this
		/// interface you can access the KeePass main window, the
		/// currently open database, etc.</param>
		/// <returns>You must return <c>true</c> in order to signal
		/// successful initialization. If you return <c>false</c>,
		/// KeePass unloads your plugin (without calling the
		/// <c>Terminate</c> method of your plugin).</returns>
		public virtual bool Initialize(IPluginHost host)
		{
			return (host != null);
		}

		/// <summary>
		/// The <c>Terminate</c> method is called by KeePass when
		/// you should free all resources, close files/streams,
		/// remove event handlers, etc.
		/// </summary>
		public virtual void Terminate()
		{
		}

		/// <summary>
		/// Get a small icon representing the plugin.
		/// The image's width should be <c>DpiUtil.ScaleIntX(16)</c>,
		/// and its height should be <c>DpiUtil.ScaleIntY(16)</c>.
		/// </summary>
		public virtual Image SmallIcon
		{
			get { return null; }
		}

		/// <summary>
		/// Get the URL of a version information file. See
		/// https://keepass.info/help/v2_dev/plg_index.html#upd
		/// </summary>
		public virtual string UpdateUrl
		{
			get { return null; }
		}

		/// <summary>
		/// Get a menu item of the plugin. See
		/// https://keepass.info/help/v2_dev/plg_index.html#co_menuitem
		/// </summary>
		/// <param name="t">Type of the menu that the plugin should
		/// return an item for.</param>
		public virtual ToolStripMenuItem GetMenuItem(PluginMenuType t)
		{
			return null;
		}
	}

	public enum PluginMenuType
	{
		/// <summary>
		/// Main menu item of the plugin, which KeePass typically
		/// shows in the 'Tools' menu.
		/// </summary>
		Main = 0,

		/// <summary>
		/// Group menu item of the plugin, which KeePass typically
		/// shows in the context menu of a group.
		/// </summary>
		Group,

		/// <summary>
		/// Entry menu item of the plugin, which KeePass typically
		/// shows in the context menu of an entry.
		/// </summary>
		Entry,

		/// <summary>
		/// Tray menu item of the plugin, which KeePass typically
		/// shows in the context menu of its system tray icon.
		/// </summary>
		Tray
	}
}
