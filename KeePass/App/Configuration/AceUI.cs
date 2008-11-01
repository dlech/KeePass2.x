/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2008 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePass.UI;

namespace KeePass.App.Configuration
{
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

		private BannerStyle m_bannerStyle = BannerStyle.WinVistaBlack;
		public BannerStyle BannerStyle
		{
			get { return m_bannerStyle; }
			set { m_bannerStyle = value; }
		}

		private bool m_bOptScreenReader = false;
		public bool OptimizeForScreenReader
		{
			get { return m_bOptScreenReader; }
			set { m_bOptScreenReader = value; }
		}
	}

	public sealed class AceHiding
	{
		public AceHiding()
		{
		}

		private bool m_bSepHiding = true;
		public bool SeparateHidingSettings
		{
			get { return m_bSepHiding; }
			set { m_bSepHiding = value; }
		}

		private bool m_bHideInEntryDialog = true;
		public bool HideInEntryWindow
		{
			get { return m_bHideInEntryDialog; }
			set { m_bHideInEntryDialog = value; }
		}
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

		private float m_fSize = 8.0f;
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

		private FontStyle m_fStyle = FontStyle.Regular;
		public FontStyle Style
		{
			get { return m_fStyle; }
			set { m_fStyle = value; m_bCacheValid = false; }
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

		public AceFont(Font f)
		{
			this.Family = f.FontFamily.Name;
			this.Size = f.Size;
			this.Style = f.Style;
			this.GraphicsUnit = f.Unit;
		}

		public Font ToFont()
		{
			if(m_bCacheValid) return m_fCached;

			m_fCached = new Font(m_strFamily, m_fSize, m_fStyle, m_gu);
			m_bCacheValid = true;
			return m_fCached;
		}
	}
}
