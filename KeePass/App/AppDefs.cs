/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2007 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Drawing;
using System.Windows.Forms;

using KeePass.UI;

namespace KeePass.App
{
	public static class AppDefs
	{
		public enum ColumnID
		{
			Title,
			UserName,
			Password,
			Url,
			Notes,
			CreationTime,
			LastAccessTime,
			LastModificationTime,
			ExpiryTime,
			Uuid,
			Attachment,
			Count // Virtual identifier representing the number of columns
		}

		public static class ConfigKeys
		{
			public static readonly KeyValuePair<string, string> Language = new
				KeyValuePair<string, string>("Application.Language", string.Empty);
			public static readonly KeyValuePair<string, string> EnableLogging = new
				KeyValuePair<string, string>("Application.Logging.Enable", "False");

			public static readonly KeyValuePair<string, string> MainWindowPositionX = new
				KeyValuePair<string, string>("MainWindow.Position.X", "-16381");
			public static readonly KeyValuePair<string, string> MainWindowPositionY = new
				KeyValuePair<string, string>("MainWindow.Position.Y", "-16381");
			public static readonly KeyValuePair<string, string> MainWindowWidth = new
				KeyValuePair<string, string>("MainWindow.Size.Width", "-16381");
			public static readonly KeyValuePair<string, string> MainWindowHeight = new
				KeyValuePair<string, string>("MainWindow.Size.Height", "-16381");

			public static readonly KeyValuePair<string, string> MainWindowMaximized = new
				KeyValuePair<string, string>("MainWindow.Maximized", "False");
			
			public static readonly KeyValuePair<string, string> MainWindowHorzSplitter = new
				KeyValuePair<string, string>("MainWindow.Splitters.Horizontal", "-128");
			public static readonly KeyValuePair<string, string> MainWindowVertSplitter = new
				KeyValuePair<string, string>("MainWindow.Splitters.Vertical", "-128");
			public static readonly KeyValuePair<string, string> MainWindowLayoutSideBySide = new
				KeyValuePair<string, string>("MainWindow.Layout.SideBySide", "False");

			public static readonly KeyValuePair<string, string> AlwaysOnTop = new
				KeyValuePair<string, string>("MainWindow.AlwaysOnTop", "False");

			public static readonly KeyValuePair<string, string> CloseButtonMinimizes = new
				KeyValuePair<string, string>("MainWindow.CloseButtonMinimizesWindow", "False");

			public static readonly KeyValuePair<string, string> ShowFullPathInTitleBar = new
				KeyValuePair<string, string>("MainWindow.TitleBar.ShowFullPaths", "False");

			public static readonly KeyValuePair<string, string> MinimizeToTray = new
				KeyValuePair<string, string>("UI.Tray.MinimizeToTray", "False");
			public static readonly KeyValuePair<string, string> ShowTrayIconOnlyIfTrayed = new
				KeyValuePair<string, string>("UI.Tray.ShowIconOnlyIfTrayed", "False");
			public static readonly KeyValuePair<string, string> SingleClickForTrayAction = new
				KeyValuePair<string, string>("UI.Tray.SingleClickDefault", "False");
			public static readonly KeyValuePair<string, string> MinimizeAfterCopy = new
				KeyValuePair<string, string>("UI.MinimizeAfterClipboardCopy", "False");

			public static readonly KeyValuePair<string, string> TitleColumnWidth = new
				KeyValuePair<string, string>("EntryList.Columns.Title.Width", "75");
			public static readonly KeyValuePair<string, string> UserNameColumnWidth = new
				KeyValuePair<string, string>("EntryList.Columns.UserName.Width", "75");
			public static readonly KeyValuePair<string, string> PasswordColumnWidth = new
				KeyValuePair<string, string>("EntryList.Columns.Password.Width", "75");
			public static readonly KeyValuePair<string, string> UrlColumnWidth = new
				KeyValuePair<string, string>("EntryList.Columns.URL.Width", "75");
			public static readonly KeyValuePair<string, string> NotesColumnWidth = new
				KeyValuePair<string, string>("EntryList.Columns.Notes.Width", "75");
			public static readonly KeyValuePair<string, string> CreationTimeColumnWidth = new
				KeyValuePair<string, string>("EntryList.Columns.CreationTime.Width", "0");
			public static readonly KeyValuePair<string, string> LastAccessTimeColumnWidth = new
				KeyValuePair<string, string>("EntryList.Columns.LastAccessTime.Width", "0");
			public static readonly KeyValuePair<string, string> LastModTimeColumnWidth = new
				KeyValuePair<string, string>("EntryList.Columns.LastModificationTime.Width", "0");
			public static readonly KeyValuePair<string, string> ExpireTimeColumnWidth = new
				KeyValuePair<string, string>("EntryList.Columns.ExpireTime.Width", "0");
			public static readonly KeyValuePair<string, string> UuidColumnWidth = new
				KeyValuePair<string, string>("EntryList.Columns.UUID.Width", "0");
			public static readonly KeyValuePair<string, string> AttachmentColumnWidth = new
				KeyValuePair<string, string>("EntryList.Columns.Attachment.Width", "0");

			public static readonly KeyValuePair<string, string> ShowGridLines = new
				KeyValuePair<string, string>("EntryList.ShowGridLines", "False");

			public static readonly KeyValuePair<string, string> HideTitles = new
				KeyValuePair<string, string>("UI.Hiding.HideTitles", "False");
			public static readonly KeyValuePair<string, string> HideUserNames = new
				KeyValuePair<string, string>("UI.Hiding.HideUserNames", "False");
			public static readonly KeyValuePair<string, string> HidePasswords = new
				KeyValuePair<string, string>("UI.Hiding.HidePasswords", "True");
			public static readonly KeyValuePair<string, string> HideUrls = new
				KeyValuePair<string, string>("UI.Hiding.HideURLs", "False");
			public static readonly KeyValuePair<string, string> HideNotes = new
				KeyValuePair<string, string>("UI.Hiding.HideNotes", "False");

			public static readonly KeyValuePair<string, string> RememberHidingInDialogs = new
				KeyValuePair<string, string>("UI.Hiding.InDialogs.RememberSeparately", "True");
			public static readonly KeyValuePair<string, string> HidePasswordInEntryForm = new
				KeyValuePair<string, string>("UI.Hiding.InDialogs.EntryForm.HidePassword", "True");

			public static readonly KeyValuePair<string, string> ShowToolBar = new
				KeyValuePair<string, string>("UI.ToolBar.Show", "True");
			public static readonly KeyValuePair<string, string> ShowEntryView = new
				KeyValuePair<string, string>("UI.EntryView.Show", "True");

			public static readonly KeyValuePair<string, string> ListFont = new
				KeyValuePair<string, string>("UI.ListFont", string.Empty);

			public static readonly KeyValuePair<string, string> MruMaxItemCount = new
				KeyValuePair<string, string>("UI.MRU.MaxItemCount", "6");
			public static readonly KeyValuePair<string, string> MruItem = new
				KeyValuePair<string, string>("UI.MRU.Item", null);

			public static readonly KeyValuePair<string, string> BannerStyle = new
				KeyValuePair<string, string>("UI.Banner.Style",
				((uint)BannerFactory.BannerStyle.WinVistaBlack).ToString());

			public static readonly KeyValuePair<string, string> LockOnMinimize = new
				KeyValuePair<string, string>("Security.Locking.OnWindowMinimize", "True");
			public static readonly KeyValuePair<string, string> LockOnSessionLock = new
				KeyValuePair<string, string>("Security.Locking.OnSessionLock", "False");
			public static readonly KeyValuePair<string, string> LockAfterTime = new
				KeyValuePair<string, string>("Security.Locking.AfterTime", "0");
			public static readonly KeyValuePair<string, string> UseNativeForKeyEnc = new
				KeyValuePair<string, string>("Security.KeyTransformations.UseNative", "True");
			public static readonly KeyValuePair<string, string> ClipboardAutoClearOnExit = new
				KeyValuePair<string, string>("Security.Clipboard.AutoClear.OnExit", "True");
			public static readonly KeyValuePair<string, string> ClipboardAutoClearTime = new
				KeyValuePair<string, string>("Security.Clipboard.AutoClear.Seconds", "12");

			public static readonly KeyValuePair<string, string> PwGenProfile = new
				KeyValuePair<string, string>("PasswordGenerator.Profile", null);

			public static readonly KeyValuePair<string, string> DefaultExpireDays = new
				KeyValuePair<string, string>("Defaults.NewEntry.ExpireDays", "-1");
			public static readonly KeyValuePair<string, string> DefaultGeneratePw = new
				KeyValuePair<string, string>("Defaults.NewEntry.GenerateRandomPassword", "True");

			public static readonly KeyValuePair<string, string> TanSimpleList = new
				KeyValuePair<string, string>("UI.TANView.UseSimpleList", "True");
			public static readonly KeyValuePair<string, string> TanIndices = new
				KeyValuePair<string, string>("UI.TANView.ShowIndices", "True");

			public static readonly KeyValuePair<string, string> PolicyPlugins = new
				KeyValuePair<string, string>("Policy.Plugins", "True");
			public static readonly KeyValuePair<string, string> PolicyExport = new
				KeyValuePair<string, string>("Policy.Export", "True");
			public static readonly KeyValuePair<string, string> PolicyImport = new
				KeyValuePair<string, string>("Policy.Import", "True");
			public static readonly KeyValuePair<string, string> PolicyPrint = new
				KeyValuePair<string, string>("Policy.Print", "True");
			public static readonly KeyValuePair<string, string> PolicySaveDatabase = new
				KeyValuePair<string, string>("Policy.SaveDatabase", "True");
			public static readonly KeyValuePair<string, string> PolicyAutoType = new
				KeyValuePair<string, string>("Policy.AutoType", "True");
			public static readonly KeyValuePair<string, string> PolicyCopyToClipboard = new
				KeyValuePair<string, string>("Policy.CopyToClipboard", "True");
			public static readonly KeyValuePair<string, string> PolicyDragDrop = new
				KeyValuePair<string, string>("Policy.DragDrop", "True");

			public static readonly KeyValuePair<string, string> GlobalAutoTypeHotKey = new
				KeyValuePair<string, string>("Integration.HotKeys.AutoType.Key",
				((ulong)Keys.A).ToString());
			public static readonly KeyValuePair<string, string> GlobalAutoTypeModifiers = new
				KeyValuePair<string, string>("Integration.HotKeys.AutoType.Modifiers",
				((ulong)(Keys.Control | Keys.Alt)).ToString());
			public static readonly KeyValuePair<string, string> ShowWindowHotKey = new
				KeyValuePair<string, string>("Integration.HotKeys.ShowWindow.Key",
				((ulong)Keys.K).ToString());
			public static readonly KeyValuePair<string, string> ShowWindowHotKeyModifiers = new
				KeyValuePair<string, string>("Integration.HotKeys.ShowWindow.Modifiers",
				((ulong)(Keys.Control | Keys.Alt)).ToString());
			public static readonly KeyValuePair<string, string> UrlOverride = new
				KeyValuePair<string, string>("Integration.URLsOverride", string.Empty);

			public static readonly KeyValuePair<string, string> SearchKeyFilesOnRemovable = new
				KeyValuePair<string, string>("Automation.SearchKeyFilesOnRemovableMedia", "False");

			public static readonly KeyValuePair<string, string> AutoOpenLastFile = new
				KeyValuePair<string, string>("StartUp.AutoOpenLastDatabase", "True");
			public static readonly KeyValuePair<string, string> LimitSingleInstance = new
				KeyValuePair<string, string>("StartUp.LimitSingleInstance", "False");
			public static readonly KeyValuePair<string, string> AutoCheckForUpdate = new
				KeyValuePair<string, string>("StartUp.AutoCheckForUpdate", "False");

			public static readonly KeyValuePair<string, string> ShowExpiredOnDbOpen = new
				KeyValuePair<string, string>("Opening.EntryExpiry.AutoShowExpiredEntries", "False");
			public static readonly KeyValuePair<string, string> ShowSoonToExpireOnDbOpen = new
				KeyValuePair<string, string>("Opening.EntryExpiry.AutoShowSoonToExpireEntries", "False");

			public static readonly KeyValuePair<string, string> AutoSaveOnExit = new
				KeyValuePair<string, string>("Closing.AutoSaveDatabase", "False");

			public static readonly KeyValuePair<string, string> LastDatabase = new
				KeyValuePair<string, string>("State.LastDatabase", string.Empty);

			public static readonly KeyValuePair<string, string> HelpUseLocal = new
				KeyValuePair<string, string>("Help.Source.UseLocal", "False");
		}

		public static readonly Color ColorControlNormal = SystemColors.Window;
		public static readonly Color ColorControlDisabled = SystemColors.Control;
		public static readonly Color ColorEditError = Color.FromArgb(255, 192, 192);

		public const string XslFileHtmlLite = "XSL\\KDB4_DetailsLite.xsl";
		public const string XslFileHtmlFull = "XSL\\KDB4_DetailsFull.xsl";
		public const string XslFileHtmlTabular = "XSL\\KDB4_Tabular.xsl";

		public const string PluginProductName = "KeePass Plugin";

		public const string MruNameValueSplitter = @"/::/";

		/// <summary>
		/// Hot key IDs (used in WM_HOTKEY window messages).
		/// </summary>
		public enum GlobalHotKeyID : int
		{
			AutoType = 195,
			ShowWindow = 226
		}

		public static class HelpTopics
		{
			public const string Acknowledgements = "base/credits";
			public const string License = "v2/license";

			public const string DatabaseSettings = "v2/dbsettings";
			public const string DbSettingsGeneral = "general";
			public const string DbSettingsSecurity = "security";
			public const string DbSettingsProtection = "protection";
			public const string DbSettingsCompression = "compression";

			public const string AutoType = "base/autotype";

			public const string Entry = "v2/entry";
			public const string EntryGeneral = "general";
			public const string EntryStrings = "advanced";
			public const string EntryAutoType = "autotype";
			public const string EntryHistory = "history";

			public const string KeySources = "base/keys";
			public const string PwGenerator = "v2/pwgenerator";
			public const string IOConnections = "v2/ioconnect";

			public const string ImportExport = "base/importexport";
			public const string ImportExportSteganos = "imp_steganos";
		}

		public static class CommandLineOptions
		{
			public const string Password = "pw";
			public const string KeyFile = "keyfile";
			public const string UserAccount = "useraccount";

			public const string PreSelect = "preselect";

			public const string FileExtRegister = "registerfileext";
			public const string FileExtUnregister = "unregisterfileext";
		}

		public static class FileExtension
		{
			public const string FileExt = "kdbx";
			public const string ExtID = "kdbfile";

			public const string KeyFile = "key";
		}

		public const string AutoRunName = "KeePass Password Safe";

		public const string MutexName = "KeePassApplicationMutex";

		public const string ScriptExtension = "kps";

		public static class NamedEntryColor
		{
			public static readonly Color LightRed = Color.FromArgb(255, 204, 204);
			public static readonly Color LightGreen = Color.FromArgb(204, 255, 204);
			public static readonly Color LightBlue = Color.FromArgb(153, 204, 255);
			public static readonly Color LightYellow = Color.FromArgb(255, 255, 153);
		}

		public const string DefaultTrlAuthor = "Dominik Reichl";
		public const string DefaultTrlContact = @"http://www.dominik-reichl.de/";

		public const string LanguageInfoFileName = "LanguageInfo.xml";
	}
}
