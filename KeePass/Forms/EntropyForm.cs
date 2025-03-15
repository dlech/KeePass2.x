/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2025 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

using KeePass.App;
using KeePass.Resources;
using KeePass.UI;

using KeePassLib.Cryptography;
using KeePassLib.Cryptography.PasswordGenerator;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class EntropyForm : Form
	{
		private SHA256Managed m_h = null;
		private float m_fBits = 0;

		private Bitmap m_bmpRandom = null;

		private byte[] m_pbEntropy = null;
		public byte[] GeneratedEntropy
		{
			get { return m_pbEntropy; }
		}

		public static byte[] CollectEntropyIfEnabled(PwProfile pp)
		{
			if(pp == null) { Debug.Assert(false); return null; }
			if(!pp.CollectUserEntropy) return null;

			EntropyForm ef = new EntropyForm();
			if(UIUtil.ShowDialogNotValue(ef, DialogResult.OK)) return null;

			byte[] pb = ef.GeneratedEntropy;
			UIUtil.DestroyForm(ef);
			return pb;
		}

		public EntropyForm()
		{
			InitializeComponent();
			GlobalWindowManager.InitializeForm(this);
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			// Can be invoked by tray command; don't use CenterParent
			Debug.Assert(this.StartPosition == FormStartPosition.CenterScreen);

			GlobalWindowManager.AddWindow(this);

			BannerFactory.CreateBannerEx(this, m_bannerImage,
				Properties.Resources.B48x48_Binary, KPRes.EntropyTitle,
				KPRes.EntropyDesc);
			this.Icon = AppIcons.Default;
			this.Text = KPRes.EntropyTitle;

			m_bmpRandom = CreateRandomBitmap(m_picRandom.ClientSize);
			m_picRandom.Image = m_bmpRandom;

			m_h = new SHA256Managed();

			byte[] pb = Guid.NewGuid().ToByteArray();
			m_h.TransformBlock(pb, 0, pb.Length, pb, 0);

			UpdateUIState();
			UIUtil.SetFocus(m_tbEdit, this);
		}

		private void UpdateUIState()
		{
			int cBits = Math.Min((int)m_fBits, 256); // Max. of SHA-256

			m_pbGenerated.Value = (cBits * 100) / 256;

			Debug.Assert(!m_lblStatus.AutoSize); // For RTL support
			m_lblStatus.Text = KPRes.BitsEx.Replace(@"{PARAM}", cBits.ToString());
		}

		private void OnRandomMouseMove(object sender, MouseEventArgs e)
		{
			if(m_h == null) { Debug.Assert(false); return; }

			byte[] pb = new byte[8 + 4 + 4];
			MemUtil.Int64ToBytesEx(DateTime.UtcNow.ToBinary(), pb, 0);
			MemUtil.Int32ToBytesEx(e.X, pb, 8);
			MemUtil.Int32ToBytesEx(e.Y, pb, 12);
			m_h.TransformBlock(pb, 0, pb.Length, pb, 0);

			m_fBits += 0.125f;

			UpdateUIState();
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			if(m_h == null) { Debug.Assert(false); return; }

			string str = m_tbEdit.Text;
			if(!string.IsNullOrEmpty(str))
			{
				byte[] pbText = StrUtil.Utf8.GetBytes(str);
				m_h.TransformBlock(pbText, 0, pbText.Length, pbText, 0);
			}

			m_h.TransformFinalBlock(MemUtil.EmptyByteArray, 0, 0);

			byte[] pb = m_h.Hash;
			m_pbEntropy = pb;
			CryptoRandom.Instance.AddEntropy(pb);

			m_h.Clear();
			m_h = null;
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			if(m_h != null) // E.g. when clicking [Cancel]
			{
				m_h.Clear();
				m_h = null;
			}

			if(m_bmpRandom != null)
			{
				m_picRandom.Image = null;
				m_bmpRandom.Dispose();
				m_bmpRandom = null;
			}
			else { Debug.Assert(false); }

			GlobalWindowManager.RemoveWindow(this);
		}

		private static Bitmap CreateRandomBitmap(Size sz)
		{
			int w = sz.Width, h = sz.Height;
			if((w <= 0) || (h <= 0)) { Debug.Assert(false); return null; }

			byte[] pbRandom = new byte[w * h];
			Program.GlobalRandom.NextBytes(pbRandom);

			Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);

			Rectangle rect = new Rectangle(0, 0, w, h);
			BitmapData bd = bmp.LockBits(rect, ImageLockMode.WriteOnly,
				PixelFormat.Format32bppArgb);

			bool bFastCopy = (bd.Stride == (w * 4));
			Debug.Assert(bFastCopy); // 32 bits per pixel => no excess in line

			if(bFastCopy)
			{
				byte[] pbBmpData = new byte[w * h * 4];
				int p = 0;
				if(BitConverter.IsLittleEndian)
				{
					for(int i = 0; i < pbBmpData.Length; i += 4)
					{
						byte bt = pbRandom[p++];

						pbBmpData[i] = bt;
						pbBmpData[i + 1] = bt;
						pbBmpData[i + 2] = bt;
						pbBmpData[i + 3] = 255;
					}
				}
				else // Big-endian
				{
					for(int i = 0; i < pbBmpData.Length; i += 4)
					{
						byte bt = pbRandom[p++];

						pbBmpData[i] = 255;
						pbBmpData[i + 1] = bt;
						pbBmpData[i + 2] = bt;
						pbBmpData[i + 3] = bt;
					}
				}
				Debug.Assert(p == (w * h));

				Marshal.Copy(pbBmpData, 0, bd.Scan0, pbBmpData.Length);
			}

			bmp.UnlockBits(bd);

			if(!bFastCopy)
			{
				int p = 0;
				for(int y = 0; y < h; ++y)
				{
					for(int x = 0; x < w; ++x)
					{
						int c = pbRandom[p++];
						bmp.SetPixel(x, y, Color.FromArgb(255, c, c, c));
					}
				}
			}

			return bmp;
		}
	}
}
