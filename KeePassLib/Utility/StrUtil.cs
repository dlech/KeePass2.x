/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2009 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Text.RegularExpressions;

using KeePassLib.Collections;
using KeePassLib.Native;
using KeePassLib.Security;
using KeePassLib.Resources;

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
			Debug.Assert(str != null);
			if(str == null) throw new ArgumentNullException("str");

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

		public char ReadChar(bool bSkipWhitespace)
		{
			if(bSkipWhitespace == false) return ReadChar();

			while(true)
			{
				char ch = ReadChar();

				if((ch != ' ') && (ch != '\t') && (ch != '\r') && (ch != '\n'))
					return ch;
			}
		}

		public char PeekChar()
		{
			if(m_nPos < 0) return char.MinValue;
			if(m_nPos >= m_strString.Length) return char.MinValue;

			return m_strString[m_nPos];
		}

		public char PeekChar(bool bSkipWhitespace)
		{
			if(bSkipWhitespace == false) return PeekChar();

			int iIndex = m_nPos;
			while(true)
			{
				if(iIndex < 0) return char.MinValue;
				if(iIndex >= m_strString.Length) return char.MinValue;

				char ch = m_strString[iIndex];

				if((ch != ' ') && (ch != '\t') && (ch != '\r') && (ch != '\n'))
					return ch;

				++iIndex;
			}
		}
	}

	/// <summary>
	/// A class containing various string helper methods.
	/// </summary>
	public static class StrUtil
	{
		/// <summary>
		/// Convert a string into a valid RTF string.
		/// </summary>
		/// <param name="str">Any string.</param>
		/// <returns>RTF-encoded string.</returns>
		public static string MakeRtfString(string str)
		{
			Debug.Assert(str != null); if(str == null) throw new ArgumentNullException("str");

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
			Debug.Assert(str != null); if(str == null) throw new ArgumentNullException("str");

			str = str.Replace(@"&", @"&amp;");
			str = str.Replace(@"<", @"&lt;");
			str = str.Replace(@">", @"&gt;");
			str = str.Replace("\"", @"&quot;");
			str = str.Replace("\'", @"&#39;");

			str = str.Replace("\r", string.Empty);
			str = str.Replace("\n", @"<br />");

			return str;
		}

		public static string XmlToString(string str)
		{
			Debug.Assert(str != null); if(str == null) throw new ArgumentNullException("str");

			str = str.Replace(@"&amp;", @"&");
			str = str.Replace(@"&lt;", @"<");
			str = str.Replace(@"&gt;", @">");
			str = str.Replace(@"&quot;", "\"");
			str = str.Replace(@"&#39;", "\'");

			return str;
		}

		public static string ReplaceCaseInsensitive(string strString, string strFind,
			string strNew)
		{
			Debug.Assert(strString != null); if(strString == null) return strString;
			Debug.Assert(strFind != null); if(strFind == null) return strString;
			Debug.Assert(strNew != null); if(strNew == null) return strString;

			string str = strString;

			int nPos = 0;
			while(nPos < str.Length)
			{
				nPos = str.IndexOf(strFind, nPos, StringComparison.OrdinalIgnoreCase);
				if(nPos < 0) break;

				str = str.Remove(nPos, strFind.Length);
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
		public static string FormatException(Exception excp)
		{
			string strText = string.Empty;
			
			if(excp.Message != null)
				strText += excp.Message + MessageService.NewLine;
#if !KeePassLibSD
			if(excp.Source != null)
				strText += excp.Source + MessageService.NewLine;
#endif
			if(excp.StackTrace != null)
				strText += excp.StackTrace + MessageService.NewLine;
#if !KeePassLibSD
			if(excp.TargetSite != null)
				strText += excp.TargetSite.ToString() + MessageService.NewLine;

			if(excp.Data != null)
			{
				strText += MessageService.NewLine;
				foreach(DictionaryEntry de in excp.Data)
					strText += @"'" + de.Key + @"' -> '" + de.Value + @"'" +
						MessageService.NewLine;
			}
#endif

			if(excp.InnerException != null)
			{
				strText += MessageService.NewLine + "Inner:" + MessageService.NewLine;
				if(excp.InnerException.Message != null)
					strText += excp.InnerException.Message + MessageService.NewLine;
#if !KeePassLibSD
				if(excp.InnerException.Source != null)
					strText += excp.InnerException.Source + MessageService.NewLine;
#endif
				if(excp.InnerException.StackTrace != null)
					strText += excp.InnerException.StackTrace + MessageService.NewLine;
#if !KeePassLibSD
				if(excp.InnerException.TargetSite != null)
					strText += excp.InnerException.TargetSite.ToString();

				if(excp.InnerException.Data != null)
				{
					strText += MessageService.NewLine;
					foreach(DictionaryEntry de in excp.InnerException.Data)
						strText += @"'" + de.Key + @"' -> '" + de.Value + @"'" +
							MessageService.NewLine;
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

		public static string CompactString3Dots(string strText, int nMaxChars)
		{
			Debug.Assert(strText != null);
			if(strText == null) throw new ArgumentNullException("strText");
			Debug.Assert(nMaxChars >= 0);
			if(nMaxChars < 0) throw new ArgumentOutOfRangeException("nMaxChars");

			if(nMaxChars == 0) return string.Empty;
			if(strText.Length <= nMaxChars) return strText;

			if(nMaxChars <= 3) return strText.Substring(0, nMaxChars);

			return strText.Substring(0, nMaxChars - 3) + "...";
		}

		public static string GetStringBetween(string strText, int nStartIndex,
			string strStart, string strEnd)
		{
			int nTemp;
			return GetStringBetween(strText, nStartIndex, strStart, strEnd, out nTemp);
		}

		public static string GetStringBetween(string strText, int nStartIndex,
			string strStart, string strEnd, out int nInnerStartIndex)
		{
			if(strText == null) throw new ArgumentNullException("strText");
			if(strStart == null) throw new ArgumentNullException("strStart");
			if(strEnd == null) throw new ArgumentNullException("strEnd");

			nInnerStartIndex = -1;

			int nIndex = strText.IndexOf(strStart, nStartIndex);
			if(nIndex < 0) return string.Empty;

			nIndex += strStart.Length;

			int nEndIndex = strText.IndexOf(strEnd, nIndex);
			if(nEndIndex < 0) return string.Empty;

			nInnerStartIndex = nIndex;
			return strText.Substring(nIndex, nEndIndex - nIndex);
		}

		/// <summary>
		/// Removes all characters that are not valid XML characters,
		/// according to http://www.w3.org/TR/xml/#charsets .
		/// </summary>
		/// <param name="strText">Source text.</param>
		/// <returns>Text containing only valid XML characters.</returns>
		public static string SafeXmlString(string strText)
		{
			Debug.Assert(strText != null); // No throw
			if(string.IsNullOrEmpty(strText)) return strText;

			char[] vChars = strText.ToCharArray();
			StringBuilder sb = new StringBuilder(strText.Length, strText.Length);
			char ch;

			for(int i = 0; i < vChars.Length; ++i)
			{
				ch = vChars[i];

				if(((ch >= 0x20) && (ch <= 0xD7FF)) ||
					(ch == 0x9) || (ch == 0xA) || (ch == 0xD) ||
					((ch >= 0xE000) && (ch <= 0xFFFD)))
					sb.Append(ch);
				// Range ((ch >= 0x10000) && (ch <= 0x10FFFF)) excluded
			}

			return sb.ToString();
		}

		private static Regex m_rxNaturalSplit = null;
		public static int CompareNaturally(string strX, string strY)
		{
			Debug.Assert(strX != null);
			if(strX == null) throw new ArgumentNullException("strX");
			Debug.Assert(strY != null);
			if(strY == null) throw new ArgumentNullException("strY");

			if(NativeMethods.SupportsStrCmpNaturally)
				return NativeMethods.StrCmpNaturally(strX, strY);

			strX = strX.ToLower();
			strY = strY.ToLower();

			if(m_rxNaturalSplit == null)
				m_rxNaturalSplit = new Regex(@"([0-9]+)", RegexOptions.Compiled);

			string[] vPartsX = m_rxNaturalSplit.Split(strX);
			string[] vPartsY = m_rxNaturalSplit.Split(strY);

			for(int i = 0; i < Math.Min(vPartsX.Length, vPartsY.Length); ++i)
			{
				string strPartX = vPartsX[i], strPartY = vPartsY[i];
				int iPartCompare;

#if KeePassLibSD
				ulong uX = 0, uY = 0;
				try
				{
					uX = ulong.Parse(strPartX);
					uY = ulong.Parse(strPartY);
					iPartCompare = uX.CompareTo(uY);
				}
				catch(Exception) { iPartCompare = strPartX.CompareTo(strPartY); }
#else
				ulong uX, uY;
				if(ulong.TryParse(strPartX, out uX) && ulong.TryParse(strPartY, out uY))
					iPartCompare = uX.CompareTo(uY);
				else iPartCompare = strPartX.CompareTo(strPartY);
#endif

				if(iPartCompare != 0) return iPartCompare;
			}

			if(vPartsX.Length == vPartsY.Length) return 0;
			if(vPartsX.Length < vPartsY.Length) return -1;
			return 1;
		}

		public static string RemoveAccelerator(string strMenuText)
		{
			if(strMenuText == null) throw new ArgumentNullException("strMenuText");

			string str = strMenuText;

			for(char ch = 'A'; ch <= 'Z'; ++ch)
			{
				string strEnhAcc = @"(&" + ch.ToString() + @")";

				if(str.IndexOf(strEnhAcc) >= 0)
				{
					str = str.Replace(@" " + strEnhAcc, string.Empty);
					str = str.Replace(strEnhAcc, string.Empty);
				}
			}

			str = str.Replace(@"&", string.Empty);

			return str;
		}

		public static bool IsHexString(string str, bool bStrict)
		{
			if(str == null) throw new ArgumentNullException("str");
			if(str.Length == 0) return true;

			foreach(char ch in str)
			{
				if((ch >= '0') && (ch <= '9')) continue;
				if((ch >= 'a') && (ch <= 'z')) continue;
				if((ch >= 'A') && (ch <= 'Z')) continue;

				if(bStrict) return false;

				if((ch == ' ') || (ch == '\t') || (ch == '\r') || (ch == '\n'))
					continue;

				return false;
			}

			return true;
		}

#if !KeePassLibSD
		public static bool SimplePatternMatch(string strPattern, string strText,
			StringComparison sc)
		{
			if(strPattern == null) throw new ArgumentNullException("strPattern");
			if(strText == null) throw new ArgumentNullException("strText");

			if(strPattern.IndexOf('*') < 0) return strText.Equals(strPattern, sc);

			string[] vPatternParts = strPattern.Split(new char[]{ '*' },
				StringSplitOptions.RemoveEmptyEntries);
			if(vPatternParts == null) { Debug.Assert(false); return true; }
			if(vPatternParts.Length == 0) return true;

			if(strText.Length == 0) return false;

			if(!strPattern.StartsWith(@"*") && !strText.StartsWith(vPatternParts[0], sc))
			{
				return false;
			}

			if(!strPattern.EndsWith(@"*") && !strText.EndsWith(vPatternParts[
				vPatternParts.Length - 1], sc))
			{
				return false;
			}

			int iOffset = 0;
			for(int i = 0; i < vPatternParts.Length; ++i)
			{
				string strPart = vPatternParts[i];

				int iFound = strText.IndexOf(strPart, iOffset, sc);
				if(iFound < iOffset) return false;

				iOffset = iFound + strPart.Length;
				if(iOffset == strText.Length) return (i == (vPatternParts.Length - 1));
			}

			return true;
		}
#endif // !KeePassLibSD

		public static bool StringToBool(string str)
		{
			if(string.IsNullOrEmpty(str)) return false; // No assert

			string s = str.ToLower().Trim();
			if(s == "true") return true;
			if(s == "yes") return true;
			if(s == "1") return true;
			if(s == "enabled") return true;
			if(s == "checked") return true;

			return false;
		}

		public static string BoolToString(bool bValue)
		{
			return (bValue ? "true" : "false");
		}

		public static string ToWindowsString(string str)
		{
			if(string.IsNullOrEmpty(str)) return str;

			string strSingular = str.Replace("\r", string.Empty);
			return strSingular.Replace("\n", "\r\n");
		}
	}
}
