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
using System.Xml.XPath;

using KeePass.App;

using KeePassLib.Utility;

namespace KeePass.Util
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
		private const string XPathLanguageID = "/Translation/Language/ID";
		private const string XPathLanguageEnglishName = "/Translation/Language/EnglishName";
		private const string XPathLanguageNativeName = "/Translation/Language/NativeName";
		private const string XPathVersionForApp = "/Translation/Version/ForApplicationVersion";
		private const string XPathAuthorName = "/Translation/Author/Name";
		private const string XPathAuthorContact = "/Translation/Author/Contact";

		public static List<AvTranslation> GetAvailableTranslations(string strBasePath)
		{
			DirectoryInfo diBase = new DirectoryInfo(strBasePath);
			DirectoryInfo[] vDI = diBase.GetDirectories();
			List<AvTranslation> lTranslations = new List<AvTranslation>();

			foreach(DirectoryInfo di in vDI)
			{
				string strFile = UrlUtil.EnsureTerminatingSeparator(di.FullName, false);
				strFile += AppDefs.LanguageInfoFileName;

				if(!File.Exists(strFile)) continue;

				AvTranslation trl = new AvTranslation();

				try
				{
					XPathDocument xpDoc = new XPathDocument(strFile);
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

				lTranslations.Add(trl);
			}

			return lTranslations;
		}
	}
}
