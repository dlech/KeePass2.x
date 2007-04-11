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
using System.Drawing;
using System.Windows.Forms;
using System.IO;

using KeePass.Resources;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	public sealed class KeePassKdb1x : FormatImporter
	{
		public override string FormatName { get { return "KeePass KDB (1.x)"; } }
		public override string DefaultExtension { get { return "kdb"; } }
		public override string AppGroup { get { return PwDefs.ShortProductName; } }

		public override bool SupportsUuids { get { return true; } }
		public override bool RequiresKey { get { return true; } }

		public override Image SmallIcon
		{
			get { return KeePass.Properties.Resources.B16x16_KeePass; }
		}

		public override bool TryBeginImport()
		{
			Exception exLib;
			if(!Kdb3File.IsLibraryInstalled(out exLib))
			{
				MessageService.ShowWarning(KPRes.KeePassLibCNotFound,
					KPRes.KDB3KeePassLibC, exLib);

				return false;
			}

			return true;
		}

		public override void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			string strTempFile = Path.GetTempFileName();
			try { File.Delete(strTempFile); }
			catch(Exception) { }

			BinaryReader br = new BinaryReader(sInput);
			byte[] pb = br.ReadBytes((int)sInput.Length);
			br.Close();
			File.WriteAllBytes(strTempFile, pb);

			Kdb3File kdb3 = new Kdb3File(pwStorage, slLogger);
			kdb3.Load(strTempFile);

			try { File.Delete(strTempFile); }
			catch(Exception) { }
		}
	}
}
