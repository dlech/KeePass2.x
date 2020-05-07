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
using System.Reflection;
using System.Text;
using System.Xml;

namespace KeePassLib.Native
{
	internal sealed class SimpleStat
	{
		private readonly object m_oPermissions;
		private readonly long m_nUserId;
		private readonly long m_nGroupId;

		private SimpleStat(object oPermissions, long nUserId, long nGroupId)
		{
			if(oPermissions == null) throw new ArgumentNullException("oPermissions");

			m_oPermissions = oPermissions;
			m_nUserId = nUserId;
			m_nGroupId = nGroupId;
		}

		public static SimpleStat Get(string strFilePath)
		{
			try
			{
				Type tUfsi, tUfi;
				object oUfi = GetUnixFileInfo(strFilePath, out tUfsi, out tUfi);
				if(oUfi == null) return null;

				PropertyInfo piPerm = tUfsi.GetProperty("FileAccessPermissions",
					BindingFlags.Public | BindingFlags.Instance);
				if(piPerm == null) { Debug.Assert(false); return null; }
				Debug.Assert(piPerm.PropertyType.IsEnum);

				PropertyInfo piUser = tUfsi.GetProperty("OwnerUserId",
					BindingFlags.Public | BindingFlags.Instance);
				if(piUser == null) { Debug.Assert(false); return null; }
				Debug.Assert(piUser.PropertyType == typeof(long));

				PropertyInfo piGroup = tUfsi.GetProperty("OwnerGroupId",
					BindingFlags.Public | BindingFlags.Instance);
				if(piGroup == null) { Debug.Assert(false); return null; }
				Debug.Assert(piGroup.PropertyType == typeof(long));

				return new SimpleStat(piPerm.GetValue(oUfi, null),
					(long)piUser.GetValue(oUfi, null),
					(long)piGroup.GetValue(oUfi, null));
			}
			catch(Exception) { Debug.Assert(false); }

			return null;
		}

		public static void Set(string strFilePath, SimpleStat s)
		{
			if(s == null) { Debug.Assert(false); return; }

			try
			{
				Type tUfsi, tUfi;
				object oUfi = GetUnixFileInfo(strFilePath, out tUfsi, out tUfi);
				if(oUfi == null) return;

				PropertyInfo piPerm = tUfsi.GetProperty("FileAccessPermissions",
					BindingFlags.Public | BindingFlags.Instance);
				if(piPerm == null) { Debug.Assert(false); return; }

				piPerm.SetValue(oUfi, s.m_oPermissions, null);

				MethodInfo miOwner = tUfsi.GetMethod("SetOwner",
					new Type[] { typeof(long), typeof(long) });
				if(miOwner == null) { Debug.Assert(false); return; }

				miOwner.Invoke(oUfi, new object[] { s.m_nUserId, s.m_nGroupId });
			}
			catch(Exception) { Debug.Assert(false); }
		}

		private static bool GetTypes(out Type tUfsi, out Type tUfi)
		{
			tUfsi = null;
			tUfi = null;

			try
			{
				if(!NativeLib.IsUnix()) return false;

				string strVer = typeof(XmlNode).Assembly.GetName().Version.ToString();
				string strPosix = "Mono.Posix, Version=" + strVer;
				Assembly asmPosix = Assembly.Load(strPosix);
				if(asmPosix == null) { Debug.Assert(false); return false; }

				tUfsi = asmPosix.GetType("Mono.Unix.UnixFileSystemInfo", false);
				tUfi = asmPosix.GetType("Mono.Unix.UnixFileInfo", false);

				bool b = ((tUfsi != null) && (tUfi != null));
				Debug.Assert(b);
				return b;
			}
			catch(Exception) { Debug.Assert(false); }

			return false;
		}

		private static object GetUnixFileInfo(string strFilePath, Type tUfi)
		{
			if(string.IsNullOrEmpty(strFilePath)) { Debug.Assert(false); return null; }

			try
			{
				if(!File.Exists(strFilePath)) { Debug.Assert(false); return null; }

				return Activator.CreateInstance(tUfi, strFilePath);
			}
			catch(Exception) { Debug.Assert(false); }

			return null;
		}

		private static object GetUnixFileInfo(string strFilePath,
			out Type tUfsi, out Type tUfi)
		{
			if(!GetTypes(out tUfsi, out tUfi)) return null;
			return GetUnixFileInfo(strFilePath, tUfi);
		}
	}
}
