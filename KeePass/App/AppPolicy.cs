/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2010 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Diagnostics;
using System.Windows.Forms;

using KeePass.Resources;

using KeePassLib;
using KeePassLib.Utility;

namespace KeePass.App
{
	/// <summary>
	/// Application policy IDs.
	/// </summary>
	public enum AppPolicyId
	{
		Plugins = 0,
		Export,
		Import,
		Print,
		SaveFile,
		AutoType,
		CopyToClipboard,
		DragDrop,
		ChangeMasterKey,
		EditTriggers,
		UnhidePasswords
	}

	/// <summary>
	/// Application policy flags.
	/// </summary>
	public sealed class AppPolicyFlags
	{
		private bool m_bPlugins = true;
		public bool Plugins
		{
			get { return m_bPlugins; }
			set { m_bPlugins = value; }
		}

		private bool m_bExport = true;
		public bool Export
		{
			get { return m_bExport;}
			set { m_bExport = value;}
		}

		private bool m_bImport = true;
		public bool Import
		{
			get { return m_bImport; }
			set { m_bImport = value; }
		}

		private bool m_bPrint = true;
		public bool Print
		{
			get { return m_bPrint; }
			set { m_bPrint = value; }
		}

		private bool m_bSave = true;
		public bool SaveFile
		{
			get { return m_bSave; }
			set { m_bSave = value; }
		}

		private bool m_bAutoType = true;
		public bool AutoType
		{
			get { return m_bAutoType; }
			set { m_bAutoType = value; }
		}

		private bool m_bClipboard = true;
		public bool CopyToClipboard
		{
			get { return m_bClipboard; }
			set { m_bClipboard = value; }
		}

		private bool m_bDragDrop = true;
		public bool DragDrop
		{
			get { return m_bDragDrop; }
			set { m_bDragDrop = value; }
		}

		private bool m_bChangeMasterKey = true;
		public bool ChangeMasterKey
		{
			get { return m_bChangeMasterKey; }
			set { m_bChangeMasterKey = value; }
		}

		private bool m_bTriggersEdit = true;
		public bool EditTriggers
		{
			get { return m_bTriggersEdit; }
			set { m_bTriggersEdit = value; }
		}

		private bool m_bUnhidePasswords = true;
		public bool UnhidePasswords
		{
			get { return m_bUnhidePasswords; }
			set { m_bUnhidePasswords = value; }
		}

		public AppPolicyFlags CloneDeep()
		{
			return (AppPolicyFlags)this.MemberwiseClone();
		}
	}

	/// <summary>
	/// Application policy settings.
	/// </summary>
	public static class AppPolicy
	{
		private static AppPolicyFlags m_apfCurrent = new AppPolicyFlags();
		// private static AppPolicyFlags m_apfNew = new AppPolicyFlags();

		public static AppPolicyFlags Current
		{
			get { return m_apfCurrent; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_apfCurrent = value;
			}
		}

		/* public static AppPolicyFlags New
		{
			get { return m_apfNew; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_apfNew = value;
			}
		} */

		private static string PolicyToString(AppPolicyId flag, bool bPrefix)
		{
			string str = (bPrefix ? "* " : string.Empty);
			str += KPRes.Feature + ": ";

			switch(flag)
			{
				case AppPolicyId.Plugins:
					str += KPRes.Plugins;
					break;
				case AppPolicyId.Export:
					str += KPRes.Export;
					break;
				case AppPolicyId.Import:
					str += KPRes.Import;
					break;
				case AppPolicyId.Print:
					str += KPRes.Print;
					break;
				case AppPolicyId.SaveFile:
					str += KPRes.SaveDatabase;
					break;
				case AppPolicyId.AutoType:
					str += KPRes.AutoType;
					break;
				case AppPolicyId.CopyToClipboard:
					str += KPRes.Clipboard;
					break;
				case AppPolicyId.DragDrop:
					str += KPRes.DragDrop;
					break;
				case AppPolicyId.ChangeMasterKey:
					str += KPRes.ChangeMasterKey;
					break;
				case AppPolicyId.EditTriggers:
					str += KPRes.TriggersEdit;
					break;
				case AppPolicyId.UnhidePasswords:
					str += KPRes.UnhidePasswords;
					break;
				default:
					Debug.Assert(false);
					str += KPRes.Unknown + ".";
					break;
			}

			str += MessageService.NewLine;
			if(bPrefix) str += "* ";
			str += KPRes.Description + ": ";

			switch(flag)
			{
				case AppPolicyId.Plugins:
					str += KPRes.PolicyPluginsDesc;
					break;
				case AppPolicyId.Export:
					str += KPRes.PolicyExportDesc;
					break;
				case AppPolicyId.Import:
					str += KPRes.PolicyImportDesc;
					break;
				case AppPolicyId.Print:
					str += KPRes.PolicyPrintDesc;
					break;
				case AppPolicyId.SaveFile:
					str += KPRes.PolicySaveDatabaseDesc;
					break;
				case AppPolicyId.AutoType:
					str += KPRes.PolicyAutoTypeDesc;
					break;
				case AppPolicyId.CopyToClipboard:
					str += KPRes.PolicyClipboardDesc;
					break;
				case AppPolicyId.DragDrop:
					str += KPRes.PolicyDragDropDesc;
					break;
				case AppPolicyId.ChangeMasterKey:
					str += KPRes.PolicyChangeMasterKey;
					break;
				case AppPolicyId.EditTriggers:
					str += KPRes.PolicyTriggersEditDesc;
					break;
				case AppPolicyId.UnhidePasswords:
					str += KPRes.UnhidePasswordsDesc;
					break;
				default:
					Debug.Assert(false);
					str += KPRes.Unknown + ".";
					break;
			}

			return str;
		}

		public static string RequiredPolicyMessage(AppPolicyId flag)
		{
			string str = KPRes.PolicyDisallowed + MessageService.NewParagraph;
			str += KPRes.PolicyRequiredFlag + ":" + MessageService.NewLine;
			str += PolicyToString(flag, true);

			return str;
		}

		public static bool Try(AppPolicyId flag)
		{
			bool bAllowed = true;

			switch(flag)
			{
				case AppPolicyId.Plugins: bAllowed = m_apfCurrent.Plugins; break;
				case AppPolicyId.Export: bAllowed = m_apfCurrent.Export; break;
				case AppPolicyId.Import: bAllowed = m_apfCurrent.Import; break;
				case AppPolicyId.Print: bAllowed = m_apfCurrent.Print; break;
				case AppPolicyId.SaveFile: bAllowed = m_apfCurrent.SaveFile; break;
				case AppPolicyId.AutoType: bAllowed = m_apfCurrent.AutoType; break;
				case AppPolicyId.CopyToClipboard: bAllowed = m_apfCurrent.CopyToClipboard; break;
				case AppPolicyId.DragDrop: bAllowed = m_apfCurrent.DragDrop; break;
				case AppPolicyId.ChangeMasterKey: bAllowed = m_apfCurrent.ChangeMasterKey; break;
				case AppPolicyId.EditTriggers: bAllowed = m_apfCurrent.EditTriggers; break;
				case AppPolicyId.UnhidePasswords: bAllowed = m_apfCurrent.UnhidePasswords; break;
				default: Debug.Assert(false); break;
			}

			if(bAllowed == false)
			{
				string strMsg = RequiredPolicyMessage(flag);
				MessageService.ShowWarning(strMsg);
			}

			return bAllowed;
		}
	}

	/*
	/// <summary>
	/// Application policy settings
	/// </summary>
	public static class AppPolicy
	{
		private static bool[] m_vCurPolicyFlags = new bool[(int)AppPolicyFlag.Count];
		private static bool[] m_vNewPolicyFlags = new bool[(int)AppPolicyFlag.Count];

		private static string PolicyToString(AppPolicyFlag flag)
		{
			string str = KPRes.Feature + @": ";

			switch(flag)
			{
				case AppPolicyFlag.Plugins:
					str += KPRes.Plugins;
					break;
				case AppPolicyFlag.Export:
					str += KPRes.Export;
					break;
				case AppPolicyFlag.Import:
					str += KPRes.Import;
					break;
				case AppPolicyFlag.Print:
					str += KPRes.Print;
					break;
				case AppPolicyFlag.SaveDatabase:
					str += KPRes.SaveDatabase;
					break;
				case AppPolicyFlag.AutoType:
					str += KPRes.AutoType;
					break;
				case AppPolicyFlag.CopyToClipboard:
					str += KPRes.Clipboard;
					break;
				case AppPolicyFlag.DragDrop:
					str += KPRes.DragDrop;
					break;
				case AppPolicyFlag.ChangeMasterKey:
					str += KPRes.ChangeMasterKey;
					break;
				case AppPolicyFlag.EditTriggers:
					str += KPRes.TriggersEdit;
					break;
				case AppPolicyFlag.UnhidePasswords:
					str += KPRes.UnhidePasswords;
					break;
				default:
					Debug.Assert(false);
					str += KPRes.Unknown + ".";
					break;
			}

			str += MessageService.NewLine + KPRes.Description + @": ";

			switch(flag)
			{
				case AppPolicyFlag.Plugins:
					str += KPRes.PolicyPluginsDesc;
					break;
				case AppPolicyFlag.Export:
					str += KPRes.PolicyExportDesc;
					break;
				case AppPolicyFlag.Import:
					str += KPRes.PolicyImportDesc;
					break;
				case AppPolicyFlag.Print:
					str += KPRes.PolicyPrintDesc;
					break;
				case AppPolicyFlag.SaveDatabase:
					str += KPRes.PolicySaveDatabaseDesc;
					break;
				case AppPolicyFlag.AutoType:
					str += KPRes.PolicyAutoTypeDesc;
					break;
				case AppPolicyFlag.CopyToClipboard:
					str += KPRes.PolicyClipboardDesc;
					break;
				case AppPolicyFlag.DragDrop:
					str += KPRes.PolicyDragDropDesc;
					break;
				case AppPolicyFlag.ChangeMasterKey:
					str += KPRes.PolicyChangeMasterKey;
					break;
				case AppPolicyFlag.EditTriggers:
					str += KPRes.PolicyTriggersEditDesc;
					break;
				case AppPolicyFlag.UnhidePasswords:
					str += KPRes.UnhidePasswordsDesc;
					break;
				default:
					Debug.Assert(false);
					str += KPRes.Unknown + ".";
					break;
			}

			return str;
		}

		public static string RequiredPolicyMessage(AppPolicyFlag flag)
		{
			string str = KPRes.PolicyDisallowed + MessageService.NewParagraph;
			str += KPRes.PolicyRequiredFlag + ":" + MessageService.NewLine;
			str += PolicyToString(flag);

			return str;
		}

		public static void CurrentAllowAll(bool bAllow)
		{
			for(int i = 0; i < (int)AppPolicyFlag.Count; ++i)
				m_vCurPolicyFlags[i] = m_vNewPolicyFlags[i] = bAllow;
		}

		public static void CurrentAllow(AppPolicyFlag flag, bool bAllow)
		{
			m_vCurPolicyFlags[(int)flag] = m_vNewPolicyFlags[(int)flag] = bAllow;
		}

		public static void NewAllow(AppPolicyFlag flag, bool bAllow)
		{
			m_vNewPolicyFlags[(int)flag] = bAllow;
		}

		public static bool NewIsAllowed(AppPolicyFlag flag)
		{
			return m_vNewPolicyFlags[(int)flag];
		}

		public static bool IsAllowed(AppPolicyFlag flag)
		{
			return m_vCurPolicyFlags[(int)flag];
		}

		public static bool Try(AppPolicyFlag flag)
		{
			bool bAllowed = m_vCurPolicyFlags[(int)flag];

			if(bAllowed == false)
			{
				string strMsg = RequiredPolicyMessage(flag);
				MessageService.ShowWarning(strMsg);
			}

			return bAllowed;
		}
	}
	*/
}
