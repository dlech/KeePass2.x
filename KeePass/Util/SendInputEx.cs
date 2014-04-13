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
using System.Diagnostics;

using KeePass.Resources;
using KeePass.Util.SendInputExt;

using KeePassLib.Utility;

namespace KeePass.Util
{
	internal enum SiEventType
	{
		None = 0,
		Key,
		KeyModifier,
		Char,
		Delay,
		SetDefaultDelay,
		ClipboardCopy
	}

	internal sealed class SiEvent
	{
		public SiEventType Type = SiEventType.None;

		public int VKey = 0;
		public bool? ExtendedKey = null;

		public Keys KeyModifier = Keys.None;

		public char Char = char.MinValue;

		public bool? Down = null;

		public uint Delay = 0;

		public string Text = null;

#if DEBUG
		// For debugger display
		public override string ToString()
		{
			string str = Enum.GetName(typeof(SiEventType), this.Type);

			string strSub = null;
			switch(this.Type)
			{
				case SiEventType.Key:
					strSub = this.VKey.ToString() + " " + this.ExtendedKey.ToString() +
						" " + this.Down.ToString();
					break;
				case SiEventType.KeyModifier:
					strSub = this.KeyModifier.ToString() + " " + this.Down.ToString();
					break;
				case SiEventType.Char:
					strSub = this.Char.ToString() + " " + this.Down.ToString();
					break;
				case SiEventType.Delay:
				case SiEventType.SetDefaultDelay:
					strSub = this.Delay.ToString();
					break;
				case SiEventType.ClipboardCopy:
					strSub = this.Text;
					break;
				default: break;
			}

			if(!string.IsNullOrEmpty(strSub)) return (str + ": " + strSub);
			return str;
		}
#endif
	}

	public static class SendInputEx
	{
		private static CriticalSectionEx m_csSending = new CriticalSectionEx();

		public static void SendKeysWait(string strKeys, bool bObfuscate)
		{
			if(strKeys == null) { Debug.Assert(false); return; }

			List<SiEvent> l = Parse(strKeys);
			if(l.Count == 0) return;

			if(bObfuscate) SiObf.Obfuscate(l);

			FixEventSeq(l);

			bool bUnix = KeePassLib.Native.NativeLib.IsUnix();
			ISiEngine si;
			if(bUnix) si = new SiEngineUnix();
			else si = new SiEngineWin();

			bool bInter = Program.Config.Integration.AutoTypeAllowInterleaved;
			if(!bInter)
			{
				if(!m_csSending.TryEnter()) return;
			}

			try
			{
				si.Init();
				Send(si, l);
			}
			finally
			{
				try { si.Release(); }
				catch(Exception) { Debug.Assert(false); }

				if(!bInter) m_csSending.Exit();
			}
		}

		private static List<SiEvent> Parse(string strSequence)
		{
			CharStream cs = new CharStream(strSequence);
			List<SiEvent> l = new List<SiEvent>();
			string strError = KPRes.AutoTypeSequenceInvalid;

			Keys kCurKbMods = Keys.None;

			List<Keys> lMods = new List<Keys>();
			lMods.Add(Keys.None);

			while(true)
			{
				char ch = cs.ReadChar();
				if(ch == char.MinValue) break;

				if((ch == '+') || (ch == '^') || (ch == '%'))
				{
					if(lMods.Count == 0) { Debug.Assert(false); break; }
					else if(ch == '+') lMods[lMods.Count - 1] |= Keys.Shift;
					else if(ch == '^') lMods[lMods.Count - 1] |= Keys.Control;
					else if(ch == '%') lMods[lMods.Count - 1] |= Keys.Alt;
					else { Debug.Assert(false); }

					continue;
				}
				else if(ch == '(')
				{
					lMods.Add(Keys.None);
					continue;
				}
				else if(ch == ')')
				{
					if(lMods.Count >= 2)
					{
						lMods.RemoveAt(lMods.Count - 1);
						lMods[lMods.Count - 1] = Keys.None;
					}
					else throw new FormatException(strError);

					continue;
				}

				Keys kEffMods = Keys.None;
				foreach(Keys k in lMods) kEffMods |= k;

				EnsureKeyModifiers(kEffMods, ref kCurKbMods, l);

				if(ch == '{')
				{
					List<SiEvent> lSub = ParseSpecial(cs);
					if(lSub == null) throw new FormatException(strError);

					l.AddRange(lSub);
				}
				else if(ch == '}')
					throw new FormatException(strError);
				else if(ch == '~')
				{
					SiEvent si = new SiEvent();
					si.Type = SiEventType.Key;
					si.VKey = (int)Keys.Enter;

					l.Add(si);
				}
				else
				{
					SiEvent si = new SiEvent();
					si.Type = SiEventType.Char;
					si.Char = ch;

					l.Add(si);
				}

				lMods[lMods.Count - 1] = Keys.None;
			}

			EnsureKeyModifiers(Keys.None, ref kCurKbMods, l);

			return l;
		}

		private static void EnsureKeyModifiers(Keys kReqMods, ref Keys kCurKbMods,
			List<SiEvent> l)
		{
			if(kReqMods == kCurKbMods) return;

			if((kReqMods & Keys.Shift) != (kCurKbMods & Keys.Shift))
			{
				SiEvent si = new SiEvent();
				si.Type = SiEventType.KeyModifier;
				si.KeyModifier = Keys.Shift;
				si.Down = ((kReqMods & Keys.Shift) != Keys.None);

				l.Add(si);
			}

			if((kReqMods & Keys.Control) != (kCurKbMods & Keys.Control))
			{
				SiEvent si = new SiEvent();
				si.Type = SiEventType.KeyModifier;
				si.KeyModifier = Keys.Control;
				si.Down = ((kReqMods & Keys.Control) != Keys.None);

				l.Add(si);
			}

			if((kReqMods & Keys.Alt) != (kCurKbMods & Keys.Alt))
			{
				SiEvent si = new SiEvent();
				si.Type = SiEventType.KeyModifier;
				si.KeyModifier = Keys.Alt;
				si.Down = ((kReqMods & Keys.Alt) != Keys.None);

				l.Add(si);
			}

			kCurKbMods = kReqMods;
		}

		private static List<SiEvent> ParseSpecial(CharStream cs)
		{
			// Skip leading white space
			while(true)
			{
				char ch = cs.PeekChar();
				if(ch == char.MinValue) { Debug.Assert(false); return null; }

				if(!char.IsWhiteSpace(ch)) break;
				cs.ReadChar();
			}

			// First char is *always* part of the name (support for "{{}", etc.)
			char chFirst = cs.ReadChar();
			if(chFirst == char.MinValue) { Debug.Assert(false); return null; }

			int iPart = 0;
			string strName = string.Empty, strParam = string.Empty;

			StringBuilder sb = new StringBuilder();
			sb.Append(chFirst);

			bool bParsed = false;
			while(!bParsed)
			{
				char ch = cs.ReadChar();
				if(ch == char.MinValue) { Debug.Assert(false); return null; }

				if(ch == '}')
				{
					ch = ' '; // Finish current part
					bParsed = true;
				}

				if(char.IsWhiteSpace(ch))
				{
					if(iPart == 0)
					{
						strName = sb.ToString();
						sb.Remove(0, sb.Length);
						++iPart;
					}
					else if((iPart == 1) && (sb.Length > 0))
					{
						strParam = sb.ToString();
						sb.Remove(0, sb.Length);
						++iPart;
					}
				}
				else sb.Append(ch);
			}

			uint? uParam = null;
			if(strParam.Length > 0)
			{
				uint uParamTry;
				if(uint.TryParse(strParam, out uParamTry)) uParam = uParamTry;
			}

			List<SiEvent> l = new List<SiEvent>();

			if(strName.Equals("DELAY", StrUtil.CaseIgnoreCmp))
			{
				if(!uParam.HasValue) { Debug.Assert(false); return null; }

				SiEvent si = new SiEvent();
				si.Type = SiEventType.Delay;
				si.Delay = uParam.Value;

				l.Add(si);
				return l;
			}
			else if(strName.StartsWith("DELAY=", StrUtil.CaseIgnoreCmp))
			{
				SiEvent si = new SiEvent();
				si.Type = SiEventType.SetDefaultDelay;

				string strDelay = strName.Substring(6).Trim();
				uint uDelay;
				if(uint.TryParse(strDelay, out uDelay))
					si.Delay = uDelay;
				else { Debug.Assert(false); return null; }

				l.Add(si);
				return l;
			}
			else if(strName.Equals("VKEY", StrUtil.CaseIgnoreCmp))
			{
				if(!uParam.HasValue) { Debug.Assert(false); return null; }

				SiEvent si = new SiEvent();
				si.Type = SiEventType.Key;
				si.VKey = (int)uParam.Value;

				l.Add(si);
				return l;
			}
			else if(strName.Equals("VKEY-NX", StrUtil.CaseIgnoreCmp))
			{
				if(!uParam.HasValue) { Debug.Assert(false); return null; }

				SiEvent si = new SiEvent();
				si.Type = SiEventType.Key;
				si.VKey = (int)uParam.Value;
				si.ExtendedKey = false;

				l.Add(si);
				return l;
			}
			else if(strName.Equals("VKEY-EX", StrUtil.CaseIgnoreCmp))
			{
				if(!uParam.HasValue) { Debug.Assert(false); return null; }

				SiEvent si = new SiEvent();
				si.Type = SiEventType.Key;
				si.VKey = (int)uParam.Value;
				si.ExtendedKey = true;

				l.Add(si);
				return l;
			}

			SiCode siCode = SiCodes.Get(strName);

			SiEvent siTmpl = new SiEvent();
			if(siCode != null)
			{
				siTmpl.Type = SiEventType.Key;
				siTmpl.VKey = siCode.VKey;
				siTmpl.ExtendedKey = siCode.ExtKey;
			}
			else if(strName.Length == 1)
			{
				siTmpl.Type = SiEventType.Char;
				siTmpl.Char = strName[0];
			}
			else
			{
				throw new FormatException(KPRes.AutoTypeUnknownPlaceholder +
					MessageService.NewLine + @"{" + strName + @"}");
			}

			uint uRepeat = 1;
			if(uParam.HasValue) uRepeat = uParam.Value;

			for(uint u = 0; u < uRepeat; ++u)
			{
				SiEvent si = new SiEvent();
				si.Type = siTmpl.Type;
				si.VKey = siTmpl.VKey;
				si.ExtendedKey = siTmpl.ExtendedKey;
				si.Char = siTmpl.Char;

				l.Add(si);
			}

			return l;
		}

		private static void FixEventSeq(List<SiEvent> l)
		{
			// Convert chars to keys
			// Keys kMod = Keys.None;
			for(int i = 0; i < l.Count; ++i)
			{
				SiEvent si = l[i];
				SiEventType t = si.Type;

				// if(t == SiEventType.KeyModifier)
				// {
				//	if(!si.Down.HasValue) { Debug.Assert(false); continue; }
				//	if(si.Down.Value)
				//	{
				//		Debug.Assert((kMod & si.KeyModifier) == Keys.None);
				//		kMod |= si.KeyModifier;
				//	}
				//	else
				//	{
				//		Debug.Assert((kMod & si.KeyModifier) == si.KeyModifier);
				//		kMod &= ~si.KeyModifier;
				//	}
				// }
				if(t == SiEventType.Char)
				{
					// bool bLightConv = (kMod == Keys.None);
					int iVKey = SiCodes.CharToVKey(si.Char, true);
					if(iVKey > 0)
					{
						si.Type = SiEventType.Key;
						si.VKey = iVKey;
					}
				}
			}
		}

		private static void Send(ISiEngine siEngine, List<SiEvent> l)
		{
			bool bHasClipOp = l.Exists(SendInputEx.IsClipboardOp);
			ClipboardEventChainBlocker cev = null;
			ClipboardContents cnt = null;
			if(bHasClipOp)
			{
				cev = new ClipboardEventChainBlocker();
				cnt = new ClipboardContents(true, true);
			}

			try { SendPriv(siEngine, l); }
			finally
			{
				if(bHasClipOp)
				{
					ClipboardUtil.Clear();
					cnt.SetData();
					cev.Release();
				}
			}
		}

		private static bool IsClipboardOp(SiEvent si)
		{
			if(si == null) { Debug.Assert(false); return false; }
			return (si.Type == SiEventType.ClipboardCopy);
		}

		private static void SendPriv(ISiEngine siEngine, List<SiEvent> l)
		{
			// For 2000 alphanumeric characters:
			// * KeePass 1.26: 0:31 min
			// * KeePass 2.24: 1:58 min
			// * New engine of KeePass 2.25 with default delay DD:
			//   * DD =  1: 0:31 min
			//   * DD = 31: 1:03 min
			//   * DD = 32: 1:34 min
			//   * DD = 33: 1:34 min
			//   * DD = 43: 1:34 min
			//   * DD = 46: 1:34 min
			//   * DD = 47: 2:05 min
			//   * DD = 49: 2:05 min
			//   * DD = 59: 2:05 min
			uint uDefaultDelay = 33; // Slice boundary + 1

			// Induced by SiEngineWin.TrySendCharByKeypresses
			uDefaultDelay += 2;

			int iDefOvr = Program.Config.Integration.AutoTypeInterKeyDelay;
			if(iDefOvr >= 0)
			{
				if(iDefOvr == 0) iDefOvr = 1; // 1 ms is minimum
				uDefaultDelay = (uint)iDefOvr;
			}

			bool bFirstInput = true;
			foreach(SiEvent si in l)
			{
				// Also delay key modifiers, as a workaround for applications
				// with broken time-dependent message processing;
				// https://sourceforge.net/p/keepass/bugs/1213/
				if((si.Type == SiEventType.Key) || (si.Type == SiEventType.Char) ||
					(si.Type == SiEventType.KeyModifier))
				{
					if(!bFirstInput)
						siEngine.Delay(uDefaultDelay);

					bFirstInput = false;
				}

				switch(si.Type)
				{
					case SiEventType.Key:
						siEngine.SendKey(si.VKey, si.ExtendedKey, si.Down);
						break;

					case SiEventType.KeyModifier:
						if(si.Down.HasValue)
							siEngine.SetKeyModifier(si.KeyModifier, si.Down.Value);
						else { Debug.Assert(false); }
						break;

					case SiEventType.Char:
						siEngine.SendChar(si.Char, si.Down);
						break;

					case SiEventType.Delay:
						siEngine.Delay(si.Delay);
						break;

					case SiEventType.SetDefaultDelay:
						uDefaultDelay = si.Delay;
						break;

					case SiEventType.ClipboardCopy:
						if(!string.IsNullOrEmpty(si.Text))
							ClipboardUtil.Copy(si.Text, false, false, null,
								null, IntPtr.Zero);
						else if(si.Text != null)
							ClipboardUtil.Clear();
						break;

					default:
						Debug.Assert(false);
						break;
				}

				// Extra delay after tabs
				if(((si.Type == SiEventType.Key) && (si.VKey == (int)Keys.Tab)) ||
					((si.Type == SiEventType.Char) && (si.Char == '\t')))
				{
					if(uDefaultDelay < 100)
						siEngine.Delay(uDefaultDelay);
				}
			}
		}
	}
}
