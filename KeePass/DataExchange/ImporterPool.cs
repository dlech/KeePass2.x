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

			m_vImporters.Add(new KeePassKdb1x());
			m_vImporters.Add(new KeePassKdb2x());
			m_vImporters.Add(new KeePassXml2x());

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

			m_vImporters.Add(new MozillaBookmarksHtml100());
			m_vImporters.Add(new PwExporterXml105());
		}
	}
}
