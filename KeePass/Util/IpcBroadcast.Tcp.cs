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
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

using KeePass.App;
using KeePass.Forms;

using KeePassLib;
using KeePassLib.Cryptography;
using KeePassLib.Delegates;
using KeePassLib.Utility;

namespace KeePass.Util
{
	public static partial class IpcBroadcast
	{
		private static volatile TcpListener g_tl = null;
		private static volatile string g_strLockFile = null;

		private static List<KeyValuePair<PwUuid, DateTime>> g_lOldMsgs =
			new List<KeyValuePair<PwUuid, DateTime>>();

		private const string IpcTcpSuffix = ".act";
		private const int IpcTcpPortMin = 49152; // https://tools.ietf.org/html/rfc6335
		private const int IpcTcpPortMaxExcl = 65535;
		private const double IpcTcpTtlMs = 8000;
		private const int IpcTcpMsgSizeMax = 256 * 1024; // Normal: ~ 270 bytes
		private static readonly byte[] IpcTcpOptEnt = new byte[] {
			0x44, 0x91, 0x62, 0xF2, 0x30, 0xC0, 0xD7, 0x49
		};

		private static string TcpGetPrefix()
		{
			// BC = broadcast, P = port
			return (IpcUtilEx.IpcMsgFilePreID + GetUserID() + "-BC-P-");
		}

		private static void TcpSend(Program.AppMessage msg, int lParam)
		{
			PwUuid puID = new PwUuid(true); // One message ID for all processes

			string strPrefix = TcpGetPrefix();
			List<string> lLocks = UrlUtil.GetFilePaths(UrlUtil.GetTempPath(),
				strPrefix + "*" + IpcTcpSuffix, SearchOption.TopDirectoryOnly);

			foreach(string strLock in lLocks)
			{
				try
				{
					string strName = UrlUtil.GetFileName(strLock);
					ushort uPort = ushort.Parse(strName.Substring(strPrefix.Length,
						strName.Length - strPrefix.Length - IpcTcpSuffix.Length),
						NumberFormatInfo.InvariantInfo);
					if((uPort < (uint)IpcTcpPortMin) || (uPort >= (uint)IpcTcpPortMaxExcl))
					{
						Debug.Assert(false);
						continue;
					}

					using(TcpClient tc = new TcpClient())
					{
						tc.Connect(IPAddress.Loopback, (int)uPort);
						using(NetworkStream s = tc.GetStream())
						{
							byte[] pb = TcpCreateMessage(puID, msg, lParam); // Timestamped
							s.Write(pb, 0, pb.Length);
						}
					}
				}
				catch(Exception) { Debug.Assert(false); }
			}
		}

		private static void TcpStartServer()
		{
			Thread th = new Thread(TcpThreadProc);
			th.Start();
		}

		private static void TcpStopServer()
		{
			if(g_tl == null) return;

			try { if(g_strLockFile != null) File.Delete(g_strLockFile); }
			catch(Exception) { Debug.Assert(false); }
			g_strLockFile = null;

			try { g_tl.Stop(); } // Throws an exception in the thread
			catch(Exception) { Debug.Assert(false); }
			g_tl = null;
		}

		private static void TcpThreadProc()
		{
			try { TcpThreadProcPriv(); }
			// catch(ThreadAbortException)
			// {
			//	try { Thread.ResetAbort(); }
			//	catch(Exception) { Debug.Assert(false); }
			// }
			catch(Exception ex)
			{
				Debug.Assert(false);
				if(Program.CommandLineArgs[AppDefs.CommandLineOptions.Debug] != null)
				{
					Console.WriteLine("Exception in IpcBroadcast.TcpThreadProc:");
					Console.WriteLine(ex.Message);
				}
			}
		}

		private static void TcpThreadProcPriv()
		{
			TcpListener tl = null;
			int iPort = 0;
			for(int i = 0; i < (IpcTcpPortMaxExcl - IpcTcpPortMin); ++i)
			{
				try
				{
					iPort = Program.GlobalRandom.Next(IpcTcpPortMin, IpcTcpPortMaxExcl);

					tl = new TcpListener(IPAddress.Loopback, iPort);
					tl.ExclusiveAddressUse = true;

					tl.Start();
					break;
				}
				catch(Exception) { tl = null; }
			}
			if(tl == null) { Debug.Assert(false); return; }
			g_tl = tl;

			NumberFormatInfo nfi = NumberFormatInfo.InvariantInfo;
			string strLock = UrlUtil.EnsureTerminatingSeparator(UrlUtil.GetTempPath(),
				false) + TcpGetPrefix() + iPort.ToString(nfi) + IpcTcpSuffix;
			File.WriteAllBytes(strLock, new byte[1] { 0x31 });
			g_strLockFile = strLock;

			try
			{
				while(true)
				{
					using(TcpClient tc = tl.AcceptTcpClient())
					{
						try
						{
							using(NetworkStream s = tc.GetStream())
							{
								using(BinaryReader br = new BinaryReader(s))
								{
									TcpProcessMessage(br);
								}
							}
						}
						catch(Exception) { Debug.Assert(false); }
					}

					for(int i = g_lOldMsgs.Count - 1; i >= 0; --i)
					{
						if((DateTime.UtcNow - g_lOldMsgs[i].Value).TotalMilliseconds >
							IpcTcpTtlMs)
							g_lOldMsgs.RemoveAt(i);
					}
				}
			}
			catch(SocketException exS)
			{
				// Stopped by main thread
				Debug.Assert(exS.SocketErrorCode == SocketError.Interrupted);
			}
			catch(InvalidOperationException) { } // AcceptTcpClient fails at exit
		}

		private static byte[] TcpCreateMessage(PwUuid puID, Program.AppMessage msg,
			int lParam)
		{
			const int cbID = (int)PwUuid.UuidSize;

			byte[] pb = new byte[cbID + 8 + 4 + 4];
			Array.Copy(puID.UuidBytes, pb, cbID);
			MemUtil.Int64ToBytesEx(DateTime.UtcNow.ToBinary(), pb, cbID);
			MemUtil.Int32ToBytesEx((int)msg, pb, cbID + 8);
			MemUtil.Int32ToBytesEx(lParam, pb, cbID + 8 + 4);

			byte[] pbEnc = CryptoUtil.ProtectData(pb, IpcTcpOptEnt,
				DataProtectionScope.CurrentUser);

			byte[] pbSend = new byte[1 + 4 + pbEnc.Length];
			pbSend[0] = 1; // Message type
			MemUtil.Int32ToBytesEx(pbEnc.Length, pbSend, 1);
			Array.Copy(pbEnc, 0, pbSend, 1 + 4, pbEnc.Length);

			Debug.Assert(pbSend.Length <= IpcTcpMsgSizeMax);
			return pbSend;
		}

		private static void TcpProcessMessage(BinaryReader br)
		{
			if(br.ReadByte() != 1) { Debug.Assert(false); return; }

			int cb = MemUtil.BytesToInt32(br.ReadBytes(4));
			if((cb <= 0) || (cb > IpcTcpMsgSizeMax)) { Debug.Assert(false); return; }

			byte[] pbEnc = br.ReadBytes(cb);
			if((pbEnc == null) || (pbEnc.Length != cb)) { Debug.Assert(false); return; }

			const int cbID = (int)PwUuid.UuidSize;

			byte[] pb = CryptoUtil.UnprotectData(pbEnc, IpcTcpOptEnt,
				DataProtectionScope.CurrentUser);
			if((pb == null) || (pb.Length != (cbID + 8 + 4 + 4)))
			{
				Debug.Assert(false);
				return;
			}

			PwUuid puID = new PwUuid(MemUtil.Mid(pb, 0, cbID));
			foreach(KeyValuePair<PwUuid, DateTime> kvp in g_lOldMsgs)
			{
				if(puID.Equals(kvp.Key)) { Debug.Assert(false); return; }
			}

			DateTime dtNow = DateTime.UtcNow, dtMsg = DateTime.FromBinary(
				MemUtil.BytesToInt64(pb, cbID));
			if((dtNow - dtMsg).TotalMilliseconds > IpcTcpTtlMs)
			{
				Debug.Assert(false);
				return;
			}

			g_lOldMsgs.Add(new KeyValuePair<PwUuid, DateTime>(puID, dtMsg));

			int iMsg = MemUtil.BytesToInt32(pb, cbID + 8);
			int lParam = MemUtil.BytesToInt32(pb, cbID + 8 + 4);
			MainForm mf = Program.MainForm;

			VoidDelegate f = delegate()
			{
				try { mf.ProcessAppMessage(new IntPtr(iMsg), new IntPtr(lParam)); }
				catch(Exception) { Debug.Assert(false); }
			};
			mf.Invoke(f);
		}
	}
}
