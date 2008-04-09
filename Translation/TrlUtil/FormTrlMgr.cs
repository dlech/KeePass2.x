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
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;

using KeePass.Forms;
using KeePassLib.Translation;

namespace TrlUtil
{
	public static class FormTrlMgr
	{
		public static List<KPFormCustomization> CreateListOfCurrentVersion()
		{
			List<KPFormCustomization> l = new List<KPFormCustomization>();

			AddForm(l, new AboutForm());
			AddForm(l, new CharPickerForm());
			AddForm(l, new DatabaseOperationsForm());
			AddForm(l, new DatabaseSettingsForm());
			AddForm(l, new DataViewerForm());
			AddForm(l, new EditAutoTypeItemForm());
			AddForm(l, new EditStringForm());
			AddForm(l, new EntropyForm());
			AddForm(l, new EntryListForm());
			AddForm(l, new GroupForm());
			AddForm(l, new HelpSourceForm());
			AddForm(l, new IconPickerForm());
			AddForm(l, new ImportCsvForm());
			AddForm(l, new ImportDataForm());
			AddForm(l, new ImportMethodForm());
			AddForm(l, new InternalBrowserForm());
			AddForm(l, new IOConnectionForm());
			AddForm(l, new KeyCreationForm());
			AddForm(l, new KeyPromptForm());
			AddForm(l, new LanguageForm());
			AddForm(l, new KeePass.Forms.MainForm());
			AddForm(l, new OptionsForm());
			AddForm(l, new PluginsForm());
			AddForm(l, new PrintForm());
			AddForm(l, new PwEntryForm());
			AddForm(l, new PwGeneratorForm());
			AddForm(l, new SearchForm());
			AddForm(l, new SingleLineEditForm());
			AddForm(l, new StatusLoggerForm());
			AddForm(l, new TanWizardForm());

			return l;
		}

		private static void AddForm(List<KPFormCustomization> listForms, Form f)
		{
			KPFormCustomization kpfc = new KPFormCustomization();

			kpfc.FullName = f.GetType().FullName;
			kpfc.TextEnglish = f.Text;

			foreach(Control c in f.Controls) AddControl(kpfc, c);

			listForms.Add(kpfc);
		}

		private static void AddControl(KPFormCustomization kpfc, Control c)
		{
			if((kpfc == null) || (c == null)) { Debug.Assert(false); return; }

			bool bAdd = true;
			Type t = c.GetType();

			if(c.Text.Length == 0) bAdd = false;
			else if(c.Name.Length == 0) bAdd = false;
			else if(t == typeof(MenuStrip)) bAdd = false;
			else if(t == typeof(TabControl)) bAdd = false;
			else if(t == typeof(PictureBox)) bAdd = false;
			else if(t == typeof(ListView)) bAdd = false;
			else if(t == typeof(TreeView)) bAdd = false;
			else if(t == typeof(ToolStrip)) bAdd = false;
			else if(t == typeof(WebBrowser)) bAdd = false;
			else if(t == typeof(Panel)) bAdd = false;
			else if(t == typeof(StatusStrip)) bAdd = false;
			else if(t == typeof(ProgressBar)) bAdd = false;
			else if(t == typeof(NumericUpDown)) bAdd = false;
			else if(c.Text == @"<>") bAdd = false;

			if(bAdd)
			{
				KPControlCustomization kpcc = new KPControlCustomization();
				kpcc.Name = c.Name;
				kpcc.TextEnglish = c.Text;
				kpfc.ControlsList.Add(kpcc);
			}

			foreach(Control cSub in c.Controls) AddControl(kpfc, cSub);
		}

		public static void RenderToTreeControl(List<KPFormCustomization> listCustoms,
			TreeView tv)
		{
			tv.BeginUpdate();
			tv.Nodes.Clear();

			foreach(KPFormCustomization kpfc in listCustoms)
			{
				string strName = kpfc.FullName;
				int nLastDot = strName.LastIndexOf('.');
				if(nLastDot >= 0) strName = strName.Substring(nLastDot + 1);

				TreeNode tnForm = tv.Nodes.Add(strName);
				tnForm.Tag = kpfc;

				foreach(KPControlCustomization kpcc in kpfc.ControlsList)
				{
					TreeNode tnControl = tnForm.Nodes.Add(kpcc.Name);
					tnControl.Tag = kpcc;
				}

				tnForm.ExpandAll();
			}

			tv.EndUpdate();
		}

		public static void MergeForms(List<KPFormCustomization> lInto,
			List<KPFormCustomization> lFrom)
		{
			foreach(KPFormCustomization kpInto in lInto)
			{
				foreach(KPFormCustomization kpFrom in lFrom)
				{
					if(kpInto.FullName == kpFrom.FullName)
						MergeFormCustomizations(kpInto, kpFrom);
				}
			}
		}

		private static void MergeFormCustomizations(KPFormCustomization kpInto,
			KPFormCustomization kpFrom)
		{
			foreach(KPControlCustomization ccInto in kpInto.ControlsList)
			{
				foreach(KPControlCustomization ccFrom in kpFrom.ControlsList)
				{
					if(ccInto.Name == ccFrom.Name)
						MergeControlCustomizations(ccInto, ccFrom);
				}
			}
		}

		private static void MergeControlCustomizations(KPControlCustomization ccInto,
			KPControlCustomization ccFrom)
		{
			if(ccFrom.Text.Length > 0) ccInto.Text = ccFrom.Text;

			if(ccFrom.Layout.X.Length > 0) ccInto.Layout.X = ccFrom.Layout.X;
			if(ccFrom.Layout.Y.Length > 0) ccInto.Layout.Y = ccFrom.Layout.Y;
			if(ccFrom.Layout.Width.Length > 0) ccInto.Layout.Width = ccFrom.Layout.Width;
			if(ccFrom.Layout.Height.Length > 0) ccInto.Layout.Height = ccFrom.Layout.Height;
		}
	}
}
