/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2021 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using KeePass.App;
using KeePass.Resources;

namespace KeePass.UI
{
	public sealed class HotKeyControlEx : TextBox
	{
		private readonly Color m_clrNormalBack;

		private readonly Keys[] m_vModKeys = new Keys[] {
			Keys.ControlKey, Keys.RControlKey, Keys.Menu, Keys.RMenu,
			Keys.ShiftKey, Keys.RShiftKey
		};

		private Keys m_k = Keys.None;
		[Browsable(false)]
		[DefaultValue(Keys.None)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Keys HotKey
		{
			get { return m_k; }
			set { m_k = FixHotKey(value); UpdateUI(m_k, m_k); }
		}
		public bool ShouldSerializeHotKey() { return false; }

		private string m_strTextNone = (Program.DesignMode ? string.Empty : KPRes.None);
		[Browsable(false)]
		[DefaultValue("")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string TextNone
		{
			get { return m_strTextNone; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strTextNone = value;
				UpdateUI(m_k, m_k);
			}
		}
		public bool ShouldSerializeTextNone() { return false; }

		public HotKeyControlEx()
		{
			m_clrNormalBack = this.BackColor;
			if(Program.DesignMode) return;

			this.ContextMenuStrip = new ContextMenuStrip(); // No context menu

			UpdateUI(m_k, m_k); // Initialize text
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			// base.OnKeyDown(e);
			UIUtil.SetHandled(e, true);

			m_k = FixHotKey(e.KeyData);
			UpdateUI(e.KeyData, m_k);
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			// base.OnKeyUp(e);
			UIUtil.SetHandled(e, true);

			if(Control.ModifierKeys == Keys.None)
				UpdateUI(m_k, m_k); // Clear 'Invalid' when releasing all modifiers
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			// base.OnKeyPress(e);
			e.Handled = true;
		}

		protected override void OnLostFocus(EventArgs e)
		{
			UpdateUI(m_k, m_k); // For Shift+Tab, ...
			base.OnLostFocus(e);
		}

		private Keys FixHotKey(Keys kUser)
		{
			Keys k = (kUser & Keys.KeyCode);
			Keys kMod = (kUser & (Keys.Control | Keys.Alt | Keys.Shift)); // Others unsupported

			if(Array.IndexOf<Keys>(m_vModKeys, k) >= 0) return Keys.None;

			Keys[] vInv = new Keys[] {
				Keys.None, Keys.Escape, Keys.Tab, Keys.CapsLock,
				Keys.LWin, Keys.RWin, Keys.Return,
				Keys.F12, // Reserved for debugger, see RegisterHotKey API function
				Keys.Scroll, Keys.NumLock
			};
			if(Array.IndexOf<Keys>(vInv, k) >= 0) return Keys.None;

			if((kMod == Keys.None) || (kMod == Keys.Shift))
			{
				if((k < Keys.F1) || (k > Keys.F24)) return Keys.None;
			}
			else if(kMod == Keys.Control)
			{
				if((k == Keys.Back) || (k == Keys.Insert) || (k == Keys.Delete))
					return Keys.None;
			}
			else if(kMod == (Keys.Control | Keys.Alt))
			{
				if((k >= Keys.D0) && (k <= Keys.D9)) return Keys.None;
			}

			return (kMod | k);
		}

		private void UpdateUI(Keys kUser, Keys kFixed)
		{
			if(Program.DesignMode) return;

			try
			{
				Keys kUC = (kUser & Keys.KeyCode);
				string strText = string.Empty;
				bool bInvalid = false;

				if(kUser == Keys.None)
					strText = m_strTextNone;
				else if(kUser == kFixed)
					strText = UIUtil.GetKeysName(kFixed);
				else
				{
					if((kUC == Keys.None) || (Array.IndexOf<Keys>(m_vModKeys, kUC) >= 0))
						strText = UIUtil.GetKeysName(kUser & Keys.Modifiers) + "+";
					else if(kFixed == Keys.None)
					{
						strText = KPRes.Invalid;
						bInvalid = true;
					}
					else strText = UIUtil.GetKeysName(kFixed);
				}

				if(UIUtil.ColorsEqual(m_clrNormalBack, Color.White))
					this.BackColor = (bInvalid ? AppDefs.ColorEditError : m_clrNormalBack);

				if(strText != this.Text) // Avoid flicker
				{
					this.Text = strText;
					Select(strText.Length, 0);
				}
			}
			catch(Exception) { Debug.Assert(false); }
		}
	}
}
