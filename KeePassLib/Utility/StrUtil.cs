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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Drawing;
using System.IO;

using KeePassLib.Security;

namespace KeePassLib.Utility
{
	/// <summary>
	/// Character stream class.
	/// </summary>
	public sealed class CharStream
	{
		private string m_strString = string.Empty;
		private int m_nPos = 0;

		public CharStream(string str)
		{
			Debug.Assert(str != null); if(str == null) throw new ArgumentNullException();
			m_strString = str;
		}

		public void Seek(SeekOrigin org, int nSeek)
		{
			if(org == SeekOrigin.Begin)
				m_nPos = nSeek;
			else if(org == SeekOrigin.Current)
				m_nPos += nSeek;
			else if(org == SeekOrigin.End)
				m_nPos = m_strString.Length + nSeek;
		}

		public char ReadChar()
		{
			if(m_nPos < 0) return char.MinValue;
			if(m_nPos >= m_strString.Length) return char.MinValue;

			char chRet = m_strString[m_nPos];
			++m_nPos;
			return chRet;
		}
	}

	/// <summary>
	/// A class containing various string helper methods.
	/// </summary>
	public static class StrUtil
	{
		private static string m_localizedExceptionOccured = "An exception occured.";

		/// <summary>
		/// IDs of localizable strings.
		/// </summary>
		public enum LocalizedStringID
		{
			/// <summary>
			/// ID of a string representing a message that an exception has occured.
			/// </summary>
			ExceptionOccured = 0
		}

		/// <summary>
		/// Set a localized version of a string.
		/// </summary>
		/// <param name="id">ID of the string to set.</param>
		/// <param name="strText">New, localized text.</param>
		public static void SetLocalizedString(LocalizedStringID id, string strText)
		{
			if(id == LocalizedStringID.ExceptionOccured)
				m_localizedExceptionOccured = strText;
			else { Debug.Assert(false); }
		}

		/// <summary>
		/// Convert a string into a valid RTF string.
		/// </summary>
		/// <param name="str">Any string.</param>
		/// <returns>RTF-encoded string.</returns>
		public static string MakeRtfString(string str)
		{
			Debug.Assert(str != null); if(str == null) throw new ArgumentNullException();

			str = str.Replace("\\", "\\\\");
			str = str.Replace("\r", "");
			str = str.Replace("{", "\\{");
			str = str.Replace("}", "\\}");
			str = str.Replace("\n", "\\par ");

			return str;
		}

		/// <summary>
		/// Convert a string into a valid HTML sequence representing that string.
		/// </summary>
		/// <param name="str">String to convert.</param>
		/// <returns>String, HTML-encoded.</returns>
		public static string StringToHtml(string str)
		{
			Debug.Assert(str != null); if(str == null) throw new ArgumentNullException();

			str = str.Replace(@"&", @"&amp;");
			str = str.Replace(@"<", @"&lt;");
			str = str.Replace(@">", @"&gt;");
			str = str.Replace("\"", @"&quot;");
			str = str.Replace("\'", @"&#39;");

			str = str.Replace("\r", string.Empty);
			str = str.Replace("\n", @"<br />");

			return str;
		}

		/// <summary>
		/// Search for a substring case-insensitively and replace it by some new string.
		/// </summary>
		/// <param name="strString">Base string, which will be searched.</param>
		/// <param name="strToReplace">The string to search for (and to replace).</param>
		/// <param name="psNew">New replacement string. Must not be <c>null</c>.</param>
		/// <param name="bCmdQuotes">If <c>true</c>, quotes will be replaced by
		/// command-line friendly escape sequences.</param>
		/// <returns>Modified string object.</returns>
		public static string ReplaceCaseInsensitive(string strString, string strToReplace,
			ProtectedString psNew, bool bCmdQuotes)
		{
			Debug.Assert(strString != null); if(strString == null) return null;
			Debug.Assert(strToReplace != null); if(strToReplace == null) return strString;
			Debug.Assert(psNew != null); if(psNew == null) return strString;

			string str = strString, strNew = psNew.ReadString();
			int nPos = 0;

			if(bCmdQuotes) strNew = strNew.Replace("\"", "\"\"\"");

			while(true)
			{
				nPos = str.IndexOf(strToReplace, nPos, StringComparison.OrdinalIgnoreCase);
				if(nPos < 0) break;

				str = str.Remove(nPos, strToReplace.Length);
				str = str.Insert(nPos, strNew);

				nPos += strNew.Length;
			}

			return str;
		}

		/// <summary>
		/// Split up a command-line into application and argument.
		/// </summary>
		/// <param name="strCmdLine">Command-line to split.</param>
		/// <param name="strApp">Application path.</param>
		/// <param name="strArgs">Arguments.</param>
		public static void SplitCommandLine(string strCmdLine, out string strApp, out string strArgs)
		{
			Debug.Assert(strCmdLine != null); if(strCmdLine == null) throw new ArgumentNullException("strCmdLine");

			string str = strCmdLine.Trim();

			strApp = null; strArgs = null;

			if(str.StartsWith("\""))
			{
				int nSecond = str.IndexOf('\"', 1);
				if(nSecond >= 1)
				{
					strApp = str.Substring(1, nSecond - 1).Trim();
					strArgs = str.Remove(0, nSecond + 1).Trim();
				}
			}

			if(strApp == null)
			{
				int nSpace = str.IndexOf(' ');

				if(nSpace >= 0)
				{
					strApp = str.Substring(0, nSpace);
					strArgs = str.Remove(0, nSpace).Trim();
				}
				else strApp = strCmdLine;
			}

			if(strApp == null) strApp = string.Empty;
			if(strArgs == null) strArgs = string.Empty;
		}

		/// <summary>
		/// Fill in all placeholders in a string using entry information.
		/// </summary>
		/// <param name="strSeq">String containing placeholders.</param>
		/// <param name="pe">Entry to retrieve the data from.</param>
		/// <param name="strAppPath">Path of the current executable file.</param>
		/// <param name="pwDatabase">Current database.</param>
		/// <param name="bCmdQuotes">If <c>true</c>, quotes will be replaced by
		/// command-line friendly escape sequences.</param>
		/// <returns>Returns the new string.</returns>
		public static string FillPlaceholders(string strSeq, PwEntry pe,
			string strAppPath, PwDatabase pwDatabase, bool bCmdQuotes)
		{
			string str = strSeq;

			for(int i = 0; i < 2; ++i) // Two-pass replacement
			{
				if(pe != null)
				{
					foreach(KeyValuePair<string, ProtectedString> kvp in pe.Strings)
					{
						string strKey = PwDefs.IsStandardField(kvp.Key) ?
							(@"{" + kvp.Key + @"}") :
							(@"{" + PwDefs.AutoTypeStringPrefix + kvp.Key + @"}");

						str = StrUtil.ReplaceCaseInsensitive(str, strKey, kvp.Value, bCmdQuotes);
					}

					if(pe.ParentGroup != null)
						str = StrUtil.ReplaceCaseInsensitive(str, @"{GROUP}", new ProtectedString(
							false, pe.ParentGroup.Name), bCmdQuotes);
				}

				str = StrUtil.ReplaceCaseInsensitive(str, @"{APPDIR}", new ProtectedString(
					false, UrlUtil.GetFileDirectory(strAppPath, false)), bCmdQuotes);

				if(pwDatabase != null)
				{
					str = StrUtil.ReplaceCaseInsensitive(str, @"{DOCDIR}", new ProtectedString(
						false, UrlUtil.GetFileDirectory(pwDatabase.IOConnectionInfo.Url, false)), bCmdQuotes);
				}
			}

#if !KeePassLibSD
			// Replace environment variables
			foreach(DictionaryEntry de in Environment.GetEnvironmentVariables())
			{
				string strKey = de.Key as string;
				string strValue = de.Value as string;

				if((strKey != null) && (strValue != null))
					str = StrUtil.ReplaceCaseInsensitive(str, @"%" + strKey +
						@"%", new ProtectedString(false, strValue), false);
				else { Debug.Assert(false); }
			}
#endif

			return str;
		}

		/// <summary>
		/// Initialize an RTF document based on given font face and size.
		/// </summary>
		/// <param name="sb"><c>StringBuilder</c> to put the generated RTF into.</param>
		/// <param name="strFontFace">Face name of the font to use.</param>
		/// <param name="fFontSize">Size of the font to use.</param>
		public static void InitRtf(StringBuilder sb, string strFontFace, float fFontSize)
		{
			Debug.Assert(sb != null); if(sb == null) throw new ArgumentNullException("sb");
			Debug.Assert(strFontFace != null); if(strFontFace == null) throw new ArgumentNullException("strFontFace");

			sb.Append("{\\rtf1\\ansi\\ansicpg");
			sb.Append(Encoding.Default.CodePage);
			sb.Append("\\deff0{\\fonttbl{\\f0\\fswiss MS Sans Serif;}{\\f1\\froman\\fcharset2 Symbol;}{\\f2\\fswiss ");
			sb.Append(strFontFace);
			sb.Append(";}{\\f3\\fswiss Arial;}}");
			sb.Append("{\\colortbl\\red0\\green0\\blue0;}");
			sb.Append("\\deflang1031\\pard\\plain\\f2\\cf0 ");
			sb.Append("\\fs");
			sb.Append((int)(fFontSize * 2));
		}

		/// <summary>
		/// Convert a simple HTML string to an RTF string.
		/// </summary>
		/// <param name="strHtmlString">Input HTML string.</param>
		/// <returns>RTF string representing the HTML input string.</returns>
		public static string SimpleHtmlToRtf(string strHtmlString)
		{
			StringBuilder sb = new StringBuilder();

			StrUtil.InitRtf(sb, "Microsoft Sans Serif", 8.25f);
			sb.Append(" ");

			string str = MakeRtfString(strHtmlString);
			str = str.Replace("<b>", "\\b ");
			str = str.Replace("</b>", "\\b0 ");
			str = str.Replace("<i>", "\\i ");
			str = str.Replace("</i>", "\\i0 ");
			str = str.Replace("<u>", "\\ul ");
			str = str.Replace("</u>", "\\ul0 ");
			str = str.Replace("<br />", "\\par ");

			sb.Append(str);
			return sb.ToString();
		}

		/// <summary>
		/// Convert a <c>Color</c> to a HTML color identifier string.
		/// </summary>
		/// <param name="color">Color to convert.</param>
		/// <param name="bEmptyIfTransparent">If this is <c>true</c>, an empty string
		/// is returned if the color is transparent.</param>
		/// <returns>HTML color identifier string.</returns>
		public static string ColorToUnnamedHtml(Color color, bool bEmptyIfTransparent)
		{
			if(bEmptyIfTransparent && (color.A != 255))
				return string.Empty;

			StringBuilder sb = new StringBuilder();
			byte bt;

			sb.Append('#');

			bt = (byte)(color.R >> 4);
			if(bt < 10) sb.Append((char)('0' + bt)); else sb.Append((char)('A' - 10 + bt));
			bt = (byte)(color.R & 0x0F);
			if(bt < 10) sb.Append((char)('0' + bt)); else sb.Append((char)('A' - 10 + bt));

			bt = (byte)(color.G >> 4);
			if(bt < 10) sb.Append((char)('0' + bt)); else sb.Append((char)('A' - 10 + bt));
			bt = (byte)(color.G & 0x0F);
			if(bt < 10) sb.Append((char)('0' + bt)); else sb.Append((char)('A' - 10 + bt));

			bt = (byte)(color.B >> 4);
			if(bt < 10) sb.Append((char)('0' + bt)); else sb.Append((char)('A' - 10 + bt));
			bt = (byte)(color.B & 0x0F);
			if(bt < 10) sb.Append((char)('0' + bt)); else sb.Append((char)('A' - 10 + bt));

			return sb.ToString();
		}

		/// <summary>
		/// Format an exception and convert it to a string.
		/// </summary>
		/// <param name="excp"><c>Exception</c> to convert/format.</param>
		/// <param name="bHeaderText">If this is <c>true</c>, a header text is prepended
		/// to the result string. This text is a generic, localized error message.</param>
		/// <returns>String representing the exception.</returns>
		public static string FormatException(Exception excp, bool bHeaderText)
		{
			string strText = string.Empty;
			
			if(bHeaderText)
				strText += m_localizedExceptionOccured + "\r\n\r\n";

			if(excp.Message != null)
				strText += excp.Message + "\r\n";
#if !KeePassLibSD
			if(excp.Source != null)
				strText += excp.Source + "\r\n";
#endif
			if(excp.StackTrace != null)
				strText += excp.StackTrace + "\r\n";
#if !KeePassLibSD
			if(excp.TargetSite != null)
				strText += excp.TargetSite.ToString() + "\r\n";

			if(excp.Data != null)
			{
				strText += "\r\n";
				foreach(DictionaryEntry de in excp.Data)
					strText += @"'" + de.Key + @"' -> '" + de.Value + "'\r\n";
			}
#endif

			if(excp.InnerException != null)
			{
				strText += "\r\nInner:\r\n";
				if(excp.InnerException.Message != null)
					strText += excp.InnerException.Message + "\r\n";
#if !KeePassLibSD
				if(excp.InnerException.Source != null)
					strText += excp.InnerException.Source + "\r\n";
#endif
				if(excp.InnerException.StackTrace != null)
					strText += excp.InnerException.StackTrace + "\r\n";
#if !KeePassLibSD
				if(excp.InnerException.TargetSite != null)
					strText += excp.InnerException.TargetSite.ToString();

				if(excp.InnerException.Data != null)
				{
					strText += "\r\n";
					foreach(DictionaryEntry de in excp.InnerException.Data)
						strText += @"'" + de.Key + @"' -> '" + de.Value + "'\r\n";
				}
#endif
			}

			return strText;
		}

		public static bool TryParseInt(string str, out int n)
		{
#if !KeePassLibSD
			return int.TryParse(str, out n);
#else
			try { n = int.Parse(str); return true; }
			catch(Exception) { n = 0; return false; }
#endif
		}

		public static bool TryParseUInt(string str, out uint u)
		{
#if !KeePassLibSD
			return uint.TryParse(str, out u);
#else
			try { u = uint.Parse(str); return true; }
			catch(Exception) { u = 0; return false; }
#endif
		}

		public static bool TryParseULong(string str, out ulong u)
		{
#if !KeePassLibSD
			return ulong.TryParse(str, out u);
#else
			try { u = ulong.Parse(str); return true; }
			catch(Exception) { u = 0; return false; }
#endif
		}

		public static bool TryParseDateTime(string str, out DateTime dt)
		{
#if !KeePassLibSD
			return DateTime.TryParse(str, out dt);
#else
			try { dt = DateTime.Parse(str); return true; }
			catch(Exception) { dt = DateTime.MinValue; return false; }
#endif
		}
	}
}
