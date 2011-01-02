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
using System.Net;
using System.Net.Cache;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.Threading;

// using KeePass.Native;
using KeePass.Resources;

using KeePassLib;
using KeePassLib.Resources;
using KeePassLib.Utility;

namespace KeePass.Util
{
	public static class CheckForUpdate
	{
		private const string ElemRoot = "KeePass";

		private const string ElemVersionU = "Version32";
		private const string ElemVersionStr = "VersionDisplayString";

		private static volatile string m_strVersionURL = string.Empty;
		private static volatile ToolStripStatusLabel m_tsResultsViewer = null;

		public static void StartAsync(string strVersionUrl, ToolStripStatusLabel tsResultsViewer)
		{
			m_strVersionURL = strVersionUrl;
			m_tsResultsViewer = tsResultsViewer;

			// Local, but thread will continue to run anyway
			Thread th = new Thread(new ThreadStart(CheckForUpdate.OnStartCheck));
			th.Start();
		}

		private static void OnStartCheck()
		{
			// NativeProgressDialog dlg = null;
			// if(m_tsResultsViewer == null)
			// {
			//	dlg = new NativeProgressDialog();
			//	dlg.StartLogging(KPRes.Wait + "...", false);
			// }

			try
			{
				WebClient webClient = new WebClient();
				webClient.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);

				Uri uri = new Uri(m_strVersionURL);

				webClient.DownloadDataCompleted +=
					new DownloadDataCompletedEventHandler(OnDownloadCompleted);

				webClient.DownloadDataAsync(uri);
			}
			catch(NotImplementedException)
			{
				ReportStatusEx(KLRes.FrameworkNotImplExcp, true);
			}
			catch(Exception) { }

			// if(dlg != null) dlg.EndLogging();
		}

		private static void OnDownloadCompleted(object sender, DownloadDataCompletedEventArgs e)
		{
			string strXmlFile = NetUtil.GZipUtf8ResultToString(e);

			if(strXmlFile == null)
			{
				if(e.Error != null)
				{
					if(m_tsResultsViewer == null)
						MessageService.ShowWarning(KPRes.UpdateCheckingFailed, e.Error);
					else if(e.Error.Message != null)
						CheckForUpdate.SetTsStatus(KPRes.UpdateCheckingFailed + " " +
							e.Error.Message);
				}
				else ReportStatusEx(KPRes.UpdateCheckingFailed, true);

				return;
			}

			XmlDocument xmlDoc = new XmlDocument();

			try { xmlDoc.LoadXml(strXmlFile); }
			catch(Exception)
			{
				StructureFail();
				return;
			}

			XmlElement xmlRoot = xmlDoc.DocumentElement;
			if(xmlRoot == null) { StructureFail(); return; }
			if(xmlRoot.Name != ElemRoot) { StructureFail(); return; }

			uint uVersion = 0;

			foreach(XmlNode xmlChild in xmlRoot.ChildNodes)
			{
				if(xmlChild.Name == ElemVersionU)
					uint.TryParse(xmlChild.InnerText, out uVersion);
				else if(xmlChild.Name == ElemVersionStr)
				{
					// strVersion = xmlChild.InnerText;
				}
			}

			if(uVersion > PwDefs.Version32)
			{
				if(m_tsResultsViewer == null)
				{
					if(MessageService.AskYesNo(KPRes.ChkForUpdNewVersion +
						MessageService.NewParagraph + KPRes.WebsiteVisitQuestion))
					{
						WinUtil.OpenUrl(PwDefs.HomepageUrl, null);
					}
				}
				else CheckForUpdate.SetTsStatus(KPRes.ChkForUpdNewVersion);
			}
			else if(uVersion == PwDefs.Version32)
				ReportStatusEx(KPRes.ChkForUpdGotLatest, false);
			else
				ReportStatusEx(KPRes.UnknownFileVersion, true);
		}

		private static void ReportStatusEx(string strText, bool bIsWarning)
		{
			ReportStatusEx(strText, strText, bIsWarning);
		}

		private static void ReportStatusEx(string strLongText, string strShortText,
			bool bIsWarning)
		{
			if(m_tsResultsViewer == null)
			{
				if(bIsWarning) MessageService.ShowWarning(strLongText);
				else MessageService.ShowInfo(strLongText);
			}
			else CheckForUpdate.SetTsStatus(strShortText);
		}

		private static void StructureFail()
		{
			Debug.Assert(false);

			if(m_tsResultsViewer == null)
				MessageService.ShowWarning(KPRes.InvalidFileStructure);
			else
				CheckForUpdate.SetTsStatus(KPRes.ChkForUpdGotLatest + " " +
					KPRes.InvalidFileStructure);
		}

		private static void SetTsStatus(string strText)
		{
			if(strText == null) { Debug.Assert(false); return; }
			if(m_tsResultsViewer == null) { Debug.Assert(false); return; }

			try
			{
				ToolStrip pParent = m_tsResultsViewer.Owner;
				if((pParent != null) && pParent.InvokeRequired)
				{
					pParent.Invoke(new Priv_CfuSsd(CheckForUpdate.SetTsStatusDirect),
						new object[] { strText });
				}
				else CheckForUpdate.SetTsStatusDirect(strText);
			}
			catch(Exception) { Debug.Assert(false); }
		}

		public delegate void Priv_CfuSsd(string strText);

		private static void SetTsStatusDirect(string strText)
		{
			try { m_tsResultsViewer.Text = strText; }
			catch(Exception) { Debug.Assert(false); }
		}
	}
}
