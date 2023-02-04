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

using KeePass.App.Configuration;
using KeePass.Resources;

using KeePassLib.Utility;

namespace KeePass.UI
{
	internal sealed class FontControlGroup : IDisposable
	{
		private CheckBox m_cb;
		private Button m_btn;
		// private ToolTip m_tt;

		private AceFont m_af;
		public AceFont SelectedFont
		{
			get { return m_af; }
		}

		public bool Enabled
		{
			get
			{
				if(m_cb != null) return m_cb.Enabled;
				Debug.Assert(false);
				return true;
			}

			set
			{
				if(m_cb != null) { m_cb.Enabled = value; UpdateUI(); }
				else { Debug.Assert(false); }
			}
		}

		public FontControlGroup(CheckBox cb, Button btn, AceFont afCurrent,
			AceFont afDefault)
		{
			if(cb == null) throw new ArgumentNullException("cb");
			if(btn == null) throw new ArgumentNullException("btn");

			m_cb = cb;
			m_btn = btn;

			if((afCurrent != null) && afCurrent.OverrideUIDefault)
				m_af = afCurrent.CloneDeep();
			else
			{
				AceFont af;
				if(afDefault != null) af = afDefault;
				else if(afCurrent != null) af = afCurrent;
				else
				{
					FontUtil.SetDefaultFont(cb);
					Font f = FontUtil.DefaultFont;
					af = ((f != null) ? new AceFont(f, false) : new AceFont());
				}

				m_af = af.CloneDeep();
				m_af.OverrideUIDefault = false;
			}

			// When btn.AutoEllipsis is true, a tooltip is shown automatically
			// (and if we create an own tooltip, both are shown)
			// m_tt = new ToolTip();
			// UIUtil.ConfigureToolTip(m_tt);

			UIUtil.SetChecked(cb, m_af.OverrideUIDefault);

			Debug.Assert(!btn.AutoEllipsis && !btn.AutoSize);
			btn.AutoEllipsis = true;

			UpdateUI();

			m_cb.CheckedChanged += this.OnCheckedChanged;
			m_btn.Click += this.OnSelectFont;
		}

		public void Dispose()
		{
			if(m_cb == null) { Debug.Assert(false); return; }

			m_cb.CheckedChanged -= this.OnCheckedChanged;
			m_btn.Click -= this.OnSelectFont;

			Debug.Assert(m_cb.Checked == m_af.OverrideUIDefault);

			// m_tt.Dispose();

			m_cb = null;
			m_btn = null;
			// m_tt = null;
		}

		private void UpdateUI()
		{
			if(m_cb == null) { Debug.Assert(false); return; }

			bool b = m_af.OverrideUIDefault;
			string strFont = (b ? m_af.ToString() : ("(" + KPRes.Default + ")"));

			if(m_cb.Checked != b)
			{
				Debug.Assert(false);
				UIUtil.SetChecked(m_cb, b);
			}

			m_btn.Text = StrUtil.EncodeMenuText(strFont);
			// UIUtil.SetToolTip(m_tt, m_btn, StrUtil.EncodeToolTipText(
			//	KPRes.SelectFont + " \u2013 " + KPRes.Selected + ": " +
			//	strFont), true);
			AccessibilityEx.SetName(m_btn, KPRes.SelectFont, KPRes.Selected + ": " + strFont);

			m_btn.Enabled = (b && m_cb.Enabled);
		}

		private void OnCheckedChanged(object sender, EventArgs e)
		{
			m_af.OverrideUIDefault = m_cb.Checked;
			UpdateUI();
		}

		private void OnSelectFont(object sender, EventArgs e)
		{
			try
			{
				using(FontDialog dlg = UIUtil.CreateFontDialog(false))
				{
					dlg.Font = m_af.ToFont();

					if(dlg.ShowDialog() == DialogResult.OK)
					{
						m_af = new AceFont(dlg.Font, true);
						UpdateUI();
					}
				}
			}
			catch(Exception ex) { MessageService.ShowWarning(ex); }
		}
	}
}
