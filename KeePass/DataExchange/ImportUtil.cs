/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2007 Dominik Reichl <dominik.reichl@t-online.de>

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

using KeePass.App;
using KeePass.DataExchange.Formats;
using KeePass.Forms;
using KeePass.Resources;
using KeePass.UI;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Serialization;
using KeePassLib.Utility;

namespace KeePass.DataExchange
{
	public static class ImportUtil
	{
		public static void ImportInto(PwDatabase pwStorage, out bool bAppendedToRootOnly)
		{
			bAppendedToRootOnly = false;

			if(pwStorage == null) throw new ArgumentNullException("pwStorage");
			if(!pwStorage.IsOpen) return;
			if(!AppPolicy.Try(AppPolicyFlag.Import)) return;

			ImportDataForm dlgFmt = new ImportDataForm();
			dlgFmt.InitEx(pwStorage);
			if(dlgFmt.ShowDialog() == DialogResult.OK)
			{
				Debug.Assert(dlgFmt.ResultFormat != null);
				if(dlgFmt.ResultFormat == null)
				{
					MessageService.ShowWarning(KPRes.ImportFailed);
					return;
				}

				bAppendedToRootOnly = dlgFmt.ResultFormat.AppendsToRootGroupOnly;

				PerformImport(pwStorage, dlgFmt.ResultFormat,
					dlgFmt.ResultFiles, false, null);
			}
		}

		public static bool Synchronize(PwDatabase pwStorage, IUIOperations uiOps)
		{
			if(pwStorage == null) throw new ArgumentNullException("pwStorage");
			if(!pwStorage.IsOpen) return false;
			if(!AppPolicy.Try(AppPolicyFlag.Import)) return false;

			OpenFileDialog ofd = new OpenFileDialog();
			ofd.AddExtension = false;
			ofd.CheckFileExists = true;
			ofd.CheckPathExists = true;
			ofd.Filter = KPRes.AllFiles + @" (*.*)|*.*";
			ofd.Multiselect = false;
			ofd.RestoreDirectory = true;
			ofd.Title = KPRes.Synchronize;
			ofd.ValidateNames = true;

			if(ofd.ShowDialog() != DialogResult.OK) return true;

			return PerformImport(pwStorage, new KeePassKdb2x(), new string[]{
				ofd.FileName }, true, uiOps);
		}

		private static bool PerformImport(PwDatabase pwDatabase, FormatImporter fmtImp,
			string[] vFiles, bool bSynchronize, IUIOperations uiOps)
		{
			if(fmtImp.TryBeginImport() == false) return false;

			bool bUseTempDb = (fmtImp.SupportsUuids || fmtImp.RequiresKey);
			bool bAllSuccess = true;

			if(bSynchronize) { Debug.Assert(vFiles.Length == 1); }

			if(vFiles.Length == 0)
			{
				try { fmtImp.Import(pwDatabase, null, null); }
				catch(Exception exSingular)
				{
					if((exSingular.Message != null) && (exSingular.Message.Length > 0))
						MessageService.ShowWarning(exSingular);
				}

				return true;
			}

			foreach(string strFile in vFiles)
			{
				FileStream fs = null;

				try
				{
					fs = new FileStream(strFile, FileMode.Open, FileAccess.Read,
						FileShare.Read);
				}
				catch(Exception exFile)
				{
					MessageService.ShowWarning(strFile, exFile);
					bAllSuccess = false;
					continue;
				}

				if(fs == null) { Debug.Assert(false); bAllSuccess = false; continue; }

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
					kpf.InitEx(strFile, false);

					if(kpf.ShowDialog() != DialogResult.OK) { fs.Close(); continue; }

					pwImp.MasterKey = kpf.CompositeKey;
				}
				else if(bSynchronize) pwImp.MasterKey = pwDatabase.MasterKey;

				StatusLoggerForm slf = new StatusLoggerForm();
				slf.InitEx(false);
				slf.Show();

				if(bSynchronize) slf.StartLogging(KPRes.Synchronize);
				else slf.StartLogging(KPRes.ImportingStatusMsg);

				try { fmtImp.Import(pwImp, fs, slf); }
				catch(Exception excpFmt)
				{
					string strMsgEx = excpFmt.Message;
					if(bSynchronize)
					{
						strMsgEx += MessageService.NewParagraph +
							KPRes.SynchronizingHint;
					}

					MessageService.ShowWarning(strMsgEx);

					fs.Close(); slf.EndLogging(); slf.Close();
					bAllSuccess = false;
					continue;
				}

				fs.Close();

				if(bUseTempDb)
				{
					PwMergeMethod mm;
					if(!fmtImp.SupportsUuids) mm = PwMergeMethod.CreateNewUuids;
					else if(bSynchronize) mm = PwMergeMethod.Synchronize;
					else
					{
						ImportMethodForm imf = new ImportMethodForm();
						if(imf.ShowDialog() != DialogResult.OK)
						{
							slf.EndLogging(); slf.Close();
							continue;
						}
						mm = imf.MergeMethod;
					}

					slf.SetText(KPRes.MergingData, LogStatusType.Info);

					try { pwDatabase.MergeIn(pwImp, mm); }
					catch(Exception exMerge)
					{
						MessageService.ShowWarning(strFile, KPRes.ImportFailed,
							exMerge);

						bAllSuccess = false;
						slf.EndLogging(); slf.Close();
						continue;
					}
				}

				slf.EndLogging(); slf.Close();

				if(bSynchronize)
				{
					Debug.Assert(uiOps != null);
					if(uiOps == null) throw new ArgumentNullException();

					if(!uiOps.UIFileSave())
					{
						bAllSuccess = false;
						continue;
					}

					try
					{
						if(pwDatabase.IOConnectionInfo.IsLocalFile())
						{
							File.Copy(pwDatabase.IOConnectionInfo.Url,
								strFile, true);
						}
						else
							pwDatabase.SaveAs(IOConnectionInfo.FromPath(strFile), false, null);
					}
					catch(Exception exSync)
					{
						MessageService.ShowWarning(KPRes.SyncFailed,
							pwDatabase.IOConnectionInfo.GetDisplayName() +
							MessageService.NewLine + strFile, exSync);

						bAllSuccess = false;
						continue;
					}
				}
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
	}
}
