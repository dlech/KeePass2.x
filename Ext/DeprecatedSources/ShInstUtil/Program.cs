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
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Diagnostics;

using Microsoft.Win32;

namespace ShInstUtil
{
	public static class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			if((args == null) || (args.Length == 0)) return;

			string strCmd = args[0];
			if(string.IsNullOrEmpty(strCmd)) return;
			strCmd = strCmd.ToLower();
			strCmd = strCmd.Trim(new char[]{ '\r', '\n', ' ', '\t', '-', '/',
				'\"', '\'' });

			if(strCmd == "ngen_install")
			{
				UpdateNativeImage(false);
				UpdateNativeImage(true);
			}
			else if(strCmd == "ngen_uninstall")
				UpdateNativeImage(false);
		}

		private static string FindNGenPath()
		{
			RegistryKey kSoftware = Registry.LocalMachine.OpenSubKey("SOFTWARE");
			RegistryKey kMicrosoft = kSoftware.OpenSubKey("Microsoft");
			RegistryKey kNet = kMicrosoft.OpenSubKey(".NETFramework");

			string strPath = (kNet.GetValue("InstallRoot") as string);

			kNet.Close();
			kMicrosoft.Close();
			kSoftware.Close();

			if(string.IsNullOrEmpty(strPath)) return null;

			DirectoryInfo di = new DirectoryInfo(strPath);
			FileInfo[] vNGens = di.GetFiles("ngen.exe", SearchOption.AllDirectories);
			if((vNGens == null) || (vNGens.Length == 0)) return null;

			strPath = null;
			DateTime dt = DateTime.MinValue.AddYears(1);
			foreach(FileInfo fi in vNGens)
			{
				if(fi.CreationTime >= dt)
				{
					strPath = fi.FullName;
					dt = fi.CreationTime;
				}
			}

			return strPath;
		}

		private static void UpdateNativeImage(bool bInstall)
		{
			try
			{
				string strNGen = FindNGenPath();
				if(string.IsNullOrEmpty(strNGen)) return;

				string strMe = Assembly.GetExecutingAssembly().Location;
				string strBasePath = Path.GetDirectoryName(strMe).Trim(new char[]{
					'\r', '\n', ' ', '\t', '\"', '\'' });
				string strToReg = strBasePath + Path.DirectorySeparatorChar + "KeePass.exe";

				string strPreCmd = (bInstall ? string.Empty : "un");

				ProcessStartInfo psi = new ProcessStartInfo();
				psi.Arguments = strPreCmd + "install \"" + strToReg + "\"";
				psi.CreateNoWindow = true;
				psi.FileName = strNGen;
				psi.WindowStyle = ProcessWindowStyle.Hidden;

				Process p = Process.Start(psi);
				p.WaitForExit(16000);
			}
			catch(Exception) { }
		}
	}
}
