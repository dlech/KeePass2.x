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

/*
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

using KeePassLib;
using KeePassLib.Interfaces;

namespace KeePass.Native
{
	public sealed class NativeProgressDialog : IStatusLogger
	{
		private IProgressDialog m_p = null;
		private uint m_uFlags = 0;

		private const uint PROGDLG_NORMAL = 0x00000000;
		private const uint PROGDLG_MODAL = 0x00000001;
		private const uint PROGDLG_AUTOTIME = 0x00000002;
		private const uint PROGDLG_NOTIME = 0x00000004;
		private const uint PROGDLG_NOMINIMIZE = 0x00000008;
		private const uint PROGDLG_NOPROGRESSBAR = 0x00000010;
		private const uint PROGDLG_MARQUEEPROGRESS = 0x00000020;
		private const uint PROGDLG_NOCANCEL = 0x00000040;

		public NativeProgressDialog(bool bAutoTime, bool bNoTime, bool bMarquee,
			bool bNoCancel)
		{
			if(bAutoTime) m_uFlags |= PROGDLG_AUTOTIME;
			if(bNoTime) m_uFlags |= PROGDLG_NOTIME;
			if(bMarquee) m_uFlags |= PROGDLG_MARQUEEPROGRESS;
			if(bNoCancel) m_uFlags |= PROGDLG_NOCANCEL;

			try { m_p = (IProgressDialog)(new Win32ProgressDialog()); }
			catch(Exception) { Debug.Assert(false); }
		}

		~NativeProgressDialog()
		{
			EndLogging();
		}

		public void StartLogging(string strOperation, bool bWriteOperationToLog)
		{
			if(m_p != null)
			{
				m_p.SetTitle(PwDefs.ShortProductName);

				m_p.StartProgressDialog(IntPtr.Zero, IntPtr.Zero, m_uFlags, IntPtr.Zero);

				m_p.SetLine(1, strOperation, false, IntPtr.Zero);
			}
		}

		public void EndLogging()
		{
			if(m_p != null)
			{
				m_p.StopProgressDialog();
				try { Marshal.ReleaseComObject(m_p); }
				catch(Exception) { Debug.Assert(false); }
				m_p = null;
			}
		}

		public bool SetProgress(uint uPercent)
		{
			if(m_p != null)
			{
				m_p.SetProgress(uPercent, 100);
				return !m_p.HasUserCancelled();
			}

			return true;
		}

		public bool SetText(string strNewText, LogStatusType lsType)
		{
			if(m_p != null)
			{
				m_p.SetLine(2, strNewText, false, IntPtr.Zero);
				return !m_p.HasUserCancelled();
			}

			return true;
		}

		public bool ContinueWork()
		{
			if(m_p != null) return !m_p.HasUserCancelled();

			return true;
		}
	}

	[ComImport]
	[Guid("EBBC7C04-315E-11D2-B62F-006097DF5BD4")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IProgressDialog
	{
		void StartProgressDialog(IntPtr hwndParent, IntPtr punkEnableModless,
			uint dwFlags, IntPtr pvReserved);
		void StopProgressDialog();

		void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pwzTitle);
		void SetAnimation(IntPtr hInstAnimation, uint idAnimation);

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Bool)]
		bool HasUserCancelled();

		void SetProgress(uint dwCompleted, uint dwTotal);
		void SetProgress64(ulong ullCompleted, ulong ullTotal);

		void SetLine(uint dwLineNum, [MarshalAs(UnmanagedType.LPWStr)] string pwzString,
			[MarshalAs(UnmanagedType.Bool)] bool fCompactPath, IntPtr pvReserved);
		void SetCancelMsg([MarshalAs(UnmanagedType.LPWStr)] string pwzCancelMsg,
			IntPtr pvReserved);

		void Timer(uint dwTimerAction, IntPtr pvReserved);
	}

	[ComImport]
	[Guid("F8383852-FCD3-11D1-A6B9-006097DF5BD4")]
	internal class Win32ProgressDialog { }
}
*/
