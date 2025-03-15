/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2025 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePass.App;
using KeePass.Resources;

using KeePassLib.Utility;

namespace KeePass.UI
{
	public sealed class UIElementSearchBoxEx : TextBox
	{
		private object m_oHighlighted = null;
#if DEBUG
		private Color m_clrNormalBack = Color.Empty;
#endif

		public UIElementSearchBoxEx()
		{
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			if(!Program.DesignMode)
			{
				UIUtil.SetCueBanner(this, KPRes.Search);
				AccessibilityEx.SetName(this, KPRes.Search);
			}

			base.OnHandleCreated(e);
		}

		private void AddUIElements(Control cOwner, List<object> lOut)
		{
			if(cOwner == null) { Debug.Assert(false); return; }
			if(lOut == null) { Debug.Assert(false); return; }

			if(cOwner.Controls.Count == 0) return;

			// The order of tab pages in Controls varies
			TabControl tc = (cOwner as TabControl);
			if(tc != null)
			{
				foreach(TabPage tp in tc.TabPages)
				{
					lOut.Add(tp);
					AddUIElements(tp, lOut);
				}
				return;
			}

			List<KeyValuePair<Rectangle, Control>> l =
				new List<KeyValuePair<Rectangle, Control>>();
			foreach(Control c in cOwner.Controls)
			{
				if((c != null) && (c != this))
					l.Add(new KeyValuePair<Rectangle, Control>(c.Bounds, c));
			}
			l.Sort(AccessibilityEx.CompareByLocation);

			foreach(KeyValuePair<Rectangle, Control> kvp in l)
			{
				Control c = kvp.Value;

#if DEBUG
				if(c != m_oHighlighted)
				{
					TabPage tp = (c as TabPage);
					if(tp != null) { Debug.Assert(tp.UseVisualStyleBackColor); }
					ComboBox cmb = (c as ComboBox);
					if(cmb != null) { Debug.Assert(cmb.FlatStyle == FlatStyle.Standard); }
				}
#endif

				ListView lv = (c as ListView);
				if(lv != null)
				{
					if(lv.ShowGroups && (lv.Groups.Count != 0))
					{
						foreach(ListViewGroup lvg in lv.Groups)
						{
							lOut.Add(lvg);

							foreach(ListViewItem lvi in lvg.Items)
								lOut.Add(lvi);
						}
					}
					else
					{
						foreach(ListViewItem lvi in lv.Items)
							lOut.Add(lvi);
					}

					continue;
				}

				lOut.Add(c);
				AddUIElements(c, lOut);
			}
		}

		private static Color GetHighlightColor(Color clrBackground)
		{
			// Text may be gray
			return (UIUtil.IsDarkColor(clrBackground) ?
				Color.FromArgb(160, 160, 0) : Color.FromArgb(255, 255, 0));
		}

		private static void ResetBackColorEx(Control c)
		{
			if(c == null) { Debug.Assert(false); return; }

			c.ResetBackColor();

			TabPage tp = (c as TabPage);
			if(tp != null) tp.UseVisualStyleBackColor = true;

			ComboBox cmb = (c as ComboBox);
			if(cmb != null) cmb.FlatStyle = FlatStyle.Standard;
		}

		private void EnsureVisibleEx(Control c)
		{
			if(c == null) { Debug.Assert(false); return; }

			Form f = (c.TopLevelControl as Form);
			Control cFocused = ((f != null) ? UIUtil.GetActiveControl(f) : null);
			Debug.Assert((f != Program.MainForm) && (cFocused == this));

			TabPage tp = (c as TabPage);
			Control cParent = c.Parent;

			while(cParent != null)
			{
				TabControl tc = (cParent as TabControl);
				if(tc != null)
				{
					if((tp != null) && tc.TabPages.Contains(tp))
						tc.SelectedTab = tp;
					else { Debug.Assert(false); }
				}

				tp = (cParent as TabPage);
				cParent = cParent.Parent;
			}

			if(cFocused != null) UIUtil.SetFocus(cFocused, f);
		}

		private void Highlight(object o, bool bHighlight)
		{
			ListViewGroup lvg = (o as ListViewGroup);
			if(lvg != null)
			{
				ListView lv = lvg.ListView;
				if(lv == null) { Debug.Assert(false); return; }
				lv.BeginUpdate();

				foreach(ListViewItem lviE in lvg.Items)
				{
					if(bHighlight)
					{
						Debug.Assert(lviE.BackColor == lv.BackColor);
						lviE.BackColor = GetHighlightColor(lviE.BackColor);
						lviE.Selected = false;
					}
					else lviE.BackColor = lv.BackColor;
				}

				if(bHighlight)
				{
					int ciGroup = lvg.Items.Count;
					if(ciGroup != 0)
					{
						lvg.Items[ciGroup - 1].EnsureVisible();
						lvg.Items[0].EnsureVisible();
					}
				}

				lv.EndUpdate();
				if(bHighlight) EnsureVisibleEx(lv);
				return;
			}

			ListViewItem lvi = (o as ListViewItem);
			if(lvi != null)
			{
				ListView lv = lvi.ListView;
				if(lv == null) { Debug.Assert(false); return; }

				if(bHighlight)
				{
					Debug.Assert(lvi.BackColor == lv.BackColor);
					lvi.BackColor = GetHighlightColor(lvi.BackColor);
					lvi.Selected = false;

					lvi.EnsureVisible();
					EnsureVisibleEx(lv);
				}
				else lvi.BackColor = lv.BackColor;
				return;
			}

			Control c = (o as Control);
			if(c != null)
			{
				if(bHighlight)
				{
#if DEBUG
					m_clrNormalBack = c.BackColor;
#endif

					ComboBox cmb = (c as ComboBox);
					if(cmb != null) cmb.FlatStyle = FlatStyle.Popup; // Color support

					c.BackColor = GetHighlightColor(c.BackColor);

					EnsureVisibleEx(c);
				}
				else
				{
					ResetBackColorEx(c);
#if DEBUG
					Debug.Assert(UIUtil.ColorsEqual(c.BackColor, m_clrNormalBack));
#endif
				}
				return;
			}

			Debug.Assert(false); // Unknown UI element type
		}

		private static string GetTextEx(object o)
		{
			if(o == null) { Debug.Assert(false); return null; }

			ListViewGroup lvg = (o as ListViewGroup);
			if(lvg != null) return lvg.Header;

			ListViewItem lvi = (o as ListViewItem);
			if(lvi != null)
			{
				if(lvi.SubItems.Count <= 1) return lvi.Text;

				StringBuilder sb = new StringBuilder();
				foreach(ListViewItem.ListViewSubItem lvsi in lvi.SubItems)
					sb.AppendLine(lvsi.Text ?? string.Empty);
				return sb.ToString();
			}

			ComboBox cmb = (o as ComboBox);
			if(cmb != null)
			{
				StringBuilder sb = new StringBuilder();
				sb.AppendLine(cmb.Text ?? string.Empty);
				foreach(object oI in cmb.Items)
					sb.AppendLine(((oI != null) ? oI.ToString() : null) ?? string.Empty);
				return sb.ToString();
			}

			Control c = (o as Control);
			if(c != null) return NormalizeControlText(c.Text);

			Debug.Assert(false); // Unknown UI element type
			return null;
		}

		private static string g_strDblAmpPlh = null;
		private static string NormalizeControlText(string str)
		{
			if(string.IsNullOrEmpty(str)) return str;

			string strPlh = g_strDblAmpPlh;
			if(strPlh == null)
			{
				strPlh = Guid.NewGuid().ToString();
				Debug.Assert(strPlh.IndexOf('&') < 0);
				g_strDblAmpPlh = strPlh;
			}

			str = str.Replace("&&", strPlh);
			str = str.Replace("&", string.Empty);
			str = str.Replace(strPlh, "&");

			return str;
		}

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);

			PerformSearch(false, false);
		}

		protected override bool IsInputKey(Keys keyData)
		{
			// TextBox.AcceptsReturn works only if Multiline is true
			if(((keyData & Keys.KeyCode) == Keys.Return) &&
				((keyData & Keys.Alt) == Keys.None))
				return true;

			return base.IsInputKey(keyData);
		}

		private bool HandleKey(KeyEventArgs e, bool bDown)
		{
			if((e.KeyCode == Keys.Return) && !e.Alt)
			{
				UIUtil.SetHandled(e, true);
				if(bDown) PerformSearch(true, e.Shift);
				return true;
			}

			return false;
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if(!HandleKey(e, true)) base.OnKeyDown(e);
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			if(!HandleKey(e, false)) base.OnKeyUp(e);
		}

		private void PerformSearch(bool bNext, bool bBackwards)
		{
			if(Program.DesignMode) return;

			try
			{
				object o;
				bool b = PerformSearchEx(bNext, bBackwards, out o);

				if(m_oHighlighted != null) Highlight(m_oHighlighted, false);
				if(o != null) Highlight(o, true);
				m_oHighlighted = o;

				if(b) ResetBackColor();
				else this.BackColor = AppDefs.ColorEditError;
			}
			catch(Exception) { Debug.Assert(false); }
		}

		private bool PerformSearchEx(bool bNext, bool bBackwards, out object oResult)
		{
			oResult = null;

			string strSearch = this.Text;
			if(string.IsNullOrEmpty(strSearch)) return true;

			List<object> l = new List<object>();
			AddUIElements(this.Parent, l);

			int n = l.Count;
			if(n == 0) { Debug.Assert(false); return false; }

			int iStart = -1;
			if(m_oHighlighted != null)
			{
				iStart = l.IndexOf(m_oHighlighted);
				Debug.Assert(iStart >= 0);
			}

			if(iStart < 0) iStart = 0;
			else if(bNext) iStart = (iStart + (bBackwards ? (n - 1) : 1)) % n;

			int j = iStart;
			for(int i = 0; i < n; ++i)
			{
				object o = l[j];
				string str = GetTextEx(o);

				if(!string.IsNullOrEmpty(str) && (str.IndexOf(strSearch,
					StrUtil.CaseIgnoreCmp) >= 0))
				{
					oResult = o;
					return true;
				}

				j = (j + (bBackwards ? (n - 1) : 1)) % n;
			}

			return false;
		}
	}
}
