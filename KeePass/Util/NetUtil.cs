/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2025 Dominik Reichl <dominik.reichl@t-online.de>

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
using System.IO;
using System.Net;
using System.Text;

using KeePassLib.Utility;

namespace KeePass.Util
{
	public static class NetUtil
	{
		/* public static string GZipUtf8ResultToString(DownloadDataCompletedEventArgs e)
		{
			if((e == null) || e.Cancelled || (e.Error != null) || (e.Result == null))
				return null;

			using(MemoryStream msGZ = new MemoryStream(e.Result, false))
			{
				using(GZipStream sGZ = new GZipStream(msGZ, CompressionMode.Decompress))
				{
					using(MemoryStream msUtf8 = new MemoryStream())
					{
						MemUtil.CopyStream(sGZ, msUtf8);
						return StrUtil.Utf8.GetString(msUtf8.ToArray());
					}
				}
			}
		} */

		public static string WebPageLogin(Uri uri, string strPostData,
			out List<KeyValuePair<string, string>> lCookies)
		{
			if(uri == null) throw new ArgumentNullException("uri");

			byte[] pbPostData = Encoding.ASCII.GetBytes(strPostData);

			HttpWebRequest hwr = (HttpWebRequest)HttpWebRequest.Create(uri);
			hwr.Method = "POST";
			hwr.ContentType = "application/x-www-form-urlencoded";
			hwr.ContentLength = pbPostData.Length;
			hwr.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.0)";

			using(Stream s = hwr.GetRequestStream())
			{
				s.Write(pbPostData, 0, pbPostData.Length);
			}

			string strResponse;
			using(WebResponse wr = hwr.GetResponse())
			{
				using(Stream s = wr.GetResponseStream())
				{
					strResponse = MemUtil.ReadString(s, StrUtil.Utf8);
				}

				lCookies = new List<KeyValuePair<string, string>>();
				WebHeaderCollection whc = wr.Headers;
				for(int i = 0; i < whc.Count; ++i)
				{
					if(whc.GetKey(i) != "Set-Cookie") continue;

					string strCookie = whc.Get(i);
					if(strCookie == null) { Debug.Assert(false); continue; }

					string[] vParts = strCookie.Split(';');
					if(vParts.Length == 0) { Debug.Assert(false); continue; }

					string[] vInfo = vParts[0].Split('=');
					if(vInfo.Length != 2) { Debug.Assert(false); continue; }

					lCookies.Add(new KeyValuePair<string, string>(vInfo[0], vInfo[1]));
				}
			}

			return strResponse;
		}

		public static string WebPageGetWithCookies(Uri uri,
			List<KeyValuePair<string, string>> lCookies, string strDomain)
		{
			if(uri == null) throw new ArgumentNullException("uri");

			HttpWebRequest hwr = (HttpWebRequest)HttpWebRequest.Create(uri);
			hwr.Method = "GET";
			hwr.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.0)";

			if((lCookies != null) && (lCookies.Count != 0))
			{
				hwr.CookieContainer = new CookieContainer();
				foreach(KeyValuePair<string, string> kvp in lCookies)
				{
					Cookie ck = new Cookie(kvp.Key, kvp.Value, "/", strDomain);
					hwr.CookieContainer.Add(ck);
				}
			}

			using(WebResponse wr = hwr.GetResponse())
			{
				using(Stream s = wr.GetResponseStream())
				{
					return MemUtil.ReadString(s, StrUtil.Utf8);
				}
			}
		}
	}
}
