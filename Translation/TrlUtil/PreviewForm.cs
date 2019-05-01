/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2019 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using KeePass.UI;
using KeePass.Util;

using KeePassLib.Cryptography;
using KeePassLib.Delegates;

using TrlUtil.Native;

using NativeLib = KeePassLib.Native.NativeLib;

namespace TrlUtil
{
	public sealed class PreviewForm : Form
	{
		private static readonly Random g_rand = CryptoRandom.NewWeakRandom();

		public PreviewForm()
		{
			try { this.DoubleBuffered = true; }
			catch(Exception) { Debug.Assert(false); }
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if(e.KeyCode == Keys.Escape) UIUtil.SetHandled(e, true);
			else base.OnKeyDown(e);
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			if(e.KeyCode == Keys.Escape) UIUtil.SetHandled(e, true);
			else base.OnKeyUp(e);
		}

		public void CopyForm(Form f, Action<Control> fCustomizeCopy)
		{
			SuspendLayout();

			try
			{
				this.MinimizeBox = false;
				this.MaximizeBox = false;
				this.ControlBox = false;
				this.FormBorderStyle = FormBorderStyle.FixedDialog;

				this.Controls.Clear();

				this.ClientSize = f.ClientSize;
				this.Text = f.Text;

				CopyChildControls(this, f, fCustomizeCopy);
			}
			catch(Exception) { Debug.Assert(false); }

			ResumeLayout();
		}

		private void CopyChildControls(Control cDest, Control cSource,
			Action<Control> fCustomizeCopy)
		{
			if((cDest == null) || (cSource == null)) return;

			foreach(Control c in cSource.Controls)
			{
				if(c == null) { Debug.Assert(false); continue; }

				bool bSetText = true;

				Control cCopy;
				if(c is Button) cCopy = new Button();
				else if(c is CheckBox)
				{
					cCopy = new CheckBox();
					(cCopy as CheckBox).Appearance = (c as CheckBox).Appearance;
				}
				else if(c is ComboBox)
				{
					cCopy = new ComboBox();
					(cCopy as ComboBox).DropDownStyle = (c as ComboBox).DropDownStyle;
				}
				else if(c is GroupBox) cCopy = new GroupBox();
				else if(c is HotKeyControlEx)
				{
					cCopy = new TextBox();
					bSetText = false;
				}
				else if(c is Label) cCopy = new Label();
				else if(c is NumericUpDown)
				{
					cCopy = new TextBox(); // NumericUpDown leads to GDI objects leak
					bSetText = false;
				}
				else if(c is RadioButton) cCopy = new RadioButton();
				else if(c is RichTextBox)
				{
					cCopy = new TextBox(); // RTB leads to GDI objects leak
					(cCopy as TextBox).Multiline = true;
				}
				else if(c is TabControl) cCopy = new TabControl();
				else if(c is TabPage) cCopy = new TabPage();
				// HotKeyControlEx is a TextBox, so HotKeyControlEx must be first
				else if(c is TextBox)
				{
					cCopy = new TextBox();
					(cCopy as TextBox).Multiline = (c as TextBox).Multiline;
				}
				// TabPage is a Panel, so TabPage must be first
				else if(c is Panel) cCopy = new Panel();
				else cCopy = new Label();

				Color clr = Color.FromArgb(128 + g_rand.Next(0, 128),
					128 + g_rand.Next(0, 128), 128 + g_rand.Next(0, 128));

				cCopy.Name = c.Name;
				cCopy.Font = c.Font;
				cCopy.BackColor = clr;
				if(bSetText) cCopy.Text = c.Text;

				cCopy.Location = c.Location;
				cCopy.Size = c.Size;
				// Debug.Assert(cCopy.ClientSize == c.ClientSize);
				cCopy.Dock = c.Dock;
				cCopy.Padding = c.Padding;

				// Type tCopy = cCopy.GetType();
				// PropertyInfo piAutoSizeSrc = t.GetProperty("AutoSize", typeof(bool));
				// PropertyInfo piAutoSizeDst = tCopy.GetProperty("AutoSize", typeof(bool));
				// if((piAutoSizeSrc != null) && (piAutoSizeDst != null))
				// {
				//	MethodInfo miSrc = piAutoSizeSrc.GetGetMethod();
				//	MethodInfo miDst = piAutoSizeDst.GetSetMethod();
				//	miDst.Invoke(cCopy, new object[] { miSrc.Invoke(c, null) });
				// }
				cCopy.AutoSize = c.AutoSize;

				cCopy.TabIndex = c.TabIndex;
				cCopy.TabStop = c.TabStop;

				ButtonBase bbCopy = (cCopy as ButtonBase);
				if((bbCopy != null) && (c is ButtonBase))
					bbCopy.TextAlign = (c as ButtonBase).TextAlign;

				Label lCopy = (cCopy as Label);
				if((lCopy != null) && (c is Label))
					lCopy.TextAlign = (c as Label).TextAlign;

				try
				{
					if(fCustomizeCopy != null) fCustomizeCopy(cCopy);

					cDest.Controls.Add(cCopy);

					if((c is GroupBox) || (c is Panel) ||
						(c is SplitContainer) || (c is TabControl))
						CopyChildControls(cCopy, c, fCustomizeCopy);
				}
				catch(Exception) { Debug.Assert(false); }
			}
		}

		internal void EnsureControlPageVisible(string strName)
		{
			if(string.IsNullOrEmpty(strName)) return; // No assert

			Control[] v = this.Controls.Find(strName, true);
			if((v == null) || (v.Length == 0)) return; // No assert

			Control c = v[0];
			while((c != null) && !(c is Form))
			{
				TabPage tp = (c as TabPage);
				if(tp != null)
				{
					TabControl tc = (tp.Parent as TabControl);
					if(tc != null) tc.SelectedTab = tp;
					else { Debug.Assert(false); }
				}

				c = c.Parent;
			}
		}

		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);

			// Timing-dependent, 100 ms is insufficient
			ShowAcceleratorsAsync(200);
			ShowAcceleratorsAsync(600);
		}

		public void ShowAccelerators()
		{
			try
			{
				IntPtr hWnd = this.Handle;
				if(hWnd == IntPtr.Zero) { Debug.Assert(false); return; }

				NativeMethods.SendMessage(hWnd, NativeMethods.WM_UPDATEUISTATE,
					new IntPtr(NativeMethods.MakeLong(NativeMethods.UIS_CLEAR,
					NativeMethods.UISF_HIDEACCEL)), IntPtr.Zero);
			}
			catch(Exception) { Debug.Assert(NativeLib.IsUnix()); }
		}

		private void ShowAcceleratorsAsync(int msDelay)
		{
			try
			{
				ThreadPool.QueueUserWorkItem(delegate(object state)
				{
					try
					{
						Thread.Sleep(msDelay);
						Invoke(new VoidDelegate(this.ShowAccelerators));
					}
					catch(Exception) { Debug.Assert(false); }
				});
			}
			catch(Exception) { Debug.Assert(false); }
		}
	}
}
