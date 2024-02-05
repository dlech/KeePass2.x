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
using System.Windows.Forms;

using KeePass.App;
using KeePass.Forms;
using KeePass.Native;
using KeePass.Resources;
using KeePass.UI;

using KeePassLib;
using KeePassLib.Utility;

using NativeLib = KeePassLib.Native.NativeLib;

namespace KeePass.Util
{
	public static class HotKeyManager
	{
		private static Form g_fRecvWnd = null;
		private static readonly Dictionary<int, Keys> g_dRegKeys = new Dictionary<int, Keys>();

		// private static NativeMethods.BindKeyHandler g_hOnHotKey =
		//	new NativeMethods.BindKeyHandler(HotKeyManager.OnHotKey);

		// public static Form ReceiverWindow
		// {
		//	get { return g_fRecvWnd; }
		//	set { g_fRecvWnd = value; }
		// }

		public static bool Initialize(Form fRecvWnd)
		{
			g_fRecvWnd = fRecvWnd;

			// if(NativeLib.IsUnix())
			// {
			//	try { NativeMethods.tomboy_keybinder_init(); }
			//	catch(Exception) { Debug.Assert(false); return false; }
			// }

			return true;
		}

		public static bool RegisterHotKey(int nId, Keys kKey)
		{
			UnregisterHotKey(nId);

			uint uMod = 0;
			if((kKey & Keys.Shift) != Keys.None) uMod |= NativeMethods.MOD_SHIFT;
			if((kKey & Keys.Alt) != Keys.None) uMod |= NativeMethods.MOD_ALT;
			if((kKey & Keys.Control) != Keys.None) uMod |= NativeMethods.MOD_CONTROL;

			uint vkCode = (uint)(kKey & Keys.KeyCode);
			if(vkCode == (uint)Keys.None) return false; // Don't register mod keys only

			try
			{
				if(!NativeLib.IsUnix())
				{
					if(NativeMethods.RegisterHotKey(g_fRecvWnd.Handle, nId, uMod, vkCode))
					{
						g_dRegKeys[nId] = kKey;
						return true;
					}
				}
				else // Unix
				{
					// NativeMethods.tomboy_keybinder_bind(EggAccKeysToString(kKey),
					//	g_hOnHotKey);
					// g_dRegKeys[nId] = kKey;
					// return true;
				}
			}
			catch(Exception) { Debug.Assert(false); }

			return false;
		}

		public static bool UnregisterHotKey(int nId)
		{
			if(g_dRegKeys.ContainsKey(nId))
			{
				// Keys k = g_dRegKeys[nId];
				g_dRegKeys.Remove(nId);

				try
				{
					bool bResult;
					if(!NativeLib.IsUnix())
						bResult = NativeMethods.UnregisterHotKey(g_fRecvWnd.Handle, nId);
					else // Unix
					{
						// NativeMethods.tomboy_keybinder_unbind(EggAccKeysToString(k),
						//	g_hOnHotKey);
						// bResult = true;
						bResult = false;
					}

					// Debug.Assert(bResult);
					return bResult;
				}
				catch(Exception) { Debug.Assert(false); }
			}

			return false;
		}

		public static void UnregisterAll()
		{
			List<int> vIDs = new List<int>(g_dRegKeys.Keys);
			foreach(int nID in vIDs) UnregisterHotKey(nID);

			Debug.Assert(g_dRegKeys.Count == 0);
		}

		public static bool IsHotKeyRegistered(Keys kKey, bool bGlobal)
		{
			if(g_dRegKeys.ContainsValue(kKey)) return true;
			if(!bGlobal) return false;

			int nID = AppDefs.GlobalHotKeyId.TempRegTest;
			if(!RegisterHotKey(nID, kKey)) return true;

			UnregisterHotKey(nID);
			return false;
		}

		/* private static void OnHotKey(string strKey, IntPtr lpUserData)
		{
			if(string.IsNullOrEmpty(strKey)) return;
			if(strKey.IndexOf(@"<Release>", StrUtil.CaseIgnoreCmp) >= 0) return;

			if(g_fRecvWnd != null)
			{
				MainForm mf = (g_fRecvWnd as MainForm);
				if(mf == null) { Debug.Assert(false); return; }

				Keys k = EggAccStringToKeys(strKey);
				foreach(KeyValuePair<int, Keys> kvp in g_dRegKeys)
				{
					if(kvp.Value == k) mf.HandleHotKey(kvp.Key);
				}
			}
			else { Debug.Assert(false); }
		}

		private static Keys EggAccStringToKeys(string strKey)
		{
			if(string.IsNullOrEmpty(strKey)) return Keys.None;

			Keys k = Keys.None;

			if(strKey.IndexOf(@"<Alt>", StrUtil.CaseIgnoreCmp) >= 0)
				k |= Keys.Alt;
			if((strKey.IndexOf(@"<Ctl>", StrUtil.CaseIgnoreCmp) >= 0) ||
				(strKey.IndexOf(@"<Ctrl>", StrUtil.CaseIgnoreCmp) >= 0) ||
				(strKey.IndexOf(@"<Control>", StrUtil.CaseIgnoreCmp) >= 0))
				k |= Keys.Control;
			if((strKey.IndexOf(@"<Shft>", StrUtil.CaseIgnoreCmp) >= 0) ||
				(strKey.IndexOf(@"<Shift>", StrUtil.CaseIgnoreCmp) >= 0))
				k |= Keys.Shift;

			string strKeyCode = strKey;
			while(strKeyCode.IndexOf('<') >= 0)
			{
				int nStart = strKeyCode.IndexOf('<');
				int nEnd = strKeyCode.IndexOf('>');
				if((nStart < 0) || (nEnd < 0) || (nEnd <= nStart)) { Debug.Assert(false); break; }

				strKeyCode = strKeyCode.Remove(nStart, nEnd - nStart + 1);
			}
			strKeyCode = strKeyCode.Trim();

			try { k |= (Keys)Enum.Parse(typeof(Keys), strKeyCode, true); }
			catch(Exception) { Debug.Assert(false); }

			return k;
		}

		private static string EggAccKeysToString(Keys k)
		{
			StringBuilder sb = new StringBuilder();

			if((k & Keys.Shift) != Keys.None) sb.Append(@"<Shift>");
			if((k & Keys.Control) != Keys.None) sb.Append(@"<Control>");
			if((k & Keys.Alt) != Keys.None) sb.Append(@"<Alt>");

			sb.Append((k & Keys.KeyCode).ToString());
			return sb.ToString();
		} */

		internal static void CheckCtrlAltA(Form fParent)
		{
			try
			{
				if(!Program.Config.Integration.CheckHotKeys) return;
				if(NativeLib.IsUnix()) return;

				// Check for a conflict only in the very specific case of
				// Ctrl+Alt+A; in all other cases we assume that the user
				// is aware of a possible conflict and intentionally wants
				// to override any system key combination
				if(Program.Config.Integration.HotKeyGlobalAutoType !=
					(long)(Keys.Control | Keys.Alt | Keys.A)) return;

				// Check for a conflict only on Polish systems; other
				// languages typically don't use Ctrl+Alt+A frequently
				// and a conflict warning would just be confusing for
				// most users
				IntPtr hKL = NativeMethods.GetKeyboardLayout(0);
				ushort uLangID = (ushort)(hKL.ToInt64() & 0xFFFFL);
				ushort uPriLangID = NativeMethods.GetPrimaryLangID(uLangID);
				if(uPriLangID != NativeMethods.LANG_POLISH) return;

				int vk = (int)Keys.A;

				// We actually check for RAlt (which maps to Ctrl+Alt)
				// instead of LCtrl+LAlt
				byte[] pbState = new byte[256];
				pbState[NativeMethods.VK_CONTROL] = 0x80;
				pbState[NativeMethods.VK_MENU] = 0x80;
				pbState[NativeMethods.VK_RMENU] = 0x80;
				pbState[NativeMethods.VK_NUMLOCK] = 0x01; // Toggled
				// pbState[vk] = 0x80;

				string strUni = NativeMethods.ToUnicode3(vk, pbState, IntPtr.Zero);
				if(string.IsNullOrEmpty(strUni)) return;
				if(strUni.EndsWith("a") || strUni.EndsWith("A")) return;

				if(char.IsControl(strUni, 0)) { Debug.Assert(false); strUni = "?"; }

				string str = KPRes.CtrlAltAConflict.Replace("{PARAM}", strUni) +
					MessageService.NewParagraph + KPRes.CtrlAltAConflictHint;

				VistaTaskDialog dlg = new VistaTaskDialog();
				dlg.AddButton((int)DialogResult.Cancel, KPRes.Ok, null);
				dlg.CommandLinks = false;
				dlg.Content = str;
				dlg.DefaultButtonID = (int)DialogResult.Cancel;
				dlg.MainInstruction = KPRes.KeyboardKeyCtrl + "+" +
					KPRes.KeyboardKeyAlt + "+A - " + KPRes.Warning;
				dlg.SetIcon(VtdIcon.Warning);
				dlg.VerificationText = UIUtil.GetDialogNoShowAgainText(null);
				dlg.WindowTitle = PwDefs.ShortProductName;

				if(dlg.ShowDialog(fParent))
				{
					if(dlg.ResultVerificationChecked)
						Program.Config.Integration.CheckHotKeys = false;
				}
				else MessageService.ShowWarning(str);
			}
			catch(Exception) { Debug.Assert(false); }
		}

		internal static bool HandleHotKeyIntoSelf(int wParam)
		{
			try
			{
				OptionsForm f = (GlobalWindowManager.TopWindow as OptionsForm);
				if(f == null) return false;

				IntPtr h = NativeMethods.GetForegroundWindowHandle();
				if(h != f.Handle) return false;

				HotKeyControlEx c = (f.ActiveControl as HotKeyControlEx);
				if(c == null) return false;

				Keys k;
				if(!g_dRegKeys.TryGetValue(wParam, out k)) return false;
				if(k == Keys.None) { Debug.Assert(false); return false; }

				c.HotKey = k;
				return true;
			}
			catch(Exception) { Debug.Assert(false); }

			return false;
		}
	}
}
