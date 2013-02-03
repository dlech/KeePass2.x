/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2013 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePass.App;
using KeePass.App.Configuration;
using KeePass.Resources;
using KeePass.UI;

using KeePassLib;
using KeePassLib.Native;
using KeePassLib.Security;

namespace KeePass.Forms
{
	public partial class CharPickerForm : Form
	{
		private ProtectedString m_psWord = null;
		private ProtectedString m_psSelected = ProtectedString.Empty;

		private SecureEdit m_secWord = new SecureEdit();
		private List<Button> m_lButtons = new List<Button>();
		private List<Label> m_lLabels = new List<Label>();
		private bool m_bInFormLoad = false;
		private int m_nFormHeight = 0;
		private string m_strInitialFormRect = string.Empty;
		private bool m_bSetForeground = false;
		private uint m_uCharCount = 0;
		private bool? m_bInitHide = null;
		private int m_nBannerWidth = -1;

		private Font m_fontChars = null;
		private char m_chMaskChar = ((NativeLib.GetPlatformID() ==
			PlatformID.Win32Windows) ? '\u00D7' : '\u25CF');

		public ProtectedString SelectedCharacters
		{
			get { return m_psSelected; }
		}

		public CharPickerForm()
		{
			InitializeComponent();
			Program.Translation.ApplyTo(this);
		}

		/// <summary>
		/// Initialize the dialog.
		/// </summary>
		/// <param name="psWord">Password to pick characters from.</param>
		/// <param name="bCenterScreen">Specifies whether to center the form
		/// on the screen or not.</param>
		/// <param name="bSetForeground">If <c>true</c>, the window will be
		/// brought to the foreground when showing it.</param>
		/// <param name="uCharCount">Number of characters to pick. Specify
		/// 0 to allow picking a variable amount of characters.</param>
		public void InitEx(ProtectedString psWord, bool bCenterScreen,
			bool bSetForeground, uint uCharCount, bool? bInitHide)
		{
			m_psWord = psWord;

			if(bCenterScreen) this.StartPosition = FormStartPosition.CenterScreen;

			m_bSetForeground = bSetForeground;
			m_uCharCount = uCharCount;
			m_bInitHide = bInitHide;
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			Debug.Assert(m_psWord != null);
			if(m_psWord == null) throw new InvalidOperationException();

			m_bInFormLoad = true;

			GlobalWindowManager.AddWindow(this);

			m_nFormHeight = this.Height; // Before restoring the position/size

			string strRect = Program.Config.UI.CharPickerRect;
			if(strRect.Length > 0) UIUtil.SetWindowScreenRect(this, strRect);
			m_strInitialFormRect = UIUtil.GetWindowScreenRect(this);

			m_fontChars = FontUtil.CreateFont("Tahoma", 8.25f, FontStyle.Bold);

			this.Icon = Properties.Resources.KeePass;
			this.Text = KPRes.PickCharacters + " - " + PwDefs.ShortProductName;

			m_secWord.Attach(m_tbSelected, OnSelectedTextChangedEx, true);

			AceColumn colPw = Program.Config.MainWindow.FindColumn(AceColumnType.Password);
			bool bHide = ((colPw != null) ? colPw.HideWithAsterisks : true);
			if(m_bInitHide.HasValue) bHide = m_bInitHide.Value;
			bHide |= !AppPolicy.Current.UnhidePasswords;
			m_cbHideChars.Checked = bHide;

			RecreateResizableWindowControls();

			if(m_uCharCount > 0)
			{
				m_btnOK.Enabled = false;
				// m_btnOK.Visible = false;
			}

			if(m_bSetForeground)
			{
				this.BringToFront();
				this.Activate();
			}

			m_bInFormLoad = false;
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			CleanUpEx();
			GlobalWindowManager.RemoveWindow(this);
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			byte[] pbUtf8 = m_secWord.ToUtf8();
			m_psSelected = new ProtectedString(true, pbUtf8);
			Array.Clear(pbUtf8, 0, pbUtf8.Length);
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
		}

		private void CleanUpEx()
		{
			string strRect = UIUtil.GetWindowScreenRect(this);
			if(strRect != m_strInitialFormRect)
				Program.Config.UI.CharPickerRect = strRect;

			m_secWord.Detach();

			RemoveAllCharButtons();
			m_fontChars.Dispose();
		}

		private void RemoveAllCharButtons()
		{
			if((m_lButtons != null) && (m_pnlSelect != null))
			{
				foreach(Button btn in m_lButtons)
				{
					btn.Click -= this.OnSelectCharacter;
					m_pnlSelect.Controls.Remove(btn);
					btn.Dispose();
				}

				m_lButtons.Clear();
			}

			if((m_lLabels != null) && (m_pnlSelect != null))
			{
				foreach(Label lbl in m_lLabels)
				{
					m_pnlSelect.Controls.Remove(lbl);
					lbl.Dispose();
				}

				m_lLabels.Clear();
			}
		}

		private void RecreateResizableWindowControls()
		{
			string strTitle = KPRes.PickCharacters;
			if(m_uCharCount > 0) strTitle += " (" + m_uCharCount.ToString() + ")";

			BannerFactory.UpdateBanner(this, m_bannerImage,
				Properties.Resources.B48x48_KGPG_Key2, strTitle,
				KPRes.PickCharactersDesc, ref m_nBannerWidth);

			RemoveAllCharButtons();

			string strWord = ((m_psWord != null) ? m_psWord.ReadString() : string.Empty);
			if(strWord.Length >= 1)
			{
				int x = 0;
				int nPnlWidth = m_pnlSelect.Width, nPnlHeight = m_pnlSelect.Height;
				for(int i = 0; i < strWord.Length; ++i)
				{
					int w = ((nPnlWidth * (i + 1)) / strWord.Length) - x;

					Button btn = new Button();
					btn.Location = new Point(x, 0);
					btn.Size = new Size(w, nPnlHeight / 2 - 1);
					btn.Font = m_fontChars;
					btn.Tag = strWord[i];
					btn.Click += this.OnSelectCharacter;

					m_lButtons.Add(btn);
					m_pnlSelect.Controls.Add(btn);

					Label lbl = new Label();
					lbl.Text = (i + 1).ToString();
					lbl.TextAlign = ContentAlignment.MiddleCenter;
					lbl.Location = new Point(x, nPnlHeight / 2);
					lbl.Size = new Size(w + 1, nPnlHeight / 2 - 3);

					m_lLabels.Add(lbl);
					m_pnlSelect.Controls.Add(lbl);

					x += w;
				}
			}

			OnHideCharsCheckedChanged(null, EventArgs.Empty);
		}

		private void OnHideCharsCheckedChanged(object sender, EventArgs e)
		{
			bool bHide = m_cbHideChars.Checked;
			if(!bHide && !AppPolicy.Try(AppPolicyId.UnhidePasswords))
			{
				m_cbHideChars.Checked = true;
				return;
			}

			m_secWord.EnableProtection(bHide);

			bool bHideBtns = bHide;
			bHideBtns |= !Program.Config.UI.Hiding.UnhideButtonAlsoUnhidesSource;
			foreach(Button btn in m_lButtons)
			{
				if(bHideBtns) btn.Text = string.Empty + m_chMaskChar;
				else btn.Text = string.Empty + (char)btn.Tag;
			}
		}

		private void OnSelectCharacter(object sender, EventArgs e)
		{
			Button btn = (sender as Button);
			if(btn == null) { Debug.Assert(false); return; }

			char ch = (char)btn.Tag;

			m_tbSelected.Text += ch;
		}

		private void OnFormResize(object sender, EventArgs e)
		{
			ProcessResize();
		}

		private void OnFormSizeChanged(object sender, EventArgs e)
		{
			ProcessResize();
		}

		private void ProcessResize()
		{
			if((this.Height != m_nFormHeight) && (m_nFormHeight != 0))
				this.Height = m_nFormHeight;

			if(!m_bInFormLoad) RecreateResizableWindowControls();
		}

		private void OnSelectedTextChangedEx(object sender, EventArgs e)
		{
			if((m_uCharCount > 0) && (m_secWord.TextLength == m_uCharCount))
			{
				// m_btnOK.Visible = true;
				m_btnOK.Enabled = true;
				m_btnOK.PerformClick();
			}
		}
	}
}
