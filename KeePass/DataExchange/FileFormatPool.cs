/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2022 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using KeePass.DataExchange.Formats;
using KeePass.UI;

using KeePassLib.Utility;

namespace KeePass.DataExchange
{
	public sealed class FileFormatPool : IEnumerable<FileFormatProvider>
	{
		private readonly List<FileFormatProvider> m_lFormats = new List<FileFormatProvider>();

		public IEnumerable<FileFormatProvider> Importers
		{
			get
			{
				List<FileFormatProvider> l = new List<FileFormatProvider>();
				foreach(FileFormatProvider p in m_lFormats)
				{
					if(p.SupportsImport) l.Add(p);
				}
				return l;
			}
		}

		public IEnumerable<FileFormatProvider> Exporters
		{
			get
			{
				List<FileFormatProvider> l = new List<FileFormatProvider>();
				foreach(FileFormatProvider p in m_lFormats)
				{
					if(p.SupportsExport) l.Add(p);
				}
				return l;
			}
		}

		public int Count
		{
			get { return m_lFormats.Count; }
		}

		public FileFormatPool()
		{
			List<FileFormatProvider> l = m_lFormats;

			l.Add(new KeePassCsv1x());
			l.Add(new KeePassKdb1x());
			l.Add(new KeePassKdb2x());
			l.Add(new KeePassKdb2xRepair());
			l.Add(new KeePassKdb2x3());
			l.Add(new KeePassXml1x());
			l.Add(new KeePassXml2x());

			l.Add(new GenericCsv());

			l.Add(new KeePassHtml2x());
			l.Add(new XslTransform2x());
			l.Add(new WinFavorites10(false));
			l.Add(new WinFavorites10(true));

			l.Add(new OnePw1Pux8());
			l.Add(new OnePwProCsv599());
			l.Add(new AmpXml250());
			l.Add(new AnyPwCsv144());
			l.Add(new BitwardenJson112());
			l.Add(new CodeWalletTxt605());
			l.Add(new DashlaneCsv2());
			l.Add(new DashlaneJson6());
			l.Add(new DataVaultCsv47());
			l.Add(new DesktopKnoxXml32());
			l.Add(new EnpassTxt5());
			l.Add(new FlexWalletXml17());
			l.Add(new HandySafeTxt512());
			l.Add(new HandySafeProXml12());
			l.Add(new KasperskyPwMgrTxt90());
			l.Add(new KasperskyPwMgrXml50());
			l.Add(new KeePassXXml041());
			l.Add(new KeeperJson16());
			l.Add(new KeyFolderXml1());
			l.Add(new LastPassCsv2());
			l.Add(new MSecureCsv355());
			l.Add(new NetworkPwMgrCsv4());
			l.Add(new NortonIdSafeCsv2013());
			l.Add(new NPasswordNpw102());
			l.Add(new PassKeeper12());
			l.Add(new PpKeeperHtml270());
			l.Add(new PwAgentXml3());
			l.Add(new PwDepotXml26());
			l.Add(new PwKeeperCsv70());
			l.Add(new PwMemory2008Xml104());
			l.Add(new PwPrompterDat12());
			l.Add(new PwSafeXml302());
			l.Add(new PwSaverXml412());
			l.Add(new PwsPlusCsv1007());
			l.Add(new PwTresorXml100());
			l.Add(new PVaultTxt14());
			l.Add(new PinsTxt450());
			l.Add(new RevelationXml04());
			l.Add(new RoboFormHtml69());
			l.Add(new SafeWalletXml3());
			l.Add(new SecurityTxt12());
			l.Add(new SplashIdCsv402());
			l.Add(new SteganosCsv20());
			l.Add(new SteganosUI2007());
			l.Add(new StickyPwXml50());
			l.Add(new TrueKeyCsv4());
			l.Add(new TurboPwsCsv5());
			l.Add(new VisKeeperTxt3());
			l.Add(new Whisper32Csv116());
			l.Add(new ZdnPwProTxt314());

			l.Add(new ChromeCsv66());
			l.Add(new MozillaBookmarksHtml100());
			l.Add(new MozillaBookmarksJson100());
			l.Add(new PwExporterXml105());

			l.Add(new Spamex20070328());

#if DEBUG
			// Ensure name uniqueness
			for(int i = 0; i < l.Count; ++i)
			{
				FileFormatProvider pi = l[i];
				for(int j = i + 1; j < l.Count; ++j)
				{
					FileFormatProvider pj = l[j];
					Debug.Assert(!string.Equals(pi.FormatName, pj.FormatName, StrUtil.CaseIgnoreCmp));
					Debug.Assert(!string.Equals(pi.FormatName, pj.DisplayName, StrUtil.CaseIgnoreCmp));
					Debug.Assert(!string.Equals(pi.DisplayName, pj.FormatName, StrUtil.CaseIgnoreCmp));
					Debug.Assert(!string.Equals(pi.DisplayName, pj.DisplayName, StrUtil.CaseIgnoreCmp));
				}
			}

			foreach(FileFormatProvider p in l)
			{
				Type t = p.GetType();
				Debug.Assert(t.IsNotPublic);
				Debug.Assert(t.IsSealed || l.Exists(px => px.GetType().IsSubclassOf(t)));

				string strExts = p.DefaultExtension;
				if(!string.IsNullOrEmpty(strExts))
				{
					Debug.Assert(!strExts.StartsWith("."));
					Debug.Assert(strExts.ToLower() == strExts);

					string strExtU = UIUtil.GetPrimaryFileTypeExt(strExts).ToUpper();
					Debug.Assert(p.DisplayName.EndsWith(" " + strExtU) ||
						p.DisplayName.Contains(" " + strExtU + " ") ||
						p.DisplayName.Contains(" " + strExtU + "-"));
				}
			}
#endif
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return m_lFormats.GetEnumerator();
		}

		public IEnumerator<FileFormatProvider> GetEnumerator()
		{
			return m_lFormats.GetEnumerator();
		}

		public void Add(FileFormatProvider p)
		{
			if(p == null) { Debug.Assert(false); throw new ArgumentNullException("p"); }

			m_lFormats.Add(p);
		}

		public bool Remove(FileFormatProvider p)
		{
			if(p == null) { Debug.Assert(false); throw new ArgumentNullException("p"); }

			return m_lFormats.Remove(p);
		}

		public FileFormatProvider Find(string strName)
		{
			if(string.IsNullOrEmpty(strName)) return null;

			// Format and display names may differ (e.g. the Generic
			// CSV Importer has different names)

			foreach(FileFormatProvider p in m_lFormats)
			{
				if(string.Equals(strName, p.DisplayName, StrUtil.CaseIgnoreCmp))
					return p;
			}

			foreach(FileFormatProvider p in m_lFormats)
			{
				if(string.Equals(strName, p.FormatName, StrUtil.CaseIgnoreCmp))
					return p;
			}

			return null;
		}
	}
}
