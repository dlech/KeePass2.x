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
using System.Xml.Serialization;
using System.Diagnostics;
using System.Reflection;

namespace KeePassLib.Translation
{
	public sealed class KPFormCustomization
	{
		private string m_strFQName = string.Empty;
		/// <summary>
		/// The fully qualified name of the form.
		/// </summary>
		[XmlAttribute]
		public string FullName
		{
			get { return m_strFQName; }
			set { m_strFQName = value; }
		}

		private List<KPControlCustomization> m_vControls =
			new List<KPControlCustomization>();
		[XmlIgnore]
		public List<KPControlCustomization> ControlsList
		{
			get { return m_vControls; }
			set { m_vControls = value; }
		}

		[XmlArray("ChildControls")]
		[XmlArrayItem("Control")]
		public KPControlCustomization[] Controls
		{
			get { return m_vControls.ToArray(); }
			set
			{
				if(value == null) throw new ArgumentNullException("value");

				m_vControls.Clear();
				foreach(KPControlCustomization kpcc in value)
					m_vControls.Add(kpcc);
			}
		}

		private string m_strTextEng = string.Empty;
		[XmlIgnore]
		public string TextEnglish
		{
			get { return m_strTextEng; }
			set { m_strTextEng = value; }
		}

#if !KeePassLibSD
		public void ApplyTo(Form form)
		{
			Debug.Assert(form != null); if(form == null) throw new ArgumentNullException("form");

			Debug.Assert(form.GetType().FullName == m_strFQName);

			if(m_vControls.Count == 0) return;

			foreach(Control c in form.Controls) ApplyToControl(c);
		}

		private void ApplyToControl(Control c)
		{
			foreach(KPControlCustomization cc in m_vControls)
			{
				if(c.Name == cc.Name)
				{
					cc.ApplyTo(c);
					break;
				}
			}

			foreach(Control cSub in c.Controls) ApplyToControl(cSub);
		}
#endif
	}
}
