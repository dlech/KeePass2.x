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
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

using KeePass.Forms;
using KeePass.Resources;
using KeePass.Util.Spr;

using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Cryptography.PasswordGenerator;
using KeePassLib.Delegates;
using KeePassLib.Security;
using KeePassLib.Utility;
using KeePassLib.Serialization;

namespace KeePass.Util
{
	/// <summary>
	/// This class contains various static functions for entry operations.
	/// </summary>
	public static class EntryUtil
	{
		/// <summary>
		/// Save all attachments of an array of entries to a directory.
		/// </summary>
		/// <param name="vEntries">Array of entries whose attachments are extracted and saved.</param>
		/// <param name="strBasePath">Directory in which the attachments are stored.</param>
		public static void SaveEntryAttachments(PwEntry[] vEntries, string strBasePath)
		{
			Debug.Assert(vEntries != null); if(vEntries == null) return;
			Debug.Assert(strBasePath != null); if(strBasePath == null) return;

			string strPath = UrlUtil.EnsureTerminatingSeparator(strBasePath, false);
			bool bCancel = false;

			foreach(PwEntry pe in vEntries)
			{
				foreach(KeyValuePair<string, ProtectedBinary> kvp in pe.Binaries)
				{
					string strFile = strPath + kvp.Key;

					if(File.Exists(strFile))
					{
						string strMsg = KPRes.FileExistsAlready + MessageService.NewLine;
						strMsg += strFile + MessageService.NewParagraph;
						strMsg += KPRes.OverwriteExistingFileQuestion;

						DialogResult dr = MessageService.Ask(strMsg, null,
							MessageBoxButtons.YesNoCancel);

						if(dr == DialogResult.Cancel)
						{
							bCancel = true;
							break;
						}
						else if(dr == DialogResult.Yes)
						{
							try { File.Delete(strFile); }
							catch(Exception exDel)
							{
								MessageService.ShowWarning(strFile, exDel);
								continue;
							}
						}
						else continue; // DialogResult.No
					}

					try { File.WriteAllBytes(strFile, kvp.Value.ReadData()); }
					catch(Exception exWrite)
					{
						MessageService.ShowWarning(strFile, exWrite);
					}
				}
				if(bCancel) break;
			}
		}

		public const string ClipFormatEntries = "KeePassEntries";
		private static byte[] AdditionalEntropy = { 0xF8, 0x03, 0xFA, 0x51, 0x87, 0x18, 0x49, 0x5C };

		public static void CopyEntriesToClipboard(PwDatabase pwDatabase, PwEntry[] vEntries)
		{
			MemoryStream ms = new MemoryStream();
			GZipStream gz = new GZipStream(ms, CompressionMode.Compress);
			Kdb4File.WriteEntries(gz, pwDatabase, vEntries);

			byte[] pbFinal = ProtectedData.Protect(ms.ToArray(), AdditionalEntropy, DataProtectionScope.CurrentUser);

			ClipboardUtil.Copy(pbFinal, ClipFormatEntries, true);

			gz.Close(); ms.Close();
		}

		public static void PasteEntriesFromClipboard(PwDatabase pwDatabase, PwGroup pgStorage)
		{
			if(Clipboard.ContainsData(ClipFormatEntries) == false) return;

			byte[] pbEnc = (byte[])Clipboard.GetData(ClipFormatEntries);

			byte[] pbPlain = ProtectedData.Unprotect(pbEnc, AdditionalEntropy, DataProtectionScope.CurrentUser);
			MemoryStream ms = new MemoryStream(pbPlain, false);
			GZipStream gz = new GZipStream(ms, CompressionMode.Decompress);

			List<PwEntry> vEntries = Kdb4File.ReadEntries(pwDatabase, gz);

			// Adjust protection settings and add entries
			foreach(PwEntry pe in vEntries)
			{
				ProtectedString ps = pe.Strings.Get(PwDefs.TitleField);
				if(ps != null) ps.EnableProtection(pwDatabase.MemoryProtection.ProtectTitle);

				ps = pe.Strings.Get(PwDefs.UserNameField);
				if(ps != null) ps.EnableProtection(pwDatabase.MemoryProtection.ProtectUserName);

				ps = pe.Strings.Get(PwDefs.PasswordField);
				if(ps != null) ps.EnableProtection(pwDatabase.MemoryProtection.ProtectPassword);

				ps = pe.Strings.Get(PwDefs.UrlField);
				if(ps != null) ps.EnableProtection(pwDatabase.MemoryProtection.ProtectUrl);

				ps = pe.Strings.Get(PwDefs.NotesField);
				if(ps != null) ps.EnableProtection(pwDatabase.MemoryProtection.ProtectNotes);

				pgStorage.AddEntry(pe, true, true);
			}

			gz.Close(); ms.Close();
		}

		public static string FillPlaceholders(string strText, PwEntry pe,
			PwDatabase pd, SprContentFlags cf)
		{
			if(pe == null) return strText;

			string str = strText;

			str = ReplacePickPw(str, pe, pd, cf);

			return str;
		}

		public static string FillPlaceholdersFinal(string strText, PwEntry pe,
			PwDatabase pd, SprContentFlags cf)
		{
			if(pe == null) return strText;

			string str = strText;

			str = ReplaceNewPasswordPlaceholder(str, pe, pd, cf);

			return str;
		}

		/* private static string ReplacePickPw(string strText, PwEntry pe,
			SprContentFlags cf)
		{
			string str = strText;

			for(int iID = 1; iID < (int.MaxValue - 1); ++iID)
			{
				string strPlaceholder = @"{PICKPASSWORDCHARS";
				if(iID > 1) strPlaceholder += iID.ToString();
				strPlaceholder += @"}";

				if(str.IndexOf(strPlaceholder, StrUtil.CaseIgnoreCmp) >= 0)
				{
					ProtectedString ps = pe.Strings.Get(PwDefs.PasswordField);
					if(ps != null)
					{
						byte[] pb = ps.ReadUtf8();
						bool bNotEmpty = (pb.Length > 0);
						Array.Clear(pb, 0, pb.Length);

						if(bNotEmpty)
						{
							CharPickerForm cpf = new CharPickerForm();
							cpf.InitEx(ps, true, true, 0);

							if(cpf.ShowDialog() == DialogResult.OK)
								str = StrUtil.ReplaceCaseInsensitive(str, strPlaceholder,
									SprEngine.TransformContent(cpf.SelectedCharacters.ReadString(), cf));
						}
					}

					str = StrUtil.ReplaceCaseInsensitive(str, strPlaceholder, string.Empty);
				}
				else break;
			}

			return str;
		} */

		private static string ReplacePickPw(string strText, PwEntry pe,
			PwDatabase pd, SprContentFlags cf)
		{
			string str = strText;

			while(true)
			{
				const string strStart = @"{PICKPASSWORDCHARS";

				int iStart = str.IndexOf(strStart, StrUtil.CaseIgnoreCmp);
				if(iStart < 0) break;

				int iEnd = str.IndexOf('}', iStart);
				if(iEnd < 0) break;

				string strPlaceholder = str.Substring(iStart, iEnd - iStart + 1);

				string strParam = str.Substring(iStart + strStart.Length,
					iEnd - (iStart + strStart.Length));
				string[] vParams = strParam.Split(new char[] { ':' });

				uint uCharCount = 0;
				if(vParams.Length >= 2) uint.TryParse(vParams[1], out uCharCount);

				str = ReplacePickPlaceholder(str, strPlaceholder, pe, pd, cf, uCharCount);
			}

			return str;
		}

		private static string ReplacePickPlaceholder(string str,
			string strPlaceholder, PwEntry pe, PwDatabase pd, SprContentFlags cf,
			uint uCharCount)
		{
			if(str.IndexOf(strPlaceholder, StrUtil.CaseIgnoreCmp) < 0) return str;

			ProtectedString ps = pe.Strings.Get(PwDefs.PasswordField);
			if(ps != null)
			{
				string strPassword = ps.ReadString();
				string strPick = SprEngine.Compile(strPassword, false, pe, pd,
					(cf != null) ? cf.EncodeAsAutoTypeSequence : false,
					(cf != null) ? cf.EncodeQuotesForCommandLine : false);

				if(!string.IsNullOrEmpty(strPick))
				{
					ProtectedString psPick = new ProtectedString(false, strPick);
					CharPickerForm dlg = new CharPickerForm();
					dlg.InitEx(psPick, true, true, uCharCount);

					if(dlg.ShowDialog() == DialogResult.OK)
						str = StrUtil.ReplaceCaseInsensitive(str, strPlaceholder,
							SprEngine.TransformContent(dlg.SelectedCharacters.ReadString(), cf));
				}
			}

			return StrUtil.ReplaceCaseInsensitive(str, strPlaceholder, string.Empty);
		}

		private static string ReplaceNewPasswordPlaceholder(string strText,
			PwEntry pe, PwDatabase pd, SprContentFlags cf)
		{
			if((pe == null) || (pd == null)) return strText;

			string str = strText;

			const string strNewPwPlh = @"{NEWPASSWORD}";
			if(str.IndexOf(strNewPwPlh, StrUtil.CaseIgnoreCmp) >= 0)
			{
				ProtectedString psAutoGen = new ProtectedString(
					pd.MemoryProtection.ProtectPassword);
				PwgError e = PwGenerator.Generate(psAutoGen,
					Program.Config.PasswordGenerator.AutoGeneratedPasswordsProfile,
					null, Program.PwGeneratorPool);

				if(e == PwgError.Success)
				{
					pe.CreateBackup();
					pe.Strings.Set(PwDefs.PasswordField, psAutoGen);
					pd.Modified = true;

					str = StrUtil.ReplaceCaseInsensitive(str, strNewPwPlh,
						psAutoGen.ReadString());
				}
			}

			return str;
		}

		public static bool EntriesHaveSameParent(PwObjectList<PwEntry> v)
		{
			if(v == null) { Debug.Assert(false); return true; }
			if(v.UCount == 0) return true;

			PwGroup pg = v.GetAt(0).ParentGroup;
			foreach(PwEntry pe in v)
			{
				if(pe.ParentGroup != pg) return false;
			}

			return true;
		}

		public static void ReorderEntriesAsInDatabase(PwObjectList<PwEntry> v,
			PwDatabase pd)
		{
			if((v == null) || (pd == null)) { Debug.Assert(false); return; }

			PwObjectList<PwEntry> vRem = v.CloneShallow();
			v.Clear();

			EntryHandler eh = delegate(PwEntry pe)
			{
				int p = vRem.IndexOf(pe);
				if(p >= 0)
				{
					v.Add(pe);
					vRem.RemoveAt((uint)p);
				}

				return true;
			};

			pd.RootGroup.TraverseTree(TraversalMethod.PreOrder, null, eh);

			foreach(PwEntry peRem in vRem) v.Add(peRem); // Entries not found
		}

		public static void ExpireTanEntry(PwEntry pe)
		{
			if(pe == null) throw new ArgumentNullException("pe");
			Debug.Assert(PwDefs.IsTanEntry(pe));

			if(Program.Config.Defaults.TanExpiresOnUse)
			{
				pe.ExpiryTime = DateTime.Now;
				pe.Expires = true;
				pe.Touch(true);
			}
		}
	}
}
