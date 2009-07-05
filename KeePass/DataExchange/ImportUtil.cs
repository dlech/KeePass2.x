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
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml;
using System.Threading;

using KeePass.App;
using KeePass.DataExchange.Formats;
using KeePass.Forms;
using KeePass.Native;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Serialization;
using KeePassLib.Utility;

namespace KeePass.DataExchange
{
	public static class ImportUtil
	{
		public static bool ImportInto(PwDatabase pwStorage, out bool bAppendedToRootOnly)
		{
			bAppendedToRootOnly = false;

			if(pwStorage == null) throw new ArgumentNullException("pwStorage");
			if(!pwStorage.IsOpen) return false;
			if(!AppPolicy.Try(AppPolicyId.Import)) return false;

			ExchangeDataForm dlgFmt = new ExchangeDataForm();
			dlgFmt.InitEx(false, pwStorage, pwStorage.RootGroup);

			if(dlgFmt.ShowDialog() == DialogResult.OK)
			{
				Debug.Assert(dlgFmt.ResultFormat != null);
				if(dlgFmt.ResultFormat == null)
				{
					MessageService.ShowWarning(KPRes.ImportFailed);
					return false;
				}

				bAppendedToRootOnly = dlgFmt.ResultFormat.ImportAppendsToRootGroupOnly;

				List<IOConnectionInfo> lConnections = new List<IOConnectionInfo>();
				foreach(string strSelFile in dlgFmt.ResultFiles)
					lConnections.Add(IOConnectionInfo.FromPath(strSelFile));

				PerformImport(pwStorage, dlgFmt.ResultFormat, lConnections.ToArray(),
					false, null, false);
			}
			else return false;

			return true;
		}

		public static bool? Synchronize(PwDatabase pwStorage, IUIOperations uiOps,
			bool bOpenFromUrl)
		{
			if(pwStorage == null) throw new ArgumentNullException("pwStorage");
			if(!pwStorage.IsOpen) return null;
			if(!AppPolicy.Try(AppPolicyId.Import)) return null;

			List<IOConnectionInfo> vConnections = new List<IOConnectionInfo>();
			if(bOpenFromUrl == false)
			{
				OpenFileDialog ofd = UIUtil.CreateOpenFileDialog(KPRes.Synchronize,
					UIUtil.CreateFileTypeFilter(null, null, true), 1, null, true, true);

				if(ofd.ShowDialog() != DialogResult.OK) return null;

				foreach(string strSelFile in ofd.FileNames)
					vConnections.Add(IOConnectionInfo.FromPath(strSelFile));
			}
			else // Open URL
			{
				IOConnectionForm iocf = new IOConnectionForm();
				iocf.InitEx(false, new IOConnectionInfo(), false, true);

				if(iocf.ShowDialog() != DialogResult.OK) return null;

				vConnections.Add(iocf.IOConnectionInfo);
			}

			return PerformImport(pwStorage, new KeePassKdb2x(), vConnections.ToArray(),
				true, uiOps, false);
		}

		public static bool Synchronize(PwDatabase pwStorage, IUIOperations uiOps,
			IOConnectionInfo iocSyncWith, bool bForceSave)
		{
			if(pwStorage == null) throw new ArgumentNullException("pwStorage");
			if(!pwStorage.IsOpen) return false;
			if(iocSyncWith == null) throw new ArgumentNullException("iocSyncWith");
			if(!AppPolicy.Try(AppPolicyId.Import)) return false;

			List<IOConnectionInfo> vConnections = new List<IOConnectionInfo>();
			vConnections.Add(iocSyncWith);

			return PerformImport(pwStorage, new KeePassKdb2x(), vConnections.ToArray(),
				true, uiOps, bForceSave);
		}

		private static bool PerformImport(PwDatabase pwDatabase, FileFormatProvider fmtImp,
			IOConnectionInfo[] vConnections, bool bSynchronize, IUIOperations uiOps,
			bool bForceSave)
		{
			if(fmtImp.TryBeginImport() == false) return false;

			bool bUseTempDb = (fmtImp.SupportsUuids || fmtImp.RequiresKey);
			bool bAllSuccess = true;

			// if(bSynchronize) { Debug.Assert(vFiles.Length == 1); }

			StatusLoggerForm slf = new StatusLoggerForm();
			slf.InitEx(false);
			slf.Show();

			if(bSynchronize) slf.StartLogging(KPRes.Synchronize, false);
			else slf.StartLogging(KPRes.ImportingStatusMsg, false);

			if(vConnections.Length == 0)
			{
				try { fmtImp.Import(pwDatabase, null, slf); }
				catch(Exception exSingular)
				{
					if((exSingular.Message != null) && (exSingular.Message.Length > 0))
					{
						slf.SetText(exSingular.Message, LogStatusType.Warning);
						MessageService.ShowWarning(exSingular);
					}
				}

				slf.EndLogging(); slf.Close();
				return true;
			}

			foreach(IOConnectionInfo iocIn in vConnections)
			{
				Stream s = null;

				try { s = IOConnection.OpenRead(iocIn); }
				catch(Exception exFile)
				{
					MessageService.ShowWarning(iocIn.GetDisplayName(), exFile);
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
					KeyPromptForm kpf = new KeyPromptForm();
					kpf.InitEx(iocIn.GetDisplayName(), false);

					if(kpf.ShowDialog() != DialogResult.OK) { s.Close(); continue; }

					pwImp.MasterKey = kpf.CompositeKey;
				}
				else if(bSynchronize) pwImp.MasterKey = pwDatabase.MasterKey;

				slf.SetText((bSynchronize ? KPRes.Synchronizing : KPRes.ImportingStatusMsg) +
					" " + iocIn.GetDisplayName(), LogStatusType.Info);

				try { fmtImp.Import(pwImp, s, slf); }
				catch(Exception excpFmt)
				{
					string strMsgEx = excpFmt.Message;
					if(bSynchronize)
					{
						strMsgEx += MessageService.NewParagraph +
							KPRes.SynchronizingHint;
					}

					MessageService.ShowWarning(strMsgEx);

					s.Close();
					bAllSuccess = false;
					continue;
				}

				s.Close();

				if(bUseTempDb)
				{
					PwMergeMethod mm;
					if(!fmtImp.SupportsUuids) mm = PwMergeMethod.CreateNewUuids;
					else if(bSynchronize) mm = PwMergeMethod.Synchronize;
					else
					{
						ImportMethodForm imf = new ImportMethodForm();
						if(imf.ShowDialog() != DialogResult.OK)
							continue;
						mm = imf.MergeMethod;
					}

					slf.SetText(KPRes.MergingData, LogStatusType.Info);

					try { pwDatabase.MergeIn(pwImp, mm); }
					catch(Exception exMerge)
					{
						MessageService.ShowWarning(iocIn.GetDisplayName(),
							KPRes.ImportFailed, exMerge);

						bAllSuccess = false;
						continue;
					}
				}
			}

			slf.EndLogging(); slf.Close();

			if(bSynchronize && bAllSuccess)
			{
				Debug.Assert(uiOps != null);
				if(uiOps == null) throw new ArgumentNullException("uiOps");

				if(uiOps.UIFileSave(bForceSave))
				{
					foreach(IOConnectionInfo ioc in vConnections)
					{
						try
						{
							if(pwDatabase.IOConnectionInfo.Path != ioc.Path)
							{
								if(pwDatabase.IOConnectionInfo.IsLocalFile() &&
									ioc.IsLocalFile())
								{
									File.Copy(pwDatabase.IOConnectionInfo.Path,
										ioc.Path, true);
								}
								else pwDatabase.SaveAs(ioc, false, null);
							}
							else { Debug.Assert(false); }
						}
						catch(Exception exSync)
						{
							MessageService.ShowWarning(KPRes.SyncFailed,
								pwDatabase.IOConnectionInfo.GetDisplayName() +
								MessageService.NewLine + ioc.GetDisplayName(), exSync);

							bAllSuccess = false;
							continue;
						}
					}
				}
				else
				{
					MessageService.ShowWarning(KPRes.SyncFailed,
						pwDatabase.IOConnectionInfo.GetDisplayName());

					bAllSuccess = false;
				}
			}
			else if(bSynchronize) // Synchronized but not successfully imported
			{
				MessageService.ShowWarning(KPRes.SyncFailed,
					pwDatabase.IOConnectionInfo.GetDisplayName());
				
				bAllSuccess = false;
			}

			return bAllSuccess;
		}

		public static string SafeInnerText(XmlNode xmlNode)
		{
			Debug.Assert(xmlNode != null); if(xmlNode == null) return string.Empty;

			string strInner = xmlNode.InnerText;
			if(strInner == null) return string.Empty;

			return strInner;
		}

		public static string SafeInnerText(XmlNode xmlNode, string strNewLineCode)
		{
			if((strNewLineCode == null) || (strNewLineCode.Length == 0))
				return SafeInnerText(xmlNode);

			string strInner = SafeInnerText(xmlNode);
			return strInner.Replace(strNewLineCode, "\r\n");
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

		private static readonly string[] m_vNonStandardEntitiesTable = new string[]{
			@"&sect;", @"§", @"&acute;", @"´", @"&szlig;", @"ß",
			@"&Auml;", @"Ä", @"&Ouml;", @"Ö", @"&Uuml;", @"Ü",
			@"&auml;", @"ä", @"&ouml;", @"ö", @"&uuml;", @"ü",
			@"&micro;", @"µ", @"&euro;", @"€", @"&copy;", @"©",
			@"&reg;", @"®", @"&sup2;", @"²", @"&sup3;", @"³",
			@"&eacute;", @"é", @"&egrave;", @"è", @"&ecirc;", @"ê",
			@"&Aacute;", @"Á", @"&Agrave;", @"À", @"&Acirc;", @"Â",
			@"&aacute;", @"á", @"&agrave;", @"à", @"&acirc;", @"â"
		};

		/// <summary>
		/// Decode most entities except the standard ones (&lt;, &gt;, etc.).
		/// </summary>
		/// <param name="strToDecode">String to decode.</param>
		/// <returns>String without non-standard entities.</returns>
		public static string DecodeNonStandardEntities(string strToDecode)
		{
			string str = strToDecode;

			Debug.Assert((m_vNonStandardEntitiesTable.Length % 2) == 0); // Static

			for(int i = 0; i < m_vNonStandardEntitiesTable.Length; i += 2)
				str = str.Replace(m_vNonStandardEntitiesTable[i],
					m_vNonStandardEntitiesTable[i + 1]);

			return str;
		}

		public static bool SetStatus(IStatusLogger slLogger, uint uPercent)
		{
			if(slLogger != null) return slLogger.SetProgress(uPercent);
			else return true;
		}

		private static readonly string[] m_vTitles = {
			"title", "system", "account", "entry",
			"item", "itemname", "item name", "subject",
			"service", "servicename", "service name",
			"head", "heading"
		};

		private static readonly string[] m_vUserNames = {
			"user", "name", "user name", "username",
			"email", "e-mail", "id", "userid", "user id",
			"login", "form_loginname", "wpname"
		};

		private static readonly string[] m_vPasswords = {
			"password", "pass word", "passphrase", "pass phrase",
			"pass", "code", "code word", "codeword",
			"secret", "secret word",
			"key", "keyword", "key word", "keyphrase", "key phrase",
			"form_pw", "wppassword"
		};

		private static readonly string[] m_vUrls = {
			"url", "hyper link", "hyperlink", "link",
			"host", "address", "hyper ref", "href",
			"web", "website", "web site", "site"
		};

		private static readonly string[] m_vNotes = {
			"note", "notes", "comment", "comments", "memo",
			"description", "free form", "freeform",
			"free text", "freetext", "free"
		};

		public static string MapNameToStandardField(string strName)
		{
			Debug.Assert(strName != null);
			if(strName == null) throw new ArgumentNullException("strName");

			string strFind = strName.ToLower();

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

			return string.Empty;
		}

		public static bool EntryEquals(PwEntry pe1, PwEntry pe2)
		{
			if(pe1.ParentGroup == null) return false;
			if(pe2.ParentGroup == null) return false;

			if(pe1.ParentGroup.Name != pe2.ParentGroup.Name)
				return false;

			if(pe1.Strings.ReadSafe(PwDefs.TitleField) !=
				pe2.Strings.ReadSafe(PwDefs.TitleField))
			{
				return false;
			}

			if(pe1.Strings.ReadSafe(PwDefs.UserNameField) !=
				pe2.Strings.ReadSafe(PwDefs.UserNameField))
			{
				return false;
			}

			if(pe1.Strings.ReadSafe(PwDefs.PasswordField) !=
				pe2.Strings.ReadSafe(PwDefs.PasswordField))
			{
				return false;
			}

			if(pe1.Strings.ReadSafe(PwDefs.UrlField) !=
				pe2.Strings.ReadSafe(PwDefs.UrlField))
			{
				return false;
			}

			if(pe1.Strings.ReadSafe(PwDefs.NotesField) !=
				pe2.Strings.ReadSafe(PwDefs.NotesField))
			{
				return false;
			}

			return true;
		}

		internal static string GuiSendRetrieve(string strSendPrefix)
		{
			if(strSendPrefix.Length > 0)
				GuiSendKeysPrc(strSendPrefix);

			return GuiRetrieveDataField();
		}

		private static string GuiRetrieveDataField()
		{
			Clipboard.Clear();
			Application.DoEvents();

			GuiSendKeysPrc(@"^c");

			if(Clipboard.ContainsText())
				return Clipboard.GetText();

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
			IntPtr ptrCur = NativeMethods.GetForegroundWindow();

			ImportUtil.GuiSendKeysPrc(strSend);

			int nRound = 0;
			while(true)
			{
				Application.DoEvents();

				IntPtr ptr = NativeMethods.GetForegroundWindow();
				if(ptr != ptrCur) break;

				++nRound;
				if(nRound > 1000)
					throw new InvalidOperationException();

				Thread.Sleep(50);
			}

			Thread.Sleep(100);
			Application.DoEvents();
		}
	}
}
