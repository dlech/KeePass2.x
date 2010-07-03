/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2010 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePass.Native;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;

using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class DataViewerForm : Form
	{
		private string m_strDataDesc = string.Empty;
		private byte[] m_pbData = null;

		private bool m_bInitializing = false;

		private uint m_uStartOffset = 0;
		private BinaryDataClass m_bdc = BinaryDataClass.Unknown;

		private RichTextBoxContextMenu m_ctxText = new RichTextBoxContextMenu();

		public event EventHandler<DvfContextEventArgs> Initializing;
		public event EventHandler<DvfContextEventArgs> UpdatingDataView;
		public event EventHandler<DvfContextEventArgs> Uninitializing;

		public void InitEx(string strDataDesc, byte[] pbData)
		{
			if(strDataDesc != null) m_strDataDesc = strDataDesc;

			m_pbData = pbData;
		}

		public DataViewerForm()
		{
			InitializeComponent();
			Program.Translation.ApplyTo(this);
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			Debug.Assert(m_pbData != null);
			if(m_pbData == null) throw new InvalidOperationException();

			GlobalWindowManager.AddWindow(this);

			this.Icon = Properties.Resources.KeePass;

			m_tslViewer.Text = KPRes.ShowIn + ":";
			m_tslEncoding.Text = KPRes.Encoding + ":";

			if(m_strDataDesc.Length > 0)
				this.Text = m_strDataDesc + " - " + this.Text;

			this.DoubleBuffered = true;

			m_tssStatusMain.Text = KPRes.Ready;

			m_ctxText.Attach(m_rtbText);

			m_bInitializing = true;

			m_rtbText.Dock = DockStyle.Fill;
			m_webBrowser.Dock = DockStyle.Fill;
			m_pnlImageViewer.Dock = DockStyle.Fill;
			m_picBox.Dock = DockStyle.Fill;

			m_tscEncoding.Items.Add(BinaryDataClassifier.BdeAnsi + " (" +
				KPRes.SystemCodePage + ")");
			m_tscEncoding.Items.Add(BinaryDataClassifier.BdeAscii);
			m_tscEncoding.Items.Add(BinaryDataClassifier.BdeUtf7);
			m_tscEncoding.Items.Add(BinaryDataClassifier.BdeUtf8);
			m_tscEncoding.Items.Add(BinaryDataClassifier.BdeUtf32);
			m_tscEncoding.Items.Add(BinaryDataClassifier.BdeUnicodeLE);
			m_tscEncoding.Items.Add(BinaryDataClassifier.BdeUnicodeBE);

			string strEnc;
			Encoding enc = BinaryDataClassifier.GetStringEncoding(m_pbData,
				false, out strEnc, out m_uStartOffset);

			if(strEnc == BinaryDataClassifier.BdeAnsi)
				m_tscEncoding.SelectedIndex = 0;
			else if(strEnc == BinaryDataClassifier.BdeAscii)
				m_tscEncoding.SelectedIndex = 1;
			else if(strEnc == BinaryDataClassifier.BdeUtf7)
				m_tscEncoding.SelectedIndex = 2;
			else if(strEnc == BinaryDataClassifier.BdeUtf8)
				m_tscEncoding.SelectedIndex = 3;
			else if(strEnc == BinaryDataClassifier.BdeUtf32)
				m_tscEncoding.SelectedIndex = 4;
			else if(strEnc == BinaryDataClassifier.BdeUnicodeLE)
				m_tscEncoding.SelectedIndex = 5;
			else if(strEnc == BinaryDataClassifier.BdeUnicodeBE)
				m_tscEncoding.SelectedIndex = 6;
			else m_tscEncoding.SelectedIndex = 0;

			m_tscViewers.Items.Add(KPRes.TextViewer);
			m_tscViewers.Items.Add(KPRes.ImageViewer);
			m_tscViewers.Items.Add(KPRes.WebBrowser);

			m_bdc = BinaryDataClassifier.Classify(m_strDataDesc, m_pbData);

			if((m_bdc == BinaryDataClass.Text) || (m_bdc == BinaryDataClass.RichText))
				m_tscViewers.SelectedIndex = 0;
			else if(m_bdc == BinaryDataClass.Image) m_tscViewers.SelectedIndex = 1;
			else if(m_bdc == BinaryDataClass.WebDocument) m_tscViewers.SelectedIndex = 2;
			else m_tscViewers.SelectedIndex = 0;

			if(this.Initializing != null)
				this.Initializing(this, new DvfContextEventArgs(this,
					m_pbData, m_strDataDesc, m_tscViewers));

			m_bInitializing = false;
			UpdateDataView(enc);
		}

		private void OnRichTextBoxLinkClicked(object sender, LinkClickedEventArgs e)
		{
			WinUtil.OpenUrl(e.LinkText, null);
		}

		private string BinaryDataToString(Encoding enc)
		{
			if(enc == null)
			{
				string strEnc = m_tscEncoding.Text;

				if(strEnc == (BinaryDataClassifier.BdeAnsi + " (" +
					KPRes.SystemCodePage + ")"))
				{
					enc = Encoding.Default;
				}
				else if(strEnc == BinaryDataClassifier.BdeAscii)
					enc = Encoding.ASCII;
				else if(strEnc == BinaryDataClassifier.BdeUtf7)
					enc = Encoding.UTF7;
				else if(strEnc == BinaryDataClassifier.BdeUtf8)
					enc = Encoding.UTF8;
				else if(strEnc == BinaryDataClassifier.BdeUtf32)
					enc = Encoding.UTF32;
				else if(strEnc == BinaryDataClassifier.BdeUnicodeLE)
					enc = Encoding.Unicode;
				else if(strEnc == BinaryDataClassifier.BdeUnicodeBE)
					enc = Encoding.BigEndianUnicode;
				else enc = Encoding.Default;
			}

			return enc.GetString(m_pbData, (int)m_uStartOffset,
				m_pbData.Length - (int)m_uStartOffset);
		}

		private void UpdateDataView(Encoding enc)
		{
			if(m_bInitializing) return;

			m_rtbText.Visible = false;
			m_webBrowser.Visible = false;
			m_picBox.Visible = false;
			m_pnlImageViewer.Visible = false;

			string strViewer = m_tscViewers.Text;

			m_tssSeparator0.Visible = m_tslEncoding.Visible =
				m_tscEncoding.Visible = !(strViewer == KPRes.ImageViewer);

			try
			{
				if(this.UpdatingDataView != null)
				{
					DvfContextEventArgs args = new DvfContextEventArgs(this,
						m_pbData, m_strDataDesc, m_tscViewers);
					this.UpdatingDataView(this, args);
					if(args.Cancel) return;
				}

				if(strViewer == KPRes.TextViewer)
				{
					string strData = BinaryDataToString(enc);

					if(m_bdc == BinaryDataClass.RichText) m_rtbText.Rtf = strData;
					else m_rtbText.Text = strData;

					m_rtbText.Visible = true;
				}
				else if(strViewer == KPRes.ImageViewer)
				{
					Image img = UIUtil.LoadImage(m_pbData);
					m_picBox.Image = img;

					m_pnlImageViewer.Visible = true;
					m_picBox.Visible = true;

					OnFormSizeChanged(null, null);
				}
				else if(strViewer == KPRes.WebBrowser)
				{
					string strData = BinaryDataToString(enc);

					UIUtil.SetWebBrowserDocument(m_webBrowser, strData);

					m_webBrowser.Visible = true;
				}
			}
			catch(Exception) { }
		}

		private void OnViewersSelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateDataView(null);
		}

		private void OnEncodingSelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateDataView(null);
		}

		private void OnFormSizeChanged(object sender, EventArgs e)
		{
			Image img = m_picBox.Image;
			if(img != null)
			{
				if((img.Width > m_picBox.ClientSize.Width) ||
					(img.Height > m_picBox.ClientSize.Height))
				{
					m_picBox.SizeMode = PictureBoxSizeMode.Zoom;
				}
				else m_picBox.SizeMode = PictureBoxSizeMode.CenterImage;
			}
		}

		private void OnFormClosing(object sender, FormClosingEventArgs e)
		{
			if(this.Uninitializing != null)
			{
				DvfContextEventArgs args = new DvfContextEventArgs(this,
					m_pbData, m_strDataDesc, m_tscViewers);
				this.Uninitializing(sender, args);
				if(args.Cancel)
				{
					e.Cancel = true;
					return;
				}
			}

			m_ctxText.Detach();
			GlobalWindowManager.RemoveWindow(this);
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if(keyData == Keys.Escape)
			{
				if(msg.Msg == NativeMethods.WM_KEYDOWN)
				{
					this.Close();
					return true;
				}
				else if(msg.Msg == NativeMethods.WM_KEYUP) return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}
	}

	public sealed class DvfContextEventArgs : CancelEventArgs
	{
		private DataViewerForm m_form;
		public DataViewerForm Form { get { return m_form; } }

		private byte[] m_pbData;
		public byte[] Data { get { return m_pbData; } }

		private string m_strDataDesc;
		public string DataDescription { get { return m_strDataDesc; } }

		private ToolStripComboBox m_tscViewers;
		public ToolStripComboBox ViewersComboBox { get { return m_tscViewers; } }

		public DvfContextEventArgs(DataViewerForm form, byte[] pbData,
			string strDataDesc, ToolStripComboBox cbViewers) :
			base(false)
		{
			m_form = form;
			m_pbData = pbData;
			m_strDataDesc = strDataDesc;
			m_tscViewers = cbViewers;
		}
	}
}
