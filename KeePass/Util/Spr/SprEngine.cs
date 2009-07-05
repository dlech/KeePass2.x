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
using System.Text;
using System.IO;
using System.Diagnostics;

using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Security;
using KeePassLib.Utility;

using SprRefsCache = System.Collections.Generic.Dictionary<string, string>;

namespace KeePass.Util.Spr
{
	/// <summary>
	/// String placeholders and field reference replacement engine.
	/// </summary>
	public static class SprEngine
	{
		private const uint MaxRecursionDepth = 12;
		private const StringComparison ScMethod = StringComparison.OrdinalIgnoreCase;

		private static string m_strAppExePath = string.Empty;

		private static void InitializeStatic()
		{
			m_strAppExePath = WinUtil.GetExecutable();
		}

		public static string Compile(string strText, bool bIsAutoTypeSequence,
			PwEntry pwEntry, PwDatabase pwDatabase, bool bEscapeForAutoType,
			bool bEscapeQuotesForCommandLine)
		{
			if(strText == null) { Debug.Assert(false); return string.Empty; }
			if(strText.Length == 0) return string.Empty;

			SprEngine.InitializeStatic();

			SprContentFlags cf = new SprContentFlags(bEscapeForAutoType &&
				bIsAutoTypeSequence, bEscapeQuotesForCommandLine);

			SprRefsCache vRefsCache = new SprRefsCache();

			string str = SprEngine.CompileInternal(strText, pwEntry, pwDatabase,
				cf, 0, vRefsCache);

			if(bEscapeForAutoType && (bIsAutoTypeSequence == false))
				str = SprEncoding.MakeAutoTypeSequence(str);

			return str;
		}

		private static string CompileInternal(string strText, PwEntry pwEntry,
			PwDatabase pwDatabase, SprContentFlags cf, uint uRecursionLevel,
			SprRefsCache vRefsCache)
		{
			if(strText == null) { Debug.Assert(false); return string.Empty; }

			if(uRecursionLevel >= SprEngine.MaxRecursionDepth)
			{
				Debug.Assert(false); // Most likely a recursive reference
				return string.Empty; // Do not return strText
			}

			string str = strText;
			str = AppLocator.FillPlaceholders(str, cf);
			str = EntryUtil.FillPlaceholders(str, pwEntry, pwDatabase, cf);

			if(pwEntry != null)
			{
				foreach(KeyValuePair<string, ProtectedString> kvp in pwEntry.Strings)
				{
					string strKey = PwDefs.IsStandardField(kvp.Key) ?
						(@"{" + kvp.Key + @"}") :
						(@"{" + PwDefs.AutoTypeStringPrefix + kvp.Key + @"}");

					str = SprEngine.FillIfExists(str, strKey, kvp.Value, pwEntry,
						pwDatabase, cf, uRecursionLevel, vRefsCache);
				}

				if(pwEntry.ParentGroup != null)
				{
					str = SprEngine.FillIfExists(str, @"{GROUP}", new ProtectedString(
						false, pwEntry.ParentGroup.Name), pwEntry, pwDatabase,
						cf, uRecursionLevel, vRefsCache);

					str = SprEngine.FillIfExists(str, @"{GROUPPATH}", new ProtectedString(
						false, pwEntry.ParentGroup.GetFullPath()), pwEntry, pwDatabase,
						cf, uRecursionLevel, vRefsCache);
				}
			}

			if(m_strAppExePath != null)
				str = SprEngine.FillIfExists(str, @"{APPDIR}", new ProtectedString(
					false, UrlUtil.GetFileDirectory(m_strAppExePath, false, false)),
					pwEntry, pwDatabase, cf, uRecursionLevel, vRefsCache);

			if(pwDatabase != null)
			{
				// For backward compatibility only
				str = SprEngine.FillIfExists(str, @"{DOCDIR}", new ProtectedString(
					false, UrlUtil.GetFileDirectory(pwDatabase.IOConnectionInfo.Path,
					false, false)), pwEntry, pwDatabase, cf, uRecursionLevel, vRefsCache);

				str = SprEngine.FillIfExists(str, @"{DB_PATH}", new ProtectedString(
					false, pwDatabase.IOConnectionInfo.Path), pwEntry, pwDatabase,
					cf, uRecursionLevel, vRefsCache);
				str = SprEngine.FillIfExists(str, @"{DB_DIR}", new ProtectedString(
					false, UrlUtil.GetFileDirectory(pwDatabase.IOConnectionInfo.Path,
					false, false)), pwEntry, pwDatabase, cf, uRecursionLevel, vRefsCache);
				str = SprEngine.FillIfExists(str, @"{DB_NAME}", new ProtectedString(
					false, UrlUtil.GetFileName(pwDatabase.IOConnectionInfo.Path)),
					pwEntry, pwDatabase, cf, uRecursionLevel, vRefsCache);
				str = SprEngine.FillIfExists(str, @"{DB_BASENAME}", new ProtectedString(
					false, UrlUtil.StripExtension(UrlUtil.GetFileName(
					pwDatabase.IOConnectionInfo.Path))), pwEntry, pwDatabase, cf,
					uRecursionLevel, vRefsCache);
				str = SprEngine.FillIfExists(str, @"{DB_EXT}", new ProtectedString(
					false, UrlUtil.GetExtension(pwDatabase.IOConnectionInfo.Path)),
					pwEntry, pwDatabase, cf, uRecursionLevel, vRefsCache);
			}

			str = SprEngine.FillIfExists(str, @"{ENV_DIRSEP}", new ProtectedString(
				false, Path.DirectorySeparatorChar.ToString()), pwEntry, pwDatabase, cf,
				uRecursionLevel, vRefsCache);

			DateTime dtNow = DateTime.Now; // Local time
			str = SprEngine.FillIfExists(str, @"{DT_YEAR}", new ProtectedString(
				false, dtNow.Year.ToString("D4")), pwEntry, pwDatabase, cf,
				uRecursionLevel, vRefsCache);
			str = SprEngine.FillIfExists(str, @"{DT_MONTH}", new ProtectedString(
				false, dtNow.Month.ToString("D2")), pwEntry, pwDatabase, cf,
				uRecursionLevel, vRefsCache);
			str = SprEngine.FillIfExists(str, @"{DT_DAY}", new ProtectedString(
				false, dtNow.Day.ToString("D2")), pwEntry, pwDatabase, cf,
				uRecursionLevel, vRefsCache);
			str = SprEngine.FillIfExists(str, @"{DT_HOUR}", new ProtectedString(
				false, dtNow.Hour.ToString("D2")), pwEntry, pwDatabase, cf,
				uRecursionLevel, vRefsCache);
			str = SprEngine.FillIfExists(str, @"{DT_MINUTE}", new ProtectedString(
				false, dtNow.Minute.ToString("D2")), pwEntry, pwDatabase, cf,
				uRecursionLevel, vRefsCache);
			str = SprEngine.FillIfExists(str, @"{DT_SECOND}", new ProtectedString(
				false, dtNow.Second.ToString("D2")), pwEntry, pwDatabase, cf,
				uRecursionLevel, vRefsCache);
			str = SprEngine.FillIfExists(str, @"{DT_SIMPLE}", new ProtectedString(
				false, dtNow.ToString("yyyyMMddHHmmss")), pwEntry, pwDatabase, cf,
				uRecursionLevel, vRefsCache);

			dtNow = dtNow.ToUniversalTime();
			str = SprEngine.FillIfExists(str, @"{DT_UTC_YEAR}", new ProtectedString(
				false, dtNow.Year.ToString("D4")), pwEntry, pwDatabase, cf,
				uRecursionLevel, vRefsCache);
			str = SprEngine.FillIfExists(str, @"{DT_UTC_MONTH}", new ProtectedString(
				false, dtNow.Month.ToString("D2")), pwEntry, pwDatabase, cf,
				uRecursionLevel, vRefsCache);
			str = SprEngine.FillIfExists(str, @"{DT_UTC_DAY}", new ProtectedString(
				false, dtNow.Day.ToString("D2")), pwEntry, pwDatabase, cf,
				uRecursionLevel, vRefsCache);
			str = SprEngine.FillIfExists(str, @"{DT_UTC_HOUR}", new ProtectedString(
				false, dtNow.Hour.ToString("D2")), pwEntry, pwDatabase, cf,
				uRecursionLevel, vRefsCache);
			str = SprEngine.FillIfExists(str, @"{DT_UTC_MINUTE}", new ProtectedString(
				false, dtNow.Minute.ToString("D2")), pwEntry, pwDatabase, cf,
				uRecursionLevel, vRefsCache);
			str = SprEngine.FillIfExists(str, @"{DT_UTC_SECOND}", new ProtectedString(
				false, dtNow.Second.ToString("D2")), pwEntry, pwDatabase, cf,
				uRecursionLevel, vRefsCache);
			str = SprEngine.FillIfExists(str, @"{DT_UTC_SIMPLE}", new ProtectedString(
				false, dtNow.ToString("yyyyMMddHHmmss")), pwEntry, pwDatabase, cf,
				uRecursionLevel, vRefsCache);

			str = SprEngine.FillRefPlaceholders(str, pwDatabase, cf, uRecursionLevel,
				vRefsCache);

			// Replace environment variables
			foreach(DictionaryEntry de in Environment.GetEnvironmentVariables())
			{
				string strKey = de.Key as string;
				string strValue = de.Value as string;

				if((strKey != null) && (strValue != null))
					str = SprEngine.FillIfExists(str, @"%" + strKey + @"%",
						new ProtectedString(false, strValue), pwEntry, pwDatabase,
						cf, uRecursionLevel, vRefsCache);
				else { Debug.Assert(false); }
			}

			return str;
		}

		private static string FillIfExists(string strData, string strPlaceholder,
			ProtectedString psParsable, PwEntry pwEntry, PwDatabase pwDatabase,
			SprContentFlags cf, uint uRecursionLevel, SprRefsCache vRefsCache)
		{
			if(strData == null) { Debug.Assert(false); return string.Empty; }
			if(strPlaceholder == null) { Debug.Assert(false); return strData; }
			if(strPlaceholder.Length == 0) { Debug.Assert(false); return strData; }
			if(psParsable == null) { Debug.Assert(false); return strData; }

			if(strData.IndexOf(strPlaceholder, SprEngine.ScMethod) >= 0)
				return SprEngine.FillPlaceholder(strData, strPlaceholder,
					SprEngine.CompileInternal(psParsable.ReadString(), pwEntry,
					pwDatabase, null, uRecursionLevel + 1, vRefsCache), cf);

			return strData;
		}

		private static string FillPlaceholder(string strData, string strPlaceholder,
			string strReplaceWith, SprContentFlags cf)
		{
			if(strData == null) { Debug.Assert(false); return string.Empty; }
			if(strPlaceholder == null) { Debug.Assert(false); return strData; }
			if(strPlaceholder.Length == 0) { Debug.Assert(false); return strData; }
			if(strReplaceWith == null) { Debug.Assert(false); return strData; }

			return StrUtil.ReplaceCaseInsensitive(strData, strPlaceholder,
				SprEngine.TransformContent(strReplaceWith, cf));
		}

		public static string TransformContent(string strContent, SprContentFlags cf)
		{
			if(strContent == null) { Debug.Assert(false); return string.Empty; }

			string str = strContent;

			if(cf != null)
			{
				if(cf.EncodeQuotesForCommandLine)
					str = SprEncoding.MakeCommandQuotes(str);

				if(cf.EncodeAsAutoTypeSequence)
					str = SprEncoding.MakeAutoTypeSequence(str);
			}

			return str;
		}

		private static string FillRefPlaceholders(string strSeq, PwDatabase pwDatabase,
			SprContentFlags cf, uint uRecursionLevel, SprRefsCache vRefsCache)
		{
			if(pwDatabase == null) return strSeq;

			string str = strSeq;

			const string strStart = @"{REF:";
			const string strEnd = @"}";

			int nOffset = 0;
			for(int iLoop = 0; iLoop < 20; ++iLoop)
			{
				str = SprEngine.FillRefsUsingCache(str, vRefsCache);

				int nStart = str.IndexOf(strStart, nOffset, SprEngine.ScMethod);
				if(nStart < 0) break;
				int nEnd = str.IndexOf(strEnd, nStart, SprEngine.ScMethod);
				if(nEnd < 0) break;

				string strFullRef = str.Substring(nStart, nEnd - nStart + 1);

				string strRef = str.Substring(nStart + strStart.Length, nEnd -
					nStart - strStart.Length);
				if(strRef.Length <= 4) { nOffset = nStart + 1; continue; }
				if(strRef[1] != '@') { nOffset = nStart + 1; continue; }
				if(strRef[3] != ':') { nOffset = nStart + 1; continue; }

				char chScan = char.ToUpper(strRef[2]);
				char chWanted = char.ToUpper(strRef[0]);

				SearchParameters sp = SearchParameters.None;
				sp.SearchString = strRef.Substring(4);
				if(chScan == 'T') sp.SearchInTitles = true;
				else if(chScan == 'U') sp.SearchInUserNames = true;
				else if(chScan == 'A') sp.SearchInUrls = true;
				else if(chScan == 'P') sp.SearchInPasswords = true;
				else if(chScan == 'N') sp.SearchInNotes = true;
				else if(chScan == 'I') sp.SearchInUuids = true;
				else if(chScan == 'O') sp.SearchInOther = true;
				else { nOffset = nStart + 1; continue; }

				PwObjectList<PwEntry> lFound = new PwObjectList<PwEntry>();
				pwDatabase.RootGroup.SearchEntries(sp, lFound, true);
				if(lFound.UCount > 0)
				{
					PwEntry peFound = lFound.GetAt(0);

					string strInsData;
					if(chWanted == 'T')
						strInsData = peFound.Strings.ReadSafe(PwDefs.TitleField);
					else if(chWanted == 'U')
						strInsData = peFound.Strings.ReadSafe(PwDefs.UserNameField);
					else if(chWanted == 'A')
						strInsData = peFound.Strings.ReadSafe(PwDefs.UrlField);
					else if(chWanted == 'P')
						strInsData = peFound.Strings.ReadSafe(PwDefs.PasswordField);
					else if(chWanted == 'N')
						strInsData = peFound.Strings.ReadSafe(PwDefs.NotesField);
					else if(chWanted == 'I')
						strInsData = peFound.Uuid.ToHexString();
					else { nOffset = nStart + 1; continue; }

					string strInnerContent = SprEngine.CompileInternal(strInsData,
						peFound, pwDatabase, null, uRecursionLevel + 1, vRefsCache);
					strInnerContent = SprEngine.TransformContent(strInnerContent, cf);

					// str = str.Substring(0, nStart) + strInnerContent + str.Substring(nEnd + 1);
					SprEngine.AddRefToCache(strFullRef, strInnerContent, vRefsCache);
					str = SprEngine.FillRefsUsingCache(str, vRefsCache);
				}
				else { nOffset = nStart + 1; continue; }
			}

			return str;
		}

		private static string FillRefsUsingCache(string strText, SprRefsCache vRefs)
		{
			string str = strText;

			foreach(KeyValuePair<string, string> kvp in vRefs)
			{
				str = str.Replace(kvp.Key, kvp.Value);
			}

			return str;
		}

		private static void AddRefToCache(string strRef, string strValue,
			SprRefsCache vRefs)
		{
			if(strRef == null) { Debug.Assert(false); return; }
			if(strValue == null) { Debug.Assert(false); return; }
			if(vRefs == null) { Debug.Assert(false); return; }

			// Only add if not exists, do not overwrite
			if(vRefs.ContainsKey(strRef) == false)
				vRefs.Add(strRef, strValue);
		}
	}
}
