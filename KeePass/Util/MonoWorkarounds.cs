/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2012 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.ComponentModel;
using System.Reflection;
using System.Diagnostics;

using KeePassLib.Native;

namespace KeePass.Util
{
	public static class MonoWorkarounds
	{
		private static bool? m_bReq = null;
		public static bool IsRequired
		{
			get
			{
				if(!m_bReq.HasValue) m_bReq = NativeLib.IsUnix();
				return m_bReq.Value;
			}
		}

		public static void ApplyTo(Form f)
		{
			if(!MonoWorkarounds.IsRequired) return;
			if(f == null) { Debug.Assert(false); return; }

			ApplyToControlsRec(f.Controls, f, MonoWorkarounds.ApplyToControl);
		}

		public static void Release(Form f)
		{
			if(!MonoWorkarounds.IsRequired) return;
			if(f == null) { Debug.Assert(false); return; }

			ApplyToControlsRec(f.Controls, f, MonoWorkarounds.ReleaseControl);
		}

		private delegate void MwaControlHandler(Control c, Form fContext);

		private static void ApplyToControlsRec(Control.ControlCollection cc,
			Form fContext, MwaControlHandler fn)
		{
			if(cc == null) { Debug.Assert(false); return; }

			foreach(Control c in cc)
			{
				fn(c, fContext);
				ApplyToControlsRec(c.Controls, fContext, fn);
			}
		}

		private sealed class MwaHandlerInfo
		{
			private readonly Delegate m_fnOrg; // May be null
			public Delegate FunctionOriginal
			{
				get { return m_fnOrg; }
			}

			private readonly Delegate m_fnOvr;
			public Delegate FunctionOverride
			{
				get { return m_fnOvr; }
			}

			private readonly DialogResult m_dr;
			public DialogResult Result
			{
				get { return m_dr; }
			}

			private readonly Form m_fContext;
			public Form FormContext
			{
				get { return m_fContext; }
			}

			public MwaHandlerInfo(Delegate fnOrg, Delegate fnOvr, DialogResult dr,
				Form fContext)
			{
				m_fnOrg = fnOrg;
				m_fnOvr = fnOvr;
				m_dr = dr;
				m_fContext = fContext;
			}
		}

		private static void ApplyToControl(Control c, Form fContext)
		{
			Button btn = (c as Button);
			if(btn != null) ApplyToButton(btn, fContext);
		}

		private static EventHandlerList GetEventHandlers(Component c,
			out object objClickEvent)
		{
			FieldInfo fi = typeof(Control).GetField("ClickEvent", // Mono
				BindingFlags.Static | BindingFlags.NonPublic);
			if(fi == null)
				fi = typeof(Control).GetField("EventClick", // .NET
					BindingFlags.Static | BindingFlags.NonPublic);
			if(fi == null) { Debug.Assert(false); objClickEvent = null; return null; }

			objClickEvent = fi.GetValue(null);
			if(objClickEvent == null) { Debug.Assert(false); return null; }

			PropertyInfo pi = typeof(Component).GetProperty("Events",
				BindingFlags.Instance | BindingFlags.NonPublic);
			return (pi.GetValue(c, null) as EventHandlerList);
		}

		private static Dictionary<object, MwaHandlerInfo> m_dictHandlers =
			new Dictionary<object, MwaHandlerInfo>();
		private static void ApplyToButton(Button btn, Form fContext)
		{
			DialogResult dr = btn.DialogResult;
			if(dr == DialogResult.None) return; // No workaround required

			object objClickEvent;
			EventHandlerList ehl = GetEventHandlers(btn, out objClickEvent);
			if(ehl == null) { Debug.Assert(false); return; }
			Delegate fnClick = ehl[objClickEvent]; // May be null

			EventHandler fnOvr = new EventHandler(MonoWorkarounds.OnButtonClick);
			m_dictHandlers[btn] = new MwaHandlerInfo(fnClick, fnOvr, dr, fContext);

			btn.DialogResult = DialogResult.None;
			if(fnClick != null) ehl.RemoveHandler(objClickEvent, fnClick);
			ehl[objClickEvent] = fnOvr;
		}

		private static void ReleaseControl(Control c, Form fContext)
		{
			Button btn = (c as Button);
			if(btn != null) ReleaseButton(btn, fContext);
		}

		private static void ReleaseButton(Button btn, Form fContext)
		{
			MwaHandlerInfo hi;
			m_dictHandlers.TryGetValue(btn, out hi);
			if(hi == null) return;

			object objClickEvent;
			EventHandlerList ehl = GetEventHandlers(btn, out objClickEvent);
			if(ehl == null) { Debug.Assert(false); return; }

			ehl.RemoveHandler(objClickEvent, hi.FunctionOverride);
			if(hi.FunctionOriginal != null)
				ehl[objClickEvent] = hi.FunctionOriginal;

			btn.DialogResult = hi.Result;
			m_dictHandlers.Remove(btn);
		}

		private static void OnButtonClick(object sender, EventArgs e)
		{
			Button btn = (sender as Button);
			if(btn == null) { Debug.Assert(false); return; }

			MwaHandlerInfo hi;
			m_dictHandlers.TryGetValue(btn, out hi);
			if(hi == null) { Debug.Assert(false); return; }

			Form f = hi.FormContext;

			// Set current dialog result by setting the form's private
			// variable; the DialogResult property can't be used,
			// because it raises close events
			FieldInfo fiRes = typeof(Form).GetField("dialog_result",
				BindingFlags.Instance | BindingFlags.NonPublic);
			if(fiRes == null) { Debug.Assert(false); return; }
			if(f != null) fiRes.SetValue(f, hi.Result);

			if(hi.FunctionOriginal != null)
				hi.FunctionOriginal.Method.Invoke(hi.FunctionOriginal.Target,
					new object[]{ btn, e });

			// Raise close events, if the click event handler hasn't
			// reset the dialog result
			if((f != null) && (f.DialogResult == hi.Result))
				f.DialogResult = hi.Result; // Raises close events
		}
	}
}
