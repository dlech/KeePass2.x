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
using System.Net;
using System.Net.Cache;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.Threading;

using KeePass.Resources;

using KeePassLib;
using KeePassLib.Utility;

namespace KeePass.Util
{
	public static class CheckForUpdate
	{
		private const string ElemRoot = "KeePass";

		private const string ElemVersionU = "Version32";
		private const string ElemVersionStr = "VersionDisplayString";

		private static string m_strVersionURL = string.Empty;
		private static ToolStripStatusLabel m_tsResultsViewer = null;

		public static void StartAsync(string strVersionURL, ToolStripStatusLabel tsResultsViewer)
		{
			m_strVersionURL = strVersionURL;
			m_tsResultsViewer = tsResultsViewer;

			Thread th = new Thread(new ThreadStart(CheckForUpdate.OnStartCheck));
			th.Start();
		}

		private static void OnStartCheck()
		{
			WebClient webClient = new WebClient();
			webClient.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);

			Uri uri = new Uri(m_strVersionURL);

			webClient.DownloadDataCompleted +=
				new DownloadDataCompletedEventHandler(OnDownloadCompleted);

			try { webClient.DownloadDataAsync(uri); }
			catch(Exception) { }
		}

		private static void OnDownloadCompleted(object sender, DownloadDataCompletedEventArgs e)
		{
			string strXmlFile = NetUtil.GZipUtf8ResultToString(e);

			if(strXmlFile == null)
			{
				if(e.Error != null)
				{
					if(m_tsResultsViewer == null)
						MessageService.ShowWarning(KPRes.DownloadFailed, e);
					else if(e.Error.Message != null)
						m_tsResultsViewer.Text = KPRes.DownloadFailed + " " +
							e.Error.Message;
				}
				else
				{
					if(m_tsResultsViewer == null)
						MessageService.ShowWarning(KPRes.DownloadFailed);
					else
						m_tsResultsViewer.Text = KPRes.DownloadFailed;
				}

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
						MessageService.NewParagraph + KPRes.HomepageVisitQuestion))
					{
						WinUtil.OpenUrlInNewBrowser(PwDefs.HomepageUrl, null);
					}
				}
				else m_tsResultsViewer.Text = KPRes.ChkForUpdNewVersion;
			}
			else if(uVersion == PwDefs.Version32)
			{
				if(m_tsResultsViewer == null)
					MessageService.ShowInfo(KPRes.ChkForUpdGotLatest);
				else
					m_tsResultsViewer.Text = KPRes.ChkForUpdGotLatest;
			}
			else
			{
				if(m_tsResultsViewer == null)
					MessageService.ShowWarning(KPRes.UnknownFileVersion);
				else
					m_tsResultsViewer.Text = KPRes.UnknownFileVersion;
			}
		}

		private static void StructureFail()
		{
			Debug.Assert(false);

			if(m_tsResultsViewer == null)
				MessageService.ShowWarning(KPRes.InvalidFileStructure);
			else
				m_tsResultsViewer.Text = KPRes.ChkForUpdGotLatest + " " +
					KPRes.InvalidFileStructure;
		}
	}
}
