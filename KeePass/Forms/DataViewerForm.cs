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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;

using KeePass.App;
using KeePass.Native;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Utility;

using NativeLib = KeePassLib.Native.NativeLib;

namespace KeePass.Forms
{
	public partial class DataViewerForm : Form
	{
		private string m_strDataDesc = string.Empty;
		private byte[] m_pbData = null;

		private bool m_bInitializing = true;

		private uint m_uStartOffset = 0;
		private BinaryDataClass m_bdc = BinaryDataClass.Unknown;

		private readonly string m_strViewerHex = KPRes.HexViewer;
		private readonly string m_strViewerText = KPRes.TextViewer;
		private readonly string m_strViewerImage = KPRes.ImageViewer;
		private readonly string m_strViewerWeb = KPRes.WebBrowser;

		private readonly string m_strZoomAuto = KPRes.Auto;

		// Link on Windows, hint on Linux (linkifying only works on Windows)
		private readonly string m_strDataExpand = (NativeLib.IsUnix() ?
			("--- " + KPRes.More + " ---") :
			("--- " + KPRes.ShowMore + " (" + KPRes.TimeReq + ") ---"));
		private bool m_bDataExpanded = false;

		private string m_strInitialFormRect = string.Empty;
		private RichTextBoxContextMenu m_ctxText = new RichTextBoxContextMenu();

		private Image m_img = null;
		private Image m_imgResized = null;

		public event EventHandler<DvfContextEventArgs> DvfInit;
		public event EventHandler<DvfContextEventArgs> DvfUpdating;
		public event EventHandler<DvfContextEventArgs> DvfRelease;

		public static bool SupportsDataType(BinaryDataClass bdc)
		{
			return ((bdc == BinaryDataClass.Text) || (bdc == BinaryDataClass.RichText) ||
				(bdc == BinaryDataClass.Image) || (bdc == BinaryDataClass.WebDocument));
		}

		public void InitEx(string strDataDesc, byte[] pbData)
		{
			if(strDataDesc != null) m_strDataDesc = strDataDesc;

			m_pbData = pbData;
		}

		public DataViewerForm()
		{
			InitializeComponent();

			// GlobalWindowManager.InitializeForm checks docked controls
			m_rtbText.Dock = DockStyle.Fill;
			m_pnlImageViewer.Dock = DockStyle.Fill;
			m_picBox.Dock = DockStyle.Fill;
			m_webBrowser.Dock = DockStyle.Fill;

			GlobalWindowManager.InitializeForm(this);
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			Debug.Assert(m_pbData != null);
			if(m_pbData == null) throw new InvalidOperationException();

			m_bInitializing = true;

			GlobalWindowManager.AddWindow(this);

			this.Icon = AppIcons.Default;
			this.DoubleBuffered = true;

			string strTitle = KPRes.DataViewerKP;
			if(m_strDataDesc.Length > 0)
				strTitle = m_strDataDesc + " - " + strTitle;
			this.Text = strTitle;

			m_strInitialFormRect = UIUtil.SetWindowScreenRectEx(this,
				Program.Config.UI.DataViewerRect);

			m_tssStatusMain.Text = KPRes.Ready;
			m_ctxText.Attach(m_rtbText, this);

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

			// Required for mouse wheel handling
			Debug.Assert(m_tscZoom.DropDownStyle == ComboBoxStyle.DropDownList);

			m_tscZoom.Items.Add(m_strZoomAuto);
			int[] vZooms = new int[] { 10, 25, 50, 75, 100, 125, 150, 200, 400 };
			foreach(int iZoom in vZooms)
				m_tscZoom.Items.Add(iZoom.ToString() + "%");
			m_tscZoom.SelectedIndex = 0;

			m_tsbZoomOut.ToolTipText = KPRes.Zoom + " - (" + UIUtil.GetKeysName(
				Keys.Control | Keys.Subtract) + ")";
			m_tsbZoomIn.ToolTipText = KPRes.Zoom + " + (" + UIUtil.GetKeysName(
				Keys.Control | Keys.Add) + ")";

			m_tslViewer.Text = KPRes.ShowIn + ":";

			m_tscViewers.Items.Add(m_strViewerHex);
			m_tscViewers.Items.Add(m_strViewerText);
			m_tscViewers.Items.Add(m_strViewerImage);
			m_tscViewers.Items.Add(m_strViewerWeb);

			m_bdc = BinaryDataClassifier.Classify(m_strDataDesc, m_pbData);

			if((m_bdc == BinaryDataClass.Text) || (m_bdc == BinaryDataClass.RichText))
				m_tscViewers.SelectedIndex = 1;
			else if(m_bdc == BinaryDataClass.Image) m_tscViewers.SelectedIndex = 2;
			else if(m_bdc == BinaryDataClass.WebDocument) m_tscViewers.SelectedIndex = 3;
			else m_tscViewers.SelectedIndex = 0;

			if(this.DvfInit != null)
				this.DvfInit(this, new DvfContextEventArgs(this, m_pbData,
					m_strDataDesc, m_tscViewers));

			m_picBox.MouseWheel += this.OnPicBoxMouseWheel;

			m_bInitializing = false;
			UpdateDataView();
		}

		private void OnRichTextBoxLinkClicked(object sender, LinkClickedEventArgs e)
		{
			try
			{
				string strLink = e.LinkText;
				if(string.IsNullOrEmpty(strLink)) { Debug.Assert(false); return; }

				string strViewer = m_tscViewers.Text;
				bool bTextViewer = ((strViewer == m_strViewerHex) ||
					(strViewer == m_strViewerText));

				if((strLink == m_strDataExpand) && bTextViewer)
				{
					m_bDataExpanded = true;

					UpdateDataView();

					m_rtbText.Select(m_rtbText.TextLength, 0);
					m_rtbText.ScrollToCaret();
				}
				else WinUtil.OpenUrl(strLink, null);
			}
			catch(Exception) { } // ScrollToCaret might throw (but still works)
		}

		private string BinaryDataToString(bool bReplaceNulls, out bool bDecodedStringValid)
		{
			string strEnc = m_tscEncoding.Text;
			StrEncodingInfo sei = StrUtil.GetEncoding(strEnc);

			try
			{
				string str = (sei.Encoding.GetString(m_pbData, (int)m_uStartOffset,
					m_pbData.Length - (int)m_uStartOffset) ?? string.Empty);

				bDecodedStringValid = StrUtil.IsValid(str);

				if(bReplaceNulls) str = StrUtil.ReplaceNulls(str);
				return str;
			}
			catch(Exception) { bDecodedStringValid = false; }

			return string.Empty;
		}

		private void SetRtbData(string strData, bool bRtf, bool bFixedFont,
			bool bLinkify)
		{
			if(strData == null) { Debug.Assert(false); strData = string.Empty; }

			m_rtbText.Clear(); // Clear formatting (esp. induced by Unicode)

			if(bFixedFont) FontUtil.AssignDefaultMono(m_rtbText, false);
			else FontUtil.AssignDefault(m_rtbText);

			if(bRtf) m_rtbText.Rtf = StrUtil.RtfFix(strData);
			else m_rtbText.Text = strData;

			if(bLinkify) UIUtil.RtfLinkifyUrls(m_rtbText);

			if(!bRtf)
			{
				Font f = (bFixedFont ? FontUtil.MonoFont : FontUtil.DefaultFont);
				if(f != null)
				{
					m_rtbText.SelectAll();
					m_rtbText.SelectionFont = f;
				}
				else { Debug.Assert(false); }
			}

			m_rtbText.Select(0, 0);
		}

		private void UpdateHexView()
		{
			int cbData = (m_bDataExpanded ? m_pbData.Length :
				Math.Min(m_pbData.Length, 16 * 256));

			const int cbGrp = 4;
			const int cbLine = 16;

			int iMaxAddrWidth = Convert.ToString(Math.Max(cbData - 1, 0), 16).Length;

			int cbDataUp = cbData;
			if((cbDataUp % cbLine) != 0)
				cbDataUp = cbDataUp + cbLine - (cbDataUp % cbLine);
			Debug.Assert(((cbDataUp % cbLine) == 0) && (cbDataUp >= cbData));

			StringBuilder sb = new StringBuilder();
			for(int i = 0; i < cbDataUp; ++i)
			{
				if((i % cbLine) == 0)
				{
					sb.Append(Convert.ToString(i, 16).ToUpper().PadLeft(
						iMaxAddrWidth, '0'));
					sb.Append(": ");
				}

				if(i < cbData)
				{
					byte bt = m_pbData[i];
					byte btHigh = (byte)(bt >> 4);
					byte btLow = (byte)(bt & 0x0F);
					if(btHigh >= 10) sb.Append((char)('A' + btHigh - 10));
					else sb.Append((char)('0' + btHigh));
					if(btLow >= 10) sb.Append((char)('A' + btLow - 10));
					else sb.Append((char)('0' + btLow));
				}
				else sb.Append("  ");

				if(((i + 1) % cbGrp) == 0)
					sb.Append(' ');

				if(((i + 1) % cbLine) == 0)
				{
					sb.Append(' ');
					int iStart = i - cbLine + 1;
					int iEndExcl = Math.Min(iStart + cbLine, cbData);
					for(int j = iStart; j < iEndExcl; ++j)
						sb.Append(StrUtil.ByteToSafeChar(m_pbData[j]));
					sb.AppendLine();
				}
			}

			if(cbData < m_pbData.Length) sb.AppendLine(m_strDataExpand);

			SetRtbData(sb.ToString(), false, true, false);

			if(cbData < m_pbData.Length) LinkifyExpandLink();
		}

		private void UpdateTextView()
		{
			bool bValid;
			string strData = BinaryDataToString(true, out bValid);

			bool bRtf = (m_bdc == BinaryDataClass.RichText);

			const int ccInvMax = 1024;
			bool bShorten = (!bValid && !bRtf && !m_bDataExpanded &&
				(strData.Length > ccInvMax));

			if(bShorten)
				strData = strData.Substring(0, ccInvMax) + MessageService.NewLine +
					m_strDataExpand + MessageService.NewLine;

			SetRtbData(strData, bRtf, false, true);

			if(bShorten) LinkifyExpandLink();
		}

		private void LinkifyExpandLink()
		{
			int i = m_rtbText.Text.LastIndexOf(m_strDataExpand);
			if(i < 0) { Debug.Assert(false); return; }

			m_rtbText.Select(i, m_strDataExpand.Length);
			UIUtil.RtfSetSelectionLink(m_rtbText);
			m_rtbText.Select(0, 0);
		}

		private void UpdateVisibility(string strViewer, bool bMakeVisible)
		{
			if(string.IsNullOrEmpty(strViewer)) { Debug.Assert(false); return; }

			if(!bMakeVisible) // Hide all except strViewer
			{
				if((strViewer != m_strViewerHex) && (strViewer != m_strViewerText))
					m_rtbText.Visible = false;
				if(strViewer != m_strViewerImage)
				{
					m_picBox.Visible = false;
					m_pnlImageViewer.Visible = false;
				}
				if(strViewer != m_strViewerWeb)
					m_webBrowser.Visible = false;
			}
			else // Show strViewer
			{
				if((strViewer == m_strViewerHex) || (strViewer == m_strViewerText))
					m_rtbText.Visible = true;
				else if(strViewer == m_strViewerImage)
				{
					m_pnlImageViewer.Visible = true;
					m_picBox.Visible = true;
				}
				else if(strViewer == m_strViewerWeb)
					m_webBrowser.Visible = true;
			}
		}

		private void UpdateDataView()
		{
			if(m_bInitializing) return;

			string strViewer = m_tscViewers.Text;
			bool bText = ((strViewer == m_strViewerText) ||
				(strViewer == m_strViewerWeb));
			bool bImage = (strViewer == m_strViewerImage);

			UpdateVisibility(strViewer, false);
			m_tssSeparator0.Visible = (bText || bImage);
			m_tslEncoding.Visible = m_tscEncoding.Visible = bText;
			m_tslZoom.Visible = m_tscZoom.Visible = m_tsbZoomOut.Visible =
				m_tsbZoomIn.Visible = bImage;

			try
			{
				if(this.DvfUpdating != null)
				{
					DvfContextEventArgs args = new DvfContextEventArgs(this,
						m_pbData, m_strDataDesc, m_tscViewers);
					this.DvfUpdating(this, args);
					if(args.Cancel) return;
				}

				if(strViewer == m_strViewerHex)
					UpdateHexView();
				else if(strViewer == m_strViewerText)
					UpdateTextView();
				else if(strViewer == m_strViewerImage)
				{
					if(m_img == null) m_img = GfxUtil.LoadImage(m_pbData);
					UpdateImageView();
				}
				else if(strViewer == m_strViewerWeb)
				{
					string strData = string.Empty;
					if(m_bdc == BinaryDataClass.WebDocument)
					{
						bool bValid;
						strData = BinaryDataToString(false, out bValid);
					}
					UIUtil.SetWebBrowserDocument(m_webBrowser, strData);
				}
			}
			catch(Exception) { Debug.Assert(strViewer == m_strViewerImage); }

			UpdateVisibility(strViewer, true);
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
			if(string.IsNullOrEmpty(strZoom) || (strZoom == m_strZoomAuto))
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

			if(!strZoom.EndsWith("%")) { Debug.Assert(false); return; }

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

				m_imgResized = GfxUtil.ScaleImage(m_img, dx, dy);
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

			string strRect = UIUtil.GetWindowScreenRect(this);
			if(strRect != m_strInitialFormRect) // Don't overwrite ""
				Program.Config.UI.DataViewerRect = strRect;
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			m_picBox.MouseWheel -= this.OnPicBoxMouseWheel;

			m_picBox.Image = null;
			if(m_img != null) { m_img.Dispose(); m_img = null; }
			if(m_imgResized != null) { m_imgResized.Dispose(); m_imgResized = null; }

			m_ctxText.Detach();
			GlobalWindowManager.RemoveWindow(this);
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			bool bDown;
			if(!NativeMethods.GetKeyMessageState(ref msg, out bDown))
				return base.ProcessCmdKey(ref msg, keyData);

			if(keyData == Keys.Escape) // No modifiers
			{
				if(bDown) Close();
				return true;
			}

			Keys kc = (keyData & Keys.KeyCode);
			bool bCtrl = ((keyData & Keys.Control) != Keys.None);

			if(bCtrl && m_tscZoom.Visible)
			{
				if((kc == Keys.Add) || (kc == Keys.Subtract))
				{
					if(bDown) PerformZoom((kc == Keys.Add) ? 1 : -1);
					return true;
				}
				if((kc == Keys.Oemplus) || (kc == Keys.OemMinus))
				{
					if(bDown) PerformZoom((kc == Keys.Oemplus) ? 1 : -1);
					return true;
				}
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		private void OnZoomSelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateImageView();
		}

		private void PerformZoom(int d)
		{
			if(!m_tscZoom.Visible) { Debug.Assert(false); return; }

			int iCur = m_tscZoom.SelectedIndex, cMax = m_tscZoom.Items.Count;
			if((iCur < 0) || (iCur >= cMax)) { Debug.Assert(false); return; }

			int iAuto = m_tscZoom.Items.IndexOf(m_strZoomAuto);
			if((iAuto < 0) || (iAuto >= cMax)) { Debug.Assert(false); return; }

			if(iCur == iAuto)
			{
				iCur = GetNearestFixedZoomItemIndex();
				if((iCur < 0) || (iCur >= cMax)) { Debug.Assert(false); return; }
			}

			int iNew = Math.Min(Math.Max(iCur + d, 0), cMax - 1);
			if((iNew != iCur) && (iNew != iAuto)) m_tscZoom.SelectedIndex = iNew;
		}

		private void OnPicBoxMouseWheel(object sender, MouseEventArgs e)
		{
			if(e == null) { Debug.Assert(false); return; }
			if((Control.ModifierKeys & Keys.Control) == Keys.None) return;

			int d = e.Delta / 120; // See Control.MouseWheel event
			PerformZoom(d);
		}

		private void OnViewerZoomOut(object sender, EventArgs e)
		{
			PerformZoom(-1);
		}

		private void OnViewerZoomIn(object sender, EventArgs e)
		{
			PerformZoom(1);
		}

		private int GetNearestFixedZoomItemIndex()
		{
			if(m_img == null) { Debug.Assert(false); return -1; }

			int iW = m_img.Width, iH = m_img.Height;
			if((iW <= 0) || (iH <= 0)) { Debug.Assert(false); return -1; }

			int cW = m_picBox.ClientSize.Width, cH = m_picBox.ClientSize.Height;
			if((cW <= 0) || (cH <= 0)) { Debug.Assert(false); return -1; }

			int z = 100;
			if((iW > cW) || (iH > cH))
			{
				int zW = (int)Math.Round(100.0 * (double)cW / (double)iW);
				int zH = (int)Math.Round(100.0 * (double)cH / (double)iH);
				z = Math.Min(zW, zH);
			}

			int dMin = int.MaxValue;
			int iBest = -1;
			for(int i = 0; i < m_tscZoom.Items.Count; ++i)
			{
				string str = (m_tscZoom.Items[i] as string);
				if(string.IsNullOrEmpty(str)) { Debug.Assert(false); continue; }

				if(!str.EndsWith("%")) continue;
				str = str.Substring(0, str.Length - 1);

				int zItem = 0;
				if(!int.TryParse(str, out zItem)) { Debug.Assert(false); continue; }

				int d = Math.Abs(z - zItem);
				if(d < dMin) { iBest = i; dMin = d; }
			}

			return iBest;
		}
	}
}
