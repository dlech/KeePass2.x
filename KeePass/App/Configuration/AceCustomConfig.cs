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
using System.Globalization;
using System.Text;

using KeePassLib.Utility;

namespace KeePass.App.Configuration
{
	public sealed class AceKvp
	{
		private string m_strKey = null;
		public string Key
		{
			get { return m_strKey; }
			set { m_strKey = value; }
		}

		private string m_strValue = null;
		public string Value
		{
			get { return m_strValue; }
			set { m_strValue = value; }
		}

		public AceKvp()
		{
		}

		public AceKvp(string strKey, string strValue)
		{
			m_strKey = strKey;
			m_strValue = strValue;
		}
	}

	public sealed class AceCustomConfig
	{
		private readonly Dictionary<string, string> m_d = new Dictionary<string, string>();

		public AceCustomConfig()
		{
		}

		internal AceKvp[] Serialize()
		{
			int c = m_d.Count;
			if(c == 0) return MemUtil.EmptyArray<AceKvp>();

			AceKvp[] v = new AceKvp[c];
			int i = 0;
			foreach(KeyValuePair<string, string> kvp in m_d)
				v[i++] = new AceKvp(kvp.Key, kvp.Value);
			Debug.Assert(i == c);
			return v;
		}

		internal void Deserialize(AceKvp[] v)
		{
			if(v == null) throw new ArgumentNullException("v");

			m_d.Clear();

			foreach(AceKvp kvp in v)
			{
				if(kvp == null) { Debug.Assert(false); continue; }
				if(string.IsNullOrEmpty(kvp.Key)) { Debug.Assert(false); continue; }
				if(kvp.Value == null) { Debug.Assert(false); continue; }

				m_d[kvp.Key] = kvp.Value;
			}
		}

		/// <summary>
		/// Set a configuration item's value.
		/// </summary>
		/// <param name="strID">ID of the configuration item. This identifier
		/// should consist only of English characters (a-z, A-Z, 0-9, '.',
		/// ',', '-', '_') and should be unique -- for example (without quotes):
		/// "PluginName.YourConfigGroupName.ItemName". Use upper camel
		/// case as naming convention.</param>
		/// <param name="strValue">New value of the configuration item.</param>
		public void SetString(string strID, string strValue)
		{
			if(strID == null) throw new ArgumentNullException("strID");
			if(strID.Length == 0) throw new ArgumentException();

			if(strValue == null) m_d.Remove(strID);
			else m_d[strID] = strValue;
		}

		/// <summary>
		/// Set a configuration item's value.
		/// </summary>
		/// <param name="strID">ID of the configuration item. This identifier
		/// should consist only of English characters (a-z, A-Z, 0-9, '.',
		/// ',', '-', '_') and should be unique -- for example (without quotes):
		/// "PluginName.YourConfigGroupName.ItemName". Use upper camel
		/// case as naming convention.</param>
		/// <param name="bValue">New value of the configuration item.</param>
		public void SetBool(string strID, bool bValue)
		{
			SetString(strID, StrUtil.BoolToString(bValue));
		}

		/// <summary>
		/// Set a configuration item's value.
		/// </summary>
		/// <param name="strID">ID of the configuration item. This identifier
		/// should consist only of English characters (a-z, A-Z, 0-9, '.',
		/// ',', '-', '_') and should be unique -- for example (without quotes):
		/// "PluginName.YourConfigGroupName.ItemName". Use upper camel
		/// case as naming convention.</param>
		/// <param name="lValue">New value of the configuration item.</param>
		public void SetLong(string strID, long lValue)
		{
			SetString(strID, lValue.ToString(NumberFormatInfo.InvariantInfo));
		}

		/// <summary>
		/// Set a configuration item's value.
		/// </summary>
		/// <param name="strID">ID of the configuration item. This identifier
		/// should consist only of English characters (a-z, A-Z, 0-9, '.',
		/// ',', '-', '_') and should be unique -- for example (without quotes):
		/// "PluginName.YourConfigGroupName.ItemName". Use upper camel
		/// case as naming convention.</param>
		/// <param name="uValue">New value of the configuration item.</param>
		public void SetULong(string strID, ulong uValue)
		{
			SetString(strID, uValue.ToString(NumberFormatInfo.InvariantInfo));
		}

		public string GetString(string strID)
		{
			return GetString(strID, null);
		}

		/// <summary>
		/// Get the current value of a custom configuration string.
		/// </summary>
		/// <param name="strID">ID of the configuration item.</param>
		/// <param name="strDefault">Default value that is returned if
		/// the specified configuration does not exist.</param>
		/// <returns>Value of the configuration item.</returns>
		public string GetString(string strID, string strDefault)
		{
			if(strID == null) throw new ArgumentNullException("strID");
			if(strID.Length == 0) throw new ArgumentException();

			string strValue;
			if(m_d.TryGetValue(strID, out strValue)) return strValue;

			return strDefault;
		}

		public bool GetBool(string strID, bool bDefault)
		{
			string strValue = GetString(strID, null);
			if(string.IsNullOrEmpty(strValue)) return bDefault;

			return StrUtil.StringToBool(strValue);
		}

		public long GetLong(string strID, long lDefault)
		{
			string strValue = GetString(strID, null);
			if(string.IsNullOrEmpty(strValue)) return lDefault;

			long lValue;
			if(StrUtil.TryParseLongInvariant(strValue, out lValue))
				return lValue;

			return lDefault;
		}

		public ulong GetULong(string strID, ulong uDefault)
		{
			string strValue = GetString(strID, null);
			if(string.IsNullOrEmpty(strValue)) return uDefault;

			ulong uValue;
			if(StrUtil.TryParseULongInvariant(strValue, out uValue))
				return uValue;

			return uDefault;
		}
	}
}
