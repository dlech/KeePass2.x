/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2019 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Windows.Forms;

using KeePass.Resources;
using KeePass.UI;

using KeePassLib;
using KeePassLib.Utility;

namespace KeePass.App
{
	public static class AppDefs
	{
		public static readonly Color ColorControlNormal = SystemColors.Window;
		public static readonly Color ColorControlDisabled = SystemColors.Control;
		public static readonly Color ColorEditError = Color.FromArgb(255, 192, 192);

		public static readonly Color ColorQualityLow = Color.FromArgb(255, 128, 0);
		public static readonly Color ColorQualityHigh = Color.FromArgb(0, 255, 0);
		public static readonly Color ColorQualityMid = Color.FromArgb(255, 255, 0);

		public static readonly string LanguagesDir = "Languages";

		public static readonly string PluginsDir = "Plugins";
		public static readonly string PluginProductName = "KeePass Plugin";

		public static readonly string XslFilesDir = "XSL";
		public static readonly string XslFileHtmlFull = "KDBX_DetailsFull_HTML.xsl";
		public static readonly string XslFileHtmlLight = "KDBX_DetailsLight_HTML.xsl";
		public static readonly string XslFileHtmlTabular = "KDBX_Tabular_HTML.xsl";

		public static class FileNames
		{
			public static readonly string Program = "KeePass.exe";
			public static readonly string XmlSerializers = "KeePass.XmlSerializers.dll";

			public static readonly string NativeLib32 = "KeePassLibC32.dll";
			public static readonly string NativeLib64 = "KeePassLibC64.dll";

			public static readonly string ShInstUtil = "ShInstUtil.exe";
		}

		// internal const string MruNameValueSplitter = @"/::/";

		/// <summary>
		/// Hot key IDs (used in <c>WM_HOTKEY</c> window messages).
		/// </summary>
		public static class GlobalHotKeyId
		{
			public static readonly int AutoType = 195;
			public static readonly int AutoTypePassword = 197;
			public static readonly int AutoTypeSelected = 196;
			public static readonly int ShowWindow = 226;
			public static readonly int EntryMenu = 227;

			internal const int TempRegTest = 225;
		}

		public static class HelpTopics
		{
			public static readonly string Acknowledgements = "base/credits";
			public static readonly string License = "v2/license";

			public static readonly string DatabaseSettings = "v2/dbsettings";
			public static readonly string DbSettingsGeneral = "general";
			public static readonly string DbSettingsSecurity = "security";
			// public static readonly string DbSettingsProtection = "protection";
			public static readonly string DbSettingsCompression = "compression";

			public static readonly string AutoType = "base/autotype";
			public static readonly string AutoTypeObfuscation = "v2/autotype_obfuscation";
			public static readonly string AutoTypeWindowFilters = "autowindows";

			public static readonly string Entry = "v2/entry";
			public static readonly string EntryGeneral = "general";
			public static readonly string EntryStrings = "advanced";
			public static readonly string EntryAutoType = "autotype";
			public static readonly string EntryHistory = "history";

			public static readonly string KeySources = "base/keys";
			public static readonly string KeySourcesKeyFile = "keyfiles";
			public static readonly string KeySourcesUserAccount = "winuser";

			public static readonly string PwGenerator = "base/pwgenerator";
			public static readonly string IOConnections = "v2/ioconnect";
			public static readonly string UrlField = "base/autourl";
			public static readonly string CommandLine = "base/cmdline";
			public static readonly string FieldRefs = "base/fieldrefs";

			public static readonly string ImportExport = "base/importexport";
			public static readonly string ImportExportGenericCsv = "genericcsv";
			public static readonly string ImportExportSteganos = "imp_steganos";
			public static readonly string ImportExportPassKeeper = "imp_passkeeper";

			public static readonly string AppPolicy = "v2/policy";

			public static readonly string Triggers = "v2/triggers";
			public static readonly string TriggersEvents = "events";
			public static readonly string TriggersConditions = "conditions";
			public static readonly string TriggersActions = "actions";

			public static readonly string TriggerUIStateUpd = "kb/trigger_uistateupd";

			public static readonly string Setup = "v2/setup";
			public static readonly string SetupMono = "mono";

			// public static readonly string FaqTech = "base/faq_tech";
			// public static readonly string FaqTechMemProt = "memprot";

			public static readonly string XmlReplace = "v2/xml_replace";

			public static readonly string KbFaq = "kb/faq";
			public static readonly string KbFaqURtf = "urtf";
		}

		public static class CommandLineOptions
		{
			public static readonly string Password = "pw";
			public static readonly string KeyFile = "keyfile";
			public static readonly string UserAccount = "useraccount";

			public static readonly string PasswordEncrypted = "pw-enc";
			public static readonly string PasswordStdIn = "pw-stdin";

			public static readonly string PreSelect = "preselect";

			public static readonly string IoCredUserName = "iousername";
			public static readonly string IoCredPassword = "iopassword";
			public static readonly string IoCredFromRecent = "iocredfromrecent";
			public static readonly string IoCredIsComplete = "ioiscomplete";

			// User-friendly Pascal-case (shown in UAC dialog)
			public static readonly string FileExtRegister = "RegisterFileExt";
			public static readonly string FileExtUnregister = "UnregisterFileExt";

			public static readonly string PreLoad = "preload";
			// public static readonly string PreLoadRegister = "registerpreload";
			// public static readonly string PreLoadUnregister = "unregisterpreload";

			public static readonly string ExitAll = "exit-all";
			public static readonly string Minimize = "minimize";
			public static readonly string AutoType = "auto-type";
			public static readonly string AutoTypePassword = "auto-type-password";
			public static readonly string AutoTypeSelected = "auto-type-selected";
			public static readonly string OpenEntryUrl = "entry-url-open";
			public static readonly string LockAll = "lock-all";
			public static readonly string UnlockAll = "unlock-all";
			public static readonly string Cancel = "cancel";
			public static readonly string IpcEvent = "e";
			public static readonly string IpcEvent1 = "e1";

			public static readonly string Uuid = "uuid";

			public static readonly string Help = "?";
			public static readonly string HelpLong = "help";

			public static readonly string WorkaroundDisable = "wa-disable";

			public static readonly string ConfigPathLocal = "cfg-local";

			public static readonly string ConfigSetUrlOverride = "set-urloverride";
			public static readonly string ConfigClearUrlOverride = "clear-urloverride";
			public static readonly string ConfigGetUrlOverride = "get-urloverride";

			public static readonly string ConfigSetLanguageFile = "set-languagefile";

			public static readonly string PlgxCreate = "plgx-create";
			public static readonly string PlgxCreateInfo = "plgx-create-info";
			public static readonly string PlgxPrereqKP = "plgx-prereq-kp";
			public static readonly string PlgxPrereqNet = "plgx-prereq-net";
			public static readonly string PlgxPrereqOS = "plgx-prereq-os";
			public static readonly string PlgxPrereqPtr = "plgx-prereq-ptr";
			public static readonly string PlgxBuildPre = "plgx-build-pre";
			public static readonly string PlgxBuildPost = "plgx-build-post";

			public static readonly string Debug = "debug";
			public static readonly string DebugThrowException = "debug-throwexcp";
			// public static readonly string SavePluginCompileRes = "saveplgxcr"; // Now: Debug
			public static readonly string ShowAssemblyInfo = "showasminfo";
			public static readonly string MakeXmlSerializerEx = "makexmlserializerex";
			public static readonly string MakeXspFile = "makexspfile";

#if DEBUG
			public static readonly string TestGfx = "testgfx";
#endif

			public static readonly string Version = "version"; // For Unix

			// #if (DEBUG && !KeePassLibSD)
			// public static readonly string MakePopularPasswordTable = "makepopularpasswordtable";
			// #endif
		}

		public static class FileExtension
		{
			public static readonly string FileExt = "kdbx";
			public static readonly string ExtId = "kdbxfile";

			public static readonly string KeyFile = "key";
		}

		public static readonly string AutoRunName = "KeePass Password Safe 2";
		public static readonly string PreLoadName = "KeePass 2 PreLoad";

		public static readonly string MutexName = "KeePassAppMutex";
		public static readonly string MutexNameGlobal = "KeePassAppMutexEx";

		// public static readonly string ScriptExtension = "kps";

		public static readonly int InvalidWindowValue = -16381;

		public static class NamedEntryColor
		{
			public static readonly Color LightRed = Color.FromArgb(255, 204, 204);
			public static readonly Color LightGreen = Color.FromArgb(204, 255, 204);
			public static readonly Color LightBlue = Color.FromArgb(153, 204, 255);
			public static readonly Color LightYellow = Color.FromArgb(255, 255, 153);
		}

		public static class FileDialogContext
		{
			// Values must not contain '@'

			public static readonly string Database = "Database";
			public static readonly string Sync = "Sync";
			public static readonly string KeyFile = "KeyFile";
			public static readonly string Import = "Import";
			public static readonly string Export = "Export";
			public static readonly string Attachments = "Attachments";
			public static readonly string Xsl = "Xsl";
		}

		public static readonly string DefaultTrlAuthor = "Dominik Reichl";
		public static readonly string DefaultTrlContact = "https://www.dominik-reichl.de/";

		// public static readonly string LanguageInfoFileName = "LanguageInfo.xml";

		internal const string Rsa4096PublicKeyXml =
			@"<RSAKeyValue><Modulus>9Oa8Bb9if4rSYBxczLVQ3Yyae95dWQrNJ1FlqS7DoF" +
			@"RF80tD2hq84vxDE8slVeSHs68KMFnJhPsXFD6nM9oTRBaUlU/alnRTUU+X/cUXbr" +
			@"mhYN9DkJhM0OcWk5Vsl9Qxl613sA+hqIwmPc+el/fCM/1vP6JkHo/JTJ2OxQvDKN" +
			@"4cC55pHYMZt+HX6AhemsPe7ejTG7l9nN5tHGmD+GrlwuxBTddzFBARmoknFzDPWd" +
			@"QHddjuK1mXDs6lWeu73ODlSLSHMc5n0R2xMwGHN4eaiIMGzEbt0lv1aMWz+Iy1H3" +
			@"XgFgWGDHX9kx8yefmfcgFIK4Y/xHU5EyGAV68ZHPatv6i4pT4ZuecIb5GSoFzVXq" +
			@"8BZjbe+zDI+Wr1u8jLcBH0mySTWkF2gooQLvE1vgZXP1blsA7UFZSVFzYjBt36HQ" +
			@"SJLpQ9AjjB5MKpMSlvdb5SnvjzREiFVLoBsY7KH2TMz+IG1Rh3OZTGwjQKXkgRVj" +
			@"5XrEMTFRmT1zo2BHWhx8vrY6agVzqsCVqxYRbjeAhgOi6hLDMHSNAVuNg6ZHOKS8" +
			@"6x6kmBcBhGJriwY017H3Oxuhfz33ehRFX/C05egCvmR2TAXbqm+CUgrq1bZ96T/y" +
			@"s+O5uvKpe7H+EZuWb655Y9WuQSby+q0Vqqny7T6Z2NbEnI8nYHg5ZZP+TijSxeH0" +
			@"8=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

		public static readonly string ColumnIdnGroup = "Group";
		public static readonly string ColumnIdnCreationTime = "CreationTime";
		public static readonly string ColumnIdnLastModificationTime = "LastModificationTime";
		public static readonly string ColumnIdnLastAccessTime = "LastAccessTime";
		public static readonly string ColumnIdnExpiryTime = "ExpiryTime";
		public static readonly string ColumnIdnUuid = "UUID";
		public static readonly string ColumnIdnAttachment = "Attachment";

		public static string GetEntryField(PwEntry pe, string strFieldId)
		{
			if(pe == null) throw new ArgumentNullException("pe");
			if(strFieldId == null) throw new ArgumentNullException("strFieldId");

			if(strFieldId == AppDefs.ColumnIdnGroup)
				return ((pe.ParentGroup != null) ? pe.ParentGroup.Name : string.Empty);
			else if(strFieldId == AppDefs.ColumnIdnCreationTime)
				return TimeUtil.ToDisplayString(pe.CreationTime);
			else if(strFieldId == AppDefs.ColumnIdnLastModificationTime)
				return TimeUtil.ToDisplayString(pe.LastModificationTime);
			else if(strFieldId == AppDefs.ColumnIdnLastAccessTime)
				return TimeUtil.ToDisplayString(pe.LastAccessTime);
			else if(strFieldId == AppDefs.ColumnIdnExpiryTime)
			{
				if(!pe.Expires) return KPRes.NeverExpires;
				return TimeUtil.ToDisplayString(pe.ExpiryTime);
			}
			else if(strFieldId == AppDefs.ColumnIdnUuid)
				return pe.Uuid.ToHexString();
			else if(strFieldId == AppDefs.ColumnIdnAttachment)
				return pe.Binaries.UCount.ToString();

			return pe.Strings.ReadSafe(strFieldId);
		}

		internal static Color GetQualityColor(float fQ, bool bToControlBack)
		{
			if(fQ < 0.0f) { Debug.Assert(false); fQ = 0.0f; }
			if(fQ > 1.0f) { Debug.Assert(false); fQ = 1.0f; }

			Color clrL = AppDefs.ColorQualityLow;
			Color clrH = AppDefs.ColorQualityHigh;
			Color clrM = AppDefs.ColorQualityMid;

			int iR, iG, iB;
			if(fQ <= 0.5f)
			{
				fQ *= 2.0f;
				iR = clrL.R + (int)(fQ * ((int)clrM.R - (int)clrL.R));
				iG = clrL.G + (int)(fQ * ((int)clrM.G - (int)clrL.G));
				iB = clrL.B + (int)(fQ * ((int)clrM.B - (int)clrL.B));
			}
			else
			{
				fQ = (fQ - 0.5f) * 2.0f;
				iR = clrM.R + (int)(fQ * ((int)clrH.R - (int)clrM.R));
				iG = clrM.G + (int)(fQ * ((int)clrH.G - (int)clrM.G));
				iB = clrM.B + (int)(fQ * ((int)clrH.B - (int)clrM.B));
			}

			if(iR < 0) { Debug.Assert(false); iR = 0; }
			if(iR > 255) { Debug.Assert(false); iR = 255; }
			if(iG < 0) { Debug.Assert(false); iG = 0; }
			if(iG > 255) { Debug.Assert(false); iG = 255; }
			if(iB < 0) { Debug.Assert(false); iB = 0; }
			if(iB > 255) { Debug.Assert(false); iB = 255; }

			Color clrQ = Color.FromArgb(iR, iG, iB);
			if(bToControlBack)
				return UIUtil.ColorTowards(clrQ, AppDefs.ColorControlNormal, 0.5);
			return clrQ;
		}
	}
}
