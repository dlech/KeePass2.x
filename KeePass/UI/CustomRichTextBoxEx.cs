/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2009 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Diagnostics;

namespace KeePass.UI
{
	public sealed class CustomRichTextBoxEx : RichTextBox
	{
		private bool m_bSimpleTextOnly = false;
		public bool SimpleTextOnly
		{
			get { return m_bSimpleTextOnly; }
			set { m_bSimpleTextOnly = value; }
		}

		public CustomRichTextBoxEx() : base()
		{
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
	}
}
