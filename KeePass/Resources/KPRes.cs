/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2025 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePass.App;

using KeePassLib.Resources;
using KeePassLib.Utility;

namespace KeePass.Resources
{
	public static partial class KPRes
	{
		// Obsoletes: for backward compatibility with plugins

		[Obsolete]
		public static string AlternatingBgColors
		{
			get { return string.Empty; }
		}

		[Obsolete]
		public static string ChangeMasterKeyIntroShort
		{
			get { return string.Empty; }
		}

		[Obsolete]
		public static string CreateNewDatabase
		{
			get { return KPRes.CreateNewDatabase2; }
		}

		public static string DeleteEntriesTitle
		{
			get { return StrUtil.CommandToText(KPRes.DeleteEntriesCmd); }
		}

		public static string DeleteEntriesTitleSingle
		{
			get { return StrUtil.CommandToText(KPRes.DeleteEntryCmd); }
		}

		[Obsolete]
		public static string EditCmd
		{
			get { return @"&Edit"; }
		}

		public static string EditEntries
		{
			get { return StrUtil.CommandToText(KPRes.EditEntriesCmd); }
		}

		public static string EditEntry
		{
			get { return StrUtil.CommandToText(KPRes.EditEntryCmd); }
		}

		[Obsolete]
		public static string GeneratedPasswordSamples
		{
			get { return KPRes.GeneratedPasswords; }
		}

		[Obsolete]
		public static string HmacOtp
		{
			get { return string.Empty; } // "HMAC-Based OTP"
		}

		[Obsolete]
		public static string InvalidUserPassword
		{
			get { return (KPRes.Invalid + " (" + KPRes.UserName + " / " + KPRes.Password + ")!"); }
		}

		[Obsolete]
		public static string NewDatabaseFileName
		{
			get { return (KPRes.Database + "." + AppDefs.FileExtension.FileExt); }
		}

		[Obsolete]
		public static string ShowObject
		{
			get { return @"{PARAM}"; } // "Show {PARAM}"
		}

		[Obsolete]
		public static string SslCertsAcceptInvalid
		{
			get { return KPRes.TlsCertsAcceptInvalid; }
		}

		[Obsolete]
		public static string TimeOtp
		{
			get { return string.Empty; } // "Time-Based OTP"
		}

		[Obsolete]
		public static string ToolBarNew
		{
			get { return KPRes.NewDatabase; }
		}

		[Obsolete]
		public static string ToolBarOpen
		{
			get { return KPRes.OpenDatabase; }
		}

		[Obsolete]
		public static string ToolBarSaveAll
		{
			get { return KPRes.SaveAllDatabases; }
		}

		[Obsolete]
		public static string UnknownError
		{
			get { return KLRes.UnknownError; }
		}

		[Obsolete]
		public static string UserNamePrompt
		{
			get { return (KPRes.UserNameStc + ":"); }
		}

		[Obsolete]
		public static string WebSiteLogin
		{
			get { return "Web Site Login"; }
		}

		[Obsolete]
		public static string WebSites
		{
			get { return "Web Sites"; }
		}
	}
}
