/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2024 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;

#if (!KeePassLibSD && !KeePassUAP)
using System.Security.AccessControl;
#endif

using Microsoft.Win32;

using KeePassLib.Cryptography;
using KeePassLib.Delegates;
using KeePassLib.Native;
using KeePassLib.Resources;
using KeePassLib.Utility;

namespace KeePassLib.Serialization
{
	public sealed class FileTransactionEx : IDisposable
	{
		private readonly bool m_bTransacted;
		private IOConnectionInfo m_iocBase; // Null means disposed
		private IOConnectionInfo m_iocTemp;
		private IOConnectionInfo m_iocTxfMidFallback = null; // Null <=> TxF not used

		private bool m_bMadeUnhidden = false;
		private readonly List<IOConnectionInfo> m_lToDelete = new List<IOConnectionInfo>();

		internal const string StrTempSuffix = ".tmp";
		private static readonly string StrTxfTempPrefix = PwDefs.ShortProductName + "_TxF_";
		internal const string StrTxfTempSuffix = ".tmp";

		private static readonly Dictionary<string, bool> g_dEnabled =
			new Dictionary<string, bool>(StrUtil.CaseIgnoreComparer);

		private static bool g_bExtraSafe = false;
		internal static bool ExtraSafe
		{
			get { return g_bExtraSafe; }
			set { g_bExtraSafe = value; }
		}

		public FileTransactionEx(IOConnectionInfo iocBaseFile) :
			this(iocBaseFile, true)
		{
		}

		public FileTransactionEx(IOConnectionInfo iocBaseFile, bool bTransacted)
		{
			if(iocBaseFile == null) throw new ArgumentNullException("iocBaseFile");

			m_bTransacted = bTransacted;

			m_iocBase = iocBaseFile.CloneDeep();
			if(m_iocBase.IsLocalFile())
				m_iocBase.Path = UrlUtil.GetShortestAbsolutePath(m_iocBase.Path);

			string strPath = m_iocBase.Path;

			if(m_iocBase.IsLocalFile())
			{
				try
				{
					if(File.Exists(strPath))
					{
						// Symbolic links are realized via reparse points;
						// https://msdn.microsoft.com/en-us/library/windows/desktop/aa365503.aspx
						// https://msdn.microsoft.com/en-us/library/windows/desktop/aa365680.aspx
						// https://msdn.microsoft.com/en-us/library/windows/desktop/aa365006.aspx
						// Performing a file transaction on a symbolic link
						// would delete/replace the symbolic link instead of
						// writing to its target
						FileAttributes fa = File.GetAttributes(strPath);
						if((long)(fa & FileAttributes.ReparsePoint) != 0)
							m_bTransacted = false;
					}
					else
					{
						// If the base and the temporary file are in different
						// folders and the base file doesn't exist (i.e. we can't
						// backup the ACL), a transaction would cause the new file
						// to have the default ACL of the temporary folder instead
						// of the one of the base folder; therefore, we don't use
						// a transaction when the base file doesn't exist (this
						// also results in other applications monitoring the folder
						// to see one file creation only)
						m_bTransacted = false;
					}
				}
				catch(Exception) { Debug.Assert(false); }
			}

#if !KeePassUAP
			// Prevent transactions for FTP URLs under .NET 4.0 in order to
			// avoid/workaround .NET bug 621450:
			// https://connect.microsoft.com/VisualStudio/feedback/details/621450/problem-renaming-file-on-ftp-server-using-ftpwebrequest-in-net-framework-4-0-vs2010-only
			if(strPath.StartsWith("ftp:", StrUtil.CaseIgnoreCmp) &&
				(Environment.Version.Major >= 4) && !NativeLib.IsUnix())
				m_bTransacted = false;
#endif

			foreach(KeyValuePair<string, bool> kvp in g_dEnabled)
			{
				if(strPath.StartsWith(kvp.Key, StrUtil.CaseIgnoreCmp))
				{
					m_bTransacted = kvp.Value;
					break;
				}
			}

			if(m_bTransacted)
			{
				m_iocTemp = m_iocBase.CloneDeep();
				m_iocTemp.Path += StrTempSuffix;

				TxfPrepare(); // Adjusts m_iocTemp
			}
			else m_iocTemp = m_iocBase;
		}

		~FileTransactionEx()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool bDisposing)
		{
			m_iocBase = null;
			if(!bDisposing) return;

			try
			{
				foreach(IOConnectionInfo ioc in m_lToDelete)
				{
					if(IOConnection.FileExists(ioc, false))
						IOConnection.DeleteFile(ioc);
				}

				m_lToDelete.Clear();
			}
			catch(Exception) { Debug.Assert(false); }
		}

		public Stream OpenWrite()
		{
			if(m_iocBase == null) { Debug.Assert(false); throw new ObjectDisposedException(null); }

			if(!m_bTransacted) m_bMadeUnhidden |= UrlUtil.UnhideFile(m_iocTemp.Path);

			return IOConnection.OpenWrite(m_iocTemp);
		}

		public void CommitWrite()
		{
			if(m_iocBase == null) { Debug.Assert(false); throw new ObjectDisposedException(null); }

			if(!m_bTransacted)
			{
				if(m_bMadeUnhidden) UrlUtil.HideFile(m_iocTemp.Path, true);
			}
			else CommitWriteTransaction();

			m_iocBase = null; // Dispose
		}

		private void CommitWriteTransaction()
		{
			if(g_bExtraSafe)
			{
				if(!IOConnection.FileExists(m_iocTemp))
					throw new FileNotFoundException(m_iocTemp.Path +
						MessageService.NewLine + KLRes.FileSaveFailed);
			}

			bool bMadeUnhidden = UrlUtil.UnhideFile(m_iocBase.Path);

#if !KeePassUAP
			// 'All' includes 'Audit' (SACL), which requires SeSecurityPrivilege,
			// which we usually don't have and therefore get an exception;
			// trying to set 'Owner' or 'Group' can result in an
			// UnauthorizedAccessException; thus we restore 'Access' (DACL) only
			const AccessControlSections acs = AccessControlSections.Access;

			bool bEfsEncrypted = false;
			byte[] pbSec = null;
#endif
			DateTime? otCreation = null;
			SimpleStat sStat = null;

			bool bBaseExists = IOConnection.FileExists(m_iocBase);
			if(bBaseExists && m_iocBase.IsLocalFile())
			{
				// FileAttributes faBase = FileAttributes.Normal;
				try
				{
#if !KeePassUAP
					FileAttributes faBase = File.GetAttributes(m_iocBase.Path);
					bEfsEncrypted = ((long)(faBase & FileAttributes.Encrypted) != 0);
					try { if(bEfsEncrypted) File.Decrypt(m_iocBase.Path); } // For TxF
					catch(Exception) { Debug.Assert(false); }
#endif
					otCreation = File.GetCreationTimeUtc(m_iocBase.Path);
					sStat = SimpleStat.Get(m_iocBase.Path);
#if !KeePassUAP
					// May throw with Mono
					FileSecurity sec = File.GetAccessControl(m_iocBase.Path, acs);
					if(sec != null) pbSec = sec.GetSecurityDescriptorBinaryForm();
#endif
				}
				catch(Exception) { Debug.Assert(NativeLib.IsUnix()); }

				// if((long)(faBase & FileAttributes.ReadOnly) != 0)
				//	throw new UnauthorizedAccessException();
			}

			if(!TxfMove())
			{
				if(bBaseExists) IOConnection.DeleteFile(m_iocBase);
				IOConnection.RenameFile(m_iocTemp, m_iocBase);
			}
			else { Debug.Assert(pbSec != null); } // TxF success => NTFS => has ACL

			try
			{
				// If File.GetCreationTimeUtc fails, it may return a
				// date with year 1601, and Unix times start in 1970,
				// so testing for 1971 should ensure validity;
				// https://msdn.microsoft.com/en-us/library/system.io.file.getcreationtimeutc.aspx
				if(otCreation.HasValue && (otCreation.Value.Year >= 1971))
					File.SetCreationTimeUtc(m_iocBase.Path, otCreation.Value);

				if(sStat != null) SimpleStat.Set(m_iocBase.Path, sStat);

#if !KeePassUAP
				if(bEfsEncrypted)
				{
					try { File.Encrypt(m_iocBase.Path); }
					catch(Exception) { Debug.Assert(false); }
				}

				// File.SetAccessControl(m_iocBase.Path, secPrev);
				// Directly calling File.SetAccessControl with the previous
				// FileSecurity object does not work; the binary form
				// indirection is required;
				// https://sourceforge.net/p/keepass/bugs/1738/
				// https://msdn.microsoft.com/en-us/library/system.io.file.setaccesscontrol.aspx
				if((pbSec != null) && (pbSec.Length != 0))
				{
					FileSecurity sec = new FileSecurity();
					sec.SetSecurityDescriptorBinaryForm(pbSec, acs);

					File.SetAccessControl(m_iocBase.Path, sec);
				}
#endif
			}
			catch(Exception) { Debug.Assert(false); }

			if(bMadeUnhidden) UrlUtil.HideFile(m_iocBase.Path, true);
		}

		// For plugins
		public static void Configure(string strPrefix, bool? obTransacted)
		{
			if(string.IsNullOrEmpty(strPrefix)) { Debug.Assert(false); return; }

			if(obTransacted.HasValue)
				g_dEnabled[strPrefix] = obTransacted.Value;
			else g_dEnabled.Remove(strPrefix);
		}

		private static bool TxfIsSupported(char chDriveLetter)
		{
			if(chDriveLetter == '\0') return false;

			try
			{
				string strRoot = (new string(chDriveLetter, 1)) + ":\\";

				const int cch = NativeMethods.MAX_PATH + 1;
				StringBuilder sbName = new StringBuilder(cch + 1);
				uint uSerial = 0, cchMaxComp = 0, uFlags = 0;
				StringBuilder sbFileSystem = new StringBuilder(cch + 1);

				if(!NativeMethods.GetVolumeInformation(strRoot, sbName, (uint)cch,
					ref uSerial, ref cchMaxComp, ref uFlags, sbFileSystem, (uint)cch))
				{
					Debug.Assert(false, (new Win32Exception()).Message);
					return false;
				}

				return ((uFlags & NativeMethods.FILE_SUPPORTS_TRANSACTIONS) != 0);
			}
			catch(Exception) { Debug.Assert(false); }

			return false;
		}

		private void TxfPrepare()
		{
			try
			{
				if(NativeLib.IsUnix()) return;
				if(!m_iocBase.IsLocalFile()) return;
				if(TxfIsUnusable()) return;

				string strID = StrUtil.AlphaNumericOnly(Convert.ToBase64String(
					CryptoRandom.Instance.GetRandomBytes(16)));
				string strTempDir = UrlUtil.GetTempPath();
				// See also ClearOld method
				string strTemp = UrlUtil.EnsureTerminatingSeparator(strTempDir,
					false) + StrTxfTempPrefix + strID + StrTxfTempSuffix;

				char chB = UrlUtil.GetDriveLetter(m_iocBase.Path);
				char chT = UrlUtil.GetDriveLetter(strTemp);
				if(!TxfIsSupported(chB)) return;
				if((chT != chB) && !TxfIsSupported(chT)) return;

				m_iocTxfMidFallback = m_iocTemp;
				m_iocTemp = IOConnectionInfo.FromPath(strTemp);

				m_lToDelete.Add(m_iocTemp);
			}
			catch(Exception) { Debug.Assert(false); m_iocTxfMidFallback = null; }
		}

		private bool TxfMove()
		{
			if(m_iocTxfMidFallback == null) return false;

			if(TxfMoveWithTx()) return true;

			// Move the temporary file onto the base file's drive first,
			// such that it cannot happen that both the base file and
			// the temporary file are deleted/corrupted
			const uint f = (NativeMethods.MOVEFILE_COPY_ALLOWED |
				NativeMethods.MOVEFILE_REPLACE_EXISTING);
			bool b = NativeMethods.MoveFileEx(m_iocTemp.Path, m_iocTxfMidFallback.Path, f);
			if(b) b = NativeMethods.MoveFileEx(m_iocTxfMidFallback.Path, m_iocBase.Path, f);
			if(!b) throw new Win32Exception();

			Debug.Assert(!File.Exists(m_iocTemp.Path));
			Debug.Assert(!File.Exists(m_iocTxfMidFallback.Path));
			return true;
		}

		private bool TxfMoveWithTx()
		{
			IntPtr hTx = new IntPtr((int)NativeMethods.INVALID_HANDLE_VALUE);
			Debug.Assert(hTx.ToInt64() == NativeMethods.INVALID_HANDLE_VALUE);
			try
			{
				string strTx = PwDefs.ShortProductName + " TxF - " +
					StrUtil.AlphaNumericOnly(Convert.ToBase64String(
					CryptoRandom.Instance.GetRandomBytes(16)));
				const int mchTx = NativeMethods.MAX_TRANSACTION_DESCRIPTION_LENGTH;
				if(strTx.Length >= mchTx) strTx = strTx.Substring(0, mchTx - 1);

				hTx = NativeMethods.CreateTransaction(IntPtr.Zero,
					IntPtr.Zero, 0, 0, 0, 0, strTx);
				if(hTx.ToInt64() == NativeMethods.INVALID_HANDLE_VALUE)
				{
					Debug.Assert(false, (new Win32Exception()).Message);
					return false;
				}

				if(!NativeMethods.MoveFileTransacted(m_iocTemp.Path, m_iocBase.Path,
					IntPtr.Zero, IntPtr.Zero, (NativeMethods.MOVEFILE_COPY_ALLOWED |
					NativeMethods.MOVEFILE_REPLACE_EXISTING), hTx))
				{
					Debug.Assert(false, (new Win32Exception()).Message);
					return false;
				}

				if(!NativeMethods.CommitTransaction(hTx))
				{
					Debug.Assert(false, (new Win32Exception()).Message);
					return false;
				}

				Debug.Assert(!File.Exists(m_iocTemp.Path));
				return true;
			}
			catch(Exception) { Debug.Assert(false); }
			finally
			{
				if(hTx.ToInt64() != NativeMethods.INVALID_HANDLE_VALUE)
				{
					try { if(!NativeMethods.CloseHandle(hTx)) { Debug.Assert(false); } }
					catch(Exception) { Debug.Assert(false); }
				}
			}

			return false;
		}

		private bool TxfIsUnusable()
		{
			try
			{
				string strReleaseId = (Registry.GetValue(
					"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion",
					"ReleaseId", string.Empty) as string);

				// Due to a bug in Microsoft's 'cldflt.sys' driver, a TxF transaction
				// results in a Blue Screen of Death on Windows 10 1903/1909;
				// https://www.windowslatest.com/2019/10/20/windows-10-update-issues-bsod-broken-apps-and-defender-atp/
				// https://sourceforge.net/p/keepass/discussion/329221/thread/924b94ea48/
				// This bug is fixed by the Windows update 4530684;
				// https://support.microsoft.com/en-us/help/4530684/windows-10-update-kb4530684
				// if(strReleaseId == "1903") return true;
				// if(strReleaseId == "1909") return true;

				if(strReleaseId != "1809") return false;

				// On Windows 10 1809, OneDrive crashes if the file is
				// in a OneDrive folder;
				// https://sourceforge.net/p/keepass/discussion/329220/thread/672ffecc65/
				// https://sourceforge.net/p/keepass/discussion/329221/thread/514786c23a/

				string strFile = m_iocBase.Path;

				GFunc<string, string, bool> fMatch = delegate(string strRoot, string strSfx)
				{
					if(string.IsNullOrEmpty(strRoot)) return false;
					string strPfx = UrlUtil.EnsureTerminatingSeparator(
						strRoot, false) + strSfx;
					return strFile.StartsWith(strPfx, StrUtil.CaseIgnoreCmp);
				};
				GFunc<string, string, bool> fMatchEnv = delegate(string strEnv, string strSfx)
				{
					return fMatch(Environment.GetEnvironmentVariable(strEnv), strSfx);
				};

				string strKnown = NativeMethods.GetKnownFolderPath(
					NativeMethods.FOLDERID_SkyDrive);
				if(fMatch(strKnown, string.Empty)) return true;

				if(fMatchEnv("USERPROFILE", "OneDrive\\")) return true;
				if(fMatchEnv("OneDrive", string.Empty)) return true;
				if(fMatchEnv("OneDriveCommercial", string.Empty)) return true;
				if(fMatchEnv("OneDriveConsumer", string.Empty)) return true;

				using(RegistryKey kAccs = Registry.CurrentUser.OpenSubKey(
					"Software\\Microsoft\\OneDrive\\Accounts", false))
				{
					string[] vAccs = (((kAccs != null) ? kAccs.GetSubKeyNames() :
						null) ?? MemUtil.EmptyArray<string>());

					foreach(string strAcc in vAccs)
					{
						if(string.IsNullOrEmpty(strAcc)) { Debug.Assert(false); continue; }

						using(RegistryKey kTenants = kAccs.OpenSubKey(
							strAcc + "\\Tenants", false))
						{
							string[] vTenants = (((kTenants != null) ?
								kTenants.GetSubKeyNames() : null) ?? MemUtil.EmptyArray<string>());

							foreach(string strT in vTenants)
							{
								if(string.IsNullOrEmpty(strT)) { Debug.Assert(false); continue; }

								using(RegistryKey kT = kTenants.OpenSubKey(strT, false))
								{
									string[] vPaths = (((kT != null) ?
										kT.GetValueNames() : null) ?? MemUtil.EmptyArray<string>());

									foreach(string strPath in vPaths)
									{
										if((strPath == null) || (strPath.Length < 4) ||
											(strPath[1] != ':'))
										{
											Debug.Assert(false);
											continue;
										}

										if(fMatch(strPath, string.Empty)) return true;
									}
								}
							}
						}
					}
				}
			}
			catch(Exception) { Debug.Assert(false); }

			return false;
		}

		internal static void ClearOld()
		{
			try
			{
				// See also TxfPrepare method
				DirectoryInfo di = new DirectoryInfo(UrlUtil.GetTempPath());
				List<FileInfo> l = UrlUtil.GetFileInfos(di, StrTxfTempPrefix +
					"*" + StrTxfTempSuffix, SearchOption.TopDirectoryOnly);

				foreach(FileInfo fi in l)
				{
					if(fi == null) { Debug.Assert(false); continue; }
					if(!fi.Name.StartsWith(StrTxfTempPrefix, StrUtil.CaseIgnoreCmp) ||
						!fi.Name.EndsWith(StrTxfTempSuffix, StrUtil.CaseIgnoreCmp))
						continue;

					if((DateTime.UtcNow - fi.LastWriteTimeUtc).TotalDays > 1.0)
						fi.Delete();
				}
			}
			catch(Exception) { Debug.Assert(false); }
		}
	}
}
