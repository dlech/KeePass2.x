/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2025 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.IO;
using System.Text;
using System.Threading;

using KeePassLib.Native;
using KeePassLib.Utility;

namespace KeePass.Util
{
	[Flags]
	internal enum TempClearFlags
	{
		None = 0,

		RegisteredFiles = 1,
		RegisteredDirectories = 2,

		ContentTaggedFiles = 4,

		All = 0x7FFF
	}

	public sealed class TempFilesPool
	{
		private List<string> m_lFiles = new List<string>();
		private List<KeyValuePair<string, bool>> m_lDirs =
			new List<KeyValuePair<string, bool>>();

		private readonly Dictionary<string, bool> m_dContentLoc = new Dictionary<string, bool>();
		private readonly object m_oContentLocSync = new object();

		private long m_nThreads = 0;

		private string m_strContentTag = null;
		public string TempContentTag
		{
			get
			{
				if(m_strContentTag == null)
				{
					// The tag should consist only of lower case letters
					// and digits (for maximum compatibility); it can
					// for instance be used as CSS class name
					string strL = "cfbm4v27xyk0dk5lyeq5";
					string strR = "qxi7bxozyph6qyexr9kw"; // Together ~207 bit

					// Avoid that the content tag is directly visible in
					// the source code and binaries, in case the development
					// environment creates temporary files containing the
					// tag that might then be deleted unintentionally
					StringBuilder sb = new StringBuilder();
					sb.Append(strL);
					for(int i = strR.Length - 1; i >= 0; --i)
						sb.Append(strR[i]); // Reverse

					m_strContentTag = sb.ToString();
				}

				return m_strContentTag;
			}
		}

		public TempFilesPool()
		{
		}

#if DEBUG
		~TempFilesPool()
		{
			Debug.Assert(Interlocked.Read(ref m_nThreads) == 0);
		}
#endif

		internal void Clear(TempClearFlags f)
		{
			if((f & TempClearFlags.RegisteredFiles) != TempClearFlags.None)
			{
				List<string> lFailed = new List<string>();

				foreach(string strFile in m_lFiles)
				{
					try
					{
						if(File.Exists(strFile))
							File.Delete(strFile);
					}
					catch(Exception)
					{
						Debug.Assert(false);
						lFailed.Add(strFile);
					}
				}

				m_lFiles = lFailed;
			}

			if((f & TempClearFlags.RegisteredDirectories) != TempClearFlags.None)
			{
				List<KeyValuePair<string, bool>> lFailed =
					new List<KeyValuePair<string, bool>>();

				foreach(KeyValuePair<string, bool> kvp in m_lDirs)
				{
					try
					{
						if(Directory.Exists(kvp.Key))
							Directory.Delete(kvp.Key, kvp.Value);
					}
					catch(Exception)
					{
						Debug.Assert(false);
						lFailed.Add(kvp);
					}
				}

				m_lDirs = lFailed;
			}

			if((f & TempClearFlags.ContentTaggedFiles) != TempClearFlags.None)
				ClearContentAsync();
		}

		internal void WaitForThreads()
		{
			try
			{
				while(Interlocked.Read(ref m_nThreads) > 0)
				{
					Thread.Sleep(1);
				}
			}
			catch(Exception) { Debug.Assert(false); }
		}

		public void Add(string strTempFile)
		{
			if(string.IsNullOrEmpty(strTempFile)) { Debug.Assert(false); return; }

			m_lFiles.Add(strTempFile);
		}

		public void AddDirectory(string strTempDir, bool bRecursive)
		{
			if(string.IsNullOrEmpty(strTempDir)) { Debug.Assert(false); return; }

			m_lDirs.Add(new KeyValuePair<string, bool>(strTempDir, bRecursive));
		}

		public void AddContent(string strFilePattern, bool bRecursive)
		{
			if(string.IsNullOrEmpty(strFilePattern)) { Debug.Assert(false); return; }

			lock(m_oContentLocSync)
			{
				if(m_dContentLoc.ContainsKey(strFilePattern) && !bRecursive)
					return; // Do not overwrite recursive with non-recursive

				m_dContentLoc[strFilePattern] = bRecursive;
			}
		}

		public void AddWebBrowserPrintContent()
		{
			if(!NativeLib.IsUnix())
			{
				// MSHTML may create and forget temporary files under
				// C:\\Users\\USER\\AppData\\Local\\Temp\\*.htm
				// (e.g. when printing fails)
				AddContent("*.htm", false);
			}
		}

		public string GetTempFileName()
		{
			return GetTempFileName(true);
		}

		public string GetTempFileName(bool bCreateEmptyFile)
		{
			string strFile = Path.GetTempFileName();
			m_lFiles.Add(strFile);

			if(!bCreateEmptyFile)
			{
				try { File.Delete(strFile); }
				catch(Exception) { Debug.Assert(false); }
			}

			return strFile;
		}

		public string GetTempFileName(string strFileExt)
		{
			if(string.IsNullOrEmpty(strFileExt))
				return GetTempFileName();

			try
			{
				while(true)
				{
					string str = UrlUtil.EnsureTerminatingSeparator(
						UrlUtil.GetTempPath(), false);
					str += "Temp_";

					byte[] pbRandom = new byte[9];
					Program.GlobalRandom.NextBytes(pbRandom);
					str += StrUtil.AlphaNumericOnly(Convert.ToBase64String(
						pbRandom, Base64FormattingOptions.None));

					str += "." + strFileExt;

					if(!File.Exists(str))
					{
						m_lFiles.Add(str);
						return str;
					}
				}
			}
			catch(Exception) { Debug.Assert(false); }

			return GetTempFileName();
		}

		public bool Delete(string strTempFile)
		{
			if(string.IsNullOrEmpty(strTempFile)) { Debug.Assert(false); return false; }

			int i = m_lFiles.IndexOf(strTempFile);
			if(i < 0) { Debug.Assert(false); return false; }

			try
			{
				File.Delete(strTempFile);

				m_lFiles.RemoveAt(i);
				return true;
			}
			catch(Exception) { Debug.Assert(false); }

			return false;
		}

		private void ClearContentAsync()
		{
			lock(m_oContentLocSync)
			{
				if(m_dContentLoc.Count == 0) return;
			}

			Interlocked.Increment(ref m_nThreads); // Here, not in thread
			try { ThreadPool.QueueUserWorkItem(this.ClearContentTh); }
			catch(Exception)
			{
				Debug.Assert(false);
				Interlocked.Decrement(ref m_nThreads);
			}
		}

		private void ClearContentTh(object state)
		{
			try
			{
				Debug.Assert(Interlocked.Read(ref m_nThreads) > 0);

				string strTag = m_strContentTag;
				if(string.IsNullOrEmpty(strTag)) { Debug.Assert(false); return; }

				UnicodeEncoding ue = new UnicodeEncoding(false, false, false);
				byte[] pbTagA = StrUtil.Utf8.GetBytes(strTag);
				byte[] pbTagW = ue.GetBytes(strTag);

				string strTempPath = UrlUtil.GetTempPath();

				Dictionary<string, bool> dToDo;
				lock(m_oContentLocSync)
				{
					dToDo = new Dictionary<string, bool>(m_dContentLoc);
					m_dContentLoc.Clear();
				}

				foreach(KeyValuePair<string, bool> kvp in dToDo)
				{
					bool bSuccess = false;
					try
					{
						bSuccess = ClearContentPriv(strTempPath, kvp.Key, kvp.Value,
							pbTagA, pbTagW);
					}
					catch(Exception) { Debug.Assert(false); }

					if(!bSuccess)
					{
						lock(m_oContentLocSync)
						{
							m_dContentLoc[kvp.Key] = kvp.Value; // Try again next time
						}
					}
				}
			}
			catch(Exception) { Debug.Assert(false); }
			finally { Interlocked.Decrement(ref m_nThreads); }
		}

		private bool ClearContentPriv(string strTempPath, string strFilePattern,
			bool bRecursive, byte[] pbTagA, byte[] pbTagW)
		{
			List<string> lFiles = UrlUtil.GetFilePaths(strTempPath, strFilePattern,
				(bRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
			bool bSuccess = true;

			foreach(string strFile in lFiles)
			{
				if(string.IsNullOrEmpty(strFile)) continue;
				if((strFile == ".") || (strFile == "..")) continue;

				try
				{
					byte[] pb = File.ReadAllBytes(strFile);
					if(pb == null) { Debug.Assert(false); continue; }

					if((MemUtil.IndexOf(pb, pbTagA) >= 0) ||
						(MemUtil.IndexOf(pb, pbTagW) >= 0))
					{
						File.Delete(strFile);
					}
				}
				catch(Exception) { Debug.Assert(false); bSuccess = false; }
			}

			return bSuccess;
		}
	}
}
