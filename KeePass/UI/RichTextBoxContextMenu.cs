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
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;

using KeePass.Resources;

namespace KeePass.UI
{
	internal enum RichTextBoxContextMenuCommands
	{
		Undo = 0,
		Cut,
		Copy,
		Paste,
		Delete,
		CopyAll,
		SelectAll,
		Count
	}

	public sealed class RichTextBoxContextMenu
	{
		private RichTextBox m_rtb = null;
		private ContextMenuStrip m_ctx = null;
		private ToolStripItem[] m_vMenuItems =
			new ToolStripItem[(int)RichTextBoxContextMenuCommands.Count];

		public RichTextBoxContextMenu()
		{
		}

		~RichTextBoxContextMenu()
		{
			this.Detach();
		}

		public void Attach(RichTextBox rtb)
		{
			this.Detach();

			m_rtb = rtb;

			m_ctx = CreateContextMenu();
			m_ctx.Opening += this.OnMenuOpening;

			m_rtb.ContextMenuStrip = m_ctx;
		}

		public void Detach()
		{
			if(m_rtb != null)
			{
				m_rtb.ContextMenuStrip = null;

				m_ctx.Opening -= this.OnMenuOpening;
				m_ctx = null;

				for(int i = 0; i < (int)RichTextBoxContextMenuCommands.Count; ++i)
					m_vMenuItems[i] = null;

				m_rtb = null;
			}
		}

		private ContextMenuStrip CreateContextMenu()
		{
			ContextMenuStrip ctx = new ContextMenuStrip();
			int iPos = -1;

			m_vMenuItems[++iPos] = ctx.Items.Add(KPRes.Undo,
				KeePass.Properties.Resources.B16x16_Undo, this.OnUndoCommand);
			ctx.Items.Add(new ToolStripSeparator());

			m_vMenuItems[++iPos] = ctx.Items.Add(KPRes.Cut,
				KeePass.Properties.Resources.B16x16_Cut, this.OnCutCommand);
			m_vMenuItems[++iPos] = ctx.Items.Add(KPRes.Copy,
				KeePass.Properties.Resources.B16x16_EditCopy, this.OnCopyCommand);
			m_vMenuItems[++iPos] = ctx.Items.Add(KPRes.Paste,
				KeePass.Properties.Resources.B16x16_EditPaste, this.OnPasteCommand);
			m_vMenuItems[++iPos] = ctx.Items.Add(KPRes.Delete,
				KeePass.Properties.Resources.B16x16_EditDelete, this.OnDeleteCommand);
			ctx.Items.Add(new ToolStripSeparator());

			m_vMenuItems[++iPos] = ctx.Items.Add(KPRes.CopyAll,
				KeePass.Properties.Resources.B16x16_EditShred, this.OnCopyAllCommand);
			m_vMenuItems[++iPos] = ctx.Items.Add(KPRes.SelectAll,
				KeePass.Properties.Resources.B16x16_Edit, this.OnSelectAllCommand);

			Debug.Assert(iPos == ((int)RichTextBoxContextMenuCommands.Count - 1));
			return ctx;
		}

		private void OnMenuOpening(object sender, EventArgs e)
		{
			m_vMenuItems[(int)RichTextBoxContextMenuCommands.Undo].Enabled =
				m_vMenuItems[(int)RichTextBoxContextMenuCommands.Cut].Enabled =
				m_vMenuItems[(int)RichTextBoxContextMenuCommands.Paste].Enabled =
				m_vMenuItems[(int)RichTextBoxContextMenuCommands.Delete].Enabled =
				!m_rtb.ReadOnly;
		}

		private void OnUndoCommand(object sender, EventArgs e)
		{
			m_rtb.Undo();
		}

		private void OnCutCommand(object sender, EventArgs e)
		{
			m_rtb.Cut();
		}

		private void OnCopyCommand(object sender, EventArgs e)
		{
			m_rtb.Copy();
		}

		private void OnPasteCommand(object sender, EventArgs e)
		{
			m_rtb.Paste();
		}

		private void OnDeleteCommand(object sender, EventArgs e)
		{
			int nStart = m_rtb.SelectionStart, nLength = m_rtb.SelectionLength;
			if((nStart < 0) || (nLength <= 0)) return;
			
			string strText = m_rtb.Text;
			strText = strText.Remove(nStart, nLength);
			m_rtb.Text = strText;

			m_rtb.Select(nStart, 0);
		}

		private void OnCopyAllCommand(object sender, EventArgs e)
		{
			int nStart = m_rtb.SelectionStart, nLength = m_rtb.SelectionLength;
			m_rtb.SelectAll();
			m_rtb.Copy();
			m_rtb.Select(nStart, nLength);
		}

		private void OnSelectAllCommand(object sender, EventArgs e)
		{
			m_rtb.SelectAll();
		}
	}
}
