/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2024 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using Microsoft.Win32;

using KeePass.Forms;
using KeePass.Native;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util.Spr;

using KeePassLib;
using KeePassLib.Delegates;
using KeePassLib.Serialization;
using KeePassLib.Utility;

using NativeLib = KeePassLib.Native.NativeLib;

namespace KeePass.Util
{
	public sealed class OpenUrlEventArgs : EventArgs
	{
		private string m_strUrl;
		public string Url
		{
			get { return m_strUrl; }
			set { m_strUrl = value; }
		}

		private readonly PwEntry m_pe;
		public PwEntry Entry
		{
			get { return m_pe; }
		}

		private readonly bool m_bAllowOverride;
		public bool AllowOverride
		{
			get { return m_bAllowOverride; }
		}

		private readonly string m_strBaseRaw;
		public string BaseRaw
		{
			get { return m_strBaseRaw; }
		}

		public OpenUrlEventArgs(string strUrlToOpen, PwEntry peDataSource,
			bool bAllowOverride, string strBaseRaw)
		{
			m_strUrl = strUrlToOpen;
			m_pe = peDataSource;
			m_bAllowOverride = bAllowOverride;
			m_strBaseRaw = strBaseRaw;
		}
	}

	public static class WinUtil
	{
		private static readonly bool g_bIsWindows9x = false;
		private static readonly bool g_bIsWindows2000 = false;
		private static readonly bool g_bIsWindowsXP = false;
		private static readonly bool g_bIsAtLeastWindows2000 = false;
		private static readonly bool g_bIsAtLeastWindowsVista = false;
		private static readonly bool g_bIsAtLeastWindows7 = false;
		private static readonly bool g_bIsAtLeastWindows8 = false;
		private static readonly bool g_bIsAtLeastWindows10 = false;
		private static readonly bool g_bIsAppX = false;

		private static string g_strExePath = null;

		private static ulong g_uFrameworkVersion = 0;

		public static event EventHandler<OpenUrlEventArgs> OpenUrlPre;

		public static bool IsWindows9x
		{
			get { return g_bIsWindows9x; }
		}

		public static bool IsWindows2000
		{
			get { return g_bIsWindows2000; }
		}

		public static bool IsWindowsXP
		{
			get { return g_bIsWindowsXP; }
		}

		public static bool IsAtLeastWindows2000
		{
			get { return g_bIsAtLeastWindows2000; }
		}

		public static bool IsAtLeastWindowsVista
		{
			get { return g_bIsAtLeastWindowsVista; }
		}

		public static bool IsAtLeastWindows7
		{
			get { return g_bIsAtLeastWindows7; }
		}

		public static bool IsAtLeastWindows8
		{
			get { return g_bIsAtLeastWindows8; }
		}

		public static bool IsAtLeastWindows10
		{
			get { return g_bIsAtLeastWindows10; }
		}

		public static bool IsAppX
		{
			get { return g_bIsAppX; }
		}

		static WinUtil()
		{
			if(NativeLib.IsUnix()) return;

			OperatingSystem os = Environment.OSVersion;
			Version v = os.Version;

			g_bIsWindows9x = (os.Platform == PlatformID.Win32Windows);
			g_bIsWindows2000 = ((v.Major == 5) && (v.Minor == 0));
			g_bIsWindowsXP = ((v.Major == 5) && (v.Minor == 1));

			g_bIsAtLeastWindows2000 = (v.Major >= 5);
			g_bIsAtLeastWindowsVista = (v.Major >= 6);
			g_bIsAtLeastWindows7 = ((v.Major >= 7) || ((v.Major == 6) && (v.Minor >= 1)));
			g_bIsAtLeastWindows8 = ((v.Major >= 7) || ((v.Major == 6) && (v.Minor >= 2)));

			// Environment.OSVersion is reliable only up to version 6.2;
			// https://msdn.microsoft.com/library/windows/desktop/ms724832.aspx
			RegistryKey rk = null;
			try
			{
				rk = Registry.LocalMachine.OpenSubKey(
					"SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", false);
				if(rk != null)
				{
					string str = rk.GetValue("CurrentMajorVersionNumber",
						string.Empty).ToString();
					uint u;
					if(uint.TryParse(str, out u))
						g_bIsAtLeastWindows10 = (u >= 10);
					else { Debug.Assert(string.IsNullOrEmpty(str)); }
				}
				else { Debug.Assert(false); }
			}
			catch(Exception) { Debug.Assert(false); }
			finally { if(rk != null) rk.Close(); }

			try
			{
				string strDir = UrlUtil.GetFileDirectory(GetExecutable(), false, false);
				if(strDir.IndexOf("\\WindowsApps\\", StrUtil.CaseIgnoreCmp) >= 0)
				{
					Regex rx = new Regex("\\\\WindowsApps\\\\.*?_\\d+(\\.\\d+)*_",
						RegexOptions.IgnoreCase);
					g_bIsAppX = rx.IsMatch(strDir);
				}
				else { Debug.Assert(!g_bIsAppX); } // No AppX by default
			}
			catch(Exception) { Debug.Assert(false); }
		}

		public static void OpenEntryUrl(PwEntry pe)
		{
			if(pe == null) { Debug.Assert(false); throw new ArgumentNullException("pe"); }

			string strUrl = pe.Strings.ReadSafe(PwDefs.UrlField);

			// The user interface enables the URL open command if and
			// only if the URL is not empty, i.e. it ignores overrides
			if(strUrl.Length == 0) return;

			if(pe.OverrideUrl.Length > 0)
				OpenUrl(pe.OverrideUrl, pe, true, strUrl);
			else
			{
				string strOverride = (Program.Config.Integration.UrlOverrideEnabled ?
					Program.Config.Integration.UrlOverride : null);

				if(!string.IsNullOrEmpty(strOverride))
					OpenUrl(strOverride, pe, true, strUrl);
				else
					OpenUrl(strUrl, pe, true);
			}
		}

		public static void OpenUrl(string strUrlToOpen, PwEntry peDataSource)
		{
			OpenUrl(strUrlToOpen, peDataSource, true, null);
		}

		public static void OpenUrl(string strUrlToOpen, PwEntry peDataSource,
			bool bAllowOverride)
		{
			OpenUrl(strUrlToOpen, peDataSource, bAllowOverride, null);
		}

		public static void OpenUrl(string strUrlToOpen, PwEntry peDataSource,
			bool bAllowOverride, string strBaseRaw)
		{
			VoidDelegate f = delegate()
			{
				try { OpenUrlPriv(strUrlToOpen, peDataSource, bAllowOverride, strBaseRaw); }
				catch(Exception) { Debug.Assert(false); }
			};

			MainForm mf = Program.MainForm;
			if((mf != null) && mf.InvokeRequired) mf.Invoke(f);
			else f();
		}

		private static void OpenUrlPriv(string strUrlToOpen, PwEntry peDataSource,
			bool bAllowOverride, string strBaseRaw)
		{
			if(string.IsNullOrEmpty(strUrlToOpen)) { Debug.Assert(false); return; }

			if(WinUtil.OpenUrlPre != null)
			{
				OpenUrlEventArgs e = new OpenUrlEventArgs(strUrlToOpen, peDataSource,
					bAllowOverride, strBaseRaw);
				WinUtil.OpenUrlPre(null, e);
				strUrlToOpen = e.Url;

				if(string.IsNullOrEmpty(strUrlToOpen)) return;
			}

			string strPrevWorkDir = WinUtil.GetWorkingDirectory();
			string strThisExe = WinUtil.GetExecutable();
			
			string strExeDir = UrlUtil.GetFileDirectory(strThisExe, false, true);
			WinUtil.SetWorkingDirectory(strExeDir);

			string strUrl = CompileUrl(strUrlToOpen, peDataSource, bAllowOverride,
				strBaseRaw, null);

			if(string.IsNullOrEmpty(strUrl)) { } // Might be placeholder only
			else if(WinUtil.IsCommandLineUrl(strUrl))
			{
				string strApp, strArgs;
				StrUtil.SplitCommandLine(WinUtil.GetCommandLineFromUrl(strUrl),
					out strApp, out strArgs);

				try
				{
					try { NativeLib.StartProcess(strApp, strArgs); }
					catch(Win32Exception)
					{
						ProcessStartInfo psi = new ProcessStartInfo();
						psi.FileName = strApp;
						if(!string.IsNullOrEmpty(strArgs)) psi.Arguments = strArgs;
						psi.UseShellExecute = false;

						NativeLib.StartProcess(psi);
					}
				}
				catch(Exception exCmd)
				{
					string strMsg = KPRes.FileOrUrl + ": " + strApp;
					if(!string.IsNullOrEmpty(strArgs))
						strMsg += MessageService.NewParagraph +
							KPRes.Arguments + ": " + strArgs;

					MessageService.ShowWarning(strMsg, exCmd);
				}
			}
			else // Standard URL
			{
				try { NativeLib.StartProcess(strUrl); }
				catch(Exception exUrl)
				{
					MessageService.ShowWarning(strUrl, exUrl);
				}
			}

			// Restore previous working directory
			WinUtil.SetWorkingDirectory(strPrevWorkDir);

			if(peDataSource != null) peDataSource.Touch(false);

			// SprEngine.Compile might have modified the database
			MainForm mf = Program.MainForm;
			if(mf != null)
			{
				mf.RefreshEntriesList();
				mf.UpdateUI(false, null, false, null, false, null, false);
			}
		}

		internal static string CompileUrl(string strUrlToOpen, PwEntry pe,
			bool bAllowOverride, string strBaseRaw, bool? obForceEncCmd)
		{
			MainForm mf = Program.MainForm;
			PwDatabase pd = null;
			try { if(mf != null) pd = mf.DocumentManager.SafeFindContainerOf(pe); }
			catch(Exception) { Debug.Assert(false); }

			string strUrlFlt = strUrlToOpen;
			strUrlFlt = strUrlFlt.TrimStart(new char[] { ' ', '\t', '\r', '\n' });

			bool bEncCmd = (obForceEncCmd ?? WinUtil.IsCommandLineUrl(strUrlFlt));

			SprContext ctx = new SprContext(pe, pd, SprCompileFlags.All, false, bEncCmd);
			ctx.Base = strBaseRaw;
			ctx.BaseIsEncoded = false;

			string strUrl = SprEngine.Compile(strUrlFlt, ctx);

			string strOvr = Program.Config.Integration.UrlSchemeOverrides.GetOverrideForUrl(
				strUrl);
			if(!bAllowOverride) strOvr = null;
			if(strOvr != null)
			{
				bool bEncCmdOvr = WinUtil.IsCommandLineUrl(strOvr);

				SprContext ctxOvr = new SprContext(pe, pd, SprCompileFlags.All,
					false, bEncCmdOvr);
				ctxOvr.Base = strUrl;
				ctxOvr.BaseIsEncoded = bEncCmd;

				strUrl = SprEngine.Compile(strOvr, ctxOvr);
			}

			return strUrl;
		}

		public static void OpenUrlWithApp(string strUrlToOpen, PwEntry peDataSource,
			string strAppPath)
		{
			if(string.IsNullOrEmpty(strUrlToOpen)) { Debug.Assert(false); return; }
			if(string.IsNullOrEmpty(strAppPath)) { Debug.Assert(false); return; }

			string strUrl = strUrlToOpen.Trim();
			if(strUrl.Length == 0) { Debug.Assert(false); return; }
			strUrl = SprEncoding.EncodeForCommandLine(strUrl);

			string strApp = strAppPath.Trim();
			if(strApp.Length == 0) { Debug.Assert(false); return; }
			strApp = SprEncoding.EncodeForCommandLine(strApp);

			string str = "cmd://\"" + strApp + "\" \"" + strUrl + "\"";
			OpenUrl(str, peDataSource, false);
		}

		internal static void OpenUrlDirectly(string strUrl)
		{
			if(string.IsNullOrEmpty(strUrl)) { Debug.Assert(false); return; }

			try { NativeLib.StartProcess(strUrl); }
			catch(Exception ex) { MessageService.ShowWarning(strUrl, ex); }
		}

		public static void Restart()
		{
			try { using(Process p = StartSelfEx(null)) { Debug.Assert(p != null); } }
			catch(Exception ex) { MessageService.ShowWarning(ex); }
		}

		internal static Process StartSelfEx(string strArgs)
		{
			string strExe = WinUtil.GetExecutable();

			ProcessStartInfo psi = new ProcessStartInfo();

			// Mono detects CLR binaries and runs them with the same Mono
			// binary (see mono/metadata/w32process-unix.c)
			// if(NativeLib.IsUnix())
			// {
			//	psi.FileName = "mono";
			//	string strArgsEx = "\"" + NativeLib.EncodeDataToArgs(strExe) + "\"";
			//	if(!string.IsNullOrEmpty(strArgs)) strArgsEx += " " + strArgs;
			//	psi.Arguments = strArgsEx;
			// }
			// else

			psi.FileName = strExe;
			if(!string.IsNullOrEmpty(strArgs)) psi.Arguments = strArgs;

			return NativeLib.StartProcessEx(psi);
		}

		public static string GetExecutable()
		{
			string str = g_strExePath;
			if(str != null) return str;

			try { str = Assembly.GetExecutingAssembly().Location; }
			catch(Exception) { }

			if(string.IsNullOrEmpty(str))
			{
				str = Assembly.GetExecutingAssembly().GetName().CodeBase;
				str = UrlUtil.FileUrlToPath(str);
			}

			g_strExePath = str;
			return str;
		}

		private static string g_strAsmVersion = null;
		internal static string GetAssemblyVersion()
		{
			if(g_strAsmVersion == null)
			{
				try
				{
					Version v = typeof(WinUtil).Assembly.GetName().Version;
					g_strAsmVersion = v.ToString(4);
				}
				catch(Exception) { Debug.Assert(false); }

				if(g_strAsmVersion == null)
					g_strAsmVersion = StrUtil.VersionToString(PwDefs.FileVersion64, 4);
			}

			return g_strAsmVersion;
		}

		/// <summary>
		/// Shorten a path.
		/// </summary>
		/// <param name="strPath">Path to make shorter.</param>
		/// <param name="cchMax">Maximum number of characters in the returned string.</param>
		/// <returns>Shortened path.</returns>
		public static string CompactPath(string strPath, int cchMax)
		{
			Debug.Assert(strPath != null);
			if(strPath == null) throw new ArgumentNullException("strPath");
			Debug.Assert(cchMax >= 0);
			if(cchMax < 0) throw new ArgumentOutOfRangeException("cchMax");

			if(strPath.Length <= cchMax) return strPath;
			if(cchMax == 0) return string.Empty;

			try
			{
				if(!NativeLib.IsUnix())
				{
					StringBuilder sb = new StringBuilder(strPath.Length + 2);

					if(NativeMethods.PathCompactPathEx(sb, strPath, (uint)cchMax + 1, 0))
					{
						if((sb.Length <= cchMax) && (sb.Length != 0))
							return sb.ToString();
						else { Debug.Assert(false); }
					}
				}
			}
			catch(Exception) { Debug.Assert(false); }

			return StrUtil.CompactString3Dots(strPath, cchMax);
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
					IntPtr.Zero, 0, IntPtr.Zero, 0, out dwDummy, IntPtr.Zero))
				{
					if(!NativeMethods.DeviceIoControl(hDevice, NativeMethods.FSCTL_UNLOCK_VOLUME,
						IntPtr.Zero, 0, IntPtr.Zero, 0, out dwDummy, IntPtr.Zero))
					{
						Debug.Assert(false);
					}
				}
				else bResult = false;

				if(strDir.Length > 0) WinUtil.SetWorkingDirectory(strDir);

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
				string strCur = WinUtil.GetWorkingDirectory();
				if((strCur == null) || (strCur.Length < 3)) return string.Empty;
				if(strCur[1] != ':') return string.Empty;
				if(strCur[2] != '\\') return string.Empty;

				char chPar = char.ToUpper(chDriveLetter);
				char chCur = char.ToUpper(strCur[0]);
				if(chPar != chCur) return string.Empty;

				string strTemp = UrlUtil.GetTempPath();
				WinUtil.SetWorkingDirectory(strTemp);

				return strCur;
			}
			catch(Exception) { Debug.Assert(false); }

			return string.Empty;
		}

		private static readonly string[] g_vIE7Windows = new string[] {
			"Windows Internet Explorer", "Maxthon"
		};

		public static bool IsInternetExplorer7Window(string strWindowTitle)
		{
			if(strWindowTitle == null) return false; // No assert or throw
			if(strWindowTitle.Length == 0) return false; // No assert or throw

			foreach(string str in g_vIE7Windows)
			{
				if(strWindowTitle.IndexOf(str) >= 0) return true;
			}

			return false;
		}

		public static byte[] HashFile(IOConnectionInfo iocFile)
		{
			if(iocFile == null) { Debug.Assert(false); return null; }

			try
			{
				using(Stream s = IOConnection.OpenRead(iocFile))
				{
					if(s == null) { Debug.Assert(false); return null; }

					using(SHA256Managed h = new SHA256Managed())
					{
						return h.ComputeHash(s);
					}
				}
			}
			catch(Exception) { Debug.Assert(false); }

			return null;
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
			return RunElevated(strExe, strArgs, bShowMessageIfFailed, 0);
		}

		internal static bool RunElevated(string strExe, string strArgs,
			bool bShowMessageIfFailed, int iWaitForExitMS)
		{
			try
			{
				if(strExe == null) throw new ArgumentNullException("strExe");

				ProcessStartInfo psi = new ProcessStartInfo();
				psi.FileName = strExe;
				if(!string.IsNullOrEmpty(strArgs)) psi.Arguments = strArgs;
				psi.UseShellExecute = true;

				string strStdIn = null;

				// Elevate on Windows Vista and higher
				if(WinUtil.IsAtLeastWindowsVista) psi.Verb = "RunAs";
				else if(NativeLib.IsUnix())
				{
					string strArgsEx = "\"" + NativeLib.EncodeDataToArgs(strExe) + "\"";
					if(strExe == GetExecutable()) strArgsEx = "mono " + strArgsEx;
					if(!string.IsNullOrEmpty(strArgs)) strArgsEx += " " + strArgs;

					psi.FileName = "sudo";
					psi.Arguments = "-k -S " + strArgsEx;
					psi.UseShellExecute = false;
					psi.RedirectStandardInput = true;

					SingleLineEditForm dlg = new SingleLineEditForm();
					dlg.InitEx(KPRes.AdminPassword, psi.FileName + " " + psi.Arguments,
						KPRes.AdminPassword + " (" + psi.FileName + "):",
						Properties.Resources.B48x48_KGPG_Key2, string.Empty, null);
					dlg.FlagsEx |= SlfFlags.Sensitive;
					if(UIUtil.ShowDialogAndDestroy(dlg) != DialogResult.OK)
						return false;

					strStdIn = dlg.ResultString + Environment.NewLine;
				}

				using(Process p = NativeLib.StartProcessEx(psi))
				{
					if(!string.IsNullOrEmpty(strStdIn))
						p.StandardInput.Write(strStdIn);

					if(iWaitForExitMS == 0) { }
					else if(iWaitForExitMS < 0) p.WaitForExit();
					else if(!p.WaitForExit(iWaitForExitMS))
						throw new TimeoutException();
				}
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
			ulong u = g_uFrameworkVersion;
			if(u != 0) return u;

			// https://www.mono-project.com/docs/about-mono/releases/
			ulong m = NativeLib.MonoVersion;
			if(m >= 0x0006000600000000UL) u = 0x0004000800000000UL;
			else if(m >= 0x0005001200000000UL) u = 0x0004000700020000UL;
			else if(m >= 0x0005000A00000000UL) u = 0x0004000700010000UL;
			else if(m >= 0x0005000400000000UL) u = 0x0004000700000000UL;
			else if(m >= 0x0004000600000000UL) u = 0x0004000600020000UL;
			else if(m >= 0x0004000400000000UL) u = 0x0004000600010000UL;
			else if(m >= 0x0003000800000000UL) u = 0x0004000500010000UL;
			else if(m >= 0x0003000000000000UL) u = 0x0004000500000000UL;

			if(u == 0)
			{
				try { u = GetMaxNetVersionPriv(); }
				catch(Exception) { Debug.Assert(false); }
			}

			if(u == 0)
			{
				Version v = Environment.Version;
				if(v.Major > 0) u |= (uint)v.Major;
				u <<= 16;
				if(v.Minor > 0) u |= (uint)v.Minor;
				u <<= 16;
				if(v.Build > 0) u |= (uint)v.Build;
				u <<= 16;
				if(v.Revision > 0) u |= (uint)v.Revision;
			}

			g_uFrameworkVersion = u;
			return u;
		}

		private static ulong GetMaxNetVersionPriv()
		{
			RegistryKey kNdp = Registry.LocalMachine.OpenSubKey(
				"SOFTWARE\\Microsoft\\NET Framework Setup\\NDP", false);
			if(kNdp == null) { Debug.Assert(false); return 0; }

			ulong uMaxVer = 0;

			string[] vInNdp = kNdp.GetSubKeyNames();
			foreach(string strInNdp in vInNdp)
			{
				if(strInNdp == null) { Debug.Assert(false); continue; }
				if(!strInNdp.StartsWith("v", StrUtil.CaseIgnoreCmp)) continue;

				RegistryKey kVer = kNdp.OpenSubKey(strInNdp, false);
				if(kVer != null)
				{
					UpdateNetVersionFromRegKey(kVer, ref uMaxVer);

					string[] vProfiles = kVer.GetSubKeyNames();
					foreach(string strProfile in vProfiles)
					{
						if(string.IsNullOrEmpty(strProfile)) { Debug.Assert(false); continue; }

						RegistryKey kPro = kVer.OpenSubKey(strProfile, false);
						UpdateNetVersionFromRegKey(kPro, ref uMaxVer);
						if(kPro != null) kPro.Close();
					}

					kVer.Close();
				}
				else { Debug.Assert(false); }
			}

			kNdp.Close();
			return uMaxVer;
		}

		private static void UpdateNetVersionFromRegKey(RegistryKey k, ref ulong uMaxVer)
		{
			if(k == null) { Debug.Assert(false); return; }

			try
			{
				// https://msdn.microsoft.com/en-us/library/hh925568.aspx
				string strInstall = k.GetValue("Install", string.Empty).ToString();
				if((strInstall.Length > 0) && (strInstall != "1")) return;

				string strVer = k.GetValue("Version", string.Empty).ToString();
				if(strVer.Length > 0)
				{
					ulong uVer = StrUtil.ParseVersion(strVer);
					if(uVer > uMaxVer) uMaxVer = uVer;
				}
			}
			catch(Exception) { Debug.Assert(false); }
		}

		/* private static ulong GetMaxNetVersionPriv()
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
				ulong uVer = StrUtil.ParseVersion(strVer);
				if(uVer > uFrameworkVersion) uFrameworkVersion = uVer;
			}

			return uFrameworkVersion;
		} */

		public static string GetOSStr()
		{
			if(NativeLib.IsUnix()) return "Unix";
			return "Windows";
		}

		public static void RemoveZoneIdentifier(string strFilePath)
		{
			// No throw
			if(string.IsNullOrEmpty(strFilePath)) { Debug.Assert(false); return; }

			try
			{
				string strZoneId = strFilePath + ":Zone.Identifier";

				if(NativeMethods.FileExists(strZoneId))
					NativeMethods.DeleteFile(strZoneId);
			}
			catch(Exception) { Debug.Assert(NativeLib.IsUnix()); }
		}

		[Obsolete]
		public static string RunConsoleApp(string strAppPath, string strParams)
		{
			return NativeLib.RunConsoleApp(strAppPath, strParams);
		}

		public static string LocateSystemApp(string strExeName)
		{
			if(strExeName == null) { Debug.Assert(false); return string.Empty; }
			if(strExeName.Length == 0) return strExeName;

			if(NativeLib.IsUnix()) return strExeName;

			try
			{
				string str = null;
				for(int i = 0; i < 3; ++i)
				{
					if(i == 0)
						str = Environment.GetFolderPath(
							Environment.SpecialFolder.System);
					else if(i == 1)
						str = Environment.GetEnvironmentVariable("WinDir");
					else if(i == 2)
						str = Environment.GetEnvironmentVariable("SystemRoot");

					if(!string.IsNullOrEmpty(str))
					{
						str = UrlUtil.EnsureTerminatingSeparator(str, false);
						str += strExeName;

						if(File.Exists(str)) return str;
					}
				}
			}
			catch(Exception) { Debug.Assert(false); }

			return strExeName;
		}

		public static string GetHomeDirectory()
		{
			string str = null;
			try
			{
				str = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			}
			catch(Exception) { Debug.Assert(false); }

			if(string.IsNullOrEmpty(str))
			{
				try
				{
					str = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				}
				catch(Exception) { Debug.Assert(false); }
			}

			if(string.IsNullOrEmpty(str)) { Debug.Assert(false); return string.Empty; }

			return str;
		}

		public static string GetWorkingDirectory()
		{
			string strWorkDir = null;
			try { strWorkDir = Directory.GetCurrentDirectory(); }
			catch(Exception) { Debug.Assert(false); }

			return (!string.IsNullOrEmpty(strWorkDir) ? strWorkDir : GetHomeDirectory());
		}

		public static void SetWorkingDirectory(string strWorkDir)
		{
			string str = strWorkDir; // May be null

			if(!string.IsNullOrEmpty(str))
			{
				try { if(!Directory.Exists(str)) str = null; }
				catch(Exception) { Debug.Assert(false); str = null; }
			}

			if(string.IsNullOrEmpty(str))
				str = GetHomeDirectory(); // Not app dir

			try { Directory.SetCurrentDirectory(str); }
			catch(Exception) { Debug.Assert(false); }
		}

		internal static void ShowFileInFileManager(string strFilePath, bool bShowError)
		{
			if(string.IsNullOrEmpty(strFilePath)) { Debug.Assert(false); return; }

			try
			{
				string strDir = UrlUtil.GetFileDirectory(strFilePath, false, true);
				if(NativeLib.IsUnix())
				{
					NativeLib.StartProcess(strDir);
					return;
				}

				string strExplorer = WinUtil.LocateSystemApp("Explorer.exe");

				if(File.Exists(strFilePath))
					NativeLib.StartProcess(strExplorer, "/select,\"" +
						NativeLib.EncodeDataToArgs(strFilePath) + "\"");
				else
					NativeLib.StartProcess(strDir);
			}
			catch(Exception ex)
			{
				if(bShowError)
					MessageService.ShowWarning(strFilePath, ex.Message);
			}
		}
	}
}
