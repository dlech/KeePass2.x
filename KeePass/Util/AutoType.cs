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
using System.Windows.Forms;
using System.Threading;

using KeePass.App;
using KeePass.Forms;
using KeePass.Resources;

using KeePassLib;
using KeePassLib.Security;
using KeePassLib.Collections;
using KeePassLib.Delegates;
using KeePassLib.Utility;

namespace KeePass.Util
{
	public static class AutoType
	{
		private const StringComparison StrCaseCmpMethod = StringComparison.OrdinalIgnoreCase;

		private static string StringToSequence(string str, bool bReplaceEscBrackets)
		{
			Debug.Assert(str != null); if(str == null) return string.Empty;

			if(bReplaceEscBrackets && ((str.IndexOf('{') >= 0) || (str.IndexOf('}') >= 0)))
			{
				char chOpen = '\u25A1';
				while(str.IndexOf(chOpen) >= 0) ++chOpen;

				char chClose = chOpen;
				++chClose;
				while(str.IndexOf(chClose) >= 0) ++chClose;

				str = str.Replace('{', chOpen);
				str = str.Replace('}', chClose);

				str = str.Replace(new string(chOpen, 1), @"{{}");
				str = str.Replace(new string(chClose, 1), @"{}}");
			}

			str = str.Replace(@"[", @"{[}");
			str = str.Replace(@"]", @"{]}");

			str = str.Replace(@"+", @"{+}");
			str = str.Replace(@"^", @"{^}");
			str = str.Replace(@"%", @"{%}");
			str = str.Replace(@"~", @"{~}");
			str = str.Replace(@"(", @"{(}");
			str = str.Replace(@")", @"{)}");

			return str;
		}

		public static bool MatchWindows(string strFilter, string strWindow)
		{
			Debug.Assert(strFilter != null); if(strFilter == null) return false;
			Debug.Assert(strWindow != null); if(strWindow == null) return false;

			string strF = strFilter.Trim();
			bool bArbStart = strF.StartsWith("*"), bArbEnd = strF.EndsWith("*");

			if(bArbStart) strF = strF.Remove(0, 1);
			if(bArbEnd) strF = strF.Substring(0, strF.Length - 1);

			if(bArbStart && bArbEnd)
				return (strWindow.IndexOf(strF, StrCaseCmpMethod) >= 0);
			else if(bArbStart)
				return strWindow.EndsWith(strF, StrCaseCmpMethod);
			else if(bArbEnd)
				return strWindow.StartsWith(strF, StrCaseCmpMethod);

			return strWindow.Equals(strF, StrCaseCmpMethod);
		}

		public static bool Execute(string strSeq, PwEntry pweData)
		{
			Debug.Assert(strSeq != null); if(strSeq == null) return false;
			Debug.Assert(pweData != null); if(pweData == null) return false;

			if(!pweData.AutoType.Enabled) return false;
			if(!AppPolicy.Try(AppPolicyFlag.AutoType)) return false;

			PwDatabase pwDatabase = null;
			try { pwDatabase = Program.MainForm.PluginHost.Database; }
			catch(Exception) { pwDatabase = null; }

			string strSend = strSeq;
			strSend = StrUtil.FillPlaceholders(strSend, pweData, WinUtil.GetExecutable(),
				pwDatabase, false);
			strSend = AppLocator.FillPlaceholders(strSend);
			strSend = AutoType.StringToSequence(strSend, false);

			Application.DoEvents();

			try { SendInputEx.SendKeysWait(strSend); }
			catch(Exception excpAT)
			{
				MessageService.ShowWarning(excpAT);
			}

			return true;
		}

		public static bool Perform(PwEntry pwe, string strWindow)
		{
			Debug.Assert(pwe != null); if(pwe == null) return false;

			string strSeq = GetSequenceForWindow(pwe, strWindow, false);
			if((strSeq == null) || (strSeq.Length == 0)) return false;

			AutoType.Execute(strSeq, pwe);
			return true;
		}

		public static string GetSequenceForWindow(PwEntry pwe, string strWindow, bool bRequireDefinedWindow)
		{
			Debug.Assert(strWindow != null); if(strWindow == null) return null;
			Debug.Assert(pwe != null); if(pwe == null) return null;

			if(pwe.AutoType.Enabled == false) return null;

			string strSeq = null;

			foreach(KeyValuePair<string, string> kvp in pwe.AutoType.WindowSequencePairs)
			{
				if(MatchWindows(kvp.Key, strWindow))
				{
					strSeq = kvp.Value;
					break;
				}
			}

			if((strSeq == null) && (strWindow.IndexOf(pwe.Strings.ReadSafe(PwDefs.TitleField),
				StrCaseCmpMethod) >= 0))
			{
				strSeq = pwe.AutoType.DefaultSequence;
				Debug.Assert(strSeq != null);
			}

			if((strSeq == null) && bRequireDefinedWindow) return null;

			if(strSeq == null) // Assume default sequence now
				strSeq = pwe.AutoType.DefaultSequence;

			PwGroup pg = pwe.ParentGroup;
			while(pg != null)
			{
				if(strSeq.Length != 0) break;

				strSeq = pg.DefaultAutoTypeSequence;
				pg = pg.ParentGroup;
			}

			if(strSeq.Length != 0) return strSeq;

			if(PwDefs.IsTanEntry(pwe)) return PwDefs.DefaultAutoTypeSequenceTan;
			return PwDefs.DefaultAutoTypeSequence;
		}

		public static bool PerformGlobal(PwDatabase pwDatabase, ImageList ilIcons)
		{
			Debug.Assert(pwDatabase != null); if(pwDatabase == null) return false;
			Debug.Assert(pwDatabase.IsOpen); if(!pwDatabase.IsOpen) return false;

			IntPtr hWnd = WinUtil.GetForegroundWindow();
			string strWindow = WinUtil.GetWindowText(hWnd);
			if((strWindow == null) || (strWindow.Length == 0)) return false;

			PwObjectList<PwEntry> m_vList = new PwObjectList<PwEntry>();

			EntryHandler eh = delegate(PwEntry pe)
			{
				if(GetSequenceForWindow(pe, strWindow, true) != null)
					m_vList.Add(pe);

				return true;
			};

			pwDatabase.RootGroup.TraverseTree(TraversalMethod.PreOrder, null, eh);

			if(m_vList.UCount == 1) AutoType.Perform(m_vList.GetAt(0), strWindow);
			else if(m_vList.UCount > 1)
			{
				EntryListForm elf = new EntryListForm();
				elf.InitEx(KPRes.AutoTypeEntrySelection, KPRes.AutoTypeEntrySelectionDescShort,
					KPRes.AutoTypeEntrySelectionDescLong,
					KeePass.Properties.Resources.B48x48_KGPG_Key2, ilIcons, m_vList);
				elf.EnsureForeground = true;

				if(elf.ShowDialog() == DialogResult.OK)
				{
					WinUtil.EnsureForegroundWindow(hWnd);

					if(elf.SelectedEntry != null)
						AutoType.Perform(elf.SelectedEntry, strWindow);
				}
			}

			return true;
		}

		public static bool PerformIntoPreviousWindow(IntPtr hWndCurrent, PwEntry pe)
		{
			WinUtil.LoseFocus(hWndCurrent);

			string strWindow = WinUtil.GetWindowText(WinUtil.GetForegroundWindow());
			Debug.Assert(strWindow != null); if(strWindow == null) return false;

			Thread.Sleep(100);

			return AutoType.Perform(pe, strWindow);
		}
	}
}
