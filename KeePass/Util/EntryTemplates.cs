/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2023 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using KeePass.Forms;
using KeePass.Resources;
using KeePass.UI;

using KeePassLib;
using KeePassLib.Delegates;
using KeePassLib.Utility;

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
		private static ToolStripDropDownItem g_tsiHost = null;
		private static List<ToolStripItem> g_lTopLevelItems = new List<ToolStripItem>();

		public static event EventHandler<TemplateEntryEventArgs> EntryCreating;
		public static event EventHandler<TemplateEntryEventArgs> EntryCreated;

		public static void Init(ToolStripSplitButton btnHost)
		{
			Release();
			if(btnHost == null) throw new ArgumentNullException("btnHost");

			g_tsiHost = btnHost;
			g_tsiHost.DropDownOpening += OnMenuOpening;
		}

		public static void Release()
		{
			if(g_tsiHost == null) return;

			Clear();

			g_tsiHost.DropDownOpening -= OnMenuOpening;
			g_tsiHost = null;
		}

		private static void Clear()
		{
			int n = g_lTopLevelItems.Count;
			if(n == 0) return;

			BlockLayout(true);

			ToolStripItemCollection tsic = g_tsiHost.DropDownItems;

			// The following is very slow
			// int iKnown = n - 1;
			// for(int iMenu = tsic.Count - 1; iMenu >= 0; --iMenu)
			// {
			//	if(tsic[iMenu] == g_lTopLevelItems[iKnown])
			//	{
			//		tsic.RemoveAt(iMenu);
			//		if(--iKnown < 0) break;
			//	}
			// }
			// Debug.Assert(iKnown == -1);

			List<ToolStripItem> lOthers = new List<ToolStripItem>();
			int iKnown = 0;
			for(int iMenu = 0; iMenu < tsic.Count; ++iMenu)
			{
				ToolStripItem tsi = tsic[iMenu];
				if((iKnown < n) && (tsi == g_lTopLevelItems[iKnown])) ++iKnown;
				else lOthers.Add(tsi);
			}
			Debug.Assert((iKnown == n) && ((lOthers.Count + n) == tsic.Count));
			tsic.Clear();
			tsic.AddRange(lOthers.ToArray()); // Restore others

			g_lTopLevelItems.Clear();

			BlockLayout(false);
		}

		private static void BlockLayout(bool bBlock)
		{
			ToolStrip ts = ((g_tsiHost != null) ? g_tsiHost.Owner : null);
			if(ts == null) { Debug.Assert(false); return; }

			if(bBlock) ts.SuspendLayout();
			else ts.ResumeLayout();
		}

		private static void OnMenuOpening(object sender, EventArgs e)
		{
			if(g_tsiHost == null) { Debug.Assert(false); return; }

			PwGroup pg = GetTemplatesGroup(null);
			if(pg == null) pg = new PwGroup();

			BlockLayout(true);

			Clear();

			ToolStripSeparator s = new ToolStripSeparator();
			g_tsiHost.DropDownItems.Add(s);
			g_lTopLevelItems.Add(s);

			List<ToolStripItem> l = CreateGroupItems(pg);
			g_tsiHost.DropDownItems.AddRange(l.ToArray());
			g_lTopLevelItems.AddRange(l);

			BlockLayout(false);
		}

		internal static PwGroup GetTemplatesGroup(PwDatabase pdIn)
		{
			PwDatabase pd = (pdIn ?? Program.MainForm.ActiveDatabase);
			if((pd == null) || !pd.IsOpen) { Debug.Assert(false); return null; }

			PwUuid pu = pd.EntryTemplatesGroup;
			if(pu.Equals(PwUuid.Zero)) return null;

			return pd.RootGroup.FindGroup(pu, true);
		}

		private static Image GetImage(PwIcon ic, PwUuid puCustom)
		{
			MainForm mf = Program.MainForm;
			PwDatabase pd = mf.ActiveDatabase;
			if((pd == null) || !pd.IsOpen) { Debug.Assert(false); return null; }

			ImageList il = mf.ClientIcons;
			if(il == null) { Debug.Assert(false); return null; }

			int i;
			if(puCustom.Equals(PwUuid.Zero)) i = (int)ic;
			else i = (int)PwIcon.Count + pd.GetCustomIconIndex(puCustom);

			if((i < 0) || (i >= il.Images.Count)) { Debug.Assert(false); return null; }
			return il.Images[i];
		}

		private static ToolStripMenuItem CreateEmptyItem()
		{
			ToolStripMenuItem tsmi = new ToolStripMenuItem("(" +
				KPRes.TemplatesNotFound + ")");
			tsmi.Enabled = false;
			return tsmi;
		}

		private static List<ToolStripItem> CreateGroupItems(PwGroup pg)
		{
			List<ToolStripItem> l = new List<ToolStripItem>();

			if(pg == null) { Debug.Assert(false); return l; }
			if(g_tsiHost == null) { Debug.Assert(false); return l; }

			bool bGroups = (pg.Groups.UCount != 0);
			bool bEntries = (pg.Entries.UCount != 0);
			if(!bGroups && !bEntries)
			{
				l.Add(CreateEmptyItem());
				return l;
			}

			AccessKeyManagerEx ak = new AccessKeyManagerEx();

			foreach(PwGroup pgSub in pg.Groups)
			{
				string strText = ak.CreateText(pgSub.Name, true);
				ToolStripMenuItem tsmi = new ToolStripMenuItem(strText);
				tsmi.Image = GetImage(pgSub.IconId, pgSub.CustomIconUuid);
				tsmi.Tag = pgSub;

				ToolStripMenuItem tsmiDynPlh = new ToolStripMenuItem("<>");
				tsmi.DropDownItems.Add(tsmiDynPlh); // Ensure popup arrow

				PwGroup pgSubCur = pgSub; // Copy the ref., as pgSub changes

				tsmi.DropDownOpening += delegate(object sender, EventArgs e)
				{
					Debug.Assert(object.ReferenceEquals(sender, tsmi));
					Debug.Assert(object.ReferenceEquals(tsmi.Tag, pgSubCur));

					ToolStripItemCollection tsic = tsmi.DropDownItems;
					if((tsic.Count == 1) && (tsic[0] == tsmiDynPlh))
					{
						BlockLayout(true);

						tsic.Clear();
						tsic.AddRange(CreateGroupItems(pgSubCur).ToArray());

						BlockLayout(false);
					}
				};

				l.Add(tsmi);
			}

			if(bGroups && bEntries) l.Add(new ToolStripSeparator());

			foreach(PwEntry pe in pg.Entries)
			{
				string strText = ak.CreateText(pe.Strings.ReadSafe(
					PwDefs.TitleField), true);
				ToolStripMenuItem tsmi = new ToolStripMenuItem(strText);
				tsmi.Image = GetImage(pe.IconId, pe.CustomIconUuid);
				tsmi.Tag = pe;
				tsmi.Click += OnMenuExecute;

				l.Add(tsmi);
			}

			return l;
		}

		private static void OnMenuExecute(object sender, EventArgs e)
		{
			ToolStripMenuItem tsmi = (sender as ToolStripMenuItem);
			if(tsmi == null) { Debug.Assert(false); return; }

			PwEntry peTemplate = (tsmi.Tag as PwEntry);
			if(peTemplate == null) { Debug.Assert(false); return; }

			MainForm mf = Program.MainForm;
			if(mf == null) { Debug.Assert(false); return; }

			PwDatabase pd = mf.ActiveDatabase;
			if((pd == null) || !pd.IsOpen) { Debug.Assert(false); return; }

			// Ensure that the correct database is still active
			if(pd != mf.DocumentManager.FindContainerOf(peTemplate)) return;

			GFunc<PwEntry> fNewEntry = delegate()
			{
				PwEntry pe = peTemplate.Duplicate();
				pe.History.Clear();
				return pe;
			};

			Action<PwEntry> fAddPre = delegate(PwEntry pe)
			{
				if(EntryTemplates.EntryCreating != null)
					EntryTemplates.EntryCreating(null, new TemplateEntryEventArgs(
						peTemplate.CloneDeep(), pe));
			};

			Action<PwEntry> fAddPost = delegate(PwEntry pe)
			{
				if(EntryTemplates.EntryCreated != null)
					EntryTemplates.EntryCreated(null, new TemplateEntryEventArgs(
						peTemplate.CloneDeep(), pe));
			};

			mf.AddEntryEx(null, fNewEntry, fAddPre, fAddPost);
		}
	}
}
