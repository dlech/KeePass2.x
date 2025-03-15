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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

using KeePass.App;
using KeePass.App.Configuration;
using KeePass.Forms;
using KeePass.Resources;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Resources;
using KeePassLib.Utility;

namespace KeePass.UI
{
	public enum FileSaveOrigin
	{
		Closing = 0,
		Locking = 1,
		Exiting = 2
	}

	public static class FileDialogsEx
	{
		public static DialogResult ShowFileSaveQuestion(string strFile,
			FileSaveOrigin fsOrigin)
		{
			bool bFile = !string.IsNullOrEmpty(strFile);

			if(WinUtil.IsAtLeastWindowsVista)
			{
				VistaTaskDialog dlg = new VistaTaskDialog();

				dlg.CommandLinks = true;
				dlg.Content = (!bFile ? (KPRes.DatabaseModifiedNoDot + ".") :
					(KPRes.DatabaseFile + ":" + MessageService.NewLine + strFile));
				dlg.WindowTitle = PwDefs.ShortProductName;
				dlg.SetIcon(VtdCustomIcon.Question);

				bool bShowCheckBox = true;
				if(fsOrigin == FileSaveOrigin.Locking)
				{
					dlg.MainInstruction = KPRes.FileSaveQLocking;
					dlg.AddButton((int)DialogResult.Yes, KPRes.SaveCmd, KPRes.FileSaveQOpYesLocking);
					dlg.AddButton((int)DialogResult.No, KPRes.DiscardChangesCmd, KPRes.FileSaveQOpNoLocking);
					dlg.AddButton((int)DialogResult.Cancel, KPRes.Cancel, KPRes.FileSaveQOpCancel +
						" " + KPRes.FileSaveQOpCancelLocking);
				}
				else if(fsOrigin == FileSaveOrigin.Exiting)
				{
					dlg.MainInstruction = KPRes.FileSaveQExiting;
					dlg.AddButton((int)DialogResult.Yes, KPRes.SaveCmd, KPRes.FileSaveQOpYesExiting);
					dlg.AddButton((int)DialogResult.No, KPRes.DiscardChangesCmd, KPRes.FileSaveQOpNoExiting);
					dlg.AddButton((int)DialogResult.Cancel, KPRes.Cancel, KPRes.FileSaveQOpCancel +
						" " + KPRes.FileSaveQOpCancelExiting);
				}
				else
				{
					dlg.MainInstruction = KPRes.FileSaveQClosing;
					dlg.AddButton((int)DialogResult.Yes, KPRes.SaveCmd, KPRes.FileSaveQOpYesClosing);
					dlg.AddButton((int)DialogResult.No, KPRes.DiscardChangesCmd, KPRes.FileSaveQOpNoClosing);
					dlg.AddButton((int)DialogResult.Cancel, KPRes.Cancel, KPRes.FileSaveQOpCancel +
						" " + KPRes.FileSaveQOpCancelClosing);
					bShowCheckBox = false;
				}

				if(Program.Config.Application.FileClosing.AutoSave) bShowCheckBox = false;
				if(bShowCheckBox) dlg.VerificationText = KPRes.AutoSaveAtExit;

				if(dlg.ShowDialog())
				{
					if(bShowCheckBox && (dlg.Result == (int)DialogResult.Yes))
						Program.Config.Application.FileClosing.AutoSave = dlg.ResultVerificationChecked;

					return (DialogResult)dlg.Result;
				}
			}

			string strMsg = KPRes.DatabaseModifiedNoDot + "." + MessageService.NewParagraph;
			if(bFile)
				strMsg += KPRes.DatabaseFile + ":" + MessageService.NewLine +
					strFile + MessageService.NewParagraph;
			strMsg += KPRes.SaveBeforeCloseQuestion;
			return MessageService.Ask(strMsg, KPRes.SaveBeforeCloseTitle,
				MessageBoxButtons.YesNoCancel);
		}

		public static bool ShowNewDatabaseIntro(Form fParent)
		{
			string str = KPRes.DatabaseFileIntro + MessageService.NewParagraph +
				KPRes.DatabaseFileRem + MessageService.NewParagraph +
				KPRes.BackupDatabase;

			int r = VistaTaskDialog.ShowMessageBoxEx(str, KPRes.NewDatabase,
				PwDefs.ShortProductName, VtdIcon.Information, fParent, KPRes.Ok,
				(int)DialogResult.OK, KPRes.Cancel, (int)DialogResult.Cancel);
			if(r >= 0) return (r == (int)DialogResult.OK);

			MessageService.ShowInfo(str);
			return true;
		}

		internal static bool CheckAttachmentSize(long lSize, string strOp)
		{
			// https://sourceforge.net/p/keepass/discussion/329221/thread/42ddc71a/
			const long cbMax = 512 * 1024 * 1024;

			if(lSize > cbMax)
			{
				MessageService.ShowWarning(strOp, KPRes.FileTooLarge +
					" " + KPRes.MaxAttachmentSize.Replace(@"{PARAM}",
					StrUtil.FormatDataSize((ulong)cbMax)));
				return false;
			}

			return true;
		}

		internal static bool CheckAttachmentSize(string strPath, string strOp)
		{
			FileInfo fi = new FileInfo(strPath);
			return CheckAttachmentSize(fi.Length, strOp);
		}

		internal static void ShowConfigError(string strPath, Exception exError,
			bool bSaving, bool bCreateBackup)
		{
			if(exError == null) { Debug.Assert(false); return; }

			StringBuilder sb = new StringBuilder();

			if(!string.IsNullOrEmpty(strPath))
			{
				sb.AppendLine(VistaTaskDialog.CreateLink("c", strPath));
				sb.AppendLine();
			}

			sb.AppendLine(bSaving ? KLRes.FileSaveFailed : KLRes.FileLoadFailed);
			sb.AppendLine();
			sb.Append(StrUtil.FormatException(exError, null));

			string strText = sb.ToString();

			VistaTaskDialog dlg = new VistaTaskDialog();
			dlg.AddButton((int)DialogResult.Cancel, KPRes.Ok, null);
			dlg.CommandLinks = false;
			dlg.Content = strText;
			dlg.DefaultButtonID = (int)DialogResult.Cancel;
			dlg.EnableHyperlinks = true;
			dlg.MainInstruction = KPRes.ConfigError;
			dlg.SetIcon(VtdIcon.Warning);
			dlg.WindowTitle = PwDefs.ShortProductName;

			string strBackupText = null;
			string strBackupPath = (bCreateBackup ? AppConfigSerializer.TryCreateBackup(
				strPath) : null);
			if(!string.IsNullOrEmpty(strBackupPath))
			{
				strBackupText = KPRes.ConfigOverwriteBackup + MessageService.NewLine +
					VistaTaskDialog.CreateLink("b", strBackupPath);
				dlg.FooterText = strBackupText;
				dlg.SetFooterIcon(VtdIcon.Information);
			}

			dlg.LinkClicked += delegate(object sender, LinkClickedEventArgs e)
			{
				string str = (e.LinkText ?? string.Empty);
				if(str.Equals("c", StrUtil.CaseIgnoreCmp))
					WinUtil.ShowFileInFileManager(strPath, false);
				else if(str.Equals("b", StrUtil.CaseIgnoreCmp))
					WinUtil.ShowFileInFileManager(strBackupPath, false);
				else { Debug.Assert(false); }
			};

			if(!dlg.ShowDialog())
			{
				if(!string.IsNullOrEmpty(strBackupText))
					strText += MessageService.NewParagraph + strBackupText;
				strText = VistaTaskDialog.Unlink(strText);

				MessageService.ShowWarning(KPRes.ConfigError + "!", strText);
			}
		}

		private static string ShowFileDialog(bool bSaveMode, string strTitle,
			string strSuggestedFileName, string strFilter, int iFilterIndex,
			string strDefaultExt, string strContext, bool bSecureDesktop)
		{
			if(bSecureDesktop)
			{
				FileBrowserForm fbf = new FileBrowserForm();
				fbf.InitEx(bSaveMode, strTitle, KPRes.SecDeskFileDialogHint, strContext);
				fbf.SuggestedFile = (strSuggestedFileName ?? string.Empty);

				try
				{
					DialogResult drF = fbf.ShowDialog();
					return ((drF == DialogResult.OK) ? fbf.SelectedFile : null);
				}
				finally { UIUtil.DestroyForm(fbf); }
			}

			if(bSaveMode)
			{
				SaveFileDialogEx sfd = UIUtil.CreateSaveFileDialog(strTitle,
					strSuggestedFileName, strFilter, iFilterIndex, strDefaultExt,
					strContext);
				DialogResult drS = sfd.ShowDialog();
				return ((drS == DialogResult.OK) ? sfd.FileName : null);
			}

			OpenFileDialogEx ofd = UIUtil.CreateOpenFileDialog(strTitle,
				strFilter, iFilterIndex, strDefaultExt, false, strContext);
			DialogResult drO = ofd.ShowDialog();
			return ((drO == DialogResult.OK) ? ofd.FileName : null);
		}

		internal static string ShowKeyFileDialog(bool bSaveMode, string strTitle,
			string strSuggestedFileName, bool bAllFilesByDefault, bool bSecureDesktop)
		{
			Debug.Assert(!bSaveMode || !bAllFilesByDefault); // Not all files when saving

			string strFilter = AppDefs.GetKeyFileFilter();
			int iFilterIndex = (bAllFilesByDefault ? 2 : 1);
			string strExt = (bSaveMode ? AppDefs.FileExtension.KeyFile : null);
			string strContext = AppDefs.FileDialogContext.KeyFile;

			return ShowFileDialog(bSaveMode, strTitle, strSuggestedFileName,
				strFilter, iFilterIndex, strExt, strContext, bSecureDesktop);
		}

		internal static string ShowAttachmentSaveFileDialog(string strSuggestedFileName)
		{
			string strName = UrlUtil.GetSafeFileName(strSuggestedFileName);

			SaveFileDialogEx sfd = UIUtil.CreateSaveFileDialog(KPRes.AttachmentSave,
				strName, UIUtil.CreateFileTypeFilter(null, null, true), 1, null,
				AppDefs.FileDialogContext.Attachments);
			return ((sfd.ShowDialog() == DialogResult.OK) ? sfd.FileName : null);
		}

		internal static bool ConfirmRunFile(string strCmdLine, string strCmpFile,
			string strCmpArgs, object oCfgContainer, string strCfgPropName)
		{
			PropertyInfo piForSet;
			if(!AppConfigEx.GetPropertyValue<bool>(oCfgContainer, strCfgPropName,
				true, out piForSet))
				return true;

			strCmdLine = (strCmdLine ?? string.Empty).Trim();

			string strText = KPRes.RunOpenFileQ;
			if(strCmdLine.Length != 0)
				strText = strCmdLine + MessageService.NewParagraph + strText;

			string strExpText = WinUtil.GetFileArgsText(strCmpFile, strCmpArgs);

			VistaTaskDialog dlg = new VistaTaskDialog();
			dlg.Content = strText;
			if(!string.IsNullOrEmpty(strExpText))
			{
				dlg.ExpandedInformation = strExpText;
				dlg.ExpandedControlText = KPRes.Details;
				dlg.CollapsedControlText = KPRes.Details;
				dlg.FooterText = KPRes.FileArgsDetailsHint;
				dlg.SetFooterIcon(VtdIcon.Information);
			}
			if(piForSet != null)
				dlg.VerificationText = UIUtil.GetDialogNoShowAgainText(null);
			dlg.WindowTitle = PwDefs.ShortProductName;
			dlg.SetIcon(VtdCustomIcon.Question);
			dlg.AddButton((int)DialogResult.OK, KPRes.Yes, null);
			dlg.AddButton((int)DialogResult.Cancel, KPRes.No, null);

			using(FocusRestoreScope frs = new FocusRestoreScope())
			{
				if(dlg.ShowDialog())
				{
					bool b = (dlg.Result == (int)DialogResult.OK);
					if(b && (piForSet != null) && dlg.ResultVerificationChecked)
						piForSet.SetValue(oCfgContainer, false, null);
					return b;
				}
				return MessageService.AskYesNo(strText);
			}
		}

		internal static void ShowFileException(string strCmdLine, Exception ex,
			string strCmpFile, string strCmpArgs)
		{
			StringBuilder sb = new StringBuilder();
			StrUtil.AppendTrim(sb, null, strCmdLine);
			StrUtil.AppendTrim(sb, MessageService.NewParagraph,
				StrUtil.FormatException(ex, null));
			string strText = sb.ToString();

			string strExpText = WinUtil.GetFileArgsText(strCmpFile, strCmpArgs);

			VistaTaskDialog dlg = new VistaTaskDialog();
			dlg.Content = strText;
			if(!string.IsNullOrEmpty(strExpText))
			{
				dlg.ExpandedInformation = strExpText;
				dlg.ExpandedControlText = KPRes.Details;
				dlg.CollapsedControlText = KPRes.Details;
				dlg.FooterText = KPRes.FileArgsDetailsHint;
				dlg.SetFooterIcon(VtdIcon.Information);
			}
			dlg.WindowTitle = PwDefs.ShortProductName;
			dlg.SetIcon(VtdIcon.Warning);
			dlg.AddButton((int)DialogResult.Cancel, KPRes.Ok, null);

			if(!dlg.ShowDialog()) MessageService.ShowWarning(strText);
		}
	}

	public abstract class FileDialogEx
	{
		private readonly bool m_bSaveMode;
		private readonly string m_strContext;

		public abstract FileDialog FileDialog
		{
			get;
		}

		public string DefaultExt
		{
			get { return this.FileDialog.DefaultExt; }
			set { this.FileDialog.DefaultExt = value; }
		}

		public string FileName
		{
			get { return this.FileDialog.FileName; }
			set { this.FileDialog.FileName = value; }
		}

		public string[] FileNames
		{
			get { return this.FileDialog.FileNames; }
		}

		public string Filter
		{
			get { return this.FileDialog.Filter; }
			set { this.FileDialog.Filter = value; }
		}

		public int FilterIndex
		{
			get { return this.FileDialog.FilterIndex; }
			set { this.FileDialog.FilterIndex = value; }
		}

		private string m_strInitialDirectoryOvr = null;
		public string InitialDirectory
		{
			get { return m_strInitialDirectoryOvr; }
			set { m_strInitialDirectoryOvr = value; }
		}

		public string Title
		{
			get { return this.FileDialog.Title; }
			set { this.FileDialog.Title = value; }
		}

		protected FileDialogEx(bool bSaveMode, string strContext)
		{
			m_bSaveMode = bSaveMode;
			m_strContext = strContext; // May be null
		}

		public DialogResult ShowDialog()
		{
			string strPrevWorkDir = PreShowDialog();
			DialogResult dr = this.FileDialog.ShowDialog();
			PostShowDialog(strPrevWorkDir, dr);
			return dr;
		}

		public DialogResult ShowDialog(IWin32Window owner)
		{
			string strPrevWorkDir = PreShowDialog();
			DialogResult dr = this.FileDialog.ShowDialog(owner);
			PostShowDialog(strPrevWorkDir, dr);
			return dr;
		}

		private string PreShowDialog()
		{
			MonoWorkarounds.EnsureRecentlyUsedValid();

			string strPrevWorkDir = WinUtil.GetWorkingDirectory();

			string strNew = Program.Config.Application.GetWorkingDirectory(m_strContext);
			if(!string.IsNullOrEmpty(m_strInitialDirectoryOvr))
				strNew = m_strInitialDirectoryOvr;
			WinUtil.SetWorkingDirectory(strNew); // Always, even when no context

			try
			{
				string strWD = WinUtil.GetWorkingDirectory();
				this.FileDialog.InitialDirectory = strWD;
			}
			catch(Exception) { Debug.Assert(false); }

			return strPrevWorkDir;
		}

		private void PostShowDialog(string strPrevWorkDir, DialogResult dr)
		{
			string strCur = null;
			// Modern file dialogs (on Windows >= Vista) do not change the
			// working directory (in contrast to Windows <= XP), thus we
			// derive the working directory from the first file
			try
			{
				if(dr == DialogResult.OK)
				{
					string strFile = null;
					if(m_bSaveMode) strFile = this.FileDialog.FileName;
					else if(this.FileDialog.FileNames.Length > 0)
						strFile = this.FileDialog.FileNames[0];

					if(!string.IsNullOrEmpty(strFile))
						strCur = UrlUtil.GetFileDirectory(strFile, false, true);
				}
			}
			catch(Exception) { Debug.Assert(false); }

			if(!string.IsNullOrEmpty(strCur))
				Program.Config.Application.SetWorkingDirectory(m_strContext, strCur);

			WinUtil.SetWorkingDirectory(strPrevWorkDir);
		}
	}

	public sealed class OpenFileDialogEx : FileDialogEx
	{
		private readonly OpenFileDialog m_dlg = new OpenFileDialog();

		public override FileDialog FileDialog
		{
			get { return m_dlg; }
		}

		public bool Multiselect
		{
			get { return m_dlg.Multiselect; }
			set { m_dlg.Multiselect = value; }
		}

		public OpenFileDialogEx(string strContext) : base(false, strContext)
		{
			m_dlg.CheckFileExists = true;
			m_dlg.CheckPathExists = true;
			m_dlg.DereferenceLinks = true;
			m_dlg.ReadOnlyChecked = false;
			m_dlg.ShowHelp = false;
			m_dlg.ShowReadOnly = false;
			// m_dlg.SupportMultiDottedExtensions = false; // Default
			m_dlg.ValidateNames = true;

			m_dlg.RestoreDirectory = false; // Want new working directory
		}
	}

	public sealed class SaveFileDialogEx : FileDialogEx
	{
		private readonly SaveFileDialog m_dlg = new SaveFileDialog();

		public override FileDialog FileDialog
		{
			get { return m_dlg; }
		}

		public SaveFileDialogEx(string strContext) : base(true, strContext)
		{
			m_dlg.AddExtension = true;
			m_dlg.CheckFileExists = false;
			m_dlg.CheckPathExists = true;
			m_dlg.CreatePrompt = false;
			m_dlg.DereferenceLinks = true;
			m_dlg.OverwritePrompt = true;
			m_dlg.ShowHelp = false;
			// m_dlg.SupportMultiDottedExtensions = false; // Default
			m_dlg.ValidateNames = true;

			m_dlg.RestoreDirectory = false; // Want new working directory
		}
	}
}
