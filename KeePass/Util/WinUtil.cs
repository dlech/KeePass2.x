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
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;

using KeePass.App;
using KeePass.Native;
using KeePass.Resources;
using KeePass.Util.Spr;

using KeePassLib;
using KeePassLib.Serialization;
using KeePassLib.Utility;

using NativeLib = KeePassLib.Native.NativeLib;

namespace KeePass.Util
{
	public static class WinUtil
	{
		private const int ERROR_ACCESS_DENIED = 5;

		private static bool m_bIsWindows9x = false;
		private static bool m_bIsWindows2000 = false;
		private static bool m_bIsWindowsXP = false;
		private static bool m_bIsAtLeastWindows2000 = false;
		private static bool m_bIsAtLeastWindowsVista = false;
		private static bool m_bIsAtLeastWindows7 = false;

		private static string m_strExePath = null;

		private static ulong m_uFrameworkVersion = 0;

		public static bool IsWindows9x
		{
			get { return m_bIsWindows9x; }
		}

		public static bool IsWindows2000
		{
			get { return m_bIsWindows2000; }
		}

		public static bool IsWindowsXP
		{
			get { return m_bIsWindowsXP; }
		}

		public static bool IsAtLeastWindows2000
		{
			get { return m_bIsAtLeastWindows2000; }
		}

		public static bool IsAtLeastWindowsVista
		{
			get { return m_bIsAtLeastWindowsVista; }
		}

		public static bool IsAtLeastWindows7
		{
			get { return m_bIsAtLeastWindows7; }
		}

		static WinUtil()
		{
			OperatingSystem os = Environment.OSVersion;
			Version v = os.Version;

			m_bIsWindows9x = (os.Platform == PlatformID.Win32Windows);
			m_bIsWindows2000 = ((v.Major == 5) && (v.Minor == 0));
			m_bIsWindowsXP = ((v.Major == 5) && (v.Minor == 1));

			m_bIsAtLeastWindows2000 = (v.Major >= 5);
			m_bIsAtLeastWindowsVista = (v.Major >= 6);
			m_bIsAtLeastWindows7 = ((v.Major >= 7) || ((v.Major == 6) && (v.Minor >= 1)));
		}

		public static void OpenEntryUrl(PwEntry pe)
		{
			Debug.Assert(pe != null);
			if(pe == null) throw new ArgumentNullException("pe");

			if(pe.OverrideUrl.Length > 0)
				WinUtil.OpenUrl(pe.OverrideUrl, pe);
			else
			{
				string strOverride = Program.Config.Integration.UrlOverride;
				if(strOverride.Length > 0)
					WinUtil.OpenUrl(strOverride, pe);
				else
					WinUtil.OpenUrl(pe.Strings.ReadSafe(PwDefs.UrlField), pe);
			}
		}

		public static void OpenUrl(string strUrlToOpen, PwEntry peDataSource)
		{
			// If URL is null, return false, do not throw exception.
			Debug.Assert(strUrlToOpen != null); if(strUrlToOpen == null) return;

			string strPrevWorkDir = Directory.GetCurrentDirectory();
			string strThisExe = WinUtil.GetExecutable();
			
			string strExeDir = UrlUtil.GetFileDirectory(strThisExe, false, true);
			try { Directory.SetCurrentDirectory(strExeDir); }
			catch(Exception) { Debug.Assert(false); }

			string strUrlFlt = strUrlToOpen;
			strUrlFlt = strUrlFlt.TrimStart(new char[]{ ' ', '\t', '\r', '\n' });

			PwDatabase pwDatabase = null;
			try { pwDatabase = Program.MainForm.PluginHost.Database; }
			catch(Exception) { Debug.Assert(false); pwDatabase = null; }

			bool bCmdQuotes = WinUtil.IsCommandLineUrl(strUrlFlt);

			string strUrl = SprEngine.Compile(strUrlFlt, false, peDataSource,
				pwDatabase, false, bCmdQuotes);

			string strOvr = Program.Config.Integration.UrlSchemeOverrides.GetOverrideForUrl(
				strUrl);
			if(strOvr != null)
			{
				bCmdQuotes = WinUtil.IsCommandLineUrl(strOvr);
				strUrl = SprEngine.Compile(strOvr, false, peDataSource, pwDatabase,
					false, bCmdQuotes);
			}

			if(WinUtil.IsCommandLineUrl(strUrl))
			{
				string strApp, strArgs;
				StrUtil.SplitCommandLine(WinUtil.GetCommandLineFromUrl(strUrl),
					out strApp, out strArgs);

				try
				{
					if((strArgs != null) && (strArgs.Length > 0))
						Process.Start(strApp, strArgs);
					else
						Process.Start(strApp);
				}
				catch(Win32Exception)
				{
					StartWithoutShellExecute(strApp, strArgs);
				}
				catch(Exception exCmd)
				{
					string strInf = KPRes.FileOrUrl + ": " + strApp;
					if((strArgs != null) && (strArgs.Length > 0))
						strInf += MessageService.NewParagraph +
							KPRes.Arguments + ": " + strArgs;

					MessageService.ShowWarning(strInf, exCmd);
				}
			}
			else // Standard URL
			{
				try { Process.Start(strUrl); }
				catch(Exception exUrl)
				{
					MessageService.ShowWarning(strUrl, exUrl);
				}
			}

			// Restore previous working directory
			try { Directory.SetCurrentDirectory(strPrevWorkDir); }
			catch(Exception) { Debug.Assert(false); }

			// SprEngine.Compile might have modified the database
			Program.MainForm.UpdateUI(false, null, false, null, false, null, false);
		}

		private static void StartWithoutShellExecute(string strApp, string strArgs)
		{
			try
			{
				ProcessStartInfo psi = new ProcessStartInfo();

				psi.FileName = strApp;

				if((strArgs != null) && (strArgs.Length > 0))
					psi.Arguments = strArgs;

				psi.UseShellExecute = false;

				Process.Start(psi);
			}
			catch(Exception exCmd)
			{
				string strInf = KPRes.FileOrUrl + ": " + strApp;
				if((strArgs != null) && (strArgs.Length > 0))
					strInf += MessageService.NewParagraph +
						KPRes.Arguments + ": " + strArgs;

				MessageService.ShowWarning(strInf, exCmd);
			}
		}

		public static void Restart()
		{
			try { Process.Start(WinUtil.GetExecutable()); }
			catch(Exception exRestart)
			{
				MessageService.ShowWarning(exRestart);
			}
		}

		public static string GetExecutable()
		{
			if(m_strExePath != null) return m_strExePath;

			try { m_strExePath = Assembly.GetExecutingAssembly().Location; }
			catch(Exception) { }

			if((m_strExePath == null) || (m_strExePath.Length == 0))
			{
				m_strExePath = Assembly.GetExecutingAssembly().GetName().CodeBase;
				m_strExePath = UrlUtil.FileUrlToPath(m_strExePath);
			}

			return m_strExePath;
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

			return new Font(vParts[0], fSize, fs, GraphicsUnit.Point);
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

		/// <summary>
		/// Shorten a path.
		/// </summary>
		/// <param name="strPath">Path to make shorter.</param>
		/// <param name="nMaxChars">Maximum number of characters in the returned string.</param>
		/// <returns>Shortened path.</returns>
		public static string CompactPath(string strPath, int nMaxChars)
		{
			Debug.Assert(strPath != null);
			if(strPath == null) throw new ArgumentNullException("strPath");
			Debug.Assert(nMaxChars >= 0);
			if(nMaxChars < 0) throw new ArgumentOutOfRangeException("nMaxChars");

			if(nMaxChars == 0) return string.Empty;
			if(strPath.Length <= nMaxChars) return strPath;

			try
			{
				StringBuilder sb = new StringBuilder(strPath.Length * 2 + 4);

				if(NativeMethods.PathCompactPathEx(sb, strPath, (uint)nMaxChars, 0) == false)
					return StrUtil.CompactString3Dots(strPath, nMaxChars);

				Debug.Assert(sb.Length <= nMaxChars);
				if((sb.Length <= nMaxChars) && (sb.Length != 0))
					return sb.ToString();
				else
					return StrUtil.CompactString3Dots(strPath, nMaxChars);
			}
			catch(Exception) { Debug.Assert(false); }

			return StrUtil.CompactString3Dots(strPath, nMaxChars);
		}

		public static bool FlushStorageBuffers(char chDriveLetter, bool bOnlyIfRemovable)
		{
			string strDriveLetter = new string(chDriveLetter, 1);
			bool bResult = true;

			try
			{
				if(bOnlyIfRemovable)
				{
					DriveInfo di = new DriveInfo(strDriveLetter);
					if(di.DriveType != DriveType.Removable) return true;
				}

				string strDevice = "\\\\.\\" + strDriveLetter + ":";

				IntPtr hDevice = NativeMethods.CreateFile(strDevice,
					NativeMethods.EFileAccess.GenericRead | NativeMethods.EFileAccess.GenericWrite,
					NativeMethods.EFileShare.Read | NativeMethods.EFileShare.Write,
					IntPtr.Zero, NativeMethods.ECreationDisposition.OpenExisting,
					0, IntPtr.Zero);
				if(NativeMethods.IsInvalidHandleValue(hDevice))
				{
					Debug.Assert(false);
					return false;
				}

				string strDir = FreeDriveIfCurrent(chDriveLetter);

				uint dwDummy;
				if(NativeMethods.DeviceIoControl(hDevice, NativeMethods.FSCTL_LOCK_VOLUME,
					IntPtr.Zero, 0, IntPtr.Zero, 0, out dwDummy, IntPtr.Zero) != false)
				{
					if(NativeMethods.DeviceIoControl(hDevice, NativeMethods.FSCTL_UNLOCK_VOLUME,
						IntPtr.Zero, 0, IntPtr.Zero, 0, out dwDummy, IntPtr.Zero) == false)
					{
						Debug.Assert(false);
					}
				}
				else bResult = false;

				if(strDir.Length > 0) Directory.SetCurrentDirectory(strDir);

				if(!NativeMethods.CloseHandle(hDevice)) { Debug.Assert(false); }
			}
			catch(Exception)
			{
				Debug.Assert(false);
				return false;
			}

			return bResult;
		}

		public static bool FlushStorageBuffers(string strFileOnStorage, bool bOnlyIfRemovable)
		{
			if(strFileOnStorage == null) { Debug.Assert(false); return false; }
			if(strFileOnStorage.Length < 3) return false;
			if(strFileOnStorage[1] != ':') return false;
			if(strFileOnStorage[2] != '\\') return false;

			return FlushStorageBuffers(char.ToUpper(strFileOnStorage[0]), bOnlyIfRemovable);
		}

		private static string FreeDriveIfCurrent(char chDriveLetter)
		{
			try
			{
				string strCur = Directory.GetCurrentDirectory();
				if((strCur == null) || (strCur.Length < 3)) return string.Empty;
				if(strCur[1] != ':') return string.Empty;
				if(strCur[2] != '\\') return string.Empty;

				char chPar = char.ToUpper(chDriveLetter);
				char chCur = char.ToUpper(strCur[0]);
				if(chPar != chCur) return string.Empty;

				string strTemp = Path.GetTempPath();
				Directory.SetCurrentDirectory(strTemp);

				return strCur;
			}
			catch(Exception) { Debug.Assert(false); }

			return string.Empty;
		}

		private static readonly string[] m_vIE7Windows = new string[] {
			"Windows Internet Explorer", "Maxthon"
		};

		public static bool IsInternetExplorer7Window(string strWindowTitle)
		{
			if(strWindowTitle == null) return false; // No assert or throw
			if(strWindowTitle.Length == 0) return false; // No assert or throw

			foreach(string str in m_vIE7Windows)
			{
				if(strWindowTitle.IndexOf(str) >= 0) return true;
			}

			return false;
		}

		public static byte[] HashFile(IOConnectionInfo iocFile)
		{
			if(iocFile == null) { Debug.Assert(false); return null; } // Assert only

			Stream sIn;
			try
			{
				sIn = IOConnection.OpenRead(iocFile);
				if(sIn == null) throw new FileNotFoundException();
			}
			catch(Exception) { return null; }

			byte[] pbHash;
			try
			{
				SHA256Managed sha256 = new SHA256Managed();
				pbHash = sha256.ComputeHash(sIn);
			}
			catch(Exception) { Debug.Assert(false); sIn.Close(); return null; }

			sIn.Close();
			return pbHash;
		}

		// See GetCommandLineFromUrl when editing this method
		public static bool IsCommandLineUrl(string strUrl)
		{
			if(strUrl == null) { Debug.Assert(false); return false; }

			string strLower = strUrl.ToLower();

			if(strLower.StartsWith("cmd://")) return true;
			if(strLower.StartsWith("\\\\")) return true; // UNC path support

			return false;
		}

		// See IsCommandLineUrl when editing this method
		public static string GetCommandLineFromUrl(string strUrl)
		{
			if(strUrl == null) { Debug.Assert(false); return string.Empty; }

			string strLower = strUrl.ToLower();

			if(strLower.StartsWith("cmd://")) return strUrl.Remove(0, 6);
			if(strLower.StartsWith("\\\\")) return strUrl; // UNC path support

			return strUrl;
		}

		public static bool RunElevated(string strExe, string strArgs,
			bool bShowMessageIfFailed)
		{
			if(strExe == null) throw new ArgumentNullException("strExe");

			try
			{
				ProcessStartInfo psi = new ProcessStartInfo();
				if(strArgs != null) psi.Arguments = strArgs;
				psi.FileName = strExe;
				psi.UseShellExecute = true;
				psi.WindowStyle = ProcessWindowStyle.Normal;

				// Elevate on Windows Vista and higher
				if(WinUtil.IsAtLeastWindowsVista) psi.Verb = "runas";

				Process.Start(psi);
			}
			catch(Exception ex)
			{
				if(bShowMessageIfFailed) MessageService.ShowWarning(ex);

				return false;
			}

			return true;
		}

		public static ulong GetMaxNetFrameworkVersion()
		{
			if(m_uFrameworkVersion != 0) return m_uFrameworkVersion;

			try
			{
				m_uFrameworkVersion = GetNetVersion();
				return m_uFrameworkVersion;
			}
			catch(Exception) { Debug.Assert(false); }

			return 0;
		}

		private static ulong GetNetVersion()
		{
			string strSysRoot = Environment.GetEnvironmentVariable("SystemRoot");
			string strFrameworks = UrlUtil.EnsureTerminatingSeparator(strSysRoot,
				false) + "Microsoft.NET" + Path.DirectorySeparatorChar + "Framework";
			if(!Directory.Exists(strFrameworks)) { Debug.Assert(false); return 0; }

			ulong uFrameworkVersion = 0;
			DirectoryInfo diFrameworks = new DirectoryInfo(strFrameworks);
			foreach(DirectoryInfo di in diFrameworks.GetDirectories("v*",
				SearchOption.TopDirectoryOnly))
			{
				string strVer = di.Name.TrimStart('v', 'V');
				ulong uVer = StrUtil.GetVersion(strVer);
				if(uVer > uFrameworkVersion) uFrameworkVersion = uVer;
			}

			return uFrameworkVersion;
		}

		public static string GetOSStr()
		{
			if(NativeLib.IsUnix()) return "Unix";
			return "Windows";
		}
	}
}
