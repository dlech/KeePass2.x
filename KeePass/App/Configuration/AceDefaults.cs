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
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;

using KeePass.Util;

using KeePassLib.Keys;
using KeePassLib.Serialization;
using KeePassLib.Utility;

namespace KeePass.App.Configuration
{
	public sealed class AceKeyAssoc
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

		private bool m_bPassword = false;
		[DefaultValue(false)]
		public bool Password
		{
			get { return m_bPassword; }
			set { m_bPassword = value; }
		}

		private string m_strKey = string.Empty;
		[DefaultValue("")]
		public string KeyFilePath
		{
			get { return m_strKey; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strKey = value;
			}
		}

		private string m_strProv = string.Empty;
		[DefaultValue("")]
		public string KeyProvider
		{
			get { return m_strProv; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strProv = value;
			}
		}

		private bool m_bUserAcc = false;
		[DefaultValue(false)]
		public bool UserAccount
		{
			get { return m_bUserAcc; }
			set { m_bUserAcc = value; }
		}

		public AceKeyAssoc() { }
	}

	public sealed class AceDefaults
	{
		public AceDefaults()
		{
		}

		private int m_nNewEntryExpireDays = -1;
		[DefaultValue(-1)]
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

		private const string DefaultTanChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-";
		private string m_strTanChars = DefaultTanChars;
		[DefaultValue(DefaultTanChars)]
		public string TanCharacters
		{
			get { return m_strTanChars; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strTanChars = value;
			}
		}

		private bool m_bExpireTansOnUse = true;
		[DefaultValue(true)]
		public bool TanExpiresOnUse
		{
			get { return m_bExpireTansOnUse; }
			set { m_bExpireTansOnUse = value; }
		}

		private string m_strDbSaveAsPath = string.Empty;
		[DefaultValue("")]
		public string FileSaveAsDirectory
		{
			get { return m_strDbSaveAsPath; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strDbSaveAsPath = value;
			}
		}

		private bool m_bRememberKeySources = true;
		[DefaultValue(true)]
		public bool RememberKeySources
		{
			get { return m_bRememberKeySources; }
			set { m_bRememberKeySources = value; }
		}

		private List<AceKeyAssoc> m_vKeySources = new List<AceKeyAssoc>();
		[XmlArrayItem("Association")]
		public List<AceKeyAssoc> KeySources
		{
			get { return m_vKeySources; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_vKeySources = value;
			}
		}

		private string m_strCustomColors = string.Empty;
		[DefaultValue("")]
		public string CustomColors
		{
			get { return m_strCustomColors; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strCustomColors = value;
			}
		}

		[DefaultValue(false)]
		public bool RecycleBinCollapse { get; set; }

		private AceDuplication m_aceDup = new AceDuplication();
		public AceDuplication Duplication
		{
			get { return m_aceDup; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_aceDup = value;
			}
		}

		private AcePrint m_acePrint = new AcePrint();
		public AcePrint Print
		{
			get { return m_acePrint; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_acePrint = value;
			}
		}

		private bool m_bExportMasterKeySpec = false;
		[DefaultValue(false)]
		public bool ExportMasterKeySpec
		{
			get { return m_bExportMasterKeySpec; }
			set { m_bExportMasterKeySpec = value; }
		}

		private bool m_bExportParentGroups = false;
		[DefaultValue(false)]
		public bool ExportParentGroups
		{
			get { return m_bExportParentGroups; }
			set { m_bExportParentGroups = value; }
		}

		private bool m_bExportPostOpen = false;
		[DefaultValue(false)]
		public bool ExportPostOpen
		{
			get { return m_bExportPostOpen; }
			set { m_bExportPostOpen = value; }
		}

		private bool m_bExportPostShow = false;
		[DefaultValue(false)]
		public bool ExportPostShow
		{
			get { return m_bExportPostShow; }
			set { m_bExportPostShow = value; }
		}

		private string m_strWinFavsBaseName = string.Empty;
		[DefaultValue("")]
		public string WinFavsBaseFolderName
		{
			get { return m_strWinFavsBaseName; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strWinFavsBaseName = value;
			}
		}

		private string m_strWinFavsFilePrefix = string.Empty;
		[DefaultValue("")]
		public string WinFavsFileNamePrefix
		{
			get { return m_strWinFavsFilePrefix; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strWinFavsFilePrefix = value;
			}
		}

		private string m_strWinFavsFileSuffix = string.Empty;
		[DefaultValue("")]
		public string WinFavsFileNameSuffix
		{
			get { return m_strWinFavsFileSuffix; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strWinFavsFileSuffix = value;
			}
		}

		private static string GetKeyAssocID(IOConnectionInfo iocDb)
		{
			if(iocDb == null) throw new ArgumentNullException("iocDb");

			string strDb = iocDb.Path;
			if((strDb.Length > 0) && iocDb.IsLocalFile() &&
				!UrlUtil.IsAbsolutePath(strDb))
				strDb = UrlUtil.MakeAbsolutePath(WinUtil.GetExecutable(), strDb);

			return strDb;
		}

		private int GetKeyAssocIndex(string strID)
		{
			for(int i = 0; i < m_vKeySources.Count; ++i)
			{
				if(strID.Equals(m_vKeySources[i].DatabasePath, StrUtil.CaseIgnoreCmp))
					return i;
			}

			return -1;
		}

		public void SetKeySources(IOConnectionInfo iocDb, CompositeKey cmpKey)
		{
			string strID = GetKeyAssocID(iocDb);
			int idx = GetKeyAssocIndex(strID);

			if((cmpKey == null) || !m_bRememberKeySources)
			{
				if(idx >= 0) m_vKeySources.RemoveAt(idx);
				return;
			}

			AceKeyAssoc a = new AceKeyAssoc();
			a.DatabasePath = strID;

			IUserKey kcpPassword = cmpKey.GetUserKey(typeof(KcpPassword));
			a.Password = (kcpPassword != null);

			IUserKey kcpFile = cmpKey.GetUserKey(typeof(KcpKeyFile));
			if(kcpFile != null)
			{
				string strKeyFile = ((KcpKeyFile)kcpFile).Path;
				if(!string.IsNullOrEmpty(strKeyFile) && !StrUtil.IsDataUri(strKeyFile))
				{
					if(!UrlUtil.IsAbsolutePath(strKeyFile))
						strKeyFile = UrlUtil.MakeAbsolutePath(WinUtil.GetExecutable(),
							strKeyFile);

					a.KeyFilePath = strKeyFile;
				}
			}

			IUserKey kcpCustom = cmpKey.GetUserKey(typeof(KcpCustomKey));
			if(kcpCustom != null)
				a.KeyProvider = ((KcpCustomKey)kcpCustom).Name;

			IUserKey kcpUser = cmpKey.GetUserKey(typeof(KcpUserAccount));
			a.UserAccount = (kcpUser != null);

			bool bAtLeastOne = (a.Password || (a.KeyFilePath.Length > 0) ||
				(a.KeyProvider.Length > 0) || a.UserAccount);
			if(bAtLeastOne)
			{
				if(idx >= 0) m_vKeySources[idx] = a;
				else m_vKeySources.Add(a);
			}
			else if(idx >= 0) m_vKeySources.RemoveAt(idx);
		}

		public AceKeyAssoc GetKeySources(IOConnectionInfo iocDb)
		{
			string strID = GetKeyAssocID(iocDb);
			int idx = GetKeyAssocIndex(strID);

			if(!m_bRememberKeySources) return null;

			if(idx >= 0) return m_vKeySources[idx];
			return null;
		}
	}

	public sealed class AceDuplication
	{
		private bool m_bExtendTitle = true;
		[DefaultValue(true)]
		public bool ExtendTitle
		{
			get { return m_bExtendTitle; }
			set { m_bExtendTitle = value; }
		}

		[DefaultValue(false)]
		public bool CreateFieldReferences { get; set; }

		private bool m_bCopyHistory = true;
		[DefaultValue(true)]
		public bool CopyHistory
		{
			get { return m_bCopyHistory; }
			set { m_bCopyHistory = value; }
		}
	}

	public enum AcePrintLayout
	{
		None = 0,
		Tables,
		Blocks
	}

	public sealed class AcePrint
	{
		private AcePrintLayout m_lay = AcePrintLayout.Tables;
		[DefaultValue(AcePrintLayout.Tables)]
		public AcePrintLayout Layout
		{
			get { return m_lay; }
			set { m_lay = value; }
		}

		private bool m_bIncTitle = true;
		[DefaultValue(true)]
		public bool IncludeTitle
		{
			get { return m_bIncTitle; }
			set { m_bIncTitle = value; }
		}

		private bool m_bIncUserName = true;
		[DefaultValue(true)]
		public bool IncludeUserName
		{
			get { return m_bIncUserName; }
			set { m_bIncUserName = value; }
		}

		private bool m_bIncPassword = true;
		[DefaultValue(true)]
		public bool IncludePassword
		{
			get { return m_bIncPassword; }
			set { m_bIncPassword = value; }
		}

		[DefaultValue(false)]
		public bool IncludeUrl { get; set; }

		private bool m_bIncNotes = true;
		[DefaultValue(true)]
		public bool IncludeNotes
		{
			get { return m_bIncNotes; }
			set { m_bIncNotes = value; }
		}

		[DefaultValue(false)]
		public bool IncludeCreationTime { get; set; }
		[DefaultValue(false)]
		public bool IncludeLastModificationTime { get; set; }
		[DefaultValue(false)]
		public bool IncludeExpiryTime { get; set; }

		[DefaultValue(false)]
		public bool IncludeAutoType { get; set; }
		[DefaultValue(false)]
		public bool IncludeTags { get; set; }
		[DefaultValue(false)]
		public bool IncludeIcon { get; set; }
		[DefaultValue(false)]
		public bool IncludeCustomStrings { get; set; }
		[DefaultValue(false)]
		public bool IncludeGroupName { get; set; }
		[DefaultValue(false)]
		public bool IncludeUuid { get; set; }

		private bool m_bColorP = true;
		[DefaultValue(true)]
		public bool ColorP
		{
			get { return m_bColorP; }
			set { m_bColorP = value; }
		}

		private string m_strColorPU = "#0000FF";
		[DefaultValue("#0000FF")]
		public string ColorPU
		{
			get { return m_strColorPU; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strColorPU = value;
			}
		}

		private string m_strColorPL = "#000000";
		[DefaultValue("#000000")]
		public string ColorPL
		{
			get { return m_strColorPL; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strColorPL = value;
			}
		}

		private string m_strColorPD = "#008000";
		[DefaultValue("#008000")]
		public string ColorPD
		{
			get { return m_strColorPD; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strColorPD = value;
			}
		}

		private string m_strColorPO = "#C00000";
		[DefaultValue("#C00000")]
		public string ColorPO
		{
			get { return m_strColorPO; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strColorPO = value;
			}
		}

		private AceFont m_fontMain = new AceFont();
		public AceFont MainFont
		{
			get { return m_fontMain; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_fontMain = value;
			}
		}

		private AceFont m_fontPassword = new AceFont(true);
		public AceFont PasswordFont
		{
			get { return m_fontPassword; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_fontPassword = value;
			}
		}

		[DefaultValue(false)]
		public bool SortEntries { get; set; }

		private AceColumnType m_ctSortEntries = AceColumnType.Title;
		[DefaultValue(AceColumnType.Title)]
		public AceColumnType SortEntriesBy
		{
			get { return m_ctSortEntries; }
			set { m_ctSortEntries = value; }
		}

		[DefaultValue(0)]
		public int Spr { get; set; }
	}
}
