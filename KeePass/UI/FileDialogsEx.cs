/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2012 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using KeePass.Resources;
using KeePass.Util;

using KeePassLib;
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
			FileSaveOrigin fsOrigin, IntPtr hParent)
		{
			bool bFile = ((strFile != null) && (strFile.Length > 0));

			if(WinUtil.IsAtLeastWindowsVista)
			{
				VistaTaskDialog dlg = new VistaTaskDialog(hParent);

				string strText = KPRes.DatabaseModifiedNoDot;
				if(bFile) strText += ":\r\n" + strFile;
				else strText += ".";

				dlg.CommandLinks = true;
				dlg.WindowTitle = PwDefs.ShortProductName;
				dlg.Content = strText;
				dlg.SetIcon(VtdCustomIcon.Question);

				bool bShowCheckBox = true;
				if(fsOrigin == FileSaveOrigin.Locking)
				{
					dlg.MainInstruction = KPRes.FileSaveQLocking;
					dlg.AddButton((int)DialogResult.Yes, KPRes.SaveCmd, KPRes.FileSaveQOpYesLocking);
					dlg.AddButton((int)DialogResult.No, KPRes.DiscardChangesCmd, KPRes.FileSaveQOpNoLocking);
					dlg.AddButton((int)DialogResult.Cancel, KPRes.CancelCmd, KPRes.FileSaveQOpCancel +
						" " + KPRes.FileSaveQOpCancelLocking);
				}
				else if(fsOrigin == FileSaveOrigin.Exiting)
				{
					dlg.MainInstruction = KPRes.FileSaveQExiting;
					dlg.AddButton((int)DialogResult.Yes, KPRes.SaveCmd, KPRes.FileSaveQOpYesExiting);
					dlg.AddButton((int)DialogResult.No, KPRes.DiscardChangesCmd, KPRes.FileSaveQOpNoExiting);
					dlg.AddButton((int)DialogResult.Cancel, KPRes.CancelCmd, KPRes.FileSaveQOpCancel +
						" " + KPRes.FileSaveQOpCancelExiting);
				}
				else
				{
					dlg.MainInstruction = KPRes.FileSaveQClosing;
					dlg.AddButton((int)DialogResult.Yes, KPRes.SaveCmd, KPRes.FileSaveQOpYesClosing);
					dlg.AddButton((int)DialogResult.No, KPRes.DiscardChangesCmd, KPRes.FileSaveQOpNoClosing);
					dlg.AddButton((int)DialogResult.Cancel, KPRes.CancelCmd, KPRes.FileSaveQOpCancel +
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

			string strMessage = (bFile ? (strFile + MessageService.NewParagraph) : string.Empty);
			strMessage += KPRes.DatabaseModifiedNoDot + "." +
				MessageService.NewParagraph + KPRes.SaveBeforeCloseQuestion;
			return MessageService.Ask(strMessage, KPRes.SaveBeforeCloseTitle,
				MessageBoxButtons.YesNoCancel);
		}
	}
}
