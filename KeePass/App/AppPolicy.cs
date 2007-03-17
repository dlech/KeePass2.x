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
using System.Diagnostics;
using System.Windows.Forms;

using KeePass.Resources;

using KeePassLib;

namespace KeePass.App
{
	public enum AppPolicyFlag
	{
		Plugins = 0,
		Export,
		Import,
		Print,
		SaveDatabase,
		AutoType,
		CopyToClipboard,
		DragDrop,
		Count
	}

	/// <summary>
	/// Application policy settings
	/// </summary>
	public static class AppPolicy
	{
		private static bool[] m_vCurPolicyFlags = new bool[(int)AppPolicyFlag.Count];
		private static bool[] m_vNewPolicyFlags = new bool[(int)AppPolicyFlag.Count];

		private static string PolicyToString(AppPolicyFlag flag)
		{
			string str = KPRes.Flag + @": ";

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
				default:
					Debug.Assert(false);
					str += KPRes.Unknown + ".";
					break;
			}

			str += "\r\n" + KPRes.Description + @": ";

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
				default:
					Debug.Assert(false);
					str += KPRes.Unknown + ".";
					break;
			}

			return str;
		}

		public static string RequiredPolicyMessage(AppPolicyFlag flag)
		{
			string str = KPRes.PolicyDisallowed + "\r\n\r\n";
			str += KPRes.PolicyRequiredFlag + ":\r\n";
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

			if(!bAllowed)
			{
				string str = RequiredPolicyMessage(flag);
				MessageBox.Show(str, PwDefs.ShortProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}

			return bAllowed;
		}
	}
}
