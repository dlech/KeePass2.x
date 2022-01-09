/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2022 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePassLib;
using KeePassLib.Utility;

namespace TrlUtil.App.Configuration
{
	[XmlType("Configuration")]
	public sealed class TceConfig
	{
		private TceApplication m_tceApp = null;
		public TceApplication Application
		{
			get
			{
				if(m_tceApp == null) m_tceApp = new TceApplication();
				return m_tceApp;
			}
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_tceApp = value;
			}
		}

		private static string GetFilePath(out string strDir)
		{
			strDir = UrlUtil.EnsureTerminatingSeparator(Environment.GetFolderPath(
				Environment.SpecialFolder.ApplicationData), false) +
				PwDefs.ShortProductName;

			return (UrlUtil.EnsureTerminatingSeparator(strDir, false) +
				TuDefs.ConfigFile);
		}

		internal static TceConfig Load()
		{
			try
			{
				string strDir;
				string strFile = GetFilePath(out strDir);

				if(File.Exists(strFile))
				{
					using(FileStream fs = new FileStream(strFile, FileMode.Open,
						FileAccess.Read, FileShare.Read))
					{
						return XmlUtilEx.Deserialize<TceConfig>(fs);
					}
				}
			}
			catch(Exception) { Debug.Assert(false); }

			return null;
		}

		internal static void Save(TceConfig cfg)
		{
			if(cfg == null) { Debug.Assert(false); return; }

			try
			{
				string strDir;
				string strFile = GetFilePath(out strDir);

				if(!Directory.Exists(strDir))
					Directory.CreateDirectory(strDir);

				using(FileStream fs = new FileStream(strFile, FileMode.Create,
					FileAccess.Write, FileShare.None))
				{
					XmlUtilEx.Serialize<TceConfig>(fs, cfg);
				}
			}
			catch(Exception) { Debug.Assert(false); }
		}
	}
}
