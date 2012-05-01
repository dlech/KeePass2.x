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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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

		private Image m_img = null;
		private Image m_imgResized = null;

		public event EventHandler<DvfContextEventArgs> DvfInit;
		public event EventHandler<DvfContextEventArgs> DvfUpdating;
		public event EventHandler<DvfContextEventArgs> DvfRelease;

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

			if(m_strDataDesc.Length > 0)
				this.Text = m_strDataDesc + " - " + this.Text;

			this.DoubleBuffered = true;

			m_bInitializing = true;

			m_tssStatusMain.Text = KPRes.Ready;
			m_ctxText.Attach(m_rtbText, this);
			m_rtbText.Dock = DockStyle.Fill;
			m_webBrowser.Dock = DockStyle.Fill;
			m_pnlImageViewer.Dock = DockStyle.Fill;
			m_picBox.Dock = DockStyle.Fill;

			m_tslEncoding.Text = KPRes.Encoding + ":";

			foreach(StrEncodingInfo seiEnum in StrUtil.Encodings)
			{
				m_tscEncoding.Items.Add(seiEnum.Name);
			}

			StrEncodingInfo seiGuess = BinaryDataClassifier.GetStringEncoding(
				m_pbData, out m_uStartOffset);

			int iSel = 0;
			if(seiGuess != null)
				iSel = Math.Max(m_tscEncoding.FindStringExact(seiGuess.Name), 0);
			m_tscEncoding.SelectedIndex = iSel;

			m_tslZoom.Text = KPRes.Zoom + ":";

			m_tscZoom.Items.Add(KPRes.Auto);
			int[] vZooms = new int[] { 10, 25, 50, 75, 100, 125, 150, 200, 400 };
			foreach(int iZoom in vZooms)
				m_tscZoom.Items.Add(iZoom.ToString() + @"%");
			m_tscZoom.SelectedIndex = 0;

			m_tslViewer.Text = KPRes.ShowIn + ":";

			m_tscViewers.Items.Add(KPRes.TextViewer);
			m_tscViewers.Items.Add(KPRes.ImageViewer);
			m_tscViewers.Items.Add(KPRes.WebBrowser);

			m_bdc = BinaryDataClassifier.Classify(m_strDataDesc, m_pbData);

			if((m_bdc == BinaryDataClass.Text) || (m_bdc == BinaryDataClass.RichText))
				m_tscViewers.SelectedIndex = 0;
			else if(m_bdc == BinaryDataClass.Image) m_tscViewers.SelectedIndex = 1;
			else if(m_bdc == BinaryDataClass.WebDocument) m_tscViewers.SelectedIndex = 2;
			else m_tscViewers.SelectedIndex = 0;

			if(this.DvfInit != null)
				this.DvfInit(this, new DvfContextEventArgs(this, m_pbData,
					m_strDataDesc, m_tscViewers));

			m_bInitializing = false;
			UpdateDataView();
		}

		private void OnRichTextBoxLinkClicked(object sender, LinkClickedEventArgs e)
		{
			WinUtil.OpenUrl(e.LinkText, null);
		}

		private string BinaryDataToString()
		{
			string strEnc = m_tscEncoding.Text;
			StrEncodingInfo sei = StrUtil.GetEncoding(strEnc);

			try
			{
				return (sei.Encoding.GetString(m_pbData, (int)m_uStartOffset,
					m_pbData.Length - (int)m_uStartOffset) ?? string.Empty);
			}
			catch(Exception) { }

			return string.Empty;
		}

		private void UpdateDataView()
		{
			if(m_bInitializing) return;

			m_rtbText.Visible = false;
			m_webBrowser.Visible = false;
			m_picBox.Visible = false;
			m_pnlImageViewer.Visible = false;

			string strViewer = m_tscViewers.Text;

			bool bText = ((strViewer == KPRes.TextViewer) ||
				(strViewer == KPRes.WebBrowser));
			bool bImage = (strViewer == KPRes.ImageViewer);

			m_tssSeparator0.Visible = (bText || bImage);
			m_tslEncoding.Visible = m_tscEncoding.Visible = bText;
			m_tslZoom.Visible = m_tscZoom.Visible = bImage;

			try
			{
				if(this.DvfUpdating != null)
				{
					DvfContextEventArgs args = new DvfContextEventArgs(this,
						m_pbData, m_strDataDesc, m_tscViewers);
					this.DvfUpdating(this, args);
					if(args.Cancel) return;
				}

				if(strViewer == KPRes.TextViewer)
				{
					string strData = BinaryDataToString();

					m_rtbText.Clear(); // Clear formatting
					if(m_bdc == BinaryDataClass.RichText) m_rtbText.Rtf = strData;
					else m_rtbText.Text = strData;

					m_rtbText.Visible = true;
				}
				else if(strViewer == KPRes.ImageViewer)
				{
					if(m_img == null) m_img = UIUtil.LoadImage(m_pbData);
					// m_picBox.Image = m_img;

					m_pnlImageViewer.Visible = true;
					m_picBox.Visible = true;

					UpdateImageView();
				}
				else if(strViewer == KPRes.WebBrowser)
				{
					string strData = BinaryDataToString();

					UIUtil.SetWebBrowserDocument(m_webBrowser, strData);

					m_webBrowser.Visible = true;
				}
			}
			catch(Exception) { }
		}

		private void OnViewersSelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateDataView();
		}

		private void OnEncodingSelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateDataView();
		}

		private void OnFormSizeChanged(object sender, EventArgs e)
		{
			UpdateImageView();
		}

		private void UpdateImageView()
		{
			if(m_img == null) return;

			string strZoom = m_tscZoom.Text;
			if(string.IsNullOrEmpty(strZoom) || (strZoom == KPRes.Auto))
			{
				m_pnlImageViewer.AutoScroll = false;
				m_picBox.Dock = DockStyle.Fill;
				m_picBox.Image = m_img;

				if((m_img.Width > m_picBox.ClientSize.Width) ||
					(m_img.Height > m_picBox.ClientSize.Height))
				{
					m_picBox.SizeMode = PictureBoxSizeMode.Zoom;
				}
				else m_picBox.SizeMode = PictureBoxSizeMode.CenterImage;

				return;
			}

			if(!strZoom.EndsWith(@"%")) { Debug.Assert(false); return; }

			int iZoom;
			if(!int.TryParse(strZoom.Substring(0, strZoom.Length - 1), out iZoom))
			{
				Debug.Assert(false);
				return;
			}

			int cliW = m_pnlImageViewer.ClientRectangle.Width;
			int cliH = m_pnlImageViewer.ClientRectangle.Height;

			int dx = (m_img.Width * iZoom) / 100;
			int dy = (m_img.Height * iZoom) / 100;

			float fScrollX = 0.5f, fScrollY = 0.5f;
			if(m_pnlImageViewer.AutoScroll)
			{
				Point ptOffset = m_pnlImageViewer.AutoScrollPosition;
				Size sz = m_picBox.ClientSize;

				if(sz.Width > cliW)
				{
					fScrollX = Math.Abs((float)ptOffset.X / (float)(sz.Width - cliW));
					if(fScrollX < 0.0f) { Debug.Assert(false); fScrollX = 0.0f; }
					if(fScrollX > 1.0f) { Debug.Assert(false); fScrollX = 1.0f; }
				}

				if(sz.Height > cliH)
				{
					fScrollY = Math.Abs((float)ptOffset.Y / (float)(sz.Height - cliH));
					if(fScrollY < 0.0f) { Debug.Assert(false); fScrollY = 0.0f; }
					if(fScrollY > 1.0f) { Debug.Assert(false); fScrollY = 1.0f; }
				}
			}
			m_pnlImageViewer.AutoScroll = false;

			m_picBox.Dock = DockStyle.None;
			m_picBox.SizeMode = PictureBoxSizeMode.AutoSize;

			int x = 0, y = 0;
			if(dx < cliW) x = (cliW - dx) / 2;
			if(dy < cliH) y = (cliH - dy) / 2;

			m_picBox.Location = new Point(x, y);

			if((dx == m_img.Width) && (dy == m_img.Height))
				m_picBox.Image = m_img;
			else if((m_imgResized != null) && (m_imgResized.Width == dx) &&
				(m_imgResized.Height == dy))
				m_picBox.Image = m_imgResized;
			else
			{
				Image imgToDispose = m_imgResized;

				Image img = new Bitmap(dx, dy, PixelFormat.Format32bppArgb);
				using(Graphics g = Graphics.FromImage(img))
				{
					g.InterpolationMode = InterpolationMode.High;
					g.SmoothingMode = SmoothingMode.HighQuality;
					g.DrawImage(m_img, 0, 0, img.Width, img.Height);
				}
				m_imgResized = img;
				m_picBox.Image = m_imgResized;

				if(imgToDispose != null) imgToDispose.Dispose();
			}

			m_pnlImageViewer.AutoScroll = true;

			int sx = 0, sy = 0;
			if(dx > cliW) sx = (int)(fScrollX * (float)(dx - cliW));
			if(dy > cliH) sy = (int)(fScrollY * (float)(dy - cliH));
			try { m_pnlImageViewer.AutoScrollPosition = new Point(sx, sy); }
			catch(Exception) { Debug.Assert(false); }
		}

		private void OnFormClosing(object sender, FormClosingEventArgs e)
		{
			if(this.DvfRelease != null)
			{
				DvfContextEventArgs args = new DvfContextEventArgs(this,
					m_pbData, m_strDataDesc, m_tscViewers);
				this.DvfRelease(sender, args);
				if(args.Cancel)
				{
					e.Cancel = true;
					return;
				}
			}

			m_picBox.Image = null;
			if(m_img != null) { m_img.Dispose(); m_img = null; }
			if(m_imgResized != null) { m_imgResized.Dispose(); m_imgResized = null; }

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

		private void OnZoomSelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateImageView();
		}
	}

	public sealed class DvfContextEventArgs : CancellableOperationEventArgs
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
			string strDataDesc, ToolStripComboBox cbViewers)
		{
			m_form = form;
			m_pbData = pbData;
			m_strDataDesc = strDataDesc;
			m_tscViewers = cbViewers;
		}
	}
}
