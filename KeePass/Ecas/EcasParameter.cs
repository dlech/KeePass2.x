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

using KeePassLib.Utility;

namespace KeePass.Ecas
{
	public sealed class EcasParameter
	{
		public static EcasParameter[] EmptyArray
		{
			get { return MemUtil.EmptyArray<EcasParameter>(); }
		}

		private string m_strName;
		public string Name
		{
			get { return m_strName; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strName = value;
			}
		}

		private EcasValueType m_type;
		public EcasValueType Type
		{
			get { return m_type; }
			set { m_type = value; }
		}

		private EcasEnum m_vEnumValues;
		public EcasEnum EnumValues
		{
			get { return m_vEnumValues; }
			set { m_vEnumValues = value; } // May be null
		}

		public EcasParameter(string strName, EcasValueType t, EcasEnum eEnumValues)
		{
			if(strName == null) throw new ArgumentNullException("strName");

			m_strName = strName;
			m_type = t;
			m_vEnumValues = eEnumValues;
		}
	}
}
