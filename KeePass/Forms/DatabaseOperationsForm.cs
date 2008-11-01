/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2008 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePass.UI;
using KeePass.Resources;

using KeePassLib;
using KeePassLib.Delegates;

namespace KeePass.Forms
{
	public partial class DatabaseOperationsForm : Form, IGwmWindow
	{
		private PwDatabase m_pwDatabase = null;

		public bool CanCloseWithoutDataLoss { get { return true; } }

		public void InitEx(PwDatabase pwDatabase)
		{
			m_pwDatabase = pwDatabase;
		}

		public DatabaseOperationsForm()
		{
			InitializeComponent();
			Program.Translation.ApplyTo(this);
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			Debug.Assert(m_pwDatabase != null); if(m_pwDatabase == null) throw new InvalidOperationException();

			GlobalWindowManager.AddWindow(this, this);

			m_bannerImage.Image = BannerFactory.CreateBanner(m_bannerImage.Width,
				m_bannerImage.Height, BannerStyle.Default,
				Properties.Resources.B48x48_Package_Settings, KPRes.DatabaseMaintenance,
				KPRes.DatabaseMaintenanceDesc);
			this.Icon = Properties.Resources.KeePass;
			this.Text = KPRes.DatabaseMaintenance;

			m_numHistoryDays.Value = m_pwDatabase.MaintenanceHistoryDays;

			m_pbStatus.Visible = false;
		}

		private void OnBtnClose(object sender, EventArgs e)
		{
			m_pwDatabase.MaintenanceHistoryDays = (uint)m_numHistoryDays.Value;
		}

		private void OnBtnDelete(object sender, EventArgs e)
		{
			m_btnClose.Enabled = m_btnHistoryEntriesDelete.Enabled = false;
			if(!m_pbStatus.Visible) m_pbStatus.Visible = true;

			DateTime dtNow = DateTime.Now;
			TimeSpan tsSpan = new TimeSpan((int)m_numHistoryDays.Value, 0, 0, 0);

			uint uNumGroups, uNumEntries;
			m_pwDatabase.RootGroup.GetCounts(true, out uNumGroups, out uNumEntries);

			m_pbStatus.Value = 0;

			uint uCurEntryNumber = 1;
			EntryHandler eh = delegate(PwEntry pe)
			{
				for(uint u = 0; u < pe.History.UCount; ++u)
				{
					PwEntry peHist = pe.History.GetAt(u);

					if((dtNow - peHist.LastAccessTime) >= tsSpan)
					{
						pe.History.Remove(peHist);
						--u;
					}
				}

				m_pbStatus.Value = (int)((uCurEntryNumber * 100) / uNumEntries);
				++uCurEntryNumber;
				return true;
			};

			m_pwDatabase.RootGroup.TraverseTree(TraversalMethod.PreOrder, null, eh);

			m_btnClose.Enabled = m_btnHistoryEntriesDelete.Enabled = true;

			// Database is set modified by parent
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			GlobalWindowManager.RemoveWindow(this);
		}
	}
}
