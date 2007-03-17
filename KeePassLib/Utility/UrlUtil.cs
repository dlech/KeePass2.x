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
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace KeePassLib.Utility
{
	/// <summary>
	/// A class containing various static path utility helper methods (like
	/// stripping extension from a file, etc.).
	/// </summary>
	public static class UrlUtil
	{
		private static readonly char[] m_vDirSeps = new char[] { '\\', '/', Path.DirectorySeparatorChar };

		/// <summary>
		/// Get the directory (path) of a file name. The returned string is
		/// terminated by a directory separator character. Example:
		/// passing <c>C:\\My Documents\\My File.kdb</c> in <paramref name="strFile" />
		/// would produce this string: <c>C:\\My Documents\\</c>.
		/// </summary>
		/// <param name="strFile">Full path of a file.</param>
		/// <param name="bAppendTerminatingChar">Append a terminating directory separator
		/// character to the returned path.</param>
		/// <returns>Directory of the file. The return value is an empty string
		/// (<c>""</c>) if the input parameter is <c>null</c>.</returns>
		public static string GetFileDirectory(string strFile, bool bAppendTerminatingChar)
		{
			Debug.Assert(strFile != null);
			if(strFile == null) throw new ArgumentNullException("strFile");

			int nLastSep = strFile.LastIndexOfAny(m_vDirSeps);
			if(nLastSep <= 2) return strFile; // None or X:\\

			if(!bAppendTerminatingChar) return strFile.Substring(0, nLastSep);
			return EnsureTerminatingSeparator(strFile.Substring(0, nLastSep), false);
		}

		/// <summary>
		/// Gets the file name of the specified file (full path). Example:
		/// if <paramref name="strPath" /> is <c>C:\\My Documents\\My File.kdb</c>
		/// the returned string is <c>My File.kdb</c>.
		/// </summary>
		/// <param name="strPath">Full path of a file.</param>
		/// <returns>File name of the specified file. The return value is
		/// an empty string (<c>""</c>) if the input parameter is <c>null</c>.</returns>
		public static string GetFileName(string strPath)
		{
			Debug.Assert(strPath != null); if(strPath == null) throw new ArgumentNullException();

			int nLastSep = strPath.LastIndexOfAny(m_vDirSeps);

			if(nLastSep < 0) return strPath;
			if(nLastSep >= (strPath.Length - 1)) return string.Empty;

			return strPath.Substring(nLastSep + 1);
		}

		/// <summary>
		/// Strip the extension of a file.
		/// </summary>
		/// <param name="strPath">Full path of a file with extension.</param>
		/// <returns>File name without extension. The return value is
		/// an empty string (<c>""</c>) if the input parameter is <c>null</c>.</returns>
		public static string StripExtension(string strPath)
		{
			Debug.Assert(strPath != null); if(strPath == null) throw new ArgumentNullException();

			int nLastDirSep = strPath.LastIndexOfAny(m_vDirSeps);
			int nLastExtDot = strPath.LastIndexOf('.');

			if(nLastExtDot <= nLastDirSep) return strPath;

			return strPath.Substring(0, nLastExtDot);
		}

		/// <summary>
		/// Get the extension of a file.
		/// </summary>
		/// <param name="strPath">Full path of a file with extension.</param>
		/// <returns>Extension without prepending dot.</returns>
		public static string GetExtension(string strPath)
		{
			Debug.Assert(strPath != null); if(strPath == null) throw new ArgumentNullException();

			int nLastDirSep = strPath.LastIndexOfAny(m_vDirSeps);
			int nLastExtDot = strPath.LastIndexOf('.');

			if(nLastExtDot <= nLastDirSep) return string.Empty;
			if(nLastExtDot == (strPath.Length - 1)) return string.Empty;

			return strPath.Substring(nLastExtDot + 1);
		}

		/// <summary>
		/// Ensure that a path is terminated with a directory separator character.
		/// </summary>
		/// <param name="strPath">Input path.</param>
		/// <param name="bURL">If <c>true</c>, a slash (<c>/</c>) is appended to
		/// the string if it's not terminated already. If <c>false</c>, the
		/// default system directory separator character is used.</param>
		/// <returns>Path having a directory separator as last character.</returns>
		public static string EnsureTerminatingSeparator(string strPath, bool bURL)
		{
			Debug.Assert(strPath != null); if(strPath == null) throw new ArgumentNullException();

			int nLength = strPath.Length;
			if(nLength <= 0) return string.Empty;

			char chLast = strPath[nLength - 1];

			for(int i = 0; i < m_vDirSeps.Length; i++)
				if(chLast == m_vDirSeps[i])
					return strPath;

			if(bURL) return strPath + '/';
			return strPath + Path.DirectorySeparatorChar;
		}

		[DllImport("ShlWApi.dll", CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool PathCompactPathEx(StringBuilder pszOut, string szPath, uint cchMax, uint dwFlags);

		/// <summary>
		/// Shorten a path.
		/// </summary>
		/// <param name="strPath">Path to make shorter.</param>
		/// <param name="uMaxChars">Maximum number of characters in the returned string.</param>
		/// <returns>Shortened path.</returns>
		public static string CompactPath(string strPath, uint uMaxChars)
		{
			Debug.Assert(strPath != null); if(strPath == null) throw new ArgumentNullException("strPath");

			if(strPath.Length <= (int)uMaxChars) return strPath;
			try
			{
				StringBuilder sb = new StringBuilder(strPath.Length * 2 + 2);

				if(UrlUtil.PathCompactPathEx(sb, strPath, uMaxChars, 0) == false)
					return strPath;

				Debug.Assert(sb.Length <= uMaxChars);
				return sb.ToString();
			}
			catch(Exception) { return strPath; }
		}

		/// <summary>
		/// File access mode enumeration. Used by the <c>FileAccessible</c>
		/// method.
		/// </summary>
		public enum FileAccessMode
		{
			/// <summary>
			/// Opening a file in read mode. The specified file must exist.
			/// </summary>
			Read = 0,

			/// <summary>
			/// Opening a file in create mode. If the file exists already, it
			/// will be overwritten. If it doesn't exist, it will be created.
			/// The return value is <c>true</c>, if data can be written to the
			/// file.
			/// </summary>
			Create
		}

		/// <summary>
		/// Test if a specified path is accessible, either in read or write mode.
		/// </summary>
		/// <param name="strFilePath">Path to test.</param>
		/// <param name="fMode">Requested file access mode.</param>
		/// <returns>Returns <c>true</c> if the specified path is accessible in
		/// the requested mode, otherwise the return value is <c>false</c>.</returns>
		public static bool FileAccessible(string strFilePath, FileAccessMode fMode)
		{
			Debug.Assert(strFilePath != null); if(strFilePath == null) throw new ArgumentNullException();

			if(fMode == FileAccessMode.Read)
			{
				FileStream fs;

				try { fs = File.OpenRead(strFilePath); }
				catch(Exception) { return false; }
				if(fs == null) return false;

				fs.Close();
				return true;
			}
			else if(fMode == FileAccessMode.Create)
			{
				FileStream fs;

				try { fs = File.Create(strFilePath); }
				catch(Exception) { return false; }
				if(fs == null) return false;

				fs.Close();
				return true;
			}

			return false;
		}

		public static string GetQuotedAppPath(string strPath)
		{
			int nFirst = strPath.IndexOf('\"');
			int nSecond = strPath.IndexOf('\"', nFirst + 1);

			if((nFirst >= 0) && (nSecond >= 0))
				return strPath.Substring(nFirst + 1, nSecond - nFirst - 1);

			return strPath;
		}

		public static string FileUrlToPath(string strUrl)
		{
			Debug.Assert(strUrl != null);
			if(strUrl == null) throw new ArgumentNullException("strUrl");

			string str = strUrl;
			if(str.ToLower().StartsWith(@"file:///"))
				str = str.Substring(8, str.Length - 8);

			str = str.Replace('/', Path.DirectorySeparatorChar);

			return str;
		}
	}
}
