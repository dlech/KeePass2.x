/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2024 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePassLib.Delegates;
using KeePassLib.Utility;

namespace KeePass.UI
{
	internal sealed class MenuItemLinks
	{
		private readonly Dictionary<ToolStripItem, ToolStripItem> m_dCopies =
			new Dictionary<ToolStripItem, ToolStripItem>();

		public void CreateCopy(ToolStripItemCollection tsicTarget,
			ToolStripItem tsiPosRef, bool bAfter, ToolStripMenuItem tsmiBase)
		{
			if(tsicTarget == null) { Debug.Assert(false); return; }
			if(tsmiBase == null) { Debug.Assert(false); return; }

			ToolStripMenuItem tsmi = new ToolStripMenuItem();

			string strName = tsmiBase.Name, strNameNew = null;
			if(!string.IsNullOrEmpty(strName))
			{
				if(strName.StartsWith("m_menu", StrUtil.CaseIgnoreCmp))
					strNameNew = "m_ctx" + strName.Substring(6);
			}
			if(!string.IsNullOrEmpty(strNameNew))
			{
				ToolStripItem[] v = tsicTarget.Find(strNameNew, true);
				if((v == null) || (v.Length == 0))
					tsmi.Name = strNameNew;
				else { Debug.Assert(false); }
			}
			else { Debug.Assert(false); }

			CreateLink(tsmi, tsmiBase, (tsmiBase.DropDownItems.Count == 0));

			int i, n = tsicTarget.Count;
			if(tsiPosRef == null) i = (bAfter ? n : 0);
			else
			{
				i = tsicTarget.IndexOf(tsiPosRef);
				if(i < 0) { Debug.Assert(false); i = n; }
				else if(bAfter) ++i;
			}

			tsicTarget.Insert(i, tsmi);

			foreach(ToolStripItem tsiSub in tsmiBase.DropDownItems)
			{
				ToolStripMenuItem tsmiSub = (tsiSub as ToolStripMenuItem);
				if(tsmiSub != null)
					CreateCopy(tsmi.DropDownItems, null, true, tsmiSub);
				else if(tsiSub is ToolStripSeparator)
					tsmi.DropDownItems.Add(new ToolStripSeparator());
				else { Debug.Assert(false); }
			}
		}

		public void CreateLink(ToolStripMenuItem tsmi, ToolStripMenuItem tsmiBase,
			bool bHandleClick)
		{
			if(tsmi == null) { Debug.Assert(false); return; }
			if(tsmiBase == null) { Debug.Assert(false); return; }

			tsmi.Text = tsmiBase.Text;
			tsmiBase.TextChanged += delegate(object sender, EventArgs e)
			{
				Debug.Assert(sender == tsmiBase);
				Debug.Assert(tsmi.Text != tsmiBase.Text);
				try { tsmi.Text = tsmiBase.Text; }
				catch(Exception) { Debug.Assert(false); }
			};

			tsmi.Image = tsmiBase.Image;
			Debug.Assert(object.ReferenceEquals(tsmi.Image, tsmiBase.Image));
			Debug.Assert(tsmiBase.ImageTransparentColor == Color.Empty);

			tsmi.Size = tsmiBase.Size;

			// Getting 'Visible' also checks parent
			tsmi.Available = tsmiBase.Available;
			tsmiBase.AvailableChanged += delegate(object sender, EventArgs e)
			{
				Debug.Assert(sender == tsmiBase);
				Debug.Assert(tsmi.Available != tsmiBase.Available);
				try { tsmi.Available = tsmiBase.Available; }
				catch(Exception) { Debug.Assert(false); }
			};

			VoidDelegate fCopyEnabled = delegate()
			{
				// Check (direct) owner state; see ToolStripItem.Enabled
				ToolStrip tsOwner = tsmiBase.Owner;
				if((tsOwner != null) && !tsOwner.Enabled)
				{
					Debug.Assert(false); // tsmiBase.Enabled unusable
					return;
				}
				tsmi.Enabled = tsmiBase.Enabled;
			};

			fCopyEnabled();
			tsmiBase.EnabledChanged += delegate(object sender, EventArgs e)
			{
				Debug.Assert(sender == tsmiBase);
				Debug.Assert(tsmi.Enabled != tsmiBase.Enabled);
				try { fCopyEnabled(); }
				catch(Exception) { Debug.Assert(false); }
			};

			string strSh = tsmiBase.ShortcutKeyDisplayString;
			if(!string.IsNullOrEmpty(strSh)) tsmi.ShortcutKeyDisplayString = strSh;

			if(bHandleClick)
			{
				Debug.Assert(tsmiBase.DropDownItems.Count == 0);
				tsmi.Click += delegate(object sender, EventArgs e)
				{
					Debug.Assert(sender == tsmi);
					try { tsmiBase.PerformClick(); }
					catch(Exception) { Debug.Assert(false); }
				};
			}

			Debug.Assert(!m_dCopies.ContainsKey(tsmiBase)); // One copy only
			m_dCopies[tsmiBase] = tsmi;
		}

		public void SetCopyAvailable(ToolStripMenuItem tsmiBase, bool bAvailable)
		{
			if(tsmiBase == null) { Debug.Assert(false); return; }

			ToolStripItem tsi;
			if(!m_dCopies.TryGetValue(tsmiBase, out tsi)) { Debug.Assert(false); return; }
			tsi.Available = bAvailable;
		}

		public void SetImage(ToolStripMenuItem tsmiBase, Image img)
		{
			if(tsmiBase == null) { Debug.Assert(false); return; }

			tsmiBase.Image = img;

			ToolStripItem tsi;
			if(!m_dCopies.TryGetValue(tsmiBase, out tsi)) { Debug.Assert(false); return; }
			tsi.Image = img;
		}
	}
}
