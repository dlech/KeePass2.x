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
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

using KeePass.Util.XmlSerialization;

using KeePassLib.Utility;

namespace KeePass.Util
{
	public static partial class XmlUtil
	{
		public static string SafeInnerText(XmlNode xmlNode)
		{
			if(xmlNode == null) { Debug.Assert(false); return string.Empty; }

			return (xmlNode.InnerText ?? string.Empty);
		}

		public static string SafeInnerText(XmlNode xmlNode, string strNewLineCode)
		{
			string str = SafeInnerText(xmlNode);

			if(!string.IsNullOrEmpty(strNewLineCode))
				str = str.Replace(strNewLineCode, MessageService.NewLine);

			return str;
		}

		public static string SafeInnerXml(XmlNode xmlNode)
		{
			if(xmlNode == null) { Debug.Assert(false); return string.Empty; }

			return (xmlNode.InnerXml ?? string.Empty);
		}

		internal static string SafeInnerXml(XmlNode xmlNode, string strXPath)
		{
			if(xmlNode == null) { Debug.Assert(false); return string.Empty; }
			if(string.IsNullOrEmpty(strXPath)) return SafeInnerXml(xmlNode);

			XmlNode xnSub = xmlNode.SelectSingleNode(strXPath);
			return ((xnSub != null) ? SafeInnerXml(xnSub) : string.Empty);
		}

		public static string SafeInnerText(HtmlElement htmlNode)
		{
			if(htmlNode == null) { Debug.Assert(false); return string.Empty; }

			return (htmlNode.InnerText ?? string.Empty);
		}

		public static string SafeAttribute(HtmlElement htmlNode, string strName)
		{
			if(htmlNode == null) { Debug.Assert(false); return string.Empty; }
			if(string.IsNullOrEmpty(strName)) { Debug.Assert(false); return string.Empty; }

			string strValue = (htmlNode.GetAttribute(strName) ?? string.Empty);

			// https://msdn.microsoft.com/en-us/library/ie/ms536429.aspx
			if((strValue.Length == 0) && strName.Equals("class", StrUtil.CaseIgnoreCmp))
				strValue = (htmlNode.GetAttribute("className") ?? string.Empty);

			return strValue;
		}

		private static Dictionary<string, char> g_dHtmlEntities = null;
		/// <summary>
		/// Decode common HTML entities except the XML ones (&lt;, &gt;, etc.).
		/// </summary>
		/// <param name="strXml">String to decode.</param>
		/// <returns>String without non-standard entities.</returns>
		public static string DecodeHtmlEntities(string strXml)
		{
			if(strXml == null) { Debug.Assert(false); return string.Empty; }

			if(g_dHtmlEntities == null)
			{
				Dictionary<string, char> d = new Dictionary<string, char>
				{
					{ "nbsp", '\u00A0' }, { "iexcl", '\u00A1' },
					{ "cent", '\u00A2' }, { "pound", '\u00A3' },
					{ "curren", '\u00A4' }, { "yen", '\u00A5' },
					{ "brvbar", '\u00A6' }, { "sect", '\u00A7' },
					{ "uml", '\u00A8' }, { "copy", '\u00A9' },
					{ "ordf", '\u00AA' }, { "laquo", '\u00AB' },
					{ "not", '\u00AC' }, { "shy", '\u00AD' },
					{ "reg", '\u00AE' }, { "macr", '\u00AF' },
					{ "deg", '\u00B0' }, { "plusmn", '\u00B1' },
					{ "sup2", '\u00B2' }, { "sup3", '\u00B3' },
					{ "acute", '\u00B4' }, { "micro", '\u00B5' },
					{ "para", '\u00B6' }, { "middot", '\u00B7' },
					{ "cedil", '\u00B8' }, { "sup1", '\u00B9' },
					{ "ordm", '\u00BA' }, { "raquo", '\u00BB' },
					{ "frac14", '\u00BC' }, { "frac12", '\u00BD' },
					{ "frac34", '\u00BE' }, { "iquest", '\u00BF' },
					{ "Agrave", '\u00C0' }, { "Aacute", '\u00C1' },
					{ "Acirc", '\u00C2' }, { "Atilde", '\u00C3' },
					{ "Auml", '\u00C4' }, { "Aring", '\u00C5' },
					{ "AElig", '\u00C6' }, { "Ccedil", '\u00C7' },
					{ "Egrave", '\u00C8' }, { "Eacute", '\u00C9' },
					{ "Ecirc", '\u00CA' }, { "Euml", '\u00CB' },
					{ "Igrave", '\u00CC' }, { "Iacute", '\u00CD' },
					{ "Icirc", '\u00CE' }, { "Iuml", '\u00CF' },
					{ "ETH", '\u00D0' }, { "Ntilde", '\u00D1' },
					{ "Ograve", '\u00D2' }, { "Oacute", '\u00D3' },
					{ "Ocirc", '\u00D4' }, { "Otilde", '\u00D5' },
					{ "Ouml", '\u00D6' }, { "times", '\u00D7' },
					{ "Oslash", '\u00D8' }, { "Ugrave", '\u00D9' },
					{ "Uacute", '\u00DA' }, { "Ucirc", '\u00DB' },
					{ "Uuml", '\u00DC' }, { "Yacute", '\u00DD' },
					{ "THORN", '\u00DE' }, { "szlig", '\u00DF' },
					{ "agrave", '\u00E0' }, { "aacute", '\u00E1' },
					{ "acirc", '\u00E2' }, { "atilde", '\u00E3' },
					{ "auml", '\u00E4' }, { "aring", '\u00E5' },
					{ "aelig", '\u00E6' }, { "ccedil", '\u00E7' },
					{ "egrave", '\u00E8' }, { "eacute", '\u00E9' },
					{ "ecirc", '\u00EA' }, { "euml", '\u00EB' },
					{ "igrave", '\u00EC' }, { "iacute", '\u00ED' },
					{ "icirc", '\u00EE' }, { "iuml", '\u00EF' },
					{ "eth", '\u00F0' }, { "ntilde", '\u00F1' },
					{ "ograve", '\u00F2' }, { "oacute", '\u00F3' },
					{ "ocirc", '\u00F4' }, { "otilde", '\u00F5' },
					{ "ouml", '\u00F6' }, { "divide", '\u00F7' },
					{ "oslash", '\u00F8' }, { "ugrave", '\u00F9' },
					{ "uacute", '\u00FA' }, { "ucirc", '\u00FB' },
					{ "uuml", '\u00FC' }, { "yacute", '\u00FD' },
					{ "thorn", '\u00FE' }, { "yuml", '\u00FF' }
				};
				Debug.Assert(d.Count == (0xFF - 0xA0 + 1));
				d["circ"] = '\u02C6';
				d["tilde"] = '\u02DC';
				d["euro"] = '\u20AC';

				g_dHtmlEntities = d;

#if DEBUG
				Dictionary<char, bool> dChars = new Dictionary<char, bool>();
				foreach(KeyValuePair<string, char> kvp in d)
				{
					Debug.Assert((kvp.Key.IndexOf('&') < 0) &&
						(kvp.Key.IndexOf(';') < 0));
					dChars[kvp.Value] = true;
				}
				Debug.Assert(dChars.Count == d.Count); // Injective

				Debug.Assert(DecodeHtmlEntities("&euro; A &ecircZ; B &copy;") ==
					"\u20AC A &ecircZ; B \u00A9");
				Debug.Assert(DecodeHtmlEntities(@"&lt;&gt;&amp;&apos;&quot;") ==
					"&lt;&gt;&amp;&apos;&quot;"); // Do not decode XML entities
				Debug.Assert(DecodeHtmlEntities(@"<![CDATA[&euro;]]>&euro;<![CDATA[&euro;]]>") ==
					"<![CDATA[&euro;]]>\u20AC<![CDATA[&euro;]]>");
#endif
			}

			StringBuilder sb = new StringBuilder();
			List<string> lParts = SplitByCData(strXml);

			foreach(string str in lParts)
			{
				if(str.StartsWith(@"<![CDATA["))
				{
					sb.Append(str);
					continue;
				}

				int iOffset = 0;
				while(iOffset < str.Length)
				{
					int pAmp = str.IndexOf('&', iOffset);

					if(pAmp < 0)
					{
						sb.Append(str, iOffset, str.Length - iOffset);
						break;
					}

					if(pAmp > iOffset)
						sb.Append(str, iOffset, pAmp - iOffset);

					int pTerm = str.IndexOf(';', pAmp);
					if(pTerm < 0)
					{
						Debug.Assert(false); // At least one entity not terminated
						sb.Append(str, pAmp, str.Length - pAmp);
						break;
					}

					string strEntity = str.Substring(pAmp + 1, pTerm - pAmp - 1);
					char ch;
					if(g_dHtmlEntities.TryGetValue(strEntity, out ch))
						sb.Append(ch);
					else sb.Append(str, pAmp, pTerm - pAmp + 1);

					iOffset = pTerm + 1;
				}
			}

			return sb.ToString();
		}

		public static List<string> SplitByCData(string str)
		{
			List<string> l = new List<string>();
			if(str == null) { Debug.Assert(false); return l; }

			int iOffset = 0;
			while(iOffset < str.Length)
			{
				int pStart = str.IndexOf(@"<![CDATA[", iOffset);

				if(pStart < 0)
				{
					l.Add(str.Substring(iOffset, str.Length - iOffset));
					break;
				}

				if(pStart > iOffset)
					l.Add(str.Substring(iOffset, pStart - iOffset));

				const string strTerm = @"]]>";
				int pTerm = str.IndexOf(strTerm, pStart);
				if(pTerm < 0)
				{
					Debug.Assert(false); // At least one CDATA not terminated
					l.Add(str.Substring(pStart, str.Length - pStart));
					break;
				}

				l.Add(str.Substring(pStart, pTerm - pStart + strTerm.Length));

				iOffset = pTerm + strTerm.Length;
			}

			Debug.Assert(l.IndexOf(string.Empty) < 0);
			return l;
		}

		public static int GetMultiChildIndex(XmlNodeList xlList, XmlNode xnFind)
		{
			if(xlList == null) throw new ArgumentNullException("xlList");
			if(xnFind == null) throw new ArgumentNullException("xnFind");

			string strChildName = xnFind.Name;
			int iChild = 0;
			for(int i = 0; i < xlList.Count; ++i)
			{
				if(xlList[i] == xnFind) return iChild;
				if(xlList[i].Name == strChildName) ++iChild;
			}

			return -1;
		}

		public static XmlNode FindMultiChild(XmlNodeList xlList, string strChildName,
			int iMultiChild)
		{
			if(xlList == null) throw new ArgumentNullException("xlList");
			if(strChildName == null) throw new ArgumentNullException("strChildName");

			int iChild = 0;
			for(int i = 0; i < xlList.Count; ++i)
			{
				if(xlList[i].Name == strChildName)
				{
					if(iChild == iMultiChild) return xlList[i];
					++iChild;
				}
			}

			return null;
		}

		private sealed class XuopContainer
		{
			private readonly object m_o;
			public object Object { get { return m_o; } }

			public XuopContainer(object o)
			{
				m_o = o;
			}
		}

		private const string GoxpSep = "/";
		public static string GetObjectXmlPath(object oContainer, object oNeedle)
		{
			if(oContainer == null) { Debug.Assert(false); return null; }

			XuopContainer c = new XuopContainer(oContainer);
			string strXPath = GetObjectXmlPathRec(c, typeof(XuopContainer),
				oNeedle, string.Empty);
			if(string.IsNullOrEmpty(strXPath)) return strXPath;

			if(!strXPath.StartsWith("/Object")) { Debug.Assert(false); return null; }
			strXPath = strXPath.Substring(7);

			Type tRoot = oContainer.GetType();
			string strRoot = XmlSerializerEx.GetXmlName(tRoot);

			return (GoxpSep + strRoot + strXPath);
		}

		private static string GetObjectXmlPathRec(object oContainer, Type tContainer,
			object oNeedle, string strCurPath)
		{
			if(oContainer == null) { Debug.Assert(false); return null; }
			if(oNeedle == null) { Debug.Assert(false); return null; }
			Debug.Assert(oContainer.GetType() == tContainer);

			PropertyInfo[] vProps = tContainer.GetProperties();
			foreach(PropertyInfo pi in vProps)
			{
				if((pi == null) || !pi.CanRead) continue;

				object[] vPropAttribs = pi.GetCustomAttributes(true);
				if(XmlSerializerEx.GetAttribute<XmlIgnoreAttribute>(
					vPropAttribs) != null) continue;

				object oSub = pi.GetValue(oContainer, null);
				if(oSub == null) continue;

				string strPropName = XmlSerializerEx.GetXmlName(pi);
				string strSubPath = strCurPath + GoxpSep + strPropName;

				if(oSub == oNeedle) return strSubPath;

				Type tSub = oSub.GetType();
				string strPrimarySubType;
				string strPropTypeCS = XmlSerializerEx.GetFullTypeNameCS(tSub,
					out strPrimarySubType);
				if(XmlSerializerEx.TypeIsArray(strPropTypeCS) ||
					XmlSerializerEx.TypeIsList(strPropTypeCS))
					continue;
				if(strPropTypeCS.StartsWith("System.")) continue;

				string strSubFound = GetObjectXmlPathRec(oSub, tSub, oNeedle,
					strSubPath);
				if(strSubFound != null) return strSubFound;
			}

			return null;
		}

		internal static string GetPath(XmlElement xe)
		{
			if(xe == null) { Debug.Assert(false); return null; }

			StringBuilder sb = new StringBuilder();
			GetPathRec(sb, xe);
			return sb.ToString();
		}

		private static void GetPathRec(StringBuilder sb, XmlNode xn)
		{
			XmlNode xnP = xn.ParentNode;
			if(xnP != null)
			{
				if(xnP.NodeType == XmlNodeType.Element)
					GetPathRec(sb, xnP);
				else { Debug.Assert(xnP.NodeType == XmlNodeType.Document); }
			}
			else { Debug.Assert(false); }

			Debug.Assert(xn.NodeType == XmlNodeType.Element);
			sb.Append('/');
			sb.Append(xn.Name);
		}

		internal static XmlElement FindOrCreateElement(XmlNode xnBase,
			string strChildPath, XmlDocument xd)
		{
			if(xnBase == null) { Debug.Assert(false); return null; }
			if(string.IsNullOrEmpty(strChildPath)) { Debug.Assert(false); return null; }
			if(xd == null) { Debug.Assert(false); return null; }

			if(strChildPath.IndexOf('/') >= 0)
			{
				string[] v = strChildPath.Split('/');
				if((v == null) || (v.Length < 2)) { Debug.Assert(false); return null; }

				XmlElement xeCur = FindOrCreateElement(xnBase, v[0], xd);
				for(int i = 1; i < v.Length; ++i)
					xeCur = FindOrCreateElement(xeCur, v[i], xd);

				return xeCur;
			}

			foreach(XmlNode xnChild in xnBase.ChildNodes)
			{
				if((xnChild.NodeType == XmlNodeType.Element) &&
					(xnChild.Name == strChildPath))
				{
					XmlElement xeChild = (xnChild as XmlElement);
					Debug.Assert(xeChild != null);
					return xeChild;
				}
			}

			XmlElement xeNew = xd.CreateElement(strChildPath);
			xnBase.AppendChild(xeNew);
			return xeNew;
		}
	}
}
