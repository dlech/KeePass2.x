/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2024 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

using KeePass.App;
using KeePass.App.Configuration;
using KeePass.Forms;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util.MultipleValues;

using KeePassLib;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.Util
{
	internal static class DiffUtil
	{
		private const string CueNull = "\u2014"; // Em Dash

		internal static string CueNew
		{
			get { return ("(" + KPRes.New + "/" + KPRes.All + ")"); }
		}

		private const int LvsiValueA = 1;
		private const int LvsiValueB = 3;

		private sealed class DfxContext
		{
			public readonly PwEntry EntryA;
			public readonly PwGroup GroupA;
			public readonly PwDatabase DatabaseA;

			public readonly PwEntry EntryB;
			public readonly PwGroup GroupB;
			public readonly PwDatabase DatabaseB;

			public StringBuilder OutNames = null;
			public List<object> OutLvo = null;

			public bool ReportValues
			{
				get { return (this.OutLvo != null); }
			}

			private List<Image> m_lImages = null;
			public List<Image> Images
			{
				get { EnsureImages(); return m_lImages; }
			}

			private Color m_clrYes = Color.Empty;
			public Color ColorYes
			{
				get { EnsureColors(); return m_clrYes; }
			}

			private Color m_clrNo = Color.Empty;
			public Color ColorNo
			{
				get { EnsureColors(); return m_clrNo; }
			}

			private Color m_clrDiff = Color.Empty;
			public Color ColorDiff
			{
				get { EnsureColors(); return m_clrDiff; }
			}

			public DfxContext(PwEntry peA, PwGroup pgA, PwDatabase pdA,
				PwEntry peB, PwGroup pgB, PwDatabase pdB)
			{
				this.EntryA = peA;
				this.GroupA = pgA;
				this.DatabaseA = pdA;

				this.EntryB = peB;
				this.GroupB = pgB;
				this.DatabaseB = pdB;
			}

			private void EnsureImages()
			{
				if(m_lImages != null) return;

				m_lImages = new List<Image>((int)DfxIcon.Count)
				{
					Properties.Resources.B16x16_MessageBox_Info,
					Properties.Resources.B16x16_Folder_Home,
					Properties.Resources.B16x16_Folder,
					Properties.Resources.B16x16_Identity,
					Properties.Resources.B16x16_Personal,
					Properties.Resources.B16x16_KGPG_Info,
					Properties.Resources.B16x16_FTP,
					Properties.Resources.B16x16_EditPaste,
					Properties.Resources.B16x16_Spreadsheet,
					Properties.Resources.B16x16_History_Clear,
					Properties.Resources.B16x16_History,
					Properties.Resources.B16x16_ASCII,
					Properties.Resources.B16x16_Attach,
					Properties.Resources.B16x16_Colorize,
					Properties.Resources.B16x16_Color_Fill,
					Properties.Resources.B16x16_KNotes,
					Properties.Resources.B16x16_EditCopyLink,
					Properties.Resources.B16x16_BlockDevice,
					Properties.Resources.B16x16_KTouch,
					Properties.Resources.B16x16_Binary
				};
				Debug.Assert(m_lImages.Count == (int)DfxIcon.Count);
			}

			private void EnsureColors()
			{
				if(!m_clrYes.IsEmpty) return;

				bool bDark = UIUtil.IsDarkTheme;

				m_clrYes = (bDark ?
					Color.FromArgb(0, 128, 0) : Color.FromArgb(128, 255, 128));
				m_clrNo = (bDark ?
					Color.FromArgb(64, 64, 64) : Color.FromArgb(192, 192, 192));
				m_clrDiff = (bDark ?
					Color.FromArgb(128, 128, 0) : Color.FromArgb(255, 255, 128));
			}
		}

		private enum DfxChange
		{
			None = 0,
			Edit,
			Add,
			Remove
		}

		private enum DfxIcon
		{
			Info = 0,
			Home,
			Folder,
			Identity,
			User,
			Password,
			Url,
			Notes,
			Icon,
			TimeNo,
			Time,
			Text,
			Attachment,
			ColorF,
			ColorB,
			Tag,
			DocLink,
			Plugin,
			AutoType,
			Binary,

			Count
		}

		public static string GetDiffNames(PwEntry peA, PwGroup pgA, PwDatabase pdA,
			PwEntry peB, PwGroup pgB, PwDatabase pdB, bool bCueNone)
		{
			DfxContext ctx = new DfxContext(peA, pgA, pdA, peB, pgB, pdB);
			ctx.OutNames = new StringBuilder();

			GetDiff(ctx);

			string str = ctx.OutNames.ToString();
			if(string.IsNullOrEmpty(str) && bCueNone) str = DiffUtil.CueNull;
			return str;
		}

		private static List<object> GetDiffLvo(PwEntry peA, PwGroup pgA, PwDatabase pdA,
			PwEntry peB, PwGroup pgB, PwDatabase pdB, out ImageList il,
			out Action<ListView> fInit)
		{
			DfxContext ctx = new DfxContext(peA, pgA, pdA, peB, pgB, pdB);
			ctx.OutLvo = new List<object>();

			GetDiff(ctx);

			Size sz = UIUtil.GetSmallIconSize();
			il = UIUtil.BuildImageListUnscaled(ctx.Images, sz.Width, sz.Height);

			fInit = delegate(ListView lv)
			{
				lv.HotTracking = false;
				lv.HoverSelection = false; // After changing HotTracking
				lv.Activation = ItemActivation.Standard; // After changing HotTracking

				int w = lv.ClientSize.Width - UIUtil.GetVScrollBarWidth();
				int wStatus = DpiUtil.ScaleIntX(28);
				int wValue = (int)(((long)(w - wStatus) * 3) >> 3);
				int wName = w - wValue - wStatus - wValue;

				string strSfxA = string.Empty, strSfxB = string.Empty;
				if((peA != null) && (peB != null))
				{
					if(peA.LastModificationTime < peB.LastModificationTime)
					{
						strSfxA = " (" + KPRes.Older + ")";
						strSfxB = " (" + KPRes.Newer + ")";
					}
					else if(peA.LastModificationTime > peB.LastModificationTime)
					{
						strSfxA = " (" + KPRes.Newer + ")";
						strSfxB = " (" + KPRes.Older + ")";
					}
				}
				else { Debug.Assert(false); }

				lv.Columns.Add(KPRes.Field, wName);
				lv.Columns.Add(KPRes.Value + " 1" + strSfxA, wValue);
				lv.Columns.Add("~", wStatus, HorizontalAlignment.Center);
				lv.Columns.Add(KPRes.Value + " 2" + strSfxB, wValue);

				lv.ContextMenuStrip = ConstructContextMenu(lv);
			};

			return ctx.OutLvo;
		}

		private static void GetDiff(DfxContext ctx)
		{
			PwEntry peA = ctx.EntryA, peB = ctx.EntryB;
			if((peA == null) || (peB == null)) { Debug.Assert(false); return; }

			// pe*.ParentGroup is not suitable here (history entries, ...)
			PwGroup pgA = ctx.GroupA, pgB = ctx.GroupB;
			// Debug.Assert((peA.ParentGroup == pgA) || (peA.ParentGroup == null));
			// Debug.Assert((peB.ParentGroup == pgB) || (peB.ParentGroup == null));

			PwDatabase pdA = ctx.DatabaseA, pdB = ctx.DatabaseB;
#if DEBUG
			PwDatabase pdFA = Program.MainForm.DocumentManager.FindContainerOf(peA);
			PwDatabase pdFB = Program.MainForm.DocumentManager.FindContainerOf(peB);
			Debug.Assert((pdFA == pdA) || (pdFA == null));
			Debug.Assert((pdFB == pdB) || (pdFB == null));
#endif

			string strA, strB;
			DfxChange c;
			bool bEqual;
			bool bOutLvo = (ctx.OutLvo != null);
			bool bReportValues = ctx.ReportValues;

			if(bOutLvo)
				ctx.OutLvo.Add(new ListViewGroup(KPRes.Database + " & " + KPRes.Group));

			strA = null;
			strB = null;
			if(bReportValues)
			{
				if(pdA != null) strA = pdA.IOConnectionInfo.GetDisplayName();
				if(pdB != null) strB = pdB.IOConnectionInfo.GetDisplayName();
			}
			Report(ctx, DfxIcon.Home, KPRes.Database, strA, strB, DfxEquals(pdA, pdB));

			strA = null;
			strB = null;
			if(bReportValues)
			{
				if(pgA != null) strA = pgA.GetFullPath(true, true);
				if(pgB != null) strB = pgB.GetFullPath(true, true);
			}
			Report(ctx, DfxIcon.Folder, KPRes.Group, strA, strB, DfxEquals(pgA, pgB));

			if(bOutLvo) ctx.OutLvo.Add(new ListViewGroup(KPRes.General));

			ReportString(ctx, DfxIcon.Identity, KPRes.Title, PwDefs.TitleField, true);
			ReportString(ctx, DfxIcon.User, KPRes.UserName, PwDefs.UserNameField, true);
			ReportString(ctx, DfxIcon.Password, KPRes.Password, PwDefs.PasswordField, true);
			ReportString(ctx, DfxIcon.Url, KPRes.Url, PwDefs.UrlField, true);
			ReportString(ctx, DfxIcon.Notes, KPRes.Notes, PwDefs.NotesField, true);

			strA = null;
			strB = null;
			if(bReportValues)
			{
				strA = GetIconText(peA, pdA, pdB);
				strB = GetIconText(peB, pdB, pdA);
			}
			bool bStdIconA = peA.CustomIconUuid.Equals(PwUuid.Zero);
			bEqual = (peA.CustomIconUuid.Equals(peB.CustomIconUuid) &&
				(!bStdIconA || (peA.IconId == peB.IconId)));
			Report(ctx, DfxIcon.Icon, KPRes.Icon, strA, strB, bEqual);

			Report(ctx, DfxIcon.Info, KPRes.QualityEstimation, peA.QualityCheck,
				peB.QualityCheck);

			Report(ctx, DfxIcon.TimeNo, KPRes.ExpiryTime, peA.Expires, peA.ExpiryTime,
				peB.Expires, peB.ExpiryTime);

			string[] v = GetNames<ProtectedString>(peA.Strings, peB.Strings, true);
			if(bOutLvo && (v.Length != 0))
				ctx.OutLvo.Add(new ListViewGroup(KPRes.Custom));
			foreach(string str in v) { ReportString(ctx, DfxIcon.Text, str, str, false); }

			v = GetNames<ProtectedBinary>(peA.Binaries, peB.Binaries, false);
			if(bOutLvo && (v.Length != 0))
				ctx.OutLvo.Add(new ListViewGroup(KPRes.Attachments));
			foreach(string str in v)
			{
				Report(ctx, DfxIcon.Attachment, str, peA.Binaries.Get(str),
					peB.Binaries.Get(str));
			}

			if(bOutLvo) ctx.OutLvo.Add(new ListViewGroup(KPRes.Properties));

			Report(ctx, DfxIcon.ColorF, KPRes.ForegroundColor, peA.ForegroundColor,
				peB.ForegroundColor);
			Report(ctx, DfxIcon.ColorB, KPRes.BackgroundColor, peA.BackgroundColor,
				peB.BackgroundColor);

			List<string> lA = peA.Tags, lB = peB.Tags;
			strA = null;
			strB = null;
			if(bReportValues)
			{
				if(lA.Count != 0) strA = StrUtil.TagsToString(lA, true);
				if(lB.Count != 0) strB = StrUtil.TagsToString(lB, true);
			}
			if(lA.Count == 0) c = ((lB.Count == 0) ? DfxChange.None : DfxChange.Add);
			else if(lB.Count == 0) c = DfxChange.Remove;
			else c = (MemUtil.ListsEqual<string>(lA, lB) ? DfxChange.None : DfxChange.Edit);
			Report(ctx, DfxIcon.Tag, KPRes.Tags, strA, strB, c);

			strA = peA.OverrideUrl;
			strB = peB.OverrideUrl;
			c = DfxEquals(ref strA, ref strB, true);
			Report(ctx, DfxIcon.Url, KPRes.UrlOverride, strA, strB, c);

			strA = null;
			strB = null;
			if(bReportValues)
			{
				strA = peA.Uuid.ToHexString();
				strB = peB.Uuid.ToHexString();
			}
			Report(ctx, DfxIcon.DocLink, KPRes.Uuid, strA, strB, peA.Uuid.Equals(peB.Uuid));

			v = GetNames<string>(peA.CustomData, peB.CustomData, false);
			if(bOutLvo && (v.Length != 0))
				ctx.OutLvo.Add(new ListViewGroup(KPRes.Custom + " (" + KPRes.Plugins + ")"));
			foreach(string str in v)
			{
				strA = peA.CustomData.Get(str);
				strB = peB.CustomData.Get(str);
				DateTime? odtA = peA.CustomData.GetLastModificationTime(str);
				DateTime? odtB = peB.CustomData.GetLastModificationTime(str);
				if(strA == null) c = ((strB == null) ? DfxChange.None : DfxChange.Add);
				else if(strB == null) c = DfxChange.Remove;
				else
					c = (((strA == strB) && (odtA.HasValue == odtB.HasValue) &&
						(!odtA.HasValue || TimeUtil.EqualsFloor(odtA.Value, odtB.Value))) ?
						DfxChange.None : DfxChange.Edit);
				if(bReportValues)
				{
					if(odtA.HasValue && (strA != null))
						strA += (((strA.Length != 0) ? " (" : "(") +
							TimeUtil.ToDisplayString(odtA.Value) + ")");
					if(odtB.HasValue && (strB != null))
						strB += (((strB.Length != 0) ? " (" : "(") +
							TimeUtil.ToDisplayString(odtB.Value) + ")");
				}
				Report(ctx, DfxIcon.Plugin, str, strA, strB, c);
			}

			if(bOutLvo) ctx.OutLvo.Add(new ListViewGroup(KPRes.AutoType));

			strA = MultipleValuesEx.CueString;
			Report(ctx, DfxIcon.AutoType, KPRes.AutoType, // + " (" + KPRes.All + ")",
				strA, strA, peA.AutoType.Equals(peB.AutoType));

			if(bOutLvo)
			{
				ctx.OutLvo.Add(new ListViewGroup(KPRes.History));

				Report(ctx, DfxIcon.Time, KPRes.CreationTime, true,
					peA.CreationTime, true, peB.CreationTime);
				Report(ctx, DfxIcon.Time, KPRes.LastModificationTime, true,
					peA.LastModificationTime, true, peB.LastModificationTime);
				if((Program.Config.UI.UIFlags & (ulong)AceUIFlags.ShowLastAccessTime) != 0)
					Report(ctx, DfxIcon.Time, KPRes.LastAccessTime, true,
						peA.LastAccessTime, true, peB.LastAccessTime);
			}

			const PwCompareOptions coAll = (PwCompareOptions.NullEmptyEquivStd |
				PwCompareOptions.IgnoreParentGroup | PwCompareOptions.IgnoreLastAccess |
				PwCompareOptions.IgnoreHistory); // Cf. PwDatabase.MergeIn
			const MemProtCmpMode mpcmAll = MemProtCmpMode.CustomOnly;
			bool bNamesEmpty = ((ctx.OutNames != null) && (ctx.OutNames.Length == 0));
			if(bNamesEmpty || bOutLvo)
			{
				bEqual = peA.EqualsEntry(peB, coAll, mpcmAll);
				if(bOutLvo) ctx.OutLvo.Add(new ListViewGroup(KPRes.All));
				strA = MultipleValuesEx.CueString;
				Report(ctx, DfxIcon.Binary, ((bNamesEmpty && !bOutLvo) ?
					"..." : KPRes.AllDataInclOrg), strA, strA, bEqual);
			}
		}

		private static string[] GetNames<T>(IEnumerable<KeyValuePair<string, T>> eA,
			IEnumerable<KeyValuePair<string, T>> eB, bool bExclStd)
		{
			Dictionary<string, bool> d = null;
			foreach(KeyValuePair<string, T> kvp in eA)
			{
				if(bExclStd && PwDefs.IsStandardField(kvp.Key)) continue;
				if(d == null) d = new Dictionary<string, bool>();
				d[kvp.Key] = true;
			}
			foreach(KeyValuePair<string, T> kvp in eB)
			{
				if(bExclStd && PwDefs.IsStandardField(kvp.Key)) continue;
				if(d == null) d = new Dictionary<string, bool>();
				d[kvp.Key] = true;
			}
			if(d == null) return MemUtil.EmptyArray<string>();

			string[] v = new string[d.Count];
			d.Keys.CopyTo(v, 0);
			Array.Sort<string>(v, StrUtil.CompareNaturally);
			return v;
		}

		private static DfxChange DfxEquals(object oA, object oB)
		{
			if(oA == null) return ((oB == null) ? DfxChange.None : DfxChange.Add);
			if(oB == null) return DfxChange.Remove;
			return ((oA == oB) ? DfxChange.None : DfxChange.Edit);
		}

		private static DfxChange DfxEquals(ref string strA, ref string strB,
			bool bEmptyEqualsNull)
		{
			if(bEmptyEqualsNull)
			{
				if((strA != null) && (strA.Length == 0)) strA = null;
				if((strB != null) && (strB.Length == 0)) strB = null;
			}

			if(strA == null) return ((strB == null) ? DfxChange.None : DfxChange.Add);
			if(strB == null) return DfxChange.Remove;
			return ((strA == strB) ? DfxChange.None : DfxChange.Edit);
		}

		private static void Report(DfxContext ctx, DfxIcon ic, string strName,
			string strValueA, string strValueB, bool bEqual)
		{
			Report(ctx, ic, strName, strValueA, strValueA, strValueB, strValueB,
				(bEqual ? DfxChange.None : DfxChange.Edit));
		}

		private static void Report(DfxContext ctx, DfxIcon ic, string strName,
			string strValueA, string strValueB, DfxChange c)
		{
			Report(ctx, ic, strName, strValueA, strValueA, strValueB, strValueB, c);
		}

		private static void Report(DfxContext ctx, DfxIcon ic, string strName,
			string strValueA, object oValueA, string strValueB, object oValueB,
			DfxChange c)
		{
			if(string.IsNullOrEmpty(strName)) { Debug.Assert(false); return; }

			StringBuilder sbNames = ctx.OutNames;
			if((sbNames != null) && (c != DfxChange.None))
			{
				bool bQuote = ((strName.IndexOf(',') >= 0) ||
					char.IsWhiteSpace(strName[0]) ||
					char.IsWhiteSpace(strName[strName.Length - 1]));

				if(sbNames.Length != 0) sbNames.Append(", ");

				if(bQuote) sbNames.Append('\'');
				sbNames.Append(strName);
				if(bQuote) sbNames.Append('\'');

				if(c == DfxChange.Add)
					sbNames.Append(" (+)");
				else if(c == DfxChange.Remove)
					sbNames.Append(" (\u2212)"); // Minus
			}

			List<object> lLvo = ctx.OutLvo;
			if(lLvo != null)
			{
				string strStatus;
				Color? oclrA, oclrB;
				switch(c)
				{
					case DfxChange.None:
						// Debug.Assert(strValueA == strValueB);
						Debug.Assert(StrUtil.NormalizeNewLines((strValueA ?? "#%$&!@"), false) ==
							StrUtil.NormalizeNewLines((strValueB ?? "#%$&!@"), false));
						strStatus = "=";
						oclrA = null;
						oclrB = null;
						break;
					case DfxChange.Add:
						Debug.Assert(strValueA == null);
						strStatus = "\u25E8"; // Square with Right Half Black
						oclrA = ctx.ColorNo;
						oclrB = ctx.ColorYes;
						break;
					case DfxChange.Remove:
						Debug.Assert(strValueB == null);
						strStatus = "\u25E7"; // Square with Left Half Black
						oclrA = ctx.ColorYes;
						oclrB = ctx.ColorNo;
						break;
					default:
						// Debug.Assert(strValueA != strValueB);
						strStatus = "\u2260"; // Not Equal To
						oclrA = ctx.ColorDiff;
						oclrB = ctx.ColorDiff;
						break;
				}

				int iImage = (int)ic;
				if(ic == DfxIcon.Attachment)
				{
					Image img = FileIcons.GetImageForName(strName, null);
					if(img != null)
					{
						iImage = ctx.Images.Count;
						ctx.Images.Add(img);
					}
					else { Debug.Assert(false); }
				}

				ListViewItem lvi = new ListViewItem(strName, iImage);

				Debug.Assert(lvi.SubItems.Count == LvsiValueA);
				ListViewItem.ListViewSubItem lvsiA = lvi.SubItems.Add(
					StrUtil.MultiToSingleLine((strValueA ?? DiffUtil.CueNull).Trim()));

				ListViewItem.ListViewSubItem lvsiS = lvi.SubItems.Add(strStatus);

				Debug.Assert(lvi.SubItems.Count == LvsiValueB);
				ListViewItem.ListViewSubItem lvsiB = lvi.SubItems.Add(
					StrUtil.MultiToSingleLine((strValueB ?? DiffUtil.CueNull).Trim()));

				lvsiA.Tag = oValueA;
				lvsiB.Tag = oValueB;

				if(oclrA.HasValue && oclrB.HasValue)
				{
					lvi.UseItemStyleForSubItems = false;
					lvsiA.BackColor = oclrA.Value;
					lvsiS.BackColor = UIUtil.ColorMiddle(oclrA.Value, oclrB.Value);
					lvsiB.BackColor = oclrB.Value;
				}

				lLvo.Add(lvi);
			}
		}

		private static void Report(DfxContext ctx, DfxIcon ic, string strName,
			ProtectedString psValueA, bool bSensitiveA, ProtectedString psValueB,
			bool bSensitiveB, bool bCheckProt)
		{
			bool bOutLvo = (ctx.OutLvo != null);
			bool bA = (psValueA != null), bB = (psValueB != null);

			string strA = null, strB = null;
			if(ctx.ReportValues)
			{
				if(bA)
				{
					if(bOutLvo && bSensitiveA) strA = string.Empty; // Set below
					else
					{
						strA = psValueA.ReadString().Trim();
						if(bCheckProt && psValueA.IsProtected)
							strA += ((strA.Length == 0) ? "(***)" : " (***)");
					}
				}

				if(bB)
				{
					if(bOutLvo && bSensitiveB) strB = string.Empty; // Set below
					else
					{
						strB = psValueB.ReadString().Trim();
						if(bCheckProt && psValueB.IsProtected)
							strB += ((strB.Length == 0) ? "(***)" : " (***)");
					}
				}
			}

			DfxChange c;
			if(!bA) c = (bB ? DfxChange.Add : DfxChange.None);
			else if(!bB) c = DfxChange.Remove;
			else
			{
				// c = (psValueA.Equals(psValueB, bCheckProt) ? DfxChange.None : DfxChange.Edit);

				if(bCheckProt && (psValueA.IsProtected != psValueB.IsProtected))
					c = DfxChange.Edit;
				else
				{
					char[] vA = null, vB = null;
					try
					{
						vA = psValueA.ReadChars();
						vB = psValueB.ReadChars();
						c = (StrUtil.Equals(vA, vB, true) ? DfxChange.None : DfxChange.Edit);
					}
					catch(Exception)
					{
						Debug.Assert(false);
						c = (psValueA.Equals(psValueB, false) ? DfxChange.None : DfxChange.Edit);
					}
					finally
					{
						if(psValueA.IsProtected) MemUtil.ZeroArray<char>(vA);
						if(psValueB.IsProtected) MemUtil.ZeroArray<char>(vB);
					}
				}
			}

			Report(ctx, ic, strName, strA, psValueA, strB, psValueB, c);

			if(bOutLvo && (bSensitiveA || bSensitiveB))
			{
				int i = ctx.OutLvo.Count - 1;
				ListViewItem lvi = ((i >= 0) ? (ctx.OutLvo[i] as ListViewItem) : null);
				if((lvi != null) && (lvi.Text == strName))
				{
					LvfItem lvfi = new LvfItem(lvi);

					if(bSensitiveA)
					{
						LvfSubItem si = lvfi.SubItems[LvsiValueA];
						if(bA)
						{
							ProtectedString ps = psValueA.Trim();
							if(bCheckProt && ps.IsProtected)
								ps += (ps.IsEmpty ? "(***)" : " (***)");
							Debug.Assert(si.Text.Length == 0);
							si.SetText(ps);
						}
						si.Flags |= LvfiFlags.Sensitive;
					}
					if(bSensitiveB)
					{
						LvfSubItem si = lvfi.SubItems[LvsiValueB];
						if(bB)
						{
							ProtectedString ps = psValueB.Trim();
							if(bCheckProt && ps.IsProtected)
								ps += (ps.IsEmpty ? "(***)" : " (***)");
							Debug.Assert(si.Text.Length == 0);
							si.SetText(ps);
						}
						si.Flags |= LvfiFlags.Sensitive;
					}

					ctx.OutLvo[i] = lvfi;
				}
				else { Debug.Assert(false); }
			}
		}

		private static void ReportString(DfxContext ctx, DfxIcon ic, string strName,
			string strKey, bool bStd)
		{
			Debug.Assert(bStd == PwDefs.IsStandardField(strKey));

			ProtectedString psA = ctx.EntryA.Strings.Get(strKey);
			ProtectedString psB = ctx.EntryB.Strings.Get(strKey);

			if(bStd)
			{
				if((psA != null) && psA.IsEmpty) psA = null;
				if((psB != null) && psB.IsEmpty) psB = null;
			}

			// if((ic == DfxIcon.Text) && (((psB != null) && psB.IsProtected) ||
			//	((psB == null) && (psA != null) && psA.IsProtected)))
			//	ic = DfxIcon.Password;

			bool bSensA = ((psA != null) && psA.IsProtected);
			bool bSensB = ((psB != null) && psB.IsProtected);
			if(strKey == PwDefs.PasswordField) { bSensA = true; bSensB = true; }

			Report(ctx, ic, strName, psA, bSensA, psB, bSensB, !bStd);
		}

		private static void Report(DfxContext ctx, DfxIcon ic, string strName,
			ProtectedBinary pbValueA, ProtectedBinary pbValueB)
		{
			string strA = null, strB = null;
			if(ctx.ReportValues)
			{
				if(pbValueA != null) strA = StrUtil.FormatDataSizeKB(pbValueA.Length);
				if(pbValueB != null) strB = StrUtil.FormatDataSizeKB(pbValueB.Length);
			}

			DfxChange c;
			if(pbValueA == null) c = ((pbValueB == null) ? DfxChange.None : DfxChange.Add);
			else if(pbValueB == null) c = DfxChange.Remove;
			else c = (pbValueA.Equals(pbValueB, false) ? DfxChange.None : DfxChange.Edit);

			Report(ctx, ic, strName, strA, pbValueA, strB, pbValueB, c);
		}

		private static void Report(DfxContext ctx, DfxIcon ic, string strName,
			bool bHasValueA, DateTime dtValueA, bool bHasValueB, DateTime dtValueB)
		{
			string strA = null, strB = null;
			if(ctx.ReportValues)
			{
				if(bHasValueA) strA = TimeUtil.ToDisplayString(dtValueA);
				if(bHasValueB) strB = TimeUtil.ToDisplayString(dtValueB);
			}

			DfxChange c;
			if(!bHasValueA) c = (bHasValueB ? DfxChange.Add : DfxChange.None);
			else if(!bHasValueB) c = DfxChange.Remove;
			else
				c = (TimeUtil.EqualsFloor(dtValueA, dtValueB) ?
					DfxChange.None : DfxChange.Edit);

			Report(ctx, ic, strName, strA, strB, c);
		}

		private static void Report(DfxContext ctx, DfxIcon ic, string strName,
			Color clrValueA, Color clrValueB)
		{
			bool bA = !UIUtil.ColorsEqual(clrValueA, Color.Empty);
			bool bB = !UIUtil.ColorsEqual(clrValueB, Color.Empty);

			string strA = null, strB = null;
			if(ctx.ReportValues)
			{
				if(bA) strA = UIUtil.ColorToString(clrValueA);
				if(bB) strB = UIUtil.ColorToString(clrValueB);
			}

			DfxChange c;
			if(!bA) c = (bB ? DfxChange.Add : DfxChange.None);
			else if(!bB) c = DfxChange.Remove;
			else
				c = (UIUtil.ColorsEqual(clrValueA, clrValueB) ?
					DfxChange.None : DfxChange.Edit);

			Report(ctx, ic, strName, strA, strB, c);
		}

		private static void Report(DfxContext ctx, DfxIcon ic, string strName,
			bool bValueA, bool bValueB)
		{
			Report(ctx, ic, strName, (bValueA ? KPRes.Yes : KPRes.No),
				(bValueB ? KPRes.Yes : KPRes.No), (bValueA == bValueB));
		}

		private static string GetIconText(PwEntry pe, PwDatabase pd,
			PwDatabase pdOther)
		{
			if(pe.CustomIconUuid.Equals(PwUuid.Zero))
				return (KPRes.BuiltInU + ": " + ((long)pe.IconId).ToString());

			StringBuilder sb = new StringBuilder();
			sb.Append(KPRes.Custom);

			string strName = null;
			// Show the index and the name only if the two entries are
			// in the same database, otherwise users might be confused
			// by different indices/names
			if((pd != null) && (pd == pdOther))
			{
				int i = pd.GetCustomIconIndex(pe.CustomIconUuid);
				if(i >= 0)
				{
					sb.Append(": ");
					sb.Append(i);

					strName = pd.CustomIcons[i].Name;
				}
				else { Debug.Assert(false); }
			}

			sb.Append(" (");

			if(!string.IsNullOrEmpty(strName))
			{
				sb.Append('\'');
				sb.Append(strName);
				sb.Append("\', ");
			}

			sb.Append(pe.CustomIconUuid.ToHexString());
			sb.Append(')');

			return sb.ToString();
		}

		public static void ShowDiff(PwEntry peA, PwGroup pgA, PwDatabase pdA,
			PwEntry peB, PwGroup pgB, PwDatabase pdB)
		{
			ImageList il;
			Action<ListView> fInit;
			List<object> lLvo = GetDiffLvo(peA, pgA, pdA, peB, pgB, pdB,
				out il, out fInit);

			try
			{
				ListViewForm dlg = new ListViewForm();
				dlg.InitEx(KPRes.CompareEntries, KPRes.EntryDifferences + ".", null,
					Properties.Resources.B48x48_View_Detailed, lLvo, il, fInit);
				dlg.FlagsEx |= LvfFlags.StandardTheme; // For colors on hover
				dlg.FlagsEx &= ~(LvfFlags.Print | LvfFlags.Export); // MultiToSingleLine
				dlg.DatabaseEx = (pdA ?? pdB);

				UIUtil.ShowDialogAndDestroy(dlg);
			}
			finally { if(il != null) il.Dispose(); }
		}

		private static ListViewItem GetSelectedItem(ListView lv)
		{
			if(lv == null) { Debug.Assert(false); return null; }

			ListView.SelectedListViewItemCollection lvsic = lv.SelectedItems;
			if(lvsic.Count != 1) return null;

			return lvsic[0];
		}

		private static CustomContextMenuStripEx ConstructContextMenu(ListView lv)
		{
			List<ToolStripItem> l = new List<ToolStripItem>();
			AccessKeyManagerEx ak = new AccessKeyManagerEx();

			ToolStripMenuItem tsmiCopyA = new ToolStripMenuItem(
				ak.CreateText(KPRes.CopyObject, true, null, "1"),
				Properties.Resources.B16x16_EditCopy);
			tsmiCopyA.Click += delegate(object sender, EventArgs e)
			{
				OnCtxCopyValue(lv, LvsiValueA);
			};
			l.Add(tsmiCopyA);

			ToolStripMenuItem tsmiCopyB = new ToolStripMenuItem(
				ak.CreateText(KPRes.CopyObject, true, null, "2"),
				Properties.Resources.B16x16_EditCopy);
			tsmiCopyB.Click += delegate(object sender, EventArgs e)
			{
				OnCtxCopyValue(lv, LvsiValueB);
			};
			l.Add(tsmiCopyB);

			l.Add(new ToolStripSeparator());

			ToolStripMenuItem tsmiSaveA = new ToolStripMenuItem(
				ak.CreateText(KPRes.SaveObjectAs + "...", true, null, "1"),
				Properties.Resources.B16x16_FileSaveAs);
			tsmiSaveA.Click += delegate(object sender, EventArgs e)
			{
				OnCtxSaveValue(lv, LvsiValueA);
			};
			l.Add(tsmiSaveA);

			ToolStripMenuItem tsmiSaveB = new ToolStripMenuItem(
				ak.CreateText(KPRes.SaveObjectAs + "...", true, null, "2"),
				Properties.Resources.B16x16_FileSaveAs);
			tsmiSaveB.Click += delegate(object sender, EventArgs e)
			{
				OnCtxSaveValue(lv, LvsiValueB);
			};
			l.Add(tsmiSaveB);

			CustomContextMenuStripEx ctx = new CustomContextMenuStripEx();
			ctx.Items.AddRange(l.ToArray());

			ctx.Opening += delegate(object sender, CancelEventArgs e)
			{
				ListViewItem lvi = GetSelectedItem(lv);
				object oA = ((lvi != null) ? lvi.SubItems[LvsiValueA].Tag : null);
				object oB = ((lvi != null) ? lvi.SubItems[LvsiValueB].Tag : null);

				tsmiCopyA.Enabled = ((oA != null) && ((oA is string) || (oA is ProtectedString)));
				tsmiCopyB.Enabled = ((oB != null) && ((oB is string) || (oB is ProtectedString)));

				tsmiSaveA.Enabled = ((oA != null) && (oA is ProtectedBinary));
				tsmiSaveB.Enabled = ((oB != null) && (oB is ProtectedBinary));
			};

			return ctx;
		}

		private static void OnCtxCopyValue(ListView lv, int iSubItem)
		{
			ListViewItem lvi = GetSelectedItem(lv);
			object o = ((lvi != null) ? lvi.SubItems[iSubItem].Tag : null);

			string str = (o as string);
			if(str == null)
			{
				ProtectedString ps = (o as ProtectedString);
				if(ps != null) str = ps.ReadString();
			}
			if(str == null) { Debug.Assert(false); str = string.Empty; }

			ClipboardUtil.Copy(str, false, true, null, null, IntPtr.Zero);
		}

		private static void OnCtxSaveValue(ListView lv, int iSubItem)
		{
			ListViewItem lvi = GetSelectedItem(lv);
			object o = ((lvi != null) ? lvi.SubItems[iSubItem].Tag : null);

			ProtectedBinary pb = (o as ProtectedBinary);
			if(pb != null)
			{
				string str = FileDialogsEx.ShowAttachmentSaveFileDialog(lvi.Text);
				if(!string.IsNullOrEmpty(str))
				{
					byte[] pbData = pb.ReadData();
					try { File.WriteAllBytes(str, pbData); }
					catch(Exception ex) { MessageService.ShowWarning(str, ex); }
					finally
					{
						if(pb.IsProtected) MemUtil.ZeroByteArray(pbData);
					}
				}
			}
			else { Debug.Assert(false); }
		}
	}
}
