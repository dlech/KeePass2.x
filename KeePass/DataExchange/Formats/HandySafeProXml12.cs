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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;

using KeePass.Resources;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	[XmlRoot("HandySafe")]
	public sealed class HspFolder
	{
		[XmlAttribute("name")]
		public string Name { get; set; }

		[XmlElement("Folder")]
		public HspFolder[] Folders { get; set; }

		[XmlElement("Card")]
		public HspCard[] Cards { get; set; }
	}

	public sealed class HspCard
	{
		[XmlAttribute("name")]
		public string Name { get; set; }

		[XmlElement("Field")]
		public HspField[] Fields { get; set; }

		public string Note { get; set; }
	}

	public sealed class HspField
	{
		[XmlAttribute("name")]
		public string Name { get; set; }

		[XmlText]
		public string Value { get; set; }
	}

	// 1.2-3.01+
	internal sealed class HandySafeProXml12 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Handy Safe Pro XML"; } }
		public override string DefaultExtension { get { return "xml"; } }
		public override string ApplicationGroup { get { return KPRes.PasswordManagers; } }

		public override void Import(PwDatabase pdStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			HspFolder hspRoot = XmlUtilEx.Deserialize<HspFolder>(sInput);

			AddFolder(pdStorage.RootGroup, hspRoot, false, pdStorage);
		}

		private static void AddFolder(PwGroup pgParent, HspFolder hspFolder,
			bool bNewGroup, PwDatabase pd)
		{
			if(hspFolder == null) { Debug.Assert(false); return; }

			PwGroup pg;
			if(bNewGroup)
			{
				pg = new PwGroup(true, true);
				pgParent.AddGroup(pg, true);

				if(!string.IsNullOrEmpty(hspFolder.Name))
					pg.Name = hspFolder.Name;
			}
			else pg = pgParent;

			if(hspFolder.Folders != null)
			{
				foreach(HspFolder fld in hspFolder.Folders)
					AddFolder(pg, fld, true, pd);
			}

			if(hspFolder.Cards != null)
			{
				foreach(HspCard crd in hspFolder.Cards)
					AddCard(pg, crd, pd);
			}
		}

		private static void AddCard(PwGroup pgParent, HspCard hspCard, PwDatabase pd)
		{
			if(hspCard == null) { Debug.Assert(false); return; }

			PwEntry pe = new PwEntry(true, true);
			pgParent.AddEntry(pe, true);

			if(!string.IsNullOrEmpty(hspCard.Name))
				ImportUtil.Add(pe, PwDefs.TitleField, hspCard.Name, pd);

			if(!string.IsNullOrEmpty(hspCard.Note))
				ImportUtil.Add(pe, PwDefs.NotesField, hspCard.Note, pd);

			if(hspCard.Fields == null) return;
			foreach(HspField fld in hspCard.Fields)
			{
				if(fld == null) { Debug.Assert(false); continue; }
				if(string.IsNullOrEmpty(fld.Name) || string.IsNullOrEmpty(fld.Value)) continue;

				string strKey = ImportUtil.MapName(fld.Name, true);

				ImportUtil.Add(pe, strKey, fld.Value, pd);
			}
		}
	}
}
