/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2009 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Diagnostics;
using System.IO;

using KeePass.Resources;
using KeePass.Util.Spr;

using KeePassLib;

namespace KeePass.Ecas
{
	internal sealed class EcasDefaultConditionProvider : EcasConditionProvider
	{
		public EcasDefaultConditionProvider()
		{
			m_conditions.Add(new EcasConditionType(new PwUuid(new byte[] {
				0x9F, 0x11, 0xD0, 0xBD, 0xEC, 0xE9, 0x45, 0x3B,
				0xA5, 0x45, 0x26, 0x1F, 0xF7, 0xA4, 0xFF, 0x1F }),
				KPRes.EnvironmentVariable, PwIcon.Console, new EcasParameter[] {
					new EcasParameter(KPRes.Name, EcasValueType.String, null),
					new EcasParameter(KPRes.Value + " - " + KPRes.Comparison,
						EcasValueType.EnumStrings, EcasUtil.StdStringCompare),
					new EcasParameter(KPRes.Value, EcasValueType.String, null) },
				IsMatchEnvironmentVar));

			m_conditions.Add(new EcasConditionType(new PwUuid(new byte[] {
				0xCB, 0x4A, 0x9E, 0x34, 0x56, 0x8C, 0x4C, 0x95,
				0xAD, 0x67, 0x4D, 0x1C, 0xA1, 0x04, 0x19, 0xBC }),
				KPRes.FileExists, PwIcon.PaperReady, new EcasParameter[] {
					new EcasParameter(KPRes.File, EcasValueType.String, null) },
				IsMatchFile));
		}

		private bool IsMatchEnvironmentVar(EcasCondition c, EcasContext ctx)
		{
			string strName = EcasUtil.GetParamString(c.Parameters, 0);
			uint uCompareType = EcasUtil.GetParamEnum(c.Parameters, 1,
				EcasUtil.StdStringCompareEquals, EcasUtil.StdStringCompare);
			string strValue = EcasUtil.GetParamString(c.Parameters, 2);

			if(string.IsNullOrEmpty(strName) || (strValue == null))
				return false;

			try
			{
				string strVar = Environment.GetEnvironmentVariable(strName);
				if(strVar == null) return false;

				return EcasUtil.CompareStrings(strVar, strValue, uCompareType);
			}
			catch(Exception) { Debug.Assert(false); }

			return false;
		}

		private bool IsMatchFile(EcasCondition c, EcasContext ctx)
		{
			string strFileSpec = EcasUtil.GetParamString(c.Parameters, 0);
			if(string.IsNullOrEmpty(strFileSpec)) return true;

			string strFile = SprEngine.Compile(strFileSpec, false, null,
				null, false, false);
			if(string.IsNullOrEmpty(strFile)) return true;

			try { return File.Exists(strFile); }
			catch(Exception) { }

			return false;
		}
	}
}
