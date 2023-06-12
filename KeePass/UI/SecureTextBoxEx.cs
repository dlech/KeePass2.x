/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2023 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using KeePass.Native;

using KeePassLib.Cryptography;
using KeePassLib.Cryptography.Cipher;
using KeePassLib.Cryptography.PasswordGenerator;
using KeePassLib.Delegates;
using KeePassLib.Security;
using KeePassLib.Utility;

using NativeLib = KeePassLib.Native.NativeLib;

namespace KeePass.UI
{
	// Non-sealed for plugins
	public class SecureTextBoxEx : TextBox
	{
		private uint m_uBlockTextChanged = 0;
		private bool m_bFirstGotFocus = true;

#if DEBUG
		private bool m_bInitExCalled = false;
#endif

		// With ProtectedString.Empty no incremental protection (RemoveInsert)
		[NonSerialized]
		private ProtectedString m_psText = ProtectedString.EmptyEx;
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual ProtectedString TextEx
		{
			get { return m_psText; }
			set
			{
				if(value == null) { Debug.Assert(false); value = ProtectedString.EmptyEx; }
				m_psText = value.WithProtection(true); // For incremental editing

				ShowCurrentText(0, 0);
			}
		}
		// https://msdn.microsoft.com/en-us/library/53b8022e.aspx
		public virtual void ResetTextEx() { this.TextEx = ProtectedString.EmptyEx; }
		public virtual bool ShouldSerializeTextEx() { return false; }

		private static char? m_ochPasswordChar = null;
		public static char PasswordCharEx
		{
			get
			{
				if(!m_ochPasswordChar.HasValue)
				{
					// On Windows 98/ME, an ANSI character must be used as
					// password char
					m_ochPasswordChar = ((Environment.OSVersion.Platform ==
						PlatformID.Win32Windows) ? '\u00D7' : '\u25CF');
				}

				return m_ochPasswordChar.Value;
			}
		}

		private bool UseWindowsApi
		{
			get
			{
				if(NativeLib.IsUnix() || !this.IsHandleCreated) return false;
				if(GetStyle(ControlStyles.CacheText)) { Debug.Assert(false); return false; }
				return true;
			}
		}

		public SecureTextBoxEx()
		{
			if(Program.DesignMode) return;

			try
			{
				bool bSTA = (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA);
				this.AllowDrop = bSTA;
			}
			catch(Exception) { Debug.Assert(false); this.AllowDrop = false; }

			this.UseSystemPasswordChar = true;
		}

#if DEBUG
		~SecureTextBoxEx()
		{
			if(!Program.DesignMode) { Debug.Assert(m_bInitExCalled); }
		}
#endif

		public static void InitEx(ref SecureTextBoxEx s)
		{
			if(Program.DesignMode) return;

#if DEBUG
			if(s != null) s.m_bInitExCalled = true; // Org. object
#endif
			UIUtil.PerformOverride(ref s);
#if DEBUG
			if(s != null) s.m_bInitExCalled = true; // New object
#endif
		}

		private static Dictionary<int, string> g_dPwCharStrings = null;
		internal static string GetPasswordCharString(int cch)
		{
			if(cch <= 0) { Debug.Assert(cch == 0); return string.Empty; }

			Dictionary<int, string> d = g_dPwCharStrings;
			if(d == null)
			{
				d = new Dictionary<int, string>();

				for(int i = 1; i < 40; ++i)
					d[i] = new string(SecureTextBoxEx.PasswordCharEx, i);

				g_dPwCharStrings = d;
			}

			string str;
			if(d.TryGetValue(cch, out str)) return str;

			str = new string(SecureTextBoxEx.PasswordCharEx, cch);
			d[cch] = str;
			return str;
		}

		public virtual void EnableProtection(bool bEnable)
		{
			if(bEnable == this.UseSystemPasswordChar) return;

			if(!MonoWorkarounds.IsRequired(5795))
			{
				FontUtil.SetDefaultFont(this);

				if(bEnable) FontUtil.AssignDefault(this);
				else FontUtil.AssignDefaultMono(this, true);
			}

			this.UseSystemPasswordChar = bEnable;
			ShowCurrentText(-1, -1);
		}

		private void ShowCurrentText(int nSelStart, int nSelLength)
		{
			if(nSelStart < 0) nSelStart = this.SelectionStart;
			if(nSelLength < 0) nSelLength = this.SelectionLength;

			++m_uBlockTextChanged;
			if(!this.UseSystemPasswordChar)
				this.Text = m_psText.ReadString();
			else
			{
				string str = GetPasswordCharString(m_psText.Length);
				if(!this.UseWindowsApi) this.Text = str;
				else if(NativeMethods.SetWindowText(this.Handle, str)) { Debug.Assert(this.Text == str); }
				else { Debug.Assert(false); }
			}
			--m_uBlockTextChanged;

			int nNewTextLen = this.TextLength;
			if(nSelStart < 0) { Debug.Assert(false); nSelStart = 0; }
			if(nSelStart > nNewTextLen) nSelStart = nNewTextLen; // Behind last char
			if(nSelLength < 0) { Debug.Assert(false); nSelLength = 0; }
			if((nSelStart + nSelLength) > nNewTextLen)
				nSelLength = nNewTextLen - nSelStart;
			Select(nSelStart, nSelLength);

			ClearUndo(); // Would need special undo buffer

			base.OnTextChanged(EventArgs.Empty); // Text has been set above
		}

		protected override void OnTextChanged(EventArgs e)
		{
			if(m_uBlockTextChanged != 0) return;

			if(!this.UseSystemPasswordChar)
			{
				m_psText = new ProtectedString(true, this.Text);
				base.OnTextChanged(e);
				return;
			}

			int nSelPos = this.SelectionStart;
			int nSelLen = this.SelectionLength;

			char[] vText = (!this.UseWindowsApi ? this.Text.ToCharArray() :
				NativeMethods.GetWindowTextV(this.Handle, true));
			Debug.Assert(MemUtil.ArrayHelperExOfChar.Equals(vText, this.Text.ToCharArray()));

			char chPasswordChar = SecureTextBoxEx.PasswordCharEx;
			int iLeft = -1, iRight = 0;
			for(int i = 0; i < vText.Length; ++i)
			{
				if(vText[i] != chPasswordChar)
				{
					if(iLeft < 0) iLeft = i;
					iRight = i;
				}
			}

			if(iLeft < 0)
				RemoveInsert(nSelPos, vText.Length - nSelPos, vText, 0, 0);
			else
				RemoveInsert(iLeft, vText.Length - iRight - 1,
					vText, iLeft, iRight - iLeft + 1);

			MemUtil.ZeroArray<char>(vText);

			ShowCurrentText(nSelPos, nSelLen);

			// ThreadPool.QueueUserWorkItem(new WaitCallback(this.CreateDummyStrings));
			CreateDummyStrings(); // Other thread => less intertwining in memory
		}

		private void RemoveInsert(int nLeftRem, int nRightRem, char[] vInsert,
			int iInsert, int cchInsert)
		{
			Debug.Assert(nLeftRem >= 0);

			try
			{
				int cr = m_psText.Length - (nLeftRem + nRightRem);
				if(cr >= 0) m_psText = m_psText.Remove(nLeftRem, cr);
				else { Debug.Assert(false); }
				Debug.Assert(m_psText.Length == (nLeftRem + nRightRem));
			}
			catch(Exception) { Debug.Assert(false); }

			try { m_psText = m_psText.Insert(nLeftRem, vInsert, iInsert, cchInsert); }
			catch(Exception) { Debug.Assert(false); }
		}

		private readonly string[] m_vDummyStrings = new string[64];
		private readonly string m_strAlphMain = PwCharSet.UpperCase +
			PwCharSet.LowerCase + PwCharSet.Digits + GetPasswordCharString(1);
		private void CreateDummyStrings()
		{
			try
			{
#if DEBUG
				// Stopwatch sw = Stopwatch.StartNew();
#endif

				int cch = m_psText.Length;
				if(cch == 0) return;

				int cRefs = m_vDummyStrings.Length;
				string[] vRefs = new string[cRefs];
				// lock(m_vDummyStrings) {
				Array.Copy(m_vDummyStrings, vRefs, cRefs); // Read access

				bool bUnix = NativeLib.IsUnix();
				char[] v0 = GetPasswordCharString(cch).ToCharArray();
				char[] v1 = GetPasswordCharString(cch + 1).ToCharArray();
				char[] v2 = (bUnix ? GetPasswordCharString(cch + 2).ToCharArray() : null);
				char chPasswordChar = v0[0];

				byte[] pbKey = CryptoRandom.Instance.GetRandomBytes(32);
				ChaCha20Cipher c = new ChaCha20Cipher(pbKey, new byte[12], true);
				byte[] pbBuf = new byte[4];
				GFunc<uint> fR = delegate()
				{
					c.Encrypt(pbBuf, 0, 4);
					return (uint)BitConverter.ToInt32(pbBuf, 0);
				};
				GFunc<uint, uint> fRM = delegate(uint uMaxExcl)
				{
					uint uGen, uRem;
					do { uGen = fR(); uRem = uGen % uMaxExcl; }
					while((uGen - uRem) > (uint.MaxValue - (uMaxExcl - 1U)));
					return uRem;
				};
				GFunc<uint, uint, uint> fRLow = delegate(uint uRandom, uint uMaxExcl)
				{
					const uint m = 0x000FFFFF;
					Debug.Assert(uMaxExcl <= (m + 1U));
					uint uGen = uRandom & m;
					uint uRem = uGen % uMaxExcl;
					if((uGen - uRem) <= (m - (uMaxExcl - 1U))) return uRem;
					return fRM(uMaxExcl);
				};
				GFunc<uint, string, char> fRLowChar = delegate(uint uRandom, string strCS)
				{
					return strCS[(int)fRLow(uRandom, (uint)strCS.Length)];
				};

				uint cIt = (uint)m_strAlphMain.Length * 3;
				if(bUnix) cIt <<= 2;
				cIt += fRM(cIt);

				for(uint uIt = cIt; uIt != 0; --uIt)
				{
					uint r = fR();

					char[] v;
					if(v2 != null)
					{
						while(r < 0x10000000U) { r = fR(); }
						if(r < 0x60000000U) v = v0;
						else if(r < 0xB0000000U) v = v1;
						else v = v2;
					}
					else v = (((r & 0x10000000) == 0) ? v0 : v1);

					int iPos;
					if((r & 0x07000000) == 0) iPos = (int)fRM((uint)v.Length);
					else iPos = v.Length - 1;

					char ch;
					if((r & 0x00300000) != 0)
						ch = fRLowChar(r, m_strAlphMain);
					else if((r & 0x00400000) == 0)
						ch = fRLowChar(r, PwCharSet.PrintableAsciiSpecial);
					else if((r & 0x00800000) == 0)
						ch = fRLowChar(r, PwCharSet.Latin1S);
					else
						ch = (char)(fRLow(r, 0xD7FFU) + 1);

					int cRep = 1;
					if(bUnix) cRep = (((r & 0x08000000) == 0) ? 1 : 9);

					Debug.Assert(v[iPos] == chPasswordChar);
					v[iPos] = ch;

					int iRef0 = (int)r & int.MaxValue;
					for(int i = cRep; i != 0; --i)
						vRefs[(iRef0 ^ i) % cRefs] = new string(v);

					v[iPos] = chPasswordChar;
				}

				c.Dispose();
				MemUtil.ZeroByteArray(pbKey);
				MemUtil.ZeroArray<char>(v0);
				MemUtil.ZeroArray<char>(v1);
				if(v2 != null) MemUtil.ZeroArray<char>(v2);

				// lock(m_vDummyStrings) {
				Array.Copy(vRefs, m_vDummyStrings, cRefs);

#if DEBUG
				// sw.Stop();
				// Trace.WriteLine(string.Format("CreateDummyStrings: {0} ms.", sw.ElapsedMilliseconds));
				// Console.WriteLine("CreateDummyStrings: {0} ms.", sw.ElapsedMilliseconds);
#endif
			}
			catch(Exception) { Debug.Assert(false); }
		}

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);

			if(m_bFirstGotFocus)
			{
				m_bFirstGotFocus = false;

				// OnGotFocus is not called when the box initially has the
				// focus; the user can select characters without triggering
				// OnGotFocus, thus we select all characters only if the
				// selection is in its original state (0, 0), otherwise
				// e.g. the selection restoration when hiding/unhiding does
				// not work the first time (because after restoring the
				// selection, we would override it here by selecting all)
				if((this.SelectionStart <= 0) && (this.SelectionLength <= 0))
					SelectAll();
			}
		}

		private static bool DragCheck(DragEventArgs e)
		{
			if(e.Data.GetDataPresent(typeof(string)))
			{
				DragDropEffects eA = e.AllowedEffect;
				if((eA & DragDropEffects.Copy) != DragDropEffects.None)
				{
					e.Effect = DragDropEffects.Copy;
					return true;
				}
				if((eA & DragDropEffects.Move) != DragDropEffects.None)
				{
					e.Effect = DragDropEffects.Move;
					return true;
				}
			}

			e.Effect = DragDropEffects.None;
			return false;
		}

		protected override void OnDragEnter(DragEventArgs drgevent)
		{
			if(!DragCheck(drgevent)) base.OnDragEnter(drgevent);
		}

		protected override void OnDragOver(DragEventArgs drgevent)
		{
			if(!DragCheck(drgevent)) base.OnDragOver(drgevent);
		}

		protected override void OnDragDrop(DragEventArgs drgevent)
		{
			if(drgevent.Data.GetDataPresent(typeof(string)))
			{
				string str = (drgevent.Data.GetData(typeof(string)) as string);
				if(str == null) { Debug.Assert(false); return; }
				if(str.Length == 0) return;

				Paste(str);
			}
			else base.OnDragDrop(drgevent);
		}
	}
}
