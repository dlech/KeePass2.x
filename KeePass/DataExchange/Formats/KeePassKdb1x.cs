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

using KeePass.Resources;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Native;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	internal sealed class KeePassKdb1x : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return true; } }

		public override string FormatName { get { return "KeePass KDB (1.x)"; } }
		public override string DefaultExtension { get { return KeePassKdb1x.FileExt1; } }
		public override string ApplicationGroup { get { return PwDefs.ShortProductName; } }

		public override bool RequiresKey { get { return true; } }
		public override bool SupportsUuids { get { return true; } }

		public override Image SmallIcon
		{
			get { return KeePass.Properties.Resources.B16x16_KeePass; }
		}

		internal const string FileExt1 = "kdb";
		internal const string FileExt2 = "pwd";

		public override bool TryBeginImport()
		{
			if(NativeLib.IsUnix())
			{
				MessageService.ShowWarning(KPRes.KeePassLibCNotWindows,
					KPRes.KeePassLibCNotWindowsHint);
				return false;
			}

			Exception exLib;
			if(!KdbFile.IsLibraryInstalled(out exLib))
			{
				MessageService.ShowWarning(KPRes.KeePassLibCNotFound,
					KPRes.KdbKeePassLibC, exLib);
				return false;
			}

			return true;
		}

		public override bool TryBeginExport()
		{
			return TryBeginImport();
		}

		public override void Import(PwDatabase pdStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			string strTempFile = Program.TempFilesPool.GetTempFileName();

			try
			{
				using(FileStream fs = new FileStream(strTempFile, FileMode.Create,
					FileAccess.Write, FileShare.None))
				{
					MemUtil.CopyStream(sInput, fs);
				}

				KdbFile kdb = new KdbFile(pdStorage, slLogger);
				kdb.Load(strTempFile);
			}
			finally { Program.TempFilesPool.Delete(strTempFile); }
		}

		public override bool Export(PwExportInfo pwExportInfo, Stream sOutput,
			IStatusLogger slLogger)
		{
			PwDatabase pd = (pwExportInfo.ContextDatabase ?? new PwDatabase());

			string strTempFile = Program.TempFilesPool.GetTempFileName(false);

			try
			{
				KdbFile kdb = new KdbFile(pd, slLogger);
				kdb.Save(strTempFile, pwExportInfo.DataGroup);

				byte[] pbKdb = File.ReadAllBytes(strTempFile);
				sOutput.Write(pbKdb, 0, pbKdb.Length);
				MemUtil.ZeroByteArray(pbKdb);
			}
			catch(Exception ex)
			{
				if(slLogger != null)
					slLogger.SetText(StrUtil.FormatException(ex, null), LogStatusType.Error);

				return false;
			}
			finally { Program.TempFilesPool.Delete(strTempFile); }

			return true;
		}
	}
}
