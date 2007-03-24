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
using System.Diagnostics;

using KeePassLib.Interfaces;

namespace KeePassLib
{
	/// <summary>
	/// Contains KeePassLib-global definitions and enums.
	/// </summary>
	public static class PwDefs
	{
		/// <summary>
		/// The product name.
		/// </summary>
		public const string ProductName = "KeePass Password Safe";

		/// <summary>
		/// A short, simple string representing the product name. The string
		/// should contain no spaces, directory separator characters, etc.
		/// </summary>
		public const string ShortProductName = "KeePass";

		/// <summary>
		/// Version, encoded as 32-bit unsigned integer.
		/// 2.00 = 0x02000000, 2.01 = 0x02000100, ...
		/// </summary>
		public const uint Version32 = 0x02000100;

		/// <summary>
		/// Version, encoded as string.
		/// </summary>
		public const string VersionString = "2.01 Alpha";

		/// <summary>
		/// Product homepage URL. Terminated by a forward slash.
		/// </summary>
		public const string HomepageUrl = "http://keepass.info/";

		/// <summary>
		/// Product donations URL.
		/// </summary>
		public const string DonationsUrl = "http://keepass.info/donate.html";

		/// <summary>
		/// URL to the online plugins page.
		/// </summary>
		public const string PluginsUrl = "http://keepass.info/plugins.html";

		/// <summary>
		/// URL to an XML file that contains information about the latest KeePass
		/// available on the homepage.
		/// </summary>
		public const string VersionUrl = "http://keepass.info/update/version2.xml.gz";

		/// <summary>
		/// URL to the root path of the online KeePass help. Terminated by
		/// a forward slash.
		/// </summary>
		public const string HelpUrl = "http://keepass.info/help/";

		/// <summary>
		/// A DateTime object that represents infinity.
		/// Example: setting the expire time of an entry to DtInfinity would mean
		/// that the entry never expires.
		/// </summary>
		public static readonly DateTime DtDefaultNow = DateTime.Now;

		/// <summary>
		/// Default number of master key encryption/transformation rounds (making dictionary attacks harder).
		/// </summary>
		public const ulong DefaultKeyEncryptionRounds = 6000;

		/// <summary>
		/// Default identifier string for the title field. Should not contain
		/// spaces, tabs or other whitespace.
		/// </summary>
		public const string TitleField = "Title";

		/// <summary>
		/// Default identifier string for the user name field. Should not contain
		/// spaces, tabs or other whitespace.
		/// </summary>
		public const string UserNameField = "UserName";

		/// <summary>
		/// Default identifier string for the password field. Should not contain
		/// spaces, tabs or other whitespace.
		/// </summary>
		public const string PasswordField = "Password";

		/// <summary>
		/// Default identifier string for the URL field. Should not contain
		/// spaces, tabs or other whitespace.
		/// </summary>
		public const string UrlField = "URL";

		/// <summary>
		/// Default identifier string for the notes field. Should not contain
		/// spaces, tabs or other whitespace.
		/// </summary>
		public const string NotesField = "Notes";

		/// <summary>
		/// Default identifier string for the field which will contain TAN indices.
		/// </summary>
		public const string TanIndexField = UserNameField;

		/// <summary>
		/// Default title of an entry that is really a TAN entry.
		/// </summary>
		public const string TanTitle = @"<TAN>";

		/// <summary>
		/// Prefix of a custom auto-type string field.
		/// </summary>
		public const string AutoTypeStringPrefix = "S:";

		/// <summary>
		/// Default string representing a hidden password.
		/// </summary>
		public const string HiddenPassword = "********";

		/// <summary>
		/// Default auto-type keystroke sequence. If no custom sequence is
		/// specified, this sequence is used.
		/// </summary>
		public const string DefaultAutoTypeSequence = @"{USERNAME}{TAB}{PASSWORD}{ENTER}";

		/// <summary>
		/// Default auto-type keystroke sequence for TAN entries. If no custom
		/// sequence is specified, this sequence is used.
		/// </summary>
		public const string DefaultAutoTypeSequenceTan = @"{PASSWORD}";

		/// <summary>
		/// Name of the registration key that holds the user's account key.
		/// </summary>
		public const string ProtectedUserRegKey = "ProtectedUserKey";

		/// <summary>
		/// Check if a name is a standard field name.
		/// </summary>
		/// <param name="strFieldName">Input field name.</param>
		/// <returns>Returns <c>true</c>, if the field name is a standard
		/// field name (title, user name, password, ...), otherwise <c>false</c>.</returns>
		public static bool IsStandardField(string strFieldName)
		{
			Debug.Assert(strFieldName != null); if(strFieldName == null) return false;

			if(strFieldName.Equals(TitleField)) return true;
			if(strFieldName.Equals(UserNameField)) return true;
			if(strFieldName.Equals(PasswordField)) return true;
			if(strFieldName.Equals(UrlField)) return true;
			if(strFieldName.Equals(NotesField)) return true;

			return false;
		}

		/// <summary>
		/// Check if an entry is a TAN.
		/// </summary>
		/// <param name="pe">Password entry.</param>
		/// <returns>Returns <c>true</c> if the entry is a TAN.</returns>
		public static bool IsTanEntry(PwEntry pe)
		{
			return pe.Strings.ReadSafe(PwDefs.TitleField) == TanTitle;
		}
	}

	#pragma warning disable 1591 // Missing XML comments warning
	/// <summary>
	/// Search parameters for group and entry searches.
	/// </summary>
	public sealed class SearchParameters
	{
		/// <summary>
		/// The text to be searched.
		/// </summary>
		public string SearchText = string.Empty;

		/// <summary>
		/// If this flag is <c>true</c>, all string fields of entries will
		/// be searched. This parameter overrides all other <c>SearchIn*</c>
		/// flags (only if set to <c>true</c>).
		/// </summary>
		public bool SearchInAllStrings = false;

		public bool SearchInTitles = true;
		public bool SearchInUserNames = true;
		public bool SearchInPasswords = false;
		public bool SearchInUrls = true;
		public bool SearchInNotes = true;

		/// <summary>
		/// String comparison type. Specifies the condition when the specified
		/// text matches a group/entry string.
		/// </summary>
		public StringComparison StringCompare = StringComparison.InvariantCultureIgnoreCase;

		/// <summary>
		/// Construct a new search parameters object.
		/// </summary>
		public SearchParameters()
		{
		}
	}
	#pragma warning restore 1591 // Missing XML comments warning

	#pragma warning disable 1591 // Missing XML comments warning
	/// <summary>
	/// Memory protection configuration structure (for default fields).
	/// </summary>
	public sealed class MemoryProtectionConfig : IDeepClonable<MemoryProtectionConfig>
	{
		public bool ProtectTitle = false;
		public bool ProtectUserName = false;
		public bool ProtectPassword = true;
		public bool ProtectUrl = false;
		public bool ProtectNotes = false;

		public bool AutoEnableVisualHiding = true;

		public MemoryProtectionConfig CloneDeep()
		{
			return (MemoryProtectionConfig)this.MemberwiseClone();
		}
	}
	#pragma warning restore 1591 // Missing XML comments warning
}
