/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2011 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Threading;
using System.Diagnostics;

using KeePass.Resources;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	internal sealed class WinFavorites10 : FileFormatProvider
	{
		public override bool SupportsImport { get { return false; } }
		public override bool SupportsExport { get { return true; } }

		public override string FormatName { get { return "Windows Favorites"; } }
		public override bool RequiresFile { get { return false; } }
		public override string ApplicationGroup { get { return KPRes.General; } }

		public override Image SmallIcon
		{
			get { return KeePass.Properties.Resources.B16x16_WinFavs; }
		}

		public override bool Export(PwExportInfo pwExportInfo, Stream sOutput,
			IStatusLogger slLogger)
		{
			PwGroup pg = pwExportInfo.DataGroup;
			if(pg == null) { Debug.Assert(false); return true; }

			string strBaseName = FilterFileName(string.IsNullOrEmpty(
				Program.Config.Defaults.WinFavsBaseFolderName) ? PwDefs.ShortProductName :
				Program.Config.Defaults.WinFavsBaseFolderName);

			string strRootName = strBaseName + " - " + FilterFileName(pg.Name);
			if(pwExportInfo.ContextDatabase != null)
			{
				if(pg == pwExportInfo.ContextDatabase.RootGroup)
					strRootName = strBaseName;
			}

			string strFavsRoot = Environment.GetFolderPath(
				Environment.SpecialFolder.Favorites);
			if(string.IsNullOrEmpty(strFavsRoot)) return false;

			string strFavsSub = UrlUtil.EnsureTerminatingSeparator(strFavsRoot,
				false) + strRootName;
			if(Directory.Exists(strFavsSub))
			{
				Directory.Delete(strFavsSub, true);
				WaitForDirCommit(strFavsSub, false);
			}

			ExportGroup(pwExportInfo.DataGroup, strFavsSub);
			return true;
		}

		private static void ExportGroup(PwGroup pg, string strDir)
		{
			if(!Directory.Exists(strDir))
			{
				Directory.CreateDirectory(strDir);
				WaitForDirCommit(strDir, true);
			}

			foreach(PwEntry pe in pg.Entries) ExportEntry(pe, strDir);

			foreach(PwGroup pgSub in pg.Groups)
			{
				string strGroup = FilterFileName(pgSub.Name);
				string strSub = (UrlUtil.EnsureTerminatingSeparator(strDir, false) +
					(!string.IsNullOrEmpty(strGroup) ? strGroup : KPRes.Group));

				ExportGroup(pgSub, strSub);
			}
		}

		private static void ExportEntry(PwEntry pe, string strDir)
		{
			string strUrl = pe.Strings.ReadSafe(PwDefs.UrlField);
			if(string.IsNullOrEmpty(strUrl)) return;

			string strTitle = pe.Strings.ReadSafe(PwDefs.TitleField);
			if(string.IsNullOrEmpty(strTitle)) strTitle = KPRes.Entry;
			string strFileBase = (UrlUtil.EnsureTerminatingSeparator(strDir,
				false) + FilterFileName(strTitle));
			string strFile = strFileBase + ".url";
			int iFind = 2;
			while(File.Exists(strFile))
			{
				strFile = strFileBase + " " + iFind.ToString() + ".url";
				++iFind;
			}

			StringBuilder sb = new StringBuilder();

			sb.AppendLine(@"[InternetShortcut]");
			sb.AppendLine("URL=" + strUrl);

			File.WriteAllText(strFile, sb.ToString(), Encoding.Default);
		}

		private static string FilterFileName(string strName)
		{
			if(strName == null) { Debug.Assert(false); return string.Empty; }

			string str = strName;

			str = str.Replace('/', '-');
			str = str.Replace('\\', '-');
			str = str.Replace(":", string.Empty);
			str = str.Replace("*", string.Empty);
			str = str.Replace("?", string.Empty);
			str = str.Replace("\"", string.Empty);
			str = str.Replace(@"'", string.Empty);
			str = str.Replace('<', '(');
			str = str.Replace('>', ')');
			str = str.Replace('|', '-');

			return str;
		}

		private static void WaitForDirCommit(string strDir, bool bRequireExists)
		{
			for(int i = 0; i < 20; ++i)
			{
				bool bExists = Directory.Exists(strDir);
				if(bExists && bRequireExists) return;
				if(!bExists && !bRequireExists) return;

				Thread.Sleep(50);
			}
		}
	}
}
