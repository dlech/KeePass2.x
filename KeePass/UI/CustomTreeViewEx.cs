﻿/*
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
using System.Text;
using System.Windows.Forms;

using KeePass.Native;
using KeePass.Util;

using NativeLib = KeePassLib.Native.NativeLib;

namespace KeePass.UI
{
	// public delegate string QueryToolTipDelegate(TreeNode tn);

	public sealed class CustomTreeViewEx : TreeView
	{
		// private TreeNode m_tnReadyForLabelEdit = null;

		// private QueryToolTipDelegate m_fnQueryToolTip = null;
		// /// <summary>
		// /// This handler will be used to dynamically query tooltip
		// /// texts for tree nodes.
		// /// </summary>
		// public QueryToolTipDelegate QueryToolTip
		// {
		//	get { return m_fnQueryToolTip; }
		//	set { m_fnQueryToolTip = value; }
		// }

		public CustomTreeViewEx() : base()
		{
			if(Program.DesignMode) return;

			// Enable default double buffering (must be combined with
			// TVS_EX_DOUBLEBUFFER, see OnHandleCreated)
			try { this.DoubleBuffered = true; }
			catch(Exception) { Debug.Assert(!WinUtil.IsAtLeastWindowsVista); }

			// try
			// {
			//	IntPtr hWnd = this.Handle;
			//	if((hWnd != IntPtr.Zero) && (this.ItemHeight == 16))
			//	{
			//		int nStyle = NativeMethods.GetWindowStyle(hWnd);
			//		nStyle |= (int)NativeMethods.TVS_NONEVENHEIGHT;
			//		NativeMethods.SetWindowLong(hWnd, NativeMethods.GWL_STYLE, nStyle);
			//		this.ItemHeight = 17;
			//	}
			// }
			// catch(Exception) { }

			// this.ItemHeight = 18;

			// try
			// {
			//	IntPtr hWnd = this.Handle;
			//	if((hWnd != IntPtr.Zero) && UIUtil.VistaStyleListsSupported)
			//	{
			//		int nStyle = NativeMethods.GetWindowStyle(hWnd);
			//		nStyle |= (int)NativeMethods.TVS_FULLROWSELECT;
			//		NativeMethods.SetWindowLong(hWnd, NativeMethods.GWL_STYLE, nStyle);
			//		nStyle = NativeMethods.GetWindowLong(hWnd, NativeMethods.GWL_EXSTYLE);
			//		nStyle |= (int)NativeMethods.TVS_EX_FADEINOUTEXPANDOS;
			//		NativeMethods.SetWindowLong(hWnd, NativeMethods.GWL_EXSTYLE, nStyle);
			//	}
			// }
			// catch(Exception) { }

			ApplyOptions();
		}

		internal void ApplyOptions()
		{
			this.ShowLines = Program.Config.UI.TreeViewShowLines;
		}

		// protected override void WndProc(ref Message m)
		// {
		//	if(m.Msg == NativeMethods.WM_NOTIFY)
		//	{
		//		NativeMethods.NMHDR nm = (NativeMethods.NMHDR)m.GetLParam(
		//			typeof(NativeMethods.NMHDR));
		//		if((nm.code == NativeMethods.TTN_NEEDTEXTA) ||
		//			(nm.code == NativeMethods.TTN_NEEDTEXTW))
		//			DynamicAssignNodeToolTip();
		//	}
		//	base.WndProc(ref m);
		// }

		// private void DynamicAssignNodeToolTip()
		// {
		//	if(m_fnQueryToolTip == null) return;
		//	TreeViewHitTestInfo hti = HitTest(PointToClient(Cursor.Position));
		//	if(hti == null) { Debug.Assert(false); return; }
		//	TreeNode tn = hti.Node;
		//	if(tn == null) return;
		//	tn.ToolTipText = (m_fnQueryToolTip(tn) ?? string.Empty);
		// }

		/* protected override void OnAfterSelect(TreeViewEventArgs e)
		{
			base.OnAfterSelect(e);

			if(!this.Focused) m_tnReadyForLabelEdit = null;
			else m_tnReadyForLabelEdit = this.SelectedNode;
		}

		protected override void OnLeave(EventArgs e)
		{
			m_tnReadyForLabelEdit = null;

			base.OnLeave(e);
		}

		protected override void OnBeforeLabelEdit(NodeLabelEditEventArgs e)
		{
			if(e != null)
			{
				if((m_tnReadyForLabelEdit == null) || (e.Node !=
					m_tnReadyForLabelEdit))
				{
					e.CancelEdit = true;
					return;
				}
			}
			else { Debug.Assert(false); }

			base.OnBeforeLabelEdit(e); // Call BeforeLabelEdit event
		} */

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			if(Program.DesignMode) return;

			try
			{
				if(this.DoubleBuffered && !NativeLib.IsUnix())
				{
					IntPtr p = new IntPtr((int)NativeMethods.TVS_EX_DOUBLEBUFFER);
					NativeMethods.SendMessage(this.Handle,
						NativeMethods.TVM_SETEXTENDEDSTYLE, p, p);
				}
				else { Debug.Assert(!WinUtil.IsAtLeastWindowsVista); }
			}
			catch(Exception) { Debug.Assert(false); }

			// Display tooltips for a longer time;
			// https://sourceforge.net/p/keepass/feature-requests/2038/
			UIUtil.ConfigureToolTip(this);
		}

		/* [Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= NativeMethods.WS_EX_COMPOSITED;
				return cp;
			}
		} */

		/* protected override void WndProc(ref Message m)
		{
			if(m.Msg == NativeMethods.WM_ERASEBKGND)
			{
				m.Result = IntPtr.Zero;
				return;
			}

			base.WndProc(ref m);
		} */

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if(UIUtil.HandleCommonKeyEvent(e, true, this)) return;

			base.OnKeyDown(e);
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			if(UIUtil.HandleCommonKeyEvent(e, false, this)) return;

			base.OnKeyUp(e);
		}

		protected override void OnBeforeCollapse(TreeViewCancelEventArgs e)
		{
			TreeNode tn = ((e != null) ? e.Node : null);
			if(tn != null)
			{
				if((tn.Parent == null) && !this.ShowRootLines)
				{
					// This should only occur due to a user action (e.g.
					// double-click on the node), not programmatically
					Debug.Assert(false);

					e.Cancel = true;
					return;
				}
			}
			else { Debug.Assert(false); }

			base.OnBeforeCollapse(e);
		}
	}
}
