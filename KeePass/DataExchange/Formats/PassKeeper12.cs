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
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using KeePass.App;
using KeePass.Resources;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	// 1.2
	internal sealed class PassKeeper12 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "PassKeeper"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override bool ImportAppendsToRootGroupOnly { get { return true; } }
		public override bool RequiresFile { get { return false; } }

		public override Image SmallIcon
		{
			get { return KeePass.Properties.Resources.B16x16_View_Detailed; }
		}

		public override void Import(PwDatabase pdStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			if(!MessageService.AskYesNo(KPRes.ImportMustRead + MessageService.NewParagraph +
				KPRes.ImportMustReadQuestion))
			{
				AppHelp.ShowHelp(AppDefs.HelpTopics.ImportExport,
					AppDefs.HelpTopics.ImportExportPassKeeper);
				return;
			}

			PwEntry pePrev = new PwEntry(true, true);

			for(int i = 0; i < 20; ++i)
			{
				Thread.Sleep(500);
				Application.DoEvents();
			}

			try
			{
				while(true)
				{
					PwEntry pe = ImportEntry(pdStorage);

					if(ImportUtil.EntryEquals(pe, pePrev))
					{
						if(pe.ParentGroup != null) // Remove duplicate
							pe.ParentGroup.Entries.Remove(pe);
						break;
					}

					ImportUtil.GuiSendKeysPrc(@"{DOWN}");
					pePrev = pe;
				}

				MessageService.ShowInfo(KPRes.ImportFinished);
			}
			catch(Exception ex) { MessageService.ShowWarning(ex); }
		}

		private static PwEntry ImportEntry(PwDatabase pd)
		{
			ImportUtil.GuiSendWaitWindowChange(@"{ENTER}");
			Thread.Sleep(250);
			ImportUtil.GuiSendKeysPrc(string.Empty); // Process messages

			string strTitle = ImportUtil.GuiSendRetrieve(string.Empty);
			string strUserName = ImportUtil.GuiSendRetrieve(@"{TAB}");
			string strPassword = ImportUtil.GuiSendRetrieve(@"{TAB}");
			string strNotes = ImportUtil.GuiSendRetrieve(@"{TAB}");

			ImportUtil.GuiSendWaitWindowChange(@"{ESC}");

			PwEntry pe = new PwEntry(true, true);
			pd.RootGroup.AddEntry(pe, true);

			ImportUtil.Add(pe, PwDefs.TitleField, strTitle, pd);
			ImportUtil.Add(pe, PwDefs.UserNameField, strUserName, pd);
			ImportUtil.Add(pe, PwDefs.PasswordField, strPassword, pd);
			ImportUtil.Add(pe, PwDefs.NotesField, strNotes, pd);

			return pe;
		}
	}
}
