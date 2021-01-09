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
using System.IO;
using System.Text;
using System.Windows.Forms;

using Microsoft.Win32;

using KeePass.Resources;
using KeePass.Util;
using KeePass.Util.Spr;

using KeePassLib;
using KeePassLib.Cryptography.PasswordGenerator;
using KeePassLib.Native;
using KeePassLib.Utility;

namespace KeePass.UI
{
	internal enum OwFilePathType
	{
		/// <summary>
		/// Path to an executable file that is invoked with
		/// the target URI as command line parameter.
		/// </summary>
		Executable = 0,

		/// <summary>
		/// Shell path (e.g. URI) in which a placeholder is
		/// replaced by the target URI.
		/// </summary>
		ShellExpand
	}

	internal sealed class OpenWithItem
	{
		private readonly string m_strPath;
		public string FilePath { get { return m_strPath; } }

		private readonly OwFilePathType m_tPath;
		public OwFilePathType FilePathType { get { return m_tPath; } }

		private readonly string m_strName;
		public string Name { get { return m_strName; } }

		private readonly Image m_imgIcon;
		public Image Image { get { return m_imgIcon; } }

		private readonly DynamicMenu m_dynMenu;

		private ToolStripMenuItem m_tsmi = null;
		public ToolStripMenuItem MenuItem { get { return m_tsmi; } }

		public OpenWithItem(string strFilePath, OwFilePathType tPath,
			string strName, Image imgIcon, DynamicMenu dynMenu)
		{
			if(strFilePath == null) { Debug.Assert(false); throw new ArgumentNullException("strFilePath"); }
			if(strName == null) { Debug.Assert(false); throw new ArgumentNullException("strName"); }
			if(dynMenu == null) { Debug.Assert(false); throw new ArgumentNullException("dynMenu"); }

			m_strPath = strFilePath;
			m_tPath = tPath;
			m_strName = strName.Trim();
			m_imgIcon = imgIcon;
			m_dynMenu = dynMenu;

			if(m_strName.Length == 0)
			{
				Debug.Assert(false);
				m_strName = m_strPath;
				if(m_strName.Length == 0) m_strName = KPRes.Unknown;
			}
		}

		public void AddMenuItem(List<char> lAvailKeys)
		{
			if(m_tsmi != null) { Debug.Assert(false); return; }

			string strNameAccel = StrUtil.AddAccelerator(
				StrUtil.EncodeMenuText(m_strName), lAvailKeys);

			Debug.Assert(StrUtil.EncodeMenuText(KPRes.OpenWith) == KPRes.OpenWith);
			string strText = KPRes.OpenWith.Replace(@"{PARAM}", strNameAccel);

			m_tsmi = m_dynMenu.AddItem(strText, m_imgIcon, this);

			try
			{
				string strTip = m_strPath;
				if(strTip.StartsWith("cmd://", StrUtil.CaseIgnoreCmp))
					strTip = strTip.Substring(6);

				if(strTip.Length != 0) m_tsmi.ToolTipText = strTip;
			}
			catch(Exception) { Debug.Assert(false); } // Too long?
		}

		public static int CompareByName(OpenWithItem itA, OpenWithItem itB)
		{
			if(itA == null) { Debug.Assert(false); return ((itB == null) ? 0 : -1); }
			if(itB == null) { Debug.Assert(false); return 1; }

			return string.Compare(itA.Name, itB.Name, StrUtil.CaseIgnoreCmp);
		}
	}

	public sealed class OpenWithMenu
	{
		private ToolStripDropDownItem m_tsmiHost;
		private DynamicMenu m_dynMenu;

		private List<OpenWithItem> m_lOpenWith = null;

		private const string PlhTargetUri = @"{OW_URI}";

		public OpenWithMenu(ToolStripDropDownItem tsmiHost)
		{
			if(tsmiHost == null) { Debug.Assert(false); return; }

			m_tsmiHost = tsmiHost;
			m_dynMenu = new DynamicMenu(m_tsmiHost);
			m_dynMenu.MenuClick += this.OnOpenUrl;

			m_tsmiHost.DropDownOpening += this.OnMenuOpening;
		}

		~OpenWithMenu()
		{
			try { Debug.Assert(m_dynMenu == null); Destroy(); }
			catch(Exception) { Debug.Assert(false); }
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
			FinishOpenWithList();

			if(m_lOpenWith.Count == 0) m_dynMenu.Clear(); // Remove separator
			else
			{
				List<char> lAvailKeys = new List<char>(PwCharSet.MenuAccels);

				// Remove keys that are used by non-dynamic menu items
				foreach(ToolStripItem tsi in m_tsmiHost.DropDownItems)
				{
					string str = ((tsi != null) ? tsi.Text : null);
					if(!string.IsNullOrEmpty(str))
					{
						str = str.Replace(@"&&", string.Empty);
						int i = str.IndexOf('&');
						Debug.Assert(i == str.LastIndexOf('&'));
						if((i >= 0) && (i < (str.Length - 1)))
						{
							lAvailKeys.Remove(char.ToLowerInvariant(str[i + 1]));
							lAvailKeys.Remove(char.ToUpperInvariant(str[i + 1]));
						}
					}
				}

				foreach(OpenWithItem it in m_lOpenWith)
					it.AddMenuItem(lAvailKeys);
			}
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

			string strApp = it.FilePath;

			PwEntry[] v = Program.MainForm.GetSelectedEntries();
			if(v == null) { Debug.Assert(false); return; }

			foreach(PwEntry pe in v)
			{
				// Get the entry's URL, avoid URL override
				string strUrl = pe.Strings.ReadSafe(PwDefs.UrlField);
				if(string.IsNullOrEmpty(strUrl)) continue;

				if(it.FilePathType == OwFilePathType.Executable)
					WinUtil.OpenUrlWithApp(strUrl, pe, strApp);
				else if(it.FilePathType == OwFilePathType.ShellExpand)
				{
					string str = strApp.Replace(PlhTargetUri,
						SprEncoding.EncodeForCommandLine(strUrl));
					WinUtil.OpenUrl(str, pe, false);
				}
				else { Debug.Assert(false); }
			}
		}

		private bool AddAppByFile(string strAppCmdLine, string strName)
		{
			if(string.IsNullOrEmpty(strAppCmdLine)) return false; // No assert

			string strPath = UrlUtil.GetShortestAbsolutePath(
				UrlUtil.GetQuotedAppPath(strAppCmdLine).Trim());
			if(strPath.Length == 0) { Debug.Assert(false); return false; }

			foreach(OpenWithItem it in m_lOpenWith)
			{
				if(it.FilePath.Equals(strPath, StrUtil.CaseIgnoreCmp))
					return false; // Already have an item for this
			}

			// Filter non-existing/legacy applications
			try { if(!File.Exists(strPath)) return false; }
			catch(Exception) { Debug.Assert(false); return false; }

			if(string.IsNullOrEmpty(strName))
				strName = UrlUtil.StripExtension(UrlUtil.GetFileName(strPath));

			// Image img = UIUtil.GetFileIcon(strPath, DpiUtil.ScaleIntX(16),
			//	DpiUtil.ScaleIntY(16));
			Image img = FileIcons.GetImageForPath(strPath, null, true, true);

			OpenWithItem owi = new OpenWithItem(strPath, OwFilePathType.Executable,
				strName, img, m_dynMenu);
			m_lOpenWith.Add(owi);
			return true;
		}

		private void AddAppByShellExpand(string strShell, string strName,
			string strIconExe)
		{
			if(string.IsNullOrEmpty(strShell)) return;

			if(string.IsNullOrEmpty(strName)) strName = strShell;

			Image img = null;
			if(!string.IsNullOrEmpty(strIconExe))
			{
				// img = UIUtil.GetFileIcon(strIconExe, DpiUtil.ScaleIntX(16),
				//	DpiUtil.ScaleIntY(16));
				img = FileIcons.GetImageForPath(strIconExe, null, true, true);
			}

			OpenWithItem owi = new OpenWithItem(strShell, OwFilePathType.ShellExpand,
				strName, img, m_dynMenu);
			m_lOpenWith.Add(owi);
		}

		private void FindAppsByKnown()
		{
			string strIE = AppLocator.InternetExplorerPath;
			if(AddAppByFile(strIE, "Internet Explorer"))
			{
				// https://msdn.microsoft.com/en-us/library/hh826025.aspx
				AddAppByShellExpand("cmd://\"" + SprEncoding.EncodeForCommandLine(
					strIE) + "\" -private \"" + PlhTargetUri + "\"",
					"Internet Explorer (" + KPRes.Private + ")", strIE);
			}

			string strFF = AppLocator.FirefoxPath;
			if(AddAppByFile(strFF, "Firefox"))
			{
				// The command line options -private and -private-window work
				// correctly with Firefox 49.0.1 (before, they did not);
				// https://developer.mozilla.org/en-US/docs/Mozilla/Command_Line_Options
				// https://bugzilla.mozilla.org/show_bug.cgi?id=856839
				// https://bugzilla.mozilla.org/show_bug.cgi?id=829180
				AddAppByShellExpand("cmd://\"" + SprEncoding.EncodeForCommandLine(
					strFF) + "\" -private-window \"" + PlhTargetUri + "\"",
					"Firefox (" + KPRes.Private + ")", strFF);
			}

			string strCh = AppLocator.ChromePath;
			if(AddAppByFile(strCh, "Google Chrome"))
			{
				// https://www.chromium.org/developers/how-tos/run-chromium-with-flags
				// https://peter.sh/experiments/chromium-command-line-switches/
				AddAppByShellExpand("cmd://\"" + SprEncoding.EncodeForCommandLine(
					strCh) + "\" --incognito \"" + PlhTargetUri + "\"",
					"Google Chrome (" + KPRes.Private + ")", strCh);
			}

			string strOp = AppLocator.OperaPath;
			if(AddAppByFile(strOp, "Opera"))
			{
				// Doesn't work with Opera 34.0.2036.25:
				// AddAppByShellExpand("cmd://\"" + SprEncoding.EncodeForCommandLine(
				//	strOp) + "\" -newprivatetab \"" + PlhTargetUri + "\"",
				//	"Opera (" + KPRes.Private + ")", strOp);

				// Doesn't work with Opera 36.0.2130.65:
				// AddAppByShellExpand("cmd://\"" + SprEncoding.EncodeForCommandLine(
				//	strOp) + "\" --incognito \"" + PlhTargetUri + "\"",
				//	"Opera (" + KPRes.Private + ")", strOp);

				// Works with Opera 40.0.2308.81:
				AddAppByShellExpand("cmd://\"" + SprEncoding.EncodeForCommandLine(
					strOp) + "\" --private \"" + PlhTargetUri + "\"",
					"Opera (" + KPRes.Private + ")", strOp);
			}

			AddAppByFile(AppLocator.SafariPath, "Safari");

			if(NativeLib.IsUnix())
			{
				AddAppByFile(AppLocator.FindAppUnix("epiphany-browser"), "Epiphany");
				AddAppByFile(AppLocator.FindAppUnix("galeon"), "Galeon");
				AddAppByFile(AppLocator.FindAppUnix("konqueror"), "Konqueror");
				AddAppByFile(AppLocator.FindAppUnix("rekonq"), "Rekonq");
				AddAppByFile(AppLocator.FindAppUnix("arora"), "Arora");
				AddAppByFile(AppLocator.FindAppUnix("midori"), "Midori");
				AddAppByFile(AppLocator.FindAppUnix("Dooble"), "Dooble"); // Upper-case
			}
		}

		private void FindAppsByRegistry()
		{
			const string strSmiDef = "SOFTWARE\\Clients\\StartMenuInternet";
			const string strSmiWow = "SOFTWARE\\Wow6432Node\\Clients\\StartMenuInternet";

			// https://msdn.microsoft.com/en-us/library/windows/desktop/dd203067.aspx
			try { FindAppsByRegistryPriv(Registry.CurrentUser, strSmiDef); }
			catch(Exception) { Debug.Assert(NativeLib.IsUnix()); }
			try { FindAppsByRegistryPriv(Registry.CurrentUser, strSmiWow); }
			catch(Exception) { Debug.Assert(NativeLib.IsUnix()); }
			try { FindAppsByRegistryPriv(Registry.LocalMachine, strSmiDef); }
			catch(Exception) { Debug.Assert(NativeLib.IsUnix()); }
			try { FindAppsByRegistryPriv(Registry.LocalMachine, strSmiWow); }
			catch(Exception) { Debug.Assert(NativeLib.IsUnix()); }
		}

		private void FindAppsByRegistryPriv(RegistryKey kBase, string strRootSubKey)
		{
			RegistryKey kRoot = kBase.OpenSubKey(strRootSubKey, false);
			if(kRoot == null) return; // No assert, key might not exist
			string[] vAppSubKeys = kRoot.GetSubKeyNames();

			foreach(string strAppSubKey in vAppSubKeys)
			{
				RegistryKey kApp = kRoot.OpenSubKey(strAppSubKey, false);
				string strName = (kApp.GetValue(string.Empty) as string);
				string strAltName = null;

				RegistryKey kCmd = kApp.OpenSubKey("shell\\open\\command", false);
				if(kCmd == null) { kApp.Close(); continue; } // No assert (XP)
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

		private void FinishOpenWithList()
		{
			OpenWithItem itEdge = null; // New (Chromium-based) Edge
			OpenWithItem itVivaldi = null;
			foreach(OpenWithItem it in m_lOpenWith)
			{
				if(it.FilePathType != OwFilePathType.Executable) continue;

				string strFile = it.FilePath;
				if((strFile.IndexOf("\\Microsoft", StrUtil.CaseIgnoreCmp) >= 0) &&
					strFile.EndsWith("\\msedge.exe", StrUtil.CaseIgnoreCmp))
				{
					if((itEdge == null) || it.Name.Equals("Microsoft Edge", StrUtil.CaseIgnoreCmp))
						itEdge = it;
					else { Debug.Assert(false); } // Duplicate?
				}
				else if(strFile.EndsWith("\\vivaldi.exe", StrUtil.CaseIgnoreCmp))
				{
					if((itVivaldi == null) || it.Name.Equals("Vivaldi", StrUtil.CaseIgnoreCmp))
						itVivaldi = it;
					else { Debug.Assert(false); } // Duplicate?
				}
			}

			if(itEdge != null)
			{
				// The legacy Edge (EdgeHTML) doesn't register itself in the
				// 'StartMenuInternet' registry key, whereas the new one
				// (Chromium) does; so, the one that we found must be the
				// new Edge, which supports a command line option for the
				// private mode
				AddAppByShellExpand("cmd://\"" + SprEncoding.EncodeForCommandLine(
					itEdge.FilePath) + "\" --inprivate \"" + PlhTargetUri + "\"",
					itEdge.Name + " (" + KPRes.Private + ")", itEdge.FilePath);
			}
			else // Add the legacy Edge (EdgeHTML), if available
			{
				if(AppLocator.EdgeProtocolSupported)
					AddAppByShellExpand("microsoft-edge:" + PlhTargetUri,
						"Microsoft Edge", AppLocator.EdgePath);
			}

			if(itVivaldi != null)
				AddAppByShellExpand("cmd://\"" + SprEncoding.EncodeForCommandLine(
					itVivaldi.FilePath) + "\" --incognito \"" + PlhTargetUri + "\"",
					itVivaldi.Name + " (" + KPRes.Private + ")", itVivaldi.FilePath);

			m_lOpenWith.Sort(OpenWithItem.CompareByName);
		}
	}
}
