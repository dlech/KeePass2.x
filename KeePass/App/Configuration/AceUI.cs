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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text;

using KeePass.Resources;
using KeePass.UI;

using KeePassLib.Utility;

namespace KeePass.App.Configuration
{
	[Flags]
	public enum AceKeyUIFlags : ulong
	{
		None = 0,
		EnablePassword = 0x1,
		EnableKeyFile = 0x2,
		EnableUserAccount = 0x4,
		EnableHidePassword = 0x8,
		DisablePassword = 0x100,
		DisableKeyFile = 0x200,
		DisableUserAccount = 0x400,
		DisableHidePassword = 0x800,
		CheckPassword = 0x10000,
		CheckKeyFile = 0x20000,
		CheckUserAccount = 0x40000,
		CheckHidePassword = 0x80000,
		UncheckPassword = 0x1000000,
		UncheckKeyFile = 0x2000000,
		UncheckUserAccount = 0x4000000,
		UncheckHidePassword = 0x8000000
	}

	[Flags]
	public enum AceUIFlags : ulong
	{
		None = 0,

		DisableOptions = 0x1,
		DisablePlugins = 0x2,
		DisableTriggers = 0x4,
		DisableKeyChangeDays = 0x8,
		HidePwQuality = 0x10,
		DisableUpdateCheck = 0x20,
		DisableXmlReplace = 0x40,
		DisableDbSettings = 0x80,

		HideBuiltInPwGenPrfInEntryDlg = 0x10000,
		ShowLastAccessTime = 0x20000,
		HideNewDbInfoDialogs = 0x40000,
		HideAutoTypeObfInfo = 0x80000,
		NoQuickSearchClear = 0x100000,
		SecureDesktopIme = 0x200000,
		AdjustWeakKdfParameters = 0x400000
	}

	[Flags]
	public enum AceAutoTypeCtxFlags : long
	{
		None = 0,

		ColTitle = 0x1,
		ColUserName = 0x2,
		ColPassword = 0x4,
		ColUrl = 0x8,
		ColNotes = 0x10,
		ColSequence = 0x20,
		ColSequenceComments = 0x40,

		Default = (ColTitle | ColUserName | ColUrl | ColSequence)
	}

	public sealed class AceUI
	{
		public AceUI()
		{
		}

		private AceTrayIcon m_tray = new AceTrayIcon();
		public AceTrayIcon TrayIcon
		{
			get { return m_tray; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_tray = value;
			}
		}

		private AceHiding m_uiHiding = new AceHiding();
		public AceHiding Hiding
		{
			get { return m_uiHiding; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_uiHiding = value;
			}
		}

		private bool m_bRepeatPwOnlyWhenHidden = true;
		[DefaultValue(true)]
		public bool RepeatPasswordOnlyWhenHidden
		{
			get { return m_bRepeatPwOnlyWhenHidden; }
			set { m_bRepeatPwOnlyWhenHidden = value; }
		}

		private AceFont m_font = new AceFont();
		public AceFont StandardFont
		{
			get { return m_font; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_font = value;
			}
		}

		// internal const float PasswordFontSizeMaximum = 12.0f; // Points

		private AceFont m_fontPasswords = new AceFont(true);
		public AceFont PasswordFont
		{
			get { return m_fontPasswords; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_fontPasswords = value;
			}
		}

		private bool m_bForceSysFont = true;
		[DefaultValue(true)]
		public bool ForceSystemFontUnix
		{
			get { return m_bForceSysFont; }
			set { m_bForceSysFont = value; }
		}

		private BannerStyle m_bannerStyle = BannerStyle.Dark;
		public BannerStyle BannerStyle
		{
			get { return m_bannerStyle; }
			set { m_bannerStyle = value; }
		}

		[DefaultValue(false)]
		public bool ShowImportStatusDialog { get; set; }

		private bool m_bShowRecycleDlg = true;
		[DefaultValue(true)]
		public bool ShowRecycleConfirmDialog
		{
			get { return m_bShowRecycleDlg; }
			set { m_bShowRecycleDlg = value; }
		}

		private bool m_bShowCmdUriDlg = true;
		[DefaultValue(true)]
		public bool ShowCmdUriConfirmDialog
		{
			get { return m_bShowCmdUriDlg; }
			set { m_bShowCmdUriDlg = value; }
		}

		private bool m_bShowCmdPlhDlg = true;
		[DefaultValue(true)]
		public bool ShowCmdPlhConfirmDialog
		{
			get { return m_bShowCmdPlhDlg; }
			set { m_bShowCmdPlhDlg = value; }
		}

		private bool m_bShowRefPPlhDlg = true;
		[DefaultValue(true)]
		public bool ShowRefPPlhConfirmDialog
		{
			get { return m_bShowRefPPlhDlg; }
			set { m_bShowRefPPlhDlg = value; }
		}

		private bool m_bShowDbMntncResDlg = true;
		[DefaultValue(true)]
		public bool ShowDbMntncResultsDialog
		{
			get { return m_bShowDbMntncResDlg; }
			set { m_bShowDbMntncResDlg = value; }
		}

		private bool m_bShowEmSheetDlg = true;
		[DefaultValue(true)]
		public bool ShowEmSheetDialog
		{
			get { return m_bShowEmSheetDlg; }
			set { m_bShowEmSheetDlg = value; }
		}

		private bool m_bShowDbOpenUnkVerDlg = true;
		[DefaultValue(true)]
		public bool ShowDbOpenUnkVerDialog
		{
			get { return m_bShowDbOpenUnkVerDlg; }
			set { m_bShowDbOpenUnkVerDlg = value; }
		}

		// private bool m_bUseCustomTsRenderer = true;
		// [DefaultValue(true)]
		// public bool UseCustomToolStripRenderer
		// {
		//	get { return m_bUseCustomTsRenderer; }
		//	set { m_bUseCustomTsRenderer = value; }
		// }

		private string m_strToolStripRenderer = string.Empty;
		[DefaultValue("")]
		public string ToolStripRenderer
		{
			get { return m_strToolStripRenderer; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strToolStripRenderer = value;
			}
		}

		[DefaultValue(false)]
		public bool TreeViewShowLines { get; set; }

		[DefaultValue(false)]
		public bool OptimizeForScreenReader { get; set; }

		private string m_strDataViewerRect = string.Empty;
		[DefaultValue("")]
		public string DataViewerRect
		{
			get { return m_strDataViewerRect; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strDataViewerRect = value;
			}
		}

		private string m_strDataEditorRect = string.Empty;
		[DefaultValue("")]
		public string DataEditorRect
		{
			get { return m_strDataEditorRect; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strDataEditorRect = value;
			}
		}

		private AceFont m_deFont = new AceFont();
		public AceFont DataEditorFont
		{
			get { return m_deFont; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_deFont = value;
			}
		}

		private bool m_bDeWordWrap = true;
		[DefaultValue(true)]
		public bool DataEditorWordWrap
		{
			get { return m_bDeWordWrap; }
			set { m_bDeWordWrap = value; }
		}

		private string m_strCharPickerRect = string.Empty;
		[DefaultValue("")]
		public string CharPickerRect
		{
			get { return m_strCharPickerRect; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strCharPickerRect = value;
			}
		}

		private string m_strAutoTypeCtxRect = string.Empty;
		[DefaultValue("")]
		public string AutoTypeCtxRect
		{
			get { return m_strAutoTypeCtxRect; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strAutoTypeCtxRect = value;
			}
		}

		private long m_lAutoTypeCtxFlags = (long)AceAutoTypeCtxFlags.Default;
		[DefaultValue((long)AceAutoTypeCtxFlags.Default)]
		public long AutoTypeCtxFlags
		{
			get { return m_lAutoTypeCtxFlags; }
			set { m_lAutoTypeCtxFlags = value; }
		}

		private string m_strAutoTypeCtxColWidths = string.Empty;
		[DefaultValue("")]
		public string AutoTypeCtxColumnWidths
		{
			get { return m_strAutoTypeCtxColWidths; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strAutoTypeCtxColWidths = value;
			}
		}

		private ulong m_uUIFlags = (ulong)AceUIFlags.None;
		public ulong UIFlags
		{
			get { return m_uUIFlags; }
			set { m_uUIFlags = value; }
		}

		private ulong m_uKeyCreationFlags = (ulong)AceKeyUIFlags.None;
		public ulong KeyCreationFlags
		{
			get { return m_uKeyCreationFlags; }
			set { m_uKeyCreationFlags = value; }
		}

		private ulong m_uKeyPromptFlags = (ulong)AceKeyUIFlags.None;
		public ulong KeyPromptFlags
		{
			get { return m_uKeyPromptFlags; }
			set { m_uKeyPromptFlags = value; }
		}

		// private bool m_bEditCancelConfirmation = true;
		// public bool EntryEditCancelConfirmation
		// {
		//	get { return m_bEditCancelConfirmation; }
		//	set { m_bEditCancelConfirmation = value; }
		// }

		[DefaultValue(false)]
		public bool ShowAdvAutoTypeCommands { get; set; }

		private bool m_bSecDeskSound = true;
		[DefaultValue(true)]
		public bool SecureDesktopPlaySound
		{
			get { return m_bSecDeskSound; }
			set { m_bSecDeskSound = value; }
		}
	}

	public sealed class AceHiding
	{
		public AceHiding()
		{
		}

		[DefaultValue(false)]
		public bool RememberHidingPasswordsMain { get; set; }

		[DefaultValue(false)]
		public bool SeparateHidingSettings { get; set; }

		private bool m_bHideInEntryDialog = true;
		[DefaultValue(true)]
		public bool HideInEntryWindow
		{
			get { return m_bHideInEntryDialog; }
			set { m_bHideInEntryDialog = value; }
		}

		[DefaultValue(false)]
		public bool UnhideButtonAlsoUnhidesSource { get; set; }

		[DefaultValue(false)]
		public bool UnhideEmptyData { get; set; }
	}

	public sealed class AceFont
	{
		private Font m_fCached = null;
		private bool m_bCacheValid = false;

		private string m_strFamily = "Microsoft Sans Serif";
		public string Family
		{
			get { return m_strFamily; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strFamily = value;
				m_bCacheValid = false;
			}
		}

		private float m_fSize = 8.25f;
		public float Size
		{
			get { return m_fSize; }
			set { m_fSize = value; m_bCacheValid = false; }
		}

		private GraphicsUnit m_gu = GraphicsUnit.Point;
		public GraphicsUnit GraphicsUnit
		{
			get { return m_gu; }
			set { m_gu = value; m_bCacheValid = false; }
		}

		private FontStyle m_fs = FontStyle.Regular;
		[DefaultValue(FontStyle.Regular)]
		public FontStyle Style
		{
			get { return m_fs; }
			set { m_fs = value; m_bCacheValid = false; }
		}

		private bool m_bOverrideUIDefault = false;
		public bool OverrideUIDefault
		{
			get { return m_bOverrideUIDefault; }
			set { m_bOverrideUIDefault = value; }
		}

		public AceFont()
		{
		}

		public AceFont(Font f) : this(f, false)
		{
		}

		internal AceFont(Font f, bool bOverrideUIDefault)
		{
			if(f == null) throw new ArgumentNullException("f");

			this.Family = f.FontFamily.Name;
			m_fSize = f.Size;
			m_gu = f.Unit;
			m_fs = f.Style;

			m_bOverrideUIDefault = bOverrideUIDefault;
		}

		public AceFont(bool bMonospace)
		{
			if(bMonospace) m_strFamily = "Courier New";
		}

		internal AceFont CloneDeep()
		{
			return (AceFont)MemberwiseClone();
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			if(string.IsNullOrEmpty(m_strFamily))
			{
				sb.Append('(');
				sb.Append(KPRes.None);
				sb.Append(')');
			}
			else sb.Append(m_strFamily);

			sb.Append(", ");
			sb.Append(Math.Round(m_fSize)); // 8.25, 9, 9.75, 11.25, 12, ...
			sb.Append(' ');
			sb.Append(GfxUtil.GraphicsUnitToString(m_gu));

			string strStyle = FontUtil.FontStyleToString(m_fs);
			if(!string.IsNullOrEmpty(strStyle))
			{
				sb.Append(", ");
				sb.Append(strStyle);
			}

			return sb.ToString();
		}

		public Font ToFont()
		{
			if(m_bCacheValid) return m_fCached;

			m_fCached = FontUtil.CreateFont(m_strFamily, m_fSize, m_fs, m_gu);
			m_bCacheValid = true;
			return m_fCached;
		}

		private static void AppendCss(StringBuilder sb, string strIndent,
			string strProperty, params string[] vValueParts)
		{
			sb.Append(strIndent);
			sb.Append(strProperty);
			sb.Append(": ");

			for(int i = 0; i < vValueParts.Length; ++i)
				sb.Append(vValueParts[i]); // Without separator space

			sb.AppendLine(";");
		}

		internal string ToCss(string strIndent, string strFamilyFallback,
			bool bWithDefaults)
		{
			if(strIndent == null) { Debug.Assert(false); strIndent = string.Empty; }

			StringBuilder sb = new StringBuilder();
			NumberFormatInfo nfi = NumberFormatInfo.InvariantInfo;

			string strFamily = string.Empty;
			if(!string.IsNullOrEmpty(m_strFamily))
				strFamily = "\"" + StrUtil.CssEscapeString(m_strFamily) + "\"";
			else { Debug.Assert(false); }
			if(!string.IsNullOrEmpty(strFamilyFallback))
				strFamily += ((strFamily.Length != 0) ? ", " : string.Empty) +
					strFamilyFallback;
			if(strFamily.Length != 0)
				AppendCss(sb, strIndent, "font-family", strFamily);

			AppendCss(sb, strIndent, "font-size", m_fSize.ToString(nfi),
				GfxUtil.GraphicsUnitToString(m_gu));

			if((m_fs & FontStyle.Bold) != FontStyle.Regular)
				AppendCss(sb, strIndent, "font-weight", "bold");
			else if(bWithDefaults)
				AppendCss(sb, strIndent, "font-weight", "normal");

			if((m_fs & FontStyle.Italic) != FontStyle.Regular)
				AppendCss(sb, strIndent, "font-style", "italic");
			else if(bWithDefaults)
				AppendCss(sb, strIndent, "font-style", "normal");

			const FontStyle fsUS = FontStyle.Underline | FontStyle.Strikeout;
			if((m_fs & fsUS) == fsUS)
				AppendCss(sb, strIndent, "text-decoration", "underline line-through");
			else if((m_fs & FontStyle.Underline) != FontStyle.Regular)
				AppendCss(sb, strIndent, "text-decoration", "underline");
			else if((m_fs & FontStyle.Strikeout) != FontStyle.Regular)
				AppendCss(sb, strIndent, "text-decoration", "line-through");
			else if(bWithDefaults)
				AppendCss(sb, strIndent, "text-decoration", "none");

			return sb.ToString();
		}
	}
}
