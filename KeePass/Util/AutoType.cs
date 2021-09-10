/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2021 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Media;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

using KeePass.App;
using KeePass.Forms;
using KeePass.Native;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util.Spr;

using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Delegates;
using KeePassLib.Resources;
using KeePassLib.Security;
using KeePassLib.Utility;

using NativeLib = KeePassLib.Native.NativeLib;

namespace KeePass.Util
{
	public sealed class AutoTypeEventArgs : EventArgs
	{
		private string m_strSeq; // Never null
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
		public PwDatabase Database { get; private set; }

		public AutoTypeEventArgs(string strSequence, bool bObfuscated, PwEntry pe,
			PwDatabase pd)
		{
			if(strSequence == null) throw new ArgumentNullException("strSequence");
			// pe may be null

			m_strSeq = strSequence;
			this.SendObfuscated = bObfuscated;
			this.Entry = pe;
			this.Database = pd;
		}
	}

	public static class AutoType
	{
		private const int TargetActivationDelay = 100;

		public static event EventHandler<AutoTypeEventArgs> FilterCompilePre;
		public static event EventHandler<AutoTypeEventArgs> FilterSendPre;
		public static event EventHandler<AutoTypeEventArgs> FilterSend;

		public static event EventHandler<AutoTypeEventArgs> SendPost;

		public static event EventHandler<SequenceQueryEventArgs> SequenceQueryPre;
		public static event EventHandler<SequenceQueryEventArgs> SequenceQuery;
		public static event EventHandler<SequenceQueryEventArgs> SequenceQueryPost;

		public static event EventHandler<SequenceQueriesEventArgs> SequenceQueriesBegin;
		public static event EventHandler<SequenceQueriesEventArgs> SequenceQueriesEnd;

		private static int m_iEventID = 0;
		public static int GetNextEventID()
		{
			return Interlocked.Increment(ref m_iEventID);
		}

		internal static void InitStatic()
		{
			try
			{
				// SendKeys is not used anymore, thus the following is
				// not required:
				// // Enable new SendInput method; see
				// // https://msdn.microsoft.com/en-us/library/system.windows.forms.sendkeys.aspx
				// ConfigurationManager.AppSettings.Set("SendKeys", "SendInput");
			}
			catch(Exception) { Debug.Assert(false); }
		}

		internal static bool IsMatchWindow(string strWindow, string strFilter)
		{
			if(strWindow == null) { Debug.Assert(false); return false; }
			if(strFilter == null) { Debug.Assert(false); return false; }

			Debug.Assert(NormalizeWindowText(strWindow) == strWindow); // Should be done by caller
			string strF = NormalizeWindowText(strFilter);

			int ccF = strF.Length;
			if((ccF > 4) && (strF[0] == '/') && (strF[1] == '/') &&
				(strF[ccF - 2] == '/') && (strF[ccF - 1] == '/'))
			{
				try
				{
					string strRx = strF.Substring(2, ccF - 4);
					return Regex.IsMatch(strWindow, strRx, RegexOptions.IgnoreCase);
				}
				catch(Exception) { return false; }
			}

			return StrUtil.SimplePatternMatch(strF, strWindow, StrUtil.CaseIgnoreCmp);
		}

		private static bool IsMatchSub(string strWindow, string strSub)
		{
			if(strWindow == null) { Debug.Assert(false); return false; }
			if(strSub == null) { Debug.Assert(false); return false; }

			Debug.Assert(NormalizeWindowText(strWindow) == strWindow); // Should be done by caller
			string strS = NormalizeWindowText(strSub);

			// If strS is empty, return false, because strS is a substring
			// that must occur, not a filter
			if(strS.Length == 0) return false;

			return (strWindow.IndexOf(strS, StrUtil.CaseIgnoreCmp) >= 0);
		}

		private static bool Execute(AutoTypeCtx ctx)
		{
			if(ctx == null) { Debug.Assert(false); return false; }

			string strSeq = ctx.Sequence;
			PwEntry pweData = ctx.Entry;
			if(pweData == null) { Debug.Assert(false); return false; }

			if(!pweData.GetAutoTypeEnabled()) return false;
			if(!AppPolicy.Try(AppPolicyId.AutoType)) return false;

			if(NativeLib.IsUnix())
			{
				if(!NativeMethods.TryXDoTool() && !NativeLib.IsWayland())
				{
					MessageService.ShowWarning(KPRes.AutoTypeXDoToolRequired,
						KPRes.PackageInstallHint);
					return false;
				}
			}

			PwDatabase pwDatabase = ctx.Database;

			bool bObfuscate = (pweData.AutoType.ObfuscationOptions !=
				AutoTypeObfuscationOptions.None);
			AutoTypeEventArgs args = new AutoTypeEventArgs(strSeq, bObfuscate,
				pweData, pwDatabase);

			if(AutoType.FilterCompilePre != null) AutoType.FilterCompilePre(null, args);

			args.Sequence = SprEngine.Compile(args.Sequence, new SprContext(
				pweData, pwDatabase, SprCompileFlags.All, true, false));

			// string strError = ValidateAutoTypeSequence(args.Sequence);
			// if(!string.IsNullOrEmpty(strError))
			// {
			//	MessageService.ShowWarning(args.Sequence +
			//		MessageService.NewParagraph + strError);
			//	return false;
			// }

			Application.DoEvents();

			if(AutoType.FilterSendPre != null) AutoType.FilterSendPre(null, args);
			if(AutoType.FilterSend != null) AutoType.FilterSend(null, args);

			if(args.Sequence.Length > 0)
			{
				string strError = null;
				try { SendInputEx.SendKeysWait(args.Sequence, args.SendObfuscated); }
				catch(SecurityException exSec) { strError = exSec.Message; }
				catch(Exception ex)
				{
					strError = args.Sequence + MessageService.NewParagraph +
						ex.Message;
				}

				if(AutoType.SendPost != null) AutoType.SendPost(null, args);

				if(!string.IsNullOrEmpty(strError))
				{
					try
					{
						MainForm mfP = Program.MainForm;
						if(mfP != null)
							mfP.EnsureVisibleForegroundWindow(false, false);
					}
					catch(Exception) { Debug.Assert(false); }

					MessageService.ShowWarning(strError);
				}
			}

			pweData.Touch(false);
			EntryUtil.ExpireTanEntryIfOption(pweData, pwDatabase);

			MainForm mf = Program.MainForm;
			if(mf != null)
			{
				// Always refresh entry list (e.g. {NEWPASSWORD} might
				// have changed data)
				mf.RefreshEntriesList();

				// SprEngine.Compile might have modified the database;
				// pd.Modified is set by SprEngine
				mf.UpdateUI(false, null, false, null, false, null, false);

				if(Program.Config.MainWindow.MinimizeAfterAutoType &&
					mf.IsCommandTypeInvokable(null, MainForm.AppCommandType.Window))
					UIUtil.SetWindowState(mf, FormWindowState.Minimized);
			}

			return true;
		}

		private static bool PerformInternal(AutoTypeCtx ctx, string strWindow)
		{
			if(ctx == null) { Debug.Assert(false); return false; }

			AutoTypeCtx ctxNew = ctx.Clone();

			if(Program.Config.Integration.AutoTypePrependInitSequenceForIE &&
				WinUtil.IsInternetExplorer7Window(strWindow))
			{
				ctxNew.Sequence = @"{DELAY 50}1{DELAY 50}{BACKSPACE}" +
					ctxNew.Sequence;
			}

			return AutoType.Execute(ctxNew);
		}

		private static SequenceQueriesEventArgs GetSequencesForWindowBegin(
			IntPtr hWnd, string strWindow)
		{
			SequenceQueriesEventArgs e = new SequenceQueriesEventArgs(
				GetNextEventID(), hWnd, strWindow);

			if(AutoType.SequenceQueriesBegin != null)
				AutoType.SequenceQueriesBegin(null, e);

			return e;
		}

		private static void GetSequencesForWindowEnd(SequenceQueriesEventArgs e)
		{
			if(AutoType.SequenceQueriesEnd != null)
				AutoType.SequenceQueriesEnd(null, e);
		}

		// Multiple calls of this method are wrapped in
		// GetSequencesForWindowBegin and GetSequencesForWindowEnd
		private static List<string> GetSequencesForWindow(PwEntry pwe,
			IntPtr hWnd, string strWindow, PwDatabase pdContext, int iEventID)
		{
			List<string> l = new List<string>();

			if(pwe == null) { Debug.Assert(false); return l; }
			if(strWindow == null) { Debug.Assert(false); return l; } // May be empty

			if(!pwe.GetAutoTypeEnabled()) return l;

			SprContext sprCtx = new SprContext(pwe, pdContext,
				SprCompileFlags.NonActive);

			RaiseSequenceQueryEvent(AutoType.SequenceQueryPre, iEventID,
				hWnd, strWindow, pwe, pdContext, l);

			// Specifically defined sequences must match before the title,
			// in order to allow selecting the first item as default one
			foreach(AutoTypeAssociation a in pwe.AutoType.Associations)
			{
				string strFilter = SprEngine.Compile(a.WindowName, sprCtx);
				if(IsMatchWindow(strWindow, strFilter))
				{
					string strSeq = a.Sequence;
					if(string.IsNullOrEmpty(strSeq))
						strSeq = pwe.GetAutoTypeSequence();
					AddSequence(l, strSeq);
				}
			}

			RaiseSequenceQueryEvent(AutoType.SequenceQuery, iEventID,
				hWnd, strWindow, pwe, pdContext, l);

			if(Program.Config.Integration.AutoTypeMatchByTitle)
			{
				string strTitle = SprEngine.Compile(pwe.Strings.ReadSafe(
					PwDefs.TitleField), sprCtx);
				if(IsMatchSub(strWindow, strTitle))
					AddSequence(l, pwe.GetAutoTypeSequence());
			}

			string strCmpUrl = null; // To cache the compiled URL
			if(Program.Config.Integration.AutoTypeMatchByUrlInTitle)
			{
				strCmpUrl = SprEngine.Compile(pwe.Strings.ReadSafe(
					PwDefs.UrlField), sprCtx);
				if(IsMatchSub(strWindow, strCmpUrl))
					AddSequence(l, pwe.GetAutoTypeSequence());
			}

			if(Program.Config.Integration.AutoTypeMatchByUrlHostInTitle)
			{
				if(strCmpUrl == null)
					strCmpUrl = SprEngine.Compile(pwe.Strings.ReadSafe(
						PwDefs.UrlField), sprCtx);

				string strCleanUrl = StrUtil.RemovePlaceholders(strCmpUrl).Trim();
				string strHost = UrlUtil.GetHost(strCleanUrl);

				if(strHost.StartsWith("www.", StrUtil.CaseIgnoreCmp) &&
					(strCleanUrl.StartsWith("http:", StrUtil.CaseIgnoreCmp) ||
					strCleanUrl.StartsWith("https:", StrUtil.CaseIgnoreCmp)))
					strHost = strHost.Substring(4);

				if(IsMatchSub(strWindow, strHost))
					AddSequence(l, pwe.GetAutoTypeSequence());
			}

			if(Program.Config.Integration.AutoTypeMatchByTagInTitle)
			{
				foreach(string strTag in pwe.Tags)
				{
					if(IsMatchSub(strWindow, strTag))
					{
						AddSequence(l, pwe.GetAutoTypeSequence());
						break;
					}
				}
			}

			RaiseSequenceQueryEvent(AutoType.SequenceQueryPost, iEventID,
				hWnd, strWindow, pwe, pdContext, l);

			return l;
		}

		private static void RaiseSequenceQueryEvent(
			EventHandler<SequenceQueryEventArgs> f, int iEventID, IntPtr hWnd,
			string strWindow, PwEntry pwe, PwDatabase pdContext,
			List<string> lSeq)
		{
			if(f == null) return;

			SequenceQueryEventArgs e = new SequenceQueryEventArgs(iEventID,
				hWnd, strWindow, pwe, pdContext);
			f(null, e);

			foreach(string strSeq in e.Sequences)
				AddSequence(lSeq, strSeq);
		}

		private static void AddSequence(List<string> lSeq, string strSeq)
		{
			if(strSeq == null) { Debug.Assert(false); return; }

			string strCanSeq = CanonicalizeSeq(strSeq);

			for(int i = 0; i < lSeq.Count; ++i)
			{
				string strCanEx = CanonicalizeSeq(lSeq[i]);
				if(strCanEx.Equals(strCanSeq)) return; // Exists already
			}

			lSeq.Add(strSeq); // Non-canonical version
		}

		private const string StrBraceOpen = @"{1E1F63AB-2F63-4B60-ADBA-7F38B8D7778E}";
		private const string StrBraceClose = @"{34D698D7-CEBF-4AF0-87BF-DC1B1F5E95A0}";
		private static string CanonicalizeSeq(string strSeq)
		{
			// Preprocessing: balance braces
			strSeq = strSeq.Replace(@"{{}", StrBraceOpen);
			strSeq = strSeq.Replace(@"{}}", StrBraceClose);

			StringBuilder sb = new StringBuilder();

			bool bInPlh = false;
			for(int i = 0; i < strSeq.Length; ++i)
			{
				char ch = strSeq[i];

				if(ch == '{') bInPlh = true;
				else if(ch == '}') bInPlh = false;
				else if(bInPlh) ch = char.ToUpper(ch);

				sb.Append(ch);
			}

			strSeq = sb.ToString();

			// Postprocessing: restore braces
			strSeq = strSeq.Replace(StrBraceOpen, @"{{}");
			strSeq = strSeq.Replace(StrBraceClose, @"{}}");

			return strSeq;
		}

		public static bool IsValidAutoTypeWindow(IntPtr hWnd, bool bBeepIfNot)
		{
			bool bValid = !GlobalWindowManager.HasWindowMW(hWnd);

			if(!bValid && bBeepIfNot) SystemSounds.Beep.Play();

			return bValid;
		}

		public static bool PerformGlobal(List<PwDatabase> lSources,
			ImageList ilIcons)
		{
			return PerformGlobal(lSources, ilIcons, null);
		}

		internal static bool PerformGlobal(List<PwDatabase> lSources,
			ImageList ilIcons, string strSeq)
		{
			if(lSources == null) { Debug.Assert(false); return false; }

			if(NativeLib.IsUnix())
			{
				if(!NativeMethods.TryXDoTool(true) && !NativeLib.IsWayland())
				{
					MessageService.ShowWarning(KPRes.AutoTypeXDoToolRequiredGlobalVer);
					return false;
				}
			}

			IntPtr hWnd;
			string strWindow;
			GetForegroundWindowInfo(out hWnd, out strWindow);

			// if(string.IsNullOrEmpty(strWindow)) return false;
			if(strWindow == null) { Debug.Assert(false); return false; }
			if(!IsValidAutoTypeWindow(hWnd, true)) return false;

			SequenceQueriesEventArgs evQueries = GetSequencesForWindowBegin(
				hWnd, strWindow);

			List<AutoTypeCtx> lCtxs = new List<AutoTypeCtx>();
			PwDatabase pdCurrent = null;
			bool bExpCanMatch = Program.Config.Integration.AutoTypeExpiredCanMatch;
			DateTime dtNow = DateTime.UtcNow;

			EntryHandler eh = delegate(PwEntry pe)
			{
				if(!bExpCanMatch && pe.Expires && (pe.ExpiryTime <= dtNow))
					return true; // Ignore expired entries

				List<string> lSeq = GetSequencesForWindow(pe, hWnd, strWindow,
					pdCurrent, evQueries.EventID);

				if(!string.IsNullOrEmpty(strSeq) && (lSeq.Count != 0))
					lCtxs.Add(new AutoTypeCtx(strSeq, pe, pdCurrent));
				else
				{
					foreach(string strSeqCand in lSeq)
					{
						lCtxs.Add(new AutoTypeCtx(strSeqCand, pe, pdCurrent));
					}
				}

				return true;
			};

			foreach(PwDatabase pd in lSources)
			{
				if((pd == null) || !pd.IsOpen) continue;
				pdCurrent = pd;
				pd.RootGroup.TraverseTree(TraversalMethod.PreOrder, null, eh);
			}

			GetSequencesForWindowEnd(evQueries);

			bool bForceDlg = Program.Config.Integration.AutoTypeAlwaysShowSelDialog;

			if((lCtxs.Count >= 2) || bForceDlg)
			{
				AutoTypeCtxForm dlg = new AutoTypeCtxForm();
				dlg.InitEx(lCtxs, ilIcons);

				bool bOK = (dlg.ShowDialog() == DialogResult.OK);
				AutoTypeCtx ctx = (bOK ? dlg.SelectedCtx : null);
				UIUtil.DestroyForm(dlg);

				if(ctx != null)
				{
					try { NativeMethods.EnsureForegroundWindow(hWnd); }
					catch(Exception) { Debug.Assert(false); }

					int nActDelayMS = TargetActivationDelay;
					string strWindowT = strWindow.Trim();

					// https://sourceforge.net/p/keepass/discussion/329220/thread/3681f343/
					// This apparently is only required here (after showing the
					// auto-type entry selection dialog), not when using the
					// context menu command in the main window
					if(strWindowT.EndsWith("Microsoft Edge", StrUtil.CaseIgnoreCmp))
					{
						// 700 skips the first 1-2 characters,
						// 750 sometimes skips the first character
						nActDelayMS = 1000;
					}

					// Allow target window to handle its activation
					// (required by some applications, e.g. Edge)
					Application.DoEvents();
					Thread.Sleep(nActDelayMS);
					Application.DoEvents();

					AutoType.PerformInternal(ctx, strWindow);
				}
			}
			else if(lCtxs.Count == 1)
				AutoType.PerformInternal(lCtxs[0], strWindow);

			return true;
		}

		[Obsolete]
		public static bool PerformIntoPreviousWindow(Form fCurrent, PwEntry pe)
		{
			return PerformIntoPreviousWindow(fCurrent, pe,
				Program.MainForm.DocumentManager.SafeFindContainerOf(pe), null);
		}

		public static bool PerformIntoPreviousWindow(Form fCurrent, PwEntry pe,
			PwDatabase pdContext)
		{
			return PerformIntoPreviousWindow(fCurrent, pe, pdContext, null);
		}

		public static bool PerformIntoPreviousWindow(Form fCurrent, PwEntry pe,
			PwDatabase pdContext, string strSeq)
		{
			if(pe == null) { Debug.Assert(false); return false; }
			if(!pe.GetAutoTypeEnabled()) return false;
			if(!AppPolicy.Try(AppPolicyId.AutoTypeWithoutContext)) return false;

			bool bTopMost = ((fCurrent != null) ? fCurrent.TopMost : false);
			if(bTopMost) fCurrent.TopMost = false;

			try
			{
				if(!NativeMethods.LoseFocus(fCurrent, true)) { Debug.Assert(false); }

				return PerformIntoCurrentWindow(pe, pdContext, strSeq);
			}
			finally
			{
				if(bTopMost) fCurrent.TopMost = true;
			}
		}

		[Obsolete]
		public static bool PerformIntoCurrentWindow(PwEntry pe)
		{
			return PerformIntoCurrentWindow(pe,
				Program.MainForm.DocumentManager.SafeFindContainerOf(pe), null);
		}

		public static bool PerformIntoCurrentWindow(PwEntry pe, PwDatabase pdContext)
		{
			return PerformIntoCurrentWindow(pe, pdContext, null);
		}

		public static bool PerformIntoCurrentWindow(PwEntry pe, PwDatabase pdContext,
			string strSeq)
		{
			if(pe == null) { Debug.Assert(false); return false; }
			if(!pe.GetAutoTypeEnabled()) return false;
			if(!AppPolicy.Try(AppPolicyId.AutoTypeWithoutContext)) return false;

			Thread.Sleep(TargetActivationDelay);

			IntPtr hWnd;
			string strWindow;
			GetForegroundWindowInfo(out hWnd, out strWindow);

			if(!NativeLib.IsUnix())
			{
				if(strWindow == null) { Debug.Assert(false); return false; }
			}
			else strWindow = string.Empty;

			if(strSeq == null)
			{
				SequenceQueriesEventArgs evQueries = GetSequencesForWindowBegin(
					hWnd, strWindow);

				List<string> lSeq = GetSequencesForWindow(pe, hWnd, strWindow,
					pdContext, evQueries.EventID);

				GetSequencesForWindowEnd(evQueries);

				if(lSeq.Count == 0) strSeq = pe.GetAutoTypeSequence();
				else strSeq = lSeq[0];
			}

			AutoTypeCtx ctx = new AutoTypeCtx(strSeq, pe, pdContext);
			return AutoType.PerformInternal(ctx, strWindow);
		}

		// ValidateAutoTypeSequence is not required anymore, because
		// SendInputEx now validates the sequence
		/* private static string ValidateAutoTypeSequence(string strSequence)
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
						return (KPRes.AutoTypeUnknownPlaceholder +
							MessageService.NewLine + strValue);
				}
			}
			catch(Exception ex) { Debug.Assert(false); return ex.Message; }

			return null;
		} */

		private static readonly char[] g_vNormToHyphen = new char[] {
			// Sync with UI option name
			'\u2010', // Hyphen
			'\u2011', // Non-breaking hyphen
			'\u2012', // Figure dash
			'\u2013', // En dash
			'\u2014', // Em dash
			'\u2015', // Horizontal bar
			'\u2212' // Minus sign
		};
		internal static string NormalizeWindowText(string str)
		{
			if(string.IsNullOrEmpty(str)) return string.Empty;

			str = str.Trim();

			if(Program.Config.Integration.AutoTypeMatchNormDashes &&
				(str.IndexOfAny(g_vNormToHyphen) >= 0))
			{
				for(int i = 0; i < g_vNormToHyphen.Length; ++i)
					str = str.Replace(g_vNormToHyphen[i], '-');
			}

			return str;
		}

		private static void GetForegroundWindowInfo(out IntPtr hWnd, out string strWindow)
		{
			try
			{
				NativeMethods.GetForegroundWindowInfo(out hWnd, out strWindow, false);
			}
			catch(Exception)
			{
				Debug.Assert(false);
				hWnd = IntPtr.Zero;
				strWindow = null;
				return;
			}

			strWindow = NormalizeWindowText(strWindow);
		}

		// Cf. PwEntry.GetAutoTypeEnabled
		internal static string GetEnabledText(PwEntry pe, out object oBlocker)
		{
			oBlocker = null;
			if(pe == null) { Debug.Assert(false); return string.Empty; }

			if(!pe.AutoType.Enabled)
			{
				oBlocker = pe;
				return (KPRes.No + " (" + KLRes.EntryLower + ")");
			}

			PwGroup pg = pe.ParentGroup;
			while(pg != null)
			{
				if(pg.EnableAutoType.HasValue)
				{
					if(pg.EnableAutoType.Value) break;

					oBlocker = pg;
					return (KPRes.No + " (" + KLRes.GroupLower + " '" + pg.Name + "')");
				}

				pg = pg.ParentGroup;
			}

			// Options like 'Expired entries can match' influence the global
			// auto-type matching only, not commands like 'Perform Auto-Type'

			return KPRes.Yes;
		}

		internal static string GetSequencesText(PwEntry pe)
		{
			if(pe == null) { Debug.Assert(false); return string.Empty; }

			string strSeq = pe.GetAutoTypeSequence();
			Debug.Assert(strSeq.Length != 0);
			string str = ((strSeq.Length != 0) ? strSeq : ("(" + KPRes.Empty + ")"));

			int cAssoc = pe.AutoType.AssociationsCount;
			if(cAssoc != 0)
			{
				Dictionary<string, bool> d = new Dictionary<string, bool>();
				d[strSeq] = true;

				foreach(AutoTypeAssociation a in pe.AutoType.Associations)
				{
					string strAssocSeq = a.Sequence;
					if(strAssocSeq.Length != 0) d[strAssocSeq] = true;
				}

				int c = d.Count;
				str += ((c >= 2) ? (" " + KPRes.MoreAnd.Replace(@"{PARAM}",
					(c - 1).ToString())) : string.Empty) + " (" +
					cAssoc.ToString() + " " + KPRes.AssociationsLower + ")";
			}

			return str;
		}
	}
}
