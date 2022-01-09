/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2022 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

using KeePass.App;
using KeePass.DataExchange;
using KeePass.Forms;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util.Spr;

using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Cryptography;
using KeePassLib.Cryptography.PasswordGenerator;
using KeePassLib.Delegates;
using KeePassLib.Interfaces;
using KeePassLib.Security;
using KeePassLib.Serialization;
using KeePassLib.Utility;

namespace KeePass.Util
{
	internal delegate List<object> EntryReportDelegate(PwDatabase pd,
		IStatusLogger sl, out Action<ListView> fInit);

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
			if(vEntries == null) { Debug.Assert(false); return; }
			if(string.IsNullOrEmpty(strBasePath)) { Debug.Assert(false); return; }

			string strPath = UrlUtil.EnsureTerminatingSeparator(strBasePath, false);
			bool bCancel = false;

			foreach(PwEntry pe in vEntries)
			{
				foreach(KeyValuePair<string, ProtectedBinary> kvp in pe.Binaries)
				{
					string strFile = strPath + UrlUtil.GetSafeFileName(kvp.Key);

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

					ProtectedBinary pb = kvp.Value;
					byte[] pbData = pb.ReadData();
					try { File.WriteAllBytes(strFile, pbData); }
					catch(Exception exWrite)
					{
						MessageService.ShowWarning(strFile, exWrite);
					}
					if(pb.IsProtected) MemUtil.ZeroByteArray(pbData);
				}
				if(bCancel) break;
			}
		}

		internal const string ClipFormatGroup = "Group-F"; // F = flags
		// Old format name (<= 2.14): "KeePassEntriesCF",
		// old format name (<= 2.22): "KeePassEntriesCX",
		// old format name (<= 2.40): "KeePassEntries",
		// old format name (<= 2.41): media type "vnd" + "Entries-E" // E = encrypted
		public static readonly string ClipFormatEntries = "Entries-F"; // F = flags
		private static readonly byte[] ClipDomainSep = new byte[] {
			0xF8, 0x03, 0xFA, 0x51, 0x87, 0x18, 0x49, 0x5D };

		private static void CopyDataToClipboard(byte[] pbData, string strFormat,
			IntPtr hOwner, bool bEncrypt)
		{
			if(bEncrypt)
				pbData = CryptoUtil.ProtectData(pbData, ClipDomainSep,
					DataProtectionScope.CurrentUser);

			byte[] pbC = new byte[4 + pbData.Length];
			MemUtil.UInt32ToBytesEx((bEncrypt ? 1U : 0U), pbC, 0); // Flags
			Array.Copy(pbData, 0, pbC, 4, pbData.Length);

			ClipboardUtil.Copy(pbC, strFormat, true, hOwner);
		}

		internal static void CopyGroupToClipboard(PwDatabase pd, PwGroup pg,
			IntPtr hOwner, bool bEncrypt)
		{
			using(MemoryStream ms = new MemoryStream())
			{
				using(GZipStream gz = new GZipStream(ms, CompressionMode.Compress))
				{
					KdbxFile.WriteGroup(gz, pd, pg);
				}

				CopyDataToClipboard(ms.ToArray(), ClipFormatGroup, hOwner, bEncrypt);
			}
		}

		[Obsolete]
		public static void CopyEntriesToClipboard(PwDatabase pd, PwEntry[] vEntries)
		{
			CopyEntriesToClipboard(pd, vEntries, IntPtr.Zero, true);
		}

		public static void CopyEntriesToClipboard(PwDatabase pd, PwEntry[] vEntries,
			IntPtr hOwner)
		{
			CopyEntriesToClipboard(pd, vEntries, hOwner, true);
		}

		internal static void CopyEntriesToClipboard(PwDatabase pd, PwEntry[] vEntries,
			IntPtr hOwner, bool bEncrypt)
		{
			using(MemoryStream ms = new MemoryStream())
			{
				using(GZipStream gz = new GZipStream(ms, CompressionMode.Compress))
				{
					KdbxFile.WriteEntries(gz, pd, vEntries);
				}

				CopyDataToClipboard(ms.ToArray(), ClipFormatEntries, hOwner, bEncrypt);
			}
		}

		private static byte[] GetClipboardData(string strFormat)
		{
			byte[] pbC = ClipboardUtil.GetData(strFormat);
			if(pbC == null) return null;
			if(pbC.Length < 4) { Debug.Assert(false); return null; }

			uint uFlags = MemUtil.BytesToUInt32(pbC, 0);
			byte[] pbData = MemUtil.Mid(pbC, 4, pbC.Length - 4);

			if((uFlags & 1) != 0)
				pbData = CryptoUtil.UnprotectData(pbData, ClipDomainSep,
					DataProtectionScope.CurrentUser);

			return pbData;
		}

		internal static PwGroup PasteGroupFromClipboard(PwDatabase pd,
			PwGroup pgStorage)
		{
			byte[] pbData = GetClipboardData(ClipFormatGroup);
			if(pbData == null) return null;

			PwGroup pg;
			using(MemoryStream ms = new MemoryStream(pbData, false))
			{
				using(GZipStream gz = new GZipStream(ms, CompressionMode.Decompress))
				{
					pg = KdbxFile.ReadGroup(gz, pd, true, true, true);
					pgStorage.AddGroup(pg, true, true);
				}
			}

			return pg;
		}

		public static void PasteEntriesFromClipboard(PwDatabase pd, PwGroup pgStorage)
		{
			PwObjectList<PwEntry> l;
			PasteEntriesFromClipboard(pd, pgStorage, out l);
		}

		internal static void PasteEntriesFromClipboard(PwDatabase pd,
			PwGroup pgStorage, out PwObjectList<PwEntry> lAdded)
		{
			lAdded = new PwObjectList<PwEntry>();

			byte[] pbData = GetClipboardData(ClipFormatEntries);
			if(pbData == null) return;

			using(MemoryStream ms = new MemoryStream(pbData, false))
			{
				using(GZipStream gz = new GZipStream(ms, CompressionMode.Decompress))
				{
					List<PwEntry> lEntries = KdbxFile.ReadEntries(gz, pd, true);
					foreach(PwEntry pe in lEntries)
					{
						pgStorage.AddEntry(pe, true, true);
						lAdded.Add(pe);
					}
				}
			}
		}

		[Obsolete]
		public static string FillPlaceholders(string strText, SprContext ctx)
		{
			return FillPlaceholders(strText, ctx, 0);
		}

		public static string FillPlaceholders(string strText, SprContext ctx,
			uint uRecursionLevel)
		{
			if((ctx == null) || (ctx.Entry == null)) return strText;

			string str = strText;

			if((ctx.Flags & SprCompileFlags.NewPassword) != SprCompileFlags.None)
				str = ReplaceNewPasswordPlaceholder(str, ctx, uRecursionLevel);

			if((ctx.Flags & SprCompileFlags.HmacOtp) != SprCompileFlags.None)
			{
				str = ReplaceHmacOtpPlaceholder(str, ctx);
				str = ReplaceTimeOtpPlaceholder(str, ctx);
			}

			if((ctx.Flags & SprCompileFlags.PickChars) != SprCompileFlags.None)
				str = ReplacePickField(str, ctx);

			return str;
		}

		private static string ReplaceNewPasswordPlaceholder(string strText,
			SprContext ctx, uint uRecursionLevel)
		{
			PwEntry pe = ctx.Entry;
			PwDatabase pd = ctx.Database;
			if((pe == null) || (pd == null)) return strText;

			string str = strText;

			const string strNewPwStart = @"{NEWPASSWORD";
			if(str.IndexOf(strNewPwStart, StrUtil.CaseIgnoreCmp) < 0) return str;

			string strGen = null;

			int iStart;
			List<string> lParams;
			while(SprEngine.ParseAndRemovePlhWithParams(ref str, ctx, uRecursionLevel,
				strNewPwStart + ":", out iStart, out lParams, true))
			{
				if(strGen == null)
					strGen = GeneratePassword((((lParams != null) &&
						(lParams.Count > 0)) ? lParams[0] : string.Empty), ctx);

				string strIns = SprEngine.TransformContent(strGen, ctx);
				str = str.Insert(iStart, strIns);
			}

			const string strNewPwPlh = strNewPwStart + @"}";
			if(str.IndexOf(strNewPwPlh, StrUtil.CaseIgnoreCmp) >= 0)
			{
				if(strGen == null) strGen = GeneratePassword(null, ctx);

				string strIns = SprEngine.TransformContent(strGen, ctx);
				str = StrUtil.ReplaceCaseInsensitive(str, strNewPwPlh, strIns);
			}

			if(strGen != null)
			{
				pe.CreateBackup(pd);

				ProtectedString psGen = new ProtectedString(
					pd.MemoryProtection.ProtectPassword, strGen);
				pe.Strings.Set(PwDefs.PasswordField, psGen);

				pe.Touch(true, false);
				pd.Modified = true;
			}
			else { Debug.Assert(false); }

			return str;
		}

		private static string GeneratePassword(string strProfile, SprContext ctx)
		{
			PwProfile prf = Program.Config.PasswordGenerator.AutoGeneratedPasswordsProfile;
			if(!string.IsNullOrEmpty(strProfile))
			{
				if(strProfile == @"~")
					prf = PwProfile.DeriveFromPassword(ctx.Entry.Strings.GetSafe(
						PwDefs.PasswordField));
				else
				{
					List<PwProfile> lPrf = PwGeneratorUtil.GetAllProfiles(false);
					foreach(PwProfile p in lPrf)
					{
						if(strProfile.Equals(p.Name, StrUtil.CaseIgnoreCmp))
						{
							prf = p;
							break;
						}
					}
				}
			}

			PwEntry peCtx = ((ctx != null) ? ctx.Entry : null);
			PwDatabase pdCtx = ((ctx != null) ? ctx.Database : null);
			ProtectedString ps = PwGeneratorUtil.GenerateAcceptable(
				prf, null, peCtx, pdCtx, false);
			return ps.ReadString();
		}

		// Cf. ImportOtpAuth
		private static byte[] GetOtpSecret(PwEntry pe, string strPrefix)
		{
			try
			{
				string str = pe.Strings.ReadSafe(strPrefix + "Secret");
				if(!string.IsNullOrEmpty(str))
					return StrUtil.Utf8.GetBytes(str);

				str = pe.Strings.ReadSafe(strPrefix + "Secret-Hex");
				if(!string.IsNullOrEmpty(str))
					return MemUtil.HexStringToByteArray(str);

				str = pe.Strings.ReadSafe(strPrefix + "Secret-Base32");
				if(!string.IsNullOrEmpty(str))
				{
					// https://sourceforge.net/p/keepass/discussion/329220/thread/59b61fddea/
					if((str.Length % 8) != 0)
						str = str.PadRight((str.Length & ~7) + 8, '=');
					return MemUtil.ParseBase32(str);
				}

				str = pe.Strings.ReadSafe(strPrefix + "Secret-Base64");
				if(!string.IsNullOrEmpty(str))
					return Convert.FromBase64String(str);
			}
			catch(Exception) { Debug.Assert(false); }

			return null;
		}

		// Cf. ImportOtpAuth
		private static string ReplaceHmacOtpPlaceholder(string strText, SprContext ctx)
		{
			const string strPlh = @"{HMACOTP}";
			if(strText.IndexOf(strPlh, StrUtil.CaseIgnoreCmp) < 0) return strText;

			PwEntry pe = ctx.Entry;
			PwDatabase pd = ctx.Database;
			if((pe == null) || (pd == null)) return strText;

			byte[] pbSecret = GetOtpSecret(pe, "HmacOtp-");

			const string strCounterField = "HmacOtp-Counter";
			string strCounter = pe.Strings.ReadSafe(strCounterField);
			ulong uCounter;
			ulong.TryParse(strCounter, out uCounter);

			string strValue = string.Empty;
			if((pbSecret != null) && (pbSecret.Length != 0))
			{
				strValue = HmacOtp.Generate(pbSecret, uCounter, 6, false, -1);

				pe.Strings.Set(strCounterField, new ProtectedString(false,
					(uCounter + 1).ToString()));
				pe.Touch(true, false);
				pd.Modified = true;
			}

			return StrUtil.ReplaceCaseInsensitive(strText, strPlh, strValue);
		}

		// Cf. ImportOtpAuth
		private static string ReplaceTimeOtpPlaceholder(string strText, SprContext ctx)
		{
			const string strPlh = @"{TIMEOTP}";
			if(strText.IndexOf(strPlh, StrUtil.CaseIgnoreCmp) < 0) return strText;

			PwEntry pe = ctx.Entry;
			if(pe == null) return strText;

			byte[] pbSecret = GetOtpSecret(pe, "TimeOtp-");

			string strPeriod = pe.Strings.ReadSafe("TimeOtp-Period");
			uint uPeriod;
			uint.TryParse(strPeriod, out uPeriod);

			string strLength = pe.Strings.ReadSafe("TimeOtp-Length");
			uint uLength;
			uint.TryParse(strLength, out uLength);
			if(uLength == 0) uLength = 6;

			string strAlg = pe.Strings.ReadSafe("TimeOtp-Algorithm");

			string strValue = string.Empty;
			if((pbSecret != null) && (pbSecret.Length != 0))
				strValue = HmacOtp.GenerateTimeOtp(pbSecret, null, uPeriod,
					uLength, strAlg);

			return StrUtil.ReplaceCaseInsensitive(strText, strPlh, strValue);
		}

		// https://github.com/google/google-authenticator/wiki/Key-Uri-Format
		internal static void ImportOtpAuth(PwEntry pe, string strOtpAuthUri,
			PwDatabase pd)
		{
			if(pe == null) { Debug.Assert(false); return; }
			if(string.IsNullOrEmpty(strOtpAuthUri)) return;

			try
			{
				Uri uri = new Uri(strOtpAuthUri);
				string strScheme = (uri.Scheme ?? string.Empty);
				if(!strScheme.Equals("otpauth", StrUtil.CaseIgnoreCmp))
				{
					Debug.Assert(false);
					return;
				}

				string[] vSeg = (uri.Segments ?? new string[0]);

				string strLabel = string.Empty;
				if(vSeg.Length >= 2)
				{
					strLabel = (vSeg[1] ?? string.Empty);
					if(strLabel.EndsWith("/"))
						strLabel = strLabel.Substring(0, strLabel.Length - 1);
					strLabel = Uri.UnescapeDataString(strLabel).Trim();

					if(!string.IsNullOrEmpty(strLabel))
					{
						string strAccount = strLabel, strIssuer = string.Empty;
						int iSep = strLabel.IndexOf(':');
						if(iSep >= 0)
						{
							strIssuer = strLabel.Substring(0, iSep).Trim();
							strAccount = strLabel.Substring(iSep + 1).Trim();
						}

						ImportUtil.AppendToField(pe, PwDefs.TitleField, strIssuer,
							pd, null, true);
						ImportUtil.AppendToField(pe, PwDefs.UserNameField, strAccount,
							pd, null, true);
					}
				}

				Dictionary<string, string> d = UrlUtil.ParseQuery(uri.Query);

				string strIssuerP;
				d.TryGetValue("issuer", out strIssuerP);
				if(!string.IsNullOrEmpty(strIssuerP) &&
					!strLabel.StartsWith(strIssuerP + ":", StrUtil.CaseIgnoreCmp))
					ImportUtil.AppendToField(pe, PwDefs.TitleField, strIssuerP,
						pd, null, true);

				GAction<string, string, Dictionary<string, string>, bool> f =
					delegate(string strQueryKey, string strEntryField,
					Dictionary<string, string> dValueMap, bool bProtect)
				{
					string strValue;
					d.TryGetValue(strQueryKey, out strValue);
					if(string.IsNullOrEmpty(strValue)) return;

					if(dValueMap != null)
					{
						string strValueM;
						if(dValueMap.TryGetValue(strValue, out strValueM))
							strValue = strValueM;
					}

					pe.Strings.Set(strEntryField, new ProtectedString(
						bProtect, strValue));
				};

				Dictionary<string, string> dAlgMap = new Dictionary<string, string>(
					StrUtil.CaseIgnoreComparer);
				dAlgMap["SHA1"] = "HMAC-SHA-1";
				dAlgMap["SHA256"] = "HMAC-SHA-256";
				dAlgMap["SHA512"] = "HMAC-SHA-512";

				string strType = (uri.Host ?? string.Empty);
				if(strType.Equals("hotp", StrUtil.CaseIgnoreCmp))
				{
					f("secret", "HmacOtp-Secret-Base32", null, true);
					f("counter", "HmacOtp-Counter", null, false);
				}
				else if(strType.Equals("totp", StrUtil.CaseIgnoreCmp))
				{
					f("secret", "TimeOtp-Secret-Base32", null, true);
					f("digits", "TimeOtp-Length", null, false);
					f("period", "TimeOtp-Period", null, false);
					f("algorithm", "TimeOtp-Algorithm", dAlgMap, false);
				}
				else { Debug.Assert(false); }
			}
			catch(Exception) { Debug.Assert(false); }
		}

		private static string ReplacePickField(string strText, SprContext ctx)
		{
			string str = strText;
			PwEntry pe = ((ctx != null) ? ctx.Entry : null);
			PwDatabase pd = ((ctx != null) ? ctx.Database : null);

			while(true)
			{
				const string strPlh = @"{PICKFIELD}";
				int p = str.IndexOf(strPlh, StrUtil.CaseIgnoreCmp);
				if(p < 0) break;

				string strRep = string.Empty;

				List<FpField> l = new List<FpField>();
				string strGroup;

				if(pe != null)
				{
					strGroup = KPRes.Entry + " - " + KPRes.StandardFields;

					Debug.Assert(PwDefs.GetStandardFields().Count == 5);
					l.Add(new FpField(KPRes.Title, pe.Strings.GetSafe(PwDefs.TitleField),
						strGroup));
					l.Add(new FpField(KPRes.UserName, pe.Strings.GetSafe(PwDefs.UserNameField),
						strGroup));
					l.Add(new FpField(KPRes.Password, pe.Strings.GetSafe(PwDefs.PasswordField),
						strGroup));
					l.Add(new FpField(KPRes.Url, pe.Strings.GetSafe(PwDefs.UrlField),
						strGroup));
					l.Add(new FpField(KPRes.Notes, pe.Strings.GetSafe(PwDefs.NotesField),
						strGroup));

					strGroup = KPRes.Entry + " - " + KPRes.CustomFields;
					foreach(KeyValuePair<string, ProtectedString> kvp in pe.Strings)
					{
						if(PwDefs.IsStandardField(kvp.Key)) continue;

						l.Add(new FpField(kvp.Key, kvp.Value, strGroup));
					}

					PwGroup pg = pe.ParentGroup;
					if(pg != null)
					{
						strGroup = KPRes.Group;

						l.Add(new FpField(KPRes.Name, new ProtectedString(
							false, pg.Name), strGroup));

						if(pg.Notes.Length > 0)
							l.Add(new FpField(KPRes.Notes, new ProtectedString(
								false, pg.Notes), strGroup));
					}
				}

				if(pd != null)
				{
					strGroup = KPRes.Database;

					if(pd.Name.Length != 0)
						l.Add(new FpField(KPRes.Name, new ProtectedString(
							false, pd.Name), strGroup));
					l.Add(new FpField(KPRes.FileOrUrl, new ProtectedString(
						false, pd.IOConnectionInfo.Path), strGroup));
				}

				FpField fpf = FieldPickerForm.ShowAndRestore(KPRes.PickField,
					KPRes.PickFieldDesc, l);
				if(fpf != null) strRep = fpf.Value.ReadString();

				strRep = SprEngine.Compile(strRep, ctx.WithoutContentTransformations());
				strRep = SprEngine.TransformContent(strRep, ctx);

				str = str.Remove(p, strPlh.Length);
				str = str.Insert(p, strRep);
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
			if(pd.RootGroup == null) { Debug.Assert(false); return; } // DB must be open

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
			ExpireTanEntryIfOption(pe, null);
		}

		[Obsolete]
		public static bool ExpireTanEntryIfOption(PwEntry pe)
		{
			return ExpireTanEntryIfOption(pe, null);
		}

		/// <summary>
		/// Test whether an entry is a TAN entry and if so, expire it, provided
		/// that the option for expiring TANs on use is enabled.
		/// </summary>
		/// <param name="pe">Entry.</param>
		/// <returns>If the entry has been modified, the return value is
		/// <c>true</c>, otherwise <c>false</c>.</returns>
		public static bool ExpireTanEntryIfOption(PwEntry pe, PwDatabase pdContext)
		{
			if(pe == null) throw new ArgumentNullException("pe");
			// pdContext may be null
			if(!PwDefs.IsTanEntry(pe)) return false; // No assert

			if(Program.Config.Defaults.TanExpiresOnUse)
			{
				pe.ExpiryTime = DateTime.UtcNow;
				pe.Expires = true;
				pe.Touch(true);
				if(pdContext != null) pdContext.Modified = true;
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

		private static int CompareLastMod(PwEntry a, PwEntry b)
		{
			return TimeUtil.CompareLastMod(a, b, true);
		}

		private static int CompareLastModReverse(PwEntry a, PwEntry b)
		{
			return TimeUtil.CompareLastMod(b, a, true); // Descending
		}

		public static DateTime GetLastPasswordModTime(PwEntry pe)
		{
			if(pe == null) { Debug.Assert(false); return TimeUtil.ToUtc(DateTime.Today, false); }

			List<PwEntry> l = new List<PwEntry>(pe.History);
			l.Sort(EntryUtil.CompareLastMod);

			// Decrypt the current password only once
			byte[] pbC = pe.Strings.GetSafe(PwDefs.PasswordField).ReadUtf8();
			DateTime dt = pe.LastModificationTime;

			for(int i = l.Count - 1; i >= 0; --i)
			{
				PwEntry peH = l[i];

				byte[] pbH = peH.Strings.GetSafe(PwDefs.PasswordField).ReadUtf8();
				bool bSame = MemUtil.ArraysEqual(pbH, pbC);
				MemUtil.ZeroByteArray(pbH);

				if(!bSame) break;
				dt = peH.LastModificationTime;
			}

			MemUtil.ZeroByteArray(pbC);
			return dt;
		}

		private static int CompareListSizeDesc(List<PwEntry> x, List<PwEntry> y)
		{
			if(x == null) { Debug.Assert(false); return ((y == null) ? 0 : -1); }
			if(y == null) { Debug.Assert(false); return 1; }

			return y.Count.CompareTo(x.Count); // Descending
		}

		private static string GetPasswordForDisplay(PwEntry pe)
		{
			if(pe == null) { Debug.Assert(false); return string.Empty; }
			if(!AppPolicy.Current.UnhidePasswords) return PwDefs.HiddenPassword;

			return pe.Strings.ReadSafe(PwDefs.PasswordField);
		}

		internal static List<object> FindDuplicatePasswords(PwDatabase pd,
			IStatusLogger sl, out Action<ListView> fInit)
		{
			fInit = delegate(ListView lv)
			{
				int w = lv.ClientSize.Width - UIUtil.GetVScrollBarWidth();
				int wf = w / 4;
				int di = Math.Min(UIUtil.GetSmallIconSize().Width, wf);

				lv.Columns.Add(KPRes.Title, wf + di);
				lv.Columns.Add(KPRes.UserName, wf);
				lv.Columns.Add(KPRes.Password, wf);
				lv.Columns.Add(KPRes.Group, wf - di);

				UIUtil.SetDisplayIndices(lv, new int[] { 1, 2, 3, 0 });
			};

			List<List<PwEntry>> lDups = FindDuplicatePasswordsEx(pd, sl);
			if(lDups == null) return null;

			List<object> lResults = new List<object>();
			DateTime dtNow = DateTime.UtcNow;

			foreach(List<PwEntry> l in lDups)
			{
				PwGroup pg = new PwGroup(true, true);
				pg.IsVirtual = true;

				ListViewGroup lvg = new ListViewGroup(KPRes.DuplicatePasswordsGroup);
				lvg.Tag = pg;
				lResults.Add(lvg);

				foreach(PwEntry pe in l)
				{
					pg.AddEntry(pe, false, false);

					string strGroup = string.Empty;
					if(pe.ParentGroup != null)
						strGroup = pe.ParentGroup.GetFullPath(" - ", false);

					ListViewItem lvi = new ListViewItem(pe.Strings.ReadSafe(
						PwDefs.TitleField));
					lvi.SubItems.Add(pe.Strings.ReadSafe(PwDefs.UserNameField));
					lvi.SubItems.Add(GetPasswordForDisplay(pe));
					lvi.SubItems.Add(strGroup);

					lvi.ImageIndex = UIUtil.GetEntryIconIndex(pd, pe, dtNow);
					lvi.Tag = pe;
					lResults.Add(lvi);
				}
			}

			return lResults;
		}

		private static List<List<PwEntry>> FindDuplicatePasswordsEx(PwDatabase pd,
			IStatusLogger sl)
		{
			if((pd == null) || !pd.IsOpen) { Debug.Assert(false); return null; }
			PwGroup pg = pd.RootGroup;
			if(pg == null) { Debug.Assert(false); return null; }

			uint uEntries = pg.GetEntriesCount(true);
			uint uEntriesDone = 0;
			Dictionary<string, List<PwEntry>> d =
				new Dictionary<string, List<PwEntry>>();

			EntryHandler eh = delegate(PwEntry pe)
			{
				if((sl != null) && (uEntries != 0))
				{
					uint u = (uEntriesDone * 100) / uEntries;
					if(!sl.SetProgress(u)) return false;

					++uEntriesDone;
				}

				if(!pe.GetSearchingEnabled()) return true;

				SprContext ctx = new SprContext(pe, pd, SprCompileFlags.NonActive);
				string str = SprEngine.Compile(pe.Strings.ReadSafe(
					PwDefs.PasswordField), ctx);
				if(str.Length != 0)
				{
					List<PwEntry> l;
					if(d.TryGetValue(str, out l)) l.Add(pe);
					else
					{
						l = new List<PwEntry>();
						l.Add(pe);
						d[str] = l;
					}
				}

				return true;
			};

			if(!pg.TraverseTree(TraversalMethod.PreOrder, null, eh))
				return null;

			List<List<PwEntry>> lRes = new List<List<PwEntry>>();
			foreach(List<PwEntry> l in d.Values)
			{
				if(l.Count <= 1) continue;
				lRes.Add(l);
			}
			lRes.Sort(EntryUtil.CompareListSizeDesc);

			return lRes;
		}

		private sealed class EuSimilarPasswords
		{
			public readonly PwEntry EntryA;
			public readonly PwEntry EntryB;
			public readonly float Similarity;

			public EuSimilarPasswords(PwEntry peA, PwEntry peB, float fSimilarity)
			{
				if(peA == null) throw new ArgumentNullException("peA");
				if(peB == null) throw new ArgumentNullException("peB");

				this.EntryA = peA;
				this.EntryB = peB;
				this.Similarity = fSimilarity;
			}
		}

		internal static List<object> FindSimilarPasswordsP(PwDatabase pd,
			IStatusLogger sl, out Action<ListView> fInit)
		{
			fInit = delegate(ListView lv)
			{
				int w = lv.ClientSize.Width - UIUtil.GetVScrollBarWidth();
				int wf = w / 4;
				int di = Math.Min(UIUtil.GetSmallIconSize().Width, wf);

				lv.Columns.Add(KPRes.Title, wf + di);
				lv.Columns.Add(KPRes.UserName, wf);
				lv.Columns.Add(KPRes.Password, wf);
				lv.Columns.Add(KPRes.Group, wf - di);

				UIUtil.SetDisplayIndices(lv, new int[] { 1, 2, 3, 0 });
			};

			List<EuSimilarPasswords> l = FindSimilarPasswordsPEx(pd, sl);
			if(l == null) return null;

			List<object> lResults = new List<object>();
			DateTime dtNow = DateTime.UtcNow;

			foreach(EuSimilarPasswords sp in l)
			{
				PwGroup pg = new PwGroup(true, true);
				pg.IsVirtual = true;

				float fSim = sp.Similarity * 100.0f;
				string strLvg = KPRes.SimilarPasswordsGroup.Replace(
					@"{PARAM}", fSim.ToString("F02") + @"%");
				ListViewGroup lvg = new ListViewGroup(strLvg);
				lvg.Tag = pg;
				lResults.Add(lvg);

				for(int i = 0; i < 2; ++i)
				{
					PwEntry pe = ((i == 0) ? sp.EntryA : sp.EntryB);
					pg.AddEntry(pe, false, false);

					string strGroup = string.Empty;
					if(pe.ParentGroup != null)
						strGroup = pe.ParentGroup.GetFullPath(" - ", false);

					ListViewItem lvi = new ListViewItem(pe.Strings.ReadSafe(
						PwDefs.TitleField));
					lvi.SubItems.Add(pe.Strings.ReadSafe(PwDefs.UserNameField));
					lvi.SubItems.Add(GetPasswordForDisplay(pe));
					lvi.SubItems.Add(strGroup);

					lvi.ImageIndex = UIUtil.GetEntryIconIndex(pd, pe, dtNow);
					lvi.Tag = pe;
					lResults.Add(lvi);
				}
			}

			return lResults;
		}

		private static bool GetEntryPasswords(PwGroup pg, PwDatabase pd,
			IStatusLogger sl, uint uPrePct, List<PwEntry> lEntries,
			List<string> lPasswords, bool bExclTans)
		{
			uint uEntries = pg.GetEntriesCount(true);
			uint uEntriesDone = 0;

			EntryHandler eh = delegate(PwEntry pe)
			{
				if((sl != null) && (uEntries != 0))
				{
					uint u = (uEntriesDone * uPrePct) / uEntries;
					if(!sl.SetProgress(u)) return false;

					++uEntriesDone;
				}

				if(!pe.GetSearchingEnabled()) return true;
				if(bExclTans && PwDefs.IsTanEntry(pe)) return true;

				SprContext ctx = new SprContext(pe, pd, SprCompileFlags.NonActive);
				string str = SprEngine.Compile(pe.Strings.ReadSafe(
					PwDefs.PasswordField), ctx);
				if(str.Length != 0)
				{
					lEntries.Add(pe);
					lPasswords.Add(str);
				}

				return true;
			};

			return pg.TraverseTree(TraversalMethod.PreOrder, null, eh);
		}

		private static List<EuSimilarPasswords> FindSimilarPasswordsPEx(
			PwDatabase pd, IStatusLogger sl)
		{
			if((pd == null) || !pd.IsOpen) { Debug.Assert(false); return null; }
			PwGroup pg = pd.RootGroup;
			if(pg == null) { Debug.Assert(false); return null; }

			const uint uPrePct = 33;
			const long cPostPct = 67;

			List<PwEntry> lEntries = new List<PwEntry>();
			List<string> lPasswords = new List<string>();
			if(!GetEntryPasswords(pg, pd, sl, uPrePct, lEntries, lPasswords, true))
				return null;

			Debug.Assert(TextSimilarity.LevenshteinDistance("Columns", "Comments") == 4);
			Debug.Assert(TextSimilarity.LevenshteinDistance("File/URL", "Field Value") == 8);

			int n = lEntries.Count;
			long cTotal = ((long)n * (long)(n - 1)) / 2L;
			long cDone = 0;

			List<EuSimilarPasswords> l = new List<EuSimilarPasswords>();
			for(int i = 0; i < (n - 1); ++i)
			{
				string strA = lPasswords[i];
				Debug.Assert(strA.Length != 0);

				for(int j = i + 1; j < n; ++j)
				{
					string strB = lPasswords[j];

					if(strA != strB)
						l.Add(new EuSimilarPasswords(lEntries[i], lEntries[j], 1.0f -
							((float)TextSimilarity.LevenshteinDistance(strA, strB) /
							(float)Math.Max(strA.Length, strB.Length))));

					if(sl != null)
					{
						++cDone;

						uint u = uPrePct + (uint)((cDone * cPostPct) / cTotal);
						if(!sl.SetProgress(u)) return null;
					}
				}
			}
			Debug.Assert((cDone == cTotal) || (sl == null));

			Comparison<EuSimilarPasswords> fCmp = delegate(EuSimilarPasswords x,
				EuSimilarPasswords y)
			{
				return y.Similarity.CompareTo(x.Similarity); // Descending
			};
			l.Sort(fCmp);

			int ciMax = Math.Max(n / 2, 20);
			if(l.Count > ciMax) l.RemoveRange(ciMax, l.Count - ciMax);

			return l;
		}

		/* private const int FspcShowItemsPerCluster = 7;
		internal static List<object> FindSimilarPasswordsC(PwDatabase pd,
			IStatusLogger sl, out Action<ListView> fInit)
		{
			fInit = delegate(ListView lv)
			{
				int w = lv.ClientSize.Width - UIUtil.GetVScrollBarWidth();
				int wf = w / 5;
				int di = Math.Min(UIUtil.GetSmallIconSize().Width, wf);

				lv.Columns.Add(KPRes.Title, wf + di);
				lv.Columns.Add(KPRes.UserName, wf);
				lv.Columns.Add(KPRes.Password, wf);
				lv.Columns.Add(KPRes.Group, wf - di);
				lv.Columns.Add(KPRes.Similarity, wf, HorizontalAlignment.Right);

				UIUtil.SetDisplayIndices(lv, new int[] { 1, 2, 3, 0, 4 });
			};

			List<List<EuSimilarPasswords>> l = FindSimilarPasswordsCEx(pd, sl);
			if(l == null) return null;

			List<object> lResults = new List<object>();
			DateTime dtNow = DateTime.UtcNow;

			foreach(List<EuSimilarPasswords> lSp in l)
			{
				PwGroup pg = new PwGroup(true, true);
				pg.IsVirtual = true;

				ListViewGroup lvg = new ListViewGroup(KPRes.SimilarPasswordsGroupSh);
				lvg.Tag = pg;
				lResults.Add(lvg);

				for(int i = 0; i < lSp.Count; ++i)
				{
					EuSimilarPasswords sp = lSp[i];
					PwEntry pe = sp.EntryB;
					pg.AddEntry(pe, false, false);

					// The group contains all cluster entries (such that they
					// are displayed in the main window when clicking an entry),
					// but the report dialog only shows the first few entries
					if(i >= FspcShowItemsPerCluster) continue;

					string strGroup = string.Empty;
					if(pe.ParentGroup != null)
						strGroup = pe.ParentGroup.GetFullPath(" - ", false);

					ListViewItem lvi = new ListViewItem(pe.Strings.ReadSafe(
						PwDefs.TitleField));
					lvi.SubItems.Add(pe.Strings.ReadSafe(PwDefs.UserNameField));
					lvi.SubItems.Add(pe.Strings.ReadSafe(PwDefs.PasswordField));
					lvi.SubItems.Add(strGroup);

					string strSim = KPRes.ClusterCenter;
					if(i > 0) strSim = (sp.Similarity * 100.0f).ToString("F02") + @"%";
					else { Debug.Assert(sp.Similarity == float.MaxValue); }
					lvi.SubItems.Add(strSim);

					lvi.ImageIndex = UIUtil.GetEntryIconIndex(pd, pe, dtNow);
					lvi.Tag = pe;
					lResults.Add(lvi);
				}
			}

			return lResults;
		} */

		internal static List<object> FindSimilarPasswordsC(PwDatabase pd,
			IStatusLogger sl, out Action<ListView> fInit)
		{
			fInit = delegate(ListView lv)
			{
				int wm = 4;
				int w = lv.ClientSize.Width - UIUtil.GetVScrollBarWidth() - (3 * wm);
				int wf = w / 12;
				int wl = w / 4;
				int wr = w - (3 * wl + 3 * wf);
				int di = Math.Min(UIUtil.GetSmallIconSize().Width, wf);

				lv.Columns.Add(KPRes.Title, wl + di + wr);
				lv.Columns.Add(KPRes.UserName, wl - di);
				lv.Columns.Add(KPRes.Password, wl);
				lv.Columns.Add("> 90%", wf + wm, HorizontalAlignment.Right);
				lv.Columns.Add("> 70%", wf + wm, HorizontalAlignment.Right);
				lv.Columns.Add("> 50%", wf + wm, HorizontalAlignment.Right);
			};

			List<List<EuSimilarPasswords>> l = FindSimilarPasswordsCEx(pd, sl);
			if(l == null) return null;

			List<object> lResults = new List<object>();
			DateTime dtNow = DateTime.UtcNow;

			float[] vSimBounds = new float[] { 0.9f, 0.7f, 0.5f };
			int[] vSimCounts = new int[3];

			foreach(List<EuSimilarPasswords> lSp in l)
			{
				PwGroup pg = new PwGroup(true, true);
				pg.IsVirtual = true;

				PwEntry pe = lSp[0].EntryA; // Cluster center
				pg.AddEntry(pe, false, false);

				ListViewItem lvi = new ListViewItem(pe.Strings.ReadSafe(
					PwDefs.TitleField));
				lvi.SubItems.Add(pe.Strings.ReadSafe(PwDefs.UserNameField));
				lvi.SubItems.Add(GetPasswordForDisplay(pe));

				Array.Clear(vSimCounts, 0, vSimCounts.Length);
				for(int i = 1; i < lSp.Count; ++i)
				{
					EuSimilarPasswords sp = lSp[i];
					pg.AddEntry(sp.EntryB, false, false);

					for(int j = 0; j < vSimCounts.Length; ++j)
					{
						if(sp.Similarity > vSimBounds[j]) ++vSimCounts[j];
					}
				}

				for(int i = 0; i < vSimCounts.Length; ++i)
					lvi.SubItems.Add(vSimCounts[i].ToString());

				lvi.ImageIndex = UIUtil.GetEntryIconIndex(pd, pe, dtNow);
				lvi.Tag = pg;
				lResults.Add(lvi);
			}

			return lResults;
		}

		private static List<List<EuSimilarPasswords>> FindSimilarPasswordsCEx(
			PwDatabase pd, IStatusLogger sl)
		{
			if((pd == null) || !pd.IsOpen) { Debug.Assert(false); return null; }
			PwGroup pg = pd.RootGroup;
			if(pg == null) { Debug.Assert(false); return null; }

			const uint uPrePct = 33;
			const long cPostPct = 67;

			List<PwEntry> lEntries = new List<PwEntry>();
			List<string> lPasswords = new List<string>();
			if(!GetEntryPasswords(pg, pd, sl, uPrePct, lEntries, lPasswords, true))
				return null;

			int n = lEntries.Count;
			long cTotal = ((long)n * (long)(n - 1)) / 2L;
			long cDone = 0;
			float[,] mxSim = new float[n, n];

			for(int i = 0; i < (n - 1); ++i)
			{
				string strA = lPasswords[i];
				Debug.Assert(strA.Length != 0);

				for(int j = i + 1; j < n; ++j)
				{
					string strB = lPasswords[j];

					float s = 0.0f;
					if(strA != strB)
						s = 1.0f - ((float)TextSimilarity.LevenshteinDistance(strA,
							strB) / (float)Math.Max(strA.Length, strB.Length));
					mxSim[i, j] = s;
					mxSim[j, i] = s;

					if(sl != null)
					{
						++cDone;

						uint u = uPrePct + (uint)((cDone * cPostPct) / cTotal);
						if(!sl.SetProgress(u)) return null;
					}
				}
			}
			Debug.Assert((cDone == cTotal) || (sl == null));

			float[] vXSums = new float[n];
			for(int i = 0; i < (n - 1); ++i)
			{
				for(int j = i + 1; j < n; ++j)
				{
					float s = mxSim[i, j];
					if(s != 1.0f)
					{
						float x = s / (1.0f - s);
						vXSums[i] += x;
						vXSums[j] += x;
					}
					else { Debug.Assert(false); }
				}
			}

			List<KeyValuePair<int, float>> lXSums = new List<KeyValuePair<int, float>>();
			for(int i = 0; i < n; ++i)
				lXSums.Add(new KeyValuePair<int, float>(i, vXSums[i]));
			Comparison<KeyValuePair<int, float>> fCmpS = delegate(KeyValuePair<int, float> x,
				KeyValuePair<int, float> y)
			{
				return y.Value.CompareTo(x.Value); // Descending
			};
			lXSums.Sort(fCmpS);

			Comparison<EuSimilarPasswords> fCmpE = delegate(EuSimilarPasswords x,
				EuSimilarPasswords y)
			{
				return y.Similarity.CompareTo(x.Similarity); // Descending
			};

			List<List<EuSimilarPasswords>> l = new List<List<EuSimilarPasswords>>();
			// int cgMax = Math.Min(Math.Max(n / FspcShowItemsPerCluster, 20), n);
			int cgMax = Math.Min(Math.Max(n / 2, 20), n);

			for(int i = 0; i < lXSums.Count; ++i)
			{
				int p = lXSums[i].Key;
				PwEntry pe = lEntries[p];

				List<EuSimilarPasswords> lSim = new List<EuSimilarPasswords>();
				lSim.Add(new EuSimilarPasswords(pe, pe, float.MaxValue));

				for(int j = 0; j < n; ++j)
				{
					float s = mxSim[p, j];
					Debug.Assert((j != p) || (s == 0.0f));

					if(s > 0.5f)
						lSim.Add(new EuSimilarPasswords(pe, lEntries[j], s));
				}

				if(lSim.Count >= 2)
				{
					lSim.Sort(fCmpE);
					l.Add(lSim);

					if(l.Count >= cgMax) break;
				}
			}

			return l;
		}

		internal static List<object> CreatePwQualityList(PwDatabase pd,
			IStatusLogger sl, out Action<ListView> fInit)
		{
			fInit = delegate(ListView lv)
			{
				int w = lv.ClientSize.Width - UIUtil.GetVScrollBarWidth();
				int wf = (int)(((long)w * 2L) / 9L);
				int wq = w - (wf * 4);
				int di = Math.Min(UIUtil.GetSmallIconSize().Width, wf);

				lv.Columns.Add(KPRes.Title, wf + di);
				lv.Columns.Add(KPRes.UserName, wf);
				lv.Columns.Add(KPRes.Password, wf);
				lv.Columns.Add(KPRes.Group, wf - di);
				lv.Columns.Add(KPRes.Quality, wq, HorizontalAlignment.Right);

				UIUtil.SetDisplayIndices(lv, new int[] { 1, 2, 3, 0, 4 });
			};

			List<KeyValuePair<PwEntry, ulong>> l = CreatePwQualityListEx(pd, sl);
			if(l == null) return null;

			List<object> lResults = new List<object>();
			DateTime dtNow = DateTime.UtcNow;

			foreach(KeyValuePair<PwEntry, ulong> kvp in l)
			{
				PwEntry pe = kvp.Key;

				string strGroup = string.Empty;
				if(pe.ParentGroup != null)
					strGroup = pe.ParentGroup.GetFullPath(" - ", false);

				ListViewItem lvi = new ListViewItem(pe.Strings.ReadSafe(
					PwDefs.TitleField));
				lvi.UseItemStyleForSubItems = false;

				lvi.SubItems.Add(pe.Strings.ReadSafe(PwDefs.UserNameField));
				lvi.SubItems.Add(GetPasswordForDisplay(pe));
				lvi.SubItems.Add(strGroup);

				ulong q = (kvp.Value >> 32);
				ListViewItem.ListViewSubItem lvsi = lvi.SubItems.Add(
					KPRes.BitsEx.Replace(@"{PARAM}", q.ToString()));

				try
				{
					float fQ = (float)Math.Min(q, 128UL) / 128.0f;
					lvsi.BackColor = AppDefs.GetQualityColor(fQ, true);
				}
				catch(Exception) { Debug.Assert(false); }

				lvi.ImageIndex = UIUtil.GetEntryIconIndex(pd, pe, dtNow);
				lvi.Tag = pe;
				lResults.Add(lvi);
			}

			return lResults;
		}

		private static List<KeyValuePair<PwEntry, ulong>> CreatePwQualityListEx(
			PwDatabase pd, IStatusLogger sl)
		{
			if((pd == null) || !pd.IsOpen) { Debug.Assert(false); return null; }
			PwGroup pg = pd.RootGroup;
			if(pg == null) { Debug.Assert(false); return null; }

			uint uEntries = pg.GetEntriesCount(true);
			uint uEntriesDone = 0;
			List<KeyValuePair<PwEntry, ulong>> l = new List<KeyValuePair<PwEntry, ulong>>();

			EntryHandler eh = delegate(PwEntry pe)
			{
				if((sl != null) && (uEntries != 0))
				{
					uint u = (uEntriesDone * 100) / uEntries;
					if(!sl.SetProgress(u)) return false;
				}
				++uEntriesDone; // Also used for sorting, see below

				if(!pe.GetSearchingEnabled()) return true;
				if(!pe.QualityCheck) return true;
				if(PwDefs.IsTanEntry(pe)) return true;

				SprContext ctx = new SprContext(pe, pd, SprCompileFlags.NonActive);
				string str = SprEngine.Compile(pe.Strings.ReadSafe(
					PwDefs.PasswordField), ctx);
				if(str.Length != 0)
				{
					uint q = QualityEstimation.EstimatePasswordBits(str.ToCharArray());
					l.Add(new KeyValuePair<PwEntry, ulong>(pe,
						((ulong)q << 32) | (ulong)uEntriesDone));
				}

				return true;
			};

			if(!pg.TraverseTree(TraversalMethod.PreOrder, null, eh))
				return null;

			Comparison<KeyValuePair<PwEntry, ulong>> fCompare = delegate(
				KeyValuePair<PwEntry, ulong> x, KeyValuePair<PwEntry, ulong> y)
			{
				return x.Value.CompareTo(y.Value);
			};
			l.Sort(fCompare);

			return l;
		}

		internal static List<object> FindLargeEntries(PwDatabase pd,
			IStatusLogger sl, out Action<ListView> fInit)
		{
			fInit = delegate(ListView lv)
			{
				int w = lv.ClientSize.Width - UIUtil.GetVScrollBarWidth();
				int wf = w / 5;

				lv.Columns.Add(KPRes.Title, wf * 2);
				lv.Columns.Add(KPRes.UserName, wf);
				lv.Columns.Add(KPRes.Size, wf, HorizontalAlignment.Right);
				lv.Columns.Add(KPRes.Group, wf);

				UIUtil.SetDisplayIndices(lv, new int[] { 1, 2, 3, 0 });
			};

			PwObjectList<PwEntry> lEntries = pd.RootGroup.GetEntries(true);
			List<KeyValuePair<ulong, PwEntry>> l = new List<KeyValuePair<ulong, PwEntry>>();
			foreach(PwEntry pe in lEntries)
			{
				l.Add(new KeyValuePair<ulong, PwEntry>(pe.GetSize(), pe));
			}
			l.Sort(EntryUtil.CompareKvpBySize);

			List<object> lResults = new List<object>();
			DateTime dtNow = DateTime.UtcNow;

			foreach(KeyValuePair<ulong, PwEntry> kvp in l)
			{
				PwEntry pe = kvp.Value;

				string strGroup = string.Empty;
				if(pe.ParentGroup != null)
					strGroup = pe.ParentGroup.GetFullPath(" - ", false);

				ListViewItem lvi = new ListViewItem(pe.Strings.ReadSafe(
					PwDefs.TitleField));
				lvi.SubItems.Add(pe.Strings.ReadSafe(PwDefs.UserNameField));
				lvi.SubItems.Add(StrUtil.FormatDataSizeKB(kvp.Key));
				lvi.SubItems.Add(strGroup);

				lvi.ImageIndex = UIUtil.GetEntryIconIndex(pd, pe, dtNow);
				lvi.Tag = pe;
				lResults.Add(lvi);
			}

			return lResults;
		}

		private static int CompareKvpBySize(KeyValuePair<ulong, PwEntry> a,
			KeyValuePair<ulong, PwEntry> b)
		{
			return b.Key.CompareTo(a.Key); // Descending
		}

		internal static List<object> FindLastModEntries(PwDatabase pd,
			IStatusLogger sl, out Action<ListView> fInit)
		{
			fInit = delegate(ListView lv)
			{
				int w = lv.ClientSize.Width - UIUtil.GetVScrollBarWidth();
				int wfS = (w * 2) / 9, wfL = w / 3;
				int dr = w - wfL - (3 * wfS);
				int ds = DpiUtil.ScaleIntX(12);

				lv.Columns.Add(KPRes.Title, wfL + dr - ds);
				lv.Columns.Add(KPRes.UserName, wfS);
				lv.Columns.Add(KPRes.LastModificationTime, wfS + ds);
				lv.Columns.Add(KPRes.Group, wfS);

				UIUtil.SetDisplayIndices(lv, new int[] { 1, 2, 3, 0 });
			};

			PwObjectList<PwEntry> lEntries = pd.RootGroup.GetEntries(true);
			lEntries.Sort(EntryUtil.CompareLastModReverse);

			List<object> lResults = new List<object>();
			DateTime dtNow = DateTime.UtcNow;

			foreach(PwEntry pe in lEntries)
			{
				string strGroup = string.Empty;
				if(pe.ParentGroup != null)
					strGroup = pe.ParentGroup.GetFullPath(" - ", false);

				ListViewItem lvi = new ListViewItem(pe.Strings.ReadSafe(
					PwDefs.TitleField));
				lvi.SubItems.Add(pe.Strings.ReadSafe(PwDefs.UserNameField));
				lvi.SubItems.Add(TimeUtil.ToDisplayString(pe.LastModificationTime));
				// lvi.SubItems.Add("12/30/2020 02:45 AM"); // Max. width
				lvi.SubItems.Add(strGroup);

				lvi.ImageIndex = UIUtil.GetEntryIconIndex(pd, pe, dtNow);
				lvi.Tag = pe;
				lResults.Add(lvi);
			}

			return lResults;
		}
	}
}
