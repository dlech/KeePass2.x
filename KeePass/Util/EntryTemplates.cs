/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2010 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

using KeePass.Forms;
using KeePass.Resources;

using KeePassLib;

namespace KeePass.Util
{
	public sealed class TemplateEntryEventArgs : EventArgs
	{
		private PwEntry m_peTemplate;
		public PwEntry TemplateEntry { get { return m_peTemplate; } }

		private PwEntry m_pe;
		public PwEntry Entry { get { return m_pe; } }

		public TemplateEntryEventArgs(PwEntry peTemplate, PwEntry pe)
		{
			m_peTemplate = peTemplate;
			m_pe = pe;
		}
	}

	public static class EntryTemplates
	{
		private static ToolStripSplitButton m_btnItemsHost = null;
		private static List<ToolStripItem> m_vToolStripItems = new List<ToolStripItem>();

		public static event EventHandler<TemplateEntryEventArgs> EntryCreating;
		public static event EventHandler<TemplateEntryEventArgs> EntryCreated;

		public static void Init(ToolStripSplitButton btnHost)
		{
			if(btnHost == null) throw new ArgumentNullException("btnHost");
			m_btnItemsHost = btnHost;

			m_btnItemsHost.DropDownOpening += OnMenuOpening;
		}

		public static void Release()
		{
			if(m_btnItemsHost != null)
			{
				Clear();
				m_btnItemsHost.DropDownOpening -= OnMenuOpening;

				m_btnItemsHost = null;
			}
		}

		private static void AddSeparator()
		{
			ToolStripSeparator tsSep = new ToolStripSeparator();
			m_btnItemsHost.DropDownItems.Add(tsSep);
			m_vToolStripItems.Add(tsSep);
		}

		private static void AddItem(PwEntry pe)
		{
			if(pe == null) { Debug.Assert(false); return; }

			ToolStripMenuItem tsmi = new ToolStripMenuItem(pe.Strings.ReadSafe(
				PwDefs.TitleField));
			tsmi.Tag = pe;
			tsmi.Click += OnMenuExecute;

			Image img = null;
			PwDatabase pd = Program.MainForm.ActiveDatabase;
			if(pd != null)
			{
				if(!pe.CustomIconUuid.EqualsValue(PwUuid.Zero))
					img = pd.GetCustomIcon(pe.CustomIconUuid);
				if(img == null)
				{
					try { img = Program.MainForm.ClientIcons.Images[(int)pe.IconId]; }
					catch(Exception) { Debug.Assert(false); }
				}
			}
			if(img == null) img = KeePass.Properties.Resources.B16x16_KGPG_Key1;
			tsmi.Image = img;

			m_btnItemsHost.DropDownItems.Add(tsmi);
			m_vToolStripItems.Add(tsmi);
		}

		private static void Update()
		{
			Clear();

			PwDatabase pd = Program.MainForm.ActiveDatabase;
			if(pd == null) { Debug.Assert(false); return; }
			if(pd.IsOpen == false) { Debug.Assert(false); return; }
			if(pd.EntryTemplatesGroup.EqualsValue(PwUuid.Zero)) return;

			PwGroup pg = pd.RootGroup.FindGroup(pd.EntryTemplatesGroup, true);
			if(pg == null) { Debug.Assert(false); return; }
			if(pg.Entries.UCount == 0) return;

			AddSeparator();
			for(uint u = 0; u < Math.Min(pg.Entries.UCount, 30); ++u)
			{
				try { AddItem(pg.Entries.GetAt(u)); }
				catch(Exception) { Debug.Assert(false); }
			}
		}

		private static void Clear()
		{
			int nCount = m_vToolStripItems.Count;
			for(int i = 0; i < nCount; ++i)
			{
				int j = nCount - i - 1;
				ToolStripItem tsmi = m_vToolStripItems[j];
				tsmi.Click -= OnMenuExecute;
				m_btnItemsHost.DropDownItems.Remove(tsmi);
			}

			m_vToolStripItems.Clear();
		}

		private static void OnMenuExecute(object sender, EventArgs e)
		{
			ToolStripMenuItem tsmi = (sender as ToolStripMenuItem);
			if(tsmi == null) { Debug.Assert(false); return; }

			CreateEntry(tsmi.Tag as PwEntry);
		}

		private static void OnMenuOpening(object sender, EventArgs e)
		{
			Update();
		}

		private static void CreateEntry(PwEntry peTemplate)
		{
			if(peTemplate == null) { Debug.Assert(false); return; }

			PwDatabase pd = Program.MainForm.ActiveDatabase;
			if(pd == null) { Debug.Assert(false); return; }
			if(pd.IsOpen == false) { Debug.Assert(false); return; }

			PwGroup pgContainer = Program.MainForm.GetSelectedGroup();
			if(pgContainer == null) pgContainer = pd.RootGroup;

			PwEntry pe = peTemplate.CloneDeep();
			pe.Uuid = new PwUuid(true);
			pe.CreationTime = pe.LastModificationTime = pe.LastAccessTime = DateTime.Now;

			if(EntryTemplates.EntryCreating != null)
				EntryTemplates.EntryCreating(null, new TemplateEntryEventArgs(
					peTemplate.CloneDeep(), pe));

			PwEntryForm pef = new PwEntryForm();
			pef.InitEx(pe, PwEditMode.AddNewEntry, pd, Program.MainForm.ClientIcons,
				false, true);

			if(pef.ShowDialog() == DialogResult.OK)
			{
				pgContainer.AddEntry(pe, true, true);

				if(EntryTemplates.EntryCreated != null)
					EntryTemplates.EntryCreated(null, new TemplateEntryEventArgs(
						peTemplate.CloneDeep(), pe));

				// Program.MainForm.UpdateEntryList(null, true);
				// Program.MainForm.UpdateUIState(true);
				Program.MainForm.UpdateUI(false, null, pd.UINeedsIconUpdate, null,
					true, null, true);
			}
			else Program.MainForm.UpdateUI(false, null, pd.UINeedsIconUpdate, null,
				pd.UINeedsIconUpdate, null, false);
		}
	}
}
