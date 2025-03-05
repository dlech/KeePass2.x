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
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using KeePass.App;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util.MultipleValues;

using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Delegates;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class EditStringForm : Form
	{
		private ProtectedStringDictionary m_dStrings = null;
		private string m_strInitName = null;
		private ProtectedString m_psInitValue = null;
		private PwDatabase m_pdContext = null;

		private readonly List<string> m_lSuggestedNames = new List<string>();
		private readonly List<string> m_lStdNames = PwDefs.GetStandardFields();
		private readonly char[] m_vInvalidChars = new char[] { '{', '}' };

		private readonly RichTextBoxContextMenu m_ctxValue = new RichTextBoxContextMenu();
		private PwGeneratorMenu m_pgm = null;

		private bool m_bReadOnly = false;
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DefaultValue(false)]
		public bool ReadOnlyEx
		{
			get { return m_bReadOnly; }
			set { m_bReadOnly = value; }
		}

		// The following is used when editing multiple strings with the
		// same name in different entries (not when editing multiple
		// strings within the same entry)
		private MultipleValuesEntryContext m_mvec = null;
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DefaultValue((object)null)]
		internal MultipleValuesEntryContext MultipleValuesEntryContext
		{
			get { return m_mvec; }
			set { m_mvec = value; }
		}

		public EditStringForm()
		{
			InitializeComponent();
			GlobalWindowManager.InitializeForm(this);
		}

		/// <summary>
		/// Initialize the dialog. Needs to be called before the dialog is shown.
		/// </summary>
		public void InitEx(ProtectedStringDictionary dStrings, string strInitName,
			ProtectedString psInitValue, PwDatabase pdContext)
		{
			if(dStrings == null) { Debug.Assert(false); throw new ArgumentNullException("dStrings"); }

			m_dStrings = dStrings;
			m_strInitName = strInitName;
			m_psInitValue = psInitValue;
			m_pdContext = pdContext;
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			if(m_dStrings == null) { Debug.Assert(false); throw new InvalidOperationException(); }

			GlobalWindowManager.AddWindow(this);

			string strTitle, strDesc;
			if(m_strInitName == null)
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

			UIUtil.ConfigureToolTip(m_ttRect);

			UIUtil.EnableAutoCompletion(m_cmbName, true);

			UIUtil.PrepareStandardMultilineControl(m_rtbValue, true, true);
			m_ctxValue.Attach(m_rtbValue, this);

			GFunc<PwEntry> fGetContextEntry = delegate()
			{
				return PwEntry.CreateVirtual(((m_pdContext != null) ? m_pdContext.RootGroup :
					null) ?? new PwGroup(true, true), m_dStrings);
			};
			m_pgm = new PwGeneratorMenu(m_btnGenPw, m_ttRect, m_rtbValue,
				fGetContextEntry, m_pdContext, (m_mvec != null));

			if(m_strInitName != null) m_cmbName.Text = m_strInitName;
			if(m_psInitValue != null)
			{
				m_rtbValue.Text = StrUtil.NormalizeNewLines(
					m_psInitValue.ReadString(), true);
				UIUtil.SetChecked(m_cbProtect, m_psInitValue.IsProtected);
			}

			ValidateStringNameUI();
			PopulateNamesComboBox();

			if(m_mvec != null)
			{
				m_cmbName.Enabled = false;
				MultipleValuesEx.ConfigureText(m_rtbValue, true);

				bool bMultiProt;
				m_mvec.MultiStringProt.TryGetValue(m_cmbName.Text, out bMultiProt);
				if(bMultiProt)
					MultipleValuesEx.ConfigureState(m_cbProtect, true);
			}

			if(m_bReadOnly)
			{
				m_cmbName.Enabled = false;
				m_rtbValue.ReadOnly = true;
				m_cbProtect.Enabled = false;
				m_btnGenPw.Enabled = false;
				// m_btnOK.Enabled = false; // See ValidateStringNameUI
			}

			// UIUtil.SetFocus(..., this); // See PopulateNamesComboBox
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			m_ctxValue.Detach();

			if(m_pgm != null) { m_pgm.Dispose(); m_pgm = null; }
			else { Debug.Assert(false); }

			GlobalWindowManager.RemoveWindow(this);
		}

		private bool ValidateStringNameUI()
		{
			string strResult;
			bool bError;
			bool b = ValidateStringName(m_cmbName.Text, out strResult, out bError);

			Debug.Assert(!m_lblValidationInfo.AutoSize); // For RTL support
			m_lblValidationInfo.Text = strResult;
			if(bError) m_cmbName.BackColor = AppDefs.ColorEditError;
			else m_cmbName.ResetBackColor();

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

			if(str == m_strInitName) // Case-sensitive
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

			if(str.Equals(m_strInitName, StrUtil.CaseIgnoreCmp) &&
				!m_dStrings.Exists(str)) { } // Just changing case
			else
			{
				foreach(string strExisting in m_dStrings.GetKeys())
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

			string strName = m_cmbName.Text;

			if(!ValidateStringNameUI())
			{
				this.DialogResult = DialogResult.None;
				return;
			}

			if(m_strInitName == null) // Add string field
			{
				Debug.Assert(!m_dStrings.Exists(strName));
			}
			else // Edit string field
			{
				if(strName != m_strInitName)
					m_dStrings.Remove(m_strInitName);
			}

			string strValue = StrUtil.NormalizeNewLines(m_rtbValue.Text, true);
			if(m_psInitValue != null)
			{
				string strValueIn = m_psInitValue.ReadString();

				// If the initial and the new value differ only by
				// new-line encoding, use the initial value to avoid
				// unnecessary changes
				if(StrUtil.NormalizeNewLines(strValue, false) ==
					StrUtil.NormalizeNewLines(strValueIn, false))
					strValue = strValueIn;
			}

			CheckState cs = m_cbProtect.CheckState;

			ProtectedString ps = new ProtectedString((cs == CheckState.Checked), strValue);
			m_dStrings.Set(strName, ps);

			if(m_mvec != null)
				m_mvec.MultiStringProt[strName] = (cs == CheckState.Indeterminate);
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
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
			if(m_pdContext == null) { Debug.Assert(false); return; }

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
						if(!kvp.Key.Equals(m_strInitName, StrUtil.CaseIgnoreCmp))
							m_lSuggestedNames.Add(kvp.Key);
					}
				}

				return true;
			};

			m_pdContext.RootGroup.TraverseTree(TraversalMethod.PreOrder, null, eh);

			m_lSuggestedNames.Sort();

			if(m_cmbName.InvokeRequired)
				m_cmbName.Invoke(new VoidDelegate(this.PopulateNamesAddFunc));
			else PopulateNamesAddFunc();
		}

		private void PopulateNamesAddFunc()
		{
			List<object> l = m_lSuggestedNames.ConvertAll<object>(
				delegate(string str) { return (object)str; });
			m_cmbName.Items.AddRange(l.ToArray());

			if(m_strInitName == null)
				UIUtil.SetFocus(m_cmbName, this, true);
			else UIUtil.SetFocus(m_rtbValue, this, true);
		}

		private void OnBtnHelp(object sender, EventArgs e)
		{
			AppHelp.ShowHelp(AppDefs.HelpTopics.Entry, AppDefs.HelpTopics.EntryStrings);
		}

		private void OnNameTextChanged(object sender, EventArgs e)
		{
			ValidateStringNameUI();
		}
	}
}
