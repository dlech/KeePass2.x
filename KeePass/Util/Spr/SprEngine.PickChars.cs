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
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using KeePass.Forms;
using KeePass.UI;

using KeePassLib;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.Util.Spr
{
	/// <summary>
	/// String placeholders and field reference replacement engine.
	/// </summary>
	public static partial class SprEngine
	{
		/* private static string ReplacePickPw(string strText, PwEntry pe,
			SprContentFlags cf)
		{
			string str = strText;

			for(int iID = 1; iID < (int.MaxValue - 1); ++iID)
			{
				string strPlaceholder = @"{PICKPASSWORDCHARS";
				if(iID > 1) strPlaceholder += iID.ToString();
				strPlaceholder += @"}";

				if(str.IndexOf(strPlaceholder, StrUtil.CaseIgnoreCmp) >= 0)
				{
					ProtectedString ps = pe.Strings.Get(PwDefs.PasswordField);
					if(ps != null)
					{
						byte[] pb = ps.ReadUtf8();
						bool bNotEmpty = (pb.Length > 0);
						Array.Clear(pb, 0, pb.Length);

						if(bNotEmpty)
						{
							CharPickerForm cpf = new CharPickerForm();
							cpf.InitEx(ps, true, true, 0);

							if(cpf.ShowDialog() == DialogResult.OK)
								str = StrUtil.ReplaceCaseInsensitive(str, strPlaceholder,
									SprEngine.TransformContent(cpf.SelectedCharacters.ReadString(), cf));
							UIUtil.DestroyForm(cpf);
						}
					}

					str = StrUtil.ReplaceCaseInsensitive(str, strPlaceholder, string.Empty);
				}
				else break;
			}

			return str;
		} */

		// Legacy, for backward compatibility only; see PickChars
		private static string ReplacePickPw(string strText, SprContext ctx,
			uint uRecursionLevel)
		{
			if(ctx.Entry == null) { Debug.Assert(false); return strText; }

			string str = strText;

			while(true)
			{
				const string strStart = @"{PICKPASSWORDCHARS";

				int iStart = str.IndexOf(strStart, StrUtil.CaseIgnoreCmp);
				if(iStart < 0) break;

				int iEnd = str.IndexOf('}', iStart);
				if(iEnd < 0) break;

				string strPlaceholder = str.Substring(iStart, iEnd - iStart + 1);

				string strParam = str.Substring(iStart + strStart.Length,
					iEnd - (iStart + strStart.Length));
				string[] vParams = strParam.Split(new char[] { ':' });

				uint uCharCount = 0;
				if(vParams.Length >= 2) uint.TryParse(vParams[1], out uCharCount);

				str = ReplacePickPwPlaceholder(str, strPlaceholder, uCharCount,
					ctx, uRecursionLevel);
			}

			return str;
		}

		private static string ReplacePickPwPlaceholder(string str,
			string strPlaceholder, uint uCharCount, SprContext ctx,
			uint uRecursionLevel)
		{
			if(str.IndexOf(strPlaceholder, StrUtil.CaseIgnoreCmp) < 0) return str;

			ProtectedString ps = ctx.Entry.Strings.Get(PwDefs.PasswordField);
			if(ps != null)
			{
				string strPassword = ps.ReadString();

				string strPick = SprEngine.CompileInternal(strPassword,
					ctx.WithoutContentTransformations(), uRecursionLevel + 1);

				if(!string.IsNullOrEmpty(strPick))
				{
					ProtectedString psPick = new ProtectedString(false, strPick);
					CharPickerForm dlg = new CharPickerForm();
					dlg.InitEx(psPick, true, true, uCharCount, null);

					if(dlg.ShowDialog() == DialogResult.OK)
						str = StrUtil.ReplaceCaseInsensitive(str, strPlaceholder,
							SprEngine.TransformContent(
							dlg.SelectedCharacters.ReadString(), ctx));
					UIUtil.DestroyForm(dlg);
				}
			}

			return StrUtil.ReplaceCaseInsensitive(str, strPlaceholder, string.Empty);
		}

		private static string ReplacePickChars(string strText, SprContext ctx,
			uint uRecursionLevel)
		{
			if(ctx.Entry == null) return strText; // No assert

			string str = strText;

			Dictionary<string, string> dPicked = new Dictionary<string, string>();
			while(true)
			{
				const string strStart = @"{PICKCHARS";

				int iStart = str.IndexOf(strStart, StrUtil.CaseIgnoreCmp);
				if(iStart < 0) break;

				int iEnd = str.IndexOf('}', iStart);
				if(iEnd < 0) break;

				string strPlaceholder = str.Substring(iStart, iEnd - iStart + 1);

				string strParam = str.Substring(iStart + strStart.Length,
					iEnd - (iStart + strStart.Length));

				string strRep = string.Empty;
				bool bEncode = true;

				if(strParam.Length == 0)
					strRep = ShowCharPickDlg(ctx.Entry.Strings.ReadSafe(
						PwDefs.PasswordField), 0, null, ctx, uRecursionLevel);
				else if(strParam.StartsWith(":"))
				{
					string strParams = strParam.Substring(1);
					string[] vParams = strParams.Split(new char[] { ':' },
						StringSplitOptions.None);

					string strField = string.Empty;
					if(vParams.Length >= 1) strField = (vParams[0] ?? string.Empty).Trim();
					if(strField.Length == 0) strField = PwDefs.PasswordField;

					string strOptions = string.Empty;
					if(vParams.Length >= 2) strOptions = (vParams[1] ?? string.Empty);

					Dictionary<string, string> dOptions = new Dictionary<string, string>();
					string[] vOptions = strOptions.Split(new char[] { ',' },
						StringSplitOptions.RemoveEmptyEntries);
					foreach(string strOption in vOptions)
					{
						string[] vKvp = strOption.Split(new char[] { '=' },
							StringSplitOptions.None);
						if(vKvp.Length != 2) continue;

						dOptions[vKvp[0].Trim().ToLower()] = vKvp[1].Trim();
					}

					string strID = string.Empty;
					if(dOptions.ContainsKey("id")) strID = dOptions["id"].ToLower();

					uint uCharCount = 0;
					if(dOptions.ContainsKey("c"))
						uint.TryParse(dOptions["c"], out uCharCount);
					if(dOptions.ContainsKey("count"))
						uint.TryParse(dOptions["count"], out uCharCount);

					bool? bInitHide = null;
					if(dOptions.ContainsKey("hide"))
						bInitHide = StrUtil.StringToBool(dOptions["hide"]);

					string strContent = ctx.Entry.Strings.ReadSafe(strField);
					if(strContent.Length == 0) { } // Leave strRep empty
					else if((strID.Length > 0) && dPicked.ContainsKey(strID))
						strRep = dPicked[strID];
					else
						strRep = ShowCharPickDlg(strContent, uCharCount, bInitHide,
							ctx, uRecursionLevel);

					if(strID.Length > 0) dPicked[strID] = strRep;

					if(dOptions.ContainsKey("conv"))
					{
						int iOffset = 0;
						if(dOptions.ContainsKey("conv-offset"))
							int.TryParse(dOptions["conv-offset"], out iOffset);

						string strConvFmt = string.Empty;
						if(dOptions.ContainsKey("conv-fmt"))
							strConvFmt = dOptions["conv-fmt"];

						string strConv = dOptions["conv"];
						if(strConv.Equals("d", StrUtil.CaseIgnoreCmp))
						{
							strRep = ConvertToDownArrows(strRep, iOffset, strConvFmt);
							bEncode = false;
						}
					}
				}

				str = StrUtil.ReplaceCaseInsensitive(str, strPlaceholder,
					bEncode ? SprEngine.TransformContent(strRep, ctx) : strRep);
			}

			return str;
		}

		private static string ShowCharPickDlg(string strWord, uint uCharCount,
			bool? bInitHide, SprContext ctx, uint uRecursionLevel)
		{
			string strPick = SprEngine.CompileInternal(strWord,
				ctx.WithoutContentTransformations(), uRecursionLevel + 1);

			// No need to show the dialog when there's nothing to pick from
			// (this also prevents the dialog from showing up MaxRecursionDepth
			// times in case of a cyclic {PICKCHARS})
			if(string.IsNullOrEmpty(strPick)) return string.Empty;

			CharPickerForm cpf = new CharPickerForm();
			cpf.InitEx(new ProtectedString(false, strPick), true, true,
				uCharCount, bInitHide);

			string strResult = string.Empty;
			if(cpf.ShowDialog() == DialogResult.OK)
				strResult = cpf.SelectedCharacters.ReadString();

			UIUtil.DestroyForm(cpf);
			return strResult; // Don't transform here
		}

		private static string ConvertToDownArrows(string str, int iOffset,
			string strLayout)
		{
			if(string.IsNullOrEmpty(str)) return string.Empty;

			StringBuilder sb = new StringBuilder();
			for(int i = 0; i < str.Length; ++i)
			{
				// if((sb.Length > 0) && !string.IsNullOrEmpty(strSep)) sb.Append(strSep);

				char ch = str[i];

				int? iDowns = null;
				if(strLayout.Length == 0)
				{
					if((ch >= '0') && (ch <= '9')) iDowns = (int)ch - '0';
					else if((ch >= 'a') && (ch <= 'z')) iDowns = (int)ch - 'a';
					else if((ch >= 'A') && (ch <= 'Z')) iDowns = (int)ch - 'A';
				}
				else if(strLayout.Equals("0a", StrUtil.CaseIgnoreCmp))
				{
					if((ch >= '0') && (ch <= '9')) iDowns = (int)ch - '0';
					else if((ch >= 'a') && (ch <= 'z')) iDowns = (int)ch - 'a' + 10;
					else if((ch >= 'A') && (ch <= 'Z')) iDowns = (int)ch - 'A' + 10;
				}
				else if(strLayout.Equals("a0", StrUtil.CaseIgnoreCmp))
				{
					if((ch >= '0') && (ch <= '9')) iDowns = (int)ch - '0' + 26;
					else if((ch >= 'a') && (ch <= 'z')) iDowns = (int)ch - 'a';
					else if((ch >= 'A') && (ch <= 'Z')) iDowns = (int)ch - 'A';
				}
				else if(strLayout.Equals("1a", StrUtil.CaseIgnoreCmp))
				{
					if((ch >= '1') && (ch <= '9')) iDowns = (int)ch - '1';
					else if(ch == '0') iDowns = 9;
					else if((ch >= 'a') && (ch <= 'z')) iDowns = (int)ch - 'a' + 10;
					else if((ch >= 'A') && (ch <= 'Z')) iDowns = (int)ch - 'A' + 10;
				}
				else if(strLayout.Equals("a1", StrUtil.CaseIgnoreCmp))
				{
					if((ch >= '1') && (ch <= '9')) iDowns = (int)ch - '1' + 26;
					else if(ch == '0') iDowns = 9 + 26;
					else if((ch >= 'a') && (ch <= 'z')) iDowns = (int)ch - 'a';
					else if((ch >= 'A') && (ch <= 'Z')) iDowns = (int)ch - 'A';
				}

				if(!iDowns.HasValue) continue;

				for(int j = 0; j < (iOffset + iDowns); ++j) sb.Append(@"{DOWN}");
			}

			return sb.ToString();
		}
	}
}
