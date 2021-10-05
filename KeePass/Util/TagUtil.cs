/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2021 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using KeePass.Resources;
using KeePass.UI;

using KeePassLib;
using KeePassLib.Utility;

namespace KeePass.Util
{
	internal static class TagUtil
	{
		public static string TagsInheritedToString(List<string> lTags,
			PwGroup pgParent)
		{
			if(lTags == null) { Debug.Assert(false); return string.Empty; }

			string str = StrUtil.TagsToString(lTags, true);

			if(pgParent != null)
			{
				string strP = StrUtil.TagsToString(pgParent.GetTagsInherited(
					true), true);
				if(strP.Length != 0)
					str += ((str.Length == 0) ? "(" : " (") + strP + ")";
			}

			return str;
		}

		public static void MakeInheritedTagsLink(LinkLabel ll, PwGroup pg, Form fParent)
		{
			if(ll == null) { Debug.Assert(false); return; }

			List<KeyValuePair<PwGroup, List<string>>> l =
				new List<KeyValuePair<PwGroup, List<string>>>();
			List<string> lAllTags = new List<string>();

			while(pg != null)
			{
				List<string> lTags = pg.Tags;
				if(lTags.Count != 0)
				{
					l.Add(new KeyValuePair<PwGroup, List<string>>(pg, lTags));
					lAllTags.AddRange(lTags);
				}
				pg = pg.ParentGroup;
			}

			StrUtil.NormalizeTags(lAllTags);

			if(lAllTags.Count == 0)
			{
				ll.Enabled = false;
				ll.Visible = false;
				return;
			}

			string strTitle = KPRes.TagsInheritedCount.Replace(@"{PARAM}",
				lAllTags.Count.ToString());

			ll.Text = strTitle;
			ll.AutoEllipsis = true;

			StringBuilder sb = new StringBuilder();
			for(int i = l.Count - 1; i >= 0; --i)
			{
				KeyValuePair<PwGroup, List<string>> kvp = l[i];

				if(sb.Length != 0) sb.Append(MessageService.NewParagraph);

				sb.Append(KPRes.Group);
				sb.Append(": ");
				sb.Append(kvp.Key.GetFullPath(" \u2192 ", true));
				sb.AppendLine(".");

				sb.Append(KPRes.Tags);
				sb.Append(": ");
				sb.Append(StrUtil.TagsToString(kvp.Value, true));
				sb.Append('.');
			}

			string strMsg = sb.ToString();

			ll.Click += delegate(object sender, EventArgs e)
			{
				if(!VistaTaskDialog.ShowMessageBox(strMsg, strTitle,
					PwDefs.ShortProductName, VtdIcon.Information, fParent))
					MessageService.ShowInfo(strTitle + ":", strMsg);
			};
		}

		public static void MakeTagsButton(Button btn, TextBox tb, ToolTip tt,
			PwGroup pgParent, PwGroup pgTagsSource)
		{
			if((btn == null) || (tb == null)) { Debug.Assert(false); return; }
			Debug.Assert(tt != null);

			Image img = UIUtil.CreateDropDownImage(Properties.Resources.B16x16_KNotes);
			UIUtil.SetButtonImage(btn, img, true);

			UIUtil.SetToolTip(tt, btn, KPRes.TagsAddRemove, true);

			CustomContextMenuStripEx ctx = null;
			Font fItalic = null;

			btn.Click += delegate(object sender, EventArgs e)
			{
				if(ctx == null)
				{
					ctx = new CustomContextMenuStripEx();
					fItalic = FontUtil.CreateFont(ctx.Font, FontStyle.Italic);
				}

				List<string> lCur = StrUtil.StringToTags(tb.Text);

				Dictionary<string, bool> dCur = new Dictionary<string, bool>();
				foreach(string strTag in lCur) dCur[strTag] = true;

				List<string> lParent = ((pgParent != null) ?
					pgParent.GetTagsInherited(true) : new List<string>());
				Dictionary<string, bool> dParent = new Dictionary<string, bool>();
				foreach(string strTag in lParent) dParent[strTag] = true;

				List<string> lAll = new List<string>(lCur);
				if(pgTagsSource != null)
				{
					lAll.AddRange(pgTagsSource.BuildEntryTagsList(false, true));
					StrUtil.NormalizeTags(lAll);
				}

				List<ToolStripItem> lMenuItems = new List<ToolStripItem>();

				if(lAll.Count == 0)
				{
					ToolStripMenuItem tsmi = new ToolStripMenuItem(
						StrUtil.EncodeMenuText("(" + KPRes.TagsNotFound + ")"));
					tsmi.Enabled = false;

					lMenuItems.Add(tsmi);
				}
				else
				{
					for(int i = 0; i < lAll.Count; ++i)
					{
						string strTag = lAll[i]; // Used in Click handler
						bool bHasTag = dCur.ContainsKey(strTag);
						bool bInh = dParent.ContainsKey(strTag);

						string strSuffix = string.Empty;
						if(bInh) strSuffix = " (" + KPRes.Inherited + ")";

						ToolStripMenuItem tsmi = new ToolStripMenuItem(
							StrUtil.EncodeMenuText(strTag + strSuffix));
						UIUtil.SetChecked(tsmi, bHasTag);

						if(bInh) tsmi.Font = fItalic;

						tsmi.Click += delegate(object senderT, EventArgs eT)
						{
							if(bHasTag) lCur.Remove(strTag);
							else
							{
								lCur.Add(strTag);
								StrUtil.NormalizeTags(lCur);
							}

							tb.Text = StrUtil.TagsToString(lCur, true);
						};

						lMenuItems.Add(tsmi);
					}
				}

				ctx.Items.Clear();
				ctx.Items.AddRange(lMenuItems.ToArray());

				ctx.ShowEx(btn);
			};
		}
	}
}
