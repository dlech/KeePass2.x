/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2007 Dominik Reichl <dominik.reichl@t-online.de>

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
		private const char PasswordChar = '\u25CF';

		private TextBox m_tbPassword = null;
		private EventHandler m_evTextChanged = null;
		private SecureString m_secString = new SecureString();

		private bool m_bBlockTextChanged = false;

		/// <summary>
		/// Construct a new <c>SecureEdit</c> object. You must call the
		/// <c>Attach</c> member function to associate the secure edit control
		/// with a text box.
		/// </summary>
		public SecureEdit()
		{
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
		public void Attach(TextBox tbPasswordBox, EventHandler evTextChanged, bool bHidePassword)
		{
			Debug.Assert(tbPasswordBox != null);
			if(tbPasswordBox == null) throw new ArgumentNullException("tbPasswordBox");

			this.Detach();

			m_tbPassword = tbPasswordBox;
			m_evTextChanged = evTextChanged;

			// Initialize to zero-length string
			m_tbPassword.Text = string.Empty;
			m_secString.Clear();

			EnableProtection(bHidePassword);

			if(m_evTextChanged != null) m_evTextChanged(m_tbPassword, EventArgs.Empty);

			// Register event handler
			m_tbPassword.TextChanged += this.OnPasswordTextChanged;
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
				m_tbPassword = null;
			}
		}

		public void EnableProtection(bool bEnable)
		{
			if(m_tbPassword.UseSystemPasswordChar == bEnable) return;

			m_tbPassword.UseSystemPasswordChar = bEnable;
			ShowCurrentPassword(m_tbPassword.SelectionStart, m_tbPassword.SelectionLength);
		}

		private void OnPasswordTextChanged(object sender, EventArgs e)
		{
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
			string strNewPart = string.Empty;

			for(int i = 0; i < strText.Length; ++i)
			{
				if(strText[i] != SecureEdit.PasswordChar)
				{
					if(inxLeft == -1) inxLeft = i;
					inxRight = i;

					strNewPart += strText[i];
				}
			}

			if(inxLeft < 0)
				RemoveInsert(nSelPos, strText.Length - nSelPos, string.Empty);
			else
				RemoveInsert(inxLeft, strText.Length - inxRight - 1, strNewPart);

			ShowCurrentPassword(nSelPos, nSelLen);
		}

		private void ShowCurrentPassword(int nSelStart, int nSelLength)
		{
			if(m_tbPassword.UseSystemPasswordChar == false)
			{
				m_bBlockTextChanged = true;
				m_tbPassword.Text = GetAsString();
				m_bBlockTextChanged = false;

				if(m_evTextChanged != null) m_evTextChanged(m_tbPassword, EventArgs.Empty);
				return;
			}

			m_bBlockTextChanged = true;
			m_tbPassword.Text = new string(PasswordChar, m_secString.Length);
			m_bBlockTextChanged = false;

			m_tbPassword.SelectionStart = nSelStart;
			m_tbPassword.SelectionLength = nSelLength;

			if(m_evTextChanged != null) m_evTextChanged(m_tbPassword, EventArgs.Empty);
		}

		public byte[] ToUTF8()
		{
			UTF8Encoding utf8 = new UTF8Encoding();

			Debug.Assert(sizeof(char) == 2);

			char[] vChars = new char[m_secString.Length];
			IntPtr p = Marshal.SecureStringToGlobalAllocUnicode(m_secString);
			for(int i = 0; i < m_secString.Length; ++i)
				vChars[i] = (char)Marshal.ReadInt16(p, i * 2);
			Marshal.ZeroFreeGlobalAllocUnicode(p);

			byte[] pb = utf8.GetBytes(vChars);
			Array.Clear(vChars, 0, vChars.Length);

			return pb;
		}

		private string GetAsString()
		{
			IntPtr p = Marshal.SecureStringToGlobalAllocUnicode(m_secString);
			string str = Marshal.PtrToStringUni(p);
			Marshal.ZeroFreeGlobalAllocUnicode(p);

			return str;
		}

		private void RemoveInsert(int nLeftRem, int nRightRem, string strInsert)
		{
			Debug.Assert(nLeftRem >= 0);

			while(m_secString.Length != (nLeftRem + nRightRem))
				m_secString.RemoveAt(nLeftRem);

			for(int i = 0; i < strInsert.Length; ++i)
				m_secString.InsertAt(nLeftRem + i, strInsert[i]);
		}

		public bool ContentsEqualTo(SecureEdit secOther)
		{
			Debug.Assert(secOther != null); if(secOther == null) return false;

			byte[] pbThis = this.ToUTF8();
			byte[] pbOther = secOther.ToUTF8();

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

		public void SetPassword(byte[] pbUTF8)
		{
			Debug.Assert(pbUTF8 != null);
			if(pbUTF8 == null) throw new ArgumentNullException("pbUTF8");

			m_secString.Clear();

			UTF8Encoding utf8 = new UTF8Encoding();
			char[] vChars = utf8.GetChars(pbUTF8);

			for(int i = 0; i < vChars.Length; ++i)
			{
				m_secString.AppendChar(vChars[i]);
				vChars[i] = char.MinValue;
			}

			ShowCurrentPassword(0, 0);
		}
	}
}
