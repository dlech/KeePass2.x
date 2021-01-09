/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2021 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Security.Cryptography;
using System.Text;
using System.Threading;

using KeePass.Forms;
using KeePass.UI;

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

	public sealed class IpcEventArgs : EventArgs
	{
		private readonly string m_strName;
		public string Name { get { return m_strName; } }

		private readonly CommandLineArgs m_args;
		public CommandLineArgs Args { get { return m_args; } }

		public IpcEventArgs(string strName, CommandLineArgs clArgs)
		{
			if(strName == null) throw new ArgumentNullException("strName");

			m_strName = strName;
			m_args = clArgs;
		}
	}

	public static class IpcUtilEx
	{
		internal const string IpcMsgFilePreID = "KeePassIPC-";
		// internal const string IpcMsgFilePostID = "-Msgs.tmp";

		public static readonly string CmdOpenDatabase = "OpenDatabase";
		public static readonly string CmdOpenEntryUrl = "OpenEntryUrl";
		public static readonly string CmdIpcEvent = "IpcEvent";

		private static readonly byte[] IpcOptEnt = new byte[] {
			0x51, 0xE4, 0xCA, 0x4B, 0xBB, 0x63, 0x57, 0x21
		};

		/// <summary>
		/// Event that is raised e.g. when running KeePass with the
		/// <c>AppDefs.CommandLineOptions.IpcEvent</c> command line parameter.
		/// </summary>
		/// <!-- https://sourceforge.net/p/keepass/feature-requests/1870/ -->
		public static event EventHandler<IpcEventArgs> IpcEvent;

		public static void SendGlobalMessage(IpcParamEx ipcMsg)
		{
			SendGlobalMessage(ipcMsg, true); // Backward compatibility
		}

		public static void SendGlobalMessage(IpcParamEx ipcMsg, bool bOneInstance)
		{
			if(ipcMsg == null) throw new ArgumentNullException("ipcMsg");

			int nId = (int)(MemUtil.BytesToUInt32(CryptoRandom.Instance.GetRandomBytes(
				4)) & 0x7FFFFFFF);

			if(!WriteIpcInfoFile(nId, ipcMsg)) return;

			try
			{
				// NativeMethods.SendMessage((IntPtr)NativeMethods.HWND_BROADCAST,
				//	Program.ApplicationMessage, (IntPtr)Program.AppMessage.IpcByFile,
				//	(IntPtr)nId);

				// IntPtr pResult = new IntPtr(0);
				// NativeMethods.SendMessageTimeout((IntPtr)NativeMethods.HWND_BROADCAST,
				//	Program.ApplicationMessage, (IntPtr)Program.AppMessage.IpcByFile,
				//	(IntPtr)nId, NativeMethods.SMTO_ABORTIFHUNG, 5000, ref pResult);

				IpcBroadcast.Send(bOneInstance ? Program.AppMessage.IpcByFile1 :
					Program.AppMessage.IpcByFile, nId, true);
			}
			catch(Exception) { Debug.Assert(false); }

			if(bOneInstance)
			{
				string strIpcFile = GetIpcFilePath(nId);
				for(int r = 0; r < 50; ++r)
				{
					try { if(!File.Exists(strIpcFile)) break; }
					catch(Exception) { Debug.Assert(false); }
					Thread.Sleep(20);
				}
			}
			else Thread.Sleep(1000);

			RemoveIpcInfoFile(nId);
		}

		private static string GetIpcFilePath(int nId)
		{
			try
			{
				string str = UrlUtil.GetTempPath();
				str = UrlUtil.EnsureTerminatingSeparator(str, false);
				
				return (str + IpcMsgFilePreID + nId.ToString() + ".tmp");
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
				using(MemoryStream ms = new MemoryStream())
				{
					XmlUtilEx.Serialize<IpcParamEx>(ms, ipcMsg);

					byte[] pb = ms.ToArray();
					byte[] pbCmp = MemUtil.Compress(pb);
					byte[] pbEnc = CryptoUtil.ProtectData(pbCmp, IpcOptEnt,
						DataProtectionScope.CurrentUser);

					File.WriteAllBytes(strPath, pbEnc);
				}

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

		private static IpcParamEx LoadIpcInfoFile(int nId, bool bOneInstance)
		{
			string strPath = GetIpcFilePath(nId);
			if(string.IsNullOrEmpty(strPath)) return null;

			string strMtxName = null;
			if(bOneInstance)
			{
				strMtxName = IpcMsgFilePreID + nId.ToString();
				if(!GlobalMutexPool.CreateMutex(strMtxName, true)) return null;
			}

			IpcParamEx ipcParam = null;
			try
			{
				byte[] pbEnc = File.ReadAllBytes(strPath);
				byte[] pbCmp = CryptoUtil.UnprotectData(pbEnc, IpcOptEnt,
					DataProtectionScope.CurrentUser);
				byte[] pb = MemUtil.Decompress(pbCmp);

				using(MemoryStream ms = new MemoryStream(pb, false))
				{
					ipcParam = XmlUtilEx.Deserialize<IpcParamEx>(ms);
				}
			}
			catch(Exception) { Debug.Assert(!File.Exists(strPath)); }

			if(bOneInstance)
			{
				RemoveIpcInfoFile(nId);
				if(!GlobalMutexPool.ReleaseMutex(strMtxName)) { Debug.Assert(false); }
			}

			return ipcParam;
		}

		public static void ProcessGlobalMessage(int nId, MainForm mf)
		{
			ProcessGlobalMessage(nId, mf, true); // Backward compatibility
		}

		public static void ProcessGlobalMessage(int nId, MainForm mf, bool bOneInstance)
		{
			if(mf == null) throw new ArgumentNullException("mf");

			IpcParamEx ipcMsg = LoadIpcInfoFile(nId, bOneInstance);
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

				mf.OpenDatabase(mf.IocFromCommandLine(), KeyUtil.KeyFromCommandLine(
					Program.CommandLineArgs), true);
			}
			else if(ipcMsg.Message == CmdOpenEntryUrl) OpenEntryUrl(ipcMsg, mf);
			else if(ipcMsg.Message == CmdIpcEvent)
			{
				try
				{
					if(IpcUtilEx.IpcEvent == null) return;

					string strName = ipcMsg.Param0;
					if(string.IsNullOrEmpty(strName)) { Debug.Assert(false); return; }

					string[] vArgs = CommandLineArgs.SafeDeserialize(ipcMsg.Param1);
					if(vArgs == null) { Debug.Assert(false); return; }

					CommandLineArgs clArgs = new CommandLineArgs(vArgs);

					IpcEventArgs e = new IpcEventArgs(strName, clArgs);
					IpcUtilEx.IpcEvent(null, e);
				}
				catch(Exception) { Debug.Assert(false); }
			}
			else { Debug.Assert(false); }
		}

		private static void OpenEntryUrl(IpcParamEx ip, MainForm mf)
		{
			string strUuid = ip.Param0;
			if(string.IsNullOrEmpty(strUuid)) return; // No assert (user data)

			byte[] pbUuid = MemUtil.HexStringToByteArray(strUuid);
			if((pbUuid == null) || (pbUuid.Length != PwUuid.UuidSize)) return;
			PwUuid pwUuid = new PwUuid(pbUuid);

			List<PwDocument> lDocs = mf.DocumentManager.GetDocuments(int.MinValue);
			foreach(PwDocument pwDoc in lDocs)
			{
				if(pwDoc == null) { Debug.Assert(false); continue; }

				PwDatabase pdb = pwDoc.Database;
				if((pdb == null) || !pdb.IsOpen) continue;

				PwEntry pe = pdb.RootGroup.FindEntry(pwUuid, true);
				if(pe == null) continue;

				mf.PerformDefaultUrlAction(new PwEntry[] { pe }, true);
				break;
			}
		}
	}
}
