/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2013 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;
using System.Diagnostics;

using KeePass.Util.XmlSerialization;

namespace KeePass.Util
{
	public static class XmlUtil
	{
		public static string SafeInnerText(XmlNode xmlNode)
		{
			Debug.Assert(xmlNode != null); if(xmlNode == null) return string.Empty;

			string strInner = xmlNode.InnerText;
			if(strInner == null) return string.Empty;

			return strInner;
		}

		public static string SafeInnerText(XmlNode xmlNode, string strNewLineCode)
		{
			if((strNewLineCode == null) || (strNewLineCode.Length == 0))
				return SafeInnerText(xmlNode);

			string strInner = SafeInnerText(xmlNode);
			return strInner.Replace(strNewLineCode, "\r\n");
		}

		public static string SafeInnerXml(XmlNode xmlNode)
		{
			Debug.Assert(xmlNode != null); if(xmlNode == null) return string.Empty;

			string strInner = xmlNode.InnerXml;
			if(strInner == null) return string.Empty;

			return strInner;
		}

		private static readonly string[] m_vNonStandardEntitiesTable = new string[]{
			@"&sect;", @"§", @"&acute;", @"´", @"&szlig;", @"ß",
			@"&Auml;", @"Ä", @"&Ouml;", @"Ö", @"&Uuml;", @"Ü",
			@"&auml;", @"ä", @"&ouml;", @"ö", @"&uuml;", @"ü",
			@"&micro;", @"µ", @"&euro;", @"€", @"&copy;", @"©",
			@"&reg;", @"®", @"&sup2;", @"²", @"&sup3;", @"³",
			@"&eacute;", @"é", @"&egrave;", @"è", @"&ecirc;", @"ê",
			@"&Aacute;", @"Á", @"&Agrave;", @"À", @"&Acirc;", @"Â",
			@"&aacute;", @"á", @"&agrave;", @"à", @"&acirc;", @"â"
		};

		/// <summary>
		/// Decode most entities except the standard ones (&lt;, &gt;, etc.).
		/// </summary>
		/// <param name="strToDecode">String to decode.</param>
		/// <returns>String without non-standard entities.</returns>
		public static string DecodeNonStandardEntities(string strToDecode)
		{
			string str = strToDecode;

			Debug.Assert((m_vNonStandardEntitiesTable.Length % 2) == 0); // Static

			for(int i = 0; i < m_vNonStandardEntitiesTable.Length; i += 2)
				str = str.Replace(m_vNonStandardEntitiesTable[i],
					m_vNonStandardEntitiesTable[i + 1]);

			return str;
		}

		public static int GetMultiChildIndex(XmlNodeList xlList, XmlNode xnFind)
		{
			if(xlList == null) throw new ArgumentNullException("xlList");
			if(xnFind == null) throw new ArgumentNullException("xnFind");

			string strChildName = xnFind.Name;
			int nChildIndex = 0;
			for(int i = 0; i < xlList.Count; ++i)
			{
				if(xlList[i] == xnFind) return nChildIndex;
				else if(xlList[i].Name == strChildName) ++nChildIndex;
			}

			return -1;
		}

		public static XmlNode FindMultiChild(XmlNodeList xlList, string strChildName,
			int nMultiChildIndex)
		{
			if(xlList == null) throw new ArgumentNullException("xlList");
			if(strChildName == null) throw new ArgumentNullException("strChildName");

			int nChildIndex = 0;
			for(int i = 0; i < xlList.Count; ++i)
			{
				if(xlList[i].Name == strChildName)
				{
					if(nChildIndex == nMultiChildIndex) return xlList[i];
					else ++nChildIndex;
				}
			}

			return null;
		}

		public static void MergeNodes(XmlDocument xd, XmlNode xn, XmlNode xnOverride)
		{
			if(xd == null) throw new ArgumentNullException("xd");
			if(xn == null) throw new ArgumentNullException("xn");
			if(xnOverride == null) throw new ArgumentNullException("xnOverride");
			Debug.Assert(xn.Name == xnOverride.Name);

			foreach(XmlNode xnOvrChild in xnOverride.ChildNodes)
			{
				int nOvrIndex = GetMultiChildIndex(xnOverride.ChildNodes, xnOvrChild);
				if(nOvrIndex < 0) { Debug.Assert(false); continue; }

				XmlNode xnRep = FindMultiChild(xn.ChildNodes, xnOvrChild.Name, nOvrIndex);
				bool bHasSub = (XmlUtil.SafeInnerXml(xnOvrChild).IndexOf('>') >= 0);

				if(xnRep == null)
				{
					XmlNode xnNew = xd.CreateElement(xnOvrChild.Name);
					xn.AppendChild(xnNew);

					if(!bHasSub) xnNew.InnerText = XmlUtil.SafeInnerText(xnOvrChild);
					else MergeNodes(xd, xnNew, xnOvrChild);
				}
				else if(bHasSub) MergeNodes(xd, xnRep, xnOvrChild);
				else xnRep.InnerText = XmlUtil.SafeInnerText(xnOvrChild);
			}
		}

		private sealed class XuopContainer
		{
			private object m_o;
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
	}
}
