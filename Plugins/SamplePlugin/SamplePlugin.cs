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
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;

using KeePass.Plugins;
using KeePass.Forms;
using KeePass.Resources;

using KeePassLib;
using KeePassLib.Security;

// The namespace name must be the same as the filename of the
// plugin without its extension.
// For example, if you compile a plugin 'SamplePlugin.dll', the
// namespace must be named 'SamplePlugin'.
namespace SamplePlugin
{
	/// <summary>
	/// This is the main plugin class. It must be named exactly
	/// like the namespace and must be derived from
	/// <c>KeePassPlugin</c>.
	/// </summary>
	public sealed class SamplePlugin : KeePassPlugin
	{
		// The sample plugin remembers its host in this variable.
		private IKeePassPluginHost m_host = null;

		private ToolStripSeparator m_tsSeparator = null;
		private ToolStripMenuItem m_tsmiPopup = null;
		private ToolStripMenuItem m_tsmiAddGroups = null;
		private ToolStripMenuItem m_tsmiAddEntries = null;

		/// <summary>
		/// The <c>Initialize</c> function is called by KeePass when
		/// you should initialize your plugin (create menu items, etc.).
		/// </summary>
		/// <param name="host">Plugin host interface. By using this
		/// interface, you can access the KeePass main window and the
		/// currently opened database.</param>
		/// <returns>You must return <c>true</c> in order to signal
		/// successful initialization. If you return <c>false</c>,
		/// KeePass unloads your plugin (without calling the
		/// <c>Terminate</c> function of your plugin).</returns>
		public override bool Initialize(IKeePassPluginHost host)
		{
			Debug.Assert(host != null);
			if(host == null) return false;
			m_host = host;

			// Get a reference to the 'Tools' menu item container
			ToolStripItemCollection tsMenu = m_host.MainWindow.ToolsMenu.DropDownItems;

			// Add a separator at the bottom
			m_tsSeparator = new ToolStripSeparator();
			tsMenu.Add(m_tsSeparator);

			// Add the popup menu item
			m_tsmiPopup = new ToolStripMenuItem();
			m_tsmiPopup.Text = "Sample Plugin for Developers";
			tsMenu.Add(m_tsmiPopup);

			// Add menu item 'Add Some Groups'
			m_tsmiAddGroups = new ToolStripMenuItem();
			m_tsmiAddGroups.Text = "Add Some Groups";
			m_tsmiAddGroups.Click += OnMenuAddGroups;
			m_tsmiPopup.DropDownItems.Add(m_tsmiAddGroups);

			// Add menu item 'Add Some Entries'
			m_tsmiAddEntries = new ToolStripMenuItem();
			m_tsmiAddEntries.Text = "Add Some Entries";
			m_tsmiAddEntries.Click += OnMenuAddEntries;
			m_tsmiPopup.DropDownItems.Add(m_tsmiAddEntries);

			// We want a notification when the user tried to save the
			// current database
			m_host.MainWindow.FileSaved += OnFileSaved;

			return true; // Initialization successful
		}

		/// <summary>
		/// The <c>Terminate</c> function is called by KeePass when
		/// you should free all resources, close open files/streams,
		/// etc. It is also recommended that you remove all your
		/// plugin menu items from the KeePass menu.
		/// </summary>
		public override void Terminate()
		{
			// Remove all of our menu items
			ToolStripItemCollection tsMenu = m_host.MainWindow.ToolsMenu.DropDownItems;
			tsMenu.Remove(m_tsSeparator);
			tsMenu.Remove(m_tsmiPopup);
			tsMenu.Remove(m_tsmiAddGroups);
			tsMenu.Remove(m_tsmiAddEntries);

			// Important! Remove event handlers!
			m_host.MainWindow.FileSaved -= OnFileSaved;
		}

		private void OnMenuAddGroups(object sender, EventArgs e)
		{
			if(!m_host.Database.IsOpen)
			{
				MessageBox.Show("You first need to open a database!", "Sample Plugin");
				return;
			}

			Random r = new Random((int)DateTime.Now.Ticks);

			// Create 10 groups
			for(int i = 0; i < 10; ++i)
			{
				// A new group with a random icon
				PwGroup pg = new PwGroup(true, true, "Sample Group #" + i.ToString(),
					(PwIcon)r.Next(0, (int)(PwIcon.Count - 1)));

				// Important! Set the parent group pointer of the new group
				// to the correct container group!
				pg.ParentGroup = m_host.Database.RootGroup;

				// Finally add our new group to an existing group as subgroup
				m_host.Database.RootGroup.Groups.Add(pg);
			}

			// Select the root group in which we created the new subgroups
			m_host.MainWindow.UpdateGroupList(false, m_host.Database.RootGroup);

			// Recreate the password list window
			m_host.MainWindow.UpdateEntryList(null, false);

			// Finally update the UI state. By passing 'true' as first
			// parameter, we mark the database as modified.
			m_host.MainWindow.UpdateUIState(true);
		}

		private void OnMenuAddEntries(object sender, EventArgs e)
		{
			if(!m_host.Database.IsOpen)
			{
				MessageBox.Show("You first need to open a database!", "Sample Plugin");
				return;
			}

			// Create 10 groups
			for(int i = 0; i < 10; ++i)
			{
				// Create a new entry. Here's the same as with groups: you
				// need to set the parent container group (the group that
				// contains the new entry; first parameter)
				PwEntry pe = new PwEntry(m_host.Database.RootGroup, true, true);

				// Set some of the string fields
				pe.Strings.Set(PwDefs.TitleField, new ProtectedString(false, "Sample Entry"));
				pe.Strings.Set(PwDefs.UserNameField, new ProtectedString(false,
					Guid.NewGuid().ToString()));

				// Finally tell the parent group that it owns this entry now
				m_host.Database.RootGroup.Entries.Add(pe);
			}

			// Select the root group in which we created the new entries
			m_host.MainWindow.UpdateGroupList(false, m_host.Database.RootGroup);

			// Recreate the password list window
			m_host.MainWindow.UpdateEntryList(null, false);

			// Finally update the UI state. By passing 'true' as first
			// parameter, we mark the database as modified.
			m_host.MainWindow.UpdateUIState(true);
		}

		private void OnFileSaved(object sender, MainForm.FileSavedEventArgs e)
		{
			MessageBox.Show("Notification received: the user has tried to save the current database to:\r\n" +
				m_host.Database.IOConnectionInfo.Url + "\r\n\r\nResult:\r\n" +
				ResUtil.FileSaveResultToString(e.FileSaveResult),
				"Sample Plugin", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
	}
}
