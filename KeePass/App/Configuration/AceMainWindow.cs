/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2009 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Xml.Serialization;

using KeePassLib;

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
			this.AddColumn(PwDefs.TitleField, 75);
			this.AddColumn(PwDefs.UserNameField, 75);
			this.AddColumn(PwDefs.PasswordField, 75, true);
			this.AddColumn(PwDefs.UrlField, 75);
			this.AddColumn(PwDefs.NotesField, 75);

			this.AddColumn(AppDefs.ColumnIdnCreationTime, 0);
			this.AddColumn(AppDefs.ColumnIdnLastAccessTime, 0);
			this.AddColumn(AppDefs.ColumnIdnLastModificationTime, 0);
			this.AddColumn(AppDefs.ColumnIdnExpiryTime, 0);
			this.AddColumn(AppDefs.ColumnIdnUuid, 0);
			this.AddColumn(AppDefs.ColumnIdnAttachment, 0);
		}

		private void AddColumn(string strID, int nWidth)
		{
			m_aceColumns.Add(strID, new AceColumn(strID, nWidth));
		}

		private void AddColumn(string strID, int nWidth, bool bHide)
		{
			m_aceColumns.Add(strID, new AceColumn(strID, nWidth, bHide));
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

		private int m_nSplitterHorz = AppDefs.InvalidWindowValue;
		public int SplitterHorizontalPosition
		{
			get { return m_nSplitterHorz; }
			set { m_nSplitterHorz = value; }
		}

		private int m_nSplitterVert = AppDefs.InvalidWindowValue;
		public int SplitterVerticalPosition
		{
			get { return m_nSplitterVert; }
			set { m_nSplitterVert = value; }
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

		private bool m_bFullPath = false;
		public bool ShowFullPathInTitle
		{
			get { return m_bFullPath; }
			set { m_bFullPath = value; }
		}

		private bool m_bMinToTray = false;
		public bool MinimizeToTray
		{
			get { return m_bMinToTray; }
			set { m_bMinToTray = value; }
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

		private Dictionary<string, AceColumn> m_aceColumns =
			new Dictionary<string, AceColumn>();
		[XmlIgnore]
		public Dictionary<string, AceColumn> ColumnsDict
		{
			get { return m_aceColumns; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_aceColumns = value;
			}
		}

		[XmlArray("EntryListColumns")]
		public AceColumn[] ColumnsSerializable
		{
			get
			{
				AceColumn[] a = new AceColumn[m_aceColumns.Count];
				int i = 0;
				foreach(KeyValuePair<string, AceColumn> kvp in m_aceColumns)
				{
					a[i] = kvp.Value;
					++i;
				}
				return a;
			}
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				foreach(AceColumn ac in value)
					m_aceColumns[ac.Name] = ac;
			}
		}

		private bool m_bGridLines = false;
		public bool ShowGridLines
		{
			get { return m_bGridLines; }
			set { m_bGridLines = value; }
		}

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

	[XmlType(TypeName = "Column")]
	public sealed class AceColumn
	{
		public AceColumn()
		{
		}

		public AceColumn(string strName, int nWidth)
		{
			this.Name = strName;
			this.Width = nWidth;
		}

		public AceColumn(string strName, int nWidth, bool bHide)
		{
			this.Name = strName;
			this.Width = nWidth;
			this.HideWithAsterisks = bHide;
		}

		private string m_strName = string.Empty;
		public string Name
		{
			get { return m_strName; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strName = value;
			}
		}

		private int m_nWidth = -1;
		public int Width
		{
			get { return m_nWidth; }
			set { m_nWidth = value; }
		}

		private int m_nDisplayIndex = -1;
		public int DisplayIndex
		{
			get { return m_nDisplayIndex; }
			set { m_nDisplayIndex = value; }
		}

		private bool m_bHide = false;
		public bool HideWithAsterisks
		{
			get { return m_bHide; }
			set { m_bHide = value; }
		}

		public int SafeGetWidth(int nDefaultWidth)
		{
			if(m_nWidth >= 0) return m_nWidth;
			return nDefaultWidth;
		}
	}
}
