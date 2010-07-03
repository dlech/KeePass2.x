/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2010 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Diagnostics;

using KeePassLib;

using KeePass.Resources;
using KeePass.UI;

namespace KeePass.App.Configuration
{
	public enum AceMainWindowLayout
	{
		Default = 0,
		SideBySide = 1
	}

	public sealed class AceMainWindow
	{
		public AceMainWindow()
		{
		}

		private int m_posX = AppDefs.InvalidWindowValue;
		public int X
		{
			get { return m_posX; }
			set { m_posX = value; }
		}

		private int m_posY = AppDefs.InvalidWindowValue;
		public int Y
		{
			get { return m_posY; }
			set { m_posY = value; }
		}

		private int m_sizeW = AppDefs.InvalidWindowValue;
		public int Width
		{
			get { return m_sizeW; }
			set { m_sizeW = value; }
		}

		private int m_sizeH = AppDefs.InvalidWindowValue;
		public int Height
		{
			get { return m_sizeH; }
			set { m_sizeH = value; }
		}

		private bool m_bMax = false;
		public bool Maximized
		{
			get { return m_bMax; }
			set { m_bMax = value; }
		}

		private float m_fSplitterHorz = float.Epsilon;
		public float SplitterHorizontalFrac
		{
			get { return m_fSplitterHorz; }
			set { m_fSplitterHorz = value; }
		}

		private float m_fSplitterVert = float.Epsilon;
		public float SplitterVerticalFrac
		{
			get { return m_fSplitterVert; }
			set { m_fSplitterVert = value; }
		}

		private AceMainWindowLayout m_layout = AceMainWindowLayout.Default;
		public AceMainWindowLayout Layout
		{
			get { return m_layout; }
			set { m_layout = value; }
		}

		private bool m_bTop = false;
		public bool AlwaysOnTop
		{
			get { return m_bTop; }
			set { m_bTop = value; }
		}

		private bool m_bCloseMin = false;
		public bool CloseButtonMinimizesWindow
		{
			get { return m_bCloseMin; }
			set { m_bCloseMin = value; }
		}

		private bool m_bMinToTray = false;
		public bool MinimizeToTray
		{
			get { return m_bMinToTray; }
			set { m_bMinToTray = value; }
		}

		private bool m_bFullPath = false;
		public bool ShowFullPathInTitle
		{
			get { return m_bFullPath; }
			set { m_bFullPath = value; }
		}

		private bool m_bDropToBackAfterCopy = false;
		public bool DropToBackAfterClipboardCopy
		{
			get { return m_bDropToBackAfterCopy; }
			set { m_bDropToBackAfterCopy = value; }
		}

		private bool m_bMinAfterCopy = false;
		public bool MinimizeAfterClipboardCopy
		{
			get { return m_bMinAfterCopy; }
			set { m_bMinAfterCopy = value; }
		}

		private bool m_bMinAfterLocking = false;
		public bool MinimizeAfterLocking
		{
			get { return m_bMinAfterLocking; }
			set { m_bMinAfterLocking = value; }
		}

		private bool m_bMinAfterOpeningDb = false;
		public bool MinimizeAfterOpeningDatabase
		{
			get { return m_bMinAfterOpeningDb; }
			set { m_bMinAfterOpeningDb = value; }
		}

		private bool m_bQuickFindSearchInPasswords = false;
		public bool QuickFindSearchInPasswords
		{
			get { return m_bQuickFindSearchInPasswords; }
			set { m_bQuickFindSearchInPasswords = value; }
		}

		private bool m_bQuickFindExcludeExpired = false;
		public bool QuickFindExcludeExpired
		{
			get { return m_bQuickFindExcludeExpired; }
			set { m_bQuickFindExcludeExpired = value; }
		}

		private bool m_bFocusResAfterQuickFind = false;
		public bool FocusResultsAfterQuickFind
		{
			get { return m_bFocusResAfterQuickFind; }
			set { m_bFocusResAfterQuickFind = value; }
		}

		private bool m_bFocusQuickFindOnUntray = false;
		public bool FocusQuickFindOnUntray
		{
			get { return m_bFocusQuickFindOnUntray; }
			set { m_bFocusQuickFindOnUntray = value; }
		}

		private bool m_bCopyUrls = false;
		public bool CopyUrlsInsteadOfOpening
		{
			get { return m_bCopyUrls; }
			set { m_bCopyUrls = value; }
		}

		private bool m_bDisableSaveIfNotModified = false;
		/// <summary>
		/// Disable 'Save' button (instead of graying it out) if the database
		/// hasn't been modified.
		/// </summary>
		public bool DisableSaveIfNotModified
		{
			get { return m_bDisableSaveIfNotModified; }
			set { m_bDisableSaveIfNotModified = value; }
		}

		private AceToolBar m_tb = new AceToolBar();
		public AceToolBar ToolBar
		{
			get { return m_tb; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_tb = value;
			}
		}

		private AceEntryView m_ev = new AceEntryView();
		public AceEntryView EntryView
		{
			get { return m_ev; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_ev = value;
			}
		}

		private AceTanView m_tan = new AceTanView();
		public AceTanView TanView
		{
			get { return m_tan; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_tan = value;
			}
		}

		private List<AceColumn> m_aceColumns = new List<AceColumn>();
		[XmlArray("EntryListColumnCollection")]
		public List<AceColumn> EntryListColumns
		{
			get { return m_aceColumns; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_aceColumns = value;
			}
		}

		private string m_strDisplayIndices = string.Empty;
		public string EntryListColumnDisplayOrder
		{
			get { return m_strDisplayIndices; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strDisplayIndices = value;
			}
		}

		private bool m_bAutoResizeColumns = false;
		public bool EntryListAutoResizeColumns
		{
			get { return m_bAutoResizeColumns; }
			set { m_bAutoResizeColumns = value; }
		}

		private bool m_bAlternatingBgColor = true;
		public bool EntryListAlternatingBgColors
		{
			get { return m_bAlternatingBgColor; }
			set { m_bAlternatingBgColor = value; }
		}

		// private bool m_bGridLines = false;
		// public bool ShowGridLines
		// {
		//	get { return m_bGridLines; }
		//	set { m_bGridLines = value; }
		// }

		private ListSorter m_pListSorter = new ListSorter();
		public ListSorter ListSorting
		{
			get { return m_pListSorter; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_pListSorter = value;
			}
		}

		private bool m_bShowEntriesOfSubGroups = false;
		public bool ShowEntriesOfSubGroups
		{
			get { return m_bShowEntriesOfSubGroups; }
			set { m_bShowEntriesOfSubGroups = value; }
		}

		public AceColumn FindColumn(AceColumnType t)
		{
			foreach(AceColumn c in m_aceColumns)
			{
				if(c.Type == t) return c;
			}

			return null;
		}
	}

	public sealed class AceEntryView
	{
		public AceEntryView()
		{
		}

		private bool m_bShow = true;
		public bool Show
		{
			get { return m_bShow; }
			set { m_bShow = value; }
		}

		private bool m_bHideProtectedCustomStrings = true;
		public bool HideProtectedCustomStrings
		{
			get { return m_bHideProtectedCustomStrings; }
			set { m_bHideProtectedCustomStrings = value; }
		}
	}

	public sealed class AceTanView
	{
		public AceTanView()
		{
		}

		private bool m_bSimple = true;
		public bool UseSimpleView
		{
			get { return m_bSimple; }
			set { m_bSimple = value; }
		}

		private bool m_bIndices = true;
		public bool ShowIndices
		{
			get { return m_bIndices; }
			set { m_bIndices = value; }
		}
	}

	public enum AceColumnType
	{
		Title = 0,
		UserName,
		Password,
		Url,
		Notes,
		CreationTime,
		LastAccessTime,
		LastModificationTime,
		ExpiryTime,
		Uuid,
		Attachment,

		CustomString,

		PluginExt, // Column data provided by a plugin

		OverrideUrl,
		Tags,
		ExpiryTimeDateOnly,
		Size,
		HistoryCount,

		Count // Virtual identifier representing the number of types
	}

	[XmlType(TypeName = "Column")]
	public sealed class AceColumn
	{
		private AceColumnType m_type = AceColumnType.Count;
		public AceColumnType Type
		{
			get { return m_type; }
			set
			{
				if(((int)value >= 0) && ((int)value < (int)AceColumnType.Count))
					m_type = value;
				else { Debug.Assert(false); }
			}
		}

		private string m_strCustomName = string.Empty;
		public string CustomName
		{
			get { return m_strCustomName; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strCustomName = value;
			}
		}

		private int m_nWidth = -1;
		public int Width
		{
			get { return m_nWidth; }
			set { m_nWidth = value; }
		}

		private bool m_bHide = false;
		public bool HideWithAsterisks
		{
			get { return m_bHide; }
			set { m_bHide = value; }
		}

		public AceColumn()
		{
		}

		public AceColumn(AceColumnType t)
		{
			m_type = t;
		}

		public AceColumn(AceColumnType t, string strCustomName, bool bHide,
			int nWidth)
		{
			m_type = t;
			m_strCustomName = strCustomName;
			m_bHide = bHide;
			m_nWidth = nWidth;
		}

		public string GetDisplayName()
		{
			string str = string.Empty;

			switch(m_type)
			{
				case AceColumnType.Title: str = KPRes.Title; break;
				case AceColumnType.UserName: str = KPRes.UserName; break;
				case AceColumnType.Password: str = KPRes.Password; break;
				case AceColumnType.Url: str = KPRes.Url; break;
				case AceColumnType.Notes: str = KPRes.Notes; break;
				case AceColumnType.CreationTime: str = KPRes.CreationTime; break;
				case AceColumnType.LastAccessTime: str = KPRes.LastAccessTime; break;
				case AceColumnType.LastModificationTime: str = KPRes.LastModificationTime; break;
				case AceColumnType.ExpiryTime: str = KPRes.ExpiryTime; break;
				case AceColumnType.Uuid: str = KPRes.Uuid; break;
				case AceColumnType.Attachment: str = KPRes.Attachments; break;
				case AceColumnType.CustomString: str = m_strCustomName; break;
				case AceColumnType.PluginExt: str = m_strCustomName; break;
				case AceColumnType.OverrideUrl: str = KPRes.UrlOverride; break;
				case AceColumnType.Tags: str = KPRes.Tags; break;
				case AceColumnType.ExpiryTimeDateOnly: str = KPRes.ExpiryTimeDateOnly; break;
				case AceColumnType.Size: str = KPRes.Size; break;
				case AceColumnType.HistoryCount: str = KPRes.History + " (" +
					KPRes.Count + ")"; break;
				default: Debug.Assert(false); break;
			};

			return str;
		}

		public int SafeGetWidth(int nDefaultWidth)
		{
			if(m_nWidth >= 0) return m_nWidth;
			return nDefaultWidth;
		}

		public static bool IsTimeColumn(AceColumnType t)
		{
			return ((t == AceColumnType.CreationTime) || (t == AceColumnType.LastAccessTime) ||
				(t == AceColumnType.LastModificationTime) || (t == AceColumnType.ExpiryTime) ||
				(t == AceColumnType.ExpiryTimeDateOnly));
		}

		public static HorizontalAlignment GetTextAlign(AceColumnType t)
		{
			if((t == AceColumnType.Size) || (t == AceColumnType.HistoryCount))
				return HorizontalAlignment.Right;

			return HorizontalAlignment.Left;
		}
	}
}
