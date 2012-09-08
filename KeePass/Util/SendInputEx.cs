/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2012 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

using KeePass.Native;

using KeePassLib.Utility;

namespace KeePass.Util
{
	internal sealed class SiStateEx
	{
		public bool InputBlocked = false;

		public IntPtr OriginalKeyboardLayout = IntPtr.Zero;
		public IntPtr CurrentKeyboardLayout = IntPtr.Zero;

		public uint DefaultDelay = 10;

		public IntPtr TargetHWnd = IntPtr.Zero;
		public string TargetWindowTitle = string.Empty;

		public uint ThisThreadID = 0;
		public uint TargetThreadID = 0;
		public uint TargetProcessID = 0;

		// public bool ThreadInputAttached = false;

		public bool Cancelled = false;
	}

	public static partial class SendInputEx
	{
		// private const ushort LangIDGerman = 0x0407;

		public static void SendKeysWait(string strKeys, bool bObfuscate)
		{
			SiStateEx si = InitSendKeys();

			bool bUnix = KeePassLib.Native.NativeLib.IsUnix();
			try
			{
				if(!bUnix) { Debug.Assert(GetActiveKeyModifiers().Count == 0); }

				strKeys = ExtractGlobalDelay(strKeys, si); // Before TCATO splitting

				if(bObfuscate)
				{
					try { SendObfuscated(strKeys, si); }
					catch(Exception) { SendKeysWithSpecial(strKeys, si); }
				}
				else SendKeysWithSpecial(strKeys, si);
			}
			catch
			{
				FinishSendKeys(si);
				throw;
			}

			FinishSendKeys(si);
		}

		private static SiStateEx InitSendKeys()
		{
			SiStateEx si = new SiStateEx();
			if(KeePassLib.Native.NativeLib.IsUnix())
			{
				si.DefaultDelay /= 3; // Starting external program takes time
				return si;
			}

			try
			{
				IntPtr hWndTarget;
				string strTargetTitle;
				NativeMethods.GetForegroundWindowInfo(out hWndTarget,
					out strTargetTitle, false);
				si.TargetHWnd = hWndTarget;
				si.TargetWindowTitle = (strTargetTitle ?? string.Empty);

				si.ThisThreadID = NativeMethods.GetCurrentThreadId();
				uint uTargetProcessID;
				si.TargetThreadID = NativeMethods.GetWindowThreadProcessId(
					si.TargetHWnd, out uTargetProcessID);
				si.TargetProcessID = uTargetProcessID;

				EnsureSameKeyboardLayout(si);

				// Do not use SendKeys.Flush here, use Application.DoEvents
				// instead; SendKeys.Flush might run into an infinite loop here
				// if a previous auto-type process failed with throwing an
				// exception (SendKeys.Flush is waiting in a loop for an internal
				// queue being empty, however the queue is never processed)
				Application.DoEvents();

				// if(si.ThisThreadID != si.TargetThreadID)
				// {
				//	si.ThreadInputAttached = NativeMethods.AttachThreadInput(
				//		si.ThisThreadID, si.TargetThreadID, true);
				//	Debug.Assert(si.ThreadInputAttached);
				// }
				// else { Debug.Assert(false); }

				List<int> lMod = GetActiveKeyModifiers();
				ActivateKeyModifiers(lMod, false);
				SpecialReleaseModifiers(lMod);

				Debug.Assert(GetActiveKeyModifiers().Count == 0);

				si.InputBlocked = NativeMethods.BlockInput(true);
			}
			catch(Exception) { Debug.Assert(false); }

			return si;
		}

		private static void FinishSendKeys(SiStateEx si)
		{
			if(KeePassLib.Native.NativeLib.IsUnix()) return;

			try
			{
				// Do not restore original modifier keys here, otherwise
				// modifier keys are restored even when the user released
				// them while KeePass is auto-typing!
				// ActivateKeyModifiers(lRestore, true);

				if(si.InputBlocked) NativeMethods.BlockInput(false); // Unblock

				// if(si.ThreadInputAttached)
				//	NativeMethods.AttachThreadInput(si.ThisThreadID,
				//		si.TargetThreadID, false); // Detach

				if(si.OriginalKeyboardLayout != IntPtr.Zero)
					NativeMethods.ActivateKeyboardLayout(si.OriginalKeyboardLayout, 0);

				Application.DoEvents();
			}
			catch(Exception) { Debug.Assert(false); }
		}

		private static void SendObfuscated(string strKeys, SiStateEx siState)
		{
			if(string.IsNullOrEmpty(strKeys)) return;

			ClipboardEventChainBlocker cev = new ClipboardEventChainBlocker();
			ClipboardContents cnt = new ClipboardContents(true, true);
			Exception excpInner = null;

			char[] vSpecial = new char[]{ '{', '}', '(', ')', '+', '^', '%',
				' ', '\t', '\r', '\n' };

			try
			{
				List<string> vParts = SplitKeySequence(strKeys);
				foreach(string strPart in vParts)
				{
					if(string.IsNullOrEmpty(strPart)) continue;

					if(strPart.IndexOfAny(vSpecial) >= 0)
						SendKeysWithSpecial(strPart, siState);
					else MixedTransfer(strPart, siState);
				}
			}
			catch(Exception ex) { excpInner = ex; }

			cnt.SetData();
			cev.Release();

			if(excpInner != null) throw excpInner;
		}

		private static List<string> SplitKeySequence(string strKeys)
		{
			List<string> vParts = new List<string>();
			if(string.IsNullOrEmpty(strKeys)) return vParts;

			CharStream cs = new CharStream(strKeys);
			StringBuilder sbRawText = new StringBuilder();

			while(true)
			{
				char ch = cs.ReadChar();
				if(ch == char.MinValue) break;

				switch(ch)
				{
					case ')':
					case '}':
						throw new FormatException();

					case '(':
					case '{':
					case '+':
					case '^':
					case '%':
					case ' ':
					case '\t':
						string strBuf = sbRawText.ToString();
						if(strBuf.IndexOfAny(new char[]{ '+', '^', '%',
							' ', '\t' }) < 0)
						{
							if(strBuf.Length > 0) vParts.Add(strBuf);
							sbRawText.Remove(0, sbRawText.Length);
						}

						if(ch == '(')
						{
							ReadParenthesized(cs, sbRawText);
							if(sbRawText.Length > 0)
								vParts.Add(sbRawText.ToString());
							sbRawText.Remove(0, sbRawText.Length);
						}
						else if(ch == '{')
						{
							ReadBraced(cs, sbRawText);
							if(sbRawText.Length > 0)
								vParts.Add(sbRawText.ToString());
							sbRawText.Remove(0, sbRawText.Length);
						}
						else if(ch == ' ')
						{
							vParts.Add(" ");
							sbRawText.Remove(0, sbRawText.Length);
						}
						else if(ch == '\t')
						{
							vParts.Add("\t");
							sbRawText.Remove(0, sbRawText.Length);
						}
						else sbRawText.Append(ch);
						break;

					default:
						sbRawText.Append(ch);
						break;
				}
			}

			if(sbRawText.Length > 0) vParts.Add(sbRawText.ToString());
			return vParts;
		}

		private static void ReadParenthesized(CharStream csIn, StringBuilder sbBuffer)
		{
			sbBuffer.Append('(');

			while(true)
			{
				char ch = csIn.ReadChar();

				if((ch == char.MinValue) || (ch == '}'))
					throw new FormatException();
				else if(ch == ')')
				{
					sbBuffer.Append(ch);
					break;
				}
				else if(ch == '(')
					ReadParenthesized(csIn, sbBuffer);
				else if(ch == '{')
					ReadBraced(csIn, sbBuffer);
				else sbBuffer.Append(ch);
			}
		}

		private static void ReadBraced(CharStream csIn, StringBuilder sbBuffer)
		{
			sbBuffer.Append('{');

			char chFirst = csIn.ReadChar();
			if(chFirst == char.MinValue)
				throw new FormatException();

			char chSecond = csIn.ReadChar();
			if(chSecond == char.MinValue)
				throw new FormatException();

			if((chFirst == '{') && (chSecond == '}'))
			{
				sbBuffer.Append(@"{}");
				return;
			}
			else if((chFirst == '}') && (chSecond == '}'))
			{
				sbBuffer.Append(@"}}");
				return;
			}
			else if(chSecond == '}')
			{
				sbBuffer.Append(chFirst);
				sbBuffer.Append(chSecond);
				return;
			}

			sbBuffer.Append(chFirst);
			sbBuffer.Append(chSecond);

			while(true)
			{
				char ch = csIn.ReadChar();

				if((ch == char.MinValue) || (ch == ')'))
					throw new FormatException();
				else if(ch == '(')
					ReadParenthesized(csIn, sbBuffer);
				else if(ch == '{')
					ReadBraced(csIn, sbBuffer);
				else if(ch == '}')
				{
					sbBuffer.Append(ch);
					break;
				}
				else sbBuffer.Append(ch);
			}
		}

		private static void MixedTransfer(string strText, SiStateEx siState)
		{
			StringBuilder sbKeys = new StringBuilder();
			StringBuilder sbClip = new StringBuilder();
			
			// The string should be split randomly, but the same each
			// time this function is called. Otherwise an attacker could
			// get information by observing different splittings each
			// time auto-type is performed. Therefore, compute the random
			// seed based on the string to be auto-typed.
			Random r = new Random(GetRandomSeed(strText));

			foreach(char ch in strText)
			{
				if(r.Next(0, 2) == 0)
				{
					sbClip.Append(ch);
					sbKeys.Append(@"{RIGHT}");
				}
				else sbKeys.Append(ch);
			}

			string strClip = sbClip.ToString();
			string strKeys = sbKeys.ToString();

			if(strClip.Length > 0)
			{
				StringBuilder sbNav = new StringBuilder();
				sbNav.Append(@"^v");
				for(int iLeft = 0; iLeft < strClip.Length; ++iLeft)
					sbNav.Append(@"{LEFT}");

				strKeys = sbNav.ToString() + strKeys;
			}

			if(strClip.Length > 0)
				ClipboardUtil.Copy(strClip, false, false, null, null, IntPtr.Zero);
			else ClipboardUtil.Clear();

			if(strKeys.Length > 0) SendKeysWithSpecial(strKeys, siState);

			ClipboardUtil.Clear();
		}

		private static int GetRandomSeed(string strText)
		{
			int nSeed = 3;

			unchecked
			{
				foreach(char ch in strText)
					nSeed = nSeed * 13 + ch;
			}

			// Prevent overflow (see Random class constructor)
			if(nSeed == int.MinValue) nSeed = 13;
			return nSeed;
		}

		private static bool ValidateTargetWindow(SiStateEx siState)
		{
			if(siState.Cancelled) return false;

			if(KeePassLib.Native.NativeLib.IsUnix()) return true;

			bool bChkWnd = Program.Config.Integration.AutoTypeCancelOnWindowChange;
			bool bChkTitle = Program.Config.Integration.AutoTypeCancelOnTitleChange;
			if(!bChkWnd && !bChkTitle) return true;

			bool bValid = true;
			try
			{
				IntPtr h;
				string strTitle;
				NativeMethods.GetForegroundWindowInfo(out h, out strTitle, false);

				if(bChkWnd && (h != siState.TargetHWnd))
				{
					siState.Cancelled = true;
					bValid = false;
				}

				if(bChkTitle && ((strTitle ?? string.Empty) != siState.TargetWindowTitle))
				{
					siState.Cancelled = true;
					bValid = false;
				}
			}
			catch(Exception) { Debug.Assert(false); }

			return bValid;
		}

		/// <summary>
		/// This method searches for a <c>{DELAY=X}</c> placeholder,
		/// removes it from the sequence and sets the global delay in
		/// <paramref name="siState" /> to X.
		/// </summary>
		private static string ExtractGlobalDelay(string strSequence, SiStateEx siState)
		{
			if(string.IsNullOrEmpty(strSequence)) return string.Empty;

			const string strDefDelay = @"(\{[Dd][Ee][Ll][Aa][Yy]\s*=\s*)(\d+)(\})";
			Match mDefDelay = Regex.Match(strSequence, strDefDelay);
			if(mDefDelay.Success)
			{
				string strTime = mDefDelay.Groups[2].Value;
				strSequence = Regex.Replace(strSequence, strDefDelay, string.Empty);

				uint uTime;
				if(uint.TryParse(strTime, out uTime)) siState.DefaultDelay = uTime;
				else { Debug.Assert(false); }
			}

			return strSequence;
		}

		private static string ApplyGlobalDelay(string strSequence, SiStateEx siState)
		{
			if(string.IsNullOrEmpty(strSequence)) return string.Empty;

			// strSequence = Regex.Replace(strSequence, @"(\{.+?\}+?|.+?)",
			//	@"{delay " + strTime + @"}$1");
			// strSequence = Regex.Replace(strSequence, @"(\{.+?\}+?|([\+\^%]\(.+?\))|[\+\^%].+?|.+?)",
			//	@"{delay " + strTime + @"}$1");
			if(siState.DefaultDelay > 0)
			{
				// const string strRx = @"(\{.+?\}+?|([\+\^%]\(.+?\))|([\+\^%]\{.+?\})|[\+\^%].+?|.+?)";
				const string strRx = @"(\{.+?\}+?|([\+\^%]+\(.+?\))|([\+\^%]+\{.+?\})|[\+\^%]+.+?|.+?)";
				strSequence = Regex.Replace(strSequence, strRx, @"{DELAY " +
					siState.DefaultDelay.ToString() + @"}$1");
			}

			return strSequence;
		}

		/* private static void SendKeysWithSpecial(string strSequence, SiStateEx siState)
		{
			Debug.Assert(strSequence != null);
			if(string.IsNullOrEmpty(strSequence)) return;

			strSequence = ApplyGlobalDelay(strSequence, siState);

			while(true)
			{
				int nDelayStart = strSequence.IndexOf("{DELAY ", StrUtil.CaseIgnoreCmp);
				if(nDelayStart >= 0)
				{
					int nDelayEnd = strSequence.IndexOf('}', nDelayStart);
					if(nDelayEnd >= 0)
					{
						uint uDelay;
						string strDelay = strSequence.Substring(nDelayStart + 7,
							nDelayEnd - (nDelayStart + 7));
						if(uint.TryParse(strDelay, out uDelay))
						{
							string strFirstPart = strSequence.Substring(0,
								nDelayStart);
							string strSecondPart = strSequence.Substring(
								nDelayEnd + 1);

							if(!string.IsNullOrEmpty(strFirstPart))
								OSSendKeys(strFirstPart);
							SendKeys.Flush();
							if(uDelay == 0) uDelay = 1;
							Thread.Sleep((int)uDelay);

							strSequence = strSecondPart;
						}
						else { Debug.Assert(false); break; }
					}
					else { Debug.Assert(false); break; }
				}
				else break;
			}

			if(!string.IsNullOrEmpty(strSequence)) OSSendKeys(strSequence);
		} */

		private const string SkcDelay = "DELAY";
		private const string SkcVKey = "VKEY";
		private static readonly string[] SkcAll = new string[] {
			SkcDelay, SkcVKey
		};

		private static void SendKeysWithSpecial(string strSequence, SiStateEx siState)
		{
			Debug.Assert(strSequence != null);
			if(string.IsNullOrEmpty(strSequence)) return;

			strSequence = ExtractGlobalDelay(strSequence, siState); // Update
			strSequence = ApplyGlobalDelay(strSequence, siState);
			List<string> v = SplitSpecialSequence(strSequence);

			foreach(string strPart in v)
			{
				string strParam = GetParamIfSpecial(strPart, SkcDelay);
				if(strParam != null) // Might be empty (invalid parameter)
				{
					uint uDelay;
					if(uint.TryParse(strParam, out uDelay))
					{
						if(uDelay == 0) uDelay = 1;
						if((uDelay <= (uint)int.MaxValue) && !siState.Cancelled)
							Thread.Sleep((int)uDelay);
					}
					continue;
				}

				strParam = GetParamIfSpecial(strPart, SkcVKey);
				if(strParam != null) // Might be empty (invalid parameter)
				{
					int vKey;
					if(int.TryParse(strParam, out vKey) &&
						!KeePassLib.Native.NativeLib.IsUnix())
					{
						SendVKeyNative(vKey, true);
						SendVKeyNative(vKey, false);
						Application.DoEvents();
					}

					continue;
				}

				OSSendKeys(strPart, siState);
				Application.DoEvents(); // SendKeys.SendWait uses SendKeys.Flush

				if(siState.Cancelled) break;
			}
		}

		private static string GetParamIfSpecial(string strSeq, string strSpecialCode)
		{
			if(!strSeq.StartsWith(@"{" + strSpecialCode + @" ", StrUtil.CaseIgnoreCmp))
				return null;
			if(!strSeq.EndsWith(@"}", StrUtil.CaseIgnoreCmp)) return null;

			string strParam = strSeq.Substring(strSpecialCode.Length + 2);
			strParam = strParam.Substring(0, strParam.Length - 1); // Remove '}'

			return strParam.Trim();
		}

		private static List<string> SplitSpecialSequence(string strSeq)
		{
			List<string> v = new List<string>();
			if(string.IsNullOrEmpty(strSeq)) return v;

			v.Add(strSeq);

			bool bModified = true;
			while(bModified)
			{
				bModified = false;

				foreach(string strSplitCode in SkcAll)
				{
					List<string> vNew = new List<string>();

					foreach(string str in v)
					{
						int l = str.IndexOf(@"{" + strSplitCode + @" ", StrUtil.CaseIgnoreCmp);
						if(l < 0) { vNew.Add(str); continue; }

						int r = str.IndexOf('}', l);
						if(r < 0) { Debug.Assert(false); vNew.Add(str); continue; }

						if((l == 0) && (r == (str.Length - 1))) // Is atomic
						{
							vNew.Add(str);
							continue;
						}

						if(l > 0) vNew.Add(str.Substring(0, l));
						vNew.Add(str.Substring(l, r - l + 1));
						if(r < (str.Length - 1)) vNew.Add(str.Substring(r + 1));

						bModified = true;
					}

					v = vNew;
				}
			}

			return v;
		}

		private static void OSSendKeys(string strSequence, SiStateEx siState)
		{
			if(!ValidateTargetWindow(siState)) return;

			if(!KeePassLib.Native.NativeLib.IsUnix())
				OSSendKeysWindows(strSequence);
			else // Unix
				OSSendKeysUnix(strSequence);
		}
	}
}
