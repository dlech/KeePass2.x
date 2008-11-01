/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2008 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;

using KeePass.App;
using KeePass.Forms;
using KeePass.Native;
using KeePass.Resources;
using KeePass.Util.Spr;

using KeePassLib;
using KeePassLib.Security;
using KeePassLib.Collections;
using KeePassLib.Delegates;
using KeePassLib.Utility;

namespace KeePass.Util
{
	public static class AutoType
	{
		private const StringComparison StrCaseIgnoreCmp = StringComparison.OrdinalIgnoreCase;

		private static bool MatchWindows(string strFilter, string strWindow)
		{
			Debug.Assert(strFilter != null); if(strFilter == null) return false;
			Debug.Assert(strWindow != null); if(strWindow == null) return false;

			string strF = strFilter.Trim();
			
			/* bool bArbStart = strF.StartsWith("*"), bArbEnd = strF.EndsWith("*");

			if(bArbStart) strF = strF.Remove(0, 1);
			if(bArbEnd) strF = strF.Substring(0, strF.Length - 1);

			if(bArbStart && bArbEnd)
				return (strWindow.IndexOf(strF, StrCaseIgnoreCmp) >= 0);
			else if(bArbStart)
				return strWindow.EndsWith(strF, StrCaseIgnoreCmp);
			else if(bArbEnd)
				return strWindow.StartsWith(strF, StrCaseIgnoreCmp);

			return strWindow.Equals(strF, StrCaseIgnoreCmp); */

			if(strF.StartsWith(@"//") && strF.EndsWith(@"//") && (strF.Length > 4))
			{
				try
				{
					Regex rx = new Regex(strF.Substring(2, strF.Length - 4),
						RegexOptions.IgnoreCase);

					return rx.IsMatch(strWindow);					
				}
				catch(Exception) { }
			}

			return StrUtil.SimplePatternMatch(strF, strWindow, StrCaseIgnoreCmp);
		}

		private static bool Execute(string strSeq, PwEntry pweData)
		{
			Debug.Assert(strSeq != null); if(strSeq == null) return false;
			Debug.Assert(pweData != null); if(pweData == null) return false;

			if(!pweData.AutoType.Enabled) return false;
			if(!AppPolicy.Try(AppPolicyId.AutoType)) return false;

			PwDatabase pwDatabase = null;
			try { pwDatabase = Program.MainForm.PluginHost.Database; }
			catch(Exception) { pwDatabase = null; }

			string strSend = SprEngine.Compile(strSeq, true, pweData,
				pwDatabase, true, false);

			string strError = ValidateAutoTypeSequence(strSend);
			if(strError != null)
			{
				MessageService.ShowWarning(strError);
				return false;
			}

			bool bObfuscate = (pweData.AutoType.ObfuscationOptions !=
				AutoTypeObfuscationOptions.None);

			Application.DoEvents();

			try { SendInputEx.SendKeysWait(strSend, bObfuscate); }
			catch(Exception excpAT)
			{
				MessageService.ShowWarning(excpAT);
			}

			return true;
		}

		private static bool PerformInternal(PwEntry pwe, string strWindow)
		{
			Debug.Assert(pwe != null); if(pwe == null) return false;

			string strSeq = GetSequenceForWindow(pwe, strWindow, false);
			if((strSeq == null) || (strSeq.Length == 0)) return false;

			if(Program.Config.Integration.AutoTypePrependInitSequenceForIE &&
				WinUtil.IsInternetExplorer7Window(strWindow))
			{
				strSeq = @"{DELAY 50}1{DELAY 50}{BACKSPACE}" + strSeq;
			}

			AutoType.Execute(strSeq, pwe);
			return true;
		}

		private static string GetSequenceForWindow(PwEntry pwe, string strWindow, bool bRequireDefinedWindow)
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

			string strTitle = pwe.Strings.ReadSafe(PwDefs.TitleField);
			if(((strSeq == null) || (strSeq.Length == 0)) &&
				(strTitle.Length > 0) &&
				(strWindow.IndexOf(strTitle, StrCaseIgnoreCmp) >= 0))
			{
				strSeq = pwe.AutoType.DefaultSequence;
				Debug.Assert(strSeq != null);
			}

			if((strSeq == null) && bRequireDefinedWindow) return null;

			// Assume default sequence now
			if((strSeq == null) || (strSeq.Length == 0))
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

		public static bool PerformGlobal(List<PwDatabase> vSources,
			ImageList ilIcons)
		{
			Debug.Assert(vSources != null); if(vSources == null) return false;

			IntPtr hWnd;
			string strWindow;

			try
			{
				hWnd = NativeMethods.GetForegroundWindow();
				strWindow = NativeMethods.GetWindowText(hWnd);
			}
			catch(Exception) { Debug.Assert(false); hWnd = IntPtr.Zero; strWindow = null; }

			if((strWindow == null) || (strWindow.Length == 0)) return false;

			PwObjectList<PwEntry> m_vList = new PwObjectList<PwEntry>();

			DateTime dtNow = DateTime.Now;

			EntryHandler eh = delegate(PwEntry pe)
			{
				// Ignore expired entries
				if(pe.Expires && (pe.ExpiryTime < dtNow)) return true;

				if(GetSequenceForWindow(pe, strWindow, true) != null)
					m_vList.Add(pe);

				return true;
			};

			foreach(PwDatabase pwSource in vSources)
			{
				if(pwSource.IsOpen == false) continue;
				pwSource.RootGroup.TraverseTree(TraversalMethod.PreOrder, null, eh);
			}

			if(m_vList.UCount == 1)
				AutoType.PerformInternal(m_vList.GetAt(0), strWindow);
			else if(m_vList.UCount > 1)
			{
				EntryListForm elf = new EntryListForm();
				elf.InitEx(KPRes.AutoTypeEntrySelection, KPRes.AutoTypeEntrySelectionDescShort,
					KPRes.AutoTypeEntrySelectionDescLong,
					Properties.Resources.B48x48_KGPG_Key2, ilIcons, m_vList);
				elf.EnsureForeground = true;

				if(elf.ShowDialog() == DialogResult.OK)
				{
					try { NativeMethods.EnsureForegroundWindow(hWnd); }
					catch(Exception) { Debug.Assert(false); }

					if(elf.SelectedEntry != null)
						AutoType.PerformInternal(elf.SelectedEntry, strWindow);
				}
			}

			return true;
		}

		public static bool PerformIntoPreviousWindow(IntPtr hWndCurrent, PwEntry pe)
		{
			try { NativeMethods.LoseFocus(hWndCurrent); }
			catch(Exception) { Debug.Assert(false); }

			return PerformIntoCurrentWindow(pe);
		}

		public static bool PerformIntoCurrentWindow(PwEntry pe)
		{
			string strWindow;

			try
			{
				strWindow = NativeMethods.GetWindowText(
					NativeMethods.GetForegroundWindow());
			}
			catch(Exception) { strWindow = null; }

			Debug.Assert(strWindow != null); if(strWindow == null) return false;

			Thread.Sleep(100);

			return AutoType.PerformInternal(pe, strWindow);
		}

		private static string ValidateAutoTypeSequence(string strSequence)
		{
			Debug.Assert(strSequence != null);

			string strSeq = strSequence;
			strSeq = strSeq.Replace(@"{{}", string.Empty);
			strSeq = strSeq.Replace(@"{}}", string.Empty);

			int cBrackets = 0;
			for(int c = 0; c < strSeq.Length; ++c)
			{
				if(strSeq[c] == '{') ++cBrackets;
				else if(strSeq[c] == '}') --cBrackets;

				if((cBrackets < 0) || (cBrackets > 1))
					return KPRes.AutoTypeSequenceInvalid;
			}
			if(cBrackets != 0) return KPRes.AutoTypeSequenceInvalid;

			if(strSeq.IndexOf(@"{}") >= 0) return KPRes.AutoTypeSequenceInvalid;

			try
			{
				Regex r = new Regex(@"\{[^\{\}]+\}", RegexOptions.CultureInvariant);
				MatchCollection matches = r.Matches(strSeq);

				foreach(Match m in matches)
				{
					string strValue = m.Value;
					string strLower = strValue.ToLower();

					if(strLower.StartsWith(@"{s:"))
						return KPRes.AutoTypeUnknownPlaceholder +
							MessageService.NewLine + strValue;
				}
			}
			catch(Exception ex) { Debug.Assert(false); return ex.Message; }

			return null;
		}
	}
}
