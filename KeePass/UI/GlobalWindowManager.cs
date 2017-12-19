/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2017 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;

using KeePass.App;
using KeePass.Forms;

using KeePassLib.Native;
using KeePassLib.Utility;

namespace KeePass.UI
{
	public sealed class GwmWindowEventArgs : EventArgs
	{
		private Form m_form;
		public Form Form
		{
			get { return m_form; }
		}

		private IGwmWindow m_gwmWindow;
		public IGwmWindow GwmWindow
		{
			get { return m_gwmWindow; }
		}

		public GwmWindowEventArgs(Form form, IGwmWindow gwmWindow)
		{
			m_form = form;
			m_gwmWindow = gwmWindow;
		}
	}

	public static class GlobalWindowManager
	{
		private static List<KeyValuePair<Form, IGwmWindow>> g_vWindows =
			new List<KeyValuePair<Form, IGwmWindow>>();
		private static List<CommonDialog> g_vDialogs = new List<CommonDialog>();

		private static object g_oSyncRoot = new object();

		public static uint WindowCount
		{
			get
			{
				uint u;
				lock(g_oSyncRoot)
				{
					u = ((uint)(g_vWindows.Count + g_vDialogs.Count) +
						MessageService.CurrentMessageCount);
				}
				return u;
			}
		}

		public static bool CanCloseAllWindows
		{
			get
			{
				lock(g_oSyncRoot)
				{
					if(g_vDialogs.Count > 0) return false;
					if(MessageService.CurrentMessageCount > 0) return false;

					foreach(KeyValuePair<Form, IGwmWindow> kvp in g_vWindows)
					{
						if(kvp.Value == null) return false;
						else if(!kvp.Value.CanCloseWithoutDataLoss)
							return false;
					}
				}

				return true;
			}
		}

		public static Form TopWindow
		{
			get
			{
				lock(g_oSyncRoot)
				{
					int n = g_vWindows.Count;
					if(n > 0) return g_vWindows[n - 1].Key;
				}

				return null;
			}
		}

		public static event EventHandler<GwmWindowEventArgs> WindowAdded;
		public static event EventHandler<GwmWindowEventArgs> WindowRemoved;

		public static void AddWindow(Form form)
		{
			AddWindow(form, null);
		}

		public static void AddWindow(Form form, IGwmWindow wnd)
		{
			Debug.Assert(form != null);
			if(form == null) throw new ArgumentNullException("form");

			KeyValuePair<Form, IGwmWindow> kvp = new KeyValuePair<Form, IGwmWindow>(
				form, wnd);

			lock(g_oSyncRoot)
			{
				Debug.Assert(g_vWindows.IndexOf(kvp) < 0);
				g_vWindows.Add(kvp);
			}

			// The control box must be enabled, otherwise DPI scaling
			// doesn't work due to a .NET bug:
			// http://connect.microsoft.com/VisualStudio/feedback/details/694242/difference-dpi-let-the-button-cannot-appear-completely-while-remove-the-controlbox-for-the-form
			// http://social.msdn.microsoft.com/Forums/en-US/winforms/thread/67407313-8cb2-42b4-afb9-8be816f0a601/
			Debug.Assert(form.ControlBox);

			form.TopMost = Program.Config.MainWindow.AlwaysOnTop;
			// Form formParent = form.ParentForm;
			// if(formParent != null) form.TopMost = formParent.TopMost;
			// else { Debug.Assert(false); }

			// form.Font = new System.Drawing.Font(System.Drawing.SystemFonts.MessageBoxFont.Name, 12.0f);

			CustomizeForm(form);

			MonoWorkarounds.ApplyTo(form);

			if(GlobalWindowManager.WindowAdded != null)
				GlobalWindowManager.WindowAdded(null, new GwmWindowEventArgs(
					form, wnd));
		}

		public static void AddDialog(CommonDialog dlg)
		{
			Debug.Assert(dlg != null);
			if(dlg == null) throw new ArgumentNullException("dlg");

			lock(g_oSyncRoot) { g_vDialogs.Add(dlg); }
		}

		public static void RemoveWindow(Form form)
		{
			Debug.Assert(form != null);
			if(form == null) throw new ArgumentNullException("form");

			lock(g_oSyncRoot)
			{
				for(int i = 0; i < g_vWindows.Count; ++i)
				{
					if(g_vWindows[i].Key == form)
					{
						if(GlobalWindowManager.WindowRemoved != null)
							GlobalWindowManager.WindowRemoved(null, new GwmWindowEventArgs(
								form, g_vWindows[i].Value));

						MonoWorkarounds.Release(form);

#if DEBUG
						DebugClose(form);
#endif

						g_vWindows.RemoveAt(i);
						return;
					}
				}
			}

			Debug.Assert(false); // Window not found
		}

		public static void RemoveDialog(CommonDialog dlg)
		{
			Debug.Assert(dlg != null);
			if(dlg == null) throw new ArgumentNullException("dlg");

			lock(g_oSyncRoot)
			{
				Debug.Assert(g_vDialogs.IndexOf(dlg) >= 0);
				g_vDialogs.Remove(dlg);
			}
		}

		public static void CloseAllWindows()
		{
			Debug.Assert(GlobalWindowManager.CanCloseAllWindows);

			KeyValuePair<Form, IGwmWindow>[] vWindows;
			lock(g_oSyncRoot) { vWindows = g_vWindows.ToArray(); }
			Array.Reverse(vWindows); // Close windows in reverse order

			foreach(KeyValuePair<Form, IGwmWindow> kvp in vWindows)
			{
				if(kvp.Value == null) continue;
				else if(kvp.Value.CanCloseWithoutDataLoss)
				{
					if(kvp.Key.InvokeRequired)
						kvp.Key.Invoke(new CloseFormDelegate(
							GlobalWindowManager.CloseForm), kvp.Key);
					else CloseForm(kvp.Key);

					Application.DoEvents();
				}
			}
		}

		private delegate void CloseFormDelegate(Form f);

		private static void CloseForm(Form f)
		{
			try
			{
				f.DialogResult = DialogResult.Cancel;
				f.Close();
			}
			catch(Exception) { Debug.Assert(false); }
		}

		public static bool HasWindow(IntPtr hWindow)
		{
			lock(g_oSyncRoot)
			{
				foreach(KeyValuePair<Form, IGwmWindow> kvp in g_vWindows)
				{
					if(kvp.Key.Handle == hWindow) return true;
				}
			}

			return false;
		}

		/* internal static bool HasWindowMW(IntPtr hWindow)
		{
			if(HasWindow(hWindow)) return true;

			MainForm mf = Program.MainForm;
			if((mf != null) && (mf.Handle == hWindow)) return true;

			return false;
		} */

		internal static bool ActivateTopWindow()
		{
			try
			{
				Form f = GlobalWindowManager.TopWindow;
				if(f == null) return false;

				f.Activate();
				return true;
			}
			catch(Exception) { Debug.Assert(false); }

			return false;
		}

		public static void CustomizeForm(Form f)
		{
			CustomizeControl(f);

			try
			{
				const string strForms = "KeePass.Forms.";
				Debug.Assert(typeof(PwEntryForm).FullName.StartsWith(strForms));

				if(f.GetType().FullName.StartsWith(strForms) &&
					(f.FormBorderStyle == FormBorderStyle.FixedDialog))
					UIUtil.RemoveBannerIfNecessary(f);
			}
			catch(Exception) { Debug.Assert(false); }
		}

		public static void CustomizeControl(Control c)
		{
			if(NativeLib.IsUnix() && Program.Config.UI.ForceSystemFontUnix)
			{
				Font font = UISystemFonts.DefaultFont;
				if(font != null) CustomizeFont(c, font);
			}
		}

		private static void CustomizeFont(Control c, Font font)
		{
			if((c is Form) || (c is ToolStrip) || (c is ContextMenuStrip))
				c.Font = font;

			foreach(Control cSub in c.Controls)
				CustomizeFont(cSub, font);

			if(c.ContextMenuStrip != null)
				CustomizeFont(c.ContextMenuStrip, font);
		}

#if DEBUG
		private static void DebugClose(Control c)
		{
			if(c == null) { Debug.Assert(false); return; }

			List<ImageList> lInv = new List<ImageList>();
			lInv.Add(Program.MainForm.ClientIcons);

			ListView lv = (c as ListView);
			if(lv != null)
			{
				// Image list properties must be set to null manually
				// when closing, otherwise we get a memory leak
				// (because the image list holds event handlers to
				// the list)
				Debug.Assert(!lInv.Contains(lv.LargeImageList));
				Debug.Assert(!lInv.Contains(lv.SmallImageList));
			}

			TreeView tv = (c as TreeView);
			if(tv != null)
			{
				Debug.Assert(!lInv.Contains(tv.ImageList)); // See above
			}

			TabControl tc = (c as TabControl);
			if(tc != null)
			{
				Debug.Assert(!lInv.Contains(tc.ImageList)); // See above
			}

			ToolStrip ts = (c as ToolStrip);
			if(ts != null)
			{
				Debug.Assert(!lInv.Contains(ts.ImageList)); // See above
			}

			Button btn = (c as Button);
			if(btn != null)
			{
				Debug.Assert(!lInv.Contains(btn.ImageList)); // See above
			}

			foreach(Control cc in c.Controls)
				DebugClose(cc);
		}
#endif
	}
}
