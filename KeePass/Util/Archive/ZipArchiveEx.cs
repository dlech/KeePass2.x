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
using System.Reflection;
using System.Text;

using KeePassLib.Utility;

namespace KeePass.Util.Archive
{
	internal sealed class ZipArchiveEx : IDisposable
	{
		private static Type g_tZa = null;
		private static MethodInfo g_miZaGetEntry = null;
		private static MethodInfo g_miZaeOpen = null;

		private readonly object m_za;

		public ZipArchiveEx(Stream s)
		{
			if(s == null) { Debug.Assert(false); throw new ArgumentNullException("s"); }

			EnsureApi();

			m_za = Activator.CreateInstance(g_tZa, new object[] { s });
			if(m_za == null) throw new MissingMethodException();
		}

		private static void EnsureApi()
		{
			if(g_tZa != null) return;

			Type tZa = Type.GetType(
				"System.IO.Compression.ZipArchive, System.IO.Compression", false);
			Type tZae = Type.GetType(
				"System.IO.Compression.ZipArchiveEntry, System.IO.Compression", false);
			Debug.Assert((tZa == null) == (tZae == null));
			if((tZa == null) || (tZae == null))
			{
				string strAsm = typeof(System.Xml.XmlDocument).Assembly.FullName;
				strAsm = StrUtil.ReplaceCaseInsensitive(strAsm, "System.Xml",
					"System.IO.Compression");
				Assembly asm = Assembly.Load(strAsm);

				tZa = asm.GetType("System.IO.Compression.ZipArchive", true);
				tZae = asm.GetType("System.IO.Compression.ZipArchiveEntry", true);
			}

			g_miZaGetEntry = tZa.GetMethod("GetEntry");
			if(g_miZaGetEntry == null) throw new MissingMethodException();

			g_miZaeOpen = tZae.GetMethod("Open");
			if(g_miZaeOpen == null) throw new MissingMethodException();

			g_tZa = tZa; // Must be last, indicates success
		}

		public void Dispose()
		{
			((IDisposable)m_za).Dispose();
		}

		public Stream OpenEntry(string strName)
		{
			if(strName == null) { Debug.Assert(false); throw new ArgumentNullException("strName"); }

			object zae = g_miZaGetEntry.Invoke(m_za, new object[] { strName });
			if(zae == null) throw new FileNotFoundException();

			return (Stream)g_miZaeOpen.Invoke(zae, null);
		}
	}
}
