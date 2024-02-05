/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2024 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePassLib.Serialization;

namespace TrlUtil.App.Configuration
{
	public sealed class TceApplication
	{
		private IOConnectionInfo m_ioLastFile = null;
		public IOConnectionInfo LastUsedFile
		{
			get
			{
				if(m_ioLastFile == null) m_ioLastFile = new IOConnectionInfo();
				return m_ioLastFile;
			}
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_ioLastFile = value;
			}
		}
	}
}
