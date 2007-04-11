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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using KeePass.App;
using KeePass.Native;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Security;
using KeePassLib.Collections;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class EditAutoTypeItemForm : Form
	{
		private AutoTypeConfig m_atConfig = null;
		private ProtectedStringDictionary m_vStringDict = null;
		private string m_strOriginalName = null;
		private bool m_bEditSequenceOnly = false;

		private Color m_clrOriginalForeground = Color.Black;
		private Color m_clrOriginalBackground = Color.White;
		private string m_strOriginalWindowHint = string.Empty;

		private RichTextBoxContextMenu m_ctxKeySeq = new RichTextBoxContextMenu();
		private RichTextBoxContextMenu m_ctxKeyCodes = new RichTextBoxContextMenu();
		private bool m_bBlockUpdates = false;

		private static string[] SpecialKeyCodes = new string[]{
			"TAB", "ENTER", "UP", "DOWN", "LEFT", "RIGHT",
			"HOME", "END", "PGUP", "PGDN",
			"INSERT", "DELETE", "<break />",
			"BACKSPACE", "BREAK", "CAPSLOCK",
			"ESC", "HELP", "NUMLOCK", "PRTSC", "SCROLLLOCK", "<break />",
			"F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12",
			"F13", "F14", "F15", "F16", "<break />",
			"ADD", "SUBTRACT", "MULTIPLY", "DIVIDE"
		};

		private static string[] SpecialPlaceholders = new string[]{
			"APPDIR", "DOCDIR"
		};

		public EditAutoTypeItemForm()
		{
			InitializeComponent();
		}

		public void InitEx(AutoTypeConfig atConfig, ProtectedStringDictionary vStringDict, string strOriginalName, bool bEditSequenceOnly)
		{
			Debug.Assert(vStringDict != null); if(vStringDict == null) throw new ArgumentNullException();
			Debug.Assert(atConfig != null); if(atConfig == null) throw new ArgumentNullException();

			m_atConfig = atConfig;
			m_vStringDict = vStringDict;
			m_strOriginalName = strOriginalName;
			m_bEditSequenceOnly = bEditSequenceOnly;
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			Debug.Assert(m_vStringDict != null); if(m_vStringDict == null) throw new ArgumentNullException();
			Debug.Assert(m_atConfig != null); if(m_atConfig == null) throw new ArgumentNullException();

			GlobalWindowManager.AddWindow(this);

			m_ctxKeySeq.Attach(m_rbKeySeq);
			m_ctxKeyCodes.Attach(m_rtbPlaceholders);

			if(!m_bEditSequenceOnly)
			{
				m_bannerImage.Image = BannerFactory.CreateBanner(m_bannerImage.Width,
					m_bannerImage.Height, BannerFactory.BannerStyle.Default,
					Properties.Resources.B48x48_KCMSystem, KPRes.ConfigureAutoTypeItem,
					KPRes.ConfigureAutoTypeItemDesc);
			}
			else // Edit keystrokes only
			{
				m_bannerImage.Image = BannerFactory.CreateBanner(m_bannerImage.Width,
					m_bannerImage.Height, BannerFactory.BannerStyle.Default,
					Properties.Resources.B48x48_KCMSystem, KPRes.ConfigureKeystrokeSeq,
					KPRes.ConfigureKeystrokeSeqDesc);
			}

			this.Icon = Properties.Resources.KeePass;

			m_clrOriginalForeground = m_lblTargetWindowInfo.ForeColor;
			m_clrOriginalBackground = m_cmbWindow.BackColor;
			m_strOriginalWindowHint = m_lblTargetWindowInfo.Text;

			StringBuilder sbPH = new StringBuilder();
			sbPH.Append("<b>");
			sbPH.Append(KPRes.StandardFields);
			sbPH.Append(":</b><br />");

			sbPH.Append("{" + PwDefs.TitleField + "} ");
			sbPH.Append("{" + PwDefs.UserNameField + "} ");
			sbPH.Append("{" + PwDefs.PasswordField + "} ");
			sbPH.Append("{" + PwDefs.UrlField + "} ");
			sbPH.Append("{" + PwDefs.NotesField + "}");

			bool bCustomInitialized = false;
			foreach(KeyValuePair<string, ProtectedString> kvp in m_vStringDict)
			{
				if(!PwDefs.IsStandardField(kvp.Key))
				{
					if(bCustomInitialized == false)
					{
						sbPH.Append("<br /><br /><b>");
						sbPH.Append(KPRes.CustomFields);
						sbPH.Append(":</b><br />");
						bCustomInitialized = true;
					}

					sbPH.Append("{" + PwDefs.AutoTypeStringPrefix + kvp.Key + "} ");
				}
			}

			sbPH.Append("<br /><br /><b>" + KPRes.SpecialKeys + ":</b><br />");
			foreach(string strNav in SpecialKeyCodes)
			{
				if(strNav == "<break />") sbPH.Append("<br /><br />");
				else sbPH.Append("{" + strNav + "} ");
			}

			sbPH.Append("<br /><br /><b>" + KPRes.OtherPlaceholders + ":</b><br />");
			foreach(string strPH in SpecialPlaceholders)
			{
				if(strPH == "<break />") sbPH.Append("<br /><br />");
				else sbPH.Append("{" + strPH + "} ");
			}

			m_rtbPlaceholders.Rtf = StrUtil.SimpleHtmlToRtf(sbPH.ToString());
			LinkifyRtf(m_rtbPlaceholders);

			if(m_strOriginalName != null)
			{
				m_cmbWindow.Text = m_strOriginalName;

				if(!m_bEditSequenceOnly)
					m_rbKeySeq.Text = m_atConfig.GetSafe(m_strOriginalName);
				else
					m_rbKeySeq.Text = m_atConfig.DefaultSequence;
			}

			try
			{
				NativeMethods.EnumWindowsProc procEnum = delegate(IntPtr hWnd,
					IntPtr lParam)
				{
					string strName = NativeMethods.GetWindowText(hWnd);
					if((strName != null) && (strName.Length > 0))
					{
						if((NativeMethods.GetWindowStyle(hWnd) &
							NativeMethods.WS_VISIBLE) != 0)
						{
							m_cmbWindow.Items.Add(strName);
						}
					}

					return true;
				};

				NativeMethods.EnumWindows(procEnum, IntPtr.Zero);
			}
			catch(Exception) { Debug.Assert(false); }

			EnableControlsEx();
			m_cmbWindow.Focus();
		}

		private void CleanUpEx()
		{
			m_ctxKeyCodes.Detach();
			m_ctxKeySeq.Detach();
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			EnableControlsEx();
			Debug.Assert(m_btnOK.Enabled); if(!m_btnOK.Enabled) return;

			if(!m_bEditSequenceOnly)
			{
				if(m_strOriginalName != null)
				{
					m_atConfig.Remove(m_strOriginalName);
				}

				m_atConfig.Set(m_cmbWindow.Text, m_rbKeySeq.Text);
			}
			else m_atConfig.DefaultSequence = m_rbKeySeq.Text;

			CleanUpEx();
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
			CleanUpEx();
		}

		private void OnBtnHelp(object sender, EventArgs e)
		{
			AppHelp.ShowHelp(AppDefs.HelpTopics.AutoType, null);
		}

		private void EnableControlsEx()
		{
			if(m_bBlockUpdates) return;
			m_bBlockUpdates = true;

			string strItemName = m_cmbWindow.Text;

			bool bEnableOK = true;
			string strError = string.Empty;

			if((m_atConfig.Get(strItemName) != null) && !m_bEditSequenceOnly)
				if((m_strOriginalName == null) || !strItemName.Equals(m_strOriginalName))
				{
					bEnableOK = false;
					strError = KPRes.FieldNameExistsAlready;
				}

			if((strItemName.IndexOf('{') >= 0) || (strItemName.IndexOf('}') >= 0))
			{
				bEnableOK = false;
				strError = KPRes.FieldNameInvalid;
			}

			if(bEnableOK)
			{
				m_lblTargetWindowInfo.Text = m_strOriginalWindowHint;
				m_lblTargetWindowInfo.ForeColor = m_clrOriginalForeground;
				m_cmbWindow.BackColor = m_clrOriginalBackground;
				m_btnOK.Enabled = true;
			}
			else
			{
				m_lblTargetWindowInfo.Text = strError;
				m_lblTargetWindowInfo.ForeColor = Color.Red;
				m_cmbWindow.BackColor = AppDefs.ColorEditError;
				m_btnOK.Enabled = false;
			}

			if(m_bEditSequenceOnly)
			{
				m_cmbWindow.Enabled = false;
				m_lblTargetWindowInfo.Enabled = false;
			}

			m_bBlockUpdates = false;
		}

		private void ColorizeKeySeq()
		{
			string strText = m_rbKeySeq.Text;

			int iSelStart = m_rbKeySeq.SelectionStart, iSelLen = m_rbKeySeq.SelectionLength;

			m_rbKeySeq.SelectAll();
			m_rbKeySeq.SelectionBackColor = SystemColors.Window;

			int iStart = 0;
			while(true)
			{
				int iPos = strText.IndexOf('{', iStart);
				if(iPos < 0) break;

				int iEnd = strText.IndexOf('}', iPos + 1);
				if(iEnd < 0) break;

				m_rbKeySeq.Select(iPos, iEnd - iPos + 1);
				m_rbKeySeq.SelectionBackColor = Color.FromArgb(212, 255, 212);
				iStart = iEnd;
			}

			m_rbKeySeq.SelectionStart = iSelStart;
			m_rbKeySeq.SelectionLength = iSelLen;
		}

		private void OnTextChangedKeySeq(object sender, EventArgs e)
		{
			ColorizeKeySeq();
		}

		private static void LinkifyRtf(RichTextBox rtb)
		{
			string str = rtb.Text;

			int iPos = str.IndexOf('{');
			while(iPos >= 0)
			{
				int iEnd = str.IndexOf('}', iPos);
				if(iEnd >= 1)
				{
					rtb.Select(iPos, iEnd - iPos + 1);
					UIUtil.RtfSetSelectionLink(rtb);
				}

				iPos = str.IndexOf('{', iPos + 1);
			}

			rtb.Select(0, 0);
		}

		private void OnPlaceholdersLinkClicked(object sender, LinkClickedEventArgs e)
		{
			int nSelStart = m_rbKeySeq.SelectionStart;
			int nSelLength = m_rbKeySeq.SelectionLength;
			string strText = m_rbKeySeq.Text;
			string strURL = e.LinkText;

			if(nSelLength > 0)
				strText = strText.Remove(nSelStart, nSelLength);

			m_rbKeySeq.Text = strText.Insert(nSelStart, strURL);
			m_rbKeySeq.Select(nSelStart + strURL.Length, 0);
			m_rbKeySeq.Focus();
		}

		private void OnWindowTextUpdate(object sender, EventArgs e)
		{
			EnableControlsEx();
		}

		private void OnWindowSelectedIndexChanged(object sender, EventArgs e)
		{
			EnableControlsEx();
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			GlobalWindowManager.RemoveWindow(this);
		}
	}
}