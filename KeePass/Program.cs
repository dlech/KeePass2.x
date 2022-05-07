/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2022 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Resources;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

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
using KeePassLib.Cryptography.Cipher;
using KeePassLib.Cryptography.PasswordGenerator;
using KeePassLib.Delegates;
using KeePassLib.Keys;
using KeePassLib.Resources;
using KeePassLib.Security;
using KeePassLib.Serialization;
using KeePassLib.Translation;
using KeePassLib.Utility;

using NativeLib = KeePassLib.Native.NativeLib;

namespace KeePass
{
	public static class Program
	{
		private const string m_strWndMsgID = "EB2FE38E1A6A4A138CF561442F1CF25A";

		private static CommandLineArgs m_cmdLineArgs = null;
		private static Random m_rndGlobal = null;
		private static int m_nAppMessage = 0;
		private static MainForm m_formMain = null;
		private static AppConfigEx m_appConfig = null;
		private static KeyProviderPool m_keyProviderPool = null;
		private static KeyValidatorPool m_keyValidatorPool = null;
		private static FileFormatPool m_fmtPool = null;
		private static KPTranslation m_kpTranslation = new KPTranslation();
		private static TempFilesPool m_tempFilesPool = null;
		private static EcasPool m_ecasPool = null;
		private static EcasTriggerSystem m_ecasTriggers = null;
		private static CustomPwGeneratorPool m_pwGenPool = null;
		private static ColumnProviderPool m_colProvPool = null;

		private static bool m_bDesignMode = true;
#if DEBUG
		private static bool m_bDesignModeQueried = false;
#endif
		private static bool m_bEnableTranslation = true;

#if KP_DEVSNAP
		private static bool m_bAsmResReg = false;
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

		public static CommandLineArgs CommandLineArgs
		{
			get
			{
				if(m_cmdLineArgs == null) // No assert (KeePass as library)
					m_cmdLineArgs = new CommandLineArgs(null);

				return m_cmdLineArgs;
			}
		}

		public static Random GlobalRandom
		{
			get { return m_rndGlobal; }
		}

		public static int ApplicationMessage
		{
			get { return m_nAppMessage; }
		}

		public static MainForm MainForm
		{
			get { return m_formMain; }
		}

		public static AppConfigEx Config
		{
			get
			{
				if(m_appConfig == null) m_appConfig = new AppConfigEx();
				return m_appConfig;
			}
		}

		public static KeyProviderPool KeyProviderPool
		{
			get
			{
				if(m_keyProviderPool == null) m_keyProviderPool = new KeyProviderPool();
				return m_keyProviderPool;
			}
		}

		public static KeyValidatorPool KeyValidatorPool
		{
			get
			{
				if(m_keyValidatorPool == null) m_keyValidatorPool = new KeyValidatorPool();
				return m_keyValidatorPool;
			}
		}

		public static FileFormatPool FileFormatPool
		{
			get
			{
				if(m_fmtPool == null) m_fmtPool = new FileFormatPool();
				return m_fmtPool;
			}
		}

		public static KPTranslation Translation
		{
			get { return m_kpTranslation; }
		}

		public static TempFilesPool TempFilesPool
		{
			get
			{
				if(m_tempFilesPool == null) m_tempFilesPool = new TempFilesPool();
				return m_tempFilesPool;
			}
		}

		public static EcasPool EcasPool // Construct on first access
		{
			get
			{
				if(m_ecasPool == null) m_ecasPool = new EcasPool(true);
				return m_ecasPool;
			}
		}

		public static EcasTriggerSystem TriggerSystem
		{
			get
			{
				if(m_ecasTriggers == null) m_ecasTriggers = new EcasTriggerSystem();
				return m_ecasTriggers;
			}
		}

		public static CustomPwGeneratorPool PwGeneratorPool
		{
			get
			{
				if(m_pwGenPool == null) m_pwGenPool = new CustomPwGeneratorPool();
				return m_pwGenPool;
			}
		}

		public static ColumnProviderPool ColumnProviderPool
		{
			get
			{
				if(m_colProvPool == null) m_colProvPool = new ColumnProviderPool();
				return m_colProvPool;
			}
		}

		public static ResourceManager Resources
		{
			get { return KeePass.Properties.Resources.ResourceManager; }
		}

		public static bool DesignMode
		{
			get
			{
#if DEBUG
				m_bDesignModeQueried = true;
#endif
				return m_bDesignMode;
			}
		}

		public static bool EnableTranslation
		{
			get { return m_bEnableTranslation; }
			set { m_bEnableTranslation = value; }
		}

		/// <summary>
		/// Main entry point for the application.
		/// </summary>
		[STAThread]
		public static void Main(string[] args)
		{
#if DEBUG
			MainPriv(args);
#else
			try { MainPriv(args); }
			catch(Exception ex) { ShowFatal(ex); }
#endif
		}

		private static void MainPriv(string[] args)
		{
#if DEBUG
			// Program.DesignMode should not be queried before executing
			// Main (e.g. by a static Control) when running the program
			// normally
			Debug.Assert(!m_bDesignModeQueried);
#endif
			m_bDesignMode = false; // Designer doesn't call Main method

			m_cmdLineArgs = new CommandLineArgs(args);

			// Before loading the configuration
			string strWa = m_cmdLineArgs[AppDefs.CommandLineOptions.WorkaroundDisable];
			if(!string.IsNullOrEmpty(strWa))
				MonoWorkarounds.SetEnabled(strWa, false);
			strWa = m_cmdLineArgs[AppDefs.CommandLineOptions.WorkaroundEnable];
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

			if(!CommonInit()) { CommonTerminate(); return; }

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

			if(m_appConfig.Application.Start.PluginCacheClearOnce)
			{
				PlgxCache.Clear();
				m_appConfig.Application.Start.PluginCacheClearOnce = false;
				AppConfigSerializer.Save(Program.Config);
			}

			if(m_cmdLineArgs[AppDefs.CommandLineOptions.FileExtRegister] != null)
			{
				ShellUtil.RegisterExtension(AppDefs.FileExtension.FileExt,
					AppDefs.FileExtension.FileExtId, KPRes.FileExtName2,
					WinUtil.GetExecutable(), PwDefs.ShortProductName, false);
				MainCleanUp();
				return;
			}
			if(m_cmdLineArgs[AppDefs.CommandLineOptions.FileExtUnregister] != null)
			{
				ShellUtil.UnregisterExtension(AppDefs.FileExtension.FileExt,
					AppDefs.FileExtension.FileExtId);
				MainCleanUp();
				return;
			}
			if(m_cmdLineArgs[AppDefs.CommandLineOptions.PreLoad] != null)
			{
				// All important .NET assemblies are in memory now already
				try { SelfTest.Perform(); }
				catch(Exception) { Debug.Assert(false); }
				MainCleanUp();
				return;
			}
			/* if(m_cmdLineArgs[AppDefs.CommandLineOptions.PreLoadRegister] != null)
			{
				string strPreLoadPath = WinUtil.GetExecutable().Trim();
				if(!strPreLoadPath.StartsWith("\""))
					strPreLoadPath = "\"" + strPreLoadPath + "\"";
				ShellUtil.RegisterPreLoad(AppDefs.PreLoadName, strPreLoadPath,
					"--" + AppDefs.CommandLineOptions.PreLoad, true);
				MainCleanUp();
				return;
			}
			if(m_cmdLineArgs[AppDefs.CommandLineOptions.PreLoadUnregister] != null)
			{
				ShellUtil.RegisterPreLoad(AppDefs.PreLoadName, string.Empty,
					string.Empty, false);
				MainCleanUp();
				return;
			} */
			if((m_cmdLineArgs[AppDefs.CommandLineOptions.Help] != null) ||
				(m_cmdLineArgs[AppDefs.CommandLineOptions.HelpLong] != null))
			{
				AppHelp.ShowHelp(AppDefs.HelpTopics.CommandLine, null);
				MainCleanUp();
				return;
			}
			if(m_cmdLineArgs[AppDefs.CommandLineOptions.ConfigSetUrlOverride] != null)
			{
				Program.Config.Integration.UrlOverride = m_cmdLineArgs[
					AppDefs.CommandLineOptions.ConfigSetUrlOverride];
				AppConfigSerializer.Save(Program.Config);
				MainCleanUp();
				return;
			}
			if(m_cmdLineArgs[AppDefs.CommandLineOptions.ConfigClearUrlOverride] != null)
			{
				Program.Config.Integration.UrlOverride = string.Empty;
				AppConfigSerializer.Save(Program.Config);
				MainCleanUp();
				return;
			}
			if(m_cmdLineArgs[AppDefs.CommandLineOptions.ConfigGetUrlOverride] != null)
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
				MainCleanUp();
				return;
			}
			if(m_cmdLineArgs[AppDefs.CommandLineOptions.ConfigAddUrlOverride] != null)
			{
				bool bAct = (m_cmdLineArgs[AppDefs.CommandLineOptions.Activate] != null);
				Program.Config.Integration.UrlSchemeOverrides.AddCustomOverride(
					m_cmdLineArgs[AppDefs.CommandLineOptions.Scheme],
					m_cmdLineArgs[AppDefs.CommandLineOptions.Value], bAct, bAct);
				AppConfigSerializer.Save(Program.Config);
				MainCleanUp();
				return;
			}
			if(m_cmdLineArgs[AppDefs.CommandLineOptions.ConfigRemoveUrlOverride] != null)
			{
				Program.Config.Integration.UrlSchemeOverrides.RemoveCustomOverride(
					m_cmdLineArgs[AppDefs.CommandLineOptions.Scheme],
					m_cmdLineArgs[AppDefs.CommandLineOptions.Value]);
				AppConfigSerializer.Save(Program.Config);
				MainCleanUp();
				return;
			}
			if(m_cmdLineArgs[AppDefs.CommandLineOptions.ConfigSetLanguageFile] != null)
			{
				Program.Config.Application.LanguageFile = m_cmdLineArgs[
					AppDefs.CommandLineOptions.ConfigSetLanguageFile];
				AppConfigSerializer.Save(Program.Config);
				MainCleanUp();
				return;
			}
			if(m_cmdLineArgs[AppDefs.CommandLineOptions.PlgxCreate] != null)
			{
				PlgxPlugin.CreateFromCommandLine();
				MainCleanUp();
				return;
			}
			if(m_cmdLineArgs[AppDefs.CommandLineOptions.PlgxCreateInfo] != null)
			{
				PlgxPlugin.CreateInfoFile(m_cmdLineArgs.FileName);
				MainCleanUp();
				return;
			}
			if(m_cmdLineArgs[AppDefs.CommandLineOptions.ShowAssemblyInfo] != null)
			{
				MessageService.ShowInfo(Assembly.GetExecutingAssembly().ToString());
				MainCleanUp();
				return;
			}
			if(m_cmdLineArgs[AppDefs.CommandLineOptions.MakeXmlSerializerEx] != null)
			{
				XmlSerializerEx.GenerateSerializers(m_cmdLineArgs);
				MainCleanUp();
				return;
			}
			if(m_cmdLineArgs[AppDefs.CommandLineOptions.MakeXspFile] != null)
			{
				XspArchive.CreateFile(m_cmdLineArgs.FileName, m_cmdLineArgs["d"]);
				MainCleanUp();
				return;
			}
			if(m_cmdLineArgs[AppDefs.CommandLineOptions.Version] != null)
			{
				Console.WriteLine(PwDefs.ShortProductName + " " + PwDefs.VersionString);
				Console.WriteLine(PwDefs.Copyright);
				MainCleanUp();
				return;
			}
#if DEBUG
			if(m_cmdLineArgs[AppDefs.CommandLineOptions.TestGfx] != null)
			{
				List<Image> lImg = new List<Image>();
				lImg.Add(Properties.Resources.B16x16_Browser);
				lImg.Add(Properties.Resources.B48x48_Keyboard_Layout);
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
			// if(m_cmdLineArgs[AppDefs.CommandLineOptions.MakePopularPasswordTable] != null)
			// {
			//	PopularPasswords.MakeList();
			//	MainCleanUp();
			//	return;
			// }
			// #endif

			try { m_nAppMessage = NativeMethods.RegisterWindowMessage(m_strWndMsgID); }
			catch(Exception) { Debug.Assert(NativeLib.IsUnix()); }

			if(m_cmdLineArgs[AppDefs.CommandLineOptions.ExitAll] != null)
			{
				BroadcastAppMessageAndCleanUp(AppMessage.Exit);
				return;
			}
			if(m_cmdLineArgs[AppDefs.CommandLineOptions.AutoType] != null)
			{
				BroadcastAppMessageAndCleanUp(AppMessage.AutoType);
				return;
			}
			if(m_cmdLineArgs[AppDefs.CommandLineOptions.AutoTypePassword] != null)
			{
				BroadcastAppMessageAndCleanUp(AppMessage.AutoTypePassword);
				return;
			}
			if(m_cmdLineArgs[AppDefs.CommandLineOptions.AutoTypeSelected] != null)
			{
				BroadcastAppMessageAndCleanUp(AppMessage.AutoTypeSelected);
				return;
			}
			if(m_cmdLineArgs[AppDefs.CommandLineOptions.OpenEntryUrl] != null)
			{
				string strEntryUuid = m_cmdLineArgs[AppDefs.CommandLineOptions.Uuid];
				if(!string.IsNullOrEmpty(strEntryUuid))
				{
					IpcParamEx ipUrl = new IpcParamEx(IpcUtilEx.CmdOpenEntryUrl,
						strEntryUuid, null, null, null, null);
					IpcUtilEx.SendGlobalMessage(ipUrl, false);
				}

				MainCleanUp();
				return;
			}
			if(m_cmdLineArgs[AppDefs.CommandLineOptions.LockAll] != null)
			{
				BroadcastAppMessageAndCleanUp(AppMessage.Lock);
				return;
			}
			if(m_cmdLineArgs[AppDefs.CommandLineOptions.UnlockAll] != null)
			{
				BroadcastAppMessageAndCleanUp(AppMessage.Unlock);
				return;
			}
			if(m_cmdLineArgs[AppDefs.CommandLineOptions.Cancel] != null)
			{
				BroadcastAppMessageAndCleanUp(AppMessage.Cancel);
				return;
			}

			string strIpc = m_cmdLineArgs[AppDefs.CommandLineOptions.IpcEvent];
			string strIpc1 = m_cmdLineArgs[AppDefs.CommandLineOptions.IpcEvent1];
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

				MainCleanUp();
				return;
			}

			// Mutex mSingleLock = TrySingleInstanceLock(AppDefs.MutexName, true);
			bool bSingleLock = GlobalMutexPool.CreateMutex(AppDefs.MutexName, true);
			// if((mSingleLock == null) && m_appConfig.Integration.LimitToSingleInstance)
			if(!bSingleLock && m_appConfig.Integration.LimitToSingleInstance)
			{
				ActivatePreviousInstance(args);
				MainCleanUp();
				return;
			}

			Mutex mGlobalNotify = TryGlobalInstanceNotify(AppDefs.MutexNameGlobal);

			AutoType.InitStatic();

			CustomMessageFilterEx cmfx = new CustomMessageFilterEx();
			Application.AddMessageFilter(cmfx);

#if DEBUG
			if(m_cmdLineArgs[AppDefs.CommandLineOptions.DebugThrowException] != null)
				throw new Exception(AppDefs.CommandLineOptions.DebugThrowException);

			m_formMain = new MainForm();
			Application.Run(m_formMain);
#else
			try
			{
				if(m_cmdLineArgs[AppDefs.CommandLineOptions.DebugThrowException] != null)
					throw new Exception(AppDefs.CommandLineOptions.DebugThrowException);

				m_formMain = new MainForm();
				Application.Run(m_formMain);
			}
			catch(Exception exPrg) { ShowFatal(exPrg); }
#endif

			Application.RemoveMessageFilter(cmfx);

			Debug.Assert(GlobalWindowManager.WindowCount == 0);
			Debug.Assert(MessageService.CurrentMessageCount == 0);

			MainCleanUp();

#if DEBUG
			string strEndWorkDir = WinUtil.GetWorkingDirectory();
			Debug.Assert(strEndWorkDir.Equals(strInitialWorkDir, StrUtil.CaseIgnoreCmp));
#endif

			if(mGlobalNotify != null) { GC.KeepAlive(mGlobalNotify); }
			// if(mSingleLock != null) { GC.KeepAlive(mSingleLock); }
		}

		/// <summary>
		/// Common program initialization function that can also be
		/// used by applications that use KeePass as a library
		/// (like e.g. KPScript).
		/// </summary>
		public static bool CommonInit()
		{
			m_bDesignMode = false; // Again, for the ones not calling Main

			m_rndGlobal = CryptoRandom.NewWeakRandom();

			InitEnvSecurity();
			InitAppContext();
			MonoWorkarounds.Initialize();

			// Do not run as AppX, because of compatibility problems
			if(WinUtil.IsAppX) return false;

			try { SelfTest.TestFipsComplianceProblems(); }
			catch(Exception exFips)
			{
				MessageService.ShowWarning(KPRes.SelfTestFailed, exFips);
				return false;
			}

			// Set global localized strings
			PwDatabase.LocalizedAppName = PwDefs.ShortProductName;
			KdbxFile.DetermineLanguageId();

			m_appConfig = AppConfigSerializer.Load();
			if(m_appConfig.Logging.Enabled)
				AppLogEx.Open(PwDefs.ShortProductName);

			AppPolicy.Current = m_appConfig.Security.Policy.CloneDeep();
			AppPolicy.ApplyToConfig();

			if(m_appConfig.Security.ProtectProcessWithDacl)
				KeePassLib.Native.NativeMethods.ProtectProcessWithDacl();

			m_appConfig.Apply(AceApplyFlags.All);

			m_ecasTriggers = m_appConfig.Application.TriggerSystem;
			m_ecasTriggers.SetToInitialState();

			// InitEnvWorkarounds();
			LoadTranslation();

			CustomResourceManager.Override(typeof(KeePass.Properties.Resources));

#if KP_DEVSNAP
			if(!m_bAsmResReg)
			{
				AppDomain.CurrentDomain.AssemblyResolve += Program.AssemblyResolve;
				m_bAsmResReg = true;
			}
			else { Debug.Assert(false); }
#endif

			return true;
		}

		public static void CommonTerminate()
		{
#if DEBUG
			Debug.Assert(ShutdownBlocker.Instance == null);
			Debug.Assert(!SendInputEx.IsSending);
			// GC.Collect(); // Force invocation of destructors
#endif

			AppLogEx.Close();

			if(m_tempFilesPool != null)
			{
				m_tempFilesPool.Clear(TempClearFlags.All);
				m_tempFilesPool.WaitForThreads();
			}

			EnableThemingInScope.StaticDispose();
			MonoWorkarounds.Terminate();

#if KP_DEVSNAP
			if(m_bAsmResReg)
			{
				AppDomain.CurrentDomain.AssemblyResolve -= Program.AssemblyResolve;
				m_bAsmResReg = false;
			}
			else { Debug.Assert(false); }
#endif
		}

		private static void MainCleanUp()
		{
			IpcBroadcast.StopServer();

			EntryMenu.Destroy();

			GlobalMutexPool.ReleaseAll();

			CommonTerminate();
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
				f("Switch.System.IO.Compression.DoNotUseNativeZipLibraryForDecompression", false); // 4.7.2
				f("Switch.System.IO.Compression.ZipFile.UseBackslash", false); // 4.6.1
				f("Switch.System.Security.Cryptography.AesCryptoServiceProvider.DontCorrectlyResetDecryptor", false); // 4.6.2
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
				// Check that the internal classes do not cache other values already

				const BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic |
					BindingFlags.Static;

				GAction<Type, string, bool> fCheckB = delegate(Type tClass,
					string strProperty, bool bValue)
				{
					PropertyInfo pi = tClass.GetProperty(strProperty, bf);
					string strFullName = tClass.FullName + "." + strProperty;
					if(pi == null) { Debug.Assert(false, strFullName + " not found!"); return; }
					Debug.Assert(((bool)pi.GetValue(null, null) == bValue),
						strFullName + " returned an unexpected value!");
				};

				Type tS = typeof(GZipStream).Assembly.GetType(
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

				fCheckB(tD, "DontSupportPngFramesInIcons", false);
				fCheckB(tD, "OptimizePrintPreview", true);
				fCheckB(tS, "DoNotUseNativeZipLibraryForDecompression", false);
				fCheckB(tW, "DoNotLoadLatestRichEditControl", false);
				fCheckB(tW, "DoNotSupportSelectAllShortcutInMultilineTextBox", false);
				fCheckB(tW, "DontSupportReentrantFilterMessage", false);
				fCheckB(tW, "EnableVisualStyleValidation", false);
				fCheckB(tA, "Level4", true);
#endif
			}
			catch(Exception) { Debug.Assert(false); }
		}

		// internal static Mutex TrySingleInstanceLock(string strName, bool bInitiallyOwned)
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

		internal static Mutex TryGlobalInstanceNotify(string strBaseName)
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

		// internal static void DestroyMutex(Mutex m, bool bReleaseFirst)
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
			if((m_nAppMessage == 0) && !NativeLib.IsUnix())
			{
				Debug.Assert(false);
				return;
			}

			try
			{
				if(string.IsNullOrEmpty(m_cmdLineArgs.FileName))
				{
					// NativeMethods.PostMessage((IntPtr)NativeMethods.HWND_BROADCAST,
					//	m_nAppMessage, (IntPtr)AppMessage.RestoreWindow, IntPtr.Zero);
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
			MainForm mf = m_formMain;
			if(mf != null) mf.NotifyUserActivity();
		}

		public static IntPtr GetSafeMainWindowHandle()
		{
			try
			{
				MainForm mf = m_formMain;
				if(mf != null) return mf.Handle;
			}
			catch(Exception) { Debug.Assert(false); }

			return IntPtr.Zero;
		}

		private static void BroadcastAppMessageAndCleanUp(AppMessage msg)
		{
			try
			{
				// NativeMethods.PostMessage((IntPtr)NativeMethods.HWND_BROADCAST,
				//	m_nAppMessage, (IntPtr)msg, IntPtr.Zero);
				IpcBroadcast.Send(msg, 0, false);
			}
			catch(Exception) { Debug.Assert(false); }

			MainCleanUp();
		}

		private static void LoadTranslation()
		{
			if(!m_bEnableTranslation) return;

			string strPath = m_appConfig.Application.GetLanguageFilePath();
			if(string.IsNullOrEmpty(strPath)) return;

			try
			{
				// Performance optimization
				if(!File.Exists(strPath)) return;

				XmlSerializerEx xs = new XmlSerializerEx(typeof(KPTranslation));
				m_kpTranslation = KPTranslation.Load(strPath, xs);

				KPRes.SetTranslatedStrings(
					m_kpTranslation.SafeGetStringTableDictionary(
					"KeePass.Resources.KPRes"));
				KLRes.SetTranslatedStrings(
					m_kpTranslation.SafeGetStringTableDictionary(
					"KeePassLib.Resources.KLRes"));

				StrUtil.RightToLeft = m_kpTranslation.Properties.RightToLeft;
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
		private static Stack<TraceListener[]> g_sTraceListeners = null;
#endif
		[Conditional("DEBUG")]
		internal static void EnableAssertions(bool bEnable)
		{
#if DEBUG
			Stack<TraceListener[]> s = g_sTraceListeners;
			if(s == null)
			{
				s = new Stack<TraceListener[]>();
				g_sTraceListeners = s;
			}

			if(bEnable)
			{
				Debug.Listeners.Clear();
				Debug.Listeners.AddRange(s.Pop());
			}
			else
			{
				TraceListener[] v = new TraceListener[Debug.Listeners.Count];
				Debug.Listeners.CopyTo(v, 0);
				s.Push(v);
				Debug.Listeners.Clear();
			}
#endif
		}
	}
}
