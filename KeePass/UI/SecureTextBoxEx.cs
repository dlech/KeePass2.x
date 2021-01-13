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
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using KeePassLib.Delegates;
using KeePassLib.Security;
using KeePassLib.Utility;

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
				this.Text = new string(SecureTextBoxEx.PasswordCharEx, m_psText.Length);
			--m_uBlockTextChanged;

			int nNewTextLen = this.TextLength;
			if(nSelStart < 0) { Debug.Assert(false); nSelStart = 0; }
			if(nSelStart > nNewTextLen) nSelStart = nNewTextLen; // Behind last char
			if(nSelLength < 0) { Debug.Assert(false); nSelLength = 0; }
			if((nSelStart + nSelLength) > nNewTextLen)
				nSelLength = nNewTextLen - nSelStart;
			Select(nSelStart, nSelLength);

			ClearUndo(); // Would need special undo buffer

			base.OnTextChanged(EventArgs.Empty); // this.Text set above
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

			string strText = this.Text;
			int nSelPos = this.SelectionStart;
			int nSelLen = this.SelectionLength;

			int inxLeft = -1, inxRight = 0;
			StringBuilder sbNewPart = new StringBuilder();

			char chPasswordChar = SecureTextBoxEx.PasswordCharEx;
			for(int i = 0; i < strText.Length; ++i)
			{
				if(strText[i] != chPasswordChar)
				{
					if(inxLeft == -1) inxLeft = i;
					inxRight = i;

					sbNewPart.Append(strText[i]);
				}
			}

			if(inxLeft < 0)
				RemoveInsert(nSelPos, strText.Length - nSelPos, string.Empty);
			else
				RemoveInsert(inxLeft, strText.Length - inxRight - 1,
					sbNewPart.ToString());

			ShowCurrentText(nSelPos, nSelLen);
		}

		private void RemoveInsert(int nLeftRem, int nRightRem, string strInsert)
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

			try { m_psText = m_psText.Insert(nLeftRem, strInsert); }
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
