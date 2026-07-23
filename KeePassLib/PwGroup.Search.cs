/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2026 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

using KeePassLib.Collections;
using KeePassLib.Delegates;
using KeePassLib.Interfaces;
using KeePassLib.Security;
using KeePassLib.Serialization;
using KeePassLib.Utility;

namespace KeePassLib
{
	public sealed partial class PwGroup
	{
		private sealed class SrsmContext
		{
			public SearchParameters SearchParameters { get; private set; }
			public CompareInfo CompareInfo { get; private set; }
			public CompareOptions CompareOptions { get; private set; }
			public Regex Regex { get; private set; }

			public SrsmContext(SearchParameters sp)
			{
				if(sp == null) { Debug.Assert(false); throw new ArgumentNullException("sp"); }

				this.SearchParameters = sp;

				StringComparison sc = sp.ComparisonMode;
				bool bCurrent = ((sc == StringComparison.CurrentCulture) ||
					(sc == StringComparison.CurrentCultureIgnoreCase));
				bool bOrdinal = ((sc == StringComparison.Ordinal) ||
					(sc == StringComparison.OrdinalIgnoreCase));
				bool bIgnoreCase = StrUtil.GetIgnoreCase(sc);

				this.CompareInfo = (bCurrent ? CultureInfo.CurrentCulture :
					CultureInfo.InvariantCulture).CompareInfo;

				CompareOptions co = CompareOptions.None;
				if(bOrdinal)
					co |= (bIgnoreCase ? CompareOptions.OrdinalIgnoreCase :
						CompareOptions.Ordinal);
				else
				{
					if(bIgnoreCase) co |= CompareOptions.IgnoreCase;
					if(!sp.MatchDiacritics) co |= CompareOptions.IgnoreNonSpace;
				}
				this.CompareOptions = co;

				if(sp.SearchMode == PwSearchMode.Regular)
				{
					RegexOptions ro = RegexOptions.None;
					if(!bCurrent) ro |= RegexOptions.CultureInvariant;
					if(bIgnoreCase) ro |= RegexOptions.IgnoreCase;

					this.Regex = new Regex(sp.SearchString, ro);
				}
			}
		}

		/// <summary>
		/// Search this group and all subgroups for entries.
		/// </summary>
		/// <param name="sp">Specifies the search parameters.</param>
		/// <param name="lResults">Entry list in which the search results
		/// will be stored.</param>
		public void SearchEntries(SearchParameters sp, PwObjectList<PwEntry> lResults)
		{
			SearchEntries(sp, lResults, null);
		}

		/// <summary>
		/// Search this group and all subgroups for entries.
		/// </summary>
		/// <param name="sp">Specifies the search parameters.</param>
		/// <param name="lResults">Entry list in which the search results
		/// will be stored.</param>
		/// <param name="slStatus">Optional status reporting object.</param>
		public void SearchEntries(SearchParameters sp, PwObjectList<PwEntry> lResults,
			IStatusLogger slStatus)
		{
			if(sp == null) { Debug.Assert(false); return; }
			if(lResults == null) { Debug.Assert(false); return; }

			Debug.Assert(lResults.UCount == 0);
			lResults.Clear();

			PwSearchMode sm = sp.SearchMode;
			if((sm == PwSearchMode.Simple) || (sm == PwSearchMode.Regular))
				SrsmSearch(sp, lResults, slStatus);
			else if(sm == PwSearchMode.XPath)
				SrxpSearch(sp, lResults, slStatus);
			else { Debug.Assert(false); }
		}

		private void SrsmSearch(SearchParameters sp, PwObjectList<PwEntry> lResults,
			IStatusLogger sl)
		{
			PwObjectList<PwEntry> lAll = GetEntries(true);
			DateTime dtNow = DateTime.UtcNow;

			List<PwEntry> l = new List<PwEntry>();
			foreach(PwEntry pe in lAll)
			{
				if(sp.ExcludeExpired && pe.Expires && (pe.ExpiryTime <= dtNow))
					continue;
				if(sp.RespectEntrySearchingDisabled && !pe.GetSearchingEnabled())
					continue;

				l.Add(pe);
			}

			List<string> lTerms;
			if(sp.SearchMode == PwSearchMode.Simple)
				lTerms = StrUtil.SplitSearchTerms(sp.SearchString);
			else
				lTerms = new List<string> { sp.SearchString };

			// Search longer strings first (for improved performance)
			lTerms.Sort(StrUtil.CompareLengthGt);

			SearchParameters spSub = sp.Clone();

			for(int iTerm = 0; iTerm < lTerms.Count; ++iTerm)
			{
				string strTerm = lTerms[iTerm]; // No trim
				if(string.IsNullOrEmpty(strTerm)) continue;

				bool bNegate = false;
				if((sp.SearchMode == PwSearchMode.Simple) &&
					(strTerm.Length >= 2) && (strTerm[0] == '-'))
				{
					strTerm = strTerm.Substring(1);
					bNegate = true;
				}

				spSub.SearchString = strTerm;

				ulong uStEntriesDone = (ulong)iTerm * (ulong)l.Count;
				ulong uStEntriesMax = (ulong)lTerms.Count * (ulong)l.Count;

				List<PwEntry> lOut;
				if(!SrsmSearchSingle(spSub, l, out lOut, sl, uStEntriesDone,
					uStEntriesMax))
				{
					l.Clear(); // Do not return non-matching entries
					break;
				}

				if(bNegate)
				{
					List<PwEntry> lNew = new List<PwEntry>();
					int iNeg = 0;

					foreach(PwEntry pe in l)
					{
						if((iNeg < lOut.Count) && object.ReferenceEquals(lOut[iNeg], pe))
							++iNeg;
						else
						{
							Debug.Assert(!lOut.Contains(pe));
							lNew.Add(pe);
						}
					}
					Debug.Assert(lNew.Count == (l.Count - lOut.Count));

					l = lNew;
				}
				else l = lOut;
			}

			lResults.Add(l);
		}

		private static bool SrsmSearchSingle(SearchParameters sp, List<PwEntry> lIn,
			out List<PwEntry> lOut, IStatusLogger sl, ulong uStEntriesDone,
			ulong uStEntriesMax)
		{
			lOut = new List<PwEntry>();

			if(string.IsNullOrEmpty(sp.SearchString))
			{
				lOut.AddRange(lIn);
				return true;
			}

			SrsmContext ctx = new SrsmContext(sp);

			foreach(PwEntry pe in lIn)
			{
				if(sl != null)
				{
					uint uPct = (uint)((uStEntriesDone * 100UL) / uStEntriesMax);
					if(!sl.SetProgress(uPct)) return false;
					++uStEntriesDone;
				}

				if(SrsmIsMatch(ctx, pe)) lOut.Add(pe);
			}

			return true;
		}

		private static bool SrsmIsMatch(SrsmContext ctx, PwEntry pe)
		{
			if(pe == null) { Debug.Assert(false); return false; }

			SearchParameters sp = ctx.SearchParameters;

			foreach(KeyValuePair<string, ProtectedString> kvp in pe.Strings)
			{
				string strKey = kvp.Key;
				ProtectedString ps = kvp.Value;

				switch(strKey)
				{
					case PwDefs.TitleField:
						if(sp.SearchInTitles && SrsmIsMatch(ctx, pe, ps.ReadString()))
							return true;
						break;
					case PwDefs.UserNameField:
						if(sp.SearchInUserNames && SrsmIsMatch(ctx, pe, ps.ReadString()))
							return true;
						break;
					case PwDefs.PasswordField:
						if(sp.SearchInPasswords && SrsmIsMatch(ctx, pe, ps.ReadString()))
							return true;
						break;
					case PwDefs.UrlField:
						if(sp.SearchInUrls && SrsmIsMatch(ctx, pe, ps.ReadString()))
							return true;
						break;
					case PwDefs.NotesField:
						if(sp.SearchInNotes && SrsmIsMatch(ctx, pe, ps.ReadString()))
							return true;
						break;
					default:
						Debug.Assert(!PwDefs.IsStandardField(strKey));
						if(sp.SearchInOther && SrsmIsMatch(ctx, pe, ps.ReadString()))
							return true;
						break;
				}

				if(sp.SearchInStringNames && SrsmIsMatch(ctx, pe, strKey))
					return true;
			}

			if(sp.SearchInTags)
			{
				foreach(string strTag in pe.GetTagsInherited())
				{
					if(SrsmIsMatch(ctx, pe, strTag)) return true;
				}
			}

			if(sp.SearchInUuids && SrsmIsMatch(ctx, pe, pe.Uuid.ToHexString()))
				return true;

			if(sp.SearchInGroupPaths && (pe.ParentGroup != null) &&
				SrsmIsMatch(ctx, pe, pe.ParentGroup.GetFullPath("\n", true)))
				return true;

			if(sp.SearchInGroupNames && (pe.ParentGroup != null) &&
				SrsmIsMatch(ctx, pe, pe.ParentGroup.Name))
				return true;

			if(sp.SearchInHistory)
			{
				foreach(PwEntry peH in pe.History)
				{
					if(SrsmIsMatch(ctx, peH)) return true;
				}
			}

			return false;
		}

		private static bool SrsmIsMatch(SrsmContext ctx, PwEntry pe, string strData)
		{
			if(strData == null) { Debug.Assert(false); strData = string.Empty; }

			SearchParameters sp = ctx.SearchParameters;

			StrPwEntryDelegate f = sp.DataTransformationFn;
			if(f != null) strData = f(strData, pe);

			Regex rx = ctx.Regex;
			if(rx != null) return rx.IsMatch(strData);

			return (ctx.CompareInfo.IndexOf(strData, sp.SearchString,
				ctx.CompareOptions) >= 0);
		}

		private void SrxpSearch(SearchParameters sp, PwObjectList<PwEntry> lResults,
			IStatusLogger sl)
		{
			PwDatabase pd = new PwDatabase();

			// Prevent removal of history entries (by database maintenance
			// with *default* settings when saving);
			// https://sourceforge.net/p/keepass/discussion/329221/thread/fc60c1a2b3/
			pd.HistoryMaxItems = -1;
			pd.HistoryMaxSize = -1;

			pd.RootGroup = SrxpFilterCloneSelf(sp);

			HashSet<PwUuid> hsResults = new HashSet<PwUuid>();

			XmlDocument xd;
			XPathNodeIterator xpIt = XmlUtilEx.FindNodes(pd, sp.SearchString, sl, out xd);

			if((sl != null) && !sl.SetProgress(98)) return;

			while(xpIt.MoveNext())
			{
				if((sl != null) && !sl.ContinueWork()) return;

				XPathNavigator xpNav = xpIt.Current.Clone();

				while(true)
				{
					XPathNodeType nt = xpNav.NodeType;
					if(nt == XPathNodeType.Root) break;

					if((nt == XPathNodeType.Element) &&
						(xpNav.Name == KdbxFile.ElemEntry))
					{
						SrxpAddResult(hsResults, xpNav);
						break;
					}

					if(!xpNav.MoveToParent()) { Debug.Assert(false); break; }
				}
			}

			EntryHandler eh = delegate(PwEntry pe)
			{
				if(hsResults.Contains(pe.Uuid)) lResults.Add(pe);
				return true;
			};
			TraverseTree(TraversalMethod.PreOrder, null, eh);
			Debug.Assert(lResults.UCount == (uint)hsResults.Count);
		}

		private static void SrxpClearString(bool bIf, PwEntry pe, string strKey)
		{
			if(bIf)
			{
				ProtectedString ps = pe.Strings.Get(strKey);
				if(ps != null)
					pe.Strings.Set(strKey, (ps.IsProtected ?
						ProtectedString.EmptyEx : ProtectedString.Empty));
			}
		}

		private PwGroup SrxpFilterCloneSelf(SearchParameters sp)
		{
			PwGroup pgNew = CloneDeep();
			pgNew.ParentGroup = null;

			DateTime dtNow = DateTime.UtcNow;

			GroupHandler gh = delegate(PwGroup pg)
			{
				if(!sp.SearchInGroupNames) pg.Name = string.Empty;
				if(!sp.SearchInTags) pg.Tags.Clear();

				PwObjectList<PwEntry> l = pg.Entries;
				for(int i = (int)l.UCount - 1; i >= 0; --i)
				{
					PwEntry pe = l.GetAt((uint)i);

					if(sp.ExcludeExpired && pe.Expires && (pe.ExpiryTime <= dtNow)) { }
					else if(sp.RespectEntrySearchingDisabled && !pe.GetSearchingEnabled()) { }
					else continue;

					l.RemoveAt((uint)i);
				}

				return true;
			};

			EntryHandler eh = delegate(PwEntry pe)
			{
				SrxpClearString(!sp.SearchInTitles, pe, PwDefs.TitleField);
				SrxpClearString(!sp.SearchInUserNames, pe, PwDefs.UserNameField);
				SrxpClearString(!sp.SearchInPasswords, pe, PwDefs.PasswordField);
				SrxpClearString(!sp.SearchInUrls, pe, PwDefs.UrlField);
				SrxpClearString(!sp.SearchInNotes, pe, PwDefs.NotesField);

				if(!sp.SearchInOther)
				{
					List<string> lKeys = pe.Strings.GetKeys();
					foreach(string strKey in lKeys)
						SrxpClearString(!PwDefs.IsStandardField(strKey), pe, strKey);
				}

				if(!sp.SearchInTags) pe.Tags.Clear();
				if(!sp.SearchInHistory) pe.History.Clear();

				return true;
			};

			gh(pgNew);
			pgNew.TraverseTree(TraversalMethod.PreOrder, gh, eh);

			return pgNew;
		}

		private static void SrxpAddResult(HashSet<PwUuid> hsResults,
			XPathNavigator xpNavEntry)
		{
			try
			{
				Debug.Assert(xpNavEntry.NamespaceURI == string.Empty);
				if(!xpNavEntry.MoveToChild(KdbxFile.ElemUuid, string.Empty))
				{
					Debug.Assert(false);
					return;
				}

				string strUuid = xpNavEntry.Value;
				if(string.IsNullOrEmpty(strUuid)) { Debug.Assert(false); return; }

				hsResults.Add(new PwUuid(Convert.FromBase64String(strUuid)));
			}
			catch(Exception) { Debug.Assert(false); }
		}
	}
}
