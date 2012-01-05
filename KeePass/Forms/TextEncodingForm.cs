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

using KeePass.UI;
using KeePass.Util;
using KeePass.Resources;

namespace KeePass.Forms
{
	public partial class TextEncodingForm : Form
	{
		private string m_strContext = string.Empty;
		private byte[] m_pbData = null;
		private bool m_bInitializing = false;
		private Encoding m_encSel = null;

		private string[] m_vEncText = new string[] {
			KPRes.BinaryNoConv,
			BinaryDataClassifier.BdeAnsi + " (" + KPRes.SystemCodePage + ")",
			BinaryDataClassifier.BdeAscii,
			BinaryDataClassifier.BdeUtf7, BinaryDataClassifier.BdeUtf8,
			BinaryDataClassifier.BdeUtf32,
			BinaryDataClassifier.BdeUnicodeLE, BinaryDataClassifier.BdeUnicodeBE
		};

		private Encoding[] m_vEnc = new Encoding[] {
			null,
			Encoding.Default,
			Encoding.ASCII,
			Encoding.UTF7, new UTF8Encoding(false),
			Encoding.UTF32,
			Encoding.Unicode, Encoding.BigEndianUnicode
		};

		public Encoding SelectedEncoding
		{
			get { return m_encSel; }
		}

		public void InitEx(string strContext, byte[] pbData)
		{
			m_strContext = (strContext ?? string.Empty);
			m_pbData = pbData;
		}

		public TextEncodingForm()
		{
			InitializeComponent();
			Program.Translation.ApplyTo(this);
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			GlobalWindowManager.AddWindow(this);

			m_bInitializing = true;
			FontUtil.AssignDefaultBold(m_lblContext);
			m_lblContext.Text = m_strContext;

			foreach(string strEnc in m_vEncText)
				m_cmbEnc.Items.Add(strEnc);

			string strDet;
			uint uStartOffset;
			BinaryDataClassifier.GetStringEncoding(m_pbData, false,
				out strDet, out uStartOffset);
			int iDet = Array.IndexOf<string>(m_vEncText, strDet);
			m_cmbEnc.SelectedIndex = ((iDet >= 0) ? iDet : 0);

			m_bInitializing = false;
			UpdateTextPreview();
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			GlobalWindowManager.RemoveWindow(this);
		}

		private void UpdateTextPreview()
		{
			if(m_bInitializing) return;
			if(m_pbData == null) { Debug.Assert(false); return; }

			try
			{
				Encoding enc = m_vEnc[m_cmbEnc.SelectedIndex];
				if(enc == null) enc = new UTF8Encoding(false);

				m_rtbPreview.Text = enc.GetString(m_pbData);
			}
			catch(Exception) { m_rtbPreview.Text = string.Empty; }
		}

		private void OnEncSelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateTextPreview();
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			m_encSel = m_vEnc[m_cmbEnc.SelectedIndex];
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
		}
	}
}
