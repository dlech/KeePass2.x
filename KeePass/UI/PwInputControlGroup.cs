/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2024 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using KeePass.App;
using KeePass.App.Configuration;
using KeePass.Forms;
using KeePass.Resources;
using KeePass.Util.MultipleValues;
using KeePass.Util.Spr;

using KeePassLib;
using KeePassLib.Cryptography;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.UI
{
	public sealed class PwInputControlGroup
	{
		private SecureTextBoxEx m_tbPassword = null;
		private CheckBox m_cbHide = null;
		private Label m_lblRepeat = null;
		private SecureTextBoxEx m_tbRepeat = null;
		private Label m_lblQualityPrompt = null;
		private QualityProgressBar m_pbQuality = null;
		private Label m_lblQualityInfo = null;
		private ToolTip m_ttHint = null;
		private Form m_fParent = null;

		private bool m_bInitializing = false;
		private uint m_uPrgmCheck = 0;

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

		private bool m_bQualityEnabled = true;
		public bool QualityEnabled
		{
			get { return m_bQualityEnabled; }
			set
			{
				if(value != m_bQualityEnabled)
				{
					m_bQualityEnabled = value;
					UpdateUI();
				}
			}
		}

		private bool m_bSprVar = false;
		public bool IsSprVariant
		{
			get { return m_bSprVar; }
			set { m_bSprVar = value; }
		}

		private bool AutoRepeat
		{
			get
			{
				if(!Program.Config.UI.RepeatPasswordOnlyWhenHidden)
					return false;

				if(m_cbHide == null) { Debug.Assert(false); return false; }
				return !m_cbHide.Checked;
			}
		}

		private PwDatabase m_ctxDatabase = null;
		public PwDatabase ContextDatabase
		{
			get { return m_ctxDatabase; }
			set { m_ctxDatabase = value; }
		}

		private PwEntry m_ctxEntry = null;
		public PwEntry ContextEntry
		{
			get { return m_ctxEntry; }
			set { m_ctxEntry = value; }
		}

		public PwInputControlGroup()
		{
		}

#if DEBUG
		~PwInputControlGroup()
		{
			Debug.Assert(m_tbPassword == null); // Owner should call Release()
			Debug.Assert(m_uBlockUIUpdate == 0);
			Debug.Assert(m_lUqiTasks.Count == 0);
		}
#endif

		public void Attach(SecureTextBoxEx tbPassword, CheckBox cbHide, Label lblRepeat,
			SecureTextBoxEx tbRepeat, Label lblQualityPrompt, QualityProgressBar pbQuality,
			Label lblQualityInfo, ToolTip ttHint, Form fParent, bool bInitialHide,
			bool bSecureDesktopMode)
		{
			if(tbPassword == null) throw new ArgumentNullException("tbPassword");
			if(cbHide == null) throw new ArgumentNullException("cbHide");
			if(lblRepeat == null) throw new ArgumentNullException("lblRepeat");
			if(tbRepeat == null) throw new ArgumentNullException("tbRepeat");
			if(lblQualityPrompt == null) throw new ArgumentNullException("lblQualityPrompt");
			if(pbQuality == null) throw new ArgumentNullException("pbQuality");
			if(lblQualityInfo == null) throw new ArgumentNullException("lblQualityInfo");
			Debug.Assert(ttHint != null);
			if(fParent == null) throw new ArgumentNullException("fParent");

			Release();
			m_bInitializing = true;

			m_tbPassword = tbPassword;
			m_cbHide = cbHide;
			m_lblRepeat = lblRepeat;
			m_tbRepeat = tbRepeat;
			m_lblQualityPrompt = lblQualityPrompt;
			m_pbQuality = pbQuality;
			m_lblQualityInfo = lblQualityInfo;
			m_ttHint = ttHint;
			m_fParent = fParent;

			m_tbPassword.TextChanged += this.OnPasswordTextChanged;
			m_tbPassword.EnableProtection(bInitialHide);

			m_tbRepeat.TextChanged += this.OnRepeatTextChanged;
			m_tbRepeat.EnableProtection(bInitialHide);

			ConfigureHideButton(m_cbHide, m_ttHint);

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

			m_tbPassword.TextChanged -= this.OnPasswordTextChanged;
			m_tbRepeat.TextChanged -= this.OnRepeatTextChanged;

			m_cbHide.CheckedChanged -= this.OnHideCheckedChanged;

			m_tbPassword = null;
			m_cbHide = null;
			m_lblRepeat = null;
			m_tbRepeat = null;
			m_lblQualityPrompt = null;
			m_pbQuality = null;
			m_lblQualityInfo = null;
			m_ttHint = null;
			m_fParent = null;
		}

		private uint m_uBlockUIUpdate = 0;
		private void UpdateUI()
		{
			if((m_uBlockUIUpdate > 0) || m_bInitializing) return;
			++m_uBlockUIUpdate;

			ulong uFlags = 0;
			if(m_fParent is KeyCreationForm)
				uFlags = Program.Config.UI.KeyCreationFlags;

			byte[] pbUtf8 = m_tbPassword.TextEx.ReadUtf8();
			char[] v = StrUtil.Utf8.GetChars(pbUtf8);

#if DEBUG
			byte[] pbTest = StrUtil.Utf8.GetBytes(v);
			Debug.Assert(MemUtil.ArraysEqual(pbUtf8, pbTest));
			MemUtil.ZeroByteArray(pbTest);
#endif

			m_tbPassword.Enabled = m_bEnabled;
			m_cbHide.Enabled = (m_bEnabled && ((uFlags &
				(ulong)AceKeyUIFlags.DisableHidePassword) == 0));

			bool bAutoRepeat = this.AutoRepeat;
			if(bAutoRepeat && (m_tbRepeat.TextLength > 0))
				m_tbRepeat.Text = string.Empty;

			byte[] pbRepeat = m_tbRepeat.TextEx.ReadUtf8();
			string strTip = KPRes.PasswordRepeatHint;
			if(MemUtil.ArraysEqual(pbUtf8, pbRepeat) || bAutoRepeat)
				m_tbRepeat.ResetBackColor();
			else
			{
				m_tbRepeat.BackColor = AppDefs.ColorEditError;
				strTip += MessageService.NewLine + KPRes.ValidationFailed +
					"! " + KPRes.PasswordRepeatFailed;
			}
			UIUtil.SetToolTip(m_ttHint, m_tbRepeat, strTip, true);

			bool bRepeatEnable = (m_bEnabled && !bAutoRepeat);
			m_lblRepeat.Enabled = bRepeatEnable;
			m_tbRepeat.Enabled = bRepeatEnable;

			bool bQuality = (m_bEnabled && m_bQualityEnabled);

			byte[] pbMultiple = StrUtil.Utf8.GetBytes(MultipleValuesEx.CueString);
			if(MemUtil.ArraysEqual(pbUtf8, pbMultiple)) bQuality = false;

			if(m_bSprVar && bQuality)
			{
				if(SprEngine.MightChange(v, null)) // Perf. opt.
				{
					// {S:...} and {REF:...} may reference the entry that
					// is currently being edited and SprEngine will not see
					// the current data entered in the dialog; thus we
					// disable quality estimation for all strings containing
					// one of these placeholders
					string str = new string(v);
					if((str.IndexOf(@"{S:", StrUtil.CaseIgnoreCmp) >= 0) ||
						(str.IndexOf(@"{REF:", StrUtil.CaseIgnoreCmp) >= 0))
						bQuality = false;
					else
					{
						SprContext ctx = new SprContext(m_ctxEntry, m_ctxDatabase,
							SprCompileFlags.NonActive, false, false);
						string strCmp = SprEngine.Compile(str, ctx);
						if(strCmp != str) bQuality = false;
					}
				}
#if DEBUG
				else
				{
					string str = new string(v);
					SprContext ctx = new SprContext(m_ctxEntry, m_ctxDatabase,
						SprCompileFlags.NonActive, false, false);
					string strCmp = SprEngine.Compile(str, ctx);
					Debug.Assert(strCmp == str);
				}
#endif
			}

			m_lblQualityPrompt.Enabled = bQuality;
			m_pbQuality.Enabled = bQuality;
			m_lblQualityInfo.Enabled = bQuality;

			if((Program.Config.UI.UIFlags & (ulong)AceUIFlags.HidePwQuality) != 0)
			{
				m_lblQualityPrompt.Visible = false;
				m_pbQuality.Visible = false;
				m_lblQualityInfo.Visible = false;
			}
			else if(bQuality || !m_bSprVar) UpdateQualityInfo(v);
			else UqiShowQuality(0, 0);

			MemUtil.ZeroByteArray(pbUtf8);
			MemUtil.ZeroByteArray(pbRepeat);
			MemUtil.ZeroArray<char>(v);
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
			if(!bHide && (m_uPrgmCheck == 0))
			{
				if(!AppPolicy.Try(AppPolicyId.UnhidePasswords))
				{
					++m_uPrgmCheck;
					m_cbHide.Checked = true;
					--m_uPrgmCheck;
					return;
				}
			}

			m_tbPassword.EnableProtection(bHide);
			m_tbRepeat.EnableProtection(bHide);

			bool bWasAutoRepeat = Program.Config.UI.RepeatPasswordOnlyWhenHidden;
			if(bHide && (m_uPrgmCheck == 0) && bWasAutoRepeat)
			{
				++m_uBlockUIUpdate;
				m_tbRepeat.TextEx = m_tbPassword.TextEx;
				--m_uBlockUIUpdate;
			}

			UpdateUI();
			if(m_uPrgmCheck == 0) UIUtil.SetFocus(m_tbPassword, m_fParent);
		}

		public void SetPassword(ProtectedString ps, bool bSetRepeatPw)
		{
			if(ps == null) { Debug.Assert(false); return; }

			++m_uBlockUIUpdate;
			m_tbPassword.TextEx = ps;
			if(bSetRepeatPw && !this.AutoRepeat) m_tbRepeat.TextEx = ps;
			--m_uBlockUIUpdate;

			UpdateUI();
		}

		public void SetPasswords(string strPassword, string strRepeat)
		{
			ProtectedString psP = ((strPassword != null) ? new ProtectedString(
				false, strPassword) : null);
			ProtectedString psR = ((strRepeat != null) ? new ProtectedString(
				false, strRepeat) : null);
			SetPasswords(psP, psR);
		}

		public void SetPasswords(ProtectedString psPassword, ProtectedString psRepeat)
		{
			++m_uBlockUIUpdate;
			if(psPassword != null)
				m_tbPassword.TextEx = psPassword;
			if((psRepeat != null) && !this.AutoRepeat)
				m_tbRepeat.TextEx = psRepeat;
			--m_uBlockUIUpdate;

			UpdateUI();
		}

		[Obsolete]
		public string GetPassword()
		{
			return m_tbPassword.TextEx.ReadString();
		}

		public ProtectedString GetPasswordEx()
		{
			return m_tbPassword.TextEx;
		}

		public byte[] GetPasswordUtf8()
		{
			return m_tbPassword.TextEx.ReadUtf8();
		}

		[Obsolete]
		public string GetRepeat()
		{
			if(this.AutoRepeat) return m_tbPassword.TextEx.ReadString();
			return m_tbRepeat.TextEx.ReadString();
		}

		public ProtectedString GetRepeatEx()
		{
			if(this.AutoRepeat) return GetPasswordEx();
			return m_tbRepeat.TextEx;
		}

		public byte[] GetRepeatUtf8()
		{
			if(this.AutoRepeat) return GetPasswordUtf8();
			return m_tbRepeat.TextEx.ReadUtf8();
		}

		public bool ValidateData(bool bUIOnError)
		{
			if(this.AutoRepeat) return true;
			if(m_tbPassword.TextEx.Equals(m_tbRepeat.TextEx, false)) return true;

			if(bUIOnError)
			{
				if(!VistaTaskDialog.ShowMessageBox(KPRes.PasswordRepeatFailed,
					KPRes.ValidationFailed, PwDefs.ShortProductName,
					VtdIcon.Warning, m_fParent))
					MessageService.ShowWarning(KPRes.PasswordRepeatFailed);
			}

			return false;
		}

		private static string GetUqiTaskID(char[] v)
		{
			byte[] pb = StrUtil.Utf8.GetBytes(v);
			byte[] pbHash = CryptoUtil.HashSha256(pb);
			MemUtil.ZeroByteArray(pb);
			return Convert.ToBase64String(pbHash);
		}

		private readonly List<string> m_lUqiTasks = new List<string>();
		private readonly object m_oUqiTasksSync = new object();
		private void UpdateQualityInfo(char[] v)
		{
			if(v == null) { Debug.Assert(false); return; }

			int nTasks;
			lock(m_oUqiTasksSync)
			{
				string strTask = GetUqiTaskID(v);
				if(m_lUqiTasks.Contains(strTask)) return;

				nTasks = m_lUqiTasks.Count;
				m_lUqiTasks.Add(strTask);
			}

			char[] vForTh = new char[v.Length]; // Will be cleared by thread
			Array.Copy(v, vForTh, v.Length);

			int nPoolWorkers, nPoolCompletions;
			ThreadPool.GetAvailableThreads(out nPoolWorkers, out nPoolCompletions);

			if((nTasks <= 3) && (nPoolWorkers >= 2))
				ThreadPool.QueueUserWorkItem(new WaitCallback(
					this.UpdateQualityInfoTh), vForTh);
			else
			{
				ParameterizedThreadStart pts = new ParameterizedThreadStart(
					this.UpdateQualityInfoTh);
				Thread th = new Thread(pts);
				th.Start(vForTh);
			}
		}

		private void UpdateQualityInfoTh(object oPassword)
		{
			char[] v = (oPassword as char[]);
			if(v == null) { Debug.Assert(false); return; }

			char[] vNew = null;
			try
			{
				uint uBits = QualityEstimation.EstimatePasswordBits(v);

				SecureTextBoxEx tb = m_tbPassword;
				if(tb == null) return; // Control disposed in the meanwhile
				Debug.Assert(tb.InvokeRequired);

				// Test whether the password has changed in the meanwhile
				vNew = (tb.Invoke(new UqiGetPasswordDelegate(
					this.UqiGetPassword)) as char[]);
				if(!MemUtil.ArrayHelperExOfChar.Equals(v, vNew)) return;

				tb.Invoke(new UqiShowQualityDelegate(this.UqiShowQuality),
					uBits, (uint)v.Length);
			}
			catch(Exception) { Debug.Assert(false); }
			finally
			{
				string strTask = GetUqiTaskID(v);
				lock(m_oUqiTasksSync) { m_lUqiTasks.Remove(strTask); }

				MemUtil.ZeroArray<char>(v);
				if(vNew != null) MemUtil.ZeroArray<char>(vNew);
			}
		}

		private delegate char[] UqiGetPasswordDelegate();
		private char[] UqiGetPassword()
		{
			try { return m_tbPassword.TextEx.ReadChars(); }
			catch(Exception) { Debug.Assert(false); }

			return null;
		}

		private delegate void UqiShowQualityDelegate(uint uBits, uint uLength);
		private void UqiShowQuality(uint uBits, uint uLength)
		{
			try
			{
				bool bUnknown = (m_bSprVar && !m_pbQuality.Enabled);

				m_pbQuality.ProgressText = (bUnknown ? string.Empty :
					KPRes.BitsEx.Replace(@"{PARAM}", uBits.ToString()));

				int iPct = (int)((100 * uBits) / 128);
				iPct = Math.Min(Math.Max(iPct, 0), 100);
				m_pbQuality.Value = iPct;

				string strLength = (bUnknown ? "?" : uLength.ToString());
				string strInfo = strLength + " " + KPRes.CharsAbbr;
				UIUtil.SetText(m_lblQualityInfo, strInfo);
				UIUtil.SetToolTip(m_ttHint, m_lblQualityInfo, KPRes.PasswordLength +
					": " + strLength + " " + KPRes.CharsStc, true);
			}
			catch(Exception) { Debug.Assert(false); }
		}

		private static Bitmap g_bmpLightDots = null;
		internal static void ConfigureHideButton(CheckBox cb, ToolTip tt)
		{
			if(cb == null) { Debug.Assert(false); return; }

			Debug.Assert(!cb.AutoSize);
			Debug.Assert(cb.Appearance == Appearance.Button);
			Debug.Assert(cb.Image == null);
			Debug.Assert(cb.Text == "***");
			Debug.Assert(cb.TextAlign == ContentAlignment.MiddleCenter);
			Debug.Assert(cb.TextImageRelation == TextImageRelation.Overlay);
			Debug.Assert(cb.UseVisualStyleBackColor);
			Debug.Assert((cb.Width == 32) || DpiUtil.ScalingRequired ||
				MonoWorkarounds.IsRequired(100001));
			Debug.Assert((cb.Height == 23) || DpiUtil.ScalingRequired ||
				MonoWorkarounds.IsRequired(100001));

			// Too much spacing between the dots when using the default font
			// cb.Text = SecureTextBoxEx.GetPasswordCharString(3);
			cb.Text = string.Empty;

			Image img = Properties.Resources.B19x07_3BlackDots;

			if(UIUtil.IsDarkTheme)
			{
				if(g_bmpLightDots == null)
					g_bmpLightDots = UIUtil.InvertImage(img);

				if(g_bmpLightDots != null) img = g_bmpLightDots;
			}
			else { Debug.Assert(g_bmpLightDots == null); } // Always or never

			cb.Image = img;
			Debug.Assert(cb.ImageAlign == ContentAlignment.MiddleCenter);

			if(tt != null)
				UIUtil.SetToolTip(tt, cb, KPRes.TogglePasswordAsterisks, false);
			AccessibilityEx.SetName(cb, KPRes.TogglePasswordAsterisks); // Even if tt is null
		}
	}
}
