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
using System.Text;
using System.Windows.Forms;

using KeePass;

using KeePassLib;
using KeePassLib.Native;
using KeePassLib.Utility;

namespace KeePass.UI
{
	public sealed class NotifyIconEx
	{
		private readonly NotifyIcon m_ntf;

		private Icon m_ico = null; // Property value
		private Icon m_icoShell = null; // Private copy

		public NotifyIcon NotifyIcon { get { return m_ntf; } }

		public ContextMenuStrip ContextMenuStrip
		{
			get
			{
				try { if(m_ntf != null) return m_ntf.ContextMenuStrip; }
				catch(Exception) { Debug.Assert(false); }
				return null;
			}
			set
			{
				try { if(m_ntf != null) m_ntf.ContextMenuStrip = value; }
				catch(Exception) { Debug.Assert(false); }
			}
		}

		public bool Visible
		{
			get
			{
				try { if(m_ntf != null) return m_ntf.Visible; }
				catch(Exception) { Debug.Assert(false); }
				return false;
			}
			set
			{
				try { if(m_ntf != null) m_ntf.Visible = value; }
				catch(Exception) { Debug.Assert(false); }
			}
		}

		public Icon Icon
		{
			get { return m_ico; }
			set
			{
				if(value == m_ico) return; // Avoid small icon recreation

				m_ico = value;
				RefreshShellIcon();
			}
		}

		public string Text
		{
			get
			{
				try { if(m_ntf != null) return m_ntf.Text; }
				catch(Exception) { Debug.Assert(false); }
				return string.Empty;
			}
			set
			{
				try { if(m_ntf != null) m_ntf.Text = value; }
				catch(Exception) { Debug.Assert(false); }
			}
		}

		public NotifyIconEx(IContainer container)
		{
			try
			{
				bool bNtf = true;
				if(NativeLib.GetPlatformID() == PlatformID.MacOSX)
					bNtf = !MonoWorkarounds.IsRequired(1574);
				else
				{
					DesktopType t = NativeLib.GetDesktopType();
					if((t == DesktopType.Unity) || (t == DesktopType.Pantheon))
						bNtf = !MonoWorkarounds.IsRequired(1354);
				}

				if(bNtf) m_ntf = new NotifyIcon(container);
			}
			catch(Exception) { Debug.Assert(false); }
		}

		public void SetHandlers(EventHandler ehClick, EventHandler ehDoubleClick,
			MouseEventHandler ehMouseDown)
		{
			if(m_ntf == null) return;

			try
			{
				if(ehClick != null) m_ntf.Click += ehClick;
				if(ehDoubleClick != null) m_ntf.DoubleClick += ehDoubleClick;
				if(ehMouseDown != null) m_ntf.MouseDown += ehMouseDown;
			}
			catch(Exception) { Debug.Assert(false); }
		}

		internal void RefreshShellIcon()
		{
			if(m_ntf == null) return;

			try
			{
				Icon icoToDispose = m_icoShell;
				try
				{
					if(m_ico != null)
					{
						Size sz = UIUtil.GetSmallIconSize();

						if(Program.Config.UI.TrayIcon.GrayIcon)
						{
							using(Bitmap bmpOrg = UIUtil.IconToBitmap(m_ico,
								sz.Width, sz.Height))
							{
								using(Bitmap bmpGray = UIUtil.CreateGrayImage(
									bmpOrg))
								{
									m_icoShell = UIUtil.BitmapToIcon(bmpGray);
								}
							}
						}
						else m_icoShell = new Icon(m_ico, sz);

						m_ntf.Icon = m_icoShell;
					}
					else m_ntf.Icon = null;
				}
				catch(Exception)
				{
					Debug.Assert(false);
					m_ntf.Icon = m_ico;
				}

				if(icoToDispose != null) icoToDispose.Dispose();
			}
			catch(Exception) { Debug.Assert(false); }
		}
	}
}
