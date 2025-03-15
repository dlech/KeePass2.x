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

using KeePassLib.Resources;
using KeePassLib.Utility;

namespace KeePassLib.Serialization
{
	public sealed class OldFormatException : Exception
	{
		private readonly string m_strFormat;
		private readonly OldFormatType m_type;

		public enum OldFormatType
		{
			Unknown = 0,
			KeePass1x = 1
		}

		public override string Message
		{
			get
			{
				string str = KLRes.OldFormat + ((m_strFormat.Length != 0) ?
					(" (" + m_strFormat + ")") : string.Empty) + ".";

				if(m_type == OldFormatType.KeePass1x)
					str += MessageService.NewParagraph + KLRes.KeePass1xHint;

				return str;
			}
		}

		public OldFormatException(string strFormatName) :
			this(strFormatName, OldFormatType.Unknown)
		{
		}

		public OldFormatException(string strFormatName, OldFormatType t)
		{
			m_strFormat = (strFormatName ?? string.Empty);
			m_type = t;
		}
	}
}
