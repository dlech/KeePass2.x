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
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

using KeePass.App;
using KeePass.Forms;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Utility;

namespace KeePass.DataExchange
{
	public static class ExportUtil
	{
		public static void Export(PwExportInfo pwExportInfo, IStatusLogger slLogger)
		{
			if(pwExportInfo == null) throw new ArgumentNullException("pwExportInfo");
			if(pwExportInfo.DataGroup == null) throw new ArgumentException();

			if(!AppPolicy.Try(AppPolicyId.Export)) return;

			ExchangeDataForm dlg = new ExchangeDataForm();
			dlg.InitEx(true, pwExportInfo.ContextDatabase, pwExportInfo.DataGroup);

			if(dlg.ShowDialog() == DialogResult.OK)
			{
				if(dlg.ResultFormat == null) { Debug.Assert(false); return; }
				if(dlg.ResultFiles.Length != 1) { Debug.Assert(false); return; }
				if(dlg.ResultFiles[0] == null) { Debug.Assert(false); return; }
				if(dlg.ResultFiles[0].Length == 0) { Debug.Assert(false); return; }

				Application.DoEvents(); // Redraw parent window

				try
				{
					PerformExport(pwExportInfo, dlg.ResultFormat, dlg.ResultFiles[0],
						slLogger);
				}
				catch(Exception ex)
				{
					MessageService.ShowWarning(ex);
				}
			}
		}

		private static void PerformExport(PwExportInfo pwExportInfo,
			FileFormatProvider fileFormat, string strOutputFile, IStatusLogger slLogger)
		{
			if(!fileFormat.SupportsExport) return;
			if(fileFormat.TryBeginExport() == false) return;

			bool bExistedAlready = File.Exists(strOutputFile);

			FileStream fsOut = new FileStream(strOutputFile, FileMode.Create,
				FileAccess.Write, FileShare.None);

			bool bResult = fileFormat.Export(pwExportInfo, fsOut, slLogger);

			fsOut.Close();
			fsOut.Dispose();

			if((bResult == false) && (bExistedAlready == false))
			{
				try { File.Delete(strOutputFile); }
				catch(Exception) { }
			}
		}
	}
}
