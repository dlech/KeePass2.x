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
using System.IO;
using System.Drawing;

using KeePass.Forms;
using KeePass.Resources;

using KeePassLib;
using KeePassLib.Interfaces;

namespace KeePass.DataExchange.Formats
{
	public sealed class GenericCsv : FormatImporter
	{
		public override string FormatName { get { return "CSV / Text File"; } }
		public override string DisplayName { get { return "Generic CSV Importer"; } }
		public override string DefaultExtension { get { return @"*"; } }
		public override string AppGroup { get { return KPRes.General; } }

		public override Image SmallIcon
		{
			get { return KeePass.Properties.Resources.B16x16_ASCII; }
		}

		public override void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			BinaryReader br = new BinaryReader(sInput);
			byte[] pbData = br.ReadBytes((int)sInput.Length);
			br.Close();

			ImportCsvForm csv = new ImportCsvForm();
			csv.InitEx(pwStorage, pbData);
			csv.ShowDialog();
		}
	}
}
