/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2014 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

using KeePass.Resources;

using KeePassLib;
using KeePassLib.Serialization;
using KeePassLib.Utility;

namespace KeePass.UI
{
	/// <summary>
	/// MRU handler interface. An MRU handler must support executing an MRU
	/// item and clearing the MRU list.
	/// </summary>
	public interface IMruExecuteHandler
	{
		/// <summary>
		/// Function that is called when an MRU item is executed (i.e. when
		/// the user has clicked the menu item).
		/// </summary>
		void OnMruExecute(string strDisplayName, object oTag);

		/// <summary>
		/// Secondary MRU item click handler.
		/// </summary>
		void OnMruExecute2(string strDisplayName, object oTag);

		/// <summary>
		/// Function to clear the MRU (for example all menu items must be
		/// removed from the menu).
		/// </summary>
		void OnMruClear();
	}

	public sealed class MruList
	{
		private IMruExecuteHandler m_handler = null;
		private ToolStripMenuItem m_tsmiContainer = null;
		private ToolStripMenuItem m_tsmiContainer2 = null;

		private List<KeyValuePair<string, object>> m_vItems =
			new List<KeyValuePair<string, object>>();

		// private Font m_fItalic = null;

		private uint m_uMaxItemCount = 0;
		public uint MaxItemCount
		{
			get { return m_uMaxItemCount; }
			set { m_uMaxItemCount = value; }
		}

		private bool m_bMarkOpened = false;
		public bool MarkOpened
		{
			get { return m_bMarkOpened; }
			set { m_bMarkOpened = value; }
		}

		public uint ItemCount
		{
			get { return (uint)m_vItems.Count; }
		}

		public bool IsValid
		{
			get { return (m_handler != null); }
		}

		public MruList()
		{
		}

		public void Initialize(IMruExecuteHandler handler, ToolStripMenuItem tsmiContainer,
			ToolStripMenuItem tsmiContainer2)
		{
			Release();

			Debug.Assert(handler != null); // No throw
			Debug.Assert(tsmiContainer != null); // No throw
			// Debug.Assert(tsmiContainer2 != null); // Is optional

			m_handler = handler;
			m_tsmiContainer = tsmiContainer;
			m_tsmiContainer2 = tsmiContainer2;

			if(m_tsmiContainer != null)
				m_tsmiContainer.DropDownOpening += this.OnDropDownOpening;
			if(m_tsmiContainer2 != null)
				m_tsmiContainer2.DropDownOpening += this.OnDropDownOpening;
		}

		public void Release()
		{
			if(m_tsmiContainer != null)
				m_tsmiContainer.DropDownOpening -= this.OnDropDownOpening;
			if(m_tsmiContainer2 != null)
				m_tsmiContainer2.DropDownOpening -= this.OnDropDownOpening;

			m_handler = null;
			m_tsmiContainer = null;
			m_tsmiContainer2 = null;
		}

		public void Clear()
		{
			m_vItems.Clear();
		}

		public void AddItem(string strDisplayName, object oTag, bool bUpdateMenu)
		{
			Debug.Assert(strDisplayName != null);
			if(strDisplayName == null) throw new ArgumentNullException("strDisplayName");
			Debug.Assert(oTag != null);
			if(oTag == null) throw new ArgumentNullException("oTag");

			bool bExists = false;
			foreach(KeyValuePair<string, object> kvp in m_vItems)
			{
				Debug.Assert(kvp.Key != null);
				if(kvp.Key.Equals(strDisplayName, StrUtil.CaseIgnoreCmp))
				{
					bExists = true;
					break;
				}
			}

			if(bExists) MoveItemToTop(strDisplayName);
			else
			{
				m_vItems.Insert(0, new KeyValuePair<string, object>(strDisplayName, oTag));

				if(m_vItems.Count > m_uMaxItemCount)
					m_vItems.RemoveAt(m_vItems.Count - 1);
			}

			if(bUpdateMenu) UpdateMenu();
		}

		public void UpdateMenu()
		{
			if(m_tsmiContainer == null) return;

			m_tsmiContainer.DropDownItems.Clear();
			if(m_tsmiContainer2 != null) m_tsmiContainer2.DropDownItems.Clear();

			uint uAccessKey = 1, uNull = 0;
			if(m_vItems.Count > 0)
			{
				foreach(KeyValuePair<string, object> kvp in m_vItems)
					AddMenuItem(StrUtil.EncodeMenuText(kvp.Key), kvp.Value,
						false, null, true, ref uAccessKey);

				m_tsmiContainer.DropDownItems.Add(new ToolStripSeparator());
				if(m_tsmiContainer2 != null)
					m_tsmiContainer2.DropDownItems.Add(new ToolStripSeparator());

				AddMenuItem(KPRes.ClearMru, null, true,
					Properties.Resources.B16x16_EditDelete, true, ref uNull);
			}
			else AddMenuItem("(" + KPRes.Empty + ")", null, null, null, false, ref uNull);
		}

		private void AddMenuItem(string strText, object oTag, bool? bClearHandler,
			Image img, bool bEnabled, ref uint uAccessKey)
		{
			if(m_tsmiContainer != null)
				m_tsmiContainer.DropDownItems.Add(CreateMenuItem(strText, oTag,
					bClearHandler, img, bEnabled, uAccessKey, false));
			if(m_tsmiContainer2 != null)
				m_tsmiContainer2.DropDownItems.Add(CreateMenuItem(strText, oTag,
					bClearHandler, img, bEnabled, uAccessKey, true));

			if(uAccessKey != 0) ++uAccessKey;
		}

		private ToolStripMenuItem CreateMenuItem(string strText, object oTag,
			bool? bClearHandler, Image img, bool bEnabled, uint uAccessKey,
			bool b2)
		{
			string strItem = strText;
			if(uAccessKey >= 1)
			{
				if(uAccessKey < 10)
					strItem = @"&" + uAccessKey.ToString() + " " + strItem;
				else if(uAccessKey == 10)
					strItem = @"1&0 " + strItem;
				else strItem = uAccessKey.ToString() + " " + strItem;
			}

			ToolStripMenuItem tsi = new ToolStripMenuItem(strItem);
			if(oTag != null) tsi.Tag = oTag;
			if(img != null) tsi.Image = img;

			IOConnectionInfo ioc = (oTag as IOConnectionInfo);
			if(m_bMarkOpened && (ioc != null) && (Program.MainForm != null))
			{
				foreach(PwDatabase pd in Program.MainForm.DocumentManager.GetOpenDatabases())
				{
					if(pd.IOConnectionInfo.GetDisplayName().Equals(
						ioc.GetDisplayName(), StrUtil.CaseIgnoreCmp))
					{
						// if(m_fItalic == null)
						// {
						//	Font f = tsi.Font;
						//	if(f != null)
						//		m_fItalic = FontUtil.CreateFont(f, FontStyle.Italic);
						//	else { Debug.Assert(false); }
						// }

						// if(m_fItalic != null) tsi.Font = m_fItalic;
						// 153, 51, 153
						tsi.ForeColor = Color.FromArgb(64, 64, 255);
						tsi.Text += " [" + KPRes.Opened + "]";
						break;
					}
				}
			}

			if(bClearHandler.HasValue)
				tsi.Click += (bClearHandler.Value ? new EventHandler(this.ClearHandler) :
					(b2 ? new EventHandler(this.ClickedHandler2) :
					new EventHandler(this.ClickedHandler)));

			if(bEnabled == false) tsi.Enabled = false;
			return tsi;
		}

		public KeyValuePair<string, object> GetItem(uint uIndex)
		{
			Debug.Assert(uIndex < (uint)m_vItems.Count);
			if(uIndex >= (uint)m_vItems.Count) throw new ArgumentException();

			return m_vItems[(int)uIndex];
		}

		public bool RemoveItem(string strDisplayName)
		{
			Debug.Assert(strDisplayName != null);
			if(strDisplayName == null) throw new ArgumentNullException("strDisplayName");

			for(int i = 0; i < m_vItems.Count; ++i)
			{
				KeyValuePair<string, object> kvp = m_vItems[i];
				if(kvp.Key.Equals(strDisplayName, StrUtil.CaseIgnoreCmp))
				{
					m_vItems.RemoveAt(i);
					return true;
				}
			}

			return false;
		}

		private void ClickedHandler(object sender, EventArgs args)
		{
			ProcessClick(sender, false);
		}

		private void ClickedHandler2(object sender, EventArgs args)
		{
			ProcessClick(sender, true);
		}

		private void ProcessClick(object sender, bool b2)
		{
			ToolStripMenuItem tsi = (sender as ToolStripMenuItem);
			if(tsi == null) { Debug.Assert(false); return; }

			string strName = tsi.Text;
			object oTag = tsi.Tag;

			// MoveItemToTop(strName);

			if(m_handler != null)
			{
				if(b2) m_handler.OnMruExecute2(strName, oTag);
				else m_handler.OnMruExecute(strName, oTag);
			}
		}

		private void MoveItemToTop(string strName)
		{
			for(int i = 0; i < m_vItems.Count; ++i)
			{
				Debug.Assert(m_vItems[i].Key != null);
				if(m_vItems[i].Key.Equals(strName, StrUtil.CaseIgnoreCmp))
				{
					KeyValuePair<string, object> t = m_vItems[i];
					m_vItems.RemoveAt(i);
					m_vItems.Insert(0, t);

					return;
				}
			}

			Debug.Assert(false);
		}

		private void ClearHandler(object sender, EventArgs e)
		{
			if(m_handler != null) m_handler.OnMruClear();
		}

		private void OnDropDownOpening(object sender, EventArgs e)
		{
			UpdateMenu();
		}
	}
}
