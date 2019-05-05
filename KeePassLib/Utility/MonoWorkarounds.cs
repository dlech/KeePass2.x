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

#if DEBUG
// #define DEBUG_BREAKONFAIL
#endif

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;

#if !KeePassUAP
using System.Windows.Forms;
#endif

using KeePassLib.Native;

namespace KeePassLib.Utility
{
	public static class MonoWorkarounds
	{
		private const string AppXDoTool = "xdotool";

		private static Dictionary<uint, bool> g_dForceReq = new Dictionary<uint, bool>();
		private static Thread g_thFixClip = null;
		// private static Predicate<IntPtr> g_fOwnWindow = null;

#if DEBUG_BREAKONFAIL
		private static DebugBreakTraceListener g_tlBreak = null;
#endif

		private static bool? g_bReq = null;
		public static bool IsRequired()
		{
			if(!g_bReq.HasValue) g_bReq = NativeLib.IsUnix();
			return g_bReq.Value;
		}

		// 106:
		//   Mono throws exceptions when no X server is running.
		//   https://sourceforge.net/p/keepass/patches/106/
		// 1219:
		//   Mono prepends byte order mark (BOM) to StdIn.
		//   https://sourceforge.net/p/keepass/bugs/1219/
		// 1245:
		//   Key events not raised while Alt is down, and nav keys out of order.
		//   https://sourceforge.net/p/keepass/bugs/1245/
		// 1254:
		//   NumericUpDown bug: text is drawn below up/down buttons.
		//   https://sourceforge.net/p/keepass/bugs/1254/
		// 1354:
		//   Finalizer of NotifyIcon throws on Unity.
		//   See also 1574.
		//   https://sourceforge.net/p/keepass/bugs/1354/
		// 1358:
		//   FileDialog crashes when ~/.recently-used is invalid.
		//   https://sourceforge.net/p/keepass/bugs/1358/
		// 1366:
		//   Drawing bug when scrolling a RichTextBox.
		//   https://sourceforge.net/p/keepass/bugs/1366/
		// 1378:
		//   Mono doesn't implement Microsoft.Win32.SystemEvents events.
		//   https://sourceforge.net/p/keepass/bugs/1378/
		//   https://github.com/mono/mono/blob/master/mcs/class/System/Microsoft.Win32/SystemEvents.cs
		// 1418:
		//   Minimizing a form while loading it doesn't work.
		//   https://sourceforge.net/p/keepass/bugs/1418/
		// 1468:
		//   Use LibGCrypt for AES-KDF, because Mono's implementations
		//   of RijndaelManaged and AesCryptoServiceProvider are slow.
		//   https://sourceforge.net/p/keepass/bugs/1468/
		// 1527:
		//   Timer causes 100% CPU load.
		//   https://sourceforge.net/p/keepass/bugs/1527/
		// 1530:
		//   Mono's clipboard functions don't work properly.
		//   https://sourceforge.net/p/keepass/bugs/1530/
		// 1574:
		//   Finalizer of NotifyIcon throws on Mac OS X.
		//   See also 1354.
		//   https://sourceforge.net/p/keepass/bugs/1574/
		// 1632:
		//   RichTextBox rendering bug for bold/italic text.
		//   https://sourceforge.net/p/keepass/bugs/1632/
		// 1690:
		//   Removing items from a list view doesn't work properly.
		//   https://sourceforge.net/p/keepass/bugs/1690/
		// 1710:
		//   Mono doesn't always raise the FormClosed event properly.
		//   https://sourceforge.net/p/keepass/bugs/1710/
		// 1716:
		//   'Always on Top' doesn't work properly on the Cinnamon desktop.
		//   https://sourceforge.net/p/keepass/bugs/1716/
		// 1760:
		//   Input focus is not restored when activating a form.
		//   https://sourceforge.net/p/keepass/bugs/1760/
		// 2140:
		//   Explicit control focusing is ignored.
		//   https://sourceforge.net/p/keepass/feature-requests/2140/
		// 5795:
		//   Text in input field is incomplete.
		//   https://bugzilla.xamarin.com/show_bug.cgi?id=5795
		//   https://sourceforge.net/p/keepass/discussion/329220/thread/d23dc88b/
		// 9604:
		//   Trying to resolve a non-existing metadata token crashes Mono.
		//   https://github.com/mono/mono/issues/9604
		// 10163:
		//   WebRequest GetResponse call missing, breaks WebDAV due to no PUT.
		//   https://bugzilla.xamarin.com/show_bug.cgi?id=10163
		//   https://sourceforge.net/p/keepass/bugs/1117/
		//   https://sourceforge.net/p/keepass/discussion/329221/thread/9422258c/
		//   https://github.com/mono/mono/commit/8e67b8c2fc7cb66bff7816ebf7c1039fb8cfc43b
		//   https://bugzilla.xamarin.com/show_bug.cgi?id=1512
		//   https://sourceforge.net/p/keepass/patches/89/
		// 12525:
		//   PictureBox not rendered when bitmap height >= control height.
		//   https://bugzilla.xamarin.com/show_bug.cgi?id=12525
		//   https://sourceforge.net/p/keepass/discussion/329220/thread/54f61e9a/
		// 100001:
		//   Control locations/sizes are invalid/unexpected.
		//   [NoRef]
		// 373134:
		//   Control.InvokeRequired doesn't always return the correct value.
		//   https://bugzilla.novell.com/show_bug.cgi?id=373134
		// 586901:
		//   RichTextBox doesn't handle Unicode string correctly.
		//   https://bugzilla.novell.com/show_bug.cgi?id=586901
		// 620618:
		//   ListView column headers not drawn.
		//   https://bugzilla.novell.com/show_bug.cgi?id=620618
		// 649266:
		//   Calling Control.Hide doesn't remove the application from taskbar.
		//   https://bugzilla.novell.com/show_bug.cgi?id=649266
		// 686017:
		//   Minimum sizes must be enforced.
		//   https://bugs.debian.org/cgi-bin/bugreport.cgi?bug=686017
		// 801414:
		//   Mono recreates the main window incorrectly.
		//   https://bugs.launchpad.net/ubuntu/+source/keepass2/+bug/801414
		// 891029:
		//   Increase tab control height and don't use images on tabs.
		//   https://sourceforge.net/projects/keepass/forums/forum/329221/topic/4519750
		//   https://bugs.launchpad.net/ubuntu/+source/keepass2/+bug/891029
		//   https://sourceforge.net/p/keepass/bugs/1256/
		//   https://sourceforge.net/p/keepass/bugs/1566/
		//   https://sourceforge.net/p/keepass/bugs/1634/
		// 836428016:
		//   ListView group header selection unsupported.
		//   https://sourceforge.net/p/keepass/discussion/329221/thread/31dae0f0/
		// 2449941153:
		//   RichTextBox doesn't properly escape '}' when generating RTF data.
		//   https://sourceforge.net/p/keepass/discussion/329221/thread/920722a1/
		// 3471228285:
		//   Mono requires command line arguments to be encoded differently.
		//   https://sourceforge.net/p/keepass/discussion/329221/thread/cee6bd7d/
		// 3574233558:
		//   Problems with minimizing windows, no content rendered.
		//   https://sourceforge.net/p/keepass/discussion/329220/thread/d50a79d6/
		public static bool IsRequired(uint uBugID)
		{
			if(!MonoWorkarounds.IsRequired()) return false;

			bool bForce;
			if(g_dForceReq.TryGetValue(uBugID, out bForce)) return bForce;

			ulong v = NativeLib.MonoVersion;
			if(v != 0)
			{
				if(uBugID == 10163)
					return (v >= 0x0002000B00000000UL); // >= 2.11
			}

			return true;
		}

		internal static void SetEnabled(string strIDs, bool bEnabled)
		{
			if(string.IsNullOrEmpty(strIDs)) return;

			string[] vIDs = strIDs.Split(new char[] { ',' });
			foreach(string strID in vIDs)
			{
				if(string.IsNullOrEmpty(strID)) continue;

				uint uID;
				if(StrUtil.TryParseUInt(strID.Trim(), out uID))
					g_dForceReq[uID] = bEnabled;
			}
		}

		internal static void Initialize()
		{
			Terminate();

			// g_fOwnWindow = fOwnWindow;

			if(IsRequired(1530))
			{
				try
				{
					ThreadStart ts = new ThreadStart(MonoWorkarounds.FixClipThread);
					g_thFixClip = new Thread(ts);
					g_thFixClip.Start();
				}
				catch(Exception) { Debug.Assert(false); }
			}

#if DEBUG_BREAKONFAIL
			if(IsRequired() && (g_tlBreak == null))
			{
				g_tlBreak = new DebugBreakTraceListener();
				Debug.Listeners.Add(g_tlBreak);
			}
#endif
		}

		internal static void Terminate()
		{
			if(g_thFixClip != null)
			{
				try { g_thFixClip.Abort(); }
				catch(Exception) { Debug.Assert(false); }

				g_thFixClip = null;
			}
		}

		private static void FixClipThread()
		{
			try
			{
#if !KeePassUAP
				const int msDelay = 250;

				string strTest = ClipboardU.GetText();
				if(strTest == null) return; // No clipboard support

				// Without XDoTool, the workaround would be applied to
				// all applications, which may corrupt the clipboard
				// when it doesn't contain simple text only;
				// https://sourceforge.net/p/keepass/bugs/1603/#a113
				strTest = (NativeLib.RunConsoleApp(AppXDoTool,
					"help") ?? string.Empty).Trim();
				if(strTest.Length == 0) return;

				Thread.Sleep(msDelay);

				string strLast = null;
				while(true)
				{
					string str = ClipboardU.GetText();
					if(str == null) { Debug.Assert(false); }
					else if(str != strLast)
					{
						if(NeedClipboardWorkaround())
							ClipboardU.SetText(str, true);

						strLast = str;
					}

					Thread.Sleep(msDelay);
				}
#endif
			}
			catch(ThreadAbortException)
			{
				try { Thread.ResetAbort(); }
				catch(Exception) { Debug.Assert(false); }
			}
			catch(Exception) { Debug.Assert(false); }
			finally { g_thFixClip = null; }
		}

#if !KeePassUAP
		private static bool NeedClipboardWorkaround()
		{
			try
			{
				string strHandle = (NativeLib.RunConsoleApp(AppXDoTool,
					"getactivewindow") ?? string.Empty).Trim();
				if(strHandle.Length == 0) { Debug.Assert(false); return false; }

				// IntPtr h = new IntPtr(long.Parse(strHandle));
				long.Parse(strHandle); // Validate

				// Detection of own windows based on Form.Handle
				// comparisons doesn't work reliably (Mono's handles
				// are usually off by 1)
				// Predicate<IntPtr> fOwnWindow = g_fOwnWindow;
				// if(fOwnWindow != null)
				// {
				//	if(fOwnWindow(h)) return true;
				// }
				// else { Debug.Assert(false); }

				string strWmClass = (NativeLib.RunConsoleApp("xprop",
					"-id " + strHandle + " WM_CLASS") ?? string.Empty);

				if(strWmClass.IndexOf("\"" + PwDefs.ResClass + "\"",
					StrUtil.CaseIgnoreCmp) >= 0) return true;
				if(strWmClass.IndexOf("\"Remmina\"",
					StrUtil.CaseIgnoreCmp) >= 0) return true;
			}
			catch(ThreadAbortException) { throw; }
			catch(Exception) { Debug.Assert(false); }

			return false;
		}

		public static void ApplyTo(Form f)
		{
			if(!MonoWorkarounds.IsRequired()) return;
			if(f == null) { Debug.Assert(false); return; }

#if !KeePassLibSD
			f.HandleCreated += MonoWorkarounds.OnFormHandleCreated;
			SetWmClass(f);

			ApplyToControlsRec(f.Controls, f, MonoWorkarounds.ApplyToControl);
#endif
		}

		public static void Release(Form f)
		{
			if(!MonoWorkarounds.IsRequired()) return;
			if(f == null) { Debug.Assert(false); return; }

#if !KeePassLibSD
			f.HandleCreated -= MonoWorkarounds.OnFormHandleCreated;

			ApplyToControlsRec(f.Controls, f, MonoWorkarounds.ReleaseControl);
#endif
		}

#if !KeePassLibSD
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

		private static void ApplyToControl(Control c, Form fContext)
		{
			Button btn = (c as Button);
			if(btn != null) ApplyToButton(btn, fContext);

			NumericUpDown nc = (c as NumericUpDown);
			if((nc != null) && MonoWorkarounds.IsRequired(1254))
			{
				if(nc.TextAlign == HorizontalAlignment.Right)
					nc.TextAlign = HorizontalAlignment.Left;
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
					new object[] { btn, e });

			// Raise close events, if the click event handler hasn't
			// reset the dialog result
			if((f != null) && (f.DialogResult == hi.Result))
				f.DialogResult = hi.Result; // Raises close events
		}

		private static void SetWmClass(Form f)
		{
			NativeMethods.SetWmClass(f, PwDefs.UnixName, PwDefs.ResClass);
		}

		private static void OnFormHandleCreated(object sender, EventArgs e)
		{
			Form f = (sender as Form);
			if(f == null) { Debug.Assert(false); return; }

			if(!f.IsHandleCreated) return; // Prevent infinite loop

			SetWmClass(f);
		}

		/// <summary>
		/// Set the value of the private <c>shown_raised</c> member
		/// variable of a form.
		/// </summary>
		/// <returns>Previous <c>shown_raised</c> value.</returns>
		internal static bool ExchangeFormShownRaised(Form f, bool bNewValue)
		{
			if(f == null) { Debug.Assert(false); return true; }

			try
			{
				FieldInfo fi = typeof(Form).GetField("shown_raised",
					BindingFlags.Instance | BindingFlags.NonPublic);
				if(fi == null) { Debug.Assert(false); return true; }

				bool bPrevious = (bool)fi.GetValue(f);

				fi.SetValue(f, bNewValue);

				return bPrevious;
			}
			catch(Exception) { Debug.Assert(false); }

			return true;
		}
#endif

		/// <summary>
		/// Ensure that the file ~/.recently-used is valid (in order to
		/// prevent Mono's FileDialog from crashing).
		/// </summary>
		internal static void EnsureRecentlyUsedValid()
		{
			if(!MonoWorkarounds.IsRequired(1358)) return;

			try
			{
				string strFile = Environment.GetFolderPath(
					Environment.SpecialFolder.Personal);
				strFile = UrlUtil.EnsureTerminatingSeparator(strFile, false);
				strFile += ".recently-used";

				if(File.Exists(strFile))
				{
					try
					{
						// Mono's WriteRecentlyUsedFiles method also loads the
						// XML file using XmlDocument
						XmlDocument xd = XmlUtilEx.CreateXmlDocument();
						xd.Load(strFile);
					}
					catch(Exception) // The XML file is invalid
					{
						File.Delete(strFile);
					}
				}
			}
			catch(Exception) { Debug.Assert(false); }
		}
#endif // !KeePassUAP

#if DEBUG_BREAKONFAIL
		private sealed class DebugBreakTraceListener : TraceListener
		{
			public override void Fail(string message)
			{
				Debugger.Break();
			}

			public override void Fail(string message, string detailMessage)
			{
				Debugger.Break();
			}

			public override void Write(string message) { }
			public override void WriteLine(string message) { }
		}
#endif
	}
}
