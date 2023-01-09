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
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

using KeePass.Forms;
using KeePass.UI;

using KeePassLib.Translation;

namespace TrlUtil
{
	public static class FormTrlMgr
	{
		private static bool m_bIgnoreBaseHash = false;
		public static bool IgnoreBaseHash
		{
			get { return m_bIgnoreBaseHash; }
			set { m_bIgnoreBaseHash = value; }
		}

		public static List<KPFormCustomization> CreateListOfCurrentVersion()
		{
			List<KPFormCustomization> l = new List<KPFormCustomization>();

			AddForm(l, new AboutForm());
			AddForm(l, new AutoTypeCtxForm());
			AddForm(l, new CharPickerForm());
			AddForm(l, new ColumnsForm());
			AddForm(l, new CsvImportForm());
			AddForm(l, new DatabaseOperationsForm());
			AddForm(l, new DatabaseSettingsForm());
			AddForm(l, new DataEditorForm());
			AddForm(l, new DataViewerForm());
			AddForm(l, new DuplicationForm());
			AddForm(l, new EcasActionForm());
			AddForm(l, new EcasConditionForm());
			AddForm(l, new EcasEventForm());
			AddForm(l, new EcasTriggerForm());
			AddForm(l, new EcasTriggersForm());
			AddForm(l, new EditAutoTypeItemForm());
			AddForm(l, new EditStringForm());
			AddForm(l, new EntropyForm());
			AddForm(l, new ExchangeDataForm());
			AddForm(l, new FieldPickerForm());
			AddForm(l, new FieldRefForm());
			AddForm(l, new FileBrowserForm());
			AddForm(l, new GroupForm());
			AddForm(l, new HelpSourceForm());
			AddForm(l, new IconPickerForm());
			AddForm(l, new ImportMethodForm());
			AddForm(l, new InternalBrowserForm());
			AddForm(l, new IOConnectionForm());
			AddForm(l, new KeyCreationForm());
			AddForm(l, new KeyFileCreationForm());
			AddForm(l, new KeyPromptForm());
			AddForm(l, new LanguageForm());
			AddForm(l, new ListViewForm());
			AddForm(l, new KeePass.Forms.MainForm());
			AddForm(l, new OptionsForm());
			AddForm(l, new OtpGeneratorForm());
			AddForm(l, new PluginsForm());
			AddForm(l, new PrintForm());
			AddForm(l, new ProxyForm());
			AddForm(l, new PwEntryForm());
			AddForm(l, new PwGeneratorForm());
			AddForm(l, new SearchForm());
			AddForm(l, new SingleLineEditForm());
			AddForm(l, new StatusLoggerForm());
			AddForm(l, new StatusProgressForm());
			AddForm(l, new TanWizardForm());
			AddForm(l, new TextEncodingForm());
			AddForm(l, new UpdateCheckForm());
			AddForm(l, new UrlOverrideForm());
			AddForm(l, new UrlOverridesForm());
			AddForm(l, new WebDocForm());
			AddForm(l, new XmlReplaceForm());

			return l;
		}

		private static void AddForm(List<KPFormCustomization> listForms, Form f)
		{
			KPFormCustomization kpfc = new KPFormCustomization();

			kpfc.FullName = f.GetType().FullName;
			kpfc.FormEnglish = f;

			kpfc.Window.TextEnglish = f.Text;
			kpfc.Window.BaseHash = KPControlCustomization.HashControl(f);

			foreach(Control c in f.Controls) AddControl(kpfc, c);

			kpfc.Controls.Sort();

			listForms.Add(kpfc);
		}

		private static void AddControl(KPFormCustomization kpfc, Control c)
		{
			if((kpfc == null) || (c == null)) { Debug.Assert(false); return; }

			bool bAdd = true;

			if(string.IsNullOrEmpty(c.Text) ||
				(c.Text.StartsWith("<") && c.Text.EndsWith(">")) ||
				((c is Panel) && !(c is TabPage)) || (c is PictureBox) ||
				(c is ToolStrip) || (c is TreeView) || (c is WebBrowser))
				bAdd = false;

			// For layout adjustments
			if((c is ButtonBase) || (c is DateTimePicker) || (c is Label) ||
				(c is ListControl) || (c is ListView) || (c is ProgressBar) ||
				(c is QualityProgressBar) || (c is TabControl) || (c is TextBoxBase))
				bAdd |= (c.Dock != DockStyle.Fill);

			if(string.IsNullOrEmpty(c.Name)) bAdd = false;

			if(bAdd)
			{
				KPControlCustomization kpcc = new KPControlCustomization();
				kpcc.Name = c.Name;
				kpcc.BaseHash = KPControlCustomization.HashControl(c);

				Debug.Assert(!(c is TabControl) || string.IsNullOrEmpty(c.Text));
				if((c is HotKeyControlEx) || (c is NumericUpDown))
					kpcc.TextEnglish = string.Empty;
				else kpcc.TextEnglish = (c.Text ?? string.Empty);

				kpfc.Controls.Add(kpcc);
			}

			foreach(Control cSub in c.Controls) AddControl(kpfc, cSub);
		}

		public static void RenderToTreeControl(List<KPFormCustomization> listCustoms,
			TreeView tv, Dictionary<string, TreeNode> dControls)
		{
			tv.BeginUpdate();
			tv.Nodes.Clear();
			dControls.Clear();

			foreach(KPFormCustomization kpfc in listCustoms)
			{
				string strName = kpfc.FullName;
				int nLastDot = strName.LastIndexOf('.');
				if(nLastDot >= 0) strName = strName.Substring(nLastDot + 1);

				TreeNode tnForm = tv.Nodes.Add(strName);
				tnForm.Tag = kpfc;
				// Bold font results in clipping bug in release mode

				TreeNode tnWindow = tnForm.Nodes.Add("Window");
				tnWindow.Tag = kpfc.Window;

				foreach(KPControlCustomization kpcc in kpfc.Controls)
				{
					TreeNode tnControl = tnForm.Nodes.Add(kpcc.Name);
					tnControl.Tag = kpcc;

					dControls[kpfc.FullName + "." + kpcc.Name] = tnControl;
				}

				tnForm.Expand();
			}

			tv.EndUpdate();
		}

		public static void MergeForms(List<KPFormCustomization> lInto,
			List<KPFormCustomization> lFrom, StringBuilder sbUnusedText)
		{
			foreach(KPFormCustomization kpInto in lInto)
			{
				foreach(KPFormCustomization kpFrom in lFrom)
				{
					if(kpInto.FullName == kpFrom.FullName)
						MergeFormCustomizations(kpInto, kpFrom, sbUnusedText);
				}
			}
		}

		private static void MergeFormCustomizations(KPFormCustomization kpInto,
			KPFormCustomization kpFrom, StringBuilder sbUnusedText)
		{
			MergeControlCustomizations(kpInto.Window, kpFrom.Window, sbUnusedText);

			foreach(KPControlCustomization ccInto in kpInto.Controls)
			{
				foreach(KPControlCustomization ccFrom in kpFrom.Controls)
				{
					if(ccInto.Name == ccFrom.Name)
						MergeControlCustomizations(ccInto, ccFrom, sbUnusedText);
				}
			}
		}

		private static void MergeControlCustomizations(KPControlCustomization ccInto,
			KPControlCustomization ccFrom, StringBuilder sbUnusedText)
		{
			if(ccFrom.Text.Length > 0)
			{
				bool bTextValid = true;

				if(!m_bIgnoreBaseHash && (ccFrom.BaseHash.Length > 0) &&
					!ccInto.MatchHash(ccFrom.BaseHash))
					bTextValid = false;

				if(bTextValid) ccInto.Text = ccFrom.Text;
				else // Create a backup
				{
					string strTrimmed = ccFrom.Text.Trim();
					if(strTrimmed.Length > 0) sbUnusedText.AppendLine(strTrimmed);
				}
			}

			if(ccFrom.Layout.X.Length > 0) ccInto.Layout.X = ccFrom.Layout.X;
			if(ccFrom.Layout.Y.Length > 0) ccInto.Layout.Y = ccFrom.Layout.Y;
			if(ccFrom.Layout.Width.Length > 0) ccInto.Layout.Width = ccFrom.Layout.Width;
			if(ccFrom.Layout.Height.Length > 0) ccInto.Layout.Height = ccFrom.Layout.Height;
		}
	}
}
