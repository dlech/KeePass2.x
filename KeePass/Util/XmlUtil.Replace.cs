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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

using KeePass.Forms;
using KeePass.Resources;

using KeePassLib;
using KeePassLib.Delegates;
using KeePassLib.Interfaces;
using KeePassLib.Security;
using KeePassLib.Serialization;
using KeePassLib.Utility;

namespace KeePass.Util
{
	[Flags]
	public enum XmlReplaceFlags
	{
		None = 0x0,
		StatusUI = 0x1,

		CaseSensitive = 0x10,
		Regex = 0x20
	}

	public enum XmlReplaceOp
	{
		None = 0,
		RemoveNodes,
		ReplaceData
	}

	public enum XmlReplaceData
	{
		None = 0,
		InnerText,
		InnerXml,
		OuterXml
	}

	public sealed class XmlReplaceOptions
	{
		private XmlReplaceFlags m_f = XmlReplaceFlags.None;
		public XmlReplaceFlags Flags
		{
			get { return m_f; }
			set { m_f = value; }
		}

		private string m_strSelXPath = string.Empty;
		public string SelectNodesXPath
		{
			get { return m_strSelXPath; }
			set
			{
				if(value == null) { Debug.Assert(false); m_strSelXPath = string.Empty; }
				else m_strSelXPath = value;
			}
		}

		private XmlReplaceOp m_op = XmlReplaceOp.None;
		public XmlReplaceOp Operation
		{
			get { return m_op; }
			set { m_op = value; }
		}

		private XmlReplaceData m_d = XmlReplaceData.None;
		public XmlReplaceData Data
		{
			get { return m_d; }
			set { m_d = value; }
		}

		private string m_strFind = string.Empty;
		public string FindText
		{
			get { return m_strFind; }
			set
			{
				if(value == null) { Debug.Assert(false); m_strFind = string.Empty; }
				else m_strFind = value;
			}
		}

		private string m_strReplace = string.Empty;
		public string ReplaceText
		{
			get { return m_strReplace; }
			set
			{
				if(value == null) { Debug.Assert(false); m_strReplace = string.Empty; }
				else m_strReplace = value;
			}
		}

		private Form m_fParent = null;
		public Form ParentForm
		{
			get { return m_fParent; }
			set { m_fParent = value; }
		}

		public XmlReplaceOptions() { }
	}

	public static partial class XmlUtil
	{
		public static void Replace(PwDatabase pd, XmlReplaceOptions opt)
		{
			if(pd == null) { Debug.Assert(false); return; }
			if(opt == null) { Debug.Assert(false); return; }

			StatusProgressForm dlg = null;
			try
			{
				if((opt.Flags & XmlReplaceFlags.StatusUI) != XmlReplaceFlags.None)
					dlg = StatusProgressForm.ConstructEx(KPRes.XmlReplace,
						true, false, opt.ParentForm, KPRes.XmlReplace + "...");

				PerformXmlReplace(pd, opt, dlg);
			}
			finally
			{
				if(dlg != null) StatusProgressForm.DestroyEx(dlg);
			}
		}

		private static void PerformXmlReplace(PwDatabase pd, XmlReplaceOptions opt,
			IStatusLogger sl)
		{
			if(opt.SelectNodesXPath.Length == 0) return;
			if(opt.Operation == XmlReplaceOp.None) return;

			bool bRemove = (opt.Operation == XmlReplaceOp.RemoveNodes);
			bool bReplace = (opt.Operation == XmlReplaceOp.ReplaceData);
			bool bMatchCase = ((opt.Flags & XmlReplaceFlags.CaseSensitive) != XmlReplaceFlags.None);
			bool bRegex = ((opt.Flags & XmlReplaceFlags.Regex) != XmlReplaceFlags.None);

			Regex rxFind = null;
			if(bReplace && bRegex)
				rxFind = new Regex(opt.FindText, (bMatchCase ? RegexOptions.None :
					RegexOptions.IgnoreCase));

			EnsureStandardFieldsExist(pd);

			XmlDocument xd;
			XPathNodeIterator xpIt = XmlUtilEx.FindNodes(pd, opt.SelectNodesXPath,
				sl, out xd);

			// XPathNavigators must be cloned to make them independent
			List<XPathNavigator> lNodes = new List<XPathNavigator>();
			while(xpIt.MoveNext()) lNodes.Add(xpIt.Current.Clone());

			if(lNodes.Count == 0) return;

			for(int i = lNodes.Count - 1; i >= 0; --i)
			{
				if((sl != null) && !sl.ContinueWork()) return;

				XPathNavigator xpNav = lNodes[i];

				if(bRemove) xpNav.DeleteSelf();
				else if(bReplace) ApplyReplace(xpNav, opt, rxFind);
				else { Debug.Assert(false); } // Unknown action
			}

			PwDatabase pdMod = new PwDatabase();
			try
			{
				using(MemoryStream msW = new MemoryStream())
				{
					using(XmlWriter xw = XmlUtilEx.CreateXmlWriter(msW))
					{
						xd.Save(xw);
					}

					using(MemoryStream msR = new MemoryStream(msW.ToArray(), false))
					{
						KdbxFile kdbx = new KdbxFile(pdMod);
						kdbx.Load(msR, KdbxFormat.PlainXml, sl);
					}
				}
			}
			catch(Exception ex)
			{
				throw new ExtendedException(KPRes.XmlModInvalid, ex,
					KPRes.OpAborted + " " + KPRes.DbNoModBy.Replace("{PARAM}",
					"'" + KPRes.XmlReplace + "'"));
			}

			PrepareModDbForMerge(pdMod, pd);

			pd.Modified = true;
			pd.UINeedsIconUpdate = true;
			pd.MergeIn(pdMod, PwMergeMethod.Synchronize, sl);
		}

		private static void ApplyReplace(XPathNavigator xpNav, XmlReplaceOptions opt,
			Regex rxFind)
		{
			string strData;
			if(opt.Data == XmlReplaceData.InnerText) strData = xpNav.Value;
			else if(opt.Data == XmlReplaceData.InnerXml) strData = xpNav.InnerXml;
			else if(opt.Data == XmlReplaceData.OuterXml) strData = xpNav.OuterXml;
			else return;
			if(strData == null) { Debug.Assert(false); strData = string.Empty; }

			string str;
			if(rxFind != null) str = rxFind.Replace(strData, opt.ReplaceText);
			else
			{
				if((opt.Flags & XmlReplaceFlags.CaseSensitive) != XmlReplaceFlags.None)
					str = strData.Replace(opt.FindText, opt.ReplaceText);
				else
					str = StrUtil.ReplaceCaseInsensitive(strData, opt.FindText,
						opt.ReplaceText);
			}

			if((str != null) && (str != strData))
			{
				if(opt.Data == XmlReplaceData.InnerText)
					xpNav.SetValue(str);
				else if(opt.Data == XmlReplaceData.InnerXml)
					xpNav.InnerXml = str;
				else if(opt.Data == XmlReplaceData.OuterXml)
					xpNav.OuterXml = str;
				else { Debug.Assert(false); }
			}
		}

		private static void PrepareModDbForMerge(PwDatabase pd, PwDatabase pdOrg)
		{
			PwGroup pgRootOrg = pdOrg.RootGroup;
			PwGroup pgRootNew = pd.RootGroup;
			if(pgRootNew == null) { Debug.Assert(false); return; }

			const PwCompareOptions cmpOpt = (PwCompareOptions.IgnoreParentGroup |
				PwCompareOptions.IgnoreHistory | PwCompareOptions.NullEmptyEquivStd);
			const MemProtCmpMode cmpMem = MemProtCmpMode.CustomOnly;
			DateTime dtNow = DateTime.UtcNow;

			GroupHandler ghOrg = delegate(PwGroup pg)
			{
				PwGroup pgNew = pgRootNew.FindGroup(pg.Uuid, true);
				if(pgNew == null)
				{
					AddDeletedObject(pd, pg.Uuid);
					return true;
				}

				if(!pgNew.EqualsGroup(pg, (cmpOpt | PwCompareOptions.PropertiesOnly),
					cmpMem))
					pgNew.Touch(true, false);

				PwGroup pgParentA = pg.ParentGroup;
				PwGroup pgParentB = pgNew.ParentGroup;
				if((pgParentA != null) && (pgParentB != null))
				{
					if(!pgParentA.Uuid.Equals(pgParentB.Uuid))
						pgNew.LocationChanged = dtNow;
				}
				else if((pgParentA == null) && (pgParentB == null)) { }
				else pgNew.LocationChanged = dtNow;

				return true;
			};

			EntryHandler ehOrg = delegate(PwEntry pe)
			{
				PwEntry peNew = pgRootNew.FindEntry(pe.Uuid, true);
				if(peNew == null)
				{
					AddDeletedObject(pd, pe.Uuid);
					return true;
				}

				// Restore history entries
				peNew.History = pe.History.CloneDeep();
				foreach(PwEntry peHistNew in peNew.History)
					peHistNew.ParentGroup = peNew.ParentGroup;

				if(!peNew.EqualsEntry(pe, cmpOpt, cmpMem))
					peNew.Touch(true, false);

				PwGroup pgParentA = pe.ParentGroup;
				PwGroup pgParentB = peNew.ParentGroup;
				if((pgParentA != null) && (pgParentB != null))
				{
					if(!pgParentA.Uuid.Equals(pgParentB.Uuid))
						peNew.LocationChanged = dtNow;
				}
				else if((pgParentA == null) && (pgParentB == null)) { }
				else peNew.LocationChanged = dtNow;

				return true;
			};

			pgRootOrg.TraverseTree(TraversalMethod.PreOrder, ghOrg, ehOrg);
		}

		private static void AddDeletedObject(PwDatabase pd, PwUuid pu)
		{
			foreach(PwDeletedObject pdo in pd.DeletedObjects)
			{
				if(pdo.Uuid.Equals(pu)) { Debug.Assert(false); return; }
			}

			PwDeletedObject pdoNew = new PwDeletedObject(pu, DateTime.UtcNow);
			pd.DeletedObjects.Add(pdoNew);
		}

		private static void EnsureStandardFieldsExist(PwDatabase pd)
		{
			List<string> l = PwDefs.GetStandardFields();

			EntryHandler eh = delegate(PwEntry pe)
			{
				foreach(string strName in l)
				{
					ProtectedString ps = pe.Strings.Get(strName);
					if(ps == null)
						pe.Strings.Set(strName, new ProtectedString(
							pd.MemoryProtection.GetProtection(strName), string.Empty));
				}

				return true;
			};

			pd.RootGroup.TraverseTree(TraversalMethod.PreOrder, null, eh);
		}
	}
}
