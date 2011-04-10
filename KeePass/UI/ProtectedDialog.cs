/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2011 Dominik Reichl <dominik.reichl@t-online.de>

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
	public sealed class ProtectedDialog
	{
		private Form m_form;

		private sealed class SecureThreadParams
		{
			public Bitmap BackgroundBitmap = null;
			public IntPtr ThreadDesktop = IntPtr.Zero;

			public DialogResult Result = DialogResult.None;
		}

		public ProtectedDialog(Form form)
		{
			Debug.Assert(form != null);
			if(form == null) throw new ArgumentNullException("form");

			m_form = form;
		}

		public DialogResult ShowDialog()
		{
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
				{
					Debug.Assert(false);
					return DialogResult.Cancel;
				}

				SecureThreadParams stp = new SecureThreadParams();
				stp.BackgroundBitmap = bmpBack;
				stp.ThreadDesktop = pNewDesktop;

				Thread th = new Thread(this.SecureDialogThread);
				th.CurrentCulture = Thread.CurrentThread.CurrentCulture;
				th.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
				th.Start(stp);
				th.Join();

				if(!NativeMethods.SwitchDesktop(pOrgDesktop)) { Debug.Assert(false); }
				NativeMethods.SetThreadDesktop(pOrgDesktop);

				if(!NativeMethods.CloseDesktop(pNewDesktop)) { Debug.Assert(false); }
				NativeMethods.CloseDesktop(pOrgDesktop);

				dr = stp.Result;
			}
			catch(Exception) { Debug.Assert(false); }

			byte[] pbNewClipHash = ClipboardUtil.ComputeHash();
			if((pbClipHash != null) && (pbNewClipHash != null) &&
				!MemUtil.ArraysEqual(pbClipHash, pbNewClipHash))
				ClipboardUtil.Clear();
			ccb.Release();

			if(bmpBack != null) bmpBack.Dispose();
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
			try
			{
				SecureThreadParams stp = (oParam as SecureThreadParams);
				if(stp == null) { Debug.Assert(false); return; }

				if(!NativeMethods.SetThreadDesktop(stp.ThreadDesktop)) { Debug.Assert(false); }
				Debug.Assert(NativeMethods.GetThreadDesktop(
					NativeMethods.GetCurrentThreadId()) == stp.ThreadDesktop);

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

				BackgroundForm formBack = new BackgroundForm(stp.BackgroundBitmap);
				formBack.Show();

				ProcessMessagesEx();

				if(!NativeMethods.SwitchDesktop(stp.ThreadDesktop)) { Debug.Assert(false); }

				ProcessMessagesEx();

				stp.Result = m_form.ShowDialog(formBack);

				UIUtil.DestroyForm(formBack);
			}
			catch(Exception) { Debug.Assert(false); }
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
