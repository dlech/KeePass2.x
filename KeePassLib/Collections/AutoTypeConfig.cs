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
using System.Diagnostics;

using KeePassLib.Interfaces;

namespace KeePassLib.Collections
{
	[Flags]
	public enum AutoTypeObfuscationOptions : uint
	{
		None = 0,
		UseClipboard = 1
	}

	/* public sealed class AutoTypeAssociation
	{
		private string m_strWindow = string.Empty;
		private string m_strSequence = string.Empty;

		public string WindowName
		{
			get { return m_strWindow; }
			set
			{
				Debug.Assert(value != null); if(value == null) throw new ArgumentNullException("value");

				m_strWindow = value;
			}
		}

		public string KeySequence
		{
			get { return m_strSequence; }
			set
			{
				Debug.Assert(value != null); if(value == null) throw new ArgumentNullException("value");

				m_strSequence = value;
			}
		}
	} */

	/// <summary>
	/// A dictionary of auto-type window/keystroke sequence pairs.
	/// </summary>
	public sealed class AutoTypeConfig : IDeepClonable<AutoTypeConfig>
	{
		private bool m_bEnabled = true;
		private AutoTypeObfuscationOptions m_atooObfuscation =
			AutoTypeObfuscationOptions.None;
		private string m_strDefaultSequence = string.Empty;
		private Dictionary<string, string> m_vWindowSeqPairs =
			new Dictionary<string, string>();

		/// <summary>
		/// Specify whether auto-type is enabled or not.
		/// </summary>
		public bool Enabled
		{
			get { return m_bEnabled; }
			set { m_bEnabled = value; }
		}

		/// <summary>
		/// Specify whether the typing should be obfuscated.
		/// </summary>
		public AutoTypeObfuscationOptions ObfuscationOptions
		{
			get { return m_atooObfuscation; }
			set { m_atooObfuscation = value; }
		}

		/// <summary>
		/// The default keystroke sequence that is auto-typed if
		/// no matching window is found in the <c>Items</c>
		/// container.
		/// </summary>
		public string DefaultSequence
		{
			get { return m_strDefaultSequence; }
			set
			{
				Debug.Assert(value != null); if(value == null) throw new ArgumentNullException("value");
				m_strDefaultSequence = value;
			}
		}

		/// <summary>
		/// Get all auto-type window/keystroke sequence pairs.
		/// </summary>
		public IEnumerable<KeyValuePair<string, string>> WindowSequencePairs
		{
			get { return m_vWindowSeqPairs; }
		}

		/// <summary>
		/// Construct a new auto-type dictionary.
		/// </summary>
		public AutoTypeConfig()
		{
		}

		/// <summary>
		/// Remove all window/keystroke sequence associations.
		/// </summary>
		public void Clear()
		{
			m_vWindowSeqPairs.Clear();
		}

		/// <summary>
		/// Clone the auto-type dictionary.
		/// </summary>
		/// <returns>New, cloned object.</returns>
		public AutoTypeConfig CloneDeep()
		{
			AutoTypeConfig newDic = new AutoTypeConfig();

			newDic.m_bEnabled = this.m_bEnabled;
			newDic.m_atooObfuscation = this.m_atooObfuscation;
			newDic.m_strDefaultSequence = this.m_strDefaultSequence;

			foreach(KeyValuePair<string, string> kvp in m_vWindowSeqPairs)
				newDic.Set(kvp.Key, kvp.Value);

			return newDic;
		}

		/// <summary>
		/// Set a window/keystroke sequence pair.
		/// </summary>
		/// <param name="strWindow">Name of the window. Must not be <c>null</c>.</param>
		/// <param name="strKeystrokeSequence">Keystroke sequence for the specified
		/// window. Must not be <c>null</c>.</param>
		/// <exception cref="System.ArgumentNullException">Thrown if one of the input
		/// parameters is <c>null</c>.</exception>
		public void Set(string strWindow, string strKeystrokeSequence)
		{
			Debug.Assert(strWindow != null); if(strWindow == null) throw new ArgumentNullException("strWindow");
			Debug.Assert(strKeystrokeSequence != null); if(strKeystrokeSequence == null) throw new ArgumentNullException("strKeystrokeSequence");

			m_vWindowSeqPairs[strWindow] = strKeystrokeSequence;
		}

		/// <summary>
		/// Get a keystroke sequence associated with the specified window.
		/// Returns <c>null</c>, if no sequence can be found.
		/// </summary>
		/// <param name="strWindow">Window identifier.</param>
		/// <returns>Keystroke sequence associated with the specified window.
		/// The return value is <c>null</c>, if no keystroke sequence has been
		/// defined for this window yet.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if the input
		/// parameter is <c>null</c>.</exception>
		public string Get(string strWindow)
		{
			Debug.Assert(strWindow != null); if(strWindow == null) throw new ArgumentNullException("strWindow");

			string str;
			if(m_vWindowSeqPairs.TryGetValue(strWindow, out str))
				return str;

			return null;
		}

		/// <summary>
		/// Get a keystroke sequence associated with the specified window.
		/// Returns an empty string (<c>""</c>), if no sequence can be found.
		/// </summary>
		/// <param name="strWindow">Window identifier.</param>
		/// <returns>Keystroke sequence associated with the specified window.
		/// The return value is an empty string (<c>""</c>), if no keystroke
		/// sequence has been defined for this window yet.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if the input
		/// parameter is <c>null</c>.</exception>
		public string GetSafe(string strWindow)
		{
			Debug.Assert(strWindow != null); if(strWindow == null) throw new ArgumentNullException("strWindow");

			string str;
			if(m_vWindowSeqPairs.TryGetValue(strWindow, out str))
				return str;

			return string.Empty;
		}

		/// <summary>
		/// Remove an auto-type entry.
		/// </summary>
		/// <param name="strWindow">Window identifier. Must not be <c>null</c>.</param>
		/// <returns>Returns <c>true</c> if the entry has been removed.</returns>
		public bool Remove(string strWindow)
		{
			Debug.Assert(strWindow != null); if(strWindow == null) throw new ArgumentNullException("strWindow");

			return m_vWindowSeqPairs.Remove(strWindow);
		}
	}
}
