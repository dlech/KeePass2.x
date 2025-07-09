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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

#if !KeePassUAP
using System.IO;
using System.Threading;
using System.Windows.Forms;
#endif

using KeePassLib.Resources;
using KeePassLib.Utility;

namespace KeePassLib.Native
{
	public static class NativeLib
	{
		internal const string BaseName = "KeePassLibN";
		internal const string DllFileX32 = NativeLib.BaseName + ".x32.dll";
		internal const string DllFileX64 = NativeLib.BaseName + ".x64.dll";
		internal const string DllFileA64 = NativeLib.BaseName + ".a64.dll";

		internal const CallingConvention DllCallingConvention = CallingConvention.Cdecl;

		private static readonly object g_oSyncRoot = new object();

		private static bool g_bAllowNative = true;
		public static bool AllowNative
		{
			get { return g_bAllowNative; }
			set { g_bAllowNative = value; }
		}

		internal static string FileName
		{
			get
			{
				string str;
				switch(NativeLib.ProcessArchitecture)
				{
					case ArchitectureEx.X86: str = DllFileX32; break;
					case ArchitectureEx.X64: str = DllFileX64; break;
					case ArchitectureEx.Arm64: str = DllFileA64; break;
					default: Debug.Assert(false); str = null; break;
				}
				return str;
			}
		}

		private static int g_iProcessArch = -1;
		internal static ArchitectureEx ProcessArchitecture
		{
			get
			{
#if DEBUG
				foreach(ArchitectureEx aE in Enum.GetValues(typeof(ArchitectureEx)))
				{
					Debug.Assert((int)aE >= 0);
				}
#endif

				if(g_iProcessArch >= 0) return (ArchitectureEx)g_iProcessArch;

				ArchitectureEx a = ArchitectureEx.None;
				try
				{
					Type t = typeof(Marshal).Assembly.GetType(
						"System.Runtime.InteropServices.RuntimeInformation", false);
					if(t != null)
					{
						PropertyInfo pi = t.GetProperty("ProcessArchitecture",
							(BindingFlags.Public | BindingFlags.Static));
						if(pi != null)
						{
							object o = pi.GetValue(null, null);
							string str = ((o != null) ? o.ToString() : null);
							if(str != null)
							{
								switch(str)
								{
									case "X86": a = ArchitectureEx.X86; break;
									case "X64": a = ArchitectureEx.X64; break;
									case "Arm": a = ArchitectureEx.Arm; break;
									case "Arm64": a = ArchitectureEx.Arm64; break;
									default: Debug.Assert(false); break;
								}
							}
							else { Debug.Assert(false); }
						}
						else { Debug.Assert(false); }
					}
					else
					{
						// The RuntimeInformation class has been added in
						// .NET 4.7.1; before that, we can assume x86/x64
						a = ((IntPtr.Size == 4) ? ArchitectureEx.X86 : ArchitectureEx.X64);
					}
				}
				catch(Exception) { Debug.Assert(false); }

				g_iProcessArch = (int)a;
				return a;
			}
		}

		private static ulong? g_ouMonoVersion = null;
		public static ulong MonoVersion
		{
			get
			{
				if(g_ouMonoVersion.HasValue) return g_ouMonoVersion.Value;

				ulong uVersion = 0;
				try
				{
					Type t = Type.GetType("Mono.Runtime", false);
					if(t != null)
					{
						MethodInfo mi = t.GetMethod("GetDisplayName",
							BindingFlags.NonPublic | BindingFlags.Static);
						if(mi != null)
						{
							string strName = (mi.Invoke(null, null) as string);
							if(!string.IsNullOrEmpty(strName))
							{
								Match m = Regex.Match(strName, "\\d+(\\.\\d+)+");
								if(m.Success)
									uVersion = StrUtil.ParseVersion(m.Value);
								else { Debug.Assert(false); }
							}
							else { Debug.Assert(false); }
						}
						else { Debug.Assert(false); }
					}
					else { Debug.Assert(!IsUnix()); }
				}
				catch(Exception) { Debug.Assert(false); }

				g_ouMonoVersion = uVersion;
				return uVersion;
			}
		}

		public static bool IsLibraryInstalled()
		{
			byte[] pbDummyD = new byte[32], pbDummyS = new byte[32];

			lock(g_oSyncRoot)
			{
				bool bAllow = g_bAllowNative;
				g_bAllowNative = true;

				try { return TransformKey256(pbDummyD, pbDummyS, 16); }
				finally { g_bAllowNative = bAllow; }
			}
		}

		private static bool? g_obIsUnix = null;
		public static bool IsUnix()
		{
			if(g_obIsUnix.HasValue) return g_obIsUnix.Value;

			PlatformID p = GetPlatformID();

			// Mono defines Unix as 128 in early .NET versions
#if !KeePassLibSD
			g_obIsUnix = ((p == PlatformID.Unix) || (p == PlatformID.MacOSX) ||
				((int)p == 128));
#else
			g_obIsUnix = (((int)p == 4) || ((int)p == 6) || ((int)p == 128));
#endif
			return g_obIsUnix.Value;
		}

		private static PlatformID? g_opl = null;
		public static PlatformID GetPlatformID()
		{
			if(g_opl.HasValue) return g_opl.Value;

#if KeePassUAP
			g_opl = EnvironmentExt.OSVersion.Platform;
#else
			g_opl = Environment.OSVersion.Platform;
#endif

#if (!KeePassLibSD && !KeePassUAP)
			// Mono returns PlatformID.Unix on MacOS, workaround this
			if(g_opl.Value == PlatformID.Unix)
			{
				if((RunConsoleApp("uname", null) ?? string.Empty).Trim().Equals(
					"Darwin", StrUtil.CaseIgnoreCmp))
					g_opl = PlatformID.MacOSX;
			}
#endif

			return g_opl.Value;
		}

		private static DesktopType? g_otDesktop = null;
		public static DesktopType GetDesktopType()
		{
			if(!g_otDesktop.HasValue)
			{
				DesktopType t = DesktopType.None;
				if(!IsUnix()) t = DesktopType.Windows;
				else
				{
					try
					{
						string strXdg = (Environment.GetEnvironmentVariable(
							"XDG_CURRENT_DESKTOP") ?? string.Empty).Trim();
						string strGdm = (Environment.GetEnvironmentVariable(
							"GDMSESSION") ?? string.Empty).Trim();
						StringComparison sc = StrUtil.CaseIgnoreCmp;

						if(strXdg.Equals("Unity", sc))
							t = DesktopType.Unity;
						else if(strXdg.Equals("LXDE", sc))
							t = DesktopType.Lxde;
						else if(strXdg.Equals("XFCE", sc))
							t = DesktopType.Xfce;
						else if(strXdg.Equals("MATE", sc))
							t = DesktopType.Mate;
						else if(strXdg.Equals("X-Cinnamon", sc)) // Mint 18.3
							t = DesktopType.Cinnamon;
						else if(strXdg.Equals("Pantheon", sc)) // Elementary OS
							t = DesktopType.Pantheon;
						else if(strXdg.Equals("KDE", sc) || // Mint 16, Kubuntu 17.10
							strGdm.Equals("kde-plasma", sc)) // Ubuntu 12.04
							t = DesktopType.Kde;
						else if(strXdg.Equals("GNOME", sc))
						{
							if(strGdm.Equals("cinnamon", sc)) // Mint 13
								t = DesktopType.Cinnamon;
							else t = DesktopType.Gnome; // Fedora 27
						}
						else if(strXdg.Equals("ubuntu:GNOME", sc))
							t = DesktopType.Gnome;
					}
					catch(Exception) { Debug.Assert(false); }
				}

				g_otDesktop = t;
			}

			return g_otDesktop.Value;
		}

		private static bool? g_obWayland = null;
		internal static bool IsWayland()
		{
			if(!g_obWayland.HasValue)
			{
				bool b = false;
				try
				{
					// https://www.freedesktop.org/software/systemd/man/pam_systemd.html
					b = ((Environment.GetEnvironmentVariable("XDG_SESSION_TYPE") ??
						string.Empty).Trim().Equals("wayland", StrUtil.CaseIgnoreCmp));
				}
				catch(Exception) { Debug.Assert(false); }

				g_obWayland = b;
			}

			return g_obWayland.Value;
		}

		internal static string ToString(ArchitectureEx a)
		{
			string str;

			switch(a)
			{
				case ArchitectureEx.X86: str = "x86"; break;
				case ArchitectureEx.X64: str = "x64"; break;
				case ArchitectureEx.Arm: str = "ARM32"; break;
				case ArchitectureEx.Arm64: str = "ARM64"; break;
				default:
					Debug.Assert(a == ArchitectureEx.None);
					str = string.Empty;
					break;
			}

			return str;
		}

#if (!KeePassLibSD && !KeePassUAP)
		public static string RunConsoleApp(string strAppPath, string strParams)
		{
			return RunConsoleApp(strAppPath, strParams, null);
		}

		public static string RunConsoleApp(string strAppPath, string strParams,
			string strStdInput)
		{
			return RunConsoleApp(strAppPath, strParams, strStdInput,
				(AppRunFlags.GetStdOutput | AppRunFlags.WaitForExit));
		}

		private delegate string RunProcessDelegate();

		public static string RunConsoleApp(string strAppPath, string strParams,
			string strStdInput, AppRunFlags f)
		{
			if(strAppPath == null) throw new ArgumentNullException("strAppPath");
			if(strAppPath.Length == 0) throw new ArgumentException("strAppPath");

			bool bStdOut = ((f & AppRunFlags.GetStdOutput) != AppRunFlags.None);

			RunProcessDelegate fnRun = delegate()
			{
				Process pToDispose = null;
				try
				{
					ProcessStartInfo psi = new ProcessStartInfo();

					psi.FileName = strAppPath;
					if(!string.IsNullOrEmpty(strParams)) psi.Arguments = strParams;

					psi.CreateNoWindow = true;
					psi.WindowStyle = ProcessWindowStyle.Hidden;
					psi.UseShellExecute = false;

					psi.RedirectStandardOutput = bStdOut;
					if(strStdInput != null) psi.RedirectStandardInput = true;

					Process p = StartProcessEx(psi);
					pToDispose = p;

					if(strStdInput != null)
					{
						EnsureNoBom(p.StandardInput);

						p.StandardInput.Write(strStdInput);
						p.StandardInput.Dispose();
					}

					string strOutput = string.Empty;
					if(bStdOut) strOutput = p.StandardOutput.ReadToEnd();

					if((f & AppRunFlags.WaitForExit) != AppRunFlags.None)
						p.WaitForExit();
					else if((f & AppRunFlags.GCKeepAlive) != AppRunFlags.None)
					{
						pToDispose = null; // Thread disposes it

						Thread th = new Thread(delegate()
						{
							try { p.WaitForExit(); p.Dispose(); }
							catch(Exception) { Debug.Assert(false); }
						});
						th.Start();
					}

					return strOutput;
				}
#if DEBUG
				catch(ThreadAbortException) { }
				catch(Win32Exception exW)
				{
					Debug.Assert((strAppPath == ClipboardU.XSel) &&
						(exW.NativeErrorCode == 2)); // XSel not found
				}
				catch(Exception) { Debug.Assert(false); }
#else
				catch(Exception) { }
#endif
				finally
				{
					try { if(pToDispose != null) pToDispose.Dispose(); }
					catch(Exception) { Debug.Assert(false); }
				}

				return null;
			};

			if((f & AppRunFlags.DoEvents) != AppRunFlags.None)
			{
				List<Form> lDisabledForms = new List<Form>();
				if((f & AppRunFlags.DisableForms) != AppRunFlags.None)
				{
					foreach(Form form in Application.OpenForms)
					{
						if(!form.Enabled) continue;

						lDisabledForms.Add(form);
						form.Enabled = false;
					}
				}

				IAsyncResult ar = fnRun.BeginInvoke(null, null);

				while(!ar.AsyncWaitHandle.WaitOne(0))
				{
					Application.DoEvents();
					Thread.Sleep(2);
				}

				string strRet = fnRun.EndInvoke(ar);

				for(int i = lDisabledForms.Count - 1; i >= 0; --i)
					lDisabledForms[i].Enabled = true;

				return strRet;
			}

			return fnRun();
		}

		private static void EnsureNoBom(StreamWriter sw)
		{
			if(sw == null) { Debug.Assert(false); return; }
			if(!MonoWorkarounds.IsRequired(1219)) return;

			try
			{
				Encoding enc = sw.Encoding;
				if(enc == null) { Debug.Assert(false); return; }
				byte[] pbBom = enc.GetPreamble();
				if((pbBom == null) || (pbBom.Length == 0)) return;

				// For Mono >= 4.0 (using Microsoft's reference source)
				try
				{
					FieldInfo fi = typeof(StreamWriter).GetField("haveWrittenPreamble",
						BindingFlags.Instance | BindingFlags.NonPublic);
					if(fi != null)
					{
						fi.SetValue(sw, true);
						return;
					}
				}
				catch(Exception) { Debug.Assert(false); }

				// For Mono < 4.0
				FieldInfo fiPD = typeof(StreamWriter).GetField("preamble_done",
					BindingFlags.Instance | BindingFlags.NonPublic);
				if(fiPD != null) fiPD.SetValue(sw, true);
				else { Debug.Assert(false); }
			}
			catch(Exception) { Debug.Assert(false); }
		}
#endif

		public static bool TransformKey256(byte[] pbData32, byte[] pbSeed32,
			ulong uRounds)
		{
#if KeePassUAP
			return false;
#else
			if(pbData32 == null) { Debug.Assert(false); return false; }
			if(pbData32.Length != 32) { Debug.Assert(false); return false; }
			if(pbSeed32 == null) { Debug.Assert(false); return false; }
			if(pbSeed32.Length != 32) { Debug.Assert(false); return false; }

			if(IsUnix() || !g_bAllowNative) return false;

			byte[] pbDataL = null, pbDataR = null;
			try
			{
				pbDataL = MemUtil.Mid(pbData32, 0, 16);
				pbDataR = MemUtil.Mid(pbData32, 16, 16);

				bool bL = false, bR = false;

				Thread thL = new Thread(new ThreadStart(() => { bL =
					TransformKeyHalf(pbDataL, pbSeed32, uRounds); }));
				Thread thR = new Thread(new ThreadStart(() => { bR =
					TransformKeyHalf(pbDataR, pbSeed32, uRounds); }));
				thL.Start();
				thR.Start();
				thL.Join();
				thR.Join();

				if(bL && bR)
				{
					Array.Copy(pbDataL, 0, pbData32, 0, 16);
					Array.Copy(pbDataR, 0, pbData32, 16, 16);
					return true;
				}
			}
			catch(Exception) { Debug.Assert(false); }
			finally
			{
				MemUtil.ZeroByteArray(pbDataL);
				MemUtil.ZeroByteArray(pbDataR);
			}

			return false;
		}

		private static bool TransformKeyHalf(byte[] pbData16, byte[] pbSeed32,
			ulong uRounds)
		{
			try
			{
				using(NativeBufferEx nbData16 = new NativeBufferEx(pbData16,
					true, true, 16))
				{
					using(NativeBufferEx nbSeed32 = new NativeBufferEx(pbSeed32,
						true, true, 16))
					{
						if(NativeMethods.AesKdfTransformHalf(nbData16.Data,
							nbSeed32.Data, uRounds))
						{
							nbData16.CopyTo(pbData16);
							return true;
						}
						Debug.Assert(false);
					}
				}
			}
			catch(DllNotFoundException) { }
			catch(Exception) { Debug.Assert(false); }

			return false;
		}
#endif

		public static bool TransformKeyBenchmark256(uint uMilliseconds, out ulong uRounds)
		{
			uRounds = 0;

#if KeePassUAP
			return false;
#else
			if(IsUnix() || !g_bAllowNative) return false;

			try
			{
				ulong uRoundsL = 0, uRoundsR = 0;

				Thread thL = new Thread(new ThreadStart(() => { uRoundsL =
					TransformKeyBenchmarkHalf(uMilliseconds); }));
				Thread thR = new Thread(new ThreadStart(() => { uRoundsR =
					TransformKeyBenchmarkHalf(uMilliseconds); }));
				thL.Start();
				thR.Start();
				thL.Join();
				thR.Join();

				if((uRoundsL != 0) && (uRoundsR != 0))
				{
					uRounds = (uRoundsL >> 1) + (uRoundsR >> 1);
					return true;
				}
			}
			catch(Exception) { Debug.Assert(false); }

			return false;
		}

		private static ulong TransformKeyBenchmarkHalf(uint uMilliseconds)
		{
			try { return NativeMethods.AesKdfTransformBenchmarkHalf(uMilliseconds); }
			catch(DllNotFoundException) { }
			catch(Exception) { Debug.Assert(false); }

			return 0;
		}
#endif

		// internal static Type GetUwpType(string strType)
		// {
		//	if(string.IsNullOrEmpty(strType)) { Debug.Assert(false); return null; }
		//	// https://referencesource.microsoft.com/#mscorlib/system/runtime/interopservices/windowsruntime/winrtclassactivator.cs
		//	return Type.GetType(strType + ", Windows, ContentType=WindowsRuntime", false);
		// }

		// Cf. DecodeArgsToData
		internal static string EncodeDataToArgs(string strData)
		{
			if(strData == null) { Debug.Assert(false); return string.Empty; }

			if(MonoWorkarounds.IsRequired(3471228285U) && IsUnix())
			{
				string str = strData;

				str = str.Replace("\\", "\\\\");
				str = str.Replace("\"", "\\\"");

				// Whether '\'' needs to be encoded depends on the context
				// (e.g. surrounding quotes); as we do not know what the
				// caller does with the returned string, we assume that
				// it will be used in a context where '\'' must not be
				// encoded; this behavior is documented
				// str = str.Replace("\'", "\\\'");

				return str;
			}

			// SHELLEXECUTEINFOW structure documentation:
			// https://docs.microsoft.com/en-us/windows/desktop/api/shellapi/ns-shellapi-shellexecuteinfow
			// return strData.Replace("\"", "\"\"\"");

			// Microsoft C/C++ startup code:
			// https://docs.microsoft.com/en-us/cpp/cpp/parsing-cpp-command-line-arguments
			// CommandLineToArgvW function:
			// https://docs.microsoft.com/en-us/windows/desktop/api/shellapi/nf-shellapi-commandlinetoargvw

			StringBuilder sb = new StringBuilder();
			int i = 0;
			while(i < strData.Length)
			{
				char ch = strData[i++];

				if(ch == '\\')
				{
					int cBackslashes = 1;
					while((i < strData.Length) && (strData[i] == '\\'))
					{
						++cBackslashes;
						++i;
					}

					if(i == strData.Length)
						sb.Append('\\', cBackslashes); // Assume no quote follows
					else if(strData[i] == '\"')
					{
						sb.Append('\\', (cBackslashes * 2) + 1);
						sb.Append('\"');
						++i;
					}
					else sb.Append('\\', cBackslashes);
				}
				else if(ch == '\"') sb.Append("\\\"");
				else sb.Append(ch);
			}

			return sb.ToString();
		}

		// Cf. EncodeDataToArgs
		internal static string DecodeArgsToData(string strArgs)
		{
			if(strArgs == null) { Debug.Assert(false); return string.Empty; }

			Debug.Assert(StrUtil.Count(strArgs, "\"") == StrUtil.Count(strArgs, "\\\""));

			if(MonoWorkarounds.IsRequired(3471228285U) && IsUnix())
			{
				string str = strArgs;

				str = str.Replace("\\\"", "\"");
				str = str.Replace("\\\\", "\\");

				return str;
			}

			StringBuilder sb = new StringBuilder();
			int i = 0;
			while(i < strArgs.Length)
			{
				char ch = strArgs[i++];

				if(ch == '\\')
				{
					int cBackslashes = 1;
					while((i < strArgs.Length) && (strArgs[i] == '\\'))
					{
						++cBackslashes;
						++i;
					}

					if(i == strArgs.Length)
						sb.Append('\\', cBackslashes); // Assume no quote follows
					else if(strArgs[i] == '\"')
					{
						Debug.Assert((cBackslashes & 1) == 1);
						sb.Append('\\', (cBackslashes - 1) / 2);
						sb.Append('\"');
						++i;
					}
					else sb.Append('\\', cBackslashes);
				}
				else sb.Append(ch);
			}

			return sb.ToString();
		}

		internal static void StartProcess(string strFile)
		{
			StartProcess(strFile, null);
		}

		internal static void StartProcess(string strFile, string strArgs)
		{
			ProcessStartInfo psi = new ProcessStartInfo();
			if(!string.IsNullOrEmpty(strFile)) psi.FileName = strFile;
			if(!string.IsNullOrEmpty(strArgs)) psi.Arguments = strArgs;
			psi.UseShellExecute = true;

			StartProcess(psi);
		}

		internal static void StartProcess(ProcessStartInfo psi)
		{
			Process p = StartProcessEx(psi);

			try { if(p != null) p.Dispose(); }
			catch(Exception) { Debug.Assert(false); }
		}

		internal static Process StartProcessEx(ProcessStartInfo psi)
		{
			if(psi == null) { Debug.Assert(false); return null; }

			string strFileOrg = psi.FileName;
			if(string.IsNullOrEmpty(strFileOrg)) { Debug.Assert(false); return null; }
			string strArgsOrg = psi.Arguments;

			Process p;
			try
			{
				CustomizeProcessStartInfo(psi);
				p = Process.Start(psi);
			}
			finally
			{
				psi.FileName = strFileOrg; // Restore
				psi.Arguments = strArgsOrg;
			}

			return p;
		}

		private static void CustomizeProcessStartInfo(ProcessStartInfo psi)
		{
			string strFile = psi.FileName, strArgs = psi.Arguments;

			string[] vUrlEncSchemes = new string[] {
				"file:", "ftp:", "ftps:", "http:", "https:",
				"mailto:", "scp:", "sftp:"
			};
			foreach(string strPfx in vUrlEncSchemes)
			{
				if(strFile.StartsWith(strPfx, StrUtil.CaseIgnoreCmp))
				{
					Debug.Assert(string.IsNullOrEmpty(strArgs));

					strFile = strFile.Replace("\"", "%22");
					strFile = strFile.Replace("\'", "%27");
					strFile = strFile.Replace("\\", "%5C");
					break;
				}
			}

			if(IsUnix())
			{
				if(MonoWorkarounds.IsRequired(19836) && string.IsNullOrEmpty(strArgs))
				{
					if(Regex.IsMatch(strFile, "^[a-zA-Z][a-zA-Z0-9\\+\\-\\.]*:",
						RegexOptions.Singleline) ||
						strFile.EndsWith(".html", StrUtil.CaseIgnoreCmp))
					{
						bool bMacOS = (GetPlatformID() == PlatformID.MacOSX);

						strArgs = "\"" + EncodeDataToArgs(strFile) + "\"";
						strFile = (bMacOS ? "open" : "xdg-open");
					}
				}

				// Mono's Process.Start method replaces '\\' by '/',
				// which may cause a different file to be executed;
				// therefore, we refuse to start such files
				if(strFile.Contains("\\") && MonoWorkarounds.IsRequired(190417))
					throw new ArgumentException(KLRes.PathBackslash);

				strFile = strFile.Replace("\\", "\\\\"); // If WA not required
				strFile = strFile.Replace("\"", "\\\"");
			}

			psi.FileName = strFile;
			psi.Arguments = strArgs;
		}
	}
}
