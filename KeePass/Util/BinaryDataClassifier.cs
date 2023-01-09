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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;

using KeePass.Resources;

using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePass.Util
{
	public enum BinaryDataClass
	{
		Unknown = 0,
		Text,
		RichText,
		Image,
		WebDocument
	}

	public static class BinaryDataClassifier
	{
		private static readonly string[] g_vTextExts = new string[] {
			".asc", ".bat", ".c", ".cpp", ".css", ".csv", ".h", ".hpp",
			".js", ".ps1", ".tab", ".tsv", ".txt"
		};

		private static readonly string[] g_vRichTextExts = new string[] {
			".rtf"
		};

		private static readonly string[] g_vImageExts = new string[] {
			".bmp", ".emf", ".exif", ".gif", ".ico", ".jpe", ".jpeg",
			".jpg", ".png", ".tif", ".tiff", ".wmf"
		};

		private static readonly string[] g_vWebExts = new string[] {
			".htm", ".html"

			// The following types can be displayed by Internet Explorer,
			// but not by the WebBrowser control
			// ".mht", ".xml", ".xsl", ".xslt"
		};

		private static bool UrlHasExt(string strUrl, string[] vExts)
		{
			Debug.Assert(strUrl.Trim().ToLowerInvariant() == strUrl);

			foreach(string strExt in vExts)
			{
				Debug.Assert(strExt.StartsWith("."));
				Debug.Assert(strExt.Trim().ToLower() == strExt);

				if(strUrl.EndsWith(strExt, StringComparison.Ordinal))
					return true;
			}

			return false;
		}

		public static BinaryDataClass ClassifyUrl(string strUrl)
		{
			if(strUrl == null) { Debug.Assert(false); throw new ArgumentNullException("strUrl"); }

			string str = strUrl.Trim().ToLowerInvariant();

			if(UrlHasExt(str, g_vTextExts))
				return BinaryDataClass.Text;
			if(UrlHasExt(str, g_vRichTextExts))
				return BinaryDataClass.RichText;
			if(UrlHasExt(str, g_vImageExts))
				return BinaryDataClass.Image;
			if(UrlHasExt(str, g_vWebExts))
				return BinaryDataClass.WebDocument;

			return BinaryDataClass.Unknown;
		}

		public static BinaryDataClass ClassifyData(byte[] pbData)
		{
			if(pbData == null) { Debug.Assert(false); throw new ArgumentNullException("pbData"); }

			try
			{
				Image img = GfxUtil.LoadImage(pbData);
				if(img != null)
				{
					img.Dispose();
					return BinaryDataClass.Image;
				}
			}
			catch(Exception) { }

			return BinaryDataClass.Unknown;
		}

		// Cf. other overload
		public static BinaryDataClass Classify(string strUrl, byte[] pbData)
		{
			BinaryDataClass bdc = ClassifyUrl(strUrl);
			if(bdc != BinaryDataClass.Unknown) return bdc;

			return ClassifyData(pbData);
		}

		// Cf. other overload
		public static BinaryDataClass Classify(string strUrl, ProtectedBinary pb)
		{
			BinaryDataClass bdc = ClassifyUrl(strUrl);
			if(bdc != BinaryDataClass.Unknown) return bdc;

			if(pb == null) throw new ArgumentNullException("pb");
			byte[] pbData = pb.ReadData();
			try { bdc = ClassifyData(pbData); }
			finally { if(pb.IsProtected) MemUtil.ZeroByteArray(pbData); }

			return bdc;
		}

		public static StrEncodingInfo GetStringEncoding(byte[] pbData,
			out uint uStartOffset)
		{
			if(pbData == null) { Debug.Assert(false); throw new ArgumentNullException("pbData"); }

			uStartOffset = 0;

			List<StrEncodingInfo> lEncs = new List<StrEncodingInfo>(StrUtil.Encodings);
			lEncs.Sort(BinaryDataClassifier.CompareBySigLengthRev);

			foreach(StrEncodingInfo sei in lEncs)
			{
				byte[] pbSig = sei.StartSignature;
				if((pbSig == null) || (pbSig.Length == 0)) continue;
				if(pbSig.Length > pbData.Length) continue;

				byte[] pbStart = MemUtil.Mid<byte>(pbData, 0, pbSig.Length);
				if(MemUtil.ArraysEqual(pbStart, pbSig))
				{
					uStartOffset = (uint)pbSig.Length;
					return sei;
				}
			}

			if((pbData.Length % 4) == 0)
			{
				byte[] z3 = new byte[] { 0, 0, 0 };
				int i = MemUtil.IndexOf<byte>(pbData, z3);
				if((i >= 0) && (i < (pbData.Length - 4))) // Ignore last zero char
				{
					if((i % 4) == 0) return StrUtil.GetEncoding(StrEncodingType.Utf32BE);
					if((i % 4) == 1) return StrUtil.GetEncoding(StrEncodingType.Utf32LE);
					// Don't assume UTF-32 for other offsets
				}
			}

			if((pbData.Length % 2) == 0)
			{
				int i = Array.IndexOf<byte>(pbData, 0);
				if((i >= 0) && (i < (pbData.Length - 2))) // Ignore last zero char
				{
					if((i % 2) == 0) return StrUtil.GetEncoding(StrEncodingType.Utf16BE);
					return StrUtil.GetEncoding(StrEncodingType.Utf16LE);
				}
			}

			try
			{
				UTF8Encoding utf8Throw = new UTF8Encoding(false, true);
				utf8Throw.GetString(pbData);
				return StrUtil.GetEncoding(StrEncodingType.Utf8);
			}
			catch(Exception) { }

			return StrUtil.GetEncoding(StrEncodingType.Default);
		}

		private static int CompareBySigLengthRev(StrEncodingInfo a, StrEncodingInfo b)
		{
			Debug.Assert((a != null) && (b != null));

			int na = 0, nb = 0;
			if((a != null) && (a.StartSignature != null))
				na = a.StartSignature.Length;
			if((b != null) && (b.StartSignature != null))
				nb = b.StartSignature.Length;

			return -(na.CompareTo(nb));
		}
	}
}
