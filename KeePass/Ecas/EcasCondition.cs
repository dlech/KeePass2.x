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
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;

using KeePassLib;
using KeePassLib.Interfaces;

namespace KeePass.Ecas
{
	public sealed class EcasCondition : IDeepCloneable<EcasCondition>, IEcasObject
	{
		private PwUuid m_puTypeC = PwUuid.Zero;
		[XmlIgnore]
		public PwUuid Type
		{
			get { return m_puTypeC; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_puTypeC = value;
			}
		}

		[XmlElement("TypeGuid")]
		public string TypeString
		{
			get { return Convert.ToBase64String(m_puTypeC.UuidBytes); }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_puTypeC = new PwUuid(Convert.FromBase64String(value));
			}
		}

		private List<string> m_lParamsC = new List<string>();
		[XmlArrayItem("Parameter")]
		public List<string> Parameters
		{
			get { return m_lParamsC; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_lParamsC = value;
			}
		}

		private bool m_bNegate = false;
		[DefaultValue(false)]
		public bool Negate
		{
			get { return m_bNegate; }
			set { m_bNegate = value; }
		}

		public EcasCondition()
		{
		}

		public EcasCondition CloneDeep()
		{
			EcasCondition c = new EcasCondition();

			c.m_puTypeC = m_puTypeC; // PwUuid is immutable
			c.m_lParamsC.AddRange(m_lParamsC);
			c.m_bNegate = m_bNegate;

			return c;
		}
	}
}
