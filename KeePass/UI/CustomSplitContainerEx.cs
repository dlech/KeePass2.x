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
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace KeePass.UI
{
	public sealed class CustomSplitContainerEx : SplitContainer
	{
		private ControlCollection m_ccControls = null;
		private Control m_cDefault = null;

		private Control m_cFocused = null;
		private Control m_cLastKnown = null;

		public void InitEx(ControlCollection cc, Control cDefault)
		{
			m_ccControls = cc;
			m_cDefault = m_cLastKnown = cDefault;
		}

		private static Control FindInputFocus(ControlCollection cc)
		{
			if(cc == null) { Debug.Assert(false); return null; }

			foreach(Control c in cc)
			{
				if(c.Focused)
					return c;
				else if(c.ContainsFocus)
					return FindInputFocus(c.Controls);
			}

			return null;
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			m_cFocused = FindInputFocus(m_ccControls);
			if(m_cFocused == null) m_cFocused = m_cDefault;

			if(m_cFocused != null) m_cLastKnown = m_cFocused;

			base.OnMouseDown(e);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			if(m_cFocused != null)
			{
				m_cFocused.Focus();
				m_cFocused = null;
			}
			else { Debug.Assert(false); }
		}

		protected override void OnEnter(EventArgs e)
		{
			base.OnEnter(e);

			if(this.Focused && (m_cFocused == null))
			{
				if(m_cLastKnown != null) m_cLastKnown.Focus();
				else if(m_cDefault != null) m_cDefault.Focus();
			}
		}
	}
}
