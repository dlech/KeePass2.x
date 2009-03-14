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
using System.Diagnostics;
using System.Runtime.InteropServices;

using KeePass.Resources;

using KeePassLib;
using KeePassLib.Security;
using KeePassLib.Delegates;
using KeePassLib.Interfaces;
using KeePassLib.Keys;
using KeePassLib.Utility;
using KeePassLib.Resources;

namespace KeePass.DataExchange
{
	/// <summary>
	/// Serialization to KeePass KDB files.
	/// </summary>
	public sealed class Kdb3File
	{
		private const StringComparison StrCaseIgnoreCmp = StringComparison.OrdinalIgnoreCase;

		private PwDatabase m_pwDatabase = null;
		private IStatusLogger m_slLogger = null;

		private const string Kdb3Prefix = "KDB3: ";

		private const string AutoTypePrefix = "Auto-Type";
		private const string AutoTypeWindowPrefix = "Auto-Type-Window";

		private const string UrlOverridePrefix = "Url-Override:";

		public static bool IsLibraryInstalled()
		{
			Exception ex;
			return IsLibraryInstalled(out ex);
		}

		public static bool IsLibraryInstalled(out Exception ex)
		{
			try
			{
				Kdb3Manager mgr = new Kdb3Manager();
				mgr.Unload();
			}
			catch(Exception exMgr)
			{
				ex = exMgr;
				return false;
			}

			ex = null;
			return true;
		}

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="pwDataStore">The <c>PwDatabase</c> instance that the class
		/// will load file data into or use to create a KDB file. Must not be <c>null</c>.</param>
		/// <exception cref="System.ArgumentNullException">Thrown if the database
		/// reference is <c>null</c>.</exception>
		public Kdb3File(PwDatabase pwDataStore, IStatusLogger slLogger)
		{
			Debug.Assert(pwDataStore != null);
			if(pwDataStore == null) throw new ArgumentNullException("pwDataStore");
			m_pwDatabase = pwDataStore;

			m_slLogger = slLogger;
		}

		private static Kdb3ErrorCode SetDatabaseKey(Kdb3Manager mgr, CompositeKey pwKey)
		{
			Kdb3ErrorCode e;

			bool bPassword = pwKey.ContainsType(typeof(KcpPassword));
			bool bKeyFile = pwKey.ContainsType(typeof(KcpKeyFile));

			string strPassword = (bPassword ? (pwKey.GetUserKey(
				typeof(KcpPassword)) as KcpPassword).Password.ReadString() : string.Empty);
			string strKeyFile = (bKeyFile ? (pwKey.GetUserKey(
				typeof(KcpKeyFile)) as KcpKeyFile).Path : string.Empty);

			if(bPassword && bKeyFile)
				e = mgr.SetMasterKey(strKeyFile, true, strPassword, IntPtr.Zero, false);
			else if(bPassword && !bKeyFile)
				e = mgr.SetMasterKey(strPassword, false, null, IntPtr.Zero, false);
			else if(!bPassword && bKeyFile)
				e = mgr.SetMasterKey(strKeyFile, true, null, IntPtr.Zero, false);
			else { Debug.Assert(false); throw new Exception(KLRes.InvalidCompositeKey); }

			return e;
		}

		/// <summary>
		/// Loads a KDB file and stores all loaded entries in the current
		/// PwDatabase instance.
		/// </summary>
		/// <param name="strFilePath">Relative or absolute path to the file to open.</param>
		public void Load(string strFilePath)
		{
			Debug.Assert(strFilePath != null);
			if(strFilePath == null) throw new ArgumentNullException("strFilePath");

			Kdb3Manager mgr = new Kdb3Manager();
			Kdb3ErrorCode e;

			e = Kdb3File.SetDatabaseKey(mgr, m_pwDatabase.MasterKey);
			if(e != Kdb3ErrorCode.Success) throw new Exception(KLRes.InvalidCompositeKey);

			e = mgr.OpenDatabase(strFilePath, IntPtr.Zero);
			if(e != Kdb3ErrorCode.Success)
			{
				mgr.Unload();
				throw new Exception(KLRes.FileLoadFailed);
			}

			// Copy properties
			m_pwDatabase.KeyEncryptionRounds = mgr.KeyTransformationRounds;

			// Read groups and entries
			Dictionary<UInt32, PwGroup> dictGroups = ReadGroups(mgr);
			ReadEntries(mgr, dictGroups);

			mgr.Unload();
		}

		private Dictionary<UInt32, PwGroup> ReadGroups(Kdb3Manager mgr)
		{
			uint uGroupCount = mgr.GroupCount;
			Dictionary<UInt32, PwGroup> dictGroups = new Dictionary<uint, PwGroup>();

			Stack<PwGroup> vGroupStack = new Stack<PwGroup>();
			vGroupStack.Push(m_pwDatabase.RootGroup);

			DateTime dtNeverExpire = Kdb3Manager.GetNeverExpireTime();

			for(uint uGroup = 0; uGroup < uGroupCount; ++uGroup)
			{
				Kdb3Group g = mgr.GetGroup(uGroup);

				PwGroup pg = new PwGroup(true, false);

				pg.Name = g.Name;
				pg.IconId = (g.ImageId < (uint)PwIcon.Count) ? (PwIcon)g.ImageId : PwIcon.Folder;
				
				pg.CreationTime = g.CreationTime.ToDateTime();
				pg.LastModificationTime = g.LastModificationTime.ToDateTime();
				pg.LastAccessTime = g.LastAccessTime.ToDateTime();
				pg.ExpiryTime = g.ExpirationTime.ToDateTime();

				pg.Expires = (pg.ExpiryTime != dtNeverExpire);

				pg.IsExpanded = ((g.Flags & (uint)Kdb3GroupFlags.Expanded) != 0);

				while(g.Level < (vGroupStack.Count - 1))
					vGroupStack.Pop();

				vGroupStack.Peek().AddGroup(pg, true);

				dictGroups[g.GroupId] = pg;

				if(g.Level == (uint)(vGroupStack.Count - 1))
					vGroupStack.Push(pg);
			}

			return dictGroups;
		}

		private void ReadEntries(Kdb3Manager mgr, Dictionary<UInt32, PwGroup> dictGroups)
		{
			DateTime dtNeverExpire = Kdb3Manager.GetNeverExpireTime();
			uint uEntryCount = mgr.EntryCount;

			for(uint uEntry = 0; uEntry < uEntryCount; ++uEntry)
			{
				Kdb3Entry e = mgr.GetEntry(uEntry);

				PwGroup pgContainer;
				if(!dictGroups.TryGetValue(e.GroupId, out pgContainer))
				{
					Debug.Assert(false);
					continue;
				}

				PwEntry pe = new PwEntry(false, false);
				pe.Uuid = new PwUuid(e.Uuid.ToByteArray());

				pgContainer.AddEntry(pe, true);

				pe.IconId = (e.ImageId < (uint)PwIcon.Count) ? (PwIcon)e.ImageId : PwIcon.Key;

				pe.Strings.Set(PwDefs.TitleField, new ProtectedString(
					m_pwDatabase.MemoryProtection.ProtectTitle, e.Title));
				pe.Strings.Set(PwDefs.UserNameField, new ProtectedString(
					m_pwDatabase.MemoryProtection.ProtectUserName, e.UserName));
				pe.Strings.Set(PwDefs.PasswordField, new ProtectedString(
					m_pwDatabase.MemoryProtection.ProtectPassword, e.Password));
				pe.Strings.Set(PwDefs.UrlField, new ProtectedString(
					m_pwDatabase.MemoryProtection.ProtectUrl, e.Url));

				string strNotes = e.Additional;
				ImportAutoType(ref strNotes, pe);
				ImportUrlOverride(ref strNotes, pe);
				pe.Strings.Set(PwDefs.NotesField, new ProtectedString(
					m_pwDatabase.MemoryProtection.ProtectNotes, strNotes));

				pe.CreationTime = e.CreationTime.ToDateTime();
				pe.LastModificationTime = e.LastModificationTime.ToDateTime();
				pe.LastAccessTime = e.LastAccessTime.ToDateTime();
				pe.ExpiryTime = e.ExpirationTime.ToDateTime();

				pe.Expires = (pe.ExpiryTime != dtNeverExpire);

				if((e.BinaryDataLength > 0) && (e.BinaryData != IntPtr.Zero))
				{
					byte[] pbData = Kdb3Manager.ReadBinary(e.BinaryData, e.BinaryDataLength);
					Debug.Assert(pbData.Length == e.BinaryDataLength);

					string strDesc = e.BinaryDescription;
					if((strDesc == null) || (strDesc.Length == 0))
						strDesc = "Attachment";

					pe.Binaries.Set(strDesc, new ProtectedBinary(false, pbData));
				}

				if(m_slLogger != null)
					if(!m_slLogger.SetProgress((100 * uEntry) / uEntryCount))
						throw new Exception(KPRes.Cancel);
			}
		}

		/// <summary>
		/// Save the contents of the current <c>PwDatabase</c> to a KDB file.
		/// </summary>
		/// <param name="strSaveToFile">Location to save the KDB file to.</param>
		public void Save(string strSaveToFile, PwGroup pgDataSource)
		{
			Debug.Assert(strSaveToFile != null);
			if(strSaveToFile == null) throw new ArgumentNullException("strSaveToFile");

			Kdb3Manager mgr = new Kdb3Manager();

			Kdb3ErrorCode e = Kdb3File.SetDatabaseKey(mgr, m_pwDatabase.MasterKey);
			if(e != Kdb3ErrorCode.Success)
			{
				Debug.Assert(false);
				throw new Exception(KLRes.InvalidCompositeKey);
			}

			if(m_slLogger != null)
			{
				if(m_pwDatabase.Name.Length != 0)
					m_slLogger.SetText(Kdb3Prefix + KPRes.FormatNoDatabaseName, LogStatusType.Warning);
				if(m_pwDatabase.Description.Length != 0)
					m_slLogger.SetText(Kdb3Prefix + KPRes.FormatNoDatabaseDesc, LogStatusType.Warning);
			}

			// Set properties
			if(m_pwDatabase.KeyEncryptionRounds >= (ulong)UInt32.MaxValue)
				mgr.KeyTransformationRounds = unchecked(uint.MaxValue - 1);
			else mgr.KeyTransformationRounds = (uint)m_pwDatabase.KeyEncryptionRounds;

			PwGroup pgRoot = (pgDataSource ?? m_pwDatabase.RootGroup);

			// Write groups and entries
			Dictionary<PwGroup, UInt32> dictGroups = WriteGroups(mgr, pgRoot);
			WriteEntries(mgr, dictGroups, pgRoot);

			e = mgr.SaveDatabase(strSaveToFile);
			if(e != Kdb3ErrorCode.Success) throw new Exception(KLRes.FileSaveFailed);

			mgr.Unload();
		}

		private static Dictionary<PwGroup, UInt32> WriteGroups(Kdb3Manager mgr,
			PwGroup pgRoot)
		{
			Dictionary<PwGroup, UInt32> dictGroups = new Dictionary<PwGroup, uint>();

			uint uGroupIndex = 1;
			DateTime dtNeverExpire = Kdb3Manager.GetNeverExpireTime();

			GroupHandler gh = delegate(PwGroup pg)
			{
				if(pg == pgRoot) return true;

				Kdb3Group grp = new Kdb3Group();

				grp.GroupId = uGroupIndex;
				dictGroups[pg] = grp.GroupId;

				grp.ImageId = (uint)pg.IconId;
				grp.Name = pg.Name;
				grp.CreationTime.Set(pg.CreationTime);
				grp.LastModificationTime.Set(pg.LastModificationTime);
				grp.LastAccessTime.Set(pg.LastAccessTime);

				if(pg.Expires)
					grp.ExpirationTime.Set(pg.ExpiryTime);
				else grp.ExpirationTime.Set(dtNeverExpire);

				grp.Level = (ushort)(pg.GetLevel() - 1);

				if(pg.IsExpanded) grp.Flags |= (uint)Kdb3GroupFlags.Expanded;

				if(!mgr.AddGroup(ref grp))
				{
					Debug.Assert(false);
					throw new InvalidOperationException();
				}

				++uGroupIndex;
				return true;
			};

			pgRoot.TraverseTree(TraversalMethod.PreOrder, gh, null);
			Debug.Assert(dictGroups.Count == (int)(uGroupIndex - 1));
			return dictGroups;
		}

		private void WriteEntries(Kdb3Manager mgr, Dictionary<PwGroup, uint> dictGroups,
			PwGroup pgRoot)
		{
			bool bWarnedOnce = false;
			uint uGroupCount, uEntryCount, uEntriesSaved = 0;
			pgRoot.GetCounts(true, out uGroupCount, out uEntryCount);

			DateTime dtNeverExpire = Kdb3Manager.GetNeverExpireTime();

			EntryHandler eh = delegate(PwEntry pe)
			{
				Kdb3Entry e = new Kdb3Entry();

				e.Uuid.Set(pe.Uuid.UuidBytes);

				if(pe.ParentGroup != pgRoot)
					e.GroupId = dictGroups[pe.ParentGroup];
				else
				{
					e.GroupId = 1;
					if((m_slLogger != null) && !bWarnedOnce)
					{
						m_slLogger.SetText(Kdb3Prefix +
							KPRes.FormatNoRootEntries, LogStatusType.Warning);
						bWarnedOnce = true;
					}

					if(dictGroups.Count == 0)
						throw new Exception(KPRes.FormatNoSubGroupsInRoot);
				}

				e.ImageId = (uint)pe.IconId;

				e.Title = pe.Strings.ReadSafe(PwDefs.TitleField);
				e.UserName = pe.Strings.ReadSafe(PwDefs.UserNameField);
				e.Password = pe.Strings.ReadSafe(PwDefs.PasswordField);
				e.Url = pe.Strings.ReadSafe(PwDefs.UrlField);

				string strNotes = pe.Strings.ReadSafe(PwDefs.NotesField);
				ExportAutoType(pe, ref strNotes);
				ExportUrlOverride(pe, ref strNotes);
				e.Additional = strNotes;

				e.PasswordLength = (uint)e.Password.Length;

				Debug.Assert(TimeUtil.PwTimeLength == 7);
				e.CreationTime.Set(pe.CreationTime);
				e.LastModificationTime.Set(pe.LastModificationTime);
				e.LastAccessTime.Set(pe.LastAccessTime);

				if(pe.Expires) e.ExpirationTime.Set(pe.ExpiryTime);
				else e.ExpirationTime.Set(dtNeverExpire);

				IntPtr hBinaryData = IntPtr.Zero;
				if(pe.Binaries.UCount >= 1)
				{
					foreach(KeyValuePair<string, ProtectedBinary> kvp in pe.Binaries)
					{
						e.BinaryDescription = kvp.Key;

						byte[] pbAttached = kvp.Value.ReadData();
						e.BinaryDataLength = (uint)pbAttached.Length;

						if(e.BinaryDataLength > 0)
						{
							hBinaryData = Marshal.AllocHGlobal((int)e.BinaryDataLength);
							Marshal.Copy(pbAttached, 0, hBinaryData, (int)e.BinaryDataLength);

							e.BinaryData = hBinaryData;
						}

						break;
					}

					if((pe.Binaries.UCount > 1) && (m_slLogger != null))
						m_slLogger.SetText(Kdb3Prefix + KPRes.FormatOnlyOneAttachment + "\r\n\r\n" +
							KPRes.Entry + ":\r\n" + KPRes.Title + ": " + e.Title + "\r\n" +
							KPRes.UserName + ": " + e.UserName, LogStatusType.Warning);
				}

				bool bResult = mgr.AddEntry(ref e);

				Marshal.FreeHGlobal(hBinaryData);
				hBinaryData = IntPtr.Zero;

				if(!bResult)
				{
					Debug.Assert(false);
					throw new InvalidOperationException();
				}

				++uEntriesSaved;
				if(m_slLogger != null)
					if(!m_slLogger.SetProgress((100 * uEntriesSaved) / uEntryCount))
						return false;

				return true;
			};

			if(!pgRoot.TraverseTree(TraversalMethod.PreOrder, null, eh))
				throw new InvalidOperationException();
		}

		/* private static void ImportAutoType(ref string strNotes, PwEntry peStorage)
		{
			string str = strNotes;
			char[] vTrim = new char[]{ '\r', '\n', '\t', ' ' };

			int nFirstAutoType = str.IndexOf(AutoTypePrefix, StringComparison.OrdinalIgnoreCase);
			if(nFirstAutoType < 0) nFirstAutoType = int.MaxValue;
			int nFirstAutoTypeWindow = str.IndexOf(AutoTypeWindowPrefix, StringComparison.OrdinalIgnoreCase);
			if((nFirstAutoTypeWindow >= 0) && (nFirstAutoTypeWindow < nFirstAutoType))
			{
				int nWindowEnd = str.IndexOf('\n', nFirstAutoTypeWindow);
				if(nWindowEnd < 0) nWindowEnd = str.Length - 1;

				string strWindow = str.Substring(nFirstAutoTypeWindow + AutoTypeWindowPrefix.Length,
					nWindowEnd - nFirstAutoTypeWindow - AutoTypeWindowPrefix.Length + 1);
				strWindow = strWindow.Trim(vTrim);

				str = str.Remove(nFirstAutoTypeWindow, nWindowEnd - nFirstAutoTypeWindow + 1);
				peStorage.AutoType.Set(strWindow, string.Empty);
			}

			while(true)
			{
				int nAutoTypeStart = str.IndexOf(AutoTypePrefix, 0,
					StringComparison.OrdinalIgnoreCase);
				if(nAutoTypeStart < 0) break;

				int nAutoTypeEnd = str.IndexOf('\n', nAutoTypeStart);
				if(nAutoTypeEnd < 0) nAutoTypeEnd = str.Length - 1;

				string strAutoType = str.Substring(nAutoTypeStart + AutoTypePrefix.Length,
					nAutoTypeEnd - nAutoTypeStart - AutoTypePrefix.Length + 1);
				strAutoType = strAutoType.Trim(vTrim);

				str = str.Remove(nAutoTypeStart, nAutoTypeEnd - nAutoTypeStart + 1);

				int nWindowStart = str.IndexOf(AutoTypeWindowPrefix, 0,
					StringComparison.OrdinalIgnoreCase);

				if((nWindowStart < 0) || (nWindowStart >= nAutoTypeStart))
				{
					peStorage.AutoType.DefaultSequence = strAutoType;
					continue;
				}

				int nWindowEnd = str.IndexOf('\n', nWindowStart);
				if(nWindowEnd < 0) nWindowEnd = str.Length - 1;

				string strWindow = str.Substring(nWindowStart + AutoTypeWindowPrefix.Length,
					nWindowEnd - nWindowStart - AutoTypeWindowPrefix.Length + 1);
				strWindow = strWindow.Trim(vTrim);

				str = str.Remove(nWindowStart, nWindowEnd - nWindowStart + 1);
				peStorage.AutoType.Set(strWindow, strAutoType);
			}

			strNotes = str;
		} */

		private static void ImportAutoType(ref string strNotes, PwEntry peStorage)
		{
			if(string.IsNullOrEmpty(strNotes)) return;

			string str = strNotes.Replace("\r", string.Empty);
			string[] vLines = str.Split('\n');

			string strOvr = FindPrefixedLine(vLines, AutoTypePrefix + ":");
			if((strOvr != null) && (strOvr.Length > (AutoTypePrefix.Length + 1)))
			{
				strOvr = strOvr.Substring(AutoTypePrefix.Length + 1).Trim();
				peStorage.AutoType.DefaultSequence = strOvr;
			}

			StringBuilder sb = new StringBuilder();
			foreach(string strLine in vLines)
			{
				bool bProcessed = false;
				for(int iIdx = 0; iIdx < 32; ++iIdx)
				{
					string s = ((iIdx == 0) ? string.Empty : "-" + iIdx.ToString());
					string strWndPrefix = (AutoTypeWindowPrefix + s + ":");
					string strSeqPrefix = (AutoTypePrefix + s + ":");

					if(strLine.StartsWith(strWndPrefix, StrCaseIgnoreCmp) &&
						(strLine.Length > strWndPrefix.Length))
					{
						string strWindow = strLine.Substring(strWndPrefix.Length).Trim();
						string strSeq = FindPrefixedLine(vLines, strSeqPrefix);
						if((strSeq != null) && (strSeq.Length > strSeqPrefix.Length))
						{
							peStorage.AutoType.Set(strWindow, strSeq.Substring(
								strSeqPrefix.Length).Trim());
							bProcessed = true;
							break;
						}
					}
					else if(strLine.StartsWith(strSeqPrefix, StrCaseIgnoreCmp))
					{
						bProcessed = true;
						break;
					}
				}

				if(bProcessed == false)
				{
					sb.Append(strLine);
					sb.Append(MessageService.NewLine);
				}
			}

			strNotes = sb.ToString();
		}

		private static string FindPrefixedLine(string[] vLines, string strPrefix)
		{
			foreach(string str in vLines)
			{
				if(str.StartsWith(strPrefix, StrCaseIgnoreCmp))
					return str;
			}

			return null;
		}

		private static void ExportAutoType(PwEntry peSource, ref string strNotes)
		{
			StringBuilder sbAppend = new StringBuilder();
			bool bSeparator = false;

			if(peSource.AutoType.DefaultSequence.Length > 0)
			{
				sbAppend.Append(MessageService.NewParagraph);
				sbAppend.Append(AutoTypePrefix);
				sbAppend.Append(@": ");
				sbAppend.Append(peSource.AutoType.DefaultSequence);
				sbAppend.Append(MessageService.NewLine);

				bSeparator = true;
			}

			foreach(KeyValuePair<string, string> kvp in peSource.AutoType.WindowSequencePairs)
			{
				if(bSeparator == false)
				{
					sbAppend.Append(MessageService.NewParagraph);

					bSeparator = true;
				}

				sbAppend.Append(AutoTypeWindowPrefix);
				sbAppend.Append(@": ");
				sbAppend.Append(kvp.Key);
				sbAppend.Append(MessageService.NewLine);
				sbAppend.Append(AutoTypePrefix);
				sbAppend.Append(@": ");
				sbAppend.Append(kvp.Value);
				sbAppend.Append(MessageService.NewLine);
			}

			strNotes = strNotes.TrimEnd(new char[]{ '\r', '\n', '\t', ' ' });
			strNotes += sbAppend.ToString();
		}

		private static void ImportUrlOverride(ref string strNotes, PwEntry peStorage)
		{
			string str = strNotes;
			char[] vTrim = new char[] { '\r', '\n', '\t', ' ' };

			int nUrlStart = str.IndexOf(UrlOverridePrefix, 0,
				StringComparison.OrdinalIgnoreCase);
			if(nUrlStart < 0) return;

			int nUrlEnd = str.IndexOf('\n', nUrlStart);
			if(nUrlEnd < 0) nUrlEnd = str.Length - 1;

			string strUrl = str.Substring(nUrlStart + UrlOverridePrefix.Length,
				nUrlEnd - nUrlStart - UrlOverridePrefix.Length + 1);
			strUrl = strUrl.Trim(vTrim);

			peStorage.OverrideUrl = strUrl;

			str = str.Remove(nUrlStart, nUrlEnd - nUrlStart + 1);

			strNotes = str;
		}

		private static void ExportUrlOverride(PwEntry peSource, ref string strNotes)
		{
			if(peSource.OverrideUrl.Length > 0)
			{
				StringBuilder sbAppend = new StringBuilder();

				sbAppend.Append(MessageService.NewParagraph);
				sbAppend.Append(UrlOverridePrefix);
				sbAppend.Append(@" ");
				sbAppend.Append(peSource.OverrideUrl);
				sbAppend.Append(MessageService.NewLine);

				strNotes = strNotes.TrimEnd(new char[] { '\r', '\n', '\t', ' ' });
				strNotes += sbAppend.ToString();
			}
		}
	}
}
