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
using System.Text;

namespace KeePassLib.Utility
{
	internal sealed class ExtendedException : Exception
	{
		private readonly string m_strMessageStart;
		internal string MessageStart { get { return m_strMessageStart; } }

		private readonly string m_strMessageEnd;
		internal string MessageEnd { get { return m_strMessageEnd; } }

		public ExtendedException(string strMessageStart, Exception exInner) :
			base(ConstructMessage(strMessageStart, exInner, null), exInner)
		{
			m_strMessageStart = (strMessageStart ?? string.Empty).Trim();
			m_strMessageEnd = string.Empty;
		}

		public ExtendedException(string strMessageStart, Exception exInner,
			string strMessageEnd) :
			base(ConstructMessage(strMessageStart, exInner, strMessageEnd), exInner)
		{
			m_strMessageStart = (strMessageStart ?? string.Empty).Trim();
			m_strMessageEnd = (strMessageEnd ?? string.Empty).Trim();
		}

		private static string ConstructMessage(string strMessageStart,
			Exception exInner, string strMessageEnd)
		{
			StringBuilder sb = new StringBuilder();
			string strNP = MessageService.NewParagraph;

			StrUtil.AppendTrim(sb, null, strMessageStart);
			StrUtil.AppendTrim(sb, strNP, ((exInner != null) ? exInner.Message : null));
			StrUtil.AppendTrim(sb, strNP, strMessageEnd);

			return sb.ToString();
		}
	}
}
