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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using KeePass.Native;

using KeePassLib.Utility;

namespace KeePass.Util.SendInputExt
{
	internal sealed class SiEngineWin : SiEngineStd
	{
		private IntPtr m_hklOriginal = IntPtr.Zero;
		private IntPtr m_hklCurrent = IntPtr.Zero;

		private bool m_bInputBlocked = false;

		private SiSendMethod? m_osmEnforced = null;

		private readonly Dictionary<IntPtr, SiWindowInfo> m_dWindowInfos =
			new Dictionary<IntPtr, SiWindowInfo>();
		private SiWindowInfo m_swiCurrent = new SiWindowInfo(IntPtr.Zero);

		// private bool m_bThreadInputAttached = false;

		private Keys m_kModCur = Keys.None;

		private static readonly int[] g_vToggleKeys = new int[] {
			NativeMethods.VK_CAPITAL // Caps Lock
		};

		public override void Init()
		{
			base.Init();

			try
			{
				InitForEnv();

				// Do not use SendKeys.Flush here, use Application.DoEvents
				// instead; SendKeys.Flush might run into an infinite loop here
				// if a previous auto-type process failed with throwing an
				// exception (SendKeys.Flush is waiting in a loop for an internal
				// queue being empty, however the queue is never processed)
				Application.DoEvents();

				// if(m_uThisThreadID != m_uTargetThreadID)
				// {
				//	m_bThreadInputAttached = NativeMethods.AttachThreadInput(
				//		m_uThisThreadID, m_uTargetThreadID, true);
				//	Debug.Assert(m_bThreadInputAttached);
				// }
				// else { Debug.Assert(false); }

				m_bInputBlocked = NativeMethods.BlockInput(true);

				uint? otLastInput = NativeMethods.GetLastInputTime();
				if(otLastInput.HasValue)
				{
					int iDiff = Environment.TickCount - (int)otLastInput.Value;
					Debug.Assert(iDiff >= 0);
					if(iDiff == 0)
					{
						// Enforce delay after pressing the global auto-type
						// hot key, as a workaround for applications
						// with broken time-dependent message processing;
						// https://sourceforge.net/p/keepass/bugs/1213/
						SleepAndDoEvents(1);
					}
				}

				PrepareSend();
				if(ReleaseModifiers(true) > 0)
				{
					// Enforce delay between releasing modifiers and sending
					// the actual sequence, as a workaround for applications
					// with broken time-dependent message processing;
					// https://sourceforge.net/p/keepass/bugs/1213/
					SleepAndDoEvents(1);
				}
			}
			catch(Exception) { Debug.Assert(false); }
		}

		public override void Release()
		{
			try
			{
				PrepareSend(); // For releasing modifiers

				if(m_bInputBlocked)
				{
					NativeMethods.BlockInput(false); // Unblock
					m_bInputBlocked = false;
				}

				Debug.Assert(m_kModCur == Keys.None);
				// Do not restore original modifier keys here, otherwise
				// modifier keys are restored even when the user released
				// them while KeePass is auto-typing
				if(ReleaseModifiers(false) != 0) { Debug.Assert(false); }

				// if(m_bThreadInputAttached)
				//	NativeMethods.AttachThreadInput(m_uThisThreadID,
				//		m_uTargetThreadID, false); // Detach

				Debug.Assert(NativeMethods.GetKeyboardLayout(0) == m_hklCurrent);
				if((m_hklCurrent != m_hklOriginal) && (m_hklOriginal != IntPtr.Zero))
				{
					if(NativeMethods.ActivateKeyboardLayout(m_hklOriginal, 0) !=
						m_hklCurrent)
					{
						Debug.Assert(false);
					}

					m_hklCurrent = m_hklOriginal;
				}

				Application.DoEvents();
			}
			catch(Exception) { Debug.Assert(false); }

			base.Release();
		}

		public override void SendKeyImpl(int iVKey, bool? obExtKey, bool? obDown)
		{
			PrepareSend();

			// Disable IME (only required for sending VKeys, not for chars);
			// https://sourceforge.net/p/keepass/discussion/329221/thread/5da4bd14/
			// using(SiImeBlocker sib = new SiImeBlocker(hWnd))

			if(obDown.HasValue)
			{
				SendVKeyNative(iVKey, obExtKey, obDown.Value);
				return;
			}

			SendVKeyNative(iVKey, obExtKey, true);
			SendVKeyNative(iVKey, obExtKey, false);
		}

		public override void SetKeyModifierImpl(Keys kMod, bool bDown)
		{
			SetKeyModifierImplEx(kMod, bDown, false);
		}

		private void SetKeyModifierImplEx(Keys kMod, bool bDown, bool bForChar)
		{
			PrepareSend();

			bool bShift = ((kMod & Keys.Shift) != Keys.None);
			bool bCtrl = ((kMod & Keys.Control) != Keys.None);
			bool bAlt = ((kMod & Keys.Alt) != Keys.None);

			if(bShift)
				SendVKeyNative((int)Keys.LShiftKey, null, bDown);
			if(bCtrl && bAlt && bForChar)
			{
				if(!m_swiCurrent.CharsRAltAsCtrlAlt)
					SendVKeyNative((int)Keys.LControlKey, null, bDown);

				// Send RAlt for better AltGr compatibility;
				// https://sourceforge.net/p/keepass/bugs/1475/
				SendVKeyNative((int)Keys.RMenu, null, bDown);
			}
			else
			{
				if(bCtrl)
					SendVKeyNative((int)Keys.LControlKey, null, bDown);
				if(bAlt)
					SendVKeyNative((int)Keys.LMenu, null, bDown);
			}

			if(bDown) m_kModCur |= kMod;
			else m_kModCur &= ~kMod;
		}

		public override void SendCharImpl(char ch, bool? obDown)
		{
			PrepareSend();

			if(TrySendCharByKeypresses(ch, obDown)) return;

			if(obDown.HasValue)
			{
				SendCharNative(ch, obDown.Value);
				return;
			}

			SendCharNative(ch, true);
			SendCharNative(ch, false);
		}

		private void InitForEnv()
		{
#if DEBUG
			Stopwatch sw = Stopwatch.StartNew();
#endif

			try
			{
				m_hklCurrent = NativeMethods.GetKeyboardLayout(0);
				m_hklOriginal = m_hklCurrent;
				Debug.Assert(m_hklOriginal != IntPtr.Zero);

				Process[] vProcesses = Process.GetProcesses();
				foreach(Process p in vProcesses)
				{
					if(p == null) { Debug.Assert(false); continue; }

					try
					{
						string strName = SiWindowInfo.GetProcessName(p);

						// If the Neo keyboard layout is being used, we must
						// send Unicode characters; keypresses are converted
						// and thus do not lead to the expected result
						if(SiWindowInfo.ProcessNameMatches(strName, "Neo20"))
						{
							FileVersionInfo fvi = p.MainModule.FileVersionInfo;
							if(((fvi.ProductName ?? string.Empty).Trim().Length == 0) &&
								((fvi.FileDescription ?? string.Empty).Trim().Length == 0))
								m_osmEnforced = SiSendMethod.UnicodePacket;
							else { Debug.Assert(false); }
						}
						else if(SiWindowInfo.ProcessNameMatches(strName, "KbdNeo_Ahk"))
							m_osmEnforced = SiSendMethod.UnicodePacket;
					}
					catch(Exception) { Debug.Assert(false); }

					try { p.Dispose(); }
					catch(Exception) { Debug.Assert(false); }
				}
			}
			catch(Exception) { Debug.Assert(false); }

#if DEBUG
			sw.Stop();
			Debug.Assert(sw.ElapsedMilliseconds < 100);
#endif
		}

		private bool SendVKeyNative(int vKey, bool? obExtKey, bool bDown)
		{
			bool bRes = false;

			if(IntPtr.Size == 4)
				bRes = SendVKeyNative32(vKey, obExtKey, null, bDown);
			else if(IntPtr.Size == 8)
				bRes = SendVKeyNative64(vKey, obExtKey, null, bDown);
			else { Debug.Assert(false); }

			// The following does not hold when sending keypresses to
			// key state-consuming windows (e.g. VM windows)
			// if(bDown && (vKey != NativeMethods.VK_CAPITAL))
			// {
			//	Debug.Assert(IsKeyActive(vKey));
			// }

			return bRes;
		}

		private bool SendCharNative(char ch, bool bDown)
		{
			if(IntPtr.Size == 4)
				return SendVKeyNative32(0, null, ch, bDown);
			else if(IntPtr.Size == 8)
				return SendVKeyNative64(0, null, ch, bDown);
			else { Debug.Assert(false); }

			return false;
		}

		private bool SendVKeyNative32(int vKey, bool? obExtKey, char? optUnicodeChar,
			bool bDown)
		{
			NativeMethods.INPUT32[] pInput = new NativeMethods.INPUT32[1];

			pInput[0].Type = NativeMethods.INPUT_KEYBOARD;

			if(optUnicodeChar.HasValue && WinUtil.IsAtLeastWindows2000)
			{
				pInput[0].KeyboardInput.VirtualKeyCode = 0;
				pInput[0].KeyboardInput.ScanCode = (ushort)optUnicodeChar.Value;
				pInput[0].KeyboardInput.Flags = ((bDown ? 0 :
					NativeMethods.KEYEVENTF_KEYUP) | NativeMethods.KEYEVENTF_UNICODE);
			}
			else
			{
				IntPtr hKL = m_swiCurrent.KeyboardLayout;

				if(optUnicodeChar.HasValue)
					vKey = (int)(NativeMethods.VkKeyScan3(optUnicodeChar.Value,
						hKL) & 0xFFU);

				pInput[0].KeyboardInput.VirtualKeyCode = (ushort)vKey;
				pInput[0].KeyboardInput.ScanCode =
					(ushort)(NativeMethods.MapVirtualKey3((uint)vKey,
					NativeMethods.MAPVK_VK_TO_VSC, hKL) & 0xFFU);
				pInput[0].KeyboardInput.Flags = GetKeyEventFlags(vKey, obExtKey, bDown);
			}

			pInput[0].KeyboardInput.Time = 0;
			pInput[0].KeyboardInput.ExtraInfo = NativeMethods.GetMessageExtraInfo();

			Debug.Assert(Marshal.SizeOf(typeof(NativeMethods.INPUT32)) == 28);
			if(NativeMethods.SendInput32(1, pInput,
				Marshal.SizeOf(typeof(NativeMethods.INPUT32))) != 1)
				return false;

			return true;
		}

		private bool SendVKeyNative64(int vKey, bool? obExtKey, char? optUnicodeChar,
			bool bDown)
		{
			NativeMethods.SpecializedKeyboardINPUT64[] pInput = new
				NativeMethods.SpecializedKeyboardINPUT64[1];

			pInput[0].Type = NativeMethods.INPUT_KEYBOARD;

			if(optUnicodeChar.HasValue && WinUtil.IsAtLeastWindows2000)
			{
				pInput[0].VirtualKeyCode = 0;
				pInput[0].ScanCode = (ushort)optUnicodeChar.Value;
				pInput[0].Flags = ((bDown ? 0 : NativeMethods.KEYEVENTF_KEYUP) |
					NativeMethods.KEYEVENTF_UNICODE);
			}
			else
			{
				IntPtr hKL = m_swiCurrent.KeyboardLayout;

				if(optUnicodeChar.HasValue)
					vKey = (int)(NativeMethods.VkKeyScan3(optUnicodeChar.Value,
						hKL) & 0xFFU);

				pInput[0].VirtualKeyCode = (ushort)vKey;
				pInput[0].ScanCode = (ushort)(NativeMethods.MapVirtualKey3(
					(uint)vKey, NativeMethods.MAPVK_VK_TO_VSC, hKL) & 0xFFU);
				pInput[0].Flags = GetKeyEventFlags(vKey, obExtKey, bDown);
			}

			pInput[0].Time = 0;
			pInput[0].ExtraInfo = NativeMethods.GetMessageExtraInfo();

			Debug.Assert(Marshal.SizeOf(typeof(NativeMethods.SpecializedKeyboardINPUT64)) == 40);
			if(NativeMethods.SendInput64Special(1, pInput,
				Marshal.SizeOf(typeof(NativeMethods.SpecializedKeyboardINPUT64))) != 1)
				return false;

			return true;
		}

		private static uint GetKeyEventFlags(int vKey, bool? obExtKey, bool bDown)
		{
			uint u = 0;

			if(!bDown) u |= NativeMethods.KEYEVENTF_KEYUP;

			if(obExtKey.HasValue)
			{
				if(obExtKey.Value) u |= NativeMethods.KEYEVENTF_EXTENDEDKEY;
			}
			else if(IsExtendedKeyEx(vKey))
				u |= NativeMethods.KEYEVENTF_EXTENDEDKEY;

			return u;
		}

		private static bool IsExtendedKeyEx(int vKey)
		{
#if DEBUG
			// https://msdn.microsoft.com/en-us/library/windows/desktop/dd375731.aspx
			// https://www.win.tue.nl/~aeb/linux/kbd/scancodes-1.html
			const uint m = NativeMethods.MAPVK_VK_TO_VSC;
			IntPtr h = IntPtr.Zero;
			Debug.Assert(NativeMethods.MapVirtualKey3((uint)
				NativeMethods.VK_LSHIFT, m, h) == 0x2AU);
			Debug.Assert(NativeMethods.MapVirtualKey3((uint)
				NativeMethods.VK_RSHIFT, m, h) == 0x36U);
			Debug.Assert(NativeMethods.MapVirtualKey3((uint)
				NativeMethods.VK_SHIFT, m, h) == 0x2AU);
			Debug.Assert(NativeMethods.MapVirtualKey3((uint)
				NativeMethods.VK_LCONTROL, m, h) == 0x1DU);
			Debug.Assert(NativeMethods.MapVirtualKey3((uint)
				NativeMethods.VK_RCONTROL, m, h) == 0x1DU);
			Debug.Assert(NativeMethods.MapVirtualKey3((uint)
				NativeMethods.VK_CONTROL, m, h) == 0x1DU);
			Debug.Assert(NativeMethods.MapVirtualKey3((uint)
				NativeMethods.VK_LMENU, m, h) == 0x38U);
			Debug.Assert(NativeMethods.MapVirtualKey3((uint)
				NativeMethods.VK_RMENU, m, h) == 0x38U);
			Debug.Assert(NativeMethods.MapVirtualKey3((uint)
				NativeMethods.VK_MENU, m, h) == 0x38U);
			Debug.Assert(NativeMethods.MapVirtualKey3(0x5BU, m, h) == 0x5BU);
			Debug.Assert(NativeMethods.MapVirtualKey3(0x5CU, m, h) == 0x5CU);
			Debug.Assert(NativeMethods.MapVirtualKey3(0x5DU, m, h) == 0x5DU);
			Debug.Assert(NativeMethods.MapVirtualKey3(0x6AU, m, h) == 0x37U);
			Debug.Assert(NativeMethods.MapVirtualKey3(0x6BU, m, h) == 0x4EU);
			Debug.Assert(NativeMethods.MapVirtualKey3(0x6DU, m, h) == 0x4AU);
			Debug.Assert(NativeMethods.MapVirtualKey3(0x6EU, m, h) == 0x53U);
			Debug.Assert(NativeMethods.MapVirtualKey3(0x6FU, m, h) == 0x35U);
#endif

			if((vKey >= 0x21) && (vKey <= 0x2E)) return true;
			if((vKey >= 0x5B) && (vKey <= 0x5D)) return true;
			if(vKey == 0x6F) return true; // VK_DIVIDE

			// RShift is separate; no E0
			if(vKey == NativeMethods.VK_RCONTROL) return true;
			if(vKey == NativeMethods.VK_RMENU) return true;

			return false;
		}

		private int ReleaseModifiers(bool bWithSpecial)
		{
			List<int> lMods = new List<int>();
			lMods.AddRange(new int[] {
				NativeMethods.VK_LWIN, NativeMethods.VK_RWIN,
				NativeMethods.VK_LSHIFT, NativeMethods.VK_RSHIFT,
				NativeMethods.VK_SHIFT,
				NativeMethods.VK_LCONTROL, NativeMethods.VK_RCONTROL,
				NativeMethods.VK_CONTROL,
				NativeMethods.VK_LMENU, NativeMethods.VK_RMENU,
				NativeMethods.VK_MENU
			});
			lMods.AddRange(g_vToggleKeys);

			List<int> lReleased = new List<int>();

			foreach(int vKey in lMods)
			{
				if(IsKeyActive(vKey))
				{
					// The generic modifiers should not be activated,
					// when the left and right keys are up
					Debug.Assert(vKey != NativeMethods.VK_SHIFT);
					Debug.Assert(vKey != NativeMethods.VK_CONTROL);
					Debug.Assert(vKey != NativeMethods.VK_MENU);

					ActivateOrToggleKey(vKey, false);
					lReleased.Add(vKey);

					Application.DoEvents();
				}
			}

			if(bWithSpecial)
				ReleaseModifiersSpecialPost(lReleased);

			return lReleased.Count;
		}

		private static bool IsKeyActive(int vKey)
		{
			if(Array.IndexOf<int>(g_vToggleKeys, vKey) >= 0)
			{
				ushort us = NativeMethods.GetKeyState(vKey);
				return ((us & 1) != 0);
			}

			ushort usState = NativeMethods.GetAsyncKeyState(vKey);
			return ((usState & 0x8000) != 0);

			// For GetKeyState:
			// if(Array.IndexOf<int>(g_vToggleKeys, vKey) >= 0)
			//	return ((usState & 1) != 0);
			// else
			//	return ((usState & 0x8000) != 0);
		}

		private void ActivateOrToggleKey(int vKey, bool bDown)
		{
			if(Array.IndexOf<int>(g_vToggleKeys, vKey) >= 0)
			{
				SendVKeyNative(vKey, null, true);
				SendVKeyNative(vKey, null, false);
			}
			else SendVKeyNative(vKey, null, bDown);
		}

		private void ReleaseModifiersSpecialPost(List<int> vKeys)
		{
			if(vKeys.Count == 0) return;

			// Get out of a menu bar that was focused when only
			// using Alt as hot key modifier
			if(Program.Config.Integration.AutoTypeReleaseAltWithKeyPress &&
				vKeys.TrueForAll(SiEngineWin.IsAltOrToggle))
			{
				if(vKeys.Contains(NativeMethods.VK_LMENU))
				{
					SendVKeyNative(NativeMethods.VK_LMENU, null, true);
					SendVKeyNative(NativeMethods.VK_LMENU, null, false);
				}
				else if(vKeys.Contains(NativeMethods.VK_RMENU))
				{
					SendVKeyNative(NativeMethods.VK_RMENU, null, true);
					SendVKeyNative(NativeMethods.VK_RMENU, null, false);
				}
			}
		}

		private static bool IsAltOrToggle(int vKey)
		{
			if(vKey == NativeMethods.VK_LMENU) return true;
			if(vKey == NativeMethods.VK_RMENU) return true;
			if(vKey == NativeMethods.VK_MENU) return true;

			if(Array.IndexOf<int>(g_vToggleKeys, vKey) >= 0) return true;

			return false;
		}

		private static char[] m_vForcedUniChars = null;
		private bool TrySendCharByKeypresses(char ch, bool? obDown)
		{
			if(ch == char.MinValue) { Debug.Assert(false); return false; }

			SiSendMethod sm = GetSendMethod(m_swiCurrent);
			if(sm == SiSendMethod.UnicodePacket) return false;

			// Sync. with documentation
			if(m_vForcedUniChars == null)
				m_vForcedUniChars = new char[] {
					// All of the following diacritics are spacing / non-combining

					'\u005E', // Circumflex ^
					'\u0060', // Grave accent
					'\u00A8', // Diaeresis
					'\u00AF', // Macron above, long
					'\u00B0', // Degree (e.g. for Czech)
					'\u00B4', // Acute accent
					'\u00B8', // Cedilla

					// E.g. for US-International;
					// https://sourceforge.net/p/keepass/discussion/329220/thread/5708e5ef/
					'\u0022', // Quotation mark
					'\u0027', // Apostrophe
					'\u007E' // Tilde

					// Spacing Modifier Letters; see below
					// '\u02C7', // Caron (e.g. for Canadian Multilingual)
					// '\u02C9', // Macron above, modifier, short
					// '\u02CD', // Macron below, modifier, short
					// '\u02D8', // Breve
					// '\u02D9', // Dot above
					// '\u02DA', // Ring above
					// '\u02DB', // Ogonek
					// '\u02DC', // Small tilde
					// '\u02DD', // Double acute accent
				};
			if(sm != SiSendMethod.KeyEvent) // If Unicode packets allowed
			{
				if(Array.IndexOf<char>(m_vForcedUniChars, ch) >= 0) return false;

				// U+02B0 to U+02FF are Spacing Modifier Letters;
				// https://www.unicode.org/charts/PDF/U02B0.pdf
				// https://en.wikipedia.org/wiki/Spacing_Modifier_Letters
				if((ch >= '\u02B0') && (ch <= '\u02FF')) return false;
			}

			IntPtr hKL = m_swiCurrent.KeyboardLayout;
			ushort u = NativeMethods.VkKeyScan3(ch, hKL);
			if(u == 0xFFFFU) return false;

			int vKey = (int)(u & 0xFFU);

			Keys kMod = Keys.None;
			int nMods = 0;
			if((u & 0x100U) != 0U) { ++nMods; kMod |= Keys.Shift; }
			if((u & 0x200U) != 0U) { ++nMods; kMod |= Keys.Control; }
			if((u & 0x400U) != 0U) { ++nMods; kMod |= Keys.Alt; }
			if((u & 0x800U) != 0U) return false; // Hankaku unsupported

			// Do not send a key combination that is registered as hot key;
			// https://sourceforge.net/p/keepass/bugs/1235/
			// Windows shortcut hot keys involve at least 2 modifiers
			if(nMods >= 2)
			{
				Keys kFull = (kMod | (Keys)vKey);
				if(HotKeyManager.IsHotKeyRegistered(kFull, true))
					return false;
			}

			bool bShift = ((kMod & Keys.Shift) != Keys.None);
			bool bCtrl = ((kMod & Keys.Control) != Keys.None);
			bool bAlt = ((kMod & Keys.Alt) != Keys.None);
			bool bCapsLock = false;

			// Windows' GetKeyboardState function does not return the
			// current virtual key array (especially not after changing
			// them below), thus we build the array on our own
			byte[] pbState = new byte[256];
			if(bShift)
			{
				pbState[NativeMethods.VK_SHIFT] = 0x80;
				pbState[NativeMethods.VK_LSHIFT] = 0x80;
			}
			if(bCtrl && bAlt) // Use LCtrl+RAlt as Ctrl+Alt
			{
				pbState[NativeMethods.VK_CONTROL] = 0x80;
				pbState[NativeMethods.VK_LCONTROL] = 0x80;
				pbState[NativeMethods.VK_MENU] = 0x80;
				pbState[NativeMethods.VK_RMENU] = 0x80;
			}
			else
			{
				if(bCtrl)
				{
					pbState[NativeMethods.VK_CONTROL] = 0x80;
					pbState[NativeMethods.VK_LCONTROL] = 0x80;
				}
				if(bAlt)
				{
					pbState[NativeMethods.VK_MENU] = 0x80;
					pbState[NativeMethods.VK_LMENU] = 0x80;
				}
			}
			pbState[NativeMethods.VK_NUMLOCK] = 0x01; // Toggled

			// The keypress that VkKeyScan returns may require a specific
			// state of toggle keys, on which it provides no information;
			// thus we now check whether the keypress will really result
			// in the character that we expect;
			// https://sourceforge.net/p/keepass/bugs/1594/
			string strUni = NativeMethods.ToUnicode3(vKey, pbState, hKL);
			if((strUni != null) && (strUni.Length == 0)) { } // Dead key
			else if(string.IsNullOrEmpty(strUni) || (strUni[strUni.Length - 1] != ch))
			{
				// Among the keyboard layouts that were tested, the
				// Czech one was the only one where the translation
				// may fail (due to dependency on the Caps Lock state)
				Debug.Assert(NativeMethods.GetPrimaryLangID((ushort)(hKL.ToInt64() &
					0xFFFFL)) == NativeMethods.LANG_CZECH);

				// Test whether Caps Lock is required
				pbState[NativeMethods.VK_CAPITAL] = 0x01;
				strUni = NativeMethods.ToUnicode3(vKey, pbState, hKL);
				if((strUni != null) && (strUni.Length == 0)) { } // Dead key
				else if(string.IsNullOrEmpty(strUni) || (strUni[strUni.Length - 1] != ch))
				{
					Debug.Assert(false); // An unknown key modifier is required
					return false;
				}

				bCapsLock = true;
			}

			Keys kModDiff = (kMod & ~m_kModCur);
			bool bSleep = (bCapsLock || (kModDiff != Keys.None));
			int msSleep = m_swiCurrent.SleepAroundKeyMod;

			if(bCapsLock)
				SendKeyImpl(NativeMethods.VK_CAPITAL, null, null);

			if(kModDiff != Keys.None)
				SetKeyModifierImplEx(kModDiff, true, true);

			if(bSleep) SleepAndDoEvents(msSleep);

			SendKeyImpl(vKey, null, obDown);

			if(bSleep) SleepAndDoEvents(msSleep);

			if(kModDiff != Keys.None)
				SetKeyModifierImplEx(kModDiff, false, true);

			if(bCapsLock)
				SendKeyImpl(NativeMethods.VK_CAPITAL, null, null);

			if(bSleep) SleepAndDoEvents(msSleep);

			return true;
		}

		private SiSendMethod GetSendMethod(SiWindowInfo swi)
		{
			if(m_osmEnforced.HasValue) return m_osmEnforced.Value;

			return swi.SendMethod;
		}

		private SiWindowInfo GetWindowInfo(IntPtr hWnd)
		{
			SiWindowInfo swi;
			if(m_dWindowInfos.TryGetValue(hWnd, out swi)) return swi;

			swi = new SiWindowInfo(hWnd);
			m_dWindowInfos[hWnd] = swi;
			return swi;
		}

		private void PrepareSend()
		{
			IntPtr hWnd = NativeMethods.GetForegroundWindowHandle();
			m_swiCurrent = GetWindowInfo(hWnd);

			EnsureSameKeyboardLayout();
		}

		private void EnsureSameKeyboardLayout()
		{
			if(!Program.Config.Integration.AutoTypeAdjustKeyboardLayout) return;

			IntPtr hklTarget = m_swiCurrent.KeyboardLayout;
			Debug.Assert(hklTarget != IntPtr.Zero);
			Debug.Assert(NativeMethods.GetKeyboardLayout(0) == m_hklCurrent);

			if((m_hklCurrent != hklTarget) && (hklTarget != IntPtr.Zero))
			{
				if(NativeMethods.ActivateKeyboardLayout(hklTarget, 0) != m_hklCurrent)
				{
					Debug.Assert(false);
				}
				m_hklCurrent = hklTarget;

				SleepAndDoEvents(1);
			}
		}

		private static void SleepAndDoEvents(int msSleep)
		{
			if(msSleep >= 0) Thread.Sleep(msSleep);
			Application.DoEvents();
		}
	}

	/* internal sealed class SiImeBlocker : IDisposable
	{
		private IntPtr m_hWnd;
		private IntPtr m_hOrgIme = IntPtr.Zero;

		public SiImeBlocker(IntPtr hWnd)
		{
			m_hWnd = hWnd;
			if(hWnd == IntPtr.Zero) return;

			try { m_hOrgIme = NativeMethods.ImmAssociateContext(hWnd, IntPtr.Zero); }
			catch(Exception) { Debug.Assert(false); }
		}

		~SiImeBlocker()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool bDisposing)
		{
			if(m_hOrgIme != IntPtr.Zero)
			{
				try { NativeMethods.ImmAssociateContext(m_hWnd, m_hOrgIme); }
				catch(Exception) { Debug.Assert(false); }

				m_hOrgIme = IntPtr.Zero;
			}
		}
	} */
}
