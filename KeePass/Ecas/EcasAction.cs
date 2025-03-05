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
using System.Text;
using System.Xml.Serialization;

using KeePassLib;
using KeePassLib.Interfaces;

namespace KeePass.Ecas
{
	public sealed class EcasAction : IDeepCloneable<EcasAction>, IEcasObject
	{
		private PwUuid m_puTypeA = PwUuid.Zero;
		[XmlIgnore]
		public PwUuid Type
		{
			get { return m_puTypeA; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_puTypeA = value;
			}
		}

		[XmlElement("TypeGuid")]
		public string TypeString
		{
			get { return Convert.ToBase64String(m_puTypeA.UuidBytes); }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_puTypeA = new PwUuid(Convert.FromBase64String(value));
			}
		}

		private List<string> m_lParamsA = new List<string>();
		[XmlArrayItem("Parameter")]
		public List<string> Parameters
		{
			get { return m_lParamsA; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_lParamsA = value;
			}
		}

		public EcasAction()
		{
		}

		public EcasAction CloneDeep()
		{
			EcasAction a = new EcasAction();

			a.m_puTypeA = m_puTypeA; // PwUuid is immutable
			a.m_lParamsA.AddRange(m_lParamsA);

			return a;
		}
	}
}
