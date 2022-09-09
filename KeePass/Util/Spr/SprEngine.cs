/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2022 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using KeePass.App.Configuration;
using KeePass.Forms;

using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Native;
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

		// private static readonly char[] m_vPlhEscapes = new char[] { '{', '}', '%' };

		// Important notes for plugin developers subscribing to the following events:
		// * If possible, prefer subscribing to FilterCompile instead of
		//   FilterCompilePre.
		// * If your plugin provides an active transformation (e.g. replacing a
		//   placeholder that changes some state or requires UI interaction), you
		//   must only perform the transformation if the ExtActive bit is set in
		//   args.Context.Flags of the event arguments object args provided to the
		//   event handler.
		// * Non-active transformations should only be performed if the ExtNonActive
		//   bit is set in args.Context.Flags.
		// * If your plugin provides a placeholder (like e.g. {EXAMPLE}), you
		//   should add this placeholder to the FilterPlaceholderHints list
		//   (e.g. add the string "{EXAMPLE}"). Please remove your strings from
		//   the list when your plugin is terminated.
		public static event EventHandler<SprEventArgs> FilterCompilePre;
		public static event EventHandler<SprEventArgs> FilterCompile;

		private static List<string> m_lFilterPlh = new List<string>();
		// See the events above
		public static List<string> FilterPlaceholderHints
		{
			get { return m_lFilterPlh; }
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

			if(ctx == null) ctx = new SprContext();
			ctx.RefCache.Clear();

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
			MainForm mf = Program.MainForm;

			bool bExt = ((ctx.Flags & (SprCompileFlags.ExtActive |
				SprCompileFlags.ExtNonActive)) != SprCompileFlags.None);
			if(bExt && (SprEngine.FilterCompilePre != null))
			{
				SprEventArgs args = new SprEventArgs(str, ctx.Clone());
				SprEngine.FilterCompilePre(null, args);
				str = args.Text;
			}

			if((ctx.Flags & SprCompileFlags.Comments) != SprCompileFlags.None)
				str = RemoveComments(str, null, null);

			// The following realizes {T-CONV:/Text/Raw/}, which should be
			// one of the first transformations (except comments)
			if((ctx.Flags & SprCompileFlags.TextTransforms) != SprCompileFlags.None)
				str = PerformTextTransforms(str, ctx, uRecursionLevel);

			if((ctx.Flags & SprCompileFlags.Run) != SprCompileFlags.None)
				str = RunCommands(str, ctx, uRecursionLevel);

			if((ctx.Flags & SprCompileFlags.DataActive) != SprCompileFlags.None)
				str = PerformClipboardCopy(str, ctx, uRecursionLevel);

			if(((ctx.Flags & SprCompileFlags.DataNonActive) != SprCompileFlags.None) &&
				(str.IndexOf(@"{CLIPBOARD}", SprEngine.ScMethod) >= 0))
			{
				string strCb = null;
				try { strCb = ClipboardUtil.GetText(); }
				catch(Exception) { Debug.Assert(false); }
				str = Fill(str, @"{CLIPBOARD}", strCb ?? string.Empty, ctx, null);
			}

			if((ctx.Flags & SprCompileFlags.AppPaths) != SprCompileFlags.None)
				str = AppLocator.FillPlaceholders(str, ctx);

			if(ctx.Entry != null)
			{
				if((ctx.Flags & SprCompileFlags.PickChars) != SprCompileFlags.None)
					str = ReplacePickPw(str, ctx, uRecursionLevel);

				if((ctx.Flags & SprCompileFlags.EntryStrings) != SprCompileFlags.None)
					str = FillEntryStrings(str, ctx, uRecursionLevel);

				if((ctx.Flags & SprCompileFlags.EntryStringsSpecial) != SprCompileFlags.None)
					str = FillEntryStringsSpecial(str, ctx, uRecursionLevel);

				if(((ctx.Flags & SprCompileFlags.EntryProperties) != SprCompileFlags.None) &&
					(str.IndexOf(@"{UUID}", SprEngine.ScMethod) >= 0))
					str = Fill(str, @"{UUID}", ctx.Entry.Uuid.ToHexString(), ctx, null);

				if(((ctx.Flags & SprCompileFlags.PasswordEnc) != SprCompileFlags.None) &&
					(str.IndexOf(@"{PASSWORD_ENC}", SprEngine.ScMethod) >= 0))
				{
					string strPwCmp = SprEngine.CompileInternal(@"{PASSWORD}",
						ctx.WithoutContentTransformations(), uRecursionLevel + 1);
					str = Fill(str, @"{PASSWORD_ENC}", StrUtil.EncryptString(
						strPwCmp), ctx, null);
				}

				PwGroup pg = ctx.Entry.ParentGroup;
				if(((ctx.Flags & SprCompileFlags.Group) != SprCompileFlags.None) &&
					(pg != null))
					str = FillGroupPlh(str, @"{GROUP", pg, ctx, uRecursionLevel);
			}

			if((ctx.Flags & SprCompileFlags.Paths) != SprCompileFlags.None)
			{
				if(mf != null)
				{
					PwGroup pgSel = mf.GetSelectedGroup();
					if(pgSel != null)
						str = FillGroupPlh(str, @"{GROUP_SEL", pgSel, ctx, uRecursionLevel);
				}

				str = Fill(str, @"{APPDIR}", UrlUtil.GetFileDirectory(
					WinUtil.GetExecutable(), false, false), ctx, uRecursionLevel);

				str = Fill(str, @"{ENV_DIRSEP}", Path.DirectorySeparatorChar.ToString(),
					ctx, null);

				string strPF86 = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
				if(string.IsNullOrEmpty(strPF86))
					strPF86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
				if(strPF86 != null)
					str = Fill(str, @"{ENV_PROGRAMFILES_X86}", strPF86, ctx, uRecursionLevel);
				else { Debug.Assert(false); }

				if(ctx.Database != null)
				{
					string strPath = ctx.Database.IOConnectionInfo.Path;
					string strDir = UrlUtil.GetFileDirectory(strPath, false, false);
					string strName = UrlUtil.GetFileName(strPath);

					// For backward compatibility only
					str = Fill(str, @"{DOCDIR}", strDir, ctx, uRecursionLevel);

					str = Fill(str, @"{DB_PATH}", strPath, ctx, uRecursionLevel);
					str = Fill(str, @"{DB_DIR}", strDir, ctx, uRecursionLevel);
					str = Fill(str, @"{DB_NAME}", strName, ctx, uRecursionLevel);
					str = Fill(str, @"{DB_BASENAME}", UrlUtil.StripExtension(
						strName), ctx, uRecursionLevel);
					str = Fill(str, @"{DB_EXT}", UrlUtil.GetExtension(
						strPath), ctx, uRecursionLevel);
				}
			}

			if((ctx.Flags & SprCompileFlags.AutoType) != SprCompileFlags.None)
			{
				// Use Bksp instead of Del (in order to avoid Ctrl+Alt+Del);
				// https://sourceforge.net/p/keepass/discussion/329220/thread/4f1aa6b8/
				str = StrUtil.ReplaceCaseInsensitive(str, @"{CLEARFIELD}",
					@"{HOME}+({END}){BKSP}{DELAY 50}");
			}

			if(((ctx.Flags & SprCompileFlags.DateTime) != SprCompileFlags.None) &&
				(str.IndexOf(@"{DT_", SprEngine.ScMethod) >= 0))
			{
				DateTime dtNow = DateTime.UtcNow;
				str = Fill(str, @"{DT_UTC_YEAR}", dtNow.Year.ToString("D4"),
					ctx, null);
				str = Fill(str, @"{DT_UTC_MONTH}", dtNow.Month.ToString("D2"),
					ctx, null);
				str = Fill(str, @"{DT_UTC_DAY}", dtNow.Day.ToString("D2"),
					ctx, null);
				str = Fill(str, @"{DT_UTC_HOUR}", dtNow.Hour.ToString("D2"),
					ctx, null);
				str = Fill(str, @"{DT_UTC_MINUTE}", dtNow.Minute.ToString("D2"),
					ctx, null);
				str = Fill(str, @"{DT_UTC_SECOND}", dtNow.Second.ToString("D2"),
					ctx, null);
				str = Fill(str, @"{DT_UTC_SIMPLE}", dtNow.ToString("yyyyMMddHHmmss"),
					ctx, null);

				dtNow = dtNow.ToLocalTime();
				str = Fill(str, @"{DT_YEAR}", dtNow.Year.ToString("D4"),
					ctx, null);
				str = Fill(str, @"{DT_MONTH}", dtNow.Month.ToString("D2"),
					ctx, null);
				str = Fill(str, @"{DT_DAY}", dtNow.Day.ToString("D2"),
					ctx, null);
				str = Fill(str, @"{DT_HOUR}", dtNow.Hour.ToString("D2"),
					ctx, null);
				str = Fill(str, @"{DT_MINUTE}", dtNow.Minute.ToString("D2"),
					ctx, null);
				str = Fill(str, @"{DT_SECOND}", dtNow.Second.ToString("D2"),
					ctx, null);
				str = Fill(str, @"{DT_SIMPLE}", dtNow.ToString("yyyyMMddHHmmss"),
					ctx, null);
			}

			if((ctx.Flags & SprCompileFlags.References) != SprCompileFlags.None)
				str = SprEngine.FillRefPlaceholders(str, ctx, uRecursionLevel);

			if(((ctx.Flags & SprCompileFlags.EnvVars) != SprCompileFlags.None) &&
				(str.IndexOf('%') >= 0))
			{
				foreach(DictionaryEntry de in Environment.GetEnvironmentVariables())
				{
					string strKey = (de.Key as string);
					if(string.IsNullOrEmpty(strKey)) { Debug.Assert(false); continue; }

					string strValue = (de.Value as string);
					if(strValue == null) { Debug.Assert(false); strValue = string.Empty; }

					str = Fill(str, @"%" + strKey + @"%", strValue, ctx, uRecursionLevel);
				}
			}

			if((ctx.Flags & SprCompileFlags.Env) != SprCompileFlags.None)
				str = FillUriSpecial(str, ctx, @"{BASE", (ctx.Base ?? string.Empty),
					ctx.BaseIsEncoded, uRecursionLevel);

			str = EntryUtil.FillPlaceholders(str, ctx, uRecursionLevel);

			if((ctx.Flags & SprCompileFlags.PickChars) != SprCompileFlags.None)
				str = ReplacePickChars(str, ctx, uRecursionLevel);

			if(bExt && (SprEngine.FilterCompile != null))
			{
				SprEventArgs args = new SprEventArgs(str, ctx.Clone());
				SprEngine.FilterCompile(null, args);
				str = args.Text;
			}

			if(ctx.EncodeAsAutoTypeSequence)
			{
				str = StrUtil.NormalizeNewLines(str, false);
				str = str.Replace("\n", @"{ENTER}");
			}

			return str;
		}

		private static string Fill(string strData, string strPlaceholder,
			string strReplacement, SprContext ctx, uint? ouRecursionLevel)
		{
			if(strData == null) { Debug.Assert(false); return string.Empty; }
			if(string.IsNullOrEmpty(strPlaceholder)) { Debug.Assert(false); return strData; }
			if(strReplacement == null) { Debug.Assert(false); strReplacement = string.Empty; }

			if(strData.IndexOf(strPlaceholder, SprEngine.ScMethod) < 0) return strData;

			string strValue = strReplacement;
			if(ouRecursionLevel.HasValue)
				strValue = SprEngine.CompileInternal(strValue, ((ctx != null) ?
					ctx.WithoutContentTransformations() : null),
					ouRecursionLevel.Value + 1);

			return StrUtil.ReplaceCaseInsensitive(strData, strPlaceholder,
				SprEngine.TransformContent(strValue, ctx));
		}

		private static string Fill(string strData, string strPlaceholder,
			ProtectedString psReplacement, SprContext ctx, uint? ouRecursionLevel)
		{
			if(strData == null) { Debug.Assert(false); return string.Empty; }
			if(string.IsNullOrEmpty(strPlaceholder)) { Debug.Assert(false); return strData; }
			if(psReplacement == null) { Debug.Assert(false); psReplacement = ProtectedString.Empty; }

			if(strData.IndexOf(strPlaceholder, SprEngine.ScMethod) < 0) return strData;

			return Fill(strData, strPlaceholder, psReplacement.ReadString(),
				ctx, ouRecursionLevel);
		}

		public static string TransformContent(string strContent, SprContext ctx)
		{
			if(strContent == null) { Debug.Assert(false); return string.Empty; }

			string str = strContent;

			if(ctx != null)
			{
				if(ctx.EncodeForCommandLine)
					str = SprEncoding.EncodeForCommandLine(str);

				if(ctx.EncodeAsAutoTypeSequence)
					str = SprEncoding.EncodeAsAutoTypeSequence(str);
			}

			return str;
		}

		private static string UntransformContent(string strContent, SprContext ctx)
		{
			if(strContent == null) { Debug.Assert(false); return string.Empty; }

			string str = strContent;

			if(ctx != null)
			{
				if(ctx.EncodeAsAutoTypeSequence) { Debug.Assert(false); }

				if(ctx.EncodeForCommandLine)
					str = SprEncoding.DecodeCommandLine(str);
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
					Program.Config.MainWindow.IsColumnHidden(AceColumnType.Password))
				{
					str = Fill(str, strKey, PwDefs.HiddenPassword, ctx, null);
					continue;
				}

				// Use GetSafe because the field doesn't necessarily exist
				// (might be a standard field that has been added above)
				str = Fill(str, strKey, ctx.Entry.Strings.GetSafe(strField),
					ctx, uRecursionLevel);
			}

			return str;
		}

		private static string FillEntryStringsSpecial(string str, SprContext ctx,
			uint uRecursionLevel)
		{
			return FillUriSpecial(str, ctx, @"{URL", ctx.Entry.Strings.ReadSafe(
				PwDefs.UrlField), false, uRecursionLevel);
		}

		private static string FillUriSpecial(string strText, SprContext ctx,
			string strPlhInit, string strData, bool bDataIsEncoded,
			uint uRecursionLevel)
		{
			Debug.Assert(strPlhInit.StartsWith(@"{") && !strPlhInit.EndsWith(@"}"));
			Debug.Assert(strData != null);

			string[] vPlhs = new string[] {
				strPlhInit + @"}",
				strPlhInit + @":RMVSCM}",
				strPlhInit + @":SCM}",
				strPlhInit + @":HOST}",
				strPlhInit + @":PORT}",
				strPlhInit + @":PATH}",
				strPlhInit + @":QUERY}",
				strPlhInit + @":USERINFO}",
				strPlhInit + @":USERNAME}",
				strPlhInit + @":PASSWORD}"
			};

			string str = strText;
			string strDataCmp = null;
			Uri uri = null;
			for(int i = 0; i < vPlhs.Length; ++i)
			{
				string strPlh = vPlhs[i];
				if(str.IndexOf(strPlh, SprEngine.ScMethod) < 0) continue;

				if(strDataCmp == null)
				{
					SprContext ctxData = (bDataIsEncoded ?
						ctx.WithoutContentTransformations() : ctx);
					strDataCmp = SprEngine.CompileInternal(strData, ctxData,
						uRecursionLevel + 1);
				}

				string strRep = null;
				if(i == 0) strRep = strDataCmp;
				// UrlUtil supports prefixes like cmd://
				else if(i == 1) strRep = UrlUtil.RemoveScheme(strDataCmp);
				else if(i == 2) strRep = UrlUtil.GetScheme(strDataCmp);
				else
				{
					try
					{
						if(uri == null) uri = new Uri(strDataCmp);

						int t;
						switch(i)
						{
							// case 2: strRep = uri.Scheme; break; // No cmd:// support
							case 3: strRep = uri.Host; break;
							case 4:
								strRep = uri.Port.ToString(
									NumberFormatInfo.InvariantInfo);
								break;
							case 5: strRep = uri.AbsolutePath; break;
							case 6: strRep = uri.Query; break;
							case 7: strRep = uri.UserInfo; break;
							case 8:
								strRep = uri.UserInfo;
								t = strRep.IndexOf(':');
								if(t >= 0) strRep = strRep.Substring(0, t);
								break;
							case 9:
								strRep = uri.UserInfo;
								t = strRep.IndexOf(':');
								if(t < 0) strRep = string.Empty;
								else strRep = strRep.Substring(t + 1);
								break;
							default: Debug.Assert(false); break;
						}
					}
					catch(Exception) { } // Invalid URI
				}
				if(strRep == null) strRep = string.Empty; // No assert

				str = StrUtil.ReplaceCaseInsensitive(str, strPlh, strRep);
			}

			return str;
		}

		internal static string RemoveComments(string strSeq, List<string> lComments,
			SprContext ctxComments)
		{
			if(strSeq == null) { Debug.Assert(false); return string.Empty; }

			const string strStartP = @"{C:";
			string str = strSeq;
			int iOffsetP = 0;

			while(iOffsetP < str.Length)
			{
				int iStartP = str.IndexOf(strStartP, iOffsetP, SprEngine.ScMethod);
				if(iStartP < 0) break;

				int iStartC = iStartP + strStartP.Length, cBraces = 1;

				for(int iEndP = iStartC; iEndP < str.Length; ++iEndP)
				{
					char ch = str[iEndP];
					if(ch == '{') ++cBraces;
					else if(ch == '}')
					{
						if(--cBraces == 0)
						{
							if(lComments != null)
							{
								string strC = str.Substring(iStartC, iEndP - iStartC);
								if(ctxComments != null)
									strC = Compile(strC, ctxComments);
								lComments.Add(strC);
							}

							str = str.Remove(iStartP, iEndP - iStartP + 1);
							break;
						}
					}
				}

				if(cBraces != 0) break; // Malformed input; avoid infinite loop
				iOffsetP = iStartP;
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
				str = ctx.RefCache.Fill(str, ctx);

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
						Program.Config.MainWindow.IsColumnHidden(AceColumnType.Password))
						strInsData = PwDefs.HiddenPassword;

					SprContext sprSub = ctx.WithoutContentTransformations();
					sprSub.Entry = peFound;

					string strInnerContent = SprEngine.CompileInternal(strInsData,
						sprSub, uRecursionLevel + 1);
					strInnerContent = SprEngine.TransformContent(strInnerContent, ctx);

					// str = str.Substring(0, nStart) + strInnerContent + str.Substring(nEnd + 1);
					ctx.RefCache.Add(strFullRef, strInnerContent, ctx);
					str = ctx.RefCache.Fill(str, ctx);
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
			sp.RespectEntrySearchingDisabled = false;

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

		// internal static bool MightChange(string strText)
		// {
		//	if(string.IsNullOrEmpty(strText)) return false;
		//	return (strText.IndexOfAny(m_vPlhEscapes) >= 0);
		// }

		/* internal static bool MightChange(string str)
		{
			if(str == null) { Debug.Assert(false); return false; }

			int iBStart = str.IndexOf('{');
			if(iBStart >= 0)
			{
				int iBEnd = str.LastIndexOf('}');
				if(iBStart < iBEnd) return true;
			}

			int iPFirst = str.IndexOf('%');
			if(iPFirst >= 0)
			{
				int iPLast = str.LastIndexOf('%');
				if(iPFirst < iPLast) return true;
			}

			return false;
		} */

		internal static bool MightChange(char[] v)
		{
			if(v == null) { Debug.Assert(false); return false; }

			int iBStart = Array.IndexOf<char>(v, '{');
			if(iBStart >= 0)
			{
				int iBEnd = Array.LastIndexOf<char>(v, '}');
				if(iBStart < iBEnd) return true;
			}

			int iPFirst = Array.IndexOf<char>(v, '%');
			if(iPFirst >= 0)
			{
				int iPLast = Array.LastIndexOf<char>(v, '%');
				if(iPFirst < iPLast) return true;
			}

			return false;
		}

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

		/// <summary>
		/// Parse and remove a placeholder of the form
		/// <c>{PLH:/Param1/Param2/.../}</c>.
		/// </summary>
		internal static bool ParseAndRemovePlhWithParams(ref string str,
			SprContext ctx, uint uRecursionLevel, string strPlhStart,
			out int iStart, out List<string> lParams, bool bSprCmpParams)
		{
			Debug.Assert(strPlhStart.StartsWith(@"{") && !strPlhStart.EndsWith(@"}"));

			iStart = str.IndexOf(strPlhStart, SprEngine.ScMethod);
			if(iStart < 0) { lParams = null; return false; }

			lParams = new List<string>();

			try
			{
				int p = iStart + strPlhStart.Length;
				if(p >= str.Length) throw new FormatException();

				char chSep = str[p];

				while(true)
				{
					if((p + 1) >= str.Length) throw new FormatException();

					if(str[p + 1] == '}') break;

					int q = str.IndexOf(chSep, p + 1);
					if(q < 0) throw new FormatException();

					lParams.Add(str.Substring(p + 1, q - p - 1));
					p = q;
				}

				Debug.Assert(str[p + 1] == '}');
				str = str.Remove(iStart, (p + 1) - iStart + 1);
			}
			catch(Exception)
			{
				str = str.Substring(0, iStart);
			}

			if(bSprCmpParams && (ctx != null))
			{
				SprContext ctxSub = ctx.WithoutContentTransformations();
				for(int i = 0; i < lParams.Count; ++i)
					lParams[i] = CompileInternal(lParams[i], ctxSub, uRecursionLevel);
			}

			return true;
		}

		private static string PerformTextTransforms(string strText, SprContext ctx,
			uint uRecursionLevel)
		{
			string str = strText;
			int iStart;
			List<string> lParams;

			// {T-CONV:/Text/Raw/} should be the first transformation

			while(ParseAndRemovePlhWithParams(ref str, ctx, uRecursionLevel,
				@"{T-CONV:", out iStart, out lParams, true))
			{
				if(lParams.Count < 2) continue;

				try
				{
					string strNew = lParams[0];
					string strCmd = lParams[1].ToLower();

					if((strCmd == "u") || (strCmd == "upper"))
						strNew = strNew.ToUpper();
					else if((strCmd == "l") || (strCmd == "lower"))
						strNew = strNew.ToLower();
					else if(strCmd == "base64")
					{
						byte[] pbUtf8 = StrUtil.Utf8.GetBytes(strNew);
						strNew = Convert.ToBase64String(pbUtf8);
					}
					else if(strCmd == "hex")
					{
						byte[] pbUtf8 = StrUtil.Utf8.GetBytes(strNew);
						strNew = MemUtil.ByteArrayToHexString(pbUtf8);
					}
					else if(strCmd == "uri")
						strNew = Uri.EscapeDataString(strNew);
					else if(strCmd == "uri-dec")
						strNew = Uri.UnescapeDataString(strNew);
					// "raw": no modification

					if(strCmd != "raw")
						strNew = TransformContent(strNew, ctx);

					str = str.Insert(iStart, strNew);
				}
				catch(Exception) { Debug.Assert(false); }
			}

			while(ParseAndRemovePlhWithParams(ref str, ctx, uRecursionLevel,
				@"{T-REPLACE-RX:", out iStart, out lParams, true))
			{
				if(lParams.Count < 2) continue;
				if(lParams.Count == 2) lParams.Add(string.Empty);

				try
				{
					string strNew = Regex.Replace(lParams[0], lParams[1], lParams[2]);
					strNew = TransformContent(strNew, ctx);
					str = str.Insert(iStart, strNew);
				}
				catch(Exception) { }
			}

			return str;
		}

		private static string PerformClipboardCopy(string strText, SprContext ctx,
			uint uRecursionLevel)
		{
			string str = strText;
			int iStart;
			List<string> lParams;
			SprContext ctxData = ((ctx != null) ? ctx.WithoutContentTransformations() : null);

			while(ParseAndRemovePlhWithParams(ref str, ctxData, uRecursionLevel,
				@"{CLIPBOARD-SET:", out iStart, out lParams, true))
			{
				if(lParams.Count < 1) continue;

				try
				{
					ClipboardUtil.Copy(lParams[0] ?? string.Empty, false,
						true, null, null, IntPtr.Zero);
				}
				catch(Exception) { Debug.Assert(false); }
			}

			return str;
		}

		private static string FillGroupPlh(string strData, string strPlhPrefix,
			PwGroup pg, SprContext ctx, uint uRecursionLevel)
		{
			Debug.Assert(strPlhPrefix.StartsWith("{"));
			Debug.Assert(!strPlhPrefix.EndsWith("_"));
			Debug.Assert(!strPlhPrefix.EndsWith("}"));

			string str = strData;

			str = Fill(str, strPlhPrefix + @"}", pg.Name, ctx, uRecursionLevel);

			string strGroupPath = pg.GetFullPath();
			str = Fill(str, strPlhPrefix + @"_PATH}", strGroupPath,
				ctx, uRecursionLevel);
			str = Fill(str, strPlhPrefix + @"PATH}", strGroupPath,
				ctx, uRecursionLevel); // Obsolete; for backward compatibility

			str = Fill(str, strPlhPrefix + @"_NOTES}", pg.Notes, ctx, uRecursionLevel);

			return str;
		}

		private static string RunCommands(string strText, SprContext ctx,
			uint uRecursionLevel)
		{
			string str = strText;
			int iStart;
			List<string> lParams;

			while(ParseAndRemovePlhWithParams(ref str, ctx, uRecursionLevel,
				@"{CMD:", out iStart, out lParams, false))
			{
				if(lParams.Count == 0) continue;

				string strBaseRaw = null;
				if((ctx != null) && (ctx.Base != null))
				{
					if(ctx.BaseIsEncoded)
						strBaseRaw = UntransformContent(ctx.Base, ctx);
					else strBaseRaw = ctx.Base;
				}

				string strCmd = WinUtil.CompileUrl((lParams[0] ?? string.Empty),
					((ctx != null) ? ctx.Entry : null), true, strBaseRaw, true);
				if(WinUtil.IsCommandLineUrl(strCmd))
					strCmd = WinUtil.GetCommandLineFromUrl(strCmd);
				if(string.IsNullOrEmpty(strCmd)) continue;

				Process p = null;
				try
				{
					StringComparison sc = StrUtil.CaseIgnoreCmp;

					string strOpt = ((lParams.Count >= 2) ? lParams[1] :
						string.Empty);
					Dictionary<string, string> d = SplitParams(strOpt);

					ProcessStartInfo psi = new ProcessStartInfo();

					string strApp, strArgs;
					StrUtil.SplitCommandLine(strCmd, out strApp, out strArgs);
					if(string.IsNullOrEmpty(strApp)) continue;
					psi.FileName = strApp;
					if(!string.IsNullOrEmpty(strArgs)) psi.Arguments = strArgs;

					string strMethod = GetParam(d, "m", "s");
					bool bShellExec = !strMethod.Equals("c", sc);
					psi.UseShellExecute = bShellExec;

					string strO = GetParam(d, "o", (bShellExec ? "0" : "1"));
					bool bStdOut = strO.Equals("1", sc);
					if(bStdOut) psi.RedirectStandardOutput = true;

					string strWS = GetParam(d, "ws", "n");
					if(strWS.Equals("h", sc))
					{
						psi.CreateNoWindow = true;
						psi.WindowStyle = ProcessWindowStyle.Hidden;
					}
					else if(strWS.Equals("min", sc))
						psi.WindowStyle = ProcessWindowStyle.Minimized;
					else if(strWS.Equals("max", sc))
						psi.WindowStyle = ProcessWindowStyle.Maximized;
					else { Debug.Assert(psi.WindowStyle == ProcessWindowStyle.Normal); }

					string strVerb = GetParam(d, "v", null);
					if(!string.IsNullOrEmpty(strVerb))
						psi.Verb = strVerb;

					bool bWait = GetParam(d, "w", "1").Equals("1", sc);

					p = NativeLib.StartProcessEx(psi);
					if(p == null) { Debug.Assert(false); continue; }

					if(bStdOut)
					{
						string strOut = (p.StandardOutput.ReadToEnd() ?? string.Empty);

						// Remove trailing new-line characters, like $(...);
						// https://pubs.opengroup.org/onlinepubs/9699919799/utilities/V3_chap02.html
						// https://www.gnu.org/software/bash/manual/html_node/Command-Substitution.html#Command-Substitution
						strOut = strOut.TrimEnd('\r', '\n');

						strOut = TransformContent(strOut, ctx);
						str = str.Insert(iStart, strOut);
					}

					if(bWait) p.WaitForExit();
				}
				catch(Exception ex)
				{
					string strMsg = strCmd + MessageService.NewParagraph + ex.Message;
					MessageService.ShowWarning(strMsg);
				}
				finally
				{
					try { if(p != null) p.Dispose(); }
					catch(Exception) { Debug.Assert(false); }
				}
			}

			return str;
		}

		private static Dictionary<string, string> SplitParams(string str)
		{
			Dictionary<string, string> d = new Dictionary<string, string>();
			if(string.IsNullOrEmpty(str)) return d;

			char[] vSplitPrm = new char[] { ',' };
			char[] vSplitKvp = new char[] { '=' };

			string[] v = str.Split(vSplitPrm);
			foreach(string strOption in v)
			{
				if(string.IsNullOrEmpty(strOption)) continue;

				string[] vKvp = strOption.Split(vSplitKvp);
				if(vKvp.Length != 2) continue;

				string strKey = (vKvp[0] ?? string.Empty).Trim().ToLower();
				string strValue = (vKvp[1] ?? string.Empty).Trim();

				d[strKey] = strValue;
			}

			return d;
		}

		private static string GetParam(Dictionary<string, string> d,
			string strName, string strDefaultValue)
		{
			if(d == null) { Debug.Assert(false); return strDefaultValue; }
			if(strName == null) { Debug.Assert(false); return strDefaultValue; }

			Debug.Assert(strName == strName.ToLower());

			string strValue;
			if(d.TryGetValue(strName, out strValue)) return strValue;

			return strDefaultValue;
		}
	}
}
