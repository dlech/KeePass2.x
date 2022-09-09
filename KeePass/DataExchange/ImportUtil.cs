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
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using KeePass.App;
using KeePass.DataExchange.Formats;
using KeePass.Ecas;
using KeePass.Forms;
using KeePass.Native;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Interfaces;
using KeePassLib.Keys;
using KeePassLib.Resources;
using KeePassLib.Security;
using KeePassLib.Serialization;
using KeePassLib.Utility;

namespace KeePass.DataExchange
{
	public static class ImportUtil
	{
		public static bool? Import(PwDatabase pwStorage, out bool bAppendedToRootOnly,
			Form fParent)
		{
			bAppendedToRootOnly = false;

			if(pwStorage == null) throw new ArgumentNullException("pwStorage");
			if(!pwStorage.IsOpen) { Debug.Assert(false); return null; }
			if(!AppPolicy.Try(AppPolicyId.Import)) return null;

			ExchangeDataForm dlg = new ExchangeDataForm();
			dlg.InitEx(false, pwStorage, pwStorage.RootGroup);

			if(UIUtil.ShowDialogNotValue(dlg, DialogResult.OK)) return null;

			FileFormatProvider ffp = dlg.ResultFormat;
			if(ffp == null)
			{
				Debug.Assert(false);
				MessageService.ShowWarning(KPRes.ImportFailed);
				UIUtil.DestroyForm(dlg);
				return null;
			}

			bAppendedToRootOnly = ffp.ImportAppendsToRootGroupOnly;

			List<IOConnectionInfo> lConnections = new List<IOConnectionInfo>();
			foreach(string strFile in dlg.ResultFiles)
				lConnections.Add(IOConnectionInfo.FromPath(strFile));

			UIUtil.DestroyForm(dlg);
			return Import(pwStorage, ffp, lConnections.ToArray(), false, null,
				false, fParent);
		}

		public static bool? Import(PwDatabase pwDatabase, FileFormatProvider fmtImp,
			IOConnectionInfo[] vConnections, bool bSynchronize, IUIOperations uiOps,
			bool bForceSave, Form fParent)
		{
			if(pwDatabase == null) throw new ArgumentNullException("pwDatabase");
			if(!pwDatabase.IsOpen) { Debug.Assert(false); return null; }
			if(fmtImp == null) throw new ArgumentNullException("fmtImp");
			if(vConnections == null) throw new ArgumentNullException("vConnections");

			if(!AppPolicy.Try(AppPolicyId.Import)) return null;
			if(!fmtImp.TryBeginImport()) return null;

			MainForm mf = Program.MainForm; // Null for KPScript
			bool bUseTempDb = (fmtImp.SupportsUuids || fmtImp.RequiresKey);
			bool bAllSuccess = true;

			IStatusLogger dlgStatus;
			if(Program.Config.UI.ShowImportStatusDialog ||
				((mf != null) && !mf.HasFormLoaded))
				dlgStatus = new OnDemandStatusDialog(false, fParent);
			else dlgStatus = new UIBlockerStatusLogger(fParent);

			dlgStatus.StartLogging(PwDefs.ShortProductName + " - " + (bSynchronize ?
				KPRes.Synchronizing : KPRes.ImportingStatusMsg), false);
			dlgStatus.SetText(bSynchronize ? KPRes.Synchronizing :
				KPRes.ImportingStatusMsg, LogStatusType.Info);

			if(vConnections.Length == 0)
			{
				try
				{
					pwDatabase.Modified = true;
					fmtImp.Import(pwDatabase, null, dlgStatus);
				}
				catch(Exception ex)
				{
					MessageService.ShowWarning(ex);
					bAllSuccess = false;
				}

				dlgStatus.EndLogging();
				return bAllSuccess;
			}

			foreach(IOConnectionInfo iocIn in vConnections)
			{
				Stream s = null;
				try { s = IOConnection.OpenRead(iocIn); }
				catch(Exception ex)
				{
					MessageService.ShowWarning(iocIn.GetDisplayName(), ex);
					bAllSuccess = false;
					continue;
				}
				if(s == null) { Debug.Assert(false); bAllSuccess = false; continue; }

				PwDatabase pwImp;
				if(bUseTempDb)
				{
					pwImp = new PwDatabase();
					pwImp.New(new IOConnectionInfo(), pwDatabase.MasterKey);
					pwImp.MemoryProtection = pwDatabase.MemoryProtection.CloneDeep();
				}
				else pwImp = pwDatabase;

				if(fmtImp.RequiresKey && !bSynchronize)
				{
					KeyPromptFormResult r;
					DialogResult dr = KeyPromptForm.ShowDialog(iocIn, false, null, out r);
					if((dr != DialogResult.OK) || (r == null))
					{
						s.Close();
						bAllSuccess = false;
						continue;
					}

					pwImp.MasterKey = r.CompositeKey;
				}
				else if(bSynchronize) pwImp.MasterKey = pwDatabase.MasterKey;

				dlgStatus.SetText((bSynchronize ? KPRes.Synchronizing :
					KPRes.ImportingStatusMsg) + " (" + iocIn.GetDisplayName() +
					")", LogStatusType.Info);

				try
				{
					pwImp.Modified = true;
					fmtImp.Import(pwImp, s, dlgStatus);
				}
				catch(Exception ex)
				{
					string strMsg = ex.Message;
					if(bSynchronize && (ex is InvalidCompositeKeyException))
						strMsg = KLRes.InvalidCompositeKey + MessageService.NewParagraph +
							KPRes.SynchronizingHint;
					MessageService.ShowWarning(iocIn.GetDisplayName(),
						KPRes.FileImportFailed, strMsg);

					bAllSuccess = false;
					continue;
				}
				finally { s.Close(); }

				if(bUseTempDb)
				{
					PwMergeMethod mm;
					if(!fmtImp.SupportsUuids) mm = PwMergeMethod.CreateNewUuids;
					else if(bSynchronize) mm = PwMergeMethod.Synchronize;
					else
					{
						ImportMethodForm imf = new ImportMethodForm();
						if(UIUtil.ShowDialogNotValue(imf, DialogResult.OK))
						{
							bAllSuccess = false;
							continue;
						}
						mm = imf.MergeMethod;
						UIUtil.DestroyForm(imf);
					}

					try
					{
						pwDatabase.Modified = true;
						pwDatabase.MergeIn(pwImp, mm, dlgStatus);
					}
					catch(Exception ex)
					{
						MessageService.ShowWarning(iocIn.GetDisplayName(),
							KPRes.ImportFailed, ex);
						bAllSuccess = false;
						continue;
					}
				}
			}

			if(bSynchronize && bAllSuccess)
			{
				if(uiOps == null) { Debug.Assert(false); throw new ArgumentNullException("uiOps"); }

				dlgStatus.SetText(KPRes.Synchronizing + " (" +
					KPRes.SavingDatabase + ")", LogStatusType.Info);

				if(mf != null)
				{
					try { mf.DocumentManager.ActiveDatabase = pwDatabase; }
					catch(Exception) { Debug.Assert(false); }
				}

				if(uiOps.UIFileSave(bForceSave))
				{
					foreach(IOConnectionInfo ioc in vConnections)
					{
						try
						{
							// dlgStatus.SetText(KPRes.Synchronizing + " (" +
							//	KPRes.SavingDatabase + " " + ioc.GetDisplayName() +
							//	")", LogStatusType.Info);

							string strSource = pwDatabase.IOConnectionInfo.Path;
							if(!string.Equals(ioc.Path, strSource, StrUtil.CaseIgnoreCmp))
							{
								bool bSaveAs = true;

								if(pwDatabase.IOConnectionInfo.IsLocalFile() &&
									ioc.IsLocalFile())
								{
									// Do not try to copy an encrypted file;
									// https://sourceforge.net/p/keepass/discussion/329220/thread/9c9eb989/
									// https://msdn.microsoft.com/en-us/library/windows/desktop/aa363851.aspx
									if((long)(File.GetAttributes(strSource) &
										FileAttributes.Encrypted) == 0)
									{
										File.Copy(strSource, ioc.Path, true);
										bSaveAs = false;
									}
								}

								if(bSaveAs) pwDatabase.SaveAs(ioc, false, null);
							}
							// else { } // No assert (sync on save)

							if(mf != null)
								mf.FileMruList.AddItem(ioc.GetDisplayName(),
									ioc.CloneDeep());
						}
						catch(Exception ex)
						{
							MessageService.ShowWarning(
								pwDatabase.IOConnectionInfo.GetDisplayName() +
								MessageService.NewLine + ioc.GetDisplayName(),
								KPRes.SyncFailed, ex);
							bAllSuccess = false;
							continue;
						}
					}
				}
				else
				{
					MessageService.ShowWarning(
						pwDatabase.IOConnectionInfo.GetDisplayName(), KPRes.SyncFailed);
					bAllSuccess = false;
				}
			}

			dlgStatus.EndLogging();
			return bAllSuccess;
		}

		public static bool? Import(PwDatabase pd, FileFormatProvider fmtImp,
			IOConnectionInfo iocImp, PwMergeMethod mm, CompositeKey cmpKey)
		{
			if(pd == null) throw new ArgumentNullException("pd");
			if(fmtImp == null) throw new ArgumentNullException("fmtImp");
			if(iocImp == null) throw new ArgumentNullException("iocImp");
			if(cmpKey == null) cmpKey = new CompositeKey();

			if(!AppPolicy.Try(AppPolicyId.Import)) return null;
			if(!fmtImp.TryBeginImport()) return null;

			PwDatabase pdImp = new PwDatabase();
			pdImp.New(new IOConnectionInfo(), cmpKey);
			pdImp.MemoryProtection = pd.MemoryProtection.CloneDeep();

			Stream s = IOConnection.OpenRead(iocImp);
			if(s == null)
				throw new FileNotFoundException(iocImp.GetDisplayName() +
					MessageService.NewParagraph + KPRes.FileNotFoundError);

			try { fmtImp.Import(pdImp, s, null); }
			finally { s.Close(); }

			pd.Modified = true;
			pd.MergeIn(pdImp, mm);
			return true;
		}

		public static bool? Synchronize(PwDatabase pwStorage, IUIOperations uiOps,
			bool bOpenFromUrl, Form fParent)
		{
			if(pwStorage == null) throw new ArgumentNullException("pwStorage");
			if(!pwStorage.IsOpen) { Debug.Assert(false); return null; }
			if(!AppPolicy.Try(AppPolicyId.Import)) return null;

			List<IOConnectionInfo> lConnections = new List<IOConnectionInfo>();
			if(!bOpenFromUrl)
			{
				OpenFileDialogEx ofd = UIUtil.CreateOpenFileDialog(KPRes.Synchronize,
					UIUtil.CreateFileTypeFilter(AppDefs.FileExtension.FileExt,
					KPRes.KdbxFiles, true), 1, null, true,
					AppDefs.FileDialogContext.Sync);

				if(ofd.ShowDialog() != DialogResult.OK) return null;

				foreach(string strSelFile in ofd.FileNames)
					lConnections.Add(IOConnectionInfo.FromPath(strSelFile));
			}
			else // Open URL
			{
				IOConnectionForm iocf = new IOConnectionForm();
				iocf.InitEx(false, null, true, true);

				if(UIUtil.ShowDialogNotValue(iocf, DialogResult.OK)) return null;

				lConnections.Add(iocf.IOConnectionInfo);
				UIUtil.DestroyForm(iocf);
			}

			return Import(pwStorage, new KeePassKdb2x(), lConnections.ToArray(),
				true, uiOps, false, fParent);
		}

		public static bool? Synchronize(PwDatabase pwStorage, IUIOperations uiOps,
			IOConnectionInfo iocSyncWith, bool bForceSave, Form fParent)
		{
			if(pwStorage == null) throw new ArgumentNullException("pwStorage");
			if(!pwStorage.IsOpen) { Debug.Assert(false); return null; }
			if(iocSyncWith == null) throw new ArgumentNullException("iocSyncWith");
			if(!AppPolicy.Try(AppPolicyId.Import)) return null;

			Program.TriggerSystem.RaiseEvent(EcasEventIDs.SynchronizingDatabaseFile,
				EcasProperty.Database, pwStorage);

			IOConnectionInfo[] vConnections = new IOConnectionInfo[1] { iocSyncWith };

			bool? ob = Import(pwStorage, new KeePassKdb2x(), vConnections,
				true, uiOps, bForceSave, fParent);

			// Always raise the post event, such that the event pair can
			// for instance be used to turn off/on other triggers
			Program.TriggerSystem.RaiseEvent(EcasEventIDs.SynchronizedDatabaseFile,
				EcasProperty.Database, pwStorage);

			return ob;
		}

		public static int CountQuotes(string str, int posMax)
		{
			int i = 0, n = 0;

			while(true)
			{
				i = str.IndexOf('\"', i);
				if(i < 0) return n;

				++i;
				if(i > posMax) return n;

				++n;
			}
		}

		public static List<string> SplitCsvLine(string strLine, string strDelimiter)
		{
			List<string> list = new List<string>();

			int nOffset = 0;
			while(true)
			{
				int i = strLine.IndexOf(strDelimiter, nOffset);
				if(i < 0) break;

				int nQuotes = CountQuotes(strLine, i);
				if((nQuotes & 1) == 0)
				{
					list.Add(strLine.Substring(0, i));
					strLine = strLine.Remove(0, i + strDelimiter.Length);
					nOffset = 0;
				}
				else
				{
					nOffset = i + strDelimiter.Length;
					if(nOffset >= strLine.Length) break;
				}
			}

			list.Add(strLine);
			return list;
		}

		public static bool SetStatus(IStatusLogger slLogger, uint uPercent)
		{
			if(slLogger != null) return slLogger.SetProgress(uPercent);
			return true;
		}

		private static readonly string[] m_vTitles = {
			"title", "system", "account", "entry",
			"item", "itemname", "item name", "subject",
			"service", "servicename", "service name",
			"head", "heading", "card", "product", "provider", "bank",
			"type",

			// Non-English names
			"seite"
		};

		private static readonly string[] m_vTitlesSubstr = {
			"title", "system", "account", "entry",
			"item", "subject", "service", "head"
		};

		private static readonly string[] m_vUserNames = {
			"user", "name", "username", "user name", "login name",
			"login", "form_loginname", "wpname", "mail",
			"email", "e-mail", "id", "userid", "user id",
			"loginid", "login id", "log", "uin",
			"first name", "last name", "card#", "account #",
			"member", "member #", "owner",

			// Non-English names
			"nom", "benutzername"
		};

		private static readonly string[] m_vUserNamesSubstr = {
			"user", "name", "login", "mail", "owner"
		};

		private static readonly string[] m_vPasswords = {
			"password", "pass word", "passphrase", "pass phrase",
			"pass", "code", "code word", "codeword",
			"secret", "secret word",
			"key", "keyword", "key word", "keyphrase", "key phrase",
			"form_pw", "wppassword", "pin", "pwd", "pw", "pword",
			"p", "serial", "serial#", "license key", "reg #",

			// Non-English names
			"passwort", "kennwort"
		};

		private static readonly string[] m_vPasswordsSubstr = {
			"pass", "code",	"secret", "key", "pw", "pin"
		};

		private static readonly string[] m_vUrls = {
			"url", "hyper link", "hyperlink", "link",
			"host", "hostname", "host name", "server", "address",
			"hyper ref", "href", "web", "website", "web site", "site",
			"web-site",

			// Non-English names
			"ort", "adresse", "webseite"
		};

		private static readonly string[] m_vUrlsSubstr = {
			"url", "link", "host", "address", "hyper ref", "href",
			"web", "site"
		};

		private static readonly string[] m_vNotes = {
			"note", "notes", "comment", "comments", "memo",
			"description", "free form", "freeform",
			"free text", "freetext", "free",

			// Non-English names
			"kommentar", "hinweis"
		};

		private static readonly string[] m_vNotesSubstr = { 
			"note", "comment", "memo", "description", "free"
		};

		public static string MapNameToStandardField(string strName, bool bAllowFuzzy)
		{
			if(strName == null) { Debug.Assert(false); return string.Empty; }

			string strFind = strName.Trim().ToLower();

			if(Array.IndexOf<string>(m_vTitles, strFind) >= 0)
				return PwDefs.TitleField;
			if(Array.IndexOf<string>(m_vUserNames, strFind) >= 0)
				return PwDefs.UserNameField;
			if(Array.IndexOf<string>(m_vPasswords, strFind) >= 0)
				return PwDefs.PasswordField;
			if(Array.IndexOf<string>(m_vUrls, strFind) >= 0)
				return PwDefs.UrlField;
			if(Array.IndexOf<string>(m_vNotes, strFind) >= 0)
				return PwDefs.NotesField;

			if(strFind.Equals(KPRes.Title, StrUtil.CaseIgnoreCmp))
				return PwDefs.TitleField;
			if(strFind.Equals(KPRes.UserName, StrUtil.CaseIgnoreCmp))
				return PwDefs.UserNameField;
			if(strFind.Equals(KPRes.Password, StrUtil.CaseIgnoreCmp))
				return PwDefs.PasswordField;
			if(strFind.Equals(KPRes.Url, StrUtil.CaseIgnoreCmp))
				return PwDefs.UrlField;
			if(strFind.Equals(KPRes.Notes, StrUtil.CaseIgnoreCmp))
				return PwDefs.NotesField;

			if(!bAllowFuzzy) return string.Empty;

			// Check for passwords first, then user names ("vb_login_password")
			foreach(string strSub in m_vPasswordsSubstr)
			{
				if(strFind.Contains(strSub)) return PwDefs.PasswordField;
			}
			foreach(string strSub in m_vUserNamesSubstr)
			{
				if(strFind.Contains(strSub)) return PwDefs.UserNameField;
			}
			foreach(string strSub in m_vUrlsSubstr)
			{
				if(strFind.Contains(strSub)) return PwDefs.UrlField;
			}
			foreach(string strSub in m_vNotesSubstr)
			{
				if(strFind.Contains(strSub)) return PwDefs.NotesField;
			}
			foreach(string strSub in m_vTitlesSubstr)
			{
				if(strFind.Contains(strSub)) return PwDefs.TitleField;
			}

			return string.Empty;
		}

		public static void AppendToField(PwEntry pe, string strName, string strValue,
			PwDatabase pdContext)
		{
			AppendToField(pe, strName, strValue, pdContext, null, false);
		}

		public static void AppendToField(PwEntry pe, string strName, string strValue,
			PwDatabase pdContext, string strSeparator, bool bOnlyIfNotDup)
		{
			if(pe == null) { Debug.Assert(false); return; }
			if(string.IsNullOrEmpty(strName)) { Debug.Assert(false); return; }

			if(strValue == null) { Debug.Assert(false); strValue = string.Empty; }

			if(strSeparator == null)
			{
				if(PwDefs.IsStandardField(strName) && (strName != PwDefs.NotesField))
					strSeparator = ", ";
				else strSeparator = MessageService.NewLine;
			}

			ProtectedString psPrev = pe.Strings.Get(strName);
			if((psPrev == null) || psPrev.IsEmpty)
			{
				MemoryProtectionConfig mpc = ((pdContext != null) ?
					pdContext.MemoryProtection : new MemoryProtectionConfig());
				bool bProtect = mpc.GetProtection(strName);

				pe.Strings.Set(strName, new ProtectedString(bProtect, strValue));
			}
			else if(strValue.Length != 0)
			{
				bool bAppend = true;
				if(bOnlyIfNotDup)
				{
					ProtectedString psValue = new ProtectedString(false, strValue);
					bAppend = !psPrev.Equals(psValue, false);
				}

				if(bAppend)
					pe.Strings.Set(strName, psPrev + (strSeparator + strValue));
			}
		}

		internal static void CreateFieldWithIndex(ProtectedStringDictionary d,
			string strName, string strValue, PwDatabase pdContext, bool bAllowEmptyValue)
		{
			if(string.IsNullOrEmpty(strName)) { Debug.Assert(false); return; }

			MemoryProtectionConfig mpc = ((pdContext != null) ?
				pdContext.MemoryProtection : new MemoryProtectionConfig());
			bool bProtect = mpc.GetProtection(strName);

			CreateFieldWithIndex(d, strName, strValue, bProtect, bAllowEmptyValue);
		}

		internal static void CreateFieldWithIndex(ProtectedStringDictionary d,
			string strName, string strValue, bool bProtect, bool bAllowEmptyValue)
		{
			if(d == null) { Debug.Assert(false); return; }
			if(string.IsNullOrEmpty(strName)) { Debug.Assert(false); return; }
			if(strValue == null) { Debug.Assert(false); return; }
			if((strValue.Length == 0) && !bAllowEmptyValue) return;

			ProtectedString psValue = new ProtectedString(bProtect, strValue);

			ProtectedString psEx = d.Get(strName);
			if((psEx == null) || (PwDefs.IsStandardField(strName) && psEx.IsEmpty))
			{
				d.Set(strName, psValue);
				return;
			}

			NumberFormatInfo nfi = NumberFormatInfo.InvariantInfo;
			for(int i = 2; i < int.MaxValue; ++i)
			{
				string strNameI = strName + " (" + i.ToString(nfi) + ")";
				if(!d.Exists(strNameI))
				{
					d.Set(strNameI, psValue);
					break;
				}
			}
		}

		public static bool EntryEquals(PwEntry pe1, PwEntry pe2)
		{
			if(pe1.ParentGroup == null) return false;
			if(pe2.ParentGroup == null) return false;
			if(pe1.ParentGroup.Name != pe2.ParentGroup.Name)
				return false;

			return pe1.Strings.EqualsDictionary(pe2.Strings,
				PwCompareOptions.NullEmptyEquivStd, MemProtCmpMode.None);
		}

		internal static string GuiSendRetrieve(string strSendPrefix)
		{
			if(strSendPrefix.Length > 0)
				GuiSendKeysPrc(strSendPrefix);

			return GuiRetrieveDataField();
		}

		private static string GuiRetrieveDataField()
		{
			ClipboardUtil.Clear();
			Application.DoEvents();

			GuiSendKeysPrc(@"^c");

			try
			{
				if(ClipboardUtil.ContainsText())
					return (ClipboardUtil.GetText() ?? string.Empty);
			}
			catch(Exception) { Debug.Assert(false); } // Opened by other process

			return string.Empty;
		}

		internal static void GuiSendKeysPrc(string strSend)
		{
			if(strSend.Length > 0)
				SendInputEx.SendKeysWait(strSend, false);

			Application.DoEvents();
			Thread.Sleep(100);
			Application.DoEvents();
		}
		
		internal static void GuiSendWaitWindowChange(string strSend)
		{
			IntPtr ptrCur = NativeMethods.GetForegroundWindowHandle();

			ImportUtil.GuiSendKeysPrc(strSend);

			int nRound = 0;
			while(true)
			{
				Application.DoEvents();

				IntPtr ptr = NativeMethods.GetForegroundWindowHandle();
				if(ptr != ptrCur) break;

				++nRound;
				if(nRound > 1000)
					throw new InvalidOperationException();

				Thread.Sleep(50);
			}

			Thread.Sleep(100);
			Application.DoEvents();
		}

		internal static string FixUrl(string strUrl)
		{
			strUrl = (strUrl ?? string.Empty).Trim();

			if((strUrl.Length > 0) && (strUrl.IndexOf('.') >= 0) &&
				(strUrl.IndexOf(':') < 0) && (strUrl.IndexOf('@') < 0))
			{
				string strNew = ("https://" + strUrl.ToLower());
				if(strUrl.IndexOf('/') < 0) strNew += "/";
				return strNew;
			}

			return strUrl;
		}
	}
}
