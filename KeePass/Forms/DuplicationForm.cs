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

using KeePass.App;
using KeePass.UI;

namespace KeePass.Forms
{
	public partial class DuplicationForm : Form
	{
		private bool m_bAppendCopy = true;
		public bool AppendCopyToTitles
		{
			get { return m_bAppendCopy; }
		}

		private bool m_bFieldRefs = false;
		public bool ReplaceDataByFieldRefs
		{
			get { return m_bFieldRefs; }
		}

		public DuplicationForm()
		{
			InitializeComponent();
			Program.Translation.ApplyTo(this);
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			GlobalWindowManager.AddWindow(this);

			this.Icon = Properties.Resources.KeePass;

			FontUtil.AssignDefaultBold(m_cbAppendCopy);
			FontUtil.AssignDefaultBold(m_cbFieldRefs);

			m_cbAppendCopy.Checked = m_bAppendCopy;
			m_cbFieldRefs.Checked = m_bFieldRefs;
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			GlobalWindowManager.RemoveWindow(this);
		}

		private void OnFieldRefsLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			AppHelp.ShowHelp(AppDefs.HelpTopics.FieldRefs, null);
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			m_bAppendCopy = m_cbAppendCopy.Checked;
			m_bFieldRefs = m_cbFieldRefs.Checked;
		}
	}
}
