/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2022 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Security;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using KeePass.Native;
using KeePass.Resources;

using KeePassLib.Utility;

namespace KeePass.Util.SendInputExt
{
	internal abstract class SiEngineStd : ISiEngine
	{
		protected IntPtr TargetWindowHandle = IntPtr.Zero;
		protected string TargetWindowTitle = string.Empty;

		protected bool Cancelled = false;

		private Stopwatch m_swLastEvent = new Stopwatch();
#if DEBUG
		private List<long> m_lDelaysRec = new List<long>();
#endif

		public virtual void Init()
		{
			Debug.Assert(!m_swLastEvent.IsRunning);

			UpdateExpectedFocus();
		}

		public virtual void Release()
		{
			m_swLastEvent.Stop();
		}

		public void UpdateExpectedFocus()
		{
			try
			{
				IntPtr hWnd;
				string strTitle;
				NativeMethods.GetForegroundWindowInfo(out hWnd, out strTitle, false);

				this.TargetWindowHandle = hWnd;
				this.TargetWindowTitle = (strTitle ?? string.Empty);
			}
			catch(Exception) { Debug.Assert(false); }
		}

		public abstract void SendKeyImpl(int iVKey, bool? obExtKey, bool? obDown);
		public abstract void SetKeyModifierImpl(Keys kMod, bool bDown);
		public abstract void SendCharImpl(char ch, bool? obDown);

		private bool PreSendEvent()
		{
			// Update event time *before* actually performing the event
			m_swLastEvent.Reset();
			m_swLastEvent.Start();

			return ValidateState();
		}

		public void SendKey(int iVKey, bool? obExtKey, bool? obDown)
		{
			if(!PreSendEvent()) return;

			SendKeyImpl(iVKey, obExtKey, obDown);

			Application.DoEvents();
		}

		public void SetKeyModifier(Keys kMod, bool bDown)
		{
			if(!PreSendEvent()) return;

			SetKeyModifierImpl(kMod, bDown);

			Application.DoEvents();
		}

		public void SendChar(char ch, bool? obDown)
		{
			if(!PreSendEvent()) return;

			SendCharImpl(ch, obDown);

			Application.DoEvents();
		}

		public virtual void Delay(uint uMs)
		{
			if(this.Cancelled) return;

			if(!m_swLastEvent.IsRunning)
			{
				Thread.Sleep((int)uMs);
				m_swLastEvent.Reset();
				m_swLastEvent.Start();
				return;
			}

			m_swLastEvent.Stop();
			long lAlreadyDelayed = m_swLastEvent.ElapsedMilliseconds;
			long lRemDelay = (long)uMs - lAlreadyDelayed;

			if(lRemDelay >= 0) Thread.Sleep((int)lRemDelay);

#if DEBUG
			m_lDelaysRec.Add(lAlreadyDelayed);
#endif

			m_swLastEvent.Reset();
			m_swLastEvent.Start();
		}

		private bool ValidateState()
		{
			if(this.Cancelled) return false;

			List<string> lAbortWindows = Program.Config.Integration.AutoTypeAbortOnWindows;

			bool bChkWndCh = Program.Config.Integration.AutoTypeCancelOnWindowChange;
			bool bChkTitleCh = Program.Config.Integration.AutoTypeCancelOnTitleChange;
			bool bChkTitleFx = (lAbortWindows.Count != 0);

			if(bChkWndCh || bChkTitleCh || bChkTitleFx)
			{
				IntPtr h = IntPtr.Zero;
				string strTitle = null;
				bool bHasInfo = true;
				try
				{
					NativeMethods.GetForegroundWindowInfo(out h, out strTitle, false);
				}
				catch(Exception) { Debug.Assert(false); bHasInfo = false; }
				if(strTitle == null) strTitle = string.Empty;

				if(bHasInfo)
				{
					if(bChkWndCh && (h != this.TargetWindowHandle))
					{
						this.Cancelled = true;
						return false;
					}

					if(bChkTitleCh && (strTitle != this.TargetWindowTitle))
					{
						this.Cancelled = true;
						return false;
					}

					if(bChkTitleFx)
					{
						string strT = AutoType.NormalizeWindowText(strTitle);

						foreach(string strF in lAbortWindows)
						{
							if(AutoType.IsMatchWindow(strT, strF))
							{
								this.Cancelled = true;
								throw new SecurityException(KPRes.AutoTypeAbortedOnWindow +
									MessageService.NewParagraph + KPRes.TargetWindow +
									@": '" + strTitle + @"'.");
							}
						}
					}
				}
			}

			return true;
		}
	}
}
