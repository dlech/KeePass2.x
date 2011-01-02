/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2011 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

using KeePass.App;
using KeePass.UI;
using KeePass.Resources;

using KeePassLib;
using KeePassLib.Delegates;
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
		private PwDatabase m_pwContext = null;

		private volatile List<string> m_vSuggestedNames = new List<string>();

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
		/// <param name="psStringValue">Initial value. May be <c>null</c>.</param>
		public void InitEx(ProtectedStringDictionary vStringDict, string strStringName,
			ProtectedString psStringValue, PwDatabase pwContext)
		{
			Debug.Assert(vStringDict != null); if(vStringDict == null) throw new ArgumentNullException("vStringDict");
			m_vStringDict = vStringDict;

			m_strStringName = strStringName;
			m_psStringValue = psStringValue;

			m_pwContext = pwContext;
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			Debug.Assert(m_vStringDict != null); if(m_vStringDict == null) throw new InvalidOperationException();

			GlobalWindowManager.AddWindow(this);

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
				m_bannerImage.Height, BannerStyle.Default,
				Properties.Resources.B48x48_Font, strTitle, strDesc);
			this.Icon = Properties.Resources.KeePass;

			m_clrNormalBackground = m_cmbStringName.BackColor;

			UIUtil.PrepareStandardMultilineControl(m_richStringValue, true);

			if(m_strStringName != null) m_cmbStringName.Text = m_strStringName;
			if(m_psStringValue != null)
			{
				m_richStringValue.Text = m_psStringValue.ReadString();
				m_cbProtect.Checked = m_psStringValue.IsProtected;
			}

			ValidateStringName();

			PopulateNamesComboBox();

			m_cmbStringName.Focus();
		}

		private bool ValidateStringNameEx(string str)
		{
			if(str == null) { Debug.Assert(false); return false; }

			if(PwDefs.IsStandardField(str)) return false;
			if(str.Length <= 0) return false;

			char[] vInvalidChars = new char[] { '{', '}' };
			if(str.IndexOfAny(vInvalidChars) >= 0) return false;

			string strStart = (m_strStringName != null) ? m_strStringName : string.Empty;
			if(!strStart.Equals(str) && m_vStringDict.Exists(str)) return false;
			// See ValidateStringName

			return true;
		}

		private bool ValidateStringName()
		{
			string str = m_cmbStringName.Text;
			string strStart = (m_strStringName != null) ? m_strStringName : string.Empty;
			char[] vInvalidChars = new char[]{ '{', '}' };

			if(PwDefs.IsStandardField(str))
			{
				m_lblValidationInfo.Text = KPRes.FieldNameInvalid;
				m_cmbStringName.BackColor = AppDefs.ColorEditError;
				m_btnOK.Enabled = false;
				return false;
			}
			else if(str.Length <= 0)
			{
				m_lblValidationInfo.Text = KPRes.FieldNamePrompt;
				m_cmbStringName.BackColor = m_clrNormalBackground;
				m_btnOK.Enabled = false;
				return false;

			}
			else if(str.IndexOfAny(vInvalidChars) >= 0)
			{
				m_lblValidationInfo.Text = KPRes.FieldNameInvalid;
				m_cmbStringName.BackColor = AppDefs.ColorEditError;
				m_btnOK.Enabled = false;
				return false;
			}
			else if(!strStart.Equals(str) && m_vStringDict.Exists(str))
			{
				m_lblValidationInfo.Text = KPRes.FieldNameExistsAlready;
				m_cmbStringName.BackColor = AppDefs.ColorEditError;
				m_btnOK.Enabled = false;
				return false;
			}
			else
			{
				m_lblValidationInfo.Text = string.Empty;
				m_cmbStringName.BackColor = m_clrNormalBackground;
				m_btnOK.Enabled = true;
			}
			// See ValidateStringNameEx

			return true;
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			string strName = m_cmbStringName.Text;

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
			if(m_pwContext == null) { Debug.Assert(false); return; }

			EntryHandler eh = delegate(PwEntry pe)
			{
				if(pe == null) { Debug.Assert(false); return true; }

				foreach(KeyValuePair<string, ProtectedString> kvp in pe.Strings)
				{
					if(ValidateStringNameEx(kvp.Key) &&
						!m_vSuggestedNames.Contains(kvp.Key))
					{
						m_vSuggestedNames.Add(kvp.Key);
					}
				}

				return true;
			};

			m_pwContext.RootGroup.TraverseTree(TraversalMethod.PreOrder, null, eh);

			m_vSuggestedNames.Sort();

			if(m_cmbStringName.InvokeRequired)
				m_cmbStringName.Invoke(new Priv_PnFnVoid(this.PopulateNamesAddFunc));
			else this.PopulateNamesAddFunc();
		}

		public delegate void Priv_PnFnVoid();

		private void PopulateNamesAddFunc()
		{
			foreach(string str in m_vSuggestedNames)
				m_cmbStringName.Items.Add(str);
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
			ValidateStringName();
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if(((keyData == Keys.Return) || (keyData == Keys.Enter)) && m_richStringValue.Focused)
				return false; // Forward to RichTextBox

			return base.ProcessDialogKey(keyData);
		}

		private void OnFormClosing(object sender, FormClosingEventArgs e)
		{
			CleanUpEx();
		}
	}
}