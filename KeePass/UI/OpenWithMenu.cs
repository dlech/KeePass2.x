/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2012 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;

using Microsoft.Win32;

using KeePass.Resources;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Native;
using KeePassLib.Utility;

namespace KeePass.UI
{
	internal sealed class OpenWithItem
	{
		private string m_strPath;
		public string FilePath { get { return m_strPath; } }

		private string m_strMenuText;
		// public string MenuText { get { return m_strMenuText; } }

		private Image m_imgIcon;
		public Image Image { get { return m_imgIcon; } }

		private ToolStripMenuItem m_tsmi;
		public ToolStripMenuItem MenuItem { get { return m_tsmi; } }

		private OpenWithItem(string strFilePath, string strMenuText,
			Image imgIcon, DynamicMenu dynMenu)
		{
			m_strPath = strFilePath;
			m_strMenuText = strMenuText;
			m_imgIcon = imgIcon;

			m_tsmi = dynMenu.AddItem(m_strMenuText, m_imgIcon, this);

			try { m_tsmi.ToolTipText = m_strPath; }
			catch(Exception) { } // Too long?
		}

		public static OpenWithItem Create(string strFilePath, string strMenuText,
			Image imgIcon, DynamicMenu dynMenu)
		{
			return new OpenWithItem(strFilePath, strMenuText, imgIcon, dynMenu);
		}
	}

	public sealed class OpenWithMenu
	{
		private ToolStripMenuItem m_tsmiHost;
		private DynamicMenu m_dynMenu;

		private List<OpenWithItem> m_lOpenWith = null;

		public OpenWithMenu(ToolStripMenuItem tsmiHost)
		{
			if(tsmiHost == null) { Debug.Assert(false); return; }

			m_tsmiHost = tsmiHost;
			m_dynMenu = new DynamicMenu(m_tsmiHost);
			m_dynMenu.MenuClick += this.OnOpenUrl;

			m_tsmiHost.DropDownOpening += this.OnMenuOpening;
		}

		~OpenWithMenu()
		{
			Destroy();
		}

		public void Destroy()
		{
			if(m_dynMenu != null)
			{
				m_dynMenu.Clear();
				m_dynMenu.MenuClick -= this.OnOpenUrl;
				m_dynMenu = null;

				m_tsmiHost.DropDownOpening -= this.OnMenuOpening;
				m_tsmiHost = null;

				// After the menu has been destroyed:
				ReleaseOpenWithList(); // Release icons, ...
			}
		}

		private void OnMenuOpening(object sender, EventArgs e)
		{
			if(m_dynMenu == null) { Debug.Assert(false); return; }

			if(m_lOpenWith == null) CreateOpenWithList();

			PwEntry[] v = Program.MainForm.GetSelectedEntries();
			if(v == null) v = new PwEntry[0];

			bool bCanOpenWith = true;
			uint uValidUrls = 0;
			foreach(PwEntry pe in v)
			{
				string strUrl = pe.Strings.ReadSafe(PwDefs.UrlField);
				if(string.IsNullOrEmpty(strUrl)) continue;

				++uValidUrls;
				bCanOpenWith &= !WinUtil.IsCommandLineUrl(strUrl);
			}
			if((v.Length == 0) || (uValidUrls == 0)) bCanOpenWith = false;

			foreach(OpenWithItem it in m_lOpenWith)
				it.MenuItem.Enabled = bCanOpenWith;
		}

		private void CreateOpenWithList()
		{
			ReleaseOpenWithList();

			m_dynMenu.Clear();
			m_dynMenu.AddSeparator();

			m_lOpenWith = new List<OpenWithItem>();
			FindAppsByKnown();
			FindAppsByRegistry();

			if(m_lOpenWith.Count == 0) m_dynMenu.Clear(); // Remove separator
		}

		private void ReleaseOpenWithList()
		{
			if(m_lOpenWith == null) return;

			foreach(OpenWithItem it in m_lOpenWith)
			{
				if(it.Image != null) it.Image.Dispose();
			}

			m_lOpenWith = null;
		}

		private void OnOpenUrl(object sender, DynamicMenuEventArgs e)
		{
			if(e == null) { Debug.Assert(false); return; }

			OpenWithItem it = (e.Tag as OpenWithItem);
			if(it == null) { Debug.Assert(false); return; }

			PwEntry[] v = Program.MainForm.GetSelectedEntries();
			if(v == null) { Debug.Assert(false); return; }

			foreach(PwEntry pe in v)
			{
				string strUrl = pe.Strings.ReadSafe(PwDefs.UrlField);
				if(string.IsNullOrEmpty(strUrl)) continue;

				WinUtil.OpenUrlWithApp(strUrl, pe, it.FilePath);
			}
		}

		private void AddAppByFile(string strAppCmdLine, string strName)
		{
			if(string.IsNullOrEmpty(strAppCmdLine)) return; // No assert

			string strPath = UrlUtil.GetShortestAbsolutePath(
				UrlUtil.GetQuotedAppPath(strAppCmdLine).Trim());
			if(strPath.Length == 0) { Debug.Assert(false); return; }

			foreach(OpenWithItem it in m_lOpenWith)
			{
				if(it.FilePath.Equals(strPath, StrUtil.CaseIgnoreCmp))
					return; // Already have an item for this
			}

			// Filter non-existing/legacy applications
			try { if(!File.Exists(strPath)) return; }
			catch(Exception) { Debug.Assert(false); return; }

			if(string.IsNullOrEmpty(strName))
				strName = UrlUtil.StripExtension(UrlUtil.GetFileName(strPath));

			Image img = null;
			try
			{
				Icon ico = Icon.ExtractAssociatedIcon(strPath);
				if(ico == null) throw new InvalidOperationException();

				img = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
				using(Graphics g = Graphics.FromImage(img))
				{
					g.Clear(Color.Transparent);
					g.InterpolationMode = InterpolationMode.HighQualityBicubic;
					g.SmoothingMode = SmoothingMode.HighQuality;
					g.DrawIcon(ico, new Rectangle(0, 0, img.Width, img.Height));					
				}

				ico.Dispose();
			}
			catch(Exception) { Debug.Assert(false); }

			string strMenuText = KPRes.OpenWith.Replace(@"{PARAM}", strName);
			OpenWithItem owi = OpenWithItem.Create(strPath, strMenuText,
				img, m_dynMenu);
			m_lOpenWith.Add(owi);
		}

		private void FindAppsByKnown()
		{
			AddAppByFile(AppLocator.InternetExplorerPath, @"&Internet Explorer");
			AddAppByFile(AppLocator.FirefoxPath, @"&Firefox");
			AddAppByFile(AppLocator.OperaPath, @"O&pera");
			AddAppByFile(AppLocator.ChromePath, @"&Google Chrome");
			AddAppByFile(AppLocator.SafariPath, @"&Safari");

			if(NativeLib.IsUnix())
			{
				AddAppByFile(AppLocator.FindAppUnix("epiphany-browser"), @"&Epiphany");
				AddAppByFile(AppLocator.FindAppUnix("galeon"), @"Ga&leon");
				AddAppByFile(AppLocator.FindAppUnix("konqueror"), @"&Konqueror");
				AddAppByFile(AppLocator.FindAppUnix("rekonq"), @"&Rekonq");
				AddAppByFile(AppLocator.FindAppUnix("arora"), @"&Arora");
				AddAppByFile(AppLocator.FindAppUnix("midori"), @"&Midori");
				AddAppByFile(AppLocator.FindAppUnix("Dooble"), @"&Dooble"); // Upper-case
			}
		}

		private void FindAppsByRegistry()
		{
			try { FindAppsByRegistryPriv("SOFTWARE\\Clients\\StartMenuInternet"); }
			catch(Exception) { }
			try { FindAppsByRegistryPriv("SOFTWARE\\Wow6432Node\\Clients\\StartMenuInternet"); }
			catch(Exception) { }
		}

		private void FindAppsByRegistryPriv(string strRootSubKey)
		{
			RegistryKey kRoot = Registry.LocalMachine.OpenSubKey(strRootSubKey, false);
			if(kRoot == null) return; // No assert, key might not exist
			string[] vAppSubKeys = kRoot.GetSubKeyNames();

			foreach(string strAppSubKey in vAppSubKeys)
			{
				RegistryKey kApp = kRoot.OpenSubKey(strAppSubKey, false);
				string strName = (kApp.GetValue(string.Empty) as string);
				string strAltName = null;

				RegistryKey kCmd = kApp.OpenSubKey("shell\\open\\command", false);
				if(kCmd == null) { Debug.Assert(false); kApp.Close(); continue; }
				string strCmdLine = (kCmd.GetValue(string.Empty) as string);
				kCmd.Close();

				RegistryKey kCaps = kApp.OpenSubKey("Capabilities", false);
				if(kCaps != null)
				{
					strAltName = (kCaps.GetValue("ApplicationName") as string);
					kCaps.Close();
				}

				kApp.Close();

				string strDisplayName = string.Empty;
				if(strName != null) strDisplayName = strName;
				if((strAltName != null) && (strAltName.Length <= strDisplayName.Length))
					strDisplayName = strAltName;

				AddAppByFile(strCmdLine, strDisplayName);
			}

			kRoot.Close();
		}
	}
}
