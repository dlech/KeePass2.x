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
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

using KeePass.Forms;
using KeePass.Resources;

using KeePassLib;
using KeePassLib.Security;

namespace KeePass.Util
{
	internal sealed class EntryTemplateItem
	{
		private bool m_bProtected = false;
		private string m_strItemName = string.Empty;

		public bool Protected
		{
			get { return m_bProtected; }
		}

		public string Name
		{
			get { return m_strItemName; }
		}

		public EntryTemplateItem(string strItemName, bool bProtected)
		{
			if(strItemName == null) throw new ArgumentNullException("strItemName");
			m_strItemName = strItemName;
			m_bProtected = bProtected;
		}
	}

	internal sealed class EntryTemplate
	{
		private string m_strName = string.Empty;
		private Image m_imgSmallIcon = null;
		private EntryTemplateItem[] m_pItems = null;

		public string Name
		{
			get { return m_strName; }
		}

		public Image SmallIcon
		{
			get { return m_imgSmallIcon; }
		}

		public EntryTemplateItem[] Items
		{
			get { return m_pItems; }
		}

		public EntryTemplate(string strName, Image imgSmallIcon, EntryTemplateItem[] pEntries)
		{
			if(strName == null) throw new ArgumentNullException("strName");
			if(pEntries == null) throw new ArgumentNullException("pEntries");

			m_strName = strName;
			m_imgSmallIcon = imgSmallIcon;
			m_pItems = pEntries;
		}
	}

	public static class EntryTemplates
	{
		private static List<EntryTemplate> m_vTemplates = new List<EntryTemplate>();

		private static ToolStripSplitButton m_btnItemsHost = null;
		private static List<ToolStripItem> m_vToolStripItems = new List<ToolStripItem>();

		public static void Init(ToolStripSplitButton btnHost)
		{
			if(btnHost == null) throw new ArgumentNullException("btnHost");
			m_btnItemsHost = btnHost;

			ToolStripSeparator tsSep = new ToolStripSeparator();
			m_btnItemsHost.DropDownItems.Add(tsSep);
			m_vToolStripItems.Add(tsSep);

			EntryTemplates.AddItem(BankAccount);
			EntryTemplates.AddItem(PersonalContact);
		}

		private static void AddItem(EntryTemplate et)
		{
			m_vTemplates.Add(et);

			ToolStripMenuItem tsmi = new ToolStripMenuItem(et.Name);
			tsmi.Click += OnEntryTemplatesExecute;
			m_btnItemsHost.DropDownItems.Add(tsmi);

			if(et.SmallIcon != null) tsmi.Image = et.SmallIcon;
			else tsmi.Image = KeePass.Properties.Resources.B16x16_KGPG_Key1;

			m_vToolStripItems.Add(tsmi);
		}

		public static void Clear()
		{
			m_vTemplates.Clear();

			foreach(ToolStripItem tsmi in m_vToolStripItems)
			{
				tsmi.Click -= OnEntryTemplatesExecute;
				m_btnItemsHost.DropDownItems.Remove(tsmi);
			}

			m_vToolStripItems.Clear();
		}

		private static void OnEntryTemplatesExecute(object sender, EventArgs e)
		{
			ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
			if(tsmi == null) { Debug.Assert(false); return; }

			string strName = tsmi.Text;
			foreach(EntryTemplate et in m_vTemplates)
			{
				if(et.Name == strName)
				{
					CreateEntry(et);
					break;
				}
			}
		}

		private static void CreateEntry(EntryTemplate et)
		{
			if(Program.MainForm.Database.IsOpen == false) { Debug.Assert(false); return; }

			PwGroup pgContainer = Program.MainForm.GetSelectedGroup();
			if(pgContainer == null) pgContainer = Program.MainForm.Database.RootGroup;

			PwEntry pe = new PwEntry(pgContainer, true, true);

			// pe.Strings.Set(PwDefs.TitleField, new ProtectedString(
			//	Program.MainForm.Database.MemoryProtection.ProtectTitle,
			//	et.Name));

			foreach(EntryTemplateItem eti in et.Items)
				pe.Strings.Set(eti.Name, new ProtectedString(eti.Protected, string.Empty));

			PwEntryForm pef = new PwEntryForm();
			pef.InitEx(pe, PwEntryForm.PwEditMode.AddNewEntry, Program.MainForm.Database,
				Program.MainForm.ClientIcons);

			pef.ShowAdvancedByDefault = true;
			if(pef.ShowDialog() == DialogResult.OK)
			{
				pgContainer.Entries.Add(pe);

				Program.MainForm.UpdateEntryList(null, true);
				Program.MainForm.UpdateUIState(true);
			}
			else Program.MainForm.UpdateUIState(false);
		}

		private static readonly EntryTemplate BankAccount = new EntryTemplate(
			KPRes.BankAccount, null, new EntryTemplateItem[]{
				new EntryTemplateItem(KPRes.TAccountNumber, false),
				new EntryTemplateItem(KPRes.TAccountType, false),
				new EntryTemplateItem(KPRes.BranchCode, false),
				new EntryTemplateItem(KPRes.RoutingCode, false),
				new EntryTemplateItem(KPRes.SortCode, false),
				new EntryTemplateItem(KPRes.IBAN, false),
				new EntryTemplateItem(KPRes.SWIFTCode, false),
				new EntryTemplateItem(KPRes.BranchTel, false),
				new EntryTemplateItem(KPRes.TAccountInfoTel, false),
				new EntryTemplateItem(KPRes.BranchHours, false),
				new EntryTemplateItem(KPRes.TAccountNames, false),
				new EntryTemplateItem(KPRes.MinBalance, false),
				new EntryTemplateItem(KPRes.TransfersOf, false),
				new EntryTemplateItem(KPRes.Frequency, false)
			});

		private static readonly EntryTemplate PersonalContact = new EntryTemplate(
			KPRes.Contact, null, new EntryTemplateItem[]{
				new EntryTemplateItem(KPRes.JobTitle, false),
				new EntryTemplateItem(KPRes.Department, false),
				new EntryTemplateItem(KPRes.Company, false),
				new EntryTemplateItem(KPRes.WorkTel, false),
				new EntryTemplateItem(KPRes.EMail, false),
				new EntryTemplateItem(KPRes.HomeTel, false),
				new EntryTemplateItem(KPRes.MobileTel, false),
				new EntryTemplateItem(KPRes.Pager, false),
				new EntryTemplateItem(KPRes.CarTel, false),
				new EntryTemplateItem(KPRes.WorkFax, false),
				new EntryTemplateItem(KPRes.HomeFax, false),
				new EntryTemplateItem(KPRes.WebPage, false),
				new EntryTemplateItem(KPRes.Assistant, false),
				new EntryTemplateItem(KPRes.AssistantTel, false),
				new EntryTemplateItem(KPRes.HomeAddress, false)
			});
	}
}
