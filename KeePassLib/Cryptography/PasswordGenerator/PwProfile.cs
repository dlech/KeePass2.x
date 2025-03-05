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
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Xml.Serialization;

using KeePassLib.Interfaces;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePassLib.Cryptography.PasswordGenerator
{
	/// <summary>
	/// Type of the password generator. Different types like generators
	/// based on given patterns, based on character sets, etc. are
	/// available.
	/// </summary>
	public enum PasswordGeneratorType
	{
		/// <summary>
		/// Generator based on character spaces/sets, i.e. groups
		/// of characters like lower-case, upper-case or numeric characters.
		/// </summary>
		CharSet = 0,

		/// <summary>
		/// Password generation based on a pattern. The user has provided
		/// a pattern, which describes how the generated password has to
		/// look like.
		/// </summary>
		Pattern = 1,

		Custom = 2
	}

	public sealed class PwProfile : IDeepCloneable<PwProfile>, IEquatable<PwProfile>
	{
		private string m_strName = string.Empty;
		[DefaultValue("")]
		public string Name
		{
			get { return m_strName; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strName = value;
			}
		}

		private PasswordGeneratorType m_type = PasswordGeneratorType.CharSet;
		public PasswordGeneratorType GeneratorType
		{
			get { return m_type; }
			set { m_type = value; }
		}

		private bool m_bUserEntropy = false;
		[DefaultValue(false)]
		public bool CollectUserEntropy
		{
			get { return m_bUserEntropy; }
			set { m_bUserEntropy = value; }
		}

		private uint m_uLength = 20;
		public uint Length
		{
			get { return m_uLength; }
			set { m_uLength = value; }
		}

		private PwCharSet m_pwCharSet = new PwCharSet(PwCharSet.UpperCase +
			PwCharSet.LowerCase + PwCharSet.Digits);
		[XmlIgnore]
		public PwCharSet CharSet
		{
			get { return m_pwCharSet; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_pwCharSet = value;
			}
		}

		private string m_strCharSetRanges = string.Empty;
		[DefaultValue("")]
		public string CharSetRanges
		{
			get { UpdateCharSet(true); return m_strCharSetRanges; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strCharSetRanges = value;
				UpdateCharSet(false);
			}
		}

		private string m_strCharSetAdditional = string.Empty;
		[DefaultValue("")]
		public string CharSetAdditional
		{
			get { UpdateCharSet(true); return m_strCharSetAdditional; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strCharSetAdditional = value;
				UpdateCharSet(false);
			}
		}

		private string m_strPattern = string.Empty;
		[DefaultValue("")]
		public string Pattern
		{
			get { return m_strPattern; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strPattern = value;
			}
		}

		private bool m_bPatternPermute = false;
		[DefaultValue(false)]
		public bool PatternPermutePassword
		{
			get { return m_bPatternPermute; }
			set { m_bPatternPermute = value; }
		}

		private bool m_bNoLookAlike = false;
		[DefaultValue(false)]
		public bool ExcludeLookAlike
		{
			get { return m_bNoLookAlike; }
			set { m_bNoLookAlike = value; }
		}

		private bool m_bNoRepeat = false;
		[DefaultValue(false)]
		public bool NoRepeatingCharacters
		{
			get { return m_bNoRepeat; }
			set { m_bNoRepeat = value; }
		}

		private string m_strExclude = string.Empty;
		[DefaultValue("")]
		public string ExcludeCharacters
		{
			get { return m_strExclude; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strExclude = value;
			}
		}

		private string m_strCustomUuid = string.Empty;
		[DefaultValue("")]
		public string CustomAlgorithmUuid
		{
			get { return m_strCustomUuid; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strCustomUuid = value;
			}
		}

		private string m_strCustomOpt = string.Empty;
		[DefaultValue("")]
		public string CustomAlgorithmOptions
		{
			get { return m_strCustomOpt; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");
				m_strCustomOpt = value;
			}
		}

		public PwProfile()
		{
		}

		public PwProfile CloneDeep()
		{
			PwProfile p = new PwProfile();

			p.m_strName = m_strName;
			p.m_type = m_type;
			p.m_bUserEntropy = m_bUserEntropy;
			p.m_uLength = m_uLength;
			p.m_pwCharSet = new PwCharSet(m_pwCharSet.ToString());
			p.m_strCharSetRanges = m_strCharSetRanges;
			p.m_strCharSetAdditional = m_strCharSetAdditional;
			p.m_strPattern = m_strPattern;
			p.m_bPatternPermute = m_bPatternPermute;
			p.m_bNoLookAlike = m_bNoLookAlike;
			p.m_bNoRepeat = m_bNoRepeat;
			p.m_strExclude = m_strExclude;
			p.m_strCustomUuid = m_strCustomUuid;
			p.m_strCustomOpt = m_strCustomOpt;

			return p;
		}

		public bool Equals(PwProfile other)
		{
			if(object.ReferenceEquals(other, this)) return true;
			if(object.ReferenceEquals(other, null)) return false;

			if(m_strName != other.m_strName) return false;
			if(m_type != other.m_type) return false;
			if(m_bUserEntropy != other.m_bUserEntropy) return false;
			if(m_uLength != other.m_uLength) return false;
			if(!m_pwCharSet.Equals(other.m_pwCharSet)) return false;
			// if(m_strCharSetRanges != other.m_strCharSetRanges) return false;
			// if(m_strCharSetAdditional != other.m_strCharSetAdditional) return false;
			if(m_strPattern != other.m_strPattern) return false;
			if(m_bPatternPermute != other.m_bPatternPermute) return false;
			if(m_bNoLookAlike != other.m_bNoLookAlike) return false;
			if(m_bNoRepeat != other.m_bNoRepeat) return false;
			if(m_strExclude != other.m_strExclude) return false;
			if(m_strCustomUuid != other.m_strCustomUuid) return false;
			if(m_strCustomOpt != other.m_strCustomOpt) return false;

			return true;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as PwProfile);
		}

		public override int GetHashCode()
		{
			return m_strName.GetHashCode();
		}

		private void UpdateCharSet(bool bSetStrings)
		{
			if(bSetStrings)
			{
				PwCharSet pcs = new PwCharSet(m_pwCharSet.ToString());
				m_strCharSetRanges = pcs.PackAndRemoveCharRanges();
				m_strCharSetAdditional = pcs.ToString();
			}
			else
			{
				PwCharSet pcs = new PwCharSet(m_strCharSetAdditional);
				pcs.UnpackCharRanges(m_strCharSetRanges);
				m_pwCharSet = pcs;
			}
		}

		public static PwProfile DeriveFromPassword(ProtectedString psPassword)
		{
			PwProfile pp = new PwProfile();
			if(psPassword == null) { Debug.Assert(false); return pp; }

			char[] vChars = psPassword.ReadChars();

			pp.GeneratorType = PasswordGeneratorType.CharSet;
			pp.Length = (uint)vChars.Length;

			PwCharSet pcs = pp.CharSet;
			pcs.Clear();

			foreach(char ch in vChars)
			{
				if((ch >= 'A') && (ch <= 'Z')) pcs.Add(PwCharSet.UpperCase);
				else if((ch >= 'a') && (ch <= 'z')) pcs.Add(PwCharSet.LowerCase);
				else if((ch >= '0') && (ch <= '9')) pcs.Add(PwCharSet.Digits);
				else if(PwCharSet.Special.IndexOf(ch) >= 0)
					pcs.Add(PwCharSet.Special);
				else if(ch == ' ') pcs.Add(' ');
				else if(ch == '-') pcs.Add('-');
				else if(ch == '_') pcs.Add('_');
				else if(PwCharSet.Brackets.IndexOf(ch) >= 0)
					pcs.Add(PwCharSet.Brackets);
				else if(PwCharSet.Latin1S.IndexOf(ch) >= 0)
					pcs.Add(PwCharSet.Latin1S);
				else pcs.Add(ch);
			}

			MemUtil.ZeroArray<char>(vChars);
			return pp;
		}

		public bool HasSecurityReducingOption()
		{
			// Cf. PwGeneratorForm.EnableControlsEx
			return (m_bNoLookAlike || m_bNoRepeat || (m_strExclude.Length != 0));
		}
	}
}
