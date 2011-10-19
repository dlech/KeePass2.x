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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

using KeePass.App.Configuration;

using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.Util.Spr
{
	/// <summary>
	/// String placeholders and field reference replacement engine.
	/// </summary>
	public static partial class SprEngine
	{
		private const uint MaxRecursionDepth = 12;
		private const StringComparison ScMethod = StringComparison.OrdinalIgnoreCase;

		private static string m_strAppExePath = string.Empty;

		// private static readonly char[] m_vPlhEscapes = new char[] { '{', '}', '%' };

		private static void InitializeStatic()
		{
			m_strAppExePath = WinUtil.GetExecutable();
		}

		[Obsolete]
		public static string Compile(string strText, bool bIsAutoTypeSequence,
			PwEntry pwEntry, PwDatabase pwDatabase, bool bEscapeForAutoType,
			bool bEscapeQuotesForCommandLine)
		{
			SprContext ctx = new SprContext(pwEntry, pwDatabase, SprCompileFlags.All,
				bEscapeForAutoType, bEscapeQuotesForCommandLine);
			return Compile(strText, ctx);
		}

		public static string Compile(string strText, SprContext ctx)
		{
			if(strText == null) { Debug.Assert(false); return string.Empty; }
			if(strText.Length == 0) return string.Empty;

			SprEngine.InitializeStatic();

			if(ctx == null) ctx = new SprContext();
			ctx.RefsCache.Clear();

			string str = SprEngine.CompileInternal(strText, ctx, 0);

			// if(bEscapeForAutoType && !bIsAutoTypeSequence)
			//	str = SprEncoding.MakeAutoTypeSequence(str);

			return str;
		}

		private static string CompileInternal(string strText, SprContext ctx,
			uint uRecursionLevel)
		{
			if(strText == null) { Debug.Assert(false); return string.Empty; }
			if(ctx == null) { Debug.Assert(false); ctx = new SprContext(); }

			if(uRecursionLevel >= SprEngine.MaxRecursionDepth)
			{
				Debug.Assert(false); // Most likely a recursive reference
				return string.Empty; // Do not return strText (endless loop)
			}

			string str = strText;

			if((ctx.Flags & SprCompileFlags.Comments) != SprCompileFlags.None)
				str = RemoveComments(str);

			if((ctx.Flags & SprCompileFlags.AppPaths) != SprCompileFlags.None)
				str = AppLocator.FillPlaceholders(str, ctx);

			if(ctx.Entry != null)
			{
				if((ctx.Flags & SprCompileFlags.PickChars) != SprCompileFlags.None)
					str = ReplacePickPw(str, ctx, uRecursionLevel);

				if((ctx.Flags & SprCompileFlags.EntryStrings) != SprCompileFlags.None)
					str = FillEntryStrings(str, ctx, uRecursionLevel);

				if((ctx.Flags & SprCompileFlags.UrlRmvScm) != SprCompileFlags.None)
				{
					ctx.UrlRemoveSchemeOnce = true;
					str = SprEngine.FillIfExists(str, @"{URL:RMVSCM}",
						ctx.Entry.Strings.GetSafe(PwDefs.UrlField), ctx, uRecursionLevel);
					Debug.Assert(!ctx.UrlRemoveSchemeOnce);
				}

				if(((ctx.Flags & SprCompileFlags.PasswordEnc) != SprCompileFlags.None) &&
					(str.IndexOf(@"{PASSWORD_ENC}", SprEngine.ScMethod) >= 0))
					str = SprEngine.FillIfExists(str, @"{PASSWORD_ENC}", new ProtectedString(false,
						StrUtil.EncryptString(ctx.Entry.Strings.ReadSafe(PwDefs.PasswordField))),
						ctx, uRecursionLevel);

				if(((ctx.Flags & SprCompileFlags.Group) != SprCompileFlags.None) &&
					(ctx.Entry.ParentGroup != null))
				{
					str = SprEngine.FillIfExists(str, @"{GROUP}", new ProtectedString(
						false, ctx.Entry.ParentGroup.Name), ctx, uRecursionLevel);

					str = SprEngine.FillIfExists(str, @"{GROUPPATH}", new ProtectedString(
						false, ctx.Entry.ParentGroup.GetFullPath()), ctx, uRecursionLevel);
				}
			}

			if((ctx.Flags & SprCompileFlags.Paths) != SprCompileFlags.None)
				str = SprEngine.FillIfExists(str, @"{APPDIR}", new ProtectedString(
					false, UrlUtil.GetFileDirectory(m_strAppExePath, false, false)),
					ctx, uRecursionLevel);

			if(ctx.Database != null)
			{
				if((ctx.Flags & SprCompileFlags.Paths) != SprCompileFlags.None)
				{
					// For backward compatibility only
					str = SprEngine.FillIfExists(str, @"{DOCDIR}", new ProtectedString(
						false, UrlUtil.GetFileDirectory(ctx.Database.IOConnectionInfo.Path,
						false, false)), ctx, uRecursionLevel);

					str = SprEngine.FillIfExists(str, @"{DB_PATH}", new ProtectedString(
						false, ctx.Database.IOConnectionInfo.Path), ctx, uRecursionLevel);
					str = SprEngine.FillIfExists(str, @"{DB_DIR}", new ProtectedString(
						false, UrlUtil.GetFileDirectory(ctx.Database.IOConnectionInfo.Path,
						false, false)), ctx, uRecursionLevel);
					str = SprEngine.FillIfExists(str, @"{DB_NAME}", new ProtectedString(
						false, UrlUtil.GetFileName(ctx.Database.IOConnectionInfo.Path)),
						ctx, uRecursionLevel);
					str = SprEngine.FillIfExists(str, @"{DB_BASENAME}", new ProtectedString(
						false, UrlUtil.StripExtension(UrlUtil.GetFileName(
						ctx.Database.IOConnectionInfo.Path))), ctx, uRecursionLevel);
					str = SprEngine.FillIfExists(str, @"{DB_EXT}", new ProtectedString(
						false, UrlUtil.GetExtension(ctx.Database.IOConnectionInfo.Path)),
						ctx, uRecursionLevel);
				}
			}

			if((ctx.Flags & SprCompileFlags.Paths) != SprCompileFlags.None)
				str = SprEngine.FillIfExists(str, @"{ENV_DIRSEP}", new ProtectedString(
					false, Path.DirectorySeparatorChar.ToString()), ctx, uRecursionLevel);

			if((ctx.Flags & SprCompileFlags.AutoType) != SprCompileFlags.None)
				str = StrUtil.ReplaceCaseInsensitive(str, @"{CLEARFIELD}",
					@"{HOME}+({END}){DEL}{DELAY 50}");

			if((ctx.Flags & SprCompileFlags.DateTime) != SprCompileFlags.None)
			{
				DateTime dtNow = DateTime.Now; // Local time
				str = SprEngine.FillIfExists(str, @"{DT_YEAR}", new ProtectedString(
					false, dtNow.Year.ToString("D4")), ctx, uRecursionLevel);
				str = SprEngine.FillIfExists(str, @"{DT_MONTH}", new ProtectedString(
					false, dtNow.Month.ToString("D2")), ctx, uRecursionLevel);
				str = SprEngine.FillIfExists(str, @"{DT_DAY}", new ProtectedString(
					false, dtNow.Day.ToString("D2")), ctx, uRecursionLevel);
				str = SprEngine.FillIfExists(str, @"{DT_HOUR}", new ProtectedString(
					false, dtNow.Hour.ToString("D2")), ctx, uRecursionLevel);
				str = SprEngine.FillIfExists(str, @"{DT_MINUTE}", new ProtectedString(
					false, dtNow.Minute.ToString("D2")), ctx, uRecursionLevel);
				str = SprEngine.FillIfExists(str, @"{DT_SECOND}", new ProtectedString(
					false, dtNow.Second.ToString("D2")), ctx, uRecursionLevel);
				str = SprEngine.FillIfExists(str, @"{DT_SIMPLE}", new ProtectedString(
					false, dtNow.ToString("yyyyMMddHHmmss")), ctx, uRecursionLevel);

				dtNow = dtNow.ToUniversalTime();
				str = SprEngine.FillIfExists(str, @"{DT_UTC_YEAR}", new ProtectedString(
					false, dtNow.Year.ToString("D4")), ctx, uRecursionLevel);
				str = SprEngine.FillIfExists(str, @"{DT_UTC_MONTH}", new ProtectedString(
					false, dtNow.Month.ToString("D2")), ctx, uRecursionLevel);
				str = SprEngine.FillIfExists(str, @"{DT_UTC_DAY}", new ProtectedString(
					false, dtNow.Day.ToString("D2")), ctx, uRecursionLevel);
				str = SprEngine.FillIfExists(str, @"{DT_UTC_HOUR}", new ProtectedString(
					false, dtNow.Hour.ToString("D2")), ctx, uRecursionLevel);
				str = SprEngine.FillIfExists(str, @"{DT_UTC_MINUTE}", new ProtectedString(
					false, dtNow.Minute.ToString("D2")), ctx, uRecursionLevel);
				str = SprEngine.FillIfExists(str, @"{DT_UTC_SECOND}", new ProtectedString(
					false, dtNow.Second.ToString("D2")), ctx, uRecursionLevel);
				str = SprEngine.FillIfExists(str, @"{DT_UTC_SIMPLE}", new ProtectedString(
					false, dtNow.ToString("yyyyMMddHHmmss")), ctx, uRecursionLevel);
			}

			if((ctx.Flags & SprCompileFlags.References) != SprCompileFlags.None)
				str = SprEngine.FillRefPlaceholders(str, ctx, uRecursionLevel);

			if(((ctx.Flags & SprCompileFlags.EnvVars) != SprCompileFlags.None) &&
				(str.IndexOf('%') >= 0))
			{
				// Replace environment variables
				foreach(DictionaryEntry de in Environment.GetEnvironmentVariables())
				{
					string strKey = (de.Key as string);
					string strValue = (de.Value as string);

					if((strKey != null) && (strValue != null))
						str = SprEngine.FillIfExists(str, @"%" + strKey + @"%",
							new ProtectedString(false, strValue), ctx, uRecursionLevel);
					else { Debug.Assert(false); }
				}
			}

			str = EntryUtil.FillPlaceholders(str, ctx);

			if((ctx.Flags & SprCompileFlags.PickChars) != SprCompileFlags.None)
				str = ReplacePickChars(str, ctx, uRecursionLevel);

			if(ctx.EncodeAsAutoTypeSequence)
			{
				str = str.Replace("\r\n", "\n");
				str = str.Replace("\r", "\n");
				str = str.Replace("\n", @"{ENTER}");
			}

			return str;
		}

		private static string FillIfExists(string strData, string strPlaceholder,
			ProtectedString psParsable, SprContext ctx, uint uRecursionLevel)
		{
			// The UrlRemoveSchemeOnce property of ctx must be cleared
			// before this method returns and before any recursive call
			bool bRemoveScheme = false;
			if(ctx != null)
			{
				bRemoveScheme = ctx.UrlRemoveSchemeOnce;
				ctx.UrlRemoveSchemeOnce = false;
			}

			if(strData == null) { Debug.Assert(false); return string.Empty; }
			if(strPlaceholder == null) { Debug.Assert(false); return strData; }
			if(strPlaceholder.Length == 0) { Debug.Assert(false); return strData; }
			if(psParsable == null) { Debug.Assert(false); return strData; }

			if(strData.IndexOf(strPlaceholder, SprEngine.ScMethod) >= 0)
			{
				string strReplacement = SprEngine.CompileInternal(
					psParsable.ReadString(), ctx.WithoutContentTransformations(),
					uRecursionLevel + 1);

				if(bRemoveScheme)
					strReplacement = UrlUtil.RemoveScheme(strReplacement);

				return SprEngine.FillPlaceholder(strData, strPlaceholder,
					strReplacement, ctx);
			}

			return strData;
		}

		private static string FillPlaceholder(string strData, string strPlaceholder,
			string strReplaceWith, SprContext ctx)
		{
			if(strData == null) { Debug.Assert(false); return string.Empty; }
			if(strPlaceholder == null) { Debug.Assert(false); return strData; }
			if(strPlaceholder.Length == 0) { Debug.Assert(false); return strData; }
			if(strReplaceWith == null) { Debug.Assert(false); return strData; }

			return StrUtil.ReplaceCaseInsensitive(strData, strPlaceholder,
				SprEngine.TransformContent(strReplaceWith, ctx));
		}

		public static string TransformContent(string strContent, SprContext ctx)
		{
			if(strContent == null) { Debug.Assert(false); return string.Empty; }

			string str = strContent;

			if(ctx != null)
			{
				if(ctx.EncodeQuotesForCommandLine)
					str = SprEncoding.MakeCommandQuotes(str);

				if(ctx.EncodeAsAutoTypeSequence)
					str = SprEncoding.MakeAutoTypeSequence(str);
			}

			return str;
		}

		private static string FillEntryStrings(string str, SprContext ctx,
			uint uRecursionLevel)
		{
			List<string> vKeys = ctx.Entry.Strings.GetKeys();

			// Ensure that all standard field names are in the list
			// (this is required in order to replace the standard placeholders
			// even if the corresponding standard field isn't present in
			// the entry)
			List<string> vStdNames = PwDefs.GetStandardFields();
			foreach(string strStdField in vStdNames)
			{
				if(!vKeys.Contains(strStdField)) vKeys.Add(strStdField);
			}

			// Do not directly enumerate the strings in ctx.Entry.Strings,
			// because strings might change during the Spr compilation
			foreach(string strField in vKeys)
			{
				string strKey = (PwDefs.IsStandardField(strField) ?
					(@"{" + strField + @"}") :
					(@"{" + PwDefs.AutoTypeStringPrefix + strField + @"}"));

				if(!ctx.ForcePlainTextPasswords && strKey.Equals(@"{" +
					PwDefs.PasswordField + @"}", StrUtil.CaseIgnoreCmp) &&
					Program.Config.MainWindow.IsColumnHidden(AceColumnType.Password, true))
				{
					str = SprEngine.FillIfExists(str, strKey, new ProtectedString(
						false, PwDefs.HiddenPassword), ctx, uRecursionLevel);
					continue;
				}

				// Use GetSafe because the field doesn't necessarily exist
				// (might be a standard field that has been added above)
				str = SprEngine.FillIfExists(str, strKey, ctx.Entry.Strings.GetSafe(
					strField), ctx, uRecursionLevel);
			}

			return str;
		}

		private const string StrRemStart = @"{C:";
		private const string StrRemEnd = @"}";
		private static string RemoveComments(string strSeq)
		{
			string str = strSeq;

			while(true)
			{
				int iStart = str.IndexOf(StrRemStart, SprEngine.ScMethod);
				if(iStart < 0) break;
				int iEnd = str.IndexOf(StrRemEnd, iStart + 1, SprEngine.ScMethod);
				if(iEnd <= iStart) break;

				str = (str.Substring(0, iStart) + str.Substring(iEnd + StrRemEnd.Length));
			}

			return str;
		}

		internal const string StrRefStart = @"{REF:";
		internal const string StrRefEnd = @"}";
		private static string FillRefPlaceholders(string strSeq, SprContext ctx,
			uint uRecursionLevel)
		{
			if(ctx.Database == null) return strSeq;

			string str = strSeq;

			int nOffset = 0;
			for(int iLoop = 0; iLoop < 20; ++iLoop)
			{
				str = SprEngine.FillRefsUsingCache(str, ctx);

				int nStart = str.IndexOf(StrRefStart, nOffset, SprEngine.ScMethod);
				if(nStart < 0) break;
				int nEnd = str.IndexOf(StrRefEnd, nStart + 1, SprEngine.ScMethod);
				if(nEnd <= nStart) break;

				string strFullRef = str.Substring(nStart, nEnd - nStart + 1);
				char chScan, chWanted;
				PwEntry peFound = FindRefTarget(strFullRef, ctx, out chScan, out chWanted);

				if(peFound != null)
				{
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

					if((chWanted == 'P') && !ctx.ForcePlainTextPasswords &&
						Program.Config.MainWindow.IsColumnHidden(AceColumnType.Password, true))
						strInsData = PwDefs.HiddenPassword;

					SprContext sprSub = ctx.WithoutContentTransformations();
					sprSub.Entry = peFound;

					string strInnerContent = SprEngine.CompileInternal(strInsData,
						sprSub, uRecursionLevel + 1);
					strInnerContent = SprEngine.TransformContent(strInnerContent, ctx);

					// str = str.Substring(0, nStart) + strInnerContent + str.Substring(nEnd + 1);
					SprEngine.AddRefToCache(strFullRef, strInnerContent, ctx);
					str = SprEngine.FillRefsUsingCache(str, ctx);
				}
				else { nOffset = nStart + 1; continue; }
			}

			return str;
		}

		public static PwEntry FindRefTarget(string strFullRef, SprContext ctx,
			out char chScan, out char chWanted)
		{
			chScan = char.MinValue;
			chWanted = char.MinValue;

			if(strFullRef == null) { Debug.Assert(false); return null; }
			if(!strFullRef.StartsWith(StrRefStart, SprEngine.ScMethod) ||
				!strFullRef.EndsWith(StrRefEnd, SprEngine.ScMethod))
				return null;
			if((ctx == null) || (ctx.Database == null)) { Debug.Assert(false); return null; }

			string strRef = strFullRef.Substring(StrRefStart.Length,
				strFullRef.Length - StrRefStart.Length - StrRefEnd.Length);
			if(strRef.Length <= 4) return null;
			if(strRef[1] != '@') return null;
			if(strRef[3] != ':') return null;

			chScan = char.ToUpper(strRef[2]);
			chWanted = char.ToUpper(strRef[0]);

			SearchParameters sp = SearchParameters.None;
			sp.SearchString = strRef.Substring(4);
			if(chScan == 'T') sp.SearchInTitles = true;
			else if(chScan == 'U') sp.SearchInUserNames = true;
			else if(chScan == 'A') sp.SearchInUrls = true;
			else if(chScan == 'P') sp.SearchInPasswords = true;
			else if(chScan == 'N') sp.SearchInNotes = true;
			else if(chScan == 'I') sp.SearchInUuids = true;
			else if(chScan == 'O') sp.SearchInOther = true;
			else return null;

			PwObjectList<PwEntry> lFound = new PwObjectList<PwEntry>();
			ctx.Database.RootGroup.SearchEntries(sp, lFound);

			return ((lFound.UCount > 0) ? lFound.GetAt(0) : null);
		}

		private static string FillRefsUsingCache(string strText, SprContext ctx)
		{
			string str = strText;

			foreach(KeyValuePair<string, string> kvp in ctx.RefsCache)
			{
				// str = str.Replace(kvp.Key, kvp.Value);
				str = StrUtil.ReplaceCaseInsensitive(str, kvp.Key, kvp.Value);
			}

			return str;
		}

		private static void AddRefToCache(string strRef, string strValue,
			SprContext ctx)
		{
			if(strRef == null) { Debug.Assert(false); return; }
			if(strValue == null) { Debug.Assert(false); return; }
			if(ctx == null) { Debug.Assert(false); return; }

			// Only add if not exists, do not overwrite
			if(!ctx.RefsCache.ContainsKey(strRef))
				ctx.RefsCache.Add(strRef, strValue);
		}

		// internal static bool MightChange(string strText)
		// {
		//	if(string.IsNullOrEmpty(strText)) return false;
		//	return (strText.IndexOfAny(m_vPlhEscapes) >= 0);
		// }

		/// <summary>
		/// Fast probabilistic test whether a string might be
		/// changed when compiling with <c>SprCompileFlags.Deref</c>.
		/// </summary>
		internal static bool MightDeref(string strText)
		{
			if(strText == null) return false;
			return (strText.IndexOf('{') >= 0);
		}

		internal static string DerefFn(string str, PwEntry pe)
		{
			if(!MightDeref(str)) return str;

			SprContext ctx = new SprContext(pe,
				Program.MainForm.DocumentManager.SafeFindContainerOf(pe),
				SprCompileFlags.Deref);
			// ctx.ForcePlainTextPasswords = false;

			return Compile(str, ctx);
		}
	}
}
