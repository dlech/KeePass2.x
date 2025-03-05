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

// #define KP_DEVSNAP
#if KP_DEVSNAP
#warning KP_DEVSNAP is defined!
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Resources;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

using KeePass.App;
using KeePass.App.Configuration;
using KeePass.DataExchange;
using KeePass.Ecas;
using KeePass.Forms;
using KeePass.Native;
using KeePass.Plugins;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;
using KeePass.Util.Archive;
using KeePass.Util.XmlSerialization;

using KeePassLib;
using KeePassLib.Cryptography;
using KeePassLib.Cryptography.PasswordGenerator;
using KeePassLib.Delegates;
using KeePassLib.Keys;
using KeePassLib.Resources;
using KeePassLib.Serialization;
using KeePassLib.Translation;
using KeePassLib.Utility;

using NativeLib = KeePassLib.Native.NativeLib;

namespace KeePass
{
	public static class Program
	{
		private const string StrWndMsgID = "EB2FE38E1A6A4A138CF561442F1CF25A";

		private static bool g_bMain = false;
		private static Mutex g_mGlobalNotify = null;

#if KP_DEVSNAP
		private static bool g_bAsmResReg = false;
#endif

		public enum AppMessage // int
		{
			Null = 0,
			RestoreWindow = 1,
			Exit = 2,
			IpcByFile = 3, // Handled by all other instances
			AutoType = 4,
			Lock = 5,
			Unlock = 6,
			AutoTypeSelected = 7,
			Cancel = 8,
			AutoTypePassword = 9,
			IpcByFile1 = 10 // Handled by 1 other instance
		}

		private static CommandLineArgs g_cmdLineArgs = null;
		public static CommandLineArgs CommandLineArgs
		{
			get
			{
				if(g_cmdLineArgs == null)
				{
					Debug.Assert(!g_bMain);
					g_cmdLineArgs = new CommandLineArgs(null);
				}

				return g_cmdLineArgs;
			}
		}

		private static Random g_rndGlobal = null;
		public static Random GlobalRandom
		{
			get
			{
				if(g_rndGlobal == null) g_rndGlobal = CryptoRandom.NewWeakRandom();
				return g_rndGlobal;
			}
		}

		private static int g_nAppMessage = 0;
		public static int ApplicationMessage
		{
			get
			{
				Debug.Assert((g_nAppMessage != 0) || !g_bMain || NativeLib.IsUnix());
				return g_nAppMessage;
			}
		}

		private static MainForm g_formMain = null;
		public static MainForm MainForm
		{
			get { return g_formMain; }
		}

		private static AppConfigEx g_appConfig = null;
		public static AppConfigEx Config
		{
			get
			{
				if(g_appConfig == null) { Debug.Assert(false); g_appConfig = new AppConfigEx(); }
				return g_appConfig;
			}
		}

		private static KeyProviderPool g_keyProviderPool = null;
		public static KeyProviderPool KeyProviderPool
		{
			get
			{
				if(g_keyProviderPool == null) g_keyProviderPool = new KeyProviderPool();
				return g_keyProviderPool;
			}
		}

		private static KeyValidatorPool g_keyValidatorPool = null;
		public static KeyValidatorPool KeyValidatorPool
		{
			get
			{
				if(g_keyValidatorPool == null) g_keyValidatorPool = new KeyValidatorPool();
				return g_keyValidatorPool;
			}
		}

		private static FileFormatPool g_fmtPool = null;
		public static FileFormatPool FileFormatPool
		{
			get
			{
				if(g_fmtPool == null) g_fmtPool = new FileFormatPool();
				return g_fmtPool;
			}
		}

		private static KPTranslation g_kpTranslation = null;
		public static KPTranslation Translation
		{
			get
			{
				if(g_kpTranslation == null) g_kpTranslation = new KPTranslation();
				return g_kpTranslation;
			}
		}

		private static TempFilesPool g_tempFilesPool = null;
		public static TempFilesPool TempFilesPool
		{
			get
			{
				if(g_tempFilesPool == null) g_tempFilesPool = new TempFilesPool();
				return g_tempFilesPool;
			}
		}

		private static EcasPool g_ecasPool = null;
		public static EcasPool EcasPool
		{
			get
			{
				if(g_ecasPool == null) g_ecasPool = new EcasPool(true);
				return g_ecasPool;
			}
		}

		public static EcasTriggerSystem TriggerSystem
		{
			get { return Program.Config.Application.TriggerSystem; }
		}

		private static CustomPwGeneratorPool g_pwGenPool = null;
		public static CustomPwGeneratorPool PwGeneratorPool
		{
			get
			{
				if(g_pwGenPool == null) g_pwGenPool = new CustomPwGeneratorPool();
				return g_pwGenPool;
			}
		}

		private static ColumnProviderPool g_colProvPool = null;
		public static ColumnProviderPool ColumnProviderPool
		{
			get
			{
				if(g_colProvPool == null) g_colProvPool = new ColumnProviderPool();
				return g_colProvPool;
			}
		}

		public static ResourceManager Resources
		{
			get { return KeePass.Properties.Resources.ResourceManager; }
		}

		private static bool g_bEnableTranslation = true;
		public static bool EnableTranslation
		{
			get { return g_bEnableTranslation; }
			set { g_bEnableTranslation = value; }
		}

		private static bool g_bDesignMode = true;
#if DEBUG
		private static bool g_bDesignModeQueried = false;
#endif
		public static bool DesignMode
		{
			get
			{
#if DEBUG
				g_bDesignModeQueried = true;
#endif
				return g_bDesignMode;
			}
		}

		/// <summary>
		/// Main entry point for the application.
		/// </summary>
		[STAThread]
		public static void Main(string[] args)
		{
			try { MainPriv(args); }
#if !DEBUG
			catch(Exception ex) { ShowFatal(ex); }
#endif
			finally
			{
				try { CommonTerminate(); }
				catch(Exception) { Debug.Assert(false); }

				GC.KeepAlive(g_mGlobalNotify);
			}
		}

		private static void MainPriv(string[] args)
		{
			g_bMain = true;
			g_bDesignMode = false; // Designer doesn't call Main method
#if DEBUG
			// Program.DesignMode should not be queried before executing
			// Main (e.g. by a static Control) when running the program
			// normally
			Debug.Assert(!g_bDesignModeQueried);
#endif
			InitAppContext();

			g_cmdLineArgs = new CommandLineArgs(args);

			if(g_cmdLineArgs[AppDefs.CommandLineOptions.Debug] != null)
				PwDefs.DebugMode = true;

			// Before loading the configuration
			string strWa = g_cmdLineArgs[AppDefs.CommandLineOptions.WorkaroundDisable];
			if(!string.IsNullOrEmpty(strWa))
				MonoWorkarounds.SetEnabled(strWa, false);
			strWa = g_cmdLineArgs[AppDefs.CommandLineOptions.WorkaroundEnable];
			if(!string.IsNullOrEmpty(strWa))
				MonoWorkarounds.SetEnabled(strWa, true);

			try
			{
				DpiUtil.ConfigureProcess();
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.DoEvents(); // Required
			}
			catch(Exception) { Debug.Assert(MonoWorkarounds.IsRequired(106)); }

#if DEBUG
			string strInitialWorkDir = WinUtil.GetWorkingDirectory();
#endif

			CommonInitialize();

			KdbxFile.ConfirmOpenUnknownVersion = delegate()
			{
				if(!Program.Config.UI.ShowDbOpenUnkVerDialog) return true;

				string strMsg = KPRes.DatabaseOpenUnknownVersionInfo +
					MessageService.NewParagraph + KPRes.DatabaseOpenUnknownVersionRec +
					MessageService.NewParagraph + KPRes.DatabaseOpenUnknownVersionQ;
				// No 'Do not show this dialog again' option;
				// https://sourceforge.net/p/keepass/discussion/329220/thread/096c122154/
				return MessageService.AskYesNo(strMsg, PwDefs.ShortProductName,
					false, MessageBoxIcon.Warning);
			};

			if(g_appConfig.Application.Start.PluginCacheClearOnce)
			{
				PlgxCache.Clear();
				g_appConfig.Application.Start.PluginCacheClearOnce = false;
				AppConfigSerializer.Save();
			}

			if(g_cmdLineArgs[AppDefs.CommandLineOptions.FileExtRegister] != null)
			{
				ShellUtil.RegisterExtension(AppDefs.FileExtension.FileExt,
					AppDefs.FileExtension.FileExtId, KPRes.FileExtName2,
					WinUtil.GetExecutable(), PwDefs.ShortProductName, false);
				return;
			}
			if(g_cmdLineArgs[AppDefs.CommandLineOptions.FileExtUnregister] != null)
			{
				ShellUtil.UnregisterExtension(AppDefs.FileExtension.FileExt,
					AppDefs.FileExtension.FileExtId);
				return;
			}
			if(g_cmdLineArgs[AppDefs.CommandLineOptions.PreLoad] != null)
			{
				// All important .NET assemblies are in memory now already
				try { SelfTest.Perform(); }
				catch(Exception) { Debug.Assert(false); }
				return;
			}
			/* if(g_cmdLineArgs[AppDefs.CommandLineOptions.PreLoadRegister] != null)
			{
				string strPreLoadPath = WinUtil.GetExecutable().Trim();
				if(!strPreLoadPath.StartsWith("\""))
					strPreLoadPath = "\"" + strPreLoadPath + "\"";
				ShellUtil.RegisterPreLoad(AppDefs.PreLoadName, strPreLoadPath,
					"--" + AppDefs.CommandLineOptions.PreLoad, true);
				return;
			}
			if(g_cmdLineArgs[AppDefs.CommandLineOptions.PreLoadUnregister] != null)
			{
				ShellUtil.RegisterPreLoad(AppDefs.PreLoadName, string.Empty,
					string.Empty, false);
				return;
			} */
			if((g_cmdLineArgs[AppDefs.CommandLineOptions.Help] != null) ||
				(g_cmdLineArgs[AppDefs.CommandLineOptions.HelpLong] != null))
			{
				AppHelp.ShowHelp(AppDefs.HelpTopics.CommandLine, null);
				return;
			}
			if(g_cmdLineArgs[AppDefs.CommandLineOptions.ConfigSetUrlOverride] != null)
			{
				Program.Config.Integration.UrlOverride = g_cmdLineArgs[
					AppDefs.CommandLineOptions.ConfigSetUrlOverride];
				Program.Config.Integration.UrlOverrideEnabled = true;
				EnforceAndSave(AceSections.UrlOverride);
				return;
			}
			if(g_cmdLineArgs[AppDefs.CommandLineOptions.ConfigClearUrlOverride] != null)
			{
				Program.Config.Integration.UrlOverride = string.Empty;
				Program.Config.Integration.UrlOverrideEnabled = false;
				EnforceAndSave(AceSections.UrlOverride);
				return;
			}
			if(g_cmdLineArgs[AppDefs.CommandLineOptions.ConfigGetUrlOverride] != null)
			{
				try
				{
					string strFileOut = UrlUtil.EnsureTerminatingSeparator(
						UrlUtil.GetTempPath(), false) + "KeePass_UrlOverride.tmp";
					string strContent = ("[KeePass]\r\nKeeURLOverride=" +
						Program.Config.Integration.UrlOverride + "\r\n");
					File.WriteAllText(strFileOut, strContent);
				}
				catch(Exception) { Debug.Assert(false); }
				return;
			}
			if(g_cmdLineArgs[AppDefs.CommandLineOptions.ConfigAddUrlOverride] != null)
			{
				bool bAct = (g_cmdLineArgs[AppDefs.CommandLineOptions.Activate] != null);
				Program.Config.Integration.UrlSchemeOverrides.AddCustomOverride(
					g_cmdLineArgs[AppDefs.CommandLineOptions.Scheme],
					g_cmdLineArgs[AppDefs.CommandLineOptions.Value], bAct, bAct);
				EnforceAndSave(AceSections.UrlSchemeOverrides);
				return;
			}
			if(g_cmdLineArgs[AppDefs.CommandLineOptions.ConfigRemoveUrlOverride] != null)
			{
				Program.Config.Integration.UrlSchemeOverrides.RemoveCustomOverride(
					g_cmdLineArgs[AppDefs.CommandLineOptions.Scheme],
					g_cmdLineArgs[AppDefs.CommandLineOptions.Value]);
				EnforceAndSave(AceSections.UrlSchemeOverrides);
				return;
			}
			if(g_cmdLineArgs[AppDefs.CommandLineOptions.ConfigSetLanguageFile] != null)
			{
				Program.Config.Application.LanguageFile = g_cmdLineArgs[
					AppDefs.CommandLineOptions.ConfigSetLanguageFile];
				AppConfigSerializer.Save();
				return;
			}
			if(AppEnforcedConfig.SetupAsChild()) return;
			if(KeyUtil.KdfPrcTestAsChild()) return;
			if(g_cmdLineArgs[AppDefs.CommandLineOptions.PlgxCreate] != null)
			{
				PlgxPlugin.CreateFromCommandLine();
				return;
			}
			if(g_cmdLineArgs[AppDefs.CommandLineOptions.PlgxCreateInfo] != null)
			{
				PlgxPlugin.CreateInfoFile(g_cmdLineArgs.FileName);
				return;
			}
			if(g_cmdLineArgs[AppDefs.CommandLineOptions.ShowAssemblyInfo] != null)
			{
				MessageService.ShowInfo(Assembly.GetExecutingAssembly().ToString());
				return;
			}
			if(g_cmdLineArgs[AppDefs.CommandLineOptions.MakeXmlSerializerEx] != null)
			{
				XmlSerializerEx.GenerateSerializers(g_cmdLineArgs);
				return;
			}
			if(g_cmdLineArgs[AppDefs.CommandLineOptions.MakeXspFile] != null)
			{
				XspArchive.CreateFile(g_cmdLineArgs.FileName, g_cmdLineArgs["d"]);
				return;
			}
			if(g_cmdLineArgs[AppDefs.CommandLineOptions.Version] != null)
			{
				Console.WriteLine(PwDefs.ShortProductName + " " + PwDefs.VersionString);
				Console.WriteLine(PwDefs.Copyright);
				return;
			}
#if DEBUG
			if(g_cmdLineArgs[AppDefs.CommandLineOptions.TestGfx] != null)
			{
				List<Image> lImg = new List<Image>
				{
					Properties.Resources.B16x16_Browser,
					Properties.Resources.B48x48_Keyboard_Layout
				};
				ImageArchive aHighRes = new ImageArchive();
				aHighRes.Load(Properties.Resources.Images_Client_HighRes);
				lImg.Add(aHighRes.GetForObject("C12_IRKickFlash"));
				if(File.Exists("Test.png"))
					lImg.Add(Image.FromFile("Test.png"));
				Image img = GfxUtil.ScaleTest(lImg.ToArray());
				img.Save("GfxScaleTest.png", ImageFormat.Png);
				return;
			}
#endif
			// #if (DEBUG && !KeePassLibSD)
			// if(g_cmdLineArgs[AppDefs.CommandLineOptions.MakePopularPasswordTable] != null)
			// {
			//	PopularPasswords.MakeList();
			//	return;
			// }
			// #endif

			try { g_nAppMessage = NativeMethods.RegisterWindowMessage(StrWndMsgID); }
			catch(Exception) { Debug.Assert(NativeLib.IsUnix()); }

			if(g_cmdLineArgs[AppDefs.CommandLineOptions.ExitAll] != null)
			{
				BroadcastAppMessage(AppMessage.Exit);
				return;
			}
			if(g_cmdLineArgs[AppDefs.CommandLineOptions.AutoType] != null)
			{
				BroadcastAppMessage(AppMessage.AutoType);
				return;
			}
			if(g_cmdLineArgs[AppDefs.CommandLineOptions.AutoTypePassword] != null)
			{
				BroadcastAppMessage(AppMessage.AutoTypePassword);
				return;
			}
			if(g_cmdLineArgs[AppDefs.CommandLineOptions.AutoTypeSelected] != null)
			{
				BroadcastAppMessage(AppMessage.AutoTypeSelected);
				return;
			}
			if(g_cmdLineArgs[AppDefs.CommandLineOptions.OpenEntryUrl] != null)
			{
				string strEntryUuid = g_cmdLineArgs[AppDefs.CommandLineOptions.Uuid];
				if(!string.IsNullOrEmpty(strEntryUuid))
				{
					IpcParamEx ipUrl = new IpcParamEx(IpcUtilEx.CmdOpenEntryUrl,
						strEntryUuid, null, null, null, null);
					IpcUtilEx.SendGlobalMessage(ipUrl, false);
				}
				return;
			}
			if(g_cmdLineArgs[AppDefs.CommandLineOptions.LockAll] != null)
			{
				BroadcastAppMessage(AppMessage.Lock);
				return;
			}
			if(g_cmdLineArgs[AppDefs.CommandLineOptions.UnlockAll] != null)
			{
				BroadcastAppMessage(AppMessage.Unlock);
				return;
			}
			if(g_cmdLineArgs[AppDefs.CommandLineOptions.Cancel] != null)
			{
				BroadcastAppMessage(AppMessage.Cancel);
				return;
			}

			string strIpc = g_cmdLineArgs[AppDefs.CommandLineOptions.IpcEvent];
			string strIpc1 = g_cmdLineArgs[AppDefs.CommandLineOptions.IpcEvent1];
			if((strIpc != null) || (strIpc1 != null))
			{
				bool bIpc1 = (strIpc1 != null);
				string strName = (bIpc1 ? strIpc1 : strIpc);
				if(strName.Length != 0)
				{
					string[] vFlt = KeyUtil.MakeCtxIndependent(args);

					IpcParamEx ipcP = new IpcParamEx(IpcUtilEx.CmdIpcEvent, strName,
						CommandLineArgs.SafeSerialize(vFlt), null, null, null);
					IpcUtilEx.SendGlobalMessage(ipcP, bIpc1);
				}
				return;
			}

			// Mutex mSingleLock = TrySingleInstanceLock(AppDefs.MutexName, true);
			bool bSingleLock = GlobalMutexPool.CreateMutex(AppDefs.MutexName, true);
			// if((mSingleLock == null) && g_appConfig.Integration.LimitToSingleInstance)
			if(!bSingleLock && g_appConfig.Integration.LimitToSingleInstance)
			{
				ActivatePreviousInstance(args);
				return;
			}

			g_mGlobalNotify = TryGlobalInstanceNotify(AppDefs.MutexNameGlobal);

			AutoType.InitStatic();

			CustomMessageFilterEx cmf = new CustomMessageFilterEx();
			Application.AddMessageFilter(cmf);

			try
			{
				if(g_cmdLineArgs[AppDefs.CommandLineOptions.DebugThrowException] != null)
					throw new Exception(AppDefs.CommandLineOptions.DebugThrowException);

				g_formMain = new MainForm();
				Application.Run(g_formMain);
			}
			finally { Application.RemoveMessageFilter(cmf); }

#if DEBUG
			string strEndWorkDir = WinUtil.GetWorkingDirectory();
			Debug.Assert(strEndWorkDir.Equals(strInitialWorkDir, StrUtil.CaseIgnoreCmp));
#endif

			// GC.KeepAlive(mSingleLock);
		}

		/// <summary>
		/// Common initialization method that can also be used by applications
		/// that use KeePass as a library.
		/// <c>CommonInitialize</c> may throw an exception, whereas
		/// <c>CommonInit</c> shows any error and returns a <c>bool</c>.
		/// <c>CommonInitialize</c> allows a custom error handling/reporting.
		/// <c>CommonInitialize</c> (or <c>CommonInit</c>) and
		/// <c>CommonTerminate</c> should be called exactly once.
		/// </summary>
		public static bool CommonInit()
		{
			try { CommonInitialize(); return true; }
			catch(Exception ex) { ShowFatal(ex); }
			return false;
		}

		/// <summary>
		/// Common initialization method that can also be used by applications
		/// that use KeePass as a library.
		/// <c>CommonInitialize</c> may throw an exception, whereas
		/// <c>CommonInit</c> shows any error and returns a <c>bool</c>.
		/// <c>CommonInitialize</c> allows a custom error handling/reporting.
		/// <c>CommonInitialize</c> (or <c>CommonInit</c>) and
		/// <c>CommonTerminate</c> should be called exactly once.
		/// </summary>
		public static void CommonInitialize()
		{
			if(!g_bMain)
			{
				// Again, for the apps that are not calling Main
				g_bDesignMode = false;
				InitAppContext();
			}

			InitEnvSecurity();
			// InitEnvWorkarounds();
			MonoWorkarounds.Initialize();

			// Do not run as AppX, because of compatibility problems
			if(WinUtil.IsAppX) throw new PlatformNotSupportedException();

			SelfTest.TestFipsComplianceProblems();

			// Set global localized strings
			PwDatabase.LocalizedAppName = PwDefs.ShortProductName;
			KdbxFile.DetermineLanguageId();

			Debug.Assert(g_appConfig == null);
			g_appConfig = AppConfigSerializer.Load();

			if(g_appConfig.Logging.Enabled)
				AppLogEx.Open(PwDefs.ShortProductName);

			AppPolicy.Current = g_appConfig.Security.Policy.CloneDeep();
			AppPolicy.ApplyToConfig();

			if(g_appConfig.Security.ProtectProcessWithDacl)
				KeePassLib.Native.NativeMethods.ProtectProcessWithDacl();

			g_appConfig.Apply(AceApplyFlags.All);

			Program.TriggerSystem.SetToInitialState();

			LoadTranslation();
			CustomResourceManager.Override(typeof(KeePass.Properties.Resources));

			AppConfigSerializer.CreateBackupIfNecessary();

			AceSections s = AppConfigEx.GetEnabledNonEnforcedSections();
			if((s != AceSections.None) && !g_appConfig.Meta.PreferUserConfiguration)
			{
				if(AppConfigEx.EnforceSections(s, g_appConfig, false, false, null, null))
					s = AceSections.None;
			}
			AppConfigEx.DisableSections(s);

#if KP_DEVSNAP
			if(!g_bAsmResReg)
			{
				AppDomain.CurrentDomain.AssemblyResolve += Program.AssemblyResolve;
				g_bAsmResReg = true;
			}
			else { Debug.Assert(false); }
#endif
		}

		/// <summary>
		/// Common termination method that can also be used by applications
		/// that use KeePass as a library.
		/// <c>CommonInitialize</c> (or <c>CommonInit</c>) and
		/// <c>CommonTerminate</c> should be called exactly once.
		/// </summary>
		public static void CommonTerminate()
		{
			Debug.Assert(GlobalWindowManager.WindowCount == 0);
			Debug.Assert(MessageService.CurrentMessageCount == 0);
			Debug.Assert(!SendInputEx.IsSending);
#if DEBUG
			Debug.Assert(ShutdownBlocker.PrimaryInstance == null);
#endif

			IpcBroadcast.StopServer();
			EntryMenu.Destroy();
			GlobalMutexPool.ReleaseAll();

			AppLogEx.Close();

			if(g_tempFilesPool != null)
			{
				g_tempFilesPool.Clear(TempClearFlags.All);
				g_tempFilesPool.WaitForThreads();
			}

			EnableThemingInScope.StaticDispose();
			MonoWorkarounds.Terminate();

#if KP_DEVSNAP
			if(g_bAsmResReg)
			{
				AppDomain.CurrentDomain.AssemblyResolve -= Program.AssemblyResolve;
				g_bAsmResReg = false;
			}
			else { Debug.Assert(false); }
#endif
		}

		private static void EnforceAndSave(AceSections s)
		{
			if(AppConfigEx.EnforceSections(s, Program.Config, false, true, null, null))
				AppConfigSerializer.Save();
		}

		private static void ShowFatal(Exception ex)
		{
			if(ex == null) { Debug.Assert(false); return; }

			// Catch message box exception;
			// https://sourceforge.net/p/keepass/patches/86/
			try { MessageService.ShowFatal(ex); }
			catch(Exception) { Console.Error.WriteLine(ex.ToString()); }
		}

		private static void InitEnvSecurity()
		{
			try
			{
				// Do not load libraries from the current working directory
				if(!NativeMethods.SetDllDirectory(string.Empty)) { Debug.Assert(false); }
			}
			catch(Exception) { Debug.Assert(NativeLib.IsUnix()); }

			try
			{
				if(NativeMethods.WerAddExcludedApplication(
					AppDefs.FileNames.Program, false) < 0)
				{
					Debug.Assert(false);
				}
			}
			catch(Exception) { Debug.Assert(NativeLib.IsUnix() || !WinUtil.IsAtLeastWindowsVista); }
		}

		private static void InitAppContext()
		{
			try
			{
				Type t = typeof(string).Assembly.GetType("System.AppContext", false);
				if(t == null) return; // Available in .NET >= 4.6

				MethodInfo mi = t.GetMethod("SetSwitch", BindingFlags.Public |
					BindingFlags.Static);
				if(mi == null) { Debug.Assert(false); return; }

				GAction<string, bool> f = delegate(string strSwitch, bool bValue)
				{
					mi.Invoke(null, new object[] { strSwitch, bValue });
				};

				f("Switch.System.Drawing.DontSupportPngFramesInIcons", false); // 4.6
				f("Switch.System.Drawing.Printing.OptimizePrintPreview", true); // 4.6, optional
				f("Switch.System.IO.BlockLongPaths", false); // 4.6.2
				f("Switch.System.IO.Compression.DoNotUseNativeZipLibraryForDecompression", false); // 4.7.2
				f("Switch.System.IO.Compression.ZipFile.UseBackslash", false); // 4.6.1
				f("Switch.System.IO.UseLegacyPathHandling", false); // 4.6.2
				f("Switch.System.Net.DontEnableSchUseStrongCrypto", false); // 4.6
				f("Switch.System.Net.DontEnableSystemDefaultTlsVersions", false); // 4.7
				f("Switch.System.Security.Cryptography.AesCryptoServiceProvider.DontCorrectlyResetDecryptor", false); // 4.6.2
				f("Switch.System.Security.Cryptography.UseLegacyFipsThrow", false); // 4.8
				f("Switch.System.Windows.Forms.DoNotLoadLatestRichEditControl", false); // 4.7
				f("Switch.System.Windows.Forms.DoNotSupportSelectAllShortcutInMultilineTextBox", false); // 4.6.1
				f("Switch.System.Windows.Forms.DontSupportReentrantFilterMessage", false); // 4.6.1
				f("Switch.System.Windows.Forms.EnableVisualStyleValidation", false); // 4.8
				// f("Switch.System.Windows.Forms.UseLegacyToolTipDisplay", false); // 4.8, optional
				f("Switch.UseLegacyAccessibilityFeatures", false); // 4.7.1
				f("Switch.UseLegacyAccessibilityFeatures.2", false); // 4.7.2
				f("Switch.UseLegacyAccessibilityFeatures.3", false); // 4.8
				f("Switch.UseLegacyAccessibilityFeatures.4", false); // 4.8 upd.

#if DEBUG
				if(NativeLib.IsUnix()) return;

				// Check that the internal classes do not cache other values already

				const BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic |
					BindingFlags.Static;

				GAction<Type, string, bool> fCheck = delegate(Type tClass,
					string strProperty, bool bValue)
				{
					PropertyInfo pi = tClass.GetProperty(strProperty, bf);
					string strFullName = tClass.FullName + "." + strProperty;
					if(pi == null) { Debug.Assert(false, strFullName + " not found!"); return; }
					Debug.Assert(((bool)pi.GetValue(null, null) == bValue),
						strFullName + " returned an unexpected value!");
				};

				Type tM = typeof(string).Assembly.GetType( // mscorlib
					"System.AppContextSwitches", false);
				if(tM == null) { Debug.Assert(false); return; }

				Type tS = typeof(GZipStream).Assembly.GetType( // System
					"System.LocalAppContextSwitches", false);
				if(tS == null) { Debug.Assert(false); return; }

				Type tD = typeof(Image).Assembly.GetType(
					"System.Drawing.LocalAppContextSwitches", false);
				if(tD == null) { Debug.Assert(false); return; }

				Type tW = typeof(ListViewItem).Assembly.GetType(
					"System.Windows.Forms.LocalAppContextSwitches", false);
				if(tW == null) { Debug.Assert(false); return; }

				Type tA = typeof(ListViewItem).Assembly.GetType(
					"System.AccessibilityImprovements", false);
				if(tA == null) { Debug.Assert(false); return; }

				fCheck(tD, "DontSupportPngFramesInIcons", false);
				fCheck(tD, "OptimizePrintPreview", true);
				fCheck(tM, "BlockLongPaths", false);
				fCheck(tS, "DoNotUseNativeZipLibraryForDecompression", false);
				fCheck(tM, "UseLegacyPathHandling", false);
				fCheck(tS, "DontEnableSchUseStrongCrypto", false);
				fCheck(tS, "DontEnableSystemDefaultTlsVersions", false);
				fCheck(tM, "UseLegacyFipsThrow", false);
				fCheck(tW, "DoNotLoadLatestRichEditControl", false);
				fCheck(tW, "DoNotSupportSelectAllShortcutInMultilineTextBox", false);
				fCheck(tW, "DontSupportReentrantFilterMessage", false);
				fCheck(tW, "EnableVisualStyleValidation", false);
				fCheck(tA, "Level4", true);
#endif
			}
			catch(Exception) { Debug.Assert(false); }
		}

		// private static Mutex TrySingleInstanceLock(string strName, bool bInitiallyOwned)
		// {
		//	if(strName == null) throw new ArgumentNullException("strName");
		//	try
		//	{
		//		bool bCreatedNew;
		//		Mutex mSingleLock = new Mutex(bInitiallyOwned, strName, out bCreatedNew);
		//		if(!bCreatedNew) return null;
		//		return mSingleLock;
		//	}
		//	catch(Exception) { }
		//	return null;
		// }

		private static Mutex TryGlobalInstanceNotify(string strBaseName)
		{
			if(strBaseName == null) throw new ArgumentNullException("strBaseName");

			try
			{
				string strName = "Global\\" + strBaseName;
				string strIdentity = Environment.UserDomainName + "\\" +
					Environment.UserName;
				MutexSecurity ms = new MutexSecurity();

				MutexAccessRule mar = new MutexAccessRule(strIdentity,
					MutexRights.FullControl, AccessControlType.Allow);
				ms.AddAccessRule(mar);

				SecurityIdentifier sid = new SecurityIdentifier(
					WellKnownSidType.WorldSid, null);
				mar = new MutexAccessRule(sid, MutexRights.ReadPermissions |
					MutexRights.Synchronize, AccessControlType.Allow);
				ms.AddAccessRule(mar);

				bool bCreatedNew;
				return new Mutex(false, strName, out bCreatedNew, ms);
			}
			catch(Exception) { } // Windows 9x and Mono 2.0+ (AddAccessRule) throw

			return null;
		}

		// private static void DestroyMutex(Mutex m, bool bReleaseFirst)
		// {
		//	if(m == null) return;
		//	if(bReleaseFirst)
		//	{
		//		try { m.ReleaseMutex(); }
		//		catch(Exception) { Debug.Assert(false); }
		//	}
		//	try { m.Close(); }
		//	catch(Exception) { Debug.Assert(false); }
		// }

		private static void ActivatePreviousInstance(string[] args)
		{
			if((g_nAppMessage == 0) && !NativeLib.IsUnix())
			{
				Debug.Assert(false);
				return;
			}

			try
			{
				if(string.IsNullOrEmpty(g_cmdLineArgs.FileName))
				{
					// NativeMethods.PostMessage((IntPtr)NativeMethods.HWND_BROADCAST,
					//	g_nAppMessage, (IntPtr)AppMessage.RestoreWindow, IntPtr.Zero);
					IpcBroadcast.Send(AppMessage.RestoreWindow, 0, false);
				}
				else
				{
					string[] vFlt = KeyUtil.MakeCtxIndependent(args);

					IpcParamEx ipcMsg = new IpcParamEx(IpcUtilEx.CmdOpenDatabase,
						CommandLineArgs.SafeSerialize(vFlt), null, null, null, null);
					IpcUtilEx.SendGlobalMessage(ipcMsg, true);
				}
			}
			catch(Exception) { Debug.Assert(false); }
		}

		// For plugins
		public static void NotifyUserActivity()
		{
			MainForm mf = g_formMain;
			if(mf != null) mf.NotifyUserActivity();
		}

		public static IntPtr GetSafeMainWindowHandle()
		{
			try
			{
				MainForm mf = g_formMain;
				if(mf != null) return mf.Handle;
			}
			catch(Exception) { Debug.Assert(false); }

			return IntPtr.Zero;
		}

		private static void BroadcastAppMessage(AppMessage msg)
		{
			try
			{
				// NativeMethods.PostMessage((IntPtr)NativeMethods.HWND_BROADCAST,
				//	g_nAppMessage, (IntPtr)msg, IntPtr.Zero);
				IpcBroadcast.Send(msg, 0, false);
			}
			catch(Exception) { Debug.Assert(false); }
		}

		private static void LoadTranslation()
		{
			Debug.Assert(g_kpTranslation == null);
			if(!g_bEnableTranslation) return;

			string strPath = g_appConfig.Application.GetLanguageFilePath();
			if(string.IsNullOrEmpty(strPath)) return;

			try
			{
				// Performance optimization
				if(!File.Exists(strPath)) return;

				XmlSerializerEx xs = new XmlSerializerEx(typeof(KPTranslation));
				g_kpTranslation = KPTranslation.Load(strPath, xs);

				KPRes.SetTranslatedStrings(
					g_kpTranslation.SafeGetStringTableDictionary(
					"KeePass.Resources.KPRes"));
				KLRes.SetTranslatedStrings(
					g_kpTranslation.SafeGetStringTableDictionary(
					"KeePassLib.Resources.KLRes"));

				StrUtil.RightToLeft = g_kpTranslation.Properties.RightToLeft;
			}
			// catch(DirectoryNotFoundException) { } // Ignore
			// catch(FileNotFoundException) { } // Ignore
			catch(Exception) { Debug.Assert(false); }
		}

		internal static bool IsStableAssembly()
		{
			try
			{
				Assembly asm = typeof(Program).Assembly;
				byte[] pk = asm.GetName().GetPublicKeyToken();
				string strPk = MemUtil.ByteArrayToHexString(pk);
				Debug.Assert(string.IsNullOrEmpty(strPk) || (strPk.Length == 16));
				return string.Equals(strPk, "fed2ed7716aecf5c", StrUtil.CaseIgnoreCmp);
			}
			catch(Exception) { Debug.Assert(false); }

			return false;
		}

		internal static bool IsDevelopmentSnapshot()
		{
#if KP_DEVSNAP
			return true;
#else
			return !IsStableAssembly();
#endif
		}

		/* private static void InitEnvWorkarounds()
		{
			InitFtpWorkaround();
		} */

		/* private static void InitFtpWorkaround()
		{
			// https://support.microsoft.com/kb/2134299
			// https://connect.microsoft.com/VisualStudio/feedback/details/621450/problem-renaming-file-on-ftp-server-using-ftpwebrequest-in-net-framework-4-0-vs2010-only
			try
			{
				if((Environment.Version.Major >= 4) && !NativeLib.IsUnix())
				{
					Type tFtp = typeof(FtpWebRequest);

					Assembly asm = Assembly.GetAssembly(tFtp);
					Type tFlags = asm.GetType("System.Net.FtpMethodFlags");
					Debug.Assert(Enum.GetUnderlyingType(tFlags) == typeof(int));
					int iAdd = (int)Enum.Parse(tFlags, "MustChangeWorkingDirectoryToPath");
					Debug.Assert(iAdd == 0x100);

					FieldInfo fiMethod = tFtp.GetField("m_MethodInfo",
						BindingFlags.Instance | BindingFlags.NonPublic);
					if(fiMethod == null) { Debug.Assert(false); return; }
					Type tMethod = fiMethod.FieldType;

					FieldInfo fiKnown = tMethod.GetField("KnownMethodInfo",
						BindingFlags.Static | BindingFlags.NonPublic);
					if(fiKnown == null) { Debug.Assert(false); return; }
					Array arKnown = (Array)fiKnown.GetValue(null);

					FieldInfo fiFlags = tMethod.GetField("Flags",
						BindingFlags.Instance | BindingFlags.NonPublic);
					if(fiFlags == null) { Debug.Assert(false); return; }

					foreach(object oKnown in arKnown)
					{
						int i = (int)fiFlags.GetValue(oKnown);
						i |= iAdd;
						fiFlags.SetValue(oKnown, i);
					}
				}
			}
			catch(Exception) { Debug.Assert(false); }
		} */

#if KP_DEVSNAP
		private static Assembly AssemblyResolve(object sender, ResolveEventArgs e)
		{
			string str = ((e != null) ? e.Name : null);
			if(string.IsNullOrEmpty(str)) { Debug.Assert(false); return null; }

			try
			{
				AssemblyName n = new AssemblyName(str);
				if(string.Equals(n.Name, "KeePass", StrUtil.CaseIgnoreCmp))
					return typeof(KeePass.Program).Assembly;
			}
			catch(Exception)
			{
				Debug.Assert(false);

				if(str.Equals("KeePass", StrUtil.CaseIgnoreCmp) ||
					str.StartsWith("KeePass,", StrUtil.CaseIgnoreCmp))
					return typeof(KeePass.Program).Assembly;
			}

			Debug.Assert(false);
			return null;
		}
#endif

#if DEBUG
		private static readonly Stack<TraceListener[]> g_sTraceListeners =
			new Stack<TraceListener[]>();
#endif
		[Conditional("DEBUG")]
		internal static void EnableAssertions(bool bEnable)
		{
#if DEBUG
			if(bEnable)
			{
				Debug.Listeners.Clear();
				Debug.Listeners.AddRange(g_sTraceListeners.Pop());
			}
			else
			{
				TraceListener[] v = new TraceListener[Debug.Listeners.Count];
				Debug.Listeners.CopyTo(v, 0);
				g_sTraceListeners.Push(v);
				Debug.Listeners.Clear();
			}
#endif
		}

		internal static void CheckExeConfig()
		{
			string strPath = WinUtil.GetExecutable() + ".config";

			try
			{
				GAction<bool, bool> fAssert = delegate(bool bCondition, bool bInvalidData)
				{
					if(!bCondition)
					{
						if(bInvalidData) throw new InvalidDataException();
						throw new Exception(KLRes.FileCorrupted);
					}
				};
				GAction<object> fAssertEx = delegate(object o)
				{
					fAssert((o != null), false);
				};

				bool bDev = IsDevelopmentSnapshot();
				if(bDev && !File.Exists(strPath)) return;

				XmlDocument xd = XmlUtilEx.LoadXmlDocument(strPath, StrUtil.Utf8);

				XmlNamespaceManager nm = new XmlNamespaceManager(xd.NameTable);
				const string strAsm1P = "asm1";
				const string strAsm1U = "urn:schemas-microsoft-com:asm.v1";
				string strU = nm.LookupNamespace(strAsm1P);
				if(strU == null) nm.AddNamespace(strAsm1P, strAsm1U);
				else fAssert((strU == strAsm1U), true);

				fAssertEx(xd.SelectSingleNode(
					"/configuration/startup/supportedRuntime[@version = \"v4.0\"]"));

				if(!bDev)
				{
					ulong u = StrUtil.ParseVersion(typeof(
						Program).Assembly.GetName().Version.ToString());
					// string strOld = "2.0.9.0-" + StrUtil.VersionToString(
					//	u & 0xFFFFFFFFFFFF0000UL, 4);
					string strNew = StrUtil.VersionToString(u, 4);

					XmlNode xn = xd.SelectSingleNode("/configuration/runtime/" +
						strAsm1P + ":assemblyBinding/" +
						strAsm1P + ":dependentAssembly[" +
						strAsm1P + ":assemblyIdentity/@name = \"KeePass\"]/" +
						strAsm1P + ":bindingRedirect", nm);
					fAssertEx(xn);

					// XmlAttribute xa = xn.Attributes["oldVersion"];
					// fAssert(((xa != null) && (xa.Value == strOld)), true);

					XmlAttribute xa = xn.Attributes["newVersion"];
					fAssert(((xa != null) && (xa.Value == strNew)), true);
				}
			}
			catch(Exception ex)
			{
				MessageService.ShowWarning(strPath, ex, KPRes.FixByReinstall);
			}
		}
	}
}
