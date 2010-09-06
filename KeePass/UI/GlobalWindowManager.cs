/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2010 Dominik Reichl <dominik.reichl@t-online.de>

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
		private static List<KeyValuePair<Form, IGwmWindow>> m_vWindows =
			new List<KeyValuePair<Form, IGwmWindow>>();
		private static List<CommonDialog> m_vDialogs = new List<CommonDialog>();

		public static uint WindowCount
		{
			get
			{
				return (uint)(m_vWindows.Count + m_vDialogs.Count) +
					MessageService.CurrentMessageCount;
			}
		}

		public static bool CanCloseAllWindows
		{
			get
			{
				if(m_vDialogs.Count > 0) return false;
				if(MessageService.CurrentMessageCount > 0) return false;

				foreach(KeyValuePair<Form, IGwmWindow> kvp in m_vWindows)
				{
					if(kvp.Value == null) return false;
					else if(!kvp.Value.CanCloseWithoutDataLoss)
						return false;
				}

				return true;
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

			Debug.Assert(m_vWindows.IndexOf(kvp) < 0);
			m_vWindows.Add(kvp);

			form.TopMost = Program.Config.MainWindow.AlwaysOnTop;
			// Form formParent = form.ParentForm;
			// if(formParent != null) form.TopMost = formParent.TopMost;
			// else { Debug.Assert(false); }

			// form.Font = new System.Drawing.Font(System.Drawing.SystemFonts.MessageBoxFont.Name, 12.0f);

			CustomizeControl(form);

			if(GlobalWindowManager.WindowAdded != null)
				GlobalWindowManager.WindowAdded(null, new GwmWindowEventArgs(
					form, wnd));
		}

		public static void AddDialog(CommonDialog dlg)
		{
			Debug.Assert(dlg != null);
			if(dlg == null) throw new ArgumentNullException("dlg");

			m_vDialogs.Add(dlg);
		}

		public static void RemoveWindow(Form form)
		{
			Debug.Assert(form != null);
			if(form == null) throw new ArgumentNullException("form");

			for(int i = 0; i < m_vWindows.Count; ++i)
			{
				if(m_vWindows[i].Key == form)
				{
					if(GlobalWindowManager.WindowRemoved != null)
						GlobalWindowManager.WindowRemoved(null, new GwmWindowEventArgs(
							form, m_vWindows[i].Value));

					m_vWindows.RemoveAt(i);
					return;
				}
			}

			Debug.Assert(false); // Window not found!
		}

		public static void RemoveDialog(CommonDialog dlg)
		{
			Debug.Assert(dlg != null);
			if(dlg == null) throw new ArgumentNullException("dlg");

			Debug.Assert(m_vDialogs.IndexOf(dlg) >= 0);
			m_vDialogs.Remove(dlg);
		}

		public static void CloseAllWindows()
		{
			Debug.Assert(GlobalWindowManager.CanCloseAllWindows == true);

			KeyValuePair<Form, IGwmWindow>[] vWindows = m_vWindows.ToArray();
			Array.Reverse(vWindows); // Close windows in reverse order

			foreach(KeyValuePair<Form, IGwmWindow> kvp in vWindows)
			{
				if(kvp.Value == null) continue;
				else if(kvp.Value.CanCloseWithoutDataLoss)
				{
					kvp.Key.DialogResult = DialogResult.Cancel;
					kvp.Key.Close();

					Application.DoEvents();
				}
			}
		}

		public static bool HasWindow(IntPtr hWindow)
		{
			foreach(KeyValuePair<Form, IGwmWindow> kvp in m_vWindows)
			{
				if(kvp.Key.Handle == hWindow) return true;
			}

			return false;
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
	}
}
