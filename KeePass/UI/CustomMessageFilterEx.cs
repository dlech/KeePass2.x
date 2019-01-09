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

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using KeePass.Native;
using KeePass.Util;

namespace KeePass.UI
{
	internal sealed class CustomMessageFilterEx : IMessageFilter
	{
		public bool PreFilterMessage(ref Message m)
		{
			if((m.Msg == NativeMethods.WM_KEYDOWN) || (m.Msg == NativeMethods.WM_LBUTTONDOWN) ||
				(m.Msg == NativeMethods.WM_RBUTTONDOWN) || (m.Msg == NativeMethods.WM_MBUTTONDOWN))
			{
				Program.NotifyUserActivity();
			}

			// Workaround for .NET 4.6 overflow bug in InputLanguage.Culture
			// (handle casted to Int32 without the 'unchecked' keyword);
			// https://sourceforge.net/p/keepass/bugs/1598/
			// https://stackoverflow.com/questions/25619831/arithmetic-operation-resulted-in-an-overflow-in-inputlanguagechangingeventargs
			// https://referencesource.microsoft.com/#System.Windows.Forms/winforms/Managed/System/WinForms/InputLanguage.cs
			if(m.Msg == NativeMethods.WM_INPUTLANGCHANGEREQUEST)
			{
				long l = m.LParam.ToInt64();
				if((l < (long)int.MinValue) || (l > (long)int.MaxValue))
					return true; // Ignore it (better than an exception)
			}

			return false;
		}
	}
}
