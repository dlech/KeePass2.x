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
using System.Xml.Serialization;

using KeePass.Util;

using KeePassLib;
using KeePassLib.Utility;

namespace KeePass.App.Configuration
{
	public sealed class AceKeyFilePath
	{
		private string m_strDb = string.Empty;
		public string DatabasePath
		{
			get { return m_strDb; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strDb = value;
			}
		}

		private string m_strKey = string.Empty;
		public string KeyFilePath
		{
			get { return m_strKey; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strKey = value;
			}
		}

		public AceKeyFilePath() { }
	}

	public sealed class AceDefaults
	{
		public AceDefaults()
		{
		}

		private int m_nNewEntryExpireDays = -1;
		public int NewEntryExpiresInDays
		{
			get { return m_nNewEntryExpireDays; }
			set { m_nNewEntryExpireDays = value; }
		}

		private uint m_uDefaultOptionsTab = 0;
		public uint OptionsTabIndex
		{
			get { return m_uDefaultOptionsTab; }
			set { m_uDefaultOptionsTab = value; }
		}

		private string m_strTanChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-";
		public string TanCharacters
		{
			get { return m_strTanChars; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strTanChars = value;
			}
		}

		private SearchParameters m_searchParams = new SearchParameters();
		public SearchParameters SearchParameters
		{
			get { return m_searchParams; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_searchParams = value;
			}
		}

		private bool m_bRememberKeyFilePaths = true;
		public bool RememberKeyFilePaths
		{
			get { return m_bRememberKeyFilePaths; }
			set { m_bRememberKeyFilePaths = value; }
		}

		private List<AceKeyFilePath> m_vKeyFilePaths = new List<AceKeyFilePath>();
		[XmlArrayItem("Association")]
		public List<AceKeyFilePath> KeyFilePaths
		{
			get { return m_vKeyFilePaths; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_vKeyFilePaths = value;
			}
		}

		public void SetKeyFilePath(string strDatabase, string strKeyFile)
		{
			if(strDatabase == null) throw new ArgumentNullException("strDatabase");

			string strDb = strDatabase;
			if((strDb.Length > 0) && !UrlUtil.IsAbsolutePath(strDb))
				strDb = UrlUtil.MakeAbsolutePath(WinUtil.GetExecutable(), strDb);

			string strKey = strKeyFile;
			if(!string.IsNullOrEmpty(strKey) && !UrlUtil.IsAbsolutePath(strKey))
				strKey = UrlUtil.MakeAbsolutePath(WinUtil.GetExecutable(), strKey);

			if(!m_bRememberKeyFilePaths) strKey = null;

			foreach(AceKeyFilePath kfp in m_vKeyFilePaths)
			{
				if(strDb.Equals(kfp.DatabasePath, StrUtil.CaseIgnoreCmp))
				{
					if(string.IsNullOrEmpty(strKey)) m_vKeyFilePaths.Remove(kfp);
					else kfp.KeyFilePath = strKey;
					return;
				}
			}

			if(string.IsNullOrEmpty(strKey)) return;

			AceKeyFilePath kfpNew = new AceKeyFilePath();
			kfpNew.DatabasePath = strDb;
			kfpNew.KeyFilePath = strKey;
			m_vKeyFilePaths.Add(kfpNew);
		}

		public string GetKeyFilePath(string strDatabase)
		{
			if(strDatabase == null) throw new ArgumentNullException("strDatabase");

			string strDb = strDatabase;
			if((strDb.Length > 0) && !UrlUtil.IsAbsolutePath(strDb))
				strDb = UrlUtil.MakeAbsolutePath(WinUtil.GetExecutable(), strDb);

			foreach(AceKeyFilePath kfp in m_vKeyFilePaths)
			{
				if(strDb.Equals(kfp.DatabasePath, StrUtil.CaseIgnoreCmp))
					return kfp.KeyFilePath;
			}

			return null;
		}
	}
}
