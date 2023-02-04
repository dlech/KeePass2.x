/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2023 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePassLib.Utility;

namespace KeePass.UI
{
	public static class FontUtil
	{
		public static Font CreateFont(string strFamily, float fEmSize, FontStyle fs)
		{
			return CreateFont(strFamily, fEmSize, fs, GraphicsUnit.Point);
		}

		public static Font CreateFont(string strFamily, float fEmSize, FontStyle fs,
			GraphicsUnit gu)
		{
			try { return new Font(strFamily, fEmSize, fs, gu); }
			catch(Exception) { Debug.Assert(false); } // Style unsupported?

			return new Font(strFamily, fEmSize, gu); // Regular style
		}

		public static Font CreateFont(FontFamily ff, float fEmSize, FontStyle fs)
		{
			try { return new Font(ff, fEmSize, fs); }
			catch(Exception) { Debug.Assert(false); } // Style unsupported?

			return new Font(ff, fEmSize);
		}

		public static Font CreateFont(Font fBase, FontStyle fs)
		{
			try { return new Font(fBase, fs); }
			catch(Exception) { Debug.Assert(false); } // Style unsupported?

			return new Font(fBase, fBase.Style); // Clone
		}

		private static void Assign(Control c, Font f)
		{
			if(c == null) { Debug.Assert(false); return; }
			if(f == null) { Debug.Assert(false); return; }

			try
			{
				using(RtlAwareResizeScope r = new RtlAwareResizeScope(c))
				{
					c.Font = f;
				}
			}
			catch(Exception) { Debug.Assert(false); }
		}

		private static Font m_fontDefault = null;
		/// <summary>
		/// Get the default UI font. This might be <c>null</c>!
		/// </summary>
		public static Font DefaultFont
		{
			get { return m_fontDefault; }
		}

		public static void SetDefaultFont(Control c)
		{
			if(c == null) { Debug.Assert(false); return; }

			// Allow specifying the default font once only
			if(m_fontDefault == null) m_fontDefault = c.Font;
		}

		public static void AssignDefault(Control c)
		{
			Assign(c, m_fontDefault);
		}

		private static Font m_fontBold = null;
		public static void AssignDefaultBold(Control c)
		{
			if(c == null) { Debug.Assert(false); return; }

			if(m_fontBold == null)
			{
				try { m_fontBold = new Font(c.Font, FontStyle.Bold); }
				catch(Exception) { Debug.Assert(false); m_fontBold = c.Font; }
			}

			Assign(c, m_fontBold);
		}

		private static Font m_fontItalic = null;
		public static void AssignDefaultItalic(Control c)
		{
			if(c == null) { Debug.Assert(false); return; }

			if(m_fontItalic == null)
			{
				try { m_fontItalic = new Font(c.Font, FontStyle.Italic); }
				catch(Exception) { Debug.Assert(false); m_fontItalic = c.Font; }
			}

			Assign(c, m_fontItalic);
		}

		private static Font m_fontMono = null;
		/// <summary>
		/// Get the default UI monospace font. This might be <c>null</c>!
		/// </summary>
		public static Font MonoFont
		{
			get { return m_fontMono; }
		}

		internal static Font GetDefaultMonoFont(Control cFor)
		{
			Font f = m_fontMono;
			if(f != null) return f;

			try
			{
				float fSize;
				if(m_fontDefault != null) fSize = m_fontDefault.SizeInPoints;
				else if(cFor != null) fSize = cFor.Font.SizeInPoints;
				else { Debug.Assert(false); fSize = 8.25f; }

				f = new Font(FontFamily.GenericMonospace, fSize);
			}
			catch(Exception)
			{
				Debug.Assert(false);
				if(m_fontDefault != null) f = m_fontDefault;
				else if(cFor != null) f = cFor.Font;
			}

			m_fontMono = f;
			return f;
		}

		public static void AssignDefaultMono(Control c, bool bIsPasswordBox)
		{
			if(bIsPasswordBox && Program.Config.UI.PasswordFont.OverrideUIDefault)
				Assign(c, Program.Config.UI.PasswordFont.ToFont());
			else Assign(c, GetDefaultMonoFont(c));
		}

		internal static bool IsInstalled(string strFamily)
		{
			if(string.IsNullOrEmpty(strFamily)) { Debug.Assert(false); return false; }

			try
			{
				using(Font f = new Font(strFamily, 9.0f))
				{
					if(!strFamily.Equals(f.Name, StrUtil.CaseIgnoreCmp))
						return false;
				}
			}
			catch(Exception) { Debug.Assert(false); return false; }

			return true;
		}

		/* private const string FontPartsSeparator = @"/:/";

		public static Font FontIDToFont(string strFontID)
		{
			Debug.Assert(strFontID != null); if(strFontID == null) return null;

			string[] vParts = strFontID.Split(new string[] { FontPartsSeparator },
				StringSplitOptions.None);
			if((vParts == null) || (vParts.Length != 6)) return null;

			float fSize;
			if(!float.TryParse(vParts[1], out fSize)) { Debug.Assert(false); return null; }

			FontStyle fs = FontStyle.Regular;
			if(vParts[2] == "1") fs |= FontStyle.Bold;
			if(vParts[3] == "1") fs |= FontStyle.Italic;
			if(vParts[4] == "1") fs |= FontStyle.Underline;
			if(vParts[5] == "1") fs |= FontStyle.Strikeout;

			return FontUtil.CreateFont(vParts[0], fSize, fs);
		}

		public static string FontToFontID(Font f)
		{
			Debug.Assert(f != null); if(f == null) return string.Empty;

			StringBuilder sb = new StringBuilder();

			sb.Append(f.Name);
			sb.Append(FontPartsSeparator);
			sb.Append(f.SizeInPoints.ToString());
			sb.Append(FontPartsSeparator);
			sb.Append(f.Bold ? "1" : "0");
			sb.Append(FontPartsSeparator);
			sb.Append(f.Italic ? "1" : "0");
			sb.Append(FontPartsSeparator);
			sb.Append(f.Underline ? "1" : "0");
			sb.Append(FontPartsSeparator);
			sb.Append(f.Strikeout ? "1" : "0");

			return sb.ToString();
		} */

		private static void AppendFontStyleFlag(StringBuilder sb, ref FontStyle fs,
			FontStyle fsFlag, string strFlag)
		{
			if((fs & fsFlag) == FontStyle.Regular) return;

			if(sb.Length != 0) sb.Append(", "); // Compatible with Enum.ToString
			sb.Append(strFlag);

			fs ^= fsFlag;
		}

		internal static string FontStyleToString(FontStyle fs)
		{
			if(fs == FontStyle.Regular) return string.Empty;

			StringBuilder sb = new StringBuilder();

			AppendFontStyleFlag(sb, ref fs, FontStyle.Bold, KPRes.Bold);
			AppendFontStyleFlag(sb, ref fs, FontStyle.Italic, KPRes.Italic);
			AppendFontStyleFlag(sb, ref fs, FontStyle.Underline, KPRes.Underline);
			AppendFontStyleFlag(sb, ref fs, FontStyle.Strikeout, KPRes.Strikeout);

			if(fs != FontStyle.Regular)
			{
				Debug.Assert(false);
				if(sb.Length != 0) sb.Append(", "); // Compatible with Enum.ToString
				sb.Append(fs.ToString());
			}

			return sb.ToString();
		}
	}
}
