/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2025 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Text;

using KeePass.Native;

using KeePassLib.Utility;

namespace KeePass.Util.SendInputExt
{
	internal enum SiSendMethod
	{
		Default = 0,
		KeyEvent,
		UnicodePacket // VK_PACKET via SendInput
	}

	internal sealed class SiWindowInfo
	{
		private static string[] g_vProcessNamesUni = null;
		private static string[] g_vProcessNamesVMs = null;

		private readonly IntPtr m_hWnd;
		public IntPtr HWnd
		{
			get { return m_hWnd; }
		}

		private IntPtr m_hKL = IntPtr.Zero;
		public IntPtr KeyboardLayout
		{
			get { return m_hKL; }
		}

		private SiSendMethod m_sm = SiSendMethod.Default;
		public SiSendMethod SendMethod
		{
			get { return m_sm; }
		}

		private bool m_bCharsRAltAsCtrlAlt = false;
		/// <summary>
		/// Specifies whether characters that are realized with Ctrl+Alt
		/// should be sent with RAlt (AltGr) only.
		/// On some Linux systems (in a virtual machine window), RAlt is
		/// different from Ctrl+RAlt, whereas on Windows, there are
		/// keyboard layouts that require Ctrl+RAlt;
		/// https://sourceforge.net/p/keepass/bugs/1857/
		/// As we do not know what is running within a virtual machine
		/// window, we use RAlt for such windows and Ctrl+RAlt for all
		/// other windows.
		/// </summary>
		public bool CharsRAltAsCtrlAlt
		{
			get { return m_bCharsRAltAsCtrlAlt; }
		}

		private int m_msSleepAroundKeyMod = 1;
		public int SleepAroundKeyMod
		{
			get { return m_msSleepAroundKeyMod; }
		}

		public SiWindowInfo(IntPtr hWnd)
		{
			m_hWnd = hWnd;

			Init();
		}

		private void Init()
		{
			if(m_hWnd == IntPtr.Zero) return; // No assert

			Process p = null;
			try
			{
				uint uPID;
				uint uTID = NativeMethods.GetWindowThreadProcessId(m_hWnd, out uPID);

				m_hKL = NativeMethods.GetKeyboardLayout(uTID);

				p = Process.GetProcessById((int)uPID);

				string strName = GetProcessName(p);
				InitByProcessName(strName);

				// The workaround attempt for Edge below doesn't work;
				// Edge simply ignores Unicode packets for '@', Euro sign, etc.
				/* if(m_sm == SiSendMethod.Default)
				{
					string strTitle = NativeMethods.GetWindowText(m_hWnd, true);

					// Workaround for Edge;
					// https://sourceforge.net/p/keepass/discussion/329220/thread/fd3a6776/
					// The window title is:
					// Page name + Space + U+200E (left-to-right mark) + "- Microsoft Edge"
					if(strTitle.EndsWith("- Microsoft Edge", StrUtil.CaseIgnoreCmp))
						m_sm = SiSendMethod.UnicodePacket;
				} */

#if DEBUG
				Trace.WriteLine("SiWindowInfo constructed: process '" + strName +
					"', send method '" + m_sm.ToString() + "', CharsRAltAsCtrlAlt '" +
					m_bCharsRAltAsCtrlAlt.ToString() + "'.");
#endif
			}
			catch(Exception) { Debug.Assert(false); }
			finally
			{
				try { if(p != null) p.Dispose(); }
				catch(Exception) { Debug.Assert(false); }
			}
		}

		private void InitByProcessName(string strName)
		{
			if(g_vProcessNamesUni == null)
				g_vProcessNamesUni = new string[] {
					"PuTTY",
					"KiTTY", "KiTTY_Portable", "KiTTY_NoTrans",
					"KiTTY_NoHyperlink", "KiTTY_NoCompress",
					"PuTTYjp",
					// "mRemoteNG", // No effect
					// "PuTTYNG", // No effect

					// SuperPuTTY spawns PuTTY processes whose windows are
					// displayed embedded in the SuperPuTTY window (without
					// window borders), thus the definition "PuTTY" also
					// fixes SuperPuTTY; no "SuperPuTTY" required
					// "SuperPuTTY",

					"MinTTY" // Cygwin window "~"
				};
			foreach(string str in g_vProcessNamesUni)
			{
				if(ProcessNameMatches(strName, str))
				{
					m_sm = SiSendMethod.UnicodePacket;
					return;
				}
			}

			if(g_vProcessNamesVMs == null)
				g_vProcessNamesVMs = new string[] {
					"MSTSC", // Remote Desktop Connection client (Windows)
					"MSRDC", // Remote Desktop client (WSL)
					"VirtualBox", // Oracle VirtualBox <= 5
					"VirtualBoxVM", // Oracle VirtualBox >= 6
					"VpxClient", // VMware vSphere client

					// VMware Player;
					// https://sourceforge.net/p/keepass/discussion/329221/thread/c94e0f096e/
					"VMware-VMX", "VMware-AuthD", "VMPlayer", "VMware-Unity-Helper",

					// VMware Workstation;
					// https://sourceforge.net/p/keepass/bugs/1588/
					"VMware",

					// VMware Remote Console;
					// https://sourceforge.net/p/keepass/bugs/1588/
					"VMRC",

					// VMware Horizon Client;
					// https://sourceforge.net/p/keepass/discussion/329221/thread/f9c025af4b/
					"VMware-View",

					// Dameware Mini Remote Control;
					// https://sourceforge.net/p/keepass/bugs/1874/
					"DWRCC",

					// Kaseya Live Connect;
					// https://sourceforge.net/p/keepass/discussion/329220/thread/85c109edb6/
					"KaseyaLiveConnect"
				};
			foreach(string str in g_vProcessNamesVMs)
			{
				if(ProcessNameMatches(strName, str))
				{
					m_sm = SiSendMethod.KeyEvent;
					m_bCharsRAltAsCtrlAlt = true;

					// Determined with Fedora 31 in VirtualBox 6.1
					// (running 'stress', {DELAY=1})
					m_msSleepAroundKeyMod = 50;

					return;
				}
			}
		}

		internal static string GetProcessName(Process p)
		{
			if(p == null) { Debug.Assert(false); return string.Empty; }

			try { return (p.ProcessName ?? string.Empty).Trim(); }
			catch(Exception) { Debug.Assert(false); }
			return string.Empty;
		}

		internal static bool ProcessNameMatches(string strUnk, string strPattern)
		{
			if(strUnk == null) { Debug.Assert(false); return false; }
			if(strPattern == null) { Debug.Assert(false); return false; }
			Debug.Assert(strUnk.Trim() == strUnk);
			Debug.Assert(strPattern.Trim() == strPattern);
			Debug.Assert(!strPattern.EndsWith(".exe", StrUtil.CaseIgnoreCmp));

			return (strUnk.Equals(strPattern, StrUtil.CaseIgnoreCmp) ||
				strUnk.Equals(strPattern + ".exe", StrUtil.CaseIgnoreCmp));
		}
	}
}
