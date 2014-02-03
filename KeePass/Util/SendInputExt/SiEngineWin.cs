/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2014 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;

using KeePass.Native;

using KeePassLib.Utility;

namespace KeePass.Util.SendInputExt
{
	internal sealed class SiEngineWin : SiEngineStd
	{
		private IntPtr m_pOriginalKeyboardLayout = IntPtr.Zero;
		// private IntPtr m_pCurrentKeyboardLayout = IntPtr.Zero;

		// private uint m_uThisThreadID = 0;
		private uint m_uTargetThreadID = 0;
		// private uint m_uTargetProcessID = 0;

		private bool m_bInputBlocked = false;
		private bool m_bForceUnicodeChars = false;

		// private bool m_bThreadInputAttached = false;

		private Keys m_kModCur = Keys.None;

		public override void Init()
		{
			base.Init();

			try
			{
				// m_uThisThreadID = NativeMethods.GetCurrentThreadId();
				uint uTargetProcessID;
				m_uTargetThreadID = NativeMethods.GetWindowThreadProcessId(
					this.TargetHWnd, out uTargetProcessID);
				// m_uTargetProcessID = uTargetProcessID;

				ConfigureForEnv();

				EnsureSameKeyboardLayout();

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

				ReleaseModifiers(true);
			}
			catch(Exception) { Debug.Assert(false); }
		}

		public override void Release()
		{
			try
			{
				Debug.Assert(m_kModCur == Keys.None);

				if(m_bInputBlocked)
				{
					NativeMethods.BlockInput(false); // Unblock
					m_bInputBlocked = false;
				}

				Debug.Assert(GetActiveKeyModifiers().Count == 0);
				// Do not restore original modifier keys here, otherwise
				// modifier keys are restored even when the user released
				// them while KeePass is auto-typing!
				// ActivateKeyModifiers(lRestore, true);
				ReleaseModifiers(false);

				// if(m_bThreadInputAttached)
				//	NativeMethods.AttachThreadInput(m_uThisThreadID,
				//		m_uTargetThreadID, false); // Detach

				if(m_pOriginalKeyboardLayout != IntPtr.Zero)
				{
					NativeMethods.ActivateKeyboardLayout(m_pOriginalKeyboardLayout, 0);
					m_pOriginalKeyboardLayout = IntPtr.Zero;
				}

				Application.DoEvents();
			}
			catch(Exception) { Debug.Assert(false); }

			base.Release();
		}

		public override void SendKeyImpl(int iVKey, bool? bExtKey, bool? bDown)
		{
			// IntPtr hWnd = NativeMethods.GetForegroundWindowHandle();

			// Disable IME (only required for sending VKeys, not for chars);
			// https://sourceforge.net/p/keepass/discussion/329221/thread/5da4bd14/
			// using(SiImeBlocker sib = new SiImeBlocker(hWnd))

			if(bDown.HasValue)
			{
				SendVKeyNative(iVKey, bExtKey, bDown.Value);
				return;
			}

			SendVKeyNative(iVKey, bExtKey, true);
			SendVKeyNative(iVKey, bExtKey, false);
		}

		public override void SetKeyModifierImpl(Keys kMod, bool bDown)
		{
			if((kMod & Keys.Shift) != Keys.None)
				SendVKeyNative((int)Keys.ShiftKey, null, bDown);
			if((kMod & Keys.Control) != Keys.None)
				SendVKeyNative((int)Keys.ControlKey, null, bDown);
			if((kMod & Keys.Alt) != Keys.None)
				SendVKeyNative((int)Keys.Menu, null, bDown);

			if(bDown) m_kModCur |= kMod;
			else m_kModCur &= ~kMod;
		}

		public override void SendCharImpl(char ch, bool? bDown)
		{
			if(TrySendCharByKeypresses(ch, bDown)) return;

			if(bDown.HasValue)
			{
				SendCharNative(ch, bDown.Value);
				return;
			}

			SendCharNative(ch, true);
			SendCharNative(ch, false);
		}

		private void EnsureSameKeyboardLayout()
		{
			IntPtr hklSelf = NativeMethods.GetKeyboardLayout(0);
			IntPtr hklTarget = NativeMethods.GetKeyboardLayout(m_uTargetThreadID);

			// m_pCurrentKeyboardLayout = hklSelf;

			if(!Program.Config.Integration.AutoTypeAdjustKeyboardLayout) return;

			if(hklSelf != hklTarget)
			{
				m_pOriginalKeyboardLayout = NativeMethods.ActivateKeyboardLayout(
					hklTarget, 0);
				// m_pCurrentKeyboardLayout = hklTarget;

				Debug.Assert(m_pOriginalKeyboardLayout == hklSelf);
			}
		}

		private void ConfigureForEnv()
		{
#if DEBUG
			Stopwatch sw = Stopwatch.StartNew();
#endif

			try
			{
				Process[] v = Process.GetProcesses();
				foreach(Process p in v)
				{
					if(p == null) { Debug.Assert(false); continue; }

					try
					{
						string strName = p.ProcessName.Trim();

						// If the Neo keyboard layout is being used, we must
						// send Unicode characters; keypresses are converted
						// and thus do not lead to the expected result
						if(strName.Equals("Neo20.exe", StrUtil.CaseIgnoreCmp) ||
							strName.Equals("Neo20", StrUtil.CaseIgnoreCmp))
						{
							FileVersionInfo fvi = p.MainModule.FileVersionInfo;
							if(((fvi.ProductName ?? string.Empty).Trim().Length == 0) &&
								((fvi.FileDescription ?? string.Empty).Trim().Length == 0))
								m_bForceUnicodeChars = true;
							else { Debug.Assert(false); }
						}
						else if(strName.Equals("KbdNeo_Ahk.exe", StrUtil.CaseIgnoreCmp) ||
							strName.Equals("KbdNeo_Ahk", StrUtil.CaseIgnoreCmp))
							m_bForceUnicodeChars = true;
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

		private static bool SendVKeyNative(int vKey, bool? bExtKey, bool bDown)
		{
			bool bRes = false;

			if(IntPtr.Size == 4)
				bRes = SendVKeyNative32(vKey, bExtKey, null, bDown);
			else if(IntPtr.Size == 8)
				bRes = SendVKeyNative64(vKey, bExtKey, null, bDown);
			else { Debug.Assert(false); }

			// The following does not hold when sending keypresses to
			// key state-consuming windows (e.g. VM windows)
			// if(bDown && (vKey != NativeMethods.VK_CAPITAL))
			// {
			//	Debug.Assert(IsKeyActive(vKey));
			// }

			return bRes;
		}

		private static bool SendCharNative(char ch, bool bDown)
		{
			if(IntPtr.Size == 4)
				return SendVKeyNative32(0, null, ch, bDown);
			else if(IntPtr.Size == 8)
				return SendVKeyNative64(0, null, ch, bDown);
			else { Debug.Assert(false); }

			return false;
		}

		private static bool SendVKeyNative32(int vKey, bool? bExtKey,
			char? optUnicodeChar, bool bDown)
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
				if(optUnicodeChar.HasValue)
					vKey = (int)(NativeMethods.VkKeyScan(optUnicodeChar.Value) & 0xFFU);

				pInput[0].KeyboardInput.VirtualKeyCode = (ushort)vKey;
				pInput[0].KeyboardInput.ScanCode =
					(ushort)(NativeMethods.MapVirtualKey((uint)vKey, 0) & 0xFFU);
				pInput[0].KeyboardInput.Flags = GetKeyEventFlags(vKey, bExtKey, bDown);
			}

			pInput[0].KeyboardInput.Time = 0;
			pInput[0].KeyboardInput.ExtraInfo = NativeMethods.GetMessageExtraInfo();

			Debug.Assert(Marshal.SizeOf(typeof(NativeMethods.INPUT32)) == 28);
			if(NativeMethods.SendInput32(1, pInput,
				Marshal.SizeOf(typeof(NativeMethods.INPUT32))) != 1)
				return false;

			return true;
		}

		private static bool SendVKeyNative64(int vKey, bool? bExtKey,
			char? optUnicodeChar, bool bDown)
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
				if(optUnicodeChar.HasValue)
					vKey = (int)(NativeMethods.VkKeyScan(optUnicodeChar.Value) & 0xFFU);

				pInput[0].VirtualKeyCode = (ushort)vKey;
				pInput[0].ScanCode = (ushort)(NativeMethods.MapVirtualKey(
					(uint)vKey, 0) & 0xFFU);
				pInput[0].Flags = GetKeyEventFlags(vKey, bExtKey, bDown);
			}

			pInput[0].Time = 0;
			pInput[0].ExtraInfo = NativeMethods.GetMessageExtraInfo();

			Debug.Assert(Marshal.SizeOf(typeof(NativeMethods.SpecializedKeyboardINPUT64)) == 40);
			if(NativeMethods.SendInput64Special(1, pInput,
				Marshal.SizeOf(typeof(NativeMethods.SpecializedKeyboardINPUT64))) != 1)
				return false;

			return true;
		}

		private static uint GetKeyEventFlags(int vKey, bool? bExtKey, bool bDown)
		{
			uint u = 0;

			if(!bDown) u |= NativeMethods.KEYEVENTF_KEYUP;

			if(bExtKey.HasValue)
			{
				if(bExtKey.Value) u |= NativeMethods.KEYEVENTF_EXTENDEDKEY;
			}
			else if(IsExtendedKeyEx(vKey))
				u |= NativeMethods.KEYEVENTF_EXTENDEDKEY;

			return u;
		}

		private static bool IsExtendedKeyEx(int vKey)
		{
			// http://msdn.microsoft.com/en-us/library/windows/desktop/dd375731.aspx
			// http://www.win.tue.nl/~aeb/linux/kbd/scancodes-1.html
			Debug.Assert(NativeMethods.MapVirtualKey((uint)
				NativeMethods.VK_LSHIFT, 0) == 0x2AU);
			Debug.Assert(NativeMethods.MapVirtualKey((uint)
				NativeMethods.VK_RSHIFT, 0) == 0x36U);
			Debug.Assert(NativeMethods.MapVirtualKey((uint)
				NativeMethods.VK_SHIFT, 0) == 0x2AU);
			Debug.Assert(NativeMethods.MapVirtualKey((uint)
				NativeMethods.VK_LCONTROL, 0) == 0x1DU);
			Debug.Assert(NativeMethods.MapVirtualKey((uint)
				NativeMethods.VK_RCONTROL, 0) == 0x1DU);
			Debug.Assert(NativeMethods.MapVirtualKey((uint)
				NativeMethods.VK_CONTROL, 0) == 0x1DU);
			Debug.Assert(NativeMethods.MapVirtualKey((uint)
				NativeMethods.VK_LMENU, 0) == 0x38U);
			Debug.Assert(NativeMethods.MapVirtualKey((uint)
				NativeMethods.VK_RMENU, 0) == 0x38U);
			Debug.Assert(NativeMethods.MapVirtualKey((uint)
				NativeMethods.VK_MENU, 0) == 0x38U);
			Debug.Assert(NativeMethods.MapVirtualKey(0x5BU, 0) == 0x5BU);
			Debug.Assert(NativeMethods.MapVirtualKey(0x5CU, 0) == 0x5CU);
			Debug.Assert(NativeMethods.MapVirtualKey(0x5DU, 0) == 0x5DU);
			Debug.Assert(NativeMethods.MapVirtualKey(0x6AU, 0) == 0x37U);
			Debug.Assert(NativeMethods.MapVirtualKey(0x6BU, 0) == 0x4EU);
			Debug.Assert(NativeMethods.MapVirtualKey(0x6DU, 0) == 0x4AU);
			Debug.Assert(NativeMethods.MapVirtualKey(0x6EU, 0) == 0x53U);
			Debug.Assert(NativeMethods.MapVirtualKey(0x6FU, 0) == 0x35U);

			if((vKey >= 0x21) && (vKey <= 0x2E)) return true;
			if((vKey >= 0x5B) && (vKey <= 0x5D)) return true;
			if(vKey == 0x6F) return true; // VK_DIVIDE

			// RShift is separate; no E0
			if(vKey == NativeMethods.VK_RCONTROL) return true;
			if(vKey == NativeMethods.VK_RMENU) return true;

			return false;
		}

		private static int ReleaseModifiers(bool bWithSpecial)
		{
			List<int> lMod = GetActiveKeyModifiers();
			ActivateKeyModifiers(lMod, false);

			if(bWithSpecial) SpecialReleaseModifiers(lMod);

			Debug.Assert(GetActiveKeyModifiers().Count == 0);
			return lMod.Count;
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
			if(IsKeyActive(vKey)) lList.Add(vKey);
		}

		private static bool IsKeyActive(int vKey)
		{
			if(vKey == NativeMethods.VK_CAPITAL)
			{
				ushort usCap = NativeMethods.GetKeyState(vKey);
				return ((usCap & 1) != 0);
			}

			ushort usState = NativeMethods.GetAsyncKeyState(vKey);
			return ((usState & 0x8000) != 0);

			// For GetKeyState:
			// if(vKey == NativeMethods.VK_CAPITAL)
			//	return ((usState & 1) != 0);
			// else
			//	return ((usState & 0x8000) != 0);
		}

		private static void ActivateKeyModifiers(List<int> vKeys, bool bDown)
		{
			Debug.Assert(vKeys != null);
			if(vKeys == null) throw new ArgumentNullException("vKeys");

			foreach(int vKey in vKeys)
			{
				if(vKey == NativeMethods.VK_CAPITAL) // Toggle
				{
					SendVKeyNative(vKey, null, true);
					SendVKeyNative(vKey, null, false);
				}
				else SendVKeyNative(vKey, null, bDown);
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

		private static char[] m_vForcedChars = null;
		private bool TrySendCharByKeypresses(char ch, bool? bDown)
		{
			if(ch == char.MinValue) { Debug.Assert(false); return false; }
			if(m_bForceUnicodeChars) return false;

			if(m_vForcedChars == null)
				m_vForcedChars = new char[] {
					// All of the following diacritics are spacing / non-combining
					'\u00B4', // Acute accent
					'\u02DD', // Double acute accent
					'\u0060', // Grave accent
					'\u02D8', // Breve
					'\u00B8', // Cedilla
					'\u005E', // Circumflex ^
					'\u00A8', // Diaeresis
					'\u02D9', // Dot above
					'\u00AF', // Macron above, long
					'\u02C9', // Macron above, modifier, short
					'\u02CD', // Macron below, modifier, short
					'\u02DB' // Ogonek
				};
			if(Array.IndexOf<char>(m_vForcedChars, ch) >= 0) return false;

			IntPtr hWnd = NativeMethods.GetForegroundWindowHandle();
			if(hWnd == IntPtr.Zero) { Debug.Assert(false); return false; }

			uint uTargetProcessID;
			uint uTargetThreadID = NativeMethods.GetWindowThreadProcessId(
				hWnd, out uTargetProcessID);

			IntPtr hKL = NativeMethods.GetKeyboardLayout(uTargetThreadID);

			ushort u = ((hKL == IntPtr.Zero) ? NativeMethods.VkKeyScan(ch) :
				NativeMethods.VkKeyScanEx(ch, hKL));
			if(u == 0xFFFFU) return false;

			int vKey = (int)(u & 0xFFU);

			Keys kMod = Keys.None;
			if((u & 0x100U) != 0U) kMod |= Keys.Shift;
			if((u & 0x200U) != 0U) kMod |= Keys.Control;
			if((u & 0x400U) != 0U) kMod |= Keys.Alt;
			if((u & 0x800U) != 0U) return false; // Hankaku unsupported

			Keys kModDiff = (kMod & ~m_kModCur);
			if(kModDiff != Keys.None)
			{
				SetKeyModifierImpl(kModDiff, true);

				Thread.Sleep(1);
				Application.DoEvents();
			}

			SendKeyImpl(vKey, null, bDown);

			if(kModDiff != Keys.None)
			{
				Thread.Sleep(1);
				Application.DoEvents();

				SetKeyModifierImpl(kModDiff, false);
			}

			return true;
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
