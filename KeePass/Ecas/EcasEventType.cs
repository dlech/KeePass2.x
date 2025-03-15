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

using KeePassLib;

namespace KeePass.Ecas
{
	public delegate bool EcasEventCompare(EcasEvent e, EcasContext ctx);

	public sealed class EcasEventType : IEcasParameterized
	{
		private readonly PwUuid m_puTypeE;
		public PwUuid Type { get { return m_puTypeE; } }

		private readonly string m_strNameE;
		public string Name { get { return m_strNameE; } }

		private readonly PwIcon m_piE;
		public PwIcon Icon { get { return m_piE; } }

		private readonly EcasParameter[] m_vParamsE;
		public EcasParameter[] Parameters { get { return m_vParamsE; } }

		private readonly EcasEventCompare m_fn;
		public EcasEventCompare CompareMethod { get { return m_fn; } }

		private static bool EcasEventCompareTrue(EcasEvent e, EcasContext ctx)
		{
			return true;
		}

		public EcasEventType(PwUuid puType, string strName, PwIcon pi,
			EcasParameter[] vParams, EcasEventCompare f)
		{
			if(puType == null) throw new ArgumentNullException("puType");
			if(puType.IsZero) throw new ArgumentOutOfRangeException("puType");
			if(strName == null) throw new ArgumentNullException("strName");

			m_puTypeE = puType;
			m_strNameE = strName;
			m_piE = pi;
			m_vParamsE = (vParams ?? EcasParameter.EmptyArray);
			m_fn = (f ?? EcasEventCompareTrue);
		}
	}
}
