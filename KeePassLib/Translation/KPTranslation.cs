/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2009 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Diagnostics;

#if !KeePassLibSD
using System.IO.Compression;
#else
using ICSharpCode.SharpZipLib.GZip;
#endif

namespace KeePassLib.Translation
{
	[XmlRoot("Translation")]
	public sealed class KPTranslation
	{
		public const string FileExtension = "lngx";

		private KPTranslationProperties m_props = new KPTranslationProperties();
		public KPTranslationProperties Properties
		{
			get { return m_props; }
			set { m_props = value; }
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
		public string UnusedText
		{
			get { return m_strUnusedText; }
			set
			{
				if(value == null) throw new ArgumentNullException("value");

				m_strUnusedText = value;
			}
		}

		public static void SaveToFile(KPTranslation kpTrl, string strFileName)
		{
			FileStream fs = new FileStream(strFileName, FileMode.Create,
				FileAccess.Write, FileShare.None);

#if !KeePassLibSD
			GZipStream gz = new GZipStream(fs, CompressionMode.Compress);
#else
			GZipOutputStream gz = new GZipOutputStream(fs);
#endif

			XmlWriterSettings xws = new XmlWriterSettings();
			xws.CheckCharacters = true;
			xws.Encoding = Encoding.UTF8;
			xws.Indent = true;
			xws.IndentChars = "\t";

			XmlWriter xw = XmlWriter.Create(gz, xws);

			XmlSerializer xmlSerial = new XmlSerializer(typeof(KPTranslation));
			xmlSerial.Serialize(xw, kpTrl);

			xw.Close();
			gz.Close();
			fs.Close();
		}

		public static KPTranslation LoadFromFile(string strFile)
		{
			FileStream fs = new FileStream(strFile, FileMode.Open,
				FileAccess.Read, FileShare.Read);

#if !KeePassLibSD
			GZipStream gz = new GZipStream(fs, CompressionMode.Decompress);
#else
			GZipInputStream gz = new GZipInputStream(fs);
#endif

			XmlSerializer xmlSerial = new XmlSerializer(typeof(KPTranslation));
			KPTranslation kpTrl = xmlSerial.Deserialize(gz) as KPTranslation;

			gz.Close();
			fs.Close();
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

#if !KeePassLibSD
		public void ApplyTo(Form form)
		{
			if(form == null) throw new ArgumentNullException("form");

			string strTypeName = form.GetType().FullName;
			foreach(KPFormCustomization kpfc in m_vForms)
			{
				if(kpfc.FullName == strTypeName)
				{
					kpfc.ApplyTo(form);
					break;
				}
			}
		}

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
	}
}
