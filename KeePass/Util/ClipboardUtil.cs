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
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Security.Cryptography;

using KeePass.App;

namespace KeePass.Util
{
	public static class ClipboardUtil
	{
		private static string m_strFormat = null;
		private static byte[] m_pbDataHash32 = null;

		public static bool Copy(string strToCopy, bool bIsEntryInfo)
		{
			Debug.Assert(strToCopy != null);
			if(strToCopy == null) throw new ArgumentNullException("strToCopy");

			if(bIsEntryInfo && !AppPolicy.Try(AppPolicyFlag.CopyToClipboard))
				return false;

			try
			{
				Clipboard.Clear();
				Clipboard.SetText(strToCopy);

				m_pbDataHash32 = HashClipboard();
				m_strFormat = null;
			}
			catch(Exception) { Debug.Assert(false); return false; }

			return true;
		}

		public static bool Copy(byte[] pbToCopy, string strFormat, bool bIsEntryInfo)
		{
			Debug.Assert(pbToCopy != null);
			if(pbToCopy == null) throw new ArgumentNullException("pbToCopy");

			if(bIsEntryInfo && !AppPolicy.Try(AppPolicyFlag.CopyToClipboard))
				return false;

			try
			{
				Clipboard.Clear();
				Clipboard.SetData(strFormat, pbToCopy);

				m_strFormat = strFormat;

				SHA256Managed sha256 = new SHA256Managed();
				m_pbDataHash32 = sha256.ComputeHash(pbToCopy);
			}
			catch(Exception) { Debug.Assert(false); return false; }

			return true;
		}

		public static bool CopyAndMinimize(string strToCopy, bool bIsEntryInfo,
			Form formToMinimize)
		{
			if(ClipboardUtil.Copy(strToCopy, bIsEntryInfo))
			{
				if(formToMinimize != null)
					formToMinimize.WindowState = FormWindowState.Minimized;

				return true;
			}

			return false;
		}

		public static void ClearIfOwner()
		{
			// If we didn't copy anything or cleared it already: do nothing
			if(m_pbDataHash32 == null) return;
			if(m_pbDataHash32.Length != 32) { Debug.Assert(false); return; }

			byte[] pbHash = HashClipboard(); // Hash current contents
			if(pbHash == null) return; // Unknown data (i.e. no KeePass data)
			if(pbHash.Length != 32) { Debug.Assert(false); return; }

			for(int i = 0; i < m_pbDataHash32.Length; ++i)
			{
				if(m_pbDataHash32[i] != pbHash[i])
					return; // No KeePass data
			}

			m_pbDataHash32 = null;
			m_strFormat = null;

			try { Clipboard.Clear(); }
			catch(Exception) { Debug.Assert(false); }
		}

		private static byte[] HashClipboard()
		{
			try
			{
				if(Clipboard.ContainsText())
				{
					string strData = Clipboard.GetText();
					byte[] pbUtf8 = Encoding.UTF8.GetBytes(strData);

					SHA256Managed sha256 = new SHA256Managed();
					return sha256.ComputeHash(pbUtf8);
				}
				else if(m_strFormat != null)
				{
					if(Clipboard.ContainsData(m_strFormat))
					{
						byte[] pbData = (byte[])Clipboard.GetData(m_strFormat);

						SHA256Managed sha256 = new SHA256Managed();
						return sha256.ComputeHash(pbData);
					}
				}
			}
			catch(Exception) { Debug.Assert(false); }

			return null;
		}
	}
}
