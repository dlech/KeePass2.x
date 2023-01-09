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

		private static void CopyChildControls(Control cDest, Control cSource,
			Action<Control> fCustomizeDest)
		{
			if(cDest == null) { Debug.Assert(false); return; }

			cDest.SuspendLayout();
			try { CopyChildControlsPriv(cDest, cSource, fCustomizeDest); }
			finally { cDest.ResumeLayout(); }
		}

		private static void CopyChildControlsPriv(Control cDest, Control cSource,
			Action<Control> fCustomizeDest)
		{
			if((cDest == null) || (cSource == null)) { Debug.Assert(false); return; }

			foreach(Control cS in cSource.Controls)
			{
				if(cS == null) { Debug.Assert(false); continue; }

				bool bSetText = true;

				Control cD;
				if(cS is Button) cD = new Button();
				else if(cS is CheckBox)
				{
					cD = new CheckBox();
					(cD as CheckBox).Appearance = (cS as CheckBox).Appearance;
				}
				else if(cS is ComboBox)
				{
					cD = new ComboBox();
					(cD as ComboBox).DropDownStyle = (cS as ComboBox).DropDownStyle;
				}
				else if(cS is GroupBox) cD = new GroupBox();
				else if(cS is HotKeyControlEx) // Before TextBox
				{
					cD = new TextBox();
					bSetText = false;
				}
				else if(cS is Label) cD = new Label();
				else if(cS is NumericUpDown)
				{
					cD = new TextBox(); // NumericUpDown leads to GDI objects leak
					bSetText = false;
				}
				else if(cS is RadioButton) cD = new RadioButton();
				else if(cS is RichTextBox)
				{
					cD = new TextBox(); // RTB leads to GDI objects leak
					(cD as TextBox).Multiline = true;
				}
				else if(cS is TabControl) cD = new TabControl();
				else if(cS is TabPage) // Before Panel
					cD = new TabPage();
				else if(cS is TextBox) // After HotKeyControlEx
				{
					cD = new TextBox();
					(cD as TextBox).Multiline = (cS as TextBox).Multiline;
				}
				else if(cS is Panel) // After TabPage
					cD = new Panel();
				else cD = new Label();

				Color clr = Color.FromArgb(128 + g_rand.Next(0, 128),
					128 + g_rand.Next(0, 128), 128 + g_rand.Next(0, 128));

				cD.Name = cS.Name;
				cD.Font = cS.Font;
				cD.BackColor = clr;
				if(bSetText) cD.Text = cS.Text;

				cD.Location = cS.Location;
				cD.Size = cS.Size;
				// Debug.Assert(cD.ClientSize == cS.ClientSize);
				cD.Dock = cS.Dock;
				cD.Padding = cS.Padding;

				// Type tD = cD.GetType();
				// PropertyInfo piAutoSizeS = tS.GetProperty("AutoSize", typeof(bool));
				// PropertyInfo piAutoSizeD = tD.GetProperty("AutoSize", typeof(bool));
				// if((piAutoSizeS != null) && (piAutoSizeD != null))
				// {
				//	MethodInfo miS = piAutoSizeS.GetGetMethod();
				//	MethodInfo miD = piAutoSizeD.GetSetMethod();
				//	miD.Invoke(cD, new object[] { miS.Invoke(cS, null) });
				// }
				cD.AutoSize = cS.AutoSize;

				cD.TabIndex = cS.TabIndex;
				cD.TabStop = cS.TabStop;

				ButtonBase bbD = (cD as ButtonBase);
				if((bbD != null) && (cS is ButtonBase))
					bbD.TextAlign = (cS as ButtonBase).TextAlign;

				Label lblD = (cD as Label);
				if((lblD != null) && (cS is Label))
					lblD.TextAlign = (cS as Label).TextAlign;

				try
				{
					if(fCustomizeDest != null) fCustomizeDest(cD);

					cDest.Controls.Add(cD);

					if((cS is GroupBox) || (cS is Panel) ||
						(cS is SplitContainer) || (cS is TabControl))
						CopyChildControls(cD, cS, fCustomizeDest);
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
					if(tc != null)
					{
						int i = tc.TabPages.IndexOf(tp);
						Debug.Assert(i >= 0);

						// Workaround for .NET bug: setting SelectedTab causes
						// the preview form to get the focus, which breaks the
						// ability to enter composite characters (Korean, ...)
						// in the main form
						if(!NativeLib.IsUnix() && tc.IsHandleCreated && (i >= 0))
							NativeMethods.SendMessage(tc.Handle,
								NativeMethods.TCM_SETCURSEL, (IntPtr)i, IntPtr.Zero);
						else tc.SelectedTab = tp;
					}
					else { Debug.Assert(false); }
				}

				c = c.Parent;
			}
		}

		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);

			ShowAccelerators();
		}

		public void ShowAccelerators()
		{
			try
			{
				ShowAcceleratorsPriv();

				ThreadPool.QueueUserWorkItem(delegate(object state)
				{
					try
					{
						// Timing-dependent, 100 ms is insufficient
						Thread.Sleep(200);
						Invoke(new VoidDelegate(this.ShowAcceleratorsPriv));
						Thread.Sleep(300);
						Invoke(new VoidDelegate(this.ShowAcceleratorsPriv));
					}
					catch(Exception) { Debug.Assert(false); }
				});
			}
			catch(Exception) { Debug.Assert(false); }
		}

		private void ShowAcceleratorsPriv()
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
	}
}
