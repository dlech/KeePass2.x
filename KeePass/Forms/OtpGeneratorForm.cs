/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2023 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Windows.Forms;

using KeePass.App;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;
using KeePass.Util.Spr;

using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Cryptography;
using KeePassLib.Delegates;
using KeePassLib.Resources;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class OtpGeneratorForm : Form
	{
		private readonly KeyValuePair<string, string>[] m_vSecretEncs =
			new KeyValuePair<string, string>[] {
				new KeyValuePair<string, string>(EntryUtil.OtpSecret, "UTF-8"),
				new KeyValuePair<string, string>(EntryUtil.OtpSecretHex, "Base16/Hex"),
				new KeyValuePair<string, string>(EntryUtil.OtpSecretBase32, "Base32"),
				new KeyValuePair<string, string>(EntryUtil.OtpSecretBase64, "Base64")
		};
		private const int g_iSecretEncBase32 = 2;

		private readonly string[] m_vTotpAlg = new string[] { string.Empty,
			HmacOtp.AlgHmacSha1, HmacOtp.AlgHmacSha256, HmacOtp.AlgHmacSha512
		};

		private ProtectedStringDictionary m_d = null;
		private PwDatabase m_pd = null;

		private uint m_uBlockUIUpdate = 0;

		public OtpGeneratorForm()
		{
			InitializeComponent();
			GlobalWindowManager.InitializeForm(this);
		}

		public void InitEx(ProtectedStringDictionary d, PwDatabase pd)
		{
			m_d = d;
			m_pd = pd;
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			if(m_d == null) { Debug.Assert(false); throw new InvalidOperationException(); }
			Debug.Assert(m_pd != null); // Required for Spr-compiling {HMACOTP} preview

			++m_uBlockUIUpdate;

			GlobalWindowManager.AddWindow(this);

			BannerFactory.CreateBannerEx(this, m_bannerImage,
				Properties.Resources.B48x48_KGPG_Gen,
				KPRes.OtpGenSettings, KPRes.OtpGenSettingsDesc);
			this.Icon = AppIcons.Default;
			this.Text = KPRes.OtpGenSettings;

			Debug.Assert(m_lblHotpSecret.Bounds == m_lblTotpSecret.Bounds);
			Debug.Assert(m_tbHotpSecret.Bounds == m_tbTotpSecret.Bounds);
			Debug.Assert(m_cmbHotpSecretEnc.Bounds == m_cmbTotpSecretEnc.Bounds);
			Debug.Assert(m_lblHotpCounter.Location == m_lblTotpLength.Location);
			Debug.Assert(m_tbHotpCounter.Bounds == m_tbTotpLength.Bounds);
			Debug.Assert(m_lblHotpCounterDefault.Location == m_lblTotpLengthDefault.Location);
			Debug.Assert(m_lblHotpPreview.Bounds == m_lblTotpPreview.Bounds);
			Debug.Assert(m_lblHotpPreviewValue.Bounds == m_lblTotpPreviewValue.Bounds);
			Debug.Assert(m_lblHotpUsage.Bounds == m_lblTotpUsage.Bounds);
			Debug.Assert(m_lnkHotpPlh.Bounds == m_lnkTotpPlh.Bounds);

			UIUtil.ConfigureToolTip(m_ttRect);

			FontUtil.SetDefaultFont(m_lblHotpSecret);
			FontUtil.AssignDefaultMono(m_tbHotpSecret, false);
			FontUtil.AssignDefaultMono(m_tbTotpSecret, false);
			FontUtil.AssignDefaultBold(m_lblHotpPreviewValue);
			FontUtil.AssignDefaultBold(m_lblTotpPreviewValue);

			Debug.Assert(!m_cmbHotpSecretEnc.Sorted && !m_cmbTotpSecretEnc.Sorted);
			foreach(KeyValuePair<string, string> kvp in m_vSecretEncs)
			{
				m_cmbHotpSecretEnc.Items.Add(kvp.Value);
				m_cmbTotpSecretEnc.Items.Add(kvp.Value);
			}
			m_cmbHotpSecretEnc.SelectedIndex = g_iSecretEncBase32;
			m_cmbTotpSecretEnc.SelectedIndex = g_iSecretEncBase32;

			GAction<Label, string> fSetDefault = delegate(Label lbl, string strValue)
			{
				UIUtil.SetText(lbl, "(" + KPRes.IfEmpty + ": " + strValue + ")");
			};
			fSetDefault(m_lblHotpCounterDefault, "0");
			fSetDefault(m_lblTotpLengthDefault, EntryUtil.TotpLengthDefault.ToString());
			fSetDefault(m_lblTotpPeriodDefault, HmacOtp.TotpTimeStepDefault.ToString());
			fSetDefault(m_lblTotpAlgDefault, HmacOtp.TotpAlgDefault);

			Debug.Assert(!m_cmbTotpAlg.Sorted);
			foreach(string str in m_vTotpAlg) m_cmbTotpAlg.Items.Add(str);
			m_cmbTotpAlg.SelectedIndex = 0;

			UIUtil.SetText(m_lblHotpUsage, KPRes.OtpUsage.Replace(@"{PARAM}", EntryUtil.HotpPlh));
			UIUtil.SetText(m_lblTotpUsage, KPRes.OtpUsage.Replace(@"{PARAM}", EntryUtil.TotpPlh));

			UIUtil.SetText(m_lnkHotpPlh, KPRes.HelpPlh.Replace(@"{PARAM}", EntryUtil.HotpPlh));
			UIUtil.SetText(m_lnkTotpPlh, KPRes.HelpPlh.Replace(@"{PARAM}", EntryUtil.TotpPlh));

			LoadSettings(m_d, false, true);

			m_tbHotpSecret.TextChanged += this.OnOtpParamChanged;
			m_cmbHotpSecretEnc.SelectedIndexChanged += this.OnOtpParamChanged;
			m_tbHotpCounter.TextChanged += this.OnOtpParamChanged;
			m_tbTotpSecret.TextChanged += this.OnOtpParamChanged;
			m_cmbTotpSecretEnc.SelectedIndexChanged += this.OnOtpParamChanged;
			m_tbTotpLength.TextChanged += this.OnOtpParamChanged;
			m_tbTotpPeriod.TextChanged += this.OnOtpParamChanged;
			m_cmbTotpAlg.TextChanged += this.OnOtpParamChanged;

			--m_uBlockUIUpdate;
			UpdateUI();

			m_tMain.Start();
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			m_tMain.Stop();
			GlobalWindowManager.RemoveWindow(this);
		}

		private void LoadSettings(ProtectedStringDictionary d, bool bUpdateUI,
			bool bSwitchTab)
		{
			if(d == null) { Debug.Assert(false); return; }

			++m_uBlockUIUpdate;

			for(int iType = 0; iType < 2; ++iType)
			{
				TextBox tb = ((iType == 0) ? m_tbHotpSecret : m_tbTotpSecret);
				ComboBox cmb = ((iType == 0) ? m_cmbHotpSecretEnc : m_cmbTotpSecretEnc);
				string strPfx = ((iType == 0) ? EntryUtil.HotpPrefix : EntryUtil.TotpPrefix);
				string strSecret = null;

				for(int iEnc = 0; iEnc < m_vSecretEncs.Length; ++iEnc)
				{
					strSecret = d.ReadSafe(strPfx + m_vSecretEncs[iEnc].Key);
					if(!string.IsNullOrEmpty(strSecret))
					{
						cmb.SelectedIndex = iEnc;
						break;
					}
				}

				tb.Text = (strSecret ?? string.Empty);
			}

			m_tbHotpCounter.Text = d.ReadSafe(EntryUtil.HotpCounter);

			m_tbTotpLength.Text = d.ReadSafe(EntryUtil.TotpLength);
			m_tbTotpPeriod.Text = d.ReadSafe(EntryUtil.TotpPeriod);

			m_cmbTotpAlg.SelectedIndex = -1; // Free text
			m_cmbTotpAlg.Text = d.ReadSafe(EntryUtil.TotpAlg);

			if(bSwitchTab)
				m_tabMain.SelectedTab = (((m_tbHotpSecret.TextLength == 0) ||
					(m_tbTotpSecret.TextLength != 0)) ? m_tabTotp : m_tabHotp);

			--m_uBlockUIUpdate;
			if(bUpdateUI) UpdateUI();
		}

		private void SaveSettings(ProtectedStringDictionary d)
		{
			if(d == null) { Debug.Assert(false); return; }

			EntryUtil.RemoveOtpSecrets(d, EntryUtil.HotpPrefix);
			EntryUtil.RemoveOtpSecrets(d, EntryUtil.TotpPrefix);

			GAction<string, string, bool> f = delegate(string strKey, string strValue,
				bool bProtect)
			{
				if(!string.IsNullOrEmpty(strValue))
					d.Set(strKey, new ProtectedString(bProtect, strValue));
				else d.Remove(strKey);
			};

			f(EntryUtil.HotpPrefix + m_vSecretEncs[m_cmbHotpSecretEnc.SelectedIndex].Key,
				m_tbHotpSecret.Text, true);
			f(EntryUtil.HotpCounter, m_tbHotpCounter.Text, false);

			f(EntryUtil.TotpPrefix + m_vSecretEncs[m_cmbTotpSecretEnc.SelectedIndex].Key,
				m_tbTotpSecret.Text, true);
			f(EntryUtil.TotpLength, m_tbTotpLength.Text, false);
			f(EntryUtil.TotpPeriod, m_tbTotpPeriod.Text, false);
			f(EntryUtil.TotpAlg, m_cmbTotpAlg.Text, false);
		}

		private void UpdateUI()
		{
			if(m_uBlockUIUpdate != 0) return;

			GAction<Control, string> f = delegate(Control c, string strError)
			{
				c.BackColor = (string.IsNullOrEmpty(strError) ?
					AppDefs.ColorControlNormal : AppDefs.ColorEditError);
				UIUtil.SetToolTip(m_ttRect, c, (strError ?? string.Empty), true);
			};

			for(int iType = 0; iType < 2; ++iType)
			{
				TextBox tb = ((iType == 0) ? m_tbHotpSecret : m_tbTotpSecret);
				ComboBox cmb = ((iType == 0) ? m_cmbHotpSecretEnc : m_cmbTotpSecretEnc);

				try
				{
					string strSecret = tb.Text;
					if(!string.IsNullOrEmpty(strSecret))
					{
						byte[] pbSecret = null;
						switch(m_vSecretEncs[cmb.SelectedIndex].Key)
						{
							case EntryUtil.OtpSecret:
								pbSecret = StrUtil.Utf8.GetBytes(strSecret);
								break;

							case EntryUtil.OtpSecretHex:
								if(StrUtil.IsHexString(strSecret, true) &&
									((strSecret.Length & 1) == 0))
									pbSecret = new byte[1];
								break;

							case EntryUtil.OtpSecretBase32:
								Program.EnableAssertions(false);
								try { pbSecret = MemUtil.ParseBase32(strSecret, true); }
								finally { Program.EnableAssertions(true); }
								break;

							case EntryUtil.OtpSecretBase64:
								pbSecret = Convert.FromBase64String(strSecret);
								break;

							default:
								Debug.Assert(false);
								break;
						}

						if((pbSecret == null) || (pbSecret.Length == 0))
							throw new FormatException();
					}

					f(tb, null);
				}
				catch(Exception ex) { f(tb, ex.Message); }
			}

			GAction<string, uint, uint> fCheckUIntInRange = delegate(
				string strValue, uint uMin, uint uMax)
			{
				if(string.IsNullOrEmpty(strValue)) return;
				uint u = uint.Parse(strValue);
				if((u < uMin) || (u > uMax)) throw new ArgumentOutOfRangeException();
			};

			try
			{
				string str = m_tbHotpCounter.Text;
				if(!string.IsNullOrEmpty(str)) ulong.Parse(str);
				f(m_tbHotpCounter, null);
			}
			catch(Exception ex) { f(m_tbHotpCounter, ex.Message); }

			try
			{
				fCheckUIntInRange(m_tbTotpLength.Text, 1, 8);
				f(m_tbTotpLength, null);
			}
			catch(Exception ex) { f(m_tbTotpLength, ex.Message); }

			try
			{
				fCheckUIntInRange(m_tbTotpPeriod.Text, 1, uint.MaxValue);
				f(m_tbTotpPeriod, null);
			}
			catch(Exception ex) { f(m_tbTotpPeriod, ex.Message); }

			try
			{
				string str = (m_cmbTotpAlg.Text ?? string.Empty);
				if(Array.IndexOf(m_vTotpAlg, str) < 0)
					throw new Exception(KLRes.AlgorithmUnknown);
				f(m_cmbTotpAlg, null);
			}
			catch(Exception ex) { f(m_cmbTotpAlg, ex.Message); }

			UpdatePreviews();
		}

		private void UpdatePreviews()
		{
			if(m_cmbHotpSecretEnc.DroppedDown || m_cmbTotpSecretEnc.DroppedDown)
				return; // Text is updated, but not validated (no event raised yet)

			PwEntry pe = new PwEntry(true, true);
			SaveSettings(pe.Strings);

			bool bDbModPre = ((m_pd != null) ? m_pd.Modified : false);
			SprContext ctx = new SprContext(pe, m_pd, SprCompileFlags.HmacOtp);

			Predicate<Control> fValid = delegate(Control c)
			{
				return (c.BackColor != AppDefs.ColorEditError);
			};

			for(int iType = 0; iType < 2; ++iType)
			{
				string strPlh = ((iType == 0) ? EntryUtil.HotpPlh : EntryUtil.TotpPlh);
				Label lbl = ((iType == 0) ? m_lblHotpPreviewValue : m_lblTotpPreviewValue);

				Control[] vValidatable;
				if(iType == 0)
					vValidatable = new Control[] { m_tbHotpSecret, m_tbHotpCounter };
				else
					vValidatable = new Control[] { m_tbTotpSecret, m_tbTotpLength,
						m_tbTotpPeriod, m_cmbTotpAlg };

				string strPreview = null;
				if(Array.TrueForAll(vValidatable, fValid))
				{
					try { strPreview = SprEngine.Compile(strPlh, ctx); }
					catch(Exception) { Debug.Assert(false); }
				}
				if(string.IsNullOrEmpty(strPreview) || (strPreview == strPlh))
					strPreview = "\u2014"; // \u2013

				UIUtil.SetText(lbl, strPreview);
			}

			if(m_pd != null) m_pd.Modified = bDbModPre;
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			SaveSettings(m_d);
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
		}

		private void OnBtnImportOtpAuthUri(object sender, EventArgs e)
		{
			SingleLineEditForm dlg = new SingleLineEditForm();
			dlg.InitEx(KPRes.OtpAuthUri, KPRes.OtpGenSettingsDesc,
				KPRes.OtpAuthUri + ":", Properties.Resources.B48x48_Wizard,
				string.Empty, null);

			if(UIUtil.ShowDialogAndDestroy(dlg) == DialogResult.OK)
			{
				try
				{
					string str = dlg.ResultString;
					if(string.IsNullOrEmpty(str)) return;

					ProtectedStringDictionary d = new ProtectedStringDictionary();
					SaveSettings(d); // Preserve settings of other OTP type(s)

					PwEntry pe = PwEntry.CreateVirtual(null, d);
					EntryUtil.ImportOtpAuth(pe, str, m_pd);

					LoadSettings(d, true, true);
				}
				catch(Exception ex) { MessageService.ShowWarning(ex.Message); }
			}
		}

		private void OnOtpParamChanged(object sender, EventArgs e)
		{
			UpdateUI();
		}

		private void OnTimerMainTick(object sender, EventArgs e)
		{
			UpdatePreviews();
		}

		private void OnPlhLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			AppHelp.ShowHelp(AppDefs.HelpTopics.Placeholders,
				AppDefs.HelpTopics.PlaceholdersOtp);
		}
	}
}
