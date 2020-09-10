/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2020 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace KeePass.Util.SendInputExt
{
	internal sealed class SiCode
	{
		public readonly string Code;
		public readonly int VKey;
		public readonly bool? ExtKey = null; // Currently all have default ext state
		public readonly string XKeySym;

		public SiCode(string strCode, int iVKey, string strXKeySym)
		{
			if(string.IsNullOrEmpty(strCode)) { Debug.Assert(false); strCode = " "; }

			this.Code = strCode;
			this.VKey = iVKey;
			this.XKeySym = strXKeySym;
		}

		public SiCode(string strCode, Keys k, string strXKeySym)
		{
			if(string.IsNullOrEmpty(strCode)) { Debug.Assert(false); strCode = " "; }

			this.Code = strCode;
			this.VKey = (int)k;
			this.XKeySym = strXKeySym;
		}
	}

	internal enum SiDiacritic
	{
		None = 0,
		Grave,
		Acute,
		Circumflex,
		Tilde,
		Diaeresis, // Umlaut
		Ring, // Ring above
		Cedilla,
		Macron,
		Breve,
		Ogonek,
		DotAbove,
		Caron,
		AcuteDouble
	}

	internal static class SiCodes
	{
		private static List<SiCode> m_l = null;
		public static List<SiCode> KeyCodes
		{
			get
			{
				if(m_l != null) return m_l;

				List<SiCode> l = new List<SiCode>();

				// XKeySym codes from 'keysymdef.h'

				l.Add(new SiCode("BACKSPACE", Keys.Back, "BackSpace"));
				l.Add(new SiCode("BKSP", Keys.Back, "BackSpace"));
				l.Add(new SiCode("BS", Keys.Back, "BackSpace"));
				l.Add(new SiCode("BREAK", Keys.Cancel, "Cancel")); // See SendKeys class
				l.Add(new SiCode("CAPSLOCK", Keys.CapsLock, "Caps_Lock"));
				l.Add(new SiCode("CLEAR", Keys.Clear, "Clear"));
				l.Add(new SiCode("DEL", Keys.Delete, "Delete"));
				l.Add(new SiCode("DELETE", Keys.Delete, "Delete"));
				l.Add(new SiCode("END", Keys.End, "End"));
				l.Add(new SiCode("ENTER", Keys.Enter, "Return"));
				l.Add(new SiCode("ESC", Keys.Escape, "Escape"));
				l.Add(new SiCode("ESCAPE", Keys.Escape, "Escape"));
				l.Add(new SiCode("HELP", Keys.Help, "Help"));
				l.Add(new SiCode("HOME", Keys.Home, "Home"));
				l.Add(new SiCode("INS", Keys.Insert, "Insert"));
				l.Add(new SiCode("INSERT", Keys.Insert, "Insert"));
				l.Add(new SiCode("NUMLOCK", Keys.NumLock, "Num_Lock"));
				l.Add(new SiCode("PGDN", Keys.PageDown, "Page_Down"));
				l.Add(new SiCode("PGUP", Keys.PageUp, "Page_Up"));
				l.Add(new SiCode("PRTSC", Keys.PrintScreen, "Print"));
				l.Add(new SiCode("SCROLLLOCK", Keys.Scroll, "Scroll_Lock"));
				l.Add(new SiCode("SPACE", Keys.Space, "space"));
				l.Add(new SiCode("TAB", Keys.Tab, "Tab"));

				l.Add(new SiCode("UP", Keys.Up, "Up"));
				l.Add(new SiCode("DOWN", Keys.Down, "Down"));
				l.Add(new SiCode("LEFT", Keys.Left, "Left"));
				l.Add(new SiCode("RIGHT", Keys.Right, "Right"));

				for(int i = 1; i <= 24; ++i)
				{
					string strF = "F" + i.ToString(NumberFormatInfo.InvariantInfo);
					l.Add(new SiCode(strF, (int)Keys.F1 + i - 1, strF));

					Debug.Assert(Enum.IsDefined(typeof(Keys), (int)Keys.F1 + i - 1) &&
						Enum.GetName(typeof(Keys), (int)Keys.F1 + i - 1) == strF);
				}

				l.Add(new SiCode("ADD", Keys.Add, "KP_Add"));
				l.Add(new SiCode("SUBTRACT", Keys.Subtract, "KP_Subtract"));
				l.Add(new SiCode("MULTIPLY", Keys.Multiply, "KP_Multiply"));
				l.Add(new SiCode("DIVIDE", Keys.Divide, "KP_Divide"));

				for(int i = 0; i < 10; ++i)
				{
					string strI = i.ToString(NumberFormatInfo.InvariantInfo);
					l.Add(new SiCode("NUMPAD" + strI, (int)Keys.NumPad0 + i,
						"KP_" + strI));

					Debug.Assert(((int)Keys.NumPad9 - (int)Keys.NumPad0) == 9);
				}

				l.Add(new SiCode("WIN", Keys.LWin, "Super_L"));
				l.Add(new SiCode("LWIN", Keys.LWin, "Super_L"));
				l.Add(new SiCode("RWIN", Keys.RWin, "Super_R"));
				l.Add(new SiCode("APPS", Keys.Apps, "Menu"));

#if DEBUG
				foreach(SiCode si in l)
				{
					// All key codes must be upper-case (for 'Get' method)
					Debug.Assert(si.Code == si.Code.ToUpperInvariant());
				}
#endif

				m_l = l;
				return l;
			}
		}

		private static Dictionary<string, SiCode> m_dictS = null;
		public static SiCode Get(string strCode)
		{
			if(strCode == null) { Debug.Assert(false); return null; }

			if(m_dictS == null)
			{
				Dictionary<string, SiCode> d = new Dictionary<string, SiCode>();
				foreach(SiCode siCp in SiCodes.KeyCodes)
				{
					d[siCp.Code] = siCp;
				}
				Debug.Assert(d.Count == SiCodes.KeyCodes.Count);

				m_dictS = d;
			}

			string strUp = strCode.ToUpperInvariant();
			SiCode si;
			if(m_dictS.TryGetValue(strUp, out si)) return si;
			return null;
		}

		public static SiCode Get(int iVKey, bool? obExtKey)
		{
			foreach(SiCode si in SiCodes.KeyCodes)
			{
				if(si.VKey == iVKey)
				{
					if(!si.ExtKey.HasValue || !obExtKey.HasValue) return si;
					if(si.ExtKey.Value == obExtKey.Value) return si;
				}
			}

			return null;
		}

		// Characters that should be converted to VKeys in special
		// situations (e.g. when a key modifier is active)
		private static Dictionary<char, int> m_dChToVk = null;

		// Characters that should always be converted to VKeys
		// (independent of e.g. whether key modifier is active or not)
		private static Dictionary<char, int> m_dChToVkAlways = null;

		private static void EnsureChToVkDicts()
		{
			if(m_dChToVk != null) return;

			Dictionary<char, int> d = new Dictionary<char, int>();

			// The following characters should *always* be converted
			d['\u0008'] = (int)Keys.Back;
			d['\t'] = (int)Keys.Tab;
			d['\n'] = (int)Keys.LineFeed;
			d['\r'] = (int)Keys.Return;
			d['\u001B'] = (int)Keys.Escape;
			d[' '] = (int)Keys.Space; // For toggling checkboxes
			d['\u007F'] = (int)Keys.Delete; // Different values

			m_dChToVkAlways = new Dictionary<char, int>(d);

			Debug.Assert((int)Keys.D0 == (int)'0');
			for(char ch = '0'; ch <= '9'; ++ch)
				d[ch] = (int)ch - (int)'0' + (int)Keys.D0;
			Debug.Assert(d['9'] == (int)Keys.D9);

			Debug.Assert((int)Keys.A == (int)'A');
			// Do not translate upper-case letters;
			// on Windows, sending VK_A results in 'a', and 'A' with Shift;
			// on some Linux systems, only Ctrl+'v' pastes, not Ctrl+'V':
			// https://sourceforge.net/p/keepass/discussion/329220/thread/bce61102/
			// for(char ch = 'A'; ch <= 'Z'; ++ch)
			//	d[ch] = (int)ch - (int)'A' + (int)Keys.A;
			for(char ch = 'a'; ch <= 'z'; ++ch)
				d[ch] = (int)ch - (int)'a' + (int)Keys.A;
			Debug.Assert(d['z'] == (int)Keys.Z);

			m_dChToVk = d;
			Debug.Assert(m_dChToVk.Count > m_dChToVkAlways.Count); // Independency
		}

		public static int CharToVKey(char ch, bool bLightConv)
		{
			EnsureChToVkDicts();

			Dictionary<char, int> d = (bLightConv ? m_dChToVkAlways : m_dChToVk);

			int iVKey;
			if(d.TryGetValue(ch, out iVKey)) return iVKey;
			return 0;
		}

		public static char VKeyToChar(int iVKey)
		{
			EnsureChToVkDicts();

			foreach(KeyValuePair<char, int> kvp in m_dChToVk)
			{
				if(kvp.Value == iVKey) return kvp.Key;
			}

			return char.MinValue;
		}

		private static Dictionary<char, string> m_dChToXKeySym = null;
		public static string CharToXKeySym(char ch)
		{
			if(m_dChToXKeySym == null)
			{
				Dictionary<char, string> d = new Dictionary<char, string>();

				// XDoTool sends some characters in the wrong case;
				// a workaround is to specify the keypress as an
				// XKeySym combination;
				// https://sourceforge.net/p/keepass/bugs/1532/
				// https://github.com/jordansissel/xdotool/issues/41

				AddDiacritic(d, 'A', '\u00C0', '\u00E0', SiDiacritic.Grave);
				AddDiacritic(d, 'A', '\u00C1', '\u00E1', SiDiacritic.Acute);
				AddDiacritic(d, 'A', '\u00C2', '\u00E2', SiDiacritic.Circumflex);
				AddDiacritic(d, 'A', '\u00C3', '\u00E3', SiDiacritic.Tilde);
				AddDiacritic(d, 'A', '\u00C4', '\u00E4', SiDiacritic.Diaeresis);
				AddDiacritic(d, 'A', '\u00C5', '\u00E5', SiDiacritic.Ring);

				AddDiacritic(d, 'C', '\u00C7', '\u00E7', SiDiacritic.Cedilla);

				AddDiacritic(d, 'E', '\u00C8', '\u00E8', SiDiacritic.Grave);
				AddDiacritic(d, 'E', '\u00C9', '\u00E9', SiDiacritic.Acute);
				AddDiacritic(d, 'E', '\u00CA', '\u00EA', SiDiacritic.Circumflex);
				AddDiacritic(d, 'E', '\u00CB', '\u00EB', SiDiacritic.Diaeresis);

				AddDiacritic(d, 'I', '\u00CC', '\u00EC', SiDiacritic.Grave);
				AddDiacritic(d, 'I', '\u00CD', '\u00ED', SiDiacritic.Acute);
				AddDiacritic(d, 'I', '\u00CE', '\u00EE', SiDiacritic.Circumflex);
				AddDiacritic(d, 'I', '\u00CF', '\u00EF', SiDiacritic.Diaeresis);

				AddDiacritic(d, 'N', '\u00D1', '\u00F1', SiDiacritic.Tilde);

				AddDiacritic(d, 'O', '\u00D2', '\u00F2', SiDiacritic.Grave);
				AddDiacritic(d, 'O', '\u00D3', '\u00F3', SiDiacritic.Acute);
				AddDiacritic(d, 'O', '\u00D4', '\u00F4', SiDiacritic.Circumflex);
				AddDiacritic(d, 'O', '\u00D5', '\u00F5', SiDiacritic.Tilde);
				AddDiacritic(d, 'O', '\u00D6', '\u00F6', SiDiacritic.Diaeresis);

				AddDiacritic(d, 'U', '\u00D9', '\u00F9', SiDiacritic.Grave);
				AddDiacritic(d, 'U', '\u00DA', '\u00FA', SiDiacritic.Acute);
				AddDiacritic(d, 'U', '\u00DB', '\u00FB', SiDiacritic.Circumflex);
				AddDiacritic(d, 'U', '\u00DC', '\u00FC', SiDiacritic.Diaeresis);

				AddDiacritic(d, 'Y', '\u00DD', '\u00FD', SiDiacritic.Acute);
				AddDiacritic(d, 'Y', '\u0178', '\u00FF', SiDiacritic.Diaeresis);

				AddDiacritic(d, 'A', '\u0100', '\u0101', SiDiacritic.Macron);
				AddDiacritic(d, 'A', '\u0102', '\u0103', SiDiacritic.Breve);
				AddDiacritic(d, 'A', '\u0104', '\u0105', SiDiacritic.Ogonek);

				AddDiacritic(d, 'C', '\u0106', '\u0107', SiDiacritic.Acute);
				AddDiacritic(d, 'C', '\u0108', '\u0109', SiDiacritic.Circumflex);
				AddDiacritic(d, 'C', '\u010A', '\u010B', SiDiacritic.DotAbove);
				AddDiacritic(d, 'C', '\u010C', '\u010D', SiDiacritic.Caron);

				AddDiacritic(d, 'D', '\u010E', '\u010F', SiDiacritic.Caron);

				AddDiacritic(d, 'E', '\u0112', '\u0113', SiDiacritic.Macron);
				AddDiacritic(d, 'E', '\u0114', '\u0115', SiDiacritic.Breve);
				AddDiacritic(d, 'E', '\u0116', '\u0117', SiDiacritic.DotAbove);
				AddDiacritic(d, 'E', '\u0118', '\u0119', SiDiacritic.Ogonek);
				AddDiacritic(d, 'E', '\u011A', '\u011B', SiDiacritic.Caron);

				AddDiacritic(d, 'G', '\u011C', '\u011D', SiDiacritic.Circumflex);
				AddDiacritic(d, 'G', '\u011E', '\u011F', SiDiacritic.Breve);
				AddDiacritic(d, 'G', '\u0120', '\u0121', SiDiacritic.DotAbove);
				AddDiacritic(d, 'G', '\u0122', '\u0123', SiDiacritic.Cedilla);

				AddDiacritic(d, 'H', '\u0124', '\u0125', SiDiacritic.Circumflex);

				AddDiacritic(d, 'I', '\u0128', '\u0129', SiDiacritic.Tilde);
				AddDiacritic(d, 'I', '\u012A', '\u012B', SiDiacritic.Macron);
				AddDiacritic(d, 'I', '\u012C', '\u012D', SiDiacritic.Breve);
				AddDiacritic(d, 'I', '\u012E', '\u012F', SiDiacritic.Ogonek);
				AddDiacritic(d, 'I', '\u0130', '\0', SiDiacritic.DotAbove);

				AddDiacritic(d, 'J', '\u0134', '\u0135', SiDiacritic.Circumflex);

				AddDiacritic(d, 'K', '\u0136', '\u0137', SiDiacritic.Cedilla);

				AddDiacritic(d, 'L', '\u0139', '\u013A', SiDiacritic.Acute);
				AddDiacritic(d, 'L', '\u013B', '\u013C', SiDiacritic.Cedilla);
				AddDiacritic(d, 'L', '\u013D', '\u013E', SiDiacritic.Caron);

				AddDiacritic(d, 'N', '\u0143', '\u0144', SiDiacritic.Acute);
				AddDiacritic(d, 'N', '\u0145', '\u0146', SiDiacritic.Cedilla);
				AddDiacritic(d, 'N', '\u0147', '\u0148', SiDiacritic.Caron);

				AddDiacritic(d, 'O', '\u014C', '\u014D', SiDiacritic.Macron);
				AddDiacritic(d, 'O', '\u014E', '\u014F', SiDiacritic.Breve);
				AddDiacritic(d, 'O', '\u0150', '\u0151', SiDiacritic.AcuteDouble);

				AddDiacritic(d, 'R', '\u0154', '\u0155', SiDiacritic.Acute);
				AddDiacritic(d, 'R', '\u0156', '\u0157', SiDiacritic.Cedilla);
				AddDiacritic(d, 'R', '\u0158', '\u0159', SiDiacritic.Caron);

				AddDiacritic(d, 'S', '\u015A', '\u015B', SiDiacritic.Acute);
				AddDiacritic(d, 'S', '\u015C', '\u015D', SiDiacritic.Circumflex);
				AddDiacritic(d, 'S', '\u015E', '\u015F', SiDiacritic.Cedilla);
				AddDiacritic(d, 'S', '\u0160', '\u0161', SiDiacritic.Caron);

				AddDiacritic(d, 'T', '\u0162', '\u0163', SiDiacritic.Cedilla);
				AddDiacritic(d, 'T', '\u0164', '\u0165', SiDiacritic.Caron);

				AddDiacritic(d, 'U', '\u0168', '\u0169', SiDiacritic.Tilde);
				AddDiacritic(d, 'U', '\u016A', '\u016B', SiDiacritic.Macron);
				AddDiacritic(d, 'U', '\u016C', '\u016D', SiDiacritic.Breve);
				AddDiacritic(d, 'U', '\u016E', '\u016F', SiDiacritic.Ring);
				AddDiacritic(d, 'U', '\u0170', '\u0171', SiDiacritic.AcuteDouble);
				AddDiacritic(d, 'U', '\u0172', '\u0173', SiDiacritic.Ogonek);

				AddDiacritic(d, 'W', '\u0174', '\u0175', SiDiacritic.Circumflex);

				AddDiacritic(d, 'Y', '\u0176', '\u0177', SiDiacritic.Circumflex);

				AddDiacritic(d, 'Z', '\u0179', '\u017A', SiDiacritic.Acute);
				AddDiacritic(d, 'Z', '\u017B', '\u017C', SiDiacritic.DotAbove);
				AddDiacritic(d, 'Z', '\u017D', '\u017E', SiDiacritic.Caron);

				m_dChToXKeySym = d;
			}

			string str;
			if(m_dChToXKeySym.TryGetValue(ch, out str)) return str;

			// Unicode is supported; codes are 'UHHHH' with 'HHHH' being
			// the Unicode value; see header of 'keysymdef.h'
			return ("U" + ((int)ch).ToString("X4", NumberFormatInfo.InvariantInfo));
		}

		private static void AddDiacritic(Dictionary<char, string> d,
			char chBase, char chDiaU, char chDiaL, SiDiacritic dc)
		{
			if(d == null) { Debug.Assert(false); return; }

			string strPrefix = string.Empty;
			switch(dc)
			{
				case SiDiacritic.Grave:
					strPrefix = "dead_grave ";
					break;
				case SiDiacritic.Acute:
					strPrefix = "dead_acute ";
					break;
				case SiDiacritic.Circumflex:
					strPrefix = "dead_circumflex ";
					break;
				case SiDiacritic.Tilde:
					strPrefix = "dead_tilde ";
					break;
				case SiDiacritic.Diaeresis:
					strPrefix = "dead_diaeresis ";
					break;
				case SiDiacritic.Ring:
					strPrefix = "dead_abovering ";
					break;
				case SiDiacritic.Cedilla:
					strPrefix = "dead_cedilla ";
					break;
				case SiDiacritic.Macron:
					strPrefix = "dead_macron ";
					break;
				case SiDiacritic.Breve:
					strPrefix = "dead_breve ";
					break;
				case SiDiacritic.Ogonek:
					strPrefix = "dead_ogonek ";
					break;
				case SiDiacritic.DotAbove:
					strPrefix = "dead_abovedot ";
					break;
				case SiDiacritic.Caron:
					strPrefix = "dead_caron ";
					break;
				case SiDiacritic.AcuteDouble:
					strPrefix = "dead_doubleacute ";
					break;
				default:
					Debug.Assert(dc == SiDiacritic.None);
					break;
			}
			Debug.Assert((strPrefix.Length == 0) || strPrefix.EndsWith(" "));

			if(chDiaU != '\0')
			{
				Debug.Assert(!d.ContainsKey(chDiaU));
				d[chDiaU] = (strPrefix + char.ToUpperInvariant(chBase));
			}
			if(chDiaL != '\0')
			{
				Debug.Assert(!d.ContainsKey(chDiaL));
				d[chDiaL] = (strPrefix + char.ToLowerInvariant(chBase));
			}
		}
	}
}
