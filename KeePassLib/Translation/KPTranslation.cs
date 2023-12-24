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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

#if !KeePassUAP
using System.Drawing;
using System.Windows.Forms;
#endif

#if KeePassLibSD
using ICSharpCode.SharpZipLib.GZip;
#else
using System.IO.Compression;
#endif

using KeePassLib.Interfaces;
using KeePassLib.Utility;

namespace KeePassLib.Translation
{
	[XmlRoot("Translation")]
	public sealed class KPTranslation
	{
		public static readonly string FileExtension = "lngx";
		internal const string FileExtension1x = "lng";

		private KPTranslationProperties m_props = new KPTranslationProperties();
		public KPTranslationProperties Properties
		{
			get { return m_props; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");

				m_props = value;
			}
		}

		private List<KPStringTable> m_vStringTables = new List<KPStringTable>();

		[XmlArrayItem("StringTable")]
		public List<KPStringTable> StringTables
		{
			get { return m_vStringTables; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");

				m_vStringTables = value;
			}
		}

		private List<KPFormCustomization> m_vForms = new List<KPFormCustomization>();

		[XmlArrayItem("Form")]
		public List<KPFormCustomization> Forms
		{
			get { return m_vForms; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");

				m_vForms = value;
			}
		}

		private string m_strUnusedText = string.Empty;
		[DefaultValue("")]
		public string UnusedText
		{
			get { return m_strUnusedText; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");

				m_strUnusedText = value;
			}
		}

		public static void Save(KPTranslation kpTrl, string strFileName,
			IXmlSerializerEx xs)
		{
			using(FileStream fs = new FileStream(strFileName, FileMode.Create,
				FileAccess.Write, FileShare.None))
			{
				Save(kpTrl, fs, xs);
			}
		}

		public static void Save(KPTranslation kpTrl, Stream sOut,
			IXmlSerializerEx xs)
		{
			if(xs == null) throw new ArgumentNullException("xs");

#if !KeePassLibSD
			using(GZipStream gz = new GZipStream(sOut, CompressionMode.Compress))
#else
			using(GZipOutputStream gz = new GZipOutputStream(sOut))
#endif
			{
				using(XmlWriter xw = XmlUtilEx.CreateXmlWriter(gz))
				{
					xs.Serialize(xw, kpTrl);
				}
			}

			sOut.Close();
		}

		public static KPTranslation Load(string strFile, IXmlSerializerEx xs)
		{
			KPTranslation kpTrl = null;

			using(FileStream fs = new FileStream(strFile, FileMode.Open,
				FileAccess.Read, FileShare.Read))
			{
				kpTrl = Load(fs, xs);
			}

			return kpTrl;
		}

		public static KPTranslation Load(Stream s, IXmlSerializerEx xs)
		{
			if(xs == null) throw new ArgumentNullException("xs");

			KPTranslation kpTrl = null;

#if !KeePassLibSD
			using(GZipStream gz = new GZipStream(s, CompressionMode.Decompress))
#else
			using(GZipInputStream gz = new GZipInputStream(s))
#endif
			{
				kpTrl = (xs.Deserialize(gz) as KPTranslation);
			}

			s.Close();
			return kpTrl;
		}

		public Dictionary<string, string> SafeGetStringTableDictionary(
			string strTableName)
		{
			foreach(KPStringTable kpst in m_vStringTables)
			{
				if(kpst.Name == strTableName) return kpst.ToDictionary();
			}

			return new Dictionary<string, string>();
		}

#if (!KeePassLibSD && !KeePassUAP)
		public void ApplyTo(Form form)
		{
			if(form == null) throw new ArgumentNullException("form");

			if(m_props.RightToLeft)
			{
				try
				{
					form.RightToLeft = RightToLeft.Yes;
					form.RightToLeftLayout = true;
				}
				catch(Exception) { Debug.Assert(false); }
			}

			string strTypeName = form.GetType().FullName;
			foreach(KPFormCustomization kpfc in m_vForms)
			{
				if(kpfc.FullName == strTypeName)
				{
					kpfc.ApplyTo(form);
					break;
				}
			}

			if(m_props.RightToLeft)
			{
				try { RtlApplyToControls(form.Controls); }
				catch(Exception) { Debug.Assert(false); }
			}
		}

		private static void RtlApplyToControls(Control.ControlCollection cc)
		{
			foreach(Control c in cc)
			{
				if(c.Controls.Count > 0) RtlApplyToControls(c.Controls);

				if(c is DateTimePicker)
					((DateTimePicker)c).RightToLeftLayout = true;
				else if(c is ListView)
					((ListView)c).RightToLeftLayout = true;
				else if(c is MonthCalendar)
					((MonthCalendar)c).RightToLeftLayout = true;
				else if(c is ProgressBar)
					((ProgressBar)c).RightToLeftLayout = true;
				else if(c is TabControl)
					((TabControl)c).RightToLeftLayout = true;
				else if(c is TrackBar)
					((TrackBar)c).RightToLeftLayout = true;
				else if(c is TreeView)
					((TreeView)c).RightToLeftLayout = true;
				// else if(c is ToolStrip)
				//	RtlApplyToToolStripItems(((ToolStrip)c).Items);
				/* else if(c is Button) // Cf. Label
				{
					Button btn = (c as Button);
					Image img = btn.Image;
					if(img != null)
					{
						Image imgNew = (Image)img.Clone();
						imgNew.RotateFlip(RotateFlipType.RotateNoneFlipX);
						btn.Image = imgNew;
					}
				}
				else if(c is Label) // Cf. Button
				{
					Label lbl = (c as Label);
					Image img = lbl.Image;
					if(img != null)
					{
						Image imgNew = (Image)img.Clone();
						imgNew.RotateFlip(RotateFlipType.RotateNoneFlipX);
						lbl.Image = imgNew;
					}
				} */

				if(IsRtlMoveChildsRequired(c)) RtlMoveChildControls(c);
			}
		}

		internal static bool IsRtlMoveChildsRequired(Control c)
		{
			if(c == null) { Debug.Assert(false); return false; }

			return ((c is GroupBox) || (c is Panel));
		}

		private static void RtlMoveChildControls(Control cParent)
		{
			int nParentWidth = cParent.Size.Width;

			foreach(Control c in cParent.Controls)
			{
				DockStyle ds = c.Dock;
				if(ds == DockStyle.Left)
					c.Dock = DockStyle.Right;
				else if(ds == DockStyle.Right)
					c.Dock = DockStyle.Left;
				else
				{
					Point ptCur = c.Location;
					c.Location = new Point(nParentWidth - c.Size.Width - ptCur.X, ptCur.Y);
				}
			}
		}

		/* private static readonly string[] g_vRtlMirrorItemNames = new string[] { };
		private static void RtlApplyToToolStripItems(ToolStripItemCollection tsic)
		{
			foreach(ToolStripItem tsi in tsic)
			{
				if(tsi == null) { Debug.Assert(false); continue; }

				if(Array.IndexOf<string>(g_vRtlMirrorItemNames, tsi.Name) >= 0)
					tsi.RightToLeftAutoMirrorImage = true;

				ToolStripDropDownItem tsdd = (tsi as ToolStripDropDownItem);
				if(tsdd != null)
					RtlApplyToToolStripItems(tsdd.DropDownItems);
			}
		} */

		public void ApplyTo(string strTableName, ToolStripItemCollection tsic)
		{
			if(tsic == null) throw new ArgumentNullException("tsic");

			KPStringTable kpst = null;
			foreach(KPStringTable kpstEnum in m_vStringTables)
			{
				if(kpstEnum.Name == strTableName)
				{
					kpst = kpstEnum;
					break;
				}
			}

			if(kpst != null) kpst.ApplyTo(tsic);
		}
#endif

		internal bool IsFor(string strIso6391Code)
		{
			if(strIso6391Code == null) { Debug.Assert(false); return false; }

			return string.Equals(strIso6391Code, m_props.Iso6391Code,
				StrUtil.CaseIgnoreCmp);
		}

		internal string CombineToSentence(params string[] v)
		{
			if((v == null) || (v.Length == 0)) return string.Empty;

			string strSep = "; ", strEnd = ".";
			if(IsFor("zh-TW")) { strSep = "\uFF1B"; strEnd = "\u3002"; }

			StringBuilder sb = new StringBuilder();
			foreach(string str in v)
			{
				if(string.IsNullOrEmpty(str)) continue;

				if(sb.Length != 0) sb.Append(strSep);
				sb.Append(str);
			}

			if(sb.Length != 0) sb.Append(strEnd);
			return sb.ToString();
		}
	}
}
