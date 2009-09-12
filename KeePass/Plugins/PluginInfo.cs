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
using System.Diagnostics;

using KeePass.App;

using KeePassLib;
using KeePassLib.Utility;

namespace KeePass.Plugins
{
	internal sealed class PluginInfo
	{
		private string m_strFilePath;
		private string m_strDisplayFilePath;
		private Plugin m_pluginInterface = null;

		private string m_strFileVersion = string.Empty;

		private string m_strName = string.Empty;
		private string m_strDescription = string.Empty;
		private string m_strAuthor = string.Empty;

		public string FilePath
		{
			get { return m_strFilePath; }
		}

		public string DisplayFilePath
		{
			get { return (m_strDisplayFilePath ?? m_strFilePath); }
		}

		public Plugin Interface
		{
			get { return m_pluginInterface; }
			set { m_pluginInterface = value; }
		}

		public string FileVersion
		{
			get { return m_strFileVersion; }
		}

		public string Name
		{
			get { return m_strName; }
		}

		public string Description
		{
			get { return m_strDescription; }
		}

		public string Author
		{
			get { return m_strAuthor; }
		}

		public PluginInfo(string strFilePath, FileVersionInfo fvi,
			string strDisplayFilePath)
		{
			Debug.Assert(strFilePath != null);
			if(strFilePath == null) throw new ArgumentNullException("strFilePath");
			Debug.Assert(fvi != null);
			if(fvi == null) throw new ArgumentNullException("fvi");

			m_strFilePath = strFilePath;
			m_strDisplayFilePath = strDisplayFilePath;

			if(fvi.FileVersion != null) m_strFileVersion = fvi.FileVersion;

			if(fvi.FileDescription != null) m_strName = fvi.FileDescription;
			if(fvi.Comments != null) m_strDescription = fvi.Comments;
			if(fvi.CompanyName != null) m_strAuthor = fvi.CompanyName;
		}
	}
}
