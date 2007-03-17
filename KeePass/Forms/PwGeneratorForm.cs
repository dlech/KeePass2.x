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
using System.Threading;

using KeePass.App;
using KeePass.UI;
using KeePass.Resources;

using KeePassLib;
using KeePassLib.Cryptography;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class PwGeneratorForm : Form
	{
		private const uint MaxPreviewPasswords = 30;

		private List<PasswordGenerationOptions> m_vProfiles =
			new List<PasswordGenerationOptions>();
		private PasswordGenerationOptions m_optInitial = null;
		private PasswordGenerationOptions m_optSelected = new PasswordGenerationOptions();

		private readonly string CustomMeta = "(" + KPRes.Custom + ")";
		private readonly string DeriveFromPrevious = "(" + KPRes.GenPwBasedOnPrevious + ")";

		private bool m_bInitializing = false;
		private bool m_bCanAccept = true;
		
		private const int PreviousOptionsIndex = 1;

		public PasswordGenerationOptions SelectedOptions
		{
			get { return m_optSelected; }
		}

		/// <summary>
		/// Initialize this password generator form instance.
		/// </summary>
		/// <param name="optInitial">Initial options (may be <c>null</c>).</param>
		public void InitEx(PasswordGenerationOptions optInitial, bool bCanAccept)
		{
			m_optInitial = optInitial;
			m_bCanAccept = bCanAccept;
		}

		public PwGeneratorForm()
		{
			InitializeComponent();
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			m_bannerImage.Image = BannerFactory.CreateBanner(m_bannerImage.Width,
				m_bannerImage.Height, BannerFactory.BannerStyle.Default,
				Properties.Resources.B48x48_KGPG_Gen, KPRes.PasswordOptions,
				KPRes.PasswordOptionsDesc);
			this.Icon = Properties.Resources.KeePass;

			m_bInitializing = true;

			for(int i = 0; i < 5; ++i)
				m_clbOptions.SetItemChecked(i, true);

			m_cmbProfiles.Items.Add(CustomMeta);

			if(m_optInitial != null)
			{
				m_cmbProfiles.Items.Add(DeriveFromPrevious);
				SetGenerationOptions(m_optInitial);
			}

			m_cmbProfiles.SelectedIndex = (m_optInitial == null) ? 0 : PreviousOptionsIndex;

			uint uProfileIndex = 0;
			while(true)
			{
				string strProfile = AppConfigEx.GetValue(AppDefs.ConfigKeys.PwGenProfile.Key +
					uProfileIndex.ToString(), null);
				if(strProfile == null) break;

				PasswordGenerationOptions pwgo =
					PasswordGenerationOptions.UnserializeFromString(strProfile);
				if(pwgo.ProfileName.Length == 0) { Debug.Assert(false); ++uProfileIndex; continue; }
				
				m_vProfiles.Add(pwgo);
				m_cmbProfiles.Items.Add(pwgo.ProfileName);

				++uProfileIndex;
			}

			if(m_bCanAccept == false)
			{
				m_btnOK.Visible = false;
				m_btnCancel.Text = KPRes.CloseButton;
			}

			m_bInitializing = false;
			EnableControlsEx(false);
		}

		private void EnableControlsEx(bool bSwitchToCustomProfile)
		{
			if(m_bInitializing) return;

			m_clbOptions.Enabled = m_rbStandardCharSet.Checked;
			m_tbCustomCharSet.Enabled = m_rbCustomCharSet.Checked;
			m_tbPattern.Enabled = m_rbPattern.Checked;

			m_numGenChars.Enabled = !m_rbPattern.Checked;

			if(bSwitchToCustomProfile)
				m_cmbProfiles.SelectedIndex = 0;

			string strProfile = m_cmbProfiles.Text;
			m_btnProfileRemove.Enabled = ((strProfile != CustomMeta) &&
				(strProfile != DeriveFromPrevious));
		}

		private void CleanUpEx()
		{
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			m_optSelected = GetGenerationOptions();

			this.SaveProfiles();
			CleanUpEx();
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
			CleanUpEx();
		}

		private void SaveProfiles()
		{
			AppConfigEx.RemoveIndexedItems(AppDefs.ConfigKeys.PwGenProfile.Key, 0);

			uint uIndex = 0;
			foreach(PasswordGenerationOptions pwgo in m_vProfiles)
			{
				AppConfigEx.SetValue(AppDefs.ConfigKeys.PwGenProfile.Key + uIndex.ToString(),
					PasswordGenerationOptions.SerializeToString(pwgo));
				++uIndex;
			}
		}

		private PasswordGenerationOptions GetGenerationOptions()
		{
			PasswordGenerationOptions opt = new PasswordGenerationOptions();

			if(m_rbStandardCharSet.Checked)
				opt.GeneratorType = PasswordGeneratorType.CharSpaces;
			else if(m_rbCustomCharSet.Checked)
				opt.GeneratorType = PasswordGeneratorType.CustomCharSet;
			else if(m_rbPattern.Checked)
				opt.GeneratorType = PasswordGeneratorType.Pattern;

			opt.CollectUserEntropy = m_cbEntropy.Checked;
			opt.PasswordLength = (uint)m_numGenChars.Value;

			opt.CharSpaces.UpperCase = m_clbOptions.GetItemChecked(0);
			opt.CharSpaces.LowerCase = m_clbOptions.GetItemChecked(1);
			opt.CharSpaces.Numeric = m_clbOptions.GetItemChecked(2);
			opt.CharSpaces.Underline = m_clbOptions.GetItemChecked(3);
			opt.CharSpaces.Minus = m_clbOptions.GetItemChecked(4);
			opt.CharSpaces.Space = m_clbOptions.GetItemChecked(5);
			opt.CharSpaces.Special = m_clbOptions.GetItemChecked(6);
			opt.CharSpaces.Brackets = m_clbOptions.GetItemChecked(7);
			opt.CharSpaces.HighANSI = m_clbOptions.GetItemChecked(8);

			opt.CustomCharSet.Set(m_tbCustomCharSet.Text);
			opt.Pattern = m_tbPattern.Text;

			return opt;
		}

		private void SetGenerationOptions(PasswordGenerationOptions opt)
		{
			bool bPrevInit = m_bInitializing;
			m_bInitializing = true; // Ensure initializing

			m_cbEntropy.Checked = opt.CollectUserEntropy;
			m_numGenChars.Value = opt.PasswordLength;

			m_clbOptions.SetItemChecked(0, opt.CharSpaces.UpperCase);
			m_clbOptions.SetItemChecked(1, opt.CharSpaces.LowerCase);
			m_clbOptions.SetItemChecked(2, opt.CharSpaces.Numeric);
			m_clbOptions.SetItemChecked(3, opt.CharSpaces.Underline);
			m_clbOptions.SetItemChecked(4, opt.CharSpaces.Minus);
			m_clbOptions.SetItemChecked(5, opt.CharSpaces.Space);
			m_clbOptions.SetItemChecked(6, opt.CharSpaces.Special);
			m_clbOptions.SetItemChecked(7, opt.CharSpaces.Brackets);
			m_clbOptions.SetItemChecked(8, opt.CharSpaces.HighANSI);

			m_rbStandardCharSet.Checked = (opt.GeneratorType == PasswordGeneratorType.CharSpaces);
			m_rbCustomCharSet.Checked = (opt.GeneratorType == PasswordGeneratorType.CustomCharSet);
			m_rbPattern.Checked = (opt.GeneratorType == PasswordGeneratorType.Pattern);

			m_tbCustomCharSet.Text = opt.CustomCharSet.GetAsString();
			m_tbPattern.Text = opt.Pattern;

			m_bInitializing = bPrevInit;
		}

		private void OnStandardCharSetCheckedChanged(object sender, EventArgs e)
		{
			EnableControlsEx(true);
		}

		private void OnCustomCharSetCheckedChanged(object sender, EventArgs e)
		{
			EnableControlsEx(true);
		}

		private void OnPatternCheckedChanged(object sender, EventArgs e)
		{
			EnableControlsEx(true);
		}

		private void OnNumGenCharsValueChanged(object sender, EventArgs e)
		{
			EnableControlsEx(true);
		}

		private void OnCharacterTypesItemCheck(object sender, ItemCheckEventArgs e)
		{
			EnableControlsEx(true);
		}

		private void OnCustomCharSetTextChanged(object sender, EventArgs e)
		{
			EnableControlsEx(true);
		}

		private void OnProfilesSelectedIndexChanged(object sender, EventArgs e)
		{
			string strProfile = m_cmbProfiles.Text;

			if(strProfile.Equals(CustomMeta)) return;
			else if(strProfile.Equals(DeriveFromPrevious))
			{
				SetGenerationOptions(m_optInitial);
				m_cmbProfiles.SelectedIndex = PreviousOptionsIndex;
				return;
			}

			foreach(PasswordGenerationOptions pwgo in m_vProfiles)
			{
				if(pwgo.ProfileName.Equals(strProfile))
				{
					SetGenerationOptions(pwgo);
					EnableControlsEx(false);
				}
			}
		}

		private void OnBtnProfileAdd(object sender, EventArgs e)
		{
			SingleLineEditForm slef = new SingleLineEditForm();

			slef.InitEx(KPRes.GenProfileAdd, KPRes.GenProfileAddDesc, KPRes.GenProfileAddDescLong,
				KeePass.Properties.Resources.B48x48_KGPG_Gen);

			if(slef.ShowDialog() == DialogResult.OK)
			{
				string strProfile = slef.ResultString;

				if(strProfile.Equals(CustomMeta) || strProfile.Equals(DeriveFromPrevious) ||
					(strProfile.Length == 0))
				{
					MessageBox.Show(KPRes.FieldNameInvalid, PwDefs.ShortProductName,
						MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
				else
				{
					bool bExists = false;
					for(int i = 0; i < m_vProfiles.Count; ++i)
						if(m_vProfiles[i].ProfileName == strProfile)
							bExists = true;

					if(bExists)
						MessageBox.Show(KPRes.FieldNameExistsAlready, PwDefs.ShortProductName,
							MessageBoxButtons.OK, MessageBoxIcon.Warning);
					else
					{
						PasswordGenerationOptions pwgo = GetGenerationOptions();
						pwgo.ProfileName = strProfile;

						if(m_rbStandardCharSet.Checked)
							pwgo.GeneratorType = PasswordGeneratorType.CharSpaces;
						else if(m_rbCustomCharSet.Checked)
							pwgo.GeneratorType = PasswordGeneratorType.CustomCharSet;
						else if(m_rbPattern.Checked)
							pwgo.GeneratorType = PasswordGeneratorType.Pattern;
						else { Debug.Assert(false); }
						
						m_vProfiles.Add(pwgo);

						m_cmbProfiles.Items.Add(strProfile);
						m_cmbProfiles.SelectedIndex = m_cmbProfiles.Items.Count - 1;
					}
				}
			}
		}

		private void OnBtnProfileRemove(object sender, EventArgs e)
		{
			string strProfile = m_cmbProfiles.Text;

			if(strProfile.Equals(CustomMeta) || strProfile.Equals(DeriveFromPrevious))
				return;

			m_cmbProfiles.SelectedIndex = 0;
			for(int i = 0; i < m_cmbProfiles.Items.Count; ++i)
			{
				if(strProfile.Equals(m_cmbProfiles.Items[i].ToString()))
				{
					m_cmbProfiles.Items.RemoveAt(i);

					for(int j = 0; j < m_vProfiles.Count; ++j)
						if(m_vProfiles[j].ProfileName == strProfile)
						{
							m_vProfiles.RemoveAt(j);
							break;
						}

					break;
				}
			}
		}

		private void OnBtnHelp(object sender, EventArgs e)
		{
			AppHelp.ShowHelp(AppDefs.HelpTopics.PwGenerator, null);
		}

		private void OnTabMainSelectedIndexChanged(object sender, EventArgs e)
		{
			if(m_tabMain.SelectedTab == m_tabPreview)
				GeneratePreviewPasswords();
		}

		private void GeneratePreviewPasswords()
		{
			m_pbPreview.Value = 0;
			m_tbPreview.Text = string.Empty;

			PasswordGenerationOptions opt = GetGenerationOptions();
			StringBuilder sbList = new StringBuilder();

			System.Windows.Forms.Cursor cNormalCursor = this.Cursor;
			this.Cursor = Cursors.WaitCursor;

			for(uint i = 0; i < MaxPreviewPasswords; ++i)
			{
				Application.DoEvents();

				sbList.AppendLine(PasswordGenerator.Generate(opt, false, null).ReadString());
				m_pbPreview.Value = (int)((100 * i) / MaxPreviewPasswords);
			}
			
			m_pbPreview.Value = 100;
			m_tbPreview.Text = sbList.ToString();

			this.Cursor = cNormalCursor;
		}
	}
}
