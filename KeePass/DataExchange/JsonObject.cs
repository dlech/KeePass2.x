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

using KeePass.Resources;

using KeePassLib.Resources;
using KeePassLib.Utility;

namespace KeePass.DataExchange
{
	public sealed class JsonObject
	{
		private const int MaxTreeHeight = 50000;

		private readonly Dictionary<string, object> m_dItems = new Dictionary<string, object>();
		public IDictionary<string, object> Items
		{
			get { return m_dItems; }
		}

		private JsonObject()
		{
		}

		public JsonObject(CharStream csJsonData)
		{
			if(csJsonData == null) throw new ArgumentNullException("csJsonData");

			Load(csJsonData);
		}

		private static void ThrowDataException()
		{
			Debug.Assert(false);
			throw new FormatException(KLRes.InvalidDataWhileDecoding);
		}

		private static void ThrowImplException()
		{
			Debug.Assert(false);
			throw new InvalidOperationException(KLRes.UnknownError);
		}

		private void Load(CharStream cs)
		{
			char chInit = cs.ReadChar(true);
			if(chInit != '{') ThrowDataException();

			Stack<object> sCtx = new Stack<object>();
			sCtx.Push(this);

			while(sCtx.Count != 0)
			{
				if(sCtx.Count > MaxTreeHeight)
				{
					Debug.Assert(false);
					throw new InvalidOperationException(KLRes.StructsTooDeep);
				}

				object oCtx = sCtx.Peek();

				JsonObject joCtx = (oCtx as JsonObject);
				if(joCtx != null) { ReadObjectPart(cs, joCtx, sCtx); continue; }

				List<object> lCtx = (oCtx as List<object>);
				if(lCtx != null) { ReadArrayPart(cs, lCtx, sCtx); continue; }

				ThrowImplException(); // Unknown context object
			}
		}

		private static void ReadObjectPart(CharStream cs, JsonObject joCtx,
			Stack<object> sCtx)
		{
			char ch = cs.PeekChar(true);

			if(ch == '}') EndContainer(cs, joCtx, sCtx);
			else if(ch == '\"')
			{
				string strName = ReadString(cs);

				char chSep = cs.ReadChar(true);
				if(chSep != ':') ThrowDataException();

				object oSub = TryBeginContainer(cs, sCtx);
				if(oSub != null)
					joCtx.m_dItems[strName] = oSub;
				else
				{
					joCtx.m_dItems[strName] = ReadAtomicValue(cs);
					if(cs.PeekChar(true) == ',') cs.ReadChar(true);
				}
			}
			else ThrowDataException();
		}

		private static void ReadArrayPart(CharStream cs, List<object> lCtx,
			Stack<object> sCtx)
		{
			char ch = cs.PeekChar(true);

			if(ch == ']') { EndContainer(cs, lCtx, sCtx); return; }

			object oSub = TryBeginContainer(cs, sCtx);
			if(oSub != null)
				lCtx.Add(oSub);
			else
			{
				lCtx.Add(ReadAtomicValue(cs));
				if(cs.PeekChar(true) == ',') cs.ReadChar(true);
			}
		}

		private static object TryBeginContainer(CharStream cs, Stack<object> sCtx)
		{
			char ch = cs.PeekChar(true);
			object oNew = null;

			if(ch == '{') oNew = new JsonObject();
			else if(ch == '[') oNew = new List<object>();

			if(oNew != null)
			{
				cs.ReadChar(true);
				sCtx.Push(oNew);
			}
			return oNew;
		}

		private static void EndContainer(CharStream cs, object oCtx,
			Stack<object> sCtx)
		{
			Debug.Assert(object.ReferenceEquals(oCtx, sCtx.Peek())); // For Pop()

			char chTerm = cs.ReadChar(true);
			if(chTerm == '}')
			{
				if(!(oCtx is JsonObject)) ThrowImplException();
			}
			else if(chTerm == ']')
			{
				if(!(oCtx is List<object>)) ThrowImplException();
			}
			else ThrowImplException(); // Unknown terminator

			sCtx.Pop();
			if(sCtx.Count == 0) return;

			object oParent = sCtx.Peek();
			if((oParent is JsonObject) || (oParent is List<object>))
			{
				if(cs.PeekChar(true) == ',') cs.ReadChar(true);
			}
			else ThrowImplException(); // Unknown context object
		}

		private static object ReadAtomicValue(CharStream cs)
		{
			char chInit = cs.PeekChar(true);

			if(chInit == '\"') return ReadString(cs);
			if(chInit == 't')
			{
				cs.ReadChar(true);
				if(cs.ReadChar() != 'r') ThrowDataException();
				if(cs.ReadChar() != 'u') ThrowDataException();
				if(cs.ReadChar() != 'e') ThrowDataException();
				return true;
			}
			if(chInit == 'f')
			{
				cs.ReadChar(true);
				if(cs.ReadChar() != 'a') ThrowDataException();
				if(cs.ReadChar() != 'l') ThrowDataException();
				if(cs.ReadChar() != 's') ThrowDataException();
				if(cs.ReadChar() != 'e') ThrowDataException();
				return false;
			}
			if(chInit == 'n')
			{
				cs.ReadChar(true);
				if(cs.ReadChar() != 'u') ThrowDataException();
				if(cs.ReadChar() != 'l') ThrowDataException();
				if(cs.ReadChar() != 'l') ThrowDataException();
				return null;
			}

			return ReadNumber(cs);
		}

		private static string ReadString(CharStream cs)
		{
			char chInit = cs.ReadChar(true);
			if(chInit != '\"') ThrowImplException(); // Caller should ensure '\"'

			StringBuilder sb = new StringBuilder();

			while(true)
			{
				char ch = cs.ReadChar();
				if(ch == char.MinValue) ThrowDataException(); // End of JSON data
				if(ch == '\"') break; // End of string

				if(ch == '\\')
				{
					char chNext = cs.ReadChar();
					switch(chNext)
					{
						case '\"':
						case '\'': // C
						case '/':
						case '\\':
						case '?': // C
							sb.Append(chNext);
							break;

						case 'a': sb.Append('\a'); break; // C
						case 'b': sb.Append('\b'); break;
						case 'f': sb.Append('\f'); break;
						case 'n': sb.Append('\n'); break;
						case 'r': sb.Append('\r'); break;
						case 't': sb.Append('\t'); break;
						case 'v': sb.Append('\v'); break; // C

						case 'u':
						case 'x': // C, only special case supported
							sb.Append(ReadHex4Char(cs));
							break;

						default: ThrowDataException(); break;
					}
				}
				else sb.Append(ch);
			}

			return sb.ToString();
		}

		private static char ReadHex4Char(CharStream cs)
		{
			char ch1 = cs.ReadChar();
			char ch2 = cs.ReadChar();
			char ch3 = cs.ReadChar();
			char ch4 = cs.ReadChar();

			char[] vHex = new char[4] { ch1, ch2, ch3, ch4 };
			if(Array.IndexOf(vHex, char.MinValue) >= 0) ThrowDataException();

			string strHex = new string(vHex);
			if(!StrUtil.IsHexString(strHex, true)) ThrowDataException();

			return (char)Convert.ToUInt16(strHex, 16);
		}

		private static object ReadNumber(CharStream cs)
		{
			StringBuilder sb = new StringBuilder();
			bool bFloat = false, bNeg = false;

			while(true)
			{
				char ch = cs.PeekChar(true);

				if(((ch >= '0') && (ch <= '9')) || (ch == '+')) { }
				else if(ch == '-') bNeg = true;
				else if(ch == '.') bFloat = true;
				else if((ch == 'e') || (ch == 'E')) bFloat = true;
				else break;

				cs.ReadChar(true);
				sb.Append(ch);
			}
			if(sb.Length == 0) ThrowDataException();

			string str = sb.ToString();
			NumberFormatInfo nfi = NumberFormatInfo.InvariantInfo;

			if(bFloat)
			{
				double d;
				if(!double.TryParse(str, NumberStyles.Float, nfi, out d))
					ThrowDataException();
				return d;
			}

			if(bNeg)
			{
				long i;
				if(!long.TryParse(str, NumberStyles.Integer, nfi, out i))
					ThrowDataException();
				return i;
			}

			ulong u;
			if(!ulong.TryParse(str, NumberStyles.Integer, nfi, out u))
				ThrowDataException();
			return u;
		}

		private static T ConvertOrDefault<T>(object o, T tDefault)
			where T : struct // Use 'as' for class
		{
			if(o == null) return tDefault;

			try
			{
				if(o is T) return (T)o;
				return (T)Convert.ChangeType(o, typeof(T));
			}
			catch(Exception) { Debug.Assert(false); }

			try { return (T)o; }
			catch(Exception) { Debug.Assert(false); }

			return tDefault;
		}

		public T GetValue<T>(string strKey)
			where T : class
		{
			if(strKey == null) throw new ArgumentNullException("strKey");

			object o;
			m_dItems.TryGetValue(strKey, out o);
			return (o as T);
		}

		public T GetValue<T>(string strKey, T tDefault)
			where T : struct
		{
			if(strKey == null) throw new ArgumentNullException("strKey");

			object o;
			m_dItems.TryGetValue(strKey, out o);
			return ConvertOrDefault(o, tDefault);
		}

		public T[] GetValueArray<T>(string strKey)
			where T : class
		{
			return GetValueArray<T>(strKey, false);
		}

		internal T[] GetValueArray<T>(string strKey, bool bEmptyIfNotExists)
			where T : class
		{
			List<object> lO = GetValue<List<object>>(strKey);
			if(lO == null) return (bEmptyIfNotExists ? MemUtil.EmptyArray<T>() : null);

			T[] vT = new T[lO.Count];
			for(int i = 0; i < lO.Count; ++i) vT[i] = (lO[i] as T);

			return vT;
		}

		public T[] GetValueArray<T>(string strKey, T tDefault)
			where T : struct
		{
			return GetValueArray<T>(strKey, tDefault, false);
		}

		internal T[] GetValueArray<T>(string strKey, T tDefault, bool bEmptyIfNotExists)
			where T : struct
		{
			List<object> lO = GetValue<List<object>>(strKey);
			if(lO == null) return (bEmptyIfNotExists ? MemUtil.EmptyArray<T>() : null);

			T[] vT = new T[lO.Count];
			for(int i = 0; i < lO.Count; ++i)
				vT[i] = ConvertOrDefault(lO[i], tDefault);

			return vT;
		}
	}
}
