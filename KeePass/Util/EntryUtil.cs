/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2011 Dominik Reichl <dominik.reichl@t-online.de>

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
using KeePass.UI;
using KeePass.Util.Spr;

using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Cryptography;
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

		// Old format name (<= 2.14): "KeePassEntriesCF"
		public const string ClipFormatEntries = "KeePassEntriesCX";
		private static byte[] AdditionalEntropy = { 0xF8, 0x03, 0xFA, 0x51, 0x87, 0x18, 0x49, 0x5D };

		[Obsolete]
		public static void CopyEntriesToClipboard(PwDatabase pwDatabase, PwEntry[] vEntries)
		{
			CopyEntriesToClipboard(pwDatabase, vEntries, IntPtr.Zero);
		}

		public static void CopyEntriesToClipboard(PwDatabase pwDatabase, PwEntry[] vEntries,
			IntPtr hOwner)
		{
			MemoryStream ms = new MemoryStream();
			GZipStream gz = new GZipStream(ms, CompressionMode.Compress);
			Kdb4File.WriteEntries(gz, vEntries);

			byte[] pbFinal;
			if(WinUtil.IsWindows9x) pbFinal = ms.ToArray();
			else pbFinal = ProtectedData.Protect(ms.ToArray(), AdditionalEntropy,
				DataProtectionScope.CurrentUser);

			ClipboardUtil.Copy(pbFinal, ClipFormatEntries, true, true, hOwner);

			gz.Close(); ms.Close();
		}

		public static void PasteEntriesFromClipboard(PwDatabase pwDatabase,
			PwGroup pgStorage)
		{
			try { PasteEntriesFromClipboardPriv(pwDatabase, pgStorage); }
			catch(Exception) { Debug.Assert(false); }
		}

		private static void PasteEntriesFromClipboardPriv(PwDatabase pwDatabase,
			PwGroup pgStorage)
		{
			if(!Clipboard.ContainsData(ClipFormatEntries)) return;

			byte[] pbEnc = ClipboardUtil.GetEncodedData(ClipFormatEntries, IntPtr.Zero);
			if(pbEnc == null) { Debug.Assert(false); return; }

			byte[] pbPlain;
			if(WinUtil.IsWindows9x) pbPlain = pbEnc;
			else pbPlain = ProtectedData.Unprotect(pbEnc, AdditionalEntropy,
				DataProtectionScope.CurrentUser);

			MemoryStream ms = new MemoryStream(pbPlain, false);
			GZipStream gz = new GZipStream(ms, CompressionMode.Decompress);

			List<PwEntry> vEntries = Kdb4File.ReadEntries(gz);

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

			// Legacy, for backward compatibility only; see PickChars
			str = ReplacePickPw(str, pe, pd, cf);

			return str;
		}

		public static string FillPlaceholdersFinal(string strText, PwEntry pe,
			PwDatabase pd, SprContentFlags cf)
		{
			if(pe == null) return strText;

			string str = strText;

			str = ReplacePickChars(str, pe, pd, cf);
			str = ReplaceNewPasswordPlaceholder(str, pe, pd, cf);
			str = ReplaceHmacOtpPlaceholder(str, pe, pd, cf);

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
							UIUtil.DestroyForm(cpf);
						}
					}

					str = StrUtil.ReplaceCaseInsensitive(str, strPlaceholder, string.Empty);
				}
				else break;
			}

			return str;
		} */

		// Legacy, for backward compatibility only; see PickChars
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
				string[] vParams = strParam.Split(new char[]{ ':' });

				uint uCharCount = 0;
				if(vParams.Length >= 2) uint.TryParse(vParams[1], out uCharCount);

				str = ReplacePickPwPlaceholder(str, strPlaceholder, pe, pd, cf, uCharCount);
			}

			return str;
		}

		private static string ReplacePickPwPlaceholder(string str,
			string strPlaceholder, PwEntry pe, PwDatabase pd, SprContentFlags cf,
			uint uCharCount)
		{
			if(str.IndexOf(strPlaceholder, StrUtil.CaseIgnoreCmp) < 0) return str;

			ProtectedString ps = pe.Strings.Get(PwDefs.PasswordField);
			if(ps != null)
			{
				string strPassword = ps.ReadString();
				string strPick = SprEngine.Compile(strPassword, false, pe, pd,
					false, false); // Do not transform content yet

				if(!string.IsNullOrEmpty(strPick))
				{
					ProtectedString psPick = new ProtectedString(false, strPick);
					CharPickerForm dlg = new CharPickerForm();
					dlg.InitEx(psPick, true, true, uCharCount, null);

					if(dlg.ShowDialog() == DialogResult.OK)
						str = StrUtil.ReplaceCaseInsensitive(str, strPlaceholder,
							SprEngine.TransformContent(dlg.SelectedCharacters.ReadString(), cf));
					UIUtil.DestroyForm(dlg);
				}
			}

			return StrUtil.ReplaceCaseInsensitive(str, strPlaceholder, string.Empty);
		}

		private static string ReplacePickChars(string strText, PwEntry pe,
			PwDatabase pd, SprContentFlags cf)
		{
			string str = strText;

			Dictionary<string, string> dPicked = new Dictionary<string, string>();
			while(true)
			{
				const string strStart = @"{PICKCHARS";

				int iStart = str.IndexOf(strStart, StrUtil.CaseIgnoreCmp);
				if(iStart < 0) break;

				int iEnd = str.IndexOf('}', iStart);
				if(iEnd < 0) break;

				string strPlaceholder = str.Substring(iStart, iEnd - iStart + 1);

				string strParam = str.Substring(iStart + strStart.Length,
					iEnd - (iStart + strStart.Length));

				string strRep = string.Empty;
				bool bEncode = true;

				if(strParam.Length == 0)
					strRep = ShowCharPickDlg(pe.Strings.GetSafe(PwDefs.PasswordField),
						0, null);
				else if(strParam.StartsWith(":"))
				{
					string strParams = strParam.Substring(1);
					string[] vParams = strParams.Split(new char[] { ':' },
						StringSplitOptions.None);

					string strField = string.Empty;
					if(vParams.Length >= 1) strField = (vParams[0] ?? string.Empty).Trim();
					if(strField.Length == 0) strField = PwDefs.PasswordField;

					string strOptions = string.Empty;
					if(vParams.Length >= 2) strOptions = (vParams[1] ?? string.Empty);

					Dictionary<string, string> dOptions = new Dictionary<string, string>();
					string[] vOptions = strOptions.Split(new char[] { ',' },
						StringSplitOptions.RemoveEmptyEntries);
					foreach(string strOption in vOptions)
					{
						string[] vKvp = strOption.Split(new char[] { '=' },
							StringSplitOptions.None);
						if(vKvp.Length != 2) continue;

						dOptions[vKvp[0].Trim().ToLower()] = vKvp[1].Trim();
					}

					string strID = string.Empty;
					if(dOptions.ContainsKey("id")) strID = dOptions["id"].ToLower();

					uint uCharCount = 0;
					if(dOptions.ContainsKey("c"))
						uint.TryParse(dOptions["c"], out uCharCount);
					if(dOptions.ContainsKey("count"))
						uint.TryParse(dOptions["count"], out uCharCount);

					bool? bInitHide = null;
					if(dOptions.ContainsKey("hide"))
						bInitHide = StrUtil.StringToBool(dOptions["hide"]);

					ProtectedString psContent = pe.Strings.GetSafe(strField);
					if(psContent.Length == 0) { } // Leave strRep empty
					else if((strID.Length > 0) && dPicked.ContainsKey(strID))
						strRep = dPicked[strID];
					else
						strRep = ShowCharPickDlg(psContent, uCharCount, bInitHide);

					if(strID.Length > 0) dPicked[strID] = strRep;

					if(dOptions.ContainsKey("conv"))
					{
						int iOffset = 0;
						if(dOptions.ContainsKey("conv-offset"))
							int.TryParse(dOptions["conv-offset"], out iOffset);

						string strConvFmt = string.Empty;
						if(dOptions.ContainsKey("conv-fmt"))
							strConvFmt = dOptions["conv-fmt"];

						string strConv = dOptions["conv"];
						if(strConv.Equals("d", StrUtil.CaseIgnoreCmp))
						{
							strRep = ConvertToDownArrows(strRep, iOffset, strConvFmt);
							bEncode = false;
						}
					}
				}

				str = StrUtil.ReplaceCaseInsensitive(str, strPlaceholder,
					bEncode ? SprEngine.TransformContent(strRep, cf) : strRep);
			}

			return str;
		}

		private static string ShowCharPickDlg(ProtectedString psWord, uint uCharCount,
			bool? bInitHide)
		{
			CharPickerForm cpf = new CharPickerForm();
			cpf.InitEx(psWord, true, true, uCharCount, bInitHide);

			string strResult = string.Empty;
			if(cpf.ShowDialog() == DialogResult.OK)
				strResult = cpf.SelectedCharacters.ReadString();

			UIUtil.DestroyForm(cpf);
			return strResult;
		}

		private static string ConvertToDownArrows(string str, int iOffset,
			string strLayout)
		{
			if(string.IsNullOrEmpty(str)) return string.Empty;

			StringBuilder sb = new StringBuilder();
			for(int i = 0; i < str.Length; ++i)
			{
				// if((sb.Length > 0) && !string.IsNullOrEmpty(strSep)) sb.Append(strSep);

				char ch = str[i];

				int? iDowns = null;
				if(strLayout.Length == 0)
				{
					if((ch >= '0') && (ch <= '9')) iDowns = (int)ch - '0';
					else if((ch >= 'a') && (ch <= 'z')) iDowns = (int)ch - 'a';
					else if((ch >= 'A') && (ch <= 'Z')) iDowns = (int)ch - 'A';
				}
				else if(strLayout.Equals("0a", StrUtil.CaseIgnoreCmp))
				{
					if((ch >= '0') && (ch <= '9')) iDowns = (int)ch - '0';
					else if((ch >= 'a') && (ch <= 'z')) iDowns = (int)ch - 'a' + 10;
					else if((ch >= 'A') && (ch <= 'Z')) iDowns = (int)ch - 'A' + 10;
				}
				else if(strLayout.Equals("a0", StrUtil.CaseIgnoreCmp))
				{
					if((ch >= '0') && (ch <= '9')) iDowns = (int)ch - '0' + 26;
					else if((ch >= 'a') && (ch <= 'z')) iDowns = (int)ch - 'a';
					else if((ch >= 'A') && (ch <= 'Z')) iDowns = (int)ch - 'A';
				}
				else if(strLayout.Equals("1a", StrUtil.CaseIgnoreCmp))
				{
					if((ch >= '1') && (ch <= '9')) iDowns = (int)ch - '1';
					else if(ch == '0') iDowns = 9;
					else if((ch >= 'a') && (ch <= 'z')) iDowns = (int)ch - 'a' + 10;
					else if((ch >= 'A') && (ch <= 'Z')) iDowns = (int)ch - 'A' + 10;
				}
				else if(strLayout.Equals("a1", StrUtil.CaseIgnoreCmp))
				{
					if((ch >= '1') && (ch <= '9')) iDowns = (int)ch - '1' + 26;
					else if(ch == '0') iDowns = 9 + 26;
					else if((ch >= 'a') && (ch <= 'z')) iDowns = (int)ch - 'a';
					else if((ch >= 'A') && (ch <= 'Z')) iDowns = (int)ch - 'A';
				}

				if(!iDowns.HasValue) continue;

				for(int j = 0; j < (iOffset + iDowns); ++j) sb.Append(@"{DOWN}");
			}

			return sb.ToString();
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
					pe.CreateBackup(pd);
					pe.Strings.Set(PwDefs.PasswordField, psAutoGen);
					pd.Modified = true;

					string strIns = SprEngine.TransformContent(psAutoGen.ReadString(), cf);
					str = StrUtil.ReplaceCaseInsensitive(str, strNewPwPlh, strIns);
				}
			}

			return str;
		}

		private static string ReplaceHmacOtpPlaceholder(string strText,
			PwEntry pe, PwDatabase pd, SprContentFlags cf)
		{
			if((pe == null) || (pd == null)) return strText;

			string str = strText;

			const string strHmacOtpPlh = @"{HMACOTP}";
			if(str.IndexOf(strHmacOtpPlh, StrUtil.CaseIgnoreCmp) >= 0)
			{
				const string strKeyField = "HmacOtp-Secret";
				const string strCounterField = "HmacOtp-Counter";

				byte[] pbSecret = StrUtil.Utf8.GetBytes(pe.Strings.ReadSafe(
					strKeyField));

				string strCounter = pe.Strings.ReadSafe(strCounterField);
				ulong uCounter;
				ulong.TryParse(strCounter, out uCounter);

				string strValue = HmacOtp.Generate(pbSecret, uCounter, 6, false, -1);

				pe.Strings.Set(strCounterField, new ProtectedString(false,
					(uCounter + 1).ToString()));
				pd.Modified = true;

				str = StrUtil.ReplaceCaseInsensitive(str, strHmacOtpPlh, strValue);
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

		[Obsolete]
		public static void ExpireTanEntry(PwEntry pe)
		{
			ExpireTanEntryIfOption(pe);
		}

		/// <summary>
		/// Test whether an entry is a TAN entry and if so, expire it, provided
		/// that the option for expiring TANs on use is enabled.
		/// </summary>
		/// <param name="pe">Entry.</param>
		/// <returns>If the entry has been modified, the return value is
		/// <c>true</c>, otherwise <c>false</c>.</returns>
		public static bool ExpireTanEntryIfOption(PwEntry pe)
		{
			if(pe == null) throw new ArgumentNullException("pe");
			if(!PwDefs.IsTanEntry(pe)) return false; // No assert

			if(Program.Config.Defaults.TanExpiresOnUse)
			{
				pe.ExpiryTime = DateTime.Now;
				pe.Expires = true;
				pe.Touch(true);
				return true;
			}

			return false;
		}

		public static string CreateSummaryList(PwGroup pgItems, bool bStartWithNewPar)
		{
			List<PwEntry> l = pgItems.GetEntries(true).CloneShallowToList();
			string str = CreateSummaryList(pgItems, l.ToArray());

			if((str.Length == 0) || !bStartWithNewPar) return str;
			return (MessageService.NewParagraph + str);
		}

		public static string CreateSummaryList(PwGroup pgSubGroups, PwEntry[] vEntries)
		{
			int nMaxEntries = 10;
			string strSummary = string.Empty;

			if(pgSubGroups != null)
			{
				PwObjectList<PwGroup> vGroups = pgSubGroups.GetGroups(true);
				if(vGroups.UCount > 0)
				{
					StringBuilder sbGroups = new StringBuilder();
					sbGroups.Append("- ");
					uint uToList = Math.Min(3U, vGroups.UCount);
					for(uint u = 0; u < uToList; ++u)
					{
						if(sbGroups.Length > 2) sbGroups.Append(", ");
						sbGroups.Append(vGroups.GetAt(u).Name);
					}
					if(uToList < vGroups.UCount) sbGroups.Append(", ...");
					strSummary += sbGroups.ToString(); // New line below

					nMaxEntries -= 2;
				}
			}

			int nSummaryShow = Math.Min(nMaxEntries, vEntries.Length);
			if(nSummaryShow == (vEntries.Length - 1)) --nSummaryShow; // Plural msg

			for(int iSumEnum = 0; iSumEnum < nSummaryShow; ++iSumEnum)
			{
				if(strSummary.Length > 0) strSummary += MessageService.NewLine;

				PwEntry pe = vEntries[iSumEnum];
				strSummary += ("- " + StrUtil.CompactString3Dots(
					pe.Strings.ReadSafe(PwDefs.TitleField), 39));
				if(PwDefs.IsTanEntry(pe))
				{
					string strTanIdx = pe.Strings.ReadSafe(PwDefs.UserNameField);
					if(!string.IsNullOrEmpty(strTanIdx))
						strSummary += (@" (#" + strTanIdx + @")");
				}
			}
			if(nSummaryShow != vEntries.Length)
				strSummary += (MessageService.NewLine + "- " +
					KPRes.MoreEntries.Replace(@"{PARAM}", (vEntries.Length -
					nSummaryShow).ToString()));

			return strSummary;
		}
	}
}
