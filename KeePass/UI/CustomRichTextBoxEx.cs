/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2013 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;
using System.Diagnostics;

namespace KeePass.UI
{
	public sealed class CustomRichTextBoxEx : RichTextBox
	{
		// private int m_nClearFormatting = 0;

		private bool m_bSimpleTextOnly = false;
		[Browsable(false)]
		[DefaultValue(false)]
		public bool SimpleTextOnly
		{
			get { return m_bSimpleTextOnly; }
			set { m_bSimpleTextOnly = value; }
		}

		private bool m_bCtrlEnterAccepts = false;
		[Browsable(false)]
		[DefaultValue(false)]
		public bool CtrlEnterAccepts
		{
			get { return m_bCtrlEnterAccepts; }
			set { m_bCtrlEnterAccepts = value; }
		}

		public CustomRichTextBoxEx() : base()
		{
			// this.EnableAutoDragDrop = true;
		}

/* #if DEBUG
		~CustomRichTextBoxEx()
		{
			Debug.Assert(m_nClearFormatting == 0);
		}
#endif */

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if(m_bSimpleTextOnly && ((e.Control && (e.KeyCode == Keys.V)) ||
				(e.Shift && (e.KeyCode == Keys.Insert))))
			{
				e.Handled = true;

				PasteAcceptable();
				return;
			}

			if(m_bCtrlEnterAccepts && e.Control && ((e.KeyCode == Keys.Return) ||
				(e.KeyCode == Keys.Enter)))
			{
				e.Handled = true;
				Debug.Assert(this.Multiline);

				Control p = this;
				Form f;
				while(true)
				{
					f = (p as Form);
					if(f != null) break;

					Control pParent = p.Parent;
					if((pParent == null) || (pParent == p)) break;
					p = pParent;
				}
				if(f != null)
				{
					IButtonControl btn = f.AcceptButton;
					if(btn != null) btn.PerformClick();
					else { Debug.Assert(false); }
				}
				else { Debug.Assert(false); }

				return;
			}

			base.OnKeyDown(e);
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			if(m_bSimpleTextOnly && ((e.Control && (e.KeyCode == Keys.V)) ||
				(e.Shift && (e.KeyCode == Keys.Insert))))
			{
				e.Handled = true;
				return;
			}

			if(m_bCtrlEnterAccepts && e.Control && ((e.KeyCode == Keys.Return) ||
				(e.KeyCode == Keys.Enter)))
			{
				e.Handled = true;
				return;
			}

			base.OnKeyUp(e);
		}

		public void PasteAcceptable()
		{
			try
			{
				if(!m_bSimpleTextOnly) Paste();
				else if(Clipboard.ContainsData(DataFormats.UnicodeText))
					Paste(DataFormats.GetFormat(DataFormats.UnicodeText));
				else if(Clipboard.ContainsData(DataFormats.Text))
					Paste(DataFormats.GetFormat(DataFormats.Text));
			}
			catch(Exception) { Debug.Assert(false); }
		}

		/* private void ClearFormattingEx()
		{
			try
			{
				int iSelStart = this.SelectionStart;
				int iSelLen = this.SelectionLength;
				string strText = this.Text;

				Clear();
				this.Text = strText;

				Select(iSelStart, iSelLen);
				ScrollToCaret();
			}
			catch(Exception) { Debug.Assert(false); }
		}

		private bool DropHasText(DragEventArgs e)
		{
			if(e == null) { Debug.Assert(false); return false; }

			IDataObject ido = e.Data;
			if(ido == null)
			{
				Debug.Assert(false);
				e.Effect = DragDropEffects.None;
				return false;
			}

			return (ido.GetDataPresent(DataFormats.Text) ||
				ido.GetDataPresent(DataFormats.UnicodeText));
		}

		private void CustomizeDropEffect(DragEventArgs e)
		{
			if(e == null) { Debug.Assert(false); return; }
			if(!DropHasText(e)) e.Effect = DragDropEffects.None;
		}

		protected override void OnDragEnter(DragEventArgs drgevent)
		{
			base.OnDragEnter(drgevent);
			CustomizeDropEffect(drgevent);
		}

		protected override void OnDragOver(DragEventArgs drgevent)
		{
			base.OnDragOver(drgevent);
			CustomizeDropEffect(drgevent);
		}

		protected override void OnDragDrop(DragEventArgs drgevent)
		{
			if(drgevent == null) { Debug.Assert(false); return; }
			if(!DropHasText(drgevent))
			{
				drgevent.Effect = DragDropEffects.None;
				return;
			}

			base.OnDragDrop(drgevent);

			if(m_bSimpleTextOnly && (drgevent.Effect != DragDropEffects.None))
			{
				Debug.Assert(m_nClearFormatting == 0);
				++m_nClearFormatting;
			}
		}

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);

			if(m_nClearFormatting > 0)
			{
				Debug.Assert(m_nClearFormatting == 1);
				--m_nClearFormatting; // *Before* changing the text again!
				ClearFormattingEx();
			}
		} */

		// protected override void OnMouseDown(MouseEventArgs e)
		// {
		//	string strText = this.SelectedText;
		//	if(!string.IsNullOrEmpty(strText))
		//		this.DoDragDrop(this.SelectedText, DragDropEffects.Copy |
		//			DragDropEffects.Scroll);
		//	else base.OnMouseDown(e);
		// }

		/* private Rectangle m_rectDragBox = Rectangle.Empty;

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			if(((e.Button & MouseButtons.Left) == MouseButtons.Left) &&
				(this.SelectionLength > 0))
			{
				Size szDrag = SystemInformation.DragSize;
				m_rectDragBox = new Rectangle(new Point(e.X - (szDrag.Width / 2),
					e.Y - (szDrag.Height / 2)), szDrag);
			}
		}

		protected override void OnMouseUp(MouseEventArgs mevent)
		{
			m_rectDragBox = Rectangle.Empty;
			base.OnMouseUp(mevent);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if(((e.Button & MouseButtons.Left) == MouseButtons.Left) &&
				!m_rectDragBox.IsEmpty)
			{
				if(!m_rectDragBox.Contains(e.X, e.Y))
				{
					m_rectDragBox = Rectangle.Empty;

					string strText = this.SelectedText;
					int iSelStart = this.SelectionStart;
					int iSelLen = this.SelectionLength;

					if(!string.IsNullOrEmpty(strText))
					{
						DoDragDrop(strText, DragDropEffects.Copy |
							DragDropEffects.Scroll);
						Select(iSelStart, iSelLen);
					}
					else { Debug.Assert(false); }
				}
			}
			else m_rectDragBox = Rectangle.Empty;
		} */
	}
}
