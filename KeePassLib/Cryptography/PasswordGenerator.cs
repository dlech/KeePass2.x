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

using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePassLib.Cryptography
{
	/// <summary>
	/// Type of the password generator. Different types like generators
	/// based on given patterns, based on character sets, etc. are
	/// available.
	/// </summary>
	public enum PasswordGeneratorType : uint
	{
		/// <summary>
		/// Generator based on character spaces/sets, i.e. groups
		/// of characters like lower-case, upper-case or numeric characters.
		/// </summary>
		CharSpaces = 0,

		/// <summary>
		/// Custom character set. The user has provided a list of
		/// characters which may be used in the final generated password.
		/// </summary>
		CustomCharSet,

		/// <summary>
		/// Password generation based on a pattern. The user has provided
		/// a pattern, which describes how the generated password has to
		/// look like.
		/// </summary>
		Pattern
	}

	/// <summary>
	/// Random password generation options / layout.
	/// </summary>
	public sealed class PasswordGenerationOptions
	{
		private const string FieldSeparator = @"/:/";

		private string m_strProfileName = string.Empty;

		private PasswordGeneratorType m_gtType = PasswordGeneratorType.CharSpaces;
		private bool m_bCollectEntropy = false;
		private uint m_uPwLen = 16;
		private CharSpaces m_charSpaces = new CharSpaces();
		private CustomCharSet m_customCharSet = new CustomCharSet();
		private string m_strPattern = string.Empty;

		/// <summary>
		/// Name of the current generation options profile.
		/// </summary>
		public string ProfileName
		{
			get { return m_strProfileName; }
			set
			{
				if(value != null)
					m_strProfileName = value.Replace(FieldSeparator, string.Empty);
				else m_strProfileName = string.Empty;
			}
		}

		/// <summary>
		/// Get or set the generator type.
		/// </summary>
		public PasswordGeneratorType GeneratorType
		{
			get { return m_gtType; }
			set { m_gtType = value; }
		}

		/// <summary>
		/// Specify if the user should generate additional entropy (by moving
		/// mouse and typing random keys, ...).
		/// </summary>
		public bool CollectUserEntropy
		{
			get { return m_bCollectEntropy; }
			set { m_bCollectEntropy = value; }
		}

		/// <summary>
		/// Length of the password to be generated.
		/// </summary>
		public uint PasswordLength
		{
			get { return m_uPwLen; }
			set { m_uPwLen = value; }
		}

		/// <summary>
		/// Character spaces from which characters will be selected to build
		/// the generated password.
		/// </summary>
		public CharSpaces CharSpaces
		{
			get { return m_charSpaces; }
			set
			{
				if(value != null) m_charSpaces = value;
				else m_charSpaces = new CharSpaces();
			}
		}

		/// <summary>
		/// A custom char set overriding the character spaces definition.
		/// This set contains all characters that will be used to build the generated
		/// password.
		/// </summary>
		public CustomCharSet CustomCharSet
		{
			get { return m_customCharSet; }
			set
			{
				if(value != null) m_customCharSet = value;
				else m_customCharSet.Clear();
			}
		}

		/// <summary>
		/// Password pattern.
		/// </summary>
		public string Pattern
		{
			get { return m_strPattern; }
			set
			{
				if(value != null) m_strPattern = value;
				else m_strPattern = string.Empty;
			}
		}

		/// <summary>
		/// Convert the options of a <c>GenerationOptions</c> object to a string.
		/// </summary>
		/// <returns>String representing the current options.</returns>
		public static string SerializeToString(PasswordGenerationOptions gopt)
		{
			Debug.Assert(gopt != null);
			if(gopt == null) throw new ArgumentNullException("gopt");

			StringBuilder sb = new StringBuilder();
			sb.Append(gopt.m_strProfileName);
			sb.Append(FieldSeparator);
			sb.Append(((uint)gopt.m_gtType).ToString());
			sb.Append(FieldSeparator);
			sb.Append(gopt.m_bCollectEntropy ? '1' : '0');
			sb.Append(FieldSeparator);
			sb.Append(gopt.m_uPwLen.ToString());
			sb.Append(FieldSeparator);
			sb.Append(CharSpaces.SerializeToString(gopt.m_charSpaces));
			sb.Append(FieldSeparator);
			sb.Append(gopt.m_customCharSet.GetAsString().Replace(FieldSeparator, string.Empty));
			sb.Append(FieldSeparator);
			sb.Append(gopt.m_strPattern.Replace(FieldSeparator, string.Empty));
			return sb.ToString();
		}

#if !KeePassLibSD
		/// <summary>
		/// Convert a serialized <c>GenerationOptions</c> string to an object.
		/// </summary>
		/// <param name="strSource">String to unserialize.</param>
		/// <returns>Unserialized object representing the string.</returns>
		public static PasswordGenerationOptions UnserializeFromString(string strSource)
		{
			Debug.Assert(strSource != null);
			if(strSource == null) throw new ArgumentNullException("strSource");

			PasswordGenerationOptions gopt = new PasswordGenerationOptions();

			string[] vParts = strSource.Split(new string[]{ FieldSeparator },
				StringSplitOptions.None);
			Debug.Assert(vParts.Length == 7); if(vParts.Length < 7) return gopt;

			gopt.m_strProfileName = vParts[0];

			uint u;
			uint.TryParse(vParts[1], out u);
			gopt.m_gtType = (PasswordGeneratorType)u;

			uint.TryParse(vParts[2], out u);
			gopt.m_bCollectEntropy = (u == 1);

			uint.TryParse(vParts[3], out u);
			gopt.m_uPwLen = u;

			gopt.m_charSpaces = CharSpaces.UnserializeFromString(vParts[4]);
			gopt.m_customCharSet.Set(vParts[5]);
			gopt.m_strPattern = vParts[6];

			return gopt;
		}
#endif
	}

	/// <summary>
	/// A custom char set overriding the character spaces definition.
	/// This set contains all characters that will be used to build the generated
	/// password.
	/// </summary>
	public sealed class CustomCharSet
	{
		private List<char> m_vChars = new List<char>();

		/// <summary>
		/// Number of characters in this set.
		/// </summary>
		public uint UCount
		{
			get { return (uint)m_vChars.Count; }
		}

		/// <summary>
		/// Get a character of the set using an index.
		/// </summary>
		/// <param name="uPos">Index of the character to get.</param>
		/// <returns>Character at the specified position. If the index is invalid,
		/// an <c>IndexOutOfRangeException</c> is thrown.</returns>
		public char this[uint uPos]
		{
			get
			{
				if(uPos >= m_vChars.Count) throw new IndexOutOfRangeException();
				return m_vChars[(int)uPos];
			}
		}

		/// <summary>
		/// Remove all characters from this set.
		/// </summary>
		public void Clear()
		{
			m_vChars.Clear();
		}

		/// <summary>
		/// Add characters to the set.
		/// </summary>
		/// <param name="ch">Character to add.</param>
		public void Add(char ch)
		{
			if(ch == char.MinValue) return;

			if(m_vChars.IndexOf(ch) < 0)
				m_vChars.Add(ch);
		}

		/// <summary>
		/// Add characters to the set.
		/// </summary>
		/// <param name="strCharSet">String containing characters to add.</param>
		public void Add(string strCharSet)
		{
			Debug.Assert(strCharSet != null); if(strCharSet == null) throw new ArgumentNullException();

			foreach(char ch in strCharSet)
				this.Add(ch);
		}

		/// <summary>
		/// Set the character set to use.
		/// </summary>
		/// <param name="strCharSet">String containing all characters to use.</param>
		public void Set(string strCharSet)
		{
			this.Clear();
			this.Add(strCharSet);
		}

		/// <summary>
		/// Convert the character set to a string containing all its characters.
		/// </summary>
		/// <returns>String containing all character set characters.</returns>
		public string GetAsString()
		{
			StringBuilder sb = new StringBuilder();
			foreach(char ch in m_vChars)
				sb.Append(ch);

			return sb.ToString();
		}
	}

	/// <summary>
	/// Character spaces from which characters will be selected to build
	/// the generated password.
	/// </summary>
	public sealed class CharSpaces
	{
		private const int SpacesCount = 9;

#pragma warning disable 1591 // Missing XML comments warning
		public bool UpperCase = true;
		public bool LowerCase = true;
		public bool Numeric = true;
		public bool Underline = true;
		public bool Minus = true;
		public bool Space = false;
		public bool Special = false;
		public bool Brackets = false;
		public bool HighANSI = false;
#pragma warning restore 1591 // Missing XML comments warning

		/// <summary>
		/// Disable all character spaces.
		/// </summary>
		public void Clear()
		{
			this.UpperCase = this.LowerCase = this.Numeric = this.Underline =
				this.Minus = this.Space = this.Special = this.Brackets =
				this.HighANSI = false;
		}

		/// <summary>
		/// Convert the current settings to a string.
		/// </summary>
		/// <returns>String representing the options.</returns>
		public static string SerializeToString(CharSpaces cs)
		{
			Debug.Assert(cs != null);
			if(cs == null) throw new ArgumentNullException("cs");

			StringBuilder sb = new StringBuilder();

			sb.Append(cs.UpperCase ? '1' : '0');
			sb.Append(cs.LowerCase ? '1' : '0');
			sb.Append(cs.Numeric ? '1' : '0');
			sb.Append(cs.Underline ? '1' : '0');
			sb.Append(cs.Minus ? '1' : '0');
			sb.Append(cs.Space ? '1' : '0');
			sb.Append(cs.Special ? '1' : '0');
			sb.Append(cs.Brackets ? '1' : '0');
			sb.Append(cs.HighANSI ? '1' : '0');

			return sb.ToString();
		}

		/// <summary>
		/// Extract settings from a compiled string (created by the <c>Compile</c>
		/// member function).
		/// </summary>
		/// <param name="strCompiled">Compiled string generated by <c>Compile</c>.</param>
		public static CharSpaces UnserializeFromString(string strCompiled)
		{
			Debug.Assert((strCompiled != null) && (strCompiled.Length == SpacesCount));
			if(strCompiled == null) throw new ArgumentNullException("strCompiled");
			if(strCompiled.Length < SpacesCount) throw new ArgumentException();

			CharSpaces cs = new CharSpaces();

			int nIndex = -1;
			cs.UpperCase = (strCompiled[++nIndex] == '1');
			cs.LowerCase = (strCompiled[++nIndex] == '1');
			cs.Numeric = (strCompiled[++nIndex] == '1');
			cs.Underline = (strCompiled[++nIndex] == '1');
			cs.Minus = (strCompiled[++nIndex] == '1');
			cs.Space = (strCompiled[++nIndex] == '1');
			cs.Special = (strCompiled[++nIndex] == '1');
			cs.Brackets = (strCompiled[++nIndex] == '1');
			cs.HighANSI = (strCompiled[++nIndex] == '1');

			Debug.Assert((nIndex + 1) <= SpacesCount);
			return cs;
		}
	}

	/// <summary>
	/// Utility functions for generating random passwords.
	/// </summary>
	public static class PasswordGenerator
	{
		private const string CharSetUpper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		private const string CharSetLower = "abcdefghijklmnopqrstuvwxyz";
		private const string CharSetNumeric = "0123456789";
		private const string CharSetUpperConsonants = "BCDFGHJKLMNPQRSTVWXYZ";
		private const string CharSetLowerConsonants = "bcdfghjklmnpqrstvwxyz";
		private const string CharSetUpperVowel = "AEIOU";
		private const string CharSetLowerVowel = "aeiou";
		private const string CharSetPunctuation = @",.;:";
		private const string CharSetHexLower = "0123456789abcdef";
		private const string CharSetHexUpper = "0123456789ABCDEF";

		private static CustomCharSet CreateCustomCharSet(CharSpaces opt)
		{
			CustomCharSet ccs = new CustomCharSet();

			if(opt.UpperCase) ccs.Add(CharSetUpper);
			if(opt.LowerCase) ccs.Add(CharSetLower);
			if(opt.Numeric) ccs.Add(CharSetNumeric);
			if(opt.Underline) ccs.Add('_');
			if(opt.Minus) ccs.Add('-');
			if(opt.Space) ccs.Add(' ');
			if(opt.Special)
			{
				ccs.Add(@"!#$%&'*+,./:;=?@^");
				ccs.Add('\"');
				ccs.Add('\\');
			}
			if(opt.Brackets) ccs.Add(@"()[]{}<>");
			if(opt.HighANSI)
			{
				for(char ch = '~'; ch < 255; ++ch)
					ccs.Add(ch);
			}

			return ccs;
		}

		public static PasswordGenerationOptions DeriveOptionsFromPassword(ProtectedString psPassword)
		{
			PasswordGenerationOptions opt = new PasswordGenerationOptions();
			Debug.Assert(psPassword != null); if(psPassword == null) return opt;

			opt.CharSpaces.Clear();

			UTF8Encoding utf8 = new UTF8Encoding();
			byte[] pbUTF8 = psPassword.ReadUTF8();
			char[] vChars = utf8.GetChars(pbUTF8);

			opt.PasswordLength = (uint)vChars.Length;
			foreach(char ch in vChars)
			{
				if((ch >= 'A') && (ch <= 'Z')) opt.CharSpaces.UpperCase = true;
				else if((ch >= 'a') && (ch <= 'z')) opt.CharSpaces.LowerCase = true;
				else if((ch >= '0') && (ch <= '9')) opt.CharSpaces.Numeric = true;
				else if(ch == '_') opt.CharSpaces.Underline = true;
				else if(ch == '-') opt.CharSpaces.Minus = true;
				else if(ch == ' ') opt.CharSpaces.Space = true;
				else if((@"!#$%&'*+,./:;=?@^").IndexOf(ch) >= 0) opt.CharSpaces.Special = true;
				else if(ch == '\"') opt.CharSpaces.Special = true;
				else if(ch == '\\') opt.CharSpaces.Special = true;
				else if((@"()[]{}<>").IndexOf(ch) >= 0) opt.CharSpaces.Brackets = true;
				else if(ch >= '~') opt.CharSpaces.HighANSI = true;
			}

			Array.Clear(vChars, 0, vChars.Length);
			Array.Clear(pbUTF8, 0, pbUTF8.Length);

			return opt;
		}

		public static ProtectedString Generate(PasswordGenerationOptions opt,
			bool bReturnProtected, byte[] pbAdditionalEntropy)
		{
			Debug.Assert(opt != null); if(opt == null) throw new ArgumentNullException();

			if(opt.GeneratorType == PasswordGeneratorType.CustomCharSet)
				return CustomCharSetGenerate(opt.CustomCharSet, opt.PasswordLength,
					bReturnProtected, pbAdditionalEntropy);
			else if(opt.GeneratorType == PasswordGeneratorType.CharSpaces)
			{
				CustomCharSet ccs = CreateCustomCharSet(opt.CharSpaces);
				return CustomCharSetGenerate(ccs, opt.PasswordLength, bReturnProtected,
					pbAdditionalEntropy);
			}
			else if(opt.GeneratorType == PasswordGeneratorType.Pattern)
				return PatternGenerate(opt.Pattern, bReturnProtected, pbAdditionalEntropy);
			else { Debug.Assert(false); return new ProtectedString(false, string.Empty); }
		}

		private static CryptoRandomStream CreateCryptoStream(byte[] pbAdditionalEntropy)
		{
			byte[] pbKey = CryptoRandom.GetRandomBytes(256);

			// Mix in additional entropy
			if((pbAdditionalEntropy != null) && (pbAdditionalEntropy.Length > 0))
			{
				for(int nKeyPos = 0; nKeyPos < pbKey.Length; ++nKeyPos)
					pbKey[nKeyPos] ^= pbAdditionalEntropy[nKeyPos % pbAdditionalEntropy.Length];
			}

			return new CryptoRandomStream(CrsAlgorithm.ArcFour, pbKey);
		}

		private static ProtectedString CustomCharSetGenerate(CustomCharSet charSet, uint uPasswordLength,
			bool bReturnProtected, byte[] pbAdditionalEntropy)
		{
			Debug.Assert(charSet != null); if(charSet == null) throw new ArgumentNullException();
			if(charSet.UCount == 0) return new ProtectedString(false, string.Empty);
			if(uPasswordLength == 0) return new ProtectedString(false, string.Empty);

			CryptoRandomStream crs = CreateCryptoStream(pbAdditionalEntropy);
			UTF8Encoding utf8 = new UTF8Encoding();
			uint uGeneratedCount = 0, uNewCharIndex, uRandomPos = 0;
			byte[] pbRandom = crs.GetRandomBytes(1024);
			bool b16BitIndex = (charSet.UCount > 256);

			char[] vGenerated = new char[uPasswordLength];
			Array.Clear(vGenerated, 0, vGenerated.Length);

			while(uGeneratedCount < uPasswordLength)
			{
				uNewCharIndex = pbRandom[uRandomPos];
				++uRandomPos;

				if(b16BitIndex)
				{
					uNewCharIndex *= 256;
					uNewCharIndex += pbRandom[uRandomPos];
					++uRandomPos;
				}

				if(uRandomPos >= (uint)pbRandom.Length)
				{
					pbRandom = CryptoRandom.GetRandomBytes(1024);
					uRandomPos = 0;
				}

				if(uNewCharIndex < charSet.UCount)
				{
					vGenerated[uGeneratedCount] = charSet[uNewCharIndex];
					++uGeneratedCount;
				}
			}

			if(Array.IndexOf<char>(vGenerated, char.MinValue) >= 0)
			{
				Debug.Assert(false);
				throw new System.Security.SecurityException();
			}

			byte[] pbUTF8 = utf8.GetBytes(vGenerated);
			ProtectedString ps = new ProtectedString(bReturnProtected, pbUTF8);
			Array.Clear(pbUTF8, 0, pbUTF8.Length);
			Array.Clear(vGenerated, 0, vGenerated.Length);
			return ps;
		}

		private static ProtectedString PatternGenerate(string strPattern,
			bool bReturnProtected, byte[] pbAdditionalEntropy)
		{
			LinkedList<char> vGenerated = new LinkedList<char>();
			CryptoRandomStream crs = CreateCryptoStream(pbAdditionalEntropy);
			byte[] pbRandom = crs.GetRandomBytes(1024);
			int nRandomPos = 0;

			StringBuilder sb = new StringBuilder();
			for(char chAdd = '~'; chAdd < 255; ++chAdd)
				sb.Append(chAdd);
			string strCharSetHighANSI = sb.ToString();

			strPattern = PasswordGenerator.ExpandPattern(strPattern);
			CharStream csStream = new CharStream(strPattern);
			char ch = csStream.ReadChar();

			while(ch != char.MinValue)
			{
				string strCurrentCharSet = string.Empty;

				switch(ch)
				{
					case '\\':
						ch = csStream.ReadChar();
						if(ch == char.MinValue) // Backslash at the end
						{
							vGenerated.AddLast('\\');
							break;
						}

						vGenerated.AddLast(ch);
						break;

					case 'a': strCurrentCharSet = CharSetLower + CharSetNumeric; break;
					case 'A': strCurrentCharSet = CharSetLower + CharSetUpper +
						CharSetNumeric; break;
					case '\u00C2': strCurrentCharSet = CharSetUpper + CharSetNumeric; break;
					case 'c': strCurrentCharSet = CharSetLowerConsonants; break;
					case 'C': strCurrentCharSet = CharSetLowerConsonants +
						CharSetUpperConsonants; break;
					case '\u0108': strCurrentCharSet = CharSetUpperConsonants; break;
					case 'd': strCurrentCharSet = CharSetNumeric; break; // Digit
					case 'h': strCurrentCharSet = CharSetHexLower; break;
					case 'H': strCurrentCharSet = CharSetHexUpper; break;
					case 'l': strCurrentCharSet = CharSetLower; break;
					case 'L': strCurrentCharSet = CharSetLower + CharSetUpper; break;
					case '\u013D': strCurrentCharSet = CharSetUpper; break;
					case 'p': strCurrentCharSet = CharSetPunctuation; break;
					case 'v': strCurrentCharSet = CharSetLowerVowel; break;
					case 'V': strCurrentCharSet = CharSetLowerVowel +
						CharSetUpperVowel; break;
					case '\u0177': strCurrentCharSet = CharSetUpperVowel; break;
					case 'x': strCurrentCharSet = strCharSetHighANSI; break;
					default: vGenerated.AddLast(ch); break;
				}

				if(strCurrentCharSet.Length > 0)
				{
					while(true)
					{
						byte bt = pbRandom[nRandomPos];

						++nRandomPos;
						if(nRandomPos == pbRandom.Length)
						{
							pbRandom = crs.GetRandomBytes(1024);
							nRandomPos = 0;
						}

						if(bt < (byte)strCurrentCharSet.Length)
						{
							vGenerated.AddLast(strCurrentCharSet[(int)bt]);
							break;
						}
					}
				}

				ch = csStream.ReadChar();
			}

			char[] vArray = new char[vGenerated.Count];
			vGenerated.CopyTo(vArray, 0);
			UTF8Encoding utf8 = new UTF8Encoding();
			ProtectedString ps = new ProtectedString(bReturnProtected, utf8.GetBytes(vArray));
			Array.Clear(vArray, 0, vArray.Length);
			vGenerated.Clear();
			return ps;
		}

		private static string ExpandPattern(string strPattern)
		{
			Debug.Assert(strPattern != null); if(strPattern == null) return string.Empty;
			string str = strPattern;

			while(true)
			{
				int nOpen = str.IndexOf('{');
				int nClose = str.IndexOf('}');

				if((nOpen >= 0) && (nOpen < nClose))
				{
					string strCount = str.Substring(nOpen + 1, nClose - nOpen - 1);
					str = str.Remove(nOpen, nClose - nOpen + 1);

					uint uRepeat;
					if(StrUtil.TryParseUInt(strCount, out uRepeat) && (nOpen >= 1))
					{
						if(uRepeat == 0)
							str = str.Remove(nOpen - 1, 1);
						else
							str = str.Insert(nOpen, new string(str[nOpen - 1], (int)uRepeat - 1));
					}
				}
				else break;
			}

			return str;
		}
	}
}
