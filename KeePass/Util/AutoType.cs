/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2011 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Media;
using System.Configuration;

using KeePass.App;
using KeePass.Forms;
using KeePass.Native;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util.Spr;

using KeePassLib;
using KeePassLib.Security;
using KeePassLib.Collections;
using KeePassLib.Delegates;
using KeePassLib.Utility;

namespace KeePass.Util
{
	public sealed class AutoTypeEventArgs : EventArgs
	{
		private string m_strSeq;
		public string Sequence
		{
			get { return m_strSeq; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strSeq = value;
			}
		}

		public bool SendObfuscated { get; set; }
		public PwEntry Entry { get; private set; }

		public AutoTypeEventArgs(string strSequence, bool bObfuscated, PwEntry pe)
		{
			if(strSequence == null) throw new ArgumentNullException("strSequence");
			// pe may be null

			m_strSeq = strSequence;
			this.SendObfuscated = bObfuscated;
			this.Entry = pe;
		}
	}

	public static class AutoType
	{
		public static event EventHandler<AutoTypeEventArgs> FilterCompilePre;
		public static event EventHandler<AutoTypeEventArgs> FilterSendPre;
		public static event EventHandler<AutoTypeEventArgs> FilterSend;

		internal static void InitStatic()
		{
			try
			{
				// Enable new SendInput method; see
				// http://msdn.microsoft.com/en-us/library/system.windows.forms.sendkeys.aspx
				ConfigurationManager.AppSettings.Set("SendKeys", "SendInput");
			}
			catch(Exception) { Debug.Assert(false); }
		}

		private static bool MatchWindows(string strFilter, string strWindow)
		{
			Debug.Assert(strFilter != null); if(strFilter == null) return false;
			Debug.Assert(strWindow != null); if(strWindow == null) return false;

			string strF = strFilter.Trim();

			/* bool bArbStart = strF.StartsWith("*"), bArbEnd = strF.EndsWith("*");

			if(bArbStart) strF = strF.Remove(0, 1);
			if(bArbEnd) strF = strF.Substring(0, strF.Length - 1);

			if(bArbStart && bArbEnd)
				return (strWindow.IndexOf(strF, StrUtil.CaseIgnoreCmp) >= 0);
			else if(bArbStart)
				return strWindow.EndsWith(strF, StrUtil.CaseIgnoreCmp);
			else if(bArbEnd)
				return strWindow.StartsWith(strF, StrUtil.CaseIgnoreCmp);

			return strWindow.Equals(strF, StrUtil.CaseIgnoreCmp); */

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

			return StrUtil.SimplePatternMatch(strF, strWindow, StrUtil.CaseIgnoreCmp);
		}

		private static bool Execute(string strSeq, PwEntry pweData)
		{
			Debug.Assert(strSeq != null); if(strSeq == null) return false;
			Debug.Assert(pweData != null); if(pweData == null) return false;

			if(!pweData.GetAutoTypeEnabled()) return false;
			if(!AppPolicy.Try(AppPolicyId.AutoType)) return false;

			if(KeePassLib.Native.NativeLib.IsUnix())
			{
				if(!NativeMethods.TryXDoTool())
				{
					MessageService.ShowWarning(KPRes.AutoTypeXDoToolRequired,
						KPRes.PackageInstallHint);
					return false;
				}
			}

			PwDatabase pwDatabase = null;
			try { pwDatabase = Program.MainForm.PluginHost.Database; }
			catch(Exception) { }

			bool bObfuscate = (pweData.AutoType.ObfuscationOptions !=
				AutoTypeObfuscationOptions.None);
			AutoTypeEventArgs args = new AutoTypeEventArgs(strSeq, bObfuscate, pweData);

			if(AutoType.FilterCompilePre != null) AutoType.FilterCompilePre(null, args);

			args.Sequence = SprEngine.Compile(args.Sequence, true, pweData,
				pwDatabase, true, false);

			string strError = ValidateAutoTypeSequence(args.Sequence);
			if(strError != null)
			{
				MessageService.ShowWarning(strError);
				return false;
			}

			Application.DoEvents();

			if(AutoType.FilterSendPre != null) AutoType.FilterSendPre(null, args);
			if(AutoType.FilterSend != null) AutoType.FilterSend(null, args);

			if(args.Sequence.Length > 0)
			{
				try { SendInputEx.SendKeysWait(args.Sequence, args.SendObfuscated); }
				catch(Exception excpAT)
				{
					MessageService.ShowWarning(excpAT);
				}
			}

			pweData.Touch(false);
			if(EntryUtil.ExpireTanEntryIfOption(pweData))
				Program.MainForm.RefreshEntriesList();

			// SprEngine.Compile might have modified the database;
			// pd.Modified is set by SprEngine
			Program.MainForm.UpdateUI(false, null, false, null, false, null, false);

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

		private static string GetSequenceForWindow(PwEntry pwe, string strWindow,
			bool bRequireDefinedWindow)
		{
			Debug.Assert(strWindow != null); if(strWindow == null) return null;
			Debug.Assert(pwe != null); if(pwe == null) return null;

			if(!pwe.GetAutoTypeEnabled()) return null;

			string strSeq = null;
			foreach(KeyValuePair<string, string> kvp in pwe.AutoType.WindowSequencePairs)
			{
				string strWndSpec = kvp.Key;
				if(strWndSpec == null) { Debug.Assert(false); continue; }

				strWndSpec = strWndSpec.Trim();

				if(strWndSpec.Length > 0)
					strWndSpec = SprEngine.Compile(strWndSpec, false, pwe,
						null, false, false);

				if(MatchWindows(strWndSpec, strWindow))
				{
					strSeq = kvp.Value;
					break;
				}
			}

			if(Program.Config.Integration.AutoTypeMatchByTitle)
			{
				string strTitle = pwe.Strings.ReadSafe(PwDefs.TitleField);
				strTitle = strTitle.Trim();

				if(string.IsNullOrEmpty(strSeq) && (strTitle.Length > 0) &&
					(strWindow.IndexOf(strTitle, StrUtil.CaseIgnoreCmp) >= 0))
				{
					strSeq = pwe.AutoType.DefaultSequence;
					Debug.Assert(strSeq != null);
				}
			}

			if((strSeq == null) && bRequireDefinedWindow) return null;

			if(!string.IsNullOrEmpty(strSeq)) return strSeq;
			return pwe.GetAutoTypeSequence();
		}

		public static bool IsValidAutoTypeWindow(IntPtr hWindow, bool bBeepIfNot)
		{
			bool bValid = ((hWindow != Program.MainForm.Handle) &&
				!GlobalWindowManager.HasWindow(hWindow));

			if(!bValid && bBeepIfNot) SystemSounds.Beep.Play();

			return bValid;
		}

		public static bool PerformGlobal(List<PwDatabase> vSources,
			ImageList ilIcons)
		{
			Debug.Assert(vSources != null); if(vSources == null) return false;

			if(KeePassLib.Native.NativeLib.IsUnix())
			{
				if(!NativeMethods.TryXDoTool(true))
				{
					MessageService.ShowWarning(KPRes.AutoTypeXDoToolRequiredGlobalVer);
					return false;
				}
			}

			IntPtr hWnd;
			string strWindow;
			try
			{
				// hWnd = NativeMethods.GetForegroundWindowHandle();
				// strWindow = NativeMethods.GetWindowText(hWnd);
				NativeMethods.GetForegroundWindowInfo(out hWnd, out strWindow, true);
			}
			catch(Exception) { Debug.Assert(false); hWnd = IntPtr.Zero; strWindow = null; }

			if(string.IsNullOrEmpty(strWindow)) return false;
			if(!IsValidAutoTypeWindow(hWnd, true)) return false;

			PwObjectList<PwEntry> vList = new PwObjectList<PwEntry>();
			DateTime dtNow = DateTime.Now;

			EntryHandler eh = delegate(PwEntry pe)
			{
				// Ignore expired entries
				if(pe.Expires && (pe.ExpiryTime < dtNow)) return true;

				if(GetSequenceForWindow(pe, strWindow, true) != null)
					vList.Add(pe);

				return true;
			};

			foreach(PwDatabase pwSource in vSources)
			{
				if(pwSource.IsOpen == false) continue;
				pwSource.RootGroup.TraverseTree(TraversalMethod.PreOrder, null, eh);
			}

			if(vList.UCount == 1)
				AutoType.PerformInternal(vList.GetAt(0), strWindow);
			else if(vList.UCount > 1)
			{
				EntryListForm elf = new EntryListForm();
				elf.InitEx(KPRes.AutoTypeEntrySelection, KPRes.AutoTypeEntrySelectionDescShort,
					KPRes.AutoTypeEntrySelectionDescLong,
					Properties.Resources.B48x48_KGPG_Key2, ilIcons, vList);
				elf.EnsureForeground = true;

				if(elf.ShowDialog() == DialogResult.OK)
				{
					try { NativeMethods.EnsureForegroundWindow(hWnd); }
					catch(Exception) { Debug.Assert(false); }

					if(elf.SelectedEntry != null)
						AutoType.PerformInternal(elf.SelectedEntry, strWindow);
				}
				UIUtil.DestroyForm(elf);
			}

			return true;
		}

		public static bool PerformIntoPreviousWindow(Form fCurrent, PwEntry pe)
		{
			if((pe != null) && !pe.GetAutoTypeEnabled()) return false;
			if(!AppPolicy.Try(AppPolicyId.AutoTypeWithoutContext)) return false;

			bool bTopMost = ((fCurrent != null) ? fCurrent.TopMost : false);
			if(bTopMost) fCurrent.TopMost = false;

			try
			{
				if(!NativeMethods.LoseFocus(fCurrent)) { Debug.Assert(false); }

				return PerformIntoCurrentWindow(pe);
			}
			finally
			{
				if(bTopMost) fCurrent.TopMost = true;
			}
		}

		public static bool PerformIntoCurrentWindow(PwEntry pe)
		{
			if(!AppPolicy.Try(AppPolicyId.AutoTypeWithoutContext)) return false;

			string strWindow;
			try
			{
				IntPtr hDummy;
				NativeMethods.GetForegroundWindowInfo(out hDummy, out strWindow, true);
			}
			catch(Exception) { strWindow = null; }

			if(!KeePassLib.Native.NativeLib.IsUnix())
			{
				if(strWindow == null) { Debug.Assert(false); return false; }
			}
			else strWindow = string.Empty;

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
					if(strValue.StartsWith(@"{s:", StrUtil.CaseIgnoreCmp))
						return KPRes.AutoTypeUnknownPlaceholder +
							MessageService.NewLine + strValue;
				}
			}
			catch(Exception ex) { Debug.Assert(false); return ex.Message; }

			return null;
		}
	}
}
