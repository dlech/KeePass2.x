/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2008 Dominik Reichl <dominik.reichl@t-online.de>

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
			Application.DoEvents();

			Bitmap bmpBack = UIUtil.CreateScreenshot();
			UIUtil.DimImage(bmpBack);

			DialogResult dr = DialogResult.None;

			try
			{
				uint uOrgThreadId = NativeMethods.GetCurrentThreadId();
				IntPtr pOrgDesktop = NativeMethods.GetThreadDesktop(uOrgThreadId);

				string strName = "D" + Guid.NewGuid().ToString();
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

				SecureThreadParams stp = new SecureThreadParams();
				stp.BackgroundBitmap = bmpBack;
				stp.ThreadDesktop = pNewDesktop;

				Thread th = new Thread(this.SecureDialogThread);
				th.CurrentCulture = Thread.CurrentThread.CurrentCulture;
				th.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
				th.Start(stp);
				th.Join();

				NativeMethods.SwitchDesktop(pOrgDesktop);
				NativeMethods.SetThreadDesktop(pOrgDesktop);

				NativeMethods.CloseDesktop(pNewDesktop);
				NativeMethods.CloseDesktop(pOrgDesktop);

				dr = stp.Result;
			}
			catch(Exception) { Debug.Assert(false); }

			return dr;
		}

		private void SecureDialogThread(object oParam)
		{
			try
			{
				SecureThreadParams stp = oParam as SecureThreadParams;
				if(stp == null) { Debug.Assert(false); return; }

				NativeMethods.SetThreadDesktop(stp.ThreadDesktop);

				BackgroundForm formBack = new BackgroundForm(
					stp.BackgroundBitmap);
				formBack.Show();

				NativeMethods.SwitchDesktop(stp.ThreadDesktop);

				stp.Result = m_form.ShowDialog(formBack);

				formBack.Hide();
			}
			catch(Exception) { Debug.Assert(false); }
		}
	}
}
