/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2025 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Text.RegularExpressions;
using System.Windows.Forms;

using KeePassLib.Utility;

namespace KeePass.UI
{
	public sealed class RichTextBuilder
	{
		private readonly StringBuilder m_sb = new StringBuilder();

		private static List<RtfbTag> g_vTags = null;
		private static bool? g_obURtf = null;

		private Font m_fDefault = null;
		public Font DefaultFont
		{
			get { return m_fDefault; }
			set { m_fDefault = value; }
		}

		private sealed class RtfbTag
		{
			public string IdCode { get; private set; }
			public string RtfCode { get; private set; }
			public bool StartTag { get; private set; }
			public FontStyle Style { get; private set; }

			public RtfbTag(string strId, string strRtf, bool bStartTag, FontStyle fs)
			{
				if(string.IsNullOrEmpty(strId)) strId = GenerateRandomIdCode();
				this.IdCode = strId;

				this.RtfCode = strRtf;
				this.StartTag = bStartTag;
				this.Style = fs;
			}

			internal static string GenerateRandomIdCode()
			{
				StringBuilder sb = new StringBuilder(15); // 1 + 12 + 2
				Random r = Program.GlobalRandom;

				// On Hebrew systems, IDs starting with digits break
				// the RTF generation; for safety, we do not use
				// digits at all
				sb.Append((char)('A' + r.Next(26)));
				for(int i = 0; i < 12; ++i)
				{
					int n = r.Next(52); // 62
					if(n < 26) sb.Append((char)('A' + n));
					// else if(n < 52)
					else sb.Append((char)('a' + (n - 26)));
					// else sb.Append((char)('0' + (n - 52)));
				}

				return sb.ToString();
			}

#if DEBUG
			// For debugger display
			public override string ToString()
			{
				return (this.IdCode + " => " + (this.RtfCode ?? string.Empty));
			}
#endif
		}

		public RichTextBuilder()
		{
			EnsureInitializedStatic();
		}

		private static void EnsureInitializedStatic()
		{
			if(g_vTags != null) return;

			// When running under Mono, replace bold and italic text
			// by underlined text, which is rendered correctly (in
			// contrast to bold and italic text)
			string strOvrS = null, strOvrE = null;
			if(MonoWorkarounds.IsRequired(1632))
			{
				strOvrS = "\\ul ";
				strOvrE = "\\ul0 ";
			}

			g_vTags = new List<RtfbTag>
			{
				new RtfbTag(null, (strOvrS ?? "\\b "), true, FontStyle.Bold),
				new RtfbTag(null, (strOvrE ?? "\\b0 "), false, FontStyle.Bold),
				new RtfbTag(null, (strOvrS ?? "\\i "), true, FontStyle.Italic),
				new RtfbTag(null, (strOvrE ?? "\\i0 "), false, FontStyle.Italic),
				new RtfbTag(null, "\\ul ", true, FontStyle.Underline),
				new RtfbTag(null, "\\ul0 ", false, FontStyle.Underline),
				new RtfbTag(null, "\\strike ", true, FontStyle.Strikeout),
				new RtfbTag(null, "\\strike0 ", false, FontStyle.Strikeout)
			};
		}

		public static KeyValuePair<string, string> GetStyleIdCodes(FontStyle fs)
		{
			string strL = null, strR = null;

			foreach(RtfbTag rTag in g_vTags)
			{
				if(rTag.Style == fs)
				{
					if(rTag.StartTag) strL = rTag.IdCode;
					else strR = rTag.IdCode;
				}
			}

			return new KeyValuePair<string, string>(strL, strR);
		}

		public void Append(string str)
		{
			m_sb.Append(str);
		}

		public void AppendLine()
		{
			m_sb.AppendLine();
		}

		public void AppendLine(string str)
		{
			m_sb.AppendLine(str);
		}

		public void Append(string str, FontStyle fs)
		{
			Append(str, fs, null, null, null, null);
		}

		public void Append(string str, FontStyle fs, string strOuterPrefix,
			string strInnerPrefix, string strInnerSuffix, string strOuterSuffix)
		{
			KeyValuePair<string, string> kvpTags = GetStyleIdCodes(fs);

			if(!string.IsNullOrEmpty(strOuterPrefix)) m_sb.Append(strOuterPrefix);

			if(!string.IsNullOrEmpty(kvpTags.Key)) m_sb.Append(kvpTags.Key);
			if(!string.IsNullOrEmpty(strInnerPrefix)) m_sb.Append(strInnerPrefix);
			m_sb.Append(str);
			if(!string.IsNullOrEmpty(strInnerSuffix)) m_sb.Append(strInnerSuffix);
			if(!string.IsNullOrEmpty(kvpTags.Value)) m_sb.Append(kvpTags.Value);

			if(!string.IsNullOrEmpty(strOuterSuffix)) m_sb.Append(strOuterSuffix);
		}

		public void AppendLine(string str, FontStyle fs, string strOuterPrefix,
			string strInnerPrefix, string strInnerSuffix, string strOuterSuffix)
		{
			Append(str, fs, strOuterPrefix, strInnerPrefix, strInnerSuffix, strOuterSuffix);
			m_sb.AppendLine();
		}

		public void AppendLine(string str, FontStyle fs)
		{
			Append(str, fs);
			m_sb.AppendLine();
		}

		private static RichTextBox CreateOpRtb(RichTextBox rtbUI)
		{
			RichTextBox rtbOp = new RichTextBox();
			rtbOp.Visible = false;
			rtbOp.DetectUrls = false;
			rtbOp.HideSelection = true;
			rtbOp.Multiline = true;
			rtbOp.WordWrap = false;

			// rtbOp.BorderStyle = rtbUI.BorderStyle;
			// rtbOp.ScrollBars = rtbUI.ScrollBars;
			rtbOp.Size = rtbUI.Size;

			if(rtbUI.RightToLeft == RightToLeft.Yes)
			{
				rtbOp.RightToLeft = RightToLeft.Yes;
				// rtbOp.SelectionAlignment = HorizontalAlignment.Right;
			}

			return rtbOp;
		}

		private static bool MayGetURtf(RichTextBox rtbUI)
		{
			if(!g_obURtf.HasValue)
			{
				using(RichTextBox rtb = CreateOpRtb(rtbUI))
				{
					// Ensure Rtf != null and encourage Unicode
					rtb.Text = "\u0413\u043E\u0434\r\n\u0397\u03C4\u03B1\r\n" +
						"\u30CE\u30FC\u30C8\r\n\u2211\u2026\u20AC";

					g_obURtf = StrUtil.RtfIsURtf(rtb.Rtf);
				}
			}

			return g_obURtf.Value;
		}

		public void Build(RichTextBox rtb)
		{
			Build(rtb, false);
		}

		internal void Build(RichTextBox rtb, bool bBracesBalanced)
		{
			string strRtf = null;
			Build(rtb, bBracesBalanced, ref strRtf);
		}

		internal bool Build(RichTextBox rtb, bool bBracesBalanced,
			ref string strLastRtf)
		{
			if(rtb == null) throw new ArgumentNullException("rtb");

			string strText = m_sb.ToString();
			bool bURtf = MayGetURtf(rtb);

			// Workaround for encoding bugs in Windows and Mono;
			// Windows: https://sourceforge.net/p/keepass/bugs/1780/
			// Mono: https://bugzilla.novell.com/show_bug.cgi?id=586901
			Dictionary<char, string> dEnc = new Dictionary<char, string>();
			if(bURtf || MonoWorkarounds.IsRequired(586901))
			{
				StringBuilder sbEnc = new StringBuilder();

				for(int i = 0; i < strText.Length; ++i)
				{
					char ch = strText[i];

					if(ch <= '\u00FF') sbEnc.Append(ch);
					else
					{
						string strCharEnc;
						if(!dEnc.TryGetValue(ch, out strCharEnc))
						{
							strCharEnc = RtfbTag.GenerateRandomIdCode();
							dEnc[ch] = strCharEnc;
						}

						sbEnc.Append(strCharEnc);
					}
				}

				strText = sbEnc.ToString();
			}

			string strRtf;
			using(RichTextBox rtbOp = CreateOpRtb(rtb))
			{
				string strFlt = StrUtil.RtfFilterText(strText);
				rtbOp.Text = strFlt;
				Debug.Assert(rtbOp.Text == strFlt); // Test committed

				if(m_fDefault != null)
				{
					rtbOp.SelectAll();
					rtbOp.SelectionFont = m_fDefault;
				}

				strRtf = rtbOp.Rtf;
			}

			foreach(KeyValuePair<char, string> kvpEnc in dEnc)
			{
				strRtf = strRtf.Replace(kvpEnc.Value,
					StrUtil.RtfEncodeChar(kvpEnc.Key));
			}
			foreach(RtfbTag rTag in g_vTags)
			{
				strRtf = strRtf.Replace(rTag.IdCode, rTag.RtfCode);
			}

			if(bBracesBalanced && MonoWorkarounds.IsRequired(2449941153U))
				strRtf = Regex.Replace(strRtf,
					@"(\\)(\{[\u0020-\u005B\u005D-z\w\s]*?)(\})", "$1$2$1$3");

			strRtf = StrUtil.RtfFix(strRtf);

			if(strRtf == strLastRtf) return false;
			rtb.Rtf = strRtf;
			strLastRtf = strRtf;
			return true;
		}
	}
}
