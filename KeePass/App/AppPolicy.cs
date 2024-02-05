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
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

using KeePass.App.Configuration;
using KeePass.Resources;
using KeePass.Util;

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
		ExportNoKey, // Obsolete (for backward compatibility with plugins only)
		Import,
		Print,
		PrintNoKey, // Don't require the current key to be repeated
		NewFile,
		SaveFile,
		AutoType,
		AutoTypeWithoutContext,
		CopyToClipboard,
		CopyWholeEntries,
		DragDrop,
		UnhidePasswords,
		ChangeMasterKey,
		ChangeMasterKeyNoKey, // Don't require the current key to be repeated
		EditTriggers
	}

	/// <summary>
	/// Application policy flags.
	/// </summary>
	public sealed class AppPolicyFlags
	{
		private bool m_bPlugins = true;
		[DefaultValue(true)]
		public bool Plugins
		{
			get { return m_bPlugins; }
			set { m_bPlugins = value; }
		}

		private bool m_bExport = true;
		[DefaultValue(true)]
		public bool Export
		{
			get { return m_bExport; }
			set { m_bExport = value; }
		}

		// Obsolete (for backward compatibility with plugins only)
		[DefaultValue(false)]
		[XmlIgnore]
		public bool ExportNoKey
		{
			get { Debug.Assert(false); return false; }
			set { Debug.Assert(false); }
		}

		private bool m_bImport = true;
		[DefaultValue(true)]
		public bool Import
		{
			get { return m_bImport; }
			set { m_bImport = value; }
		}

		private bool m_bPrint = true;
		[DefaultValue(true)]
		public bool Print
		{
			get { return m_bPrint; }
			set { m_bPrint = value; }
		}

		private bool m_bPrintNoKey = true;
		[DefaultValue(true)]
		public bool PrintNoKey
		{
			get { return m_bPrintNoKey; }
			set { m_bPrintNoKey = value; }
		}

		private bool m_bNewFile = true;
		[DefaultValue(true)]
		public bool NewFile
		{
			get { return m_bNewFile; }
			set { m_bNewFile = value; }
		}

		private bool m_bSave = true;
		[DefaultValue(true)]
		public bool SaveFile
		{
			get { return m_bSave; }
			set { m_bSave = value; }
		}

		private bool m_bAutoType = true;
		[DefaultValue(true)]
		public bool AutoType
		{
			get { return m_bAutoType; }
			set { m_bAutoType = value; }
		}

		private bool m_bAutoTypeWithoutContext = true;
		[DefaultValue(true)]
		public bool AutoTypeWithoutContext
		{
			get { return m_bAutoTypeWithoutContext; }
			set { m_bAutoTypeWithoutContext = value; }
		}

		private bool m_bClipboard = true;
		[DefaultValue(true)]
		public bool CopyToClipboard
		{
			get { return m_bClipboard; }
			set { m_bClipboard = value; }
		}

		private bool m_bCopyWholeEntries = true;
		[DefaultValue(true)]
		public bool CopyWholeEntries
		{
			get { return m_bCopyWholeEntries; }
			set { m_bCopyWholeEntries = value; }
		}

		private bool m_bDragDrop = true;
		[DefaultValue(true)]
		public bool DragDrop
		{
			get { return m_bDragDrop; }
			set { m_bDragDrop = value; }
		}

		private bool m_bUnhidePasswords = true;
		[DefaultValue(true)]
		public bool UnhidePasswords
		{
			get { return m_bUnhidePasswords; }
			set { m_bUnhidePasswords = value; }
		}

		private bool m_bChangeMasterKey = true;
		[DefaultValue(true)]
		public bool ChangeMasterKey
		{
			get { return m_bChangeMasterKey; }
			set { m_bChangeMasterKey = value; }
		}

		private bool m_bChangeMasterKeyNoKey = true;
		[DefaultValue(true)]
		public bool ChangeMasterKeyNoKey
		{
			get { return m_bChangeMasterKeyNoKey; }
			set { m_bChangeMasterKeyNoKey = value; }
		}

		private bool m_bTriggersEdit = true;
		[DefaultValue(true)]
		public bool EditTriggers
		{
			get { return m_bTriggersEdit; }
			set { m_bTriggersEdit = value; }
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

		internal static string GetName(AppPolicyId p)
		{
			string str;

			switch(p)
			{
				case AppPolicyId.Plugins:
					str = KPRes.Plugins;
					break;
				case AppPolicyId.Export:
					str = KPRes.Export;
					break;
				case AppPolicyId.ExportNoKey:
					str = KPRes.Export + " - " + KPRes.NoKeyRepeat;
					break;
				case AppPolicyId.Import:
					str = KPRes.Import;
					break;
				case AppPolicyId.Print:
					str = KPRes.Print;
					break;
				case AppPolicyId.PrintNoKey:
					str = KPRes.Print + " - " + KPRes.NoKeyRepeat;
					break;
				case AppPolicyId.NewFile:
					str = KPRes.NewDatabase;
					break;
				case AppPolicyId.SaveFile:
					str = KPRes.SaveDatabase;
					break;
				case AppPolicyId.AutoType:
					str = KPRes.AutoType;
					break;
				case AppPolicyId.AutoTypeWithoutContext:
					str = KPRes.AutoType + " - " + KPRes.WithoutContext;
					break;
				case AppPolicyId.CopyToClipboard:
					str = KPRes.Copy;
					break;
				case AppPolicyId.CopyWholeEntries:
					str = KPRes.CopyWholeEntries;
					break;
				case AppPolicyId.DragDrop:
					str = KPRes.DragDrop;
					break;
				case AppPolicyId.UnhidePasswords:
					str = KPRes.UnhidePasswords;
					break;
				case AppPolicyId.ChangeMasterKey:
					str = KPRes.ChangeMasterKey;
					break;
				case AppPolicyId.ChangeMasterKeyNoKey:
					str = KPRes.ChangeMasterKey + " - " + KPRes.NoKeyRepeat;
					break;
				case AppPolicyId.EditTriggers:
					str = KPRes.TriggersEdit;
					break;
				default:
					Debug.Assert(false);
					str = KPRes.Unknown;
					break;
			}

			return str;
		}

		internal static string GetDesc(AppPolicyId p)
		{
			string str;

			switch(p)
			{
				case AppPolicyId.Plugins:
					str = KPRes.PolicyPluginsDesc;
					break;
				case AppPolicyId.Export:
					str = KPRes.PolicyExportDesc2;
					break;
				case AppPolicyId.ExportNoKey:
					str = KPRes.PolicyExportNoKeyDesc;
					break;
				case AppPolicyId.Import:
					str = KPRes.PolicyImportDesc;
					break;
				case AppPolicyId.Print:
					str = KPRes.PolicyPrintDesc;
					break;
				case AppPolicyId.PrintNoKey:
					str = KPRes.PolicyPrintNoKeyDesc;
					break;
				case AppPolicyId.NewFile:
					str = KPRes.PolicyNewDatabaseDesc;
					break;
				case AppPolicyId.SaveFile:
					str = KPRes.PolicySaveDatabaseDesc;
					break;
				case AppPolicyId.AutoType:
					str = KPRes.PolicyAutoTypeDesc;
					break;
				case AppPolicyId.AutoTypeWithoutContext:
					str = KPRes.PolicyAutoTypeWithoutContextDesc;
					break;
				case AppPolicyId.CopyToClipboard:
					str = KPRes.PolicyClipboardDesc;
					break;
				case AppPolicyId.CopyWholeEntries:
					str = KPRes.PolicyCopyWholeEntriesDesc;
					break;
				case AppPolicyId.DragDrop:
					str = KPRes.PolicyDragDropDesc;
					break;
				case AppPolicyId.UnhidePasswords:
					str = KPRes.UnhidePasswordsDesc;
					break;
				case AppPolicyId.ChangeMasterKey:
					str = KPRes.PolicyChangeMasterKey;
					break;
				case AppPolicyId.ChangeMasterKeyNoKey:
					str = KPRes.PolicyChangeMasterKeyNoKeyDesc;
					break;
				case AppPolicyId.EditTriggers:
					str = KPRes.PolicyTriggersEditDesc;
					break;
				default:
					Debug.Assert(false);
					str = KPRes.Unknown + ".";
					break;
			}

			return str;
		}

		public static string RequiredPolicyMessage(AppPolicyId p)
		{
			return (KPRes.PolicyDisallowed + MessageService.NewParagraph +
				KPRes.PolicyRequiredFlag + ":" + MessageService.NewLine +
				AppPolicy.GetName(p) + ".");
		}

		public static bool Try(AppPolicyId p)
		{
			bool b = true;

			switch(p)
			{
				case AppPolicyId.Plugins: b = m_apfCurrent.Plugins; break;
				case AppPolicyId.Export: b = m_apfCurrent.Export; break;
				case AppPolicyId.ExportNoKey: b = m_apfCurrent.ExportNoKey; break;
				case AppPolicyId.Import: b = m_apfCurrent.Import; break;
				case AppPolicyId.Print: b = m_apfCurrent.Print; break;
				case AppPolicyId.PrintNoKey: b = m_apfCurrent.PrintNoKey; break;
				case AppPolicyId.NewFile: b = m_apfCurrent.NewFile; break;
				case AppPolicyId.SaveFile: b = m_apfCurrent.SaveFile; break;
				case AppPolicyId.AutoType: b = m_apfCurrent.AutoType; break;
				case AppPolicyId.AutoTypeWithoutContext: b = m_apfCurrent.AutoTypeWithoutContext; break;
				case AppPolicyId.CopyToClipboard: b = m_apfCurrent.CopyToClipboard; break;
				case AppPolicyId.CopyWholeEntries: b = m_apfCurrent.CopyWholeEntries; break;
				case AppPolicyId.DragDrop: b = m_apfCurrent.DragDrop; break;
				case AppPolicyId.UnhidePasswords: b = m_apfCurrent.UnhidePasswords; break;
				case AppPolicyId.ChangeMasterKey: b = m_apfCurrent.ChangeMasterKey; break;
				case AppPolicyId.ChangeMasterKeyNoKey: b = m_apfCurrent.ChangeMasterKeyNoKey; break;
				case AppPolicyId.EditTriggers: b = m_apfCurrent.EditTriggers; break;
				default: Debug.Assert(false); break;
			}

			if(!b)
			{
				string strMsg = RequiredPolicyMessage(p);
				MessageService.ShowWarning(strMsg);
			}

			return b;
		}

		internal static bool TryWithKey(AppPolicyId p, bool bNoKeyRepeat,
			PwDatabase pd, string strContext)
		{
			if(!Try(p)) return false;

			if(bNoKeyRepeat) return true;
			return KeyUtil.ReAskKey(pd, true, strContext);
		}

		internal static void ApplyToConfig()
		{
			try
			{
				AppConfigEx cfg = Program.Config;

				if(!AppPolicy.Current.UnhidePasswords)
				{
					List<AceColumn> l = cfg.MainWindow.EntryListColumns;
					foreach(AceColumn c in l)
					{
						if(c == null) { Debug.Assert(false); continue; }

						if(c.Type == AceColumnType.Password)
							c.HideWithAsterisks = true;
					}
				}
			}
			catch(Exception) { Debug.Assert(false); }
		}
	}
}
