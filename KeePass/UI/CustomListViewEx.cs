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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using KeePass.Native;

using KeePassLib.Utility;

using NativeLib = KeePassLib.Native.NativeLib;

namespace KeePass.UI
{
	public sealed class CustomListViewEx : ListView
	{
		private int m_cUpdating = 0;
		private IComparer m_cmpUpdatingPre = null;
		private SortOrder m_soUpdatingPre = SortOrder.None;

		private ContextMenuStrip m_ctxHeader = null;
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DefaultValue((object)null)]
		internal ContextMenuStrip HeaderContextMenuStrip
		{
			get { return m_ctxHeader; }
			set { m_ctxHeader = value; }
		}

		private bool m_bAltItemStyles = false;
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DefaultValue(false)]
		internal bool UseAlternatingItemStyles
		{
			get { return m_bAltItemStyles; }
			set { m_bAltItemStyles = value; }
		}

		public CustomListViewEx() : base()
		{
			if(Program.DesignMode) return;

			try { this.DoubleBuffered = true; }
			catch(Exception) { Debug.Assert(false); }
		}

#if DEBUG
		protected override void Dispose(bool disposing)
		{
			Debug.Assert(m_cUpdating == 0);

			base.Dispose(disposing);
		}
#endif

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			if(Program.DesignMode) return;

			UIUtil.ConfigureToolTip(this);
		}

		/* private Color m_clrPrev = Color.Black;
		private Point m_ptLast = new Point(-1, -1);
		protected override void OnMouseHover(EventArgs e)
		{
			if((m_ptLast.X >= 0) && (m_ptLast.X < this.Columns.Count) &&
				(m_ptLast.Y >= 0) && (m_ptLast.Y < this.Items.Count))
			{
				this.Items[m_ptLast.Y].SubItems[m_ptLast.X].ForeColor = m_clrPrev;
			}

			ListViewHitTestInfo lh = this.HitTest(this.PointToClient(Cursor.Position));
			if((lh.Item != null) && (lh.SubItem != null))
			{
				m_ptLast = new Point(lh.Item.SubItems.IndexOf(lh.SubItem),
					lh.Item.Index);
				m_clrPrev = lh.SubItem.ForeColor;

				lh.SubItem.ForeColor = Color.LightBlue;
			}

			base.OnMouseHover(e);
		} */

		/* protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);
			UnfocusGroupInSingleMode();
		}
		private void UnfocusGroupInSingleMode()
		{
			try
			{
				if(!WinUtil.IsAtLeastWindowsVista) return;
				if(KeePassLib.Native.NativeLib.IsUnix()) return;
				if(!this.ShowGroups) return;
				if(this.MultiSelect) return;

				const uint m = (NativeMethods.LVGS_FOCUSED | NativeMethods.LVGS_SELECTED);

				uint uGroups = (uint)this.Groups.Count;
				for(uint u = 0; u < uGroups; ++u)
				{
					int iGroupID;
					if(NativeMethods.GetGroupStateByIndex(this, u, m,
						out iGroupID) == m)
					{
						NativeMethods.SetGroupState(this, iGroupID, m,
							NativeMethods.LVGS_SELECTED);
						return;
					}
				}
			}
			catch(Exception) { Debug.Assert(false); }
		} */

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if(UIUtil.HandleCommonKeyEvent(e, true, this)) return;
			if(HandleRenameKeyEvent(e, true)) return;

			try { if(SkipGroupHeaderIfRequired(e)) return; }
			catch(Exception) { Debug.Assert(false); }

			base.OnKeyDown(e);
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			if(UIUtil.HandleCommonKeyEvent(e, false, this)) return;
			if(HandleRenameKeyEvent(e, false)) return;

			base.OnKeyUp(e);
		}

		private bool SkipGroupHeaderIfRequired(KeyEventArgs e)
		{
			if(!UIUtil.GetGroupsEnabled(this)) return false;
			if(this.MultiSelect) return false;

			if(MonoWorkarounds.IsRequired(836428016)) return false;

			ListViewItem lvi = this.FocusedItem;
			if(lvi != null)
			{
				ListViewGroup g = lvi.Group;
				ListViewItem lviChangeTo = null;

				if((e.KeyCode == Keys.Up) && IsFirstLastItemInGroup(g, lvi, true))
					lviChangeTo = (GetNextLvi(g, true) ?? lvi); // '??' for top item
				else if((e.KeyCode == Keys.Down) && IsFirstLastItemInGroup(g, lvi, false))
					lviChangeTo = (GetNextLvi(g, false) ?? lvi); // '??' for bottom item

				if(lviChangeTo != null)
				{
					foreach(ListViewItem lviEnum in this.Items)
						lviEnum.Selected = false;

					EnsureVisible(lviChangeTo.Index);
					UIUtil.SetFocusedItem(this, lviChangeTo, true);

					UIUtil.SetHandled(e, true);
					return true;
				}
			}

			return false;
		}

		private static bool IsFirstLastItemInGroup(ListViewGroup g,
			ListViewItem lvi, bool bFirst)
		{
			if(g == null) { Debug.Assert(false); return false; }

			ListViewItemCollection c = g.Items;
			if(c.Count == 0) { Debug.Assert(false); return false; }

			return (bFirst ? (c[0] == lvi) : (c[c.Count - 1] == lvi));
		}

		private ListViewItem GetNextLvi(ListViewGroup gBaseExcl, bool bUp)
		{
			if(gBaseExcl == null) { Debug.Assert(false); return null; }

			int i = this.Groups.IndexOf(gBaseExcl);
			if(i < 0) { Debug.Assert(false); return null; }

			if(bUp)
			{
				--i;
				while(i >= 0)
				{
					ListViewGroup g = this.Groups[i];
					if(g.Items.Count > 0) return g.Items[g.Items.Count - 1];

					--i;
				}
			}
			else // Down
			{
				++i;
				int nGroups = this.Groups.Count;
				while(i < nGroups)
				{
					ListViewGroup g = this.Groups[i];
					if(g.Items.Count > 0) return g.Items[0];

					++i;
				}
			}

			return null;
		}

		private bool HandleRenameKeyEvent(KeyEventArgs e, bool bDown)
		{
			try
			{
				if((e.KeyData == Keys.F2) && this.LabelEdit)
				{
					ListView.SelectedListViewItemCollection lvsic = this.SelectedItems;
					if(lvsic.Count >= 1)
					{
						UIUtil.SetHandled(e, true);
						if(bDown) lvsic[0].BeginEdit();
						return true;
					}
				}
			}
			catch(Exception) { Debug.Assert(false); }

			return false;
		}

		/* protected override void WndProc(ref Message m)
		{
			if(m.Msg == NativeMethods.WM_NOTIFY)
			{
				NativeMethods.NMHDR nm = (NativeMethods.NMHDR)m.GetLParam(
					typeof(NativeMethods.NMHDR));
				if(nm.code == NativeMethods.NM_RCLICK)
				{
					m.Result = (IntPtr)1;
					return;
				}
			}

			base.WndProc(ref m);
		} */

		protected override void WndProc(ref Message m)
		{
			try
			{
				if((m.Msg == NativeMethods.WM_CONTEXTMENU) && (m_ctxHeader != null) &&
					(this.View == View.Details) && (this.HeaderStyle !=
					ColumnHeaderStyle.None) && !NativeLib.IsUnix())
				{
					IntPtr hList = this.Handle;
					if(hList != IntPtr.Zero)
					{
						IntPtr hHeader = NativeMethods.SendMessage(hList,
							NativeMethods.LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
						if(hHeader != IntPtr.Zero)
						{
							NativeMethods.RECT rc = new NativeMethods.RECT();
							if(NativeMethods.GetWindowRect(hHeader, ref rc))
							{
								long l = m.LParam.ToInt64();
								short x = (short)(l & 0xFFFF);
								short y = (short)((l >> 16) & 0xFFFF);

								if((x >= rc.Left) && (x < rc.Right) &&
									(y >= rc.Top) && (y < rc.Bottom) &&
									((x != -1) || (y != -1)))
								{
									m_ctxHeader.Show(x, y);
									return;
								}
							}
							else { Debug.Assert(false); }
						}
						else { Debug.Assert(false); }
					}
					else { Debug.Assert(false); }
				}
			}
			catch(Exception) { Debug.Assert(false); }

			base.WndProc(ref m);
		}

		internal void BeginUpdateEx()
		{
			BeginUpdate();

			if(++m_cUpdating == 1) // Increment before setting properties
			{
				m_cmpUpdatingPre = this.ListViewItemSorter;
				m_soUpdatingPre = this.Sorting;

				this.Sorting = SortOrder.None;
				this.ListViewItemSorter = null;
			}
		}

		internal void EndUpdateEx()
		{
			EndUpdateEx(null);
		}

		internal void EndUpdateEx(ListViewItem lviTop)
		{
			Debug.Assert(m_cUpdating > 0);

			if(m_cUpdating == 1)
			{
				// The caller should not change the sorting while updating
				Debug.Assert(this.ListViewItemSorter == null);
				Debug.Assert(this.Sorting == SortOrder.None);

				this.ListViewItemSorter = m_cmpUpdatingPre;
				if(m_soUpdatingPre != SortOrder.None)
					this.Sorting = m_soUpdatingPre;

				m_cmpUpdatingPre = null;
				// m_soUpdatingPre = SortOrder.None;

				ApplyAlternatingItemStyles();

				if(lviTop != null)
				{
					Debug.Assert(lviTop.ListView == this);
					int i = lviTop.Index;
					Debug.Assert((i >= 0) && (i < this.Items.Count));
					UIUtil.SetTopVisibleItem(this, i, false);
				}
			}
			else { Debug.Assert(lviTop == null); } // Outermost sorts, changing the view

			--m_cUpdating; // Decrement after setting properties

			EndUpdate();
		}

		private void ApplyAlternatingItemStyles()
		{
			if(!m_bAltItemStyles) return;

			Color clrAlt = UIUtil.GetAlternateColorEx(this.BackColor);

			UIUtil.SetAlternatingBgColors(this, clrAlt,
				Program.Config.MainWindow.EntryListAlternatingBgColors);
		}
	}
}
