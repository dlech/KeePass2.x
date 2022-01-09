/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2022 Dominik Reichl <dominik.reichl@t-online.de>

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
using KeePass.Native;
using KeePass.Resources;
using KeePass.UI;

using KeePassLib;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class FieldPickerForm : Form
	{
		private string m_strTitle = null;
		private string m_strText = null;
		private List<FpField> m_lFields = null;

		private FpField m_fpResult = null;
		public FpField SelectedField { get { return m_fpResult; } }

		internal static FpField ShowAndRestore(string strTitle, string strText,
			List<FpField> lFields)
		{
			IntPtr h = IntPtr.Zero;
			try { h = NativeMethods.GetForegroundWindowHandle(); }
			catch(Exception) { Debug.Assert(false); }

			FieldPickerForm dlg = new FieldPickerForm();
			dlg.InitEx(strTitle, strText, lFields);

			FpField fpResult = null;
			if(UIUtil.ShowDialogAndDestroy(dlg) == DialogResult.OK)
				fpResult = dlg.SelectedField;

			try
			{
				if(h != IntPtr.Zero)
					NativeMethods.EnsureForegroundWindow(h);
			}
			catch(Exception) { Debug.Assert(false); }

			return fpResult;
		}

		public FieldPickerForm()
		{
			InitializeComponent();
			GlobalWindowManager.InitializeForm(this);
		}

		public void InitEx(string strTitle, string strText, List<FpField> lFields)
		{
			m_strTitle = strTitle;
			m_strText = strText;
			m_lFields = lFields;
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			// Can be invoked during auto-type; don't use CenterParent
			Debug.Assert(this.StartPosition == FormStartPosition.CenterScreen);

			GlobalWindowManager.AddWindow(this);

			string strTitle = (m_strTitle ?? string.Empty);
			string strTitleEx = strTitle;
			if(strTitle.Length > 0) strTitleEx += " - ";
			strTitleEx += PwDefs.ShortProductName;

			this.Icon = AppIcons.Default;
			this.Text = strTitleEx;

			BannerFactory.CreateBannerEx(this, m_bannerImage,
				Properties.Resources.B48x48_Binary, strTitle,
				(m_strText ?? string.Empty));

			UIUtil.SetExplorerTheme(m_lvFields, true);

			int w = (m_lvFields.ClientSize.Width - UIUtil.GetVScrollBarWidth()) / 2;
			m_lvFields.Columns.Add(KPRes.Name, w);
			m_lvFields.Columns.Add(KPRes.Value, w);

			if(m_lFields != null)
			{
				ListViewGroup lvg = null;
				string strGroup = string.Empty;

				foreach(FpField fpf in m_lFields)
				{
					if(fpf == null) { Debug.Assert(false); continue; }

					ListViewItem lvi = new ListViewItem(fpf.Name);
					lvi.SubItems.Add(fpf.Value.IsProtected ? PwDefs.HiddenPassword :
						StrUtil.MultiToSingleLine(fpf.Value.ReadString()));
					lvi.Tag = fpf;

					m_lvFields.Items.Add(lvi);

					if(fpf.Group.Length > 0)
					{
						if(fpf.Group != strGroup)
						{
							strGroup = fpf.Group;
							lvg = new ListViewGroup(strGroup, HorizontalAlignment.Left);

							m_lvFields.Groups.Add(lvg);
						}

						lvg.Items.Add(lvi);
					}
				}
			}

			this.BringToFront();
			this.Activate();
			if(m_lvFields.Items.Count > 0)
				UIUtil.SetFocusedItem(m_lvFields, m_lvFields.Items[0], true);
			UIUtil.SetFocus(m_lvFields, this, true);
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			GlobalWindowManager.RemoveWindow(this);
		}

		private void ProcessItemSelection()
		{
			if(this.DialogResult == DialogResult.OK) return; // Already closing

			ListView.SelectedListViewItemCollection lvsic =
				m_lvFields.SelectedItems;
			if((lvsic == null) || (lvsic.Count != 1)) { Debug.Assert(false); return; }

			ListViewItem lvi = lvsic[0];
			if(lvi == null) { Debug.Assert(false); return; }

			FpField fpf = (lvi.Tag as FpField);
			if(fpf == null) { Debug.Assert(false); return; }

			m_fpResult = fpf;
			this.DialogResult = DialogResult.OK;
		}

		private void OnFieldItemActivate(object sender, EventArgs e)
		{
			ProcessItemSelection();
		}

		// The item activation handler has a slight delay when clicking an
		// item, thus as a performance optimization we additionally handle
		// item clicks
		private void OnFieldClick(object sender, EventArgs e)
		{
			ProcessItemSelection();
		}
	}
}
