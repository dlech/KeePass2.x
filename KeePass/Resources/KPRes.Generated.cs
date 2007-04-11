// This is a generated file!
// Do not edit manually, changes will be overwritten.

using System;
using System.Collections.Generic;

namespace KeePass.Resources
{
	/// <summary>
	/// A strongly-typed resource class, for looking up localized strings, etc.
	/// </summary>
	public static class KPRes
	{
		private static string TryGetEx(Dictionary<string, string> dictNew,
			string strName, string strDefault)
		{
			string strTemp;

			if(dictNew.TryGetValue(strName, out strTemp))
				return strTemp;

			return strDefault;
		}

		public static void SetTranslatedStrings(Dictionary<string, string> dictNew)
		{
			if(dictNew == null) throw new ArgumentNullException("dictNew");

			m_strAddEntry = TryGetEx(dictNew, "AddEntry", m_strAddEntry);
			m_strAddEntryDesc = TryGetEx(dictNew, "AddEntryDesc", m_strAddEntryDesc);
			m_strAddStringField = TryGetEx(dictNew, "AddStringField", m_strAddStringField);
			m_strAddStringFieldDesc = TryGetEx(dictNew, "AddStringFieldDesc", m_strAddStringFieldDesc);
			m_strAdvanced = TryGetEx(dictNew, "Advanced", m_strAdvanced);
			m_strAdvancedEntryProperties = TryGetEx(dictNew, "AdvancedEntryProperties", m_strAdvancedEntryProperties);
			m_strAdvancedEntryPropertiesDesc = TryGetEx(dictNew, "AdvancedEntryPropertiesDesc", m_strAdvancedEntryPropertiesDesc);
			m_strAfterDatabaseOpen = TryGetEx(dictNew, "AfterDatabaseOpen", m_strAfterDatabaseOpen);
			m_strAllEntriesTitle = TryGetEx(dictNew, "AllEntriesTitle", m_strAllEntriesTitle);
			m_strAllFiles = TryGetEx(dictNew, "AllFiles", m_strAllFiles);
			m_strAllSupportedFiles = TryGetEx(dictNew, "AllSupportedFiles", m_strAllSupportedFiles);
			m_strAskContinue = TryGetEx(dictNew, "AskContinue", m_strAskContinue);
			m_strAssistant = TryGetEx(dictNew, "Assistant", m_strAssistant);
			m_strAssistantTel = TryGetEx(dictNew, "AssistantTel", m_strAssistantTel);
			m_strAttachedExistsAlready = TryGetEx(dictNew, "AttachedExistsAlready", m_strAttachedExistsAlready);
			m_strAttachFailed = TryGetEx(dictNew, "AttachFailed", m_strAttachFailed);
			m_strAttachments = TryGetEx(dictNew, "Attachments", m_strAttachments);
			m_strAttachNewRename = TryGetEx(dictNew, "AttachNewRename", m_strAttachNewRename);
			m_strAttachNewRenameRemarks0 = TryGetEx(dictNew, "AttachNewRenameRemarks0", m_strAttachNewRenameRemarks0);
			m_strAttachNewRenameRemarks1 = TryGetEx(dictNew, "AttachNewRenameRemarks1", m_strAttachNewRenameRemarks1);
			m_strAttachNewRenameRemarks2 = TryGetEx(dictNew, "AttachNewRenameRemarks2", m_strAttachNewRenameRemarks2);
			m_strAuthor = TryGetEx(dictNew, "Author", m_strAuthor);
			m_strAutoOpenLastFile = TryGetEx(dictNew, "AutoOpenLastFile", m_strAutoOpenLastFile);
			m_strAutoSaveAtExit = TryGetEx(dictNew, "AutoSaveAtExit", m_strAutoSaveAtExit);
			m_strAutoShowExpiredEntries = TryGetEx(dictNew, "AutoShowExpiredEntries", m_strAutoShowExpiredEntries);
			m_strAutoShowSoonToExpireEntries = TryGetEx(dictNew, "AutoShowSoonToExpireEntries", m_strAutoShowSoonToExpireEntries);
			m_strAutoType = TryGetEx(dictNew, "AutoType", m_strAutoType);
			m_strAutoTypeEntrySelection = TryGetEx(dictNew, "AutoTypeEntrySelection", m_strAutoTypeEntrySelection);
			m_strAutoTypeEntrySelectionDescLong = TryGetEx(dictNew, "AutoTypeEntrySelectionDescLong", m_strAutoTypeEntrySelectionDescLong);
			m_strAutoTypeEntrySelectionDescShort = TryGetEx(dictNew, "AutoTypeEntrySelectionDescShort", m_strAutoTypeEntrySelectionDescShort);
			m_strAutoTypeObfuscationHint = TryGetEx(dictNew, "AutoTypeObfuscationHint", m_strAutoTypeObfuscationHint);
			m_strAvailableLanguages = TryGetEx(dictNew, "AvailableLanguages", m_strAvailableLanguages);
			m_strBankAccount = TryGetEx(dictNew, "BankAccount", m_strBankAccount);
			m_strBits = TryGetEx(dictNew, "Bits", m_strBits);
			m_strBranchCode = TryGetEx(dictNew, "BranchCode", m_strBranchCode);
			m_strBranchHours = TryGetEx(dictNew, "BranchHours", m_strBranchHours);
			m_strBranchTel = TryGetEx(dictNew, "BranchTel", m_strBranchTel);
			m_strBrowser = TryGetEx(dictNew, "Browser", m_strBrowser);
			m_strButtonBack = TryGetEx(dictNew, "ButtonBack", m_strButtonBack);
			m_strButtonFinish = TryGetEx(dictNew, "ButtonFinish", m_strButtonFinish);
			m_strButtonNext = TryGetEx(dictNew, "ButtonNext", m_strButtonNext);
			m_strCannotMoveEntriesBcsGroup = TryGetEx(dictNew, "CannotMoveEntriesBcsGroup", m_strCannotMoveEntriesBcsGroup);
			m_strCarTel = TryGetEx(dictNew, "CarTel", m_strCarTel);
			m_strChangeMasterKeyIntro = TryGetEx(dictNew, "ChangeMasterKeyIntro", m_strChangeMasterKeyIntro);
			m_strCheckForUpdAtStart = TryGetEx(dictNew, "CheckForUpdAtStart", m_strCheckForUpdAtStart);
			m_strChkForUpdGotLatest = TryGetEx(dictNew, "ChkForUpdGotLatest", m_strChkForUpdGotLatest);
			m_strChkForUpdNewVersion = TryGetEx(dictNew, "ChkForUpdNewVersion", m_strChkForUpdNewVersion);
			m_strClearMRU = TryGetEx(dictNew, "ClearMRU", m_strClearMRU);
			m_strClipboard = TryGetEx(dictNew, "Clipboard", m_strClipboard);
			m_strClipboardAutoClear = TryGetEx(dictNew, "ClipboardAutoClear", m_strClipboardAutoClear);
			m_strClipboardClearInSeconds = TryGetEx(dictNew, "ClipboardClearInSeconds", m_strClipboardClearInSeconds);
			m_strClipboardClearOnExit = TryGetEx(dictNew, "ClipboardClearOnExit", m_strClipboardClearOnExit);
			m_strCloseButton = TryGetEx(dictNew, "CloseButton", m_strCloseButton);
			m_strCloseButtonMinimizes = TryGetEx(dictNew, "CloseButtonMinimizes", m_strCloseButtonMinimizes);
			m_strCompany = TryGetEx(dictNew, "Company", m_strCompany);
			m_strComponents = TryGetEx(dictNew, "Components", m_strComponents);
			m_strConfigAffectAdmin = TryGetEx(dictNew, "ConfigAffectAdmin", m_strConfigAffectAdmin);
			m_strConfigAffectUser = TryGetEx(dictNew, "ConfigAffectUser", m_strConfigAffectUser);
			m_strConfigureAutoType = TryGetEx(dictNew, "ConfigureAutoType", m_strConfigureAutoType);
			m_strConfigureAutoTypeDesc = TryGetEx(dictNew, "ConfigureAutoTypeDesc", m_strConfigureAutoTypeDesc);
			m_strConfigureAutoTypeItem = TryGetEx(dictNew, "ConfigureAutoTypeItem", m_strConfigureAutoTypeItem);
			m_strConfigureAutoTypeItemDesc = TryGetEx(dictNew, "ConfigureAutoTypeItemDesc", m_strConfigureAutoTypeItemDesc);
			m_strConfigureKeystrokeSeq = TryGetEx(dictNew, "ConfigureKeystrokeSeq", m_strConfigureKeystrokeSeq);
			m_strConfigureKeystrokeSeqDesc = TryGetEx(dictNew, "ConfigureKeystrokeSeqDesc", m_strConfigureKeystrokeSeqDesc);
			m_strConfigureOnNewDatabase = TryGetEx(dictNew, "ConfigureOnNewDatabase", m_strConfigureOnNewDatabase);
			m_strConfigWriteGlobal = TryGetEx(dictNew, "ConfigWriteGlobal", m_strConfigWriteGlobal);
			m_strConfigWriteLocal = TryGetEx(dictNew, "ConfigWriteLocal", m_strConfigWriteLocal);
			m_strContact = TryGetEx(dictNew, "Contact", m_strContact);
			m_strCopy = TryGetEx(dictNew, "Copy", m_strCopy);
			m_strCopyAll = TryGetEx(dictNew, "CopyAll", m_strCopyAll);
			m_strCreateMasterKey = TryGetEx(dictNew, "CreateMasterKey", m_strCreateMasterKey);
			m_strCreateNewDatabase = TryGetEx(dictNew, "CreateNewDatabase", m_strCreateNewDatabase);
			m_strCreationTime = TryGetEx(dictNew, "CreationTime", m_strCreationTime);
			m_strCredSaveAll = TryGetEx(dictNew, "CredSaveAll", m_strCredSaveAll);
			m_strCredSaveNone = TryGetEx(dictNew, "CredSaveNone", m_strCredSaveNone);
			m_strCredSaveUserOnly = TryGetEx(dictNew, "CredSaveUserOnly", m_strCredSaveUserOnly);
			m_strCustom = TryGetEx(dictNew, "Custom", m_strCustom);
			m_strCustomFields = TryGetEx(dictNew, "CustomFields", m_strCustomFields);
			m_strCut = TryGetEx(dictNew, "Cut", m_strCut);
			m_strDatabase = TryGetEx(dictNew, "Database", m_strDatabase);
			m_strDatabaseDescPrompt = TryGetEx(dictNew, "DatabaseDescPrompt", m_strDatabaseDescPrompt);
			m_strDatabaseMaintenance = TryGetEx(dictNew, "DatabaseMaintenance", m_strDatabaseMaintenance);
			m_strDatabaseMaintenanceDesc = TryGetEx(dictNew, "DatabaseMaintenanceDesc", m_strDatabaseMaintenanceDesc);
			m_strDatabaseModified = TryGetEx(dictNew, "DatabaseModified", m_strDatabaseModified);
			m_strDatabaseNamePrompt = TryGetEx(dictNew, "DatabaseNamePrompt", m_strDatabaseNamePrompt);
			m_strDatabaseSettings = TryGetEx(dictNew, "DatabaseSettings", m_strDatabaseSettings);
			m_strDatabaseSettingsDesc = TryGetEx(dictNew, "DatabaseSettingsDesc", m_strDatabaseSettingsDesc);
			m_strDecompressing = TryGetEx(dictNew, "Decompressing", m_strDecompressing);
			m_strDefault = TryGetEx(dictNew, "Default", m_strDefault);
			m_strDelete = TryGetEx(dictNew, "Delete", m_strDelete);
			m_strDeleteEntriesCaption = TryGetEx(dictNew, "DeleteEntriesCaption", m_strDeleteEntriesCaption);
			m_strDeleteEntriesInfo = TryGetEx(dictNew, "DeleteEntriesInfo", m_strDeleteEntriesInfo);
			m_strDeleteEntriesQuestion = TryGetEx(dictNew, "DeleteEntriesQuestion", m_strDeleteEntriesQuestion);
			m_strDeleteGroupCaption = TryGetEx(dictNew, "DeleteGroupCaption", m_strDeleteGroupCaption);
			m_strDeleteGroupInfo = TryGetEx(dictNew, "DeleteGroupInfo", m_strDeleteGroupInfo);
			m_strDeleteGroupQuestion = TryGetEx(dictNew, "DeleteGroupQuestion", m_strDeleteGroupQuestion);
			m_strDepartment = TryGetEx(dictNew, "Department", m_strDepartment);
			m_strDescription = TryGetEx(dictNew, "Description", m_strDescription);
			m_strDetails = TryGetEx(dictNew, "Details", m_strDetails);
			m_strDocumentationHint = TryGetEx(dictNew, "DocumentationHint", m_strDocumentationHint);
			m_strDownloadAndInstall = TryGetEx(dictNew, "DownloadAndInstall", m_strDownloadAndInstall);
			m_strDownloadFailed = TryGetEx(dictNew, "DownloadFailed", m_strDownloadFailed);
			m_strDownloading = TryGetEx(dictNew, "Downloading", m_strDownloading);
			m_strDragDrop = TryGetEx(dictNew, "DragDrop", m_strDragDrop);
			m_strDuplicateStringFieldName = TryGetEx(dictNew, "DuplicateStringFieldName", m_strDuplicateStringFieldName);
			m_strEditEntry = TryGetEx(dictNew, "EditEntry", m_strEditEntry);
			m_strEditEntryDesc = TryGetEx(dictNew, "EditEntryDesc", m_strEditEntryDesc);
			m_strEditGroup = TryGetEx(dictNew, "EditGroup", m_strEditGroup);
			m_strEditGroupDesc = TryGetEx(dictNew, "EditGroupDesc", m_strEditGroupDesc);
			m_strEditStringField = TryGetEx(dictNew, "EditStringField", m_strEditStringField);
			m_strEditStringFieldDesc = TryGetEx(dictNew, "EditStringFieldDesc", m_strEditStringFieldDesc);
			m_strEMail = TryGetEx(dictNew, "EMail", m_strEMail);
			m_strEmpty = TryGetEx(dictNew, "Empty", m_strEmpty);
			m_strEnterCompositeKey = TryGetEx(dictNew, "EnterCompositeKey", m_strEnterCompositeKey);
			m_strEntropyDesc = TryGetEx(dictNew, "EntropyDesc", m_strEntropyDesc);
			m_strEntropyTitle = TryGetEx(dictNew, "EntropyTitle", m_strEntropyTitle);
			m_strEntry = TryGetEx(dictNew, "Entry", m_strEntry);
			m_strEntryList = TryGetEx(dictNew, "EntryList", m_strEntryList);
			m_strErrorCode = TryGetEx(dictNew, "ErrorCode", m_strErrorCode);
			m_strErrors = TryGetEx(dictNew, "Errors", m_strErrors);
			m_strExpiredEntries = TryGetEx(dictNew, "ExpiredEntries", m_strExpiredEntries);
			m_strExpiryTime = TryGetEx(dictNew, "ExpiryTime", m_strExpiryTime);
			m_strExport = TryGetEx(dictNew, "Export", m_strExport);
			m_strExportHTML = TryGetEx(dictNew, "ExportHTML", m_strExportHTML);
			m_strExportHTMLDesc = TryGetEx(dictNew, "ExportHTMLDesc", m_strExportHTMLDesc);
			m_strExportingStatusMsg = TryGetEx(dictNew, "ExportingStatusMsg", m_strExportingStatusMsg);
			m_strFatalError = TryGetEx(dictNew, "FatalError", m_strFatalError);
			m_strFieldName = TryGetEx(dictNew, "FieldName", m_strFieldName);
			m_strFieldNameExistsAlready = TryGetEx(dictNew, "FieldNameExistsAlready", m_strFieldNameExistsAlready);
			m_strFieldNameInvalid = TryGetEx(dictNew, "FieldNameInvalid", m_strFieldNameInvalid);
			m_strFieldValue = TryGetEx(dictNew, "FieldValue", m_strFieldValue);
			m_strFileExistsAlready = TryGetEx(dictNew, "FileExistsAlready", m_strFileExistsAlready);
			m_strFileExtInstallFailed = TryGetEx(dictNew, "FileExtInstallFailed", m_strFileExtInstallFailed);
			m_strFileExtInstallSuccess = TryGetEx(dictNew, "FileExtInstallSuccess", m_strFileExtInstallSuccess);
			m_strFileExtName = TryGetEx(dictNew, "FileExtName", m_strFileExtName);
			m_strFileLockedBy = TryGetEx(dictNew, "FileLockedBy", m_strFileLockedBy);
			m_strFileLockedWarning = TryGetEx(dictNew, "FileLockedWarning", m_strFileLockedWarning);
			m_strFileNameContainsSemicolonError = TryGetEx(dictNew, "FileNameContainsSemicolonError", m_strFileNameContainsSemicolonError);
			m_strFileNotFoundError = TryGetEx(dictNew, "FileNotFoundError", m_strFileNotFoundError);
			m_strFiles = TryGetEx(dictNew, "Files", m_strFiles);
			m_strFlag = TryGetEx(dictNew, "Flag", m_strFlag);
			m_strFormatNoDbDesc = TryGetEx(dictNew, "FormatNoDbDesc", m_strFormatNoDbDesc);
			m_strFormatNoDbName = TryGetEx(dictNew, "FormatNoDbName", m_strFormatNoDbName);
			m_strFormatNoRootEntries = TryGetEx(dictNew, "FormatNoRootEntries", m_strFormatNoRootEntries);
			m_strFormatNoSubGroupsInRoot = TryGetEx(dictNew, "FormatNoSubGroupsInRoot", m_strFormatNoSubGroupsInRoot);
			m_strFormatOnlyOneAttachment = TryGetEx(dictNew, "FormatOnlyOneAttachment", m_strFormatOnlyOneAttachment);
			m_strFrequency = TryGetEx(dictNew, "Frequency", m_strFrequency);
			m_strGeneral = TryGetEx(dictNew, "General", m_strGeneral);
			m_strGenerateCount = TryGetEx(dictNew, "GenerateCount", m_strGenerateCount);
			m_strGenerateCountDesc = TryGetEx(dictNew, "GenerateCountDesc", m_strGenerateCountDesc);
			m_strGenerateCountLongDesc = TryGetEx(dictNew, "GenerateCountLongDesc", m_strGenerateCountLongDesc);
			m_strGeneratedPasswordSamples = TryGetEx(dictNew, "GeneratedPasswordSamples", m_strGeneratedPasswordSamples);
			m_strGenProfileAdd = TryGetEx(dictNew, "GenProfileAdd", m_strGenProfileAdd);
			m_strGenProfileAddDesc = TryGetEx(dictNew, "GenProfileAddDesc", m_strGenProfileAddDesc);
			m_strGenProfileAddDescLong = TryGetEx(dictNew, "GenProfileAddDescLong", m_strGenProfileAddDescLong);
			m_strGenPwBasedOnPrevious = TryGetEx(dictNew, "GenPwBasedOnPrevious", m_strGenPwBasedOnPrevious);
			m_strGenRandomPwForNewEntry = TryGetEx(dictNew, "GenRandomPwForNewEntry", m_strGenRandomPwForNewEntry);
			m_strGroup = TryGetEx(dictNew, "Group", m_strGroup);
			m_strGroupCannotStoreEntries = TryGetEx(dictNew, "GroupCannotStoreEntries", m_strGroupCannotStoreEntries);
			m_strHelpSourceNoLocalOption = TryGetEx(dictNew, "HelpSourceNoLocalOption", m_strHelpSourceNoLocalOption);
			m_strHelpSourceSelection = TryGetEx(dictNew, "HelpSourceSelection", m_strHelpSourceSelection);
			m_strHelpSourceSelectionDesc = TryGetEx(dictNew, "HelpSourceSelectionDesc", m_strHelpSourceSelectionDesc);
			m_strHomeAddress = TryGetEx(dictNew, "HomeAddress", m_strHomeAddress);
			m_strHomebanking = TryGetEx(dictNew, "Homebanking", m_strHomebanking);
			m_strHomeFax = TryGetEx(dictNew, "HomeFax", m_strHomeFax);
			m_strHomepageVisitQuestion = TryGetEx(dictNew, "HomepageVisitQuestion", m_strHomepageVisitQuestion);
			m_strHomeTel = TryGetEx(dictNew, "HomeTel", m_strHomeTel);
			m_strIBAN = TryGetEx(dictNew, "IBAN", m_strIBAN);
			m_strImageViewer = TryGetEx(dictNew, "ImageViewer", m_strImageViewer);
			m_strImport = TryGetEx(dictNew, "Import", m_strImport);
			m_strImportBehavior = TryGetEx(dictNew, "ImportBehavior", m_strImportBehavior);
			m_strImportBehaviorDesc = TryGetEx(dictNew, "ImportBehaviorDesc", m_strImportBehaviorDesc);
			m_strImportFailed = TryGetEx(dictNew, "ImportFailed", m_strImportFailed);
			m_strImportFileDesc = TryGetEx(dictNew, "ImportFileDesc", m_strImportFileDesc);
			m_strImportFileTitle = TryGetEx(dictNew, "ImportFileTitle", m_strImportFileTitle);
			m_strImportFinished = TryGetEx(dictNew, "ImportFinished", m_strImportFinished);
			m_strImportingStatusMsg = TryGetEx(dictNew, "ImportingStatusMsg", m_strImportingStatusMsg);
			m_strImportMustRead = TryGetEx(dictNew, "ImportMustRead", m_strImportMustRead);
			m_strImportMustReadQuestion = TryGetEx(dictNew, "ImportMustReadQuestion", m_strImportMustReadQuestion);
			m_strInternet = TryGetEx(dictNew, "Internet", m_strInternet);
			m_strInvalidFileStructure = TryGetEx(dictNew, "InvalidFileStructure", m_strInvalidFileStructure);
			m_strInvalidKey = TryGetEx(dictNew, "InvalidKey", m_strInvalidKey);
			m_strInvalidUrl = TryGetEx(dictNew, "InvalidUrl", m_strInvalidUrl);
			m_strInvalidUserPassword = TryGetEx(dictNew, "InvalidUserPassword", m_strInvalidUserPassword);
			m_strJobTitle = TryGetEx(dictNew, "JobTitle", m_strJobTitle);
			m_strKDB3KeePassLibC = TryGetEx(dictNew, "KDB3KeePassLibC", m_strKDB3KeePassLibC);
			m_strKeePassLibCLong = TryGetEx(dictNew, "KeePassLibCLong", m_strKeePassLibCLong);
			m_strKeePassLibCNotFound = TryGetEx(dictNew, "KeePassLibCNotFound", m_strKeePassLibCNotFound);
			m_strKeyFileError = TryGetEx(dictNew, "KeyFileError", m_strKeyFileError);
			m_strKeystrokeSequence = TryGetEx(dictNew, "KeystrokeSequence", m_strKeystrokeSequence);
			m_strLanguageSelected = TryGetEx(dictNew, "LanguageSelected", m_strLanguageSelected);
			m_strLastAccessTime = TryGetEx(dictNew, "LastAccessTime", m_strLastAccessTime);
			m_strLastModificationTime = TryGetEx(dictNew, "LastModificationTime", m_strLastModificationTime);
			m_strLimitSingleInstance = TryGetEx(dictNew, "LimitSingleInstance", m_strLimitSingleInstance);
			m_strLockMenuLock = TryGetEx(dictNew, "LockMenuLock", m_strLockMenuLock);
			m_strLockMenuUnlock = TryGetEx(dictNew, "LockMenuUnlock", m_strLockMenuUnlock);
			m_strLockOnMinimize = TryGetEx(dictNew, "LockOnMinimize", m_strLockOnMinimize);
			m_strLockOnSessionLock = TryGetEx(dictNew, "LockOnSessionLock", m_strLockOnSessionLock);
			m_strMainWindow = TryGetEx(dictNew, "MainWindow", m_strMainWindow);
			m_strMasterKeyChanged = TryGetEx(dictNew, "MasterKeyChanged", m_strMasterKeyChanged);
			m_strMasterKeyChangedSavePrompt = TryGetEx(dictNew, "MasterKeyChangedSavePrompt", m_strMasterKeyChangedSavePrompt);
			m_strMenus = TryGetEx(dictNew, "Menus", m_strMenus);
			m_strMergingData = TryGetEx(dictNew, "MergingData", m_strMergingData);
			m_strMinBalance = TryGetEx(dictNew, "MinBalance", m_strMinBalance);
			m_strMinimizeAfterCopy = TryGetEx(dictNew, "MinimizeAfterCopy", m_strMinimizeAfterCopy);
			m_strMinimizeToTray = TryGetEx(dictNew, "MinimizeToTray", m_strMinimizeToTray);
			m_strMobileTel = TryGetEx(dictNew, "MobileTel", m_strMobileTel);
			m_strNativeLibUse = TryGetEx(dictNew, "NativeLibUse", m_strNativeLibUse);
			m_strNavigation = TryGetEx(dictNew, "Navigation", m_strNavigation);
			m_strNetwork = TryGetEx(dictNew, "Network", m_strNetwork);
			m_strNeverExpires = TryGetEx(dictNew, "NeverExpires", m_strNeverExpires);
			m_strNewDatabaseKDBFileName = TryGetEx(dictNew, "NewDatabaseKDBFileName", m_strNewDatabaseKDBFileName);
			m_strNewGroup = TryGetEx(dictNew, "NewGroup", m_strNewGroup);
			m_strNoFileAccessRead = TryGetEx(dictNew, "NoFileAccessRead", m_strNoFileAccessRead);
			m_strNoKeyFileSpecifiedMeta = TryGetEx(dictNew, "NoKeyFileSpecifiedMeta", m_strNoKeyFileSpecifiedMeta);
			m_strNone = TryGetEx(dictNew, "None", m_strNone);
			m_strNotes = TryGetEx(dictNew, "Notes", m_strNotes);
			m_strNotInstalled = TryGetEx(dictNew, "NotInstalled", m_strNotInstalled);
			m_strNoXSLFile = TryGetEx(dictNew, "NoXSLFile", m_strNoXSLFile);
			m_strOfLower = TryGetEx(dictNew, "OfLower", m_strOfLower);
			m_strOpeningDatabase = TryGetEx(dictNew, "OpeningDatabase", m_strOpeningDatabase);
			m_strOptions = TryGetEx(dictNew, "Options", m_strOptions);
			m_strOptionsDesc = TryGetEx(dictNew, "OptionsDesc", m_strOptionsDesc);
			m_strOtherPlaceholders = TryGetEx(dictNew, "OtherPlaceholders", m_strOtherPlaceholders);
			m_strOverwriteExistingFileQuestion = TryGetEx(dictNew, "OverwriteExistingFileQuestion", m_strOverwriteExistingFileQuestion);
			m_strPager = TryGetEx(dictNew, "Pager", m_strPager);
			m_strPassword = TryGetEx(dictNew, "Password", m_strPassword);
			m_strPasswordManagers = TryGetEx(dictNew, "PasswordManagers", m_strPasswordManagers);
			m_strPasswordOptions = TryGetEx(dictNew, "PasswordOptions", m_strPasswordOptions);
			m_strPasswordOptionsDesc = TryGetEx(dictNew, "PasswordOptionsDesc", m_strPasswordOptionsDesc);
			m_strPasswordPrompt = TryGetEx(dictNew, "PasswordPrompt", m_strPasswordPrompt);
			m_strPasswordRepeatFailed = TryGetEx(dictNew, "PasswordRepeatFailed", m_strPasswordRepeatFailed);
			m_strPaste = TryGetEx(dictNew, "Paste", m_strPaste);
			m_strPluginFailedToLoad = TryGetEx(dictNew, "PluginFailedToLoad", m_strPluginFailedToLoad);
			m_strPlugins = TryGetEx(dictNew, "Plugins", m_strPlugins);
			m_strPluginsDesc = TryGetEx(dictNew, "PluginsDesc", m_strPluginsDesc);
			m_strPolicyAutoTypeDesc = TryGetEx(dictNew, "PolicyAutoTypeDesc", m_strPolicyAutoTypeDesc);
			m_strPolicyClipboardDesc = TryGetEx(dictNew, "PolicyClipboardDesc", m_strPolicyClipboardDesc);
			m_strPolicyDisallowed = TryGetEx(dictNew, "PolicyDisallowed", m_strPolicyDisallowed);
			m_strPolicyDragDropDesc = TryGetEx(dictNew, "PolicyDragDropDesc", m_strPolicyDragDropDesc);
			m_strPolicyExportDesc = TryGetEx(dictNew, "PolicyExportDesc", m_strPolicyExportDesc);
			m_strPolicyImportDesc = TryGetEx(dictNew, "PolicyImportDesc", m_strPolicyImportDesc);
			m_strPolicyPluginsDesc = TryGetEx(dictNew, "PolicyPluginsDesc", m_strPolicyPluginsDesc);
			m_strPolicyPrintDesc = TryGetEx(dictNew, "PolicyPrintDesc", m_strPolicyPrintDesc);
			m_strPolicyRequiredFlag = TryGetEx(dictNew, "PolicyRequiredFlag", m_strPolicyRequiredFlag);
			m_strPolicySaveDatabaseDesc = TryGetEx(dictNew, "PolicySaveDatabaseDesc", m_strPolicySaveDatabaseDesc);
			m_strPrint = TryGetEx(dictNew, "Print", m_strPrint);
			m_strPrintDesc = TryGetEx(dictNew, "PrintDesc", m_strPrintDesc);
			m_strQuickSearch = TryGetEx(dictNew, "QuickSearch", m_strQuickSearch);
			m_strReady = TryGetEx(dictNew, "Ready", m_strReady);
			m_strRememberHidingSettings = TryGetEx(dictNew, "RememberHidingSettings", m_strRememberHidingSettings);
			m_strRestartKeePassQuestion = TryGetEx(dictNew, "RestartKeePassQuestion", m_strRestartKeePassQuestion);
			m_strRoutingCode = TryGetEx(dictNew, "RoutingCode", m_strRoutingCode);
			m_strSampleEntry = TryGetEx(dictNew, "SampleEntry", m_strSampleEntry);
			m_strSaveBeforeCloseQuestion = TryGetEx(dictNew, "SaveBeforeCloseQuestion", m_strSaveBeforeCloseQuestion);
			m_strSaveBeforeCloseTitle = TryGetEx(dictNew, "SaveBeforeCloseTitle", m_strSaveBeforeCloseTitle);
			m_strSaveDatabase = TryGetEx(dictNew, "SaveDatabase", m_strSaveDatabase);
			m_strSavingDatabase = TryGetEx(dictNew, "SavingDatabase", m_strSavingDatabase);
			m_strSearchDesc = TryGetEx(dictNew, "SearchDesc", m_strSearchDesc);
			m_strSearchGroupName = TryGetEx(dictNew, "SearchGroupName", m_strSearchGroupName);
			m_strSearchItemsFoundSmall = TryGetEx(dictNew, "SearchItemsFoundSmall", m_strSearchItemsFoundSmall);
			m_strSearchKeyFilesOnRemovable = TryGetEx(dictNew, "SearchKeyFilesOnRemovable", m_strSearchKeyFilesOnRemovable);
			m_strSearchResultsInSeparator = TryGetEx(dictNew, "SearchResultsInSeparator", m_strSearchResultsInSeparator);
			m_strSearchTitle = TryGetEx(dictNew, "SearchTitle", m_strSearchTitle);
			m_strSelectAll = TryGetEx(dictNew, "SelectAll", m_strSelectAll);
			m_strSelectDifferentGroup = TryGetEx(dictNew, "SelectDifferentGroup", m_strSelectDifferentGroup);
			m_strSelectedLower = TryGetEx(dictNew, "SelectedLower", m_strSelectedLower);
			m_strSelectLanguage = TryGetEx(dictNew, "SelectLanguage", m_strSelectLanguage);
			m_strSelectLanguageDesc = TryGetEx(dictNew, "SelectLanguageDesc", m_strSelectLanguageDesc);
			m_strSelfTestFailed = TryGetEx(dictNew, "SelfTestFailed", m_strSelfTestFailed);
			m_strShowFullPathInTitleBar = TryGetEx(dictNew, "ShowFullPathInTitleBar", m_strShowFullPathInTitleBar);
			m_strShowGridLines = TryGetEx(dictNew, "ShowGridLines", m_strShowGridLines);
			m_strShowTrayOnlyIfTrayed = TryGetEx(dictNew, "ShowTrayOnlyIfTrayed", m_strShowTrayOnlyIfTrayed);
			m_strSoonToExpireEntries = TryGetEx(dictNew, "SoonToExpireEntries", m_strSoonToExpireEntries);
			m_strSortCode = TryGetEx(dictNew, "SortCode", m_strSortCode);
			m_strSpecialKeys = TryGetEx(dictNew, "SpecialKeys", m_strSpecialKeys);
			m_strSpecifyApplication = TryGetEx(dictNew, "SpecifyApplication", m_strSpecifyApplication);
			m_strSpecifyApplicationDesc = TryGetEx(dictNew, "SpecifyApplicationDesc", m_strSpecifyApplicationDesc);
			m_strStandardFields = TryGetEx(dictNew, "StandardFields", m_strStandardFields);
			m_strStartAndExit = TryGetEx(dictNew, "StartAndExit", m_strStartAndExit);
			m_strSuccess = TryGetEx(dictNew, "Success", m_strSuccess);
			m_strSWIFTCode = TryGetEx(dictNew, "SWIFTCode", m_strSWIFTCode);
			m_strSyncFailed = TryGetEx(dictNew, "SyncFailed", m_strSyncFailed);
			m_strSynchronize = TryGetEx(dictNew, "Synchronize", m_strSynchronize);
			m_strSynchronizingHint = TryGetEx(dictNew, "SynchronizingHint", m_strSynchronizingHint);
			m_strSyncSuccess = TryGetEx(dictNew, "SyncSuccess", m_strSyncSuccess);
			m_strSystem = TryGetEx(dictNew, "System", m_strSystem);
			m_strSystemCodepage = TryGetEx(dictNew, "SystemCodepage", m_strSystemCodepage);
			m_strTAccountInfoTel = TryGetEx(dictNew, "TAccountInfoTel", m_strTAccountInfoTel);
			m_strTAccountNames = TryGetEx(dictNew, "TAccountNames", m_strTAccountNames);
			m_strTAccountNumber = TryGetEx(dictNew, "TAccountNumber", m_strTAccountNumber);
			m_strTAccountType = TryGetEx(dictNew, "TAccountType", m_strTAccountType);
			m_strTANWizard = TryGetEx(dictNew, "TANWizard", m_strTANWizard);
			m_strTANWizardDesc = TryGetEx(dictNew, "TANWizardDesc", m_strTANWizardDesc);
			m_strTargetWindow = TryGetEx(dictNew, "TargetWindow", m_strTargetWindow);
			m_strTextViewer = TryGetEx(dictNew, "TextViewer", m_strTextViewer);
			m_strTitle = TryGetEx(dictNew, "Title", m_strTitle);
			m_strTooManyFilesError = TryGetEx(dictNew, "TooManyFilesError", m_strTooManyFilesError);
			m_strTransfersOf = TryGetEx(dictNew, "TransfersOf", m_strTransfersOf);
			m_strUndo = TryGetEx(dictNew, "Undo", m_strUndo);
			m_strUnknown = TryGetEx(dictNew, "Unknown", m_strUnknown);
			m_strUnknownError = TryGetEx(dictNew, "UnknownError", m_strUnknownError);
			m_strUnknownFileVersion = TryGetEx(dictNew, "UnknownFileVersion", m_strUnknownFileVersion);
			m_strURL = TryGetEx(dictNew, "URL", m_strURL);
			m_strUrlOpenDesc = TryGetEx(dictNew, "UrlOpenDesc", m_strUrlOpenDesc);
			m_strUrlOpenTitle = TryGetEx(dictNew, "UrlOpenTitle", m_strUrlOpenTitle);
			m_strUrlOverride = TryGetEx(dictNew, "UrlOverride", m_strUrlOverride);
			m_strUrlSaveDesc = TryGetEx(dictNew, "UrlSaveDesc", m_strUrlSaveDesc);
			m_strUrlSaveTitle = TryGetEx(dictNew, "UrlSaveTitle", m_strUrlSaveTitle);
			m_strUrlStarterCustomText = TryGetEx(dictNew, "UrlStarterCustomText", m_strUrlStarterCustomText);
			m_strUrlStarterFailCaption = TryGetEx(dictNew, "UrlStarterFailCaption", m_strUrlStarterFailCaption);
			m_strUrlStarterNotFoundPost = TryGetEx(dictNew, "UrlStarterNotFoundPost", m_strUrlStarterNotFoundPost);
			m_strUrlStarterNotFoundPre = TryGetEx(dictNew, "UrlStarterNotFoundPre", m_strUrlStarterNotFoundPre);
			m_strUrlStarterUsesDefault = TryGetEx(dictNew, "UrlStarterUsesDefault", m_strUrlStarterUsesDefault);
			m_strUserName = TryGetEx(dictNew, "UserName", m_strUserName);
			m_strUserNamePrompt = TryGetEx(dictNew, "UserNamePrompt", m_strUserNamePrompt);
			m_strUUID = TryGetEx(dictNew, "UUID", m_strUUID);
			m_strVersion = TryGetEx(dictNew, "Version", m_strVersion);
			m_strViewEntry = TryGetEx(dictNew, "ViewEntry", m_strViewEntry);
			m_strViewEntryDesc = TryGetEx(dictNew, "ViewEntryDesc", m_strViewEntryDesc);
			m_strWarnings = TryGetEx(dictNew, "Warnings", m_strWarnings);
			m_strWebBrowser = TryGetEx(dictNew, "WebBrowser", m_strWebBrowser);
			m_strWebPage = TryGetEx(dictNew, "WebPage", m_strWebPage);
			m_strWebSiteLogin = TryGetEx(dictNew, "WebSiteLogin", m_strWebSiteLogin);
			m_strWebSites = TryGetEx(dictNew, "WebSites", m_strWebSites);
			m_strWindowsOS = TryGetEx(dictNew, "WindowsOS", m_strWindowsOS);
			m_strWorkFax = TryGetEx(dictNew, "WorkFax", m_strWorkFax);
			m_strWorkspaceLocked = TryGetEx(dictNew, "WorkspaceLocked", m_strWorkspaceLocked);
			m_strWorkTel = TryGetEx(dictNew, "WorkTel", m_strWorkTel);
			m_strXSLStylesheets = TryGetEx(dictNew, "XSLStylesheets", m_strXSLStylesheets);
		}

		private static string m_strAddEntry =
			@"Add Entry";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Add Entry'.
		/// </summary>
		public static string AddEntry
		{
			get { return m_strAddEntry; }
		}

		private static string m_strAddEntryDesc =
			@"Add a new password entry to the database.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Add a new password entry to the database.'.
		/// </summary>
		public static string AddEntryDesc
		{
			get { return m_strAddEntryDesc; }
		}

		private static string m_strAddStringField =
			@"Add Entry String Field";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Add Entry String Field'.
		/// </summary>
		public static string AddStringField
		{
			get { return m_strAddStringField; }
		}

		private static string m_strAddStringFieldDesc =
			@"Add a new string field to the current entry.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Add a new string field to the current entry.'.
		/// </summary>
		public static string AddStringFieldDesc
		{
			get { return m_strAddStringFieldDesc; }
		}

		private static string m_strAdvanced =
			@"Advanced";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Advanced'.
		/// </summary>
		public static string Advanced
		{
			get { return m_strAdvanced; }
		}

		private static string m_strAdvancedEntryProperties =
			@"Entry Properties";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Entry Properties'.
		/// </summary>
		public static string AdvancedEntryProperties
		{
			get { return m_strAdvancedEntryProperties; }
		}

		private static string m_strAdvancedEntryPropertiesDesc =
			@"Here you can configure advanced properties of the entry.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Here you can configure advanced properties of the entry.'.
		/// </summary>
		public static string AdvancedEntryPropertiesDesc
		{
			get { return m_strAdvancedEntryPropertiesDesc; }
		}

		private static string m_strAfterDatabaseOpen =
			@"After opening a database";
		/// <summary>
		/// Look up a localized string similar to
		/// 'After opening a database'.
		/// </summary>
		public static string AfterDatabaseOpen
		{
			get { return m_strAfterDatabaseOpen; }
		}

		private static string m_strAllEntriesTitle =
			@"All Entries";
		/// <summary>
		/// Look up a localized string similar to
		/// 'All Entries'.
		/// </summary>
		public static string AllEntriesTitle
		{
			get { return m_strAllEntriesTitle; }
		}

		private static string m_strAllFiles =
			@"All Files";
		/// <summary>
		/// Look up a localized string similar to
		/// 'All Files'.
		/// </summary>
		public static string AllFiles
		{
			get { return m_strAllFiles; }
		}

		private static string m_strAllSupportedFiles =
			@"All Supported Files";
		/// <summary>
		/// Look up a localized string similar to
		/// 'All Supported Files'.
		/// </summary>
		public static string AllSupportedFiles
		{
			get { return m_strAllSupportedFiles; }
		}

		private static string m_strAskContinue =
			@"Do you want to continue?";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Do you want to continue?'.
		/// </summary>
		public static string AskContinue
		{
			get { return m_strAskContinue; }
		}

		private static string m_strAssistant =
			@"Assistant";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Assistant'.
		/// </summary>
		public static string Assistant
		{
			get { return m_strAssistant; }
		}

		private static string m_strAssistantTel =
			@"AssistantTel";
		/// <summary>
		/// Look up a localized string similar to
		/// 'AssistantTel'.
		/// </summary>
		public static string AssistantTel
		{
			get { return m_strAssistantTel; }
		}

		private static string m_strAttachedExistsAlready =
			@"The following file has already been attached to the current entry:";
		/// <summary>
		/// Look up a localized string similar to
		/// 'The following file has already been attached to the current entry:'.
		/// </summary>
		public static string AttachedExistsAlready
		{
			get { return m_strAttachedExistsAlready; }
		}

		private static string m_strAttachFailed =
			@"Failed to attach file:";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Failed to attach file:'.
		/// </summary>
		public static string AttachFailed
		{
			get { return m_strAttachFailed; }
		}

		private static string m_strAttachments =
			@"Attachments";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Attachments'.
		/// </summary>
		public static string Attachments
		{
			get { return m_strAttachments; }
		}

		private static string m_strAttachNewRename =
			@"Do you wish to rename the new file or overwrite the existing attached file?";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Do you wish to rename the new file or overwrite the existing attached file?'.
		/// </summary>
		public static string AttachNewRename
		{
			get { return m_strAttachNewRename; }
		}

		private static string m_strAttachNewRenameRemarks0 =
			@"Click [Yes] to rename the new file.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Click [Yes] to rename the new file.'.
		/// </summary>
		public static string AttachNewRenameRemarks0
		{
			get { return m_strAttachNewRenameRemarks0; }
		}

		private static string m_strAttachNewRenameRemarks1 =
			@"Click [No] to overwrite the existing attached file.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Click [No] to overwrite the existing attached file.'.
		/// </summary>
		public static string AttachNewRenameRemarks1
		{
			get { return m_strAttachNewRenameRemarks1; }
		}

		private static string m_strAttachNewRenameRemarks2 =
			@"Click [Cancel] to skip this file.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Click [Cancel] to skip this file.'.
		/// </summary>
		public static string AttachNewRenameRemarks2
		{
			get { return m_strAttachNewRenameRemarks2; }
		}

		private static string m_strAuthor =
			@"Author";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Author'.
		/// </summary>
		public static string Author
		{
			get { return m_strAuthor; }
		}

		private static string m_strAutoOpenLastFile =
			@"Automatically open last used database on startup";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Automatically open last used database on startup'.
		/// </summary>
		public static string AutoOpenLastFile
		{
			get { return m_strAutoOpenLastFile; }
		}

		private static string m_strAutoSaveAtExit =
			@"Automatically save database on exit and workspace locking";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Automatically save database on exit and workspace locking'.
		/// </summary>
		public static string AutoSaveAtExit
		{
			get { return m_strAutoSaveAtExit; }
		}

		private static string m_strAutoShowExpiredEntries =
			@"Show expired entries (if any)";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Show expired entries (if any)'.
		/// </summary>
		public static string AutoShowExpiredEntries
		{
			get { return m_strAutoShowExpiredEntries; }
		}

		private static string m_strAutoShowSoonToExpireEntries =
			@"Show entries that will expire soon (if any)";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Show entries that will expire soon (if any)'.
		/// </summary>
		public static string AutoShowSoonToExpireEntries
		{
			get { return m_strAutoShowSoonToExpireEntries; }
		}

		private static string m_strAutoType =
			@"Auto-Type";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Auto-Type'.
		/// </summary>
		public static string AutoType
		{
			get { return m_strAutoType; }
		}

		private static string m_strAutoTypeEntrySelection =
			@"Auto-Type Entry Selection";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Auto-Type Entry Selection'.
		/// </summary>
		public static string AutoTypeEntrySelection
		{
			get { return m_strAutoTypeEntrySelection; }
		}

		private static string m_strAutoTypeEntrySelectionDescLong =
			@"Multiple entries have been found for the currently active window. Please select the entry, which you want to auto-type into the active window.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Multiple entries have been found for the currently active window. Please select the entry, which you want to auto-type into the active window.'.
		/// </summary>
		public static string AutoTypeEntrySelectionDescLong
		{
			get { return m_strAutoTypeEntrySelectionDescLong; }
		}

		private static string m_strAutoTypeEntrySelectionDescShort =
			@"Multiple entries exist for the current window.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Multiple entries exist for the current window.'.
		/// </summary>
		public static string AutoTypeEntrySelectionDescShort
		{
			get { return m_strAutoTypeEntrySelectionDescShort; }
		}

		private static string m_strAutoTypeObfuscationHint =
			@"Auto-type obfuscation may not work with all windows.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Auto-type obfuscation may not work with all windows.'.
		/// </summary>
		public static string AutoTypeObfuscationHint
		{
			get { return m_strAutoTypeObfuscationHint; }
		}

		private static string m_strAvailableLanguages =
			@"Available Languages";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Available Languages'.
		/// </summary>
		public static string AvailableLanguages
		{
			get { return m_strAvailableLanguages; }
		}

		private static string m_strBankAccount =
			@"Bank Account";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Bank Account'.
		/// </summary>
		public static string BankAccount
		{
			get { return m_strBankAccount; }
		}

		private static string m_strBits =
			@"Bits";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Bits'.
		/// </summary>
		public static string Bits
		{
			get { return m_strBits; }
		}

		private static string m_strBranchCode =
			@"Branch Code";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Branch Code'.
		/// </summary>
		public static string BranchCode
		{
			get { return m_strBranchCode; }
		}

		private static string m_strBranchHours =
			@"Branch Hours";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Branch Hours'.
		/// </summary>
		public static string BranchHours
		{
			get { return m_strBranchHours; }
		}

		private static string m_strBranchTel =
			@"Branch Tel.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Branch Tel.'.
		/// </summary>
		public static string BranchTel
		{
			get { return m_strBranchTel; }
		}

		private static string m_strBrowser =
			@"Browser";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Browser'.
		/// </summary>
		public static string Browser
		{
			get { return m_strBrowser; }
		}

		private static string m_strButtonBack =
			@"< &Back";
		/// <summary>
		/// Look up a localized string similar to
		/// '< &Back'.
		/// </summary>
		public static string ButtonBack
		{
			get { return m_strButtonBack; }
		}

		private static string m_strButtonFinish =
			@"&Finish";
		/// <summary>
		/// Look up a localized string similar to
		/// '&Finish'.
		/// </summary>
		public static string ButtonFinish
		{
			get { return m_strButtonFinish; }
		}

		private static string m_strButtonNext =
			@"&Next >";
		/// <summary>
		/// Look up a localized string similar to
		/// '&Next >'.
		/// </summary>
		public static string ButtonNext
		{
			get { return m_strButtonNext; }
		}

		private static string m_strCannotMoveEntriesBcsGroup =
			@"Cannot move entries because they aren't stored in the same group.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Cannot move entries because they aren't stored in the same group.'.
		/// </summary>
		public static string CannotMoveEntriesBcsGroup
		{
			get { return m_strCannotMoveEntriesBcsGroup; }
		}

		private static string m_strCarTel =
			@"Car Tel.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Car Tel.'.
		/// </summary>
		public static string CarTel
		{
			get { return m_strCarTel; }
		}

		private static string m_strChangeMasterKeyIntro =
			@"You are changing the composite master key for the currently-open database. After you do this, only the new composite master key will open the database; the old composite master key will no longer work. You need to save the database in order to apply the new composite master key.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'You are changing the composite master key for the currently-open database. After you do this, only the new composite master key will open the database; the old composite master key will no longer work. You need to save the database in order to apply the new composite master key.'.
		/// </summary>
		public static string ChangeMasterKeyIntro
		{
			get { return m_strChangeMasterKeyIntro; }
		}

		private static string m_strCheckForUpdAtStart =
			@"Check for update at KeePass startup";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Check for update at KeePass startup'.
		/// </summary>
		public static string CheckForUpdAtStart
		{
			get { return m_strCheckForUpdAtStart; }
		}

		private static string m_strChkForUpdGotLatest =
			@"You have the latest version.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'You have the latest version.'.
		/// </summary>
		public static string ChkForUpdGotLatest
		{
			get { return m_strChkForUpdGotLatest; }
		}

		private static string m_strChkForUpdNewVersion =
			@"New KeePass version available!";
		/// <summary>
		/// Look up a localized string similar to
		/// 'New KeePass version available!'.
		/// </summary>
		public static string ChkForUpdNewVersion
		{
			get { return m_strChkForUpdNewVersion; }
		}

		private static string m_strClearMRU =
			@"&Clear List";
		/// <summary>
		/// Look up a localized string similar to
		/// '&Clear List'.
		/// </summary>
		public static string ClearMRU
		{
			get { return m_strClearMRU; }
		}

		private static string m_strClipboard =
			@"Clipboard";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Clipboard'.
		/// </summary>
		public static string Clipboard
		{
			get { return m_strClipboard; }
		}

		private static string m_strClipboardAutoClear =
			@"Clipboard Auto-Clear";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Clipboard Auto-Clear'.
		/// </summary>
		public static string ClipboardAutoClear
		{
			get { return m_strClipboardAutoClear; }
		}

		private static string m_strClipboardClearInSeconds =
			@"Clipboard is automatically cleared in [PARAM] seconds";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Clipboard is automatically cleared in [PARAM] seconds'.
		/// </summary>
		public static string ClipboardClearInSeconds
		{
			get { return m_strClipboardClearInSeconds; }
		}

		private static string m_strClipboardClearOnExit =
			@"Clear clipboard when KeePass is closed";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Clear clipboard when KeePass is closed'.
		/// </summary>
		public static string ClipboardClearOnExit
		{
			get { return m_strClipboardClearOnExit; }
		}

		private static string m_strCloseButton =
			@"&Close";
		/// <summary>
		/// Look up a localized string similar to
		/// '&Close'.
		/// </summary>
		public static string CloseButton
		{
			get { return m_strCloseButton; }
		}

		private static string m_strCloseButtonMinimizes =
			@"Close button [X] minimizes main window instead of terminating the application";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Close button [X] minimizes main window instead of terminating the application'.
		/// </summary>
		public static string CloseButtonMinimizes
		{
			get { return m_strCloseButtonMinimizes; }
		}

		private static string m_strCompany =
			@"Company";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Company'.
		/// </summary>
		public static string Company
		{
			get { return m_strCompany; }
		}

		private static string m_strComponents =
			@"Components";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Components'.
		/// </summary>
		public static string Components
		{
			get { return m_strComponents; }
		}

		private static string m_strConfigAffectAdmin =
			@"Changes to the configuration/policy will affect you and all users of this KeePass installation.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Changes to the configuration/policy will affect you and all users of this KeePass installation.'.
		/// </summary>
		public static string ConfigAffectAdmin
		{
			get { return m_strConfigAffectAdmin; }
		}

		private static string m_strConfigAffectUser =
			@"Changes to the configuration/policy will only affect you. Policy flags that are enforced by the administrator are reset after restarting KeePass.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Changes to the configuration/policy will only affect you. Policy flags that are enforced by the administrator are reset after restarting KeePass.'.
		/// </summary>
		public static string ConfigAffectUser
		{
			get { return m_strConfigAffectUser; }
		}

		private static string m_strConfigureAutoType =
			@"Configure Auto-Type";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Configure Auto-Type'.
		/// </summary>
		public static string ConfigureAutoType
		{
			get { return m_strConfigureAutoType; }
		}

		private static string m_strConfigureAutoTypeDesc =
			@"Configure auto-type behaviour for this entry.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Configure auto-type behaviour for this entry.'.
		/// </summary>
		public static string ConfigureAutoTypeDesc
		{
			get { return m_strConfigureAutoTypeDesc; }
		}

		private static string m_strConfigureAutoTypeItem =
			@"Configure Auto-Type Item";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Configure Auto-Type Item'.
		/// </summary>
		public static string ConfigureAutoTypeItem
		{
			get { return m_strConfigureAutoTypeItem; }
		}

		private static string m_strConfigureAutoTypeItemDesc =
			@"Associate a window title with a keystroke sequence.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Associate a window title with a keystroke sequence.'.
		/// </summary>
		public static string ConfigureAutoTypeItemDesc
		{
			get { return m_strConfigureAutoTypeItemDesc; }
		}

		private static string m_strConfigureKeystrokeSeq =
			@"Configure Keystroke Sequence";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Configure Keystroke Sequence'.
		/// </summary>
		public static string ConfigureKeystrokeSeq
		{
			get { return m_strConfigureKeystrokeSeq; }
		}

		private static string m_strConfigureKeystrokeSeqDesc =
			@"Define a default keystroke sequence.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Define a default keystroke sequence.'.
		/// </summary>
		public static string ConfigureKeystrokeSeqDesc
		{
			get { return m_strConfigureKeystrokeSeqDesc; }
		}

		private static string m_strConfigureOnNewDatabase =
			@"Create New Password Database - Step 2";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Create New Password Database - Step 2'.
		/// </summary>
		public static string ConfigureOnNewDatabase
		{
			get { return m_strConfigureOnNewDatabase; }
		}

		private static string m_strConfigWriteGlobal =
			@"Write to Global Configuration";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Write to Global Configuration'.
		/// </summary>
		public static string ConfigWriteGlobal
		{
			get { return m_strConfigWriteGlobal; }
		}

		private static string m_strConfigWriteLocal =
			@"Write to Local Configuration";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Write to Local Configuration'.
		/// </summary>
		public static string ConfigWriteLocal
		{
			get { return m_strConfigWriteLocal; }
		}

		private static string m_strContact =
			@"Contact";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Contact'.
		/// </summary>
		public static string Contact
		{
			get { return m_strContact; }
		}

		private static string m_strCopy =
			@"Copy";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Copy'.
		/// </summary>
		public static string Copy
		{
			get { return m_strCopy; }
		}

		private static string m_strCopyAll =
			@"Copy All";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Copy All'.
		/// </summary>
		public static string CopyAll
		{
			get { return m_strCopyAll; }
		}

		private static string m_strCreateMasterKey =
			@"Create Composite Master Key";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Create Composite Master Key'.
		/// </summary>
		public static string CreateMasterKey
		{
			get { return m_strCreateMasterKey; }
		}

		private static string m_strCreateNewDatabase =
			@"Create New Password Database";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Create New Password Database'.
		/// </summary>
		public static string CreateNewDatabase
		{
			get { return m_strCreateNewDatabase; }
		}

		private static string m_strCreationTime =
			@"Creation Time";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Creation Time'.
		/// </summary>
		public static string CreationTime
		{
			get { return m_strCreationTime; }
		}

		private static string m_strCredSaveAll =
			@"Remember user name and password";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Remember user name and password'.
		/// </summary>
		public static string CredSaveAll
		{
			get { return m_strCredSaveAll; }
		}

		private static string m_strCredSaveNone =
			@"Do not remember user name and password";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Do not remember user name and password'.
		/// </summary>
		public static string CredSaveNone
		{
			get { return m_strCredSaveNone; }
		}

		private static string m_strCredSaveUserOnly =
			@"Remember user name only";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Remember user name only'.
		/// </summary>
		public static string CredSaveUserOnly
		{
			get { return m_strCredSaveUserOnly; }
		}

		private static string m_strCustom =
			@"Custom";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Custom'.
		/// </summary>
		public static string Custom
		{
			get { return m_strCustom; }
		}

		private static string m_strCustomFields =
			@"Custom Fields";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Custom Fields'.
		/// </summary>
		public static string CustomFields
		{
			get { return m_strCustomFields; }
		}

		private static string m_strCut =
			@"Cut";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Cut'.
		/// </summary>
		public static string Cut
		{
			get { return m_strCut; }
		}

		private static string m_strDatabase =
			@"Datenbank";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Datenbank'.
		/// </summary>
		public static string Database
		{
			get { return m_strDatabase; }
		}

		private static string m_strDatabaseDescPrompt =
			@"Enter a short description of the database or leave it empty.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Enter a short description of the database or leave it empty.'.
		/// </summary>
		public static string DatabaseDescPrompt
		{
			get { return m_strDatabaseDescPrompt; }
		}

		private static string m_strDatabaseMaintenance =
			@"Database Maintenance";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Database Maintenance'.
		/// </summary>
		public static string DatabaseMaintenance
		{
			get { return m_strDatabaseMaintenance; }
		}

		private static string m_strDatabaseMaintenanceDesc =
			@"Here you can maintain the currently opened database.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Here you can maintain the currently opened database.'.
		/// </summary>
		public static string DatabaseMaintenanceDesc
		{
			get { return m_strDatabaseMaintenanceDesc; }
		}

		private static string m_strDatabaseModified =
			@"The currently opened database has been modified.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'The currently opened database has been modified.'.
		/// </summary>
		public static string DatabaseModified
		{
			get { return m_strDatabaseModified; }
		}

		private static string m_strDatabaseNamePrompt =
			@"Enter a name for the database or leave it empty.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Enter a name for the database or leave it empty.'.
		/// </summary>
		public static string DatabaseNamePrompt
		{
			get { return m_strDatabaseNamePrompt; }
		}

		private static string m_strDatabaseSettings =
			@"Database Settings";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Database Settings'.
		/// </summary>
		public static string DatabaseSettings
		{
			get { return m_strDatabaseSettings; }
		}

		private static string m_strDatabaseSettingsDesc =
			@"Here you can configure various database settings.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Here you can configure various database settings.'.
		/// </summary>
		public static string DatabaseSettingsDesc
		{
			get { return m_strDatabaseSettingsDesc; }
		}

		private static string m_strDecompressing =
			@"Decompressing";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Decompressing'.
		/// </summary>
		public static string Decompressing
		{
			get { return m_strDecompressing; }
		}

		private static string m_strDefault =
			@"Default";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Default'.
		/// </summary>
		public static string Default
		{
			get { return m_strDefault; }
		}

		private static string m_strDelete =
			@"Delete";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Delete'.
		/// </summary>
		public static string Delete
		{
			get { return m_strDelete; }
		}

		private static string m_strDeleteEntriesCaption =
			@"Delete Entries Confirmation";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Delete Entries Confirmation'.
		/// </summary>
		public static string DeleteEntriesCaption
		{
			get { return m_strDeleteEntriesCaption; }
		}

		private static string m_strDeleteEntriesInfo =
			@"This will remove all selected entries unrecoverably!";
		/// <summary>
		/// Look up a localized string similar to
		/// 'This will remove all selected entries unrecoverably!'.
		/// </summary>
		public static string DeleteEntriesInfo
		{
			get { return m_strDeleteEntriesInfo; }
		}

		private static string m_strDeleteEntriesQuestion =
			@"Are you sure you want to permanently delete all selected entries?";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Are you sure you want to permanently delete all selected entries?'.
		/// </summary>
		public static string DeleteEntriesQuestion
		{
			get { return m_strDeleteEntriesQuestion; }
		}

		private static string m_strDeleteGroupCaption =
			@"Delete Group Confirmation";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Delete Group Confirmation'.
		/// </summary>
		public static string DeleteGroupCaption
		{
			get { return m_strDeleteGroupCaption; }
		}

		private static string m_strDeleteGroupInfo =
			@"Deleting a group will delete all entries and subgroups in that group.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Deleting a group will delete all entries and subgroups in that group.'.
		/// </summary>
		public static string DeleteGroupInfo
		{
			get { return m_strDeleteGroupInfo; }
		}

		private static string m_strDeleteGroupQuestion =
			@"Are you sure you want to permanently delete this group?";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Are you sure you want to permanently delete this group?'.
		/// </summary>
		public static string DeleteGroupQuestion
		{
			get { return m_strDeleteGroupQuestion; }
		}

		private static string m_strDepartment =
			@"Department";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Department'.
		/// </summary>
		public static string Department
		{
			get { return m_strDepartment; }
		}

		private static string m_strDescription =
			@"Description";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Description'.
		/// </summary>
		public static string Description
		{
			get { return m_strDescription; }
		}

		private static string m_strDetails =
			@"Details";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Details'.
		/// </summary>
		public static string Details
		{
			get { return m_strDetails; }
		}

		private static string m_strDocumentationHint =
			@"Please see the documentation for more details.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Please see the documentation for more details.'.
		/// </summary>
		public static string DocumentationHint
		{
			get { return m_strDocumentationHint; }
		}

		private static string m_strDownloadAndInstall =
			@"Download & Install";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Download & Install'.
		/// </summary>
		public static string DownloadAndInstall
		{
			get { return m_strDownloadAndInstall; }
		}

		private static string m_strDownloadFailed =
			@"Download failed.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Download failed.'.
		/// </summary>
		public static string DownloadFailed
		{
			get { return m_strDownloadFailed; }
		}

		private static string m_strDownloading =
			@"Downloading";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Downloading'.
		/// </summary>
		public static string Downloading
		{
			get { return m_strDownloading; }
		}

		private static string m_strDragDrop =
			@"Drag&Drop";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Drag&Drop'.
		/// </summary>
		public static string DragDrop
		{
			get { return m_strDragDrop; }
		}

		private static string m_strDuplicateStringFieldName =
			@"The string field name you specified already exists. String field names must be unique for each entry.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'The string field name you specified already exists. String field names must be unique for each entry.'.
		/// </summary>
		public static string DuplicateStringFieldName
		{
			get { return m_strDuplicateStringFieldName; }
		}

		private static string m_strEditEntry =
			@"Edit Entry";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Edit Entry'.
		/// </summary>
		public static string EditEntry
		{
			get { return m_strEditEntry; }
		}

		private static string m_strEditEntryDesc =
			@"You're editing an existing password entry.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'You're editing an existing password entry.'.
		/// </summary>
		public static string EditEntryDesc
		{
			get { return m_strEditEntryDesc; }
		}

		private static string m_strEditGroup =
			@"Edit Group";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Edit Group'.
		/// </summary>
		public static string EditGroup
		{
			get { return m_strEditGroup; }
		}

		private static string m_strEditGroupDesc =
			@"Edit properties of the currently selected group.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Edit properties of the currently selected group.'.
		/// </summary>
		public static string EditGroupDesc
		{
			get { return m_strEditGroupDesc; }
		}

		private static string m_strEditStringField =
			@"Edit Entry String Field";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Edit Entry String Field'.
		/// </summary>
		public static string EditStringField
		{
			get { return m_strEditStringField; }
		}

		private static string m_strEditStringFieldDesc =
			@"Edit one of the entry's string fields.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Edit one of the entry's string fields.'.
		/// </summary>
		public static string EditStringFieldDesc
		{
			get { return m_strEditStringFieldDesc; }
		}

		private static string m_strEMail =
			@"eMail";
		/// <summary>
		/// Look up a localized string similar to
		/// 'eMail'.
		/// </summary>
		public static string EMail
		{
			get { return m_strEMail; }
		}

		private static string m_strEmpty =
			@"Empty";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Empty'.
		/// </summary>
		public static string Empty
		{
			get { return m_strEmpty; }
		}

		private static string m_strEnterCompositeKey =
			@"Enter Composite Master Key";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Enter Composite Master Key'.
		/// </summary>
		public static string EnterCompositeKey
		{
			get { return m_strEnterCompositeKey; }
		}

		private static string m_strEntropyDesc =
			@"Generate additional random bits.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Generate additional random bits.'.
		/// </summary>
		public static string EntropyDesc
		{
			get { return m_strEntropyDesc; }
		}

		private static string m_strEntropyTitle =
			@"Entropy Collection";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Entropy Collection'.
		/// </summary>
		public static string EntropyTitle
		{
			get { return m_strEntropyTitle; }
		}

		private static string m_strEntry =
			@"Entry";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Entry'.
		/// </summary>
		public static string Entry
		{
			get { return m_strEntry; }
		}

		private static string m_strEntryList =
			@"Entry List";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Entry List'.
		/// </summary>
		public static string EntryList
		{
			get { return m_strEntryList; }
		}

		private static string m_strErrorCode =
			@"Error Code";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Error Code'.
		/// </summary>
		public static string ErrorCode
		{
			get { return m_strErrorCode; }
		}

		private static string m_strErrors =
			@"Errors";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Errors'.
		/// </summary>
		public static string Errors
		{
			get { return m_strErrors; }
		}

		private static string m_strExpiredEntries =
			@"Expired Entries";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Expired Entries'.
		/// </summary>
		public static string ExpiredEntries
		{
			get { return m_strExpiredEntries; }
		}

		private static string m_strExpiryTime =
			@"Expiry Time";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Expiry Time'.
		/// </summary>
		public static string ExpiryTime
		{
			get { return m_strExpiryTime; }
		}

		private static string m_strExport =
			@"Export";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Export'.
		/// </summary>
		public static string Export
		{
			get { return m_strExport; }
		}

		private static string m_strExportHTML =
			@"Export To HTML";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Export To HTML'.
		/// </summary>
		public static string ExportHTML
		{
			get { return m_strExportHTML; }
		}

		private static string m_strExportHTMLDesc =
			@"Export entries to a HTML file.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Export entries to a HTML file.'.
		/// </summary>
		public static string ExportHTMLDesc
		{
			get { return m_strExportHTMLDesc; }
		}

		private static string m_strExportingStatusMsg =
			@"Exporting...";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Exporting...'.
		/// </summary>
		public static string ExportingStatusMsg
		{
			get { return m_strExportingStatusMsg; }
		}

		private static string m_strFatalError =
			@"Fatal Error";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Fatal Error'.
		/// </summary>
		public static string FatalError
		{
			get { return m_strFatalError; }
		}

		private static string m_strFieldName =
			@"Field Name";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Field Name'.
		/// </summary>
		public static string FieldName
		{
			get { return m_strFieldName; }
		}

		private static string m_strFieldNameExistsAlready =
			@"The entered name exists already and cannot be used.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'The entered name exists already and cannot be used.'.
		/// </summary>
		public static string FieldNameExistsAlready
		{
			get { return m_strFieldNameExistsAlready; }
		}

		private static string m_strFieldNameInvalid =
			@"The entered name is invalid and cannot be used.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'The entered name is invalid and cannot be used.'.
		/// </summary>
		public static string FieldNameInvalid
		{
			get { return m_strFieldNameInvalid; }
		}

		private static string m_strFieldValue =
			@"Field Value";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Field Value'.
		/// </summary>
		public static string FieldValue
		{
			get { return m_strFieldValue; }
		}

		private static string m_strFileExistsAlready =
			@"The following file exists already:";
		/// <summary>
		/// Look up a localized string similar to
		/// 'The following file exists already:'.
		/// </summary>
		public static string FileExistsAlready
		{
			get { return m_strFileExistsAlready; }
		}

		private static string m_strFileExtInstallFailed =
			@"Failed to create the file association. Make sure you have write access to the file associations list.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Failed to create the file association. Make sure you have write access to the file associations list.'.
		/// </summary>
		public static string FileExtInstallFailed
		{
			get { return m_strFileExtInstallFailed; }
		}

		private static string m_strFileExtInstallSuccess =
			@"Successfully associated KDBX files with KeePass! KDBX files will now be opened by KeePass when you double-click on them.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Successfully associated KDBX files with KeePass! KDBX files will now be opened by KeePass when you double-click on them.'.
		/// </summary>
		public static string FileExtInstallSuccess
		{
			get { return m_strFileExtInstallSuccess; }
		}

		private static string m_strFileExtName =
			@"KeePass Password Database";
		/// <summary>
		/// Look up a localized string similar to
		/// 'KeePass Password Database'.
		/// </summary>
		public static string FileExtName
		{
			get { return m_strFileExtName; }
		}

		private static string m_strFileLockedBy =
			@"The specified file is currently locked by the following user:";
		/// <summary>
		/// Look up a localized string similar to
		/// 'The specified file is currently locked by the following user:'.
		/// </summary>
		public static string FileLockedBy
		{
			get { return m_strFileLockedBy; }
		}

		private static string m_strFileLockedWarning =
			@"KeePass will open the file, but note that you might overwrite changes each other when saving.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'KeePass will open the file, but note that you might overwrite changes each other when saving.'.
		/// </summary>
		public static string FileLockedWarning
		{
			get { return m_strFileLockedWarning; }
		}

		private static string m_strFileNameContainsSemicolonError =
			@"This file path contains a semicolon (;) and therefore cannot be processed. Replace the semicolon and repeat the procedure.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'This file path contains a semicolon (;) and therefore cannot be processed. Replace the semicolon and repeat the procedure.'.
		/// </summary>
		public static string FileNameContainsSemicolonError
		{
			get { return m_strFileNameContainsSemicolonError; }
		}

		private static string m_strFileNotFoundError =
			@"The specified file could not be found.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'The specified file could not be found.'.
		/// </summary>
		public static string FileNotFoundError
		{
			get { return m_strFileNotFoundError; }
		}

		private static string m_strFiles =
			@"Files";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Files'.
		/// </summary>
		public static string Files
		{
			get { return m_strFiles; }
		}

		private static string m_strFlag =
			@"Flag";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Flag'.
		/// </summary>
		public static string Flag
		{
			get { return m_strFlag; }
		}

		private static string m_strFormatNoDbDesc =
			@"This file format doesn't support database descriptions.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'This file format doesn't support database descriptions.'.
		/// </summary>
		public static string FormatNoDbDesc
		{
			get { return m_strFormatNoDbDesc; }
		}

		private static string m_strFormatNoDbName =
			@"This file format doesn't support database names.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'This file format doesn't support database names.'.
		/// </summary>
		public static string FormatNoDbName
		{
			get { return m_strFormatNoDbName; }
		}

		private static string m_strFormatNoRootEntries =
			@"This file format doesn't support root groups. All entries in the root group are moved to the first subgroup.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'This file format doesn't support root groups. All entries in the root group are moved to the first subgroup.'.
		/// </summary>
		public static string FormatNoRootEntries
		{
			get { return m_strFormatNoRootEntries; }
		}

		private static string m_strFormatNoSubGroupsInRoot =
			@"To export to this file format, the root group must have at least one subgroup.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'To export to this file format, the root group must have at least one subgroup.'.
		/// </summary>
		public static string FormatNoSubGroupsInRoot
		{
			get { return m_strFormatNoSubGroupsInRoot; }
		}

		private static string m_strFormatOnlyOneAttachment =
			@"This file format only supports one attachment per entry. Only the first attachment is saved, the others are ignored.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'This file format only supports one attachment per entry. Only the first attachment is saved, the others are ignored.'.
		/// </summary>
		public static string FormatOnlyOneAttachment
		{
			get { return m_strFormatOnlyOneAttachment; }
		}

		private static string m_strFrequency =
			@"Frequency";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Frequency'.
		/// </summary>
		public static string Frequency
		{
			get { return m_strFrequency; }
		}

		private static string m_strGeneral =
			@"General";
		/// <summary>
		/// Look up a localized string similar to
		/// 'General'.
		/// </summary>
		public static string General
		{
			get { return m_strGeneral; }
		}

		private static string m_strGenerateCount =
			@"Generated Passwords Count";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Generated Passwords Count'.
		/// </summary>
		public static string GenerateCount
		{
			get { return m_strGenerateCount; }
		}

		private static string m_strGenerateCountDesc =
			@"Enter number of passwords to generate.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Enter number of passwords to generate.'.
		/// </summary>
		public static string GenerateCountDesc
		{
			get { return m_strGenerateCountDesc; }
		}

		private static string m_strGenerateCountLongDesc =
			@"Please enter the number of passwords to generate:";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Please enter the number of passwords to generate:'.
		/// </summary>
		public static string GenerateCountLongDesc
		{
			get { return m_strGenerateCountLongDesc; }
		}

		private static string m_strGeneratedPasswordSamples =
			@"Generated Passwords Samples";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Generated Passwords Samples'.
		/// </summary>
		public static string GeneratedPasswordSamples
		{
			get { return m_strGeneratedPasswordSamples; }
		}

		private static string m_strGenProfileAdd =
			@"Add Profile";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Add Profile'.
		/// </summary>
		public static string GenProfileAdd
		{
			get { return m_strGenProfileAdd; }
		}

		private static string m_strGenProfileAddDesc =
			@"Add new password generation options profile";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Add new password generation options profile'.
		/// </summary>
		public static string GenProfileAddDesc
		{
			get { return m_strGenProfileAddDesc; }
		}

		private static string m_strGenProfileAddDescLong =
			@"You are about to save the currently selected options as a profile. Each profile must have a unique name. Please enter a name for the new profile:";
		/// <summary>
		/// Look up a localized string similar to
		/// 'You are about to save the currently selected options as a profile. Each profile must have a unique name. Please enter a name for the new profile:'.
		/// </summary>
		public static string GenProfileAddDescLong
		{
			get { return m_strGenProfileAddDescLong; }
		}

		private static string m_strGenPwBasedOnPrevious =
			@"Derive from previous password";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Derive from previous password'.
		/// </summary>
		public static string GenPwBasedOnPrevious
		{
			get { return m_strGenPwBasedOnPrevious; }
		}

		private static string m_strGenRandomPwForNewEntry =
			@"Automatically generate random passwords for new entries";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Automatically generate random passwords for new entries'.
		/// </summary>
		public static string GenRandomPwForNewEntry
		{
			get { return m_strGenRandomPwForNewEntry; }
		}

		private static string m_strGroup =
			@"Group";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Group'.
		/// </summary>
		public static string Group
		{
			get { return m_strGroup; }
		}

		private static string m_strGroupCannotStoreEntries =
			@"The selected group cannot store any entries.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'The selected group cannot store any entries.'.
		/// </summary>
		public static string GroupCannotStoreEntries
		{
			get { return m_strGroupCannotStoreEntries; }
		}

		private static string m_strHelpSourceNoLocalOption =
			@"This option is grayed out because local help is not installed.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'This option is grayed out because local help is not installed.'.
		/// </summary>
		public static string HelpSourceNoLocalOption
		{
			get { return m_strHelpSourceNoLocalOption; }
		}

		private static string m_strHelpSourceSelection =
			@"Help Source Selection";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Help Source Selection'.
		/// </summary>
		public static string HelpSourceSelection
		{
			get { return m_strHelpSourceSelection; }
		}

		private static string m_strHelpSourceSelectionDesc =
			@"Choose between local help and online help center.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Choose between local help and online help center.'.
		/// </summary>
		public static string HelpSourceSelectionDesc
		{
			get { return m_strHelpSourceSelectionDesc; }
		}

		private static string m_strHomeAddress =
			@"Home Address";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Home Address'.
		/// </summary>
		public static string HomeAddress
		{
			get { return m_strHomeAddress; }
		}

		private static string m_strHomebanking =
			@"Homebanking";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Homebanking'.
		/// </summary>
		public static string Homebanking
		{
			get { return m_strHomebanking; }
		}

		private static string m_strHomeFax =
			@"Home Fax";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Home Fax'.
		/// </summary>
		public static string HomeFax
		{
			get { return m_strHomeFax; }
		}

		private static string m_strHomepageVisitQuestion =
			@"Do you want to visit the KeePass homepage now?";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Do you want to visit the KeePass homepage now?'.
		/// </summary>
		public static string HomepageVisitQuestion
		{
			get { return m_strHomepageVisitQuestion; }
		}

		private static string m_strHomeTel =
			@"Home Tel.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Home Tel.'.
		/// </summary>
		public static string HomeTel
		{
			get { return m_strHomeTel; }
		}

		private static string m_strIBAN =
			@"IBAN";
		/// <summary>
		/// Look up a localized string similar to
		/// 'IBAN'.
		/// </summary>
		public static string IBAN
		{
			get { return m_strIBAN; }
		}

		private static string m_strImageViewer =
			@"Image Viewer";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Image Viewer'.
		/// </summary>
		public static string ImageViewer
		{
			get { return m_strImageViewer; }
		}

		private static string m_strImport =
			@"Import";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Import'.
		/// </summary>
		public static string Import
		{
			get { return m_strImport; }
		}

		private static string m_strImportBehavior =
			@"Import Behavior";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Import Behavior'.
		/// </summary>
		public static string ImportBehavior
		{
			get { return m_strImportBehavior; }
		}

		private static string m_strImportBehaviorDesc =
			@"Select an import method.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Select an import method.'.
		/// </summary>
		public static string ImportBehaviorDesc
		{
			get { return m_strImportBehaviorDesc; }
		}

		private static string m_strImportFailed =
			@"Import failed.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Import failed.'.
		/// </summary>
		public static string ImportFailed
		{
			get { return m_strImportFailed; }
		}

		private static string m_strImportFileDesc =
			@"Import an external file.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Import an external file.'.
		/// </summary>
		public static string ImportFileDesc
		{
			get { return m_strImportFileDesc; }
		}

		private static string m_strImportFileTitle =
			@"Import File/Data";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Import File/Data'.
		/// </summary>
		public static string ImportFileTitle
		{
			get { return m_strImportFileTitle; }
		}

		private static string m_strImportFinished =
			@"The import process has finished!";
		/// <summary>
		/// Look up a localized string similar to
		/// 'The import process has finished!'.
		/// </summary>
		public static string ImportFinished
		{
			get { return m_strImportFinished; }
		}

		private static string m_strImportingStatusMsg =
			@"Importing...";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Importing...'.
		/// </summary>
		public static string ImportingStatusMsg
		{
			get { return m_strImportingStatusMsg; }
		}

		private static string m_strImportMustRead =
			@"It is indispensable that you read the documentation about this import method before continuing.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'It is indispensable that you read the documentation about this import method before continuing.'.
		/// </summary>
		public static string ImportMustRead
		{
			get { return m_strImportMustRead; }
		}

		private static string m_strImportMustReadQuestion =
			@"Have you understood how the import process works and want to start it now?";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Have you understood how the import process works and want to start it now?'.
		/// </summary>
		public static string ImportMustReadQuestion
		{
			get { return m_strImportMustReadQuestion; }
		}

		private static string m_strInternet =
			@"Internet";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Internet'.
		/// </summary>
		public static string Internet
		{
			get { return m_strInternet; }
		}

		private static string m_strInvalidFileStructure =
			@"Invalid file structure!";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Invalid file structure!'.
		/// </summary>
		public static string InvalidFileStructure
		{
			get { return m_strInvalidFileStructure; }
		}

		private static string m_strInvalidKey =
			@"Invalid Key";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Invalid Key'.
		/// </summary>
		public static string InvalidKey
		{
			get { return m_strInvalidKey; }
		}

		private static string m_strInvalidUrl =
			@"The specified URL is invalid.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'The specified URL is invalid.'.
		/// </summary>
		public static string InvalidUrl
		{
			get { return m_strInvalidUrl; }
		}

		private static string m_strInvalidUserPassword =
			@"The specified user name / password combination is invalid.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'The specified user name / password combination is invalid.'.
		/// </summary>
		public static string InvalidUserPassword
		{
			get { return m_strInvalidUserPassword; }
		}

		private static string m_strJobTitle =
			@"Job Title";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Job Title'.
		/// </summary>
		public static string JobTitle
		{
			get { return m_strJobTitle; }
		}

		private static string m_strKDB3KeePassLibC =
			@"The KeePassLibC library is required to open and save KDB files created by KeePass 1.x.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'The KeePassLibC library is required to open and save KDB files created by KeePass 1.x.'.
		/// </summary>
		public static string KDB3KeePassLibC
		{
			get { return m_strKDB3KeePassLibC; }
		}

		private static string m_strKeePassLibCLong =
			@"KeePassLibC (1.x File Support)";
		/// <summary>
		/// Look up a localized string similar to
		/// 'KeePassLibC (1.x File Support)'.
		/// </summary>
		public static string KeePassLibCLong
		{
			get { return m_strKeePassLibCLong; }
		}

		private static string m_strKeePassLibCNotFound =
			@"KeePassLibC could not be found.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'KeePassLibC could not be found.'.
		/// </summary>
		public static string KeePassLibCNotFound
		{
			get { return m_strKeePassLibCNotFound; }
		}

		private static string m_strKeyFileError =
			@"The specified key file could not be found or its format is unknown.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'The specified key file could not be found or its format is unknown.'.
		/// </summary>
		public static string KeyFileError
		{
			get { return m_strKeyFileError; }
		}

		private static string m_strKeystrokeSequence =
			@"Keystroke Sequence";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Keystroke Sequence'.
		/// </summary>
		public static string KeystrokeSequence
		{
			get { return m_strKeystrokeSequence; }
		}

		private static string m_strLanguageSelected =
			@"The selected language has been activated. KeePass must be restarted in order to load the language.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'The selected language has been activated. KeePass must be restarted in order to load the language.'.
		/// </summary>
		public static string LanguageSelected
		{
			get { return m_strLanguageSelected; }
		}

		private static string m_strLastAccessTime =
			@"Last Access Time";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Last Access Time'.
		/// </summary>
		public static string LastAccessTime
		{
			get { return m_strLastAccessTime; }
		}

		private static string m_strLastModificationTime =
			@"Last Modification Time";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Last Modification Time'.
		/// </summary>
		public static string LastModificationTime
		{
			get { return m_strLastModificationTime; }
		}

		private static string m_strLimitSingleInstance =
			@"Limit to single instance";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Limit to single instance'.
		/// </summary>
		public static string LimitSingleInstance
		{
			get { return m_strLimitSingleInstance; }
		}

		private static string m_strLockMenuLock =
			@"&Lock Workspace";
		/// <summary>
		/// Look up a localized string similar to
		/// '&Lock Workspace'.
		/// </summary>
		public static string LockMenuLock
		{
			get { return m_strLockMenuLock; }
		}

		private static string m_strLockMenuUnlock =
			@"Un&lock Workspace";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Un&lock Workspace'.
		/// </summary>
		public static string LockMenuUnlock
		{
			get { return m_strLockMenuUnlock; }
		}

		private static string m_strLockOnMinimize =
			@"Lock workspace when minimizing main window";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Lock workspace when minimizing main window'.
		/// </summary>
		public static string LockOnMinimize
		{
			get { return m_strLockOnMinimize; }
		}

		private static string m_strLockOnSessionLock =
			@"Lock workspace when locking Windows or switching user";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Lock workspace when locking Windows or switching user'.
		/// </summary>
		public static string LockOnSessionLock
		{
			get { return m_strLockOnSessionLock; }
		}

		private static string m_strMainWindow =
			@"Main Window";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Main Window'.
		/// </summary>
		public static string MainWindow
		{
			get { return m_strMainWindow; }
		}

		private static string m_strMasterKeyChanged =
			@"Composite master key has been changed!";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Composite master key has been changed!'.
		/// </summary>
		public static string MasterKeyChanged
		{
			get { return m_strMasterKeyChanged; }
		}

		private static string m_strMasterKeyChangedSavePrompt =
			@"Save the database now in order to get the new key applied.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Save the database now in order to get the new key applied.'.
		/// </summary>
		public static string MasterKeyChangedSavePrompt
		{
			get { return m_strMasterKeyChangedSavePrompt; }
		}

		private static string m_strMenus =
			@"Menus";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Menus'.
		/// </summary>
		public static string Menus
		{
			get { return m_strMenus; }
		}

		private static string m_strMergingData =
			@"Merging data...";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Merging data...'.
		/// </summary>
		public static string MergingData
		{
			get { return m_strMergingData; }
		}

		private static string m_strMinBalance =
			@"Min. Balance";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Min. Balance'.
		/// </summary>
		public static string MinBalance
		{
			get { return m_strMinBalance; }
		}

		private static string m_strMinimizeAfterCopy =
			@"Minimize main window after copying data to the clipboard";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Minimize main window after copying data to the clipboard'.
		/// </summary>
		public static string MinimizeAfterCopy
		{
			get { return m_strMinimizeAfterCopy; }
		}

		private static string m_strMinimizeToTray =
			@"Minimize to tray instead of taskbar";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Minimize to tray instead of taskbar'.
		/// </summary>
		public static string MinimizeToTray
		{
			get { return m_strMinimizeToTray; }
		}

		private static string m_strMobileTel =
			@"Mobile Tel.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Mobile Tel.'.
		/// </summary>
		public static string MobileTel
		{
			get { return m_strMobileTel; }
		}

		private static string m_strNativeLibUse =
			@"Use native library for faster key transformations";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Use native library for faster key transformations'.
		/// </summary>
		public static string NativeLibUse
		{
			get { return m_strNativeLibUse; }
		}

		private static string m_strNavigation =
			@"Navigation";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Navigation'.
		/// </summary>
		public static string Navigation
		{
			get { return m_strNavigation; }
		}

		private static string m_strNetwork =
			@"Network";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Network'.
		/// </summary>
		public static string Network
		{
			get { return m_strNetwork; }
		}

		private static string m_strNeverExpires =
			@"Never expires";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Never expires'.
		/// </summary>
		public static string NeverExpires
		{
			get { return m_strNeverExpires; }
		}

		private static string m_strNewDatabaseKDBFileName =
			@"NewDatabase.kdbx";
		/// <summary>
		/// Look up a localized string similar to
		/// 'NewDatabase.kdbx'.
		/// </summary>
		public static string NewDatabaseKDBFileName
		{
			get { return m_strNewDatabaseKDBFileName; }
		}

		private static string m_strNewGroup =
			@"New Group";
		/// <summary>
		/// Look up a localized string similar to
		/// 'New Group'.
		/// </summary>
		public static string NewGroup
		{
			get { return m_strNewGroup; }
		}

		private static string m_strNoFileAccessRead =
			@"The operating system didn't KeePass grant read access to the specified file.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'The operating system didn't KeePass grant read access to the specified file.'.
		/// </summary>
		public static string NoFileAccessRead
		{
			get { return m_strNoFileAccessRead; }
		}

		private static string m_strNoKeyFileSpecifiedMeta =
			@"(None)";
		/// <summary>
		/// Look up a localized string similar to
		/// '(None)'.
		/// </summary>
		public static string NoKeyFileSpecifiedMeta
		{
			get { return m_strNoKeyFileSpecifiedMeta; }
		}

		private static string m_strNone =
			@"None";
		/// <summary>
		/// Look up a localized string similar to
		/// 'None'.
		/// </summary>
		public static string None
		{
			get { return m_strNone; }
		}

		private static string m_strNotes =
			@"Notes";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Notes'.
		/// </summary>
		public static string Notes
		{
			get { return m_strNotes; }
		}

		private static string m_strNotInstalled =
			@"Not installed";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Not installed'.
		/// </summary>
		public static string NotInstalled
		{
			get { return m_strNotInstalled; }
		}

		private static string m_strNoXSLFile =
			@"The selected file isn't a valid XSL stylesheet.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'The selected file isn't a valid XSL stylesheet.'.
		/// </summary>
		public static string NoXSLFile
		{
			get { return m_strNoXSLFile; }
		}

		private static string m_strOfLower =
			@"of";
		/// <summary>
		/// Look up a localized string similar to
		/// 'of'.
		/// </summary>
		public static string OfLower
		{
			get { return m_strOfLower; }
		}

		private static string m_strOpeningDatabase =
			@"Opening password database...";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Opening password database...'.
		/// </summary>
		public static string OpeningDatabase
		{
			get { return m_strOpeningDatabase; }
		}

		private static string m_strOptions =
			@"Options";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Options'.
		/// </summary>
		public static string Options
		{
			get { return m_strOptions; }
		}

		private static string m_strOptionsDesc =
			@"Here you can configure the global KeePass program options.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Here you can configure the global KeePass program options.'.
		/// </summary>
		public static string OptionsDesc
		{
			get { return m_strOptionsDesc; }
		}

		private static string m_strOtherPlaceholders =
			@"Other Placeholders";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Other Placeholders'.
		/// </summary>
		public static string OtherPlaceholders
		{
			get { return m_strOtherPlaceholders; }
		}

		private static string m_strOverwriteExistingFileQuestion =
			@"Overwrite the existing file?";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Overwrite the existing file?'.
		/// </summary>
		public static string OverwriteExistingFileQuestion
		{
			get { return m_strOverwriteExistingFileQuestion; }
		}

		private static string m_strPager =
			@"Pager";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Pager'.
		/// </summary>
		public static string Pager
		{
			get { return m_strPager; }
		}

		private static string m_strPassword =
			@"Password";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Password'.
		/// </summary>
		public static string Password
		{
			get { return m_strPassword; }
		}

		private static string m_strPasswordManagers =
			@"Password Managers";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Password Managers'.
		/// </summary>
		public static string PasswordManagers
		{
			get { return m_strPasswordManagers; }
		}

		private static string m_strPasswordOptions =
			@"Password Generation Options";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Password Generation Options'.
		/// </summary>
		public static string PasswordOptions
		{
			get { return m_strPasswordOptions; }
		}

		private static string m_strPasswordOptionsDesc =
			@"Here you can define how the generated password should look like.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Here you can define how the generated password should look like.'.
		/// </summary>
		public static string PasswordOptionsDesc
		{
			get { return m_strPasswordOptionsDesc; }
		}

		private static string m_strPasswordPrompt =
			@"Enter the password:";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Enter the password:'.
		/// </summary>
		public static string PasswordPrompt
		{
			get { return m_strPasswordPrompt; }
		}

		private static string m_strPasswordRepeatFailed =
			@"Password and repeated password aren't identical!";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Password and repeated password aren't identical!'.
		/// </summary>
		public static string PasswordRepeatFailed
		{
			get { return m_strPasswordRepeatFailed; }
		}

		private static string m_strPaste =
			@"Paste";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Paste'.
		/// </summary>
		public static string Paste
		{
			get { return m_strPaste; }
		}

		private static string m_strPluginFailedToLoad =
			@"The following plugin cannot be loaded:";
		/// <summary>
		/// Look up a localized string similar to
		/// 'The following plugin cannot be loaded:'.
		/// </summary>
		public static string PluginFailedToLoad
		{
			get { return m_strPluginFailedToLoad; }
		}

		private static string m_strPlugins =
			@"Plugins";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Plugins'.
		/// </summary>
		public static string Plugins
		{
			get { return m_strPlugins; }
		}

		private static string m_strPluginsDesc =
			@"Here you can configure all loaded KeePass plugins.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Here you can configure all loaded KeePass plugins.'.
		/// </summary>
		public static string PluginsDesc
		{
			get { return m_strPluginsDesc; }
		}

		private static string m_strPolicyAutoTypeDesc =
			@"Allow auto-typing entries to other windows.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Allow auto-typing entries to other windows.'.
		/// </summary>
		public static string PolicyAutoTypeDesc
		{
			get { return m_strPolicyAutoTypeDesc; }
		}

		private static string m_strPolicyClipboardDesc =
			@"Allow copying entry information to clipboard (main window only).";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Allow copying entry information to clipboard (main window only).'.
		/// </summary>
		public static string PolicyClipboardDesc
		{
			get { return m_strPolicyClipboardDesc; }
		}

		private static string m_strPolicyDisallowed =
			@"This operation is disallowed by the application policy. Ask your administrator to allow this operation.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'This operation is disallowed by the application policy. Ask your administrator to allow this operation.'.
		/// </summary>
		public static string PolicyDisallowed
		{
			get { return m_strPolicyDisallowed; }
		}

		private static string m_strPolicyDragDropDesc =
			@"Allow sending information to other windows using drag&drop.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Allow sending information to other windows using drag&drop.'.
		/// </summary>
		public static string PolicyDragDropDesc
		{
			get { return m_strPolicyDragDropDesc; }
		}

		private static string m_strPolicyExportDesc =
			@"Allow exporting entries to (non-encrypted) file formats.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Allow exporting entries to (non-encrypted) file formats.'.
		/// </summary>
		public static string PolicyExportDesc
		{
			get { return m_strPolicyExportDesc; }
		}

		private static string m_strPolicyImportDesc =
			@"Allow importing entries from external files.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Allow importing entries from external files.'.
		/// </summary>
		public static string PolicyImportDesc
		{
			get { return m_strPolicyImportDesc; }
		}

		private static string m_strPolicyPluginsDesc =
			@"Allow loading plugins to extend KeePass functionality.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Allow loading plugins to extend KeePass functionality.'.
		/// </summary>
		public static string PolicyPluginsDesc
		{
			get { return m_strPolicyPluginsDesc; }
		}

		private static string m_strPolicyPrintDesc =
			@"Allow printing password entry lists.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Allow printing password entry lists.'.
		/// </summary>
		public static string PolicyPrintDesc
		{
			get { return m_strPolicyPrintDesc; }
		}

		private static string m_strPolicyRequiredFlag =
			@"The following policy flag is required";
		/// <summary>
		/// Look up a localized string similar to
		/// 'The following policy flag is required'.
		/// </summary>
		public static string PolicyRequiredFlag
		{
			get { return m_strPolicyRequiredFlag; }
		}

		private static string m_strPolicySaveDatabaseDesc =
			@"Allow saving databases to disk/drive.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Allow saving databases to disk/drive.'.
		/// </summary>
		public static string PolicySaveDatabaseDesc
		{
			get { return m_strPolicySaveDatabaseDesc; }
		}

		private static string m_strPrint =
			@"Print";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Print'.
		/// </summary>
		public static string Print
		{
			get { return m_strPrint; }
		}

		private static string m_strPrintDesc =
			@"Print password entries.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Print password entries.'.
		/// </summary>
		public static string PrintDesc
		{
			get { return m_strPrintDesc; }
		}

		private static string m_strQuickSearch =
			@"Quick Search";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Quick Search'.
		/// </summary>
		public static string QuickSearch
		{
			get { return m_strQuickSearch; }
		}

		private static string m_strReady =
			@"Ready.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Ready.'.
		/// </summary>
		public static string Ready
		{
			get { return m_strReady; }
		}

		private static string m_strRememberHidingSettings =
			@"Remember password hiding setting in 'Edit Entry' window";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Remember password hiding setting in 'Edit Entry' window'.
		/// </summary>
		public static string RememberHidingSettings
		{
			get { return m_strRememberHidingSettings; }
		}

		private static string m_strRestartKeePassQuestion =
			@"Do you wish to restart KeePass now?";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Do you wish to restart KeePass now?'.
		/// </summary>
		public static string RestartKeePassQuestion
		{
			get { return m_strRestartKeePassQuestion; }
		}

		private static string m_strRoutingCode =
			@"Routing Code";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Routing Code'.
		/// </summary>
		public static string RoutingCode
		{
			get { return m_strRoutingCode; }
		}

		private static string m_strSampleEntry =
			@"Sample Entry";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Sample Entry'.
		/// </summary>
		public static string SampleEntry
		{
			get { return m_strSampleEntry; }
		}

		private static string m_strSaveBeforeCloseQuestion =
			@"Do you want to save the changes before closing?";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Do you want to save the changes before closing?'.
		/// </summary>
		public static string SaveBeforeCloseQuestion
		{
			get { return m_strSaveBeforeCloseQuestion; }
		}

		private static string m_strSaveBeforeCloseTitle =
			@"KeePass - Save Before Close/Lock?";
		/// <summary>
		/// Look up a localized string similar to
		/// 'KeePass - Save Before Close/Lock?'.
		/// </summary>
		public static string SaveBeforeCloseTitle
		{
			get { return m_strSaveBeforeCloseTitle; }
		}

		private static string m_strSaveDatabase =
			@"Save Database";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Save Database'.
		/// </summary>
		public static string SaveDatabase
		{
			get { return m_strSaveDatabase; }
		}

		private static string m_strSavingDatabase =
			@"Saving database...";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Saving database...'.
		/// </summary>
		public static string SavingDatabase
		{
			get { return m_strSavingDatabase; }
		}

		private static string m_strSearchDesc =
			@"Search the password database for entries.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Search the password database for entries.'.
		/// </summary>
		public static string SearchDesc
		{
			get { return m_strSearchDesc; }
		}

		private static string m_strSearchGroupName =
			@"Search Results";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Search Results'.
		/// </summary>
		public static string SearchGroupName
		{
			get { return m_strSearchGroupName; }
		}

		private static string m_strSearchItemsFoundSmall =
			@"entries found.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'entries found.'.
		/// </summary>
		public static string SearchItemsFoundSmall
		{
			get { return m_strSearchItemsFoundSmall; }
		}

		private static string m_strSearchKeyFilesOnRemovable =
			@"Automatically search key files on removable media";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Automatically search key files on removable media'.
		/// </summary>
		public static string SearchKeyFilesOnRemovable
		{
			get { return m_strSearchKeyFilesOnRemovable; }
		}

		private static string m_strSearchResultsInSeparator =
			@"in";
		/// <summary>
		/// Look up a localized string similar to
		/// 'in'.
		/// </summary>
		public static string SearchResultsInSeparator
		{
			get { return m_strSearchResultsInSeparator; }
		}

		private static string m_strSearchTitle =
			@"Search";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Search'.
		/// </summary>
		public static string SearchTitle
		{
			get { return m_strSearchTitle; }
		}

		private static string m_strSelectAll =
			@"Select All";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Select All'.
		/// </summary>
		public static string SelectAll
		{
			get { return m_strSelectAll; }
		}

		private static string m_strSelectDifferentGroup =
			@"Please select a different group.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Please select a different group.'.
		/// </summary>
		public static string SelectDifferentGroup
		{
			get { return m_strSelectDifferentGroup; }
		}

		private static string m_strSelectedLower =
			@"selected";
		/// <summary>
		/// Look up a localized string similar to
		/// 'selected'.
		/// </summary>
		public static string SelectedLower
		{
			get { return m_strSelectedLower; }
		}

		private static string m_strSelectLanguage =
			@"Select Language";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Select Language'.
		/// </summary>
		public static string SelectLanguage
		{
			get { return m_strSelectLanguage; }
		}

		private static string m_strSelectLanguageDesc =
			@"Here you can select a different user interface language.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Here you can select a different user interface language.'.
		/// </summary>
		public static string SelectLanguageDesc
		{
			get { return m_strSelectLanguageDesc; }
		}

		private static string m_strSelfTestFailed =
			@"One or more of the KeePass self-tests failed.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'One or more of the KeePass self-tests failed.'.
		/// </summary>
		public static string SelfTestFailed
		{
			get { return m_strSelfTestFailed; }
		}

		private static string m_strShowFullPathInTitleBar =
			@"Show full path in title bar (instead of file name only)";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Show full path in title bar (instead of file name only)'.
		/// </summary>
		public static string ShowFullPathInTitleBar
		{
			get { return m_strShowFullPathInTitleBar; }
		}

		private static string m_strShowGridLines =
			@"Show grid lines in password entry list";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Show grid lines in password entry list'.
		/// </summary>
		public static string ShowGridLines
		{
			get { return m_strShowGridLines; }
		}

		private static string m_strShowTrayOnlyIfTrayed =
			@"Show tray icon only if main window has been sent to tray";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Show tray icon only if main window has been sent to tray'.
		/// </summary>
		public static string ShowTrayOnlyIfTrayed
		{
			get { return m_strShowTrayOnlyIfTrayed; }
		}

		private static string m_strSoonToExpireEntries =
			@"Expired Entries and Entries That Will Expire Soon";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Expired Entries and Entries That Will Expire Soon'.
		/// </summary>
		public static string SoonToExpireEntries
		{
			get { return m_strSoonToExpireEntries; }
		}

		private static string m_strSortCode =
			@"Sort Code";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Sort Code'.
		/// </summary>
		public static string SortCode
		{
			get { return m_strSortCode; }
		}

		private static string m_strSpecialKeys =
			@"Special Keys";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Special Keys'.
		/// </summary>
		public static string SpecialKeys
		{
			get { return m_strSpecialKeys; }
		}

		private static string m_strSpecifyApplication =
			@"Specify Application";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Specify Application'.
		/// </summary>
		public static string SpecifyApplication
		{
			get { return m_strSpecifyApplication; }
		}

		private static string m_strSpecifyApplicationDesc =
			@"Customize used application for the current URL.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Customize used application for the current URL.'.
		/// </summary>
		public static string SpecifyApplicationDesc
		{
			get { return m_strSpecifyApplicationDesc; }
		}

		private static string m_strStandardFields =
			@"Standard Fields";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Standard Fields'.
		/// </summary>
		public static string StandardFields
		{
			get { return m_strStandardFields; }
		}

		private static string m_strStartAndExit =
			@"Start and Exit";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Start and Exit'.
		/// </summary>
		public static string StartAndExit
		{
			get { return m_strStartAndExit; }
		}

		private static string m_strSuccess =
			@"Success.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Success.'.
		/// </summary>
		public static string Success
		{
			get { return m_strSuccess; }
		}

		private static string m_strSWIFTCode =
			@"SWIFT Code";
		/// <summary>
		/// Look up a localized string similar to
		/// 'SWIFT Code'.
		/// </summary>
		public static string SWIFTCode
		{
			get { return m_strSWIFTCode; }
		}

		private static string m_strSyncFailed =
			@"Synchronization failed.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Synchronization failed.'.
		/// </summary>
		public static string SyncFailed
		{
			get { return m_strSyncFailed; }
		}

		private static string m_strSynchronize =
			@"Synchronize";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Synchronize'.
		/// </summary>
		public static string Synchronize
		{
			get { return m_strSynchronize; }
		}

		private static string m_strSynchronizingHint =
			@"Make sure that the two databases use the same composite master key. This is required for synchronization.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Make sure that the two databases use the same composite master key. This is required for synchronization.'.
		/// </summary>
		public static string SynchronizingHint
		{
			get { return m_strSynchronizingHint; }
		}

		private static string m_strSyncSuccess =
			@"Synchronization completed successfully.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Synchronization completed successfully.'.
		/// </summary>
		public static string SyncSuccess
		{
			get { return m_strSyncSuccess; }
		}

		private static string m_strSystem =
			@"System";
		/// <summary>
		/// Look up a localized string similar to
		/// 'System'.
		/// </summary>
		public static string System
		{
			get { return m_strSystem; }
		}

		private static string m_strSystemCodepage =
			@"System Codepage";
		/// <summary>
		/// Look up a localized string similar to
		/// 'System Codepage'.
		/// </summary>
		public static string SystemCodepage
		{
			get { return m_strSystemCodepage; }
		}

		private static string m_strTAccountInfoTel =
			@"Account Info Tel.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Account Info Tel.'.
		/// </summary>
		public static string TAccountInfoTel
		{
			get { return m_strTAccountInfoTel; }
		}

		private static string m_strTAccountNames =
			@"Account names";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Account names'.
		/// </summary>
		public static string TAccountNames
		{
			get { return m_strTAccountNames; }
		}

		private static string m_strTAccountNumber =
			@"Account Number";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Account Number'.
		/// </summary>
		public static string TAccountNumber
		{
			get { return m_strTAccountNumber; }
		}

		private static string m_strTAccountType =
			@"Account Type";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Account Type'.
		/// </summary>
		public static string TAccountType
		{
			get { return m_strTAccountType; }
		}

		private static string m_strTANWizard =
			@"TAN Wizard";
		/// <summary>
		/// Look up a localized string similar to
		/// 'TAN Wizard'.
		/// </summary>
		public static string TANWizard
		{
			get { return m_strTANWizard; }
		}

		private static string m_strTANWizardDesc =
			@"With this TAN wizard you can easily add TAN entries.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'With this TAN wizard you can easily add TAN entries.'.
		/// </summary>
		public static string TANWizardDesc
		{
			get { return m_strTANWizardDesc; }
		}

		private static string m_strTargetWindow =
			@"Target Window";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Target Window'.
		/// </summary>
		public static string TargetWindow
		{
			get { return m_strTargetWindow; }
		}

		private static string m_strTextViewer =
			@"Text Viewer";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Text Viewer'.
		/// </summary>
		public static string TextViewer
		{
			get { return m_strTextViewer; }
		}

		private static string m_strTitle =
			@"Title";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Title'.
		/// </summary>
		public static string Title
		{
			get { return m_strTitle; }
		}

		private static string m_strTooManyFilesError =
			@"Too many files have been selected. Select smaller groups and repeat the current procedure a few times.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Too many files have been selected. Select smaller groups and repeat the current procedure a few times.'.
		/// </summary>
		public static string TooManyFilesError
		{
			get { return m_strTooManyFilesError; }
		}

		private static string m_strTransfersOf =
			@"Transfers Of";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Transfers Of'.
		/// </summary>
		public static string TransfersOf
		{
			get { return m_strTransfersOf; }
		}

		private static string m_strUndo =
			@"Undo";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Undo'.
		/// </summary>
		public static string Undo
		{
			get { return m_strUndo; }
		}

		private static string m_strUnknown =
			@"Unknown";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Unknown'.
		/// </summary>
		public static string Unknown
		{
			get { return m_strUnknown; }
		}

		private static string m_strUnknownError =
			@"An unknown error occured.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'An unknown error occured.'.
		/// </summary>
		public static string UnknownError
		{
			get { return m_strUnknownError; }
		}

		private static string m_strUnknownFileVersion =
			@"Unknown file version!";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Unknown file version!'.
		/// </summary>
		public static string UnknownFileVersion
		{
			get { return m_strUnknownFileVersion; }
		}

		private static string m_strURL =
			@"URL";
		/// <summary>
		/// Look up a localized string similar to
		/// 'URL'.
		/// </summary>
		public static string URL
		{
			get { return m_strURL; }
		}

		private static string m_strUrlOpenDesc =
			@"Open a database stored on a server.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Open a database stored on a server.'.
		/// </summary>
		public static string UrlOpenDesc
		{
			get { return m_strUrlOpenDesc; }
		}

		private static string m_strUrlOpenTitle =
			@"Open From URL";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Open From URL'.
		/// </summary>
		public static string UrlOpenTitle
		{
			get { return m_strUrlOpenTitle; }
		}

		private static string m_strUrlOverride =
			@"URL Override";
		/// <summary>
		/// Look up a localized string similar to
		/// 'URL Override'.
		/// </summary>
		public static string UrlOverride
		{
			get { return m_strUrlOverride; }
		}

		private static string m_strUrlSaveDesc =
			@"Save current database on a server.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Save current database on a server.'.
		/// </summary>
		public static string UrlSaveDesc
		{
			get { return m_strUrlSaveDesc; }
		}

		private static string m_strUrlSaveTitle =
			@"Save To URL";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Save To URL'.
		/// </summary>
		public static string UrlSaveTitle
		{
			get { return m_strUrlSaveTitle; }
		}

		private static string m_strUrlStarterCustomText =
			@"Custom (Specify Path)";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Custom (Specify Path)'.
		/// </summary>
		public static string UrlStarterCustomText
		{
			get { return m_strUrlStarterCustomText; }
		}

		private static string m_strUrlStarterFailCaption =
			@"Plugin/program not found";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Plugin/program not found'.
		/// </summary>
		public static string UrlStarterFailCaption
		{
			get { return m_strUrlStarterFailCaption; }
		}

		private static string m_strUrlStarterNotFoundPost =
			@"However, this program/plugin is not installed on this system.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'However, this program/plugin is not installed on this system.'.
		/// </summary>
		public static string UrlStarterNotFoundPost
		{
			get { return m_strUrlStarterNotFoundPost; }
		}

		private static string m_strUrlStarterNotFoundPre =
			@"You have specified the following program/plugin to be used to open the URL of the current entry:";
		/// <summary>
		/// Look up a localized string similar to
		/// 'You have specified the following program/plugin to be used to open the URL of the current entry:'.
		/// </summary>
		public static string UrlStarterNotFoundPre
		{
			get { return m_strUrlStarterNotFoundPre; }
		}

		private static string m_strUrlStarterUsesDefault =
			@"The default system shell will be used to start the URL.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'The default system shell will be used to start the URL.'.
		/// </summary>
		public static string UrlStarterUsesDefault
		{
			get { return m_strUrlStarterUsesDefault; }
		}

		private static string m_strUserName =
			@"User Name";
		/// <summary>
		/// Look up a localized string similar to
		/// 'User Name'.
		/// </summary>
		public static string UserName
		{
			get { return m_strUserName; }
		}

		private static string m_strUserNamePrompt =
			@"Enter the user name:";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Enter the user name:'.
		/// </summary>
		public static string UserNamePrompt
		{
			get { return m_strUserNamePrompt; }
		}

		private static string m_strUUID =
			@"UUID";
		/// <summary>
		/// Look up a localized string similar to
		/// 'UUID'.
		/// </summary>
		public static string UUID
		{
			get { return m_strUUID; }
		}

		private static string m_strVersion =
			@"Version";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Version'.
		/// </summary>
		public static string Version
		{
			get { return m_strVersion; }
		}

		private static string m_strViewEntry =
			@"View Entry";
		/// <summary>
		/// Look up a localized string similar to
		/// 'View Entry'.
		/// </summary>
		public static string ViewEntry
		{
			get { return m_strViewEntry; }
		}

		private static string m_strViewEntryDesc =
			@"You're viewing a password entry.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'You're viewing a password entry.'.
		/// </summary>
		public static string ViewEntryDesc
		{
			get { return m_strViewEntryDesc; }
		}

		private static string m_strWarnings =
			@"Warnings";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Warnings'.
		/// </summary>
		public static string Warnings
		{
			get { return m_strWarnings; }
		}

		private static string m_strWebBrowser =
			@"Web Browser";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Web Browser'.
		/// </summary>
		public static string WebBrowser
		{
			get { return m_strWebBrowser; }
		}

		private static string m_strWebPage =
			@"Web Page";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Web Page'.
		/// </summary>
		public static string WebPage
		{
			get { return m_strWebPage; }
		}

		private static string m_strWebSiteLogin =
			@"Web Site Login";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Web Site Login'.
		/// </summary>
		public static string WebSiteLogin
		{
			get { return m_strWebSiteLogin; }
		}

		private static string m_strWebSites =
			@"Web Sites";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Web Sites'.
		/// </summary>
		public static string WebSites
		{
			get { return m_strWebSites; }
		}

		private static string m_strWindowsOS =
			@"Windows";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Windows'.
		/// </summary>
		public static string WindowsOS
		{
			get { return m_strWindowsOS; }
		}

		private static string m_strWorkFax =
			@"Work Fax";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Work Fax'.
		/// </summary>
		public static string WorkFax
		{
			get { return m_strWorkFax; }
		}

		private static string m_strWorkspaceLocked =
			@"Workspace Locked";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Workspace Locked'.
		/// </summary>
		public static string WorkspaceLocked
		{
			get { return m_strWorkspaceLocked; }
		}

		private static string m_strWorkTel =
			@"Work Tel.";
		/// <summary>
		/// Look up a localized string similar to
		/// 'Work Tel.'.
		/// </summary>
		public static string WorkTel
		{
			get { return m_strWorkTel; }
		}

		private static string m_strXSLStylesheets =
			@"XSL Stylesheets for KDB4 XML";
		/// <summary>
		/// Look up a localized string similar to
		/// 'XSL Stylesheets for KDB4 XML'.
		/// </summary>
		public static string XSLStylesheets
		{
			get { return m_strXSLStylesheets; }
		}
	}
}
