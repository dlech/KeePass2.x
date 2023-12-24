/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2023 Dominik Reichl <dominik.reichl@t-online.de>

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

using StrDict = System.Collections.Generic.Dictionary<string, string>;

namespace KeePass.Util
{
	public sealed class IniFile
	{
		private readonly Dictionary<string, StrDict> m_dSections =
			new Dictionary<string, StrDict>();

		public IniFile()
		{
		}

		public static IniFile Read(string strFile, Encoding enc)
		{
			IniFile ini = new IniFile();

			using(StreamReader sr = new StreamReader(strFile, enc))
			{
				string strSection = string.Empty;

				while(true)
				{
					string str = sr.ReadLine();
					if(str == null) break; // End of stream

					str = str.Trim();
					if(str.Length == 0) continue;

					if(str.StartsWith("[") && str.EndsWith("]"))
						strSection = str.Substring(1, str.Length - 2);
					else
					{
						int iSep = str.IndexOf('=');
						if(iSep < 0) { Debug.Assert(false); }
						else
						{
							string strKey = str.Substring(0, iSep);
							string strValue = str.Substring(iSep + 1);

							if(!ini.m_dSections.ContainsKey(strSection))
								ini.m_dSections[strSection] = new StrDict();
							ini.m_dSections[strSection][strKey] = strValue;
						}
					}
				}
			}

			return ini;
		}

		public string Get(string strSection, string strKey)
		{
			if(strSection == null) throw new ArgumentNullException("strSection");
			if(strKey == null) throw new ArgumentNullException("strKey");

			StrDict d;
			if(!m_dSections.TryGetValue(strSection, out d)) return null;

			string strValue;
			d.TryGetValue(strKey, out strValue);
			return strValue;
		}
	}
}
