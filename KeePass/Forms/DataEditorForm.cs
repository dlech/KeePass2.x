/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2009 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Drawing.Text;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using KeePass.App;
using KeePass.App.Configuration;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;

using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class DataEditorForm : Form
	{
		private string m_strDataDesc = string.Empty;
		private byte[] m_pbData = null;
		private byte[] m_pbEditedData = null;

		private bool m_bModified = false;

		private bool m_bBlockEvents = false;
		private Stack<bool> m_lBlockStateStack = new Stack<bool>();
		private Stack<KeyValuePair<int, int>> m_lSelections =
			new Stack<KeyValuePair<int, int>>();
		private BinaryDataClass m_bdc = BinaryDataClass.Unknown;

		private RichTextBoxContextMenu m_ctxText = new RichTextBoxContextMenu();

		public byte[] EditedBinaryData
		{
			get { return m_pbEditedData; }
		}

		public MenuStrip MainMenuEx { get { return m_menuMain; } }

		public static bool SupportsDataType(BinaryDataClass bdc)
		{
			return ((bdc == BinaryDataClass.Text) || (bdc == BinaryDataClass.RichText));
		}

		public void InitEx(string strDataDesc, byte[] pbData)
		{
			if(strDataDesc != null) m_strDataDesc = strDataDesc;

			m_pbData = pbData;
		}

		public DataEditorForm()
		{
			InitializeComponent();
			Program.Translation.ApplyTo(this);
			Program.Translation.ApplyTo("KeePass.Forms.DataEditorForm.m_menuMain", m_menuMain.Items);

			if((Program.Config.UI.DataEditorWidth != AppDefs.InvalidWindowValue) &&
				(Program.Config.UI.DataEditorHeight != AppDefs.InvalidWindowValue))
			{
				this.Width = Program.Config.UI.DataEditorWidth;
				this.Height = Program.Config.UI.DataEditorHeight;
			}
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			Debug.Assert(m_pbData != null);
			if(m_pbData == null) throw new InvalidOperationException();

			GlobalWindowManager.AddWindow(this);

			this.Icon = Properties.Resources.KeePass;
			this.DoubleBuffered = true;

			m_bdc = BinaryDataClassifier.Classify(m_strDataDesc, m_pbData);
			string strEncodingName;
			uint uStartOffset;
			Encoding enc = BinaryDataClassifier.GetStringEncoding(m_pbData,
				false, out strEncodingName, out uStartOffset);
			string strData = enc.GetString(m_pbData);

			BlockUIEvents(true);

			UIUtil.ConfigureTbButton(m_tbFileSave, KPRes.Save, null);
			UIUtil.ConfigureTbButton(m_tbEditCut, KPRes.Cut, null);
			UIUtil.ConfigureTbButton(m_tbEditCopy, KPRes.Copy, null);
			UIUtil.ConfigureTbButton(m_tbEditPaste, KPRes.Paste, null);
			UIUtil.ConfigureTbButton(m_tbEditUndo, KPRes.Undo, null);
			UIUtil.ConfigureTbButton(m_tbEditRedo, KPRes.Redo, null);
			UIUtil.ConfigureTbButton(m_tbFormatBold, KPRes.Bold, null);
			UIUtil.ConfigureTbButton(m_tbFormatItalic, KPRes.Italic, null);
			UIUtil.ConfigureTbButton(m_tbFormatUnderline, KPRes.Underline, null);
			UIUtil.ConfigureTbButton(m_tbFormatStrikeout, KPRes.Strikeout, null);
			UIUtil.ConfigureTbButton(m_tbColorForeground, KPRes.TextColor, null);
			UIUtil.ConfigureTbButton(m_tbColorBackground, KPRes.BackgroundColor, null);
			UIUtil.ConfigureTbButton(m_tbAlignCenter, KPRes.AlignCenter, null);
			UIUtil.ConfigureTbButton(m_tbAlignLeft, KPRes.AlignLeft, null);
			UIUtil.ConfigureTbButton(m_tbAlignRight, KPRes.AlignRight, null);

			m_rtbText.Dock = DockStyle.Fill;
			m_ctxText.Attach(m_rtbText);
			m_tssStatusMain.Text = KPRes.Ready;
			m_rtbText.WordWrap = Program.Config.UI.DataEditorWordWrap;

			InitFormattingToolBar();

			bool bSimpleText = true;
			if(m_bdc == BinaryDataClass.RichText)
			{
				try { m_rtbText.Rtf = strData; bSimpleText = false; }
				catch(Exception) { } // Show as simple text
			}
			
			if(bSimpleText)
			{
				m_rtbText.Text = strData;

				if(Program.Config.UI.DataEditorFont.OverrideUIDefault)
				{
					m_rtbText.SelectAll();
					m_rtbText.SelectionFont = Program.Config.UI.DataEditorFont.ToFont();
				}
			}

			m_rtbText.Select(0, 0);
			BlockUIEvents(false);
			UpdateUIState(false, true);
		}

		private void InitFormattingToolBar()
		{
			if(m_bdc != BinaryDataClass.RichText)
			{
				m_toolFormat.Visible = false;
				return;
			}

			InstalledFontCollection c = new InstalledFontCollection();
			foreach(FontFamily ff in c.Families)
				m_tbFontCombo.Items.Add(ff.Name);

			m_tbFontCombo.ToolTipText = KPRes.Font;

			int[] vSizes = new int[]{ 8, 9, 10, 11, 12, 14, 16, 18, 20,
				22, 24, 26, 28, 36, 48, 72 };
			foreach(int nSize in vSizes)
				m_tbFontSizeCombo.Items.Add(nSize.ToString());

			m_tbFontSizeCombo.ToolTipText = KPRes.Size;
		}

		private void UpdateUIState(bool bSetModified, bool bFocusText)
		{
			BlockUIEvents(true);
			if(bSetModified) m_bModified = true;

			this.Text = (((m_strDataDesc.Length > 0) ? (m_strDataDesc +
				(m_bModified ? "*" : string.Empty) + " - ") : string.Empty) +
				KPRes.KeePassEditor);

			m_menuViewFont.Enabled = (m_bdc == BinaryDataClass.Text);
			m_menuViewWordWrap.Checked = m_rtbText.WordWrap;

			m_tbFileSave.Image = (m_bModified ? Properties.Resources.B16x16_FileSave :
				Properties.Resources.B16x16_FileSave_Disabled);

			m_tbEditUndo.Enabled = m_rtbText.CanUndo;
			m_tbEditRedo.Enabled = m_rtbText.CanRedo;

			Font fSel = m_rtbText.SelectionFont;
			if(fSel != null)
			{
				m_tbFormatBold.Checked = fSel.Bold;
				m_tbFormatItalic.Checked = fSel.Italic;
				m_tbFormatUnderline.Checked = fSel.Underline;
				m_tbFormatStrikeout.Checked = fSel.Strikeout;

				string strFontName = fSel.Name;
				if(m_tbFontCombo.Items.IndexOf(strFontName) >= 0)
					m_tbFontCombo.SelectedItem = strFontName;
				else m_tbFontCombo.Text = strFontName;

				string strFontSize = fSel.SizeInPoints.ToString();
				if(m_tbFontSizeCombo.Items.IndexOf(strFontSize) >= 0)
					m_tbFontSizeCombo.SelectedItem = strFontSize;
				else m_tbFontSizeCombo.Text = strFontSize;
			}

			HorizontalAlignment ha = m_rtbText.SelectionAlignment;
			m_tbAlignLeft.Checked = (ha == HorizontalAlignment.Left);
			m_tbAlignCenter.Checked = (ha == HorizontalAlignment.Center);
			m_tbAlignRight.Checked = (ha == HorizontalAlignment.Right);

			BlockUIEvents(false);
			if(bFocusText) m_rtbText.Focus();
		}

		private void BlockUIEvents(bool bBlock)
		{
			if(bBlock)
			{
				m_lBlockStateStack.Push(m_bBlockEvents);
				m_bBlockEvents = true;
			}
			else m_bBlockEvents = m_lBlockStateStack.Pop();
		}

		private void UISelectAllText(bool bSelect)
		{
			if(bSelect)
			{
				m_lSelections.Push(new KeyValuePair<int, int>(m_rtbText.SelectionStart,
					m_rtbText.SelectionLength));
				m_rtbText.SelectAll();
			}
			else
			{
				KeyValuePair<int, int> kvp = m_lSelections.Pop();
				m_rtbText.Select(kvp.Key, kvp.Value);
			}
		}

		private void OnFileSave(object sender, EventArgs e)
		{
			if(m_bdc == BinaryDataClass.RichText)
				m_pbEditedData = Encoding.UTF8.GetBytes(m_rtbText.Rtf);
			else m_pbEditedData = Encoding.UTF8.GetBytes(m_rtbText.Text);

			m_bModified = false;
			UpdateUIState(false, false);
		}

		private void OnFormClosing(object sender, FormClosingEventArgs e)
		{
			if(m_bModified)
			{
				if(MessageService.AskYesNo(KPRes.SaveBeforeCloseQuestion))
					OnFileSave(sender, EventArgs.Empty);
			}

			Program.Config.UI.DataEditorWidth = this.Width;
			Program.Config.UI.DataEditorHeight = this.Height;

			m_ctxText.Detach();
			GlobalWindowManager.RemoveWindow(this);
		}

		private void OnFormatBoldClicked(object sender, EventArgs e)
		{
			if(m_bBlockEvents || (m_bdc != BinaryDataClass.RichText)) return;

			Font f = m_rtbText.SelectionFont;
			if(f == null) return;

			m_rtbText.SelectionFont = new Font(f, f.Style ^ FontStyle.Bold);
	
			UpdateUIState(true, true);
		}

		private void OnFormatItalicClicked(object sender, EventArgs e)
		{
			if(m_bBlockEvents || (m_bdc != BinaryDataClass.RichText)) return;

			Font f = m_rtbText.SelectionFont;
			if(f == null) return;

			m_rtbText.SelectionFont = new Font(f, f.Style ^ FontStyle.Italic);
	
			UpdateUIState(true, true);
		}

		private void OnFormatUnderlineClicked(object sender, EventArgs e)
		{
			if(m_bBlockEvents || (m_bdc != BinaryDataClass.RichText)) return;

			Font f = m_rtbText.SelectionFont;
			if(f == null) return;

			m_rtbText.SelectionFont = new Font(f, f.Style ^ FontStyle.Underline);

			UpdateUIState(true, true);
		}

		private void OnFormatStrikeoutClicked(object sender, EventArgs e)
		{
			if(m_bBlockEvents || (m_bdc != BinaryDataClass.RichText)) return;

			Font f = m_rtbText.SelectionFont;
			if(f == null) return;

			m_rtbText.SelectionFont = new Font(f, f.Style ^ FontStyle.Strikeout);
			
			UpdateUIState(true, true);
		}

		private void OnTextSelectionChanged(object sender, EventArgs e)
		{
			if(m_bBlockEvents || (m_bdc != BinaryDataClass.RichText)) return;

			UpdateUIState(false, false);
		}

		private static bool ShowColorDialog(Color clrCurrent, out Color clrSelected)
		{
			ColorDialog dlg = new ColorDialog();

			dlg.AllowFullOpen = true;
			dlg.AnyColor = true;
			dlg.Color = clrCurrent;
			dlg.FullOpen = true;
			dlg.SolidColorOnly = true;

			if(dlg.ShowDialog() == DialogResult.OK)
			{
				clrSelected = dlg.Color;
				return true;
			}

			clrSelected = clrCurrent;
			return false;
		}

		private void OnColorForegroundClicked(object sender, EventArgs e)
		{
			if(m_bBlockEvents || (m_bdc != BinaryDataClass.RichText)) return;

			Color clr;
			if(ShowColorDialog(m_rtbText.SelectionColor, out clr))
			{
				m_rtbText.SelectionColor = clr;
				UpdateUIState(true, true);
			}
		}

		private void OnColorBackgroundClicked(object sender, EventArgs e)
		{
			if(m_bBlockEvents || (m_bdc != BinaryDataClass.RichText)) return;

			Color clr;
			if(ShowColorDialog(m_rtbText.SelectionBackColor, out clr))
			{
				m_rtbText.SelectionBackColor = clr;
				UpdateUIState(true, true);
			}
		}

		private void OnTextLinkClicked(object sender, LinkClickedEventArgs e)
		{
			WinUtil.OpenUrl(e.LinkText, null);
		}

		private void OnFileExit(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
		}

		private void OnTextTextChanged(object sender, EventArgs e)
		{
			if(m_bBlockEvents) return;

			UpdateUIState(true, false);
		}

		private void OnAlignLeftClicked(object sender, EventArgs e)
		{
			if(m_bBlockEvents || (m_bdc != BinaryDataClass.RichText)) return;

			m_rtbText.SelectionAlignment = HorizontalAlignment.Left;

			UpdateUIState(true, true);
		}

		private void OnAlignCenterClicked(object sender, EventArgs e)
		{
			if(m_bBlockEvents || (m_bdc != BinaryDataClass.RichText)) return;

			m_rtbText.SelectionAlignment = HorizontalAlignment.Center;

			UpdateUIState(true, true);
		}

		private void OnAlignRightClicked(object sender, EventArgs e)
		{
			if(m_bBlockEvents || (m_bdc != BinaryDataClass.RichText)) return;

			m_rtbText.SelectionAlignment = HorizontalAlignment.Right;

			UpdateUIState(true, true);
		}

		private void OnFontComboSelectedIndexChanged(object sender, EventArgs e)
		{
			if(m_bBlockEvents || (m_bdc != BinaryDataClass.RichText)) return;

			Font f = m_rtbText.SelectionFont;
			m_rtbText.SelectionFont = new Font(m_tbFontCombo.Text, f.Size,
				f.Style, f.Unit, f.GdiCharSet, f.GdiVerticalFont);

			UpdateUIState(true, true);
		}

		private void OnFontSizeComboSelectedIndexChanged(object sender, EventArgs e)
		{
			if(m_bBlockEvents || (m_bdc != BinaryDataClass.RichText)) return;

			Font f = m_rtbText.SelectionFont;
			float fSize;
			if(!float.TryParse(m_tbFontSizeCombo.Text, out fSize)) fSize = f.SizeInPoints;
			m_rtbText.SelectionFont = new Font(f.Name, fSize,
				f.Style, f.Unit, f.GdiCharSet, f.GdiVerticalFont);

			UpdateUIState(true, true);
		}

		private void OnFontComboKeyDown(object sender, KeyEventArgs e)
		{
			if((e.KeyCode == Keys.Enter) || (e.KeyCode == Keys.Return))
				OnFontComboSelectedIndexChanged(sender, e);
		}

		private void OnFontSizeComboKeyDown(object sender, KeyEventArgs e)
		{
			if((e.KeyCode == Keys.Enter) || (e.KeyCode == Keys.Return))
				OnFontSizeComboSelectedIndexChanged(sender, e);
		}

		private void OnEditCut(object sender, EventArgs e)
		{
			m_rtbText.Cut();
			UpdateUIState(true, true);
		}

		private void OnEditCopy(object sender, EventArgs e)
		{
			m_rtbText.Copy();
			UpdateUIState(false, true);
		}

		private void OnEditPaste(object sender, EventArgs e)
		{
			m_rtbText.Paste();
			UpdateUIState(true, true);
		}

		private void OnEditUndo(object sender, EventArgs e)
		{
			m_rtbText.Undo();
			UpdateUIState(true, true);
		}

		private void OnEditRedo(object sender, EventArgs e)
		{
			m_rtbText.Redo();
			UpdateUIState(true, true);
		}

		private void OnViewFont(object sender, EventArgs e)
		{
			FontDialog dlg = new FontDialog();
			dlg.Font = Program.Config.UI.DataEditorFont.ToFont();
			dlg.ShowColor = false;

			if(dlg.ShowDialog() == DialogResult.OK)
			{
				Program.Config.UI.DataEditorFont = new AceFont(dlg.Font);
				Program.Config.UI.DataEditorFont.OverrideUIDefault = true;

				if(m_bdc == BinaryDataClass.Text)
				{
					bool bModified = m_bModified; // Save modified state

					UISelectAllText(true);
					m_rtbText.SelectionFont = dlg.Font;
					UISelectAllText(false);

					m_bModified = bModified;
					UpdateUIState(false, false);
				}
			}
		}

		private void OnViewWordWrap(object sender, EventArgs e)
		{
			m_rtbText.WordWrap = !m_rtbText.WordWrap;
			Program.Config.UI.DataEditorWordWrap = m_rtbText.WordWrap;
			UpdateUIState(false, false);
		}
	}
}
