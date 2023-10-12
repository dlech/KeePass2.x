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
using System.Reflection;
using System.Text;
using System.Windows.Forms;

using KeePass.App.Configuration;
using KeePass.Util;

using KeePassLib.Utility;

namespace KeePass.UI
{
	internal sealed class FormDataExchange
	{
		private readonly bool m_bUpdateOnAdd;
		private readonly bool m_bReadOnlyIfEnforced;

		private readonly List<FdxItem> m_l = new List<FdxItem>();

		private sealed class FdxItem
		{
			public readonly Control Control;
			public readonly object Object;
			public readonly PropertyInfo PropertyInfo;
			public readonly bool ReadOnly;

#if DEBUG
			public readonly string ProgramConfigPath;
#endif

			public FdxItem(Control c, object o, PropertyInfo pi, bool bReadOnly)
			{
				this.Control = c;
				this.Object = o;
				this.PropertyInfo = pi;
				this.ReadOnly = bReadOnly;

#if DEBUG
				this.ProgramConfigPath = XmlUtil.GetObjectXmlPath(Program.Config, o);
#endif
			}
		}

		public FormDataExchange(Form f, bool bUpdateOnAdd, bool bUpdateOnClosed,
			bool bReadOnlyIfEnforced)
		{
			if(f == null) throw new ArgumentNullException("f");

			m_bUpdateOnAdd = bUpdateOnAdd;
			m_bReadOnlyIfEnforced = bReadOnlyIfEnforced;

			if(bUpdateOnClosed) f.FormClosed += this.OnFormClosed;
		}

		public void Add(Control c, object o, string strPropertyName)
		{
			if(c == null) throw new ArgumentNullException("c");
			if(o == null) throw new ArgumentNullException("o");
			if(strPropertyName == null) throw new ArgumentNullException("strPropertyName");

			PropertyInfo pi = o.GetType().GetProperty(strPropertyName);
			if((pi == null) || !pi.CanRead || !pi.CanWrite)
				throw new MethodAccessException();

			bool bReadOnly = (m_bReadOnlyIfEnforced ? AppConfigEx.IsOptionEnforced(
				o, pi) : false);

			FdxItem it = new FdxItem(c, o, pi, bReadOnly);
			m_l.Add(it);

			if(m_bUpdateOnAdd) UpdateData(it, false);
		}

		public void UpdateData(bool bControlsToData)
		{
			foreach(FdxItem it in m_l)
				UpdateData(it, bControlsToData);
		}

		private void UpdateData(FdxItem it, bool bControlToData)
		{
			Control c = it.Control;
			object o = it.Object;
			PropertyInfo pi = it.PropertyInfo;
			Type tValue = pi.PropertyType;

#if DEBUG
			// Check that the *current* Program.Config owns the object,
			// if this was the case initially
			Debug.Assert(XmlUtil.GetObjectXmlPath(Program.Config, o) == it.ProgramConfigPath);
			// If enforceable, it should be in Program.Config
			Debug.Assert(!m_bReadOnlyIfEnforced || !string.IsNullOrEmpty(it.ProgramConfigPath));
#endif

			if(it.ReadOnly)
			{
				if(bControlToData) return;
				c.Enabled = false;
			}

			CheckBox cb = (c as CheckBox);
			if((cb != null) && (tValue == typeof(bool)))
			{
				Debug.Assert(cb.CheckState != CheckState.Indeterminate);
				if(bControlToData) pi.SetValue(o, cb.Checked, null);
				else UIUtil.SetChecked(cb, (bool)pi.GetValue(o, null));
				return;
			}

			ComboBox cmb = (c as ComboBox);
			if((cmb != null) && (tValue == typeof(int)))
			{
				Debug.Assert(cmb.DropDownStyle == ComboBoxStyle.DropDownList);
				if(bControlToData) pi.SetValue(o, cmb.SelectedIndex, null);
				else
				{
					int i = (int)pi.GetValue(o, null);
					if((i >= 0) && (i < cmb.Items.Count))
						cmb.SelectedIndex = i;
					else
					{
						Debug.Assert(false);
						if(cmb.Items.Count != 0) cmb.SelectedIndex = 0;
					}
				}
				return;
			}

			ColorButtonEx clrbtn = (c as ColorButtonEx);
			if((clrbtn != null) && (tValue == typeof(string)))
			{
				if(bControlToData)
					pi.SetValue(o, StrUtil.ColorToUnnamedHtml(
						clrbtn.SelectedColor, true), null);
				else
				{
					string str = (pi.GetValue(o, null) as string);
					Color clr = Color.Empty;
					try
					{
						if(!string.IsNullOrEmpty(str))
							clr = ColorTranslator.FromHtml(str);
					}
					catch(Exception) { Debug.Assert(false); }
					clrbtn.SelectedColor = clr;
				}
				return;
			}

			Debug.Assert(false); // Unknown control/type
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			UpdateData(true);
		}
	}
}
