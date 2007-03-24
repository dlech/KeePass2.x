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
using System.Diagnostics;

namespace KeePass.Util
{
	public sealed class CommandLineArgs
	{
		private List<string> m_vFileNames = new List<string>();
		private SortedDictionary<string, string> m_vParams =
			new SortedDictionary<string, string>();

		/// <summary>
		/// Get the primary file name.
		/// </summary>
		public string FileName
		{
			get
			{
				if(m_vFileNames.Count < 1) return null;
				return m_vFileNames[0];
			}
		}

		/// <summary>
		/// All file names.
		/// </summary>
		public IEnumerable<string> FileNames
		{
			get { return m_vFileNames; }
		}

		public IEnumerable<KeyValuePair<string, string>> Parameters
		{
			get { return m_vParams; }
		}

		public CommandLineArgs(string[] vArgs)
		{
			if(vArgs == null) return;

			foreach(string str in vArgs)
			{
				if((str == null) || (str.Length < 1)) continue;

				KeyValuePair<string, string> kvp = GetParameter(str);

				if(kvp.Key.Length == 0) m_vFileNames.Add(kvp.Value);
				else m_vParams[kvp.Key] = kvp.Value;
			}
		}

		/// <summary>
		/// Get the value of a command-line parameter. Returns <c>null</c>
		/// if no key-value pair with the specified key exists.
		/// </summary>
		/// <param name="strParam">Key name.</param>
		/// <returns>Value of the specified key or <c>null</c>.</returns>
		public string this[string strKey]
		{
			get
			{
				if((strKey == null) || (strKey.Length == 0))
					return this.FileName;
				else
				{
					string strValue;

					if(m_vParams.TryGetValue(strKey.ToLower(), out strValue))
						return strValue;
				}

				return null;
			}
		}

		private KeyValuePair<string, string> GetParameter(string strCompiled)
		{
			string str = strCompiled;

			if(str.StartsWith("--")) str = str.Remove(0, 2);
			else if(str.StartsWith("-")) str = str.Remove(0, 1);
			else if(str.StartsWith("/")) str = str.Remove(0, 1);
			else return new KeyValuePair<string, string>(string.Empty, str);

			int posDbl = str.IndexOf(':');
			int posEq = str.IndexOf('=');

			if((posDbl < 0) && (posEq < 0))
				return new KeyValuePair<string, string>(str.ToLower(), string.Empty);

			int posMin = Math.Min(posDbl, posEq);
			if(posMin < 0) posMin = (posDbl < 0) ? posEq : posDbl;

			if(posMin <= 0)
				return new KeyValuePair<string, string>(str.ToLower(), string.Empty);

			string strKey = str.Substring(0, posMin).ToLower();
			string strValue = str.Remove(0, posMin + 1);
			return new KeyValuePair<string, string>(strKey, strValue);
		}
	}
}
