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
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

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
	}

	public static class SendInputEx
	{
		// private const ushort LangIDGerman = 0x0407;

		public static void SendKeysWait(string strKeys, bool bObfuscate)
		{
			if(KeePassLib.Native.NativeLib.IsUnix())
			{
				try { OSSendKeys(strKeys); }
				catch(Exception) { Debug.Assert(false); }
				return;
			}

			SiStateEx si = InitSendKeys();

			try
			{
				Debug.Assert(GetActiveKeyModifiers().Count == 0);

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

			try
			{
				EnsureSameKeyboardLayout(si);

				// Do not use SendKeys.Flush here, use Application.DoEvents
				// instead; SendKeys.Flush might run into an infinite loop here
				// if a previous auto-type process failed with throwing an
				// exception (SendKeys.Flush is waiting in a loop for an internal
				// queue being empty, however the queue is never processed)
				Application.DoEvents();

				List<int> lMod = GetActiveKeyModifiers();
				ActivateKeyModifiers(lMod, false);
				SpecialReleaseModifiers(lMod);

				si.InputBlocked = NativeMethods.BlockInput(true);
			}
			catch(Exception) { Debug.Assert(false); }

			return si;
		}

		private static void FinishSendKeys(SiStateEx si)
		{
			try
			{
				// Do not restore original modifier keys here, otherwise
				// modifier keys are restored even when the user released
				// them while KeePass is auto-typing!
				// ActivateKeyModifiers(lRestore, true);

				if(si.InputBlocked) NativeMethods.BlockInput(false); // Unblock

				if(si.OriginalKeyboardLayout != IntPtr.Zero)
					NativeMethods.ActivateKeyboardLayout(si.OriginalKeyboardLayout, 0);

				Application.DoEvents();
			}
			catch(Exception) { Debug.Assert(false); }
		}

		private static void EnsureSameKeyboardLayout(SiStateEx si)
		{
			IntPtr hWndTarget = NativeMethods.GetForegroundWindow();

			uint uTargetProcessId;
			uint uTargetThreadId = NativeMethods.GetWindowThreadProcessId(hWndTarget,
				out uTargetProcessId);
			
			IntPtr hklSelf = NativeMethods.GetKeyboardLayout(0);
			IntPtr hklTarget = NativeMethods.GetKeyboardLayout(uTargetThreadId);

			si.CurrentKeyboardLayout = hklSelf;

			if(hklSelf != hklTarget)
			{
				si.OriginalKeyboardLayout = NativeMethods.ActivateKeyboardLayout(
					hklTarget, 0);
				si.CurrentKeyboardLayout = hklTarget;

				Debug.Assert(si.OriginalKeyboardLayout == hklSelf);
			}

			// ushort uLangID = (ushort)(si.CurrentKeyboardLayout.ToInt64() & 0xFFFF);
			// si.EnableCaretWorkaround = (uLangID == LangIDGerman);
		}

		private static bool SendModifierVKey(int vKey, bool bDown)
		{
			if(bDown || IsKeyModifierActive(vKey))
			{
				if(IntPtr.Size == 4) return SendModifierVKey32Unchecked(vKey, bDown);
				else if(IntPtr.Size == 8) return SendModifierVKey64Unchecked(vKey, bDown);
				else { Debug.Assert(false); }
			}

			return false;
		}

		private static bool SendModifierVKey32Unchecked(int vKey, bool bDown)
		{
			NativeMethods.INPUT32[] pInput = new NativeMethods.INPUT32[1];

			pInput[0].Type = NativeMethods.INPUT_KEYBOARD;
			pInput[0].KeyboardInput.VirtualKeyCode = (ushort)vKey;
			pInput[0].KeyboardInput.ScanCode =
				(ushort)(NativeMethods.MapVirtualKey((uint)vKey, 0) & 0xFF);
			pInput[0].KeyboardInput.Flags = ((bDown ? 0 :
				NativeMethods.KEYEVENTF_KEYUP) | (IsExtendedKeyEx(vKey) ?
				NativeMethods.KEYEVENTF_EXTENDEDKEY : 0));
			pInput[0].KeyboardInput.Time = 0;
			pInput[0].KeyboardInput.ExtraInfo = NativeMethods.GetMessageExtraInfo();

			Debug.Assert(Marshal.SizeOf(typeof(NativeMethods.INPUT32)) == 28);
			if(NativeMethods.SendInput32(1, pInput,
				Marshal.SizeOf(typeof(NativeMethods.INPUT32))) != 1)
				return false;

			return true;
		}

		private static bool SendModifierVKey64Unchecked(int vKey, bool bDown)
		{
			NativeMethods.SpecializedKeyboardINPUT64[] pInput = new
				NativeMethods.SpecializedKeyboardINPUT64[1];

			pInput[0].Type = NativeMethods.INPUT_KEYBOARD;
			pInput[0].VirtualKeyCode = (ushort)vKey;
			pInput[0].ScanCode = (ushort)(NativeMethods.MapVirtualKey(
				(uint)vKey, 0) & 0xFF);
			pInput[0].Flags = ((bDown ? 0 : NativeMethods.KEYEVENTF_KEYUP) |
				(IsExtendedKeyEx(vKey) ? NativeMethods.KEYEVENTF_EXTENDEDKEY : 0));
			pInput[0].Time = 0;
			pInput[0].ExtraInfo = NativeMethods.GetMessageExtraInfo();

			Debug.Assert(Marshal.SizeOf(typeof(NativeMethods.SpecializedKeyboardINPUT64)) == 40);
			if(NativeMethods.SendInput64Special(1, pInput,
				Marshal.SizeOf(typeof(NativeMethods.SpecializedKeyboardINPUT64))) != 1)
				return false;

			return true;
		}

		private static bool IsExtendedKeyEx(int vKey)
		{
			// if(vKey == NativeMethods.VK_CAPITAL) return true;

			if((vKey >= 0x21) && (vKey <= 0x2E)) return true;
			if((vKey >= 0x6A) && (vKey <= 0x6F)) return true;

			return false;
		}

		private static List<int> GetActiveKeyModifiers()
		{
			List<int> lSet = new List<int>();

			AddKeyModifierIfSet(lSet, NativeMethods.VK_LSHIFT);
			AddKeyModifierIfSet(lSet, NativeMethods.VK_RSHIFT);
			AddKeyModifierIfSet(lSet, NativeMethods.VK_SHIFT);

			AddKeyModifierIfSet(lSet, NativeMethods.VK_LCONTROL);
			AddKeyModifierIfSet(lSet, NativeMethods.VK_RCONTROL);
			AddKeyModifierIfSet(lSet, NativeMethods.VK_CONTROL);

			AddKeyModifierIfSet(lSet, NativeMethods.VK_LMENU);
			AddKeyModifierIfSet(lSet, NativeMethods.VK_RMENU);
			AddKeyModifierIfSet(lSet, NativeMethods.VK_MENU);

			AddKeyModifierIfSet(lSet, NativeMethods.VK_LWIN);
			AddKeyModifierIfSet(lSet, NativeMethods.VK_RWIN);

			AddKeyModifierIfSet(lSet, NativeMethods.VK_CAPITAL);

			return lSet;
		}

		private static void AddKeyModifierIfSet(List<int> lList, int vKey)
		{
			if(IsKeyModifierActive(vKey)) lList.Add(vKey);
		}

		private static bool IsKeyModifierActive(int vKey)
		{
			ushort usState = NativeMethods.GetKeyState(vKey);

			if(vKey == NativeMethods.VK_CAPITAL)
				return ((usState & 1) != 0);
			else
				return ((usState & 0x8000) != 0);
		}

		private static void ActivateKeyModifiers(List<int> vKeys, bool bDown)
		{
			Debug.Assert(vKeys != null);
			if(vKeys == null) throw new ArgumentNullException("vKeys");

			foreach(int vKey in vKeys)
			{
				if(vKey == NativeMethods.VK_CAPITAL) // Toggle
				{
					SendModifierVKey(vKey, true);
					SendModifierVKey(vKey, false);
				}
				else SendModifierVKey(vKey, bDown);
			}
		}

		private static void SpecialReleaseModifiers(List<int> vKeys)
		{
			// Get out of a menu bar that was focused when only
			// using Alt as hot key modifier
			if(Program.Config.Integration.AutoTypeReleaseAltWithKeyPress &&
				(vKeys.Count == 2) && vKeys.Contains(NativeMethods.VK_MENU))
			{
				if(vKeys.Contains(NativeMethods.VK_LMENU))
				{
					SendModifierVKey(NativeMethods.VK_LMENU, true);
					SendModifierVKey(NativeMethods.VK_LMENU, false);
				}
				else if(vKeys.Contains(NativeMethods.VK_RMENU))
				{
					SendModifierVKey(NativeMethods.VK_RMENU, true);
					SendModifierVKey(NativeMethods.VK_RMENU, false);
				}
			}
		}

		private static void SendObfuscated(string strKeys, SiStateEx siState)
		{
			Debug.Assert(strKeys != null);
			if(strKeys == null) throw new ArgumentNullException("strKeys");
			if(strKeys.Length == 0) return;

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

			if(strClip.Length > 0) Clipboard.SetText(strClip);
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

		private static string ApplyGlobalDelay(string strSequence, SiStateEx siState)
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

		private static void SendKeysWithSpecial(string strSequence, SiStateEx siState)
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
		}

		private static void OSSendKeys(string strSequence)
		{
			if(!KeePassLib.Native.NativeLib.IsUnix())
				SendKeys.SendWait(strSequence);
			else // Unix
				OSSendKeysUnix(strSequence);
		}

		private static void OSSendKeysUnix(string strSequence)
		{
			StringBuilder sb = new StringBuilder();

			for(int i = 0; i < strSequence.Length; ++i)
			{
				char ch = strSequence[i];

				if(ch == '{')
				{
					if(sb.Length > 0)
					{
						OSSendKeysUnixString(sb.ToString());
						sb.Remove(0, sb.Length);
					}
				}
				else if(ch == '}')
				{
					if(sb.Length > 0)
					{
						// KeySyms need to be in Pascal case (e.g. "Tab")
						string strKey = sb.ToString().ToLower();
						strKey = ((new string(strKey[0], 1)).ToUpper() +
							strKey.Substring(1));

						if(strKey == "Enter") strKey = "Return";

						if(strKey.StartsWith("Delay")) { }
						else OSSendKeysUnixKey(strKey);

						sb.Remove(0, sb.Length);
					}
				}
				else sb.Append(ch);
			}

			if(sb.Length > 0) OSSendKeysUnixString(sb.ToString());
		}

		private static void OSSendKeysUnixKey(string strKey)
		{
			NativeMethods.RunXDoTool("key " + strKey);
		}

		private static void OSSendKeysUnixString(string strString)
		{
			NativeMethods.RunXDoTool(@"type '" + strString + @"'");
		}
	}
}
