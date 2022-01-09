/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2022 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using KeePass.App;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util.MultipleValues;

using KeePassLib;
using KeePassLib.Utility;

namespace KeePass.Forms
{
	public partial class IconPickerForm : Form
	{
		private ImageList m_ilIcons = null;
		private uint m_uNumberOfStandardIcons = 0;
		private PwDatabase m_pd = null;
		private uint m_uDefaultIcon = 0;
		private PwUuid m_puDefaultCustomIcon = PwUuid.Zero;

		private uint m_uChosenIcon = 0;
		private PwUuid m_puChosenCustomIcon = PwUuid.Zero;

		public uint ChosenIconId
		{
			get { return m_uChosenIcon; }
		}

		public PwUuid ChosenCustomIconUuid
		{
			get { return m_puChosenCustomIcon; }
		}

		public IconPickerForm()
		{
			InitializeComponent();
			GlobalWindowManager.InitializeForm(this);
		}

		public void InitEx(ImageList ilIcons, uint uNumberOfStandardIcons,
			PwDatabase pd, uint uDefaultIcon, PwUuid puDefaultCustomIcon)
		{
			m_ilIcons = ilIcons;
			m_uNumberOfStandardIcons = uNumberOfStandardIcons;
			m_pd = pd;
			m_uDefaultIcon = uDefaultIcon;
			m_puDefaultCustomIcon = puDefaultCustomIcon;
		}

		private void OnFormLoad(object sender, EventArgs e)
		{
			if(m_ilIcons == null) { Debug.Assert(false); throw new InvalidOperationException(); }
			if(m_pd == null) { Debug.Assert(false); throw new InvalidOperationException(); }

			GlobalWindowManager.AddWindow(this);

			this.Icon = AppIcons.Default;

			FontUtil.AssignDefaultBold(m_radioStandard);
			FontUtil.AssignDefaultBold(m_radioCustom);

			// With the Explorer theme, item selection rectangles are larger
			// and thus (gray/inactive) selections are easier to see
			UIUtil.SetExplorerTheme(m_lvIcons, false);
			UIUtil.SetExplorerTheme(m_lvCustomIcons, false);

			if(m_ilIcons.Images.Count < (long)m_uNumberOfStandardIcons)
			{
				Debug.Assert(false);
				m_uNumberOfStandardIcons = (uint)m_ilIcons.Images.Count;
			}

			m_lvIcons.BeginUpdate();
			m_lvIcons.SmallImageList = m_ilIcons;
			for(uint i = 0; i < m_uNumberOfStandardIcons; ++i)
				m_lvIcons.Items.Add(i.ToString(), (int)i);
			m_lvIcons.EndUpdate();

			if(m_uDefaultIcon < m_uNumberOfStandardIcons)
			{
				m_lvIcons.EnsureVisible((int)m_uDefaultIcon);
				UIUtil.SetFocusedItem(m_lvIcons, m_lvIcons.Items[
					(int)m_uDefaultIcon], true);
			}
			else { Debug.Assert(false); }

			RecreateCustomIconList(m_puDefaultCustomIcon);

			if(!m_puDefaultCustomIcon.Equals(PwUuid.Zero))
			{
				m_radioCustom.Checked = true;
				UIUtil.SetFocus(m_lvCustomIcons, this);
			}
			else
			{
				m_radioStandard.Checked = true;
				UIUtil.SetFocus(m_lvIcons, this);
			}

			EnableControlsEx();
		}

		private void EnableControlsEx()
		{
			int cCustom = m_lvCustomIcons.Items.Count;
			int cSelStd = m_lvIcons.SelectedIndices.Count;
			int cSelCustom = m_lvCustomIcons.SelectedIndices.Count;

			if(m_radioStandard.Checked && (cSelStd == 1))
				m_btnOK.Enabled = true;
			else if(m_radioCustom.Checked && (cSelCustom == 1))
				m_btnOK.Enabled = true;
			else m_btnOK.Enabled = false;

			m_btnCustomDelete.Enabled = (cSelCustom >= 1);
			m_btnCustomExport.Enabled = (cSelCustom >= 1);

			UIUtil.SetEnabledFast((cCustom != 0), m_lblFind, m_tbFind);

			// if(m_bBlockCancel)
			// {
			//	m_btnCancel.Enabled = false;
			//	if(this.ControlBox) this.ControlBox = false;
			// }
		}

		private void SelectCustomIcon(PwUuid pu)
		{
			if(pu.Equals(PwUuid.Zero)) return;

			foreach(ListViewItem lvi in m_lvCustomIcons.Items)
			{
				PwCustomIcon ci = (lvi.Tag as PwCustomIcon);
				if(ci == null) { Debug.Assert(false); continue; }

				if(ci.Uuid.Equals(pu))
				{
					m_lvCustomIcons.BeginUpdate(); // Avoid animation from left
					lvi.EnsureVisible();
					UIUtil.SetFocusedItem(m_lvCustomIcons, lvi, true);
					m_lvCustomIcons.EndUpdate();
					return;
				}
			}

			Debug.Assert(false);
		}

		private void RecreateCustomIconList(PwUuid puSelect)
		{
			StringBuilder sb = new StringBuilder();

			List<ListViewItem> lUnnamed = new List<ListViewItem>();
			List<ListViewItem> lNamed = new List<ListViewItem>();

			for(int i = 0; i < m_pd.CustomIcons.Count; ++i)
			{
				PwCustomIcon ci = m_pd.CustomIcons[i];
				bool bMulti = (ci.Name == MultipleValuesEx.CueString);

				ListViewItem lvi = new ListViewItem();
				if(ci.Name.Length == 0)
				{
					lvi.Text = lUnnamed.Count.ToString();
					lUnnamed.Add(lvi);
				}
				else
				{
					lvi.Text = ci.Name;
					if(bMulti) lUnnamed.Insert(0, lvi);
					else lNamed.Add(lvi);
				}

				lvi.ImageIndex = i;
				lvi.Tag = ci;

				Image img = ci.GetImage();
				if(bMulti)
					lvi.ToolTipText = ci.Name;
				else if(img != null)
				{
					if(sb.Length != 0) sb.Remove(0, sb.Length);

					if(ci.Name.Length != 0) sb.AppendLine(ci.Name);
					sb.Append(img.Width);
					sb.Append(" \u00D7 ");
					sb.Append(img.Height);
					sb.AppendLine(" px");
					sb.Append(StrUtil.FormatDataSizeKB((ulong)ci.ImageDataPng.Length));
#if DEBUG
					if(ci.LastModificationTime.HasValue)
					{
						sb.AppendLine();
						sb.Append("((( "); // Debug indicator
						sb.Append(TimeUtil.ToDisplayString(ci.LastModificationTime.Value));
						sb.Append(" )))"); // Debug indicator
					}
#endif

					lvi.ToolTipText = sb.ToString();
				}
			}

			Comparison<ListViewItem> f = delegate(ListViewItem x, ListViewItem y)
			{
				string strX = (((x != null) ? x.Text : null) ?? string.Empty);
				string strY = (((y != null) ? y.Text : null) ?? string.Empty);
				return StrUtil.CompareNaturally(strX, strY);
			};
			lNamed.Sort(f);

			List<ListViewItem> lAll = new List<ListViewItem>(lUnnamed.Count + lNamed.Count);
			lAll.AddRange(lUnnamed);
			lAll.AddRange(lNamed);

			ImageList ilCustom = UIUtil.BuildImageList(m_pd.CustomIcons,
				DpiUtil.ScaleIntX(16), DpiUtil.ScaleIntY(16));

			m_lvCustomIcons.BeginUpdate();
			m_lvCustomIcons.Items.Clear();

			m_lvCustomIcons.SmallImageList = ilCustom;
			m_lvCustomIcons.Items.AddRange(lAll.ToArray());

			m_lvCustomIcons.EndUpdate();

			SelectCustomIcon(puSelect); // Doesn't always work before EndUpdate()
		}

		private void OnBtnOK(object sender, EventArgs e)
		{
			if(!SaveChosenIcon())
			{
				this.DialogResult = DialogResult.None;
				MessageService.ShowWarning(KPRes.PickIcon);
			}
		}

		private bool SaveChosenIcon()
		{
			ListView.SelectedIndexCollection lvsicS = m_lvIcons.SelectedIndices;
			int cS = lvsicS.Count;
			m_uChosenIcon = ((cS > 0) ? (uint)lvsicS[0] : m_uDefaultIcon);

			if(m_radioStandard.Checked && (cS != 1)) return false;

			if(m_radioCustom.Checked)
			{
				ListView.SelectedListViewItemCollection lvsicC = m_lvCustomIcons.SelectedItems;
				if(lvsicC.Count != 1) return false;

				PwCustomIcon ci = (lvsicC[0].Tag as PwCustomIcon);
				if(ci == null) { Debug.Assert(false); return false; }

				m_puChosenCustomIcon = ci.Uuid;
			}
			else m_puChosenCustomIcon = PwUuid.Zero;

			return true;
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
			// Detach event handlers
			m_lvIcons.SmallImageList = null;
			m_lvCustomIcons.SmallImageList = null;

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

			OpenFileDialogEx ofd = UIUtil.CreateOpenFileDialog(KPRes.ImportFileTitle,
				sbFilter.ToString(), 1, null, true, AppDefs.FileDialogContext.Import);
			if(ofd.ShowDialog() != DialogResult.OK) return;

			// Predicate<PwCustomIcon> fRequiresKdbx4p1 = delegate(PwCustomIcon ci)
			// {
			//	return ((ci.Name.Length != 0) || ci.LastModificationTime.HasValue);
			// };
			// bool bUseFileName = m_pd.CustomIcons.Exists(fRequiresKdbx4p1);

			PwUuid puSelect = PwUuid.Zero;
			foreach(string strFile in ofd.FileNames)
			{
				MemoryStream msPng = new MemoryStream();
				string strError = null;

				try
				{
					byte[] pb = File.ReadAllBytes(strFile);

					using(Image img = GfxUtil.LoadImage(pb))
					{
						if(img == null) throw new FormatException();

						const int wMax = PwCustomIcon.MaxWidth;
						const int hMax = PwCustomIcon.MaxHeight;

						if((img.Width <= wMax) && (img.Height <= hMax))
							img.Save(msPng, ImageFormat.Png);
						else
						{
							using(Image imgSc = GfxUtil.ScaleImage(img, wMax, hMax))
							{
								imgSc.Save(msPng, ImageFormat.Png);
							}
						}
					}

					PwUuid pu = new PwUuid(true);
					PwCustomIcon ci = new PwCustomIcon(pu, msPng.ToArray());

					// if(bUseFileName)
					// {
					//	string strName = UrlUtil.StripExtension(
					//		UrlUtil.GetFileName(strFile));
					//	if(!string.IsNullOrEmpty(strName)) ci.Name = strName;
					// }

					m_pd.CustomIcons.Add(ci);
					m_pd.UINeedsIconUpdate = true;
					m_pd.Modified = true;

					if(puSelect.Equals(PwUuid.Zero)) puSelect = pu;
				}
				catch(ArgumentException)
				{
					strError = KPRes.ImageFormatFeatureUnsupported;
				}
				catch(System.Runtime.InteropServices.ExternalException)
				{
					strError = KPRes.ImageFormatFeatureUnsupported;
				}
				catch(Exception ex)
				{
					strError = ex.Message;
				}
				finally { msPng.Close(); }

				if(!string.IsNullOrEmpty(strError))
					MessageService.ShowWarning(strFile, strError);
			}

			RecreateCustomIconList(puSelect);
			EnableControlsEx();
			UIUtil.SetFocus(m_lvCustomIcons, this);
		}

		private static void AddFileType(StringBuilder sbBuffer, string strEnding,
			string strName)
		{
			if(sbBuffer.Length > 0) sbBuffer.Append('|');
			sbBuffer.Append(strName);
			sbBuffer.Append('|');
			sbBuffer.Append(strEnding);
		}

		private void OnBtnCustomRemove(object sender, EventArgs e)
		{
			ListView.SelectedListViewItemCollection lvsic = m_lvCustomIcons.SelectedItems;
			if(lvsic.Count == 0) { Debug.Assert(false); return; }

			List<PwUuid> lDel = new List<PwUuid>();
			foreach(ListViewItem lvi in lvsic)
			{
				PwCustomIcon ci = (lvi.Tag as PwCustomIcon);
				if(ci != null) lDel.Add(ci.Uuid);
				else { Debug.Assert(false); }
			}

			m_pd.DeleteCustomIcons(lDel);
			m_pd.UINeedsIconUpdate = true;
			m_pd.Modified = true;

			RecreateCustomIconList(PwUuid.Zero);
			EnableControlsEx();
		}

		private void OnIconsItemActivate(object sender, EventArgs e)
		{
			OnIconsItemSelectionChanged(sender, null);
			if(!SaveChosenIcon()) return;
			this.DialogResult = DialogResult.OK;
		}

		private void OnCustomIconsItemActivate(object sender, EventArgs e)
		{
			OnCustomIconsItemSelectionChanged(sender, null);
			if(!SaveChosenIcon()) return;
			this.DialogResult = DialogResult.OK;
		}

		private void OnBtnCustomSave(object sender, EventArgs e)
		{
			ListView.SelectedListViewItemCollection lvsic = m_lvCustomIcons.SelectedItems;
			if(lvsic.Count == 0) return;

			if(lvsic.Count == 1)
			{
				PwCustomIcon ci = (lvsic[0].Tag as PwCustomIcon);
				if(ci == null) { Debug.Assert(false); return; }

				string strName = KPRes.Export;
				if(ci.Name.Length != 0) strName = ci.Name;
				strName = UrlUtil.FilterFileName(strName);

				StringBuilder sbFilter = new StringBuilder();
				AddFileType(sbFilter, "*.png", "Portable Network Graphics (*.png)");
				// AddFileType(sbFilter, "*.ico", "Windows Icon (*.ico)");
				sbFilter.Append(@"|" + KPRes.AllFiles + @" (*.*)|*.*");

				SaveFileDialogEx sfd = UIUtil.CreateSaveFileDialog(KPRes.ExportFileTitle,
					strName + ".png", sbFilter.ToString(), 1, null,
					AppDefs.FileDialogContext.Export);
				if(sfd.ShowDialog() == DialogResult.OK)
					SaveImageFile(ci, sfd.FileName);
			}
			else // lvsic.Count >= 2
			{
				FolderBrowserDialog fbd = UIUtil.CreateFolderBrowserDialog(KPRes.ExportToPrompt);
				if(fbd.ShowDialog() != DialogResult.OK) return;

				string strDir = UrlUtil.EnsureTerminatingSeparator(
					fbd.SelectedPath, false);
				Dictionary<string, int> dStart = new Dictionary<string, int>();

				foreach(ListViewItem lvi in lvsic)
				{
					try
					{
						PwCustomIcon ci = (lvi.Tag as PwCustomIcon);
						if(ci == null) { Debug.Assert(false); continue; }

						string strName = KPRes.Export;
						if(ci.Name.Length != 0) strName = ci.Name;
						strName = UrlUtil.FilterFileName(strName);

						int iStart;
						dStart.TryGetValue(strName, out iStart);

						for(int i = iStart; i < int.MaxValue; ++i)
						{
							string strFile = strDir + strName + ((i == 0) ?
								string.Empty : (" (" + i.ToString() + ")")) +
								".png";

							if(!File.Exists(strFile))
							{
								SaveImageFile(ci, strFile);
								dStart[strName] = i + 1;
								break;
							}
						}
					}
					catch(Exception ex) { MessageService.ShowWarning(ex); }
				}
			}
		}

		private static void SaveImageFile(PwCustomIcon ci, string strFile)
		{
			if((ci == null) || string.IsNullOrEmpty(strFile)) { Debug.Assert(false); return; }

			try
			{
				Image img = ci.GetImage();
				if(img == null) { Debug.Assert(false); return; }

				// string strExt = UrlUtil.GetExtension(strFile);
				ImageFormat fmt = ImageFormat.Png;
				// if(strExt.Equals("ico", StrUtil.CaseIgnoreCmp)) fmt = ImageFormat.Icon;

				img.Save(strFile, fmt);
			}
			catch(Exception ex) { MessageService.ShowWarning(strFile, ex); }
		}

		private void OnCustomIconsBeforeLabelEdit(object sender, LabelEditEventArgs e)
		{
			PwCustomIcon ci = (m_lvCustomIcons.Items[e.Item].Tag as PwCustomIcon);
			if(ci == null) { Debug.Assert(false); e.CancelEdit = true; return; }

			if(ci.Name == MultipleValuesEx.CueString)
				e.CancelEdit = true;
		}

		private void OnCustomIconsAfterLabelEdit(object sender, LabelEditEventArgs e)
		{
			string strNew = e.Label;
			int iItem = e.Item;
			e.CancelEdit = true;

			if(strNew == null) return; // Edit aborted (Esc)
			if(strNew == MultipleValuesEx.CueString) return;
			if(Regex.IsMatch(strNew, "^\\d+$")) strNew = string.Empty;

			PwCustomIcon ci = (m_lvCustomIcons.Items[iItem].Tag as PwCustomIcon);
			if(ci == null) { Debug.Assert(false); return; }

			if(ci.Name != strNew)
			{
				ci.Name = strNew;
				ci.LastModificationTime = DateTime.UtcNow;

				m_pd.UINeedsIconUpdate = true;
				m_pd.Modified = true;
			}

			RecreateCustomIconList(ci.Uuid);
			EnableControlsEx();
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if(((keyData & Keys.KeyCode) == Keys.Return) && m_tbFind.Focused)
			{
				msg.Result = IntPtr.Zero;

				string strFind = m_tbFind.Text;
				if(string.IsNullOrEmpty(strFind)) return true;

				bool bFwd = ((keyData & Keys.Shift) == Keys.None);
				int n = m_lvCustomIcons.Items.Count;
				int iStart = (bFwd ? 0 : (n - 1));

				ListView.SelectedIndexCollection lvsic = m_lvCustomIcons.SelectedIndices;
				if(lvsic.Count != 0)
				{
					iStart = lvsic[0] + (bFwd ? 1 : -1);
					UIUtil.DeselectAllItems(m_lvCustomIcons);
				}

				for(int i = 0; i < n; ++i)
				{
					int j = (bFwd ? (iStart + i) : (iStart - i + n)) % n;
					ListViewItem lvi = m_lvCustomIcons.Items[j];

					string strText = lvi.Text;
					if(strText.IndexOf(strFind, StrUtil.CaseIgnoreCmp) >= 0)
					{
						lvi.EnsureVisible();
						UIUtil.SetFocusedItem(m_lvCustomIcons, lvi, true);
						break;
					}
				}

				return true;
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}
	}
}
