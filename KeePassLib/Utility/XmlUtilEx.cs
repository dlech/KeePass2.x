/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2020 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Xml;
using System.Xml.Serialization;

namespace KeePassLib.Utility
{
	public static class XmlUtilEx
	{
		public static XmlDocument CreateXmlDocument()
		{
			XmlDocument d = new XmlDocument();

			// .NET 4.5.2 and newer do not resolve external XML resources
			// by default; for older .NET versions, we explicitly
			// prevent resolving
			d.XmlResolver = null; // Default in old .NET: XmlUrlResolver object

			return d;
		}

		public static XmlReaderSettings CreateXmlReaderSettings()
		{
			XmlReaderSettings xrs = new XmlReaderSettings();

			xrs.CloseInput = false;
			xrs.IgnoreComments = true;
			xrs.IgnoreProcessingInstructions = true;
			xrs.IgnoreWhitespace = true;

#if KeePassUAP
			xrs.DtdProcessing = DtdProcessing.Prohibit;
#else
			// Also see PrepMonoDev.sh script
			xrs.ProhibitDtd = true; // Obsolete in .NET 4, but still there
			// xrs.DtdProcessing = DtdProcessing.Prohibit; // .NET 4 only
#endif

			xrs.ValidationType = ValidationType.None;
			xrs.XmlResolver = null;

			return xrs;
		}

		public static XmlReader CreateXmlReader(Stream s)
		{
			if(s == null) { Debug.Assert(false); throw new ArgumentNullException("s"); }

			return XmlReader.Create(s, CreateXmlReaderSettings());
		}

		public static XmlWriterSettings CreateXmlWriterSettings()
		{
			XmlWriterSettings xws = new XmlWriterSettings();

			xws.CloseOutput = false;
			xws.Encoding = StrUtil.Utf8;
			xws.Indent = true;
			xws.IndentChars = "\t";
			xws.NewLineOnAttributes = false;

			return xws;
		}

		public static XmlWriter CreateXmlWriter(Stream s)
		{
			if(s == null) { Debug.Assert(false); throw new ArgumentNullException("s"); }

			return XmlWriter.Create(s, CreateXmlWriterSettings());
		}

		public static void Serialize<T>(Stream s, T t)
		{
			if(s == null) { Debug.Assert(false); throw new ArgumentNullException("s"); }

			XmlSerializer xs = new XmlSerializer(typeof(T));
			using(XmlWriter xw = CreateXmlWriter(s))
			{
				xs.Serialize(xw, t);
			}
		}

		public static T Deserialize<T>(Stream s)
		{
			if(s == null) { Debug.Assert(false); throw new ArgumentNullException("s"); }

			XmlSerializer xs = new XmlSerializer(typeof(T));

			T t = default(T);
			using(XmlReader xr = CreateXmlReader(s))
			{
				t = (T)xs.Deserialize(xr);
			}

			return t;
		}

#if DEBUG
		internal static void ValidateXml(string strXml, bool bReplaceStdEntities)
		{
			if(strXml == null) throw new ArgumentNullException("strXml");
			if(strXml.Length == 0) { Debug.Assert(false); return; }

			string str = strXml;

			if(bReplaceStdEntities)
				str = str.Replace("&nbsp;", "&#160;");

			XmlDocument d = new XmlDocument();
			d.LoadXml(str);
		}
#endif
	}
}
