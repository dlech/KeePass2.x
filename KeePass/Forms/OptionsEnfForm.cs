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
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

using KeePass.App;
using KeePass.App.Configuration;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;

using KeePassLib.Delegates;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class OptionsEnfForm : Form
	{
		private readonly string m_strConfigEnfUrl = AppHelp.GetOnlineUrl(
			AppDefs.HelpTopics.KbConfigEnf, null);

		public OptionsEnfForm()
		{
			InitializeComponent();
			GlobalWindowManager.InitializeForm(this);

			if(Program.DesignMode || !Program.EnableTranslation) return;

			m_lblInfo.Text = KPRes.ConfigEnfAdminUac + MessageService.NewParagraph +
				KPRes.ConfigEnfAllUsers + MessageService.NewParagraph +
				KPRes.ConfigEnfChangeValue + MessageService.NewParagraph +
				KPRes.ConfigEnfDialogSubset;

			int h = m_lblInfo.GetPreferredSize(new Size(m_lblInfo.Width,
				int.MaxValue)).Height;
			int d = h - m_lblInfo.Height;

			this.Height += d;
			m_lblInfo.Height = h;
			m_lnkConfigEnf.Top += d;
			m_btnOK.Top += d;
			m_btnCancel.Top += d;
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			GlobalWindowManager.AddWindow(this);

			string strTitle = KPRes.EnforceOptions + " (" + KPRes.AllUsers + ")";
			using(Bitmap bmp = UIUtil.AddShieldOverlay(Properties.Resources.B48x48_KCMSystem))
			{
				BannerFactory.CreateBannerEx(this, m_bannerImage, bmp,
					strTitle, KPRes.ConfigEnfEdit);
			}
			this.Icon = AppIcons.Default;
			this.Text = strTitle;

			UIUtil.SetText(m_lblEnforce, KPRes.Enforce + ":");

			UIUtil.SetExplorerTheme(m_lvMain, false);
			CreateOptionsList();

			UIUtil.SetText(m_lnkConfigEnf, m_strConfigEnfUrl);

			Debug.Assert(m_btnOK.FlatStyle == m_btnCancel.FlatStyle);
			UIUtil.SetShield(m_btnOK, true);
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			GlobalWindowManager.RemoveWindow(this);
		}

		private ListViewGroup AddGroup(string strName)
		{
			ListViewGroup lvg = new ListViewGroup(strName);
			m_lvMain.Groups.Add(lvg);
			return lvg;
		}

		private void AddOption(object oContainer, string strPropertyName,
			ListViewGroup lvg, string strDisplayName)
		{
			try
			{
				PropertyInfo pi = oContainer.GetType().GetProperty(strPropertyName);
				if((pi == null) || !pi.CanRead) { Debug.Assert(false); return; }

				Type tV = pi.PropertyType;
				object oV = pi.GetValue(oContainer, null);
				Debug.Assert((oV == null) || (oV.GetType() == tV));

				// Cf. AppEnforcedConfig.Modify
				string strV = null;
				if(tV == typeof(bool))
					strV = ((bool)oV ? KPRes.On : KPRes.Off);
				else if(tV == typeof(int))
					strV = ((int)oV).ToString();
				else if(tV == typeof(uint))
					strV = ((uint)oV).ToString();
				else { Debug.Assert(false); }

				ListViewItem lvi = new ListViewItem(strDisplayName, lvg);
				lvi.SubItems.Add(strV ?? string.Empty);
				lvi.Checked = AppConfigEx.IsOptionEnforced(oContainer, pi);
				lvi.Tag = new AecItem(oContainer, strPropertyName, false);

				m_lvMain.Items.Add(lvi);
			}
			catch(Exception) { Debug.Assert(false); }
		}

		private void CreateOptionsList()
		{
			AppConfigEx cfg = Program.Config;
			AceDefaults aceDef = cfg.Defaults;
			AceSecurity aceSec = cfg.Security;
			AceWorkspaceLocking aceWL = aceSec.WorkspaceLocking;

			int w = m_lvMain.ClientSize.Width - UIUtil.GetVScrollBarWidth();
			ListViewGroup lvg;
			GFunc<string, string> fRA = StrUtil.RemoveAccelerator;

			m_lvMain.BeginUpdateEx();

			m_lvMain.Columns.Add(KPRes.Name, (w * 3) / 4);
			m_lvMain.Columns.Add(KPRes.Value, w / 4);

			lvg = AddGroup(KPRes.General);
			AddOption(aceWL, "LockAfterTime", lvg, fRA(KPRes.LockAfterTime));
			AddOption(aceWL, "LockAfterGlobalTime", lvg, fRA(KPRes.LockAfterGlobalTime));
			AddOption(aceWL, "LockOnWindowMinimize", lvg, KPRes.LockOnMinimizeTaskbar);
			AddOption(aceWL, "LockOnWindowMinimizeToTray", lvg, KPRes.LockOnMinimizeTray);
			AddOption(aceWL, "LockOnSessionSwitch", lvg, KPRes.LockOnSessionSwitch);
			AddOption(aceWL, "LockOnSuspend", lvg, KPRes.LockOnSuspend);
			AddOption(aceWL, "LockOnRemoteControlChange", lvg, KPRes.LockOnRemoteControlChange);
			AddOption(aceWL, "ExitInsteadOfLockingAfterTime", lvg, KPRes.ExitInsteadOfLockingAfterTime);
			AddOption(aceWL, "AlwaysExitInsteadOfLocking", lvg, KPRes.ExitInsteadOfLockingAlways);

			lvg = AddGroup(KPRes.ClipboardMain);
			AddOption(aceSec, "ClipboardClearAfterSeconds", lvg, fRA(KPRes.ClipboardClearTime));
			AddOption(aceSec, "ClipboardClearOnExit", lvg, KPRes.ClipboardClearOnExit);
			AddOption(aceSec, "ClipboardNoPersist", lvg, KPRes.ClipboardNoPersist);
			AddOption(aceSec, "UseClipboardViewerIgnoreFormat", lvg, KPRes.ClipboardViewerIgnoreFormat);

			lvg = AddGroup(KPRes.Advanced);
			AddOption(aceDef, "NewEntryExpiresInDays", lvg, fRA(KPRes.ExpiryDefaultDays));
			AddOption(cfg.Native, "NativeKeyTransformations", lvg, KPRes.NativeLibUse);
			AddOption(aceSec, "KeyTransformWeakWarning", lvg, KPRes.KeyTransformWeakWarning);
			AddOption(aceSec, "PreventScreenCapture", lvg, KPRes.PreventScreenCapture);
			AddOption(aceSec, "MasterKeyOnSecureDesktop", lvg, KPRes.MasterKeyOnSecureDesktop);
			AddOption(aceSec, "ClearKeyCommandLineParams", lvg, KPRes.ClearKeyCmdLineParams);
			AddOption(aceSec.MasterPassword, "RememberWhileOpen", lvg, KPRes.MasterPasswordRmbWhileOpen);

			lvg = AddGroup(KPRes.Policy);
			GAction<string, AppPolicyId> fAddPolicy = delegate(string strPropertyName,
				AppPolicyId p)
			{
				AddOption(aceSec.Policy, strPropertyName, lvg, AppPolicy.GetName(p));
			};
			fAddPolicy("Plugins", AppPolicyId.Plugins);
			fAddPolicy("Export", AppPolicyId.Export);
			// fAddPolicy("ExportNoKey", AppPolicyId.ExportNoKey);
			fAddPolicy("Import", AppPolicyId.Import);
			fAddPolicy("Print", AppPolicyId.Print);
			fAddPolicy("PrintNoKey", AppPolicyId.PrintNoKey);
			fAddPolicy("NewFile", AppPolicyId.NewFile);
			fAddPolicy("SaveFile", AppPolicyId.SaveFile);
			fAddPolicy("AutoType", AppPolicyId.AutoType);
			fAddPolicy("AutoTypeWithoutContext", AppPolicyId.AutoTypeWithoutContext);
			fAddPolicy("CopyToClipboard", AppPolicyId.CopyToClipboard);
			fAddPolicy("CopyWholeEntries", AppPolicyId.CopyWholeEntries);
			fAddPolicy("DragDrop", AppPolicyId.DragDrop);
			fAddPolicy("UnhidePasswords", AppPolicyId.UnhidePasswords);
			fAddPolicy("ChangeMasterKey", AppPolicyId.ChangeMasterKey);
			fAddPolicy("ChangeMasterKeyNoKey", AppPolicyId.ChangeMasterKeyNoKey);
			fAddPolicy("EditTriggers", AppPolicyId.EditTriggers);

			lvg = AddGroup(KPRes.IOConnectionLong);
			AddOption(aceSec, "SslCertsAcceptInvalid", lvg, KPRes.TlsCertsAcceptInvalid);

			m_lvMain.EndUpdateEx();
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			m_btnOK.Enabled = false;

			List<AecItem> l = new List<AecItem>();
			foreach(ListViewItem lvi in m_lvMain.Items)
			{
				AecItem it = (lvi.Tag as AecItem);
				if(it == null) { Debug.Assert(false); continue; }
				Debug.Assert(!it.Enforce);

				l.Add(new AecItem(it.Container, it.PropertyName, lvi.Checked));
			}

			if(!AppEnforcedConfig.Modify(l, Program.Config, true))
				this.DialogResult = DialogResult.None;

			m_btnOK.Enabled = true;
		}

		private void OnConfigEnfLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			WinUtil.OpenUrl(m_strConfigEnfUrl, null);
		}

		private void SetAllChecked(bool bCheck)
		{
			m_lvMain.BeginUpdate();
			foreach(ListViewItem lvi in m_lvMain.Items) lvi.Checked = bCheck;
			m_lvMain.EndUpdate();
		}

		private void OnLinkEnfAll(object sender, LinkLabelLinkClickedEventArgs e)
		{
			SetAllChecked(true);
		}

		private void OnLinkEnfNone(object sender, LinkLabelLinkClickedEventArgs e)
		{
			SetAllChecked(false);
		}
	}
}
