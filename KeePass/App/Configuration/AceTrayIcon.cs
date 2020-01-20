/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2020 Dominik Reichl <dominik.reichl@t-online.de>

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

namespace KeePass.App.Configuration
{
	public sealed class AceTrayIcon
	{
		public AceTrayIcon()
		{
		}

		[Obsolete] // For backward compatibility with plugins only
		[XmlIgnore]
		public bool ShowOnlyIfTrayed
		{
			get { return this.ShowOnlyIfTrayedEx; }
			// Old setting should not affect 'ShowOnlyIfTrayedEx';
			// a reset of this option is intended
			set { }
		}

		// Not available through the options dialog, see documentation;
		// 'ShowOnlyIfTrayed' was used by KeePass <= 2.41
		private bool m_bOnlyIfTrayedEx = false;
		[DefaultValue(false)]
		public bool ShowOnlyIfTrayedEx
		{
			get { return m_bOnlyIfTrayedEx; }
			set { m_bOnlyIfTrayedEx = value; }
		}

		private bool m_bGrayIcon = false;
		[DefaultValue(false)]
		public bool GrayIcon
		{
			get { return m_bGrayIcon; }
			set { m_bGrayIcon = value; }
		}

		private bool m_bSingleClickDefault = false;
		[DefaultValue(false)]
		public bool SingleClickDefault
		{
			get { return m_bSingleClickDefault; }
			set { m_bSingleClickDefault = value; }
		}
	}
}
