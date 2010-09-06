/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2010 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.IO;
using System.Xml.Serialization;
using System.Threading;

using KeePass.Forms;
using KeePass.Native;

using KeePassLib;
using KeePassLib.Cryptography;
using KeePassLib.Serialization;
using KeePassLib.Utility;

namespace KeePass.Util
{
	public sealed class IpcParamEx
	{
		public string Message { get; set; }

		public string Param0 { get; set; }
		public string Param1 { get; set; }
		public string Param2 { get; set; }
		public string Param3 { get; set; }
		public string Param4 { get; set; }

		public IpcParamEx()
		{
			this.Message = string.Empty;
			this.Param0 = string.Empty;
			this.Param1 = string.Empty;
			this.Param2 = string.Empty;
			this.Param3 = string.Empty;
			this.Param4 = string.Empty;
		}
		
		public IpcParamEx(string strMessage, string strParam0, string strParam1,
			string strParam2, string strParam3, string strParam4)
		{
			this.Message = (strMessage ?? string.Empty);
			this.Param0 = (strParam0 ?? string.Empty);
			this.Param1 = (strParam1 ?? string.Empty);
			this.Param2 = (strParam2 ?? string.Empty);
			this.Param3 = (strParam3 ?? string.Empty);
			this.Param4 = (strParam4 ?? string.Empty);
		}
	}

	public static class IpcUtilEx
	{
		public const string CmdOpenDatabase = "OpenDatabase";
		public const string CmdOpenEntryUrl = "OpenEntryUrl";

		public static void SendGlobalMessage(IpcParamEx ipcMsg)
		{
			if(ipcMsg == null) throw new ArgumentNullException("ipcMsg");

			int nId = (int)(MemUtil.BytesToUInt32(CryptoRandom.Instance.GetRandomBytes(
				4)) & 0x7FFFFFFF);

			if(WriteIpcInfoFile(nId, ipcMsg) == false) return;

			try
			{
				// NativeMethods.SendMessage((IntPtr)NativeMethods.HWND_BROADCAST,
				//	Program.ApplicationMessage, (IntPtr)Program.AppMessage.IpcByFile,
				//	(IntPtr)nId);

				// IntPtr pResult = new IntPtr(0);
				// NativeMethods.SendMessageTimeout((IntPtr)NativeMethods.HWND_BROADCAST,
				//	Program.ApplicationMessage, (IntPtr)Program.AppMessage.IpcByFile,
				//	(IntPtr)nId, NativeMethods.SMTO_ABORTIFHUNG, 5000, ref pResult);

				IpcBroadcast.Send(Program.AppMessage.IpcByFile, nId, true);
			}
			catch(Exception) { Debug.Assert(false); }

			RemoveIpcInfoFile(nId);
		}

		private static string GetIpcFilePath(int nId)
		{
			try
			{
				string str = Path.GetTempPath();
				str = UrlUtil.EnsureTerminatingSeparator(str, false);
				
				return (str + "KeePassIpc" + nId.ToString() + ".tmp");
			}
			catch(Exception) { Debug.Assert(false); }

			return null;
		}

		private static bool WriteIpcInfoFile(int nId, IpcParamEx ipcMsg)
		{
			string strPath = GetIpcFilePath(nId);
			if(string.IsNullOrEmpty(strPath)) return false;

			try
			{
				XmlSerializer xml = new XmlSerializer(typeof(IpcParamEx));
				FileStream fs = new FileStream(strPath, FileMode.Create, FileAccess.Write,
					FileShare.None);

				try { xml.Serialize(fs, ipcMsg); }
				catch(Exception) { Debug.Assert(false); }

				fs.Close();
				return true;
			}
			catch(Exception) { Debug.Assert(false); }

			return false;
		}

		private static void RemoveIpcInfoFile(int nId)
		{
			string strPath = GetIpcFilePath(nId);
			if(string.IsNullOrEmpty(strPath)) return;

			try { if(File.Exists(strPath)) File.Delete(strPath); }
			catch(Exception) { Debug.Assert(false); }
		}

		private static IpcParamEx LoadIpcInfoFile(int nId)
		{
			string strPath = GetIpcFilePath(nId);
			if(string.IsNullOrEmpty(strPath)) return null;

			string strMtxName = ("KeePassIpc" + nId.ToString());
			Mutex m = Program.TrySingleInstanceLock(strMtxName, true);
			if(m == null) return null;

			IpcParamEx ipcParam = null;
			try
			{
				XmlSerializer xml = new XmlSerializer(typeof(IpcParamEx));
				FileStream fs = new FileStream(strPath, FileMode.Open, FileAccess.Read,
					FileShare.Read);

				try { ipcParam = (IpcParamEx)xml.Deserialize(fs); }
				catch(Exception) { Debug.Assert(false); }

				fs.Close();
			}
			catch(Exception) { }

			RemoveIpcInfoFile(nId);

			Program.DestroyMutex(m, true);
			return ipcParam;
		}

		public static void ProcessGlobalMessage(int nId, MainForm mf)
		{
			if(mf == null) throw new ArgumentNullException("mf");

			IpcParamEx ipcMsg = LoadIpcInfoFile(nId);
			if(ipcMsg == null) return;

			if(ipcMsg.Message == CmdOpenDatabase)
			{
				mf.UIBlockAutoUnlock(true);
				mf.EnsureVisibleForegroundWindow(true, true);
				mf.UIBlockAutoUnlock(false);

				string[] vArgs = CommandLineArgs.SafeDeserialize(ipcMsg.Param0);
				if(vArgs == null) { Debug.Assert(false); return; }

				CommandLineArgs args = new CommandLineArgs(vArgs);
				Program.CommandLineArgs.CopyFrom(args);

				mf.OpenDatabase(IOConnectionInfo.FromPath(args.FileName),
					KeyUtil.KeyFromCommandLine(args), true);
			}
			else if(ipcMsg.Message == CmdOpenEntryUrl) OpenEntryUrl(ipcMsg, mf);
			else { Debug.Assert(false); }
		}

		private static void OpenEntryUrl(IpcParamEx ip, MainForm mf)
		{
			string strUuid = ip.Param0;
			if(string.IsNullOrEmpty(strUuid)) return; // No assert (user data)

			byte[] pbUuid = MemUtil.HexStringToByteArray(strUuid);
			if((pbUuid == null) || (pbUuid.Length != PwUuid.UuidSize)) return;

			PwDatabase pdb = mf.ActiveDatabase;
			if((pdb == null) || !pdb.IsOpen) return;

			PwEntry pe = pdb.RootGroup.FindEntry(new PwUuid(pbUuid), true);
			if(pe == null) return;

			mf.PerformDefaultUrlAction(new PwEntry[]{ pe }, true);
		}
	}
}
