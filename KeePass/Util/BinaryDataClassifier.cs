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
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace KeePass.Util
{
	public enum BinaryDataClass
	{
		Unknown = 0,
		Text,
		Image,
		WebDocument
	}

	public static class BinaryDataClassifier
	{
		public const string BdeAnsi = "ANSI";
		public const string BdeAscii = "ASCII";
		public const string BdeUtf7 = "UTF-7";
		public const string BdeUtf8 = "UTF-8";
		public const string BdeUtf32 = "UTF-32";
		public const string BdeUnicodeLE = "Unicode (Little Endian)";
		public const string BdeUnicodeBE = "Unicode (Big Endian)";

		private static readonly string[] m_vTextExtensions = new string[]{
			"txt", "csv", "c", "cpp", "h", "hpp", "css", "js", "bat"
		};

		private static readonly string[] m_vImageExtensions = new string[]{
			"bmp", "emf", "exif", "gif", "ico", "jpeg", "jpe", "jpg",
			"png", "tiff", "tif", "wmf"
		};

		private static readonly string[] m_vWebExtensions = new string[]{
			"htm", "html", "mht", "xml", "xslt"
		};

		public static BinaryDataClass ClassifyUrl(string strUrl)
		{
			Debug.Assert(strUrl != null);
			if(strUrl == null) throw new ArgumentNullException("strUrl");

			string str = strUrl.ToLower();

			foreach(string strTextExt in m_vTextExtensions)
			{
				if(str.EndsWith("." + strTextExt))
					return BinaryDataClass.Text;
			}

			foreach(string strImageExt in m_vImageExtensions)
			{
				if(str.EndsWith("." + strImageExt))
					return BinaryDataClass.Image;
			}

			foreach(string strWebExt in m_vWebExtensions)
			{
				if(str.EndsWith("." + strWebExt))
					return BinaryDataClass.WebDocument;
			}

			return BinaryDataClass.Unknown;
		}

		public static BinaryDataClass ClassifyData(byte[] pbData)
		{
			Debug.Assert(pbData != null);
			if(pbData == null) throw new ArgumentNullException("pbData");

			MemoryStream ms = new MemoryStream(pbData, false);
			try
			{
				Image.FromStream(ms);

				ms.Close();
				return BinaryDataClass.Image;
			}
			catch(Exception) { ms.Close(); }

			return BinaryDataClass.Unknown;
		}

		public static Encoding GetStringEncoding(byte[] pbData, bool bBom,
			out string strOutEncodingName, out uint uStartOffset)
		{
			Debug.Assert(pbData != null);
			if(pbData == null) throw new ArgumentNullException("pbData");

			byte bt1 = ((pbData.Length >= 1) ? pbData[0] : (byte)0);
			byte bt2 = ((pbData.Length >= 2) ? pbData[1] : (byte)0);
			byte bt3 = ((pbData.Length >= 3) ? pbData[2] : (byte)0);
			byte bt4 = ((pbData.Length >= 4) ? pbData[3] : (byte)0);

			if((bt1 == 0xEF) && (bt2 == 0xBB) && (bt3 == 0xBF))
			{
				strOutEncodingName = BdeUtf8;
				uStartOffset = 3;
				return new UTF8Encoding(bBom);
			}
			if((bt1 == 0x00) && (bt2 == 0x00) && (bt3 == 0xFE) && (bt4 == 0xFF))
			{
				strOutEncodingName = BdeUtf32;
				uStartOffset = 4;
				return new UTF32Encoding(true, bBom);
			}
			if((bt1 == 0xFF) && (bt2 == 0xFE) && (bt3 == 0x00) && (bt4 == 0x00))
			{
				strOutEncodingName = BdeUtf32;
				uStartOffset = 4;
				return new UTF32Encoding(false, bBom);
			}
			if((bt1 == 0xFF) && (bt2 == 0xFE))
			{
				strOutEncodingName = BdeUnicodeLE;
				uStartOffset = 2;
				return new UnicodeEncoding(false, bBom);
			}
			if((bt1 == 0xFE) && (bt2 == 0xFF))
			{
				strOutEncodingName = BdeUnicodeBE;
				uStartOffset = 2;
				return new UnicodeEncoding(true, bBom);
			}

			uStartOffset = 0;

			try
			{
				Encoding.UTF8.GetString(pbData);

				strOutEncodingName = BdeUtf8;
				return new UTF8Encoding(bBom);
			}
			catch(Exception) { }

			strOutEncodingName = BdeAnsi;
			return Encoding.Default;
		}
	}
}
