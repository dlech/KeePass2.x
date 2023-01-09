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
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

using KeePass.Native;

using KeePassLib.Utility;

using NativeLib = KeePassLib.Native.NativeLib;

namespace KeePass.UI
{
	public static class AccessibilityEx
	{
		private static bool g_bScreenReaderActive = false;

		public static bool Enabled
		{
			get
			{
				if(Program.DesignMode) return false;

				return (Program.Config.UI.OptimizeForScreenReader ||
					g_bScreenReaderActive);
			}
		}

		internal static void OnSystemSettingChange()
		{
			try
			{
				if(NativeLib.IsUnix()) return;

				Debug.Assert(Marshal.SizeOf(typeof(int)) == 4); // BOOL = int
				int i = 0;
				if(NativeMethods.SystemParametersInfoI32(
					NativeMethods.SPI_GETSCREENREADER, 0, ref i, 0))
				{
					bool b = (i != 0);
#if DEBUG
					if(b != g_bScreenReaderActive)
						Trace.WriteLine("Screen reader is " + (b ?
							string.Empty : "in") + "active.");
#endif
					g_bScreenReaderActive = b;
				}
				else { Debug.Assert(false); }
			}
			catch(Exception) { Debug.Assert(false); }
		}

		internal static void InitializeForm(Form f)
		{
			if(f == null) { Debug.Assert(false); return; }
			if(!AccessibilityEx.Enabled) return;

			ReorderChildControlsByLocation(f);
		}

		internal static void CustomizeForm(Form f)
		{
			if(f == null) { Debug.Assert(false); return; }
			// Scroll bars can be useful even when not using an
			// accessibility tool
			// if(!AccessibilityEx.Enabled) return;

			EnableScrollBarsIfNecessary(f);
		}

		private static void ReorderChildControlsByLocation(Control c)
		{
			if(c == null) { Debug.Assert(false); return; }

			Control.ControlCollection cc = c.Controls;
			if((cc == null) || (cc.Count == 0)) return;

			c.SuspendLayout();
			try
			{
				List<KeyValuePair<Rectangle, Control>> l =
					new List<KeyValuePair<Rectangle, Control>>();
				int cDockT = 0, cDockB = 0, cDockL = 0, cDockR = 0, cDockF = 0;

				foreach(Control cSub in cc)
				{
					if((cSub == null) || (cSub == c)) { Debug.Assert(false); continue; }

					Rectangle r = cSub.Bounds;
					if((r.Width < 0) || (r.Height < 0)) { Debug.Assert(false); continue; }

					l.Add(new KeyValuePair<Rectangle, Control>(r, cSub));

					switch(cSub.Dock)
					{
						case DockStyle.Top:
							++cDockT; break;
						case DockStyle.Bottom:
							++cDockB; break;
						case DockStyle.Left:
							++cDockL; break;
						case DockStyle.Right:
							++cDockR; break;
						case DockStyle.Fill:
							++cDockF; break;
						default: break;
					}
				}

				// Reordering docked controls can move them visually
				bool bDockMovePossible = (
					((cDockT + cDockL + cDockF) >= 2) || // Top left
					((cDockT + cDockR + cDockF) >= 2) || // Top right
					((cDockB + cDockL + cDockF) >= 2) || // Bottom left
					((cDockB + cDockR + cDockF) >= 2)); // Bottom right

				bool bIgnoreType = ((c is DataGridView) || (c is NumericUpDown) ||
					(c is SplitContainer) || (c is TabControl) || (c is ToolStrip));

				Debug.Assert(bIgnoreType || !cc.IsReadOnly);
				if((l.Count >= 2) && !bDockMovePossible && !bIgnoreType &&
					!cc.IsReadOnly)
				{
					l.Sort(AccessibilityEx.CompareByLocation);

					for(int i = 0; i < l.Count; ++i)
						cc.SetChildIndex(l[i].Value, i);

#if DEBUG
					for(int i = 0; i < l.Count; ++i)
						Debug.Assert(cc[i] == l[i].Value);
#endif
				}

				foreach(KeyValuePair<Rectangle, Control> kvp in l)
					ReorderChildControlsByLocation(kvp.Value);
			}
			catch(Exception) { Debug.Assert(false); }
			finally { c.ResumeLayout(); }
		}

		private static int CompareByLocation(KeyValuePair<Rectangle, Control> kvpA,
			KeyValuePair<Rectangle, Control> kvpB)
		{
			Rectangle rA = kvpA.Key, rB = kvpB.Key;
			bool bAB = (rB.Y > (rA.Y + (rA.Height >> 1)));
			bool bBA = (rA.Y > (rB.Y + (rB.Height >> 1)));

			if(bAB && bBA) { Debug.Assert(false); } // Compare by X
			else if(bAB) return -1;
			else if(bBA) return 1;
			// else: they are on the same line, compare by X

			return rA.X.CompareTo(rB.X);
		}

		private static void EnableScrollBarsIfNecessary(Form f)
		{
			if(f == null) { Debug.Assert(false); return; }

			try
			{
				if(f.FormBorderStyle != FormBorderStyle.FixedDialog) return;
				if(f.WindowState != FormWindowState.Normal) { Debug.Assert(false); return; }
				Debug.Assert(!f.AutoScroll);
				Debug.Assert(f.IsHandleCreated); // For location

				Screen s = Screen.FromControl(f);
				if(s == null) { Debug.Assert(false); return; }

				Rectangle rW = s.WorkingArea;
				Rectangle rF = f.Bounds;
				if((rW.Width <= 0) || (rW.Height <= 0) || (rF.Width <= 0) ||
					(rF.Height <= 0)) { Debug.Assert(false); return; }

				bool bEnoughW = (rF.Width <= rW.Width);
				bool bEnoughH = (rF.Height <= rW.Height);
				if(bEnoughW && bEnoughH) return;

				int xFCenter = rF.X + (rF.Width >> 1);
				int yFCenter = rF.Y + (rF.Height >> 1);

				if(!bEnoughW) rF.Height += UIUtil.GetHScrollBarHeight();
				if(!bEnoughH) rF.Width += UIUtil.GetVScrollBarWidth();

				if((rF.Width <= 0) || (rF.Height <= 0)) { Debug.Assert(false); return; }
				if(rF.Width > rW.Width) rF.Width = rW.Width;
				if(rF.Height > rW.Height) rF.Height = rW.Height;

				rF.X = xFCenter - (rF.Width >> 1);
				if(rF.X < rW.X) rF.X = rW.X;
				if(rF.Right > rW.Right) rF.X = rW.Right - rF.Width;

				rF.Y = yFCenter - (rF.Height >> 1);
				if(rF.Y < rW.Y) rF.Y = rW.Y;
				if(rF.Bottom > rW.Bottom) rF.Y = rW.Bottom - rF.Height;

				Debug.Assert(rW.Contains(rF));

				// f.AutoScrollMinSize = f.ClientSize; // ClientSize is unreliable
				f.AutoScroll = true;

				f.Bounds = rF;
			}
			catch(Exception) { Debug.Assert(false); }
		}

		private static bool IsTextEditable(Control c)
		{
			if(c == null) { Debug.Assert(false); return false; }

			return ((c is DateTimePicker) || (c is ListControl) ||
				(c is TextBoxBase) || (c is UpDownBase));
		}

		public static void SetName(Control c, string strName)
		{
			SetName(c, strName, null);
		}

		internal static void SetName(Control c, string strName, string strNameSub)
		{
			if(c == null) { Debug.Assert(false); return; }

			try
			{
				if(!AccessibilityEx.Enabled) return;

				if(!string.IsNullOrEmpty(strName))
					strName = strName.TrimEnd(':');

				string str;
				if(!string.IsNullOrEmpty(strName))
				{
					if(!string.IsNullOrEmpty(strNameSub))
						str = strName + " \u2013 " + strNameSub;
					else str = strName;
				}
				else str = strNameSub;

				c.AccessibleName = str; // Null is allowed

				if((c is PictureBox) || (c is QualityProgressBar))
					c.AccessibleRole = AccessibleRole.StaticText;
			}
			catch(Exception) { Debug.Assert(false); }
		}

		internal static void SetContext(Control c, Control cContext)
		{
			if(c == null) { Debug.Assert(false); return; }
			if(cContext == null) { Debug.Assert(false); return; }

			Debug.Assert(!cContext.TabStop || !c.TabStop ||
				(cContext.TabIndex == (c.TabIndex - 1)));

			try
			{
				if(!AccessibilityEx.Enabled) return;

				string str1 = (IsTextEditable(cContext) ? string.Empty :
					StrUtil.RemoveAccelerator(cContext.Text ?? string.Empty));
				string str2 = (IsTextEditable(c) ? string.Empty :
					StrUtil.RemoveAccelerator(c.Text ?? string.Empty));

				SetName(c, str1, str2);
			}
			catch(Exception) { Debug.Assert(false); }
		}
	}
}
