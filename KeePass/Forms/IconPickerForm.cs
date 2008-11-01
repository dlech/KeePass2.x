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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

using KeePass.Resources;
using KeePass.UI;

using KeePassLib;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class IconPickerForm : Form
	{
		private ImageList m_ilIcons = null;
		private uint m_uNumberOfStandardIcons = 0;
		private PwDatabase m_pwDatabase = null;
		private uint m_uDefaultIcon = 0;
		private PwUuid m_pwDefaultCustomIcon = PwUuid.Zero;

		private uint m_uChosenImageID = 0;
		private PwUuid m_pwChosenCustomImageUuid = PwUuid.Zero;

		private bool m_bBlockCancel = false;

		public uint ChosenIconId
		{
			get { return m_uChosenImageID; }
		}

		public PwUuid ChosenCustomIconUuid
		{
			get { return m_pwChosenCustomImageUuid; }
		}

		public IconPickerForm()
		{
			InitializeComponent();
			Program.Translation.ApplyTo(this);
		}

		public void InitEx(ImageList ilIcons, uint uNumberOfStandardIcons,
			PwDatabase pwDatabase, uint uDefaultIcon, PwUuid pwCustomIconUuid)
		{
			m_ilIcons = ilIcons;
			m_uNumberOfStandardIcons = uNumberOfStandardIcons;
			m_pwDatabase = pwDatabase;
			m_uDefaultIcon = uDefaultIcon;
			m_pwDefaultCustomIcon = pwCustomIconUuid;
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			Debug.Assert(m_ilIcons != null); if(m_ilIcons == null) throw new InvalidOperationException();
			Debug.Assert(m_pwDatabase != null); if(m_pwDatabase == null) throw new InvalidOperationException();

			GlobalWindowManager.AddWindow(this);

			this.Icon = Properties.Resources.KeePass;

			m_lvIcons.SmallImageList = m_ilIcons;
			for(uint i = 0; i < m_uNumberOfStandardIcons; ++i)
				m_lvIcons.Items.Add(i.ToString(), (int)i);

			int iFoundCustom = RecreateCustomIconList();

			if((m_pwDefaultCustomIcon != PwUuid.Zero) && (iFoundCustom >= 0))
			{
				m_radioCustom.Checked = true;
				m_lvCustomIcons.Items[iFoundCustom].Selected = true;
			}
			else if(m_uDefaultIcon < m_uNumberOfStandardIcons)
			{
				m_radioStandard.Checked = true;
				m_lvIcons.Items[(int)m_uDefaultIcon].Selected = true;
			}
			else
			{
				Debug.Assert(false);
				m_radioStandard.Checked = true;
			}

			EnableControlsEx();
		}

		private void EnableControlsEx()
		{
			ListView.SelectedIndexCollection lvsic = m_lvCustomIcons.SelectedIndices;

			if(m_radioStandard.Checked && (m_lvIcons.SelectedIndices.Count == 1))
				m_btnOK.Enabled = true;
			else if(m_radioCustom.Checked && (lvsic.Count == 1))
				m_btnOK.Enabled = true;
			else
				m_btnOK.Enabled = false;

			m_btnCustomRemove.Enabled = (lvsic.Count >= 1);

			if(m_bBlockCancel) m_btnCancel.Enabled = false;
		}

		private int RecreateCustomIconList()
		{
			m_lvCustomIcons.Items.Clear();

			ImageList ilCustom = UIUtil.BuildImageList(m_pwDatabase.CustomIcons, 16, 16);
			m_lvCustomIcons.SmallImageList = ilCustom;

			int j = 0, iFoundCustom = -1;
			foreach(PwCustomIcon pwci in m_pwDatabase.CustomIcons)
			{
				ListViewItem lvi = m_lvCustomIcons.Items.Add(j.ToString(), j);
				lvi.Tag = pwci.Uuid;

				if(pwci.Uuid.EqualsValue(m_pwDefaultCustomIcon))
					iFoundCustom = j;

				++j;
			}

			return iFoundCustom;
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			if(m_radioStandard.Checked)
			{
				ListView.SelectedIndexCollection lvsi = m_lvIcons.SelectedIndices;

				if(lvsi.Count != 1)
				{
					this.DialogResult = DialogResult.None;
					return;
				}

				m_uChosenImageID = (uint)lvsi[0];
			}
			else // Custom icon
			{
				ListView.SelectedListViewItemCollection lvsic = m_lvCustomIcons.SelectedItems;

				if(lvsic.Count != 1)
				{
					this.DialogResult = DialogResult.None;
					return;
				}

				m_pwChosenCustomImageUuid = (PwUuid)lvsic[0].Tag;
			}
		}

		private void OnBtnCancel(object sender, EventArgs e)
		{
			if(m_bBlockCancel) this.DialogResult = DialogResult.None;
		}

		private void OnIconsItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			m_radioCustom.Checked = false;
			m_radioStandard.Checked = true;
			EnableControlsEx();
		}

		private void OnCustomIconsItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			m_radioStandard.Checked = false;
			m_radioCustom.Checked = true;
			EnableControlsEx();
		}

		private void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			GlobalWindowManager.RemoveWindow(this);
		}

		private void OnStandardRadioCheckedChanged(object sender, EventArgs e)
		{
			EnableControlsEx();
		}

		private void OnBtnCustomAdd(object sender, EventArgs e)
		{
			string strAllSupportedFilter = KPRes.AllSupportedFiles +
				@" (*.bmp; *.emf; *.gif; *.ico; *.jpg; *.jpe; *.jpeg; *.jfif; *.jfi; *.jif; *.png; *.tif; *.tiff; *.wmf)" +
				@"|*.bmp;*.emf;*.gif;*.ico;*.jpg;*.jpe;*.jpeg;*.jfif;*.jfi;*.jif;*.png;*.tif;*.tiff;*.wmf";
			StringBuilder sbFilter = new StringBuilder();
			sbFilter.Append(strAllSupportedFilter);
			AddFileType(sbFilter, "*.bmp", "Windows Bitmap (*.bmp)");
			AddFileType(sbFilter, "*.emf", "Windows Enhanced Metafile (*.emf)");
			AddFileType(sbFilter, "*.gif", "Graphics Interchange Format (*.gif)");
			AddFileType(sbFilter, "*.ico", "Windows Icon (*.ico)");
			AddFileType(sbFilter, "*.jpg;*.jpe;*.jpeg;*.jfif;*.jfi;*.jif", "JPEG (*.jpg; *.jpe; *.jpeg; *.jfif; *.jfi; *.jif)");
			AddFileType(sbFilter, "*.png", "Portable Network Graphics (*.png)");
			AddFileType(sbFilter, "*.tif;*.tiff", "Tagged Image File Format (*.tif; *.tiff)");
			AddFileType(sbFilter, "*.wmf", "Windows Metafile (*.wmf)");
			sbFilter.Append(@"|" + KPRes.AllFiles + @" (*.*)|*.*");

			OpenFileDialog ofd = UIUtil.CreateOpenFileDialog(KPRes.ImportFileTitle,
				sbFilter.ToString(), 1, null, true, true);

			if(ofd.ShowDialog() == DialogResult.OK)
			{
				foreach(string strFile in ofd.FileNames)
				{
					bool bUnsupportedFormat = false;

					try
					{
						if(File.Exists(strFile) == false)
							throw new FileNotFoundException();

						// Image img = Image.FromFile(strFile);
						// Image img = Image.FromFile(strFile, false);
						// Image img = Bitmap.FromFile(strFile);
						// Bitmap img = new Bitmap(strFile);
						// Image img = Image.FromFile(strFile);
						byte[] pb = File.ReadAllBytes(strFile);
						MemoryStream msSource = new MemoryStream(pb, false);
						Image img = Image.FromStream(msSource);
						msSource.Close();

						Image imgNew = new Bitmap(img, new Size(16, 16));

						MemoryStream ms = new MemoryStream();
						imgNew.Save(ms, ImageFormat.Png);

						PwCustomIcon pwci = new PwCustomIcon(new PwUuid(true),
							ms.ToArray());
						m_pwDatabase.CustomIcons.Add(pwci);

						m_pwDatabase.UINeedsIconUpdate = true;
					}
					catch(ArgumentException)
					{
						bUnsupportedFormat = true;
					}
					catch(System.Runtime.InteropServices.ExternalException)
					{
						bUnsupportedFormat = true;
					}
					catch(Exception exImg)
					{
						MessageService.ShowWarning(strFile, exImg);
					}

					if(bUnsupportedFormat)
						MessageService.ShowWarning(strFile, KPRes.ImageFormatFeatureUnsupported);
				}

				RecreateCustomIconList();
			}

			EnableControlsEx();
		}

		private static void AddFileType(StringBuilder sbBuffer, string strEnding,
			string strName)
		{
			sbBuffer.Append('|');
			sbBuffer.Append(strName);
			sbBuffer.Append('|');
			sbBuffer.Append(strEnding);
		}

		private void OnBtnCustomRemove(object sender, EventArgs e)
		{
			ListView.SelectedListViewItemCollection lvsicSel = m_lvCustomIcons.SelectedItems;
			List<PwUuid> vUuidsToDelete = new List<PwUuid>();

			foreach(ListViewItem lvi in lvsicSel)
			{
				PwUuid uuidIcon = lvi.Tag as PwUuid;

				Debug.Assert(uuidIcon != null);
				if(uuidIcon != null) vUuidsToDelete.Add(uuidIcon);
			}

			m_pwDatabase.DeleteCustomIcons(vUuidsToDelete);

			if(vUuidsToDelete.Count > 0)
			{
				m_bBlockCancel = true;

				m_pwDatabase.UINeedsIconUpdate = true;
			}

			RecreateCustomIconList();
			EnableControlsEx();
		}
	}
}