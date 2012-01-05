/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2012 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.IO;

using KeePass.DataExchange;
using KeePass.UI;
using KeePass.Resources;

using KeePassLib;
using KeePassLib.Security;

namespace KeePass.Forms
{
	public partial class ImportCsvForm : Form
	{
		private PwDatabase m_pwDatabase = null;
		private byte[] m_pbInData = null;
		private bool m_bBlockChangedEvent = false;

		private string m_strSource = string.Empty;

		private const string StrEncAscii = "ASCII";
		private const string StrEncUtf7 = "UTF-7";
		private const string StrEncUtf8 = "UTF-8";
		private const string StrEncUtf32 = "UTF-32";
		private const string StrEncUnicode = "Unicode";
		private const string StrEncBigUnicode = "Big Endian Unicode";

		public void InitEx(PwDatabase pwStorage, byte[] pbInData)
		{
			m_pwDatabase = pwStorage;
			m_pbInData = pbInData;
		}

		public ImportCsvForm()
		{
			InitializeComponent();
			Program.Translation.ApplyTo(this);
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			Debug.Assert(m_pwDatabase != null); if(m_pwDatabase == null) throw new InvalidOperationException();
			Debug.Assert(m_pbInData != null); if(m_pbInData == null) throw new InvalidOperationException();

			GlobalWindowManager.AddWindow(this);

			this.Icon = Properties.Resources.KeePass;

			m_bBlockChangedEvent = true;

			AddFieldColumn(m_lvHeaderOrder, KPRes.Title, -1);
			AddFieldColumn(m_lvHeaderOrder, KPRes.UserName, -1);
			AddFieldColumn(m_lvHeaderOrder, KPRes.Password, -1);
			AddFieldColumn(m_lvHeaderOrder, KPRes.Url, -1);
			AddFieldColumn(m_lvHeaderOrder, KPRes.Notes, -1);
			AddFieldColumn(m_lvHeaderOrder, KPRes.Custom + " 1", -1);
			AddFieldColumn(m_lvHeaderOrder, KPRes.Custom + " 2", -1);
			AddFieldColumn(m_lvHeaderOrder, KPRes.Custom + " 3", -1);
			AddFieldColumn(m_lvHeaderOrder, "(" + KPRes.Ignore + ")", -1);
			AddFieldColumn(m_lvHeaderOrder, "(" + KPRes.Ignore + ")", -1);
			AddFieldColumn(m_lvHeaderOrder, "(" + KPRes.Ignore + ")", -1);

			foreach(ColumnHeader chHdr in m_lvHeaderOrder.Columns)
				chHdr.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);

			const int nPrevColSize = 120;
			AddFieldColumn(m_lvPreview, KPRes.Title, nPrevColSize);
			AddFieldColumn(m_lvPreview, KPRes.UserName, nPrevColSize);
			AddFieldColumn(m_lvPreview, KPRes.Password, nPrevColSize);
			AddFieldColumn(m_lvPreview, KPRes.Url, nPrevColSize);
			AddFieldColumn(m_lvPreview, KPRes.Notes, nPrevColSize);
			AddFieldColumn(m_lvPreview, KPRes.Custom + " 1", nPrevColSize);
			AddFieldColumn(m_lvPreview, KPRes.Custom + " 2", nPrevColSize);
			AddFieldColumn(m_lvPreview, KPRes.Custom + " 3", nPrevColSize);

			m_cmbEncoding.Items.Add(KPRes.EncodingAnsi);
			m_cmbEncoding.Items.Add(StrEncAscii);
			m_cmbEncoding.Items.Add(StrEncUtf7);
			m_cmbEncoding.Items.Add(StrEncUtf8);
			m_cmbEncoding.Items.Add(StrEncUtf32);
			m_cmbEncoding.Items.Add(StrEncUnicode);
			m_cmbEncoding.Items.Add(StrEncBigUnicode);

			m_cmbEncoding.SelectedIndex = 0;
			m_tbSepChar.Text = ",";
			m_cbDoubleQuoteToSingle.Checked = true;
			
			m_bBlockChangedEvent = false;

			UpdateStringSource();
			UpdatePreview();
		}

		private static void AddFieldColumn(ListView lv, string strText, int nSize)
		{
			ColumnHeader hdr = lv.Columns.Add(strText);

			if(nSize >= 0) hdr.Width = nSize;
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			OnBtnPreviewRefresh(sender, e);
			Application.DoEvents();

			int nItem = 0;
			foreach(ListViewItem lvi in m_lvPreview.Items)
			{
				m_pbRender.Value = (100 * nItem) / m_lvPreview.Items.Count;
				++nItem;

				PwEntry pe = new PwEntry(true, true);
				m_pwDatabase.RootGroup.AddEntry(pe, true);

				pe.Strings.Set(PwDefs.TitleField, new ProtectedString(
					m_pwDatabase.MemoryProtection.ProtectTitle, lvi.Text));
				pe.Strings.Set(PwDefs.UserNameField, new ProtectedString(
					m_pwDatabase.MemoryProtection.ProtectUserName, lvi.SubItems[1].Text));
				pe.Strings.Set(PwDefs.PasswordField, new ProtectedString(
					m_pwDatabase.MemoryProtection.ProtectPassword, lvi.SubItems[2].Text));
				pe.Strings.Set(PwDefs.UrlField, new ProtectedString(
					m_pwDatabase.MemoryProtection.ProtectUrl, lvi.SubItems[3].Text));
				pe.Strings.Set(PwDefs.NotesField, new ProtectedString(
					m_pwDatabase.MemoryProtection.ProtectNotes, lvi.SubItems[4].Text));

				string strCustom = lvi.SubItems[5].Text;
				if(strCustom.Length > 0)
					pe.Strings.Set(KPRes.Custom + " 1", new ProtectedString(false, strCustom));

				strCustom = lvi.SubItems[6].Text;
				if(strCustom.Length > 0)
					pe.Strings.Set(KPRes.Custom + " 2", new ProtectedString(false, strCustom));

				strCustom = lvi.SubItems[7].Text;
				if(strCustom.Length > 0)
					pe.Strings.Set(KPRes.Custom + " 3", new ProtectedString(false, strCustom));
			}

			m_pbRender.Value = 100;
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
		}

		private void UpdateStringSource()
		{
			string strEncoding = (m_cmbEncoding.Items[m_cmbEncoding.SelectedIndex] as string);

			Encoding enc = Encoding.Default;
			if(strEncoding == StrEncAscii) enc = Encoding.ASCII;
			else if(strEncoding == StrEncUtf7) enc = Encoding.UTF7;
			else if(strEncoding == StrEncUtf8) enc = Encoding.UTF8;
			else if(strEncoding == StrEncUtf32) enc = Encoding.UTF32;
			else if(strEncoding == StrEncUnicode) enc = Encoding.Unicode;
			else if(strEncoding == StrEncBigUnicode) enc = Encoding.BigEndianUnicode;
			else { Debug.Assert(strEncoding == KPRes.EncodingAnsi); }

			try
			{
				MemoryStream ms = new MemoryStream(m_pbInData, false);
				StreamReader sr = new StreamReader(ms, enc);
				m_strSource = sr.ReadToEnd();
				sr.Close();
				ms.Close();
			}
			catch(Exception) { m_strSource = KPRes.EncodingFail; }

			m_tbSourcePreview.Text = string.Empty;
			m_tbSourcePreview.Text = m_strSource;
		}

		private void UpdatePreview()
		{
			m_lvPreview.Items.Clear();
			// m_lvPreview.BeginUpdate();

			string strDelimiter = m_tbSepChar.Text;
			if(strDelimiter.Length == 0) return;

			char[] vPartTrimChars = new char[] { '\"' };
			int nLine = 0;
			string[] vLines = m_strSource.Split(new char[] { '\r', '\n' });
			foreach(string strLine in vLines)
			{
				m_pbRender.Value = (100 * nLine) / vLines.Length;
				++nLine;

				if((strLine == null) || (strLine.Length == 0)) continue;

				List<string> vParts = ImportUtil.SplitCsvLine(strLine, strDelimiter);

				for(int i = 0; i < vParts.Count; ++i)
					vParts[i] = vParts[i].Trim(vPartTrimChars);

				ListViewItem lvi = new ListViewItem(string.Empty);

				lvi.Text = SafeListIndex(vParts, m_lvHeaderOrder.Columns[0].DisplayIndex);
				lvi.SubItems.Add(SafeListIndex(vParts, m_lvHeaderOrder.Columns[1].DisplayIndex));
				lvi.SubItems.Add(SafeListIndex(vParts, m_lvHeaderOrder.Columns[2].DisplayIndex));
				lvi.SubItems.Add(SafeListIndex(vParts, m_lvHeaderOrder.Columns[3].DisplayIndex));
				lvi.SubItems.Add(SafeListIndex(vParts, m_lvHeaderOrder.Columns[4].DisplayIndex));
				lvi.SubItems.Add(SafeListIndex(vParts, m_lvHeaderOrder.Columns[5].DisplayIndex));
				lvi.SubItems.Add(SafeListIndex(vParts, m_lvHeaderOrder.Columns[6].DisplayIndex));
				lvi.SubItems.Add(SafeListIndex(vParts, m_lvHeaderOrder.Columns[7].DisplayIndex));

				m_lvPreview.Items.Add(lvi);
			}

			m_pbRender.Value = 100;
			// m_lvPreview.EndUpdate();
		}

		private static string SafeListIndex(List<string> vList, int nIndex)
		{
			if(nIndex >= vList.Count) return string.Empty;
			return vList[nIndex];
		}

		private void OnCmbEncodingSelectedIndexChanged(object sender, EventArgs e)
		{
			if(m_bBlockChangedEvent) return;

			UpdateStringSource();
			UpdatePreview();
		}

		private void OnDelimiterTextChanged(object sender, EventArgs e)
		{
			if(m_bBlockChangedEvent) return;

			UpdatePreview();
		}

		private void OnDoubleQuoteCheckedChanged(object sender, EventArgs e)
		{
			if(m_bBlockChangedEvent) return;

			UpdatePreview();
		}

		private void OnBtnPreviewRefresh(object sender, EventArgs e)
		{
			UpdatePreview();
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			GlobalWindowManager.RemoveWindow(this);
		}
	}
}