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

		/* protected override void OnDragDrop(DragEventArgs drgevent)
		{
			if(m_bSimpleTextOnly)
			{
				try
				{
					string strText = this.Text;

					base.OnDragDrop(drgevent);

					if(this.Text != strText)
						this.Text = strText; // Clears all formats
				}
				catch(Exception) { Debug.Assert(false); }

				return; // Do not accept other drops in simple text mode
			}

			base.OnDragDrop(drgevent);
		} */
	}
}
