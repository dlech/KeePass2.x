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
using System.Windows.Forms;
using System.Security;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;

namespace KeePass.UI
{
	/// <summary>
	/// Secure edit control class. Supports storing passwords in an encrypted
	/// form in the process memory.
	/// </summary>
	public sealed class SecureEdit
	{
		private static char m_chPasswordChar = '\u25CF';

		private TextBox m_tbPassword = null;
		private EventHandler m_evTextChanged = null;
		
		// SecureString objects are supported only on Windows 2000 SP3 and
		// higher. On all other systems (98 / ME) we use a standard string
		// object instead of the secure one. This of course decreases the
		// security of the control, but at least allows the application
		// to run on older systems, too.
		private SecureString m_secString = null; // Created in constructor
		private string m_strAlternativeSecString = string.Empty;

		private bool m_bBlockTextChanged = false;

		private bool m_bFirstGotFocus = true;

		public uint TextLength
		{
			get
			{
				if(m_secString != null) return (uint)m_secString.Length;
				return (uint)m_strAlternativeSecString.Length;
			}
		}

		static SecureEdit()
		{
			// On Windows 98 / ME, an ANSI character must be used as
			// password char!
			if(Environment.OSVersion.Platform == PlatformID.Win32Windows)
				m_chPasswordChar = '\u00D7';
		}

		/// <summary>
		/// Construct a new <c>SecureEdit</c> object. You must call the
		/// <c>Attach</c> member function to associate the secure edit control
		/// with a text box.
		/// </summary>
		public SecureEdit()
		{
			try { m_secString = new SecureString(); }
			catch(NotSupportedException) { } // Windows 98 / ME
		}

		~SecureEdit()
		{
			this.Detach();
		}

		/// <summary>
		/// Associate the current secure edit object with a text box.
		/// </summary>
		/// <param name="tbPasswordBox">Text box to link to.</param>
		/// <param name="bHidePassword">Initial protection flag.</param>
		public void Attach(TextBox tbPasswordBox, EventHandler evTextChanged,
			bool bHidePassword)
		{
			Debug.Assert(tbPasswordBox != null);
			if(tbPasswordBox == null) throw new ArgumentNullException("tbPasswordBox");

			this.Detach();

			m_tbPassword = tbPasswordBox;
			m_evTextChanged = evTextChanged;

			// Initialize to zero-length string
			m_tbPassword.Text = string.Empty;

			if(m_secString != null) m_secString.Clear();
			else m_strAlternativeSecString = string.Empty;

			EnableProtection(bHidePassword);

			if(m_evTextChanged != null) m_evTextChanged(m_tbPassword, EventArgs.Empty);

			m_tbPassword.AllowDrop = true;

			// Register event handler
			m_tbPassword.TextChanged += this.OnPasswordTextChanged;
			m_tbPassword.GotFocus += this.OnGotFocus;
			m_tbPassword.DragEnter += this.OnDragCheck;
			m_tbPassword.DragOver += this.OnDragCheck;
			m_tbPassword.DragDrop += this.OnDragDrop;
		}

		/// <summary>
		/// Remove the current association. You should call this before the
		/// text box is destroyed.
		/// </summary>
		public void Detach()
		{
			if(m_tbPassword != null)
			{
				m_tbPassword.TextChanged -= this.OnPasswordTextChanged;
				m_tbPassword.GotFocus -= this.OnGotFocus;
				m_tbPassword.DragEnter -= this.OnDragCheck;
				m_tbPassword.DragOver -= this.OnDragCheck;
				m_tbPassword.DragDrop -= this.OnDragDrop;

				m_tbPassword = null;
			}
		}

		public void EnableProtection(bool bEnable)
		{
			if(m_tbPassword == null) { Debug.Assert(false); return; }

			if(bEnable) FontUtil.AssignDefault(m_tbPassword);
			else
			{
				FontUtil.SetDefaultFont(m_tbPassword);
				FontUtil.AssignDefaultMono(m_tbPassword, true);
			}

			if(m_tbPassword.UseSystemPasswordChar == bEnable) return;
			m_tbPassword.UseSystemPasswordChar = bEnable;

			ShowCurrentPassword(m_tbPassword.SelectionStart, m_tbPassword.SelectionLength);
		}

		private void OnPasswordTextChanged(object sender, EventArgs e)
		{
			if(m_tbPassword == null) { Debug.Assert(false); return; }

			if(m_bBlockTextChanged) return;

			int nSelPos = m_tbPassword.SelectionStart;
			int nSelLen = m_tbPassword.SelectionLength;

			if(m_tbPassword.UseSystemPasswordChar == false)
			{
				RemoveInsert(0, 0, m_tbPassword.Text);
				ShowCurrentPassword(nSelPos, nSelLen);
				return;
			}

			string strText = m_tbPassword.Text;

			int inxLeft = -1, inxRight = 0;
			StringBuilder sbNewPart = new StringBuilder();

			for(int i = 0; i < strText.Length; ++i)
			{
				if(strText[i] != m_chPasswordChar)
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

			ShowCurrentPassword(nSelPos, nSelLen);

			// Check for m_tbPassword being null from on now; the
			// control might be disposed already (by the user handler
			// triggered by the ShowCurrentPassword call)
			if(m_tbPassword != null)
				m_tbPassword.ClearUndo(); // Would need special undo buffer
		}

		private void ShowCurrentPassword(int nSelStart, int nSelLength)
		{
			if(m_tbPassword == null) { Debug.Assert(false); return; }

			if(m_tbPassword.UseSystemPasswordChar == false)
			{
				m_bBlockTextChanged = true;
				m_tbPassword.Text = GetAsString();
				m_bBlockTextChanged = false;

				if(m_evTextChanged != null) m_evTextChanged(m_tbPassword, EventArgs.Empty);
				return;
			}

			m_bBlockTextChanged = true;
			if(m_secString != null)
				m_tbPassword.Text = new string(m_chPasswordChar, m_secString.Length);
			else
				m_tbPassword.Text = new string(m_chPasswordChar, m_strAlternativeSecString.Length);
			m_bBlockTextChanged = false;

			m_tbPassword.SelectionStart = nSelStart;
			m_tbPassword.SelectionLength = nSelLength;

			if(m_evTextChanged != null) m_evTextChanged(m_tbPassword, EventArgs.Empty);
		}

		public byte[] ToUtf8()
		{
			Debug.Assert(sizeof(char) == 2);

			if(m_secString != null)
			{
				char[] vChars = new char[m_secString.Length];
				IntPtr p = Marshal.SecureStringToGlobalAllocUnicode(m_secString);
				for(int i = 0; i < m_secString.Length; ++i)
					vChars[i] = (char)Marshal.ReadInt16(p, i * 2);
				Marshal.ZeroFreeGlobalAllocUnicode(p);

				byte[] pb = Encoding.UTF8.GetBytes(vChars);
				Array.Clear(vChars, 0, vChars.Length);

				return pb;
			}
			else return Encoding.UTF8.GetBytes(m_strAlternativeSecString);
		}

		private string GetAsString()
		{
			if(m_secString != null)
			{
				IntPtr p = Marshal.SecureStringToGlobalAllocUnicode(m_secString);
				string str = Marshal.PtrToStringUni(p);
				Marshal.ZeroFreeGlobalAllocUnicode(p);

				return str;
			}
			else return m_strAlternativeSecString;
		}

		private void RemoveInsert(int nLeftRem, int nRightRem, string strInsert)
		{
			Debug.Assert(nLeftRem >= 0);

			if(m_secString != null)
			{
				while(m_secString.Length > (nLeftRem + nRightRem))
					m_secString.RemoveAt(nLeftRem);

				for(int i = 0; i < strInsert.Length; ++i)
					m_secString.InsertAt(nLeftRem + i, strInsert[i]);
			}
			else
			{
				StringBuilder sb = new StringBuilder(m_strAlternativeSecString);

				while(sb.Length > (nLeftRem + nRightRem))
					sb.Remove(nLeftRem, 1);

				sb.Insert(nLeftRem, strInsert);

				m_strAlternativeSecString = sb.ToString();
			}
		}

		public bool ContentsEqualTo(SecureEdit secOther)
		{
			Debug.Assert(secOther != null); if(secOther == null) return false;

			byte[] pbThis = this.ToUtf8();
			byte[] pbOther = secOther.ToUtf8();

			bool bEqual = true;

			if(pbThis.Length != pbOther.Length) bEqual = false;
			else
			{
				for(int i = 0; i < pbThis.Length; ++i)
				{
					if(pbThis[i] != pbOther[i])
					{
						bEqual = false;
						break;
					}
				}
			}

			Array.Clear(pbThis, 0, pbThis.Length);
			Array.Clear(pbOther, 0, pbOther.Length);
			return bEqual;
		}

		public void SetPassword(byte[] pbUtf8)
		{
			Debug.Assert(pbUtf8 != null);
			if(pbUtf8 == null) throw new ArgumentNullException("pbUTF8");

			if(m_secString != null)
			{
				m_secString.Clear();

				UTF8Encoding utf8 = new UTF8Encoding();
				char[] vChars = utf8.GetChars(pbUtf8);

				for(int i = 0; i < vChars.Length; ++i)
				{
					m_secString.AppendChar(vChars[i]);
					vChars[i] = char.MinValue;
				}
			}
			else m_strAlternativeSecString = Encoding.UTF8.GetString(pbUtf8);

			ShowCurrentPassword(0, 0);
		}

		private void OnGotFocus(object sender, EventArgs e)
		{
			if(m_tbPassword == null) { Debug.Assert(false); return; }

			if(m_bFirstGotFocus && (m_tbPassword != null))
				m_tbPassword.SelectAll();

			m_bFirstGotFocus = false;
		}

		private void OnDragCheck(object sender, DragEventArgs e)
		{
			if(e.Data.GetDataPresent(typeof(string)))
				e.Effect = DragDropEffects.Copy;
			else e.Effect = DragDropEffects.None;
		}

		private void OnDragDrop(object sender, DragEventArgs e)
		{
			if(e.Data.GetDataPresent(typeof(string)))
			{
				string strData = e.Data.GetData(typeof(string)) as string;
				if(strData == null) { Debug.Assert(false); return; }

				if(m_tbPassword != null) m_tbPassword.Paste(strData);
			}
		}
	}
}
