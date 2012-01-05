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
using System.Diagnostics;
using System.Threading;
using System.Drawing;

using KeePass.Native;
using KeePass.Util;

using KeePassLib.Utility;

namespace KeePass.UI
{
	public delegate Form UIFormConstructor(object objParam);
	public delegate object UIFormResultBuilder(Form f);

	public sealed class ProtectedDialog
	{
		private UIFormConstructor m_fnConstruct;
		private UIFormResultBuilder m_fnResultBuilder;

		private sealed class SecureThreadParams
		{
			public Bitmap BackgroundBitmap = null;
			public IntPtr ThreadDesktop = IntPtr.Zero;

			public object FormConstructParam = null;

			public DialogResult DialogResult = DialogResult.None;
			public object ResultObject = null;
		}

		public ProtectedDialog(UIFormConstructor fnConstruct,
			UIFormResultBuilder fnResultBuilder)
		{
			if(fnConstruct == null) { Debug.Assert(false); throw new ArgumentNullException("fnConstruct"); }
			if(fnResultBuilder == null) { Debug.Assert(false); throw new ArgumentNullException("fnResultBuilder"); }

			m_fnConstruct = fnConstruct;
			m_fnResultBuilder = fnResultBuilder;
		}

		public DialogResult ShowDialog(out object objResult, object objConstructParam)
		{
			objResult = null;

			ProcessMessagesEx();

			ClipboardEventChainBlocker ccb = new ClipboardEventChainBlocker();
			byte[] pbClipHash = ClipboardUtil.ComputeHash();

			Bitmap bmpBack = UIUtil.CreateScreenshot();
			if(bmpBack != null) UIUtil.DimImage(bmpBack);

			DialogResult dr = DialogResult.None;
			try
			{
				uint uOrgThreadId = NativeMethods.GetCurrentThreadId();
				IntPtr pOrgDesktop = NativeMethods.GetThreadDesktop(uOrgThreadId);

				string strName = "D" + Convert.ToBase64String(
					Guid.NewGuid().ToByteArray(), Base64FormattingOptions.None);
				strName = strName.Replace(@"+", string.Empty);
				strName = strName.Replace(@"/", string.Empty);
				strName = strName.Replace(@"=", string.Empty);

				NativeMethods.DesktopFlags deskFlags =
					NativeMethods.DesktopFlags.CreateMenu |
					NativeMethods.DesktopFlags.CreateWindow |
					NativeMethods.DesktopFlags.ReadObjects |
					NativeMethods.DesktopFlags.WriteObjects |
					NativeMethods.DesktopFlags.SwitchDesktop;

				IntPtr pNewDesktop = NativeMethods.CreateDesktop(strName,
					null, IntPtr.Zero, 0, deskFlags, IntPtr.Zero);
				if(pNewDesktop == IntPtr.Zero)
					throw new InvalidOperationException();

				SecureThreadParams stp = new SecureThreadParams();
				stp.BackgroundBitmap = bmpBack;
				stp.ThreadDesktop = pNewDesktop;
				stp.FormConstructParam = objConstructParam;

				Thread th = new Thread(this.SecureDialogThread);
				th.CurrentCulture = Thread.CurrentThread.CurrentCulture;
				th.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
				th.Start(stp);
				th.Join();

				if(!NativeMethods.SwitchDesktop(pOrgDesktop)) { Debug.Assert(false); }
				NativeMethods.SetThreadDesktop(pOrgDesktop);

				if(!NativeMethods.CloseDesktop(pNewDesktop)) { Debug.Assert(false); }
				NativeMethods.CloseDesktop(pOrgDesktop);

				dr = stp.DialogResult;
				objResult = stp.ResultObject;
			}
			catch(Exception) { Debug.Assert(false); }

			byte[] pbNewClipHash = ClipboardUtil.ComputeHash();
			if((pbClipHash != null) && (pbNewClipHash != null) &&
				!MemUtil.ArraysEqual(pbClipHash, pbNewClipHash))
				ClipboardUtil.Clear();
			ccb.Release();

			if(bmpBack != null) bmpBack.Dispose();

			// If something failed, show the dialog on the normal desktop
			if(dr == DialogResult.None)
			{
				Form f = m_fnConstruct(objConstructParam);
				dr = f.ShowDialog();
				objResult = m_fnResultBuilder(f);
				UIUtil.DestroyForm(f);
			}

			return dr;
		}

		private static void ProcessMessagesEx()
		{
			Application.DoEvents();
			Thread.Sleep(5);
			Application.DoEvents();
		}

		private void SecureDialogThread(object oParam)
		{
			BackgroundForm formBack = null;

			try
			{
				SecureThreadParams stp = (oParam as SecureThreadParams);
				if(stp == null) { Debug.Assert(false); return; }

				if(!NativeMethods.SetThreadDesktop(stp.ThreadDesktop))
				{
					Debug.Assert(false);
					return;
				}

				ProcessMessagesEx();

				// Test whether we're really on the secure desktop
				if(NativeMethods.GetThreadDesktop(NativeMethods.GetCurrentThreadId()) !=
					stp.ThreadDesktop)
				{
					Debug.Assert(false);
					return;
				}

				// Creating a window on the new desktop spawns a CtfMon.exe child
				// process by default. On Windows Vista, this process is terminated
				// correctly when the desktop is closed. However, on Windows 7 it
				// isn't terminated (probably a bug); creating multiple desktops
				// accumulates CtfMon.exe child processes. In order to prevent this,
				// we simply disable IME for the new desktop thread (CtfMon.exe then
				// isn't loaded automatically).
				try { NativeMethods.ImmDisableIME(0); } // Always false on 2000/XP
				catch(Exception) { Debug.Assert(!WinUtil.IsAtLeastWindows2000); }

				ProcessMessagesEx();

				formBack = new BackgroundForm(stp.BackgroundBitmap);
				formBack.Show();

				ProcessMessagesEx();

				if(!NativeMethods.SwitchDesktop(stp.ThreadDesktop)) { Debug.Assert(false); }

				ProcessMessagesEx();

				Form f = m_fnConstruct(stp.FormConstructParam);
				if(f == null) { Debug.Assert(false); return; }

				if(Program.Config.UI.SecureDesktopPlaySound)
					UIUtil.PlayUacSound();

				stp.DialogResult = f.ShowDialog(formBack);
				stp.ResultObject = m_fnResultBuilder(f);

				UIUtil.DestroyForm(f);
			}
			catch(Exception) { Debug.Assert(false); }
			finally
			{
				if(formBack != null)
				{
					try
					{
						formBack.Close();
						UIUtil.DestroyForm(formBack);
					}
					catch(Exception) { Debug.Assert(false); }
				}
			}
		}

		/* private static void BlockPrintScreen(Form f, bool bBlock)
		{
			if(f == null) { Debug.Assert(false); return; }

			try
			{
				if(bBlock)
				{
					NativeMethods.RegisterHotKey(f.Handle, NativeMethods.IDHOT_SNAPDESKTOP,
						0, NativeMethods.VK_SNAPSHOT);
					NativeMethods.RegisterHotKey(f.Handle, NativeMethods.IDHOT_SNAPWINDOW,
						NativeMethods.MOD_ALT, NativeMethods.VK_SNAPSHOT);
				}
				else
				{
					NativeMethods.UnregisterHotKey(f.Handle, NativeMethods.IDHOT_SNAPWINDOW);
					NativeMethods.UnregisterHotKey(f.Handle, NativeMethods.IDHOT_SNAPDESKTOP);
				}
			}
			catch(Exception) { Debug.Assert(false); }
		} */
	}
}
