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
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using KeePass.Forms;
using KeePass.Resources;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	internal sealed class KeePassHtml2x : FileFormatProvider
	{
		public override bool SupportsImport { get { return false; } }
		public override bool SupportsExport { get { return true; } }

		public override string FormatName { get { return KPRes.CustomizableHtml; } }
		public override string DefaultExtension { get { return "html"; } }
		public override string ApplicationGroup { get { return KPRes.General; } }

		public override Image SmallIcon
		{
			get { return KeePass.Properties.Resources.B16x16_HTML; }
		}

		public override bool Export(PwExportInfo pwExportInfo, Stream sOutput,
			IStatusLogger slLogger)
		{
			PrintForm dlg = new PrintForm();
			dlg.InitEx(pwExportInfo.DataGroup, false);

			if(dlg.ShowDialog() == DialogResult.OK)
			{
				StreamWriter sw = new StreamWriter(sOutput, Encoding.UTF8);
				sw.Write(dlg.GeneratedHtml);
				sw.Close();

				return true;
			}

			return false;
		}
	}
}
