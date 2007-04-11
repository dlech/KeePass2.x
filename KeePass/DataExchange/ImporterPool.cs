/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2007 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePass.DataExchange.Formats;

namespace KeePass.DataExchange
{
	public static class ImporterPool
	{
		private static List<FormatImporter> m_vImporters = null;

		public static List<FormatImporter> Importers
		{
			get
			{
				if(m_vImporters == null) CreateDefaultImportersList();

				return m_vImporters;
			}
		}

		private static void CreateDefaultImportersList()
		{
			Debug.Assert(m_vImporters == null);

			m_vImporters = new List<FormatImporter>();

			m_vImporters.Add(new GenericCsv());

			m_vImporters.Add(new AmpXml250());
			m_vImporters.Add(new AnyPwCsv144());
			m_vImporters.Add(new CodeWalletTxt605());
			m_vImporters.Add(new PwAgentXml234());
			m_vImporters.Add(new PwDepotXml26());
			m_vImporters.Add(new PwGorillaCsv142());
			m_vImporters.Add(new PwSafeXml302());
			m_vImporters.Add(new PVaultTxt14());
			m_vImporters.Add(new PinsTxt450());
			m_vImporters.Add(new SecurityTxt12());
			m_vImporters.Add(new SteganosPwManager2007());
			m_vImporters.Add(new Whisper32Csv116());

			m_vImporters.Add(new KeePassCsv1x());
			m_vImporters.Add(new KeePassKdb1x());
			m_vImporters.Add(new KeePassKdb2x());
			m_vImporters.Add(new KeePassXml2x());

			m_vImporters.Add(new MozillaBookmarksHtml100());
			m_vImporters.Add(new PwExporterXml105());

			m_vImporters.Add(new Spamex20070328());
		}
	}
}
