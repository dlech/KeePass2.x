/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2007 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.IO;
using System.Drawing;
using System.Threading;

using KeePass.App;
using KeePass.Native;
using KeePass.Resources;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	public sealed class SteganosPwManager2007 : FormatImporter
	{
		public override string FormatName { get { return "Steganos Password Manager 2007"; } }
		public override string DefaultExtension { get { return ""; } }
		public override string AppGroup { get { return KPRes.PasswordManagers; } }

		public override bool RequiresFile { get { return false; } }

		public override Image SmallIcon
		{
			get { return KeePass.Properties.Resources.B16x16_Imp_Steganos; }
		}

		public override void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			if(MessageService.AskYesNo(KPRes.ImportMustRead + MessageService.NewParagraph +
				KPRes.ImportMustReadQuestion) == false)
			{
				AppHelp.ShowHelp(AppDefs.HelpTopics.ImportExport,
					AppDefs.HelpTopics.ImportExportSteganos);
				return;
			}

			PwEntry pePrev = new PwEntry(null, true, true);

			for(int i = 0; i < 20; ++i)
			{
				Thread.Sleep(500);
				Application.DoEvents();
			}

			while(true)
			{
				PwEntry pe = ImportEntry(pwStorage);

				if(EntryEquals(pe, pePrev))
				{
					if(pe.ParentGroup != null) // Remove duplicate
						pe.ParentGroup.Entries.Remove(pe);
					break;
				}

				SendKeysPrc(@"{DOWN}");
				pePrev = pe;
			}

			MessageService.ShowInfo(KPRes.ImportFinished);
		}

		private static bool EntryEquals(PwEntry pe1, PwEntry pe2)
		{
			if(pe1.ParentGroup == null) return false;
			if(pe2.ParentGroup == null) return false;

			if(pe1.ParentGroup.Name != pe2.ParentGroup.Name)
				return false;

			if(pe1.Strings.ReadSafe(PwDefs.TitleField) !=
				pe2.Strings.ReadSafe(PwDefs.TitleField))
			{
				return false;
			}

			if(pe1.Strings.ReadSafe(PwDefs.UserNameField) !=
				pe2.Strings.ReadSafe(PwDefs.UserNameField))
			{
				return false;
			}
			
			if(pe1.Strings.ReadSafe(PwDefs.PasswordField) !=
				pe2.Strings.ReadSafe(PwDefs.PasswordField))
			{
				return false;
			}
			
			if(pe1.Strings.ReadSafe(PwDefs.UrlField) !=
				pe2.Strings.ReadSafe(PwDefs.UrlField))
			{
				return false;
			}

			if(pe1.Strings.ReadSafe(PwDefs.NotesField) !=
				pe2.Strings.ReadSafe(PwDefs.NotesField))
			{
				return false;
			}

			return true;
		}

		private static PwEntry ImportEntry(PwDatabase pwDb)
		{
			SendWaitWindowChange(@"{ENTER}");
			Thread.Sleep(1000);
			SendKeysPrc(string.Empty);

			string strTitle = SendRetrieve(string.Empty);
			string strGroup = SendRetrieve(@"{TAB}");
			string strUserName = SendRetrieve(@"{TAB}");
			SendKeysPrc(@"{TAB}{TAB}");
			SendKeysPrc(@" ");
			SendKeysPrc(@"+({TAB})");
			string strPassword = SendRetrieve(string.Empty);
			SendKeysPrc(@"{TAB} ");
			string strNotes = SendRetrieve(@"{TAB}{TAB}");
			
			string strUrl = SendRetrieve(@"{TAB}");
			string strUrl2 = SendRetrieve(@"{TAB}");

			SendWaitWindowChange(@"{ESC}");

			if(strGroup.Length == 0) strGroup = "Steganos";

			PwGroup pg = pwDb.RootGroup.FindCreateGroup(strGroup, true);
			PwEntry pe = new PwEntry(pg, true, true);
			pg.Entries.Add(pe);

			pe.Strings.Set(PwDefs.TitleField, new ProtectedString(
				pwDb.MemoryProtection.ProtectTitle, strTitle));
			pe.Strings.Set(PwDefs.UserNameField, new ProtectedString(
				pwDb.MemoryProtection.ProtectUserName, strUserName));
			pe.Strings.Set(PwDefs.PasswordField, new ProtectedString(
				pwDb.MemoryProtection.ProtectPassword, strPassword));
			pe.Strings.Set(PwDefs.NotesField, new ProtectedString(
				pwDb.MemoryProtection.ProtectNotes, strNotes));

			if(strUrl.Length > 0)
				pe.Strings.Set(PwDefs.UrlField, new ProtectedString(
					pwDb.MemoryProtection.ProtectUrl, strUrl));
			else
				pe.Strings.Set(PwDefs.UrlField, new ProtectedString(
					pwDb.MemoryProtection.ProtectUrl, strUrl2));

			return pe;
		}

		private static void SendWaitWindowChange(string strSend)
		{
			IntPtr ptrCur = NativeMethods.GetForegroundWindow();

			SendKeysPrc(strSend);

			int nRound = 0;
			while(true)
			{
				Application.DoEvents();

				IntPtr ptr = NativeMethods.GetForegroundWindow();
				if(ptr != ptrCur) break;

				++nRound;
				if(nRound > 1000)
					throw new InvalidOperationException();

				Thread.Sleep(50);
			}

			Thread.Sleep(100);
			Application.DoEvents();
		}

		private static string SendRetrieve(string strSendPrefix)
		{
			if(strSendPrefix.Length > 0)
				SendKeysPrc(strSendPrefix);

			return RetrieveDataField();
		}

		private static string RetrieveDataField()
		{
			Clipboard.Clear();
			Application.DoEvents();

			SendKeysPrc(@"^c");

			if(Clipboard.ContainsText())
				return Clipboard.GetText();

			return string.Empty;
		}

		private static void SendKeysPrc(string strSend)
		{
			if(strSend.Length > 0)
				SendInputEx.SendKeysWait(strSend, false);

			Application.DoEvents();
			Thread.Sleep(100);
			Application.DoEvents();
		}
	}
}
