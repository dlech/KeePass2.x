/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2011 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePassLib.Native;

namespace KeePassLib.Utility
{
	/// <summary>
	/// A class containing various static path utility helper methods (like
	/// stripping extension from a file, etc.).
	/// </summary>
	public static class UrlUtil
	{
		private static readonly char[] m_vDirSeps = new char[] { '\\', '/',
			Path.DirectorySeparatorChar };

		/// <summary>
		/// Get the directory (path) of a file name. The returned string is
		/// terminated by a directory separator character. Example:
		/// passing <c>C:\\My Documents\\My File.kdb</c> in <paramref name="strFile" />
		/// would produce this string: <c>C:\\My Documents\\</c>.
		/// </summary>
		/// <param name="strFile">Full path of a file.</param>
		/// <param name="bAppendTerminatingChar">Append a terminating directory separator
		/// character to the returned path.</param>
		/// <param name="bEnsureValidDirSpec">If <c>true</c>, the returned path
		/// is guaranteed to be a valid directory path (for example <c>X:\\</c> instead
		/// of <c>X:</c>, overriding <paramref name="bAppendTerminatingChar" />).
		/// This should only be set to <c>true</c>, if the returned path is directly
		/// passed to some directory API.</param>
		/// <returns>Directory of the file. The return value is an empty string
		/// (<c>""</c>) if the input parameter is <c>null</c>.</returns>
		public static string GetFileDirectory(string strFile, bool bAppendTerminatingChar,
			bool bEnsureValidDirSpec)
		{
			Debug.Assert(strFile != null);
			if(strFile == null) throw new ArgumentNullException("strFile");

			int nLastSep = strFile.LastIndexOfAny(m_vDirSeps);
			if(nLastSep < 0) return strFile; // None

			if(bEnsureValidDirSpec && (nLastSep == 2) && (strFile[1] == ':') &&
				(strFile[2] == '\\')) // Length >= 3 and Windows root directory
				bAppendTerminatingChar = true;

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
			Debug.Assert(strPath != null); if(strPath == null) throw new ArgumentNullException("strPath");

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
			Debug.Assert(strPath != null); if(strPath == null) throw new ArgumentNullException("strPath");

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
			Debug.Assert(strPath != null); if(strPath == null) throw new ArgumentNullException("strPath");

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
		/// <param name="bUrl">If <c>true</c>, a slash (<c>/</c>) is appended to
		/// the string if it's not terminated already. If <c>false</c>, the
		/// default system directory separator character is used.</param>
		/// <returns>Path having a directory separator as last character.</returns>
		public static string EnsureTerminatingSeparator(string strPath, bool bUrl)
		{
			Debug.Assert(strPath != null); if(strPath == null) throw new ArgumentNullException("strPath");

			int nLength = strPath.Length;
			if(nLength <= 0) return string.Empty;

			char chLast = strPath[nLength - 1];

			for(int i = 0; i < m_vDirSeps.Length; ++i)
			{
				if(chLast == m_vDirSeps[i]) return strPath;
			}

			if(bUrl) return (strPath + '/');
			return strPath + Path.DirectorySeparatorChar;
		}

		/* /// <summary>
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
		} */

		/* /// <summary>
		/// Test if a specified path is accessible, either in read or write mode.
		/// </summary>
		/// <param name="strFilePath">Path to test.</param>
		/// <param name="fMode">Requested file access mode.</param>
		/// <returns>Returns <c>true</c> if the specified path is accessible in
		/// the requested mode, otherwise the return value is <c>false</c>.</returns>
		public static bool FileAccessible(string strFilePath, FileAccessMode fMode)
		{
			Debug.Assert(strFilePath != null);
			if(strFilePath == null) throw new ArgumentNullException("strFilePath");

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
		} */

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

		public static bool UnhideFile(string strFile)
		{
#if KeePassLibSD
			return false;
#else
			if(strFile == null) throw new ArgumentNullException("strFile");

			try
			{
				FileAttributes fa = File.GetAttributes(strFile);
				if((long)(fa & FileAttributes.Hidden) == 0) return false;

				return HideFile(strFile, false);
			}
			catch(Exception) { }

			return false;
#endif
		}

		public static bool HideFile(string strFile, bool bHide)
		{
#if KeePassLibSD
			return false;
#else
			if(strFile == null) throw new ArgumentNullException("strFile");

			try
			{
				FileAttributes fa = File.GetAttributes(strFile);

				if(bHide) fa = ((fa & ~FileAttributes.Normal) | FileAttributes.Hidden);
				else // Unhide
				{
					fa &= ~FileAttributes.Hidden;
					if((long)fa == 0) fa |= FileAttributes.Normal;
				}

				File.SetAttributes(strFile, fa);
				return true;
			}
			catch(Exception) { }

			return false;
#endif
		}

		public static string MakeRelativePath(string strBaseFile, string strTargetFile)
		{
			if(strBaseFile == null) throw new ArgumentNullException("strBasePath");
			if(strTargetFile == null) throw new ArgumentNullException("strTargetPath");
			if(strBaseFile.Length == 0) return strTargetFile;
			if(strTargetFile.Length == 0) return string.Empty;

			if(strTargetFile.StartsWith("\\\\")) return strTargetFile;

			if((strBaseFile.Length >= 3) && (strTargetFile.Length >= 3))
			{
				if((strBaseFile[1] == ':') && (strTargetFile[1] == ':') &&
					(strBaseFile[2] == '\\') && (strTargetFile[2] == '\\') &&
					(strBaseFile[0] != strTargetFile[0]))
					return strTargetFile;
			}

#if KeePassLibSD
			return strTargetFile;
#else
			try
			{
				const int nMaxPath = NativeMethods.MAX_PATH * 2;
				StringBuilder sb = new StringBuilder(nMaxPath + 2);
				if(NativeMethods.PathRelativePathTo(sb, strBaseFile, 0,
					strTargetFile, 0) == false)
					return strTargetFile;

				string str = sb.ToString();
				while(str.StartsWith(".\\")) str = str.Substring(2, str.Length - 2);

				return str;
			}
			catch(Exception) { Debug.Assert(false); return strTargetFile; }
#endif
		}

		public static string MakeAbsolutePath(string strBaseFile, string strTargetFile)
		{
			if(strBaseFile == null) throw new ArgumentNullException("strBasePath");
			if(strTargetFile == null) throw new ArgumentNullException("strTargetPath");
			if(strBaseFile.Length == 0) return strTargetFile;
			if(strTargetFile.Length == 0) return string.Empty;

			if(IsAbsolutePath(strTargetFile)) return strTargetFile;

			string strBaseDir = GetFileDirectory(strBaseFile, true, false);
			return GetShortestAbsolutePath(strBaseDir + strTargetFile);
		}

		public static bool IsAbsolutePath(string strPath)
		{
			if(strPath == null) throw new ArgumentNullException("strPath");
			if(strPath.Length == 0) return false;

			if(strPath.StartsWith("\\\\")) return true;

			try { return Path.IsPathRooted(strPath); }
			catch(Exception) { Debug.Assert(false); }

			return true;
		}

		public static string GetShortestAbsolutePath(string strPath)
		{
			if(strPath == null) throw new ArgumentNullException("strPath");
			if(strPath.Length == 0) return string.Empty;

			string str;
			try { str = Path.GetFullPath(strPath); }
			catch(Exception) { Debug.Assert(false); return strPath; }

			Debug.Assert(str.IndexOf("\\..\\") < 0);
			return str.Replace("\\.\\", "\\");
		}

		public static int GetUrlLength(string strText, int nOffset)
		{
			if(strText == null) throw new ArgumentNullException("strText");
			if(nOffset > strText.Length) throw new ArgumentException(); // Not >= (0 len)

			int iPosition = nOffset, nLength = 0, nStrLen = strText.Length;

			while(iPosition < nStrLen)
			{
				char ch = strText[iPosition];
				++iPosition;

				if((ch == ' ') || (ch == '\t') || (ch == '\r') || (ch == '\n'))
					break;

				++nLength;
			}

			return nLength;
		}

		public static string RemoveScheme(string strUrl)
		{
			if(string.IsNullOrEmpty(strUrl)) return string.Empty;

			int nNetScheme = strUrl.IndexOf(@"://", StrUtil.CaseIgnoreCmp);
			int nShScheme = strUrl.IndexOf(@":/", StrUtil.CaseIgnoreCmp);
			int nSmpScheme = strUrl.IndexOf(@":", StrUtil.CaseIgnoreCmp);

			if((nNetScheme < 0) && (nShScheme < 0) && (nSmpScheme < 0))
				return strUrl; // No scheme

			int nMin = Math.Min(Math.Min((nNetScheme >= 0) ? nNetScheme : int.MaxValue,
				(nShScheme >= 0) ? nShScheme : int.MaxValue),
				(nSmpScheme >= 0) ? nSmpScheme : int.MaxValue);

			if(nMin == nNetScheme) return strUrl.Substring(nMin + 3);
			if(nMin == nShScheme) return strUrl.Substring(nMin + 2);
			return strUrl.Substring(nMin + 1);
		}

		public static string ConvertSeparators(string strPath)
		{
			return ConvertSeparators(strPath, Path.DirectorySeparatorChar);
		}

		public static string ConvertSeparators(string strPath, char chSeparator)
		{
			if(string.IsNullOrEmpty(strPath)) return string.Empty;

			strPath = strPath.Replace('/', chSeparator);
			strPath = strPath.Replace('\\', chSeparator);

			return strPath;
		}
	}
}
