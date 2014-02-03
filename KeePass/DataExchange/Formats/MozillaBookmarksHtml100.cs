/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2014 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;

using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Cryptography;
using KeePassLib.Interfaces;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.DataExchange.Formats
{
	// 1.00
	internal sealed class MozillaBookmarksHtml100 : FileFormatProvider
	{
		public override bool SupportsImport { get { return true; } }
		public override bool SupportsExport { get { return false; } }

		public override string FormatName { get { return "Mozilla Bookmarks HTML"; } }
		public override string DefaultExtension { get { return @"html|htm"; } }
		public override string ApplicationGroup { get { return KPRes.Browser; } }

		// public override bool ImportAppendsToRootGroupOnly { get { return false; } }

		public override Image SmallIcon
		{
			get { return KeePass.Properties.Resources.B16x16_ASCII; }
		}

		public override void Import(PwDatabase pwStorage, Stream sInput,
			IStatusLogger slLogger)
		{
			StreamReader sr = new StreamReader(sInput, Encoding.UTF8);
			string strContent = sr.ReadToEnd();
			sr.Close();

			if(strContent.IndexOf(@"<!DOCTYPE NETSCAPE-Bookmark-file-1>") < 0)
				throw new FormatException("Invalid DOCTYPE!");

			strContent = strContent.Replace(@"<!DOCTYPE NETSCAPE-Bookmark-file-1>", string.Empty);
			strContent = strContent.Replace(@"<HR>", string.Empty);
			strContent = strContent.Replace(@"<p>", string.Empty);
			// strContent = strContent.Replace(@"<DD>", string.Empty);
			// strContent = strContent.Replace(@"<DL>", string.Empty);
			// strContent = strContent.Replace(@"</DL>", string.Empty);
			strContent = strContent.Replace(@"<DT>", string.Empty);

			// int nOffset = strContent.IndexOf('&');
			// while(nOffset >= 0)
			// {
			//	string str4 = strContent.Substring(nOffset, 4);
			//	string str5 = strContent.Substring(nOffset, 5);
			//	string str6 = strContent.Substring(nOffset, 6);
			//	if((str6 != @"&nbsp;") && (str5 != @"&amp;") && (str4 != @"&lt;") &&
			//		(str4 != @"&gt;") && (str5 != @"&#39;") && (str6 != @"&quot;"))
			//	{
			//		strContent = strContent.Remove(nOffset, 1);
			//		strContent = strContent.Insert(nOffset, @"&amp;");
			//	}
			//	else nOffset = strContent.IndexOf('&', nOffset + 1);
			// }

			string[] vPreserve = new string[] { @"&nbsp;", @"&amp;", @"&lt;",
				@"&gt;", @"&#39;", @"&quot;" };
			Dictionary<string, string> dPreserve = new Dictionary<string, string>();
			CryptoRandom cr = CryptoRandom.Instance;
			foreach(string strPreserve in vPreserve)
			{
				string strCode = Convert.ToBase64String(cr.GetRandomBytes(16));
				Debug.Assert(strCode.IndexOf('&') < 0);
				dPreserve[strPreserve] = strCode;

				strContent = strContent.Replace(strPreserve, strCode);
			}
			strContent = strContent.Replace(@"&", @"&amp;");
			foreach(KeyValuePair<string, string> kvpPreserve in dPreserve)
			{
				strContent = strContent.Replace(kvpPreserve.Value, kvpPreserve.Key);
			}

			// Terminate <DD>s
			int iDD = -1;
			while(true)
			{
				iDD = strContent.IndexOf(@"<DD>", iDD + 1);
				if(iDD < 0) break;

				int iNextTag = strContent.IndexOf('<', iDD + 1);
				if(iNextTag <= 0) { Debug.Assert(false); break; }

				strContent = strContent.Insert(iNextTag, @"</DD>");
			}

			strContent = "<RootSentinel>" + strContent + "</META></RootSentinel>";

			byte[] pbFixedData = StrUtil.Utf8.GetBytes(strContent);
			MemoryStream msFixed = new MemoryStream(pbFixedData, false);

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(msFixed);
			msFixed.Close();

			XmlNode xmlRoot = xmlDoc.DocumentElement;
			foreach(XmlNode xmlChild in xmlRoot)
			{
				if(xmlChild.Name == "META")
					ImportMeta(xmlChild, pwStorage);
			}
		}

		private static void ImportMeta(XmlNode xmlNode, PwDatabase pwStorage)
		{
			foreach(XmlNode xmlChild in xmlNode)
			{
				if(xmlChild.Name == "DL")
					ImportGroup(xmlChild, pwStorage, pwStorage.RootGroup);
				else if(xmlChild.Name == "TITLE") { }
				else if(xmlChild.Name == "H1") { }
				else { Debug.Assert(false); }
			}
		}

		private static void ImportGroup(XmlNode xmlNode, PwDatabase pwStorage,
			PwGroup pg)
		{
			PwGroup pgSub = pg;
			PwEntry pe = null;

			foreach(XmlNode xmlChild in xmlNode)
			{
				if(xmlChild.Name == "A")
				{
					pe = new PwEntry(true, true);
					pg.AddEntry(pe, true);

					pe.Strings.Set(PwDefs.TitleField, new ProtectedString(
						pwStorage.MemoryProtection.ProtectTitle,
						XmlUtil.SafeInnerText(xmlChild)));

					XmlNode xnUrl = xmlChild.Attributes.GetNamedItem("HREF");
					if((xnUrl != null) && (xnUrl.Value != null))
						pe.Strings.Set(PwDefs.UrlField, new ProtectedString(
							pwStorage.MemoryProtection.ProtectUrl, xnUrl.Value));
					else { Debug.Assert(false); }

					// pe.Strings.Set("RDF_ID", new ProtectedString(
					//	false, xmlChild.Attributes.GetNamedItem("ID").Value));

					ImportIcon(xmlChild, pe, pwStorage);
				}
				else if(xmlChild.Name == "DD")
				{
					if(pe != null)
						ImportUtil.AppendToField(pe, PwDefs.NotesField,
							XmlUtil.SafeInnerText(xmlChild).Trim(), pwStorage,
							"\r\n", false);
					else { Debug.Assert(false); }
				}
				else if(xmlChild.Name == "H3")
				{
					string strGroup = XmlUtil.SafeInnerText(xmlChild);
					if(strGroup.Length == 0) { Debug.Assert(false); pgSub = pg; }
					else
					{
						pgSub = new PwGroup(true, true, strGroup, PwIcon.Folder);
						pg.AddGroup(pgSub, true);
					}
				}
				else if(xmlChild.Name == "DL")
					ImportGroup(xmlChild, pwStorage, pgSub);
				else { Debug.Assert(false); }
			}
		}

		private static void ImportIcon(XmlNode xn, PwEntry pe, PwDatabase pd)
		{
			XmlNode xnIcon = xn.Attributes.GetNamedItem("ICON");
			if(xnIcon == null) return;

			string strIcon = xnIcon.Value;
			if(!StrUtil.IsDataUri(strIcon)) { Debug.Assert(false); return; }

			try
			{
				byte[] pbImage = StrUtil.DataUriToData(strIcon);
				if((pbImage == null) || (pbImage.Length == 0)) { Debug.Assert(false); return; }

				Image img = GfxUtil.LoadImage(pbImage);
				if(img == null) { Debug.Assert(false); return; }

				byte[] pbPng;
				if((img.Width == 16) && (img.Height == 16))
				{
					MemoryStream msPng = new MemoryStream();
					img.Save(msPng, ImageFormat.Png);
					pbPng = msPng.ToArray();
					msPng.Close();
				}
				else
				{
					Bitmap bmp = UIUtil.CreateScaledImage(img, 16, 16);
					MemoryStream msPng = new MemoryStream();
					bmp.Save(msPng, ImageFormat.Png);
					pbPng = msPng.ToArray();
					msPng.Close();

					bmp.Dispose();
				}
				img.Dispose();

				PwUuid pwUuid = null;
				int iEx = pd.GetCustomIconIndex(pbPng);
				if(iEx >= 0) pwUuid = pd.CustomIcons[iEx].Uuid;
				else
				{
					pwUuid = new PwUuid(true);
					pd.CustomIcons.Add(new PwCustomIcon(pwUuid, pbPng));
					pd.UINeedsIconUpdate = true;
					pd.Modified = true;
				}
				pe.CustomIconUuid = pwUuid;
			}
			catch(Exception) { Debug.Assert(false); }
		}
	}
}
