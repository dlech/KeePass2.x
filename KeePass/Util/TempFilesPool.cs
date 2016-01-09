/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2016 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.IO;
using System.Diagnostics;

namespace KeePass.Util
{
	public sealed class TempFilesPool
	{
		private List<string> m_vFiles = new List<string>();
		private List<KeyValuePair<string, bool>> m_vDirs =
			new List<KeyValuePair<string, bool>>();

		public TempFilesPool()
		{
		}

		public void Clear()
		{
			for(int i = m_vFiles.Count - 1; i >= 0; --i)
			{
				try
				{
					File.Delete(m_vFiles[i]);
					m_vFiles.RemoveAt(i);
				}
				catch(Exception) { Debug.Assert(false); }
			}

			for(int j = m_vDirs.Count - 1; j >= 0; --j)
			{
				try
				{
					Directory.Delete(m_vDirs[j].Key, m_vDirs[j].Value);
					m_vDirs.RemoveAt(j);
				}
				catch(Exception) { Debug.Assert(false); }
			}
		}

		public void Add(string strTempFile)
		{
			Debug.Assert(strTempFile != null);
			if(string.IsNullOrEmpty(strTempFile)) return;

			m_vFiles.Add(strTempFile);
		}

		public void AddDirectory(string strTempDir, bool bDeleteRecursive)
		{
			Debug.Assert(strTempDir != null);
			if(string.IsNullOrEmpty(strTempDir)) return;

			m_vDirs.Add(new KeyValuePair<string, bool>(strTempDir, bDeleteRecursive));
		}

		public string GetTempFileName()
		{
			return GetTempFileName(true);
		}

		public string GetTempFileName(bool bCreateEmptyFile)
		{
			string strFile = Path.GetTempFileName();
			m_vFiles.Add(strFile);

			if(bCreateEmptyFile == false)
			{
				try { File.Delete(strFile); }
				catch(Exception) { Debug.Assert(false); }
			}

			return strFile;
		}

		public bool Delete(string strTempFile)
		{
			Debug.Assert(strTempFile != null);
			if(strTempFile == null) return false;
			if(strTempFile.Length == 0) return false;

			int nFile = m_vFiles.IndexOf(strTempFile);
			if(nFile < 0) { Debug.Assert(false); return false; }

			bool bResult = false;
			try
			{
				File.Delete(strTempFile);

				m_vFiles.RemoveAt(nFile);
				bResult = true;
			}
			catch(Exception) { Debug.Assert(false); }

			return bResult;
		}
	}
}
