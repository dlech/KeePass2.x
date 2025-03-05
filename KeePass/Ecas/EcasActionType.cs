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
	public delegate void EcasActionExecute(EcasAction a, EcasContext ctx);

	public sealed class EcasActionType : IEcasParameterized
	{
		private readonly PwUuid m_puTypeA;
		public PwUuid Type { get { return m_puTypeA; } }

		private readonly string m_strNameA;
		public string Name { get { return m_strNameA; } }

		private readonly PwIcon m_piA;
		public PwIcon Icon { get { return m_piA; } }

		private readonly EcasParameter[] m_vParamsA;
		public EcasParameter[] Parameters { get { return m_vParamsA; } }

		private readonly EcasActionExecute m_fn;
		public EcasActionExecute ExecuteMethod { get { return m_fn; } }

		private static void EcasActionExecuteNull(EcasAction a, EcasContext ctx)
		{
		}

		public EcasActionType(PwUuid puType, string strName, PwIcon pi,
			EcasParameter[] vParams, EcasActionExecute f)
		{
			if(puType == null) throw new ArgumentNullException("puType");
			if(puType.IsZero) throw new ArgumentOutOfRangeException("puType");
			if(strName == null) throw new ArgumentNullException("strName");

			m_puTypeA = puType;
			m_strNameA = strName;
			m_piA = pi;
			m_vParamsA = (vParams ?? EcasParameter.EmptyArray);
			m_fn = (f ?? EcasActionExecuteNull);
		}
	}
}
