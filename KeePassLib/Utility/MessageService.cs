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
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using KeePassLib.Resources;
using KeePassLib.Serialization;

namespace KeePassLib.Utility
{
	public static class MessageService
	{
		private static uint m_uCurrentMessageCount = 0;

#if !KeePassLibSD
		private const MessageBoxIcon m_mbiInfo = MessageBoxIcon.Information;
		private const MessageBoxIcon m_mbiWarning = MessageBoxIcon.Warning;
		private const MessageBoxIcon m_mbiFatal = MessageBoxIcon.Error;
#else
		private const MessageBoxIcon m_mbiInfo = MessageBoxIcon.Asterisk;
		private const MessageBoxIcon m_mbiWarning = MessageBoxIcon.Exclamation;
		private const MessageBoxIcon m_mbiFatal = MessageBoxIcon.Hand;
#endif
		private const MessageBoxIcon m_mbiQuestion = MessageBoxIcon.Question;

		public static string NewLine
		{
#if !KeePassLibSD
			get { return Environment.NewLine; }
#else
			get { return "\r\n"; }
#endif
		}

		public static string NewParagraph
		{
#if !KeePassLibSD
			get { return Environment.NewLine + Environment.NewLine; }
#else
			get { return "\r\n\r\n"; }
#endif
		}

		public static uint CurrentMessageCount
		{
			get { return m_uCurrentMessageCount; }
		}

		private static string ObjectsToMessage(object[] vLines)
		{
			return ObjectsToMessage(vLines, false);
		}

		private static string ObjectsToMessage(object[] vLines, bool bFullExceptions)
		{
			if(vLines == null) return string.Empty;

			string strNewPara = MessageService.NewParagraph;

			StringBuilder sbText = new StringBuilder();
			bool bSeparator = false;

			foreach(object obj in vLines)
			{
				if(obj == null) continue;

				string strAppend = null;

				Exception exObj = obj as Exception;
				string strObj = obj as string;

				if(exObj != null)
				{
					if(bFullExceptions)
						strAppend = StrUtil.FormatException(exObj);
					else if((exObj.Message != null) && (exObj.Message.Length > 0))
						strAppend = exObj.Message;
				}
				else if(strObj != null)
					strAppend = strObj;
				else
					strAppend = obj.ToString();

				if((strAppend != null) && (strAppend.Length > 0))
				{
					if(bSeparator) sbText.Append(strNewPara);
					else bSeparator = true;

					sbText.Append(strAppend);
				}
			}

			return sbText.ToString();
		}

		public static void ShowInfo(params object[] vLines)
		{
			string strText = ObjectsToMessage(vLines);

			++m_uCurrentMessageCount;
			MessageBox.Show(strText, PwDefs.ShortProductName,
				MessageBoxButtons.OK, m_mbiInfo, MessageBoxDefaultButton.Button1);
			--m_uCurrentMessageCount;
		}

		public static void ShowWarning(params object[] vLines)
		{
			string strText = ObjectsToMessage(vLines);

			++m_uCurrentMessageCount;
			MessageBox.Show(strText, PwDefs.ShortProductName,
				MessageBoxButtons.OK, m_mbiWarning, MessageBoxDefaultButton.Button1);
			--m_uCurrentMessageCount;
		}

		public static void ShowFatal(params object[] vLines)
		{
			string strText = KLRes.FatalErrorText + MessageService.NewParagraph +
				KLRes.ErrorFeedbackRequest + MessageService.NewParagraph +
				ObjectsToMessage(vLines);

			try
			{
#if !KeePassLibSD
				Clipboard.Clear();
				Clipboard.SetText(ObjectsToMessage(vLines, true));
#else
				Clipboard.SetDataObject(ObjectsToMessage(vLines, true));
#endif
			}
			catch(Exception) { }

			++m_uCurrentMessageCount;
			MessageBox.Show(strText, PwDefs.ShortProductName + @" - " +
				KLRes.FatalError, MessageBoxButtons.OK, m_mbiFatal,
				MessageBoxDefaultButton.Button1);
			--m_uCurrentMessageCount;
		}

		public static DialogResult Ask(string strText, string strTitle,
			MessageBoxButtons mbb)
		{
			string strTextEx = ((strText != null) ? strText : string.Empty);
			string strTitleEx = ((strTitle != null) ? strTitle : PwDefs.ShortProductName);

			++m_uCurrentMessageCount;
			DialogResult dr = MessageBox.Show(strTextEx, strTitleEx, mbb,
				m_mbiQuestion, MessageBoxDefaultButton.Button1);
			--m_uCurrentMessageCount;

			return dr;
		}

		public static bool AskYesNo(string strText, string strTitle)
		{
			string strTextEx = ((strText != null) ? strText : string.Empty);
			string strTitleEx = ((strTitle != null) ? strTitle : PwDefs.ShortProductName);

			++m_uCurrentMessageCount;
			DialogResult dr = MessageBox.Show(strTextEx, strTitleEx, MessageBoxButtons.YesNo,
				m_mbiQuestion, MessageBoxDefaultButton.Button1);
			--m_uCurrentMessageCount;

			return (dr == DialogResult.Yes);
		}

		public static bool AskYesNo(string strText)
		{
			return AskYesNo(strText, null);
		}

		public static void ShowLoadWarning(string strFilePath, Exception ex)
		{
			string str = string.Empty;

			if((strFilePath != null) && (strFilePath.Length > 0))
				str += strFilePath + MessageService.NewParagraph;

			str += KLRes.FileLoadFailed;

			if((ex != null) && (ex.Message != null) && (ex.Message.Length > 0))
				str += MessageService.NewParagraph + ex.Message;

			ShowWarning(str);
		}

		public static void ShowLoadWarning(IOConnectionInfo ioConnection, Exception ex)
		{
			if(ioConnection != null)
				ShowLoadWarning(ioConnection.GetDisplayName(), ex);
			else ShowWarning(ex);
		}

		public static void ShowSaveWarning(string strFilePath, Exception ex)
		{
			string str = string.Empty;

			if((strFilePath != null) && (strFilePath.Length > 0))
				str += strFilePath + MessageService.NewParagraph;

			str += KLRes.FileSaveFailed;

			if((ex != null) && (ex.Message != null) && (ex.Message.Length > 0))
				str += MessageService.NewParagraph + ex.Message;

			ShowWarning(str);
		}

		public static void ShowSaveWarning(IOConnectionInfo ioConnection, Exception ex)
		{
			if(ioConnection != null)
				ShowSaveWarning(ioConnection.GetDisplayName(), ex);
			else ShowWarning(ex);
		}
	}
}
