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
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

using KeePass.App;
using KeePass.App.Configuration;
using KeePass.Forms;
using KeePass.Resources;

using KeePassLib;
using KeePassLib.Cryptography;
using KeePassLib.Utility;

namespace KeePass.UI
{
	public sealed class PwInputControlGroup
	{
		private TextBox m_tbPassword = null;
		private CheckBox m_cbHide = null;
		private Label m_lblRepeat = null;
		private TextBox m_tbRepeat = null;
		private Label m_lblQualityPrompt = null;
		private QualityProgressBar m_pbQuality = null;
		private Label m_lblQualityBits = null;
		private Form m_fParent = null;

		private SecureEdit m_secPassword = null;
		private SecureEdit m_secRepeat = null;

		private bool m_bInitializing = false;
		private bool m_bPrgmCheck = false;

		private bool m_bEnabled = true;
		public bool Enabled
		{
			get { return m_bEnabled; }
			set
			{
				if(value != m_bEnabled)
				{
					m_bEnabled = value;
					UpdateUI();
				}
			}
		}

		public uint PasswordLength
		{
			get
			{
				if(m_secPassword == null) { Debug.Assert(false); return 0; }
				return m_secPassword.TextLength;
			}
		}

		private bool AutoRepeat
		{
			get
			{
				if(m_cbHide == null) { Debug.Assert(false); return false; }
				return !m_cbHide.Checked;
			}
		}

		public PwInputControlGroup()
		{
		}

#if DEBUG
		~PwInputControlGroup()
		{
			Debug.Assert(m_tbPassword == null); // Owner should call Release()
			Debug.Assert(m_uBlockUIUpdate == 0);
		}
#endif

		public void Attach(TextBox tbPassword, CheckBox cbHide, Label lblRepeat,
			TextBox tbRepeat, Label lblQualityPrompt, QualityProgressBar pbQuality,
			Label lblQualityBits, Form fParent, bool bInitialHide, bool bSecureDesktopMode)
		{
			if(tbPassword == null) throw new ArgumentNullException("tbPassword");
			if(cbHide == null) throw new ArgumentNullException("cbHide");
			if(lblRepeat == null) throw new ArgumentNullException("lblRepeat");
			if(tbRepeat == null) throw new ArgumentNullException("tbRepeat");
			if(lblQualityPrompt == null) throw new ArgumentNullException("lblQualityPrompt");
			if(pbQuality == null) throw new ArgumentNullException("pbQuality");
			if(lblQualityBits == null) throw new ArgumentNullException("lblQualityBits");
			if(fParent == null) throw new ArgumentNullException("fParent");

			Release();
			m_bInitializing = true;

			m_tbPassword = tbPassword;
			m_cbHide = cbHide;
			m_lblRepeat = lblRepeat;
			m_tbRepeat = tbRepeat;
			m_lblQualityPrompt = lblQualityPrompt;
			m_pbQuality = pbQuality;
			m_lblQualityBits = lblQualityBits;
			m_fParent = fParent;

			m_secPassword = new SecureEdit();
			m_secPassword.SecureDesktopMode = bSecureDesktopMode;
			m_secPassword.Attach(m_tbPassword, this.OnPasswordTextChanged, bInitialHide);

			m_secRepeat = new SecureEdit();
			m_secRepeat.SecureDesktopMode = bSecureDesktopMode;
			m_secRepeat.Attach(m_tbRepeat, this.OnRepeatTextChanged, bInitialHide);

			m_cbHide.Checked = bInitialHide;
			m_cbHide.CheckedChanged += this.OnHideCheckedChanged;

			Debug.Assert(m_pbQuality.Minimum == 0);
			Debug.Assert(m_pbQuality.Maximum == 100);

			m_bInitializing = false;
			UpdateUI();
		}

		public void Release()
		{
			Debug.Assert(!m_bInitializing);
			if(m_tbPassword == null) return;

			m_secPassword.Detach();
			m_secRepeat.Detach();

			m_cbHide.CheckedChanged -= this.OnHideCheckedChanged;

			m_tbPassword = null;
			m_cbHide = null;
			m_lblRepeat = null;
			m_tbRepeat = null;
			m_lblQualityPrompt = null;
			m_pbQuality = null;
			m_lblQualityBits = null;
			m_fParent = null;

			m_secPassword = null;
			m_secRepeat = null;
		}

		private uint m_uBlockUIUpdate = 0;
		private void UpdateUI()
		{
			if((m_uBlockUIUpdate > 0) || m_bInitializing) return;
			++m_uBlockUIUpdate;

			ulong uFlags = 0;
			if(m_fParent is KeyCreationForm)
				uFlags = Program.Config.UI.KeyCreationFlags;

			byte[] pbUtf8 = m_secPassword.ToUtf8();

			m_tbPassword.Enabled = m_bEnabled;
			m_cbHide.Enabled = (m_bEnabled && ((uFlags &
				(ulong)AceKeyUIFlags.DisableHidePassword) == 0));

			if((uFlags & (ulong)AceKeyUIFlags.CheckHidePassword) != 0)
			{
				m_bPrgmCheck = true;
				m_cbHide.Checked = true;
				m_bPrgmCheck = false;
			}
			if((uFlags & (ulong)AceKeyUIFlags.UncheckHidePassword) != 0)
			{
				m_bPrgmCheck = true;
				m_cbHide.Checked = false;
				m_bPrgmCheck = false;
			}

			bool bAutoRepeat = this.AutoRepeat;
			if(bAutoRepeat && (m_secRepeat.TextLength > 0))
				m_secRepeat.SetPassword(new byte[0]);

			byte[] pbRepeat = m_secRepeat.ToUtf8();
			if(!MemUtil.ArraysEqual(pbUtf8, pbRepeat) && !bAutoRepeat)
				m_tbRepeat.BackColor = AppDefs.ColorEditError;
			else m_tbRepeat.ResetBackColor();

			bool bRepeatEnable = (m_bEnabled && !bAutoRepeat);
			m_lblRepeat.Enabled = bRepeatEnable;
			m_tbRepeat.Enabled = bRepeatEnable;

			m_lblQualityPrompt.Enabled = m_bEnabled;
			m_pbQuality.Enabled = m_bEnabled;
			m_lblQualityBits.Enabled = m_bEnabled;

			uint uBits = QualityEstimation.EstimatePasswordBits(pbUtf8);
			m_lblQualityBits.Text = uBits.ToString() + " " + KPRes.Bits;
			int iPos = (int)((100 * uBits) / (256 / 2));
			if(iPos < 0) iPos = 0; else if(iPos > 100) iPos = 100;
			m_pbQuality.Value = iPos;

			MemUtil.ZeroByteArray(pbUtf8);
			MemUtil.ZeroByteArray(pbRepeat);
			--m_uBlockUIUpdate;
		}

		private void OnPasswordTextChanged(object sender, EventArgs e)
		{
			UpdateUI();
		}

		private void OnRepeatTextChanged(object sender, EventArgs e)
		{
			UpdateUI();
		}

		private void OnHideCheckedChanged(object sender, EventArgs e)
		{
			if(m_bInitializing) return;

			bool bHide = m_cbHide.Checked;
			if(!bHide && !m_bPrgmCheck && !AppPolicy.Try(AppPolicyId.UnhidePasswords))
			{
				m_cbHide.Checked = true;
				return;
			}

			m_secPassword.EnableProtection(bHide);
			m_secRepeat.EnableProtection(bHide);

			if(bHide && !m_bPrgmCheck)
			{
				++m_uBlockUIUpdate;
				byte[] pb = GetPasswordUtf8();
				m_secRepeat.SetPassword(pb);
				MemUtil.ZeroByteArray(pb);
				--m_uBlockUIUpdate;
			}

			UpdateUI();
			if(!m_bPrgmCheck) UIUtil.SetFocus(m_tbPassword, m_fParent);
		}

		public void SetPassword(byte[] pbUtf8, bool bSetRepeatPw)
		{
			if(pbUtf8 == null) { Debug.Assert(false); return; }

			++m_uBlockUIUpdate;
			m_secPassword.SetPassword(pbUtf8);
			if(bSetRepeatPw && !this.AutoRepeat)
				m_secRepeat.SetPassword(pbUtf8);
			--m_uBlockUIUpdate;

			UpdateUI();
		}

		public void SetPasswords(string strPassword, string strRepeat)
		{
			byte[] pbP = ((strPassword != null) ? StrUtil.Utf8.GetBytes(
				strPassword) : null);
			byte[] pbR = ((strRepeat != null) ? StrUtil.Utf8.GetBytes(
				strRepeat) : null);
			SetPasswords(pbP, pbR);
		}

		public void SetPasswords(byte[] pbPasswordUtf8, byte[] pbRepeatUtf8)
		{
			++m_uBlockUIUpdate;
			if(pbPasswordUtf8 != null)
				m_secPassword.SetPassword(pbPasswordUtf8);
			if((pbRepeatUtf8 != null) && !this.AutoRepeat)
				m_secRepeat.SetPassword(pbRepeatUtf8);
			--m_uBlockUIUpdate;

			UpdateUI();
		}

		public string GetPassword()
		{
			return StrUtil.Utf8.GetString(m_secPassword.ToUtf8());
		}

		public byte[] GetPasswordUtf8()
		{
			return m_secPassword.ToUtf8();
		}

		public string GetRepeat()
		{
			if(this.AutoRepeat) return GetPassword();
			return StrUtil.Utf8.GetString(m_secRepeat.ToUtf8());
		}

		public byte[] GetRepeatUtf8()
		{
			if(this.AutoRepeat) return GetPasswordUtf8();
			return m_secRepeat.ToUtf8();
		}

		public bool ValidateData(bool bUIOnError)
		{
			if(this.AutoRepeat) return true;
			if(m_secPassword.ContentsEqualTo(m_secRepeat)) return true;

			if(bUIOnError)
			{
				if(!VistaTaskDialog.ShowMessageBox(KPRes.PasswordRepeatFailed,
					KPRes.ValidationFailed, PwDefs.ShortProductName,
					VtdIcon.Warning, m_fParent.Handle))
					MessageService.ShowWarning(KPRes.PasswordRepeatFailed);
			}

			return false;
		}
	}
}
