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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Xml;

namespace KeePass.Util
{
	internal enum XmNodeMode
	{
		None = 0,
		Create,
		Open,
		OpenOrCreate,
		Remove
	}

	internal enum XmContentMode
	{
		None = 0,
		Merge,
		Replace
	}

	internal enum XmChildrenOtherMode
	{
		None = 0,
		Remove
	}

	internal enum XmChildrenSortOrder
	{
		None = 0,
		Other,
		This
	}

	internal delegate void XmNodeOptionsDelegate(XmNodeOptions o, string strXPath);
	internal delegate string XmNodeKeyDelegate(XmlNode xn, string strXPath);

	internal sealed class XmNodeOptions
	{
		internal const string AttribNodeMode = "MergeNodeMode";

		private XmNodeMode m_nm = XmNodeMode.OpenOrCreate;
		public XmNodeMode NodeMode
		{
			get { return m_nm; }
			set { m_nm = value; }
		}

		private XmContentMode m_cm = XmContentMode.Merge;
		public XmContentMode ContentMode
		{
			get { return m_cm; }
			set { m_cm = value; }
		}

		private XmChildrenOtherMode m_com = XmChildrenOtherMode.None;
		public XmChildrenOtherMode ChildrenOtherMode
		{
			get { return m_com; }
			// set { m_com = value; }
		}

		private XmChildrenSortOrder m_cso = XmChildrenSortOrder.None;
		public XmChildrenSortOrder ChildrenSortOrder
		{
			get { return m_cso; }
			// set { m_cso = value; }
		}

		public XmNodeOptions() { }

		public void LoadElementAttributes(XmlElement xe)
		{
			if(xe == null) { Debug.Assert(false); return; }

			string str = xe.GetAttribute(AttribNodeMode);
			if(!string.IsNullOrEmpty(str))
			{
				switch(str)
				{
					// case "None": m_nm = XmNodeMode.None; break;
					case "Create": m_nm = XmNodeMode.Create; break;
					case "Open": m_nm = XmNodeMode.Open; break;
					case "OpenOrCreate": m_nm = XmNodeMode.OpenOrCreate; break;
					case "Remove": m_nm = XmNodeMode.Remove; break;
					default: Debug.Assert(false); break;
				}
			}

			str = xe.GetAttribute("MergeContentMode");
			if(!string.IsNullOrEmpty(str))
			{
				switch(str)
				{
					// case "None": m_cm = XmContentMode.None; break;
					case "Merge": m_cm = XmContentMode.Merge; break;
					case "Replace": m_cm = XmContentMode.Replace; break;
					default: Debug.Assert(false); break;
				}
			}

			str = xe.GetAttribute("MergeChildrenOtherMode");
			if(!string.IsNullOrEmpty(str))
			{
				switch(str)
				{
					case "None": m_com = XmChildrenOtherMode.None; break;
					case "Remove": m_com = XmChildrenOtherMode.Remove; break;
					default: Debug.Assert(false); break;
				}
			}

			str = xe.GetAttribute("MergeChildrenSortOrder");
			if(!string.IsNullOrEmpty(str))
			{
				switch(str)
				{
					// case "None": m_cso = XmChildrenSortOrder.None; break;
					case "Other": m_cso = XmChildrenSortOrder.Other; break;
					case "This": m_cso = XmChildrenSortOrder.This; break;
					default: Debug.Assert(false); break;
				}
			}
		}
	}

	internal sealed class XmContext
	{
		private readonly XmlDocument m_xdBase;
		public XmlDocument BaseDocument
		{
			get { return m_xdBase; }
		}

		private readonly XmNodeOptionsDelegate m_fnGetNodeOptions;
		public XmNodeOptionsDelegate GetNodeOptions
		{
			get { return m_fnGetNodeOptions; }
		}

		private readonly XmNodeKeyDelegate m_fnGetNodeKey;
		public XmNodeKeyDelegate GetNodeKey
		{
			get { return m_fnGetNodeKey; }
		}

		public XmContext(XmlDocument xdBase, XmNodeOptionsDelegate fnGetNodeOptions,
			XmNodeKeyDelegate fnGetNodeKey)
		{
			// xdBase may be null
			if(fnGetNodeOptions == null) throw new ArgumentNullException("fnGetNodeOptions");
			if(fnGetNodeKey == null) throw new ArgumentNullException("fnGetNodeKey");

			m_xdBase = xdBase;
			m_fnGetNodeOptions = fnGetNodeOptions;
			m_fnGetNodeKey = fnGetNodeKey;
		}
	}

	public static partial class XmlUtil
	{
		private sealed class XmElementCollection : IEnumerable<KeyValuePair<string, XmlElement>>,
			IEnumerable
		{
			private List<KeyValuePair<string, XmlElement>> m_l =
				new List<KeyValuePair<string, XmlElement>>();
			private Dictionary<string, XmlElement> m_d =
				new Dictionary<string, XmlElement>();

			public IEnumerator<KeyValuePair<string, XmlElement>> GetEnumerator()
			{
				return m_l.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return m_l.GetEnumerator();
			}

			public XmlElement this[string strKey]
			{
				get
				{
					if(strKey == null) { Debug.Assert(false); return null; }

					XmlElement xe;
					m_d.TryGetValue(strKey, out xe);
					return xe;
				}
			}

			private XmElementCollection() { }

			public static XmElementCollection FromChildNodes(XmlElement xeParent,
				string strParentXPath, XmContext ctx)
			{
				XmElementCollection c = new XmElementCollection();
				if(xeParent == null) { Debug.Assert(false); return c; }
				if(strParentXPath == null) { Debug.Assert(false); return c; }
				if(ctx == null) { Debug.Assert(false); return c; }

				Dictionary<string, XmlElement> d = c.m_d;

				foreach(XmlNode xn in xeParent.ChildNodes)
				{
					XmlElement xe = (xn as XmlElement);
					if(xe == null) continue;

					string strNameC = xe.Name;
					string strXPathC = strParentXPath + "/" + strNameC;
					string strKeyCustomC = ctx.GetNodeKey(xe, strXPathC);

					string strKey = BuildNodeKey(strNameC, strKeyCustomC, 0);
					if(d.ContainsKey(strKey))
					{
						// In general, custom node keys should be unique
						Debug.Assert(string.IsNullOrEmpty(strKeyCustomC));

						for(int i = d.Count; i >= 0; --i)
						{
							string strKeyTest = BuildNodeKey(strNameC, strKeyCustomC, i);
							if(d.ContainsKey(strKeyTest))
							{
								Debug.Assert(i != d.Count); // Key must change in first iteration
								break;
							}

							strKey = strKeyTest;
						}
						Debug.Assert(!d.ContainsKey(strKey));
					}

					c.Add(strKey, xe);
				}

				return c;
			}

			private static string BuildNodeKey(string strName, string strKeyCustom,
				int iIndex)
			{
				if(string.IsNullOrEmpty(strName)) { Debug.Assert(false); strName = string.Empty; }
				if(iIndex < 0) throw new ArgumentOutOfRangeException("iIndex");

				StringBuilder sb = new StringBuilder();
				sb.Append(strName);
				sb.Append(' '); // Sep. must not be a valid XML element name

				if(!string.IsNullOrEmpty(strKeyCustom)) sb.Append(strKeyCustom);
				sb.Append(' ');

				string strIndex = iIndex.ToString(NumberFormatInfo.InvariantInfo);
				if(strIndex.Length <= 10) sb.Append('0', 10 - strIndex.Length);
				else { Debug.Assert(false); }
				sb.Append(strIndex);

				return sb.ToString();
			}

			public void Add(string strKey, XmlElement xe)
			{
				if(strKey == null) { Debug.Assert(false); return; }
				if(xe == null) { Debug.Assert(false); return; }

				if(!m_d.ContainsKey(strKey))
				{
					m_l.Add(new KeyValuePair<string, XmlElement>(strKey, xe));
					m_d[strKey] = xe;
				}
				else { Debug.Assert(false); }
			}

			public void Remove(string strKey)
			{
				if(strKey == null) { Debug.Assert(false); return; }

				if(m_d.Remove(strKey))
				{
					for(int i = 0; i < m_l.Count; ++i)
					{
						if(m_l[i].Key == strKey)
						{
							m_l.RemoveAt(i);
							break;
						}
					}
				}
				else { Debug.Assert(false); }
			}

			public XmElementCollection Except(List<string> lExcept)
			{
				Dictionary<string, bool> dEx = new Dictionary<string, bool>();
				if(lExcept != null)
				{
					foreach(string str in lExcept)
					{
						if(str != null) dEx[str] = true;
						else { Debug.Assert(false); }
					}
				}
				else { Debug.Assert(false); }

				XmElementCollection c = new XmElementCollection();

				foreach(KeyValuePair<string, XmlElement> kvp in m_l)
				{
					if(!dEx.ContainsKey(kvp.Key)) c.Add(kvp.Key, kvp.Value);
				}

				return c;
			}

			public void SortBy(XmElementCollection xcOrder)
			{
				if(xcOrder == null) { Debug.Assert(false); return; }

				LinkedList<KeyValuePair<string, XmlElement>> llRem =
					new LinkedList<KeyValuePair<string, XmlElement>>(m_l);

				m_l.Clear();

				foreach(KeyValuePair<string, XmlElement> kvp in xcOrder)
				{
					if(m_d.ContainsKey(kvp.Key))
					{
						for(LinkedListNode<KeyValuePair<string, XmlElement>> n =
							llRem.First; n != null; n = n.Next)
						{
							if(n.Value.Key == kvp.Key)
							{
								m_l.Add(n.Value);
								llRem.Remove(n);
								break;
							}
						}
					}
				}

				m_l.AddRange(llRem);
				Debug.Assert(m_l.Count == m_d.Count);
			}

			public void ReorderElementsOf(XmlElement xeParent)
			{
				if(xeParent == null) { Debug.Assert(false); return; }

				for(int i = m_l.Count - 1; i >= 0; --i)
					xeParent.RemoveChild(m_l[i].Value);
				Debug.Assert(!HasChildElement(xeParent));

				for(int i = 0; i < m_l.Count; ++i)
					xeParent.AppendChild(m_l[i].Value);
			}
		}

		// Old, for plugins
		public static void MergeNodes(XmlDocument xd, XmlNode xn, XmlNode xnOverride)
		{
			XmlElement xeBase = (xn as XmlElement);
			XmlElement xeOverride = (xnOverride as XmlElement);
			string strXPath = ((xn != null) ? xn.Name : string.Empty);

			XmContext ctx = new XmContext(xd,
				delegate(XmNodeOptions oP, string strXPathP) { },
				delegate(XmlNode xnP, string strXPathP) { return null; });

			MergeElements(xeBase, xeOverride, strXPath, null, ctx);
		}

		internal static XmlElement MergeElements(XmlElement xeBase, XmlElement xeOverride,
			string strXPath, XmlElement xeBaseParent, XmContext ctx)
		{
			if(xeOverride == null) throw new ArgumentNullException("xeOverride");
			if(strXPath == null) throw new ArgumentNullException("strXPath");
			if(ctx == null) throw new ArgumentNullException("ctx");

			string strName = xeOverride.Name;

			Debug.Assert((xeBase == null) || (xeBase.Name == strName));
			Debug.Assert((strXPath == strName) || strXPath.EndsWith("/" + strName));
			Debug.Assert((xeBase == null) || (xeBaseParent == null) ||
				object.ReferenceEquals(xeBase.ParentNode, xeBaseParent));

			XmNodeOptions o = GetNodeOptions(xeOverride, strXPath, ctx);

			bool bContinue = false;
			switch(o.NodeMode)
			{
				case XmNodeMode.None: break;

				case XmNodeMode.Create:
					if((xeBase == null) && (xeBaseParent != null))
					{
						xeBase = ctx.BaseDocument.CreateElement(strName);
						xeBaseParent.AppendChild(xeBase);
						bContinue = true;
					}
					break;

				case XmNodeMode.Open:
					if(xeBase != null) bContinue = true;
					break;

				case XmNodeMode.OpenOrCreate:
					if(xeBase != null) bContinue = true;
					else if(xeBaseParent != null)
					{
						xeBase = ctx.BaseDocument.CreateElement(strName);
						xeBaseParent.AppendChild(xeBase);
						bContinue = true;
					}
					break;

				case XmNodeMode.Remove:
					if(xeBase != null)
					{
						if(xeBaseParent != null)
						{
							xeBaseParent.RemoveChild(xeBase);
							xeBase = null; // Return value, indicate removal
						}
						else
						{
							// Cannot remove element; clear it instead
							xeBase.InnerXml = string.Empty;
						}
					}
					break;

				default: Debug.Assert(false); break;
			}
			if(!bContinue) return xeBase;
			if(xeBase == null) { Debug.Assert(false); return null; }

			XmContentMode cm = o.ContentMode;
			if((cm == XmContentMode.Merge) && !HasChildElement(xeBase) &&
				!HasChildElement(xeOverride))
				cm = XmContentMode.Replace;

			XmElementCollection xcBase = null, xcOverride = null;
			if(cm == XmContentMode.Merge)
			{
				xcBase = XmElementCollection.FromChildNodes(xeBase, strXPath, ctx);
				xcOverride = XmElementCollection.FromChildNodes(xeOverride, strXPath, ctx);

				if(o.ChildrenOtherMode == XmChildrenOtherMode.Remove)
				{
					List<string> lRemove = new List<string>();
					foreach(KeyValuePair<string, XmlElement> kvpBaseC in xcBase)
					{
						if(xcOverride[kvpBaseC.Key] == null)
						{
							xeBase.RemoveChild(kvpBaseC.Value);
							lRemove.Add(kvpBaseC.Key);
						}
					}
					// Do not simply rebuild xcBase, because indices in
					// node keys would change
					xcBase = xcBase.Except(lRemove);
				}
			}

			switch(cm)
			{
				case XmContentMode.None: Debug.Assert(false); break; // Currently unused

				case XmContentMode.Merge:
					foreach(KeyValuePair<string, XmlElement> kvpOverrideC in xcOverride)
					{
						string strKeyC = kvpOverrideC.Key;
						XmlElement xeBaseC = xcBase[strKeyC];

						XmlElement xeOverrideC = kvpOverrideC.Value;
						string strXPathC = strXPath + "/" + xeOverrideC.Name;

						XmlElement xeBaseCNew = MergeElements(xeBaseC,
							xeOverrideC, strXPathC, xeBase, ctx);

						if(!object.ReferenceEquals(xeBaseCNew, xeBaseC))
						{
							Debug.Assert((xeBaseCNew == null) || (xeBaseC == null));
							if(xeBaseCNew == null) xcBase.Remove(strKeyC);
							else xcBase.Add(strKeyC, xeBaseCNew);
						}
					}
					break;

				case XmContentMode.Replace:
					xeBase.InnerXml = SafeInnerXml(xeOverride);
					break;

				default: Debug.Assert(false); break;
			}

			if((cm == XmContentMode.Merge) && (o.ChildrenSortOrder ==
				XmChildrenSortOrder.This))
			{
				xcBase.SortBy(xcOverride);
				xcBase.ReorderElementsOf(xeBase);
			}

			return xeBase;
		}

		internal static XmNodeOptions GetNodeOptions(XmlElement xe, string strXPath,
			XmContext ctx)
		{
			XmNodeOptions o = new XmNodeOptions();
			ctx.GetNodeOptions(o, strXPath);
			o.LoadElementAttributes(xe);
			return o;
		}

		private static bool HasChildElement(XmlElement xe)
		{
			if(xe == null) return false; // No assert

			foreach(XmlNode xnChild in xe.ChildNodes)
			{
				if(xnChild == null) { Debug.Assert(false); continue; }

				Debug.Assert(xnChild.NodeType != XmlNodeType.EndElement);
				if(xnChild.NodeType == XmlNodeType.Element)
				{
					Debug.Assert(xnChild is XmlElement);
					return true;
				}
				Debug.Assert(!(xnChild is XmlElement));
			}

			return false;
		}

		internal static bool IsAlwaysEnforced(XmlNode xn, string strXPath,
			XmContext ctx)
		{
			if(xn == null) { Debug.Assert(false); return false; }
			if(xn.NodeType == XmlNodeType.Document) return true;
			// If xn is the document node, strXPath is empty
			if(string.IsNullOrEmpty(strXPath)) { Debug.Assert(false); return false; }
			if(ctx == null) { Debug.Assert(false); return false; }

			XmlElement xe = (xn as XmlElement);
			if(xe == null) { Debug.Assert(false); return false; }

			XmNodeOptions o = GetNodeOptions(xe, strXPath, ctx);

			if((o.NodeMode == XmNodeMode.None) ||
				(o.NodeMode == XmNodeMode.Create)) return false;
			if(o.ContentMode == XmContentMode.None) return false;

			XmlNode xnP = xe.ParentNode;
			if(xnP == null) { Debug.Assert(false); return false; }
			if(object.ReferenceEquals(xnP, xn)) { Debug.Assert(false); return false; }

			int iSep = strXPath.LastIndexOf('/');
			if(iSep < 0) { Debug.Assert(false); return false; }
			string strXPathP = strXPath.Substring(0, iSep);

			return IsAlwaysEnforced(xnP, strXPathP, ctx);
		}
	}
}
