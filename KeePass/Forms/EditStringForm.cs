/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2018 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using KeePass.App;
using KeePass.Resources;
using KeePass.UI;

using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Delegates;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class EditStringForm : Form
	{
		private ProtectedStringDictionary m_vStringDict = null;
		private string m_strStringName = null;
		private ProtectedString m_psStringInitialValue = null;
		private PwDatabase m_pwContext = null;

		private List<string> m_lSuggestedNames = new List<string>();
		private List<string> m_lStdNames = PwDefs.GetStandardFields();
		private char[] m_vInvalidChars = new char[] { '{', '}' };

		private RichTextBoxContextMenu m_ctxValue = new RichTextBoxContextMenu();

		private bool m_bReadOnly = false;
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DefaultValue(false)]
		public bool ReadOnlyEx
		{
			get { return m_bReadOnly; }
			set { m_bReadOnly = value; }
		}

		public EditStringForm()
		{
			InitializeComponent();
			Program.Translation.ApplyTo(this);
		}

		/// <summary>
		/// Initialize the dialog. Needs to be called before the dialog is shown.
		/// </summary>
		/// <param name="vStringDict">String container. Must not be <c>null</c>.</param>
		/// <param name="strStringName">Initial name of the string. May be <c>null</c>.</param>
		/// <param name="psStringInitialValue">Initial value. May be <c>null</c>.</param>
		public void InitEx(ProtectedStringDictionary vStringDict, string strStringName,
			ProtectedString psStringInitialValue, PwDatabase pwContext)
		{
			Debug.Assert(vStringDict != null); if(vStringDict == null) throw new ArgumentNullException("vStringDict");
			m_vStringDict = vStringDict;

			m_strStringName = strStringName;
			m_psStringInitialValue = psStringInitialValue;

			m_pwContext = pwContext;
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			Debug.Assert(m_vStringDict != null); if(m_vStringDict == null) throw new InvalidOperationException();

			GlobalWindowManager.AddWindow(this);

			m_ctxValue.Attach(m_richStringValue, this);

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

			BannerFactory.CreateBannerEx(this, m_bannerImage,
				Properties.Resources.B48x48_Font, strTitle, strDesc);
			this.Icon = AppIcons.Default;

			UIUtil.EnableAutoCompletion(m_cmbStringName, true);
			UIUtil.PrepareStandardMultilineControl(m_richStringValue, true, true);

			if(m_strStringName != null) m_cmbStringName.Text = m_strStringName;
			if(m_psStringInitialValue != null)
			{
				m_richStringValue.Text = StrUtil.NormalizeNewLines(
					m_psStringInitialValue.ReadString(), true);
				m_cbProtect.Checked = m_psStringInitialValue.IsProtected;
			}

			ValidateStringNameUI();
			PopulateNamesComboBox();

			if(m_bReadOnly)
			{
				m_cmbStringName.Enabled = false;
				m_richStringValue.ReadOnly = true;
				m_cbProtect.Enabled = false;
				// m_btnOK.Enabled = false; // See ValidateStringNameUI
			}

			// UIUtil.SetFocus(..., this); // See PopulateNamesComboBox
		}

		private bool ValidateStringNameUI()
		{
			string strResult;
			bool bError;
			bool b = ValidateStringName(m_cmbStringName.Text, out strResult,
				out bError);

			Debug.Assert(!m_lblValidationInfo.AutoSize); // For RTL support
			m_lblValidationInfo.Text = strResult;
			if(bError) m_cmbStringName.BackColor = AppDefs.ColorEditError;
			else m_cmbStringName.ResetBackColor();

			b &= !m_bReadOnly;
			m_btnOK.Enabled = b;
			return b;			
		}

		private bool ValidateStringName(string str)
		{
			string strResult;
			bool bError;
			return ValidateStringName(str, out strResult, out bError);
		}

		private bool ValidateStringName(string str, out string strResult,
			out bool bError)
		{
			strResult = KPRes.FieldNameInvalid;
			bError = true;

			if(str == null) { Debug.Assert(false); return false; }

			if(str.Length == 0)
			{
				strResult = KPRes.FieldNamePrompt;
				bError = false; // Name not acceptable, but no error
				return false;
			}

			if(str == m_strStringName) // Case-sensitive
			{
				strResult = string.Empty;
				bError = false;
				return true;
			}

			foreach(string strStd in m_lStdNames)
			{
				if(str.Equals(strStd, StrUtil.CaseIgnoreCmp))
					return false;
			}

			if(str.IndexOfAny(m_vInvalidChars) >= 0) return false;

			if(str.Equals(m_strStringName, StrUtil.CaseIgnoreCmp) &&
				!m_vStringDict.Exists(str)) { } // Just changing case
			else
			{
				foreach(string strExisting in m_vStringDict.GetKeys())
				{
					if(str.Equals(strExisting, StrUtil.CaseIgnoreCmp))
					{
						strResult = KPRes.FieldNameExistsAlready;
						return false;
					}
				}
			}

			strResult = string.Empty;
			bError = false;
			return true;
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			if(m_bReadOnly) { Debug.Assert(false); return; }

			string strName = m_cmbStringName.Text;

			if(!ValidateStringNameUI())
			{
				this.DialogResult = DialogResult.None;
				return;
			}

			if(m_strStringName == null) // Add string field
			{
				Debug.Assert(!m_vStringDict.Exists(strName));
			}
			else // Edit string field
			{
				if(!m_strStringName.Equals(strName))
					m_vStringDict.Remove(m_strStringName);
			}

			string strValue = StrUtil.NormalizeNewLines(m_richStringValue.Text, true);
			if(m_psStringInitialValue != null)
			{
				string strValueIn = m_psStringInitialValue.ReadString();

				// If the initial and the new value differ only by
				// new-line encoding, use the initial value to avoid
				// unnecessary changes
				if(StrUtil.NormalizeNewLines(strValue, false) ==
					StrUtil.NormalizeNewLines(strValueIn, false))
					strValue = strValueIn;
			}

			ProtectedString ps = new ProtectedString(m_cbProtect.Checked, strValue);
			m_vStringDict.Set(strName, ps);
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
		}

		private void CleanUpEx()
		{
			m_ctxValue.Detach();
		}

		private void PopulateNamesComboBox()
		{
			ThreadStart ts = new ThreadStart(this.PopulateNamesCollectFunc);
			Thread th = new Thread(ts);
			th.Start();
		}

		private void PopulateNamesCollectFunc()
		{
			try { PopulateNamesCollectFuncPriv(); }
			catch(Exception) { Debug.Assert(false); }
		}

		private void PopulateNamesCollectFuncPriv()
		{
			if(m_pwContext == null) { Debug.Assert(false); return; }

			EntryHandler eh = delegate(PwEntry pe)
			{
				if(pe == null) { Debug.Assert(false); return true; }

				foreach(KeyValuePair<string, ProtectedString> kvp in pe.Strings)
				{
					if(ValidateStringName(kvp.Key) &&
						!m_lSuggestedNames.Contains(kvp.Key))
					{
						// Do not suggest any case-insensitive variant of the
						// initial string, otherwise the string case cannot
						// be changed (due to auto-completion resetting it)
						if(!kvp.Key.Equals(m_strStringName, StrUtil.CaseIgnoreCmp))
							m_lSuggestedNames.Add(kvp.Key);
					}
				}

				return true;
			};

			m_pwContext.RootGroup.TraverseTree(TraversalMethod.PreOrder, null, eh);

			m_lSuggestedNames.Sort();

			if(m_cmbStringName.InvokeRequired)
				m_cmbStringName.Invoke(new VoidDelegate(this.PopulateNamesAddFunc));
			else PopulateNamesAddFunc();
		}

		private void PopulateNamesAddFunc()
		{
			foreach(string str in m_lSuggestedNames)
				m_cmbStringName.Items.Add(str);

			if(m_strStringName == null)
				UIUtil.SetFocus(m_cmbStringName, this, true);
			else UIUtil.SetFocus(m_richStringValue, this, true);
		}

		private void OnBtnHelp(object sender, EventArgs e)
		{
			AppHelp.ShowHelp(AppDefs.HelpTopics.Entry, AppDefs.HelpTopics.EntryStrings);
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			GlobalWindowManager.RemoveWindow(this);
		}

		private void OnNameTextChanged(object sender, EventArgs e)
		{
			ValidateStringNameUI();
		}

		private void OnFormClosing(object sender, FormClosingEventArgs e)
		{
			CleanUpEx();
		}
	}
}
