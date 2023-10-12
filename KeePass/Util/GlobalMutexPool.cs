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
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

using KeePassLib.Cryptography;
using KeePassLib.Native;
using KeePassLib.Utility;

namespace KeePass.Util
{
	/// <summary>
	/// Low performance, system-wide mutex objects pool.
	/// </summary>
	public static class GlobalMutexPool
	{
		private static readonly List<KeyValuePair<string, Mutex>> g_lMutexesWin =
			new List<KeyValuePair<string, Mutex>>();
		private static readonly List<KeyValuePair<string, string>> g_lMutexesUnix =
			new List<KeyValuePair<string, string>>();

		private static int m_iLastRefresh = 0;

		private const double GmpMutexValidSecs = 190.0;
		private const int GmpMutexRefreshMs = 60 * 1000;

		private static readonly byte[] GmpOptEnt = { 0x08, 0xA6, 0x5E, 0x40 };

		public static bool CreateMutex(string strName, bool bInitiallyOwned)
		{
			if(!NativeLib.IsUnix()) // Windows
				return CreateMutexWin(strName, bInitiallyOwned);

			return CreateMutexUnix(strName);
		}

		private static bool CreateMutexWin(string strName, bool bInitiallyOwned)
		{
			try
			{
				bool bCreatedNew;
				Mutex m = new Mutex(bInitiallyOwned, strName, out bCreatedNew);

				if(bCreatedNew)
				{
					g_lMutexesWin.Add(new KeyValuePair<string, Mutex>(strName, m));
					return true;
				}
			}
			catch(Exception) { }

			return false;
		}

		private static bool CreateMutexUnix(string strName)
		{
			string strPath = GetMutexPath(strName);
			try
			{
				if(File.Exists(strPath))
				{
					byte[] pbEnc = File.ReadAllBytes(strPath);
					byte[] pb = CryptoUtil.UnprotectData(pbEnc, GmpOptEnt,
						DataProtectionScope.CurrentUser);
					if(pb.Length == 12)
					{
						long lTime = MemUtil.BytesToInt64(pb, 0);
						DateTime dt = DateTime.FromBinary(lTime);

						if((DateTime.UtcNow - dt).TotalSeconds < GmpMutexValidSecs)
						{
							int pid = MemUtil.BytesToInt32(pb, 8);
							try
							{
								Process.GetProcessById(pid); // Throws if process is not running
								return false; // Actively owned by other process
							}
							catch(Exception) { }
						}

						// Release the old mutex since process is not running
						ReleaseMutexUnix(strName);
					}
					else { Debug.Assert(false); }
				}
			}
			catch(Exception) { Debug.Assert(false); }

			try { WriteMutexFilePriv(strPath); }
			catch(Exception) { Debug.Assert(false); }

			g_lMutexesUnix.Add(new KeyValuePair<string, string>(strName, strPath));
			return true;
		}

		private static void WriteMutexFilePriv(string strPath)
		{
			byte[] pb = new byte[12];
			MemUtil.Int64ToBytes(DateTime.UtcNow.ToBinary()).CopyTo(pb, 0);
			MemUtil.Int32ToBytes(Process.GetCurrentProcess().Id).CopyTo(pb, 8);
			byte[] pbEnc = CryptoUtil.ProtectData(pb, GmpOptEnt,
				DataProtectionScope.CurrentUser);
			File.WriteAllBytes(strPath, pbEnc);
		}

		public static bool ReleaseMutex(string strName)
		{
			if(!NativeLib.IsUnix()) // Windows
				return ReleaseMutexWin(strName);

			return ReleaseMutexUnix(strName);
		}

		private static bool ReleaseMutexWin(string strName)
		{
			for(int i = 0; i < g_lMutexesWin.Count; ++i)
			{
				if(g_lMutexesWin[i].Key.Equals(strName, StrUtil.CaseIgnoreCmp))
				{
					try { g_lMutexesWin[i].Value.ReleaseMutex(); }
					catch(Exception) { Debug.Assert(false); }

					try { g_lMutexesWin[i].Value.Close(); }
					catch(Exception) { Debug.Assert(false); }

					g_lMutexesWin.RemoveAt(i);
					return true;
				}
			}

			return false;
		}

		private static bool ReleaseMutexUnix(string strName)
		{
			for(int i = 0; i < g_lMutexesUnix.Count; ++i)
			{
				if(g_lMutexesUnix[i].Key.Equals(strName, StrUtil.CaseIgnoreCmp))
				{
					for(int r = 0; r < 12; ++r)
					{
						try
						{
							if(!File.Exists(g_lMutexesUnix[i].Value)) break;

							File.Delete(g_lMutexesUnix[i].Value);
							break;
						}
						catch(Exception) { }

						Thread.Sleep(10);
					}

					g_lMutexesUnix.RemoveAt(i);
					return true;
				}
			}

			return false;
		}

		public static void ReleaseAll()
		{
			if(!NativeLib.IsUnix()) // Windows
			{
				for(int i = g_lMutexesWin.Count - 1; i >= 0; --i)
					ReleaseMutexWin(g_lMutexesWin[i].Key);
			}
			else
			{
				for(int i = g_lMutexesUnix.Count - 1; i >= 0; --i)
					ReleaseMutexUnix(g_lMutexesUnix[i].Key);
			}
		}

		public static void Refresh()
		{
			if(!NativeLib.IsUnix()) return; // Windows, no refresh required

			// Unix
			int iTicksDiff = (Environment.TickCount - m_iLastRefresh);
			if(iTicksDiff >= GmpMutexRefreshMs)
			{
				m_iLastRefresh = Environment.TickCount;

				for(int i = 0; i < g_lMutexesUnix.Count; ++i)
				{
					try { WriteMutexFilePriv(g_lMutexesUnix[i].Value); }
					catch(Exception) { Debug.Assert(false); }
				}
			}
		}

		private static string GetMutexPath(string strName)
		{
			string strDir = UrlUtil.EnsureTerminatingSeparator(
				UrlUtil.GetTempPath(), false);
			return (strDir + IpcUtilEx.IpcMsgFilePreID + IpcBroadcast.GetUserID() +
				"-Mutex-" + strName + ".tmp");
		}
	}
}
