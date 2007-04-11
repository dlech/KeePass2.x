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
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Diagnostics;
using System.Windows.Forms;

#if !KeePassLibSD
using System.IO.Compression;
#else
using ICSharpCode.SharpZipLib.GZip;
#endif

namespace KeePassLib.Utility
{
	public sealed class AvTranslation
	{
		public string LanguageID = string.Empty;
		public string LanguageEnglishName = string.Empty;
		public string LanguageNativeName = string.Empty;
		public string VersionForApp = string.Empty;
		public string AuthorName = string.Empty;
		public string AuthorContact = string.Empty;
	}

	public static class TranslationUtil
	{
		private const string XPathLanguageID = "/Translation/Meta/Language/ID";
		private const string XPathLanguageEnglishName = "/Translation/Meta/Language/EnglishName";
		private const string XPathLanguageNativeName = "/Translation/Meta/Language/NativeName";
		private const string XPathVersionForApp = "/Translation/Meta/Version/ForApplicationVersion";
		private const string XPathAuthorName = "/Translation/Meta/Author/Name";
		private const string XPathAuthorContact = "/Translation/Meta/Author/Contact";

		private const string ElemStringTable = "StringTable";
		private const string ElemStringData = "Data";
		private const string ElemStringValue = "Value";

		private const string AttrStringTableName = "Name";
		private const string AttrStringDataName = "Name";

		private const string MetaStringTableName = @"[Meta: Name]";

		private static List<Dictionary<string, string>> m_vDictionaries =
			new List<Dictionary<string, string>>();

		public static void Load(string strFile)
		{
			FileStream fs = new FileStream(strFile, FileMode.Open, FileAccess.Read,
				FileShare.Read);

#if !KeePassLibSD
			GZipStream gz = new GZipStream(fs, CompressionMode.Decompress);
#else
			GZipInputStream gz = new GZipInputStream(fs);
#endif

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(gz);

			foreach(XmlNode xmlChild in xmlDoc.DocumentElement.ChildNodes)
			{
				if(xmlChild.Name == ElemStringTable)
					LoadStringTable(xmlChild);
			}
		}

		private static void LoadStringTable(XmlNode xmlNode)
		{
			Dictionary<string, string> dict = new Dictionary<string, string>();

			dict.Add(MetaStringTableName, xmlNode.Attributes[AttrStringTableName].Value);

			foreach(XmlNode xmlChild in xmlNode.ChildNodes)
			{
				if(xmlChild.Name != ElemStringData)
				{
					Debug.Assert(false);
					continue;
				}

				dict.Add(xmlNode.Attributes[AttrStringDataName].Value,
					xmlNode.SelectSingleNode(ElemStringValue).InnerText);
			}

			m_vDictionaries.Add(dict);
		}

		/// <summary>
		/// Get and remove a loaded dictionary. Returns an empty dictionary
		/// if there is no matching dictionary.
		/// </summary>
		/// <param name="strName">Name of the dictionary.</param>
		/// <returns>Dictionary. Is never <c>null</c>.</returns>
		public static Dictionary<string, string> GetAndRemoveDictionary(string strName)
		{
			for(int i = 0; i < m_vDictionaries.Count; ++i)
			{
				if(m_vDictionaries[i][MetaStringTableName] == strName)
				{
					Dictionary<string, string> dict = m_vDictionaries[i];

					m_vDictionaries[i] = null;
					m_vDictionaries.RemoveAt(i);

					return dict;
				}
			}

			return new Dictionary<string, string>();
		}

#if !KeePassLibSD
		public static List<AvTranslation> GetAvailableTranslations(string strBasePath)
		{
			List<AvTranslation> lTranslations = new List<AvTranslation>();

			DirectoryInfo diBase = new DirectoryInfo(strBasePath);
			FileInfo[] vFiles = diBase.GetFiles(@"*.lngx",
				SearchOption.TopDirectoryOnly);

			foreach(FileInfo fi in vFiles)
			{
				string strFile = fi.FullName;

				if(!File.Exists(strFile)) continue;

				AvTranslation trl = new AvTranslation();

				FileStream fs = null;
				GZipStream gz = null;

				try
				{
					fs = new FileStream(strFile, FileMode.Open, FileAccess.Read,
						FileShare.Read);
					gz = new GZipStream(fs, CompressionMode.Decompress);

					XPathDocument xpDoc = new XPathDocument(gz);
					XPathNavigator xpDocNav = xpDoc.CreateNavigator();
					XPathNavigator xpNav;

					xpNav = xpDocNav.SelectSingleNode(XPathLanguageID);
					if(xpNav != null) trl.LanguageID = xpNav.Value;

					xpNav = xpDocNav.SelectSingleNode(XPathLanguageEnglishName);
					if(xpNav != null) trl.LanguageEnglishName = xpNav.Value;

					xpNav = xpDocNav.SelectSingleNode(XPathLanguageNativeName);
					if(xpNav != null) trl.LanguageNativeName = xpNav.Value;

					xpNav = xpDocNav.SelectSingleNode(XPathVersionForApp);
					if(xpNav != null) trl.VersionForApp = xpNav.Value;

					xpNav = xpDocNav.SelectSingleNode(XPathAuthorName);
					if(xpNav != null) trl.AuthorName = xpNav.Value;

					xpNav = xpDocNav.SelectSingleNode(XPathAuthorContact);
					if(xpNav != null) trl.AuthorContact = xpNav.Value;
				}
				catch(Exception) { }

				if(gz != null) gz.Close();
				if(fs != null) fs.Close();

				lTranslations.Add(trl);
			}

			return lTranslations;
		}
#endif

		private static void TranslateControl(Control c)
		{
			// c.Text = ...

			foreach(Control cSub in c.Controls)
				TranslateControl(cSub);
		}
	}
}
