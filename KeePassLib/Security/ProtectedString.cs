/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2008 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Security;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

using KeePassLib.Cryptography;

#if KeePassLibSD
using KeePassLibSD;
#endif

namespace KeePassLib.Security
{
	/// <summary>
	/// Represents an in-memory encrypted string.
	/// </summary>
	public sealed class ProtectedString
	{
		// SecureString objects are supported only on Windows 2000 SP3 and
		// higher. On all other systems (98 / ME) we use a standard string
		// object instead of the secure one. This of course decreases the
		// security of the class, but at least allows the application
		// to run on older systems, too.
		private SecureString m_secString = null; // Created in constructor
		private string m_strAlternativeSecString = string.Empty;

		private string m_strPlainText = string.Empty; // Never null
		private bool m_bIsProtected = false; // See default constructor

		private XorredBuffer m_xbEncrypted = null; // UTF-8 representation

		/// <summary>
		/// A flag specifying whether the <c>ProtectedString</c> object has turned on
		/// in-memory protection or not.
		/// </summary>
		public bool IsProtected
		{
			get { return m_bIsProtected; }
		}

		/// <summary>
		/// A value specifying whether the <c>ProtectedString</c> object is currently
		/// in-memory protected or not. This flag can be different than
		/// <c>IsProtected</c>: if a <c>XorredBuffer</c> is used, the <c>IsProtected</c>
		/// flag represents the memory protection flag, but not the actual protection.
		/// In this case use <c>IsViewable</c>, which returns <c>true</c> if a
		/// <c>XorredBuffer</c> is currently in use.
		/// </summary>
		public bool IsViewable
		{
			get { return (!m_bIsProtected && (m_xbEncrypted == null)); }
		}

		public int Length
		{
			get
			{
				if(m_xbEncrypted != null)
				{
					byte[] pb = m_xbEncrypted.ReadPlainText();

					string str = Encoding.UTF8.GetString(pb, 0, pb.Length);
					SetString(str); // Clear the XorredBuffer object

					// No need to erase the pb buffer, the plain text is
					// now readable in memory anyway (in str).

					return ((str != null) ? str.Length : 0);
				}

				if(m_bIsProtected)
				{
					if(m_secString != null) return m_secString.Length;
					else return m_strAlternativeSecString.Length;
				}
				
				return m_strPlainText.Length; // Unprotected string
			}
		}

		/// <summary>
		/// Construct a new protected string object. Protection is
		/// disabled by default! You need to call the
		/// <c>EnableProtection</c> member function in order to
		/// enable the protection, if you wish the string to be protected.
		/// </summary>
		public ProtectedString()
		{
			try { m_secString = new SecureString(); }
			catch(NotSupportedException) { } // Windows 98 / ME
		}

		/// <summary>
		/// Construct a new in-memory encrypted string object.
		/// </summary>
		/// <param name="bEnableProtection">If this parameter is <c>true</c>,
		/// the string will be protected in-memory (encrypted). If it
		/// is <c>false</c>, the string will be stored as plain-text.</param>
		public ProtectedString(bool bEnableProtection)
		{
			try { m_secString = new SecureString(); }
			catch(NotSupportedException) { } // Windows 98 / ME

			m_bIsProtected = bEnableProtection;
		}

		/// <summary>
		/// Construct a new protected string. The string is initialized
		/// to the value supplied in the parameters.
		/// </summary>
		/// <param name="bEnableProtection">If this parameter is <c>true</c>,
		/// the string will be protected in-memory (encrypted). If it
		/// is <c>false</c>, the string will be stored as plain-text.</param>
		/// <param name="strValue">The initial string value. This
		/// parameter won't be modified.</param>
		public ProtectedString(bool bEnableProtection, string strValue)
		{
			try { m_secString = new SecureString(); }
			catch(NotSupportedException) { } // Windows 98 / ME

			m_bIsProtected = bEnableProtection;
			SetString(strValue);
		}

		/// <summary>
		/// Construct a new protected string. The string is initialized
		/// to the value supplied in the parameters (UTF-8 encoded string).
		/// </summary>
		/// <param name="bEnableProtection">If this parameter is <c>true</c>,
		/// the string will be protected in-memory (encrypted). If it
		/// is <c>false</c>, the string will be stored as plain-text.</param>
		/// <param name="vUtf8Value">The initial string value, encoded as
		/// UTF-8 byte array. This parameter won't be modified.</param>
		public ProtectedString(bool bEnableProtection, byte[] vUtf8Value)
		{
			if(vUtf8Value == null) throw new ArgumentNullException("vUtf8Value");

			try { m_secString = new SecureString(); }
			catch(NotSupportedException) { } // Windows 98 / ME

			m_bIsProtected = bEnableProtection;
			SetString(Encoding.UTF8.GetString(vUtf8Value, 0, vUtf8Value.Length));
		}

		/// <summary>
		/// Construct a new protected string. The string is initialized
		/// to the value passed in the <c>pbTemplate</c> protected string.
		/// </summary>
		/// <param name="psTemplate">The initial string value. This
		/// parameter won't be modified. Must not be <c>null</c>.</param>
		/// <exception cref="System.ArgumentNullException">Thrown if the input
		/// parameter is <c>null</c>.</exception>
		public ProtectedString(ProtectedString psTemplate)
		{
			Debug.Assert(psTemplate != null);
			if(psTemplate == null) throw new ArgumentNullException("psTemplate");

			try { m_secString = new SecureString(); }
			catch(NotSupportedException) { } // Windows 98 / ME

			m_bIsProtected = psTemplate.m_bIsProtected;
			SetString(psTemplate.ReadString());
		}

		/// <summary>
		/// Construct a new protected string. The string is initialized
		/// to the value passed in the <c>XorredBuffer</c> object.
		/// </summary>
		/// <param name="bEnableProtection">Enable protection or not.</param>
		/// <param name="xbProtected"><c>XorredBuffer</c> object containing the
		/// string in UTF-8 representation. The UTF-8 string must not
		/// be <c>null</c>-terminated.</param>
		/// <exception cref="System.ArgumentNullException">Thrown if the input
		/// parameter is <c>null</c>.</exception>
		public ProtectedString(bool bEnableProtection, XorredBuffer xbProtected)
		{
			Debug.Assert(xbProtected != null);
			if(xbProtected == null) throw new ArgumentNullException("xbProtected");

			try { m_secString = new SecureString(); }
			catch(NotSupportedException) { } // Windows 98 / ME

			m_bIsProtected = bEnableProtection;
			m_xbEncrypted = xbProtected;
		}

		/// <summary>
		/// Clear the string. Doesn't change the protection level.
		/// </summary>
		public void Clear()
		{
			if(m_secString != null) m_secString.Clear();
			else m_strAlternativeSecString = string.Empty;

			m_strPlainText = "";

			m_xbEncrypted = null;
		}

		/// <summary>
		/// Change the protection level (protect or don't protect). Note: you
		/// only need to call this function if you really want to change the
		/// protection. If you specified the protection flag in the constructor,
		/// and don't want to change it, you don't need to call this function.
		/// </summary>
		/// <param name="bProtect">If <c>true</c>, the string will be protected
		/// (encrypted in-memory). Otherwise the string will be stored in
		/// plain-text in the process memory.</param>
		public void EnableProtection(bool bProtect)
		{
			if(m_xbEncrypted != null)
			{
				m_bIsProtected = bProtect;
				return;
			}

			if(m_bIsProtected && !bProtect) // Unprotect
			{
				string strPlainText = ReadString();

				Clear();

				m_strPlainText = strPlainText;
				m_bIsProtected = false;
			}
			else if(!m_bIsProtected && bProtect) // Protect
			{
				m_bIsProtected = true;

				SetString(m_strPlainText);
			}
		}

		/// <summary>
		/// Assign a new string value to the object.
		/// </summary>
		/// <param name="strNewValue">New string. The string must not contain
		/// a <c>null</c> terminator.</param>
		/// <exception cref="System.ArgumentNullException">Thrown if the input
		/// parameter is <c>null</c>.</exception>
		/// <exception cref="System.ArgumentException">Thrown if the new string
		/// contains a <c>null</c> terminator.</exception>
		public void SetString(string strNewValue)
		{
			Clear();

			Debug.Assert(strNewValue != null); if(strNewValue == null) throw new ArgumentNullException("strNewValue");

			// String must not contain any null character
			Debug.Assert(strNewValue.IndexOf((char)0) < 0);

			if(m_bIsProtected)
			{
				if(m_secString != null)
				{
					char ch;
					for(int i = 0; i < strNewValue.Length; i++)
					{
						ch = strNewValue[i];
						if(ch == 0) throw new ArgumentException();

						m_secString.AppendChar(ch);
					}
				}
				else m_strAlternativeSecString = strNewValue;
			}
			else // Currently not protected
			{
				m_strPlainText = strNewValue;
			}
		}

		/// <summary>
		/// Convert the protected string to a normal string object.
		/// Be careful with this function, the returned string object
		/// isn't protected any more and stored in plain-text in the
		/// process memory.
		/// </summary>
		/// <returns>Plain-text string. Is never <c>null</c>.</returns>
		public string ReadString()
		{
			if(m_xbEncrypted != null)
			{
				byte[] pb = m_xbEncrypted.ReadPlainText();

				string str = Encoding.UTF8.GetString(pb, 0, pb.Length);
				SetString(str); // Clear the XorredBuffer object

				// No need to erase the pb buffer, the plain text is
				// now readable in memory anyway (in str).

				return (str ?? string.Empty);
			}

			if(m_bIsProtected)
			{
				if(m_secString != null)
				{
#if !KeePassLibSD
					IntPtr p = Marshal.SecureStringToGlobalAllocUnicode(m_secString);
					string str = Marshal.PtrToStringUni(p);
					Marshal.ZeroFreeGlobalAllocUnicode(p);
#else
					string str = m_secString.ReadAsString();
#endif
					return (str ?? string.Empty);
				}
				else return m_strAlternativeSecString;
			}
			
			return m_strPlainText; // Unprotected string
		}

		/// <summary>
		/// Read out the string and return a byte array that contains the
		/// string encoded using UTF-8. The returned string is not protected
		/// anymore!
		/// </summary>
		/// <returns>Plain-text UTF-8 byte array.</returns>
		public byte[] ReadUtf8()
		{
			if(m_xbEncrypted != null)
			{
				byte[] pb = m_xbEncrypted.ReadPlainText();

				// Clear XorredBuffer
				SetString(Encoding.UTF8.GetString(pb, 0, pb.Length));

				return pb;
			}

			if(m_bIsProtected)
			{
				if(m_secString != null)
				{
#if !KeePassLibSD
					Debug.Assert(sizeof(char) == 2);
					char[] vChars = new char[m_secString.Length];

					IntPtr p = Marshal.SecureStringToGlobalAllocUnicode(m_secString);
					for(int i = 0; i < vChars.Length; ++i)
						vChars[i] = (char)Marshal.ReadInt16(p, i * 2);
					Marshal.ZeroFreeGlobalAllocUnicode(p);

					byte[] pb = Encoding.UTF8.GetBytes(vChars, 0, vChars.Length);
					Array.Clear(vChars, 0, vChars.Length);
#else
					byte[] pb = Encoding.UTF8.GetBytes(m_secString.ReadAsString());
#endif
					return pb;
				}
				else return Encoding.UTF8.GetBytes(m_strAlternativeSecString);
			}

			return Encoding.UTF8.GetBytes(m_strPlainText); // Unprotected string
		}

		/// <summary>
		/// Read the protected string and return it protected with a sequence
		/// of bytes generated by a random stream. The object's data will be
		/// invisible in process memory only if the object has been initialized
		/// using a <c>XorredBuffer</c>. If no <c>XorredBuffer</c> has been used
		/// or the string has been read once already (in plain-text), the
		/// operation won't be secure and the protected string will be visible
		/// in process memory.
		/// </summary>
		/// <param name="crsRandomSource">Random number source.</param>
		/// <returns>Protected string.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if the input
		/// parameter is <c>null</c>.</exception>
		public byte[] ReadXorredString(CryptoRandomStream crsRandomSource)
		{
			Debug.Assert(crsRandomSource != null); if(crsRandomSource == null) throw new ArgumentNullException("crsRandomSource");

			if(m_xbEncrypted != null)
			{
				uint uLen = m_xbEncrypted.Length;
				byte[] randomPad = crsRandomSource.GetRandomBytes(uLen);
				return m_xbEncrypted.ChangeKey(randomPad);
			}
			else // Not using XorredBuffer
			{
				byte[] pbData = ReadUtf8();
				uint uLen = (uint)pbData.Length;

				byte[] randomPad = crsRandomSource.GetRandomBytes(uLen);
				Debug.Assert(randomPad.Length == uLen);

				for(uint i = 0; i < uLen; i++)
					pbData[i] ^= randomPad[i];

				return pbData;
			}
		}
	}
}
