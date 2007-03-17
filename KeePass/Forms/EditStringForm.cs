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
using KeePass.UI;
using KeePass.Resources;

using KeePassLib;
using KeePassLib.Security;
using KeePassLib.Collections;

namespace KeePass.Forms
{
	public partial class EditStringForm : Form
	{
		private ProtectedStringDictionary m_vStringDict = null;
		private string m_strStringName = null;
		private ProtectedString m_psStringValue = null;
		private Color m_clrNormalBackground;
		private RichTextBoxContextMenu m_ctxValue = new RichTextBoxContextMenu();

		public EditStringForm()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Initialize the dialog. Needs to be called before the dialog is shown.
		/// </summary>
		/// <param name="vStringDict">String container. Must not be <c>null</c>.</param>
		/// <param name="strStringName">Initial name of the string. May be <c>null</c>.</param>
		/// <param name="psStringValue">Initial value. May be <c>null</c>.</param>
		public void InitEx(ProtectedStringDictionary vStringDict, string strStringName, ProtectedString psStringValue)
		{
			Debug.Assert(vStringDict != null); if(vStringDict == null) throw new ArgumentNullException();
			m_vStringDict = vStringDict;

			m_strStringName = strStringName;
			m_psStringValue = psStringValue;
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			Debug.Assert(m_vStringDict != null); if(m_vStringDict == null) throw new ArgumentNullException();

			m_ctxValue.Attach(m_richStringValue);

			string strTitle, strDesc;
			if(m_strStringName == null)
			{
				strTitle = KPRes.AddStringField;
				strDesc = KPRes.AddStringFieldDesc;
			}
			else
			{
				strTitle = KPRes.EditStringField;
				strDesc = KPRes.EditStringFieldDesc;
			}

			m_bannerImage.Image = BannerFactory.CreateBanner(m_bannerImage.Width,
				m_bannerImage.Height, BannerFactory.BannerStyle.Default,
				Properties.Resources.B48x48_Font, strTitle, strDesc);
			this.Icon = Properties.Resources.KeePass;

			m_clrNormalBackground = m_tbStringName.BackColor;

			if(m_strStringName != null) m_tbStringName.Text = m_strStringName;
			if(m_psStringValue != null)
			{
				m_richStringValue.Text = m_psStringValue.ReadString();
				m_cbProtect.Checked = m_psStringValue.IsProtected;
			}

			ValidateStringName();
			m_tbStringName.Focus();
		}

		private bool ValidateStringName()
		{
			string str = m_tbStringName.Text;
			string strStart = (m_strStringName != null) ? m_strStringName : "";
			char[] vInvalidChars = new char[]{ '{', '}' };

			if(PwDefs.IsStandardField(str))
			{
				m_lblValidationInfo.Text = KPRes.FieldNameInvalid;
				m_tbStringName.BackColor = AppDefs.ColorEditError;
				m_btnOK.Enabled = false;

				return false;
			}
			else if((str.Length <= 0) || (str.IndexOfAny(vInvalidChars) >= 0))
			{
				m_lblValidationInfo.Text = KPRes.FieldNameInvalid;
				m_tbStringName.BackColor = AppDefs.ColorEditError;
				m_btnOK.Enabled = false;

				return false;
			}
			else if(!strStart.Equals(str) && m_vStringDict.Exists(str))
			{
				m_lblValidationInfo.Text = KPRes.FieldNameExistsAlready;
				m_tbStringName.BackColor = AppDefs.ColorEditError;
				m_btnOK.Enabled = false;

				return false;
			}
			else
			{
				m_lblValidationInfo.Text = "";
				m_tbStringName.BackColor = m_clrNormalBackground;
				m_btnOK.Enabled = true;
			}

			return true;
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			string strName = m_tbStringName.Text;

			if(!ValidateStringName())
			{
				this.DialogResult = DialogResult.None;
				return;
			}

			if(m_strStringName == null) // Add string field
			{
				Debug.Assert(m_vStringDict.Exists(strName) == false);

				ProtectedString ps = new ProtectedString(m_cbProtect.Checked, m_richStringValue.Text);
					m_vStringDict.Set(strName, ps);
			}
			else // Edit string field
			{
				if(m_strStringName.Equals(strName))
				{
					if(m_psStringValue != null)
					{
						m_psStringValue.SetString(m_richStringValue.Text);
						m_psStringValue.EnableProtection(m_cbProtect.Checked);
					}
					else
					{
						ProtectedString ps = new ProtectedString(m_cbProtect.Checked, m_richStringValue.Text);
						m_vStringDict.Set(strName, ps);
					}
				}
				else
				{
					m_vStringDict.Remove(m_strStringName);

					ProtectedString ps = new ProtectedString(m_cbProtect.Checked, m_richStringValue.Text);
					m_vStringDict.Set(strName, ps);
				}
			}

			CleanUpEx();
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
			CleanUpEx();
		}

		private void CleanUpEx()
		{
			m_ctxValue.Detach();
		}

		private void OnBtnHelp(object sender, EventArgs e)
		{
			AppHelp.ShowHelp(AppDefs.HelpTopics.Entry, AppDefs.HelpTopics.EntryStrings);
		}

		private void OnTextChangedName(object sender, EventArgs e)
		{
			ValidateStringName();
		}
	}
}